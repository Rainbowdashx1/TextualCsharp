using TextualCsharp.Widgets;

namespace TextualCsharp.Reactive;

/// <summary>
/// Valor derivado de una o más reactivas, recalculado automáticamente cuando
/// alguna dependencia cambia. Equivalente a <c>@reactive</c> compute en Python.
/// </summary>
public sealed class Computed<T>
{
    private readonly Func<T> _compute;
    private readonly Reactive<T> _backing;
    private readonly List<IDisposable> _subscriptions = new();

    public Computed(Func<T> compute, Widget? owner = null, string name = "computed", params IReactiveObservable[] dependencies)
    {
        _compute = compute ?? throw new ArgumentNullException(nameof(compute));
        _backing = new Reactive<T>(compute(), owner, name);
        foreach (var dep in dependencies)
        {
            if (dep is null) continue;
            _subscriptions.Add(dep.WatchAny(() => _backing.Set(_compute())));
        }
    }

    public T Value => _backing.Value;

    public IDisposable Watch(Action<T, T> callback) => _backing.Watch(callback);

    public static implicit operator T(Computed<T> c) => c._backing.Value;

    public void Dispose()
    {
        foreach (var s in _subscriptions) s.Dispose();
        _subscriptions.Clear();
    }
}
