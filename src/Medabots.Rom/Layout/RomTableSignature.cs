namespace Medabots.Rom.Layout;

/// <summary>
/// A code fingerprint that locates a data table: a byte pattern around a Thumb
/// <c>ldr rX, [pc, #imm]</c> whose literal points at (or near) the table.
/// </summary>
internal sealed record RomTableSignature(string Table, string Pattern, int LdrOffset, int Delta)
{
    private (byte Value, bool Fixed)[]? _bytes;

    public (byte Value, bool Fixed)[] Bytes => _bytes ??= Pattern
        .Split(' ', StringSplitOptions.RemoveEmptyEntries)
        .Select(token => token == "??" ? ((byte)0, false) : (Convert.ToByte(token, 16), true))
        .ToArray();
}

/// <summary>A table that code never reads directly, stored at a fixed distance from a resolved one.</summary>
internal sealed record RomDerivedTable(string Table, string BaseTable, int Delta);
