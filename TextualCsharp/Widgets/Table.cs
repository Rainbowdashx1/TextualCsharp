using TextualCsharp.Core;
using TextualCsharp.Core.Geometry;
using TextualCsharp.Input;
using TextualCsharp.Rendering;

namespace TextualCsharp.Widgets;

/// <summary>
/// Tabla simple con headers y scroll vertical. Equivalente a
/// <c>textual.widgets.DataTable</c>.
/// </summary>
public sealed class Table : Widget
{
    private readonly List<string> _headers = new();
    private readonly List<string[]> _rows = new();
    private int _selected;
    private int _offset;

    public Table()
    {
        CanFocus = true;
    }

    public IList<string> Headers => _headers;
    public IReadOnlyList<string[]> Rows => _rows;
    public int SelectedRow
    {
        get => _selected;
        set
        {
            if (_rows.Count == 0) { _selected = 0; return; }
            var v = Math.Clamp(value, 0, _rows.Count - 1);
            if (v == _selected) return;
            _selected = v;
            Invalidate();
        }
    }

    public Color HeaderForeground { get; set; } = Color.Black;
    public Color HeaderBackground { get; set; } = Color.FromRgb(200, 200, 200);
    public Color Foreground { get; set; } = Color.White;
    public Color Background { get; set; } = Color.FromRgb(20, 20, 30);
    public Color SelectionBackground { get; set; } = Color.FromRgb(60, 100, 160);

    public void AddRow(params string[] values)
    {
        _rows.Add(values);
        Invalidate();
    }

    public void Clear() { _rows.Clear(); _selected = 0; _offset = 0; Invalidate(); }

    public override bool HandleKey(KeyEvent ev)
    {
        switch (ev.Key)
        {
            case Key.Up: SelectedRow--; return true;
            case Key.Down: SelectedRow++; return true;
            case Key.Home: SelectedRow = 0; return true;
            case Key.End: SelectedRow = _rows.Count - 1; return true;
        }
        return false;
    }

    public override bool HandleMouse(MouseEvent ev)
    {
        if (ev.Type != MouseEventType.Down || ev.Button != MouseButton.Left) return false;

        // La primera fila es el header (no seleccionable); las de datos empiezan en Y+1
        int row = ev.Y - Region.Y - 1;
        if (row < 0) return false;
        int idx = _offset + row;
        if (idx < 0 || idx >= _rows.Count) return false;

        SelectedRow = idx;
        return true;
    }

    private int[] ComputeColumnWidths(int totalWidth)
    {
        int cols = Math.Max(_headers.Count, _rows.Count > 0 ? _rows.Max(r => r.Length) : 0);
        if (cols == 0) return Array.Empty<int>();
        var widths = new int[cols];
        for (int c = 0; c < cols; c++)
        {
            int w = c < _headers.Count ? _headers[c].Length : 0;
            foreach (var row in _rows)
                if (c < row.Length) w = Math.Max(w, row[c].Length);
            widths[c] = w + 1;
        }
        int sum = widths.Sum();
        if (sum <= 0) return widths;
        // Escalar a totalWidth si es necesario.
        if (sum > totalWidth)
        {
            for (int c = 0; c < cols; c++)
                widths[c] = Math.Max(1, (int)Math.Floor((double)widths[c] / sum * totalWidth));
        }
        return widths;
    }

    private static string Pad(string s, int width)
    {
        if (s.Length > width) return s[..width];
        return s.PadRight(width);
    }

    public override IEnumerable<Strip> Render(Size size)
    {
        if (size.Width <= 0 || size.Height <= 0) yield break;
        var widths = ComputeColumnWidths(size.Width);
        int viewH = size.Height - 1; // 1 fila para header
        if (_selected < _offset) _offset = _selected;
        else if (_selected >= _offset + viewH) _offset = _selected - viewH + 1;

        // Header
        {
            var cells = new ConsoleCell[size.Width];
            int x = 0;
            for (int c = 0; c < widths.Length && x < size.Width; c++)
            {
                string h = c < _headers.Count ? _headers[c] : "";
                string txt = Pad(h, widths[c]);
                for (int i = 0; i < txt.Length && x < size.Width; i++, x++)
                    cells[x] = new ConsoleCell(txt[i], HeaderForeground, HeaderBackground, StyleFlags.Bold);
            }
            while (x < size.Width)
                cells[x++] = new ConsoleCell(' ', HeaderForeground, HeaderBackground, StyleFlags.None);
            yield return new Strip(cells);
        }

        // Filas
        for (int row = 0; row < viewH; row++)
        {
            int idx = _offset + row;
            bool isSelected = idx == _selected && idx < _rows.Count;
            var bg = isSelected ? SelectionBackground : Background;
            var fg = Foreground;
            var cells = new ConsoleCell[size.Width];
            int x = 0;
            if (idx < _rows.Count)
            {
                var rdata = _rows[idx];
                for (int c = 0; c < widths.Length && x < size.Width; c++)
                {
                    string v = c < rdata.Length ? rdata[c] : "";
                    string txt = Pad(v, widths[c]);
                    for (int i = 0; i < txt.Length && x < size.Width; i++, x++)
                        cells[x] = new ConsoleCell(txt[i], fg, bg, isSelected ? StyleFlags.Bold : StyleFlags.None);
                }
            }
            while (x < size.Width)
                cells[x++] = new ConsoleCell(' ', fg, bg, StyleFlags.None);
            yield return new Strip(cells);
        }
    }
}
