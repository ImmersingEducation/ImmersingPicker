namespace ImmersingPicker.Core.Models;

public class AppSettings
{
    public enum ThemeEnums
    {
        System, Light, Dark
    }

    public enum SeatGridRowArrangementMode { T2B, B2T }
    public enum SeatGridColumnArrangementMode { L2R, R2L }
    public enum FloatingWindowDockPositionMode { Left, Right }

    public static AppSettings Instance { get; set;  } = new AppSettings();

    private bool _isFirstLaunch = true;

    public AppSettings()
    {
        AppTheme = ThemeEnums.System;
        AppThemeColor = "#ffffb900";

        WeightCalculationParam1 = 0.49;
        WeightCalculationParam2 = 1.23;
        WeightCalculationParam3 = 3;
        WeightCalculationParam4 = 1.14;
        WeightCalculationParam5 = 0.98;
        WeightCalculationParam6 = 5.14;
        WeightCalculationParam7 = 2;
        WeightCalculationParam8 = 6;
        WeightCalculationParam9 = 4;
        WeightCalculationParam10 = 9;

        HomeAnimationPlayAmount = 10;
        HomeAnimationPlayDelay = 100;
        SeatGridRowArrangement = SeatGridRowArrangementMode.T2B;
        SeatGridColumnArrangement = SeatGridColumnArrangementMode.L2R;

        OpenPassword = false;
        EnableUsbSecurityCheck = false;
        UsbSecurityDriveInfo = null;

        FloatingWindowEnabled = true;
        FloatingWindowDockPosition = FloatingWindowDockPositionMode.Right;
        FloatingWindowVerticalPosition = 50;

        EnableClassIslandLinkage = false;
        EnableDisablingAfterClasses = false;
    }

    public event Action<bool> LaunchOnSystemStartChanged;
    public event Action<bool> OpenUrlAndIpcChanged;
    public event Action<ThemeEnums> AppThemeChanged;
    public event Action<string> AppThemeColorChanged;

    public event Action<double> WeightCalculationParam1Changed;
    public event Action<double> WeightCalculationParam2Changed;
    public event Action<int> WeightCalculationParam3Changed;
    public event Action<double> WeightCalculationParam4Changed;
    public event Action<double> WeightCalculationParam5Changed;
    public event Action<double> WeightCalculationParam6Changed;
    public event Action<int> WeightCalculationParam7Changed;
    public event Action<int> WeightCalculationParam8Changed;
    public event Action<int> WeightCalculationParam9Changed;
    public event Action<int> WeightCalculationParam10Changed;

    public event Action<int> HomeAnimationPlayAmountChanged;
    public event Action<int> HomeAnimationPlayDelayChanged;
    public event Action<SeatGridRowArrangementMode> SeatGridRowArrangementChanged;
    public event Action<SeatGridColumnArrangementMode> SeatGridColumnArrangementChanged;

    public event Action<bool> OpenPasswordChanged;
    public event Action<bool> EnableUsbSecurityCheckChanged;
    public event Action<UsbDriveInfo?> UsbSecurityDriveInfoChanged;

    public event Action<bool> FloatingWindowEnabledChanged;
    public event Action<FloatingWindowDockPositionMode> FloatingWindowDockPositionChanged;
    public event Action<int> FloatingWindowVerticalPositionChanged;

    public event Action<bool> EnableClassIslandLinkageChanged;
    public event Action<bool> EnableDisablingAfterClassesChanged;
    public event Action<bool> IsFirstLaunchChanged;

    public event Action AnyChanged;

    private bool _launchOnSystemStart;
    private bool _openUrlAndIpc;
    private ThemeEnums _appTheme;
    private string _appThemeColor;

    private double _weightCalculationParam1;
    private double _weightCalculationParam2;
    private int _weightCalculationParam3;
    private double _weightCalculationParam4;
    private double _weightCalculationParam5;
    private double _weightCalculationParam6;
    private int _weightCalculationParam7;
    private int _weightCalculationParam8;
    private int _weightCalculationParam9;
    private int _weightCalculationParam10;

