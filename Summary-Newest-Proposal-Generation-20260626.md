# SmartLabOS 售前方案生成 —— WORD 提案功能改造与端到端验证总结

> 日期：2026-06-26
> 解决方案：`DataMaintenance/SmartLabOS.DataMaintenance.sln`
> 范围：HTML 模版路径变更 + 新增「后台 Claude Code 生成 WORD 提案」两阶段流程

---

## 一、需求

1. **HTML 模版路径变更**
   生成 HTML 解决方案时参照的模版文档，由
   `projects\上海海关项目\SmartLabOS-技术提案输出模版.html`
   改为
   `references\_templates\99-SmartLabOS-技术提案输出模版.html`
   （内容一致，仅路径与文件名变化）。

2. **生成 WORD 提案**
   在生成解决方案 HTML 文档与 `xxxx_URS.html` 之后，依据
   `references\_templates\02-SmartLabOS提案标准-详细设计方案版.md`
   的内容提纲要求，**后台调用 Claude Code 生成 WORD 文档**。

3. **WORD 文件保存规则**
   - 目录：`projects\$ProjectName`
   - 文件名：`($ProjectName)_SmartLabOS_Presales_提案_YYYYMMDDHHMMSS.docx`
   - `$ProjectName` = 项目名称；`YYYYMMDDHHMMSS` = 年月日时分秒

4. 修改 `DataMaintenance/SmartLabOS.DataMaintenance.sln` 的源程序完成以上要求。

---

## 二、设计方案

将原「单次 headless `claude -p` 运行」改造为**两阶段流水线**（同一次生成任务内串行执行）：

```
阶段一（原逻辑）：生成解决方案 *.html  +  xxx-模块-URS.html  +  Summary-输出总结.md
        │  成功后
        ▼
阶段二（新增）  ：写「WORD生成指令-….txt」→ 再启一个 claude -p
                 依据 02-详细设计方案版.md 提纲，整合阶段一 HTML/URS + references/ 知识库，
                 用 docx 技能产出真正的 .docx，保存到指定路径/文件名
```

要点：
- 阶段二依赖阶段一产物（需读取已生成的 HTML 方案再综合成正式提案），故必须**串行、且阶段一成功后才进入阶段二**。
- WORD 文件名由 **C# 端**确定（含时间戳），再写入指令传给 Claude，便于服务端精确核对落盘结果。
- 进程启动/超时/退出码逻辑抽为可复用方法，两阶段共用；新增**瞬态错误自动重试**以提升 headless 长任务稳定性。

---

## 三、代码改动清单

### 1. 配置 — `appsettings.json`
- `TemplateFile` 改为 `…\references\_templates\99-SmartLabOS-技术提案输出模版.html`
- 新增 `DocxOutlineFile` = `…\references\_templates\02-SmartLabOS提案标准-详细设计方案版.md`
  （部署时改配置即可，无需重编译）

### 2. 路径配置 — `Presales/PresalesPaths.cs`
- 新增属性 `DocxOutlineFile`
- `TemplateFile` 默认值改为 `references\_templates\99-…html`
- `DocxOutlineFile` 默认值兜底为 `references\_templates\02-…md`

### 3. 执行器 — `Services/ClaudeCodeRunner.cs`（核心）
- `RunClaudeAsync` 重构为**两阶段编排**：
  - 阶段一：生成 HTML/URS（沿用原提示语）
  - 阶段二：`BuildDocxFileName` 生成带时间戳文件名 → `BuildWordCommandDocument` 生成指令并落盘 `WORD生成指令-….txt` → 运行第二个 claude → 校验 `.docx` 是否落盘（缺失写警告日志）
