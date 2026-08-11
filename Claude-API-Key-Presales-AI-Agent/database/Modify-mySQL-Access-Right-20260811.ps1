<#
.SYNOPSIS
    为 SmartLabOS Presales AI Agent 开通 MySQL 远程访问权限。

.DESCRIPTION
    症状：程序启动自检报「MySQL 连通性：失败」，接口返回
        Host 'x.x.x.x' is not allowed to connect to this MySQL server
    端口其实是通的 —— 卡在 MySQL 的账号授权：账号只对 'localhost' 授权，
    而程序按 env 配置里的 IP 连过来，服务端看到的来源主机是那个 IP，于是拒绝。

    本脚本按顺序处理三层障碍，每一层都先检测、再按需修复：
        1. Windows 防火墙  —— 放行 3306 入站（仅当规则不存在时新增）
        2. MySQL bind-address —— 只监听 127.0.0.1 时给出 my.ini 修改指引
        3. 账号授权        —— CREATE USER + GRANT，让指定来源主机可连接

    连接参数默认从 env/Presales-AI-Agent-config.json 读取，与程序完全同源，
    不会出现「脚本改的账号和程序用的账号不是同一个」。

.PARAMETER ConfigPath
    env 配置文件路径。默认取本脚本上级目录的 env\Presales-AI-Agent-config.json。

.PARAMETER AllowHost
    授权的来源主机。默认取配置中的 DB_IP。
    '%' = 任意主机（内网便利但面最大）；'192.168.101.%' = 限定网段（推荐）。

.PARAMETER AdminUser
    执行授权用的管理员账号，默认 root（从 localhost 连本机 MySQL 执行）。

.PARAMETER AdminPassword
    管理员口令。不传则用配置中的 DB_Password；仍为空时交互式询问。

.PARAMETER MysqlExe
    mysql.exe 路径。不传则自动在 PATH 与 Program Files 下搜索。

.PARAMETER SkipFirewall
    跳过防火墙检查/修改（无管理员权限时用）。

.PARAMETER WhatIf
    只体检不动手：打印将要执行的每一条操作，不做任何变更。

.EXAMPLE
    # 体检，不做任何修改
    pwsh -File .\Modify-mySQL-Access-Right-20260811.ps1 -WhatIf

.EXAMPLE
    # 按配置文件里的 DB_IP 开通（需管理员权限才能加防火墙规则）
    pwsh -File .\Modify-mySQL-Access-Right-20260811.ps1

.EXAMPLE
    # 限定整个网段，比开 % 安全
    pwsh -File .\Modify-mySQL-Access-Right-20260811.ps1 -AllowHost '192.168.101.%'

.NOTES
    在 MySQL 服务所在的那台机器上运行（本项目里 MySQL 与 Agent 同机）。
    加防火墙规则需要「以管理员身份运行」PowerShell；不加可用 -SkipFirewall。
#>

[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'Medium')]
param(
    [string] $ConfigPath,
    [string] $AllowHost,
    [string] $AdminUser = 'root',
    [string] $AdminPassword,
    [string] $MysqlExe,
    [switch] $SkipFirewall
)

$ErrorActionPreference = 'Stop'

# 让中文在重定向/管道场景下也不乱码（Windows PowerShell 5.1 默认走控制台代码页）
try { [Console]::OutputEncoding = [Text.Encoding]::UTF8 } catch { }

# ---------------------------------------------------------------- 输出helper
function Write-Step  { param([string]$m) Write-Host "`n=== $m ===" -ForegroundColor Cyan }
function Write-Ok    { param([string]$m) Write-Host "  [OK]   $m" -ForegroundColor Green }
function Write-Warn  { param([string]$m) Write-Host "  [警告] $m" -ForegroundColor Yellow }
function Write-Fail  { param([string]$m) Write-Host "  [失败] $m" -ForegroundColor Red }
function Write-Info  { param([string]$m) Write-Host "  $m" }

