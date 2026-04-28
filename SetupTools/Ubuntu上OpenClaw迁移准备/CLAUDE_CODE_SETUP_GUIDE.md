# SmartLabOS AI 提案助手 — Claude Code 迁移与搭建手册

> 从 Ubuntu + OpenClaw + DeepSeek 迁移到 Windows + VS Code + Claude Code（Claude Max 订阅）
>
> 对应 AI-Summary.md 的【方案 A：Claude Projects + 结构化知识库】的 Claude Code 版本
>
> **本手册绑定目录约定：**
> - **项目根** ：`C:\TestClaude\SmartLabOS-AI-Assistant`
> - **Ubuntu 数据中转目录** ：`C:\TestClaude\SmartLabOS-AI-Assistant\FromUbuntu`

---

## 0. 路线概览

你现在要做的事，本质上就是三件：

1. **在 Ubuntu 上跑 `export_openclaw_kb.sh`**，把知识库、人格、Skills 抽取打包成 `.tar.gz`
2. **把包传到 Windows** 的 `C:\TestClaude\SmartLabOS-AI-Assistant\FromUbuntu\` 下并解压
3. **在 VS Code 里用 Claude Code 打开 `C:\TestClaude\SmartLabOS-AI-Assistant\`**，跑一键摆放脚本 `setup.ps1`，然后就能用

### 关键概念映射

这张表贯穿整份手册，记住：

| OpenClaw 里 | Claude Code 里 | 说明 |
|---|---|---|
| `~/.openclaw/workspace/AGENTS.md` | `C:\TestClaude\SmartLabOS-AI-Assistant\CLAUDE.md` | 项目级系统提示，启动即加载 |
| `~/.openclaw/workspace/SOUL.md` | 并入同一个 `CLAUDE.md` | Claude Code 没有 SOUL 这个独立概念 |
| `~/.openclaw/skills/xxx/SKILL.md` | `.\.claude\skills\xxx\SKILL.md` | **格式完全兼容，直接搬** |
| `~/.openclaw/workspace/references/*.md` | `.\references\*.md` | 纯数据文件 |
| `~/.openclaw/workspace/projects/` | `.\projects\` | 让 Claude 把生成的提案写到这里 |
| `openclaw.json` | ❌ 无需迁移 | API Key/Gateway/Telegram Claude Code 完全用不上 |
| DeepSeek API | Claude Max 订阅（已含 Claude Code） | 后端 LLM 自动换掉 |

### 最终目录长这样

```
C:\TestClaude\SmartLabOS-AI-Assistant\
├── CLAUDE.md                        ← 从 AGENTS.md + SOUL.md 合并而来
├── setup.ps1                        ← 一键摆放脚本（本手册提供）
├── README.md
├── .gitignore
│
├── .claude\
│   └── skills\
│       ├── proposal-workflow\SKILL.md
│       ├── proposal-collector\SKILL.md
│       ├── proposal-analyzer\SKILL.md
│       ├── proposal-generator\SKILL.md
│       └── proposal-reviewer\SKILL.md
│
├── references\                      ← 知识库（6 个 .md）
│   ├── product-specs.md
│   ├── process-quechers.md
│   ├── process-inorganic.md
│   ├── standards-library.md
│   ├── unit-catalog.md
│   └── proposal-template.md
│
├── projects\                        ← Claude 生成的提案放这儿（初始空）
│   └── .gitkeep
│
├── samples\                         ← 历史项目参考（只读）
│   └── ...
│
└── FromUbuntu\                      ← Ubuntu 侧传来的原始归档（摆放后可删）
    ├── FromUbuntu.tar.gz
    ├── MANIFEST.md
    ├── config-redacted.json
    ├── persona\
    ├── references\
    ├── skills\
    └── projects-archive\
```

---

## 1. 在 Ubuntu 虚拟机上抽取知识库

### 1.1 把脚本放到 Ubuntu 上

两种办法任选：

**办法 A：scp 从 Windows 上传**
```powershell
# 在 Windows PowerShell 里
scp export_openclaw_kb.sh <ubuntu用户>@<ubuntu-ip>:~/
```

**办法 B：直接在 Ubuntu 上创建**
```bash
nano ~/export_openclaw_kb.sh
# 粘贴脚本全文，Ctrl+O 保存，Ctrl+X 退出
chmod +x ~/export_openclaw_kb.sh
```

### 1.2 跑抽取

```bash
# 默认会抽到 ~/FromUbuntu 并打成 ~/FromUbuntu.tar.gz
bash ~/export_openclaw_kb.sh

# 如果 OpenClaw 目录不在 ~/.openclaw（比如装在 /opt/openclaw）
OPENCLAW_HOME=/opt/openclaw bash ~/export_openclaw_kb.sh
```

### 1.3 手动复核脱敏

脚本会自动把 `openclaw.json` 里的 Telegram Token、Gateway Token、DeepSeek Key 替换成占位符，但 **references 里的 .md 知识库文件本身可能嵌入过敏感信息**（客户名、联系方式、合同价格），脚本不会去改它们的内容。传出去前自己扫一遍：

```bash
grep -rE "sk-[A-Za-z0-9]{20,}|botToken|password|合同|报价|¥|\$[0-9]" ~/FromUbuntu/ | grep -v REDACTED
# 理想情况下无输出。有输出就手动处理后重新打包。
```

### 1.4 传到 Windows

目标目录：`C:\TestClaude\SmartLabOS-AI-Assistant\FromUbuntu\`

**先在 Windows 上准备好目录**（PowerShell）：
```powershell
New-Item -ItemType Directory -Force -Path "C:\TestClaude\SmartLabOS-AI-Assistant\FromUbuntu"
```

**然后把 tar.gz 放进去**，三种方式任选：

```bash
# 方式 A：从 Ubuntu 推（需要 Windows 开了 OpenSSH Server）
scp ~/FromUbuntu.tar.gz <win用户>@<win-ip>:/C:/TestClaude/SmartLabOS-AI-Assistant/FromUbuntu/

# 方式 B：从 Windows 拉（Windows 一般默认能 ssh 出去）
# 在 Windows PowerShell 里执行：
# scp <ubuntu用户>@<ubuntu-ip>:~/FromUbuntu.tar.gz C:\TestClaude\SmartLabOS-AI-Assistant\FromUbuntu\

# 方式 C：U 盘 / 共享文件夹 / 企业网盘 — 最稳
```

---

## 2. 在 Windows 上搭建 Claude Code 项目

### 2.1 前置准备

确认：

- 已装 VS Code
- 已订阅 Claude Max
- 已装 Claude Code（VS Code 扩展 `Claude Code`，或命令行 `claude`；命令行需要 Node.js 18+）
- 已在 Claude Code 登录你的 Max 账号

### 2.2 创建项目目录并解压

```powershell
# 在 PowerShell 里
New-Item -ItemType Directory -Force -Path "C:\TestClaude\SmartLabOS-AI-Assistant\FromUbuntu"
cd C:\TestClaude\SmartLabOS-AI-Assistant\FromUbuntu

# 假设 FromUbuntu.tar.gz 已放在当前目录
tar -xzf FromUbuntu.tar.gz --strip-components=1
# --strip-components=1 去掉归档里多出来的一层 FromUbuntu/ 目录
# 解压后 persona\、references\、skills\ 等直接在 FromUbuntu\ 下
```

解压完 `FromUbuntu\` 下应该看到：
```
FromUbuntu\
├── FromUbuntu.tar.gz
├── MANIFEST.md
├── config-redacted.json
├── persona\
├── references\
├── skills\
└── projects-archive\
```

### 2.3 用一键摆放脚本 `setup.ps1`

> **⚠️ 编码注意**：请**直接使用迁移工具包里自带的 `setup.ps1` 文件**（已保存为 UTF-8 with BOM），不要从本手册里复制粘贴再保存——如果保存编码不对，Windows PowerShell 5.1 会按 GBK 解析脚本内的字符导致语法错误。
>
> 把 `setup.ps1` 放到 `C:\TestClaude\SmartLabOS-AI-Assistant\setup.ps1` 即可。
>
> **脚本内容预览**（仅供阅读参考）：

```powershell
# =============================================================================
# setup.ps1 — SmartLabOS Claude Code 项目一键摆放脚本
# -----------------------------------------------------------------------------
# 用途：从 FromUbuntu\ 中转目录搬到 Claude Code 规范位置
# 前提：已将 Ubuntu 导出的内容解压到 .\FromUbuntu\
# 运行：在项目根目录 PowerShell 里执行 .\setup.ps1
# =============================================================================

$ErrorActionPreference = "Stop"

$ProjectRoot = "C:\TestClaude\SmartLabOS-AI-Assistant"
$FromUbuntu  = Join-Path $ProjectRoot "FromUbuntu"

if ((Get-Location).Path -ne $ProjectRoot) {
    Write-Host "切换到项目根: $ProjectRoot"
    Set-Location $ProjectRoot
}

if (-not (Test-Path $FromUbuntu)) {
    Write-Host "❌ 找不到 $FromUbuntu" -ForegroundColor Red
    Write-Host "   请先把 Ubuntu 导出的 .tar.gz 解压到这个目录下" -ForegroundColor Red
    exit 1
}

Write-Host "=========================================="
Write-Host "SmartLabOS Claude Code 一键摆放"
Write-Host "=========================================="
Write-Host "项目根  : $ProjectRoot"
Write-Host "中转目录: $FromUbuntu"
Write-Host ""

# 1) 创建目录骨架
Write-Host "[1/6] 创建目录骨架..."
$dirs = @(".\.claude\skills", ".\references", ".\projects", ".\samples")
foreach ($d in $dirs) {
    New-Item -ItemType Directory -Force -Path $d | Out-Null
}
New-Item -ItemType File -Force -Path ".\projects\.gitkeep" | Out-Null
Write-Host "    ✅ .claude\skills\  references\  projects\  samples\"

# 2) 搬 references 知识库
Write-Host "[2/6] 搬 references 知识库..."
$refSrc = Join-Path $FromUbuntu "references"
if (Test-Path $refSrc) {
    Copy-Item "$refSrc\*.md" ".\references\" -Force
    $count = (Get-ChildItem ".\references\*.md").Count
    Write-Host "    ✅ 共 $count 个知识库文件"
} else {
    Write-Host "    ⚠️  $refSrc 不存在" -ForegroundColor Yellow
}

# 3) 搬 Skills
Write-Host "[3/6] 搬 Skills..."
$skillsSrc = Join-Path $FromUbuntu "skills"
if (Test-Path $skillsSrc) {
    Copy-Item "$skillsSrc\*" ".\.claude\skills\" -Recurse -Force
    $count = (Get-ChildItem ".\.claude\skills" -Filter "SKILL.md" -Recurse).Count
    Write-Host "    ✅ 共 $count 个 Skill"
} else {
    Write-Host "    ⚠️  $skillsSrc 不存在" -ForegroundColor Yellow
}

# 4) 合并 AGENTS.md + SOUL.md -> CLAUDE.md，自动改写路径
Write-Host "[4/6] 合并生成 CLAUDE.md..."
$agentsFile = Join-Path $FromUbuntu "persona\AGENTS.md"
$soulFile   = Join-Path $FromUbuntu "persona\SOUL.md"
$agents = if (Test-Path $agentsFile) { Get-Content $agentsFile -Raw -Encoding UTF8 } else { "" }
$soul   = if (Test-Path $soulFile)   { Get-Content $soulFile   -Raw -Encoding UTF8 } else { "" }

# 路径改写：workspace/references/ -> references/
$agents = $agents -replace 'workspace/references/', 'references/'
$agents = $agents -replace 'workspace/projects/',   'projects/'
$agents = $agents -replace '~/\.openclaw/workspace/', ''
$soul   = $soul   -replace 'workspace/references/', 'references/'
$soul   = $soul   -replace 'workspace/projects/',   'projects/'

$claudeMd = @"
# SmartLabOS 方案生成 Agent — 项目指令

> 本文件在每次 Claude Code 会话启动时被加载到 system prompt。
> 从原 OpenClaw 的 AGENTS.md + SOUL.md 合并而来，路径已改写为 Claude Code 规范。

## 来源：AGENTS.md

$agents

---

## 来源：SOUL.md

$soul

---

## Claude Code 特定约定（补充）

- 本项目根：``C:\TestClaude\SmartLabOS-AI-Assistant\``
- 知识库位置：项目根下的 ``references/``
- 提案输出位置：项目根下的 ``projects/<客户或项目名>/``
- Skills 位置：``.claude/skills/``（Claude Code 自动加载）
- 所有技术参数必须从 ``references/`` 读取，**绝不编造**；不确定时明确告知缺失信息
- 涉及价格一律回复 "请联系销售团队"，不给任何数字
- 只处理 SmartLabOS 相关方案，不回答无关问题
"@

Set-Content -Path ".\CLAUDE.md" -Value $claudeMd -Encoding UTF8
Write-Host "    ✅ CLAUDE.md 已生成（路径自动改写 workspace/references/ -> references/）"

# 5) 搬历史项目作为样例
Write-Host "[5/6] 搬历史项目作为样例..."
$archSrc = Join-Path $FromUbuntu "projects-archive"
if ((Test-Path $archSrc) -and (Get-ChildItem $archSrc -ErrorAction SilentlyContinue)) {
    Copy-Item "$archSrc\*" ".\samples\" -Recurse -Force
    $count = (Get-ChildItem ".\samples" -Filter "*.md" -Recurse).Count
    Write-Host "    ✅ 共 $count 个历史项目 .md"
} else {
    Write-Host "    ℹ️  无历史项目，跳过"
}

# 6) 批量改写 Skills 里的路径引用
Write-Host "[6/6] 批量改写 Skills 中的 OpenClaw 路径引用..."
$skillMds = Get-ChildItem ".\.claude\skills" -Filter "*.md" -Recurse
$modified = 0
foreach ($f in $skillMds) {
    $content  = Get-Content $f.FullName -Raw -Encoding UTF8
    $original = $content
    $content = $content -replace 'workspace/references/', 'references/'
    $content = $content -replace 'workspace/projects/',   'projects/'
    $content = $content -replace '~/\.openclaw/workspace/', ''
    if ($content -ne $original) {
        Set-Content -Path $f.FullName -Value $content -Encoding UTF8
        $modified++
    }
}
Write-Host "    ✅ 改写了 $modified 个 Skill 文件"

# 补 .gitignore 和 README（如果不存在）
if (-not (Test-Path ".\.gitignore")) {
@"
# 生成的提案输出
projects/**/*.md
!projects/.gitkeep

