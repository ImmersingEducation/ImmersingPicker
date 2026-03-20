using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using ImmersingPicker.Core;
using ImmersingPicker.Core.Models;

namespace ImmersingPicker.Controls;

public partial class HistoryItem : UserControl
{
    public static readonly StyledProperty<History> HistoryProperty = 
        AvaloniaProperty.Register<HistoryItem, History>(nameof(History));
    
    public History History
    {
        get => GetValue(HistoryProperty);
        set => SetValue(HistoryProperty, value);
    }
    
    public event EventHandler? Clicked;
    
    public HistoryItem()
    {
        InitializeComponent();
        var button = this.FindControl<Button>("Button");
        if (button != null)
        {
            button.Click += OnButtonClick;
        }
    }

    public HistoryItem(History history)
    {
        InitializeComponent();
        History = history;
        TimeText.Text = $"时间：{history.CreateTime.ToString()}";
        AmountText.Text = $"使用 {history.Selector} 抽选，共抽选 {history.Students.Count.ToString()} 人";
        var button = this.FindControl<Button>("Button");
        if (button != null)
        {
            button.Click += OnButtonClick;
        }
    }
    
    private void OnButtonClick(object? sender, RoutedEventArgs e)
    {
        Clicked?.Invoke(this, EventArgs.Empty);
    }
}