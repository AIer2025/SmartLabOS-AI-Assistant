using System.Threading.Channels;

namespace Presales.Proposal.AI.Agent.Agent;

/// <summary>
/// 有界后台作业队列（替代旧实现的 Task.Run 即发即忘）。控制器把工作入队即返回，
/// 由 GenerationWorker(BackgroundService) 消费；进程退出/回收时不再“凭空丢任务”，
/// 且可配合并发上限保护 API 限额。
/// </summary>
public sealed class GenerationQueue
{
    private readonly Channel<PresalesWorkItem> _channel =
        Channel.CreateBounded<PresalesWorkItem>(new BoundedChannelOptions(200)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false,
        });

    public ValueTask EnqueueAsync(PresalesWorkItem item, CancellationToken ct = default) =>
        _channel.Writer.WriteAsync(item, ct);

    public IAsyncEnumerable<PresalesWorkItem> ReadAllAsync(CancellationToken ct) =>
        _channel.Reader.ReadAllAsync(ct);
}
