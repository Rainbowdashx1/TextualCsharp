using TextualCsharp.Core;
using TextualCsharp.Core.Geometry;
using TextualCsharp.Input;
using TextualCsharp.Rendering;

namespace TextualCsharp.Widgets;

/// <summary>
/// Botón focusable que dispara <see cref="Pressed"/> al pulsar Enter o Espacio.
/// Equivalente a <c>textual.widgets.Button</c>.
/// </summary>
public sealed class Button : Widget
{
    public Button(string label = "")
    {
        Label = label;
        CanFocus = true;
        Height = Layout.LayoutSize.Fixed(3);
    }

    public string Label { get; set; }
    public Color Foreground { get; set; } = Color.White;
    public Color Background { get; set; } = Color.FromRgb(40, 40, 90);
    public Color FocusedBackground { get; set; } = Color.FromRgb(80, 80, 160);
    public StyleFlags Style { get; set; } = StyleFlags.None;

    /// <summary>Disparado cuando el botón se activa (Enter/Espacio).</summary>
    public event Action<Button>? Pressed;

    public override Size GetPreferredSize(Size available)
        => new(Math.Min((Label?.Length ?? 0) + 4, available.Width), 3);

    public override bool HandleKey(KeyEvent ev)
    {
        if (ev.Key == Key.Enter || ev.Key == Key.Space)
        {
            Press();
            return true;
        }
        return false;
    }

    public override bool HandleMouse(MouseEvent ev)
    {
        if (ev.Type == MouseEventType.Down && ev.Button == MouseButton.Left)
        {
            Press();
            return true;
        }
        return false;
    }

    public void Press()
    {
        Pressed?.Invoke(this);
        Invalidate();
    }

    public override IEnumerable<Strip> Render(Size size)
    {
        if (size.Width <= 0 || size.Height <= 0) yield break;
        var bg = HasFocus ? FocusedBackground : Background;
        var style = Style | (HasFocus ? StyleFlags.Bold : StyleFlags.None);

        var blank = Strip.Filled(size.Width, new ConsoleCell(' ', Foreground, bg, style));

        // Fila superior (padding)
        yield return blank;

        // Fila central con el texto
        string text = $" {Label} ";
        if (text.Length > size.Width) text = text[..size.Width];
        else text = text.PadRight(size.Width);
        yield return Strip.FromText(text, Foreground, bg, style);

        // Fila inferior (padding)
        if (size.Height >= 3) yield return blank;

        // Filas extra si alguien pone un height mayor
        for (int i = 3; i < size.Height; i++) yield return blank;
    }
}
