/* Pilot: GetPartStats (0x08042B58) rewritten in C, with the same behaviour, plus a call counter in free EWRAM.
   Proves the pipeline (compile, place in free space, hook) without changing the game. */
#include "mtbe.h"

void Inject_GetPartStats(u32 bot, u32 kind, u8 *dest)
{
    const u8 *src = *PART_TABLE_LITERAL + bot * 0x40 + kind * 0x10;
    for (u32 i = 0; i < 0x10; i++)
        dest[i] = src[i];

    INJECT_DEBUG->magic = INJECT_MAGIC;
    INJECT_DEBUG->get_part_stats_calls++;
}
