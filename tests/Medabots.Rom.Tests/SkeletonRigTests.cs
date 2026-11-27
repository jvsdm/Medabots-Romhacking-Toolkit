using System.Text.Json;
using Medabots.Rom.Animation;
using Xunit;

namespace Medabots.Rom.Tests;

public sealed class SkeletonRigTests
{
    // The decomp project (Medabots repo) this toolkit checkout lives in, if any: the folder with data/anim/poses.
    private static string? FindProject()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
            if (Directory.Exists(Path.Combine(dir.FullName, "data", "anim", "poses")))
                return dir.FullName;
        return null;
    }

    [Fact]
    public void StepKey_ReachesTheKeyframeOnItsLastFrame_TakingTheShortWayRound()
    {
        var dir = Directory.CreateTempSubdirectory("pose-step");
        try
        {
            var path = Path.Combine(dir.FullName, "00.json");
            File.WriteAllText(path, """{"script": 0, "ends": [4], "angles": {"core/02": [-30000]}, "offsets": {"core/02": [[8, -4]]}}""");
            var script = PoseScript.Load(path);
            var start = new PoseState();
            start.Rot[2] = 30000; // shortest way from 30000 to -30000 goes up through 0x8000
            var frames = SkeletonRig.PlayScript(script, start);

            Assert.Equal(4, frames.Count);
            Assert.Equal(30000 + Div(5536, 3), frames[0].State.Rot[2]);
            Assert.Equal(-30000 & 0xFFFF, frames[^1].State.Rot[2]);
            Assert.Equal(8 << 8, frames[^1].State.OffV[2]);
            Assert.Equal(-4 << 8, frames[^1].State.OffH[2]);
        }
        finally
        {
            dir.Delete(recursive: true);
        }

        static int Div(int a, int b) => a / b;
    }

    [Fact]
    public void PlayScript_RowFrom_CopiesAnotherNodesRow_AndStillNodesStayZero()
    {
        var dir = Directory.CreateTempSubdirectory("pose-rows");
        try
        {
            var path = Path.Combine(dir.FullName, "00.json");
            File.WriteAllText(path, """{"script": 0, "ends": [4], "angles": {"core/19": [4096]}, "offsets": {"core/19": [[2, -3]]}}""");
            var script = PoseScript.Load(path);
            var rows = new PoseRows();
            rows.Copy[52] = 19;
            rows.Copy[57] = 0;

            var withRows = SkeletonRig.PlayScript(script, new PoseState(), rows);
            var last = withRows[^1].State;
            Assert.Equal(4096, last.Rot[52]);
            Assert.Equal(2 << 8, last.OffV[52]);
            Assert.Equal(-3 << 8, last.OffH[52]);
            Assert.Equal(0, last.Rot[57]);

            var without = SkeletonRig.PlayScript(script, new PoseState());
            Assert.Equal(0, without[^1].State.Rot[52]);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    [Fact]
    public void PlayScript_TurnRows_SumTwoAnglesWithWrap_OffsetStillFromCopy()
    {
        var dir = Directory.CreateTempSubdirectory("pose-turn");
        try
        {
            var path = Path.Combine(dir.FullName, "00.json");
            File.WriteAllText(path, """{"script": 0, "ends": [4], "angles": {"core/17": [30000], "core/18": [10000]}}""");
            var script = PoseScript.Load(path);
            var rows = new PoseRows();
            rows.Turn[50] = (17, 18);

            var frames = SkeletonRig.PlayScript(script, new PoseState(), rows);
            Assert.Equal((((30000 + 10000 + 0x8000) & 0xFFFF) - 0x8000) & 0xFFFF, frames[^1].State.Rot[50]);
            Assert.Equal(40000, frames[^1].State.Rot[50]); // 40000 is already in 0..0xFFFF, so no wrap to -25536
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    [Fact]
    public void PoseLibrary_SaveWithoutEdits_KeepsEveryFileTheSame()
    {
        var project = FindProject();
        if (project is null)
            return; // only inside the decomp project checkout
        var src = Path.Combine(project, "data", "anim", "poses");
        var dir = Directory.CreateTempSubdirectory("pose-roundtrip");
        try
        {
            var poses = Path.Combine(dir.FullName, "data", "anim", "poses");
            Directory.CreateDirectory(poses);
            foreach (var f in Directory.GetFiles(src, "*.json"))
                File.Copy(f, Path.Combine(poses, Path.GetFileName(f)));
            var lib = PoseLibrary.Load(dir.FullName);
            Assert.Equal(59, lib.Scripts.Count);
            foreach (var s in lib.Scripts.Values)
                s.Save();
            foreach (var f in Directory.GetFiles(src, "*.json"))
                Assert.Equal(File.ReadAllText(f).Replace("\r\n", "\n"),
                    File.ReadAllText(Path.Combine(poses, Path.GetFileName(f)))); // the repo keeps LF (autocrlf checkouts have CRLF)

            // an edit lands in the script that owns the block and survives a reload
            // (the game itself shares no block; this extra script borrows script 7's)
            var ends = string.Join(", ", lib.Scripts[7].Ends);
            File.WriteAllText(Path.Combine(poses, "90.json"),
                $$"""{"script": 90, "ends": [{{ends}}], "angles_from": 7, "offsets_from": 7}""");
            lib = PoseLibrary.Load(dir.FullName);
            var shared = lib.Scripts[90];
            var owner = shared.SetAngle(7, 0, 4096);
            Assert.Same(lib.Scripts[7], owner);
            owner.Save();
            var reloaded = PoseLibrary.Load(dir.FullName);
            Assert.Equal(4096, reloaded.Scripts[90].Angle(7, 0));
            Assert.Equal(4096, reloaded.Scripts[7].Angle(7, 0));
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    [Fact]
    public void PoseScript_EndsEditAndCopy_RoundTrip()
    {
        var dir = Directory.CreateTempSubdirectory("pose-copy");
        try
        {
            var poses = Path.Combine(dir.FullName, "data", "anim", "poses");
            Directory.CreateDirectory(poses);
            File.WriteAllText(Path.Combine(poses, "00.json"),
                """{"script": 0, "ends": [4, 6], "angles": {"core/02": [100, -200]}, "offsets": {"core/02": [[1, 2], [3, 4]]}}""");
            var lib = PoseLibrary.Load(dir.FullName);
            var s = lib.Scripts[0];
            s.SetEnd(1, 9);
            s.Save();
            var copy = PoseScript.CreateCopy(lib, s);

            var again = PoseLibrary.Load(dir.FullName);
            Assert.Equal([4, 9], again.Scripts[0].Ends);
            Assert.Equal(1, copy.Id);
            Assert.Equal([4, 9], again.Scripts[1].Ends);
            Assert.Equal(-200, again.Scripts[1].Angle(2, 1));
            Assert.Equal((3, 4), again.Scripts[1].Offset(2, 1));
            Assert.Equal(10, SkeletonRig.PlayScript(again.Scripts[1], new PoseState()).Count - 3);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }


    [Fact]
    public void MoveLibrary_SaveWithoutEdits_KeepsEveryFileTheSame_AndValidates()
    {
        var project = FindProject();
        if (project is null || !Directory.Exists(Path.Combine(project, "data", "anim", "seq")))
            return;
        var dir = Directory.CreateTempSubdirectory("move-roundtrip");
        try
        {
            var seq = Path.Combine(dir.FullName, "data", "anim", "seq");
            Directory.CreateDirectory(seq);
            foreach (var f in Directory.GetFiles(Path.Combine(project, "data", "anim", "seq"), "*.json"))
                File.Copy(f, Path.Combine(seq, Path.GetFileName(f)));
            var lib = MoveLibrary.Load(dir.FullName);
            Assert.Equal(132, lib.Scripts.Count);
            foreach (var s in lib.Scripts.Values)
            {
                Assert.Empty(s.Validate());
                s.Save();
            }
            foreach (var f in Directory.GetFiles(Path.Combine(project, "data", "anim", "seq"), "*.json"))
                Assert.Equal(File.ReadAllText(f).Replace("\r\n", "\n"),
                    File.ReadAllText(Path.Combine(seq, Path.GetFileName(f))));

            var copy = lib.CreateCopy(lib.Scripts[2], "NEW_SHOT");
            Assert.Equal(60, copy.Id);
            Assert.Equal(lib.Scripts[2].Ops.Count, copy.Ops.Count);
            copy.Ops[0].Args[1] = 47;
            copy.Ops.Add(new MoveOp { Name = "goto", Args = [200] });
            Assert.Contains(copy.Validate(), p => p.Contains("label 200"));
            Assert.Equal(61, lib.NextFreeId());
            Assert.Equal(132, lib.Scripts.Count);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }


    [Fact]
    public void BaseBotMovesAndPoseAnims_RoundTrip()
    {
        var dir = Directory.CreateTempSubdirectory("anim-tables");
        try
        {
            var anim = Path.Combine(dir.FullName, "data", "anim");
            Directory.CreateDirectory(anim);
            File.WriteAllText(Path.Combine(anim, "pose_anims.json"), """
                {"_doc": "x", "anims": [[1, 1, 2, 1, 1, 2, 2], [3, 3, 3, 3, 3, 3, 3]]}
                """);
            var table = PoseAnimTable.Load(dir.FullName);
            Assert.Equal(2, table.AddRow(9));
            table.Save();
            Assert.Equal(3, PoseAnimTable.Load(dir.FullName).Rows.Count);
            Assert.Equal(new[] { 9, 9, 9, 9, 9, 9, 9 }, PoseAnimTable.Load(dir.FullName).Rows[2]);

            var moves = BaseBotMoves.Load(dir.FullName); // file does not exist yet
            Assert.Null(moves.Get(43, "head").Anim);
            moves.Set(43, "right_arm", new PartMove(60, 0, 4));
            moves.Set(7, "head", new PartMove(null, 0, 0)); // nothing to store
            moves.Save();
            var again = BaseBotMoves.Load(dir.FullName);
            Assert.Equal(new PartMove(60, 0, 4), again.Get(43, "right_arm"));
            Assert.Null(again.Get(7, "head").Anim);
            again.Set(43, "right_arm", new PartMove(null, 0, 0)); // clearing removes the bot
            again.Save();
            Assert.DoesNotContain("43", File.ReadAllText(Path.Combine(anim, "base_bots.json")));
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    // Writes what the renderer computes for the Tinpet so tools/anim_render.py can be compared with it
    // (set POSE_PARITY_OUT=<file> and POSE_PARITY_ROM=<built rom with its .map>).
    [Fact]
    public void Render_WritesParityDump_WhenAsked()
    {
        var outPath = Environment.GetEnvironmentVariable("POSE_PARITY_OUT");
        var romPath = Environment.GetEnvironmentVariable("POSE_PARITY_ROM");
        var project = FindProject();
        if (outPath is null || romPath is null || project is null)
            return;
        var rig = new SkeletonRig(File.ReadAllBytes(romPath), File.ReadAllText(Path.ChangeExtension(romPath, ".map")));
        var lib = PoseLibrary.Load(project);
        var tinpet = int.Parse(Environment.GetEnvironmentVariable("POSE_PARITY_BOT") ?? "120");
        var lists = rig.PartLists(tinpet, tinpet, tinpet, tinpet);
        var dump = new List<object>();
        foreach (var id in new[] { 1, 4, 7, 28, 35, 47 })
            foreach (var side in new[] { 0, 1 })
            {
                var frames = SkeletonRig.PlayScript(lib.Scripts[id], new PoseState());
                for (var f = 0; f < frames.Count; f++)
                {
                    var (sprites, _) = rig.Render(lists, frames[f].State, 120, 112, side: side);
                    dump.Add(new
                    {
                        script = id, side, frame = f,
                        sprites = sprites.Select(s => new[] { s.Node, s.Y, s.X, s.W, s.H, s.Z }
                            .Concat(s.Matrix?.Select(m => (int)m) ?? []).ToArray()),
                    });
                }
            }
        File.WriteAllText(outPath, JsonSerializer.Serialize(dump));
    }
}
