# Handoff: function discovery (Medabots Metabee EU)

Updated 2026-10-17. Discovery continues, and **the user resumed the Lua port of the Robattle on 2026-10-17**:
contract and module split in `D:/claude-brain/Medabots/pc/research/PORT_BATALHA.md` (modules in `pc/game/src/fight/`).

## How to resume (read in this order)
0. **`INDEX.md`**: map of all the docs by subsystem + the big open issues (2026-10-14).
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

## Next discovery tasks (updated 2026-10-14, after the 4th round)
Single flow: agent writes `XX_simbolos.csv` (and `*_criar.txt`/`*_lista.txt` for addresses that must become
functions) -> Opus runs `python -I re/tools/build_sync.py` -> commit -> the user runs `re/ghidra/MedabotsSync.java`
and Ctrl+S. Any 2-3 letter prefix is merged in automatically. Lists that become functions: H_lista, HC_lista,
JT_criar, AS_criar, MS_lista, TX_lista, EXTRA_criar (append new addresses there).

4th round (done, see the .md files): QA1/QA2 (fake functions and internal labels), N1/N2, FN1-3 (the FUN_ left in
Ghidra), CH4/CH5 (no guesses left in R0-R4), NM/NM2 (Portuguese names translated), MS (message engine), TX,
MD/DM (full damage formula), AS1/AS2 (action-script, partial), SV (save<->RAM), EV/EX (event VM and extractor),
BT/BT2 (Robattle flow), OW/OW2 (overworld, speed, camera, warps), GF/GF2/GF3 (graphics catalog), SN/SN2 (sound
engine + 4 bugs in extract_music.py), PS/PS2 (parts screens), MN (main loop), AI/AI2 (CPU decision), IN/IN2
(key items, parts shop), EN (246 enemy templates), RW (reward, medal XP), MF (Medaforce), TQ (60 techniques),
ST/ST2 (scene step blocks), HD (HUD), DB (debug menu), LK (link cable).

0. **PENDING ON THE USER:** run MedabotsSync (it creates ~82 functions: action-script handlers 0x08034524..0x08034E78,
   message states, menus 5/6/10, CPU driver 0x08012B94, debug options, technique validation 0x080332EC, PCM DMA).
   Remove the function at 0x08000000 by hand (it's the ARM ROM header; QA1).
1. **After the sync (blocked until then):**
   - AS3: action-script opcodes 0x01..0x35 and the ones left at 0x36..0x6A (AS1/AS2 pending).
   - MF2: effect of each Medaforce (script 0x50+id; tables 0x083B7F8C/0x083B7E54).
   - TQ conflict: 20 techniques share the script 0x083C6A04 (Sword and Missile the same?): read the script.
   - Document the new functions from EXTRA_criar (menus 5/6/10, CPU driver, debug Item/Mdlpic, PCM DMA).
2. **Without the sync:** GF4 (graphics left: 0x0805BA34, 0x08044D0C, debug/number screens); key item effect
   (table 0x0805D2C8); loss in battle (fi+0x9C vs reward); who fills bot block +0x4C; enemy armor bar
   animation (HUD); trading over link (menu 0x15).
3. **Validate in the emulator** (later, debug ROM): FH/MD/DM formulas, medal fields, message 0x02,
   group/mask/flag of the spawns (EX), FE 1 in songs, speed 0x03006D3A, part detail screen (menu 5 step 66).
4. **PC bugs found (do not fix now, focus is discovery):** extract_music.py (0x90/A0/B0/C0 = 1 byte; 0x81-0x8F rest
   with operand; EC-EF note; FE 1), extract_save.py (parts inventory u16 in the original layout; SV.md),
   extract_events.py (filter chapter/flag; EX.md), C# IsVisibleInChapter (chapter>=16).


## State on 2026-10-15 (after relaunching the cut-off agents)
All of them finished: CS2, JT5, SV2, DC4, DC5, FM3, FS, VA, MF3, TR, TP2 (+ MF4, IV).
- **PENDING ON THE USER:** run MedabotsSync (creates ~540 functions: FS_lista with 376 starts found by a ROM scan +
  EXTRA/TL/FM/TP lists) and delete the function at 0x08000000 by hand.
- **Next round (after the sync):** document the ~400 new functions from FS_lista.txt (split into lists of ~40, one
  Haiku agent per list, like FL1..FL10). Then: field-menu screen on B (FM3), 4 lines k -> option, team pick (TP2:
  needs the emulator), Scout/Conceal (TR), writer of E+0xC28 (VA).
- **PC bugs found (do not fix during discovery):** extract_save.py inventory (IV.md §3), extract_music.py (SN2),
  extract_events.py 0x7D=2 bytes (E2F), save does not cover event flags 0..15 (SV2, possible game bug).
- Validation in the emulator: list in INDEX.md "Abertos importantes".

## State on 2026-10-14 (night, after the review round)
- Agents now read ONLY `re/functions/AGENTE.md` (short primer); the `medabots` agent definition skips the 93 KB
  research README when the task says so. Prompts: see RV*/RM*/RD* launches (review lists of ~60).
- Review done: RV1..RV23 (all 1212 unconfirmed functions), RM1..RM6 (RAM), RD1..RD8 (ROM data), CH6/CH7, FN4..FN6.
  Functions now ~45% confirmado, ~45% provavel, ~9% chute/sem tag, ~2% untouched.
- build_sync: S lines (split a function swallowed by another; QA5_criar), repair of empty-body functions (Thumb +
  body fixup in MedabotsSync), CODE_END 0x08080000 (no functions above), NEVER_FUNC 0x08000000, review rows that
  only confirm a name add the confidence to mtbe.
- **PENDING ON THE USER:** run MedabotsSync (creates ~56, splits 5, repairs empty-body ones, pushes renamed names).
- Open: Conceal/Scout writer (emulator watch, INDEX.md); who sets GS=35 (enter Robattle) and the normal battle
  exit (BT4); +0xAA=0x22 setter (OB); 0x0806DE0C consumer of 0x08485988 (BA2_criar).

## State on 2026-10-15 (emulator validation round)
- Validated in mGBA with the user's recordings (data/records/rec_7..rec_14, see its INDEX.md) and savestates .ss2/.ss3:
  BT7 (battle entry/exit; GS is per menu), TR6 (Scout = Z_0E, Conceal = AA_0A, cap 160/120, accumulate),
  SS3 (Melt ticks but never breaks a part; Bug no damage; encounter 17..32 steps), DS (Sacrifice: M_04 breaks the
  actor's used part even on a miss). Rows with "emulador ... confirmado" now win every tier in build_sync.
- Emulator tips: replay_record.py takes ONE --watch, needs a window @A-B and LEN 4 (1-byte watch never fires);
  recordings where the user reloaded a state mid-way (rec_14) are not reproducible.
- Ghidra is in sync (last MedabotsSync: 0 failed). Open: the step that zeroes the target part on a Sacrifice hit.

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
