using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Presales.Proposal.AI.Agent.Configuration;

namespace Presales.Proposal.AI.Agent.Agent;

/// <summary>
/// 把《详细设计方案》Markdown 导出为 .docx。两条路径，按可用性自动选择：
///   1) Pandoc —— 若本机安装了 pandoc（或 Agent:PandocPath 指向可执行文件），保真度最高；
///   2) 内置 OpenXML 转换器 —— 无任何外部依赖、离线可用的兜底，覆盖标题/段落/列表/表格/粗斜体/代码。
/// 另有第三条可选路径（Anthropic Agent Skills 容器 + docx 技能）见 README，本类默认不启用（避免额外 API 成本与 beta 依赖）。
/// </summary>
public sealed class DocxExporter
{
    private readonly AgentSettings _s;
    private readonly PresalesPaths _paths;
    private readonly ProposalTemplateSet _templates;
    private readonly ILogger<DocxExporter> _log;

    public DocxExporter(AgentSettings s, PresalesPaths paths, ProposalTemplateSet templates, ILogger<DocxExporter> log)
    { _s = s; _paths = paths; _templates = templates; _log = log; }

    /// <summary>
    /// WORD 版式来源：优先用提案模版目录里的 .docx（<c>Proposal_Template</c>），它同时喂给
    /// Pandoc 的 --reference-doc 和内置 OpenXML 的样式克隆，两条路径产出的 WORD 版式因此一致。
    /// 模版目录没有 .docx 时退回 references/_templates 下由 Pandoc 自动生成的那份。
    /// </summary>
    public string? StyleSourcePath
    {
        get
        {
            var p = _templates.ReferenceDocx;
            return !string.IsNullOrWhiteSpace(p) && File.Exists(p) ? p : null;
        }
    }

