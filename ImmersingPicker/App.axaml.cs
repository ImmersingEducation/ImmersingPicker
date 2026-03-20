using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FluentAvalonia.Styling;
using FluentAvalonia.UI.Windowing;
using ImmersingPicker.Views;
using ImmersingPicker.Core.Models;
using ImmersingPicker.Services.Services.Picker;
using ImmersingPicker.Services.Services.Storage;
using ImmersingPicker.Services.Services;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Controls.Primitives;
using Avalonia.Platform.Storage;
using ImmersingPicker.Services;
using ImmersingPicker.Helpers;
using Serilog;

namespace ImmersingPicker;

public partial class App : Application
{
    private static readonly ILogger _logger = Log.ForContext<App>();

    private Timer? _autoSaveTimer;
    private AppWindow? _mainWindow;
    private FloatingWindow? _floatingWindow;
    private WelcomeWindow? _welcomeWindow;

    public override void Initialize()
    {
        if (AppSettings.Instance.EnableClassIslandLinkage)
        {
            try
            {
                _logger.Information("检测到 ClassIsland 联动功能已启用，开始初始化 IPC 服务");
                ClassIslandIPCService.Instance.Initialize();
                _logger.Information("ClassIsland IPC 服务初始化完成");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ClassIsland IPC 服务初始化失败，联动功能将不可用");
            }
        }
        else
        {
            _logger.Information("ClassIsland 联动功能未启用，跳过 IPC 服务初始化");
        }
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        _logger.Information("应用程序框架初始化完成，开始加载数据");
        // 加载班级数据
        try
        {
            _logger.Information("开始加载班级数据");
            _logger.Verbose("获取ClassStorageService实例");
            var storageService = ClassStorageService.Instance;
            storageService.LoadClasses();
            _logger.Information("班级数据加载完成");
            _logger.Verbose("班级数量: {Count}", Clazz.Classes.Count);
        }
        catch (Exception ex)
        {
            // 如果加载失败，使用默认数据
            _logger.Error(ex, "加载班级数据失败");
            _logger.Warning("使用默认班级数据");
        }

        try
        {
            _logger.Information("开始加载应用设置");
            _logger.Verbose("获取SettingsStorageService实例");
            var storageService = SettingsStorageService.Instance;
            storageService.LoadSettings();
            _logger.Information("应用设置加载完成");
            _logger.Verbose("当前主题: {Theme}", AppSettings.Instance.AppTheme);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "加载应用设置失败");
            _logger.Warning("使用默认应用设置");
        }

        // 为每个Clazz创建对应的Picker实例（如果还没有的话）
        foreach (var clazz in Clazz.Classes)
        {
            if (!clazz.Pickers.ContainsKey("FairStudentPicker"))
            {
                new FairStudentPicker(clazz);
            }
            if (!clazz.Pickers.ContainsKey("PlainStudentPicker"))
            {
                new PlainStudentPicker(clazz);
            }
        }

