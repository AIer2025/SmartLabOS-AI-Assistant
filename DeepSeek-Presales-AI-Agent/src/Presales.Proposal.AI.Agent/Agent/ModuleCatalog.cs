using System.Text.RegularExpressions;
using Presales.Proposal.AI.Agent.Configuration;

namespace Presales.Proposal.AI.Agent.Agent;

/// <summary>单个模块的精简信息（用于「添加模块」选择器与模块ID校验/显示）。</summary>
public sealed record ModuleInfo(string Id, string Name, string Category);

/// <summary>
/// 模块知识库目录(references/01-modules)的只读目录服务：
///   - 扫描全部 MOD-*.md 卡片，解析最小信息(id/name/category)；
///   - 提供模块ID校验与「ID → 名称/分类」解析，供「模块确认」与「方案生成」使用。
/// 模块ID以**文件名**(去扩展名)为准，形如 MOD-CC-001，可靠且不依赖 YAML 解析。
/// 结果带轻量缓存，按目录签名(文件数 + 最新修改时间)失效。
/// </summary>
public sealed class ModuleCatalog
{
    private readonly PresalesPaths _paths;
    private readonly object _lock = new();
    private string _signature = "";
    private List<ModuleInfo> _cache = new();

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
                if (!ModuleIdRegex.IsMatch(id)) continue;
                var (name, category) = ParseHeader(f);
                list.Add(new ModuleInfo(id.ToUpperInvariant(), name, category));
            }
            list.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));
            _cache = list;
            _signature = sig;
            return _cache;
        }
    }

    public ModuleInfo? Find(string id) =>
        All().FirstOrDefault(m => string.Equals(m.Id, id, StringComparison.OrdinalIgnoreCase));

    /// <summary>规整候选模块ID：去空白、统一大写、按目录校验存在性、去重并保持首次出现顺序。仅返回真实存在的ID。</summary>
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
            using var reader = new StreamReader(file);
            int dashes = 0;
            for (string? line; (line = reader.ReadLine()) is not null;)
            {
                if (line.StartsWith("---"))
                {
                    if (++dashes >= 2) break;
                    continue;
                }
                if (name.Length == 0 && NameRegex.Match(line) is { Success: true } nm)
                    name = Unquote(nm.Groups[1].Value);
                else if (category.Length == 0 && CategoryRegex.Match(line) is { Success: true } cm)
                    category = Unquote(cm.Groups[1].Value);
                if (name.Length > 0 && category.Length > 0) break;
            }
        }
        catch { /* 解析失败退化为仅ID展示 */ }
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
