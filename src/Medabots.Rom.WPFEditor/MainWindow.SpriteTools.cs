using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Medabots.Rom.WPFEditor.Models;
using WpfPoint = System.Windows.Point;
using Color = System.Windows.Media.Color;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace Medabots.Rom.WPFEditor;

/// <summary>
/// Shape tools for the sprite editor (line, rectangle, filled rectangle, bucket fill), redo, keyboard
/// shortcuts and centring. They work in preview pixel space and paint through the pencil, so every
/// asset kind the pencil edits (overworld sheets, portraits, battle sprites, part pieces) gets them.
/// </summary>
public partial class MainWindow
{
    private const string SpriteToolPreviewTag = "tool-preview";
    private (int X, int Y)? _spriteShapeStart;
    private List<(int X, int Y)> _spriteShapeCells = [];
    private string? _centeredSpriteKey;

    private static bool IsSpriteShapeTool(SpriteEditorTool tool) =>
        tool is SpriteEditorTool.Line or SpriteEditorTool.Rectangle or SpriteEditorTool.FilledRectangle;

    private (int X, int Y) SpritePixelAt(WpfPoint point) => ((int)(point.X / _spriteEditorZoom), (int)(point.Y / _spriteEditorZoom));

    private WpfPoint SpritePointOf(int x, int y) => new((x + 0.5) * _spriteEditorZoom, (y + 0.5) * _spriteEditorZoom);

    /// <summary>Handles the new tools on mouse down; false lets the original pencil/eraser/picker path run.</summary>
    private bool BeginSpriteShapeTool(WpfPoint point)
    {
        if (_selectedSpriteEditorTool == SpriteEditorTool.Bucket)
        {
            var (x, y) = SpritePixelAt(point);
            PaintSpritePixels(SpriteFloodRegion(x, y));
            return true;
        }

        if (!IsSpriteShapeTool(_selectedSpriteEditorTool))
        {
            return false;
        }

        _spriteShapeStart = SpritePixelAt(point);
        _spriteShapeCells = SpriteShapeCells(_spriteShapeStart.Value, _spriteShapeStart.Value);
        DrawSpriteShapePreview();
        return true;
    }

    private bool UpdateSpriteShapeTool(WpfPoint point)
    {
        if (_spriteShapeStart is not { } start || !IsSpriteShapeTool(_selectedSpriteEditorTool))
        {
            return false;
        }

        var cells = SpriteShapeCells(start, SpritePixelAt(point));
        if (cells.Count != _spriteShapeCells.Count || !cells.SequenceEqual(_spriteShapeCells))
        {
            _spriteShapeCells = cells;
            DrawSpriteShapePreview();
        }

        return true;
    }

    private bool EndSpriteShapeTool()
    {
        if (_spriteShapeStart is null)
        {
            return false;
        }

        var cells = _spriteShapeCells;
        _spriteShapeStart = null;
        _spriteShapeCells = [];
        ClearSpriteShapePreview();
        PaintSpritePixels(cells);
        return true;
    }

    // One undo step, one preview refresh: the pencil writes each pixel.
    private void PaintSpritePixels(IEnumerable<(int X, int Y)> pixels)
    {
        var tool = _selectedSpriteEditorTool;
        _selectedSpriteEditorTool = SpriteEditorTool.Pencil;
        _hasCapturedUndoForCurrentStroke = false;
        try
        {
            foreach (var (x, y) in pixels)
            {
                ApplySpriteToolAtPoint(SpritePointOf(x, y), invalidate: false);
            }
        }
        finally
        {
            _selectedSpriteEditorTool = tool;
            _hasCapturedUndoForCurrentStroke = false;
        }

        InvalidateSelectedSpritePreview();
    }

    private List<(int X, int Y)> SpriteShapeCells((int X, int Y) a, (int X, int Y) b)
    {
        if (_selectedSpriteEditorTool == SpriteEditorTool.Line)
        {
            return LineCells(a.X, a.Y, b.X, b.Y).ToList();
        }

        int minX = Math.Min(a.X, b.X), maxX = Math.Max(a.X, b.X), minY = Math.Min(a.Y, b.Y), maxY = Math.Max(a.Y, b.Y);
        var cells = new List<(int, int)>();
        for (var y = minY; y <= maxY; y++)
        {
            for (var x = minX; x <= maxX; x++)
            {
                if (_selectedSpriteEditorTool == SpriteEditorTool.FilledRectangle || x == minX || x == maxX || y == minY || y == maxY)
                {
                    cells.Add((x, y));
                }
            }
        }

        return cells;
    }

