# SmartLabOS DeepSeek Presales AI Agent - 部署指南（2026-08-17）

## 概述

本指南说明如何在新的 **Windows 11 Enterprise** 电脑上部署已编译的 DeepSeek Presales AI Agent 应用。

该过程完全自动化，通过 PowerShell 脚本处理：
- MySQL 数据库初始化
- IIS 环境配置
- 应用程序发布
- 网络绑定与权限配置

---

## 前置要求

### 在源机器（编译机）上

✅ **已完成**（您当前机器）：
- ✓ 项目已编译：`c:\TestClaude\SmartLabOS-AI-Assistant\DeepSeek-Presales-AI-Agent\IIS-Publish\`
- ✓ 部署脚本已生成：`Publish-DeepSeek-AI-Agent-20260817.ps1`
- ✓ 配置文件已准备：`Presales-AI-Agent-config.json`

### 在目标机器（新 PC）上

需要提前准备：

1. **Windows 11 Enterprise** 已安装
2. **MySQL Server** 已安装（或脚本将尝试安装，需要 MySQL 安装包）
3. **PowerShell 5.0+**（Windows 11 默认已装）
4. **管理员权限**（运行脚本时需要）
5. **网络连接**（部分功能需要联网）

---

## 部署步骤

### 第一步：在源机器上准备发布包

**已完成** - 编译好的文件位置：
```
c:\TestClaude\SmartLabOS-AI-Assistant\DeepSeek-Presales-AI-Agent\IIS-Publish\
```

该目录包含以下内容：
```
IIS-Publish/
├── Presales.Proposal.AI.Agent.dll      (主程序集)
├── Presales.Proposal.AI.Agent.exe      (可执行文件)
├── appsettings.json                     (应用设置)
├── appsettings.Development.json         (开发设置)
├── Presales-AI-Agent-config.json        (知识库和DeepSeek配置)
├── web.config                           (IIS配置)
├── wwwroot/                             (前端资源)
└── [其他依赖库和配置文件]
```

### 第二步：复制文件到新 PC

在新 PC 上创建临时目录并复制发布包：

```powershell
# 在新PC上创建临时目录（例如D盘）
mkdir D:\Deploy-Temp
# 或您选择的其他位置

# 从源机器复制发布包（网络共享或U盘）
# 确保整个 IIS-Publish 目录被复制到 D:\Deploy-Temp\
```

同时复制部署脚本：
```powershell
# 复制 Publish-DeepSeek-AI-Agent-20260817.ps1 到同一位置
# 例如: D:\Deploy-Temp\Publish-DeepSeek-AI-Agent-20260817.ps1
```

### 第三步：准备 SQL 脚本

数据库脚本位置：
```
c:\TestClaude\SmartLabOS-AI-Assistant\DeepSeek-Presales-AI-Agent\database\01-presales-schema.sql
```

**选项 A**（推荐）：在新 PC 上也有此文件的副本
```powershell
# 复制数据库脚本到新PC的同一相对位置
```

**选项 B**：在脚本参数中指定 SQL 脚本的完整路径

### 第四步：准备知识库文件

应用需要以下知识库目录（在新 PC 上）：

```
C:\SmartLabOS-References\
├── 01-modules\
├── 02-platforms\
├── 06-pallet\
├── potocol\MD\
├── projects\
├── 09-ProposalTemplate\
└── _templates\
```

**复制方法**：
```powershell
# 将源机器的知识库目录复制到新PC
# 源路径: C:\TestClaude\SmartLabOS-AI-Assistant\references\
# 目标路径: C:\SmartLabOS-References\
```

### 第五步：在新 PC 上运行部署脚本

**以管理员身份打开 PowerShell**：

```powershell
# 1. 进入脚本所在目录
cd D:\Deploy-Temp

# 2. 允许脚本执行
Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process

# 3. 运行部署脚本（基础参数）
.\Publish-DeepSeek-AI-Agent-20260817.ps1 `
  -SourcePublishDir "D:\Deploy-Temp\IIS-Publish" `
  -SqlScriptPath "D:\Deploy-Temp\01-presales-schema.sql"

# 或使用完整参数（自定义路径和配置）：
.\Publish-DeepSeek-AI-Agent-20260817.ps1 `
  -SourcePublishDir "D:\Deploy-Temp\IIS-Publish" `
  -SqlScriptPath "D:\Deploy-Temp\01-presales-schema.sql" `
  -TargetAppDir "C:\inetpub\DeepSeek-Presales-AI-Agent" `
  -IISSiteName "DeepSeek-Presales-AI-Agent" `
  -IISAppPoolName "DeepSeekPresalesAIAgent" `
  -HttpPort 80 `
  -MySqlServer "127.0.0.1" `
  -MySqlPort 3306 `
  -MySqlUser "root" `
  -MySqlPassword "PUT-DB-PASSWORD-HERE" `
  -MySqlDatabase "SmartLabOS-Presales-AI" `
  -DeepSeekApiKey "PUT-DEEPSEEK-API-KEY-HERE" `
  -ReferenceBasePath "C:\SmartLabOS-References"
```

