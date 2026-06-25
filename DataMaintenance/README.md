# SmartLabOS-Presales-AI 主数据维护系统

对 SmartLabOS 售前 AI 知识库的五类主数据（模块 / 平台基类 / 工作站 / 解决方案 / 项目）
提供 **增删改查（CRUD）** 的全栈程序。

| 层 | 技术 |
|----|------|
| 前端 | 原生 HTML + CSS + JavaScript（零依赖、由后端元数据驱动） |
| 后端 | C# 12 + ASP.NET Core Web API（.NET 8.0），RESTful |
| 数据访问 | Dapper + MySqlConnector（参数化、防注入） |
| 数据库 | MySQL 8.x（数据库名 `SmartLabOS-Presales-AI`） |
| 导入器 | Python（pyyaml + pymysql），把 `references/*.md` 卡片导入 MySQL |
| 开发 | Windows 11 Enterprise + Visual Studio 2026 |
| 部署 | Windows Server 2025 DataCenter + IIS（.NET 8 Hosting Bundle） |

---

## 目录结构

```
DataMaintenance/
├─ README.md                         本文档
├─ db.config.json                    ★ MySQL 连接配置（供 Python 导入器；独立于程序）
├─ SmartLabOS.DataMaintenance.sln    解决方案（VS 2026 直接打开）
├─ database/
│   ├─ 01-schema.sql                 建库 + 5 张表
│   └─ 02-verify.sql                 导入结果校验
├─ tools/
│   └─ import_data.py                YAML-MD → MySQL 导入器
└─ src/
    └─ SmartLabOS.DataMaintenance.Api/
        ├─ SmartLabOS.DataMaintenance.Api.csproj
        ├─ Program.cs                启动/中间件
        ├─ appsettings.json          ★ 后端 MySQL 连接字符串（独立于程序）
        ├─ web.config                IIS 托管配置
        ├─ Data/                     元数据注册表 + 通用仓储
        ├─ Controllers/              Meta + 通用 CRUD 控制器
        ├─ Properties/PublishProfiles/FolderProfile.pubxml
        └─ wwwroot/                  前端（index.html / css / js）
```

> ★ **连接独立可配置**：MySQL 连接信息不写死在代码里。
> Python 导入器读 `db.config.json`；后端 API 读 `appsettings.json` 的
> `ConnectionStrings:SmartLabOS`（也可被环境变量 `ConnectionStrings__SmartLabOS` 覆盖）。
> 改连接只改配置文件，**无需重新编译**。

---

## 数据模型

五张表命名与需求一致（含连字符，MySQL 中以反引号引用）：

| 表名 | 数据源目录 | 卡片数 |
|------|-----------|--------|
| `SmartLabOS-Module`       | `references/01-modules`     | 97 |
| `SmartLabOS-PlatformBase` | `references/02-platforms`   | 3  |
| `SmartLabOS-WorkStation`  | `references/03-workstation` | 8  |
| `SmartLabOS-Solution`     | `references/04-solutions`   | 5  |
| `SmartLabOS-Project`      | `references/05-Project`     | 1  |

每张表采用 **「关键字段列 + 全量保真」** 结构：

- 关键字段提为独立列（`id`、`name`、`status`、`owner`、`category`/`customer`/`platform_id` 等），便于检索；
- `data_json`（MySQL JSON 类型）：整段 front-matter 解析后的 JSON，可做结构化查询；
- `raw_frontmatter`（LONGTEXT）：原始 YAML 原文，无损保留；
- `body_markdown`（LONGTEXT）：front-matter 之后的 Markdown 正文，无损保留。

这样既能在列表里按字段筛选，又能在编辑界面查看/修改完整内容，且与卡片源文件无损往返。

---

## 一、Windows 11 上开发与测试

### 0. 前置环境
- **.NET SDK 8.0+**（本机已验证可用 SDK 10 编译 `net8.0`）：`dotnet --version`
- **MySQL 8.x** 已安装并运行在 `localhost:3306`
- **Python 3.9+**（仅导入数据用）：`pip install pyyaml pymysql`
- Visual Studio 2026（可选，命令行亦可全程完成）

