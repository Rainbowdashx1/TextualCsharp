using TextualCsharp.Core;

namespace TextualCsharp.Theming;

/// <summary>
/// Fallback para terminales con sólo 16 colores. Convierte un <see cref="Color"/>
/// truecolor a su índice ANSI 16 más cercano. Equivalente a <c>textual._ansi_theme</c>.
/// </summary>
public static class AnsiTheme
{
    private static readonly (byte R, byte G, byte B)[] Palette = new (byte, byte, byte)[]
    {
        (0,   0,   0  ),  // 0  black
        (170, 0,   0  ),  // 1  red
        (0,   170, 0  ),  // 2  green
        (170, 85,  0  ),  // 3  yellow
        (0,   0,   170),  // 4  blue
        (170, 0,   170),  // 5  magenta
        (0,   170, 170),  // 6  cyan
        (170, 170, 170),  // 7  white
        (85,  85,  85 ),  // 8  bright black
        (255, 85,  85 ),  // 9  bright red
        (85,  255, 85 ),  // 10 bright green
        (255, 255, 85 ),  // 11 bright yellow
        (85,  85,  255),  // 12 bright blue
        (255, 85,  255),  // 13 bright magenta
        (85,  255, 255),  // 14 bright cyan
        (255, 255, 255),  // 15 bright white
    };

    /// <summary>Devuelve el color ANSI 16 más cercano al color RGB dado.</summary>
    public static Color DowngradeToAnsi16(Color c)
    {
        if (c.Mode == ColorMode.Default || c.Mode == ColorMode.Indexed16) return c;
        int best = 0;
        int bestDist = int.MaxValue;
        for (int i = 0; i < Palette.Length; i++)
        {
            int dr = c.R - Palette[i].R;
            int dg = c.G - Palette[i].G;
            int db = c.B - Palette[i].B;
            int d = dr * dr + dg * dg + db * db;
            if (d < bestDist) { bestDist = d; best = i; }
        }
        return Color.FromIndexed16((byte)best);
    }
}
