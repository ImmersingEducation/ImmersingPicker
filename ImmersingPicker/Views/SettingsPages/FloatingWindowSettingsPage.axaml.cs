using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using ImmersingPicker.Controls;
using ImmersingPicker.Core.Models;

namespace ImmersingPicker.Views.SettingsPages;

public partial class FloatingWindowSettingsPage : SettingsPageBase
{
    public FloatingWindowSettingsPage()
    {
        InitializeComponent();
        PageTitle.Text = "浮窗设置";
        LoadSettings();
        UpdateControlsState();
    }

    protected override void LoadSettings()
    {
        // 加载浮窗启用状态
        FloatingWindowEnabled.IsChecked = AppSettings.FloatingWindowEnabled;

        // 加载停靠位置设置
        FloatingWindowDockPosition.SelectedIndex = AppSettings.FloatingWindowDockPosition switch
        {
            AppSettings.FloatingWindowDockPositionMode.Left => 0,
            AppSettings.FloatingWindowDockPositionMode.Right => 1,
            _ => 1
        };

        // 加载竖向位置设置
        FloatingWindowVerticalPosition.Value = AppSettings.FloatingWindowVerticalPosition;
    }

    protected override void UpdateControlsState()
    {
        bool enabled = FloatingWindowEnabled.IsChecked ?? false;
        FloatingWindowDockPosition.IsEnabled = enabled;
        FloatingWindowVerticalPosition.IsEnabled = enabled;
    }

    private void FloatingWindowEnabled_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        AppSettings.FloatingWindowEnabled = FloatingWindowEnabled.IsChecked ?? false;
        UpdateControlsState();
    }

    private void FloatingWindowDockPosition_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        AppSettings.FloatingWindowDockPosition = FloatingWindowDockPosition.SelectedIndex switch
        {
            0 => AppSettings.FloatingWindowDockPositionMode.Left,
            1 => AppSettings.FloatingWindowDockPositionMode.Right,
            _ => AppSettings.FloatingWindowDockPositionMode.Right
        };
    }

    private void FloatingWindowVerticalPosition_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        AppSettings.FloatingWindowVerticalPosition = (int)FloatingWindowVerticalPosition.Value;
    }
}
