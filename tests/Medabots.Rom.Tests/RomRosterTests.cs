using Medabots.Rom.Layout;
using Medabots.Rom.Parts;
using Medabots.Rom.Text;
using Xunit;

namespace Medabots.Rom.Tests;

public sealed class RomRosterTests
{
    [Theory]
    [InlineData(TestRomLocator.RokushoEuropeHeader)]
    [InlineData(TestRomLocator.MetabeeEuropeHeader)]
    public void Resolve_OriginalRom_Reports120BotsAndTinpet120(string header)
    {
        var rom = File.ReadAllBytes(TestRomLocator.FindRomByHeader(header));
        var layout = RomLayout.Resolve(rom);

        Assert.Equal(120, layout.BotCount);
        Assert.Equal(120, layout.TinpetId);
        Assert.Equal(481, layout.PartCount);
        Assert.Equal("METABEE", layout.ReadBotName(rom, 43, new MedabotsTextCodec()));
    }

    [Fact]
    public async Task ExpandedRom_ReadsEveryBotPartAndName()
    {
        // Built by the expansion project (tools/build.py); skipped when it is not around.
        var path = TestRomLocator.FindOptionalFile(Path.Combine("build", "Medabots_expanded.gba"));
        if (path is null)
        {
            return;
        }

        var rom = await RomFile.LoadAsync(path);
        var layout = rom.Layout;
        var codec = new MedabotsTextCodec();

        Assert.True(layout.BotCount > 120, $"expected an expanded roster, got {layout.BotCount}");
        Assert.True(layout.TinpetId >= layout.BotCount);
        Assert.Equal(layout.PartCount, new PartTableReader().ReadAll(rom).Count);
        Assert.Equal("METABEE", layout.ReadBotName(rom.Data, 43, codec));
        Assert.Equal("VALENTORERO", layout.ReadBotName(rom.Data, 140, codec));
        Assert.Null(layout.ReadBotName(rom.Data, layout.BotCount, codec));

        // Shops edited by the expansion build no longer match the content signature.
        var shops = new Shops.ShopTableReader().ReadAll(rom);
        Assert.Equal(15, shops.Count);
    }

    [Theory]
    [InlineData(TestRomLocator.RokushoEuropeHeader, 0x483904)]
    [InlineData(TestRomLocator.MetabeeEuropeHeader, 0x4838E4)]
    public void ShopTable_ComesFromGameCode(string header, int expected)
    {
        var rom = new RomFile("rom.gba", File.ReadAllBytes(TestRomLocator.FindRomByHeader(header)));
        Assert.Equal(expected, new Shops.ShopTableReader().FindTableOffset(rom));
    }
}
