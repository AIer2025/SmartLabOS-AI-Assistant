using Microsoft.AspNetCore.Mvc;
using Presales.Proposal.AI.Agent.Agent;
using Presales.Proposal.AI.Agent.Configuration;
using Presales.Proposal.AI.Agent.Data;
using Presales.Proposal.AI.Agent.Domain;

namespace Presales.Proposal.AI.Agent.Controllers;

/// <summary>
/// 「SmartLabOS 售前提案 AI Agent」接口。功能与被改造的 DataMaintenance 保持一致的前后端契约，
/// 但内部由「官方 Anthropic SDK + 后台队列/工作者」实现，取代「Claude Code CLI 子进程」。
/// </summary>
[ApiController]
[Route("api/presales")]
public sealed class PresalesController : ControllerBase
{
    private readonly PresalesRepository _repo;
    private readonly PresalesPaths _paths;
    private readonly ModuleCatalog _catalog;
    private readonly ClaudeAgentService _agent;
    private readonly GenerationQueue _queue;
    private readonly JobRegistry _jobs;
    private readonly AgentEnvFile _envFile;
    private readonly ProposalTemplateSet _templateSet;

    public PresalesController(PresalesRepository repo, PresalesPaths paths, ModuleCatalog catalog,
        ClaudeAgentService agent, GenerationQueue queue, JobRegistry jobs, AgentEnvFile envFile,
        ProposalTemplateSet templateSet)
    {
        _repo = repo; _paths = paths; _catalog = catalog; _agent = agent; _queue = queue; _jobs = jobs;
        _envFile = envFile; _templateSet = templateSet;
    }

    // ----------------------- 配置自检 -----------------------

    /// <summary>
    /// 回报 env/Presales-AI-Agent-config.json 的实际生效情况：读到哪个文件、各目录是否存在、
    /// MySQL 目标与连通性。部署现场用它一眼确认「改的那份配置是不是程序真正读到的那份」。
    /// 不返回任何口令或连接串。
    /// </summary>
    [HttpGet("config")]
    public async Task<IActionResult> Config()
    {
        static object Dir(string path) => new { path, exists = Directory.Exists(path) };

        return Ok(new
        {
            configFile = _envFile.Location,
            loaded = _envFile.Loaded,
            error = _envFile.Error,
            warnings = _envFile.Warnings,
            database = new { target = _envFile.DbSummary, connected = await _repo.PingAsync() },
            paths = new
            {
                projectRoot = Dir(_paths.ProjectRoot),
                modules = Dir(_paths.ModulesDir),
                platforms = Dir(_paths.PlatformsDir),
                pallet = Dir(_paths.PalletDir),
                protocol = Dir(_paths.ProtocolDir),
                projects = Dir(_paths.ProjectsDir),
                proposalTemplate = Dir(_paths.ProposalTemplateDir),
                templates = Dir(_paths.TemplatesDir),
            },
            // 模版目录中已归位的样例：角色 → 文件名。缺哪一类在这里一眼可见。
            proposalTemplates = _templateSet.All().Select(t => new { role = t.Role, file = t.FileName, label = t.Label }),
            // 已废弃、被 FileTools 隐去且拒绝读取的知识库目录
            deprecatedDirs = _paths.DeprecatedDirs,
            // Template_Path 下的标准/兜底文件。detailStandardMd 是**活引用**（每次生成都内联），
            // 它 exists=false 意味着 ★必备项校验失效，需要立刻处理。
            standardTemplates = new
            {
                detailStandardMd = new { path = _paths.DetailTemplateFile, exists = System.IO.File.Exists(_paths.DetailTemplateFile), inUse = "always" },
                proposalHtmlFallback = new { path = _paths.TemplateFile, exists = System.IO.File.Exists(_paths.TemplateFile), inUse = "fallback" },
                referenceDocxFallback = new { path = _paths.ReferenceDocxFile, exists = System.IO.File.Exists(_paths.ReferenceDocxFile), inUse = "fallback" },
            },
            moduleCount = _catalog.All().Count,
            agentReady = _agent.HasApiKey,
        });
    }

    // ----------------------- 选项 -----------------------

