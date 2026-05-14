using System.Text;
using TextualCsharp.Core;

namespace TextualCsharp.Rendering;

/// <summary>
/// Renderer que emite secuencias ANSI (CSI / SGR) al <see cref="TextWriter"/> destino.
/// Coalescea celdas contiguas con el mismo estilo y minimiza movimientos de cursor.
/// </summary>
public sealed class AnsiWriter : IRenderer
{
    private const string Esc = "\u001b";
    private const string Csi = "\u001b[";

    private readonly TextWriter _out;
    private readonly StringBuilder _sb = new(1024);

    public AnsiWriter(TextWriter? output = null)
    {
        _out = output ?? Console.Out;
    }

    public void Apply(IReadOnlyList<CellChange> changes)
    {
        ArgumentNullException.ThrowIfNull(changes);
        if (changes.Count == 0) return;

        _sb.Clear();

        int? lastX = null, lastY = null;
        Color? lastFg = null;
        Color? lastBg = null;
        StyleFlags? lastStyle = null;

        for (int i = 0; i < changes.Count; i++)
        {
            var ch = changes[i];

            // Posición: si no es continuación de la celda previa, mover cursor (CUP).
            if (lastY != ch.Y || lastX != ch.X)
                _sb.Append(Csi).Append(ch.Y + 1).Append(';').Append(ch.X + 1).Append('H');

            // SGR sólo si cambia algún atributo.
            if (lastFg is null || !lastFg.Value.Equals(ch.Cell.Foreground)
                || lastBg is null || !lastBg.Value.Equals(ch.Cell.Background)
                || lastStyle is null || lastStyle.Value != ch.Cell.Style)
            {
                AppendSgr(_sb, ch.Cell.Foreground, ch.Cell.Background, ch.Cell.Style);
                lastFg = ch.Cell.Foreground;
                lastBg = ch.Cell.Background;
                lastStyle = ch.Cell.Style;
            }

            _sb.Append(ch.Cell.Glyph);

            lastX = ch.X + 1;
            lastY = ch.Y;
        }

        // Reset al final para no contaminar el terminal del usuario.
        _sb.Append(Csi).Append("0m");
        _out.Write(_sb.ToString());
    }

    public void Clear()
    {
        // Reset SGR + clear screen + cursor home.
        _out.Write(Csi + "0m" + Csi + "2J" + Csi + "H");
    }

    /// <summary>Entra al alternate screen buffer (no afecta al scrollback principal).</summary>
    public void EnterAltScreen() => _out.Write(Csi + "?1049h");

    /// <summary>Sale del alternate screen buffer y restaura el contenido previo.</summary>
    public void LeaveAltScreen() => _out.Write(Csi + "?1049l");

    /// <summary>Oculta el cursor.</summary>
    public void HideCursor() => _out.Write(Csi + "?25l");

    /// <summary>Muestra el cursor.</summary>
    public void ShowCursor() => _out.Write(Csi + "?25h");

    public void Flush() => _out.Flush();

    /// <summary>Emite la secuencia SGR (Select Graphic Rendition) para un estilo dado.</summary>
    internal static void AppendSgr(StringBuilder sb, Color fg, Color bg, StyleFlags style)
    {
        sb.Append(Csi).Append('0'); // reset y luego añadimos atributos

        if ((style & StyleFlags.Bold) != 0) sb.Append(";1");
        if ((style & StyleFlags.Dim) != 0) sb.Append(";2");
        if ((style & StyleFlags.Italic) != 0) sb.Append(";3");
        if ((style & StyleFlags.Underline) != 0) sb.Append(";4");
        if ((style & StyleFlags.Blink) != 0) sb.Append(";5");
        if ((style & StyleFlags.Reverse) != 0) sb.Append(";7");
        if ((style & StyleFlags.Strikethrough) != 0) sb.Append(";9");

        AppendColor(sb, fg, foreground: true);
        AppendColor(sb, bg, foreground: false);

        sb.Append('m');
    }

    private static void AppendColor(StringBuilder sb, Color c, bool foreground)
    {
        switch (c.Mode)
        {
            case ColorMode.Default:
                // No se emite nada para el modo por defecto (ya hubo reset al inicio).
                break;
            case ColorMode.Indexed16:
                int idx = c.Index & 0x0F;
                int baseCode = foreground
                    ? (idx < 8 ? 30 + idx : 90 + (idx - 8))
                    : (idx < 8 ? 40 + idx : 100 + (idx - 8));
                sb.Append(';').Append(baseCode);
                break;
            case ColorMode.Indexed256:
                sb.Append(foreground ? ";38;5;" : ";48;5;").Append(c.Index);
                break;
            case ColorMode.Rgb:
                sb.Append(foreground ? ";38;2;" : ";48;2;")
                  .Append(c.R).Append(';').Append(c.G).Append(';').Append(c.B);
                break;
        }
    }
}
