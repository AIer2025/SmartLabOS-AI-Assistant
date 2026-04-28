# SmartLabOS AI 提案助手 — 设计方法、工作原理与演进路线总结

> 编制日期：2026-04-24
> 范围：基于 [C:\TestClaude\SmartLabOS-AI-Assistant](.) 当前工程实现的完整回顾
> 读者：项目负责人、售前团队、工程维护人员

---

## 一、项目定位

**SmartLabOS AI 提案助手**是百泉聚兴（北京）科技有限公司的售前技术方案自动生成工具，运行于 **Claude Code + Claude Max** 之上，面向智慧实验室前处理系统的售前工程师，辅助把客户需求快速转化为符合公司规范与国标要求的专业技术方案 Markdown 文档。

核心诉求是**"把隐性经验变成可复用的 Skill 工作流"**：让售前工程师不再需要从零翻模板、查国标、凑设备表，而由 Agent 按固定流程引导、读知识库、套模板、做校对。

---

## 二、设计方法论

### 2.1 架构选型回顾

参照 [SetupTools/ClaudeCode-AI助手演进路线总结.md](SetupTools/ClaudeCode-AI助手演进路线总结.md)，曾评估过三种方案：

| 方案 | 工程量 | 特点 |
|------|--------|------|
| A. Claude Projects + 结构化知识库 | 0 代码，1–2 天 | 人工驱动，手贴需求 |
| **B. Claude Code + Skills + 本地知识库** | **中等，1–2 周** | **当前落地方案** |
| C. 独立 Web 平台 + Claude API + RAG | 重，1–2 月 | 多人协作、可沉淀版本 |

**当前落地采用的是方案 B 的轻量变体**：不依赖 MCP Server，而是用 Claude Code 原生的 Skill 机制 + 文件系统作为状态存储，把"数据库/检索"的职责简化为"按约定读 references/ 目录下的 Markdown"。

### 2.2 核心设计原则（来自 [CLAUDE.md](CLAUDE.md)）

1. **严格按阶段推进** — 五阶段不可跳过，每阶段强制校验前置
2. **依赖知识库，绝不编造** — 所有技术参数从 `references/` 读取，缺失即明示停止
3. **状态可追踪** — 每步都要更新 `workflow-status.md`
4. **聚焦业务** — 只处理 SmartLabOS 相关方案，其他请求直接拒绝
5. **价格红线** — 涉及报价统一回复"请联系销售团队"
6. **中文交互、专业简洁、分步引导**（每轮最多 2–3 个问题）

### 2.3 身份与行为注入机制

通过 `CLAUDE.md`（会话启动自动加载到系统提示）合并了 OpenClaw 遗留的 `AGENTS.md`（身份与工作流）与 `SOUL.md`（行为准则），形成**持久化的"人设 + 规则"**。这是整个系统的"宪法"，优先级高于一切默认行为。

---

## 三、工作原理

### 3.1 总体架构

```
┌─────────────────────────────────────────────────────────┐
│  用户（售前工程师） ──────对话──────▶ Claude Code       │
└────────────────────────────┬────────────────────────────┘
                             │ 读取
                             ▼
           ┌──────────────────────────────────┐
           │  CLAUDE.md（身份 + 规则注入）     │
           └──────────────────────────────────┘
                             │ 触发
                             ▼
┌─────────────────────────────────────────────────────────┐
│            .claude/skills/  （五个 Skill）                │
│  proposal-workflow ─┬─ proposal-collector               │
│                     ├─ proposal-analyzer                │
│                     ├─ proposal-generator               │
│                     └─ proposal-reviewer                │
└─────────────────────────┬───────────────────────────────┘
                          │ 读  ↑ 写
                          ▼
        ┌─────────────────────────────────────┐
        │  references/   （知识库，只读）      │
        │  ├─ product-specs.md                │
        │  ├─ process-quechers.md             │
        │  ├─ process-inorganic.md            │
        │  ├─ unit-catalog.md                 │
        │  ├─ standards-library.md            │
        │  └─ proposal-template.md            │
        └─────────────────────────────────────┘
                          │ 写
                          ▼
        ┌─────────────────────────────────────┐
        │  projects/<项目名>/   （每项目产出）  │
        │  ├─ workflow-status.md              │
        │  ├─ collected-info.md               │
        │  ├─ analysis-report.md              │
        │  ├─ technical-proposal-draft.md     │
        │  ├─ review-report.md                │
        │  ├─ technical-proposal.md           │
        │  └─ project-summary.md              │
        └─────────────────────────────────────┘
```

