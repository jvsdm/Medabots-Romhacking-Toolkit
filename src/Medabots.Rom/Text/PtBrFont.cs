using System.Diagnostics.CodeAnalysis;

namespace Medabots.Rom.Text;

/// <summary>
/// PT-BR dialogue fonts drawn for the translation (European ROMs): retouched normal font with Ã ã Õ õ drawn in the
/// Ä ä Ö ö slots (0x50, 0x51, 0x64, 0x65) and the italic font redrawn. Glyphs come from ptbr_font.bin
/// (re/tools/text/ptbr_font_extract.py: only the glyphs that differ from Metabee (E)) and are written where the game
/// draws them (<see cref="FontLayout.GameFont"/>). Ã ã Õ õ are copied into the extension slots too (0x4F õ, 0x7D Ã,
/// 0x7E ã, 0x7F Õ), so text encoded for <see cref="ExtendedFont"/> shows the same letters.
/// Widths: 0x4F-0x7F measured from the pixels (<see cref="ExtendedFont.MeasureGlyph"/>), at least the width of the base
/// letter (the shipped italic widths are often 1 px wider than the rule); a redrawn glyph below 0x4F keeps its shipped
/// width plus the change in its measured advance.
/// </summary>
public static class PtBrFont
{
    private const string ResourceName = "Medabots.Rom.Text.ptbr_font.bin";
    private const string Magic = "MBPTFONT";

    // Extension slot <- PT-BR letter drawn in the Ä/Ö slots
    private static readonly (int Slot, int Source)[] Aliases = [(0x4F, 0x65), (0x7D, 0x50), (0x7E, 0x51), (0x7F, 0x64)];

    // Letters of glyphs 0x4F-0x7F (the game's order); an accented letter advances at least as far as its base letter
    private const string UpperGlyphs = "õÄäÁáÂâÈèÉéÊêËëÎîÏïÍíÖöÔôÓóÜüÛûÙùÚúßÇçÑñ¡¿ÀàÆæÃãÕ";
    private const string Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    private static readonly Lazy<(Dictionary<int, byte[]> Normal, Dictionary<int, byte[]> Italic)> Glyphs = new(Load);

    public const string Summary =
        "PT-BR font (100%, European ROMs only)\n\n" +
        "Writes the fonts drawn for the PT-BR translation over the dialogue fonts, at the address the game draws " +
        "from (8 bytes before the offset shown above):\n" +
        "  Normal font: retouched glyphs; Ã ã Õ õ in the slots of Ä ä Ö ö (0x50, 0x51, 0x64, 0x65).\n" +
        "  Italic font: redrawn (replaces the italic font repair).\n" +
        "Ã ã Õ õ are also copied to 0x4F õ, 0x7D Ã, 0x7E ã, 0x7F Õ, the slots the text encoder uses, so the text " +
        "encoding is the same as with the extended font.\n\n" +
        "Widths: 0x4F-0x7F measured from the pixels with the extended font rule, never narrower than the base " +
        "letter (Ã as A, ã as a); a redrawn glyph below 0x4F keeps its width plus the change in its measured advance.\n\n" +
        "Replaces \"Use extended font\" when both are on.";

    public static IReadOnlyCollection<int> NormalGlyphs => Glyphs.Value.Normal.Keys;

    public static IReadOnlyCollection<int> ItalicGlyphs => Glyphs.Value.Italic.Keys;

    public static ReadOnlySpan<byte> GetGlyph(int glyph, bool italic = false) =>
        (italic ? Glyphs.Value.Italic : Glyphs.Value.Normal).TryGetValue(glyph, out var data) ? data : ReadOnlySpan<byte>.Empty;

    /// <summary>European ROM with the dialogue font found.</summary>
    public static bool CanApply(ReadOnlySpan<byte> rom, [NotNullWhen(true)] out FontLayout? layout)
    {
        layout = ExtendedFont.IsEuropean(rom) ? ExtendedFont.Locate(rom) : null;
        return layout is not null;
    }

    /// <summary>Glyphs (normal, italic, extension copies) and the width entries they change.</summary>
    public static IReadOnlyList<RomPatchAction> BuildPatches(ReadOnlySpan<byte> rom, FontLayout layout)
    {
        ArgumentNullException.ThrowIfNull(layout);
        var copy = rom.ToArray();
        var actions = new List<RomPatchAction>();
        AddFont(rom, copy, actions, layout.GameFont, layout.Widths, Glyphs.Value.Normal, false);
        AddFont(rom, copy, actions, layout.GameItalicFont, layout.ItalicWidths, Glyphs.Value.Italic, true);
        return actions;
    }

    /// <summary>Copy of the ROM as exported with the PT-BR font.</summary>
    public static byte[] Preview(ReadOnlySpan<byte> rom, FontLayout layout)
    {
        var copy = rom.ToArray();
        foreach (var action in BuildPatches(rom, layout))
        {
            action.Data.CopyTo(copy, action.Offset);
        }

        return copy;
    }

