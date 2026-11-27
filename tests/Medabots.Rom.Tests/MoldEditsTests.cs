using Medabots.Rom.Animation;
using Xunit;

namespace Medabots.Rom.Tests;

public sealed class MoldEditsTests
{
    private static MoldDesc Mold(byte child = 0, byte sibling = 0) => new() { Child = child, Sibling = sibling, Height = 16, Width = 16 };

    [Fact]
    public void CheckTree_AcceptsATree_RefusesLoopsTwoParentsAndMissingMolds()
    {
        var tree = new Dictionary<int, MoldDesc> { [1] = Mold(child: 2), [2] = Mold(sibling: 3), [3] = Mold() };
        Assert.Null(MoldEdits.CheckTree(tree));

        tree[3] = Mold(child: 1);
        Assert.Contains("twice", MoldEdits.CheckTree(tree));

        tree[3] = Mold(child: 2); // 2 would have two parents
        Assert.NotNull(MoldEdits.CheckTree(tree));

        tree[3] = Mold(child: 9);
        Assert.Contains("mold 9", MoldEdits.CheckTree(tree));
    }

    [Fact]
    public void LayerFields_OnlyMoveJointAndDepth()
    {
        var d = Mold(child: 7, sibling: 4);
        d.PivotV = 5;
        new MoldFields { JointDown = -6, JointRight = 2, Depth = 1, PivotDown = 99, Child = 0, Sibling = 0, Layer = true }.ApplyTo(d);
        Assert.Equal((-6, 2, 1), (d.JointV, d.JointH, d.Depth));
        Assert.Equal((5, 7, 4), (d.PivotV, d.Child, d.Sibling));
    }

    [Fact]
    public void SaveThenLoad_KeepsTheEditsAndTheDocKey()
    {
        var dir = Directory.CreateTempSubdirectory("molds");
        try
        {
            Directory.CreateDirectory(Path.Combine(dir.FullName, "data", "anim"));
            File.WriteAllText(MoldEdits.FileFor(dir.FullName), """{"_doc": "test", "core": {"8": {"joint": [-15, -1]}}}""");
            var edits = MoldEdits.Load(dir.FullName);
            Assert.Equal(-15, edits.Core[8].JointDown);
            Assert.Equal(int.MinValue, edits.Core[8].PivotDown); // filled from the ROM by ApplyTo

            edits.Core[8] = new MoldFields { JointDown = -15, JointRight = -1, PivotDown = 26, PivotRight = 15, Depth = -1 };
            edits.Core[3] = new MoldFields { JointDown = -6, Layer = true };
            edits.Navi[50] = new MoldFields { JointDown = -12, PivotDown = 60, PivotRight = 35 };
            edits.Save();

            var text = File.ReadAllText(edits.Path);
            Assert.Contains("\"_doc\": \"test\"", text);
            Assert.Contains("\"3\": {\"joint\": [-6, 0], \"depth\": 0}", text); // a layer has no pivot / links
            var back = MoldEdits.Load(dir.FullName);
            Assert.Equal((26, 15, -1), (back.Core[8].PivotDown, back.Core[8].PivotRight, back.Core[8].Depth));
            Assert.True(back.Core[3].Layer);
            Assert.Equal(-12, back.Navi[50].JointDown);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    [Fact]
    public void NaviBots_Side1_IsTheMirrorOfSide0()
    {
        // 2026-11-23: Navi parts with side-1 art (alt) on a swapped node fell apart on the right side (Solid Cottage,
        // Gemini, Discratch); the game now draws them as a true mirror, and so does the rig. Silhouettes, not colours.
        string? rom = null;
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null && rom is null; dir = dir.Parent)
        {
            var scratch = Path.Combine(dir.FullName, "build", "scratch");
            if (Directory.Exists(Path.Combine(dir.FullName, "data", "anim", "poses")) && Directory.Exists(scratch))
                rom = Directory.GetDirectories(scratch).Select(d => Path.Combine(d, "medabots_rokusho.gba"))
                    .FirstOrDefault(f => File.Exists(f) && File.Exists(Path.ChangeExtension(f, ".map")) &&
                                         File.ReadAllText(Path.ChangeExtension(f, ".map")).Contains("gNaviSpriteNodeDescs"));
        }
        if (rom is null)
            return; // only with a Navi build in the decomp project checkout
        var rig = new SkeletonRig(File.ReadAllBytes(rom), File.ReadAllText(Path.ChangeExtension(rom, ".map")));
        var layout = Path.ChangeExtension(rom, ".layout.json");
        var count = File.Exists(layout) &&
                    System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(layout))?["tinpet_id"] is { } t
            ? t.GetValue<int>()
            : 120; // bots 0..N-1 (the Tinpet and the Medachange forms come after)
        var scores = new List<double>();
        for (var b = 120; b < count; b++)
        {
            var lists = rig.PartLists(b, b, b, b);
            if (!lists.Any(l => l.Any(r => r.Navi)))
                continue;
            var m0 = Mask(rig, lists, 0).Select(p => (239 - p.X, p.Y)).ToHashSet();
            var m1 = Mask(rig, lists, 1);
            scores.Add(Enumerable.Range(-4, 9).Max(dx =>
            {
                var shifted = m0.Select(p => (p.Item1 + dx, p.Y)).ToHashSet();
                return (double)m1.Count(shifted.Contains) / m1.Union(shifted).Count();
            }));
        }
        Assert.NotEmpty(scores);
        scores.Sort();
        Assert.True(scores[0] >= 0.7, $"worst {scores[0]:0.00}");
        Assert.True(scores[scores.Count / 2] >= 0.95, $"median {scores[scores.Count / 2]:0.00}");

        static HashSet<(int X, int Y)> Mask(SkeletonRig rig, List<SpriteRef>[] lists, int side)
        {
            var (sprites, pals) = rig.Render(lists, new PoseState(), 120, 110, side: side);
            var buf = new uint[240 * 160];
            SkeletonRig.Rasterize(sprites, pals, buf, 240, 160);
            var set = new HashSet<(int, int)>();
            for (var i = 0; i < buf.Length; i++)
                if (buf[i] != 0)
                    set.Add((i % 240, i / 240));
            return set;
        }
    }

