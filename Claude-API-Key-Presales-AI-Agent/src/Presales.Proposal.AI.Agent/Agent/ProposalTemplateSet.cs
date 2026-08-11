using Presales.Proposal.AI.Agent.Configuration;

namespace Presales.Proposal.AI.Agent.Agent;

/// <summary>模版目录中的一份样例文件及其在交付物中的角色。</summary>
/// <param name="Role">角色标识，如 solution-html / urs-html / detail-md / summary-md / word-style。</param>
/// <param name="Label">给模型看的中文角色说明。</param>
/// <param name="Path">绝对路径。</param>
public sealed record ProposalTemplateFile(string Role, string Label, string Path)
{
    public string FileName => System.IO.Path.GetFileName(Path);
}

/// <summary>
/// 提案模版目录（<c>Proposal_Template</c> → references/09-ProposalTemplate）的只读归类服务。
///
/// 该目录放的是**各类交付物的真实样例**（而非填空模版），本类按文件名/扩展名把它们归位到
/// 五个角色，供三处使用：
///   1) 系统提示词 —— 内联「解决方案 HTML」样例（阶段一每次都要用，放缓存前缀最划算），
///      其余样例只列路径 + 强制「动笔前先 read_kb_file 读」，避免把十几万字全塞进前缀；
///   2) 阶段一/二任务提示词 —— 在写 URS / 详细设计方案之前点名要读哪一份；
///   3) <see cref="DocxExporter"/> —— 用样例 .docx 作 Pandoc --reference-doc 与内置 OpenXML 的样式来源，
///      让 WORD 交付物的字体/标题/表格版式与模版一致。
///
/// 归类靠文件名关键字，宽松匹配、允许同角色多份（取最大的那份，通常最完整）。
/// 结果带轻量缓存，按目录签名（文件数 + 最新修改时间）失效，改模版无需重启。
/// </summary>
public sealed class ProposalTemplateSet
{
    private readonly PresalesPaths _paths;
    private readonly object _lock = new();
    private string _signature = "";
    private List<ProposalTemplateFile> _cache = new();

    public ProposalTemplateSet(PresalesPaths paths) => _paths = paths;

    /// <summary>模版目录中已归类的全部样例（按角色固定顺序）。</summary>
    public IReadOnlyList<ProposalTemplateFile> All()
    {
        var dir = _paths.ProposalTemplateDir;
        if (!Directory.Exists(dir)) return Array.Empty<ProposalTemplateFile>();

        var files = Directory.EnumerateFiles(dir).ToList();
        var sig = Signature(files);
        lock (_lock)
        {
            if (sig == _signature && _cache.Count > 0) return _cache;
            _cache = Classify(files);
            _signature = sig;
            return _cache;
        }
    }

    public ProposalTemplateFile? ByRole(string role) =>
        All().FirstOrDefault(t => t.Role == role);

    /// <summary>解决方案 HTML 样例（阶段一 1.html/2.html… 的版式依据）。</summary>
    public string? SolutionHtml => ByRole(RoleSolutionHtml)?.Path;
    /// <summary>模块 URS HTML 样例（阶段一 xxx-模块-URS.html 的版式依据）。</summary>
    public string? UrsHtml => ByRole(RoleUrsHtml)?.Path;
    /// <summary>《详细设计方案》Markdown 样例（阶段二正文的版式依据）。</summary>
    public string? DetailMd => ByRole(RoleDetailMd)?.Path;
    /// <summary>《输出总结》Markdown 样例。</summary>
    public string? SummaryMd => ByRole(RoleSummaryMd)?.Path;

    /// <summary>
    /// WORD 版式来源：模版目录里的 .docx。没有则退回 <see cref="PresalesPaths.ReferenceDocxFile"/>
    /// （Pandoc 首次运行时自动生成的那份）。
    /// </summary>
    public string ReferenceDocx => ByRole(RoleWordStyle)?.Path ?? _paths.ReferenceDocxFile;

    public const string RoleSolutionHtml = "solution-html";
    public const string RoleUrsHtml = "urs-html";
    public const string RoleDetailMd = "detail-md";
    public const string RoleSummaryMd = "summary-md";
    public const string RoleWordStyle = "word-style";

    // ---------------- 归类 ----------------

    private static List<ProposalTemplateFile> Classify(List<string> files)
    {
        string? Pick(Func<string, bool> match) => files
            .Where(f => match(Path.GetFileName(f)))
            .OrderByDescending(f => { try { return new FileInfo(f).Length; } catch { return 0L; } })
            .FirstOrDefault();

        static bool IsHtml(string n) => n.EndsWith(".html", StringComparison.OrdinalIgnoreCase)
                                     || n.EndsWith(".htm", StringComparison.OrdinalIgnoreCase);
        static bool IsMd(string n) => n.EndsWith(".md", StringComparison.OrdinalIgnoreCase);
        static bool HasUrs(string n) => n.Contains("URS", StringComparison.OrdinalIgnoreCase);
        static bool IsSummary(string n) => n.StartsWith("Summary", StringComparison.OrdinalIgnoreCase)
                                        || n.Contains("总结", StringComparison.Ordinal);

        var result = new List<ProposalTemplateFile>();

        void Add(string role, string label, string? path)
        {
            if (path is not null) result.Add(new ProposalTemplateFile(role, label, Path.GetFullPath(path)));
        }

        Add(RoleSolutionHtml, "解决方案 HTML 提案的版式与内容组织（阶段一 1.html、2.html…）",
            Pick(n => IsHtml(n) && !HasUrs(n)));
        Add(RoleUrsHtml, "新建模块需求说明书 URS 的版式（阶段一 xxx-模块-URS.html）",
            Pick(n => IsHtml(n) && HasUrs(n)));
        Add(RoleDetailMd, "《详细设计方案》Markdown 的章节层级、表格与措辞（阶段二正文）",
            Pick(n => IsMd(n) && !IsSummary(n)));
        Add(RoleSummaryMd, "《输出总结》Markdown 的条目组织（阶段二收尾）",
            Pick(n => IsMd(n) && IsSummary(n)));
        Add(RoleWordStyle, "WORD 交付物的字体/标题/表格样式来源（转 docx 时自动套用，无需模型处理）",
            Pick(n => n.EndsWith(".docx", StringComparison.OrdinalIgnoreCase)
                   && !n.StartsWith("~$", StringComparison.Ordinal)));   // 跳过 Word 临时锁文件

        return result;
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
