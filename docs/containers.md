# Contenedores y layouts

Los contenedores son widgets que aplican un `ILayout` a sus hijos.

## Tipos disponibles

| Contenedor              | Layout                |
|-------------------------|-----------------------|
| `VerticalContainer`     | `VerticalLayout`      |
| `HorizontalContainer`   | `HorizontalLayout`    |
| `GridContainer`         | `GridLayout`          |

## Tamaños (`LayoutSize`)

| Factoría                       | Significado                                |
|--------------------------------|--------------------------------------------|
| `LayoutSize.Fixed(int cells)`  | Tamaño absoluto en celdas.                 |
| `LayoutSize.Percent(double p)` | Porcentaje del contenedor (0–100).         |
| `LayoutSize.Auto()`            | Tamaño determinado por el contenido.       |
| `LayoutSize.Fraction(double f)`| Fracción del espacio sobrante (CSS `fr`).  |

Todos los widgets exponen `Width` y `Height` (de tipo `LayoutSize`).

## Ejemplo: layout vertical con cabecera fija y cuerpo elástico

```csharp
var header = new Label("[bold]Mi App[/]") { Height = LayoutSize.Fixed(1) };
var body   = new HorizontalContainer { Height = LayoutSize.Fraction(1) };

var root = new VerticalContainer();
root.Children.Add(header);
root.Children.Add(body);
```

## Ejemplo: dos columnas 1/3 + 2/3

```csharp
var left  = new VerticalContainer { Width = LayoutSize.Fraction(1) };
var right = new VerticalContainer { Width = LayoutSize.Fraction(2) };

var row = new HorizontalContainer();
row.Children.Add(left);
row.Children.Add(right);
```

## Ejemplo: grid 2x2

```csharp
var grid = new GridContainer(
	columns: new[] { LayoutSize.Fraction(1), LayoutSize.Fraction(1) },
	rows:    new[] { LayoutSize.Fraction(1), LayoutSize.Fraction(1) });

grid.Children.Add(new Label("A"));
grid.Children.Add(new Label("B"));
grid.Children.Add(new Label("C"));
grid.Children.Add(new Label("D"));
```
