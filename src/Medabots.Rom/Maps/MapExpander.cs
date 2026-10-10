using System.Buffers.Binary;
using Medabots.Rom.Layout;
using Medabots.Rom.Projects;

namespace Medabots.Rom.Maps;

public sealed record MapAddResult(byte[] Rom, int MapId, IReadOnlyList<string> Log);

/// <summary>
/// New maps for Medabots: Metabee (E). The game indexes 16 tables with the map ID (a byte, never range
/// checked: 0x3005FDE in RAM, read by ~60 routines), so maps beyond the original 192 only need room in
/// those tables. <see cref="Expand"/> moves the tables to free space with <see cref="Capacity"/> entries
/// and repoints the code literals that load them (each table has 1-4, all literal-pool words); a marker
/// before the layer table records how many maps exist. <see cref="AddMap"/> then clones a map into the
/// next ID: same layers, tileset, palette, collision, music and area; warps and people optionally empty.
/// </summary>
public static class MapExpander
{
    /// <summary>IDs 0-254; 0xFF means "no map" in the game's checks.</summary>
    public const int Capacity = 255;

    private const int GbaBase = 0x08000000;
    private const int CodeEnd = 0x100000;

    /// <summary>Every table indexed by the map ID, with its entry size.</summary>
    public static readonly (string Table, int EntrySize)[] Tables =
    [
        ("MapLayerTilemapPointerTable", 12),     // first: the marker sits right before it
        ("MapEncounterSettingsTable", 8),
        ("MapEncounterExtraTable", 4),
        ("MapAreaTable", 1),
        ("MapAreaTableB", 1),
        ("MapDimensionsInMetaTilesTable", 2),
        ("MapTilesetGraphicsPointerTable", 4),
        ("MapColorAttributePointerTable", 4),
        ("MapTilesetPalettePointerTable", 4),
        ("MapCollisionPointerTable", 4),
        ("MapBlockPointerTableA", 4),
        ("MapBlockPointerTableB", 4),
        ("MapWarpPointerTable", 4),
        ("MapEventObjectResourcePointerTable", 4),
        ("MapMusicTable", 1),
        ("MapEntitySpawnPointerTable", 4),
    ];

    /// <summary>Moves the per-map tables so the ROM can hold <see cref="Capacity"/> maps (no-op if done).</summary>
    public static byte[] Expand(byte[] rom, ICollection<string>? log = null)
    {
        ArgumentNullException.ThrowIfNull(rom);
        var layout = RomLayout.Resolve(rom);
        if (layout.HasExpandedMaps)
        {
            return rom;
        }

        var missing = Tables.Where(t => !layout.TryGet(t.Table, out _)).Select(t => t.Table).ToList();
        Require(missing.Count == 0, $"Map tables not found in this ROM ({string.Join(", ", missing)}); new maps need Medabots: Metabee (Europe).");
        var count = layout.MapCount;
        var output = (byte[])rom.Clone();
        var allocator = FreeSpaceAllocator.ForRom(output);
        var size = RomLayout.MapMarkerSize + Tables.Sum(t => FreeSpaceAllocator.AlignUp(Capacity * t.EntrySize, 4));
        var start = allocator.Reserve(size, 4);
        EnsureRoom(ref output, start + size);

        var cursor = start + RomLayout.MapMarkerSize;
        foreach (var (table, entrySize) in Tables)
        {
            layout.TryGet(table, out var oldTable);
            var length = count * entrySize;
            rom.AsSpan(oldTable, length).CopyTo(output.AsSpan(cursor));
            for (var id = count; id < Capacity; id++)
            {
                rom.AsSpan(oldTable, entrySize).CopyTo(output.AsSpan(cursor + id * entrySize));     // map 0's values until used
            }

            var repointed = Repoint(output, oldTable, length, cursor);
            Require(repointed > 0, $"No code reference to the {table} was found.");
            log?.Add($"{table}: 0x{oldTable:X6} -> 0x{cursor:X6} ({repointed} code reference(s))");
            cursor += FreeSpaceAllocator.AlignUp(Capacity * entrySize, 4);
        }

        WriteMarker(output, start, count);
        var check = RomLayout.Resolve(output);
        Require(check.MapCapacity == Capacity && check.MapCount == count, "The moved map tables could not be read back.");
        return output;
    }

