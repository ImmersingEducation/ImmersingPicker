using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using ImmersingPicker.Core.Models;

namespace ImmersingPicker.Controls;

public partial class StudentCard : UserControl
{
    public static readonly StyledProperty<Student> StudentProperty =
        AvaloniaProperty.Register<StudentCard, Student>(nameof(Student));

    public Student Student
    {
        get => GetValue(StudentProperty);
        set => SetValue(StudentProperty, value);
    }

    public event Action<Student>? EditRequested;
    public event Action<Student>? DeleteRequested;

    // 颜色资源
    private readonly SolidColorBrush _lightBackgroundBrush = new(Color.Parse("#F5F5F5"));
    private readonly SolidColorBrush _lightBorderBrush = new(Color.Parse("#CCCCCC"));
    private readonly SolidColorBrush _darkBackgroundBrush = new(Color.Parse("#2D2D30"));
    private readonly SolidColorBrush _darkBorderBrush = new(Color.Parse("#505050"));
    private readonly SolidColorBrush _lightHoverBackgroundBrush = new(Color.Parse("#E8E8E8"));
    private readonly SolidColorBrush _lightHoverBorderBrush = new(Color.Parse("#999999"));
    private readonly SolidColorBrush _darkHoverBackgroundBrush = new(Color.Parse("#3D3D40"));
    private readonly SolidColorBrush _darkHoverBorderBrush = new(Color.Parse("#707075"));

    private Border? _mainBorder;

    public StudentCard()
    {
        InitializeComponent();
        DataContext = this;
        
        // 获取主 Border 控件
        _mainBorder = this.FindControl<Border>("MainBorder");
        
        // 初始化主题颜色
        UpdateThemeColors();
    }

    private void UpdateThemeColors()
    {
        if (_mainBorder == null) return;
        
        var currentTheme = Application.Current?.ActualThemeVariant;
        bool isDark = currentTheme == ThemeVariant.Dark;

        _mainBorder.Background = isDark ? _darkBackgroundBrush : _lightBackgroundBrush;
        _mainBorder.BorderBrush = isDark ? _darkBorderBrush : _lightBorderBrush;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (Application.Current != null)
        {
            Application.Current.ActualThemeVariantChanged += OnThemeVariantChanged;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (Application.Current != null)
        {
            Application.Current.ActualThemeVariantChanged -= OnThemeVariantChanged;
        }
    }

    private void OnThemeVariantChanged(object? sender, EventArgs e)
    {
        // 在UI线程上更新颜色
        Avalonia.Threading.Dispatcher.UIThread.Post(UpdateThemeColors);
    }

    private void MainBorder_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        if (_mainBorder == null) return;
        
        var currentTheme = Application.Current?.ActualThemeVariant;
        bool isDark = currentTheme == ThemeVariant.Dark;

        _mainBorder.Background = isDark ? _darkHoverBackgroundBrush : _lightHoverBackgroundBrush;
        _mainBorder.BorderBrush = isDark ? _darkHoverBorderBrush : _lightHoverBorderBrush;
    }

    private void MainBorder_OnPointerExited(object? sender, PointerEventArgs e)
    {
        if (_mainBorder == null) return;
        
        UpdateThemeColors();
    }

    private void EditButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Student != null)
        {
            EditRequested?.Invoke(Student);
        }
    }

    private void DeleteButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Student != null)
        {
            DeleteRequested?.Invoke(Student);
        }
    }

    private void CardBorder_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // 双击打开编辑对话框
        if (e.ClickCount == 2 && Student != null)
        {
            EditRequested?.Invoke(Student);
        }
    }
}
