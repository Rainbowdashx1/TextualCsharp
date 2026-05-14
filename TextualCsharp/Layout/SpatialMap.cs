using TextualCsharp.Core.Geometry;
using TextualCsharp.Widgets;

namespace TextualCsharp.Layout;

/// <summary>
/// Mapa espacial simple para hit-testing por coordenadas. Equivalente a
/// <c>textual._spatial_map.SpatialMap</c>. Implementación lineal (O(n)) suficiente
/// para TUIs con decenas/cientos de widgets.
/// </summary>
public sealed class SpatialMap
{
    private readonly List<(Widget Widget, Region Region)> _entries = new();

    public void Add(Widget widget, Region region)
    {
        ArgumentNullException.ThrowIfNull(widget);
        _entries.Add((widget, region));
    }

    public void Clear() => _entries.Clear();

    /// <summary>Devuelve el widget más profundo (último añadido) cuya región contiene el punto.</summary>
    public Widget? HitTest(int x, int y)
    {
        for (int i = _entries.Count - 1; i >= 0; i--)
            if (_entries[i].Region.Contains(x, y))
                return _entries[i].Widget;
        return null;
    }

    public static SpatialMap FromGeometry(MapGeometry geometry)
    {
        var map = new SpatialMap();
        foreach (var kv in geometry.Entries) map.Add(kv.Key, kv.Value);
        return map;
    }
}
