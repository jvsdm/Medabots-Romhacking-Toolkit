using System.Buffers.Binary;

namespace Medabots.Rom.Layout;

/// <summary>
/// Per-map tables and the map count. The game has 192 maps and no code constant for that number (the
/// map ID is a byte read from 0x3005FDE and used directly as a table index), so an expanded map set is
/// recognised by the marker <see cref="MapExpansionMarker"/> written just before the moved layer table:
/// "MBXMAPS\0", then the number of maps in use and the number of table entries (u16 each).
/// </summary>
public sealed partial class RomLayout
{
    public const int OriginalMapCount = 192;
    public const int MapMarkerSize = 16;

    public static ReadOnlySpan<byte> MapExpansionMarker => "MBXMAPS\0"u8;

    // Tables the generated signature list derives from others, plus per-map tables the editor did not
    // know; all read directly by code in Metabee (E) (0x529C0, 0x4830C, 0x11E40, 0x48A22, 0x6087E,
    // 0x48182, 0x52636). Literal-based, so tables moved by the map expansion are followed.
    internal static readonly RomTableSignature[] MapTableSignatures =
    [
        new("MapDimensionsInMetaTilesTable", "00 06 02 0E 09 06 09 0E 00 20 00 29 02 D0 01 29 06 D0 0A E0 ?? 49 50 00", 20, 0x0),
        new("MapTilesetPalettePointerTable", "01 3D 00 2D ED DA F0 BC 01 BC 00 47 80 7E 3F 08 00 00 01 06 ?? 48 ?? 49", 20, 0x0),
        new("MapEncounterExtraTable", "?? 4A 81 18 09 78 05 1C 00 29 26 D1 ?? 49 68 18 00 78 80 00 ?? 49 40 18", 20, 0x0),
        new("MapAreaTable", "84 8A 04 08 84 8A 04 08 84 8A 04 08 50 8A 04 08 50 8A 04 08 ?? 49 ?? 4B", 20, 0x0),
        new("MapAreaTableB", "?? 48 ?? 49 A0 F7 AA F8 ?? 48 ?? 49 18 22 1C F0 27 FA 2A 68 ?? 49 ?? 48", 20, 0x0),
        new("MapBlockPointerTableA", "00 03 E0 57 00 03 36 0C 00 00 BA 0C 00 00 4C 0C 00 00 30 B5 ?? 49 ?? 4B", 20, 0x0),
        new("MapBlockPointerTableB", "00 00 E0 57 00 03 5C 0C 00 00 69 0C 00 00 F0 B5 01 23 5B 42 ?? 49 ?? 4A", 20, 0x0),
    ];

    /// <summary>Maps in use (IDs 0..MapCount-1): 192, or the count of an expanded map set.</summary>
    public int MapCount { get; private init; } = OriginalMapCount;

    /// <summary>Entries in the per-map tables (maps that can exist without moving them again).</summary>
    public int MapCapacity { get; private init; } = OriginalMapCount;

    public bool HasExpandedMaps => MapCapacity > OriginalMapCount;

    private RomLayout WithMaps(ReadOnlySpan<byte> rom)
    {
        if (!_tables.TryGetValue("MapLayerTilemapPointerTable", out var layers) || layers < MapMarkerSize || layers > rom.Length)
        {
            return this;
        }

        var marker = rom.Slice(layers - MapMarkerSize, MapMarkerSize);
        if (!marker[..8].SequenceEqual(MapExpansionMarker))
        {
            return this;
        }

        return new RomLayout(_tables)
        {
            BotCount = BotCount,
            TinpetId = TinpetId,
            MapCount = BinaryPrimitives.ReadUInt16LittleEndian(marker[8..]),
            MapCapacity = BinaryPrimitives.ReadUInt16LittleEndian(marker[10..]),
        };
    }
}
