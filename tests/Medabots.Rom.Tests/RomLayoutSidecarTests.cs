using Medabots.Rom.Layout;
using Xunit;

namespace Medabots.Rom.Tests;

public sealed class RomLayoutSidecarTests
{
    [Fact]
    public void Resolve_WithLayoutFile_OverridesTablesAndRoster()
    {
        var romData = File.ReadAllBytes(TestRomLocator.FindWorkspaceRom());
        var dir = Directory.CreateTempSubdirectory("layout-sidecar");
        try
        {
            var romPath = Path.Combine(dir.FullName, "medabots_rokusho.gba");
            File.WriteAllBytes(romPath, romData);
            File.WriteAllText(Path.Combine(dir.FullName, "medabots_rokusho.layout.json"),
                """{"bot_count": 326, "tinpet_id": 326, "tables": {"PartTable": 8400000, "BotNameTable": 8424300}}""");

            var withFile = RomLayout.Resolve(romData, romPath);
            var withoutFile = RomLayout.Resolve(romData);

            Assert.True(withFile.FromLayoutFile);
            Assert.Equal(326, withFile.BotCount);
            Assert.Equal(326, withFile.TinpetId);
            Assert.Equal(8400000, withFile.PartTableOffset);
            Assert.Equal(8424300, withFile.BotNameTableOffset);
            // tables the file does not list still come from the code signatures
            Assert.Equal(withoutFile.ShopTableOffset, withFile.ShopTableOffset);
            Assert.False(withoutFile.FromLayoutFile);
            Assert.Equal(RomLayout.OriginalBotCount, withoutFile.BotCount);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    [Fact]
    public void Resolve_WithoutLayoutFile_IsUnchanged()
    {
        var romData = File.ReadAllBytes(TestRomLocator.FindWorkspaceRom());
        var a = RomLayout.Resolve(romData);
        var b = RomLayout.Resolve(romData, Path.Combine(Path.GetTempPath(), "no-such-dir", "rom.gba"));

        Assert.Equal(a.Tables.OrderBy(t => t.Key), b.Tables.OrderBy(t => t.Key));
        Assert.Equal(a.BotCount, b.BotCount);
        Assert.False(b.FromLayoutFile);
    }
}
