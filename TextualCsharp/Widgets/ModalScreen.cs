using TextualCsharp.App;
using TextualCsharp.Core;
using TextualCsharp.Core.Geometry;
using TextualCsharp.Input;
using TextualCsharp.Layout;
using TextualCsharp.Rendering;
using TextualCsharp.Widgets;

namespace TextualCsharp.Widgets;

/// <summary>
/// Pantalla modal que se superpone a la pantalla anterior, dejando un marco
/// con borde alrededor del contenido. Equivalente a <c>textual.screen.ModalScreen</c>.
/// </summary>
public class ModalScreen : Screen
{
    public ModalScreen(Widget content, string? title = null, int? width = null, int? height = null)
        : base(BuildRoot(content, title, width, height))
    {
        ContentWidget = content;
        Title = title;
        Bindings.Add("escape", "close");
    }

    public Widget ContentWidget { get; }
    public string? Title { get; }

    public override void Arrange(Region region)
    {
        Region = region;
        Root.Region = region;
        if (Root is CenterContainer cc) cc.ArrangeSelf();
        else if (Root is Layout.Container container) container.Arrange();
        Focus.Rebuild(Root);
    }

    private static Widget BuildRoot(Widget content, string? title, int? width, int? height)
    {
        var panel = new PanelWidget
        {
            Title = title,
            BorderForeground = Color.FromRgb(200, 200, 80),
            Background = Color.FromRgb(10, 10, 20),
            Layout = new VerticalLayout(),
        };
        panel.Children.Add(content);
        if (width is not null) panel.Width = LayoutSize.Fixed(width.Value);
        if (height is not null) panel.Height = LayoutSize.Fixed(height.Value);
        return new CenterContainer(panel, width, height);
    }
}

/// <summary>Contenedor que centra a un único hijo en su área. Soporta tamaño deseado.</summary>
internal sealed class CenterContainer : Widget
{
    private readonly Widget _child;
    private readonly int? _w;
    private readonly int? _h;

    public CenterContainer(Widget child, int? width = null, int? height = null)
    {
        _child = child;
        _w = width;
        _h = height;
        Children.Add(child);
    }

    public void ArrangeSelf()
    {
        int w = _w ?? Math.Min(Region.Width, Math.Max(20, Region.Width * 2 / 3));
        int h = _h ?? Math.Min(Region.Height, Math.Max(7, Region.Height / 2));
        int x = Region.X + (Region.Width - w) / 2;
        int y = Region.Y + (Region.Height - h) / 2;
        _child.Region = new Region(x, y, w, h);
        if (_child is Layout.Container c) c.Arrange();
        else if (_child is PanelWidget p) p.Arrange();
    }

    public override IEnumerable<Strip> Render(Size size)
    {
        // El contenido propio del centrador es transparente.
        var blank = Strip.Filled(size.Width, new ConsoleCell(' ', Color.Default, Color.Default, StyleFlags.None));
        for (int y = 0; y < size.Height; y++) yield return blank;
    }
}
