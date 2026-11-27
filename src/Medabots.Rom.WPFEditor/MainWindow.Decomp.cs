using System.Windows;
using Medabots.Rom.WPFEditor.Dialogs;

namespace Medabots.Rom.WPFEditor;

public partial class MainWindow
{
    private DecompProjectWindow? _decompProjectWindow;

    /// <summary>
    /// Opens the decompilation project editor: selects its Bot subtab in the Bots group (hosted there, see
    /// MainWindow.BotsHost.cs). The project is the one above the open ROM when it was built from a decomp project.
    /// </summary>
    private void OnOpenDecompProjectClicked(object? sender, RoutedEventArgs e)
    {
        BotSection.Visibility = Visibility.Visible; // a retail ROM hides it; the menu asks for it explicitly
        SelectSection(BotSection);
    }
}
