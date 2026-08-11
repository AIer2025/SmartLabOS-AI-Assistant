using System.Data;
using System.Text;
using Dapper;
using MySqlConnector;
using Presales.Proposal.AI.Agent.Configuration;

namespace SmartLabOS.DataMaintenance.Api.Data;

/// <summary>分页结果。</summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, long Total, int Page, int PageSize);

/// <summary>
/// 元数据驱动的通用仓储：对五张表执行参数化的增删改查。
/// 列名一律取自 <see cref="EntityRegistry"/>，永远不会拼接用户输入到 SQL 中，规避注入。
/// </summary>
public sealed class SmartLabRepository
{
    private readonly string _connectionString;

    public SmartLabRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("SmartLabOS")
            ?? throw new InvalidOperationException(
                $"未取得 MySQL 连接串。请检查配置文件 {AgentEnvFile.EnvDirName}/{AgentEnvFile.FileName} 的 DB_Connect 节。");
    }

    private MySqlConnection Open()
    {
        var conn = new MySqlConnection(_connectionString);
        conn.Open();
        return conn;
    }

    public async Task<bool> PingAsync()
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        var one = await conn.ExecuteScalarAsync<int>("SELECT 1");
        return one == 1;
    }

    private static string Cols(EntityDef e) => string.Join(", ", e.ColumnNames.Select(c => $"`{c}`"));

    /// <summary>列表查询（支持关键字模糊检索 + 分页）。返回字典行，键为列名。</summary>
    public async Task<PagedResult<Dictionary<string, object?>>> ListAsync(
        EntityDef e, string? search, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 500 ? 50 : pageSize;
        var offset = (page - 1) * pageSize;

        var where = new StringBuilder();
        var args = new DynamicParameters();
        if (!string.IsNullOrWhiteSpace(search))
        {
            // 仅在受控的列上做 LIKE 检索
            var searchable = e.Columns
                .Where(c => c.Kind is ColumnKind.Key or ColumnKind.Text)
                .Select(c => c.Name)
                .ToList();
            var ors = searchable.Select((c, i) => $"`{c}` LIKE @kw").ToList();
            where.Append(" WHERE (").Append(string.Join(" OR ", ors)).Append(')');
            args.Add("@kw", $"%{search.Trim()}%");
        }

        await using var conn = Open();
        var total = await conn.ExecuteScalarAsync<long>(
            $"SELECT COUNT(*) FROM `{e.Table}`{where}", args);

        args.Add("@limit", pageSize);
        args.Add("@offset", offset);
        var rows = await conn.QueryAsync(
            $"SELECT {Cols(e)} FROM `{e.Table}`{where} ORDER BY `id` LIMIT @limit OFFSET @offset", args);

        var items = rows.Select(r => ((IDictionary<string, object?>)r)
                          .ToDictionary(kv => kv.Key, kv => Normalize(kv.Value)))
                        .ToList();
        return new PagedResult<Dictionary<string, object?>>(items, total, page, pageSize);
    }

    public async Task<Dictionary<string, object?>?> GetAsync(EntityDef e, string id)
    {
        await using var conn = Open();
        var row = await conn.QueryFirstOrDefaultAsync(
            $"SELECT {Cols(e)} FROM `{e.Table}` WHERE `id` = @id", new { id });
        if (row is null) return null;
        return ((IDictionary<string, object?>)row)
               .ToDictionary(kv => kv.Key, kv => Normalize(kv.Value));
    }

    public async Task<bool> ExistsAsync(EntityDef e, string id)
    {
        await using var conn = Open();
        return await conn.ExecuteScalarAsync<int>(
            $"SELECT COUNT(*) FROM `{e.Table}` WHERE `id` = @id", new { id }) > 0;
    }

    /// <summary>批量取整行（含 raw_frontmatter / body_markdown），供 YAML-MD 导出。
    /// ids 为 null 取全表，否则仅取指定主键。按 id 升序。</summary>
    public async Task<IReadOnlyList<Dictionary<string, object?>>> GetManyAsync(
        EntityDef e, IReadOnlyList<string>? ids)
    {
        await using var conn = Open();
        string sql;
        object? args = null;
        if (ids is null)
        {
            sql = $"SELECT {Cols(e)} FROM `{e.Table}` ORDER BY `id`";
        }
        else
        {
            if (ids.Count == 0) return Array.Empty<Dictionary<string, object?>>();
            sql = $"SELECT {Cols(e)} FROM `{e.Table}` WHERE `id` IN @ids ORDER BY `id`";
            args = new { ids };
        }
        var rows = await conn.QueryAsync(sql, args);
        return rows.Select(r => ((IDictionary<string, object?>)r)
                      .ToDictionary(kv => kv.Key, kv => Normalize(kv.Value)))
                   .ToList();
    }

    /// <summary>返回给定主键中已存在于库中的集合，供导入冲突检测。</summary>
    public async Task<HashSet<string>> ExistingIdsAsync(EntityDef e, IReadOnlyList<string> ids)
    {
        if (ids.Count == 0) return new HashSet<string>();
        await using var conn = Open();
        var found = await conn.QueryAsync<string>(
            $"SELECT `id` FROM `{e.Table}` WHERE `id` IN @ids", new { ids });
        return found.ToHashSet();
    }

    public async Task CreateAsync(EntityDef e, IDictionary<string, object?> input)
    {
        var cols = e.EditableColumns.Append(e.KeyColumn)
                    .Select(c => c.Name).Distinct().ToList();
        var args = BuildArgs(e, cols, input);
        var colSql = string.Join(", ", cols.Select(c => $"`{c}`"));
        var valSql = string.Join(", ", cols.Select(c => $"@{c}"));
        await using var conn = Open();
        await conn.ExecuteAsync($"INSERT INTO `{e.Table}` ({colSql}) VALUES ({valSql})", args);
    }

    /// <summary>更新。onlyColumns 为 null 时更新全部可编辑列；否则仅更新其中给定的列
    /// （用于 Excel 导入"覆盖"：只改表格里出现的字段，不抹掉其它已有数据）。</summary>
    public async Task<bool> UpdateAsync(EntityDef e, string id, IDictionary<string, object?> input,
        IReadOnlyCollection<string>? onlyColumns = null)
    {
        var cols = e.EditableColumns.Select(c => c.Name)
                    .Where(c => c != e.KeyColumn.Name).ToList();
        if (onlyColumns is not null)
        {
            var set = new HashSet<string>(onlyColumns, StringComparer.OrdinalIgnoreCase);
            cols = cols.Where(set.Contains).ToList();
        }
        if (cols.Count == 0) return false;           // 无可更新列
        var args = BuildArgs(e, cols, input);
        args.Add("@id", id);
        var setSql = string.Join(", ", cols.Select(c => $"`{c}` = @{c}"));
        await using var conn = Open();
        var affected = await conn.ExecuteAsync(
            $"UPDATE `{e.Table}` SET {setSql} WHERE `id` = @id", args);
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(EntityDef e, string id)
    {
        await using var conn = Open();
        var affected = await conn.ExecuteAsync(
            $"DELETE FROM `{e.Table}` WHERE `id` = @id", new { id });
        return affected > 0;
    }

    // 仅接受已登记列；JSON 列空串转 NULL，其余空串保留。
    private static DynamicParameters BuildArgs(EntityDef e, IEnumerable<string> cols, IDictionary<string, object?> input)
    {
        var byName = e.Columns.ToDictionary(c => c.Name, c => c);
        var args = new DynamicParameters();
        foreach (var col in cols)
        {
            input.TryGetValue(col, out var raw);
            var def = byName[col];
            object? value = raw is null ? null : raw.ToString();
            if (def.Kind == ColumnKind.Json && value is string s && string.IsNullOrWhiteSpace(s))
                value = null;
            args.Add("@" + col, value);
        }
        return args;
    }

    // DateTime -> ISO 字符串；byte[](JSON 列有时回 byte[]) -> 字符串。
    private static object? Normalize(object? v) => v switch
    {
        null => null,
        DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss"),
        byte[] b => Encoding.UTF8.GetString(b),
        _ => v
    };
}
