using TextualCsharp.Core;
using TextualCsharp.Core.Geometry;

namespace TextualCsharp.Rendering;

/// <summary>
/// Buffer virtual 2D de celdas de consola. Permite escribir glifos/strips
/// en posiciones arbitrarias y luego comparar con un buffer previo (diff).
/// </summary>
public sealed class ConsoleBuffer
{
    private ConsoleCell[] _cells;
    private int _width;
    private int _height;

    public ConsoleBuffer(int width, int height)
    {
        if (width < 0 || height < 0)
            throw new ArgumentOutOfRangeException($"width/height must be >= 0 (got {width}x{height})");
        _width = width;
        _height = height;
        _cells = new ConsoleCell[width * height];
        Clear();
    }

    public int Width => _width;
    public int Height => _height;
    public Size Size => new(_width, _height);
    public ReadOnlySpan<ConsoleCell> Cells => _cells;

    /// <summary>Cambia el tamaño del buffer. El contenido se descarta y se rellena con <see cref="ConsoleCell.Empty"/>.</summary>
    public void Resize(int width, int height)
    {
        if (width < 0 || height < 0)
            throw new ArgumentOutOfRangeException($"width/height must be >= 0 (got {width}x{height})");
        _width = width;
        _height = height;
        _cells = new ConsoleCell[width * height];
        Clear();
    }

    /// <summary>Rellena el buffer entero con la celda vacía.</summary>
    public void Clear() => Array.Fill(_cells, ConsoleCell.Empty);

    /// <summary>Rellena el buffer con una celda específica.</summary>
    public void Fill(ConsoleCell cell) => Array.Fill(_cells, cell);

    public ConsoleCell this[int x, int y]
    {
        get
        {
            if (!InBounds(x, y)) throw new ArgumentOutOfRangeException();
            return _cells[y * _width + x];
        }
        set
        {
            if (!InBounds(x, y)) throw new ArgumentOutOfRangeException();
            _cells[y * _width + x] = value;
        }
    }

    public bool InBounds(int x, int y) => (uint)x < (uint)_width && (uint)y < (uint)_height;

    /// <summary>Dibuja un strip en la fila <paramref name="y"/> empezando en la columna <paramref name="x"/>. Recorta si excede los límites.</summary>
    public void DrawStrip(int x, int y, Strip strip)
    {
        ArgumentNullException.ThrowIfNull(strip);
        if ((uint)y >= (uint)_height) return;
        int startCol = Math.Max(0, x);
        int srcStart = startCol - x;
        int endCol = Math.Min(_width, x + strip.Width);
        if (endCol <= startCol) return;
        var dst = _cells.AsSpan(y * _width + startCol, endCol - startCol);
        strip.Cells.Slice(srcStart, endCol - startCol).CopyTo(dst);
    }

    /// <summary>Dibuja texto plano en (x, y) con estilo opcional.</summary>
    public void DrawText(int x, int y, string text, Color? foreground = null, Color? background = null, StyleFlags style = StyleFlags.None)
        => DrawStrip(x, y, Strip.FromText(text, foreground, background, style));

    /// <summary>Copia el contenido de este buffer en <paramref name="destination"/> (deben tener el mismo tamaño).</summary>
    public void CopyTo(ConsoleBuffer destination)
    {
        ArgumentNullException.ThrowIfNull(destination);
        if (destination._width != _width || destination._height != _height)
            throw new ArgumentException("Buffer dimensions do not match.");
        _cells.AsSpan().CopyTo(destination._cells);
    }

    /// <summary>Devuelve una copia independiente de este buffer.</summary>
    public ConsoleBuffer Clone()
    {
        var copy = new ConsoleBuffer(_width, _height);
        _cells.AsSpan().CopyTo(copy._cells);
        return copy;
    }
}
