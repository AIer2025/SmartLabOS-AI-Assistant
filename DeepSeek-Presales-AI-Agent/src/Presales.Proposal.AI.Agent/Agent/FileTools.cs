using System.Text;
using Presales.Proposal.AI.Agent.Configuration;

namespace Presales.Proposal.AI.Agent.Agent;

/// <summary>
/// 受控文件工具集 —— 本项目取代旧实现「claude --permission-mode bypassPermissions --add-dir 项目根」
/// 越权模式的核心安全边界。所有读写都由本类的 C# 代码亲自执行，并强制：
///   - 读：仅限知识库 references/ 与 流程标准 potocol/ 之下（只读、大小上限）；
///   - 写：仅限**单个项目输出目录**之内，且扩展名白名单(.html/.md)，防目录穿越。
/// 每个生成任务实例化一个，与该项目输出目录绑定；记录本次写出的文件清单。
/// </summary>
public sealed class FileTools
{
    private readonly PresalesPaths _paths;
    private readonly string _projectDir;      // 规范化后的项目输出目录（写入白名单根）
    private readonly string[] _readRoots;      // 允许读取的根（references / potocol）
    private const long MaxReadBytes = 512 * 1024;    // 单文件读取上限 512KB
    private const int MaxListEntries = 400;

    public FileTools(PresalesPaths paths, string projectDir)
    {
        _paths = paths;
        _projectDir = Path.GetFullPath(projectDir);
        Directory.CreateDirectory(_projectDir);
        // 允许读取：配置文件声明的全部知识库根（模块/平台/托盘/流程标准，见 PresalesPaths.KnowledgeRoots），
        // 以及本项目**自己**的输出目录（便于第二阶段整合已生成的 HTML）。
        _readRoots = _paths.KnowledgeRoots.Append(_projectDir).ToArray();
    }

    /// <summary>本次任务已写出的输出文件名（去重、保持顺序）。</summary>
    public List<string> WrittenFiles { get; } = new();

    /// <summary>本次任务读取过的本项目输出文件（用于校验阶段二是否真的整合了阶段一产物）。</summary>
    public List<string> ReadOutputFiles { get; } = new();

    /// <summary>项目输出目录的寻址前缀——模型只能通过它访问本项目已生成的文件。</summary>
    private static readonly string[] ProjectPrefixes = { "project/", "output/", "项目/", "outputs/" };

    // ---------------- 只读：知识库 / 流程标准 ----------------

    /// <summary>列出某目录下的文件与子目录（references/、potocol/，或用 project/ 前缀访问本项目输出目录）。</summary>
    public string ListDir(string relativeOrAbsolute)
    {
        var (full, error) = ResolveForRead(relativeOrAbsolute);
        if (error is not null) return error;
        if (!Directory.Exists(full)) return $"[错误] 目录不存在：{relativeOrAbsolute}{Hint()}";

        var sb = new StringBuilder();
        sb.AppendLine($"目录：{Rel(full)}");
        int n = 0;
        foreach (var d in Directory.EnumerateDirectories(full).OrderBy(x => x, StringComparer.Ordinal))
        {
            if (_paths.IsDeprecated(Path.GetFullPath(d))) continue;   // 废弃资料不出现在列表里
            if (n++ >= MaxListEntries) { sb.AppendLine("… (超出上限，已截断)"); return sb.ToString(); }
            sb.AppendLine($"  [DIR] {Path.GetFileName(d)}/");
        }
        foreach (var f in Directory.EnumerateFiles(full).OrderBy(x => x, StringComparer.Ordinal))
        {
            if (n++ >= MaxListEntries) { sb.AppendLine("… (超出上限，已截断)"); return sb.ToString(); }
            sb.AppendLine($"  {Path.GetFileName(f)}");
        }
        return sb.ToString();
    }

