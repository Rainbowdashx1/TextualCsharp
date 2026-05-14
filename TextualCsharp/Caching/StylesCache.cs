using TextualCsharp.Widgets;

namespace TextualCsharp.Caching;

/// <summary>
/// Cache global de spans de markup ya parseados. Acelera labels que repiten
/// la misma cadena de markup. Equivalente a <c>textual._styles_cache</c>.
/// </summary>
public static class StylesCache
{
    private static readonly LruCache<string, IReadOnlyList<Markup.Span>> _cache = new(1024);

    public static IReadOnlyList<Markup.Span> Parse(string markup)
        => _cache.GetOrAdd(markup, Markup.Parse);

    public static long Hits => _cache.Hits;
    public static long Misses => _cache.Misses;
    public static int Count => _cache.Count;
    public static void Clear() => _cache.Clear();
}
