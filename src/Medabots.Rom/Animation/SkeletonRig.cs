using System.Text.RegularExpressions;
using Medabots.Rom.Compression;

namespace Medabots.Rom.Animation;

// Battle Medabot sprite tree, read from a decomp build (ROM + linker .map). Port of the reference renderer
// tools/anim_render.py in the decomp project, which was checked against the game in mGBA (OAM positions and
// matrices of both sides, Navi and Core parts; the pose interpolation frame by frame).
//
// The decomp's C names X/Y the other way round (set_oam_sprite_shape_size takes the height first): in
// SpriteNodeDesc "width", "pivotX" and "jointX" are vertical. Here V = vertical (down +), H = horizontal (right +).

public sealed class MoldDesc
{
    public uint Frames;
    public int PivotV, PivotH;
    public sbyte JointV, JointH, Depth;
    public byte Sibling, Child, Palette, Height, Width, HeightDiv, WidthDiv;
    public bool Bpp8, HFlip, VFlip;
    public int AffineMode;
    public string? Layer; // gHeadPartLayer etc. for the four part layer nodes (IWRAM, filled at run time)

    public static MoldDesc Parse(byte[] rom, int o) => new()
    {
        Frames = BitConverter.ToUInt32(rom, o),
        PivotV = BitConverter.ToInt32(rom, o + 4),
        PivotH = BitConverter.ToInt32(rom, o + 8),
        JointV = (sbyte)rom[o + 12],
        JointH = (sbyte)rom[o + 13],
        Depth = (sbyte)rom[o + 14],
        Sibling = rom[o + 15],
        Child = rom[o + 16],
        Palette = rom[o + 17],
        Height = rom[o + 18],
        Width = rom[o + 19],
        HeightDiv = (byte)(rom[o + 20] & 15),
        WidthDiv = (byte)(rom[o + 20] >> 4),
        Bpp8 = (rom[o + 21] & 1) != 0,
        AffineMode = (rom[o + 21] >> 1) & 3,
        HFlip = ((rom[o + 21] >> 5) & 1) != 0,
        VFlip = ((rom[o + 21] >> 6) & 1) != 0,
    };

    public MoldDesc Clone() => (MoldDesc)MemberwiseClone();
}

// Custom = own mold of this bot (1..127, gCustomSpriteNodeDescs; EXP_CUSTOM_MOLDS, data/anim/molds.json "custom").
public readonly record struct SpriteRef(int Node, int Frame, bool Affine, bool Alt, bool Navi, int Lift, int Custom = 0)
{
    public static SpriteRef FromWord(uint v) => new((int)(v & 0x3F), (int)((v >> 6) & 0xFF), ((v >> 14) & 1) != 0,
        ((v >> 15) & 1) != 0, ((v >> 16) & 1) != 0, (sbyte)(v >> 24), (int)((v >> 17) & 0x7F));
}

// Per node: rotation (u16, 0x10000 = full turn) and offset in 8.8 fixed point, as the game keeps them.
public sealed class PoseState
{
    public const int Nodes = 0x40;
    public readonly int[] Rot = new int[Nodes];
    public readonly int[] OffV = new int[Nodes];
    public readonly int[] OffH = new int[Nodes];

    public PoseState Clone()
    {
        var p = new PoseState();
        Array.Copy(Rot, p.Rot, Nodes);
        Array.Copy(OffV, p.OffV, Nodes);
        Array.Copy(OffH, p.OffH, Nodes);
        return p;
    }
}

public sealed class RenderedSprite
{
    public int Node, X, Y, W, H, Z, Palette;
    public int JointX, JointY; // screen point the node hangs from (its joint after the parent's rotation)
    public bool Bpp8, Double, HFlip, VFlip;
    public short[]? Matrix; // pa pb pc pd (texture from screen), null = no affine
    public byte[]? Pixels;
    public int ScreenX => X >= 240 + 64 ? X - 512 : X;
    public int ScreenY => Y >= 160 + 32 ? Y - 256 : Y;
}

