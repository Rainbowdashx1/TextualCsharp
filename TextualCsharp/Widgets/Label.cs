using TextualCsharp.Core;
using TextualCsharp.Core.Geometry;
using TextualCsharp.Rendering;

namespace TextualCsharp.Widgets;

/// <summary>
/// Widget de texto con soporte de markup inline (<c>[bold red]…[/]</c>).
/// Equivalente a <c>textual.widgets.Label</c>.
/// </summary>
public sealed class Label : Widget
{
    private Content _content;
    private string _markup = "";

    public Label(string markup = "")
    {
        Markup = markup;
        Height = Layout.LayoutSize.Fixed(1);
        _content ??= Content.Plain("");
    }

    public string Markup
    {
        get => _markup;
        set
        {
            if (_markup == value) return;
            _markup = value ?? "";
            _content = Content.FromMarkup(_markup);
            Invalidate();
        }
    }

    public Color Background { get; set; } = Color.Default;

    public override Size GetPreferredSize(Size available)
        => new(Math.Min(_content.Length, available.Width), 1);

    public override IEnumerable<Strip> Render(Size size)
    {
        if (size.Width <= 0 || size.Height <= 0) yield break;
        yield return _content.ToStrip(size.Width, Background);
        var blank = Strip.Filled(size.Width, new ConsoleCell(' ', Color.Default, Background, StyleFlags.None));
        for (int i = 1; i < size.Height; i++) yield return blank;
    }
}
