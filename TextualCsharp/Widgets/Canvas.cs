using TextualCsharp.Core;
using TextualCsharp.Core.Geometry;
using TextualCsharp.Rendering;

namespace TextualCsharp.Widgets;

/// <summary>
/// Canvas de dibujo libre. Almacena celdas en una matriz interna que el widget
/// renderiza tal cual. Equivalente a <c>textual.canvas.Canvas</c>.
/// </summary>
public sealed class Canvas : Widget
{
    private ConsoleCell[,] _cells;
    private int _w;
    private int _h;

    public Canvas(int width = 0, int height = 0)
    {
        _w = Math.Max(0, width);
        _h = Math.Max(0, height);
        _cells = new ConsoleCell[Math.Max(1, _h), Math.Max(1, _w)];
        Resize(_w, _h);
    }

    public Color Background { get; set; } = Color.FromRgb(10, 10, 20);

    public void Resize(int width, int height)
    {
        _w = Math.Max(0, width);
        _h = Math.Max(0, height);
        _cells = new ConsoleCell[Math.Max(1, _h), Math.Max(1, _w)];
        Clear();
    }

    public void Clear()
    {
        var c = new ConsoleCell(' ', Color.Default, Background, StyleFlags.None);
        for (int y = 0; y < _cells.GetLength(0); y++)
            for (int x = 0; x < _cells.GetLength(1); x++)
                _cells[y, x] = c;
        Invalidate();
    }

    public void Set(int x, int y, char glyph, Color? fg = null, Color? bg = null, StyleFlags style = StyleFlags.None)
    {
        if ((uint)x >= (uint)_w || (uint)y >= (uint)_h) return;
        _cells[y, x] = new ConsoleCell(glyph, fg ?? Color.Default, bg ?? Background, style);
        Invalidate();
    }

    public void DrawText(int x, int y, string text, Color? fg = null, Color? bg = null, StyleFlags style = StyleFlags.None)
    {
        for (int i = 0; i < text.Length; i++) Set(x + i, y, text[i], fg, bg, style);
    }

    public void DrawLine(int x0, int y0, int x1, int y1, char glyph = '*', Color? fg = null)
    {
        // Bresenham
        int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = dx + dy;
        while (true)
        {
            Set(x0, y0, glyph, fg);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 >= dy) { err += dy; x0 += sx; }
            if (e2 <= dx) { err += dx; y0 += sy; }
        }
    }

    public override IEnumerable<Strip> Render(Size size)
    {
        if (size.Width <= 0 || size.Height <= 0) yield break;
        if (size.Width != _w || size.Height != _h) Resize(size.Width, size.Height);
        for (int y = 0; y < size.Height; y++)
        {
            var row = new ConsoleCell[size.Width];
            for (int x = 0; x < size.Width; x++) row[x] = _cells[y, x];
            yield return new Strip(row);
        }
    }
}
