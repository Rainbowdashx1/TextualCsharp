using TextualCsharp.Core;

namespace TextualCsharp.Theming;

/// <summary>
/// Tokens de diseño base (paleta semántica) que todo <see cref="Theme"/>
/// debe proveer. Equivalente a <c>textual.design</c>.
/// </summary>
public sealed record Design(
    Color Background,
    Color Surface,
    Color Panel,
    Color Foreground,
    Color SubtleForeground,
    Color Primary,
    Color Secondary,
    Color Accent,
    Color Success,
    Color Warning,
    Color Error,
    Color Border);
