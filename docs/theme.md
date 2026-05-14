# Theme

Paleta de colores semántica. La app tiene siempre un tema activo, accesible
via `app.Theme` o `ThemeProvider.Current`.

## Temas incluidos

`Theme.Dark`, `Theme.Light`, `Theme.Dracula`, `Theme.Nord`, `Theme.Neon`.
La lista completa está en `Theme.All`.

## Tokens semánticos

| Token              | Uso típico                                  |
|--------------------|---------------------------------------------|
| `background`       | Fondo de la pantalla.                       |
| `surface`          | Fondo de áreas secundarias (header, footer).|
| `panel`            | Fondo de paneles con borde.                 |
| `foreground`       | Texto principal.                            |
| `subtle`           | Texto atenuado.                             |
| `primary`          | Color de marca / acción primaria.           |
| `secondary`        | Color secundario.                           |
| `accent`           | Resaltado, énfasis.                         |
| `success`          | Estado positivo.                            |
| `warning`          | Advertencia.                                |
| `error`            | Error / destructivo.                        |
| `border`           | Borde de paneles.                           |

## API

```csharp
Color c = theme.GetColor("primary");
app.Theme = Theme.Dracula;   // dispara ThemeChanged + repinta
```

## Hot-reload

Suscríbete y repinta tus widgets con los tokens del nuevo tema:

```csharp
app.ThemeChanged += t =>
{
	panel.BorderForeground = t.Design.Primary;
	panel.Background       = t.Design.Panel;
	button.Background      = t.Design.Primary;
};
```

## Tema personalizado

```csharp
var mio = new Theme(
	"mio",
	new Design(
		Background: Color.FromRgb(0, 0, 0),
		Surface:    Color.FromRgb(10, 10, 10),
		Panel:      Color.FromRgb(20, 20, 20),
		Foreground: Color.White,
		SubtleForeground: Color.FromRgb(150, 150, 150),
		Primary:   Color.FromRgb(0, 180, 255),
		Secondary: Color.FromRgb(200, 100, 255),
		Accent:    Color.FromRgb(255, 200, 80),
		Success:   Color.FromRgb(80, 200, 120),
		Warning:   Color.FromRgb(240, 190, 70),
		Error:     Color.FromRgb(240, 90, 90),
		Border:    Color.FromRgb(80, 80, 100)));

app.Theme = mio;
```
