using System.Buffers.Binary;
using System.Text;

using Medabots.Rom.Text;

namespace Medabots.Rom.Expansion;

public sealed record ShopAssignment(int Shop, int Slot, int Bot);

/// <param name="TestPrice">Temporary shop price for the new bots (ID >= 120) and the bots placed in shops.</param>
public sealed record ExpansionOptions(int? TestPrice = null, IReadOnlyList<ShopAssignment>? Shops = null);

public sealed record ExpansionResult(byte[] Rom, int BotCount, IReadOnlyList<string> Log);

/// <summary>
/// Builds the expanded Medabots: Metabee (E) ROM (120 original bots + the manifest's new bots and the
/// 60 Shingata Medarot bots, Tinpet fixed at 236/237). Mirrors tools/build.py step by step, so both
/// produce the same ROM. Inputs: the Metabee (E) ROM and the Shingata Medarot Kabuto ROM.
/// </summary>
public static class RomExpander
{
    public const string MetabeeEuropeHeader = "MEDABOTSMTBVA8BPE9";
    public const string ShingataKabutoHeader = "SHINMEDA-KBTBKVJ9C";

    private const int GbaBase = 0x08000000;
    private const int RomSize = 0x1000000;
    private const int TableArea = 0xC00000;
    private const int CodeEnd = 0x0A0000;
    private const int OriginalBots = 120;
    private const int MaxBots = 240;
    private const int PartSize = 0x10;
    private const int PartStats = 0x3B83FC;
    private const int BotNames = 0x3BA658;
    private const int PartNames = 0x3BBB4C;
    private const int ShingataPartStats = 0x467238;
    private static readonly byte[] ShopTableSignature = [0x13, 0x00, 0xFF, 0xFF, 0x13, 0x00, 0x42, 0xFF, 0x13, 0x00];

    // (name, original table, bytes per bot) — order is the allocation order.
    private static readonly (string Name, int Table, int Size)[] GraphicsTables =
    [
        ("BATTLE_SPRITES", 0x3B1A08, 24),
        ("PALETTE_FAMILY", 0x3BEB94, 1),
        ("MOVE_STYLE", 0x3CAD78, 1),
        ("PREVIEW_HEAD", 0x07EE70, 16),
        ("PREVIEW_RIGHT_ARM", 0x07F610, 16),
        ("PREVIEW_LEFT_ARM", 0x07FDB0, 16),
        ("PREVIEW_LEGS", 0x080550, 32),
    ];

    public static string ReadHeader(ReadOnlySpan<byte> rom) =>
        rom.Length < 0xA0 + 18 ? "" : Encoding.ASCII.GetString(rom.Slice(0xA0, 18));

