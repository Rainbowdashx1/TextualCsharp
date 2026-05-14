namespace TextualCsharp.Reactive;

/// <summary>Suscripción no genérica a cambios de una reactiva.</summary>
public interface IReactiveObservable
{
    /// <summary>Registra un callback (sin argumentos) que se invoca cuando cambia el valor.</summary>
    IDisposable WatchAny(Action callback);
}
