# SmartLabOS Presales AI Assistant

用 **C# 14 + .NET 10 + ASP.NET Core** 对 [`DataMaintenance`](../DataMaintenance) 的**完整移植版**，含两大模块（原始两栏界面，操作完全一致）：

1. **主数据维护**（移植自 DataMaintenance，逐字保留 UI 与功能）：5 类主数据（模块/平台基类/工作站/解决方案/项目）的
   CRUD、自定义可见列、`data_json` 折叠树形编辑器、批量导出 YAML-MD、导入 Excel（.xlsx/.xlsm，零依赖解析）、本地文件/目录选择器。
2. **售前方案自动生成**（AI Agent）：对同名功能的**安全改造版**——把「启动 Claude Code CLI 子进程 + 扫盘回读」
   换成 **Anthropic 官方 C# SDK 直调 Messages API**，落地了《20260702-AI-Agent-Summary》分析报告中的“路线 A”。

> 两模块共用同一个本地 MySQL 库 `SmartLabOS-Presales-AI`。前端顶部通过「主数据维护 / 售前方案自动生成」两个标签切换。

| 层 | 技术 |
|----|------|
| 后端 | C# 14 + ASP.NET Core Web API（.NET 10） |
| 大模型 | Anthropic Claude（`claude-opus-4-8`），官方 NuGet 包 `Anthropic` |
| 数据访问 | Dapper + MySqlConnector（参数化、防注入） |
| 数据库 | 本地 MySQL 8.x（复用 `SmartLabOS-Presales-AI` 库） |
| 前端 | 原生 HTML/CSS/JS（零依赖，与 API 同源） |

## 相比旧实现的改进（对应分析报告）

| 分析项 | 旧实现（DataMaintenance） | 本项目 |
|--------|--------------------------|--------|
| 大模型调用 | `Process.Start(claude -p …)` 子进程 | **官方 SDK `client.Messages.Create`（进程内 HTTPS）** |
| 权限/安全 | `--permission-mode bypassPermissions --add-dir 项目根`（越权） | **受控文件工具**：读只限 `references/`、`potocol/`、本项目输出目录；写只限本项目目录 + 扩展名白名单 |
| 结构化结果 | 让模型写 JSON 文件再回读 | **结构化输出** `OutputConfig.Format`（JSON Schema） |
| 知识库成本 | 每次全盘读入 | **提示词缓存** `CacheControlEphemeral`（稳定知识库前缀命中降本） |
| 错误处理 | 字符串匹配 stdout | **强类型异常** + SDK 自动退避重试；可读 `usage`（token 用量） |
| 后台任务 | `Task.Run` 即发即忘、内存态 | **`IHostedService` + 有界队列 + 并发上限**，状态落库 |
| 配置 | 连接串/路径散落在 `appsettings.json` 与代码常量 | **`env/Presales-AI-Agent-config.json` 单一来源**，优先级最高，启动自检打印实际生效值 |
| 机密 | 明文口令入仓 | API Key 走**环境变量/user-secrets**；口令集中在 env 配置文件（已加入 `.gitignore`） |
| 交付物 | HTML/URS + WORD(docx，靠 CLI 的 docx 技能) | HTML/URS + **《详细设计方案》Markdown**，并**内置导出 .docx**（Pandoc / OpenXML，见下文） |

## 运行前置

- **.NET SDK 10+**（本机已装 10.0.301）
- **本地 MySQL 8.x** 且已存在 `SmartLabOS-Presales-AI` 库（DataMaintenance 已建；或执行 `database/01-presales-schema.sql`）
- **Anthropic API Key**

## 配置：`env/Presales-AI-Agent-config.json`（唯一需要改的文件）

MySQL 连接参数与 ClaudeAI 运行目录**全部**来自项目根的 `env/Presales-AI-Agent-config.json`。
改配置只改这一个 JSON，不需要重新编译；`appsettings.json` 中已不再保留任何连接串或路径。

