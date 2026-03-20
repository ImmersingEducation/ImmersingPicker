using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ImmersingPicker.Core.Models;

namespace ImmersingPicker.Views.SettingsPages;

public partial class HomePageSettingsPage : UserControl
{
    public HomePageSettingsPage()
    {
        InitializeComponent();

        HomeAnimationPlayAmount.Value = Convert.ToDecimal(AppSettings.Instance.HomeAnimationPlayAmount);
        HomeAnimationPlayDelay.Value = Convert.ToDecimal(AppSettings.Instance.HomeAnimationPlayDelay);
        SeatGridRowArrangement.SelectedIndex = (int)AppSettings.Instance.SeatGridRowArrangement;
        SeatGridColumnArrangement.SelectedIndex = (int)AppSettings.Instance.SeatGridColumnArrangement;
    }

    private void HomeAnimationPlayAmount_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        AppSettings.Instance.HomeAnimationPlayAmount = Convert.ToInt32(HomeAnimationPlayAmount.Value);
    }

    private void HomeAnimationPlayDelay_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        AppSettings.Instance.HomeAnimationPlayDelay = Convert.ToInt32(HomeAnimationPlayDelay.Value);
    }

    private void SeatGridRowArrangement_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        AppSettings.Instance.SeatGridRowArrangement = (AppSettings.SeatGridRowArrangementMode)SeatGridRowArrangement.SelectedIndex;
    }

    private void SeatGridColumnArrangement_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        AppSettings.Instance.SeatGridColumnArrangement = (AppSettings.SeatGridColumnArrangementMode)SeatGridColumnArrangement.SelectedIndex;
    }
}