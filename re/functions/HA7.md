# HA7: handlers dos despachantes Fight_DispatchOnSubstateAA_AE/AF/AG/AH

Lista: `HA7_lista.txt` (42 entradas). Cada entrada é um ponteiro de uma tabela de substados (+0xAA) do stub
despachante. Tabelas (ponteiro lido do literal do stub, ROM original): AE `0x083ADFE0`, AF `0x083AE064`,
AG `0x083AE0C4`, AH `0x083AE124`; entrada = `tabela + 4*índice`. Índice = valor de +0xAA. Bit Thumb removido.
Correção ao R1.md: "tabela 0x080240B4/0x080249BC/0x08024E28/0x080251B0" é o SLOT DO LITERAL (ponteiro para a
tabela), não a tabela.

Struct de combate `0x03001800` (+0x88 = índice do bot, +0xAA = substado, +0xB5 zerado pelo stub, +0xD0 = parte,
+0xC8 = dano/valor). Bloco do bot = `0x03000590 + bot*0x100` (literal em 0x0802477C; +0x59 = parte quebrada).
`0x03002C30` = gObjectSlots (+0x840 flags, +0x86 contador). Literal `0x03000440` (AG/AE) não identificado.
Alguns ponteiros de tabela são compartilhados com outros despachantes (ex.: AF 0x0C aparece também em AI).

## Status
- Redo (2026-10-13): as 23 seções "sem função" foram lidas no Ghidra (agora são funções no Ghidra) e renomeadas.
  Nenhum capstone usado. Todas as 42 entradas de HA7_lista.txt agora têm seção e nome.

## Máquina de estados (cadeias)
- AE: 0A -> 0B -> 0C -> 0D -> 0E -> 0F -> 10 -> 11. Em 11: se a parte não está quebrada (bloco+0x59 != 0) -> 13;
  se quebrada -> 1A (com `Fight_QueueHeadBrokenBotForResolution`). Depois 13 -> 15 -> 16 -> 1A -> 1B -> 1C -> 1D -> 1E (sem entrada AE).
- AF: 00 -> 08 -> 0B -> 0C -> 0E -> 0F -> 10 -> 11 -> 12 (-> 13 compartilhado).
- AG: 00 -> 08 -> 0B -> 0C -> 0D -> 0F -> 10.
- AH: 00 -> 08 -> 09 -> 0A -> 0B -> 0C -> 0D -> 0E -> 0F -> 10 -> 11 (-> 12 compartilhado).
Transições "->" sem handler confirmado são inferidas pelo +1 dos handlers vizinhos.

## Handlers AE
### 0x080241F0 AE 0x0A: Fight_DispatchOnSubstateAA_AE_StepSlotToTarget
- `Fight_StepSlotTowardTarget(0x21)`; quando retorna 1 (chegou), fight+0xAA += 1. Próximo: 0x0B.
- Confiança: confirmado (decompile lido; o nome é provável).
### 0x08024210 AE 0x0B: Fight_DispatchOnSubstateAA_AE_ResetBotGfx
- Copia 4x 0x40 bytes do bot (0x880..0x940) via `FUN_0803f63c`; zera bytes do bot; `Slot_ClearAttrMask(0x22,4)`; mostra slot; sfx 0x34. Próximo: 0x0C.
### 0x080242F8 AE 0x0C: Fight_DispatchOnSubstateAA_AE_StepSlotAnim
- Move cursor ±0x10 conforme +0xF8 e `FUN_08036ac4(0x21,0)`; anima 4 slots (índices bot*4+0x5A..0x5D). Ao terminar: esconde slot, sfx 0x41. Próximo: 0x0D.
### 0x08024488 AE 0x0D: Fight_DispatchOnSubstateAA_AE_SetupFourSlots
- Preenche 4 entradas de 0x20 halfwords em 0x980 (posições do bot). Zera +0x84. Próximo: 0x0E.
### 0x08024538 AE 0x0E: Fight_DispatchOnSubstateAA_AE_WaitCopyBotGfx
- Cada quadro: `Anim_AdvanceSlotTileOffset` nos slots 0x26..0x29. Contador byte em `0x03000440+0x84` até 0x10; então
  `Tile_ClearSlots(0x26,4)`, copia 4 blocos de 0x40 bytes (`Mem_CopyBytes(src,dst,n)` = FUN_0803F63C, ordem src,dst) entre
  `0x030034B0` e `0x030034B0+bot*0x100+0xE00..0xEC0`; liga o bit 0x8000 em 4 halfwords de gObjectSlots (0x03002C30) no
  bot (+0x1680/+0x16C0/+0x1700/+0x1740); `Tile_ClearSlots(0x21,4)`; fight+0xAA += 1. Senão, incrementa o contador. Próximo: 0x0F (inferido, ver nota).
