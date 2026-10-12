using System.Diagnostics.CodeAnalysis;

namespace Medabots.Rom.Text;

/// <summary>Dialogue font of a ROM (file offsets). Italic font and widths sit at fixed distances in every release.</summary>
public sealed record FontLayout(int Font, int Widths)
{
    public int ItalicFont => Font + 0x5D40;

    public int ItalicWidths => Widths + 0x200;

    /// <summary>Normal font as the game draws it: the code (glyph &lt;&lt; 6 + pointer, 0x08040132) points 8 bytes
    /// before <see cref="Font"/>. Glyph pixels are drawn from here; widths are measured from <see cref="Font"/>.</summary>
    public int GameFont => Font - 8;

    public int GameItalicFont => ItalicFont - 8;
}

/// <summary>
/// PT-BR extension of the dialogue fonts (European ROMs): new glyphs in the original blank slots (0x4F õ, 0x7D Ã,
/// 0x7E ã, 0x7F Õ) in the normal and italic fonts, and widths for 0x4F-0x7F (zero in the original ROM) computed
/// from the glyph pixels with the rule the shipped glyphs follow.
/// Located by signature: Metabee (E) font 0x4BFC6C / widths 0x3B4E08, Rokusho (E) 0x4BFC90 / 0x3B4E28.
/// Rokusho (U) font 0x4C01D0 / widths 0x3B4C88 is found too, but holds only 0x00-0x4E (no accents; 0x50 onward is
/// other data) and its italic font is not at +0x5D40; (U) ROMs are rejected by <see cref="IsEuropean"/>.
/// </summary>
public static class ExtendedFont
{
    public const int FontOffset = 0x4BFC6C;
    public const int WidthTableOffset = 0x3B4E08;
    public const int GlyphSize = 64;
    public const int GlyphCount = 128;
    public const int FirstWidthGlyph = 0x4F;

    // GBA header region letter (game code 0xAC-0xAF): P = Europe
    private const int RegionOffset = 0xAF;

    private const byte BlankPair = 0x11;
    private const int Background = 1;

    /// <summary>Dialogue palette (RGB555), shared by the normal and italic fonts.</summary>
    public static readonly ushort[] Palette =
    [
        0x0000, 0x27DF, 0x237D, 0x1F3B, 0x1EFA, 0x1AB8, 0x1676, 0x1635,
        0x11F3, 0x0DB1, 0x0D70, 0x092E, 0x04EC, 0x04AB, 0x0069, 0x0028,
    ];

    public const string Summary =
        "Use extended font (PT-BR, European ROMs only)\n\n" +
        "Glyphs written over the original blank slots, in the normal and in the italic font (8x16, 4bpp):\n" +
        "  0x4F  õ\n  0x7D  Ã\n  0x7E  ã\n  0x7F  Õ\n\n" +
        "Widths of glyphs 0x4F-0x7F (zero in the original ROM, so accented letters were drawn with no spacing) " +
        "are computed from the pixels of each glyph:\n\n" +
        "Each glyph has 2 bytes in its width table.\n" +
        "  Byte 1, advance: how many pixels the pen moves after drawing the glyph. The glyph is drawn from x = 0 " +
        "of its 8-pixel cell; the next glyph starts at x = advance. Ink right of that line is overdrawn by the " +
        "next letter.\n" +
        "  Normal font: advance = last inked column + 2 (one blank column between letters).\n" +
        "  Italic font: advance = last inked column + 1 (the slant already separates letters).\n" +
        "  Byte 2, vertical class (normal font): 0 = ink above row 2 (b, d, f, h, accented capitals), " +
        "1 = rows 2-9, 2 = reaches row 10 (comma, cedilla), 3 = reaches row 11 (g, p, q, y). Rows 12-15 are ignored. Italic: always 0.\n\n" +
        "Italic font repair: the European ROMs lost the dark shades of the italic font (palette B-F stored as 0, " +
        "so italic text fades into the box). Each lost pixel gets back a dark shade (F/E/D by inked neighbours). " +
        "Applied before the extension; skipped when the italic font is not damaged. Where the italic slots already " +
        "hold glyphs (Metabee/Rokusho E ship õ Ã ã Õ in italic), those glyphs are kept and only get widths.\n\n" +
        "In the sheet, the grid marks the 8x16 cells and the red line marks the advance of each glyph.\n\n" +
        "Font and tables are found by signature (Metabee (E) font 0x4BFC6C / widths 0x3B4E08, Rokusho (E) " +
        "0x4BFC90 / 0x3B4E28). Skipped when the slots already hold other glyphs.";

