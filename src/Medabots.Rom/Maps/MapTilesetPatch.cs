namespace Medabots.Rom.Maps;

/// <summary>
/// New tileset graphics and/or palette for a set of maps (a tileset is usually shared by several maps).
/// <paramref name="TilePixels"/>: one byte per pixel (palette index 0-15), 64 per tile in tile order;
/// <paramref name="PaletteBytes"/>: the 16 BG palettes (0x200 bytes, BGR555). Null keeps the current one.
/// </summary>
public sealed record MapTilesetPatch(IReadOnlyList<int> MapIds, byte[]? TilePixels, byte[]? PaletteBytes);
