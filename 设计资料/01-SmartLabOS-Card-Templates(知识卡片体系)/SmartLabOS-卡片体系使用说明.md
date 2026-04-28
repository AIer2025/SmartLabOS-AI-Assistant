# SmartLabOS 卡片体系使用说明

> 配套文件:平台卡片、模块卡片、工艺方案卡片三套模板及示例
> 编制日期:2026-04-25

本文档说明三类卡片的设计意图、关联关系、使用流程,以及在 `C:\TestClaude\SmartLabOS-AI-Assistant` 项目下的目录规划建议。

---

## 一、三类卡片的定位

三类卡片对应售前提案的三个抽象层级,各自回答一个问题:

| 卡片类型 | 文件前缀 | 回答的问题 | 类比 |
|---------|---------|-----------|------|
| 平台卡片 (Platform Card) | `PLT-` | 我有什么硬件底盘? | 汽车的"车型平台" |
| 模块卡片 (Module Card) | `MOD-` | 我能做什么动作? | 汽车的"零部件" |
| 工艺方案卡片 (Solution Card) | `SOL-` | 针对某类需求,我推荐什么组合? | 汽车的"配置版本" |

平台和模块是物理资产,工艺方案是组合资产。客户来了,销售应该先从工艺方案卡片库里找最接近的,再做微调,而不是从模块开始重新拼。

---

## 二、三类卡片的关联关系

### 2.1 引用关系图

```
                    ┌─────────────────────┐
                    │  工艺方案卡片 (SOL)  │
                    │  - 行业/样品/通量    │
                    │  - 工艺流程         │
                    │  - 性能与价格       │
                    └──────┬──────┬───────┘
                           │      │
                  引用 1:1 │      │ 引用 1:N
                           ▼      ▼
              ┌──────────────┐  ┌──────────────┐
              │ 平台卡片 PLT │  │ 模块卡片 MOD │
              │  - 物理底盘  │◄─┤  - 兼容平台  │
              │  - 容量上限  │  │  - 工艺能力  │
              └──────────────┘  └──────────────┘
                       ▲                ▲
                       │  容量校验关系   │
                       └────────────────┘
                       (模块占位累加 ≤ 平台上限)
```

### 2.2 字段级对齐规则

下面这些字段的值在三类卡片间必须保持一致,这是后期自动校验的依据:

**方案 → 平台 引用**
- `solution.platform.id` 必须能在平台卡片库里找到对应 `platform.id`

**方案 → 模块 引用**
- `solution.modules[].id` 必须能在模块卡片库里找到对应 `module.id`

**模块 → 平台 兼容**
- 方案选用的每个模块的 `module.compatible_platforms` 必须包含 `solution.platform.id`
- 反之,如果模块的 `incompatible_platforms` 包含该平台,方案非法

**资源容量约束**
- 所有模块 `module_slots` 累加 ≤ `platform.max_modules`
- 所有模块 `tray_slots` 累加 ≤ `platform.max_trays`
- 所有模块 `power_kva_peak` 累加 ≤ `platform.power_kva_peak`

**功能集合约束**
- `solution.workflow[].step` 用到的模块,其 `function` 必须落在 `platform.supported_functions` 集合内

**特殊条件触发**
- 累计 `heat_generating: true` 的模块数 > `platform.constraints.max_heating_modules` → `ventilation_required: true`
- 任一模块 `requires_compressed_air: true` → `solution.resource_summary.compressed_air_required: true`

**枚举词表对齐**
- 平台的 `supported_functions` 与模块的 `function` 来自同一份词表(称量/提取/浓缩/净化/...)
- 平台的 `gripper_options` 与模块的 `gripper_required` 来自同一份词表

### 2.3 命名规则约定

为了方便人和 Claude 都能一眼看出关系,建议统一命名:

