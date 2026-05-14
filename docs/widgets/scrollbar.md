# ScrollBar

Barra de scroll empotrable (vertical u horizontal). Pensada para componer dentro
de tus propios widgets scrollables.

## Constructor

```csharp
new ScrollBar(ScrollOrientation orientation = ScrollOrientation.Vertical)
```

## Propiedades

| Propiedad      | Tipo                  | Descripción                            |
|----------------|-----------------------|----------------------------------------|
| `Orientation`  | `ScrollOrientation`   | `Vertical` o `Horizontal`.             |
| `ContentSize`  | `int`                 | Tamaño total del contenido.            |
| `ViewportSize` | `int`                 | Tamaño visible.                        |
| `Position`     | `int`                 | Offset actual.                         |
| `Foreground`   | `Color`               | Color del "pulgar".                    |
| `Background`   | `Color`               | Color del riel.                        |

## Ejemplo

```csharp
var sb = new ScrollBar(ScrollOrientation.Vertical)
{
	ContentSize  = items.Count,
	ViewportSize = visibleRows,
	Position     = scrollOffset,
};
```

> Nota: en la mayoría de casos basta con usar [`ScrollView`](./scrollview.md),
> que ya integra su propia barra.
