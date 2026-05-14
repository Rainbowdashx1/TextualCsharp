namespace TextualCsharp.Core;

/// <summary>Atributos visuales aplicables a una celda. Combinables como flags.</summary>
[Flags]
public enum StyleFlags : ushort
{
    None          = 0,
    Bold          = 1 << 0,
    Dim           = 1 << 1,
    Italic        = 1 << 2,
    Underline     = 1 << 3,
    Blink         = 1 << 4,
    Reverse       = 1 << 5,
    Strikethrough = 1 << 6,
}
