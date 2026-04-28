# SmartLabOS 卡片库辅助工具

> 配套 SmartLabOS 卡片体系的 Python 脚本工具集
> 编制日期:2026-04-25

本工具包提供两个 Python 脚本,用于自动化维护 SmartLabOS 知识库中的平台卡片(PLT)、模块卡片(MOD)、工艺方案卡片(SOL)。

```
SmartLabOS-Card-Tools/
├── README.md                    ← 本文档
├── tools/
│   ├── validate_solution.py     ← 方案合法性校验
│   └── build_indexes.py         ← 索引文件自动生成
└── sample_kb/                   ← 示例知识库(供测试和参考)
    ├── platforms/
    │   ├── PLT-1400.md
    │   └── _index.md            (build_indexes.py 生成)
    ├── modules/
    │   ├── MOD-WEIGH-12.md
    │   ├── MOD-DISP-SHAKE-24.md
    │   ├── MOD-CENT-24.md
    │   ├── MOD-PIPETTE-8.md
    │   ├── MOD-SPE-12.md
    │   ├── MOD-CONC-6.md
    │   └── _index.md            (build_indexes.py 生成)
    └── solutions/
        ├── SOL-QUECHERS-MID.md
        └── _index.md            (build_indexes.py 生成)
```

---

## 一、环境准备

### 1.1 Python 版本要求

需要 Python 3.9 或以上。检查方式:

```bash
python --version
# 或
python3 --version
```

如果 Windows 命令行里 `python` 是 3.9+ 直接用 `python`,否则用 `python3`。下面文档统一写 `python`,你按实际情况替换。

### 1.2 依赖安装

只依赖一个第三方库 `pyyaml`:

```bash
pip install pyyaml
```

如果你的环境用的是公司代理或离线包,从 PyPI 下载 `PyYAML` 离线安装即可。

### 1.3 把工具放到项目里

建议在你的 `C:\TestClaude\SmartLabOS-AI-Assistant\` 项目下建一个 `tools\` 目录,把两个脚本放进去:

```
C:\TestClaude\SmartLabOS-AI-Assistant\
├── references\                  ← 卡片库(已有)
│   ├── platforms\
│   ├── modules\
│   └── solutions\
└── tools\                       ← 工具脚本
    ├── validate_solution.py
    └── build_indexes.py
```

后面命令行示例都基于这个目录结构。

---

## 二、validate_solution.py — 方案合法性校验

### 2.1 它做什么

读入一份 SOL-XXX.md 工艺方案卡片,根据它引用的平台和模块卡片,自动检查方案是否合法。会校验 11 类规则:

| # | 规则 | 触发 FAIL 的条件 |
|---|------|----------------|
| 1 | 平台存在性 | `platform.id` 在知识库中找不到 |
| 2 | 模块存在性 | 任一 `modules[].id` 在知识库中找不到 |
| 3 | 平台-模块兼容性 | 模块的 `compatible_platforms` 不含选中平台,或在 `incompatible_platforms` 中 |
| 4 | 模块位占用 | 模块 `module_slots` 累加 > 平台 `max_modules` |
| 5 | 托盘位占用 | 模块 `tray_slots` 累加 > 平台 `max_trays` |
| 6 | 峰值功率 | 模块 `power_kva_peak` 累加 > 平台 `power_kva_peak` |
| 7 | 通风规则 | 加热类模块超出限制但未启用通风(WARN) |
| 8 | 公用工程一致性 | 模块需要压缩空气/水但未在 `resource_summary` 声明(WARN) |
| 9 | resource_summary 一致性 | 手填值与实际汇总不符(WARN) |
| 10 | workflow 引用一致性 | `workflow[].module_id` 不在 `modules` 列表中 |
| 11 | 平台功能集合 | 模块 `function` 不在平台 `supported_functions` 中(WARN) |

**FAIL 是硬错误**,意味着方案不可用。**WARN 是软警告**,意味着可能有疏漏但不一定致命。

### 2.2 基本用法

```bash
# 在项目根目录下运行,脚本会自动定位 references/ 知识库根
cd C:\TestClaude\SmartLabOS-AI-Assistant
python tools\validate_solution.py references\solutions\SOL-QUECHERS-MID.md

