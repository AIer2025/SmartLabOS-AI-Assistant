# SmartLabOS AI Assistant

> 百泉聚兴（北京）科技有限公司 · SmartLabOS 售前技术方案智能助手  
> 基于 Claude Code + Claude Max 的五阶段提案生成 Agent

## 项目简介

本仓库实现了一个面向**实验室全流程自动化样品前处理系统**的售前技术方案生成 Agent。
项目负责人通过自然语言对话，由 AI 助手按"五阶段工作流"逐步引导，完成从客户需求采集到正式技术方案输出的全过程。

所有技术参数、工艺流程、设备规格均来自 [references/](references/) 知识库，**严禁编造数据**；
工艺流程严格遵循对应国标（GB23200.113-2018、GB23200.121-2021、GB31658.26-2025 等）。

## 五阶段工作流

```text
阶段 1 创建提案项目  →  用户说"新建提案 {项目名称}"
阶段 2 提案信息收集  →  用户说"开始采集"
阶段 3 提案初稿生成  →  用户说"生成初稿"
阶段 4 审核及修改    →  用户说"审核方案"
阶段 5 正式发布      →  用户说"发布方案"

随时查看进度        →  用户说"查看进度"或"提案状态"
```

每个阶段对应一个独立的 Skill，由 `proposal-workflow` 主控编排器协调，状态写入项目目录下的 `workflow-status.md`。

## 快速开始

1. 在 VS Code 中打开 `C:\TestClaude\SmartLabOS-AI-Assistant`
2. 启动 Claude Code（终端运行 `claude`，或使用 VS Code 扩展）
3. 验证知识库：让 Claude 列出 [references/](references/) 下所有文件并简要说明
4. 创建提案：输入"新建提案 {客户名称}"，按提示逐步推进五个阶段
5. 输出位置：`projects/<客户或项目名称>/`

## 目录结构

| 路径 | 说明 |
|------|------|
| [CLAUDE.md](CLAUDE.md) | Agent 角色、规则与约束（每次会话自动加载到系统提示词） |
| [.claude/skills/](.claude/skills/) | 五阶段工作流的 Skill 定义 |
| [references/](references/) | 知识库：国标、设备参数、工艺流程、卡片库（PLT/MOD/SOL）、模板、词表 |
| [tools/](tools/) | 卡片库自动化脚本（validate / build_indexes / recompute） |
| [scripts/](scripts/) | 钩子安装与一键体检脚本（install-hooks / check-all） |
| [git-hooks/](git-hooks/) | 受版本控制的 pre-commit 钩子源（需通过 scripts 安装到 .git/hooks/） |
| [.github/workflows/](.github/workflows/) | GitHub Actions 远程兜底：方案/索引/资源汇总三项 CI 校验 |
| [projects/](projects/) | 每个客户/项目一个目录，存放六类工作产物 |
| [samples/](samples/) | 历史项目参考样例（如北京海关农残检测前处理系统） |
| [SetupTools/](SetupTools/) | 安装工具与演进路线说明 |
| [设计资料/](设计资料/) | SmartLabOS 设计方法、知识卡片体系、自动化资料（原始设计文档） |
| [大连项目/](大连项目/) | 大连中药经典名方质量分析自动化项目原始资料 |
| [食检院项目/](食检院项目/) | 成都食检院项目国标 PDF 与设计稿 |
| [FromUbuntu/](FromUbuntu/) | 原 Ubuntu OpenClaw 环境导出归档（迁移完成后可删除） |

## Skills 一览

位于 [.claude/skills/](.claude/skills/)，由 Claude Code 自动加载：

| Skill | 触发关键词 | 职责 |
|-------|-----------|------|
| `proposal-workflow` | 新建提案 / 提案流程 | 主控编排器，管理五个阶段的状态机 |
| `proposal-collector` | 开始采集 / 信息收集 | 引导用户逐步采集六大类业务/技术信息 |
| `proposal-analyzer` | 分析需求 / 开始分析 | 匹配工艺流程、设备选型、系统配置 |
| `proposal-generator` | 生成方案 / 出方案 | 按模板生成完整的技术方案 Markdown |
| `proposal-reviewer` | 审查方案 / 检查方案 | 五维度审查：完整性、参数一致性、国标合规、通量合理性、需求匹配 |

## 知识库（references/）

### 当前已就位的工艺与模板文件

