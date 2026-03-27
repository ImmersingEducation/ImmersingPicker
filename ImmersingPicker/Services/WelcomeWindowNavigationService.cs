using System;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using ImmersingPicker.Abstractions;
using Serilog;

namespace ImmersingPicker.Services;

public class WelcomeWindowNavigationService : NavigationServiceBase
{
    public new static WelcomeWindowNavigationService Instance { get; } = new WelcomeWindowNavigationService();
    
    private readonly ILogger _logger = Log.ForContext(typeof(WelcomeWindowNavigationService));
    
    public void NavigateTo(ViewType viewType)
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
                ViewType.Shortcut => typeof(Views.WelcomePages.ShortcutPage),
                ViewType.Congratulation => typeof(Views.WelcomePages.CongratulationPage),
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
        Welcome, License, ClazzInitialization, Appearance, Shortcut, Congratulation
    }
}