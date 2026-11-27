using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Medabots.Rom.Maps;
using Medabots.Rom.Projects;

namespace Medabots.Rom.WPFEditor;

/// <summary>
/// Tileset and palette of the loaded map as PNG files: export, edit in any paint program, import. The
/// import is staged in the project (MapTilesetPatches) for every map sharing the tileset, or just this
/// one, like the other map edits; the preview reads maps from a copy of the ROM with those patches applied.
/// </summary>
public partial class MainWindow
{
    private const string PngFilter = "PNG picture (*.png)|*.png";
    private RomFile? _tilesetPreviewRom;
    private RomFile? _mapAdditionsRom;

    /// <summary>The session's ROM with the staged new maps added (what the other map edits apply to).</summary>
    private RomFile MapAdditionsRom()
    {
        var session = _session ?? throw new InvalidOperationException("No ROM is open.");
        if (_project.MapAdditions.Count == 0 && _project.PortraitAdditions.Count == 0)
        {
            return session.RomFile;
        }

        // In the order Export applies them: new maps, then new portrait characters.
        return _mapAdditionsRom ??= new RomFile(session.RomFile.FilePath,
            Images.PortraitExpander.ApplyAdditions(MapExpander.ApplyAdditions(session.RomFile.Data, _project.MapAdditions), _project.PortraitAdditions));
    }

    /// <summary>The ROM maps are read from: the session's, plus the staged new maps and tileset patches.</summary>
    private RomFile MapSourceRom()
    {
        var baseRom = MapAdditionsRom();
        if (_project.MapTilesetPatches.Count == 0)
        {
            return baseRom;
        }

        if (_tilesetPreviewRom is null)
        {
            var data = baseRom.Data.ToArray();
            var allocator = FreeSpaceAllocator.ForRom(data);
            foreach (var patch in _project.MapTilesetPatches)
            {
                foreach (var action in MapTilesetCodec.BuildActions(baseRom, patch, allocator))
                {
                    if (action.Offset + action.Data.Length > data.Length)
                    {
                        var old = data.Length;
                        Array.Resize(ref data, FreeSpaceAllocator.AlignUp(action.Offset + action.Data.Length, 0x100000));
                        data.AsSpan(old).Fill(0xFF);
                    }

                    action.Data.CopyTo(data, action.Offset);
                }
            }

            _tilesetPreviewRom = new RomFile(baseRom.FilePath, data);
        }

        return _tilesetPreviewRom;
    }

    private void ResetTilesetPreview()
    {
        _tilesetPreviewRom = null;
        _mapAdditionsRom = null;
        _mapTilesetCache.Clear();
        _mapTilesetOptions.Clear();
    }

    private void OnExportMapTilesetClicked(object? sender, RoutedEventArgs e)
    {
        if (_loadedMapTileset is not { } asset)
        {
            return;
        }

        var path = PickSaveFilePath("Export the map's tileset", PngFilter, $"tileset_map{asset.MapId:D3}.png");
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        var (width, height, argb) = MapTilesetCodec.ExportSheet(asset);
        SaveArgbPng(path, width, height, argb);
        MapTilesetSummaryLabel.Text = $"Tileset exported: {Path.GetFileName(path)} ({width / 8 * (height / 8)} tile slots, 16 per row). " +
                                      "Keep the size; transparent = colour 0; each tile keeps the palette row it is shown with.";
    }

    private void OnExportMapPaletteClicked(object? sender, RoutedEventArgs e)
    {
        if (_loadedMapTileset is not { } asset)
        {
            return;
        }

        var path = PickSaveFilePath("Export the map's palettes", PngFilter, $"palette_map{asset.MapId:D3}.png");
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        var (width, height, argb) = MapTilesetCodec.ExportPalette(asset.PaletteBytes);
        SaveArgbPng(path, width, height, argb);
        MapTilesetSummaryLabel.Text = $"Palettes exported: {Path.GetFileName(path)} (16 rows of 16 colours; row = palette bank).";
    }

    private async void OnImportMapTilesetClicked(object? sender, RoutedEventArgs e) => await ImportTilesetPictureAsync(palette: false);

    private async void OnImportMapPaletteClicked(object? sender, RoutedEventArgs e) => await ImportTilesetPictureAsync(palette: true);

