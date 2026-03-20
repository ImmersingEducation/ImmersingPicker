using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using ImmersingPicker.Core.Models;
using ImmersingPicker.Services;
using ImmersingPicker.Services.Services;

namespace ImmersingPicker.Views.WelcomePages;

public partial class WelcomePage : UserControl
{
    public WelcomePage()
    {
        InitializeComponent();
        VersionText.Text = VersionServices.VersionString(VersionServices.CurrentVersion);
        StartButton.Background = Brush.Parse(AppSettings.Instance.AppThemeColor);
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
        WelcomeWindowNavigationService.NavigateTo(WelcomeWindowNavigationService.ViewType.License);
    }
}