using Medabots.Rom.Metadata;

namespace Medabots.Rom.Shops;

public sealed class ShopTableReader
{
    public const int ShopCount = MedabotsRomSchema.ShopCount;
    public const int ShopEntrySize = MedabotsRomSchema.ShopEntrySize;
    public const int ShopSlotCount = MedabotsRomSchema.ShopSlotCount;
    public const byte EmptySlot = MedabotsRomSchema.EmptyShopSlot;

    public int FindTableOffset(RomFile romFile)
    {
        ArgumentNullException.ThrowIfNull(romFile);
        // The game code's pointer works on ROMs whose shops were already edited; the content
        // signature is only a fallback for images without recognizable code.
        return romFile.Layout.TryGet("ShopTable", out var offset)
            ? offset
            : MedabotsRomSchema.ShopTable.Locate(romFile.Data);
    }

    public IReadOnlyList<ShopDefinition> ReadAll(RomFile romFile)
    {
        ArgumentNullException.ThrowIfNull(romFile);
        var tableOffset = FindTableOffset(romFile);
        return Enumerable.Range(0, ShopCount)
            .Select(shopId => Read(romFile, shopId, ShopEntrySize, tableOffset))
            .ToArray();
    }

    public ShopDefinition Read(RomFile romFile, int shopId) => Read(romFile, shopId, ShopEntrySize);

    public ShopDefinition Read(RomFile romFile, int shopId, int entryLength)
    {
        ArgumentNullException.ThrowIfNull(romFile);
        ArgumentOutOfRangeException.ThrowIfNegative(shopId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(entryLength);

        var tableOffset = FindTableOffset(romFile);
        return Read(romFile, shopId, entryLength, tableOffset);
    }

    private static ShopDefinition Read(RomFile romFile, int shopId, int entryLength, int tableOffset)
    {
        var dataOffset = tableOffset + (shopId * entryLength);
        return new ShopDefinition(shopId, dataOffset, romFile.ReadBytes(dataOffset, entryLength).ToArray());
    }
}
