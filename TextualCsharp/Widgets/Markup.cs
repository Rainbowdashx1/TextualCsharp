using TextualCsharp.Core;

namespace TextualCsharp.Widgets;

/// <summary>
/// Parser sencillo de markup tipo Rich/Textual: <c>[bold red]texto[/]</c>.
/// Etiquetas soportadas: <c>bold</c>, <c>dim</c>, <c>italic</c>, <c>underline</c>,
/// <c>blink</c>, <c>reverse</c>, <c>strike</c>, nombres de color ANSI 16
/// (<c>red</c>, <c>green</c>…), <c>on &lt;color&gt;</c> para background, o
/// <c>#RRGGBB</c> para truecolor. <c>[/]</c> cierra el ámbito.
/// </summary>
public static class Markup
{
    public readonly record struct Span(string Text, Color Foreground, Color Background, StyleFlags Style);

    public static IReadOnlyList<Span> Parse(string source)
    {
        var spans = new List<Span>();
        if (string.IsNullOrEmpty(source)) return spans;

        var stack = new Stack<(Color Fg, Color Bg, StyleFlags Style)>();
        var current = (Fg: Color.Default, Bg: Color.Default, Style: StyleFlags.None);
        int i = 0;
        var buffer = new System.Text.StringBuilder();

        void Flush()
        {
            if (buffer.Length == 0) return;
            spans.Add(new Span(buffer.ToString(), current.Fg, current.Bg, current.Style));
            buffer.Clear();
        }

        while (i < source.Length)
        {
            char ch = source[i];
            if (ch == '[' && i + 1 < source.Length && source[i + 1] == '[')
            {
                buffer.Append('['); i += 2; continue;
            }
            if (ch == '[')
            {
                int end = source.IndexOf(']', i + 1);
                if (end < 0) { buffer.Append(ch); i++; continue; }
                string tag = source.Substring(i + 1, end - i - 1).Trim();
                Flush();
                if (tag == "/" || tag.StartsWith("/"))
                {
                    if (stack.Count > 0) current = stack.Pop();
                }
                else
                {
                    stack.Push(current);
                    ApplyTag(ref current, tag);
                }
                i = end + 1;
                continue;
            }
            buffer.Append(ch);
            i++;
        }
        Flush();
        return spans;
    }

    private static void ApplyTag(ref (Color Fg, Color Bg, StyleFlags Style) current, string tag)
    {
        foreach (var token in tag.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (string.Equals(token, "on", StringComparison.OrdinalIgnoreCase))
            {
                // marcador: la siguiente palabra será background; ignoramos aquí,
                // se procesa abajo gracias al estado del bucle.
                continue;
            }
            // Heurística: si la última palabra fue 'on', el color es bg.
        }

        // Re-tokenizamos para distinguir 'on <color>'.
        var parts = tag.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        bool nextIsBg = false;
        foreach (var raw in parts)
        {
            string tok = raw.ToLowerInvariant();
            if (tok == "on") { nextIsBg = true; continue; }

            switch (tok)
            {
                case "bold": current.Style |= StyleFlags.Bold; continue;
                case "dim": current.Style |= StyleFlags.Dim; continue;
                case "italic": current.Style |= StyleFlags.Italic; continue;
                case "underline": current.Style |= StyleFlags.Underline; continue;
                case "blink": current.Style |= StyleFlags.Blink; continue;
                case "reverse": current.Style |= StyleFlags.Reverse; continue;
                case "strike":
                case "strikethrough": current.Style |= StyleFlags.Strikethrough; continue;
            }

            if (TryParseColor(tok, out var color))
            {
                if (nextIsBg) current.Bg = color;
                else current.Fg = color;
                nextIsBg = false;
            }
        }
    }

    private static bool TryParseColor(string token, out Color color)
    {
        switch (token)
        {
            case "black": color = Color.Black; return true;
            case "red": color = Color.Red; return true;
            case "green": color = Color.Green; return true;
            case "yellow": color = Color.Yellow; return true;
            case "blue": color = Color.Blue; return true;
            case "magenta": color = Color.Magenta; return true;
            case "cyan": color = Color.Cyan; return true;
            case "white": color = Color.White; return true;
            case "default": color = Color.Default; return true;
        }
        if (token.StartsWith("#") && token.Length == 7
            && byte.TryParse(token.AsSpan(1, 2), System.Globalization.NumberStyles.HexNumber, null, out var r)
            && byte.TryParse(token.AsSpan(3, 2), System.Globalization.NumberStyles.HexNumber, null, out var g)
            && byte.TryParse(token.AsSpan(5, 2), System.Globalization.NumberStyles.HexNumber, null, out var b))
        {
            color = Color.FromRgb(r, g, b);
            return true;
        }
        color = Color.Default;
        return false;
    }
}
