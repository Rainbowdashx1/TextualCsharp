using TextualCsharp.Core;
using TextualCsharp.Core.Geometry;
using TextualCsharp.Input;
using TextualCsharp.Rendering;

namespace TextualCsharp.Widgets;

/// <summary>
/// Botón de radio asociado a un <see cref="RadioGroup"/>. Sólo uno seleccionado
/// por grupo a la vez. Equivalente a <c>textual.widgets.RadioButton</c>.
/// </summary>
public sealed class RadioButton : Widget
{
    private bool _isSelected;

    public RadioButton(string label = "", RadioGroup? group = null)
    {
        Label = label;
        Group = group;
        CanFocus = true;
        Height = Layout.LayoutSize.Fixed(1);
        group?.Register(this);
    }

    public string Label { get; set; }
    public RadioGroup? Group { get; }
    public Color Foreground { get; set; } = Color.Default;
    public Color Background { get; set; } = Color.Default;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            Invalidate();
            if (value) Group?.NotifySelected(this);
            Changed?.Invoke(this, value);
        }
    }

    public event Action<RadioButton, bool>? Changed;

    public override Size GetPreferredSize(Size available)
        => new(Math.Min((Label?.Length ?? 0) + 4, available.Width), 1);

    public override bool HandleKey(KeyEvent ev)
    {
        if (ev.Key == Key.Space || ev.Key == Key.Enter)
        {
            IsSelected = true;
            return true;
        }
        return false;
    }

    public override bool HandleMouse(MouseEvent ev)
    {
        if (ev.Type == MouseEventType.Down && ev.Button == MouseButton.Left)
        {
            IsSelected = true;
            return true;
        }
        return false;
    }

    public override IEnumerable<Strip> Render(Size size)
    {
        if (size.Width <= 0 || size.Height <= 0) yield break;
        string mark = _isSelected ? "(*)" : "( )";
        string text = $"{mark} {Label}";
        if (text.Length > size.Width) text = text[..size.Width];
        else text = text.PadRight(size.Width);
        var style = HasFocus ? StyleFlags.Bold : StyleFlags.None;
        yield return Strip.FromText(text, Foreground, Background, style);
        for (int i = 1; i < size.Height; i++)
            yield return Strip.Filled(size.Width, new ConsoleCell(' ', Foreground, Background, StyleFlags.None));
    }
}

/// <summary>Grupo de radios mutuamente excluyentes.</summary>
public sealed class RadioGroup
{
    private readonly List<RadioButton> _buttons = new();

    public RadioButton? Selected => _buttons.FirstOrDefault(b => b.IsSelected);

    internal void Register(RadioButton button) => _buttons.Add(button);

    internal void NotifySelected(RadioButton selected)
    {
        foreach (var b in _buttons)
            if (b != selected && b.IsSelected) b.IsSelected = false;
    }
}
