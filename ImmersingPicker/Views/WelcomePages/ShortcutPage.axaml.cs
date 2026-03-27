using Avalonia.Interactivity;
using ImmersingPicker.Controls;
using ImmersingPicker.Services;
using ImmersingPicker.Services.Services;

namespace ImmersingPicker.Views.WelcomePages;

public partial class ShortcutPage : WelcomePageBase
{
    public ShortcutPage()
    {
        InitializeComponent();
        NextButtonClick += OnNextButtonClick;
    }

    private void OnNextButtonClick(object? sender, RoutedEventArgs e)
    {
        var platformServices = PlatformServices.Instance;

        if (DesktopShortcut.IsChecked == true)
        {
            platformServices.CreateDesktopShortcut();
        }

        if (StartMenuShortcut.IsChecked == true)
        {
            platformServices.CreateStartMenuShortcut();
        }

        WelcomeWindowNavigationService.Instance.NavigateTo(WelcomeWindowNavigationService.ViewType.Congratulation);
    }
}
