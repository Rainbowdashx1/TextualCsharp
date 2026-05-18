# Soporte multiplataforma y SSH

TextualCsharp detecta automáticamente el entorno de terminal en el que corre
la aplicación y elige el driver más adecuado, sin intervención del usuario.

## Detección de entorno

La clase [`TerminalEnvironment`](../TextualCsharp/Input/TerminalEnvironment.cs)
expone el método `Detect()` que devuelve uno de estos valores:

| Kind          | Cuándo se devuelve                                                                                |
|---------------|----------------------------------------------------------------------------------------------------|
| `Local`       | Terminal local (cmd, Windows Terminal, xterm, gnome-terminal, iTerm, Alacritty…)                  |
| `RemoteSsh`   | Cuando alguna de las variables `SSH_CONNECTION`, `SSH_CLIENT` o `SSH_TTY` está definida, o cuando `TEXTUALCSHARP_FORCE_SSH=1`. |
| `Headless`    | Cuando stdin y stdout están redirigidos (CI, pipes, contenedor sin TTY).                          |

### Forzar el modo SSH manualmente

```csharp
// 1. Por código (p. ej. desde un parser de argumentos --ssh)
TerminalEnvironment.ForceKind = TerminalEnvironmentKind.RemoteSsh;

// 2. Por variable de entorno
//    PS> $env:TEXTUALCSHARP_FORCE_SSH = "1"
//    sh> export TEXTUALCSHARP_FORCE_SSH=1
```

### Limitaciones conocidas

- Algunos perfiles de `sudo` o launchers de contenedor no propagan las variables
  `SSH_*`. En esos casos la sesión remota aparece como local; usa el override
  manual.
- `tmux`/`screen` detached pueden perder las variables originales tras
  reattach. Se recomienda exportarlas en el `.bashrc`/`.zshrc`.
- `mosh` no exporta `SSH_*`; trátalo manualmente como SSH si lo necesitas.

## Selección de driver

[`TerminalDriverFactory.Create()`](../TextualCsharp/Input/TerminalDriverFactory.cs)
elige automáticamente:

```
Headless       ─► HeadlessTerminalDriver
POSIX (local)  ─► PosixTerminalDriver
POSIX (SSH)    ─► PosixTerminalDriver
Windows (SSH)  ─► PosixTerminalDriver  (sshd usa PTY VT)
Windows (local)─► WindowsTerminalDriver (Win32 ReadConsoleInput)
```

`ConsoleApp.RunAsync` invoca esta factory si no hay un `Driver` asignado
manualmente, así que las apps existentes no requieren cambios.

## Drivers disponibles

- [`WindowsTerminalDriver`](../TextualCsharp/Input/WindowsTerminalDriver.cs):
  usa `ReadConsoleInput` (Win32) para mejor latencia y soporte nativo de ratón.
- [`PosixTerminalDriver`](../TextualCsharp/Input/PosixTerminalDriver.cs):
  pone el TTY en modo crudo vía `termios` y delega el parseo de secuencias
  ANSI/XTerm a [`XTermParser`](../TextualCsharp/Input/XTermParser.cs). Habilita
  reporte SGR de ratón (`?1000h`, `?1003h`, `?1006h`). Si `termios` no está
  disponible (entornos exóticos), cae a `Console.ReadKey`.
- [`HeadlessTerminalDriver`](../TextualCsharp/Input/HeadlessTerminalDriver.cs):
  driver para tests/scripts; el pilot inyecta mensajes sintéticos.

## UTF-8 en sesiones remotas

[`AnsiWriter`](../TextualCsharp/Rendering/AnsiWriter.cs) configura
`Console.OutputEncoding`/`Console.InputEncoding` como UTF-8 sin BOM al
construirse. Esto garantiza que los bordes Unicode y caracteres anchos lleguen
intactos a través de SSH y en terminales con locale `C`.

Adicionalmente, recomendamos en el servidor remoto:

```sh
export LANG=C.UTF-8
export LC_ALL=C.UTF-8
```

## Probar manualmente

### Linux (bash/zsh)

```sh
git clone https://github.com/Rainbowdashx1/TextualCsharp.git
cd TextualCsharp
dotnet test
dotnet run --project samples/Demo   # si el sample existe
```

### Sesión SSH simulada

```sh
TEXTUALCSHARP_FORCE_SSH=1 dotnet test
```

### Sesión SSH real

```sh
ssh usuario@host
# dentro del host remoto
dotnet run --project /ruta/al/proyecto
```

## CI

El workflow [`.github/workflows/nuget-publish.yml`](../.github/workflows/nuget-publish.yml)
ejecuta los tests en una matriz `ubuntu-latest`, `ubuntu-22.04`,
`windows-latest` y `macos-latest`, y vuelve a correrlos en Linux/macOS con las
variables `SSH_*` exportadas para validar el camino de detección remota.