    /// <summary>把 mdPath 转为同目录同名 .docx。返回所用引擎("pandoc"/"openxml")，失败返回 null。</summary>
    public string? Export(string mdPath, Action<string>? log = null)
    {
        if (!_s.EnableDocx) return null;
        if (!File.Exists(mdPath)) { log?.Invoke($"[docx] 源 Markdown 不存在：{mdPath}"); return null; }
        var docxPath = Path.ChangeExtension(mdPath, ".docx");

        // 1) 优先 Pandoc
        if (TryPandoc(mdPath, docxPath, log)) return "pandoc";

        // 2) 兜底：内置 OpenXML（尽量克隆模版 docx 的样式，使版式与 Pandoc 路径一致）
        try
        {
            var md = File.ReadAllText(mdPath);
            var style = StyleSourcePath;
            MarkdownToDocx(md, docxPath, style);
            log?.Invoke($"[docx] 已用内置 OpenXML 转换器生成：{Path.GetFileName(docxPath)}"
                      + (style is null ? "（内置版式）" : $"（版式取自模版 {Path.GetFileName(style)}）"));
            return "openxml";
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "OpenXML 导出 docx 失败 {Path}", docxPath);
            log?.Invoke($"[docx] 生成失败：{ex.Message}");
            return null;
        }
    }

    // ---------------- Pandoc ----------------

    private bool TryPandoc(string mdPath, string docxPath, Action<string>? log)
    {
        var exe = string.IsNullOrWhiteSpace(_s.PandocPath) ? "pandoc" : _s.PandocPath!.Trim();
        try
        {
            // 参考样式文档：决定中文字体、标题配色、页眉页脚页码。
            // 优先用提案模版目录里的 .docx；没有才退回 _templates 下那份（缺失时由 Pandoc 现生成）。
            var reference = StyleSourcePath ?? ReferenceDocxBuilder.Ensure(_paths.ReferenceDocxFile, exe, log);

            // 封面标题取自文件名「<项目名>-详细设计方案.md」
            var title = Path.GetFileNameWithoutExtension(mdPath);

            var psi = new ProcessStartInfo
            {
                FileName = exe,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            };
            psi.ArgumentList.Add(mdPath);
            psi.ArgumentList.Add("--from"); psi.ArgumentList.Add("gfm");
            if (reference is not null) { psi.ArgumentList.Add("--reference-doc"); psi.ArgumentList.Add(reference); }
            psi.ArgumentList.Add("--toc");                        // 自动生成目录（Word 中可右键更新）
            psi.ArgumentList.Add("--toc-depth=3");
            psi.ArgumentList.Add("--metadata"); psi.ArgumentList.Add($"title={title}");
            psi.ArgumentList.Add("-o"); psi.ArgumentList.Add(docxPath);

            using var p = Process.Start(psi);
            if (p is null) return false;
            var err = p.StandardError.ReadToEnd();
            if (!p.WaitForExit(120_000)) { try { p.Kill(true); } catch { } return false; }
            if (p.ExitCode == 0 && File.Exists(docxPath))
            {
                log?.Invoke($"[docx] 已用 Pandoc 生成：{Path.GetFileName(docxPath)}");
                return true;
            }
            if (!string.IsNullOrWhiteSpace(err)) log?.Invoke($"[docx] Pandoc 返回码 {p.ExitCode}：{Truncate(err, 200)}");
            return false;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            // 未安装 pandoc / 路径无效 —— 交由内置转换器兜底（不记为错误）
            return false;
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "调用 Pandoc 异常");
            return false;
        }
    }

    // ---------------- 内置 Markdown → OpenXML ----------------

    private static readonly Regex InlineRegex =
        new(@"(\*\*[^*]+\*\*|\*[^*]+\*|`[^`]+`)", RegexOptions.Compiled);

    private const string FontCn = "Microsoft YaHei";
    private const string Brand = "0C5C8F";
    private const string Brand2 = "0A7A72";
    private const string Ink = "1F2A38";

    /// <summary>
    /// 将 Markdown（标题/段落/有序无序列表/GFM 表格/代码块/粗斜体/行内代码）转换为 .docx。
    /// <paramref name="styleSourceDocx"/> 给出时克隆其样式表与主题，使版式与提案模版一致；
    /// 为 null 或克隆失败则退回内置样式（不影响转换本身）。
    /// </summary>
    public static void MarkdownToDocx(string markdown, string docxPath, string? styleSourceDocx = null)
    {
        using var doc = WordprocessingDocument.Create(docxPath, WordprocessingDocumentType.Document);
        var main = doc.AddMainDocumentPart();
        main.Document = new Document();
        var body = main.Document.AppendChild(new Body());

        // 真正的 Heading1-4 样式：Word 导航窗格与「插入目录」才能识别
        if (!TryCloneStyles(main, styleSourceDocx)) BuildStyles(main);

        var lines = markdown.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
        int i = 0;
        bool inCode = false;
        var codeBuf = new StringBuilder();

        while (i < lines.Length)
        {
            var line = lines[i];

            // 代码围栏 ```
            if (line.TrimStart().StartsWith("```"))
            {
                if (inCode)
                {
                    AppendCodeBlock(body, codeBuf.ToString());
                    codeBuf.Clear(); inCode = false;
                }
                else inCode = true;
                i++; continue;
            }
            if (inCode) { codeBuf.AppendLine(line); i++; continue; }

            // 空行
            if (string.IsNullOrWhiteSpace(line)) { i++; continue; }

            // 水平线
            if (Regex.IsMatch(line.Trim(), @"^(-{3,}|\*{3,}|_{3,})$")) { i++; continue; }

            // 标题
            var h = Regex.Match(line, @"^(#{1,6})\s+(.*)$");
            if (h.Success)
            {
                AppendHeading(body, h.Groups[2].Value.Trim(), h.Groups[1].Value.Length);
                i++; continue;
            }

            // GFM 表格：当前行含 | 且下一行是分隔行
            if (line.Contains('|') && i + 1 < lines.Length && IsTableSeparator(lines[i + 1]))
            {
                var tableLines = new List<string> { line };
                i += 2; // 跳过表头与分隔行之间——先收表头
                // 收集数据行
                var dataRows = new List<string>();
                while (i < lines.Length && lines[i].Contains('|') && !string.IsNullOrWhiteSpace(lines[i]))
                { dataRows.Add(lines[i]); i++; }
                AppendTable(body, SplitRow(line), dataRows.Select(SplitRow).ToList());
                continue;
            }

            // 列表项
            var ul = Regex.Match(line, @"^\s*[-*+]\s+(.*)$");
            var ol = Regex.Match(line, @"^\s*\d+\.\s+(.*)$");
            if (ul.Success) { AppendParagraph(body, "• " + ul.Groups[1].Value); i++; continue; }
            if (ol.Success) { AppendParagraph(body, ol.Value.TrimStart()); i++; continue; }

            // 普通段落
            AppendParagraph(body, line.Trim());
            i++;
        }
        if (inCode && codeBuf.Length > 0) AppendCodeBlock(body, codeBuf.ToString());

        AppendSection(doc, main, body);   // A4 版面 + 页脚页码
        main.Document.Save();
    }

    /// <summary>
    /// 从模版 .docx 克隆样式表（含 Heading1-4、正文字体、表格样式）与主题，让内置转换器产出的
    /// WORD 与模版版式一致。只复制 styles.xml / theme1.xml，不带入模版正文。
    /// 模版缺失、无样式部件或克隆出错时返回 false，由调用方退回内置样式。
    /// </summary>
    private static bool TryCloneStyles(MainDocumentPart main, string? styleSourceDocx)
    {
        if (string.IsNullOrWhiteSpace(styleSourceDocx) || !File.Exists(styleSourceDocx)) return false;
        try
        {
            using var src = WordprocessingDocument.Open(styleSourceDocx, false);
            var srcMain = src.MainDocumentPart;
            var srcStyles = srcMain?.StyleDefinitionsPart;
            if (srcStyles?.Styles is null) return false;

            // Heading 样式缺失的模版克隆过来反而更糟（Word 认不出层级、目录域空白）——宁可用内置样式
            var hasHeading = srcStyles.Styles.Elements<Style>()
                .Any(s => string.Equals(s.StyleId?.Value, "Heading1", StringComparison.OrdinalIgnoreCase));
            if (!hasHeading) return false;

            var part = main.AddNewPart<StyleDefinitionsPart>();
            part.Styles = (Styles)srcStyles.Styles.CloneNode(true);
            part.Styles.Save();

            if (srcMain?.ThemePart?.Theme is { } theme)
            {
                var themePart = main.AddNewPart<ThemePart>();
                themePart.Theme = (DocumentFormat.OpenXml.Drawing.Theme)theme.CloneNode(true);
                themePart.Theme.Save();
            }
            return true;
        }
        catch { return false; }   // 模版被 Word 占用 / 格式异常：不阻断导出
    }

    /// <summary>样式表：默认中文字体 + Heading1-4（带 outlineLvl，Word 可据此生成目录/导航）。</summary>
    private static void BuildStyles(MainDocumentPart main)
    {
        var part = main.AddNewPart<StyleDefinitionsPart>();
        var styles = new Styles();

        styles.AppendChild(new DocDefaults(
            new RunPropertiesDefault(new RunPropertiesBaseStyle(
                new RunFonts { Ascii = FontCn, HighAnsi = FontCn, EastAsia = FontCn, ComplexScript = FontCn },
                new FontSize { Val = "21" },
                new Color { Val = Ink })),
            new ParagraphPropertiesDefault(new ParagraphPropertiesBaseStyle(
                new SpacingBetweenLines { Line = "360", LineRule = LineSpacingRuleValues.Auto, After = "120" }))));

        AddHeadingStyle(styles, "Heading1", "标题 1", 1, 32, Brand);
        AddHeadingStyle(styles, "Heading2", "标题 2", 2, 26, Ink);
        AddHeadingStyle(styles, "Heading3", "标题 3", 3, 23, Brand2);
        AddHeadingStyle(styles, "Heading4", "标题 4", 4, 21, Ink);

        part.Styles = styles;
        part.Styles.Save();
    }

    private static void AddHeadingStyle(Styles styles, string id, string name, int level, int halfPt, string color)
    {
        styles.AppendChild(new Style(
            new StyleName { Val = name },
            new BasedOn { Val = "Normal" },
            new StyleParagraphProperties(
                new KeepNext(),
                new OutlineLevel { Val = level - 1 },
                new SpacingBetweenLines { Before = (400 - level * 60).ToString(), After = (200 - level * 30).ToString() }),
            new StyleRunProperties(
                new RunFonts { Ascii = FontCn, HighAnsi = FontCn, EastAsia = FontCn, ComplexScript = FontCn },
                new Bold(),
                new FontSize { Val = halfPt.ToString() },
                new Color { Val = color }))
        {
            Type = StyleValues.Paragraph,
            StyleId = id,
            PrimaryStyle = new PrimaryStyle(),
        });
    }

    /// <summary>A4 版面与「第 X 页」页脚。</summary>
    private static void AppendSection(WordprocessingDocument doc, MainDocumentPart main, Body body)
    {
        var footerPart = main.AddNewPart<FooterPart>();
        var para = new Paragraph(new ParagraphProperties(new Justification { Val = JustificationValues.Center }));
        para.AppendChild(SmallRun("第 "));
        para.AppendChild(new Run(new FieldChar { FieldCharType = FieldCharValues.Begin }));
        para.AppendChild(new Run(new FieldCode(" PAGE ") { Space = SpaceProcessingModeValues.Preserve }));
        para.AppendChild(new Run(new FieldChar { FieldCharType = FieldCharValues.Separate }));
        para.AppendChild(SmallRun("1"));
        para.AppendChild(new Run(new FieldChar { FieldCharType = FieldCharValues.End }));
        para.AppendChild(SmallRun(" 页"));
        footerPart.Footer = new Footer(para);
        footerPart.Footer.Save();

        body.AppendChild(new SectionProperties(
            new FooterReference { Type = HeaderFooterValues.Default, Id = main.GetIdOfPart(footerPart) },
            new PageSize { Width = 11906U, Height = 16838U },
            new PageMargin { Top = 1418, Bottom = 1418, Left = 1418, Right = 1418, Header = 851, Footer = 851, Gutter = 0 }));
    }

    private static Run SmallRun(string text) =>
        new(new RunProperties(
                new RunFonts { Ascii = FontCn, HighAnsi = FontCn, EastAsia = FontCn },
                new FontSize { Val = "18" },
                new Color { Val = "8A97A6" }),
            new Text(text) { Space = SpaceProcessingModeValues.Preserve });

    private static bool IsTableSeparator(string line) =>
        Regex.IsMatch(line.Trim(), @"^\|?\s*:?-{1,}:?\s*(\|\s*:?-{1,}:?\s*)+\|?$");

    private static List<string> SplitRow(string row)
    {
        var t = row.Trim();
        if (t.StartsWith("|")) t = t[1..];
        if (t.EndsWith("|")) t = t[..^1];
        return t.Split('|').Select(c => c.Trim()).ToList();
    }

    private static void AppendHeading(Body body, string text, int level)
    {
        // 用真正的 Heading 样式（而非仅加粗字号），Word 导航窗格、目录域、书签才认得
        var styleId = "Heading" + Math.Min(level, 4);
        var p = new Paragraph(new ParagraphProperties(new ParagraphStyleId { Val = styleId }));
        p.AppendChild(new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve }));
        body.AppendChild(p);
    }

    private static void AppendParagraph(Body body, string text)
    {
        var p = new Paragraph();
        AppendInlineRuns(p, text, mono: false);
        body.AppendChild(p);
    }

    private static void AppendCodeBlock(Body body, string code)
    {
        foreach (var ln in code.Replace("\r\n", "\n").TrimEnd('\n').Split('\n'))
        {
            var p = new Paragraph(new ParagraphProperties(new Shading { Fill = "F2F2F2", Val = ShadingPatternValues.Clear }));
            var run = new Run { RunProperties = new RunProperties(new RunFonts { Ascii = "Consolas", HighAnsi = "Consolas" }, new FontSize { Val = "18" }) };
            run.AppendChild(new Text(ln) { Space = SpaceProcessingModeValues.Preserve });
            p.AppendChild(run);
            body.AppendChild(p);
        }
    }

    private static void AppendTable(Body body, List<string> header, List<List<string>> rows)
    {
        var table = new Table();
        var borders = new TableBorders(
            new TopBorder { Val = BorderValues.Single, Size = 4 },
            new BottomBorder { Val = BorderValues.Single, Size = 4 },
            new LeftBorder { Val = BorderValues.Single, Size = 4 },
            new RightBorder { Val = BorderValues.Single, Size = 4 },
            new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4 },
            new InsideVerticalBorder { Val = BorderValues.Single, Size = 4 });
        table.AppendChild(new TableProperties(
            new TableStyle { Val = "TableGrid" },
            new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct },
            borders));

        table.AppendChild(BuildRow(header, bold: true));
        foreach (var r in rows) table.AppendChild(BuildRow(r, bold: false));
        body.AppendChild(table);
        body.AppendChild(new Paragraph()); // 表后空行
    }

    private static TableRow BuildRow(List<string> cells, bool bold)
    {
        var tr = new TableRow();
        foreach (var c in cells)
        {
            var p = new Paragraph();
            AppendInlineRuns(p, c, mono: false, forceBold: bold);
            tr.AppendChild(new TableCell(p));
        }
        return tr;
    }

    /// <summary>行内格式：**粗体** *斜体* `代码`。</summary>
    private static void AppendInlineRuns(Paragraph p, string text, bool mono, bool forceBold = false)
    {
        if (mono) { p.AppendChild(MakeRun(text, bold: false, italic: false, code: true)); return; }
        text ??= "";
        int last = 0;
        foreach (Match m in InlineRegex.Matches(text))
        {
            if (m.Index > last) p.AppendChild(MakeRun(text[last..m.Index], forceBold, false, false));
            var tok = m.Value;
            if (tok.StartsWith("**")) p.AppendChild(MakeRun(tok[2..^2], true, false, false));
            else if (tok.StartsWith("`")) p.AppendChild(MakeRun(tok[1..^1], forceBold, false, true));
            else p.AppendChild(MakeRun(tok[1..^1], forceBold, true, false));
            last = m.Index + m.Length;
        }
        if (last < text.Length) p.AppendChild(MakeRun(text[last..], forceBold, false, false));
        if (p.ChildElements.Count == 0) p.AppendChild(MakeRun("", forceBold, false, false));
    }

    private static Run MakeRun(string text, bool bold, bool italic, bool code)
    {
        var rPr = new RunProperties();
        if (bold) rPr.AppendChild(new Bold());
        if (italic) rPr.AppendChild(new Italic());
        if (code) { rPr.AppendChild(new RunFonts { Ascii = "Consolas", HighAnsi = "Consolas" }); rPr.AppendChild(new FontSize { Val = "18" }); }
        var run = new Run();
        if (rPr.HasChildren) run.RunProperties = rPr;
        run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
        return run;
    }

    private static string Truncate(string s, int n) => s.Length <= n ? s : s[..n] + "…";
}
