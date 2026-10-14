# Handoff: function discovery (Medabots Metabee EU)

Updated 2026-10-13. **Current focus: discovery and documentation of ROM functions.** Porting to Lua
(`pc/game/src/script.lua`) is paused by the user's choice; don't start it without being asked.

## How to resume (read in this order)
1. This file.
2. `re/functions/README.md`: .md/.csv format, Ghidra tips, English naming `Subsystem_VerbObject`.
3. `re/functions/H.md`: the battle dispatchers (Fight_DispatchOnSubstateAA_*).
4. `re/functions/MERGE_RELATORIO.md`: what's already in `re/symbols/mtbe.csv` and the open conflicts.
5. `D:/claude-brain/Medabots/pc/research/README.md` (big; read it **with grep**, not whole).
6. `re/HANDOFF.md`: the older cloud <-> local protocol and diary (history; the end has the latest entries).

## Environment
- Ghidra open with GhidraMCP (tools `mcp__ghidra__*`). The project is saved by the **user**; agents never close or save it.
  The user ran `re/ghidra/CreateHandlerFunctions.java` (457 created): handlers from `H_lista.txt` are now functions.
- mGBA MCP (`mcp__mgba__*`): always muted (`-C mute=1`), window on display 2, never the Scripting window.
- Data ROM: `D:/claude-brain/Medabots/Medabots - Metabee (Europe).gba` (never modify; copy to scratch).
- Repos:
  - Toolkit `D:/claude-brain/Medabots/git/Medabots-Romhacking-Toolkit`, branch `claude/medabots-project-ajyaeb`
    (push authorized).
  - Main `D:/claude-brain/Medabots` (branch `dev`, no remote, commits stay local).
- Commit: `git -c user.name=jvsdm -c user.email=62727762+jvsdm@users.noreply.github.com commit`, message ending in
  `Co-Authored-By: Claude ...`.
- Never commit a ROM, save, game text, screenshots, `pc/cache/`, `*.cheats` or `*.wav`.
- Way of working: Opus plans and reviews; small "medabots" agents (subagent_type `medabots`) or a Workflow (max 15 in
  parallel worked fine) do the lists. Every agent writes only its own `XX.md` / `XX_simbolos.csv`, without commit or
  mtbe.csv. Opus merges.

## Where we are (numbers)
- `re/symbols/mtbe.csv`: 2,592 lines (+1,458 from the merge on 2026-10-13, commit `f73a901`).
- Documented in `re/functions/`:
  - R0..R4 (+a/b): the 1,507 functions reached by BL. **Done.**
  - G: gaps.
  - H: 827 handlers by pointer (list in `H_lista.txt`).
  - HN1..HN10: the 387 handlers that had no section. **Done**, decompiled and renamed in Ghidra.
  - HE1..HE4: event opcodes 0x00..0x88 (table 0x083D222C). **Done**, decompiled.
  - HO1/HO2: ObjectScript 0x01..0x3A (table 0x083AF8C0, argc at 0x083CE374). **Done**.
  - HA1..HA8: battle handlers. **Redo done on 2026-10-13** with decompile (about 190 sections rewritten and
    renamed). Remaining items are in each file's "Pendente redo" line: in HA2, 12 sections never reread (E02, E03,
    E08, G00, G01, G03, G06, H00, H0D, F02, H15, H18).
  - HA9..HA13: already covered by HN1..HN10 (checked).
  - HC.md: the event continuation table 0x083D2630 (137 ops, 85 unique targets, 30 are already functions).
- Confidence across the CSVs: about 723 entries "provável" and 178 "chute"; the rest "confirmado".

## Next discovery tasks (updated 2026-10-13, after the 3rd round: 15 agents)
Done in the 3rd round: MG (2nd merge into mtbe.csv, +498 / 81 swapped; MERGE_RELATORIO2.md), CF (conflicts decided
and applied), FIX (old notes fixed), FR1..FR3 (FL leftovers; nothing left), JT2/JT3 (dispatch tables), A6
(+0xA6 code = direct index into gTextGameMessages 0x083C6744), SD (substate tables A 9 / C 10), DT2/DT3 (tables),
CH1 (38 guesses), CH2 (39 guesses), CH3 (16 of 38 from R3).

0. **User step in Ghidra:** run CreateHandlerFunctions with `JT_criar.txt` (24 addresses), then Ctrl+S.
   Then document those 24 functions: the targets are in JT2.md / JT3.md / SD.md; the SD names go in
   SD_simbolos.csv.
1. **3rd merge into mtbe.csv** of the CSVs from this round that are not in it yet: FR1..FR3, JT2, JT3, A6, SD,
   DT2, DT3, CH1 (R0/R0a/R0b/R1/R1b/G), CH2 (R2), CH3 (R3).
   - Decide the 62 conflicts in MERGE_RELATORIO2.md: most are a generic "Substate" name -> new name. Suggestion:
     swap them all.
   - 0x08037830: FR3 = Fight_FindActiveBotInSideWithGroup, CH2 = Fight_FindLiveSlotMatchingField91 (the latter
     is the one in Ghidra). Pick one.
   - Remove/mark 0x08045A40 (data). Unify the soft-float duplicates (0x0807DA50/0x0807DAE8/0x0807DB34/0x0807DBCC;
     0x0807D48C vs 0x0807D4BC).
