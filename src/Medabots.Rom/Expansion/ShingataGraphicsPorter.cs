using System.Buffers.Binary;

namespace Medabots.Rom.Expansion;

/// <summary>
/// Copies a Shingata Medarot bot's graphics into the expanded ROM (mirrors tools/gfx_port.py):
/// battle mini sprite blocks, palette family, movement style and the large part images, whose art is
/// appended to the shared mold blob lists in free selector slots (8-bit selector, 256 per mold).
/// </summary>
internal sealed class ShingataGraphicsPorter
{
    private const int GbaBase = 0x08000000;
    private const int DescriptorTable = 0x3D1D78;
    private const int SelectorMask = 0xFF;
    private const int SelectorSlots = 256;
    private const int OriginalSlots = 128;
    private const int OriginalGraphicsRows = 122;
    private const int PaletteSize = 0x20;

    internal static readonly (string Kind, int RowSize, int EuTable, int ShingataTable, string GraphicsTable)[] PreviewRows =
    [
        ("head", 16, 0x07EE70, 0x091A98, "PREVIEW_HEAD"),
        ("right_arm", 16, 0x07F610, 0x091E78, "PREVIEW_RIGHT_ARM"),
        ("left_arm", 16, 0x07FDB0, 0x092258, "PREVIEW_LEFT_ARM"),
        ("legs", 32, 0x080550, 0x092638, "PREVIEW_LEGS"),
    ];

    private const int ShingataBattleSprites = 0x461168;
    private const int ShingataPaletteFamily = 0x46A72C;
    private const int ShingataMoveStyle = 0x4748D4;
    private const int ShingataMiniPalettes = 0x58E0A0;
    private const int ShingataDescriptors = 0x47A93C;

    private readonly byte[] _rom;
    private readonly byte[] _original;
    private readonly byte[] _shingata;
    private readonly RomAllocator _allocator;
    private readonly IReadOnlyDictionary<string, int> _graphics;
    private readonly ICollection<string> _log;
    private readonly Dictionary<int, HashSet<int>> _used;
    private readonly Dictionary<int, uint[]> _tables = new(); // insertion order matters (allocation order)
    private readonly List<int> _tableOrder = [];
    private readonly Dictionary<int, int> _copied = new();

    public ShingataGraphicsPorter(byte[] rom, byte[] original, byte[] shingata, RomAllocator allocator,
        IReadOnlyDictionary<string, int> graphicsTables, ICollection<string> log)
    {
        _rom = rom;
        _original = original;
        _shingata = shingata;
        _allocator = allocator;
        _graphics = graphicsTables;
        _log = log;
        _used = EuDescriptorUsage(original);
    }