        // 初始化平台服务
        var platformServices = PlatformServices.Instance;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (AppSettings.Instance.IsFirstLaunch)
            {
                _logger.Information("检测到首次启动，显示欢迎窗口");
                _welcomeWindow = new WelcomeWindow();
                desktop.MainWindow = _welcomeWindow;
            }
            else
            {
                _logger.Information("非首次启动，显示主窗口");
                _mainWindow = new MainWindow();
                desktop.MainWindow = _mainWindow;

                _logger.Information("创建悬浮窗口实例");
                _floatingWindow = new FloatingWindow();
                _floatingWindow.FloatingWindowClicked += ShowMainWindow;

                _mainWindow.Closing += MainWindow_Closing;
                _mainWindow.Deactivated += MainWindow_Deactivated;
                _mainWindow.Activated += MainWindow_Activated;

                AppSettings.Instance.FloatingWindowEnabledChanged += OnFloatingWindowEnabledChanged;
                AppSettings.Instance.FloatingWindowDockPositionChanged += OnFloatingWindowSettingsChanged;
                AppSettings.Instance.FloatingWindowVerticalPositionChanged += OnFloatingWindowSettingsChanged;
                _logger.Information("悬浮窗口及事件监听初始化完成");
            }
        }

        var a = AppSettings.Instance.AppTheme;

        // 初始化主题管理器
        _logger.Information("初始化主题管理器");
        ThemeManager.Instance.Initialize();
        _logger.Information("主题管理器初始化完成");

        // 监听 ClassIsland 联动设置变更
        AppSettings.Instance.EnableClassIslandLinkageChanged += OnEnableClassIslandLinkageChanged;

        // 初始化自动保存定时器
        InitializeAutoSaveTimer();

        base.OnFrameworkInitializationCompleted();
    }

    private void InitializeAutoSaveTimer()
    {
        _logger.Information("初始化自动保存定时器");
        _logger.Verbose("设置定时器间隔为300秒");
        _autoSaveTimer = new Timer(300000); // 300秒
        _logger.Verbose("添加定时器事件处理");
        _autoSaveTimer.Elapsed += AutoSaveTimer_Elapsed;
        _logger.Verbose("设置定时器自动重置");
        _autoSaveTimer.AutoReset = true;
        _logger.Verbose("启动定时器");
        _autoSaveTimer.Start();
        _logger.Information("自动保存定时器初始化完成");
    }

    private void AutoSaveTimer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        try
        {
            _logger.Information("开始自动保存班级数据");
            var storageService = ClassStorageService.Instance;
            storageService.SaveClasses(Clazz.Classes);
            _logger.Information("班级数据自动保存完成");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "自动保存班级数据失败");
        }

        try
        {
            _logger.Information("开始自动保存应用设置");
            var storageService = SettingsStorageService.Instance;
            storageService.SaveSettings();
            _logger.Information("应用设置自动保存完成");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "自动保存应用设置失败");
        }
    }

    // 应用程序关闭时的清理操作
    public void Shutdown()
    {
        _logger.Information("开始应用程序关闭清理操作");
        
        // 停止定时器
        _logger.Information("停止自动保存定时器");
        try
        {
            _autoSaveTimer?.Stop();
            _autoSaveTimer?.Dispose();
            _logger.Verbose("定时器已停止并释放");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "停止定时器失败");
        }

        // 退出时保存数据
        try
        {
            _logger.Information("开始保存班级数据");
            var storageService = ClassStorageService.Instance;
            storageService.SaveClasses(Clazz.Classes);
            _logger.Information("班级数据保存完成");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "保存班级数据失败，可能导致数据丢失");
        }

        try
        {
            _logger.Information("开始保存应用设置");
            var storageService = SettingsStorageService.Instance;
            storageService.SaveSettings();
            _logger.Information("应用设置保存完成");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "保存应用设置失败，可能导致设置丢失");
        }
        
        _logger.Information("应用程序关闭清理操作完成");
    }

    public void ShowMainWindow(object? sender, EventArgs e)
    {
        _logger.Information("显示主窗口");
        if (_mainWindow != null)
        {
            _mainWindow.Show();
            _mainWindow.Activate();
            _mainWindow.Focus();
        }

        _floatingWindow?.HideFloatingWindow();
    }

    public void CompleteWelcomeSetup()
    {
        _logger.Information("欢迎向导完成，切换到主窗口");
        
        AppSettings.Instance.IsFirstLaunch = false;
        
        try
        {
            _logger.Information("保存设置（标记非首次启动）");
            SettingsStorageService.Instance.SaveSettings();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "保存设置失败");
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _mainWindow = new MainWindow();
            desktop.MainWindow = _mainWindow;

            _logger.Information("创建悬浮窗口实例");
            _floatingWindow = new FloatingWindow();
            _floatingWindow.FloatingWindowClicked += ShowMainWindow;

            _mainWindow.Closing += MainWindow_Closing;
            _mainWindow.Deactivated += MainWindow_Deactivated;
            _mainWindow.Activated += MainWindow_Activated;

            AppSettings.Instance.FloatingWindowEnabledChanged += OnFloatingWindowEnabledChanged;
            AppSettings.Instance.FloatingWindowDockPositionChanged += OnFloatingWindowSettingsChanged;
            AppSettings.Instance.FloatingWindowVerticalPositionChanged += OnFloatingWindowSettingsChanged;
            _logger.Information("主窗口和悬浮窗口初始化完成");

            _welcomeWindow?.Close();
            _mainWindow.Show();
            _mainWindow.Activate();
        }
    }

    /// <summary>
    /// 主窗口关闭事件处理
    /// </summary>
    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        _logger.Information("主窗口关闭事件触发");
        // 取消关闭操作
        e.Cancel = true;
        _logger.Warning("窗口关闭操作被取消，改为隐藏窗口");
        // 隐藏窗口
        _mainWindow?.Hide();
        _logger.Information("主窗口已隐藏");
        
        // 显示悬浮窗口（如果已启用）
        if (AppSettings.Instance.FloatingWindowEnabled)
        {
            _floatingWindow?.ShowFloatingWindow();
        }
        
        // 保存数据
        try
        {
            _logger.Information("开始保存班级数据");
            var storageService = ClassStorageService.Instance;
            int classCount = Clazz.Classes.Count;
            _logger.Verbose("班级数量: {ClassCount}", classCount);
            storageService.SaveClasses(Clazz.Classes);
            _logger.Information("班级数据保存完成");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "保存班级数据失败");
        }
    }

    /// <summary>
    /// 主窗口失去焦点事件处理
    /// </summary>
    private void MainWindow_Deactivated(object? sender, EventArgs e)
    {
        _logger.Information("主窗口失去焦点");
        // 显示悬浮窗口（如果已启用）
        if (AppSettings.Instance.FloatingWindowEnabled)
        {
            _floatingWindow?.ShowFloatingWindow();
        }
    }

    /// <summary>
    /// 主窗口获得焦点事件处理
    /// </summary>
    private void MainWindow_Activated(object? sender, EventArgs e)
    {
        _logger.Information("主窗口获得焦点");
        // 隐藏悬浮窗口
        _floatingWindow?.HideFloatingWindow();
    }

    /// <summary>
    /// 浮窗启用状态变更事件处理
    /// </summary>
    private void OnFloatingWindowEnabledChanged(bool enabled)
    {
        _logger.Information("浮窗启用状态变更: {Enabled}", enabled);
        if (!enabled)
        {
            // 禁用时隐藏浮窗
            _floatingWindow?.HideFloatingWindow();
        }
    }

    /// <summary>
    /// 浮窗设置变更事件处理
    /// </summary>
    private void OnFloatingWindowSettingsChanged<T>(T _)
    {
        _logger.Information("浮窗设置变更，更新位置");
        _floatingWindow?.UpdatePosition();
    }

    /// <summary>
    /// ClassIsland 联动功能启用状态变更事件处理
    /// </summary>
    private void OnEnableClassIslandLinkageChanged(bool enabled)
    {
        _logger.Information("ClassIsland 联动功能启用状态变更：{Enabled}", enabled);
        if (enabled)
        {
            try
            {
                _logger.Information("开始初始化 ClassIsland IPC 服务");
                ClassIslandIPCService.Instance.Initialize();
                _logger.Information("ClassIsland IPC 服务初始化完成");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ClassIsland IPC 服务初始化失败");
            }
        }
        else
        {
            _logger.Information("ClassIsland 联动功能已禁用");
            // TODO: 如果需要，可以在这里添加清理逻辑
        }
    }

    private async void OpenEditor(object? sender, EventArgs e)
    {
        // 打开编辑器窗口
        await MainWindowNavigationService.OpenEditorWindow();
    }

    private async void OpenSettings(object? sender, EventArgs e)
    {
        // 打开设置窗口
        await MainWindowNavigationService.OpenSettingsWindow();
    }

    public async void RestartApplication(object? sender, EventArgs e)
    {
        _logger.Information("开始应用程序重启流程");
        
        // 保存所有数据和设置
        _logger.Information("保存所有数据和设置");
        Shutdown();
        
        // 关闭所有窗口
        _logger.Information("关闭所有窗口");
        _floatingWindow?.Close();
        _welcomeWindow?.Close();
        _mainWindow?.Close();
        
        // 获取当前可执行文件路径
        string executablePath = Process.GetCurrentProcess().MainModule?.FileName ?? 
                               System.Reflection.Assembly.GetEntryAssembly()?.Location ?? 
                               "ImmersingPicker.exe";
        
        _logger.Information("准备重新启动应用程序：{ExecutablePath}", executablePath);
        
        // 启动新实例
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = executablePath,
                UseShellExecute = true,
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
            });
            _logger.Information("新应用程序实例已启动");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "启动新应用程序实例失败");
        }
        
        // 退出当前实例
        _logger.Information("退出当前应用程序实例");
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }

    public void ExitApplication(object? sender, EventArgs e)
    {
        // 执行清理操作
        Shutdown();
        
        // 退出应用程序
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }
}