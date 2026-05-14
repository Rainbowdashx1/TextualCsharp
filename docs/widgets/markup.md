# Markup inline

Tanto [`Label`](./label.md) como [`Content`](#content) parsean markup tipo
Rich/Textual: `[bold red]texto[/]`. Las etiquetas se pueden anidar y se cierran
con `[/]`.

## Etiquetas soportadas

### Estilos

| Tag          | Efecto                |
|--------------|-----------------------|
| `bold`       | Negrita               |
| `dim`        | Atenuado              |
| `italic`     | Cursiva               |
| `underline`  | Subrayado             |
| `blink`      | Parpadeo              |
| `reverse`    | Invertir fg/bg        |
| `strike`     | Tachado               |

### Colores

- Nombres ANSI 16: `red`, `green`, `yellow`, `blue`, `magenta`, `cyan`, `white`,
  `black`, `bright_red`, ...
- Tokens semánticos: `primary`, `secondary`, `accent`, `success`, `warning`,
  `error`, `subtle`.
- Truecolor: `#RRGGBB`.
- Fondo: prefija con `on`: `[white on #003366]...[/]`.

## Ejemplos

```csharp
new Label("[bold green]OK[/]: tarea completada");
new Label("[on #222222] menú [/]");
new Label("[bold #FFC080]texto naranja en negrita[/]");
new Label("[reverse]seleccionado[/]");
```

## Escape

Para imprimir un `[` literal, duplícalo: `[[`.
