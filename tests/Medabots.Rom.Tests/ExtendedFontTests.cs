using Medabots.Rom.Text;
using Xunit;

namespace Medabots.Rom.Tests;

public sealed class ExtendedFontTests
{
    private const int RokushoFont = 0x4BFC90;
    private const int RokushoWidths = 0x3B4E28;

    private static byte[] SyntheticRom(int font = ExtendedFont.FontOffset, int widths = ExtendedFont.WidthTableOffset, char region = 'P')
    {
        var rom = new byte[0x500000];
        var layout = new FontLayout(font, widths);
        rom.AsSpan(layout.Font, ExtendedFont.GlyphCount * ExtendedFont.GlyphSize).Fill(0x11);
        rom.AsSpan(layout.ItalicFont, ExtendedFont.GlyphCount * ExtendedFont.GlyphSize).Fill(0x11);
        ExtendedFont.FontSignature.CopyTo(rom, font + ExtendedFont.GlyphSize);
        ExtendedFont.WidthSignature.CopyTo(rom, widths);
        rom[0xAF] = (byte)region;
        return rom;
    }

    private static void Apply(byte[] rom, FontLayout layout)
    {
        foreach (var action in ExtendedFont.BuildPatches(rom, layout))
        {
            action.Data.CopyTo(rom, action.Offset);
        }
    }

    [Theory]
    [InlineData(ExtendedFont.FontOffset, ExtendedFont.WidthTableOffset)]
    [InlineData(RokushoFont, RokushoWidths)]
    public void BuildPatches_WritesNormalAndItalicGlyphsAndWidths(int font, int widths)
    {
        var rom = SyntheticRom(font, widths);
        Assert.True(ExtendedFont.CanApply(rom, out var layout));
        Assert.Equal(new FontLayout(font, widths), layout);

        Apply(rom, layout);

        foreach (var glyph in ExtendedFont.Glyphs)
        {
            Assert.True(rom.AsSpan(layout.Font + glyph * ExtendedFont.GlyphSize, ExtendedFont.GlyphSize).SequenceEqual(ExtendedFont.GetGlyph(glyph)));
            Assert.True(rom.AsSpan(layout.ItalicFont + glyph * ExtendedFont.GlyphSize, ExtendedFont.GlyphSize).SequenceEqual(ExtendedFont.GetGlyph(glyph, true)));
            var (advance, cls) = ExtendedFont.MeasureGlyph(ExtendedFont.GetGlyph(glyph), false);
            Assert.Equal(advance, rom[layout.Widths + glyph * 2]);
            Assert.Equal(cls, rom[layout.Widths + glyph * 2 + 1]);
            Assert.InRange(advance, 4, 8);
            Assert.Equal(ExtendedFont.MeasureGlyph(ExtendedFont.GetGlyph(glyph, true), true).Advance, rom[layout.ItalicWidths + glyph * 2]);
        }

        Assert.Equal(6, rom[layout.Widths + 0x4F * 2]);
        Assert.Equal(7, rom[layout.Widths + 0x7D * 2]);
        Assert.Equal(7, rom[layout.Widths + 0x7F * 2]);
        Assert.Equal(0, rom[layout.Widths + 0x50 * 2]);
        Assert.Equal(8, rom[layout.Widths + 0x4E * 2]);
        Assert.True(ExtendedFont.CanApply(rom));
    }

    [Fact]
    public void BuildPatches_SkipsItalicGlyphsWhenSlotsTaken()
    {
        var rom = SyntheticRom();
        var layout = ExtendedFont.Locate(rom)!;
        rom[layout.ItalicFont + 0x4F * ExtendedFont.GlyphSize] = 0x22;

        var actions = ExtendedFont.BuildPatches(rom, layout);

        Assert.DoesNotContain(actions, a => a.Offset >= layout.ItalicFont && a.Offset < layout.ItalicFont + ExtendedFont.GlyphCount * ExtendedFont.GlyphSize);
        Assert.Contains(actions, a => a.Offset == layout.Font + 0x4F * ExtendedFont.GlyphSize);
    }

    [Fact]
    public void CanApply_RejectsUsaOccupiedSlotOrMissingFont()
    {
        Assert.False(ExtendedFont.CanApply(SyntheticRom(region: 'E')));

        var occupied = SyntheticRom();
        occupied[ExtendedFont.FontOffset + 0x7D * ExtendedFont.GlyphSize] = 0xFF;
        Assert.False(ExtendedFont.CanApply(occupied));

        var noWidths = SyntheticRom();
        noWidths[ExtendedFont.WidthTableOffset] = 0x00;
        Assert.False(ExtendedFont.CanApply(noWidths));
        Assert.Null(ExtendedFont.Locate(noWidths));
    }

