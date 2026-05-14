using TextualCsharp.Core;

namespace TextualCsharp.Rendering;

/// <summary>
/// Una tira (strip) horizontal de celdas con estilos. Unidad básica de composición.
/// Equivalente a <c>textual.strip.Strip</c>.
/// </summary>
public sealed class Strip
{
    private readonly ConsoleCell[] _cells;

    public Strip(ConsoleCell[] cells)
    {
        _cells = cells ?? throw new ArgumentNullException(nameof(cells));
    }

    public int Width => _cells.Length;
    public ReadOnlySpan<ConsoleCell> Cells => _cells;
    public ConsoleCell this[int index] => _cells[index];

    /// <summary>Crea un strip de ancho fijo relleno con la celda dada.</summary>
    public static Strip Filled(int width, ConsoleCell cell)
    {
        var cells = new ConsoleCell[width];
        Array.Fill(cells, cell);
        return new Strip(cells);
    }

    /// <summary>Crea un strip a partir de un texto plano con estilo uniforme.</summary>
    public static Strip FromText(string text, Color? foreground = null, Color? background = null, StyleFlags style = StyleFlags.None)
    {
        var fg = foreground ?? Color.Default;
        var bg = background ?? Color.Default;
        var cells = new ConsoleCell[text.Length];
        for (int i = 0; i < text.Length; i++)
            cells[i] = new ConsoleCell(text[i], fg, bg, style);
        return new Strip(cells);
    }
}
