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
    private readonly ModuleCatalog _catalog;
    private readonly ILogger<ClaudeCodeRunner> _log;

    // 项目ID -> 实时任务状态（仅最近一次运行）
    private readonly ConcurrentDictionary<long, GenJob> _jobs = new();
    // 项目ID -> 最近一次「模块选定」推理任务状态
    private readonly ConcurrentDictionary<long, ModuleSelectJob> _selectJobs = new();

    public ClaudeCodeRunner(PresalesPaths paths, PresalesRepository repo, ModuleCatalog catalog, ILogger<ClaudeCodeRunner> log)
    {
        _paths = paths;
        _repo = repo;
        _catalog = catalog;
        _log = log;
    }

    public GenJob? GetJob(long projectId) => _jobs.TryGetValue(projectId, out var j) ? j : null;

    public bool IsRunning(long projectId) =>
        _jobs.TryGetValue(projectId, out var j) && j.Status is "queued" or "running";

    public ModuleSelectJob? GetSelectJob(long projectId) =>
        _selectJobs.TryGetValue(projectId, out var j) ? j : null;

    public bool IsSelecting(long projectId) =>
        _selectJobs.TryGetValue(projectId, out var j) && j.Status is "queued" or "running";

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
            job.Phase = 1;
            AppendLog(sb, "==== 阶段一：生成解决方案 HTML 提案与 URS 文档 ====");
            var htmlPrompt =
                $"请严格按照以下指令文档完成 SmartLabOS 售前技术方案的生成。所有技术参数必须引用自 references/ 知识库，" +
                $"禁止编造。指令文档内容如下：\n\n{commandText}";

            var (exit1, fatal1) = await RunHeadlessWithRetryAsync(
                project, htmlPrompt, sb, "HTML 提案", pid => job.ProcessId = pid);
            if (fatal1 is not null) { await FailAsync(project, job, null, fatal1); return; }
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

            job.Phase = 2;
            AppendLog(sb, "==== 阶段二：生成 WORD(.docx) 提案文档 ====");
            var wordPrompt =
                $"请严格按照以下指令文档，基于已生成的 HTML 提案与 URS 文档，产出一份 WORD(.docx) 详细设计方案提案。" +
                $"所有技术参数必须引用自 references/ 知识库及本项目目录下已生成的 HTML 文件，禁止编造。指令文档内容如下：\n\n{wordCommand}";

            var (exit2, fatal2) = await RunHeadlessWithRetryAsync(
                project, wordPrompt, sb, "WORD 提案", pid => job.ProcessId = pid);
            if (fatal2 is not null) { await FailAsync(project, job, null, fatal2); return; }
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
    /// 返回 (退出码, 致命错误)：正常退出时 Fatal 为 null；进程未能启动/超时时 Exit 为 null、Fatal 为原因。
    /// 本方法不直接改写任务状态/数据库，由调用方据 Fatal/Exit 决定如何收尾。
    /// </summary>
    private async Task<(int? Exit, string? Fatal)> RunHeadlessWithRetryAsync(
        PresalesProject project, string prompt, StringBuilder sb, string phaseLabel, Action<int> setPid, int maxAttempts = 2)
    {
        for (int attempt = 1; ; attempt++)
        {
            if (attempt > 1)
                AppendLog(sb, $"[重试] {phaseLabel}：第 {attempt}/{maxAttempts} 次尝试…");

            var markLen = SbLength(sb);
            var (exit, fatal) = await RunHeadlessOnceAsync(project, prompt, sb, setPid);
            if (fatal is not null) return (null, fatal);   // 启动失败/超时
            if (exit == 0) return (0, null);

            var recent = RecentLog(sb, markLen);
            var transient = TransientErrorMarkers.Any(m => recent.Contains(m, StringComparison.OrdinalIgnoreCase));
            if (!transient || attempt >= maxAttempts)
            {
                if (transient)
                    AppendLog(sb, $"[重试] {phaseLabel}：已达最大重试次数({maxAttempts})，放弃。");
                return (exit, null);
            }
            AppendLog(sb, $"[重试] {phaseLabel}：检测到疑似瞬态错误，准备重试。");
        }
    }

    /// <summary>
    /// 启动一次 headless claude 进程并等待其结束。返回 (退出码, 致命错误)：
    /// 正常结束时 (ExitCode, null)；进程未能启动或超时时 (null, 原因)。不改写任务状态/数据库。
    /// </summary>
    private async Task<(int? Exit, string? Fatal)> RunHeadlessOnceAsync(
        PresalesProject project, string prompt, StringBuilder sb, Action<int> setPid)
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
                return (null, "无法启动 claude 进程。");
        }
        catch (Exception ex)
        {
            return (null,
                $"无法启动 claude：{ex.Message}。请确认已安装 Claude Code，且 appsettings.json 的 Presales:ClaudeExecutable 指向正确的可执行文件(如 C:\\Users\\<用户>\\.local\\bin\\claude.exe)。");
        }
        setPid(p.Id);
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
            return (null, $"执行超时（>{_paths.GenerationTimeoutMinutes} 分钟），已终止。");
        }
        p.WaitForExit(); // 确保异步日志读取收尾
        return (p.ExitCode, null);
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

    // ---------------- 模块选定（自动推理） ----------------

    /// <summary>
    /// 启动一次「模块选定」自动推理（不阻塞）：调用 headless claude，依据国标与客户需求，
    /// 从 references/01-modules 的97个模块中选出实现前处理流程所需的模块，写入
    /// 「模块选定-推荐.json」。完成后解析该文件得到推荐模块ID，落库(confirmed=false)。
    /// </summary>
    public Task StartModuleSelectionAsync(PresalesProject project)
    {
        Directory.CreateDirectory(project.ProjectDir!);

        var job = new ModuleSelectJob
        {
            ProjectId = project.Id,
            Status = "running",
            StartedAt = DateTime.Now,
        };
        _selectJobs[project.Id] = job;

        var jsonOut = Path.Combine(project.ProjectDir!, "模块选定-推荐.json");
        // 删除上一轮残留，确保读到本次新结果
        try { if (File.Exists(jsonOut)) File.Delete(jsonOut); } catch { /* ignore */ }

        var command = BuildModuleSelectionCommand(project, jsonOut);
        _ = Task.Run(() => RunModuleSelectionAsync(project, command, jsonOut, job));
        return Task.CompletedTask;
    }

    private async Task RunModuleSelectionAsync(PresalesProject project, string command, string jsonOut, ModuleSelectJob job)
    {
        var sb = job.LogBuilder;
        try
        {
            AppendLog(sb, "==== 模块选定：依据国标 + 客户需求，从 references/01-modules 推理推荐模块 ====");
            var prompt =
                $"请严格按照以下指令完成 SmartLabOS「模块选定」。只能从 references/01-modules 目录下真实存在的模块中选择，" +
                $"严禁编造模块编号。只需产出一个 JSON 结果文件，不要生成其它文件。指令内容如下：\n\n{command}";

            var (exit, fatal) = await RunHeadlessWithRetryAsync(
                project, prompt, sb, "模块选定", pid => job.ProcessId = pid);
            if (fatal is not null) { FailSelect(job, fatal); return; }
            if (exit != 0) { FailSelect(job, $"模块选定失败：claude 退出码 {exit}"); return; }

            var ids = ReadRecommendedModules(jsonOut, sb);
            await _repo.SetModulesAsync(project.Id, ids, confirmed: false);

            job.Recommended = ids;
            job.Status = "succeeded";
            job.FinishedAt = DateTime.Now;
            AppendLog(sb, ids.Count == 0
                ? "[警告] 未能从结果中解析出任何有效模块ID，请改用手动「添加模块」。"
                : $"模块选定完成：推荐 {ids.Count} 个模块 —— {string.Join("、", ids)}");
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "模块选定失败 project={Project}", project.ProjectName);
            FailSelect(job, ex.Message);
        }
    }

    private void FailSelect(ModuleSelectJob job, string message)
    {
        AppendLog(job.LogBuilder, "[失败] " + message);
        job.Status = "failed";
        job.Error = message;
        job.FinishedAt = DateTime.Now;
    }

    /// <summary>解析「模块选定-推荐.json」得到推荐模块ID；文件缺失/不规范时回退到运行日志中按规则抽取。</summary>
    private List<string> ReadRecommendedModules(string jsonOut, StringBuilder sb)
    {
        string content = "";
        try { if (File.Exists(jsonOut)) content = File.ReadAllText(jsonOut); }
        catch (Exception ex) { AppendLog(sb, "[警告] 读取模块选定结果文件失败：" + ex.Message); }

        var ids = _catalog.ExtractFromText(content);
        if (ids.Count == 0)
        {
            // 回退：claude 可能未落盘文件，但在日志中列出了模块ID
            ids = _catalog.ExtractFromText(Tail(sb));
            if (ids.Count > 0)
                AppendLog(sb, "[提示] 结果文件缺失/为空，已从运行日志回退解析模块ID。");
        }
        return ids;
    }

    /// <summary>「模块选定」指令文档：令 claude 读取国标与需求，从97个模块中选型并输出 JSON 结果文件。</summary>
    public string BuildModuleSelectionCommand(PresalesProject p, string jsonOutPath)
    {
        var sb = new StringBuilder();
        var protocolDir = _paths.ProtocolDir;

        sb.AppendLine($"# SmartLabOS 模块选定指令 —— 项目：{p.ProjectName}");
        sb.AppendLine($"# 生成时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();
        sb.AppendLine("目标：依据下述检验/检测国标的「提取 & 净化」前处理流程，结合客户需求，");
        sb.AppendLine("从模块知识库中选出**实现该前处理流程所必需的最小充分模块集合**。");
        sb.AppendLine();

        sb.AppendLine($"1. 模块知识库目录（候选模块仅限于此，模块ID = 文件名去扩展名，如 MOD-CC-001）：");
        sb.AppendLine($"   {_paths.ModulesDir}");
        sb.AppendLine("   - 必须先浏览该目录下的模块卡片，依据每个模块的功能描述/功能码/分类判断是否匹配；");
        sb.AppendLine("   - 严禁编造目录中不存在的模块编号；只能引用真实存在的卡片文件名作为模块ID。");
        sb.AppendLine();

        sb.AppendLine("2. 需实现的检验/检测流程标准：");
        if (p.Protocols.Count == 0)
        {
            sb.AppendLine("   （未选择任何流程标准——请结合下方客户需求与流程范围进行选型）");
        }
        else
        {
            for (int i = 0; i < p.Protocols.Count; i++)
                sb.AppendLine($"   2.{i + 1} {Path.Combine(protocolDir, p.Protocols[i])}");
        }
        sb.AppendLine();

        sb.AppendLine("3. 客户需求信息（用于约束选型）：");
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

        sb.AppendLine("4. 选型原则：");
        sb.AppendLine("   (1) 覆盖前处理流程的每一道工序（如加液/涡旋/离心/浓缩/SPE净化/定容等），不遗漏关键步骤；");
        sb.AppendLine("   (2) 在满足工艺前提下尽量精简，避免功能重复的冗余模块；");
        sb.AppendLine("   (3) 若某工序在97个模块中确无匹配模块，可在结果 notes 中说明（不要编造模块ID）。");
        sb.AppendLine();

        sb.AppendLine("5. 输出要求（务必只产出以下一个 JSON 文件，不要生成 HTML/Word/Markdown 等其它文件）：");
        sb.AppendLine($"   文件路径：{jsonOutPath}");
        sb.AppendLine("   文件内容为 UTF-8 的 JSON 对象，格式如下：");
        sb.AppendLine("   {");
        sb.AppendLine("     \"modules\": [\"MOD-XX-001\", \"MOD-YY-002\"],   // 推荐模块ID数组，按前处理流程顺序排列");
        sb.AppendLine("     \"notes\": \"选型理由与未覆盖工序说明（可选）\"");
        sb.AppendLine("   }");
        sb.AppendLine("6. 完成后，请在运行日志中明确列出最终推荐的模块ID清单，便于核对。");

        return sb.ToString();
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

        AppendConfirmedModules(sb, p);

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
                sb.AppendLine($"    (b) 仅从上文「模块选型范围 — 强制」清单中的已确认模块选型，选定3种平台中适合的型号，在平台上搭载模组组成工作站，");
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

        AppendConfirmedModules(sb, p);

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

    /// <summary>
    /// 渲染「已确认模块（强制选型范围）」区块：方案生成只能从该清单中选型组装平台/工作站，
    /// 不得引入清单外的模块。模块经「模块选定 → 模块确认」流程确定。
    /// </summary>
    private void AppendConfirmedModules(StringBuilder sb, PresalesProject p)
    {
        sb.AppendLine("【模块选型范围 — 强制】");
        sb.AppendLine("本方案已经过「模块选定 → 模块确认」，最终确认的模块清单如下；");
        sb.AppendLine("方案生成**只能从以下已确认模块中选型**，组装平台/工作站与解决方案，");
        sb.AppendLine("严禁引入清单之外的任何模块（如确有缺口，请在文中明确说明，不得自行替换）。");
        if (p.Modules.Count == 0)
        {
            sb.AppendLine("  （未确认任何模块——请联系业务确认模块清单后再生成）");
        }
        else
        {
            for (int i = 0; i < p.Modules.Count; i++)
            {
                var id = p.Modules[i];
                var info = _catalog.Find(id);
                var label = info is null
                    ? id
                    : $"{info.Id}  {info.Name}{(string.IsNullOrWhiteSpace(info.Category) ? "" : $"（{info.Category}）")}";
                sb.AppendLine($"  {i + 1}. {label}");
            }
            sb.AppendLine($"  模块卡片目录：{_paths.ModulesDir}（按上述ID取对应 .md 卡片读取参数）");
        }
        sb.AppendLine();
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
    /// <summary>当前阶段：0=准备中, 1=生成 HTML/URS, 2=生成 WORD。共 2 个实质阶段。</summary>
    public int Phase { get; set; }
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

/// <summary>单个项目最近一次「模块选定」自动推理任务的实时状态（内存）。</summary>
public sealed class ModuleSelectJob
{
    public long ProjectId { get; set; }
    public string Status { get; set; } = "queued";   // queued/running/succeeded/failed
    public int? ProcessId { get; set; }
    public string? Error { get; set; }
    /// <summary>本次推理推荐的模块ID清单（已按知识库校验、保持顺序）。</summary>
    public List<string> Recommended { get; set; } = new();
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
