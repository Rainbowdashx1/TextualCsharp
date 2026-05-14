using TextualCsharp.Core;

namespace TextualCsharp.Theming;

/// <summary>
/// Tema completo: variables semánticas + diccionario de extensiones. Inmutable.
/// Equivalente a <c>textual.theme.Theme</c>.
/// </summary>
public sealed record Theme(string Name, Design Design, IReadOnlyDictionary<string, Color>? Extras = null)
{
    /// <summary>Obtiene un color por nombre, ya sea semántico o extra.</summary>
    public Color GetColor(string name)
    {
        return name.ToLowerInvariant() switch
        {
            "background" => Design.Background,
            "surface" => Design.Surface,
            "panel" => Design.Panel,
            "foreground" => Design.Foreground,
            "subtle" or "subtleforeground" => Design.SubtleForeground,
            "primary" => Design.Primary,
            "secondary" => Design.Secondary,
            "accent" => Design.Accent,
            "success" => Design.Success,
            "warning" => Design.Warning,
            "error" => Design.Error,
            "border" => Design.Border,
            _ => Extras is not null && Extras.TryGetValue(name, out var c) ? c : Color.Default,
        };
    }

    // ---- Temas incluidos ----

    public static readonly Theme Dark = new(
        "dark",
        new Design(
            Background: Color.FromRgb(15, 18, 28),
            Surface: Color.FromRgb(24, 28, 42),
            Panel: Color.FromRgb(32, 38, 56),
            Foreground: Color.FromRgb(220, 226, 240),
            SubtleForeground: Color.FromRgb(140, 146, 165),
            Primary: Color.FromRgb(95, 165, 255),
            Secondary: Color.FromRgb(180, 130, 255),
            Accent: Color.FromRgb(255, 200, 100),
            Success: Color.FromRgb(80, 200, 120),
            Warning: Color.FromRgb(240, 190, 70),
            Error: Color.FromRgb(240, 90, 90),
            Border: Color.FromRgb(80, 90, 120)));

    public static readonly Theme Light = new(
        "light",
        new Design(
            Background: Color.FromRgb(245, 246, 250),
            Surface: Color.FromRgb(235, 237, 244),
            Panel: Color.FromRgb(220, 224, 235),
            Foreground: Color.FromRgb(30, 35, 50),
            SubtleForeground: Color.FromRgb(110, 116, 135),
            Primary: Color.FromRgb(40, 100, 220),
            Secondary: Color.FromRgb(140, 60, 200),
            Accent: Color.FromRgb(210, 130, 30),
            Success: Color.FromRgb(50, 150, 80),
            Warning: Color.FromRgb(200, 140, 30),
            Error: Color.FromRgb(200, 60, 60),
            Border: Color.FromRgb(180, 184, 200)));

    public static readonly Theme Dracula = new(
        "dracula",
        new Design(
            Background: Color.FromRgb(40, 42, 54),
            Surface: Color.FromRgb(54, 58, 79),
            Panel: Color.FromRgb(68, 71, 90),
            Foreground: Color.FromRgb(248, 248, 242),
            SubtleForeground: Color.FromRgb(98, 114, 164),
            Primary: Color.FromRgb(189, 147, 249),
            Secondary: Color.FromRgb(255, 121, 198),
            Accent: Color.FromRgb(241, 250, 140),
            Success: Color.FromRgb(80, 250, 123),
            Warning: Color.FromRgb(255, 184, 108),
            Error: Color.FromRgb(255, 85, 85),
            Border: Color.FromRgb(98, 114, 164)));

    public static readonly Theme Nord = new(
        "nord",
        new Design(
            Background: Color.FromRgb(46, 52, 64),
            Surface: Color.FromRgb(59, 66, 82),
            Panel: Color.FromRgb(67, 76, 94),
            Foreground: Color.FromRgb(236, 239, 244),
            SubtleForeground: Color.FromRgb(180, 187, 200),
            Primary: Color.FromRgb(136, 192, 208),
            Secondary: Color.FromRgb(180, 142, 173),
            Accent: Color.FromRgb(235, 203, 139),
            Success: Color.FromRgb(163, 190, 140),
            Warning: Color.FromRgb(235, 203, 139),
            Error: Color.FromRgb(191, 97, 106),
            Border: Color.FromRgb(76, 86, 106)));

    public static readonly Theme Neon = new(
        "neon",
        new Design(
            Background: Color.FromRgb(10, 6, 24),
            Surface: Color.FromRgb(20, 12, 44),
            Panel: Color.FromRgb(32, 20, 70),
            Foreground: Color.FromRgb(220, 255, 250),
            SubtleForeground: Color.FromRgb(140, 130, 200),
            Primary: Color.FromRgb(0, 220, 255),
            Secondary: Color.FromRgb(255, 60, 200),
            Accent: Color.FromRgb(180, 255, 80),
            Success: Color.FromRgb(0, 255, 170),
            Warning: Color.FromRgb(255, 220, 80),
            Error: Color.FromRgb(255, 80, 130),
            Border: Color.FromRgb(120, 60, 200)));

    public static IReadOnlyList<Theme> All { get; } = new[] { Dark, Light, Dracula, Nord, Neon };
}

/// <summary>
/// Proveedor estático del tema activo. Los widgets que quieran reaccionar
/// pueden suscribirse a <see cref="Changed"/>.
/// </summary>
public static class ThemeProvider
{
    private static Theme _current = Theme.Dark;

    public static Theme Current
    {
        get => _current;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (_current == value) return;
            _current = value;
            Changed?.Invoke(value);
        }
    }

    public static event Action<Theme>? Changed;
}
