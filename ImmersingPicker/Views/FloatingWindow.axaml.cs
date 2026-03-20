using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using FluentAvalonia.UI.Windowing;
using ImmersingPicker.Core.Models;
using Serilog;

namespace ImmersingPicker.Views;

public partial class FloatingWindow : AppWindow
{
    private static readonly ILogger _logger = Log.ForContext(typeof(FloatingWindow));
    
    private bool _isAnimating;
    private PixelPoint _visiblePosition;
    private PixelPoint _hiddenPosition;

    public event EventHandler? FloatingWindowClicked;

    public FloatingWindow()
    {
        _logger.Information("悬浮窗口初始化开始");
        InitializeComponent();
        TitleBar.Height = -1;
        CalculatePositions();
        _logger.Information("悬浮窗口初始化完成");
    }

    private void CalculatePositions()
    {
        var screen = Screens.Primary;
        if (screen == null)
        {
            _logger.Warning("无法获取主显示器信息，使用默认位置");
            _visiblePosition = new PixelPoint(100, 100);
            _hiddenPosition = new PixelPoint(200, 100);
            return;
        }

        var screenBounds = screen.Bounds;
        var windowWidth = (int)Width;
        var windowHeight = (int)Height;

        // 根据设置计算竖向位置（百分比转换为像素）
        var verticalPercent = AppSettings.Instance.FloatingWindowVerticalPosition / 100.0;
        var y = (int)((screenBounds.Height - windowHeight) * verticalPercent);

        // 根据设置计算水平位置
        int x, hiddenX;
        if (AppSettings.Instance.FloatingWindowDockPosition == AppSettings.FloatingWindowDockPositionMode.Left)
        {
            // 停靠在左边缘
            x = 10;
            hiddenX = -windowWidth - 10;
        }
        else
        {
            // 停靠在右边缘
            x = screenBounds.Right - windowWidth - 10;
            hiddenX = screenBounds.Right + 10;
        }

        _visiblePosition = new PixelPoint(x, y);
        _hiddenPosition = new PixelPoint(hiddenX, y);

        _logger.Verbose("悬浮窗口位置计算完成: 可见位置=({VX},{VY}), 隐藏位置=({HX},{HY})", 
            _visiblePosition.X, _visiblePosition.Y, _hiddenPosition.X, _hiddenPosition.Y);
    }

    public void PositionToRightEdge()
    {
        CalculatePositions();
        Position = _visiblePosition;
        _logger.Verbose("悬浮窗口位置已设置: X={X}, Y={Y}", Position.X, Position.Y);
    }

    public void UpdatePosition()
    {
        CalculatePositions();
        if (IsVisible)
        {
            Position = _visiblePosition;
        }
    }

    public async void SlideIn()
    {
        if (_isAnimating)
        {
            _logger.Verbose("动画正在进行中，跳过滑入操作");
            return;
        }

        _logger.Information("悬浮窗口滑入显示");
        
        CalculatePositions();
        Position = _hiddenPosition;
        Show();

        _isAnimating = true;
        try
        {
            var startX = _hiddenPosition.X;
            var endX = _visiblePosition.X;
            var distance = endX - startX;

            for (int i = 0; i <= 10; i++)
            {
                var progress = i / 10.0;
                var easedProgress = EaseOutCubic(progress);
                var currentX = startX + (int)(distance * easedProgress);
                Position = new PixelPoint(currentX, _visiblePosition.Y);
                await Task.Delay(20);
            }
        }
        finally
        {
            _isAnimating = false;
        }

        Position = _visiblePosition;
        _logger.Information("悬浮窗口滑入完成");
    }

    public async void SlideOut()
    {
        if (_isAnimating)
        {
            _logger.Verbose("动画正在进行中，跳过滑出操作");
            return;
        }

        _logger.Information("悬浮窗口滑出隐藏");

        _isAnimating = true;
        try
        {
            CalculatePositions();
            var startX = _visiblePosition.X;
            var endX = _hiddenPosition.X;
            var distance = endX - startX;

            for (int i = 0; i <= 10; i++)
            {
                var progress = i / 10.0;
                var easedProgress = EaseInCubic(progress);
                var currentX = startX + (int)(distance * easedProgress);
                Position = new PixelPoint(currentX, _visiblePosition.Y);
                await Task.Delay(20);
            }
        }
        finally
        {
            _isAnimating = false;
        }

        Hide();
        _logger.Information("悬浮窗口滑出完成");
    }

    private static double EaseOutCubic(double t)
    {
        return 1 - Math.Pow(1 - t, 3);
    }

    private static double EaseInCubic(double t)
    {
        return t * t * t;
    }

    public void ShowFloatingWindow()
    {
        SlideIn();
    }

    public void HideFloatingWindow()
    {
        SlideOut();
    }

    private void FloatingWindow_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _logger.Information("悬浮窗口被点击");
        FloatingWindowClicked?.Invoke(this, EventArgs.Empty);
    }
}
