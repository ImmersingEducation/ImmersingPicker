using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using FluentAvalonia.UI.Controls;
using ImmersingPicker.Core.Models;
using ImmersingPicker.Helpers;
using ImmersingPicker.Services.Services.Security;

namespace ImmersingPicker.Controls;

public partial class BatchSeatEditDialog : UserControl
{
    private Clazz? _currentClazz;
    private Dictionary<int, (TextBox rowBox, TextBox columnBox)> _seatEditors = new();
    
    public bool SaveClicked { get; private set; }
    
    public BatchSeatEditDialog()
    {
        InitializeComponent();
        
        SaveButton.Click += OnSaveButtonClick;
        VerifyButton.Click += OnVerifyButtonClick;
        CancelButton.Click += OnCancelClick;
        
        InitializeVerifyButtonVisibility();
    }
    
    public BatchSeatEditDialog(Clazz clazz)
    {
        InitializeComponent();
        SetClazz(clazz);
        
        SaveButton.Click += OnSaveButtonClick;
        VerifyButton.Click += OnVerifyButtonClick;
        CancelButton.Click += OnCancelClick;
        
        InitializeVerifyButtonVisibility();
    }
    
    private void InitializeVerifyButtonVisibility()
    {
        VerifyButton.IsVisible = AppSettings.Instance.OpenPassword && PasswordService.Instance.HasPassword;
    }
    
    public void SetClazz(Clazz clazz)
    {
        _currentClazz = clazz;
        LoadStudents();
    }
    
    private void LoadStudents()
    {
        StudentsStackPanel.Children.Clear();
        _seatEditors.Clear();
        
        if (_currentClazz == null)
            return;
        
        var sortedStudents = _currentClazz.Students.OrderBy(s => s.Id).ToList();
        
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
        if (_currentClazz == null)
            return;
        
        if (!ValidateSeatInputs(out string errorMessage))
        {
            ShowErrorDialog(errorMessage);
            return;
        }
        
        if (VerifyButton.IsVisible)
        {
            CloseDialog();
            _ = ShowVerifyDialogAndSave();
        }
        else
        {
            SaveClicked = true;
            CloseDialog();
        }
    }
    
    private async Task ShowVerifyDialogAndSave()
    {
        if (TopLevel.GetTopLevel(this) is not Window parentWindow)
            return;
        
        bool verified = await VerifyHelper.VerifyPassword(parentWindow);
        
        if (verified)
        {
            SaveClicked = true;
        }
    }
    
    private bool ValidateSeatInputs(out string errorMessage)
    {
        errorMessage = "";
        
        if (_currentClazz == null)
        {
            errorMessage = "当前没有可操作的数据";
            return false;
        }
        
        foreach (var student in _currentClazz.Students)
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
    
    private async void OnVerifyButtonClick(object? sender, RoutedEventArgs e)
    {
        if (_currentClazz == null)
            return;
        
        if (!ValidateSeatInputs(out string errorMessage))
        {
            ShowErrorDialog(errorMessage);
            return;
        }
        
        CloseDialog();
        
        if (TopLevel.GetTopLevel(this) is Window parentWindow)
        {
            bool verified = await VerifyHelper.VerifyPassword(parentWindow);
            
            if (verified)
            {
                SaveClicked = true;
            }
        }
    }
    
    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        SaveClicked = false;
        CloseDialog();
    }
    
    private void CloseDialog()
    {
        if (TopLevel.GetTopLevel(this) is Window parentWindow)
        {
            parentWindow.Close();
        }
    }
}
