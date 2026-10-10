using Medabots.Rom.Expansion;
using Medabots.Rom.Parts;
using Medabots.Rom.Text;
using Xunit;

namespace Medabots.Rom.Tests;

public sealed class BotAdderTests
{
    private const string Json = """
        {
         "name": "TESTBOT",
         "graphics_from": 40,
         "recolor": { "#f8f070": "#d8a878", "#f8d800": "#9a6c46" },
         "parts": {
          "head": { "name": "TESTHEAD", "stats": { "medal_compat": 1, "technique": 1, "speciality": 1, "female": 0, "armor": 30, "success": 30, "power": 20, "chain_reaction": 0, "uses_or_charge": 4, "radiation": 0, "u10": 40, "u11": 1, "u12": 1, "u13": 0, "u14": 0, "u15": 0 } },
          "right_arm": { "name": "TESTRIGHT", "stats": { "medal_compat": 1, "technique": 2, "speciality": 0, "female": 0, "armor": 30, "success": 30, "power": 20, "chain_reaction": 0, "uses_or_charge": 4, "radiation": 0, "u10": 40, "u11": 1, "u12": 1, "u13": 0, "u14": 0, "u15": 0 } },
          "left_arm": { "name": "TESTLEFT", "stats": { "medal_compat": 1, "technique": 3, "speciality": 0, "female": 0, "armor": 30, "success": 30, "power": 20, "chain_reaction": 0, "uses_or_charge": 4, "radiation": 0, "u10": 40, "u11": 1, "u12": 1, "u13": 0, "u14": 0, "u15": 0 } },
          "legs": { "name": "TESTLEGS", "stats": { "medal_compat": 1, "leg_type": 102, "speciality": 0, "female": 0, "armor": 30, "propulsion": 20, "evasion": 20, "defense": 40, "proximity": 10, "remoteness": 10, "u10": 40, "u11": 1, "u12": 1, "u13": 0, "u14": 0, "u15": 0 } }
         }
        }
        """;

    [Fact]
    public void Add_AppendsTheNextBotAndRaisesEveryCountConstant()
    {
        var metabee = File.ReadAllBytes(TestRomLocator.FindRomByHeader(TestRomLocator.MetabeeEuropeHeader));
        var expanded = RomExpander.Build(metabee).Rom;
        var definition = BotDefinition.Parse(Json);

        var result = BotAdder.Add(expanded, definition);
        var rom = new RomFile("added.gba", result.Rom);

        Assert.Equal(121, result.BotId);
        Assert.Equal(122, rom.Layout.BotCount);
        Assert.Equal(236, rom.Layout.TinpetId);
        Assert.Equal("TESTBOT", rom.Layout.ReadBotName(rom.Data, 121, new MedabotsTextCodec()));
        Assert.Equal("METABEE", rom.Layout.ReadBotName(rom.Data, 43, new MedabotsTextCodec()));
        var parts = new PartTableReader().ReadAll(rom);
        Assert.Equal(rom.Layout.PartCount, parts.Count);
        Assert.Equal(102, parts[121 * 4 + 3].TechniqueOrLegType);

        // The count constants must match a ROM generated from scratch with 122 bots.
        var manifest = new ExpansionManifest
        {
            Bots =
            [
                .. ExpansionManifest.Default.Bots.Where(bot => bot.ShingataBot is null),
                new ExpansionBot { Id = 121, Name = "X", PartNames = ["A", "B", "C", "D"], GraphicsFrom = 40,
                    Stats = [.. Enumerable.Range(0, 4).Select(_ => Enumerable.Repeat(0, 16).ToList())] },
            ],
        };
        var reference = RomExpander.Build(metabee, null, null, manifest).Rom;
        foreach (var pc in ExpansionPatchSites.LastBotSites.Concat(ExpansionPatchSites.ShopScrollAddSites).Concat(ExpansionPatchSites.LastPageSites))
        {
            Assert.Equal(reference[pc], result.Rom[pc]);
        }

        foreach (var (address, _) in ExpansionPatchSites.PageTotalBytes)
        {
            Assert.Equal(reference[address], result.Rom[address]);
        }

        foreach (var at in ExpansionPatchSites.ShopScrollLiterals.Append(ExpansionPatchSites.ShopItemsLastLiteral))
        {
            Assert.Equal(BitConverter.ToUInt32(reference, at), BitConverter.ToUInt32(result.Rom, at));
        }

        // Large images: same molds as Multikolor, recoloured palette for the main pieces.
        var legs = rom.Layout.CompositePreviewLegsAppearanceTableOffset;
        for (var i = 0; i < 8; i++)
        {
            Assert.Equal(BitConverter.ToUInt32(rom.Data, legs + 40 * 32 + 4 * i) & 0x3F, BitConverter.ToUInt32(rom.Data, legs + 121 * 32 + 4 * i) & 0x3F);
        }

        // Adding again keeps working.
        var twice = BotAdder.Add(result.Rom, definition);
        Assert.Equal(123, new RomFile("x.gba", twice.Rom).Layout.BotCount);
    }

