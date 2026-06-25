using Microsoft.AspNetCore.Mvc;
using SmartLabOS.DataMaintenance.Api.Presales;
using SmartLabOS.DataMaintenance.Api.Services;

namespace SmartLabOS.DataMaintenance.Api.Controllers;

/// <summary>
/// 「SmartLabOS 售前方案自动生成」接口。
///   GET    /api/presales/options                       取选项(流程标准列表/流程范围/上下料/软件)
///   GET    /api/presales/projects                      项目列表
///   POST   /api/presales/projects                      新建项目(创建 projects/&lt;name&gt; 目录)
///   GET    /api/presales/projects/{id}                 取单个项目需求信息
///   PUT    /api/presales/projects/{id}                 保存/修订需求信息
///   DELETE /api/presales/projects/{id}                 删除项目记录
///   POST   /api/presales/projects/{id}/generate        触发方案生成(异步调用 Claude Code)
///   GET    /api/presales/projects/{id}/generate/status 轮询生成状态/日志
///   GET    /api/presales/projects/{id}/command-preview 预览将传给 Claude Code 的指令文档
///   GET    /api/presales/projects/{id}/files           列出已生成的 *.html 文件
/// </summary>
[ApiController]
[Route("api/presales")]
public sealed class PresalesController : ControllerBase
{
    private readonly PresalesRepository _repo;
    private readonly PresalesPaths _paths;
    private readonly ClaudeCodeRunner _runner;

    public PresalesController(PresalesRepository repo, PresalesPaths paths, ClaudeCodeRunner runner)
    {
        _repo = repo;
        _paths = paths;
        _runner = runner;
    }

    // ----------------------- 选项 -----------------------

    [HttpGet("options")]
    public IActionResult Options()
    {
        var protocols = new List<string>();
        if (Directory.Exists(_paths.ProtocolDir))
            protocols = Directory.EnumerateFiles(_paths.ProtocolDir, "*.md")
                                 .Select(Path.GetFileName)
                                 .Where(n => n is not null).Select(n => n!)
                                 .OrderBy(n => n, StringComparer.Ordinal).ToList();

        return Ok(new
        {
            protocols,
            processScope = PresalesOptions.ProcessScope,
            loadingMethods = PresalesOptions.LoadingMethods,
            softwareTypes = PresalesOptions.SoftwareTypes,
            limits = new
            {
                maxStatusLen = PresalesOptions.MaxStatusLen,
                maxItems = PresalesOptions.MaxItems,
                maxItemLen = PresalesOptions.MaxItemLen,
            }
        });
    }

    // ----------------------- 项目 CRUD -----------------------

    public sealed record CreateProjectRequest(string? ProjectName);

