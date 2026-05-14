using TextualCsharp.Core.Geometry;
using TextualCsharp.Dom;
using TextualCsharp.Input;
using TextualCsharp.Layout;
using TextualCsharp.Messaging;
using TextualCsharp.Rendering;
using TextualCsharp.Widgets;

namespace TextualCsharp.App;

/// <summary>
/// Una pantalla coordina layout + render + foco para un árbol de widgets.
/// Equivalente a <c>textual.screen.Screen</c>.
/// </summary>
public class Screen : DomNode
{
    public Screen(Widget root)
    {
        Root = root ?? throw new ArgumentNullException(nameof(root));
        Root.SetParent(this);
        Children.Add(Root);
        Focus = new FocusManager();
        Bindings = new BindingMap();
    }

    public Widget Root { get; }
    public FocusManager Focus { get; }
    public BindingMap Bindings { get; }
    public Region Region { get; protected set; } = Region.Empty;

    /// <summary>Reorganiza el árbol dentro del área dada y devuelve el mapa geométrico.</summary>
    public virtual void Arrange(Region region)
    {
        Region = region;
        Root.Region = region;
        if (Root is Container container)
            container.Arrange();
        Focus.Rebuild(Root);
    }

    /// <summary>Pinta el árbol de widgets en el buffer destino.</summary>
    public void Paint(ConsoleBuffer buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        foreach (var widget in Walk.DepthFirst(Root).OfType<Widget>())
            widget.Paint(buffer);
    }

    /// <summary>
    /// Hit test del árbol — devuelve el widget más profundo bajo (x, y).
    /// Al haber solapamiento de regiones padre/hijo, el hijo siempre gana
    /// porque DepthFirst lo visita después y sobreescribe <c>hit</c>.
    /// </summary>
    public Widget? HitTest(int x, int y)
    {
        Widget? hit = null;
        foreach (var w in Walk.DepthFirst(Root).OfType<Widget>())
            if (w.Region.Contains(x, y)) hit = w;
        return hit;
    }

    /// <summary>
    /// Igual que <see cref="HitTest"/> pero devuelve el widget <em>focusable</em>
    /// más profundo bajo (x, y). Si el widget más profundo no es focusable, sube
    /// por sus ancestros hasta encontrar uno que sí lo sea.
    /// </summary>
    public Widget? HitTestFocusable(int x, int y)
    {
        // Primero el widget más profundo que contenga el punto.
        Widget? deepest = HitTest(x, y);
        if (deepest is null) return null;
        if (deepest.CanFocus) return deepest;

        // Subir por el árbol DOM buscando un ancestro focusable.
        foreach (var ancestor in deepest.Ancestors.OfType<Widget>())
            if (ancestor.CanFocus && ancestor.Region.Contains(x, y))
                return ancestor;

        return null;
    }

    /// <summary>Procesa una pulsación de tecla: bindings + focus dispatch.</summary>
    /// <returns>El nombre de la acción ejecutada por el binding, o null si no hubo.</returns>
    public string? DispatchKey(KeyEvent ev)
    {
        ArgumentNullException.ThrowIfNull(ev);

        // El widget enfocado tiene primera oportunidad de consumir la tecla.
        if (Focus.Focused is { } focused && focused.HandleKey(ev))
            return null;

        if (Bindings.TryResolve(ev, out var binding))
            return binding.Action;

        // Navegación por defecto: Tab / Shift+Tab
        if (ev.Key == Key.Tab) { Focus.FocusNext(); return null; }
        if (ev.Key == Key.BackTab) { Focus.FocusPrevious(); return null; }

        // Despacha al widget enfocado
        Focus.Focused?.TryPost(ev);
        return null;
    }
}
