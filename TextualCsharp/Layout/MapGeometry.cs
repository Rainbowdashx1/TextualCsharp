using TextualCsharp.Core.Geometry;
using TextualCsharp.Widgets;

namespace TextualCsharp.Layout;

/// <summary>
/// Mapeo del layout calculado a regiones de pantalla por widget.
/// Equivalente a <c>textual.map_geometry.MapGeometry</c>.
/// </summary>
public sealed class MapGeometry
{
    private readonly Dictionary<Widget, Region> _map = new();

    public int Count => _map.Count;
    public Region this[Widget w] => _map[w];

    public void Set(Widget widget, Region region)
    {
        ArgumentNullException.ThrowIfNull(widget);
        _map[widget] = region;
        widget.Region = region;
    }

    public bool TryGet(Widget widget, out Region region) => _map.TryGetValue(widget, out region);

    public IEnumerable<KeyValuePair<Widget, Region>> Entries => _map;
}
