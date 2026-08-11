namespace SmartLabOS.DataMaintenance.Api.Data;

/// <summary>列的呈现/校验种类。</summary>
public enum ColumnKind
{
    Key,        // 主键 (id)
    Text,       // 单行文本
    LongText,   // 多行文本 (Markdown / YAML)
    Json,       // JSON 文本
    ReadOnly    // 只读 (时间戳)
}

public sealed class ColumnDef
{
    public required string Name { get; init; }
    public required string Label { get; init; }
    public ColumnKind Kind { get; init; } = ColumnKind.Text;
    public bool InList { get; init; }       // 是否出现在列表视图
    public bool Editable { get; init; } = true;
    public bool Required { get; init; }
}

public sealed class EntityDef
{
    public required string Key { get; init; }       // 路由键, 如 "modules"
    public required string Table { get; init; }      // 数据库表名 (含连字符)
    public required string Label { get; init; }      // 中文显示名
    public required IReadOnlyList<ColumnDef> Columns { get; init; }

    public IEnumerable<string> ColumnNames => Columns.Select(c => c.Name);
    public IEnumerable<ColumnDef> EditableColumns => Columns.Where(c => c.Editable && c.Kind != ColumnKind.ReadOnly);
    public ColumnDef KeyColumn => Columns.First(c => c.Kind == ColumnKind.Key);
}

/// <summary>
/// 五张主数据表的元数据。CRUD 控制器、仓储与前端均由此驱动，
/// 新增字段只需在此登记，无需改动其它代码。
/// </summary>
public static class EntityRegistry
{
    private static readonly ColumnDef[] TailCommon =
    {
        new() { Name = "tags",            Label = "标签",        Kind = ColumnKind.Text,     InList = true },
        new() { Name = "data_json",       Label = "结构化数据(JSON)", Kind = ColumnKind.Json },
        new() { Name = "raw_frontmatter", Label = "原始 YAML",   Kind = ColumnKind.LongText },
        new() { Name = "body_markdown",   Label = "Markdown 正文", Kind = ColumnKind.LongText },
        new() { Name = "source_file",     Label = "来源文件",    Kind = ColumnKind.Text },
        new() { Name = "created_at",      Label = "创建时间",    Kind = ColumnKind.ReadOnly, InList = false, Editable = false },
        new() { Name = "updated_at",      Label = "更新时间",    Kind = ColumnKind.ReadOnly, InList = true,  Editable = false },
    };

    private static ColumnDef[] Build(params ColumnDef[] head) => head.Concat(TailCommon).ToArray();

    private static readonly ColumnDef IdCol =
        new() { Name = "id", Label = "ID", Kind = ColumnKind.Key, InList = true, Required = true };
    private static readonly ColumnDef NameCol =
        new() { Name = "name", Label = "名称", Kind = ColumnKind.Text, InList = true };
    private static ColumnDef Txt(string n, string l, bool inList = false) =>
        new() { Name = n, Label = l, Kind = ColumnKind.Text, InList = inList };

    public static readonly IReadOnlyList<EntityDef> All = new List<EntityDef>
    {
        new()
        {
            Key = "modules", Table = "SmartLabOS-Module", Label = "模块 Module",
            Columns = Build(IdCol, NameCol,
                Txt("module_part_no", "物料号"),
                Txt("category", "分类", inList: true),
                Txt("version", "版本"),
                Txt("status", "状态", inList: true),
                Txt("owner", "负责人", inList: true),
                Txt("last_updated", "最后更新")),
        },
        new()
        {
            Key = "platforms", Table = "SmartLabOS-PlatformBase", Label = "平台基类 PlatformBase",
            Columns = Build(IdCol, NameCol,
                Txt("platform_part_no", "物料号"),
                Txt("version", "版本"),
                Txt("status", "状态", inList: true),
                Txt("owner", "负责人", inList: true),
                Txt("last_updated", "最后更新")),
        },
        new()
        {
            Key = "workstations", Table = "SmartLabOS-WorkStation", Label = "工作站 WorkStation",
            Columns = Build(IdCol, NameCol,
                Txt("platform_id", "平台ID", inList: true),
                Txt("maturity", "成熟度", inList: true),
                Txt("version", "版本"),
                Txt("status", "状态", inList: true),
                Txt("owner", "负责人"),
                Txt("last_updated", "最后更新")),
        },
        new()
        {
            Key = "solutions", Table = "SmartLabOS-Solution", Label = "解决方案 Solution",
            Columns = Build(IdCol, NameCol,
                Txt("proposal_type", "提案类型", inList: true),
                Txt("maturity", "成熟度", inList: true),
                Txt("version", "版本"),
                Txt("status", "状态", inList: true),
                Txt("owner", "负责人"),
                Txt("last_updated", "最后更新")),
        },
        new()
        {
            Key = "projects", Table = "SmartLabOS-Project", Label = "项目 Project",
            Columns = Build(IdCol, NameCol,
                Txt("customer", "客户", inList: true),
                Txt("proposal_type", "提案类型"),
                Txt("stage", "阶段", inList: true),
                Txt("version", "版本"),
                Txt("status", "状态", inList: true),
                Txt("owner", "负责人"),
                Txt("last_updated", "最后更新")),
        },
    };

    public static EntityDef? Find(string key) =>
        All.FirstOrDefault(e => string.Equals(e.Key, key, StringComparison.OrdinalIgnoreCase));
}
