using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using ImmersingPicker.Core.Models;
using ImmersingPicker.Services;
using ImmersingPicker.Services.Services.Picker;

namespace ImmersingPicker.Views.WelcomePages;

public partial class ClazzInitializationPage : UserControl
{
    public ClazzInitializationPage()
    {
        InitializeComponent();
        NextButton.Background = Brush.Parse(AppSettings.Instance.AppThemeColor);
    }

    private async void NextButton_OnClick(object? sender, RoutedEventArgs e)
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
            // 使用 ClazzFactory.NewClazz 创建班级，自动创建抽选器
            ClazzFactory.NewClazz(FirstClazzNameInput.Text);
            WelcomeWindowNavigationService.NavigateTo(WelcomeWindowNavigationService.ViewType.Appearance);
        }
    }
}