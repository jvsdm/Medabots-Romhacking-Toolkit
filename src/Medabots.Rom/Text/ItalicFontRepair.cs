namespace Medabots.Rom.Text;

/// <summary>
/// The European releases lost the dark shades of their italic font (B-F became 0 = transparent, so italic lines
/// such as "Click!" fade into the text box). Each lost pixel gets a shade from the number of inked 4-neighbours:
/// 4 -> F, 3 -> E, fewer -> D. Used by the Font tab (extended font) and by the ROM expansion.
/// </summary>
public static class ItalicFontRepair
{
    public const int FontSize = 128 * ExtendedFont.GlyphSize;

    /// <summary>Repaired copy of the italic font at <paramref name="offset"/>; null when it is not damaged.</summary>
    public static byte[]? Repair(ReadOnlySpan<byte> rom, int offset, out int fixedPixels)
    {
        fixedPixels = 0;
        if (offset < 0 || offset + FontSize > rom.Length)
        {
            return null;
        }

        var font = rom.Slice(offset, FontSize);
        var pixels = new int[FontSize * 2];
        for (var i = 0; i < FontSize; i++)
        {
            pixels[2 * i] = font[i] & 15;
            pixels[2 * i + 1] = font[i] >> 4;
        }

        if (!pixels.Contains(0) || pixels.Any(value => value >= 0xB))
        {
            return null;
        }

        var output = font.ToArray();
        for (var glyph = 0; glyph < FontSize / 64; glyph++)
        {
            var g = pixels.AsSpan(glyph * 128, 128).ToArray();
            int At(int y, int x) => g[(y / 8) * 64 + (y % 8) * 8 + x];
            var repaired = (int[])g.Clone();
            for (var y = 0; y < 16; y++)
            {
                for (var x = 0; x < 8; x++)
                {
                    if (At(y, x) != 0)
                    {
                        continue;
                    }

                    var ink = 0;
                    foreach (var (dy, dx) in new[] { (-1, 0), (1, 0), (0, -1), (0, 1) })
                    {
                        if (y + dy is >= 0 and < 16 && x + dx is >= 0 and < 8 && At(y + dy, x + dx) != 1)
                        {
                            ink++;
                        }
                    }

                    repaired[(y / 8) * 64 + (y % 8) * 8 + x] = ink switch { 4 => 0xF, 3 => 0xE, _ => 0xD };
                    fixedPixels++;
                }
            }

            for (var i = 0; i < 64; i++)
            {
                output[glyph * 64 + i] = (byte)(repaired[2 * i] | repaired[2 * i + 1] << 4);
            }
        }

        return output;
    }
}
