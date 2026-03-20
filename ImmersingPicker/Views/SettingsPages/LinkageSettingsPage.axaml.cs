using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ImmersingPicker.Core.Models;

namespace ImmersingPicker.Views.SettingsPages;

public partial class LinkageSettingsPage : UserControl
{
    public LinkageSettingsPage()
    {
        InitializeComponent();
        LoadSettings();
        UpdateControlsEnabledState();
    }

    private void LoadSettings()
    {
        EnableClassIslandLinkage.IsChecked = AppSettings.Instance.EnableClassIslandLinkage;
        EnableDisablingAfterClasses.IsChecked = AppSettings.Instance.EnableDisablingAfterClasses;
    }

    private void UpdateControlsEnabledState()
    {
        bool classIslandLinkageEnabled = EnableClassIslandLinkage.IsChecked ?? false;
        EnableDisablingAfterClasses.IsEnabled = classIslandLinkageEnabled;
    }

    private void EnableClassIslandLinkage_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        AppSettings.Instance.EnableClassIslandLinkage = EnableClassIslandLinkage.IsChecked ?? false;
        UpdateControlsEnabledState();
    }

    private void EnableDisablingAfterClasses_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        AppSettings.Instance.EnableDisablingAfterClasses = EnableDisablingAfterClasses.IsChecked ?? false;
    }
}