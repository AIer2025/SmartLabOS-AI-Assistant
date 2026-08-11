# SmartLabOS Presales AI Agent 改造总结（2026-08-11 第 01 次）

> 范围：`C:\TestClaude\SmartLabOS-AI-Assistant\Claude-API-Key-Presales-AI-Agent`
> 结果：`dotnet build` 0 警告 0 错误；配置加载、模版归位、docx 两条导出路径、MySQL 诊断均已实机验证。

## 目录

- [一、总体目标](#一总体目标)
- [二、配置外置：env/Presales-AI-Agent-config.json](#二配置外置envpresales-ai-agent-configjson)
- [三、提案模版目录 Proposal_Template](#三提案模版目录-proposal_template)
- [四、废弃 references/04-solutions 的引用](#四废弃-references04-solutions-的引用)
- [五、MySQL 远程访问授权脚本](#五mysql-远程访问授权脚本)
- [六、文件变更清单](#六文件变更清单)
- [七、验证记录](#七验证记录)
- [八、遗留事项与待确认](#八遗留事项与待确认)
- [九、补充：Template_Path 显式外置](#九补充template_pathreferences_templates显式外置)
- [十、补充：配置文件编码与占位符检测](#十补充配置文件编码与占位符检测)

---

## 一、总体目标

| # | 需求 | 状态 |
| --- | --- | --- |
| 1 | MySQL 与 ClaudeAI 运行环境变量从 `env/Presales-AI-Agent-config.json` 读取，取消程序中的硬编码 | ✅ 完成 |
| 2 | 生成的 HTML / MD / WORD 遵照 `references/09-ProposalTemplate` 的模版格式 | ✅ 完成 |
| 3 | 删除程序中对 `references/04-solutions` 的引用，并评估影响 | ✅ 完成（零影响，见第四节） |
| 4 | 配置文件更新为最新内容（新增 `DB_Name`/`DB_SslMode`/`DB_CharSet`/`Proposal_Template`） | ✅ 完成 |
| 5 | 给出 MySQL 访问权限添加方法（PowerShell 形式） | ✅ 完成（脚本已交付，未代为执行） |

---

## 二、配置外置：`env/Presales-AI-Agent-config.json`

### 2.1 设计

新增 [`Configuration/AgentEnvFile.cs`](../src/Presales.Proposal.AI.Agent/Configuration/AgentEnvFile.cs)，在 `Program.cs` 首行把该 JSON 解析成标准 IConfiguration 键，**以内存配置层追加到最高优先级**。下游代码（仓储、`PresalesPaths`）键名不变，因此无需感知它的存在。

| 配置文件节点 | 映射到的配置键 |
| --- | --- |
| `DB_Connect.*` | `ConnectionStrings:SmartLabOS` |
| `Module_Path` | `Presales:ModulesDir` |
| `Platform_Path` | `Presales:PlatformsDir` |
| `Pallet_Path` | `Presales:PalletDir` |
| `Protocol_Path` | `Presales:ProtocolDir` |
| `Proposal_Output_Path` | `Presales:ProjectsDir` |
| `Proposal_Template` | `Presales:ProposalTemplateDir` |
| `Template_Path`（可省略） | `Presales:TemplatesDir` |
| （派生）`Module_Path` 上推一级 | `Presales:ReferencesDir` |
| （派生）再上推一级 | `Presales:ProjectRoot` |

`DB_Name` / `DB_SslMode` / `DB_CharSet` 为可选键，缺省 `SmartLabOS-Presales-AI` / `None` / `utf8mb4`。

### 2.2 几个关键决定

**连接串用 `MySqlConnectionStringBuilder` 组装，不做字符串拼接。**
口令 `#Bqjx@2026#` 含 `#`；若口令里出现 `;` 或 `'`，手工拼接会静默产生错误的连接串。Builder 负责转义。

**配置文件优先级高于环境变量。**
放在最后一层意味着它会覆盖 `appsettings.json` 和同名环境变量。这是刻意的——避免"改了 JSON 却被残留的 `ConnectionStrings__SmartLabOS` 顶掉"这类幽灵问题。
（Anthropic API Key 不在此文件内，仍走 `ANTHROPIC_API_KEY` / user-secrets。）

**文件查找顺序**：环境变量 `PRESALES_AGENT_CONFIG`（可指文件或目录）→ 从内容根 / 程序目录 / 当前目录逐级向上找 `env/Presales-AI-Agent-config.json`（最多 8 层）。
IDE 调试（内容根在 `src/项目/`）与 publish 后运行（bin 目录）因此读到同一份文件。

**读白名单与提示词同源。**
`PresalesPaths.KnowledgeRoots` 收集配置声明的全部知识库目录（去重、剔除被其它根包含的子目录），`FileTools` 用它做读权限判定，`ClaudeAgentService` 用它渲染提示词路径。
改造前只放行 `ReferencesDir`+`ProtocolDir`——一旦把平台或托盘目录配到 `references/` 之外，模型按提示词去读会被判越界，方案就会缺参数。这是本次必须一并修掉的隐患。

**文件缺失不阻断启动。**
启动日志打红字错误，访问数据库时抛出指向该文件的清晰异常。避免"服务起不来、也看不到原因"。

### 2.3 清理

`appsettings.json` 删除了连接串占位符与整个 `Presales` 路径节，只保留 `Agent` 模型参数与日志配置——单一来源，不留第二处可改的地方。

### 2.4 自检

启动日志打印配置文件的**实际读取路径**、生效的各目录、MySQL 目标（脱敏）与连通性。
另新增 `GET /api/presales/config`（不返回口令），回报配置来源、各目录是否存在、模版归位结果、废弃目录清单、模块数、Agent 就绪度。

---

## 三、提案模版目录 `Proposal_Template`

### 3.1 认识

`references/09-ProposalTemplate` 里放的是**五类交付物的真实样例**（不是填空模版）：

```text
1.html                          73 KB   解决方案 HTML
微量加注-模块-URS.html          18 KB   模块 URS HTML
测试项目-详细设计方案.md         58 KB   详细设计方案 Markdown
测试项目-详细设计方案.docx       49 KB   同上的 WORD 版
Summary-输出总结.md            4.5 KB   输出总结
运行日志-方案生成-*.log                 无关，自动忽略
```

### 3.2 实现

新增 [`Agent/ProposalTemplateSet.cs`](../src/Presales.Proposal.AI.Agent/Agent/ProposalTemplateSet.cs)，扫描目录并按文件名/扩展名归位到五个角色，无需在配置里逐个指定路径：

| 角色 | 匹配规则 | 使用方式 |
| --- | --- | --- |
| `solution-html` | `.html` 且文件名不含 `URS` | 全文内联进带缓存的 system 前缀 |
| `urs-html` | `.html` 且文件名含 `URS` | 阶段一提示词点名路径，写 URS 前强制 `read_kb_file` |
| `detail-md` | `.md` 且非 Summary/总结 | 阶段二提示词点名路径，动笔前强制读 |
| `summary-md` | `.md` 且以 Summary 开头或含「总结」 | 阶段二收尾参照 |
| `word-style` | `.docx`（跳过 `~$` 锁文件） | 转 WORD 的样式来源，模型不接触 |

同角色多份时取体积最大的那份。结果带轻量缓存，按目录签名（文件数 + 最新修改时间）失效——改模版无需重启。

### 3.3 一个刻意的取舍：不把全部样例内联进提示词

解决方案 HTML(73 KB) + 详细设计方案(58 KB) 合计十几万字。system 前缀在工具循环里每一轮都要重发，全内联会让输入成本失控。

采取的方案：

- **只内联 `solution-html`** —— 阶段一每次都要用，放进带 1h 缓存的 system 前缀最划算，跨轮跨阶段命中；
- **其余点名路径 + 强制读取** —— 写 URS / 详细设计方案 / 总结之前，提示词明确要求先 `read_kb_file <具体路径>`。读到的内容进入该阶段的消息缓存，同样只付一次。

system 前缀里新增了「提案模版目录」清单，逐条说明每份样例的角色、以及"已内联无需再读 / 必须先读 / 程序自动套用无需关心"。

### 3.4 WORD 版式对齐（两条导出路径都改）

| 路径 | 改法 |
| --- | --- |
| Pandoc（首选） | `--reference-doc` 指向模版目录的 `.docx`，取代原先自动生成的 `98-SmartLabOS-WORD参考样式.docx` |
| 内置 OpenXML（兜底） | 新增 `TryCloneStyles()`：克隆模版的 `styles.xml` 与 theme（**不带入模版正文**） |

安全兜底：模版缺 `Heading1` 样式（Word 认不出层级、目录域会空白）或读取失败（被 Word 占用等），自动退回内置样式，不中断导出。
`GET /api/dev/docx-selftest` 新增 `styleSource` 字段，回报本次实际用的模版文件。

### 3.5 一条硬约束

提示词中明确写入：**模版是版式样板，不是内容来源**。照抄结构、章节顺序、表格列设计与图示风格；样例里的设备型号、数值、客户信息一律不得进入新方案。

---

## 四、废弃 `references/04-solutions` 的引用

### 4.1 影响分析（结论：删除引用对程序运行零影响）

全代码库检索后，该目录**只有一处引用**——[`ClaudeAgentService.cs`](../src/Presales.Proposal.AI.Agent/Agent/ClaudeAgentService.cs) 中 system 提示词知识库导航的一行文本：

```csharp
sb.AppendLine($"- {refRoot}/04-solutions 解决方案范例");
```

逐项确认无其它依赖：

| 检查项 | 结论 |
| --- | --- |
| 是否有代码读取该目录 | 否 |
| 是否参与模块选型 | 否（`ModuleCatalog` 只扫 `01-modules`） |
| 是否参与产物校验 | 否（`VerifyHtmlPhase` / `VerifyDetailPhaseGrounding` 都不涉及） |
| 是否被 `PresalesPaths` 引用 | 否 |
| 是否出现在 `appsettings.json` | 否 |
| 主数据维护模块的「解决方案」实体 | 走 MySQL 表，与该目录无关 |

### 4.2 为什么只删导航行不够

删掉导航行后，模型仍可能通过 `list_kb_dir references` 发现该目录并读取。因此补了真正的拦截，在 [`FileTools.cs`](../src/Presales.Proposal.AI.Agent/Agent/FileTools.cs)：

- `list_kb_dir` 列目录时**隐去**废弃目录；
- `read_kb_file` 读取时返回**明确拒绝理由**：
  `[错误] xxx 属于已废弃资料，不作为选型与写作依据，请勿引用。`

第二点是关键——若返回含糊的"路径越界"，模型会以为自己路径写错而反复重试，白烧工具轮次与 token。

废弃清单定义在 `PresalesPaths.DeprecatedDirs`，可用配置键 `Presales:DeprecatedDirs`（分号分隔的绝对路径）覆盖，默认即 `references/04-solutions`。

---

## 五、MySQL 远程访问授权脚本

### 5.1 问题定位

现象是「访问本地 MySQL，3306 不通」。实测结论并非端口不通：

```text
127.0.0.1:3306      可连接
192.168.101.26:3306 可连接          ← 端口是通的
服务端 bind_address：*              ← 监听范围没问题
账号 'root' 当前已授权的来源主机：localhost   ← 真正的原因
服务端返回：Host '192.168.101.26' is not allowed to connect to this MySQL server
```

MySQL 8.4 只给 `root@localhost` 授权；程序按配置的 `DB_IP` 连过来，服务端看到的来源主机是 `192.168.101.26`，于是拒绝。

顺带说明：这也反证了第二节的连接串组装是正确的——请求确实握手到了 MySQL 服务端并拿到了它的业务级拒绝，而不是连接串错误。

### 5.2 交付

[`database/Modify-mySQL-Access-Right-20260811.ps1`](../database/Modify-mySQL-Access-Right-20260811.ps1)，按顺序处理三层障碍，每层先检测再按需修复：

1. **Windows 防火墙** —— 3306/TCP 入站放行（仅 Private+Domain 配置文件，不含 Public；规则已存在则跳过）
2. **MySQL `bind-address`** —— 只监听 `127.0.0.1` 时给出 `my.ini` 修改与重启服务的指引
3. **账号授权** —— `CREATE USER` + `GRANT` + `FLUSH PRIVILEGES`

用法：

```powershell
cd database

# 体检：打印将执行的每条 SQL，不做任何修改
pwsh -File .\Modify-mySQL-Access-Right-20260811.ps1 -WhatIf

# 执行（加防火墙规则需以管理员身份运行；否则加 -SkipFirewall）
pwsh -File .\Modify-mySQL-Access-Right-20260811.ps1

# 更稳妥：授权整个网段而不是单个 IP，也不要用 '%'
pwsh -File .\Modify-mySQL-Access-Right-20260811.ps1 -AllowHost '192.168.101.%'
```

设计要点：

| 要点 | 说明 |
| --- | --- |
| 参数同源 | 直接读 `env/Presales-AI-Agent-config.json`，杜绝「脚本改的账号和程序用的不是同一个」 |
| 口令不进命令行 | 通过临时 option 文件传给 `mysql.exe`（不进进程列表、不触发客户端告警），ACL 收紧到当前用户，用完即删 |
| 回显脱敏 | `IDENTIFIED BY '...'` 一律替换为 `'********'`，`-WhatIf` 输出也不例外 |
| SQL 转义 | 口令/用户名/主机的单引号加倍，库名反引号加倍 |
| 收尾验证 | 按程序那套参数（`DB_IP`/`DB_User`/`DB_Password`/`DB_Name`）实连一次并报告结果 |
| 权限提示 | `root` 会拿到全库权限以对齐 `root@localhost`；输出中提示可改用专用账号获得单库最小权限 |

### 5.3 两个踩过的坑（已在脚本中修掉）

1. **必须存为 UTF-8 BOM**。Windows PowerShell 5.1 对无 BOM 的 `.ps1` 按 ANSI 解码，中文全部乱码并直接语法错误。脚本文件已带 BOM。
2. **临时 `.cnf` 必须写成无 BOM 的 UTF-8**。`Set-Content -Encoding UTF8` 在 PS 5.1 下会写 BOM，`mysql.exe` 会把它当作 `[client]` 之前的杂字符，报 `Found option without preceding group`。脚本改用 `[System.IO.File]::WriteAllText` + `UTF8Encoding($false)`。

---

## 六、文件变更清单

### 新增

| 文件 | 说明 |
| --- | --- |
| `src/.../Configuration/AgentEnvFile.cs` | env 配置文件的定位 / 解析 / 映射为 IConfiguration 键 |
| `src/.../Agent/ProposalTemplateSet.cs` | 提案模版目录的角色归类与缓存 |
| `env/Presales-AI-Agent-config.sample.json` | 配置模板（正式文件含口令，已 gitignore） |
| `database/Modify-mySQL-Access-Right-20260811.ps1` | MySQL 远程访问授权脚本（UTF-8 BOM） |
| `Documentation/Modification-Summary-20260811-01.md` | 本文件 |

### 修改

| 文件 | 改动 |
| --- | --- |
| `Program.cs` | 首行加载 env 配置并注入最高优先级层；注册 `ProposalTemplateSet`；启动自检打印配置来源、各目录、模版归位、废弃目录、MySQL 目标 |
| `Configuration/PresalesConfig.cs` | 新增 `PlatformsDir` / `PalletDir` / `ProposalTemplateDir` / `TemplatesDir` / `KnowledgeRoots` / `DeprecatedDirs` / `IsDeprecated()` / `Display()`；三个标准/兜底文件改由 `TemplatesDir` 拼出 |
| `Agent/FileTools.cs` | 读白名单改为 `KnowledgeRoots`；废弃目录隐藏与拒读；越界提示按配置渲染 |
| `Agent/ClaudeAgentService.cs` | 注入 `ProposalTemplateSet`；提示词与工具描述的知识库路径全部改为按配置渲染；新增「提案模版目录」清单；两阶段提示词点名模版；模块数改为实扫；删除 `04-solutions` 导航行；`BuildTools()` 由静态改实例 |
| `Agent/DocxExporter.cs` | 注入 `ProposalTemplateSet`；新增 `StyleSourcePath`；Pandoc `--reference-doc` 与 OpenXML `TryCloneStyles()` 统一取模版 docx |
| `Controllers/PresalesController.cs` | 新增 `GET /api/presales/config` 自检接口；`docx-selftest` 增加 `styleSource` 字段 |
| `Data/PresalesRepository.cs`、`Data/SmartLabRepository.cs` | 连接串缺失时的异常文案改为指向 env 配置文件 |
| `appsettings.json` | 删除连接串占位符与整个 `Presales` 路径节，仅保留 `Agent` 与日志 |
| `.gitignore` | 排除 `env/Presales-AI-Agent-config.json`（含明文口令） |
| `Documentation/README.md` | 配置章节重写；新增提案模版目录、废弃目录、MySQL 授权三节；目录结构与安全说明更新 |

---

## 七、验证记录

### 构建

```text
dotnet build → 0 警告 0 错误（.NET 10 / C# 14）
```

### 启动自检（实际日志）

```text
配置文件：...\Claude-API-Key-Presales-AI-Agent\env\Presales-AI-Agent-config.json
运行目录：模块=...\references\01-modules | 平台=...\02-platforms | 托盘=...\06-pallet
        | 国标=...\potocol\MD | 输出=...\projects | 提案模版=...\09-ProposalTemplate
已废弃(隐藏且拒读)：...\references\04-solutions
提案模版 [solution-html] 1.html
提案模版 [urs-html]      微量加注-模块-URS.html
提案模版 [detail-md]     测试项目-详细设计方案.md
提案模版 [summary-md]    Summary-输出总结.md
提案模版 [word-style]    测试项目-详细设计方案.docx
MySQL 目标：root@192.168.101.26:3306/SmartLabOS-Presales-AI（口令 已配置，11 位）
Anthropic API Key：已配置（来源=环境变量 ANTHROPIC_API_KEY, 长度=108）
```

### `GET /api/presales/config`

```text
loaded: true，warnings: []
六个目录 exists 全部为 true；三份兜底模版文件 exists 全部为 true
proposalTemplates：五个角色全部归位
moduleCount: 97，agentReady: true
```

### `GET /api/dev/docx-selftest`

| 场景 | 结果 |
| --- | --- |
| 默认（本机装有 Pandoc） | `{"ok":true,"engine":"pandoc","size":12530,"styleSource":"...\\09-ProposalTemplate\\测试项目-详细设计方案.docx"}` |
| 强制走兜底（`Agent__PandocPath` 指向不存在的路径） | `{"ok":true,"engine":"openxml","size":6963,"styleSource":"...同上..."}` |

兜底路径输出体积从改造前的 1660 字节增至 6963 字节，即克隆进来的模版样式表——样式克隆确实生效。

### MySQL 脚本 `-WhatIf`

完整走通配置读取 → mysql.exe 定位 → 端口探测 → 管理员连接 → 现状查询 → SQL 预览（口令已脱敏），见第 5.1 节输出。

### 未通过项

**MySQL 连通性：失败。** 原因见第五节——服务端授权限制，非本次改造引入。执行授权脚本后重启 Agent 即应显示「MySQL 连通性：OK」。

---

## 八、遗留事项与待确认

| # | 事项 | 说明 |
| --- | --- | --- |
| 1 | **授权脚本未代为执行** | 授权 `root@192.168.101.26` 并带 `GRANT OPTION` 是对数据库安全边界的改动，留给项目方决定。执行后重启 Agent 验证。 |
| 2 | **口令文件已加入 .gitignore** | `env/Presales-AI-Agent-config.json` 含明文口令，按仓库既有的「机密绝不入仓」策略排除，并提交 `.sample.json` 模板。若希望正式文件跟随仓库，删掉 `.gitignore` 中那一行即可。 |
| 3 | `references/_templates` 已由 `Template_Path` 显式配置 | 见下方补充。 |
| 4 | 模版目录换文件后无需重启 | `ProposalTemplateSet` 按目录签名失效缓存。但 `solution-html` 内联进 system 前缀的内容由 `_templateCache` 在进程内长期持有，替换该角色文件后需重启服务才会生效。 |
| 5 | 生产部署 | Swagger 默认仅开发环境开放；建议前置反向代理与鉴权；`PickerController` 会在服务端弹出 Windows 原生对话框，仅适用于本机交互式运行。 |

---

## 九、补充：`Template_Path`（`references/_templates`）显式外置

第三节把「版式样例」外置到了 `Proposal_Template`，但 `_templates` 下还有三个文件仍靠 `Module_Path` 上推两级再拼 `_templates` 推导。审查后确认其中一份是**活引用**，不能留在隐式推导上。

### 9.1 `_templates` 下各文件的真实角色

| 文件 | 角色 | 何时用 |
| --- | --- | --- |
| `02-SmartLabOS提案标准-详细设计方案版.md` | 《详细设计方案》**章节范围标准**（★必备/○按需检查清单） | **每次生成都内联**进 system 前缀 |
| `99-SmartLabOS-技术提案输出模版.html` | 解决方案 HTML 模版 | 仅当提案模版目录无 `solution-html` |
| `98-SmartLabOS-WORD参考样式.docx` | WORD 参考样式 | 仅当提案模版目录无 `.docx`；**也是写入点**（缺失时程序往这里生成） |
| `01~05-卡片输入录入程序…xlsm` | 主数据导入的 Excel 卡片模板 | 程序不主动读，导入时在界面上选具体文件 |

### 9.2 为什么必须显式配置

`02-…详细设计方案版.md` 缺失时，`LoadDetailTemplate()` 会退回一段兜底文字继续跑——**★必备项校验事实上失效，且不报错**。当前布局下推导正确，但把路径外置到 JSON 的意义正是允许知识库挪位置；一旦挪走，这里会静默降级。这类"配置改了、程序不吭声地降级"正是本次重构要消灭的失败模式。

### 9.3 改动

- `AgentEnvFile`：新增 `Template_Path` → `Presales:TemplatesDir`；`Dir()` 增加 `optional` 参数，可选键缺失不再产生无谓警告。
- `PresalesPaths`：新增 `TemplatesDir`，上述三个文件改为由它拼出（原先直接拼 `ReferencesDir + "_templates"`）；同时纳入 `KnowledgeRoots`，目录挪到 `references/` 之外也不会被判越界。
- `Program.cs`：`DetailTemplateFile` 不存在时打**红字错误**，明确指出「★必备项校验失效，请检查配置 Template_Path」。
- `/api/presales/config`：原 `fallbackTemplates` 改名 `standardTemplates`，每项带 `inUse` 字段区分 `always` / `fallback`，避免把活引用误当兜底。
- 配置文件与 `.sample.json`、README 同步。

### 9.4 验证

```text
运行目录：… | 提案模版=…\references\09-ProposalTemplate | 标准模版=…\references\_templates
/api/presales/config → warnings: []，paths.templates.exists: true
  standardTemplates.detailStandardMd       exists=true  inUse=always
  standardTemplates.proposalHtmlFallback   exists=true  inUse=fallback
  standardTemplates.referenceDocxFallback  exists=true  inUse=fallback
```

`dotnet build` 0 警告 0 错误。

---

## 十、补充：配置文件编码与占位符检测

### 10.1 现象

用 `JSONEdit` 打开 `env/Presales-AI-Agent-config.json`，中文全部显示为乱码；用 Notepad++ 打开正常。

### 10.2 原因

该 JSON 存的是 **UTF-8 无 BOM**（首字节 `7B 0A`，即 `{` + 换行，没有 `EF BB BF`）。

- **Notepad++ / VS Code** 有 UTF-8 自动探测：扫描字节序列，符合 UTF-8 多字节规则就按 UTF-8 解码 → 正常。
- **JSONEdit** 没有这套探测，无 BOM 即按系统 ANSI 代码页解码（简体中文 Windows 为 GBK）。UTF-8 编码的中文按 GBK 解出来就是乱码。

与程序无关：C# 侧 `File.ReadAllText` 显式按 UTF-8 读，一直是对的。

### 10.3 修正

| 措施 | 说明 |
| --- | --- |
| 正式配置文件只写 ASCII | 路径、IP、账号本就无需中文；中文说明留在 `.sample.json`。**推荐做法** |
| 需要写中文时另存为 UTF-8 with BOM | 程序侧对 BOM 免疫（`File.ReadAllText` 自动识别并剥离）；PowerShell 脚本也实测通过 |
| `.sample.json` 改存 UTF-8 with BOM | 它本就承载中文说明，加 BOM 后在 JSONEdit 中显示正常 |
| 不要用 GBK 存盘 | 程序按 UTF-8 读，中文会乱码或解析失败 |

BOM 兼容性已实测：把 `PRESALES_AGENT_CONFIG` 指向带 BOM 的 `.sample.json`，`/api/presales/config` 返回 `loaded: true, error: null`，各字段解析正常；`Modify-mySQL-Access-Right-*.ps1 -ConfigPath` 指向同一文件也正常读出参数。

### 10.4 同时发现并修复的一个配置错误

排查编码时发现正式配置文件已被模板内容覆盖，且两个键的值串位：

```jsonc
"DB_Password": "在此填入 MySQL 口令",   // ← 占位符，不是真口令
"DB_Name":     "#Bqjx@2026#",           // ← 口令被填进了库名
```

这会让程序去连一个名为 `#Bqjx@2026#` 的库、且口令是占位符文本。**不会有任何语法错误**，只表现为"连不上"，而报错信息指向网络或授权，很难联想到是配置串位。

已按 `env/BK/Presales-AI-Agent-config-OLD.json` 恢复正确值（`DB_Password = #Bqjx@2026#`、`DB_Name = SmartLabOS-Presales-AI`），并补回 `Presales-AI-Agent-config.sample.json`（模板口令改为纯 ASCII 的 `PUT-MYSQL-PASSWORD-HERE`，避免再次混淆）。

### 10.5 为此增加的防呆

`AgentEnvFile` 新增占位符检测：`DB_Password` / `DB_Name` 命中 `PUT-`、`在此填入`、`请填入`、`__SET`、`YOUR-`、`<`、`xxxx` 等模板标记时，启动日志与 `/api/presales/config` 的 `warnings` 点名提示。只警告不阻断——现场也可能真用这类字面量。

实测：

```text
正式配置   → "warnings":[]
            "target":"root@192.168.101.26:3306/SmartLabOS-Presales-AI（口令 已配置，11 位）"
模板配置   → "warnings":["DB_Password 看起来仍是模板占位符（PUT-MYSQL-PASSWORD-HERE），请填入真实口令。"]
```

`dotnet build` 0 警告 0 错误。
