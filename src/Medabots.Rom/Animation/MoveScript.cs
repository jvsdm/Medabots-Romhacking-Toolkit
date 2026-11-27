using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Medabots.Rom.Animation;

// One battle sequence script of the decomp project (data/anim/seq/NNN.json, tools/anim_seq.py): the show of a move,
// a list of commands with operands. Same rules as anim_seq.py (the generator refuses what Validate refuses).
//   {"seq": 2, "name": "RIFLE", "ops": [["actor_pose", 0, 13, 14, 15, 13, 15, 14], ["label", 128], ["end"]]}
//   {"seq": 23, "name": "...", "same_as": 21}   (the game points both skills at one script)
public sealed class MoveOp
{
    public string Name { get; set; } = "end";
    public List<int> Args { get; set; } = [];

    public string ArgsText
    {
        get => string.Join(", ", Args);
        set => Args = value.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
    }

    public MoveOp Clone() => new() { Name = Name, Args = [.. Args] };
}

public sealed class MoveScript
{
    // (name, operand count) per opcode: gBattleSeqCommands + gBattleSeqOperandCounts of the original ROM.
    public static readonly (string Name, int Operands)[] Commands =
    [
        ("nop", 0), ("end", 0), ("goto", 1), ("branch", 4), ("test_missed", 0), ("test_reaction", 0),
        ("test_nullified", 0), ("test_absorbed", 0), ("actor_pose", 7), ("yield", 0), ("target_pose", 7),
        ("actor_dash", 1), ("wait_pose", 1), ("bg_effect", 8), ("wait_frames", 1), ("sound", 5), ("leg_sound", 5),
        ("actor_move", 3), ("test_evaded", 0), ("bg_effect_fixed", 8), ("actor_home", 0), ("camera_track", 5),
        ("wait_camera", 0), ("load_effect_gfx", 1), ("spawn_effect", 8), ("wait_sprite_x", 2), ("clear_sprites", 2),
        ("play_anim", 5), ("camera_track_all", 5), ("bg0_wide", 0), ("bg_sweep", 8), ("bg_effect_at", 8),
        ("bg_effect_end", 0), ("actor_pose_loop", 7), ("target_pose_loop", 7), ("bg_sweep_stop", 0), ("trail", 7),
        ("sprite_motion", 3), ("sprite_flip", 3), ("palette_fade", 5), ("backup_bot_palettes", 0),
        ("spawn_effect_wave", 8), ("backup_palette", 2), ("clear_trails", 0), ("bg_effect_keep", 8),
        ("spawn_orbit", 8), ("spawn_effect_set", 4), ("load_effect_gfx_ewram", 1), ("spawn_effect_tiles", 8),
        ("set_tile_anim", 2), ("load_skill_gfx", 0), ("load_effect_gfx_at", 2), ("scale_sprite", 6),
        ("spawn_ring", 4), ("armor_panel", 2), ("wait_armor_panel", 0), ("bg_effect_clear", 0),
        ("bg_effect_noflip", 8), ("show_rolls", 0),
    ];

    // Labels (0x80-0xFF) the game can skip: only those whose gBattleSeqOperandCounts entry is 0 (skipping any other
    // label would skip that many entries). From the original ROM (tools/anim_seq.py safe_labels).
    public static IReadOnlyList<int> SafeLabels { get; private set; } =
    [
        0x80, 0x81, 0x82, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88, 0x89, 0x8a, 0x8b, 0x8c, 0x8d, 0x8e, 0x8f, 0x90, 0x91,
        0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99, 0x9a, 0x9b, 0x9e, 0x9f, 0xa0, 0xa1, 0xa5, 0xa6, 0xa7, 0xa8,
        0xab, 0xaf, 0xb0, 0xb1, 0xb2, 0xb5, 0xb6, 0xb7, 0xbb, 0xbf, 0xc0, 0xc3, 0xc5, 0xc7, 0xc8, 0xc9, 0xcd, 0xd1,
        0xd2, 0xd5, 0xd7, 0xd9, 0xda, 0xdb, 0xdc, 0xdf, 0xe3, 0xe4, 0xe5, 0xe6, 0xe9, 0xed, 0xee, 0xef, 0xf0, 0xf3,
        0xf7, 0xf8, 0xf9, 0xfa, 0xfd,
    ];

    public int Id { get; }
    public string FilePath { get; }
    public string Name { get; set; }
    public int? SameAs { get; private set; }
    public List<MoveOp> Ops { get; } = [];
    public bool Dirty { get; set; }

