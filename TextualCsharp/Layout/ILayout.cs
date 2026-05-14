using TextualCsharp.Core.Geometry;
using TextualCsharp.Widgets;

namespace TextualCsharp.Layout;

/// <summary>
/// Contrato de un algoritmo de layout. Posiciona los widgets dados dentro del
/// rectángulo disponible. Equivalente a <c>textual.layout.Layout</c>.
/// </summary>
public interface ILayout
{
    /// <summary>Asigna <see cref="Widget.Region"/> a cada widget dentro de <paramref name="area"/>.</summary>
    /// <returns>El mapa geometry generado (widget → región).</returns>
    MapGeometry Arrange(IReadOnlyList<Widget> widgets, Region area);
}