    private int _homeAnimationPlayAmount;
    private int _homeAnimationPlayDelay;
    private SeatGridRowArrangementMode _seatGridRowArrangement;
    private SeatGridColumnArrangementMode _seatGridColumnArrangement;

    private bool _openPassword;
    private string _passwordHash = string.Empty;
    private bool _enableUsbSecurityCheck;
    private UsbDriveInfo? _usbSecurityDriveInfo;

    private bool _floatingWindowEnabled;
    private FloatingWindowDockPositionMode _floatingWindowDockPosition;
    private int _floatingWindowVerticalPosition;

    private bool _enableClassIslandLinkage;
    private bool _enableDisablingAfterClasses;

    public bool LaunchOnSystemStart
    {
        get => _launchOnSystemStart;
        set
        {
            _launchOnSystemStart = value;
            LaunchOnSystemStartChanged?.Invoke(value);
            AnyChanged?.Invoke();
        }
    }

    public bool OpenUrlAndIpc
    {
        get => _openUrlAndIpc;
        set
        {
            _openUrlAndIpc = value;
            OpenUrlAndIpcChanged?.Invoke(value);
            AnyChanged?.Invoke();
        }
    }

    public ThemeEnums AppTheme
    {
        get => _appTheme;
        set
        {
            _appTheme = value;
            AppThemeChanged?.Invoke(value);
            AnyChanged?.Invoke();
        }
    }

    public string AppThemeColor
    {
        get => _appThemeColor;
        set
        {
            _appThemeColor = value;
            AppThemeColorChanged?.Invoke(value);
            AnyChanged?.Invoke();
        }
    }

    public double WeightCalculationParam1
    {
        get => _weightCalculationParam1;
        set
        {
            _weightCalculationParam1 = value;
            WeightCalculationParam1Changed?.Invoke(value);
            AnyChanged?.Invoke();
        }
    }

    public double WeightCalculationParam2
    {
        get => _weightCalculationParam2;
        set
        {
            _weightCalculationParam2 = value;
            WeightCalculationParam2Changed?.Invoke(value);
            AnyChanged?.Invoke();
        }
    }

    public int WeightCalculationParam3
    {
        get => _weightCalculationParam3;
        set
        {
            _weightCalculationParam3 = value;
            WeightCalculationParam3Changed?.Invoke(value);
            AnyChanged?.Invoke();
        }
    }

    public double WeightCalculationParam4
    {
        get => _weightCalculationParam4;
        set
        {
            _weightCalculationParam4 = value;
            WeightCalculationParam4Changed?.Invoke(value);
            AnyChanged?.Invoke();
        }
    }

    public double WeightCalculationParam5
    {
        get => _weightCalculationParam5;
        set
        {
            _weightCalculationParam5 = value;
            WeightCalculationParam5Changed?.Invoke(value);
            AnyChanged?.Invoke();
        }
    }

    public double WeightCalculationParam6
    {
        get => _weightCalculationParam6;
        set
        {
            _weightCalculationParam6 = value;
            WeightCalculationParam6Changed?.Invoke(value);
            AnyChanged?.Invoke();
        }
    }

    public int WeightCalculationParam7
    {
        get => _weightCalculationParam7;
        set
        {
            _weightCalculationParam7 = value;
            WeightCalculationParam7Changed?.Invoke(value);
            AnyChanged?.Invoke();
        }
    }

    public int WeightCalculationParam8
    {
        get => _weightCalculationParam8;
        set
        {
            _weightCalculationParam8 = value;
            WeightCalculationParam8Changed?.Invoke(value);
            AnyChanged?.Invoke();
        }
    }

    public int WeightCalculationParam9
    {
        get => _weightCalculationParam9;
        set
        {
            _weightCalculationParam9 = value;
            WeightCalculationParam9Changed?.Invoke(value);
            AnyChanged?.Invoke();
        }
    }

