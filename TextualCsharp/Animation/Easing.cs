namespace TextualCsharp.Animation;

/// <summary>
/// Funciones de easing estándar. Cada función recibe t en [0, 1] y devuelve
/// el progreso suavizado en [0, 1]. Equivalente a <c>textual._easing</c>.
/// </summary>
public static class EasingFunctions
{
    public static double Linear(double t) => t;
    public static double EaseInQuad(double t) => t * t;
    public static double EaseOutQuad(double t) => 1 - (1 - t) * (1 - t);
    public static double EaseInOutQuad(double t) =>
        t < 0.5 ? 2 * t * t : 1 - Math.Pow(-2 * t + 2, 2) / 2;

    public static double EaseInCubic(double t) => t * t * t;
    public static double EaseOutCubic(double t) => 1 - Math.Pow(1 - t, 3);
    public static double EaseInOutCubic(double t) =>
        t < 0.5 ? 4 * t * t * t : 1 - Math.Pow(-2 * t + 2, 3) / 2;

    public static double EaseOutBack(double t)
    {
        const double c1 = 1.70158;
        const double c3 = c1 + 1;
        return 1 + c3 * Math.Pow(t - 1, 3) + c1 * Math.Pow(t - 1, 2);
    }

    public static double EaseOutBounce(double t)
    {
        const double n1 = 7.5625;
        const double d1 = 2.75;
        if (t < 1 / d1) return n1 * t * t;
        if (t < 2 / d1) { t -= 1.5 / d1; return n1 * t * t + 0.75; }
        if (t < 2.5 / d1) { t -= 2.25 / d1; return n1 * t * t + 0.9375; }
        t -= 2.625 / d1;
        return n1 * t * t + 0.984375;
    }

    public static double EaseInOutSine(double t) => -(Math.Cos(Math.PI * t) - 1) / 2;
}

public enum Easing
{
    Linear,
    EaseInQuad, EaseOutQuad, EaseInOutQuad,
    EaseInCubic, EaseOutCubic, EaseInOutCubic,
    EaseOutBack, EaseOutBounce, EaseInOutSine,
}

internal static class EasingExtensions
{
    public static double Apply(this Easing e, double t) => e switch
    {
        Easing.EaseInQuad => EasingFunctions.EaseInQuad(t),
        Easing.EaseOutQuad => EasingFunctions.EaseOutQuad(t),
        Easing.EaseInOutQuad => EasingFunctions.EaseInOutQuad(t),
        Easing.EaseInCubic => EasingFunctions.EaseInCubic(t),
        Easing.EaseOutCubic => EasingFunctions.EaseOutCubic(t),
        Easing.EaseInOutCubic => EasingFunctions.EaseInOutCubic(t),
        Easing.EaseOutBack => EasingFunctions.EaseOutBack(t),
        Easing.EaseOutBounce => EasingFunctions.EaseOutBounce(t),
        Easing.EaseInOutSine => EasingFunctions.EaseInOutSine(t),
        _ => EasingFunctions.Linear(t),
    };
}
