using Medabots.Rom.Compression;
using Medabots.Rom.Metadata;

namespace Medabots.Rom.Images;

public sealed partial class ImageAssetRepository
{
    public PortraitAsset ReadPortrait(RomFile romFile, int characterId, int portraitIndex)
    {
        ArgumentNullException.ThrowIfNull(romFile);
        ArgumentOutOfRangeException.ThrowIfNegative(characterId);
        ArgumentOutOfRangeException.ThrowIfNegative(portraitIndex);

        var imagePointerOffset = romFile.Layout.PortraitPointerTableOffset + ((characterId * PortraitsPerCharacter) + portraitIndex) * sizeof(uint);
        var palettePointerOffset = romFile.Layout.PortraitPaletteTableOffset + (characterId * sizeof(uint));
        var imageOffset = ReadRequiredPointer(romFile, imagePointerOffset);
        var paletteOffset = ReadRequiredPointer(romFile, palettePointerOffset);

        var compressed = Malias2.Decompress(romFile.Data, imageOffset) ?? DecompressPortraitPayload(romFile.Data, imageOffset);
        var unpacked = TileImageCodec.Split4BppTiles(compressed);
        var palette = romFile.ReadBytes(paletteOffset, PaletteSize).ToArray();
        return new PortraitAsset(
            characterId,
            portraitIndex,
            imagePointerOffset,
            palettePointerOffset,
            imageOffset,
            paletteOffset,
            new IndexedImage(
                MedabotsRomSchema.PortraitTileWidth,
                unpacked.Length / 0x40 / MedabotsRomSchema.PortraitTileWidth,
                unpacked,
                palette));
    }

    public SpriteAsset ReadSprite(RomFile romFile, int spriteId)
    {
        ArgumentNullException.ThrowIfNull(romFile);
        ArgumentOutOfRangeException.ThrowIfNegative(spriteId);

        var imagePointerOffset = romFile.Layout.SpritePointerTableOffset + (spriteId * sizeof(uint));
        var palettePointerOffset = romFile.Layout.SpritePaletteTableOffset + (spriteId * sizeof(uint));
        var imageOffset = ReadRequiredPointer(romFile, imagePointerOffset);
        var paletteOffset = ReadRequiredPointer(romFile, palettePointerOffset);

        var compressed = GbaLz77.Decompress(romFile.Data, imageOffset)
            ?? throw new InvalidDataException($"Sprite {spriteId} does not contain valid LZ77 image data.");
        var unpacked = TileImageCodec.Split4BppTiles(compressed);
        var palette = romFile.ReadBytes(paletteOffset, PaletteSize).ToArray();
        const int tileWidth = 2;
        var tileCount = Math.Max(1, unpacked.Length / 0x40);
        return new SpriteAsset(
            spriteId,
            imagePointerOffset,
            palettePointerOffset,
            imageOffset,
            paletteOffset,
            new IndexedImage(tileWidth, Math.Max(1, tileCount / tileWidth), unpacked, palette));
    }