### 3.2 五阶段工作流

由 [proposal-workflow Skill](.claude/skills/proposal-workflow/SKILL.md) 作为主控编排器。

| 阶段 | 触发口令 | 负责 Skill | 产出文件 | 完成标志 |
|------|---------|-----------|---------|---------|
| 1. 创建提案项目 | "新建提案 {名称}" | proposal-workflow | `workflow-status.md`、空 `collected-info.md` | 目录与状态文件创建 |
| 2. 提案信息收集 | "开始采集" | [proposal-collector](.claude/skills/proposal-collector/SKILL.md) | `collected-info.md` | A/B/D 三类至少完成 |
| 3. 提案初稿生成 | "生成初稿" | [proposal-analyzer](.claude/skills/proposal-analyzer/SKILL.md) + [proposal-generator](.claude/skills/proposal-generator/SKILL.md) | `analysis-report.md`、`technical-proposal-draft.md` | 初稿生成 |
| 4. 审核及修改 | "审核方案" | [proposal-reviewer](.claude/skills/proposal-reviewer/SKILL.md) | `review-report.md`、（必要时）`*-v2.md` | 全部 ✅ 或用户确认 |
| 5. 正式发布 | "发布方案" | proposal-workflow | `technical-proposal.md`、`project-summary.md` | 正式版本产出 |

### 3.3 信息采集的六大类（阶段 2）

由 proposal-collector 分轮引导：

- **A. 业务需求与痛点**：检测领域、客户类型、现流程、痛点
- **B. 技术规范要求**：国标编号、检测方法、分析物清单、基质类型
- **C. 行业规范要求**：ISO/IEC 17025、GMP/GLP、FDA 21 CFR Part 11、LIMS 对接
- **D. 处理效率要求**：日送检量、单批次量、工时、后端仪器配置
- **E. 机位/检测线布局**：面积层高、布局、动线
- **F. 基础设施条件**：供电、供水、楼层承重、环境、通道

### 3.4 需求分析的六项内容（阶段 3 前半）

由 proposal-analyzer 基于采集结果 + 知识库执行：

1. **检测领域识别** — 匹配主工艺路线（QuEChERS / EMR-Lipid / 酸消解 + ICP）
2. **国标匹配** — 确定试样量、溶剂、盐包、离心、SPE、浓缩参数
3. **通量计算** — 批次数 = 日检量 ÷ 单批次，核对后端仪器供给
4. **设备选型** — 列功能单元清单和关键参数
5. **基础设施校验** — 功率/尺寸/承重/水电核对
6. **风险与缺项识别** — ❌ / ⚠️ / 💡 三级标注

### 3.5 方案审查的五个维度（阶段 4）

由 proposal-reviewer 对初稿执行：

1. **完整性检查**：7 个章节是否齐全
2. **参数一致性**：与 `product-specs.md` 和 `unit-catalog.md` 对照
3. **国标合规性**：与 `standards-library.md` 对照
4. **通量合理性**：计算数字自洽性
5. **客户需求匹配**：与 `collected-info.md` 对照

每项给出 ✅ / ⚠️ / ❌ 判定，有 ❌ 则自动修正生成 v2，有 ⚠️ 则列出待人工确认清单。

### 3.6 知识库结构

`references/` 目录承载所有技术参数，**是整个 Agent "不编造" 原则的物理依托**：

- [product-specs.md](references/product-specs.md) — SmartLabOS 系统参数（微服务、.NET 8、MySQL/达梦、麒麟）
- [process-quechers.md](references/process-quechers.md) — QuEChERS 农残/兽残流程（GB23200.113/121）
- [process-inorganic.md](references/process-inorganic.md) — 无机元素前处理流程
- [unit-catalog.md](references/unit-catalog.md) — 功能单元目录（加液、加盐、振荡、离心、SPE、浓缩等）
- [standards-library.md](references/standards-library.md) — 国标库
- [proposal-template.md](references/proposal-template.md) — 方案文档模板（7 章结构）
- [_sources/](references/_sources/) — 原始 PDF（3 份官方技术文件）
- [_gaps.md](references/_gaps.md)、[_gaps_01.md](references/_gaps_01.md) — 知识库缺口盘点

### 3.7 状态追踪机制

