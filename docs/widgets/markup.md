# Markup inline

Los widgets [`Label`](./label.md) y [`WrapTextWidget`](./wraptextwidget.md)
parsean markup tipo Rich/Textual: `[bold red]texto[/]`. Las etiquetas se pueden
combinar en una sola apertura y se cierran con `[/]`.

## Estilos

| Tag            | Efecto                          |
|----------------|---------------------------------|
| `bold`         | Negrita                         |
| `dim`          | Atenuado (brillo reducido)      |
| `italic`       | Cursiva                         |
| `underline`    | Subrayado                       |
| `blink`        | Parpadeo                        |
| `reverse`      | Invertir foreground y background|
| `strike` / `strikethrough` | Tachado               |

## Colores de primer plano (foreground)

### Colores ANSI básicos

| Token       | Color               |
|-------------|---------------------|
| `black`     | Negro               |
| `red`       | Rojo                |
| `green`     | Verde               |
| `yellow`    | Amarillo            |
| `blue`      | Azul                |
| `magenta`   | Magenta             |
| `cyan`      | Cian                |
| `white`     | Blanco              |
| `default`   | Color por defecto del terminal |

### Truecolor `#RRGGBB`

Cualquier color RGB exacto en notación hexadecimal de 6 dígitos:

```
[#FF5733]texto naranja[/]
[#00BFFF]texto azul cielo[/]
[bold #FFC080]negrita naranja pastel[/]
```

## Color de fondo (background)

Antepón `on` antes del color para aplicarlo al fondo:

```
[white on red]alerta[/]
[on #003366]fondo azul oscuro[/]
[bold yellow on #1A1A1A]advertencia oscura[/]
```

Se puede combinar foreground, background y estilo en una sola etiqueta:

```
[bold white on red]ERROR[/]
```

## Anidar etiquetas

Las etiquetas se apilan: `[/]` cierra el ámbito más reciente.

```csharp
new Label("[bold]título [red]rojo[/] y normal otra vez[/]");
```

## Escape

Para imprimir un `[` literal, duplícalo: `[[`.

## Ejemplos completos

```csharp
new Label("[bold green]✓ OK[/]: operación completada");
new Label("[bold red]✗ Error[/]: fichero no encontrado");
new Label("[dim]Pulsa [bold]Q[/] para salir.[/]");
new Label("[bold #FFC080]advertencia[/]: espacio en disco bajo");
new Label("[white on #8B0000] CRÍTICO [/]");
new WrapTextWidget("[bold]TextualCsharp[/] es una librería TUI para .NET 10. " +
                   "Construye apps de consola con [cyan]widgets[/], [cyan]layouts[/] " +
                   "y [cyan]markup[/].");
```

## ¿Qué widgets soportan markup?

| Widget                                          | Markup |
|-------------------------------------------------|:------:|
| [`Label`](./label.md)                           | ✅     |
| [`WrapTextWidget`](./wraptextwidget.md)         | ✅     |
| [`TextWidget`](./textwidget.md)                 | ❌     |
| [`Button`](./button.md)                         | ❌     |