- Nota: o ramo final do decompile incrementa +0xAA; o 0x0F vem depois. Sentido exato da cópia = provável.
- Confiança: provável (estrutura lida; endereços dos literais confirmados na ROM; nomes dos slots 0x26/0x21 provável).
### 0x0802463C AE 0x0F: Fight_DispatchOnSubstateAA_AE_ApplyDamageShowNumbers
- bot = fight+0x88. Se `0x03000590+bot*0x100+0xBD` & 4 == 0: `Fight_ApplyDamageToPart(bot,0,fight+0xEC)`; senão `Fight_ApplyDamageToPart(bot,0,1)`.
  Depois `Fight_WriteNumberTiles(0x0300052E, fight+0xC8, 1)`, `Fight_BuildBotOamEntriesAt(0x0300052E, bot, 0x21)`, sfx 0x5B,
  zera byte `0x030004C4`, fight+0xAA += 1. Próximo: 0x10.
- Confiança: provável (decompile lido; literal 0x0300052E no ROM confirmado; semântica do bit 0x04 = provável).
### 0x080246C0 AE 0x10: Fight_DispatchOnSubstateAA_AE_WaitRowAnim
- `Fight_StepRowAnimation(bot,0x21)`; conta em +0x...-0x6A até >0x32 ou 'x'; ao 'x' limpa slots e avança. Próximo: 0x11.
### 0x0802473C AE 0x11: Fight_DispatchOnSubstateAA_AE_ApplyPartDamage
- `Fight_SubtractPartArmorClampZero(bot,0,+0xC8)`. Parte quebrada (bloco+0x59==0): enfileira bot e vai para 0x1A; senão 0x13.
### 0x08024790 AE 0x13: Fight_DispatchOnSubstateAA_AE_BranchOnBotFlag04
- Se `0x03000590+bot*0x100+0xBD` & 4 == 0: fight+0xAA = 0x15. Senão: fight+0xAA += 1 (vai para 0x14, não lido).
  Correção: o "próximo 0x15 (inferido)" vale só para o ramo "flag clear"; o ramo "flag set" leva a 0x14.