# 显式指定知识库根目录(适合脚本调用)
python tools\validate_solution.py SOL-QUECHERS-MID.md --kb-root C:\TestClaude\SmartLabOS-AI-Assistant\references
```

### 2.3 输出示例

正常方案:

```
============================================================
校验报告: SOL-QUECHERS-MID
============================================================
  [INFO] · metadata: 方案状态: status=active, maturity=已交付
  [PASS] ✓ platform.exists: 平台 PLT-1400 存在 (PLT-1400.md)
  [PASS] ✓ modules.exists: 全部 6 个模块均已找到
  [PASS] ✓ compatibility: 所有模块均兼容平台 PLT-1400
  [PASS] ✓ capacity.modules: 模块位占用 6/6
  [PASS] ✓ capacity.trays: 托盘位占用 6/8
  [WARN] ! capacity.power: 峰值功率累加 4.10 kVA 接近平台上限 4.50 kVA(>90%),建议核对
  [PASS] ✓ workflow.refs: workflow 6 个步骤引用一致
小结: 7 通过, 0 失败, 2 警告
============================================================
```

错误方案(故意引用不存在的平台和模块):

```
  [FAIL] ✗ platform.exists: 平台 PLT-9999 在知识库中找不到
  [FAIL] ✗ modules.exists: 以下模块在知识库中找不到: MOD-NOT-EXIST
  [FAIL] ✗ workflow.refs: workflow 引用了未在 modules 列表声明的模块: step 2(MOD-MYSTERY)
小结: 0 通过, 4 失败, 3 警告
```

### 2.4 退出码(供自动化使用)

| 退出码 | 含义 |
|--------|------|
| 0 | 全部通过(可能有 WARN) |
| 1 | 至少一项 FAIL |
| 2 | 脚本本身出错(文件找不到、YAML 格式错等) |

这设计是为了能配合 git 钩子或 CI:退出码非 0 就拒绝提交。

### 2.5 跑全库校验(批量场景)

脚本本身一次只处理一份方案,但可以用 shell 配合循环跑全库:

**Windows PowerShell:**
```powershell
Get-ChildItem references\solutions\*.md -Exclude _*.md | ForEach-Object {
    Write-Host "校验 $($_.Name) ..."
    python tools\validate_solution.py $_.FullName
}
```

**Linux/Mac/WSL:**
```bash
for f in references/solutions/SOL-*.md; do
    echo "校验 $f ..."
    python tools/validate_solution.py "$f"
done
```

### 2.6 测试一下

工具包附带的 `sample_kb/` 是个迷你知识库,可以直接用来测试:

```bash
cd SmartLabOS-Card-Tools
python tools/validate_solution.py sample_kb/solutions/SOL-QUECHERS-MID.md
```

应该会输出 7 通过 / 2 警告 / 0 失败。

---

## 三、build_indexes.py — 索引文件自动生成

### 3.1 它做什么

扫描 `references/platforms/`、`references/modules/`、`references/solutions/` 三个目录下的所有卡片(.md 文件),解析每份卡片的 frontmatter,把关键字段渲染成 Markdown 表格,生成各目录下的 `_index.md`。

为什么需要索引?当模块数量从 5 个长到 150 个,Claude 要回答"我有哪些 SPE 模块"就得读 150 个文件,慢且费 token。索引把全库视图拍平成一张表,Claude 读一份索引就能掌握全库,然后精准定位到要深读哪几份卡片。

人也一样——打开 `_index.md` 5 分钟就看清全貌,不用翻遍目录。

### 3.2 基本用法

```bash
# 自动探测知识库根目录
cd C:\TestClaude\SmartLabOS-AI-Assistant
python tools\build_indexes.py

# 显式指定
python tools\build_indexes.py --kb-root C:\TestClaude\SmartLabOS-AI-Assistant\references