    /// <summary>
    /// Adds a map that starts as a copy of <paramref name="sourceMap"/> (moving the tables first if
    /// needed). With <paramref name="copyWarpsAndPeople"/> false its warp and person lists are empty.
    /// </summary>
    public static MapAddResult AddMap(byte[] rom, int sourceMap, bool copyWarpsAndPeople = false)
    {
        ArgumentNullException.ThrowIfNull(rom);
        var log = new List<string>();
        var output = Expand(rom, log);
        var layout = RomLayout.Resolve(output);
        Require(sourceMap >= 0 && sourceMap < layout.MapCount, $"Map {sourceMap} does not exist (0-{layout.MapCount - 1}).");
        Require(layout.MapCount < layout.MapCapacity, $"The ROM already has {layout.MapCount} maps, the maximum.");
        var id = layout.MapCount;
        foreach (var (table, entrySize) in Tables)
        {
            layout.TryGet(table, out var offset);
            output.AsSpan(offset + sourceMap * entrySize, entrySize).CopyTo(output.AsSpan(offset + id * entrySize));
        }

        if (!copyWarpsAndPeople)
        {
            var allocator = FreeSpaceAllocator.ForRom(output);
            var empty = allocator.Reserve(EmptyList.Length, 4);
            EnsureRoom(ref output, empty + EmptyList.Length);
            EmptyList.CopyTo(output, empty);
            WriteU32(output, layout.MapWarpPointerTableOffset + id * 4, (uint)(GbaBase + empty));
            WriteU32(output, layout.MapEntitySpawnPointerTableOffset + id * 4, (uint)(GbaBase + empty));
        }

        WriteMarker(output, layout.MapLayerTilemapPointerTableOffset - RomLayout.MapMarkerSize, id + 1);
        log.Add($"map {id}: copy of map {sourceMap}{(copyWarpsAndPeople ? "" : ", no warps or people yet")}");
        return new MapAddResult(output, id, log);
    }

    /// <summary>The ROM with the staged new maps added, in order (the input is not modified).</summary>
    public static byte[] ApplyAdditions(byte[] rom, IEnumerable<MapAddition> additions)
    {
        ArgumentNullException.ThrowIfNull(additions);
        var output = rom;
        foreach (var addition in additions)
        {
            output = addition.Blank
                ? AddBlankMap(output, addition.SourceMapId, addition.Width, addition.Height, addition.BlankTileset).Rom
                : AddMap(output, addition.SourceMapId, addition.CopyWarpsAndPeople).Rom;
        }

        return ReferenceEquals(output, rom) ? (byte[])rom.Clone() : output;
    }

    /// <summary>Largest map the game shows: the BG screens are 64 x 64 tiles.</summary>
    public const int MaxMapSize = 64;

    /// <summary>Tiles the tileset VRAM area holds (0x6009000 up to the sprites at 0x6010000).</summary>
    public const int MaxTilesetTiles = 896;

