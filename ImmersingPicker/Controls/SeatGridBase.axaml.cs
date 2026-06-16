using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using ImmersingPicker.Core;
using ImmersingPicker.Core.Models;
using Serilog;

namespace ImmersingPicker.Controls;

public abstract partial class SeatGridBase : UserControl
{
    private static readonly ILogger _logger = Log.ForContext<SeatGridBase>();
    
    private readonly Dictionary<int, int> _rowIndexMap = new();
    private readonly Dictionary<int, int> _columnIndexMap = new();

    private double _magnifyLens = 1;
    public double MagnifyingLens
    {
        get => _magnifyLens;
        set
        {
            if (value < 1) return;
            _magnifyLens = value;
            RefreshStudents();
        }
    }

    private Clazz? _clazz;

    protected readonly Dictionary<int, Button> _seats = new();
    
    protected Grid? grid;

    public SeatGridBase()
    {
        _logger.Information("初始化 SeatGridBase 控件");
    }

    protected void InitializeBase()
    {
        grid = this.FindControl<Grid>("grid")!;
        
        _logger.Verbose("刷新班级信息");
        RefreshClazz();
        _logger.Verbose("添加班级变更事件处理");
        Clazz.CurrentClassChanged += RefreshClazz;
        _logger.Verbose("添加座位排列设置变更事件处理");
        AppSettings.Instance.SeatGridRowArrangementChanged += OnSeatGridRowArrangementChanged;
        AppSettings.Instance.SeatGridColumnArrangementChanged += OnSeatGridColumnArrangementChanged;
        _logger.Information("SeatGridBase 初始化完成");
    }

    private void OnSeatGridRowArrangementChanged(AppSettings.SeatGridRowArrangementMode mode)
    {
        _logger.Debug("座位行排列方式变更：{Mode}", mode);
        RefreshStudents();
    }

    private void OnSeatGridColumnArrangementChanged(AppSettings.SeatGridColumnArrangementMode mode)
    {
        _logger.Debug("座位列排列方式变更：{Mode}", mode);
        RefreshStudents();
    }

    public void RefreshClazz()
    {
        _logger.Information("刷新班级信息");
        if (_clazz != null) {
            _logger.Verbose("移除旧班级的学生列表变更事件");
            _clazz.StudentListChanged -= RefreshStudents;
        }
        _clazz = Clazz.GetCurrentClazz();
        if (_clazz != null)
        {
            _logger.Information("当前班级：{ClassName}", _clazz.Name);
            _clazz.StudentListChanged += RefreshStudents;
        }
        else
        {
            _logger.Warning("当前班级为 null");
        }
        RefreshStudents();
    }

    public void RefreshStudents()
    {
        _logger.Verbose("刷新学生座位显示");
        _seats.Clear();
        _rowIndexMap.Clear();
        _columnIndexMap.Clear();

        if (grid == null)
        {
            _logger.Warning("grid 为 null，无法刷新");
            return;
        }

        grid.Children.Clear();
        grid.RowDefinitions.Clear();
        grid.ColumnDefinitions.Clear();

        if (_clazz == null)
        {
            _logger.Warning("班级为 null，显示提示");
            SetTip("当前暂无班级");
        }
        else if (_clazz.Students.Count <= 0)
        {
            _logger.Warning("班级 {ClassName} 无学生", _clazz.Name);
            SetTip($"班级 {_clazz.Name} 中暂无学生");
        }
        else
        {
            _logger.Information("开始渲染座位表，学生数：{Count}", _clazz.Students.Count);
            
            // 首先收集所有唯一的 SeatRow 和 SeatColumn 值
            var uniqueRows = _clazz.Students
                .Where(s => s.SeatRow > 0)
                .Select(s => s.SeatRow)
                .Distinct()
                .OrderBy(row => row)
                .ToList();

            var uniqueColumns = _clazz.Students
                .Where(s => s.SeatColumn > 0)
                .Select(s => s.SeatColumn)
                .Distinct()
                .OrderBy(col => col)
                .ToList();

            _logger.Debug("唯一行数：{Rows}, 唯一列数：{Columns}", uniqueRows.Count, uniqueColumns.Count);

            // 为每个唯一的 SeatRow 和 SeatColumn 值分配一个连续的索引
            var rowArrangement = AppSettings.Instance.SeatGridRowArrangement;
            var columnArrangement = AppSettings.Instance.SeatGridColumnArrangement;

            for (int i = 0; i < uniqueRows.Count; i++)
            {
                int mappedIndex = rowArrangement == AppSettings.SeatGridRowArrangementMode.T2B ? i : uniqueRows.Count - 1 - i;
                _rowIndexMap[uniqueRows[i]] = mappedIndex;
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            }

            for (int i = 0; i < uniqueColumns.Count; i++)
            {
                int mappedIndex = columnArrangement == AppSettings.SeatGridColumnArrangementMode.L2R ? i : uniqueColumns.Count - 1 - i;
                _columnIndexMap[uniqueColumns[i]] = mappedIndex;
                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            }

            _logger.Verbose("创建座位按钮");
            // 然后创建并添加所有座位按钮
            int createdCount = 0;
            foreach (var student in _clazz.Students)
            {
                if (student.SeatRow < 0 || student.SeatColumn < 1)
                {
                    _logger.Debug("跳过无效座位的学生：{Name}, 座位：({Row},{Column})", student.Name, student.SeatRow, student.SeatColumn);
                    continue;
                }

                var button = new Button
                {
                    Content = $"{student.Id} {student.Name}",
                    Width = 80*MagnifyingLens,
                    Height = 48*MagnifyingLens,
                    Margin = new Thickness(5),
                    BorderThickness = new Thickness(2),
                    BorderBrush = Avalonia.Media.Brushes.Gray,
                };
                button.Click += (sender, e) => OnStudentSeatClicked(student);

                _seats[student.Id] = button;
                grid.Children.Add(button);
                
                // 使用映射后的索引来设置按钮的位置
                Grid.SetRow(button, _rowIndexMap[student.SeatRow]);
                Grid.SetColumn(button, _columnIndexMap[student.SeatColumn]);
                createdCount++;
            }
            
            _logger.Information("座位表渲染完成，创建按钮数：{Count}", createdCount);
        }

        void SetTip(string tip)
        {
            if (grid == null) return;
            
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

            TextBlock tipTextBlock = new TextBlock
            {
                Text = tip,
                FontSize = 48,
                FontWeight = Avalonia.Media.FontWeight.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            grid.Children.Add(tipTextBlock);
            Grid.SetRow(tipTextBlock, 0);
            Grid.SetColumn(tipTextBlock, 0);
        }
    }

    public void Select(Student student)
    {
        _logger.Verbose("选中学生座位：{Name}", student.Name);
        if (_seats.TryGetValue(student.Id, out var seat))
        {
            seat.BorderBrush = Avalonia.Media.Brushes.LimeGreen;
        }
        else
        {
            _logger.Warning("未找到学生座位：{Id}", student.Id);
        }
    }

    public void Deselect(Student student)
    {
        _logger.Verbose("取消选中学生座位：{Name}", student.Name);
        if (_seats.TryGetValue(student.Id, out var seat))
        {
            seat.BorderBrush = Avalonia.Media.Brushes.Gray;
        }
    }

    public void DeselectAll()
    {
        _logger.Verbose("取消所有选中");
        if (_clazz == null) return;
        foreach (Student student in _clazz.Students)
        {
            Deselect(student);
        }
    }

    protected abstract void OnStudentSeatClicked(Student student);
}
