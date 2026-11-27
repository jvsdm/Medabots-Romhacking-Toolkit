using System.Text.Json.Nodes;

namespace Medabots.Rom.Animation;

// pose_rows.json next to the built ROM (written by the build). The Navi molds 50+ reuse a game mold's movement rows:
// copy = node -> the row it takes its offsets (and its angle, unless turn says otherwise) from;
// still = nodes that stay 0 (no row 0 exists); turn = node -> two rows whose angles are summed (s16, with wrap).
public sealed class PoseRows
{
    public Dictionary<int, int> Copy { get; } = [];
    public Dictionary<int, (int A, int B)> Turn { get; } = [];

    // Null when the file does not exist.
    public static PoseRows? Load(string romPath)
    {
        var file = Path.Combine(Path.GetDirectoryName(romPath)!, "pose_rows.json");
        if (!File.Exists(file))
            return null;
        var doc = JsonNode.Parse(File.ReadAllText(file)) as JsonObject;
        var rows = new PoseRows();
        if (doc?["copy"] is JsonObject copy)
            foreach (var (k, v) in copy)
                rows.Copy[int.Parse(k)] = v!.GetValue<int>();
        if (doc?["still"] is JsonArray still)
            foreach (var n in still)
                rows.Copy[n!.GetValue<int>()] = 0;
        if (doc?["turn"] is JsonObject turn)
            foreach (var (k, v) in turn)
            {
                var pair = v!.AsArray();
                rows.Turn[int.Parse(k)] = (pair[0]!.GetValue<int>(), pair[1]!.GetValue<int>());
            }
        return rows;
    }
}
