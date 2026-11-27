using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using Medabots.Rom.Events;
using Medabots.Rom.Metadata;
using Medabots.Rom.WPFEditor.Decomp;
using Medabots.Rom.WPFEditor.Dialogs;

namespace Medabots.Rom.WPFEditor;

/// <summary>
/// Decomp mode: the Events tab, the Maps "Linked Event Script" text and the Event scripts (.evt) window use the same
/// scripts, the files of data/events. A script reads from its .evt when the file changed after the open ROM was built
/// (or its id is past the ROM's table); edits in the Events tab are written back to the .evt (tools/events_text.py
/// --write) instead of being stored in the WPF project. Outside decomp mode nothing here runs.
/// </summary>
public partial class MainWindow
{
    /// <summary>Scripts of data/events that differ from the open ROM (assembled bytes), by id.</summary>
    private readonly Dictionary<short, byte[]> _decompEventBytes = [];

    private string? DecompEventsFolder()
    {
        if (!IsDecompMode() || ResolveDecompRoot() is not { } root)
        {
            return null;
        }

        var events = Path.Combine(root, "data", "events");
        return File.Exists(Path.Combine(events, "_opcodes.json")) ? events : null;
    }

    /// <summary>Runs tools/events_text.py with the given arguments (and standard input); returns exit code and output.</summary>
    private static (int Code, string Output) RunEventsText(string root, string? input, params string[] args)
    {
        var (exe, prefixArgs) = DecompBuild.SplitCommand(DecompProjectWindow.SavedPython());
        var psi = new ProcessStartInfo(exe)
        {
            WorkingDirectory = root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = input is not null,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };
        foreach (var arg in prefixArgs)
        {
            psi.ArgumentList.Add(arg);
        }

        psi.ArgumentList.Add(Path.Combine(root, "tools", "events_text.py"));
        foreach (var arg in args)
        {
            psi.ArgumentList.Add(arg);
        }

        psi.Environment["PYTHONIOENCODING"] = "utf-8";
        using var process = Process.Start(psi) ?? throw new InvalidOperationException("could not start Python");
        if (input is not null)
        {
            process.StandardInput.Write(input);
            process.StandardInput.Close();
        }

        var stderr = process.StandardError.ReadToEndAsync();
        var stdout = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return (process.ExitCode, (stdout + stderr.Result).Trim());
    }

