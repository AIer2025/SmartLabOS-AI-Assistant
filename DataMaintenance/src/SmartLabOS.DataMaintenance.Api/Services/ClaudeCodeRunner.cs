using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using SmartLabOS.DataMaintenance.Api.Presales;

namespace SmartLabOS.DataMaintenance.Api.Services;

/// <summary>
/// 「售前方案自动生成」的后端执行器：
///   1) 根据项目需求信息生成一份指令文档（形如 01-最新设计-…(1~8).txt），落盘到项目目录；
///   2) 以无头(headless)模式调用本机 Claude Code CLI(`claude -p`)，由其在项目目录下生成
///      *.html 提案文件及 xxx-模块-URS.html 需求说明书；
///   3) 任务异步执行，前端通过 /status 轮询；运行状态/日志同时留痕到内存与数据库。
///
/// 设计要点：
///   - 指令文本通过 **标准输入** 传给 claude，彻底规避命令行引号/中文转义问题；
///   - 经由 cmd.exe 启动，使 PATH 中的 claude(.cmd 垫片) 能被解析；
///   - --permission-mode bypassPermissions 让 claude 在无人值守下自动写文件；
///   - --add-dir 指向项目根，确保可读取 references/ 与 potocol/。
/// </summary>
public sealed class ClaudeCodeRunner
{
    private readonly PresalesPaths _paths;
    private readonly PresalesRepository _repo;
    private readonly ILogger<ClaudeCodeRunner> _log;

    // 项目ID -> 实时任务状态（仅最近一次运行）
    private readonly ConcurrentDictionary<long, GenJob> _jobs = new();

    public ClaudeCodeRunner(PresalesPaths paths, PresalesRepository repo, ILogger<ClaudeCodeRunner> log)
    {
        _paths = paths;
        _repo = repo;
        _log = log;
    }

    public GenJob? GetJob(long projectId) => _jobs.TryGetValue(projectId, out var j) ? j : null;

    public bool IsRunning(long projectId) =>
        _jobs.TryGetValue(projectId, out var j) && j.Status is "queued" or "running";

    /// <summary>
    /// 启动一次方案生成（不阻塞）。返回所写指令文件的文件名。
    /// </summary>
    public async Task<string> StartAsync(PresalesProject project)
    {
        Directory.CreateDirectory(project.ProjectDir!);

        // 1) 生成并落盘指令文档
        var commandText = BuildCommandDocument(project);
        var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var commandFileName = $"生成指令-{stamp}.txt";
        var commandPath = Path.Combine(project.ProjectDir!, commandFileName);
        // 与既有卡片一致：UTF-8 BOM，便于在 Windows 记事本正确显示中文
        await File.WriteAllTextAsync(commandPath, commandText, new UTF8Encoding(true));

        // 2) 记录生成任务
        var genId = await _repo.CreateGenerationAsync(new PresalesGeneration
        {
            ProjectId = project.Id,
            ProjectName = project.ProjectName,
            CommandFile = commandFileName,
            Status = "running",
        });
        await _repo.SetGenStatusAsync(project.Id, "running", commandFileName);

        var job = new GenJob
        {
            ProjectId = project.Id,
            GenerationId = genId,
            CommandFile = commandFileName,
            Status = "running",
            StartedAt = DateTime.Now,
        };
        _jobs[project.Id] = job;

        // 3) 后台执行 claude，不阻塞请求线程
        _ = Task.Run(() => RunClaudeAsync(project, commandText, commandPath, job));

        return commandFileName;
    }

