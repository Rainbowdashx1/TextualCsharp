# NotificationCenter

Centro de notificaciones (toasts) que se renderizan superpuestas en una esquina
y desaparecen tras un tiempo. Equivalente a `textual.notifications`.

## Niveles

`NotificationLevel.Info`, `Success`, `Warning`, `Error`.
Cada nivel se renderiza con su color e icono ASCII (`i`, `+`, `!`, `x`).

## Propiedades

| Propiedad          | Tipo        | Descripción                                       |
|--------------------|-------------|---------------------------------------------------|
| `DefaultTimeout`   | `TimeSpan`  | Tiempo por defecto antes de expirar (3 s).        |

## Métodos

| Método                                                                                  | Descripción                          |
|-----------------------------------------------------------------------------------------|--------------------------------------|
| `Notify(string message, NotificationLevel = Info, TimeSpan? timeout = null)`            | Añade una notificación.              |
| `Tick()`                                                                                 | Limpia notificaciones expiradas.    |

## Ejemplo

```csharp
var toasts = new NotificationCenter();
// añadirlo al árbol como overlay (por ejemplo en la última fila de un layout)

toasts.Notify("Guardado correctamente", NotificationLevel.Success);
toasts.Notify("Conexion perdida", NotificationLevel.Error, TimeSpan.FromSeconds(5));

// Tick periódico para expirarlas
app.AddTimer(TimeSpan.FromSeconds(1), _ => { toasts.Tick(); return ValueTask.CompletedTask; });
```