# 安静模式(只打印警告和错误)
python tools\build_indexes.py --quiet
```

### 3.3 生成的索引长什么样

**`platforms/_index.md`**:

```markdown
# 平台卡片索引
> SmartLabOS 平台型号清单
> 共收录 3 份卡片

## 平台一览
| ID | 名称 | 状态 | 尺寸(L×W×H mm) | 模块/托盘上限 | 峰值功率 | 标签 |
|----|------|------|----------------|--------------|---------|------|
| [PLT-1400](PLT-1400.md) | 标准型 1400 平台 | 🟢 active | 1400×850×1200 | 6M / 8T | 4.5 kVA | 主力机型 |
```

**`modules/_index.md`**(按 category 自动分组):

```markdown
# 模块卡片索引

## 类别概览
| 类别 | 数量 |
| SPE | 8 |
| 离心 | 5 |
| 浓缩 | 12 |
...

## SPE (8 个)
| ID | 名称 | 状态 | 通量 | 兼容平台 | 功率 | 需通风/气/水 | 标签 |
| [MOD-SPE-12](MOD-SPE-12.md) | 12 通道 SPE | 🟢 active | 12 管/25min | PLT-1200, PLT-1400, PLT-1800 | 0.6 kVA | 气 | 核心 |
...
```

**`solutions/_index.md`**(按成熟度排序,附行业反查):

```markdown
# 工艺方案卡片索引

## 方案一览
| ID | 名称 | 成熟度 | 行业 | 通量 | 平台 | 模块数 | 标签 |
| [SOL-QUECHERS-MID](SOL-QUECHERS-MID.md) | QuEChERS 中通量 | ✅ 已交付 | 食品检测 | 200 | PLT-1400 | 6 | 主推 |

## 按行业反查
- **食品检测**: SOL-QUECHERS-MID, SOL-QUECHERS-HIGH
- **环境监测**: SOL-DIGEST-METAL
```

### 3.4 什么时候跑

每次卡片有变动后跑一次。三种触发方式,你选最舒服的:

**方式 A:手动跑(最简单)**
改完一批卡片后在终端敲一次命令。

**方式 B:做成 .bat / .sh 一键脚本**
在项目根放个 `rebuild-indexes.bat`(Windows):

```batch
@echo off
cd /d %~dp0
python tools\build_indexes.py
pause
```

双击就跑完。

**方式 C:git 钩子(进阶)**
在 `.git/hooks/post-commit` 里加一行调用,每次 commit 后自动重建索引。但要注意索引会进入下一次提交,对 git 历史可能有点啰嗦。

### 3.5 退出码

| 退出码 | 含义 |
|--------|------|
| 0 | 全部成功 |
| 1 | 部分卡片解析失败(已跳过,索引仍生成) |
| 2 | 脚本本身出错(目录不存在等) |

### 3.6 _index.md 的特殊地位

注意几点:

- 文件名以下划线开头(`_index.md`),意味着它是"基础设施文件"。`build_indexes.py` 和 `validate_solution.py` 都会跳过下划线开头的文件,不会把索引当成卡片再处理一次
- 文件头明确写了"⚠️ 本文件由脚本自动生成,请勿手工编辑"。改了也会被下次运行覆盖
- 索引里的卡片名是 Markdown 链接,在 VS Code 或 Obsidian 里点击就能跳转到对应卡片

---

## 四、典型工作流

### 4.1 日常维护(单卡更新)

```bash
# 1. 你修改了某份模块卡片,比如 MOD-SPE-12.md
notepad references\modules\spe\MOD-SPE-12.md

# 2. 重建索引,让索引和卡片同步
python tools\build_indexes.py

# 3. 如果你改的是模块的关键字段(比如功率),
#    检查所有引用了这个模块的方案是否还合法
for f in references/solutions/SOL-*.md; do
    python tools/validate_solution.py "$f"
done
```

### 4.2 新增方案

```bash
# 1. 复制方案模板
copy references\_templates\solution-card-template.md ^
     references\solutions\SOL-NEW-CASE.md