- Confiança: confirmado (decompile lido; o nome é provável).
### 0x080247C8 AE 0x15: Fight_DispatchOnSubstateAA_AE_SetBotFlag01
- bot block+0xBD |= 1; fight+0xAA += 1. Próximo: 0x16.
- Confiança: confirmado (decompile lido; o nome é provável).
### 0x080247F4 AE 0x16: Fight_DispatchOnSubstateAA_AE_SetStep2E
- fight+0xA6 = 0x2E (código de passo/mensagem); fight+0xAA += 1. Próximo: 0x1A (inferido).
- Confiança: confirmado (decompile lido; o nome é provável).
### 0x0802480C AE 0x1A: Fight_DispatchOnSubstateAA_AE_LoadBrokenGfx
- `decompress_and_store` + `bios_cpufastset` (8 words); `FUN_08036ff8(0x21,x,y,4)` com posição do registro de +0xD0. Próximo: 0x1B.
### 0x08024870 AE 0x1B: Fight_DispatchOnSubstateAA_AE_WaitSlotAnim
- `FUN_080370AC(0x21,4)`; quando retorna != 0 (animação dos slots terminou), fight+0xAA += 1. Próximo: 0x1C.
- Confiança: confirmado (decompile lido; o nome é provável; semântica de FUN_080370AC não lida).
### 0x08024890 AE 0x1C: Fight_DispatchOnSubstateAA_AE_LoadSlotGfx
- `decompress_and_store` + `bios_cpufastset`; seta gObjectSlots+0x840 e posição do slot; `Slot_SetupFromRecordList(bot,0x21)`. Próximo: 0x1D.
### 0x0802494C AE 0x1D: Fight_DispatchOnSubstateAA_AE_RunSlotScript
- `Anim_RunSlotScript(bot,0x21)`; quando o script termina (byte == -1): `Tile_ClearSlots(0x21,1)`, `Fight_LoadBotCompositeSpriteTiles(bot)`. Próximo: 0x1E.

## Handlers AF
### 0x080249C0 AF 0x00: Fight_DispatchOnSubstateAA_AF_AccumPartAndCheckTech
- `Fight_AccumRosPart(fight+0x87, fight+0xA7)`; `Fight_ApplyStatusCostIfFlag(fight+0x87)`; se `FUN_0803725C()` != 0: +0xA6 = 0x1A, +0xAA += 1.
  Senão, se `Fight_HasNoTechnique16Part(bot)` == 0: +0xA6 = 0x39, +0xAA += 1. Senão: `Fight_IncrementActionUsageCounter_08036270`,
  `FUN_08037280(fight+0x87, fight+0xA7)`, +0xAA = 0x04. Próximos: 0x39 / 0x04 / 0x1A (não lidos).
- Confiança: provável (decompile lido; nome "AccumRosPart" vem do mtbe.csv e está em conflito pendente com 0x0803579C).
### 0x08024AF4 AF 0x0B: Fight_DispatchOnSubstateAA_AF_InitSlotAttrs
- `Slot_ClearAttrMask(0x21,1)`; em gObjectSlots (0x03002C30, inferido pelos literais): zera o halfword +0x880 com DAT_08024B70,
  zera um byte, copia 2 halfwords do bot (+0x1742 e +0x1744-4) para slots fixos, escreve bytes 7 e 0xE0 e 0;
  pela fight+0x8B (1 / 2 / outro) escreve uma palavra de posição; `snd_sfx_request(0x6A)`; fight+0xAA += 1. Próximo: 0x0C (inferido).
- Confiança: provável (decompile lido; endereços exatos dos offsets saem de literais não traduzidos).
### 0x08024BCC AF 0x0C: Fight_DispatchOnSubstateAA_AF_WaitSlot22
- `FUN_08036AC4(0x22,0)`; quando != 0: contador gObjectSlots+0x86 = 0x3C; fight+0xAA += 1. Próximo: 0x0E (inferido).
- Ponteiro de tabela compartilhado com AI (mesmo handler).
- Confiança: provável (decompile lido; ver nota de semântica de FUN_08036AC4 = não lida).
### 0x08024A30 AF 0x08: Fight_DispatchOnSubstateAA_AF_SetupOverlay
- decompress + cpufastset; gObjectSlots+0x840 |= 0x220; atributos do slot pelo bot (+0x1742/+0x1744); +0x86 = 0x3C. Próximo: 0x0B.
### 0x08024BF8 AF 0x0E: Fight_DispatchOnSubstateAA_AF_SetSlotFlagsPos
- `Slot_SetFlagBit15(0x21,1)`; gObjectSlots+0x840 |= 0x220; dois bytes de posição (gObjectSlots+DAT_08024C38/3C) = 0x80; fight+0xAA += 1. Próximo: 0x0F.
- Ponteiro de tabela compartilhado com AI.
- Confiança: provável (decompile lido; nome provável; offsets dos bytes de posição não resolvidos).
### 0x08024C44 AF 0x0F: Fight_DispatchOnSubstateAA_AF_CountDownToMark
- Decrementa byte de posição em 8 até o valor '@'; então sfx 0x3C, zera +0x86, avança. Próximo: 0x10.
### 0x08024C94 AF 0x10: Fight_DispatchOnSubstateAA_AF_OscillateSlot
- +0x86++; a cada passo muda x do slot ±2 (alterna pelo bit 4). Em 0x3C: `Tile_ClearSlots(0x21,2)`, avança. Próximo: 0x11.
### 0x08024D18 AF 0x11: Fight_DispatchOnSubstateAA_AF_SetSlotParams
- Escreve bot+0xC9 = 0x2B; `FUN_08036e54` sobre +0xE8/+0xEC; modo de +0x8B (1->2, 2->3, senão 1) em bot+0xCA. Próximo: 0x12.
### 0x08024DA8 AF 0x12: Fight_DispatchOnSubstateAA_AF_CopyTileData
- Copia 0xE bytes (`FUN_0803f63c`) da fonte escolhida por +0x8B; +0xA6 = 0x30. Próximo: 0x13 (compartilhado).

