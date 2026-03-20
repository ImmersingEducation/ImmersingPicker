using ImmersingPicker.Core.Abstractions.Picker;
using ImmersingPicker.Core.Models;
using Serilog;

namespace ImmersingPicker.Services.Services.Picker;

public static class ClazzFactory
{
    private static readonly ILogger _logger = Log.ForContext(typeof(ClazzFactory));

    public static Clazz NewClazz(string name, List<Student>? students = null, List<History>? histories = null)
    {
        _logger.Information("创建新班级：{Name}", name);
        
        if (string.IsNullOrWhiteSpace(name))
        {
            _logger.Error("班级名称为空，创建失败");
            throw new ArgumentException("班级名称不能为空", nameof(name));
        }

        var studentCount = students?.Count ?? 0;
        var historyCount = histories?.Count ?? 0;
        _logger.Verbose("创建班级参数：学生数：{StudentCount}, 历史数：{HistoryCount}", studentCount, historyCount);

        var newClazz = new Clazz(
            name,
            students ?? new List<Student>(),
            histories ?? new List<History>()
        );

        _logger.Debug("创建抽选器实例");
        _ = new FairStudentPicker(newClazz);
        _logger.Verbose("FairStudentPicker 创建成功");
        
        _ = new PlainStudentPicker(newClazz);
        _logger.Verbose("PlainStudentPicker 创建成功");

        if (Clazz.Classes.Count == 1)
        {
            _logger.Information("首个班级创建，设置为当前班级，索引：0");
            Clazz.CurrentClassIndex = 0;
        }

        _logger.Information("班级创建完成：{Name}, 当前班级总数：{Count}", name, Clazz.Classes.Count);
        return newClazz;
    }
}
