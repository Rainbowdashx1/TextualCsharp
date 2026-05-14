using System.Threading.Channels;
using TextualCsharp.Animation;
using TextualCsharp.Core.Geometry;
using TextualCsharp.Input;
using TextualCsharp.Messaging;
using TextualCsharp.Rendering;
using TextualCsharp.Theming;
using TextualCsharp.Widgets;

namespace TextualCsharp.App;

/// <summary>Notificación interna: refrescar el frame (timers, workers, etc.).</summary>
public sealed record FrameTick : Message
{
    public override bool Bubble => false;
}

/// <summary>
/// Entry point de una aplicación TUI. Equivalente a <c>textual.app.App</c>.
/// Coordina el driver, el stack de pantallas, el event loop principal y el render.
/// </summary>
public class ConsoleApp : IAsyncDisposable
{
    private readonly Stack<Screen> _screens = new();
    private readonly List<Timer> _timers = new();
    private readonly Channel<Message> _inbox = Channel.CreateUnbounded<Message>(
        new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });
    private ITerminalDriver? _driver;
    private IRenderer? _renderer;
    private AnsiWriter? _ansiWriter;
    private ConsoleBuffer? _current;
    private ConsoleBuffer? _previous;
    private CancellationTokenSource? _appCts;
    private Task? _driverPump;
    private bool _exitRequested;
    private int _actionsHandled;
    private Theme _theme = ThemeProvider.Current;

    /// <summary>Pantalla activa (top del stack), o null si no hay ninguna.</summary>
    public Screen? ActiveScreen => _screens.Count > 0 ? _screens.Peek() : null;

    /// <summary>Driver del terminal en uso (solo válido mientras corre).</summary>
    public ITerminalDriver? Driver
    {
        get => _driver;
        set => _driver = value;
    }

    /// <summary>Renderer (sink) en uso. Si es null, se usa <see cref="AnsiWriter"/>.</summary>
    public IRenderer? Renderer
    {
        get => _renderer;
        set => _renderer = value;
    }

    /// <summary>Motor de animaciones de la app.</summary>
    public Animator Animator { get; } = new();

    /// <summary>Buffer actual (último frame compuesto). Útil para tests headless.</summary>
    public ConsoleBuffer? CurrentBuffer => _current;

    /// <summary>Tema activo. Cambiarlo dispara <see cref="ThemeChanged"/> e invalida.</summary>
    public Theme Theme
    {
        get => _theme;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (_theme == value) return;
            _theme = value;
            ThemeProvider.Current = value;
            ThemeChanged?.Invoke(value);
            Invalidate();
            Post(new FrameTick());
        }
    }

    /// <summary>Notificado cuando el tema activo cambia (hot-reload).</summary>
    public event Action<Theme>? ThemeChanged;

    /// <summary>Bindings globales (se consultan después de los de la pantalla activa).</summary>
    public BindingMap GlobalBindings { get; } = new();

    /// <summary>Hook que ejecuta una acción declarada por un binding.</summary>
    public event Action<string>? ActionInvoked;

    /// <summary>Apila una pantalla nueva como activa (modal push).</summary>
    public async Task PushScreenAsync(Screen screen)
    {
        ArgumentNullException.ThrowIfNull(screen);
        _screens.Push(screen);
        await screen.MountAsync().ConfigureAwait(false);
        Layout();
        Invalidate();
    }

    /// <summary>Saca la pantalla actual del stack y la desmonta.</summary>
    public async Task PopScreenAsync()
    {
        if (_screens.Count == 0) return;
        var top = _screens.Pop();
        await top.UnmountAsync().ConfigureAwait(false);
        Layout();
        Invalidate();
    }

    /// <summary>Encola un mensaje para que el loop lo procese en el próximo turno.</summary>
    public void Post(Message message)
    {
        ArgumentNullException.ThrowIfNull(message);
        _inbox.Writer.TryWrite(message);
    }

    /// <summary>Registra un timer periódico (se inicia automáticamente al ejecutar la app).</summary>
    public Timer AddTimer(TimeSpan interval, Func<CancellationToken, ValueTask> callback)
    {
        var t = new Timer(interval, async ct =>
        {
            await callback(ct).ConfigureAwait(false);
            Post(new FrameTick());
        });
        _timers.Add(t);
        if (_appCts is not null) t.Start();
        return t;
    }

    /// <summary>Solicita el cierre limpio de la app.</summary>
    public void Exit() => _exitRequested = true;

    /// <summary>
    /// Ejecuta el event loop principal. Bloquea hasta que se llame a <see cref="Exit"/>
    /// o se cancele el token. Devuelve el número de acciones ejecutadas (útil para tests).
    /// </summary>
    public async Task<int> RunAsync(CancellationToken cancellationToken = default)
    {
        if (_screens.Count == 0)
            throw new InvalidOperationException("No screen pushed. Call PushScreenAsync before RunAsync.");

        _driver ??= new WindowsTerminalDriver();
        if (_renderer is null)
        {
            _ansiWriter = new AnsiWriter();
            _renderer = _ansiWriter;
        }
        _appCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        await _driver.StartAsync(_appCts.Token).ConfigureAwait(false);
        _driverPump = Task.Run(() => PumpDriverAsync(_appCts.Token));
        foreach (var t in _timers) t.Start();

        // Animation pump: ~30 fps mientras haya animaciones activas.
        var animTimer = new Timer(TimeSpan.FromMilliseconds(33), _ =>
        {
            if (Animator.ActiveCount > 0)
            {
                Animator.Tick();
                Post(new FrameTick());
            }
            return ValueTask.CompletedTask;
        });
        animTimer.Start();

        try
        {
            _ansiWriter?.EnterAltScreen();
            _ansiWriter?.HideCursor();
            _renderer.Clear();
            _renderer.Flush();
            (int w, int h) = _driver.GetSize();
            ResizeBuffers(w, h);
            Layout();
            RenderFrame(fullRedraw: true);

            while (!_exitRequested && !_appCts.IsCancellationRequested)
            {
                Message? msg;
                try
                {
                    msg = await _inbox.Reader.ReadAsync(_appCts.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) { break; }
                catch (ChannelClosedException) { break; }
                if (msg is null) break;

                await HandleMessageAsync(msg).ConfigureAwait(false);
                // Drena el resto de mensajes encolados antes de renderizar.
                while (_inbox.Reader.TryRead(out var extra))
                    await HandleMessageAsync(extra).ConfigureAwait(false);

                RenderFrame(fullRedraw: false);
            }
        }
        finally
        {
            await animTimer.DisposeAsync().ConfigureAwait(false);
            foreach (var t in _timers) await t.DisposeAsync().ConfigureAwait(false);
            _timers.Clear();
            _appCts.Cancel();
            await _driver.StopAsync().ConfigureAwait(false);
            _inbox.Writer.TryComplete();
            if (_driverPump is not null)
            {
                try { await _driverPump.ConfigureAwait(false); }
                catch (OperationCanceledException) { }
            }
            _renderer?.Clear();
            _ansiWriter?.ShowCursor();
            _ansiWriter?.LeaveAltScreen();
            _renderer?.Flush();
            _appCts.Dispose();
            _appCts = null;
            _driverPump = null;
        }
        return _actionsHandled;
    }

    private async Task PumpDriverAsync(CancellationToken token)
    {
        if (_driver is null) return;
        try
        {
            while (!token.IsCancellationRequested)
            {
                var msg = await _driver.ReadAsync(token).ConfigureAwait(false);
                if (msg is null) break;
                _inbox.Writer.TryWrite(msg);
            }
        }
        catch (OperationCanceledException) { }
    }

    private async ValueTask HandleMessageAsync(Message message)
    {
        switch (message)
        {
            case ResizeEvent r:
                ResizeBuffers(r.Width, r.Height);
                Layout();
                RenderFrame(fullRedraw: true);
                return;

            case KeyEvent k:
                if (ActiveScreen is { } scr)
                {
                    string? action = scr.DispatchKey(k);
                    if (action is null && GlobalBindings.TryResolve(k, out var gb))
                        action = gb.Action;
                    if (action is not null)
                    {
                        _actionsHandled++;
                        ActionInvoked?.Invoke(action);
                        if (string.Equals(action, "quit", StringComparison.OrdinalIgnoreCase))
                            Exit();
                    }
                }
                return;

            case MouseEvent m:
                if (ActiveScreen is { } scr2)
                {
                    var hit = scr2.HitTest(m.X, m.Y);
                    if (hit is not null)
                    {
                        if (m.Type == MouseEventType.Down)
                        {
                            // Mover el foco al widget focusable más cercano bajo el cursor.
                            var focusTarget = scr2.HitTestFocusable(m.X, m.Y);
                            if (focusTarget is not null)
                                scr2.Focus.SetFocus(focusTarget);
                        }
                        // Intentar despachar al widget más profundo primero.
                        // Si no lo consume (devuelve false), despachar al focusable.
                        if (!hit.HandleMouse(m))
                        {
                            var focusable = scr2.HitTestFocusable(m.X, m.Y);
                            focusable?.HandleMouse(m);
                        }
                        hit.TryPost(m);
                    }
                }
                return;

            case QuitEvent:
                Exit();
                return;

            case FrameTick:
                // No-op: solo provoca un re-render en el loop principal.
                return;
        }

        await ValueTask.CompletedTask.ConfigureAwait(false);
    }

    private void ResizeBuffers(int width, int height)
    {
        width = Math.Max(1, width);
        height = Math.Max(1, height);
        if (_current is null || _current.Width != width || _current.Height != height)
        {
            _current = new ConsoleBuffer(width, height);
            _previous = new ConsoleBuffer(width, height);
        }
    }

    private void Layout()
    {
        if (_current is null) return;
        var region = new Region(0, 0, _current.Width, _current.Height);
        foreach (var screen in _screens.Reverse())
            screen.Arrange(region);
    }

    private void Invalidate()
    {
        // Marca el buffer previo como "vacío" para forzar un full redraw en el próximo frame.
        _previous = null;
        if (_current is not null)
        {
            int w = _current.Width;
            int h = _current.Height;
            _previous = new ConsoleBuffer(w, h);
            // Rellena con celda inválida (glifo nulo) para garantizar diff total.
            _previous.Fill(new Core.ConsoleCell('\0', Core.Color.Default, Core.Color.Default, Core.StyleFlags.None));
        }
    }

    private void RenderFrame(bool fullRedraw)
    {
        if (_current is null || _renderer is null) return;
        // Re-layout antes de cada frame para que cambios dinámicos (cambio de
        // pestaña, push de modal, etc.) propaguen Region a todos los descendientes.
        Layout();
        _current.Clear();
        foreach (var screen in _screens.Reverse())
            screen.Paint(_current);

        var prev = fullRedraw ? null : _previous;
        var changes = DiffRenderer.Diff(prev, _current);
        if (changes.Count > 0)
        {
            _renderer.Apply(changes);
            _renderer.Flush();
        }
        if (_previous is null || _previous.Width != _current.Width || _previous.Height != _current.Height)
            _previous = new ConsoleBuffer(_current.Width, _current.Height);
        _current.CopyTo(_previous);
    }

    /// <summary>Fuerza un render inmediato (útil para tests/pilot).</summary>
    public void ForceRender()
    {
        if (_current is null) return;
        Layout();
        RenderFrame(fullRedraw: true);
    }

    public async ValueTask DisposeAsync()
    {
        Exit();
        if (_driver is not null)
            await _driver.DisposeAsync().ConfigureAwait(false);
        foreach (var screen in _screens)
            await screen.DisposeAsync().ConfigureAwait(false);
        _screens.Clear();
        GC.SuppressFinalize(this);
    }
}