    /// <param name="shingataKabuto">Optional: with it the 60 Shingata Medarot bots are added; without it the
    /// expanded base holds the original roster plus the manifest's own bots (Sagehorn).</param>
    public static ExpansionResult Build(byte[] metabeeEurope, byte[]? shingataKabuto = null, ExpansionOptions? options = null,
        ExpansionManifest? manifest = null)
    {
        options ??= new ExpansionOptions();
        manifest ??= ExpansionManifest.Default;
        Require(ReadHeader(metabeeEurope) == MetabeeEuropeHeader,
            $"The first ROM must be Medabots: Metabee (Europe) ({MetabeeEuropeHeader}); got '{ReadHeader(metabeeEurope)}'.");
        Require(shingataKabuto is null || ReadHeader(shingataKabuto) == ShingataKabutoHeader,
            $"The second ROM must be Shingata Medarot Kabuto Version ({ShingataKabutoHeader}); got '{(shingataKabuto is null ? "" : ReadHeader(shingataKabuto))}'.");

        var original = metabeeEurope;
        var rom = new byte[RomSize];
        Array.Fill(rom, (byte)0xFF);
        original.CopyTo(rom, 0);

        var added = manifest.Bots.Where(bot => bot.ShingataBot is null || shingataKabuto is not null)
            .OrderBy(bot => bot.Id).ToList();
        Require(added.Select(bot => bot.Id).SequenceEqual(Enumerable.Range(OriginalBots, added.Count)),
            "Manifest bot IDs must continue the original roster without gaps.");
        var n = OriginalBots + added.Count;
        var tinpet = ExpansionPatchSites.TinpetId;
        Require(n <= tinpet && n <= MaxBots, $"{n} bots: the maximum is {tinpet} (Tinpet fixed at {tinpet}/{tinpet + 1}).");
        var rows = tinpet + 2;
        var shopBots = (options.Shops ?? []).Select(shop => shop.Bot).ToHashSet();
        var allocator = new RomAllocator(TableArea);
        var log = new List<string> { $"{n} bots; Tinpet fixed = {tinpet}/{tinpet + 1}; IDs {n}-{tinpet - 1} empty" };

        // ---- stats ----
        var statsAt = allocator.Take(rows * 4 * PartSize);
        rom.AsSpan(statsAt, rows * 4 * PartSize).Clear();
        for (var bot = 0; bot < n; bot++)
        {
            for (var kind = 0; kind < 4; kind++)
            {
                var record = PartRecord(bot, kind, original, shingataKabuto, added);
                if (options.TestPrice is { } price && (bot >= OriginalBots || shopBots.Contains(bot)))
                {
                    record[11] = (byte)price;
                }

                record.CopyTo(rom, statsAt + (bot * 4 + kind) * PartSize);
            }
        }

        // ---- names ----
        int NewString(string text)
        {
            var encoded = EuropeanNameEncoding.Encode(text);
            var at = allocator.Take(encoded.Length, align: 1);
            encoded.CopyTo(rom, at);
            return at;
        }

        var tinpetName = NewString("TINPET");
        var botNamesAt = allocator.Take(rows * 4);
        var partNamesAt = allocator.Take(rows * 4 * 4);
        for (var bot = 0; bot < rows; bot++)
        {
            int botName;
            var partNames = new int[4];
            if (bot < OriginalBots)
            {
                botName = (int)U32(original, BotNames + bot * 4) - GbaBase;
                for (var kind = 0; kind < 4; kind++)
                {
                    partNames[kind] = (int)U32(original, PartNames + (bot * 4 + kind) * 4) - GbaBase;
                }
            }
            else if (bot < n)
            {
                var entry = added[bot - OriginalBots];
                botName = NewString(entry.Name);
                for (var kind = 0; kind < 4; kind++)
                {
                    partNames[kind] = NewString(entry.PartNames[kind]);
                }
            }
            else
            {
                botName = tinpetName;
                Array.Fill(partNames, tinpetName);
            }

            WriteU32(rom, botNamesAt + bot * 4, (uint)(GbaBase + botName));
            for (var kind = 0; kind < 4; kind++)
            {
                WriteU32(rom, partNamesAt + (bot * 4 + kind) * 4, (uint)(GbaBase + partNames[kind]));
            }
        }

        // ---- graphics tables ----
        int SourceRow(int bot)
        {
            if (bot < OriginalBots)
            {
                return bot;
            }

            if (bot >= tinpet)
            {
                return OriginalBots + (bot - tinpet);     // original Tinpet M/F rows (120/121)
            }

            if (bot >= n)
            {
                return OriginalBots;                       // empty slot: Tinpet graphics as filler
            }

            return added[bot - OriginalBots].GraphicsFrom ?? 43;
        }

        var graphics = new Dictionary<string, int>();
        foreach (var (name, table, size) in GraphicsTables)
        {
            var at = allocator.Take(rows * size);
            for (var bot = 0; bot < rows; bot++)
            {
                original.AsSpan(table + SourceRow(bot) * size, size).CopyTo(rom.AsSpan(at + bot * size));
            }

            graphics[name] = at;
        }

        new ShingataGraphicsPorter(rom, original, shingataKabuto ?? [], allocator, graphics, log)
            .Port(added.Where(bot => bot.ShingataBot is not null || bot.Recolor is not null).ToList());

        // ---- repoint ----
        Repoint(rom, GbaBase + PartStats, GbaBase + statsAt, "PART_STATS", log);
        Repoint(rom, GbaBase + BotNames, GbaBase + botNamesAt, "BOT_NAMES", log);
        Repoint(rom, GbaBase + PartNames, GbaBase + partNamesAt, "PART_NAMES", log);
        foreach (var (name, table, _) in GraphicsTables)
        {
            Repoint(rom, GbaBase + table, GbaBase + graphics[name], name, log);
        }

        // ---- bot count constants ----
        foreach (var pc in ExpansionPatchSites.LastBotSites)
        {
            PatchImmediate(rom, pc, 0x77, n - 1);
        }

        foreach (var pc in ExpansionPatchSites.TinpetCheckSites.Concat(ExpansionPatchSites.TinpetSites))
        {
            PatchImmediate(rom, pc, 0x78, tinpet);
        }

        foreach (var pc in ExpansionPatchSites.ShopScrollAddSites)
        {
            PatchImmediate(rom, pc, 0x78, n);
        }

        foreach (var pc in ExpansionPatchSites.FemaleTinpetSites)
        {
            PatchImmediate(rom, pc, 0x79, tinpet + 1);
        }

        var lastPage = (n + 3) / 4 - 1;
        foreach (var pc in ExpansionPatchSites.LastPageSites)
        {
            PatchImmediate(rom, pc, 0x1D, lastPage);
        }

        PatchPageTotal(rom, lastPage + 1);
        log.Add($"constants: {ExpansionPatchSites.LastBotSites.Length} x N-1, {ExpansionPatchSites.ShopScrollAddSites.Length} x N (shop), " +
                $"{ExpansionPatchSites.TinpetCheckSites.Length + ExpansionPatchSites.TinpetSites.Length} Tinpet, " +
                $"{ExpansionPatchSites.FemaleTinpetSites.Length} female Tinpet, {ExpansionPatchSites.LastPageSites.Length} pages ({lastPage + 1})");

        // ---- inventory ----
        Repoint(rom, ExpansionPatchSites.InventoryOld, ExpansionPatchSites.InventoryNew, "INVENTORY", log);
        foreach (var pc in ExpansionPatchSites.InventoryStrideSites)
        {
            var hw = U16(rom, pc);
            Require(hw >> 11 == 0 && ((hw >> 6) & 31) == 4, $"Inventory stride 0x{pc:X6}: expected lsls #4, found {hw:X4}.");
            WriteU16(rom, pc, (hw & ~(31 << 6)) | (5 << 6));
        }

        foreach (var (address, old, value) in ExpansionPatchSites.InventoryHalfwordPatches)
        {
            PatchHalfword(rom, address, old, value);
        }

        foreach (var (address, old, value) in ExpansionPatchSites.InventoryLiteralPatches)
        {
            PatchLiteral(rom, address, (uint)old, (uint)value);
        }

        PatchStarter(rom, log);
        RepairItalicFont(rom, log);

        foreach (var (address, old, value) in ExpansionPatchSites.Selector8BitPatch)
        {
            PatchHalfword(rom, address, old, value);
        }

        log.Add("part selector: 8 bits (0x45ECC)");
        foreach (var (address, old, value) in ExpansionPatchSites.UnsignedLoopPatches)
        {
            PatchHalfword(rom, address, old, value);
        }

        log.Add($"unsigned loop counters: {ExpansionPatchSites.UnsignedLoopPatches.Length}");

        // ---- shop: scroll variable after the quantity array; last item of the sell list ----
        foreach (var at in ExpansionPatchSites.ShopScrollLiterals)
        {
            var current = U32(rom, at);
            Require(current == ExpansionPatchSites.ShopScrollBase + OriginalBots || current == ExpansionPatchSites.ShopScrollBase + n,
                $"Shop scroll literal 0x{at:X6}: unexpected value 0x{current:X}.");
            WriteU32(rom, at, (uint)(ExpansionPatchSites.ShopScrollBase + n));
        }

        PatchLiteral(rom, ExpansionPatchSites.ShopItemsLastLiteral, (uint)(ExpansionPatchSites.ShopItemsBase + 119 * 8),
            (uint)(ExpansionPatchSites.ShopItemsBase + (n - 1) * 8));

        // ---- shops ----
        if (options.Shops is { Count: > 0 } shops)
        {
            var shopBase = rom.AsSpan().IndexOf(ShopTableSignature);
            Require(shopBase >= 0, "Shop table not found.");
            foreach (var (shop, slot, bot) in shops)
            {
                Require(shop is >= 0 and < 15 && slot is >= 0 and < 4 && bot >= 0 && bot < n, $"Invalid shop slot {shop}:{slot}:{bot}.");
                rom[shopBase + shop * 4 + slot] = (byte)bot;
                log.Add($"shop {shop} slot {slot} = bot {bot}");
            }
        }

        log.Add($"tables 0x{TableArea:X6}-0x{allocator.Next:X6}");
        return new ExpansionResult(rom, n, log);
    }

