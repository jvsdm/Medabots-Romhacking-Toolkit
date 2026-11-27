using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Medabots.Rom.Editor;
using Medabots.Rom.Images;
using Medabots.Rom.Maps;
using Medabots.Rom.Metadata;

namespace Medabots.Rom.WPFEditor;

/// <summary>
/// Events tab, scene preview: the map at the selected action, with the actors the script created up to there, each where
/// its moves left it. The script is followed in list order from the start of the block that holds the selection (a block
/// ends at End / Goto_Event, so each branch of a Switch_Event_Counter starts fresh), jumps are not taken.
/// The first map is where an NPC or floor trigger runs this event; Warp_A / Warp_B change it.
/// Actor sprites use the same byte as the map NPCs: high nibble = entry of the map's event object list, low 2 bits = facing.
/// </summary>
public partial class MainWindow
{
    private Dictionary<int, (int MapId, int TileX, int TileY, bool FloorTrigger)>? _eventStartIndex;
    private readonly Dictionary<int, MapTilesetAsset> _eventSceneMapCache = new();

    private sealed class SceneActor
    {
        public int Sprite;
        public int X;
        public int Y;
        public int Facing;
        public int Order;
    }

    private void UpdateEventScenePreview()
    {
        if (EventScenePreviewPanel is null)
        {
            return;
        }

        try
        {
            if (!TryBuildEventScene(out var bitmap, out var label))
            {
                EventScenePreviewPanel.Visibility = Visibility.Collapsed;
                return;
            }

            EventScenePreviewImage.Source = bitmap;
            EventScenePreviewLabel.Text = label;
            EventScenePreviewPanel.Visibility = Visibility.Visible;
        }
        catch (Exception ex)
        {
            EventScenePreviewImage.Source = null;
            EventScenePreviewLabel.Text = $"Scene preview failed: {ex.Message}";
            EventScenePreviewPanel.Visibility = Visibility.Visible;
        }
    }

    private bool TryBuildEventScene(out BitmapSource? bitmap, out string label)
    {
        bitmap = null;
        label = string.Empty;
        if (_session is null || _selectedEventId is not short eventId || _selectedEventInstruction is null)
        {
            return false;
        }

        var items = _visibleEventInstructions.ToList();
        var selectedIndex = items.IndexOf(_selectedEventInstruction);
        if (selectedIndex < 0)
        {
            return false;
        }

        // start of the block that holds the selection
        var blockStart = 0;
        for (var i = selectedIndex - 1; i >= 0; i--)
        {
            if (IsSceneBlockEnd(items[i].Name))
            {
                blockStart = i + 1;
                break;
            }
        }

        var start = FindEventStart(eventId);
        int? mapId = start?.MapId;
        (int X, int Y)? player = start is { } s ? (s.TileX, s.TileY) : null;
        var playerFacing = 1;
        var actors = new Dictionary<int, SceneActor>();
        var touched = -1;
        var order = 0;

        for (var i = blockStart; i <= selectedIndex; i++)
        {
            var item = items[i];
            var args = item.Arguments;
            int Arg(int index) => index < args.Count ? args[index].RawValue : 0;
            var isSelected = i == selectedIndex;
            switch (item.Name)
            {
                case "Initiate_Actor":
                case "Initiate_Moving_Actor_A":
                case "Initiate_Moving_Actor_B":
                {
                    var slot = Arg(0) & 0x0F;
                    var actor = new SceneActor { Sprite = Arg(1), X = Arg(2), Y = Arg(3), Facing = Arg(1) & 3, Order = order++ };
                    actors[slot] = actor;
                    if (args.Count > 4)
                    {
                        ApplySceneMove(ref actor.X, ref actor.Y, ref actor.Facing, Arg(4));
                    }

                    if (isSelected)
                    {
                        touched = slot;
                    }

                    break;
                }
                case "Move_Actor_A":
                case "Move_Actor_B":
                    if (actors.TryGetValue(Arg(0), out var moved))
                    {
                        ApplySceneMove(ref moved.X, ref moved.Y, ref moved.Facing, Arg(1));
                        if (isSelected)
                        {
                            touched = Arg(0);
                        }
                    }

                    break;
                case "Rotate_Actor":
                    if (actors.TryGetValue(Arg(0), out var rotated))
                    {
                        rotated.Facing = Arg(1) & 3;
                        if (isSelected)
                        {
                            touched = Arg(0);
                        }
                    }

                    break;
                case "Unload_Actor":
                    actors.Remove(Arg(0) & 0x0F);
                    break;
                case "Warp_A":
                case "Warp_B":
                    mapId = Arg(0);
                    player = (Arg(3), Arg(4));
                    actors.Clear();
                    break;
                case "Move_Player":
                    if (player is { } p)
                    {
                        var (px, py) = p;
                        for (var step = 0; step < 4; step++)
                        {
                            ApplySceneMove(ref px, ref py, ref playerFacing, Arg(step));
                        }

                        player = (px, py);
                    }

                    break;
                case "Player_Face_Direction":
                    playerFacing = Arg(0) & 3;
                    break;
            }
        }

        if (mapId is not int sceneMap)
        {
            label = "Scene preview: no NPC or floor trigger runs this event, so the map is only known after a Warp.";
            bitmap = null;
            return actors.Count > 0;
        }

        var asset = GetEventSceneMap(sceneMap);
        bitmap = RenderEventScene(asset, actors, touched, player);
        var actorText = actors.Count == 0
            ? "no actors"
            : string.Join(", ", actors.OrderBy(a => a.Key).Select(a => $"slot {a.Key} at ({a.Value.X},{a.Value.Y})"));
        label = $"Map {sceneMap:D3} {MapDisplayName(sceneMap)}  |  {actorText}" +
                (player is { } pl ? $"  |  player ({pl.X},{pl.Y})" : string.Empty) +
                "  |  white box = actor of this line, red box = player";
        return true;
    }

