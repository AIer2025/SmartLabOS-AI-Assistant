using Presales.Proposal.AI.Agent.Configuration;
using Presales.Proposal.AI.Agent.Data;
using Presales.Proposal.AI.Agent.Domain;

namespace Presales.Proposal.AI.Agent.Agent;

/// <summary>
/// 后台生成工作者（BackgroundService）。消费 GenerationQueue，按并发上限执行「模块选定」/「方案生成」，
/// 并把状态同时写入内存登记表(供轮询)与数据库(供重启后回退展示)。每个任务带独立超时。
/// </summary>
public sealed class GenerationWorker : BackgroundService
{
    private readonly GenerationQueue _queue;
    private readonly JobRegistry _registry;
    private readonly PresalesRepository _repo;
    private readonly LlmAgentService _agent;
    private readonly AgentSettings _settings;
    private readonly ILogger<GenerationWorker> _log;

    public GenerationWorker(GenerationQueue queue, JobRegistry registry, PresalesRepository repo,
        LlmAgentService agent, AgentSettings settings, ILogger<GenerationWorker> log)
    {
        _queue = queue; _registry = registry; _repo = repo; _agent = agent; _settings = settings; _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stopping)
    {
        var sem = new SemaphoreSlim(Math.Max(1, _settings.MaxConcurrency));
        await foreach (var item in _queue.ReadAllAsync(stopping))
        {
            await sem.WaitAsync(stopping);
            _ = Task.Run(async () =>
            {
                try { await HandleAsync(item, stopping); }
                catch (Exception ex) { _log.LogError(ex, "后台任务异常 kind={Kind} project={Id}", item.Kind, item.ProjectId); }
                finally { sem.Release(); }
            }, stopping);
        }
    }

    private async Task HandleAsync(PresalesWorkItem item, CancellationToken stopping)
    {
        var project = await _repo.GetAsync(item.ProjectId);
        if (project is null) return;

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(stopping);
        cts.CancelAfter(TimeSpan.FromMinutes(Math.Max(1, _settings.TimeoutMinutes)));
        var ct = cts.Token;

        if (item.Kind == WorkKind.SelectModules) await RunSelectAsync(project, ct);
        else await RunGenerateAsync(project, ct);
    }

    private async Task RunSelectAsync(PresalesProject project, CancellationToken ct)
    {
        var job = _registry.GetSelect(project.Id) ?? new ModuleSelectJob { ProjectId = project.Id, StartedAt = DateTime.Now };
        _registry.Selections[project.Id] = job;
        job.RunLog = RunLogFile.Create(project.ProjectDir, "模块选定", project.ProjectName, RunHeader());
        job.Status = "running";
        try
        {
            var pick = await _agent.SelectModulesAsync(project, job.Log, ct);
            await _repo.SetModulesAsync(project.Id, pick.Modules, confirmed: false);
            job.Recommended = pick.Modules;
            job.Status = "succeeded";
        }
        catch (OperationCanceledException) { Fail(job, "已取消或超时"); }
        catch (Exception ex) { _log.LogError(ex, "模块选定失败 {Project}", project.ProjectName); Fail(job, ex.Message); }
        finally
        {
            job.FinishedAt = DateTime.Now;
            job.RunLog?.Finish(job.Status, job.Error);
        }
    }

    /// <summary>
    /// 写进日志文件头部的运行参数，便于事后核对这次用的是什么供应商/模型/档位。
    /// 供应商必须记进去：同一批提案可能横跨切换前后，事后对比质量时这是唯一的区分依据。
    /// </summary>
    private string RunHeader() =>
        $"供应商={_settings.Provider}  模型={_settings.Model}  努力档位={_settings.Effort}  "
      + $"思考={_settings.Thinking}  MaxTokens={_settings.MaxTokens}  "
      + $"缓存={(_settings.IsDeepSeek ? "自动" : $"TTL={_settings.CacheTtl}")}  最大尝试={_settings.MaxAttempts}  "
      + $"工具轮上限={_settings.MaxToolIterations}  超时={_settings.TimeoutMinutes}分钟";

    private async Task RunGenerateAsync(PresalesProject project, CancellationToken ct)
    {
        var job = _registry.GetGen(project.Id) ?? new GenJob { ProjectId = project.Id, StartedAt = DateTime.Now };
        _registry.Generations[project.Id] = job;
        job.RunLog = RunLogFile.Create(project.ProjectDir, "方案生成", project.ProjectName, RunHeader());
        job.Status = "running";

        long genId = 0;
        try
        {
            genId = await _repo.CreateGenerationAsync(new PresalesGeneration
            {
                ProjectId = project.Id, ProjectName = project.ProjectName,
                CommandFile = job.CommandFile, Status = "running",
            });
            job.GenerationId = genId;
            await _repo.SetGenStatusAsync(project.Id, "running", job.CommandFile);

            var outputs = await _agent.GenerateProposalAsync(project, ph => job.Phase = ph, job.Log, ct);

            job.OutputFiles = WithRunLog(outputs, job);
            job.ExitCode = 0;
            job.Status = "succeeded";
            await _repo.FinishGenerationAsync(genId, "succeeded", 0, outputs, job.LogTail(20000));
            await _repo.SetGenStatusAsync(project.Id, "succeeded", markGenerated: true);
        }
        catch (OperationCanceledException)
        {
            await FailGenAsync(project, job, genId, "已取消或超时");
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "方案生成失败 {Project}", project.ProjectName);
            await FailGenAsync(project, job, genId, ex.Message);
        }
        finally
        {
            job.FinishedAt = DateTime.Now;
            job.RunLog?.Finish(job.Status, job.Error);
        }
    }

    /// <summary>把本次运行日志文件一并列入产物，前端可直接点开查看/下载。</summary>
    private static List<string> WithRunLog(List<string> outputs, GenJob job)
    {
        var list = new List<string>(outputs);
        if (job.RunLog?.FileName is { } name && !list.Contains(name, StringComparer.OrdinalIgnoreCase))
            list.Add(name);
        return list;
    }

    private static void Fail(ModuleSelectJob job, string message)
    {
        job.Status = "failed"; job.Error = message; job.Log("[失败] " + message);
    }

    private async Task FailGenAsync(PresalesProject project, GenJob job, long genId, string message)
    {
        job.Status = "failed"; job.Error = message; job.Log("[失败] " + message);
        job.OutputFiles = WithRunLog(job.OutputFiles, job);   // 失败时更需要能拿到日志
        if (genId > 0)
            await _repo.FinishGenerationAsync(genId, "failed", null, job.OutputFiles, job.LogTail(20000));
        await _repo.SetGenStatusAsync(project.Id, "failed");
    }
}
