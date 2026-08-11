using System.Text;
using SmartLabOS.DataMaintenance.Api.Data;

namespace SmartLabOS.DataMaintenance.Api.Services;

/// <summary>
/// 将一行主数据还原为 YAML-MD 卡片文本（与 references/ 下源卡片同构）：
///   ---\n{raw_frontmatter}\n---\n\n{body_markdown}
/// 优先使用无损保留的 raw_frontmatter / body_markdown；二者皆缺时退化为
/// 由 data_json 兜底，保证导出始终有内容。
/// </summary>
public static class YamlMdCard
{
    public static string Build(IDictionary<string, object?> row)
    {
        var fm   = Str(row, "raw_frontmatter");
        var body = Str(row, "body_markdown");

        // raw_frontmatter 缺失时，用 data_json 兜底成最小 front-matter
        if (string.IsNullOrWhiteSpace(fm))
        {
            var json = Str(row, "data_json");
            if (!string.IsNullOrWhiteSpace(json))
                fm = "data_json: |\n" + Indent(json, "  ");
        }

        var sb = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(fm))
        {
            sb.Append("---\n").Append(fm.TrimEnd('\n')).Append("\n---\n\n");
        }
        sb.Append(body ?? string.Empty);
        var text = sb.ToString();
        if (!text.EndsWith('\n')) text += "\n";
        return text;
    }

    /// <summary>导出文件名：优先 source_file，其次 {id}.md。已做非法字符净化。</summary>
    public static string FileName(IDictionary<string, object?> row)
    {
        var src = Str(row, "source_file");
        if (!string.IsNullOrWhiteSpace(src))
        {
            src = src.Trim();
            if (!src.EndsWith(".md", StringComparison.OrdinalIgnoreCase)) src += ".md";
            return Sanitize(src);
        }
        var id = Str(row, "id");
        return Sanitize(string.IsNullOrWhiteSpace(id) ? "card" : id.Trim()) + ".md";
    }

    private static string Str(IDictionary<string, object?> row, string key)
        => row.TryGetValue(key, out var v) && v is not null ? v.ToString() ?? "" : "";

    private static string Indent(string text, string pad)
        => string.Join("\n", text.Replace("\r\n", "\n").Split('\n').Select(l => pad + l));

    private static string Sanitize(string name)
    {
        foreach (var ch in Path.GetInvalidFileNameChars())
            name = name.Replace(ch, '_');
        return name;
    }
}
