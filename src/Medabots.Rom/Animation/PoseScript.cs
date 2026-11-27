using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Medabots.Rom.Animation;

// One battle pose script of the decomp project (data/anim/poses/NN.json, written by tools/anim_poses.py):
//   {"script": 7, "ends": [8, 16], "angles": {"core/07": [0, 4096]}, "offsets": {"core/07": [[0, 0], [2, -1]]}}
// ends[k] = frames keyframe k takes; angles = s16 per keyframe (0x10000 = full turn, + = clockwise on screen);
// offsets = [down, right] pixels per keyframe. "angles_from"/"offsets_from": N = the block of script N (the game
// shares blocks between scripts). Rows are mold node ids ("core/NN"); "navi/MM" rows only matter for Navi molds.
public sealed class PoseScript
{
    public int Id { get; }
    public string FilePath { get; }
    public List<int> Ends { get; } = [];
    public SortedDictionary<int, List<int>> Angles { get; } = [];
    public SortedDictionary<int, List<(int V, int H)>> Offsets { get; } = [];
    public int? AnglesFrom { get; set; }
    public int? OffsetsFrom { get; set; }
    public bool Dirty { get; set; }
    private JsonObject _doc;

    // Resolved owners of the shared blocks (set by PoseLibrary).
    public PoseScript? AngleOwner { get; internal set; }
    public PoseScript? OffsetOwner { get; internal set; }

    private PoseScript(string path, JsonObject doc)
    {
        FilePath = path;
        _doc = doc;
        Id = doc["script"]!.GetValue<int>();
        foreach (var e in doc["ends"]!.AsArray())
            Ends.Add(e!.GetValue<int>());
        if (doc["angles"] is JsonObject ang)
            foreach (var (k, v) in ang)
                if (k.StartsWith("core/"))
                    Angles[int.Parse(k[5..])] = v!.AsArray().Select(x => x!.GetValue<int>()).ToList();
        if (doc["offsets"] is JsonObject off)
            foreach (var (k, v) in off)
                if (k.StartsWith("core/"))
                    Offsets[int.Parse(k[5..])] = v!.AsArray()
                        .Select(p => (p![0]!.GetValue<int>(), p[1]!.GetValue<int>())).ToList();
        AnglesFrom = doc["angles_from"]?.GetValue<int>();
        OffsetsFrom = doc["offsets_from"]?.GetValue<int>();
    }

    public static PoseScript Load(string path) =>
        new(path, (JsonObject)JsonNode.Parse(File.ReadAllText(path))!);

    public int KeyCount => Ends.Count;

    public int Angle(int node, int key)
    {
        var src = AngleOwner ?? this;
        return src.Angles.TryGetValue(node, out var row) && key < row.Count ? row[key] : 0;
    }

    public (int V, int H) Offset(int node, int key)
    {
        var src = OffsetOwner ?? this;
        return src.Offsets.TryGetValue(node, out var row) && key < row.Count ? row[key] : (0, 0);
    }

    // Edits go to the script that owns the block (shared blocks change every script that uses them).
    public PoseScript SetAngle(int node, int key, int value)
    {
        var src = AngleOwner ?? this;
        if (!src.Angles.TryGetValue(node, out var row))
            src.Angles[node] = row = Enumerable.Repeat(0, KeyCount).ToList();
        row[key] = (short)value;
        src.Dirty = true;
        return src;
    }

    public PoseScript SetOffset(int node, int key, int v, int h)
    {
        var src = OffsetOwner ?? this;
        if (!src.Offsets.TryGetValue(node, out var row))
            src.Offsets[node] = row = Enumerable.Repeat((0, 0), KeyCount).ToList();
        row[key] = ((sbyte)v, (sbyte)h);
        src.Dirty = true;
        return src;
    }

    // Frames keyframe `key` takes (1..255).
    public void SetEnd(int key, int frames)
    {
        Ends[key] = Math.Clamp(frames, 1, 255);
        Dirty = true;
    }

    // A new script (next free id) with this one's keyframes, written to disk: the start of a new move's pose.
    // Shared blocks are copied, so the new script owns its rows.
    public static PoseScript CreateCopy(PoseLibrary lib, PoseScript source)
    {
        var id = lib.Scripts.Keys.Max() + 1;
        var path = Path.Combine(Path.GetDirectoryName(source.FilePath)!, $"{id:00}.json");
        var keys = source.KeyCount;
        var ang = new JsonObject();
        foreach (var n in (source.AngleOwner ?? source).Angles.Keys)
            ang[$"core/{n:00}"] = new JsonArray(Enumerable.Range(0, keys).Select(k => (JsonNode)source.Angle(n, k)).ToArray());
        var off = new JsonObject();
        foreach (var n in (source.OffsetOwner ?? source).Offsets.Keys)
            off[$"core/{n:00}"] = new JsonArray(Enumerable.Range(0, keys)
                .Select(k => (JsonNode)new JsonArray(source.Offset(n, k).V, source.Offset(n, k).H)).ToArray());
        var doc = new JsonObject
        {
            ["script"] = id,
            ["ends"] = new JsonArray(source.Ends.Select(e => (JsonNode)e).ToArray()),
            ["angles"] = ang,
            ["offsets"] = off,
        };
        File.WriteAllText(path, CompactJson(doc) + "\n", new UTF8Encoding(false));
        var created = Load(path);
        lib.Scripts[id] = created;
        return created;
    }

