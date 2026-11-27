using System.Buffers.Binary;
using Medabots.Rom.Layout;

namespace Medabots.Rom.Expansion;

public sealed record BotAddResult(byte[] Rom, int BotId, IReadOnlyList<string> Log);

/// <summary>
/// Appends one Medabot to an already expanded Metabee (E) ROM (made by <see cref="RomExpander"/> or
/// tools/build.py), keeping every existing bot, edit and save valid: the new bot takes the next ID,
/// whose table rows the expansion already reserved, and the bot-count constants move from N to N+1.
/// Graphics reuse an existing bot's drawings, optionally recoloured into new palettes; the mini sprite
/// is mapped onto the palettes already in the ROM so no other bot changes colour.
/// </summary>
public static class BotAdder
{
    /// <summary>Bots an expanded ROM can hold (IDs 0-235; Tinpet is fixed at 236/237).</summary>
    public const int MaxBots = ExpansionPatchSites.TinpetId;

    private const int GbaBase = 0x08000000;
    private const int MoveStyleLiteral = 0x044AF8;      // ldr pool of the movement style table (Metabee E code)
    private const int PartSize = 0x10;
    private const int SelectorMask = 0xFF;
    private const int SelectorSlots = 256;
    private const int OriginalSlots = 128;
    private const int PaletteSize = 0x20;

    private static readonly (string Name, int RowSize)[] PreviewKinds =
        [("head", 16), ("right_arm", 16), ("left_arm", 16), ("legs", 32)];

    public static BotAddResult Add(byte[] expandedRom, BotDefinition bot)
    {
        ArgumentNullException.ThrowIfNull(expandedRom);
        ArgumentNullException.ThrowIfNull(bot);
        Require(RomExpander.ReadHeader(expandedRom) == RomExpander.MetabeeEuropeHeader,
            "Adding bots needs an expanded Medabots: Metabee (Europe) ROM.");
        var rom = (byte[])expandedRom.Clone();
        var layout = RomLayout.Resolve(rom);
        Require(layout.TinpetId == ExpansionPatchSites.TinpetId,
            "This ROM is not expanded (Tinpet is not at 236): generate the expanded ROM first.");
        var n = layout.BotCount;
        var id = n;
        Require(id < ExpansionPatchSites.TinpetId, $"The ROM already has {n} bots, the maximum.");
        Require(bot.GraphicsFrom >= 0 && bot.GraphicsFrom < n, $"graphics_from {bot.GraphicsFrom} is not a bot of this ROM (0-{n - 1}).");
        Require(bot.Parts.Count == 4, "A bot needs head, right_arm, left_arm and legs.");
        var log = new List<string> { $"{bot.Name}: bot {id} (graphics from bot {bot.GraphicsFrom})" };
        var allocator = FreeSpace(ref rom);

        // ---- stats and names ----
        for (var kind = 0; kind < 4; kind++)
        {
            bot.Parts[kind].Stats.CopyTo(rom, layout.PartTableOffset + (id * 4 + kind) * PartSize);
        }

        int NewString(string text)
        {
            var encoded = EuropeanNameEncoding.Encode(text);
            var at = allocator.Take(encoded.Length, align: 1);
            encoded.CopyTo(rom, at);
            return GbaBase + at;
        }

        WriteU32(rom, layout.BotNameTableOffset + id * 4, (uint)NewString(bot.Name));
        for (var kind = 0; kind < 4; kind++)
        {
            WriteU32(rom, layout.PartNameTableOffset + (id * 4 + kind) * 4, (uint)NewString(bot.Parts[kind].Name));
        }

        // ---- graphics rows copied from the source bot ----
        var sprites = layout.CompositeBattleSpritePointerTableOffset;
        var families = layout.CompositeBattleSpritePaletteFamilyTableOffset;
        var moveStyles = (int)U32(rom, MoveStyleLiteral) - GbaBase;
        var previews = new Dictionary<string, int>
        {
            ["head"] = layout.CompositePreviewHeadAppearanceTableOffset,
            ["right_arm"] = layout.CompositePreviewRightArmAppearanceTableOffset,
            ["left_arm"] = layout.CompositePreviewLeftArmAppearanceTableOffset,
            ["legs"] = layout.CompositePreviewLegsAppearanceTableOffset,
        };
        var from = bot.GraphicsFrom;
        CopyRow(rom, sprites, 24, from, id);
        CopyRow(rom, families, 1, from, id);
        CopyRow(rom, moveStyles, 1, from, id);
        foreach (var (name, rowSize) in PreviewKinds)
        {
            CopyRow(rom, previews[name], rowSize, from, id);
        }

        if (bot.Art is { Count: > 0 } art)
        {
            ApplyArt(rom, layout, previews, sprites, families, id, art, allocator, log);
        }
        else if (bot.Recolor is { Count: > 0 } recolor)
        {
            RecolorPreviews(rom, layout, previews, id, recolor, allocator, log);
            RecolorMiniSprite(rom, sprites, families, id, recolor, allocator, log);
        }

        RaiseBotCount(rom, n, log);
        log.Add($"free space used: 0x{allocator.Start:X6}-0x{allocator.Next:X6}");
        return new BotAddResult(rom, id, log);
    }

