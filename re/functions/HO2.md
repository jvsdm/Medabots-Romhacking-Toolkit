# HO2 — handlers do ObjectScript (roteiro de objeto da batalha), opcodes 0x1E–0x3A

Despachante `ObjectScript_OpcodeDispatch` 0x080393EC. Tabela de handlers 0x083AF8C0 (literal em 0x0803942C), indexada pelo opcode (×4), chamada via `CallViaR0`.

Fluxo (lido no decompile do despachante e de `FightPreview_AdvanceSequenceCursor`):
- `ObjectScript_ReadWord` 0x0803D424 lê um halfword do roteiro. O byte baixo vira o opcode, guardado em ctx+0x29 (`DAT_08039428`).
- Se o bit 7 do byte estiver ligado (opcode >= 0x80), o despachante avança o cursor (0x0803D4AC) e relê. Só opcodes 0x00–0x7F chegam à tabela.
- Argumentos: `FightPreview_UnpackSequenceEntry` 0x0803D450 lê uma contagem de 1 byte da entrada atual e copia shorts para o buffer ctx+0x102 em diante. Os campos usados pelos handlers são halfwords em +0x102, +0x104, +0x106, +0x108, +0x10A, +0x10C, +0x10E, +0x110. Cada handler que chama o unpack lê estes campos.
- Quase todo handler termina com `FightPreview_AdvanceSequenceCursor` (avança 1 + tamanho do registro).
- Ctx do roteiro: `DAT_0803942C`/`DAT_08039428` (despachante); bit 1 de ctx+0xF8 escolhe o lado (esquerdo/direito, espelhado) em vários handlers.
- Flag ctx+0x2A: é lida pelo laço `FUN_080393BC` (até a flag virar não-zero); o opcode 0x37 a liga.

## Fim real da tabela

- 0x00: `0x08000841` (aponta para 0x08000840, código do HO1, não é handler deste roteiro).
- 0x01–0x3A: handlers válidos (ponteiros thumb ímpares).
- 0x3B–0x4F: `0x08000841` (sentinela repetida). Fora da tabela, não usar.
- 0x50 em diante: palavras que não são ponteiros (0x696B6A6A, 0x006A6C6C…) e depois ponteiros para outra área (0x083AFA00, 0x084D3024…). Dado, não tabela de handlers.
- Portanto o tamanho real é 0x3B (59 entradas), não 80. Os opcodes 0x3B+ estão "fora da tabela".

## 0x0803A474 (op 0x1E)
- Não é código no Ghidra (sem função em 0x0803A474). Provável código não analisado ou dado. Não documentado.

## 0x0803A5CC (op 0x1F)
- Não é código no Ghidra (sem função). Não documentado.

## 0x0803A6FC ObjScript_Op20_ClearTilesReset (op 0x20)
- Assinatura: `undefined4 f(void)`
- O que faz: chama `FUN_0803efd8` duas vezes (coords 0x20/0x20), escreve halfword 0x0500 em `*DAT_0803a754`, zera dois bytes (`DAT_0803a758`, `DAT_0803a75c`), limpa slots de tile (`Tile_ClearSlots(4,1)`, `Tile_ClearSlots(0x23,1)`) e avança o cursor. Sem argumento de registro.
- Chama: FUN_0803efd8, Tile_ClearSlots, FightPreview_AdvanceSequenceCursor. Chamada por: 0x080393EC via tabela.
- Memória: ponteiros em DAT_0803a754/58/5C (endereços não resolvidos).
- Para o Lua: portar (limpeza de tiles/reset de estado visual).
- Confiança: confirmado (código); nome provável.

## 0x0803A760 (op 0x21)
- Não é código no Ghidra (sem função). Não documentado.

## 0x0803A840 (op 0x22)
- Não é código no Ghidra (sem função). Não documentado.

## 0x0803A920 ObjScript_Op23_SetFlagAdvance (op 0x23)
- Assinatura: `undefined4 f(void)`
- O que faz: escreve 1 no byte `DAT_0803a934 + DAT_0803a938` e avança o cursor. Sem argumento de registro.
- Chama: FightPreview_AdvanceSequenceCursor. Chamada por: 0x080393EC.
- Memória: byte de flag em endereço calculado (ponteiros não resolvidos).
- Para o Lua: portar (marca um estado).
- Confiança: confirmado (código); nome provável (o que a flag marca é desconhecido).

## 0x0803A93C (op 0x24)
- Não é código no Ghidra (sem função). Não documentado.

## 0x0803AA00 (op 0x25)
- Não é código no Ghidra (sem função). Não documentado.

## 0x0803AA90 (op 0x26)
- Não é código no Ghidra (sem função). Não documentado.

## 0x0803AB00 (op 0x27)
- Não é código no Ghidra (sem função). Não documentado.

