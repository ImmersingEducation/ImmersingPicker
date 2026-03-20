using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using ImmersingPicker.Controls;
using ImmersingPicker.Core.Models;
using ImmersingPicker.Services.Services;
using ImmersingPicker.Services.Services.Security;

namespace ImmersingPicker.Helpers;

public static class VerifyHelper
{
    public static async Task<bool> VerifyAndExecute(Window parentWindow, Func<Task> action)
    {
        if (!AppSettings.Instance.OpenPassword)
        {
            await action();
            return true;
        }

        if (!PasswordService.Instance.HasPassword)
        {
            await action();
            return true;
        }

        var verifyDialog = new UniversalVerifyDialog();
        
        var dialog = new ContentDialog
        {
            Title = "身份验证",
            Content = verifyDialog,
            PrimaryButtonText = "确认",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary
        };

        var result = await dialog.ShowAsync(parentWindow);

        if (result == ContentDialogResult.Primary)
        {
            if (verifyDialog.Verify())
            {
                await action();
                return true;
            }
            else
            {
                var errorDialog = new ContentDialog
                {
                    Title = "验证失败",
                    Content = "验证失败，操作已取消",
                    CloseButtonText = "确定"
                };
                await errorDialog.ShowAsync(parentWindow);
                return false;
            }
        }

        return false;
    }

    public static async Task<bool> VerifyAndExecute<T>(Window parentWindow, Func<T, Task> action, T param)
    {
        if (!AppSettings.Instance.OpenPassword)
        {
            await action(param);
            return true;
        }

        if (!PasswordService.Instance.HasPassword)
        {
            await action(param);
            return true;
        }

        var verifyDialog = new UniversalVerifyDialog();
        
        var dialog = new ContentDialog
        {
            Title = "身份验证",
            Content = verifyDialog,
            PrimaryButtonText = "确认",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary
        };

        var result = await dialog.ShowAsync(parentWindow);

        if (result == ContentDialogResult.Primary)
        {
            if (verifyDialog.Verify())
            {
                await action(param);
                return true;
            }
            else
            {
                var errorDialog = new ContentDialog
                {
                    Title = "验证失败",
                    Content = "验证失败，操作已取消",
                    CloseButtonText = "确定"
                };
                await errorDialog.ShowAsync(parentWindow);
                return false;
            }
        }

        return false;
    }

    public static async Task<bool> VerifyPassword(Window parentWindow)
    {
        if (!AppSettings.Instance.OpenPassword)
        {
            return true;
        }

        if (!PasswordService.Instance.HasPassword)
        {
            return true;
        }

        var verifyDialog = new UniversalVerifyDialog();
        
        var dialog = new ContentDialog
        {
            Title = "身份验证",
            Content = verifyDialog,
            PrimaryButtonText = "确认",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary
        };

        var result = await dialog.ShowAsync(parentWindow);

        if (result == ContentDialogResult.Primary)
        {
            if (verifyDialog.Verify())
            {
                return true;
            }
            else
            {
                var errorDialog = new ContentDialog
                {
                    Title = "验证失败",
                    Content = "验证失败",
                    CloseButtonText = "确定"
                };
                await errorDialog.ShowAsync(parentWindow);
                return false;
            }
        }

        return false;
    }
}