    private MoveScript(string path, JsonObject doc)
    {
        FilePath = path;
        Id = doc["seq"]!.GetValue<int>();
        Name = doc["name"]?.GetValue<string>() ?? "";
        SameAs = doc["same_as"]?.GetValue<int>();
        if (doc["ops"] is JsonArray ops)
            foreach (var o in ops)
            {
                var a = o!.AsArray();
                Ops.Add(new MoveOp { Name = a[0]!.GetValue<string>(), Args = a.Skip(1).Select(x => x!.GetValue<int>()).ToList() });
            }
    }

    public static MoveScript Load(string path) => new(path, (JsonObject)JsonNode.Parse(File.ReadAllText(path))!);

    public string Display => $"{Id:000}  {Name}" + (SameAs is { } s ? $"  (= {s:000})" : "");

    // Makes this script its own copy instead of pointing at another one ("same_as").
    public void Detach(MoveLibrary lib)
    {
        if (SameAs is null)
            return;
        var src = lib.Resolve(this);
        Ops.Clear();
        Ops.AddRange(src.Ops.Select(o => o.Clone()));
        SameAs = null;
        Dirty = true;
    }

    public static int OperandCount(string name) =>
        name == "label" ? 0 : Array.FindIndex(Commands, c => c.Name == name) is var i and >= 0 ? Commands[i].Operands : -1;

    // Problems that would stop the generator (empty = fine).
    public IEnumerable<string> Validate()
    {
        var labels = new HashSet<int>();
        var targets = new HashSet<int>();
        foreach (var (op, i) in Ops.Select((o, i) => (o, i + 1)))
        {
            if (op.Name == "label")
            {
                if (op.Args.Count != 1 || !SafeLabels.Contains(op.Args[0]))
                    yield return $"command {i}: label needs one value from {string.Join(", ", SafeLabels.Take(6))}... (only labels whose operand count is 0)";
                else
                    labels.Add(op.Args[0]);
                continue;
            }
            var n = OperandCount(op.Name);
            if (n < 0)
                yield return $"command {i}: unknown command '{op.Name}'";
            else if (op.Args.Count != n)
                yield return $"command {i}: {op.Name} takes {n} operands, has {op.Args.Count}";
            else if (op.Name == "goto")
                targets.Add(op.Args[0]);
            else if (op.Name == "branch")
                targets.UnionWith(op.Args);
        }
        foreach (var t in targets.Where(t => !labels.Contains(t)))
            yield return $"goto/branch to label {t}, which does not exist";
        if (SameAs is null && (Ops.Count == 0 || Ops[^1].Name != "end"))
            yield return "the script should end with 'end'";
    }

    public void Save()
    {
        var sb = new StringBuilder("{\n");
        sb.Append($" \"seq\": {Id},\n \"name\": {JsonSerializer.Serialize(Name)}");
        if (SameAs is { } s)
            sb.Append($",\n \"same_as\": {s}");
        else
        {
            sb.Append(",\n \"ops\": [\n");
            for (var i = 0; i < Ops.Count; i++)
            {
                var o = Ops[i];
                var items = new List<string> { JsonSerializer.Serialize(o.Name) };
                items.AddRange(o.Args.Select(a => a.ToString()));
                sb.Append("  [" + string.Join(", ", items) + "]" + (i < Ops.Count - 1 ? "," : "") + "\n");
            }
            sb.Append(" ]");
        }
        sb.Append("\n}\n");
        File.WriteAllText(FilePath, sb.ToString(), new UTF8Encoding(false));
        Dirty = false;
    }
}

public sealed class MoveLibrary
{
    public SortedDictionary<int, MoveScript> Scripts { get; } = [];
    public string Folder { get; private init; } = "";

    public static MoveLibrary Load(string projectRoot)
    {
        var dir = Path.Combine(projectRoot, "data", "anim", "seq");
        var lib = new MoveLibrary { Folder = dir };
        foreach (var f in Directory.GetFiles(dir, "*.json"))
        {
            var s = MoveScript.Load(f);
            lib.Scripts[s.Id] = s;
        }
        return lib;
    }

    public MoveScript Resolve(MoveScript s)
    {
        for (var guard = 0; s.SameAs is { } n && guard < 64; guard++)
            s = Scripts[n];
        return s;
    }

    // The game's table has 60-79 pointing at script 0 (files named unused_NN): those are the free slots; after them
    // come ids past the last script (255 is the most the u8 animSkill can hold).
    public int NextFreeId()
    {
        var free = Scripts.Values.FirstOrDefault(s => s.Id is >= 60 and < 80 && s.Name.StartsWith("unused_"));
        if (free is not null)
            return free.Id;
        var next = Scripts.Keys.Max() + 1;
        return next <= 254 ? next : throw new InvalidOperationException("no free move script id");
    }