```jsonc
{
  "DB_Connect": {
    "DB_IP":       "192.168.101.26",   // MySQL 地址
    "DB_Port":     "3306",             // MySQL 端口
    "DB_User":     "root",             // MySQL 用户
    "DB_Password": "……",               // MySQL 口令
    "DB_Name":     "SmartLabOS-Presales-AI",  // 可省略，默认同此
    "DB_SslMode":  "None",                    // 可省略，默认 None
    "DB_CharSet":  "utf8mb4"                  // 可省略，默认 utf8mb4
  },
  "ClaudeAI_Env": {
    "Module_Path":          "……\\references\\01-modules",       // 模块目录
    "Platform_Path":        "……\\references\\02-platforms",     // 平台目录
    "Pallet_Path":          "……\\references\\06-pallet",        // 托盘目录
    "Protocol_Path":        "……\\potocol\\MD",                  // 国标 Protocol 目录
    "Proposal_Output_Path": "……\\projects",                      // 提案输出目录
    "Proposal_Template":    "……\\references\\09-ProposalTemplate",// 提案模版目录（见下节）
    "Template_Path":        "……\\references\\_templates"          // 标准/模版文件目录（见下节）
  }
}
```

生效规则：

- **加载顺序**：环境变量 `PRESALES_AGENT_CONFIG`（指向文件或其所在目录）→ 从内容根 / 程序目录 / 当前目录逐级向上找 `env/Presales-AI-Agent-config.json`。因此 IDE 调试与 `publish` 后运行找到的是同一份文件。
- **优先级最高**：该文件的值覆盖 `appsettings.json` 与同名环境变量，避免「改了 JSON 却被残留的 `ConnectionStrings__SmartLabOS` 顶掉」。
- **派生路径**：`references/` 根与项目根由 `Module_Path` 逐级上推得出，《详细设计方案》章节标准与兜底模版据此定位。
- **越权边界同源**：配置里的四个知识库目录会自动成为模型的只读白名单（`FileTools`），把某个目录指到 `references/` 之外也不会被误判越界。
- **文件缺失或写错**：服务照常启动，但启动日志会打红字错误，且访问数据库时抛出指向该文件的清晰异常。
- **占位符检测**：`DB_Password` / `DB_Name` 仍是模板占位符时（照抄 `.sample.json` 忘了改），启动日志与 `/api/presales/config` 的 `warnings` 会点名提示——否则表现为"连不上"，报错却指向网络/授权，很难往这想。

### 文件编码

**正式配置文件请只写 ASCII**（路径、IP、账号本就无需中文），中文说明留在 `.sample.json` 里。原因：

- 该 JSON 存的是 **UTF-8 无 BOM**。`JSONEdit` 等编辑器没有 UTF-8 自动探测，会按系统代码页（简中 Windows 为 GBK）解码，中文全部显示成乱码；Notepad++、VS Code 有探测逻辑所以正常。
- 若确实要在正式配置里写中文，把文件**另存为 UTF-8 with BOM**。程序侧对 BOM 免疫（`File.ReadAllText` 自动识别并剥离），`Modify-mySQL-Access-Right-*.ps1` 也已实测通过，两者都不受影响。`.sample.json` 就是带 BOM 存的，在 JSONEdit 中可正常显示。
- 反过来不要用 GBK 存盘：程序按 UTF-8 读取，中文会变成乱码或解析失败。

**Anthropic API Key 不进这个文件**（也不进任何配置文件）：

```powershell
setx ANTHROPIC_API_KEY "sk-ant-..."        # 新开终端生效
# 或： cd src\Presales.Proposal.AI.Agent; dotnet user-secrets set "Agent:ApiKey" "sk-ant-..."
```

> 未配置 API Key 时服务仍可启动与浏览，只是在触发「模块选定 / 方案生成」时明确报错。

## 提案模版目录（`Proposal_Template`）

交付物的版式以该目录为唯一权威来源。目录里放的是**各类交付物的真实样例**，程序按文件名/扩展名自动归位到五个角色（`ProposalTemplateSet`），无需在配置里逐个指定：

| 角色 | 匹配规则 | 本项目当前对应 | 怎么用 |
| --- | --- | --- | --- |
| `solution-html` | `.html` 且文件名不含 `URS` | `1.html` | 全文内联进带缓存的 system 前缀，阶段一写 `1.html`/`2.html`… 时遵循 |
| `urs-html` | `.html` 且文件名含 `URS` | `微量加注-模块-URS.html` | 阶段一提示词点名路径，写 URS 前强制 `read_kb_file` |
| `detail-md` | `.md` 且非 Summary/总结 | `测试项目-详细设计方案.md` | 阶段二提示词点名路径，动笔前强制 `read_kb_file` |
| `summary-md` | `.md` 且以 Summary 开头或含「总结」 | `Summary-输出总结.md` | 阶段二收尾时参照 |
| `word-style` | `.docx` | `测试项目-详细设计方案.docx` | 转 WORD 的样式来源，模型不接触 |

