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
        var existingBefore = SnapshotHtml(project.ProjectDir!);
        try
        {
            // 提示语简短，正文经 stdin 传入；让 claude 严格执行指令文档。
            var prompt =
                $"请严格按照以下指令文档完成 SmartLabOS 售前技术方案的生成。所有技术参数必须引用自 references/ 知识库，" +
                $"禁止编造。指令文档内容如下：\n\n{commandText}";

            var args =
                $"/c \"{_paths.ClaudeExecutable}\" -p " +
                $"--permission-mode bypassPermissions " +
                $"--add-dir \"{_paths.ProjectRoot}\"";

            var psi = new ProcessStartInfo("cmd.exe", args)
            {
                WorkingDirectory = project.ProjectDir!,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };

            using var p = new Process { StartInfo = psi };
            p.OutputDataReceived += (_, e) => AppendLog(sb, e.Data);
            p.ErrorDataReceived += (_, e) => AppendLog(sb, e.Data);

            if (!p.Start())
            {
                await FailAsync(project, job, null, "无法启动 claude 进程。");
                return;
            }
            job.ProcessId = p.Id;
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            // 经 stdin 传入提示语后关闭，触发 headless 处理
            await p.StandardInput.WriteAsync(prompt);
            p.StandardInput.Close();

            var timeout = TimeSpan.FromMinutes(_paths.GenerationTimeoutMinutes);
            if (!p.WaitForExit((int)timeout.TotalMilliseconds))
            {
                try { p.Kill(true); } catch { }
                await FailAsync(project, job, null, $"生成超时（>{_paths.GenerationTimeoutMinutes} 分钟），已终止。");
                return;
            }
            p.WaitForExit(); // 确保异步日志读取收尾

            var exit = p.ExitCode;
            var outputs = NewOrChangedHtml(project.ProjectDir!, existingBefore);

            if (exit == 0)
            {
                job.Status = "succeeded";
                job.ExitCode = exit;
                job.OutputFiles = outputs;
                job.FinishedAt = DateTime.Now;
                await _repo.FinishGenerationAsync(job.GenerationId, "succeeded", exit, outputs, Tail(sb));
                await _repo.SetGenStatusAsync(project.Id, "succeeded", markGenerated: true);
            }
            else
            {
                await FailAsync(project, job, exit, $"claude 退出码 {exit}");
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "方案生成失败 project={Project}", project.ProjectName);
            AppendLog(sb, "[异常] " + ex.Message);
            await FailAsync(project, job, null, ex.Message);
        }
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

    private static Dictionary<string, DateTime> SnapshotHtml(string dir)
    {
        var map = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        if (!Directory.Exists(dir)) return map;
        foreach (var f in Directory.EnumerateFiles(dir, "*.html"))
            map[Path.GetFileName(f)] = File.GetLastWriteTimeUtc(f);
        return map;
    }

    private static List<string> NewOrChangedHtml(string dir, Dictionary<string, DateTime> before)
    {
        var outFiles = new List<string>();
        if (!Directory.Exists(dir)) return outFiles;
        foreach (var f in Directory.EnumerateFiles(dir, "*.html"))
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
