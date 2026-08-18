namespace Presales.Proposal.AI.Agent.Domain;

/// <summary>
/// 售前项目需求信息。多值字段(protocols/challenges/expectations/processScope)在数据库中
/// 以 JSON 数组保存，这里直接以 .NET 列表承载，由仓储负责序列化/反序列化。
/// 与被改造的 DataMaintenance 保持同一张 MySQL 表结构，可无缝复用既有数据。
/// </summary>
public sealed class PresalesProject
{
    public long Id { get; set; }
    public string ProjectName { get; set; } = "";
    public string? ProjectDir { get; set; }

    /// <summary>客户项目现状(≤1500字)。</summary>
    public string? CurrentStatus { get; set; }
    /// <summary>已选流程标准(potocol/MD 文件名)。</summary>
    public List<string> Protocols { get; set; } = new();
    /// <summary>客户挑战(≤20项, 每项≤200字)。</summary>
    public List<string> Challenges { get; set; } = new();
    /// <summary>客户期望(≤20项, 每项≤200字)。</summary>
    public List<string> Expectations { get; set; } = new();
    /// <summary>流程范围(采样/制样/分样/前处理/检测/出具报告)。</summary>
    public List<string> ProcessScope { get; set; } = new();
    /// <summary>上下料方式: 人工上下料 / 自动上下料。</summary>
    public string? LoadingMethod { get; set; }
    /// <summary>软件功能: 设备操作软件 / 智慧实验室软件。</summary>
    public string? SoftwareType { get; set; }

    /// <summary>已选定/已确认的模块ID列表(如 MOD-CC-001)。在「模块选定→模块确认」流程中维护。</summary>
    public List<string> Modules { get; set; } = new();
    /// <summary>是否已点击「模块确认」。仅在已确认后才允许进入「方案生成」。</summary>
    public bool ModulesConfirmed { get; set; }

    public string GenStatus { get; set; } = "draft";
    public string? LastCommandFile { get; set; }
    public string? LastGeneratedAt { get; set; }
    public string? CreatedAt { get; set; }
    public string? UpdatedAt { get; set; }
}

/// <summary>一次方案生成执行记录（历史留痕，落库便于服务重启后恢复展示）。</summary>
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

/// <summary>「模块选定」结构化结果（由大模型按 JSON Schema 约束产出，机器可读）。</summary>
public sealed class ModulePick
{
    public List<string> Modules { get; set; } = new();
    public string? Notes { get; set; }
}
