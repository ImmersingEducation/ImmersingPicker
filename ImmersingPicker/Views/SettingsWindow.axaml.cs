using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Windowing;
using ImmersingPicker.Abstractions;
using ImmersingPicker.Services;
using Serilog;

namespace ImmersingPicker.Views;

public partial class SettingsWindow : AppWindow
{
    private static readonly ILogger _logger = Log.ForContext<SettingsWindow>();

    public SettingsWindow()
    {
        _logger.Information("初始化 SettingsWindow");
        InitializeComponent();

        _logger.Verbose("设置 TitleBar 属性");
        TitleBar.Height = 36;
        TitleBar.ExtendsContentIntoTitleBar = false;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;

        _logger.Verbose("初始化设置窗口导航服务");
        SettingsWindowNavigationService.Instance.Initialize(ContentFrame, this);
        _logger.Information("SettingsWindow 初始化完成");

        MainNavView.SelectedItem = MainNavView.MenuItems[0];
    }

    private void MainNavView_OnSelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e)
    {
        if (e.SelectedItem is not NavigationViewItem { Tag: string tag }) return;

        _logger.Information("设置窗口导航选择变更：{Tag}", tag);
        var viewType = tag switch
        {
            "BasicSettings" => Services.SettingsWindowNavigationService.ViewType.BasicSettings,
            "PickerSettings" => Services.SettingsWindowNavigationService.ViewType.PickerSettings,
            "HomePageSettings" => Services.SettingsWindowNavigationService.ViewType.HomePageSettings,
            "SecurityAndPrivacySettings" => Services.SettingsWindowNavigationService.ViewType.SecurityAndPrivacySettings,
            "FloatingWindowSettings" => Services.SettingsWindowNavigationService.ViewType.FloatingWindowSettings,
            "LinkageSettings" => Services.SettingsWindowNavigationService.ViewType.LinkageSettings,
            "UpdateSettings" => Services.SettingsWindowNavigationService.ViewType.UpdateSettings,
            "About" => Services.SettingsWindowNavigationService.ViewType.About,
            _ => throw new ArgumentException("Invalid view type")
        };
        _logger.Debug("导航到视图类型：{ViewType}", viewType);
        Services.SettingsWindowNavigationService.Instance.NavigateTo(viewType);
    }
}