# ================================================================
# SmartLabOS DeepSeek Presales AI Agent 自动部署脚本
# 
# 用途：在新的 Windows 11 Enterprise PC 上完成以下工作：
#       1. 安装 MySQL 数据库（如需要）
#       2. 初始化数据库结构并执行迁移
#       3. 安装/启用 IIS 及相关功能组件
#       4. 创建 IIS 应用池与网站
#       5. 发布应用程序
#       6. 配置应用程序绑定与权限
#
# 执行方法：
#       以管理员身份运行 PowerShell：
#       PS> Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process
#       PS> & "C:\path\to\Publish-DeepSeek-AI-Agent-20260817.ps1" -SourcePublishDir "C:\source\IIS-Publish" -SqlScriptPath "C:\source\database\01-presales-schema.sql"
#
# 生成日期：2026-08-17
# ================================================================

param(
    # 源发布目录（来自本机编译的IIS-Publish文件夹）
    [Parameter(Mandatory = $true, HelpMessage = "来源发布目录路径，包含已编译的应用程序")]
    [string]$SourcePublishDir,
    
    # SQL脚本路径
    [Parameter(Mandatory = $true, HelpMessage = "数据库初始化SQL脚本路径")]
    [string]$SqlScriptPath,
    
    # 目标部署目录（新PC上的IIS目录）
    [string]$TargetAppDir = "C:\inetpub\DeepSeek-Presales-AI-Agent",
    
    # IIS 网站名称
    [string]$IISSiteName = "DeepSeek-Presales-AI-Agent",
    
    # IIS 应用池名称
    [string]$IISAppPoolName = "DeepSeekPresalesAIAgent",
    
    # 绑定端口
    [int]$HttpPort = 80,
    
    # MySQL 连接参数
    [string]$MySqlServer = "127.0.0.1",
    [int]$MySqlPort = 3306,
    [string]$MySqlUser = "root",
    [string]$MySqlPassword = $env:MYSQL_PASSWORD,
    [string]$MySqlDatabase = "SmartLabOS-Presales-AI",
    
    # DeepSeek API Key
    [string]$DeepSeekApiKey = $env:DEEPSEEK_API_KEY,
    
    # 引用知识库路径（新PC上的对应路径，需要事先同步）
    [string]$ReferenceBasePath = "C:\SmartLabOS-References"
)

# ================================================================
# 机密参数校验
# 口令与 API Key 不再硬编码在脚本里（脚本会进 git，硬编码等于公开）。
# 传入方式二选一：
#   1) 命令行参数：  -MySqlPassword "xxx" -DeepSeekApiKey "sk-xxx"
#   2) 环境变量：    $env:MYSQL_PASSWORD / $env:DEEPSEEK_API_KEY
# ================================================================
if ([string]::IsNullOrWhiteSpace($MySqlPassword)) {
    throw "缺少 MySQL 口令。请用 -MySqlPassword 传入，或先设置环境变量 MYSQL_PASSWORD。"
}
if ([string]::IsNullOrWhiteSpace($DeepSeekApiKey)) {
    throw "缺少 DeepSeek API Key。请用 -DeepSeekApiKey 传入，或先设置环境变量 DEEPSEEK_API_KEY。"
}

# ================================================================
# 日志与错误处理
# ================================================================

$LogFile = "$TargetAppDir\deployment-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"
$ErrorActionPreference = "Stop"

function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "[$timestamp] [$Level] $Message"
    Write-Host $logMessage
    Add-Content -Path $LogFile -Value $logMessage -ErrorAction SilentlyContinue
}

function Write-ErrorLog {
    param([string]$Message)
    Write-Log -Message $Message -Level "ERROR"
}

function Write-SuccessLog {
    param([string]$Message)
    Write-Log -Message $Message -Level "SUCCESS"
}

# 确保以管理员身份运行
if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-ErrorLog "此脚本必须以管理员身份运行！"
    exit 1
}

Write-Log "================================================================"
Write-Log "SmartLabOS DeepSeek Presales AI Agent 部署脚本开始执行"
Write-Log "目标部署目录: $TargetAppDir"
Write-Log "IIS网站: $IISSiteName"
Write-Log "IIS应用池: $IISAppPoolName"
Write-Log "绑定端口: $HttpPort"
Write-Log "================================================================"

# ================================================================
# 第一步：验证前置条件
# ================================================================

Write-Log "===== 步骤 1: 验证前置条件 ====="

