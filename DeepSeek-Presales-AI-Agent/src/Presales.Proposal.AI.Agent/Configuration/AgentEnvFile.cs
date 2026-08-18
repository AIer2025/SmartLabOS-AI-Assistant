using System.Text.Json;
using MySqlConnector;

namespace Presales.Proposal.AI.Agent.Configuration;

/// <summary>
/// <c>env/Presales-AI-Agent-config.json</c> —— 部署现场**唯一**需要修改的配置文件。
///
/// 本类把该文件解析成标准 IConfiguration 键值，注入配置系统的最高优先级层，
/// 因此下游代码（仓储、PresalesPaths）无需感知它的存在，仍按原来的键读取：
/// <list type="bullet">
///   <item><c>DB_Connect</c>     → <c>ConnectionStrings:SmartLabOS</c>（用 MySqlConnectionStringBuilder 组装，口令自动转义）</item>
///   <item><c>ClaudeAI_Env</c>   → <c>Presales:ModulesDir / PlatformsDir / PalletDir / ProtocolDir / ProjectsDir / ProposalTemplateDir</c>
///         （节名沿用历史命名，现场配置文件不必改；它只是"知识库运行目录"的意思，与具体用哪家大模型无关）</item>
///   <item>并由 Module_Path 反推 <c>Presales:ReferencesDir</c> 与 <c>Presales:ProjectRoot</c>（模版文件、路径显示、越权校验都要用）</item>
///   <item><c>DeepSeek_Env</c>   → <c>Agent:ApiKey</c>（大模型密钥。本层优先级最高，会盖过 user-secrets 与环境变量，
///         这正是想要的：现场只认这一份文件，杜绝"机器上残留的旧 ANTHROPIC_API_KEY 把新密钥顶掉"）</item>
/// </list>
///
/// 查找顺序：环境变量 <c>PRESALES_AGENT_CONFIG</c>（文件或目录）→ 内容根 / 程序目录 / 当前目录逐级向上找
/// <c>env/Presales-AI-Agent-config.json</c>。这样 IDE 调试（内容根在 src/项目下）与发布运行（bin 目录）都能找到同一份文件。
///
/// 找不到或解析失败**不阻断启动**：启动自检会打出醒目错误，真正用到数据库时再抛出指向本文件的清晰异常，
/// 避免「服务起不来、也看不到原因」。
/// </summary>
public sealed class AgentEnvFile
{
    public const string FileName = "Presales-AI-Agent-config.json";
    public const string EnvDirName = "env";
    /// <summary>可选：用环境变量显式指定配置文件（或其所在目录），优先级最高。</summary>
    public const string PathVariable = "PRESALES_AGENT_CONFIG";

    /// <summary>库名不在配置文件的必填项内，缺省沿用既有库。可在文件中加 <c>DB_Name</c> 覆盖。</summary>
    public const string DefaultDatabase = "SmartLabOS-Presales-AI";

    /// <summary>实际读到的文件绝对路径；未找到为 null。</summary>
    public string? FilePath { get; private init; }

    /// <summary>读取/解析失败的原因；成功为 null。</summary>
    public string? Error { get; private init; }

    /// <summary>映射出的配置键值，注入 IConfiguration。</summary>
    public IReadOnlyList<KeyValuePair<string, string?>> Values { get; private init; } = [];

    /// <summary>脱敏后的 MySQL 连接摘要（不含口令），供启动自检日志。</summary>
    public string DbSummary { get; private init; } = "未配置";

    /// <summary>本文件是否提供了大模型 API Key（供启动自检区分"文件给的"还是"环境变量给的"）。</summary>
    public bool HasLlmApiKey { get; private init; }

    /// <summary>解析过程中的非致命提醒（如某个路径不存在）。</summary>
    public IReadOnlyList<string> Warnings { get; private init; } = [];

    public bool Loaded => FilePath is not null && Error is null;

    /// <summary>供异常/日志引用的一句话定位提示。</summary>
    public string Location => FilePath ?? $"（未找到 {EnvDirName}/{FileName}）";

    // ------------------------------------------------------------------

    public static AgentEnvFile Load(string contentRoot)
    {
        var path = Locate(contentRoot);
        if (path is null)
            return new AgentEnvFile
            {
                Error = $"未找到配置文件 {EnvDirName}/{FileName}。" +
                        $"请将其放在项目根的 {EnvDirName}/ 目录下，或用环境变量 {PathVariable} 指定完整路径。",
            };

        string text;
        try { text = File.ReadAllText(path); }
        catch (Exception ex) { return new AgentEnvFile { FilePath = path, Error = $"读取失败：{ex.Message}" }; }

        try { return Parse(path, text); }
        catch (JsonException ex) { return new AgentEnvFile { FilePath = path, Error = $"JSON 解析失败：{ex.Message}" }; }
        catch (Exception ex) { return new AgentEnvFile { FilePath = path, Error = ex.Message }; }
    }

    private static AgentEnvFile Parse(string path, string text)
    {
        using var doc = JsonDocument.Parse(text, new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip,
        });
        var root = doc.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
            return new AgentEnvFile { FilePath = path, Error = "根节点必须是 JSON 对象。" };

