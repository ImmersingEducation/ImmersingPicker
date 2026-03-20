using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Windowing;
using ImmersingPicker.Views.EditorPages;
using FluentAvalonia.UI.Controls;
using Serilog;

namespace ImmersingPicker.Views;

public partial class EditorWindow : AppWindow
{
    private static readonly ILogger _logger = Log.ForContext<EditorWindow>();
    private EditPage? _editPage;

    public EditorWindow()
    {
        _logger.Information("初始化EditorWindow");
        InitializeComponent();

        _logger.Verbose("设置TitleBar属性");
        TitleBar.Height = 36;
        TitleBar.ExtendsContentIntoTitleBar = false;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;
        _logger.Information("EditorWindow初始化完成");
    }

    private void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
        _logger.Information("EditorWindow加载");
        _logger.Verbose("创建EditPage实例");
        _editPage = new EditorPages.EditPage();
        _logger.Verbose("设置ContentFrame内容");
        ContentFrame.Content = _editPage;
        _logger.Information("EditorWindow加载完成");
    }

    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        _logger.Information("EditorWindow关闭事件触发");
        if (_editPage != null && _editPage.IsModified)
        {
            _logger.Warning("有未保存的更改，显示保存确认对话框");
            e.Cancel = true;
            
            var dialog = new ContentDialog
            {
                Title = "未保存的更改",
                Content = "您有未保存的更改，是否保存？",
                PrimaryButtonText = "保存",
                SecondaryButtonText = "不保存",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary
            };

            var result = await dialog.ShowAsync(this);
            
            if (result == ContentDialogResult.Primary)
            {
                // 保存数据
                _logger.Information("用户选择保存更改");
                try
                {
                    _logger.Verbose("保存班级数据");
                    var storageService = new ImmersingPicker.Services.Services.Storage.ClassStorageService();
                    storageService.SaveClasses(ImmersingPicker.Core.Models.Clazz.Classes);
                    _editPage.IsModified = false;
                    _logger.Information("数据保存成功");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "保存数据失败");
                }
                _logger.Verbose("关闭窗口");
                Close();
            }
            else if (result == ContentDialogResult.Secondary)
            {
                // 不保存，直接关闭
                _logger.Information("用户选择不保存更改");
                _editPage.IsModified = false;
                _logger.Verbose("关闭窗口");
                Close();
            }
            else
            {
                // 取消关闭
                _logger.Information("用户取消关闭操作");
            }
        }
        else
        {
            _logger.Verbose("无未保存更改，直接关闭");
        }
        
        base.OnClosing(e);
    }
}