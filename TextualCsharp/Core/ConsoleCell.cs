namespace TextualCsharp.Core;

/// <summary>
/// Una celda renderizable de la consola: glifo + color FG/BG + flags de estilo.
/// Inmutable y compacta para minimizar allocations durante el diff.
/// </summary>
public readonly record struct ConsoleCell(
    char Glyph,
    Color Foreground,
    Color Background,
    StyleFlags Style)
{
    public static ConsoleCell Empty => new(' ', Color.Default, Color.Default, StyleFlags.None);

    public ConsoleCell(char glyph) : this(glyph, Color.Default, Color.Default, StyleFlags.None) { }
}
