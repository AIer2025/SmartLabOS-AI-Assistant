# SmartLabOS 卡片库自动化工具包

> 在前两个工具包(Card-Templates、Card-Tools)的基础上,新增 `recompute_resource_summary.py`(资源占用自动重算)、git pre-commit 钩子、GitHub Actions 工作流,把卡片库的质量保障**自动化**起来。
> 编制日期:2026-04-25

---

## 一、工具包内容

```
SmartLabOS-Automation/
├── README.md                                    ← 本文档
│
├── tools/                                       ← Python 脚本(放到项目 tools/ 下)
│   ├── validate_solution.py                     · 方案合法性校验(已有)
│   ├── build_indexes.py                         · 索引生成(已有)
│   └── recompute_resource_summary.py            · 🆕 资源占用自动重算
│
├── git-hooks/                                   ← git 钩子(安装到 .git/hooks/)
│   └── pre-commit                               · 提交前自动校验改动的方案
│
├── .github/                                     ← GitHub 远程兜底
│   └── workflows/
│       └── validate-cards.yml                   · 推送/PR 时自动校验
│
├── scripts/                                     ← 安装与一键脚本
│   ├── install-hooks.sh                         · Linux/Mac/Git Bash 装钩子
│   ├── install-hooks.ps1                        · Windows PowerShell 装钩子
│   └── check-all.bat                            · Windows 双击一键体检
│
└── sample_kb/                                   ← 示例知识库(供测试和参考)
    ├── platforms/   (1 份卡片 + _index.md)
    ├── modules/     (6 份卡片 + _index.md)
    └── solutions/   (1 份卡片 + _index.md)
```

---

## 二、为什么需要这些自动化

前两个工具包已经给了**模板**和**两个查询脚本**(校验、索引)。但这些都是"被动"的——人不主动跑就不起作用。实际维护中会出现这些情况:

- 改了模块的功率,忘记跑 `recompute_resource_summary`,结果方案 frontmatter 里写的是旧值
- 新加了卡片,忘记跑 `build_indexes`,索引和实际不同步
- 写完一份方案就直接 commit,实际有 FAIL 但没人发现
- 团队成员不熟悉这套流程,操作随意

这个工具包要解决的就是**让 Claude / 人都不需要"记得"运行脚本**——一切由 git 自动触发。

---

## 三、recompute_resource_summary.py — 资源占用自动重算

### 3.1 它做什么

读入一份 SOL-XXX.md 方案卡片,根据它引用的所有模块卡片,自动计算 `resource_summary` 各字段的实际值,并回写到 frontmatter:

- `total_modules_used`:模块位累加
- `total_trays_used`:托盘位累加
- `total_power_kva_peak`:峰值功率累加
- `ventilation_required`:加热模块超过限制时自动置 true
- `compressed_air_required`:任一模块需要时置 true
- `water_required`:同上

### 3.2 它和 validate_solution.py 是什么关系

**validate 抓问题,recompute 修问题**。

| 场景 | validate | recompute |
|------|---------|-----------|
| 你改了某模块的功率,方案没同步 | 报警 WARN | 一键重算回填 |
| 新方案完全没有 resource_summary 字段 | 报警 WARN | 自动追加完整字段块 |
| 方案的字段已经准确 | PASS | 识别"无需修改" |

通常的工作流是:写完方案 → 跑 recompute(让脚本算)→ 跑 validate(确认其他规则也通过)。

### 3.3 基本用法

```bash
# Dry-run(预览,不修改文件) ——默认就是 dry-run
python tools/recompute_resource_summary.py references/solutions/SOL-QUECHERS-MID.md

# 实际写入
python tools/recompute_resource_summary.py references/solutions/SOL-QUECHERS-MID.md --write

# 批量处理整个目录
python tools/recompute_resource_summary.py references/solutions/ --write

# 不要备份 .bak 文件
python tools/recompute_resource_summary.py references/solutions/ --write --no-backup
```

### 3.4 输出示例

```
知识库根目录: /your/project/references
模式: DRY-RUN(预览,不修改)
待处理: 1 份方案文件

📄 references/solutions/SOL-QUECHERS-MID.md
  字段差异:
    total_modules_used: 6  (无变化)
    total_trays_used: 6  (无变化)
    total_power_kva_peak: 3.8  →  4.1
    ventilation_required: False  (无变化)
    compressed_air_required: True  (无变化)
    water_required: False  (无变化)

==================================================
总计: 1 份处理,1 份需修改,0 份无变化,0 份失败

提示: 加 --write 参数实际写入修改
==================================================
```

### 3.5 设计承诺

