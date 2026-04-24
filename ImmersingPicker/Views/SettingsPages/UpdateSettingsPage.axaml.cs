using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using ImmersingPicker.Controls;
using ImmersingPicker.Core.Models;
using ImmersingPicker.Services;
using ImmersingPicker.Services.Helper;
using ImmersingPicker.Services.Services;
using ImmersingPicker.Views.Dialogs;
using Serilog;

namespace ImmersingPicker.Views.SettingsPages;

public partial class UpdateSettingsPage : SettingsPageBase
{
    private static readonly ILogger _logger = Log.ForContext<UpdateSettingsPage>();

    public UpdateSettingsPage()
    {
        InitializeComponent();
        PageTitle.Text = "更新管理";
        LoadSettings();
    }

    protected override void LoadSettings()
    {
        // 加载更新设置
        AutoCheckUpdateToggle.IsChecked = AppSettings.AutoCheckUpdateEnabled;
        AllowPrereleaseToggle.IsChecked = AppSettings.AllowPrereleaseUpdates;

        // 显示当前版本
        var currentVersion = VersionHelper.GetCurrentVersion();
        if (currentVersion != null)
        {
            CurrentVersionText.Text = VersionHelper.FormatVersion(currentVersion);
        }
        else
        {
            CurrentVersionText.Text = "未知";
        }
    }

    /// <summary>
    /// 检查更新按钮点击
    /// </summary>
    private async void CheckUpdateButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await CheckForUpdatesManuallyAsync();
    }

    /// <summary>
    /// 手动检查更新
    /// </summary>
    public async Task CheckForUpdatesManuallyAsync()
    {
        try
        {
            CheckUpdateButton.IsEnabled = false;
            CheckUpdateButton.Content = "检查中...";

            _logger.Information("手动检查更新...");

            var (result, updateInfo) = await UpdateService.Instance.CheckForUpdatesAsync(
                AppSettings.AllowPrereleaseUpdates);

            // 更新最后检查时间
            AppSettings.LastUpdateCheckTime = DateTime.Now;

            // 处理检查结果
            switch (result)
            {
                case UpdateCheckResult.UpdateAvailable when updateInfo != null:
                    _logger.Information("发现新版本: {Version}", updateInfo.Version);
                    await ShowUpdateDialogAsync(updateInfo);
                    break;

                case UpdateCheckResult.NoUpdate:
                    _logger.Information("当前已是最新版本");
                    await ShowInfoDialogAsync("当前已是最新版本", "没有可用的更新");
                    break;

                case UpdateCheckResult.Cancelled:
                    _logger.Information("用户已跳过此版本");
                    await ShowInfoDialogAsync("已跳过", "您已选择跳过此版本");
                    break;

                case UpdateCheckResult.CheckFailed:
                    _logger.Warning("检查更新失败");
                    await ShowErrorDialogAsync(
                        "检查失败", 
                        "无法检查更新，请检查：\n1. 网络连接是否正常\n2. GitHub 是否可访问\n3. 防火墙设置");
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "检查更新时发生错误");
            await ShowErrorDialogAsync(
                "错误", 
                $"检查更新失败:\n{ex.Message}\n\n详细信息请查看日志文件。");
        }
        finally
        {
            CheckUpdateButton.IsEnabled = true;
            CheckUpdateButton.Content = "检查更新";
        }
    }

    /// <summary>
    /// 显示更新对话框
    /// </summary>
    private async Task ShowUpdateDialogAsync(UpdateInfo updateInfo)
    {
        // 发送系统通知
        SystemNotificationService.Instance.ShowUpdateAvailableNotification(
            updateInfo.Version,
            updateInfo.ReleaseNotes);

        var dialog = new ContentDialog
        {
            Title = null,
            Content = new UpdateDialog(updateInfo),
            FullSizeDesired = false
        };

        await dialog.ShowAsync();
    }

    /// <summary>
    /// 显示信息对话框
    /// </summary>
    private async Task ShowInfoDialogAsync(string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "确定"
        };

        await dialog.ShowAsync();
    }

    /// <summary>
    /// 显示错误对话框
    /// </summary>
    private async Task ShowErrorDialogAsync(string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "确定"
        };

        await dialog.ShowAsync();
    }

    /// <summary>
    /// 自动检查更新开关
    /// </summary>
    private void AutoCheckUpdateToggle_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (AutoCheckUpdateToggle is ToggleSwitch toggle)
        {
            AppSettings.AutoCheckUpdateEnabled = toggle.IsChecked ?? true;
            _logger.Information("自动检查更新: {Enabled}", AppSettings.AutoCheckUpdateEnabled);
        }
    }

    /// <summary>
    /// 允许预发布版本开关
    /// </summary>
    private void AllowPrereleaseToggle_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (AllowPrereleaseToggle is ToggleSwitch toggle)
        {
            AppSettings.AllowPrereleaseUpdates = toggle.IsChecked ?? false;
            _logger.Information("允许预发布版本: {Enabled}", AppSettings.AllowPrereleaseUpdates);
        }
    }

    /// <summary>
    /// 更新说明按钮点击
    /// </summary>
    private async void UpdateInfoButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "更新说明",
            Content = new ScrollViewer
            {
                MaxHeight = 400,
                Content = new TextBlock
                {
                    Text = "• 更新会自动从 GitHub 下载并安装\n" +
                           "• 安装完成后应用会自动重启\n" +
                           "• 您可以选择跳过某个版本\n" +
                           "• 更新过程中可以随时取消\n" +
                           "• 如果更新失败，会自动恢复到之前版本",
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight = 24,
                    FontSize = 14
                }
            },
            CloseButtonText = "确定"
        };

        await dialog.ShowAsync();
    }
}
