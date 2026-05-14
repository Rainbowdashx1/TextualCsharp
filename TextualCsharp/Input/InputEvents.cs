using TextualCsharp.Core.Geometry;
using TextualCsharp.Messaging;

namespace TextualCsharp.Input;

/// <summary>Evento de teclado. Equivalente a <c>textual.events.Key</c>.</summary>
public sealed record KeyEvent(Key Key, char Character, KeyModifiers Modifiers) : Message
{
    /// <summary>Nombre canónico tipo <c>ctrl+shift+a</c> (útil para bindings).</summary>
    public string Name
    {
        get
        {
            var parts = new List<string>(4);
            if ((Modifiers & KeyModifiers.Control) != 0) parts.Add("ctrl");
            if ((Modifiers & KeyModifiers.Alt) != 0) parts.Add("alt");
            if ((Modifiers & KeyModifiers.Shift) != 0) parts.Add("shift");
            parts.Add(Key == Key.Character
                ? char.ToLowerInvariant(Character).ToString()
                : Key.ToString().ToLowerInvariant());
            return string.Join('+', parts);
        }
    }
}

/// <summary>Botones del ratón.</summary>
[Flags]
public enum MouseButton
{
    None = 0,
    Left = 1 << 0,
    Right = 1 << 1,
    Middle = 1 << 2,
}

/// <summary>Tipo de evento de ratón.</summary>
public enum MouseEventType
{
    Move,
    Down,
    Up,
    ScrollUp,
    ScrollDown,
}

/// <summary>Evento de ratón. Equivalente a <c>textual.events.MouseEvent</c>.</summary>
public sealed record MouseEvent(
    MouseEventType Type,
    int X,
    int Y,
    MouseButton Button,
    KeyModifiers Modifiers) : Message
{
    public Offset Position => new(X, Y);
}

/// <summary>El terminal ha cambiado de tamaño.</summary>
public sealed record ResizeEvent(int Width, int Height) : Message
{
    public Size Size => new(Width, Height);
}

/// <summary>El widget ha obtenido el foco.</summary>
public sealed record FocusEvent : Message
{
    public override bool Bubble => false;
}

/// <summary>El widget ha perdido el foco.</summary>
public sealed record BlurEvent : Message
{
    public override bool Bubble => false;
}

/// <summary>Solicitud de salida limpia de la app.</summary>
public sealed record QuitEvent : Message;
