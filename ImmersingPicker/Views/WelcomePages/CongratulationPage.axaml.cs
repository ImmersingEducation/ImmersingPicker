using Avalonia;
using Avalonia.Interactivity;
using ImmersingPicker.Controls;

namespace ImmersingPicker.Views.WelcomePages;

public partial class CongratulationPage : WelcomePageBase
{
    public CongratulationPage()
    {
        InitializeComponent();
        NextButtonClick += OnNextButtonClick;
    }

    private void OnNextButtonClick(object? sender, RoutedEventArgs e)
    {
        if (Application.Current is App app)
        {
            app.CompleteWelcomeSetup();
        }
    }
}
