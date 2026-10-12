using System.Diagnostics.CodeAnalysis;

namespace Medabots.Rom.Text;

/// <summary>One character rasterized into an 8x16 cell: coverage 0-255 per pixel (row-major), pen advance in pixels
/// (fractional, from the font metrics), and whether ink fell outside the cell.</summary>
public sealed record RasterizedGlyph(byte[] Coverage, double Advance, bool Clipped);

/// <summary>
/// Dialogue fonts built from a TrueType/OpenType font (Font tab, "Import font"): 128 glyphs per font in the game's
/// 4bpp layout and their width table entries. The editor rasterizes the font (it owns the font engine); this class
/// turns coverage into the dialogue shades, picks the rounding of the fractional advances and writes the ROM.
/// </summary>
public sealed class ImportedFont
{
    public const int CellWidth = 8;
    public const int CellHeight = 16;

    /// <summary>Row the pen sits on: ink of letters on the baseline ends on row 10, as in the game's own font.</summary>
    public const int Baseline = 11;

    /// <summary>Height of a capital (H) in pixels, rows 4-10 as in the game's own font; sets the pixel size.</summary>
    public const int CapHeight = 7;

    /// <summary>The game wipes the next letter at some positions when an advance is above 8.</summary>
    public const int MaxAdvance = 8;

    /// <summary>Characters of glyphs 0x00-0x7F (the game's order). '·' is the game's ellipsis ("···"), drawn as '.'.</summary>
    public const string GlyphChars =
        " ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789·.,'-/:?!\"()♥£&%õ" +
        "ÄäÁáÂâÈèÉéÊêËëÎîÏïÍíÖöÔôÓóÜüÛûÙùÚúßÇçÑñ¡¿ÀàÆæÃãÕ";

    // Line lengths are compared on this text when choosing how to round the advances
    private const string SampleText =
        "Será possível··· A raríssima Medalha \"?\"!? Boa tarde. Tem um monte de livros difíceis de ler. " +
        "Ação! Então vá até a sala dele, lá no fundo. O Medabot Kuwagata venceu o Robattle por 3 a 0. " +
        "Você não pode usar essa Medapeça agora, é perigoso! WAAAH! Minha Medalha sumiu··· Cadê ela, Ikki? " +
        "Ótimo! Último ônibus às 18:30, não se atrase.";

    private const double Gamma = 0.8;

    public required string Name { get; init; }

    public double PixelSize { get; init; }

    /// <summary>128 glyphs x 64 bytes (two 4bpp tiles, top then bottom; shade 1 = background).</summary>
    public required byte[] Normal { get; init; }

    /// <summary>Pen advance of each of the 128 glyphs; 0 = not in the font (the game's glyph and width are kept).</summary>
    public required byte[] NormalAdvances { get; init; }

    public byte[]? Italic { get; init; }

    public byte[]? ItalicAdvances { get; init; }

    public IReadOnlyList<string> Warnings { get; init; } = [];

    public static char SourceChar(int glyph) => GlyphChars[glyph] == '·' ? '.' : GlyphChars[glyph];

    /// <summary>Builds the fonts from rasterized glyphs (<paramref name="rasterize"/> gets the character of each glyph).</summary>
    public static ImportedFont Build(string name, double pixelSize, Func<char, RasterizedGlyph?> rasterize, Func<char, RasterizedGlyph?>? rasterizeItalic = null)
    {
        ArgumentNullException.ThrowIfNull(rasterize);
        var warnings = new List<string>();
        var (normal, normalAdvances) = BuildFont(rasterize, "normal", warnings);
        byte[]? italic = null, italicAdvances = null;
        if (rasterizeItalic is not null)
        {
            (italic, italicAdvances) = BuildFont(rasterizeItalic, "italic", warnings);
        }

        return new ImportedFont
        {
            Name = name,
            PixelSize = pixelSize,
            Normal = normal,
            NormalAdvances = normalAdvances,
            Italic = italic,
            ItalicAdvances = italicAdvances,
            Warnings = warnings,
        };
    }

    public static bool CanApply(ReadOnlySpan<byte> rom, [NotNullWhen(true)] out FontLayout? layout) => PtBrFont.CanApply(rom, out layout);

    /// <summary>Glyphs at the game font address and the advance byte of each width entry (the class byte is kept).</summary>
    public IReadOnlyList<RomPatchAction> BuildPatches(ReadOnlySpan<byte> rom, FontLayout layout)
    {
        ArgumentNullException.ThrowIfNull(layout);
        var actions = new List<RomPatchAction>
        {
            new(layout.GameFont, Glyphs(rom, layout.GameFont, Normal, NormalAdvances), $"Imported font {Name}: normal glyphs"),
            new(layout.Widths, Widths(rom, layout.Widths, NormalAdvances), $"Imported font {Name}: normal widths"),
        };
        if (Italic is not null && ItalicAdvances is not null)
        {
            actions.Add(new RomPatchAction(layout.GameItalicFont, Glyphs(rom, layout.GameItalicFont, Italic, ItalicAdvances), $"Imported font {Name}: italic glyphs"));
            actions.Add(new RomPatchAction(layout.ItalicWidths, Widths(rom, layout.ItalicWidths, ItalicAdvances), $"Imported font {Name}: italic widths"));
        }

        return actions;
    }