    [HttpGet("projects")]
    public async Task<IActionResult> List()
    {
        var items = await _repo.ListAsync();
        return Ok(new { items = items.Select(ToDto) });
    }

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
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest(new { message = "请填写项目名称" });
        if (NameError(name) is { } nameErr)
            return BadRequest(new { message = nameErr });
        if (await _repo.NameExistsAsync(name))
            return Conflict(new { message = $"项目名称已存在: {name}" });

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
        List<string>? Expectations, List<string>? ProcessScope,
        string? LoadingMethod, string? SoftwareType);

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
        var updated = await _repo.GetAsync(id);
        return Ok(ToDto(updated!));
    }

    [HttpDelete("projects/{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        // 仅删除数据库记录，保留 projects/&lt;name&gt; 目录及其中已生成的提案文件。
        var ok = await _repo.DeleteAsync(id);
        return ok ? NoContent() : NotFound(new { message = $"项目不存在: {id}" });
    }

    // ----------------------- 方案生成 -----------------------

    [HttpGet("projects/{id:long}/command-preview")]
    public async Task<IActionResult> CommandPreview(long id)
    {
        var p = await _repo.GetAsync(id);
        if (p is null) return NotFound(new { message = $"项目不存在: {id}" });
        EnsureDir(p);
        return Ok(new { command = _runner.BuildCommandDocument(p) });
    }

    [HttpPost("projects/{id:long}/generate")]
    public async Task<IActionResult> Generate(long id)
    {
        var p = await _repo.GetAsync(id);
        if (p is null) return NotFound(new { message = $"项目不存在: {id}" });
        if (p.Protocols.Count == 0)
            return BadRequest(new { message = "请至少选择一个流程标准后再生成方案" });
        if (_runner.IsRunning(id))
            return Conflict(new { message = "该项目的方案正在生成中，请稍候" });

        EnsureDir(p);
        var commandFile = await _runner.StartAsync(p);
        return Accepted(new { message = "已开始生成方案", commandFile, status = "running" });
    }

    [HttpGet("projects/{id:long}/generate/status")]
    public async Task<IActionResult> GenerateStatus(long id)
    {
        var p = await _repo.GetAsync(id);
        if (p is null) return NotFound(new { message = $"项目不存在: {id}" });

        var job = _runner.GetJob(id);
        if (job is null)
        {
            // 进程内无实时任务（如服务重启后）：回退到数据库中保存的状态
            return Ok(new
            {
                status = p.GenStatus,
                running = false,
                outputFiles = ListHtml(p),
                log = (string?)null,
                lastGeneratedAt = p.LastGeneratedAt,
            });
        }

        return Ok(new
        {
            status = job.Status,
            running = job.Status is "queued" or "running",
            exitCode = job.ExitCode,
            error = job.Error,
            outputFiles = ListHtml(p),
            commandFile = job.CommandFile,
            startedAt = job.StartedAt.ToString("yyyy-MM-dd HH:mm:ss"),
            finishedAt = job.FinishedAt?.ToString("yyyy-MM-dd HH:mm:ss"),
            log = job.LogTail(),
        });
    }

    [HttpGet("projects/{id:long}/files")]
    public async Task<IActionResult> Files(long id)
    {
        var p = await _repo.GetAsync(id);
        if (p is null) return NotFound(new { message = $"项目不存在: {id}" });
        return Ok(new { directory = p.ProjectDir, files = ListHtml(p) });
    }

    /// <summary>在浏览器中查看某个已生成的提案 HTML（仅限该项目目录下、防目录穿越）。</summary>
    [HttpGet("projects/{id:long}/file")]
    public async Task<IActionResult> File(long id, [FromQuery] string name)
    {
        var p = await _repo.GetAsync(id);
        if (p is null) return NotFound(new { message = $"项目不存在: {id}" });
        if (string.IsNullOrWhiteSpace(name) || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            return BadRequest(new { message = "文件名非法" });

        var dir = Path.GetFullPath(p.ProjectDir!);
        var full = Path.GetFullPath(Path.Combine(dir, name));
        // 防目录穿越：必须仍在项目目录内
        if (!full.StartsWith(dir, StringComparison.OrdinalIgnoreCase) || !System.IO.File.Exists(full))
            return NotFound(new { message = $"文件不存在: {name}" });

        var ext = Path.GetExtension(full).ToLowerInvariant();
        var contentType = ext switch
        {
            ".html" or ".htm" => "text/html; charset=utf-8",
            ".md" or ".txt" => "text/plain; charset=utf-8",
            _ => "application/octet-stream"
        };
        return PhysicalFile(full, contentType);
    }

    // ----------------------- helpers -----------------------

    private void EnsureDir(PresalesProject p)
    {
        if (string.IsNullOrWhiteSpace(p.ProjectDir))
            p.ProjectDir = Path.GetFullPath(_paths.ProjectDir(p.ProjectName));
        Directory.CreateDirectory(p.ProjectDir);
    }

    private static List<object> ListHtml(PresalesProject p)
    {
        var list = new List<object>();
        if (string.IsNullOrWhiteSpace(p.ProjectDir) || !Directory.Exists(p.ProjectDir)) return list;
        foreach (var f in Directory.EnumerateFiles(p.ProjectDir, "*.html")
                                   .OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
        {
            var fi = new FileInfo(f);
            list.Add(new
            {
                name = fi.Name,
                size = fi.Length,
                modified = fi.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                isUrs = fi.Name.Contains("URS", StringComparison.OrdinalIgnoreCase),
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
        if (name is "." or ".." || name.Length > 200)
            return "项目名称不合法或过长";
        return null;
    }

    private static string? ValidateRequirement(UpdateProjectRequest req)
    {
        if ((req.CurrentStatus?.Length ?? 0) > PresalesOptions.MaxStatusLen)
            return $"客户项目现状不能超过 {PresalesOptions.MaxStatusLen} 字";

        if (ItemError(req.Challenges, "挑战") is { } c) return c;
        if (ItemError(req.Expectations, "期望") is { } e) return e;

        if (req.ProcessScope is { } ps && ps.Any(s => !PresalesOptions.ProcessScope.Contains(s)))
            return "流程范围包含非法选项";
        if (!string.IsNullOrWhiteSpace(req.LoadingMethod) && !PresalesOptions.LoadingMethods.Contains(req.LoadingMethod))
            return "上下料方式取值非法";
        if (!string.IsNullOrWhiteSpace(req.SoftwareType) && !PresalesOptions.SoftwareTypes.Contains(req.SoftwareType))
            return "软件功能取值非法";
        return null;
    }

    private static string? ItemError(List<string>? items, string label)
    {
        if (items is null) return null;
        var nonEmpty = items.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        if (nonEmpty.Count > PresalesOptions.MaxItems)
            return $"{label}最多 {PresalesOptions.MaxItems} 条";
        if (nonEmpty.Any(s => s.Length > PresalesOptions.MaxItemLen))
            return $"每条{label}不能超过 {PresalesOptions.MaxItemLen} 字";
        return null;
    }

    private static object ToDto(PresalesProject p) => new
    {
        id = p.Id,
        projectName = p.ProjectName,
        projectDir = p.ProjectDir,
        currentStatus = p.CurrentStatus,
        protocols = p.Protocols,
        challenges = p.Challenges,
        expectations = p.Expectations,
        processScope = p.ProcessScope,
        loadingMethod = p.LoadingMethod,
        softwareType = p.SoftwareType,
        genStatus = p.GenStatus,
        lastCommandFile = p.LastCommandFile,
        lastGeneratedAt = p.LastGeneratedAt,
        createdAt = p.CreatedAt,
        updatedAt = p.UpdatedAt,
    };
}
