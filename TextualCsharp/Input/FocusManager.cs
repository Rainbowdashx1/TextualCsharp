using TextualCsharp.Dom;
using TextualCsharp.Messaging;
using TextualCsharp.Widgets;

namespace TextualCsharp.Input;

/// <summary>
/// Gestiona el foco entre widgets focusables. Equivalente a la lógica de
/// <c>textual._widget_navigation</c>.
/// </summary>
public sealed class FocusManager
{
    private readonly List<Widget> _order = new();
    private Widget? _focused;

    public Widget? Focused => _focused;

    /// <summary>Reconstruye la lista de focusables a partir del árbol raíz.</summary>
    public void Rebuild(Widget root)
    {
        ArgumentNullException.ThrowIfNull(root);
        _order.Clear();
        foreach (var node in Walk.DepthFirst(root))
        {
            if (node is Widget w && w.CanFocus)
                _order.Add(w);
        }
        if (_focused is not null && !_order.Contains(_focused))
            _focused = null;
        if (_focused is null && _order.Count > 0)
            SetFocus(_order[0]);
    }

    public void FocusNext() => Move(+1);
    public void FocusPrevious() => Move(-1);

    public void SetFocus(Widget? widget)
    {
        if (widget == _focused) return;
        if (_focused is not null)
        {
            _focused.HasFocus = false;
            _focused.TryPost(new BlurEvent { Sender = _focused });
        }
        _focused = widget;
        if (_focused is not null)
        {
            _focused.HasFocus = true;
            _focused.TryPost(new FocusEvent { Sender = _focused });
        }
    }

    private void Move(int delta)
    {
        if (_order.Count == 0) return;
        int idx = _focused is null ? -1 : _order.IndexOf(_focused);
        idx = (idx + delta + _order.Count) % _order.Count;
        SetFocus(_order[idx]);
    }
}
