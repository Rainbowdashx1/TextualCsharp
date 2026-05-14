using TextualCsharp.Core;
using TextualCsharp.Rendering;

namespace TextualCsharp.Widgets;

/// <summary>
/// Contenido renderizable: una secuencia de spans con estilos inline.
/// Equivalente a <c>textual.content.Content</c>. Usable para construir
/// strips coloreados a partir de markup o spans manuales.
/// </summary>
public sealed class Content
{
    private readonly List<Markup.Span> _spans;

    public Content(IEnumerable<Markup.Span> spans)
    {
        _spans = spans?.ToList() ?? throw new ArgumentNullException(nameof(spans));
    }

    public static Content FromMarkup(string markup) => new(Markup.Parse(markup));

    public static Content Plain(string text, Color? fg = null, Color? bg = null, StyleFlags style = StyleFlags.None)
        => new(new[] { new Markup.Span(text ?? "", fg ?? Color.Default, bg ?? Color.Default, style) });

    /// <summary>Longitud total en celdas (suma de longitudes de los spans).</summary>
    public int Length
    {
        get
        {
            int n = 0;
            foreach (var s in _spans) n += s.Text.Length;
            return n;
        }
    }

    public IReadOnlyList<Markup.Span> Spans => _spans;

    /// <summary>Crea un strip de ancho exacto <paramref name="width"/> con padding/clip.</summary>
    public Strip ToStrip(int width, Color? backgroundFill = null, StyleFlags fillStyle = StyleFlags.None)
    {
        var bg = backgroundFill ?? Color.Default;
        var cells = new ConsoleCell[width];
        for (int i = 0; i < width; i++) cells[i] = new ConsoleCell(' ', Color.Default, bg, fillStyle);

        int col = 0;
        foreach (var span in _spans)
        {
            foreach (var ch in span.Text)
            {
                if (col >= width) return new Strip(cells);
                cells[col++] = new ConsoleCell(ch, span.Foreground, span.Background.Mode == ColorMode.Default ? bg : span.Background, span.Style);
            }
        }
        return new Strip(cells);
    }
}
