using System;
using System.Collections.Generic;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using ImmersingPicker.Abstractions;
using Serilog;

namespace ImmersingPicker.Services;

public class SettingsWindowNavigationService : NavigationServiceBase
{
    public new static SettingsWindowNavigationService Instance { get; } = new SettingsWindowNavigationService();
    
    private readonly ILogger _logger = Log.ForContext(typeof(SettingsWindowNavigationService));
    
    public void NavigateTo(ViewType viewType)
    {
        _logger.Information("导航到设置视图类型: {ViewType}", viewType);
        try
        {
            Type targetType = viewType switch
            {
                ViewType.BasicSettings => typeof(Views.SettingsPages.BasicSettingsPage),
                ViewType.PickerSettings => typeof(Views.SettingsPages.PickerSettingsPage),
                ViewType.HomePageSettings => typeof(Views.SettingsPages.HomePageSettingsPage),
                ViewType.About => typeof(Views.SettingsPages.AboutPage),
                ViewType.SecurityAndPrivacySettings => typeof(Views.SettingsPages.SecurityAndPrivacySettingsPage),
                ViewType.FloatingWindowSettings => typeof(Views.SettingsPages.FloatingWindowSettingsPage),
                ViewType.LinkageSettings => typeof(Views.SettingsPages.LinkageSettingsPage),
                ViewType.UpdateSettings => typeof(Views.SettingsPages.UpdateSettingsPage),
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
        BasicSettings,
        PickerSettings,
        HomePageSettings,
        SecurityAndPrivacySettings,
        FloatingWindowSettings,
        LinkageSettings,
        UpdateSettings,
        About
    }
}