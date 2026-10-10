using Medabots.Rom.Projects;

namespace Medabots.Rom.Maps;

internal sealed class MapTilesetProjectEditSystem : IProjectEditSystem
{
    public string DisplayName => "Map Tileset";

    public IEnumerable<string> DescribeChanges(RomHackProject project) =>
        project.MapTilesetPatches.Select(patch =>
            $"Tileset {(patch.TilePixels is null ? "" : "graphics ")}{(patch.PaletteBytes is null ? "" : "palette ")}for map(s) {string.Join(", ", patch.MapIds)}");

    public IEnumerable<ProjectChange> BuildChanges(RomHackProject project, ProjectBuildContext context) =>
        project.MapTilesetPatches
            .Select(patch => new ProjectChange(DisplayName, $"Tileset for map(s) {string.Join(", ", patch.MapIds)}", MapTilesetCodec.BuildActions(context.SourceRom, patch, context.Allocator)))
            .ToArray();
}
