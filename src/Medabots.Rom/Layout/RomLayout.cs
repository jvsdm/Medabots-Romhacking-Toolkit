using System.Buffers.Binary;

namespace Medabots.Rom.Layout;

/// <summary>
/// Data table addresses resolved from the game's own code, so every regional build
/// (Metabee/Rokusho, EU/US) and expanded ROMs whose code was repointed are read correctly.
/// </summary>
public sealed partial class RomLayout
{
    private const int GbaRomBase = 0x08000000;
    private const int CodeSearchEnd = 0x0E0000;

    private readonly Dictionary<string, int> _tables;

    private RomLayout(Dictionary<string, int> tables)
    {
        _tables = tables;
    }

    public IReadOnlyDictionary<string, int> Tables => _tables;

    public static RomLayout Resolve(ReadOnlySpan<byte> rom)
    {
        var tables = new Dictionary<string, int>(StringComparer.Ordinal);
        var code = rom[..Math.Min(rom.Length, CodeSearchEnd)];

        foreach (var signature in RomTableSignatures.All.Concat(NameTableSignatures).Concat(MapTableSignatures))
        {
            if (tables.ContainsKey(signature.Table))
            {
                continue;
            }

            var match = FindUnique(code, signature.Bytes);
            if (match < 0)
            {
                continue;
            }

            var ldrAddress = match + signature.LdrOffset;
            var instruction = BinaryPrimitives.ReadUInt16LittleEndian(rom[ldrAddress..]);
            var literalAddress = ((ldrAddress + 4) & ~3) + ((instruction & 0xFF) * 4);
            var literal = BinaryPrimitives.ReadInt32LittleEndian(rom[literalAddress..]);
            tables[signature.Table] = literal - GbaRomBase + signature.Delta;
        }

        if (tables.Count == 0)
        {
            // No game code recognized at all (e.g. a synthetic test image): fall back to the
            // Rokusho (E) reference layout that MedabotsRomSchema documents.
            return new RomLayout(ReferenceTables(rom));
        }

        foreach (var derived in RomTableSignatures.Derived)
        {
            // A table found through its own code literal wins (it may have been moved on its own).
            if (!tables.ContainsKey(derived.Table) && tables.TryGetValue(derived.BaseTable, out var baseOffset))
            {
                tables[derived.Table] = baseOffset + derived.Delta;
            }
        }

        return new RomLayout(tables).WithRoster(code).WithMaps(rom).WithPortraits(rom);
    }

    private static Dictionary<string, int> ReferenceTables(ReadOnlySpan<byte> rom)
    {
        var tables = typeof(Metadata.MedabotsRomSchema).GetFields()
            .Where(field => field.IsLiteral && field.Name.EndsWith("Offset", StringComparison.Ordinal))
            .ToDictionary(field => field.Name[..^"Offset".Length], field => (int)field.GetRawConstantValue()!, StringComparer.Ordinal);
        var partTable = rom.IndexOf(Metadata.MedabotsRomSchema.PartTable.Signature);
        if (partTable >= 0)
        {
            tables["PartTable"] = partTable;
        }

        return tables;
    }

    public bool TryGet(string table, out int offset) => _tables.TryGetValue(table, out offset);

    private int Get(string table) => _tables.TryGetValue(table, out var offset)
        ? offset
        : throw new InvalidDataException($"Could not locate the {table} in this ROM: no code signature matched.");

    internal static int FindUniquePattern(ReadOnlySpan<byte> haystack, (byte Value, bool Fixed)[] pattern) =>
        FindUnique(haystack, pattern);

    // First match wins only if it is the sole match; ambiguous signatures are skipped.
    private static int FindUnique(ReadOnlySpan<byte> haystack, (byte Value, bool Fixed)[] pattern)
    {
        var found = -1;
        var anchor = Array.FindIndex(pattern, entry => entry.Fixed);
        for (var i = 0; i <= haystack.Length - pattern.Length; i += 2)
        {
            if (anchor >= 0 && haystack[i + anchor] != pattern[anchor].Value)
            {
                continue;
            }

            var matches = true;
            for (var j = 0; j < pattern.Length; j++)
            {
                if (pattern[j].Fixed && haystack[i + j] != pattern[j].Value)
                {
                    matches = false;
                    break;
                }
            }

            if (!matches)
            {
                continue;
            }

            if (found >= 0)
            {
                return -1;
            }

            found = i;
        }

        return found;
    }
}
