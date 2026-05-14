using TextualCsharp.Core;
using TextualCsharp.Core.Geometry;
using TextualCsharp.Rendering;

namespace TextualCsharp.Widgets;

/// <summary>Widget que dibuja un borde alrededor de su área. Sin contenido interior.</summary>
public sealed class BorderWidget : Widget
{
    public BoxDrawing.BorderKind Kind { get; set; } = BoxDrawing.BorderKind.Light;
    public Color Foreground { get; set; } = Color.Default;
    public Color Background { get; set; } = Color.Default;
    public string? Title { get; set; }

    public override IEnumerable<Strip> Render(Size size)
    {
        if (size.Width <= 0 || size.Height <= 0) yield break;
        var (h, v, tl, tr, bl, br) = BoxDrawing.GetGlyphs(Kind);
        int w = size.Width;

        // Top
        var topCells = new ConsoleCell[w];
        topCells[0] = new ConsoleCell(w == 1 ? v : tl, Foreground, Background, StyleFlags.None);
        for (int i = 1; i < w - 1; i++) topCells[i] = new ConsoleCell(h, Foreground, Background, StyleFlags.None);
        if (w > 1) topCells[w - 1] = new ConsoleCell(tr, Foreground, Background, StyleFlags.None);
        if (!string.IsNullOrEmpty(Title) && w > 4)
        {
            string t = Title!.Length > w - 4 ? Title[..(w - 4)] : Title;
            int start = 2;
            for (int i = 0; i < t.Length; i++)
                topCells[start + i] = new ConsoleCell(t[i], Foreground, Background, StyleFlags.Bold);
        }
        yield return new Strip(topCells);

        // Middle rows
        for (int y = 1; y < size.Height - 1; y++)
        {
            var row = new ConsoleCell[w];
            row[0] = new ConsoleCell(v, Foreground, Background, StyleFlags.None);
            for (int i = 1; i < w - 1; i++) row[i] = new ConsoleCell(' ', Foreground, Background, StyleFlags.None);
            if (w > 1) row[w - 1] = new ConsoleCell(v, Foreground, Background, StyleFlags.None);
            yield return new Strip(row);
        }

        if (size.Height > 1)
        {
            var bot = new ConsoleCell[w];
            bot[0] = new ConsoleCell(w == 1 ? v : bl, Foreground, Background, StyleFlags.None);
            for (int i = 1; i < w - 1; i++) bot[i] = new ConsoleCell(h, Foreground, Background, StyleFlags.None);
            if (w > 1) bot[w - 1] = new ConsoleCell(br, Foreground, Background, StyleFlags.None);
            yield return new Strip(bot);
        }
    }
}
