#pragma once

typedef unsigned char u8;
typedef unsigned short u16;
typedef unsigned int u32;
typedef signed char s8;
typedef signed short s16;
typedef signed int s32;

/* Free EWRAM for injected code: 0x0202E780-0x0202F7FF stayed zero in every capture (overworld, battle);
   0x0202E000-0x0202E77F is the expanded inventory. Keep injected state small and documented here. */
#define INJECT_RAM 0x0202F700u