    /// <summary>
    /// Ports in bot order: Shingata bots (ShingataBot set) and EU-graphics bots with their own colours
    /// (Recolor set). Mirrors tools/gfx_port.py port_graphics.
    /// </summary>
    public void Port(IReadOnlyList<ExpansionBot> bots)
    {
        var recolorJobs = new List<(int BotId, MiniSpritePalette.Job Job)>();
        var spritesAt = _graphics["BATTLE_SPRITES"];
        foreach (var bot in bots)
        {
            if (bot.Recolor is not null)
            {
                recolorJobs.Add((bot.Id, RecolorNative(bot)));
                continue;
            }

            if (bot.ShingataBot is not { } source)
            {
                continue;
            }

            var (botId, name) = (bot.Id, bot.Name);
            for (var k = 0; k < 6; k++)
            {
                var pointer = U32(_shingata, ShingataBattleSprites + (source * 6 + k) * 4);
                WriteU32(spritesAt + (botId * 6 + k) * 4, (uint)CopyBlob(pointer, palette: false));
            }

            var family = _shingata[ShingataPaletteFamily + source];
            _rom[_graphics["PALETTE_FAMILY"] + botId] = family;
            _rom[_graphics["MOVE_STYLE"] + botId] = _shingata[ShingataMoveStyle + source];
            var components = Enumerable.Range(0, 6)
                .Select(k => ExpansionCodecs.MaliasDecompress(_shingata, (int)U32(_shingata, ShingataBattleSprites + (source * 6 + k) * 4) - GbaBase).Payload)
                .ToArray();
            recolorJobs.Add((botId, new MiniSpritePalette.Job(name, MiniSpritePalette.Palette(_shingata, ShingataMiniPalettes + 32 * family), components)));

            var molds = new SortedSet<int>();
            foreach (var (_, rowSize, _, shingataTable, graphicsTable) in PreviewRows)
            {
                var row = Enumerable.Range(0, rowSize / 4).Select(i => U32(_shingata, shingataTable + source * rowSize + 4 * i)).ToArray();
                var rowAt = _graphics[graphicsTable] + botId * rowSize;
                for (var i = 0; i < row.Length; i++)
                {
                    var entry = row[i];
                    var mold = (int)(entry & 0x3F);
                    if (mold != 0)
                    {
                        var sourceBlobs = DescriptorBlobTable(_shingata, ShingataDescriptors, mold);
                        var table = Table(mold);
                        var taken = Taken(mold);
                        var selectors = EntrySelectors(entry);
                        var selector = FreeSelector(taken, selectors.Count)
                            ?? throw new InvalidDataException($"Mold {mold} has no free selector left (256 slots).");
                        for (var k = 0; k < selectors.Count; k++)
                        {
                            taken.Add(selector + k);
                            var image = U32(_shingata, sourceBlobs + 8 * selectors[k]);
                            var palette = U32(_shingata, sourceBlobs + 8 * selectors[k] + 4);
                            var newImage = CopyBlob(image, palette: false);
                            var newPalette = palette == image ? newImage : CopyBlob(palette, palette: true);
                            table[(selector + k) * 2] = (uint)newImage;
                            table[(selector + k) * 2 + 1] = (uint)newPalette;
                        }

                        entry = (entry & ~0x3Fu & ~((uint)SelectorMask << 6)) | (uint)mold | (uint)(selector << 6);
                        molds.Add(mold);
                    }

                    WriteU32(rowAt + 4 * i, entry);
                }
            }

            _log.Add($"graphics of {name} (bot {botId}) <- Shingata bot {source}; molds [{string.Join(", ", molds)}]");
        }

        RecolorMiniSprites(recolorJobs);
        WriteTables();
    }

    // A bot reusing an EU bot's drawings with its own colours: each "#rrggbb" -> "#rrggbb" swap is
    // applied to the large-image palettes (new selector slots pointing at the same images) and to
    // the mini sprite's source palette.
    private MiniSpritePalette.Job RecolorNative(ExpansionBot bot)
    {
        var map = bot.Recolor!.ToDictionary(pair => HexToBgr555(pair.Key), pair => HexToBgr555(pair.Value));
        int Mapped(int color) => (map.TryGetValue(color & 0x7FFF, out var value) ? value : color & 0x7FFF) | (color & 0x8000);

        var newPalettes = new Dictionary<uint, uint>();
        var molds = new SortedSet<int>();
        foreach (var (_, rowSize, _, _, graphicsTable) in PreviewRows)
        {
            var rowAt = _graphics[graphicsTable] + bot.Id * rowSize;
            for (var i = 0; i < rowSize / 4; i++)
            {
                var entry = U32(_rom, rowAt + 4 * i);
                var mold = (int)(entry & 0x3F);
                if (mold == 0)
                {
                    continue;
                }

                var table = Table(mold);
                var taken = Taken(mold);
                var selectors = EntrySelectors(entry);
                var pairs = selectors.Select(s => (Image: table[s * 2], Palette: table[s * 2 + 1])).ToList();
                if (pairs.All(pair => pair.Palette == pair.Image))
                {
                    continue; // piece without a palette of its own: uses the main piece's
                }

                var selector = FreeSelector(taken, selectors.Count)
                    ?? throw new InvalidDataException($"Mold {mold} has no free selector left (256 slots).");
                for (var k = 0; k < pairs.Count; k++)
                {
                    taken.Add(selector + k);
                    var (image, palette) = pairs[k];
                    if (palette != image && !newPalettes.ContainsKey(palette))
                    {
                        var at = _allocator.Take(PaletteSize);
                        for (var c = 0; c < 16; c++)
                        {
                            var color = _original[(int)palette - GbaBase + 2 * c] | _original[(int)palette - GbaBase + 2 * c + 1] << 8;
                            BinaryPrimitives.WriteUInt16LittleEndian(_rom.AsSpan(at + 2 * c), (ushort)Mapped(color));
                        }

                        newPalettes[palette] = (uint)(GbaBase + at);
                    }

                    table[(selector + k) * 2] = image;
                    table[(selector + k) * 2 + 1] = newPalettes.TryGetValue(palette, out var newPalette) ? newPalette : image;
                }

                WriteU32(rowAt + 4 * i, (entry & ~0x3Fu & ~((uint)SelectorMask << 6)) | (uint)mold | (uint)(selector << 6));
                molds.Add(mold);
            }
        }

        var spritesAt = _graphics["BATTLE_SPRITES"];
        var family = _rom[_graphics["PALETTE_FAMILY"] + bot.Id];
        var components = Enumerable.Range(0, 6)
            .Select(k => ExpansionCodecs.MaliasDecompress(_original, (int)U32(_rom, spritesAt + (bot.Id * 6 + k) * 4) - GbaBase).Payload)
            .ToArray();
        var paletteAt = MiniSpritePalette.EuPalettes + 32 * family;
        var sourcePalette = Enumerable.Range(0, 16)
            .Select(c => MiniSpritePalette.Rgb(Mapped(_original[paletteAt + 2 * c] | _original[paletteAt + 2 * c + 1] << 8) & 0x7FFF))
            .ToArray();
        _log.Add($"colours of {bot.Name} (bot {bot.Id}): {newPalettes.Count} new palettes; molds [{string.Join(", ", molds)}]");
        return new MiniSpritePalette.Job(bot.Name, sourcePalette, components, Restyle: false);
    }