| 类型 | 前缀 | 命名格式 | 示例 |
|------|------|---------|------|
| 平台 | `PLT-` | `PLT-` + 型号编号 | `PLT-1400`、`PLT-1800` |
| 模块 | `MOD-` | `MOD-` + 类别缩写 + 通道数/规格 | `MOD-SPE-12`、`MOD-CENT-24` |
| 方案 | `SOL-` | `SOL-` + 工艺缩写 + 通量级别 | `SOL-QUECHERS-MID`、`SOL-DIGEST-HIGH` |

文件名直接用 ID,如 `PLT-1400.md`、`MOD-SPE-12.md`、`SOL-QUECHERS-MID.md`。

---

## 三、三类卡片的使用方法

### 3.1 建卡顺序(从下往上)

**第一阶段:建平台卡片(P0)**
公司只有 3 个平台型号(800/1200/1400),一周内全部建完。这是底层抽象,后续所有方案都基于它们。

**第二阶段:建高频模块卡片(P0)**
先挑销售最常用的 20~30 个核心模块,一周内建完;剩余 120 多个模块用模板批量补,可以先只填 ID/名称/类别三个字段,参数后续逐步补全。同时建一份 `modules-index.md` 列出所有模块 ID/名称/分类,便于 Claude 快速检索。

**第三阶段:建工艺方案卡片(P1)**
方案卡片是"组合"资产,只有平台和模块都建好了才能写。建议每跑完一个真实提案就沉淀一份方案卡片,起步先做 3~5 份覆盖主流场景(QuEChERS 农残、酸消解金属、SPE 兽残等)。

### 3.2 客户来了之后的工作流

```
客户需求 → 在 SOL 库找最接近的方案
            │
            ├─ 找到了 → 微调参数 → 出报价
            │
            └─ 没找到 → 从 PLT + MOD 重新组合
                        → 沉淀为新的 SOL 卡片
                        → 进入 SOL 库
```

这个流程让方案库越用越值钱:每次提案都给方案库添砖加瓦,3 个月后大部分客户需求都能在库里找到 80% 匹配的现成方案。

### 3.3 维护更新规则

**平台卡片**:很少改动。只在硬件大版本升级、规格调整、停产时更新,改 `version` 和 `status`。

**模块卡片**:中等频率改动。新模块上市、参数微调、价格变动时更新。停产模块改 `status: deprecated` 但不删除文件,因为可能还有历史方案在引用。

**方案卡片**:高频改动。每次新交付都回填 `deployments`,每次标准更新都同步 `matches_standards`,每次性能验证都更新 `performance.validated`。

### 3.4 Claude 的查询模式

写好的卡片库后,Claude 可以做这些事(基于 frontmatter 的精准抽取):

- "PLT-1400 能装多少个加热类模块?" → 读 `platform.constraints.max_heating_modules`
- "MOD-SPE-12 兼容哪些平台?" → 读 `module.compatible_platforms`
- "找一个 200 样/天的农残方案" → 在 SOL 库筛 `target_analytes:农残` + `throughput_target.samples_per_day >= 200`
- "这套配置合法吗?" → 同时读方案 + 平台 + 所有模块,跑 §2.2 的所有约束检查
- "客户预算不够,有降级方案吗?" → 读 `solution.alternatives`

---

## 四、目录结构规划

基于你的提案文档(§2.1 知识库总览)中已经规划的 `references/` 路径,我建议在 `C:\TestClaude\SmartLabOS-AI-Assistant` 下采用如下结构:

