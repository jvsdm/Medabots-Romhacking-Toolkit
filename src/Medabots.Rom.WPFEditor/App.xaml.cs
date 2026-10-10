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

        // Medabots.Rom.WPFEditor.exe <rom.gba>: opens that ROM once the main window is up.
        if (e.Args.Length > 0 && System.IO.File.Exists(e.Args[0]))
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

