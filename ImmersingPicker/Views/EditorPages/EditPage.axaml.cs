using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using ImmersingPicker.Controls;
using ImmersingPicker.Core.Models;
using Serilog;

namespace ImmersingPicker.Views.EditorPages;

public partial class EditPage : UserControl
{
    private static readonly ILogger _logger = Log.ForContext<EditPage>();

    public EditPage()
    {
        InitializeComponent();
        Clazz.CurrentClassChanged += RefreshClassComboBox;
        RefreshClassComboBox();
    }

    private void RefreshClassComboBox()
    {
        ClazzComboBox.SelectionChanged -= ClazzComboBox_OnSelectionChanged;
        ClazzComboBox.SelectedItem = null;
        ClazzComboBox.Items.Clear();
        foreach (var clazz in Clazz.Classes)
        {
            ClazzComboBox.Items.Add(clazz.Name);
        }
        var current = Clazz.GetCurrentClazz();
        if (current != null)
        {
            ClazzComboBox.SelectedItem = current.Name;
        }
        ClazzComboBox.SelectionChanged += ClazzComboBox_OnSelectionChanged;
        RefreshStudentCards();
    }

    private void ClazzComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is FAComboBox { SelectedItem: string selectedName })
        {
            var current = Clazz.GetCurrentClazz();
            if (current != null && current.Name == selectedName) return;
            Clazz.SetCurrentClazz(selectedName);
            RefreshStudentCards();
        }
    }

    private void RefreshStudentCards()
    {
        StudentPanel.Children.Clear();
        var clazz = Clazz.GetCurrentClazz();
        if (clazz == null) return;
        foreach (var student in clazz.Students)
        {
            var card = new StudentCard
            {
                Student = student,
                Width = 280,
                Margin = new Avalonia.Thickness(2)
            };
            card.EditRequested += OnStudentEditRequested;
            card.DeleteRequested += OnStudentDeleteRequested;
            StudentPanel.Children.Add(card);
        }
    }

    private async void AddClazzButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var textBox = new TextBox { Watermark = "请输入班级名称" };
        var panel = new StackPanel { Spacing = 10 };
        panel.Children.Add(new TextBlock { Text = "请输入新班级名称：" });
        panel.Children.Add(textBox);

        var dialog = new ContentDialog
        {
            Title = "新建班级",
            Content = panel,
            PrimaryButtonText = "确定",
            CloseButtonText = "取消"
        };
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            var name = textBox.Text?.Trim();
            if (string.IsNullOrEmpty(name))
            {
                var warnDialog = new ContentDialog
                {
                    Title = "提示",
                    Content = "班级名称不能为空。",
                    CloseButtonText = "确定"
                };
                await warnDialog.ShowAsync();
                return;
            }
            if (Clazz.CheckIfNameExists(name))
            {
                var warnDialog = new ContentDialog
                {
                    Title = "提示",
                    Content = $"班级\"{name}\"已存在。",
                    CloseButtonText = "确定"
                };
                await warnDialog.ShowAsync();
                return;
            }
            new Clazz(name);
            Clazz.CurrentClassIndex = Clazz.Classes.Count - 1;
            RefreshClassComboBox();
        }
    }

    private async void DeleteClazzButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var clazz = Clazz.GetCurrentClazz();
        if (clazz == null)
        {
            var warnDialog = new ContentDialog
            {
                Title = "提示",
                Content = "当前没有可删除的班级。",
                CloseButtonText = "确定"
            };
            await warnDialog.ShowAsync();
            return;
        }

        var textBox = new TextBox { Watermark = "请输入班级名称" };
        var panel = new StackPanel { Spacing = 10 };
        panel.Children.Add(new TextBlock { Text = $"请输入班级名称以确认删除：" });
        panel.Children.Add(textBox);

        var dialog = new ContentDialog
        {
            Title = "删除班级",
            Content = panel,
            PrimaryButtonText = "确定",
            CloseButtonText = "取消"
        };
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            if (textBox.Text?.Trim() != clazz.Name)
            {
                var warnDialog = new ContentDialog
                {
                    Title = "提示",
                    Content = "输入的班级名称不正确，删除已取消。",
                    CloseButtonText = "确定"
                };
                await warnDialog.ShowAsync();
                return;
            }
            var index = Clazz.Classes.IndexOf(clazz);
            Clazz.Classes.RemoveAt(index);
            if (Clazz.Classes.Count == 0)
            {
                Clazz.CurrentClassIndex = -1;
            }
            else if (index >= Clazz.Classes.Count)
            {
                Clazz.CurrentClassIndex = Clazz.Classes.Count - 1;
            }
            else
            {
                Clazz.CurrentClassIndex = index;
            }
            RefreshClassComboBox();
        }
    }

    private async void AddStudentButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var clazz = Clazz.GetCurrentClazz();
        if (clazz == null)
        {
            var warnDialog = new ContentDialog
            {
                Title = "提示",
                Content = "请先选择或创建一个班级。",
                CloseButtonText = "确定"
            };
            await warnDialog.ShowAsync();
            return;
        }

        var editDialog = new StudentEditCardDialog();
        var contentDialog = new ContentDialog
        {
            Title = "添加学生",
            Content = editDialog,
            PrimaryButtonText = "确定",
            CloseButtonText = "取消"
        };
        var result = await contentDialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            var student = editDialog.GetUpdatedStudent();
            if (string.IsNullOrEmpty(student.Name) || student.Id <= 0)
            {
                var warnDialog = new ContentDialog
                {
                    Title = "提示",
                    Content = "请至少输入姓名和学号。",
                    CloseButtonText = "确定"
                };
                await warnDialog.ShowAsync();
                return;
            }
            if (clazz.CheckIfIdExists(student.Id))
            {
                var warnDialog = new ContentDialog
                {
                    Title = "提示",
                    Content = $"学号 {student.Id} 已存在。",
                    CloseButtonText = "确定"
                };
                await warnDialog.ShowAsync();
                return;
            }
            clazz.AddStudent(student.Name, student.Id, (student.SeatRow, student.SeatColumn), student.Tags);
            RefreshStudentCards();
        }
    }

    private async void OnStudentEditRequested(Student student)
    {
        var editDialog = new StudentEditCardDialog(student);
        var contentDialog = new ContentDialog
        {
            Title = "编辑学生",
            Content = editDialog,
            PrimaryButtonText = "确定",
            CloseButtonText = "取消"
        };
        var result = await contentDialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            editDialog.GetUpdatedStudent();
            RefreshStudentCards();
        }
    }

    private async void OnStudentDeleteRequested(Student student)
    {
        var dialog = new ContentDialog
        {
            Title = "确认删除",
            Content = $"确定要删除学生\"{student.Name}\"（学号：{student.Id}）吗？",
            PrimaryButtonText = "确定",
            CloseButtonText = "取消"
        };
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            var clazz = Clazz.GetCurrentClazz();
            if (clazz == null) return;
            clazz.RemoveStudent(student.Id);
            RefreshStudentCards();
        }
    }
}
