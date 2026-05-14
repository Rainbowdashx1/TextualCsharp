# Button

Botón focusable que dispara `Pressed` al pulsar **Enter** o **Espacio**.
Equivalente a `textual.widgets.Button`.

## Constructor

```csharp
new Button(string label = "")
```

## Propiedades

| Propiedad           | Tipo         | Descripción                                       |
|---------------------|--------------|---------------------------------------------------|
| `Label`             | `string`     | Texto mostrado.                                   |
| `Foreground`        | `Color`      |                                                   |
| `Background`        | `Color`      | Fondo cuando no tiene foco.                       |
| `FocusedBackground` | `Color`      | Fondo cuando tiene foco.                          |
| `Style`             | `StyleFlags` |                                                   |

## Eventos

- `event Action<Button> Pressed` — al activarse.

## Teclas manejadas

| Tecla        | Acción     |
|--------------|------------|
| `Enter`      | Activar    |
| `Espacio`    | Activar    |

## Ejemplo

```csharp
var save = new Button("Guardar")
{
	Background        = theme.Design.Primary,
	FocusedBackground = theme.Design.Accent,
};
save.Pressed += _ => Document.Save();
```
