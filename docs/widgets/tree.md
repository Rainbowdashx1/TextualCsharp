# Tree + TreeNode

Árbol expandible/colapsable. Equivalente a `textual.widgets.Tree`.
Cada nodo tiene un `Label`, hijos y un estado `IsExpanded`.

## Construcción

```csharp
var tree = new Tree("Proyecto");
var src = tree.Root.Add("src");
src.IsExpanded = true;
src.Add("App.cs");
src.Add("Program.cs");
var widgets = src.Add("Widgets");
widgets.Add("Button.cs");
widgets.Add("Label.cs");
```

## API de `TreeNode`

| Miembro                                   | Descripción                            |
|-------------------------------------------|----------------------------------------|
| `Label`                                   | Texto mostrado.                        |
| `Data`                                    | Payload arbitrario asociado.           |
| `Children`                                | Lista de hijos.                        |
| `IsExpanded`                              | Plegado/desplegado.                    |
| `Parent`                                  | Nodo padre (null en `Root`).           |
| `Add(string label, object? data = null)`  | Añade un hijo y lo devuelve.           |

## Propiedades de `Tree`

| Propiedad             | Tipo         | Descripción                          |
|-----------------------|--------------|--------------------------------------|
| `Root`                | `TreeNode`   | Nodo raíz (auto-expandido).          |
| `SelectedNode`        | `TreeNode?`  | Nodo seleccionado.                   |
| `Foreground/Background/SelectionForeground/SelectionBackground` | `Color` | Estilos. |

## Eventos

- `event Action<Tree, TreeNode> NodeSelected` — al moverse la selección.
- `event Action<Tree, TreeNode> NodeActivated` — al pulsar Enter.

## Teclas manejadas

| Tecla    | Acción                                           |
|----------|--------------------------------------------------|
| `Up/Down`| Mover selección                                  |
| `Left`   | Colapsar (o subir al padre si ya colapsado)      |
| `Right`  | Expandir                                         |
| `Enter`  | Alternar expandido; emite `NodeActivated`        |

## Ejemplo

```csharp
tree.NodeSelected += (_, n) => status.Markup = $"Sel: {n.Label}";
tree.NodeActivated += (_, n) =>
{
	if (n.Data is string path) OpenFile(path);
};
```