## 0x0803AB9C ObjScript_Op28_CopyFourBlocks (op 0x28)
- Assinatura: `undefined4 f(void)`
- O que faz: se ctx+0xF8 bit 1 for 0, faz 4 cópias `bios_cpufastset(src, dst, 8)` (set A); senão, outras 4 (set B, ponteiros diferentes). Depois avança o cursor.
- Chama: bios_cpufastset (SWI 0x0C), FightPreview_AdvanceSequenceCursor. Chamada por: 0x080393EC.
- Memória: ctx em DAT_0803abd8; origem/destino são ponteiros não resolvidos (provável VRAM/paleta ou cópia de tabela de sprite).
- Para o Lua: portar como cópia de blocos (ou ignorar se o visual não usa).
- Confiança: confirmado (código); destino não identificado.

## 0x0803AC4C (op 0x2A)
- Não é código no Ghidra (sem função). Não documentado.

## 0x0803ACB4 (op 0x29)
- Não é código no Ghidra (sem função). Não documentado.

## 0x0803ADE8 (op 0x2B)
- Não é código no Ghidra (sem função). Não documentado.

## 0x0803ADFC (op 0x2C)
- Não é código no Ghidra (sem função). Não documentado.

## 0x0803AF2C ObjScript_Op2D_SetupFourSlots (op 0x2D)
- Assinatura: `undefined4 f(void)`
- O que faz: desempacota a entrada; grava campos do registro em um bloco de estado (byte de +0x102, halfword de +0x106, bytes de +0x10A/+0x10C, byte 0x30); se bit 1 de ctx+0xF8 for 0, copia +0x104 e +0x108 direto; senão espelha (0xF0 - x, 0x20 - y & 0x1F). Depois, 4 vezes: monta um registro de sprite (`halfword = (+0x10E << 10) | k`), chama `Slot_SetupFromRecordList` com tipo vindo de +0x110 e calcula deslocamento com `CompositeSprite_ComputeOffsetWord`. Avança o cursor.
- Argumentos (shorts do registro): +0x102 (índice), +0x104 (x), +0x106, +0x108 (y), +0x10A, +0x10C, +0x10E (paleta/flags), +0x110 (tipo).
- Chama: FightPreview_UnpackSequenceEntry, Slot_SetupFromRecordList, CompositeSprite_ComputeOffsetWord, FightPreview_AdvanceSequenceCursor. Chamada por: 0x080393EC.
- Memória: base DAT_0803afac (bloco do bot); tabelas de tipo em DAT_0803b0b8.
- Para o Lua: portar (cria 4 sprites de uma vez).
- Confiança: confirmado (código); nome provável (4 slots).

## 0x0803B0C0 ObjScript_Op2E_SetupFourSprites (op 0x2E)
- Assinatura: `undefined4 f(void)`
- O que faz: desempacota a entrada; monta 4 entradas de sprite a partir de +0x102..+0x108 e de pares de halfwords (`DAT_0803b164`); tipo de slot vem de +0x108 (tabela `DAT_0803b170`); escreve x e y por `(psVar4[k])` ou espelhado conforme bit 1 de +0xF8. Avança.
- Argumentos (shorts): +0x102, +0x104, +0x106, +0x108 (índice do tipo), +0x10A/+0x10C via psVar4 (pares de coords por sprite).
- Chama: FightPreview_UnpackSequenceEntry, Slot_SetupFromRecordList, FightPreview_AdvanceSequenceCursor. Chamada por: 0x080393EC.
- Memória: DAT_0803b160 (base), DAT_0803b168 (lista de slots, stride 0x40), DAT_0803b170 (tabela de tipos).
- Para o Lua: portar junto com 0x2D (mesmo padrão).
- Confiança: confirmado (código); nome provável.

## 0x0803B19C (op 0x2F)
- Não é código no Ghidra (sem função). Não documentado.

## 0x0803B1EC ObjScript_Op30_SetupFreeSlot (op 0x30)
- Assinatura: `undefined4 f(void)`
- O que faz: desempacota a entrada; se o halfword de ctx+0x280 for 0, usa o slot 0x0A; senão procura o primeiro slot de 0x0B a 0x13 cujo halfword de +0 seja 0. Limpa o slot (`Tile_ClearSlots`), monta o registro (x/y de +0x104/+0x108 ou espelhados), tipo de +0x110 via `Slot_SetupFromRecordList`. Avança.
- Argumentos (shorts): +0x102, +0x104 (x), +0x106, +0x108 (y), +0x10A, +0x10C, +0x110 (tipo).
- Chama: FightPreview_UnpackSequenceEntry, Tile_ClearSlots, Slot_SetupFromRecordList, FightPreview_AdvanceSequenceCursor. Chamada por: 0x080393EC.
- Memória: DAT_0803b20c (ctx), DAT_0803b2d8 (slots, stride 0x40), DAT_0803b2e4 (tabela de tipos).
- Para o Lua: portar (cria 1 objeto em slot livre).
- Confiança: confirmado (código); nome provável.

