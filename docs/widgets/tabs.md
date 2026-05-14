# Tabs

Pestañas con contenido intercambiable. Equivalente a `textual.widgets.Tabs`.

## Construcción

```csharp
var tabs = new Tabs();
tabs.AddTab("General",   panelGeneral);
tabs.AddTab("Avanzado",  panelAvanzado);
tabs.AddTab("Acerca de", panelAbout);
```

## Propiedades

| Propiedad             | Tipo               | Descripción                       |
|-----------------------|--------------------|-----------------------------------|
| `TabItems`            | `IReadOnlyList<Tab>` | Pestañas registradas.           |
| `ActiveIndex`         | `int`              |                                   |
| `ActiveTab`           | `Tab?`             |                                   |
| `TabForeground`       | `Color`            | Pestañas inactivas.               |
| `ActiveTabForeground` | `Color`            |                                   |
| `ActiveTabBackground` | `Color`            |                                   |
| `Background`          | `Color`            |                                   |

## Eventos

- `event Action<Tabs, int> ActiveChanged` — al cambiar de pestaña.

## Teclas manejadas

| Tecla              | Acción                       |
|--------------------|------------------------------|
| `Left` / `Right`   | Pestaña anterior / siguiente |
| `Ctrl+Tab` / `Ctrl+Shift+Tab` | Igual, vía Ctrl   |
| (otras)            | Se delegan al contenido activo |

## Ejemplo

```csharp
tabs.ActiveChanged += (_, i) => status.Markup = $"Pestaña: {tabs.ActiveTab?.Title}";
```
