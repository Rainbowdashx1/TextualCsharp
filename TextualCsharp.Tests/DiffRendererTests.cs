using TextualCsharp.Core;
using TextualCsharp.Rendering;
using Xunit;

namespace TextualCsharp.Tests;

public class DiffRendererTests
{
    [Fact]
    public void Diff_WithNullPrevious_ReturnsAllCells()
    {
        var current = new ConsoleBuffer(3, 2);
        var changes = DiffRenderer.Diff(null, current);
        Assert.Equal(6, changes.Count);
    }

    [Fact]
    public void Diff_WithIdenticalBuffers_ReturnsEmpty()
    {
        var a = new ConsoleBuffer(4, 2);
        var b = new ConsoleBuffer(4, 2);
        a.DrawText(0, 0, "test");
        b.DrawText(0, 0, "test");
        var changes = DiffRenderer.Diff(a, b);
        Assert.Empty(changes);
    }

    [Fact]
    public void Diff_ReturnsOnlyChangedCells()
    {
        var previous = new ConsoleBuffer(5, 1);
        var current = new ConsoleBuffer(5, 1);
        previous.DrawText(0, 0, "AAAAA");
        current.DrawText(0, 0, "AABAA");

        var changes = DiffRenderer.Diff(previous, current);
        Assert.Single(changes);
        Assert.Equal(new CellChange(2, 0, new ConsoleCell('B', Color.Default, Color.Default, StyleFlags.None)), changes[0]);
    }

    [Fact]
    public void Diff_WhenStyleChanges_DetectsDifference()
    {
        var previous = new ConsoleBuffer(2, 1);
        var current = new ConsoleBuffer(2, 1);
        previous.DrawText(0, 0, "Hi");
        current.DrawText(0, 0, "Hi", style: StyleFlags.Bold);

        var changes = DiffRenderer.Diff(previous, current);
        Assert.Equal(2, changes.Count);
        Assert.All(changes, c => Assert.Equal(StyleFlags.Bold, c.Cell.Style));
    }

    [Fact]
    public void Diff_WhenSizeChanges_TreatsAsFullRedraw()
    {
        var previous = new ConsoleBuffer(2, 2);
        var current = new ConsoleBuffer(3, 3);
        var changes = DiffRenderer.Diff(previous, current);
        Assert.Equal(9, changes.Count);
    }
}