public sealed class SkeletonRig
{
    private const uint Gba = 0x08000000;
    private static readonly string[] PartTables =
        ["gHeadPartSpriteNodes", "gRightArmPartSpriteNodes", "gLeftArmPartSpriteNodes", "gLegPartSpriteNodes"];
    private static readonly Dictionary<string, int> LayerPart = new()
    {
        ["gHeadPartLayer"] = 0, ["gRightArmPartLayer"] = 1, ["gLeftArmPartLayer"] = 2, ["gLegPartLayer"] = 3,
    };

    public byte[] Rom { get; }
    public IReadOnlyDictionary<string, uint> Symbols { get; }
    public Dictionary<int, MoldDesc> Core { get; }
    public Dictionary<int, MoldDesc>? NaviMolds { get; }
    public HashSet<int> NaviOwn { get; } = []; // Navi molds with their own record (data/anim/molds.json "navi")
    // Own molds of one bot (gCustomSpriteNodeDescs of the build, then data/anim/molds.json "custom" on top).
    public Dictionary<int, MoldDesc> Customs { get; } = [];
    // (part 0..3, part id, node) -> own mold, for edits not built yet (data/anim/molds.json "use"); 0 = shared mold.
    public Dictionary<(int Part, int Id, int Node), int> UseOverride { get; } = [];
    public const int MaxCustom = 127;
    private readonly byte[] _mirror;

    public SkeletonRig(byte[] rom, string mapText)
    {
        Rom = rom;
        Symbols = ParseMap(mapText);
        Core = DescTable("gMedabotSpriteNodeDescs");
        NaviMolds = Symbols.ContainsKey("gNaviSpriteNodeDescs") ? DescTable("gNaviSpriteNodeDescs") : null;
        if (NaviMolds is not null)
        {
            // molds with a Navi record of their own (the rest of gNaviSpriteNodeDescs points at the core record);
            // the new molds 50+ are Navi records that the core table points at too
            int core = Off("gMedabotSpriteNodeDescs"), navi = Off("gNaviSpriteNodeDescs");
            foreach (var n in NaviMolds.Keys)
                if (n >= MoldEdits.CoreCount || U32(navi + 4 * n) != U32(core + 4 * n))
                    NaviOwn.Add(n);
        }
        if (Symbols.ContainsKey("gCustomSpriteNodeDescs"))
        {
            var t = Off("gCustomSpriteNodeDescs");
            for (var c = 1; c <= MaxCustom; c++)
            {
                var p = U32(t + 4 * c);
                if (p == 0)
                    continue;
                if (p >> 24 != 0x08 || p - Gba >= Rom.Length - 0x18)
                    break; // past the table
                Customs[c] = MoldDesc.Parse(Rom, (int)(p - Gba));
            }
        }
        var mirrorName = Symbols.ContainsKey("gMirroredPartNodeNavi") ? "gMirroredPartNodeNavi" : "gMirroredPartNode";
        _mirror = rom.AsSpan(Off(mirrorName), PoseState.Nodes).ToArray();
    }

    public static Dictionary<string, uint> ParseMap(string text)
    {
        var syms = new Dictionary<string, uint>();
        var re = new Regex(@"^\s*0x([0-9a-fA-F]{8})\s+(g\w+)", RegexOptions.Multiline);
        foreach (Match m in re.Matches(text))
            syms.TryAdd(m.Groups[2].Value, Convert.ToUInt32(m.Groups[1].Value, 16));
        return syms;
    }

    public int Off(string name) => (int)(Symbols[name] - Gba);
    private uint U32(int o) => BitConverter.ToUInt32(Rom, o);

    private Dictionary<int, MoldDesc> DescTable(string name)
    {
        var byAddr = new Dictionary<uint, string>();
        foreach (var (k, v) in Symbols)
            byAddr.TryAdd(v, k);
        var table = new Dictionary<int, MoldDesc>();
        var b = Off(name);
        for (var n = 1; n < PoseState.Nodes; n++)
        {
            var p = U32(b + 4 * n);
            if (p >> 24 == 0x08 && p - Gba < Rom.Length - 0x18)
                table[n] = MoldDesc.Parse(Rom, (int)(p - Gba));
            else if (p >> 24 == 0x03 && byAddr.TryGetValue(p, out var layer) && layer.EndsWith("PartLayer"))
            {
                var d = MoldDesc.Parse(Rom, Off(layer + "Template"));
                d.Layer = layer;
                table[n] = d;
            }
            else if (n > 1)
                break;
        }
        return table;
    }

