using System.IO;
using System.Windows;
using Medabots.Rom.Editor;
using Medabots.Rom.Maps;
using WpfButton = System.Windows.Controls.Button;
using WpfComboBox = System.Windows.Controls.ComboBox;
using WpfGrid = System.Windows.Controls.Grid;
using WpfRadioButton = System.Windows.Controls.RadioButton;
using WpfStackPanel = System.Windows.Controls.StackPanel;
using WpfTextBlock = System.Windows.Controls.TextBlock;
using WpfTextBox = System.Windows.Controls.TextBox;

namespace Medabots.Rom.WPFEditor;

/// <summary>
/// The green "+" of the Maps tab: "New map..." (an empty map of a chosen size, with a blank or an existing
/// tileset, optionally imported from PNGs right away) or "Copy map..." (a copy of an existing map).
/// </summary>
public partial class MainWindow
{
    private async void OnAddMapClicked(object? sender, RoutedEventArgs e)
    {
        if (_session is null)
        {
            await DisplayAlertAsync("New map", "Open a ROM first.", "OK");
            return;
        }

        var menu = new System.Windows.Controls.ContextMenu
        {
            PlacementTarget = sender as UIElement,
            Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom,
            // Not the + button's big bold glyph font, which a context menu would inherit.
            FontSize = 14,
            FontWeight = FontWeights.Normal,
        };
        var newMap = new System.Windows.Controls.MenuItem { Header = "New map...", ToolTip = "An empty map of the size you choose" };
        newMap.Click += OnNewBlankMapClicked;
        var copyMap = new System.Windows.Controls.MenuItem { Header = "Copy map...", ToolTip = "A copy of an existing map" };
        copyMap.Click += OnCopyMapClicked;
        menu.Items.Add(newMap);
        menu.Items.Add(copyMap);
        menu.IsOpen = true;
    }

