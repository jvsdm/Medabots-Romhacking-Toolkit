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
- 19 de 42 lidos no Ghidra (decompile) e renomeados.
- 23 de 42: o Ghidra não tem função nesse endereço (miolo não definido; `decompile` e `disassemble` falham).
  Não decodificados à mão. Pendente: criar a função no Ghidra (Create Function) ou usar capstone.

## Máquina de estados (cadeias)
- AE: 0A -> 0B -> 0C -> 0D -> 0E -> 0F -> 10 -> 11. Em 11: se a parte não está quebrada (bloco+0x59 != 0) -> 13;
  se quebrada -> 1A (com `Fight_QueueHeadBrokenBotForResolution`). Depois 13 -> 15 -> 16 -> 1A -> 1B -> 1C -> 1D -> 1E (sem entrada AE).
- AF: 00 -> 08 -> 0B -> 0C -> 0E -> 0F -> 10 -> 11 -> 12 (-> 13 compartilhado).
- AG: 00 -> 08 -> 0B -> 0C -> 0D -> 0F -> 10.
- AH: 00 -> 08 -> 09 -> 0A -> 0B -> 0C -> 0D -> 0E -> 0F -> 10 -> 11 (-> 12 compartilhado).
Transições "->" sem handler confirmado são inferidas pelo +1 dos handlers vizinhos.

## Handlers AE
### 0x08024210 AE 0x0B: Fight_DispatchOnSubstateAA_AE_ResetBotGfx
- Copia 4x 0x40 bytes do bot (0x880..0x940) via `FUN_0803f63c`; zera bytes do bot; `Slot_ClearAttrMask(0x22,4)`; mostra slot; sfx 0x34. Próximo: 0x0C.
### 0x080242F8 AE 0x0C: Fight_DispatchOnSubstateAA_AE_StepSlotAnim
- Move cursor ±0x10 conforme +0xF8 e `FUN_08036ac4(0x21,0)`; anima 4 slots (índices bot*4+0x5A..0x5D). Ao terminar: esconde slot, sfx 0x41. Próximo: 0x0D.
### 0x08024488 AE 0x0D: Fight_DispatchOnSubstateAA_AE_SetupFourSlots
- Preenche 4 entradas de 0x20 halfwords em 0x980 (posições do bot). Zera +0x84. Próximo: 0x0E.
### 0x080246C0 AE 0x10: Fight_DispatchOnSubstateAA_AE_WaitRowAnim
- `Fight_StepRowAnimation(bot,0x21)`; conta em +0x...-0x6A até >0x32 ou 'x'; ao 'x' limpa slots e avança. Próximo: 0x11.
### 0x0802473C AE 0x11: Fight_DispatchOnSubstateAA_AE_ApplyPartDamage
- `Fight_SubtractPartArmorClampZero(bot,0,+0xC8)`. Parte quebrada (bloco+0x59==0): enfileira bot e vai para 0x1A; senão 0x13.
### 0x08024790 AE 0x13: sem função no Ghidra (pendente). Próximo: 0x15 (inferido).
### 0x080247C8 AE 0x15: sem função no Ghidra (pendente).
### 0x080247F4 AE 0x16: sem função no Ghidra (pendente).
### 0x0802480C AE 0x1A: Fight_DispatchOnSubstateAA_AE_LoadBrokenGfx
- `decompress_and_store` + `bios_cpufastset` (8 words); `FUN_08036ff8(0x21,x,y,4)` com posição do registro de +0xD0. Próximo: 0x1B.
### 0x08024870 AE 0x1B: sem função no Ghidra (pendente).
### 0x08024890 AE 0x1C: Fight_DispatchOnSubstateAA_AE_LoadSlotGfx
- `decompress_and_store` + `bios_cpufastset`; seta gObjectSlots+0x840 e posição do slot; `Slot_SetupFromRecordList(bot,0x21)`. Próximo: 0x1D.
### 0x0802494C AE 0x1D: Fight_DispatchOnSubstateAA_AE_RunSlotScript
- `Anim_RunSlotScript(bot,0x21)`; quando o script termina (byte == -1): `Tile_ClearSlots(0x21,1)`, `Fight_LoadBotCompositeSpriteTiles(bot)`. Próximo: 0x1E.