    private static int HexToBgr555(string hex)
    {
        int r = Convert.ToInt32(hex.Substring(1, 2), 16), g = Convert.ToInt32(hex.Substring(3, 2), 16), b = Convert.ToInt32(hex.Substring(5, 2), 16);
        return r / 8 | (g / 8) << 5 | (b / 8) << 10;
    }

    private void RecolorMiniSprites(List<(int BotId, MiniSpritePalette.Job Job)> jobs)
    {
        if (jobs.Count == 0)
        {
            return;
        }

        var basePalettes = Enumerable.Range(0, 3).Select(k => MiniSpritePalette.Palette(_original, MiniSpritePalette.EuPalettes + 32 * k)).ToList();
        var result = MiniSpritePalette.Recolor(jobs.Select(job => job.Job).ToList(), basePalettes, _log);
        var at = MiniSpritePalette.EuPalettes + 32 * MiniSpritePalette.ExtraBank;
        if (_original.AsSpan(at + 2, 30).ContainsAnyExcept((byte)0))
        {
            throw new InvalidDataException("OBJ bank 3 of the mini sprite palettes is not free in this ROM.");
        }

        result.ExtraPalette.CopyTo(_rom, at);
        var spritesAt = _graphics["BATTLE_SPRITES"];
        for (var j = 0; j < jobs.Count; j++)
        {
            var (bank, components) = result.Bots[j];
            for (var k = 0; k < components.Length; k++)
            {
                var blob = ExpansionCodecs.MaliasStore(components[k]);
                var p = _allocator.Take(blob.Length);
                blob.CopyTo(_rom, p);
                WriteU32(spritesAt + (jobs[j].BotId * 6 + k) * 4, (uint)(GbaBase + p));
            }

            _rom[_graphics["PALETTE_FAMILY"] + jobs[j].BotId] = (byte)bank;
        }

        _log.Add($"extra mini sprite palette in bank {MiniSpritePalette.ExtraBank} (0x{at:X6})");
    }

    private void WriteTables()
    {
        foreach (var mold in _tableOrder)
        {
            var words = _tables[mold];
            var at = _allocator.Take(words.Length * 4);
            for (var i = 0; i < words.Length; i++)
            {
                WriteU32(at + 4 * i, words[i]);
            }

            var record = (int)U32(_original, DescriptorTable + 4 * mold) - GbaBase;
            WriteU32(record, (uint)(GbaBase + at));
        }

        _log.Add($"previews: {_tables.Count} mold blob lists rebuilt, {_copied.Count} blocks copied");
    }