#### 脚本参数说明

| 参数 | 说明 | 默认值 |
|------|------|--------|
| `SourcePublishDir` | 源发布目录（必需） | - |
| `SqlScriptPath` | SQL脚本路径（必需） | - |
| `TargetAppDir` | 目标应用目录 | `C:\inetpub\DeepSeek-Presales-AI-Agent` |
| `IISSiteName` | IIS网站名称 | `DeepSeek-Presales-AI-Agent` |
| `IISAppPoolName` | IIS应用池名称 | `DeepSeekPresalesAIAgent` |
| `HttpPort` | HTTP绑定端口 | `80` |
| `MySqlServer` | MySQL服务器地址 | `127.0.0.1` |
| `MySqlPort` | MySQL端口 | `3306` |
| `MySqlUser` | MySQL用户名 | `root` |
| `MySqlPassword` | MySQL密码 | `PUT-DB-PASSWORD-HERE` |
| `MySqlDatabase` | 数据库名称 | `SmartLabOS-Presales-AI` |
| `DeepSeekApiKey` | DeepSeek API密钥 | `PUT-DEEPSEEK-API-KEY-HERE` |
| `ReferenceBasePath` | 知识库根目录 | `C:\SmartLabOS-References` |

### 第六步：验证部署

脚本执行完毕后，验证以下项目：

#### 1. IIS 验证
```powershell
# 打开 IIS 管理器
inetmgr

# 检查：
# - 应用池 "DeepSeekPresalesAIAgent" 是否存在且运行中
# - 网站 "DeepSeek-Presales-AI-Agent" 是否存在且运行中
# - 绑定是否为 http://*:80
```

#### 2. 数据库验证
```powershell
# 连接到MySQL验证数据库
mysql -h 127.0.0.1 -u root -pPUT-DB-PASSWORD-HERE -e "USE `SmartLabOS-Presales-AI`; SHOW TABLES;"

# 应该看到两个表：
# - SmartLabOS-PresalesProject
# - SmartLabOS-PresalesGeneration
```

#### 3. 应用验证

**在浏览器中访问**：
```
http://localhost/
或
http://<新PC的IP地址>/
```

应该看到 Swagger UI 或应用首页。

#### 4. API 测试

```powershell
# 测试元数据端点
Invoke-WebRequest -Uri "http://localhost/api/presales/config" -Method GET | ConvertFrom-Json

# 应返回配置信息，包括：
# - 数据库连接状态
# - DeepSeek 配置
# - 知识库路径验证
```

---

## 常见问题排查

### 问题 1: PowerShell 脚本执行被拒绝

**症状**：
```
...ps1 cannot be loaded because running scripts is disabled on this system
```

**解决方案**：
```powershell
Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process
# 然后重新运行脚本
```

### 问题 2: MySQL 连接失败

**症状**：
```
HTTP 503: Service Unavailable
或
Database connection failed
```

**排查步骤**：
```powershell
# 1. 检查MySQL服务是否运行
Get-Service -Name MySQL80 | Start-Service

# 2. 测试连接
mysql -h 127.0.0.1 -u root -pPUT-DB-PASSWORD-HERE

# 3. 验证数据库是否已创建
mysql -e "SHOW DATABASES;" -u root -pPUT-DB-PASSWORD-HERE

# 4. 查看应用日志
# 位置: C:\inetpub\DeepSeek-Presales-AI-Agent\
# 或: C:\inetpub\logs\LogFiles\
```

### 问题 3: IIS 显示 502 Bad Gateway

**症状**：
```
HTTP 502: Bad Gateway
应用池进程已停止
```

**排查步骤**：

1. **检查应用池状态**：
```powershell
# 在IIS管理器中右键应用池 → 查看事件
# 或查看事件日志：
Get-WinEvent -LogName "Application" -MaxEvents 20 | Format-Table
```

2. **检查 .NET Runtime**：
```powershell
# 验证 ASP.NET Core Hosting Bundle 是否已安装
Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*" | 
  Where-Object { $_.DisplayName -like "*ASP.NET Core*" }

# 如果缺失，从此处下载：
# https://dotnet.microsoft.com/download/dotnet (选择 .NET 10.0)
```

3. **检查配置文件**：
```powershell
# 验证 appsettings.json 格式是否正确
Test-Json -Path "C:\inetpub\DeepSeek-Presales-AI-Agent\appsettings.json"

# 验证 Presales-AI-Agent-config.json 格式是否正确
Test-Json -Path "C:\inetpub\DeepSeek-Presales-AI-Agent\Presales-AI-Agent-config.json"
```

### 问题 4: 知识库路径错误

