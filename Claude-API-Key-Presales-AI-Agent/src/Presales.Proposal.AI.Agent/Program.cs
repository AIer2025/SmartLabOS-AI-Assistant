using System.Text.Encodings.Web;
using System.Text.Unicode;
using Presales.Proposal.AI.Agent.Agent;
using Presales.Proposal.AI.Agent.Configuration;
using Presales.Proposal.AI.Agent.Data;
using SmartLabOS.DataMaintenance.Api.Data;   // 移植自 DataMaintenance 的主数据维护仓储（保留原命名空间）

var builder = WebApplication.CreateBuilder(args);

// ---- 唯一外部配置源：env/Presales-AI-Agent-config.json ----
// MySQL 连接参数与 ClaudeAI 运行目录全部来自该文件；以内存配置层追加，
// 优先级高于 appsettings.json 与环境变量——现场只认这一份文件，杜绝
// 「改了 JSON 却被残留的 ConnectionStrings__SmartLabOS 环境变量顶掉」这类幽灵问题。
// （Anthropic API Key 不在此文件内，仍走 ANTHROPIC_API_KEY / user-secrets。）
var envFile = AgentEnvFile.Load(builder.Environment.ContentRootPath);
if (envFile.Values.Count > 0) builder.Configuration.AddInMemoryCollection(envFile.Values);

// 控制器 + 不转义中文，便于直接阅读 API 输出
builder.Services.AddControllers().AddJsonOptions(o =>
    o.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
    c.SwaggerDoc("v1", new() { Title = "SmartLabOS 售前提案 AI Agent API", Version = "v1" }));

// ---- 主数据维护（移植自 DataMaintenance）：无状态仓储，单例 ----
builder.Services.AddSingleton<SmartLabRepository>();

// ---- 售前提案 Agent：领域/数据/Agent 依赖 ----
builder.Services.AddSingleton(envFile);            // 供健康检查/诊断接口回报配置来源
builder.Services.AddSingleton<PresalesPaths>();
builder.Services.AddSingleton(sp => AgentSettings.FromConfig(sp.GetRequiredService<IConfiguration>()));
builder.Services.AddSingleton<ModuleCatalog>();
builder.Services.AddSingleton<ProposalTemplateSet>();
builder.Services.AddSingleton<PresalesRepository>();
builder.Services.AddSingleton<DocxExporter>();
builder.Services.AddSingleton<ClaudeAgentService>();

// ---- 后台生成：登记表 + 队列 + 工作者（替代 Task.Run 即发即忘）----
builder.Services.AddSingleton<JobRegistry>();
builder.Services.AddSingleton<GenerationQueue>();
builder.Services.AddHostedService<GenerationWorker>();

var app = builder.Build();

// Swagger 仅在开发环境开放（生产默认关闭，收敛攻击面）
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartLabOS 售前提案 AI Agent API v1");
        c.RoutePrefix = "swagger";
    });
}

// 前端静态页面（与 API 同源，无需 CORS）；禁用缓存避免加载旧版脚本
app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
        ctx.Context.Response.Headers.Pragma = "no-cache";
    }
});

app.MapControllers();

// 启动自检：打印配置来源、各路径、数据库连通性与 Agent 就绪度，便于第一时间发现配置问题
try
{
    if (envFile.Loaded)
        app.Logger.LogInformation("配置文件：{Path}", envFile.FilePath);
    else
        app.Logger.LogError("配置文件未生效（{Path}）：{Error} —— MySQL 与知识库目录将退回内置默认值，请修复后重启。",
            envFile.Location, envFile.Error);

    foreach (var w in envFile.Warnings) app.Logger.LogWarning("配置提醒：{Warning}", w);

    var paths = app.Services.GetRequiredService<PresalesPaths>();
    app.Logger.LogInformation(
        "运行目录：模块={Modules} | 平台={Platforms} | 托盘={Pallet} | 国标={Protocol} | 输出={Projects}"
      + " | 提案模版={Template} | 标准模版={Templates}",
        paths.ModulesDir, paths.PlatformsDir, paths.PalletDir, paths.ProtocolDir, paths.ProjectsDir,
        paths.ProposalTemplateDir, paths.TemplatesDir);
    app.Logger.LogInformation("已废弃(隐藏且拒读)：{Deprecated}", string.Join("；", paths.DeprecatedDirs));

    // 《详细设计方案》章节范围标准（★必备/○按需）是每次生成都要内联的检查清单。
    // 它缺失时程序只会退回一段兜底文字继续跑——完整性校验事实上失效且不报错，
    // 所以这里必须显式示警，而不是等交付物出问题才发现。
    if (!File.Exists(paths.DetailTemplateFile))
        app.Logger.LogError("缺少《详细设计方案》章节范围标准：{File}。生成时将退回兜底描述，★必备项校验失效。"
                          + "请检查配置 Template_Path。", paths.DetailTemplateFile);

    var templates = app.Services.GetRequiredService<ProposalTemplateSet>();
    if (templates.All().Count == 0)
        app.Logger.LogWarning("提案模版目录中未找到任何样例文件（{Dir}）——交付物版式将退回内置模版。",
            paths.ProposalTemplateDir);
    else
        foreach (var t in templates.All())
            app.Logger.LogInformation("提案模版 [{Role}] {File}", t.Role, t.FileName);

    var repo = app.Services.GetRequiredService<PresalesRepository>();
    var agent = app.Services.GetRequiredService<ClaudeAgentService>();
    app.Logger.LogInformation("MySQL 目标：{Target}", envFile.DbSummary);
    app.Logger.LogInformation("MySQL 连通性：{Db}", await repo.PingAsync() ? "OK" : "失败（请检查 env 配置文件中的 DB_Connect）");
    app.Logger.LogInformation("Anthropic API Key：{Key}", agent.ApiKeyDescription);
}
catch (Exception ex) { app.Logger.LogWarning(ex, "启动自检异常"); }

app.Run();
