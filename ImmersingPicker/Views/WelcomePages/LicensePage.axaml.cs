using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using ImmersingPicker.Core.Models;
using ImmersingPicker.Services;

namespace ImmersingPicker.Views.WelcomePages;

public partial class LicensePage : UserControl
{
    public LicensePage()
    {
        InitializeComponent();
        NextButton.Background = Brush.Parse(AppSettings.Instance.AppThemeColor);
    }

    private async void NextButton_OnClick(object? sender, RoutedEventArgs e)
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
            WelcomeWindowNavigationService.NavigateTo(WelcomeWindowNavigationService.ViewType.ClazzInitialization);
        }
    }
}