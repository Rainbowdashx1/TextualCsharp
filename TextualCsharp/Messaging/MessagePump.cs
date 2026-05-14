using System.Threading.Channels;

namespace TextualCsharp.Messaging;

/// <summary>
/// Bus de mensajes asíncrono basado en <see cref="Channel{T}"/>. Equivalente a
/// <c>textual.message_pump.MessagePump</c>. Cada nodo del árbol DOM hereda de aquí
/// y procesa mensajes serializadamente en su propio loop.
/// </summary>
public abstract class MessagePump : IAsyncDisposable
{
    private readonly Channel<Message> _channel;
    private Task? _pumpTask;
    private CancellationTokenSource? _cts;

    protected MessagePump()
    {
        _channel = Channel.CreateUnbounded<Message>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
        });
    }

    /// <summary>Indica si el pump está corriendo (entre <see cref="StartAsync"/> y <see cref="StopAsync"/>).</summary>
    public bool IsRunning => _pumpTask is { IsCompleted: false };

    /// <summary>Encola un mensaje para ser procesado en el loop.</summary>
    public ValueTask PostAsync(Message message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        return _channel.Writer.WriteAsync(message, cancellationToken);
    }

    /// <summary>Intenta encolar de forma síncrona (no bloqueante).</summary>
    public bool TryPost(Message message)
    {
        ArgumentNullException.ThrowIfNull(message);
        return _channel.Writer.TryWrite(message);
    }

    /// <summary>Arranca el loop de procesamiento.</summary>
    public Task StartAsync()
    {
        if (IsRunning) return Task.CompletedTask;
        _cts = new CancellationTokenSource();
        _pumpTask = Task.Run(() => RunAsync(_cts.Token));
        return Task.CompletedTask;
    }

    /// <summary>Detiene el loop y espera a que termine de drenar los mensajes pendientes.</summary>
    public async Task StopAsync()
    {
        if (_pumpTask is null) return;
        _channel.Writer.TryComplete();
        try
        {
            await _pumpTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // expected on cancel
        }
        _cts?.Dispose();
        _cts = null;
        _pumpTask = null;
    }

    private async Task RunAsync(CancellationToken token)
    {
        var reader = _channel.Reader;
        try
        {
            while (await reader.WaitToReadAsync(token).ConfigureAwait(false))
            {
                while (reader.TryRead(out var message))
                {
                    try
                    {
                        await OnMessageAsync(message).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        OnError(message, ex);
                    }
                }
            }
        }
        catch (OperationCanceledException) { /* graceful stop */ }
    }

    /// <summary>Hook principal: procesa un mensaje. Sobrescribir en subclases.</summary>
    protected virtual ValueTask OnMessageAsync(Message message) => ValueTask.CompletedTask;

    /// <summary>Hook para reportar errores en el procesamiento (por defecto no hace nada).</summary>
    protected virtual void OnError(Message message, Exception exception) { }

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }
}
