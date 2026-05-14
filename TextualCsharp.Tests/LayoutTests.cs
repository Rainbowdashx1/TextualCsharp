using TextualCsharp.Core.Geometry;
using TextualCsharp.Layout;
using TextualCsharp.Widgets;
using Xunit;

namespace TextualCsharp.Tests;

public class LayoutTests
{
    [Fact]
    public void VerticalLayout_stacks_widgets()
    {
        var a = new TextWidget("a") { Height = LayoutSize.Fixed(2) };
        var b = new TextWidget("b") { Height = LayoutSize.Fraction(1) };
        var layout = new VerticalLayout();
        var map = layout.Arrange(new[] { a, b }, new Region(0, 0, 10, 5));
        Assert.Equal(new Region(0, 0, 10, 2), a.Region);
        Assert.Equal(new Region(0, 2, 10, 3), b.Region);
        Assert.Equal(2, map.Count);
    }

    [Fact]
    public void HorizontalLayout_places_side_by_side()
    {
        var a = new TextWidget("a") { Width = LayoutSize.Fixed(3) };
        var b = new TextWidget("b") { Width = LayoutSize.Fraction(1) };
        var layout = new HorizontalLayout();
        layout.Arrange(new[] { a, b }, new Region(0, 0, 10, 4));
        Assert.Equal(new Region(0, 0, 3, 4), a.Region);
        Assert.Equal(new Region(3, 0, 7, 4), b.Region);
    }

    [Fact]
    public void GridLayout_places_widgets_in_row_major()
    {
        var cols = new[] { LayoutSize.Fixed(2), LayoutSize.Fraction(1) };
        var rows = new[] { LayoutSize.Fixed(1), LayoutSize.Fraction(1) };
        var w1 = new TextWidget("1");
        var w2 = new TextWidget("2");
        var w3 = new TextWidget("3");
        var w4 = new TextWidget("4");
        var grid = new GridLayout(cols, rows);
        grid.Arrange(new[] { w1, w2, w3, w4 }, new Region(0, 0, 10, 5));
        Assert.Equal(new Region(0, 0, 2, 1), w1.Region);
        Assert.Equal(new Region(2, 0, 8, 1), w2.Region);
        Assert.Equal(new Region(0, 1, 2, 4), w3.Region);
        Assert.Equal(new Region(2, 1, 8, 4), w4.Region);
    }

    [Fact]
    public void SpatialMap_HitTest_finds_widget()
    {
        var w = new TextWidget("x") { Region = new Region(2, 2, 3, 3) };
        var map = new MapGeometry();
        map.Set(w, w.Region);
        var spatial = SpatialMap.FromGeometry(map);
        Assert.Same(w, spatial.HitTest(3, 3));
        Assert.Null(spatial.HitTest(0, 0));
    }

    [Fact]
    public void Container_arranges_children()
    {
        var container = new VerticalContainer { Region = new Region(0, 0, 10, 6) };
        container.Children.Add(new TextWidget("a") { Height = LayoutSize.Fixed(2) });
        container.Children.Add(new TextWidget("b") { Height = LayoutSize.Fraction(1) });
        var geometry = container.Arrange();
        Assert.Equal(2, geometry.Count);
    }
}