        var values = new List<KeyValuePair<string, string?>>();
        var warnings = new List<string>();

        // ---------------- DB_Connect → ConnectionStrings:SmartLabOS ----------------
        var dbSummary = "未配置（缺少 DB_Connect 节）";
        if (Section(root, "DB_Connect") is { } db)
        {
            var ip   = Str(db, "DB_IP");
            var user = Str(db, "DB_User");
            var pwd  = Str(db, "DB_Password") ?? "";
            var name = Str(db, "DB_Name") ?? DefaultDatabase;
            var portText = Str(db, "DB_Port") ?? "3306";

            if (ip is null || user is null)
                return new AgentEnvFile { FilePath = path, Error = "DB_Connect 缺少必填项 DB_IP 或 DB_User。" };
            if (!uint.TryParse(portText, out var port) || port is 0 or > 65535)
                return new AgentEnvFile { FilePath = path, Error = $"DB_Port 非法：{portText}" };

            var csb = new MySqlConnectionStringBuilder
            {
                Server = ip,
                Port = port,
                Database = name,
                UserID = user,
                Password = pwd,          // 由 Builder 负责转义，口令含 ; # ' 等字符也安全
                AllowPublicKeyRetrieval = true,
                CharacterSet = Str(db, "DB_CharSet") ?? "utf8mb4",
                SslMode = ParseSslMode(Str(db, "DB_SslMode"), warnings),
            };

            values.Add(new("ConnectionStrings:SmartLabOS", csb.ConnectionString));
            dbSummary = $"{user}@{ip}:{port}/{name}（口令 {(pwd.Length == 0 ? "空" : $"已配置，{pwd.Length} 位")}）";

            // 由模板复制而来却忘了替换占位符——表现为「连不上」，报错却指向网络/授权，很难往这想。
            if (LooksLikePlaceholder(pwd)) warnings.Add($"DB_Password 看起来仍是模板占位符（{pwd}），请填入真实口令。");
            if (LooksLikePlaceholder(name)) warnings.Add($"DB_Name 看起来仍是模板占位符（{name}），请填入真实库名。");
        }

        // ---------------- ClaudeAI_Env → Presales:* ----------------
        if (Section(root, "ClaudeAI_Env") is { } env)
        {
            var modules   = Dir(env, "Module_Path",          values, "Presales:ModulesDir",          warnings);
            _             = Dir(env, "Platform_Path",        values, "Presales:PlatformsDir",        warnings);
            _             = Dir(env, "Pallet_Path",          values, "Presales:PalletDir",           warnings);
            _             = Dir(env, "Protocol_Path",        values, "Presales:ProtocolDir",         warnings);
            var projects  = Dir(env, "Proposal_Output_Path", values, "Presales:ProjectsDir",         warnings);
            _             = Dir(env, "Proposal_Template",    values, "Presales:ProposalTemplateDir", warnings);
            // Template_Path 可省略：省略时由 references/ 推导，见 PresalesPaths。
            // 但一旦知识库目录被挪出默认布局，推导会失效且不报错——所以配置文件里写明更稳。
            _             = Dir(env, "Template_Path",        values, "Presales:TemplatesDir",        warnings, optional: true);

            // references/ 与项目根不在配置文件里，但模版文件定位、路径显示、读权限校验都要用：
            // 由 Module_Path(=references/01-modules) 逐级反推，反推不出时退回输出目录的上级。
            var references = Parent(modules);
            if (references is not null) values.Add(new("Presales:ReferencesDir", references));

            var projectRoot = Parent(references) ?? Parent(projects);
            if (projectRoot is not null) values.Add(new("Presales:ProjectRoot", projectRoot));
        }
        else
        {
            warnings.Add("配置文件缺少 ClaudeAI_Env 节，知识库/输出目录将退回内置默认值。");
        }

        // ---------------- DeepSeek_Env → Agent:ApiKey ----------------
        // 大模型密钥从本文件读取，与 MySQL 口令同等对待（本文件已在 .gitignore 中排除）。
        // 兼容 DeepSeek_API_Key / Api_Key / ApiKey 三种写法，避免现场因大小写下划线写错而"静默无密钥"。
        var hasKey = false;
        if (Section(root, "DeepSeek_Env") is { } llm)
        {
            var key = Str(llm, "DeepSeek_API_Key") ?? Str(llm, "Api_Key") ?? Str(llm, "ApiKey");
            if (key is null)
            {
                warnings.Add("DeepSeek_Env 节存在但没读到 DeepSeek_API_Key，将退回环境变量 DEEPSEEK_API_KEY。");
            }
            else if (LooksLikePlaceholder(key))
            {
                warnings.Add($"DeepSeek_API_Key 看起来仍是模板占位符（{Mask(key)}），请填入真实密钥。");
            }
            else
            {
                // 首尾空白与成对引号由 AgentSettings.Clean 统一清理（复制粘贴/setx 常带入，直接导致 401）。
                values.Add(new("Agent:ApiKey", key));
                hasKey = true;
            }

            // 可选：允许现场按项目覆盖模型与接入点，不必重新编译。
            if (Str(llm, "Model") is { } model) values.Add(new("Agent:Model", model));
            if ((Str(llm, "Base_Url") ?? Str(llm, "BaseUrl")) is { } baseUrl) values.Add(new("Agent:BaseUrl", baseUrl));
            if (Str(llm, "Effort") is { } effort) values.Add(new("Agent:Effort", effort));
        }
        else
        {
            warnings.Add("配置文件缺少 DeepSeek_Env 节，大模型密钥将退回环境变量 DEEPSEEK_API_KEY。");
        }

