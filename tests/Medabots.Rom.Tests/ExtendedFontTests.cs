using Medabots.Rom.Text;
using Xunit;

namespace Medabots.Rom.Tests;

public sealed class ExtendedFontTests
{
    private const int RokushoFont = 0x4BFC90;
    private const int RokushoWidths = 0x3B4E28;

    private static byte[] SyntheticRom(int font = ExtendedFont.FontOffset, int widths = ExtendedFont.WidthTableOffset)
    {
        var rom = new byte[0x500000];
        rom.AsSpan(font, 128 * ExtendedFont.GlyphSize).Fill(0x11);
        ExtendedFont.FontSignature.CopyTo(rom, font + ExtendedFont.GlyphSize);
        ExtendedFont.WidthSignature.CopyTo(rom, widths);
        return rom;
    }

    private static void Apply(byte[] rom, FontLayout layout)
    {
        foreach (var action in ExtendedFont.BuildPatches(layout))
        {
            action.Data.CopyTo(rom, action.Offset);
        }
    }

    [Theory]
    [InlineData(ExtendedFont.FontOffset, ExtendedFont.WidthTableOffset)]
    [InlineData(RokushoFont, RokushoWidths)]
    public void BuildPatches_WritesGlyphsAndWidthsAtLocatedFont(int font, int widths)
    {
        var rom = SyntheticRom(font, widths);
        Assert.True(ExtendedFont.CanApply(rom, out var layout));
        Assert.Equal(new FontLayout(font, widths), layout);

        Apply(rom, layout);

        foreach (var glyph in ExtendedFont.Glyphs)
        {
            Assert.True(rom.AsSpan(font + glyph * ExtendedFont.GlyphSize, ExtendedFont.GlyphSize)
                .SequenceEqual(ExtendedFont.GetGlyph(glyph)));
        }

        Assert.Equal(6, rom[widths + 0x4F * 2]);
        Assert.Equal(7, rom[widths + 0x7D * 2]);
        Assert.Equal(7, rom[widths + 0x7F * 2]);
        Assert.Equal(8, rom[widths + 0x4E * 2]);
        Assert.Equal(0, rom[widths + 0x80 * 2]);
        Assert.True(ExtendedFont.CanApply(rom));
    }

    [Fact]
    public void CanApply_RejectsOccupiedSlotOrMissingFont()
    {
        var occupied = SyntheticRom();
        occupied[ExtendedFont.FontOffset + 0x7D * ExtendedFont.GlyphSize] = 0xFF;
        Assert.False(ExtendedFont.CanApply(occupied));

        var noWidths = SyntheticRom();
        noWidths[ExtendedFont.WidthTableOffset] = 0x00;
        Assert.False(ExtendedFont.CanApply(noWidths));
        Assert.Null(ExtendedFont.Locate(noWidths));
    }

    // Rokusho (U): font 0x4C01D0 / widths 0x3B4C88, but it only holds 0x00-0x4E (no accents); 0x50 onward is other
    // data, so the new glyphs cannot go into 0x7D-0x7F there.
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
}
