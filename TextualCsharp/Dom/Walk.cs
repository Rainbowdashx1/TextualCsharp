namespace TextualCsharp.Dom;

/// <summary>Recorridos del árbol DOM. Equivalente a <c>textual.walk</c>.</summary>
public static class Walk
{
    /// <summary>Recorre el árbol en profundidad (pre-order: padre antes que hijos).</summary>
    public static IEnumerable<DomNode> DepthFirst(DomNode root)
    {
        ArgumentNullException.ThrowIfNull(root);
        var stack = new Stack<DomNode>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            var node = stack.Pop();
            yield return node;
            for (int i = node.Children.Count - 1; i >= 0; i--)
                stack.Push(node.Children[i]);
        }
    }

    /// <summary>Recorre el árbol en anchura (BFS).</summary>
    public static IEnumerable<DomNode> BreadthFirst(DomNode root)
    {
        ArgumentNullException.ThrowIfNull(root);
        var queue = new Queue<DomNode>();
        queue.Enqueue(root);
        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            yield return node;
            foreach (var child in node.Children)
                queue.Enqueue(child);
        }
    }
}
