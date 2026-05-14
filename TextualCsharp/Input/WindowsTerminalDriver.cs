using System.Runtime.InteropServices;
using System.Threading.Channels;
using TextualCsharp.Messaging;

namespace TextualCsharp.Input;

/// <summary>
/// Driver de terminal para Windows. Usa <c>ReadConsoleInput</c> (Win32) para leer
/// eventos de teclado y ratón directamente como structs nativos, sin parsear
/// secuencias de escape ANSI. Esto elimina la ambigüedad entre ESC solo y el
/// inicio de secuencias multi-byte (flechas, ratón, etc.).
/// </summary>
public sealed class WindowsTerminalDriver : ITerminalDriver
{
    // ── Handles y modos de consola ────────────────────────────────────────────
    private const int  STD_OUTPUT_HANDLE = -11;
    private const int  STD_INPUT_HANDLE  = -10;
    private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
    private const uint ENABLE_PROCESSED_OUTPUT            = 0x0001;
    private const uint ENABLE_MOUSE_INPUT                 = 0x0010;
    private const uint ENABLE_WINDOW_INPUT                = 0x0008;
    private const uint ENABLE_ECHO_INPUT                  = 0x0004;
    private const uint ENABLE_LINE_INPUT                  = 0x0002;
    private const uint ENABLE_PROCESSED_INPUT             = 0x0001;
    private const uint ENABLE_EXTENDED_FLAGS              = 0x0080;
    private const uint ENABLE_QUICK_EDIT_MODE             = 0x0040;

    // ── Tipos de evento de ReadConsoleInput ───────────────────────────────────
    private const ushort KEY_EVENT   = 0x0001;
    private const ushort MOUSE_EVENT = 0x0002;

    // ── Flags de ratón ────────────────────────────────────────────────────────
    private const uint FROM_LEFT_1ST_BUTTON_PRESSED = 0x0001;
    private const uint RIGHTMOST_BUTTON_PRESSED     = 0x0002;
    private const uint FROM_LEFT_2ND_BUTTON_PRESSED = 0x0004;
    private const uint MOUSE_MOVED                  = 0x0001;
    private const uint MOUSE_WHEELED               = 0x0004;

