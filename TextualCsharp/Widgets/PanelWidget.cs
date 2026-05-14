using TextualCsharp.Core;
using TextualCsharp.Core.Geometry;
using TextualCsharp.Rendering;

namespace TextualCsharp.Widgets;

/// <summary>
/// Contenedor con borde que pinta sus hijos en su área interior. Combinable con
/// layouts: si tiene un layout asignado, lo aplica a sus hijos en el rectángulo interior.
/// </summary>
public sealed class PanelWidget : Widget
{
    public BoxDrawing.BorderKind Border { get; set; } = BoxDrawing.BorderKind.Light;
    public Color BorderForeground { get; set; } = Color.Default;
    public Color Background { get; set; } = Color.Default;
    public string? Title { get; set; }
    public Layout.ILayout? Layout { get; set; }

    /// <summary>Aplica el layout a sus hijos en el rectángulo interior.</summary>
    public void Arrange()
    {
        if (Layout is null) return;
        var inner = new Region(Region.X + 1, Region.Y + 1,
            Math.Max(0, Region.Width - 2), Math.Max(0, Region.Height - 2));
        var widgets = Children.OfType<Widget>().ToArray();
        Layout.Arrange(widgets, inner);
        // Recurre en contenedores y paneles hijos para que sus hijos también
        // reciban Region (de lo contrario sólo se renderiza el primer nivel).
        foreach (var child in widgets)
        {
            if (child is Layout.Container cc) cc.Arrange();
            else if (child is PanelWidget pp) pp.Arrange();
        }
    }

    public override IEnumerable<Strip> Render(Size size)
    {
        if (size.Width <= 0 || size.Height <= 0) yield break;
        // Reusamos BorderWidget para el frame.
        var frame = new BorderWidget
        {
            Kind = Border,
            Foreground = BorderForeground,
            Background = Background,
            Title = Title,
        };
        foreach (var s in frame.Render(size)) yield return s;
    }
}
