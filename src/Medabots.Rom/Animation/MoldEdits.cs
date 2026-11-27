using System.Text;
using System.Text.Json.Nodes;

namespace Medabots.Rom.Animation;

// The editable fields of one mold, in screen terms: [down, right] like the pose offsets.
public sealed class MoldFields
{
    public int PivotDown, PivotRight, JointDown, JointRight, Depth, Sibling, Child;
    public bool Layer; // part layer node 3..6: only joint and depth are saved (the child is set in game)

    public static MoldFields From(MoldDesc d, bool layer) => new()
    {
        PivotDown = d.PivotV, PivotRight = d.PivotH, JointDown = d.JointV, JointRight = d.JointH, Depth = d.Depth,
        Sibling = d.Sibling, Child = d.Child, Layer = layer,
    };

    public void ApplyTo(MoldDesc d)
    {
        d.JointV = (sbyte)JointDown;
        d.JointH = (sbyte)JointRight;
        d.Depth = (sbyte)Depth;
        if (Layer)
            return;
        d.PivotV = PivotDown;
        d.PivotH = PivotRight;
        d.Sibling = (byte)Sibling;
        d.Child = (byte)Child;
    }
}

/// <summary>
/// data/anim/molds.json of a decomp project: the skeleton molds edited by hand, with absolute values
/// ({"core": {"7": {"joint": [-12, 1], ...}}, "navi": {...}}). The build (tools/anim_molds.py) writes the "core"
/// ones into the game's records and tools/decomp_navi.py the "navi" ones into the Navi records.
/// </summary>
// An own mold of one bot: a copy of a shared mold ("core:17" / "navi:52") with its own pivot, joint and depth.
public sealed class CustomMold
{
    public string Base = "core:7";
    public MoldFields Fields = new();
    public bool BaseNavi => Base.StartsWith("navi:");
    public int BaseMold => int.Parse(Base[5..]);
}

public sealed class MoldEdits
{
    public const int CoreCount = 50, Nodes = 64;
    public static readonly IReadOnlyDictionary<int, string> LayerNames = new Dictionary<int, string>
    {
        [3] = "head layer", [4] = "left arm layer", [5] = "right arm layer", [6] = "legs layer",
    };

    public string Path { get; }
    public SortedDictionary<int, MoldFields> Core { get; } = [];
    public SortedDictionary<int, MoldFields> Navi { get; } = [];
    // "custom": own molds (1..127); "use": bot key (bot_ids.json) -> part (head, right_arm, left_arm, legs) -> node -> own mold
    public SortedDictionary<int, CustomMold> Custom { get; } = [];
    public SortedDictionary<string, SortedDictionary<string, SortedDictionary<int, int>>> Use { get; } = new(StringComparer.Ordinal);
    public static readonly string[] PartKeys = ["head", "right_arm", "left_arm", "legs"];
    public bool Dirty { get; set; }
    private JsonObject _extra = []; // "_doc" and any other key, kept as they are

    private MoldEdits(string path) => Path = path;

    public static string FileFor(string projectRoot) => System.IO.Path.Combine(projectRoot, "data", "anim", "molds.json");

    public static MoldEdits Load(string projectRoot)
    {
        var edits = new MoldEdits(FileFor(projectRoot));
        if (!File.Exists(edits.Path))
            return edits;
        var doc = JsonNode.Parse(File.ReadAllText(edits.Path)) as JsonObject
                  ?? throw new InvalidDataException("molds.json is not an object");
        foreach (var (key, value) in doc)
        {
            if (key is "core" or "navi")
            {
                var into = key == "core" ? edits.Core : edits.Navi;
                foreach (var (mold, node) in value as JsonObject ?? [])
                {
                    var m = int.Parse(mold);
                    if (m is < 1 or >= Nodes || (key == "core" && m >= CoreCount))
                        throw new InvalidDataException($"molds.json {key} {m}: mold out of range");
                    into[m] = Parse(node as JsonObject ?? [], key == "core" && LayerNames.ContainsKey(m));
                }
            }
            else if (key == "custom")
                foreach (var (num, node) in value as JsonObject ?? [])
                {
                    var c = int.Parse(num);
                    var o = node as JsonObject ?? [];
                    var b = o["base"]?.GetValue<string>() ?? "";
                    if (c is < 1 or > SkeletonRig.MaxCustom || !(b.StartsWith("core:") || b.StartsWith("navi:")))
                        throw new InvalidDataException($"molds.json custom {num}: number 1..127 and base core:N / navi:N");
                    edits.Custom[c] = new CustomMold { Base = b, Fields = Parse(o, false) };
                }
            else if (key == "use")
                foreach (var (bot, parts) in value as JsonObject ?? [])
                {
                    var byPart = new SortedDictionary<string, SortedDictionary<int, int>>(StringComparer.Ordinal);
                    foreach (var (part, nodes) in parts as JsonObject ?? [])
                    {
                        var byNode = new SortedDictionary<int, int>();
                        foreach (var (n, c) in nodes as JsonObject ?? [])
                            byNode[int.Parse(n)] = c!.GetValue<int>();
                        byPart[part] = byNode;
                    }
                    edits.Use[bot] = byPart;
                }
            else
                edits._extra[key] = value?.DeepClone();
        }
        return edits;
    }

