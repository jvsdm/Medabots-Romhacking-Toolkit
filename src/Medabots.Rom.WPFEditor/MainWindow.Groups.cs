using System.Windows.Controls;

namespace Medabots.Rom.WPFEditor;

public partial class MainWindow
{
    /// <summary>
    /// Selects an inner section tab and also selects its parent group tab, so the section becomes visible.
    /// </summary>
    private void SelectSection(TabItem section)
    {
        section.IsSelected = true;
        if (section.Parent is System.Windows.Controls.TabControl { Parent: TabItem group })
            group.IsSelected = true;
    }
}
