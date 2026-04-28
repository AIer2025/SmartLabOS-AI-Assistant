#!/usr/bin/env bash
# 安装 git 钩子到本仓库的 .git/hooks/
#
# 使用:
#   bash scripts/install-hooks.sh
#
# Windows 用户:
#   - 在 Git Bash 里运行此脚本即可
#   - 或者直接手动复制 git-hooks/pre-commit 到 .git/hooks/pre-commit

set -e

REPO_ROOT="$(git rev-parse --show-toplevel 2>/dev/null)"
if [ -z "$REPO_ROOT" ]; then
    echo "❌ 当前目录不是一个 git 仓库" >&2
    exit 1
fi

HOOK_SOURCE="$REPO_ROOT/git-hooks/pre-commit"
HOOK_TARGET="$REPO_ROOT/.git/hooks/pre-commit"

if [ ! -f "$HOOK_SOURCE" ]; then
    echo "❌ 找不到源钩子文件 $HOOK_SOURCE" >&2
    exit 1
fi

# 备份已有钩子(如果有)
if [ -f "$HOOK_TARGET" ]; then
    BACKUP="$HOOK_TARGET.backup-$(date +%Y%m%d-%H%M%S)"
    cp "$HOOK_TARGET" "$BACKUP"
    echo "📦 已备份原钩子到 $BACKUP"
fi

cp "$HOOK_SOURCE" "$HOOK_TARGET"
chmod +x "$HOOK_TARGET"
echo "✅ pre-commit 钩子安装完成: $HOOK_TARGET"
echo ""
echo "测试: 修改任意 references/solutions/SOL-*.md 后 git commit,会自动触发校验"
echo "跳过(应急): git commit --no-verify"
