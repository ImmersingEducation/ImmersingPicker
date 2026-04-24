using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
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

    private async void OnNextButtonClick(object? sender, RoutedEventArgs e)
    {
        var platformServices = PlatformServices.Instance;
        bool hasError = false;
        string errorMessage = "";

        if (DesktopShortcut.IsChecked == true)
        {
            bool success = platformServices.CreateDesktopShortcut();
            if (!success)
            {
                hasError = true;
                errorMessage = "创建桌面快捷方式失败，请检查权限设置后重试。";
            }
        }

        if (!hasError && StartMenuShortcut.IsChecked == true)
        {
            bool success = platformServices.CreateStartMenuShortcut();
            if (!success)
            {
                hasError = true;
                errorMessage = "创建开始菜单快捷方式失败，请检查权限设置后重试。";
            }
        }

        if (hasError)
        {
            var dialog = new ContentDialog
            {
                Title = "创建失败",
                Content = errorMessage,
                CloseButtonText = "确定"
            };
            
            var parentWindow = TopLevel.GetTopLevel(this) as Window;
            if (parentWindow != null)
            {
                await dialog.ShowAsync(parentWindow);
            }
        }
        else
        {
            WelcomeWindowNavigationService.Instance.NavigateTo(WelcomeWindowNavigationService.ViewType.Congratulation);
        }
    }
}
