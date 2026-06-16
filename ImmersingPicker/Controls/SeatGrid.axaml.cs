using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using ImmersingPicker.Core;
using ImmersingPicker.Core.Models;
using Serilog;

namespace ImmersingPicker.Controls;

public partial class SeatGrid : SeatGridBase
{
    private static readonly ILogger _logger = Log.ForContext<SeatGrid>();

    public SeatGrid()
    {
        _logger.Information("初始化 SeatGrid 控件");
        InitializeComponent();
        InitializeBase();
        _logger.Information("SeatGrid 初始化完成");
    }

    protected override void OnStudentSeatClicked(Student student)
    {
        _logger.Information("座位按钮被点击：{Name}", student.Name);
        _ = ShowStudentInfoDialog(student);
    }

    private async Task ShowStudentInfoDialog(Student student)
    {
        var dialog = new ContentDialog
        {
            Title = $"当前学生：{student.Name}",
            Content = $"学号：{student.Id}",
            CloseButtonText = "确定"
        };
        await dialog.ShowAsync();
    }

    public async Task<List<Student>> PickAsync(int amount = 1)
    {
        _logger.Information("开始座位表抽选，人数：{Amount}", amount);
        
        if (Clazz.GetCurrentClazz() == null)
        {
            _logger.Error("班级为 null，无法抽选");
            return new List<Student>();
        }

        var clazz = Clazz.GetCurrentClazz();

        // 执行抽选动画
        _logger.Information("执行抽选动画");
        for (int i = 0; i < 10; i++)
        {
            DeselectAll();
            foreach (Student student in clazz.Pickers["PlainStudentPicker"].Pick(clazz, amount).Students)
            {
                Select(student);
            }

            await Task.Delay(100);
        }

        _logger.Information("执行最终抽选");
        DeselectAll();
        var picked = clazz.Pickers["FairStudentPicker"].Pick(clazz, amount).Students;
        _logger.Information("抽选完成，结果：{Students}", string.Join(", ", picked.Select(s => s.Name)));
        
        foreach (Student student in picked)
        {
            Select(student);
        }

        return picked;
    }
}