    /// <summary>Adds the bots in order (consecutive IDs) to one ROM; BotId is the first new ID.</summary>
    public static BotAddResult AddAll(byte[] expandedRom, IReadOnlyList<BotDefinition> bots)
    {
        ArgumentNullException.ThrowIfNull(bots);
        Require(bots.Count > 0, "No bots to add.");
        var count = RomLayout.Resolve(expandedRom).BotCount;
        Require(count + bots.Count <= ExpansionPatchSites.TinpetId,
            $"The ROM has {count} bots: {bots.Count} more would pass the maximum of {ExpansionPatchSites.TinpetId}.");
        var rom = expandedRom;
        var log = new List<string>();
        var first = -1;
        foreach (var bot in bots)
        {
            var result = Add(rom, bot);
            rom = result.Rom;
            first = first < 0 ? result.BotId : first;
            log.AddRange(result.Log);
        }

        return new BotAddResult(rom, first, log);
    }

    private static void RecolorPreviews(byte[] rom, RomLayout layout, Dictionary<string, int> previews, int id,
        IReadOnlyDictionary<string, string> recolor, RomAllocator allocator, ICollection<string> log)
    {
        var map = ColorMap(recolor);
        var descriptors = layout.CompositePreviewDescriptorPointerTableOffset;
        var taken = UsedSelectors(rom, layout, previews);
        var newPalettes = new Dictionary<uint, uint>();
        foreach (var (name, rowSize) in PreviewKinds)
        {
            var rowAt = previews[name] + id * rowSize;
            for (var i = 0; i < rowSize / 4; i++)
            {
                var entry = U32(rom, rowAt + 4 * i);
                var mold = (int)(entry & 0x3F);
                if (mold == 0)
                {
                    continue;
                }

                var record = (int)U32(rom, descriptors + 4 * mold) - GbaBase;
                var blobs = (int)U32(rom, record) - GbaBase;
                var selectors = Selectors(entry);
                var pairs = selectors.Select(s => (Image: U32(rom, blobs + 8 * s), Palette: U32(rom, blobs + 8 * s + 4))).ToList();
                if (pairs.All(pair => pair.Image == pair.Palette))
                {
                    continue;
                }

                var used = taken.TryGetValue(mold, out var set) ? set : taken[mold] = [];
                var selector = Enumerable.Range(1, SelectorSlots - selectors.Count)
                    .FirstOrDefault(s => Enumerable.Range(s, selectors.Count).All(x => !used.Contains(x)));
                Require(selector > 0, $"Mold {mold} has no free image slot left.");
                if (selector + selectors.Count > OriginalSlots && blobs < 0x800000)
                {
                    blobs = WidenBlobList(rom, record, blobs, allocator);       // original 128-slot list
                }

                for (var k = 0; k < pairs.Count; k++)
                {
                    used.Add(selector + k);
                    var (image, palette) = pairs[k];
                    var newPalette = image;
                    if (palette != image)
                    {
                        if (!newPalettes.TryGetValue(palette, out newPalette))
                        {
                            var at = allocator.Take(PaletteSize);
                            for (var c = 0; c < 16; c++)
                            {
                                var color = BinaryPrimitives.ReadUInt16LittleEndian(rom.AsSpan((int)palette - GbaBase + 2 * c));
                                BinaryPrimitives.WriteUInt16LittleEndian(rom.AsSpan(at + 2 * c), (ushort)Map(map, color));
                            }

                            newPalette = (uint)(GbaBase + at);
                            newPalettes[palette] = newPalette;
                        }
                    }

                    WriteU32(rom, blobs + 8 * (selector + k), image);
                    WriteU32(rom, blobs + 8 * (selector + k) + 4, newPalette);
                }

                WriteU32(rom, rowAt + 4 * i, (entry & ~0x3Fu & ~((uint)SelectorMask << 6)) | (uint)mold | (uint)(selector << 6));
            }
        }

        log.Add($"large images: {newPalettes.Count} recoloured palettes");
    }

