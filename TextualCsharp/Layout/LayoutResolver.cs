namespace TextualCsharp.Layout;

/// <summary>
/// Resuelve un conjunto de <see cref="LayoutSize"/> contra un tamaño total disponible.
/// Equivalente a <c>textual._layout_resolve.resolve</c>.
/// </summary>
/// <remarks>
/// Algoritmo:
/// 1. Asignar tamaños <c>Fixed</c> directamente.
/// 2. Asignar tamaños <c>Percent</c> = total * (percent/100).
/// 3. Asignar tamaños <c>Auto</c> usando el array de medidas mínimas (o 0 si no hay).
/// 4. Distribuir el espacio restante entre las <c>Fraction</c> según su peso.
/// 5. Recortar si la suma excede el total (priorizando Fixed > Percent > Auto > Fraction).
/// </remarks>
public static class LayoutResolver
{
    public static int[] Resolve(ReadOnlySpan<LayoutSize> sizes, int total, ReadOnlySpan<int> autoMeasurements = default)
    {
        var result = new int[sizes.Length];
        if (sizes.Length == 0 || total <= 0) return result;

        // 1 + 2: Fixed & Percent
        for (int i = 0; i < sizes.Length; i++)
        {
            switch (sizes[i].Kind)
            {
                case LayoutSizeKind.Fixed:
                    result[i] = Math.Max(0, (int)sizes[i].Value);
                    break;
                case LayoutSizeKind.Percent:
                    result[i] = Math.Max(0, (int)Math.Round(total * sizes[i].Value / 100.0));
                    break;
            }
        }

        // 3: Auto
        for (int i = 0; i < sizes.Length; i++)
        {
            if (sizes[i].Kind != LayoutSizeKind.Auto) continue;
            int m = (i < autoMeasurements.Length) ? autoMeasurements[i] : 0;
            result[i] = Math.Max(0, m);
        }

        // Espacio consumido hasta aquí
        int consumed = 0;
        double totalFraction = 0;
        for (int i = 0; i < sizes.Length; i++)
        {
            if (sizes[i].Kind == LayoutSizeKind.Fraction)
                totalFraction += Math.Max(0, sizes[i].Value);
            else
                consumed += result[i];
        }

        int remaining = Math.Max(0, total - consumed);

        // 4: Fraction
        if (totalFraction > 0 && remaining > 0)
        {
            double acc = 0;
            int distributed = 0;
            int lastFractionIndex = -1;
            for (int i = 0; i < sizes.Length; i++)
                if (sizes[i].Kind == LayoutSizeKind.Fraction) lastFractionIndex = i;

            for (int i = 0; i < sizes.Length; i++)
            {
                if (sizes[i].Kind != LayoutSizeKind.Fraction) continue;
                double share = remaining * (sizes[i].Value / totalFraction);
                acc += share;
                int cells;
                if (i == lastFractionIndex)
                {
                    cells = remaining - distributed; // absorber redondeos
                }
                else
                {
                    cells = (int)Math.Floor(acc - distributed);
                }
                if (cells < 0) cells = 0;
                result[i] = cells;
                distributed += cells;
            }
        }

        // 5: Recortar si excede (en orden inverso de prioridad)
        int sum = 0;
        for (int i = 0; i < result.Length; i++) sum += result[i];
        if (sum > total)
        {
            int overflow = sum - total;
            ReduceByKind(LayoutSizeKind.Fraction, sizes, result, ref overflow);
            if (overflow > 0) ReduceByKind(LayoutSizeKind.Auto, sizes, result, ref overflow);
            if (overflow > 0) ReduceByKind(LayoutSizeKind.Percent, sizes, result, ref overflow);
            if (overflow > 0) ReduceByKind(LayoutSizeKind.Fixed, sizes, result, ref overflow);
        }

        return result;
    }

    private static void ReduceByKind(LayoutSizeKind kind, ReadOnlySpan<LayoutSize> sizes, int[] result, ref int overflow)
    {
        for (int i = result.Length - 1; i >= 0 && overflow > 0; i--)
        {
            if (sizes[i].Kind != kind) continue;
            int take = Math.Min(result[i], overflow);
            result[i] -= take;
            overflow -= take;
        }
    }
}
