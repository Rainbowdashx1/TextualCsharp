using TextualCsharp.Messaging;

namespace TextualCsharp.Input;

/// <summary>
/// Contrato abstracto de un driver de terminal. Equivalente a <c>textual.driver.Driver</c>.
/// Una implementación se encarga de:
///   * Configurar el terminal (modo crudo, ANSI, alt-screen, cursor oculto).
///   * Leer input (teclado, ratón, resize) y publicarlo como <see cref="Message"/>.
///   * Restaurar el estado del terminal al detenerse.
/// </summary>
public interface ITerminalDriver : IAsyncDisposable
{
    /// <summary>Inicializa el terminal (modos, alt-screen, …) y arranca el loop de input.</summary>
    Task StartAsync(CancellationToken cancellationToken);

    /// <summary>Detiene el loop de input y restaura el estado original del terminal.</summary>
    Task StopAsync();

    /// <summary>Lee el siguiente evento de input. Bloquea hasta que haya uno o se cancele.</summary>
    ValueTask<Message?> ReadAsync(CancellationToken cancellationToken);

    /// <summary>Tamaño actual de la consola (columnas, filas).</summary>
    (int Width, int Height) GetSize();
}
