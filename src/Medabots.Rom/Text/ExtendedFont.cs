namespace Medabots.Rom.Text;

/// <summary>
/// PT-BR extension of the dialogue font (Medabots: Metabee (E)): new glyphs in the original blank slots
/// (0x4F õ, 0x7D Ã, 0x7E ã, 0x7F Õ) and advance widths for 0x4F-0x7F, zero in the original ROM
/// (table by Hyd, from insert_text_v2.py).
/// </summary>
public static class ExtendedFont
{
    public const int FontOffset = 0x4BFC6C;
    public const int WidthTableOffset = 0x3B4E08;
    public const int GlyphSize = 64;
    public const int FirstWidthGlyph = 0x4F;

    public const string Summary =
        "Use extended font (PT-BR)\n\n" +
        "Writes into the dialogue font (0x4BFC6C, 8x16 4bpp) the glyphs drawn over the original blank slots:\n" +
        "  0x4F  õ\n  0x7D  Ã\n  0x7E  ã\n  0x7F  Õ\n\n" +
        "Writes the advance widths for glyphs 0x4F-0x7F into the width table (0x3B4E08, 2 bytes per glyph: " +
        "advance in pixels and vertical class). In the original ROM these entries are zero, so the accented " +
        "letters (Ä..à, Æ æ) and the new glyphs are drawn with no spacing.\n\n" +
        "Only the italic font is left unchanged. Skipped when the ROM is not Metabee (E) or the slots already " +
        "hold other glyphs.";

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

    // '£', '&', '%' entries (0x4C-0x4E): 8,1 in Metabee (E)
    private static readonly byte[] WidthFingerprint = [0x08, 0x01, 0x08, 0x01, 0x08, 0x01];

    private const byte BlankPair = 0x11;

    public static IReadOnlyCollection<int> Glyphs => GlyphData.Keys;

    public static ReadOnlySpan<byte> GetGlyph(int glyph) =>
        GlyphData.TryGetValue(glyph, out var data) ? data : ReadOnlySpan<byte>.Empty;

    public static int? GetAdvance(int glyph)
    {
        var index = (glyph - FirstWidthGlyph) * 2;
        return index >= 0 && index < WidthData.Length ? WidthData[index] : null;
    }

    /// <summary>True when the ROM has the Metabee (E) layout and the target slots are blank or already extended.</summary>
    public static bool CanApply(ReadOnlySpan<byte> rom)
    {
        if (rom.Length < FontOffset + 128 * GlyphSize)
        {
            return false;
        }

        if (!rom.Slice(WidthTableOffset + 0x4C * 2, WidthFingerprint.Length).SequenceEqual(WidthFingerprint))
        {
            return false;
        }

        foreach (var (glyph, data) in GlyphData)
        {
            var slot = rom.Slice(FontOffset + glyph * GlyphSize, GlyphSize);
            if (!slot.SequenceEqual(data) && slot.IndexOfAnyExcept(BlankPair) >= 0)
            {
                return false;
            }
        }

        return true;
    }

    public static IReadOnlyList<RomPatchAction> BuildPatches()
    {
        var actions = new List<RomPatchAction>
        {
            new(WidthTableOffset + FirstWidthGlyph * 2, WidthData, "Extended font widths 0x4F-0x7F")
        };
        foreach (var (glyph, data) in GlyphData.OrderBy(pair => pair.Key))
        {
            actions.Add(new RomPatchAction(FontOffset + glyph * GlyphSize, data, $"Extended font glyph 0x{glyph:X2}"));
        }

        return actions;
    }
}
