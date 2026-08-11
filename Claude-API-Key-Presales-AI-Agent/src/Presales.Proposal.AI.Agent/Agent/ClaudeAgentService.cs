using System.Text;
using System.Text.Json;
using Anthropic;
using Anthropic.Core;
using Anthropic.Exceptions;
using Anthropic.Models.Messages;
using Presales.Proposal.AI.Agent.Configuration;
using Presales.Proposal.AI.Agent.Domain;

namespace Presales.Proposal.AI.Agent.Agent;

/// <summary>
/// SmartLabOS 售前提案 AI Agent 的大模型核心 —— 用 Anthropic 官方 C# SDK 直接调用 Messages API，
/// 取代旧实现「启动 Claude Code CLI 子进程 + 扫盘回读」。要点：
///   - 「模块选定」用 **结构化输出**(OutputConfig.Format + JSON Schema) 直接拿到机器可读结果；
///   - 「方案生成」用 **手写 Agent 工具循环**(受控 read/write 工具)，模型自主读知识库、写提案文件；
///   - 稳定的知识库上下文放入 **带缓存的 system 前缀**(CacheControlEphemeral)，跨轮/跨阶段命中降本；
///   - 强类型异常、可拿到 usage（token 用量）便于成本观测。
/// </summary>
public sealed class ClaudeAgentService
{
    private readonly AgentSettings _s;
    private readonly PresalesPaths _paths;
    private readonly ModuleCatalog _catalog;
    private readonly ProposalTemplateSet _templates;
    private readonly DocxExporter _docx;
    private readonly IConfiguration _config;
    private readonly ILogger<ClaudeAgentService> _log;

    private string? _templateCache;
    private string? _detailTemplateCache;

    /// <summary>
    /// 全局统一的提示词缓存 TTL。**必须所有 cache_control 块保持一致**：
    /// API 按 tools → system → messages 的顺序处理，且不允许 ttl='1h' 的块出现在 ttl='5m' 之后，
    /// 否则整个请求 400（曾因 system 用默认 5m、messages 用 1h 而全线失败）。
    ///
    /// 默认 1 小时而非 5 分钟：xhigh 下单轮思考可达 7 分钟以上，5 分钟 TTL 会被击穿，
    /// 导致十几万 token 的前缀整体重写（实测一次多付约 $0.84）。代价是写入单价 1.25× → 2×。
    /// 想回退只改配置 Agent:CacheTtl = "5m"，无需重新编译；两处 cache_control 都取自本方法，不会再跑偏。
    /// </summary>
    private CacheControlEphemeral Cache() =>
        string.Equals(_s.CacheTtl, "5m", StringComparison.OrdinalIgnoreCase)
            ? new CacheControlEphemeral()                        // 默认 5 分钟
            : new CacheControlEphemeral { Ttl = Ttl.Ttl1h };

    public ClaudeAgentService(AgentSettings settings, PresalesPaths paths, ModuleCatalog catalog,
        ProposalTemplateSet templates, DocxExporter docx, IConfiguration config, ILogger<ClaudeAgentService> log)
    {
        _s = settings; _paths = paths; _catalog = catalog; _templates = templates;
        _docx = docx; _config = config; _log = log;
    }

    /// <summary>API Key 是否已配置（供健康检查 / 前端提示）。</summary>
    public bool HasApiKey => !string.IsNullOrWhiteSpace(AgentSettings.ResolveApiKey(_config));

    /// <summary>Key 的脱敏描述（来源 + 长度 + 掩码），供启动自检日志排查「读到的不是我设的那把」。</summary>
    public string ApiKeyDescription => AgentSettings.Describe(AgentSettings.ResolveApiKeyInfo(_config));

    private AnthropicClient CreateClient()
    {
        var key = AgentSettings.ResolveApiKey(_config);
        if (string.IsNullOrWhiteSpace(key))
            throw new InvalidOperationException(
                "未配置 Anthropic API Key。请设置环境变量 ANTHROPIC_API_KEY，或用 user-secrets 配置 Agent:ApiKey。");
        return new AnthropicClient { ApiKey = key };
    }

    // =====================================================================
    //  一、模块选定（结构化输出）
    // =====================================================================

