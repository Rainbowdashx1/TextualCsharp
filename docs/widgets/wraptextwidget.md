# WrapTextWidget

Widget de texto con **word-wrap automático** y soporte de **markup inline**
(`[bold red]…[/]`). Cuando el contenido supera el ancho disponible, continúa
en la siguiente línea en lugar de truncarse.

Es la alternativa a [`Label`](./label.md) para textos largos (descripciones,
pistas, mensajes de ayuda) dentro de un `VerticalContainer`.

## Constructor

```csharp
new WrapTextWidget(string markup = "")
```

## Propiedades

| Propiedad    | Tipo     | Descripción                                                              |
|--------------|----------|--------------------------------------------------------------------------|
| `Markup`     | `string` | Texto con markup inline. Cambiarlo re-parsea y marca el widget como sucio. |
| `Background` | `Color`  | Color de fondo de relleno para las líneas.                               |

> El ancho y alto del widget son `Auto` por defecto. El `VerticalLayout` llama
> a `GetPreferredSize` para conocer cuántas líneas necesita el texto **antes**
> de pintarlo, reservando el espacio correcto automáticamente.

## Comportamiento de wrap

- **Word-wrap**: nunca corta una palabra a la mitad salvo que sea más larga que
  el ancho disponible, en cuyo caso la divide a la fuerza.
- **Saltos explícitos**: respeta `\n` dentro del texto como saltos de línea.
- **Markup**: los estilos de color y negrita se preservan en cada fragmento
  de línea resultante.

## Diferencias con Label y TextWidget

| Característica          | `TextWidget` | `Label`  | `WrapTextWidget` |
|-------------------------|:------------:|:--------:|:----------------:|
| Markup inline           | ❌           | ✅       | ✅               |
| Múltiples líneas (wrap) | ❌           | ❌       | ✅               |
| Altura fija por defecto | Sí (1)       | Sí (1)   | No (Auto)        |

## Ejemplo

```csharp
var descripcion = new WrapTextWidget(
	"[bold]TextualCsharp[/] es una librería TUI para .NET 10 inspirada en " +
	"Textual de Python. Permite construir interfaces de consola interactivas " +
	"con layouts, widgets, foco y animaciones.")
{
	Background = theme.Design.Surface,
};

var hint = new WrapTextWidget("[dim]Pulsa [bold]Q[/] para salir.[/]");

var panel = new VerticalContainer { Width = LayoutSize.Fraction(2) };
panel.Children.Add(descripcion);
panel.Children.Add(new SpacerWidget { Height = LayoutSize.Fixed(1) });
panel.Children.Add(hint);
```

Ver la lista completa de etiquetas de markup en [Markup y colores](./markup.md).
