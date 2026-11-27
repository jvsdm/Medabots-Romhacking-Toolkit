using System.Windows;
using System.Windows.Input;
using Medabots.Rom.WPFEditor.Models;

namespace Medabots.Rom.WPFEditor;

/// <summary>
/// Paint tools for the map editor (tile layers and collision): pencil (click/drag, stamps the palette
/// selection), line, rectangle, filled rectangle, bucket fill and picker, with undo/redo. With a block of
/// tiles selected, line/rectangle/bucket repeat it as a pattern anchored at the first cell.
/// </summary>
public partial class MainWindow
{
    private enum MapPaintTool
    {
        Pencil,
        Line,
        Rectangle,
        FilledRectangle,
        Bucket,
        Picker,
    }

    private sealed record MapEditSnapshot(int MapId, string LayerKey, ushort[]? Entries, byte[]? Collision);

    private const int MaxUndoSteps = 100;

    private MapPaintTool _mapPaintTool = MapPaintTool.Pencil;
    private (int X, int Y)? _mapToolStart;
    private (int X, int Y)? _mapToolLast;
    private bool _mapToolDragging;
    private string _mapToolLayerKey = "layer1";
    private List<(int X, int Y)> _mapToolPreviewCells = [];
    private readonly LinkedList<MapEditSnapshot> _mapUndo = new();
    private readonly Stack<MapEditSnapshot> _mapRedo = new();

    private static bool IsPaintableMapLayer(string key) => key is "layer1" or "layer2" or "layer3" or "collision";

