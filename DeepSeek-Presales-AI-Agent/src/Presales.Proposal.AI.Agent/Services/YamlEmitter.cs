using System.Globalization;
using System.Text;

namespace SmartLabOS.DataMaintenance.Api.Services;

/// <summary>一个待渲染的字段值及其 YAML 呈现风格。</summary>
public enum YamlStyle { Scalar, InlineList, BlockList, LiteralBlock }

public sealed class FieldValue
{
    public required object? Value { get; init; }   // string / long / double / bool / null / List<object?>
    public YamlStyle Style { get; init; } = YamlStyle.Scalar;
}

/// <summary>
/// 受限场景的 YAML 生成器：仅需「顶层分组 → 字段 → 标量/标量列表/多行文本」两层结构
/// （卡片录入模板的产物即如此），无需第三方 YAML 库即可输出与源卡片同构的 front-matter。
/// </summary>
public static class YamlEmitter
{
    /// <summary>groups: 有序的 (group_key, 有序字段表)。</summary>
    public static string Emit(IReadOnlyList<KeyValuePair<string, IReadOnlyList<KeyValuePair<string, FieldValue>>>> groups)
    {
        var sb = new StringBuilder();
        foreach (var (group, fields) in groups.Select(g => (g.Key, g.Value)))
        {
            sb.Append(group).Append(":\n");
            foreach (var (name, fv) in fields.Select(f => (f.Key, f.Value)))
                EmitField(sb, name, fv);
        }
        return sb.ToString().TrimEnd('\n');
    }

    private static void EmitField(StringBuilder sb, string name, FieldValue fv)
    {
        switch (fv.Style)
        {
            case YamlStyle.InlineList:
            {
                var items = AsList(fv.Value).Select(ScalarText);
                sb.Append("  ").Append(name).Append(": [").Append(string.Join(", ", items)).Append("]\n");
                break;
            }
            case YamlStyle.BlockList:
            {
                var list = AsList(fv.Value);
                if (list.Count == 0) { sb.Append("  ").Append(name).Append(":\n    []\n"); break; }
                sb.Append("  ").Append(name).Append(":\n");
                foreach (var item in list)
                    sb.Append("    - ").Append(ScalarText(item)).Append('\n');
                break;
            }
            case YamlStyle.LiteralBlock:
            {
                var text = (fv.Value?.ToString() ?? "").Replace("\r\n", "\n").TrimEnd('\n');
                sb.Append("  ").Append(name).Append(": |\n");
                foreach (var line in text.Split('\n'))
                    sb.Append("    ").Append(line).Append('\n');
                break;
            }
            default:
                sb.Append("  ").Append(name).Append(": ").Append(ScalarText(fv.Value)).Append('\n');
                break;
        }
    }

    private static List<object?> AsList(object? v)
        => v is List<object?> l ? l : v is null ? new() : new() { v };

    // 标量渲染：数字/布尔原样；null 渲染空；字符串在含 YAML 敏感字符或可能被误解析时加引号
    private static string ScalarText(object? v)
    {
        switch (v)
        {
            case null: return "";
            case bool b: return b ? "true" : "false";
            case long l: return l.ToString(CultureInfo.InvariantCulture);
            case int i: return i.ToString(CultureInfo.InvariantCulture);
            case double d: return d.ToString("0.#############", CultureInfo.InvariantCulture);
            case float f: return ((double)f).ToString("0.#############", CultureInfo.InvariantCulture);
        }
        var s = v.ToString() ?? "";
        return NeedsQuote(s) ? "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"" : s;
    }

    private static bool NeedsQuote(string s)
    {
        if (s.Length == 0) return true;
        if (s != s.Trim()) return true;                          // 首尾空白
        if (s.Contains('\n') || s.Contains('"')) return true;
        // 含 YAML 流/指示字符
        if (s.IndexOfAny(new[] { ':', '#', '[', ']', '{', '}', ',', '&', '*', '!', '|', '>', '%', '@', '`' }) >= 0)
        {
            // ": " 或行尾 ":" 才真正有歧义；保守起见含 ':' 即引用
            return true;
        }
        var c0 = s[0];
        if (c0 is '-' or '?' or '\'' or ' ') return true;
        // 看起来像数字/布尔/null 的字符串也引用，避免被解析为非字符串
        if (bool.TryParse(s, out _)) return true;
        if (string.Equals(s, "null", StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }
}
