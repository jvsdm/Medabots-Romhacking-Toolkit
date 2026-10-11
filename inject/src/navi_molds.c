/* Navi bots: pieces imported from Medarot Navi keep Navi's geometry (pivots, offsets, sizes) in a second mold
   table (written by the Toolkit's NaviImporter after the marker "MBXNAVIT"; build.py defines gNaviMoldTable).
   Their part entries carry bit 16 (unused by the game). The three functions that read the mold table for one
   piece are hooked: each wrapper picks the Navi table for a Navi piece and the Core table otherwise, then runs
   the original function (its 16-byte prologue copied into a trampoline that jumps back after the hook). */
#include "mtbe.h"

#define NAVI_ENTRY_BIT 0x10000u
#define CORE_MOLD_TABLE_LITERAL ((const u32 *const *)0x08005364)

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

static int IsNaviPiece(const u32 *entries, u32 mold)
{
    if (entries == 0)
        return 0;
    for (; (*entries & 0x3F) != 0; entries++)
        if ((*entries & 0x3F) == (mold & 0xFF))
            return (*entries & NAVI_ENTRY_BIT) != 0;
    return 0;
}

static const u32 *PickTable(const u32 *current, const u32 *entries, u32 mold)
{
    if (IsNaviPiece(entries, mold))
        return gNaviMoldTable;
    if (current == gNaviMoldTable)
        return *CORE_MOLD_TABLE_LITERAL;
    return current;
}

u32 Inject_PartPreviewPiece(const u32 *table, const u32 *entries, u32 mold, u32 palette, void *state, u16 *tile, u32 variant)
{
    return Orig_PartPreviewPiece(PickTable(table, entries, mold), entries, mold, palette, state, tile, variant);
}

u32 Inject_PlacePiece(u32 *ctx, const u32 *entries, u32 mold, s32 x, s32 y, s32 z, s32 angle, u32 mirrored)
{
    u32 saved = ctx[1];
    ctx[1] = (u32)PickTable((const u32 *)saved, entries, mold);
    u32 result = Orig_PlacePiece(ctx, entries, mold, x, y, z, angle, mirrored);
    ctx[1] = saved;
    return result;
}

u32 Inject_CopyArmGraphics(const u32 *table, void *state, const u32 *entries, u32 src, u32 dst)
{
    return Orig_CopyArmGraphics(PickTable(table, entries, src), state, entries, src, dst);
}
