# 部署清单 - SmartLabOS DeepSeek Presales AI Agent (2026-08-17)

## 概述

本清单列出了从源机器（编译机）复制到新 Windows 11 Enterprise PC 上所需的所有文件和目录。

---

## 文件清单

### 第一部分：发布的应用程序文件

**源位置**：
```
c:\TestClaude\SmartLabOS-AI-Assistant\DeepSeek-Presales-AI-Agent\IIS-Publish\
```

**目标位置**（新PC）**：
```
D:\Deploy-Temp\IIS-Publish\    (或您选择的临时目录)
```

**包含内容**（完整列表）：

```
IIS-Publish/
├── Presales.Proposal.AI.Agent.dll          ✓ 主程序集（必需）
├── Presales.Proposal.AI.Agent.exe          ✓ 可执行文件（必需）
├── Presales.Proposal.AI.Agent.deps.json    ✓ 依赖描述（必需）
├── Presales.Proposal.AI.Agent.runtimeconfig.json  ✓ 运行时配置（必需）
├── Presales.Proposal.AI.Agent.pdb          ✓ 调试符号（可选，便于故障诊断）
├── Presales-AI-Agent-config.json           ✓ 知识库配置（必需，会被脚本覆盖）
├── appsettings.json                        ✓ 应用设置（必需，会被脚本覆盖）
├── appsettings.Development.json            ✓ 开发环境配置（可选）
├── web.config                              ✓ IIS配置（必需）
├── Presales.Proposal.AI.Agent.staticwebassets.endpoints.json  ✓ 静态资产（必需）
├── Presales.Proposal.AI.Agent.staticwebassets.runtime.json    ✓ 静态资产（必需）
│
├── 【依赖库 - 必需】
├── Anthropic.dll                           ✓ Anthropic SDK v12.34.1
├── Dapper.dll                              ✓ ORM 框架
├── DocumentFormat.OpenXml.dll              ✓ Office Open XML
├── DocumentFormat.OpenXml.Framework.dll    ✓ Office Open XML Framework
├── Microsoft.AspNetCore.OpenApi.dll        ✓ OpenAPI 支持
├── Microsoft.Extensions.AI.Abstractions.dll ✓ AI 扩展
├── Microsoft.OpenApi.dll                   ✓ OpenAPI 规范
├── MySqlConnector.dll                      ✓ MySQL 数据库驱动
├── System.IO.Packaging.dll                 ✓ 系统库
├── Swashbuckle.AspNetCore.Swagger.dll      ✓ Swagger API 文档
├── Swashbuckle.AspNetCore.SwaggerGen.dll   ✓ Swagger 生成器
├── Swashbuckle.AspNetCore.SwaggerUI.dll    ✓ Swagger UI
│
└── wwwroot/                                ✓ 前端资源（必需）
    ├── index.html                          ✓ 首页
    ├── css/
    │   └── style.css                       ✓ 样式表
    └── js/
        ├── app.js                          ✓ 应用脚本
        └── presales.js                     ✓ 业务脚本
```

**复制命令**（从源机器）：

```powershell
# 使用 Robocopy 进行可靠的复制
robocopy "c:\TestClaude\SmartLabOS-AI-Assistant\DeepSeek-Presales-AI-Agent\IIS-Publish" `
         "D:\Deploy-Temp\IIS-Publish" /E /COPY:DAT

# 或使用 Copy-Item
Copy-Item -Path "c:\TestClaude\SmartLabOS-AI-Assistant\DeepSeek-Presales-AI-Agent\IIS-Publish\*" `
          -Destination "D:\Deploy-Temp\IIS-Publish" -Recurse -Force
```

**验证**（新PC）：

```powershell
# 确认所有文件已复制
Get-ChildItem "D:\Deploy-Temp\IIS-Publish" -Recurse | Measure-Object

# 应该看到 50+ 个文件（具体数量取决于依赖版本）
```

---

### 第二部分：数据库初始化脚本

**源位置**：
```
c:\TestClaude\SmartLabOS-AI-Assistant\DeepSeek-Presales-AI-Agent\database\01-presales-schema.sql
```

**目标位置**（新PC）：
```
D:\Deploy-Temp\01-presales-schema.sql
```