    /// <summary>Copy of the ROM as exported with this font.</summary>
    public byte[] Preview(ReadOnlySpan<byte> rom, FontLayout layout)
    {
        var copy = rom.ToArray();
        foreach (var action in BuildPatches(rom, layout))
        {
            action.Data.CopyTo(copy, action.Offset);
        }

        return copy;
    }

    /// <summary>Dialogue shade of a coverage value: 1 = background, 2-F = ink (F darkest).</summary>
    public static int Shade(byte coverage) => Math.Max(1, (int)Math.Round(15 * Math.Pow(coverage / 255.0, Gamma)));

    /// <summary>Advances rounded with the offset and space width whose sample lines come closest to the font's own
    /// (fractional) lengths; each advance is 1-8.</summary>
    public static byte[] RoundAdvances(IReadOnlyList<double> advances)
    {
        ArgumentNullException.ThrowIfNull(advances);
        var sample = SampleText.Select(c => GlyphChars.IndexOf(c)).Where(g => g >= 0).ToArray();
        var exact = sample.Sum(g => advances[g]);
        byte[]? best = null;
        var bestError = double.MaxValue;
        foreach (var offset in new[] { 0.0, 0.1, -0.1, 0.2, -0.2, 0.3 })
        {
            foreach (var space in new[] { 3, 2, 4 })
            {
                var rounded = advances.Select(a => (byte)Math.Clamp((int)Math.Round(a + offset, MidpointRounding.AwayFromZero), 1, MaxAdvance)).ToArray();
                rounded[0] = (byte)space;
                var error = Math.Abs(sample.Sum(g => rounded[g]) - exact);
                if (error < bestError - 0.01)
                {
                    (best, bestError) = (rounded, error);
                }
            }
        }

        return best!;
    }

    private static (byte[] Glyphs, byte[] Advances) BuildFont(Func<char, RasterizedGlyph?> rasterize, string label, List<string> warnings)
    {
        var glyphs = new byte[ExtendedFont.GlyphCount * ExtendedFont.GlyphSize];
        var advances = new double[ExtendedFont.GlyphCount];
        var missing = new List<char>();
        var clipped = new List<char>();
        var wide = new List<char>();
        for (var glyph = 0; glyph < ExtendedFont.GlyphCount; glyph++)
        {
            var ch = SourceChar(glyph);
            var raster = rasterize(ch);
            if (raster is null)
            {
                missing.Add(GlyphChars[glyph]);
            }
            else if (raster.Clipped)
            {
                clipped.Add(GlyphChars[glyph]);
            }

            advances[glyph] = raster?.Advance ?? 0;
            if (advances[glyph] > MaxAdvance + 0.5)
            {
                wide.Add(GlyphChars[glyph]);
            }

            for (var y = 0; y < CellHeight; y++)
            {
                for (var x = 0; x < CellWidth; x++)
                {
                    var shade = raster is null ? 1 : Shade(raster.Coverage[y * CellWidth + x]);
                    var i = glyph * ExtendedFont.GlyphSize + (y / 8) * 32 + (y % 8) * 4 + x / 2;
                    glyphs[i] |= (byte)((x & 1) == 1 ? shade << 4 : shade);
                }
            }
        }

        void Warn(List<char> chars, string text)
        {
            if (chars.Count > 0)
            {
                warnings.Add($"{label}: {text}: {string.Concat(chars.Where(c => c != ' ').Distinct())}");
            }
        }

        Warn(missing, "not in the font (the game's glyph is kept)");
        Warn(clipped, "cut at the 8x16 cell");
        Warn(wide, "advance limited to 8 px");
        var rounded = RoundAdvances(advances);
        for (var glyph = 1; glyph < ExtendedFont.GlyphCount; glyph++)
        {
            if (missing.Contains(GlyphChars[glyph]))
            {
                rounded[glyph] = 0;
            }
        }

        return (glyphs, rounded);
    }

    // Characters missing from the font keep the game's glyph and width
    private static byte[] Glyphs(ReadOnlySpan<byte> rom, int font, byte[] glyphs, byte[] advances)
    {
        var data = (byte[])glyphs.Clone();
        for (var glyph = 1; glyph < ExtendedFont.GlyphCount; glyph++)
        {
            if (advances[glyph] == 0)
            {
                rom.Slice(font + glyph * ExtendedFont.GlyphSize, ExtendedFont.GlyphSize).CopyTo(data.AsSpan(glyph * ExtendedFont.GlyphSize));
            }
        }

        return data;
    }

    private static byte[] Widths(ReadOnlySpan<byte> rom, int widths, byte[] advances)
    {
        var table = rom.Slice(widths, ExtendedFont.GlyphCount * 2).ToArray();
        for (var glyph = 0; glyph < ExtendedFont.GlyphCount; glyph++)
        {
            if (advances[glyph] > 0)
            {
                table[glyph * 2] = advances[glyph];
            }
        }

        return table;
    }
}
