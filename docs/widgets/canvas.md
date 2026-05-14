# Canvas

Superficie de dibujo libre, celda a celda. Equivalente a `textual.canvas.Canvas`.

## Constructor

```csharp
new Canvas(int width = 0, int height = 0)
```

El canvas se redimensiona automáticamente al área asignada por el layout.

## Propiedades

| Propiedad     | Tipo    | Descripción                |
|---------------|---------|----------------------------|
| `Background`  | `Color` | Fondo del canvas.          |

## Métodos

| Método                                           | Descripción                                    |
|--------------------------------------------------|------------------------------------------------|
| `Clear()`                                        | Limpia con el fondo actual.                    |
| `Set(int x, int y, char g, Color? fg, Color? bg, StyleFlags)` | Pinta una celda.                  |
| `DrawText(int x, int y, string, Color? fg, Color? bg, StyleFlags)` | Pinta texto horizontal.       |
| `DrawLine(int x0, int y0, int x1, int y1, char g, Color? fg)` | Línea con Bresenham.              |
| `Resize(int w, int h)`                           | Cambia tamaño manualmente.                     |

## Ejemplo: animar ondas senoidales

```csharp
double t = 0;
app.AddTimer(TimeSpan.FromMilliseconds(80), _ =>
{
	t += 0.25;
	canvas.Clear();
	int w = canvas.Region.Width;
	int h = canvas.Region.Height;
	for (int x = 0; x < w; x++)
	{
		int y = (int)((h / 2) + Math.Sin(t + x * 0.18) * (h / 2 - 1));
		canvas.Set(x, Math.Clamp(y, 0, h - 1), '*', theme.Design.Primary);
	}
	return ValueTask.CompletedTask;
});
```
