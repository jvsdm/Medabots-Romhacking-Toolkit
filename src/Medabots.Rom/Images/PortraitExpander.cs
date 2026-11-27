using System.Buffers.Binary;
using Medabots.Rom.Compression;
using Medabots.Rom.Layout;
using Medabots.Rom.Maps;
using Medabots.Rom.Projects;

namespace Medabots.Rom.Images;

/// <summary>A portrait character staged in a project: a copy of <paramref name="SourceCharacterId"/>, or blank.</summary>
public sealed record PortraitAddition(int SourceCharacterId, bool Blank = false);

public sealed record PortraitAddResult(byte[] Rom, int CharacterId, IReadOnlyList<string> Log);

/// <summary>
/// New portrait characters for Medabots: Metabee (E). The image table (character * 9 + expression, 4-byte
/// pointers to Malias2 images) and the palette table (one 32-byte palette per character) have no range check,
/// and &lt;PORTRAIT&gt; takes the character as a byte, so <see cref="Expand"/> moves both tables to free space with
/// <see cref="Capacity"/> characters and repoints the code literals that load them (3 + 4); a marker before the
/// image table records how many characters exist. <see cref="AddCharacter"/> adds the next character as a copy
/// of another (its own copies of the images and palette, so editing one never changes the other) or blank.
/// </summary>
public static class PortraitExpander
{
    /// <summary>Character IDs 0-255: the &lt;PORTRAIT&gt; character is one byte.</summary>
    public const int Capacity = 256;

    public const int ExpressionsPerCharacter = ImageAssetRepository.PortraitsPerCharacter;

    /// <summary>A portrait is 64 x 64 pixels: 8 x 8 tiles of 4 bpp.</summary>
    public const int PortraitTiles = 64;

    private const int GbaBase = 0x08000000;
    private const int PaletteSize = 32;

    /// <summary>Moves the portrait tables so the ROM can hold <see cref="Capacity"/> characters (no-op if done).</summary>
    public static byte[] Expand(byte[] rom, ICollection<string>? log = null)
    {
        ArgumentNullException.ThrowIfNull(rom);
        var layout = RomLayout.Resolve(rom);
        if (layout.HasExpandedPortraits)
        {
            return rom;
        }

        var found = layout.TryGet("PortraitPointerTable", out var oldImages);
        found &= layout.TryGet("PortraitPaletteTable", out var oldPalettes);
        Require(found, "Portrait tables not found in this ROM; new portraits need Medabots: Metabee (Europe).");
        var count = layout.PortraitCount;
        var output = (byte[])rom.Clone();
        var imageSize = Capacity * ExpressionsPerCharacter * 4;
        var size = RomLayout.PortraitMarkerSize + imageSize + Capacity * 4;
        var start = FreeSpaceAllocator.ForRom(output).Reserve(size, 4);
        MapExpander.EnsureRoom(ref output, start + size);

        var images = start + RomLayout.PortraitMarkerSize;
        var palettes = images + imageSize;
        var imageRow = ExpressionsPerCharacter * 4;
        rom.AsSpan(oldImages, count * imageRow).CopyTo(output.AsSpan(images));
        rom.AsSpan(oldPalettes, count * 4).CopyTo(output.AsSpan(palettes));
        for (var id = count; id < Capacity; id++)
        {
            rom.AsSpan(oldImages, imageRow).CopyTo(output.AsSpan(images + id * imageRow));      // character 0 until used
            rom.AsSpan(oldPalettes, 4).CopyTo(output.AsSpan(palettes + id * 4));
        }

        var imageRefs = MapExpander.Repoint(output, oldImages, count * imageRow, images);
        var paletteRefs = MapExpander.Repoint(output, oldPalettes, count * 4, palettes);
        Require(imageRefs > 0 && paletteRefs > 0, "No code reference to the portrait tables was found.");
        log?.Add($"PortraitPointerTable: 0x{oldImages:X6} -> 0x{images:X6} ({imageRefs} code reference(s))");
        log?.Add($"PortraitPaletteTable: 0x{oldPalettes:X6} -> 0x{palettes:X6} ({paletteRefs} code reference(s))");

        WriteMarker(output, start, count);
        var check = RomLayout.Resolve(output);
        Require(check.PortraitCapacity == Capacity && check.PortraitCount == count, "The moved portrait tables could not be read back.");
        return output;
    }

