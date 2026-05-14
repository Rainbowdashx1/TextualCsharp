using System.Collections;
using TextualCsharp.Messaging;

namespace TextualCsharp.Dom;

/// <summary>
/// Colección observable de nodos hijos. Equivalente a <c>textual._node_list.NodeList</c>.
/// Notifica <see cref="Added"/> y <see cref="Removed"/> cuando cambia.
/// </summary>
public sealed class NodeList : IReadOnlyList<DomNode>
{
    private readonly List<DomNode> _items = new();
    private readonly DomNode _owner;

    internal NodeList(DomNode owner) => _owner = owner;

    public int Count => _items.Count;
    public DomNode this[int index] => _items[index];

    public event Action<DomNode>? Added;
    public event Action<DomNode>? Removed;

    public void Add(DomNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        if (node.Parent is not null && node.Parent != _owner)
            throw new InvalidOperationException("Node already has a parent.");
        if (_items.Contains(node)) return;
        _items.Add(node);
        node.SetParent(_owner);
        Added?.Invoke(node);
    }

    public bool Remove(DomNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        if (!_items.Remove(node)) return false;
        node.SetParent(null);
        Removed?.Invoke(node);
        return true;
    }

    public void Clear()
    {
        var snapshot = _items.ToArray();
        _items.Clear();
        foreach (var n in snapshot)
        {
            n.SetParent(null);
            Removed?.Invoke(n);
        }
    }

    public int IndexOf(DomNode node) => _items.IndexOf(node);

    public IEnumerator<DomNode> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
