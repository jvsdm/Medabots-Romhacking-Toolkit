using System.Configuration;
using System.Data;
using System.Windows;

namespace Medabots.Rom.WPFEditor;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        ThemeManager.Apply(ThemeManager.LoadSavedIsDark(), save: false);
        base.OnStartup(e);

        // Medabots.Rom.WPFEditor.exe --pose-viewer <decomp project folder>: opens the Pose Viewer over the main window.
        // ... --move-editor <folder> [move id] / --skeleton-editor <folder> [mold]: same for the Move / Skeleton Editor.
        // ... --event-scripts <folder> [event id]: the event scripts as text (data/events), with the newest build's base ROM
        // for the message preview.
        if (e.Args.Length > 1 && e.Args[0] == "--event-scripts" && System.IO.Directory.Exists(e.Args[1]))
        {
            var evRoot = System.IO.Path.GetFullPath(e.Args[1]);
            int? evId = e.Args.Length > 2 && int.TryParse(e.Args[2], out var ev) ? ev : null;
            var baseRom = new System.IO.DirectoryInfo(System.IO.Path.Combine(evRoot, "build", "scratch"))
                .EnumerateDirectories("decomp_hp*")
                .Select(d => new System.IO.FileInfo(System.IO.Path.Combine(d.FullName, "base", "medabots_rokusho.gba")))
                .Where(f => f.Exists).OrderByDescending(f => f.LastWriteTimeUtc).FirstOrDefault()?.FullName;
            EventManager.RegisterClassHandler(typeof(MainWindow), FrameworkElement.LoadedEvent, new RoutedEventHandler((sender, _) =>
            {
                if (evRoot is not null && sender is MainWindow window)
                {
                    new Decomp.DecompEventTextWindow(evRoot, baseRom, evId) { Owner = window }.Show();
                    evRoot = null;
                }
            }));
        }
        else if (e.Args.Length > 1 && e.Args[0] is "--pose-viewer" or "--move-editor" or "--skeleton-editor" && System.IO.Directory.Exists(e.Args[1]))
        {
            var root = System.IO.Path.GetFullPath(e.Args[1]);
            var moves = e.Args[0] == "--move-editor";
            int? initialMove = e.Args.Length > 2 && int.TryParse(e.Args[2], out var m) ? m : null;
            EventManager.RegisterClassHandler(typeof(MainWindow), FrameworkElement.LoadedEvent, new RoutedEventHandler((sender, _) =>
            {
                if (root is not null && sender is MainWindow window)
                {
                    if (e.Args[0] == "--skeleton-editor")
                        new Dialogs.SkeletonEditorWindow(root, initialMove) { Owner = window }.Show();
                    else if (moves)
                        new Dialogs.MoveEditorWindow(root, initialMove) { Owner = window }.Show();
                    else
                        new Dialogs.PoseViewerWindow(root) { Owner = window }.Show();
                    root = null;
                }
            }));
        }
        // Medabots.Rom.WPFEditor.exe <rom.gba>: opens that ROM once the main window is up.
        else if (e.Args.Length > 0 && System.IO.File.Exists(e.Args[0]))
        {
            var path = System.IO.Path.GetFullPath(e.Args[0]);
            EventManager.RegisterClassHandler(typeof(MainWindow), FrameworkElement.LoadedEvent, new RoutedEventHandler(async (sender, _) =>
            {
                if (path is not null && sender is MainWindow window)
                {
                    var open = path;
                    path = null;
                    await window.OpenStartupRomAsync(open);
                }
            }));
        }
    }
}

