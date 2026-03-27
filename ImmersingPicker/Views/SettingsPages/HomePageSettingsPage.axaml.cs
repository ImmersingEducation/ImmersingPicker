using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ImmersingPicker.Controls;
using ImmersingPicker.Core.Models;

namespace ImmersingPicker.Views.SettingsPages;

public partial class HomePageSettingsPage : SettingsPageBase
{
    public HomePageSettingsPage()
    {
        InitializeComponent();
        PageTitle.Text = "主界面设置";
        LoadSettings();
    }

    protected override void LoadSettings()
    {
        PickAnimationPlayMode.SelectedIndex = (int)AppSettings.PickAnimationPlayMode;
        HomeAnimationPlayAmount.Value = Convert.ToDecimal(AppSettings.HomeAnimationPlayAmount);
        HomeAnimationPlayDelay.Value = Convert.ToDecimal(AppSettings.HomeAnimationPlayDelay);
        SeatGridRowArrangement.SelectedIndex = (int)AppSettings.SeatGridRowArrangement;
        SeatGridColumnArrangement.SelectedIndex = (int)AppSettings.SeatGridColumnArrangement;
    }

    private void UpdateControlsState()
    {
        if (PickAnimationPlayMode.SelectedIndex == 2)
        {
            HomeAnimationPlayAmountItem.IsEnabled = false;
            HomeAnimationPlayDelayItem.IsEnabled = false;
        }
        else
        {
            HomeAnimationPlayAmountItem.IsEnabled = true;
            HomeAnimationPlayDelayItem.IsEnabled = true;
        }
    }

    private void HomeAnimationPlayAmount_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        AppSettings.HomeAnimationPlayAmount = Convert.ToInt32(HomeAnimationPlayAmount.Value);
    }

    private void HomeAnimationPlayDelay_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        AppSettings.HomeAnimationPlayDelay = Convert.ToInt32(HomeAnimationPlayDelay.Value);
    }

    private void SeatGridRowArrangement_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        AppSettings.SeatGridRowArrangement = (AppSettings.SeatGridRowArrangementMode)SeatGridRowArrangement.SelectedIndex;
    }

    private void SeatGridColumnArrangement_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        AppSettings.SeatGridColumnArrangement = (AppSettings.SeatGridColumnArrangementMode)SeatGridColumnArrangement.SelectedIndex;
    }

    private void PickAnimationPlayMode_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        AppSettings.PickAnimationPlayMode = (AppSettings.PickAnimationPlayModeEnum)PickAnimationPlayMode.SelectedIndex;
        UpdateControlsState();
    }
}