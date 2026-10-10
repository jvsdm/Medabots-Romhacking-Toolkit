using Medabots.Rom.Expansion;
using Medabots.Rom.Layout;
using Medabots.Rom.Maps;
using Medabots.Rom.Projects;
using Xunit;

namespace Medabots.Rom.Tests;

public sealed class MapExpanderTests
{
    private static byte[] Metabee() => File.ReadAllBytes(TestRomLocator.FindRomByHeader(TestRomLocator.MetabeeEuropeHeader));

    [Fact]
    public void Layout_FindsEveryPerMapTableInTheOriginalRom()
    {
        var layout = RomLayout.Resolve(Metabee());

        foreach (var (table, _) in MapExpander.Tables)
        {
            Assert.True(layout.TryGet(table, out _), table);
        }

        Assert.Equal(192, layout.MapCount);
        Assert.False(layout.HasExpandedMaps);
        // The two tables the generated list derived from others are now read from code, at the same places.
        Assert.Equal(0x3F8900, layout.MapDimensionsInMetaTilesTableOffset);
        Assert.Equal(0x3F9980, layout.MapTilesetPalettePointerTableOffset);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Expand_MovesTheTablesAndEveryMapReadsTheSame(bool expandedRom)
    {
        var rom = expandedRom ? RomExpander.Build(Metabee()).Rom : Metabee();
        var log = new List<string>();

        var moved = MapExpander.Expand(rom, log);

        var before = new RomFile("before.gba", rom);
        var after = new RomFile("after.gba", moved);
        Assert.Equal(192, after.Layout.MapCount);
        Assert.Equal(MapExpander.Capacity, after.Layout.MapCapacity);
        Assert.Equal(MapExpander.Tables.Length, log.Count);
        if (expandedRom)
        {
            Assert.Equal(rom.Length, moved.Length);         // fits in the expansion's free space
        }

        foreach (var (table, size) in MapExpander.Tables)
        {
            before.Layout.TryGet(table, out var oldTable);
            after.Layout.TryGet(table, out var newTable);
            Assert.NotEqual(oldTable, newTable);
            Assert.Equal(rom.AsSpan(oldTable, 192 * size).ToArray(), moved.AsSpan(newTable, 192 * size).ToArray());
        }

        var tiles = new MapTilesetRepository();
        var overlays = new MapOverlayRepository();
        foreach (var map in new[] { 0, 3, 50, 100, 191 })
        {
            Assert.Equal(tiles.ReadMap(before, map).GraphicsDataOffset, tiles.ReadMap(after, map).GraphicsDataOffset);
            Assert.Equal(overlays.ReadMap(before, map).Warps, overlays.ReadMap(after, map).Warps);
            Assert.Equal(overlays.ReadMap(before, map).EntitySpawns, overlays.ReadMap(after, map).EntitySpawns);
        }

        // Expanding again changes nothing.
        Assert.Same(moved, MapExpander.Expand(moved));
    }

    [Fact]
    public void AddMap_ClonesTheSourceIntoTheNextId()
    {
        var expanded = RomExpander.Build(Metabee()).Rom;

        var empty = MapExpander.AddMap(expanded, 3);
        var full = MapExpander.AddMap(empty.Rom, 18, copyWarpsAndPeople: true);

        var rom = new RomFile("maps.gba", full.Rom);
        Assert.Equal(192, empty.MapId);
        Assert.Equal(193, full.MapId);
        Assert.Equal(194, rom.Layout.MapCount);
        var tiles = new MapTilesetRepository();
        Assert.Equal(tiles.ReadMap(rom, 3).GraphicsDataOffset, tiles.ReadMap(rom, 192).GraphicsDataOffset);
        foreach (var (table, size) in MapExpander.Tables.Where(t => t.Table is not ("MapWarpPointerTable" or "MapEntitySpawnPointerTable")))
        {
            rom.Layout.TryGet(table, out var offset);
            Assert.Equal(full.Rom.AsSpan(offset + 3 * size, size).ToArray(), full.Rom.AsSpan(offset + 192 * size, size).ToArray());
        }

        var overlays = new MapOverlayRepository();
        Assert.Empty(overlays.ReadMap(rom, 192).Warps);
        Assert.Empty(overlays.ReadMap(rom, 192).EntitySpawns);
        Assert.Equal(overlays.ReadMap(rom, 18).Warps, overlays.ReadMap(rom, 193).Warps);
        Assert.Equal(overlays.ReadMap(rom, 18).EntitySpawns, overlays.ReadMap(rom, 193).EntitySpawns);
        Assert.Equal(expanded.Length, full.Rom.Length);
    }

    // The editor stages new maps in the project; export adds them first, then applies the edits (which
    // may already target the new maps) against the moved tables.
    [Fact]
    public async Task Project_AddsStagedMapsThenAppliesEditsToThem()
    {
        var expanded = RomExpander.Build(Metabee()).Rom;
        var source = new RomFile("expanded.gba", expanded);
        var project = new RomHackProject { Name = "Debug room" };
        project.MapAdditions.Add(new MapAddition(136, false));
        project.MapAdditions.Add(new MapAddition(3, true));
        var preview = new RomFile("preview.gba", MapExpander.ApplyAdditions(expanded, project.MapAdditions));
        var room = new MapTilesetRepository().ReadMap(preview, 192);
        var layer = room.Layers[0];
        var edited = layer.TileEntries.ToArray();
        edited[0] = 0x0005;
        new MapLayerProjectEditor().StageLayer(project, 192, layer, edited);
        var hallway = new MapOverlayRepository().ReadMap(preview, 17);
        var door = new MapWarpRecord(1, 1, 192, 0, 0, 0, 3, 3);
        new MapWarpProjectEditor().UpsertWarpPatch(project, 17, hallway.Warps.Append(door));
        new MapWarpProjectEditor().UpsertWarpPatch(project, 192, [new MapWarpRecord(3, 3, 17, 0, 0, 0, 1, 1)]);
        var path = Path.Combine(Path.GetTempPath(), $"maps-{Guid.NewGuid():N}.json");
        await RomHackProjectSerializer.SaveAsync(project, path);
        var loaded = await RomHackProjectSerializer.LoadAsync(path);
        File.Delete(path);
        var session = RomHackSession.FromRomFile(new RomFile("export.gba", expanded.ToArray()));

        new RomHackProjectApplicator().Apply(loaded, session);

        var rom = session.RomFile;
        Assert.Equal(194, rom.Layout.MapCount);
        Assert.Equal((ushort)0x0005, new MapTilesetRepository().ReadMap(rom, 192).Layers[0].TileEntries[0]);
        Assert.Contains(new MapOverlayRepository().ReadMap(rom, 17).Warps, warp => warp.DestinationMapId == 192);
        Assert.Equal(17, new MapOverlayRepository().ReadMap(rom, 192).Warps.Single().DestinationMapId);
        Assert.Equal(new MapOverlayRepository().ReadMap(source, 3).EntitySpawns, new MapOverlayRepository().ReadMap(rom, 193).EntitySpawns);
        Assert.Equal(expanded.Length, rom.Length);
    }

    [Fact]
    public async Task BlankMap_HasTheChosenSizeEmptyLayersBorderCollisionAndABlackTileset()
    {
        var expanded = RomExpander.Build(Metabee()).Rom;
        var project = new RomHackProject { Name = "Blank" };
        project.MapAdditions.Add(new MapAddition(16, false, Blank: true, Width: 12, Height: 10, BlankTileset: true));
        project.MapAdditions.Add(new MapAddition(3, false, Blank: true, Width: 64, Height: 64));
        var path = Path.Combine(Path.GetTempPath(), $"blank-{Guid.NewGuid():N}.json");
        await RomHackProjectSerializer.SaveAsync(project, path);
        var loaded = await RomHackProjectSerializer.LoadAsync(path);
        File.Delete(path);

        var rom = new RomFile("blank.gba", MapExpander.ApplyAdditions(expanded, loaded.MapAdditions));

        var tiles = new MapTilesetRepository();
        var room = tiles.ReadMap(rom, 192);
        Assert.Equal((12, 10), (room.WidthInTiles, room.HeightInTiles));
        Assert.All(room.Layers, layer => Assert.All(layer.TileEntries, entry => Assert.Equal(0, entry)));
        Assert.Equal(MapExpander.MaxTilesetTiles * 64, room.RawTilesetPixelIndices.Length);
        Assert.All(room.RawTilesetPixelIndices, pixel => Assert.Equal(0, pixel));
        Assert.All(room.PaletteBytes, value => Assert.Equal(0, value));
        Assert.Equal(6 * 5, room.CollisionBytes.Length);
        Assert.Equal(0, room.CollisionBytes[0]);              // border blocked
        Assert.Equal(1, room.CollisionBytes[1 * 6 + 1]);      // inside walkable
        Assert.Empty(new MapOverlayRepository().ReadMap(rom, 192).Warps);
        Assert.Equal(rom.Data[rom.Layout.MapMusicTableOffset + 16], rom.Data[rom.Layout.MapMusicTableOffset + 192]);
        var big = tiles.ReadMap(rom, 193);
        Assert.Equal((64, 64), (big.WidthInTiles, big.HeightInTiles));
        Assert.Equal(tiles.ReadMap(rom, 3).GraphicsDataOffset, big.GraphicsDataOffset);

        // A smaller tileset picture with its own colours: those tiles change, the rest stay.
        var argb = new uint[128 * 8];
        argb.AsSpan(0, 8).Fill(0xFFF80000);
        var palette = MapTilesetCodec.PaletteFromPicture(128, 8, argb);
        var (pixels, approximated) = MapTilesetCodec.ImportSheet(room with { PaletteBytes = palette }, 128, 8, argb);
        Assert.Equal(0, approximated);
        Assert.NotEqual(0, pixels[0]);
        Assert.Equal(room.RawTilesetPixelIndices.Length, pixels.Length);
        Assert.Throws<InvalidDataException>(() => MapExpander.AddBlankMap(expanded, 0, 65, 10, false));
        Assert.Throws<InvalidDataException>(() => MapExpander.AddBlankMap(expanded, 0, 11, 10, false));
    }

    [Fact]
    public void BotsAndMaps_CanBeAddedInEitherOrder()
    {
        var expanded = RomExpander.Build(Metabee()).Rom;
        var stats = new byte[16];
        var legs = new byte[16];
        legs[1] = 103;
        var bot = new BotDefinition { Name = "MAPTEST", GraphicsFrom = 43, Parts = [("A", stats), ("B", stats), ("C", stats), ("D", legs)] };

        var botThenMap = MapExpander.AddMap(BotAdder.Add(expanded, bot).Rom, 0).Rom;
        var mapThenBot = BotAdder.Add(MapExpander.AddMap(expanded, 0).Rom, bot).Rom;

        foreach (var rom in new[] { botThenMap, mapThenBot })
        {
            var layout = RomLayout.Resolve(rom);
            Assert.Equal(193, layout.MapCount);
            Assert.Equal(122, layout.BotCount);
        }
    }
}
