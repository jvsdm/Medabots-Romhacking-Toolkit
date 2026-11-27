using System.Buffers.Binary;
using Medabots.Rom.Compression;
using Medabots.Rom.Images;
using Medabots.Rom.Metadata;
using Medabots.Rom.Projects;

namespace Medabots.Rom.Maps;

/// <summary>
/// A map tileset as pictures and back. The sheet is 16 tiles wide; each tile is drawn with the palette
/// bank the map's layers use it with (bank 0 when unused), and palette index 0 is transparent. Importing
/// maps every opaque pixel to the nearest of the tile's bank colours 1-15. The palette picture is a
/// 16 x 16 grid: one row per bank, one cell per colour.
/// </summary>
public static class MapTilesetCodec
{
    public const int SheetTileWidth = 16;
    public const int PaletteCellSize = 16;

    /// <summary>The tileset sheet as 0xAARRGGBB pixels (width = 128).</summary>
    public static (int Width, int Height, uint[] Argb) ExportSheet(MapTilesetAsset asset)
    {
        ArgumentNullException.ThrowIfNull(asset);
        var tiles = asset.RawTilesetPixelIndices.Length / 64;
        var rows = Math.Max(1, (tiles + SheetTileWidth - 1) / SheetTileWidth);
        int width = SheetTileWidth * 8, height = rows * 8;
        var argb = new uint[width * height];
        var colours = Colours(asset.PaletteBytes);
        for (var t = 0; t < tiles; t++)
        {
            var bank = Bank(asset, t);
            for (var p = 0; p < 64; p++)
            {
                var index = asset.RawTilesetPixelIndices[t * 64 + p];
                argb[((t / SheetTileWidth) * 8 + p / 8) * width + (t % SheetTileWidth) * 8 + p % 8] =
                    index == 0 ? 0u : colours[bank * 16 + index];
            }
        }

        return (width, height, argb);
    }

    /// <summary>
    /// Pixel indices (64 per tile) from an edited sheet of the same size; also how many opaque pixels had
    /// a colour that is not in their tile's bank (they take the nearest one).
    /// </summary>
    public static (byte[] TilePixels, int Approximated) ImportSheet(MapTilesetAsset asset, int width, int height, uint[] argb)
    {
        ArgumentNullException.ThrowIfNull(asset);
        ArgumentNullException.ThrowIfNull(argb);
        var (expectedWidth, expectedHeight, _) = ExportSheet(asset);
        if (width != expectedWidth || height > expectedHeight || height % 8 != 0 || height == 0)
        {
            throw new InvalidDataException($"The tileset picture is {width}x{height}; this tileset is {expectedWidth} wide (16 tiles per row) and up to {expectedHeight} high, in rows of 8 pixels.");
        }

        var colours = Colours(asset.PaletteBytes);
        var pixels = asset.RawTilesetPixelIndices.ToArray();     // tiles below a shorter picture are kept
        var tiles = Math.Min(pixels.Length / 64, height / 8 * SheetTileWidth);
        var approximated = 0;
        for (var t = 0; t < tiles; t++)
        {
            var bank = Bank(asset, t);
            for (var p = 0; p < 64; p++)
            {
                var pixel = argb[((t / SheetTileWidth) * 8 + p / 8) * width + (t % SheetTileWidth) * 8 + p % 8];
                if (pixel >> 24 < 128)
                {
                    pixels[t * 64 + p] = 0;
                    continue;
                }

                var best = 1;
                var bestDistance = int.MaxValue;
                for (var c = 1; c < 16; c++)
                {
                    var distance = Distance(Snap(pixel), colours[bank * 16 + c]);
                    if (distance < bestDistance)
                    {
                        (best, bestDistance) = (c, distance);
                    }
                }

                approximated += bestDistance > 0 ? 1 : 0;
                pixels[t * 64 + p] = (byte)best;
            }
        }

        return (pixels, approximated);
    }

    /// <summary>
    /// A palette for a picture with no palette yet (a new map's tileset): its colours, merged down to 15, in
    /// bank 0; the other banks are black.
    /// </summary>
    public static byte[] PaletteFromPicture(int width, int height, uint[] argb)
    {
        var (palette, _, _) = Expansion.BotArt.BuildPalette([new Expansion.ArtImage(width, height, argb)]);
        var bytes = new byte[MedabotsRomSchema.MapPaletteSize];
        Expansion.BotArt.PaletteBytes(palette).CopyTo(bytes, 0);
        return bytes;
    }

