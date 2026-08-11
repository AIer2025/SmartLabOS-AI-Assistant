using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace SmartLabOS.DataMaintenance.Api.Services;

/// <summary>一张工作表的原始单元格网格（行 × 列，0 基；空缺以 "" 补齐到该行最大列）。</summary>
public sealed class SheetGrid
{
    public required string Name { get; init; }
    public required IReadOnlyList<IReadOnlyList<string>> Rows { get; init; }

    /// <summary>取某行某列的值；越界返回 ""。</summary>
    public string Cell(int row, int col)
        => row >= 0 && row < Rows.Count && col >= 0 && col < Rows[row].Count ? Rows[row][col] : "";
}

/// <summary>
/// 零依赖 XLSX/XLSM 读取器：以 ZIP + OpenXML 直接解析，仅用 .NET BCL
/// （System.IO.Compression + System.Xml.Linq），不引入任何第三方包。
/// 返回全部工作表（按工作簿内顺序）的原始单元格网格；.xlsm 的 VBA 部分忽略。
/// </summary>
public static class XlsxReader
{
    private static readonly XNamespace S  = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private static readonly XNamespace OR = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

    public static IReadOnlyList<SheetGrid> ReadSheets(string path)
    {
        using var zip = ZipFile.OpenRead(path);

        // 共享字符串表
        var shared = new List<string>();
        if (zip.GetEntry("xl/sharedStrings.xml") is { } ss)
            foreach (var si in Load(ss).Root!.Elements(S + "si"))
                shared.Add(NodeText(si));

        var grids = new List<SheetGrid>();
        foreach (var (name, target) in ResolveSheets(zip))
        {
            var entry = zip.GetEntry(target);
            if (entry is null) continue;
            grids.Add(new SheetGrid { Name = name, Rows = ReadGrid(Load(entry), shared) });
        }
        return grids;
    }

    private static IReadOnlyList<IReadOnlyList<string>> ReadGrid(XDocument sheet, List<string> shared)
    {
        var rows = new List<IReadOnlyList<string>>();
        foreach (var row in sheet.Descendants(S + "sheetData").Elements(S + "row"))
        {
            var cells = new Dictionary<int, string>();
            int maxCol = -1;
            foreach (var c in row.Elements(S + "c"))
            {
                var col = ColIndex((string?)c.Attribute("r"));
                if (col < 0) continue;
                cells[col] = CellValue(c, shared);
                if (col > maxCol) maxCol = col;
            }
            var list = new string[maxCol + 1];
            for (int i = 0; i <= maxCol; i++) list[i] = cells.TryGetValue(i, out var v) ? v : "";
            rows.Add(list);
        }
        return rows;
    }

    // ---- helpers ----
    private static XDocument Load(ZipArchiveEntry e)
    {
        using var s = e.Open();
        return XDocument.Load(s);
    }

    private static string CellValue(XElement c, List<string> shared)
    {
        var t = (string?)c.Attribute("t");
        switch (t)
        {
            case "s":
                var idx = c.Element(S + "v")?.Value;
                return int.TryParse(idx, out var i) && i >= 0 && i < shared.Count ? shared[i] : "";
            case "inlineStr":
                var inl = c.Element(S + "is");
                return inl != null ? NodeText(inl) : "";
            default: // "str"(公式结果) / "n"(数字) / "b"(布尔0/1) / "d"(日期) / null
                return c.Element(S + "v")?.Value ?? "";
        }
    }

    private static string NodeText(XElement node)
    {
        var sb = new StringBuilder();
        foreach (var t in node.Descendants(S + "t")) sb.Append(t.Value);
        return sb.ToString();
    }

    // "AB12" -> 0 基列号；非法返回 -1
    private static int ColIndex(string? cellRef)
    {
        if (string.IsNullOrEmpty(cellRef)) return -1;
        int col = 0, n = 0;
        foreach (var ch in cellRef)
        {
            if (ch is >= 'A' and <= 'Z') { col = col * 26 + (ch - 'A' + 1); n++; }
            else if (ch is >= 'a' and <= 'z') { col = col * 26 + (ch - 'a' + 1); n++; }
            else break;
        }
        return n == 0 ? -1 : col - 1;
    }

    // 经 workbook.xml + rels 解析全部工作表（名称 + 目标路径），保持工作簿内顺序
    private static IEnumerable<(string name, string target)> ResolveSheets(ZipArchive zip)
    {
        var result = new List<(string, string)>();
        try
        {
            var wb = zip.GetEntry("xl/workbook.xml");
            var rels = zip.GetEntry("xl/_rels/workbook.xml.rels");
            if (wb != null && rels != null)
            {
                var relMap = Load(rels).Root!.Elements()
                    .ToDictionary(e => (string)e.Attribute("Id")!, e => (string)e.Attribute("Target")!);
                foreach (var s in Load(wb).Descendants(S + "sheet"))
                {
                    var name = (string?)s.Attribute("name") ?? "";
                    var rid = (string?)s.Attribute(OR + "id");
                    if (rid != null && relMap.TryGetValue(rid, out var tgt))
                        result.Add((name, NormalizeTarget(tgt)));
                }
                if (result.Count > 0) return result;
            }
        }
        catch { /* 回退 */ }

        foreach (var e in zip.Entries
                     .Where(e => e.FullName.StartsWith("xl/worksheets/") && e.FullName.EndsWith(".xml"))
                     .OrderBy(e => e.FullName))
            result.Add((Path.GetFileNameWithoutExtension(e.FullName), e.FullName));
        return result;
    }

    private static string NormalizeTarget(string target)
    {
        if (target.StartsWith('/')) return target.TrimStart('/');
        return target.StartsWith("xl/") ? target : "xl/" + target;
    }
}
