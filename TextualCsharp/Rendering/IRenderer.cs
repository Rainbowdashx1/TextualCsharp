using TextualCsharp.Core;

namespace TextualCsharp.Rendering;

/// <summary>Un cambio producido por el diff entre dos buffers.</summary>
public readonly record struct CellChange(int X, int Y, ConsoleCell Cell);

/// <summary>Salida (sink) de renderizado. Implementaciones: <see cref="AnsiWriter"/>, mocks de tests.</summary>
public interface IRenderer
{
    /// <summary>Aplica una secuencia de cambios al sink (en orden).</summary>
    void Apply(IReadOnlyList<CellChange> changes);

    /// <summary>Limpia la pantalla del sink (útil al iniciar).</summary>
    void Clear();

    /// <summary>Fuerza el envío de todos los datos pendientes.</summary>
    void Flush();
}
