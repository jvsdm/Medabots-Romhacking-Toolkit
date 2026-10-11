using System.Buffers.Binary;
using Medabots.Rom.Compression;
using Medabots.Rom.Layout;
using Medabots.Rom.Maps;

namespace Medabots.Rom.Expansion;

/// <summary>A Medarot Navi (Kuwagata, J) bot to add: its index in Navi's tables, a name and its leg type.</summary>
/// <remarks>With <paramref name="Medachange"/> the bot is the Medachange form alone: Navi's form row (molds 31-37, hung
/// from the legs joint) as its legs, no head or arms.</remarks>
public sealed record NaviBotRequest(int NaviId, string Name, string Model, string LegType, bool Medachange = false);

/// <summary>
/// Adds Medarot Navi bots to an expanded Metabee (E) ROM with Navi's own geometry. Both games use the same mold
/// system (see docs section 15), so a Navi piece goes into the Core mold of the same shape when there is one (heads
/// 7-10, two-piece arms, biped legs 19-23 -> 31-35, chain legs 25-29) and into a new mold (50+) otherwise (64x64
/// heads and arms, one-piece and long legs). New molds copy the closest Core mold's record (palette, motion rows),
/// with Navi's size and attachment points; the mold table, mirror map and motion scripts first grow to 64 molds.
/// Stats, mini sprite and movement style come from a Core bot whose legs have the same geometry.
/// </summary>
public static class NaviImporter
{
    public const string NaviHeader = "MEDAROTNVKWG";

    private const int GbaBase = 0x08000000;
    private const int NaviMolds = 0x658858;
    private static readonly (string Part, int Table, int RowSize)[] NaviRows =
        [("head", 0x4BFC40, 16), ("right_arm", 0x4C05C0, 16), ("left_arm", 0x4C0F40, 16), ("legs", 0x4C18C0, 32)];

    // Medachange forms: 32 B per bot right after the legs rows; bots without a form hold one filler entry (0x1818).
    private const int NaviMedachangeRows = 0x4C2BC0;

    // Metabee (E) code that loads the mirror map and the three motion script tables.
    private const int MirrorMapLiteral = 0x046654;
    private const int MotionAngleLiteral = 0x044B0C;
    private const int MotionOffsetLiteral = 0x044B10;
    private const int MotionDurationLiteral = 0x044AFC;
    private const int MotionScripts = 59;
    private const int OriginalMolds = 50;
    public const int MoldCapacity = 64;
    private const int OriginalMotionRows = 49;
    private const int MotionRows = 63;

    // Navi mold -> Core mold of the same shape and tree position.
    private static readonly Dictionary<int, int> SameShape = new()
    {
        [7] = 7, [8] = 8, [9] = 9, [10] = 10,
        [13] = 17, [14] = 18, [16] = 14, [17] = 15,        // right arm row: Core 17-19, left arm row: Core 14-16
        [19] = 31, [20] = 32, [21] = 33, [22] = 34, [23] = 35,
        [25] = 25, [26] = 26, [27] = 27, [28] = 28, [29] = 29,
    };

    // Navi molds with no Core counterpart -> Core mold whose record (palette) and motion rows they start from.
    private static readonly Dictionary<int, int> NewMoldAnalog = new()
    {
        [30] = 13, [11] = 11, [12] = 12,
        [15] = 19, [33] = 19, [18] = 16, [34] = 16,
        [24] = 30, [31] = 30, [32] = 30, [35] = 30, [36] = 30, [37] = 30,
    };

    private static readonly (int Right, int Left)[] MirrorPairs = [(15, 18), (33, 34)];

    /// <summary>Entries of Navi pieces carry this bit (unused by the game); the injected code then reads the Navi mold table.</summary>
    public const uint NaviEntryBit = 1u << 16;

    /// <summary>Written before the Navi mold table (64 pointers); inject/tools/build.py finds it.</summary>
    public static ReadOnlySpan<byte> NaviTableMarker => "MBXNAVIT"u8;

    // Navi's skeleton joint templates (head, right arm, left arm, legs): +0x0C..+0x0E offset of the limb.
    private static readonly int[] NaviJointTemplates = [0x4BFBE4, 0x4BFBFC, 0x4BFC14, 0x4BFC2C];

    // Core's first piece of each limb (children of the joints 3-6): head, right arm, left arm, legs.
    private static readonly int[] CoreLimbRoots = [7, 17, 14, 20];

