using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Anthropic;
using Anthropic.Models.Messages;

namespace Presales.Proposal.AI.Agent.Agent;

/// <summary>
/// 一次流式调用累积出的完整回合。
///
/// 为什么必须流式：生成整份 HTML/详细设计方案时 max_tokens 需要开到 64K，
/// 非流式请求会长时间挂在一个 HTTP 连接上，容易被超时切断；流式则边生成边收，
/// 同时还能把模型正在写的内容实时喂给运行日志。
///
/// SDK 没有提供「把事件流合成 Message」的助手，所以这里按块索引自行拼装：
/// 文本 / thinking(含签名，多轮回传必须原样) / tool_use(input 是 JSON 分片) 三类。
/// </summary>
internal sealed class StreamedTurn
{
    /// <summary>助手回合的内容块，可直接作为下一轮 messages 的 assistant 消息。</summary>
    public List<ContentBlockParam> Assistant { get; } = new();

    /// <summary>本回合请求的工具调用（顺序与出现顺序一致）。</summary>
    public List<ToolCall> ToolCalls { get; } = new();

    /// <summary>纯文本合并结果（供日志与结构化输出解析）。</summary>
    public string Text { get; private set; } = "";

    /// <summary>
    /// 本回合各内容块的类型与长度，形如 "thinking:0、text:284"。
    /// 用于诊断「有输出 token 却拿不到文本」——说明内容落进了这里没识别的块类型。
    /// </summary>
    public string BlockSummary { get; private set; } = "";

    public string StopReason { get; private set; } = "";
    public long InputTokens { get; private set; }
    public long OutputTokens { get; private set; }
    public long CacheReadTokens { get; private set; }
    public long CacheWriteTokens { get; private set; }

    /// <summary>是否因单次输出上限被截断。</summary>
    public bool IsMaxTokens => StopReason.Replace("_", "").Contains("maxtokens", StringComparison.OrdinalIgnoreCase);

    public readonly record struct ToolCall(string Id, string Name, IReadOnlyDictionary<string, JsonElement> Input);

