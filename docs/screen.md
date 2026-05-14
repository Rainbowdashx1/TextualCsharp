# Screen

Una pantalla coordina layout, render, foco y bindings para un árbol de widgets.

## Constructor

```csharp
new Screen(Widget root)
```

`root` suele ser un contenedor (`VerticalContainer`, `HorizontalContainer`, ...).

## Propiedades

| Propiedad   | Tipo            | Descripción                                          |
|-------------|-----------------|------------------------------------------------------|
| `Root`      | `Widget`        | Raíz del árbol de widgets.                           |
| `Focus`     | `FocusManager`  | Gestiona qué widget tiene el foco.                   |
| `Bindings`  | `BindingMap`    | Atajos de teclado de esta pantalla.                  |
| `Region`    | `Region`        | Área asignada por la app.                            |

## Bindings

```csharp
screen.Bindings.Add("q", "quit");
screen.Bindings.Add("escape", "quit");
screen.Bindings.Add("ctrl+s", "save", "Guarda el documento");
```

Cuando se pulsa la tecla, `ConsoleApp.ActionInvoked` recibe la cadena de acción.

## Foco

```csharp
screen.Focus.SetFocus(myButton);   // establece foco inicial
screen.Focus.FocusNext();          // Tab
screen.Focus.FocusPrevious();      // Shift+Tab
```

`Tab` / `Shift+Tab` están conectadas por defecto.

## Ejemplo completo

```csharp
var content = new VerticalContainer();
content.Children.Add(new Label("Bienvenido"));
content.Children.Add(new Button("Aceptar"));

var screen = new Screen(content);
screen.Bindings.Add("q", "quit");
screen.Focus.SetFocus(content.Children.OfType<Button>().First());
```
