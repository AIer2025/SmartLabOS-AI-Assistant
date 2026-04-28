#!/usr/bin/env bash
# =============================================================================
# export_openclaw_kb.sh
# -----------------------------------------------------------------------------
# 用途：在 Ubuntu 虚拟机上运行，把 OpenClaw 的知识库、人格文件、Skills 全部
#       抽取到一个独立目录，便于复制到 Windows 侧用 Claude Code 继续使用。
#
# 抽取范围：
#   1. ~/.openclaw/workspace/references/*.md      → references/
#   2. ~/.openclaw/workspace/AGENTS.md            → persona/
#   3. ~/.openclaw/workspace/SOUL.md              → persona/
#   4. ~/.openclaw/skills/*/SKILL.md              → skills/
#   5. ~/.openclaw/workspace/projects/*           → projects-archive/   (可选)
#   6. openclaw.json 的脱敏副本                   → config-redacted.json
#
# 自动清理：Telegram Bot Token / Gateway Token / DeepSeek API Key 会被替换成
#           占位符，避免把凭证带到别处。
#
# 用法：
#   bash export_openclaw_kb.sh                    # 默认输出到 ~/FromUbuntu
#   bash export_openclaw_kb.sh /tmp/my-output     # 指定输出目录
#
# 目标：把产物传到 Windows 机器的：
#   C:\TestClaude\SmartLabOS-AI-Assistant\FromUbuntu\
# 然后在 Windows 侧按 CLAUDE_CODE_SETUP_GUIDE.md 的步骤，将其摆放到
# Claude Code 项目根：C:\TestClaude\SmartLabOS-AI-Assistant\
# =============================================================================

set -euo pipefail

# ---- 路径配置 -------------------------------------------------------------
OPENCLAW_HOME="${OPENCLAW_HOME:-$HOME/.openclaw}"
# 默认输出目录命名为 FromUbuntu，对齐 Windows 侧中转目录：
#   C:\TestClaude\SmartLabOS-AI-Assistant\FromUbuntu
OUTPUT_DIR="${1:-$HOME/FromUbuntu}"
TIMESTAMP="$(date +%Y%m%d-%H%M%S)"

echo "=========================================="
echo "OpenClaw 知识库抽取工具"
echo "=========================================="
echo "源目录 : $OPENCLAW_HOME"
echo "目标目录: $OUTPUT_DIR"
echo ""

# ---- 前置检查 -------------------------------------------------------------
if [[ ! -d "$OPENCLAW_HOME" ]]; then
    echo "❌ 找不到 OpenClaw 目录: $OPENCLAW_HOME"
    echo "   如果路径不同，设置环境变量：OPENCLAW_HOME=/your/path bash $0"
    exit 1
fi

# 已存在则加时间戳，防止覆盖
if [[ -d "$OUTPUT_DIR" ]]; then
    OUTPUT_DIR="${OUTPUT_DIR}-${TIMESTAMP}"
    echo "⚠️  目标目录已存在，改用: $OUTPUT_DIR"
fi

mkdir -p "$OUTPUT_DIR"/{references,persona,skills,projects-archive}

# ---- 1. 抽取 references/ 知识库 -----------------------------------------
echo "[1/6] 抽取 references/ 知识库..."
REF_SRC="$OPENCLAW_HOME/workspace/references"
if [[ -d "$REF_SRC" ]]; then
    # 只抽取 .md 文件，保持平铺结构
    find "$REF_SRC" -maxdepth 2 -type f -name "*.md" -print0 \
        | xargs -0 -I {} cp -v {} "$OUTPUT_DIR/references/"
    COUNT=$(find "$OUTPUT_DIR/references" -name "*.md" | wc -l)
    echo "    ✅ 共抽取 $COUNT 个知识库文件"
else
    echo "    ⚠️  $REF_SRC 不存在，跳过"
fi

# ---- 2. 抽取 AGENTS.md / SOUL.md 人格文件 -------------------------------
echo "[2/6] 抽取 Agent 人格文件..."
for f in AGENTS.md SOUL.md; do
    src="$OPENCLAW_HOME/workspace/$f"
    if [[ -f "$src" ]]; then
        cp -v "$src" "$OUTPUT_DIR/persona/"
    else
        echo "    ⚠️  $src 不存在，跳过"
    fi
