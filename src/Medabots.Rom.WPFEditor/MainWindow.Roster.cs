using System.IO;
using Medabots.Rom.Editor;
using Medabots.Rom.Metadata;
using Medabots.Rom.Text;

namespace Medabots.Rom.WPFEditor;

public partial class MainWindow
{
    private int RosterBotCount => _session?.RomFile.Layout.BotCount ?? RomLayoutDefaults.BotCount;

    /// <summary>
    /// Expanded ROMs carry more Medabots than the bundled catalog knows: names for the extra IDs are
    /// read from the ROM's own name tables, and bot pickers are rebuilt for the ROM's roster size.
    /// </summary>
    private void ApplyRomRoster()
    {
        if (_session is null)
        {
            return;
        }

        var rom = _session.RomFile;
        var layout = rom.Layout;
        var codec = new MedabotsTextCodec();
        var catalog = _metadata.Catalog;

        for (var botId = catalog.Bots.Count; botId < layout.BotCount; botId++)
        {
            catalog.Bots.Add(TryReadName(() => layout.ReadBotName(rom.Data, botId, codec)) ?? $"Bot {botId:D3}");
        }

        for (var partId = catalog.Parts.Count; partId < layout.BotCount * 4; partId++)
        {
            catalog.Parts.Add(TryReadName(() => layout.ReadPartName(rom.Data, partId, codec)) ?? $"Part {partId:D3}");
        }

        _shopSlotOptions.RemoveAll(item => item.Id != MedabotsRomSchema.EmptyShopSlot);
        _shopSlotOptions.AddRange(Enumerable.Range(0, layout.BotCount).Select(id => new BrowserItem(id, BuildShopSlotOptionTitle(id))));
        _starterBotOptions.Clear();
        _starterBotOptions.AddRange(Enumerable.Range(0, Math.Min(catalog.Bots.Count, layout.BotCount))
            .Select(id => new BrowserItem(id, $"{id:D3}  {_metadata.GetBotName(id)}")));
        StarterPartEntry.ItemsSource = null;
        StarterPartEntry.ItemsSource = _starterBotOptions;
    }

    private static string? TryReadName(Func<string?> read)
    {
        try
        {
            var name = read();
            return string.IsNullOrWhiteSpace(name) ? null : name;
        }
        catch (Exception exception) when (exception is InvalidDataException or ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    private static class RomLayoutDefaults
    {
        public const int BotCount = Layout.RomLayout.OriginalBotCount;
    }
}
