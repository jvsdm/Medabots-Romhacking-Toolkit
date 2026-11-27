namespace Medabots.Rom.Tests;

internal static class TestRomLocator
{
    public const string RokushoEuropeHeader = "MEDABOTSRKSVA9BPE9";
    public const string MetabeeEuropeHeader = "MEDABOTSMTBVA8BPE9";
    public const string RokushoUsaHeader = "MEDABOTSRKSVA9BEE9";

    public static string FindWorkspaceRom() => FindRomByHeader(RokushoEuropeHeader);

    // Identifies the ROM by its header instead of a fixed file name, walking up from the test binaries.
    public static string FindRomByHeader(string header)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            foreach (var candidate in directory.EnumerateFiles("*.gba"))
            {
                if (HasHeader(candidate.FullName, header))
                {
                    return candidate.FullName;
                }
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not find a local ROM with header {header} for integration testing.");
    }

    // A file at a relative path inside any ancestor of the test binaries, or null.
    public static string? FindOptionalFile(string relativePath)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private static bool HasHeader(string path, string header)
    {
        using var stream = File.OpenRead(path);
        if (stream.Length < 0xA0 + header.Length)
        {
            return false;
        }

        var buffer = new byte[header.Length];
        stream.Seek(0xA0, SeekOrigin.Begin);
        stream.ReadExactly(buffer);
        return System.Text.Encoding.ASCII.GetString(buffer) == header;
    }
}