### 1. 建库建表
```powershell
# 用 mysql 客户端执行（按需替换用户名/密码）
mysql -u root -p < database\01-schema.sql
```
> 提示：MySQL 8 默认认证插件为 `caching_sha2_password`。后端连接串已带
> `AllowPublicKeyRetrieval=true`，可正常完成认证握手。

### 2. 配置连接信息
- 导入器：编辑 `db.config.json`（host / port / user / password / database）。
- 后端：编辑 `src/SmartLabOS.DataMaintenance.Api/appsettings.json` 的
  `ConnectionStrings:SmartLabOS`。

### 3. 导入主数据
```powershell
cd tools
python import_data.py            # 增量 UPSERT
# python import_data.py --truncate   # 先清空再导入
# python import_data.py --dry-run    # 仅解析不写库（自检：应为 114 条）
```
完成后用 `database\02-verify.sql` 核对各表条数。

### 4. 运行后端 + 前端（同一站点）
```powershell
cd src\SmartLabOS.DataMaintenance.Api
dotnet run
```
- 前端 UI：<http://localhost:5080/>
- Swagger（接口调试）：<http://localhost:5080/swagger>
- 健康检查：<http://localhost:5080/api/meta/health>

或在 VS 2026 中直接打开 `SmartLabOS.DataMaintenance.sln`，按 F5。

### 5. 功能自测
- 左侧切换 5 类实体；右上「搜索」按 ID/名称/状态模糊检索；翻页。
- 「+ 新增」填表保存；行内「编辑」「删除」。
- 「列 ▾」**自定义可见列**：勾选任意字段加入列表（标"扩展"的为默认隐藏列），
  选择按实体记忆在浏览器 localStorage，可「恢复默认列」。
- `data_json` 提供**折叠式树形编辑器**：树形/源码双视图切换、「美化」格式化、
  实时 JSON 校验；树形视图可折叠展开、编辑叶子值、增删字段/项、重命名键。
  保存前做 JSON 合法性校验，非法 JSON 会被拒绝并提示。
  （界面参见 `docs/screenshot-json-tree-editor.png`）

---

## 二、REST API 速查

`{entity}` ∈ `modules | platforms | workstations | solutions | projects`

| 方法 | 路径 | 说明 |
|------|------|------|
| GET    | `/api/meta` | 实体与列的元数据（前端据此渲染） |
| GET    | `/api/meta/health` | 数据库连通性 |
| GET    | `/api/{entity}?search=&page=1&pageSize=50` | 列表/查询（分页） |
| GET    | `/api/{entity}/{id}` | 单条 |
| POST   | `/api/{entity}` | 新增（JSON body；缺 `id` 或主键重复会报错） |
| PUT    | `/api/{entity}/{id}` | 修改 |
| DELETE | `/api/{entity}/{id}` | 删除 |
| POST   | `/api/{entity}/export` | 批量导出 YAML-MD 卡片到服务器目录 |
| POST   | `/api/{entity}/import/preview` | 预览 Excel 导入（列映射 + 主键冲突检测） |
| POST   | `/api/{entity}/import/commit` | 执行 Excel 导入（新增入库；冲突项按勾选覆盖） |

示例：
```bash
curl http://localhost:5080/api/modules?search=存储&pageSize=10
curl -X POST http://localhost:5080/api/modules -H "Content-Type: application/json" \
     -d '{"id":"MOD-TEST-001","name":"测试模块","status":"active","data_json":"{}"}'
```

---

## 二之二、批量导出 / 导入

> 前端工具栏「导出 YAML-MD」「导入 Excel」两个入口；**路径均为运行后端的服务器端文件系统路径**。

### 导出 YAML-MD（全部 / 指定记录）

列表每行带勾选框（跨页保留选中）。点「导出 YAML-MD」：

- 填**服务器端导出目录**（不存在自动创建）；
- 选范围：「全部记录」或「仅选中记录」；可选「覆盖同名文件」。

