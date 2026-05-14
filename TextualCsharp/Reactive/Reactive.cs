using TextualCsharp.Messaging;
using TextualCsharp.Widgets;

namespace TextualCsharp.Reactive;

/// <summary>
/// Notificación emitida cuando una propiedad reactiva cambia de valor.
/// </summary>
public sealed record Changed(string Name, object? OldValue, object? NewValue) : Message;

/// <summary>
/// Property reactivo asociado a un <see cref="Widget"/>. Cuando cambia su
/// valor, invalida el widget (forzando un re-render) y notifica a los
/// observadores. Equivalente a <c>textual.reactive.Reactive</c>.
/// </summary>
public class Reactive<T> : IReactiveObservable
{
    /// <summary>Registra un observador no genérico que se invoca tras cada cambio.</summary>
    public IDisposable WatchAny(Action callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        return Watch((_, _) => callback());
    }

    private readonly Widget? _owner;
    private readonly string _name;
    private readonly EqualityComparer<T> _comparer = EqualityComparer<T>.Default;
    private readonly List<Action<T, T>> _watchers = new();
    private T _value;

    public Reactive(T initial = default!, Widget? owner = null, string name = "")
    {
        _value = initial;
        _owner = owner;
        _name = name;
    }

    /// <summary>Valor actual. Asignar dispara watchers + invalidate del widget.</summary>
    public T Value
    {
        get => _value;
        set => Set(value);
    }

    public string Name => _name;

    /// <summary>Cambia el valor; devuelve <c>true</c> si era distinto.</summary>
    public bool Set(T newValue)
    {
        if (_comparer.Equals(_value, newValue)) return false;
        var old = _value;
        _value = newValue;
        for (int i = 0; i < _watchers.Count; i++)
        {
            try { _watchers[i](old, newValue); } catch { /* observer errors ignored */ }
        }
        if (_owner is not null)
        {
            _owner.Invalidate();
            _owner.TryPost(new Changed(_name, old, newValue) { Sender = _owner });
        }
        return true;
    }

    /// <summary>Registra un observador que se invoca con (oldValue, newValue).</summary>
    public IDisposable Watch(Action<T, T> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        _watchers.Add(callback);
        return new Subscription(() => _watchers.Remove(callback));
    }

    /// <summary>Convierte implícitamente la reactiva en su valor.</summary>
    public static implicit operator T(Reactive<T> r) => r._value;

    public override string ToString() => _value?.ToString() ?? string.Empty;

    private sealed class Subscription : IDisposable
    {
        private Action? _onDispose;
        public Subscription(Action onDispose) { _onDispose = onDispose; }
        public void Dispose() { _onDispose?.Invoke(); _onDispose = null; }
    }
}
