using TextualCsharp.Core.Geometry;
using TextualCsharp.Widgets;

namespace TextualCsharp.Layout;

/// <summary>Apila widgets verticalmente (eje principal = altura).</summary>
public sealed class VerticalLayout : ILayout
{
    public Alignment HorizontalAlignment { get; set; } = Alignment.Stretch;

    public MapGeometry Arrange(IReadOnlyList<Widget> widgets, Region area)
    {
        ArgumentNullException.ThrowIfNull(widgets);
        var map = new MapGeometry();
        if (widgets.Count == 0 || area.IsEmpty) return map;

        var sizes = new LayoutSize[widgets.Count];
        var autoMeasure = new int[widgets.Count];
        for (int i = 0; i < widgets.Count; i++)
        {
            sizes[i] = widgets[i].Height;
            if (sizes[i].IsAuto)
                autoMeasure[i] = widgets[i].GetPreferredSize(area.Size).Height;
        }

        var heights = LayoutResolver.Resolve(sizes, area.Height, autoMeasure);

        int y = area.Y;
        for (int i = 0; i < widgets.Count; i++)
        {
            var w = widgets[i];
            int h = heights[i];
            (int x, int width) = ResolveHorizontal(w, area, HorizontalAlignment);
            map.Set(w, new Region(x, y, width, h));
            y += h;
        }
        return map;
    }

    internal static (int x, int width) ResolveHorizontal(Widget widget, Region area, Alignment alignment)
    {
        int desired = widget.Width.Kind switch
        {
            LayoutSizeKind.Fixed => (int)widget.Width.Value,
            LayoutSizeKind.Percent => (int)Math.Round(area.Width * widget.Width.Value / 100.0),
            LayoutSizeKind.Auto => widget.GetPreferredSize(area.Size).Width,
            _ => area.Width,
        };
        desired = Math.Clamp(desired, 0, area.Width);
        int x = area.X;
        int width = desired;
        switch (alignment)
        {
            case Alignment.Start: break;
            case Alignment.End: x = area.Right - desired; break;
            case Alignment.Center: x = area.X + (area.Width - desired) / 2; break;
            case Alignment.Stretch: width = area.Width; break;
        }
        return (x, width);
    }
}
