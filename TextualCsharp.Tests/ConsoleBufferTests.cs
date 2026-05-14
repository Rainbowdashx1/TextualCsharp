using TextualCsharp.Core;
using TextualCsharp.Rendering;
using Xunit;

namespace TextualCsharp.Tests;

public class ConsoleBufferTests
{
    [Fact]
    public void NewBuffer_IsFilledWithEmptyCells()
    {
        var buf = new ConsoleBuffer(5, 3);
        Assert.Equal(5, buf.Width);
        Assert.Equal(3, buf.Height);
        for (int y = 0; y < buf.Height; y++)
            for (int x = 0; x < buf.Width; x++)
                Assert.Equal(ConsoleCell.Empty, buf[x, y]);
    }

    [Fact]
    public void DrawText_WritesGlyphsAtPosition()
    {
        var buf = new ConsoleBuffer(10, 2);
        buf.DrawText(2, 0, "Hi", foreground: Color.Red);

        Assert.Equal('H', buf[2, 0].Glyph);
        Assert.Equal('i', buf[3, 0].Glyph);
        Assert.Equal(Color.Red, buf[2, 0].Foreground);
        Assert.Equal(' ', buf[4, 0].Glyph); // resto vacío
    }

    [Fact]
    public void DrawStrip_ClipsAtRightEdge()
    {
        var buf = new ConsoleBuffer(5, 1);
        buf.DrawText(3, 0, "ABCDE"); // sólo caben "AB"
        Assert.Equal('A', buf[3, 0].Glyph);
        Assert.Equal('B', buf[4, 0].Glyph);
    }

    [Fact]
    public void DrawStrip_ClipsAtLeftEdge()
    {
        var buf = new ConsoleBuffer(5, 1);
        buf.DrawText(-2, 0, "ABCDE"); // se descartan A, B; queda CDE en cols 0,1,2
        Assert.Equal('C', buf[0, 0].Glyph);
        Assert.Equal('D', buf[1, 0].Glyph);
        Assert.Equal('E', buf[2, 0].Glyph);
    }

    [Fact]
    public void Resize_ChangesDimensionsAndClears()
    {
        var buf = new ConsoleBuffer(2, 2);
        buf.DrawText(0, 0, "Hi");
        buf.Resize(4, 1);
        Assert.Equal(4, buf.Width);
        Assert.Equal(1, buf.Height);
        Assert.Equal(' ', buf[0, 0].Glyph);
    }
}
