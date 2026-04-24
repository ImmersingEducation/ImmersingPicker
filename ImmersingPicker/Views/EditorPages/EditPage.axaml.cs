using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using ImmersingPicker.Controls;
using ImmersingPicker.Core;
using ImmersingPicker.Core.Abstractions.Picker;
using ImmersingPicker.Core.Models;
using ImmersingPicker.Services.Services.Picker;
using ImmersingPicker.Services.Services.Storage;

namespace ImmersingPicker.Views.EditorPages;

public partial class EditPage : UserControl
{
    private Clazz? _currentClazz;
    public bool IsModified { get; set; } = false;

    public EditPage()
    {
        InitializeComponent();
        InitializeClazzComboBox();
        UpdateCreateStudentButtonState();
    }

    private void InitializeClazzComboBox()
    {
        ClazzComboBox.Items.Clear();

        foreach (var clazz in Clazz.Classes)
        {
            ClazzComboBox.Items.Add(clazz.Name);
        }

        if (Clazz.Classes.Count > 0)
        {
            ClazzComboBox.SelectedIndex = 0;
            _currentClazz = Clazz.Classes[0];
            UpdateStudentCards();
        }
        UpdateCreateStudentButtonState();
    }

    private void UpdateCreateStudentButtonState()
    {
        if (CreateStudentButton != null)
        {
            CreateStudentButton.IsEnabled = ClazzComboBox.SelectedIndex >= 0;
        }
    }