    [HttpGet("options")]
    public IActionResult Options()
    {
        var protocols = new List<string>();
        if (Directory.Exists(_paths.ProtocolDir))
            protocols = Directory.EnumerateFiles(_paths.ProtocolDir, "*.md")
                                 .Select(Path.GetFileName).Where(n => n is not null).Select(n => n!)
                                 .OrderBy(n => n, StringComparer.Ordinal).ToList();
        return Ok(new
        {
            protocols,
            processScope = PresalesOptions.ProcessScope,
            loadingMethods = PresalesOptions.LoadingMethods,
            softwareTypes = PresalesOptions.SoftwareTypes,
            agentReady = _agent.HasApiKey,
            limits = new { maxStatusLen = PresalesOptions.MaxStatusLen, maxItems = PresalesOptions.MaxItems, maxItemLen = PresalesOptions.MaxItemLen },
        });
    }

    // ----------------------- 项目 CRUD -----------------------

    public sealed record CreateProjectRequest(string? ProjectName);

    [HttpGet("projects")]
    public async Task<IActionResult> List() => Ok(new { items = (await _repo.ListAsync()).Select(ToDto) });

    [HttpGet("projects/{id:long}")]
    public async Task<IActionResult> Get(long id)
    {
        var p = await _repo.GetAsync(id);
        return p is null ? NotFound(new { message = $"项目不存在: {id}" }) : Ok(ToDto(p));
    }

