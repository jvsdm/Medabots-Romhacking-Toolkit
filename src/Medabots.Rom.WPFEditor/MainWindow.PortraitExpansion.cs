using System.Windows;
using Medabots.Rom.Images;
using Medabots.Rom.Metadata;
using Medabots.Rom.WPFEditor.Models;

namespace Medabots.Rom.WPFEditor;

/// <summary>
/// The green "+" of the Sprites tab: new portrait characters (a copy of the selected one, or blank), staged in
/// the project like new maps (they show up in the tree at once, can be edited and named, and Export ROM writes
/// them; the portrait tables move to free space the first time, so up to 256 characters fit), and editor names.
/// </summary>
public partial class MainWindow
{
    /// <summary>Portrait characters in the ROM plus the staged new ones.</summary>
    private int PortraitCharacterCount() => _session is null ? MedabotsRomSchema.PortraitCharacterCount : MapAdditionsRom().Layout.PortraitCount;

    // The character of the selected tree node: an expression, or the character node itself.
    private int? SelectedPortraitCharacter() => SpriteTreeView.SelectedItem switch
    {
        SpriteBrowserNode { AssetKind: SpriteAssetKind.Portrait } node => node.PrimaryId,
        SpriteBrowserNode node when node.Children.FirstOrDefault()?.AssetKind == SpriteAssetKind.Portrait => node.PrimaryId,
        _ => null,
    };

    private string PortraitCharacterLabel(int characterId) =>
        _project.PortraitNames.TryGetValue(characterId, out var name) ? $"{characterId:D3} {name}" : $"{characterId:D3}";

    private async void OnAddSpriteAssetClicked(object? sender, RoutedEventArgs e)
    {
        if (_session is null)
        {
            await DisplayAlertAsync("Sprites", "Open a ROM first.", "OK");
            return;
        }

        var selected = SelectedPortraitCharacter();
        var firstNew = _session.RomFile.Layout.PortraitCount;
        var lastNew = firstNew + _project.PortraitAdditions.Count - 1;
        var menu = new System.Windows.Controls.ContextMenu
        {
            PlacementTarget = sender as UIElement,
            Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom,
            FontSize = 14,                      // not the + button's big bold glyph font
            FontWeight = FontWeights.Normal,
        };
        System.Windows.Controls.MenuItem Item(string header, string tip, bool enabled, RoutedEventHandler click)
        {
            var item = new System.Windows.Controls.MenuItem { Header = header, ToolTip = tip, IsEnabled = enabled };
            item.Click += click;
            menu.Items.Add(item);
            return item;
        }

        Item(selected is { } source ? $"New portrait character (copy of {PortraitCharacterLabel(source)})" : "New portrait character (select one to copy)",
            "All nine expressions and the palette of the selected character, as its own copy to redraw",
            selected is not null, (_, _) => AddPortraitCharacter(selected!.Value, blank: false));
        Item("New blank portrait character", "Nine empty 64x64 portraits and a black palette: import or draw them",
            true, (_, _) => AddPortraitCharacter(0, blank: true));
        menu.Items.Add(new System.Windows.Controls.Separator());
        Item(selected is { } named ? $"Rename character {PortraitCharacterLabel(named)}..." : "Rename character (select one)...",
            "Editor label only, saved in the project", selected is not null, async (_, _) => await RenamePortraitCharacterAsync(selected!.Value));
        Item(_project.PortraitAdditions.Count > 0 ? $"Remove the last new character ({lastNew:D3})" : "Remove the last new character",
            "Takes it and its edits out of the project (earlier ones would renumber the rest)",
            _project.PortraitAdditions.Count > 0, (_, _) => RemoveLastPortraitCharacter(lastNew));
        menu.IsOpen = true;
    }