    // Widths of glyphs 0x00-0x4E as shipped
    internal static readonly byte[] WidthSignature = Convert.FromHexString(
        "0401060105010601060105010501060106010401060105010501070106010701050107010601060106010601060108010601060106010601050005010500060104000503050002010401050102010601050106010403050304010401040104010501060106010503050106010501060106010601050105010601050106010401040103020200060105010401060104010400060106010801080108010801");

    // Glyphs 0x01-0x03 (A, B, C)
    internal static readonly byte[] FontSignature = Convert.FromHexString(
        "111111111111111111C1111111FC1411518F1A11C1151F11E9FF1F11BD111F116C111A11111111111111111111111111111111111111111111111111111111111111111111111111FD1A11119E9D11111EB91111CF4E1111BFBD11111FE41111FD9E111188151111111111111111111111111111111111111111111111111111111111111111111111D61E11617F1B11C5181111F7111111CA111111E8831B11A1DF111111111111111111111111111111111111111111111111111111111111");

    private static readonly Dictionary<int, byte[]> GlyphData = new()
    {
        [0x4F] = Convert.FromHexString("1111111111111111D31D1D113DD3131111181111E6CE1111BBC11611C8D11611B49F111111111111111111111111111111111111111111111111111111111111"),
        [0x7D] = Convert.FromHexString("31DDD111D1313D111111111111FC1411518F1A11C1151F11E9FF1F11BD111F116C111A1111111111111111111111111111111111111111111111111111111111"),
        [0x7E] = Convert.FromHexString("1111111111111111D31D1D111DD3131111131111E9BE11112EA611115ED91211F7AD191111111111111111111111111111111111111111111111111111111111"),
        [0x7F] = Convert.FromHexString("11DDD111D1333D1121DB1A11E119A811C711D1114C11D1111D11A311B8424C1181CE181111111111111111111111111111111111111111111111111111111111"),
    };

    private static readonly Dictionary<int, byte[]> ItalicGlyphData = new()
    {
        [0x4F] = Convert.FromHexString("1111111111BEC611C1B61E111114141171EE1311F5D31911CBF116118DE81211E54E111111111111111111111111111111111111111111111111111111111111"),
        [0x7D] = Convert.FromHexString("C1B61E1111511B1111E11F1111E71F11117E1E11815D1D11E1FF1B11D8E71A114EE1161111111111111111111111111111111111111111111111111111111111"),
        [0x7E] = Convert.FromHexString("1111111111BEC611C1B61E111116161111EE1A1111A91D11A1FD1A11E6F31511E6CE141111111111111111111111111111111111111111111111111111111111"),
        [0x7F] = Convert.FromHexString("116CEB1111911A1111FCDE11714FF711E11BE911F414BD11F8415F11E8E11C11B3DE111111111111111111111111111111111111111111111111111111111111"),
    };

    public static IReadOnlyCollection<int> Glyphs => GlyphData.Keys;

    public static ReadOnlySpan<byte> GetGlyph(int glyph, bool italic = false) =>
        (italic ? ItalicGlyphData : GlyphData).TryGetValue(glyph, out var data) ? data : ReadOnlySpan<byte>.Empty;

    public static bool IsEuropean(ReadOnlySpan<byte> rom) => rom.Length > RegionOffset && rom[RegionOffset] == (byte)'P';

    /// <summary>Dialogue font of the ROM, found by the shipped widths and glyphs A-C; null when absent.</summary>
    public static FontLayout? Locate(ReadOnlySpan<byte> rom)
    {
        var widths = rom.IndexOf(WidthSignature);
        var font = rom.IndexOf(FontSignature) - GlyphSize;
        if (widths < 0 || font < 0)
        {
            return null;
        }

        var layout = new FontLayout(font, widths);
        return layout.ItalicFont + GlyphCount * GlyphSize <= rom.Length && layout.ItalicWidths + GlyphCount * 2 <= rom.Length
            ? layout
            : null;
    }

    public static bool CanApply(ReadOnlySpan<byte> rom) => CanApply(rom, out _);

    /// <summary>European ROM, font found and the normal-font slots blank or already extended.</summary>
    public static bool CanApply(ReadOnlySpan<byte> rom, [NotNullWhen(true)] out FontLayout? layout)
    {
        layout = IsEuropean(rom) ? Locate(rom) : null;
        if (layout is null || !SlotsFree(rom, layout.Font, GlyphData))
        {
            layout = null;
            return false;
        }

        return true;
    }