# ---------------------------------------------------------------- 1) 读配置
function Resolve-ConfigPath {
    param([string] $Explicit)

    if ($Explicit) {
        if (-not (Test-Path -LiteralPath $Explicit)) { throw "配置文件不存在：$Explicit" }
        return (Resolve-Path -LiteralPath $Explicit).Path
    }
    # 本脚本在 <项目根>\database\ 下，配置在 <项目根>\env\
    $dir = Split-Path -Parent $PSCommandPath
    for ($i = 0; $i -lt 6 -and $dir; $i++) {
        $candidate = Join-Path $dir 'env\Presales-AI-Agent-config.json'
        if (Test-Path -LiteralPath $candidate) { return (Resolve-Path -LiteralPath $candidate).Path }
        $dir = Split-Path -Parent $dir
    }
    throw '未找到 env\Presales-AI-Agent-config.json，请用 -ConfigPath 指定。'
}

Write-Step '读取 env 配置'
$cfgPath = Resolve-ConfigPath -Explicit $ConfigPath
$cfg = Get-Content -LiteralPath $cfgPath -Raw -Encoding UTF8 | ConvertFrom-Json
$db = $cfg.DB_Connect
if (-not $db) { throw "配置文件缺少 DB_Connect 节：$cfgPath" }

$dbIp     = $db.DB_IP
$dbPort   = if ($db.DB_Port) { [int]$db.DB_Port } else { 3306 }
$dbUser   = $db.DB_User
$dbPwd    = $db.DB_Password
$dbName   = if ($db.DB_Name) { $db.DB_Name } else { 'SmartLabOS-Presales-AI' }
if (-not $AllowHost)     { $AllowHost = $dbIp }
if (-not $AdminPassword) { $AdminPassword = $dbPwd }

Write-Info "配置文件 : $cfgPath"
Write-Info "目标      : $dbUser@$dbIp`:$dbPort/$dbName"
Write-Info "授权来源  : '$AllowHost'"
if ($AllowHost -eq '%') { Write-Warn "'%' 表示允许任意主机连接。内网可接受，但更稳妥的是限定网段，例如 -AllowHost '192.168.101.%'。" }

# ---------------------------------------------------------------- 2) 定位 mysql.exe
Write-Step '定位 mysql.exe'
function Resolve-MysqlExe {
    param([string] $Explicit)
    if ($Explicit) {
        if (-not (Test-Path -LiteralPath $Explicit)) { throw "mysql.exe 不存在：$Explicit" }
        return (Resolve-Path -LiteralPath $Explicit).Path
    }
    $inPath = Get-Command mysql.exe -ErrorAction SilentlyContinue
    if ($inPath) { return $inPath.Source }

    $found = Get-ChildItem -Path 'C:\Program Files\MySQL', 'C:\Program Files (x86)\MySQL' `
                           -Filter 'mysql.exe' -Recurse -ErrorAction SilentlyContinue |
             Sort-Object FullName -Descending | Select-Object -First 1
    if ($found) { return $found.FullName }
    throw ' 未找到 mysql.exe。请用 -MysqlExe 指定，或把 MySQL 的 bin 目录加入 PATH。'
}
$mysql = Resolve-MysqlExe -Explicit $MysqlExe
Write-Ok "mysql.exe：$mysql"

# 口令走临时 option 文件而非命令行 —— 命令行传 -p 会把口令暴露在进程列表里，
# 且 MySQL 客户端本身也会告警。文件用完即删。
$optFile = Join-Path ([System.IO.Path]::GetTempPath()) ("mysql-opt-" + [guid]::NewGuid().ToString('N') + ".cnf")

# 必须写成**无 BOM** 的 UTF-8：Set-Content -Encoding UTF8 在 PS 5.1 下会写 BOM，
# mysql 客户端会把它当成 [client] 之前的杂字符，报 "option without preceding group"。
function Write-CnfFile {
    param([Parameter(Mandatory)][string] $Path, [Parameter(Mandatory)][string] $Body)
    [System.IO.File]::WriteAllText($Path, $Body, (New-Object System.Text.UTF8Encoding($false)))
}

function Invoke-MySql {
    <#  在 localhost 上以管理员账号执行 SQL，返回 stdout 文本行。 #>
    param([Parameter(Mandatory)][string] $Sql, [switch] $AllowFailure)

    $stdout = & $mysql "--defaults-extra-file=$optFile" '--protocol=TCP' '--host=127.0.0.1' `
                       "--port=$dbPort" '--batch' '--skip-column-names' '--execute' $Sql 2>&1
    if ($LASTEXITCODE -ne 0 -and -not $AllowFailure) {
        throw "SQL 执行失败（退出码 $LASTEXITCODE）：`n$($stdout -join "`n")`nSQL: $Sql"
    }
    return $stdout
}

