# Documentación de TextualCsharp

TextualCsharp es una librería TUI (Terminal User Interface) para **.NET 10** inspirada
en [Textual](https://textual.textualize.io/) de Python. Permite construir aplicaciones
de consola interactivas con widgets, layouts, foco, bindings de teclado, estado
reactivo, temas y animaciones.

Esta documentación describe cada control disponible, su API pública, eventos,
teclas que maneja y un ejemplo mínimo de uso.

## Empezar en 30 segundos

```csharp
using TextualCsharp.App;
using TextualCsharp.Layout;
using TextualCsharp.Widgets;

var root = new VerticalContainer();
root.Children.Add(new Label("Hola, mundo"));
root.Children.Add(new Button("Pulsame"));

var screen = new Screen(root);
screen.Bindings.Add("q", "quit");

await using var app = new ConsoleApp();
app.ActionInvoked += async a => { if (a == "quit") app.Exit(); };
await app.PushScreenAsync(screen);
await app.RunAsync();
```

## Conceptos clave

| Concepto              | Tipo                                         | Descripción                                                          |
|-----------------------|----------------------------------------------|----------------------------------------------------------------------|
| Aplicación            | [`ConsoleApp`](./app.md)                     | Event loop, stack de pantallas, timers, animator, tema.              |
| Pantalla              | [`Screen`](./screen.md) / [`ModalScreen`](./modal-screen.md) | Árbol de widgets, layout, foco y bindings.            |
| Widget                | `Widget`                                     | Clase base de todos los controles.                                   |
| Contenedor            | [`VerticalContainer`, `HorizontalContainer`, `GridContainer`](./containers.md) | Aplican un layout a sus hijos.       |
| Tamaño                | `LayoutSize.Fixed/Percent/Auto/Fraction`     | Sistema de tamaños tipo CSS.                                         |
| Markup                | [`Markup`](./widgets/markup.md)              | Etiquetas de color y estilo inline `[bold red]…[/]` para Label y WrapTextWidget. |
| Estado                | [`Reactive<T>`](./reactive.md)               | Estado observable que invalida el widget al cambiar.                 |
| Estado derivado       | `Computed<T>`                                | Valor calculado automáticamente desde una o más `Reactive<T>`.       |
| Señal pub/sub         | `Signal<T>` / `Signal`                       | Comunicación desacoplada entre widgets sin dependencia directa.      |
| Validación            | `Validator<T>` / `ValidationResult`          | Validadores componibles para reactivas e inputs.                     |
| Tema                  | [`Theme`](./theme.md)                        | Paletas de color semánticas con hot-reload.                          |
| Animación             | [`Animator`](./animator.md)                  | Interpolación numérica con easing.                                   |
| Bindings              | `BindingMap`                                 | Mapea pulsaciones a "acciones" (cadenas que dispara `ActionInvoked`).|

## Controles (widgets)

### Texto y presentación

- [Label](./widgets/label.md) — texto con markup inline `[bold red]...[/]`, una línea.
- [WrapTextWidget](./widgets/wraptextwidget.md) — texto con markup y **word-wrap automático** en múltiples líneas.
- [TextWidget](./widgets/textwidget.md) — texto plano monolínea sin markup.
- [Markup y colores](./widgets/markup.md) — referencia completa de etiquetas, colores ANSI y truecolor `#RRGGBB`.
- [BorderWidget](./widgets/borderwidget.md) — recuadro decorativo.
- [PanelWidget](./widgets/panelwidget.md) — contenedor con borde y título.
- [SpacerWidget](./widgets/spacerwidget.md) — separador flexible.

### Entradas y acciones

- [Button](./widgets/button.md) — botón focusable, evento `Pressed`.
- [Checkbox](./widgets/checkbox.md) — casilla de verificación.
- [RadioButton / RadioGroup](./widgets/radiobutton.md) — selección excluyente.
- [TextInput](./widgets/textinput.md) — campo de texto editable.

### Colecciones

- [ListView](./widgets/listview.md) — lista vertical con selección.
- [Tree / TreeNode](./widgets/tree.md) — árbol expandible.
- [Table](./widgets/table.md) — tabla con headers y selección de fila.
- [Tabs](./widgets/tabs.md) — pestañas con contenido intercambiable.

### Visualización y dibujo

- [Canvas](./widgets/canvas.md) — superficie de dibujo libre.
- [ProgressBar](./widgets/progressbar.md) — barra determinada o spinner.
- [ScrollView](./widgets/scrollview.md) — vista scrollable de líneas.
- [ScrollBar](./widgets/scrollbar.md) — barra de scroll embebible.
- [NotificationCenter](./widgets/notificationcenter.md) — toasts temporales.

### Pantallas

- [ModalScreen](./modal-screen.md) — pantalla centrada con borde sobre otra pantalla.

## Testing

TextualCsharp incluye soporte de primera clase para pruebas automatizadas sin consola real:

| Tipo                       | Descripción                                                                            |
|----------------------------|----------------------------------------------------------------------------------------|
| `AppPilot`                 | Conecta un `ConsoleApp` con un driver headless y un renderer mock. Permite inyectar input y leer el buffer resultante sin TTY. |
| `HeadlessTerminalDriver`   | Driver de teclado/resize en memoria, ideal para CI.                                    |
| `MockRenderer`             | Renderer que captura el `ConsoleBuffer` en memoria en lugar de escribir en consola.    |

```csharp
await using var pilot = new AppPilot(new ConsoleApp(), width: 80, height: 24);
await pilot.StartAsync();
await pilot.SendKeyAsync(Keys.Tab);
await pilot.WaitForRenderAsync();
// inspeccionar pilot.Renderer.LastBuffer
```

El proyecto `TextualCsharp.Tests` contiene los tests unitarios (xUnit) de los subsistemas principales: `AnsiWriter`, `ConsoleBuffer`, `DiffRenderer`, `DomNode`, `LayoutResolver`, layouts, `MessagePump` y widgets.

## Caché interna

| Tipo           | Descripción                                                      |
|----------------|------------------------------------------------------------------|
| `LruCache<K,V>`| Caché LRU genérica usada internamente por el sistema de estilos. |
| `StylesCache`  | Caché de estilos resueltos por clase CSS-like de widget.         |

## Patrón general de uso de cualquier control

1. **Crear** la instancia (suele aceptar el contenido inicial en el constructor).
2. **Configurar** propiedades (`Foreground`, `Background`, `Width`, `Height`, ...).
3. **Suscribirse** a sus eventos (`Pressed`, `Changed`, `SelectionChanged`, ...).
4. **Añadirlo** como hijo de un contenedor (`container.Children.Add(widget)`).
5. **Opcional**: asignarle el foco con `screen.Focus.SetFocus(widget)`.

Consulta los archivos individuales para detalles de cada control.
