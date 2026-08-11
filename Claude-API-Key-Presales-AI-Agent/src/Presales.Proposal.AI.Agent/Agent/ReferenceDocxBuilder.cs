using System.Diagnostics;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Presales.Proposal.AI.Agent.Agent;

/// <summary>
/// 生成 / 维护「WORD 参考样式文档」(reference.docx)——Pandoc 的 --reference-doc 参数据此排版。
/// 它决定最终 WORD 的观感：中文字体、标题层级与配色、正文行距、表格样式、页眉页脚与页码。
///
/// 做法：以 Pandoc 自带的默认参考文档为底（保证 Pandoc 需要的样式 ID 一个不缺），
/// 再用 OpenXML 覆盖字体/标题/页面设置，并补上页眉页脚 + 页码域。
/// 若本机没有 Pandoc，则返回 null，由调用方退回内置 OpenXML 转换器。
/// </summary>
public static class ReferenceDocxBuilder
{
    private const string FontCn = "Microsoft YaHei";   // 中文正文/标题字体
    private const string Brand = "0C5C8F";             // 主品牌色（一级标题、页眉）
    private const string Brand2 = "0A7A72";            // 辅助色（三级标题）
    private const string Ink = "1F2A38";               // 正文墨色

    /// <summary>确保参考样式文档存在；返回其路径，不可用时返回 null。</summary>
    public static string? Ensure(string path, string pandocExe, Action<string>? log = null)
    {
        try
        {
            if (File.Exists(path)) return path;

            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            if (!TryDumpPandocDefault(path, pandocExe))
            {
                log?.Invoke("[docx] 无法从 Pandoc 导出默认参考文档（未安装 Pandoc？），本次不使用参考样式。");
                return null;
            }

            Customize(path);
            log?.Invoke($"[docx] 已生成 WORD 参考样式文档：{Path.GetFileName(path)}");
            return path;
        }
        catch (Exception ex)
        {
            log?.Invoke($"[docx] 生成参考样式文档失败（将用 Pandoc 默认版式）：{ex.Message}");
            try { if (File.Exists(path)) File.Delete(path); } catch { /* 残缺文件不可留 */ }
            return null;
        }
    }

    // ---------------- 1) 取 Pandoc 默认参考文档 ----------------

    private static bool TryDumpPandocDefault(string path, string pandocExe)
    {
        var psi = new ProcessStartInfo
        {
            FileName = pandocExe,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
        };
        psi.ArgumentList.Add("--print-default-data-file");
        psi.ArgumentList.Add("reference.docx");

        using var proc = Process.Start(psi);
        if (proc is null) return false;

        using (var fs = File.Create(path))
            proc.StandardOutput.BaseStream.CopyTo(fs);

        if (!proc.WaitForExit(60_000)) { try { proc.Kill(true); } catch { } return false; }
        return proc.ExitCode == 0 && new FileInfo(path).Length > 1024;
    }

    // ---------------- 2) 覆盖样式 / 页面 / 页眉页脚 ----------------

    private static void Customize(string path)
    {
        using var doc = WordprocessingDocument.Open(path, true);
        var main = doc.MainDocumentPart ?? throw new InvalidOperationException("参考文档缺少主文档部件");

        ApplyStyles(main);
        ApplyPageSetupAndHeaderFooter(doc, main);

        main.Document!.Save();
    }

    /// <summary>正文与标题：统一中文字体、字号、配色；标题保持 Word 大纲级别（导航窗格 + 目录域可用）。</summary>
    private static void ApplyStyles(MainDocumentPart main)
    {
        var stylesPart = main.StyleDefinitionsPart;
        if (stylesPart?.Styles is null) return;
        var styles = stylesPart.Styles;

        // 全局默认：字体 + 正文 10.5pt + 行距 1.5
        var docDefaults = styles.GetFirstChild<DocDefaults>() ?? styles.PrependChild(new DocDefaults());
        var runDefault = docDefaults.GetFirstChild<RunPropertiesDefault>() ?? docDefaults.AppendChild(new RunPropertiesDefault());
        var runBase = runDefault.GetFirstChild<RunPropertiesBaseStyle>() ?? runDefault.AppendChild(new RunPropertiesBaseStyle());
        runBase.RemoveAllChildren<RunFonts>();
        runBase.RemoveAllChildren<FontSize>();
        runBase.RemoveAllChildren<Color>();
        runBase.AppendChild(new RunFonts { Ascii = FontCn, HighAnsi = FontCn, EastAsia = FontCn, ComplexScript = FontCn });
        runBase.AppendChild(new FontSize { Val = "21" });          // 10.5pt
        runBase.AppendChild(new Color { Val = Ink });

        var paraDefault = docDefaults.GetFirstChild<ParagraphPropertiesDefault>() ?? docDefaults.AppendChild(new ParagraphPropertiesDefault());
        var paraBase = paraDefault.GetFirstChild<ParagraphPropertiesBaseStyle>() ?? paraDefault.AppendChild(new ParagraphPropertiesBaseStyle());
        paraBase.RemoveAllChildren<SpacingBetweenLines>();
        paraBase.AppendChild(new SpacingBetweenLines { Line = "360", LineRule = LineSpacingRuleValues.Auto, After = "120" });

        // 标题层级：字号 / 颜色 / 段前段后
        Restyle(styles, "Title",    44, Brand,  before: 0,   after: 240, bold: true);
        Restyle(styles, "Heading1", 32, Brand,  before: 320, after: 160, bold: true);
        Restyle(styles, "Heading2", 26, Ink,    before: 240, after: 120, bold: true);
        Restyle(styles, "Heading3", 23, Brand2, before: 200, after: 100, bold: true);
        Restyle(styles, "Heading4", 21, Ink,    before: 160, after: 80,  bold: true);
    }

