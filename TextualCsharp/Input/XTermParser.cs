namespace TextualCsharp.Input;

/// <summary>
/// Parser de secuencias de escape XTerm/VT100. Equivalente simplificado a
/// <c>textual._xterm_parser</c>. Soporta:
///   * Teclas de navegación (CSI A/B/C/D, H, F, 1~ .. 6~).
///   * Teclas de función F1-F12 (SS3 P/Q/R/S y CSI 11~ .. 24~).
///   * Modificadores codificados como CSI 1;<m> X.
///   * Mouse en formato SGR (CSI &lt; b ; x ; y M/m).
/// </summary>
public sealed class XTermParser
{
    /// <summary>Resultado del parser: tecla / mouse / texto literal.</summary>
    public abstract record Token;
    public sealed record KeyToken(Key Key, char Character, KeyModifiers Modifiers) : Token;
    public sealed record MouseToken(MouseEventType Type, int X, int Y, MouseButton Button, KeyModifiers Modifiers) : Token;

    /// <summary>Parsea una secuencia previamente acumulada y devuelve los tokens reconocidos.</summary>
    public IEnumerable<Token> Parse(ReadOnlyMemory<char> input)
    {
        var span = input;
        int i = 0;
        while (i < span.Length)
        {
            char c = span.Span[i];
            if (c == '\u001b' && i + 1 < span.Length)
            {
                char next = span.Span[i + 1];
                if (next == '[')
                {
                    if (TryParseCsi(span, i, out var token, out int consumed))
                    {
                        if (token is not null) yield return token;
                        i += consumed;
                        continue;
                    }
                }
                else if (next == 'O' && i + 2 < span.Length)
                {
                    // SS3: F1-F4 sin modificadores
                    Key k = span.Span[i + 2] switch
                    {
                        'P' => Key.F1,
                        'Q' => Key.F2,
                        'R' => Key.F3,
                        'S' => Key.F4,
                        _ => Key.Unknown,
                    };
                    if (k != Key.Unknown)
                    {
                        yield return new KeyToken(k, '\0', KeyModifiers.None);
                        i += 3;
                        continue;
                    }
                }
                // ESC seguido de un carácter normal => Alt+<char>
                if (next >= ' ' && next < 127)
                {
                    yield return new KeyToken(Key.Character, next, KeyModifiers.Alt);
                    i += 2;
                    continue;
                }
                // ESC sola
                yield return new KeyToken(Key.Escape, '\0', KeyModifiers.None);
                i += 1;
                continue;
            }

            yield return ClassifyControlOrChar(c);
            i++;
        }
    }

    private static KeyToken ClassifyControlOrChar(char c)
    {
        switch (c)
        {
            case '\r':
            case '\n':
                return new KeyToken(Key.Enter, '\0', KeyModifiers.None);
            case '\t':
                return new KeyToken(Key.Tab, '\0', KeyModifiers.None);
            case '\u007f':
            case '\b':
                return new KeyToken(Key.Backspace, '\0', KeyModifiers.None);
            case '\u001b':
                return new KeyToken(Key.Escape, '\0', KeyModifiers.None);
        }
        if (c < ' ')
        {
            // Ctrl+A..Z
            char letter = (char)(c + 'a' - 1);
            return new KeyToken(Key.Character, letter, KeyModifiers.Control);
        }
        return new KeyToken(Key.Character, c, KeyModifiers.None);
    }

    private static bool TryParseCsi(ReadOnlyMemory<char> mem, int start, out Token? token, out int consumed)
    {
        token = null;
        consumed = 0;
        var span = mem.Span;
        // Empieza en ESC[, busca el final (cualquier letra ASCII >= 0x40)
        int p = start + 2;
        bool sgrMouse = p < span.Length && span[p] == '<';
        if (sgrMouse) p++;
        int paramStart = p;
        while (p < span.Length)
        {
            char ch = span[p];
            if (ch >= 0x40 && ch <= 0x7E)
            {
                var paramText = span.Slice(paramStart, p - paramStart).ToString();
                consumed = p - start + 1;
                token = sgrMouse
                    ? ParseSgrMouse(paramText, ch)
                    : ParseCsi(paramText, ch);
                return true;
            }
            p++;
        }
        return false;
    }

