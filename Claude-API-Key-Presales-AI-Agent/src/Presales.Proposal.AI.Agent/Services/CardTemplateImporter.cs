using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using SmartLabOS.DataMaintenance.Api.Data;

namespace SmartLabOS.DataMaintenance.Api.Services;

/// <summary>从卡片录入模板的一张工作表解析出的一条记录（DB 列）。</summary>
public sealed class CardRecord
{
    public required string SheetName { get; init; }
    public string? Id { get; set; }
    public Dictionary<string, object?> Columns { get; } = new(StringComparer.OrdinalIgnoreCase);
    public string? Error { get; set; }
}

/// <summary>
/// 卡片录入模板解析器：把"分组/字段名/输入内容 + 隐藏 group_key/field_key/类型/body_heading"
/// 的竖排卡片，按类型还原为 front-matter（data_json + raw_frontmatter）与正文，
/// 并抽取各主数据的关键列。一张工作表 = 一条记录。
/// 与 references/*.md 的 group_key/field_key/分组结构保持一致。
/// </summary>
public static class CardTemplateImporter
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        WriteIndented = false,
    };

    // 公共关键列（均位于 identification_version 下，键名同列名）
    private static readonly string[] CommonKeys = { "name", "version", "status", "owner", "last_updated" };

    // 各实体专有关键列： (DB列, 分组 group_key, 字段 field_key)
    private static readonly Dictionary<string, (string col, string grp, string fld)[]> Extra =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["modules"]      = new[] { ("module_part_no", "identification_version", "module_part_no"),
                                       ("category", "identification_version", "category") },
            ["platforms"]    = new[] { ("platform_part_no", "identification_version", "platform_part_no") },
            ["workstations"] = new[] { ("platform_id", "hardware_config", "platform_id"),
                                       ("maturity", "identification_version", "maturity") },
            ["solutions"]    = new[] { ("proposal_type", "identification_version", "proposal_type"),
                                       ("maturity", "identification_version", "maturity") },
            ["projects"]     = new[] { ("customer", "identification_version", "customer"),
                                       ("proposal_type", "identification_version", "proposal_type"),
                                       ("stage", "identification_version", "stage") },
        };

    /// <summary>判断是否为应跳过的非数据工作表（模板页）。</summary>
    public static bool IsTemplateSheet(string name)
        => name.StartsWith('_') || name.Contains("Templates", StringComparison.OrdinalIgnoreCase)
           || name.Contains("模版") || name.Contains("模板");

    public static CardRecord ParseSheet(SheetGrid grid, EntityDef e)
    {
        var rec = new CardRecord { SheetName = grid.Name };

        // 1) 定位表头行（含 group_key 的行）与关键列
        int hdr = -1;
        for (int r = 0; r < grid.Rows.Count && hdr < 0; r++)
            for (int c = 0; c < grid.Rows[r].Count; c++)
                if (grid.Cell(r, c).Contains("group_key")) { hdr = r; break; }
        if (hdr < 0) { rec.Error = "未找到隐藏列 group_key（模板未填键列，无法解析）"; return rec; }

        int cVal = FindCol(grid, hdr, "输入内容");
        int cType = FindCol(grid, hdr, "类型");
        int cGroup = FindCol(grid, hdr, "group_key");
        int cField = FindCol(grid, hdr, "field_key");
        int cBody = FindCol(grid, hdr, "body_heading");
        if (cVal < 0 || cGroup < 0 || cField < 0)
        { rec.Error = "模板缺少必要列（输入内容 / group_key / field_key）"; return rec; }

        // 2) 逐行解析
        var groups = new List<(string key, List<(string field, FieldValue fv)> fields)>();
        var groupIdx = new Dictionary<string, int>(StringComparer.Ordinal);
        var body = new List<(string heading, string content)>();
        string? lastGroup = null, lastField = null;

        for (int r = hdr + 1; r < grid.Rows.Count; r++)
        {
            string g = grid.Cell(r, cGroup).Trim();
            string f = grid.Cell(r, cField).Trim();
            string raw = grid.Cell(r, cVal);
            string type = cType >= 0 ? grid.Cell(r, cType).Trim() : "";
            string bh = cBody >= 0 ? grid.Cell(r, cBody).Trim() : "";

            bool isCont = g == "(cont)" || f == "(cont)";
            if (isCont)
            {
                AppendCont(groups, groupIdx, lastGroup, lastField, raw);
                continue;
            }
            if (g.Length == 0 && f.Length == 0) continue;        // 空行/分隔

            if (type == "body" || g == "body")
            {
                var content = raw.Replace("\r\n", "\n").TrimEnd('\n');
                if (content.Length > 0 || bh.Length > 0)
                    body.Add((bh.Length > 0 ? bh : f, content));
                lastGroup = lastField = null;
                continue;
            }
            if (f.Length == 0) continue;                          // 无字段键，跳过

            var fv = ParseValue(raw, type);
            int gi = groupIdx.TryGetValue(g, out var idx) ? idx
                : (groupIdx[g] = groups.Count);
            if (gi == groups.Count) groups.Add((g, new()));
            groups[gi].fields.Add((f, fv));
            lastGroup = g; lastField = f;
        }

        // 3) 组装 data_json（有序嵌套）+ raw_frontmatter + 正文
        var jsonRoot = new Dictionary<string, object?>();
        foreach (var (gk, fields) in groups)
        {
            var fmap = new Dictionary<string, object?>();
            foreach (var (fld, fv) in fields) fmap[fld] = fv.Value;
            jsonRoot[gk] = fmap;
        }
        string dataJson = JsonSerializer.Serialize(jsonRoot, JsonOpts);

        var emitGroups = groups.Select(g =>
            new KeyValuePair<string, IReadOnlyList<KeyValuePair<string, FieldValue>>>(
                g.key, g.fields.Select(x => new KeyValuePair<string, FieldValue>(x.field, x.fv)).ToList()))
            .ToList();
        string rawFm = YamlEmitter.Emit(emitGroups);

        string name = GetStr(jsonRoot, "identification_version", "name") ?? grid.Name;
        var bsb = new StringBuilder().Append("# ").Append(name).Append("\n\n");
        foreach (var (heading, content) in body)
            bsb.Append("## ").Append(heading).Append('\n').Append(content).Append("\n\n");

        // 4) 关键列
        string? id = GetStr(jsonRoot, "identification_version", "id");
        if (string.IsNullOrWhiteSpace(id)) { rec.Error = "无法确定主键 id（identification_version.id 为空）"; return rec; }
        rec.Id = id.Trim();
        rec.Columns["id"] = rec.Id;
        foreach (var k in CommonKeys)
            rec.Columns[k] = GetStr(jsonRoot, "identification_version", k);
        if (Extra.TryGetValue(e.Key, out var extras))
            foreach (var (col, grp, fld) in extras)
                rec.Columns[col] = GetStr(jsonRoot, grp, fld);
        rec.Columns["tags"] = ExtractTags(jsonRoot);
        rec.Columns["data_json"] = dataJson;
        rec.Columns["raw_frontmatter"] = rawFm;
        rec.Columns["body_markdown"] = bsb.ToString().TrimEnd('\n') + "\n";
        rec.Columns["source_file"] = rec.Id + ".md";
        return rec;
    }

    // ---- value parsing ----
    private static FieldValue ParseValue(string raw, string type)
    {
        string v = raw.Replace("\r\n", "\n");
        switch (type)
        {
            case "int":
                return new FieldValue { Value = ParseNum(v.Trim(), integer: true) };
            case "float":
                return new FieldValue { Value = ParseNum(v.Trim(), integer: false) };
            case "bool":
            {
                var t = v.Trim();
                if (t.Length == 0) return new FieldValue { Value = null };
                if (bool.TryParse(t, out var b)) return new FieldValue { Value = b };
                return new FieldValue { Value = t };
            }
            case "list_inline":
                return new FieldValue { Value = SplitList(v, inline: true, numeric: false), Style = YamlStyle.InlineList };
            case "list_inline_num":
                return new FieldValue { Value = SplitList(v, inline: true, numeric: true), Style = YamlStyle.InlineList };
            case "list_block":
                return new FieldValue { Value = SplitList(v, inline: false, numeric: false), Style = YamlStyle.BlockList };
            case "raw_yaml_block":
                return new FieldValue { Value = v.TrimEnd('\n'), Style = YamlStyle.LiteralBlock };
            default: // str / choice / date / function / 空 / 未知
                return new FieldValue { Value = v.Trim() };
        }
    }

    private static object? ParseNum(string s, bool integer)
    {
        if (s.Length == 0) return null;
        if (integer && long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l)) return l;
        if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
            return integer && d == Math.Floor(d) ? (object)(long)d : d;
        return s;   // "NA" 等非数字保留为字符串
    }

    private static List<object?> SplitList(string v, bool inline, bool numeric)
    {
        IEnumerable<string> parts = inline
            ? v.Split(new[] { ',', '，', '、' }, StringSplitOptions.None)
            : v.Split('\n');
        var list = new List<object?>();
        foreach (var p0 in parts)
        {
            var p = p0.Trim();
            if (!inline) { if (p.StartsWith("- ")) p = p[2..].Trim(); else if (p == "-") p = ""; }
            if (p.Length == 0) continue;
            list.Add(numeric ? ParseNum(p, integer: false) : p);
        }
        return list;
    }

    // (cont)：把上一字段并入列表并追加一项
    private static void AppendCont(List<(string key, List<(string field, FieldValue fv)> fields)> groups,
        Dictionary<string, int> groupIdx, string? lastGroup, string? lastField, string raw)
    {
        if (lastGroup is null || lastField is null) return;
        var val = raw.Trim();
        if (val.Length == 0) return;
        if (!groupIdx.TryGetValue(lastGroup, out var gi)) return;
        var fields = groups[gi].fields;
        for (int i = fields.Count - 1; i >= 0; i--)
        {
            if (fields[i].field != lastField) continue;
            var cur = fields[i].fv;
            var list = cur.Value as List<object?>;
            if (list is null)
            {
                list = new List<object?>();
                if (cur.Value is not null && cur.Value.ToString()!.Length > 0) list.Add(cur.Value);
            }
            list.Add(val);
            fields[i] = (lastField, new FieldValue { Value = list, Style = YamlStyle.BlockList });
            return;
        }
    }

    // ---- helpers ----
    private static int FindCol(SheetGrid grid, int hdr, string keyword)
    {
        for (int c = 0; c < grid.Rows[hdr].Count; c++)
            if (grid.Cell(hdr, c).Contains(keyword)) return c;
        return -1;
    }

    private static string? GetStr(Dictionary<string, object?> root, string group, string field)
    {
        if (root.TryGetValue(group, out var g) && g is Dictionary<string, object?> map
            && map.TryGetValue(field, out var v) && v is not null)
        {
            if (v is List<object?> list) return string.Join("，", list.Select(x => x?.ToString() ?? ""));
            return v.ToString();
        }
        return null;
    }

    private static string? ExtractTags(Dictionary<string, object?> root)
    {
        var t = GetStr(root, "tags_meta", "tags");
        if (t is not null) return t;
        // 顶层 tags
        if (root.TryGetValue("tags", out var v) && v is not null)
            return v is List<object?> l ? string.Join("，", l.Select(x => x?.ToString() ?? "")) : v.ToString();
        return null;
    }
}
