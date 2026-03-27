using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using ImmersingPicker.Controls;
using ImmersingPicker.Services;

namespace ImmersingPicker.Views.WelcomePages;

public partial class LicensePage : WelcomePageBase
{
    public LicensePage()
    {
        InitializeComponent();
        NextButtonClick += OnNextButtonClick;
    }

    private async void OnNextButtonClick(object? sender, RoutedEventArgs e)
    {
        if (!(FreeSoftwareLicense.IsChecked ?? false))
        {
            var dialog = new ContentDialog
            {
                Title = "请同意协议",
                Content = "您需要阅读并同意以下许可条款才可以继续使用 ImmersingPicker。（再次强调",
                CloseButtonText = "确定"
            };
            await dialog.ShowAsync();
        }
        else
        {
            WelcomeWindowNavigationService.Instance.NavigateTo(WelcomeWindowNavigationService.ViewType.ClazzInitialization);
        }
    }
}