**内容**：
```sql
-- 数据库创建与表初始化脚本
-- 包含两个表：
-- 1. SmartLabOS-PresalesProject    (售前项目需求)
-- 2. SmartLabOS-PresalesGeneration (生成执行记录)
```

**复制命令**：

```powershell
Copy-Item -Path "c:\TestClaude\SmartLabOS-AI-Assistant\DeepSeek-Presales-AI-Agent\database\01-presales-schema.sql" `
          -Destination "D:\Deploy-Temp\01-presales-schema.sql"
```

---

### 第三部分：部署脚本

**源位置**：
```
c:\TestClaude\SmartLabOS-AI-Assistant\DeepSeek-Presales-AI-Agent\Documentation\
```

**目标位置**（新PC）：
```
D:\Deploy-Temp\                    (与 IIS-Publish 同级)
```

**需要复制的脚本**：

| 文件 | 说明 | 必需 |
|------|------|------|
| `Publish-DeepSeek-AI-Agent-20260817.ps1` | 主部署脚本 | ✅ 是 |
| `Quick-Deploy.ps1` | 快速部署启动器（交互式） | ⭐ 推荐 |
| `DEPLOYMENT-GUIDE-20260817.md` | 部署指南文档 | 📖 参考 |

**复制命令**：

```powershell
# 复制脚本
Copy-Item "c:\TestClaude\SmartLabOS-AI-Assistant\DeepSeek-Presales-AI-Agent\Documentation\Publish-DeepSeek-AI-Agent-20260817.ps1" -Destination "D:\Deploy-Temp\"
Copy-Item "c:\TestClaude\SmartLabOS-AI-Assistant\DeepSeek-Presales-AI-Agent\Documentation\Quick-Deploy.ps1" -Destination "D:\Deploy-Temp\"
Copy-Item "c:\TestClaude\SmartLabOS-AI-Assistant\DeepSeek-Presales-AI-Agent\Documentation\DEPLOYMENT-GUIDE-20260817.md" -Destination "D:\Deploy-Temp\"
```

---

### 第四部分：知识库文件（重要！）

这些文件在新 PC 上 **不能与应用同级**，需要单独部署。

**源位置**：
```
C:\TestClaude\SmartLabOS-AI-Assistant\references\
```

**目标位置**（新PC）：
```
C:\SmartLabOS-References\          (或 -ReferenceBasePath 参数指定的位置)
```

**完整的目录结构**：

```
SmartLabOS-References/
├── 01-modules/                     ✓ 模块定义和配置（必需）
│   ├── *.json, *.yaml              (模块定义文件)
│   └── ...
├── 02-platforms/                   ✓ 平台配置（必需）
│   ├── *.json, *.yaml              (平台定义)
│   └── ...
├── 06-pallet/                      ✓ 调色板配置（可选）
│   └── ...
├── potocol/MD/                     ✓ 流程标准文档（必需）
│   ├── *.md                        (Markdown 格式)
│   └── ...
├── projects/                       ✓ 项目输出目录（必需，将被写入）
│   └── (空目录或现有项目)
├── 09-ProposalTemplate/            ✓ 提案模版（必需）
│   ├── *.html, *.md, *.docx
│   └── ...
└── _templates/                     ✓ 标准模版（必需，需可写）
    ├── *.md
    ├── *.html
    └── reference.docx              (将被生成)
```

**复制命令**（大量数据，可能耗时）：

```powershell
# 使用 Robocopy 以获得最佳性能和可靠性
robocopy "C:\TestClaude\SmartLabOS-AI-Assistant\references" `
         "C:\SmartLabOS-References" `
         /E /COPY:DAT /R:3 /W:10 /LOG:"C:\robocopy-references.log"

# 等待完成（可能需要几分钟到十几分钟，取决于文件大小）
```

**验证**（新PC）：

```powershell
# 检查目录结构
Get-ChildItem "C:\SmartLabOS-References" -Directory

# 应该看到: 01-modules, 02-platforms, 06-pallet, potocol, projects, 09-ProposalTemplate, _templates

# 检查文件数量
(Get-ChildItem "C:\SmartLabOS-References" -Recurse -File | Measure-Object).Count

