using TextualCsharp.Core;
using TextualCsharp.Core.Geometry;
using TextualCsharp.Rendering;

namespace TextualCsharp.Widgets;

/// <summary>Orientación de la barra de scroll.</summary>
public enum ScrollOrientation { Vertical, Horizontal }

/// <summary>
/// Barra de scroll básica. Equivalente a <c>textual.scrollbar.ScrollBar</c>.
/// </summary>
public sealed class ScrollBar : Widget
{
    public ScrollBar(ScrollOrientation orientation = ScrollOrientation.Vertical)
    {
        Orientation = orientation;
        if (orientation == ScrollOrientation.Vertical)
            Width = Layout.LayoutSize.Fixed(1);
        else
            Height = Layout.LayoutSize.Fixed(1);
    }

    public ScrollOrientation Orientation { get; }
    public int ContentSize { get; set; } = 0;
    public int ViewportSize { get; set; } = 0;
    public int Position { get; set; } = 0;
    public Color Foreground { get; set; } = Color.FromRgb(150, 150, 150);
    public Color Background { get; set; } = Color.FromRgb(40, 40, 40);

    public override IEnumerable<Strip> Render(Size size)
    {
        if (size.Width <= 0 || size.Height <= 0) yield break;
        int length = Orientation == ScrollOrientation.Vertical ? size.Height : size.Width;
        int content = Math.Max(1, ContentSize);
        int viewport = Math.Max(1, Math.Min(ViewportSize, content));
        int thumb = Math.Max(1, (int)Math.Round(length * (double)viewport / content));
        int maxScroll = Math.Max(1, content - viewport);
        int thumbStart = (int)Math.Round((double)Position / maxScroll * (length - thumb));
        thumbStart = Math.Clamp(thumbStart, 0, length - thumb);

        if (Orientation == ScrollOrientation.Vertical)
        {
            for (int y = 0; y < size.Height; y++)
            {
                bool inThumb = y >= thumbStart && y < thumbStart + thumb;
                var cell = new ConsoleCell(inThumb ? '█' : '│', Foreground, Background, StyleFlags.None);
                yield return Strip.Filled(size.Width, cell);
            }
        }
        else
        {
            var cells = new ConsoleCell[size.Width];
            for (int x = 0; x < size.Width; x++)
            {
                bool inThumb = x >= thumbStart && x < thumbStart + thumb;
                cells[x] = new ConsoleCell(inThumb ? '█' : '─', Foreground, Background, StyleFlags.None);
            }
            yield return new Strip(cells);
            for (int i = 1; i < size.Height; i++) yield return new Strip(cells);
        }
    }
}
