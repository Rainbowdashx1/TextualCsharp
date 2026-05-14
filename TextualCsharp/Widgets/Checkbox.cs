using TextualCsharp.Core;
using TextualCsharp.Core.Geometry;
using TextualCsharp.Input;
using TextualCsharp.Rendering;

namespace TextualCsharp.Widgets;

/// <summary>
/// Casilla de verificación. Equivalente a <c>textual.widgets.Checkbox</c>.
/// </summary>
public sealed class Checkbox : Widget
{
    private bool _isChecked;

    public Checkbox(string label = "", bool isChecked = false)
    {
        Label = label;
        _isChecked = isChecked;
        CanFocus = true;
        Height = Layout.LayoutSize.Fixed(1);
    }

    public string Label { get; set; }
    public Color Foreground { get; set; } = Color.Default;
    public Color Background { get; set; } = Color.Default;

    public bool IsChecked
    {
        get => _isChecked;
        set { if (_isChecked == value) return; _isChecked = value; Invalidate(); Changed?.Invoke(this, value); }
    }

    public event Action<Checkbox, bool>? Changed;

    public override Size GetPreferredSize(Size available)
        => new(Math.Min((Label?.Length ?? 0) + 4, available.Width), 1);

    public override bool HandleKey(KeyEvent ev)
    {
        if (ev.Key == Key.Space || ev.Key == Key.Enter)
        {
            IsChecked = !IsChecked;
            return true;
        }
        return false;
    }

    public override bool HandleMouse(MouseEvent ev)
    {
        if (ev.Type == MouseEventType.Down && ev.Button == MouseButton.Left)
        {
            IsChecked = !IsChecked;
            return true;
        }
        return false;
    }

    public override IEnumerable<Strip> Render(Size size)
    {
        if (size.Width <= 0 || size.Height <= 0) yield break;
        string box = _isChecked ? "[x]" : "[ ]";
        string text = $"{box} {Label}";
        if (text.Length > size.Width) text = text[..size.Width];
        else text = text.PadRight(size.Width);
        var style = HasFocus ? StyleFlags.Bold : StyleFlags.None;
        yield return Strip.FromText(text, Foreground, Background, style);
        for (int i = 1; i < size.Height; i++)
            yield return Strip.Filled(size.Width, new ConsoleCell(' ', Foreground, Background, StyleFlags.None));
    }
}
