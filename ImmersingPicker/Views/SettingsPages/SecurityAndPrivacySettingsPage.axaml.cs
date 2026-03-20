using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using ImmersingPicker.Controls;
using ImmersingPicker.Core.Models;
using ImmersingPicker.Helpers;
using ImmersingPicker.Services.Services;
using ImmersingPicker.Services.Services.Security;
using ImmersingPicker.Services.Services.Storage;

namespace ImmersingPicker.Views.SettingsPages;

public partial class SecurityAndPrivacySettingsPage : UserControl
{
    private readonly PasswordService _passwordService = PasswordService.Instance;

    public SecurityAndPrivacySettingsPage()
    {
        InitializeComponent();
        LoadSettings();
        CheckEnabled();
    }

    private void LoadSettings()
    {
        OpenPassword.IsChecked = AppSettings.Instance.OpenPassword;
        EnableUsbSecurityCheck.IsChecked = AppSettings.Instance.EnableUsbSecurityCheck;
    }

    private void CheckEnabled()
    {
        SetPasswordItem.IsEnabled = OpenPassword.IsChecked ?? false;
        EnableUsbSecurityCheckItem.IsEnabled = OpenPassword.IsChecked ?? false;
        SetUsbDriveItem.IsEnabled = (OpenPassword.IsChecked ?? false) && (EnableUsbSecurityCheck.IsChecked ?? false);
    }

    private void OpenPassword_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        AppSettings.Instance.OpenPassword = OpenPassword.IsChecked ?? false;
        CheckEnabled();
    }

    private async void SetPassword_OnClick(object? sender, RoutedEventArgs e)
    {
        await ShowPasswordDialog();
    }

    private async Task ShowPasswordDialog()
    {
        var passwordDialog = new PasswordDialog();
        passwordDialog.Clear();
        passwordDialog.SetMode(_passwordService.HasPassword);

        var parentWindow = TopLevel.GetTopLevel(this) as Window;
        if (parentWindow == null) return;

        var dialog = new ContentDialog
        {
            Title = _passwordService.HasPassword ? "修改密码" : "设置密码",
            Content = passwordDialog,
            PrimaryButtonText = "确认",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary
        };

        var result = await dialog.ShowAsync(parentWindow);

        if (result == ContentDialogResult.Primary)
        {
            if (passwordDialog.Validate())
            {
                if (passwordDialog.SavePassword())
                {
                    SettingsStorageService.Instance.SaveSettings();
                    var successDialog = new ContentDialog
                    {
                        Title = "成功",
                        Content = _passwordService.HasPassword ? "密码修改成功" : "密码设置成功",
                        CloseButtonText = "确定"
                    };
                    await successDialog.ShowAsync(parentWindow);
                }
                else
                {
                    var errorDialog = new ContentDialog
                    {
                        Title = "失败",
                        Content = "密码设置失败，请重试",
                        CloseButtonText = "确定"
                    };
                    await errorDialog.ShowAsync(parentWindow);
                }
            }
            else
            {
                await ShowPasswordDialog();
            }
        }
    }

    private void EnableUsbSecurityCheck_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        AppSettings.Instance.EnableUsbSecurityCheck = EnableUsbSecurityCheck.IsChecked ?? false;
        CheckEnabled();
    }

    private async void SetUsbDrive_OnClick(object? sender, RoutedEventArgs e)
    {
        if (!AppSettings.Instance.OpenPassword)
        {
            var parentWindow = TopLevel.GetTopLevel(this) as Window;
            if (parentWindow == null) return;

            var warningDialog = new ContentDialog
            {
                Title = "提示",
                Content = "请先开启密码保护并设置密码，然后才能设置 U 盘校验。",
                CloseButtonText = "确定"
            };
            await warningDialog.ShowAsync(parentWindow);
            return;
        }

        if (!_passwordService.HasPassword)
        {
            var parentWindow = TopLevel.GetTopLevel(this) as Window;
            if (parentWindow == null) return;

            var warningDialog = new ContentDialog
            {
                Title = "提示",
                Content = "请先设置密码，然后才能设置 U 盘校验。",
                CloseButtonText = "确定"
            };
            await warningDialog.ShowAsync(parentWindow);
            return;
        }

        var verified = await VerifyHelper.VerifyPassword(TopLevel.GetTopLevel(this) as Window ?? throw new InvalidOperationException());
        if (verified)
        {
            await ShowUsbDriveDialog();
        }
    }

    private async Task ShowUsbDriveDialog()
    {
        var usbDriveDialog = new UsbDriveDialog();
        usbDriveDialog.Clear();
        usbDriveDialog.SetCurrentDrive(AppSettings.Instance.UsbSecurityDriveInfo);

        var parentWindow = TopLevel.GetTopLevel(this) as Window;
        if (parentWindow == null) return;

        var dialog = new ContentDialog
        {
            Title = "设置校验 U 盘",
            Content = usbDriveDialog,
            PrimaryButtonText = "确认",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary
        };

        var result = await dialog.ShowAsync(parentWindow);

        if (result == ContentDialogResult.Primary)
        {
            if (usbDriveDialog.Validate())
            {
                var selectedDrive = usbDriveDialog.GetSelectedDrive();
                if (selectedDrive.HasValue)
                {
                    AppSettings.Instance.UsbSecurityDriveInfo = selectedDrive.Value;
                    var successDialog = new ContentDialog
                    {
                        Title = "成功",
                        Content = "U 盘设置成功",
                        CloseButtonText = "确定"
                    };
                    await successDialog.ShowAsync(parentWindow);
                }
                else
                {
                    var errorDialog = new ContentDialog
                    {
                        Title = "失败",
                        Content = "U 盘设置失败，请重试",
                        CloseButtonText = "确定"
                    };
                    await errorDialog.ShowAsync(parentWindow);
                }
            }
            else
            {
                await ShowUsbDriveDialog();
            }
        }
    }
}