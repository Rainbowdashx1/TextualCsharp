using TextualCsharp.Core;
using TextualCsharp.Core.Geometry;
using TextualCsharp.Rendering;

namespace TextualCsharp.Widgets;

/// <summary>
/// Widget de texto con word-wrap automático y soporte de markup inline
/// (<c>[bold red]…[/]</c>), igual que <see cref="Label"/> pero con múltiples
/// líneas cuando el contenido supera el ancho disponible.
/// </summary>
public sealed class WrapTextWidget : Widget
{
    private string _markup = "";
    private Content _content;

    public WrapTextWidget(string markup = "")
    {
        _content = Content.Plain("");
        Markup = markup;
    }

    /// <summary>
    /// Texto plano o markup inline (<c>[bold red]texto[/]</c>).
    /// Al asignarlo se parsea automáticamente igual que en <see cref="Label"/>.
    /// </summary>
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

    /// <summary>
    /// Calcula cuántas líneas necesita el contenido dado un ancho disponible,
    /// para que el VerticalLayout reserve el espacio correcto automáticamente.
    /// </summary>
    public override Size GetPreferredSize(Size available)
    {
        if (available.Width <= 0) return new Size(0, 1);
        var lines = WrapSpans(_content.Spans, available.Width);
        return new Size(available.Width, Math.Max(1, lines.Count));
    }

    public override IEnumerable<Strip> Render(Size size)
    {
        if (size.Width <= 0 || size.Height <= 0) yield break;

        var lines = WrapSpans(_content.Spans, size.Width);
        var blank = Strip.Filled(size.Width, new ConsoleCell(' ', Color.Default, Background, StyleFlags.None));

        for (int i = 0; i < size.Height; i++)
        {
            if (i < lines.Count)
                yield return new Content(lines[i]).ToStrip(size.Width, Background);
            else
                yield return blank;
        }
    }

    /// <summary>
    /// Divide los spans en líneas respetando el ancho máximo.
    /// Respeta saltos de línea explícitos <c>\n</c> dentro de cada span y
    /// hace word-wrap por palabras. Los estilos de cada span se preservan
    /// en las líneas resultantes.
    /// </summary>
    private static List<List<Markup.Span>> WrapSpans(IReadOnlyList<Markup.Span> spans, int maxWidth)
    {
        var lines = new List<List<Markup.Span>>();
        var currentLine = new List<Markup.Span>();
        int currentWidth = 0;

        void FlushLine()
        {
            lines.Add(currentLine);
            currentLine = [];
            currentWidth = 0;
        }

        foreach (var span in spans)
        {
            // Dividir el span en sub-partes por saltos de línea explícitos
            var parts = span.Text.Split('\n');
            for (int p = 0; p < parts.Length; p++)
            {
                if (p > 0) FlushLine();

                // Dividir la parte por palabras respetando el estilo del span
                var words = parts[p].Split(' ');
                for (int w = 0; w < words.Length; w++)
                {
                    string word = words[w];
                    string prefix = w > 0 ? " " : "";

                    // Palabra más larga que el ancho: dividir a la fuerza
                    if (word.Length >= maxWidth)
                    {
                        if (currentWidth > 0) FlushLine();
                        for (int i = 0; i < word.Length; i += maxWidth)
                        {
                            string chunk = word[i..Math.Min(i + maxWidth, word.Length)];
                            currentLine.Add(new Markup.Span(chunk, span.Foreground, span.Background, span.Style));
                            currentWidth += chunk.Length;
                            if (currentWidth >= maxWidth) FlushLine();
                        }
                        continue;
                    }

                    int needed = currentWidth == 0 ? word.Length : currentWidth + prefix.Length + word.Length;
                    if (needed > maxWidth)
                    {
                        FlushLine();
                        prefix = "";
                    }

                    string token = prefix + word;
                    if (token.Length > 0)
                    {
                        currentLine.Add(new Markup.Span(token, span.Foreground, span.Background, span.Style));
                        currentWidth += token.Length;
                    }
                }
            }
        }

        // Última línea pendiente
        lines.Add(currentLine);

        return lines;
    }
}