# 2. 填写方案内容(尤其是 platform.id, modules[], workflow)

# 3. 校验
python tools\validate_solution.py references\solutions\SOL-NEW-CASE.md

# 4. 看到 0 FAIL 才视为合法,可以入库
#    有 FAIL 就回去改方案;有 WARN 就根据警告判断是否要处理

# 5. 重建索引
python tools\build_indexes.py
```

### 4.3 全库体检(建议每月一次)

```bash
# 重建所有索引(顺便揭露解析失败的卡片)
python tools\build_indexes.py

# 校验所有方案
for f in references/solutions/SOL-*.md; do
    python tools/validate_solution.py "$f" || echo "❌ $f 校验失败"
done
```

### 4.4 把脚本融入 Claude Code 工作流

如果你后续把这两个脚本接入 Claude 的工作流,可以这样设计:

- Claude 修改卡片后,主动调用 `build_indexes.py` 重建索引
- Claude 生成新方案后,主动调用 `validate_solution.py` 校验,FAIL 时停下来报错给用户
- 这两个动作可以写进对应 Skill 的 SKILL.md 里作为强制步骤

---

## 五、常见问题

### Q1: 我的卡片字段与脚本里写的不完全一致,会怎样?

脚本对所有字段都做了**安全读取**——字段不存在或类型不对都返回默认值,不会崩溃。例如某个模块没写 `power_kva_peak`,功率累加时它就按 0 算,不会拖垮整个校验。

但这意味着:**字段缺失不会被脚本主动报错**。如果你想强制要求某些字段必填,自己加一段检查就好(在 `validate_solution.py` 顶部加几条 if 即可)。

### Q2: YAML 解析报错怎么办?

最常见的原因是冒号后面忘了空格、缩进用了 Tab、字符串包含特殊字符没加引号。脚本会输出具体哪一行出错,对照修就行。

如果你不确定 YAML 是否合法,可以单独测试:

```bash
python -c "import yaml; print(yaml.safe_load(open('your-card.md', encoding='utf-8').read().split('---')[1]))"
```

### Q3: 我想加新规则怎么办?

打开 `validate_solution.py`,在 `validate()` 函数里仿照已有规则的写法加一段就好。每条规则就是 `if 条件 then report.add("FAIL/WARN/PASS", "rule_name", "message")` 的模式。

例如想强制要求所有 `maturity: 已交付` 的方案必须有 `deployments` 字段:

```python
if sol.get("maturity") == "已交付" and not sol.get("deployments"):
    report.add("FAIL", "deployments",
               "已交付方案必须填写 deployments 字段")
```

### Q4: 我的模块按子目录(SPE/centrifuge/...)分类放,脚本能找到吗?

能。两个脚本都用 `rglob("*.md")` 递归扫描,放多深的子目录都能找到。

### Q5: 索引生成后我手改了一下,会被下次运行覆盖吗?

会。`_index.md` 的设计就是"完全由脚本管理"。如果你想加自定义内容,要么改 `build_indexes.py` 的渲染函数,要么把自定义内容放在另一份文件里(比如 `_overview.md`)。

### Q6: Windows 路径里的反斜杠会有问题吗?

Python 的 `pathlib` 在 Windows 上对 `/` 和 `\` 都支持。命令行里用反斜杠或正斜杠都行,跨平台脚本里用 Path 对象更稳。

---

## 六、未来演进

按你的提案文档(§6 演进路线),这两个脚本将来可以无缝过渡到阶段 2 的平台化:

- `validate_solution.py` 的校验逻辑 → 后端 API 的方案保存接口校验中间件
- `build_indexes.py` 的索引生成 → 数据库的方案列表查询 + 物化视图
- 脚本里 `load_card()` / `find_card_by_id()` 的逻辑 → ORM 的查询封装

也就是说,现在花在写规则的时间不会浪费,后期是直接复用。

---

📄 SmartLabOS 卡片库辅助工具 · v1.0 · 2026-04-25