    public void Save()
    {
        _doc["ends"] = new JsonArray(Ends.Select(e => (JsonNode)e).ToArray());
        if (AnglesFrom is null && (_doc["angles"] is not null || Angles.Count > 0))
        {
            var ang = _doc["angles"] as JsonObject ?? new JsonObject();
            foreach (var (n, row) in Angles)
                ang[$"core/{n:00}"] = new JsonArray(row.Select(v => (JsonNode)v).ToArray());
            foreach (var k in ang.Select(p => p.Key).Where(k => k.StartsWith("core/") &&
                         Angles.TryGetValue(int.Parse(k[5..]), out var r) && r.All(v => v == 0)).ToList())
                ang.Remove(k);
            _doc["angles"] = ang;
        }
        if (OffsetsFrom is null && (_doc["offsets"] is not null || Offsets.Count > 0))
        {
            var off = _doc["offsets"] as JsonObject ?? new JsonObject();
            foreach (var (n, row) in Offsets)
                off[$"core/{n:00}"] = new JsonArray(row.Select(p => (JsonNode)new JsonArray(p.V, p.H)).ToArray());
            foreach (var k in off.Select(p => p.Key).Where(k => k.StartsWith("core/") &&
                         Offsets.TryGetValue(int.Parse(k[5..]), out var r) && r.All(p => p == (0, 0))).ToList())
                off.Remove(k);
            _doc["offsets"] = off;
        }
        File.WriteAllText(FilePath, CompactJson(_doc) + "\n", new UTF8Encoding(false));
        Dirty = false;
    }

    // Same layout as tools/anim_poses.py compact_json: one line per mold row; sorted rows.
    public static string CompactJson(JsonObject doc)
    {
        var opts = new JsonSerializerOptions { WriteIndented = false };
        var sb = new StringBuilder("{\n");
        var items = doc.ToList();
        for (var i = 0; i < items.Count; i++)
        {
            var (k, v) = items[i];
            var comma = i < items.Count - 1 ? "," : "";
            if (v is JsonObject obj)
            {
                sb.Append($" \"{k}\": {{\n");
                var rows = obj.OrderBy(p => p.Key, StringComparer.Ordinal).ToList();
                for (var j = 0; j < rows.Count; j++)
                    sb.Append($"  \"{rows[j].Key}\": {rows[j].Value!.ToJsonString(opts)}{(j < rows.Count - 1 ? "," : "")}\n");
                sb.Append($" }}{comma}\n");
            }
            else
                sb.Append($" \"{k}\": {Spaced(v!.ToJsonString(opts))}{comma}\n");
        }
        return sb.Append('}').ToString();
    }

    // json.dumps default separators (", ") for the top level values such as "ends".
    private static string Spaced(string json) => json.Replace(",", ", ");
}

public sealed class PoseLibrary
{
    public SortedDictionary<int, PoseScript> Scripts { get; } = [];

    public static PoseLibrary Load(string projectRoot)
    {
        var lib = new PoseLibrary();
        var dir = Path.Combine(projectRoot, "data", "anim", "poses");
        foreach (var f in Directory.GetFiles(dir, "*.json"))
        {
            var s = PoseScript.Load(f);
            lib.Scripts[s.Id] = s;
        }
        foreach (var s in lib.Scripts.Values)
        {
            s.AngleOwner = Resolve(lib, s, x => x.AnglesFrom);
            s.OffsetOwner = Resolve(lib, s, x => x.OffsetsFrom);
        }
        return lib;
    }

    private static PoseScript? Resolve(PoseLibrary lib, PoseScript s, Func<PoseScript, int?> from)
    {
        var cur = s;
        for (var guard = 0; from(cur) is { } n && guard < 64; guard++)
            cur = lib.Scripts[n];
        return ReferenceEquals(cur, s) ? null : cur;
    }

    public IEnumerable<PoseScript> SharingAngles(PoseScript owner) =>
        Scripts.Values.Where(s => (s.AngleOwner ?? s) == owner);
}