# 应该是几百个文件
```

---

## 完整部署包结构（新PC）

部署完成后，新 PC 上应该有以下目录结构：

```
D:\Deploy-Temp\                            (临时部署目录)
├── IIS-Publish/                           (已编译的应用)
│   ├── Presales.Proposal.AI.Agent.dll
│   ├── *.dll                              (所有依赖)
│   ├── appsettings.json
│   ├── Presales-AI-Agent-config.json
│   ├── web.config
│   └── wwwroot/
├── 01-presales-schema.sql                 (数据库脚本)
├── Publish-DeepSeek-AI-Agent-20260817.ps1 (主部署脚本)
├── Quick-Deploy.ps1                       (快速启动脚本)
└── DEPLOYMENT-GUIDE-20260817.md           (部署指南)

C:\SmartLabOS-References\                  (知识库文件)
├── 01-modules/
├── 02-platforms/
├── 06-pallet/
├── potocol/MD/
├── projects/
├── 09-ProposalTemplate/
└── _templates/

C:\inetpub\DeepSeek-Presales-AI-Agent\     (应用部署后)
├── (IIS-Publish 的所有内容)
├── Presales-AI-Agent-config.json          (已配置)
├── appsettings.json                       (已配置)
└── deployment-*.log                       (部署日志)
```

---

## 复制步骤总结

### 步骤 1：准备源数据（在源机器上）

```powershell
# 1. 确保已编译发布
cd "c:\TestClaude\SmartLabOS-AI-Assistant\DeepSeek-Presales-AI-Agent"
dotnet publish -c Release -o "IIS-Publish" --self-contained false

# 2. 创建部署包收集目录
$deployPackage = "C:\Temp\DeepSeek-Presales-Deploy-Package"
mkdir $deployPackage

# 3. 复制应用文件
Copy-Item ".\IIS-Publish\*" "$deployPackage\IIS-Publish" -Recurse -Force

# 4. 复制 SQL 脚本
Copy-Item ".\database\01-presales-schema.sql" "$deployPackage\"

# 5. 复制部署脚本
Copy-Item ".\Documentation\Publish-DeepSeek-AI-Agent-20260817.ps1" "$deployPackage\"
Copy-Item ".\Documentation\Quick-Deploy.ps1" "$deployPackage\"
Copy-Item ".\Documentation\DEPLOYMENT-GUIDE-20260817.md" "$deployPackage\"

# 6. 复制知识库文件
Copy-Item "..\references\*" "$deployPackage\references" -Recurse -Force

# 7. 打包为 ZIP（便于传输）
Compress-Archive -Path "$deployPackage\*" -DestinationPath "$deployPackage.zip"
```

### 步骤 2：传输到新 PC

**选项 A：网络共享**
```powershell
# 在源机器上共享
New-SmbShare -Name "DeepSeekDeploy" -Path "C:\Temp\DeepSeek-Presales-Deploy-Package" -FullAccess "Everyone"

# 在新PC上映射网络驱动器并复制
# 或直接访问: \\<源机器IP>\DeepSeekDeploy
```

**选项 B：U盘或移动硬盘**
```
直接复制 C:\Temp\DeepSeek-Presales-Deploy-Package.zip 到 U盘
然后在新PC上解压到 D:\Deploy-Temp\
```

**选项 C：云存储（OneDrive/SharePoint）**
```
上传部署包到企业云存储
在新PC上下载并解压
```

### 步骤 3：在新 PC 上执行部署

```powershell
# 1. 解压部署包（如使用 ZIP）
$packagePath = "D:\Deploy-Temp"
Expand-Archive "C:\Downloads\DeepSeek-Presales-Deploy-Package.zip" -DestinationPath $packagePath

# 2. 进入部署目录
cd $packagePath

# 3. 允许脚本执行
Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process

# 4. 运行快速部署启动器（推荐）
.\Quick-Deploy.ps1

# 或直接运行主部署脚本（指定所有参数）
.\Publish-DeepSeek-AI-Agent-20260817.ps1 `
  -SourcePublishDir "D:\Deploy-Temp\IIS-Publish" `
  -SqlScriptPath "D:\Deploy-Temp\01-presales-schema.sql" `
  -ReferenceBasePath "C:\SmartLabOS-References"
```

---