2. **CH3 leftovers:** 22 guesses in R3 (list in CH3.md) + ~30 in R4 (not started).
3. **Bad names to fix** (CH1): Fight_DrawBotPartEntrySprites/Icons/Tiles, Fight_FormatBotLabelText (suggestions
   in the CH1 report: Fight_LoadBotPartEntryTiles, Fight_CreateBotPartEntrySprites,
   Fight_UpdateBotPartEntryMotion, Fight_CopyBotLabelRecord).
   - FL10.md: 0x08066D8C is NOT the text typewriter (it's a ring of 72 points; FR3).
4. **Message script (A6):** the order of states 0..6 of gMessageStateHandlerTable 0x083B291C (only 0 and 5
   confirmed); the meaning of `FA 50` and of the END argument (0/2/3/4); codes 0x7A..0xAF are debug placeholders.
   - Pre/post-battle texts (DT2): the 4 loaders have no function in Ghidra.
5. **Remaining FUN_:** run list_functions again and count; ~30 stayed FUN_ on purpose (guesses) in FL*/FR*.
   0x08016370, 0x08035C3C, 0x08047850, 0x0807DD08, 0x08066460 (link?) are open.
6. **Validate in the emulator** (later): ROS/damage formulas (FH), message 0x02, 0x29/0x2A, 0x41, the medal fields
   (DT3: +3..+6 thresholds do not match R0a's 10/29/59).

## Key facts (don't rediscover)
- New on 2026-10-13: battle animation context 0x083B6100 (+0x8C list, +0xEC slot table). The T/H handlers' timer is
  0x03000440+0x86. Bot part flag at bot+0x59+0x18*i (i=0..3). gObjectSlots 0x03002C30. Effect mode fi+0x51 (0..3)
  comes from part byte +0x49 (10/11/12/other).
- **Merge pending:** the HA*_simbolos.csv files from this redo are NOT in mtbe.csv yet (merge script in
  MERGE_RELATORIO.md / redo it like the previous one).
- Battle bot block: `0x03000590 + bot*0x100` (research offsets = Ghidra offset − 0x40).
- fight_information `0x03001800`:
  - `+0xAA` = action step (substate)
  - `+0xB5` = handler done
  - `+0xA6` = step/message code
  - `+0x60` = side base (0/3)
  - `+0xD2..D4` = affected bots
  - `+0xD6..DA` = value per bot
- Dispatchers `Fight_DispatchOnSubstateAA_*` (~70): literal at stub+0x20 = block, stub+0x24 = table.
  - AR..AV = action-script opcodes 0x50..0x54 (`gActionScriptOpcodeHandlerTable` 0x083AF180, wrapper
    0x08034BA0+0x1C*k).
  - AW..BA = team support (protect from status, cure, repair armor, restore parts, gauge).
  - BA..BH = techniques that don't deal damage.
- Shared handlers:
  - 0x080305E0 runs `Fight_RunSubstateDispatch(+0xA6)` and advances.
  - 0x08030674 waits `+0x86` frames (A skips when +0x89==0).
  - 0x0803080C sets +0xB5=1 (ends without effect).
- Events: table 0x083D222C (literal 0x0804B734), opcodes 0x00..0x88; 0x06 = end, 0x19 = goto event.
  - The argument sizes from the ROM match the C# + `ROM_SIZE_FIX` in `pc/extract/extract_events.py`.
  - 0x4E (money) compares against `arg*10`.
  - Inventory 0x03002BA0 (64 pairs [0x80|id, qty]).
  - Medals at 0x03000BE0 (30 × 0x40, id at +1).
  - Money u32 at 0x03002B70.
- ObjectScript: the attack choreography. It runs opcodes until one writes 1 to `ctx+0x2A` ("done for this
  frame"); the loop is at 0x080393BC.
- Utilities:
  - 0x0807CD2C = signed division.
  - `FUN_08041D04` = memcpy.
  - randbyte 0x080424B8.
  - LZ77/`decompress_and_store` 0x080009BC.
- Random encounter: >16 steps and (>31 or rnd&31==31). Collision 3 = conditional passage (part set 0x6B).
- Sound: track interpreter 0x08072D68 (1 tick/frame); `Sound_RequestSong` = FUN_0804842C(1,id).

## Recommended agent prompt (template)
"Ghidra MCP open (don't close or save it; there are other agents in parallel). Folder
D:/claude-brain/Medabots/git/Medabots-Romhacking-Toolkit/re/functions/. Read README.md IN FULL and H.md. Use
mtbe.csv and pc/research/README.md only via grep. Names in English. Write only to XX.md / XX_simbolos.csv; no
commit, don't touch mtbe.csv, no emulator. Use get_function_by_address before decompile; rename in Ghidra when
confirmed/probable. Near ~150k of context, stop at a clean point. Short report: how many, patterns, divergences."