- 新增 `RunClaudeOnceAsync`：抽取「启动一次 headless claude 并等待结束」逻辑，返回退出码（启动失败/超时返回 null 并已 `FailAsync`）。
- 新增 `RunClaudeWithRetryAsync`：命中瞬态错误标记（`Stream idle timeout`/`API Error`/`overloaded`/`rate limit`/`Connection error`/`ECONNRESET`/`fetch failed`）且退出码非 0 时，自动重试该阶段（默认最多 2 次）。
- 新增 `BuildDocxFileName`：`{项目名}_SmartLabOS_Presales_提案_{yyyyMMddHHmmss}.docx`
- 新增 `BuildWordCommandDocument`：第二阶段指令文档（提纲引用、数据来源、客户需求、输出与保存要求）。
- 产出快照由「仅 *.html」推广为 ***.html + *.docx**：`SnapshotHtml/NewOrChangedHtml` → `SnapshotOutputs/NewOrChangedOutputs`（新增 `OutputPatterns`、`EnumerateOutputs`）。
- `GenJob` 新增 `DocxFile` 字段（本次生成的 WORD 文件名）。
- 新增 `SbLength`、`RecentLog` 日志辅助（线程安全取增量日志用于瞬态判定）。

### 4. 接口 — `Controllers/PresalesController.cs`
- `ListHtml` → `ListOutputs`：同时列出 `*.html` 与 `*.docx`，每项新增 `isDocx` 标志（新增 `OutputPatterns`）。
- `/file` 端点：对 `.docx` 用 Word MIME
  `application/vnd.openxmlformats-officedocument.wordprocessingml.document`
  并带文件名作附件下载；其余仍内联打开。
- `/generate/status` 响应新增 `docxFile` 字段。

### 5. 前端 — `wwwroot/js/presales.js`
- `renderFiles`：`.docx` 用 📝 图标、`download` 属性触发下载；URS 仍 📋、HTML 仍 📄。
- 生成确认弹窗文案改为说明「先 HTML/URS、再 WORD，两阶段耗时较长」。

### 6. 样式 — `wwwroot/css/style.css`
- 新增 `.ps-file-docx`（蓝色加粗）样式。

### 7. 数据库 — `database/03-presales-schema.sql`
- `output_files` 列注释补充 `.docx`（无需改表结构，docx 文件名直接进 JSON 数组）。

---

## 四、端到端验证过程

环境：Windows 11 / .NET / MySQL 8（localhost:3306）/ Claude Code CLI `2.1.170`

| 步骤 | 操作 | 结果 |
|---|---|---|
| 编译 | `dotnet build SmartLabOS.DataMaintenance.sln` | 0 警告 0 错误 |
| 起服务 | 端口 5080 被 Windows 排除段(5041–5140)占用，改用 **8088** | 正常监听 |
| 选项/CRUD | `GET /options`、`POST /projects` 等 | 正常（中文体经 UTF-8 文件提交） |
| 建项目 | `端到端验证-20260626`（id=7） | 成功，目录创建 |
| 填需求 | 1 个流程标准 `04-GB31658.17-2021.md` + 现状/挑战/期望/范围等 | 成功 |
| 指令预览 | `GET /command-preview` | 模版路径已是 `references\_templates\99-…html` |
| 触发生成 | `POST /generate` | 202，进入 running |
| 阶段一 | 轮询状态 + 观察项目目录 | 产出 `1.html`(55KB) + `50ml高速冷冻离心模块-URS.html` + `Summary-输出总结.md` |
| 阶段二启动 | 目录出现 `WORD生成指令-20260626-165404.txt` | ✅ 确认进入阶段二 |
| 阶段二（整跑） | headless claude 生成 docx | ❌ `API Error: Stream idle timeout`，退出码 1 |
| 加固 | 新增瞬态错误自动重试，重新编译 | 0 警告 0 错误 |
| 阶段二（隔离重跑） | 以同一提示语单独跑 `claude -p` 生成 docx | ✅ 退出码 0，`.docx` 落盘 |
| 独立校验 | Python `zipfile` 校验 OOXML 结构 | ✅ 合法 |

---

## 五、验证结果

### 5.1 四项需求验收

| 需求 | 状态 | 证据 |
|---|---|---|
| 1. HTML 模版改为 `99-…html` | ✅ | 指令预览第 5.1 行指向新路径；阶段一实际据此生成 |
| 2. HTML/URS 后按 02-MD 提纲生成 WORD | ✅ | `WORD生成指令-….txt` 引用 `02-SmartLabOS提案标准-详细设计方案版.md`；docx 八章全覆盖 |
| 3. 保存路径 + 文件名格式 | ✅ | 落盘 `端到端验证-20260626_SmartLabOS_Presales_提案_20260626165404.docx`，字节级匹配 |
| 4. 改动 `.sln` 源程序 | ✅ | 编译 0 警告 0 错误 |