    /// <summary>
    /// 依据国标(protocols) + 客户需求，从知识库 97 个模块中选出实现前处理流程所需的最小充分集合。
    /// 返回经知识库校验后的真实模块ID（幻觉编号会被过滤）。
    /// </summary>
    public async Task<ModulePick> SelectModulesAsync(PresalesProject p, Action<string> log, CancellationToken ct)
    {
        var client = CreateClient();
        log("==== 模块选定：结构化输出（OutputConfig.Format）====");

        var user = BuildModuleSelectionPrompt(p, log);

        var schema = new Dictionary<string, JsonElement>
        {
            ["type"] = J("object"),
            ["properties"] = JsonSerializer.SerializeToElement(new
            {
                modules = new { type = "array", items = new { type = "string" },
                                description = "推荐模块ID数组，按前处理流程顺序排列，形如 MOD-CC-001" },
                notes = new { type = "string", description = "选型理由与未覆盖工序说明(可选)" },
            }),
            ["required"] = JsonSerializer.SerializeToElement(new[] { "modules" }),
            ["additionalProperties"] = JsonSerializer.SerializeToElement(false),
        };

        StreamedTurn turn;
        try
        {
            turn = await StreamedTurn.RunAsync(client, new MessageCreateParams
            {
                Model = _s.Model,
                MaxTokens = 16000,
                Thinking = new ThinkingConfigAdaptive(),
                System = BuildKnowledgeSystem(),
                OutputConfig = new OutputConfig { Effort = ParseEffort(_s.Effort), Format = new JsonOutputFormat { Schema = schema } },
                Messages = [ new() { Role = Role.User, Content = user } ],
            }, _s.MaxAttempts, log, ct);
        }
        catch (Exception ex)
        {
            log($"[错误] 模块选定调用失败：{ex.Message}");
            throw;
        }

        LogUsage(turn, log);
        var json = turn.Text;

        // 原始返回必须入日志：没有它，"已选 0" 就分不清是模型没给、结构化输出没落到 text 块，还是ID写法不匹配
        log($"[原文] 内容块={turn.BlockSummary} 文本长度={json.Length}");
        log($"[原文] {Flatten(Truncate(json, 1200))}");

        var pick = SafeParse<ModulePick>(json);
        var ids = _catalog.Normalize(pick?.Modules);   // 仅保留知识库中真实存在的ID

        // 兜底：模型把ID写成 "MOD-CC-001.md"/"MOD-CC-001 加液模块"，或整段JSON被包在代码围栏/说明文字里时，
        // 上面的全等匹配会全军覆没。此处退化为按 MOD-XX-000 正则从整段返回里抽取，仍按知识库校验存在性。
        if (ids.Count == 0)
        {
            ids = _catalog.ExtractFromText(json);
            if (ids.Count > 0)
                log($"[兜底] 结构化字段未匹配到有效ID，已从返回全文正则抽取到 {ids.Count} 个。");
        }

        log(ids.Count == 0
            ? "[警告] 未解析出任何有效模块ID，请改用手动「添加模块」。"
            : $"模块选定完成：推荐 {ids.Count} 个 —— {string.Join("、", ids)}");
        return new ModulePick { Modules = ids, Notes = pick?.Notes };
    }

    // =====================================================================
    //  二、方案生成（手写 Agent 工具循环）
    // =====================================================================

