namespace TextualCsharp.Layout;

/// <summary>Tipo de unidad de tamaño.</summary>
public enum LayoutSizeKind : byte
{
    /// <summary>Tamaño absoluto en celdas.</summary>
    Fixed,
    /// <summary>Porcentaje del contenedor padre (0–100).</summary>
    Percent,
    /// <summary>Tamaño determinado por el contenido (<c>auto</c>).</summary>
    Auto,
    /// <summary>Fracción del espacio restante (CSS <c>fr</c>).</summary>
    Fraction,
}

/// <summary>
/// Union type para tamaños de layout. Equivalente a <c>textual.css.scalar</c> /
/// <c>_layout_resolve</c>. Usar las factorías estáticas.
/// </summary>
public readonly record struct LayoutSize(LayoutSizeKind Kind, double Value)
{
    public static LayoutSize Fixed(int cells) => new(LayoutSizeKind.Fixed, cells);
    public static LayoutSize Percent(double percent) => new(LayoutSizeKind.Percent, percent);
    public static LayoutSize Auto() => new(LayoutSizeKind.Auto, 0);
    public static LayoutSize Fraction(double fr) => new(LayoutSizeKind.Fraction, fr);

    public bool IsFixed => Kind == LayoutSizeKind.Fixed;
    public bool IsPercent => Kind == LayoutSizeKind.Percent;
    public bool IsAuto => Kind == LayoutSizeKind.Auto;
    public bool IsFraction => Kind == LayoutSizeKind.Fraction;

    public override string ToString() => Kind switch
    {
        LayoutSizeKind.Fixed => $"{(int)Value}",
        LayoutSizeKind.Percent => $"{Value}%",
        LayoutSizeKind.Auto => "auto",
        LayoutSizeKind.Fraction => $"{Value}fr",
        _ => "?",
    };
}
