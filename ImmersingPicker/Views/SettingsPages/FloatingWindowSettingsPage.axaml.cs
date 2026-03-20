using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using ImmersingPicker.Core.Models;

namespace ImmersingPicker.Views.SettingsPages;

public partial class FloatingWindowSettingsPage : UserControl
{
    public FloatingWindowSettingsPage()
    {
        InitializeComponent();
        LoadSettings();
        UpdateControlsEnabledState();
    }

    private void LoadSettings()
    {
        // 加载浮窗启用状态
        FloatingWindowEnabled.IsChecked = AppSettings.Instance.FloatingWindowEnabled;

        // 加载停靠位置设置
        FloatingWindowDockPosition.SelectedIndex = AppSettings.Instance.FloatingWindowDockPosition switch
        {
            AppSettings.FloatingWindowDockPositionMode.Left => 0,
            AppSettings.FloatingWindowDockPositionMode.Right => 1,
            _ => 1
        };

        // 加载竖向位置设置
        FloatingWindowVerticalPosition.Value = AppSettings.Instance.FloatingWindowVerticalPosition;
    }

    private void UpdateControlsEnabledState()
    {
        bool enabled = FloatingWindowEnabled.IsChecked ?? false;
        FloatingWindowDockPosition.IsEnabled = enabled;
        FloatingWindowVerticalPosition.IsEnabled = enabled;
    }

    private void FloatingWindowEnabled_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        AppSettings.Instance.FloatingWindowEnabled = FloatingWindowEnabled.IsChecked ?? false;
        UpdateControlsEnabledState();
    }

    private void FloatingWindowDockPosition_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        AppSettings.Instance.FloatingWindowDockPosition = FloatingWindowDockPosition.SelectedIndex switch
        {
            0 => AppSettings.FloatingWindowDockPositionMode.Left,
            1 => AppSettings.FloatingWindowDockPositionMode.Right,
            _ => AppSettings.FloatingWindowDockPositionMode.Right
        };
    }

    private void FloatingWindowVerticalPosition_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        AppSettings.Instance.FloatingWindowVerticalPosition = (int)FloatingWindowVerticalPosition.Value;
    }
}
