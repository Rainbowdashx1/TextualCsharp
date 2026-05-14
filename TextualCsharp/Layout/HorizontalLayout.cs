using TextualCsharp.Core.Geometry;
using TextualCsharp.Widgets;

namespace TextualCsharp.Layout;

/// <summary>Coloca widgets horizontalmente (eje principal = ancho).</summary>
public sealed class HorizontalLayout : ILayout
{
    public Alignment VerticalAlignment { get; set; } = Alignment.Stretch;

    public MapGeometry Arrange(IReadOnlyList<Widget> widgets, Region area)
    {
        ArgumentNullException.ThrowIfNull(widgets);
        var map = new MapGeometry();
        if (widgets.Count == 0 || area.IsEmpty) return map;

        var sizes = new LayoutSize[widgets.Count];
        var autoMeasure = new int[widgets.Count];
        for (int i = 0; i < widgets.Count; i++)
        {
            sizes[i] = widgets[i].Width;
            if (sizes[i].IsAuto)
                autoMeasure[i] = widgets[i].GetPreferredSize(area.Size).Width;
        }

        var widths = LayoutResolver.Resolve(sizes, area.Width, autoMeasure);

        int x = area.X;
        for (int i = 0; i < widgets.Count; i++)
        {
            var w = widgets[i];
            int wd = widths[i];
            (int y, int height) = ResolveVertical(w, area, VerticalAlignment);
            map.Set(w, new Region(x, y, wd, height));
            x += wd;
        }
        return map;
    }

    internal static (int y, int height) ResolveVertical(Widget widget, Region area, Alignment alignment)
    {
        int desired = widget.Height.Kind switch
        {
            LayoutSizeKind.Fixed => (int)widget.Height.Value,
            LayoutSizeKind.Percent => (int)Math.Round(area.Height * widget.Height.Value / 100.0),
            LayoutSizeKind.Auto => widget.GetPreferredSize(area.Size).Height,
            _ => area.Height,
        };
        desired = Math.Clamp(desired, 0, area.Height);
        int y = area.Y;
        int height = desired;
        switch (alignment)
        {
            case Alignment.Start: break;
            case Alignment.End: y = area.Bottom - desired; break;
            case Alignment.Center: y = area.Y + (area.Height - desired) / 2; break;
            case Alignment.Stretch: height = area.Height; break;
        }
        return (y, height);
    }
}
