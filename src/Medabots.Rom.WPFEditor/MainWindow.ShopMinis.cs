using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Medabots.Rom.Metadata;

namespace Medabots.Rom.WPFEditor;

// Minis of the bots sold in the Shops tab. Same battle composite the Sprites tab draws (head, both arms, legs,
// family palette), read from the loaded ROM (retail or decomp build; the layout sidecar gives the offsets) and cached by bot id.
public partial class MainWindow
{
    private readonly Dictionary<int, BitmapSource?> _shopMiniCache = [];

    private BitmapSource? GetShopMiniBitmap(int botId)
    {
        if (_shopMiniCache.TryGetValue(botId, out var cached))
        {
            return cached;
        }

        BitmapSource? bitmap = null;
        if (_session is not null)
        {
            try
            {
                var romFile = _session.RomFile;
                // como o jogo: cada componente e uma camada 16x16 do bot inteiro, todas na mesma origem
                // (pernas: 1o dos 3 quadros de andar; bracos: 1o quadro). Ordem: braco esq., pernas, cabeca, braco dir.
                var head = _imageAssetRepository.ReadBattleCompositeSpriteComponent(romFile, botId, 0).Image;
                var rightArm = _imageAssetRepository.ReadBattleCompositeSpriteComponent(romFile, botId, 1).Image;
                var leftArm = _imageAssetRepository.ReadBattleCompositeSpriteComponent(romFile, botId, 3).Image;
                var legs = _imageAssetRepository.ReadBattleCompositeSpriteComponent(romFile, botId, 5).Image;
                var swatches = BuildPaletteSwatches(head.PaletteBytes);
                var pixels = new byte[16 * 16 * 4];
                foreach (var layer in new[] { leftArm, legs, head, rightArm })
                {
                    BlitMiniLayer(layer, swatches, pixels);
                }
                var mini = BitmapSource.Create(16, 16, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null, pixels, 16 * 4);
                mini.Freeze();
                bitmap = mini;
            }
            catch (Exception ex) when (ex is InvalidDataException or ArgumentException or InvalidOperationException)
            {
                bitmap = null;
            }
        }

        _shopMiniCache[botId] = bitmap;
        return bitmap;
    }

    // Copia os pixels de uma camada (tiles 8x8, 64 indices cada) para o quadro 16x16; indice 0 e transparente.
    private static void BlitMiniLayer(Medabots.Rom.Images.IndexedImage image, IReadOnlyList<Models.PaletteSwatchItem> swatches, byte[] output)
    {
        for (var tile = 0; tile < image.TileWidth * image.TileHeight; tile++)
        {
            int tx = tile % image.TileWidth * 8, ty = tile / image.TileWidth * 8;
            for (var i = 0; i < 64; i++)
            {
                int x = tx + i % 8, y = ty + i / 8, src = tile * 64 + i;
                if (x >= 16 || y >= 16 || src >= image.PixelIndices.Length || image.PixelIndices[src] == 0 ||
                    image.PixelIndices[src] >= swatches.Count)
                {
                    continue;
                }
                var c = swatches[image.PixelIndices[src]].Color;
                var o = (y * 16 + x) * 4;
                output[o] = c.B; output[o + 1] = c.G; output[o + 2] = c.R; output[o + 3] = 255;
            }
        }
    }

    private void RefreshShopSlotMini(System.Windows.Controls.Image image, byte itemId, bool isIgnored)
    {
        var show = itemId != MedabotsRomSchema.EmptyShopSlot && !isIgnored;
        var bitmap = show ? GetShopMiniBitmap(itemId) : null;
        image.Source = bitmap;
        image.Visibility = bitmap is null ? Visibility.Collapsed : Visibility.Visible;
        image.ToolTip = show ? BotLabel(itemId) : null;
    }
}
