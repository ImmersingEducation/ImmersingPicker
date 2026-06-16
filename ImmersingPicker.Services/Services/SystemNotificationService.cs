using System;
using System.Diagnostics;
using Serilog;

namespace ImmersingPicker.Services.Services;

/// <summary>
/// 系统通知服务 - 负责发送系统级通知
/// </summary>
public class SystemNotificationService
{
    private static readonly SystemNotificationService _instance = new();
    public static SystemNotificationService Instance => _instance;

    private static readonly ILogger _logger = Log.ForContext<SystemNotificationService>();

    private SystemNotificationService()
    {
    }

    /// <summary>
    /// 发送系统通知
    /// </summary>
    /// <param name="title">通知标题</param>
    /// <param name="message">通知内容</param>
    /// <param name="iconPath">图标路径 (可选)</param>
    public void ShowNotification(string title, string message, string? iconPath = null)
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                ShowWindowsNotification(title, message, iconPath);
            }
            else
            {
                _logger.Warning("当前平台不支持系统通知");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "发送系统通知失败");
        }
    }

    /// <summary>
    /// 发送 Windows Toast 通知
    /// </summary>
    private void ShowWindowsNotification(string title, string message, string? iconPath = null)
    {
        try
        {
            // 使用 PowerShell 发送 Toast 通知
            string script = GenerateToastNotificationScript(title, message, iconPath);
            ExecutePowerShellScript(script);

            _logger.Information("系统通知已发送: {Title}", title);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "发送 Windows Toast 通知失败");
            // 降级为显示消息框
            ShowFallbackNotification(title, message);
        }
    }

    /// <summary>
    /// 生成 Toast 通知的 PowerShell 脚本
    /// </summary>
    private string GenerateToastNotificationScript(string title, string message, string? iconPath = null)
    {
        // 转义单引号
        string escapedTitle = title.Replace("'", "''");
        string escapedMessage = message.Replace("'", "''");
        
        // 使用 XML 模板创建 Toast 通知
        return $@"
# 加载 Windows Runtime
[Windows.UI.Notifications.ToastNotificationManager, Windows.UI.Notifications, ContentType = WindowsRuntime] | Out-Null
[Windows.Data.Xml.Dom.XmlDocument, Windows.Data.Xml.Dom.XmlDocument, ContentType = WindowsRuntime] | Out-Null

# 获取模板
$toastTemplate = [Windows.UI.Notifications.ToastTemplateType]::ToastText02
$toastXml = [Windows.UI.Notifications.ToastNotificationManager]::GetTemplateContent($toastTemplate)

# 设置文本
$textElements = $toastXml.GetElementsByTagName('text')
$textElements.Item(0).AppendChild($toastXml.CreateTextNode('{escapedTitle}')) | Out-Null
$textElements.Item(1).AppendChild($toastXml.CreateTextNode('{escapedMessage}')) | Out-Null

# 创建 XML 文档
$xmlDoc = New-Object Windows.Data.Xml.Dom.XmlDocument
$xmlDoc.LoadXml($toastXml.GetXml())

# 创建并显示 Toast
$toast = New-Object Windows.UI.Notifications.ToastNotification($xmlDoc)
$toast.Tag = 'ImmersingPicker'
$toast.Group = 'ImmersingPicker'
$toast.ExpirationTime = [DateTimeOffset]::Now.AddMinutes(5)

[Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier('ImmersingPicker').Show($toast)
";
    }

    /// <summary>
    /// 执行 PowerShell 脚本
    /// </summary>
    private void ExecutePowerShellScript(string script)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -Command \"{script}\"",
            UseShellExecute = true,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        using var process = Process.Start(processStartInfo);
        process?.WaitForExit(5000); // 最多等待 5 秒
    }

    /// <summary>
    /// 降级通知方式 (消息框)
    /// </summary>
    private void ShowFallbackNotification(string title, string message)
    {
        try
        {
            // 使用 PowerShell 显示消息框
            string script = $@"
Add-Type -AssemblyName PresentationFramework
[System.Windows.MessageBox]::Show('{message.Replace("'", "''")}', '{title.Replace("'", "''")}', 'OK', 'Information')
";
            ExecutePowerShellScript(script);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "显示降级通知失败");
        }
    }

}
