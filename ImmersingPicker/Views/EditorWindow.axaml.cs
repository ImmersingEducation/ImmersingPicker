using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Windowing;
using ImmersingPicker.Views.EditorPages;

namespace ImmersingPicker.Views;

public partial class EditorWindow : AppWindow
{
    public EditorWindow()
    {
        InitializeComponent();
        Frame.Navigate(typeof(EditPage));
    }
}