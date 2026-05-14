using TextualCsharp.Core.Geometry;
using TextualCsharp.Rendering;
using TextualCsharp.Widgets;

namespace TextualCsharp.Layout;

/// <summary>
/// Widget contenedor genérico que aplica un <see cref="ILayout"/> a sus hijos.
/// Equivalente conceptual a <c>textual.containers</c>.
/// </summary>
public class Container : Widget
{
    public Container(ILayout layout)
    {
        Layout = layout ?? throw new ArgumentNullException(nameof(layout));
    }

    public ILayout Layout { get; set; }
    public MapGeometry? LastGeometry { get; private set; }

    /// <summary>Reordena los hijos dentro del área asignada al contenedor.</summary>
    public MapGeometry Arrange()
    {
        var children = Children.OfType<Widget>().ToList();
        LastGeometry = Layout.Arrange(children, Region);
        // Re-arrange contenedores y paneles hijos recursivamente para que sus
        // descendientes también reciban Region.
        foreach (var c in children)
        {
            if (c is Container cc) cc.Arrange();
            else if (c is PanelWidget pp) pp.Arrange();
        }
        return LastGeometry;
    }

    public override IEnumerable<Strip> Render(Size size)
    {
        // El contenedor no se pinta a sí mismo; sus hijos se pintan por separado.
        var blank = Strip.Filled(size.Width, new Core.ConsoleCell(' '));
        for (int y = 0; y < size.Height; y++) yield return blank;
    }
}

/// <summary>Contenedor vertical predefinido.</summary>
public sealed class VerticalContainer : Container
{
    public VerticalContainer() : base(new VerticalLayout()) { }
    public new VerticalLayout Layout => (VerticalLayout)base.Layout;
}

/// <summary>Contenedor horizontal predefinido.</summary>
public sealed class HorizontalContainer : Container
{
    public HorizontalContainer() : base(new HorizontalLayout()) { }
    public new HorizontalLayout Layout => (HorizontalLayout)base.Layout;
}

/// <summary>Contenedor en grid predefinido.</summary>
public sealed class GridContainer : Container
{
    public GridContainer(IReadOnlyList<LayoutSize> columns, IReadOnlyList<LayoutSize> rows)
        : base(new GridLayout(columns, rows)) { }
    public new GridLayout Layout => (GridLayout)base.Layout;
}