    /// <summary>
    /// 发起流式请求并累积为一个完整回合；对可重试故障自动退避重试。
    ///
    /// 为什么必须自己重试：SDK 只在 HTTP 层自动重试 429/5xx，而流式请求一旦以 200 开始，
    /// 服务端过载(overloaded_error)是**在 SSE 流中途**以事件形式返回并抛 AnthropicSseException 的，
    /// SDK 的重试根本没有机会介入 —— 不自己兜住，一次瞬时过载就会让整个任务判失败。
    /// 重试会丢弃本次已累积的内容重跑该回合；因为工具是在回合结束后才执行，重跑是安全的。
    /// </summary>
    public static async Task<StreamedTurn> RunAsync(AnthropicClient client, MessageCreateParams parameters,
        int maxAttempts, Action<string> log, CancellationToken ct)
    {
        ValidateCacheControl(parameters);   // 本地先拦一道，别让服务端替我们发现拼错的请求体

        for (int attempt = 1; ; attempt++)
        {
            try
            {
                return await StreamOnceAsync(client, parameters, ct);
            }
            catch (Exception ex) when (!ct.IsCancellationRequested && !IsRetryable(ex))
            {
                // 请求本身有问题（400/鉴权等），重试多少次都一样——如实说明，不要谎报重试次数
                log($"[错误] 调用失败且不可重试（{Brief(ex)}），未做重试。");
                throw;
            }
            catch (Exception ex) when (attempt >= Math.Max(1, maxAttempts) && !ct.IsCancellationRequested)
            {
                log($"[错误] 已重试 {attempt} 次仍失败（{Brief(ex)}），放弃本回合。");
                throw;
            }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                // 指数退避 + 抖动：2s、4s、8s、16s…上限 60s，避开与其它请求同时重试
                var seconds = Math.Min(60, Math.Pow(2, attempt)) + Random.Shared.NextDouble() * 2;
                log($"[重试] 第 {attempt} 次调用失败（{Brief(ex)}），{seconds:0.#}s 后重试…");
                await Task.Delay(TimeSpan.FromSeconds(seconds), ct);
            }
        }
    }

    private static readonly Regex CacheBlockRegex =
        new(@"""cache_control""\s*:\s*\{[^}]*\}", RegexOptions.Compiled);
    private static readonly Regex TtlRegex =
        new(@"""ttl""\s*:\s*""([^""]+)""", RegexOptions.Compiled);

    /// <summary>
    /// 发请求前的请求体自检：同一请求内所有 cache_control 块的 TTL 必须一致。
    ///
    /// Anthropic 按 tools → system → messages 顺序处理缓存块，且**不允许 ttl='1h' 出现在 ttl='5m' 之后**，
    /// 违反时整个请求 400。这类错误只有真正发出去才会暴露——曾因 system 用默认 5m、
    /// 而工具循环顶层用 1h，导致方案生成在第 1 轮就整体失败。
    /// 与其等服务端拒绝再去读那句英文报错，不如在本地立刻指出是哪几种 TTL 混用了。
    ///
    /// 标记为 internal 而非 private：便于探针/测试直接调用验证，无需真实 API 调用。
    /// </summary>
    internal static void ValidateCacheControl(MessageCreateParams parameters)
    {
        string json;
        try { json = JsonSerializer.Serialize(parameters); }
        catch { return; }   // 自检本身绝不能成为故障源：序列化不了就跳过检查

        var ttls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match block in CacheBlockRegex.Matches(json))
        {
            var ttl = TtlRegex.Match(block.Value);
            ttls.Add(ttl.Success ? ttl.Groups[1].Value : "5m");   // 未写 ttl 即默认 5 分钟
        }

        if (ttls.Count > 1)
            throw new InvalidOperationException(
                $"请求体自检未通过：同一请求内混用了 {ttls.Count} 种 cache_control TTL（{string.Join(" / ", ttls.OrderBy(x => x))}）。"
                + "Anthropic 要求它们保持一致（处理顺序 tools → system → messages，长 TTL 不得出现在短 TTL 之后），否则返回 400。"
                + "请检查是否有 cache_control 绕过了 ClaudeAgentService.Cache() 这个唯一出口。");
    }

    /// <summary>可重试判定：服务端过载/限流/网络抖动可重试；鉴权与请求本身的错误重试多少次都一样。</summary>
    private static bool IsRetryable(Exception ex)
    {
        var name = ex.GetType().Name;
        var msg = ex.Message ?? "";

        if (name is "AnthropicUnauthorizedException" or "AnthropicForbiddenException"
                 or "AnthropicBadRequestException" or "AnthropicNotFoundException"
                 or "AnthropicUnprocessableEntityException")
            return false;
        if (msg.Contains("authentication_error") || msg.Contains("invalid_request_error")
            || msg.Contains("permission_error"))
            return false;

        if (name is "AnthropicRateLimitException" or "Anthropic5xxException"
                 or "AnthropicIOException" or "AnthropicSseException"
                 or "HttpRequestException" or "IOException" or "TaskCanceledException")
            return true;

        return msg.Contains("overloaded_error") || msg.Contains("rate_limit_error")
            || msg.Contains("api_error") || msg.Contains("SSE error");
    }

    private static string Brief(Exception ex)
    {
        var msg = (ex.Message ?? "").Replace("\r", " ").Replace("\n", " ");
        if (msg.Contains("overloaded_error")) return "服务端过载 overloaded_error";
        if (msg.Contains("rate_limit_error")) return "触发限流 rate_limit_error";
        return $"{ex.GetType().Name}: {(msg.Length <= 120 ? msg : msg[..120] + "…")}";
    }

    private static async Task<StreamedTurn> StreamOnceAsync(AnthropicClient client, MessageCreateParams parameters,
        CancellationToken ct)
    {
        var turn = new StreamedTurn();
        var blocks = new SortedDictionary<int, BlockBuilder>();

        await foreach (var evt in client.Messages.CreateStreaming(parameters).WithCancellation(ct))
        {
            if (evt.TryPickStart(out RawMessageStartEvent? start) && start is not null)
            {
                turn.ReadUsage(start.Message.Usage);
            }
            else if (evt.TryPickContentBlockStart(out RawContentBlockStartEvent? bs) && bs is not null)
            {
                blocks[(int)bs.Index] = BlockBuilder.From(bs.ContentBlock);
            }
            else if (evt.TryPickContentBlockDelta(out RawContentBlockDeltaEvent? bd) && bd is not null)
            {
                if (!blocks.TryGetValue((int)bd.Index, out var b)) { b = new BlockBuilder("text"); blocks[(int)bd.Index] = b; }
                b.Apply(bd.Delta);
            }
            else if (evt.TryPickDelta(out RawMessageDeltaEvent? md) && md is not null)
            {
                if (md.Delta.StopReason is { } sr) turn.StopReason = sr.ToString() ?? "";
                turn.OutputTokens = md.Usage.OutputTokens;
            }
        }

        var text = new StringBuilder();
        var kinds = new List<string>(blocks.Count);
        foreach (var b in blocks.Values)
        {
            var param = b.Build(out var toolCall);
            if (param is not null) turn.Assistant.Add(param);
            if (toolCall is { } tc) turn.ToolCalls.Add(tc);
            if (b.Kind == "text") text.Append(b.Text);
            kinds.Add($"{b.Kind}:{b.Text.Length}");
        }
        turn.Text = text.ToString();
        turn.BlockSummary = kinds.Count == 0 ? "（无内容块）" : string.Join("、", kinds);
        return turn;
    }

    private void ReadUsage(Usage u)
    {
        try
        {
            InputTokens = u.InputTokens;
            CacheReadTokens = u.CacheReadInputTokens ?? 0;
            CacheWriteTokens = u.CacheCreationInputTokens ?? 0;
        }
        catch { /* 用量字段随 SDK 版本可能变化，不影响主流程 */ }
    }

    // ---------------- 单个内容块的拼装 ----------------

    private sealed class BlockBuilder
    {
        public string Kind { get; }
        public StringBuilder Text { get; } = new();
        private readonly StringBuilder _json = new();
        private string? _signature;
        private string? _toolId;
        private string? _toolName;
        private string? _redactedData;

        public BlockBuilder(string kind) => Kind = kind;

        public static BlockBuilder From(RawContentBlockStartEventContentBlock block)
        {
            if (block.TryPickToolUse(out ToolUseBlock? tu) && tu is not null)
                return new BlockBuilder("tool_use") { _toolId = tu.ID, _toolName = tu.Name };
            if (block.TryPickThinking(out ThinkingBlock? th) && th is not null)
            {
                var b = new BlockBuilder("thinking");
                b.Text.Append(th.Thinking ?? "");
                b._signature = th.Signature;
                return b;
            }
            if (block.TryPickRedactedThinking(out RedactedThinkingBlock? rt) && rt is not null)
                return new BlockBuilder("redacted_thinking") { _redactedData = rt.Data };
            if (block.TryPickText(out TextBlock? t) && t is not null)
            {
                var b = new BlockBuilder("text");
                b.Text.Append(t.Text ?? "");
                return b;
            }
            return new BlockBuilder("text");
        }

        public void Apply(RawContentBlockDelta delta)
        {
            if (delta.TryPickText(out TextDelta? td) && td is not null) { Text.Append(td.Text); return; }
            if (delta.TryPickThinking(out ThinkingDelta? thd) && thd is not null) { Text.Append(thd.Thinking); return; }
            if (delta.TryPickSignature(out SignatureDelta? sd) && sd is not null) { _signature = sd.Signature; return; }
            if (delta.TryPickInputJson(out InputJsonDelta? ij) && ij is not null) { _json.Append(ij.PartialJson); }
        }

        /// <summary>产出可回传的内容块；tool_use 同时给出待执行的调用。</summary>
        public ContentBlockParam? Build(out ToolCall? toolCall)
        {
            toolCall = null;
            switch (Kind)
            {
                case "thinking":
                    // 同模型多轮必须原样回传 thinking（签名不可篡改）
                    return new ThinkingBlockParam { Thinking = Text.ToString(), Signature = _signature ?? "" };

                case "redacted_thinking":
                    return new RedactedThinkingBlockParam { Data = _redactedData ?? "" };

                case "tool_use":
                {
                    var input = ParseInput(_json.ToString());
                    toolCall = new ToolCall(_toolId ?? "", _toolName ?? "", input);
                    return new ToolUseBlockParam { ID = _toolId ?? "", Name = _toolName ?? "", Input = input };
                }

                default:
                    var s = Text.ToString();
                    if (s.Length == 0) return null;
                    return new TextBlockParam { Text = s };
            }
        }

        /// <summary>
        /// tool_use 的 input 以 JSON 分片流式到达。若回合被 max_tokens 截断，
        /// 最后一个分片可能是残缺 JSON —— 此时返回空参数，由上层的续写逻辑处理，
        /// 绝不能因为反序列化异常把整个任务打挂。
        /// </summary>
        private static IReadOnlyDictionary<string, JsonElement> ParseInput(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new Dictionary<string, JsonElement>();
            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)
                       ?? new Dictionary<string, JsonElement>();
            }
            catch (JsonException)
            {
                return new Dictionary<string, JsonElement>();
            }
        }
    }
}
