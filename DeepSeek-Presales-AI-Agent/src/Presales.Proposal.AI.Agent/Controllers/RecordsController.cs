using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SmartLabOS.DataMaintenance.Api.Data;

namespace SmartLabOS.DataMaintenance.Api.Controllers;

/// <summary>
/// 五张主数据表的通用 CRUD 接口。
///   GET    /api/{entity}?search=&amp;page=&amp;pageSize=   列表/查询
///   GET    /api/{entity}/{id}                          单条
///   POST   /api/{entity}                               新增
///   PUT    /api/{entity}/{id}                          修改
///   DELETE /api/{entity}/{id}                          删除
/// {entity} ∈ modules | platforms | workstations | solutions | projects
/// </summary>
[ApiController]
[Route("api/{entity}")]
public sealed class RecordsController : ControllerBase
{
    private readonly SmartLabRepository _repo;
    public RecordsController(SmartLabRepository repo) => _repo = repo;

    private EntityDef? Resolve(string entity) => EntityRegistry.Find(entity);

    [HttpGet]
    public async Task<IActionResult> List(string entity, [FromQuery] string? search,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        if (Resolve(entity) is not { } e) return NotFound(new { message = $"未知实体: {entity}" });
        var result = await _repo.ListAsync(e, search, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string entity, string id)
    {
        if (Resolve(entity) is not { } e) return NotFound(new { message = $"未知实体: {entity}" });
        var row = await _repo.GetAsync(e, id);
        return row is null ? NotFound(new { message = $"记录不存在: {id}" }) : Ok(row);
    }

    [HttpPost]
    public async Task<IActionResult> Create(string entity, [FromBody] JsonElement body)
    {
        if (Resolve(entity) is not { } e) return NotFound(new { message = $"未知实体: {entity}" });
        var input = ToDict(body);

        var idCol = e.KeyColumn.Name;
        if (!input.TryGetValue(idCol, out var idVal) || string.IsNullOrWhiteSpace(idVal as string))
            return BadRequest(new { message = $"缺少必填主键字段: {idCol}" });

        var id = ((string)idVal!).Trim();
        if (await _repo.ExistsAsync(e, id))
            return Conflict(new { message = $"主键已存在: {id}" });

        if (ValidateJson(e, input) is { } jsonErr) return BadRequest(new { message = jsonErr });

        await _repo.CreateAsync(e, input);
        var created = await _repo.GetAsync(e, id);
        return CreatedAtAction(nameof(Get), new { entity, id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string entity, string id, [FromBody] JsonElement body)
    {
        if (Resolve(entity) is not { } e) return NotFound(new { message = $"未知实体: {entity}" });
        if (!await _repo.ExistsAsync(e, id)) return NotFound(new { message = $"记录不存在: {id}" });

        var input = ToDict(body);
        if (ValidateJson(e, input) is { } jsonErr) return BadRequest(new { message = jsonErr });

        await _repo.UpdateAsync(e, id, input);
        var updated = await _repo.GetAsync(e, id);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string entity, string id)
    {
        if (Resolve(entity) is not { } e) return NotFound(new { message = $"未知实体: {entity}" });
        var ok = await _repo.DeleteAsync(e, id);
        return ok ? NoContent() : NotFound(new { message = $"记录不存在: {id}" });
    }

    // ---- helpers ----
    private static Dictionary<string, object?> ToDict(JsonElement body)
    {
        var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        if (body.ValueKind != JsonValueKind.Object) return dict;
        foreach (var p in body.EnumerateObject())
        {
            dict[p.Name] = p.Value.ValueKind switch
            {
                JsonValueKind.Null => null,
                JsonValueKind.String => p.Value.GetString(),
                JsonValueKind.True or JsonValueKind.False => p.Value.GetBoolean().ToString(),
                JsonValueKind.Number => p.Value.GetRawText(),
                // 对象/数组(如 data_json 直接传对象) 原样序列化为字符串存储
                _ => p.Value.GetRawText()
            };
        }
        return dict;
    }

    // 对 JSON 列做合法性校验，避免写入非法 JSON 触发数据库错误。
    private static string? ValidateJson(EntityDef e, IDictionary<string, object?> input)
    {
        foreach (var col in e.Columns.Where(c => c.Kind == ColumnKind.Json))
        {
            if (input.TryGetValue(col.Name, out var v) && v is string s && !string.IsNullOrWhiteSpace(s))
            {
                try { using var _ = JsonDocument.Parse(s); }
                catch (JsonException ex) { return $"字段 {col.Label}({col.Name}) 不是合法 JSON: {ex.Message}"; }
            }
        }
        return null;
    }
}