## 文件大小预估

| 项目 | 大小 | 说明 |
|------|------|------|
| IIS-Publish | ~150-200 MB | 应用及依赖库 |
| 01-presales-schema.sql | ~50 KB | 数据库脚本 |
| 部署脚本（总计） | ~200 KB | PS1 文件 |
| 知识库文件 | ~500 MB - 2 GB | 取决于模块和文档数量 |
| **总计** | **~1-3 GB** | 因模块内容而异 |

---

## 验证清单

### 复制前验证（源机器）

- [ ] `IIS-Publish` 目录存在且包含 DLL 文件
- [ ] `01-presales-schema.sql` 文件存在
- [ ] `Publish-DeepSeek-AI-Agent-20260817.ps1` 存在
- [ ] `Quick-Deploy.ps1` 存在
- [ ] `references` 目录包含所有子目录
- [ ] 所有文件总大小在预期范围内

### 复制后验证（新PC）

- [ ] `D:\Deploy-Temp\IIS-Publish` 目录包含所有应用文件
- [ ] `D:\Deploy-Temp\01-presales-schema.sql` 文件存在
- [ ] `D:\Deploy-Temp\Publish-DeepSeek-AI-Agent-20260817.ps1` 存在
- [ ] `D:\Deploy-Temp\Quick-Deploy.ps1` 存在
- [ ] `C:\SmartLabOS-References\01-modules` 目录非空
- [ ] `C:\SmartLabOS-References\02-platforms` 目录非空
- [ ] `C:\SmartLabOS-References\potocol\MD` 目录非空
- [ ] 所有目录的文件数量与源机器一致

### 部署后验证（新PC）

- [ ] `C:\inetpub\DeepSeek-Presales-AI-Agent` 目录存在
- [ ] IIS 应用池 `DeepSeekPresalesAIAgent` 已创建且运行中
- [ ] IIS 网站 `DeepSeek-Presales-AI-Agent` 已创建
- [ ] 浏览器能访问 `http://localhost/`
- [ ] MySQL 数据库 `SmartLabOS-Presales-AI` 存在且有表
- [ ] `deployment-*.log` 文件显示所有步骤成功

---

## 常见问题

### Q: 可以将知识库放在其他位置吗？

**A**: 可以。在运行部署脚本时使用 `-ReferenceBasePath` 参数指定即可：

```powershell
.\Publish-DeepSeek-AI-Agent-20260817.ps1 `
  ... `
  -ReferenceBasePath "E:\SmartLabOS-Knowledge-Base"
```

### Q: 发布目录中哪些文件是必需的？

**A**: 最少需要：
- `Presales.Proposal.AI.Agent.dll`
- `web.config`
- `appsettings.json`
- 所有 `.dll` 依赖库
- `wwwroot/` 目录

### Q: 如果复制过程中断了怎么办？

**A**: 重新运行复制命令。 Robocopy 支持断点续传：

```powershell
robocopy ... /Z  # /Z 启用断点续传
```

### Q: 新 PC 上一定需要 MySQL 本地服务吗？

**A**: 不一定。`MySqlServer` 参数可以指向任何可访问的 MySQL 服务器。但最简单的方式是在新 PC 上安装本地 MySQL。

---

## 更新与维护

### 更新应用

```powershell
# 在源机器重新编译
dotnet publish -c Release -o "IIS-Publish" --self-contained false

# 复制新的 DLL 和相关文件到新 PC
# 保留配置文件（appsettings.json, Presales-AI-Agent-config.json）
```

### 备份知识库

```powershell
# 新 PC 上定期备份
Compress-Archive -Path "C:\SmartLabOS-References\*" `
                 -DestinationPath "C:\Backups\SmartLabOS-References-$(Get-Date -Format 'yyyyMMdd-HHmmss').zip"
```

---

## 支持与反馈

如有问题，请参考：
- 📖 [部署指南](./DEPLOYMENT-GUIDE-20260817.md)
- 📋 [系统日志](file:///C:/inetpub/logs/LogFiles/)
- 💬 原始迁移文档：`Documentation/Modification-Summary-20260815.md`

---

**版本**: 2026-08-17  
**生成人**: Deployment Automation  
**最后更新**: 2026-08-17