    /// <summary>Glyphs and widths to write; italic glyphs only where the italic slots are free.</summary>
    public static IReadOnlyList<RomPatchAction> BuildPatches(ReadOnlySpan<byte> rom, FontLayout layout)
    {
        ArgumentNullException.ThrowIfNull(layout);
        var italic = SlotsFree(rom, layout.ItalicFont, ItalicGlyphData);
        var (mainWidths, italicWidths) = BuildWidths(rom, layout, italic);
        var actions = new List<RomPatchAction>
        {
            new(layout.Widths + FirstWidthGlyph * 2, mainWidths, "Extended font widths 0x4F-0x7F"),
            new(layout.ItalicWidths + FirstWidthGlyph * 2, italicWidths, "Extended italic font widths 0x4F-0x7F"),
        };
        foreach (var (glyph, data) in GlyphData.OrderBy(pair => pair.Key))
        {
            actions.Add(new RomPatchAction(layout.Font + glyph * GlyphSize, data, $"Extended font glyph 0x{glyph:X2}"));
        }

        if (italic)
        {
            foreach (var (glyph, data) in ItalicGlyphData.OrderBy(pair => pair.Key))
            {
                actions.Add(new RomPatchAction(layout.ItalicFont + glyph * GlyphSize, data, $"Extended italic glyph 0x{glyph:X2}"));
            }
        }

        return actions;
    }

    /// <summary>Copy of the ROM as exported with the extended font: italic font repaired, then the extension.</summary>
    public static byte[] Preview(ReadOnlySpan<byte> rom, FontLayout layout)
    {
        ArgumentNullException.ThrowIfNull(layout);
        var copy = rom.ToArray();
        ItalicFontRepair.Repair(copy, layout.ItalicFont, out _)?.CopyTo(copy, layout.ItalicFont);
        foreach (var action in BuildPatches(copy, layout))
        {
            action.Data.CopyTo(copy, action.Offset);
        }

        return copy;
    }

    /// <summary>Width entries (advance, class) for glyphs 0x4F-0x7F of both fonts, extension glyphs included.</summary>
    public static (byte[] Main, byte[] Italic) BuildWidths(ReadOnlySpan<byte> rom, FontLayout layout, bool italicExtended = true)
    {
        var main = new byte[(GlyphCount - FirstWidthGlyph) * 2];
        var italic = new byte[main.Length];
        for (var glyph = FirstWidthGlyph; glyph < GlyphCount; glyph++)
        {
            var k = (glyph - FirstWidthGlyph) * 2;
            var mainGlyph = GetGlyph(glyph);
            (main[k], main[k + 1]) = MeasureGlyph(mainGlyph.IsEmpty ? rom.Slice(layout.Font + glyph * GlyphSize, GlyphSize) : mainGlyph, false);
            var italicGlyph = italicExtended ? GetGlyph(glyph, true) : ReadOnlySpan<byte>.Empty;
            (italic[k], italic[k + 1]) = MeasureGlyph(italicGlyph.IsEmpty ? rom.Slice(layout.ItalicFont + glyph * GlyphSize, GlyphSize) : italicGlyph, true);
        }

        return (main, italic);
    }

    /// <summary>Advance and vertical class of one 8x16 glyph; (0, 0) when blank.</summary>
    public static (byte Advance, byte Class) MeasureGlyph(ReadOnlySpan<byte> glyph, bool italic)
    {
        // Rows 12-15 skipped: no shipped glyph reaches them, some accented glyphs carry stray pixels there
        int right = -1, top = 16, bottom = -1;
        for (var y = 0; y < 12; y++)
        {
            for (var x = 0; x < 8; x++)
            {
                if (Pixel(glyph, x, y) == Background)
                {
                    continue;
                }

                right = Math.Max(right, x);
                top = Math.Min(top, y);
                bottom = Math.Max(bottom, y);
            }
        }

        if (right < 0)
        {
            return (0, 0);
        }

        if (italic)
        {
            return ((byte)(right + 1), 0);
        }

        byte cls = bottom >= 11 ? (byte)3 : top < 2 ? (byte)0 : bottom >= 10 ? (byte)2 : (byte)1;
        return ((byte)(right + 2), cls);
    }

    /// <summary>Palette index of a pixel of a 64-byte glyph (two 4bpp tiles, top then bottom).</summary>
    public static int Pixel(ReadOnlySpan<byte> glyph, int x, int y)
    {
        var b = glyph[(y / 8) * 32 + (y % 8) * 4 + x / 2];
        return (x & 1) == 1 ? b >> 4 : b & 15;
    }

    private static bool SlotsFree(ReadOnlySpan<byte> rom, int font, Dictionary<int, byte[]> glyphs)
    {
        foreach (var (glyph, data) in glyphs)
        {
            var slot = rom.Slice(font + glyph * GlyphSize, GlyphSize);
            if (!slot.SequenceEqual(data) && slot.IndexOfAnyExcept(BlankPair) >= 0)
            {
                return false;
            }
        }

        return true;
    }
}
