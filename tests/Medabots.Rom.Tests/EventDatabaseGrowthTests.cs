using Medabots.Rom.Events;
using Medabots.Rom.Metadata;
using Medabots.Rom.Projects;
using Xunit;

namespace Medabots.Rom.Tests;

/// <summary>
/// The event database moves (with its scripts) when it grows or when free space is beyond the reach of
/// its banks; every existing event must read back the same afterwards.
/// </summary>
public sealed class EventDatabaseGrowthTests
{
    [Fact]
    public async Task ExpandingTheEventDatabase_KeepsEveryExistingEvent()
    {
        var rom = await RomFile.LoadAsync(TestRomLocator.FindWorkspaceRom());
        var profile = Assert.IsType<MedabotsRomTextProfile>(MedabotsRomTextProfiles.Detect(rom));
        var project = new RomHackProject { Name = "Event Expansion", TextProfileId = profile.Id };
        project.EventScriptPatches.Add(new EventScriptPatch((short)(profile.EventCount + 2), [0x04, 0x02, MedabotsRomSchema.EventEndOpcode]));
        var session = RomHackSession.FromRomFile(new RomFile("event-expansion.gba", rom.Data.ToArray()));

        new RomHackProjectApplicator().Apply(project, session);

        AssertSameEvents(rom, session.RomFile, profile);
        // The three code literals the game copies into its "script base" RAM variable (0x3005590).
        foreach (var literal in new[] { 0x4A584, 0x4A8C8, 0x4B124 })
        {
            Assert.Equal((uint)rom.Layout.EventScriptDatabaseOffset + 0x08000000u, BitConverter.ToUInt32(rom.Data, literal));
            Assert.Equal((uint)session.RomFile.Layout.EventScriptDatabaseOffset + 0x08000000u, BitConverter.ToUInt32(session.RomFile.Data, literal));
        }

        var reader = new EventScriptReader();
        var added = reader.ReadById(session.RomFile, profile, (short)(profile.EventCount + 2));
        Assert.Equal([(byte)0x04, MedabotsRomSchema.EventEndOpcode], added.Instructions.Select(i => i.Opcode));
        // The slots in between exist and are empty scripts.
        Assert.Equal([MedabotsRomSchema.EventEndOpcode], reader.ReadById(session.RomFile, profile, (short)profile.EventCount).Instructions.Select(i => i.Opcode));
    }

    // In the 16 MB expanded ROM free space starts past the original database's 4 MB of banks.
    [Fact]
    public void RelocatingAnEvent_WorksInTheExpandedRom()
    {
        var metabee = File.ReadAllBytes(TestRomLocator.FindRomByHeader(TestRomLocator.MetabeeEuropeHeader));
        var expanded = new RomFile("expanded.gba", Medabots.Rom.Expansion.RomExpander.Build(metabee).Rom);
        var profile = Assert.IsType<MedabotsRomTextProfile>(MedabotsRomTextProfiles.Detect(expanded));
        var longer = Enumerable.Repeat((byte)0x00, 600).Append(MedabotsRomSchema.EventEndOpcode).ToArray();
        var project = new RomHackProject { Name = "Relocate", TextProfileId = profile.Id };
        project.EventScriptPatches.Add(new EventScriptPatch(0, longer));
        var session = RomHackSession.FromRomFile(new RomFile("expanded-copy.gba", expanded.Data.ToArray()));

        new RomHackProjectApplicator().Apply(project, session);

        var reread = new EventScriptReader().ReadById(session.RomFile, profile, 0);
        Assert.Equal(601, reread.Instructions.Count);
        Assert.Equal(expanded.Length, session.RomFile.Length);      // stays inside the 16 MB
        AssertSameEvents(expanded, session.RomFile, profile, 0);
    }

    // The editor's preview path (RewriteEvent on a session) does the same.
    [Fact]
    public void RewriteEvent_InTheExpandedRom_MovesTheDatabaseWhenNeeded()
    {
        var metabee = File.ReadAllBytes(TestRomLocator.FindRomByHeader(TestRomLocator.MetabeeEuropeHeader));
        var expanded = new RomFile("expanded.gba", Medabots.Rom.Expansion.RomExpander.Build(metabee).Rom);
        var profile = Assert.IsType<MedabotsRomTextProfile>(MedabotsRomTextProfiles.Detect(expanded));
        var session = RomHackSession.FromRomFile(new RomFile("preview.gba", expanded.Data.ToArray()));
        var patcher = new EventInstructionPatcher();

        patcher.RewriteEvent(session, profile, 5, [0x04, 0x10, MedabotsRomSchema.EventEndOpcode], "preview");
        patcher.RewriteEvent(session, profile, 6, [0x04, 0x20, MedabotsRomSchema.EventEndOpcode], "preview again");

        var reader = new EventScriptReader();
        Assert.Equal(0x10, reader.ReadById(session.RomFile, profile, 5).Instructions[0].Arguments[0].RawValue);
        Assert.Equal(0x20, reader.ReadById(session.RomFile, profile, 6).Instructions[0].Arguments[0].RawValue);
        AssertSameEvents(expanded, session.RomFile, profile, 5, 6);
    }

    private static void AssertSameEvents(RomFile before, RomFile after, MedabotsRomTextProfile profile, params int[] skip)
    {
        var reader = new EventScriptReader();
        var differences = new List<string>();
        var compared = 0;
        for (short id = 0; id < profile.EventCount; id++)
        {
            if (skip.Contains(id))
            {
                continue;
            }

            string original;
            try
            {
                original = Describe(reader.ReadById(before, profile, id));
            }
            catch (Exception ex) when (ex is InvalidDataException or ArgumentException or IndexOutOfRangeException or InvalidOperationException or KeyNotFoundException)
            {
                continue;       // unreadable already in the original
            }

            string moved;
            try
            {
                moved = Describe(reader.ReadById(after, profile, id));
            }
            catch (Exception ex) when (ex is InvalidDataException or ArgumentException or IndexOutOfRangeException or InvalidOperationException or KeyNotFoundException)
            {
                moved = $"<{ex.GetType().Name}>";
            }

            compared++;
            if (moved != original)
            {
                differences.Add($"{id}");
            }
        }

        Assert.True(compared > 2000, $"only {compared} events compared");
        Assert.True(differences.Count == 0, $"{differences.Count} events changed: {string.Join(", ", differences.Take(10))}");
    }

    // Opcodes and argument values, without addresses (the scripts move).
    private static string Describe(EventScript script) =>
        string.Join(";", script.Instructions.Select(i => $"{i.Opcode:X2}({string.Join(",", i.Arguments.Select(a => a.RawValue))})"));
}
