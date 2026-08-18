# ================================================================
# 快速部署启动脚本 (Quick Deploy Launcher)
#
# 用途：简化新PC上的部署流程
# 
# 使用方法：
#   1. 将本脚本与发布文件放在同一目录
#   2. 以管理员身份运行: PS> .\Quick-Deploy.ps1
#   3. 按照提示输入配置参数
#
# ================================================================

param(
    [switch]$Interactive = $true
)

# 颜色输出函数
function Write-Title {
    Write-Host ""
    Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║  SmartLabOS DeepSeek Presales AI Agent - 快速部署启动器  ║" -ForegroundColor Cyan
    Write-Host "║                      版本: 2026-08-17                       ║" -ForegroundColor Cyan
    Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
    Write-Host ""
}

function Write-Step {
    param([string]$Message, [int]$StepNumber)
    Write-Host "【步骤 $StepNumber】$Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠️  $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor Red
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor Green
}

# ================================================================
# 主流程
# ================================================================

Write-Title

# 检查管理员权限
if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Error "此脚本必须以管理员身份运行！"
    Write-Host "请以管理员身份重新启动 PowerShell，然后重新运行本脚本"
    Read-Host "按 Enter 键退出"
    exit 1
}

Write-Step "验证前置条件" 1

# 检查是否存在 Publish-DeepSeek-AI-Agent-20260817.ps1
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$mainScript = Join-Path $scriptDir "Publish-DeepSeek-AI-Agent-20260817.ps1"

if (-not (Test-Path $mainScript)) {
    Write-Error "未找到主部署脚本: $mainScript"
    Write-Warning "请确保与此脚本在同一目录中有 'Publish-DeepSeek-AI-Agent-20260817.ps1'"
    Read-Host "按 Enter 键退出"
    exit 1
}
Write-Success "主部署脚本已找到"

# 检查发布目录
$publishDir = Join-Path $scriptDir "IIS-Publish"
if (-not (Test-Path $publishDir)) {
    Write-Error "未找到发布目录: $publishDir"
    Write-Warning "请确保已复制 'IIS-Publish' 目录到此位置"
    Read-Host "按 Enter 键退出"
    exit 1
}
Write-Success "发布目录已找到"

# ================================================================
# 交互式配置
# ================================================================

Write-Host ""
Write-Step "配置参数" 2

Write-Host ""
Write-Host "请输入以下信息（按 Enter 使用默认值）："
Write-Host ""

# 数据库参数
Write-Host "【数据库配置】" -ForegroundColor Cyan
$dbHost = Read-Host "MySQL 服务器地址 (默认: 127.0.0.1)"
if ([string]::IsNullOrWhiteSpace($dbHost)) { $dbHost = "127.0.0.1" }

$dbPort = Read-Host "MySQL 端口 (默认: 3306)"
if ([string]::IsNullOrWhiteSpace($dbPort)) { $dbPort = "3306" }

$dbUser = Read-Host "MySQL 用户名 (默认: root)"
if ([string]::IsNullOrWhiteSpace($dbUser)) { $dbUser = "root" }

$dbPassword = ""
while ([string]::IsNullOrWhiteSpace($dbPassword)) {
    $sec = Read-Host "MySQL 密码 (必填)" -AsSecureString
    $dbPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToCoTaskMemUnicode($sec))
    if ([string]::IsNullOrWhiteSpace($dbPassword)) { Write-Host "  MySQL 密码不能为空。" -ForegroundColor Yellow }
}

$dbName = Read-Host "数据库名称 (默认: SmartLabOS-Presales-AI)"
if ([string]::IsNullOrWhiteSpace($dbName)) { $dbName = "SmartLabOS-Presales-AI" }

Write-Host ""

# IIS 参数
Write-Host "【IIS 配置】" -ForegroundColor Cyan
$iisPort = Read-Host "HTTP 绑定端口 (默认: 80)"
if ([string]::IsNullOrWhiteSpace($iisPort)) { $iisPort = 80 }

$iisAppPool = Read-Host "IIS 应用池名称 (默认: DeepSeekPresalesAIAgent)"
if ([string]::IsNullOrWhiteSpace($iisAppPool)) { $iisAppPool = "DeepSeekPresalesAIAgent" }

$iisSite = Read-Host "IIS 网站名称 (默认: DeepSeek-Presales-AI-Agent)"
if ([string]::IsNullOrWhiteSpace($iisSite)) { $iisSite = "DeepSeek-Presales-AI-Agent" }

Write-Host ""

# DeepSeek 参数
Write-Host "【DeepSeek 配置】" -ForegroundColor Cyan
$deepseekKey = ""
while ([string]::IsNullOrWhiteSpace($deepseekKey)) {
    $deepseekKey = Read-Host "DeepSeek API Key (必填)"
    if ([string]::IsNullOrWhiteSpace($deepseekKey)) { Write-Host "  DeepSeek API Key 不能为空。" -ForegroundColor Yellow }
}

