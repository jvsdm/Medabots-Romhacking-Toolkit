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

## Next discovery tasks (updated 2026-10-13, after the 10-agent round)
Done this round: HC1..HC5 (all 55 event continuations), FH (battle helpers), JT (5 of the 6 "UNRECOVERED" jumps
were a normal return; only J:0x02 has a real table, 0x0801FBEC), HA2-resto, DT (16 tables), FL (list of 940 FUN_).

0. **User step in Ghidra:** run `re/ghidra/ApplyMtbeNamesToFUN.java` (pick `re/symbols/mtbe.csv`) and save.
   402 of the 940 FUN_ already have a name in mtbe.csv. The script only renames FUN_* and touches nothing else.
   **Never** use MedabotsSetup.java on this project (it removes functions and clears the listing).
1. **Merge into mtbe.csv** all the *_simbolos.csv not merged yet: HA1..HA8, HC1..HC5, FH, JT, DT, FL (same
   script as the previous merge; see MERGE_RELATORIO.md). Name divergences to decide:
   - FH: 0x08037280 (mtbe Fight_ComputePartValue), 0x0803725C (FightPreview_ClearQueueFlag / HN1
     Fight_CheckRosMeetsMinimumThreshold; real = Fight_SpendRos20ReturnsExhausted), 0x08040CD0
     (ClearObjectAttributeEntries vs Tile_ClearSlots), 0x08036B84 (FightPreview_SetPartSpriteId, wrong).
   - HA2: 0x0801D738, 0x0801DD30 have other names in mtbe. HC1: 0x0804B758 (mtbe "Skip3Bytes" is wrong; it stores
     the args).
   - Fix the old notes: HA1/HA2/HA3 still say "tail-call not recovered" for E05/G02/H:0x3E/L:0x0B (it's a normal
     return; see JT.md). HE4 0x76/0x7A/0x7F (see HC5.md). HE2 0x32 (see HC3.md).
2. **FL1..FL10 done (2026-10-13):** about 385 of the 409 candidates documented and renamed in Ghidra.
   - Leftovers by file (see the "Pendente" line in each FLn.md):
     - FL2: 6
     - FL3: 1 (0x0806F5A0, 4.9 KB)
     - FL4: 17
     - FL5: 5
     - FL6: 1
     - FL8: 5
   - Not renamed (guesses): about 30, listed in the CSVs with an FUN_ name and the proposed name in the comment.
   - Uncovered callees: FUN_08066D8C (text typewriter), FUN_08047850, FUN_08037830, FUN_08003300, FUN_08003904,
     FUN_08048BB8.
   - Byte-indexed dispatch tables not recovered (ctx+0x33 / +0x1B, CallViaR0): 0x0801387C, 0x08040974,
     0x0803C7DC, 0x08065A94, 0x08071630, 0x080575DC, 0x08065ED0, 0x08068BC4, DAT_0800BAF0, DAT_0803C6EC,
     DAT_0805CB80. Same approach as JT.md.
   - **FLn_lista.txt sizes are wrong** (up to the next FUN_); use the Body from get_function_by_address.
   - 0x08302DF8 is data, not code.
3. **+0xA6 code -> message/script map:** the reader is 0x080305E0 -> Fight_RunSubstateDispatch 0x0801C0BC
   (table 0x083AD580, 9 entries) -> FUN_0801AC24 -> StateMachine_DispatchStep(table 0x083C6744 = scripts at
   0x083C4E24+). Decode that script format.
   - Table 0x083AD5A4 (Fight_RunSubstateDispatchC) is not decoded either.
4. **DT, remaining tables:** handlers (0x083AF8C0, 0x083AF180, 0x083D222C/0x083D2630), gMapArea 0x083D2B6B,
   gBotNames 0x083BA658, gTechniqueNames 0x083BE868, gMoldTable 0x083D1D78, pre/post-battle texts
   0x083C2D3C / 0x083C3B04. Use rename_data in Ghidra.
5. **Validate in the emulator** (later): 0x02 message B (where the box appears), 0x29/0x2A (ends at the first
   actor?), 0x41 (does it hang with no slot?), damage/ROS (FH formulas), RNG.

Damage/ROS formulas (FH, confirmed by decompile):
- Fight_RollPartValue 0x0803769C: fi+0xE8 = bot+0xAB + medal bonus + (randbyte&7) + specialty[bot+0xA9].
- 0x08037280: fi+0xEC = max(1, ((fi+0xE8>>2) + part.power) / part.scalar).
- 0x0803725C: fi+0xE8 -= 0x20 (clamped at 0); returns 1 if it ran out.

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
