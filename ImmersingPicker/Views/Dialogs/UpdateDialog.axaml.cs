using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using ImmersingPicker.Core.Models;
using ImmersingPicker.Services.Services;
using Serilog;

namespace ImmersingPicker.Views.Dialogs;

public partial class UpdateDialog : UserControl
{
    private static readonly ILogger _logger = Log.ForContext<UpdateDialog>();
    
    private readonly UpdateInfo _updateInfo;
    private ContentDialog? _parentDialog;
    private bool _isUpdating = false;

    public UpdateDialog(UpdateInfo updateInfo, ContentDialog? parentDialog = null)
    {
        _updateInfo = updateInfo;
        _parentDialog = parentDialog;
        
        InitializeComponent();
        UpdateControls();
    }

    /// <summary>
    /// 更新控件内容
    /// </summary>
    private void UpdateControls()
    {
        VersionInfoText.Text = $"{_updateInfo.ReleaseName}";
        ReleaseVersionText.Text = $"v{_updateInfo.Version}";
        ReleaseDateText.Text = _updateInfo.ReleaseDate.ToString("yyyy-MM-dd HH:mm");
        
        // 格式化文件大小
        FileSizeText.Text = FormatFileSize(_updateInfo.FileSize);
        
        // 显示更新日志 (Markdown 简单处理)
        ReleaseNotesText.Text = _updateInfo.ReleaseNotes.Length > 1000 
            ? _updateInfo.ReleaseNotes[..1000] + "..." 
            : _updateInfo.ReleaseNotes;

        // 如果是预发布版本,显示提示
        if (_updateInfo.IsPrerelease)
        {
            VersionInfoText.Text += " (预发布版本)";
        }
    }

    /// <summary>
    /// 立即更新按钮点击
    /// </summary>
    private async void UpdateButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_isUpdating)
            return;

        _isUpdating = true;
        UpdateButton.IsEnabled = false;
        LaterButton.IsEnabled = false;
        SkipVersionButton.IsEnabled = false;

        try
        {
            // 显示进度
            ProgressBorder.IsVisible = true;
            UpdateButton.Content = "更新中...";

            // 下载更新
            var downloadedFilePath = await DownloadUpdateAsync();
            
            if (string.IsNullOrEmpty(downloadedFilePath))
            {
                ShowErrorMessage("下载失败");
                return;
            }

            // 安装更新
            ProgressStatusText.Text = "正在安装更新...";
            
            var installSuccess = await UpdateInstaller.Instance.InstallUpdateAsync(
                downloadedFilePath,
                AppDomain.CurrentDomain.BaseDirectory);

            if (installSuccess)
            {
                ProgressStatusText.Text = "更新完成! 即将重启应用...";
                DownloadProgressBar.Value = 100;
                
                // 发送更新完成通知
                SystemNotificationService.Instance.ShowUpdateDownloadedNotification();
                
                await Task.Delay(1500);
                
                // 重启应用
                UpdateInstaller.Instance.RestartApplication();
                
                _parentDialog?.Hide(ContentDialogResult.Primary);
            }
            else
            {
                // 发送更新失败通知
                SystemNotificationService.Instance.ShowUpdateFailedNotification("安装更新失败");
                ShowErrorMessage("安装更新失败");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "更新过程发生错误");
            ShowErrorMessage($"更新失败: {ex.Message}");
        }
        finally
        {
            _isUpdating = false;
            UpdateButton.IsEnabled = true;
            LaterButton.IsEnabled = true;
            SkipVersionButton.IsEnabled = true;
        }
    }

    /// <summary>
    /// 下载更新
    /// </summary>
    private async Task<string?> DownloadUpdateAsync()
    {
        if (string.IsNullOrEmpty(_updateInfo.DownloadUrl))
        {
            _logger.Error("下载链接为空");
            return null;
        }

        var progress = new Progress<DownloadProgressInfo>(info =>
        {
            DownloadProgressBar.Value = info.ProgressPercentage;
            ProgressStatusText.Text = info.StatusMessage ?? "下载中...";
            ProgressDetailText.Text = info.TotalBytesToReceive > 0
                ? $"{FormatBytes(info.BytesReceived)} / {FormatBytes(info.TotalBytesToReceive)}"
                : FormatBytes(info.BytesReceived);
        });

        var downloadPath = DownloadManager.GetTempDownloadPath();
        var fileName = _updateInfo.AssetName;

        try
        {
            var filePath = await DownloadManager.Instance.DownloadFileAsync(
                _updateInfo.DownloadUrl,
                downloadPath,
                fileName,
                progress);

            return filePath;
        }
        catch (OperationCanceledException)
        {
            _logger.Warning("下载已取消");
            return null;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "下载更新失败");
            return null;
        }
    }

    /// <summary>
    /// 稍后提醒按钮点击
    /// </summary>
    private void LaterButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _logger.Information("用户选择稍后提醒");
        _parentDialog?.Hide(ContentDialogResult.Secondary);
    }

    /// <summary>
    /// 跳过此版本按钮点击
    /// </summary>
    private void SkipVersionButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _logger.Information("用户选择跳过版本: {Version}", _updateInfo.Version);
        AppSettings.Instance.SkippedUpdateVersion = _updateInfo.Version;
        _parentDialog?.Hide(ContentDialogResult.None);
    }

    /// <summary>
    /// 显示错误消息
    /// </summary>
    private void ShowErrorMessage(string message)
    {
        ProgressStatusText.Text = $"错误: {message}";
        ProgressSpinner.Text = "❌";
        UpdateButton.Content = "重试";
        UpdateButton.IsEnabled = true;
        LaterButton.IsEnabled = true;
        SkipVersionButton.IsEnabled = true;
        _isUpdating = false;
    }

    /// <summary>
    /// 格式化文件大小
    /// </summary>
    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }

    /// <summary>
    /// 格式化字节数
    /// </summary>
    private static string FormatBytes(double bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}
