using System;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using Serilog;

namespace ImmersingPicker.Services;

public class WelcomeWindowNavigationService
{
    private static readonly ILogger _logger = Log.ForContext(typeof(WelcomeWindowNavigationService));
    
    private static Frame? _mainContentFrame;

    public static void Initialize(Frame mainFrame)
    {
        _logger.Information("初始化欢迎窗口导航服务");
        _logger.Verbose("设置主内容框架");
        _mainContentFrame = mainFrame;
        _logger.Information("设置窗口导航服务初始化完成");
    }

    public static void NavigateTo(Type viewType)
    {
        _logger.Information("导航到欢迎视图: {ViewType}", viewType.Name);
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

            }
        }
        else
        {
            _logger.Error("主内容框架未初始化");
        }
    }

    public static void NavigateTo(ViewType viewType)
    {
        _logger.Information("导航到欢迎视图类型: {ViewType}", viewType);
        try
        {
            Type targetType = viewType switch
            {
                ViewType.Welcome => typeof(Views.WelcomePages.WelcomePage),
                ViewType.License => typeof(Views.WelcomePages.LicensePage),
                ViewType.ClazzInitialization => typeof(Views.WelcomePages.ClazzInitializationPage),
                ViewType.Appearance => typeof(Views.WelcomePages.AppearancePage),
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

    public enum ViewType
    {
        Welcome, License, ClazzInitialization, Appearance
    }
}