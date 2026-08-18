using System.Text;

namespace Presales.Proposal.AI.Agent.Agent;

/// <summary>
/// 把一次任务的运行日志实时落盘到项目输出目录。
///
/// 内存日志（JobRegistry）只保留末尾片段且进程一停即失，数据库也只在「方案生成」结束时
/// 存最后 2 万字符——排查流式拼装、工具调用、截断续写这类问题需要完整记录，故单独落一份文件：
///   &lt;项目输出目录&gt;\运行日志-模块选定-yyyyMMdd-HHmmss.log
///   &lt;项目输出目录&gt;\运行日志-方案生成-yyyyMMdd-HHmmss.log
///
/// 写日志绝不能把任务本身弄挂：所有 IO 异常一律吞掉，只记一次失败原因。
/// </summary>
public sealed class RunLogFile
{
    private readonly object _gate = new();
    private readonly string? _fullPath;
    private bool _broken;

    /// <summary>日志文件名（纯文件名，便于前端下载）。创建失败时为 null。</summary>
    public string? FileName { get; }

    private RunLogFile(string fullPath, string fileName) { _fullPath = fullPath; FileName = fileName; }

    private RunLogFile() { }   // 创建失败时的空实现：所有写入静默丢弃

    /// <summary>在项目输出目录下创建一份运行日志。任何异常都退化为「不写文件」，不影响任务。</summary>
    public static RunLogFile Create(string? projectDir, string kind, string projectName, string header)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(projectDir)) return new RunLogFile();
            Directory.CreateDirectory(projectDir);

            var name = $"运行日志-{kind}-{DateTime.Now:yyyyMMdd-HHmmss}.log";
            var full = Path.Combine(projectDir, name);
            var log = new RunLogFile(full, name);

            log.WriteRaw($"# SmartLabOS 售前 {kind}运行日志");
            log.WriteRaw($"# 项目：{projectName}");
            log.WriteRaw($"# 开始：{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            log.WriteRaw($"# {header}");
            log.WriteRaw(new string('-', 72));
            return log;
        }
        catch { return new RunLogFile(); }
    }

    /// <summary>追加一行（自动带时间戳）。</summary>
    public void Write(string? line)
    {
        if (line is null) return;
        WriteRaw($"[{DateTime.Now:HH:mm:ss}] {line}");
    }

    /// <summary>任务收尾：写入结束状态，便于事后一眼看出成败。</summary>
    public void Finish(string status, string? error)
    {
        WriteRaw(new string('-', 72));
        WriteRaw($"# 结束：{DateTime.Now:yyyy-MM-dd HH:mm:ss}  状态：{status}");
        if (!string.IsNullOrWhiteSpace(error)) WriteRaw($"# 错误：{error}");
    }

    private void WriteRaw(string text)
    {
        if (_fullPath is null || _broken) return;
        try
        {
            lock (_gate) File.AppendAllText(_fullPath, text + Environment.NewLine, new UTF8Encoding(true));
        }
        catch
        {
            _broken = true;   // 磁盘满/被占用等：停止后续尝试，任务照常跑
        }
    }
}