| 文件 | 内容 |
|------|------|
| [product-specs.md](references/product-specs.md) | SmartLabOS 系统参数与产品规格 |
| [process-quechers.md](references/process-quechers.md) | QuEChERS 农残前处理工艺流程 |
| [process-inorganic.md](references/process-inorganic.md) | 无机元素前处理工艺流程 |
| [unit-catalog.md](references/unit-catalog.md) | 硬件功能单元目录（农残/无机两套） |
| [standards-library.md](references/standards-library.md) | 国标/行标关键要求摘要 |
| [proposal-template.md](references/proposal-template.md) | 技术方案文档模板规范 |
| [_sources/](references/_sources/) | 国标 PDF 原文与历史方案 PDF 资料 |
| [_gaps.md](references/_gaps.md) / [_gaps_01.md](references/_gaps_01.md) | 知识库缺口清单 |

### 已落地的卡片库（seed 状态）

| 路径 | 内容 | 卡片数 |
|------|------|--------|
| [references/_templates/](references/_templates/) | 三类卡片的空白模板（建卡时复制） | 3 |
| [references/_vocabulary/](references/_vocabulary/) | 受控词表：functions / module-categories / sample-types / industries | 4 |
| [references/platforms/](references/platforms/) | 平台卡片库（含 `_index.md`） | 1 (PLT-1400) |
| [references/modules/](references/modules/) | 模块卡片库（含 `_index.md` 与 `_categories.md`） | 6 (称量/加液振荡/移液/SPE/浓缩/离心 各 1) |
| [references/solutions/](references/solutions/) | 工艺方案卡片库（含 `_index.md`） | 1 (SOL-QUECHERS-MID) |

> **种子数据来源**：`references/platforms|modules|solutions/` 下的卡片复制自 [设计资料/03-SmartLabOS-Automation/sample_kb/](设计资料/03-SmartLabOS-Automation/sample_kb/)，是工具脚本配套的迷你示例库。可直接基于这些卡片改写为公司真实数据，或先跑一次 `python tools/validate_solution.py references/solutions/SOL-QUECHERS-MID.md` 确认环境就绪。

### 知识卡片体系（Knowledge Card System）

> 完整设计与样板：[设计资料/01-SmartLabOS-Card-Templates(知识卡片体系)/](设计资料/01-SmartLabOS-Card-Templates(知识卡片体系)/) · 工具脚本：[设计资料/02-SmartLabOS-Card-Tools/](设计资料/02-SmartLabOS-Card-Tools/) · 自动化与 CI：[设计资料/03-SmartLabOS-Automation/](设计资料/03-SmartLabOS-Automation/)

#### 1. 三类卡片的定位

知识库以 **YAML frontmatter + Markdown** 为统一格式，按抽象层级分为三类卡片：

| 卡片类型 | ID 前缀 | 回答的问题 | 类比 | 模板 |
|---------|--------|-----------|------|------|
| 平台卡片（Platform） | `PLT-` | 我有什么硬件底盘？ | 汽车的"车型平台" | [platform-card-template.md](references/_templates/platform-card-template.md) |
| 模块卡片（Module） | `MOD-` | 我能做什么动作？ | 汽车的"零部件" | [module-card-template.md](references/_templates/module-card-template.md) |
| 工艺方案卡片（Solution） | `SOL-` | 针对某类需求推荐什么组合？ | 汽车的"配置版本" | [solution-card-template.md](references/_templates/solution-card-template.md) |

平台/模块为**物理资产**，方案为**组合资产**。客户来了，先在 SOL 库找最接近的方案微调，找不到再从 PLT + MOD 重新组合，并把结果沉淀回 SOL 库——库越用越值钱。

命名规则：`PLT-1400`（前缀 + 型号）、`MOD-SPE-12`（前缀 + 类别缩写 + 通道数）、`SOL-QUECHERS-MID`（前缀 + 工艺缩写 + 通量级别），文件名直接使用 ID。

#### 2. 卡片间的引用与对齐规则

```text
                    ┌─────────────────────┐
                    │  工艺方案卡片 (SOL)  │
                    │  - 行业/样品/通量    │
                    │  - 工艺流程         │
                    └──────┬──────┬───────┘
                           │      │
                  引用 1:1 │      │ 引用 1:N
                           ▼      ▼
              ┌──────────────┐  ┌──────────────┐
              │ 平台卡片 PLT │◄─┤ 模块卡片 MOD │
              │  - 物理底盘  │  │  - 兼容平台  │
              └──────────────┘  └──────────────┘
                       ▲                ▲
                       └─ 容量/功率累加 ─┘
                       (模块占位累加 ≤ 平台上限)
```