    // Sprite lists (head, right arm, left arm, legs) for the given part ids.
    public List<SpriteRef>[] PartLists(int head, int rightArm, int leftArm, int legs)
    {
        int[] ids = [head, rightArm, leftArm, legs];
        var lists = new List<SpriteRef>[4];
        for (var k = 0; k < 4; k++)
            lists[k] = PartList(k, ids[k]);
        // side 1: the opposite arm's list of the same part (load_sprite_tiles_by_table swaps the arm pictures)
        _armSwap.AddOrUpdate(lists, [PartList(2, rightArm), PartList(1, leftArm)]);
        return lists;
    }

    private readonly System.Runtime.CompilerServices.ConditionalWeakTable<List<SpriteRef>[], List<SpriteRef>[]> _armSwap = new();

    private List<SpriteRef> PartList(int table, int id)
    {
        var stride = table == 3 ? 32 : 16;
        var o = Off(PartTables[table]) + id * stride;
        var list = new List<SpriteRef>();
        for (var i = 0; i < stride / 4; i++)
        {
            var r = SpriteRef.FromWord(U32(o + 4 * i));
            if (r.Node == 0)
                break;
            if (UseOverride.TryGetValue((table, id, r.Node), out var own))
                r = r with { Custom = own };
            list.Add(r);
        }
        return list;
    }

    // ---- game math -------------------------------------------------------------------------------------------

    // ObjAffineSet as mGBA's HLE BIOS: only the high byte of the angle counts.
    public static (short Pa, short Pb, short Pc, short Pd) AffineSet(int rotation)
    {
        var theta = ((rotation & 0xFFFF) >> 8) / 128.0 * Math.PI;
        var a = Math.Cos(theta);
        var b = Math.Sin(theta);
        return ((short)(int)(a * 256), (short)(int)(-b * 256), (short)(int)(b * 256), (short)(int)(a * 256));
    }

    private static int Div(int a, int b) => a / b; // C# division truncates toward zero like the BIOS Div
    private static int S16(int v) => (short)v;

    // One frame of interpolate_entity_positions for keyframe `key` at frame counter `frame`. Returns nodes done.
    // rows (optional, see PoseRows): node i takes its offset from rows.Copy[i] and its angle from rows.Turn[i]
    // (sum of two rows) or else from rows.Copy[i].
    public static int StepKey(PoseState s, PoseScript script, int key, int frame, PoseRows? rows = null)
    {
        var end = script.Ends[key];
        var done = 0;
        for (var i = 1; i < PoseState.Nodes; i++)
        {
            var src = rows is not null && rows.Copy.TryGetValue(i, out var r) ? r : i;
            int target;
            if (rows is not null && rows.Turn.TryGetValue(i, out var turn))
                target = ((script.Angle(turn.A, key) + script.Angle(turn.B, key) + 0x8000) & 0xFFFF) - 0x8000;
            else
                target = script.Angle(src, key);
            var (tv, th) = script.Offset(src, key);
            if (frame < end)
            {
                var rem = end - frame;
                var cur = S16(s.Rot[i]);
                int c0 = target - cur, c1 = target - (cur - 0x10000), c2 = target - (cur + 0x10000);
                int a0 = Math.Abs(c0), a1 = Math.Abs(c1), a2 = Math.Abs(c2);
                var delta = a0 > a1 ? (a1 > a2 ? c2 : c1) : (a0 > a2 ? c2 : c0);
                s.Rot[i] = (cur + Div(delta, rem)) & 0xFFFF;
                s.OffV[i] = S16(s.OffV[i] + Div((tv << 8) - s.OffV[i], rem));
                s.OffH[i] = S16(s.OffH[i] + Div((th << 8) - s.OffH[i], rem));
            }
            else
            {
                s.Rot[i] = target & 0xFFFF;
                s.OffV[i] = tv << 8;
                s.OffH[i] = th << 8;
                done++;
            }
        }
        return done;
    }

