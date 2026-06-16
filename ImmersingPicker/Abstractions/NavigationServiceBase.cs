using System;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Windowing;
using Serilog;

namespace ImmersingPicker.Abstractions;

public abstract class NavigationServiceBase
{
    protected readonly ILogger _logger = Log.ForContext(typeof(NavigationServiceBase));
    private Frame? _mainContentFrame;
    protected AppWindow? _mainWindow;

    public void Initialize(Frame mainFrame, AppWindow mainWindow)
    {
        _logger.Information("初始化主窗口导航服务");
        _logger.Verbose("设置主内容框架");
        _mainContentFrame = mainFrame;
        _mainWindow = mainWindow;
        _logger.Information("主窗口导航服务初始化完成");
    }

    public void NavigateTo(Type viewType)
    {
        _logger.Information("导航到视图：{ViewType}", viewType.Name);
        if (_mainContentFrame != null)
        {
            try
            {
                // _logger.Verbose("创建视图实例");
                // if (Activator.CreateInstance(viewType) is UserControl view)
                // {
                //     _logger.Verbose("设置视图为框架内容");
                //     _mainContentFrame.Content = view;
                //     _logger.Information("导航成功");
                // }
                // else
                // {
                //     _logger.Warning("无法创建视图实例：{ViewType}", viewType.Name);
                // }
                _mainContentFrame.Navigate(viewType);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "导航失败");
            }
        }
        else
        {
            _logger.Error("主内容框架未初始化");
        }
    }
}
