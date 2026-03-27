using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ImmersingPicker.Controls;
using ImmersingPicker.Services.Services;

namespace ImmersingPicker.Views.SettingsPages;

public partial class AboutPage : SettingsPageBase
{
    public AboutPage()
    {
        InitializeComponent();
        PageTitle.Text = "关于";
        VersionText.Text = VersionServices.VersionString(VersionServices.CurrentVersion);
    }

    private void RestartButton_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // 调用 App 的重启方法
        if (Application.Current is App app)
        {
            app.RestartApplication(sender, e);
        }
    }

    private void ExitButton_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // 调用 App 的退出方法
        if (Application.Current is App app)
        {
            app.ExitApplication(sender, e);
        }
    }
}