    /// <summary>读取知识库/流程标准/本项目已生成文件（须落在允许根内）。</summary>
    public string ReadFile(string relativeOrAbsolute)
    {
        var (full, error) = ResolveForRead(relativeOrAbsolute);
        if (error is not null) return error;
        if (!File.Exists(full)) return $"[错误] 文件不存在：{relativeOrAbsolute}{Hint()}";

        // 记录对本项目产物的读取，供上层校验阶段二是否真的整合了阶段一成果
        if (full!.StartsWith(_projectDir + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            var name = Path.GetFileName(full);
            if (!ReadOutputFiles.Contains(name, StringComparer.OrdinalIgnoreCase)) ReadOutputFiles.Add(name);
        }

        try
        {
            var info = new FileInfo(full);
            if (info.Length > MaxReadBytes)
            {
                using var sr = new StreamReader(full);
                var buf = new char[256 * 1024];
                int read = sr.Read(buf, 0, buf.Length);
                return new string(buf, 0, read) + $"\n… [文件过大({info.Length} 字节)，仅返回前 256KB]";
            }
            return File.ReadAllText(full);
        }
        catch (Exception ex) { return $"[错误] 读取失败：{ex.Message}"; }
    }

    // ---------------- 写：仅限本项目输出目录 ----------------

    /// <summary>把一份提案内容写入本项目输出目录（覆盖）。扩展名白名单(.html/.md)，防目录穿越。</summary>
    public string WriteOutput(string fileName, string content)
    {
        var (full, name, err) = ResolveForWrite(fileName);
        if (err is not null) return err;

        try
        {
            File.WriteAllText(full!, content ?? "", new UTF8Encoding(true)); // UTF-8 BOM，Windows 记事本可正确显示中文
            Track(name!);
            return $"已写入 {name}（{(content ?? "").Length} 字符）。如内容尚未完整，请用 append_output 继续追加。";
        }
        catch (Exception ex) { return $"[错误] 写入失败：{ex.Message}"; }
    }

    /// <summary>向输出文件末尾追加内容（文件不存在则新建）。用于分片写出超过单次输出上限的长文档。</summary>
    public string AppendOutput(string fileName, string content)
    {
        var (full, name, err) = ResolveForWrite(fileName);
        if (err is not null) return err;

        try
        {
            if (!File.Exists(full!))
            {
                File.WriteAllText(full!, content ?? "", new UTF8Encoding(true));
                Track(name!);
                return $"{name} 原本不存在，已新建并写入（{(content ?? "").Length} 字符）。";
            }
            File.AppendAllText(full!, content ?? "", new UTF8Encoding(false)); // 追加时不再重复写 BOM
            Track(name!);
            var total = new FileInfo(full!).Length;
            return $"已追加到 {name}（本次 {(content ?? "").Length} 字符，文件现 {total} 字节）。";
        }
        catch (Exception ex) { return $"[错误] 追加失败：{ex.Message}"; }
    }

    /// <summary>输出文件是否已存在（供上层做产物校验）。</summary>
    public bool OutputExists(string fileName)
    {
        var (full, _, err) = ResolveForWrite(fileName);
        return err is null && File.Exists(full!);
    }

    private void Track(string fileName)
    {
        if (!WrittenFiles.Contains(fileName, StringComparer.OrdinalIgnoreCase))
            WrittenFiles.Add(fileName);
    }

    /// <summary>写入路径校验：纯文件名 + 扩展名白名单 + 目录穿越防护。返回 (完整路径, 规范文件名, 错误信息)。</summary>
    private (string? Full, string? Name, string? Error) ResolveForWrite(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return (null, null, "[错误] 文件名为空。");
        fileName = fileName.Trim();

        // 只取纯文件名，禁止路径分隔与非法字符（彻底杜绝穿越）
        if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            || fileName.Contains('/') || fileName.Contains('\\'))
            return (null, null, "[错误] 文件名非法：只允许纯文件名，不得包含路径分隔符或非法字符。");

        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (ext is not (".html" or ".htm" or ".md"))
            return (null, null, "[错误] 仅允许写出 .html / .md 文件。");

        var full = Path.GetFullPath(Path.Combine(_projectDir, fileName));
        if (!full.StartsWith(_projectDir + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            return (null, null, "[错误] 越出项目输出目录，已拒绝。");

        return (full, fileName, null);
    }

    // ---------------- 内部：路径解析与校验 ----------------

    /// <summary>
    /// 读路径解析。三种寻址方式：
    ///   1) <c>project/xxx</c>（或 output/、项目/）—— 本项目输出目录，阶段二据此回读阶段一产物；
    ///   2) <c>references/…</c>、<c>potocol/…</c> —— 相对项目根的知识库路径；
    ///   3) 裸文件名 —— 先按项目根找，找不到再按本项目输出目录找（模型习惯直接写 1.html）。
    /// 越界一律返回带指引的错误信息，而不是含糊的“不可访问”——否则模型会像上次那样反复瞎试。
    /// </summary>
    private (string? Full, string? Error) ResolveForRead(string? path)
    {
        var original = path ?? "";
        var raw = original.Trim().Replace('\\', '/').Trim();
        while (raw.StartsWith("./")) raw = raw[2..];
        raw = raw.TrimStart('/');

        // 空 / "." → 直接给本项目输出目录
        if (raw.Length == 0 || raw == ".") return (_projectDir, null);

        // ① 显式前缀 → 本项目输出目录
        foreach (var prefix in ProjectPrefixes)
        {
            if (raw.Equals(prefix.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
                return (_projectDir, null);
            if (raw.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return Contain(Path.Combine(_projectDir, raw[prefix.Length..]), original);
        }

        // ② 绝对路径 / 相对项目根
        var full = Path.IsPathRooted(raw)
            ? Path.GetFullPath(raw)
            : Path.GetFullPath(Path.Combine(_paths.ProjectRoot, raw));

        // 废弃资料：给出明确原因，否则模型会以为是自己路径写错而反复重试
        if (_paths.IsDeprecated(full))
            return (null, $"[错误] {original} 属于已废弃资料，不作为选型与写作依据，请勿引用。"
                        + "请改用模块/平台/托盘卡片、流程标准国标与提案模版目录。");

        if (InRoots(full)) return (full, null);

        // ③ 回退：当作本项目输出目录下的相对路径
        if (!Path.IsPathRooted(raw))
        {
            var alt = Path.GetFullPath(Path.Combine(_projectDir, raw));
            if (InRoots(alt)) return (alt, null);
        }

        return (null, $"[错误] 路径越界或不存在：{original}{Hint()}");
    }

    private (string? Full, string? Error) Contain(string candidate, string original)
    {
        var full = Path.GetFullPath(candidate);
        return InRoots(full) ? (full, null) : (null, $"[错误] 路径越界：{original}{Hint()}");
    }

    private bool InRoots(string full) => !_paths.IsDeprecated(full) && _readRoots.Any(root =>
        full.Equals(root, StringComparison.OrdinalIgnoreCase)
        || full.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase));

    /// <summary>越界/找不到时给模型明确指引（列出配置文件里真实生效的知识库根），避免它反复试错浪费轮次与 token。</summary>
    private string Hint() =>
        "。可用路径：" + string.Join("、", _paths.KnowledgeRoots.Select(r => _paths.Display(r) + "/…"))
      + "、project/…（本项目输出目录，如 project/1.html；用 list_kb_dir project 可列出已生成文件）。";

    private string Rel(string full)
    {
        try { return Path.GetRelativePath(_paths.ProjectRoot, full); }
        catch { return full; }
    }
}
