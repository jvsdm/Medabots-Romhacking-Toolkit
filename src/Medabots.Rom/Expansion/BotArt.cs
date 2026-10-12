using System.Buffers.Binary;
using Medabots.Rom.Compression;
using Medabots.Rom.Layout;

namespace Medabots.Rom.Expansion;

/// <summary>A picture in 0xAARRGGBB pixels (alpha below 128 = transparent).</summary>
public sealed record ArtImage(int Width, int Height, uint[] Argb)
{
    public bool IsTransparent(int x, int y) => (Argb[y * Width + x] >> 24) < 128;
}

/// <summary>
/// One drawable piece of a bot: a large-image piece (mold, entry, pose) or a mini sprite frame. Names
/// match tools/export_template.py ("large_cabeca_2_m10.png", "mini_pernas_1.png"...).
/// </summary>
public sealed record ArtSlot(string Name, string Part, int Entry, int Pose, int Mold, int Width, int Height,
    bool OwnPalette, bool IsMini, ArtImage Current);

/// <summary>Reads a bot's pieces as pictures and turns pictures back into GBA tiles and palettes.</summary>
public static class BotArt
{
    public const int MaxColors = 15;
    private const int GbaBase = 0x08000000;
    private const int SelectorMask = 0xFF;

    public static readonly (string Part, string Name, int RowSize)[] PreviewParts =
        [("head", "cabeca", 16), ("right_arm", "braco_dir", 16), ("left_arm", "braco_esq", 16), ("legs", "pernas", 32)];

    // Mini sprite components (battle sprite table, 6 per bot): head, right arm x2, left arm x2, legs (3 frames).
    public static readonly (string Name, int Component, int Frame)[] MiniSlots =
    [
        ("mini_cabeca", 0, 0), ("mini_braco_dir_1", 1, 0), ("mini_braco_dir_2", 2, 0), ("mini_braco_esq_1", 3, 0),
        ("mini_braco_esq_2", 4, 0), ("mini_pernas_1", 5, 0), ("mini_pernas_2", 5, 1), ("mini_pernas_3", 5, 2),
    ];

    /// <summary>Every piece of <paramref name="bot"/> in an expanded (or original) Metabee (E) ROM.</summary>
    public static IReadOnlyList<ArtSlot> Slots(byte[] rom, int bot)
    {
        var layout = RomLayout.Resolve(rom);
        var slots = new List<ArtSlot>();
        foreach (var (part, name, rowSize) in PreviewParts)
        {
            var rowAt = PreviewTable(layout, part) + bot * rowSize;
            uint[]? partPalette = null;
            for (var entry = 0; entry < rowSize / 4; entry++)
            {
                var value = U32(rom, rowAt + 4 * entry);
                var mold = (int)(value & 0x3F);
                if (mold == 0)
                {
                    continue;
                }

                var record = (int)U32(rom, layout.CompositePreviewDescriptorPointerTableOffset + 4 * mold) - GbaBase;
                int height = rom[record + 0x12], width = rom[record + 0x13];   // 0x12 = altura, 0x13 = largura (como em LargePartDisplay)
                var blobs = (int)U32(rom, record) - GbaBase;
                var selector = (int)((value >> 6) & SelectorMask);
                var poses = (int)((value >> 15) & 1) + 1;
                for (var pose = 0; pose < poses; pose++)
                {
                    var image = U32(rom, blobs + 8 * (selector + pose));
                    var palette = U32(rom, blobs + 8 * (selector + pose) + 4);
                    var ownPalette = palette != image;
                    if (ownPalette)
                    {
                        partPalette ??= ReadPalette(rom, (int)palette - GbaBase);
                    }

                    var pixels = GbaLz77.Decompress(rom, (int)image - GbaBase) ?? [];
                    var suffix = poses > 1 ? $"_pose{pose + 1}" : "";
                    slots.Add(new ArtSlot($"large_{name}_{entry + 1}_m{mold:00}{suffix}", name, entry, pose, mold,
                        width, height, ownPalette, false,
                        Render(pixels, width, height, partPalette ?? ReadPalette(rom, MiniSpritePalette.EuPalettes))));
                }
            }
        }

        var family = rom[layout.CompositeBattleSpritePaletteFamilyTableOffset + bot];
        var miniPalette = ReadPalette(rom, MiniSpritePalette.EuPalettes + 32 * family);
        foreach (var (name, component, frame) in MiniSlots)
        {
            var at = (int)U32(rom, layout.CompositeBattleSpritePointerTableOffset + (bot * 6 + component) * 4) - GbaBase;
            var payload = ExpansionCodecs.MaliasDecompress(rom, at).Payload;
            var frameBytes = payload.AsSpan(frame * 128, Math.Min(128, payload.Length - frame * 128)).ToArray();
            slots.Add(new ArtSlot(name, "mini", 0, frame, 0, 16, 16, false, true, Render(frameBytes, 16, 16, miniPalette)));
        }

        return slots;
    }

