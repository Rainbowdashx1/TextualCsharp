# SpacerWidget

Widget vacío que rellena con un glifo (por defecto, espacio). Útil para separar
elementos en un layout o ocupar espacio elástico.

## Propiedades

| Propiedad     | Tipo    | Descripción                                                  |
|---------------|---------|--------------------------------------------------------------|
| `Glyph`       | `char`  | Glifo de relleno (por defecto `' '`).                        |
| `Foreground`  | `Color` |                                                              |
| `Background`  | `Color` |                                                              |

## Ejemplo

```csharp
container.Children.Add(new Label("Arriba"));
container.Children.Add(new SpacerWidget { Height = LayoutSize.Fraction(1) }); // empuja
container.Children.Add(new Label("Abajo"));
```
