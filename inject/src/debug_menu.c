/* Opens the game's built-in debug menu (menu_list scene 63: Process, Medal, Parts, Tinpet, Item, Face, Mdlpic,
   Rnd-tk, Battle, Sound, Motion) with L + R + SELECT on the field, and restarts the game when it is closed.

   Installed by replacing gMainStateTable[0] and [1] (MainState0 runs menu_list scenes, MainState1 is the field),
   so the game's own code is untouched: without the combo both just run the original states.

   The menu (debug step 1: LEFT/RIGHT choose, A enters, B leaves) leaves by setting the scene and step saved at
   0x03000470/0x03000471 by whoever opened it; the original opener is gone, so leaving goes to scene 0 with the
   field's video setup still in place. Instead, a reset brings the game back to its intro cleanly. */
#include "mtbe.h"

void MainState0(void);
void MainState1(void);

#define REG_DISPCNT (*(volatile u16 *)0x04000000)
#define REG_KEYINPUT (*(volatile u16 *)0x04000130)
#define REG_IME (*(volatile u16 *)0x04000208)
#define KEY_SELECT 0x0004
#define KEY_R 0x0100
#define KEY_L 0x0200
#define DEBUG_COMBO (KEY_L | KEY_R | KEY_SELECT)

#define MAIN_STATE (*(volatile u32 *)0x0300178C)   /* gMainState: 0 = MainState0 runs menu_list[scene] */
#define MENU_SCENE (*(volatile u32 *)0x03002B8C)   /* index into menu_list */
#define DEBUG_STEP (*(volatile u32 *)0x0300137C)   /* the debug menu's own step (0 = init) */
#define DEBUG_RETURN_SCENE (*(volatile u8 *)0x03000470)
#define DEBUG_RETURN_STEP (*(volatile u8 *)0x03000471)
#define BIOS_RESET_TARGET (*(volatile u8 *)0x03007FFA)   /* 0 = SoftReset restarts from the ROM */
#define MENU_SCENE_DEBUG 63

#define DEBUG_OPEN (*(volatile u8 *)(INJECT_RAM + 0x10))

void Inject_MainState1(void)
{
    if ((~REG_KEYINPUT & DEBUG_COMBO) == DEBUG_COMBO) {
        /* The debug menu draws with sprites and only turns sprites on: hide the field's backgrounds,
           keep BG0 (cleared text layer) and sprites. */
        REG_DISPCNT = (REG_DISPCNT & 0xF0FF) | 0x1100;
        DEBUG_RETURN_SCENE = 0;
        DEBUG_RETURN_STEP = 0;
        DEBUG_OPEN = 1;
        MENU_SCENE = MENU_SCENE_DEBUG;
        DEBUG_STEP = 0;
        MAIN_STATE = 0;
        return;
    }
    MainState1();
}

void Inject_MainState0(void)
{
    if (DEBUG_OPEN && MENU_SCENE == 0) {
        DEBUG_OPEN = 0;
        REG_IME = 0;
        BIOS_RESET_TARGET = 0;
        __asm__ volatile("swi 0x00");   /* SoftReset */
    }
    MainState0();
}
