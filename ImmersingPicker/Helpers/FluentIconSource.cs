using Avalonia.Media;
using FluentAvalonia.UI.Controls;

namespace ImmersingPicker.Helpers;

public class FluentIconSource : FontIconSource
{
    public FluentIconSource()
    {
        FontFamily = new FontFamily("avares://ImmersingPicker/Assets/Fonts/#FluentSystemIcons");
    }

    public FluentIconSource(string glyph): this()
    {
        Glyph = glyph;
    }

    public FluentIconSource ProvideValue() => this;
}