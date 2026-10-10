using Medabots.Rom.Expansion;
using Medabots.Rom.Maps;
using Medabots.Rom.Projects;
using Xunit;

namespace Medabots.Rom.Tests;

public sealed class MapTilesetCodecTests
{
    private static RomFile Expanded() =>
        new("expanded.gba", RomExpander.Build(File.ReadAllBytes(TestRomLocator.FindRomByHeader(TestRomLocator.MetabeeEuropeHeader))).Rom);

    [Theory]
    [InlineData(0)]
    [InlineData(18)]
    [InlineData(59)]
    public void Sheet_ExportThenImport_GivesTheSamePicture(int map)
    {
        var asset = new MapTilesetRepository().ReadMap(Expanded(), map);
        var (width, height, argb) = MapTilesetCodec.ExportSheet(asset);

        var (pixels, approximated) = MapTilesetCodec.ImportSheet(asset, width, height, argb);

        Assert.Equal(0, approximated);
        Assert.Equal(argb, MapTilesetCodec.ExportSheet(asset with { RawTilesetPixelIndices = pixels }).Argb);
        Assert.Throws<InvalidDataException>(() => MapTilesetCodec.ImportSheet(asset, width, height + 8, new uint[width * (height + 8)]));
    }

    [Fact]
    public void Palette_ExportThenImport_KeepsEveryColour()
    {
        var asset = new MapTilesetRepository().ReadMap(Expanded(), 3);
        var (width, height, argb) = MapTilesetCodec.ExportPalette(asset.PaletteBytes);

        var bytes = MapTilesetCodec.ImportPalette(width, height, argb);

        for (var i = 0; i < 256; i++)
        {
            Assert.Equal(BitConverter.ToUInt16(asset.PaletteBytes, i * 2) & 0x7FFF, BitConverter.ToUInt16(bytes, i * 2));
        }
    }

    [Fact]
    public void ProjectPatch_ChangesEveryMapSharingTheTileset_AndOnlyThem()
    {
        var rom = Expanded();
        var repository = new MapTilesetRepository();
        var asset = repository.ReadMap(rom, 3);
        var sharing = MapTilesetCodec.MapsSharingGraphics(rom, asset);
        Assert.Contains(4, sharing);           // the classrooms share one tileset
        var pixels = asset.RawTilesetPixelIndices.ToArray();
        pixels.AsSpan(0, 64).Fill(5);          // first tile: solid colour 5
        var palette = asset.PaletteBytes.ToArray();
        palette[2] = 0x1F;                      // bank 0 colour 1: red
        palette[3] = 0x00;
        var project = new RomHackProject { Name = "Tileset" };
        project.MapTilesetPatches.Add(new MapTilesetPatch(sharing, pixels, palette));
        var session = RomHackSession.FromRomFile(new RomFile("t.gba", rom.Data.ToArray()));

        new RomHackProjectApplicator().Apply(project, session);

        foreach (var map in sharing)
        {
            var after = repository.ReadMap(session.RomFile, map);
            Assert.Equal(pixels, after.RawTilesetPixelIndices);
            Assert.Equal(palette, after.PaletteBytes);
        }

        var other = repository.ReadMap(rom, 0);
        Assert.DoesNotContain(0, sharing);
        Assert.Equal(other.RawTilesetPixelIndices, repository.ReadMap(session.RomFile, 0).RawTilesetPixelIndices);
        Assert.Equal(rom.Length, session.RomFile.Length);
    }

    [Fact]
    public async Task ProjectFile_KeepsTilesetPatches()
    {
        var project = new RomHackProject { Name = "Tileset file" };
        project.MapTilesetPatches.Add(new MapTilesetPatch([3, 4], [1, 2, 3], null));
        var path = Path.Combine(Path.GetTempPath(), $"tileset-{Guid.NewGuid():N}.json");
        try
        {
            await RomHackProjectSerializer.SaveAsync(project, path);
            var loaded = await RomHackProjectSerializer.LoadAsync(path);

            var patch = Assert.Single(loaded.MapTilesetPatches);
            Assert.Equal([3, 4], patch.MapIds);
            Assert.Equal([(byte)1, 2, 3], patch.TilePixels);
            Assert.Null(patch.PaletteBytes);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
