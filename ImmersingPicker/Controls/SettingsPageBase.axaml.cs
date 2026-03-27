using Avalonia.Controls;
using ImmersingPicker.Core.Models;

namespace ImmersingPicker.Controls;

public partial class SettingsPageBase : UserControl
{
    protected AppSettings AppSettings => AppSettings.Instance;

    public SettingsPageBase()
    {
        InitializeComponent();
    }

    protected virtual void LoadSettings()
    {
    }

    protected virtual void UpdateControlsState()
    {
    }
}
