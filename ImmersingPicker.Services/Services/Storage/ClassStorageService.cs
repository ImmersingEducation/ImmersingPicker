using ImmersingPicker.Core;
using ImmersingPicker.Core.Abstractions.Storage;
using ImmersingPicker.Core.Models;
using ImmersingPicker.Services.Helper;
using ImmersingPicker.Services.Services.Picker;
using System.IO;
using System.Text.Json;
using Serilog;

namespace ImmersingPicker.Services.Services.Storage;

public class ClassStorageService : IClazzStorageService
{
    private static readonly ClassStorageService _instance = new ClassStorageService();
    public static ClassStorageService Instance => _instance;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public void SaveClasses(List<Clazz> classes)
    {
        Log.Information("开始保存班级数据，共{Count}个班级", classes.Count);
        try
        {
            // 创建一个临时列表，用于存储不包含 Pickers 的班级对象
            Log.Verbose("创建临时班级列表");
            var classesToSave = new List<Clazz>();
            // 创建 classes 列表的副本，避免在遍历过程中集合被修改
            Log.Verbose("创建班级列表副本");
            var classesCopy = classes.ToList();
            foreach (var clazz in classesCopy)
            {
                Log.Verbose("处理班级：{ClassName}", clazz.Name);
                // 直接创建新班级，不添加到 Classes 集合
                var classToSave = new Clazz(clazz.Name, clazz.Students, clazz.Histories, false);
                classesToSave.Add(classToSave);
            }
            
            Log.Verbose("创建可存储的班级对象");
            var storableClasses = new StorableClasses
            {
                Classes = classesToSave,
                CurrentClassIndex = Clazz.CurrentClassIndex
            };
            
            Log.Verbose("序列化班级数据");
            string jsonContent = JsonSerializer.Serialize(storableClasses, _jsonOptions);
            Log.Debug("序列化后的JSON长度: {Length}", jsonContent.Length);
            
            string filePath = ApplicationDataDirPathGetter.GetClassesFilePath();
            Log.Information("班级数据保存路径: {FilePath}", filePath);
            // 确保目录存在
            Log.Verbose("确保目录存在");
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            Log.Verbose("写入文件");
            File.WriteAllText(filePath, jsonContent);
            Log.Information("班级数据保存成功");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "保存班级数据失败");
            throw;
        }
    }

    public void LoadClasses()
    {
        Log.Information("开始加载班级数据");
        try
        {
            string filePath = ApplicationDataDirPathGetter.GetClassesFilePath();
            Log.Information("班级数据文件路径: {FilePath}", filePath);
            if (File.Exists(filePath))
            {
                Log.Information("班级数据文件存在，开始读取");
                Log.Verbose("读取文件内容");
                string jsonContent = File.ReadAllText(filePath);
                Log.Debug("文件大小: {Size} bytes", jsonContent.Length);
                
                Log.Verbose("反序列化班级数据");
                var deserialized = JsonSerializer.Deserialize<StorableClasses>(jsonContent, _jsonOptions);
                
                // 清空现有班级列表
                Log.Verbose("清空现有班级列表");
                Clazz.Classes.Clear();
                
                // 重新创建班级对象，ClazzFactory.NewClazz 会自动添加 Picker
                Log.Verbose("重新创建班级对象");
                foreach (var clazz in deserialized.Classes)
                {
                    Log.Verbose("处理班级：{ClassName}", clazz.Name);
                    ClazzFactory.NewClazz(
                        clazz.Name, 
                        clazz.Students, 
                        clazz.Histories
                    );
                }
                
                Log.Verbose("设置当前班级索引: {Index}", deserialized.CurrentClassIndex);
                Clazz.CurrentClassIndex = deserialized.CurrentClassIndex;
                Log.Information("班级数据加载成功，共{Count}个班级", Clazz.Classes.Count);
            }
            else
            {
                Log.Information("班级数据文件不存在，使用默认数据");
                Log.Warning("首次运行或数据文件被删除");
            }
        }
        catch (JsonException ex)
        {
            Log.Error(ex, "班级数据文件格式错误");
            Log.Warning("使用默认数据");
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "加载班级数据失败");
            throw;
        }
    }
}