    private static Token? ParseCsi(string parameters, char final)
    {
        // parámetros tipo "1;5" (modificador), "5", o vacío
        int[] nums = parameters.Length == 0
            ? Array.Empty<int>()
            : parameters.Split(';').Select(s => int.TryParse(s, out var v) ? v : 0).ToArray();

        KeyModifiers mods = KeyModifiers.None;
        if (nums.Length >= 2)
            mods = DecodeModifiers(nums[1]);

        switch (final)
        {
            case 'A': return new KeyToken(Key.Up, '\0', mods);
            case 'B': return new KeyToken(Key.Down, '\0', mods);
            case 'C': return new KeyToken(Key.Right, '\0', mods);
            case 'D': return new KeyToken(Key.Left, '\0', mods);
            case 'H': return new KeyToken(Key.Home, '\0', mods);
            case 'F': return new KeyToken(Key.End, '\0', mods);
            case 'Z': return new KeyToken(Key.BackTab, '\0', mods | KeyModifiers.Shift);
            case '~' when nums.Length >= 1:
                Key k = nums[0] switch
                {
                    1 => Key.Home,
                    2 => Key.Insert,
                    3 => Key.Delete,
                    4 => Key.End,
                    5 => Key.PageUp,
                    6 => Key.PageDown,
                    11 => Key.F1,
                    12 => Key.F2,
                    13 => Key.F3,
                    14 => Key.F4,
                    15 => Key.F5,
                    17 => Key.F6,
                    18 => Key.F7,
                    19 => Key.F8,
                    20 => Key.F9,
                    21 => Key.F10,
                    23 => Key.F11,
                    24 => Key.F12,
                    _ => Key.Unknown,
                };
                return k == Key.Unknown ? null : new KeyToken(k, '\0', mods);
        }
        return null;
    }

    private static Token? ParseSgrMouse(string parameters, char final)
    {
        if (final != 'M' && final != 'm') return null;
        var parts = parameters.Split(';');
        if (parts.Length != 3) return null;
        if (!int.TryParse(parts[0], out int b) ||
            !int.TryParse(parts[1], out int x) ||
            !int.TryParse(parts[2], out int y)) return null;

        bool isUp = final == 'm';
        var mods = KeyModifiers.None;
        if ((b & 4) != 0) mods |= KeyModifiers.Shift;
        if ((b & 8) != 0) mods |= KeyModifiers.Alt;
        if ((b & 16) != 0) mods |= KeyModifiers.Control;

        int rawButton = b & 0b11;
        bool wheel = (b & 64) != 0;
        bool motion = (b & 32) != 0;

        MouseEventType type;
        MouseButton button = MouseButton.None;
        if (wheel)
        {
            type = rawButton == 0 ? MouseEventType.ScrollUp : MouseEventType.ScrollDown;
        }
        else
        {
            button = rawButton switch
            {
                0 => MouseButton.Left,
                1 => MouseButton.Middle,
                2 => MouseButton.Right,
                _ => MouseButton.None,
            };
            type = motion ? MouseEventType.Move : (isUp ? MouseEventType.Up : MouseEventType.Down);
        }
        return new MouseToken(type, x - 1, y - 1, button, mods);
    }

    private static KeyModifiers DecodeModifiers(int code)
    {
        // xterm codifica: 1 = none, 2 = shift, 3 = alt, 4 = shift+alt,
        // 5 = ctrl, 6 = shift+ctrl, 7 = alt+ctrl, 8 = shift+alt+ctrl
        int bits = Math.Max(0, code - 1);
        var mods = KeyModifiers.None;
        if ((bits & 1) != 0) mods |= KeyModifiers.Shift;
        if ((bits & 2) != 0) mods |= KeyModifiers.Alt;
        if ((bits & 4) != 0) mods |= KeyModifiers.Control;
        return mods;
    }
}
