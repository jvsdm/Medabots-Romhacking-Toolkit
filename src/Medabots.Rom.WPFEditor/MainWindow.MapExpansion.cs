using System.IO;
using System.Windows;
using Medabots.Rom.Editor;
using Medabots.Rom.Maps;
using WpfCheckBox = System.Windows.Controls.CheckBox;
using WpfComboBox = System.Windows.Controls.ComboBox;
using WpfStackPanel = System.Windows.Controls.StackPanel;
using WpfTextBlock = System.Windows.Controls.TextBlock;

namespace Medabots.Rom.WPFEditor;

public partial class MainWindow
{
    // The event script editor's map and Medabot dropdowns follow the open ROM (and the staged new maps).
    private void RegisterEventArgumentNames()
    {
        EventArgumentEditorItem.MapNames = () => _session is null
            ? []
            : Enumerable.Range(0, MapSourceRom().Layout.MapCount).Select(MapDisplayName).ToList();
        EventArgumentEditorItem.BotNames = () =>
        {
            if (_session is null)
            {
                return [];
            }

            var layout = _session.RomFile.Layout;
            var codec = new Medabots.Rom.Text.MedabotsTextCodec();
            return Enumerable.Range(0, layout.BotCount)
                .Select(id => layout.ReadBotName(_session.RomFile.Data, id, codec) ?? _metadata.GetBotName(id))
                .ToList();
        };
    }

    /// <summary>
    /// The project's name for a map if it has one, else the catalog name; new maps are named after their
    /// source ("Copy of ...").
    /// </summary>
    private string MapDisplayName(int mapId)
    {
        if (_project.MapNames.TryGetValue(mapId, out var name) && !string.IsNullOrWhiteSpace(name))
        {
            return name;
        }

        if (mapId < _metadata.Catalog.Maps.Count)
        {
            return _metadata.GetMapName(mapId);
        }

        var first = _session?.RomFile.Layout.MapCount ?? int.MaxValue;
        var index = mapId - first;
        return index >= 0 && index < _project.MapAdditions.Count
            ? $"Copy of {MapDisplayName(_project.MapAdditions[index].SourceMapId)}"
            : $"New map {mapId}";
    }

    private async void OnMapListDoubleClicked(object? sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (MapCollectionView.SelectedItem is not BrowserItem item)
        {
            return;
        }

        var name = await DisplayPromptAsync("Rename map", $"Name for map {item.Id} (editor label only; empty = default name):",
            "Rename", "Cancel", null, MapDisplayName(item.Id));
        if (name is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            _project.MapNames.Remove(item.Id);
        }
        else
        {
            _project.MapNames[item.Id] = name.Trim();
        }

        item.Title = $"{item.Id:D3}  {MapDisplayName(item.Id)}";
        if (_loadedMapTileset?.MapId == item.Id)
        {
            MapSummaryLabel.Text = $"Map {item.Id:D3}  {MapDisplayName(item.Id)}";
        }

        UpdateStatus();
    }