    private async Task RunClaudeAsync(PresalesProject project, string commandText, string commandPath, GenJob job)
    {
        var sb = job.LogBuilder;
        var existingBefore = SnapshotOutputs(project.ProjectDir!);
        try
        {
            // ===== 阶段一：生成解决方案 HTML 提案 与 xxx-模块-URS.html =====
            AppendLog(sb, "==== 阶段一：生成解决方案 HTML 提案与 URS 文档 ====");
            var htmlPrompt =
                $"请严格按照以下指令文档完成 SmartLabOS 售前技术方案的生成。所有技术参数必须引用自 references/ 知识库，" +
                $"禁止编造。指令文档内容如下：\n\n{commandText}";

            var exit1 = await RunClaudeWithRetryAsync(project, htmlPrompt, job, sb, "HTML 提案");
            if (exit1 is null) return;          // 启动失败/超时：FailAsync 已在内部调用
            if (exit1 != 0)
            {
                await FailAsync(project, job, exit1, $"HTML 提案生成失败：claude 退出码 {exit1}");
                return;
            }

            // ===== 阶段二：依据「详细设计方案版」提纲，生成 WORD(.docx) 提案 =====
            var docxFileName = BuildDocxFileName(project.ProjectName);
            var docxFullPath = Path.Combine(project.ProjectDir!, docxFileName);
            var wordCommand = BuildWordCommandDocument(project, docxFullPath);

            // 落盘 WORD 指令文档，便于追溯（UTF-8 BOM，记事本可正确显示中文）
            var wordCmdFileName = $"WORD生成指令-{DateTime.Now:yyyyMMdd-HHmmss}.txt";
            await File.WriteAllTextAsync(
                Path.Combine(project.ProjectDir!, wordCmdFileName), wordCommand, new UTF8Encoding(true));

            AppendLog(sb, "==== 阶段二：生成 WORD(.docx) 提案文档 ====");
            var wordPrompt =
                $"请严格按照以下指令文档，基于已生成的 HTML 提案与 URS 文档，产出一份 WORD(.docx) 详细设计方案提案。" +
                $"所有技术参数必须引用自 references/ 知识库及本项目目录下已生成的 HTML 文件，禁止编造。指令文档内容如下：\n\n{wordCommand}";

            var exit2 = await RunClaudeWithRetryAsync(project, wordPrompt, job, sb, "WORD 提案");
            if (exit2 is null) return;
            if (exit2 != 0)
            {
                await FailAsync(project, job, exit2, $"WORD 提案生成失败：claude 退出码 {exit2}");
                return;
            }

            // ===== 收尾：汇总本次新增/变更的输出文件（含 .docx）=====
            var outputs = NewOrChangedOutputs(project.ProjectDir!, existingBefore);
            job.Status = "succeeded";
            job.ExitCode = exit2;
            job.OutputFiles = outputs;
            job.DocxFile = File.Exists(docxFullPath) ? docxFileName : null;
            if (job.DocxFile is null)
                AppendLog(sb, $"[警告] 未在项目目录下检测到预期的 WORD 文件：{docxFileName}");
            job.FinishedAt = DateTime.Now;
            await _repo.FinishGenerationAsync(job.GenerationId, "succeeded", exit2, outputs, Tail(sb));
            await _repo.SetGenStatusAsync(project.Id, "succeeded", markGenerated: true);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "方案生成失败 project={Project}", project.ProjectName);
            AppendLog(sb, "[异常] " + ex.Message);
            await FailAsync(project, job, null, ex.Message);
        }
    }

    // headless claude 运行中常见的「瞬态」错误标记：命中且仍有重试机会时自动重试该阶段。
    private static readonly string[] TransientErrorMarkers =
    {
        "Stream idle timeout",   // 流空闲超时（长耗时阶段无 token 输出时易触发）
        "API Error",
        "overloaded",
        "rate limit",
        "Connection error",
        "ECONNRESET",
        "fetch failed",
    };

    /// <summary>
    /// 运行一个阶段，并在命中疑似瞬态错误（见 TransientErrorMarkers）且退出码非 0 时自动重试。
    /// 返回最终退出码；若进程未能启动/超时（已 FailAsync），返回 null。
    /// </summary>
    private async Task<int?> RunClaudeWithRetryAsync(
        PresalesProject project, string prompt, GenJob job, StringBuilder sb, string phaseLabel, int maxAttempts = 2)
    {
        for (int attempt = 1; ; attempt++)
        {
            if (attempt > 1)
                AppendLog(sb, $"[重试] {phaseLabel}：第 {attempt}/{maxAttempts} 次尝试…");

            var markLen = SbLength(sb);
            var exit = await RunClaudeOnceAsync(project, prompt, job, sb);
            if (exit is null) return null;   // 启动失败/超时：FailAsync 已在内部调用
            if (exit == 0) return 0;

            var recent = RecentLog(sb, markLen);
            var transient = TransientErrorMarkers.Any(m => recent.Contains(m, StringComparison.OrdinalIgnoreCase));
            if (!transient || attempt >= maxAttempts)
            {
                if (transient)
                    AppendLog(sb, $"[重试] {phaseLabel}：已达最大重试次数({maxAttempts})，放弃。");
                return exit;
            }
            AppendLog(sb, $"[重试] {phaseLabel}：检测到疑似瞬态错误，准备重试。");
        }
    }

    /// <summary>
    /// 启动一次 headless claude 进程并等待其结束。返回退出码；
    /// 若进程未能启动或超时（此时已调用 FailAsync 标记失败），返回 null。
    /// </summary>
    private async Task<int?> RunClaudeOnceAsync(PresalesProject project, string prompt, GenJob job, StringBuilder sb)
    {
        // 直接启动 claude(.exe)，不经 cmd.exe —— 规避 cmd 对引号的特殊处理与中文乱码。
        // 参数用 ArgumentList 逐项添加，由运行时负责转义，无需手工拼引号。
        var psi = new ProcessStartInfo
        {
            FileName = _paths.ClaudeExecutable,   // "claude" 走 PATH 解析，或绝对路径 claude.exe
            WorkingDirectory = project.ProjectDir!,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            // 关键：以 UTF-8(无 BOM) 写 stdin，否则中文提示语会被按控制台代码页(GBK)发送而乱码
            StandardInputEncoding = new UTF8Encoding(false),
        };
        psi.ArgumentList.Add("-p");
        psi.ArgumentList.Add("--permission-mode");
        psi.ArgumentList.Add("bypassPermissions");
        psi.ArgumentList.Add("--add-dir");
        psi.ArgumentList.Add(_paths.ProjectRoot);

        using var p = new Process { StartInfo = psi };
        p.OutputDataReceived += (_, e) => AppendLog(sb, e.Data);
        p.ErrorDataReceived += (_, e) => AppendLog(sb, e.Data);

        try
        {
            if (!p.Start())
            {
                await FailAsync(project, job, null, "无法启动 claude 进程。");
                return null;
            }
        }
        catch (Exception ex)
        {
            await FailAsync(project, job, null,
                $"无法启动 claude：{ex.Message}。请确认已安装 Claude Code，且 appsettings.json 的 Presales:ClaudeExecutable 指向正确的可执行文件(如 C:\\Users\\<用户>\\.local\\bin\\claude.exe)。");
            return null;
        }
        job.ProcessId = p.Id;
        p.BeginOutputReadLine();
        p.BeginErrorReadLine();

        // 经 stdin 传入提示语后关闭，触发 headless 处理。
        // 若进程已异常退出，写入会抛 IOException(管道已结束)，吞掉以便后续读取退出码。
        try
        {
            await p.StandardInput.WriteAsync(prompt);
            p.StandardInput.Close();
        }
        catch (IOException ioex)
        {
            AppendLog(sb, "[警告] 写入 claude 标准输入失败：" + ioex.Message);
        }

        var timeout = TimeSpan.FromMinutes(_paths.GenerationTimeoutMinutes);
        if (!p.WaitForExit((int)timeout.TotalMilliseconds))
        {
            try { p.Kill(true); } catch { }
            await FailAsync(project, job, null, $"生成超时（>{_paths.GenerationTimeoutMinutes} 分钟），已终止。");
            return null;
        }
        p.WaitForExit(); // 确保异步日志读取收尾
        return p.ExitCode;
    }

    private async Task FailAsync(PresalesProject project, GenJob job, int? exit, string message)
    {
        AppendLog(job.LogBuilder, "[失败] " + message);
        job.Status = "failed";
        job.ExitCode = exit;
        job.Error = message;
        job.FinishedAt = DateTime.Now;
        await _repo.FinishGenerationAsync(job.GenerationId, "failed", exit, job.OutputFiles, Tail(job.LogBuilder));
        await _repo.SetGenStatusAsync(project.Id, "failed");
    }

    // ---------------- 指令文档生成 ----------------

    /// <summary>
    /// 依据项目需求信息，生成一份形如「01-最新设计-…(1~8).txt」的 Claude Code 指令文档。
    /// </summary>
    public string BuildCommandDocument(PresalesProject p)
    {
        var sb = new StringBuilder();
        var protocolDir = _paths.ProtocolDir;
        var refDir = Path.Combine(_paths.ProjectRoot, "references");

        sb.AppendLine($"# SmartLabOS 售前方案自动生成指令 —— 项目：{p.ProjectName}");
        sb.AppendLine($"# 生成时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();
        sb.AppendLine("1. 知识库（所有技术参数必须引用自此，严禁编造）：");
        sb.AppendLine($"(1) 模块(共97个)：{Path.Combine(refDir, "01-modules")}");
        sb.AppendLine($"(2) 平台抽象类(3种)：{Path.Combine(refDir, "02-platforms")}  —— 一个平台最多搭载6个模组、8个托盘");
        sb.AppendLine($"    托盘资料：{Path.Combine(refDir, "06-pallet")}");
        sb.AppendLine($"(3) 工作站：{Path.Combine(refDir, "03-workstation")}");
        sb.AppendLine($"(4) 解决方案：{Path.Combine(refDir, "04-solutions")}");
        sb.AppendLine($"(5) 项目范例：{Path.Combine(refDir, "05-Project")}");
        sb.AppendLine();

        sb.AppendLine($"2. 本项目提案输出目录：{p.ProjectDir}");
        sb.AppendLine();

        sb.AppendLine("3. 客户需求信息：");
        sb.AppendLine("------------------------------------------------------------------");
        sb.AppendLine("3.1 客户项目现状：");
        sb.AppendLine(string.IsNullOrWhiteSpace(p.CurrentStatus) ? "（未填写）" : p.CurrentStatus!.Trim());
        sb.AppendLine();

        sb.AppendLine("3.2 客户挑战：");
        AppendNumbered(sb, p.Challenges, "挑战");
        sb.AppendLine();

        sb.AppendLine("3.3 客户期望：");
        AppendNumbered(sb, p.Expectations, "期望");
        sb.AppendLine();

        sb.AppendLine($"3.4 流程范围：{Join(p.ProcessScope)}");
        sb.AppendLine($"3.5 上下料方式：{Or(p.LoadingMethod)}");
        sb.AppendLine($"3.6 软件功能：{Or(p.SoftwareType)}");
        sb.AppendLine("------------------------------------------------------------------");
        sb.AppendLine();

        sb.AppendLine("4. 需实现的检验/检测流程标准（逐个生成方案）：");
        if (p.Protocols.Count == 0)
        {
            sb.AppendLine("（未选择任何流程标准）");
        }
        else
        {
            for (int i = 0; i < p.Protocols.Count; i++)
            {
                var file = p.Protocols[i];
                var full = Path.Combine(protocolDir, file);
                var idx = i + 1;
                sb.AppendLine($"4.{idx} 标准文档：{full}");
                sb.AppendLine($"    (a) 实现该文档中记述的「提取 & 净化」前处理流程；");
                sb.AppendLine($"    (b) 从97个模组中选型，选定3种平台中适合的型号，在平台上搭载模组组成工作站，");
                sb.AppendLine($"        再由工作站按前处理流程顺序串联成样品制备解决方案；用 HTML 格式给出方案，");
                sb.AppendLine($"        必要时加入 SVG、Mermaid 图示。HTML 文件保存为：{Path.Combine(p.ProjectDir!, $"{idx}.html")}");
                sb.AppendLine();
            }
        }

        sb.AppendLine("5. 输出格式与计算要求：");
        sb.AppendLine($"5.1 所有提案 HTML 必须遵循模版的全部内容项：{_paths.TemplateFile}");
        sb.AppendLine("5.2 根据各平台所含模块功能码耗时 + 上下料时间，明确列出每个平台处理耗时，");
        sb.AppendLine("    叠加得出整个解决方案耗时（明确列出）；各平台处理时间尽量相等，并尽量接近 600 秒(10分钟)，");
        sb.AppendLine("    以此为标准拆分/设立平台。");
        var loadingNote = p.LoadingMethod == "自动上下料"
            ? "本项目采用「自动上下料」，上下料时间按自动方式估算。"
            : p.LoadingMethod == "人工上下料"
                ? "本项目采用「人工上下料」，上下料时间按人工方式估算。"
                : "上下料方式未指定时，按模块卡片默认值估算。";
        sb.AppendLine("    " + loadingNote);
        sb.AppendLine("5.3 若97个模块中没有合适模块实现某前处理需求，请输出新建模块的 URS 文档，");
        sb.AppendLine("    命名为「xxx-模块-URS.html」，必要时加入 SVG、Mermaid 图示；该新建模块的处理时间");
        sb.AppendLine("    采用规范 MD 文档中规定的时间 + 上下料时间，计入平台总耗时。");
        sb.AppendLine();
        sb.AppendLine("6. 完成后，请在项目目录下输出一份「Summary-输出总结.md」，列出本次生成的所有文件与方案要点。");

        return sb.ToString();
    }

    /// <summary>
    /// WORD 提案文件名：「{项目名}_SmartLabOS_Presales_提案_yyyyMMddHHmmss.docx」。
    /// </summary>
    private static string BuildDocxFileName(string projectName) =>
        $"{projectName}_SmartLabOS_Presales_提案_{DateTime.Now:yyyyMMddHHmmss}.docx";

    /// <summary>
    /// 第二阶段指令文档：令 Claude Code 依据「详细设计方案版」内容提纲(02-*.md)，
    /// 整合本项目目录下已生成的 HTML 提案与 URS 文档，产出一份 WORD(.docx) 提案。
    /// </summary>
    public string BuildWordCommandDocument(PresalesProject p, string docxFullPath)
    {
        var sb = new StringBuilder();
        var refDir = Path.Combine(_paths.ProjectRoot, "references");

        sb.AppendLine($"# SmartLabOS 售前 WORD 提案生成指令 —— 项目：{p.ProjectName}");
        sb.AppendLine($"# 生成时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();
        sb.AppendLine("目标：在前一阶段已生成「解决方案 HTML 提案」与「xxx-模块-URS.html」的基础上，");
        sb.AppendLine("整合形成一份完整、专业的 WORD(.docx) 详细设计方案提案文档。");
        sb.AppendLine();

        sb.AppendLine("1. 内容提纲要求（必须严格按此 MD 文档的章节结构与勾选项组织 WORD 内容）：");
        sb.AppendLine($"   {_paths.DocxOutlineFile}");
        sb.AppendLine("   - ★必备 项缺项即不合格；○按需 项可按本项目裁剪，但裁剪需在文中说明理由。");
        sb.AppendLine("   - 所有设备/软件指标必须为「具体数值 + 单位 + 精度 + 通讯协议」，禁止「高精度/快速」等定性描述。");
        sb.AppendLine();

        sb.AppendLine("2. 数据来源（严禁编造，所有参数须可溯源）：");
        sb.AppendLine($"   (1) 本项目目录下已生成的全部 *.html 提案与 *-模块-URS.html：{p.ProjectDir}");
        sb.AppendLine($"   (2) 知识库 references/（模块/平台/工作站/解决方案/项目范例）：{refDir}");
        sb.AppendLine($"   (3) 客户需求信息：见下方第 5 节。");
        sb.AppendLine();

        sb.AppendLine("3. 客户需求信息（用于撰写第1章项目概述、第2章标准适配等）：");
        sb.AppendLine("------------------------------------------------------------------");
        sb.AppendLine("3.1 客户项目现状：");
        sb.AppendLine(string.IsNullOrWhiteSpace(p.CurrentStatus) ? "（未填写）" : p.CurrentStatus!.Trim());
        sb.AppendLine();
        sb.AppendLine("3.2 客户挑战：");
        AppendNumbered(sb, p.Challenges, "挑战");
        sb.AppendLine();
        sb.AppendLine("3.3 客户期望：");
        AppendNumbered(sb, p.Expectations, "期望");
        sb.AppendLine();
        sb.AppendLine($"3.4 流程范围：{Join(p.ProcessScope)}");
        sb.AppendLine($"3.5 上下料方式：{Or(p.LoadingMethod)}");
        sb.AppendLine($"3.6 软件功能：{Or(p.SoftwareType)}");
        sb.AppendLine("------------------------------------------------------------------");
        sb.AppendLine();

        sb.AppendLine("4. 生成方式与输出要求：");
        sb.AppendLine("4.1 使用 docx 文档技能（document-skills:docx / python-docx）生成真正的 .docx 文件，");
        sb.AppendLine("    不要仅输出 Markdown 或 HTML，最终交付物必须是可直接打开的 Word 文档。");
        sb.AppendLine("4.2 文档需包含规范的封面、目录(标题分级)、页码，正文按提纲分章节编排，");
        sb.AppendLine("    适当使用表格呈现「关键技术指标表」「检测标准与设备适配表」「实施阶段表」等。");
        sb.AppendLine("4.3 各功能平台须满足「平台组成 + 功能说明 + 关键技术指标表」三要素，");
        sb.AppendLine("    指标须量化并含通讯协议；耗时/产能数据须与 HTML 提案中的计算保持一致。");
        sb.AppendLine("4.4 全文使用简体中文；专业术语（QuEChERS、SPE、ICP-MS、LC-MS/MS 等）保持原文。");
        sb.AppendLine();

        sb.AppendLine("5. WORD 文件保存（务必使用以下完整路径与文件名，不得改名、不得另存到其他目录）：");
        sb.AppendLine($"   {docxFullPath}");
        sb.AppendLine();
        sb.AppendLine("6. 完成后，在运行日志中明确输出该 .docx 的最终保存绝对路径，便于核对。");

        return sb.ToString();
    }

    private static void AppendNumbered(StringBuilder sb, List<string> items, string label)
    {
        var nonEmpty = items.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        if (nonEmpty.Count == 0) { sb.AppendLine("（未填写）"); return; }
        for (int i = 0; i < nonEmpty.Count; i++)
            sb.AppendLine($"  {label}-{i + 1}：{nonEmpty[i].Trim()}");
    }

    private static string Join(List<string> items) =>
        items.Count == 0 ? "（未选择）" : string.Join("、", items);

    private static string Or(string? s) => string.IsNullOrWhiteSpace(s) ? "（未指定）" : s!;

    // ---------------- 文件/日志辅助 ----------------

    // 纳入产出快照的文件类型：解决方案/URS 的 *.html 与最终 WORD 提案 *.docx。
    private static readonly string[] OutputPatterns = { "*.html", "*.docx" };

    private static IEnumerable<string> EnumerateOutputs(string dir) =>
        OutputPatterns.SelectMany(pat => Directory.EnumerateFiles(dir, pat));

    private static Dictionary<string, DateTime> SnapshotOutputs(string dir)
    {
        var map = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        if (!Directory.Exists(dir)) return map;
        foreach (var f in EnumerateOutputs(dir))
            map[Path.GetFileName(f)] = File.GetLastWriteTimeUtc(f);
        return map;
    }

    private static List<string> NewOrChangedOutputs(string dir, Dictionary<string, DateTime> before)
    {
        var outFiles = new List<string>();
        if (!Directory.Exists(dir)) return outFiles;
        foreach (var f in EnumerateOutputs(dir))
        {
            var name = Path.GetFileName(f);
            var mtime = File.GetLastWriteTimeUtc(f);
            if (!before.TryGetValue(name, out var old) || mtime > old)
                outFiles.Add(name);
        }
        outFiles.Sort(StringComparer.OrdinalIgnoreCase);
        return outFiles;
    }

    private static void AppendLog(StringBuilder sb, string? line)
    {
        if (line is null) return;
        lock (sb)
        {
            // 限制日志总量，避免内存膨胀
            if (sb.Length < 200_000) sb.AppendLine(line);
        }
    }

    private static string Tail(StringBuilder sb)
    {
        lock (sb)
        {
            var s = sb.ToString();
            return s.Length <= 20_000 ? s : s[^20_000..];
        }
    }

    private static int SbLength(StringBuilder sb)
    {
        lock (sb) { return sb.Length; }
    }

    /// <summary>取本次尝试期间新增的日志（用于判断是否瞬态错误）。</summary>
    private static string RecentLog(StringBuilder sb, int fromIndex)
    {
        lock (sb)
        {
            if (fromIndex < 0 || fromIndex >= sb.Length) return "";
            return sb.ToString(fromIndex, sb.Length - fromIndex);
        }
    }
}

/// <summary>单个项目最近一次生成任务的实时状态（内存）。</summary>
public sealed class GenJob
{
    public long ProjectId { get; set; }
    public long GenerationId { get; set; }
    public string CommandFile { get; set; } = "";
    public string Status { get; set; } = "queued";   // queued/running/succeeded/failed
    public int? ExitCode { get; set; }
    public int? ProcessId { get; set; }
    public string? Error { get; set; }
    public List<string> OutputFiles { get; set; } = new();
    /// <summary>本次生成的 WORD 提案文件名（若已成功生成）。</summary>
    public string? DocxFile { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public StringBuilder LogBuilder { get; } = new();

    public string LogTail(int n = 4000)
    {
        lock (LogBuilder)
        {
            var s = LogBuilder.ToString();
            return s.Length <= n ? s : s[^n..];
        }
    }
}