    /// <summary>
    /// Adds an empty <paramref name="width"/> x <paramref name="height"/> map (even sizes, 2-64 tiles): three
    /// empty layers, walkable collision inside a blocked border, no warps or people, and the music, area,
    /// encounters and sprite list of <paramref name="settingsFrom"/>. Its tileset is that map's, or with
    /// <paramref name="blankTileset"/> a black one of <see cref="MaxTilesetTiles"/> empty tiles.
    /// </summary>
    public static MapAddResult AddBlankMap(byte[] rom, int settingsFrom, int width, int height, bool blankTileset)
    {
        Require(width is >= 2 and <= MaxMapSize && height is >= 2 and <= MaxMapSize && width % 2 == 0 && height % 2 == 0,
            $"A map is 2-{MaxMapSize} tiles wide and high, in even numbers (collision works on 2x2 blocks): {width}x{height}.");
        var added = AddMap(rom, settingsFrom, copyWarpsAndPeople: false);
        var output = added.Rom;
        var id = added.MapId;
        var layout = RomLayout.Resolve(output);
        var allocator = FreeSpaceAllocator.ForRom(output);
        int Store(byte[] data)
        {
            var at = allocator.Reserve(data.Length, 4);
            EnsureRoom(ref output, at + data.Length);
            data.CopyTo(output, at);
            return at;
        }

        // Tile count of the tileset: the layers' header repeats it.
        int tiles;
        if (blankTileset)
        {
            tiles = MaxTilesetTiles;
            WriteU32(output, layout.MapTilesetGraphicsPointerTableOffset + id * 4, (uint)(GbaBase + Store(Compression.GbaLz77.Compress(new byte[tiles * 32]))));
            WriteU32(output, layout.MapTilesetPalettePointerTableOffset + id * 4, (uint)(GbaBase + Store(new byte[0x200])));
        }
        else
        {
            var graphics = (int)BinaryPrimitives.ReadUInt32LittleEndian(output.AsSpan(layout.MapTilesetGraphicsPointerTableOffset + id * 4)) - GbaBase;
            tiles = (Compression.GbaLz77.Decompress(output, graphics)?.Length ?? 0) / 32;
        }

        output[layout.MapDimensionsInMetaTilesTableOffset + id * 2] = (byte)width;
        output[layout.MapDimensionsInMetaTilesTableOffset + id * 2 + 1] = (byte)height;
        var layer = new byte[16 + width * height * 2];
        BinaryPrimitives.WriteUInt16LittleEndian(layer.AsSpan(0), (ushort)width);
        BinaryPrimitives.WriteUInt16LittleEndian(layer.AsSpan(4), (ushort)height);
        BinaryPrimitives.WriteUInt16LittleEndian(layer.AsSpan(8), (ushort)tiles);
        var layerAt = Store(Compression.GbaLz77.Compress(layer));
        for (var k = 0; k < 3; k++)
        {
            WriteU32(output, layout.MapLayerTilemapPointerTableOffset + id * 12 + k * 4, (uint)(GbaBase + layerAt));
        }

        // Collision per 16x16 block: 1 = walkable, 0 = blocked (as in the original maps).
        int cw = width / 2, ch = height / 2;
        var collision = new byte[cw * ch];
        for (var y = 0; y < ch; y++)
        {
            for (var x = 0; x < cw; x++)
            {
                collision[y * cw + x] = (byte)(x == 0 || y == 0 || x == cw - 1 || y == ch - 1 ? 0 : 1);
            }
        }

        WriteU32(output, layout.MapCollisionPointerTableOffset + id * 4, (uint)(GbaBase + Store(collision)));
        var log = added.Log.ToList();
        log[^1] = $"map {id}: empty {width}x{height}, settings of map {settingsFrom}, {(blankTileset ? "blank tileset" : $"tileset of map {settingsFrom}")}";
        return new MapAddResult(output, id, log);
    }

    // A list terminator (0xFF) padded to one 8-byte record: empty for warps and for people.
    private static readonly byte[] EmptyList = [0xFF, 0, 0, 0, 0, 0, 0, 0];

    // Literal-pool words pointing into the old table, loaded by a Thumb `ldr rX, [pc, #imm]`.
    internal static int Repoint(byte[] rom, int oldTable, int length, int newTable)
    {
        var count = 0;
        for (var at = 0; at < CodeEnd; at += 4)
        {
            var value = (int)BinaryPrimitives.ReadUInt32LittleEndian(rom.AsSpan(at)) - GbaBase;
            if (value < oldTable || value >= oldTable + length || !IsLoadedLiteral(rom, at))
            {
                continue;
            }

            WriteU32(rom, at, (uint)(GbaBase + newTable + (value - oldTable)));
            count++;
        }

        return count;
    }

    private static bool IsLoadedLiteral(byte[] rom, int at)
    {
        for (var pc = Math.Max(0, at - 1020); pc < at; pc += 2)
        {
            var instruction = BinaryPrimitives.ReadUInt16LittleEndian(rom.AsSpan(pc));
            if (instruction >> 11 == 0b01001 && ((pc + 4) & ~3) + (instruction & 0xFF) * 4 == at)
            {
                return true;
            }
        }

        return false;
    }

    // An original 8 MB ROM has no free space inside the file: it grows (1 MB steps, 0xFF filled, max 32 MB).
    internal static void EnsureRoom(ref byte[] rom, int end)
    {
        if (end <= rom.Length)
        {
            return;
        }

        var size = FreeSpaceAllocator.AlignUp(end, 0x100000);
        Require(size <= 0x2000000, "The ROM would pass 32 MB, the GBA maximum.");
        var old = rom.Length;
        Array.Resize(ref rom, size);
        rom.AsSpan(old).Fill(0xFF);
    }

    private static void WriteMarker(byte[] rom, int at, int count)
    {
        RomLayout.MapExpansionMarker.CopyTo(rom.AsSpan(at));
        BinaryPrimitives.WriteUInt16LittleEndian(rom.AsSpan(at + 8), (ushort)count);
        BinaryPrimitives.WriteUInt16LittleEndian(rom.AsSpan(at + 10), Capacity);
        rom.AsSpan(at + 12, 4).Clear();
    }

    private static void WriteU32(byte[] rom, int at, uint value) => BinaryPrimitives.WriteUInt32LittleEndian(rom.AsSpan(at), value);

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidDataException(message);
        }
    }
}