## Handlers AG
### 0x08024E2C AG 0x00: Fight_DispatchOnSubstateAA_AG_AccumPartAndCheckTech
- Código idêntico ao AF 0x00 (0x080249C0), só muda o literal da struct (`0x03001800`): mesma cadeia (+0xA6 = 0x39 / 0x04 / 0x1A; +0xAA += 1).
- Confiança: provável (decompile lido; equivalência com AF 0x00 lida byte a byte no decompile, não no disassembly).
### 0x08024E9C AG 0x08: Fight_DispatchOnSubstateAA_AG_SetupSlots
- decompress + cpufastset (0x10 words); flags gObjectSlots+0x840 (|0x1000 se bot+0xF8 bit0; |0x220); +0x86 = 0x3C; posição do bot. Próximo: 0x0B.
### 0x08024F90 AG 0x0B: Fight_DispatchOnSubstateAA_AG_PlaySfx33SetFlags
- `snd_sfx_request(0x33)`; gObjectSlots+0x840 |= 0x220; +0xAA += 1. Próximo: 0x0C.
- Confiança: provável (decompile lido; nome provável).
### 0x08024FC4 AG 0x0C: Fight_DispatchOnSubstateAA_AG_SlideSlotsIn
- Desloca slot ±0x80 por contador; ao chegar em -0x80 avança. Ramo "else" incrementa outro byte (destino não resolvido). Próximo: 0x0D.
### 0x0802503C AG 0x0D: Fight_DispatchOnSubstateAA_AG_SpawnEffectSlot
- Limpa flags; atributos do slot; `Slot_SetupFromRecordList(...,0x21)`; sfx 0x4F. Próximo: 0x0F.
### 0x08025104 AG 0x0F: Fight_DispatchOnSubstateAA_AG_SetBotParamsFromE8EC
- `Tile_ClearSlots(0x21,1)`; bot block (`0x03000590+bot*0x100`): +0xC9 = 0x2C, +0xCA = 0, +0xCB = `FUN_08036E54(fight+0xE8)`,
  +0xCC = `FUN_08036E54(fight+0xEC)`; fight+0xAA += 1. Próximo: 0x10 (inferido).
- Obs.: o AF 0x11 escreve +0xC9 = 0x2B no mesmo lugar (passo parecido em outro despachante).
- Confiança: provável (decompile lido; FUN_08036E54 não lida).
### 0x08025174 AG 0x10: Fight_DispatchOnSubstateAA_AG_SetStep33
- fight+0xA6 = 0x33; fight+0xAA += 1. Próximo: 0x11 (não há handler AG 0x11 na lista; cadeia AG termina aqui).
- Confiança: confirmado (decompile lido; o nome é provável).

