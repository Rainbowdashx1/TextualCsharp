using TextualCsharp.Layout;
using Xunit;

namespace TextualCsharp.Tests;

public class LayoutResolverTests
{
    [Fact]
    public void Resolves_fixed_sizes()
    {
        var sizes = new[] { LayoutSize.Fixed(3), LayoutSize.Fixed(2) };
        var r = LayoutResolver.Resolve(sizes, 10);
        Assert.Equal(new[] { 3, 2 }, r);
    }

    [Fact]
    public void Resolves_percent_sizes()
    {
        var sizes = new[] { LayoutSize.Percent(25), LayoutSize.Percent(75) };
        var r = LayoutResolver.Resolve(sizes, 100);
        Assert.Equal(new[] { 25, 75 }, r);
    }

    [Fact]
    public void Distributes_remaining_among_fractions()
    {
        var sizes = new[] { LayoutSize.Fixed(2), LayoutSize.Fraction(1), LayoutSize.Fraction(3) };
        var r = LayoutResolver.Resolve(sizes, 10);
        Assert.Equal(2, r[0]);
        Assert.Equal(8, r[1] + r[2]); // remaining
        Assert.Equal(2, r[1]); // 1/4 of 8
        Assert.Equal(6, r[2]); // absorbs rounding
    }

    [Fact]
    public void Auto_uses_measurements()
    {
        var sizes = new[] { LayoutSize.Auto(), LayoutSize.Fraction(1) };
        var r = LayoutResolver.Resolve(sizes, 10, new[] { 4, 0 });
        Assert.Equal(4, r[0]);
        Assert.Equal(6, r[1]);
    }

    [Fact]
    public void Trims_overflow_starting_from_fractions()
    {
        var sizes = new[] { LayoutSize.Fixed(5), LayoutSize.Fraction(1) };
        var r = LayoutResolver.Resolve(sizes, 3);
        // Fixed 5 overflows; Fraction gets 0; then Fixed reduced from 5 to 3.
        Assert.Equal(3, r[0] + r[1]);
    }
}
