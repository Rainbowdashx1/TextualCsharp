# Animator

Motor de animaciones que interpola un `double` durante una duración con un
`Easing` y entrega los valores intermedios a un callback.

Acceso: `app.Animator`. La app hace tick automáticamente a ~30 fps mientras hay
animaciones activas.

## Lanzar una animación

```csharp
app.Animator.Animate(
	from: 0,
	to: 100,
	duration: TimeSpan.FromMilliseconds(800),
	onUpdate: v => progress.Progress = v,
	easing: Easing.EaseOutCubic,
	onComplete: () => Console.Beep());
```

## Easings disponibles

`Linear`, `EaseInQuad`, `EaseOutQuad`, `EaseInOutQuad`, `EaseInCubic`,
`EaseOutCubic`, `EaseInOutCubic`, `EaseOutBack`, `EaseInOutBack`, etc.
(ver `Animation/Easing.cs`).

## Ejemplo: contador animado al arrancar

```csharp
var counter = new Reactive<int>(0);
app.Animator.Animate(0, 42, TimeSpan.FromMilliseconds(1400),
	v => counter.Value = (int)v, Easing.EaseOutBack);
```

## Otros métodos

- `Animator.Tick()` — avance manual (uso interno; la app lo hace por ti).
- `Animator.Clear()` — cancela todas las animaciones activas.
- `Animator.ActiveCount` — cuántas pistas hay vivas.