    private static byte[] PartRecord(int bot, int kind, byte[] original, byte[]? shingata, List<ExpansionBot> added)
    {
        if (bot < OriginalBots)
        {
            return original.AsSpan(PartStats + (bot * 4 + kind) * PartSize, PartSize).ToArray();
        }

        var entry = added[bot - OriginalBots];
        if (entry.ShingataBot is { } source)
        {
            return shingata!.AsSpan(ShingataPartStats + (source * 4 + kind) * PartSize, PartSize).ToArray();
        }

        var stats = entry.Stats ?? throw new InvalidDataException($"{entry.Name}: no stats and no Shingata source.");
        return stats[kind].Select(value => (byte)value).ToArray();
    }

    private static void PatchPageTotal(byte[] rom, int pages)
    {
        Require(pages is >= 10 and <= 99, $"{pages} pages: the '/30' only has room for 2 digits.");
        var (tens, units) = Math.DivRem(pages, 10);
        var values = new Dictionary<int, int>
        {
            [0x4F1560] = 0x60 + 2 * tens, [0x4F1561] = 0x71, [0x4F1562] = 0x60 + 2 * units, [0x4F1563] = 0x71,
            [0x4F1580] = 0x61 + 2 * tens, [0x4F1581] = 0x71, [0x4F1583] = 0x61 + 2 * units,
        };
        foreach (var (address, old) in ExpansionPatchSites.PageTotalBytes)
        {
            Require(rom[address] == old, $"'/30' tilemap differs from the expected one at 0x{address:X6}.");
            rom[address] = (byte)values[address];
        }
    }

