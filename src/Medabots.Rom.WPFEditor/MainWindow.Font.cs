using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Medabots.Rom.Text;

namespace Medabots.Rom.WPFEditor;

/// <summary>Font tab (European ROMs): extended font toggle, normal and italic sheets with advance lines, PNG export.</summary>
public partial class MainWindow
{
    private const int FontSheetScale = 4;

    private bool _syncingFontSection;

    private void RefreshFontSection()
    {
        if (UseExtendedFontCheckBox is null)
        {
            return;
        }

        _syncingFontSection = true;
        UseExtendedFontCheckBox.IsChecked = _project.UseExtendedFont;
        _syncingFontSection = false;

        var rom = _session?.RomFile.Data;
        var european = rom is not null && ExtendedFont.IsEuropean(rom);
        var layout = rom is null ? null : ExtendedFont.Locate(rom);
        FontEditorPanel.IsEnabled = european && layout is not null;
        if (rom is null)
        {
            ExtendedFontStatusLabel.Text = "No ROM loaded.";
        }
        else if (!european)
        {
            ExtendedFontStatusLabel.Text = "Font changes are for European (E) ROMs only (Metabee or Rokusho). This ROM is not (E).";
        }
        else if (layout is null)
        {
            ExtendedFontStatusLabel.Text = "Dialogue font not found in this ROM.";
        }
        else
        {
            var state = !ExtendedFont.CanApply(rom)
                ? "Extended font not applicable: the slots 0x4F/0x7D-0x7F already hold other glyphs."
                : _project.UseExtendedFont ? "Extended font applied on export." : "Extended font off: the original font is kept.";
            ExtendedFontStatusLabel.Text = $"{state}  Font 0x{layout.Font:X6}, italic 0x{layout.ItalicFont:X6}, widths 0x{layout.Widths:X6} / 0x{layout.ItalicWidths:X6}.";
        }

        FontSheetImage.Source = european && layout is not null ? RenderFontSheet(rom!, false) : null;
        ItalicFontSheetImage.Source = european && layout is not null ? RenderFontSheet(rom!, true) : null;
    }

    private void OnUseExtendedFontChanged(object sender, RoutedEventArgs e)
    {
        if (_syncingFontSection)
        {
            return;
        }

        _project.UseExtendedFont = UseExtendedFontCheckBox.IsChecked == true;
        RefreshFontSection();
        RebuildMessagePreview(keepBox: true);
        UpdateStatus();
    }

    private async void OnExtendedFontHelpClicked(object sender, RoutedEventArgs e) =>
        await DisplayAlertAsync("Extended font", ExtendedFont.Summary, "OK");

    private async void OnExportNormalFontClicked(object sender, RoutedEventArgs e) => await ExportOriginalFontAsync(false);

    private async void OnExportItalicFontClicked(object sender, RoutedEventArgs e) => await ExportOriginalFontAsync(true);

    // Original font (as in the ROM, without the extension) as an indexed 128x128 PNG: 16 glyphs per row, palette indices kept.
    private async Task ExportOriginalFontAsync(bool italic)
    {
        var rom = _session?.RomFile.Data;
        var layout = rom is null ? null : ExtendedFont.Locate(rom);
        if (rom is null || layout is null)
        {
            return;
        }

        var name = italic ? "font_italic.png" : "font_normal.png";
        var initial = Path.Combine(Path.GetDirectoryName(_project.SourceRomPath) ?? Environment.CurrentDirectory, name);
        var path = PickSaveFilePath("Export original font", "PNG image (*.png)|*.png", initial);
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        const int size = 128;
        var pixels = FontIndexSheet(rom, italic ? layout.ItalicFont : layout.Font);
        var palette = new BitmapPalette(ExtendedFont.Palette.Select(Rgb555ToColor).ToList());
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(BitmapSource.Create(size, size, 96, 96, PixelFormats.Indexed8, palette, pixels, size)));
        await using (var stream = File.Create(path))
        {
            encoder.Save(stream);
        }

        await DisplayAlertAsync("Font exported", $"Saved {(italic ? "italic" : "normal")} font to:{Environment.NewLine}{path}", "OK");
    }

    // Palette indices of the 128 glyphs, 16 per row (128x128, one byte per pixel).
    private static byte[] FontIndexSheet(byte[] rom, int font)
    {
        const int size = 128;
        var pixels = new byte[size * size];
        for (var glyph = 0; glyph < ExtendedFont.GlyphCount; glyph++)
        {
            var data = rom.AsSpan(font + glyph * ExtendedFont.GlyphSize, ExtendedFont.GlyphSize);
            for (var y = 0; y < 16; y++)
            {
                for (var x = 0; x < 8; x++)
                {
                    pixels[(glyph / 16 * 16 + y) * size + glyph % 16 * 8 + x] = (byte)ExtendedFont.Pixel(data, x, y);
                }
            }
        }

        return pixels;
    }

    private static System.Windows.Media.Color Rgb555ToColor(ushort value) =>
        System.Windows.Media.Color.FromRgb((byte)((value & 31) * 255 / 31), (byte)((value >> 5 & 31) * 255 / 31), (byte)((value >> 10 & 31) * 255 / 31));

    // 16x8 glyphs at 4x: grid on the 8x16 cells, red line at the advance of each glyph (what the game uses).
    private BitmapSource RenderFontSheet(byte[] rom, bool italic)
    {
        s_useExtendedFont = _project.UseExtendedFont;
        UpdateFontLayout(rom);
        const int cells = 128;
        const int size = cells * FontSheetScale;
        const uint grid = 0xFFC8C8C8;
        const uint advanceLine = 0xFFE03030;
        var widths = italic ? s_fontLayout.ItalicWidths : s_fontLayout.Widths;
        var pixels = new uint[size * size];
        for (var glyph = 0; glyph < ExtendedFont.GlyphCount; glyph++)
        {
            var left = glyph % 16 * 8 * FontSheetScale;
            var top = glyph / 16 * 16 * FontSheetScale;
            int advance = rom[widths + glyph * 2];
            if (s_useExtendedFont && s_extendedWidths is { } ext && glyph >= ExtendedFont.FirstWidthGlyph)
            {
                advance = (italic ? ext.Italic : ext.Main)[(glyph - ExtendedFont.FirstWidthGlyph) * 2];
            }

            for (var y = 0; y < 16 * FontSheetScale; y++)
            {
                for (var x = 0; x < 8 * FontSheetScale; x++)
                {
                    var value = GlyphPixel(rom, glyph, italic, x / FontSheetScale, y / FontSheetScale);
                    var color = value != 1 ? Blend(0xFFFFFFFF, 0xFF000000, value == 0 ? 1.0 : (value - 1) / 14.0) : 0xFFFFFFFF;
                    if (x == 0 || y == 0)
                    {
                        color = grid;
                    }

                    if (advance is > 0 and < 8 && x == advance * FontSheetScale)
                    {
                        color = advanceLine;
                    }

                    pixels[(top + y) * size + left + x] = color;
                }
            }
        }

        var bitmap = BitmapSource.Create(size, size, 96, 96, PixelFormats.Bgra32, null, pixels, size * 4);
        bitmap.Freeze();
        return bitmap;
    }
}