    private int CopyBlob(uint pointer, bool palette)
    {
        if (pointer == 0)
        {
            return 0;
        }

        var key = (int)pointer;
        if (!_copied.TryGetValue(key, out var copy))
        {
            var offset = (int)pointer - GbaBase;
            var size = palette ? PaletteSize : ExpansionCodecs.CompressedSize(_shingata, offset);
            var at = _allocator.Take(size);
            _shingata.AsSpan(offset, size).CopyTo(_rom.AsSpan(at));
            copy = GbaBase + at;
            _copied[key] = copy;
        }

        return copy;
    }

    private uint[] Table(int mold)
    {
        if (!_tables.TryGetValue(mold, out var words))
        {
            var blobs = DescriptorBlobTable(_original, DescriptorTable, mold);
            words = new uint[SelectorSlots * 2];
            for (var i = 0; i < OriginalSlots * 2; i++)
            {
                words[i] = U32(_original, blobs + 4 * i);
            }

            for (var i = OriginalSlots; i < SelectorSlots; i++)
            {
                words[i * 2] = words[0];
                words[i * 2 + 1] = words[1];
            }

            _tables[mold] = words;
            _tableOrder.Add(mold);
        }

        return words;
    }

    private HashSet<int> Taken(int mold)
    {
        if (!_used.TryGetValue(mold, out var taken))
        {
            taken = [];
            _used[mold] = taken;
        }

        return taken;
    }

    private static List<int> EntrySelectors(uint entry)
    {
        var selector = (int)((entry >> 6) & SelectorMask);
        var variant = (int)((entry >> 15) & 1);
        return Enumerable.Range(selector, variant + 1).ToList();
    }

    private static int? FreeSelector(HashSet<int> taken, int width)
    {
        for (var selector = 1; selector <= SelectorSlots - width; selector++)
        {
            var free = true;
            for (var k = 0; k < width; k++)
            {
                if (taken.Contains(selector + k))
                {
                    free = false;
                    break;
                }
            }

            if (free)
            {
                return selector;
            }
        }

        return null;
    }

    private static int DescriptorBlobTable(byte[] rom, int descriptorTable, int mold)
    {
        var record = (int)U32(rom, descriptorTable + 4 * mold) - GbaBase;
        return (int)U32(rom, record) - GbaBase;
    }

    // mold -> selectors already used by the original ROM (preview rows and synthetic templates).
    private static Dictionary<int, HashSet<int>> EuDescriptorUsage(byte[] original)
    {
        var used = new Dictionary<int, HashSet<int>>();

        void Scan(int baseOffset, int count)
        {
            for (var i = 0; i < count; i++)
            {
                var entry = U32(original, baseOffset + 4 * i);
                var mold = (int)(entry & 0x3F);
                if (mold == 0)
                {
                    continue;
                }

                if (!used.TryGetValue(mold, out var set))
                {
                    set = [];
                    used[mold] = set;
                }

                set.UnionWith(EntrySelectors(entry));
            }
        }

        foreach (var (_, rowSize, euTable, _, _) in PreviewRows)
        {
            Scan(euTable, OriginalGraphicsRows * rowSize / 4);
        }

        Scan(PreviewRows[0].EuTable - 0x60, 4 * 0x18 / 4);
        return used;
    }

    private static uint U32(byte[] data, int offset) => BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(offset));

    private void WriteU32(int offset, uint value) => BinaryPrimitives.WriteUInt32LittleEndian(_rom.AsSpan(offset), value);
}

/// <summary>Bump allocator for the free area of the expanded ROM.</summary>
internal sealed class RomAllocator(int start)
{
    public int Start { get; } = start;

    public int Next { get; private set; } = start;

    public int Take(int size, int align = 4)
    {
        Next = (Next + align - 1) & ~(align - 1);
        var offset = Next;
        Next += size;
        return offset;
    }
}
