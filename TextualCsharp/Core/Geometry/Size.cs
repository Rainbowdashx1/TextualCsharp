namespace TextualCsharp.Core.Geometry;

/// <summary>Tamaño en celdas de consola (ancho x alto).</summary>
public readonly record struct Size(int Width, int Height)
{
    public static Size Empty => new(0, 0);
    public int Area => Width * Height;
    public bool IsEmpty => Width <= 0 || Height <= 0;
}