        return new AgentEnvFile
        {
            FilePath = path,
            Values = values,
            DbSummary = dbSummary,
            HasLlmApiKey = hasKey,
            Warnings = warnings,
        };
    }

    /// <summary>密钥脱敏（仅用于警告文案，绝不输出完整值）。</summary>
    private static string Mask(string k) => k.Length <= 12 ? "***" : $"{k[..6]}…{k[^4..]}";

    // ---------------- 解析辅助 ----------------

    /// <summary>值是否像「照抄模板却没替换」的占位符。命中只警告，不阻断——现场也可能真用这种字面量。</summary>
    private static bool LooksLikePlaceholder(string? v)
    {
        if (string.IsNullOrWhiteSpace(v)) return false;
        string[] marks = ["PUT-", "在此填入", "请填入", "__SET", "YOUR-", "<", "xxxx"];
        return marks.Any(m => v.Contains(m, StringComparison.OrdinalIgnoreCase));
    }

    private static JsonElement? Section(JsonElement parent, string name) =>
        parent.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Object ? v : null;

    private static string? Str(JsonElement parent, string name)
    {
        if (!parent.TryGetProperty(name, out var v)) return null;
        var s = v.ValueKind switch
        {
            JsonValueKind.String => v.GetString(),
            JsonValueKind.Number => v.ToString(),
            _ => null,
        };
        return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }

    /// <summary>
    /// 读一个目录项：规范化为绝对路径写入 values；不存在只警告不报错（部署顺序可能先配后建）。
    /// <paramref name="optional"/> 为 true 时，键缺失不给警告——用于「省略即按约定推导」的可选项。
    /// </summary>
    private static string? Dir(JsonElement env, string jsonKey, List<KeyValuePair<string, string?>> values,
        string configKey, List<string> warnings, bool optional = false)
    {
        var raw = Str(env, jsonKey);
        if (raw is null)
        {
            if (!optional) warnings.Add($"ClaudeAI_Env 缺少 {jsonKey}，对应目录退回默认值。");
            return null;
        }

        string full;
        try { full = Path.GetFullPath(raw); }
        catch (Exception ex) { warnings.Add($"{jsonKey} 路径非法（{ex.Message}）：{raw}"); return null; }

        if (!Directory.Exists(full)) warnings.Add($"{jsonKey} 指向的目录当前不存在：{full}");
        values.Add(new(configKey, full));
        return full;
    }

    private static string? Parent(string? dir)
    {
        if (string.IsNullOrWhiteSpace(dir)) return null;
        try { return Directory.GetParent(dir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))?.FullName; }
        catch { return null; }
    }

    private static MySqlSslMode ParseSslMode(string? text, List<string> warnings)
    {
        if (text is null) return MySqlSslMode.None;              // 与改造前的连接串保持一致
        if (Enum.TryParse<MySqlSslMode>(text, ignoreCase: true, out var mode)) return mode;
        warnings.Add($"DB_SslMode 取值无法识别（{text}），已按 None 处理。");
        return MySqlSslMode.None;
    }

    // ---------------- 文件定位 ----------------

    private static string? Locate(string contentRoot)
    {
        var overridePath = Environment.GetEnvironmentVariable(PathVariable)?.Trim().Trim('"');
        if (!string.IsNullOrEmpty(overridePath))
        {
            foreach (var candidate in new[]
                     {
                         overridePath,
                         Path.Combine(overridePath, FileName),
                         Path.Combine(overridePath, EnvDirName, FileName),
                     })
            {
                try { if (File.Exists(candidate)) return Path.GetFullPath(candidate); }
                catch { /* 路径非法，继续下一候选 */ }
            }
        }

        foreach (var start in new[] { contentRoot, AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
            if (WalkUp(start) is { } hit) return hit;

        return null;
    }

    /// <summary>从 start 起逐级向上（最多 8 层）找 <c>env/文件</c> 或同目录下的文件。</summary>
    private static string? WalkUp(string? start)
    {
        if (string.IsNullOrWhiteSpace(start)) return null;

        DirectoryInfo? dir;
        try { dir = new DirectoryInfo(Path.GetFullPath(start)); }
        catch { return null; }

        for (var depth = 0; dir is not null && depth < 8; depth++, dir = dir.Parent)
        {
            var inEnv = Path.Combine(dir.FullName, EnvDirName, FileName);
            if (File.Exists(inEnv)) return inEnv;
            var direct = Path.Combine(dir.FullName, FileName);
            if (File.Exists(direct)) return direct;
        }
        return null;
    }
}
