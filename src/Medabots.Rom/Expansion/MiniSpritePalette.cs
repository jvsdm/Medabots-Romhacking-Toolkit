namespace Medabots.Rom.Expansion;

/// <summary>
/// Colours of ported mini sprites (the composite battle/menu sprite). Each component uses OBJ bank =
/// the bot's palette family; the EU game loads 4 palettes into banks 0-3 and the 4th is all zero.
/// Shingata has its own mini sprite palettes, so the ported tiles are remapped: bank 3 receives an
/// extra palette fitted to the ported bots' colours and every bot takes its lowest-error bank.
/// Mirrors tools/mini_palette.py (same arithmetic order, so both produce the same ROM).
/// </summary>
internal static class MiniSpritePalette
{
    public const int EuPalettes = 0x50F498;
    public const int ExtraBank = 3;
    private const double ValueGain = 1.15;
    private const double SaturationGain = 0.90;

    /// <param name="Restyle">Apply the Shingata brightness/saturation tweak (hand-recoloured bots keep exact colours).</param>
    public sealed record Job(string Name, double[][] SourcePalette, byte[][] Components, bool Restyle = true);

    public sealed record Result(byte[] ExtraPalette, IReadOnlyList<(int Bank, byte[][] Components)> Bots);

    public static double[] Rgb(int color) => [(color & 31) * 8.0, (color >> 5 & 31) * 8.0, (color >> 10 & 31) * 8.0];

    public static int Bgr555(double[] value)
    {
        int Channel(double x) => (int)Math.Min(31, Math.Max(0, Math.Round(x / 8, MidpointRounding.ToEven)));
        return Channel(value[0]) | Channel(value[1]) << 5 | Channel(value[2]) << 10;
    }

    public static double[][] Palette(byte[] data, int offset) =>
        Enumerable.Range(0, 16).Select(i => Rgb((data[offset + 2 * i] | data[offset + 2 * i + 1] << 8) & 0x7FFF)).ToArray();

    public static Result Recolor(IReadOnlyList<Job> sourceJobs, IReadOnlyList<double[][]> basePalettes, ICollection<string> log)
    {
        var jobs = sourceJobs.Select(job => job.Restyle ? job with { SourcePalette = Restyle(job.SourcePalette) } : job).ToList();
        var usage = jobs.Select(job => ColorUsage(job.Components)).ToList();
        var chosen = Enumerable.Range(0, jobs.Count).ToList();
        List<double[][]> banks = [];
        for (var round = 0; round < 4; round++)
        {
            var samples = new List<(double[] Color, double Weight)>();
            foreach (var j in chosen)
            {
                for (var i = 1; i < 16; i++)
                {
                    if (usage[j][i] != 0)
                    {
                        samples.Add((jobs[j].SourcePalette[i], usage[j][i]));
                    }
                }
            }

            banks = [.. basePalettes, ExtraPalette(samples)];
            var newChosen = Enumerable.Range(0, jobs.Count)
                .Where(j => BestBank(jobs[j].SourcePalette, usage[j], banks) == ExtraBank)
                .ToList();
            if (newChosen.SequenceEqual(chosen))
            {
                break;
            }

            chosen = newChosen;
        }

        var bots = new List<(int, byte[][])>();
        for (var j = 0; j < jobs.Count; j++)
        {
            var source = jobs[j].SourcePalette;
            var bank = BestBank(source, usage[j], banks);
            var lut = new int[16];
            for (var i = 1; i < 16; i++)
            {
                lut[i] = Nearest(source[i], banks[bank]);
            }

            var components = jobs[j].Components
                .Select(component => component.Select(b => (byte)(lut[b & 15] | lut[b >> 4] << 4)).ToArray())
                .ToArray();
            bots.Add((bank, components));
            var total = usage[j].Sum();
            log.Add($"mini sprite of {jobs[j].Name}: bank {bank}, mean error {Cost(source, usage[j], banks[bank]) / Math.Max(1, total):F0}");
        }

        var blob = new byte[32];
        for (var i = 0; i < 16; i++)
        {
            var color = Bgr555(banks[ExtraBank][i]);
            blob[2 * i] = (byte)color;
            blob[2 * i + 1] = (byte)(color >> 8);
        }

        return new Result(blob, bots);
    }

    /// <summary>
    /// Maps one bot's mini sprite onto palettes already in the ROM (no new extra palette, so the other
    /// bots keep their colours): picks the lowest-error bank and remaps the component pixels.
    /// </summary>
    public static (int Bank, byte[][] Components) RemapToExistingBanks(Job job, IReadOnlyList<double[][]> banks)
    {
        var usage = ColorUsage(job.Components);
        var bank = BestBank(job.SourcePalette, usage, banks);
        var lut = new int[16];
        for (var i = 1; i < 16; i++)
        {
            lut[i] = Nearest(job.SourcePalette[i], banks[bank]);
        }

        var components = job.Components
            .Select(component => component.Select(b => (byte)(lut[b & 15] | lut[b >> 4] << 4)).ToArray())
            .ToArray();
        return (bank, components);
    }

    private static int BestBank(double[][] source, double[] usage, IReadOnlyList<double[][]> banks)
    {
        var best = 0;
        var bestCost = Cost(source, usage, banks[0]);
        for (var k = 1; k < banks.Count; k++)
        {
            var cost = Cost(source, usage, banks[k]);
            if (cost < bestCost)
            {
                best = k;
                bestCost = cost;
            }
        }

        return best;
    }

