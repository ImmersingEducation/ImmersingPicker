using Avalonia.Controls;
using ImmersingPicker.Services.Services;
using ImmersingPicker.Services.Services.Security;

namespace ImmersingPicker.Controls;

public partial class PasswordDialog : UserControl
{
    private readonly PasswordService _passwordService = PasswordService.Instance;

    public PasswordDialog()
    {
        InitializeComponent();
    }

    public void SetMode(bool hasPassword)
    {
        OldPasswordPanel.IsVisible = hasPassword;
    }

    public bool Validate()
    {
        ErrorTextBlock.IsVisible = false;
        ErrorTextBlock.Text = string.Empty;

        string oldPassword = OldPasswordBox.Text ?? string.Empty;
        string newPassword = NewPasswordBox.Text ?? string.Empty;
        string confirmPassword = ConfirmPasswordBox.Text ?? string.Empty;

        if (OldPasswordPanel.IsVisible)
        {
            if (string.IsNullOrEmpty(oldPassword))
            {
                ShowError("请输入旧密码");
                return false;
            }

            if (!_passwordService.VerifyPassword(oldPassword))
            {
                ShowError("旧密码不正确");
                return false;
            }
        }

        if (string.IsNullOrEmpty(newPassword))
        {
            ShowError("请输入新密码");
            return false;
        }

        if (newPassword.Length < 4)
        {
            ShowError("新密码长度至少为4位");
            return false;
        }

        if (newPassword != confirmPassword)
        {
            ShowError("两次输入的新密码不一致");
            return false;
        }

        return true;
    }

    public bool SavePassword()
    {
        string newPassword = NewPasswordBox.Text ?? string.Empty;
        string oldPassword = OldPasswordBox.Text ?? string.Empty;

        if (OldPasswordPanel.IsVisible)
        {
            return _passwordService.ChangePassword(oldPassword, newPassword);
        }
        else
        {
            return _passwordService.SetPassword(newPassword);
        }
    }

    private void ShowError(string message)
    {
        ErrorTextBlock.Text = message;
        ErrorTextBlock.IsVisible = true;
    }

    public void Clear()
    {
        OldPasswordBox.Text = string.Empty;
        NewPasswordBox.Text = string.Empty;
        ConfirmPasswordBox.Text = string.Empty;
        ErrorTextBlock.IsVisible = false;
        ErrorTextBlock.Text = string.Empty;
    }
}
