namespace TextualCsharp.Animation;

/// <summary>
/// Pista de animación que interpola un valor double y lo entrega vía callback.
/// </summary>
internal sealed class AnimationTrack
{
    public required double From;
    public required double To;
    public required TimeSpan Duration;
    public required Easing Easing;
    public required Action<double> OnUpdate;
    public Action? OnComplete;
    public DateTime StartUtc;
    public bool Done;
}

/// <summary>
/// Motor de animaciones simple. Equivalente a <c>textual._animator.Animator</c>.
/// Cada llamada a <see cref="Tick"/> avanza el reloj y empuja los valores
/// interpolados a sus callbacks.
/// </summary>
public sealed class Animator
{
    private readonly List<AnimationTrack> _tracks = new();
    private readonly object _gate = new();

    public int ActiveCount { get { lock (_gate) return _tracks.Count; } }

    /// <summary>Añade una animación numérica. El callback recibe el valor interpolado.</summary>
    public void Animate(
        double from,
        double to,
        TimeSpan duration,
        Action<double> onUpdate,
        Easing easing = Easing.EaseInOutCubic,
        Action? onComplete = null)
    {
        ArgumentNullException.ThrowIfNull(onUpdate);
        if (duration <= TimeSpan.Zero)
        {
            onUpdate(to);
            onComplete?.Invoke();
            return;
        }
        var track = new AnimationTrack
        {
            From = from,
            To = to,
            Duration = duration,
            Easing = easing,
            OnUpdate = onUpdate,
            OnComplete = onComplete,
            StartUtc = DateTime.UtcNow,
        };
        lock (_gate) _tracks.Add(track);
    }

    /// <summary>Avanza todas las animaciones. Devuelve <c>true</c> si alguna sigue activa.</summary>
    public bool Tick()
    {
        var now = DateTime.UtcNow;
        AnimationTrack[] snapshot;
        lock (_gate) snapshot = _tracks.ToArray();
        foreach (var t in snapshot)
        {
            if (t.Done) continue;
            double elapsed = (now - t.StartUtc).TotalMilliseconds;
            double total = t.Duration.TotalMilliseconds;
            double progress = Math.Clamp(elapsed / total, 0, 1);
            double eased = t.Easing.Apply(progress);
            double value = t.From + (t.To - t.From) * eased;
            try { t.OnUpdate(value); } catch { }
            if (progress >= 1)
            {
                t.Done = true;
                try { t.OnComplete?.Invoke(); } catch { }
            }
        }
        lock (_gate) _tracks.RemoveAll(x => x.Done);
        lock (_gate) return _tracks.Count > 0;
    }

    public void Clear()
    {
        lock (_gate) _tracks.Clear();
    }
}
