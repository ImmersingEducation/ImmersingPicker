using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using Avalonia.Media;
using FluentAvalonia.Styling;
using ImmersingPicker.Core.Models;

namespace ImmersingPicker.Helpers;

public class ThemeManager
{
    public static ThemeManager Instance { get; } = new ThemeManager();
    
    private ThemeManager()
    {
        // 订阅AppSettings的主题变更事件
        AppSettings.Instance.AppThemeChanged += OnAppThemeChanged;
        AppSettings.Instance.AppThemeColorChanged += OnAppThemeColorChanged;
    }
    
    /// <summary>
    /// 初始化主题
    /// </summary>
    public void Initialize()
    {
        UpdateTheme();
        UpdateThemeColor();
    }
    
    /// <summary>
    /// 更新应用主题
    /// </summary>
    public void UpdateTheme()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime)
        {
            var theme = AppSettings.Instance.AppTheme;
            ThemeVariant variant;
            
            switch (theme)
            {
                case AppSettings.ThemeEnums.Light:
                    variant = ThemeVariant.Light;
                    break;
                case AppSettings.ThemeEnums.Dark:
                    variant = ThemeVariant.Dark;
                    break;
                default: // System
                    variant = ThemeVariant.Default;
                    break;
            }
            
            Application.Current.RequestedThemeVariant = variant;
        }
    }
    
    /// <summary>
    /// 更新应用主题色
    /// </summary>
    public void UpdateThemeColor()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime)
        {
            var themeColor = AppSettings.Instance.AppThemeColor;
            if (!string.IsNullOrEmpty(themeColor))
            {
                try
                {
                    // 将字符串转换为Color类型
                    var color = Color.Parse(themeColor);
                    
                    // 更新多个相关的资源键，确保主题色能够正确应用
                    Application.Current.Resources["SystemAccentColor"] = color;
                    Application.Current.Resources["SystemAccentColorLight1"] = color;
                    Application.Current.Resources["SystemAccentColorLight2"] = color;
                    Application.Current.Resources["SystemAccentColorLight3"] = color;
                    Application.Current.Resources["SystemAccentColorDark1"] = color;
                    Application.Current.Resources["SystemAccentColorDark2"] = color;
                    Application.Current.Resources["SystemAccentColorDark3"] = color;
                }
                catch (Exception ex)
                {
                    // 处理颜色解析错误
                    System.Diagnostics.Debug.WriteLine($"主题色解析错误: {ex.Message}");
                }
            }
        }
    }
    
    /// <summary>
    /// 处理主题变更事件
    /// </summary>
    private void OnAppThemeChanged(AppSettings.ThemeEnums theme)
    {
        UpdateTheme();
    }
    
    /// <summary>
    /// 处理主题色变更事件
    /// </summary>
    private void OnAppThemeColorChanged(string color)
    {
        UpdateThemeColor();
    }
}