# SmartLabOS git 钩子安装脚本(Windows PowerShell 版)
#
# 使用:
#   PowerShell -ExecutionPolicy Bypass -File scripts\install-hooks.ps1
#
# 如果策略限制无法运行,先以管理员身份执行一次:
#   Set-ExecutionPolicy -Scope CurrentUser RemoteSigned

$ErrorActionPreference = "Stop"

# 找仓库根
try {
    $RepoRoot = (git rev-parse --show-toplevel) -replace '/', '\'
} catch {
    Write-Host "❌ 当前目录不是一个 git 仓库" -ForegroundColor Red
    exit 1
}

$HookSource = Join-Path $RepoRoot "git-hooks\pre-commit"
$HookTarget = Join-Path $RepoRoot ".git\hooks\pre-commit"

if (-not (Test-Path $HookSource)) {
    Write-Host "❌ 找不到源钩子文件 $HookSource" -ForegroundColor Red
    exit 1
}

# 备份已有钩子
if (Test-Path $HookTarget) {
    $TimeStamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $Backup = "$HookTarget.backup-$TimeStamp"
    Copy-Item $HookTarget $Backup
    Write-Host "📦 已备份原钩子到 $Backup" -ForegroundColor Yellow
}

Copy-Item $HookSource $HookTarget -Force
Write-Host "✅ pre-commit 钩子安装完成: $HookTarget" -ForegroundColor Green
Write-Host ""
Write-Host "重要: Windows 上 git 钩子由 Git Bash 解释执行,所以 .git/hooks/pre-commit"
Write-Host "      仍然是 bash 脚本(不需要后缀).你只要装了 Git for Windows 就有 Git Bash."
Write-Host ""
Write-Host "测试: 修改任意 references\solutions\SOL-*.md 后 git commit,会自动触发校验"
Write-Host "跳过(应急): git commit --no-verify"
