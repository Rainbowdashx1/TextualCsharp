namespace TextualCsharp.Layout;

/// <summary>Espaciado interior (en celdas) de un widget.</summary>
public readonly record struct Padding(int Top, int Right, int Bottom, int Left)
{
    public static Padding Zero => new(0, 0, 0, 0);
    public static Padding All(int v) => new(v, v, v, v);
    public static Padding Symmetric(int vertical, int horizontal) => new(vertical, horizontal, vertical, horizontal);

    public int Horizontal => Left + Right;
    public int Vertical => Top + Bottom;
}

/// <summary>Espaciado exterior (en celdas) de un widget.</summary>
public readonly record struct Margin(int Top, int Right, int Bottom, int Left)
{
    public static Margin Zero => new(0, 0, 0, 0);
    public static Margin All(int v) => new(v, v, v, v);
    public static Margin Symmetric(int vertical, int horizontal) => new(vertical, horizontal, vertical, horizontal);

    public int Horizontal => Left + Right;
    public int Vertical => Top + Bottom;
}

/// <summary>Alineación dentro de un contenedor a lo largo de un eje.</summary>
public enum Alignment
{
    Start,
    Center,
    End,
    Stretch,
}