# ---------------------------------------------------------------- 3) 防火墙
Write-Step "Windows 防火墙：3306/TCP 入站"
if ($SkipFirewall) {
    Write-Info '已按 -SkipFirewall 跳过。'
} else {
    $isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()
               ).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
    $ruleName = "MySQL $dbPort (SmartLabOS Presales AI Agent)"
    $existing = Get-NetFirewallRule -DisplayName $ruleName -ErrorAction SilentlyContinue
    if ($existing) {
        Write-Ok "入站规则已存在：$ruleName"
    } elseif (-not $isAdmin) {
        Write-Warn "未以管理员身份运行，无法新增防火墙规则。若远程连不上，请管理员执行："
        Write-Info  "  New-NetFirewallRule -DisplayName '$ruleName' -Direction Inbound -Protocol TCP -LocalPort $dbPort -Action Allow -Profile Private,Domain"
    } elseif ($PSCmdlet.ShouldProcess($ruleName, '新增 3306/TCP 入站放行规则（Private+Domain）')) {
        New-NetFirewallRule -DisplayName $ruleName -Direction Inbound -Protocol TCP `
                            -LocalPort $dbPort -Action Allow -Profile Private, Domain | Out-Null
        Write-Ok "已新增入站规则：$ruleName（仅 Private/Domain 配置文件，不含 Public）"
    }
}

# ---------------------------------------------------------------- 4) 端口探测
Write-Step "端口连通性探测"
foreach ($target in @('127.0.0.1', $dbIp)) {
    $client = New-Object System.Net.Sockets.TcpClient
    try {
        $ok = $client.ConnectAsync($target, $dbPort).Wait(4000)
        if ($ok) { Write-Ok "$target`:$dbPort 可连接" } else { Write-Fail "$target`:$dbPort 连接超时" }
    } catch { Write-Fail "$target`:$dbPort 连接失败：$($_.Exception.InnerException.Message)" }
    finally { $client.Close() }
}

# ---------------------------------------------------------------- 5) 授权
Write-Step 'MySQL 账号授权'

if (-not $AdminPassword) {
    $sec = Read-Host "请输入 MySQL 管理员 $AdminUser 的口令" -AsSecureString
    $AdminPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
        [Runtime.InteropServices.Marshal]::SecureStringToBSTR($sec))
}

# 临时 option 文件：口令里的 # ; 等字符在 my.cnf 中需用双引号包住
$optBody = @"
[client]
user="$AdminUser"
password="$AdminPassword"
"@

