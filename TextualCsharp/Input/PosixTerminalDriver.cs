using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;
using TextualCsharp.Messaging;

namespace TextualCsharp.Input;

/// <summary>
/// Driver de terminal para sistemas POSIX (Linux, macOS, *BSD). Pone el TTY en
/// modo crudo vía <c>termios</c>, habilita el reporte de ratón en formato SGR
/// y delega el parseo de secuencias a <see cref="XTermParser"/>.
///
/// <para>
/// Funciona igual de bien en sesiones SSH que en terminal local: la única
/// diferencia es que el reporte de ratón puede no estar soportado por algunos
/// clientes SSH (en cuyo caso simplemente no llegan eventos de ratón). Las
/// secuencias ANSI emitidas son estrictamente compatibles con xterm/VT.
/// </para>
/// </summary>
public sealed class PosixTerminalDriver : ITerminalDriver
{
    private const int  STDIN_FILENO  = 0;
    private const int  STDOUT_FILENO = 1;
    private const int  TCSANOW       = 0;

    // Bits de c_lflag (Linux glibc — coinciden en macOS/BSD para las máscaras usadas).
    private const uint ICANON = 0x00000002;
    private const uint ECHO   = 0x00000008;
    private const uint ISIG   = 0x00000001;
    private const uint IEXTEN = 0x00008000;
    // c_iflag
    private const uint IXON   = 0x00000400;
    private const uint ICRNL  = 0x00000100;
    private const uint BRKINT = 0x00000002;
    private const uint INPCK  = 0x00000010;
    private const uint ISTRIP = 0x00000020;
    // c_oflag
    private const uint OPOST  = 0x00000001;

    // Estructura termios — usamos tamaño suficientemente grande para Linux/macOS y
    // sólo accedemos a c_iflag/c_oflag/c_lflag por offset conocido. Se reservan
    // 128 bytes para cubrir el resto (c_cc[NCCS], speeds, …) en ambas libc.
    [StructLayout(LayoutKind.Sequential, Size = 144)]
    private struct Termios
    {
        public uint c_iflag;
        public uint c_oflag;
        public uint c_cflag;
        public uint c_lflag;
    }

    [DllImport("libc")] private static extern int tcgetattr(int fd, out Termios termios);
    [DllImport("libc")] private static extern int tcsetattr(int fd, int optional_actions, ref Termios termios);
    [DllImport("libc")] private static extern int isatty(int fd);
    [DllImport("libc", EntryPoint = "read")] private static extern IntPtr libc_read(int fd, byte[] buf, UIntPtr count);

    [StructLayout(LayoutKind.Sequential)]
    private struct PollFd { public int fd; public short events; public short revents; }
    [DllImport("libc")] private static extern int poll([In, Out] PollFd[] fds, uint nfds, int timeout);

    // ── Win32: deshabilitar QuickEdit cuando se corre en conhost (p.e. SSH desde PowerShell) ──
    private const int  STD_INPUT_HANDLE     = -10;
    private const uint ENABLE_EXTENDED_FLAGS = 0x0080;
    private const uint ENABLE_QUICK_EDIT_MODE = 0x0040;
    private const uint ENABLE_MOUSE_INPUT    = 0x0010;
    private const uint ENABLE_WINDOW_INPUT   = 0x0008;
    private const uint ENABLE_PROCESSED_INPUT = 0x0001;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    private IntPtr _hStdinWin  = IntPtr.Zero;
    private uint   _originalWinInputMode;
    private bool   _winInputModeSaved;

