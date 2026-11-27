using System.Buffers.Binary;
using Medabots.Rom.Metadata;
using Medabots.Rom.Projects;

namespace Medabots.Rom.Events;

/// <summary>
/// Moves the event script database (header, pointer table, bank table) together with every event script
/// to free space, re-encoding each event's bank/address for the new base.
/// <para>
/// The game finds event N at <c>base + bank[N] * 0x4000 + (pointer[N] - 0x4000)</c> (a Game Boy style
/// banked address), with <c>base</c> the literal the code loads, the pointer table at base + 0x4000 and
/// the bank table at base + header[2..3]. Banks are one byte, so every script must lie within
/// 256 * 16 KB after the base. Scripts only reference each other by event ID and jump by relative
/// offsets, so a run of scripts can move as long as it is copied as one block.
/// </para>
/// Needed when the table grows (a new event ID) or when free space is beyond the banks' reach (the
/// 16 MB expanded ROM): moving only the table would leave the old pointers relative to the old base.
/// </summary>
internal static class EventDatabaseRelocator
{
    private const int BankSize = MedabotsRomSchema.EventBankSize;
    private const int BankWindow = MedabotsRomSchema.EventBankBaseAddress;     // pointers are 0x4000-0x7FFF
    private const int MaxBanks = 256;
    private const int ClusterGap = 0x1000;          // scripts closer than this are copied as one block
    private const int ScriptTailMargin = 0x20;      // bytes kept after the last instruction of a block

    /// <summary>True when an event at <paramref name="address"/> can be addressed from the table at <paramref name="tableOffset"/>.</summary>
    public static bool Reaches(int tableOffset, int address)
    {
        var relative = address - (tableOffset - BankWindow);
        return relative >= BankWindow && relative < MaxBanks * BankSize;
    }

    /// <summary>
    /// Builds the moved database with <paramref name="eventCount"/> slots (new slots point at an END script)
    /// and returns the profile describing it. <paramref name="rom"/> must hold the current database.
    /// </summary>
    public static MedabotsRomTextProfile Relocate(RomFile rom, MedabotsRomTextProfile profile, int eventCount,
        FreeSpaceAllocator allocator, ICollection<RomPatchAction> actions, EventScriptReader reader)
    {
        var data = rom.Data;
        var (tableOffset, oldCount) = EventScriptReader.ResolveInstalledEventTable(rom, profile);
        var oldBase = tableOffset - BankWindow;
        eventCount = Math.Max(eventCount, oldCount);

        // Where each event starts and how far its instructions reach.
        var starts = new int[oldCount];
        var ends = new int[oldCount];
        for (short id = 0; id < oldCount; id++)
        {
            starts[id] = oldBase + data[tableOffset + oldCount * 2 + id] * BankSize
                         + BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(tableOffset + id * 2)) - BankWindow;
            ends[id] = starts[id] + ScriptTailMargin;
            if (starts[id] < 0 || starts[id] >= data.Length)
            {
                starts[id] = -1;        // a broken slot: kept pointing at the END stub
                continue;
            }

            try
            {
                var script = reader.ReadById(rom, profile, id);
                if (script.Instructions.Count > 0)
                {
                    ends[id] = Math.Max(ends[id], script.Instructions.Max(i => i.Offset) + ScriptTailMargin);
                }
            }
            catch (Exception ex) when (ex is InvalidDataException or ArgumentException or IndexOutOfRangeException or InvalidOperationException or KeyNotFoundException)
            {
                // Unparsable script: its first bytes are still copied.
            }
        }

        // Blocks of nearby scripts, copied verbatim (keeps any jump between neighbouring scripts valid).
        var blocks = new List<(int Start, int End)>();
        foreach (var id in Enumerable.Range(0, oldCount).Where(i => starts[i] >= 0).OrderBy(i => starts[i]))
        {
            var end = Math.Min(ends[id], data.Length);
            if (blocks.Count > 0 && starts[id] <= blocks[^1].End + ClusterGap)
            {
                blocks[^1] = (blocks[^1].Start, Math.Max(blocks[^1].End, end));
            }
            else
            {
                blocks.Add((starts[id], end));
            }
        }

