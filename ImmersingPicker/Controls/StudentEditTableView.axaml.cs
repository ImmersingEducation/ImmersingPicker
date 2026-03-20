using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using ImmersingPicker.Core;
using ImmersingPicker.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace ImmersingPicker.Controls;

public partial class StudentEditTableView : UserControl
{
    private Clazz? _currentClazz;
    
    public event Action? DataChanged;
    
    public StudentEditTableView()
    {
        InitializeComponent();
    }
    
    public StudentEditTableView(Clazz clazz)
    {
        InitializeComponent();
        SetClazz(clazz);
    }
    
    public void SetClazz(Clazz clazz)
    {
        _currentClazz = clazz;
        UpdateStudentList();
    }
    
    private void UpdateStudentList()
    {
        StudentsStackPanel.Children.Clear();
        
        if (_currentClazz != null)
        {
            // 按学号排序
            var sortedStudents = _currentClazz.Students.OrderBy(s => s.Id).ToList();
            
            foreach (var student in sortedStudents)
            {
                var item = new StudentEditTableViewItem { Student = student };
                item.DeleteRequested += OnDeleteRequested;
                StudentsStackPanel.Children.Add(item);
            }
        }
    }
    
    private async void OnDeleteRequested(Student student)
    {
        // 弹出挽留提示
        var dialog = new ContentDialog
        {
            Title = "删除确认",
            Content = $"确定要删除学生 {student.Name} 吗？",
            PrimaryButtonText = "删除",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary
        };
        
        // 显示对话框
        if (TopLevel.GetTopLevel(this) is Window parentWindow)
        {
            var result = await dialog.ShowAsync(parentWindow);
            if (result == ContentDialogResult.Primary && _currentClazz != null)
            {
                _currentClazz.RemoveStudent(student.Id);
                UpdateStudentList();
                DataChanged?.Invoke();
            }
        }
    }
    
    public void AddStudent(Student student)
    {
        if (_currentClazz != null)
        {
            _currentClazz.AddStudent(student.Name, student.Id, (student.SeatRow, student.SeatColumn));
            UpdateStudentList();
            DataChanged?.Invoke();
        }
    }
}