    [Fact]
    public void Add_WorksOnAPlayedExpandedRom()
    {
        // The expansion project's working ROM (181 bots, own drawings, edited shop), when present.
        var path = TestRomLocator.FindOptionalFile(Path.Combine("build", "Medabots_expanded.gba"));
        if (path is null)
        {
            return;
        }

        var before = File.ReadAllBytes(path);
        var count = new RomFile("before.gba", before).Layout.BotCount;
        var result = BotAdder.Add(before, BotDefinition.Parse(Json));
        var rom = new RomFile("after.gba", result.Rom);

        Assert.Equal(count, result.BotId);
        Assert.Equal(count + 1, rom.Layout.BotCount);
        Assert.Equal(15, new Shops.ShopTableReader().ReadAll(rom).Count);
        Assert.Equal("TESTBOT", rom.Layout.ReadBotName(rom.Data, count, new MedabotsTextCodec()));
        Assert.Equal("VALENTORERO", rom.Layout.ReadBotName(rom.Data, 140, new MedabotsTextCodec()));
    }

    [Fact]
    public void ExampleBot_IsValid()
    {
        var path = TestRomLocator.FindOptionalFile(Path.Combine("examples", "bot_exemplo.json"));
        if (path is null)
        {
            return;
        }

        var metabee = File.ReadAllBytes(TestRomLocator.FindRomByHeader(TestRomLocator.MetabeeEuropeHeader));
        var result = BotAdder.Add(RomExpander.Build(metabee).Rom, BotDefinition.Parse(File.ReadAllText(path)));
        Assert.Contains(result.Log, line => line.Contains("4 recoloured palettes", StringComparison.Ordinal));
    }

    [Fact]
    public void Add_RefusesRomsThatAreNotExpanded()
    {
        var metabee = File.ReadAllBytes(TestRomLocator.FindRomByHeader(TestRomLocator.MetabeeEuropeHeader));
        Assert.Throws<InvalidDataException>(() => BotAdder.Add(metabee, BotDefinition.Parse(Json)));
    }

    [Fact]
    public void Add_WithOwnArt_StoresThePicturesAsNewImages()
    {
        var metabee = File.ReadAllBytes(TestRomLocator.FindRomByHeader(TestRomLocator.MetabeeEuropeHeader));
        var expanded = RomExpander.Build(metabee).Rom;
        var source = BotArt.Slots(expanded, 43);
        // Metabee's own pictures, the head's first piece mirrored so the result is not just the old image.
        var art = source.ToDictionary(slot => slot.Name, slot => slot.Current);
        var head = source.First(slot => slot.Part == "cabeca" && slot.OwnPalette);
        art[head.Name] = Mirror(head.Current);
        var parsed = BotDefinition.Parse(Json);
        var definition = new BotDefinition { Name = parsed.Name, Parts = parsed.Parts, GraphicsFrom = 43, Art = art };

        var result = BotAdder.Add(expanded, definition);
        var added = BotArt.Slots(result.Rom, result.BotId);

        Assert.Equal(source.Count, added.Count);
        foreach (var slot in added.Where(slot => !slot.IsMini))
        {
            Assert.Equal(art[slot.Name].Argb, slot.Current.Argb);
        }

        // Metabee itself is untouched.
        foreach (var (before, after) in source.Zip(BotArt.Slots(result.Rom, 43)))
        {
            Assert.Equal(before.Current.Argb, after.Current.Argb);
        }

        var mini = added.First(slot => slot.IsMini).Current;
        Assert.Contains(mini.Argb, pixel => pixel != 0);
    }