    // Fields missing in the file are filled in later from the ROM (Merge); here they stay int.MinValue.
    private static MoldFields Parse(JsonObject o, bool layer)
    {
        const int none = int.MinValue;
        int[] Pair(string k) => o[k] is JsonArray a ? [a[0]!.GetValue<int>(), a[1]!.GetValue<int>()] : [none, none];
        int One(string k) => o[k]?.GetValue<int>() ?? none;
        var p = Pair("pivot");
        var j = Pair("joint");
        return new MoldFields
        {
            PivotDown = p[0], PivotRight = p[1], JointDown = j[0], JointRight = j[1], Depth = One("depth"),
            Sibling = One("sibling"), Child = One("child"), Layer = layer,
        };
    }

    // A field the file leaves out takes the ROM's value.
    private static void Merge(MoldFields f, MoldDesc d)
    {
        var rom = MoldFields.From(d, f.Layer);
        if (f.PivotDown == int.MinValue) (f.PivotDown, f.PivotRight) = (rom.PivotDown, rom.PivotRight);
        if (f.JointDown == int.MinValue) (f.JointDown, f.JointRight) = (rom.JointDown, rom.JointRight);
        if (f.Depth == int.MinValue) f.Depth = rom.Depth;
        if (f.Sibling == int.MinValue) f.Sibling = rom.Sibling;
        if (f.Child == int.MinValue) f.Child = rom.Child;
    }

    // Puts every edit into the rig's tables (the built ROM may be older than the file). Returns what it skipped.
    public List<string> ApplyTo(SkeletonRig rig)
    {
        var skipped = new List<string>();
        foreach (var (m, f) in Core)
            if (rig.Core.TryGetValue(m, out var d))
            {
                Merge(f, d);
                f.ApplyTo(d);
            }
            else
                skipped.Add($"core {m}");
        foreach (var (m, f) in Navi)
            if (rig.NaviMolds is not null && rig.NaviOwn.Contains(m))
            {
                Merge(f, rig.NaviMolds[m]);
                f.ApplyTo(rig.NaviMolds[m]);
            }
            else
                skipped.Add($"navi {m}");
        return skipped;
    }

    /// <summary>
    /// Own molds into the rig: each one a copy of its base with the file's pivot / joint / depth, and the "use" of
    /// the bots of this build (botIds = bot_ids.json) as overrides of their part entries. Returns what it skipped.
    /// </summary>
    public List<string> ApplyCustom(SkeletonRig rig, IReadOnlyDictionary<string, int>? botIds)
    {
        var skipped = new List<string>();
        foreach (var (c, cm) in Custom)
        {
            var baseDesc = cm.BaseNavi ? rig.NaviMolds?.GetValueOrDefault(cm.BaseMold) : rig.Core.GetValueOrDefault(cm.BaseMold);
            if (baseDesc is null)
            {
                skipped.Add($"custom {c} ({cm.Base})");
                continue;
            }
            var d = baseDesc.Clone();
            Merge(cm.Fields, baseDesc);
            d.PivotV = cm.Fields.PivotDown;
            d.PivotH = cm.Fields.PivotRight;
            d.JointV = (sbyte)cm.Fields.JointDown;
            d.JointH = (sbyte)cm.Fields.JointRight;
            d.Depth = (sbyte)cm.Fields.Depth;
            rig.Customs[c] = d;
        }
        rig.UseOverride.Clear();
        foreach (var (bot, parts) in Use)
        {
            if (botIds is null || !botIds.TryGetValue(bot, out var id))
            {
                skipped.Add($"use {bot}");
                continue;
            }
            foreach (var (part, nodes) in parts)
                foreach (var (node, c) in nodes)
                    rig.UseOverride[(Array.IndexOf(PartKeys, part), id, node)] = c;
        }
        return skipped;
    }