    // Connected (4 directions) pixels of the same colour in the preview, which shows the current pixels.
    private List<(int X, int Y)> SpriteFloodRegion(int x, int y)
    {
        var region = new List<(int, int)>();
        if (SpritePreviewImage?.Source is not BitmapSource bitmap)
        {
            return region;
        }

        int width = bitmap.PixelWidth, height = bitmap.PixelHeight;
        if (x < 0 || y < 0 || x >= width || y >= height)
        {
            return region;
        }

        var source = bitmap.Format == PixelFormats.Bgra32 ? bitmap : new FormatConvertedBitmap(bitmap, PixelFormats.Bgra32, null, 0);
        var pixels = new int[width * height];
        source.CopyPixels(pixels, width * 4, 0);
        var target = pixels[y * width + x];
        var seen = new bool[pixels.Length];
        var queue = new Queue<(int X, int Y)>();
        queue.Enqueue((x, y));
        seen[y * width + x] = true;
        while (queue.Count > 0)
        {
            var (cx, cy) = queue.Dequeue();
            region.Add((cx, cy));
            foreach (var (nx, ny) in new[] { (cx + 1, cy), (cx - 1, cy), (cx, cy + 1), (cx, cy - 1) })
            {
                if (nx >= 0 && ny >= 0 && nx < width && ny < height && !seen[ny * width + nx] && pixels[ny * width + nx] == target)
                {
                    seen[ny * width + nx] = true;
                    queue.Enqueue((nx, ny));
                }
            }
        }

        return region;
    }

    private void DrawSpriteShapePreview()
    {
        ClearSpriteShapePreview();
        var stroke = new SolidColorBrush(Colors.White);
        var fill = new SolidColorBrush(Color.FromArgb(110, 37, 99, 235));
        foreach (var (x, y) in _spriteShapeCells)
        {
            var cell = new Rectangle
            {
                Width = _spriteEditorZoom,
                Height = _spriteEditorZoom,
                Fill = fill,
                Stroke = _spriteEditorZoom >= 6 ? stroke : null,
                StrokeThickness = 1,
                Tag = SpriteToolPreviewTag,
                IsHitTestVisible = false,
            };
            System.Windows.Controls.Canvas.SetLeft(cell, x * _spriteEditorZoom);
            System.Windows.Controls.Canvas.SetTop(cell, y * _spriteEditorZoom);
            SpriteGridCanvas.Children.Add(cell);
        }
    }

    private void ClearSpriteShapePreview()
    {
        foreach (var element in SpriteGridCanvas.Children.OfType<FrameworkElement>().Where(e => Equals(e.Tag, SpriteToolPreviewTag)).ToList())
        {
            SpriteGridCanvas.Children.Remove(element);
        }
    }

    private void OnSpriteToolLineClicked(object? sender, RoutedEventArgs e) => SetSpriteEditorTool(SpriteEditorTool.Line);

    private void OnSpriteToolRectangleClicked(object? sender, RoutedEventArgs e) => SetSpriteEditorTool(SpriteEditorTool.Rectangle);

    private void OnSpriteToolFilledRectangleClicked(object? sender, RoutedEventArgs e) => SetSpriteEditorTool(SpriteEditorTool.FilledRectangle);

    private void OnSpriteToolBucketClicked(object? sender, RoutedEventArgs e) => SetSpriteEditorTool(SpriteEditorTool.Bucket);

