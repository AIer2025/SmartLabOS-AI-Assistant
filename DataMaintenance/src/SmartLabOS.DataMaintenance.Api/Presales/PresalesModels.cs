namespace SmartLabOS.DataMaintenance.Api.Presales;

/// <summary>
/// 售前项目需求信息。多值字段(protocols/challenges/expectations/processScope)在数据库中
/// 以 JSON 数组保存，这里直接以 .NET 列表承载，由仓储负责序列化/反序列化。
/// </summary>
public sealed class PresalesProject
{
    public long Id { get; set; }
    public string ProjectName { get; set; } = "";
    public string? ProjectDir { get; set; }

    /// <summary>6.2.1 客户项目现状(≤1500字)。</summary>
    public string? CurrentStatus { get; set; }
    /// <summary>6.2.2 已选流程标准(potocol/MD 文件名)。</summary>
    public List<string> Protocols { get; set; } = new();
    /// <summary>6.2.3 客户挑战(≤20项, 每项≤200字)。</summary>
    public List<string> Challenges { get; set; } = new();
    /// <summary>6.2.4 客户期望(≤20项, 每项≤200字)。</summary>
    public List<string> Expectations { get; set; } = new();
    /// <summary>6.2.5 流程范围(采样/制样/分样/前处理/检测/出具报告)。</summary>
    public List<string> ProcessScope { get; set; } = new();
    /// <summary>6.2.5 上下料方式: 人工上下料 / 自动上下料。</summary>
    public string? LoadingMethod { get; set; }
    /// <summary>6.2.6 软件功能: 设备操作软件 / 智慧实验室软件。</summary>
    public string? SoftwareType { get; set; }

    public string GenStatus { get; set; } = "draft";
    public string? LastCommandFile { get; set; }
    public string? LastGeneratedAt { get; set; }
    public string? CreatedAt { get; set; }
    public string? UpdatedAt { get; set; }
}

/// <summary>一次方案生成执行记录。</summary>
public sealed class PresalesGeneration
{
    public long Id { get; set; }
    public long ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string? CommandFile { get; set; }
    public string Status { get; set; } = "queued";
    public int? ExitCode { get; set; }
    public List<string> OutputFiles { get; set; } = new();
    public string? LogExcerpt { get; set; }
    public string? StartedAt { get; set; }
    public string? FinishedAt { get; set; }
    public string? CreatedAt { get; set; }
}

/// <summary>合法取值集合（前后端一致的硬约束）。</summary>
public static class PresalesOptions
{
    public static readonly string[] ProcessScope =
        { "采样", "制样", "分样", "前处理", "检测", "出具报告" };
    public static readonly string[] LoadingMethods =
        { "人工上下料", "自动上下料" };
    public static readonly string[] SoftwareTypes =
        { "设备操作软件", "智慧实验室软件" };

    public const int MaxStatusLen = 1500;
    public const int MaxItems = 20;
    public const int MaxItemLen = 200;
}