    [Fact]
    public void ToJson_RoundTripsThroughParse()
    {
        var definition = BotDefinition.Parse(Json);
        var again = BotDefinition.Parse(definition.ToJson("art/testbot"));

        Assert.Equal(definition.Name, again.Name);
        Assert.Equal(definition.GraphicsFrom, again.GraphicsFrom);
        Assert.Equal("art/testbot", again.ArtFolder);
        Assert.Equal(definition.Recolor, again.Recolor);
        for (var k = 0; k < 4; k++)
        {
            Assert.Equal(definition.Parts[k].Name, again.Parts[k].Name);
            Assert.Equal(definition.Parts[k].Stats, again.Parts[k].Stats);
        }
    }

    [Fact]
    public void AddAll_AddsTheListInOrderToOneRom()
    {
        var metabee = File.ReadAllBytes(TestRomLocator.FindRomByHeader(TestRomLocator.MetabeeEuropeHeader));
        var expanded = RomExpander.Build(metabee).Rom;
        var first = BotDefinition.Parse(Json);
        var second = BotDefinition.Parse(Json.Replace("TESTBOT", "SECONDBOT"));

        var result = BotAdder.AddAll(expanded, [first, second]);
        var rom = new RomFile("all.gba", result.Rom);

        Assert.Equal(121, result.BotId);
        Assert.Equal(123, rom.Layout.BotCount);
        Assert.Equal("TESTBOT", rom.Layout.ReadBotName(rom.Data, 121, new MedabotsTextCodec()));
        Assert.Equal("SECONDBOT", rom.Layout.ReadBotName(rom.Data, 122, new MedabotsTextCodec()));
        // Same ROM as adding them one at a time.
        Assert.Equal(BotAdder.Add(BotAdder.Add(expanded, first).Rom, second).Rom, result.Rom);
    }

    [Fact]
    public void BotProject_RoundTripsAListAndReadsASingleBot()
    {
        var bot = BotDefinition.Parse(Json);
        var json = BotProject.ToJson([(bot, null), (bot, "list_art/02_TESTBOT")]);

        var bots = BotProject.Parse(json);

        Assert.Contains(BotProject.Format, json);
        Assert.Equal(2, bots.Count);
        Assert.Null(bots[0].ArtFolder);
        Assert.Equal("list_art/02_TESTBOT", bots[1].ArtFolder);
        Assert.Equal(bot.Parts[3].Stats, bots[1].Parts[3].Stats);
        Assert.Single(BotProject.Parse(Json));
    }

    [Fact]
    public void MedalCatalog_FollowsTheCompatibilityAttributeOrder()
    {
        // The New Medabot window labels Compatibility Attribute n with medal n (Grapple = Kuwagata, Shoot = Kabuto...).
        var metadata = Medabots.Rom.Metadata.MedabotsMetadata.Default;
        Assert.Equal("Shoot", metadata.GetPartAttributeName(1));
        Assert.Contains("kabuto", metadata.GetMedalName(1), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("kuwagata", metadata.GetMedalName(0), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("alien", metadata.GetMedalName(25), StringComparison.OrdinalIgnoreCase);
    }

    private static ArtImage Mirror(ArtImage image)
    {
        var argb = new uint[image.Argb.Length];
        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                argb[y * image.Width + x] = image.Argb[y * image.Width + image.Width - 1 - x];
            }
        }

        return image with { Argb = argb };
    }
}
