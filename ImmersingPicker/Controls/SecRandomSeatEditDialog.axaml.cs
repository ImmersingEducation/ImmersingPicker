using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
using ImmersingPicker.Core.Models;

namespace ImmersingPicker.Controls;

public partial class SecRandomSeatEditDialog : UserControl
{
    private List<Student>? _students;
    private Dictionary<int, (TextBox rowBox, TextBox columnBox)> _seatEditors = new();
    
    public bool SaveClicked { get; private set; }
    
    public SecRandomSeatEditDialog()
    {
        InitializeComponent();
        
        SaveButton.Click += OnSaveButtonClick;
        CancelButton.Click += OnCancelClick;
    }
    
    public SecRandomSeatEditDialog(List<Student> students)
    {
        InitializeComponent();
        SetStudents(students);
        
        SaveButton.Click += OnSaveButtonClick;
        CancelButton.Click += OnCancelClick;
    }
    
    public void SetStudents(List<Student> students)
    {
        _students = students;
        LoadStudents();
    }
    
    private void LoadStudents()
    {
        StudentsStackPanel.Children.Clear();
        _seatEditors.Clear();
        
        if (_students == null)
            return;
        
        var sortedStudents = _students.OrderBy(s => s.Id).ToList();
        
        foreach (var student in sortedStudents)
        {
            var mainPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10,
                Margin = new Thickness(0, 5)
            };
            
            var nameTextBlock = new TextBlock
            {
                Text = $"{student.Name} (学号：{student.Id})",
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = Avalonia.Media.FontWeight.SemiBold,
                Width = 200
            };
            mainPanel.Children.Add(nameTextBlock);
            
            var seatLabelBlock = new TextBlock
            {
                Text = "座位：",
                VerticalAlignment = VerticalAlignment.Center
            };
            mainPanel.Children.Add(seatLabelBlock);
            
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                VerticalAlignment = VerticalAlignment.Center
            };
            
            var rowTextBox = new TextBox
            {
                Width = 60,
                Watermark = "行",
                Text = student.SeatRow > 0 ? student.SeatRow.ToString() : "",
                Tag = student.Id
            };
            panel.Children.Add(rowTextBox);
            
            var commaTextBlock = new TextBlock
            {
                Text = "，",
                VerticalAlignment = VerticalAlignment.Center
            };
            panel.Children.Add(commaTextBlock);
            
            var columnTextBox = new TextBox
            {
                Width = 60,
                Watermark = "列",
                Text = student.SeatColumn > 0 ? student.SeatColumn.ToString() : "",
                Tag = student.Id
            };
            panel.Children.Add(columnTextBox);
            
            mainPanel.Children.Add(panel);
            
            _seatEditors[student.Id] = (rowTextBox, columnTextBox);
            StudentsStackPanel.Children.Add(mainPanel);
        }
    }
    
    private void OnSaveButtonClick(object? sender, RoutedEventArgs e)
    {
        if (_students == null)
            return;
        
        if (!ValidateSeatInputs(out string errorMessage))
        {
            ShowErrorDialog(errorMessage);
            return;
        }
        
        SaveClicked = true;
        CloseDialog();
    }
    
    private bool ValidateSeatInputs(out string errorMessage)
    {
        errorMessage = "";
        
        if (_students == null)
        {
            errorMessage = "当前没有可操作的数据";
            return false;
        }
        
        foreach (var student in _students)
        {
            if (_seatEditors.TryGetValue(student.Id, out var editors))
            {
                if (int.TryParse(editors.rowBox.Text, out int row) && 
                    int.TryParse(editors.columnBox.Text, out int column))
                {
                    student.SeatRow = row;
                    student.SeatColumn = column;
                }
                else
                {
                    errorMessage = $"学生 {student.Name} 的座位信息无效，请输入有效的数字。";
                    return false;
                }
            }
        }
        
        return true;
    }
    
    private void ShowErrorDialog(string message)
    {
        var errorDialog = new ContentDialog
        {
            Title = "输入错误",
            Content = message,
            CloseButtonText = "确定"
        };
        
        if (TopLevel.GetTopLevel(this) is Window parentWindow)
        {
            errorDialog.ShowAsync(parentWindow);
        }
    }
    
    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        SaveClicked = false;
        CloseDialog();
    }
    
    private void CloseDialog()
    {
        var parentDialog = this.FindAncestorOfType<ContentDialog>();
        parentDialog?.Hide();
    }
}
