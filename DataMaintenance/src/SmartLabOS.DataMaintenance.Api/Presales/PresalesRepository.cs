using System.Text.Json;
using Dapper;
using MySqlConnector;

namespace SmartLabOS.DataMaintenance.Api.Presales;

/// <summary>
/// 售前项目需求 / 生成记录的持久化。与 SmartLabRepository 一致：无状态，按需开连接，参数化查询。
/// </summary>
public sealed class PresalesRepository
{
    private readonly string _connectionString;
    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = false };

    public PresalesRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("SmartLabOS")
            ?? throw new InvalidOperationException("未配置连接字符串 ConnectionStrings:SmartLabOS");
    }

    private MySqlConnection Open()
    {
        var conn = new MySqlConnection(_connectionString);
        conn.Open();
        return conn;
    }

    // ---------------- 项目 ----------------

    public async Task<IReadOnlyList<PresalesProject>> ListAsync()
    {
        await using var conn = Open();
        var rows = await conn.QueryAsync(
            "SELECT * FROM `SmartLabOS-PresalesProject` ORDER BY `updated_at` DESC");
        return rows.Select(MapProject).ToList();
    }

    public async Task<PresalesProject?> GetAsync(long id)
    {
        await using var conn = Open();
        var row = await conn.QueryFirstOrDefaultAsync(
            "SELECT * FROM `SmartLabOS-PresalesProject` WHERE `id` = @id", new { id });
        return row is null ? null : MapProject(row);
    }

    public async Task<bool> NameExistsAsync(string projectName, long? exceptId = null)
    {
        await using var conn = Open();
        var n = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM `SmartLabOS-PresalesProject` WHERE `project_name` = @projectName AND (@exceptId IS NULL OR `id` <> @exceptId)",
            new { projectName, exceptId });
        return n > 0;
    }

    public async Task<long> CreateAsync(PresalesProject p)
    {
        await using var conn = Open();
        var sql = """
            INSERT INTO `SmartLabOS-PresalesProject`
              (`project_name`,`project_dir`,`current_status`,`protocols`,`challenges`,
               `expectations`,`process_scope`,`loading_method`,`software_type`,`gen_status`)
            VALUES
              (@ProjectName,@ProjectDir,@CurrentStatus,@Protocols,@Challenges,
               @Expectations,@ProcessScope,@LoadingMethod,@SoftwareType,@GenStatus);
            SELECT LAST_INSERT_ID();
            """;
        return await conn.ExecuteScalarAsync<long>(sql, ToArgs(p));
    }

    public async Task UpdateAsync(PresalesProject p)
    {
        await using var conn = Open();
        var sql = """
            UPDATE `SmartLabOS-PresalesProject` SET
              `current_status`=@CurrentStatus, `protocols`=@Protocols, `challenges`=@Challenges,
              `expectations`=@Expectations, `process_scope`=@ProcessScope,
              `loading_method`=@LoadingMethod, `software_type`=@SoftwareType
            WHERE `id`=@Id;
            """;
        await conn.ExecuteAsync(sql, ToArgs(p));
    }

    public async Task<bool> DeleteAsync(long id)
    {
        await using var conn = Open();
        var n = await conn.ExecuteAsync(
            "DELETE FROM `SmartLabOS-PresalesProject` WHERE `id` = @id", new { id });
        return n > 0;
    }

    public async Task SetGenStatusAsync(long id, string status, string? commandFile = null, bool markGenerated = false)
    {
        await using var conn = Open();
        await conn.ExecuteAsync("""
            UPDATE `SmartLabOS-PresalesProject` SET
              `gen_status`=@status,
              `last_command_file`=COALESCE(@commandFile, `last_command_file`),
              `last_generated_at`=CASE WHEN @markGenerated THEN CURRENT_TIMESTAMP ELSE `last_generated_at` END
            WHERE `id`=@id;
            """, new { id, status, commandFile, markGenerated });
    }

    // ---------------- 生成记录 ----------------

    public async Task<long> CreateGenerationAsync(PresalesGeneration g)
    {
        await using var conn = Open();
        var sql = """
            INSERT INTO `SmartLabOS-PresalesGeneration`
              (`project_id`,`project_name`,`command_file`,`status`,`started_at`)
            VALUES (@ProjectId,@ProjectName,@CommandFile,@Status,CURRENT_TIMESTAMP);
            SELECT LAST_INSERT_ID();
            """;
        return await conn.ExecuteScalarAsync<long>(sql, g);
    }

    public async Task FinishGenerationAsync(long genId, string status, int? exitCode,
        IEnumerable<string> outputFiles, string? logExcerpt)
    {
        await using var conn = Open();
        await conn.ExecuteAsync("""
            UPDATE `SmartLabOS-PresalesGeneration` SET
              `status`=@status, `exit_code`=@exitCode, `output_files`=@files,
              `log_excerpt`=@logExcerpt, `finished_at`=CURRENT_TIMESTAMP
            WHERE `id`=@genId;
            """,
            new { genId, status, exitCode, files = JsonSerializer.Serialize(outputFiles, JsonOpts), logExcerpt });
    }

    public async Task<IReadOnlyList<PresalesGeneration>> ListGenerationsAsync(long projectId)
    {
        await using var conn = Open();
        var rows = await conn.QueryAsync(
            "SELECT * FROM `SmartLabOS-PresalesGeneration` WHERE `project_id`=@projectId ORDER BY `id` DESC LIMIT 50",
            new { projectId });
        return rows.Select(MapGeneration).ToList();
    }

    // ---------------- 映射 ----------------

    private DynamicParameters ToArgs(PresalesProject p)
    {
        var a = new DynamicParameters();
        a.Add("@Id", p.Id);
        a.Add("@ProjectName", p.ProjectName);
        a.Add("@ProjectDir", p.ProjectDir);
        a.Add("@CurrentStatus", p.CurrentStatus);
        a.Add("@Protocols", JsonSerializer.Serialize(p.Protocols, JsonOpts));
        a.Add("@Challenges", JsonSerializer.Serialize(p.Challenges, JsonOpts));
        a.Add("@Expectations", JsonSerializer.Serialize(p.Expectations, JsonOpts));
        a.Add("@ProcessScope", JsonSerializer.Serialize(p.ProcessScope, JsonOpts));
        a.Add("@LoadingMethod", p.LoadingMethod);
        a.Add("@SoftwareType", p.SoftwareType);
        a.Add("@GenStatus", p.GenStatus);
        return a;
    }

    private static PresalesProject MapProject(dynamic r)
    {
        var d = (IDictionary<string, object?>)r;
        return new PresalesProject
        {
            Id = Convert.ToInt64(d["id"]),
            ProjectName = Str(d, "project_name") ?? "",
            ProjectDir = Str(d, "project_dir"),
            CurrentStatus = Str(d, "current_status"),
            Protocols = JsonList(d, "protocols"),
            Challenges = JsonList(d, "challenges"),
            Expectations = JsonList(d, "expectations"),
            ProcessScope = JsonList(d, "process_scope"),
            LoadingMethod = Str(d, "loading_method"),
            SoftwareType = Str(d, "software_type"),
            GenStatus = Str(d, "gen_status") ?? "draft",
            LastCommandFile = Str(d, "last_command_file"),
            LastGeneratedAt = DateStr(d, "last_generated_at"),
            CreatedAt = DateStr(d, "created_at"),
            UpdatedAt = DateStr(d, "updated_at"),
        };
    }

    private static PresalesGeneration MapGeneration(dynamic r)
    {
        var d = (IDictionary<string, object?>)r;
        return new PresalesGeneration
        {
            Id = Convert.ToInt64(d["id"]),
            ProjectId = Convert.ToInt64(d["project_id"]),
            ProjectName = Str(d, "project_name"),
            CommandFile = Str(d, "command_file"),
            Status = Str(d, "status") ?? "queued",
            ExitCode = d.TryGetValue("exit_code", out var ec) && ec is not null ? Convert.ToInt32(ec) : null,
            OutputFiles = JsonList(d, "output_files"),
            LogExcerpt = Str(d, "log_excerpt"),
            StartedAt = DateStr(d, "started_at"),
            FinishedAt = DateStr(d, "finished_at"),
            CreatedAt = DateStr(d, "created_at"),
        };
    }

    private static string? Str(IDictionary<string, object?> d, string key)
        => d.TryGetValue(key, out var v) && v is not null ? v.ToString() : null;

    private static string? DateStr(IDictionary<string, object?> d, string key)
        => d.TryGetValue(key, out var v) && v is DateTime dt ? dt.ToString("yyyy-MM-dd HH:mm:ss") : Str(d, key);

    private static List<string> JsonList(IDictionary<string, object?> d, string key)
    {
        var s = Str(d, key);
        if (string.IsNullOrWhiteSpace(s)) return new();
        try
        {
            return JsonSerializer.Deserialize<List<string>>(s) ?? new();
        }
        catch { return new(); }
    }
}
