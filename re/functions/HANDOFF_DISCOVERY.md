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

## Next discovery tasks (suggested priority)
1. **Continuation table of event opcodes, `0x083D2630`** (see HC.md). `HC_lista.txt` is ready with the 55 targets
   that are not functions yet.
   - **The user needs to run** `re/ghidra/CreateHandlerFunctions.java` with `HC_lista.txt` and save the project.
   - Then document them in HC1..HCn.md (`Event_OpXX_Cont_<Name>`).
   - 0x0804DC34 is the common stub (53 ops, NoOp).
2. **Unnamed battle helpers that recur in many handlers** (high value):
   - FUN_0803725C, FUN_08037280 (common start AccumRosPart -> ApplyStatusCostIfFlag -> 0x0803725C).
   - FUN_080376F4, FUN_080370AC, FUN_08036AC4, FUN_08036E54, FUN_08036B84, FUN_08036D24.
   - Fight_RollPartValue.
   - Fight_RunSubstateDispatchC (second substate dispatcher, AJ 0C..13).
   - Who reads the codes at +0xA6 (0x1A, 0x1C, 0x1D, 0x25, 0x2B, 0x2C, 0x2D, 0x39).
3. **Indirect jumps Ghidra did not recover** (jump tables "UNRECOVERED"): HA2 E05/G02, HA3 H:0x3E (0x0801F28A),
   J:0x02 (0x0801FBC4, 5 entries + fallback 0x0801FD70), L:0x0B (0x0802040C), HA1 E/05 (0x0801DADE). Read the table
   from the ROM and fix it with set_disassembly_comment / document the targets.
4. **Leftover Ghidra functions**: list with `mcp__ghidra__list_functions` and filter names `FUN_*` that aren't in
   mtbe.csv (functions that are neither BL targets nor in handler tables: callbacks, IRQ, and the ones created now).
   Also the 62 generic names, 55 with no name and 19 named "?" that the merge ignored (see MERGE_RELATORIO.md).
5. **Pending conflicts** (MERGE_RELATORIO.md; 258 cases kept the mtbe name). Needing a decision:
   - 0x0803579C: Fight_GetPartProficiencyBase vs Fight_AccumRosPart.
   - 0x08034F68: do_damage_on_part vs Fight_ApplyDamageToPart.
   - 0x080183A4: mtbe says "Medabot preview screen cursor"; an agent says "3x3 battle cursor".
   - 0x08047BA8: the mtbe name Overworld_StartMapTransitionEffect is wrong (it only calls sfx); no new name yet.
   - 0x08045A40: may be a literal pool, not a function.
   - 0x0804305C vs 0x08042E58: possible duplicate composite-sprite routines.
   - E/01, E/05 (HA1): mtbe ActionFamily_CancelGravHandler etc. vs the code (loads graphics by follow-up +0xDF).
   - 0x08036FF8 Fight_SpawnInlineEffectTextObjects: its use in M 0x00 looks like sprite creation.
   - Duplicate names after the merge: Map_TickObjectAnimation (0x08049440 / 0x080508B4), snd_track_step
     (0x08072D68 / 0x08074118).
6. **Data tables**: name the ROM tables cited in the literal pools (part, technique, medal, text, motion tables)
   in mtbe.csv with kind `data`, and document the format of each one.
7. **Validation in the emulator** (later): replays to confirm the "provável" entries in key systems (damage,
   RNG 0x080424B8, encounter, collision, Medaforce).

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
