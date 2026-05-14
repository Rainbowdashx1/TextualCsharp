namespace TextualCsharp.Input;

/// <summary>
/// Enumeración de teclas soportadas por el driver. Equivalente a <c>textual.keys.Keys</c>.
/// Los valores que tienen un equivalente directo en <see cref="ConsoleKey"/> se mapean 1:1.
/// </summary>
public enum Key
{
    Unknown = 0,

    // Caracteres imprimibles: usar KeyEvent.Character para el glifo real.
    Character,

    // Navegación
    Up,
    Down,
    Left,
    Right,
    Home,
    End,
    PageUp,
    PageDown,

    // Edición
    Enter,
    Escape,
    Backspace,
    Tab,
    BackTab,
    Insert,
    Delete,
    Space,

    // Funciones
    F1, F2, F3, F4, F5, F6,
    F7, F8, F9, F10, F11, F12,
}

/// <summary>Flags de modificadores que pueden venir junto a una tecla.</summary>
[Flags]
public enum KeyModifiers
{
    None = 0,
    Shift = 1 << 0,
    Alt = 1 << 1,
    Control = 1 << 2,
}
