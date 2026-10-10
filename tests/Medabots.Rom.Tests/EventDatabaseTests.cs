using Medabots.Rom.Events;
using Medabots.Rom.Metadata;
using Xunit;

namespace Medabots.Rom.Tests;

public sealed class EventDatabaseTests
{
    [Theory]
    [InlineData(TestRomLocator.RokushoEuropeHeader, 0x769EE4)]
    [InlineData(TestRomLocator.MetabeeEuropeHeader, 0x769EC0)]
    public void ResolveInstalledEventTable_FollowsTheGameCode(string header, int expectedTable)
    {
        var rom = new RomFile("rom.gba", File.ReadAllBytes(TestRomLocator.FindRomByHeader(header)));
        var profile = MedabotsRomTextProfiles.Detect(rom)!;

        Assert.Equal((expectedTable, 0x8F0), EventScriptReader.ResolveInstalledEventTable(rom, profile));
        Assert.Equal(expectedTable, profile.EventTableOffset);
    }

    [Fact]
    public void ExpandedRom_EveryEventParses()
    {
        // Data far past 0x800000 used to be mistaken for a relocated event database.
        var path = TestRomLocator.FindOptionalFile(Path.Combine("build", "Medabots_expanded.gba"));
        if (path is null)
        {
            return;
        }

        var rom = new RomFile(path, File.ReadAllBytes(path));
        var profile = MedabotsRomTextProfiles.Detect(rom)!;
        var reader = new EventScriptReader();
        var (_, eventCount) = EventScriptReader.ResolveInstalledEventTable(rom, profile);
        Assert.Equal(0x8F0, eventCount);
        for (short id = 0; id < eventCount; id++)
        {
            reader.ReadById(rom, profile, id);
        }
    }
}
