using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using ImmersingPicker.Controls;
using ImmersingPicker.Core;
using ImmersingPicker.Core.Models;
using Serilog;

namespace ImmersingPicker.Views.MainPages;

public partial class HomePage : UserControl
{
    private static readonly ILogger _logger = Log.ForContext<HomePage>();
    private int _amountForPicking;

    private Clazz? _clazz;

    private int AmountForPicking
    {
        get => _amountForPicking;
        set
        {
            try
            {
                if (_clazz == null) throw new NullReferenceException();
                if (value > 0 && (_clazz.Students.Count <= 0 || value <= _clazz.Students.Count))
                {
                    _amountForPicking = value;
                    PickButton.Content = $"共{_amountForPicking}人  开始抽选！";
                }
            }
            catch (NullReferenceException)
            {
                _amountForPicking = 1;
                PickButton.Content = $"共{_amountForPicking}人  开始抽选！";
            }
        }
    }

    public HomePage()
    {
        _logger.Information("初始化HomePage");
        InitializeComponent();
        _logger.Verbose("添加班级变更事件处理");
        Clazz.CurrentClassChanged += Reset;
        _logger.Verbose("添加班级下拉框选择变更事件处理");
        ClazzComboBox.SelectionChanged += ClazzComboBox_OnSelectionChanged;
        _logger.Verbose("执行重置操作");
        Reset();
        _logger.Information("HomePage初始化完成");
    }

    private void ClazzComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _logger.Information("班级选择变更事件触发");
        if (sender is ComboBox comboBox && comboBox.SelectedItem is Clazz selectedClazz)
        {
            _logger.Information("选择班级: {ClassName}", selectedClazz.Name);
            int index = Clazz.Classes.IndexOf(selectedClazz);
            if (index != -1)
            {
                _logger.Verbose("设置当前班级索引: {Index}", index);
                Clazz.CurrentClassIndex = index;
                _logger.Information("班级切换成功");
            }
            else
            {
                _logger.Warning("无法找到选中班级的索引");
            }
        }
        else
        {
            _logger.Warning("无效的选择项");
        }
    }

    private void MinusButton_OnClick(object? sender, RoutedEventArgs e)
    {
        AmountForPicking -= 1;
    }

    private async void PickButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _logger.Information("开始执行抽选操作");
        Clazz? currentClazz = Clazz.GetCurrentClazz();
        if (currentClazz == null)
        {
            _logger.Warning("当前班级为null，无法执行抽选");
            return;
        }

        _logger.Information("使用公平抽选器抽选{Amount}名学生", AmountForPicking);
        List<Student> picked = currentClazz.Pickers["FairStudentPicker"].Pick(AmountForPicking).Students;
        _logger.Information("抽选完成，结果: {PickedStudents}", string.Join(", ", picked.Select(s => s.Name)));

        _logger.Verbose("开始动画效果");
        for (int i = 0; i < AppSettings.Instance.HomeAnimationPlayAmount; i++)
        {
            Seats.DeselectAll();
            foreach (Student student in currentClazz.Pickers["PlainStudentPicker"].Pick(AmountForPicking).Students)
            {
                Seats.Select(student);
            }

            await Task.Delay(AppSettings.Instance.HomeAnimationPlayDelay);
        }

        _logger.Verbose("显示最终结果");
        Seats.DeselectAll();
        string dialogContent = "";
        foreach (Student student in picked)
        {
            Seats.Select(student);
            dialogContent += $"{student.Id} {student.Name}\n";
        }

        _logger.Information("显示抽选结果对话框");
        var dialog = new ContentDialog
        {
            Title = "抽选结果  恭喜以下幸运儿：",
            Content = dialogContent,
            CloseButtonText = "确定"
        };
        await dialog.ShowAsync();
        _logger.Information("抽选操作完成");
    }

    private void PlusButton_OnClick(object? sender, RoutedEventArgs e)
    {
        AmountForPicking += 1;
    }

    private void ClearButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Seats.DeselectAll();
    }

    public void Reset()
    {
        _logger.Information("重置HomePage状态");
        _logger.Verbose("获取当前班级");
        _clazz = Clazz.GetCurrentClazz();
        _logger.Verbose("重置抽选数量为1");
        _amountForPicking = 1;
        PickButton.Content = $"共{_amountForPicking}人  开始抽选！";
        
        // 更新班级下拉框
        _logger.Verbose("更新班级下拉框");
        ClazzComboBox.Items.Clear();
        foreach (var clazz in Clazz.Classes)
        {
            _logger.Debug("添加班级到下拉框: {ClassName}", clazz.Name);
            ClazzComboBox.Items.Add(clazz);
        }
        
        // 选择当前班级
        if (_clazz != null)
        {
            _logger.Information("选择当前班级: {ClassName}", _clazz.Name);
            ClazzComboBox.SelectedItem = _clazz;
        }
        else
        {
            _logger.Warning("当前班级为null");
        }
        _logger.Information("重置完成");
    }
}