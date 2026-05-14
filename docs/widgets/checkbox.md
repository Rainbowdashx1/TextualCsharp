# Checkbox

Casilla de verificación. Equivalente a `textual.widgets.Checkbox`.
Se renderiza como `[x] etiqueta` o `[ ] etiqueta`.

## Constructor

```csharp
new Checkbox(string label = "", bool isChecked = false)
```

## Propiedades

| Propiedad     | Tipo    | Descripción                            |
|---------------|---------|----------------------------------------|
| `Label`       | `string`| Texto a la derecha de la casilla.      |
| `IsChecked`   | `bool`  | Estado actual.                         |
| `Foreground`  | `Color` |                                        |
| `Background`  | `Color` |                                        |

## Eventos

- `event Action<Checkbox, bool> Changed` — `(checkbox, newValue)` cuando cambia.

## Teclas manejadas

| Tecla     | Acción      |
|-----------|-------------|
| `Espacio` | Toggle      |
| `Enter`   | Toggle      |

## Ejemplo

```csharp
var bold = new Checkbox("Negrita en titulo", isChecked: true);
bold.Changed += (_, on) => title.Markup = on ? "[bold]Mi App[/]" : "Mi App";
```
