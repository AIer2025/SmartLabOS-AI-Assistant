namespace SmartLabOS.DataMaintenance.Api.Presales;

/// <summary>
/// 售前功能涉及的服务器端路径与外部命令配置。
/// 取自 appsettings.json 的 "Presales" 节，部署时只需改配置、无需重新编译。
/// </summary>
public sealed class PresalesPaths
{
    /// <summary>项目根（包含 references/、potocol/、projects/）。</summary>
    public string ProjectRoot { get; }
    /// <summary>提案输出根目录 projects/。</summary>
    public string ProjectsDir { get; }
    /// <summary>流程标准 MD 目录 potocol/MD。</summary>
    public string ProtocolDir { get; }
    /// <summary>模块知识库目录 references/01-modules（共97个模块卡片 MD）。</summary>
    public string ModulesDir { get; }
    /// <summary>提案 HTML 输出模版文件。</summary>
    public string TemplateFile { get; }
    /// <summary>WORD 提案（详细设计方案版）内容提纲 MD 文件。</summary>
    public string DocxOutlineFile { get; }
    /// <summary>Claude Code 可执行命令（PATH 中可解析的命令或绝对路径）。</summary>
    public string ClaudeExecutable { get; }
    /// <summary>单次生成的超时分钟数。</summary>
    public int GenerationTimeoutMinutes { get; }

    public PresalesPaths(IConfiguration config)
    {
        var s = config.GetSection("Presales");
        ProjectRoot = Norm(s["ProjectRoot"]) ?? @"C:\TestClaude\SmartLabOS-AI-Assistant";
        ProjectsDir = Norm(s["ProjectsDir"]) ?? Path.Combine(ProjectRoot, "projects");
        ProtocolDir = Norm(s["ProtocolDir"]) ?? Path.Combine(ProjectRoot, "potocol", "MD");
        ModulesDir = Norm(s["ModulesDir"]) ?? Path.Combine(ProjectRoot, "references", "01-modules");
        TemplateFile = Norm(s["TemplateFile"])
            ?? Path.Combine(ProjectRoot, "references", "_templates", "99-SmartLabOS-技术提案输出模版.html");
        DocxOutlineFile = Norm(s["DocxOutlineFile"])
            ?? Path.Combine(ProjectRoot, "references", "_templates", "02-SmartLabOS提案标准-详细设计方案版.md");
        ClaudeExecutable = string.IsNullOrWhiteSpace(s["ClaudeExecutable"]) ? "claude" : s["ClaudeExecutable"]!.Trim();
        GenerationTimeoutMinutes = int.TryParse(s["GenerationTimeoutMinutes"], out var m) && m > 0 ? m : 60;
    }

    private static string? Norm(string? path) =>
        string.IsNullOrWhiteSpace(path) ? null : path.Trim();

    /// <summary>校验并返回某项目的输出目录（projects/&lt;name&gt;）。</summary>
    public string ProjectDir(string projectName) => Path.Combine(ProjectsDir, projectName);
}