每个项目目录下都有一份 `workflow-status.md`，用 5 行表格记录 5 个阶段的状态与时间戳，外加操作日志。用户随时可说"查看进度"或"回到采集阶段"，系统据此重置后续阶段状态并允许重新执行。

---

## 四、当前已实现的能力

1. ✅ Skill 机制 + CLAUDE.md 身份注入完整运行
2. ✅ 五阶段工作流骨架全部落地（5 个 Skill SKILL.md 齐备）
3. ✅ 知识库目录结构就位，3 份权威 PDF 已归档
4. ✅ 每项目独立目录、产出文件命名约定明确
5. ✅ 价格红线、编造参数红线等关键护栏通过 SOUL 级规则注入
6. ✅ 中文交互、分步引导、状态查询/回退机制齐全

---

## 五、需要改进的地方（基于现状盘点）

### 5.1 知识库（references/）严重不足 — 最紧迫

参照 [references/_gaps.md](references/_gaps.md) 的盘点结论，**当前知识库的完整度不足以支撑除"植物源农残"外的其他主流场景**。

**P0 级缺口**（不补则多数场景无法生成方案）：

1. `standards-library.md` 仅 GB23200.113/121 有内容，其余 5 条 + 核心限量标准全部 TODO，**GB 2763、SN/T 系列、GB 31658/31660、GB 5009 系列、HJ 803 完全缺失**
2. `process-quechers.md` 仅覆盖植物源，**动物源（肉/蛋/奶/水产）、茶叶、香辛料、液体基质均无流程**
3. `unit-catalog.md` **缺均质/研磨/冻干前端单元**，动物组织前处理链条无法落笔
4. `process-inorganic.md` **无分步骤工艺流程**，粒度远低于 quechers，且未引用任何国标

**P1 级缺口**：升温曲线、ICP 接样衔接、CIQ 报告模板、等保 2.0、通量计算公式模板、样例段落。

**典型卡点**：海关农残场景（植物源 + 动物源双基质）在**阶段 3 的"工艺流程"和"标准引用"硬卡**，阶段 4 审核会二次触发阻塞。

### 5.2 Skill 设计层面的改进点

1. **proposal-collector 没有"适用标准选型辅助"子问题** — 用户需要自己知道选 GB 还是 SN/T
2. **proposal-analyzer 的通量计算未标准化** — 公式模板未沉淀到 `proposal-template.md`
3. **proposal-reviewer 没有"与历史方案对标"环节** — `samples/` 目录虽存在但未被任何 Skill 利用
4. **缺少"阶段 3 预检 Skill"** — 目前只能进到初稿生成阶段才发现知识库缺失，应前置到阶段 2 结束时就做一次知识库可用性检查
5. **多方案对比缺失** — 一个项目只能出一份方案，无法并行生成 A/B 方案供客户选择

### 5.3 工程与协作层面

1. **无版本管理** — `projects/` 下文件靠命名（draft、v2）区分版本，不走 git，回溯困难（项目根目录 `Is a git repository: false`）
2. **无多人协作机制** — Claude Code 本地运行，团队成员各自一份副本，知识库无同步
3. **无自动化导出** — 最终仍是 Markdown，客户交付通常需要 Word/PDF，手动转换
4. **无度量** — 没有指标跟踪 Skill 的命中率、平均生成时间、审核一次通过率
5. **FromUbuntu 遗留目录未清理** — README 已说"safe to delete"但仍在根目录
6. **大连项目目录不在 projects/ 下** — 命名与结构约定有分歧，需统一

### 5.4 模型行为层面

1. **"价格问题拦截"未经系统验证** — 依赖 prompt 约束，若客户变体提问（例如"这套系统预算量级"）未必拦截
2. **国标引用 hallucination 风险** — 即便说了"不编造"，缺资料时模型是否 100% 停止仍需端到端验证
3. **通量计算缺单元测试** — 数字自洽性靠 reviewer 自己校对，没有独立验证

---

## 六、产品演进路线

### 6.1 短期（1–2 周）— 补齐落地可用性

**核心目标**：让"海关农残"、"动物源兽残"、"无机元素"三个典型场景都能端到端跑通。

1. **P0 知识库回填**
   - 从 `references/_sources/食品安全农兽残检测样品前处理系统技术方案.pdf` 抽动物源、茶叶、香辛料流程写入 `process-quechers.md`
   - 从公开国标文本补 `standards-library.md` 的 GB 2763、SN/T 0127、SN/T 1982、GB 31658.17、GB/T 19648
   - 从 `_sources/无机元素测试...pdf` 补 `process-inorganic.md` 的分步骤流程
   - `unit-catalog.md` 补均质/研磨/冻干前端单元
