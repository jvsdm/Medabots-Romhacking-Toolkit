using Medabots.Rom.Metadata;

namespace Medabots.Rom.Projects;

public sealed partial class RomHackProjectApplicator
{
    private readonly Text.MedabotsTextPatcher _textPatcher;
    private readonly Events.EventInstructionPatcher _eventInstructionPatcher;
    private readonly Encounters.EncounterTableReader _encounterTableReader;
    private readonly Maps.MapOverlayPatcher _mapOverlayPatcher;
    private readonly Maps.MapLayerPatcher _mapLayerPatcher;
    private readonly Images.ImageAssetPatcher _imageAssetPatcher;
    private readonly Battles.BattlePatcher _battlePatcher;
    private readonly Parts.PartPatcher _partPatcher;
    private readonly Shops.ShopPatcher _shopPatcher;
    private readonly Starter.StarterPatcher _starterPatcher;
    private readonly IReadOnlyList<IProjectEditSystem> _systems;

    public RomHackProjectApplicator(Text.MedabotsTextPatcher? textPatcher = null, Events.EventInstructionPatcher? eventInstructionPatcher = null, Maps.MapOverlayPatcher? mapOverlayPatcher = null, Maps.MapLayerPatcher? mapLayerPatcher = null, Images.ImageAssetPatcher? imageAssetPatcher = null, Battles.BattlePatcher? battlePatcher = null, Parts.PartPatcher? partPatcher = null, Shops.ShopPatcher? shopPatcher = null, Starter.StarterPatcher? starterPatcher = null)
    {
        _textPatcher = textPatcher ?? new Text.MedabotsTextPatcher();
        _eventInstructionPatcher = eventInstructionPatcher ?? new Events.EventInstructionPatcher();
        _encounterTableReader = new Encounters.EncounterTableReader();
        _mapOverlayPatcher = mapOverlayPatcher ?? new Maps.MapOverlayPatcher();
        _mapLayerPatcher = mapLayerPatcher ?? new Maps.MapLayerPatcher();
        _imageAssetPatcher = imageAssetPatcher ?? new Images.ImageAssetPatcher();
        _battlePatcher = battlePatcher ?? new Battles.BattlePatcher();
        _partPatcher = partPatcher ?? new Parts.PartPatcher();
        _shopPatcher = shopPatcher ?? new Shops.ShopPatcher();
        _starterPatcher = starterPatcher ?? new Starter.StarterPatcher();
        _systems = BuildSystems();
    }

    public IReadOnlyList<IProjectEditSystem> Systems => _systems;

    public IReadOnlyList<ProjectChange> BuildChanges(RomHackProject project, RomFile sourceRom)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(sourceRom);

        var context = new ProjectBuildContext(sourceRom, ResolveLayout(project));
        var changes = new List<ProjectChange>();
        foreach (var system in _systems)
        {
            changes.AddRange(system.BuildChanges(project, context));
        }

        return changes;
    }

    public void Apply(RomHackProject project, RomHackSession session)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(session);

        // New maps and portrait characters first: they move tables, and the other edits are built against the
        // result. Written as the new base (not as patch records): those edits then update the moved tables.
        if (project.MapAdditions.Count > 0)
        {
            WriteBase(session, Maps.MapExpander.ApplyAdditions(session.RomFile.Data.ToArray(), project.MapAdditions), $"Add {project.MapAdditions.Count} map(s)");
        }

        if (project.PortraitAdditions.Count > 0)
        {
            WriteBase(session, Images.PortraitExpander.ApplyAdditions(session.RomFile.Data.ToArray(), project.PortraitAdditions), $"Add {project.PortraitAdditions.Count} portrait character(s)");
        }

        var changes = BuildChanges(project, session.RomFile);
        foreach (var change in changes)
        {
            using var scope = session.BeginPatchScope(change.Owner);
            change.Apply(session);
        }
    }

    private static void WriteBase(RomHackSession session, byte[] after, string description)
    {
        foreach (var action in DifferenceActions(session.RomFile.Data.ToArray(), after, description))
        {
            session.RomFile.WriteBytes(action.Offset, action.Data);
        }

        session.RomFile.RefreshLayout();
    }

    // The changed byte ranges between two ROM images as patch actions (ranges closer than 16 bytes merged).
    internal static IReadOnlyList<RomPatchAction> DifferenceActions(byte[] before, byte[] after, string description)
    {
        var actions = new List<RomPatchAction>();
        var at = 0;
        while (at < after.Length)
        {
            if (at < before.Length && before[at] == after[at])
            {
                at++;
                continue;
            }

            var start = at;
            var end = at + 1;
            for (var gap = 0; at < after.Length && gap < 16; at++)
            {
                if (at >= before.Length || before[at] != after[at])
                {
                    end = at + 1;
                    gap = 0;
                }
                else
                {
                    gap++;
                }
            }

            actions.Add(RomPatchAction.Create(start, after.AsSpan(start, end - start).ToArray(), description));
            at = end;
        }

        return actions;
    }

    private ResolvedRomLayout ResolveLayout(RomHackProject project)
    {
        if (string.IsNullOrWhiteSpace(project.TextProfileId))
        {
            return new ResolvedRomLayout(null);
        }

        var profile = MedabotsRomTextProfiles.FindById(project.TextProfileId)
            ?? throw new InvalidOperationException("The project does not define a known text profile.");
        return new ResolvedRomLayout(profile);
    }

    private IReadOnlyList<IProjectEditSystem> BuildSystems()
    {
        return
        [
            new PendingActionsProjectEditSystem(),
            new Text.MessageProjectEditSystem(_textPatcher),
            new Text.FontProjectEditSystem(),
            new Events.EventScriptProjectEditSystem(_eventInstructionPatcher),
            new Maps.MapSpawnProjectEditSystem(_mapOverlayPatcher),
            new Maps.MapWarpProjectEditSystem(_mapOverlayPatcher),
            new Maps.MapCollisionProjectEditSystem(_mapOverlayPatcher),
            new Maps.MapDimensionProjectEditSystem(),
            new Maps.MapLayerProjectEditSystem(_mapLayerPatcher),
            new Maps.MapTilesetProjectEditSystem(),
            new Maps.MapEncounterStateProjectEditSystem(),
            new Maps.MapEncounterProjectEditSystem(_encounterTableReader),
            new Maps.MapMusicProjectEditSystem(),
            new Maps.MapSpriteSlotProjectEditSystem(_mapOverlayPatcher),
            new Battles.BattleProjectEditSystem(_battlePatcher),
            new Parts.PartProjectEditSystem(_partPatcher),
            new Shops.ShopProjectEditSystem(_shopPatcher),
            new Starter.StarterProjectEditSystem(_starterPatcher),
            new Images.SpriteProjectEditSystem(_imageAssetPatcher)
        ];
    }
}