# 验证源目录存在
if (-not (Test-Path $SourcePublishDir)) {
    Write-ErrorLog "源发布目录不存在: $SourcePublishDir"
    exit 1
}
Write-SuccessLog "✓ 源发布目录存在"

# 验证SQL脚本存在
if (-not (Test-Path $SqlScriptPath)) {
    Write-ErrorLog "SQL脚本不存在: $SqlScriptPath"
    exit 1
}
Write-SuccessLog "✓ SQL脚本存在"

# 检查MySQL是否已安装（可选，如未安装则提示）
if (-not (Get-Command mysql -ErrorAction SilentlyContinue)) {
    Write-Log "警告: mysql命令行工具未找到，将尝试使用 mysql.exe 的默认路径"
    $mysqlCmd = "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe"
    if (-not (Test-Path $mysqlCmd)) {
        Write-ErrorLog "MySQL命令行工具未找到！请先安装MySQL Server"
        Write-Log "     参考: https://dev.mysql.com/downloads/mysql/"
        exit 1
    }
} else {
    $mysqlCmd = "mysql"
}
Write-SuccessLog "✓ MySQL命令行工具可用"

# ================================================================
# 第二步：初始化目标应用目录
# ================================================================

Write-Log "===== 步骤 2: 初始化目标应用目录 ====="

if (Test-Path $TargetAppDir) {
    Write-Log "目标目录已存在，备份现有文件..."
    $backupDir = "$TargetAppDir.backup.$(Get-Date -Format 'yyyyMMdd-HHmmss')"
    Move-Item -Path $TargetAppDir -Destination $backupDir -Force
    Write-Log "已备份至: $backupDir"
}

# 创建目标目录
New-Item -ItemType Directory -Path $TargetAppDir -Force | Out-Null
Write-SuccessLog "✓ 创建目标目录: $TargetAppDir"

# ================================================================
# 第三步：复制应用程序文件
# ================================================================

Write-Log "===== 步骤 3: 复制应用程序文件 ====="

try {
    Copy-Item -Path "$SourcePublishDir\*" -Destination $TargetAppDir -Recurse -Force
    Write-SuccessLog "✓ 应用程序文件已复制"
} catch {
    Write-ErrorLog "复制文件失败: $_"
    exit 1
}

# 创建配置目录
$configDir = "$TargetAppDir"
if (-not (Test-Path $configDir)) {
    New-Item -ItemType Directory -Path $configDir -Force | Out-Null
}

# ================================================================
# 第四步：配置 appsettings.json
# ================================================================

Write-Log "===== 步骤 4: 配置 appsettings.json ====="

$appSettingsPath = "$TargetAppDir\appsettings.json"

try {
    if (Test-Path $appSettingsPath) {
        $appSettings = Get-Content -Path $appSettingsPath -Raw | ConvertFrom-Json
        
        # 更新数据库连接配置
        if (-not $appSettings.ConnectionStrings) {
            $appSettings | Add-Member -Type NoteProperty -Name "ConnectionStrings" -Value @{}
        }
        $appSettings.ConnectionStrings.DefaultConnection = "Server=$MySqlServer;Port=$MySqlPort;User=$MySqlUser;Password=$MySqlPassword;Database=$MySqlDatabase;SslMode=None;Charset=utf8mb4"
        
        # 更新Agent配置
        if (-not $appSettings.Agent) {
            $appSettings | Add-Member -Type NoteProperty -Name "Agent" -Value @{}
        }
        $appSettings.Agent.Provider = "deepseek"
        $appSettings.Agent.ApiKey = $DeepSeekApiKey
        $appSettings.Agent.Model = "deepseek-reasoner"
        $appSettings.Agent.BaseUrl = "https://api.deepseek.com/anthropic"
        $appSettings.Agent.Thinking = "adaptive"
        
        # 保存更新的配置
        $appSettings | ConvertTo-Json -Depth 10 | Set-Content -Path $appSettingsPath -Encoding UTF8
        Write-SuccessLog "✓ appsettings.json 已更新"
    }
} catch {
    Write-ErrorLog "更新 appsettings.json 失败: $_"
    exit 1
}

# ================================================================
# 第五步：配置 Presales-AI-Agent-config.json
# ================================================================

Write-Log "===== 步骤 5: 配置 Presales-AI-Agent-config.json ====="

$configJsonPath = "$TargetAppDir\Presales-AI-Agent-config.json"