    private static bool IsSceneBlockEnd(string name) => name is "End" or "Goto_Event";

    /// <summary>Move byte: high nibble 0 north, 1 south, 2 west, 3 east; low nibble = tiles; 0xFF = unused.</summary>
    private static void ApplySceneMove(ref int x, ref int y, ref int facing, int move)
    {
        if (move == 0xFF || (move & 0xF0) > 0x30)
        {
            return;
        }

        var direction = move >> 4;
        var distance = move & 0x0F;
        facing = direction;
        switch (direction)
        {
            case 0: y -= distance; break;
            case 1: y += distance; break;
            case 2: x -= distance; break;
            case 3: x += distance; break;
        }
    }

    private (int MapId, int TileX, int TileY, bool FloorTrigger)? FindEventStart(int eventId)
    {
        if (_eventStartIndex is null)
        {
            var index = new Dictionary<int, (int, int, int, bool)>();
            var rom = MapSourceRom();
            for (var map = 0; map < rom.Layout.MapCount; map++)
            {
                try
                {
                    if (!_mapOverlayCache.TryGetValue(map, out var overlays))
                    {
                        overlays = _mapOverlayRepository.ReadMap(rom, map);
                        _mapOverlayCache[map] = overlays;
                    }

                    foreach (var spawn in overlays.EntitySpawns)
                    {
                        index.TryAdd(spawn.EventOrObjectId, (map, spawn.TileX, spawn.TileY, spawn.IsWalkOverTrigger));
                    }
                }
                catch
                {
                    // a map that does not read is just left out
                }
            }

            _eventStartIndex = index;
        }

        return _eventStartIndex.TryGetValue(eventId, out var found) ? found : null;
    }

    private MapTilesetAsset GetEventSceneMap(int mapId)
    {
        if (_mapTilesetCache.TryGetValue(mapId, out var cached) || _eventSceneMapCache.TryGetValue(mapId, out cached))
        {
            return cached;
        }

        var asset = _mapTilesetRepository.ReadMap(MapSourceRom(), mapId, MapDisplayName(mapId));
        _eventSceneMapCache[mapId] = asset;
        return asset;
    }

    private BitmapSource RenderEventScene(MapTilesetAsset asset, Dictionary<int, SceneActor> actors, int touchedSlot, (int X, int Y)? player)
    {
        var layers = asset.Layers.OrderByDescending(layer => layer.LayerIndex).ToList();
        var width = Math.Max(1, layers.Count == 0 ? 16 : layers.Max(layer => layer.Image.Width));
        var height = Math.Max(1, layers.Count == 0 ? 16 : layers.Max(layer => layer.Image.Height));
        var pixels = new byte[width * height * 4];
        foreach (var layer in layers)
        {
            BlitMapLayer(layer, width, height, pixels, layer.LayerIndex is 0 or 1, 1.0);
        }

        var compactW = Math.Max(1, asset.WidthInTiles) / 2;
        var compactH = Math.Max(1, asset.HeightInTiles) / 2;
        var offset = new System.Windows.Point(compactW < 15 ? (15 - compactW) * 8 : 0, compactH < 10 ? (10 - compactH) * 8 : 0);

        var resourceIds = _project.MapEventObjectResourcePatches.FirstOrDefault(candidate => candidate.MapId == asset.MapId)?.ResourceIds
            ?? asset.EventObjectResourceIds;
        foreach (var (slot, actor) in actors.OrderBy(a => a.Value.Y).ThenBy(a => a.Value.Order))
        {
            DrawSceneActor(pixels, width, height, resourceIds, actor, offset);
            if (slot == touchedSlot)
            {
                DrawSelectedOverlayHighlight(pixels, width, height, actor.X, actor.Y, offset);
            }
        }

        if (player is { } p)
        {
            DrawSceneOutline(pixels, width, height, p.X, p.Y, Colors.Red, offset);
        }

        var bitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, pixels, width * 4);
        bitmap.Freeze();