    /// <summary>
    /// Shared palette for a set of pictures: colours snapped to the GBA grid, then merged down to
    /// <see cref="MaxColors"/> (near-identical shades go first, so small details of a distinct colour stay).
    /// Returns the palette (index 0 transparent) and each picture's pixel indices.
    /// </summary>
    public static (uint[] Palette, byte[][] Indices, int SourceColors) BuildPalette(IReadOnlyList<ArtImage> images)
    {
        var count = new Dictionary<uint, int>();
        foreach (var image in images)
        {
            foreach (var pixel in image.Argb)
            {
                if ((pixel >> 24) >= 128)
                {
                    var c = Snap(pixel);
                    count[c] = count.GetValueOrDefault(c) + 1;
                }
            }
        }

        var sourceColors = count.Count;
        var merged = count.Keys.ToDictionary(c => c, c => c);
        while (count.Count > MaxColors)
        {
            uint Nearest(uint c) => count.Keys.Where(o => o != c)
                .OrderBy(o => Distance(c, o)).ThenByDescending(o => count[o]).ThenBy(o => o).First();
            var rare = count.Keys.OrderBy(c => Distance(c, Nearest(c)) * count[c]).ThenBy(c => c).First();
            var target = Nearest(rare);
            count[target] += count[rare];
            count.Remove(rare);
            foreach (var key in merged.Keys.ToList().Where(k => merged[k] == rare))
            {
                merged[key] = target;
            }
        }

        var palette = new uint[16];
        var ordered = count.OrderByDescending(pair => pair.Value).Select(pair => pair.Key).ToList();
        for (var i = 0; i < ordered.Count; i++)
        {
            palette[i + 1] = ordered[i];
        }

        var index = ordered.Select((c, i) => (c, i)).ToDictionary(t => t.c, t => (byte)(t.i + 1));
        var indices = images.Select(image => image.Argb
            .Select(pixel => (pixel >> 24) < 128 ? (byte)0 : index[merged[Snap(pixel)]]).ToArray()).ToArray();
        return (palette, indices, sourceColors);
    }

    /// <summary>Pixel indices (row by row) to 4bpp GBA tiles (8x8, left to right, top to bottom).</summary>
    public static byte[] ToTiles(byte[] indices, int width, int height)
    {
        var tiles = new byte[width * height / 2];
        var o = 0;
        for (var ty = 0; ty < height / 8; ty++)
        {
            for (var tx = 0; tx < width / 8; tx++)
            {
                for (var y = 0; y < 8; y++)
                {
                    for (var x = 0; x < 8; x += 2)
                    {
                        var p = (ty * 8 + y) * width + tx * 8 + x;
                        tiles[o++] = (byte)(indices[p] | indices[p + 1] << 4);
                    }
                }
            }
        }

        return tiles;
    }

    public static byte[] PaletteBytes(uint[] palette)
    {
        var bytes = new byte[32];
        for (var i = 0; i < 16; i++)
        {
            var c = palette[i];
            var value = (int)((c >> 16 & 0xFF) / 8) | (int)((c >> 8 & 0xFF) / 8) << 5 | (int)((c & 0xFF) / 8) << 10;
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(2 * i), (ushort)value);
        }

        return bytes;
    }

    internal static int PreviewTable(RomLayout layout, string part) => part switch
    {
        "head" => layout.CompositePreviewHeadAppearanceTableOffset,
        "right_arm" => layout.CompositePreviewRightArmAppearanceTableOffset,
        "left_arm" => layout.CompositePreviewLeftArmAppearanceTableOffset,
        _ => layout.CompositePreviewLegsAppearanceTableOffset,
    };

    private static ArtImage Render(byte[] tiles, int width, int height, uint[] palette)
    {
        var argb = new uint[width * height];
        var tw = width / 8;
        for (var t = 0; t < Math.Min(tiles.Length / 32, tw * (height / 8)); t++)
        {
            for (var y = 0; y < 8; y++)
            {
                for (var x = 0; x < 8; x++)
                {
                    var b = tiles[t * 32 + y * 4 + x / 2];
                    var c = (x & 1) == 1 ? b >> 4 : b & 15;
                    argb[((t / tw) * 8 + y) * width + (t % tw) * 8 + x] = c == 0 ? 0u : palette[c];
                }
            }
        }

        return new ArtImage(width, height, argb);
    }

    private static uint[] ReadPalette(byte[] rom, int offset)
    {
        var palette = new uint[16];
        for (var i = 0; i < 16; i++)
        {
            var c = BinaryPrimitives.ReadUInt16LittleEndian(rom.AsSpan(offset + 2 * i));
            palette[i] = 0xFF000000u | (uint)((c & 31) * 8) << 16 | (uint)((c >> 5 & 31) * 8) << 8 | (uint)((c >> 10 & 31) * 8);
        }

        return palette;
    }

    private static uint Snap(uint pixel)
    {
        uint Channel(uint v) => (uint)Math.Min(248, (int)Math.Round(v / 8.0, MidpointRounding.ToEven) * 8);
        return 0xFF000000u | Channel(pixel >> 16 & 0xFF) << 16 | Channel(pixel >> 8 & 0xFF) << 8 | Channel(pixel & 0xFF);
    }

    private static int Distance(uint a, uint b)
    {
        int dr = (int)(a >> 16 & 0xFF) - (int)(b >> 16 & 0xFF), dg = (int)(a >> 8 & 0xFF) - (int)(b >> 8 & 0xFF), db = (int)(a & 0xFF) - (int)(b & 0xFF);
        return dr * dr + dg * dg + db * db;
    }

    private static uint U32(byte[] data, int offset) => BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(offset));
}
