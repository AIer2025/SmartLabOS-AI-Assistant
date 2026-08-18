namespace Presales.Proposal.AI.Agent.Configuration;

/// <summary>
/// 售前功能涉及的服务器端路径。**唯一来源是 env/Presales-AI-Agent-config.json**：
/// <see cref="AgentEnvFile"/> 在启动时把该文件映射成 "Presales:*" 配置键并注入最高优先级层，
/// 本类只负责读取与补全（模版文件、references 根等文件里没写的派生路径）。
/// 部署时只需改那一个 JSON、无需重新编译。
/// </summary>
public sealed class PresalesPaths
{
    /// <summary>项目根（包含 references/、potocol/、projects/）。由 Module_Path 反推。</summary>
    public string ProjectRoot { get; }
    /// <summary>提案输出根目录 projects/ ← <c>Proposal_Output_Path</c>。</summary>
    public string ProjectsDir { get; }
    /// <summary>流程标准 MD 目录 potocol/MD ← <c>Protocol_Path</c>。</summary>
    public string ProtocolDir { get; }
    /// <summary>知识库根目录 references/。由 Module_Path 反推（其上级）。</summary>
    public string ReferencesDir { get; }
    /// <summary>模块知识库目录 ← <c>Module_Path</c>（共97个模块卡片 MD）。</summary>
    public string ModulesDir { get; }
    /// <summary>平台知识库目录 ← <c>Platform_Path</c>（平台抽象类卡片）。</summary>
    public string PlatformsDir { get; }
    /// <summary>托盘知识库目录 ← <c>Pallet_Path</c>。</summary>
    public string PalletDir { get; }
    /// <summary>
    /// 提案模版目录 ← <c>Proposal_Template</c>（references/09-ProposalTemplate）。
    /// 内含各类交付物的真实样例（解决方案 HTML、模块 URS HTML、详细设计方案 MD/DOCX、输出总结 MD），
    /// 由 <see cref="Agent.ProposalTemplateSet"/> 按角色归类，是 HTML/MD/WORD 输出格式的唯一权威来源。
    /// </summary>
    public string ProposalTemplateDir { get; }
    /// <summary>
    /// 标准/模版文件目录 ← <c>Template_Path</c>（references/_templates）。下述三个文件都取自这里。
    /// 配置省略时按 references/_templates 推导——但知识库目录一旦挪出默认布局，推导会失效且不报错，
    /// 因此部署时建议在配置文件里写明。注意本目录**需可写**：缺 WORD 参考样式时程序会往这里生成一份。
    /// </summary>
    public string TemplatesDir { get; }
    /// <summary>提案 HTML 输出模版文件（提案模版目录缺 HTML 样例时的兜底）。</summary>
    public string TemplateFile { get; }
    /// <summary>《详细设计方案》章节范围标准（★必备/○按需），注入系统前缀约束阶段二产出。</summary>
    public string DetailTemplateFile { get; }
    /// <summary>WORD 参考样式文档：承载中文字体、标题层级、表格样式、页眉页脚页码。缺失时由程序自动生成。</summary>
    public string ReferenceDocxFile { get; }

    /// <summary>
    /// 允许模型只读访问的知识库根集合（去重、去掉被其它根包含的子目录）。
    /// 配置文件把某个目录指到 references/ 之外时，这里会自动把它加进白名单——
    /// 否则模型按提示词去读却被 <see cref="Agent.FileTools"/> 判越界，方案就会缺参数。
    /// </summary>
    public IReadOnlyList<string> KnowledgeRoots { get; }

    /// <summary>
    /// 已废弃、不得作为选型/写作依据的知识库子目录。<see cref="Agent.FileTools"/> 会把它们
    /// 从目录列表中隐去并拒绝读取，提示词中也不再出现——避免模型引用过时资料。
    /// 默认 references/04-solutions；可用配置键 <c>Presales:DeprecatedDirs</c>（分号分隔）覆盖。
    /// </summary>
    public IReadOnlyList<string> DeprecatedDirs { get; }