## 0x0803B328 (op 0x31)
- Não é código no Ghidra (sem função). Não documentado.

## 0x0803B374 ObjScript_Op32_LoadGfxList (op 0x32)
- Assinatura: `undefined4 f(void)`
- O que faz: usa o bot atual (base `(byte +0x87) * 0x100 + DAT_0803b3a8`). Se o byte +0xDD for < 4, chama `Fight_LoadGfxListToObjVram` com índice de +0xA7*0x18 + 0x49; senão com `+0xA8 + 0x50`. Avança o cursor. Sem argumento de registro.
- Chama: Fight_LoadGfxListToObjVram, FightPreview_AdvanceSequenceCursor. Chamada por: 0x080393EC.
- Memória: DAT_0803b3ac (ponteiro de bots), DAT_0803b3a8 (base da lista de bots).
- Para o Lua: portar (carregamento de gráficos para OAM/VRAM).
- Confiança: confirmado (código); nome provável.

## 0x0803B3C8 (op 0x33)
- Não é código no Ghidra (sem função). Não documentado.

## 0x0803B428 (op 0x34)
- Não é código no Ghidra (sem função). Não documentado.

## 0x0803B4F8 ObjScript_Op35_SetupEightSprites (op 0x35)
- Assinatura: `undefined4 f(void)`
- O que faz: desempacota a entrada; 8 vezes monta um sprite: halfword de `DAT_0803b5d0`, coords de `CompositeSprite_ComputeOffsetWord` a partir de (+0x108 lista de x, entrada de +0x102 em stride 0x40), tipo via `DAT_0803b5d4` e `Slot_SetupFromRecordList` (slot 0x0B+i). Avança.
- Argumentos (shorts): +0x102 (índice da entrada de referência), +0x104 (valor de slot), +0x108 (lista de 8 coords em ushort).
- Chama: FightPreview_UnpackSequenceEntry, CompositeSprite_ComputeOffsetWord, Slot_SetupFromRecordList, FightPreview_AdvanceSequenceCursor. Chamada por: 0x080393EC.
- Memória: DAT_0803b5c8 (ctx), DAT_0803b5cc (slots, stride 0x40), DAT_0803b5d8 (tabela de tipos).
- Para o Lua: portar (8 sprites, usado em animações de objetos).
- Confiança: confirmado (código); nome provável.

## 0x0803B5DC (op 0x36)
- Não é código no Ghidra (sem função). Não documentado.

## 0x0803B6D8 ObjScript_Op37_WaitThenSetFlag (op 0x37)
- Assinatura: `undefined4 f(void)`
- O que faz: se o byte em `DAT_0803b6f8 + DAT_0803b6fc` for 0, avança o cursor; depois liga a flag ctx+0x2A = 1 (sempre). A flag é a que `FUN_080393BC` espera. Ou seja, o opcode 0x37 marca o fim de um passo do roteiro.
- Chama: FightPreview_AdvanceSequenceCursor. Chamada por: 0x080393EC.
- Memória: ctx em DAT_0803b6f8 (+0x2A flag), byte de condição em DAT_0803b6fc.
- Para o Lua: portar (controle do laço de passo, ver FUN_080393BC).
- Confiança: confirmado (código); papel de "espera" é provável, mas a lógica liga a flag incondicionalmente.

## 0x0803B700 ObjScript_Op38_ClearTilesFx (op 0x38)
- Assinatura: `undefined4 f(void)`
- O que faz: chama `FUN_0803ef8c` duas vezes (coords 0x20/0x20, ponteiros de DAT_0803b74c/DAT_0803b750), limpa slots de tile (4 e 0x23), avança. Parecido com 0x20, outra variante de efeito.
- Chama: FUN_0803ef8c, Tile_ClearSlots, FightPreview_AdvanceSequenceCursor. Chamada por: 0x080393EC.
- Para o Lua: portar junto com 0x20.
- Confiança: confirmado (código); nome provável.

## 0x0803B754 (op 0x39)
- Não é código no Ghidra (sem função). Não documentado.

## 0x0803B884 (op 0x3A)
- Não é código no Ghidra (sem função). Não documentado.

## Em aberto

- 19 dos 29 endereços (0x1E, 0x1F, 0x21, 0x22, 0x24–0x27, 0x29, 0x2A, 0x2B, 0x2C, 0x2F, 0x31, 0x33, 0x34, 0x36, 0x39, 0x3A) não têm função no Ghidra, mas são ponteiros thumb válidos na tabela. Provável código não analisado. Falta criar função ou decodificar a mão.
- Formato exato de "ctx" (`DAT_08039428`, `DAT_0803b6f8`, etc.) e os endereços de DAT_* não estão resolvidos aqui.
- Sentido de cada campo de argumento (x/y, paleta, tipo) é inferido pela forma do código, não confirmado no jogo.
