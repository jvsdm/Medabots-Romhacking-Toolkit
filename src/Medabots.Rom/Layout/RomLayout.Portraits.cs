using System.Buffers.Binary;

namespace Medabots.Rom.Layout;

/// <summary>
/// The portrait character count. The game has 176 characters of 9 expressions each and no range check (three
/// loaders at 0x3F41C, 0x3F480 and 0x3F51C index the tables with character * 9 + expression; the character is
/// the byte of &lt;PORTRAIT&gt;), so moved tables are recognised by the marker <see cref="PortraitExpansionMarker"/>
/// written just before the moved image table: "MBXPORT\0", then the characters in use and the table capacity
/// (u16 each).
/// </summary>
public sealed partial class RomLayout
{
    public const int OriginalPortraitCount = 176;
    public const int PortraitMarkerSize = 16;

    public static ReadOnlySpan<byte> PortraitExpansionMarker => "MBXPORT\0"u8;

    /// <summary>Portrait characters in use (0..PortraitCount-1): 176, or the count of an expanded set.</summary>
    public int PortraitCount { get; private init; } = OriginalPortraitCount;

    /// <summary>Characters the portrait tables hold without moving them again.</summary>
    public int PortraitCapacity { get; private init; } = OriginalPortraitCount;

    public bool HasExpandedPortraits => PortraitCapacity > OriginalPortraitCount;

    private RomLayout WithPortraits(ReadOnlySpan<byte> rom)
    {
        if (!_tables.TryGetValue("PortraitPointerTable", out var images) || images < PortraitMarkerSize || images > rom.Length)
        {
            return this;
        }

        var marker = rom.Slice(images - PortraitMarkerSize, PortraitMarkerSize);
        if (!marker[..8].SequenceEqual(PortraitExpansionMarker))
        {
            return this;
        }

        return new RomLayout(_tables)
        {
            BotCount = BotCount,
            TinpetId = TinpetId,
            MapCount = MapCount,
            MapCapacity = MapCapacity,
            PortraitCount = BinaryPrimitives.ReadUInt16LittleEndian(marker[8..]),
            PortraitCapacity = BinaryPrimitives.ReadUInt16LittleEndian(marker[10..]),
        };
    }
}