        // crop around the actors and the player (4 tiles of margin, at least 16 x 10 tiles) so they show big
        var tiles = actors.Values.Select(a => (a.X, a.Y)).ToList();
        if (player is { } pp)
        {
            tiles.Add(pp);
        }

        if (tiles.Count == 0)
        {
            return bitmap;
        }

        var minX = tiles.Min(t => t.Item1) - 4;
        var maxX = tiles.Max(t => t.Item1) + 4;
        var minY = tiles.Min(t => t.Item2) - 4;
        var maxY = tiles.Max(t => t.Item2) + 4;
        if (maxX - minX + 1 < 16)
        {
            var grow = 16 - (maxX - minX + 1);
            minX -= grow / 2;
            maxX += grow - grow / 2;
        }

        if (maxY - minY + 1 < 10)
        {
            var grow = 10 - (maxY - minY + 1);
            minY -= grow / 2;
            maxY += grow - grow / 2;
        }

        var cropX = Math.Clamp(minX * 16 + (int)offset.X, 0, width - 1);
        var cropY = Math.Clamp(minY * 16 + (int)offset.Y, 0, height - 1);
        var cropW = Math.Clamp((maxX + 1) * 16 + (int)offset.X, cropX + 1, width) - cropX;
        var cropH = Math.Clamp((maxY + 1) * 16 + (int)offset.Y, cropY + 1, height) - cropY;
        var cropped = new CroppedBitmap(bitmap, new Int32Rect(cropX, cropY, cropW, cropH));
        cropped.Freeze();
        return cropped;
    }

    /// <summary>A 2-pixel frame around a tile (does not cover what stands on it).</summary>
    private static void DrawSceneOutline(byte[] pixels, int width, int height, int tileX, int tileY, System.Windows.Media.Color color, System.Windows.Point offset)
    {
        var startX = tileX * 16 + (int)offset.X;
        var startY = tileY * 16 + (int)offset.Y;
        for (var y = 0; y < 16; y++)
        {
            for (var x = 0; x < 16; x++)
            {
                if (x > 1 && x < 14 && y > 1 && y < 14)
                {
                    continue;
                }

                var px = startX + x;
                var py = startY + y;
                if ((uint)px >= (uint)width || (uint)py >= (uint)height)
                {
                    continue;
                }

                var i = (py * width + px) * 4;
                pixels[i] = color.B;
                pixels[i + 1] = color.G;
                pixels[i + 2] = color.R;
                pixels[i + 3] = 255;
            }
        }
    }

    private void DrawSceneActor(byte[] pixels, int width, int height, IReadOnlyList<byte> resourceIds, SceneActor actor, System.Windows.Point offset)
    {
        var resourceIndex = (actor.Sprite >> 4) & 0x0F;
        var sheetId = resourceIndex < resourceIds.Count && resourceIds[resourceIndex] != 0xFF ? resourceIds[resourceIndex] : resourceIndex;
        if (sheetId < 0 || sheetId >= MedabotsRomSchema.SpriteCount)
        {
            DrawOverlayMarker(pixels, width, height, actor.X, actor.Y, Colors.Gold, offset);
            return;
        }

        try
        {
            var asset = GetCurrentOverworldSpriteAsset(sheetId);
            var frame = OverworldSpriteFrameExtractor.ExtractFacingFrame(asset.Image, actor.Facing & 3);
            var swatches = BuildPaletteSwatches(frame.PaletteBytes);
            var destX = (int)(actor.X * 16 + offset.X);
            var destY = (int)(actor.Y * 16 + offset.Y - Math.Max(0, frame.Height - 16));
            BlitIndexedImageOverlay(frame, width, height, destX, destY, swatches, pixels);
        }
        catch
        {
            DrawOverlayMarker(pixels, width, height, actor.X, actor.Y, Colors.Gold, offset);
        }
    }
}
