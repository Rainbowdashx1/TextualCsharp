using TextualCsharp.Messaging;

namespace TextualCsharp.Dom;

/// <summary>
/// Nodo del árbol DOM. Equivalente a <c>textual.dom.DOMNode</c>. Hereda de
/// <see cref="MessagePump"/> para tener su propio bus asíncrono de eventos.
/// </summary>
public abstract class DomNode : MessagePump
{
    private DomNode? _parent;
    private bool _isMounted;

    protected DomNode()
    {
        Children = new NodeList(this);
        Id = string.Empty;
        Classes = new HashSet<string>(StringComparer.Ordinal);
    }

    /// <summary>Identificador opcional (similar al <c>id</c> CSS).</summary>
    public string Id { get; set; }

    /// <summary>Clases CSS-like asociadas al nodo.</summary>
    public ISet<string> Classes { get; }

    public DomNode? Parent => _parent;
    public NodeList Children { get; }
    public bool IsMounted => _isMounted;

    internal void SetParent(DomNode? parent) => _parent = parent;

    /// <summary>Devuelve los ancestros del nodo, del más cercano al más lejano.</summary>
    public IEnumerable<DomNode> Ancestors
    {
        get
        {
            var p = _parent;
            while (p is not null) { yield return p; p = p.Parent; }
        }
    }

    /// <summary>Nodo raíz del árbol al que pertenece.</summary>
    public DomNode Root
    {
        get
        {
            var node = this;
            while (node._parent is not null) node = node._parent;
            return node;
        }
    }

    /// <summary>Monta este nodo y, recursivamente, sus hijos. Inicia los pumps y dispara <see cref="Mount"/>.</summary>
    public async Task MountAsync()
    {
        if (_isMounted) return;
        await StartAsync().ConfigureAwait(false);
        _isMounted = true;
        OnMount();
        await PostAsync(new Mount { Sender = this }).ConfigureAwait(false);
        foreach (var child in Children)
            await child.MountAsync().ConfigureAwait(false);
    }

    /// <summary>Desmonta recursivamente (hijos primero) y detiene el pump.</summary>
    public async Task UnmountAsync()
    {
        if (!_isMounted) return;
        foreach (var child in Children.ToArray())
            await child.UnmountAsync().ConfigureAwait(false);
        await PostAsync(new Unmount { Sender = this }).ConfigureAwait(false);
        OnUnmount();
        _isMounted = false;
        await StopAsync().ConfigureAwait(false);
    }

    /// <summary>Hook síncrono ejecutado al montar (antes de procesar mensajes).</summary>
    protected virtual void OnMount() { }
    /// <summary>Hook síncrono ejecutado al desmontar.</summary>
    protected virtual void OnUnmount() { }

    /// <summary>Procesa un mensaje. Por defecto: bubble al padre si <c>Bubble</c> y no fue detenido.</summary>
    protected override async ValueTask OnMessageAsync(Message message)
    {
        if (message.Bubble && !message.IsStopped && _parent is not null)
            await _parent.PostAsync(message).ConfigureAwait(false);
    }
}
