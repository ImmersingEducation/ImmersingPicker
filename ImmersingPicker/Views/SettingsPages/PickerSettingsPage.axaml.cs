using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ImmersingPicker.Controls;
using ImmersingPicker.Core.Models;

namespace ImmersingPicker.Views.SettingsPages;

public partial class PickerSettingsPage : SettingsPageBase
{
    public PickerSettingsPage()
    {
        InitializeComponent();
        PageTitle.Text = "抽选器设置";
        LoadSettings();
    }

    protected override void LoadSettings()
    {
        FairPickerMode.SelectedIndex = (int)AppSettings.Instance.FairPickerMode;
        WeightCalculationParam1.Value = Convert.ToDecimal(AppSettings.WeightCalculationParam1);
        WeightCalculationParam2.Value = Convert.ToDecimal(AppSettings.WeightCalculationParam2);
        WeightCalculationParam3.Value = Convert.ToDecimal(AppSettings.WeightCalculationParam3);
        WeightCalculationParam4.Value = Convert.ToDecimal(AppSettings.WeightCalculationParam4);
        WeightCalculationParam5.Value = Convert.ToDecimal(AppSettings.WeightCalculationParam5);
        WeightCalculationParam6.Value = Convert.ToDecimal(AppSettings.WeightCalculationParam6);
        WeightCalculationParam7.Value = Convert.ToDecimal(AppSettings.WeightCalculationParam7);
        WeightCalculationParam8.Value = Convert.ToDecimal(AppSettings.WeightCalculationParam8);
        WeightCalculationParam9.Value = Convert.ToDecimal(AppSettings.WeightCalculationParam9);
        WeightCalculationParam10.Value = Convert.ToDecimal(AppSettings.WeightCalculationParam10);
    }

    private void WeightCalculationParam1_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        AppSettings.WeightCalculationParam1 = Convert.ToDouble(WeightCalculationParam1.Value);
    }

    private void WeightCalculationParam2_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        AppSettings.WeightCalculationParam2 = Convert.ToDouble(WeightCalculationParam2.Value);
    }

    private void WeightCalculationParam3_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        AppSettings.WeightCalculationParam3 = Convert.ToInt32(WeightCalculationParam3.Value);
    }

    private void WeightCalculationParam4_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        AppSettings.WeightCalculationParam4 = Convert.ToDouble(WeightCalculationParam4.Value);
    }

    private void WeightCalculationParam5_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        AppSettings.WeightCalculationParam5 = Convert.ToDouble(WeightCalculationParam5.Value);
    }

    private void WeightCalculationParam6_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        AppSettings.WeightCalculationParam6 = Convert.ToDouble(WeightCalculationParam6.Value);
    }

    private void WeightCalculationParam7_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        AppSettings.WeightCalculationParam7 = Convert.ToInt32(WeightCalculationParam7.Value);
    }

    private void WeightCalculationParam8_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        AppSettings.WeightCalculationParam8 = Convert.ToInt32(WeightCalculationParam8.Value);
    }

    private void WeightCalculationParam9_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        AppSettings.WeightCalculationParam9 = Convert.ToInt32(WeightCalculationParam9.Value);
    }

    private void WeightCalculationParam10_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        AppSettings.WeightCalculationParam10 = Convert.ToInt32(WeightCalculationParam10.Value);
    }

    private void FairPickerMode_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        AppSettings.Instance.FairPickerMode = (AppSettings.FairPickerModeEnum)FairPickerMode.SelectedIndex;
    }
}
