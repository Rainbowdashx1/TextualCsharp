using TextualCsharp.App;
using TextualCsharp.Core;
using TextualCsharp.Layout;
using TextualCsharp.Widgets;

namespace TextualCsharp.Widgets;

/// <summary>
/// Modal de advertencia que se muestra automáticamente cuando se detecta un
/// terminal legacy (conhost sin soporte VT, QuickEdit activo en SSH, etc.).
/// <para>
/// El usuario puede cerrarlo pulsando <b>Enter</b>, <b>Espacio</b> o <b>Escape</b>,
/// o haciendo click en el botón «Entendido».
/// </para>
/// </summary>
public sealed class LegacyTerminalWarningModal : ModalScreen
{
    /// <summary>
    /// Construye el modal. <paramref name="app"/> se usa para hacer
    /// <see cref="ConsoleApp.PopScreenAsync"/> al confirmar.
    /// </summary>
    public LegacyTerminalWarningModal(ConsoleApp app)
        : base(BuildContent(app), title: " Terminal legacy detectado ", width: 66, height: 24)
    {
        // Escape también cierra el modal (heredado de ModalScreen).
    }

    private static Widget BuildContent(ConsoleApp app)
    {
        var body = new VerticalContainer();
        body.Padding = Padding.Symmetric(1, 1);

        // ── Texto de advertencia ──────────────────────────────────────────────
        var text = new WrapTextWidget(
            "[bold yellow]Tu cliente de terminal no respondió a las secuencias VT[/]\n" +
            "[bold yellow]estándar (Device Attributes ESC[c).[/]\n\n" +
            "Si te conectas por [bold]SSH desde Windows[/] usando [bold]conhost.exe[/]\n" +
            "(cmd / PowerShell clásico), el modo [bold red]QuickEdit[/] captura los\n" +
            "clicks antes de que lleguen a la app, congelando la pantalla.\n\n" +
            "[bold cyan]Solución recomendada[/]\n" +
            "  Usa [bold]Windows Terminal[/] como cliente SSH.\n" +
            "  Honra VT automáticamente, sin configuración extra.\n\n" +
            "[bold cyan]Solución alternativa (conhost)[/]\n" +
            "  Click derecho en barra de título → [bold]Propiedades → Opciones[/]\n" +
            "  → desmarca [bold]«Modo Edición rápida»[/] → Aceptar.\n\n" +
            "[dim]Suprime este aviso: set TEXTUALCSHARP_NO_VT_PROBE=1[/]"
        );
        text.Width  = LayoutSize.Percent(100);
        text.Height = LayoutSize.Auto();

        // ── Separador ────────────────────────────────────────────────────────
        var spacer = new SpacerWidget { Height = LayoutSize.Fixed(1) };

        // ── Botón de confirmación ────────────────────────────────────────────
        var btn = new Button("  Entendido  ");
        btn.Background        = Color.FromRgb(40, 100, 40);
        btn.FocusedBackground = Color.FromRgb(60, 160, 60);
        btn.Foreground        = Color.White;
        btn.Width  = LayoutSize.Fixed(16);
        btn.Height = LayoutSize.Fixed(3);
        btn.Pressed += async _ => await app.PopScreenAsync().ConfigureAwait(false);

        // Centrar el botón horizontalmente con un contenedor horizontal.
        var btnRow = new HorizontalContainer();
        btnRow.Height = LayoutSize.Fixed(3);
        btnRow.Children.Add(new SpacerWidget());
        btnRow.Children.Add(btn);
        btnRow.Children.Add(new SpacerWidget());

        body.Children.Add(text);
        body.Children.Add(spacer);
        body.Children.Add(btnRow);

        return body;
    }
}
