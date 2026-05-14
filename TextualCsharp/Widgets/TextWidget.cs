using TextualCsharp.Core;
using TextualCsharp.Core.Geometry;
using TextualCsharp.Rendering;

namespace TextualCsharp.Widgets;

/// <summary>Widget de texto plano de una línea.</summary>
public sealed class TextWidget : Widget
{
    public TextWidget(string text = "")
    {
        Text = text;
    }

    private string _text = string.Empty;
    public string Text
    {
        get => _text;
        set { if (_text == value) return; _text = value ?? string.Empty; Invalidate(); }
    }

    public Color Foreground { get; set; } = Color.Default;
    public Color Background { get; set; } = Color.Default;
    public StyleFlags Style { get; set; } = StyleFlags.None;

    public override Size GetPreferredSize(Size available) =>
        new(Math.Min(_text.Length, available.Width), 1);

    public override IEnumerable<Strip> Render(Size size)
    {
        if (size.Width <= 0 || size.Height <= 0) yield break;
        string line = _text.Length > size.Width ? _text[..size.Width] : _text.PadRight(size.Width);
        yield return Strip.FromText(line, Foreground, Background, Style);
        var blank = Strip.Filled(size.Width, new ConsoleCell(' ', Foreground, Background, Style));
        for (int i = 1; i < size.Height; i++) yield return blank;
    }
}
