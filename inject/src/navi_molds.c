/* Navi bots: pieces imported from Medarot Navi keep Navi's geometry (pivots, offsets, sizes) in a second mold
   table (written by the Toolkit's NaviImporter after the marker "MBXNAVIT"; build.py defines gNaviMoldTable).
   Their part entries carry bit 16 (unused by the game). The three functions that read the mold table for one
   piece are hooked: each wrapper picks the Navi table for a Navi piece and the Core table otherwise, then runs
   the original function (its 16-byte prologue copied into a trampoline that jumps back after the hook). */
#include "mtbe.h"

#define NAVI_ENTRY_BIT 0x10000u
#define CORE_MOLD_TABLE_LITERAL ((const u32 *const *)0x08005364)
#define MIRROR_MAP_LITERAL ((const u8 *const *)0x08046654)

extern const u32 gNaviMoldTable[];

u32 Orig_PartPreviewPiece(const u32 *table, const u32 *entries, u32 mold, u32 palette, void *state, u16 *tile, u32 variant);
u32 Orig_PlacePiece(u32 *ctx, const u32 *entries, u32 mold, s32 x, s32 y, s32 z, s32 angle, u32 mirrored);
u32 Orig_CopyArmGraphics(const u32 *table, void *state, const u32 *entries, u32 src, u32 dst);

/* Prologues as in the ROM; the scratch register of the jump is dead at the continuation point. */
asm(".text\n"
    ".thumb\n"
    ".align 2\n"
    ".global Orig_PartPreviewPiece\n"
    ".thumb_func\n"
    "Orig_PartPreviewPiece:\n"
    "  .hword 0xB5F0, 0x4657, 0x464E, 0x4645, 0xB4E0, 0xB088, 0x9003, 0x9104\n"
    "  ldr r0, =0x08045E7D\n"
    "  bx r0\n"
    "  .ltorg\n"
    ".align 2\n"
    ".global Orig_PlacePiece\n"
    ".thumb_func\n"
    "Orig_PlacePiece:\n"
    "  .hword 0xB5F0, 0x4657, 0x464E, 0x4645, 0xB4E0, 0xB093, 0x4680, 0x9108\n"
    "  ldr r0, =0x080465C9\n"
    "  bx r0\n"
    "  .ltorg\n"
    ".align 2\n"
    ".global Orig_CopyArmGraphics\n"
    ".thumb_func\n"
    "Orig_CopyArmGraphics:\n"
    "  .hword 0xB570, 0x1C0E, 0x9904, 0x061B, 0x0E1B, 0x0609, 0x0E0C, 0x0099\n"
    "  ldr r5, =0x08046045\n"
    "  bx r5\n"
    "  .ltorg\n");

/* 1 = Navi piece, 0 = Core piece, -1 = the mold has no entry. */
static int NaviPiece(const u32 *entries, u32 mold)
{
    if (entries == 0)
        return -1;
    for (; (*entries & 0x3F) != 0; entries++)
        if ((*entries & 0x3F) == (mold & 0xFF))
            return (*entries & NAVI_ENTRY_BIT) != 0;
    return -1;
}

static const u32 *PickTable(const u32 *current, const u32 *entries, u32 mold)
{
    if (NaviPiece(entries, mold) == 1)
        return gNaviMoldTable;
    if (current == gNaviMoldTable)
        return *CORE_MOLD_TABLE_LITERAL;
    return current;
}

u32 Inject_PartPreviewPiece(const u32 *table, const u32 *entries, u32 mold, u32 palette, void *state, u16 *tile, u32 variant)
{
    return Orig_PartPreviewPiece(PickTable(table, entries, mold), entries, mold, palette, state, tile, variant);
}

/* On side 1 a piece takes the geometry of its mirror mold (the arms swap pictures, CopyArmGraphics), so the table
   follows the piece whose picture and record are drawn there. */
u32 Inject_PlacePiece(u32 *ctx, const u32 *entries, u32 mold, s32 x, s32 y, s32 z, s32 angle, u32 mirrored)
{
    u32 saved = ctx[1];
    u32 drawn = mold;
    if ((mirrored & 0xFF) && NaviPiece(entries, (*MIRROR_MAP_LITERAL)[mold & 0xFF]) >= 0)
        drawn = (*MIRROR_MAP_LITERAL)[mold & 0xFF];
    ctx[1] = (u32)PickTable((const u32 *)saved, entries, drawn);
    u32 result = Orig_PlacePiece(ctx, entries, mold, x, y, z, angle, mirrored);
    ctx[1] = saved;
    return result;
}

static u32 EntryOf(const u32 *entries, u32 mold)
{
    if (entries != 0)
        for (; (*entries & 0x3F) != 0; entries++)
            if ((*entries & 0x3F) == (mold & 0xFF))
                return *entries;
    return 0;
}

/* Side 1 swaps the arms' pictures piece by piece. With a Core arm against a Navi one the pieces may differ in size
   (a 64x64 one-piece Navi arm), and a Navi piece with its own side-1 picture (bit 15) is not swapped: then each
   arm keeps its own picture, flipped at the mirror geometry. */
u32 Inject_CopyArmGraphics(const u32 *table, void *state, const u32 *entries, u32 src, u32 dst)
{
    const u32 *core = *CORE_MOLD_TABLE_LITERAL;
    const u8 *from = (const u8 *)core[src & 0xFF], *to = (const u8 *)core[dst & 0xFF];
    u32 entry = EntryOf(entries, src);
    if ((entry & NAVI_ENTRY_BIT && entry & 0x8000) || from[0x12] != to[0x12] || from[0x13] != to[0x13])
        return 0;
    return Orig_CopyArmGraphics(PickTable(table, entries, src), state, entries, src, dst);
}
