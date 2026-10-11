using System.Diagnostics.CodeAnalysis;

namespace Medabots.Rom.Text;

/// <summary>Dialogue font of a ROM (file offsets). Italic font and widths sit at fixed distances in every release.</summary>
public sealed record FontLayout(int Font, int Widths)
{
    public int ItalicFont => Font + 0x5D40;

    public int ItalicWidths => Widths + 0x200;
}

/// <summary>
/// PT-BR extension of the dialogue font: new glyphs in the original blank slots (0x4F õ, 0x7D Ã, 0x7E ã, 0x7F Õ)
/// and advance widths for 0x4F-0x7F, zero in the original ROM (table by Hyd, from insert_text_v2.py).
/// The font is located by signature: Metabee (E) font 0x4BFC6C / widths 0x3B4E08, Rokusho (E) 0x4BFC90 / 0x3B4E28.
/// </summary>
public static class ExtendedFont
{
    public const int FontOffset = 0x4BFC6C;
    public const int WidthTableOffset = 0x3B4E08;
    public const int GlyphSize = 64;
    public const int FirstWidthGlyph = 0x4F;

    public const string Summary =
        "Use extended font (PT-BR)\n\n" +
        "Writes into the dialogue font (8x16 4bpp) the glyphs drawn over the original blank slots:\n" +
        "  0x4F  õ\n  0x7D  Ã\n  0x7E  ã\n  0x7F  Õ\n\n" +
        "Writes the advance widths for glyphs 0x4F-0x7F into the width table (2 bytes per glyph: " +
        "advance in pixels and vertical class). In the original ROM these entries are zero, so the accented " +
        "letters (Ä..à, Æ æ) and the new glyphs are drawn with no spacing.\n\n" +
        "The font and the table are found by signature (Metabee (E) font 0x4BFC6C / widths 0x3B4E08, " +
        "Rokusho (E) 0x4BFC90 / 0x3B4E28). The italic font is left unchanged. Skipped when the font is not " +
        "found or the slots already hold other glyphs.";

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

    // Glyphs 0x4F-0x7F: advance, vertical class
    private static readonly byte[] WidthData =
    [
        0x05, 0x01, 0x06, 0x00, 0x05, 0x01, 0x06, 0x00, 0x05, 0x01, 0x06, 0x00, 0x05, 0x01, 0x05, 0x00,
        0x05, 0x01, 0x05, 0x00, 0x05, 0x01, 0x05, 0x00, 0x05, 0x01, 0x05, 0x00, 0x05, 0x01, 0x04, 0x00,
        0x04, 0x01, 0x04, 0x00, 0x04, 0x01, 0x04, 0x00, 0x04, 0x01, 0x04, 0x00, 0x04, 0x01, 0x07, 0x00,
        0x06, 0x01, 0x06, 0x00, 0x06, 0x01, 0x06, 0x00, 0x06, 0x01, 0x06, 0x00, 0x06, 0x01, 0x06, 0x00,
        0x05, 0x01, 0x05, 0x01, 0x04, 0x01, 0x05, 0x01, 0x05, 0x01, 0x05, 0x01, 0x06, 0x01, 0x05, 0x01,
        0x05, 0x01, 0x06, 0x00, 0x05, 0x01, 0x08, 0x00, 0x05, 0x01, 0x05, 0x00, 0x06, 0x01, 0x06, 0x01,
        0x06, 0x01,
    ];

    private const byte BlankPair = 0x11;

    public static IReadOnlyCollection<int> Glyphs => GlyphData.Keys;

    public static ReadOnlySpan<byte> GetGlyph(int glyph) =>
        GlyphData.TryGetValue(glyph, out var data) ? data : ReadOnlySpan<byte>.Empty;

    public static int? GetAdvance(int glyph)
    {
        var index = (glyph - FirstWidthGlyph) * 2;
        return index >= 0 && index < WidthData.Length ? WidthData[index] : null;
    }

    /// <summary>Dialogue font of the ROM, found by the shipped widths and glyphs A-C; null when absent.</summary>
    public static FontLayout? Locate(ReadOnlySpan<byte> rom)
    {
        var widths = rom.IndexOf(WidthSignature);
        var font = rom.IndexOf(FontSignature) - GlyphSize;
        if (widths < 0 || font < 0 || font + 128 * GlyphSize > rom.Length || widths + 256 * 2 > rom.Length)
        {
            return null;
        }

        return new FontLayout(font, widths);
    }

    public static bool CanApply(ReadOnlySpan<byte> rom) => CanApply(rom, out _);

    /// <summary>True when the font is found and the target slots are blank or already extended.</summary>
    public static bool CanApply(ReadOnlySpan<byte> rom, [NotNullWhen(true)] out FontLayout? layout)
    {
        layout = Locate(rom);
        if (layout is null)
        {
            return false;
        }

        foreach (var (glyph, data) in GlyphData)
        {
            var slot = rom.Slice(layout.Font + glyph * GlyphSize, GlyphSize);
            if (!slot.SequenceEqual(data) && slot.IndexOfAnyExcept(BlankPair) >= 0)
            {
                layout = null;
                return false;
            }
        }

        return true;
    }

    public static IReadOnlyList<RomPatchAction> BuildPatches(FontLayout layout)
    {
        ArgumentNullException.ThrowIfNull(layout);
        var actions = new List<RomPatchAction>
        {
            new(layout.Widths + FirstWidthGlyph * 2, WidthData, "Extended font widths 0x4F-0x7F")
        };
        foreach (var (glyph, data) in GlyphData.OrderBy(pair => pair.Key))
        {
            actions.Add(new RomPatchAction(layout.Font + glyph * GlyphSize, data, $"Extended font glyph 0x{glyph:X2}"));
        }

        return actions;
    }
}