```
C:\TestClaude\SmartLabOS-AI-Assistant\
│
├── CLAUDE.md                              # 项目宪法,会话启动注入
├── README.md                              # 项目说明
│
├── .claude\                               # Claude Code 配置
│   └── skills\                            # 各个 Skill (workflow/collector/...)
│
├── references\                            # ⭐ 知识库根目录
│   ├── platforms\                         # 平台卡片库
│   │   ├── _index.md                      # 平台索引(自动维护)
│   │   ├── PLT-800.md
│   │   ├── PLT-1200.md
│   │   └── PLT-1400.md
│   │
│   ├── modules\                           # 模块卡片库(150+)
│   │   ├── _index.md                      # 模块索引(ID/名称/类别)
│   │   ├── _categories.md                 # 类别枚举词表
│   │   ├── weighing\                      # 按类别分子目录
│   │   │   └── MOD-WEIGH-12.md
│   │   ├── dispensing\
│   │   │   └── MOD-DISP-SHAKE-24.md
│   │   ├── centrifuge\
│   │   │   └── MOD-CENT-24.md
│   │   ├── pipetting\
│   │   │   └── MOD-PIPETTE-8.md
│   │   ├── spe\
│   │   │   └── MOD-SPE-12.md
│   │   ├── concentration\
│   │   │   └── MOD-CONC-6.md
│   │   └── ...                            # 其他类别
│   │
│   ├── solutions\                         # 工艺方案卡片库
│   │   ├── _index.md                      # 方案索引
│   │   ├── SOL-QUECHERS-MID.md
│   │   ├── SOL-QUECHERS-HIGH.md
│   │   ├── SOL-DIGEST-METAL.md
│   │   └── ...
│   │
│   ├── _templates\                        # ⭐ 三类卡片模板
│   │   ├── platform-card-template.md
│   │   ├── module-card-template.md
│   │   └── solution-card-template.md
│   │
│   ├── _vocabulary\                       # 受控词表(可选,但强烈建议)
│   │   ├── functions.md                   # 工艺功能词表
│   │   ├── module-categories.md           # 模块类别词表
│   │   ├── sample-types.md                # 样品类型词表
│   │   └── industries.md                  # 行业词表
│   │
│   ├── standards-library.md               # 国标库(已规划)
│   ├── instruments.md                     # 后端仪器兼容库(已规划)
│   ├── proposal-template.md               # 文档模板(已就位)
│   ├── ops-constants.md                   # 运营常数表
│   ├── process-quechers.md                # 工艺流程库
│   ├── process-inorganic.md
│   ├── process-tcm.md
│   └── _sources\                          # 原始资料(PDF 等)
│       └── standards\                     # 国标 PDF 原文
│
├── samples\                               # 历史样本库(已规划)
│   ├── std-chengdu-foodtest\
│   │   ├── original.pdf
│   │   └── summary.md
│   └── tcm-dalian\
│       ├── 06-V1.2.html
│       └── summary.md
│
├── projects\                              # 项目产出(已规划)
│   ├── 客户A\
│   │   ├── _inputs\
│   │   ├── workflow-status.md
│   │   └── ...
│   └── 客户B\
│
└── tools\                                 # 辅助脚本(可选)
    ├── validate_solution.py               # 方案合法性校验脚本
    ├── recompute_resource_summary.py      # 自动重算资源占用
    └── build_indexes.py                   # 自动生成 _index.md
```

### 4.1 几点目录设计说明

**为什么模块按类别分子目录?**
150 多个模块平铺在一个目录下不便浏览,按 SPE/离心/浓缩/称量等类别分子目录更清晰。但子目录的名称必须和 `_categories.md` 词表对齐,Claude 检索时不会迷路。如果模块数量起步阶段不多(<50),也可以先平铺,后续再分。

**`_index.md` 是什么?**
每类卡片库下放一份索引文件,列出该库所有卡片的 ID/名称/状态/标签,便于 Claude 一次性扫描"我有哪些资源",而不必逐个文件读。建议用 Python 脚本自动生成,内容例如:

```markdown
| ID | 名称 | 类别 | 状态 | 标签 |
|----|------|------|------|------|
| MOD-SPE-12 | 12 通道 SPE 净化模块 | SPE | active | 核心模块,高频 |
| MOD-CENT-24 | 24 管离心模块 | 离心 | active | 核心模块 |
| ... |
```

