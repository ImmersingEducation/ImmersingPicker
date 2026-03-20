using System;
using System.Diagnostics;
using System.Linq;
using Avalonia.Interactivity;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Windowing;
using ImmersingPicker.Services;
using Serilog;

namespace ImmersingPicker.Views;

public partial class MainWindow : AppWindow
{
    private static readonly ILogger _logger = Log.ForContext(typeof(MainWindow));
    
    public MainWindow()
    {
        _logger.Information("主窗口初始化开始");
        _logger.Verbose("开始初始化 MainWindow 组件");
        InitializeComponent();

        _logger.Verbose("设置 TitleBar 属性");
        TitleBar.Height = 36;
        TitleBar.ExtendsContentIntoTitleBar = false;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;

        _logger.Verbose("初始化导航服务");
        MainWindowNavigationService.Initialize(ContentFrame, this);
        _logger.Information("主窗口导航服务初始化完成");
        
        // Closing 事件在 App.axaml.cs 中统一处理
        _logger.Information("主窗口初始化完成");
    }

    private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        MainNavView.SelectedItem = HomePageItem;
    }

    private void MainNavView_OnSelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs? e)
    {
        if (e is not { SelectedItem: NavigationViewItem { Tag: string tag } }) return;
        var viewType = tag switch
        {
            "Home" => MainWindowNavigationService.ViewType.Home,
            "History" => MainWindowNavigationService.ViewType.History,
            "Editor" => MainWindowNavigationService.ViewType.Editor,
            "Settings" => MainWindowNavigationService.ViewType.Settings,
            _ => throw new ArgumentException("Invalid view type")
        };
        MainWindowNavigationService.NavigateTo(viewType);
        if (viewType is MainWindowNavigationService.ViewType.Settings or MainWindowNavigationService.ViewType.Editor)
        {
            MainNavView.SelectedItem = HomePageItem;
        }
    }
}