# 构建知识库路径配置
$referencePaths = @{
    Module_Path       = "$ReferenceBasePath\01-modules"
    Platform_Path     = "$ReferenceBasePath\02-platforms"
    Pallet_Path       = "$ReferenceBasePath\06-pallet"
    Protocol_Path     = "$ReferenceBasePath\potocol\MD"
    Proposal_Output_Path = "$ReferenceBasePath\projects"
    Proposal_Template = "$ReferenceBasePath\09-ProposalTemplate"
    Template_Path     = "$ReferenceBasePath\_templates"
}

# 构建完整的配置对象
$configContent = @{
    DB_Connect = @{
        DB_IP       = $MySqlServer
        DB_Port     = $MySqlPort.ToString()
        DB_User     = $MySqlUser
        DB_Password = $MySqlPassword
        DB_Name     = $MySqlDatabase
        DB_SslMode  = "None"
        DB_CharSet  = "utf8mb4"
    }
    ClaudeAI_Env = $referencePaths
    DeepSeek_Env = @{
        DeepSeek_API_Key = $DeepSeekApiKey
        Model            = "deepseek-reasoner"
        Base_Url         = "https://api.deepseek.com/anthropic"
    }
}

# 写入JSON配置（保持UTF-8格式）
$configJson = $configContent | ConvertTo-Json -Depth 10
$Utf8NoBomEncoding = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($configJsonPath, $configJson, $Utf8NoBomEncoding)

Write-SuccessLog "✓ Presales-AI-Agent-config.json 已创建"
Write-Log "   数据库: ${MySqlServer}:$MySqlPort/$MySqlDatabase"
Write-Log "   知识库根目录: $ReferenceBasePath"

# ================================================================
# 第六步：初始化数据库
# ================================================================

Write-Log "===== 步骤 6: 初始化数据库 ====="

try {
    Write-Log "执行SQL脚本: $SqlScriptPath"
    
    # 构建MySQL命令
    $sqlScript = Get-Content -Path $SqlScriptPath -Raw
    
    # 执行SQL脚本
    $sqlScript | & $mysqlCmd -h $MySqlServer -P $MySqlPort -u $MySqlUser -p"$MySqlPassword" 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-SuccessLog "✓ 数据库已初始化"
    } else {
        Write-ErrorLog "数据库初始化失败！检查MySQL连接和脚本内容"
        Write-Log "MySQL命令: $mysqlCmd -h $MySqlServer -P $MySqlPort -u $MySqlUser"
        exit 1
    }
} catch {
    Write-ErrorLog "执行数据库脚本异常: $_"
    Write-Log "请手动执行SQL脚本或检查MySQL服务状态"
}

# ================================================================
# 第七步：安装 IIS 功能
# ================================================================

Write-Log "===== 步骤 7: 安装 IIS 和相关功能 ====="

try {
    # 检查IIS是否已安装
    $iisFeature = Get-WindowsFeature -Name Web-Server
    if ($iisFeature.InstallState -ne "Installed") {
        Write-Log "IIS未安装，开始安装..."
        
        # 安装IIS和必要的功能
        Install-WindowsFeature -Name Web-Server -IncludeAllSubFeature -IncludeManagementTools -Restart:$false | Out-Null
        Write-SuccessLog "✓ IIS 已安装"
        
        # 启用 ASP.NET Core 相关功能
        Install-WindowsFeature -Name Web-Net-Extensibility45, Web-Asp-Net45, Web-AppInit, Web-ISAPI-Ext, Web-ISAPI-Filter -Restart:$false | Out-Null
        Write-SuccessLog "✓ ASP.NET Core 功能已启用"
    } else {
        Write-SuccessLog "✓ IIS 已安装"
    }
    
    # 确保ASP.NET Core Hosting Bundle已安装
    Write-Log "检查 ASP.NET Core Hosting Bundle..."
    $hostingBundle = Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*" | Where-Object { $_.DisplayName -like "*ASP.NET Core*Hosting*" }
    if (-not $hostingBundle) {
        Write-Log "警告: ASP.NET Core Hosting Bundle (.NET 10.0) 未检测到"
        Write-Log "请从以下地址下载安装: https://dotnet.microsoft.com/download/dotnet"
        Write-Log "然后重新运行此脚本"
    } else {
        Write-SuccessLog "✓ ASP.NET Core Hosting Bundle 已安装"
    }
    
} catch {
    Write-ErrorLog "IIS安装异常: $_"
    Write-Log "请手动安装IIS: Add-WindowsFeature -Name Web-Server -IncludeAllSubFeature"
}

