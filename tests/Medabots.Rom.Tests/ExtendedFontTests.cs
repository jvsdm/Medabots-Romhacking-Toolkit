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

        Assert.Equal(5, rom[widths + 0x4F * 2]);
        Assert.Equal(6, rom[widths + 0x7F * 2]);
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

    [Theory]
    [InlineData(TestRomLocator.MetabeeEuropeHeader, ExtendedFont.FontOffset, ExtendedFont.WidthTableOffset)]
    [InlineData(TestRomLocator.RokushoEuropeHeader, RokushoFont, RokushoWidths)]
    public void Locate_FindsFontInLocalRoms(string header, int font, int widths)
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
        Assert.True(ExtendedFont.CanApply(rom));
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
