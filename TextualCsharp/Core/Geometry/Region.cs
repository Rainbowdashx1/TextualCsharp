namespace TextualCsharp.Core.Geometry;

/// <summary>Región rectangular en el espacio de coordenadas de la consola.</summary>
public readonly record struct Region(int X, int Y, int Width, int Height)
{
    public static Region Empty => new(0, 0, 0, 0);

    public int Right => X + Width;
    public int Bottom => Y + Height;
    public Size Size => new(Width, Height);
    public Offset Origin => new(X, Y);
    public bool IsEmpty => Width <= 0 || Height <= 0;

    public bool Contains(int x, int y) =>
        x >= X && x < Right && y >= Y && y < Bottom;

    public Region Intersect(Region other)
    {
        int x1 = Math.Max(X, other.X);
        int y1 = Math.Max(Y, other.Y);
        int x2 = Math.Min(Right, other.Right);
        int y2 = Math.Min(Bottom, other.Bottom);
        if (x2 <= x1 || y2 <= y1) return Empty;
        return new Region(x1, y1, x2 - x1, y2 - y1);
    }
}
