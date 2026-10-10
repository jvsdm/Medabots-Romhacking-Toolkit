using System.Windows;

namespace Medabots.Rom.WPFEditor;

public partial class MainWindow
{
    private void OnViewMenuOpened(object sender, RoutedEventArgs e) => DarkModeMenuItem.IsChecked = ThemeManager.IsDark;

    private void OnDarkModeClicked(object sender, RoutedEventArgs e) => ThemeManager.Apply(DarkModeMenuItem.IsChecked);

    /// <summary>Opens the ROM given on the command line.</summary>
    internal Task OpenStartupRomAsync(string romPath) => OpenRomPathAsync(romPath);
}