    public MoveScript CreateCopy(MoveScript source, string name)
    {
        var id = NextFreeId();
        Scripts.Remove(id);
        var path = Path.Combine(Folder, $"{id:000}.json");
        File.WriteAllText(path, "{\n \"seq\": " + id + ",\n \"name\": \"" + name + "\",\n \"ops\": [\n  [\"end\"]\n ]\n}\n",
            new UTF8Encoding(false));
        var created = MoveScript.Load(path);
        created.Ops.Clear();
        created.Ops.AddRange(Resolve(source).Ops.Select(o => o.Clone()));
        created.Save();
        Scripts[id] = created;
        return created;
    }
}

// data/anim/pose_anims.json: line (operand of actor_pose / target_pose) -> pose script per leg column (0..6).
public sealed class PoseAnimTable
{
    public const int Columns = 7;
    public string FilePath { get; }
    public List<int[]> Rows { get; } = [];
    public bool Dirty { get; set; }
    private readonly JsonObject _doc;

    private PoseAnimTable(string path, JsonObject doc)
    {
        FilePath = path;
        _doc = doc;
        foreach (var r in doc["anims"]!.AsArray())
            Rows.Add(r!.AsArray().Select(x => x!.GetValue<int>()).ToArray());
    }

    public static PoseAnimTable Load(string projectRoot)
    {
        var path = Path.Combine(projectRoot, "data", "anim", "pose_anims.json");
        return new PoseAnimTable(path, (JsonObject)JsonNode.Parse(File.ReadAllText(path))!);
    }

    public int AddRow(int poseScript)
    {
        Rows.Add(Enumerable.Repeat(poseScript, Columns).ToArray());
        Dirty = true;
        return Rows.Count - 1;
    }

    public void Save()
    {
        var sb = new StringBuilder("{\n");
        sb.Append(" \"_doc\": " + JsonSerializer.Serialize(_doc["_doc"]?.GetValue<string>() ?? "") + ",\n \"anims\": [\n");
        for (var i = 0; i < Rows.Count; i++)
            sb.Append("  [" + string.Join(", ", Rows[i]) + "]" + (i < Rows.Count - 1 ? "," : "") + "\n");
        sb.Append(" ]\n}\n");
        File.WriteAllText(FilePath, sb.ToString(), new UTF8Encoding(false));
        Dirty = false;
    }
}

// Per-part animation overrides: "anim" (move script) and "emit" ([dx, dy]) of one part slot.
public sealed record PartMove(int? Anim, int? EmitX, int? EmitY);

// data/anim/base_bots.json: the same two fields for bots 0..119 (the base game has no JSON per bot).
public sealed class BaseBotMoves
{
    public static readonly string[] Slots = ["head", "right_arm", "left_arm", "legs"];
    public string FilePath { get; }
    private readonly JsonObject _doc;

    private BaseBotMoves(string path, JsonObject doc)
    {
        FilePath = path;
        _doc = doc;
    }

    public static BaseBotMoves Load(string projectRoot)
    {
        var path = Path.Combine(projectRoot, "data", "anim", "base_bots.json");
        var doc = File.Exists(path) ? (JsonObject)JsonNode.Parse(File.ReadAllText(path))! : new JsonObject();
        return new BaseBotMoves(path, doc);
    }

    public PartMove Get(int bot, string slot)
    {
        if (_doc[bot.ToString()] is not JsonObject b || b[slot] is not JsonObject p)
            return new PartMove(null, null, null);
        var emit = p["emit"] as JsonArray;
        return new PartMove(p["anim"]?.GetValue<int>(), emit?[0]?.GetValue<int>(), emit?[1]?.GetValue<int>());
    }

    public void Set(int bot, string slot, PartMove move)
    {
        var key = bot.ToString();
        var b = _doc[key] as JsonObject ?? new JsonObject();
        var p = new JsonObject();
        if (move.Anim is { } a)
            p["anim"] = a;
        if (move.EmitX is { } x && move.EmitY is { } y && (x != 0 || y != 0))
            p["emit"] = new JsonArray(x, y);
        if (p.Count == 0)
            b.Remove(slot);
        else
            b[slot] = p;
        if (b.Count == 0)
            _doc.Remove(key);
        else
            _doc[key] = b;
    }

    public void Save()
    {
        var sb = new StringBuilder("{\n");
        var doc = _doc.OrderBy(p => p.Key.StartsWith('_') ? -1 : int.TryParse(p.Key, out var n) ? n : 9999).ToList();
        for (var i = 0; i < doc.Count; i++)
            sb.Append($" \"{doc[i].Key}\": {doc[i].Value!.ToJsonString()}" + (i < doc.Count - 1 ? "," : "") + "\n");
        sb.Append("}\n");
        File.WriteAllText(FilePath, sb.ToString(), new UTF8Encoding(false));
    }
}
