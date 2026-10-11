using Medabots.Rom.Projects;

namespace Medabots.Rom.Text;

internal sealed class FontProjectEditSystem : IProjectEditSystem
{
    public string DisplayName => "Font";

    public IEnumerable<string> DescribeChanges(RomHackProject project) =>
        project.UseExtendedFont ? new[] { "Extended font (õ Ã ã Õ, widths 0x4F-0x7F)" } : Array.Empty<string>();

    public IEnumerable<ProjectChange> BuildChanges(RomHackProject project, ProjectBuildContext context)
    {
        if (!project.UseExtendedFont || !ExtendedFont.CanApply(context.SourceRom.Data, out var layout))
        {
            return Array.Empty<ProjectChange>();
        }

        return new[] { new ProjectChange(DisplayName, "Extended font", ExtendedFont.BuildPatches(layout)) };
    }
}
