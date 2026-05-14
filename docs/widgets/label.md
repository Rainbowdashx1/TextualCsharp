# Label

Widget de texto con soporte de **markup inline** (`[bold red]hola[/]`).
Equivalente a `textual.widgets.Label`.

## Constructor

```csharp
new Label(string markup = "")
```

## Propiedades

| Propiedad     | Tipo    | Descripción                                  |
|---------------|---------|----------------------------------------------|
| `Markup`      | `string`| Texto con markup. Cambiarlo invalida.        |
| `Background`  | `Color` | Color de fondo de relleno.                   |

## Ejemplo

```csharp
var title = new Label("[bold] Mi App [/]  [dim]subtítulo[/]")
{
	Background = theme.Design.Primary,
	Height     = LayoutSize.Fixed(1),
};
```

Ver la lista completa de etiquetas soportadas en [Markup](./markup.md).