**症状**：
```
GET /api/presales/config 返回：
"error": "Knowledge base path not found: C:\SmartLabOS-References\01-modules"
```

**解决方案**：

1. **验证知识库目录结构**：
```powershell
# 检查目录是否存在
Test-Path "C:\SmartLabOS-References\01-modules"
Test-Path "C:\SmartLabOS-References\02-platforms"
# 等等...
```

2. **重新复制知识库文件**：
```powershell
# 从源机器复制
Copy-Item "C:\TestClaude\SmartLabOS-AI-Assistant\references\*" `
          "C:\SmartLabOS-References\" `
          -Recurse -Force
```

3. **更新配置文件**：
```powershell
# 编辑 C:\inetpub\DeepSeek-Presales-AI-Agent\Presales-AI-Agent-config.json
# 更新所有路径指向正确的位置
```

### 问题 5: DeepSeek API 密钥错误

**症状**：
```
HTTP 401: Unauthorized
错误: "Authentication Failed"
```

**排查步骤**：
```powershell
# 1. 验证密钥格式
# DeepSeek 密钥应以 sk-xxx 开头

# 2. 检查配置文件中的密钥
# 编辑: C:\inetpub\DeepSeek-Presales-AI-Agent\Presales-AI-Agent-config.json
# 检查: "DeepSeek_API_Key" 字段

# 3. 重启应用池
Stop-IISAppPool -Name "DeepSeekPresalesAIAgent"
Start-IISAppPool -Name "DeepSeekPresalesAIAgent"
```

---

## 部署日志

脚本执行的完整日志保存在：
```
C:\inetpub\DeepSeek-Presales-AI-Agent\deployment-yyyyMMdd-HHmmss.log
```

**查看日志**：
```powershell
Get-Content "C:\inetpub\DeepSeek-Presales-AI-Agent\deployment-*.log" -Tail 50
```

---

## 更新与回退

### 更新应用

如果需要更新已部署的应用：

```powershell
# 1. 重新编译（在源机器上）
cd "c:\TestClaude\SmartLabOS-AI-Assistant\DeepSeek-Presales-AI-Agent"
dotnet publish -c Release -o "c:\TestClaude\SmartLabOS-AI-Assistant\DeepSeek-Presales-AI-Agent\IIS-Publish" --self-contained false

# 2. 停止应用池
Stop-IISAppPool -Name "DeepSeekPresalesAIAgent"

# 3. 复制新文件到目标目录（保留配置文件）
Copy-Item "c:\...\IIS-Publish\*.dll" -Destination "C:\inetpub\DeepSeek-Presales-AI-Agent\" -Force
Copy-Item "c:\...\IIS-Publish\*.exe" -Destination "C:\inetpub\DeepSeek-Presales-AI-Agent\" -Force
Copy-Item "c:\...\IIS-Publish\*.json" -Destination "C:\inetpub\DeepSeek-Presales-AI-Agent\" -Force

# 4. 启动应用池
Start-IISAppPool -Name "DeepSeekPresalesAIAgent"
```

### 回退到上一个版本

脚本会自动备份现有文件：

```powershell
# 查看备份目录
Get-ChildItem -Path "C:\inetpub\" | Where-Object Name -like "*backup*"

# 恢复备份（如需要）
Remove-Item "C:\inetpub\DeepSeek-Presales-AI-Agent" -Recurse
Rename-Item "C:\inetpub\DeepSeek-Presales-AI-Agent.backup.*" -NewName "C:\inetpub\DeepSeek-Presales-AI-Agent"
```

---

## 性能调优（可选）

部署完成后，可根据实际需求优化：

```powershell
# 1. 调整应用池回收策略（在IIS管理器中）
# 应用池 → 高级设置 → 回收配置

# 2. 配置应用启动预加载
# 网站 → 编辑绑定 → IIS 启用预加载

# 3. 优化MySQL连接池
# 编辑 appsettings.json 中的 ConnectionStrings
```

---

## 支持与反馈

如遇到问题：

1. **检查日志**：`C:\inetpub\DeepSeek-Presales-AI-Agent\deployment-*.log`
2. **查看事件查看器**：`eventvwr.msc` → Windows日志 → 应用程序
3. **检查IIS日志**：`C:\inetpub\logs\LogFiles\`
4. **参考原始文档**：`Documentation\Modification-Summary-20260815.md`

---

## 版本信息

| 项目 | 版本 | 说明 |
|------|------|------|
| 部署日期 | 2026-08-17 | 本指南生成日期 |
| 后端框架 | .NET 10.0 | ASP.NET Core Web API |
| 数据库 | MySQL 8.0+ | utf8mb4 编码 |
| 大模型 | DeepSeek V4 Pro | Anthropic 兼容端点 |
| IIS 版本 | Windows 11 IIS | 10.0+ |

---

**祝部署顺利！** 🚀