    /// <summary>
    /// Stages a new map (a copy of an existing one, in the next free map ID) in the project, like the other
    /// edits: it shows up in the list at once, can be edited, and Export ROM writes it (the per-map tables
    /// move to free space the first time, so up to 255 maps fit).
    /// </summary>
    private async void OnCopyMapClicked(object? sender, RoutedEventArgs e)
    {
        if (_session is null)
        {
            await DisplayAlertAsync("New map", "Open a ROM first.", "OK");
            return;
        }

        var layout = MapSourceRom().Layout;
        var source = new WpfComboBox { MinWidth = 420, MaxDropDownHeight = 420, Margin = new Thickness(0, 6, 0, 0) };
        for (var id = 0; id < layout.MapCount; id++)
        {
            source.Items.Add(new BrowserItem(id, $"{id:D3}  {MapDisplayName(id)}"));
        }

        source.DisplayMemberPath = nameof(BrowserItem.Title);
        source.SelectedIndex = MapCollectionView.SelectedItem is BrowserItem selected ? selected.Id : 0;
        var copyOverlays = new WpfCheckBox
        {
            Content = "Also copy its warps and people (NPCs, event triggers)",
            Margin = new Thickness(0, 10, 0, 0),
        };
        var panel = new WpfStackPanel { Margin = new Thickness(16) };
        panel.Children.Add(new WpfTextBlock
        {
            Text = $"The new map will be map {layout.MapCount} (up to {MapExpander.Capacity - 1}); it is added to the project, " +
                   "so you can add several and edit them before exporting. It starts as a copy of:",
            TextWrapping = TextWrapping.Wrap,
        });
        panel.Children.Add(source);
        panel.Children.Add(copyOverlays);
        var hint = new WpfTextBlock
        {
            Text = "Same layers, tileset, palette, collision, music and area. Without warps the map has no exits yet: " +
                   "add warps on it (and a warp to it from another map) in the Maps tab.",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 10, 0, 0),
        };
        hint.SetResourceReference(WpfTextBlock.ForegroundProperty, "Theme.TextMuted");
        panel.Children.Add(hint);
        var buttons = new WpfStackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal, HorizontalAlignment = System.Windows.HorizontalAlignment.Right, Margin = new Thickness(0, 14, 0, 0) };
        var ok = new System.Windows.Controls.Button { Content = "Add map", IsDefault = true, Padding = new Thickness(14, 4, 14, 4) };
        var cancel = new System.Windows.Controls.Button { Content = "Cancel", IsCancel = true, Padding = new Thickness(14, 4, 14, 4), Margin = new Thickness(8, 0, 0, 0) };
        buttons.Children.Add(ok);
        buttons.Children.Add(cancel);
        panel.Children.Add(buttons);
        var dialog = new Window
        {
            Title = "Copy map",
            Content = panel,
            SizeToContent = SizeToContent.WidthAndHeight,
            ResizeMode = ResizeMode.NoResize,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = this,
            ShowInTaskbar = false,
        };
        dialog.SetResourceReference(BackgroundProperty, "Theme.WindowBackground");
        ok.Click += (_, _) => dialog.DialogResult = true;
        if (dialog.ShowDialog() != true || source.SelectedItem is not BrowserItem from)
        {
            return;
        }

        _project.MapAdditions.Add(new MapAddition(from.Id, copyOverlays.IsChecked == true));
        RefreshMapsAfterAdditionsChanged(select: layout.MapCount);
        await Task.CompletedTask;
        MapSummaryLabel.Text = $"Map {layout.MapCount} added to the project (a copy of map {from.Id}). Edit it like any map; Export ROM writes it.";
    }

    private void RefreshMapsAfterAdditionsChanged(int? select)
    {
        ResetTilesetPreview();
        _mapOverlayCache.Clear();
        _allMapItems.Clear();
        _allMapItems.AddRange(Enumerable.Range(0, MapSourceRom().Layout.MapCount).Select(mapId => new BrowserItem(mapId, $"{mapId:D3}  {MapDisplayName(mapId)}")));
        RefreshMapFilter();
        UpdateStatus();
        if (select is { } id)
        {
            MapCollectionView.SelectedItem = _visibleMapItems.FirstOrDefault(item => item.Id == id);
            MapCollectionView.ScrollIntoView(MapCollectionView.SelectedItem);
        }
    }

    /// <summary>Revert on the last staged new map removes it (and its edits); earlier ones would renumber the rest.</summary>
    private async Task<bool> TryRemoveStagedNewMapAsync(int mapId)
    {
        if (_session is null || _project.MapAdditions.Count == 0)
        {
            return false;
        }

        var firstNew = _session.RomFile.Layout.MapCount;
        if (mapId < firstNew)
        {
            return false;
        }

        if (mapId != firstNew + _project.MapAdditions.Count - 1)
        {
            await DisplayAlertAsync("New map", $"Only the last new map ({firstNew + _project.MapAdditions.Count - 1}) can be removed: removing map {mapId} would renumber the ones after it.", "OK");
            return true;
        }

        if (System.Windows.MessageBox.Show(this, $"Remove the new map {mapId} and its edits from the project?", "New map",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
        {
            return true;
        }

        _project.MapAdditions.RemoveAt(_project.MapAdditions.Count - 1);
        RemovePatchForMap(_project.MapEncounterPatches, mapId, patch => patch.MapId);
        RemovePatchForMap(_project.MapEncounterStatePatches, mapId, patch => patch.MapId);
        RemovePatchForMap(_project.MapMusicPatches, mapId, patch => patch.MapId);
        RemovePatchForMap(_project.MapEventObjectResourcePatches, mapId, patch => patch.MapId);
        RemovePatchForMap(_project.MapDimensionPatches, mapId, patch => patch.MapId);
        RemovePatchForMap(_project.MapLayerPatches, mapId, patch => patch.MapId);
        RemovePatchForMap(_project.MapCollisionPatches, mapId, patch => patch.MapId);
        RemovePatchForMap(_project.MapWarpPatches, mapId, patch => patch.MapId);
        RemovePatchForMap(_project.MapEntitySpawnPatches, mapId, patch => patch.MapId);
        RemoveTilesetPatchesForMap(mapId);
        _mapTilesetCache.Remove(mapId);
        _loadedMapTileset = null;
        RefreshMapsAfterAdditionsChanged(select: mapId - 1);
        return true;
    }
}