## Handlers AF
### 0x080249C0 AF 0x00: sem função no Ghidra (pendente).
### 0x08024A30 AF 0x08: Fight_DispatchOnSubstateAA_AF_SetupOverlay
- decompress + cpufastset; gObjectSlots+0x840 |= 0x220; atributos do slot pelo bot (+0x1742/+0x1744); +0x86 = 0x3C. Próximo: 0x0B.
### 0x08024AF4 AF 0x0B: sem função no Ghidra (pendente).
### 0x08024BCC AF 0x0C: sem função no Ghidra (pendente). Ponteiro compartilhado com AI.
### 0x08024BF8 AF 0x0E: sem função no Ghidra (pendente).
### 0x08024C44 AF 0x0F: Fight_DispatchOnSubstateAA_AF_CountDownToMark
- Decrementa byte de posição em 8 até o valor '@'; então sfx 0x3C, zera +0x86, avança. Próximo: 0x10.
### 0x08024C94 AF 0x10: Fight_DispatchOnSubstateAA_AF_OscillateSlot
- +0x86++; a cada passo muda x do slot ±2 (alterna pelo bit 4). Em 0x3C: `Tile_ClearSlots(0x21,2)`, avança. Próximo: 0x11.
### 0x08024D18 AF 0x11: Fight_DispatchOnSubstateAA_AF_SetSlotParams
- Escreve bot+0xC9 = 0x2B; `FUN_08036e54` sobre +0xE8/+0xEC; modo de +0x8B (1->2, 2->3, senão 1) em bot+0xCA. Próximo: 0x12.
### 0x08024DA8 AF 0x12: Fight_DispatchOnSubstateAA_AF_CopyTileData
- Copia 0xE bytes (`FUN_0803f63c`) da fonte escolhida por +0x8B; +0xA6 = 0x30. Próximo: 0x13 (compartilhado).

## Handlers AG
### 0x08024E2C AG 0x00: sem função no Ghidra (pendente).
### 0x08024E9C AG 0x08: Fight_DispatchOnSubstateAA_AG_SetupSlots
- decompress + cpufastset (0x10 words); flags gObjectSlots+0x840 (|0x1000 se bot+0xF8 bit0; |0x220); +0x86 = 0x3C; posição do bot. Próximo: 0x0B.
### 0x08024F90 AG 0x0B: sem função no Ghidra (pendente).
### 0x08024FC4 AG 0x0C: Fight_DispatchOnSubstateAA_AG_SlideSlotsIn
- Desloca slot ±0x80 por contador; ao chegar em -0x80 avança. Ramo "else" incrementa outro byte (destino não resolvido). Próximo: 0x0D.
### 0x0802503C AG 0x0D: Fight_DispatchOnSubstateAA_AG_SpawnEffectSlot
- Limpa flags; atributos do slot; `Slot_SetupFromRecordList(...,0x21)`; sfx 0x4F. Próximo: 0x0F.
### 0x08025104 AG 0x0F: sem função no Ghidra (pendente).
### 0x08025174 AG 0x10: sem função no Ghidra (pendente).

## Handlers AH
### 0x080251B4 AH 0x00: sem função no Ghidra (pendente).
### 0x08025230 AH 0x08: sem função no Ghidra (pendente).
### 0x08025308 AH 0x09: sem função no Ghidra (pendente).
### 0x08025354 AH 0x0A: sem função no Ghidra (pendente).
### 0x08025378 AH 0x0B: sem função no Ghidra (pendente).
### 0x0802549C AH 0x0C: sem função no Ghidra (pendente).
### 0x08025500 AH 0x0D: sem função no Ghidra (pendente).
### 0x08025570 AH 0x0E: sem função no Ghidra (pendente).
### 0x080255F8 AH 0x0F: Fight_DispatchOnSubstateAA_AH_SplitCountDamage
- `FightPreview_SplitCountByCap58(bot,+0xEC)`; +0x51 = +0xC8; `Fight_SubClampZeroByte` no bot+0x10; `FightPreview_SplitCountByFree(+0x87,+0x51)`; +0x52 = +0xC8; `Fight_AddByteClampToMax`. Próximo: 0x10.
### 0x08025674 AH 0x10: Fight_DispatchOnSubstateAA_AH_DrawNumbers
- decompress; `Fight_WriteNumberTiles` + `Fight_BuildBotOamEntriesAltTiles` para dois bots; sfx 0x5B. Próximo: 0x11.
### 0x080256EC AH 0x11: Fight_DispatchOnSubstateAA_AH_WaitRowAnim
- `Fight_StepRowAnimation` para bots +0x88 e +0x87; contador até 0x32; 'x' limpa slots dos dois bots. Próximo: 0x12 (compartilhado).

## Confiança
- Confirmado (decompile lido): 0x08024210, 0x080242F8, 0x08024488, 0x080246C0, 0x0802473C, 0x0802480C, 0x08024890, 0x0802494C, 0x08024A30, 0x08024C44, 0x08024C94, 0x08024D18, 0x08024DA8, 0x08024E9C, 0x08024FC4, 0x0802503C, 0x080255F8, 0x08025674, 0x080256EC.
- Provável: nomes dos passos; cadeias inferidas; semântica de "Split"/"Oscillate".
- Pendente (sem função no Ghidra): 0x080241F0, 0x08024538, 0x0802463C, 0x08024790, 0x080247C8, 0x080247F4, 0x08024870, 0x080249C0, 0x08024AF4, 0x08024BCC, 0x08024BF8, 0x08024E2C, 0x08024F90, 0x08025104, 0x08025174, 0x080251B4, 0x08025230, 0x08025308, 0x08025354, 0x08025378, 0x0802549C, 0x08025500, 0x08025570.
