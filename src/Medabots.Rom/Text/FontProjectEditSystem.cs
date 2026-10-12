using Medabots.Rom.Projects;

namespace Medabots.Rom.Text;

internal sealed class FontProjectEditSystem : IProjectEditSystem
{
    public string DisplayName => "Font";

    public IEnumerable<string> DescribeChanges(RomHackProject project) =>
        project.UsePtBrFont ? new[] { "PT-BR font (100%: normal and italic glyphs, õ Ã ã Õ, widths)" }
        : project.UseExtendedFont ? new[] { "Extended font (õ Ã ã Õ, normal and italic; widths 0x4F-0x7F; italic font repair)" }
        : Array.Empty<string>();

    public IEnumerable<ProjectChange> BuildChanges(RomHackProject project, ProjectBuildContext context)
    {
        if (project.UsePtBrFont)
        {
            return PtBrFont.CanApply(context.SourceRom.Data, out var ptBrLayout)
                ? [new ProjectChange(DisplayName, "PT-BR font", PtBrFont.BuildPatches(context.SourceRom.Data, ptBrLayout))]
                : Array.Empty<ProjectChange>();
        }

        if (!project.UseExtendedFont || !ExtendedFont.CanApply(context.SourceRom.Data, out var layout))
        {
            return Array.Empty<ProjectChange>();
        }

        // The European italic font lost its dark shades: repaired first, so the italic text is readable and the
        // extension is measured on the repaired glyphs.
        var rom = context.SourceRom.Data.ToArray();
        var changes = new List<ProjectChange>();
        var repaired = ItalicFontRepair.Repair(rom, layout.ItalicFont, out var fixedPixels);
        if (repaired is not null)
        {
            repaired.CopyTo(rom, layout.ItalicFont);
            changes.Add(new ProjectChange(DisplayName, "Italic font repair",
                [new RomPatchAction(layout.ItalicFont, repaired, $"Italic font: {fixedPixels} lost dark pixels rebuilt")]));
        }

        changes.Add(new ProjectChange(DisplayName, "Extended font", ExtendedFont.BuildPatches(rom, layout)));
        return changes;
    }
}
