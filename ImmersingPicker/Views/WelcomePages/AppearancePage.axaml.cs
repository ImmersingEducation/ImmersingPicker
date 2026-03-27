using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using ImmersingPicker.Controls;
using ImmersingPicker.Core.Models;
using ImmersingPicker.Services;

namespace ImmersingPicker.Views.WelcomePages;

public partial class AppearancePage : WelcomePageBase
{
    public AppearancePage()
    {
        InitializeComponent();
        NextButtonClick += OnNextButtonClick;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        LoadSettings();
    }

    protected override void LoadSettings()
    {
        AppTheme.SelectedIndex = AppSettings.AppTheme switch
        {
            AppSettings.ThemeEnums.System => 2,
            AppSettings.ThemeEnums.Light => 0,
            AppSettings.ThemeEnums.Dark => 1,
            _ => 2
        };

        if (!string.IsNullOrEmpty(AppSettings.AppThemeColor))
            try
            {
                AppThemeColor.Color = Color.Parse(AppSettings.AppThemeColor);
            }
            catch
            {
            }
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
        InitializeButtonTheme();
    }

    private void OnNextButtonClick(object? sender, RoutedEventArgs e)
    {
        WelcomeWindowNavigationService.Instance.NavigateTo(WelcomeWindowNavigationService.ViewType.Shortcut);
    }
}