    // Every frame of a pose script (medabot_pose_anim_step until it returns 1), starting from `start`.
    public static List<(PoseState State, int Key)> PlayScript(PoseScript script, PoseState start,
        PoseRows? rows = null)
    {
        var frames = new List<(PoseState, int)>();
        var s = start.Clone();
        int key = 0, cnt = 0;
        while (key < script.Ends.Count && frames.Count < 4096)
        {
            cnt++;
            var k = key;
            if (StepKey(s, script, key, cnt, rows) != 0)
            {
                key++;
                cnt = 0;
            }
            frames.Add((s.Clone(), k));
        }
        return frames;
    }

    // setup_medabot_part_sprites + render_sprite_node_recursive: sprites in OAM order (first = in front).
    public (List<RenderedSprite> Sprites, Dictionary<int, ushort[]> Palettes) Render(List<SpriteRef>[] lists,
        PoseState pose, int x, int y, int depth = 200, int side = 0, bool noAffine = false)
    {
        var flat = lists.SelectMany(l => l).ToList();
        // Navi bots with side-1 art (alt) on a node the game swaps: on side 1 they are a true mirror (sprite_engine.c
        // navi_true_mirror): no node swap, no picture swap, arms with their side-0 art. Checked in mGBA (Solid Cottage).
        // Own molds on a swapped node too (EXP_CUSTOM_MOLDS: that geometry belongs to this bot only).
        var trueMirror = side != 0 && NaviMolds is not null &&
                         flat.Any(r => ((r.Navi && r.Alt) || r.Custom != 0) && _mirror[r.Node] != r.Node);
        var armNodes = trueMirror ? lists[1].Concat(lists[2]).Select(r => r.Node).ToHashSet() : [];
        Dictionary<int, MoldDesc> Tables(Dictionary<int, MoldDesc> src)
        {
            var t = new Dictionary<int, MoldDesc>(src);
            foreach (var (n, d) in Core)
            {
                if (d.Layer is null)
                    continue;
                var c = d.Clone();
                var lst = lists[LayerPart[d.Layer]];
                c.Child = (byte)(lst.Count > 0 ? lst[0].Node : 0);
                if (side != 0)
                    c.JointH = (sbyte)(-c.JointH - 2);
                t[n] = c;
            }
            return t;
        }
        var coreT = Tables(Core);
        var naviT = NaviMolds is null ? coreT : Tables(NaviMolds);
        SpriteRef? Entry(int node)
        {
            foreach (var r in flat)
                if (r.Node == (node & 0xFF))
                    return r;
            return null;
        }

        var palettes = new Dictionary<int, ushort[]>();
        var tiles = new Dictionary<int, byte[]>();
        foreach (var lst in lists)
            for (var i = 0; i < lst.Count; i++)
            {
                var r = lst[i];
                var d = r.Custom != 0 && Customs.TryGetValue(r.Custom, out var own) ? own : (r.Navi ? naviT : coreT)[r.Node];
                var fr = (r.Frame + (armNodes.Contains(r.Node) ? 0 : side & (r.Alt ? 1 : 0))) * 2;
                var ptrs = (int)(d.Frames - Gba);
                var gfx = U32(ptrs + 4 * fr);
                if (i == 0)
                {
                    var pp = U32(ptrs + 4 * (fr + 1));
                    if (pp != 0)
                    {
                        var pal = new ushort[16];
                        for (var k = 0; k < 16; k++)
                            pal[k] = BitConverter.ToUInt16(Rom, (int)(pp - Gba) + 2 * k);
                        palettes[d.Palette] = pal;
                    }
                }
                if (gfx != 0)
                    tiles[r.Node] = GfxBytes((int)(gfx - Gba));
            }
        // Side 1 (setup_medabot_part_sprites, slot != 0): each right-arm node gets the picture of entry k of the
        // part's LEFT-arm list, and the other way round (load_sprite_tiles_by_table). Skipped: alt entries of the
        // shown list, a Navi source with alt, and game molds of different sizes. Checked pixel by pixel in mGBA.
        if (side != 0 && !trueMirror && _armSwap.TryGetValue(lists, out var swap))
            for (var p = 0; p < 2; p++)
            {
                var dst = lists[1 + p];
                var src = swap[p];
                for (var k = 0; k < dst.Count && k < src.Count; k++)
                {
                    var s = src[k];
                    if (dst[k].Alt || (s.Navi && s.Alt) || !Core.TryGetValue(s.Node, out var a) ||
                        !Core.TryGetValue(dst[k].Node, out var b) || a.Height != b.Height || a.Width != b.Width)
                        continue;
                    var d = (s.Navi ? naviT : coreT)[s.Node];
                    var gfx = U32((int)(d.Frames - Gba) + 4 * (s.Frame * 2));
                    if (gfx != 0)
                        tiles[dst[k].Node] = GfxBytes((int)(gfx - Gba));
                }
            }

        var lift = 0;
        if (NaviMolds is not null)
            foreach (var r in flat)
                if (r.Navi && r.Lift != 0)
                {
                    lift = r.Lift;
                    break;
                }

        var outList = new List<RenderedSprite>();
        var visits = 0;
        void Rec(int node, int a, int b, int dep, int angle, bool flip)
        {
            if (++visits > 256) // a child/sibling loop in edited molds (MoldEdits.CheckTree refuses those)
                throw new InvalidOperationException("the mold tree has a loop");
            var table = coreT;
            if (NaviMolds is not null)
            {
                if (node == 1)
                    a -= lift << 8;
                var drawn = flip && !trueMirror && Entry(_mirror[node]) is not null ? _mirror[node] : node;
                var e0 = Entry(drawn);
                table = e0 is { Navi: true } ? naviT : coreT;
            }
            int oa = a, ob = b;
            var st = flip && !trueMirror ? _mirror[node] : node;
            var desc = Entry(st) is { Custom: > 0 } ce && Customs.TryGetValue(ce.Custom, out var cd) ? cd : table[st];
            int rot = pose.Rot[st], ov = pose.OffV[st], oh = pose.OffH[st];
            var z = Math.Max(0, dep + desc.Depth);
            var dv = desc.JointV + (ov >> 8);
            var dh = desc.JointH + (oh >> 8);
            var srcRot = -angle & 0xFFFF;
            var (pa, pb, pc, pd) = AffineSet(srcRot);
            if (flip)
                srcRot = -srcRot & 0xFFFF;
            a += pb * dh + pd * dv;
            b += flip ? -(pa * dh + pc * dv) : pa * dh + pc * dv;
            var e = Entry(node);
            var vis = !noAffine && (e?.Affine ?? true);
            if (e is not null)
            {
                short[]? mat = null;
                int ya, xb;
                if (vis)
                {
                    srcRot = flip ? (srcRot + rot) & 0xFFFF : (srcRot - rot) & 0xFFFF;
                    (pa, pb, pc, pd) = AffineSet(srcRot);
                    if (flip)
                    {
                        pa = (short)-pa;
                        pb = (short)-pb;
                    }
                    mat = [pa, pb, pc, pd];
                    int hh = desc.Height >> 1, hw = desc.Width >> 1;
                    int dv2 = desc.PivotV - hh, dh2 = desc.PivotH - hw;
                    ya = a - (pb * dh2 + pd * dv2 + (hh << 8));
                    xb = b - (pa * dh2 + pc * dv2 + (hw << 8));
                }
                else
                {
                    ya = a - (desc.PivotV << 8);
                    xb = b - ((flip ? desc.Width - desc.PivotH : desc.PivotH) << 8);
                }
                var sp = new RenderedSprite
                {
                    Node = node, Z = z, Matrix = mat, JointX = b >> 8, JointY = a >> 8, Y = (ya >> 8) & 0xFF, X = (xb >> 8) & 0x1FF,
                    H = desc.HeightDiv > 1 ? desc.Height / desc.HeightDiv : desc.Height,
                    W = desc.WidthDiv > 1 ? desc.Width / desc.WidthDiv : desc.Width,
                    Bpp8 = desc.Bpp8, Double = vis && desc.AffineMode == 3,
                    HFlip = desc.HFlip || (!vis && flip), VFlip = desc.VFlip, Palette = desc.Palette,
                    Pixels = tiles.GetValueOrDefault(node),
                };
                var k = outList.Count;
                while (k > 0 && outList[k - 1].Z > z)
                    k--;
                outList.Insert(k, sp);
            }
            if (table[node].Child != 0)
                Rec(table[node].Child, a, b, z, vis ? S16(rot + angle) : 0, flip);
            if (table[node].Sibling != 0)
                Rec(table[node].Sibling, oa, ob, dep, angle, flip);
        }
        Rec(1, y << 8, x << 8, depth, 0, side != 0);
        return (outList, palettes);
    }

