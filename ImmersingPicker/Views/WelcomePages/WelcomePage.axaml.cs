using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using ImmersingPicker.Controls;
using ImmersingPicker.Services;
using ImmersingPicker.Services.Services;

namespace ImmersingPicker.Views.WelcomePages;

public partial class WelcomePage : WelcomePageBase
{
    public WelcomePage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (VersionText is not null)
        {
            VersionText.Text = VersionServices.VersionString(VersionServices.CurrentVersion);
        }
        if (StartButton != null && !string.IsNullOrEmpty(AppSettings.AppThemeColor))
        {
            try
            {
                StartButton.Background = Brush.Parse(AppSettings.AppThemeColor);
            }
            catch
            {
            }
        }
    }

    private void GithubHyperlinkButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c start \"\" \"https://github.com/ImmersingEducation/ImmersingPicker\"",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("xdg-open", "https://github.com/ImmersingEducation/ImmersingPicker");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start("open", "https://github.com/ImmersingEducation/ImmersingPicker");
        }
    }

    private void StartButton_OnClick(object? sender, RoutedEventArgs e)
    {
        WelcomeWindowNavigationService.Instance.NavigateTo(WelcomeWindowNavigationService.ViewType.License);
    }
}
