using Medabots.Rom.Expansion;
using Medabots.Rom.Images;
using Medabots.Rom.Layout;
using Medabots.Rom.Projects;
using Xunit;

namespace Medabots.Rom.Tests;

public sealed class PortraitExpanderTests
{
    private static byte[] Metabee() => File.ReadAllBytes(TestRomLocator.FindRomByHeader(TestRomLocator.MetabeeEuropeHeader));

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Expand_MovesBothTablesAndEveryPortraitReadsTheSame(bool expandedRom)
    {
        var rom = expandedRom ? RomExpander.Build(Metabee()).Rom : Metabee();
        var log = new List<string>();

        var moved = PortraitExpander.Expand(rom, log);

        var before = new RomFile("before.gba", rom);
        var after = new RomFile("after.gba", moved);
        Assert.Equal(RomLayout.OriginalPortraitCount, before.Layout.PortraitCount);
        Assert.Equal(RomLayout.OriginalPortraitCount, after.Layout.PortraitCount);
        Assert.Equal(PortraitExpander.Capacity, after.Layout.PortraitCapacity);
        Assert.NotEqual(before.Layout.PortraitPointerTableOffset, after.Layout.PortraitPointerTableOffset);
        Assert.NotEqual(before.Layout.PortraitPaletteTableOffset, after.Layout.PortraitPaletteTableOffset);
        Assert.Equal(2, log.Count);
        Assert.Contains("(3 code reference(s))", log[0]);
        Assert.Contains("(4 code reference(s))", log[1]);

        var images = new ImageAssetRepository();
        foreach (var character in new[] { 0, 1, 27, 100, 175 })
        {
            for (var expression = 0; expression < PortraitExpander.ExpressionsPerCharacter; expression++)
            {
                var a = images.ReadPortrait(before, character, expression).Image;
                var b = images.ReadPortrait(after, character, expression).Image;
                Assert.Equal(a.PixelIndices, b.PixelIndices);
                Assert.Equal(a.PaletteBytes, b.PaletteBytes);
            }
        }

        Assert.Same(moved, PortraitExpander.Expand(moved));         // already done: no second move
        Assert.Equal(MapExpanderTestsLayout(rom), MapExpanderTestsLayout(moved));
    }

    [Fact]
    public void AddCharacter_CopiesAnotherCharactersOwnImagesAndPalette()
    {
        var added = PortraitExpander.AddCharacter(Metabee(), 27);

        var rom = new RomFile("added.gba", added.Rom);
        Assert.Equal(RomLayout.OriginalPortraitCount, added.CharacterId);
        Assert.Equal(RomLayout.OriginalPortraitCount + 1, rom.Layout.PortraitCount);
        var images = new ImageAssetRepository();
        for (var expression = 0; expression < PortraitExpander.ExpressionsPerCharacter; expression++)
        {
            var source = images.ReadPortrait(rom, 27, expression);
            var copy = images.ReadPortrait(rom, added.CharacterId, expression);
            Assert.Equal(source.Image.PixelIndices, copy.Image.PixelIndices);
            Assert.Equal(source.Image.PaletteBytes, copy.Image.PaletteBytes);
            Assert.NotEqual(source.ImageOffset, copy.ImageOffset);          // its own data: editing one leaves the other
            Assert.NotEqual(source.PaletteOffset, copy.PaletteOffset);
        }
    }

    [Fact]
    public void AddCharacter_BlankIsTransparentAndTheNextOneFollows()
    {
        var first = PortraitExpander.AddCharacter(Metabee(), 0, blank: true);
        var second = PortraitExpander.AddCharacter(first.Rom, 5);

        var rom = new RomFile("added.gba", second.Rom);
        Assert.Equal(first.CharacterId + 1, second.CharacterId);
        Assert.Equal(RomLayout.OriginalPortraitCount + 2, rom.Layout.PortraitCount);
        var blank = new ImageAssetRepository().ReadPortrait(rom, first.CharacterId, 4).Image;
        Assert.Equal(64, blank.Width);
        Assert.Equal(64, blank.Height);
        Assert.All(blank.PixelIndices, index => Assert.Equal(0, index));
    }

    [Fact]
    public async Task Project_AddsStagedPortraitsAndKeepsTheirNames()
    {
        var project = new RomHackProject { Name = "portraits" };
        project.PortraitAdditions.Add(new PortraitAddition(27));
        project.PortraitAdditions.Add(new PortraitAddition(0, Blank: true));
        project.PortraitNames[176] = "Debug Aki";

        var path = Path.Combine(Path.GetTempPath(), $"portraits-{Guid.NewGuid():N}.json");
        await RomHackProjectSerializer.SaveAsync(project, path);
        var loaded = await RomHackProjectSerializer.LoadAsync(path);
        File.Delete(path);
        var session = RomHackSession.FromRomFile(new RomFile("test.gba", Metabee()));
        new RomHackProjectApplicator().Apply(loaded, session);

        Assert.Equal([new PortraitAddition(27), new PortraitAddition(0, true)], loaded.PortraitAdditions);
        Assert.Equal("Debug Aki", loaded.PortraitNames[176]);
        Assert.Equal(RomLayout.OriginalPortraitCount + 2, session.RomFile.Layout.PortraitCount);
    }

    [Fact]
    public void Project_EditReadBeforeTheTablesMovedStillLandsAfterwards()
    {
        var original = Metabee();
        var images = new ImageAssetRepository();
        var portrait = images.ReadPortrait(new RomFile("source.gba", original), 27, 2);     // old table addresses
        var pixels = portrait.Image.PixelIndices.ToArray();
        pixels[0] = 5;
        var project = new RomHackProject { Name = "edit then add" };
        project.PortraitEdits.Add(portrait with { Image = portrait.Image with { PixelIndices = pixels } });
        project.PortraitAdditions.Add(new PortraitAddition(0, Blank: true));
        var session = RomHackSession.FromRomFile(new RomFile("test.gba", original));

        new RomHackProjectApplicator().Apply(project, session);

        Assert.True(session.RomFile.Layout.HasExpandedPortraits);
        Assert.Equal(5, images.ReadPortrait(session.RomFile, 27, 2).Image.PixelIndices[0]);
    }

    // The map count and the other tables are untouched by moving the portraits.
    private static (int, int, int) MapExpanderTestsLayout(byte[] rom)
    {
        var layout = RomLayout.Resolve(rom);
        return (layout.MapCount, layout.BotCount, layout.MapLayerTilemapPointerTableOffset);
    }
}
