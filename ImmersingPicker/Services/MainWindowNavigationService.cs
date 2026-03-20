using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Windowing;
using ImmersingPicker.Helpers;
using ImmersingPicker.Views;
using Serilog;

namespace ImmersingPicker.Services;

public class MainWindowNavigationService
{
    private static readonly ILogger _logger = Log.ForContext(typeof(MainWindowNavigationService));

    private static Frame? _mainContentFrame;
    private static AppWindow? _mainWindow;

    public static void Initialize(Frame mainFrame, AppWindow mainWindow)
    {
        _logger.Information("初始化主窗口导航服务");
        _logger.Verbose("设置主内容框架");
        _mainContentFrame = mainFrame;
        _mainWindow = mainWindow;
        _logger.Information("主窗口导航服务初始化完成");
    }

    public static void NavigateTo(Type viewType)
    {
        _logger.Information("导航到视图: {ViewType}", viewType.Name);
        if (_mainContentFrame != null)
        {
            try
            {
                _logger.Verbose("创建视图实例");
                if (Activator.CreateInstance(viewType) is UserControl view)
                {
                    _logger.Verbose("设置视图为框架内容");
                    _mainContentFrame.Content = view;
                    _logger.Information("导航成功");
                }
                else
                {
                    _logger.Warning("无法创建视图实例: {ViewType}", viewType.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "导航失败");
            }
        }
        else
        {
            _logger.Error("主内容框架未初始化");
        }
    }

    public static void NavigateTo(ViewType viewType)
    {
        _logger.Information("导航到视图类型: {ViewType}", viewType);
        if (viewType == ViewType.Settings)
        {
            _logger.Verbose("打开设置窗口");
            OpenSettingsWindow();
            return;
        }
        if (viewType == ViewType.Editor)
        {
            _logger.Verbose("打开编辑器窗口");
            OpenEditorWindow();
            return;
        }

        try
        {
            Type targetType = viewType switch
            {
                ViewType.Home => typeof(Views.MainPages.HomePage),
                ViewType.History => typeof(Views.MainPages.HistoryPage),
                _ => throw new ArgumentException("Invalid view type")
            };
            _logger.Verbose("解析视图类型为: {TargetType}", targetType.Name);
            NavigateTo(targetType);
        }
        catch (ArgumentException ex)
        {
            _logger.Error(ex, "无效的视图类型: {ViewType}", viewType);
        }
    }

    public static async Task OpenSettingsWindow()
    {
        _logger.Information("打开设置窗口");
        if (_mainWindow == null)
        {
            _logger.Error("主窗口未初始化");
            return;
        }

        bool verified = await VerifyHelper.VerifyPassword(_mainWindow);
        if (!verified)
        {
            _logger.Information("验证失败，取消打开设置窗口");
            return;
        }

        try
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.Show();
            _logger.Information("设置窗口打开成功");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "打开设置窗口失败");
        }
    }

    public static async Task OpenEditorWindow()
    {
        _logger.Information("打开编辑器窗口");
        if (_mainWindow == null)
        {
            _logger.Error("主窗口未初始化");
            return;
        }

        bool verified = await VerifyHelper.VerifyPassword(_mainWindow);
        if (!verified)
        {
            _logger.Information("验证失败，取消打开编辑器窗口");
            return;
        }

        try
        {
            var editorWindow = new EditorWindow();
            editorWindow.Show();
            _logger.Information("编辑器窗口打开成功");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "打开编辑器窗口失败");
        }
    }

    public enum ViewType
    {
        Home, History, Settings,
        Editor
    }
}