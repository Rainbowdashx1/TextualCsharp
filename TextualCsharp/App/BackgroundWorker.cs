namespace TextualCsharp.App;

/// <summary>Estado de un <see cref="BackgroundWorker{T}"/>.</summary>
public enum WorkerState
{
    Pending,
    Running,
    Completed,
    Failed,
    Cancelled,
}

/// <summary>
/// Tarea asíncrona en segundo plano con reporte de progreso. Equivalente a
/// <c>textual.worker.Worker</c>.
/// </summary>
public sealed class BackgroundWorker<T> : IAsyncDisposable
{
    private readonly Func<IProgress<T>, CancellationToken, Task> _work;
    private readonly CancellationTokenSource _cts = new();
    private Task? _task;

    public BackgroundWorker(Func<IProgress<T>, CancellationToken, Task> work, string? name = null)
    {
        _work = work ?? throw new ArgumentNullException(nameof(work));
        Name = name ?? "worker";
    }

    public string Name { get; }
    public WorkerState State { get; private set; } = WorkerState.Pending;
    public T? LastProgress { get; private set; }
    public Exception? Error { get; private set; }

    public event Action<T>? ProgressChanged;
    public event Action<WorkerState>? StateChanged;

    public Task Start()
    {
        if (_task is not null) return _task;
        State = WorkerState.Running;
        StateChanged?.Invoke(State);

        var progress = new Progress<T>(value =>
        {
            LastProgress = value;
            ProgressChanged?.Invoke(value);
        });

        _task = Task.Run(async () =>
        {
            try
            {
                await _work(progress, _cts.Token).ConfigureAwait(false);
                State = WorkerState.Completed;
            }
            catch (OperationCanceledException)
            {
                State = WorkerState.Cancelled;
            }
            catch (Exception ex)
            {
                Error = ex;
                State = WorkerState.Failed;
            }
            finally
            {
                StateChanged?.Invoke(State);
            }
        }, _cts.Token);

        return _task;
    }

    public void Cancel() => _cts.Cancel();

    public async ValueTask DisposeAsync()
    {
        Cancel();
        try
        {
            if (_task is not null) await _task.ConfigureAwait(false);
        }
        catch { /* ignored */ }
        _cts.Dispose();
        GC.SuppressFinalize(this);
    }
}
