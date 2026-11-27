using System.ComponentModel;
using Medabots.Rom.Metadata;
using System.Windows;
using System.Windows.Controls;
using MessageBox = System.Windows.MessageBox;
using Medabots.Rom.WPFEditor.Decomp;
using Medabots.Rom.WPFEditor.Dialogs;

namespace Medabots.Rom.WPFEditor;

/// <summary>
/// The Bots group hosts the content of the decomp windows as subtabs: Bot (DecompProjectWindow), Skeleton, Moves and
/// Poses. Each window object is created the first time its subtab is selected; its content moves into the subtab and
/// the window object stays alive (its code behind references the content's named elements). The windows are never shown,
/// so their own Loaded / Closing handlers never run: the main window asks about unsaved changes and follows the project folder.
/// </summary>
public partial class MainWindow
{
    private SkeletonEditorWindow? _skeletonWindow;
    private MoveEditorWindow? _movesWindow;
    private PoseViewerWindow? _posesWindow;
    private readonly HashSet<TabItem> _hostedSections = [];

    // Project folder the Skeleton / Moves / Poses windows were created with (null when none is hosted).
    private string? _hostedRoot;

    // The open project (mode, decomp folder, build, shared rig). Its events drive FollowProjectRoot.
    private readonly Workspace _workspace = new();

    private FieldCatalog FieldNames => _workspace.Fields;

    private void OnBotsTabsSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Selection changes of lists and inner tab controls inside the hosted content bubble up here too.
        if (!ReferenceEquals(e.OriginalSource, BotsTabs) || BotsTabs.SelectedItem is not TabItem section)
        {
            return;
        }

