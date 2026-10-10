using Medabots.Rom.Layout;
using Medabots.Rom.Metadata;
using Xunit;

namespace Medabots.Rom.Tests;

public sealed class RomLayoutTests
{
    [Fact]
    public void Resolve_RokushoEurope_MatchesEverySchemaReferenceOffset()
    {
        var layout = RomLayout.Resolve(File.ReadAllBytes(TestRomLocator.FindWorkspaceRom()));

        var schemaOffsets = typeof(MedabotsRomSchema).GetFields()
            .Where(field => field.IsLiteral && field.Name.EndsWith("Offset", StringComparison.Ordinal))
            .ToDictionary(field => field.Name, field => (int)field.GetRawConstantValue()!);
        var layoutProperties = typeof(RomLayout).GetProperties()
            .Where(property => property.PropertyType == typeof(int) && schemaOffsets.ContainsKey(property.Name))
            .ToList();

        Assert.NotEmpty(layoutProperties);
        Assert.All(layoutProperties, property =>
            Assert.Equal($"{property.Name}=0x{schemaOffsets[property.Name]:X}", $"{property.Name}=0x{(int)property.GetValue(layout)!:X}"));
        Assert.Equal(0x3B841C, layout.PartTableOffset);
    }

    [Fact]
    public void Resolve_MetabeeEurope_FindsTablesShiftedFromTheRokushoReference()
    {
        var layout = RomLayout.Resolve(File.ReadAllBytes(TestRomLocator.FindRomByHeader(TestRomLocator.MetabeeEuropeHeader)));

        // Values cross-checked by hand (tools/ in the expansion project) against the Metabee (E) code.
        Assert.Equal(0x3B83FC, layout.PartTableOffset);
        Assert.Equal(0x3B1A08, layout.CompositeBattleSpritePointerTableOffset);
        Assert.Equal(0x3BEB94, layout.CompositeBattleSpritePaletteFamilyTableOffset);
        Assert.Equal(0x07EE70, layout.CompositePreviewHeadAppearanceTableOffset);
        Assert.Equal(0x080550, layout.CompositePreviewLegsAppearanceTableOffset);
        Assert.Equal(0x3B62D8, layout.MedalInfoTableOffset);
        Assert.Equal(0x3D1D78, layout.CompositePreviewDescriptorPointerTableOffset);
        Assert.Equal(0x50F2F8, layout.CompositeBattleSpritePaletteDataOffset);
    }

    [Fact]
    public async Task PartTableReader_MetabeeEurope_ReadsMetabeePartsAtTheShiftedTable()
    {
        var romFile = await RomFile.LoadAsync(TestRomLocator.FindRomByHeader(TestRomLocator.MetabeeEuropeHeader));

        var parts = new Parts.PartTableReader().ReadAll(romFile);

        var missile = parts[43 * 4];   // Metabee head
        Assert.Equal(0x3B83FC + (43 * 4 * 16), missile.DataOffset);
        Assert.Equal((byte)6, missile.TechniqueOrLegType);   // Missile
        Assert.Equal((byte)35, missile.Armor);
        Assert.Equal((byte)24, missile.RateOfSuccessOrPropulsion);
        Assert.Equal((byte)32, missile.PowerOrEvasion);
    }

    [Fact]
    public void Resolve_FollowsRepointedCode()
    {
        var rom = File.ReadAllBytes(TestRomLocator.FindWorkspaceRom());
        var original = RomLayout.Resolve(rom).PartTableOffset;
        const int relocated = 0x00C00000;

        var oldLiteral = BitConverter.GetBytes(0x08000000 + original);
        var newLiteral = BitConverter.GetBytes(0x08000000 + relocated);
        for (var i = 0; i < 0x0A0000; i += 4)
        {
            if (rom.AsSpan(i, 4).SequenceEqual(oldLiteral))
            {
                newLiteral.CopyTo(rom, i);
            }
        }

        Assert.Equal(relocated, RomLayout.Resolve(rom).PartTableOffset);
    }
}