    private void ClazzComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ClazzComboBox.SelectedIndex >= 0 && ClazzComboBox.SelectedIndex < Clazz.Classes.Count)
        {
            _currentClazz = Clazz.Classes[ClazzComboBox.SelectedIndex];
            UpdateStudentCards();
        }
        UpdateCreateStudentButtonState();
    }

    private void UpdateStudentCards()
    {
        StudentsWrapPanel.Children.Clear();

        if (_currentClazz != null)
        {
            // 按学号排序
            var sortedStudents = _currentClazz.Students.OrderBy(s => s.Id).ToList();

            foreach (var student in sortedStudents)
            {
                var card = new StudentCard { Student = student };
                card.EditRequested += OnEditRequested;
                card.DeleteRequested += OnDeleteRequested;
                StudentsWrapPanel.Children.Add(card);
            }
        }
    }

    private async void OnEditRequested(Student student)
    {
        var editDialog = new StudentEditCardDialog(student);

        var dialog = new ContentDialog
        {
            Content = editDialog,
            PrimaryButtonText = "保存",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary
        };

        if (TopLevel.GetTopLevel(this) is Window parentWindow)
        {
            var result = await dialog.ShowAsync(parentWindow);
            if (result == ContentDialogResult.Primary)
            {
                var updatedStudent = editDialog.GetUpdatedStudent();
                if (updatedStudent != null)
                {
                    // 触发自动保存
                    await AutoSave();
                }
            }
        }
    }

    private async void OnDeleteRequested(Student student)
    {
        // 弹出删除确认
        var dialog = new ContentDialog
        {
            Title = "删除确认",
            Content = $"确定要删除学生 {student.Name} 吗？",
            PrimaryButtonText = "删除",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary
        };

        if (TopLevel.GetTopLevel(this) is Window parentWindow)
        {
            var result = await dialog.ShowAsync(parentWindow);
            if (result == ContentDialogResult.Primary && _currentClazz != null)
            {
                _currentClazz.RemoveStudent(student.Id);
                UpdateStudentCards();
                await AutoSave();
            }
        }
    }

    private async void CreateClazzButton_OnClick(object? sender, RoutedEventArgs e)
    {
        // 弹出弹窗输入班级名称
        var grid = new Grid { RowDefinitions = new RowDefinitions("Auto,Auto") };
        
        var textBlock = new TextBlock
        {
            Text = "请输入班级名称：",
            Margin = new Thickness(0, 0, 0, 10),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
        };
        Grid.SetRow(textBlock, 0);
        grid.Children.Add(textBlock);
        
        var textBox = new TextBox
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
        };
        Grid.SetRow(textBox, 1);
        grid.Children.Add(textBox);
        
        var dialog = new ContentDialog
        {
            Title = "新建班级",
            Content = grid,
            PrimaryButtonText = "确认",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary
        };
        
        var parentWindow = TopLevel.GetTopLevel(this) as Window;
        if (parentWindow != null)
        {
            var result = await dialog.ShowAsync(parentWindow);
            if (result == ContentDialogResult.Primary && !string.IsNullOrEmpty(textBox.Text))
            {
                if (Clazz.CheckIfNameExists(textBox.Text))
                {
                    // 弹出重名提示
                    var errorDialog = new ContentDialog
                    {
                        Title = "创建失败",
                        Content = "已存在同名班级，请使用其他名称。",
                        CloseButtonText = "确定"
                    };
                    await errorDialog.ShowAsync(parentWindow);
                }
                else
                {
                    // 使用 ClazzFactory.NewClazz 创建班级，自动创建抽选器并设置当前班级
                    var newClazz = ClazzFactory.NewClazz(textBox.Text);
                    
                    InitializeClazzComboBox();
                    await AutoSave();
                }
            }
        }
    }

    private async void CreateStudentButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (ClazzComboBox.SelectedIndex < 0 || _currentClazz == null)
            return;

        // 使用卡片样式的编辑对话框
        var editDialog = new StudentEditCardDialog();

        var dialog = new ContentDialog
        {
            Content = editDialog,
            PrimaryButtonText = "确认",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary
        };

        var parentWindow = TopLevel.GetTopLevel(this) as Window;
        if (parentWindow != null)
        {
            var result = await dialog.ShowAsync(parentWindow);
            if (result == ContentDialogResult.Primary)
            {
                var student = editDialog.GetUpdatedStudent();
                if (student != null && !string.IsNullOrEmpty(student.Name) && student.Id > 0)
                {
                    _currentClazz.AddStudent(student.Name, student.Id, (student.SeatRow, student.SeatColumn), student.Tags);
                    UpdateStudentCards();
                    await AutoSave();
                }
            }
        }
    }

    private async Task AutoSave()
    {
        try
        {
            var storageService = ClassStorageService.Instance;
            storageService.SaveClasses(Clazz.Classes);
            IsModified = false;
            
            // 触发班级变化事件，以便HomePage刷新
            Clazz.OnCurrentClassChanged();
        }
        catch (Exception)
        {
            // 显示保存失败提示
            var parentWindow = TopLevel.GetTopLevel(this) as Window;
            if (parentWindow != null)
            {
                var dialog = new ContentDialog
                {
                    Title = "保存失败",
                    Content = "保存数据时发生错误。",
                    CloseButtonText = "确定"
                };
                await dialog.ShowAsync(parentWindow);
            }
        }
    }

    private async void SaveButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await AutoSave();

        // 显示保存成功提示
        var parentWindow = TopLevel.GetTopLevel(this) as Window;
        if (parentWindow != null)
        {
            var dialog = new ContentDialog
            {
                Title = "保存成功",
                Content = "班级数据已保存。",
                CloseButtonText = "确定"
            };
            await dialog.ShowAsync(parentWindow);
        }
    }

    private async void ImportButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var importDialog = new Controls.ImportClazzDialog();
        importDialog.ImportCompleted += async (_, result) =>
        {
            await ProcessImportResult(result);
        };

        var dialog = new ContentDialog
        {
            Title = "选择数据源",
            Content = importDialog,
            CloseButtonText = "取消"
        };

        var parentWindow = TopLevel.GetTopLevel(this) as Window;
        if (parentWindow != null)
        {
            await dialog.ShowAsync(parentWindow);
        }
    }

    private async Task ProcessImportResult(Controls.ImportClazzDialog.ImportResult result)
    {
        var parentWindow = TopLevel.GetTopLevel(this) as Window;
        if (parentWindow == null) return;

        if (!result.Success)
        {
            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                var errorDialog = new ContentDialog
                {
                    Title = "导入失败",
                    Content = result.ErrorMessage,
                    CloseButtonText = "确定"
                };
                await errorDialog.ShowAsync(parentWindow);
            }
            return;
        }

        if (result.Students == null || result.Students.Count == 0)
        {
            var emptyDialog = new ContentDialog
            {
                Title = "导入失败",
                Content = "文件中没有找到有效的学生数据。",
                CloseButtonText = "确定"
            };
            await emptyDialog.ShowAsync(parentWindow);
            return;
        }

        if (result.NewClazz != null)
        {
            InitializeClazzComboBox();
            await AutoSave();

            var successDialog = new ContentDialog
            {
                Title = "导入成功",
                Content = $"已创建新班级\"{result.NewClazz.Name}\"并导入 {result.Students.Count} 名学生。",
                CloseButtonText = "确定"
            };
            await successDialog.ShowAsync(parentWindow);

            if (result.ShowBatchEditDialog && result.Students.Count > 0)
            {
                await ShowBatchEditSeatDialog(result.NewClazz);
            }
        }
        else
        {
            var selectedClazz = Clazz.Classes[ClazzComboBox.SelectedIndex];
            selectedClazz.Students.AddRange(result.Students);
            UpdateStudentCards();
            await AutoSave();

            var successDialog = new ContentDialog
            {
                Title = "导入成功",
                Content = $"已成功导入 {result.Students.Count} 名学生。",
                CloseButtonText = "确定"
            };
            await successDialog.ShowAsync(parentWindow);
        }
    }

    private async Task ShowBatchEditSeatDialog(Clazz clazz)
    {
        var parentWindow = TopLevel.GetTopLevel(this) as Window;
        if (parentWindow == null || clazz.Students.Count == 0) return;

        var batchDialog = new Controls.BatchSeatEditDialog(clazz);

        var contentDialog = new ContentDialog
        {
            Title = "批量设置座位",
            Content = batchDialog,
            PrimaryButtonText = "保存",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Primary
        };

        var result = await contentDialog.ShowAsync(parentWindow);

        if (result == ContentDialogResult.Primary && batchDialog.SaveClicked)
        {
            UpdateStudentCards();
            await AutoSave();

            var successDialog = new ContentDialog
            {
                Title = "保存成功",
                Content = "所有学生的座位信息已更新。",
                CloseButtonText = "确定"
            };
            await successDialog.ShowAsync(parentWindow);
        }
    }

    private async void BatchEditSeatButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (ClazzComboBox.SelectedIndex < 0 || ClazzComboBox.SelectedIndex >= Clazz.Classes.Count)
            return;
        
        var selectedClazz = Clazz.Classes[ClazzComboBox.SelectedIndex];
        
        if (selectedClazz.Students.Count == 0)
        {
            var tipDialog = new ContentDialog
            {
                Title = "提示",
                Content = "当前班级没有学生，无法批量设置座位。",
                CloseButtonText = "确定"
            };
            
            var parentWindow = TopLevel.GetTopLevel(this) as Window;
            if (parentWindow != null)
            {
                await tipDialog.ShowAsync(parentWindow);
            }
            return;
        }
        
        var batchDialog = new Controls.BatchSeatEditDialog(selectedClazz);
        
        var parentWindow2 = TopLevel.GetTopLevel(this) as Window;
        if (parentWindow2 != null)
        {
            var contentDialog = new ContentDialog
            {
                Title = "批量设置座位",
                Content = batchDialog,
                PrimaryButtonText = "保存",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary
            };
            
            var result = await contentDialog.ShowAsync(parentWindow2);
            
            if (result == ContentDialogResult.Primary)
            {
                if (batchDialog.SaveClicked)
                {
                    UpdateStudentCards();
                    await AutoSave();

                    var successDialog = new ContentDialog
                    {
                        Title = "保存成功",
                        Content = "所有学生的座位信息已更新。",
                        CloseButtonText = "确定"
                    };
                    await successDialog.ShowAsync(parentWindow2);
                }
            }
        }
    }
}