每条记录还原为与 `references/` 同构的卡片（`---` + `raw_frontmatter` + `---` + 正文），
文件名取 `source_file`，缺失则 `{id}.md`，UTF-8(BOM) 编码，与源卡片无损往返。

请求体：
```json
{ "directory": "D:\\SmartLabOS\\export\\modules",
  "scope": "all | selected", "ids": ["MOD-JY-001"], "overwrite": true }
```

### 导入 Excel（卡片录入模板 .xlsx / .xlsm，主键重复可覆盖）

导入对象是 `references/_templates/` 下的**卡片录入模板**（竖排卡片：分组 / 字段名称 /
输入内容 + 隐藏列 `group_key` / `field_key` / `类型` / `body_heading`）。
**一张数据工作表 = 一条记录**；`_Templates` 等模板页自动跳过。

点「导入 Excel」→ 填**服务器端 Excel 路径**→「预览」：

- 解析器按隐藏列 `group_key`→`field_key`→`类型` 还原 front-matter，按 `body_heading`
  还原正文，生成 `data_json` / `raw_frontmatter` / `body_markdown` 及各关键列，
  与 `references/*.md` 同构。类型支持：`str/choice/date/int/float/bool/list_inline/
  list_inline_num/list_block/raw_yaml_block/body`，以及 `(cont)` 续行（并入上一字段为列表）。
- 预览按工作表列出状态：**新增 / 已存在(冲突) / 非法（无主键 id）**。
- 冲突项可逐条勾选「覆盖」（或顶部「全选」）。点「确认导入」：
  新增 → 入库；勾选覆盖 → 整条更新；未勾选的冲突 → 跳过。

> Excel 读取为零依赖实现（`System.IO.Compression` + `System.Xml.Linq`），不引入任何第三方包。

**模板就绪状态**：模板需带齐隐藏键列方可被解析。
- ✅ `01 模块` / `02 平台基类`：隐藏键列完整（01 的「上下料时间」行已补
  `group_key=module_up_unload_time / field_key=up_unload_time / 类型=str`），可直接导入。
- ⏳ `03 工作站` / `04 解决方案` / `05 项目`：隐藏键列缺失或不一致，且其数据模型含
  **对象数组**（如 `hardware_config.modules:[{…}]`、`workflows:[{…, workflow_steps:[{…}]}]`），
  超出当前扁平卡片格式的表达能力，需先由模板维护方补齐/规整键列（或扩展模板与解析器以表达对象数组）后方可导入。

---

## 三、发布（Publish）

