# CS2: sprites de personagem do mapa (continuação de CS.md) - GH-CS2

Status: PARCIAL (2026-10-14, retomada). Estático: ROM original só leitura + Ghidra (sem salvar). Sem emulador.
Símbolos: `CS2_simbolos.csv`.

## 1. Layout do bloco de sprite e tile OBJ (confirmado no código; tile = provável)
- Bloco LZ77 descomprimido por `bios_lz77_vram` para VRAM OBJ 0x06010000 + i*0x900, i = posição na lista do mapa.
- 0x900 = 2304 B = 72 tiles de 32 B (4bpp 8x8). Tile inicial do bloco i = i*72 (provável: OBJ em modo 1D, mapeamento
  linear). Índice de tile de um molde = i*72 + tile_do_molde (provável; o campo de tile dos moldes não foi lido).
- Paletas: ver §4 (n-ésima entrada da lista → paleta EWRAM n).
- Tamanho do molde 16x32 continua NÃO confirmado.

## 2. Id de sprite -> grupo, facing, frame (confirmado)
- Grupo = sprite>>4 (`Map_InitializeObjectRenderSlot` grava +0x0A = grupo). Facing = sprite&3 (0 N, 1 S, 2 W, 3 E).
- `Map_SetObjectRenderSlotFacingVariant` 0x08052D90 (decompile): grava +0x20 = +0x21 = facing e
  **+0x06 (frame) = grupo*12 + facing*3** (facing 0 → +0, 1 → +3, 2 → +6, 3 → +9). Inline do `Obj_FrameBaseFromSprite` 0x080502C8.
- `Map_SetEventMarkerFacingVariant` 0x0805218C chama a função acima e grava +0x05 do registro de evento = facing.

## 3. Origem de +0x2F e de IWRAM 0x03006D3E
- +0x2F (slot): lido por `Map_TickObjectAnimation` 0x08049440 no caso 0xFFFA: `+0x06 = a3 + byte(+0x2F)`.
  Ou seja, +0x2F é um deslocamento de quadro somado ao a3 do roteiro. **Escritor NÃO achado** (xref de campo não
  resolve no Ghidra; o slot é acessado por base+k*0x40). Pendente.
- IWRAM 0x03006D3E = byte escrito por `PartSet_GetLegIconIndexForMatchingSlotPartId` 0x08047E50 (CONFIRMADO no
  decompile: literal 0x08047EE0 = 0x03006D38, escrita em +6 = 0x03006D3E). Tabela de busca = IWRAM 0x030017A0
  (literal 0x08047E88), 9 entradas de 8 B; grava o byte +1 da entrada que casa com (id da parte, kind 0..3).
  **Chamador único: `Event_Op4C_Jump_If_Missing_Part` 0x0804CAA6** (opcode de evento 0x4C, verificação de peça).
- Zerado por `Event_Op06_End` 0x0804B7FC (fim de evento, literal 0x0804B840 = 0x03006D38, +6 = 0): confirmado.
- Leitor: `Map_PoseIdToTileIndex` 0x08048210 (ids 0xC0..0xFE: `(id-0xC0)*2 + byte[0x03006D3E] + 0x28`). Semântica
  "índice de sprite da perna equipada" = provável (nome Ghidra; faz sentido com o 0x4C).

## 4. Paletas dos personagens (0x08048708) - CONFIRMADO
- Loop i = 0..: `id = Map_GetSpriteListEntry(i)` (0x08048684) até 0xFF. `src = ROM[0x083F83C0 + 4*id]`; se 0, usa id 1.
- DMA3 (0x040000D4) copia 0x20 B (16 cores 15-bit = 1 paleta 4bpp) de `src` para EWRAM 0x0202F800 + 0x20*n (n = contador).
- Fim: DMA fixo de ROM 0x085FB684 (32 B) → 0x0202F9C0 (slot 14). Total 14 paletas de objeto + 1 fixa.
- Tabela 0x083F83C0: 61 entradas válidas (mesmo tamanho de 0x083F7E80).
- Provável: buffer 0x0202F800 vai para OBJ palette RAM 0x05000200 no VBlank (não rastreado). Banco da paleta (attr2
  bits 12-15) = n: provável, não confirmado.

## 5. Roteiro de animação (confirmado: decompile 0x08049440 + dump da ROM)
- Slot: +0x24 = anim (escrito por `Map_SetObjectAnimationVariant` 0x08049420, que zera +0x22 e +0x23).
  Anim = facing + 4*grupo (`Overworld_GetObjectAnimationIndexForFacing` 0x08050300 / `Map_SpawnTrackedObjectAtTile`).
- Ponteiro: `ROM[0x083F7B80 + 4*anim]` = endereço do roteiro. Tabela com **128 entradas válidas** (0x083F7B80..0x083F7D7C,
  0x200 B; ponteiros entre 0x083F704C e 0x083F7B7C). Entrada 128 em diante = lixo (começa 0x83F7B7A/7C, depois dados
  não-ponteiro): a tabela é 0x200 B. Anim 64+ não tem dono confirmado (grupo do sprite é só 4 bits).