    private static void RecolorMiniSprite(byte[] rom, int sprites, int families, int id,
        IReadOnlyDictionary<string, string> recolor, RomAllocator allocator, ICollection<string> log)
    {
        var map = ColorMap(recolor);
        var family = rom[families + id];
        var paletteAt = MiniSpritePalette.EuPalettes + 32 * family;
        var source = Enumerable.Range(0, 16)
            .Select(c => MiniSpritePalette.Rgb(Map(map, BinaryPrimitives.ReadUInt16LittleEndian(rom.AsSpan(paletteAt + 2 * c))) & 0x7FFF))
            .ToArray();
        var components = Enumerable.Range(0, 6)
            .Select(k => ExpansionCodecs.MaliasDecompress(rom, (int)U32(rom, sprites + (id * 6 + k) * 4) - GbaBase).Payload)
            .ToArray();
        var banks = Enumerable.Range(0, 4)
            .Select(k => Enumerable.Range(0, 16).Select(c =>
                MiniSpritePalette.Rgb(BinaryPrimitives.ReadUInt16LittleEndian(rom.AsSpan(MiniSpritePalette.EuPalettes + 32 * k + 2 * c)) & 0x7FFF)).ToArray())
            .ToList();
        var (bank, remapped) = MiniSpritePalette.RemapToExistingBanks(new MiniSpritePalette.Job("", source, components, Restyle: false), banks);
        for (var k = 0; k < remapped.Length; k++)
        {
            var blob = ExpansionCodecs.MaliasStore(remapped[k]);
            var at = allocator.Take(blob.Length);
            blob.CopyTo(rom, at);
            WriteU32(rom, sprites + (id * 6 + k) * 4, (uint)(GbaBase + at));
        }

        rom[families + id] = (byte)bank;
        log.Add($"mini sprite: palette bank {bank}");
    }