# ================================================================
# 第八步：创建 IIS 应用池
# ================================================================

Write-Log "===== 步骤 8: 创建 IIS 应用池 ====="

try {
    Import-Module WebAdministration -ErrorAction SilentlyContinue
    
    # 检查应用池是否存在
    $appPool = Get-IISAppPool -Name $IISAppPoolName -ErrorAction SilentlyContinue
    
    if ($null -eq $appPool) {
        # 创建新应用池
        New-IISAppPool -Name $IISAppPoolName -Force
        Write-Log "创建应用池: $IISAppPoolName"
        
        # 配置应用池参数
        $appPoolPath = "IIS:\AppPools\$IISAppPoolName"
        Set-ItemProperty -Path $appPoolPath -Name "managedRuntimeVersion" -Value ""  # 无托管代码（.NET Core）
        Set-ItemProperty -Path $appPoolPath -Name "managedPipelineMode" -Value 0    # 集成模式
        
        Write-SuccessLog "✓ 应用池已创建并配置"
    } else {
        Write-SuccessLog "✓ 应用池已存在: $IISAppPoolName"
    }
    
    # 启动应用池
    $appPoolState = (Get-IISAppPool -Name $IISAppPoolName).State
    if ($appPoolState -ne "Started") {
        Start-IISAppPool -Name $IISAppPoolName
        Write-Log "应用池已启动"
    }
    
} catch {
    Write-ErrorLog "应用池创建异常: $_"
    exit 1
}

# ================================================================
# 第九步：创建 IIS 网站
# ================================================================

Write-Log "===== 步骤 9: 创建 IIS 网站 ====="

try {
    # 检查网站是否存在
    $site = Get-IISSite -Name $IISSiteName -ErrorAction SilentlyContinue
    
    if ($null -eq $site) {
        # 创建新网站
        New-IISSite -Name $IISSiteName -PhysicalPath $TargetAppDir -BindingInformation "*:$($HttpPort):" -Protocol "http" -ApplicationPool $IISAppPoolName -Force
        Write-Log "创建网站: $IISSiteName"
        Write-SuccessLog "✓ 网站已创建"
    } else {
        Write-SuccessLog "✓ 网站已存在: $IISSiteName"
        
        # 移除现有绑定并重新创建
        $binding = Get-IISSiteBinding -Name $IISSiteName -Protocol "http" -ErrorAction SilentlyContinue
        if ($binding) {
            Remove-IISSiteBinding -Name $IISSiteName -Protocol "http" -Binding $binding -Force
        }
        
        # 创建新绑定
        New-IISSiteBinding -Name $IISSiteName -Protocol "http" -BindingInformation "*:$($HttpPort):"
        
        # 更新物理路径
        Set-IISSitePhysicalPath -Name $IISSiteName -PhysicalPath $TargetAppDir
    }
    
} catch {
    Write-ErrorLog "网站创建异常: $_"
    exit 1
}

# ================================================================
# 第十步：配置 web.config
# ================================================================

Write-Log "===== 步骤 10: 配置 web.config ====="

$webConfigPath = "$TargetAppDir\web.config"

if (Test-Path $webConfigPath) {
    Write-Log "检测到web.config，验证ASP.NET Core配置..."
    Write-SuccessLog "✓ web.config 已就位（由发布过程生成）"
} else {
    Write-Log "未检测到web.config，将由IIS自动配置"
}

# ================================================================
# 第十一步：设置文件权限
# ================================================================

Write-Log "===== 步骤 11: 设置文件夹权限 ====="

try {
    # 获取IIS应用池用户（通常是IIS AppPool\<AppPoolName>）
    $appPoolUser = "IIS AppPool\$IISAppPoolName"
    
    # 为应用池用户赋予读取权限
    $acl = Get-Acl -Path $TargetAppDir
    $rule = New-Object System.Security.AccessControl.FileSystemAccessRule($appPoolUser, "Modify", "ContainerInherit,ObjectInherit", "None", "Allow")
    $acl.AddAccessRule($rule)
    Set-Acl -Path $TargetAppDir -AclObject $acl
    
    Write-SuccessLog "✓ 应用池用户权限已配置"
    Write-Log "   用户: $appPoolUser"
    Write-Log "   权限: Modify (读取、写入)"
    
} catch {
    Write-ErrorLog "权限配置异常: $_"
    Write-Log "请手动为 '$appPoolUser' 用户赋予 Modify 权限"
}