    private static double[][] Restyle(double[][] palette) => palette.Select(color =>
    {
        var (h, s, v) = RgbToHsv(color[0] / 255, color[1] / 255, color[2] / 255);
        var (r, g, b) = HsvToRgb(h, Math.Min(1, s * SaturationGain), Math.Min(1, v * ValueGain));
        return new[] { r * 255, g * 255, b * 255 };
    }).ToArray();

    private static double[] ColorUsage(byte[][] components)
    {
        var count = new double[16];
        foreach (var component in components)
        {
            foreach (var b in component)
            {
                count[b & 15]++;
                count[b >> 4]++;
            }
        }

        count[0] = 0;
        return count;
    }

    private static double Norm(double[] a, double[] b)
    {
        double x = a[0] - b[0], y = a[1] - b[1], z = a[2] - b[2];
        return Math.Sqrt(x * x + y * y + z * z);
    }

    private static int Nearest(double[] color, double[][] palette)
    {
        var best = 1;
        var bestDistance = Norm(color, palette[1]);
        for (var i = 2; i < palette.Length; i++)
        {
            var distance = Norm(color, palette[i]);
            if (distance < bestDistance)
            {
                best = i;
                bestDistance = distance;
            }
        }

        return best;
    }

    private static double Cost(double[][] source, double[] usage, double[][] palette)
    {
        double sum = 0;
        for (var i = 1; i < 16; i++)
        {
            if (usage[i] != 0)
            {
                sum += usage[i] * Norm(source[i], palette[Nearest(source[i], palette)]);
            }
        }

        return sum;
    }

    // 15 colours by pixel-weighted k-means; deterministic seeding (farthest point).
    private static double[][] ExtraPalette(List<(double[] Color, double Weight)> samples)
    {
        var x = samples.Select(s => s.Color).ToArray();
        var w = samples.Select(s => s.Weight + 1e-6).ToArray();
        var centers = new List<double[]> { x[ArgMax(w)] };
        while (centers.Count < 15)
        {
            var d = new double[x.Length];
            for (var i = 0; i < x.Length; i++)
            {
                d[i] = centers.Min(c => Norm(x[i], c)) * Math.Sqrt(w[i]);
            }

            centers.Add(x[ArgMax(d)]);
        }

        var c = centers.Select(center => (double[])center.Clone()).ToArray();
        for (var iteration = 0; iteration < 60; iteration++)
        {
            var label = new int[x.Length];
            for (var i = 0; i < x.Length; i++)
            {
                var best = 0;
                var bestDistance = double.MaxValue;
                for (var k = 0; k < c.Length; k++)
                {
                    double dx = x[i][0] - c[k][0], dy = x[i][1] - c[k][1], dz = x[i][2] - c[k][2];
                    var distance = dx * dx + dy * dy + dz * dz;
                    if (distance < bestDistance)
                    {
                        best = k;
                        bestDistance = distance;
                    }
                }

                label[i] = best;
            }

            for (var k = 0; k < c.Length; k++)
            {
                double weight = 0, sx = 0, sy = 0, sz = 0;
                for (var i = 0; i < x.Length; i++)
                {
                    if (label[i] == k)
                    {
                        weight += w[i];
                        sx += x[i][0] * w[i];
                        sy += x[i][1] * w[i];
                        sz += x[i][2] * w[i];
                    }
                }

                if (weight > 0)
                {
                    c[k] = [sx / weight, sy / weight, sz / weight];
                }
            }
        }

        return [[0, 0, 0], .. c.Select(center => Rgb(Bgr555(center)))];
    }

    private static int ArgMax(double[] values)
    {
        var best = 0;
        for (var i = 1; i < values.Length; i++)
        {
            if (values[i] > values[best])
            {
                best = i;
            }
        }

        return best;
    }

    // Python colorsys.
    private static (double H, double S, double V) RgbToHsv(double r, double g, double b)
    {
        var maxc = Math.Max(r, Math.Max(g, b));
        var minc = Math.Min(r, Math.Min(g, b));
        if (minc == maxc)
        {
            return (0, 0, maxc);
        }

        var range = maxc - minc;
        var s = range / maxc;
        double rc = (maxc - r) / range, gc = (maxc - g) / range, bc = (maxc - b) / range;
        var h = r == maxc ? bc - gc : g == maxc ? 2.0 + rc - bc : 4.0 + gc - rc;
        return (PythonMod(h / 6.0, 1.0), s, maxc);
    }

    private static (double R, double G, double B) HsvToRgb(double h, double s, double v)
    {
        if (s == 0)
        {
            return (v, v, v);
        }

        var i = (int)(h * 6.0);
        var f = h * 6.0 - i;
        double p = v * (1.0 - s), q = v * (1.0 - s * f), t = v * (1.0 - s * (1.0 - f));
        return (i % 6) switch
        {
            0 => (v, t, p),
            1 => (q, v, p),
            2 => (p, v, t),
            3 => (p, q, v),
            4 => (t, p, v),
            _ => (v, p, q),
        };
    }

    private static double PythonMod(double x, double m)
    {
        var mod = x % m;
        if (mod != 0 && (mod < 0) != (m < 0))
        {
            mod += m;
        }

        return mod;
    }
}
