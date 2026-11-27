/* Game functions and data used by injected code (Metabee E). Addresses come from re/symbols/mtbe.csv through the
   generated linker symbols; prototypes here are written by hand as each one is understood. */
#pragma once
#include "gba_types.h"
#include "mtbe_types.h"

/* The literal of GetPartStats that holds the part record table address (moved by the bot expansion), so
   injected code follows the table wherever the ROM has it. */
#define PART_TABLE_LITERAL ((const u8 *const *)0x08042B7Cu)

/* Counters the pilot keeps in free EWRAM, to see from the emulator that the injected code runs. */
typedef struct InjectDebug {
    u32 magic;              /* 'MBXI' once the injected code has run */
    u32 get_part_stats_calls;
} InjectDebug;

#define INJECT_DEBUG ((volatile InjectDebug *)INJECT_RAM)
#define INJECT_MAGIC 0x4958424Du
