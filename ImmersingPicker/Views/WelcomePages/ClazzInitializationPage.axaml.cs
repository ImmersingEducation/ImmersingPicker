using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using ImmersingPicker.Controls;
using ImmersingPicker.Services;
using ImmersingPicker.Services.Services.Picker;

namespace ImmersingPicker.Views.WelcomePages;

public partial class ClazzInitializationPage : WelcomePageBase
{
    public ClazzInitializationPage()
    {
        InitializeComponent();
        NextButtonClick += OnNextButtonClick;
    }

    private async void OnNextButtonClick(object? sender, RoutedEventArgs e)
    {
        if ((FirstClazzNameInput.Text ?? "") == "")
        {
            var dialog = new ContentDialog
            {
                Title = "请输入班级名称",
                Content = "班级名称不能为空",
                CloseButtonText = "确定"
            };
            await dialog.ShowAsync();
        }
        else
        {
            ClazzFactory.NewClazz(FirstClazzNameInput.Text);
            WelcomeWindowNavigationService.Instance.NavigateTo(WelcomeWindowNavigationService.ViewType.Appearance);
        }
    }
}
