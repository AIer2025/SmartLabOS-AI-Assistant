# Presales Proposal AI Agent — 源码解析总结

> 分析对象：`Presales.Proposal.AI.Agent.slnx`
> 分析基线：提交 `2b2b5e6` · 约 3,000 行自有代码
> 日期：2026-07-16
> 配套交付：[Presales-AI-Agent-系统解析-20260716.pptx](Presales-AI-Agent-系统解析-20260716.pptx)（19 页，含架构图 / 时序图 / 流程图 / 对比表）

---

## 一、系统定位

这个项目是把「售前写方案」做成一条可重复执行的流水线：原本需要 2–3 天的
「读国标 → 选模块 → 配平台 → 组工作站 → 写方案 → 出 WORD」，用一次 Claude 驱动的自动化流程
在数十分钟内产出可交付初稿，且全部技术参数强制溯源到本地知识库。

**五步工作流**：

```
新建项目 → 采集需求 → 模块选定 → 模块确认（人工硬闸门） → 方案生成
```

核心是 `Agent/ClaudeAgentService.cs`（485 行）。

### 四项核心能力在代码中的实际分布

用户需求中提到「模块选定、平台选定、WorkStation 组装、解决方案书写」四项能力。
通读源码后需要澄清一点——**它们并非四个对等的接口**：

| 能力 | 实现位置 | 形态 |
|------|----------|------|
| 模块选定 | `POST /projects/{id}/modules/select` → `SelectModulesAsync` | **独立 API 步骤**，结构化输出（JSON Schema） |
| 平台选定 | `BuildHtmlPhasePrompt`（`ClaudeAgentService.cs:347`） | 阶段一提示词内的一条要求 |
| 工作站组装 | 同上 | 同上 |
| 解决方案书写 | 同上 + `BuildDetailPhasePrompt` | 阶段一 / 阶段二的产出目标 |

即：**只有「模块选定」是独立接口**；平台选定、工作站组装、解决方案书写全部发生在
阶段一那段提示词里，由模型在一次 Agent 工具循环内一次性完成。

---

## 二、系统架构（五层单体）

```
表现层      wwwroot 原生前端（同源，无 CORS）        | Swagger（仅开发环境）
接口层      Presales / Records / Transfer / Meta / Picker  五个 Controller
调度层      GenerationQueue（有界 Channel 200）
            GenerationWorker（BackgroundService + 信号量 2 + 60 分钟超时）
            JobRegistry（ConcurrentDictionary 内存态，供轮询）
领域层      ClaudeAgentService（核心） · FileTools（受控读写）
            ModuleCatalog（ID 校验） · DocxExporter（Pandoc / OpenXML）
基础设施    MySQL 8（Dapper） · 本地知识库文件系统 · Anthropic Messages API
```

**请求路径**：前端 `POST /generate` → 控制器仅做校验与入队并立即 `202 Accepted`
→ Worker 出队执行 → 前端轮询 `/generate/status` 拿阶段、日志与耗时。

### 技术栈

| 层 | 技术 |
|----|------|
| 后端 | C# 14 / .NET 10 / ASP.NET Core Web API |
| 大模型 | Anthropic 官方 NuGet 包 12.34.1 · `claude-opus-4-8` · Effort=high |
| 数据访问 | Dapper 2.1.79 + MySqlConnector 2.6.1（全参数化） |
| 数据库 | MySQL 8.x · `SmartLabOS-Presales-AI`（2 张表） |
| 文档导出 | DocumentFormat.OpenXml 3.5.1 + Pandoc（可选） |
| 前端 | 原生 HTML / CSS / JS（零依赖、零构建链） |

---

## 三、处理逻辑

### 3.1 模块选定 —— 双层反幻觉

1. **结构化输出**：`OutputConfig.Format` 传入 JSON Schema（`additionalProperties=false`），
   由 API 层保证返回结构，不再让模型「写 JSON 文件再回读」。
2. **目录校验兜底**：Schema 只能保证「是个字符串数组」，无法保证 `MOD-CC-999` 真的存在。
   因此返回值再过一遍 `ModuleCatalog.Normalize()`：以**文件名**为模块 ID 校验存在性、
   统一大写、去重、保序。幻觉编号在此被静默丢弃。

### 3.2 方案生成 —— 两阶段 Agent 工具循环

| 阶段 | 产出 | 要点 |
|------|------|------|
| 阶段一 | `1.html`、`2.html`… + `xxx-模块-URS.html` | 逐个国标读前处理流程；只能从「已确认模块」选型；平台节拍对齐 600 秒 |
| 阶段二 | `<项目名>-详细设计方案.md` → 自动导出 `.docx` | 读回阶段一自己写的 HTML 整合；指标必须「数值+单位+精度+通讯协议」 |