同一角色有多份时取体积最大的那份（通常最完整）。

**为什么不全部内联**：解决方案 HTML 与详细设计方案样例合计十几万字，全塞进 system 前缀会让每一轮工具循环的输入成本失控。所以只内联阶段一必用的 `solution-html`（它进缓存前缀，跨轮跨阶段命中），其余改为「提示词点名路径 + 强制读取」，读到的内容同样落在该阶段的缓存里。

**WORD 版式**：`word-style` 的 .docx 同时供两条导出路径使用 —— Pandoc 的 `--reference-doc`，以及未装 Pandoc 时内置 OpenXML 转换器的样式克隆（复制 styles.xml 与 theme，不带入模版正文）。两条路径产出的 WORD 因此版式一致。模版缺 `Heading1` 样式或读取失败时自动退回内置样式，不会中断导出。`GET /api/dev/docx-selftest` 会回报本次实际用的 `styleSource`。

**模版是版式样板，不是内容来源** —— 提示词中已明确约束：照抄结构、章节顺序、表格列设计与图示风格，但样例里的设备型号、数值、客户信息一律不得进入新方案。

## 标准/模版文件目录（`Template_Path`）

`references/_templates`。与上一节的**提案模版目录**分工不同：那边是「版式样例」，这边是「标准与兜底」。

| 文件 | 角色 | 何时用 |
| --- | --- | --- |
| `02-SmartLabOS提案标准-详细设计方案版.md` | 《详细设计方案》**章节范围标准**（★必备 / ○按需） | **每次生成都内联**进 system 前缀 |
| `99-SmartLabOS-技术提案输出模版.html` | 解决方案 HTML 模版 | 仅当提案模版目录没有 `solution-html` 时 |
| `98-SmartLabOS-WORD参考样式.docx` | WORD 参考样式 | 仅当提案模版目录没有 `.docx` 时 |
| `01~05-卡片输入录入程序…xlsm` | 主数据导入的 Excel 卡片模板 | 程序不主动读；导入时在界面上选具体文件 |

两点要注意：

- **`02-…详细设计方案版.md` 是活引用，不是模版样例。** 它是★必备项的检查清单，与 09 目录里的「详细设计方案样例」是两回事，不要当重复删掉。它缺失时程序只会退回一段兜底文字继续跑——完整性校验事实上失效且不报错，因此启动自检会为此打红字错误，`/api/presales/config` 中它标记为 `inUse: "always"`。
- **本目录需可写**：未装 Pandoc 参考文档时，程序会往 `98-…docx` 这个路径生成一份。

配置可省略 `Template_Path`，省略时按 `references/_templates` 推导。但知识库目录一旦挪出默认布局，推导会失效，所以部署时建议写明。

### 已废弃的知识库目录

`references/04-solutions` 已不再具备参照意义，程序中对它的引用已全部移除：

- 不再出现在 system 提示词的知识库导航里；
- `list_kb_dir` 列目录时被隐去；
- `read_kb_file` 读取时返回明确拒绝理由（而不是含糊的「路径越界」，否则模型会反复重试浪费轮次）。

该清单可用配置键 `Presales:DeprecatedDirs`（分号分隔的绝对路径）覆盖，默认即上述目录。

### 配置自检

启动日志会打印配置文件的**实际读取路径**、生效的各目录、MySQL 目标（脱敏）与连通性。
也可随时访问 `GET /api/presales/config`（不返回口令）：

```json
{
  "configFile": "…\\env\\Presales-AI-Agent-config.json",
  "loaded": true, "warnings": [],
  "database": { "target": "root@192.168.101.26:3306/SmartLabOS-Presales-AI（口令 已配置，11 位）", "connected": true },
  "paths": { "modules": { "path": "…\\references\\01-modules", "exists": true }, "…": "…" },
  "proposalTemplates": [{ "role": "solution-html", "file": "1.html", "label": "…" }, "…"],
  "deprecatedDirs": ["…\\references\\04-solutions"],
  "moduleCount": 97, "agentReady": true
}
```

