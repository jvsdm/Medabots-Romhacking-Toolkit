using Medabots.Rom.Metadata;
using Medabots.Rom.Projects;

namespace Medabots.Rom.Events;

public sealed partial class EventInstructionPatcher
{
    private readonly EventOperationRegistry _registry;
    private readonly EventScriptSerializer _serializer;
    private readonly Dictionary<short, (int Offset, int Length)> _eventAllocations = [];

    private const string EventScriptsPatchOwner = "Event scripts";

    public EventInstructionPatcher(EventOperationRegistry? registry = null)
    {
        _registry = registry ?? EventOperationRegistry.LoadDefault();
        _serializer = new EventScriptSerializer(_registry);
    }

    public void Apply(RomHackSession session, MedabotsRomTextProfile profile, EventScript script, EventInstruction instruction, EventOperationDefinition targetDefinition, IReadOnlyDictionary<string, int> updatedArguments)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(script);
        ArgumentNullException.ThrowIfNull(instruction);
        ArgumentNullException.ThrowIfNull(targetDefinition);
        ArgumentNullException.ThrowIfNull(updatedArguments);

        var replacementInstruction = BuildReplacementInstruction(instruction, targetDefinition, updatedArguments);
        var serializedScript = _serializer.Serialize(session.RomFile, script, replacementInstruction);
        RewriteEvent(session, profile, script.EventId, serializedScript, targetDefinition.Name);
    }

    public void RewriteEvent(RomHackSession session, MedabotsRomTextProfile profile, short eventId, byte[] serializedScript, string description)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(serializedScript);

        // The installed table (it may have moved), moved again when the event needs a new slot or free
        // space is out of the banks' reach. One patch owner: later rewrites update the moved table.
        using var scope = session.BeginPatchScope(EventScriptsPatchOwner);
        var allocator = FreeSpaceAllocator.ForRom(session.RomFile.Data);
        var (tableOffset, eventCount) = EventScriptReader.ResolveInstalledEventTable(session.RomFile, profile);
        var current = profile with { Addresses = profile.Addresses with { EventTableOffset = tableOffset, EventCount = eventCount } };
        if (eventId >= eventCount || !EventDatabaseRelocator.Reaches(tableOffset, allocator.CurrentOffset + serializedScript.Length + 4))
        {
            var moves = new List<RomPatchAction>();
            current = EventDatabaseRelocator.Relocate(session.RomFile, current, Math.Max(eventCount, eventId + 1), allocator, moves, new EventScriptReader(_registry));
            session.ApplyPatches(moves);
        }

        session.ApplyPatches(BuildRewriteActions(session.RomFile, current, eventId, serializedScript, description, allocator));
    }

    public IReadOnlyList<RomPatchAction> BuildRewriteActions(RomFile romFile, MedabotsRomTextProfile profile, short eventId, byte[] serializedScript, string description, FreeSpaceAllocator allocator)
    {
        ArgumentNullException.ThrowIfNull(romFile);
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(serializedScript);
        ArgumentNullException.ThrowIfNull(allocator);

        return BuildRelocatedEventActions(romFile, profile, eventId, serializedScript, description, allocator);
    }
}
