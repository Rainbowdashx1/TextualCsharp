# TextInput

Campo de texto editable de una línea. Equivalente a `textual.widgets.Input`.

## Constructor

```csharp
new TextInput(string value = "", string placeholder = "")
```

## Propiedades

| Propiedad                | Tipo    | Descripción                                              |
|--------------------------|---------|----------------------------------------------------------|
| `Value`                  | `string`| Contenido actual.                                        |
| `Placeholder`            | `string`| Texto mostrado cuando está vacío y sin foco.             |
| `Foreground`             | `Color` |                                                          |
| `Background`             | `Color` | Cuando no tiene foco.                                    |
| `FocusedBackground`      | `Color` | Cuando tiene foco.                                       |
| `PlaceholderForeground`  | `Color` |                                                          |

## Eventos

- `event Action<TextInput, string> Changed` — al editar.
- `event Action<TextInput> Submitted` — al pulsar Enter.

## Teclas manejadas

| Tecla       | Acción                          |
|-------------|---------------------------------|
| caracteres  | Inserta en la posición del cursor |
| `Backspace` | Borra el carácter anterior      |
| `Delete`    | Borra el carácter actual        |
| `Left/Right`| Mueve el cursor                 |
| `Home/End`  | Inicio / fin de línea           |
| `Enter`     | Dispara `Submitted`             |

## Ejemplo

```csharp
var name = new TextInput("", "Escribe tu nombre...");
name.Submitted += t => status.Markup = $"Hola, [bold]{t.Value}[/]!";
```
