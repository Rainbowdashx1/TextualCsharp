# ModalScreen

Pantalla modal centrada con borde sobre la pantalla anterior. Útil para diálogos
de confirmación, formularios o paneles "Acerca de".

Hereda de [`Screen`](./screen.md).

## Constructor

```csharp
new ModalScreen(Widget content, string? title = null, int? width = null, int? height = null)
```

- `content` — widget a centrar dentro del modal.
- `title`   — título del borde (opcional).
- `width` / `height` — tamaño fijo del modal en celdas (opcional; si se omiten se calcula).

Por defecto añade el binding `escape -> close`.

## Ejemplo

```csharp
Screen BuildAbout()
{
	var body = new VerticalContainer();
	body.Children.Add(new Label("[bold]Acerca de[/]"));
	body.Children.Add(new SpacerWidget { Height = LayoutSize.Fixed(1) });
	body.Children.Add(new Label("Aplicacion de demo."));

	var modal = new ModalScreen(body, title: " Acerca de ", width: 50, height: 9);
	modal.Bindings.Add("enter", "close");
	return modal;
}

// En la app:
app.ActionInvoked += async a =>
{
	if (a == "about") await app.PushScreenAsync(BuildAbout());
	if (a == "close") await app.PopScreenAsync();
};
```
