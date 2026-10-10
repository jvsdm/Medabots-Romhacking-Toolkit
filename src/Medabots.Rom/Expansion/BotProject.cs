using System.Text.Json;
using System.Text.Json.Nodes;

namespace Medabots.Rom.Expansion;

/// <summary>
/// A list of bots to add together (the editor's project file): { "format", "version", "bots": [ ... ] },
/// each bot in the data/bots JSON format, its "art" folder relative to the project file.
/// </summary>
public static class BotProject
{
    public const string Format = "medabots-bot-project";

    public static string ToJson(IEnumerable<(BotDefinition Bot, string? ArtFolder)> bots)
    {
        var list = new JsonArray();
        foreach (var (bot, art) in bots)
        {
            list.Add(JsonNode.Parse(bot.ToJson(art)));
        }

        var document = new JsonObject { ["format"] = Format, ["version"] = 1, ["bots"] = list };
        return document.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        });
    }

    /// <summary>The bots of a project file, or the single bot of a data/bots JSON.</summary>
    public static IReadOnlyList<BotDefinition> Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("bots", out var bots))
        {
            return [BotDefinition.Parse(json)];
        }

        return bots.EnumerateArray().Select(bot => BotDefinition.Parse(bot.GetRawText())).ToList();
    }
}
