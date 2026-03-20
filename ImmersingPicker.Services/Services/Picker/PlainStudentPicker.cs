using ImmersingPicker.Core;
using ImmersingPicker.Core.Abstractions.Picker;
using ImmersingPicker.Core.Models;
using Serilog;

namespace ImmersingPicker.Services.Services.Picker;

public class PlainStudentPicker(Clazz clazz) : PickerBase(clazz)
{
    public override string Name { get; set; } = "PlainStudentPicker";
    public override bool NeedStore { get; set; } = false;

    private List<Student> Shuffle(List<Student> students)
    {
        if (students.Count == 1) return students;
        List<Student> tmp = new List<Student>(students);
        int n = tmp.Count;
        while (n > 1)
        {
            n--;
            int k = _random.Next(n + 1);
            (tmp[n], tmp[k]) = (tmp[k], tmp[n]);
        }

        return tmp;
    }

    protected override History PickLogic(int amount)
    {
        Log.Information("开始执行随机抽取，班级: {ClassName}, 抽取数量: {Amount}", _clazz.Name, amount);
        
        try
        {
            List<Student> shuffled = Shuffle(_clazz.Students);
            Log.Information("学生列表打乱完成，班级共有{Count}个学生", _clazz.Students.Count);
            
            List<Student> result = new List<Student>();
            for (int i = 0; i < amount && i < shuffled.Count; i++)
            {
                result.Add(shuffled[i]);
            }
            
            // 记录抽取结果
            string pickedStudentNames = string.Join(", ", result.Select(s => s.Name));
            Log.Information("抽取完成，结果: {PickedStudents}", pickedStudentNames);
            
            // 创建历史记录
            var history = new History(DateTime.Now, Name, result);
            Log.Information("创建历史记录成功");
            return history;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "随机抽取过程中发生错误");
            throw;
        }
    }
}