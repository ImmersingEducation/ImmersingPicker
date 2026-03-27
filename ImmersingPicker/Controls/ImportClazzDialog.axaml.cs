using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
using ImmersingPicker.Core;
using ImmersingPicker.Core.Models;
using ImmersingPicker.Services.Services.Picker;
using ImmersingPicker.Services.Services;

namespace ImmersingPicker.Controls;

public partial class ImportClazzDialog : UserControl
{
    public event EventHandler<ImportResult>? ImportCompleted;

    public class ImportResult
    {
        public bool Success { get; set; }
        public List<Student>? Students { get; set; }
        public string? ErrorMessage { get; set; }
        public Clazz? NewClazz { get; set; }
        public bool ShowBatchEditDialog { get; set; }
    }

    public ImportClazzDialog()
    {
        InitializeComponent();
    }

    private async void OnTxtButtonClick(object? sender, RoutedEventArgs e)
    {
        CloseDialog();
        var result = await ImportFromTxtAsync();
        ImportCompleted?.Invoke(this, result);
    }

    private async void OnSecRandomButtonClick(object? sender, RoutedEventArgs e)
    {
        CloseDialog();
        var result = await ImportFromSecRandomAsync();
        ImportCompleted?.Invoke(this, result);
    }

    private void CloseDialog()
    {
        var parentDialog = this.FindAncestorOfType<ContentDialog>();
        parentDialog?.Hide();
    }

    private async Task<ImportResult> ImportFromTxtAsync()
    {
        var parentWindow = TopLevel.GetTopLevel(this) as Window;
        if (parentWindow == null)
        {
            return new ImportResult { Success = false, ErrorMessage = "无法获取父窗口" };
        }

        var storage = await parentWindow.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "选择 TXT 文件",
                AllowMultiple = false,
                FileTypeFilter = new[] { new FilePickerFileType("TXT 文件") { Patterns = new[] { "*.txt" } } }
            });

        if (storage.Count == 0)
        {
            return new ImportResult { Success = false };
        }

        try
        {
            var path = storage[0].Path.LocalPath;
            var fileName = Path.GetFileNameWithoutExtension(path);
            
            if (Clazz.CheckIfNameExists(fileName))
            {
                return new ImportResult 
                { 
                    Success = false, 
                    ErrorMessage = $"已存在名为\"{fileName}\"的班级，请使用其他文件名或重命名文件。" 
                };
            }
            
            var students = ImportClazzService.FromTxt(path);
            var newClazz = ClazzFactory.NewClazz(fileName, students);
            
            return new ImportResult 
            { 
                Success = true, 
                Students = students,
                NewClazz = newClazz,
                ShowBatchEditDialog = true
            };
        }
        catch (Exception ex)
        {
            return new ImportResult { Success = false, ErrorMessage = $"导入失败：{ex.Message}" };
        }
    }

    private async Task<ImportResult> ImportFromSecRandomAsync()
    {
        var parentWindow = TopLevel.GetTopLevel(this) as Window;
        if (parentWindow == null)
        {
            return new ImportResult { Success = false, ErrorMessage = "无法获取父窗口" };
        }

        var storage = await parentWindow.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "选择SecRandom格式文件",
                AllowMultiple = false,
                FileTypeFilter = new[] { new FilePickerFileType("JSON文件") { Patterns = new[] { "*.json" } } }
            });

        if (storage.Count == 0)
        {
            return new ImportResult { Success = false };
        }

        try
        {
            var path = storage[0].Path.LocalPath;
            var studentsWithoutSeat = ImportClazzService.FromSecRandom(path);
            var students = new List<Student>();
            int id = 1;
            foreach (var s in studentsWithoutSeat)
            {
                students.Add(new Student
                {
                    Id = s.Id > 0 ? s.Id : id,
                    Name = s.Name,
                    SeatRow = 0,
                    SeatColumn = id,
                    InitialWeight = 1.0,
                    LastSelectedTime = null,
                    SelectedAmount = 0,
                    Weight = -1.0
                });
                id++;
            }
            return new ImportResult { Success = true, Students = students };
        }
        catch (Exception ex)
        {
            return new ImportResult { Success = false, ErrorMessage = $"导入失败：{ex.Message}" };
        }
    }
}
