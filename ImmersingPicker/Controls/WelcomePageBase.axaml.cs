using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using ImmersingPicker.Core.Models;

namespace ImmersingPicker.Controls;

public partial class WelcomePageBase : UserControl
{
    protected AppSettings AppSettings => AppSettings.Instance;

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<WelcomePageBase, string>(nameof(Title));

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<string> NextButtonTextProperty =
        AvaloniaProperty.Register<WelcomePageBase, string>(nameof(NextButtonTextValue), "继续");

    public string NextButtonTextValue
    {
        get => GetValue(NextButtonTextProperty);
        set => SetValue(NextButtonTextProperty, value);
    }

    public static readonly StyledProperty<bool> UseStandardLayoutProperty =
        AvaloniaProperty.Register<WelcomePageBase, bool>(nameof(UseStandardLayout), true);

    public bool UseStandardLayout
    {
        get => GetValue(UseStandardLayoutProperty);
        set => SetValue(UseStandardLayoutProperty, value);
    }

    public static readonly StyledProperty<object> PageContentProperty =
        AvaloniaProperty.Register<WelcomePageBase, object>(nameof(PageContent));

    public object PageContent
    {
        get => GetValue(PageContentProperty);
        set => SetValue(PageContentProperty, value);
    }

    public event EventHandler<RoutedEventArgs>? NextButtonClick;

    protected Button? NextButton { get; set; }
    protected TextBlock? TitleText { get; set; }
    protected ContentControl? PageContentControl { get; set; }

    public WelcomePageBase()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (UseStandardLayout)
        {
            BuildStandardLayout();
            InitializeButtonTheme();
        }
    }

    private void BuildStandardLayout()
    {
        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        var stackPanel = new StackPanel
        {
            Orientation = global::Avalonia.Layout.Orientation.Vertical,
            Spacing = 16,
            HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Center
        };
        Grid.SetRow(stackPanel, 1);
        Grid.SetColumn(stackPanel, 1);

        TitleText = new TextBlock
        {
            FontSize = 32,
            FontWeight = FontWeight.Bold,
            HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Center
        };
        TitleText.Bind(TextBlock.TextProperty, this.GetObservable(TitleProperty));
        stackPanel.Children.Add(TitleText);

        PageContentControl = new ContentControl
        {
            HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Center
        };
        PageContentControl.Bind(ContentControl.ContentProperty, this.GetObservable(PageContentProperty));
        stackPanel.Children.Add(PageContentControl);

        NextButton = new Button
        {
            Foreground = Brushes.Black,
            CornerRadius = new CornerRadius(25),
            Width = 100,
            Height = 50,
            HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Center
        };

        var buttonContent = new StackPanel
        {
            Orientation = global::Avalonia.Layout.Orientation.Horizontal,
            Spacing = 8
        };

        var fontIcon = new FontIcon
        {
            Glyph = "\uEBE7",
            FontFamily = new FontFamily("avares://ImmersingPicker/Assets/Fonts/#FluentSystemIcons")
        };
        buttonContent.Children.Add(fontIcon);

        var buttonText = new TextBlock();
        buttonText.Bind(TextBlock.TextProperty, this.GetObservable(NextButtonTextProperty));
        buttonContent.Children.Add(buttonText);

        NextButton.Content = buttonContent;
        NextButton.Click += (s, e) => NextButtonClick?.Invoke(this, e);
        stackPanel.Children.Add(NextButton);

        grid.Children.Add(stackPanel);
        RootContent.Content = grid;
    }

    protected void InitializeButtonTheme()
    {
        if (NextButton != null && !string.IsNullOrEmpty(AppSettings.AppThemeColor))
        {
            try
            {
                NextButton.Background = Brush.Parse(AppSettings.AppThemeColor);
            }
            catch
            {
            }
        }
    }

    protected void InitializeButtonTheme(Button button)
    {
        if (button != null && !string.IsNullOrEmpty(AppSettings.AppThemeColor))
        {
            try
            {
                button.Background = Brush.Parse(AppSettings.AppThemeColor);
            }
            catch
            {
            }
        }
    }

    protected virtual void LoadSettings()
    {
    }
}