        // New layout: [base: header .. 16 KB][pointer table][bank table][END stub][blocks...]
        var bankTableRelative = BankWindow + eventCount * 2;
        if (bankTableRelative + eventCount > 0xFFFF)
        {
            throw new InvalidOperationException($"{eventCount} events do not fit the event database header (16-bit table offsets).");
        }

        var stubRelative = FreeSpaceAllocator.AlignUp(bankTableRelative + eventCount, 4);
        var cursor = stubRelative + 4;
        var blockRelative = new List<int>();
        foreach (var (start, end) in blocks)
        {
            cursor = FreeSpaceAllocator.AlignUp(cursor, 4) + (start & 3);      // keeps each block's alignment
            blockRelative.Add(cursor);
            cursor += end - start;
        }

        if (cursor >= MaxBanks * BankSize)
        {
            throw new InvalidOperationException("The event scripts do not fit the 4 MB the event banks can address.");
        }

        var newBase = allocator.Reserve(cursor, 4);
        var image = new byte[cursor];
        image.AsSpan().Fill(0xFF);
        image[0] = (byte)(BankWindow & 0xFF);
        image[1] = (byte)(BankWindow >> 8);
        image[2] = (byte)(bankTableRelative & 0xFF);
        image[3] = (byte)(bankTableRelative >> 8);
        image[stubRelative] = MedabotsRomSchema.EventEndOpcode;
        for (var b = 0; b < blocks.Count; b++)
        {
            data.AsSpan(blocks[b].Start, blocks[b].End - blocks[b].Start).CopyTo(image.AsSpan(blockRelative[b]));
        }

        void SetEntry(int id, int relative)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(image.AsSpan(BankWindow + id * 2), (ushort)(BankWindow + relative % BankSize));
            image[bankTableRelative + id] = (byte)(relative / BankSize);
        }

        for (var id = 0; id < eventCount; id++)
        {
            SetEntry(id, stubRelative);
        }

        for (var id = 0; id < oldCount; id++)
        {
            if (starts[id] < 0)
            {
                continue;
            }

            var b = blocks.FindIndex(block => starts[id] >= block.Start && starts[id] < block.End);
            SetEntry(id, blockRelative[b] + (starts[id] - blocks[b].Start));
        }

        actions.Add(RomPatchAction.Create(newBase, image, $"Move the event database and scripts ({eventCount} events) to 0x{newBase:X}"));
        foreach (var literal in CodeLiterals(data, GbaPointer.ToRomAddress(oldBase)))
        {
            actions.Add(RomPatchAction.Create(literal, BitConverter.GetBytes(GbaPointer.ToRomAddress(newBase)), $"Repoint event database literal at 0x{literal:X}"));
        }

        var addresses = profile.Addresses;
        return profile with
        {
            Addresses = new MedabotsRomAddresses(
                addresses.TextPointerTableOffset,
                addresses.TextDumpOffset,
                addresses.StarterOffset,
                addresses.BattlePointerTableOffset,
                addresses.BattleCount,
                newBase + BankWindow,
                eventCount),
        };
    }

    // Literal-pool words holding the database address, i.e. loaded by a Thumb "ldr rX, [pc, #imm]".
    private static List<int> CodeLiterals(byte[] data, uint address)
    {
        var found = new List<int>();
        for (var at = 0; at <= data.Length - 4; at += 4)
        {
            if (BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(at)) != address)
            {
                continue;
            }

            for (var pc = Math.Max(0, at - 1020); pc < at; pc += 2)
            {
                var instruction = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(pc));
                if (instruction >> 11 == 0b01001 && ((pc + 4) & ~3) + (instruction & 0xFF) * 4 == at)
                {
                    found.Add(at);
                    break;
                }
            }
        }

        return found;
    }
}