## Handlers AH
### 0x080251B4 AH 0x00: Fight_DispatchOnSubstateAA_AH_AccumPartBranchOn58
- Mesma abertura do AF 0x00: `Fight_AccumRosPart(fight+0x87, fight+0xA7)`, `Fight_ApplyStatusCostIfFlag`, `FUN_0803725C`.
  Se 0: se `byte(bot block+0x58) == 0` -> +0xA6 = 0x34, +0xAA += 1; senão `Fight_IncrementActionUsageCounter_08036270`,
  `FUN_08037280`, +0xAA = 0x04. Se `FUN_0803725C` != 0: +0xA6 = 0x1A, +0xAA += 1. Próximos: 0x34 / 0x04 / 0x1A (não lidos).
- Diferença p/ AF 0x00: o teste é do byte +0x58 do bot (vizinho de +0x59 = parte quebrada), não do técnico 16.
- Confiança: provável (decompile lido; semântica de +0x58 não confirmada).
### 0x08025230 AH 0x08: Fight_DispatchOnSubstateAA_AH_SetupOverlay
- `decompress_and_store` + `bios_cpufastset(...,0x10)`; gObjectSlots+0x840 |= 0x220; bytes/halfwords de gObjectSlots
  (+0x40, +0x40, 0x5000, +0x864 = 3, +0x870 = bot+0x1742, +0x... = bot+0x1744-4); `*(short)(gObjectSlots+0x86) = 0x3C`; fight+0xAA += 1. Próximo: 0x09 (inferido).
- Obs.: mesmo padrão do AF 0x08 (SetupOverlay) — ver as duas seções.
- Confiança: provável (decompile lido; offsets de gObjectSlots pelo padrão do AF 0x0B/0x08).
### 0x08025308 AH 0x09: Fight_DispatchOnSubstateAA_AH_WaitCounterOrFlag
- `FightPreview_AdvanceSlotYWrap(0x21)`. Se (`byte(gObjectSlots+0x89) != 0` OU bit 0 de `*(0x0802533C)` == 0) e o contador
  `short(gObjectSlots+0x86)` != 0: decrementa o contador e espera (não avança). Caso contrário: fight+0xAA += 1. Próximo: 0x0A.
- Confiança: provável (decompile lido; "gObjectSlots" inferido pelo +0x86 e +0x840 das outras seções).
### 0x08025354 AH 0x0A: Fight_DispatchOnSubstateAA_AH_StepSlotThenWrapY
- `Fight_StepSlotTowardTarget(0x21)`; se retornou != 0: fight+0xAA += 1. Sempre: `FightPreview_AdvanceSlotYWrap(0x21)`. Próximo: 0x0B.
- Confiança: confirmado (decompile lido; o nome é provável).
### 0x08025378 AH 0x0B: Fight_DispatchOnSubstateAA_AH_SetupTwoBotSlots
- `FightPreview_AdvanceSlotYWrap(0x21)`; gObjectSlots+0x880 = valor do literal; zera um byte; copia para slots 2 pares
  (+0x1742 e +0x1744-4 do bot) para os slots: o bot de fight+0x88 vai para os offsets DAT_08025480/84 e o de fight+0x87
  para +0x2C/+0x2E do mesmo bloco; escreve bytes 1/1/7/0xE0, halfwords, +0x870 e bytes = 3; `snd_sfx_request(0x6F)`;
  fight+0xAA += 1. Próximo: 0x0C (inferido).
- Obs.: é o único handler AH que lê dois bots (+0x88 e +0x87) para montar slots.
- Confiança: provável (decompile lido; offsets exatos dos slots não todos resolvidos).
### 0x0802549C AH 0x0C: Fight_DispatchOnSubstateAA_AH_StepScrollUntilMark
- `FightPreview_AdvanceSlotYWrap(0x21)`; lê um byte de tabela (cursor em `base+DAT_080254D4`). Se for `'@'` (0x40, marca de fim):
  copia 2 halfwords de uma fonte fixa para o destino e fight+0xAA += 1. Senão: cursor++ e contador fight+DAT_080254FC++ (espera).
  Mesmo padrão de marca `'@'` do AF 0x0F. Próximo: 0x0D (inferido).
