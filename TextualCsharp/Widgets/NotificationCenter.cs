using TextualCsharp.Core;
using TextualCsharp.Core.Geometry;
using TextualCsharp.Rendering;

namespace TextualCsharp.Widgets;

/// <summary>Severidad de una notificación.</summary>
public enum NotificationLevel { Info, Success, Warning, Error }

/// <summary>Una notificación con tiempo de expiración.</summary>
public sealed record Notification(
    string Message,
    NotificationLevel Level,
    DateTime ExpiresAtUtc);

/// <summary>
/// Centro de notificaciones (toasts) que se renderizan sobre el contenido.
/// Equivalente a <c>textual.notifications</c>.
/// </summary>
public sealed class NotificationCenter : Widget
{
    private readonly List<Notification> _items = new();

    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(3);

    public void Notify(string message, NotificationLevel level = NotificationLevel.Info, TimeSpan? timeout = null)
    {
        _items.Add(new Notification(message, level, DateTime.UtcNow + (timeout ?? DefaultTimeout)));
        Invalidate();
    }

    public void Tick()
    {
        var now = DateTime.UtcNow;
        int before = _items.Count;
        _items.RemoveAll(n => n.ExpiresAtUtc <= now);
        if (_items.Count != before) Invalidate();
    }

    private static (Color Fg, Color Bg, char Icon) Style(NotificationLevel l) => l switch
    {
        NotificationLevel.Success => (Color.Black, Color.FromRgb(120, 220, 120), '+'),
        NotificationLevel.Warning => (Color.Black, Color.FromRgb(220, 200, 80), '!'),
        NotificationLevel.Error => (Color.White, Color.FromRgb(200, 60, 60), 'x'),
        _ => (Color.White, Color.FromRgb(60, 100, 200), 'i'),
    };

    public override IEnumerable<Strip> Render(Size size)
    {
        if (size.Width <= 0 || size.Height <= 0) yield break;
        // Renderizamos las últimas notificaciones desde la esquina superior derecha.
        var visible = _items.TakeLast(size.Height).ToList();
        for (int row = 0; row < size.Height; row++)
        {
            var cells = new ConsoleCell[size.Width];
            for (int x = 0; x < size.Width; x++)
                cells[x] = new ConsoleCell(' ', Color.Default, Color.Default, StyleFlags.None);

            if (row < visible.Count)
            {
                var n = visible[row];
                var (fg, bg, icon) = Style(n.Level);
                string text = $" {icon} {n.Message} ";
                if (text.Length > size.Width) text = text[..size.Width];
                int start = size.Width - text.Length;
                for (int i = 0; i < text.Length; i++)
                    cells[start + i] = new ConsoleCell(text[i], fg, bg, StyleFlags.Bold);
            }
            yield return new Strip(cells);
        }
    }
}