`RunToolLoopAsync` 是心脏：不用任何 Agent 框架，用一个 `for` 循环把
「模型请求工具 → C# 执行 → 结果回喂」串起来，最多 40 轮。三个关键细节：

- **thinking 必须原样回传**（含 `Signature`，不可篡改），否则后续请求被拒；
- **assistant 消息完整重建**（text / thinking / tool_use 按原顺序）；
- **工具结果以 user 角色追加**，`ToolResultBlockParam` 携带 `ToolUseID` 配对。

### 3.3 受控文件工具 —— 安全边界

模型触碰磁盘的唯一通道是三个工具（`list_kb_dir` / `read_kb_file` / `write_output`）。
`write_output` 有四道校验：非空 → 纯文件名（禁路径分隔符）→ 扩展名白名单（`.html`/`.md`）
→ `GetFullPath` 后复核未越界。

---

## 四、优点分析

1. **安全边界是设计出来的，不是补上去的**
   `FileTools` 不是「加了校验的工具类」，而是从架构上确立「模型不直接碰磁盘」的原则。
   把旧版 `bypassPermissions` 的越权面从「整个项目根」压缩到「单个项目输出目录」。

2. **反幻觉是双层的：结构 + 内容**
   以文件名为模块 ID 这一决定尤其巧妙——绕开 YAML 解析的脆弱性，
   让「ID 存在性」变成一次文件系统查询，可靠且零歧义。

3. **人机边界划得清楚**
   AI 做推理（选型、组装、书写），人做决策（模块确认），代码做确定性的事
   （docx 转换、ID 校验、参数化 SQL）。「模块确认」硬闸门与「拒绝用模型渲染 docx」
   是同一种判断力的两次体现。

4. **中间产物即审核点**
   阶段一的 HTML 落盘后可被人工打开、核对、修改，阶段二再读回整合。
   工程师被放在流程内部而非末端。代价是阶段二要重读 HTML、多花一份输入 token,
   属于用成本换可控性的自觉取舍。

5. **成本意识贯穿实现**
   system 前缀带缓存、知识库按需读取而非全量塞入、每轮打印
   `in / out / cacheRead / cacheWrite` 四项用量、模版进程内缓存一次读取。

6. **工程基本功扎实**
   机密零入仓、Swagger 仅开发环境、全参数化查询、有界队列产生背压、
   任务级超时与令牌联动、UTF-8 BOM 兼顾记事本、静态文件禁缓存。

### 相较旧实现（DataMaintenance）的跃迁

| 分析项 | 旧实现 | 本项目 |
|--------|--------|--------|
| 大模型调用 | `Process.Start(claude -p …)` 子进程 | 官方 SDK `client.Messages.Create`，进程内 HTTPS |
| 权限模型 | `bypassPermissions` + `--add-dir 项目根` | 3 个受控工具，读限知识库、写限本项目 + 扩展名白名单 |
| 结构化结果 | 让模型写 JSON 文件再扫盘回读 | `OutputConfig.Format` 传 JSON Schema |
| 知识库成本 | 每次全盘读入 | system 放索引，卡片按需读取；前缀走提示词缓存 |
| 错误处理 | 字符串匹配 stdout 猜错误 | 强类型异常分层 + SDK 自动退避重试 + usage 可读 |
| 后台任务 | `Task.Run` 即发即忘 | 有界 Channel + BackgroundService + 并发上限 + 状态落库 |
| 机密管理 | 明文口令入仓 | user-secrets / 环境变量注入，仓库零机密 |
| WORD 交付 | 依赖 CLI 的 docx 技能 | 内置 Pandoc / OpenXML 双路径，离线、零额外成本 |

---

## 五、局限与风险（代码级复核发现）

> 以下均为**通读源码后确认**的问题，非泛泛而谈。其中若干项 README 未提及，
> 甚至与 README 的说法相反。

### 高 · 全系统零鉴权，且能在服务器上弹进程

`Program.cs` 无任何认证 / 授权中间件，`AllowedHosts=*`。
`PickerController.cs:58` 更会在服务端 `Process.Start(powershell.exe)` 弹出原生对话框。
它假设「只在 localhost 本机跑」，**但没有任何代码强制这一点**——
一旦绑到 `0.0.0.0`，同网段任何人都能删项目、跑生成、烧 API 额度。

### 高 · 截断被当成「完成」

