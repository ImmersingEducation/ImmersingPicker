using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ImmersingPicker.Controls;
using ImmersingPicker.Core.Models;

namespace ImmersingPicker.Views.SettingsPages;

public partial class LinkageSettingsPage : SettingsPageBase
{
    public LinkageSettingsPage()
    {
        InitializeComponent();
        LoadSettings();
        UpdateControlsState();
    }

    protected override void LoadSettings()
    {
        EnableClassIslandLinkage.IsChecked = AppSettings.EnableClassIslandLinkage;
        EnableDisablingAfterClass.IsChecked = AppSettings.EnableDisablingAfterClasses;
    }

    protected override void UpdateControlsState()
    {
        bool classIslandLinkageEnabled = EnableClassIslandLinkage.IsChecked ?? false;
        EnableDisablingAfterClassItem.IsEnabled = classIslandLinkageEnabled;
    }

    private void EnableClassIslandLinkage_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        AppSettings.EnableClassIslandLinkage = EnableClassIslandLinkage.IsChecked ?? false;
        UpdateControlsState();
    }

    private void EnableDisablingAfterClasses_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        AppSettings.EnableDisablingAfterClasses = EnableDisablingAfterClass.IsChecked ?? false;
    }
}