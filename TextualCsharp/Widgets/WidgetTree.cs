using TextualCsharp.Core;
using TextualCsharp.Core.Geometry;
using TextualCsharp.Dom;

namespace TextualCsharp.Widgets;

/// <summary>
/// Helper para administrar un árbol de widgets: tracking del nodo raíz, expone
/// recorridos, e integra el ciclo de vida con el sistema de mensajes.
/// </summary>
public sealed class WidgetTree
{
    public WidgetTree(Widget root)
    {
        Root = root ?? throw new ArgumentNullException(nameof(root));
    }

    public Widget Root { get; }

    public IEnumerable<Widget> DepthFirst() => Walk.DepthFirst(Root).OfType<Widget>();
    public IEnumerable<Widget> BreadthFirst() => Walk.BreadthFirst(Root).OfType<Widget>();

    public Task MountAsync() => Root.MountAsync();
    public Task UnmountAsync() => Root.UnmountAsync();

    /// <summary>Devuelve el primer widget cuyo <see cref="Widget.Region"/> contiene el punto.</summary>
    public Widget? HitTest(int x, int y)
    {
        Widget? hit = null;
        foreach (var w in DepthFirst())
        {
            if (w.Region.Contains(x, y))
                hit = w; // último (más profundo) gana
        }
        return hit;
    }
}
