using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using FluentAvalonia.UI.Windowing;
using ImmersingPicker.Core.Models;
using ImmersingPicker.Services;
using ImmersingPicker.Services.Services;
using ImmersingPicker.Views.WelcomePages;

namespace ImmersingPicker.Views;

public partial class WelcomeWindow : AppWindow
{
    public WelcomeWindow()
    {
        InitializeComponent();
        WelcomeWindowNavigationService.Initialize(ContentFrame);
        WelcomeWindowNavigationService.NavigateTo(WelcomeWindowNavigationService.ViewType.Welcome);
    }
}