字段级对齐规则（自动校验依据）：

- `solution.platform.id` 必须存在于 PLT 库
- `solution.modules[].id` 必须存在于 MOD 库
- 每个被选用模块的 `compatible_platforms` 必须包含选中的平台 ID
- `Σ module_slots ≤ platform.max_modules` / `Σ tray_slots ≤ platform.max_trays` / `Σ power_kva_peak ≤ platform.power_kva_peak`
- `workflow[].step` 用到的模块的 `function` 必须落在 `platform.supported_functions` 词表内
- 加热模块累计数 > `platform.constraints.max_heating_modules` → `ventilation_required: true`
- 任一模块 `requires_compressed_air: true` → `solution.resource_summary.compressed_air_required: true`

#### 3. references/ 目标目录规划

```text
references/
├── platforms/                # 平台卡片库（公司 3 个型号：800/1200/1400）
│   ├── _index.md             # 自动生成的平台索引
│   ├── PLT-800.md
│   ├── PLT-1200.md
│   └── PLT-1400.md
│
├── modules/                  # 模块卡片库（150+，按类别分子目录）
│   ├── _index.md             # 自动生成的模块索引（按 category 分组）
│   ├── _categories.md        # 模块类别枚举词表
│   ├── weighing/             # 称量类
│   ├── dispensing/           # 加液振荡类
│   ├── centrifuge/           # 离心类
│   ├── pipetting/            # 移液类
│   ├── spe/                  # SPE 净化类
│   ├── concentration/        # 浓缩类
│   └── ...
│
├── solutions/                # 工艺方案卡片库
│   ├── _index.md             # 自动生成的方案索引（按行业反查）
│   ├── SOL-QUECHERS-MID.md
│   ├── SOL-QUECHERS-HIGH.md
│   └── SOL-DIGEST-METAL.md
│
├── _templates/               # 三类卡片模板（建卡时复制）
│   ├── platform-card-template.md
│   ├── module-card-template.md
│   └── solution-card-template.md
│
├── _vocabulary/              # 受控词表（防止同义异写）
│   ├── functions.md          # 工艺功能词表
│   ├── module-categories.md  # 模块类别词表
│   ├── sample-types.md       # 样品类型词表
│   └── industries.md         # 行业词表
│
├── standards-library.md      # 国标库（已就位）
├── proposal-template.md      # 文档模板（已就位）
├── process-*.md              # 工艺流程库（已就位）
└── _sources/                 # 原始 PDF 资料（已就位）
```

> **当前状态**：上述目录骨架与 seed 卡片已落地（详见上节"已落地的卡片库"）。`tools/` `scripts/` `git-hooks/` `.github/workflows/` 也已部署完毕。详细 7 天上手计划见 [设计资料/01-SmartLabOS-Card-Templates(知识卡片体系)/SmartLabOS-卡片体系使用说明.md](设计资料/01-SmartLabOS-Card-Templates(知识卡片体系)/SmartLabOS-卡片体系使用说明.md) 第五节。

#### 4. 如何生成卡片

##### 建卡顺序：自下而上

1. **平台卡片 (P0)** — 公司只有 3 个型号，先一周内全部建完
2. **高频模块卡片 (P0)** — 优先填充 20–30 个最常用模块，剩余模块用模板批量补 ID/名称/类别三个字段，参数后续逐步补全
3. **方案卡片 (P1)** — 只有平台和模块都建好后才写；建议每跑完一个真实提案就沉淀一份方案卡片，起步覆盖 3–5 份主流场景

##### 单卡片填写流程

```bash
# 1. 从模板目录复制
copy references\_templates\solution-card-template.md ^
     references\solutions\SOL-NEW-CASE.md

# 2. 填写 frontmatter（重点：platform.id, modules[], workflow），正文按需扩展
# 3. resource_summary 字段可留空，由脚本自动算

# 4. 自动汇总资源占用
python tools\recompute_resource_summary.py references\solutions\SOL-NEW-CASE.md --write

# 5. 校验合法性
python tools\validate_solution.py references\solutions\SOL-NEW-CASE.md

# 6. 重建索引
python tools\build_indexes.py
```

#### 5. 如何校验卡片