    /// <summary>
    /// Reads the .evt scripts that may differ from the open ROM: ids past its table, and files written after the ROM.
    /// Those whose bytes match the ROM's script are dropped. Old event patches of the WPF project are moved into
    /// data/events first (an empty "End" placeholder never overwrites an existing .evt).
    /// </summary>
    private void LoadDecompEventOverrides(MedabotsRomTextProfile profile)
    {
        _decompEventBytes.Clear();
        var events = DecompEventsFolder();
        if (events is null || _session is null)
        {
            return;
        }

        var root = Path.GetFullPath(Path.Combine(events, "..", ".."));
        MoveProjectEventPatchesToFiles(root, events);

        var romTime = File.GetLastWriteTimeUtc(_session.RomFile.FilePath);
        var installed = EventScriptReader.ResolveInstalledEventTable(_session.RomFile, profile).EventCount;
        var changed = new HashSet<int>();
        var sharers = new List<(int Id, int Owner)>();
        foreach (var json in Directory.EnumerateFiles(events, "*.json"))
        {
            if (!int.TryParse(Path.GetFileNameWithoutExtension(json), NumberStyles.None, CultureInfo.InvariantCulture, out var id))
            {
                continue;
            }

            var evt = Path.ChangeExtension(json, ".evt");
            var time = File.GetLastWriteTimeUtc(json);
            if (File.Exists(evt) && File.GetLastWriteTimeUtc(evt) > time)
            {
                time = File.GetLastWriteTimeUtc(evt);
            }

            if (id >= installed || time > romTime)
            {
                changed.Add(id);
            }
            else if (File.ReadAllText(json).Contains("shared_with", StringComparison.Ordinal))
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(json));
                sharers.Add((id, doc.RootElement.GetProperty("shared_with").GetInt32()));
            }
        }

        changed.UnionWith(sharers.Where(s => changed.Contains(s.Owner)).Select(s => s.Id));
        if (changed.Count == 0)
        {
            return;
        }

        var (code, output) = RunEventsText(root, null, ["--bytes", .. changed.Order().Select(i => i.ToString(CultureInfo.InvariantCulture))]);
        if (code != 0)
        {
            throw new InvalidOperationException("tools/events_text.py --bytes: " + output);
        }

        using var result = JsonDocument.Parse(output);
        foreach (var entry in result.RootElement.EnumerateObject())
        {
            var id = short.Parse(entry.Name, CultureInfo.InvariantCulture);
            var bytes = Convert.FromHexString(entry.Value.GetString()!);
            if (id < installed && SameAsRomScript(profile, id, bytes))
            {
                continue;
            }

            _decompEventBytes[id] = bytes;
            EnsureEventBrowserItemExists(id);
        }
    }

    private bool SameAsRomScript(MedabotsRomTextProfile profile, short id, byte[] bytes)
    {
        try
        {
            return ResolvePatchedEventBytes(_session!.RomFile, profile, id).AsSpan().SequenceEqual(bytes);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or IndexOutOfRangeException)
        {
            return false;
        }
    }

    /// <summary>Event patches left in the WPF project (from before decomp mode wrote .evt files) go to data/events.</summary>
    private void MoveProjectEventPatchesToFiles(string root, string events)
    {
        if (_eventProjectScriptPatches.Count == 0)
        {
            return;
        }

        foreach (var (id, bytes) in _eventProjectScriptPatches.OrderBy(p => p.Key).ToList())
        {
            var placeholder = bytes is [MedabotsRomSchema.EventEndOpcode];
            if (!(placeholder && File.Exists(Path.Combine(events, $"{id:D4}.evt"))))
            {
                var (code, output) = RunEventsText(root, Convert.ToHexString(bytes), "--write", id.ToString(CultureInfo.InvariantCulture));
                if (code != 0)
                {
                    continue;   // stays in the project (the build tries again and shows the error)
                }
            }

            _eventProjectScriptPatches.Remove(id);
        }
    }

    /// <summary>Decomp mode: makes the next free script (tools/events_text.py --new) and returns its id; null otherwise.</summary>
    private short? NewDecompEventScript()
    {
        var events = DecompEventsFolder();
        var profile = RequireProfile();
        if (events is null || profile is null)
        {
            return null;
        }

        var root = Path.GetFullPath(Path.Combine(events, "..", ".."));
        var (code, output) = RunEventsText(root, null, "--new");
        var match = System.Text.RegularExpressions.Regex.Match(output, @"roteiro novo: (\d+)");
        if (code != 0 || !match.Success)
        {
            throw new InvalidOperationException("tools/events_text.py --new: " + output);
        }

        var id = short.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        LoadDecompEventOverrides(profile);
        foreach (var window in System.Windows.Application.Current.Windows.OfType<DecompEventTextWindow>())
        {
            window.ReloadFromDisk(id);
        }

        return id;
    }

    /// <summary>Writes an edited script to data/events/NNNN.evt. False when not in decomp mode (the caller uses the project).</summary>
    private bool TryWriteDecompEvent(short eventId, byte[] bytes)
    {
        var events = DecompEventsFolder();
        if (events is null)
        {
            return false;
        }

        var root = Path.GetFullPath(Path.Combine(events, "..", ".."));
        var (code, output) = RunEventsText(root, Convert.ToHexString(bytes), "--write", eventId.ToString(CultureInfo.InvariantCulture));
        if (code != 0)
        {
            throw new InvalidOperationException(output);
        }

        _decompEventBytes[eventId] = bytes.ToArray();
        _eventProjectScriptPatches.Remove(eventId);
        EnsureEventBrowserItemExists(eventId);
        UpdateEventBrowserPatchStatus(eventId);
        RefreshEventFilter();
        foreach (var window in System.Windows.Application.Current.Windows.OfType<DecompEventTextWindow>())
        {
            window.ReloadFromDisk(eventId);
        }

        return true;
    }

    /// <summary>
    /// The Event scripts window saved, created or deleted <paramref name="eventId"/>: reread data/events, drop the cached
    /// views and redraw the Events tab and the Maps panel.
    /// </summary>
    internal void ReloadDecompEvent(int eventId)
    {
        var profile = RequireProfile();
        if (profile is null || _session is null)
        {
            return;
        }

        _eventProjectScriptPatches.Remove((short)eventId);
        var before = _decompEventBytes.Keys.ToHashSet();
        LoadDecompEventOverrides(profile);
        var touched = before.Union(_decompEventBytes.Keys).Append((short)eventId).ToHashSet();
        foreach (var id in touched)
        {
            _eventCache.Remove(id);
            _eventViewCache.Remove(id);
            UpdateEventBrowserPatchStatus(id);
        }

        var events = DecompEventsFolder();
        var installed = EventScriptReader.ResolveInstalledEventTable(_session.RomFile, profile).EventCount;
        if (events is not null && eventId >= installed && !File.Exists(Path.Combine(events, $"{eventId:D4}.json")))
        {
            _allEventItems.RemoveAll(item => item.Id == eventId);   // deleted script
        }

        RefreshEventFilter();
        if (_selectedEventId is short selected && touched.Contains(selected))
        {
            try
            {
                var visualState = EnsureEventVisualState(selected);
                ApplyEventVisualState(visualState);
                ClearEventInstructionEditor();
                UpdateSelectedEventPatchStatus(selected);
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
            {
                ClearEventPresentation();
            }
        }

        UpdateMapEditorSidebar();
        UpdateStatus();
    }

    private string EventPatchStatusText(short eventId)
    {
        if (_eventProjectScriptPatches.ContainsKey(eventId))
        {
            return "Patch status: this event is overridden in the project and will be relocated on ROM export.";
        }

        if (DecompEventsFolder() is not null)
        {
            return _decompEventBytes.ContainsKey(eventId)
                ? $"Saved in data/events/{eventId:D4}.evt. Build ROM puts it in the game."
                : $"Same as data/events/{eventId:D4}.evt (already in the open ROM).";
        }

        return "Patch status: using the original ROM event script.";
    }

    /// <summary>
    /// "+ Add step": puts the command picked in the list into the script, before the selected row when it is an End
    /// (or nothing is selected: before the last End), after it otherwise, with the argument values typed below.
    /// The new row ends up selected.
    /// </summary>
    private async void OnAddEventStepClicked(object? sender, RoutedEventArgs e)
    {
        if (_session is null || _selectedEventId is not short eventId || _selectedEventVisualState is null)
        {
            await DisplayAlertAsync("No Event Selected", "Select an event first.", "OK");
            return;
        }

        if (_selectedEventOperationDefinition is not { } definition)
        {
            await DisplayAlertAsync("No Command Picked", "Pick the command to add in the list next to + Add step.", "OK");
            return;
        }

        var row = _selectedEventInstruction?.Instruction is not null
            ? _selectedEventInstruction
            : _selectedEventVisualState.Instructions.LastOrDefault(i => string.Equals(i.Instruction?.Name, "END", StringComparison.Ordinal));
        if (row?.Instruction is null)
        {
            await DisplayAlertAsync("No Place", "Select the row the new step goes after.", "OK");
            return;
        }

        var profile = RequireProfile();
        if (profile is null)
        {
            return;
        }

        var values = BuildEventArgumentUpdateMap();
        foreach (var jump in definition.Arguments.Where(IsJumpDefinition))
        {
            values.Remove(jump.Name);   // a jump typed for the selected row would point elsewhere from the new one
        }

        var before = row.Instruction.IsTerminal || string.Equals(row.Instruction.Name, "END", StringComparison.Ordinal);
        try
        {
            var script = _eventCache.TryGetValue(eventId, out var cached) ? cached : ReadEventScriptForEditor(eventId, profile);
            var labels = _selectedEventVisualState.LabelMap;
            var bytes = before
                ? _eventScriptRewriter.InsertInstructionBefore(EventRom(eventId), script, labels, row.Offset, definition)
                : _eventScriptRewriter.InsertInstructionAfter(EventRom(eventId), script, labels, row.Offset, definition);
            StoreEventScriptPatch(eventId, bytes);
            RefreshEditedEventView(eventId, profile);

            var newOffset = before ? row.Offset : NextInstructionOffset(row.Offset);
            var added = _visibleEventInstructions.FirstOrDefault(i => i.Offset == newOffset && i.Instruction?.Opcode == definition.Opcode);
            if (added?.Instruction is not null && values.Count > 0)
            {
                StoreEventScriptPatch(eventId, _eventInstructionPatcher.BuildPatchedScript(
                    EventRom(eventId), _eventCache[eventId], added.Instruction, definition, values));
                RefreshEditedEventView(eventId, profile);
            }

            ReselectEventInstruction(newOffset);
            UpdateEventBrowserPatchStatus(eventId);
            UpdateStatus();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Add Step Failed", ex.Message, "OK");
        }
    }

    /// <summary>Offset of the row that follows the one at <paramref name="offset"/> in the refreshed list.</summary>
    private int NextInstructionOffset(int offset) =>
        _visibleEventInstructions.Where(i => i.Instruction is not null && i.Offset > offset).Select(i => i.Offset).DefaultIfEmpty(offset).Min();
}
