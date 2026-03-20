using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ImmersingPicker.Core.Models;

namespace ImmersingPicker.Views.SettingsPages;

public partial class PickerSettingsPage : UserControl
{
    public PickerSettingsPage()
    {
        InitializeComponent();

        WeightCalculationParam1.Value = Convert.ToDecimal(AppSettings.Instance.WeightCalculationParam1);
        WeightCalculationParam2.Value = Convert.ToDecimal(AppSettings.Instance.WeightCalculationParam2);
        WeightCalculationParam3.Value = Convert.ToDecimal(AppSettings.Instance.WeightCalculationParam3);
        WeightCalculationParam4.Value = Convert.ToDecimal(AppSettings.Instance.WeightCalculationParam4);
        WeightCalculationParam5.Value = Convert.ToDecimal(AppSettings.Instance.WeightCalculationParam5);
        WeightCalculationParam6.Value = Convert.ToDecimal(AppSettings.Instance.WeightCalculationParam6);
        WeightCalculationParam7.Value = Convert.ToDecimal(AppSettings.Instance.WeightCalculationParam7);
        WeightCalculationParam8.Value = Convert.ToDecimal(AppSettings.Instance.WeightCalculationParam8);
        WeightCalculationParam9.Value = Convert.ToDecimal(AppSettings.Instance.WeightCalculationParam9);
        WeightCalculationParam10.Value = Convert.ToDecimal(AppSettings.Instance.WeightCalculationParam10);
    }

    private void WeightCalculationParam1_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        AppSettings.Instance.WeightCalculationParam1 = Convert.ToDouble(WeightCalculationParam1.Value);
    }

    private void WeightCalculationParam2_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        AppSettings.Instance.WeightCalculationParam2 = Convert.ToDouble(WeightCalculationParam2.Value);
    }

    private void WeightCalculationParam3_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        AppSettings.Instance.WeightCalculationParam3 = Convert.ToInt32(WeightCalculationParam3.Value);
    }

    private void WeightCalculationParam4_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        AppSettings.Instance.WeightCalculationParam4 = Convert.ToDouble(WeightCalculationParam4.Value);
    }

    private void WeightCalculationParam5_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        AppSettings.Instance.WeightCalculationParam5 = Convert.ToDouble(WeightCalculationParam5.Value);
    }

    private void WeightCalculationParam6_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        AppSettings.Instance.WeightCalculationParam6 = Convert.ToDouble(WeightCalculationParam6.Value);
    }

    private void WeightCalculationParam7_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        AppSettings.Instance.WeightCalculationParam7 = Convert.ToInt32(WeightCalculationParam7.Value);
    }

    private void WeightCalculationParam8_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        AppSettings.Instance.WeightCalculationParam8 = Convert.ToInt32(WeightCalculationParam8.Value);
    }

    private void WeightCalculationParam9_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        AppSettings.Instance.WeightCalculationParam9 = Convert.ToInt32(WeightCalculationParam9.Value);
    }

    private void WeightCalculationParam10_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        AppSettings.Instance.WeightCalculationParam10 = Convert.ToInt32(WeightCalculationParam10.Value);
    }
}