    [Fact]
    public void HeadLayerJoint_MovesTheWholeHead_OnABuiltRom()
    {
        // Same check as in mGBA (2026-11-22): head layer (node 3) joint -6 => Metabee's head pictures 6 px higher.
        string? rom = null;
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null && rom is null; dir = dir.Parent)
        {
            var scratch = Path.Combine(dir.FullName, "build", "scratch");
            if (Directory.Exists(Path.Combine(dir.FullName, "data", "anim", "poses")) && Directory.Exists(scratch))
                rom = Directory.GetDirectories(scratch).Select(d => Path.Combine(d, "medabots_rokusho.gba"))
                    .FirstOrDefault(f => File.Exists(f) && File.Exists(Path.ChangeExtension(f, ".map")));
        }
        if (rom is null)
            return; // only inside the decomp project checkout, with a build
        var rig = new SkeletonRig(File.ReadAllBytes(rom), File.ReadAllText(Path.ChangeExtension(rom, ".map")));
        var lists = rig.PartLists(43, 43, 43, 43);
        var before = rig.Render(lists, new PoseState(), 120, 112).Sprites.ToDictionary(s => s.Node, s => s.ScreenY);

        var head = MoldFields.From(rig.Core[3], layer: true);
        head.JointDown -= 6;
        head.ApplyTo(rig.Core[3]);
        Assert.Null(MoldEdits.CheckTree(rig.Core));
        var after = rig.Render(lists, new PoseState(), 120, 112).Sprites.ToDictionary(s => s.Node, s => s.ScreenY);

        foreach (var r in lists[0])
            Assert.Equal(before[r.Node] - 6, after[r.Node]);
        foreach (var r in lists[3])
            Assert.Equal(before[r.Node], after[r.Node]);

