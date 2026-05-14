# Reactive&lt;T&gt;

Estado observable. Al cambiar su valor, dispara watchers e invalida automáticamente
el widget propietario (si se le pasa uno) para forzar re-render.

## Creación

```csharp
var counter = new Reactive<int>(0);
var name    = new Reactive<string>("ana", owner: this, name: "Name");
```

## API

| Miembro             | Descripción                                                  |
|---------------------|--------------------------------------------------------------|
| `Value`             | Lee/escribe el valor (igualdad con `EqualityComparer<T>`).   |
| `Set(newValue)`     | Equivalente a asignar `Value`; devuelve `true` si cambió.    |
| `Watch((old,new)=>...)` | Observa cambios. Devuelve `IDisposable` para cancelar.   |
| `WatchAny(() => ...)`   | Observa cambios sin recibir valores.                     |
| conversión implícita    | `T value = miReactive;`                                  |

## Ejemplo

```csharp
var clock = new Reactive<string>(DateTime.Now.ToString("HH:mm:ss"));
var label = new Label();
clock.Watch((_, v) => label.Markup = $"[primary]{v}[/]");

app.AddTimer(TimeSpan.FromSeconds(1), _ =>
{
	clock.Value = DateTime.Now.ToString("HH:mm:ss");
	return ValueTask.CompletedTask;
});
```