## MySQL 连不上：`Host 'x.x.x.x' is not allowed to connect`

端口通、但账号只对 `localhost` 授权，而程序按 `DB_IP` 连过来，服务端看到的来源主机是那个 IP。
用 [`database/Modify-mySQL-Access-Right-20260811.ps1`](../database/Modify-mySQL-Access-Right-20260811.ps1) 一次性处理防火墙、`bind-address`、账号授权三层：

```powershell
cd database

# 先体检，不做任何修改：打印将执行的每条 SQL（口令自动脱敏）
pwsh -File .\Modify-mySQL-Access-Right-20260811.ps1 -WhatIf

# 确认无误后执行（加防火墙规则需以管理员身份运行；否则加 -SkipFirewall）
pwsh -File .\Modify-mySQL-Access-Right-20260811.ps1

# 更稳妥：授权整个网段而不是单个 IP，也不要用 '%'
pwsh -File .\Modify-mySQL-Access-Right-20260811.ps1 -AllowHost '192.168.101.%'
```

连接参数直接读 `env/Presales-AI-Agent-config.json`，与程序完全同源，不会出现「脚本改的账号和程序用的不是同一个」。
口令通过临时 option 文件传给 `mysql.exe`（不进命令行、不进进程列表），用完即删。
脚本最后会按程序那套参数实连一次做验证。

## 运行

```powershell
cd src\Presales.Proposal.AI.Agent
dotnet run
```

- 前端界面：<http://localhost:5xxx/>（端口见 `Properties/launchSettings.json`）
- 健康检查：`/api/health` → `{ "db": "connected", "agent": "ready" }`
- Swagger（仅开发环境）：`/swagger`

## 五阶段工作流

1. **新建项目** → 2. **采集需求**（现状/国标/挑战/期望/流程范围…）→
3. **模块选定 → 模块确认**（Claude 结构化输出推荐，人工确认）→
4. **方案生成**（阶段一：解决方案 HTML + 模块 URS；阶段二：《详细设计方案》Markdown）→
5. 在「生成的文件」中查看/下载产物。

## docx 导出（《详细设计方案》）

阶段二产出的《详细设计方案》Markdown 会自动导出为 `.docx`，两条内置路径按可用性自动选择，无需人工干预：

1. **Pandoc（首选，保真度最高）** —— 若本机安装了 pandoc（或在 `Agent:PandocPath` 指定可执行文件路径），
   用 `pandoc <md> --from gfm -o <docx>` 转换我们已生成并可复核的 Markdown。安装：`winget install --id JohnMacFarlane.Pandoc` 或见 <https://pandoc.org/installing.html>。
2. **内置 OpenXML 转换器（兜底，离线可用）** —— 未安装 pandoc 时自动回退，用 `DocumentFormat.OpenXml` 在进程内把
   Markdown（标题 / 段落 / 有序无序列表 / GFM 表格 / 代码块 / 粗斜体 / 行内代码）转为 `.docx`，**无任何外部依赖、不耗额外 API**。

> 本机已自检通过：`GET /api/dev/docx-selftest`（仅开发环境）→ `{"ok":true,"engine":"openxml","size":1660}`。

**配置**（`appsettings.json` 的 `Agent` 节）：

| 键 | 默认 | 说明 |
| --- | --- | --- |
| `EnableDocx` | `true` | 是否导出 docx（置 `false` 只留 Markdown） |
| `PandocPath` | `""` | pandoc 可执行文件路径；留空则尝试 PATH，失败回退 OpenXML |

**第三条可选路径 —— Anthropic Agent Skills 容器（默认不启用）**：也可让 Claude 用 `code_execution` + 官方 `docx` 技能在托管容器内直接渲染 docx——
经 `client.Beta.Messages.Create` 传 `Container.Skills=[{type:"anthropic",skill_id:"docx"}]` + `BetaCodeExecutionTool20250825`，
betas 传 `code-execution-2025-08-25` 与 `skills-2025-10-02`，再用 `client.Beta.Files.Download(fileId)` 取回产物。
本项目未默认采用它，因为它会**额外消耗 API 成本**、依赖 beta 特性，且是“重新生成”而非转换我们已复核的 Markdown；内置 Pandoc/OpenXML 路径更确定、离线、零额外成本。若确需，可在 `DocxExporter` 中增加该分支。

