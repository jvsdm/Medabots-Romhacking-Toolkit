using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Medabots.Rom.Text;

namespace Medabots.Rom.WPFEditor;

/// <summary>
/// Rasterizes a TrueType/OpenType font file into the 8x16 dialogue cells (Text.ImportedFont): pixel size from the
/// capital height (H = 7 px), glyph outlines drawn 8x larger and averaged into coverage, advances from the font's
/// own metrics. Variable fonts are read at their default instance.
/// </summary>
internal static class TrueTypeRasterizer
{
    private const int Oversample = 8;

    public static ImportedFont Import(string normalPath, string? italicPath)
    {
        var normal = new GlyphTypeface(new Uri(normalPath));
        var size = ImportedFont.CapHeight / normal.CapsHeight;
        var italic = italicPath is null ? null : new GlyphTypeface(new Uri(italicPath));
        var name = normal.FamilyNames.Values.FirstOrDefault() ?? System.IO.Path.GetFileNameWithoutExtension(normalPath);
        return ImportedFont.Build(name, size, ch => Rasterize(normal, size, ch), italic is null ? null : ch => Rasterize(italic, size, ch));
    }

    public static RasterizedGlyph? Rasterize(GlyphTypeface font, double size, char ch)
    {
        if (!font.CharacterToGlyphMap.TryGetValue(ch, out var index))
        {
            return null;
        }

        const int width = ImportedFont.CellWidth * Oversample;
        const int height = ImportedFont.CellHeight * Oversample;
        var outline = font.GetGlyphOutline(index, size * Oversample, size * Oversample);
        var visual = new DrawingVisual();
        using (var context = visual.RenderOpen())
        {
            context.DrawRectangle(System.Windows.Media.Brushes.White, null, new Rect(0, 0, width, height));
            // Outline origin = pen position on the baseline (y grows downward)
            context.PushTransform(new TranslateTransform(0, ImportedFont.Baseline * Oversample));
            context.DrawGeometry(System.Windows.Media.Brushes.Black, null, outline);
        }

        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        var pixels = new byte[width * height * 4];
        bitmap.CopyPixels(pixels, width * 4, 0);

        var coverage = new byte[ImportedFont.CellWidth * ImportedFont.CellHeight];
        for (var y = 0; y < ImportedFont.CellHeight; y++)
        {
            for (var x = 0; x < ImportedFont.CellWidth; x++)
            {
                var ink = 0;
                for (var sy = 0; sy < Oversample; sy++)
                {
                    for (var sx = 0; sx < Oversample; sx++)
                    {
                        ink += 255 - pixels[((y * Oversample + sy) * width + x * Oversample + sx) * 4];
                    }
                }

                coverage[y * ImportedFont.CellWidth + x] = (byte)(ink / (Oversample * Oversample));
            }
        }

        var bounds = outline.Bounds;
        var clipped = !bounds.IsEmpty && (bounds.Left < -0.5 * Oversample || bounds.Right > width + 0.5 * Oversample
            || bounds.Top + ImportedFont.Baseline * Oversample < -0.5 * Oversample || bounds.Bottom + ImportedFont.Baseline * Oversample > height + 0.5 * Oversample);
        return new RasterizedGlyph(coverage, font.AdvanceWidths[index] * size, clipped);
    }
}
