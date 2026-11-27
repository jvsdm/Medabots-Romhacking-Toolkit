using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;

namespace Medabots.Rom.WPFEditor.Decomp;

/// <summary>
/// Runs tools/decomp_build.py in a decomp project (generator + make, into a new build/scratch folder). Shared by the
/// Build ROM button of the main window's project bar and the Decomp Project window (command-line use).
/// </summary>
public static class DecompBuild
{
    /// <summary>Runs the build; every output line goes to <paramref name="log"/> on the UI thread.</summary>
    /// <returns>The exit code, and the built ROM's full path (null unless the build succeeded).</returns>
    public static async Task<(int ExitCode, string? RomPath)> RunAsync(string root, string pythonCommand, Action<string> log)
    {
        var (exe, prefixArgs) = SplitCommand(pythonCommand);
        var psi = new ProcessStartInfo(exe)
        {
            WorkingDirectory = root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };
        foreach (var arg in prefixArgs)
        {
            psi.ArgumentList.Add(arg);
        }

        psi.ArgumentList.Add(Path.Combine(root, "tools", "decomp_build.py"));
        psi.Environment["PYTHONIOENCODING"] = "utf-8";
        Dispatch(() => log($"> {exe} tools/decomp_build.py"));
        using var process = Process.Start(psi) ?? throw new InvalidOperationException("could not start Python");
        string? romPath = null;
        process.OutputDataReceived += (_, a) =>
        {
            if (a.Data is null)
            {
                return;
            }

            if (a.Data.StartsWith("ROM: ", StringComparison.Ordinal))
            {
                romPath = a.Data[5..].Trim();
            }

            Dispatch(() => log(a.Data));
        };
        process.ErrorDataReceived += (_, a) =>
        {
            if (a.Data is not null)
            {
                Dispatch(() => log(a.Data));
            }
        };
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();
        var exitCode = process.ExitCode;
        var built = exitCode == 0 && romPath is not null ? Path.GetFullPath(Path.Combine(root, romPath)) : null;
        return (exitCode, built);
    }

    /// <summary>Runs a short tool of the project (tools/&lt;script&gt; args) and returns its exit code and whole output.</summary>
    public static async Task<(int ExitCode, string Output)> RunToolAsync(string root, string pythonCommand, string script,
        params string[] args)
    {
        var (exe, prefixArgs) = SplitCommand(pythonCommand);
        var psi = new ProcessStartInfo(exe)
        {
            WorkingDirectory = root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };
        foreach (var arg in prefixArgs)
        {
            psi.ArgumentList.Add(arg);
        }

        psi.ArgumentList.Add(Path.Combine(root, "tools", script));
        foreach (var arg in args)
        {
            psi.ArgumentList.Add(arg);
        }

        psi.Environment["PYTHONIOENCODING"] = "utf-8";
        using var process = Process.Start(psi) ?? throw new InvalidOperationException("could not start Python");
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return (process.ExitCode, ((await stdout) + (await stderr)).Trim());
    }

    /// <summary>The Python command as the user typed it: "py -3.12" splits into the program and its arguments.</summary>
    internal static (string Exe, string[] Args) SplitCommand(string command)
    {
        if (File.Exists(command) || !command.Contains(' '))
        {
            return (command.Length == 0 ? "python" : command, []);
        }

        var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return (parts[0], parts[1..]);
    }

    private static void Dispatch(Action action)
    {
        var dispatcher = System.Windows.Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            action();
        }
        else
        {
            dispatcher.Invoke(action);
        }
    }
}
