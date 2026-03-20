using System;
using System.Collections.Generic;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using Serilog;

namespace ImmersingPicker.Services;

public class SettingsWindowNavigationService
{
    private static readonly ILogger _logger = Log.ForContext(typeof(SettingsWindowNavigationService));
    
    private static Frame? _mainContentFrame;

    private static List<ViewType> _pastPages = new();
    private static int _index = 0;

    public static void Initialize(Frame mainFrame)
    {
        _logger.Information("初始化设置窗口导航服务");
        _logger.Verbose("设置主内容框架");
        _mainContentFrame = mainFrame;
        _logger.Information("设置窗口导航服务初始化完成");
    }

    public static void NavigateTo(Type viewType)
    {
        _logger.Information("导航到设置视图: {ViewType}", viewType.Name);
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
        _logger.Information("导航到设置视图类型: {ViewType}", viewType);
        try
        {
            Type targetType = viewType switch
            {
                ViewType.BasicSettings => typeof(Views.SettingsPages.BasicSettingsPage),
                ViewType.PickerSettings => typeof(Views.SettingsPages.PickerSettingsPage),
                ViewType.HomePageSettings => typeof(Views.SettingsPages.HomePageSettingsPage),
                ViewType.About => typeof(Views.SettingsPages.AboutPage),
                ViewType.SecurityAndPrivacy => typeof(Views.SettingsPages.SecurityAndPrivacySettingsPage),
                ViewType.FloatingWindowSettings => typeof(Views.SettingsPages.FloatingWindowSettingsPage),
                ViewType.Linkage => typeof(Views.SettingsPages.LinkageSettingsPage),
                _ => throw new ArgumentException("Invalid view type")
            };
            _logger.Verbose("解析视图类型为: {TargetType}", targetType.Name);
            NavigateTo(targetType);
            _pastPages.Add(viewType);
        }
        catch (ArgumentException ex)
        {
            _logger.Error(ex, "无效的视图类型: {ViewType}", viewType);
        }
    }

    public static bool CanBack()
    {
        return _index != 0;
    }

    public static ViewType Back()
    {
        _index--;
        _pastPages.RemoveAt(_index + 1);
        return _pastPages[_index];
    }

    public enum ViewType
    {
        BasicSettings,
        PickerSettings,
        HomePageSettings,
        SecurityAndPrivacy,
        FloatingWindowSettings,
        Linkage,
        About
    }
}