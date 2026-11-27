using Medabots.Rom.Text;
using Xunit;

namespace Medabots.Rom.Tests;

public sealed class ImportedFontTests
{
    // Synthetic font: a 1 px stem per glyph and a fractional advance
    private static ImportedFont Build(double advance = 5.4) => ImportedFont.Build("Test", 10, ch =>
    {
        if (ch == '♥')
        {
            return null;
        }

        var coverage = new byte[ImportedFont.CellWidth * ImportedFont.CellHeight];
        if (ch != ' ')
        {
            for (var y = 4; y <= 10; y++)
            {
                coverage[y * ImportedFont.CellWidth] = 255;
            }
        }

        return new RasterizedGlyph(coverage, ch == ' ' ? 2.5 : ch == 'W' ? 9.2 : advance, false);
    });

    [Fact]
    public void Shade_MapsCoverageToDialogueShades()
    {
        Assert.Equal(1, ImportedFont.Shade(0));
        Assert.Equal(15, ImportedFont.Shade(255));
    }

    [Fact]
    public void Build_EncodesGlyphsAndCapsAdvances()
    {
        var font = Build();
        var a = ImportedFont.GlyphChars.IndexOf('a');
        Assert.Equal(0x1F, font.Normal[a * ExtendedFont.GlyphSize + 4 * 4]);          // row 4: x = 0 ink F (low nibble), x = 1 background
        Assert.Equal(0x11, font.Normal[0]);                                          // space: background
        Assert.Equal(ImportedFont.MaxAdvance, font.NormalAdvances[ImportedFont.GlyphChars.IndexOf('W')]);
        Assert.Contains(font.Warnings, w => w.Contains("limited to 8 px") && w.Contains('W'));
        Assert.Null(font.Italic);
    }

    [Fact]
    public void RoundAdvances_KeepsSampleLinesNearTheirExactLength()
    {
        var advances = Enumerable.Repeat(5.4, ExtendedFont.GlyphCount).ToArray();
        advances[0] = 2.5;
        var rounded = ImportedFont.RoundAdvances(advances);
        Assert.InRange(rounded[1], 5, 6);
        Assert.InRange(rounded[0], 2, 4);
    }

    [Fact]
    public async Task Project_KeepsImportedFont()
    {
        var path = Path.Combine(Path.GetTempPath(), $"imported_font_{Guid.NewGuid():N}.json");
        try
        {
            var font = Build();
            await Projects.RomHackProjectSerializer.SaveAsync(new RomHackProject { Name = "font", ImportedFont = font, UseImportedFont = true }, path);
            var loaded = await Projects.RomHackProjectSerializer.LoadAsync(path);
            Assert.True(loaded.UseImportedFont);
            Assert.Equal(font.Normal, loaded.ImportedFont!.Normal);
            Assert.Equal(font.NormalAdvances, loaded.ImportedFont.NormalAdvances);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Project_WritesImportedFontWhereTheGameDraws()
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
        var font = Build();
        var session = RomHackSession.FromRomFile(new RomFile("font.gba", (byte[])original.Clone()));
        new Projects.RomHackProjectApplicator().Apply(new RomHackProject { Name = "font", ImportedFont = font, UseImportedFont = true }, session);
        var rom = session.RomFile.Data.ToArray();
        var a = ImportedFont.GlyphChars.IndexOf('a');
        Assert.True(rom.AsSpan(layout.GameFont + a * 64, 64).SequenceEqual(font.Normal.AsSpan(a * 64, 64)));
        Assert.Equal(font.NormalAdvances[1], rom[layout.Widths + 2]);
        Assert.Equal(original[layout.Widths + 3], rom[layout.Widths + 3]);                 // class byte kept
        var heart = ImportedFont.GlyphChars.IndexOf('♥');                                   // missing: game's glyph and width
        Assert.True(rom.AsSpan(layout.GameFont + heart * 64, 64).SequenceEqual(original.AsSpan(layout.GameFont + heart * 64, 64)));
        Assert.Equal(original[layout.Widths + heart * 2], rom[layout.Widths + heart * 2]);
        Assert.True(rom.AsSpan(layout.GameItalicFont, 0x2000).SequenceEqual(original.AsSpan(layout.GameItalicFont, 0x2000)));
    }
}
