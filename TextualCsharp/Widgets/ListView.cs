using TextualCsharp.Core;
using TextualCsharp.Core.Geometry;
using TextualCsharp.Input;
using TextualCsharp.Rendering;

namespace TextualCsharp.Widgets;

/// <summary>
/// Lista vertical scrollable con selección. Equivalente a
/// <c>textual.widgets.ListView</c>.
/// </summary>
public sealed class ListView : Widget
{
    private readonly List<string> _items = new();
    private int _selected;
    private int _offset;

    public ListView(IEnumerable<string>? items = null)
    {
        if (items is not null) _items.AddRange(items);
        CanFocus = true;
    }

    public IList<string> Items => _items;
    public int SelectedIndex
    {
        get => _selected;
        set
        {
            if (_items.Count == 0) { _selected = 0; return; }
            var v = Math.Clamp(value, 0, _items.Count - 1);
            if (v == _selected) return;
            _selected = v;
            EnsureVisible();
            Invalidate();
            SelectionChanged?.Invoke(this, _selected);
        }
    }

    public string? SelectedItem =>
        _selected >= 0 && _selected < _items.Count ? _items[_selected] : null;

    public Color Foreground { get; set; } = Color.White;
    public Color Background { get; set; } = Color.FromRgb(20, 20, 30);
    public Color SelectionForeground { get; set; } = Color.Black;
    public Color SelectionBackground { get; set; } = Color.FromRgb(200, 200, 80);

    public event Action<ListView, int>? SelectionChanged;
    public event Action<ListView, int>? ItemActivated;

    private int _lastClickedIndex = -1;
    private DateTime _lastClickTime;

    public override bool HandleKey(KeyEvent ev)
    {
        switch (ev.Key)
        {
            case Key.Up: SelectedIndex--; return true;
            case Key.Down: SelectedIndex++; return true;
            case Key.Home: SelectedIndex = 0; return true;
            case Key.End: SelectedIndex = _items.Count - 1; return true;
            case Key.PageUp: SelectedIndex -= Math.Max(1, Region.Height); return true;
            case Key.PageDown: SelectedIndex += Math.Max(1, Region.Height); return true;
            case Key.Enter:
                if (_items.Count > 0) ItemActivated?.Invoke(this, _selected);
                return true;
        }
        return false;
    }

    public override bool HandleMouse(MouseEvent ev)
    {
        if (ev.Type != MouseEventType.Down || ev.Button != MouseButton.Left) return false;

        int row = ev.Y - Region.Y;
        int idx = _offset + row;
        if (idx < 0 || idx >= _items.Count) return false;

        bool isDoubleClick = idx == _lastClickedIndex &&
                             (DateTime.UtcNow - _lastClickTime).TotalMilliseconds < 500;

        _lastClickedIndex = idx;
        _lastClickTime    = DateTime.UtcNow;

        if (idx == _selected && isDoubleClick)
        {
            ItemActivated?.Invoke(this, _selected);
        }
        else
        {
            SelectedIndex = idx;
        }
        return true;
    }

    private void EnsureVisible()
    {
        int h = Math.Max(1, Region.Height);
        if (_selected < _offset) _offset = _selected;
        else if (_selected >= _offset + h) _offset = _selected - h + 1;
    }

    public override IEnumerable<Strip> Render(Size size)
    {
        if (size.Width <= 0 || size.Height <= 0) yield break;
        EnsureVisible();
        for (int row = 0; row < size.Height; row++)
        {
            int idx = _offset + row;
            string text = idx < _items.Count ? _items[idx] : "";
            if (text.Length > size.Width) text = text[..size.Width];
            else text = text.PadRight(size.Width);

            bool isSelected = idx == _selected && idx < _items.Count;
            var fg = isSelected ? SelectionForeground : Foreground;
            var bg = isSelected ? SelectionBackground : Background;
            var style = isSelected ? StyleFlags.Bold : StyleFlags.None;
            yield return Strip.FromText(text, fg, bg, style);
        }
    }
}