Write-Host ""

# 目录参数
Write-Host "【目录配置】" -ForegroundColor Cyan
$targetDir = Read-Host "目标应用目录 (默认: C:\inetpub\DeepSeek-Presales-AI-Agent)"
if ([string]::IsNullOrWhiteSpace($targetDir)) { $targetDir = "C:\inetpub\DeepSeek-Presales-AI-Agent" }

$refBasePath = Read-Host "知识库根目录 (默认: C:\SmartLabOS-References)"
if ([string]::IsNullOrWhiteSpace($refBasePath)) { $refBasePath = "C:\SmartLabOS-References" }

# SQL 脚本路径
$sqlScriptPath = Join-Path $scriptDir "01-presales-schema.sql"
if (-not (Test-Path $sqlScriptPath)) {
    Write-Warning "未找到 SQL 脚本: $sqlScriptPath"
    $sqlScriptPath = Read-Host "请输入 SQL 脚本的完整路径"
    if (-not (Test-Path $sqlScriptPath)) {
        Write-Error "SQL 脚本不存在！"
        Read-Host "按 Enter 键退出"
        exit 1
    }
}

Write-Host ""

# ================================================================
# 配置确认
# ================================================================

Write-Step "确认配置" 3

Write-Host ""
Write-Host "【部署配置摘要】" -ForegroundColor Cyan
Write-Host "  数据库服务器: $dbHost : $dbPort"
Write-Host "  数据库名称: $dbName"
Write-Host "  数据库用户: $dbUser"
Write-Host "  IIS 网站: $iisSite (端口 $iisPort)"
Write-Host "  IIS 应用池: $iisAppPool"
Write-Host "  应用目录: $targetDir"
Write-Host "  知识库: $refBasePath"
Write-Host "  SQL 脚本: $sqlScriptPath"
Write-Host ""

$confirm = Read-Host "确认上述配置是否正确？ (Y/N，默认: N)"
if ($confirm -ne "Y" -and $confirm -ne "y") {
    Write-Warning "已取消部署"
    Read-Host "按 Enter 键退出"
    exit 0
}

# ================================================================
# 验证前置环境
# ================================================================

Write-Step "验证前置环境" 4

Write-Host ""

# 检查知识库目录
if (-not (Test-Path $refBasePath)) {
    Write-Warning "知识库目录不存在: $refBasePath"
    Write-Host "请确保已将知识库文件复制到该目录"
    Write-Host "继续部署？(Y/N，默认: Y)"
    $continueAnyway = Read-Host
    if ($continueAnyway -eq "N" -or $continueAnyway -eq "n") {
        exit 0
    }
} else {
    Write-Success "知识库目录已找到"
}

# 检查 IIS
try {
    Import-Module WebAdministration -ErrorAction SilentlyContinue
    Write-Success "IIS PowerShell 模块已加载"
} catch {
    Write-Warning "IIS PowerShell 模块加载失败，部分功能可能无法使用"
}

# ================================================================
# 执行部署
# ================================================================

Write-Step "执行部署" 5

Write-Host ""
Write-Host "执行主部署脚本..." -ForegroundColor Cyan
Write-Host ""

& $mainScript `
    -SourcePublishDir $publishDir `
    -SqlScriptPath $sqlScriptPath `
    -TargetAppDir $targetDir `
    -IISSiteName $iisSite `
    -IISAppPoolName $iisAppPool `
    -HttpPort $iisPort `
    -MySqlServer $dbHost `
    -MySqlPort $dbPort `
    -MySqlUser $dbUser `
    -MySqlPassword $dbPassword `
    -MySqlDatabase $dbName `
    -DeepSeekApiKey $deepseekKey `
    -ReferenceBasePath $refBasePath

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Green
    Write-Host "║                     部署成功完成！                         ║" -ForegroundColor Green
    Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Green
    Write-Host ""
    Write-Host "【后续步骤】" -ForegroundColor Green
    Write-Host "1. 在浏览器中打开应用: http://localhost:$iisPort"
    Write-Host "2. 验证数据库连接: GET /api/presales/config"
    Write-Host "3. 检查日志文件: $targetDir\deployment-*.log"
    Write-Host ""
    Write-Host "【重要提醒】" -ForegroundColor Yellow
    Write-Host "• 如果在浏览器看到 502 错误，请检查:"
    Write-Host "  - ASP.NET Core Hosting Bundle (.NET 10.0) 是否已安装"
    Write-Host "  - MySQL 服务是否正常运行"
    Write-Host "  - 应用池和网站状态是否为 Started"
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Red
    Write-Host "║                   部署过程中出现错误                       ║" -ForegroundColor Red
    Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Red
    Write-Host ""
    Write-Host "请查看上方的错误信息，或检查日志文件获取更多详情"
    Write-Host ""
}

Read-Host "按 Enter 键退出"
exit $LASTEXITCODE
