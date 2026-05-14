# ListView

Lista vertical scrollable con selección. Equivalente a `textual.widgets.ListView`.

## Constructor

```csharp
new ListView(IEnumerable<string>? items = null)
```

## Propiedades

| Propiedad             | Tipo            | Descripción                       |
|-----------------------|-----------------|-----------------------------------|
| `Items`               | `IList<string>` | Modificable; añade/quita ítems.   |
| `SelectedIndex`       | `int`           |                                   |
| `SelectedItem`        | `string?`       |                                   |
| `Foreground`          | `Color`         |                                   |
| `Background`          | `Color`         |                                   |
| `SelectionForeground` | `Color`         |                                   |
| `SelectionBackground` | `Color`         |                                   |

## Eventos

- `event Action<ListView, int> SelectionChanged` — al cambiar selección.
- `event Action<ListView, int> ItemActivated` — al pulsar Enter sobre un ítem.

## Teclas manejadas

| Tecla              | Acción              |
|--------------------|---------------------|
| `Up` / `Down`      | Mover selección     |
| `Home` / `End`     | Primero / último    |
| `PageUp` / `PageDown` | Paginar          |
| `Enter`            | Activar ítem        |

## Ejemplo

```csharp
var fruits = new ListView(new[] { "Manzana", "Banana", "Cereza" });
fruits.SelectionChanged += (lv, i) => label.Markup = $"Seleccion: {lv.SelectedItem}";
fruits.ItemActivated    += (lv, i) => OpenDetail(lv.SelectedItem!);
```