2. **端到端演练**：用"北京海关农残检测前处理系统"做一次完整五阶段跑通，记录每个阶段实际卡点
3. **新增"知识库自检 Skill"**（proposal-precheck）：在阶段 2 结束时预先匹配所需知识库片段，提前发现缺口
4. **清理 FromUbuntu、统一大连项目到 `projects/` 下**

### 6.2 中期（1–2 月）— 专业度与协作

1. **模板演进**
   - 在 `proposal-template.md` 中沉淀通量计算公式模板、字数/页数建议、图表编号规范
   - 增加 1–2 份完整的"金标准"样例方案作为 few-shot
2. **多方案并行能力**
   - 允许在同一项目下生成 A（性价比）/B（标准）/C（高端）三个配置版本
   - `proposal-analyzer` 输出选型决策树
3. **Samples 利用**
   - `proposal-reviewer` 新增"与历史方案对标"维度，从 `samples/` 提取可复用段落/参数
4. **导出工具链**
   - 引入 Pandoc 或 Python-docx 把 Markdown 方案转 Word/PDF（保留公司版式、页眉页脚、封面）
   - 对应到方案 A 路线图的"C# 层做文档导出"定位
5. **版本控制**
   - 把 `projects/` 纳入 git（至少各项目目录独立 git init），或把方案产出提交到企业 Git/OneDrive
6. **度量埋点**
   - 记录每次五阶段耗时、各阶段修改次数，找出系统性瓶颈

### 6.3 长期（3–6 月）— 从"助手"到"平台"

对应历史评估中的**方案 C**：独立 Web 平台 + Claude API + RAG。

1. **数据化改造**
   - 把 `references/` 中结构化的部分（设备单元参数、国标限量值）迁到数据库，非结构化部分走向量检索
   - 对应评估中"方案 B"的 MCP Server 思路：让 Claude 能"算"（功率合计、尺寸校验）而不只是"查"
2. **独立 Web 平台**
   - 前端：售前填表单 → 自动触发五阶段流水线
   - 后端：C# ASP.NET Core 或 Python FastAPI，承担文档导出、CRM 对接、权限管控
   - 向量库：Qdrant / PGVector
3. **SmartLabOS 业务系统集成**
   - 方案中引用的设备清单直接对接公司物料主数据（SKU、库存、价格由销售系统提供）
   - LIMS/MES 对接需求自动生成配置清单
4. **多租户与审批流**
   - 售前、销售、技术总监、法务逐级审核
   - 每版方案可追溯、可对比
5. **智能营销闭环**
   - 方案发出后，跟踪客户反馈（签约 / 延期 / 丢单），把结果回写到样例库，持续优化 prompt 与选型逻辑

### 6.4 风险与抉择点

| 抉择点 | 选项 | 建议 |
|-------|------|------|
| 继续留在 Claude Code 还是上 Web 平台 | 两条路径 | 先把知识库补完 + 样本跑到 10 份以上再决策，过早平台化会重写 |
| 是否引入 MCP Server | 是 / 否 | 当硬件 SKU 频繁变更 或 审核阶段发现大量参数校验逻辑时，引入；目前仍可延后 |
| Markdown 还是 Word 作为内部正式版 | 二选一 | 内部保持 Markdown（diff 友好），对外交付才转 Word |
| 是否接入 Claude API（脱离 Max 限流） | 是 / 否 | 单人使用维持 Max；跨团队或做 CI 级批量时再接 API |

---

## 七、快速行动清单（给维护者）

立刻可做的**高 ROI**动作（一周内）：

- [ ] 按 [_gaps.md](references/_gaps.md) P0 清单，先补 `standards-library.md`（不依赖公司 PDF）
- [ ] 从 `_sources/` PDF 提取动物源 QuEChERS 流程写入 `process-quechers.md`
- [ ] 用"北京海关农残检测前处理系统"做端到端演练，记录卡点
- [ ] 清理根目录 `FromUbuntu/`、把"大连项目"迁入 `projects/`
- [ ] 在 `proposal-workflow` SKILL.md 中增加"阶段 2→3 过渡前的知识库可用性预检"条款

---

*文档编制：2026-04-24，基于当前工程快照。下次重大迭代后请更新本文件。*
