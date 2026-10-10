using Medabots.Rom.Text;

namespace Medabots.Rom.Layout;

/// <summary>
/// Roster size read from the game's own code, so ROMs with more Medabots (an expanded part table
/// whose code constants were raised) are browsed in full. Original ROMs report 120 bots, Tinpet 120.
/// </summary>
public sealed partial class RomLayout
{
    public const int OriginalBotCount = 120;
    private const string NameTerminatorToken = "<ENDLST>";

    // Medabot validator (0x43820 in every release): four `cmp rX, #lastBot` (lastBot = 119 originally).
    private static readonly RomImmediateSignature LastBotSignature = new(
        "12 06 12 0E 1B 06 1B 0E 24 06 24 0E 1E 28 09 D0 ?? 29 07 D8 ?? 2A 05 D8 ?? 2B 03 D8 ?? 2C 01 D8",
        ImmediateOffset: 16);

    // "Create a bare Medabot" (0x42238): `adds r2, #tinpet` with gender in r2 (Tinpet M = 120 originally).
    private static readonly RomImmediateSignature TinpetSignature = new(
        "70 B5 00 06 03 0E 00 24 01 25 2B 40 1A 1C ?? 32 06 49 1E 26 08 78 00 28 0A D1 0D 70 4B 70 8E 70",
        ImmediateOffset: 14);

    // GetBotName (0x47BDC) and GetPartName (0x47BEC) sit back to back in every release.
    internal static readonly RomTableSignature[] NameTableSignatures =
    [
        new("BotNameTable", "02 49 80 00 40 18 00 68 70 47 00 00 ?? ?? ?? ?? 03 4A 89 00 09 18 89 00 89 18 08 68 70 47", 0, 0x0),
        new("PartNameTable", "02 49 80 00 40 18 00 68 70 47 00 00 ?? ?? ?? ?? 03 4A 89 00 09 18 89 00 89 18 08 68 70 47", 16, 0x0),
        // Event script database base (header 00 40, pointer table at +0x4000); 0x4A3B6 in every release.
        // Literal-based, so databases relocated by the editor (which repoints these literals) are followed.
        new("EventScriptDatabase", "0E 39 F2 22 52 01 10 1C 08 80 ?? 49 ?? 48 08 60 ?? 48 07 60 ?? 49 ?? 48", 12, 0x0),
        // Shop table (15 x 4 bot IDs), loaded at 0x61B9A in every release: found even after shops were edited.
        new("ShopTable", "?? 48 01 78 89 00 20 06 06 16 89 19 ?? 48 09 18 08 78 ?? 28 2A D0 ?? 28", 12, 0x0),
    ];

    /// <summary>Number of real Medabots (IDs 0..BotCount-1).</summary>
    public int BotCount { get; private init; } = OriginalBotCount;

    /// <summary>Male Tinpet ID; female is <c>TinpetId + 1</c>. Equals BotCount in original ROMs.</summary>
    public int TinpetId { get; private init; } = OriginalBotCount;

    /// <summary>Entries read from the part table: 4 per bot plus the trailing entry original ROMs have.</summary>
    public int PartCount => BotCount * 4 + 1;

    public int BotNameTableOffset => Get("BotNameTable");
    public int EventScriptDatabaseOffset => Get("EventScriptDatabase");
    public int ShopTableOffset => Get("ShopTable");
    public int PartNameTableOffset => Get("PartNameTable");

    public bool IsRealBot(int botId) => botId >= 0 && botId < BotCount;

    public string? ReadBotName(ReadOnlySpan<byte> rom, int botId, MedabotsTextCodec codec) =>
        IsRealBot(botId) ? ReadName(rom, BotNameTableOffset + botId * 4, codec) : null;

    public string? ReadPartName(ReadOnlySpan<byte> rom, int partId, MedabotsTextCodec codec) =>
        partId >= 0 && partId < BotCount * 4 ? ReadName(rom, PartNameTableOffset + partId * 4, codec) : null;

    private static string? ReadName(ReadOnlySpan<byte> rom, int pointerOffset, MedabotsTextCodec codec)
    {
        if (pointerOffset < 0 || pointerOffset + 4 > rom.Length)
        {
            return null;
        }

        var pointer = BitConverter.ToInt32(rom.Slice(pointerOffset, 4)) - GbaRomBase;
        if (pointer < 0 || pointer >= rom.Length)
        {
            return null;
        }

        // Names end in 0xFE, which the message codec renders as a trailing token.
        var text = codec.DecodeMessage(rom, pointer);
        return text.EndsWith(NameTerminatorToken, StringComparison.Ordinal) ? text[..^NameTerminatorToken.Length] : text;
    }

    private RomLayout WithRoster(ReadOnlySpan<byte> code)
    {
        var lastBot = LastBotSignature.Read(code);
        var tinpet = TinpetSignature.Read(code);
        return new RomLayout(_tables)
        {
            BotCount = lastBot is { } last ? last + 1 : OriginalBotCount,
            TinpetId = tinpet ?? OriginalBotCount,
        };
    }
}

/// <summary>A code fingerprint whose interesting part is an 8-bit Thumb immediate inside the pattern.</summary>
internal sealed record RomImmediateSignature(string Pattern, int ImmediateOffset)
{
    private (byte Value, bool Fixed)[]? _bytes;

    private (byte Value, bool Fixed)[] Bytes => _bytes ??= Pattern
        .Split(' ', StringSplitOptions.RemoveEmptyEntries)
        .Select(token => token == "??" ? ((byte)0, false) : (Convert.ToByte(token, 16), true))
        .ToArray();

    public int? Read(ReadOnlySpan<byte> code)
    {
        var match = RomLayout.FindUniquePattern(code, Bytes);
        return match < 0 ? null : code[match + ImmediateOffset];
    }
}