    [HttpPost("projects")]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequest req)
    {
        var name = (req.ProjectName ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name)) return BadRequest(new { message = "请填写项目名称" });
        if (NameError(name) is { } nameErr) return BadRequest(new { message = nameErr });
        if (await _repo.NameExistsAsync(name)) return Conflict(new { message = $"项目名称已存在: {name}" });

        var dir = _paths.ProjectDir(name);
        try { Directory.CreateDirectory(dir); }
        catch (Exception ex) { return BadRequest(new { message = $"无法创建项目目录: {ex.Message}" }); }

        var p = new PresalesProject { ProjectName = name, ProjectDir = Path.GetFullPath(dir) };
        p.Id = await _repo.CreateAsync(p);
        var created = await _repo.GetAsync(p.Id);
        return CreatedAtAction(nameof(Get), new { id = p.Id }, ToDto(created!));
    }

    public sealed record UpdateProjectRequest(
        string? CurrentStatus, List<string>? Protocols, List<string>? Challenges,
        List<string>? Expectations, List<string>? ProcessScope, string? LoadingMethod, string? SoftwareType);

    [HttpPut("projects/{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateProjectRequest req)
    {
        var p = await _repo.GetAsync(id);
        if (p is null) return NotFound(new { message = $"项目不存在: {id}" });
        if (ValidateRequirement(req) is { } err) return BadRequest(new { message = err });

        p.CurrentStatus = req.CurrentStatus?.Trim();
        p.Protocols = Clean(req.Protocols);
        p.Challenges = Clean(req.Challenges);
        p.Expectations = Clean(req.Expectations);
        p.ProcessScope = Clean(req.ProcessScope);
        p.LoadingMethod = req.LoadingMethod;
        p.SoftwareType = req.SoftwareType;

        await _repo.UpdateAsync(p);
        return Ok(ToDto((await _repo.GetAsync(id))!));
    }

    [HttpDelete("projects/{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var ok = await _repo.DeleteAsync(id);
        return ok ? NoContent() : NotFound(new { message = $"项目不存在: {id}" });
    }

    // ----------------------- 模块选定 / 确认 -----------------------

    [HttpGet("modules")]
    public IActionResult Modules() =>
        Ok(new { items = _catalog.All().Select(m => new { id = m.Id, name = m.Name, category = m.Category }) });

    [HttpGet("projects/{id:long}/modules")]
    public async Task<IActionResult> GetModules(long id)
    {
        var p = await _repo.GetAsync(id);
        return p is null ? NotFound(new { message = $"项目不存在: {id}" }) : Ok(BuildModulesDto(p));
    }

    [HttpPost("projects/{id:long}/modules/select")]
    public async Task<IActionResult> SelectModules(long id)
    {
        var p = await _repo.GetAsync(id);
        if (p is null) return NotFound(new { message = $"项目不存在: {id}" });
        if (p.Protocols.Count == 0) return BadRequest(new { message = "请先在需求信息中至少选择一个流程标准，再进行模块选定" });
        if (!_agent.HasApiKey) return BadRequest(new { message = "未配置 Anthropic API Key，无法调用大模型。请设置环境变量 ANTHROPIC_API_KEY。" });
        if (_jobs.IsSelecting(id)) return Conflict(new { message = "该项目正在进行模块选定，请稍候" });

        EnsureDir(p);
        _jobs.Selections[id] = new ModuleSelectJob { ProjectId = id, Status = "queued", StartedAt = DateTime.Now };
        await _queue.EnqueueAsync(new PresalesWorkItem(WorkKind.SelectModules, id));
        return Accepted(new { message = "已开始模块选定（自动推理）", status = "running" });
    }

    [HttpGet("projects/{id:long}/modules/select/status")]
    public async Task<IActionResult> SelectModulesStatus(long id)
    {
        var p = await _repo.GetAsync(id);
        if (p is null) return NotFound(new { message = $"项目不存在: {id}" });

        var job = _jobs.GetSelect(id);
        if (job is null)
            return Ok(new
            {
                status = p.Modules.Count > 0 ? "succeeded" : "idle",
                running = false,
                recommended = Decorate(p.Modules),
                modulesConfirmed = p.ModulesConfirmed,
                log = (string?)null,
            });

        return Ok(new
        {
            status = job.Status,
            running = job.Status is "queued" or "running",
            recommended = Decorate(job.Recommended),
            error = job.Error,
            elapsedSeconds = ElapsedSeconds(job.StartedAt, job.FinishedAt),
            startedAt = job.StartedAt.ToString("yyyy-MM-dd HH:mm:ss"),
            finishedAt = job.FinishedAt?.ToString("yyyy-MM-dd HH:mm:ss"),
            log = job.LogTail(),
        });
    }

    public sealed record ConfirmModulesRequest(List<string>? Modules);

    [HttpPost("projects/{id:long}/modules/confirm")]
    public async Task<IActionResult> ConfirmModules(long id, [FromBody] ConfirmModulesRequest req)
    {
        var p = await _repo.GetAsync(id);
        if (p is null) return NotFound(new { message = $"项目不存在: {id}" });

        var modules = _catalog.Normalize(req.Modules);
        if (modules.Count == 0) return BadRequest(new { message = "请至少选定一个模块后再确认" });

        await _repo.SetModulesAsync(id, modules, confirmed: true);
        return Ok(BuildModulesDto((await _repo.GetAsync(id))!));
    }

    private object BuildModulesDto(PresalesProject p) => new { modules = Decorate(p.Modules), modulesConfirmed = p.ModulesConfirmed };

    private List<object> Decorate(IEnumerable<string> ids) => ids.Select(id =>
    {
        var info = _catalog.Find(id);
        return (object)new { id, name = info?.Name ?? "", category = info?.Category ?? "" };
    }).ToList();

    // ----------------------- 方案生成 -----------------------

    [HttpGet("projects/{id:long}/command-preview")]
    public async Task<IActionResult> CommandPreview(long id)
    {
        var p = await _repo.GetAsync(id);
        if (p is null) return NotFound(new { message = $"项目不存在: {id}" });
        EnsureDir(p);
        return Ok(new { command = _agent.BuildPreview(p) });
    }

    [HttpPost("projects/{id:long}/generate")]
    public async Task<IActionResult> Generate(long id)
    {
        var p = await _repo.GetAsync(id);
        if (p is null) return NotFound(new { message = $"项目不存在: {id}" });
        if (p.Protocols.Count == 0) return BadRequest(new { message = "请至少选择一个流程标准后再生成方案" });
        if (!p.ModulesConfirmed || p.Modules.Count == 0) return BadRequest(new { message = "请先完成「模块选定 → 模块确认」后再生成方案" });
        if (!_agent.HasApiKey) return BadRequest(new { message = "未配置 Anthropic API Key，无法调用大模型。请设置环境变量 ANTHROPIC_API_KEY。" });
        if (_jobs.IsGenerating(id)) return Conflict(new { message = "该项目的方案正在生成中，请稍候" });

        EnsureDir(p);
        var commandFile = $"Anthropic-Messages-API-{DateTime.Now:yyyyMMdd-HHmmss}";
        _jobs.Generations[id] = new GenJob { ProjectId = id, CommandFile = commandFile, Status = "queued", StartedAt = DateTime.Now };
        await _queue.EnqueueAsync(new PresalesWorkItem(WorkKind.GenerateProposal, id));
        return Accepted(new { message = "已开始生成方案", commandFile, status = "running" });
    }

    [HttpGet("projects/{id:long}/generate/status")]
    public async Task<IActionResult> GenerateStatus(long id)
    {
        var p = await _repo.GetAsync(id);
        if (p is null) return NotFound(new { message = $"项目不存在: {id}" });

        var job = _jobs.GetGen(id);
        if (job is null)
            return Ok(new
            {
                status = p.GenStatus, running = false,
                outputFiles = ListOutputs(p), log = (string?)null, lastGeneratedAt = p.LastGeneratedAt,
            });

        return Ok(new
        {
            status = job.Status,
            running = job.Status is "queued" or "running",
            exitCode = job.ExitCode,
            error = job.Error,
            outputFiles = ListOutputs(p),
            commandFile = job.CommandFile,
            phase = job.Phase,
            phaseCount = 2,
            phaseLabel = GenPhaseLabel(job.Phase),
            elapsedSeconds = ElapsedSeconds(job.StartedAt, job.FinishedAt),
            startedAt = job.StartedAt.ToString("yyyy-MM-dd HH:mm:ss"),
            finishedAt = job.FinishedAt?.ToString("yyyy-MM-dd HH:mm:ss"),
            log = job.LogTail(),
        });
    }

    [HttpGet("projects/{id:long}/files")]
    public async Task<IActionResult> Files(long id)
    {
        var p = await _repo.GetAsync(id);
        return p is null ? NotFound(new { message = $"项目不存在: {id}" }) : Ok(new { directory = p.ProjectDir, files = ListOutputs(p) });
    }

    [HttpGet("projects/{id:long}/file")]
    public async Task<IActionResult> File(long id, [FromQuery] string name)
    {
        var p = await _repo.GetAsync(id);
        if (p is null) return NotFound(new { message = $"项目不存在: {id}" });
        if (string.IsNullOrWhiteSpace(name) || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            return BadRequest(new { message = "文件名非法" });

        var dir = Path.GetFullPath(p.ProjectDir!);
        var full = Path.GetFullPath(Path.Combine(dir, name));
        if (!full.StartsWith(dir, StringComparison.OrdinalIgnoreCase) || !System.IO.File.Exists(full))
            return NotFound(new { message = $"文件不存在: {name}" });

        var ext = Path.GetExtension(full).ToLowerInvariant();
        if (ext == ".docx")
            return PhysicalFile(full, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", Path.GetFileName(full));
        var contentType = ext switch
        {
            ".html" or ".htm" => "text/html; charset=utf-8",
            ".md" or ".txt" or ".log" => "text/plain; charset=utf-8",
            _ => "application/octet-stream",
        };
        return PhysicalFile(full, contentType);
    }

    // ----------------------- helpers -----------------------

    private static int ElapsedSeconds(DateTime start, DateTime? end) =>
        (int)Math.Max(0, ((end ?? DateTime.Now) - start).TotalSeconds);

    private static string GenPhaseLabel(int phase) => phase switch
    {
        2 => "阶段 2/2：整合生成《详细设计方案》Markdown",
        1 => "阶段 1/2：生成解决方案 HTML 与 URS",
        _ => "准备中：组织上下文并调用 Claude…",
    };

    private void EnsureDir(PresalesProject p)
    {
        if (string.IsNullOrWhiteSpace(p.ProjectDir))
            p.ProjectDir = Path.GetFullPath(_paths.ProjectDir(p.ProjectName));
        Directory.CreateDirectory(p.ProjectDir);
    }

    private static readonly string[] OutputPatterns = { "*.html", "*.md", "*.docx" };

    private static List<object> ListOutputs(PresalesProject p)
    {
        var list = new List<object>();
        if (string.IsNullOrWhiteSpace(p.ProjectDir) || !Directory.Exists(p.ProjectDir)) return list;
        var files = OutputPatterns.SelectMany(pat => Directory.EnumerateFiles(p.ProjectDir!, pat))
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase);
        foreach (var f in files)
        {
            var fi = new FileInfo(f);
            list.Add(new
            {
                name = fi.Name, size = fi.Length,
                modified = fi.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                isUrs = fi.Name.Contains("URS", StringComparison.OrdinalIgnoreCase),
                isDocx = fi.Name.EndsWith(".docx", StringComparison.OrdinalIgnoreCase),
                isMd = fi.Name.EndsWith(".md", StringComparison.OrdinalIgnoreCase),
            });
        }
        return list;
    }

    private static List<string> Clean(List<string>? items) =>
        (items ?? new()).Select(s => (s ?? "").Trim()).Where(s => s.Length > 0).ToList();

    private static string? NameError(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        if (name.IndexOfAny(invalid) >= 0 || name.Contains('/') || name.Contains('\\'))
            return "项目名称包含非法字符（不能含 \\ / : * ? \" < > | 等）";
        if (name is "." or ".." || name.Length > 200) return "项目名称不合法或过长";
        return null;
    }

    private static string? ValidateRequirement(UpdateProjectRequest req)
    {
        if ((req.CurrentStatus?.Length ?? 0) > PresalesOptions.MaxStatusLen)
            return $"客户项目现状不能超过 {PresalesOptions.MaxStatusLen} 字";
        if (ItemError(req.Challenges, "挑战") is { } c) return c;
        if (ItemError(req.Expectations, "期望") is { } e) return e;
        if (req.ProcessScope is { } ps && ps.Any(s => !PresalesOptions.ProcessScope.Contains(s))) return "流程范围包含非法选项";
        if (!string.IsNullOrWhiteSpace(req.LoadingMethod) && !PresalesOptions.LoadingMethods.Contains(req.LoadingMethod)) return "上下料方式取值非法";
        if (!string.IsNullOrWhiteSpace(req.SoftwareType) && !PresalesOptions.SoftwareTypes.Contains(req.SoftwareType)) return "软件功能取值非法";
        return null;
    }

    private static string? ItemError(List<string>? items, string label)
    {
        if (items is null) return null;
        var nonEmpty = items.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        if (nonEmpty.Count > PresalesOptions.MaxItems) return $"{label}最多 {PresalesOptions.MaxItems} 条";
        if (nonEmpty.Any(s => s.Length > PresalesOptions.MaxItemLen)) return $"每条{label}不能超过 {PresalesOptions.MaxItemLen} 字";
        return null;
    }

    private object ToDto(PresalesProject p) => new
    {
        id = p.Id, projectName = p.ProjectName, projectDir = p.ProjectDir,
        currentStatus = p.CurrentStatus, protocols = p.Protocols, challenges = p.Challenges,
        expectations = p.Expectations, processScope = p.ProcessScope,
        loadingMethod = p.LoadingMethod, softwareType = p.SoftwareType,
        modules = Decorate(p.Modules), modulesConfirmed = p.ModulesConfirmed,
        genStatus = p.GenStatus, lastCommandFile = p.LastCommandFile, lastGeneratedAt = p.LastGeneratedAt,
        createdAt = p.CreatedAt, updatedAt = p.UpdatedAt,
    };

    // 提供一个健康检查端点（数据库 + Agent 就绪度）
    [HttpGet("/api/health")]
    public async Task<IActionResult> Health() =>
        Ok(new { db = await _repo.PingAsync() ? "connected" : "down", agent = _agent.HasApiKey ? "ready" : "no-api-key" });

    /// <summary>仅开发环境：用样例 Markdown 自检 docx 导出（无需 API Key，验证 Pandoc/OpenXML 链路）。</summary>
    [HttpGet("/api/dev/docx-selftest")]
    public IActionResult DocxSelfTest([FromServices] IHostEnvironment env, [FromServices] DocxExporter exporter)
    {
        if (!env.IsDevelopment()) return NotFound();
        var tmp = Path.Combine(Path.GetTempPath(), $"docx-selftest-{DateTime.Now:HHmmss}.md");
        System.IO.File.WriteAllText(tmp,
            "# 详细设计方案（自检）\n\n## 1. 概述\n\n这是一段**加粗**与*斜体*以及 `行内代码` 的示例。\n\n" +
            "- 要点一\n- 要点二\n\n## 2. 关键指标表\n\n| 指标 | 数值 | 单位 |\n|---|---|---|\n| 精度 | 0.1 | mg |\n| 通讯 | Modbus | - |\n\n" +
            "```\ncode block line\n```\n");
        var engine = exporter.Export(tmp);
        var docx = Path.ChangeExtension(tmp, ".docx");
        var ok = engine is not null && System.IO.File.Exists(docx);
        var size = ok ? new FileInfo(docx).Length : 0;
        try { System.IO.File.Delete(tmp); if (System.IO.File.Exists(docx)) System.IO.File.Delete(docx); } catch { }
        // styleSource 回报 WORD 版式取自哪份模版 docx——版式不对时先看这里
        return Ok(new { ok, engine, size, styleSource = exporter.StyleSourcePath });
    }
}