    /// <summary>Advance and class of a glyph as the game stores it (64 bytes from the game font address).</summary>
    public static (byte Advance, byte Class) MeasureGameGlyph(ReadOnlySpan<byte> glyph, bool italic)
    {
        // ExtendedFont.MeasureGlyph was fitted on rows read 8 bytes late (game rows 2-13): same view here
        var shifted = new byte[ExtendedFont.GlyphSize];
        glyph[8..].CopyTo(shifted);
        shifted.AsSpan(ExtendedFont.GlyphSize - 8).Fill(0x11);
        return ExtendedFont.MeasureGlyph(shifted, italic);
    }

    private static void AddFont(ReadOnlySpan<byte> rom, byte[] copy, List<RomPatchAction> actions, int font, int widths,
        Dictionary<int, byte[]> glyphs, bool italic)
    {
        var name = italic ? "PT-BR italic" : "PT-BR font";
        foreach (var (glyph, data) in glyphs.OrderBy(pair => pair.Key))
        {
            data.CopyTo(copy, font + glyph * ExtendedFont.GlyphSize);
            actions.Add(new RomPatchAction(font + glyph * ExtendedFont.GlyphSize, data, $"{name} glyph 0x{glyph:X2}"));
        }

        foreach (var (slot, source) in Aliases)
        {
            var data = copy.AsSpan(font + source * ExtendedFont.GlyphSize, ExtendedFont.GlyphSize).ToArray();
            data.CopyTo(copy, font + slot * ExtendedFont.GlyphSize);
            actions.Add(new RomPatchAction(font + slot * ExtendedFont.GlyphSize, data, $"{name} glyph 0x{slot:X2} (copy of 0x{source:X2})"));
        }

        for (var glyph = 0; glyph < ExtendedFont.FirstWidthGlyph; glyph++)
        {
            if (!glyphs.ContainsKey(glyph))
            {
                continue;
            }

            var before = MeasureGameGlyph(rom.Slice(font + glyph * ExtendedFont.GlyphSize, ExtendedFont.GlyphSize), italic).Advance;
            var after = MeasureGameGlyph(copy.AsSpan(font + glyph * ExtendedFont.GlyphSize, ExtendedFont.GlyphSize), italic).Advance;
            var advance = rom[widths + glyph * 2] + after - before;
            if (after != before && advance is > 0 and <= 16)
            {
                actions.Add(new RomPatchAction(widths + glyph * 2, [(byte)advance], $"{name} width 0x{glyph:X2}"));
            }
        }

        var table = new byte[(ExtendedFont.GlyphCount - ExtendedFont.FirstWidthGlyph) * 2];
        for (var glyph = ExtendedFont.FirstWidthGlyph; glyph < ExtendedFont.GlyphCount; glyph++)
        {
            var k = (glyph - ExtendedFont.FirstWidthGlyph) * 2;
            (table[k], table[k + 1]) = MeasureGameGlyph(copy.AsSpan(font + glyph * ExtendedFont.GlyphSize, ExtendedFont.GlyphSize), italic);
            if (table[k] > 0 && BaseGlyph(glyph) is var baseGlyph and > 0)
            {
                table[k] = Math.Max(table[k], rom[widths + baseGlyph * 2]);
            }
        }

        actions.Add(new RomPatchAction(widths + ExtendedFont.FirstWidthGlyph * 2, table, $"{name} widths 0x4F-0x7F"));
    }

    // Glyph of the unaccented letter (A = 0x01, a = 0x1B); 0 for ß, ¡, ¿, Æ, æ
    private static int BaseGlyph(int glyph)
    {
        var letter = UpperGlyphs[glyph - ExtendedFont.FirstWidthGlyph].ToString().Normalize(System.Text.NormalizationForm.FormD)[0];
        return Letters.IndexOf(letter) + 1;
    }

    private static (Dictionary<int, byte[]> Normal, Dictionary<int, byte[]> Italic) Load()
    {
        using var stream = typeof(PtBrFont).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidDataException($"Missing resource {ResourceName}.");
        using var reader = new BinaryReader(stream);
        if (new string(reader.ReadChars(Magic.Length)) != Magic)
        {
            throw new InvalidDataException($"{ResourceName}: bad header.");
        }

        Dictionary<int, byte[]> ReadFont()
        {
            var glyphs = new Dictionary<int, byte[]>();
            for (int i = 0, count = reader.ReadByte(); i < count; i++)
            {
                glyphs[reader.ReadByte()] = reader.ReadBytes(ExtendedFont.GlyphSize);
            }

            return glyphs;
        }

        var normal = ReadFont();
        return (normal, ReadFont());
    }
}