    // The European italic font lost its dark shades; see ItalicFontRepair.
    private static void RepairItalicFont(byte[] rom, ICollection<string> log)
    {
        var baseOffset = ExpansionPatchSites.ItalicFont;
        var repaired = ItalicFontRepair.Repair(rom, baseOffset, out var fixedPixels);
        if (repaired is null)
        {
            log.Add("italic font: not damaged, kept");
            return;
        }

        repaired.CopyTo(rom, baseOffset);
        log.Add($"italic font: {fixedPixels} pixels rebuilt (0x{baseOffset:X6})");
    }

    private static void PatchStarter(byte[] rom, ICollection<string> log)
    {
        var bot = rom[ExpansionPatchSites.StarterIdAt];
        Require(rom[ExpansionPatchSites.StarterIdAt + 1] == 0x20, "Starter bot routine differs from the expected one.");
        var baseOffset = bot * 2 + 1;
        int rightArm = 480 + baseOffset, leftArm = 960 + baseOffset, legs = 1440 + baseOffset;
        const int literalA = 0x044BA0, literalB = 0x044BA4;
        PatchLiteral(rom, literalA, (uint)(240 + baseOffset), (uint)rightArm);
        PatchLiteral(rom, literalB, (uint)(720 + baseOffset), (uint)leftArm);
        ushort[] code =
        [
            ThumbLdrLiteral(0x044B7E, 3, literalA), // ldr r3, =RA
            0x18C8,                                 // adds r0, r1, r3
            0x7002,                                 // strb r2, [r0]
            ThumbLdrLiteral(0x044B84, 0, literalB), // ldr r0, =LA
            0x1808,                                 // adds r0, r1, r0
            0x7002,                                 // strb r2, [r0]
            0x30F0,                                 // adds r0, #0xF0
            0x30F0,                                 // adds r0, #0xF0 (LA + 0x1E0 = legs)
            0x7002,                                 // strb r2, [r0]
            0x4770,                                 // bx lr
        ];
        Require(leftArm + 0x1E0 == legs, "Starter offsets mismatch.");
        for (var i = 0; i < code.Length; i++)
        {
            WriteU16(rom, 0x044B7E + i * 2, code[i]);
        }

        log.Add($"starter bot {bot}: offsets 0x{baseOffset:x}/0x{rightArm:x}/0x{leftArm:x}/0x{legs:x}");
    }

    private static ushort ThumbLdrLiteral(int pc, int register, int literal)
    {
        var offset = literal - ((pc + 4) & ~3);
        Require(offset % 4 == 0 && offset is >= 0 and < 1024, "ldr literal out of range.");
        return (ushort)(0x4800 | (register << 8) | (offset / 4));
    }

    private static void Repoint(byte[] rom, int oldPointer, int newPointer, string name, ICollection<string> log)
    {
        var pools = 0;
        for (var offset = 0; offset < CodeEnd; offset += 4)
        {
            if (U32(rom, offset) == (uint)oldPointer)
            {
                WriteU32(rom, offset, (uint)newPointer);
                pools++;
            }
        }

        Require(pools > 0, $"{name}: no pointer found.");
        log.Add($"{name}: {pools} pointers -> 0x{newPointer:X8}");
    }

    // imm8 of a Thumb format 3 instruction (mov/cmp/add/sub), checking the original value.
    private static void PatchImmediate(byte[] rom, int pc, int expected, int value)
    {
        var opcode = rom[pc + 1];
        Require(opcode is >= 0x20 and <= 0x3F && rom[pc] == expected,
            $"0x{pc:X6}: expected immediate 0x{expected:X}, found {rom[pc + 1]:X2}{rom[pc]:X2}.");
        rom[pc] = (byte)value;
    }

    private static void PatchHalfword(byte[] rom, int pc, int old, int value)
    {
        Require(U16(rom, pc) == old, $"0x{pc:X6}: expected {old:X4}, found {U16(rom, pc):X4}.");
        WriteU16(rom, pc, value);
    }

    private static void PatchLiteral(byte[] rom, int at, uint old, uint value)
    {
        Require(U32(rom, at) == old, $"Literal 0x{at:X6}: expected 0x{old:X}, found 0x{U32(rom, at):X}.");
        WriteU32(rom, at, value);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidDataException(message);
        }
    }

    private static uint U32(byte[] data, int offset) => BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(offset));

    private static int U16(byte[] data, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(offset));

    private static void WriteU32(byte[] data, int offset, uint value) => BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(offset), value);

    private static void WriteU16(byte[] data, int offset, int value) => BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(offset), (ushort)value);
}