    /// <summary>The 16 palettes as a 16 x 16 grid of colours, each cell <see cref="PaletteCellSize"/> pixels.</summary>
    public static (int Width, int Height, uint[] Argb) ExportPalette(byte[] paletteBytes)
    {
        var colours = Colours(paletteBytes);
        const int size = 16 * PaletteCellSize;
        var argb = new uint[size * size];
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                argb[y * size + x] = colours[(y / PaletteCellSize) * 16 + x / PaletteCellSize];
            }
        }

        return (size, size, argb);
    }

    /// <summary>Palette bytes from a palette grid (any size whose sides are multiples of 16; cell centres are read).</summary>
    public static byte[] ImportPalette(int width, int height, uint[] argb)
    {
        ArgumentNullException.ThrowIfNull(argb);
        if (width % 16 != 0 || height % 16 != 0 || width == 0 || height == 0)
        {
            throw new InvalidDataException($"The palette picture is {width}x{height}; it must be a 16 x 16 grid of colours (e.g. 256x256).");
        }

        var bytes = new byte[MedabotsRomSchema.MapPaletteSize];
        int cellWidth = width / 16, cellHeight = height / 16;
        for (var bank = 0; bank < 16; bank++)
        {
            for (var c = 0; c < 16; c++)
            {
                var pixel = argb[(bank * cellHeight + cellHeight / 2) * width + c * cellWidth + cellWidth / 2];
                var value = (int)((pixel >> 16 & 0xFF) >> 3) | (int)((pixel >> 8 & 0xFF) >> 3) << 5 | (int)((pixel & 0xFF) >> 3) << 10;
                BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan((bank * 16 + c) * 2), (ushort)value);
            }
        }

        return bytes;
    }

    /// <summary>The maps whose tileset graphics are the same data as <paramref name="asset"/>'s.</summary>
    public static IReadOnlyList<int> MapsSharingGraphics(RomFile rom, MapTilesetAsset asset)
    {
        ArgumentNullException.ThrowIfNull(rom);
        var maps = new List<int>();
        for (var map = 0; map < rom.Layout.MapCount; map++)
        {
            if (GbaPointer.TryReadFileOffset(rom.Data, rom.Layout.MapTilesetGraphicsPointerTableOffset + map * 4, out var offset) &&
                offset == asset.GraphicsDataOffset)
            {
                maps.Add(map);
            }
        }

        return maps;
    }

    /// <summary>Patch actions: new compressed graphics and/or palette, pointed at by every listed map.</summary>
    public static IReadOnlyList<RomPatchAction> BuildActions(RomFile rom, MapTilesetPatch patch, FreeSpaceAllocator allocator)
    {
        ArgumentNullException.ThrowIfNull(rom);
        ArgumentNullException.ThrowIfNull(patch);
        ArgumentNullException.ThrowIfNull(allocator);
        var actions = new List<RomPatchAction>();
        void Point(int table, int map, int destination, string what)
        {
            var pointer = new byte[4];
            GbaPointer.WriteFileOffset(pointer, 0, destination);
            actions.Add(RomPatchAction.Create(table + map * 4, pointer, $"Map {map} {what} pointer"));
        }

        if (patch.TilePixels is { Length: > 0 } tiles)
        {
            var compressed = GbaLz77.Compress(TileImageCodec.Pack4BppTiles(tiles));
            var at = allocator.Reserve(compressed.Length, 4);
            actions.Add(RomPatchAction.Create(at, compressed, $"Tileset graphics for map(s) {string.Join(", ", patch.MapIds)}"));
            foreach (var map in patch.MapIds)
            {
                Point(rom.Layout.MapTilesetGraphicsPointerTableOffset, map, at, "tileset graphics");
            }
        }

        if (patch.PaletteBytes is { Length: MedabotsRomSchema.MapPaletteSize } palette)
        {
            var at = allocator.Reserve(palette.Length, 4);
            actions.Add(RomPatchAction.Create(at, palette, $"Tileset palette for map(s) {string.Join(", ", patch.MapIds)}"));
            foreach (var map in patch.MapIds)
            {
                Point(rom.Layout.MapTilesetPalettePointerTableOffset, map, at, "tileset palette");
            }
        }

        return actions;
    }

    private static int Bank(MapTilesetAsset asset, int tile) =>
        tile < asset.TilePaletteBanks.Count && asset.TilePaletteBanks[tile] >= 0 ? asset.TilePaletteBanks[tile] : 0;

    private static uint[] Colours(byte[] paletteBytes)
    {
        var colours = new uint[256];
        for (var i = 0; i < Math.Min(256, paletteBytes.Length / 2); i++)
        {
            var c = BinaryPrimitives.ReadUInt16LittleEndian(paletteBytes.AsSpan(i * 2));
            colours[i] = 0xFF000000u | (uint)((c & 31) << 3) << 16 | (uint)((c >> 5 & 31) << 3) << 8 | (uint)((c >> 10 & 31) << 3);
        }

        return colours;
    }

    private static uint Snap(uint pixel) =>
        0xFF000000u | ((pixel >> 16 & 0xFF) >> 3 << 3) << 16 | ((pixel >> 8 & 0xFF) >> 3 << 3) << 8 | ((pixel & 0xFF) >> 3 << 3);

    private static int Distance(uint a, uint b)
    {
        int dr = (int)(a >> 16 & 0xFF) - (int)(b >> 16 & 0xFF), dg = (int)(a >> 8 & 0xFF) - (int)(b >> 8 & 0xFF), db = (int)(a & 0xFF) - (int)(b & 0xFF);
        return dr * dr + dg * dg + db * db;
    }
}