try {
    # 临时文件是内部管线，不是「要确认的变更」：即使 -WhatIf 也要真的写出来，
    # 否则体检用的只读查询都执行不了。（Write-CnfFile 不经过 ShouldProcess。）
    Write-CnfFile -Path $optFile -Body $optBody
    # 只留当前用户可读，避免同机其他账号顺手拿走口令
    $acl = Get-Acl -LiteralPath $optFile
    $acl.SetAccessRuleProtection($true, $false)
    $acl.SetAccessRule((New-Object System.Security.AccessControl.FileSystemAccessRule(
        [Security.Principal.WindowsIdentity]::GetCurrent().Name, 'FullControl', 'Allow')))
    Set-Acl -LiteralPath $optFile -AclObject $acl -WhatIf:$false -Confirm:$false

    # --- 5.1 先确认管理员账号能连上 ---
    $ver = Invoke-MySql -Sql 'SELECT VERSION();'
    Write-Ok "已以 $AdminUser 连接本机 MySQL，服务端版本 $ver"

    # --- 5.2 现状：该账号目前对哪些来源主机开放 ---
    $hosts = Invoke-MySql -Sql "SELECT host FROM mysql.user WHERE user='$dbUser' ORDER BY host;"
    Write-Info "账号 '$dbUser' 当前已授权的来源主机：$(if ($hosts) { ($hosts -join ', ') } else { '（无）' })"
    if ($hosts -contains $AllowHost) {
        Write-Ok "'$dbUser'@'$AllowHost' 已存在，本次只补齐库权限。"
    }

    # --- 5.3 bind-address 检查：只听 127.0.0.1 时，授权再全也连不进来 ---
    $bind = (Invoke-MySql -Sql "SHOW VARIABLES LIKE 'bind_address';" -AllowFailure) -join ' '
    Write-Info "服务端 bind_address：$bind"
    if ($bind -match '127\.0\.0\.1') {
        Write-Warn 'MySQL 只监听 127.0.0.1，远程/本机 IP 均连不进来。请修改 my.ini：'
        Write-Info  '    [mysqld]'
        Write-Info  '    bind-address = 0.0.0.0'
        Write-Info  '  然后重启服务： Restart-Service MySQL84   （服务名以本机实际为准）'
    }

    # --- 5.4 建账号 + 授权 ---
    #   口令用 MySQL 自己的引号规则转义（单引号加倍），不做字符串拼接的想当然处理。
    $pwdSql  = $dbPwd -replace "'", "''"
    $userSql = $dbUser -replace "'", "''"
    $hostSql = $AllowHost -replace "'", "''"
    $dbSql   = $dbName -replace '`', '``'

    $statements = @(
        "CREATE USER IF NOT EXISTS '$userSql'@'$hostSql' IDENTIFIED BY '$pwdSql';",
        "ALTER USER '$userSql'@'$hostSql' IDENTIFIED BY '$pwdSql';",
        "GRANT ALL PRIVILEGES ON ``$dbSql``.* TO '$userSql'@'$hostSql';",
        "FLUSH PRIVILEGES;"
    )

    # root 通常还需要全局权限才能建库/改表；仅业务账号则不必放这么大。
    if ($dbUser -ieq 'root') {
        $statements = $statements[0..2] + @(
            "GRANT ALL PRIVILEGES ON *.* TO '$userSql'@'$hostSql' WITH GRANT OPTION;",
            "FLUSH PRIVILEGES;"
        )
    }

    # 回显一律脱敏：IDENTIFIED BY 的明文口令不该出现在控制台、日志或 -WhatIf 输出里
    function Hide-Secret { param([string] $s) $s -replace "IDENTIFIED BY '.*?'", "IDENTIFIED BY '********'" }

    foreach ($sql in $statements) {
        $shown = Hide-Secret $sql
        if ($PSCmdlet.ShouldProcess('MySQL', $shown)) {
            Invoke-MySql -Sql $sql | Out-Null
            Write-Ok $shown
        } else {
            Write-Info "[WhatIf] $shown"
        }
    }

    if ($dbUser -ieq 'root') {
        Write-Warn "已给 root@'$AllowHost' 全库权限（与 root@localhost 对齐，程序需要建库建表）。"
        Write-Info  "若只想给最小权限，可改用专用账号：把 env 配置的 DB_User 改成业务账号后重跑本脚本，"
        Write-Info  "  届时只会授予 ``$dbName`` 单库权限。"
    }

    # --- 5.5 验证：真的用程序那套参数连一次 ---
    if (-not $WhatIfPreference) {
        Write-Step '验证：按 env 配置的参数实连一次'
        $verifyOpt = Join-Path ([System.IO.Path]::GetTempPath()) ("mysql-vfy-" + [guid]::NewGuid().ToString('N') + ".cnf")
        try {
            Write-CnfFile -Path $verifyOpt -Body @"
[client]
user="$dbUser"
password="$dbPwd"
"@
            $out = & $mysql "--defaults-extra-file=$verifyOpt" '--protocol=TCP' "--host=$dbIp" `
                            "--port=$dbPort" '--batch' '--skip-column-names' `
                            '--execute' "SELECT CONCAT(CURRENT_USER(), ' | ', DATABASE());" "$dbName" 2>&1
            if ($LASTEXITCODE -eq 0) {
                Write-Ok "连接成功：$out"
                Write-Info '现在重启 Agent，启动日志应显示「MySQL 连通性：OK」。'
            } else {
                Write-Fail "仍连不上：`n$($out -join "`n")"
                Write-Info '排查顺序：① bind_address 是否已改并重启服务；② 防火墙是否放行；'
                Write-Info '          ③ 授权主机是否与服务端看到的来源一致（SELECT USER(); 里 @ 后面那段）。'
            }
        } finally {
            Remove-Item -LiteralPath $verifyOpt -Force -ErrorAction SilentlyContinue -WhatIf:$false -Confirm:$false
        }
    }
}
finally {
    Remove-Item -LiteralPath $optFile -Force -ErrorAction SilentlyContinue -WhatIf:$false -Confirm:$false
}

Write-Step '完成'
