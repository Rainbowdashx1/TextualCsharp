using System.IO;
using TextualCsharp.Core;
using TextualCsharp.Rendering;
using Xunit;

namespace TextualCsharp.Tests;

public class AnsiWriterTests
{
    [Fact]
    public void Apply_EmptyChanges_WritesNothing()
    {
        var sw = new StringWriter();
        var w = new AnsiWriter(sw);
        w.Apply(Array.Empty<CellChange>());
        Assert.Equal(string.Empty, sw.ToString());
    }

    [Fact]
    public void Apply_SingleCell_EmitsCupAndGlyph()
    {
        var sw = new StringWriter();
        var w = new AnsiWriter(sw);
        var change = new CellChange(2, 1, new ConsoleCell('X'));
        w.Apply(new[] { change });

        var output = sw.ToString();
        // CUP es 1-based: (y=1 -> fila 2, x=2 -> col 3) => ESC[2;3H
        Assert.Contains("\u001b[2;3H", output);
        Assert.Contains("X", output);
        // Reset al final.
        Assert.EndsWith("\u001b[0m", output);
    }

    [Fact]
    public void Apply_ConsecutiveCellsSameStyle_DoesNotRepeatCursorMove()
    {
        var sw = new StringWriter();
        var w = new AnsiWriter(sw);
        var changes = new[]
        {
            new CellChange(0, 0, new ConsoleCell('A')),
            new CellChange(1, 0, new ConsoleCell('B')),
            new CellChange(2, 0, new ConsoleCell('C')),
        };
        w.Apply(changes);
        var output = sw.ToString();

        // Sólo debe haber un CUP (1;1H) al principio.
        Assert.Equal(1, CountOccurrences(output, "\u001b[1;1H"));
        // Y sólo un SGR inicial + reset final => 2 ocurrencias de "m" del SGR como mucho.
        Assert.Contains("ABC", output);
    }

    private static int CountOccurrences(string haystack, string needle)
    {
        int count = 0, i = 0;
        while ((i = haystack.IndexOf(needle, i, StringComparison.Ordinal)) >= 0)
        {
            count++;
            i += needle.Length;
        }
        return count;
    }

    [Fact]
    public void Apply_RgbForeground_EmitsTruecolorSgr()
    {
        var sw = new StringWriter();
        var w = new AnsiWriter(sw);
        var cell = new ConsoleCell('Z', Color.FromRgb(10, 20, 30), Color.Default, StyleFlags.Bold);
        w.Apply(new[] { new CellChange(0, 0, cell) });

        var output = sw.ToString();
        Assert.Contains(";1;38;2;10;20;30", output);
    }

    [Fact]
    public void Apply_NonContiguousPositions_EmitsTwoCursorMoves()
    {
        var sw = new StringWriter();
        var w = new AnsiWriter(sw);
        var changes = new[]
        {
            new CellChange(0, 0, new ConsoleCell('A')),
            new CellChange(5, 2, new ConsoleCell('B')),
        };
        w.Apply(changes);

        var output = sw.ToString();
        Assert.Contains("\u001b[1;1H", output);
        Assert.Contains("\u001b[3;6H", output);
    }
}
