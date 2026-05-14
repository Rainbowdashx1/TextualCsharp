using TextualCsharp.Core;
using TextualCsharp.Core.Geometry;
using TextualCsharp.Input;
using TextualCsharp.Rendering;

namespace TextualCsharp.Widgets;

/// <summary>Nodo de un <see cref="Tree"/>.</summary>
public sealed class TreeNode
{
    public TreeNode(string label, object? data = null)
    {
        Label = label;
        Data = data;
    }

    public string Label { get; set; }
    public object? Data { get; set; }
    public List<TreeNode> Children { get; } = new();
    public bool IsExpanded { get; set; }
    public TreeNode? Parent { get; internal set; }

    public TreeNode Add(string label, object? data = null)
    {
        var child = new TreeNode(label, data) { Parent = this };
        Children.Add(child);
        return child;
    }
}

/// <summary>
/// Árbol expandible/colapsable. Equivalente a <c>textual.widgets.Tree</c>.
/// </summary>
public sealed class Tree : Widget
{
    private readonly List<(TreeNode Node, int Depth)> _flat = new();
    private int _selected;
    private int _offset;
    private bool _dirtyFlat = true;

    public Tree(string rootLabel = "root")
    {
        Root = new TreeNode(rootLabel) { IsExpanded = true };
        CanFocus = true;
    }

    public TreeNode Root { get; }
    public Color Foreground { get; set; } = Color.White;
    public Color Background { get; set; } = Color.FromRgb(20, 20, 30);
    public Color SelectionForeground { get; set; } = Color.Black;
    public Color SelectionBackground { get; set; } = Color.FromRgb(200, 200, 80);

    public TreeNode? SelectedNode =>
        _selected >= 0 && _selected < _flat.Count ? _flat[_selected].Node : null;

    public event Action<Tree, TreeNode>? NodeSelected;
    public event Action<Tree, TreeNode>? NodeActivated;

    public void Invalidate(bool rebuild)
    {
        if (rebuild) _dirtyFlat = true;
        Invalidate();
    }

    private void RebuildFlat()
    {
        _flat.Clear();
        Visit(Root, 0);
        if (_selected >= _flat.Count) _selected = Math.Max(0, _flat.Count - 1);

        void Visit(TreeNode node, int depth)
        {
            _flat.Add((node, depth));
            if (node.IsExpanded)
                foreach (var c in node.Children) Visit(c, depth + 1);
        }
        _dirtyFlat = false;
    }

    private int _lastMouseRow = -1;

    public override bool HandleMouse(MouseEvent ev)
    {
        if (ev.Type != MouseEventType.Down || ev.Button != MouseButton.Left) return false;
        if (_dirtyFlat) RebuildFlat();

        int row = ev.Y - Region.Y;
        int idx = _offset + row;
        if (idx < 0 || idx >= _flat.Count) return false;

        var node = _flat[idx].Node;
        bool wasAlreadySelected = idx == _selected;

        _selected = idx;
        Invalidate();
        NodeSelected?.Invoke(this, node);

        // Segundo clic sobre el mismo nodo con hijos: toggle expand
        if (wasAlreadySelected && node.Children.Count > 0)
        {
            node.IsExpanded = !node.IsExpanded;
            _dirtyFlat = true;
            Invalidate();
            if (node.IsExpanded) NodeActivated?.Invoke(this, node);
        }
        _lastMouseRow = idx;
        return true;
    }

    public override bool HandleKey(KeyEvent ev)
    {
        if (_dirtyFlat) RebuildFlat();
        switch (ev.Key)
        {
            case Key.Up:
                if (_selected > 0) { _selected--; Invalidate(); NodeSelected?.Invoke(this, _flat[_selected].Node); }
                return true;
            case Key.Down:
                if (_selected < _flat.Count - 1) { _selected++; Invalidate(); NodeSelected?.Invoke(this, _flat[_selected].Node); }
                return true;
            case Key.Left:
                if (SelectedNode is { IsExpanded: true, Children.Count: > 0 } n)
                { n.IsExpanded = false; _dirtyFlat = true; Invalidate(); }
                else if (SelectedNode?.Parent is { } p)
                { _selected = _flat.FindIndex(t => t.Node == p); if (_selected < 0) _selected = 0; Invalidate(); }
                return true;
            case Key.Right:
                if (SelectedNode is { Children.Count: > 0 } sn && !sn.IsExpanded)
                { sn.IsExpanded = true; _dirtyFlat = true; Invalidate(); }
                return true;
            case Key.Enter:
                if (SelectedNode is { Children.Count: > 0 } act)
                { act.IsExpanded = !act.IsExpanded; _dirtyFlat = true; Invalidate(); }
                if (SelectedNode is { } selected) NodeActivated?.Invoke(this, selected);
                return true;
        }
        return false;
    }

    public override IEnumerable<Strip> Render(Size size)
    {
        if (size.Width <= 0 || size.Height <= 0) yield break;
        if (_dirtyFlat) RebuildFlat();
        if (_selected < _offset) _offset = _selected;
        else if (_selected >= _offset + size.Height) _offset = _selected - size.Height + 1;

        for (int row = 0; row < size.Height; row++)
        {
            int idx = _offset + row;
            if (idx >= _flat.Count)
            {
                yield return Strip.Filled(size.Width, new ConsoleCell(' ', Foreground, Background, StyleFlags.None));
                continue;
            }
            var (node, depth) = _flat[idx];
            string prefix = new string(' ', depth * 2);
            string marker = node.Children.Count == 0 ? "  " : (node.IsExpanded ? "- " : "+ ");
            string text = prefix + marker + node.Label;
            if (text.Length > size.Width) text = text[..size.Width];
            else text = text.PadRight(size.Width);

            bool isSelected = idx == _selected;
            var fg = isSelected ? SelectionForeground : Foreground;
            var bg = isSelected ? SelectionBackground : Background;
            yield return Strip.FromText(text, fg, bg, isSelected ? StyleFlags.Bold : StyleFlags.None);
        }
    }
}