done

# ---- 3. 抽取 Skills 目录 -------------------------------------------------
echo "[3/6] 抽取 Skills..."
SKILLS_SRC="$OPENCLAW_HOME/skills"
if [[ -d "$SKILLS_SRC" ]]; then
    # 保持 skills/<skill-name>/SKILL.md 的目录结构
    for skill_dir in "$SKILLS_SRC"/*/; do
        [[ -d "$skill_dir" ]] || continue
        skill_name=$(basename "$skill_dir")
        mkdir -p "$OUTPUT_DIR/skills/$skill_name"
        # 拷贝 SKILL.md 以及同级辅助文件（.md / .txt / .json / 脚本）
        # 使用 rsync 风格的浅拷贝，避免 cp --parents 带来的冗余目录
        shopt -s nullglob
        for f in "$skill_dir"*.md "$skill_dir"*.txt "$skill_dir"*.json \
                 "$skill_dir"*.sh  "$skill_dir"*.py; do
            [[ -f "$f" ]] && cp -v "$f" "$OUTPUT_DIR/skills/$skill_name/"
        done
        shopt -u nullglob
    done
    COUNT=$(find "$OUTPUT_DIR/skills" -name "SKILL.md" | wc -l)
    echo "    ✅ 共抽取 $COUNT 个 Skill"
else
    echo "    ⚠️  $SKILLS_SRC 不存在，跳过"
fi

# ---- 4. 抽取历史项目作为 few-shot 样例（可选） --------------------------
echo "[4/6] 抽取历史项目目录（作为样例）..."
PROJ_SRC="$OPENCLAW_HOME/workspace/projects"
if [[ -d "$PROJ_SRC" ]] && [[ -n "$(ls -A "$PROJ_SRC" 2>/dev/null)" ]]; then
    # 保持项目目录结构，但只拷 .md 文件
    ( cd "$PROJ_SRC" && tar cf - $(find . -type f -name "*.md") ) \
        | ( cd "$OUTPUT_DIR/projects-archive" && tar xf - )
    COUNT=$(find "$OUTPUT_DIR/projects-archive" -name "*.md" | wc -l)
    echo "    ✅ 共抽取 $COUNT 个历史项目 Markdown 文件"
else
    echo "    ℹ️  无历史项目，跳过"
fi

# ---- 5. 生成 openclaw.json 脱敏副本 -------------------------------------
echo "[5/6] 生成 openclaw.json 脱敏副本（仅供参考）..."
CFG_SRC="$OPENCLAW_HOME/openclaw.json"
if [[ -f "$CFG_SRC" ]]; then
    # 用 python 安全替换敏感字段；没有 python 则用 sed 兜底
    if command -v python3 >/dev/null 2>&1; then
        python3 - "$CFG_SRC" "$OUTPUT_DIR/config-redacted.json" <<'PYEOF'
import json, sys, re
src, dst = sys.argv[1], sys.argv[2]
with open(src, 'r', encoding='utf-8') as f:
    raw = f.read()
# 先做一轮正则替换，处理不规范 json（含注释等）
redacted = raw
patterns = [
    (r'("botToken"\s*:\s*)"[^"]*"',        r'\1"<REDACTED_TELEGRAM_BOT_TOKEN>"'),
    (r'("token"\s*:\s*)"[^"]*"',           r'\1"<REDACTED_GATEWAY_TOKEN>"'),
    (r'("apiKey"\s*:\s*)"[^"]*"',          r'\1"<REDACTED_API_KEY>"'),
    (r'("DEEPSEEK_API_KEY"\s*:\s*)"[^"]*"',r'\1"<REDACTED_DEEPSEEK_KEY>"'),
]
for pat, repl in patterns:
    redacted = re.sub(pat, repl, redacted)
with open(dst, 'w', encoding='utf-8') as f:
    f.write(redacted)
print(f"    ✅ 脱敏配置已写入 {dst}")
PYEOF
    else
        sed -E \
            -e 's/("botToken"[[:space:]]*:[[:space:]]*)"[^"]*"/\1"<REDACTED_TELEGRAM_BOT_TOKEN>"/g' \
            -e 's/("token"[[:space:]]*:[[:space:]]*)"[^"]*"/\1"<REDACTED_GATEWAY_TOKEN>"/g' \
            -e 's/("apiKey"[[:space:]]*:[[:space:]]*)"[^"]*"/\1"<REDACTED_API_KEY>"/g' \
            "$CFG_SRC" > "$OUTPUT_DIR/config-redacted.json"
        echo "    ✅ 脱敏配置已写入 $OUTPUT_DIR/config-redacted.json（sed 兜底）"
    fi
else
    echo "    ⚠️  $CFG_SRC 不存在，跳过"
fi

# ---- 6. 生成清单和 README ------------------------------------------------
echo "[6/6] 生成清单 MANIFEST.md..."
MANIFEST="$OUTPUT_DIR/MANIFEST.md"
{
    echo "# SmartLabOS 知识库抽取清单"
    echo ""
    echo "- 抽取时间：$(date -Iseconds)"
    echo "- 源机器 ：$(hostname) (user: $(whoami))"
    echo "- 源目录 ：\`$OPENCLAW_HOME\`"
    echo ""
    echo "## 目录结构"
    echo ""
    echo '```'
    (cd "$OUTPUT_DIR" && find . -type f | sort | sed 's|^\./||')
    echo '```'
    echo ""
    echo "## 文件统计"
    echo ""
    echo "| 类别 | 数量 |"
    echo "|---|---|"
    echo "| references 知识库 | $(find "$OUTPUT_DIR/references" -name "*.md" 2>/dev/null | wc -l) |"
    echo "| persona 人格文件  | $(find "$OUTPUT_DIR/persona" -name "*.md" 2>/dev/null | wc -l) |"
    echo "| skills SKILL.md   | $(find "$OUTPUT_DIR/skills" -name "SKILL.md" 2>/dev/null | wc -l) |"
    echo "| 历史项目文档      | $(find "$OUTPUT_DIR/projects-archive" -name "*.md" 2>/dev/null | wc -l) |"
    echo ""
    echo "## 下一步"
    echo ""
    echo "1. 本目录已打成 \`$(basename "${OUTPUT_DIR}").tar.gz\` 归档文件。"
    echo "2. 将归档文件传输到 Windows 机器的 \`C:\\TestClaude\\SmartLabOS-AI-Assistant\\FromUbuntu\\\` 目录下。"
    echo "3. 在 Windows 端按 CLAUDE_CODE_SETUP_GUIDE.md 摆放到 Claude Code 项目根。"
} > "$MANIFEST"

# ---- 打包 ---------------------------------------------------------------
echo ""
echo "[打包] 生成 tar.gz 归档..."
TARBALL="${OUTPUT_DIR}.tar.gz"
tar czf "$TARBALL" -C "$(dirname "$OUTPUT_DIR")" "$(basename "$OUTPUT_DIR")"
echo "    ✅ 归档文件: $TARBALL ($(du -h "$TARBALL" | cut -f1))"

echo ""
echo "=========================================="
echo "✅ 完成！"
echo "=========================================="
echo "抽取目录: $OUTPUT_DIR"
echo "归档文件: $TARBALL"
echo ""
echo "建议下一步（Ubuntu 端）："
echo "  cat $OUTPUT_DIR/MANIFEST.md          # 查看清单"
echo ""
echo "  # 传到 Windows 机器的目标目录（替换 <win-user> 和 <win-ip>）："
echo "  scp $TARBALL <win-user>@<win-ip>:/C:/TestClaude/SmartLabOS-AI-Assistant/FromUbuntu/"
echo ""
echo "  # 或者先传到 Downloads，再在 Windows 侧手动解压到："
echo "  #   C:\\TestClaude\\SmartLabOS-AI-Assistant\\FromUbuntu\\"
echo ""