    private readonly Channel<Message> _events = Channel.CreateUnbounded<Message>(
        new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });
    private readonly XTermParser _parser = new();
    private readonly bool _enableMouse;

    private CancellationTokenSource? _cts;
    private Task? _inputTask;
    private Task? _resizeTask;
    private Termios _original;
    private bool _termiosSaved;
    private int _lastWidth;
    private int _lastHeight;

    public PosixTerminalDriver(bool enableMouse = true)
    {
        _enableMouse = enableMouse;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        EnterRawMode();
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
            _cts = null;
            _inputTask = null;
            _resizeTask = null;
            RestoreTermios();
            RestoreWinInputMode();
        }
    }

    public async ValueTask<Message?> ReadAsync(CancellationToken cancellationToken)
    {
        try { return await _events.Reader.ReadAsync(cancellationToken).ConfigureAwait(false); }
        catch (ChannelClosedException) { return null; }
        catch (OperationCanceledException) { return null; }
    }

    public (int Width, int Height) GetSize() => (SafeWidth(), SafeHeight());

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    // ── Loop de input ─────────────────────────────────────────────────────────

    private void ReadInputLoop(CancellationToken token)
    {
        bool canRawRead = _termiosSaved && SafeIsatty(STDIN_FILENO);
        if (!canRawRead)
        {
            FallbackReadLoop(token);
            return;
        }

        var buf = new byte[1024];
        var decoder = Encoding.UTF8.GetDecoder();
        var chars = new char[2048];
        try
        {
            while (!token.IsCancellationRequested)
            {
                IntPtr n;
                try { n = libc_read(STDIN_FILENO, buf, (UIntPtr)buf.Length); }
                catch (DllNotFoundException) { FallbackReadLoop(token); return; }

                int bytesRead = (int)n;
                if (bytesRead <= 0)
                {
                    Thread.Sleep(10);
                    continue;
                }

                int chCount = decoder.GetChars(buf, 0, bytesRead, chars, 0, flush: false);
                if (chCount == 0) continue;

                foreach (var token2 in _parser.Parse(new ReadOnlyMemory<char>(chars, 0, chCount)))
                {
                    switch (token2)
                    {
                        case XTermParser.KeyToken k:
                            _events.Writer.TryWrite(new KeyEvent(k.Key, k.Character, k.Modifiers));
                            break;
                        case XTermParser.MouseToken m:
                            _events.Writer.TryWrite(new MouseEvent(m.Type, m.X, m.Y, m.Button, m.Modifiers));
                            break;
                    }
                }
            }
        }
        catch (OperationCanceledException) { }
    }

    private void FallbackReadLoop(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                if (!Console.KeyAvailable) { Thread.Sleep(15); continue; }
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
            ConsoleKey.Backspace  => Key.Backspace,
            ConsoleKey.Tab        => (mods & KeyModifiers.Shift) != 0 ? Key.BackTab : Key.Tab,
            ConsoleKey.Insert     => Key.Insert, ConsoleKey.Delete    => Key.Delete,
            ConsoleKey.Spacebar   => Key.Space,
            ConsoleKey.F1 => Key.F1,  ConsoleKey.F2  => Key.F2,
            ConsoleKey.F3 => Key.F3,  ConsoleKey.F4  => Key.F4,
            ConsoleKey.F5 => Key.F5,  ConsoleKey.F6  => Key.F6,
            ConsoleKey.F7 => Key.F7,  ConsoleKey.F8  => Key.F8,
            ConsoleKey.F9 => Key.F9,  ConsoleKey.F10 => Key.F10,
            ConsoleKey.F11 => Key.F11, ConsoleKey.F12 => Key.F12,
            _ => Key.Unknown,
        };
        if (key != Key.Unknown) return new KeyEvent(key, info.KeyChar, mods);
        if (info.KeyChar != '\0') return new KeyEvent(Key.Character, info.KeyChar, mods);
        return null;
    }

    // ── termios / mouse ───────────────────────────────────────────────────────

    /// <summary>
    /// En Windows (conhost / PowerShell), deshabilita QuickEdit para que los clics
    /// del ratón no pausen la salida del proceso. También habilita MOUSE_INPUT y
    /// WINDOW_INPUT para que los eventos lleguen correctamente al PTY de OpenSSH.
    /// No hace nada en POSIX.
    /// </summary>
    private void DisableWinQuickEdit()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;
        try
        {
            _hStdinWin = GetStdHandle(STD_INPUT_HANDLE);
            if (_hStdinWin == IntPtr.Zero || _hStdinWin == (IntPtr)(-1)) return;
            if (!GetConsoleMode(_hStdinWin, out uint mode)) return;
            _originalWinInputMode = mode;
            _winInputModeSaved    = true;
            // Quitar QuickEdit; mantener PROCESSED_INPUT; añadir MOUSE e WINDOW.
            uint newMode = (mode & ~(ENABLE_QUICK_EDIT_MODE | ENABLE_PROCESSED_INPUT))
                           | ENABLE_MOUSE_INPUT | ENABLE_WINDOW_INPUT | ENABLE_EXTENDED_FLAGS;
            SetConsoleMode(_hStdinWin, newMode);
        }
        catch (DllNotFoundException) { }
        catch (EntryPointNotFoundException) { }
    }

    private void RestoreWinInputMode()
    {
        if (!_winInputModeSaved) return;
        try
        {
            if (_hStdinWin != IntPtr.Zero && _hStdinWin != (IntPtr)(-1))
                SetConsoleMode(_hStdinWin, _originalWinInputMode);
        }
        catch { }
        _winInputModeSaved = false;
    }

    private void EnterRawMode()
    {
        if (!TerminalEnvironment.IsPosix) return;
        try
        {
            if (tcgetattr(STDIN_FILENO, out var t) != 0) return;
            _original = t;
            _termiosSaved = true;
            t.c_iflag &= ~(IXON | ICRNL | BRKINT | INPCK | ISTRIP);
            t.c_oflag &= ~OPOST;
            t.c_lflag &= ~(ICANON | ECHO | ISIG | IEXTEN);
            tcsetattr(STDIN_FILENO, TCSANOW, ref t);
        }
        catch (DllNotFoundException) { _termiosSaved = false; }
        catch (EntryPointNotFoundException) { _termiosSaved = false; }
    }

    private void RestoreTermios()
    {
        if (!_termiosSaved) return;
        try
        {
            var t = _original;
            tcsetattr(STDIN_FILENO, TCSANOW, ref t);
        }
        catch { }
        _termiosSaved = false;
    }

    /// <summary>
    /// Activa el reporte de ratón en el terminal remoto, escribiendo directamente
    /// al handle de stdout sin pasar por el buffer del <see cref="TextWriter"/>.
    /// <para>
    /// Solo ?1000h (button events) + ?1006h (SGR encoding). Se omite ?1003h
    /// (all-motion tracking) deliberadamente: en SSH genera un evento por cada
    /// píxel de movimiento y satura el pipe, bloqueando la salida.
    /// </para>
    /// <para>
    /// ?1000h también pide al conhost/Windows Terminal local del cliente SSH que
    /// suprima QuickEdit y enrute los clicks como secuencias VT. <b>Importante:</b>
    /// algunos clientes (conhost legacy) ignoran ?1000h y mantienen QuickEdit;
    /// en ese caso el usuario debe desactivar QuickEdit manualmente en las
    /// propiedades de la ventana del cliente, o usar Windows Terminal.
    /// </para>
    /// </summary>
    internal static void EnableMouseReporting()
    {
        WriteRawAnsi("\u001b[?1000h\u001b[?1006h");
    }

    internal static void DisableMouseReporting()
    {
        WriteRawAnsi("\u001b[?1006l\u001b[?1000l");
    }

    /// <summary>
    /// Escribe una secuencia ANSI directamente al stream nativo de stdout,
    /// evitando el buffer del <see cref="Console.Out"/> TextWriter. Garantiza
    /// que el cliente reciba la secuencia inmediatamente, lo cual es crítico
    /// para ?1000h en SSH antes del primer clic.
    /// </summary>
    internal static void WriteRawAnsi(string ansi)
    {
        try
        {
            var bytes = Encoding.UTF8.GetBytes(ansi);
            using var stdout = Console.OpenStandardOutput();
            stdout.Write(bytes, 0, bytes.Length);
            stdout.Flush();
        }
        catch { }
    }

    /// <summary>
    /// Sondea si el terminal remoto responde a la secuencia DA1 (Device Attributes,
    /// <c>ESC[c</c>). Los terminales que realmente procesan VT responden con
    /// <c>ESC[?…c</c> en menos de <paramref name="timeoutMs"/> ms.
    /// Los conhost legacy que ignoran el ratón VT tampoco responden a DA1.
    /// <para>
    /// Requiere que el driver ya esté en modo crudo (<see cref="EnterRawMode"/>)
    /// para que la respuesta llegue byte a byte sin bloquear stdin.
    /// </para>
    /// </summary>
    /// <returns>
    /// <see langword="true"/> si se recibió respuesta VT; <see langword="false"/>
    /// si no se recibió nada dentro del timeout (cliente legacy/QuickEdit).
    /// </returns>
    internal static bool ProbeVtCapabilities(int timeoutMs = 150)
    {
        if (!TerminalEnvironment.IsPosix) return false;
        try
        {
            // Enviar DA1 directamente, sin buffer.
            WriteRawAnsi("\u001b[c");

            // Esperamos hasta timeoutMs ms a que llegue el primer byte de respuesta.
            var fds = new PollFd[] { new PollFd { fd = STDIN_FILENO, events = 0x0001 /* POLLIN */ } };
            int ready = poll(fds, 1, timeoutMs);
            if (ready <= 0) return false; // timeout o error → terminal legacy

            // Consumir (y descartar) los bytes de la respuesta para no contaminar
            // el parser de input. La respuesta típica es ESC [ ? 6 2 ; … c  (~8 bytes).
            var buf = new byte[32];
            var deadline = Environment.TickCount64 + timeoutMs;
            while (Environment.TickCount64 < deadline)
            {
                var pf = new PollFd[] { new PollFd { fd = STDIN_FILENO, events = 0x0001 } };
                if (poll(pf, 1, 20) <= 0) break;
                libc_read(STDIN_FILENO, buf, (UIntPtr)buf.Length);
            }
            return true;
        }
        catch { return false; }
    }

    private static bool SafeIsatty(int fd)
    {
        try { return isatty(fd) == 1; }
        catch (DllNotFoundException) { return false; }
        catch (EntryPointNotFoundException) { return false; }
    }

    private static int SafeWidth()  { try { return Console.WindowWidth;  } catch { return 80; } }
    private static int SafeHeight() { try { return Console.WindowHeight; } catch { return 25; } }
}
