using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ImmersingPicker.Core;
using ImmersingPicker.Core.Models;

namespace ImmersingPicker.Controls;

public partial class FilterDialog : UserControl
{
    private List<Student> _allStudents = new();
    private List<CheckBox> _studentCheckBoxes = new();
    private Dictionary<Student, bool> _selectedStudents = new();

    public FilterDialog()
    {
        InitializeComponent();
        SelectAllButton.Click += OnSelectAllClick;
        DeselectAllButton.Click += OnDeselectAllClick;
        StudentSearchBox.TextChanged += OnStudentSearchTextChanged;
    }

    public FilterDialog(List<Student> students) : this()
    {
        _allStudents = students ?? new List<Student>();
        LoadStudentList();
    }

    public void SetStudents(List<Student> students)
    {
        _allStudents = students ?? new List<Student>();
        LoadStudentList();
    }

    private void LoadStudentList()
    {
        StudentListContainer.Children.Clear();
        _studentCheckBoxes.Clear();
        _selectedStudents.Clear();

        foreach (var student in _allStudents)
        {
            var checkBox = new CheckBox
            {
                Content = $"{student.Id} - {student.Name}",
                Tag = student,
                IsChecked = false,
                Margin = new Avalonia.Thickness(0, 2)
            };
            checkBox.IsCheckedChanged += OnStudentCheckBoxChanged;
            _studentCheckBoxes.Add(checkBox);
            _selectedStudents[student] = false;
            StudentListContainer.Children.Add(checkBox);
        }
    }

    private void OnStudentCheckBoxChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is CheckBox checkBox && checkBox.Tag is Student student)
        {
            _selectedStudents[student] = checkBox.IsChecked ?? false;
        }
    }

    private void OnSelectAllClick(object? sender, RoutedEventArgs e)
    {
        foreach (var checkBox in _studentCheckBoxes)
        {
            checkBox.IsChecked = true;
        }
    }

    private void OnDeselectAllClick(object? sender, RoutedEventArgs e)
    {
        foreach (var checkBox in _studentCheckBoxes)
        {
            checkBox.IsChecked = false;
        }
    }

    private void OnStudentSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        var searchText = StudentSearchBox.Text?.Trim().ToLower() ?? string.Empty;

        foreach (var checkBox in _studentCheckBoxes)
        {
            if (checkBox.Tag is Student student)
            {
                var isVisible = string.IsNullOrEmpty(searchText) ||
                               student.Name.ToLower().Contains(searchText) ||
                               student.Id.ToString().Contains(searchText);
                checkBox.IsVisible = isVisible;
            }
        }
    }

    public FilterCriteria GetFilterCriteria()
    {
        DateTime? startDateTime = null;
        DateTime? endDateTime = null;
        
        if (StartDatePicker.SelectedDate.HasValue)
        {
            var date = StartDatePicker.SelectedDate.Value.Date;
            var time = StartTimePicker.SelectedTime;
            if (time.HasValue)
            {
                startDateTime = date.Add(time.Value);
            }
            else
            {
                startDateTime = date;
            }
        }
        
        if (EndDatePicker.SelectedDate.HasValue)
        {
            var date = EndDatePicker.SelectedDate.Value.Date;
            var time = EndTimePicker.SelectedTime;
            if (time.HasValue)
            {
                endDateTime = date.Add(time.Value);
            }
            else
            {
                endDateTime = date.AddDays(1).AddTicks(-1);
            }
        }
        
        return new FilterCriteria
        {
            StartDate = startDateTime,
            EndDate = endDateTime,
            MinCount = (int)(MinCountPicker.Value ?? 0),
            MaxCount = (int)(MaxCountPicker.Value ?? 0),
            SelectedStudents = _selectedStudents
                .Where(kv => kv.Value)
                .Select(kv => kv.Key)
                .ToList()
        };
    }

    public void ResetFilter()
    {
        StartDatePicker.SelectedDate = null;
        EndDatePicker.SelectedDate = null;
        StartTimePicker.SelectedTime = null;
        EndTimePicker.SelectedTime = null;
        MinCountPicker.Value = 0;
        MaxCountPicker.Value = 0;
        StudentSearchBox.Text = string.Empty;
        foreach (var checkBox in _studentCheckBoxes)
        {
            checkBox.IsChecked = false;
        }
    }

    public void SetFilterCriteria(FilterCriteria criteria)
    {
        if (criteria == null) return;

        if (criteria.StartDate.HasValue)
        {
            StartDatePicker.SelectedDate = criteria.StartDate.Value.Date;
            StartTimePicker.SelectedTime = criteria.StartDate.Value.TimeOfDay;
        }

        if (criteria.EndDate.HasValue)
        {
            EndDatePicker.SelectedDate = criteria.EndDate.Value.Date;
            EndTimePicker.SelectedTime = criteria.EndDate.Value.TimeOfDay;
        }

        MinCountPicker.Value = criteria.MinCount;
        MaxCountPicker.Value = criteria.MaxCount;

        foreach (var checkBox in _studentCheckBoxes)
        {
            if (checkBox.Tag is Student student)
            {
                checkBox.IsChecked = criteria.SelectedStudents.Any(s => s.Id == student.Id);
            }
        }
    }
}

public class FilterCriteria
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int MinCount { get; set; }
    public int MaxCount { get; set; }
    public List<Student> SelectedStudents { get; set; } = new();

    public bool HasAnyFilter()
    {
        return StartDate.HasValue ||
               EndDate.HasValue ||
               MinCount > 0 ||
               MaxCount > 0 ||
               SelectedStudents.Count > 0;
    }

    public string GetSummary()
    {
        var parts = new List<string>();

        if (StartDate.HasValue || EndDate.HasValue)
        {
            var start = StartDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "不限";
            var end = EndDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "不限";
            parts.Add($"时间 {start} 至 {end}");
        }

        if (MinCount > 0 || MaxCount > 0)
        {
            var min = MinCount > 0 ? MinCount.ToString() : "不限";
            var max = MaxCount > 0 ? MaxCount.ToString() : "不限";
            parts.Add($"人数 {min}-{max} 人");
        }

        if (SelectedStudents.Count > 0)
        {
            var names = string.Join("、", SelectedStudents.Select(s => s.Name).Take(5));
            if (SelectedStudents.Count > 5)
            {
                names += $" 等{SelectedStudents.Count}人";
            }
            parts.Add($"学生: {names}");
        }

        return parts.Count > 0 ? "已筛选：" + string.Join("，", parts) : string.Empty;
    }
}