        // Side 1 swaps the arm pictures (load_sprite_tiles_by_table): the right arm shows the left arm's picture.
        var left = rig.Render(lists, new PoseState(), 60, 80).Sprites.First(s => s.Node == lists[2][0].Node);
        var right = rig.Render(lists, new PoseState(), 180, 80, side: 1).Sprites.First(s => s.Node == lists[1][0].Node);
        Assert.Equal(left.Pixels, right.Pixels);
    }

    [Fact]
    public void CustomAndUse_SaveThenLoad_RoundTrip()
    {
        var dir = Directory.CreateTempSubdirectory("molds-own");
        try
        {
            Directory.CreateDirectory(Path.Combine(dir.FullName, "data", "anim"));
            File.WriteAllText(MoldEdits.FileFor(dir.FullName),
                """{"core": {}, "navi": {}, "custom": {"1": {"base": "core:17", "joint": [4, -8]}}, "use": {"core/metabee": {"right_arm": {"17": 1}}}}""");
            var edits = MoldEdits.Load(dir.FullName);
            Assert.Equal("core:17", edits.Custom[1].Base);
            Assert.Equal(4, edits.Custom[1].Fields.JointDown);
            Assert.Equal(1, edits.Use["core/metabee"]["right_arm"][17]);
            Assert.Equal(2, edits.NextCustom());

            edits.Custom[1].Fields = new MoldFields { JointDown = 4, JointRight = -8, PivotDown = 18, PivotRight = 24, Depth = -10 };
            edits.Save();
            var back = MoldEdits.Load(dir.FullName);
            Assert.Equal((4, -8, 18, 24, -10), (back.Custom[1].Fields.JointDown, back.Custom[1].Fields.JointRight,
                back.Custom[1].Fields.PivotDown, back.Custom[1].Fields.PivotRight, back.Custom[1].Fields.Depth));
            Assert.Equal(1, back.Use["core/metabee"]["right_arm"][17]);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    [Fact]
    public void OwnMold_MovesOnlyThatBot_OnABuiltRom()
    {
        // Same check as in mGBA (2026-11-23): Metabee's own copy of mold 17 with the joint 12 px lower moves only its
        // right arm (nodes 17 and 18, the forearm hangs from it); another bot with mold 17 stays where it was.
        string? rom = null;
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null && rom is null; dir = dir.Parent)
        {
            var scratch = Path.Combine(dir.FullName, "build", "scratch");
            if (Directory.Exists(Path.Combine(dir.FullName, "data", "anim", "poses")) && Directory.Exists(scratch))
                rom = Directory.GetDirectories(scratch).Select(d => Path.Combine(d, "medabots_rokusho.gba"))
                    .FirstOrDefault(f => File.Exists(f) && File.Exists(Path.ChangeExtension(f, ".map")));
        }
        if (rom is null)
            return; // only inside the decomp project checkout, with a build
        var rig = new SkeletonRig(File.ReadAllBytes(rom), File.ReadAllText(Path.ChangeExtension(rom, ".map")));
        var other = Enumerable.Range(0, 120).First(b => b != 43 && rig.PartLists(b, b, b, b)[1].Any(r => r.Node == 17));
        var metabee = rig.PartLists(43, 43, 43, 43);
        var otherLists = rig.PartLists(other, other, other, other);
        var before = rig.Render(metabee, new PoseState(), 120, 112).Sprites.ToDictionary(s => s.Node, s => s.ScreenY);
        var otherBefore = rig.Render(otherLists, new PoseState(), 120, 112).Sprites.ToDictionary(s => s.Node, s => (s.ScreenX, s.ScreenY));

        var own = rig.Core[17].Clone();
        own.JointV += 12;
        rig.Customs[1] = own;
        rig.UseOverride[(1, 43, 17)] = 1;
        metabee = rig.PartLists(43, 43, 43, 43);
        Assert.Equal(1, metabee[1].First(r => r.Node == 17).Custom);
        var after = rig.Render(metabee, new PoseState(), 120, 112).Sprites.ToDictionary(s => s.Node, s => s.ScreenY);
        foreach (var r in metabee[1])
            Assert.Equal(before[r.Node] + 12, after[r.Node]);
        foreach (var r in metabee[0].Concat(metabee[2]).Concat(metabee[3]))
            Assert.Equal(before[r.Node], after[r.Node]);
        var otherAfter = rig.Render(rig.PartLists(other, other, other, other), new PoseState(), 120, 112).Sprites
            .ToDictionary(s => s.Node, s => (s.ScreenX, s.ScreenY));
        Assert.Equal(otherBefore, otherAfter);
    }
}
