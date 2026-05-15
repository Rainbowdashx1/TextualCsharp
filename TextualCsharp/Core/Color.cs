namespace TextualCsharp.Core;

/// <summary>Modo de representación de color.</summary>
public enum ColorMode : byte
{
    /// <summary>Color por defecto del terminal (no se emite SGR).</summary>
    Default = 0,
    /// <summary>Color indexado 0-15 (ANSI 16).</summary>
    Indexed16 = 1,
    /// <summary>Color indexado 0-255 (xterm 256).</summary>
    Indexed256 = 2,
    /// <summary>Color RGB truecolor 24-bit.</summary>
    Rgb = 3,
}

/// <summary>
/// Color de una celda de consola. Internamente truecolor (24-bit) con un modo
/// que indica la mejor representación al emitirse vía ANSI.
/// </summary>
public readonly record struct Color(byte R, byte G, byte B, ColorMode Mode = ColorMode.Rgb)
{
    public static Color Default => new(0, 0, 0, ColorMode.Default);

    // ANSI 16 estándar — el byte R almacena el índice (0-15), no el componente RGB.
    public static Color Black => new(0, 0, 0, ColorMode.Indexed16);
    public static Color Red => new(1, 0, 0, ColorMode.Indexed16);
    public static Color Green => new(2, 0, 0, ColorMode.Indexed16);
    public static Color Yellow => new(3, 0, 0, ColorMode.Indexed16);
    public static Color Blue => new(4, 0, 0, ColorMode.Indexed16);
    public static Color Magenta => new(5, 0, 0, ColorMode.Indexed16);
    public static Color Cyan => new(6, 0, 0, ColorMode.Indexed16);
    public static Color White => new(7, 0, 0, ColorMode.Indexed16);

    public static Color FromRgb(byte r, byte g, byte b) => new(r, g, b, ColorMode.Rgb);
    public static Color FromIndexed256(byte index) => new(index, 0, 0, ColorMode.Indexed256);
    public static Color FromIndexed16(byte index) => new(index, 0, 0, ColorMode.Indexed16);

    /// <summary>Índice (sólo válido para Indexed16 / Indexed256).</summary>
    public byte Index => R;
}
