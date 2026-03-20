using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using ImmersingPicker.Core;
using ImmersingPicker.Controls;
using ImmersingPicker.Core.Models;
using ImmersingPicker.Helpers;
using Serilog;

namespace ImmersingPicker.Views.MainPages;

public partial class HistoryPage : UserControl
{
    private static readonly ILogger _logger = Log.ForContext<HistoryPage>();
    private Clazz? _clazz;
    private FilterCriteria? _currentFilter;
    private FilterDialog? _filterDialog;

    public HistoryPage()
    {
        _logger.Information("初始化HistoryPage");
        InitializeComponent();
        _logger.Verbose("添加班级变更事件处理");
        Clazz.CurrentClassChanged += OnCurrentClassChanged;
        _logger.Verbose("添加按钮点击事件处理");
        ClearHistoryButton.Click += OnClearHistoryButtonClick;
        FilterButton.Click += OnFilterButtonClick;
        ResetFilterButton.Click += OnResetFilterButtonClick;
        ClearFilterButton.Click += OnResetFilterButtonClick;
        _logger.Verbose("获取当前班级");
        _clazz = Clazz.GetCurrentClazz();
        if (_clazz != null)
        {
            _logger.Information("当前班级: {ClassName}", _clazz.Name);
            _logger.Verbose("添加历史记录变更事件处理");
            _clazz.HistoriesChanged += OnHistoriesChanged;
            TitleText.Text = $"班级 {_clazz.Name} 的历史记录";
            _logger.Verbose("加载历史记录");
            LoadHistories();
        }
        else
        {
            _logger.Warning("当前班级为null");
        }
        _logger.Information("HistoryPage初始化完成");
    }

    public void RefreshClazz()
    {
        _logger.Information("刷新班级信息");
        if (_clazz != null)
        {
            _logger.Verbose("移除历史记录变更事件处理");
            _clazz.HistoriesChanged -= OnHistoriesChanged;
        }
        _logger.Verbose("获取当前班级");
        _clazz = Clazz.GetCurrentClazz();
        if (_clazz == null)
        {
            _logger.Warning("当前班级为null，无法刷新");
            return;
        }
        _logger.Information("当前班级: {ClassName}", _clazz.Name);
        _logger.Verbose("添加历史记录变更事件处理");
        _clazz.HistoriesChanged += OnHistoriesChanged;
        TitleText.Text = $"班级 {_clazz.Name} 的历史记录";
        _logger.Verbose("加载历史记录");
        LoadHistories();
        _logger.Information("班级信息刷新完成");
    }

    private void OnHistoriesChanged()
    {
        LoadHistories();
    }

    private void OnCurrentClassChanged()
    {
        RefreshClazz();
    }

    private void LoadHistories()
    {
        _logger.Information("加载历史记录");
        if (_clazz == null)
        {
            _logger.Warning("当前班级为null，无法加载历史记录");
            return;
        }

        _logger.Verbose("清空历史记录容器");
        HistoryItemsContainer.Children.Clear();

        var histories = _clazz.Histories.AsEnumerable();
        _logger.Debug("原始历史记录数量: {Count}", histories.Count());

        if (_currentFilter != null && _currentFilter.HasAnyFilter())
        {
            _logger.Information("应用筛选条件");
            histories = ApplyFilter(histories);
            _logger.Debug("筛选后历史记录数量: {Count}", histories.Count());
        }

        var sortedHistories = histories.OrderByDescending(h => h.CreateTime);

        if (sortedHistories.Any())
        {
            _logger.Information("显示历史记录，共{Count}条", sortedHistories.Count());
            foreach (var history in sortedHistories)
            {
                _logger.Verbose("添加历史记录项: {Time}", history.CreateTime);
                var historyItem = new HistoryItem(history);
                historyItem.Clicked += OnHistoryItemClicked;
                HistoryItemsContainer.Children.Add(historyItem);
            }
        }
        else
        {
            _logger.Information("没有历史记录可显示");
            var noHistoryText = new TextBlock
            {
                Text = _currentFilter != null && _currentFilter.HasAnyFilter() 
                    ? "没有符合筛选条件的历史记录" 
                    : "暂无历史记录",
                FontWeight = Avalonia.Media.FontWeight.Bold,
                FontSize = 32,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = new Avalonia.Thickness(0, 100, 0, 0)
            };
            HistoryItemsContainer.Children.Add(noHistoryText);
        }
        _logger.Information("历史记录加载完成");
    }

    private IEnumerable<History> ApplyFilter(IEnumerable<History> histories)
    {
        if (_currentFilter == null) return histories;

        var filtered = histories;

        if (_currentFilter.StartDate.HasValue)
        {
            var startDateTime = _currentFilter.StartDate.Value;
            filtered = filtered.Where(h => h.CreateTime >= startDateTime);
        }

        if (_currentFilter.EndDate.HasValue)
        {
            var endDateTime = _currentFilter.EndDate.Value;
            filtered = filtered.Where(h => h.CreateTime <= endDateTime);
        }

        if (_currentFilter.MinCount > 0)
        {
            filtered = filtered.Where(h => h.Students.Count >= _currentFilter.MinCount);
        }

        if (_currentFilter.MaxCount > 0)
        {
            filtered = filtered.Where(h => h.Students.Count <= _currentFilter.MaxCount);
        }

        if (_currentFilter.SelectedStudents.Count > 0)
        {
            filtered = filtered.Where(h => 
                h.Students.Any(s => _currentFilter.SelectedStudents.Any(ss => ss.Id == s.Id)));
        }

        return filtered;
    }