校验由三层防线组成（脚本已部署在项目根 [tools/](tools/)，启用前请先安装 Python 3.9+ 与 `pyyaml`，详见下方"环境与一次性部署"）：

##### 第 1 层：脚本即时校验

| 脚本 | 职责 | 退出码语义 |
|------|------|-----------|
| `tools/validate_solution.py` | 11 类规则的方案合法性校验：平台/模块存在性、兼容性、模块位/托盘位/功率容量、通风/水/气一致性、`workflow` 引用、`resource_summary` 自洽 | `0`=PASS / `1`=FAIL / `2`=脚本错 |
| `tools/recompute_resource_summary.py` | 自动汇总 `total_modules_used` / `total_trays_used` / `total_power_kva_peak` / 通风/气/水标志，回写 frontmatter（默认 dry-run，加 `--write` 实写）| 同上 |
| `tools/build_indexes.py` | 扫描三个卡片目录，自动生成各 `_index.md`（按类别分组、按行业反查），跳过 `_` 前缀文件 | 同上 |

校验结果分三级：**PASS** / **WARN**（软警告，可放行）/ **FAIL**（硬错误，方案不可用）。详细 11 类规则与示例输出见 [设计资料/02-SmartLabOS-Card-Tools/README.md](设计资料/02-SmartLabOS-Card-Tools/README.md) 第二节。

##### 第 2 层：git pre-commit 钩子

每次 `git commit` 前自动触发——

- 改方案卡片：只校验改动的方案
- 改平台/模块卡片：触发**全库**方案校验（防止上游改动打破下游方案）
- 任一方案 FAIL → 直接拒绝提交

安装：`bash scripts/install-hooks.sh`（Linux/Mac/Git Bash）或 `PowerShell scripts\install-hooks.ps1`（Windows）。
应急绕过（不推荐）：`git commit --no-verify`。

##### 第 3 层：GitHub Actions 远程兜底

`.github/workflows/validate-cards.yml` 在 push / PR 时并行跑三个 job：

1. `validate-solutions` — 全库方案校验
2. `check-indexes` — 重建索引并 diff，捕获"忘记跑 build_indexes"
3. `recompute-check` — Dry-run 跑 recompute，捕获"resource_summary 与实际不一致"

只在 `references/**` 或 `tools/**` 改动时触发，节省 Actions 配额。

#### 6. 如何使用卡片

##### Claude 的查询模式（基于 frontmatter 精准抽取）

| 用户问 | Claude 的内部动作 |
|--------|-----------------|
| "PLT-1400 能装多少个加热类模块？" | 读 `platform.constraints.max_heating_modules` |
| "MOD-SPE-12 兼容哪些平台？" | 读 `module.compatible_platforms` |
| "找一个 200 样/天的农残方案" | 在 SOL 库筛 `target_analytes:农残` + `throughput_target.samples_per_day >= 200` |
| "这套配置合法吗？" | 同时读方案 + 平台 + 所有模块，跑全部对齐与容量约束 |
| "客户预算不够，有降级方案吗？" | 读 `solution.alternatives` |

##### 售前提案工作流（与五阶段 Skill 协同）

```text
客户需求
   │
   ├─ 阶段 2 采集 → 把行业/样品/通量/基础设施记入 collected-info.md
   ├─ 阶段 3 分析 → 在 SOL 库找最接近的方案
   │                 ├─ 找到 → 微调参数填入 technical-proposal-draft.md
   │                 └─ 没找到 → 由 PLT + MOD 重新组合，并沉淀为新 SOL
   ├─ 阶段 4 审核 → validate_solution.py 跑过 + 五维度审查
   └─ 阶段 5 发布 → 把本次组合反哺回 SOL 库
```

##### 维护更新规则（不同卡片改动频率差异大）

- **平台卡片**：极低频。只在硬件大版本升级、规格调整、停产时改 `version` / `status`
- **模块卡片**：中频。新模块上市、参数微调、价格变动时更新；停产模块改 `status: deprecated` 但不删除文件（可能有历史方案在引用）
- **方案卡片**：高频。每次新交付回填 `deployments`，每次标准更新同步 `matches_standards`，每次性能验证更新 `performance.validated`

##### 未来演进

这套"卡片 + 校验脚本"将来可无缝迁移到平台化阶段——`validate_solution.py` → 后端 API 校验中间件，`build_indexes.py` → 数据库物化视图，frontmatter 字段 → ORM 表字段。现在花在写规则的时间不会浪费。

