namespace Medabots.Rom.Maps;

/// <summary>
/// A new map staged in the project; it takes the next free map ID when the project is applied (in list
/// order, before every other edit, which may then edit the new map).
/// <para>
/// Copy (<paramref name="Blank"/> false): a copy of <paramref name="SourceMapId"/>. Blank: an empty
/// <paramref name="Width"/> x <paramref name="Height"/> map (tiles) with the music, area and encounter
/// settings of <paramref name="SourceMapId"/> and either its tileset or, with
/// <paramref name="BlankTileset"/>, an empty black one; the collision is walkable inside a blocked border.
/// </para>
/// </summary>
public sealed record MapAddition(int SourceMapId, bool CopyWarpsAndPeople, bool Blank = false, int Width = 0, int Height = 0, bool BlankTileset = false);