    private async void AddPortraitCharacter(int source, bool blank)
    {
        if (PortraitCharacterCount() >= PortraitExpander.Capacity)
        {
            await DisplayAlertAsync("New portrait character", $"There are already {PortraitExpander.Capacity} characters, the most <PORTRAIT> can address.", "OK");
            return;
        }

        var id = PortraitCharacterCount();
        _project.PortraitAdditions.Add(new PortraitAddition(source, blank));
        if (!blank && _project.PortraitNames.TryGetValue(source, out var name))
        {
            _project.PortraitNames[id] = $"Copy of {name}";
        }

        RefreshSpritesAfterPortraitAdditionsChanged(select: id);
        SpritePatchStatusLabel.Text = $"Status: portrait character {id:D3} added ({(blank ? "blank" : $"copy of {source:D3}")}). Use <PORTRAIT:side, {id}, expression> in messages; Export ROM writes it.";
    }

    private async Task RenamePortraitCharacterAsync(int characterId)
    {
        var current = _project.PortraitNames.TryGetValue(characterId, out var name) ? name : "";
        var given = await DisplayPromptAsync("Rename portrait character", $"Name for portrait character {characterId:D3} (editor label only; empty = none):",
            "Rename", "Cancel", null, current);
        if (given is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(given))
        {
            _project.PortraitNames.Remove(characterId);
        }
        else
        {
            _project.PortraitNames[characterId] = given.Trim();
        }

        RefreshSpritesAfterPortraitAdditionsChanged(select: characterId);
    }

    private void RemoveLastPortraitCharacter(int characterId)
    {
        if (System.Windows.MessageBox.Show(this, $"Remove the new portrait character {characterId:D3} and its edits from the project?", "New portrait character",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
        {
            return;
        }

        _project.PortraitAdditions.RemoveAt(_project.PortraitAdditions.Count - 1);
        _project.PortraitNames.Remove(characterId);
        foreach (var edit in _project.PortraitEdits.Where(edit => edit.CharacterId == characterId).ToList())
        {
            _project.PortraitEdits.Remove(edit);
        }

        foreach (var key in _editedPortraitAssets.Keys.Where(key => key.Item1 == characterId).ToList())
        {
            _editedPortraitAssets.Remove(key);
        }

        RefreshSpritesAfterPortraitAdditionsChanged(select: null);
        SpritePatchStatusLabel.Text = $"Status: portrait character {characterId:D3} removed from the project.";
    }

    private void RefreshSpritesAfterPortraitAdditionsChanged(int? select)
    {
        ResetTilesetPreview();                   // the preview ROM now has (or no longer has) the new characters
        var states = CaptureSpriteNodeExpansionStates(_allSpriteNodes);
        _spritePreviewCache.Clear();
        _allSpriteNodes.Clear();
        _allSpriteNodes.AddRange(BuildSpriteTreeNodes());
        RestoreSpriteNodeExpansionStates(_allSpriteNodes, states);
        RefreshSpriteFilter();
        RestoreSpriteNodeExpansionStates(_visibleSpriteNodes, states);
        UpdateStatus();
        RebuildMessagePreview(keepBox: true);
        if (select is not { } id)
        {
            return;
        }

        var node = FindSpriteNode(_visibleSpriteNodes, n => n.AssetKind == SpriteAssetKind.Portrait && n.PrimaryId == id && n.SecondaryId == 0);
        if (node is not null)
        {
            ExpandSpriteNodePath(_visibleSpriteNodes, node);
            OnSpriteSelectionChanged(SpriteTreeView, new RoutedPropertyChangedEventArgs<object>(node, node));
        }
    }

    // Expands the groups above a node so it shows in the tree.
    private static bool ExpandSpriteNodePath(IEnumerable<SpriteBrowserNode> nodes, SpriteBrowserNode target)
    {
        foreach (var node in nodes)
        {
            if (ReferenceEquals(node, target) || ExpandSpriteNodePath(node.Children, target))
            {
                node.IsExpanded = node.IsExpanded || !ReferenceEquals(node, target);
                return true;
            }
        }

        return false;
    }
}