    public PresalesPaths(IConfiguration config)
    {
        var s = config.GetSection("Presales");
        ProjectRoot   = Norm(s["ProjectRoot"])   ?? @"C:\TestClaude\SmartLabOS-AI-Assistant";
        ProjectsDir   = Norm(s["ProjectsDir"])   ?? Path.Combine(ProjectRoot, "projects");
        ProtocolDir   = Norm(s["ProtocolDir"])   ?? Path.Combine(ProjectRoot, "potocol", "MD");
        ReferencesDir = Norm(s["ReferencesDir"]) ?? Path.Combine(ProjectRoot, "references");
        ModulesDir    = Norm(s["ModulesDir"])    ?? Path.Combine(ReferencesDir, "01-modules");
        PlatformsDir  = Norm(s["PlatformsDir"])  ?? Path.Combine(ReferencesDir, "02-platforms");
        PalletDir     = Norm(s["PalletDir"])     ?? Path.Combine(ReferencesDir, "06-pallet");
        ProposalTemplateDir = Norm(s["ProposalTemplateDir"])
            ?? Path.Combine(ReferencesDir, "09-ProposalTemplate");
        TemplatesDir  = Norm(s["TemplatesDir"])   ?? Path.Combine(ReferencesDir, "_templates");
        TemplateFile  = Norm(s["TemplateFile"])
            ?? Path.Combine(TemplatesDir, "99-SmartLabOS-技术提案输出模版.html");
        DetailTemplateFile = Norm(s["DetailTemplateFile"])
            ?? Path.Combine(TemplatesDir, "02-SmartLabOS提案标准-详细设计方案版.md");
        ReferenceDocxFile = Norm(s["ReferenceDocxFile"])
            ?? Path.Combine(TemplatesDir, "98-SmartLabOS-WORD参考样式.docx");

        KnowledgeRoots = Consolidate(
            ReferencesDir, ProtocolDir, ModulesDir, PlatformsDir, PalletDir, ProposalTemplateDir, TemplatesDir);

        DeprecatedDirs = (Norm2(s["DeprecatedDirs"]) ?? Path.Combine(ReferencesDir, "04-solutions"))
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(d => { try { return Path.GetFullPath(d); } catch { return d; } })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>与 <see cref="Norm"/> 同，但不做 GetFullPath（值可能是分号分隔的多路径）。</summary>
    private static string? Norm2(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    /// <summary>该路径是否落在已废弃目录内（含目录本身）。</summary>
    public bool IsDeprecated(string full) =>
        DeprecatedDirs.Any(d => full.Equals(d, StringComparison.OrdinalIgnoreCase) || IsUnder(full, d));

    private static string? Norm(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        var p = path.Trim();
        try { return Path.GetFullPath(p); } catch { return p; }
    }

    /// <summary>规范化 + 去重 + 剔除已被其它根包含的目录，得到最小读白名单。</summary>
    private static IReadOnlyList<string> Consolidate(params string[] dirs)
    {
        var full = dirs.Where(d => !string.IsNullOrWhiteSpace(d))
                       .Select(d => { try { return Path.GetFullPath(d); } catch { return d; } })
                       .Distinct(StringComparer.OrdinalIgnoreCase)
                       .ToList();
        return full.Where(d => !full.Any(other => !ReferenceEquals(other, d) && IsUnder(d, other)))
                   .ToList();
    }

    private static bool IsUnder(string child, string parent) =>
        !child.Equals(parent, StringComparison.OrdinalIgnoreCase)
        && child.StartsWith(parent.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar,
                            StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// 给模型看的路径写法：位于项目根之下时用正斜杠相对路径（references/01-modules），
    /// 否则给绝对路径。<see cref="Agent.FileTools"/> 两种写法都能解析。
    /// </summary>
    public string Display(string dir)
    {
        try
        {
            var full = Path.GetFullPath(dir);
            if (IsUnder(full, Path.GetFullPath(ProjectRoot)))
                return Path.GetRelativePath(ProjectRoot, full).Replace('\\', '/');
            return full;
        }
        catch { return dir; }
    }

    /// <summary>返回某项目的输出目录（projects/&lt;name&gt;）。</summary>
    public string ProjectDir(string projectName) => Path.Combine(ProjectsDir, projectName);
}

/// <summary>
/// 大模型 Agent 的运行参数。API Key 从 env 配置文件 / 环境变量 / 用户机密读取，绝不硬编码入代码仓。
///
/// **接入方式**：本项目通过 Anthropic Messages API 的请求形状访问大模型，
/// 由 <see cref="BaseUrl"/> 决定实际打到哪家：
///   - 留空            → Anthropic 官方 api.anthropic.com
///   - DeepSeek 兼容端点 → https://api.deepseek.com/anthropic
/// 因此"换模型"只是改配置，不需要重新编译，也能随时切回去。
/// </summary>
public sealed class AgentSettings
{
    public const string ProviderDeepSeek = "deepseek";
    public const string ProviderAnthropic = "anthropic";

    /// <summary>DeepSeek 的 Anthropic 兼容端点。</summary>
    public const string DeepSeekBaseUrl = "https://api.deepseek.com/anthropic";

    /// <summary>
    /// 供应商：deepseek（默认）| anthropic。决定密钥来源、努力档位取值范围，
    /// 以及是否发送 DeepSeek 兼容层不支持的字段（结构化输出 / cache_control / redacted_thinking）。
    /// </summary>
    public string Provider { get; init; } = ProviderDeepSeek;

    /// <summary>接入点。留空走 Anthropic 官方；deepseek 供应商下默认取 <see cref="DeepSeekBaseUrl"/>。</summary>
    public string? BaseUrl { get; init; }

    /// <summary>是否为 DeepSeek 兼容端点（多处按此分流，集中在一个属性避免各处字符串比较跑偏）。</summary>
    public bool IsDeepSeek => string.Equals(Provider, ProviderDeepSeek, StringComparison.OrdinalIgnoreCase);

    /// <summary>模型 ID。DeepSeek 用 deepseek-v4-pro；Anthropic 用 claude-opus-4-8。</summary>
    public string Model { get; init; } = "deepseek-v4-pro";
    /// <summary>
    /// 努力档位 low|medium|high|xhigh|max。xhigh 是 Anthropic 自 Opus 4.7 起的专有档位，
    /// DeepSeek 兼容层是否接受未经验证，故 deepseek 下默认降到 high。
    /// </summary>
    public string Effort { get; init; } = "high";
    /// <summary>
    /// 思考模式 adaptive|off。adaptive 是 Anthropic 的自适应思考配置；
    /// DeepSeek 兼容层声明支持 thinking 模式但未说明是否识别该形状，故 deepseek 下默认 off
    /// （V4 Pro 本身即推理模型，不发该字段不影响推理，只是少一个可能被拒的未知字段）。
    /// </summary>
    public string Thinking { get; init; } = "off";
    /// <summary>单次 API 调用的最大输出 token。长文档(HTML/详细设计)需要较大值；SDK 会按此值放大 HTTP 超时。</summary>
    public int MaxTokens { get; init; } = 64000;
    /// <summary>整个生成任务的超时分钟数。</summary>
    public int TimeoutMinutes { get; init; } = 60;
    /// <summary>单阶段 agent 循环的最大工具往返轮数（防失控）。</summary>
    public int MaxToolIterations { get; init; } = 40;
    /// <summary>并发生成任务上限（保护后端与 API 限额）。</summary>
    public int MaxConcurrency { get; init; } = 2;
    /// <summary>单次模型调用的最大尝试次数（应对服务端过载/限流；流中途的错误 SDK 不会自动重试）。</summary>
    public int MaxAttempts { get; init; } = 5;
    /// <summary>
    /// 提示词缓存 TTL："1h"（默认，长思考轮不会击穿）或 "5m"（写入便宜 1.25× 而非 2×）。
    /// **仅 anthropic 供应商有效**：DeepSeek 兼容层忽略 cache_control，改由其磁盘自动缓存接管
    /// （按前缀完全匹配、尽力而为、不保证命中，无需也无法在请求里配置）。
    /// </summary>
    public string CacheTtl { get; init; } = "1h";
    /// <summary>是否把《详细设计方案》Markdown 额外导出为 .docx。</summary>
    public bool EnableDocx { get; init; } = true;
    /// <summary>pandoc 可执行文件路径（留空则尝试 PATH 中的 pandoc；不可用时回退内置 OpenXML 转换器）。</summary>
    public string? PandocPath { get; init; }

    /// <summary>Key 解析结果：值 + 来源。来源用于启动自检日志，定位「读到的不是我设的那把」。</summary>
    public readonly record struct ApiKeyInfo(string? Key, string Source);

    /// <summary>
    /// 按供应商解析 API Key。返回 Key=null 表示未配置——启动不报错，仅在真正调用生成时给出清晰错误。
    ///
    /// **deepseek**：env 配置文件的 <c>DeepSeek_Env:DeepSeek_API_Key</c>（映射为 Agent:ApiKey，配置层优先级最高）
    ///               → 环境变量 DEEPSEEK_API_KEY。
    ///               刻意**不读** ANTHROPIC_API_KEY：这台机器上很可能还残留着旧的 Anthropic 密钥，
    ///               若按老逻辑让环境变量优先，就会拿 sk-ant-… 去打 DeepSeek，只能得到一个语焉不详的 401。
    /// **anthropic**：环境变量 ANTHROPIC_API_KEY → 配置节 Agent:ApiKey（user-secrets）。保持改造前的行为不变。
    /// </summary>
    public static ApiKeyInfo ResolveApiKeyInfo(IConfiguration config, string provider)
    {
        if (string.Equals(provider, ProviderAnthropic, StringComparison.OrdinalIgnoreCase))
        {
            var envKey = Clean(Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY"));
            if (envKey is not null) return new(envKey, "环境变量 ANTHROPIC_API_KEY");

            var cfgKey = Clean(config["Agent:ApiKey"]);
            if (cfgKey is not null) return new(cfgKey, "配置节 Agent:ApiKey");
            return new(null, "未配置");
        }

        var fromFile = Clean(config["Agent:ApiKey"]);
        if (fromFile is not null) return new(fromFile, "env 配置文件 DeepSeek_Env:DeepSeek_API_Key");

        var fromEnv = Clean(Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY"));
        if (fromEnv is not null) return new(fromEnv, "环境变量 DEEPSEEK_API_KEY");

        return new(null, "未配置");
    }

    public static string? ResolveApiKey(IConfiguration config, string provider) =>
        ResolveApiKeyInfo(config, provider).Key;

    /// <summary>去掉首尾空白/换行与成对引号：setx 与复制粘贴常带入，直接导致 401 invalid x-api-key。</summary>
    private static string? Clean(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        var v = s.Trim();
        if (v.Length >= 2 && ((v[0] == '"' && v[^1] == '"') || (v[0] == '\'' && v[^1] == '\'')))
            v = v[1..^1].Trim();
        return v.Length == 0 ? null : v;
    }

    /// <summary>
    /// 脱敏描述，仅用于启动自检日志（不输出完整密钥）。
    /// 额外做一次**前缀与供应商是否对得上**的校验：这台机器上 Anthropic 与 DeepSeek 的密钥可能同时存在，
    /// 拿错了在调用时只会看到一句 401，而在启动日志里点破则几秒钟就能定位。
    /// </summary>
    public static string Describe(ApiKeyInfo info, string provider)
    {
        if (info.Key is null)
            return string.Equals(provider, ProviderAnthropic, StringComparison.OrdinalIgnoreCase)
                ? "未配置（设置环境变量 ANTHROPIC_API_KEY 或 user-secrets Agent:ApiKey 后方可生成）"
                : "未配置（在 env 配置文件 DeepSeek_Env:DeepSeek_API_Key 填入密钥，或设置环境变量 DEEPSEEK_API_KEY）";

        var desc = $"已配置（来源={info.Source}, 长度={info.Key.Length}, {Mask(info.Key)}）";
        var isAnthropicKey = info.Key.StartsWith("sk-ant-", StringComparison.OrdinalIgnoreCase);
        var wantsAnthropic = string.Equals(provider, ProviderAnthropic, StringComparison.OrdinalIgnoreCase);

        if (!wantsAnthropic && isAnthropicKey)
            desc += "  ⚠ 这是一把 Anthropic 密钥（sk-ant- 开头），但当前供应商是 DeepSeek —— 调用必然 401，请核对。";
        else if (wantsAnthropic && !isAnthropicKey)
            desc += "  ⚠ 当前供应商是 Anthropic，但密钥不是 sk-ant- 开头 —— 请核对。";

        return desc;
    }

    private static string Mask(string k) => k.Length <= 12 ? "***" : $"{k[..8]}…{k[^4..]}";

    public static AgentSettings FromConfig(IConfiguration config)
    {
        var s = config.GetSection("Agent");

        var provider = Blank(s["Provider"]) ? ProviderDeepSeek : s["Provider"]!.Trim().ToLowerInvariant();
        if (provider is not (ProviderDeepSeek or ProviderAnthropic)) provider = ProviderDeepSeek;
        var isDeepSeek = provider == ProviderDeepSeek;

        // BaseUrl 留空时按供应商取默认值：deepseek → 兼容端点；anthropic → 留空走官方。
        // 显式写 "-" 表示"强制留空"，用于极少数需要 deepseek 供应商语义但自建网关的场景。
        var baseUrl = Blank(s["BaseUrl"])
            ? (isDeepSeek ? DeepSeekBaseUrl : null)
            : (s["BaseUrl"]!.Trim() == "-" ? null : s["BaseUrl"]!.Trim());

        return new AgentSettings
        {
            Provider = provider,
            BaseUrl  = baseUrl,
            Model    = Blank(s["Model"])  ? (isDeepSeek ? "deepseek-v4-pro" : "claude-opus-4-8") : s["Model"]!.Trim(),
            Effort   = Blank(s["Effort"]) ? (isDeepSeek ? "high" : "xhigh")                      : s["Effort"]!.Trim(),
            Thinking = Blank(s["Thinking"]) ? (isDeepSeek ? "off" : "adaptive")                  : s["Thinking"]!.Trim(),
            MaxTokens         = Int(s["MaxTokens"], 64000),
            TimeoutMinutes    = Int(s["TimeoutMinutes"], 60),
            MaxToolIterations = Int(s["MaxToolIterations"], 40),
            MaxConcurrency    = Int(s["MaxConcurrency"], 2),
            MaxAttempts       = Int(s["MaxAttempts"], 5),
            CacheTtl          = string.Equals(s["CacheTtl"]?.Trim(), "5m", StringComparison.OrdinalIgnoreCase) ? "5m" : "1h",
            EnableDocx        = !string.Equals(s["EnableDocx"], "false", StringComparison.OrdinalIgnoreCase),
            PandocPath        = Blank(s["PandocPath"]) ? null : s["PandocPath"]!.Trim(),
        };
    }

    /// <summary>启动自检用的一行运行参数摘要。</summary>
    public string Summary() =>
        $"供应商={Provider}  接入点={(string.IsNullOrWhiteSpace(BaseUrl) ? "api.anthropic.com（官方默认）" : BaseUrl)}  "
      + $"模型={Model}  努力档位={Effort}  思考={Thinking}  MaxTokens={MaxTokens}  "
      + $"缓存={(IsDeepSeek ? "DeepSeek 自动磁盘缓存（不可配置）" : $"cache_control TTL={CacheTtl}")}";

    private static bool Blank(string? s) => string.IsNullOrWhiteSpace(s);
    private static int Int(string? s, int dflt) => int.TryParse(s, out var v) && v > 0 ? v : dflt;
}
