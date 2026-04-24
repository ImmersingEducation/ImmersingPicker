using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Windowing;
using ImmersingPicker.Abstractions;
using ImmersingPicker.Helpers;
using ImmersingPicker.Views;
using ImmersingPicker.Views.MainPages;
using Serilog;

namespace ImmersingPicker.Services;


public class MainWindowNavigationService : NavigationServiceBase
{
    public new static MainWindowNavigationService Instance { get; } = new MainWindowNavigationService();
    
    private readonly ILogger _logger = Log.ForContext(typeof(MainWindowNavigationService));
    
    public void NavigateTo(ViewType viewType)
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
                ViewType.Home => typeof(HomePage),
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

    public async Task OpenSettingsWindow()
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

    public async Task OpenEditorWindow()
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
        Home, History, Editor, Settings
    }
}