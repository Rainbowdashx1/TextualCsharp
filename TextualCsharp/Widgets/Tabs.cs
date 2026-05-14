using TextualCsharp.Core;
using TextualCsharp.Core.Geometry;
using TextualCsharp.Input;
using TextualCsharp.Layout;
using TextualCsharp.Rendering;

namespace TextualCsharp.Widgets;

/// <summary>Pestañas con contenido intercambiable. Equivalente a <c>textual.widgets.Tabs</c>.</summary>
public sealed class Tabs : Widget
{
    public sealed class Tab
    {
        public Tab(string title, Widget content) { Title = title; Content = content; }
        public string Title { get; set; }
        public Widget Content { get; set; }
    }

    private readonly List<Tab> _tabs = new();
    private int _activeIndex;

    public Tabs()
    {
        CanFocus = true;
    }

    public IReadOnlyList<Tab> TabItems => _tabs;
    public int ActiveIndex
    {
        get => _activeIndex;
        set
        {
            if (_tabs.Count == 0) { _activeIndex = 0; return; }
            var v = Math.Clamp(value, 0, _tabs.Count - 1);
            if (v == _activeIndex) return;
            _activeIndex = v;
            SyncActiveChild();
            ActiveChanged?.Invoke(this, _activeIndex);
            Invalidate();
        }
    }

    public Tab? ActiveTab =>
        _activeIndex >= 0 && _activeIndex < _tabs.Count ? _tabs[_activeIndex] : null;

    public Color TabForeground { get; set; } = Color.FromRgb(150, 150, 170);
    public Color TabBackground { get; set; } = Color.FromRgb(20, 20, 30);
    public Color ActiveTabForeground { get; set; } = Color.White;
    public Color ActiveTabBackground { get; set; } = Color.FromRgb(130, 60, 160);
    public Color SeparatorForeground { get; set; } = Color.FromRgb(130, 60, 160);
    public Color Background { get; set; } = Color.FromRgb(20, 20, 30);

    public event Action<Tabs, int>? ActiveChanged;

    public void AddTab(Tab tab)
    {
        ArgumentNullException.ThrowIfNull(tab);
        _tabs.Add(tab);
        if (_tabs.Count == 1) SyncActiveChild();
        Invalidate();
    }

    public void AddTab(string title, Widget content) => AddTab(new Tab(title, content));

    /// <summary>Asegura que sólo el contenido de la pestaña activa esté en el árbol DOM.</summary>
    private void SyncActiveChild()
    {
        var active = ActiveTab?.Content;
        // Remover cualquier hijo distinto del activo.
        foreach (var child in Children.OfType<Widget>().ToArray())
        {
            if (child != active) Children.Remove(child);
        }
        if (active is not null && active.Parent != this)
            Children.Add(active);
    }

    public override bool HandleKey(KeyEvent ev)
    {
        if ((ev.Modifiers & KeyModifiers.Control) != 0)
        {
            if (ev.Key == Key.Tab) { ActiveIndex = (_activeIndex + 1) % Math.Max(1, _tabs.Count); return true; }
            if (ev.Key == Key.BackTab) { ActiveIndex = (_activeIndex - 1 + _tabs.Count) % Math.Max(1, _tabs.Count); return true; }
        }
        if (ev.Key == Key.Left) { ActiveIndex--; return true; }
        if (ev.Key == Key.Right) { ActiveIndex++; return true; }

        // Delegamos al contenido activo
        return ActiveTab?.Content.HandleKey(ev) ?? false;
    }

    public override bool HandleMouse(MouseEvent ev)
    {
        if (ev.Type != MouseEventType.Down || ev.Button != MouseButton.Left) return false;

        // La fila de cabecera es la primera fila de la región del widget.
        // Un clic en cualquier otra fila se ignora aquí (el contenido lo maneja).
        if (ev.Y != Region.Y) return false;

        // Reconstruir las posiciones X de cada pestaña igual que en Render.
        int x = Region.X;
        for (int i = 0; i < _tabs.Count; i++)
        {
            int tabWidth = _tabs[i].Title.Length + 2; // " título "
            if (ev.X >= x && ev.X < x + tabWidth)
            {
                ActiveIndex = i;
                return true;
            }
            x += tabWidth;
        }
        return false;
    }

    public override IEnumerable<Strip> Render(Size size)
    {
        if (size.Width <= 0 || size.Height <= 0) yield break;

        // --- Fila de pestañas ---
        // Calcular la posición X donde empieza y termina la pestaña activa.
        var headerCells = new ConsoleCell[size.Width];
        int x = 0;
        int activeStartX = 0, activeEndX = 0;
        for (int i = 0; i < _tabs.Count && x < size.Width; i++)
        {
            string title = $" {_tabs[i].Title} ";
            bool active = i == _activeIndex;
            if (active) activeStartX = x;
            var fg = active ? ActiveTabForeground : TabForeground;
            var bg = active ? ActiveTabBackground : Background;
            var style = active ? StyleFlags.Bold : StyleFlags.None;
            for (int j = 0; j < title.Length && x < size.Width; j++, x++)
                headerCells[x] = new ConsoleCell(title[j], fg, bg, style);
            if (active) activeEndX = x;
        }
        while (x < size.Width)
            headerCells[x++] = new ConsoleCell(' ', TabForeground, Background, StyleFlags.None);
        yield return new Strip(headerCells);

        // --- Fila separadora ---
        // La línea separadora es del color activo debajo de la pestaña activa,
        // y del color de fondo en el resto, creando el efecto visual de "tab abierto".
        if (size.Height > 1)
        {
            var separatorCells = new ConsoleCell[size.Width];
            for (int sx = 0; sx < size.Width; sx++)
            {
                bool underActive = sx >= activeStartX && sx < activeEndX;
                // Bajo la pestaña activa: sin línea (mismo fondo que el contenido).
                // Fuera: línea horizontal con el color acento.
                char ch = underActive ? ' ' : '─';
                var fg = underActive ? Background : SeparatorForeground;
                var bg = Background;
                separatorCells[sx] = new ConsoleCell(ch, fg, bg, StyleFlags.None);
            }
            yield return new Strip(separatorCells);
        }

        // --- Contenido activo ---
        int headerRows = size.Height > 1 ? 2 : 1;
        var content = ActiveTab?.Content;
        if (content is null)
        {
            for (int i = headerRows; i < size.Height; i++)
                yield return Strip.Filled(size.Width, new ConsoleCell(' ', Color.Default, Background, StyleFlags.None));
            yield break;
        }
        var contentRegion = new Region(Region.X, Region.Y + headerRows, size.Width, size.Height - headerRows);
        content.Region = contentRegion;
        if (content is Container c) c.Arrange();
        foreach (var strip in content.Render(contentRegion.Size))
            yield return strip;
    }
}