    // Own drawings: every piece of a part is re-encoded with one palette built from the pictures (pieces
    // without a picture keep the source bot's drawing); each entry gets new selector slots.
    private static void ApplyArt(byte[] rom, RomLayout layout, Dictionary<string, int> previews, int sprites, int families,
        int id, IReadOnlyDictionary<string, ArtImage> art, RomAllocator allocator, ICollection<string> log)
    {
        var slots = BotArt.Slots(rom, id);      // the new bot's rows are still the source bot's
        var unknown = art.Keys.Where(name => slots.All(slot => slot.Name != name)).ToList();
        Require(unknown.Count == 0, $"No piece named {string.Join(", ", unknown)} in this body type.");
        var taken = UsedSelectors(rom, layout, previews);
        var descriptors = layout.CompositePreviewDescriptorPointerTableOffset;

        foreach (var (part, name, rowSize) in BotArt.PreviewParts)
        {
            var partSlots = slots.Where(slot => slot.Part == name && !slot.IsMini).ToList();
            if (!partSlots.Any(slot => art.ContainsKey(slot.Name)))
            {
                continue;
            }

            var images = partSlots.Select(slot => art.GetValueOrDefault(slot.Name) ?? slot.Current).ToList();
            for (var i = 0; i < partSlots.Count; i++)
            {
                Require(images[i].Width == partSlots[i].Width && images[i].Height == partSlots[i].Height,
                    $"{partSlots[i].Name}: {images[i].Width}x{images[i].Height}, expected {partSlots[i].Width}x{partSlots[i].Height}.");
            }

            var (palette, indices, sourceColors) = BotArt.BuildPalette(images);
            var paletteAt = allocator.Take(PaletteSize);
            BotArt.PaletteBytes(palette).CopyTo(rom, paletteAt);
            var rowAt = previews[part] + id * rowSize;
            foreach (var group in partSlots.Select((slot, i) => (slot, i)).GroupBy(t => t.slot.Entry))
            {
                var entry = U32(rom, rowAt + 4 * group.Key);
                var mold = (int)(entry & 0x3F);
                var record = (int)U32(rom, descriptors + 4 * mold) - GbaBase;
                var blobs = (int)U32(rom, record) - GbaBase;
                var count = group.Count();
                var used = taken.TryGetValue(mold, out var set) ? set : taken[mold] = [];
                var selector = Enumerable.Range(1, SelectorSlots - count)
                    .FirstOrDefault(s => Enumerable.Range(s, count).All(x => !used.Contains(x)));
                Require(selector > 0, $"Mold {mold} has no free image slot left.");
                if (selector + count > OriginalSlots && blobs < 0x800000)
                {
                    blobs = WidenBlobList(rom, record, blobs, allocator);
                }

                foreach (var (slot, i) in group)
                {
                    var blob = Compression.GbaLz77.Compress(BotArt.ToTiles(indices[i], slot.Width, slot.Height));
                    var at = allocator.Take(blob.Length);
                    blob.CopyTo(rom, at);
                    var pointer = (uint)(GbaBase + at);
                    used.Add(selector + slot.Pose);
                    WriteU32(rom, blobs + 8 * (selector + slot.Pose), pointer);
                    WriteU32(rom, blobs + 8 * (selector + slot.Pose) + 4, slot.OwnPalette ? (uint)(GbaBase + paletteAt) : pointer);
                }

                WriteU32(rom, rowAt + 4 * group.Key, (entry & ~0x3Fu & ~((uint)SelectorMask << 6)) | (uint)mold | (uint)(selector << 6));
            }

            log.Add($"{name}: own drawings, {sourceColors} colours -> {palette.Count(c => c != 0)}");
        }

        var miniSlots = slots.Where(slot => slot.IsMini).ToList();
        if (miniSlots.Any(slot => art.ContainsKey(slot.Name)))
        {
            var images = miniSlots.Select(slot => art.GetValueOrDefault(slot.Name) ?? slot.Current).ToList();
            Require(images.All(image => image.Width == 16 && image.Height == 16), "Mini sprite pictures are 16x16.");
            var (palette, indices, _) = BotArt.BuildPalette(images);
            var miniPalette = palette.Select(c => new[] { (double)(c >> 16 & 0xFF), c >> 8 & 0xFF, c & 0xFF }).ToArray();
            var components = Enumerable.Range(0, 6)
                .Select(k => Enumerable.Range(0, miniSlots.Count).Where(i => BotArt.MiniSlots[i].Component == k)
                    .SelectMany(i => BotArt.ToTiles(indices[i], 16, 16)).ToArray())
                .ToArray();
            var banks = Enumerable.Range(0, 4)
                .Select(k => Enumerable.Range(0, 16).Select(c =>
                    MiniSpritePalette.Rgb(BinaryPrimitives.ReadUInt16LittleEndian(rom.AsSpan(MiniSpritePalette.EuPalettes + 32 * k + 2 * c)) & 0x7FFF)).ToArray())
                .ToList();
            var (bank, remapped) = MiniSpritePalette.RemapToExistingBanks(new MiniSpritePalette.Job("", miniPalette, components, Restyle: false), banks);
            for (var k = 0; k < remapped.Length; k++)
            {
                var blob = ExpansionCodecs.MaliasStore(remapped[k]);
                var at = allocator.Take(blob.Length);
                blob.CopyTo(rom, at);
                WriteU32(rom, sprites + (id * 6 + k) * 4, (uint)(GbaBase + at));
            }

            rom[families + id] = (byte)bank;
            log.Add($"mini sprite: own drawings, palette bank {bank}");
        }
    }

    // N -> N+1 in every code constant the expansion ties to the bot count (values checked first).
    private static void RaiseBotCount(byte[] rom, int n, ICollection<string> log)
    {
        foreach (var pc in ExpansionPatchSites.LastBotSites)
        {
            SetImmediate(rom, pc, n - 1, n);
        }

        foreach (var pc in ExpansionPatchSites.ShopScrollAddSites)
        {
            SetImmediate(rom, pc, n, n + 1);
        }

        int oldLast = (n + 3) / 4 - 1, newLast = (n + 4) / 4 - 1;
        foreach (var pc in ExpansionPatchSites.LastPageSites)
        {
            SetImmediate(rom, pc, oldLast, newLast);
        }

        var pages = newLast + 1;
        Require(pages is >= 10 and <= 99, $"{pages} pages do not fit the two-digit page total.");
        var (tens, units) = Math.DivRem(pages, 10);
        rom[0x4F1560] = (byte)(0x60 + 2 * tens);
        rom[0x4F1562] = (byte)(0x60 + 2 * units);
        rom[0x4F1580] = (byte)(0x61 + 2 * tens);
        rom[0x4F1583] = (byte)(0x61 + 2 * units);

        foreach (var at in ExpansionPatchSites.ShopScrollLiterals)
        {
            SetLiteral(rom, at, ExpansionPatchSites.ShopScrollBase + n, ExpansionPatchSites.ShopScrollBase + n + 1);
        }

        SetLiteral(rom, ExpansionPatchSites.ShopItemsLastLiteral, ExpansionPatchSites.ShopItemsBase + (n - 1) * 8,
            ExpansionPatchSites.ShopItemsBase + n * 8);
        log.Add($"bot count {n} -> {n + 1}; part list pages {oldLast + 1} -> {pages}");
    }

