# ProgressBar

Barra de progreso determinada (con `Total`) o spinner indeterminado
(cuando `Total = null`). Equivalente a `textual.widgets.ProgressBar`.

## Propiedades

| Propiedad         | Tipo       | Descripción                                       |
|-------------------|------------|---------------------------------------------------|
| `Total`           | `double?`  | Máximo. `null` => spinner indeterminado.          |
| `Progress`        | `double`   | Valor actual (se hace clamp a `[0, Total]`).      |
| `BarForeground`   | `Color`    | Color del relleno.                                |
| `BarBackground`   | `Color`    | Color del tramo no relleno.                       |
| `TextForeground`  | `Color`    | Color del porcentaje superpuesto.                 |

## Métodos

- `Tick()` — avanza un frame del spinner (llamar desde un timer si es indeterminado).

## Ejemplo determinado

```csharp
var bar = new ProgressBar { Total = 100, Progress = 0 };
worker.ProgressChanged += pct => bar.Progress = pct;
```

Combinado con [`Animator`](../animator.md) para transición suave:

```csharp
worker.ProgressChanged += pct =>
{
	app.Animator.Animate(bar.Progress, pct, TimeSpan.FromMilliseconds(180),
		v => bar.Progress = v, Easing.EaseOutCubic);
};
```

## Ejemplo indeterminado (spinner)

```csharp
var spinner = new ProgressBar { Total = null };
app.AddTimer(TimeSpan.FromMilliseconds(100), _ => { spinner.Tick(); return ValueTask.CompletedTask; });
```