## 本机验证结果（已实际运行）

```text
构建         → 0 警告 0 错误（.NET 10.0.301 / C# 14）
/api/health → {"db":"connected","agent":"no-api-key"}   ← 本地 MySQL 连接 OK
options     → 11 个国标；processScope 枚举正确
modules     → 97 个模块，首个 MOD-CC-001 800样品存储模块(存储)
/           → index.html 200（前端已接通）
projects    → 读到既有项目「测试项目」（复用旧库数据）
docx 自检   → {"ok":true,"engine":"openxml","size":1660}
```

> `agent:"no-api-key"` 属预期——配置 `ANTHROPIC_API_KEY` 后即可进行模块选定与方案生成。

## 目录结构

```
Presales.Proposal.AI.Agent/
├─ env/Presales-AI-Agent-config.json     ★ 唯一外部配置：MySQL 连接 + ClaudeAI 运行目录
├─ database/01-presales-schema.sql       库表（幂等）
├─ database/Modify-mySQL-Access-Right-*.ps1  MySQL 远程访问授权（防火墙+bind+GRANT）
├─ src/Presales.Proposal.AI.Agent/
│   ├─ Program.cs                         启动/DI/中间件/自检（首行加载 env 配置）
│   ├─ appsettings.json                   仅 Agent 模型参数与日志（无路径、无机密）
│   ├─ Configuration/AgentEnvFile.cs      ★ env 配置文件定位/解析/映射为 IConfiguration 键
│   ├─ Configuration/PresalesConfig.cs    路径 + Agent 设置 + 机密解析
│   ├─ Domain/PresalesModels.cs           领域模型（售前）
│   ├─ Data/
│   │   ├─ PresalesRepository.cs          售前 MySQL 仓储(Dapper)
│   │   ├─ SmartLabRepository.cs          ◆ 主数据维护仓储(移植)
│   │   └─ EntityRegistry.cs              ◆ 5 实体/列元数据(移植)
│   ├─ Controllers/
│   │   ├─ PresalesController.cs          售前 REST 接口
│   │   ├─ MetaController.cs              ◆ 元数据/健康(移植)
│   │   ├─ RecordsController.cs           ◆ 5 实体 CRUD(移植)
│   │   ├─ TransferController.cs          ◆ 导出 YAML-MD / 导入 Excel(移植)
│   │   └─ PickerController.cs            ◆ 本地文件/目录选择器(移植)
│   ├─ Services/                          ◆ 主数据导入导出(移植)：
│   │   ├─ CardTemplateImporter.cs / XlsxReader.cs
│   │   └─ YamlEmitter.cs / YamlMdCard.cs
│   ├─ Agent/
│   │   ├─ ClaudeAgentService.cs          ★ 官方 SDK：模块选定(结构化)+方案生成(工具循环)+缓存
│   │   ├─ FileTools.cs                    ★ 受控读写(安全边界) + 废弃目录拦截
│   │   ├─ ProposalTemplateSet.cs          ★ 提案模版目录归类(5 个角色)
│   │   ├─ DocxExporter.cs                 ★ docx 导出(Pandoc / 内置 OpenXML，套用模版样式)
│   │   ├─ ModuleCatalog.cs               模块目录/ID校验
│   │   ├─ JobModels.cs                    任务状态 + 登记表
│   │   ├─ GenerationQueue.cs             有界后台队列
│   │   └─ GenerationWorker.cs            BackgroundService 消费者
│   ├─ Controllers/PresalesController.cs  REST 接口(前后端契约同旧版)
│   └─ wwwroot/                           前端(index/css/js)
```

## 安全说明

- 模型对文件系统的能力被收敛为**四个受控工具**（`list_kb_dir`/`read_kb_file`/`write_output`/`append_output`），
  读只限 env 配置声明的知识库根与本项目目录、写只限本项目目录且仅 `.html/.md`，从根本上消除旧版 `bypassPermissions` 越权面。
  读白名单与提示词里的路径同源（都来自 `PresalesPaths`），不会出现「提示词说得通、工具判越界」。
- 生产部署：Swagger 默认仅开发环境开放；机密一律走环境变量/密钥库；建议前置反向代理与鉴权。
