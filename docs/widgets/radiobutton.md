# RadioButton + RadioGroup

Selección mutuamente excluyente. Equivalente a `textual.widgets.RadioButton`.
Cada `RadioButton` se asocia a un `RadioGroup`; sólo uno del grupo puede estar
seleccionado.

## Constructores

```csharp
new RadioGroup();
new RadioButton(string label = "", RadioGroup? group = null);
```

## Propiedades de `RadioButton`

| Propiedad      | Tipo            | Descripción                            |
|----------------|-----------------|----------------------------------------|
| `Label`        | `string`        | Texto.                                 |
| `IsSelected`   | `bool`          | Estado actual.                         |
| `Group`        | `RadioGroup?`   | Grupo al que pertenece.                |
| `Foreground`   | `Color`         |                                        |
| `Background`   | `Color`         |                                        |

## Eventos

- `event Action<RadioButton, bool> Changed` — `(button, newValue)`.

## Teclas manejadas

| Tecla     | Acción          |
|-----------|-----------------|
| `Espacio` | Seleccionar     |
| `Enter`   | Seleccionar     |

## Ejemplo

```csharp
var group = new RadioGroup();
var dark   = new RadioButton("Dark",   group) { IsSelected = true };
var light  = new RadioButton("Light",  group);
var nord   = new RadioButton("Nord",   group);

foreach (var rb in new[] { dark, light, nord })
	rb.Changed += (b, on) =>
	{
		if (!on) return;
		app.Theme = Theme.All.First(t => t.Name.Equals(b.Label, StringComparison.OrdinalIgnoreCase));
	};

container.Children.Add(dark);
container.Children.Add(light);
container.Children.Add(nord);
```
