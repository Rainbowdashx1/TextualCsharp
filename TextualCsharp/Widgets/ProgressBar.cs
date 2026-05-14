using TextualCsharp.Core;
using TextualCsharp.Core.Geometry;
using TextualCsharp.Rendering;

namespace TextualCsharp.Widgets;

/// <summary>
/// Barra de progreso. Si <see cref="Total"/> es <c>null</c>, se renderiza como
/// spinner indeterminado. Equivalente a <c>textual.widgets.ProgressBar</c>.
/// </summary>
public sealed class ProgressBar : Widget
{
    private static readonly char[] SpinnerFrames = { '|', '/', '-', '\\' };
    private int _spinnerFrame;
    private double _progress;
    private double? _total = 100;

    public ProgressBar()
    {
        Height = Layout.LayoutSize.Fixed(1);
    }

    public double? Total
    {
        get => _total;
        set { _total = value; Invalidate(); }
    }

    public double Progress
    {
        get => _progress;
        set
        {
            var v = value;
            if (_total is { } t) v = Math.Clamp(v, 0, t);
            if (Math.Abs(_progress - v) < double.Epsilon) return;
            _progress = v;
            Invalidate();
        }
    }

    /// <summary>Avanza el frame del spinner (llamar desde un timer si es indeterminado).</summary>
    public void Tick()
    {
        _spinnerFrame = (_spinnerFrame + 1) % SpinnerFrames.Length;
        Invalidate();
    }

    public Color BarForeground { get; set; } = Color.FromRgb(80, 200, 120);
    public Color BarBackground { get; set; } = Color.FromRgb(40, 40, 40);
    public Color TextForeground { get; set; } = Color.White;

    public override IEnumerable<Strip> Render(Size size)
    {
        if (size.Width <= 0 || size.Height <= 0) yield break;
        var cells = new ConsoleCell[size.Width];

        if (_total is null)
        {
            // Indeterminado: spinner + texto.
            for (int i = 0; i < size.Width; i++)
                cells[i] = new ConsoleCell(' ', TextForeground, BarBackground, StyleFlags.None);
            string label = $" {SpinnerFrames[_spinnerFrame]} working... ";
            int start = Math.Max(0, (size.Width - label.Length) / 2);
            for (int i = 0; i < label.Length && start + i < size.Width; i++)
                cells[start + i] = new ConsoleCell(label[i], TextForeground, BarBackground, StyleFlags.Bold);
        }
        else
        {
            double pct = _total.Value <= 0 ? 0 : _progress / _total.Value;
            int filled = (int)Math.Round(pct * size.Width);
            string text = $" {(int)(pct * 100)}% ";
            int textStart = Math.Max(0, (size.Width - text.Length) / 2);
            for (int i = 0; i < size.Width; i++)
            {
                bool inFill = i < filled;
                char glyph = ' ';
                int ti = i - textStart;
                if (ti >= 0 && ti < text.Length) glyph = text[ti];
                cells[i] = new ConsoleCell(
                    glyph,
                    TextForeground,
                    inFill ? BarForeground : BarBackground,
                    StyleFlags.Bold);
            }
        }

        yield return new Strip(cells);
        for (int i = 1; i < size.Height; i++)
            yield return Strip.Filled(size.Width, new ConsoleCell(' ', TextForeground, BarBackground, StyleFlags.None));
    }
}
