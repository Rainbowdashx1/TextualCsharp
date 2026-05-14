using System.Threading.Channels;
using TextualCsharp.Messaging;

namespace TextualCsharp.Input;

/// <summary>
/// Driver de terminal "fake" para testing/scripting (sin terminal real).
/// El pilot inyecta mensajes vía <see cref="Inject"/>. El tamaño es configurable.
/// </summary>
public sealed class HeadlessTerminalDriver : ITerminalDriver
{
    private readonly Channel<Message> _events = Channel.CreateUnbounded<Message>();
    private int _width;
    private int _height;

    public HeadlessTerminalDriver(int width = 100, int height = 30)
    {
        _width = width;
        _height = height;
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync()
    {
        _events.Writer.TryComplete();
        return Task.CompletedTask;
    }

    public async ValueTask<Message?> ReadAsync(CancellationToken cancellationToken)
    {
        try { return await _events.Reader.ReadAsync(cancellationToken).ConfigureAwait(false); }
        catch (ChannelClosedException) { return null; }
        catch (OperationCanceledException) { return null; }
    }

    public (int Width, int Height) GetSize() => (_width, _height);

    public void Resize(int width, int height)
    {
        _width = width;
        _height = height;
        _events.Writer.TryWrite(new ResizeEvent(width, height));
    }

    public void Inject(Message message) => _events.Writer.TryWrite(message);

    public ValueTask DisposeAsync()
    {
        _events.Writer.TryComplete();
        return ValueTask.CompletedTask;
    }
}
