using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using ImmersingPicker.Controls;
using ImmersingPicker.Core.Models;

namespace ImmersingPicker.Views.SettingsPages;

public partial class BasicSettingsPage : SettingsPageBase
{
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

    private void LaunchOnSystemStart_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        AppSettings.LaunchOnSystemStart = LaunchOnSystemStart.IsChecked ?? false;
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