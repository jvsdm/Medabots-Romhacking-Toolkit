using System.Windows;
using Medabots.Rom.WPFEditor.Decomp;
using Medabots.Rom.WPFEditor.Dialogs;
using MessageBox = System.Windows.MessageBox;

namespace Medabots.Rom.WPFEditor;

/// <summary>
/// The project bar (Decomp mode only, above the groups): the project folder, Build ROM, Open build and the build log.
/// Building saves the open tabs, runs tools/decomp_build.py (Decomp/DecompBuild.cs, the same command as before) into a new
/// build/scratch folder and opens that ROM; the reload of the tabs raises Workspace.BuildChanged (MainWindow.BotsHost.cs).
/// </summary>
public partial class MainWindow
{
    private bool _decompBuilding;

    /// <summary>One line of the build log (also the Bot tab's lines while it is hosted).</summary>
    private void DecompLog(string line)
    {
        DecompLogBox.AppendText(line + Environment.NewLine);
        DecompLogBox.ScrollToEnd();
    }

    /// <summary>The project bar shows in Decomp mode only; it names the folder that applies now.</summary>
    private void UpdateDecompBar()
    {
        DecompBar.Visibility = IsDecompMode() ? Visibility.Visible : Visibility.Collapsed;
        DecompFolderText.Text = ResolveDecompRoot() ?? "(no project: Change folder...)";
    }

    private async void OnDecompBuildClicked(object sender, RoutedEventArgs e)
    {
        var root = ResolveDecompRoot();
        if (root is null || _decompBuilding)
        {
            return;
        }

        // The same as the tabs' Save buttons: the generator reads the JSON files from the folder.
        var failed = SaveDirtyHostedTabs();
        if (failed.Count > 0)
        {
            DecompLogExpander.IsExpanded = true;
            DecompLog($"save failed, build cancelled: {string.Join(", ", failed)}");
            return;
        }

        await SaveDecompProjectAsync();   // the ROM tabs' edits go to data/wpf/project.medahack.json, applied after the build
        _decompBuilding = true;
        DecompBuildButton.IsEnabled = false;
        DecompLogExpander.IsExpanded = true;
        try
        {
            var (exitCode, built) = await DecompBuild.RunAsync(root, DecompProjectWindow.SavedPython(), DecompLog);
            DecompLog(exitCode == 0 ? "build finished" : $"build FAILED (exit {exitCode})");
            if (built is not null)
            {
                var applied = await ApplyDecompProjectAsync(root, built);
                if (applied > 0)
                {
                    DecompLog($"WPF edits applied to the new build ({applied} change(s), {DecompProjectFile(root)})");
                }

                await OpenRomPathAsync(built); // reloads the ROM in the tabs; BuildChanged follows
            }
        }
        catch (Exception ex)
        {
            DecompLog("build error: " + ex.Message);
        }
        finally
        {
            _decompBuilding = false;
            DecompBuildButton.IsEnabled = true;
        }
    }

    private async void OnDecompOpenBuildClicked(object sender, RoutedEventArgs e)
    {
        var root = ResolveDecompRoot();
        var build = root is null ? null : Workspace.FindBuild(root);
        if (build is null)
        {
            MessageBox.Show(this, "This project has no build yet (build/scratch). Use Build ROM.", Title,
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        await OpenRomPathAsync(build.RomPath);
    }

    /// <summary>Unsaved edits in the open tabs before switching project folder: Yes saves them, No stays here.</summary>
    private bool SaveDirtyBeforeSwitching()
    {
        var dirty = DirtyHostedTabs();
        if (dirty.Count == 0)
        {
            return true;
        }

        var answer = MessageBox.Show(this,
            $"Unsaved changes in: {string.Join(", ", dirty)}.\n\nYes = save them and switch project, No = stay in this project.",
            Title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (answer != MessageBoxResult.Yes)
        {
            return false;
        }

        var failed = SaveDirtyHostedTabs();
        if (failed.Count == 0)
        {
            return true;
        }

        MessageBox.Show(this, $"Not saved: {string.Join(", ", failed)}. Staying in this project.", Title,
            MessageBoxButton.OK, MessageBoxImage.Warning);
        return false;
    }

    /// <summary>
    /// File > Open Decomp Project...: remembers the folder and opens its newest build (the tabs then follow the folder).
    /// Without a build the folder is still remembered; Build ROM makes one.
    /// </summary>
    private async void OnOpenDecompProjectFolderClicked(object? sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog { Title = "Medabots project folder (with data/expansion_bots.txt)" };
        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        var root = dialog.FolderName;
        if (!DecompBotStore.LooksLikeProject(root))
        {
            MessageBox.Show(this, $"Not a Medabots project folder (no {DecompBotStore.BotListFile} inside).", Title,
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!SaveDirtyBeforeSwitching())
        {
            return;
        }

        DecompProjectWindow.SaveProjectRoot(root);
        var build = Workspace.FindBuild(root);
        if (build is null)
        {
            SyncWorkspace();
            DecompLogExpander.IsExpanded = true;
            DecompLog($"no build in {root} yet: Build ROM makes one");
            return;
        }

        await OpenRomPathAsync(build.RomPath);
    }
}
