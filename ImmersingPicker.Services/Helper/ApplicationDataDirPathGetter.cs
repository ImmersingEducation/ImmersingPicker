using System;
using System.IO;
using Serilog;

namespace ImmersingPicker.Services.Helper;

public class ApplicationDataDirPathGetter
{
    private static readonly ILogger _logger = Log.ForContext<ApplicationDataDirPathGetter>();

    public static string Get()
    {
        _logger.Verbose("获取应用程序数据目录");
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ipicker");
        if (!Directory.Exists(path))
        {
            _logger.Information("应用程序数据目录不存在，创建目录：{Path}", path);
            Directory.CreateDirectory(path);
        }
        else
        {
            _logger.Debug("应用程序数据目录已存在：{Path}", path);
        }
        return path;
    }

    public static string GetClassesFilePath()
    {
        _logger.Verbose("获取班级数据文件路径");
        var path = Path.Combine(Get(), "Classes.json");
        _logger.Debug("班级数据文件路径：{Path}", path);
        return path;
    }

    public static string GetSettingsFilePath()
    {
        _logger.Verbose("获取设置数据文件路径");
        var path = Path.Combine(Get(), "Settings.json");
        _logger.Debug("设置数据文件路径：{Path}", path);
        return path;
    }

    public static string GetLogFilePath()
    {
        _logger.Verbose("获取日志文件路径");
        var path = Path.Combine(Get(), "Logs\\");
        _logger.Debug("日志文件路径：{Path}", path);
        return path;
    }
}