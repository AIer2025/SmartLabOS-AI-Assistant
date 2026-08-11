using System.Collections.Concurrent;
using System.Text;

namespace Presales.Proposal.AI.Agent.Agent;

/// <summary>后台工作项类型。</summary>
public enum WorkKind { SelectModules, GenerateProposal }

/// <summary>入队的一条后台工作（仅携带项目ID与类型；由 worker 从库中取最新项目信息执行）。</summary>
public sealed record PresalesWorkItem(WorkKind Kind, long ProjectId);

/// <summary>单个项目最近一次「方案生成」任务的实时状态（内存态，供前端轮询）。</summary>
public sealed class GenJob
{
    public long ProjectId { get; set; }
    public long GenerationId { get; set; }
    public string CommandFile { get; set; } = "";
    public string Status { get; set; } = "queued";     // queued/running/succeeded/failed
    /// <summary>0=准备中, 1=生成 HTML/URS, 2=生成详细设计方案。</summary>
    public int Phase { get; set; }
    public int? ExitCode { get; set; }
    public string? Error { get; set; }
    public List<string> OutputFiles { get; set; } = new();
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public StringBuilder LogBuilder { get; } = new();
    /// <summary>落盘的完整运行日志（内存日志会截断，文件不会）。</summary>
    public RunLogFile? RunLog { get; set; }

    public void Log(string? line)
    {
        if (line is null) return;
        lock (LogBuilder) { if (LogBuilder.Length < 200_000) LogBuilder.AppendLine($"{DateTime.Now:HH:mm:ss} {line}"); }
        RunLog?.Write(line);
    }
    public string LogTail(int n = 6000)
    {
        lock (LogBuilder) { var s = LogBuilder.ToString(); return s.Length <= n ? s : s[^n..]; }
    }
}

/// <summary>单个项目最近一次「模块选定」推理任务的实时状态（内存态）。</summary>
public sealed class ModuleSelectJob
{
    public long ProjectId { get; set; }
    public string Status { get; set; } = "queued";     // queued/running/succeeded/failed
    public string? Error { get; set; }
    public List<string> Recommended { get; set; } = new();
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public StringBuilder LogBuilder { get; } = new();
    /// <summary>落盘的完整运行日志（内存日志会截断，文件不会）。</summary>
    public RunLogFile? RunLog { get; set; }

    public void Log(string? line)
    {
        if (line is null) return;
        lock (LogBuilder) { if (LogBuilder.Length < 100_000) LogBuilder.AppendLine($"{DateTime.Now:HH:mm:ss} {line}"); }
        RunLog?.Write(line);
    }
    public string LogTail(int n = 6000)
    {
        lock (LogBuilder) { var s = LogBuilder.ToString(); return s.Length <= n ? s : s[^n..]; }
    }
}

/// <summary>进程内任务状态登记表（单例）。控制器据此判重与轮询；worker 据此更新。</summary>
public sealed class JobRegistry
{
    public ConcurrentDictionary<long, GenJob> Generations { get; } = new();
    public ConcurrentDictionary<long, ModuleSelectJob> Selections { get; } = new();

    public GenJob? GetGen(long id) => Generations.TryGetValue(id, out var j) ? j : null;
    public ModuleSelectJob? GetSelect(long id) => Selections.TryGetValue(id, out var j) ? j : null;

    public bool IsGenerating(long id) =>
        Generations.TryGetValue(id, out var j) && j.Status is "queued" or "running";
    public bool IsSelecting(long id) =>
        Selections.TryGetValue(id, out var j) && j.Status is "queued" or "running";
}
