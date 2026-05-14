using TextualCsharp.Core;
using TextualCsharp.Core.Geometry;
using TextualCsharp.Rendering;

namespace TextualCsharp.Widgets;

/// <summary>Widget vacío que rellena con un glifo (por defecto espacio). Útil como separador flexible.</summary>
public sealed class SpacerWidget : Widget
{
    public char Glyph { get; set; } = ' ';
    public Color Foreground { get; set; } = Color.Default;
    public Color Background { get; set; } = Color.Default;

    public override IEnumerable<Strip> Render(Size size)
    {
        if (size.Width <= 0 || size.Height <= 0) yield break;
        var cell = new ConsoleCell(Glyph, Foreground, Background, StyleFlags.None);
        var row = Strip.Filled(size.Width, cell);
        for (int y = 0; y < size.Height; y++) yield return row;
    }
}
