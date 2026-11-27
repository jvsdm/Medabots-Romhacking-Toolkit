using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Medabots.Rom.Metadata;

/// <summary>
/// Names of the fields that have a fixed list: technique, medal class, speciality, leg type, gender and drive effect.
/// One source for every screen (the Parts editor and the Decomp Project dropdowns), so both show the same names.
/// The names are the game's own (medabots.catalog.json through <see cref="MedabotsMetadata"/>). With a decomp folder,
/// its include/constants/skills.h sets the technique ids and their count; a technique with no game name shows its
/// identifier in title case.
/// </summary>
public sealed class FieldCatalog
{
    /// <summary>Medal classes are the part attributes 0..26 (26 = Nothing: no medal has that class).</summary>
    public const int MedalClassCount = 27;

    public const int FirstLegType = 100;

    private const int LegTypeCount = 7;

    private static readonly string[] GenderNames = ["Male", "Female"];

    private static readonly string[] EffectNames =
        ["none", "shock", "vacuum", "booster", "scope", "level_drain", "detect", "speed", "force_plant", "force_up"];

    private static readonly ConcurrentDictionary<string, FieldCatalog> ByRoot = new(StringComparer.OrdinalIgnoreCase);

    private static readonly FieldCatalog Builtin = new(MedabotsMetadata.Default, null);

    private readonly MedabotsMetadata _meta;
    private readonly string[] _techniques;

    public FieldCatalog(MedabotsMetadata meta, string? decompRoot)
    {
        _meta = meta ?? throw new ArgumentNullException(nameof(meta));
        _techniques = ReadTechniques(meta, decompRoot);
    }

    /// <summary>The catalog for a decomp folder (null: the names built into the program). Cached per folder.</summary>
    public static FieldCatalog For(string? decompRoot) =>
        string.IsNullOrEmpty(decompRoot)
            ? Builtin
            : ByRoot.GetOrAdd(decompRoot, root => new FieldCatalog(MedabotsMetadata.Default, root));

    public int TechniqueCount => _techniques.Length;

    public string TechniqueName(int id) =>
        id >= 0 && id < _techniques.Length ? _techniques[id] : $"Unknown technique #{id}";

    public int SpecialityCount => _meta.Catalog.Specialities.Count;

    public string SpecialityName(int id) => _meta.GetSpecialityName(id);

    public IReadOnlyList<int> LegTypeIds { get; } = Enumerable.Range(FirstLegType, LegTypeCount).ToList();

    public string LegTypeName(int id) => _meta.GetLegTypeName(id);

    public string GenderName(int id) => id >= 0 && id < GenderNames.Length ? GenderNames[id] : $"Unknown gender #{id}";

    public IReadOnlyList<string> Effects { get; } = EffectNames;

    public string EffectName(int id) => id >= 0 && id < EffectNames.Length ? EffectNames[id] : $"Unknown effect #{id}";

    public string MedalClassName(int id) => _meta.GetPartAttributeName(id);

    /// <summary>The medal class with its medal in brackets ("Grapple  (Kuwagata Medal)"); class 26 has no medal.</summary>
    public string MedalClassLabel(int id) => id < MedalClassCount - 1
        ? $"{_meta.GetPartAttributeName(id)}  ({_meta.GetMedalName(id)} Medal)"
        : _meta.GetPartAttributeName(id);

    private static string[] ReadTechniques(MedabotsMetadata meta, string? decompRoot)
    {
        var builtin = meta.Catalog.Techniques;
        var header = FindSkillsHeader(decompRoot);
        if (header is null)
        {
            return builtin.ToArray();
        }

        var text = File.ReadAllText(header);
        var start = text.IndexOf("SKILL_SWORD", StringComparison.Ordinal);
        var end = text.IndexOf("SKILL_COUNT", StringComparison.Ordinal);
        if (start < 0 || end < start)
        {
            return builtin.ToArray();
        }

        return Regex.Matches(text[start..end], @"SKILL_(\w+),")
            .Select((m, id) => id < builtin.Count && !string.IsNullOrWhiteSpace(builtin[id])
                ? builtin[id]
                : TitleCase(m.Groups[1].Value))
            .ToArray();
    }

    // The decomp's own header: the folder's decomp_rok build first, else the newest build that has one.
    private static string? FindSkillsHeader(string? root)
    {
        if (string.IsNullOrEmpty(root))
        {
            return null;
        }

        var fixedPath = Path.Combine(root, "build", "scratch", "decomp_rok", "include", "constants", "skills.h");
        if (File.Exists(fixedPath))
        {
            return fixedPath;
        }

        var scratch = Path.Combine(root, "build", "scratch");
        if (!Directory.Exists(scratch))
        {
            return null;
        }

        return Directory.GetDirectories(scratch)
            .Select(d => Path.Combine(d, "include", "constants", "skills.h"))
            .Where(File.Exists)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();
    }

    private static string TitleCase(string identifier) => string.Join(' ', identifier.Split('_')
        .Where(word => word.Length > 0)
        .Select(word => char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant()));
}
