using TextualCsharp.App;
using TextualCsharp.Input;
using TextualCsharp.Messaging;
using TextualCsharp.Rendering;

namespace TextualCsharp.Testing;

/// <summary>
/// "Pilot" para testing automatizado. Equivalente a <c>textual.pilot.Pilot</c>.
/// Conecta un <see cref="ConsoleApp"/> con un <see cref="HeadlessTerminalDriver"/>
/// y un <see cref="MockRenderer"/>, expone helpers para inyectar input y leer
/// el buffer resultante.
/// </summary>
public sealed class AppPilot : IAsyncDisposable
{
    private readonly ConsoleApp _app;
    private readonly HeadlessTerminalDriver _driver;
    private readonly MockRenderer _renderer;
    private readonly CancellationTokenSource _cts = new();
    private Task<int>? _runTask;

    public AppPilot(ConsoleApp app, int width = 100, int height = 30)
    {
        _app = app ?? throw new ArgumentNullException(nameof(app));
        _driver = new HeadlessTerminalDriver(width, height);
        _renderer = new MockRenderer();
        _app.Driver = _driver;
        _app.Renderer = _renderer;
    }

    public ConsoleApp App => _app;
    public MockRenderer Renderer => _renderer;
    public HeadlessTerminalDriver Driver => _driver;

    /// <summary>Arranca el event loop en segundo plano.</summary>
    public Task StartAsync()
    {
        _runTask = _app.RunAsync(_cts.Token);
        return Task.CompletedTask;
    }

    /// <summary>Inyecta una pulsación de tecla y espera a que se procese.</summary>
    public async Task PressAsync(Key key, char ch = '\0', KeyModifiers mods = KeyModifiers.None)
    {
        _driver.Inject(new KeyEvent(key, ch, mods));
        await PumpAsync().ConfigureAwait(false);
    }

    /// <summary>Inyecta cada carácter de <paramref name="text"/> como Character events.</summary>
    public async Task TypeAsync(string text)
    {
        foreach (var c in text)
        {
            _driver.Inject(new KeyEvent(Key.Character, c, KeyModifiers.None));
            await PumpAsync().ConfigureAwait(false);
        }
    }

    /// <summary>Simula resize del terminal.</summary>
    public async Task ResizeAsync(int width, int height)
    {
        _driver.Resize(width, height);
        await PumpAsync().ConfigureAwait(false);
    }

    /// <summary>Espera el tiempo dado para que se procesen eventos asíncronos.</summary>
    public Task PumpAsync(int milliseconds = 25) => Task.Delay(milliseconds);

    /// <summary>Solicita la salida limpia y espera al fin del loop.</summary>
    public async Task<int> StopAsync()
    {
        _app.Exit();
        _driver.Inject(new QuitEvent());
        if (_runTask is null) return 0;
        try { return await _runTask.ConfigureAwait(false); }
        catch (OperationCanceledException) { return 0; }
    }

    public async ValueTask DisposeAsync()
    {
        try { await StopAsync().ConfigureAwait(false); } catch { }
        _cts.Cancel();
        _cts.Dispose();
    }
}
