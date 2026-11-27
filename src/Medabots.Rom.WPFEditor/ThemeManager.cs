using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Medabots.Rom.WPFEditor;

/// <summary>
/// Light / dark theme. Standard controls follow the WPF Fluent theme (Application.ThemeMode); the
/// editor's own colours are the "Theme.*" brushes of Themes/Light.xaml and Themes/Dark.xaml, used through
/// DynamicResource (or SetResourceReference in code) so they switch at once. The choice is remembered.
/// </summary>
public static class ThemeManager
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Medabots.Rom.WPFEditor", "theme.txt");

    public static bool IsDark { get; private set; }

    public static event EventHandler? Changed;

    /// <summary>The theme saved last time (dark by default).</summary>
    public static bool LoadSavedIsDark()
    {
        try
        {
            return !File.Exists(SettingsPath) || File.ReadAllText(SettingsPath).Trim() != "light";
        }
        catch (IOException)
        {
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return true;
        }
    }

    public static void Apply(bool dark, bool save = true)
    {
        var app = System.Windows.Application.Current;
        var dictionaries = app.Resources.MergedDictionaries;
        var colours = new ResourceDictionary
        {
            Source = new Uri($"pack://application:,,,/Themes/{(dark ? "Dark" : "Light")}.xaml", UriKind.Absolute),
        };
        var old = dictionaries.FirstOrDefault(d => d.Source?.OriginalString.Contains("/Themes/", StringComparison.Ordinal) == true);
        if (old is not null)
        {
            dictionaries.Remove(old);
        }

        dictionaries.Add(colours);
#pragma warning disable WPF0001 // Fluent ThemeMode is marked experimental in .NET 9/10
        app.ThemeMode = dark ? ThemeMode.Dark : ThemeMode.Light;
#pragma warning restore WPF0001
        IsDark = dark;
        if (!_hooked)
        {
            // Windows opened later (dialogs) get the title bar colour too.
            EventManager.RegisterClassHandler(typeof(Window), FrameworkElement.LoadedEvent, new RoutedEventHandler((s, _) => SetTitleBar((Window)s)));
            _hooked = true;
        }

        foreach (Window window in app.Windows)
        {
            SetTitleBar(window);
        }

        if (save)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
                File.WriteAllText(SettingsPath, dark ? "dark" : "light");
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // Not remembered; the theme still applies now.
            }
        }

        Changed?.Invoke(null, EventArgs.Empty);
    }

    private static bool _hooked;

    // Dark title bar (Windows 10 20H1+ / 11): DWMWA_USE_IMMERSIVE_DARK_MODE.
    private static void SetTitleBar(Window window)
    {
        var handle = new WindowInteropHelper(window).Handle;
        if (handle == IntPtr.Zero)
        {
            return;
        }

        var value = IsDark ? 1 : 0;
        if (DwmSetWindowAttribute(handle, 20, ref value, sizeof(int)) != 0)
        {
            DwmSetWindowAttribute(handle, 19, ref value, sizeof(int));     // older Windows 10 builds
        }
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

    /// <summary>Points a brush property at a theme colour, so it follows later theme switches.</summary>
    public static void SetBrush(FrameworkElement element, DependencyProperty property, string key) =>
        element.SetResourceReference(property, key);
}
