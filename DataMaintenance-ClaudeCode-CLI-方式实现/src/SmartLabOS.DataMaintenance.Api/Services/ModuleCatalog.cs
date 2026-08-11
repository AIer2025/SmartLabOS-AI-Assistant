using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using SmartLabOS.DataMaintenance.Api.Presales;

namespace SmartLabOS.DataMaintenance.Api.Services;

/// <summary>单个模块的精简信息（用于「添加模块」选择器与模块ID校验/显示）。</summary>
public sealed record ModuleInfo(string Id, string Name, string Category);

/// <summary>
/// 模块知识库目录(references/01-modules)的只读目录服务：
///   - 扫描全部 *.md 卡片，解析最小信息(id/name/category)，供前端「添加模块」选择器使用；
///   - 提供模块ID校验与「ID → 名称/分类」解析，供「模块确认」与「方案生成」指令拼装使用。
///
/// 模块ID以**文件名**(去扩展名)为准，形如 MOD-CC-001，可靠且不依赖 YAML 解析；
/// name/category 从卡片 YAML frontmatter 的 identification_version 段解析，仅用于展示。
/// 结果带轻量缓存，按目录(文件数 + 最新修改时间)签名失效，模块新增/改动后自动刷新。
/// </summary>
public sealed class ModuleCatalog
{
    private readonly PresalesPaths _paths;
    private readonly object _lock = new();
    private string _signature = "";
    private List<ModuleInfo> _cache = new();

    // 模块ID规则：MOD-字母字母-数字数字数字（如 MOD-CC-001）。
    private static readonly Regex ModuleIdRegex =
        new(@"\bMOD-[A-Z]{2,4}-\d{3}\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex NameRegex =
        new(@"^\s{2}name:\s*(.+?)\s*$", RegexOptions.Compiled);
    private static readonly Regex CategoryRegex =
        new(@"^\s{2}category:\s*(.+?)\s*$", RegexOptions.Compiled);

    public ModuleCatalog(PresalesPaths paths) => _paths = paths;

    /// <summary>返回全部模块（按ID升序），结果带缓存。</summary>
    public IReadOnlyList<ModuleInfo> All()
    {
        var dir = _paths.ModulesDir;
        if (!Directory.Exists(dir)) return Array.Empty<ModuleInfo>();

        var files = Directory.EnumerateFiles(dir, "MOD-*.md").ToList();
        var sig = Signature(files);
        lock (_lock)
        {
            if (sig == _signature && _cache.Count > 0) return _cache;

            var list = new List<ModuleInfo>(files.Count);
            foreach (var f in files)
            {
                var id = Path.GetFileNameWithoutExtension(f);
                if (!ModuleIdRegex.IsMatch(id)) continue;   // 仅收录形如 MOD-XX-000 的卡片
                var (name, category) = ParseHeader(f);
                list.Add(new ModuleInfo(id.ToUpperInvariant(), name, category));
            }
            list.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));
            _cache = list;
            _signature = sig;
            return _cache;
        }
    }

    /// <summary>大小写无关地按ID取模块；不存在返回 null。</summary>
    public ModuleInfo? Find(string id) =>
        All().FirstOrDefault(m => string.Equals(m.Id, id, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// 规整一组候选模块ID：去空白、统一大写、按目录校验存在性、去重并保持首次出现顺序。
    /// 仅返回知识库中真实存在的模块ID。
    /// </summary>
    public List<string> Normalize(IEnumerable<string>? ids)
    {
        if (ids is null) return new();
        var byId = All().ToDictionary(m => m.Id, StringComparer.OrdinalIgnoreCase);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<string>();
        foreach (var raw in ids)
        {
            var id = (raw ?? "").Trim();
            if (id.Length == 0) continue;
            if (byId.TryGetValue(id, out var info) && seen.Add(info.Id))
                result.Add(info.Id);
        }
        return result;
    }

    /// <summary>从任意文本中按 MOD-XX-000 规则抽取模块ID（保持首次出现顺序），并校验存在性。</summary>
    public List<string> ExtractFromText(string? text)
    {
        if (string.IsNullOrEmpty(text)) return new();
        var ids = ModuleIdRegex.Matches(text).Select(m => m.Value);
        return Normalize(ids);
    }

    private static (string Name, string Category) ParseHeader(string file)
    {
        string name = "", category = "";
        try
        {
            // 只读取头部若干行(frontmatter 在文件最前)，避免整文件加载。
            using var reader = new StreamReader(file);
            int dashes = 0;
            for (string? line; (line = reader.ReadLine()) is not null;)
            {
                if (line.StartsWith("---"))
                {
                    if (++dashes >= 2) break;   // 离开 frontmatter
                    continue;
                }
                if (name.Length == 0 && NameRegex.Match(line) is { Success: true } nm)
                    name = Unquote(nm.Groups[1].Value);
                else if (category.Length == 0 && CategoryRegex.Match(line) is { Success: true } cm)
                    category = Unquote(cm.Groups[1].Value);
                if (name.Length > 0 && category.Length > 0) break;
            }
        }
        catch { /* 解析失败时退化为仅ID展示 */ }
        return (name, category);
    }

    private static string Unquote(string s)
    {
        s = s.Trim();
        if (s.Length >= 2 && (s[0] == '"' || s[0] == '\'') && s[^1] == s[0])
            s = s[1..^1];
        return s.Trim();
    }

    private static string Signature(List<string> files)
    {
        long ticks = 0;
        foreach (var f in files)
        {
            try { var t = File.GetLastWriteTimeUtc(f).Ticks; if (t > ticks) ticks = t; }
            catch { /* ignore */ }
        }
        return $"{files.Count}:{ticks}";
    }
}