    // Selectors already taken in each mold: every preview row of the ROM plus the synthetic templates.
    internal static Dictionary<int, HashSet<int>> UsedSelectors(byte[] rom, RomLayout layout, Dictionary<string, int> previews)
    {
        var used = new Dictionary<int, HashSet<int>>();
        var rows = ExpansionPatchSites.TinpetId + 2;

        void Scan(int baseOffset, int count)
        {
            for (var i = 0; i < count; i++)
            {
                var entry = U32(rom, baseOffset + 4 * i);
                var mold = (int)(entry & 0x3F);
                if (mold == 0)
                {
                    continue;
                }

                if (!used.TryGetValue(mold, out var set))
                {
                    used[mold] = set = [];
                }

                set.UnionWith(Selectors(entry));
            }
        }

        foreach (var (name, rowSize) in PreviewKinds)
        {
            Scan(previews[name], rows * rowSize / 4);
        }

        Scan(layout.CompositePreviewHeadSyntheticDescriptorTemplateOffset, 4 * 0x18 / 4);
        return used;
    }

    internal static int WidenBlobList(byte[] rom, int record, int blobs, RomAllocator allocator)
    {
        var at = allocator.Take(SelectorSlots * 8);
        rom.AsSpan(blobs, OriginalSlots * 8).CopyTo(rom.AsSpan(at));
        for (var s = OriginalSlots; s < SelectorSlots; s++)
        {
            rom.AsSpan(blobs, 8).CopyTo(rom.AsSpan(at + 8 * s));
        }

        WriteU32(rom, record, (uint)(GbaBase + at));
        return at;
    }

    // Free space after the last used byte (the expansion leaves 0xFF up to 16 MB); grows the ROM if full.
    internal static RomAllocator FreeSpace(ref byte[] rom)
    {
        var last = rom.Length - 1;
        while (last > 0x800000 && rom[last] == 0xFF)
        {
            last--;
        }

        var start = (last + 1 + 0x10 + 3) & ~3;
        if (rom.Length - start < 0x10000)
        {
            Array.Resize(ref rom, Math.Min(0x2000000, rom.Length + 0x100000));
            rom.AsSpan(start).Fill(0xFF);
        }

        return new RomAllocator(start);
    }

    private static List<int> Selectors(uint entry)
    {
        var selector = (int)((entry >> 6) & SelectorMask);
        return Enumerable.Range(selector, (int)((entry >> 15) & 1) + 1).ToList();
    }

    private static Dictionary<int, int> ColorMap(IReadOnlyDictionary<string, string> recolor) =>
        recolor.ToDictionary(pair => HexToBgr555(pair.Key), pair => HexToBgr555(pair.Value));

    private static int Map(Dictionary<int, int> map, int color) =>
        (map.TryGetValue(color & 0x7FFF, out var value) ? value : color & 0x7FFF) | (color & 0x8000);

    private static int HexToBgr555(string hex)
    {
        Require(hex.Length == 7 && hex[0] == '#', $"Colour '{hex}' must be #rrggbb.");
        int r = Convert.ToInt32(hex.Substring(1, 2), 16), g = Convert.ToInt32(hex.Substring(3, 2), 16), b = Convert.ToInt32(hex.Substring(5, 2), 16);
        return r / 8 | (g / 8) << 5 | (b / 8) << 10;
    }

    private static void CopyRow(byte[] rom, int table, int size, int from, int to) =>
        rom.AsSpan(table + from * size, size).CopyTo(rom.AsSpan(table + to * size));

    private static void SetImmediate(byte[] rom, int pc, int expected, int value)
    {
        Require(rom[pc + 1] is >= 0x20 and <= 0x3F && rom[pc] == expected,
            $"0x{pc:X6}: expected the bot-count constant {expected}, found {rom[pc]} (was this ROM made by the expansion?).");
        rom[pc] = (byte)value;
    }

    private static void SetLiteral(byte[] rom, int at, int expected, int value)
    {
        Require(U32(rom, at) == (uint)expected, $"Literal 0x{at:X6}: expected 0x{expected:X}, found 0x{U32(rom, at):X}.");
        WriteU32(rom, at, (uint)value);
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