    private static readonly Dictionary<string, int> LegTypeCode = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Flying"] = 100, ["Hover"] = 101, ["Multi-leg"] = 102, ["Bipedal"] = 103, ["Wheeled"] = 104, ["Tank"] = 105, ["Aquatic"] = 106,
    };

    public static BotAddResult Import(byte[] expandedRom, byte[] naviRom, IReadOnlyList<NaviBotRequest> bots)
    {
        ArgumentNullException.ThrowIfNull(expandedRom);
        ArgumentNullException.ThrowIfNull(naviRom);
        Require(System.Text.Encoding.ASCII.GetString(naviRom, 0xA0, 12) == NaviHeader, "Expected Medarot Navi - Kuwagata Version (J).");
        var navi = new NaviRom(naviRom);
        var log = new List<string>();

        // ---- 1. the bots themselves (names, stats from a Core bot with the same leg geometry) ----
        var layout = RomLayout.Resolve(expandedRom);
        var plans = bots.Select(b => (Request: b, Rows: navi.Rows(b.NaviId, b.Medachange))).ToList();
        var definitions = plans.Select(p => Definition(expandedRom, layout, p.Request, p.Rows)).ToList();
        var added = BotAdder.AddAll(expandedRom, definitions);
        var rom = added.Rom;
        log.AddRange(added.Log);

        // ---- 2. a Navi mold table: Navi's records (Navi geometry) at the Core IDs of the same shape, so Core motion
        //         rows still fit, and new IDs 50+ (shared with the Core table) for the shapes Core lacks ----
        var allocator = BotAdder.FreeSpace(ref rom);
        layout = RomLayout.Resolve(rom);
        var used = plans.SelectMany(p => p.Rows.SelectMany(r => r.Entries)).Select(e => (int)(e & 0x3F)).Distinct().OrderBy(m => m).ToList();
        var unknown = used.Where(m => !SameShape.ContainsKey(m) && !NewMoldAnalog.ContainsKey(m)).ToList();
        Require(unknown.Count == 0, $"Navi molds without a Core counterpart: {string.Join(", ", unknown)}.");
        var newIds = used.Where(m => !SameShape.ContainsKey(m)).ToList();
        Require(OriginalMolds + newIds.Count <= MoldCapacity, $"{newIds.Count} new molds do not fit in {MoldCapacity - OriginalMolds} free slots.");
        var newMolds = newIds.Select((m, i) => (Navi: m, Core: OriginalMolds + i)).ToDictionary(x => x.Navi, x => x.Core);
        var map = new Dictionary<int, int>(SameShape);
        foreach (var (naviMold, coreMold) in newMolds)
        {
            map[naviMold] = coreMold;
        }

        // The first piece of each limb hangs from a skeleton joint: Navi keeps the limb's offset in the joint, Core
        // in the piece (joints are zero), so the joint's offsets move into that piece.
        // The joint's child is set at run time to the first entry of the row, so that piece is the limb's root.
        var limbRoot = new Dictionary<int, int>();
        foreach (var (_, rows) in plans)
        {
            for (var part = 0; part < rows.Count; part++)
            {
                if (rows[part].Entries.Count > 0)
                {
                    limbRoot[(int)(rows[part].Entries[0] & 0x3F)] = part;
                }
            }
        }

        var innerToo = plans.SelectMany(p => p.Rows.SelectMany(r => r.Entries.Skip(1))).Select(e => (int)(e & 0x3F)).Where(limbRoot.ContainsKey).Distinct().ToList();
        Require(innerToo.Count == 0, $"Navi molds that are a limb root in one bot and an inner piece in another: {string.Join(", ", innerToo)}.");

        // A new mold borrows record (palette slot) and motion rows from a Core mold of the same role: the limb's Core
        // root when it starts a limb, else the analog piece.
        var analog = newIds.ToDictionary(m => m, m => limbRoot.TryGetValue(m, out var part) ? CoreLimbRoots[part] : NewMoldAnalog[m]);
        var moldTable = layout.CompositePreviewDescriptorPointerTableOffset;
        var coreTable = new byte[4 * MoldCapacity];
        rom.AsSpan(moldTable, 4 * OriginalMolds).CopyTo(coreTable);
        var naviTable = new byte[4 * MoldCapacity];
        var artLists = new Dictionary<int, int>();
        foreach (var naviMold in used)
        {
            var target = map[naviMold];
            var baseId = target < OriginalMolds ? target : analog[naviMold];
            var record = rom.AsSpan((int)U32(rom, moldTable + 4 * baseId) - GbaBase, 0x18).ToArray();
            var naviRecord = navi.Record(naviMold);
            naviRecord.AsSpan(0x04, 0x0B).CopyTo(record.AsSpan(0x04));        // pivot, offset and depth of the Navi
            if (limbRoot.TryGetValue(naviMold, out var part))
            {
                // Head and arm joints hang from the body root (mold 2), the legs joint from mold 1; Navi's mold 2 is
                // offset (Core's is zero: Navi bots lost their waist), so head and arms carry it too.
                var joint = navi.JointOffsets(part);
                var body = part < 3 ? navi.Record(2) : new byte[0x18];
                for (var k = 0; k < 3; k++)
                {
                    record[0x0C + k] = (byte)(sbyte)((sbyte)record[0x0C + k] + joint[k] + (sbyte)body[0x0C + k]);
                }
            }

            record[0x0F] = naviRecord[0x0F] == 0 ? (byte)0 : (byte)map[naviRecord[0x0F]];
            record[0x10] = naviRecord[0x10] == 0 ? (byte)0 : (byte)map[naviRecord[0x10]];
            record[0x12] = naviRecord[0x12];
            record[0x13] = naviRecord[0x13];
            var list = allocator.Take(256 * 8);
            rom.AsSpan(list, 256 * 8).Clear();
            WriteU32(record, 0, (uint)(GbaBase + list));
            artLists[target] = list;
            var at = allocator.Take(0x18);
            record.CopyTo(rom, at);
            WriteU32(naviTable, 4 * target, (uint)(GbaBase + at));
            if (target >= OriginalMolds)
            {
                WriteU32(coreTable, 4 * target, (uint)(GbaBase + at));
            }

            log.Add($"mold {target}: Navi mold {naviMold} ({record[0x12]}x{record[0x13]}){(limbRoot.ContainsKey(naviMold) ? ", limb root" : "")}");
        }

        for (var m = 0; m < MoldCapacity; m++)
        {
            if (U32(naviTable, 4 * m) == 0)
            {
                WriteU32(naviTable, 4 * m, U32(coreTable, 4 * m));        // skeleton (1-6) and anything else: Core's
            }
        }

        // A part with no piece (Medachange-only bots: no head or arms) still gets one entry, because the game reads the
        // first entry of every part: a blank 8x8 mold with an empty picture and no palette.
        var blank = 0;
        if (plans.Any(p => p.Rows.Any(r => r.Entries.Count == 0)))
        {
            blank = OriginalMolds + newIds.Count;
            Require(blank < MoldCapacity, "No free mold left for the blank piece.");
            var record = new byte[0x18];
            record[0x14] = 0x11;
            record[0x12] = record[0x13] = 8;
            var picture = GbaLz77.Compress(new byte[32]);
            var pictureAt = allocator.Take(picture.Length);
            picture.CopyTo(rom, pictureAt);
            var list = allocator.Take(256 * 8);
            for (var slot = 0; slot < 256; slot++)
            {
                WriteU32(rom, list + 8 * slot, (uint)(GbaBase + pictureAt));
                WriteU32(rom, list + 8 * slot + 4, 0);
            }

            WriteU32(record, 0, (uint)(GbaBase + list));
            var at = allocator.Take(0x18);
            record.CopyTo(rom, at);
            WriteU32(coreTable, 4 * blank, (uint)(GbaBase + at));
            WriteU32(naviTable, 4 * blank, (uint)(GbaBase + at));
            log.Add($"mold {blank}: blank piece for empty parts");
        }

        ExpandMolds(rom, layout, coreTable, newMolds, analog, allocator, log);
        var markerAt = allocator.Take(8 + naviTable.Length);
        NaviTableMarker.CopyTo(rom.AsSpan(markerAt));
        naviTable.CopyTo(rom, markerAt + 8);
        log.Add($"Navi mold table at 0x{markerAt + 8:X6} (marker MBXNAVIT); injected code picks it for entries with bit 16");

        // ---- 3. Navi's art into each bot's rows (own lists per Navi record; entries marked with bit 16) ----
        layout = RomLayout.Resolve(rom);
        var previews = new Dictionary<string, int>
        {
            ["head"] = layout.CompositePreviewHeadAppearanceTableOffset,
            ["right_arm"] = layout.CompositePreviewRightArmAppearanceTableOffset,
            ["left_arm"] = layout.CompositePreviewLeftArmAppearanceTableOffset,
            ["legs"] = layout.CompositePreviewLegsAppearanceTableOffset,
        };
        var rowSizes = new Dictionary<string, int> { ["head"] = 16, ["right_arm"] = 16, ["left_arm"] = 16, ["legs"] = 32 };
        var nextSelector = new Dictionary<int, int>();
        for (var b = 0; b < plans.Count; b++)
        {
            var id = added.BotId + b;
            foreach (var row in plans[b].Rows)
            {
                var rowAt = previews[row.Part] + id * rowSizes[row.Part];
                rom.AsSpan(rowAt, rowSizes[row.Part]).Clear();
                var slot = 0;
                if (row.Entries.Count == 0)
                {
                    WriteU32(rom, rowAt, (uint)blank | (1u << 6));
                }

                foreach (var entry in row.Entries)
                {
                    var target = map[(int)(entry & 0x3F)];
                    var pairs = navi.Art(entry);
                    var selector = nextSelector.GetValueOrDefault(target, 1);
                    Require(selector + pairs.Count <= 256, $"Mold {target} has no free image slot left.");
                    nextSelector[target] = selector + pairs.Count;
                    var list = artLists[target];
                    for (var pose = 0; pose < pairs.Count; pose++)
                    {
                        var (image, palette) = pairs[pose];
                        if (pose == 1 && (entry & 0x4000) == 0)
                        {
                            // Second picture = side 1. Navi draws it as is when bit 15 is set; Core always flips a
                            // non-rotating piece on side 1, so the picture is stored flipped to come out right.
                            var naviRecord = navi.Record((int)(entry & 0x3F));
                            image = FlipLz77Picture(image, naviRecord[0x13], naviRecord[0x12], log);
                        }

                        var imageAt = allocator.Take(image.Length);
                        image.CopyTo(rom, imageAt);
                        var paletteAt = imageAt;
                        if (palette is not null)
                        {
                            paletteAt = allocator.Take(palette.Length);
                            palette.CopyTo(rom, paletteAt);
                        }

                        WriteU32(rom, list + 8 * (selector + pose), (uint)(GbaBase + imageAt));
                        WriteU32(rom, list + 8 * (selector + pose) + 4, (uint)(GbaBase + paletteAt));
                    }

                    WriteU32(rom, rowAt + 4 * slot++, (entry & ~0x3Fu & ~(0xFFu << 6)) | (uint)target | (uint)(selector << 6) | NaviEntryBit);
                }
            }

            log.Add($"{plans[b].Request.Name}: bot {id}, Navi {plans[b].Request.NaviId} ({plans[b].Request.Model})");
        }

        // Unused slots of every Navi list repeat its first picture (never drawn, but valid data).
        foreach (var (_, list) in artLists)
        {
            var first = Enumerable.Range(0, 256).FirstOrDefault(slot => U32(rom, list + 8 * slot) != 0);
            for (var slot = 0; slot < 256; slot++)
            {
                if (U32(rom, list + 8 * slot) == 0)
                {
                    rom.AsSpan(list + 8 * first, 8).CopyTo(rom.AsSpan(list + 8 * slot));
                }
            }
        }

        // ---- 4. feet on Core's ground line: Navi bots are taller or shorter than Core's. The rest-pose bottom of each
        //         bot (all rows, joints at one point) is compared with the median of Core's bots; the difference (px,
        //         up) goes into bits 24-31 of the first legs entry, and the injected code lifts the whole bot by it.
        var geometry = new RestPose(rom, layout.CompositePreviewDescriptorPointerTableOffset, markerAt + 8,
            [previews["head"], previews["right_arm"], previews["left_arm"], previews["legs"]]);
        var ground = Median(Enumerable.Range(0, Math.Min(CoreBots, added.BotId)).Select(geometry.Bottom).OfType<int>());
        for (var b = 0; b < plans.Count; b++)
        {
            var id = added.BotId + b;
            var bottom = geometry.Bottom(id);
            var legsRow = previews["legs"] + id * 32;
            if (bottom is null || U32(rom, legsRow) == 0)
            {
                continue;
            }

            var lift = Math.Clamp(bottom.Value - ground, sbyte.MinValue, sbyte.MaxValue);
            WriteU32(rom, legsRow, (U32(rom, legsRow) & 0x00FFFFFFu) | ((uint)(byte)(sbyte)lift << 24));
            log.Add($"{plans[b].Request.Name}: feet at {bottom}, Core ground {ground}, lift {lift}");
        }

        log.Add($"free space used up to 0x{allocator.Next:X6}");
        return new BotAddResult(rom, added.BotId, log);
    }

    private const int CoreBots = 120;

    private static int Median(IEnumerable<int> values)
    {
        var sorted = values.OrderBy(v => v).ToList();
        Require(sorted.Count > 0, "No Core bot to measure the ground line.");
        return sorted[sorted.Count / 2];
    }

    // Rest pose of a bot as the game draws it with no motion: every limb hangs from the same point (Core's joints and
    // body root have no offset), each piece at parent + (+0x0C) with its pivot (+0x04) and height (+0x12).
    private sealed class RestPose(byte[] rom, int coreTable, int naviTable, int[] rows)
    {
        private static readonly int[] RowSizes = [16, 16, 16, 32];

        public int? Bottom(int bot)
        {
            int? best = null;
            for (var part = 0; part < rows.Length; part++)
            {
                var entries = new Dictionary<int, uint>();
                uint first = 0;
                for (var k = 0; k < RowSizes[part] / 4; k++)
                {
                    var entry = U32(rom, rows[part] + bot * RowSizes[part] + 4 * k);
                    if ((entry & 0x3F) == 0)
                    {
                        break;
                    }

                    first = first == 0 ? entry : first;
                    entries.TryAdd((int)(entry & 0x3F), entry);
                }

                if (first != 0)
                {
                    Walk((int)(first & 0x3F), 0, entries, ref best, 0);
                }
            }

            return best;
        }

        private void Walk(int mold, int parent, Dictionary<int, uint> entries, ref int? best, int depth)
        {
            if (depth > 16)
            {
                return;
            }

            var has = entries.TryGetValue(mold, out var entry);
            var table = has && (entry & NaviEntryBit) != 0 ? naviTable : coreTable;
            var record = (int)U32(rom, table + 4 * mold) - GbaBase;
            var at = parent + (sbyte)rom[record + 0x0C];
            if (has)
            {
                var image = (int)U32(rom, (int)U32(rom, record) - GbaBase + 8 * (int)((entry >> 6) & 0xFF)) - GbaBase;
                var lowest = LowestRow(GbaLz77.Decompress(rom, image), rom[record + 0x13], rom[record + 0x12]);
                if (lowest >= 0)
                {
                    var bottom = at - BinaryPrimitives.ReadInt32LittleEndian(rom.AsSpan(record + 4)) + lowest + 1;
                    best = best is null ? bottom : Math.Max(best.Value, bottom);
                }
            }

            if (rom[record + 0x10] != 0)
            {
                Walk(rom[record + 0x10], at, entries, ref best, depth + 1);
            }

            if (rom[record + 0x0F] != 0)
            {
                Walk(rom[record + 0x0F], parent, entries, ref best, depth + 1);
            }
        }

        // Lowest row with an opaque pixel of a 4bpp picture (1D tiles, row-major), or -1.
        private static int LowestRow(byte[]? pixels, int width, int height)
        {
            if (pixels is null || width < 8)
            {
                return -1;
            }

            var across = width / 8;
            for (var row = height - 1; row >= 0; row--)
            {
                for (var tile = 0; tile < across; tile++)
                {
                    var at = 32 * ((row / 8) * across + tile) + 4 * (row % 8);
                    if (at + 4 <= pixels.Length && (pixels[at] | pixels[at + 1] | pixels[at + 2] | pixels[at + 3]) != 0)
                    {
                        return row;
                    }
                }
            }

            return -1;
        }
    }

    // Mold table -> 64 entries, mirror map -> 64 entries (new arms swap), motion scripts -> 63 mold rows.
    // A 4bpp picture of width x height pixels (1D tiles, row-major), mirrored left-right; unchanged if the size differs.
    private static byte[] FlipLz77Picture(byte[] compressed, int width, int height, ICollection<string> log)
    {
        var pixels = GbaLz77.Decompress(compressed, 0);
        if (pixels is null || pixels.Length < width * height / 2 || width % 8 != 0 || height % 8 != 0)
        {
            log.Add($"side-1 picture not flipped ({width}x{height}, {pixels?.Length ?? 0} bytes)");
            return compressed;
        }

        var flipped = (byte[])pixels.Clone();
        int across = width / 8, down = height / 8;
        for (var ty = 0; ty < down; ty++)
        {
            for (var tx = 0; tx < across; tx++)
            {
                var from = 32 * (ty * across + tx);
                var to = 32 * (ty * across + across - 1 - tx);
                for (var row = 0; row < 8; row++)
                {
                    for (var b = 0; b < 4; b++)
                    {
                        var value = pixels[from + 4 * row + b];
                        flipped[to + 4 * row + 3 - b] = (byte)((value >> 4) | (value << 4));
                    }
                }
            }
        }

        return GbaLz77.Compress(flipped);
    }

    private static void ExpandMolds(byte[] rom, RomLayout layout, byte[] table, Dictionary<int, int> newMolds,
        Dictionary<int, int> analog, RomAllocator allocator, ICollection<string> log)
    {
        var oldTable = layout.CompositePreviewDescriptorPointerTableOffset;
        var tableAt = allocator.Take(table.Length);
        table.CopyTo(rom, tableAt);
        var references = MapExpander.Repoint(rom, oldTable, 4 * OriginalMolds, tableAt);
        Require(references > 0, "No code reference to the mold table was found.");

        var oldMirror = (int)U32(rom, MirrorMapLiteral) - GbaBase;
        var mirror = new byte[MoldCapacity];
        rom.AsSpan(oldMirror, OriginalMolds).CopyTo(mirror);
        for (var m = OriginalMolds; m < MoldCapacity; m++)
        {
            mirror[m] = (byte)m;
        }

        foreach (var (right, left) in MirrorPairs)
        {
            if (newMolds.TryGetValue(right, out var r) && newMolds.TryGetValue(left, out var l))
            {
                (mirror[r], mirror[l]) = ((byte)l, (byte)r);
            }
        }

        var mirrorAt = allocator.Take(MoldCapacity);
        mirror.CopyTo(rom, mirrorAt);
        WriteU32(rom, MirrorMapLiteral, (uint)(GbaBase + mirrorAt));

        // Each motion script: 49 rows (molds 1-49) of angle (s16) and offset (2 x s8) per keyframe; the player reads 63.
        var rowSource = newMolds.ToDictionary(x => x.Value, x => analog[x.Key]);
        int angles = (int)U32(rom, MotionAngleLiteral) - GbaBase, offsets = (int)U32(rom, MotionOffsetLiteral) - GbaBase;
        var durations = (int)U32(rom, MotionDurationLiteral) - GbaBase;
        var moved = new Dictionary<int, int>();
        for (var script = 0; script < MotionScripts; script++)
        {
            var c = (int)U32(rom, durations + 4 * script) - GbaBase;
            var keys = 0;
            while (rom[c + keys] != 0)
            {
                keys++;
            }

            if (keys == 0)
            {
                continue;
            }

            foreach (var tablePointers in new[] { angles, offsets })
            {
                var old = (int)U32(rom, tablePointers + 4 * script) - GbaBase;
                if (!moved.TryGetValue(old, out var at))
                {
                    var row = keys * 2;
                    var data = new byte[MotionRows * row];
                    rom.AsSpan(old, OriginalMotionRows * row).CopyTo(data);
                    foreach (var (mold, source) in rowSource)
                    {
                        rom.AsSpan(old + (source - 1) * row, row).CopyTo(data.AsSpan((mold - 1) * row));
                    }

                    at = allocator.Take(data.Length);
                    data.CopyTo(rom, at);
                    moved[old] = at;
                }

                WriteU32(rom, tablePointers + 4 * script, (uint)(GbaBase + at));
            }
        }

        log.Add($"molds: table 0x{tableAt:X6} ({references} references), mirror 0x{mirrorAt:X6}, {moved.Count} motion blocks with {MotionRows} rows");
    }

    private static BotDefinition Definition(byte[] rom, RomLayout layout, NaviBotRequest request, IReadOnlyList<NaviRow> rows)
    {
        var legs = rows.Single(r => r.Part == "legs").Entries.Select(e => (int)(e & 0x3F)).ToHashSet();
        var source = legs.All(m => m is >= 19 and <= 23) ? 42                       // Rokusho: biped tree 31-35
            : legs.All(m => m is >= 25 and <= 29) ? (request.LegType == "Tank" ? 93 : 28)   // Totalizer / Spidar: chain 25-29
            : request.LegType switch { "Flying" or "Aquatic" => 0, "Wheeled" or "Tank" => 77, _ => 13 }; // one piece: Noctobat / Armorparadeen / Churlybear
        var parts = new List<(string, byte[])>();
        string[] suffix = ["HEAD", "R.ARM", "L.ARM", "LEGS"];
        for (var kind = 0; kind < 4; kind++)
        {
            var stats = rom.AsSpan(layout.PartTableOffset + (source * 4 + kind) * 16, 16).ToArray();
            if (kind == 3 && LegTypeCode.TryGetValue(request.LegType, out var code))
            {
                stats[1] = (byte)code;
            }

            parts.Add(($"{request.Model}-{suffix[kind]}", stats));
        }

        return new BotDefinition { Name = request.Name, GraphicsFrom = source, Parts = parts };
    }

    private sealed record NaviRow(string Part, IReadOnlyList<uint> Entries);

    private sealed class NaviRom(byte[] data)
    {
        public byte[] Record(int mold) => data.AsSpan((int)U32(data, NaviMolds + 4 * mold) - GbaBase, 0x18).ToArray();

        public sbyte[] JointOffsets(int part) =>
            [(sbyte)data[NaviJointTemplates[part] + 0x0C], (sbyte)data[NaviJointTemplates[part] + 0x0D], (sbyte)data[NaviJointTemplates[part] + 0x0E]];

        // The bot's drawn entries: rows of 16 B (head, arms) and 32 B (legs, as in Core), only entries whose picture
        // is LZ77 (selector 0 entries carry no picture).
        public IReadOnlyList<NaviRow> Rows(int bot, bool medachange = false)
        {
            var rows = new List<NaviRow>();
            foreach (var (part, partTable, size) in NaviRows)
            {
                var entries = new List<uint>();
                var table = medachange ? (part == "legs" ? NaviMedachangeRows : -1) : partTable;
                for (var k = 0; table >= 0 && k < size / 4; k++)
                {
                    var entry = U32(data, table + bot * size + 4 * k);
                    if (entry != 0 && entry < 0x10000 && HasPicture(entry))
                    {
                        entries.Add(entry);
                    }
                }

                rows.Add(new NaviRow(part, entries));
            }

            return rows;
        }

        public IReadOnlyList<(byte[] Image, byte[]? Palette)> Art(uint entry)
        {
            var blobs = (int)U32(data, (int)U32(data, NaviMolds + 4 * (int)(entry & 0x3F)) - GbaBase) - GbaBase;
            var selector = (int)((entry >> 6) & 0xFF);
            var pairs = new List<(byte[], byte[]?)>();
            for (var pose = 0; pose <= (int)((entry >> 15) & 1); pose++)
            {
                var image = (int)U32(data, blobs + 8 * (selector + pose)) - GbaBase;
                var palette = (int)U32(data, blobs + 8 * (selector + pose) + 4) - GbaBase;
                var length = GbaLz77.TryGetEncodedLength(data, image) ?? throw new InvalidDataException($"Navi picture at 0x{image:X6} is not LZ77.");
                pairs.Add((data.AsSpan(image, length).ToArray(), palette == image ? null : data.AsSpan(palette, 32).ToArray()));
            }

            return pairs;
        }

        private bool HasPicture(uint entry)
        {
            var record = (int)U32(data, NaviMolds + 4 * (int)(entry & 0x3F)) - GbaBase;
            if (record < 0 || record >= data.Length)
            {
                return false;
            }

            var image = (long)U32(data, (int)U32(data, record) - GbaBase + 8 * (int)((entry >> 6) & 0xFF)) - GbaBase;
            return image >= 0 && image < data.Length && data[image] == 0x10;
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidDataException(message);
        }
    }

    private static uint U32(byte[] data, int offset) => BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(offset));

    private static void WriteU32(byte[] data, int offset, uint value) => BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(offset), value);
}