    private async Task ImportTilesetPictureAsync(bool palette)
    {
        if (_loadedMapTileset is not { } asset || _session is null)
        {
            return;
        }

        var path = PickOpenFilePath(palette ? "Import the palettes (16 x 16 colour grid)" : "Import the tileset picture", PngFilter);
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        try
        {
            var (width, height, argb) = ReadArgbPng(path);
            byte[]? tiles = null, colours = null;
            var note = "";
            if (palette)
            {
                colours = MapTilesetCodec.ImportPalette(width, height, argb);
            }
            else
            {
                var (pixels, approximated) = MapTilesetCodec.ImportSheet(asset, width, height, argb);
                tiles = pixels;
                note = approximated > 0 ? $" {approximated} pixel(s) had colours outside their palette row and took the nearest one." : "";
            }

            var sharing = MapTilesetCodec.MapsSharingGraphics(MapSourceRom(), asset);
            var maps = new List<int> { asset.MapId };
            if (sharing.Count > 1)
            {
                var answer = System.Windows.MessageBox.Show(this,
                    $"This tileset is used by {sharing.Count} maps ({string.Join(", ", sharing.Take(12))}{(sharing.Count > 12 ? "..." : "")}).\n\n" +
                    "Yes: change it for all of them.\nNo: only map " + asset.MapId + " (it gets its own copy).",
                    palette ? "Import palettes" : "Import tileset", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (answer == MessageBoxResult.Cancel)
                {
                    return;
                }

                if (answer == MessageBoxResult.Yes)
                {
                    maps = sharing.ToList();
                }
            }

            StageTilesetPatch(maps, tiles, colours);
            MapTilesetSummaryLabel.Text = $"{(palette ? "Palettes" : "Tileset")} imported for map(s) {string.Join(", ", maps)} (staged; written on export).{note}";
        }
        catch (Exception ex) when (ex is InvalidDataException or IOException or NotSupportedException or FileFormatException or UnauthorizedAccessException)
        {
            await DisplayAlertAsync(palette ? "Import palettes" : "Import tileset", ex.Message, "OK");
        }
    }

    // One patch per map set: a new import for the same maps replaces that part (tiles or palette).
    private void StageTilesetPatch(IReadOnlyList<int> maps, byte[]? tiles, byte[]? palette)
    {
        var key = maps.OrderBy(id => id).ToArray();
        var existing = _project.MapTilesetPatches.FirstOrDefault(patch => patch.MapIds.OrderBy(id => id).SequenceEqual(key));
        if (existing is not null)
        {
            _project.MapTilesetPatches.Remove(existing);
            tiles ??= existing.TilePixels;
            palette ??= existing.PaletteBytes;
        }

        _project.MapTilesetPatches.Add(new MapTilesetPatch(key, tiles, palette));
        var mapId = _loadedMapTileset?.MapId;
        ResetTilesetPreview();
        if (mapId is { } id)
        {
            var asset = _mapTilesetRepository.ReadMap(MapSourceRom(), id, MapDisplayName(id));
            _mapTilesetCache[id] = asset;
            _loadedMapTileset = asset;
            PopulateMapPreview(asset);
        }

        UpdateStatus();
    }

    private void RemoveTilesetPatchesForMap(int mapId)
    {
        var removed = _project.MapTilesetPatches.Where(patch => patch.MapIds.Contains(mapId)).ToList();
        foreach (var patch in removed)
        {
            _project.MapTilesetPatches.Remove(patch);
        }

        if (removed.Count > 0)
        {
            ResetTilesetPreview();
        }
    }

    private static (int Width, int Height, uint[] Argb) ReadArgbPng(string path)
    {
        using var stream = File.OpenRead(path);
        var frame = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad).Frames[0];
        var bgra = new FormatConvertedBitmap(frame, PixelFormats.Bgra32, null, 0);
        var bytes = new byte[frame.PixelWidth * frame.PixelHeight * 4];
        bgra.CopyPixels(bytes, frame.PixelWidth * 4, 0);
        var argb = new uint[frame.PixelWidth * frame.PixelHeight];
        Buffer.BlockCopy(bytes, 0, argb, 0, bytes.Length);
        return (frame.PixelWidth, frame.PixelHeight, argb);
    }

    private static void SaveArgbPng(string path, int width, int height, uint[] argb)
    {
        var bytes = new byte[argb.Length * 4];
        Buffer.BlockCopy(argb, 0, bytes, 0, bytes.Length);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, bytes, width * 4)));
        using var stream = File.Create(path);
        encoder.Save(stream);
    }
}