    // Ctrl+Z / Ctrl+Y (Ctrl+Shift+Z) and tool keys while the Sprites tab is in front, not while typing.
    private void OnSpritesPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.OriginalSource is System.Windows.Controls.TextBox or System.Windows.Controls.ComboBox)
        {
            return;
        }

        var ctrl = (Keyboard.Modifiers & ModifierKeys.Control) != 0;
        var shift = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
        if (ctrl && (e.Key == Key.Y || (e.Key == Key.Z && shift)))
        {
            OnRedoSpriteEditClicked(sender, e);
            e.Handled = true;
        }
        else if (ctrl && e.Key == Key.Z)
        {
            OnUndoSpriteEditClicked(sender, e);
            e.Handled = true;
        }
        else if (!ctrl && (Keyboard.Modifiers & ModifierKeys.Alt) == 0)
        {
            SpriteEditorTool? tool = e.Key switch
            {
                Key.P or Key.B => SpriteEditorTool.Pencil,
                Key.E => SpriteEditorTool.Eraser,
                Key.L => SpriteEditorTool.Line,
                Key.R => SpriteEditorTool.Rectangle,
                Key.F => SpriteEditorTool.FilledRectangle,
                Key.G => SpriteEditorTool.Bucket,
                Key.I => SpriteEditorTool.Picker,
                _ => null,
            };
            if (tool is { } chosen)
            {
                SetSpriteEditorTool(chosen);
                e.Handled = true;
            }
        }
    }

    // A newly selected sprite starts centred (the surface is larger than the view, the image in its middle).
    private void CenterSpritePreviewIfNew()
    {
        var key = _selectedSpriteNode is null ? null : GetSelectedSpriteHistoryKey();
        if (key is null || key == _centeredSpriteKey)
        {
            return;
        }

        _centeredSpriteKey = key;
        CenterSpritePreview();
    }

    private void CenterSpritePreview()
    {
        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ContextIdle, () =>
        {
            if (SpritePreviewScrollViewer is not { } viewer)
            {
                return;
            }

            viewer.UpdateLayout();
            viewer.ScrollToHorizontalOffset(viewer.ScrollableWidth / 2);
            viewer.ScrollToVerticalOffset(viewer.ScrollableHeight / 2);
        });
    }

    // ---- redo ----

    /// <summary>The current pixels and palettes of the selected asset, in the shape undo snapshots have.</summary>
    private SpriteEditSnapshot? CaptureSelectedSpriteSnapshot()
    {
        if (_selectedSpriteNode is null)
        {
            return null;
        }

        static SpriteEditSnapshot One(Medabots.Rom.Images.IndexedImage image) =>
            new([new SpriteEditImageSnapshot(image.PixelIndices, image.PaletteBytes)]);

        switch (_selectedSpriteNode.AssetKind)
        {
            case SpriteAssetKind.OverworldEventObject:
                return One(GetEditableOverworldSpriteAsset(_selectedSpriteNode.PrimaryId).Image);
            case SpriteAssetKind.Portrait:
                return One(GetEditablePortraitAsset(_selectedSpriteNode.PrimaryId, _selectedSpriteNode.SecondaryId).Image);
            case SpriteAssetKind.BattleCompositePartComponent:
                return One(GetEditableBattleCompositeComponentAsset(_selectedSpriteNode.PrimaryId, _selectedSpriteNode.SecondaryId).Image);
            case SpriteAssetKind.PartCompositeEditableSprite:
            {
                var selected = GetPreviewEditableLargeDisplaySpriteReference(_selectedSpriteNode);
                var references = GetEditableLargeDisplaySpriteReferences(_selectedSpriteNode.PrimaryId, selected.Piece.ImageOffset);
                return references.Count == 0 ? null : One(references[0].Piece.Image);
            }
            case SpriteAssetKind.PartCompositeDescriptorPiece:
            {
                var asset = GetEditableLargePartDisplayAsset(_selectedSpriteNode.PrimaryId, _selectedSpriteNode.SecondaryId);
                return new SpriteEditSnapshot(asset.Pieces.Select(piece => new SpriteEditImageSnapshot(piece.Image.PixelIndices, piece.Image.PaletteBytes)).ToArray());
            }
            default:
                return null;
        }
    }

    private async void OnRedoSpriteEditClicked(object? sender, RoutedEventArgs e)
    {
        if (_selectedSpriteNode is null)
        {
            await DisplayAlertAsync("No Asset Selected", "Select a sprite or portrait to redo.", "OK");
            return;
        }

        var historyKey = GetSelectedSpriteHistoryKey();
        if (!_spriteEditHistories.TryGetValue(historyKey, out var history) || !history.CanRedo)
        {
            SpritePatchStatusLabel.Text = "Status: nothing to redo for this asset.";
            return;
        }

        if (CaptureSelectedSpriteSnapshot() is { } current)
        {
            history.PushUndoKeepingRedo(current);
        }

        RestoreSelectedSpriteSnapshot(history.PopRedo());
        _hasCapturedUndoForCurrentStroke = false;
        InvalidateSelectedSpritePreview();
        SpritePatchStatusLabel.Text = "Status: redid the last undone edit for this asset.";
    }
}
