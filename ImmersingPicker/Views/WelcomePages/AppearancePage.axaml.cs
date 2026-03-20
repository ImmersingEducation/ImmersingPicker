using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using ImmersingPicker.Core.Models;

namespace ImmersingPicker.Views.WelcomePages;

public partial class AppearancePage : UserControl
{
    public AppearancePage()
    {
        InitializeComponent();
        NextButton.Background = Brush.Parse(AppSettings.Instance.AppThemeColor);
        LoadSettings();
    }

    private void LoadSettings()
    {
        AppTheme.SelectedIndex = AppSettings.Instance.AppTheme switch
        {
            AppSettings.ThemeEnums.System => 2,
            AppSettings.ThemeEnums.Light => 0,
            AppSettings.ThemeEnums.Dark => 1,
            _ => 2
        };

        if (!string.IsNullOrEmpty(AppSettings.Instance.AppThemeColor))
            try
            {
                AppThemeColor.Color = Color.Parse(AppSettings.Instance.AppThemeColor);
            }
            catch
            {
            }
    }

    private void AppTheme_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        AppSettings.Instance.AppTheme = AppTheme.SelectedIndex switch
        {
            0 => AppSettings.ThemeEnums.Light,
            1 => AppSettings.ThemeEnums.Dark,
            2 => AppSettings.ThemeEnums.System,
            _ => AppSettings.ThemeEnums.System
        };
    }

    private void AppThemeColor_OnColorChanged(object? sender, ColorChangedEventArgs e)
    {
        AppSettings.Instance.AppThemeColor = AppThemeColor.Color.ToString();
        NextButton.Background = Brush.Parse(AppSettings.Instance.AppThemeColor);
    }

    private void NextButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Application.Current is App app)
        {
            app.CompleteWelcomeSetup();
        }
    }
}