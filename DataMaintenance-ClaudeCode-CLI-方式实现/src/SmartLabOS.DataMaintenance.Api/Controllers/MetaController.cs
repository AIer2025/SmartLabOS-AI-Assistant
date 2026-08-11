using Microsoft.AspNetCore.Mvc;
using SmartLabOS.DataMaintenance.Api.Data;

namespace SmartLabOS.DataMaintenance.Api.Controllers;

/// <summary>暴露实体/列元数据，供前端动态生成列表与表单。</summary>
[ApiController]
[Route("api/meta")]
public sealed class MetaController : ControllerBase
{
    private readonly SmartLabRepository _repo;
    public MetaController(SmartLabRepository repo) => _repo = repo;

    [HttpGet]
    public IActionResult GetMeta()
    {
        var entities = EntityRegistry.All.Select(e => new
        {
            key = e.Key,
            table = e.Table,
            label = e.Label,
            columns = e.Columns.Select(c => new
            {
                name = c.Name,
                label = c.Label,
                kind = c.Kind.ToString().ToLowerInvariant(),
                inList = c.InList,
                editable = c.Editable && c.Kind != ColumnKind.ReadOnly,
                required = c.Required
            })
        });
        return Ok(new { entities });
    }

    /// <summary>健康检查：验证数据库连通性。</summary>
    [HttpGet("health")]
    public async Task<IActionResult> Health()
    {
        try
        {
            var ok = await _repo.PingAsync();
            return Ok(new { db = ok ? "connected" : "unknown" });
        }
        catch (Exception ex)
        {
            return StatusCode(503, new { db = "error", message = ex.Message });
        }
    }
}