    [Fact]
    public void MeasureGlyph_FollowsShippedRule()
    {
        var glyph = new byte[ExtendedFont.GlyphSize];
        glyph.AsSpan().Fill(0x11);
        // Ink at x = 4, rows 2 and 8
        glyph[2 * 4 + 2] = 0x1F;
        glyph[32 + 0 * 4 + 2] = 0x1F;

        Assert.Equal(((byte)6, (byte)1), ExtendedFont.MeasureGlyph(glyph, false));
        Assert.Equal(((byte)5, (byte)0), ExtendedFont.MeasureGlyph(glyph, true));

        glyph[1 * 4 + 2] = 0x1F;
        Assert.Equal((byte)0, ExtendedFont.MeasureGlyph(glyph, false).Class);
    }

    // Rokusho (U): font 0x4C01D0 / widths 0x3B4C88, but it only holds 0x00-0x4E (no accents) and is not (E).
    [Theory]
    [InlineData(TestRomLocator.MetabeeEuropeHeader, ExtendedFont.FontOffset, ExtendedFont.WidthTableOffset, true)]
    [InlineData(TestRomLocator.RokushoEuropeHeader, RokushoFont, RokushoWidths, true)]
    [InlineData(TestRomLocator.RokushoUsaHeader, 0x4C01D0, 0x3B4C88, false)]
    public void Locate_FindsFontInLocalRoms(string header, int font, int widths, bool canApply)
    {
        string path;
        try
        {
            path = TestRomLocator.FindRomByHeader(header);
        }
        catch (FileNotFoundException)
        {
            return;
        }

        var rom = File.ReadAllBytes(path);
        Assert.Equal(new FontLayout(font, widths), ExtendedFont.Locate(rom));
        Assert.Equal(canApply, ExtendedFont.CanApply(rom));
    }

    [Fact]
    public void FontLayout_ItalicAtFixedDistance()
    {
        var layout = new FontLayout(ExtendedFont.FontOffset, ExtendedFont.WidthTableOffset);
        Assert.Equal(0x4C59AC, layout.ItalicFont);
        Assert.Equal(0x3B5008, layout.ItalicWidths);
    }

    [Fact]
    public void Project_DefaultsToExtendedFont()
    {
        Assert.True(new RomHackProject().UseExtendedFont);
    }

    [Fact]
    public void ItalicFontRepair_RebuildsLostDarkPixels()
    {
        var rom = new byte[0x10000];
        const int font = 0x1000;
        rom.AsSpan(font, ItalicFontRepair.FontSize).Fill(0x11);
        rom[font + 4] = 0x10;                                   // glyph 0, row 1, x = 0: lost dark pixel (0)
        rom[font + 0] = 0x51;                                   // row 0, x = 1: ink 5 above nothing
        var repaired = ItalicFontRepair.Repair(rom, font, out var fixedPixels);
        Assert.NotNull(repaired);
        Assert.Equal(1, fixedPixels);
        Assert.Equal(0xD, repaired![4] & 15);
        Assert.DoesNotContain(repaired.SelectMany(b => new[] { b & 15, b >> 4 }), v => v == 0);

        rom[font + 8] = 0xB1;                                   // a dark shade present: font not damaged
        Assert.Null(ItalicFontRepair.Repair(rom, font, out _));
    }

    [Fact]
    public void Project_RepairsEuropeanItalicFont()
    {
        string path;
        try
        {
            path = TestRomLocator.FindRomByHeader(TestRomLocator.MetabeeEuropeHeader);
        }
        catch (FileNotFoundException)
        {
            return;
        }

        var original = File.ReadAllBytes(path);
        var layout = ExtendedFont.Locate(original)!;
        Assert.NotNull(ItalicFontRepair.Repair(original, layout.ItalicFont, out _));

        var session = RomHackSession.FromRomFile(new RomFile("font.gba", (byte[])original.Clone()));
        new Projects.RomHackProjectApplicator().Apply(new RomHackProject { Name = "font" }, session);
        var rom = session.RomFile.Data.ToArray();
        Assert.Null(ItalicFontRepair.Repair(rom, layout.ItalicFont, out _));
        Assert.Equal(ExtendedFont.Preview(original, layout), rom);

        var off = RomHackSession.FromRomFile(new RomFile("font.gba", (byte[])original.Clone()));
        new Projects.RomHackProjectApplicator().Apply(new RomHackProject { Name = "font", UseExtendedFont = false }, off);
        Assert.Equal(original, off.RomFile.Data.ToArray());
    }
}
