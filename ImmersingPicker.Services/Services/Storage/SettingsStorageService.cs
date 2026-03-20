using System.Text.Json;
using System.Text.Json.Serialization;
using ImmersingPicker.Core.JsonConverters;
using ImmersingPicker.Core.Models;
using ImmersingPicker.Services.Helper;
using Serilog;

namespace ImmersingPicker.Services.Services.Storage;

public class SettingsStorageService
{
    private static readonly SettingsStorageService _instance = new SettingsStorageService();
    public static SettingsStorageService Instance => _instance;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(), new UsbDriveInfoNullableConverter() }
    };

    public void SaveSettings()
    {
        Log.Information("开始保存应用设置");
        try
        {
            string jsonContent = JsonSerializer.Serialize(AppSettings.Instance, _jsonOptions);
            Log.Debug("设置内容: {JsonContent}", jsonContent);
            string filePath = ApplicationDataDirPathGetter.GetSettingsFilePath();
            Log.Information("设置保存路径: {FilePath}", filePath);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            File.WriteAllText(filePath, jsonContent);
            Log.Information("设置保存成功！");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "保存设置失败");
        }
    }

    public void LoadSettings()
    {
        Log.Information("开始加载应用设置");
        try
        {
            string filePath = ApplicationDataDirPathGetter.GetSettingsFilePath();
            Log.Information("设置文件路径: {FilePath}", filePath);
            if (File.Exists(filePath))
            {
                Log.Information("设置文件存在，开始读取");
                string jsonContent = File.ReadAllText(filePath);
                Log.Debug("设置文件内容: {JsonContent}", jsonContent);
                AppSettings.Instance = JsonSerializer.Deserialize<AppSettings>(jsonContent, _jsonOptions) ?? new AppSettings();
                Log.Information("设置加载成功");
            }
            else
            {
                Log.Information("设置文件不存在，使用默认设置");
                AppSettings.Instance = new AppSettings();
                // 保存默认设置到文件
                SaveSettings();
            }
        }
        catch (JsonException ex)
        {
            Log.Error(ex, "设置文件反序列化失败");
            // 处理反序列化错误，使用默认设置
            AppSettings.Instance = new AppSettings();
            // 保存默认设置到文件
            SaveSettings();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "加载设置失败");
            // 使用默认设置
            AppSettings.Instance = new AppSettings();
            // 保存默认设置到文件
            SaveSettings();
        }
    }

    public SettingsStorageService()
    {
        AppSettings.Instance.AnyChanged += SaveSettings;
    }
}