    private async void OnFilterButtonClick(object? sender, EventArgs e)
    {
        if (_clazz == null) return;

        _filterDialog = new FilterDialog(_clazz.Students);

        if (_currentFilter != null && _currentFilter.HasAnyFilter())
        {
            _filterDialog.SetFilterCriteria(_currentFilter);
        }

        var dialog = new ContentDialog
        {
            Title = "筛选历史记录",
            Content = _filterDialog,
            PrimaryButtonText = "确定",
            CloseButtonText = "取消"
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            _currentFilter = _filterDialog.GetFilterCriteria();
            UpdateFilterSummary();
            LoadHistories();
        }
    }

    private void OnResetFilterButtonClick(object? sender, EventArgs e)
    {
        _currentFilter = null;
        _filterDialog?.ResetFilter();
        UpdateFilterSummary();
        LoadHistories();
    }

    private void UpdateFilterSummary()
    {
        var hasFilter = _currentFilter != null && _currentFilter.HasAnyFilter();
        
        ResetFilterButton.IsEnabled = hasFilter;
        FilterSummaryBorder.IsVisible = hasFilter;

        if (hasFilter && _currentFilter != null)
        {
            FilterSummaryText.Text = _currentFilter.GetSummary();
        }
        else
        {
            FilterSummaryText.Text = string.Empty;
        }
    }
    
    private async void OnHistoryItemClicked(object? sender, EventArgs e)
    {
        if (sender is not HistoryItem historyItem) return;
        var history = historyItem.History;
        if (history == null) return;
        
        string studentsList = string.Join("\n", history.Students.Select(s => $"{s.Id} {s.Name}"));
        string content = $"时间：{history.CreateTime.ToString()}\n\n" +
                        $"抽选器：{history.Selector}\n\n" +
                        $"抽选到的学生：\n{studentsList}";
        
        var dialog = new ContentDialog
        {
            Title = "抽选历史详情",
            Content = content,
            CloseButtonText = "确定"
        };
        
        await dialog.ShowAsync();
    }
    
    private async void OnClearHistoryButtonClick(object? sender, EventArgs e)
    {
        _logger.Information("清空历史记录按钮点击");
        if (_clazz == null)
        {
            _logger.Warning("当前班级为null，无法清空历史记录");
            return;
        }
        
        _logger.Information("显示清空历史记录操作选择对话框");
        var dialog = new ContentDialog
        {
            Title = "清空历史记录",
            Content = "请选择要执行的操作：",
            PrimaryButtonText = "清空历史记录",
            SecondaryButtonText = "清空历史记录并重置权重",
            CloseButtonText = "取消"
        };
        
        var result = await dialog.ShowAsync();
        
        if (result == ContentDialogResult.Primary || 
            result == ContentDialogResult.Secondary)
        {
            _logger.Information("用户选择了{Operation}操作", result == ContentDialogResult.Primary ? "清空历史记录" : "清空历史记录并重置权重");
            
            // 密码验证
            var parentWindow = TopLevel.GetTopLevel(this) as Window;
            if (parentWindow == null)
            {
                _logger.Error("无法获取父窗口");
                return;
            }
            
            bool verified = await VerifyHelper.VerifyPassword(parentWindow);
            if (!verified)
            {
                _logger.Information("密码验证失败，取消操作");
                return;
            }
            
            var confirmDialog = new ContentDialog
            {
                Title = "确认操作",
                Content = "确定要执行此操作吗？此操作不可恢复。",
                PrimaryButtonText = "确定",
                CloseButtonText = "取消"
            };
            
            var confirmResult = await confirmDialog.ShowAsync();
            
            if (confirmResult == ContentDialogResult.Primary)
            {
                _logger.Information("用户确认执行操作");
                try
                {
                    if (result == ContentDialogResult.Primary)
                    {
                        _logger.Information("执行清空历史记录操作");
                        _clazz.Histories.Clear();
                    }
                    else
                    {
                        _logger.Information("执行清空历史记录并重置权重操作");
                        _clazz.Histories.Clear();
                        foreach (var student in _clazz.Students)
                        {
                            _logger.Verbose("重置学生{Name}的历史记录", student.Name);
                            student.resetHistories();
                        }
                    }
                    
                    bool success = false;
                    try
                    {
                        _logger.Verbose("保存班级数据");
                        var storageService = new ImmersingPicker.Services.Services.Storage.ClassStorageService();
                        storageService.SaveClasses(Clazz.Classes);
                        success = true;
                        _logger.Information("班级数据保存成功");
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "保存班级数据失败");
                    }
                    
                    _logger.Verbose("重新加载历史记录");
                    LoadHistories();
                    
                    var successDialog = new ContentDialog
                    {
                        Title = success ? "操作成功" : "操作失败",
                        Content = success ? "历史记录已成功清空" : "清空历史记录时发生错误",
                        CloseButtonText = "确定"
                    };
                    
                    await successDialog.ShowAsync();
                    _logger.Information("操作完成，结果: {Result}", success ? "成功" : "失败");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "执行清空历史记录操作失败");
                }
            }
            else
            {
                _logger.Information("用户取消执行操作");
            }
        }
        else
        {
            _logger.Information("用户取消操作");
        }
    }
}