    private byte[] GfxBytes(int o)
    {
        var hdr = U32(o);
        if (((hdr >> 4) & 0xF) == 1)
            return GbaLz77.Decompress(Rom, o) ?? [];
        var size = (int)(hdr >> 8);
        return Rom.AsSpan(o, Math.Min(size, Rom.Length - o)).ToArray();
    }

    // Draws the sprites into a 32-bit BGRA buffer (GBA OBJ rules: affine sprites sample around their centre).
    public static void Rasterize(List<RenderedSprite> sprites, Dictionary<int, ushort[]> palettes, uint[] buffer,
        int width, int height, int originX = 0, int originY = 0)
    {
        for (var si = sprites.Count - 1; si >= 0; si--)
        {
            var sp = sprites[si];
            var pal = palettes.GetValueOrDefault(sp.Palette);
            int x0 = sp.ScreenX + originX, y0 = sp.ScreenY + originY;
            if (sp.Matrix is { } m)
            {
                int bw = sp.Double ? sp.W * 2 : sp.W, bh = sp.Double ? sp.H * 2 : sp.H;
                for (var yy = 0; yy < bh; yy++)
                    for (var xx = 0; xx < bw; xx++)
                    {
                        int px = x0 + xx, py = y0 + yy;
                        if (px < 0 || py < 0 || px >= width || py >= height)
                            continue;
                        int rx = xx - bw / 2, ry = yy - bh / 2;
                        var tx = ((m[0] * rx + m[1] * ry) >> 8) + sp.W / 2;
                        var ty = ((m[2] * rx + m[3] * ry) >> 8) + sp.H / 2;
                        if (tx >= 0 && ty >= 0 && tx < sp.W && ty < sp.H)
                            Plot(buffer, py * width + px, Texel(sp, tx, ty), pal);
                    }
            }
            else
                for (var yy = 0; yy < sp.H; yy++)
                    for (var xx = 0; xx < sp.W; xx++)
                    {
                        int px = x0 + xx, py = y0 + yy;
                        if (px < 0 || py < 0 || px >= width || py >= height)
                            continue;
                        var tx = sp.HFlip ? sp.W - 1 - xx : xx;
                        var ty = sp.VFlip ? sp.H - 1 - yy : yy;
                        Plot(buffer, py * width + px, Texel(sp, tx, ty), pal);
                    }
        }
    }

    private static void Plot(uint[] buffer, int at, int c, ushort[]? pal)
    {
        if (c == 0)
            return;
        var v = pal is not null && c < 16 ? pal[c] : (ushort)0x7FFF;
        uint r = (uint)((v & 31) * 255 / 31), g = (uint)(((v >> 5) & 31) * 255 / 31), b = (uint)(((v >> 10) & 31) * 255 / 31);
        buffer[at] = 0xFF000000 | (r << 16) | (g << 8) | b;
    }

    private static int Texel(RenderedSprite sp, int tx, int ty)
    {
        if (sp.Pixels is null)
            return 0;
        var t = ty / 8 * (sp.W / 8) + tx / 8;
        if (sp.Bpp8)
        {
            var o8 = t * 64 + ty % 8 * 8 + tx % 8;
            return o8 < sp.Pixels.Length ? sp.Pixels[o8] : 0;
        }
        var o = t * 32 + ty % 8 * 4 + tx % 8 / 2;
        if (o >= sp.Pixels.Length)
            return 0;
        var v = sp.Pixels[o];
        return (tx & 1) != 0 ? v >> 4 : v & 15;
    }
}