# Claude Code 本地状态
.claude/settings.local.json

# Ubuntu 中转目录 — 摆放完成后不提交
FromUbuntu/

# 系统杂项
Thumbs.db
.DS_Store
"@ | Set-Content ".\.gitignore" -Encoding UTF8
    Write-Host "    ✅ 生成 .gitignore"
}

if (-not (Test-Path ".\README.md")) {
@"
# SmartLabOS AI 提案助手（Claude Code 版）

基于 Claude Code + Claude Max 的售前技术方案自动生成工具。

## 快速开始

1. 在 VS Code 里打开 ``C:\TestClaude\SmartLabOS-AI-Assistant``
2. 启动 Claude Code（命令行 ``claude`` 或 VS Code 扩展）
3. 第一次先验证：对 Claude 说 "请列出 references/ 下所有文件并概括"
4. 真实案例：对 Claude 说 "新建提案 XX客户XX项目"，按五阶段推进

## 目录结构

- ``CLAUDE.md`` — Agent 角色/约束（会话启动即加载）
- ``.claude/skills/`` — 五阶段工作流 Skills
- ``references/`` — 行业规范、硬件规格、工艺 SOP 等知识库
- ``projects/<客户名>/`` — 每个客户一个目录，存 6 阶段产物
- ``samples/`` — 历史项目参考
- ``FromUbuntu/`` — Ubuntu 导出的原始归档（摆放完成后可删）
"@ | Set-Content ".\README.md" -Encoding UTF8
    Write-Host "    ✅ 生成 README.md"
}