    /// <summary>
    /// 两阶段生成：① 解决方案 HTML + 必要的 xxx-模块-URS.html；② 详细设计方案 Markdown(.md，可再转 WORD)。
    /// setPhase(1|2) 由服务端权威给出阶段；log 累积运行日志。返回本次写出的全部文件名。
    /// </summary>
    public async Task<List<string>> GenerateProposalAsync(
        PresalesProject p, Action<int> setPhase, Action<string> log, CancellationToken ct)
    {
        var client = CreateClient();
        var tools = new FileTools(_paths, p.ProjectDir!);
        var system = BuildKnowledgeSystem();

        // ---- 阶段一：解决方案 HTML + URS ----
        setPhase(1);
        log("==== 阶段一：生成解决方案 HTML 提案与（必要时）模块 URS 文档 ====");
        await RunToolLoopAsync(client, system, tools, BuildHtmlPhasePrompt(p), "阶段一", log, ct);
        VerifyHtmlPhase(p, tools, log);

        // ---- 阶段二：详细设计方案 Markdown ----
        setPhase(2);
        log("==== 阶段二：整合 HTML，生成《详细设计方案》Markdown ====");
        var mdName = DetailMdName(p);
        await RunToolLoopAsync(client, system, tools, BuildDetailPhasePrompt(p, tools), "阶段二", log, ct);
        VerifyDetailPhaseGrounding(tools, log);
        if (!tools.OutputExists(mdName))
            throw new InvalidOperationException(
                $"阶段二未产出《详细设计方案》「{mdName}」。请查看运行日志中是否出现 max_tokens 截断或工具调用错误。");

        // ---- 阶段二收尾：把《详细设计方案》Markdown 导出为 .docx（Pandoc + 参考样式文档，失败回退内置 OpenXML）----
        var extraDocx = new List<string>();
        if (_s.EnableDocx)
        {
            var engine = _docx.Export(Path.Combine(p.ProjectDir!, mdName), log);
            var docxName = Path.ChangeExtension(mdName, ".docx");
            if (engine is not null && File.Exists(Path.Combine(p.ProjectDir!, docxName)))
                extraDocx.Add(docxName);
            else
                log($"[警告] 未能生成 {docxName}，仅交付 Markdown。");
        }

        return tools.WrittenFiles.Concat(extraDocx).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>《详细设计方案》Markdown 的固定文件名——提示词、校验、docx 导出三处共用。</summary>
    private static string DetailMdName(PresalesProject p) => $"{Sanitize(p.ProjectName)}-详细设计方案.md";

    /// <summary>
    /// 阶段一产物校验：每个流程标准应对应一份 N.html。缺失即抛出（任务判为 failed），
    /// 杜绝「输出被截断 → 零文件 → 仍报成功」的静默失败。
    /// </summary>
    private static void VerifyHtmlPhase(PresalesProject p, FileTools tools, Action<string> log)
    {
        var missing = new List<string>();
        for (int i = 0; i < p.Protocols.Count; i++)
        {
            var name = $"{i + 1}.html";
            if (!tools.OutputExists(name)) missing.Add(name);
        }

        var urs = tools.WrittenFiles.Where(f => f.Contains("URS", StringComparison.OrdinalIgnoreCase)).ToList();
        log($"[阶段一产物] 解决方案 HTML：{p.Protocols.Count - missing.Count}/{p.Protocols.Count}"
          + $"；模块 URS：{(urs.Count == 0 ? "无（模型判定已确认模块可完全覆盖）" : Join(urs))}");

        if (missing.Count > 0)
            throw new InvalidOperationException(
                $"阶段一未产出预期的解决方案 HTML：{string.Join("、", missing)}。"
                + "常见原因：单次输出超 token 上限被截断（日志中会有 max_tokens 提示），或模型未调用 write_output。");
    }

    /// <summary>
    /// 阶段二溯源校验：《详细设计方案》必须建立在阶段一产物之上。
    /// 若一份阶段一 HTML 都没被读过，说明模型脱离 HTML 独立成文——耗时/产能会与提案对不上，
    /// 属于「任务成功但内容降级」，必须在日志里明确示警。
    /// </summary>
    private static void VerifyDetailPhaseGrounding(FileTools tools, Action<string> log)
    {
        var htmlRead = tools.ReadOutputFiles
            .Where(f => f.EndsWith(".html", StringComparison.OrdinalIgnoreCase)
                     || f.EndsWith(".htm", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (htmlRead.Count > 0)
            log($"[阶段二溯源] 已回读阶段一产物：{Join(htmlRead)}");
        else
            log("[警告][阶段二溯源] 未检测到对任何阶段一 HTML 的读取！"
              + "《详细设计方案》可能脱离解决方案提案独立生成，耗时/产能/平台构成存在与提案不一致的风险，请人工复核。");
    }

    /// <summary>驱动一次「模型 → 工具执行 → 回喂结果」的手写 Agent 循环，直到模型 end_turn 或达到轮数上限。</summary>
    private async Task RunToolLoopAsync(AnthropicClient client, List<TextBlockParam> system,
        FileTools tools, string userPrompt, string phaseLabel, Action<string> log, CancellationToken ct)
    {
        var toolDefs = BuildTools();
        var messages = new List<MessageParam> { new() { Role = Role.User, Content = userPrompt } };

        for (int iter = 1; iter <= _s.MaxToolIterations; iter++)
        {
            ct.ThrowIfCancellationRequested();

            StreamedTurn turn;
            try
            {
                turn = await StreamedTurn.RunAsync(client, new MessageCreateParams
                {
                    Model = _s.Model,
                    MaxTokens = _s.MaxTokens,
                    Thinking = new ThinkingConfigAdaptive(),
                    System = system,
                    OutputConfig = new OutputConfig { Effort = ParseEffort(_s.Effort) },
                    Tools = toolDefs,
                    Messages = messages,
                    // 自动把「工具 + 系统前缀 + 已有对话」整体缓存：工具循环每轮都要重发全部历史，
                    // 不缓存的话输入成本随轮数平方增长；命中缓存后这部分只按 0.1x 计费。
                    // TTL 必须与 system 前缀一致，见 Cache() 注释。
                    CacheControl = Cache(),
                }, _s.MaxAttempts, log, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                log($"[错误] {phaseLabel} 第{iter}轮调用失败：{ex.Message}");
                throw;
            }

            LogUsage(turn, log);

            var line = turn.Text.Trim();
            if (line.Length > 0) log(Truncate(line, 500));

            var toolResults = new List<ContentBlockParam>();
            foreach (var call in turn.ToolCalls)
            {
                var result = ExecuteTool(call.Name, call.Input, tools, log);
                toolResults.Add(new ToolResultBlockParam { ToolUseID = call.Id, Content = result });
            }

            messages.Add(new MessageParam { Role = Role.Assistant, Content = turn.Assistant });

            // 输出被 max_tokens 截断：绝不能当作「本阶段完成」——那会静默产出零文件。
            // 回喂一条续写指令让模型接着写（长文件改用 append_output 分片）。
            if (turn.IsMaxTokens)
            {
                log($"[{phaseLabel}] 第{iter}轮输出达 max_tokens 上限被截断，要求模型续写（已写出：{Join(tools.WrittenFiles)}）。");
                if (toolResults.Count > 0) messages.Add(new MessageParam { Role = Role.User, Content = toolResults });
                messages.Add(new MessageParam
                {
                    Role = Role.User,
                    Content = "你上一条回复因达到单次输出上限被截断。请继续未完成的工作，注意："
                            + "(1) 不要重复已经输出过的内容；"
                            + "(2) 长文件必须分片：先 write_output 写出开头部分，再多次 append_output 追加剩余部分，"
                            + "每次追加 4000–5000 字，直到文件完整；"
                            + "(3) 全部文件写完后再做总结。",
                });
                continue;
            }

            // 无工具调用即视为本阶段完成
            if (toolResults.Count == 0)
            {
                log($"[{phaseLabel}] 完成（模型未再请求工具，stop_reason={turn.StopReason}）。");
                return;
            }
            messages.Add(new MessageParam { Role = Role.User, Content = toolResults });
        }
        log($"[{phaseLabel}] 已达工具循环上限({_s.MaxToolIterations})，停止。");
    }

    /// <summary>执行受控工具，返回给模型的 tool_result 文本。</summary>
    private static string ExecuteTool(string name, IReadOnlyDictionary<string, JsonElement> input,
        FileTools tools, Action<string> log)
    {
        switch (name)
        {
            case "list_kb_dir":
            {
                var path = Arg(input, "path");
                log($"[工具] list_kb_dir  {path}");
                return tools.ListDir(path);
            }
            case "read_kb_file":
            {
                var path = Arg(input, "path");
                log($"[工具] read_kb_file  {path}");
                return tools.ReadFile(path);
            }
            case "write_output":
            {
                var fn = Arg(input, "file_name");
                var content = Arg(input, "content");
                var r = tools.WriteOutput(fn, content);
                log($"[工具] write_output {fn} → {r}");
                return r;
            }
            case "append_output":
            {
                var fn = Arg(input, "file_name");
                var content = Arg(input, "content");
                var r = tools.AppendOutput(fn, content);
                log($"[工具] append_output {fn} → {r}");
                return r;
            }
            default:
                return $"[错误] 未知工具：{name}";
        }
    }

    /// <summary>
    /// 定义受控工具（原始 JSON Schema）。读只限知识库/本项目输出；写只限本项目输出目录、白名单扩展名。
    /// 描述里的知识库路径全部取自 env 配置文件（<see cref="PresalesPaths"/>），
    /// 与 <see cref="FileTools"/> 的读白名单同源，避免「提示词说得通、工具判越界」。
    /// </summary>
    private List<ToolUnion> BuildTools()
    {
        var modules   = _paths.Display(_paths.ModulesDir);
        var platforms = _paths.Display(_paths.PlatformsDir);
        var protocol  = _paths.Display(_paths.ProtocolDir);
        return new()
    {
        new Tool
        {
            Name = "list_kb_dir",
            Description = $"列出目录内容。path 取值：{modules} 等知识库路径；{protocol}；"
                        + "以及 project（本项目输出目录，列出本次已生成的 HTML/URS/MD）。",
            InputSchema = new() { Properties = Props(("path", Str($"目录路径，如 {platforms} 或 project"))), Required = ["path"] },
        },
        new Tool
        {
            Name = "read_kb_file",
            Description = "读取一个文本文件。path 取值：知识库路径（模块卡片、平台、托盘、工作站、解决方案、项目范例）、"
                        + $"{protocol}/…（国标）、**project/…（本项目已生成的文件，如 project/1.html）**。"
                        + "严禁编造未读取到的参数。",
            InputSchema = new() { Properties = Props(("path", Str($"文件路径，如 {modules}/MOD-CC-001.md 或 project/1.html"))), Required = ["path"] },
        },
        new Tool
        {
            Name = "write_output",
            Description = "把一份提案文件写入本项目输出目录（覆盖同名文件）。仅允许 .html 或 .md，file_name 为纯文件名（不含路径）。"
                        + "解决方案保存为 1.html/2.html…；新建模块需求说明书保存为 xxx-模块-URS.html；详细设计方案保存为 .md。"
                        + "【重要】单次输出有 token 上限，完整 HTML/MD 通常超限：请用本工具写出开头部分，再用 append_output 分多次追加剩余部分。",
            InputSchema = new()
            {
                Properties = Props(
                    ("file_name", Str("纯文件名，如 1.html 或 前处理-模块-URS.html 或 详细设计方案.md")),
                    ("content", Str("文件的开头部分内容（不必是全文）"))),
                Required = ["file_name", "content"],
            },
        },
        new Tool
        {
            Name = "append_output",
            Description = "向本项目输出目录中已存在的文件**追加**内容（文件不存在则新建）。用于分片写出长文档："
                        + "先 write_output 写开头，再多次 append_output 续写，**每次 4000–5000 字**（篇幅越大轮次越少、成本越低），直到文件完整。"
                        + "追加内容会原样拼接在文件末尾，不要重复已写过的部分。",
            InputSchema = new()
            {
                Properties = Props(
                    ("file_name", Str("要追加的纯文件名，须与之前 write_output 的文件名一致")),
                    ("content", Str("要追加到文件末尾的内容片段"))),
                Required = ["file_name", "content"],
            },
        },
    };
    }

    // =====================================================================
    //  提示词组装
    // =====================================================================

    /// <summary>稳定的知识库 system 前缀（带 5 分钟缓存）：目录导航 + 全量模块清单 + 输出模版 + 硬约束。</summary>
    private List<TextBlockParam> BuildKnowledgeSystem()
    {
        var sb = new StringBuilder();
        sb.AppendLine("你是百泉聚兴（北京）科技有限公司的 SmartLabOS 售前技术方案 Agent。");
        sb.AppendLine("语言：简体中文。专业术语（QuEChERS、SPE、ICP-MS、LC-MS/MS 等）保持原文。");
        sb.AppendLine();
        sb.AppendLine("硬约束：");
        sb.AppendLine($"1) 所有技术参数必须引用自知识库 {_paths.Display(_paths.ReferencesDir)}/ 与流程标准 {_paths.Display(_paths.ProtocolDir)}/，严禁编造；不确定时明确说明并列出待补信息。");
        sb.AppendLine("2) 涉及价格/报价一律回复「请联系销售团队获取报价」，不给数字。");
        sb.AppendLine("3) 只能通过提供的工具读写文件；写文件只能写入本项目输出目录，且仅 .html/.md。");
        sb.AppendLine("4) 单次回复有输出 token 上限，一份完整 HTML/MD 通常写不完：必须先 write_output 写出开头，");
        sb.AppendLine("   再用 append_output 分多次追加（**每次 4000–5000 字**）直到文件完整，切勿试图一次性输出全文。");
        sb.AppendLine("   分片不是越细越好：每多一轮就要重读一遍全部上下文，请在 4000–5000 字这个区间内尽量一次多写。");
        sb.AppendLine($"5) 平台组装约束：一个平台最多搭载 6 个模组、8 个托盘；托盘资料见 {_paths.Display(_paths.PalletDir)}。");
        sb.AppendLine($"6) 交付物版式必须对齐提案模版目录 {_paths.Display(_paths.ProposalTemplateDir)}（见下文「提案模版目录」）：");
        sb.AppendLine("   动笔写某一类文件之前，先 read_kb_file 读它对应的模版样例，再照其结构、章节顺序、表格与图示风格产出。");
        sb.AppendLine();
        sb.AppendLine("## 知识库导航（用 list_kb_dir / read_kb_file 工具按需读取真实内容）");
        // 路径全部来自 env 配置文件，与工具的读白名单同源；改配置即改提示词，无需重新编译。
        var refRoot = _paths.Display(_paths.ReferencesDir);
        sb.AppendLine($"- {_paths.Display(_paths.ModulesDir)}   模块卡片({_catalog.All().Count}个)，模块ID=文件名(如 MOD-CC-001)");
        sb.AppendLine($"- {_paths.Display(_paths.PlatformsDir)} 平台抽象类；一个平台最多搭载6个模组、8个托盘");
        sb.AppendLine($"- {_paths.Display(_paths.PalletDir)}    托盘资料");
        sb.AppendLine($"- {refRoot}/03-workstation 工作站");
        sb.AppendLine($"- {refRoot}/05-Project   项目范例");
        sb.AppendLine($"- {_paths.Display(_paths.ProtocolDir)}  检验/检测流程国标(前处理流程来源)");
        sb.AppendLine($"- {_paths.Display(_paths.ProposalTemplateDir)} **提案模版目录**：各类交付物的标准样例");
        sb.AppendLine("- project/               **本项目输出目录**：本次已生成的 HTML/URS/MD 都在这里，");
        sb.AppendLine("                         用 `list_kb_dir project` 列目录、`read_kb_file project/1.html` 读文件。");
        sb.AppendLine();
        AppendTemplateCatalog(sb);
        sb.AppendLine("## 全部模块清单（ID — 名称（分类）；仅作索引，参数须用 read_kb_file 读卡片）");
        foreach (var m in _catalog.All())
            sb.AppendLine($"- {m.Id}  {m.Name}{(string.IsNullOrWhiteSpace(m.Category) ? "" : $"（{m.Category}）")}");
        sb.AppendLine();
        sb.AppendLine("## 解决方案 HTML 模版全文（生成 1.html、2.html… 时必须遵循其结构与全部内容项）");
        sb.AppendLine(LoadTemplate());
        sb.AppendLine();
        sb.AppendLine("## 《详细设计方案》章节范围标准（生成详细设计方案时必须逐条对照）");
        sb.AppendLine("说明：★必备 = 缺项即不合格；○按需 = 可按本项目裁剪，但必须在文中写明裁剪理由。");
        sb.AppendLine(LoadDetailTemplate());

        return new List<TextBlockParam>
        {
            new() { Text = sb.ToString(), CacheControl = Cache() },
        };
    }

    /// <summary>
    /// 「提案模版目录」清单：列出每类交付物对应哪一份样例、以及是否已被内联。
    /// 只有解决方案 HTML 内联进缓存前缀（阶段一必用）；URS 与详细设计方案样例合计十几万字，
    /// 全塞前缀会让每轮输入成本失控，故改为点名路径 + 强制读取。
    /// </summary>
    private void AppendTemplateCatalog(StringBuilder sb)
    {
        var all = _templates.All();
        sb.AppendLine($"## 提案模版目录（{_paths.Display(_paths.ProposalTemplateDir)}）—— 交付物版式的唯一权威来源");
        if (all.Count == 0)
        {
            sb.AppendLine("（未找到模版目录或其中无样例文件，本次按下文内联模版与标准执行。）");
            sb.AppendLine();
            return;
        }

        var dir = _paths.Display(_paths.ProposalTemplateDir);
        foreach (var t in all)
        {
            var how = t.Role switch
            {
                ProposalTemplateSet.RoleSolutionHtml => "已内联在下文，无需再读",
                ProposalTemplateSet.RoleWordStyle    => "由程序转 docx 时套用，你不必读取",
                _ => $"**动笔前必须先 `read_kb_file {dir}/{t.FileName}`**",
            };
            sb.AppendLine($"- `{t.FileName}` —— {t.Label}；{how}");
        }
        sb.AppendLine("模版是**版式与内容组织的样板**，不是内容来源：照抄其结构、章节顺序、表格列设计、图示与配色，");
        sb.AppendLine("但所有技术参数一律取自本项目的知识库与国标，严禁把样例中的设备、数值、客户信息搬进新方案。");
        sb.AppendLine();
    }

    private string BuildModuleSelectionPrompt(PresalesProject p, Action<string> log)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"任务：为下述项目做「模块选定」——依据国标前处理流程 + 客户需求，从知识库 {_catalog.All().Count} 个模块中选出实现该流程所必需的**最小充分模块集合**。");
        sb.AppendLine("只能从真实存在的模块中选择（模块ID见系统提示中的清单），严禁编造编号。");
        sb.AppendLine();
        // 本步骤是**单次结构化输出，不提供任何工具**（系统提示里的 read_kb_file / list_kb_dir 只在后续
        // 「方案生成」阶段可用）。因此国标全文必须在此内联给出——否则模型只拿到文件名，无从判断前处理
        // 流程，只能输出占位符敷衍（实测返回 {"modules":["NEED_TO_READ_PROTOCOL"]}）。
        sb.AppendLine("【重要】本步骤不提供任何工具，你无法读取文件。所需国标全文已在下方内联给出，");
        sb.AppendLine("请直接依据它作出选型判断并给出最终结果，不要输出占位符或「需先读取…」之类的推脱结论。");
        sb.AppendLine("模块的详细参数确实读不到时，按系统提示中的「全部模块清单」（ID—名称—分类）判断即可。");
        sb.AppendLine();
        AppendProtocolFullText(sb, p, log);
        AppendRequirements(sb, p);
        sb.AppendLine("选型原则：(1) 覆盖前处理每道工序(加液/涡旋/离心/浓缩/SPE净化/定容等)不遗漏；(2) 满足工艺前提下尽量精简、去冗余；(3) 确无匹配模块时在 notes 说明，不要编造。");
        sb.AppendLine();
        sb.AppendLine("请只输出符合给定 JSON Schema 的结果（modules 为按流程顺序排列的模块ID数组，notes 可选）。");
        return sb.ToString();
    }

    /// <summary>供前端「预览」用：返回阶段一将发给模型的任务提示词（不含带缓存的知识库系统前缀）。</summary>
    public string BuildPreview(PresalesProject p) => BuildHtmlPhasePrompt(p);

    private string BuildHtmlPhasePrompt(PresalesProject p)
    {
        var sb = new StringBuilder();
        sb.AppendLine("阶段一任务：生成 SmartLabOS 售前**解决方案 HTML 提案**。");
        sb.AppendLine();
        AppendConfirmedModules(sb, p);
        AppendProtocols(sb, p, "需逐个生成方案的检验/检测流程国标");
        AppendRequirements(sb, p);
        sb.AppendLine("要求：");
        sb.AppendLine("1) 对每个国标：用 read_kb_file 读取其前处理流程，仅从上文【已确认模块】中选型，选定合适的平台型号，在平台上搭载模组组成工作站，再按前处理流程顺序串联成样品制备解决方案。");
        sb.AppendLine("2) 用 HTML 呈现（遵循系统提示中的输出模版全部内容项），必要时加入 SVG、Mermaid 图示；每个国标的方案 write_output 保存为 1.html、2.html……（按国标顺序）。");
        sb.AppendLine("3) 耗时计算：根据各平台所含模块功能码耗时 + 上下料时间，列出每个平台处理耗时并叠加为整方案耗时；各平台处理时间尽量相等且接近 600 秒(10 分钟)，据此拆分/设立平台。");
        var loading = p.LoadingMethod == "自动上下料" ? "本项目采用「自动上下料」，按自动方式估算上下料时间。"
                    : p.LoadingMethod == "人工上下料" ? "本项目采用「人工上下料」，按人工方式估算上下料时间。"
                    : "上下料方式未指定时，按模块卡片默认值估算。";
        sb.AppendLine("   " + loading);
        sb.AppendLine($"4) 平台组装遵守上限：单平台最多 6 个模组、8 个托盘；需要时用 read_kb_file 读 {_paths.Display(_paths.PalletDir)} 的托盘资料与 {_paths.Display(_paths.PlatformsDir)} 的平台卡片确定型号。");
        sb.AppendLine("5) 若已确认模块无法覆盖某前处理需求，生成新建模块的 URS，write_output 保存为「xxx-模块-URS.html」（必要时含 SVG/Mermaid），其处理时间按规范 MD 规定值 + 上下料时间计入平台总耗时。");
        if (_templates.UrsHtml is { } ursTpl)
            sb.AppendLine($"   写 URS 之前**必须先 `read_kb_file {TemplateRef(ursTpl)}`**，照该样例的章节与版式撰写。");
        sb.AppendLine("6) 【写法要求】HTML 篇幅很长，单次回复写不完：先 write_output 写出 <head> 与开头部分，再用 append_output 分多次追加，**每次 4000–5000 字**，直到 </html> 收尾。写完每个文件后再开始下一个。");
        sb.AppendLine("7) 全部文件写完后，用简短文字总结本阶段写出的文件与方案要点。");
        return sb.ToString();
    }

    private string BuildDetailPhasePrompt(PresalesProject p, FileTools tools)
    {
        var sb = new StringBuilder();
        var mdName = DetailMdName(p);
        sb.AppendLine("阶段二任务：在阶段一已生成的解决方案 HTML / URS 基础上，整合形成一份完整、专业的**《详细设计方案》Markdown 文档**。");
        sb.AppendLine("（该 Markdown 随后会由 Pandoc + 参考样式文档自动转为带封面/目录/页码的 WORD 交付客户，因此章节层级与表格必须规范。）");
        sb.AppendLine();

        // 直接把阶段一产物的可读路径喂给模型：上一版只说「读取本项目输出目录」而没给寻址方式，
        // 模型连试 8 轮都没找到文件，最后脱离 HTML 独立成文——硬约束事实上落空。
        sb.AppendLine("【阶段一已产出文件 — 动笔前必须逐一 read_kb_file 读完】");
        if (tools.WrittenFiles.Count == 0)
            sb.AppendLine("  （无——请先用 list_kb_dir project 查看本项目输出目录）");
        else
            foreach (var f in tools.WrittenFiles) sb.AppendLine($"  - project/{f}");
        sb.AppendLine();
        sb.AppendLine("数据来源（严禁编造，须可溯源）：以上述文件为**第一来源**；需要补充设备参数时再读 references/ 卡片。");
        sb.AppendLine("耗时、产能、平台/工作站构成必须与上述 HTML 中的计算完全一致，不得另起一套。");
        sb.AppendLine();
        AppendConfirmedModules(sb, p);
        AppendRequirements(sb, p);
        if (_templates.DetailMd is { } detailTpl)
        {
            sb.AppendLine($"【版式模版 — 动笔前必须 `read_kb_file {TemplateRef(detailTpl)}`】");
            sb.AppendLine("  它是《详细设计方案》的标准样例：照其章节层级、编号方式、表格列设计与措辞风格撰写。");
            sb.AppendLine("  只学版式，不搬内容——样例中的设备型号、数值、客户信息一律不得出现在本方案里。");
            sb.AppendLine();
        }
        sb.AppendLine("章节与要求：");
        sb.AppendLine("1) **严格按系统提示中《详细设计方案》章节范围标准组织全文**：★必备项一项都不能少；○按需项可裁剪，但必须在对应位置写明裁剪理由。");
        sb.AppendLine("2) 所有设备/软件指标必须为「具体数值 + 单位 + 精度 + 通讯协议」，禁止「高精度/快速」等定性描述；耗时/产能须与阶段一 HTML 中的计算完全一致。");
        sb.AppendLine("3) 用 Markdown 表格呈现「关键技术指标表」「检测标准与设备适配表」「实施阶段表」等；表格列数控制在 5 列以内，避免转 WORD 后过窄。");
        sb.AppendLine("4) 版式约定（供转 WORD 用）：文档首行用一级标题写「<项目名> 详细设计方案」作为封面标题；");
        sb.AppendLine("   正文章节从一级标题「1. 项目概述」开始逐级编号；**不要手写目录**（WORD 目录由工具自动生成）。");
        sb.AppendLine($"5) 将文档 write_output 保存为「{mdName}」；篇幅超出单次输出时用 append_output 分片续写至完整，**每次 4000–5000 字**。");
        sb.AppendLine("6) 最后再写一份「Summary-输出总结.md」，列出本次生成的全部文件、各方案要点、耗时/产能结论，以及需要客户确认的待补信息。"
                    + (_templates.SummaryMd is { } sumTpl ? $"版式参照 `read_kb_file {TemplateRef(sumTpl)}`。" : ""));
        sb.AppendLine("7) 全部写完后用简短文字确认已保存的文件名清单。");
        return sb.ToString();
    }

    private void AppendConfirmedModules(StringBuilder sb, PresalesProject p)
    {
        sb.AppendLine("【已确认模块 — 强制选型范围】方案只能从以下模块中选型组装，严禁引入清单外模块（确有缺口须在文中说明）：");
        if (p.Modules.Count == 0) { sb.AppendLine("  （未确认任何模块——请联系业务确认后再生成）"); }
        else
        {
            for (int i = 0; i < p.Modules.Count; i++)
            {
                var info = _catalog.Find(p.Modules[i]);
                sb.AppendLine(info is null
                    ? $"  {i + 1}. {p.Modules[i]}"
                    : $"  {i + 1}. {info.Id}  {info.Name}{(string.IsNullOrWhiteSpace(info.Category) ? "" : $"（{info.Category}）")}");
            }
        }
        sb.AppendLine();
    }

    /// <summary>
    /// 把本项目选中的国标 MD **全文**内联进提示词，供「模块选定」这一无工具步骤使用。
    /// 国标文件本身很小（实测 7–20 KB/份，约 3–8k token），11 份全选也只有约 6 万 token，
    /// 相对 1M 上下文完全可控，因此直接内联比再搭一套工具循环划算得多。
    /// 单份仍设上限兜底，防止将来有人往目录里放进超大文件把请求撑爆。
    /// </summary>
    private void AppendProtocolFullText(StringBuilder sb, PresalesProject p, Action<string> log)
    {
        const int PerFileLimit = 60_000;   // 字符；正常国标远低于此值，仅作失控保护

        sb.AppendLine("需实现的检验/检测流程国标（全文如下）：");
        if (p.Protocols.Count == 0)
        {
            sb.AppendLine("  （未选择流程标准）");
            sb.AppendLine();
            log("[警告] 本项目未选择任何流程标准，模型只能凭客户需求推测，选型质量会明显下降。");
            return;
        }

        int total = 0;
        foreach (var name in p.Protocols)
        {
            var text = ReadProtocol(name, PerFileLimit, out var note);
            total += text.Length;
            sb.AppendLine($"### potocol/MD/{name}");
            sb.AppendLine(text);
            sb.AppendLine();
            log($"[内联国标] {name} {text.Length} 字符{note}");
        }
        sb.AppendLine();
        log($"[内联国标] 共 {p.Protocols.Count} 份，合计 {total} 字符。");
    }

    /// <summary>读取单份国标 MD。文件名来自前端勾选，此处仍按纯文件名收敛，杜绝越目录读取。</summary>
    private string ReadProtocol(string fileName, int limit, out string note)
    {
        note = "";
        var safe = Path.GetFileName(fileName ?? "");
        if (safe.Length == 0)
        {
            note = "（文件名非法，已跳过）";
            return "（该流程标准文件名非法，未能读取。）";
        }

        var path = Path.Combine(_paths.ProtocolDir, safe);
        if (!File.Exists(path))
        {
            note = "（文件不存在，已跳过）";
            return $"（未找到 {safe}，本次未能提供其全文。）";
        }

        try
        {
            var text = File.ReadAllText(path);
            if (text.Length > limit)
            {
                note = $"（超 {limit} 字符已截断）";
                return text[..limit] + "\n…（本文件过长，此处截断）";
            }
            return text;
        }
        catch (Exception ex)
        {
            note = $"（读取失败：{ex.GetType().Name}）";
            return $"（读取 {safe} 失败，本次未能提供其全文。）";
        }
    }

    private void AppendProtocols(StringBuilder sb, PresalesProject p, string title)
    {
        sb.AppendLine($"{title}：");
        if (p.Protocols.Count == 0) sb.AppendLine("  （未选择流程标准）");
        else for (int i = 0; i < p.Protocols.Count; i++)
            sb.AppendLine($"  {i + 1}. potocol/MD/{p.Protocols[i]}");
        sb.AppendLine();
    }

    private static void AppendRequirements(StringBuilder sb, PresalesProject p)
    {
        sb.AppendLine("客户需求信息：");
        sb.AppendLine("- 现状：" + (string.IsNullOrWhiteSpace(p.CurrentStatus) ? "（未填写）" : p.CurrentStatus!.Trim()));
        sb.AppendLine("- 挑战：" + JoinItems(p.Challenges));
        sb.AppendLine("- 期望：" + JoinItems(p.Expectations));
        sb.AppendLine($"- 流程范围：{(p.ProcessScope.Count == 0 ? "（未选择）" : string.Join("、", p.ProcessScope))}");
        sb.AppendLine($"- 上下料方式：{Or(p.LoadingMethod)}");
        sb.AppendLine($"- 软件功能：{Or(p.SoftwareType)}");
        sb.AppendLine();
    }

    // =====================================================================
    //  工具/辅助
    // =====================================================================

    /// <summary>把模版文件的绝对路径转成模型能直接喂给 read_kb_file 的写法。</summary>
    private string TemplateRef(string fullPath) =>
        $"{_paths.Display(_paths.ProposalTemplateDir)}/{Path.GetFileName(fullPath)}";

    /// <summary>
    /// 解决方案 HTML 模版全文。优先取提案模版目录中的 HTML 样例（<c>Proposal_Template</c>），
    /// 目录缺失时退回 <see cref="PresalesPaths.TemplateFile"/>（references/_templates 下的旧模版）。
    /// </summary>
    private string LoadTemplate()
    {
        if (_templateCache is not null) return _templateCache;
        var file = _templates.SolutionHtml ?? _paths.TemplateFile;
        try { _templateCache = File.Exists(file) ? File.ReadAllText(file) : "（未找到输出模版文件，请遵循标准提案结构）"; }
        catch (Exception ex) { _templateCache = $"（读取输出模版失败：{ex.Message}）"; }
        return _templateCache;
    }

    /// <summary>《详细设计方案》章节范围标准（★必备 / ○按需），与会话方式使用同一份模版。</summary>
    private string LoadDetailTemplate()
    {
        if (_detailTemplateCache is not null) return _detailTemplateCache;
        try
        {
            _detailTemplateCache = File.Exists(_paths.DetailTemplateFile)
                ? File.ReadAllText(_paths.DetailTemplateFile)
                : "（未找到详细设计方案章节标准模版，请按「项目概述/需求与标准适配/总体设计/功能平台详细设计/软件系统/产能效率/实施管理/交付物清单」组织）";
        }
        catch (Exception ex) { _detailTemplateCache = $"（读取详细设计章节标准失败：{ex.Message}）"; }
        return _detailTemplateCache;
    }

    /// <summary>响应是否因单次输出上限被截断（StopReason 在不同 SDK 版本里可能是 "max_tokens" 或 MaxTokens）。</summary>
    private static bool IsMaxTokens(Message resp)
    {
        var s = resp.StopReason?.ToString() ?? "";
        return s.Replace("_", "").Contains("maxtokens", StringComparison.OrdinalIgnoreCase);
    }

    private static string Join(IEnumerable<string> items)
    {
        var list = items.ToList();
        return list.Count == 0 ? "（无）" : string.Join("、", list);
    }

    /// <summary>
    /// 努力档位。SDK 12.34.1 的 Effort 枚举还没有 XHigh，但 OutputConfig.Effort 是 ApiEnum，
    /// 可直接传原始字符串 "xhigh"（Opus 4.8 支持，官方推荐用于编码/agentic 长任务）。
    /// </summary>
    private static ApiEnum<string, Effort> ParseEffort(string s) => (s ?? "xhigh").Trim().ToLowerInvariant() switch
    {
        "low" => Effort.Low,
        "medium" => Effort.Medium,
        "high" => Effort.High,
        "max" => Effort.Max,
        _ => "xhigh",
    };

    private static T? SafeParse<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return default;
        try { return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }); }
        catch { return default; }
    }

    private static void LogUsage(StreamedTurn turn, Action<string> log)
    {
        var totalIn = turn.InputTokens + turn.CacheReadTokens + turn.CacheWriteTokens;
        // 缓存未命中却重写了大段前缀 = TTL 被击穿或前缀被改动，这是最容易被忽视的成本泄漏点
        var miss = turn.CacheReadTokens == 0 && turn.CacheWriteTokens > 20_000 ? "  ⚠缓存未命中" : "";
        log($"[用量] 输入合计={totalIn}（新增={turn.InputTokens} 缓存读={turn.CacheReadTokens} 缓存写={turn.CacheWriteTokens}）"
          + $" 输出={turn.OutputTokens} stop={turn.StopReason}{miss}");
    }

    private static JsonElement J(string s) => JsonSerializer.SerializeToElement(s);
    private static JsonElement Str(string desc) => JsonSerializer.SerializeToElement(new { type = "string", description = desc });
    private static Dictionary<string, JsonElement> Props(params (string Key, JsonElement Val)[] items)
    {
        var d = new Dictionary<string, JsonElement>();
        foreach (var (k, v) in items) d[k] = v;
        return d;
    }

    private static string Arg(IReadOnlyDictionary<string, JsonElement> input, string key)
    {
        if (!input.TryGetValue(key, out var v)) return "";
        return v.ValueKind == JsonValueKind.String ? (v.GetString() ?? "") : v.GetRawText();
    }

    private static string JoinItems(List<string> items)
    {
        var ne = items.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToList();
        return ne.Count == 0 ? "（未填写）" : string.Join("；", ne);
    }

    private static string Or(string? s) => string.IsNullOrWhiteSpace(s) ? "（未指定）" : s!;
    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max] + "…";
    /// <summary>压成单行，供运行日志按行记录模型原始返回。</summary>
    private static string Flatten(string s) => s.Replace("\r", " ").Replace("\n", "␤");
    private static string Sanitize(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = name.Select(c => invalid.Contains(c) ? '_' : c).ToArray();
        return new string(chars);
    }
}
