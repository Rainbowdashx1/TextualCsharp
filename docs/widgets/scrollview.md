# ScrollView

Vista scrollable de líneas de texto. Equivalente a `textual.scroll_view.ScrollView`.
Soporta scrollbar vertical opcional.

## Constructor

```csharp
new ScrollView(IEnumerable<string>? lines = null)
```

## Propiedades

| Propiedad           | Tipo            | Descripción                                |
|---------------------|-----------------|--------------------------------------------|
| `Lines`             | `IList<string>` | Líneas mostradas.                          |
| `Offset`            | `int`           | Línea superior visible (sólo lectura).     |
| `Foreground`        | `Color`         |                                            |
| `Background`        | `Color`         |                                            |
| `FocusedBackground` | `Color`         |                                            |
| `ShowScrollbar`     | `bool`          | Mostrar barra (por defecto `true`).        |

## Métodos

| Método                 | Descripción                          |
|------------------------|--------------------------------------|
| `ScrollTo(int offset)` | Posiciona el offset (con clamp).     |
| `AppendLine(string)`   | Añade una línea al final.            |

## Teclas manejadas

| Tecla                 | Acción                |
|-----------------------|-----------------------|
| `Up` / `Down`         | Una línea             |
| `PageUp` / `PageDown` | Una página            |
| `Home` / `End`        | Inicio / final        |

## Ejemplo: log en vivo

```csharp
var log = new ScrollView();
worker.MessageReceived += m =>
{
	log.AppendLine($"[{DateTime.Now:HH:mm:ss}] {m}");
	log.ScrollTo(int.MaxValue);
};
```
