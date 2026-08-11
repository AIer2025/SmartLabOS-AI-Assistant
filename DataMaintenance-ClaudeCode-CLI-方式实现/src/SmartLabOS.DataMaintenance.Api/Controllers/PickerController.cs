using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace SmartLabOS.DataMaintenance.Api.Controllers;

/// <summary>
/// 本地原生「文件 / 目录」选择对话框。
/// 适用于在本机（localhost）交互式运行后端的场景：浏览器无法直接打开服务器端的
/// 文件选择器，故由后端在服务器（即本机）桌面弹出 Windows 原生对话框并回传所选路径。
///   POST /api/picker/file    选择 Excel 文件
///   POST /api/picker/folder  选择目录
/// 返回 { path: "..." }；用户取消则 path 为 null。
/// </summary>
[ApiController]
[Route("api/picker")]
public sealed class PickerController : ControllerBase
{
    private const string FileScript = """
        Add-Type -AssemblyName System.Windows.Forms | Out-Null
        $owner = New-Object System.Windows.Forms.Form -Property @{TopMost=$true; ShowInTaskbar=$false; Opacity=0; Width=0; Height=0}
        $owner.Show(); $owner.Activate()
        $dlg = New-Object System.Windows.Forms.OpenFileDialog
        $dlg.Title = '选择要导入的 Excel 卡片模板'
        $dlg.Filter = 'Excel 卡片模板 (*.xlsx;*.xlsm)|*.xlsx;*.xlsm|所有文件 (*.*)|*.*'
        $res = $dlg.ShowDialog($owner)
        $owner.Dispose()
        if ($res -eq [System.Windows.Forms.DialogResult]::OK) { [Console]::Out.Write([Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($dlg.FileName))) }
        """;

    private const string FolderScript = """
        Add-Type -AssemblyName System.Windows.Forms | Out-Null
        $owner = New-Object System.Windows.Forms.Form -Property @{TopMost=$true; ShowInTaskbar=$false; Opacity=0; Width=0; Height=0}
        $owner.Show(); $owner.Activate()
        $dlg = New-Object System.Windows.Forms.FolderBrowserDialog
        $dlg.Description = '选择导出目录'
        $dlg.ShowNewFolderButton = $true
        $res = $dlg.ShowDialog($owner)
        $owner.Dispose()
        if ($res -eq [System.Windows.Forms.DialogResult]::OK) { [Console]::Out.Write([Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($dlg.SelectedPath))) }
        """;

    [HttpPost("file")]
    public IActionResult PickFile() => RunPicker(FileScript);

    [HttpPost("folder")]
    public IActionResult PickFolder() => RunPicker(FolderScript);

    private IActionResult RunPicker(string script)
    {
        if (!OperatingSystem.IsWindows())
            return BadRequest(new { message = "选择对话框仅在 Windows 本机运行时可用，请手动输入路径。" });

        try
        {
            // 用 -EncodedCommand 传脚本，规避引号/中文转义问题；-Sta 供 WinForms 对话框使用。
            var enc = Convert.ToBase64String(Encoding.Unicode.GetBytes(script));
            var psi = new ProcessStartInfo("powershell.exe",
                $"-NoProfile -Sta -ExecutionPolicy Bypass -EncodedCommand {enc}")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
            };
            using var p = Process.Start(psi);
            if (p is null) return StatusCode(500, new { message = "无法启动选择对话框进程。" });

            var stdout = p.StandardOutput.ReadToEnd();
            if (!p.WaitForExit(180_000)) { try { p.Kill(true); } catch { } return Ok(new { path = (string?)null }); }

            // 脚本以 Base64(UTF-8) 回传路径，规避 Windows 控制台代码页(GBK)导致的中文乱码。
            var b64 = stdout.Trim();
            string? path = null;
            if (b64.Length > 0)
            {
                try { path = Encoding.UTF8.GetString(Convert.FromBase64String(b64)); }
                catch { path = null; }
            }
            return Ok(new { path = string.IsNullOrEmpty(path) ? null : path });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"打开选择对话框失败：{ex.Message}" });
        }
    }
}
