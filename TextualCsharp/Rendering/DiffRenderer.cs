using TextualCsharp.Core;

namespace TextualCsharp.Rendering;

/// <summary>
/// Calcula la diferencia entre dos <see cref="ConsoleBuffer"/> y produce solo
/// las celdas que cambiaron. Inspirado en <c>textual._compositor</c>.
/// </summary>
public static class DiffRenderer
{
    /// <summary>Devuelve la lista de celdas que difieren entre <paramref name="previous"/> y <paramref name="current"/>.</summary>
    /// <remarks>Si <paramref name="previous"/> es <c>null</c>, se considera que todo es nuevo.</remarks>
    public static List<CellChange> Diff(ConsoleBuffer? previous, ConsoleBuffer current)
    {
        ArgumentNullException.ThrowIfNull(current);

        var changes = new List<CellChange>();
        var curCells = current.Cells;
        int width = current.Width;
        int height = current.Height;

        bool fullRedraw = previous is null
            || previous.Width != width
            || previous.Height != height;

        if (fullRedraw)
        {
            for (int y = 0; y < height; y++)
            {
                int row = y * width;
                for (int x = 0; x < width; x++)
                    changes.Add(new CellChange(x, y, curCells[row + x]));
            }
            return changes;
        }

        var prevCells = previous!.Cells;
        for (int y = 0; y < height; y++)
        {
            int row = y * width;
            for (int x = 0; x < width; x++)
            {
                int i = row + x;
                if (!curCells[i].Equals(prevCells[i]))
                    changes.Add(new CellChange(x, y, curCells[i]));
            }
        }
        return changes;
    }
}