    public BattleCompositeSpriteComponentAsset ReadBattleCompositeSpriteComponent(RomFile romFile, int medabotId, int componentIndex)
    {
        ArgumentNullException.ThrowIfNull(romFile);
        ArgumentOutOfRangeException.ThrowIfNegative(medabotId);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(medabotId, romFile.Layout.BotCount);
        ArgumentOutOfRangeException.ThrowIfNegative(componentIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(componentIndex, CompositeBattleSpritePointersPerPart);

        var imagePointerOffset = romFile.Layout.CompositeBattleSpritePointerTableOffset +
                                 ((medabotId * CompositeBattleSpritePointersPerPart) + componentIndex) * sizeof(uint);
        var imageOffset = ReadRequiredPointer(romFile, imagePointerOffset);
        var compressed = Malias2.Decompress(romFile.Data, imageOffset)
            ?? throw new InvalidDataException($"Composite battle sprite component {medabotId}:{componentIndex} does not contain valid Malias2 image data.");
        var unpacked = TileImageCodec.Split4BppTiles(compressed);
        var tileCount = Math.Max(1, unpacked.Length / 0x40);
        var tileWidth = GuessCompositeComponentTileWidth(componentIndex, tileCount);
        var paletteFamily = ReadCompositePaletteFamily(romFile, medabotId);
        var paletteBank = (byte)(paletteFamily + 4);
        var paletteOffset = ResolvePartSelectionPaletteOffset(romFile, paletteFamily);
        var palette = romFile.ReadBytes(paletteOffset, PaletteSize).ToArray();

        return new BattleCompositeSpriteComponentAsset(
            medabotId,
            componentIndex,
            imagePointerOffset,
            imageOffset,
            romFile.Layout.CompositeBattleSpritePaletteFamilyTableOffset + medabotId,
            paletteOffset,
            paletteFamily,
            0,
            paletteBank,
            new IndexedImage(tileWidth, Math.Max(1, tileCount / tileWidth), unpacked, palette));
    }

    public byte[] ReadBattleCompositePaletteBytesForFamily(RomFile romFile, byte family)
    {
        ArgumentNullException.ThrowIfNull(romFile);
        return romFile.ReadBytes(ResolvePartSelectionPaletteOffset(romFile, family), PaletteSize).ToArray();
    }

    public SharedSpriteSheetAsset ReadSharedPartSpriteSheet(RomFile romFile, string key)
    {
        ArgumentNullException.ThrowIfNull(romFile);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var (name, imageOffset, tileWidth) = key switch
        {
            "part-screen-main" => ("Shared Part Screen Tile Sheet", romFile.Layout.SharedPartScreenTileSheetOffset, MedabotsRomSchema.SharedPartScreenTileSheetTileWidth),
            "part-detail-a" => ("Part Detail Tile Sheet A", romFile.Layout.SharedPartDetailTileSheetAOffset, 8),
            "part-detail-b" => ("Part Detail Tile Sheet B", romFile.Layout.SharedPartDetailTileSheetBOffset, 8),
            "part-detail-c" => ("Part Detail Tile Sheet C", romFile.Layout.SharedPartDetailTileSheetCOffset, 4),
            "part-detail-d" => ("Part Detail Tile Sheet D", romFile.Layout.SharedPartDetailTileSheetDOffset, 4),
            "part-detail-e" => ("Part Detail Tile Sheet E", romFile.Layout.SharedPartDetailTileSheetEOffset, 8),
            "part-detail-f" => ("Part Detail Tile Sheet F", romFile.Layout.SharedPartDetailTileSheetFOffset, 4),
            "part-detail-g" => ("Part Detail Tile Sheet G", romFile.Layout.SharedPartDetailTileSheetGOffset, 4),
            _ => throw new InvalidOperationException($"Unsupported shared sprite sheet key '{key}'.")
        };

        var compressed = Malias2.Decompress(romFile.Data, imageOffset)
            ?? throw new InvalidDataException($"Shared sprite sheet '{key}' does not contain valid Malias2 image data.");
        var unpacked = TileImageCodec.Split4BppTiles(compressed);
        var tileCount = Math.Max(1, unpacked.Length / 0x40);
        tileWidth = Math.Min(tileWidth, tileCount);

        return new SharedSpriteSheetAsset(
            key,
            name,
            imageOffset,
            new IndexedImage(tileWidth, Math.Max(1, tileCount / tileWidth), unpacked, BuildPlaceholderCompositePalette()));
    }

    private static int ReadRequiredPointer(RomFile romFile, int pointerOffset)
    {
        if (!GbaPointer.TryReadFileOffset(romFile.Data, pointerOffset, out var fileOffset))
        {
            throw new InvalidDataException($"Invalid pointer at 0x{pointerOffset:X}.");
        }

        return fileOffset;
    }

    private static int GuessCompositeComponentTileWidth(int componentIndex, int tileCount) =>
        componentIndex == 5
            ? (tileCount >= 8 ? 2 : Math.Max(1, tileCount))
            : (tileCount >= 4 ? 2 : Math.Max(1, tileCount));

    private static byte ReadCompositePaletteFamily(RomFile romFile, int medabotId)
    {
        var familyOffset = romFile.Layout.CompositeBattleSpritePaletteFamilyTableOffset + medabotId;
        if (familyOffset < 0 || familyOffset >= romFile.Length)
        {
            throw new InvalidDataException($"Composite battle sprite palette family table offset 0x{familyOffset:X} is out of range.");
        }

        var family = romFile.Data[familyOffset];
        if (romFile.Layout.CompositeBattleSpritePaletteDataOffset + ((family + 4) * PaletteSize) + PaletteSize > romFile.Length)
        {
            throw new InvalidDataException($"Composite battle sprite palette family {family} for medabot {medabotId} is out of range.");
        }

        return family;
    }

    private static int ResolvePartSelectionPaletteOffset(RomFile romFile, byte family) =>
        family >= 8
            ? romFile.Layout.CompositeBattleSpritePaletteDataOffset + (family * PaletteSize)
            : romFile.Layout.PartSelectionComponentPaletteSetOffset + (family * PaletteSize);

    private static byte[] BuildPlaceholderCompositePalette()
    {
        var palette = new byte[PaletteSize];
        for (var index = 0; index < PaletteSize / 2; index++)
        {
            var shade = (byte)Math.Min(31, index * 2);
            var raw = (ushort)(shade | (shade << 5) | (shade << 10));
            palette[index * 2] = (byte)(raw & 0xFF);
            palette[(index * 2) + 1] = (byte)(raw >> 8);
        }

        return palette;
    }

    private static byte[] DecompressPortraitPayload(byte[] data, int offset)
    {
        if (offset + 6 > data.Length || data[offset] != (byte)'L' || data[offset + 1] != (byte)'e')
        {
            throw new InvalidDataException("Portrait data does not contain a supported compression header.");
        }

        var output = new List<byte>();
        var cursor = offset + 6;
        while (cursor < data.Length)
        {
            var command = data[cursor++];
            if (command != 0xAA)
            {
                break;
            }

            for (var i = 0; i < 4 && cursor < data.Length; i++)
            {
                output.Add(data[cursor++]);
            }
        }

        return output.ToArray();
    }

    private static bool TryReadOptionalPointer(RomFile romFile, int pointerOffset, out int fileOffset)
    {
        if (!GbaPointer.TryReadFileOffset(romFile.Data, pointerOffset, out fileOffset))
        {
            fileOffset = 0;
            return false;
        }

        return true;
    }
}
