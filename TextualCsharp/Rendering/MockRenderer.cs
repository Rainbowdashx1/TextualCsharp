using TextualCsharp.Core;

namespace TextualCsharp.Rendering;

/// <summary>
/// Renderer "mock" que acumula cambios en memoria. Útil para tests headless.
/// </summary>
public sealed class MockRenderer : IRenderer
{
    private readonly List<CellChange> _all = new();
    private readonly Dictionary<(int X, int Y), ConsoleCell> _state = new();

    public IReadOnlyList<CellChange> AllChanges => _all;
    public int ClearCount { get; private set; }
    public int FlushCount { get; private set; }
    public int ApplyCount { get; private set; }

    public void Apply(IReadOnlyList<CellChange> changes)
    {
        ApplyCount++;
        foreach (var ch in changes)
        {
            _all.Add(ch);
            _state[(ch.X, ch.Y)] = ch.Cell;
        }
    }

    public void Clear()
    {
        ClearCount++;
        _state.Clear();
    }

    public void Flush() => FlushCount++;

    /// <summary>Obtiene la celda visible en (x, y) (la última aplicada).</summary>
    public ConsoleCell GetCell(int x, int y) =>
        _state.TryGetValue((x, y), out var c) ? c : ConsoleCell.Empty;

    public string GetTextLine(int y, int width)
    {
        var chars = new char[width];
        for (int x = 0; x < width; x++)
            chars[x] = _state.TryGetValue((x, y), out var c) ? c.Glyph : ' ';
        return new string(chars).TrimEnd();
    }
}
