using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using SmartLabOS.DataMaintenance.Api.Data;
using SmartLabOS.DataMaintenance.Api.Presales;
using SmartLabOS.DataMaintenance.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(o =>
{
    // 不转义中文，便于直接阅读 API 输出
    o.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "SmartLabOS 数据维护 API", Version = "v1" });
});

// 仓储为无状态、按需打开连接，单例即可。
builder.Services.AddSingleton<SmartLabRepository>();

// 售前方案自动生成：需求仓储 + 路径配置 + Claude Code 执行器（执行器持有内存任务状态，须单例）。
builder.Services.AddSingleton<PresalesRepository>();
builder.Services.AddSingleton<PresalesPaths>();
builder.Services.AddSingleton<ClaudeCodeRunner>();

// 开发期允许任意来源调试前端；生产部署同源(同一 IIS 站点)无需 CORS。
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

// Swagger 在所有环境开放，便于服务器侧接口验证。
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartLabOS 数据维护 API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors();

// 前端静态页面 (wwwroot/index.html)
// 维护工具：禁用静态资源缓存，避免浏览器加载到旧版 app.js / css 导致界面行为异常。
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

app.Run();