    private async void OnNewBlankMapClicked(object? sender, RoutedEventArgs e)
    {
        if (_session is null)
        {
            return;
        }

        var layout = MapSourceRom().Layout;
        var width = new WpfTextBox { Text = "20", Width = 60, Padding = new Thickness(4, 2, 4, 2) };
        var height = new WpfTextBox { Text = "18", Width = 60, Padding = new Thickness(4, 2, 4, 2), Margin = new Thickness(6, 0, 0, 0) };
        var settings = new WpfComboBox { MinWidth = 420, MaxDropDownHeight = 420 };
        for (var id = 0; id < layout.MapCount; id++)
        {
            settings.Items.Add(new BrowserItem(id, $"{id:D3}  {MapDisplayName(id)}"));
        }

        settings.DisplayMemberPath = nameof(BrowserItem.Title);
        settings.SelectedIndex = MapCollectionView.SelectedItem is BrowserItem selected ? selected.Id : 0;
        var blank = new WpfRadioButton { Content = "Blank tileset (black placeholder, 896 empty tiles to draw on)", IsChecked = true, Margin = new Thickness(0, 4, 0, 0) };
        var same = new WpfRadioButton { Content = "Tileset and palettes of the map above", Margin = new Thickness(0, 4, 0, 0) };
        string? tilesetPath = null, palettePath = null;
        var tilesetLabel = new WpfTextBlock { Text = "none", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 0, 0) };
        var paletteLabel = new WpfTextBlock { Text = "none", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 0, 0) };
        var pickTileset = new WpfButton { Content = "Tileset PNG...", Padding = new Thickness(10, 3, 10, 3), MinWidth = 120 };
        pickTileset.Click += (_, _) =>
        {
            tilesetPath = PickOpenFilePath("Tileset picture (128 px wide, 16 tiles per row)", PngFilter);
            tilesetLabel.Text = tilesetPath is null ? "none" : Path.GetFileName(tilesetPath);
        };
        var pickPalette = new WpfButton { Content = "Palette PNG...", Padding = new Thickness(10, 3, 10, 3), MinWidth = 120, Margin = new Thickness(0, 6, 0, 0) };
        pickPalette.Click += (_, _) =>
        {
            palettePath = PickOpenFilePath("Palettes (16 x 16 colour grid)", PngFilter);
            paletteLabel.Text = palettePath is null ? "none" : Path.GetFileName(palettePath);
        };

        var panel = new WpfStackPanel { Margin = new Thickness(16), MaxWidth = 640 };
        WpfTextBlock Text(string text, double top = 12, bool muted = false)
        {
            var block = new WpfTextBlock { Text = text, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, top, 0, 4) };
            if (muted)
            {
                block.SetResourceReference(WpfTextBlock.ForegroundProperty, "Theme.TextMuted");
            }

            return block;
        }

        panel.Children.Add(Text($"The new map will be map {layout.MapCount}. It is added to the project; Export ROM writes it.", 0));
        panel.Children.Add(Text("Size in tiles (8 px; even numbers, up to 64 x 64):"));
        var size = new WpfStackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal };
        size.Children.Add(width);
        size.Children.Add(new WpfTextBlock { Text = "x", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(6, 0, 0, 0) });
        size.Children.Add(height);
        panel.Children.Add(size);
        panel.Children.Add(Text("Music, area and encounters from (and its tileset, if chosen below):"));
        panel.Children.Add(settings);
        panel.Children.Add(Text("Tileset:"));
        panel.Children.Add(blank);
        panel.Children.Add(same);
        panel.Children.Add(Text("Import now (optional; can also be done later in the Layer Editor):"));
        var files = new WpfGrid();
        files.ColumnDefinitions.Add(new() { Width = GridLength.Auto });
        files.ColumnDefinitions.Add(new() { Width = new GridLength(1, GridUnitType.Star) });
        files.RowDefinitions.Add(new());
        files.RowDefinitions.Add(new());
        foreach (var (control, row, column) in new (UIElement, int, int)[] { (pickTileset, 0, 0), (tilesetLabel, 0, 1), (pickPalette, 1, 0), (paletteLabel, 1, 1) })
        {
            WpfGrid.SetRow(control, row);
            WpfGrid.SetColumn(control, column);
            files.Children.Add(control);
        }

        panel.Children.Add(files);
        panel.Children.Add(Text("Without a palette PNG, a new tileset picture brings its own colours (up to 15, in palette row 0). " +
                                "The map starts empty (black) with walkable collision inside a blocked border and no warps: " +
                                "paint it, then add a warp to it from another map and one back.", 12, muted: true));
        var buttons = new WpfStackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal, HorizontalAlignment = System.Windows.HorizontalAlignment.Right, Margin = new Thickness(0, 14, 0, 0) };
        var ok = new WpfButton { Content = "Create map", IsDefault = true, Padding = new Thickness(14, 4, 14, 4) };
        var cancel = new WpfButton { Content = "Cancel", IsCancel = true, Padding = new Thickness(14, 4, 14, 4), Margin = new Thickness(8, 0, 0, 0) };
        buttons.Children.Add(ok);
        buttons.Children.Add(cancel);
        panel.Children.Add(buttons);
        var dialog = new Window
        {
            Title = "New map",
            Content = panel,
            SizeToContent = SizeToContent.WidthAndHeight,
            ResizeMode = ResizeMode.NoResize,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = this,
            ShowInTaskbar = false,
        };
        dialog.SetResourceReference(BackgroundProperty, "Theme.WindowBackground");
        int w = 0, h = 0;
        ok.Click += (_, _) =>
        {
            if (!int.TryParse(width.Text, out w) || !int.TryParse(height.Text, out h) ||
                w is < 2 or > MapExpander.MaxMapSize || h is < 2 or > MapExpander.MaxMapSize || w % 2 != 0 || h % 2 != 0)
            {
                System.Windows.MessageBox.Show(dialog, $"Width and height are even numbers from 2 to {MapExpander.MaxMapSize}.", "New map");
                return;
            }

            dialog.DialogResult = true;
        };
        if (dialog.ShowDialog() != true || settings.SelectedItem is not BrowserItem from)
        {
            return;
        }

        var newId = layout.MapCount;
        _project.MapAdditions.Add(new MapAddition(from.Id, false, Blank: true, Width: w, Height: h, BlankTileset: blank.IsChecked == true));
        RefreshMapsAfterAdditionsChanged(select: newId);
        MapSummaryLabel.Text = $"Map {newId} added: empty {w}x{h}.";
        if (tilesetPath is not null || palettePath is not null)
        {
            await ImportNewMapTilesetAsync(newId, tilesetPath, palettePath);
        }
    }

    // Tileset / palette PNGs for one map (a new one): a tileset without a palette brings its own colours.
    private async Task ImportNewMapTilesetAsync(int mapId, string? tilesetPath, string? palettePath)
    {
        try
        {
            var asset = _mapTilesetRepository.ReadMap(MapSourceRom(), mapId, MapDisplayName(mapId));
            byte[]? palette = null, tiles = null;
            if (palettePath is not null)
            {
                var (pw, ph, pargb) = ReadArgbPng(palettePath);
                palette = MapTilesetCodec.ImportPalette(pw, ph, pargb);
            }

            if (tilesetPath is not null)
            {
                var (tw, th, targb) = ReadArgbPng(tilesetPath);
                palette ??= MapTilesetCodec.PaletteFromPicture(tw, th, targb);
                tiles = MapTilesetCodec.ImportSheet(asset with { PaletteBytes = palette }, tw, th, targb).TilePixels;
            }

            StageTilesetPatch([mapId], tiles, palette);
            MapTilesetSummaryLabel.Text = $"Map {mapId}: {(tiles is null ? "" : "tileset ")}{(palette is null ? "" : "palettes ")}imported.";
        }
        catch (Exception ex) when (ex is InvalidDataException or IOException or NotSupportedException or FileFormatException or UnauthorizedAccessException)
        {
            await DisplayAlertAsync("New map", $"The map was created, but the import failed: {ex.Message}", "OK");
        }
    }
}
