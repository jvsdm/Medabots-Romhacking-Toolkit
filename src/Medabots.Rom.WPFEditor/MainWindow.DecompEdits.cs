using System.IO;
using Medabots.Rom.Metadata;
using Medabots.Rom.Projects;
using Medabots.Rom.WPFEditor.Decomp;

namespace Medabots.Rom.WPFEditor;

/// <summary>
/// Phase 4: edits of the ROM tabs (messages, events, maps, shops...) that survive a decomp build. Each build keeps its pure
/// ROM in &lt;build&gt;/base/ (tools/decomp_build.py). In Decomp mode the tabs read that pure ROM and the edits live in one
/// project per decomp folder, data/wpf/project.medahack.json; Export ROM writes base + edits over the build's playable ROM,
/// and every Build ROM applies the project again to the new build.
/// </summary>
public partial class MainWindow
{
    /// <summary>The build's playable ROM while the tabs read its base/ copy (Decomp mode); null otherwise.</summary>
    private string? _decompPlayRom;

    internal static string DecompProjectFile(string root) => Path.Combine(root, "data", "wpf", "project.medahack.json");

    /// <summary>
    /// Plan M6: in Decomp mode (a build of a project with data/events) the scripts are edited as text files; opening a
    /// script (Event scripts button, or an NPC's script button in the Maps tab) shows them. Returns false outside it.
    /// </summary>
    private bool TryOpenDecompEventText(int? eventId)
    {
        var root = ResolveDecompRoot();
        if (root is null || !File.Exists(Path.Combine(root, "data", "events", "_opcodes.json")))
        {
            return false;
        }

        var window = new DecompEventTextWindow(root, _decompPlayRom is null ? null : BaseRomOf(_decompPlayRom), eventId)
        {
            Owner = this,
        };
        window.Show();
        return true;
    }

    private void OnDecompEventsClicked(object sender, System.Windows.RoutedEventArgs e)
    {
        if (!TryOpenDecompEventText(null))
        {
            System.Windows.MessageBox.Show(this, "The project folder has no data/events (tools/events_lift.py makes it).", Title);
        }
    }

    /// <summary>The pure copy of a build ROM (&lt;build&gt;/base/&lt;name&gt;), when the build has one.</summary>
    private static string? BaseRomOf(string romPath)
    {
        var dir = Path.GetDirectoryName(romPath);
        if (dir is null || string.Equals(Path.GetFileName(dir), "base", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var candidate = Path.Combine(dir, "base", Path.GetFileName(romPath));
        return File.Exists(candidate) ? candidate : null;
    }

    /// <summary>
    /// Opening a decomp build: the session is its base/ ROM and the project is the folder's data/wpf project (loaded when it
    /// exists). Returns the ROM path to open, the decomp root (null outside Decomp mode) and the project to edit.
    /// </summary>
    private async Task<(string SessionPath, string? Root, RomHackProject Project)> ResolveDecompOpenAsync(string romPath)
    {
        var basePath = BaseRomOf(romPath);
        var root = basePath is null ? null : DecompBotStore.FindProjectAbove(romPath);
        if (root is null)
        {
            _decompPlayRom = null;
            return (romPath, null, CreateInMemoryProject(romPath));
        }

        _decompPlayRom = romPath;
        var file = DecompProjectFile(root);
        var project = File.Exists(file) ? await RomHackProjectSerializer.LoadAsync(file) : CreateInMemoryProject(basePath);
        project.ProjectFilePath = file;
        project.SourceRomPath = basePath;
        return (basePath!, root, project);
    }

    /// <summary>Saves the current edits into the folder's project file (Decomp mode), before a build or an export.</summary>
    private async Task SaveDecompProjectAsync()
    {
        if (_decompPlayRom is null || string.IsNullOrWhiteSpace(_project.ProjectFilePath))
        {
            return;
        }

        PopulateProjectFromEditor(_project.ProjectFilePath);
        Directory.CreateDirectory(Path.GetDirectoryName(_project.ProjectFilePath)!);
        await RomHackProjectSerializer.SaveAsync(_project, _project.ProjectFilePath);
    }

    /// <summary>
    /// After a build: the folder's project applied over the new build's base/ ROM, written to its playable ROM. Returns
    /// the number of changes (0 without a project or without edits).
    /// </summary>
    private async Task<int> ApplyDecompProjectAsync(string root, string playRom)
    {
        var file = DecompProjectFile(root);
        var basePath = BaseRomOf(playRom);
        if (!File.Exists(file) || basePath is null)
        {
            return 0;
        }

        var project = await RomHackProjectSerializer.LoadAsync(file);
        var session = await RomHackSession.OpenAsync(basePath);
        project.SourceRomPath = basePath;
        project.TextProfileId = MedabotsRomTextProfiles.Detect(session.RomFile)?.Id ?? project.TextProfileId;
        var changes = _projectApplicator.BuildChanges(project, session.RomFile).Count;
        if (changes == 0 && project.MapAdditions.Count == 0 && project.PortraitAdditions.Count == 0)
        {
            return 0;
        }

        _projectApplicator.Apply(project, session);
        await session.SaveAsAsync(playRom);
        return Math.Max(changes, 1);
    }
}
