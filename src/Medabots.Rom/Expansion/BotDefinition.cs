using System.Text.Json;

namespace Medabots.Rom.Expansion;

/// <summary>
/// A Medabot to add to an expanded ROM, in the expansion project's data/bots JSON format:
/// name, four parts (name + named stats) and where its graphics come from.
/// </summary>
public sealed class BotDefinition
{
    public static readonly string[] PartKinds = ["head", "right_arm", "left_arm", "legs"];

    private static readonly string[] CombatFields =
    [
        "medal_compat", "technique", "speciality", "female", "armor", "success", "power", "chain_reaction",
        "uses_or_charge", "radiation", "u10", "u11", "u12", "u13", "u14", "u15",
    ];

    private static readonly string[] LegFields =
    [
        "medal_compat", "leg_type", "speciality", "female", "armor", "propulsion", "evasion", "defense",
        "proximity", "remoteness", "u10", "u11", "u12", "u13", "u14", "u15",
    ];

    public string Name { get; init; } = "";

    /// <summary>Head, right arm, left arm, legs.</summary>
    public IReadOnlyList<(string Name, byte[] Stats)> Parts { get; init; } = [];

    /// <summary>Bot whose drawings, mini sprite and movement style are reused (default Metabee, 43).</summary>
    public int GraphicsFrom { get; init; } = 43;

    /// <summary>Optional colour swaps applied to the reused drawings ("#rrggbb" -> "#rrggbb").</summary>
    public IReadOnlyDictionary<string, string>? Recolor { get; init; }

    /// <summary>Folder of the bot's own drawings as written in the JSON ("art"), relative to the JSON file.</summary>
    public string? ArtFolder { get; init; }

    /// <summary>Own drawings by slot name (see <see cref="BotArt.Slots"/>); missing slots keep the source bot's.</summary>
    public IReadOnlyDictionary<string, ArtImage>? Art { get; init; }

    /// <summary>Field names of a part's 16 stat bytes, in record order.</summary>
    public static IReadOnlyList<string> StatFields(string kind) => kind == "legs" ? LegFields : CombatFields;

    /// <summary>The data/bots JSON for this bot (stats as named fields).</summary>
    public string ToJson(string? artFolder = null)
    {
        var parts = new Dictionary<string, object>();
        for (var k = 0; k < PartKinds.Length; k++)
        {
            var fields = StatFields(PartKinds[k]);
            parts[PartKinds[k]] = new Dictionary<string, object>
            {
                ["name"] = Parts[k].Name,
                ["stats"] = fields.Select((f, i) => (f, i)).ToDictionary(t => t.f, t => (int)Parts[k].Stats[t.i]),
            };
        }

        var document = new Dictionary<string, object> { ["name"] = Name, ["graphics_from"] = GraphicsFrom };
        if (Recolor is { Count: > 0 })
        {
            document["recolor"] = Recolor;
        }

        if ((artFolder ?? ArtFolder) is { } folder)
        {
            document["art"] = folder;
        }

        document["parts"] = parts;
        return JsonSerializer.Serialize(document, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        });
    }

    public static BotDefinition Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        if (root.TryGetProperty("graphics_source", out _))
        {
            throw new InvalidDataException("\"graphics_source\" (graphics from another ROM) is only supported when generating the expansion.");
        }

        var parts = root.GetProperty("parts");
        var list = new List<(string, byte[])>();
        foreach (var kind in PartKinds)
        {
            var part = parts.GetProperty(kind);
            var stats = part.GetProperty("stats");
            var fields = kind == "legs" ? LegFields : CombatFields;
            var bytes = new byte[16];
            for (var i = 0; i < fields.Length; i++)
            {
                var value = stats.GetProperty(fields[i]).GetInt32();
                if (value is < 0 or > 255)
                {
                    throw new InvalidDataException($"{kind}.{fields[i]} = {value}: stats are single bytes (0-255).");
                }

                bytes[i] = (byte)value;
            }

            list.Add((part.GetProperty("name").GetString() ?? "", bytes));
        }

        Dictionary<string, string>? recolor = null;
        if (root.TryGetProperty("recolor", out var recolorElement))
        {
            recolor = recolorElement.EnumerateObject().ToDictionary(p => p.Name, p => p.Value.GetString() ?? "");
        }

        return new BotDefinition
        {
            Name = root.GetProperty("name").GetString() ?? "",
            Parts = list,
            GraphicsFrom = root.TryGetProperty("graphics_from", out var from) ? from.GetInt32() : 43,
            Recolor = recolor,
            ArtFolder = root.TryGetProperty("art", out var art) ? art.GetString() : null,
        };
    }
}
