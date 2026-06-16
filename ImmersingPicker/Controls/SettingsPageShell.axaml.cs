using Avalonia;
using Avalonia.Controls;

namespace ImmersingPicker.Controls;

public partial class SettingsPageShell : ContentControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<SettingsPageShell, string>(nameof(Title));

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public SettingsPageShell()
    {
        InitializeComponent();
    }
}
