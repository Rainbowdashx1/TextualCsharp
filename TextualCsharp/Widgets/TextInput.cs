using TextualCsharp.Core;
using TextualCsharp.Core.Geometry;
using TextualCsharp.Input;
using TextualCsharp.Rendering;

namespace TextualCsharp.Widgets;

/// <summary>
/// Campo de texto editable de una línea. Equivalente a <c>textual.widgets.Input</c>.
/// </summary>
public sealed class TextInput : Widget
{
    private string _value = "";
    private int _cursor;

    public TextInput(string value = "", string placeholder = "")
    {
        _value = value ?? "";
        _cursor = _value.Length;
        Placeholder = placeholder ?? "";
        CanFocus = true;
        Height = Layout.LayoutSize.Fixed(1);
    }

    public string Value
    {
        get => _value;
        set
        {
            var v = value ?? "";
            if (_value == v) return;
            _value = v;
            _cursor = Math.Min(_cursor, _value.Length);
            Invalidate();
            Changed?.Invoke(this, _value);
        }
    }

    public string Placeholder { get; set; }
    public Color Foreground { get; set; } = Color.White;
    public Color Background { get; set; } = Color.FromRgb(20, 20, 30);
    public Color FocusedBackground { get; set; } = Color.FromRgb(35, 35, 60);
    public Color PlaceholderForeground { get; set; } = Color.FromRgb(120, 120, 120);

    public event Action<TextInput, string>? Changed;
    public event Action<TextInput>? Submitted;

    public override bool HandleKey(KeyEvent ev)
    {
        switch (ev.Key)
        {
            case Key.Backspace:
                if (_cursor > 0)
                {
                    _value = _value.Remove(_cursor - 1, 1);
                    _cursor--;
                    Invalidate();
                    Changed?.Invoke(this, _value);
                }
                return true;
            case Key.Delete:
                if (_cursor < _value.Length)
                {
                    _value = _value.Remove(_cursor, 1);
                    Invalidate();
                    Changed?.Invoke(this, _value);
                }
                return true;
            case Key.Left:
                if (_cursor > 0) { _cursor--; Invalidate(); }
                return true;
            case Key.Right:
                if (_cursor < _value.Length) { _cursor++; Invalidate(); }
                return true;
            case Key.Home:
                if (_cursor != 0) { _cursor = 0; Invalidate(); }
                return true;
            case Key.End:
                if (_cursor != _value.Length) { _cursor = _value.Length; Invalidate(); }
                return true;
            case Key.Enter:
                Submitted?.Invoke(this);
                return true;
            case Key.Space:
                Insert(' ');
                return true;
            case Key.Character:
                if (!char.IsControl(ev.Character))
                {
                    Insert(ev.Character);
                    return true;
                }
                return false;
        }
        return false;
    }

    private void Insert(char ch)
    {
        _value = _value.Insert(_cursor, ch.ToString());
        _cursor++;
        Invalidate();
        Changed?.Invoke(this, _value);
    }

    public override IEnumerable<Strip> Render(Size size)
    {
        if (size.Width <= 0 || size.Height <= 0) yield break;
        var bg = HasFocus ? FocusedBackground : Background;
        bool showPlaceholder = _value.Length == 0 && !HasFocus && Placeholder.Length > 0;
        string display = showPlaceholder ? Placeholder : _value;
        var fg = showPlaceholder ? PlaceholderForeground : Foreground;

        // Ventana de visualización (scroll horizontal mínimo).
        int width = size.Width;
        int start = 0;
        if (_cursor >= width) start = _cursor - width + 1;
        string view = display.Length > start ? display[start..] : "";
        if (view.Length > width) view = view[..width];
        view = view.PadRight(width);

        var cells = new ConsoleCell[width];
        for (int i = 0; i < width; i++)
            cells[i] = new ConsoleCell(view[i], fg, bg, StyleFlags.None);

        if (HasFocus)
        {
            int cx = _cursor - start;
            if (cx >= 0 && cx < width)
                cells[cx] = new ConsoleCell(cells[cx].Glyph == ' ' ? ' ' : cells[cx].Glyph, fg, bg, StyleFlags.Reverse);
        }

        yield return new Strip(cells);
        for (int i = 1; i < size.Height; i++)
            yield return Strip.Filled(size.Width, new ConsoleCell(' ', fg, bg, StyleFlags.None));
    }
}
