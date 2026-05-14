# ConsoleApp

Entry point de la aplicación. Coordina el driver de terminal, el stack de pantallas,
el event loop, los timers, el motor de animaciones y el tema activo.

## Propiedades

| Propiedad         | Tipo                  | Descripción                                                |
|-------------------|-----------------------|------------------------------------------------------------|
| `ActiveScreen`    | `Screen?`             | Pantalla en la cima del stack.                             |
| `Theme`           | `Theme`               | Tema activo. Cambiarlo hace hot-reload.                    |
| `Animator`        | `Animator`            | Motor de animaciones.                                      |
| `GlobalBindings`  | `BindingMap`          | Bindings que se aplican en todas las pantallas.            |
| `Driver`          | `ITerminalDriver?`    | Driver de terminal (auto: `WindowsTerminalDriver`).        |
| `Renderer`        | `IRenderer?`          | Renderer (auto: `AnsiWriter`).                             |
| `CurrentBuffer`   | `ConsoleBuffer?`      | Último frame compuesto (útil en tests).                    |

## Eventos

- `ThemeChanged(Theme)` — el tema activo cambió.
- `ActionInvoked(string action)` — un binding pidió ejecutar una acción.

## Métodos

```csharp
Task PushScreenAsync(Screen screen);   // apila una pantalla nueva como activa
Task PopScreenAsync();                  // desapila la pantalla activa
void Post(Message message);             // encola un mensaje al loop
Timer AddTimer(TimeSpan interval, Func<CancellationToken, ValueTask> callback);
void Exit();                            // pide cierre limpio
Task<int> RunAsync(CancellationToken ct = default);
```

## Ejemplo

```csharp
await using var app = new ConsoleApp();
app.Theme = Theme.Dracula;
app.AddTimer(TimeSpan.FromSeconds(1), _ => { /* tick */ return ValueTask.CompletedTask; });

app.ActionInvoked += async action =>
{
	switch (action)
	{
		case "quit":  app.Exit(); break;
		case "about": await app.PushScreenAsync(BuildAboutModal()); break;
	}
};

await app.PushScreenAsync(mainScreen);
await app.RunAsync();
```
