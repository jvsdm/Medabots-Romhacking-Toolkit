namespace Medabots.Rom.Projects;

public sealed class FreeSpaceAllocator
{
    private int _nextOffset;

    public FreeSpaceAllocator(int initialOffset)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(initialOffset);
        _nextOffset = initialOffset;
    }

    public int CurrentOffset => _nextOffset;

    public int Reserve(int length, int alignment = 4)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(alignment);

        var offset = AlignUp(_nextOffset, alignment);
        _nextOffset = offset + length;
        return offset;
    }

    public void EnsureAtLeast(int offset)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        if (offset > _nextOffset)
        {
            _nextOffset = offset;
        }
    }

    /// <summary>An allocator starting at <see cref="FirstFreeOffset"/>.</summary>
    public static FreeSpaceAllocator ForRom(byte[] rom) => new(FirstFreeOffset(rom));

    /// <summary>
    /// Where new data can go, the same for every editor feature: after the last used byte (free space is
    /// 0xFF, as the expansion leaves it), with a small gap, and never below 8 MB (the original ROM's size).
    /// In the 16 MB expanded ROM this stays inside the file instead of growing it.
    /// </summary>
    public static int FirstFreeOffset(byte[] rom)
    {
        ArgumentNullException.ThrowIfNull(rom);
        var last = rom.Length - 1;
        while (last >= OriginalRomSize && rom[last] == 0xFF)
        {
            last--;
        }

        return AlignUp(Math.Max(last + 1 + FreeSpaceGap, OriginalRomSize), 4);
    }

    private const int OriginalRomSize = 0x800000;
    private const int FreeSpaceGap = 0x10;

    public static int AlignUp(int value, int alignment)
    {
        var remainder = value % alignment;
        return remainder == 0 ? value : value + (alignment - remainder);
    }
}