    public int NextCustom()
    {
        for (var c = 1; c <= SkeletonRig.MaxCustom; c++)
            if (!Custom.ContainsKey(c))
                return c;
        throw new InvalidOperationException("all 127 own molds are taken");
    }

    /// <summary>
    /// Null when the table is a proper tree; otherwise why not. From every mold (part roots are set in game), the
    /// child/sibling links may not come back to a node already seen nor lead to a mold the table does not have.
    /// Same rule as tools/anim_molds.py check_tree.
    /// </summary>
    public static string? CheckTree(IReadOnlyDictionary<int, MoldDesc> table)
    {
        foreach (var start in table.Keys)
        {
            var seen = new HashSet<int>();
            var stack = new Stack<int>();
            stack.Push(start);
            while (stack.Count > 0)
            {
                var n = stack.Pop();
                if (!seen.Add(n))
                    return $"from mold {start}, node {n} is reached twice (a loop or two parents)";
                if (!table.TryGetValue(n, out var d))
                    return $"the tree reaches mold {n}, which this table does not have";
                if (d.Child != 0)
                    stack.Push(d.Child);
                if (d.Sibling != 0)
                    stack.Push(d.Sibling);
            }
        }
        return null;
    }

    public void Save()
    {
        var sb = new StringBuilder("{\n");
        foreach (var (key, value) in _extra)
            sb.Append($"  \"{key}\": {value?.ToJsonString() ?? "null"},\n");
        Section(sb, "core", Core);
        sb.Append(",\n");
        Section(sb, "navi", Navi);
        if (Custom.Count > 0 || Use.Count > 0)
        {
            sb.Append(",\n  \"custom\": {");
            var first = true;
            foreach (var (c, cm) in Custom)
            {
                sb.Append(first ? "\n" : ",\n");
                first = false;
                var f = cm.Fields;
                sb.Append($"    \"{c}\": {{\"base\": \"{cm.Base}\", \"joint\": [{f.JointDown}, {f.JointRight}], " +
                          $"\"pivot\": [{f.PivotDown}, {f.PivotRight}], \"depth\": {f.Depth}}}");
            }
            sb.Append(first ? "}" : "\n  }");
            sb.Append(",\n  \"use\": {");
            first = true;
            foreach (var (bot, parts) in Use)
            {
                sb.Append(first ? "\n" : ",\n");
                first = false;
                var ps = string.Join(", ", parts.Select(p =>
                    $"\"{p.Key}\": {{{string.Join(", ", p.Value.Select(n => $"\"{n.Key}\": {n.Value}"))}}}"));
                sb.Append($"    \"{bot}\": {{{ps}}}");
            }
            sb.Append(first ? "}" : "\n  }");
        }
        sb.Append("\n}\n");
        File.WriteAllText(Path, sb.ToString().Replace("\r\n", "\n"));
        Dirty = false;
    }

    private static void Section(StringBuilder sb, string name, SortedDictionary<int, MoldFields> molds)
    {
        sb.Append($"  \"{name}\": {{");
        var first = true;
        foreach (var (m, f) in molds)
        {
            sb.Append(first ? "\n" : ",\n");
            first = false;
            sb.Append($"    \"{m}\": {{\"joint\": [{f.JointDown}, {f.JointRight}], \"depth\": {f.Depth}");
            if (!f.Layer)
                sb.Append($", \"pivot\": [{f.PivotDown}, {f.PivotRight}], \"sibling\": {f.Sibling}, \"child\": {f.Child}");
            sb.Append('}');
        }
        sb.Append(first ? "}" : "\n  }");
    }
}
