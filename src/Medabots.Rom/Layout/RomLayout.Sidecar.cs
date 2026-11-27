using System.Text.Json;

namespace Medabots.Rom.Layout;

/// <summary>
/// ROMs built from the decompilation (medabots-rokusho + the expansion generator) are relinked: every table moves and
/// the code compares bot IDs in 16 bits, so the code signatures and the 8-bit roster immediates no longer describe the
/// ROM. The build writes <c>&lt;rom&gt;.layout.json</c> next to it (tools/decomp_layout.py) with the table addresses
/// taken from the linker map; when that file exists it wins over the signatures.
/// </summary>
public sealed partial class RomLayout
{
    /// <summary>True when the tables came from a decompilation build's layout file.</summary>
    public bool FromLayoutFile { get; private init; }

    /// <summary>
    /// Text-profile addresses of a decompilation build (layout file "addresses": TextPointerTableOffset,
    /// TextDumpOffset, StarterOffset, BattlePointerTableOffset, BattleCount, EventTableOffset, EventCount); null for a
    /// retail ROM. The fixed addresses of MedabotsRomTextProfiles only describe the retail ROMs.
    /// </summary>
    public IReadOnlyDictionary<string, int>? ProfileAddresses { get; private init; }

    internal static string LayoutFilePath(string romPath) => Path.ChangeExtension(romPath, ".layout.json");

    private sealed record LayoutFile(Dictionary<string, int> Tables, int? BotCount, int? TinpetId,
        Dictionary<string, int>? Addresses);

    private static LayoutFile? ReadLayoutFile(string? romPath)
    {
        if (string.IsNullOrWhiteSpace(romPath))
        {
            return null;
        }

        var path = LayoutFilePath(romPath);
        if (!File.Exists(path))
        {
            return null;
        }

        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var root = doc.RootElement;
        var tables = new Dictionary<string, int>(StringComparer.Ordinal);
        if (root.TryGetProperty("tables", out var t))
        {
            foreach (var entry in t.EnumerateObject())
            {
                tables[entry.Name] = entry.Value.GetInt32();
            }
        }

        Dictionary<string, int>? addresses = null;
        if (root.TryGetProperty("addresses", out var adr))
        {
            addresses = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var entry in adr.EnumerateObject())
            {
                addresses[entry.Name] = entry.Value.GetInt32();
            }
        }

        int? Optional(string name) => root.TryGetProperty(name, out var v) ? v.GetInt32() : null;
        return new LayoutFile(tables, Optional("bot_count"), Optional("tinpet_id"), addresses);
    }

    private RomLayout WithRosterOverride(LayoutFile? file)
    {
        if (file is null)
        {
            return this;
        }

        return new RomLayout(_tables)
        {
            BotCount = file.BotCount ?? BotCount,
            TinpetId = file.TinpetId ?? TinpetId,
            FromLayoutFile = true,
            ProfileAddresses = file.Addresses,
        };
    }
}
