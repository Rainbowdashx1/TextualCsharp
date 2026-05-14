using TextualCsharp.Core;
using TextualCsharp.Core.Geometry;
using TextualCsharp.Dom;
using TextualCsharp.Input;
using TextualCsharp.Messaging;
using TextualCsharp.Rendering;

namespace TextualCsharp.Widgets;

/// <summary>
/// Clase base de todos los widgets. Equivalente a <c>textual.widget.Widget</c>.
/// Hereda de <see cref="DomNode"/> (que a su vez hereda de <c>MessagePump</c>),
/// por lo que cada widget tiene su propio bus asíncrono de mensajes.
/// </summary>
public abstract class Widget : DomNode
{
    private Region _region = Region.Empty;
    private bool _isDirty = true;

    /// <summary>Región asignada por el sistema de layout (en coords del padre/screen).</summary>
    public Region Region
    {
        get => _region;
        set
        {
            if (_region == value) return;
            _region = value;
            Invalidate();
        }
    }

    public Size Size => _region.Size;

    /// <summary>Indica si el widget necesita ser re-renderizado.</summary>
    public bool IsDirty => _isDirty;

    // ---- Propiedades de layout (consumidas por el sistema de layout) ----

    /// <summary>Tamaño deseado a lo largo del eje principal (ver <see cref="Layout.LayoutSize"/>).</summary>
    public Layout.LayoutSize Width { get; set; } = Layout.LayoutSize.Auto();

    /// <summary>Tamaño deseado en el eje secundario.</summary>
    public Layout.LayoutSize Height { get; set; } = Layout.LayoutSize.Auto();

    public Layout.Padding Padding { get; set; } = Layout.Padding.Zero;
    public Layout.Margin Margin { get; set; } = Layout.Margin.Zero;

    /// <summary>Indica si el widget puede recibir el foco vía Tab/click.</summary>
    public virtual bool CanFocus { get; set; }

    /// <summary>Marcado por el <c>FocusManager</c> cuando este widget tiene el foco.</summary>
    public bool HasFocus { get; internal set; }

    /// <summary>Marca el widget como sucio (requiere re-render).</summary>
    public void Invalidate()
    {
        _isDirty = true;
        TryPost(new Invalidate { Sender = this });
    }

    internal void ClearDirty() => _isDirty = false;

    /// <summary>
    /// Maneja un evento de ratón. Devuelve <c>true</c> si el widget consumió el evento.
    /// Sobrescribir en subclases para reaccionar a clics del ratón.
    /// </summary>
    public virtual bool HandleMouse(MouseEvent ev) => false;

    /// <summary>
    /// Produce las tiras (strips) que componen el render del widget para el tamaño dado.
    /// Debe devolver exactamente <c>size.Height</c> strips de ancho <c>size.Width</c>.
    /// </summary>
    public abstract IEnumerable<Strip> Render(Size size);

    /// <summary>
    /// Pinta el widget en un <see cref="ConsoleBuffer"/> aplicando su <see cref="Region"/>.
    /// Conveniente para integración con el renderer del Sprint 1.
    /// </summary>
    public void Paint(ConsoleBuffer buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        if (_region.IsEmpty) return;
        int y = _region.Y;
        foreach (var strip in Render(_region.Size))
        {
            buffer.DrawStrip(_region.X, y, strip);
            y++;
            if (y >= _region.Bottom) break;
        }
        ClearDirty();
    }

    /// <summary>Tamaño preferido si no hay restricciones (override en widgets concretos).</summary>
    public virtual Size GetPreferredSize(Size available) => available;

    /// <summary>
    /// Permite a un widget enfocado procesar una tecla sincrónicamente desde
    /// el loop principal. Devuelve <c>true</c> si el evento fue consumido (no se
    /// propaga al sistema de bindings ni al pump del widget).
    /// </summary>
    public virtual bool HandleKey(Input.KeyEvent ev) => false;
}
