using Medabots.Rom.Expansion;
using Medabots.Rom.Parts;
using Xunit;

namespace Medabots.Rom.Tests;

public sealed class RomExpanderTests
{
    private const string ShingataKabutoHeader = RomExpander.ShingataKabutoHeader;

    [Fact]
    public void Build_RejectsSwappedRoms()
    {
        var metabee = File.ReadAllBytes(TestRomLocator.FindRomByHeader(TestRomLocator.MetabeeEuropeHeader));
        var rokusho = File.ReadAllBytes(TestRomLocator.FindWorkspaceRom());

        Assert.Throws<InvalidDataException>(() => RomExpander.Build(rokusho, metabee));
        Assert.Throws<InvalidDataException>(() => RomExpander.Build(metabee, rokusho));
    }

    [Fact]
    public async Task Build_ProducesAReadableExpandedRom()
    {
        var shingataPath = TryFindRom(ShingataKabutoHeader);
        if (shingataPath is null)
        {
            return;
        }

        var metabee = File.ReadAllBytes(TestRomLocator.FindRomByHeader(TestRomLocator.MetabeeEuropeHeader));
        var result = RomExpander.Build(metabee, File.ReadAllBytes(shingataPath));
        var rom = new RomFile("expanded.gba", result.Rom);

        Assert.Equal(0x1000000, result.Rom.Length);
        Assert.Equal(result.BotCount, rom.Layout.BotCount);
        Assert.Equal(236, rom.Layout.TinpetId);
        Assert.Equal(rom.Layout.PartCount, new PartTableReader().ReadAll(rom).Count);
        Assert.Equal("VALENTORERO", rom.Layout.ReadBotName(rom.Data, 140, new Text.MedabotsTextCodec()));

        // Same bytes as the Python reference build (tools/build.py --out build/reference_expanded.gba).
        var reference = TestRomLocator.FindOptionalFile(Path.Combine("build", "reference_expanded.gba"));
        if (reference is not null)
        {
            var expected = await File.ReadAllBytesAsync(reference);
            var firstDifference = FirstDifference(expected, result.Rom);
            Assert.True(firstDifference < 0, $"first difference from the Python build at 0x{firstDifference:X}");
        }
    }

    [Fact]
    public void Build_WithoutShingata_IsTheExpandedEuBase()
    {
        var metabee = File.ReadAllBytes(TestRomLocator.FindRomByHeader(TestRomLocator.MetabeeEuropeHeader));
        var result = RomExpander.Build(metabee);
        var rom = new RomFile("base.gba", result.Rom);

        Assert.Equal(121, result.BotCount);
        Assert.Equal(121, rom.Layout.BotCount);
        Assert.Equal(236, rom.Layout.TinpetId);

        // tools/build.py --sem-shingata --sem-arte --out build/reference_eu_only.gba
        var reference = TestRomLocator.FindOptionalFile(Path.Combine("build", "reference_eu_only.gba"));
        if (reference is not null)
        {
            var firstDifference = FirstDifference(File.ReadAllBytes(reference), result.Rom);
            Assert.True(firstDifference < 0, $"first difference from the Python build at 0x{firstDifference:X}");
        }
    }

    private static int FirstDifference(byte[] expected, byte[] actual)
    {
        var length = Math.Min(expected.Length, actual.Length);
        for (var i = 0; i < length; i++)
        {
            if (expected[i] != actual[i])
            {
                return i;
            }
        }

        return expected.Length == actual.Length ? -1 : length;
    }

    private static string? TryFindRom(string header)
    {
        try
        {
            return TestRomLocator.FindRomByHeader(header);
        }
        catch (FileNotFoundException)
        {
            return null;
        }
    }
}