这个脚本**只动 frontmatter 里 `resource_summary` 这一段,其他什么都不碰**:

- ✅ 其他 frontmatter 字段(id、modules、workflow 等)原封不动
- ✅ 字段顺序、格式、注释完全保留
- ✅ Markdown 正文不动一个字符
- ✅ 写入前自动备份为 `<原文件>.bak`(可用 `--no-backup` 关闭)
- ✅ 默认 dry-run,不会意外破坏文件

### 3.6 退出码

| 退出码 | 含义 |
|--------|------|
| 0 | 成功(可能有修改也可能无变化) |
| 1 | 至少一份方案处理失败 |
| 2 | 脚本本身出错(参数错、依赖缺失) |

### 3.7 测试一下

工具包里有 `sample_kb/`,可以直接用来测试。当前 sample_kb 里 SOL-QUECHERS-MID 的 `total_power_kva_peak` 字段已经是正确的 4.1(我修过了),所以脚本会显示"无需修改"。你可以手动改成错误值再跑测试:

```bash
cd SmartLabOS-Automation

# 故意改错
sed -i 's/total_power_kva_peak: 4.1/total_power_kva_peak: 9.9/' \
    sample_kb/solutions/SOL-QUECHERS-MID.md

# 跑 dry-run,应该报告 9.9 → 4.1
python tools/recompute_resource_summary.py \
    sample_kb/solutions/SOL-QUECHERS-MID.md --kb-root sample_kb

# 实际修复
python tools/recompute_resource_summary.py \
    sample_kb/solutions/SOL-QUECHERS-MID.md --kb-root sample_kb --write
```

---

## 四、git pre-commit 钩子

### 4.1 它做什么

每次执行 `git commit` 时,**在提交真正写入历史之前**:

1. 检查本次提交涉及到的卡片文件
2. 如果改了**方案卡片**,只校验改动的方案
3. 如果改了**平台/模块卡片**(可能影响多个方案),自动跑全库方案校验
4. 任意方案 FAIL,**直接拒绝这次 commit**

这样就杜绝了"错误方案进入 git 历史"的可能。

### 4.2 安装方法

钩子文件存放在仓库的 `git-hooks/pre-commit`(版本受控),需要安装到 `.git/hooks/pre-commit`(本地路径,不进 git)。提供三种安装方式:

**方式 A:Linux / Mac / Windows Git Bash**
```bash
bash scripts/install-hooks.sh
```

**方式 B:Windows PowerShell**
```powershell
PowerShell -ExecutionPolicy Bypass -File scripts\install-hooks.ps1
```

**方式 C:手动**
```bash
# Linux/Mac
cp git-hooks/pre-commit .git/hooks/pre-commit
chmod +x .git/hooks/pre-commit

# Windows(用 Git Bash 或手动复制文件)
```

> ⚠️ Windows 用户注意:即便在 Windows 系统上,git 钩子也是由 Git Bash 执行的(只要装了 Git for Windows 就有)。所以 `.git/hooks/pre-commit` 不需要 .sh 后缀,内容仍然是 bash 脚本。

### 4.3 实际效果(实测三个场景)

**场景 1:正常方案提交 → 通过**
```
$ git commit -m "调整 turnaround_time"
🔍 pre-commit: 发现卡片改动,开始校验...
  · 仅方案有改动,校验改动的方案
▶ 校验 references/solutions/SOL-QUECHERS-MID.md
  小结: 7 通过, 0 失败, 1 警告
✅ pre-commit: 全部方案校验通过
[master 488dc3b] 调整 turnaround_time
```

**场景 2:错误方案提交 → 被拒绝**
```
$ git commit -m "故意把平台改成不存在的"
🔍 pre-commit: 发现卡片改动,开始校验...
  · 仅方案有改动,校验改动的方案
▶ 校验 references/solutions/SOL-QUECHERS-MID.md
  [FAIL] ✗ platform.exists: 平台 PLT-9999 在知识库中找不到
  小结: 3 通过, 2 失败, 0 警告

❌ pre-commit: 1 份方案校验失败,提交被拒绝
   修复后重新提交,或用 'git commit --no-verify' 强制提交(不推荐)
```

**场景 3:改模块卡片 → 触发全库方案校验**
```
$ git commit -m "微调模块重量"
🔍 pre-commit: 发现卡片改动,开始校验...
  · 平台/模块有改动,执行全库方案校验
▶ 校验 references/solutions/SOL-QUECHERS-MID.md
  ...
✅ pre-commit: 全部方案校验通过
[master ba6c055] 微调模块重量
```

### 4.4 应急:绕过钩子

钩子偶尔会误伤(比如你想暂存一份 work-in-progress 的草稿方案)。这时用:

```bash
git commit --no-verify -m "wip: 草稿"
```

但建议这样的草稿放在 `references/solutions/` 之外的目录(比如 `drafts/`),钩子默认不会扫到。

### 4.5 钩子的安全降级

如果环境出问题(没装 python、没装 pyyaml、找不到 validate_solution.py),钩子**不会拒绝提交**,而是打印警告后放行:

```
⚠️  pre-commit: 缺少 pyyaml,请运行 'pip install pyyaml',本次跳过校验
```

这样保证钩子不会因为环境问题彻底卡死提交流程。

---

## 五、GitHub Actions 工作流

### 5.1 它做什么

钩子是本地的——队友可以用 `--no-verify` 绕过,新克隆仓库的人没装钩子也能直接 push。GitHub Actions 是**远程兜底**:每次 push 到主分支或开 PR 时,GitHub 服务器自动跑校验,失败的话 PR 上会显示红叉。

工作流分三个并行 job:

1. **validate-solutions** — 跑全库方案校验,任意 FAIL 整个 job 失败
2. **check-indexes** — 重新生成索引并 diff,如果索引文件需要更新就报错(意味着开发者忘了跑 `build_indexes.py`)
3. **recompute-check** — Dry-run 跑 recompute,如果发现有方案的 resource_summary 与实际不一致就报错

### 5.2 安装

把 `.github/workflows/validate-cards.yml` 放到项目根的 `.github/workflows/` 下,push 到 GitHub 即可。第一次 push 后,在仓库的 Actions 标签页就能看到运行结果。

### 5.3 触发条件

工作流只在改动以下路径时触发:
- `references/**`(任何卡片改动)
- `tools/**`(脚本本身改动)

非这些路径的改动不会触发,节省 Actions 配额。

### 5.4 失败时怎么办

CI 报错的修复路径都直接写在错误信息里:

```
❌ 索引文件已过期! 请在本地运行:
    python tools/build_indexes.py
并提交更新后的 _index.md 文件
```

```
❌ 有方案的 resource_summary 字段与实际汇总不一致! 请在本地运行:
    python tools/recompute_resource_summary.py references/solutions/ --write
并提交更新后的方案文件
```

照着提示在本地跑命令然后 push 就好。

---

## 六、scripts/ 辅助脚本

### 6.1 install-hooks.sh / .ps1

安装 pre-commit 钩子。已存在的旧钩子会自动备份为 `pre-commit.backup-YYYYMMDD-HHMMSS`。

### 6.2 check-all.bat — Windows 一键体检

给不熟悉命令行的同事用。双击就跑两件事:

1. 调用 `build_indexes.py` 重建所有索引
2. 调用 `validate_solution.py` 校验所有方案

输出留在窗口里,看完按任意键关闭。

适合每次大批量改动后做一次"全库体检",或者周报前确认知识库状态。

---

## 七、推荐部署到你的项目

### 7.1 一次性安装(给项目维护者)

假设你项目在 `C:\TestClaude\SmartLabOS-AI-Assistant\`:

```
1. 复制 tools/recompute_resource_summary.py 到项目的 tools/ 下
   (如果之前已经放了 validate_solution.py 和 build_indexes.py,放在一起)

2. 复制 git-hooks/pre-commit 到项目的 git-hooks/ 下
   (这个文件需要受版本控制)

3. 复制 scripts/install-hooks.sh、install-hooks.ps1、check-all.bat 到项目的 scripts/

4. 复制 .github/workflows/validate-cards.yml 到项目的 .github/workflows/

5. 在 git 项目根目录运行钩子安装:
   bash scripts/install-hooks.sh    或   PowerShell scripts\install-hooks.ps1

6. 提交所有新文件:
   git add tools/ git-hooks/ scripts/ .github/
   git commit -m "添加自动化校验工具链"
   git push
```

### 7.2 队友首次克隆仓库后

每个新克隆仓库的人都要手动装一次钩子(因为 .git/hooks/ 不进 git):

```bash
git clone <repo>
cd <repo>
bash scripts/install-hooks.sh   # 或 PowerShell 版
pip install pyyaml
```

可以把这两步写进项目 README 的"开发环境搭建"章节,提醒新人。

### 7.3 完成后的目录结构

```
C:\TestClaude\SmartLabOS-AI-Assistant\
├── .github\workflows\validate-cards.yml      ← CI 配置
├── git-hooks\pre-commit                       ← 钩子源(进 git)
├── scripts\
│   ├── install-hooks.sh
│   ├── install-hooks.ps1
│   └── check-all.bat
├── tools\
│   ├── validate_solution.py
│   ├── build_indexes.py
│   └── recompute_resource_summary.py
└── references\
    ├── platforms\
    ├── modules\
    └── solutions\
