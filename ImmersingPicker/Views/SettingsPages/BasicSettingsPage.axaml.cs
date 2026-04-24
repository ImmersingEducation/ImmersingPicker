using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using ImmersingPicker.Controls;
using ImmersingPicker.Core.Models;
using ImmersingPicker.Services.Services;

namespace ImmersingPicker.Views.SettingsPages;

public partial class BasicSettingsPage : SettingsPageBase
{
    private bool _isUpdatingLaunchOnSystemStart;

    public BasicSettingsPage()
    {
        InitializeComponent();
        PageTitle.Text = "基础设置";
        LoadSettings();
    }

    protected override void LoadSettings()
    {
        // 加载开机自启动设置
        LaunchOnSystemStart.IsChecked = AppSettings.LaunchOnSystemStart;

        // 加载URL协议与IPC服务设置
        OpenUrlAndIpc.IsChecked = AppSettings.OpenUrlAndIpc;

        // 加载主题设置
        AppTheme.SelectedIndex = AppSettings.AppTheme switch
        {
            AppSettings.ThemeEnums.System => 2,
            AppSettings.ThemeEnums.Light => 0,
            AppSettings.ThemeEnums.Dark => 1,
            _ => 2
        };

        // 加载主题颜色设置
        if (!string.IsNullOrEmpty(AppSettings.AppThemeColor))
            try
            {
                AppThemeColor.Color = Color.Parse(AppSettings.AppThemeColor);
            }
            catch
            {
            }
    }

    private async void LaunchOnSystemStart_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        // 防止递归调用
        if (_isUpdatingLaunchOnSystemStart)
            return;

        bool isChecked = LaunchOnSystemStart.IsChecked ?? false;
        bool success = PlatformServices.Instance.AutoStart(isChecked);

        if (!success)
        {
            // 设置标志防止递归
            _isUpdatingLaunchOnSystemStart = true;
            try
            {
                // 创建失败，立即关闭开关
                LaunchOnSystemStart.IsChecked = false;
            }
            finally
            {
                _isUpdatingLaunchOnSystemStart = false;
            }

            // 弹窗提醒
            var dialog = new ContentDialog
            {
                Title = "设置失败",
                Content = "创建开机启动快捷方式失败，请检查权限设置后重试。",
                CloseButtonText = "确定"
            };

            var parentWindow = TopLevel.GetTopLevel(this) as Window;
            if (parentWindow != null)
            {
                await dialog.ShowAsync(parentWindow);
            }
        }
    }

    private void OpenUrlAndIpc_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        AppSettings.OpenUrlAndIpc = OpenUrlAndIpc.IsChecked ?? false;
    }

    private void AppTheme_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        AppSettings.AppTheme = AppTheme.SelectedIndex switch
        {
            0 => AppSettings.ThemeEnums.Light,
            1 => AppSettings.ThemeEnums.Dark,
            2 => AppSettings.ThemeEnums.System,
            _ => AppSettings.ThemeEnums.System
        };
    }

    private void AppThemeColor_OnColorChanged(object? sender, ColorChangedEventArgs e)
    {
        AppSettings.AppThemeColor = AppThemeColor.Color.ToString();
    }
}