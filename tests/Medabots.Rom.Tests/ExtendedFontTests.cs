using Medabots.Rom.Text;
using Xunit;

namespace Medabots.Rom.Tests;

public sealed class ExtendedFontTests
{
    private static byte[] SyntheticRom()
    {
        var rom = new byte[0x500000];
        rom.AsSpan(ExtendedFont.FontOffset, 128 * ExtendedFont.GlyphSize).Fill(0x11);
        rom.AsSpan(ExtendedFont.WidthTableOffset + 0x4C * 2, 6).Fill(0x08);
        for (var i = 1; i < 6; i += 2)
        {
            rom[ExtendedFont.WidthTableOffset + 0x4C * 2 + i] = 0x01;
        }

        return rom;
    }

    [Fact]
    public void BuildPatches_WritesGlyphsAndWidths()
    {
        var rom = SyntheticRom();
        Assert.True(ExtendedFont.CanApply(rom));

        foreach (var action in ExtendedFont.BuildPatches())
        {
            action.Data.CopyTo(rom, action.Offset);
        }

        foreach (var glyph in ExtendedFont.Glyphs)
        {
            Assert.True(rom.AsSpan(ExtendedFont.FontOffset + glyph * ExtendedFont.GlyphSize, ExtendedFont.GlyphSize)
                .SequenceEqual(ExtendedFont.GetGlyph(glyph)));
        }

        Assert.Equal(5, rom[ExtendedFont.WidthTableOffset + 0x4F * 2]);
        Assert.Equal(6, rom[ExtendedFont.WidthTableOffset + 0x7F * 2]);
        Assert.Equal(8, rom[ExtendedFont.WidthTableOffset + 0x4E * 2]);
        Assert.Equal(0, rom[ExtendedFont.WidthTableOffset + 0x80 * 2]);
        Assert.True(ExtendedFont.CanApply(rom));
    }

    [Fact]
    public void CanApply_RejectsOccupiedSlotOrOtherLayout()
    {
        var occupied = SyntheticRom();
        occupied[ExtendedFont.FontOffset + 0x7D * ExtendedFont.GlyphSize] = 0xFF;
        Assert.False(ExtendedFont.CanApply(occupied));

        var otherLayout = SyntheticRom();
        otherLayout[ExtendedFont.WidthTableOffset + 0x4C * 2] = 0x00;
        Assert.False(ExtendedFont.CanApply(otherLayout));
    }

    [Fact]
    public void Project_DefaultsToExtendedFont()
    {
        Assert.True(new RomHackProject().UseExtendedFont);
    }
}