    private void OnMapToolChecked(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: string name } && Enum.TryParse<MapPaintTool>(name, out var tool))
        {
            _mapPaintTool = tool;
            _mapToolPreviewCells = [];
            DrawMapOverlays();
        }
    }

    private void SelectMapTool(MapPaintTool tool)
    {
        var button = tool switch
        {
            MapPaintTool.Line => MapToolLineButton,
            MapPaintTool.Rectangle => MapToolRectangleButton,
            MapPaintTool.FilledRectangle => MapToolFilledRectangleButton,
            MapPaintTool.Bucket => MapToolBucketButton,
            MapPaintTool.Picker => MapToolPickerButton,
            _ => MapToolPencilButton,
        };
        button.IsChecked = true;
    }

    // ---- mouse ----

    /// <summary>Starts the current tool at a cell; false when the layer is not paintable (events, warps).</summary>
    private bool BeginMapTool(string layerKey, int x, int y)
    {
        if (_loadedMapTileset is null || !IsPaintableMapLayer(layerKey))
        {
            return false;
        }

        _mapToolLayerKey = layerKey;
        _mapToolStart = (x, y);
        _mapToolLast = (x, y);
        switch (_mapPaintTool)
        {
            case MapPaintTool.Picker:
                PickMapCell(layerKey, x, y);
                return true;
            case MapPaintTool.Bucket:
                PushMapUndo(layerKey);
                ApplyMapCells(layerKey, FloodRegion(layerKey, x, y), (x, y));
                return true;
            case MapPaintTool.Pencil:
                PushMapUndo(layerKey);
                ApplyMapCells(layerKey, PencilBlock(x, y), (x, y));
                _mapToolDragging = true;
                return true;
            default:
                PushMapUndo(layerKey);
                _mapToolDragging = true;
                _mapToolPreviewCells = ShapeCells(x, y, x, y);
                DrawMapOverlays();
                return true;
        }
    }

    private void UpdateMapTool(int x, int y)
    {
        if (!_mapToolDragging || _mapToolStart is not { } start || _mapToolLast == (x, y))
        {
            return;
        }

        var last = _mapToolLast ?? start;
        _mapToolLast = (x, y);
        if (_mapPaintTool == MapPaintTool.Pencil)
        {
            // Every block crossed since the last mouse position (no gaps on fast drags).
            var cells = new HashSet<(int, int)>();
            foreach (var (cx, cy) in LineCells(last.X, last.Y, x, y))
            {
                cells.UnionWith(PencilBlock(cx, cy));
            }

            ApplyMapCells(_mapToolLayerKey, cells, start);
            return;
        }

        _mapToolPreviewCells = ShapeCells(start.X, start.Y, x, y);
        DrawMapOverlays();
    }

    private void EndMapTool()
    {
        if (!_mapToolDragging)
        {
            return;
        }

        _mapToolDragging = false;
        if (_mapPaintTool is MapPaintTool.Line or MapPaintTool.Rectangle or MapPaintTool.FilledRectangle && _mapToolStart is { } start)
        {
            var cells = _mapToolPreviewCells;
            _mapToolPreviewCells = [];
            ApplyMapCells(_mapToolLayerKey, cells, start);
        }

        DropUnchangedUndo();
    }

    private void OnMapCompositePreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_mapToolDragging)
        {
            EndMapTool();
            (sender as UIElement)?.ReleaseMouseCapture();
            e.Handled = true;
        }
    }

    // ---- cells ----

    // Pencil stamps the whole palette selection; while dragging, stamps are aligned to the first one.
    private IEnumerable<(int X, int Y)> PencilBlock(int x, int y)
    {
        var (w, h) = PatternSize();
        var (sx, sy) = _mapToolStart ?? (x, y);
        var ox = sx + FloorDiv(x - sx, w) * w;
        var oy = sy + FloorDiv(y - sy, h) * h;
        for (var dy = 0; dy < h; dy++)
        {
            for (var dx = 0; dx < w; dx++)
            {
                yield return (ox + dx, oy + dy);
            }
        }
    }

    private List<(int X, int Y)> ShapeCells(int x0, int y0, int x1, int y1)
    {
        if (_mapPaintTool == MapPaintTool.Line)
        {
            return LineCells(x0, y0, x1, y1).ToList();
        }

        int minX = Math.Min(x0, x1), maxX = Math.Max(x0, x1), minY = Math.Min(y0, y1), maxY = Math.Max(y0, y1);
        var cells = new List<(int, int)>();
        for (var y = minY; y <= maxY; y++)
        {
            for (var x = minX; x <= maxX; x++)
            {
                if (_mapPaintTool == MapPaintTool.FilledRectangle || x == minX || x == maxX || y == minY || y == maxY)
                {
                    cells.Add((x, y));
                }
            }
        }

        return cells;
    }

    private static IEnumerable<(int X, int Y)> LineCells(int x0, int y0, int x1, int y1)
    {
        int dx = Math.Abs(x1 - x0), dy = -Math.Abs(y1 - y0), sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1, error = dx + dy;
        while (true)
        {
            yield return (x0, y0);
            if (x0 == x1 && y0 == y1)
            {
                yield break;
            }

            var e2 = 2 * error;
            if (e2 >= dy)
            {
                error += dy;
                x0 += sx;
            }

            if (e2 <= dx)
            {
                error += dx;
                y0 += sy;
            }
        }
    }

    // Cells connected (4 directions) to (x, y) holding the same tile entry / collision value.
    private List<(int X, int Y)> FloodRegion(string layerKey, int x, int y)
    {
        var (width, height, read) = CellReader(layerKey);
        var region = new List<(int, int)>();
        if (read is null || x < 0 || y < 0 || x >= width || y >= height)
        {
            return region;
        }

        var target = read(x, y);
        var seen = new bool[width * height];
        var queue = new Queue<(int X, int Y)>();
        queue.Enqueue((x, y));
        seen[y * width + x] = true;
        while (queue.Count > 0)
        {
            var (cx, cy) = queue.Dequeue();
            region.Add((cx, cy));
            foreach (var (nx, ny) in new[] { (cx + 1, cy), (cx - 1, cy), (cx, cy + 1), (cx, cy - 1) })
            {
                if (nx >= 0 && ny >= 0 && nx < width && ny < height && !seen[ny * width + nx] && read(nx, ny) == target)
                {
                    seen[ny * width + nx] = true;
                    queue.Enqueue((nx, ny));
                }
            }
        }

        return region;
    }

    // Width, height and a cell reader for the edited layer (tile entries) or the collision grid.
    private (int Width, int Height, Func<int, int, int>? Read) CellReader(string layerKey)
    {
        if (_loadedMapTileset is null)
        {
            return (0, 0, null);
        }

        if (layerKey == "collision")
        {
            var (cw, ch) = GetCollisionGridDimensions(_loadedMapTileset);
            var bytes = GetEffectiveMapCollisionBytes();
            return (cw, ch, (x, y) => bytes[y * cw + x]);
        }

        var index = LayerIndex(layerKey);
        if (index < 0 || index >= _loadedMapTileset.Layers.Count)
        {
            return (0, 0, null);
        }

        var layer = _loadedMapTileset.Layers[index];
        var (editWidth, editHeight) = GetEffectiveMapSizeInTiles(_loadedMapTileset);
        var (layerWidth, layerHeight) = GetEffectiveMapLayerSize(layer);
        var entries = GetEffectiveMapLayerEntries(layer);
        return (Math.Min(editWidth, layerWidth), Math.Min(editHeight, layerHeight), (x, y) => entries[y * layerWidth + x]);
    }

    private static int LayerIndex(string layerKey) => layerKey switch { "layer1" => 0, "layer2" => 1, "layer3" => 2, _ => -1 };

    private (int Width, int Height) PatternSize()
    {
        if (_mapToolLayerKey == "collision" || !_selectedMapTilesetTileIndex.HasValue)
        {
            return (1, 1);
        }

        var selection = GetSelectedMapTilesetTileSelection(GetSelectedMapTilesetAsset());
        return (Math.Max(1, selection.WidthInTiles), Math.Max(1, selection.HeightInTiles));
    }

    private static int FloorDiv(int a, int b) => (int)Math.Floor(a / (double)b);

    private static int Mod(int a, int b) => ((a % b) + b) % b;

    // ---- writing ----

    /// <summary>Writes the palette selection (as a pattern anchored at <paramref name="anchor"/>) or the collision value to the cells.</summary>
    private void ApplyMapCells(string layerKey, IEnumerable<(int X, int Y)> cells, (int X, int Y) anchor)
    {
        if (_loadedMapTileset is null)
        {
            return;
        }

        if (layerKey == "collision")
        {
            var (cw, ch) = GetCollisionGridDimensions(_loadedMapTileset);
            var bytes = GetEffectiveMapCollisionBytes();
            var changed = false;
            foreach (var (x, y) in cells)
            {
                if (x >= 0 && y >= 0 && x < cw && y < ch && bytes[y * cw + x] != _selectedMapCollisionValue)
                {
                    bytes[y * cw + x] = _selectedMapCollisionValue;
                    changed = true;
                }
            }

            if (changed)
            {
                ReplaceCurrentMapCollisionBytes(bytes);
            }
        }
        else
        {
            var index = LayerIndex(layerKey);
            if (index < 0 || index >= _loadedMapTileset.Layers.Count || !_selectedMapTilesetTileIndex.HasValue)
            {
                return;
            }

            var layer = _loadedMapTileset.Layers[index];
            var (editWidth, editHeight) = GetEffectiveMapSizeInTiles(_loadedMapTileset);
            var (layerWidth, layerHeight) = GetEffectiveMapLayerSize(layer);
            var entries = GetEffectiveMapLayerEntries(layer);
            var tileset = GetSelectedMapTilesetAsset();
            var selection = GetSelectedMapTilesetTileSelection(tileset);
            int w = Math.Max(1, selection.WidthInTiles), h = Math.Max(1, selection.HeightInTiles);
            var changed = false;
            foreach (var (x, y) in cells)
            {
                if (x < 0 || y < 0 || x >= Math.Min(editWidth, layerWidth) || y >= Math.Min(editHeight, layerHeight))
                {
                    continue;
                }

                var tile = selection.StartTileIndex + Mod(x - anchor.X, w) + Mod(y - anchor.Y, h) * tileset.TilesetSheet.TileWidth;
                var bank = tile < tileset.TilePaletteBanks.Count && tileset.TilePaletteBanks[tile] >= 0
                    ? tileset.TilePaletteBanks[tile]
                    : _selectedMapTilesetTilePaletteBank;
                var entry = (ushort)((bank << 12) | (tile & 0x03FF));
                if (entries[y * layerWidth + x] != entry)
                {
                    entries[y * layerWidth + x] = entry;
                    changed = true;
                }
            }

            if (changed)
            {
                _mapLayerProjectEditor.StageLayer(_project, _loadedMapTileset.MapId, layer, entries);
            }
        }

        UpdateMapOverlayStatus();
        RefreshMapCompositePreview();
        UpdateMapEditorSidebar();
    }

    private void PickMapCell(string layerKey, int x, int y)
    {
        var (width, height, read) = CellReader(layerKey);
        if (read is null || x < 0 || y < 0 || x >= width || y >= height || _loadedMapTileset is null)
        {
            return;
        }

        var value = read(x, y);
        if (layerKey == "collision")
        {
            _selectedMapCollisionValue = (byte)value;
            MapCollisionValueComboBox.SelectedValue = (byte)value;
        }
        else
        {
            // The entry indexes this map's own tileset: make it the paint source again.
            _selectedMapTilesetSourceMapIds.Remove(_loadedMapTileset.MapId);
            var own = _mapTilesetOptions.FirstOrDefault(option => option.GraphicsDataOffset == _loadedMapTileset.GraphicsDataOffset);
            if (own is not null && !Equals(MapTilesetComboBox.SelectedItem, own))
            {
                MapTilesetComboBox.SelectedItem = own;
            }

            _selectedMapTilesetTileIndex = value & 0x03FF;
            _selectedMapTilesetTileEndIndex = value & 0x03FF;
            _selectedMapTilesetTilePaletteBank = (value >> 12) & 0xF;
            RefreshMapTilesetPalettePreview();
        }

        UpdateMapEditorSidebar();
        SelectMapTool(MapPaintTool.Pencil);
    }

    // ---- undo / redo ----

    private MapEditSnapshot? CaptureMapState(string layerKey)
    {
        if (_loadedMapTileset is null)
        {
            return null;
        }

        if (layerKey == "collision")
        {
            return new MapEditSnapshot(_loadedMapTileset.MapId, layerKey, null, GetEffectiveMapCollisionBytes());
        }

        var index = LayerIndex(layerKey);
        return index < 0 || index >= _loadedMapTileset.Layers.Count
            ? null
            : new MapEditSnapshot(_loadedMapTileset.MapId, layerKey, GetEffectiveMapLayerEntries(_loadedMapTileset.Layers[index]), null);
    }

    private void PushMapUndo(string layerKey)
    {
        if (CaptureMapState(layerKey) is not { } snapshot)
        {
            return;
        }

        _mapUndo.AddLast(snapshot);
        if (_mapUndo.Count > MaxUndoSteps)
        {
            _mapUndo.RemoveFirst();
        }

        _mapRedo.Clear();
    }

    // An operation that changed nothing (click on an identical tile) leaves no undo step.
    private void DropUnchangedUndo()
    {
        if (_mapUndo.Last is { } last && CaptureMapState(last.Value.LayerKey) is { } now && SameState(last.Value, now))
        {
            _mapUndo.RemoveLast();
        }
    }

    private static bool SameState(MapEditSnapshot a, MapEditSnapshot b) =>
        a.MapId == b.MapId && a.LayerKey == b.LayerKey &&
        (a.Entries is null ? b.Entries is null : b.Entries is not null && a.Entries.AsSpan().SequenceEqual(b.Entries)) &&
        (a.Collision is null ? b.Collision is null : b.Collision is not null && a.Collision.AsSpan().SequenceEqual(b.Collision));

    private void UndoMapEdit() => StepMapHistory(undo: true);

    private void RedoMapEdit() => StepMapHistory(undo: false);

    private void StepMapHistory(bool undo)
    {
        if (_loadedMapTileset is null)
        {
            return;
        }

        MapEditSnapshot? target = null;
        if (undo && _mapUndo.Last is { } last && last.Value.MapId == _loadedMapTileset.MapId)
        {
            target = last.Value;
            _mapUndo.RemoveLast();
        }
        else if (!undo && _mapRedo.Count > 0 && _mapRedo.Peek().MapId == _loadedMapTileset.MapId)
        {
            target = _mapRedo.Pop();
        }

        if (target is null || CaptureMapState(target.LayerKey) is not { } current)
        {
            return;
        }

        if (undo)
        {
            _mapRedo.Push(current);
        }
        else
        {
            _mapUndo.AddLast(current);
        }

        if (target.Collision is { } bytes)
        {
            ReplaceCurrentMapCollisionBytes(bytes);
        }
        else if (target.Entries is { } entries)
        {
            _mapLayerProjectEditor.StageLayer(_project, target.MapId, _loadedMapTileset.Layers[LayerIndex(target.LayerKey)], entries);
        }

        UpdateMapOverlayStatus();
        RefreshMapCompositePreview();
        UpdateMapEditorSidebar();
    }

    private void OnMapUndoClicked(object sender, RoutedEventArgs e) => UndoMapEdit();

    private void OnMapRedoClicked(object sender, RoutedEventArgs e) => RedoMapEdit();

    // Ctrl+Z / Ctrl+Y (Ctrl+Shift+Z) and tool keys while the Maps tab is in front, not while typing.
    private void OnMapsPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.OriginalSource is System.Windows.Controls.TextBox or System.Windows.Controls.ComboBox)
        {
            return;
        }

        var ctrl = (Keyboard.Modifiers & ModifierKeys.Control) != 0;
        var shift = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
        if (ctrl && (e.Key == Key.Y || (e.Key == Key.Z && shift)))
        {
            RedoMapEdit();
            e.Handled = true;
        }
        else if (ctrl && e.Key == Key.Z)
        {
            UndoMapEdit();
            e.Handled = true;
        }
        else if (!ctrl && (Keyboard.Modifiers & ModifierKeys.Alt) == 0)
        {
            MapPaintTool? tool = e.Key switch
            {
                Key.P or Key.B => MapPaintTool.Pencil,
                Key.L => MapPaintTool.Line,
                Key.R => MapPaintTool.Rectangle,
                Key.F => MapPaintTool.FilledRectangle,
                Key.G => MapPaintTool.Bucket,
                Key.I => MapPaintTool.Picker,
                _ => null,
            };
            if (tool is { } chosen)
            {
                SelectMapTool(chosen);
                e.Handled = true;
            }
        }
    }

    private void ClearMapHistory()
    {
        _mapUndo.Clear();
        _mapRedo.Clear();
        _mapToolPreviewCells = [];
        _mapToolDragging = false;
    }

    // ---- preview of line / rectangle cells ----

    private void DrawMapToolPreview(byte[] pixels, int bitmapWidth, int bitmapHeight)
    {
        if (_mapToolPreviewCells.Count == 0 || _loadedMapTileset is null)
        {
            return;
        }

        var collision = _mapToolLayerKey == "collision";
        var size = collision ? 16 : 8;
        var offset = collision ? GetMapOverlayPixelOffset() : default;
        foreach (var (x, y) in _mapToolPreviewCells)
        {
            DrawSelectionRectangle(pixels, bitmapWidth, bitmapHeight, (int)offset.X + x * size + 1, (int)offset.Y + y * size + 1, size - 2, size - 2);
        }
    }
}
