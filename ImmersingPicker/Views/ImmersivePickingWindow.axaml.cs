using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Windowing;
using ImmersingPicker.Core.Exceptions;
using ImmersingPicker.Core.Models;
using ImmersingPicker.Helpers;
using ImmersingPicker.Services.Services;
using Serilog;

namespace ImmersingPicker.Views;

public partial class ImmersivePickingWindow: AppWindow
{
    private static readonly ILogger _logger = Log.ForContext<ImmersivePickingWindow>();
    private int _amountForPicking;

    private bool _isPicking = false;

    private Clazz? _clazz;

    public event EventHandler? WindowActivated;
    public event EventHandler? WindowDeactivated;

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
                    if (!_isPicking)
                    {
                        PickButton.Content = $"共{_amountForPicking}人  开始抽选！";
                    }
                }
            }
            catch (NullReferenceException)
            {
                _amountForPicking = 1;
                PickButton.Content = $"共{_amountForPicking}人  开始抽选！";
            }
        }
    }

    public ImmersivePickingWindow()
    {
        _logger.Information("初始化HomePage");
        InitializeComponent();
        Seats.MagnifyingLens = 1.2;
        _logger.Verbose("添加班级变更事件处理");
        Clazz.CurrentClassChanged += Reset;
        _logger.Verbose("添加班级下拉框选择变更事件处理");
        ClazzComboBox.SelectionChanged += ClazzComboBox_OnSelectionChanged;
        _logger.Verbose("执行重置操作");
        Reset();
        _logger.Information("HomePage初始化完成");
        WindowState = WindowState.FullScreen;
        Topmost = true;
        
        Activated += (s, e) => WindowActivated?.Invoke(this, EventArgs.Empty);
        Deactivated += (s, e) => WindowDeactivated?.Invoke(this, EventArgs.Empty);
    }

    private void ClazzComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _logger.Information("班级选择变更事件触发");
        if (sender is FAComboBox { SelectedItem: string selectedClazzName })
        {
            var currentClazz = Clazz.GetCurrentClazz();
            if (currentClazz != null && currentClazz.Name == selectedClazzName)
            {
                _logger.Verbose("选择的班级已经是当前班级，无需切换");
                return;
            }
            Clazz.SetCurrentClazz(selectedClazzName);
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
        try
        {
            if (AppSettings.Instance.EnableClassIslandLinkage && AppSettings.Instance.EnableDisablingAfterClasses &&
                ClassIslandIPCService.Instance.Initialized && !ClassIslandIPCService.Instance.OnClass())
            {
                var disablementDialog = new ContentDialog
                {
                    Title = "课间禁用",
                    Content = "当前为课间休息，无法使用抽选功能。",
                    CloseButtonText = "确定"
                };

                if (AppSettings.Instance.OpenPassword && AppSettings.Instance.PasswordHash != string.Empty)
                {
                    disablementDialog.PrimaryButtonText = "验证以使用";
                }

                var result = await disablementDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    bool verified = await VerifyHelper.VerifyPassword(this);

                    if (!verified) return;
                }
                else return;
            }

            if (_isPicking && AppSettings.Instance.PickAnimationPlayMode ==
                AppSettings.PickAnimationPlayModeEnum.Manual)
            {
                _isPicking = false;
                return;
            }

            _logger.Information("开始执行抽选操作");
            Clazz? currentClazz = Clazz.GetCurrentClazz();
            if (currentClazz == null)
            {
                _logger.Warning("当前班级为null，无法执行抽选");
                return;
            }

            if (currentClazz.Students.Count == 0)
            {
                _logger.Warning("当前班级没有学生，无法执行抽选");
                var noStudentDialog = new ContentDialog
                {
                    Title = "抽选失败",
                    Content = "当前班级没有学生，无法执行抽选。",
                    CloseButtonText = "确定"
                };
                await noStudentDialog.ShowAsync();
                return;
            }

            if (!currentClazz.Pickers.ContainsKey("FairStudentPicker") ||
                !currentClazz.Pickers.ContainsKey("PlainStudentPicker"))
            {
                _logger.Error("抽选器不存在");
                var noPickerDialog = new ContentDialog
                {
                    Title = "抽选失败",
                    Content = "抽选器初始化失败，请检查班级配置。",
                    CloseButtonText = "确定"
                };
                await noPickerDialog.ShowAsync();
                return;
            }

            switch (AppSettings.Instance.PickAnimationPlayMode)
            {
                case AppSettings.PickAnimationPlayModeEnum.Auto:
                    _logger.Verbose("当前为自动模式，开始动画效果");
                    _isPicking = true;
                    PickButton.IsEnabled = false;
                    ClearButton.IsEnabled = false;
                    for (int i = 0; i < AppSettings.Instance.HomeAnimationPlayAmount; i++)
                    {
                        Seats.DeselectAll();
                        foreach (Student student in currentClazz.Pickers["PlainStudentPicker"].Pick(currentClazz, AmountForPicking).Students)
                        {
                            Seats.Select(student);
                        }
                        await Task.Delay(AppSettings.Instance.HomeAnimationPlayDelay);
                    }
                    _isPicking = false;
                    PickButton.IsEnabled = true;
                    ClearButton.IsEnabled = true;
                    break;
                case AppSettings.PickAnimationPlayModeEnum.Manual:
                    _logger.Verbose("当前为手动模式，开始动画效果");
                    _isPicking = true;
                    PickButton.Content = "点击结束";
                    ClearButton.IsEnabled = false;
                    while (_isPicking)
                    {
                        Seats.DeselectAll();
                        foreach (Student student in currentClazz.Pickers["PlainStudentPicker"].Pick(currentClazz, AmountForPicking).Students)
                        {
                            Seats.Select(student);
                        }
                        await Task.Delay(AppSettings.Instance.HomeAnimationPlayDelay);
                    }
                    PickButton.Content = $"共{_amountForPicking}人  开始抽选！";
                    ClearButton.IsEnabled = true;
                    break;
                case AppSettings.PickAnimationPlayModeEnum.Direct:
                    _logger.Verbose("当前为直接显示结果模式");
                    break;
            }

            _logger.Information("使用公平抽选器抽选{Amount}名学生", AmountForPicking);
            List<Student> picked = currentClazz.Pickers["FairStudentPicker"].Pick(currentClazz, AmountForPicking).Students;
            _logger.Information("抽选完成，结果: {PickedStudents}", string.Join(", ", picked.Select(s => s.Name)));

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
        catch (NoAvailableStudentException)
        {
            _logger.Error("没有可用的学生进行抽选");
            var errorDialog = new ContentDialog
            {
                Title = "抽选失败",
                Content = "没有可用的学生进行抽选。",
                CloseButtonText = "确定"
            };
            await errorDialog.ShowAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "抽选过程中发生错误");
            var errorDialog = new ContentDialog
            {
                Title = "抽选错误",
                Content = $"抽选过程中发生错误：{ex.Message}",
                CloseButtonText = "确定"
            };
            await errorDialog.ShowAsync();
        }
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
        ClazzComboBox.SelectionChanged -= ClazzComboBox_OnSelectionChanged;
        ClazzComboBox.SelectedItem = null;
        ClazzComboBox.Items.Clear();
        foreach (var clazz in Clazz.Classes)
        {
            _logger.Debug("添加班级到下拉框: {ClassName}", clazz.Name);
            ClazzComboBox.Items.Add(clazz.Name);
        }

        // 选择当前班级
        if (_clazz != null)
        {
            _logger.Information("选择当前班级: {ClassName}", _clazz.Name);
            ClazzComboBox.SelectedItem = _clazz.Name;
        }
        else
        {
            _logger.Warning("当前班级为null");
        }
        ClazzComboBox.SelectionChanged += ClazzComboBox_OnSelectionChanged;
        _logger.Information("重置完成");
    }

    private void MinimizeButton_OnClick(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }
}