    private static void Restyle(Styles styles, string styleId, int halfPointSize, string color,
        int before, int after, bool bold)
    {
        var style = styles.Elements<Style>().FirstOrDefault(s => s.StyleId?.Value == styleId);
        if (style is null) return;

        var rPr = style.GetFirstChild<StyleRunProperties>() ?? style.AppendChild(new StyleRunProperties());
        rPr.RemoveAllChildren<RunFonts>();
        rPr.RemoveAllChildren<FontSize>();
        rPr.RemoveAllChildren<Color>();
        rPr.RemoveAllChildren<Bold>();
        rPr.AppendChild(new RunFonts { Ascii = FontCn, HighAnsi = FontCn, EastAsia = FontCn, ComplexScript = FontCn });
        rPr.AppendChild(new FontSize { Val = halfPointSize.ToString() });
        rPr.AppendChild(new Color { Val = color });
        if (bold) rPr.AppendChild(new Bold());

        var pPr = style.GetFirstChild<StyleParagraphProperties>() ?? style.AppendChild(new StyleParagraphProperties());
        pPr.RemoveAllChildren<SpacingBetweenLines>();
        pPr.AppendChild(new SpacingBetweenLines { Before = before.ToString(), After = after.ToString() });
        if (pPr.GetFirstChild<KeepNext>() is null) pPr.AppendChild(new KeepNext());
    }

    /// <summary>A4 版面 + 页眉（公司/文档名）+ 页脚（第 X 页，PAGE 域）。</summary>
    private static void ApplyPageSetupAndHeaderFooter(WordprocessingDocument doc, MainDocumentPart main)
    {
        var body = main.Document!.Body ?? main.Document.AppendChild(new Body());
        var sectPr = body.Elements<SectionProperties>().LastOrDefault();
        if (sectPr is null) { sectPr = new SectionProperties(); body.AppendChild(sectPr); }

        // A4 纵向 + 页边距（twip：1cm ≈ 567）
        sectPr.RemoveAllChildren<PageSize>();
        sectPr.RemoveAllChildren<PageMargin>();
        sectPr.PrependChild(new PageMargin
        {
            Top = 1418, Bottom = 1418, Left = 1418, Right = 1418,
            Header = 851, Footer = 851, Gutter = 0,
        });
        sectPr.PrependChild(new PageSize { Width = 11906U, Height = 16838U });

        // 页眉
        var headerPart = main.AddNewPart<HeaderPart>();
        headerPart.Header = new Header(
            new Paragraph(
                new ParagraphProperties(
                    new Justification { Val = JustificationValues.Right },
                    new ParagraphBorders(new BottomBorder { Val = BorderValues.Single, Size = 4, Color = "BFC9D4" })),
                Text("百泉聚兴 SmartLabOS · 详细设计方案", 18, "8A97A6")));
        headerPart.Header.Save();

        // 页脚（第 X 页）
        var footerPart = main.AddNewPart<FooterPart>();
        var footerPara = new Paragraph(new ParagraphProperties(new Justification { Val = JustificationValues.Center }));
        footerPara.AppendChild(Text("第 ", 18, "8A97A6"));
        footerPara.AppendChild(new Run(new FieldChar { FieldCharType = FieldCharValues.Begin }));
        footerPara.AppendChild(new Run(new FieldCode(" PAGE ") { Space = SpaceProcessingModeValues.Preserve }));
        footerPara.AppendChild(new Run(new FieldChar { FieldCharType = FieldCharValues.Separate }));
        footerPara.AppendChild(Text("1", 18, "8A97A6"));
        footerPara.AppendChild(new Run(new FieldChar { FieldCharType = FieldCharValues.End }));
        footerPara.AppendChild(Text(" 页", 18, "8A97A6"));
        footerPart.Footer = new Footer(footerPara);
        footerPart.Footer.Save();

        sectPr.RemoveAllChildren<HeaderReference>();
        sectPr.RemoveAllChildren<FooterReference>();
        sectPr.PrependChild(new FooterReference { Type = HeaderFooterValues.Default, Id = main.GetIdOfPart(footerPart) });
        sectPr.PrependChild(new HeaderReference { Type = HeaderFooterValues.Default, Id = main.GetIdOfPart(headerPart) });
    }

    private static Run Text(string text, int halfPointSize, string color) =>
        new(new RunProperties(
                new RunFonts { Ascii = FontCn, HighAnsi = FontCn, EastAsia = FontCn },
                new FontSize { Val = halfPointSize.ToString() },
                new Color { Val = color }),
            new DocumentFormat.OpenXml.Wordprocessing.Text(text) { Space = SpaceProcessingModeValues.Preserve });
}
