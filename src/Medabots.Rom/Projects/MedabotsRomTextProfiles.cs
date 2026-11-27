using System.Text;

namespace Medabots.Rom.Metadata;

public static class MedabotsRomTextProfiles
{
    private static readonly IReadOnlyList<MedabotsRomTextProfile> Profiles =
    [
        new("MEDABOTSRKSVA9BPE9", "Medabots Rokusho Version (E)", "MEDABOTSRKSVA9BPE9", new MedabotsRomAddresses(0x47DF44, 0x7F5500, 0x7852F4, 0x3C1BA0, 0xF6, 0x769EE4, 0x8F0)),
        new("MEDABOTSRKSVA9BEE9", "Medabots Rokusho Version (U)", "MEDABOTSRKSVA9BEE9", new MedabotsRomAddresses(0x47E45C, 0x7F5500, 0x7840C0, 0x3C1A00, 0xF6, 0x768CB0, 0x8F0)),
        new("MEDABOTSMTBVA8BEE9", "Medabots Metabee Version (U)", "MEDABOTSMTBVA8BEE9", new MedabotsRomAddresses(0x47E43C, 0x7F5500, 0x78409B, 0x3C19E0, 0xF6, 0x769EE4, 0x8F0)),
        new("MEDABOTSMTBVA8BPE9", "Medabots Metabee Version (E)", "MEDABOTSMTBVA8BPE9", new MedabotsRomAddresses(0x47DF24, 0x7F5500, 0x7852CF, 0x3C1B80, 0xF6, 0x769EC0, 0x8F0))
    ];

    public static IReadOnlyList<MedabotsRomTextProfile> All => Profiles;

    // Profiles of decompilation builds (the retail one with the addresses of the build's layout file), by id.
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, MedabotsRomTextProfile> Builds =
        new(StringComparer.Ordinal);

    public static MedabotsRomTextProfile? FindById(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        return Profiles.FirstOrDefault(profile => string.Equals(profile.Id, id, StringComparison.Ordinal))
               ?? Builds.GetValueOrDefault(id);
    }

    /// <summary>
    /// The profile of a loaded ROM. A decompilation build keeps the retail header but moves every block, so when its
    /// layout file gives the text / battle / event / starter addresses (tools/decomp_layout.py "addresses") the result
    /// is the retail profile with those addresses, registered under "&lt;retail id&gt;+decomp:&lt;rom file&gt;".
    /// </summary>
    public static MedabotsRomTextProfile? Detect(RomFile romFile)
    {
        ArgumentNullException.ThrowIfNull(romFile);
        var retail = Detect(romFile.Data);
        if (retail is null || romFile.Layout.ProfileAddresses is not { } a)
        {
            return retail;
        }

        int Get(string key, int fallback) => a.TryGetValue(key, out var v) ? v : fallback;
        var r = retail.Addresses;
        var build = retail with
        {
            Id = $"{retail.Id}+decomp:{Path.GetFileName(Path.GetDirectoryName(romFile.FilePath))}",
            Name = $"{retail.Name} (decomp build)",
            Addresses = new MedabotsRomAddresses(
                Get("TextPointerTableOffset", r.TextPointerTableOffset), Get("TextDumpOffset", r.TextDumpOffset),
                Get("StarterOffset", r.StarterOffset), Get("BattlePointerTableOffset", r.BattlePointerTableOffset),
                Get("BattleCount", r.BattleCount), Get("EventTableOffset", r.EventTableOffset),
                Get("EventCount", r.EventCount)),
        };
        Builds[build.Id] = build;
        return build;
    }

    public static MedabotsRomTextProfile? Detect(ReadOnlySpan<byte> romData)
    {
        var signature = ReadHeaderSignature(romData);
        return Profiles.FirstOrDefault(profile => string.Equals(profile.HeaderSignature, signature, StringComparison.Ordinal));
    }

    public static string ReadHeaderSignature(ReadOnlySpan<byte> romData)
    {
        if (romData.Length < MedabotsRomSchema.HeaderOffset + MedabotsRomSchema.HeaderLength)
        {
            throw new ArgumentException("The ROM data is too small to contain a valid GBA header.", nameof(romData));
        }

        return Encoding.ASCII.GetString(romData.Slice(MedabotsRomSchema.HeaderOffset, MedabotsRomSchema.HeaderLength)).TrimEnd('\0', ' ');
    }
}