- Confiança: provável (decompile lido; tabela de cursor não identificada).
### 0x08025500 AH 0x0D: Fight_DispatchOnSubstateAA_AH_StepSlotCopyPos
- `Fight_StepSlotTowardTarget(0x21)`; se chegou: copia 2 halfwords de posição (fonte DAT_08025560/DAT_08025568 para destino DAT_08025564),
  `snd_sfx_request(0x2B)`, +0xAA += 1. Sempre (depois): `FightPreview_AdvanceSlotYWrap(0x21)` e copia de novo as mesmas posições.
  Próximo: 0x0E (inferido).
- Confiança: provável (decompile lido; o nome é provável).
### 0x08025570 AH 0x0E: Fight_DispatchOnSubstateAA_AH_CountdownAndClear
- `FightPreview_AdvanceSlotYWrap(0x21)`; copia 2 halfwords de posição (sempre). Lê um byte contador em `base+DAT_080255C0`:
  se == 1: `Tile_ClearSlots(0x21,2)`, fight+0xAA += 1. Senão: contador--, decrementa dois bytes (DAT_080255F0 e DAT_080255F4) e um
  byte em `base+DAT_080255F0+0x40`. Próximo: 0x0F.
- Confiança: provável (decompile lido; nome provável).
### 0x080255F8 AH 0x0F: Fight_DispatchOnSubstateAA_AH_SplitCountDamage
- `FightPreview_SplitCountByCap58(bot,+0xEC)`; +0x51 = +0xC8; `Fight_SubClampZeroByte` no bot+0x10; `FightPreview_SplitCountByFree(+0x87,+0x51)`; +0x52 = +0xC8; `Fight_AddByteClampToMax`. Próximo: 0x10.
### 0x08025674 AH 0x10: Fight_DispatchOnSubstateAA_AH_DrawNumbers
- decompress; `Fight_WriteNumberTiles` + `Fight_BuildBotOamEntriesAltTiles` para dois bots; sfx 0x5B. Próximo: 0x11.
### 0x080256EC AH 0x11: Fight_DispatchOnSubstateAA_AH_WaitRowAnim
- `Fight_StepRowAnimation` para bots +0x88 e +0x87; contador até 0x32; 'x' limpa slots dos dois bots. Próximo: 0x12 (compartilhado).

## Confiança
- Confirmado (decompile lido): 0x08024210, 0x080242F8, 0x08024488, 0x080246C0, 0x0802473C, 0x0802480C, 0x08024890, 0x0802494C, 0x08024A30, 0x08024C44, 0x08024C94, 0x08024D18, 0x08024DA8, 0x08024E9C, 0x08024FC4, 0x0802503C, 0x080255F8, 0x08025674, 0x080256EC.
- Provável: nomes dos passos; cadeias inferidas; semântica de "Split"/"Oscillate".
- Redo (decompile lido, confiança provável ou confirmado por seção): 0x080241F0, 0x08024538, 0x0802463C, 0x08024790, 0x080247C8, 0x080247F4, 0x08024870, 0x080249C0, 0x08024AF4, 0x08024BCC, 0x08024BF8, 0x08024E2C, 0x08024F90, 0x08025104, 0x08025174, 0x080251B4, 0x08025230, 0x08025308, 0x08025354, 0x08025378, 0x0802549C, 0x08025500, 0x08025570.
- Pendente: nenhum nesta lista. Resta validar nomes de FUN_0803725C, FUN_08037280, FUN_080370AC, FUN_08036AC4, FUN_08036E54 (não lidos aqui).