**`_templates\` 为什么放 `references\` 下?**
模板本身是知识库的一部分(规定了"卡片该长什么样"),和 `standards-library.md`、`proposal-template.md` 同级。建卡时直接复制模板再改,而不是从空白开始。

**`_vocabulary\` 是什么,为什么需要?**
受控词表防止"加液振荡 / 加液+振荡 / 摇床加液"这种同义异写。每类枚举字段(模块类别、工艺功能、样品类型、行业)都在 `_vocabulary\` 下定义一份白名单,所有卡片只能从中选。Claude 在写卡片时也必须遵守。这个看似繁琐,但模块数量超过 30 个后会救你的命。

**`tools\` 下的脚本起什么作用?**
后期写一些 Python 脚本帮忙做机械工作:
- `validate_solution.py`:输入一份 SOL 卡片,自动检查所有 §2.2 的字段对齐和容量约束,输出一份校验报告
- `recompute_resource_summary.py`:自动从 `solution.modules` 列表汇总各项资源占用,回写到 `resource_summary` 字段
- `build_indexes.py`:扫描 `platforms\`、`modules\`、`solutions\` 目录,自动生成 `_index.md`

这些脚本不复杂,每个 50~100 行 Python,但能让卡片库永远保持一致。

### 4.2 与你提案文档(Proposal-AI-Assistant-20260424)的对齐

我刻意让目录结构和你提案文档的 §2.1 保持一致:

- `references/platforms/` ✅ 提案文档已规划
- `references/modules/` ✅ 提案文档已规划
- `references/solutions/` 🆕 本次新增,作为三类卡片的最高层抽象
- `references/_templates/` 🆕 本次新增,放三套模板
- `references/_vocabulary/` 🆕 本次新增,词表治理

新增的三个目录都用下划线开头(`_templates`、`_vocabulary`),区别于业务卡片目录,排序时会排在前面,一眼能看出是"基础设施"。

---

## 五、上手步骤(给你的 7 天计划)

| 天 | 动作 | 产出 |
|----|------|------|
| 1 | 在 `C:\TestClaude\SmartLabOS-AI-Assistant\references\` 下建好上述目录 | 空目录骨架 |
| 1 | 把本次打包的 3 个模板放入 `_templates\` | 模板就位 |
| 2 | 拷贝 PLT-1400 示例到 `platforms\`,核对参数;补 PLT-800、PLT-1200 | 3 份平台卡 |
| 2 | 起草 `_vocabulary\functions.md` 和 `module-categories.md` 词表 | 2 份词表 |
| 3-4 | 挑 20 个高频模块,基于模板填卡(参数不全的字段先留 null) | 20 份模块卡 |
| 5 | 跑一个真实提案,沉淀第一份 SOL 卡片(可以从 SOL-QUECHERS-MID 改) | 1 份方案卡 |
| 6 | 写 `tools\validate_solution.py`,验证那份方案卡通过校验 | 校验脚本 |
| 7 | 写 `tools\build_indexes.py`,生成各 `_index.md` | 索引脚本 + 完整骨架 |

7 天后你就有了一个最小可用的知识库,可以开始接真实客户提案。后续每跑一个项目就让库长大一点,3~6 个月后过渡到 API + GUI 阶段时,这套结构可以无缝迁移到数据库表(每类卡片对应一张表,frontmatter 字段对应表字段)。

---

## 六、附:本次打包的内容

```
SmartLabOS-Card-Templates\
├── README-卡片体系使用说明.md    (本文档)
├── templates\
│   ├── platform-card-template.md
│   ├── module-card-template.md
│   └── solution-card-template.md
└── examples\
    ├── PLT-1400.md
    ├── MOD-SPE-12.md
    └── SOL-QUECHERS-MID.md
```

模板是空白模板,直接复制改。示例是填好的样板,演示从空到满的样子,可以参考但不要直接当真实数据使用(其中部分参数是为了演示而虚构)。

---

📄 SmartLabOS 卡片体系使用说明 · v1.0 · 2026-04-25