    /// <summary>
    /// Adds a character (moving the tables first if needed): a copy of <paramref name="sourceCharacter"/>'s nine
    /// expressions and palette, or with <paramref name="blank"/> nine empty (transparent) portraits.
    /// </summary>
    public static PortraitAddResult AddCharacter(byte[] rom, int sourceCharacter, bool blank = false)
    {
        ArgumentNullException.ThrowIfNull(rom);
        var log = new List<string>();
        var output = Expand(rom, log);
        var layout = RomLayout.Resolve(output);
        Require(sourceCharacter >= 0 && sourceCharacter < layout.PortraitCount, $"Portrait character {sourceCharacter} does not exist (0-{layout.PortraitCount - 1}).");
        Require(layout.PortraitCount < layout.PortraitCapacity, $"The ROM already has {layout.PortraitCount} portrait characters, the maximum.");
        var id = layout.PortraitCount;
        var images = layout.PortraitPointerTableOffset;
        var palettes = layout.PortraitPaletteTableOffset;
        var allocator = FreeSpaceAllocator.ForRom(output);
        int Store(byte[] data)
        {
            var at = allocator.Reserve(data.Length, 4);
            MapExpander.EnsureRoom(ref output, at + data.Length);
            data.CopyTo(output, at);
            return at;
        }

        var blankImage = blank ? Store(Malias2.Compress(new byte[PortraitTiles * 32])) : 0;
        for (var expression = 0; expression < ExpressionsPerCharacter; expression++)
        {
            var image = blank ? blankImage : Store(CompressedImage(output, ReadPointer(output, images + (sourceCharacter * ExpressionsPerCharacter + expression) * 4)));
            WriteU32(output, images + (id * ExpressionsPerCharacter + expression) * 4, (uint)(GbaBase + image));
        }

        var palette = blank ? new byte[PaletteSize] : output.AsSpan(ReadPointer(output, palettes + sourceCharacter * 4), PaletteSize).ToArray();
        WriteU32(output, palettes + id * 4, (uint)(GbaBase + Store(palette)));
        WriteMarker(output, images - RomLayout.PortraitMarkerSize, id + 1);
        log.Add(blank ? $"portrait character {id}: blank" : $"portrait character {id}: copy of character {sourceCharacter}");
        return new PortraitAddResult(output, id, log);
    }

    /// <summary>The ROM with the staged portrait characters added, in order (the input is not modified).</summary>
    public static byte[] ApplyAdditions(byte[] rom, IEnumerable<PortraitAddition> additions)
    {
        ArgumentNullException.ThrowIfNull(additions);
        var output = rom;
        foreach (var addition in additions)
        {
            output = AddCharacter(output, addition.SourceCharacterId, addition.Blank).Rom;
        }

        return ReferenceEquals(output, rom) ? (byte[])rom.Clone() : output;
    }

    // The image as stored (Malias2); re-encoded when its stored length cannot be read.
    private static byte[] CompressedImage(byte[] rom, int offset)
    {
        if (Malias2.TryGetEncodedLength(rom, offset) is { } length)
        {
            return rom.AsSpan(offset, length).ToArray();
        }

        var pixels = Malias2.Decompress(rom, offset) ?? throw new InvalidDataException($"The portrait image at 0x{offset:X6} could not be read.");
        return Malias2.Compress(pixels);
    }

    private static int ReadPointer(byte[] rom, int at) => (int)BinaryPrimitives.ReadUInt32LittleEndian(rom.AsSpan(at)) - GbaBase;

    private static void WriteMarker(byte[] rom, int at, int count)
    {
        RomLayout.PortraitExpansionMarker.CopyTo(rom.AsSpan(at));
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
