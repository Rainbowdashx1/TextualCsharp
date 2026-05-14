namespace TextualCsharp.Messaging;

/// <summary>
/// Mensaje base del sistema. Equivalente a <c>textual.message.Message</c>.
/// </summary>
/// <remarks>
/// Los mensajes son inmutables (records) y viajan por el <see cref="MessagePump"/>
/// hasta ser procesados por <c>OnMessageAsync</c>. Eventos del ciclo de vida
/// (<see cref="Mount"/>, <see cref="Unmount"/>) y de input se modelan como subclases.
/// </remarks>
public abstract record Message
{
    public object? Sender { get; init; }
    public DateTime TimestampUtc { get; } = DateTime.UtcNow;

    /// <summary>Indica si el mensaje debe propagarse a los ancestros (bubbling).</summary>
    public virtual bool Bubble => true;

    /// <summary>Si está marcado, el message pump no lo propagará a ancestros.</summary>
    public bool IsStopped { get; private set; }

    public void Stop() => IsStopped = true;
}

/// <summary>Mensaje de ciclo de vida: nodo montado en el árbol.</summary>
public sealed record Mount : Message
{
    public override bool Bubble => false;
}

/// <summary>Mensaje de ciclo de vida: nodo desmontado del árbol.</summary>
public sealed record Unmount : Message
{
    public override bool Bubble => false;
}

/// <summary>Solicitud de re-render de un nodo (invalida su área).</summary>
public sealed record Invalidate : Message;

/// <summary>Mensaje genérico de actualización con un payload arbitrario.</summary>
public sealed record Update(object? Payload = null) : Message;
