# Expanding Medabots and parts — Medabots: Metabee (Europe), GBA

Technical write-up of the project that expands the Medabot/part roster of the European release of
**Medabots: Metabee** (Medarot 2 Core), including a port of all 60 Medarots from **Shingata Medarot**
(same engine).

> Status as of October 2026: ROM with **181 bots** (120 originals + Sägehorn + 60 from Shingata),
> tested on mGBA 0.10.5: shop, inventory, save/load, equipping, battle (including destroyed parts →
> Tinpet), part list pages, large part images, battle animations and movement, mini sprite colours.
> Fixed Tinpet ID (236/237). History in [section 12](#12-changelog).

All addresses are **file offsets in the Metabee (E) ROM** (`MEDABOTSMTBVA8BPE9`) unless stated otherwise.
`0x08…` values are GBA pointers; `0x02…` is EWRAM; `0x03…` is IWRAM.

---

## 1. Overview

| Problem | Solution |
|---|---|
| Bot/part tables have a fixed size (120) with no room to grow in place | ROM expanded to 16 MB; tables rebuilt at `0xC00000` and **every code pointer repointed** |
| 119/120/121 constants scattered through the code | 66 constants replaced with `N-1` / `N` / `N+1`, classified using Shingata as a reference |
| Tinpet uses IDs 120/121 (`gender + 120`) | Tinpet **fixed at 236/237**, independent of `N` (saves survive appended bots) |
| 960-byte inventory in IWRAM, no room left | Moved to EWRAM `0x0202E000`, 240 slots per part type; save/load now write it byte by byte |
| Part list hard-coded to 30 pages | Page wrap-around changed to `ceil(N/4)` |
| Part image selector is 7 bits (128 per mold; biped arm molds full) | 2-instruction patch: 8-bit selector (256 per mold) |
| Addresses differ between releases (EU/US, Metabee/Rokusho) | Tables located through the game's own code (literal pools of the accessor functions) |

None of this requires rewriting game routines: it is data relocation, swapped immediates and one
two-instruction patch. The game code stays the original.

---

## 2. Per-bot / per-part tables

### 2.1 Accessor functions (entry points)

| Function | What it does |
|---|---|
| `0x042B58` `GetPartStats(bot, kind, dest)` | copies 16 bytes from `PART_STATS + bot*64 + kind*16` |
| `0x047BDC` `GetBotName(bot)` | `BOT_NAMES[bot]` |
| `0x047BEC` `GetPartName(kind, bot)` | `PART_NAMES[bot*4 + kind]` |

Part = `bot*4 + kind` (0 head, 1 right arm, 2 left arm, 3 legs).
Part IDs stored in battles, shops and assembled Medabots are **1 byte** (= bot ID).

### 2.2 Tables (Metabee E)

| Table | Offset | Entries | Bytes/entry | Code refs |
|---|---|---|---|---|
| Part stats | `0x3B83FC` | 480 | 16 | 2 |
| Bot names | `0x3BA658` | 120 | 4 (pointer) | 2 |
| Part names | `0x3BBB4C` | 480 | 4 (pointer) | 16 |
| Battle sprites (Malias) | `0x3B1A08` | **122** (+Tinpet M/F) | 24 (6 pointers) | 5 |
| Palette family (= mini sprite OBJ bank) | `0x3BEB94` | 122 | 1 | 3 |
| Battle movement style (0–6) | `0x3CAD78` | 122 | 1 | 1 |
| Head preview | `0x07EE70` | 122 | 16 | 5 |
| Right arm preview | `0x07F610` | 122 | 16 | 8 |
| Left arm preview | `0x07FDB0` | 122 | 16 | 8 |
| Legs preview | `0x080550` | 122 | **32** (`bot << 5`) | 6 |

The graphics tables have **122** entries: 120 bots + male Tinpet (120) and female Tinpet (121).

### 2.3 Part record (16 bytes)

| Byte | Head/arms | Legs |
|---|---|---|
| 0 | compatible medal | compatible medal |
| 1 | technique (index into the technique name table at `0x3BE868`, 60 names) | leg type (100–106) |
| 2 | speciality (Strike, Berserk, Shoot, Aim, Defend, Heal, Support, Interrupt) | — |
| 3 | female (0/1) | female |
| 4 | armor | armor |
| 5 | success | propulsion |
| 6 | power | evasion |
| 7 | chain reaction | defense |
| 8 | uses / charge | proximity |
| 9 | radiation | remoteness |
| 10 | bot "rank" (same on all 4 parts) | same |
| 11 | **shop price** in whole pounds (54 → £54.00; 0 cannot be bought) | same |
| 12 | flag (1 normal, 32 on some bosses) | same |

### 2.4 Shop

`0x4838E4`: 15 shops × 4 slots, each slot a **bot ID** (`0xFF` = empty). Can be located with the
signature `13 00 FF FF 13 00 42 FF 13 00` (used by the Randomizer).

---

## 3. Constants that depend on the bot count

Engine rule (confirmed on Shingata, same engine with 60 bots): with `N` bots,
last real ID = `N-1`, count = `N`, male Tinpet = `N`, female Tinpet = `N+1`.
In the expansion Tinpet is fixed at `T = 236` (`patches.TINPET_ID`); since count and Tinpet coincide
in Shingata, each site was reclassified by reading the code.

| Group | Count | Example | Becomes |
|---|---|---|---|
| "Is this a real part?" / loops / cursor wrap (`cmp/mov #0x77`) | 45 | `0x005A70`, `0x043830` | `N-1` |
| "Is it the male Tinpet?" (`cmp #0x78`, part cycle `0x436AC`) | 2 | `0x0436D0` | `T` |
| Tinpet (`adds #0x78` = gender + 120) | 9 | battle `0x015862`, creation `0x042246` | `T` |
| Shop scroll (`adds #0x78` on `0x2023000`) | 5 | `0x061D6C` | `N` |
| Female Tinpet (`#0x79`) | 2 | `0x043752` | `T+1` |
| Last page of the part list (`#0x1D`) | 5 | `0x00F0A8` | `ceil(N/4)-1` |

Full list, with the source of each site, in `tools/patches.py`.

### 3.1 Shop (buy/sell) and bots with ID ≥ 128
- **Signed 1-byte counters**: the shop's `0..N-1` loops do `lsls #24 ; asrs #24` on the counter and the index.
  With more than 128 bots the loop never ends (freeze when buying) and the index goes negative. 7 `asrs` → `lsrs`
  (`0x062428`, `0x062680`, `0x0626F0`, `0x0623D8`, `0x06240A`, `0x062656`, `0x0626C6`).
- **120-item arrays**: quantities at `0x2023000 + i` and items (8 bytes: bot, quantity, price×100) at
  `0x2040000 + i*8` (mirror of `0x02000000`). The **scroll position** sits right after the quantities
  (`0x2023078` = base + 120); with N items it moves to `base + N` — 5 `adds #0x78` and **9 literals**
  (`0x05B3E8`, `0x05B9C0`, `0x05BA14`, `0x061954`, `0x061E84`, `0x0620BC`, `0x0620F4`, `0x0623F4`, `0x062804`).
  The last item (`0x020403B8`, literal at `0x05B178`) becomes `base + (N-1)*8`. The areas that grow were free.
- **Trap**: the bytes `78 30` of a `0x02023078` literal look like `adds r0, #0x78`; three literals were mistaken
  for Tinpet math in a first version (scroll written in one place and read from another).

**Method** (useful for any engine that ships in two builds of different sizes): for each `#0x77/#0x78`,
mask a byte window around it (the immediate, BLs, branches and `ldr [pc]`) and search for the same
window in Shingata. If the value there is `0x3B/0x3C` (59/60), it is a count limit. Sites with no match
were classified by reading the surrounding code. **Watch out**: `0x1D` is also "last medal" (30 medals)
in ~40 places, and `0x78` shows up in coordinates, sizes and sound effects (`0x04C996` = sound 120).

---

## 4. Inventory and save

- Original: IWRAM `0x03004C40`, 4 blocks of `0xF0` (120 bots × 2 bytes), one per part type.
  Address = `kind*240 + bot*2`. The 240 is computed in code as `kind*15 << 4` in **20 places**.
- Entry byte 0 = quantity in stock (max 99); byte 1 = total/equipped quantity.
- New: EWRAM `0x0202E000` (region confirmed free by an 88,000-frame RAM scan and with no code
  references), `<< 4` → `<< 5` (480 bytes per type = 240 slots).
- Save/load (`0x5E010` / `0x5DF18`): the original writes each inventory byte as a **halfword**
  (`0x780` bytes in the buffer, offset `+0x7AC`). Switching to byte-by-byte makes the 240 slots fit
  **exactly in the same space** in the save. Saves made with the original ROM are not compatible.
- Other adjustments: new-game clear (`0x4A2F0`), counters (`0x440D4`, `0x5D860`) and the starter bot
  routine (`0x44B58`, rewritten at the same size).
- Full save: `0x1CBC` bytes with a checksum at `+0xFF4` (16-bit sum of bytes `0x0000–0x1CBB`, field zeroed);
  8 KB EEPROM → ~830 free bytes at the end. In mGBA's `.sav` every 8-byte block is stored reversed.

---

## 5. Graphics

### 5.1 Battle sprites and palettes
6 **Malias** blocks (header `Le`) per bot: the mini sprite (16×16 per component, legs 0x180 bytes).
Each component uses OBJ bank = the "palette family" of the part's bot. The EU game loads 4 palettes
from `0x50F498` into banks 0–3; the 4th is all zero. Shingata has **its own palettes**
(`0x58E0A0/C0/E0`), so ported tiles are remapped (`tools/mini_palette.py`): an extra palette computed
into bank 3, the lowest-error bank per bot, and a +15% value / −10% saturation tweak. Only bank 3 is
free during battle.

### 5.1.1 Battle movement
`MOVE_STYLE` (`0x3CAD78`, 1 byte per bot) selects the attack movement script, indexed by the
**legs' bot**: `0x44A3C` reads `0x807E8EC[kind*7 + style]`. Battle position of the assembly root:
EWRAM `0x20260E8` (y, 8.8 fixed point) / `0x20260EA` (x).

### 5.2 Large part images (previews)
Each preview entry (4 bytes) = **mold** (bits 0–5) + **selector** (bits 6–13) + pose variant
(bit 15) + centered flag (bit 14).

- **Mold** (descriptor): shape and position of the piece; 0x18-byte records referenced by a pointer
  table at `0x3D1D78` with **50 entries** (another table starts right after it, at `0x3D1E40`).
  Record bytes `0x0F` and `0x10` = child molds. IDs 1–2 = assembly roots; 3–6 = joints built in RAM;
  7–49 = real molds (all in use).
- **Selector**: picks that bot's art from the mold's blob list, stored as pairs
  `[LZ77 image, 32-byte palette]` (palette == image means "no palette").

### 5.3 The 7-bit limit and the patch
At `0x45E6C` the game computes `(((entry >> 6) + variant) & 0x7F) << 1`
(`lsls #25 ; lsrs #24` at `0x45ECC`). The biped arm molds (14, 15, 17, 18) already use 113–127 of
their 128 slots in the original game. Patch:

```
0x045ECC: 0x0652 -> 0x0612   (lsls r2, r2, #25 -> #24)
0x045ECE: 0x0E12 -> 0x0DD2   (lsrs r2, r2, #24 -> #23)
```

8-bit selector = 256 slots per mold. Safe for the original bots (none of them sets bit 13).

### 5.4 Why molds cannot be cloned
Abandoned approach: cloning full molds into free IDs (50–63). In battle, each piece's state lives in
an array **indexed by mold number**, with 50 entries of 16 bytes (EWRAM `0x20260D0`), immediately
followed by the enemy's array. Mold 60 lands on the enemy's entry 10: pieces "fly off" and the enemy
animates pieces that are not its own. Found with an mGBA watchpoint on `0x2026491`
(function `0x45E6C`, `r4 = base + mold*16`).

---

## 6. Porting Shingata Medarot

- Same engine: `GetPartStats` is byte-for-byte identical. Tables located through the same functions
  (Kabuto: stats `0x467238`, names `0x4682C4`, parts `0x4689BC`, sprites `0x461168`,
  palette `0x46A72C`, previews `0x091A98…`, molds `0x47A93C`).
- **Stats** use the same scale; **techniques** use the same IDs (checked against the name table);
  **medals** 0–26 are in the same order (Shingata bots only use 0–24). Nothing is converted.
- **Text**: Medarot Navi charset (mdr-navi-i18n) + dakuten codes `0x38–0x51`, `ー` = `0xBC`,
  `・` = `0xBD`, learned by aligning the ROM with the Medapedia names (60/60 with no conflicts).
  Shingata's hiragana is not mapped yet (3 bots have placeholder names).
- **Molds**: all 50 are identical between the two games; a ported bot's art goes into a free slot of
  the same mold (128–255 after the patch).
- Capacity: all 60 fit; after them ~61 slots remain in the hand molds (15/18).

---

## 7. Locating tables through code (version-independent)

The fixed constants in Medabots-Romhacking-Toolkit only hold for Rokusho (E); Metabee (E) has offsets
of −0x20, −0x24 and others, and Rokusho (U) goes as far as −0x15C4. Fixed addresses do not work.

Method: for each table, take the function that reads it, mask the `ldr [pc]` immediate, BLs and
branches, and store the pattern. On the target ROM, find the (unique) pattern, read the literal and
add a delta. This works on every release **and** on expanded ROMs (the code already points at the new
table). 40 tables covered (34 by signature, 6 derived from neighbours). Implemented in the Toolkit fork
(`Layout/RomLayout`, `romFile.Layout.XxxOffset`); signatures generated by `tools/gen_signatures.py`.

---

## 8. Current limits

| Limit | Value | Cause | How to go further |
|---|---|---|---|
| Bots | 236 | Tinpet fixed at 236/237 inside the 240 inventory slots | raise `TINPET_ID` together with the inventory (section 9) |
| Bot IDs | ~252 | 1-byte ID in RAM, save, battles, shops and events; `0xFF` = empty | see section 9 |
| Slots per mold | 256 | 8-bit selector (bit 14 is a flag) | — |
| Molds (shapes) | 50, all in use | 50-entry battle state array | enlarge the battle RAM array |
| Extra bipeds after Shingata | ~30–60 | hand molds 15/18 | same |

---

## 9. Going past 240 / 256

- **Moving tables elsewhere in the ROM is not a problem**: pointers are 32-bit and we already do it.
- **240 → ~252**: the bottleneck is inventory space in the save. There are ~830 free bytes in the
  EEPROM after the save block; slots 240+ (8 bytes per bot) could be written there by extending
  save/load. Feasible.
- **Beyond 256**: the 1-byte ID is everywhere (assembled Medabot, battles, shops, event scripts,
  save). An "extended" accessor function would cover the tables, but not the bytes stored in
  RAM/save/scripts. It would need a "bank" scheme (ID + page) touching every structure —
  in practice, decompilation/reimplementation territory.

---

## 10. Tools (`tools/`)

| File | Purpose |
|---|---|
| `build.py` | original ROM + `data/bots/*.json` → expanded ROM (options `--shop`, `--preco-teste`, `--out`) |
| `patches.py` | every constant/patch, with its source documented |
| `extract.py` | 120 original bots → `data/bots/NNN_name.json` |
| `shingata.py`, `port_shingata.py` | extract and port the Shingata bots |
| `gfx_port.py`, `codecs_gba.py` | graphics copying (Malias, LZ77, molds/selectors) |
| `mini_palette.py` | extra palette and colour remap for ported mini sprites |
| `render_capture.py`, `analyze_captures.py` | rebuild the sprite layer from captures / per-capture summary |
| `gdbrsp.py` | client for mGBA's GDB server (registers, memory, watch/break) |
| `export_manifest.py` | generates the expansion manifest embedded in the Toolkit |
| `save_edit.py` | edits the `.sav` (`--todas-pecas` = all parts): un-reverses mGBA's 8-byte blocks, recomputes the checksum, keeps a backup |
| `xrefs.py`, `crossref_shingata.py`, `bot_tables.py`, `gen_signatures.py` | static analysis |
| `render_bot.py`, `oam_film.py` | visual checks |
| `ewram_scan.lua`, `inv_dump.lua`, `battle_capture.lua` | mGBA scripts (free RAM, inventory, animation capture) |

Bot format (`data/bots/NNN_name.json`): `bot_id`, `name`, `parts` (name + named stats) and, for new
bots, `graphics_from` (copy graphics from an original bot) or `graphics_source` (copy from another ROM
of the same engine; `native_previews` option to use a native bot's large image).

Note: `--preco-teste` ("test price") sets a temporary shop price for new bots and for bots placed
with `--shop`, without changing the JSON files.

### Debugging with mGBA
- Lua scripting in 0.10 can only read memory per frame. To find out what writes an address, use
  **Tools → Open debugger console**: `watch/c <address>`, `continue`, `info registers`.
- Saves carry over between builds as long as bots are only appended (fixed Tinpet). Save states
  depend on the ROM: prefer resetting and loading the save.

---

## 11. Open issues

- **Shops**: only shop 0 is set by the build (`--shop`); the new bots still need distributing.
- **Shingata hiragana** and official names for the ported parts (currently provisional romanization).
- **Toolkit**: reads and generates the expanded ROM. Still to do: editing the list of new bots in the
  UI (today the manifest comes from `tools/export_manifest.py`) and shop/price options in the dialog.

### Feature requests (to evaluate later)

**Medals: edit and, maybe, add** — requested 2026-10-06, on hold.
- *Reason to confirm:* look at later games in the series (e.g. Medarot 8, see `ref/Medals in Medarot 8.txt`)
  for medals that would justify growing the list, not just editing the current ones.
- *Known so far:* 27 medals (0–26), 16-byte record at `0x3B62D8` (Metabee E; the Toolkit already finds
  it through code) plus a per-class speciality table. Hints of an internal limit of 30 (`#0x1D` = "last
  medal"; the validator accepts medal < 30): maybe 3 slots without code changes.
- *Difficulty:* Shingata has the same 27 medals, so it cannot act as a Rosetta stone as it did for bots;
  the constants would have to be classified by reading the code.
- *First steps if approved:* (1) map the 16-byte record and list the 27 medals; (2) test slots 27–29;
  (3) only then decide whether going past 30 is worth it (tables, constants, save).

## 12. Changelog

Dates are October 2026. Hashes are commits in the tools repository.

### Toolkit: new portraits, messages, paint tools — fork `e76a8b7`…`c044f5e`
- **Maps work like bots**: new maps are staged in the project and all go in on Export; rename with a
  double-click; "+" > New map (empty up to 64×64, blank tileset or another map's, optional PNG) and Copy map;
  map_id is a dropdown in the event editor; maps open centred.
- **Paint tools** for maps and sprites: pencil, line, rectangle, filled rectangle, bucket, picker,
  undo/redo (P, L, R, F, G, I, Ctrl+Z, Ctrl+Y).
- **Message preview**: each box as the game shows it (real font and widths, left portrait mirrored).
  Saving edited text writes `<NL>`/`<NB>` where the game would break (§14.1).
- **New portraits** (green + on the Sprites tab): a new character as a copy of another (its own data) or
  blank, up to 256; character names in the project (§14.2). Test: `build/Medabots_expanded+teste_retrato.gba`.

### Toolkit: bots in the editor, dark mode, events, new maps and tilesets — fork `0066ee5`…`ad417fb`
- **New Medabots** (green + in the Parts tab): a bot built in the editor on a base bot's body (names
  and stats start from the base; list fields use the part editor's names: attribute + medal, technique,
  speciality, leg type); every piece (large images and the 8 mini sprite frames) takes a PNG, with
  template export. Several bots form a **list** that can be saved and reopened (JSON + `<list>_art/`
  folder) and go into one ROM at once (`BotAdder.AddAll`).
- **Dark mode** (View > Dark mode, remembered). Shop/part price: the byte is the price in units
  (11 → 11,00); the "×100" is gone.
- **Events — two bugs fixed.** A new event ID moved the table without re-encoding its pointers
  (relative to the base): all 2288 events broke. In the 16 MB expanded ROM free space lay past the 4 MB
  the banks reach: a growing event could not be written. The game (0x0804D83C) finds event N at
  `base + bank×0x4000 + (pointer − 0x4000)`, reading both offsets from the header; scripts refer to each
  other only by ID and relative jumps. Table and scripts now move together (base literals at
  0x4A584/0x4A8C8/0x4B124 repointed) when the table grows or free space is out of reach. Free space for
  every edit starts after the last used byte (≥ 8 MB): inside the 16 MB in the expanded ROM.
- **New maps** (green + in the Maps tab). The map ID is a byte (RAM 0x3005FDE) used directly as an
  index, never checked against 192; tracing it through the code found 16 per-map tables (the 11 the
  Toolkit knew + two area tables 0x3D2B6B/0x483774, an encounter extra table 0x3BF210 and pointer tables
  0x409480/0x40F880), each loaded by 1-4 literals and referenced by nothing else. The expansion moves
  the 16 to 255 entries and writes the `MBXMAPS` marker (maps in use, capacity) before the layer table;
  a new map is a copy of an existing one (layers, tileset, palette, collision, music, area), with warps
  and people empty or copied. Test: `build/Medabots_expanded+mapa_teste.gba` (Riverview school ground
  floor left classroom door → map 192).
- **Map tileset and palette as PNG** (Maps tab, Layer Editor): export the sheet (16 tiles per row, each
  tile in the palette bank it is used with, colour 0 transparent) and a 16×16 palette grid; import back
  for every map using the tileset or only the current one (project patch, written on export).

### Optional Shingata and "Add Bot" in the Toolkit — fork `fd2aa0a`
- The expanded ROM can be built **from the EU ROM alone** (120 originals + Sägehorn, 121 bots); the
  Shingata ROM is now optional (`build.py --sem-shingata` matches the Toolkit without it).
- **File → Add Bot to ROM...** appends a bot to an expanded ROM from a `data/bots`-style JSON (`name`,
  `parts` with stats, `graphics_from`, optional `recolor`; example in `examples/bot_exemplo.json`). The
  bot takes the next ID (rows already reserved up to 235), recoloured images go to free selector slots
  (original 128-slot lists are widened to 256 when needed), the mini sprite is mapped onto the 4
  palettes already present and the N constants move to N+1, checked against a ROM generated from
  scratch. The ROM's saves stay valid.
- Not yet: bots with their own drawings (`art`) through the Toolkit (only `build.py`).

### Own colours for bots that reuse drawings (Sägehorn)
- `recolor` field in the JSON (`"#rrggbb": "#rrggbb"`): the source bot's large-image palettes are
  copied with the colours swapped and wired to new selector slots (same image); pieces without a
  palette of their own use the main piece's. The mini sprite goes through the remap with the swapped
  palette (without the Shingata HSV tweak). Sägehorn: Metabee's drawings in brown, navy and red eyes.

### European italic font repaired
- Italic text (between `F8` codes, e.g. "Click!", message 0:263) is broken **in the European ROM
  itself**: the italic font (`0x4C59AC`, 128 8×16 glyphs, read by the text code at 0x40144–0x40504)
  lost its dark shades B–F (now 0 = transparent) and 8 (now 5). The glyph shapes are intact.
- The build and the Toolkit rebuild every lost pixel from its number of inked neighbours (4 → F,
  3 → E, fewer → D); 92% of pixels match a hand-fixed reference font. Runs only when the font shows
  the damage and needs nothing but the EU ROM.

### Toolkit opens the expanded ROM; events through code — fork `8b5d295`
- The "relocated event table" scan accepted any `00 40` past 0x800000 and hit false positives in the
  expanded data (217 of 2288 events threw → "Open Failed"). The base now comes from the literal the
  game loads (0x4A3B6), which the Toolkit itself repoints when relocating the database.
- Every profile held Rokusho (E)'s address: Metabee (E) is `0x769EC0`, Rokusho (U) `0x768CB0`.
- Battles with Tinpet (120/121): all 93 hits are filler slots (enemies past the battle's count), never
  loaded; no enemy in use has a part > 119. Nothing to fix.

### Toolkit generates the expanded ROM — fork `58ecf6a`
- **File → Generate Expanded ROM...** asks for the Metabee (E) and Shingata Kabuto ROMs (headers
  checked), saves to a new file (the sources are never overwritten) and opens the result.
- `Medabots.Rom.Expansion` mirrors `tools/build.py`. The embedded manifest
  (`tools/export_manifest.py`) only holds what neither ROM has: Sägehorn, the bot order and the
  Medapedia names; stats and graphics come from the ROMs.
- **Byte-identical output** to the Python build (automated test against
  `build/reference_expanded.gba`), including the k-means extra palette.
- Test bot 181 (MULTIKOLOR-C) removed: the official expansion has 181 bots (0–180).

### Toolkit reads the expanded ROM — fork `eef915f`
- `RomLayout.BotCount` (Medabot validator immediate, `0x43820`), `TinpetId` (bare Medabot creation,
  `0x42238`) and the name tables (`GetBotName`/`GetPartName`), all through code signatures, checked on
  Rokusho E/U, Metabee E and the expanded ROM.
- The part reader, battle sprite bounds, shop/starter pickers and names missing from the catalog now
  follow the ROM. 200/203 tests (the 3 `LargePartDisplay` failures predate this).

### In-game validation (2026-10-06)
- Save with every part (`tools/save_edit.py --todas-pecas`) loaded fine: all 182 bots show up in
  the inventory.
- Battle animations and movement checked on several ported bots beyond the ones used in the A/B tests.
- Mini sprite colours approved (HSV tweak).
- **Fixed Tinpet (236/237) validated**: parts removed on the Medabots screen become Tinpet, the part
  cycle walks through the ported bots without showing empty slots, and a part broken in battle turns
  into Tinpet without freezing (Predicador, bot 136).

### Page total in the part list ("/30")
- Malias tilemap at `0x4F1514` (BG1, 32 columns, stored tile = VRAM − `0x200`). The 7 bytes of the
  "30" (`0x4F1560–63`, `0x4F1580–81`, `0x4F1583`) are literal and not shared in the compressed
  stream; the build replaces the fixed artwork with the page number font (VRAM `0x360 + 2·digit`,
  palette 7) using `ceil(N/4)`. Found by reading VRAM from mGBA save states (`gbAs` chunk, VRAM at
  `+0x1000`).

### Fixed Tinpet ID
- Tinpet is now **fixed at 236/237** (`patches.TINPET_ID`), independent of `N`. IDs `N..235` are
  empty (filler rows in the tables; zero inventory, so the part cycle skips them).
- The `#0x78` sites were split by role: "is it Tinpet?" (`0x0436D0`, `0x04374E`) and
  `gender + 0x78` → `TINPET_ID`; the 5 shop `adds #0x78` are the scroll variable address → `N`.
- Saves with a Tinpet equipped survive new bots (as long as bots are only appended).
  Ceiling: 236 bots (59 full pages).

### Colours of ported mini sprites — `de1a158`, `c4fb9d2`
- Shingata has its own mini sprite palettes (`0x58E0A0/C0/E0`); the earlier comparison against the
  block identical to EU (`0x58DF40`) was wrong — that block is unused.
- The EU game loads 4 palettes from `0x50F498` (OBJ banks 0–3) and the 4th is all zero: the build
  writes an extra palette there (k-means over the ported bots' colours), picks the lowest-error bank
  per bot and remaps the tiles. Style tweak: +15% value, −10% saturation.

### Battle movement of non-bipeds — `073e782`
- The **MOVE_STYLE** table (`0x3CAD78`, 1 byte per bot, 0–6) selects the movement script
  (`0x44A3C`: `0x807E8EC[kind*7 + style[legs bot]]`). It was not relocated: bots ≥ 120 read garbage
  (Valentorero = 12) and walked along the top of the screen. It now moves with the graphics tables;
  in Shingata it lives at `0x4748D4`.

### Shop with IDs ≥ 128 and N bots — `8c57aaa`, `c579ee2`, `b719bd4`
- 7 loops with a signed 1-byte counter (`asrs` → `lsrs`) froze when buying parts ≥ 128.
- Scroll variable at `0x2023000 + N` (9 literals) plus the last-item literal; 3 literals had been
  mistaken for the Tinpet `adds #0x78`.

### Shingata port — `aabf38a` … `87637a2`
- 60 bots ported with graphics (bots 121–180), part names from Medapedia.
- Part image selector widened from 7 to 8 bits (patch at `0x45ECC`, 256 slots per mold).
- Mold clones (IDs ≥ 50) dropped: battle part state is a 50-entry array indexed by mold
  (`0x20260D0`), immediately followed by the enemy's array.

### Expansion foundation — `73fd11d`, `8265870`, `52ce839`
- 16 MB ROM, tables at `0xC00000`, 66 constants driven by `N`, EWRAM inventory with 240 slots,
  byte-wise save, `ceil(N/4)` pages, Sägehorn (bot 120).
- Toolkit: addresses resolved through code (literal pool signatures).

### Diagnostic tooling — `9df86e2`, `bc6866e`, `7b55f93`, `8ce1135`
- `battle_capture.lua` (per-frame OAM/IWRAM/EWRAM/VRAM, L+R), `render_capture.py`,
  `analyze_captures.py`, `gdbrsp.py` (mGBA GDB server).
- A/B experiment bots (ROKUSHO-C, MULTIKOLOR-C) to isolate causes.

---

## 13. Maps, events and limits

Validated in game on 2026-10-07: map 192 (a copy of classroom 3, linked to the upper-left door of the
Riverview school ground floor hallway) loads complete, with its people, and its exit returns to the hallway.

### 13.1 Maps
- **The map ID is one byte** in RAM at `0x3005FDE` (base `0x30057E0` + `0x7FE`), used directly as an index by
  ~60 routines, **never compared with 192**. New maps therefore only need room in the tables.
- **16 per-map tables** (EU; the Toolkit's schema offsets − 0x20), found by tracing the ID through the code
  (`tools/mapflow.py`):

| Table | EU offset | Bytes/map | Code refs |
|---|---|---|---|
| Layers (3 LZ77 pointers) | 0x3F9080 | 12 | 2 |
| Encounters | 0x3BEC10 | 8 | 4 |
| Encounters (extra) | 0x3BF210 | 4 | 1 |
| Area / region (0-10) | 0x3D2B6B | 1 | 2 |
| Area / region (copy B) | 0x483774 | 1 | 1 |
| Dimensions (width, height in tiles) | 0x3F8900 | 2 | 2 |
| Tileset graphics (LZ77) | 0x3F8A80 | 4 | 1 |
| Colour attributes | 0x3F8D80 | 4 | 1 |
| Tileset palettes (0x200 B) | 0x3F9980 | 4 | 1 |
| Collision (1 B per 16×16 metatile) | 0x404334 | 4 | 3 |
| Per-map block A | 0x409480 | 4 | 1 |
| Per-map block B (0x80 B each) | 0x40F880 | 4 | 1 |
| Warps (8 B each, 0xFF end) | 0x412E60 | 4 | 3 |
| Sprites used on the map (0xFF end) | 0x413160 | 4 | 1 |
| Music | 0x47E644 | 1 | 1 |
| People / triggers (8 B each, 0xFF end) | 0x483234 | 4 | 1 |

- No table is referenced outside those literals (checked across the whole ROM), and no RAM array is indexed
  by the map ID (the save has no per-map data).
- **Expansion** (`MapExpander.Expand`): the 16 tables move to free space with **255 entries**, their literals
  are repointed, and the marker `MBXMAPS\0` + maps in use (u16) + capacity (u16) sits 16 bytes before the
  layer table; `RomLayout` reads the map count from it (no marker = 192).
- **New map** (`MapExpander.AddMap`, green + in the Maps tab): copies the 16 entries of an existing map into
  the next ID; warps and people empty (just the terminator) or copied.
- Without warps a new map is unreachable: it needs a warp coming in (from another map) and one going out.

### 13.2 Tileset and palette
- The tileset is decompressed to VRAM at `0x6009000` (character block 2 + 0x1000); copying the tilemap to
  the screen adds 128 to each index. The 3 BGs use 64×64 screens at `0x6003000`, `0x6005000` and `0x6007000`.
- The map palette fills the 16 BG banks (0x200 B); the original maps use all 16.
- In the Toolkit: Maps tab → Layer Editor → export/import tileset (sheet of 16 tiles per row, each tile in the
  bank it is used with, colour 0 transparent) and palettes (16×16 grid). Staged as `MapTilesetPatch`, for
  every map using the tileset or only the current one.

### 13.3 Events
- The game (routine `0x0804D83C`) finds event N at `base + bank[N]×0x4000 + (pointer[N] − 0x4000)`; header
  at the base: `[0..1]` = pointer table offset (0x4000), `[2..3]` = bank table offset. The base lives in RAM
  at `0x3005590`, copied from the literals `0x4A584`, `0x4A8C8`, `0x4B124` (EU: `0x765EC0`).
- Scripts refer to each other only by ID and relative jumps → the Toolkit moves table + scripts together
  (`EventDatabaseRelocator`) when the table grows or free space is beyond the banks' reach.

### 13.4 Limits

| Item | Limit | Why | Largest use in the original game |
|---|---|---|---|
| Maps | **255** (IDs 0-254) | one-byte ID; 0xFF = "none" | 192 |
| Map size | **64 × 64 tiles** (512 × 512 px, 32 × 32 metatiles) | 64×64 BG screens; copied as 64×64 | 62 × 64 (map 67) |
| Tiles per tileset | **896** (28 KB decompressed) | VRAM from `0x6009000` to the sprites at `0x6010000`; index + 128 ≤ 1023 | 895 |
| BG palettes per map | **16 × 15 colours** (+ transparent colour 0) | the whole BG palette | 16 banks |
| Collision | 1 byte per metatile (up to 32 × 32 = 1024 B) | — | 31 × 32 |
| Warps per map | no fixed limit found (list in ROM) | read straight from ROM | 15 |
| People / triggers per map | no fixed limit found; 12-bit event ID | 8-byte record | 66 |
| Sprites used per map | **16** (the Toolkit reads up to 16) | 0xFF-terminated list | 12 |
| Event IDs | **4096** (12 bits in the person record) | spawn field | 2288 |
| Event scripts | **4 MB** from the base | 1-byte bank × 16 KB | ~110 KB |
| Medabots | **236** | Tinpet fixed at 236/237 | 120 |
| Portrait characters | **256** (9 expressions each) | 1-byte character in `<PORTRAIT>` | 176 |
| Dialogue line width | **208 px** before wrapping (212 visible) | routine `0x080402B4` | — |
| ROM | **32 MB** | GBA address space | 8 MB (expanded: 16 MB) |

Notes: 64×64 is the BG screen size; the RAM buffer the layers are decompressed into (pointer at
`0x3006D14`) has not been measured yet, so the largest **proven** size is 62×64 (3968 tiles). The "no fixed
limit found" items depend on how many objects the game allocates in RAM when loading a map; up to the
largest original use is safe.

## 14. Text, portraits and reverse engineering

### 14.1 Dialogue line wrap
- Routine `0x080402B4`, state at `0x03000440` (`+0x14` pen x as u8, `+0x15` remaining word width, `+0x19`
  wrap flag). Font `0x084BFC6C` (italic `0x084C59AC`), widths `0x083B4E08` / `0x083B5008` (2 B per glyph;
  the first is the advance).
- Before each word the game measures it up to and **including** the next space (stopping at a control
  code); if `x + width > 208` it wraps: line 1 → line 2, line 2 → a new box after the button.
- x and width are u8: a line over 256 px is drawn over its own start; a word over 255 px is measured modulo
  256 (and may not wrap). `<MEM:n>` (the player's name) is measured as one word.
- Confirmed with capture `cap_3` (RAM frame by frame) and the `teste_linha` ROM.

### 14.2 Portraits
- Tables `0x083AFE88` (character × 9 + expression, Malias2 pointers) and `0x083B1748` (one 32-byte palette
  per character). Read by `0x0803F41C`, `0x0803F480`, `0x0803F51C` (3 + 4 literals), **with no range check**.
- The expansion moves both to 256 characters, writes the `MBXPORT` marker (in use, capacity) before the image
  table and repoints the literals. Portrait edits are rebased by character number at Export, so editing
  before adding characters loses nothing.

### 14.3 Reverse engineering (Ghidra)
- Symbols are versioned in `re/symbols/mtbe.csv` in `jvsdm/Medabots-Romhacking-Toolkit`
  (branch `claude/medabots-project-ajyaeb`); Ghidra scripts in `re/ghidra/`; task list in `re/HANDOFF.md`.
- **Split of work**: the cloud session does the heavy static analysis (decompiling, naming, documenting in
  the CSV and HANDOFF); the local session does what needs this machine — Ghidra with GhidraMCP, mGBA and
  captures, in-game tests, the WPF editor — and pushes what it confirmed back to the repository.
- GhidraMCP 1.4 (LaurieWired) installed on Ghidra 12.1.4 (extension version adjusted; the release targets
  11.3.2); bridge at `tools/ghidra_mcp/bridge_mcp_ghidra.py`, server `http://127.0.0.1:8080/`.
- **mgba-mcp** (struktured-labs): each call opens mGBA with a Lua script, runs N frames, reads memory / OAM / takes a
  screenshot and closes. It needs `--script`, which only mGBA 0.11 has (portable nightly in `tools/mgba-nightly/mGBA/`;
  the installed 0.10.5 lacks it). Windows patch (no xvfb, `taskkill`, unfocused window) in
  `tools/mgba-mcp-windows.patch`. A minimized window gives empty screenshots.
- Simon's database (Rokusho E): ported to the CSV on 2026-10-10 (866 symbols, types in `re/types/`); see `re/HANDOFF.md`.

## 15. Skeleton, molds and battle animation

### 15.1 Mold tree (skeleton)
- Table `0x083D1D78`: 50 pointers to 0x18-byte records. `+0x00` art list (image/palette pairs), `+0x0F`/`+0x10`
  **two child molds** (walked recursively), `+0x11` palette, `+0x12`/`+0x13` width/height in px, `+0x15` sprite
  shape. `+0x04..+0x0E` look like attachment points.
- Tree: 1 → 2 → joints 3-6 (in RAM: head, arms, legs, linked to each part's first mold). Head 7-13, right arm
  14-16, left arm 17-19, legs 20-49 (six variations by leg type).
- Bot part entry (u32): bits 0-5 mold, 6-12 art (8 bits with the patch), bit 14 mirror, bit 15 variant.
- **Each piece is one hardware sprite: at most 64×64 px.** A bigger/more complex bot needs more pieces (molds).

### 15.2 Movement animation
- Script = `gMoveScriptTable[kind×7 + gMoveStyle[legs bot]]` (59 scripts). Per script: `0x083D2018` duration of
  each keyframe (bytes, 0-terminated; N keyframes), `0x083D1E40` s16 angle [mold][N] (1/65536 turn),
  `0x083D1F2C` offset (s8 x, s8 y) [mold][N]. The data has **49 rows (molds 1-49)**.
- The player (`0x08047428`) interpolates **63 molds** into the per-mold state (`0x020260D0`, 16 B per mold,
  0x410 B per side = 65 slots).

### 15.3 New molds (50-63) — solved (2026-10-10)
The old blocker was not the state array (65 slots per side) but **data**:
1. the mold table has 50 slots (the angle pointers follow it);
2. the **mirror map** `0x08081490` (50 B; swaps the arms for the right side): molds ≥50 became 0 and vanished on
   the right side;
3. the **movement scripts** have 49 rows: molds ≥50 read the next block and pieces "flew".
`tools/mold_lab.py` extends all three (64 / 64 / 63 rows) and makes bot 0's head use clones 50/51: in the debug
menu's Motion viewer the bot is identical to the original on both sides.

### 15.4 Navi bots with Navi geometry (2026-10-10)
- Navi uses the same mold system; shapes Core lacks are one-piece 64×64 arms (15/18, 33/34), 64×64 head (30),
  64×32+32×16 head (11/12) and one-piece (24) or long (31/32, 35-37) legs.
- **Navi rows: head and arms 16 B, legs 32 B** (as in Core). Reading 64 B pulled in the next bot's legs: the
  "extra pieces" were that, not Medachange. `art/navi/` was extracted again.
- `NaviImporter` (Toolkit, `src/Medabots.Rom/Expansion/NaviImporter.cs`) adds Navi bots to the expanded ROM. Pieces
  with a same-shape Core mold use it (heads 7-10, two-piece arms 13/14→14/15 and 16/17→17/18, biped legs 19-23→31-35,
  chain 25-29); the rest get new molds (50+) with the closest Core mold's record and Navi size/attachment. Stats, mini
  sprite and movement style come from a Core bot with the same leg geometry; the in-game leg type is Navi's.
- Test batch (21 bots, IDs 182-202) in `build/Medabots_expanded+navi+debug.gba` + `.sav` with every part. Checked in
  the Motion viewer against Medapedia: 19 match. Open: Hophop (legs without their own palette) and 64×64 arms with
  multi-piece legs on the right side (Lady Outer, Giraffe Chief: loose arm).
- Navi's Medachange forms have not been found yet (they are not in the bot rows).

## Credits and references

- [Medabots-Romhacking-Toolkit](https://github.com/STulling/Medabots-Romhacking-Toolkit), [Medabots-Randomizer](https://github.com/STulling/Medabots-Randomizer), [MedabotsLib](https://github.com/STulling/MedabotsLib) — STulling
- [mdr-navi-i18n](https://github.com/caribbeansc/mdr-navi-i18n) — Medarot Navi charset and Malias codec (MIT)
- [Medapedia](https://medarot.meowcorp.us/) — lists of Medarots, medals and actions
- Malias2 — Normmatt