- Passo = 8 B: `u16 op, u16 a1, u16 a2, u16 a3`. Índice = slot+0x22 (passo), contador de ticks = slot+0x23.
  Seleção: `op + 0xFFFF0006` → caso 0..5 (FFFA..FFFF); outros = default.
  - **0xFFFA**: `frame = a3 + byte(+0x2F)`, a2 ticks, avança.
  - **0xFFFB** (default, o mais comum): `frame = a3`, a2 ticks, avança.
  - **0xFFFC**: se contador 0 e registro LZ77 `[0x080AEB74 + 0x10*a1]` ainda não carregado: `bios_lz77_vram(src, dst)`; depois default.
  - **0xFFFD**: avança o passo, toca SFX a1 (`Snd_SfxRequestWrapper`), e repete o passo seguinte no mesmo tick.
  - **0xFFFE**: slot+0x22 = a1 (índice de PASSO, não bytes), contador = 0, repete. Dá o laço.
  - **0xFFFF**: fim (não faz nada).
- Exemplos (dump): anim 0 = passos FFFB com frames 1,0,2,0 (8 ticks cada), depois FFFE a1=0. Anim 1 = 4,3,5,3;
  anim 2 = 7,6,8,6; anim 3 = 10,9,11,9; anim 4 = 13,12,14,12. Padrão: frame base = 3*anim (= 12*grupo + 3*facing),
  parado = base+0 (meio) e andando = base+1 / base+2. Roteiros têm 0x24 B (4 passos + FFFE de 4 B na prática, pois o
  FFFE nunca avança); por isso o ponteiro do próximo anim cai 0x24 depois.
- Ops FFFA/FFFC/FFFD aparecem em alguns roteiros (ex.: FFFA no roteiro 69, FFFC no 48 com a1=0 a2=8, FFFD no 59 a1=8).
  Confirmado no código; a contagem por roteiro não foi refeita (varredura inicial mal alinhada, descartada).

## 6. Spawn: como o +4 do registro vira sprite (confirmado)
- `Map_LoadEntitySpawnsForCurrentMap` (0x0805160A é um call site dentro dela; endereço de entrada não anotado). Registro de 8 B por spawn:
  +0 x tile, +1 y tile, +2 u16 id de evento (teste `EventFlag_IsSetByPackedId`), +4 **byte = (grupo<<4) | facing**,
  +5 índice de condição, +6 u16 máscara (bit de teste `>> DAT_08051484`).
- Byte +4 >> 4 = grupo: `Map_InitializeObjectRenderSlot(slot, 0xC804, frame = grupo*12, grupo)` (slot = 0x31 + k, k = nº de spawn, até 0x1E).
  Sombra (se `Map_GetObjectPoseByGroup` tem bit): slot 0x5B + k, valor 0xCC0C.
- `Map_GetObjectPoseByGroup(grupo)` → +0x30 (pose). `Map_ResetObjectField0B` (ver §7).
- Facing = byte +4 & 0xF → `Map_SetObjectRenderSlotFacingVariant` (frame = grupo*12 + facing*3, §2).
- Anim = (grupo<<2) + (byte+4 & 0xF) → `Map_SetObjectAnimationVariant` (+0x24). Ou seja, a anim usa o NIBBLE
  alto como grupo e o baixo como facing (`&0xF` no spawn; no id de sprite do mapa o facing é `&3`, mesma
  leitura para valores 0..3).
- Posição: X = (x*16 + base)*0x100 em +0x0C, Y em +0x10 (8.8). Grupo gravado em +0x2B (= byte+4 >> 4).

## 7. Quem grava +0x0B (confirmado no decompile)
- `Map_ResetObjectField0B` 0x080487C8: `+0x0B = tab[pose]` com `tab = byte em 0x083D2C5D + pose (u16 em +0x30)`:
  byte 2 → 0xFF, byte 1 → 0x00, outro → 0x40.
- Chamadores: spawn (`Map_LoadEntitySpawnsForCurrentMap`, 0x0805160A), `Map_InitSecondaryMarkerOverlay` 0x08052FFC,
  `Map_InitObjectSprite` 0x080504D6, `Event_Op83_Set_Object_Render_Mode` (0x0804D61C/D680/D6B6),
  `EventOpcode0E_PlayerHopInPlace` (0x0804DF06/DF16), `EventOpcode0F_PlayerHopInDirection` (0x0804E0EA),
  `Map_ObjectArcStep` (0x080509BA/C2: 0xFF enquanto o salto não pousou), `Map_ObjectMoveStateStep` (0x08050B6E/76).
- Leitor: `BuildShadowOamFromObjectState` 0x08001520 copia +0x0B para a lista ordenada por `SortActiveObjectIndicesByPriority`.
  O uso como chave de ordenação não foi lido dentro do sort (provável).
- Obs.: 0x083D2C5D lido: os 32 primeiros bytes são zero; a tabela real deve ter valores só para poses altas (não lida).

## 8. Comparação com pc/extract
- pc/extract não usa 0x083F7E80, 0x083F7B80, 0x08413160, 0x083F83C0 (ver CS.md §6). Sem mudança.

## Pendente
- Escritor de +0x2F do slot (o deslocamento do 0xFFFA).
- Campo de tile dos moldes e tabela de moldes: confirmar 1D (DISPCNT bit 6) e o 16x32.
- Banco de paleta (attr2) = n: confirmar.
- Semântica de anim 64..127 (entradas extras da tabela 0x083F7B80).
- VBlank: buffer 0x0202F800 → OBJ palette RAM (não rastreado).
- Byte IWRAM 0x030017A0 (tabela da perna): quem preenche (não é ROM; provável cópia no load).
