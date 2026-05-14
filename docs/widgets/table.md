# Table

Tabla simple con headers, filas y scroll vertical. Equivalente a
`textual.widgets.DataTable`.

## Construcción

```csharp
var table = new Table();
table.Headers.Add("Id");
table.Headers.Add("Nombre");
table.Headers.Add("Estado");

table.AddRow("1", "Alice", "OK");
table.AddRow("2", "Bob",   "FAIL");
```

## Propiedades

| Propiedad              | Tipo              | Descripción                       |
|------------------------|-------------------|-----------------------------------|
| `Headers`              | `IList<string>`   | Cabeceras de columna.             |
| `Rows`                 | `IReadOnlyList<string[]>` | Datos (sólo lectura).      |
| `SelectedRow`          | `int`             | Fila seleccionada.                |
| `HeaderForeground`     | `Color`           |                                   |
| `HeaderBackground`     | `Color`           |                                   |
| `Foreground`           | `Color`           |                                   |
| `Background`           | `Color`           |                                   |
| `SelectionBackground`  | `Color`           |                                   |

## Métodos

| Método              | Descripción                              |
|---------------------|------------------------------------------|
| `AddRow(params string[])` | Añade una fila.                    |
| `Clear()`           | Vacía filas y resetea selección.         |

## Teclas manejadas

| Tecla          | Acción              |
|----------------|---------------------|
| `Up/Down`      | Mover selección     |
| `Home/End`     | Primera / última    |

## Ejemplo

```csharp
var t = new Table { Height = LayoutSize.Fraction(1) };
t.Headers.Add("Sprint"); t.Headers.Add("Nombre"); t.Headers.Add("Estado");
foreach (var (n, name, st) in data) t.AddRow(n, name, st);
```
