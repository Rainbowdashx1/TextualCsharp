using TextualCsharp.Core;
using TextualCsharp.Core.Geometry;
using TextualCsharp.Rendering;
using TextualCsharp.Widgets;
using Xunit;

namespace TextualCsharp.Tests;

public class WidgetTests
{
    [Fact]
    public void TextWidget_renders_text_padded_to_width()
    {
        var t = new TextWidget("hi") { Region = new Region(0, 0, 5, 1) };
        var strips = t.Render(new Size(5, 1)).ToArray();
        Assert.Single(strips);
        Assert.Equal(5, strips[0].Width);
        Assert.Equal('h', strips[0][0].Glyph);
        Assert.Equal('i', strips[0][1].Glyph);
        Assert.Equal(' ', strips[0][2].Glyph);
    }

    [Fact]
    public void TextWidget_truncates_when_overflowing()
    {
        var t = new TextWidget("hello world");
        var strips = t.Render(new Size(5, 1)).ToArray();
        Assert.Equal(5, strips[0].Width);
        Assert.Equal('h', strips[0][0].Glyph);
        Assert.Equal('o', strips[0][4].Glyph);
    }

    [Fact]
    public void BorderWidget_draws_box_corners()
    {
        var b = new BorderWidget { Kind = BoxDrawing.BorderKind.Light };
        var strips = b.Render(new Size(4, 3)).ToArray();
        Assert.Equal(3, strips.Length);
        Assert.Equal(BoxDrawing.LightTopLeft, strips[0][0].Glyph);
        Assert.Equal(BoxDrawing.LightTopRight, strips[0][3].Glyph);
        Assert.Equal(BoxDrawing.LightBottomLeft, strips[2][0].Glyph);
        Assert.Equal(BoxDrawing.LightBottomRight, strips[2][3].Glyph);
        Assert.Equal(BoxDrawing.LightVertical, strips[1][0].Glyph);
        Assert.Equal(BoxDrawing.LightVertical, strips[1][3].Glyph);
    }

    [Fact]
    public void Widget_Paint_writes_to_buffer()
    {
        var t = new TextWidget("ab") { Region = new Region(1, 1, 2, 1) };
        var buf = new ConsoleBuffer(5, 3);
        t.Paint(buf);
        Assert.Equal('a', buf[1, 1].Glyph);
        Assert.Equal('b', buf[2, 1].Glyph);
        Assert.False(t.IsDirty);
    }

    [Fact]
    public void Invalidate_sets_IsDirty()
    {
        var t = new TextWidget("x") { Region = new Region(0, 0, 5, 1) };
        t.Paint(new ConsoleBuffer(5, 1));
        Assert.False(t.IsDirty);
        t.Text = "y";
        Assert.True(t.IsDirty);
    }

    [Fact]
    public void WidgetTree_HitTest_returns_deepest_match()
    {
        var root = new SpacerWidget { Region = new Region(0, 0, 10, 10) };
        var inner = new TextWidget("x") { Region = new Region(2, 2, 3, 3) };
        root.Children.Add(inner);
        var tree = new WidgetTree(root);
        Assert.Same(inner, tree.HitTest(3, 3));
        Assert.Same(root, tree.HitTest(0, 0));
        Assert.Null(tree.HitTest(99, 99));
    }
}
