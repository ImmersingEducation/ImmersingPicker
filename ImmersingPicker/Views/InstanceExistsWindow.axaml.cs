using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Windowing;

namespace ImmersingPicker.Views;

public partial class InstanceExistsWindow : AppWindow
{
    public InstanceExistsWindow()
    {
        InitializeComponent();
    }

    private void OkButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }
}