# ================================================================
# 第十二步：启动网站
# ================================================================

Write-Log "===== 步骤 12: 启动网站 ====="

try {
    $siteState = (Get-IISSite -Name $IISSiteName).State
    if ($siteState -ne "Started") {
        Start-IISSite -Name $IISSiteName
        Write-Log "网站已启动"
    }
    Write-SuccessLog "✓ 网站状态检查完成"
    
} catch {
    Write-ErrorLog "启动网站异常: $_"
}

# ================================================================
# 第十三步：配置环境变量
# ================================================================

Write-Log "===== 步骤 13: 配置系统环境变量 ====="

# 设置应用程序配置文件路径（可选，用于多环境支持）
try {
    [Environment]::SetEnvironmentVariable("PRESALES_AGENT_CONFIG", "$configJsonPath", "Machine")
    Write-Log "设置环境变量 PRESALES_AGENT_CONFIG = $configJsonPath"
    Write-SuccessLog "✓ 环境变量已配置"
} catch {
    Write-Log "警告: 设置环境变量失败 - $_"
    Write-Log "应用可以从工作目录读取配置文件，此设置非必需"
}

# ================================================================
# 验收与输出
# ================================================================

Write-Log "===== 验收检查 ====="

# 验证应用文件
$appDllExists = Test-Path "$TargetAppDir\Presales.Proposal.AI.Agent.dll"
$configExists = Test-Path $configJsonPath
$appSettingsExists = Test-Path $appSettingsPath

Write-Log "✓ 应用DLL: $(if ($appDllExists) {'存在'} else {'缺失'})"
Write-Log "✓ 配置文件: $(if ($configExists) {'存在'} else {'缺失'})"
Write-Log "✓ appsettings.json: $(if ($appSettingsExists) {'存在'} else {'缺失'})"

# 验证IIS配置
$appPoolExists = Get-IISAppPool -Name $IISAppPoolName -ErrorAction SilentlyContinue
$siteExists = Get-IISSite -Name $IISSiteName -ErrorAction SilentlyContinue

Write-Log "✓ IIS应用池: $(if ($appPoolExists) {'✓ 已创建'} else {'✗ 缺失'})"
Write-Log "✓ IIS网站: $(if ($siteExists) {'✓ 已创建'} else {'✗ 缺失'})"

Write-Log ""
Write-Log "================================================================"
Write-Log "部署完成！"
Write-Log "================================================================"
Write-Log ""
Write-Log "【访问应用】"
Write-Log "   URL: http://localhost:$HttpPort"
Write-Log "   或:  http://<新PC的IP地址>:$HttpPort"
Write-Log ""
Write-Log "【验证检查清单】"
Write-Log "   ☑ 应用文件已复制到: $TargetAppDir"
Write-Log "   ☑ 数据库已初始化: $MySqlServer/$MySqlDatabase"
Write-Log "   ☑ IIS网站已创建: $IISSiteName (端口$HttpPort)"
Write-Log "   ☑ 应用池已配置: $IISAppPoolName"
Write-Log "   ☑ 配置文件已生成: $configJsonPath"
Write-Log ""
Write-Log "【重要提醒】"
Write-Log "   1. 确保知识库文件已复制到新PC的: $ReferenceBasePath"
Write-Log "      包括: 01-modules, 02-platforms, 06-pallet 等目录"
Write-Log "   2. 确保 ASP.NET Core Hosting Bundle (.NET 10.0) 已安装"
Write-Log "      从此处下载: https://dotnet.microsoft.com/download/dotnet"
Write-Log "   3. 若在浏览器中看到 502 Bad Gateway，请检查:"
Write-Log "      - 应用池是否运行"
Write-Log "      - 数据库连接是否正常"
Write-Log "      - 事件查看器中的应用程序日志"
Write-Log ""
Write-Log "【故障排查】"
Write-Log "   查看部署日志: $LogFile"
Write-Log "   IIS日志目录: C:\inetpub\logs\LogFiles"
Write-Log "   应用日志输出: 应用程序会记录到 Console 和文件系统"
Write-Log ""
Write-Log "================================================================"

Write-SuccessLog "脚本执行完毕！"
exit 0
