using System.IO;
using Medabots.Rom.Animation;
using Medabots.Rom.Metadata;

namespace Medabots.Rom.WPFEditor;

public enum WorkspaceMode
{
    Rom,
    Decomp,
}

/// <summary>The files of one build made from a decomp project (bot_ids.json, pose_rows.json and the layout sidecar sit next to the ROM).</summary>
public sealed record BuildFiles(string RomPath)
{
    public string MapPath => Path.ChangeExtension(RomPath, ".map");

    public string BotIdsPath => Path.Combine(Path.GetDirectoryName(RomPath)!, "bot_ids.json");

    public string PoseRowsPath => Path.Combine(Path.GetDirectoryName(RomPath)!, "pose_rows.json");

    public string LayoutPath => Path.Combine(Path.GetDirectoryName(RomPath)!, "medabots_rokusho.layout.json");
}

/// <summary>
/// The open project: the mode, the decomp folder, the build the editors use, the field names and one SkeletonRig shared
/// by the tabs (read on first use). The main window owns one instance and calls <see cref="Update"/> when the ROM or the
/// folder changes; the windows listen to <see cref="ProjectRootChanged"/> and <see cref="BuildChanged"/>.
/// </summary>
public sealed class Workspace
{
    private SkeletonRig? _rig;
    private bool _rigLoaded;

    /// <summary>The decomp folder changed (another folder, or none).</summary>
    public event EventHandler? ProjectRootChanged;

    /// <summary>The newest build of the folder changed (another ROM file, or none).</summary>
    public event EventHandler? BuildChanged;

    public WorkspaceMode Mode { get; private set; } = WorkspaceMode.Rom;

    public string? DecompRoot { get; private set; }

    public BuildFiles? Build { get; private set; }

    public FieldCatalog Fields => FieldCatalog.For(DecompRoot);

    /// <summary>The shared rig of <see cref="Build"/>, read on first use; null without a build or when it cannot be read.</summary>
    public SkeletonRig? Rig
    {
        get
        {
            if (!_rigLoaded && Build is { } build)
            {
                _rigLoaded = true;
                try
                {
                    _rig = new SkeletonRig(File.ReadAllBytes(build.RomPath), File.ReadAllText(build.MapPath));
                }
                catch (Exception)
                {
                    _rig = null;
                }
            }

            return _rig;
        }
    }

    /// <summary>The shared rig when it belongs to the ROM at <paramref name="romPath"/>; null otherwise (the caller reads its own copy).</summary>
    public SkeletonRig? RigFor(string romPath) =>
        Build is { } build && string.Equals(build.RomPath, romPath, StringComparison.OrdinalIgnoreCase) ? Rig : null;

    /// <summary>Sets what the project is now. Raises ProjectRootChanged and BuildChanged for what actually changed.</summary>
    public void Update(WorkspaceMode mode, string? decompRoot)
    {
        Mode = mode;
        var rootChanged = !string.Equals(DecompRoot, decompRoot, StringComparison.OrdinalIgnoreCase);
        var build = decompRoot is null ? null : FindBuild(decompRoot);
        var buildChanged = !string.Equals(Build?.RomPath, build?.RomPath, StringComparison.OrdinalIgnoreCase);

        DecompRoot = decompRoot;
        Build = build;
        if (buildChanged)
        {
            _rig = null;
            _rigLoaded = false;
        }

        // State is set before the events run: a handler may call Update again (it then sees no change).
        if (rootChanged)
        {
            ProjectRootChanged?.Invoke(this, EventArgs.Empty);
        }

        if (buildChanged)
        {
            BuildChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>Newest build/scratch/*/medabots_rokusho.gba that has its .map next to it, preferring builds with the bot list (bot_ids.json).</summary>
    public static BuildFiles? FindBuild(string root)
    {
        var scratch = Path.Combine(root, "build", "scratch");
        if (!Directory.Exists(scratch))
        {
            return null;
        }

        var rom = Directory.GetDirectories(scratch)
            .Select(d => Path.Combine(d, "medabots_rokusho.gba"))
            .Where(f => File.Exists(f) && File.Exists(Path.ChangeExtension(f, ".map")))
            .OrderByDescending(f => File.Exists(Path.Combine(Path.GetDirectoryName(f)!, "bot_ids.json")))
            .ThenByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();
        return rom is null ? null : new BuildFiles(rom);
    }
}
