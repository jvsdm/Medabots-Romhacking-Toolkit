using Medabots.Rom.Metadata;
using Medabots.Rom.Projects;

namespace Medabots.Rom.Events;

internal sealed class EventScriptProjectEditSystem : IProjectEditSystem
{
    private readonly EventInstructionPatcher _eventInstructionPatcher;

    public EventScriptProjectEditSystem(EventInstructionPatcher eventInstructionPatcher)
    {
        _eventInstructionPatcher = eventInstructionPatcher;
    }

    public string DisplayName => "Event Script";

    public IEnumerable<string> DescribeChanges(RomHackProject project) =>
        project.EventScriptPatches.Select(patch => $"Event {patch.EventId:D4}")
            .Concat(project.DeletedEventScriptIds.Select(id => $"Deleted Event {id:D4}"))
            .Concat(project.EventLabels.Select(label => $"Label Event {label.EventId:D4} @ {label.Offset:X} -> {label.Label}"));

    public IEnumerable<ProjectChange> BuildChanges(RomHackProject project, ProjectBuildContext context)
    {
        if (project.EventScriptPatches.Count == 0 && project.DeletedEventScriptIds.Count == 0)
        {
            return [];
        }

        var resolvedProfile = context.Layout.RequireTextAndEventProfile();
        var actions = new List<RomPatchAction>();
        resolvedProfile = EnsureExpandedEventScriptDatabase(project, context.SourceRom, context.Allocator, resolvedProfile, actions);
        context.Layout.ReplaceTextAndEventProfile(resolvedProfile);
        foreach (var deletedEventId in project.DeletedEventScriptIds.Distinct().OrderBy(id => id))
        {
            actions.AddRange(_eventInstructionPatcher.BuildRewriteActions(context.SourceRom, resolvedProfile, deletedEventId, [MedabotsRomSchema.EventEndOpcode], $"Delete project event patch {deletedEventId}", context.Allocator));
        }

        foreach (var patch in project.EventScriptPatches)
        {
            actions.AddRange(_eventInstructionPatcher.BuildRewriteActions(context.SourceRom, resolvedProfile, patch.EventId, patch.ScriptBytes, $"Apply project event patch {patch.EventId}", context.Allocator));
        }

        return [new ProjectChange(DisplayName, "Event script patches", actions)];
    }

    // Moves the database (and its scripts) when it must grow or when free space is beyond the banks' reach.
    private static MedabotsRomTextProfile EnsureExpandedEventScriptDatabase(RomHackProject project, RomFile romFile, FreeSpaceAllocator allocator, MedabotsRomTextProfile profile, ICollection<RomPatchAction> actions)
    {
        var (tableOffset, installedCount) = EventScriptReader.ResolveInstalledEventTable(romFile, profile);
        var requiredEventCount = Math.Max(
            installedCount,
            Math.Max(
                project.EventScriptPatches.Count == 0 ? 0 : project.EventScriptPatches.Max(patch => patch.EventId + 1),
                project.DeletedEventScriptIds.Count == 0 ? 0 : project.DeletedEventScriptIds.Max(id => id + 1)));
        var scriptBytes = project.EventScriptPatches.Sum(patch => patch.ScriptBytes.Length + 4) + project.DeletedEventScriptIds.Count * 4;
        var reachable = EventDatabaseRelocator.Reaches(tableOffset, allocator.CurrentOffset + scriptBytes + 4);
        if (requiredEventCount <= installedCount && reachable)
        {
            return profile with
            {
                Addresses = profile.Addresses with { EventTableOffset = tableOffset, EventCount = installedCount },
            };
        }

        return EventDatabaseRelocator.Relocate(romFile, profile, requiredEventCount, allocator, actions, new EventScriptReader());
    }

}
