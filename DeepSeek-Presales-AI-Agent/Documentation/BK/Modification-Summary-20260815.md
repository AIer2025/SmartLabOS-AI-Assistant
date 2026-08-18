# SmartLabOS Presales AI Agent 改造总结（2026-08-15）

> 范围：`C:\TestClaude\SmartLabOS-AI-Assistant\Claude-API-Key-Presales-AI-Agent`
> 主题：后端大模型由 **Claude Opus 4.8** 迁移到 **DeepSeek V4 Pro**
> 结果：`dotnet build` Debug/Release 均 0 警告 0 错误；启动自检、非流式探针 7 项、流式工具循环探针 2 项均已实机验证。
> 依据文档：[Transfrom-from-Claude-to-DeepSeek-20260814.html](./Transfrom-from-Claude-to-DeepSeek-20260814.html)

## 目录

- [一、总体目标](#一总体目标)
- [二、迁移路线的选择](#二迁移路线的选择)
- [三、供应商抽象层](#三供应商抽象层)
- [四、DeepSeek 兼容端点的四处差异与对策](#四deepseek-兼容端点的四处差异与对策)
- [五、实测探针：一条文档推断被推翻](#五实测探针一条文档推断被推翻)
- [六、防编造加固](#六防编造加固)
- [七、去竞品化](#七去竞品化)
- [八、文件变更清单](#八文件变更清单)
- [九、验证记录](#九验证记录)
- [十、回退方法](#十回退方法)
- [十一、遗留事项与后续建议](#十一遗留事项与后续建议)

---

## 一、总体目标

| # | 需求 | 状态 |
| --- | --- | --- |
| 1 | 后端调用的大模型从 Claude 换成 DeepSeek V4 Pro | ✅ 完成 |
| 2 | 按 `Transfrom-from-Claude-to-DeepSeek-20260814.html` 的方案实施 | ✅ 完成（其中 1 条推断被实测推翻并已修正，见第五节） |
| 3 | `env/Presales-AI-Agent-config.json` 增加 `DeepSeek_Env` 节承载密钥 | ✅ 完成 |
| 4 | 稳妥移植 | ✅ 编译零警告；启动自检 + 9 项实机探针全通过；保留一键回退 |

**一句话结果**：`StreamedTurn.cs` 的流式拼装逻辑一行未动，工具循环、重试退避、thinking 签名回传全部原样保留；换供应商只是改三个配置项。

---

## 二、迁移路线的选择

采用文档第三节的**路线 A**：保留 `Anthropic` C# SDK（v12.34.1），把 `AnthropicClient.BaseUrl` 指向 DeepSeek 的 Anthropic 兼容端点 `https://api.deepseek.com/anthropic`。

未采用路线 B（改用 OpenAI 兼容原生端点）的理由：路线 B 要重写 `StreamedTurn`（SSE 事件形状完全不同），约 2–3 天，且回退到 Claude 需要维护两套调用层。在质量尚未经 A/B 对照验证之前先付这笔重构成本，是把决策顺序做反了。

**先决条件已验证**：`Anthropic.dll` 中确实存在 `set_BaseUrl` / `BaseUrlExplicit`，SDK 支持自定义接入点。

---

## 三、供应商抽象层

### 3.1 三个新配置项

`AgentSettings`（[`Configuration/PresalesConfig.cs`](../src/Presales.Proposal.AI.Agent/Configuration/PresalesConfig.cs)）新增：

| 配置键 | 取值 | 说明 |
| --- | --- | --- |
| `Agent:Provider` | `deepseek`（默认）\| `anthropic` | 决定密钥来源、默认档位，以及是否发送兼容层不支持的字段 |
| `Agent:BaseUrl` | 接入点 URL | 留空按 Provider 取默认；写 `-` 表示强制留空（自建网关场景） |
| `Agent:Thinking` | `adaptive` \| `off` | 是否发送 `thinking` 字段 |

派生属性 `IsDeepSeek` 集中承担分流判断，避免各处散落字符串比较。

### 3.2 密钥解析改为按供应商分流

改造前：环境变量 `ANTHROPIC_API_KEY` 优先，其次配置节 `Agent:ApiKey`。

改造后：

- **deepseek**：env 配置文件 `DeepSeek_Env:DeepSeek_API_Key` → 环境变量 `DEEPSEEK_API_KEY`
- **anthropic**：环境变量 `ANTHROPIC_API_KEY` → 配置节 `Agent:ApiKey`（保持改造前行为不变）

**deepseek 路径刻意不读 `ANTHROPIC_API_KEY`。** 这台机器上很可能还残留着旧的 Anthropic 密钥；若沿用"环境变量优先"的老逻辑，就会拿 `sk-ant-…` 去打 DeepSeek，只能得到一句语焉不详的 401，而且极难往这个方向想。配置文件优先与本项目既有的"现场只认这一份文件"philosophy 也一致。

### 3.3 密钥/供应商匹配性自检

`AgentSettings.Describe()` 增加前缀校验：供应商是 DeepSeek 却拿到 `sk-ant-` 开头的密钥（或反之），在启动日志里直接点破。

> 这台机器上两家的密钥可能同时存在，拿错了在调用时只会看到一句 401；在启动日志里点破则几秒钟就能定位。

### 3.4 `DeepSeek_Env` 节的解析

[`Configuration/AgentEnvFile.cs`](../src/Presales.Proposal.AI.Agent/Configuration/AgentEnvFile.cs) 新增映射：

| 配置文件节点 | 映射到的配置键 |
| --- | --- |
| `DeepSeek_Env.DeepSeek_API_Key`（兼容 `Api_Key` / `ApiKey` 写法） | `Agent:ApiKey` |
| `DeepSeek_Env.Model`（可选） | `Agent:Model` |
| `DeepSeek_Env.Base_Url` / `BaseUrl`（可选） | `Agent:BaseUrl` |
| `DeepSeek_Env.Effort`（可选） | `Agent:Effort` |

同时复用既有的占位符检测（`PUT-` / `请填入` / `YOUR-` 等），密钥仍是模板占位符时给出警告而非静默失败。

> **`ClaudeAI_Env` 节名未改。** 它只是"知识库运行目录"的意思，与用哪家大模型无关；改了现场配置文件就会失效。已在代码注释与配置说明中写明。

---

## 四、DeepSeek 兼容端点的四处差异与对策

| # | 差异 | 对策 | 代码位置 |
| --- | --- | --- | --- |
| 1 | 忽略 `output_config.format`（结构化输出） | 模块选定改用**工具调用** `submit_modules`，Schema 原样搬过去 | `LlmAgentService.SelectModulesAsync` |
| 2 | 忽略 `cache_control` | `Cache()` 在 deepseek 下返回 `null`，该字段不出现在请求里 | `LlmAgentService.Cache()` |
| 3 | 不支持 `redacted_thinking` 内容块 | 回传时丢弃该块 | `StreamedTurn.BlockBuilder.Build()` |
| 4 | thinking 模式拒绝 `tool_choice={type:"tool"}` | 改用 `{type:"any"}` | `LlmAgentService.SelectModulesAsync` |

### 4.1 关于差异 1

Anthropic 的结构化输出路径**完整保留**：`Provider=anthropic` 时仍走 `OutputConfig.Format`。两条路径最终汇到同一套解析与知识库校验，兜底逻辑（`ModuleCatalog.ExtractFromText` 正则抽取）也共用。这样切回 Claude 时行为与改造前完全一致，便于 A/B 对照。

### 4.2 关于差异 2

不发 `cache_control` 而非"发了被忽略"，是为了少一个未知字段、少一分被拒的风险。DeepSeek 的磁盘自动缓存会接管，而本系统"稳定 system 前缀在前、对话历史在后"的结构本就是自动前缀缓存的理想形态。

`StreamedTurn.ValidateCacheControl()` 无需改动：deepseek 下请求里没有 cache_control 块，集合为空，检查自然通过。

### 4.3 关于差异 3

`redacted_thinking` 出现概率低，但一次就足以让整轮请求 400、整个生成任务判失败。丢弃它只损失一段本就不可读的思考内容，保住的是整轮生成——这个取舍在任何时候都划算。

通过 `RunAsync(..., bool dropRedactedThinking)` 参数传入，默认 `false`（不影响 Anthropic 路径）。

### 4.4 关于重试判定

`StreamedTurn.IsRetryable()` 的**异常类型名判定与供应商无关**——SDK 按 HTTP 状态码映射异常，打 DeepSeek 时抛的仍是 `AnthropicRateLimitException` / `Anthropic5xxException`，判定继续有效。失效的只是错误**文案**匹配（`overloaded_error` 等是 Anthropic 专有 type 串），已补上 DeepSeek 侧关键字：

- 不可重试：`Authentication Fails`、`Insufficient Balance`、`Invalid Request Body`
- 可重试：`Rate Limit Reached`、`Server Overloaded`、`Server Error`

`Brief()` 也相应给出中文诊断（如"账户余额不足（DeepSeek）"）。

---

## 五、实测探针：一条文档推断被推翻

迁移文档第八节原列了 6 项"基于公开文档的推断、尚未实测"。实施时对 `api.deepseek.com/anthropic` 逐项打了探针。

### 5.1 被推翻的一条

文档第四节改动清单第 4 项原写"改用强制工具调用，配 `tool_choice = tool`"。实测该写法直接被拒：

```
HTTP 400
{"message": "Thinking mode does not support this tool_choice",
 "type": "invalid_request_error"}
```

**根因**：DeepSeek V4 Pro **恒开 thinking**——即使不发 `thinking` 字段，每次响应同样带 `thinking` 块——而 thinking 模式下不接受"必须调用某个指定工具"这种 tool_choice。

**解法**：改用 `tool_choice = {"type":"any"}`（必须调用某个工具）。模块选定这一步只定义了 `submit_modules` 一个工具，因此两者语义完全等价，实测稳定返回 `stop_reason=tool_use`。

> 代码中已在该行钉了「勿改回 ToolChoiceTool」的注释，并说明了唯一的例外条件（将来这一步同时提供多个工具时）。
>
> **如果只照文档实施而不做探针，模块选定会 100% 失败。**

### 5.2 其余探针结论

| 探针项 | 结果 | 处置 |
| --- | --- | --- |
| `model=deepseek-v4-pro` 基线 | 200 | 密钥、端点、模型名均正确 |
| `effort=high` / `xhigh` | 均 200 | 都不报错；是否真正影响推理深度无法从外部观测，配置保守取 `high` |
| `thinking={"type":"adaptive"}` | 200 | 可用；但 V4 Pro 本就恒开推理，发与不发行为一致 → 配置取 `off` |
| thinking 块的 `signature` | 存在（UUID） | **工具循环安全**：多轮原样回传 thinking + tool_use 正常 |
| `cache_control` | 200（被忽略） | 与文档一致，确认只是忽略不报错 |
| `usage` 缓存字段 | 字段在，恒为 0 | 成本可观测性确实变盲，日志改为显式标注 |
| 未识别模型名 | 静默映射 | `claude-opus-4-8` 被映射到 `deepseek-v4-pro`；仍显式写模型名为宜 |
| **流式** + 工具 + `any` | 通过 | 用 SDK `CreateStreaming` 走完整两轮循环，与程序实际路径一致 |

### 5.3 为什么补了流式探针

前 7 项都是非流式（curl / urllib）验证，而程序实际走的是 `client.Messages.CreateStreaming`。SSE 事件形状是兼容层最可能出偏差的地方，且此前完全未验证。因此另建了一个引用同版本 SDK 的控制台探针，用与 `StreamedTurn.StreamOnceAsync` 同构的累积器跑完两轮工具循环——这才算真正覆盖了程序的实际路径。

上述结论已回填至迁移文档第八节（该节标题由"需要实测确认的事项"改为"实测结论"），并写进 `appsettings.json` 的注释块。

---

## 六、防编造加固

项目 SOUL.md 的第一条是"严禁编造数据；不确定时诚实说明"。迁移文档第五节判断这是本次换模型**最大的风险点**：V4 系列在"不知道答案时是否承认不知道"这一口径上表现较弱，且幻觉集中出现在**上下文含大量相似文本**时——而本系统 system 前缀恰恰是 97 条格式高度同构的模块清单。

本次在提示词层面做了两处针对性加固（`LlmAgentService.BuildKnowledgeSystem()`）：

**1. 给"不知道"一条合法退出路径。**
原硬约束只有"严禁编造"这种否定式表述——只说了不许做什么，没给出该做什么。补上正面授权：

> 读不到某个参数时，请直接写「该参数需向知识库补充：\<参数名\>」并继续后续内容——
> 这是被鼓励的正确做法，不是失败；凭印象填一个看似合理的数值才是严重错误。

**2. 在模块清单前后各钉一句硬约束。**
近百条同构条目最容易诱发"顺手编一个看着合理的编号"，不能指望模型记得几百行之前的那条约束：

> 【重要】只能使用本清单中逐字出现过的模块ID，一个字符都不能改。清单里没有的编号一律视为不存在。
> …（97 条清单）…
> 【重要】以上清单之外的任何模块ID都是编造的，会被程序直接过滤掉，请勿输出。

> **注意**：这两处只是提示词层面的缓解，**不是根治**。程序化参数溯源校验（把方案里的模块ID与数值指标回查知识库）仍未实现——见第十一节。

---

## 七、去竞品化

本系统是交付给客户的售前产品，运行日志与前端提示里出现竞品名字是商务问题，不只是代码洁癖。

| 位置 | 改前 | 改后 |
| --- | --- | --- |
| 类名 / 文件名 | `ClaudeAgentService` | `LlmAgentService` |
| 接口报错 | "未配置 Anthropic API Key…请设置环境变量 ANTHROPIC_API_KEY。" | 由 `MissingApiKeyMessage` 按供应商动态给出该去哪儿配 |
| 运行记录文件名 | `Anthropic-Messages-API-{时间戳}` | `{供应商}/{模型}-{时间戳}`（便于事后区分切换前后的运行） |
| 阶段提示 | "准备中：组织上下文并调用 Claude…" | "…调用大模型…" |
| 前端确认框 ×2 | "将调用本机 Claude Code…" / "将调用 Claude 在项目目录下…" | "将调用大模型…" |
| 首页流程说明 | "（调用 Claude Code 生成 …）" | "（调用大模型生成 …）" |
| 指令预览标题 | "将传递给 Claude Code 的指令文档（预览）" | "将传递给大模型的指令文档（预览）" |

保留未改的 Claude/Anthropic 字样均属合理：SDK 命名空间与类型、作为**回退供应商**的 anthropic 分支、`ClaudeAI_Env` 配置节名、以及解释兼容层差异的注释。

`GenerationWorker.RunHeader()` 增加供应商与思考模式字段——同一批提案可能横跨切换前后，事后对比质量时这是唯一的区分依据。

---

## 八、文件变更清单

### 重命名

| 文件 | 说明 |
| --- | --- |
| `Agent/ClaudeAgentService.cs` → [`Agent/LlmAgentService.cs`](../src/Presales.Proposal.AI.Agent/Agent/LlmAgentService.cs) | 类名同步改为 `LlmAgentService`；含本次核心改动（+277 行） |

### 修改

| 文件 | 改动 |
| --- | --- |
| [`Configuration/PresalesConfig.cs`](../src/Presales.Proposal.AI.Agent/Configuration/PresalesConfig.cs) | `AgentSettings` 增加 `Provider`/`BaseUrl`/`Thinking`/`IsDeepSeek`/`Summary()`；密钥解析按供应商分流；`Describe()` 增加前缀匹配性校验 |
| [`Configuration/AgentEnvFile.cs`](../src/Presales.Proposal.AI.Agent/Configuration/AgentEnvFile.cs) | 解析 `DeepSeek_Env` 节 → `Agent:ApiKey` 等键；增加 `HasLlmApiKey`；占位符与缺键警告 |
| [`Agent/StreamedTurn.cs`](../src/Presales.Proposal.AI.Agent/Agent/StreamedTurn.cs) | `RunAsync` 增加 `dropRedactedThinking` 参数并透传；`IsRetryable`/`Brief` 补 DeepSeek 错误码；注释更新 |
| [`Agent/GenerationWorker.cs`](../src/Presales.Proposal.AI.Agent/Agent/GenerationWorker.cs) | 类型改名；`RunHeader()` 记录供应商/思考模式/缓存方式 |
| [`Controllers/PresalesController.cs`](../src/Presales.Proposal.AI.Agent/Controllers/PresalesController.cs) | 类型改名；缺 Key 提示改用 `MissingApiKeyMessage`；运行记录名带供应商；阶段文案去竞品化 |
| [`Program.cs`](../src/Presales.Proposal.AI.Agent/Program.cs) | DI 注册改名；启动自检打印大模型参数摘要；缺 Key 时输出 Error 级提示 |
| [`Domain/PresalesModels.cs`](../src/Presales.Proposal.AI.Agent/Domain/PresalesModels.cs) | `ModulePick` 注释去竞品化 |
| [`appsettings.json`](../src/Presales.Proposal.AI.Agent/appsettings.json) | `Agent` 节切到 DeepSeek，新增 `Provider`/`BaseUrl`/`Thinking`；说明块补充切换方法、四处差异、实测结论 |
| [`wwwroot/index.html`](../src/Presales.Proposal.AI.Agent/wwwroot/index.html) | 2 处文案 |
| [`wwwroot/js/presales.js`](../src/Presales.Proposal.AI.Agent/wwwroot/js/presales.js) | 4 处文案 |
| [`env/Presales-AI-Agent-config.sample.json`](../env/Presales-AI-Agent-config.sample.json) | 增加 `DeepSeek_Env` 节；说明块同步 |
| `env/Presales-AI-Agent-config.json` | 说明块更新（原文写"Anthropic API Key 不在本文件中"，现已失实）。**该文件在 .gitignore 中，含密钥，不入库** |
| [`Documentation/Transfrom-from-Claude-to-DeepSeek-20260814.html`](./Transfrom-from-Claude-to-DeepSeek-20260814.html) | 第八节由"待实测事项"改写为"实测结论"，含被推翻推断的说明 |

### 新增

| 文件 | 说明 |
| --- | --- |
| `Documentation/Modification-Summary-20260815.md` | 本文档 |

**改动量**：15 个文件，+1345 / −142 行（含文档）。核心代码改动集中在 `LlmAgentService.cs`；`StreamedTurn.cs` 仅 49 行，流式拼装逻辑未动。

---

## 九、验证记录

### 构建

```
dotnet build -c Debug    → 0 个警告，0 个错误
dotnet build -c Release  → 0 个警告，0 个错误
```

### 启动自检（实际日志节选）

```
配置文件：C:\TestClaude\SmartLabOS-AI-Assistant\Claude-API-Key-Presales-AI-Agent\env\Presales-AI-Agent-config.json
运行目录：模块=…\references\01-modules | 平台=…\02-platforms | 托盘=…\06-pallet | 国标=…\potocol\MD
         | 输出=…\projects | 提案模版=…\09-ProposalTemplate | 标准模版=…\_templates
提案模版 [solution-html] 1.html
提案模版 [urs-html] 微量加注-模块-URS.html
提案模版 [detail-md] 测试项目-详细设计方案.md
提案模版 [summary-md] Summary-输出总结.md
提案模版 [word-style] 测试项目-详细设计方案.docx
MySQL 目标：root@192.168.101.26:3306/SmartLabOS-Presales-AI（口令 已配置，11 位）
MySQL 连通性：失败（请检查 env 配置文件中的 DB_Connect）
大模型：供应商=deepseek  接入点=https://api.deepseek.com/anthropic  模型=deepseek-v4-pro
        努力档位=high  思考=off  MaxTokens=64000  缓存=DeepSeek 自动磁盘缓存（不可配置）
大模型 API Key：已配置（来源=env 配置文件 DeepSeek_Env:DeepSeek_API_Key, 长度=35, sk-f3cd4…e1b9）
```

要点：

- 密钥来源正确显示为 env 配置文件，**未被机器上残留的 `ANTHROPIC_API_KEY` 顶掉**
- 无供应商/密钥前缀不匹配告警
- 供应商、接入点、模型、档位逐项可见

### 实机探针

| 批次 | 内容 | 结果 |
| --- | --- | --- |
| 非流式 ×7 | 基线 / effort×2 / thinking / tool_choice / cache_control / 模型名映射 | 6 通过，1 发现阻塞问题（见第五节）并已修复 |
| 非流式 ×4 | tool_choice 的 any/auto/默认三种变体 + 两轮工具循环 thinking 回传 | 全通过 |
| **流式** ×2 | 模块选定路径（Tools + ToolChoiceAny）、方案生成路径（两轮工具循环） | 全通过 |

流式探针输出节选：

```
探针 1｜模块选定路径：流式 + Tools + ToolChoiceAny
  stop_reason = "tool_use"
  内容块      = thinking:127、tool_use:0
  tool_use    = submit_modules  input={"modules": ["MOD-CC-001", "MOD-VX-002"], "notes": "…"}
  ✓ 强制工具调用在流式下成立

探针 2｜方案生成路径：两轮工具循环 + 原样回传 assistant 内容块
  第1轮 stop="tool_use" 块=thinking:104、tool_use:0 工具=1
  第2轮 stop="end_turn" 块=thinking:77、text:94 工具=0
  ✓ 工具循环（含 thinking 签名回传）在流式下成立
```

### 未通过项

| 项 | 说明 |
| --- | --- |
| MySQL 连通性 | `192.168.101.26:3306` 当前不可达。**迁移前即存在，与本次改造无关**；处置方法见 [Modification-Summary-20260811-01.md](./Modification-Summary-20260811-01.md) 第五节的授权脚本 |

### 未执行项

- **未做端到端真实提案生成**：受 MySQL 不可达限制，无法通过 API 走完整流程。第五节的流式探针已覆盖两条核心调用路径的技术可行性，但**质量层面的 A/B 对照尚未进行**（见第十一节）。
- **未提交 Git**：所有改动留在工作区未暂存、未提交。

---

## 十、回退方法

改 [`appsettings.json`](../src/Presales.Proposal.AI.Agent/appsettings.json) 的 `Agent` 节三项，**不需要重新编译**：

```jsonc
{
  "Agent": {
    "Provider": "anthropic",   // deepseek → anthropic
    "BaseUrl": "",             // 留空即走 api.anthropic.com
    "Model": "claude-opus-4-8",
    "Effort": "xhigh",         // Anthropic 下可用专有档位
    "Thinking": "adaptive"
  }
}
```

密钥回到 `ANTHROPIC_API_KEY` 环境变量。Anthropic 侧的结构化输出、`cache_control` 1h 缓存、`redacted_thinking` 回传全部自动恢复——这些路径在代码中完整保留，未被删除。

代码级回退：所有改动未提交，`git checkout -- src/ env/` 即可完全还原。

---

## 十一、遗留事项与后续建议

| # | 事项 | 优先级 | 说明 |
| --- | --- | --- | --- |
| 1 | **补程序化参数溯源校验** | 高 | 现有 `VerifyDetailPhaseGrounding()` 只检查"模型有没有回读过阶段一 HTML"，**不检查文中数字是否真的来自知识库卡片**。建议从产出的 HTML/MD 正则抽取 `MOD-\w{2}-\d{3}` 与带单位的数值指标，回查 `references/` 卡片与 `FileTools` 的实际读取记录，对不上的写入运行日志并在前端标红。换模型后这一层从"加分项"变成"必须项"（理由见迁移文档第五节） |
| 2 | **A/B 对照** | 高 | 选 2–3 个已交付过、有人工确认答案的真实提案，两个供应商各跑一遍，对比：幻觉参数数量、★必备章节缺失数、耗时计算一致性、工具循环轮数 |
| 3 | 恢复 MySQL 连通性 | 高 | 阻塞端到端验证 |
| 4 | 观察自动缓存实际命中率 | 中 | DeepSeek 官方明确"尽力而为、不保证命中"，且 `usage` 不回填，只能从账单侧间接观察 |
| 5 | 重新标定 token 用量与成本 | 中 | 两家分词器不同，迁移文档第六节的成本测算基于估算；应从 `RunLogFile` 的 `[用量]` 行取真实数据重算 |
| 6 | 复核超时与并发配置 | 中 | `TimeoutMinutes=60`、`MaxConcurrency=2` 是按 Claude 的吞吐定的，DeepSeek 侧实际延迟未测 |
| 7 | 视实测放开 `Effort` / `Thinking` | 低 | 两项均已验证"接受不报错"，当前保守取 `high` / `off`；若 A/B 显示推理深度不足，可试 `xhigh` / `adaptive` |
| 8 | 双模型方案作为后备 | 低 | 若 A/B 发现幻觉率不可接受：DeepSeek 生成 + Claude 抽检参数溯源，成本仍约为原来的 20% |

---

## 附：本次涉及的关键设计决定

**为什么密钥放配置文件而不是环境变量。**
用户明确要求，且与本项目既有的"现场只认这一份文件"设计一致。该文件已在 `.gitignore` 中（第 14 行），与 MySQL 口令同等对待。更重要的是：让它优先于环境变量，才能避免被机器上残留的旧 Anthropic 密钥顶掉。

**为什么保留 Anthropic 分支而不是删干净。**
迁移文档的实施建议是"先做路线 A 验证质量，把 Provider 做成配置项"。保留分支让"换模型"从一次不可逆的重构，变成一个可以随时回滚、甚至可以按项目区分的运行时选择——在质量对照完成之前，这个可逆性比代码简洁更值钱。

**为什么模块选定不统一走工具调用。**
统一走工具调用能少一个分支，但会改变 Anthropic 路径当前已验证可用的行为。保留结构化输出路径，切回 Claude 时行为与改造前完全一致，A/B 对照才有干净的基线。多出的分支只有约 15 行，且两条路径汇到同一套解析与兜底逻辑。

---

*文档生成：2026-08-15*
