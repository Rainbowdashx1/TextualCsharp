using TextualCsharp.Core.Geometry;
using TextualCsharp.Widgets;

namespace TextualCsharp.Layout;

/// <summary>
/// Layout en cuadrícula con columnas y filas declaradas como <see cref="LayoutSize"/>.
/// Los widgets se colocan en orden por filas (left-to-right, top-to-bottom).
/// </summary>
public sealed class GridLayout : ILayout
{
    public GridLayout(IReadOnlyList<LayoutSize> columns, IReadOnlyList<LayoutSize> rows)
    {
        Columns = columns ?? throw new ArgumentNullException(nameof(columns));
        Rows = rows ?? throw new ArgumentNullException(nameof(rows));
    }

    public IReadOnlyList<LayoutSize> Columns { get; }
    public IReadOnlyList<LayoutSize> Rows { get; }

    public MapGeometry Arrange(IReadOnlyList<Widget> widgets, Region area)
    {
        ArgumentNullException.ThrowIfNull(widgets);
        var map = new MapGeometry();
        if (Columns.Count == 0 || Rows.Count == 0 || area.IsEmpty) return map;

        var colSizes = new LayoutSize[Columns.Count];
        for (int i = 0; i < Columns.Count; i++) colSizes[i] = Columns[i];
        var rowSizes = new LayoutSize[Rows.Count];
        for (int i = 0; i < Rows.Count; i++) rowSizes[i] = Rows[i];

        var widths = LayoutResolver.Resolve(colSizes, area.Width);
        var heights = LayoutResolver.Resolve(rowSizes, area.Height);

        // Prefix sums for X/Y starts
        var xs = new int[Columns.Count];
        xs[0] = area.X;
        for (int i = 1; i < xs.Length; i++) xs[i] = xs[i - 1] + widths[i - 1];
        var ys = new int[Rows.Count];
        ys[0] = area.Y;
        for (int i = 1; i < ys.Length; i++) ys[i] = ys[i - 1] + heights[i - 1];

        int idx = 0;
        for (int r = 0; r < Rows.Count && idx < widgets.Count; r++)
        {
            for (int c = 0; c < Columns.Count && idx < widgets.Count; c++, idx++)
            {
                var w = widgets[idx];
                map.Set(w, new Region(xs[c], ys[r], widths[c], heights[r]));
            }
        }
        return map;
    }
}