`ClaudeAgentService.cs:210`：若模型未请求工具即视为本阶段结束，
只把 `stop_reason` 打进日志、不做判断。当输出撞上 `MaxTokens=16000` 上限时，
`stop_reason=max_tokens`，**半截的 HTML 会被当作成功交付**，任务标记 `succeeded`。
长方案下这是最可能的静默失败。

### 中 · 队列与任务状态全在内存

`Channel` 与 `JobRegistry` 均为进程内。重启后排队任务消失，
运行中任务在库里永远停在 `running`（无启动时回收逻辑）。
**README 宣称的「进程退出/回收时不再凭空丢任务」尚未兑现。**

### 中 · ModuleCatalog 的隐藏 O(N×97)

`Find()` 每次调用 `All()`，而 `All()` 每次都 `EnumerateFiles` 并对 97 个文件
逐个 `GetLastWriteTimeUtc` 算签名。`Decorate()` 又对每个模块 ID 调一次 `Find()`
——列表接口一次请求可产生数千次文件系统 stat。

### 中 · 缓存窗口与阶段耗时不匹配

`CacheControlEphemeral` 默认 5 分钟 TTL，而阶段一常远超 5 分钟。
阶段二起手大概率已失效，需重新写入缓存——降本效果打折。

### 低 · 零自动化测试 · 配置静默降级

- 全仓无单元 / 集成测试，重构无保护网。
- `Effort` 配 `"xhigh"` 会被 `ParseEffort` 静默降为 `high`。
- 下载接口的 `full.StartsWith(dir)` 未带路径分隔符
  （当前因文件名校验而**不可利用**，但属脆弱写法）。

### 补充

- 非流式调用使长任务无法实时吐字（前端只能轮询日志）；
- 生成失败无断点续跑，只能整阶段重来；
- `usage` 仅进日志未落库，无法按项目核算成本。

---

## 六、改进路线图

优先级判据：**先补「会造成事故或静默错误」的，再补「会拖慢日常」的，
最后才是「让系统更聪明」的。**

### 近期（1–2 周）· 止血与正确性

| # | 事项 | 说明 |
|---|------|------|
| 1 | 加鉴权 + 绑定回环 | 最小可用 API Key 中间件或 Windows 集成认证；Kestrel 显式绑 `127.0.0.1`；`PickerController` 加 `IsLocal` 断言 |
| 2 | 处理 `stop_reason` | `== max_tokens` 时自动续写（把已生成内容回喂继续）或直接标记 `failed`，杜绝半截交付 |
| 3 | ModuleCatalog 缓存 | 改为 `FileSystemWatcher` 失效 + 字典查找；`Decorate` 一次取全表，消除 O(N×97) |
| 4 | 补测试基线 | `FileTools` 越界用例、`ModuleCatalog.Normalize` 幻觉过滤、`DocxExporter` 双路径——三处最该有网的地方 |

### 中期（1–2 月）· 可靠性与体验

| # | 事项 | 说明 |
|---|------|------|
| 1 | 任务持久化 | 队列落库或上 Redis/RabbitMQ；启动时回收 `running` 僵尸任务；支持断点续跑与手动取消 |
| 2 | 流式输出 | `Messages.CreateStreaming` + SSE 推送，替代前端轮询，长任务实时可见 |
| 3 | 成本可核算 | `usage` 四项落 Generation 表，按项目 / 阶段统计 token 与费用 |
| 4 | 缓存策略优化 | 启用 1 小时 TTL 缓存或在阶段间保活；评估把模版与模块清单拆成独立缓存断点 |

### 远期（季度）· 质量与规模

| # | 事项 | 说明 |
|---|------|------|
| 1 | 自动质检闭环 | 生成后跑校验 Agent：指标是否量化、耗时是否与阶段一一致、模块是否越出清单——不合格自动返工 |
| 2 | 知识库检索升级 | 模块卡片向量化 + 混合检索，替代「全清单进 system」，模块规模上千时仍可控 |
| 3 | 多方案并行择优 | 同一需求并行生成 N 套选型，用评审 Agent 打分择优 |
| 4 | 产物版本化 | 提案入 Git 或对象存储，支持版本对比与回滚，沉淀为可复用资产 |

---

## 七、结论

**架构上**：五层单体，请求即返回、重活进后台队列。没有微服务、没有前端构建链、
没有 Agent 框架——用最少的抽象把事情做成，新人一天可通读全部核心路径。

**方法上**：AI 只做需要判断力的事：选型推理、方案书写。结构校验、ID 存在性、
docx 转换这些确定性问题，一律交给确定性代码。这条边界是整个项目最值钱的设计观。

**下一步**：当前形态更像一个「跑得通的强原型」：正确性与安全的关键洞
（鉴权、截断静默）需要优先补上，之后才谈得上流式、质检与规模化。
