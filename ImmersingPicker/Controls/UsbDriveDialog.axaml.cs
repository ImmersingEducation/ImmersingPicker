using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ImmersingPicker.Core.Models;
using ImmersingPicker.Services.Services.Security;

namespace ImmersingPicker.Controls;

public partial class UsbDriveDialog : UserControl
{
    private UsbDriveInfo? _selectedDrive;
    private UsbDriveInfo? _currentDrive;

    public UsbDriveDialog()
    {
        InitializeComponent();
        RefreshButton.Click += RefreshButton_OnClick;
    }

    public void SetCurrentDrive(UsbDriveInfo? drive)
    {
        _currentDrive = drive;
        if (drive.HasValue)
        {
            CurrentUsbDriveText.Text = $"{drive.Value.DriveLetter} - {drive.Value.VolumeLabel} (序列号：{drive.Value.VolumeSerialNumber})";
            CurrentUsbDriveText.FontStyle = Avalonia.Media.FontStyle.Normal;
        }
        else
        {
            CurrentUsbDriveText.Text = "未设置";
            CurrentUsbDriveText.FontStyle = Avalonia.Media.FontStyle.Italic;
        }
    }

    public void LoadUsbDrives()
    {
        UsbDriveListContainer.Children.Clear();
        var drives = UsbDriveService.GetUsbDrives();

        if (drives.Count == 0)
        {
            var noDriveText = new TextBlock
            {
                Text = "未检测到可用的 U 盘",
                FontStyle = Avalonia.Media.FontStyle.Italic,
                Foreground = Avalonia.Media.Brushes.Gray
            };
            UsbDriveListContainer.Children.Add(noDriveText);
            return;
        }

        foreach (var drive in drives)
        {
            var checkBox = new CheckBox
            {
                Content = $"{drive.DriveLetter} - {drive.VolumeLabel} (序列号：{drive.VolumeSerialNumber})",
                Tag = drive,
                Margin = new Avalonia.Thickness(0, 2, 0, 2)
            };

            if (_currentDrive.HasValue && drive == _currentDrive.Value)
            {
                checkBox.IsChecked = true;
                _selectedDrive = drive;
            }

            checkBox.IsCheckedChanged += UsbDriveCheckBox_OnIsCheckedChanged;
            UsbDriveListContainer.Children.Add(checkBox);
        }
    }

    private void UsbDriveCheckBox_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox checkBox) return;

        if (checkBox.IsChecked ?? false)
        {
            _selectedDrive = checkBox.Tag as UsbDriveInfo?;
            foreach (var child in UsbDriveListContainer.Children)
            {
                if (child is CheckBox otherCheckBox && otherCheckBox != checkBox)
                {
                    otherCheckBox.IsChecked = false;
                }
            }
        }
        else
        {
            _selectedDrive = null;
        }

        ErrorTextBlock.IsVisible = false;
        ErrorTextBlock.Text = string.Empty;
    }

    private void RefreshButton_OnClick(object? sender, RoutedEventArgs e)
    {
        LoadUsbDrives();
    }

    public bool Validate()
    {
        if (_selectedDrive == null)
        {
            ShowError("请选择一个 U 盘");
            return false;
        }

        return true;
    }

    public UsbDriveInfo? GetSelectedDrive()
    {
        return _selectedDrive;
    }

    public void Clear()
    {
        _selectedDrive = null;
        ErrorTextBlock.IsVisible = false;
        ErrorTextBlock.Text = string.Empty;
        LoadUsbDrives();
    }

    private void ShowError(string message)
    {
        ErrorTextBlock.Text = message;
        ErrorTextBlock.IsVisible = true;
    }
}
