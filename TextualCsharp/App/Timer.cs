namespace TextualCsharp.App;

/// <summary>
/// Timer periódico simple basado en <see cref="PeriodicTimer"/>. Equivalente a
/// <c>textual.timer.Timer</c>.
/// </summary>
public sealed class Timer : IAsyncDisposable
{
    private readonly TimeSpan _interval;
    private readonly Func<CancellationToken, ValueTask> _callback;
    private CancellationTokenSource? _cts;
    private Task? _runner;

    public Timer(TimeSpan interval, Func<CancellationToken, ValueTask> callback)
    {
        if (interval <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(interval));
        _interval = interval;
        _callback = callback ?? throw new ArgumentNullException(nameof(callback));
    }

    public bool IsRunning => _runner is { IsCompleted: false };

    public void Start()
    {
        if (IsRunning) return;
        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        _runner = Task.Run(async () =>
        {
            using var timer = new PeriodicTimer(_interval);
            try
            {
                while (await timer.WaitForNextTickAsync(token).ConfigureAwait(false))
                    await _callback(token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) { }
        }, token);
    }

    public async Task StopAsync()
    {
        if (_cts is null) return;
        _cts.Cancel();
        try
        {
            if (_runner is not null) await _runner.ConfigureAwait(false);
        }
        catch (OperationCanceledException) { }
        _cts.Dispose();
        _cts = null;
        _runner = null;
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }
}