```

---

## 八、典型工作流(集成自动化后)

### 8.1 改一个模块的参数

```bash
# 1. 改模块卡片
notepad references\modules\spe\MOD-SPE-12.md  # 把 power_kva_peak 改了

# 2. git add 后 commit
git add references\modules\spe\MOD-SPE-12.md
git commit -m "MOD-SPE-12 功率参数修正"
# 钩子自动触发: 因为改了模块,跑全库方案校验
# 如果某些方案的 resource_summary 与实际不一致,报 WARN

# 3. 看到 WARN 后修复
python tools\recompute_resource_summary.py references\solutions\ --write
git add references\solutions\
git commit -m "同步 resource_summary"
# 钩子再次校验,这次应该全通过

# 4. push 到远端
git push
# GitHub Actions 自动跑 CI,确认无遗漏
```

### 8.2 写一份新方案

```bash
# 1. 复制模板,起草方案
copy references\_templates\solution-card-template.md ^
     references\solutions\SOL-NEW-CASE.md
notepad references\solutions\SOL-NEW-CASE.md
# 不需要手填 resource_summary,放空即可

# 2. 用 recompute 自动填 resource_summary
python tools\recompute_resource_summary.py ^
       references\solutions\SOL-NEW-CASE.md --write

# 3. 提交
git add references\solutions\SOL-NEW-CASE.md
git commit -m "新方案 SOL-NEW-CASE"
# 钩子校验,通过
```

### 8.3 月度全库体检

```bash
# 双击 check-all.bat
# 或命令行:
python tools\build_indexes.py
for %f in (references\solutions\SOL-*.md) do python tools\validate_solution.py "%f"
```

---

## 九、常见问题

### Q1: 钩子拒绝了我的提交,但我现在就是要提交,怎么办?

```bash
git commit --no-verify -m "..."
```
但建议先想想为什么会被拒绝——大概率方案真的有问题,绕过去后 push 也会被 CI 拦下。

### Q2: 钩子在 Windows 上不工作?

确保你装了 Git for Windows(自带 Git Bash)。然后:
- 用 PowerShell 装钩子:`scripts\install-hooks.ps1`
- 或者从 Git Bash 装:`bash scripts/install-hooks.sh`

钩子文件本身是 bash 脚本,Windows 上由 Git Bash 自动解释执行,不需要后缀。

### Q3: GitHub Actions 跑得太慢/太频繁?

工作流配置了 `paths` 过滤,只在改动 `references/**` 或 `tools/**` 时触发,普通文档改动不会跑。如果还是太频繁,可以把 `on.push.branches` 限定为只跑 `main`。

### Q4: recompute 脚本会破坏我的 frontmatter 注释吗?

不会。脚本只动 `resource_summary:` 这一段,其他段落、注释、字段顺序完全不动。而且默认会备份成 `.bak`,出问题可以恢复。

### Q5: 我用 GitLab/Gitea/自建仓库,GitHub Actions 不能用?

钩子是通用的(任何 git 都支持)。GitHub Actions 是 GitHub 专属,但可以参考 `.github/workflows/validate-cards.yml` 的逻辑改写成 GitLab CI 的 `.gitlab-ci.yml` 或 Gitea Actions——核心就是三件事:跑 validate、跑 build_indexes 后 git diff、跑 recompute dry-run。

### Q6: 我能把这套接入 Claude Code 的工作流吗?

可以。在你的 Skill 的 `SKILL.md` 里加一段:
> "生成方案后,必须调用 `python tools/recompute_resource_summary.py <文件> --write` 自动填资源占用,然后调用 `python tools/validate_solution.py <文件>` 校验。看到 FAIL 必须返回修复,不能直接交付。"

这样 Claude 也会按照这套规矩走。

---

## 十、未来演进

按你提案文档的演进路线,这套自动化将来可以无缝迁移到平台化阶段:

- `recompute_resource_summary.py` → 后端 API 保存方案时的预处理钩子
- `validate_solution.py` → 后端 API 的方案校验中间件
- `build_indexes.py` → 数据库的物化视图或缓存预热任务
- pre-commit 钩子 → 前端 GUI 的"保存"按钮触发的实时校验
- GitHub Actions → 部署流水线的质量门

也就是说,现在用 200 行 Python 写的"业务规则",将来直接复用到生产系统里,不用重写。

---

📄 SmartLabOS 卡片库自动化工具包 · v1.0 · 2026-04-25
百泉聚兴(北京)科技有限公司 · SmartLabOS 智慧实验室