        HostBotsSection(section);
    }

    /// <summary>Selects a Bots subtab by name ("Bot", "Skeleton", "Moves", "Poses"); it is hosted on first selection.</summary>
    private void SelectBotsSubtab(string name)
    {
        var section = name switch
        {
            "Bot" => BotSection,
            "Skeleton" => SkeletonSection,
            "Moves" => MovesSection,
            "Poses" => PosesSection,
            _ => null,
        };
        if (section is not null)
        {
            SelectSection(section);
        }
    }

    private bool IsHostedSection(TabItem section) =>
        ReferenceEquals(section, BotSection) || ReferenceEquals(section, SkeletonSection) ||
        ReferenceEquals(section, MovesSection) || ReferenceEquals(section, PosesSection);

    private void HostBotsSection(TabItem section)
    {
        // Parts is in the same TabControl but is not hosted: its content must not be replaced.
        if (!IsHostedSection(section) || _hostedSections.Contains(section))
        {
            return;
        }

        SyncWorkspace();

        if (ReferenceEquals(section, BotSection))
        {
            if (_decompProjectWindow is null)
            {
                _decompProjectWindow = new DecompProjectWindow(
                    DecompBotStore.FindProjectAbove(_session?.RomFile.FilePath), OpenRomPathAsync)
                {
                    OpenSubtab = SelectBotsSubtab,
                };
                _decompProjectWindow.ProjectRootChanged += _ => SyncWorkspace();
                _decompProjectWindow.LogSink = DecompLog; // hosted: its lines go to the project bar's log
            }

            MountWindow(section, _decompProjectWindow, _decompProjectWindow.InitHosted);
            return;
        }

        var root = ResolveDecompRoot();
        if (root is null)
        {
            // Not hosted: the next selection tries again (a ROM or project may have been opened meanwhile).
            var message = new TextBlock
            {
                Margin = new Thickness(12),
                TextWrapping = TextWrapping.Wrap,
                Text = "Open a decomp project (File > Decomp Project...) or a ROM built from one.",
            };
            message.SetResourceReference(TextBlock.ForegroundProperty, "Theme.TextMuted");
            section.Content = message;
            return;
        }

        _hostedRoot ??= root;
        if (ReferenceEquals(section, SkeletonSection))
        {
            var skeleton = _skeletonWindow ??= new SkeletonEditorWindow(root);
            MountWindow(section, skeleton, skeleton.InitHosted);
        }
        else if (ReferenceEquals(section, MovesSection))
        {
            var moves = _movesWindow ??= new MoveEditorWindow(root, workspace: _workspace) { OpenSubtab = SelectBotsSubtab };
            MountWindow(section, moves, moves.InitHosted);
        }
        else if (ReferenceEquals(section, PosesSection))
        {
            var poses = _posesWindow ??= new PoseViewerWindow(root, _workspace);
            MountWindow(section, poses, poses.InitHosted);
        }
    }

    /// <summary>Puts the open project (mode, folder, build) into the workspace; its events then drive FollowProjectRoot.</summary>
    private void SyncWorkspace() =>
        _workspace.Update(IsDecompMode() ? WorkspaceMode.Decomp : WorkspaceMode.Rom, ResolveDecompRoot());

    /// <summary>Moves a window's content into its subtab, copies its resources there and runs its init.</summary>
    private void MountWindow(TabItem section, Window window, Action<UIElement> initHosted)
    {
        var content = (UIElement)window.Content;
        window.Content = null;
        foreach (var key in window.Resources.Keys)
        {
            section.Resources[key] = window.Resources[key];
        }

        section.Content = content;
        _hostedSections.Add(section);
        initHosted(content);
    }

    /// <summary>
    /// Hosted tabs with unsaved changes, by name. <paramref name="includeBot"/> false leaves the Bot tab out
    /// (its folder change is handled by the Bot tab itself).
    /// </summary>
    private List<string> DirtyHostedTabs(bool includeBot = true)
    {
        var tabs = new List<string>();
        if (includeBot && _decompProjectWindow?.HasUnsavedChanges == true)
        {
            tabs.Add("Bot");
        }

        if (_skeletonWindow?.HasUnsavedChanges == true)
        {
            tabs.Add("Skeleton");
        }

        if (_movesWindow?.HasUnsavedChanges == true)
        {
            tabs.Add("Moves");
        }

        if (_posesWindow?.HasUnsavedChanges == true)
        {
            tabs.Add("Poses");
        }

        return tabs;
    }

    /// <summary>Saves the hosted tabs that have changes (same as their Save buttons); returns the tabs that failed.</summary>
    private List<string> SaveDirtyHostedTabs(bool includeBot = true)
    {
        var failed = new List<string>();
        if (includeBot && _decompProjectWindow?.HasUnsavedChanges == true && !_decompProjectWindow.SaveAllChanges())
        {
            failed.Add("Bot");
        }

        if (_skeletonWindow?.HasUnsavedChanges == true && !_skeletonWindow.SaveAllChanges())
        {
            failed.Add("Skeleton");
        }

        if (_movesWindow?.HasUnsavedChanges == true && !_movesWindow.SaveAllChanges())
        {
            failed.Add("Moves");
        }

        if (_posesWindow?.HasUnsavedChanges == true && !_posesWindow.SaveAllChanges())
        {
            failed.Add("Poses");
        }

        return failed;
    }

    /// <summary>
    /// The main window's Closing: the hosted tabs never run their own Closing, so this asks once about all of them.
    /// Yes saves them and closes (if one fails, the window stays open), No closes without saving, Cancel stays open.
    /// </summary>
    private void OnMainWindowClosing(object? sender, CancelEventArgs e)
    {
        var dirty = DirtyHostedTabs();
        if (dirty.Count == 0)
        {
            return;
        }

        var answer = MessageBox.Show(this,
            $"Unsaved changes in: {string.Join(", ", dirty)}.\n\n" +
            "Yes = save them and close, No = close without saving, Cancel = keep the window open.",
            Title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
        if (answer == MessageBoxResult.Cancel)
        {
            e.Cancel = true;
            return;
        }

        if (answer == MessageBoxResult.No)
        {
            return;
        }

        var failed = SaveDirtyHostedTabs();
        if (failed.Count > 0)
        {
            e.Cancel = true;
            MessageBox.Show(this, $"Not saved: {string.Join(", ", failed)}. The window stays open.", Title,
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    /// <summary>
    /// The project folder that applies changed, or a new build was made. The Bot tab then shows that folder (or the new
    /// build's bot ids), and Skeleton / Moves / Poses, which keep the folder they were created with, are created again.
    /// Their unsaved edits (and the Bot tab's, when it follows) are asked about once: Yes saves them to the old state
    /// first, No discards them, Cancel keeps everything as it was.
    /// </summary>
    private void FollowProjectRoot() => ReloadForProject(buildChanged: false);

    private void OnProjectBuildChanged() => ReloadForProject(buildChanged: true);

    private void ReloadForProject(bool buildChanged)
    {
        var root = ResolveDecompRoot();
        var botFollows = _decompProjectWindow is not null && root is not null &&
            !string.Equals(_decompProjectWindow.ProjectFolder, root, StringComparison.OrdinalIgnoreCase);
        var editorsFollow = _hostedRoot is not null && _hostedRoot != root;
        if (!botFollows && !editorsFollow && !buildChanged)
        {
            UpdateDecompBar();
            return;
        }

        var reloadBot = _decompProjectWindow is not null && root is not null && (botFollows || buildChanged);
        var dirty = DirtyHostedTabs(includeBot: reloadBot);
        if (dirty.Count > 0)
        {
            var answer = MessageBox.Show(this,
                $"{(buildChanged ? "A new build was made." : "The project folder changed.")} " +
                $"Unsaved changes in: {string.Join(", ", dirty)}.\n\n" +
                "Yes = save them first (to the folder they came from), No = discard them, Cancel = keep the tabs as they are.",
                Title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (answer == MessageBoxResult.Cancel)
            {
                return;
            }

            if (answer == MessageBoxResult.Yes)
            {
                var failed = SaveDirtyHostedTabs(includeBot: reloadBot);
                if (failed.Count > 0)
                {
                    MessageBox.Show(this, $"Not saved: {string.Join(", ", failed)}. Those tabs keep their state.", Title,
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
        }

        if (reloadBot)
        {
            _decompProjectWindow!.ShowProject(root!);
        }

        DropHostedEditors();
        if (BotsTabs.SelectedItem is TabItem selected && IsHostedSection(selected) && !ReferenceEquals(selected, BotSection))
        {
            HostBotsSection(selected);
        }

        UpdateDecompBar();
    }

    /// <summary>Removes the Skeleton / Moves / Poses windows and their subtab content; the next selection creates them.</summary>
    private void DropHostedEditors()
    {
        _movesWindow?.StopPlayback();
        _posesWindow?.StopPlayback();
        foreach (var section in new[] { SkeletonSection, MovesSection, PosesSection })
        {
            section.Content = null;
            _hostedSections.Remove(section);
        }

        _skeletonWindow = null;
        _movesWindow = null;
        _posesWindow = null;
        _hostedRoot = null;
    }

    /// <summary>
    /// Decomp mode: the open ROM was built inside a decomp project (with no ROM open, a saved project counts).
    /// A saved project is not used for an open retail ROM, so that ROM keeps the Parts editor.
    /// </summary>
    private bool IsDecompMode() => _session is null
        ? ResolveDecompRoot() is not null
        : DecompBotStore.FindProjectAbove(_session.RomFile.FilePath) is not null;

    /// <summary>
    /// Decomp ROM: Bot, Skeleton, Moves and Poses; Parts is hidden. Retail ROM: Parts only. Ends with a visible tab
    /// selected: a hidden or missing selection moves to the mode's first tab. Tabs are selected directly (not with
    /// SelectSection), so the current group stays as it is.
    /// </summary>
    private void UpdateBotsTabsForMode()
    {
        TabItem[] decompSections = [BotSection, SkeletonSection, MovesSection, PosesSection];
        var decomp = IsDecompMode();
        PartsSection.Visibility = decomp ? Visibility.Collapsed : Visibility.Visible;
        foreach (var section in decompSections)
        {
            section.Visibility = decomp ? Visibility.Visible : Visibility.Collapsed;
        }

        if (BotsTabs.SelectedItem is not TabItem { Visibility: Visibility.Visible })
        {
            (decomp ? BotSection : PartsSection).IsSelected = true;
        }

        UpdateDecompBar();
    }

    /// <summary>The decomp project for the Skeleton / Moves / Poses subtabs: the one above the open ROM, else the saved one.</summary>
    private string? ResolveDecompRoot()
    {
        var above = DecompBotStore.FindProjectAbove(_session?.RomFile.FilePath);
        if (above is not null)
        {
            return above;
        }

        var saved = DecompProjectWindow.SavedProjectRoot();
        return DecompBotStore.LooksLikeProject(saved) ? saved : null;
    }
}
