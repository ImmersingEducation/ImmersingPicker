using System.Security.Cryptography;
using System.Text;
using ImmersingPicker.Core.Models;
using Serilog;

namespace ImmersingPicker.Services.Services.Security;

public class PasswordService
{
    private static readonly PasswordService _instance = new PasswordService();
    public static PasswordService Instance => _instance;

    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100000;
    private static readonly HashAlgorithmName _hashAlgorithm = HashAlgorithmName.SHA256;

    private static readonly ILogger _logger = Log.ForContext<PasswordService>();

    public bool HasPassword => !string.IsNullOrEmpty(AppSettings.Instance.PasswordHash);

    public bool SetPassword(string newPassword)
    {
        _logger.Information("开始设置密码");
        
        if (string.IsNullOrEmpty(newPassword))
        {
            _logger.Warning("密码不能为空");
            return false;
        }

        if (newPassword.Length < 4)
        {
            _logger.Warning("密码长度不足");
            return false;
        }

        try
        {
            string hash = HashPassword(newPassword);
            AppSettings.Instance.PasswordHash = hash;
            _logger.Information("密码设置成功");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "密码设置失败");
            return false;
        }
    }

    public bool VerifyPassword(string password)
    {
        _logger.Information("开始验证密码");
        
        if (string.IsNullOrEmpty(password))
        {
            _logger.Warning("输入密码为空");
            return false;
        }

        if (string.IsNullOrEmpty(AppSettings.Instance.PasswordHash))
        {
            _logger.Warning("未设置密码");
            return false;
        }

        try
        {
            bool result = VerifyPassword(password, AppSettings.Instance.PasswordHash);
            _logger.Information("密码验证结果: {Result}", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "密码验证失败");
            return false;
        }
    }

    public bool ChangePassword(string oldPassword, string newPassword)
    {
        _logger.Information("开始更改密码");
        
        if (!VerifyPassword(oldPassword))
        {
            _logger.Warning("旧密码验证失败");
            return false;
        }

        return SetPassword(newPassword);
    }

    public bool ValidatePasswordStrength(string password)
    {
        if (string.IsNullOrEmpty(password))
            return false;
        
        return password.Length >= 4;
    }

    private string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            Iterations,
            _hashAlgorithm,
            HashSize);

        byte[] hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

        return Convert.ToBase64String(hashBytes);
    }

    private bool VerifyPassword(string password, string storedHash)
    {
        byte[] hashBytes = Convert.FromBase64String(storedHash);
        
        byte[] salt = new byte[SaltSize];
        Array.Copy(hashBytes, 0, salt, 0, SaltSize);
        
        byte[] storedHashValue = new byte[HashSize];
        Array.Copy(hashBytes, SaltSize, storedHashValue, 0, HashSize);
        
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            Iterations,
            _hashAlgorithm,
            HashSize);

        return CryptographicOperations.FixedTimeEquals(hash, storedHashValue);
    }
}
