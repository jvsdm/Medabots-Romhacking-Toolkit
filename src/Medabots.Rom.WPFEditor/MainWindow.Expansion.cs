using System.IO;
using System.Windows;
using Medabots.Rom.Expansion;

namespace Medabots.Rom.WPFEditor;

public partial class MainWindow
{
    private const string GbaRomFilter = "Game Boy Advance ROM (*.gba)|*.gba|All files (*.*)|*.*";

    /// <summary>
    /// Builds the expanded Metabee (E): asks for the Metabee (E) ROM and, optionally, the Shingata
    /// Medarot Kabuto ROM (its 60 bots); the sources are never modified. Writes a new file and opens it.
    /// </summary>
    private async void OnGenerateExpandedRomClicked(object? sender, RoutedEventArgs e)
    {
        var metabeePath = PickOpenFilePath("Select the Medabots: Metabee (Europe) ROM", GbaRomFilter);
        if (string.IsNullOrWhiteSpace(metabeePath))
        {
            return;
        }

        string? shingataPath = null;
        var includeShingata = System.Windows.MessageBox.Show(this,
            "Also add the 60 Medabots from Shingata Medarot?\n\n" +
            "Yes: you will be asked for the Shingata Medarot - Kabuto Version (J) ROM.\n" +
            "No: the expanded ROM keeps the original roster (+ Sägehorn); bots can be added later with the green + in the Parts tab.",
            "Generate Expanded ROM", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
        if (includeShingata == MessageBoxResult.Cancel)
        {
            return;
        }

        if (includeShingata == MessageBoxResult.Yes)
        {
            shingataPath = PickOpenFilePath("Select the Shingata Medarot - Kabuto Version (J) ROM", GbaRomFilter);
            if (string.IsNullOrWhiteSpace(shingataPath))
            {
                return;
            }
        }

        var outputPath = PickSaveFilePath("Save the expanded ROM as", GbaRomFilter,
            Path.Combine(Path.GetDirectoryName(metabeePath) ?? "", "Medabots_expanded.gba"));
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            return;
        }

        if (SamePath(outputPath, metabeePath) || (shingataPath is not null && SamePath(outputPath, shingataPath)))
        {
            await DisplayAlertAsync("Generate Expanded ROM", "Choose a new file: the source ROMs are not overwritten.", "OK");
            return;
        }

        try
        {
            SetLoadingState(true, "Generating expanded ROM...", 0.05);
            var metabee = await File.ReadAllBytesAsync(metabeePath);
            var shingata = shingataPath is null ? null : await File.ReadAllBytesAsync(shingataPath);
            var result = await Task.Run(() => RomExpander.Build(metabee, shingata));
            await File.WriteAllBytesAsync(outputPath, result.Rom);
            SetLoadingState(false, string.Empty, 0);
            await DisplayAlertAsync("Generate Expanded ROM",
                $"{result.BotCount} Medabots written to {Path.GetFileName(outputPath)} (Tinpet 236/237).\n" +
                "Saves from the original game are not compatible with it. The ROM opens now.", "OK");
        }
        catch (Exception ex) when (ex is InvalidDataException or IOException or UnauthorizedAccessException)
        {
            SetLoadingState(false, string.Empty, 0);
            await DisplayAlertAsync("Generate Expanded ROM", ex.Message, "OK");
            return;
        }

        await OpenRomPathAsync(outputPath);
    }

    /// <summary>
    /// Appends a Medabot (data/bots JSON: name, parts with stats, graphics_from, optional recolor) to an
    /// expanded ROM as the next ID. Reads the ROM file on disk, writes a new file and opens it.
    /// </summary>
    private async void OnAddBotClicked(object? sender, RoutedEventArgs e)
    {
        var romPath = _session?.RomFile.FilePath ?? PickOpenFilePath("Select the expanded ROM", GbaRomFilter);
        if (string.IsNullOrWhiteSpace(romPath))
        {
            return;
        }

        try
        {
            var rom = await File.ReadAllBytesAsync(romPath);
            Medabots.Rom.WPFEditor.Dialogs.NewBotWindow window;
            try
            {
                window = new Medabots.Rom.WPFEditor.Dialogs.NewBotWindow(romPath, rom) { Owner = this };
            }
            catch (Exception ex) when (ex is InvalidDataException or InvalidOperationException or ArgumentException or IndexOutOfRangeException)
            {
                throw new InvalidDataException($"{Path.GetFileName(romPath)} does not look like an expanded Metabee (E) ROM: {ex.Message}");
            }

            if (window.ShowDialog() != true || window.OutputPath is null)
            {
                return;
            }

            await DisplayAlertAsync("New Medabot",
                $"{window.AddedSummary}\n\n" +
                "Unexported edits of the open ROM are not included (the ROM file on disk was used). The new ROM opens now.", "OK");
            await OpenRomPathAsync(window.OutputPath);
        }
        catch (Exception ex) when (ex is InvalidDataException or IOException or UnauthorizedAccessException)
        {
            await DisplayAlertAsync("New Medabot", ex.Message, "OK");
        }
    }

    private static bool SamePath(string a, string b) =>
        string.Equals(Path.GetFullPath(a), Path.GetFullPath(b), StringComparison.OrdinalIgnoreCase);
}
