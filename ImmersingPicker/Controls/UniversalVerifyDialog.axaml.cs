using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ImmersingPicker.Core.Models;
using ImmersingPicker.Services.Services;
using ImmersingPicker.Services.Services.Security;

namespace ImmersingPicker.Controls;

public partial class UniversalVerifyDialog : UserControl
{
    private readonly PasswordService _passwordService = PasswordService.Instance;
    private bool _isPasswordConfigured;
    private bool _isUsbConfigured;
    private UsbDriveInfo? _configuredUsbDrive;

    public UniversalVerifyDialog()
    {
        InitializeComponent();
        RefreshUsbButton.Click += RefreshUsbButton_OnClick;
        InitializeVerifyMethods();
    }

    private void InitializeVerifyMethods()
    {
        _isPasswordConfigured = AppSettings.Instance.OpenPassword && _passwordService.HasPassword;
        _isUsbConfigured = AppSettings.Instance.EnableUsbSecurityCheck && AppSettings.Instance.UsbSecurityDriveInfo.HasValue;
        _configuredUsbDrive = AppSettings.Instance.UsbSecurityDriveInfo;

        VerifyMethodComboBox.Items.Clear();

        if (_isPasswordConfigured)
        {
            VerifyMethodComboBox.Items.Add("密码验证");
        }

        if (_isUsbConfigured)
        {
            VerifyMethodComboBox.Items.Add("U 盘验证");
        }

        if (VerifyMethodComboBox.Items.Count > 0)
        {
            VerifyMethodComboBox.SelectedIndex = 0;
        }
        else
        {
            ErrorTextBlock.Text = "未配置任何验证方式";
            ErrorTextBlock.IsVisible = true;
        }
    }

    private void VerifyMethodComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (VerifyMethodComboBox.SelectedItem is not string selectedMethod) return;

        switch (selectedMethod)
        {
            case "密码验证":
                ShowPasswordVerifyPanel();
                break;
            case "U 盘验证":
                ShowUsbVerifyPanel();
                break;
        }
    }

    private void ShowPasswordVerifyPanel()
    {
        PasswordVerifyPanel.IsVisible = true;
        UsbVerifyPanel.IsVisible = false;
        InfoTextBlock.Text = "请输入密码以继续操作。";
    }

    private void ShowUsbVerifyPanel()
    {
        PasswordVerifyPanel.IsVisible = false;
        UsbVerifyPanel.IsVisible = true;
        InfoTextBlock.Text = "请插入已配置的 U 盘，系统将自动进行验证。";
        CheckUsbDrive();
    }

    private void CheckUsbDrive()
    {
        if (_configuredUsbDrive == null)
        {
            UsbStatusText.Text = "未配置 U 盘";
            UsbStatusText.Foreground = Avalonia.Media.Brushes.Red;
            return;
        }

        var currentDrives = UsbDriveService.GetUsbDrives();
        var matchingDrive = UsbDriveService.FindMatchingUsbDrive(_configuredUsbDrive.Value);

        if (matchingDrive.HasValue)
        {
            UsbStatusText.Text = $"已检测到 U 盘：{matchingDrive.Value.DriveLetter} - {matchingDrive.Value.VolumeLabel}";
            UsbStatusText.Foreground = Avalonia.Media.Brushes.Green;
        }
        else
        {
            UsbStatusText.Text = "未检测到已配置的 U 盘，请插入 U 盘";
            UsbStatusText.Foreground = Avalonia.Media.Brushes.Orange;
        }
    }

    private void RefreshUsbButton_OnClick(object? sender, RoutedEventArgs e)
    {
        CheckUsbDrive();
        ErrorTextBlock.IsVisible = false;
        ErrorTextBlock.Text = string.Empty;
    }

    public bool Verify()
    {
        if (VerifyMethodComboBox.SelectedItem is not string selectedMethod)
        {
            ShowError("请选择验证方式");
            return false;
        }

        switch (selectedMethod)
        {
            case "密码验证":
                return VerifyPassword();
            case "U 盘验证":
                return VerifyUsbDrive();
            default:
                ShowError("未知的验证方式");
                return false;
        }
    }

    private bool VerifyPassword()
    {
        ErrorTextBlock.IsVisible = false;
        ErrorTextBlock.Text = string.Empty;

        string password = PasswordBox.Text ?? string.Empty;

        if (string.IsNullOrEmpty(password))
        {
            ShowError("请输入密码");
            return false;
        }

        if (!_passwordService.VerifyPassword(password))
        {
            ShowError("密码错误");
            return false;
        }

        return true;
    }

    private bool VerifyUsbDrive()
    {
        ErrorTextBlock.IsVisible = false;
        ErrorTextBlock.Text = string.Empty;

        if (_configuredUsbDrive == null)
        {
            ShowError("未配置 U 盘验证");
            return false;
        }

        var currentDrives = UsbDriveService.GetUsbDrives();
        var matchingDrive = UsbDriveService.FindMatchingUsbDrive(_configuredUsbDrive.Value);

        if (!matchingDrive.HasValue)
        {
            ShowError("未检测到已配置的 U 盘，请插入正确的 U 盘");
            return false;
        }

        return true;
    }

    public void Clear()
    {
        PasswordBox.Text = string.Empty;
        ErrorTextBlock.IsVisible = false;
        ErrorTextBlock.Text = string.Empty;
        
        if (VerifyMethodComboBox.Items.Count > 0)
        {
            VerifyMethodComboBox.SelectedIndex = 0;
        }
        
        if (_isUsbConfigured)
        {
            CheckUsbDrive();
        }
    }

    private void ShowError(string message)
    {
        ErrorTextBlock.Text = message;
        ErrorTextBlock.IsVisible = true;
    }
}
