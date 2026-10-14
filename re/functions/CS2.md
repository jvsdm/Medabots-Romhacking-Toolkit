# CS2: sprites de personagem do mapa (continuação de CS.md) - GH-CS2

Status: em andamento (2026-10-14). Estático: ROM original só leitura + Ghidra. Sem emulador.

## 3. Paletas dos personagens (item 3) - CONFIRMADO no disasm/literal pool 0x08048708
- `Map_LoadCurrentEventObjectPalettes` 0x08048708: loop `i = 0..`; `id = Map_GetSpriteListEntry(i)` (0x08048684) até 0xFF
  (mesma lista do mapa usada em 0x08048288). Para cada id: `src = ROM[0x083F83C0 + 4*id]`; se 0, usa id 1.
  DMA3 (regs 0x040000D4) copia 0x20 B (16 cores 15-bit = 1 paleta 4bpp) de `src` para EWRAM 0x0202F800 + 0x20*n, n = contador do loop (1 paleta por entrada da lista).
  Controle DMA 0x84000008 = 8 words (32 B), 32-bit.
- No fim: DMA fixo de ROM 0x085FB684 (32 B) para 0x0202F9C0 (= slot 14, 0x0202F800 + 14*0x20). Buffer: 14 paletas de objeto + 1 fixa.
- Tabela de paletas `0x083F83C0`: 61 entradas válidas (ptr 0x085D6FF8, 0x085FB33C, 0x085D7434, ...), entrada 61 = 0. Mesmo tamanho da tabela de blocos 0x083F7E80 (61).
- Obs.: o índice da paleta é o id da lista (não o grupo +0x2B). Confirmado no decompile (`ldr r2,[0x08048768]` = 0x083F83C0; `lsl r0,r1,#2`).
- Provável: o buffer EWRAM 0x0202F800 vai para OBJ palette RAM 0x05000200 no VBlank (não rastreado ainda).
- Provável: o banco de paleta do OBJ (bits 12-15 do attr2) = n do buffer; não confirmado.

## Literal pool 0x08048708
- 0x08048714 = 0x040000D4 (DMA3), 0x08048718 = 0x0202F800, 0x08048768 = 0x083F83C0, 0x0804876C = 0x84000008,
  0x08048770 = 0x040000D4, 0x08048774 = 0x085FB684 (src fixa), 0x08048778 = 0x0202F9C0.