### 5.2 WORD 文档独立校验（不依赖 Claude 自述）

```
文件：端到端验证-20260626_SmartLabOS_Presales_提案_20260626165404.docx
大小：40,212 字节
is_zipfile        : True
zip 完整性(None=OK): None
部件数            : 26（含 [Content_Types].xml / word/document.xml / _rels/.rels）
word/document.xml : 617,727 字节
表格数(w:tbl)     : 56
```

文档结构：封面 + 自动目录(TOC 1–3 级) + 页眉(标题+蓝色分隔线) + 页脚(第 X 页/共 Y 页)；
按 02-MD 八章 + 文档元信息全覆盖，技术指标含数值/单位/精度/通讯协议(MODBUS TCP)，
数据溯源自 `1.html`/URS/`references/`，价格统一标注「请联系销售团队获取报价」。

> 注：Claude 实际采用 **docx-js**（保留生成脚本 `build_docx.js`）而非 python-docx，
> 产物为标准 `.docx`，需求满足。本机未装 LibreOffice，未做 PDF 可视化预览，已用结构化校验替代。

### 5.3 如实说明（caveat）

1. **整跑时阶段二曾失败**：原因是 Claude CLI 层 `Stream idle timeout`（长耗时 docx 生成期间无 token 输出触发空闲超时），**非集成代码缺陷**；隔离重跑同一提示语一次成功，证明 docx 生成链路本身可用。
2. **瞬态重试为防御性兜底**：已编译通过，但本次隔离重跑首次即成功、未触发超时，**该重试路径尚未在真实超时下被实测命中**。

---

## 六、运行与复测要点

起服务（5080 不可用，用 8088）：
```bash
cd "DataMaintenance/src/SmartLabOS.DataMaintenance.Api"
ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=http://127.0.0.1:8088 dotnet run --no-launch-profile
```
前端：`http://127.0.0.1:8088`

复测验收三处：
1. 指令预览中模版指向 `99-…html`；
2. 项目目录出现 `WORD生成指令-….txt`（已进入阶段二）；
3. 最终 `{项目名}_SmartLabOS_Presales_提案_YYYYMMDDHHMMSS.docx` 落盘，前端文件区显示 📝 可下载。

整跑约 30–40 分钟（阶段一 ~15 min + 阶段二 ~15–25 min），前端 3 秒轮询。
阶段二若再遇空闲超时，日志会出现 `[重试] WORD 提案：…`。

---

## 七、改动文件一览

```
DataMaintenance/src/SmartLabOS.DataMaintenance.Api/appsettings.json
DataMaintenance/src/SmartLabOS.DataMaintenance.Api/Presales/PresalesPaths.cs
DataMaintenance/src/SmartLabOS.DataMaintenance.Api/Services/ClaudeCodeRunner.cs
DataMaintenance/src/SmartLabOS.DataMaintenance.Api/Controllers/PresalesController.cs
DataMaintenance/src/SmartLabOS.DataMaintenance.Api/wwwroot/js/presales.js
DataMaintenance/src/SmartLabOS.DataMaintenance.Api/wwwroot/css/style.css
DataMaintenance/database/03-presales-schema.sql
```

> 改动尚未提交（git 仍为修改状态）。需要提交时另行告知。

## 八、测试残留（按用户要求保留，未清理）

- 数据库项目记录：`端到端验证-20260626`（id=7，状态 failed —— 因整跑阶段二超时所致）
- 目录 `projects\端到端验证-20260626\`：
  `1.html`、`50ml高速冷冻离心模块-URS.html`、`Summary-输出总结.md`、
  `生成指令-20260626-163901.txt`、`WORD生成指令-20260626-165404.txt`、
  `build_docx.js`、`端到端验证-20260626_SmartLabOS_Presales_提案_20260626165404.docx`