框架依赖发布（服务器需装 .NET 8 运行时/Hosting Bundle）：
```powershell
cd src\SmartLabOS.DataMaintenance.Api
dotnet publish -c Release -o publish
# 或使用发布配置： dotnet publish -p:PublishProfile=FolderProfile
```
产物在 `publish\`，包含 `SmartLabOS.DataMaintenance.Api.dll`、`wwwroot\`、
`appsettings.json`、`web.config` 等，整目录即可部署。

VS 2026：右键项目 → **发布** → 选择 `FolderProfile` → 发布。

---

## 四、Windows Server 2025 DataCenter + IIS 部署

### 1. 安装服务器组件
1. **启用 IIS**：服务器管理器 → 添加角色和功能 → Web 服务器(IIS)
   （含「Web 管理工具」与「应用程序开发 → .NET Extensibility」等默认项）。
2. **安装 .NET 8 Hosting Bundle**（关键，提供 ASP.NET Core Module V2）：
   下载 `dotnet-hosting-8.0.x-win.exe` 安装后执行 `net stop was /y && net start w3svc`
   或重启服务器，使 IIS 加载 AspNetCoreModuleV2。
3. （若服务器自带 MySQL 同机）确保 MySQL 8 服务运行；否则在连接串中指向远程 MySQL 主机。

### 2. 部署应用
1. 把 `publish\` 整个目录拷到服务器，例如 `C:\inetpub\SmartLabOS\`。
2. IIS 管理器 → 新建 **网站**（或应用程序）：
   - 物理路径：`C:\inetpub\SmartLabOS\`
   - 绑定端口：如 `80`（或自定义）。
3. **应用程序池**：选择「**No Managed Code / 无托管代码**」
   （ASP.NET Core 由 AspNetCoreModule 进程内承载，不用 CLR 托管模式）。
4. 给应用池标识（如 `IIS AppPool\<站点名>`）授予站点目录读取权限；
   若启用 stdout 日志，需对 `logs\` 目录授予写权限。

### 3. 配置数据库连接（独立、免重编译）
编辑服务器上 `C:\inetpub\SmartLabOS\appsettings.json` 的
`ConnectionStrings:SmartLabOS`，指向生产 MySQL。例如：
```json
"ConnectionStrings": {
  "SmartLabOS": "Server=10.0.0.5;Port=3306;Database=SmartLabOS-Presales-AI;User ID=app;Password=******;SslMode=Preferred;AllowPublicKeyRetrieval=true;CharSet=utf8mb4;"
}
```
或改用环境变量（不落盘明文于配置）：在应用池/系统环境变量中设置
`ConnectionStrings__SmartLabOS=...`，其优先级高于 `appsettings.json`。
改完在 IIS 中「回收」应用池即可生效。

### 4. 生产库初始化与导入
- 在生产 MySQL 执行 `database\01-schema.sql` 建库建表。
- 在能访问生产库的机器上改好 `db.config.json` 后运行
  `python tools\import_data.py --truncate` 完成首次灌数。

### 5. 验证
- 浏览器访问 `http://<服务器IP或域名>/` 看到维护界面。
- `http://<服务器>/api/meta/health` 返回 `{"db":"connected"}`。
- `http://<服务器>/swagger` 可调试接口。

---

## 五、常见问题（FAQ）

- **`Access denied for user 'root'@'localhost'`**
  连接串/`db.config.json` 的用户名或密码与 MySQL 实例实际口令不一致。
  用 `mysql -u root -p` 手动登录确认口令；必要时
  `ALTER USER 'root'@'localhost' IDENTIFIED BY '新口令';` 后同步到配置文件。

- **`Authentication method 'caching_sha2_password' ... RSA public key`**
  连接串需含 `AllowPublicKeyRetrieval=true`（本项目已默认带上）。

- **IIS 启动 502.5 / ANCM**
  多为未装 .NET 8 Hosting Bundle，或应用池未设为「无托管代码」。
  装好 Hosting Bundle 并重启 W3SVC。

- **中文乱码**
  数据库/表均为 `utf8mb4`，连接串带 `CharSet=utf8mb4`，API 已配置不转义中文输出。

- **跨域（仅本地分离调试时）**
  后端已开放默认 CORS（任意来源）。生产前后端同源（同一 IIS 站点），无需关心。

---

## 六、扩展：新增字段/新增实体

字段与实体均集中在 `src/.../Data/EntityRegistry.cs`：
- 在对应 `EntityDef` 的 `Columns` 里增删 `ColumnDef`（同时在 `01-schema.sql` 加列）；
- 控制器、仓储、前端表单与列表会自动适配，**无需改其它代码**。

---

## 七、数据迁移记录

**模块新增「模块上下料时间」**（2026-06）：所有 Module 卡片 front-matter 在
`platform_compatibility:` 之前插入新分组，默认值 `60s`：

```yaml
module_up_unload_time:
  up_unload_time: 60s
```

该字段随 front-matter 进入 `data_json` / `raw_frontmatter`，在维护界面经 JSON
树形编辑器查看与修改（与 `platform_compatibility` 等明细字段同构，不单列 DB 列）。
一次性迁移脚本（幂等、保留 BOM+CRLF）：

```powershell
cd tools
python add_module_up_unload_time.py --dry-run   # 预演
python add_module_up_unload_time.py             # 写入 references/01-modules/*.md
python import_data.py                           # UPSERT 回 MySQL
```
