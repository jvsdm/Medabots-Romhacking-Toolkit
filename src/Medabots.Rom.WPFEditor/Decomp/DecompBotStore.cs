using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Medabots.Rom.WPFEditor.Decomp;

/// <summary>
/// Bots of a decompilation project (the Medabots folder with data/expansion_bots.txt): one JSON per bot, the source of
/// truth the generator (tools/decomp_expand.py) builds the ROM from. data/bots/NNN_*.json = binary block (Shingata,
/// m9, binario), data/navi/bots/NNN_*.json = Medarot Navi. Files are saved in the same format the Python tools write
/// (indent 1, non-ASCII kept, same line endings and final newline), so an unedited save leaves git clean.
/// </summary>
public sealed class DecompBotStore
{
    public const string BotListFile = "data/expansion_bots.txt";

    public DecompBotStore(string root)
    {
        Root = root;
    }

    public string Root { get; }

    public List<DecompBot> Bots { get; } = [];

    public static bool LooksLikeProject(string? folder) =>
        !string.IsNullOrWhiteSpace(folder) && File.Exists(Path.Combine(folder, BotListFile));

    /// <summary>The project folder above a ROM built in build/scratch/decomp_* (or null).</summary>
    public static string? FindProjectAbove(string? path)
    {
        for (var dir = path is null ? null : Path.GetDirectoryName(Path.GetFullPath(path)); dir is not null;
             dir = Path.GetDirectoryName(dir))
        {
            if (LooksLikeProject(dir))
            {
                return dir;
            }
        }

        return null;
    }

    public void Load()
    {
        Bots.Clear();
        foreach (var file in Directory.GetFiles(Path.Combine(Root, "data", "bots"), "*.json").Order(StringComparer.Ordinal))
        {
            var stem = Path.GetFileNameWithoutExtension(file);
            if (stem.Length < 5 || !int.TryParse(stem[..3], out var id) || id < 120)
            {
                continue; // 0..119 are the game's own bots: always in the ROM, edited elsewhere
            }

            var pack = id == 120 ? "m9" : id <= 180 ? "shingata" : "binario";
            Bots.Add(DecompBot.Load(file, $"{pack}/{stem[4..]}"));
        }

        var naviDir = Path.Combine(Root, "data", "navi", "bots");
        if (Directory.Exists(naviDir))
        {
            foreach (var file in Directory.GetFiles(naviDir, "*.json").Order(StringComparer.Ordinal))
            {
                var bot = DecompBot.Load(file, null);
                bot.Key = "navi/" + (bot.Json["slug"]?.GetValue<string>() ?? Path.GetFileNameWithoutExtension(file)[4..]);
                Bots.Add(bot);
            }
        }

        ApplyBotList();
    }

    /// <summary>Marks which bots the list includes (same rules as decomp_expand.read_bot_list).</summary>
    public void ApplyBotList()
    {
        var chosen = new HashSet<string>(StringComparer.Ordinal);
        var path = Path.Combine(Root, BotListFile);
        foreach (var raw in File.ReadAllLines(path))
        {
            var line = raw.Split('#')[0].Trim();
            if (line.Length == 0)
            {
                continue;
            }

            var drop = line.StartsWith('-');
            var pattern = line.TrimStart('-').Trim();
            var hits = pattern.EndsWith("/*", StringComparison.Ordinal)
                ? Bots.Where(b => b.Pack == pattern[..^2]).Select(b => b.Key)
                : Bots.Where(b => b.Key == pattern).Select(b => b.Key);
            foreach (var key in hits.ToList())
            {
                if (drop)
                {
                    chosen.Remove(key);
                }
                else
                {
                    chosen.Add(key);
                }
            }
        }

        foreach (var bot in Bots)
        {
            bot.InList = chosen.Contains(bot.Key);
        }
    }
}

public sealed class DecompBot
{
    private static readonly JsonWriterOptions WriterOptions = new()
    {
        Indented = true,
        IndentSize = 1,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private string _lineEnding = "\n";
    private bool _finalNewline;

    private DecompBot(string path, JsonObject json)
    {
        FilePath = path;
        Json = json;
    }

    public string FilePath { get; }

    public JsonObject Json { get; }

    public string Key { get; set; } = "";

    public string Pack => Key.Split('/')[0];

    public bool InList { get; set; }

    /// <summary>The id the newest build gives this bot (bot_ids.json); null when that build has no id for it.</summary>
    public int? Id { get; set; }

    public bool Dirty { get; set; }

    public static readonly string[] Slots = ["head", "right_arm", "left_arm", "legs"];

    public string Name
    {
        get => Json["name"]?.GetValue<string>() ?? "";
        set
        {
            Json["name"] = value;
            Dirty = true;
        }
    }

    public string Display => $"{(InList ? "●" : "○")} {BotLabels.Of(Id, Name, Pack)}{(Dirty ? " *" : "")}";

    // nome do item na lista para UI Automation (roteiros em tools/wpf_ui acham o bot pelo rotulo)
    public override string ToString() => Display;

    public JsonObject Part(string slot) => (JsonObject)Json["parts"]![slot]!;

    public JsonObject? Medachange => Json["medachange"] as JsonObject;

    public static DecompBot Load(string path, string? key)
    {
        var text = File.ReadAllText(path, Encoding.UTF8);
        var json = JsonNode.Parse(text) as JsonObject ?? throw new InvalidDataException($"{path}: not a JSON object");
        return new DecompBot(path, json)
        {
            Key = key ?? "",
            _lineEnding = text.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n",
            _finalNewline = text.EndsWith('\n'),
        };
    }

    public string Serialize()
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, WriterOptions))
        {
            Json.WriteTo(writer);
        }

        var text = Encoding.UTF8.GetString(stream.ToArray()).Replace("\r\n", "\n", StringComparison.Ordinal);
        if (_lineEnding != "\n")
        {
            text = text.Replace("\n", _lineEnding, StringComparison.Ordinal);
        }

        return _finalNewline ? text + _lineEnding : text;
    }

    public void Save()
    {
        File.WriteAllText(FilePath, Serialize(), new UTF8Encoding(false));
        Dirty = false;
    }
}