## 环境与一次性部署（启用卡片自动化）

仓库已经包含全部脚本、钩子、CI 工作流与 seed 卡片，但首次使用时仍需做两件事：**装 Python 依赖** + **装 git pre-commit 钩子**。

### 1. Python 环境

脚本（[validate_solution.py](tools/validate_solution.py) / [build_indexes.py](tools/build_indexes.py) / [recompute_resource_summary.py](tools/recompute_resource_summary.py)）需要：

- Python ≥ 3.9
- 第三方库 `pyyaml`

Windows 安装步骤：

```powershell
# 方式 A：从 python.org 下载安装包（推荐，避免 Microsoft Store 别名）
# 下载 https://www.python.org/downloads/windows/ → 安装时勾选 "Add python.exe to PATH"

# 安装后验证
python --version

# 安装依赖
python -m pip install pyyaml
```

> ⚠️ Windows 自带的 `python` / `python3` 命令默认指向 Microsoft Store 别名，运行后会跳到 Store 安装页。务必从 python.org 安装真实解释器并加入 PATH。

### 2. 一键体检（确认环境就绪）

装好 Python 后，从项目根跑一次：

```bash
# 校验示例方案（应输出 7 通过 / 0 失败 / 若干警告）
python tools/validate_solution.py references/solutions/SOL-QUECHERS-MID.md

# 重建三类卡片索引（覆盖 references/{platforms,modules,solutions}/_index.md）
python tools/build_indexes.py

# 资源占用 dry-run（不写入，只看差异）
python tools/recompute_resource_summary.py references/solutions/
```

或在 Windows 上直接双击 [scripts/check-all.bat](scripts/check-all.bat) 一键跑全套。

### 3. 安装 pre-commit 钩子（每个新克隆都要装一次）

钩子源在 [git-hooks/pre-commit](git-hooks/pre-commit)（受版本控制），需安装到 `.git/hooks/pre-commit`（本地路径，不进 git）：

```bash
# Linux / Mac / Windows Git Bash
bash scripts/install-hooks.sh

# Windows PowerShell
PowerShell -ExecutionPolicy Bypass -File scripts/install-hooks.ps1
```

安装后，每次 `git commit` 涉及 `references/{platforms,modules,solutions}/` 改动时会自动跑 [validate_solution.py](tools/validate_solution.py)，FAIL 则拒绝提交。应急绕过（不推荐）：`git commit --no-verify`。

### 4. GitHub Actions 远程兜底

[.github/workflows/validate-cards.yml](.github/workflows/validate-cards.yml) 在 push / PR 触及 `references/**` 或 `tools/**` 时自动并行跑：

1. `validate-solutions` — 全库方案校验
2. `check-indexes` — 重建索引并 diff，捕获"忘记跑 build_indexes"
3. `recompute-check` — Dry-run 跑 recompute，捕获 `resource_summary` 与实际不一致

无需额外配置，推送后即生效。

### 5. 队友首次克隆仓库后

每位新成员都要跑一次：

```bash
git clone git@github.com:AIer2025/SmartLabOS-AI-Assistant.git
cd SmartLabOS-AI-Assistant
python -m pip install pyyaml          # 装依赖
bash scripts/install-hooks.sh         # 装钩子（或 PowerShell 版）
python tools/build_indexes.py         # 验证脚本可跑
```

---

## 项目工作产物（projects/{name}/）

每个项目目录会按阶段产出以下文件：

```text
collected-info.md             # 阶段 2 采集到的客户信息
analysis-report.md            # 阶段 3 需求分析报告
technical-proposal-draft.md   # 阶段 3 生成的初稿（可有 v2/v3）
review-report.md              # 阶段 4 审查报告
technical-proposal.md         # 阶段 5 正式发布版本
project-summary.md            # 阶段 5 项目摘要
workflow-status.md            # 全程状态追踪
```

## 行为准则（节选自 CLAUDE.md）

- 所有技术参数必须来自 [references/](references/) 知识库，**严禁编造**
- 工艺流程必须严格遵循对应国标
- 不确定时诚实说明，并明确列出需补充的信息
- 涉及报价问题统一回复"请联系销售团队获取报价"
- 仅处理 SmartLabOS 相关方案，不回答无关问题
- 输出统一使用 Markdown 格式，沟通使用中文

## License

[MIT](LICENSE) © 2026 AIer2025
