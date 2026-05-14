# BorderWidget

Dibuja un borde rectangular alrededor de su área (sin contenido interior).
Para combinar borde + contenido + layout usa [`PanelWidget`](./panelwidget.md).

## Propiedades

| Propiedad       | Tipo                     | Descripción                              |
|-----------------|--------------------------|------------------------------------------|
| `Kind`          | `BoxDrawing.BorderKind`  | `Light`, `Heavy`, `Double`, `Rounded`... |
| `Foreground`    | `Color`                  | Color de las líneas.                     |
| `Background`    | `Color`                  | Color de fondo dentro del borde.         |
| `Title`         | `string?`                | Título inscrustado en el borde superior. |

## Ejemplo

```csharp
var frame = new BorderWidget
{
	Kind  = BoxDrawing.BorderKind.Rounded,
	Title = " Estado ",
	Foreground = theme.Design.Primary,
};
```
