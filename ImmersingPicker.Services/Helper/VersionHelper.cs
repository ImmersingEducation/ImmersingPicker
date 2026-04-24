using System;
using System.Reflection;
using Serilog;

namespace ImmersingPicker.Services.Helper;

/// <summary>
/// 版本号比较工具
/// </summary>
public class VersionHelper
{
    private static readonly ILogger _logger = Log.ForContext<VersionHelper>();

    /// <summary>
    /// 获取当前应用程序版本
    /// </summary>
    public static Version? GetCurrentVersion()
    {
        try
        {
            var assembly = Assembly.GetEntryAssembly();
            if (assembly == null)
            {
                _logger.Warning("无法获取入口程序集");
                return null;
            }

            var version = assembly.GetName().Version;
            _logger.Debug("当前应用版本: {Version}", version?.ToString());
            return version;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "获取当前版本时发生错误");
            return null;
        }
    }

    /// <summary>
    /// 解析版本字符串为 Version 对象
    /// </summary>
    /// <param name="versionString">版本字符串 (如 "1.2.3.4")</param>
    /// <returns>Version 对象,解析失败返回 null</returns>
    public static Version? ParseVersion(string versionString)
    {
        try
        {
            // 移除可能的 'v' 前缀
            if (versionString.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                versionString = versionString[1..];
            }

            if (Version.TryParse(versionString, out var version))
            {
                return version;
            }

            _logger.Warning("无法解析版本字符串: {VersionString}", versionString);
            return null;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "解析版本时发生错误: {VersionString}", versionString);
            return null;
        }
    }

    /// <summary>
    /// 比较两个版本号
    /// </summary>
    /// <param name="currentVersion">当前版本</param>
    /// <param name="newVersionString">新版本字符串</param>
    /// <returns>
    /// 1: 新版本更新
    /// 0: 版本相同
    /// -1: 新版本更旧
    /// null: 比较失败
    /// </returns>
    public static int? CompareVersions(Version currentVersion, string newVersionString)
    {
        try
        {
            var newVersion = ParseVersion(newVersionString);
            if (newVersion == null)
            {
                _logger.Warning("新版本解析失败: {VersionString}", newVersionString);
                return null;
            }

            var result = currentVersion.CompareTo(newVersion);
            _logger.Debug("版本比较: {Current} vs {New} = {Result}", 
                currentVersion, newVersion, result);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "比较版本时发生错误");
            return null;
        }
    }

    /// <summary>
    /// 检查是否有可用更新
    /// </summary>
    /// <param name="newVersionString">新版本字符串</param>
    /// <returns>是否有可用更新</returns>
    public static bool IsUpdateAvailable(string newVersionString)
    {
        var currentVersion = GetCurrentVersion();
        if (currentVersion == null)
        {
            _logger.Warning("无法获取当前版本");
            return false;
        }

        var comparison = CompareVersions(currentVersion, newVersionString);
        return comparison.HasValue && comparison.Value < 0;
    }

    /// <summary>
    /// 格式化版本号为显示字符串
    /// </summary>
    public static string FormatVersion(Version version)
    {
        return $"v{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }
}
