using System.IO;
using System.Text.Json;
using Medabots.Rom.Layout;
using Medabots.Rom.Metadata;
using Medabots.Rom.WPFEditor.Decomp;

namespace Medabots.Rom.WPFEditor;

/// <summary>
/// The one bot label format used in every list, menu and title (project rule, WPF_UNIFICADO_PLANO.md section 3,
/// item 6): <c>NNN  NAME  [pack]</c>. The id has 3 digits, <c>---</c> when the build gave no id, the name is kept
/// as stored, and the pack is shown only when it is known (decomp builds, <c>bot_ids.json</c> keys "pack/key").
/// Examples: <c>043  Metabee  [core]</c>, <c>122  CERVO  [shingata]</c>, <c>043  Metabee</c> (retail ROM).
/// Tinpets use <c>NNN  Tinpet (male)</c> / <c>NNN  Tinpet (female)</c> with no pack.
/// </summary>
internal static class BotLabels
{
    /// <summary>Builds the label; <paramref name="id"/> null prints <c>---</c>, <paramref name="pack"/> null omits it.</summary>
    public static string Of(int? id, string name, string? pack = null) =>
        $"{(id is { } i ? i.ToString("D3") : "---")}  {name}" + (string.IsNullOrEmpty(pack) ? "" : $"  [{pack}]");

    /// <summary>The pack of a bot key ("shingata/cervo" -> "shingata"); null when there is no key.</summary>
    public static string? PackOf(string? key) => key is null ? null : key.Split('/')[0];

    /// <summary>Label of a bot known by id and key (key null when the build has no bot list entry for it).</summary>
    public static string Row(int id, string? key, IReadOnlyDictionary<string, string> namesByKey) =>
        Of(id, NameFor(id, key, namesByKey), PackOf(key));

    /// <summary>
    /// Name of a bot: base-game ids (&lt; 120) from the catalog, decomp bots from the project's stored name, else the
    /// key's last part, else <c>Bot NNN</c> (same fallback the ROM roster uses).
    /// </summary>
    public static string NameFor(int id, string? key, IReadOnlyDictionary<string, string> namesByKey)
    {
        if (id < RomLayout.OriginalBotCount)
        {
            return MedabotsMetadata.Default.GetBotName(id);
        }

        if (key is not null)
        {
            return namesByKey.TryGetValue(key, out var name) && name.Length > 0 ? name : key.Split('/')[^1];
        }

        return $"Bot {id:D3}";
    }

    /// <summary>Key by id from the <c>bot_ids.json</c> sitting next to a built ROM (empty when there is none).</summary>
    public static Dictionary<int, string> KeysById(string romPath)
    {
        var byId = new Dictionary<int, string>();
        var file = Path.Combine(Path.GetDirectoryName(romPath) ?? "", "bot_ids.json");
        if (!File.Exists(file))
        {
            return byId;
        }

        try
        {
            var ids = JsonSerializer.Deserialize<Dictionary<string, int>>(File.ReadAllText(file)) ?? [];
            foreach (var (key, id) in ids)
            {
                byId.TryAdd(id, key);
            }
        }
        catch (Exception exception) when (exception is IOException or JsonException or UnauthorizedAccessException)
        {
            byId.Clear();
        }

        return byId;
    }

    /// <summary>Stored names of the decomp project's bots by key (empty when the folder is not a project).</summary>
    public static Dictionary<string, string> DecompNames(string? root)
    {
        var names = new Dictionary<string, string>(StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(root))
        {
            return names;
        }

        try
        {
            var store = new DecompBotStore(root);
            store.Load();
            foreach (var bot in store.Bots)
            {
                names[bot.Key] = bot.Name;
            }
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException or JsonException or UnauthorizedAccessException)
        {
            names.Clear();
        }

        return names;
    }
}
