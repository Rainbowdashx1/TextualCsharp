namespace TextualCsharp.Core.Geometry;

/// <summary>Desplazamiento (x, y) en celdas de consola.</summary>
public readonly record struct Offset(int X, int Y)
{
    public static Offset Zero => new(0, 0);
    public static Offset operator +(Offset a, Offset b) => new(a.X + b.X, a.Y + b.Y);
    public static Offset operator -(Offset a, Offset b) => new(a.X - b.X, a.Y - b.Y);
}