    private readonly Channel<Message> _events = Channel.CreateUnbounded<Message>(
        new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });

    private CancellationTokenSource? _cts;
    private Task? _inputTask;
    private Task? _resizeTask;
    private IntPtr _hStdin  = IntPtr.Zero;
    private uint _originalOutputMode;
    private uint _originalInputMode;
    private bool _outputModeSaved;
    private bool _inputModeSaved;
    private int _lastWidth;
    private int _lastHeight;

    // ── Structs Win32 ─────────────────────────────────────────────────────────

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
    private struct INPUT_RECORD
    {
        [FieldOffset(0)]  public ushort EventType;
        [FieldOffset(4)]  public KEY_EVENT_RECORD KeyEvent;
        [FieldOffset(4)]  public MOUSE_EVENT_RECORD MouseEvent;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct KEY_EVENT_RECORD
    {
        public int    bKeyDown;
        public ushort wRepeatCount;
        public ushort wVirtualKeyCode;
        public ushort wVirtualScanCode;
        public char   uChar;
        public uint   dwControlKeyState;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct COORD
    {
        public short X;
        public short Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSE_EVENT_RECORD
    {
        public COORD dwMousePosition;
        public uint  dwButtonState;
        public uint  dwControlKeyState;
        public uint  dwEventFlags;
    }

    // ── Control key state bits ────────────────────────────────────────────────
    private const uint RIGHT_ALT_PRESSED  = 0x0001;
    private const uint LEFT_ALT_PRESSED   = 0x0002;
    private const uint RIGHT_CTRL_PRESSED = 0x0004;
    private const uint LEFT_CTRL_PRESSED  = 0x0008;
    private const uint SHIFT_PRESSED      = 0x0010;

    // ── Ciclo de vida ─────────────────────────────────────────────────────────

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _hStdin = GetStdHandle(STD_INPUT_HANDLE);
        EnableVirtualTerminalOutput();
        EnableMouseInput();
        _lastWidth  = SafeWidth();
        _lastHeight = SafeHeight();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _inputTask  = Task.Run(() => ReadInputLoop(_cts.Token), _cts.Token);
        _resizeTask = Task.Run(() => ResizeLoop(_cts.Token),    _cts.Token);
        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        if (_cts is null) return;
        _cts.Cancel();
        _events.Writer.TryComplete();
        try
        {
            if (_inputTask  is not null) await _inputTask.ConfigureAwait(false);
            if (_resizeTask is not null) await _resizeTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException) { }
        finally
        {
            _cts.Dispose();
            _cts        = null;
            _inputTask  = null;
            _resizeTask = null;
            RestoreMouseInput();
            RestoreVirtualTerminalOutput();
        }
    }

    public async ValueTask<Message?> ReadAsync(CancellationToken cancellationToken)
    {
        try   { return await _events.Reader.ReadAsync(cancellationToken).ConfigureAwait(false); }
        catch (ChannelClosedException)      { return null; }
        catch (OperationCanceledException)  { return null; }
    }

    public (int Width, int Height) GetSize() => (SafeWidth(), SafeHeight());

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    // ── Loop principal de input ───────────────────────────────────────────────

    private void ReadInputLoop(CancellationToken token)
    {
        // Si no estamos en Windows, caemos al fallback de Console.ReadKey
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            _hStdin == IntPtr.Zero || _hStdin == (IntPtr)(-1))
        {
            FallbackReadLoop(token);
            return;
        }

        var records = new INPUT_RECORD[16];
        try
        {
            while (!token.IsCancellationRequested)
            {
                // WaitForSingleObject con timeout de 100 ms permite comprobar
                // la cancelación sin bloquear para siempre en ReadConsoleInput.
                uint wait = WaitForSingleObject(_hStdin, 100);
                if (token.IsCancellationRequested) break;
                if (wait != 0) continue; // timeout o error, reintentamos

                if (!ReadConsoleInput(_hStdin, records, records.Length, out int count))
                    break;

                for (int i = 0; i < count; i++)
                {
                    var rec = records[i];
                    switch (rec.EventType)
                    {
                        case KEY_EVENT:
                            var km = MapKeyEvent(rec.KeyEvent);
                            if (km is not null) _events.Writer.TryWrite(km);
                            break;

                        case MOUSE_EVENT:
                            var mm = MapMouseEvent(rec.MouseEvent);
                            if (mm is not null) _events.Writer.TryWrite(mm);
                            break;
                    }
                }
            }
        }
        catch (OperationCanceledException) { }
    }

    // Fallback para no-Windows: Console.ReadKey (sin ratón)
    private void FallbackReadLoop(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                if (!Console.KeyAvailable) { Thread.Sleep(10); continue; }
                var info = Console.ReadKey(intercept: true);
                var ev   = MapConsoleKey(info);
                if (ev is not null) _events.Writer.TryWrite(ev);
            }
        }
        catch (InvalidOperationException) { }
        catch (OperationCanceledException) { }
    }

    private void ResizeLoop(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                Thread.Sleep(150);
                int w = SafeWidth(), h = SafeHeight();
                if (w != _lastWidth || h != _lastHeight)
                {
                    _lastWidth = w; _lastHeight = h;
                    _events.Writer.TryWrite(new ResizeEvent(w, h));
                }
            }
        }
        catch (OperationCanceledException) { }
    }

    // ── Mapeo de eventos Win32 → mensajes ─────────────────────────────────────

    private static KeyEvent? MapKeyEvent(KEY_EVENT_RECORD r)
    {
        // Solo procesar key-down; ignorar key-up y teclas sin carácter ni VK relevante
        if (r.bKeyDown == 0) return null;

        var mods = KeyModifiers.None;
        if ((r.dwControlKeyState & SHIFT_PRESSED) != 0)                          mods |= KeyModifiers.Shift;
        if ((r.dwControlKeyState & (LEFT_ALT_PRESSED | RIGHT_ALT_PRESSED)) != 0) mods |= KeyModifiers.Alt;
        if ((r.dwControlKeyState & (LEFT_CTRL_PRESSED | RIGHT_CTRL_PRESSED)) != 0) mods |= KeyModifiers.Control;

        Key key = (ConsoleKey)r.wVirtualKeyCode switch
        {
            ConsoleKey.UpArrow    => Key.Up,
            ConsoleKey.DownArrow  => Key.Down,
            ConsoleKey.LeftArrow  => Key.Left,
            ConsoleKey.RightArrow => Key.Right,
            ConsoleKey.Home       => Key.Home,
            ConsoleKey.End        => Key.End,
            ConsoleKey.PageUp     => Key.PageUp,
            ConsoleKey.PageDown   => Key.PageDown,
            ConsoleKey.Enter      => Key.Enter,
            ConsoleKey.Escape     => Key.Escape,
            ConsoleKey.Backspace  => Key.Backspace,
            ConsoleKey.Tab        => (mods & KeyModifiers.Shift) != 0 ? Key.BackTab : Key.Tab,
            ConsoleKey.Insert     => Key.Insert,
            ConsoleKey.Delete     => Key.Delete,
            ConsoleKey.Spacebar   => Key.Space,
            ConsoleKey.F1  => Key.F1,  ConsoleKey.F2  => Key.F2,
            ConsoleKey.F3  => Key.F3,  ConsoleKey.F4  => Key.F4,
            ConsoleKey.F5  => Key.F5,  ConsoleKey.F6  => Key.F6,
            ConsoleKey.F7  => Key.F7,  ConsoleKey.F8  => Key.F8,
            ConsoleKey.F9  => Key.F9,  ConsoleKey.F10 => Key.F10,
            ConsoleKey.F11 => Key.F11, ConsoleKey.F12 => Key.F12,
            _ => Key.Unknown,
        };

        if (key != Key.Unknown)
            return new KeyEvent(key, r.uChar, mods);

        if (r.uChar != '\0' && r.uChar >= ' ')
            return new KeyEvent(Key.Character, r.uChar, mods);

        return null;
    }

    private static MouseEvent? MapMouseEvent(MOUSE_EVENT_RECORD r)
    {
        var mods = KeyModifiers.None;
        if ((r.dwControlKeyState & SHIFT_PRESSED) != 0)                             mods |= KeyModifiers.Shift;
        if ((r.dwControlKeyState & (LEFT_ALT_PRESSED | RIGHT_ALT_PRESSED)) != 0)   mods |= KeyModifiers.Alt;
        if ((r.dwControlKeyState & (LEFT_CTRL_PRESSED | RIGHT_CTRL_PRESSED)) != 0) mods |= KeyModifiers.Control;

        int x = r.dwMousePosition.X;
        int y = r.dwMousePosition.Y;

        if ((r.dwEventFlags & MOUSE_WHEELED) != 0)
        {
            // El bit alto de dwButtonState indica dirección: 0 = arriba, negativo = abajo
            bool up = (r.dwButtonState & 0x80000000) == 0;
            var t   = up ? MouseEventType.ScrollUp : MouseEventType.ScrollDown;
            return new MouseEvent(t, x, y, MouseButton.None, mods);
        }

        if ((r.dwEventFlags & MOUSE_MOVED) != 0)
            return new MouseEvent(MouseEventType.Move, x, y, BuildButtons(r.dwButtonState), mods);

        // Click (button state change sin flags de movimiento/scroll)
        var btn = BuildButtons(r.dwButtonState);
        // dwEventFlags == 0 con botones → Down; si todos liberados → Up
        var type = btn != MouseButton.None ? MouseEventType.Down : MouseEventType.Up;
        return new MouseEvent(type, x, y, btn, mods);
    }

    private static MouseButton BuildButtons(uint state)
    {
        var b = MouseButton.None;
        if ((state & FROM_LEFT_1ST_BUTTON_PRESSED) != 0) b |= MouseButton.Left;
        if ((state & RIGHTMOST_BUTTON_PRESSED)     != 0) b |= MouseButton.Right;
        if ((state & FROM_LEFT_2ND_BUTTON_PRESSED) != 0) b |= MouseButton.Middle;
        return b;
    }

    // Fallback Console.ReadKey
    private static KeyEvent? MapConsoleKey(ConsoleKeyInfo info)
    {
        var mods = KeyModifiers.None;
        if ((info.Modifiers & ConsoleModifiers.Shift)   != 0) mods |= KeyModifiers.Shift;
        if ((info.Modifiers & ConsoleModifiers.Alt)     != 0) mods |= KeyModifiers.Alt;
        if ((info.Modifiers & ConsoleModifiers.Control) != 0) mods |= KeyModifiers.Control;

        Key key = info.Key switch
        {
            ConsoleKey.UpArrow    => Key.Up,    ConsoleKey.DownArrow  => Key.Down,
            ConsoleKey.LeftArrow  => Key.Left,  ConsoleKey.RightArrow => Key.Right,
            ConsoleKey.Home       => Key.Home,  ConsoleKey.End        => Key.End,
            ConsoleKey.PageUp     => Key.PageUp, ConsoleKey.PageDown  => Key.PageDown,
            ConsoleKey.Enter      => Key.Enter,  ConsoleKey.Escape    => Key.Escape,
            ConsoleKey.Backspace  => Key.Backspace, ConsoleKey.Tab    => (mods & KeyModifiers.Shift) != 0 ? Key.BackTab : Key.Tab,
            ConsoleKey.Insert     => Key.Insert, ConsoleKey.Delete    => Key.Delete,
            ConsoleKey.Spacebar   => Key.Space,
            ConsoleKey.F1  => Key.F1,  ConsoleKey.F2  => Key.F2,
            ConsoleKey.F3  => Key.F3,  ConsoleKey.F4  => Key.F4,
            ConsoleKey.F5  => Key.F5,  ConsoleKey.F6  => Key.F6,
            ConsoleKey.F7  => Key.F7,  ConsoleKey.F8  => Key.F8,
            ConsoleKey.F9  => Key.F9,  ConsoleKey.F10 => Key.F10,
            ConsoleKey.F11 => Key.F11, ConsoleKey.F12 => Key.F12,
            _ => Key.Unknown,
        };
        if (key != Key.Unknown) return new KeyEvent(key, info.KeyChar, mods);
        if (info.KeyChar != '\0') return new KeyEvent(Key.Character, info.KeyChar, mods);
        return null;
    }

    // ── Configuración del terminal ────────────────────────────────────────────

    private static int SafeWidth()  { try { return Console.WindowWidth;  } catch { return 80; } }
    private static int SafeHeight() { try { return Console.WindowHeight; } catch { return 25; } }

    private void EnableVirtualTerminalOutput()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;
        try
        {
            IntPtr h = GetStdHandle(STD_OUTPUT_HANDLE);
            if (h == IntPtr.Zero || h == (IntPtr)(-1)) return;
            if (!GetConsoleMode(h, out uint mode)) return;
            _originalOutputMode = mode;
            _outputModeSaved    = true;
            SetConsoleMode(h, mode | ENABLE_VIRTUAL_TERMINAL_PROCESSING | ENABLE_PROCESSED_OUTPUT);
        }
        catch (DllNotFoundException) { }
        catch (EntryPointNotFoundException) { }
    }

    private void EnableMouseInput()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;
        if (_hStdin == IntPtr.Zero || _hStdin == (IntPtr)(-1)) return;
        try
        {
            if (!GetConsoleMode(_hStdin, out uint mode)) return;
            _originalInputMode = mode;
            _inputModeSaved    = true;
            // Habilitar ratón; deshabilitar QuickEdit para que los clics no
            // seleccionen texto en lugar de enviarse como eventos al proceso.
            uint newMode = (mode & ~(ENABLE_QUICK_EDIT_MODE | ENABLE_PROCESSED_INPUT))
                           | ENABLE_MOUSE_INPUT | ENABLE_WINDOW_INPUT | ENABLE_EXTENDED_FLAGS;
            SetConsoleMode(_hStdin, newMode);
        }
        catch (DllNotFoundException) { }
        catch (EntryPointNotFoundException) { }
    }

    private void RestoreVirtualTerminalOutput()
    {
        if (!_outputModeSaved || !RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;
        try
        {
            IntPtr h = GetStdHandle(STD_OUTPUT_HANDLE);
            if (h != IntPtr.Zero && h != (IntPtr)(-1)) SetConsoleMode(h, _originalOutputMode);
        }
        catch { }
        _outputModeSaved = false;
    }

    private void RestoreMouseInput()
    {
        if (!_inputModeSaved || !RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;
        try
        {
            if (_hStdin != IntPtr.Zero && _hStdin != (IntPtr)(-1))
                SetConsoleMode(_hStdin, _originalInputMode);
        }
        catch { }
        _inputModeSaved = false;
    }

    // ── P/Invoke ──────────────────────────────────────────────────────────────

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetConsoleMode(IntPtr handle, out uint mode);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetConsoleMode(IntPtr handle, uint mode);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ReadConsoleInput(IntPtr hConsoleInput,
        [Out] INPUT_RECORD[] lpBuffer, int nLength, out int lpNumberOfEventsRead);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);
}