Write-Host ""
Write-Host "=========================================="
Write-Host "✅ 摆放完成" -ForegroundColor Green
Write-Host "=========================================="
Write-Host "下一步："
Write-Host "  1. 用 VS Code 打开 $ProjectRoot"
Write-Host "  2. 在终端里执行 claude  （或 VS Code 扩展启动 Claude Code）"
Write-Host "  3. 先说 '请列出 references/ 下所有文件并概括' 验证知识库"
Write-Host "  4. 再说 '新建提案 XX客户XX项目' 开始真实流程"
Write-Host ""
Write-Host "如果一切正常，可以把 FromUbuntu\ 目录删掉节省空间："
Write-Host "  Remove-Item -Recurse -Force $FromUbuntu"
```

### 2.4 跑 `setup.ps1`

```powershell
cd C:\TestClaude\SmartLabOS-AI-Assistant

# 第一次跑可能被 PowerShell 执行策略拦，放行本次执行：
PowerShell -ExecutionPolicy Bypass -File .\setup.ps1
```

脚本会自动做以下 6 步：

1. 创建 `.claude\skills\`、`references\`、`projects\`、`samples\` 四个目录
2. 从 `FromUbuntu\references\` 搬知识库到项目根的 `references\`
3. 从 `FromUbuntu\skills\` 搬到 `.claude\skills\`
4. 从 `FromUbuntu\persona\AGENTS.md` + `SOUL.md` 合并生成 `CLAUDE.md`，**并自动把 `workspace/references/` 改写成 `references/`**
5. 从 `FromUbuntu\projects-archive\` 搬历史项目到 `samples\`
6. **批量改写所有 SKILL.md 里的路径**，把 `workspace/references/` → `references/`

此外还会生成 `.gitignore` 和 `README.md`（如果不存在的话）。

### 2.5 验证结果

```powershell
cd C:\TestClaude\SmartLabOS-AI-Assistant
tree /F /A
```

应看到上面"最终目录长这样"那个样子。

---

## 3. 第一次试跑

### 3.1 启动 Claude Code

在 VS Code 里打开 `C:\TestClaude\SmartLabOS-AI-Assistant\` → 打开终端 → 输入：

```bash
claude
```

或者按 Ctrl+Shift+P 搜 "Claude Code: Start Session"。

### 3.2 验证知识库被加载

第一轮先不急着生成提案，先验证 Claude 真的读到了。对话框里贴：

```
你好。请列出 references/ 目录下所有文件名，并用一句话概括每个文件的用途。
不要编造，只基于实际读到的文件内容。
```

如果返回 6 个文件的准确概括，说明配置正确。如果说"目录为空"或"找不到"，检查：

- 当前工作目录是不是 `C:\TestClaude\SmartLabOS-AI-Assistant\`
- `references\` 下是不是真的有 6 个 .md 文件
- `CLAUDE.md` 是不是在项目根

### 3.3 跑一个真实案例

```
新建提案 XX海关农残检测前处理系统
```

Claude 应该触发 `proposal-workflow` Skill，创建 `projects\XX海关农残检测前处理系统\` 目录并生成 `workflow-status.md`。按原 OpenClaw 手册的五阶段继续推进：`开始采集` → `生成初稿` → `审核方案` → `发布方案`。

### 3.4 预期容易出错的点

AI-Summary.md 里已经点明了：**方案 A 的核心价值就是跑 2-3 个真实案例，把提案结构、Claude 在哪一步容易翻车搞清楚。**按经验最容易出错三处：

1. **硬件选型**——Claude 会从 unit-catalog.md 挑型号，但不会主动算"3 台 XX 够不够支撑日检 500 样"这种通量匹配。必要时在提示里明确要求它列"选型依据：通量 = X 样/小时 × Y 小时/班 × Z 班"的推理链。
2. **用电负载计算**——Claude 不会加总，容易漏设备。让它输出一张"设备—额定功率—数量—同时系数—计算负载"的表。
3. **规范引用**——如果 `standards-library.md` 是占位文件，只能引到 GB23200 这种级别。把真实在用的国标全文或条款摘录补进去，引用质量直接翻倍。

这三处正是 AI-Summary.md 里"方案 B MCP Server 兜底"要解决的问题——跑完 2-3 个真实案例再决定要不要上方案 B。

---

## 4. 常见问题

**Q1：Claude 看不到 references/ 目录？**
检查：(1) 你确实是在 `C:\TestClaude\SmartLabOS-AI-Assistant\` 启动的 claude；(2) `CLAUDE.md` 里的路径是相对路径 `references/` 而不是 `~/.openclaw/workspace/references/`。`setup.ps1` 会自动处理 (2)，但如果你手动改过 CLAUDE.md 要注意。

**Q2：Skill 没被触发？**
打开对应的 `.claude\skills\xxx\SKILL.md`，把 `description` 字段写得更具体。Claude 是靠语义匹配 description 决定要不要加载这个 Skill，描述越贴近用户会说的话，触发越准。

**Q3：Claude 编造硬件参数？**
在 `CLAUDE.md` 里加一条硬规则："如果 references/ 里找不到某个参数，不要编造，必须明确告知'此参数需要补充 references/'并停止生成。"

**Q4：Claude Max 额度够用吗？**
Max 20x 档每 5 小时的 token 池单次生成一份完整提案（几万 token 级别）很充裕。开多个会话并行跑要盯着 `/status` 里的用量。

**Q5：可以保留 Ubuntu 上的 OpenClaw 不动吗？**
完全可以。references/ 和 SKILL.md 两边格式兼容，可以两边同时维护同一份知识库。需要同步时重跑 `export_openclaw_kb.sh` + `setup.ps1` 即可。

**Q6：`setup.ps1` 被 PowerShell 执行策略挡住？**
用 `PowerShell -ExecutionPolicy Bypass -File .\setup.ps1`，这个只放行本次执行，不改系统全局策略，最安全。

---

## 附：一行命令速查

```bash
# Ubuntu 端
bash export_openclaw_kb.sh                        # 抽取 + 打包到 ~/FromUbuntu.tar.gz

# Windows 端（PowerShell）
cd C:\TestClaude\SmartLabOS-AI-Assistant\FromUbuntu
tar -xzf FromUbuntu.tar.gz --strip-components=1   # 解压到 FromUbuntu\

cd C:\TestClaude\SmartLabOS-AI-Assistant
PowerShell -ExecutionPolicy Bypass -File .\setup.ps1  # 一键摆放

claude                                             # 或用 VS Code 扩展启动

# 会话里
"请列出 references/ 下所有文件并概括"              # 验证知识库
"新建提案 XX客户XX项目"                            # 阶段 1
"开始采集"                                        # 阶段 2
"生成初稿"                                        # 阶段 3
"审核方案"                                        # 阶段 4
"发布方案"                                        # 阶段 5
"查看进度"                                        # 随时可用
```

---

*本手册配合 `export_openclaw_kb.sh` 使用。遇到问题把 Ubuntu 脚本日志或 Windows `setup.ps1` 日志贴给我继续排查。*
