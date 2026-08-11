using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SmartLabOS.DataMaintenance.Api.Data;
using SmartLabOS.DataMaintenance.Api.Services;

namespace SmartLabOS.DataMaintenance.Api.Controllers;

/// <summary>
/// 五类主数据的批量传输接口：
///   POST /api/{entity}/export          —— 导出 YAML-MD 卡片到服务器目录（全部/指定记录）
///   POST /api/{entity}/import/preview  —— 预览 Excel 导入：列映射 + 主键冲突检测
///   POST /api/{entity}/import/commit   —— 执行导入：新增入库；冲突项按用户勾选覆盖
/// 路径/文件均为服务器端文件系统路径（按需求约定）。
/// </summary>
[ApiController]
[Route("api/{entity}")]
public sealed class TransferController : ControllerBase
{
    private readonly SmartLabRepository _repo;
    public TransferController(SmartLabRepository repo) => _repo = repo;

    private EntityDef? Resolve(string entity) => EntityRegistry.Find(entity);

    // ============================ 导出 ============================
    public sealed record ExportRequest(string? Directory, string? Scope, List<string>? Ids, bool Overwrite = true);

    [HttpPost("export")]
    public async Task<IActionResult> Export(string entity, [FromBody] ExportRequest req)
    {
        if (Resolve(entity) is not { } e) return NotFound(new { message = $"未知实体: {entity}" });
        if (string.IsNullOrWhiteSpace(req.Directory))
            return BadRequest(new { message = "请提供导出目录路径" });

        var selected = string.Equals(req.Scope, "selected", StringComparison.OrdinalIgnoreCase);
        if (selected && (req.Ids is null || req.Ids.Count == 0))
            return BadRequest(new { message = "导出范围为“指定记录”，但未提供任何记录 ID" });

        string dir = req.Directory.Trim();
        try { Directory.CreateDirectory(dir); }
        catch (Exception ex) { return BadRequest(new { message = $"无法创建/访问目录: {ex.Message}" }); }

        var rows = await _repo.GetManyAsync(e, selected ? req.Ids : null);

        var files = new List<object>();
        int written = 0, skipped = 0;
        var enc = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true); // 与源卡片一致：UTF-8 BOM
        foreach (var row in rows)
        {
            var name = YamlMdCard.FileName(row);
            var full = Path.Combine(dir, name);
            var id = row.TryGetValue("id", out var idv) ? idv?.ToString() : null;
            if (System.IO.File.Exists(full) && !req.Overwrite)
            {
                skipped++;
                files.Add(new { id, file = name, status = "skipped-exists" });
                continue;
            }
            try
            {
                await System.IO.File.WriteAllTextAsync(full, YamlMdCard.Build(row), enc);
                written++;
                files.Add(new { id, file = name, status = "written" });
            }
            catch (Exception ex)
            {
                files.Add(new { id, file = name, status = "error", message = ex.Message });
            }
        }

        return Ok(new
        {
            entity = e.Key,
            directory = Path.GetFullPath(dir),
            requested = rows.Count,
            written,
            skipped,
            files
        });
    }


    // ============================ 导入（卡片录入模板） ============================
    public sealed record ImportRequest(string? FilePath, List<string>? OverwriteIds);

    [HttpPost("import/preview")]
    public async Task<IActionResult> ImportPreview(string entity, [FromBody] ImportRequest req)
    {
        if (Resolve(entity) is not { } e) return NotFound(new { message = $"未知实体: {entity}" });
        if (ParseCards(e, req, out var records, out var err) is false) return BadRequest(new { message = err });

        var ids = records.Where(r => !string.IsNullOrWhiteSpace(r.Id)).Select(r => r.Id!).Distinct().ToList();
        var existing = await _repo.ExistingIdsAsync(e, ids);

        int @new = 0, conflict = 0, invalid = 0;
        var rows = new List<object>();
        foreach (var r in records)
        {
            string status; string? msg = r.Error;
            if (r.Error is not null) { status = "invalid"; invalid++; }
            else if (existing.Contains(r.Id!)) { status = "conflict"; conflict++; }
            else { status = "new"; @new++; }
            rows.Add(new { sheet = r.SheetName, id = r.Id, status, message = msg });
        }

        return Ok(new
        {
            entity = e.Key,
            keyColumn = e.KeyColumn.Name,
            sheetCount = records.Count,
            rowCount = records.Count,
            newCount = @new,
            conflictCount = conflict,
            invalidCount = invalid,
            rows
        });
    }

    [HttpPost("import/commit")]
    public async Task<IActionResult> ImportCommit(string entity, [FromBody] ImportRequest req)
    {
        if (Resolve(entity) is not { } e) return NotFound(new { message = $"未知实体: {entity}" });
        if (ParseCards(e, req, out var records, out var err) is false) return BadRequest(new { message = err });

        var overwrite = new HashSet<string>(req.OverwriteIds ?? new(), StringComparer.OrdinalIgnoreCase);
        var ids = records.Where(r => !string.IsNullOrWhiteSpace(r.Id)).Select(r => r.Id!).Distinct().ToList();
        var existing = await _repo.ExistingIdsAsync(e, ids);

        int created = 0, updated = 0, skipped = 0, invalid = 0;
        var errors = new List<object>();
        foreach (var r in records)
        {
            if (r.Error is not null) { invalid++; errors.Add(new { sheet = r.SheetName, id = r.Id, message = r.Error }); continue; }
            try
            {
                if (!existing.Contains(r.Id!)) { await _repo.CreateAsync(e, r.Columns); created++; }
                else if (overwrite.Contains(r.Id!)) { await _repo.UpdateAsync(e, r.Id!, r.Columns); updated++; }
                else { skipped++; }
            }
            catch (Exception ex)
            {
                invalid++;
                errors.Add(new { sheet = r.SheetName, id = r.Id, message = ex.Message });
            }
        }

        return Ok(new { entity = e.Key, created, updated, skipped, invalid, errors });
    }

    // 读取卡片录入模板的全部数据工作表（跳过模板页），每张表解析为一条记录。
    private bool ParseCards(EntityDef e, ImportRequest req, out List<CardRecord> records, out string? error)
    {
        records = new();
        error = null;

        if (string.IsNullOrWhiteSpace(req.FilePath)) { error = "请提供 Excel 文件路径"; return false; }
        var path = req.FilePath.Trim();
        if (!System.IO.File.Exists(path)) { error = $"文件不存在: {path}"; return false; }
        var ext = Path.GetExtension(path).ToLowerInvariant();
        if (ext is not (".xlsx" or ".xlsm")) { error = $"仅支持 .xlsx / .xlsm 文件，当前为 {ext}"; return false; }

        IReadOnlyList<SheetGrid> sheets;
        try { sheets = XlsxReader.ReadSheets(path); }
        catch (Exception ex) { error = $"读取 Excel 失败: {ex.Message}"; return false; }

        foreach (var grid in sheets)
        {
            if (CardTemplateImporter.IsTemplateSheet(grid.Name)) continue;
            records.Add(CardTemplateImporter.ParseSheet(grid, e));
        }
        if (records.Count == 0) { error = "未找到任何数据工作表（仅有模板页或空文件）"; return false; }
        return true;
    }
}
