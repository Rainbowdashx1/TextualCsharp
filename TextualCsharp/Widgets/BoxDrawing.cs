namespace TextualCsharp.Widgets;

/// <summary>
/// Caracteres Unicode de dibujo de cajas. Equivalente a <c>textual._box_drawing</c>.
/// </summary>
public static class BoxDrawing
{
    /// <summary>
    /// Devuelve true si la consola actual puede representar caracteres Unicode de caja.
    /// Se usa para hacer fallback automático a ASCII en entornos sin soporte.
    /// </summary>
    public static bool SupportsUnicode =>
        Console.OutputEncoding.CodePage == 65001   // UTF-8
        || Console.OutputEncoding.CodePage == 1200  // UTF-16 LE
        || Console.OutputEncoding.CodePage == 1201; // UTF-16 BE

    // Light single line
    public const char LightHorizontal = '─';
    public const char LightVertical = '│';
    public const char LightTopLeft = '┌';
    public const char LightTopRight = '┐';
    public const char LightBottomLeft = '└';
    public const char LightBottomRight = '┘';
    public const char LightCross = '┼';
    public const char LightTeeDown = '┬';
    public const char LightTeeUp = '┴';
    public const char LightTeeRight = '├';
    public const char LightTeeLeft = '┤';

    // Heavy line
    public const char HeavyHorizontal = '━';
    public const char HeavyVertical = '┃';
    public const char HeavyTopLeft = '┏';
    public const char HeavyTopRight = '┓';
    public const char HeavyBottomLeft = '┗';
    public const char HeavyBottomRight = '┛';

    // Double line
    public const char DoubleHorizontal = '═';
    public const char DoubleVertical = '║';
    public const char DoubleTopLeft = '╔';
    public const char DoubleTopRight = '╗';
    public const char DoubleBottomLeft = '╚';
    public const char DoubleBottomRight = '╝';

    // Rounded
    public const char RoundedTopLeft = '╭';
    public const char RoundedTopRight = '╮';
    public const char RoundedBottomLeft = '╰';
    public const char RoundedBottomRight = '╯';

    /// <summary>Estilo del borde.</summary>
    public enum BorderKind
    {
        None,
        Light,
        Heavy,
        Double,
        Rounded,
        Ascii,
    }

    /// <summary>Obtiene los 6 caracteres de un borde: H, V, TL, TR, BL, BR.</summary>
    public static (char H, char V, char TL, char TR, char BL, char BR) GetGlyphs(BorderKind kind)
    {

        return kind switch
        {
            BorderKind.Light   => (LightHorizontal, LightVertical, LightTopLeft, LightTopRight, LightBottomLeft, LightBottomRight),
            BorderKind.Heavy   => (HeavyHorizontal, HeavyVertical, HeavyTopLeft, HeavyTopRight, HeavyBottomLeft, HeavyBottomRight),
            BorderKind.Double  => (DoubleHorizontal, DoubleVertical, DoubleTopLeft, DoubleTopRight, DoubleBottomLeft, DoubleBottomRight),
            BorderKind.Rounded => (LightHorizontal, LightVertical, RoundedTopLeft, RoundedTopRight, RoundedBottomLeft, RoundedBottomRight),
            BorderKind.Ascii   => ('-', '|', '+', '+', '+', '+'),
            _                  => (' ', ' ', ' ', ' ', ' ', ' '),
        };
    }
}
