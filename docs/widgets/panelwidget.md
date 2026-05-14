# PanelWidget

Contenedor con borde, título y layout aplicado a sus hijos. Pinta el borde y
posiciona los hijos en el área interior.

## Propiedades

| Propiedad         | Tipo                     | Descripción                            |
|-------------------|--------------------------|----------------------------------------|
| `Border`          | `BoxDrawing.BorderKind`  | Estilo del borde.                      |
| `BorderForeground`| `Color`                  | Color del borde.                       |
| `Background`      | `Color`                  | Fondo del área interior.               |
| `Title`           | `string?`                | Título mostrado en el borde superior.  |
| `Layout`          | `ILayout?`               | Layout aplicado a los hijos.           |

## Ejemplo

```csharp
var stats = new PanelWidget
{
	Title  = " Estado ",
	BorderForeground = theme.Design.Primary,
	Background       = theme.Design.Panel,
	Layout = new VerticalLayout(),
	Width  = LayoutSize.Fraction(1),
};
stats.Children.Add(new Label("Usuarios: 42"));
stats.Children.Add(new Label("Errores: 0"));
```
