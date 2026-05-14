namespace TextualCsharp.Reactive;

/// <summary>
/// Señal pub/sub desacoplada. Equivalente a <c>textual.signal.Signal</c>.
/// Permite comunicar widgets sin acoplarlos directamente.
/// </summary>
public sealed class Signal<T>
{
    private readonly List<Action<T>> _subscribers = new();

    public IDisposable Subscribe(Action<T> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        _subscribers.Add(callback);
        return new Subscription(() => _subscribers.Remove(callback));
    }

    public void Publish(T value)
    {
        for (int i = 0; i < _subscribers.Count; i++)
        {
            try { _subscribers[i](value); } catch { /* ignored */ }
        }
    }

    public int SubscriberCount => _subscribers.Count;

    private sealed class Subscription : IDisposable
    {
        private Action? _onDispose;
        public Subscription(Action onDispose) { _onDispose = onDispose; }
        public void Dispose() { _onDispose?.Invoke(); _onDispose = null; }
    }
}

/// <summary>Señal sin payload (estilo evento simple).</summary>
public sealed class Signal
{
    private readonly Signal<Unit> _inner = new();

    public IDisposable Subscribe(Action callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        return _inner.Subscribe(_ => callback());
    }

    public void Publish() => _inner.Publish(default);
}

/// <summary>Tipo "void" usable como genérico.</summary>
public readonly record struct Unit;
