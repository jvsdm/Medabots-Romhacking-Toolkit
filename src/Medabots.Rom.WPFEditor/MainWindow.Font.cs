using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Medabots.Rom.Text;

namespace Medabots.Rom.WPFEditor;

/// <summary>Font tab: extended font toggle (Text.ExtendedFont) and a 16x8 sheet of the dialogue font.</summary>
public partial class MainWindow
{
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
        ExtendedFontStatusLabel.Text = rom is null
            ? "No ROM loaded."
            : ExtendedFont.CanApply(rom)
                ? _project.UseExtendedFont ? "Applied on export." : "Off: the original font is kept."
                : "Not applicable to this ROM (not Metabee (E), or the slots hold other glyphs).";
        FontSheetImage.Source = rom is null ? null : RenderFontSheet(rom);
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

    // 128 glyphs, 16 per row, as the preview draws them (background white, F = black).
    private BitmapSource RenderFontSheet(byte[] rom)
    {
        s_useExtendedFont = _project.UseExtendedFont;
        const int size = 128;
        var pixels = new uint[size * size];
        for (var glyph = 0; glyph < 128; glyph++)
        {
            var left = glyph % 16 * 8;
            var top = glyph / 16 * 16;
            for (var y = 0; y < 16; y++)
            {
                for (var x = 0; x < 8; x++)
                {
                    var value = rom.Length >= MainFontOffset + 128 * 64 ? GlyphPixel(rom, glyph, false, x, y) : 0;
                    pixels[(top + y) * size + left + x] = value > 1 ? Blend(0xFFFFFFFF, 0xFF000000, (value - 1) / 14.0) : 0xFFFFFFFF;
                }
            }
        }

        var bitmap = BitmapSource.Create(size, size, 96, 96, PixelFormats.Bgra32, null, pixels, size * 4);
        bitmap.Freeze();
        return bitmap;
    }
}
