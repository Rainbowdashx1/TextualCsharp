# TextWidget

Texto plano de una línea, sin parsing de markup. Para texto con estilo usa
[`Label`](./label.md).

## Constructor

```csharp
new TextWidget(string text = "")
```

## Propiedades

| Propiedad     | Tipo         | Descripción                       |
|---------------|--------------|-----------------------------------|
| `Text`        | `string`     | Contenido literal.                |
| `Foreground`  | `Color`      |                                   |
| `Background`  | `Color`      |                                   |
| `Style`       | `StyleFlags` | `Bold`, `Italic`, `Underline`...  |

## Ejemplo

```csharp
var line = new TextWidget("Estado: OK")
{
	Foreground = Color.FromRgb(80, 200, 120),
	Style      = StyleFlags.Bold,
};
```