    public int WeightCalculationParam10
    {
        get => _weightCalculationParam10;
        set
        {
            _weightCalculationParam10 = value;
            WeightCalculationParam10Changed?.Invoke(value);
            AnyChanged?.Invoke();
        }
    }

    public int HomeAnimationPlayAmount
    {
        get => _homeAnimationPlayAmount;
        set
        {
            _homeAnimationPlayAmount = value;
            HomeAnimationPlayAmountChanged?.Invoke(value);
            AnyChanged?.Invoke();
        }
    }

    public int HomeAnimationPlayDelay
    {
        get => _homeAnimationPlayDelay;
        set
        {
            _homeAnimationPlayDelay = value;
            HomeAnimationPlayDelayChanged?.Invoke(value);
            AnyChanged?.Invoke();
        }
    }

    public SeatGridRowArrangementMode SeatGridRowArrangement
    {
        get => _seatGridRowArrangement;
        set
        {
            _seatGridRowArrangement = value;
            SeatGridRowArrangementChanged?.Invoke(value);
            AnyChanged?.Invoke();
        }
    }

    public SeatGridColumnArrangementMode SeatGridColumnArrangement
    {
        get => _seatGridColumnArrangement;
        set
        {
            _seatGridColumnArrangement = value;
            SeatGridColumnArrangementChanged?.Invoke(value);
            AnyChanged?.Invoke();
        }
    }

    public bool OpenPassword
    {
        get => _openPassword;
        set
        {
            _openPassword = value;
            OpenPasswordChanged?.Invoke(value);
            AnyChanged?.Invoke();
        }
    }

    public string PasswordHash
    {
        get => _passwordHash;
        set
        {
            _passwordHash = value;
            AnyChanged?.Invoke();
        }
    }

    public bool EnableUsbSecurityCheck
    {
        get => _enableUsbSecurityCheck;
        set
        {
            _enableUsbSecurityCheck = value;
            EnableUsbSecurityCheckChanged?.Invoke(value);
            AnyChanged?.Invoke();
        }
    }

    public UsbDriveInfo? UsbSecurityDriveInfo
    {
        get => _usbSecurityDriveInfo;
        set
        {
            _usbSecurityDriveInfo = value;
            UsbSecurityDriveInfoChanged?.Invoke(value);
            AnyChanged?.Invoke();
        }
    }

    public bool FloatingWindowEnabled
    {
        get => _floatingWindowEnabled;
        set
        {
            _floatingWindowEnabled = value;
            FloatingWindowEnabledChanged?.Invoke(value);
            AnyChanged?.Invoke();
        }
    }

    public FloatingWindowDockPositionMode FloatingWindowDockPosition
    {
        get => _floatingWindowDockPosition;
        set
        {
            _floatingWindowDockPosition = value;
            FloatingWindowDockPositionChanged?.Invoke(value);
            AnyChanged?.Invoke();
        }
    }

    public int FloatingWindowVerticalPosition
    {
        get => _floatingWindowVerticalPosition;
        set
        {
            _floatingWindowVerticalPosition = value;
            FloatingWindowVerticalPositionChanged?.Invoke(value);
            AnyChanged?.Invoke();
        }
    }

    public bool EnableClassIslandLinkage
    {
        get => _enableClassIslandLinkage;
        set
        {
            _enableClassIslandLinkage = value;
            EnableClassIslandLinkageChanged?.Invoke(value);
            AnyChanged?.Invoke();
        }
    }

    public bool EnableDisablingAfterClasses
    {
        get => _enableDisablingAfterClasses && _enableClassIslandLinkage;
        set
        {
            _enableDisablingAfterClasses = value;
            EnableDisablingAfterClassesChanged?.Invoke(value);
            AnyChanged?.Invoke();
        }
    }

    public bool IsFirstLaunch
    {
        get => _isFirstLaunch;
        set
        {
            _isFirstLaunch = value;
            IsFirstLaunchChanged?.Invoke(value);
            AnyChanged?.Invoke();
        }
    }
}