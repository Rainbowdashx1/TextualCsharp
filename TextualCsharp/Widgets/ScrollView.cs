using TextualCsharp.Core;
using TextualCsharp.Core.Geometry;
using TextualCsharp.Input;
using TextualCsharp.Rendering;

namespace TextualCsharp.Widgets;

/// <summary>
/// Vista scrollable de líneas de texto. Equivalente a <c>textual.scroll_view.ScrollView</c>.
/// Soporta scroll vertical con flechas, PageUp/PageDown, Home/End.
/// </summary>
public sealed class ScrollView : Widget
{
    private readonly List<string> _lines = new();
    private int _offset;

    public ScrollView(IEnumerable<string>? lines = null)
    {
        if (lines is not null) _lines.AddRange(lines);
        CanFocus = true;
    }

    public IList<string> Lines => _lines;
    public int Offset => _offset;
    public Color Foreground { get; set; } = Color.White;
    public Color Background { get; set; } = Color.FromRgb(20, 20, 30);
    public Color FocusedBackground { get; set; } = Color.FromRgb(30, 30, 50);
    public bool ShowScrollbar { get; set; } = true;

    public void ScrollTo(int offset)
    {
        int max = Math.Max(0, _lines.Count - Math.Max(1, Region.Height));
        var clamped = Math.Clamp(offset, 0, max);
        if (clamped == _offset) return;
        _offset = clamped;
        Invalidate();
    }

    public void AppendLine(string line)
    {
        _lines.Add(line);
        Invalidate();
    }

    public override bool HandleKey(KeyEvent ev)
    {
        int viewH = Math.Max(1, Region.Height);
        switch (ev.Key)
        {
            case Key.Up: ScrollTo(_offset - 1); return true;
            case Key.Down: ScrollTo(_offset + 1); return true;
            case Key.PageUp: ScrollTo(_offset - viewH); return true;
            case Key.PageDown: ScrollTo(_offset + viewH); return true;
            case Key.Home: ScrollTo(0); return true;
            case Key.End: ScrollTo(int.MaxValue); return true;
        }
        return false;
    }

    public override IEnumerable<Strip> Render(Size size)
    {
        if (size.Width <= 0 || size.Height <= 0) yield break;
        var bg = HasFocus ? FocusedBackground : Background;
        int scrollbarWidth = ShowScrollbar && _lines.Count > size.Height ? 1 : 0;
        int textWidth = Math.Max(0, size.Width - scrollbarWidth);

        for (int row = 0; row < size.Height; row++)
        {
            int lineIdx = _offset + row;
            string text = lineIdx < _lines.Count ? _lines[lineIdx] : "";
            if (text.Length > textWidth) text = text[..textWidth];
            else text = text.PadRight(textWidth);

            var cells = new ConsoleCell[size.Width];
            for (int x = 0; x < textWidth; x++)
                cells[x] = new ConsoleCell(text[x], Foreground, bg, StyleFlags.None);

            if (scrollbarWidth > 0)
            {
                int max = Math.Max(1, _lines.Count - size.Height);
                int thumbRow = (int)Math.Round((double)_offset / max * (size.Height - 1));
                bool isThumb = row == thumbRow;
                cells[size.Width - 1] = new ConsoleCell(
                    isThumb ? '█' : '│',
                    Color.FromRgb(150, 150, 150), bg, StyleFlags.None);
            }
            yield return new Strip(cells);
        }
    }
}
