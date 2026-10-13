# HA6: handlers dos despachantes Fight_DispatchOnSubstateAA_Z, _AA, _AB, _AC, _AD, _AE

Lista: `HA6_lista.txt` (42 linhas: endereço, despachante, índice = valor de +0xAA, e na coluna 4 outros
despachantes que aparecem com o mesmo endereço na lista; essa coluna NÃO foi verificada na ROM).
Bloco de estado: `0x03001800` (literal do pool). Subestado = byte +0xAA. Cada despachante chama
`tabela[+0xAA]` (trampolim `0x0807CCF0`); cada handler faz +0xAA += 1 ao terminar (passo seguinte),
ou fica no mesmo índice enquanto espera.
Literais usados pelos handlers: `0x03000440` (+0x84 = contador de passo, +0x86 = temporizador halfword),
`0x03000590` (bloco do bot, base + bot*0x100), `0x03002C30` (gObjectSlots; slot 0x21 = objeto da animação).
Dado = endereço ou valor de tabela, não código.

Redo (HA6): os 19 endereços que antes eram "NÃO DEFINIDO" agora são funções no Ghidra; foram decompilados e
renomeados como `Fight_DispatchOnSubstateAA_<D>_<idx>_<Passo>` (ex.: `..._AA_04_LoadGfxSetupSlot21Sfx4D`).
Os nomes antigos sem índice (ex.: `Fight_DispatchOnSubstateAA_Z_TickCounterUp`) continuam válidos para as seções já confirmadas.
Os alvos de chamada `Fight_GetPartProficiencyBase`/`Fight_ApplyVirusBugRosPenalty`/`Fight_CheckRosMeetsMinimumThreshold`
aparecem no Ghidra como `Fight_AccumRosPart`/`Fight_ApplyStatusCostIfFlag`/`FUN_0803725C` (conflito aberto).

## Máquina de estados (esboço, por despachante)

- Z (0x06..0x0F): contador de passo 0x84 sobe/desce até 0 ou 0x10; efeitos de tile/scroll; ganho de
  gauge com teto 0xA0; ajuste de slots. Z/0F-0E-0D-0B: espera e decaimento de slots.
- AA (0x00..0x0B): 00 = cálculo de ROS da peça; 04 = carga de gráficos + init de slot; 05-0A = animação do
  slot 0x21 com espera de fim de script, sfx 0x4D, contador 0x84 e preview; 0B = seta A6=0x1D.
- AB (0x00..0x0F): 00 = cálculo de ROS; 08 = carga de gráficos + slot; 09-0C = espera (animação, timer,
  flag); 0A = move slot até o alvo; 0B = slot + sfx 0x4E; 0D/0E = passos de fade e cópia de valores;
  0F = seta A6=0x2C.
- AC (0x00..0x0F): 00 = ROS; 08 = gráficos + init; 0B = slot + sfx 0x4E (cor 0x46); 0E = lê registro do
  bot (não concluído); 0F = seta A6=0x2B.
- AD (0x00..0x16): 00 = ROS; 08 = gráficos + init; 0B = slot + sfx (0x44); 0D/0E/0F = fade, carga
  de gráficos e escrita de linhas do bot; 12/15/16 = slot + sfx (0x43), escrita de linhas, seta A6=0x2D.
- AE (0x00, 0x08): 00 = ROS; 08 = gráficos + init.

## Despachante Z (stub 0x0801C4FC-like, tabela de 0x06..0x0F)

## 0x080230C8 Fight_DispatchOnSubstateAA_Z_TickCounterUp (índice 0x06)
- Passo: incrementa contador 0x84 de 0x03000440 com `FUN_0803f684`/`FUN_0803f998` (efeito por contador) até 0x10; então +0xAA.
- Lê/escreve: 0x03000440+0x84 (escreve); 0x03001800+0xAA (escreve). Confiança: confirmado (decompile); funções chamadas sem nome.

## 0x08023118 Fight_DispatchOnSubstateAA_Z_07_InitSlot21FromBotSfx4C (Z 0x07)
- Passo (decompile): monta UM slot (0x21 = gObjectSlots+0x840, base 0x03002C30): halfword +0x00 = 0xC604; byte +0x1C = 0; halfwords +0x02/+0x04 copiados de gObjectSlots+0x1742/+0x1744 do bot (`bot = byte 0x03001800+0x87`, passo 0x100), +0x04 com −4; halfword +0x06 = 0x4800; bytes +0x0A = 6, +0x0B = 0xF0, +0x08 = 0x40, +0x09 = 0x40; halfword +0x18 (=0x858) = 0; byte +0x2D (=0x86D) = 0; `snd_sfx_request(0x4C)`; `0x03001800+0xAA++`.
- Lê/escreve: gObjectSlots+0x840..0x86D; lê gObjectSlots+bot*0x100+0x1742/0x1744; escreve 0x03001800+0xAA.
- Para o Lua: porta (inicializa a animação 0x21 do bot).
- Confiança: confirmado (decompile) para o efeito; "bot" = byte +0x87 é provável; o sentido de 0x4800/0x4C é chute. Antes a seção dizia "3 entradas": errado, é um slot só.

## 0x080231D4 Fight_DispatchOnSubstateAA_Z_TileClearOrScroll (índice 0x08)
- Passo: se o byte em (base+0x...) == '@' limpa slots 0x21 (2) e +0xAA; senão soma 0x10 a um halfword (scroll) e avança o byte.
- Lê/escreve: 0x03001800+0xAA; halfword de base+índice (base = 0x03001800 via literal; deslocamento no literal 0x0802321C). Confiança: confirmado.

## 0x08023224 Fight_DispatchOnSubstateAA_Z_TickCounterDown (índice 0x09)
- Passo: contador 0x03000440+0x84 desce até 0 (efeito por contador com `FUN_0803f684`/`FUN_0803f998`), então +0xAA.
- Lê/escreve: 0x03000440+0x84; 0x03001800+0xAA. Confiança: confirmado.

## 0x08023274 Fight_DispatchOnSubstateAA_Z_0A_SetupAllySlotsFromBots (Z 0x0A)
- Passo (decompile): `base = Fight_GetAllySideBaseIndex(byte 0x03001800+0x87)` (0 ou 3). Para k = 0..2 com o bot `base+k` ligado (byte 0x03000590+(base+k)*0x100+0x40 != 0): monta o slot 0x21+k (gObjectSlots+0x840+k*0x40): halfword +0x00 = 0xC624; byte +0x1C = 0; halfwords +0x02/+0x04 copiados de gObjectSlots+(base+k)*0x100+0x1742/+0x1744; halfword +0x06 = 0x4801; bytes +0x0A = 7, +0x0B = 0xF0, +0x08 = 0x40, +0x09 = 0x40. Depois `0x03000440+0x86 = 0`; `snd_sfx_request(0x2E)`; `0x03001800+0xAA++`.
- Lê/escreve: 0x03000590 (flags por bot), gObjectSlots+0x840..0x9FF, gObjectSlots+0x1742 (dados por bot), 0x03000440+0x86, 0x03001800+0xAA.
- Para o Lua: porta.
- Confiança: confirmado (decompile). O lado = `Fight_GetAllySideBaseIndex` (nome do Ghidra; ver H.md para base 0/3). Antes a seção dizia "laço de 3 bots de 0x03000590 para +0x840": a fonte dos dados é gObjectSlots+0x1742, não o bloco do bot.

## 0x08023328 Fight_DispatchOnSubstateAA_Z_WaitAndDecaySlots (índice 0x0B)
- Passo: temporizador halfword 0x03000440+0x86: se 0x14 seta 0x3C e +0xAA; senão sobe e decrementa +4 dos slots com bit 0x8000 (gObjectSlots+0x840, passo 0x40).
- Lê/escreve: 0x03000440+0x86; gObjectSlots+0x840..; 0x03001800+0xAA. Confiança: confirmado.

## 0x08023384 Fight_DispatchOnSubstateAA_Z_0D_SlotBitStepClearTiles (Z 0x0D)
- Passo (decompile): laço de 3 slots (gObjectSlots+0x840+k*0x40): se o halfword +0x00 tem bit 0x8000, byte +0x08 −= 8 e byte +0x09 += 8. Depois, se o byte gObjectSlots+0x848 (= slot 0x21 +0x08) == 8: `Tile_ClearSlots(0x21, 3)` e `0x03001800+0xAA++`.
- Lê/escreve: gObjectSlots+0x840..0x9FF (bit 0x8000 e bytes +0x08/+0x09); lê +0x848; escreve 0x03001800+0xAA.
- Para o Lua: porta.
- Confiança: confirmado (decompile). Correção: o fim chama `Tile_ClearSlots`, não `ClearObjectAttributeEntries` (0x08040CD0) como estava antes.

## 0x080233E0 Fight_DispatchOnSubstateAA_Z_AddGaugeMax160 (índice 0x0E)
- Passo: `Fight_AddByteClampToMax` soma (short bot+0xEC) ao byte de gauge (bot+0x89*8+0x28), teto 0xA0; +0xAA.
- Lê/escreve: 0x03001800+0x89, +0xEC, +0x28..; +0xAA. Confiança: confirmado.

## 0x08023414 Fight_DispatchOnSubstateAA_Z_0F_SetStepCode1C (Z 0x0F)
- Passo (decompile): grava 0x1C em 0x03001800+0xA6 e `0x03001800+0xAA++`.
- Lê/escreve: +0xA6, +0xAA. Para o Lua: porta (uma linha).
- Confiança: confirmado (decompile); o sentido de 0x1C é chute (ver nota de +0xA6 no fim).

## Despachante AA (índices 0x00..0x0B)

## 0x08023454 Fight_DispatchOnSubstateAA_AA_00_RosCalcPart (AA 0x00)
- Passo (decompile): `bot = byte 0x03001800+0x87`, `part = byte 0x03001800+0xA7`. Chama `Fight_AccumRosPart(bot, part)` e `Fight_ApplyStatusCostIfFlag(bot)`; se `FUN_0803725C()` == 0: `Fight_IncrementActionUsageCounter_08036270()`, `FUN_08037280(bot, part)` e +0xAA = 4. Se != 0: grava 0x1A em +0xA6 e +0xAA++ (repete o passo).
- Lê/escreve: 0x03001800+0x87, +0xA7, +0xA6, +0xAA.
- Para o Lua: porta a lógica de controle; as funções chamadas ficam para as seções delas.
- Confiança: confirmado (decompile) para o fluxo. Nomes: o Ghidra chama o alvo de `Fight_AccumRosPart` e o de `Fight_ApplyStatusCostIfFlag` (antes anotados como `Fight_GetPartProficiencyBase` 0x0803579C e `Fight_ApplyVirusBugRosPenalty` 0x08035814; conflito ainda aberto, ver MERGE_RELATORIO); `FUN_0803725C` sem nome (antes `Fight_CheckRosMeetsMinimumThreshold`).

## 0x080234A8 Fight_DispatchOnSubstateAA_AA_04_LoadGfxSetupSlot21Sfx4D (AA 0x04)
- Passo (decompile): `decompress_and_store(0x084D745C, 0x06014800)`; `bios_cpufastset(0x0850FD58, 0x050002E0, 8)` (paleta); slot 0x21 (gObjectSlots+0x840): halfword +0x00 = 0xC404; byte +0x1C = 0; halfword +0x02 = bot+0x1742 e +0x04 = bot+0x1744 − 4 (bot = byte 0x03001800+0x87, passo 0x100); byte +0x0A = 7, +0x0B = 0xF0; `Slot_SetupFromRecordList(*(0x083B6100+0x100), 0x21)`; `snd_sfx_request(0x4D)`; `0x03001800+0xAA++`.
- Lê/escreve: gObjectSlots+0x840..0x84B, gObjectSlots+bot*0x100+0x1742/0x1744; escreve 0x03001800+0xAA.
- Para o Lua: porta a parte do slot; a descompressão e a cópia de paleta são do loader (não precisa no Lua).
- Confiança: confirmado (decompile). Os endereços de gráfico são da ROM (0x084D745C, 0x0850FD58), não de RAM.

## 0x08023564 Fight_DispatchOnSubstateAA_AA_WaitScriptEnd (AA 0x05)
- Passo: `Anim_RunSlotScript` (slot 0x21); se o byte de fim == -1: timer 0x86=0x3C e +0xAA.
- Lê/escreve: 0x03000440+0x86; 0x03001800+0xAA. Confiança: confirmado.

## 0x080235AC Fight_DispatchOnSubstateAA_AA_SetupSlotSfx4D (AA 0x07)
- Passo: `Slot_SetupFromRecordList` (slot 0x21, registro +0x104), sfx 0x4D (`snd_sfx_request`), zera +0x84; +0xAA.
- Confiança: confirmado.

## 0x080235E8 Fight_DispatchOnSubstateAA_AA_AnimCounterStep (AA 0x08)
- Passo: contador 0x84 += 1; `FightPreview_SetGroupActive4`; `Anim_RunSlotScript`; no fim do script: `Tile_ClearSlots(0x21,1)`, timer 0x86=0x3C, +0xAA.
- Confiança: confirmado.

## 0x08023658 Fight_DispatchOnSubstateAA_AA_WaitPreviewShowGroup (AA 0x09)
- Passo: contador += 1; `FightPreview_SetGroupActive4`; se bot+0x89==0 e flag 0x03001798 bit 0: `FightPreview_ShowGroup4` e +0xAA; senão espera o timer 0x86 zerar.
- Confiança: confirmado.

## 0x080236E0 Fight_DispatchOnSubstateAA_AA_AddGaugeMax120 (AA 0x0A)
- Passo: `Fight_AddByteClampToMax` com teto 0x78 sobre o byte do bot (+0x87*0x100 + base); +0xAA.
- Confiança: confirmado.

## 0x08023718 Fight_DispatchOnSubstateAA_AA_0B_SetStepCode1D (AA 0x0B)
- Passo (decompile): grava 0x1D em 0x03001800+0xA6; `0x03001800+0xAA++`.
- Lê/escreve: +0xA6, +0xAA. Para o Lua: porta (uma linha).
- Confiança: confirmado (decompile); o sentido de 0x1D é chute (ver nota de +0xA6).

## Despachante AB (índices 0x00..0x0F)

## 0x08023758 Fight_DispatchOnSubstateAA_AB_00_RosCalcPartNoTech16 (AB 0x00)
- Passo (decompile): `Fight_AccumRosPart(bot = byte +0x87, part = byte +0xA7)`; `Fight_ApplyStatusCostIfFlag(bot)`; se `FUN_0803725C()` == 0: se `Fight_HasNoTechnique16Part(byte +0x88)` == 0, grava 0x39 em +0xA6 e +0xAA++; senão `Fight_IncrementActionUsageCounter_08036270()`, `FUN_08037280(bot, part)` e +0xAA = 4. Se `FUN_0803725C()` != 0: grava 0x1A em +0xA6 e +0xAA++.
- Lê/escreve: 0x03001800+0x87, +0x88, +0xA7, +0xA6, +0xAA.
- Para o Lua: porta a lógica de controle.
- Confiança: confirmado (decompile) para o fluxo. Difere do AA 0x00 só no ramo "sem técnica 16": grava 0x39 e não avança para 4 (o 0x39 é sentido chute). Ver nota dos nomes em AA 0x00.

## 0x080237C8 Fight_DispatchOnSubstateAA_AB_LoadGfxSetupSlot (AB 0x08)
- Passo: `decompress_and_store` + `bios_cpufastset`; seta flags em gObjectSlots+0x840 (|0x220); bytes 0x40 e halfword; copia valores de display do bot (+0x1742/+0x1744) e 0x864=6; +0x84=0x42; `Slot_SetupFromRecordList`; timer 0x86=0x3C; +0xAA.
- Confiança: confirmado.

## 0x080238A0 Fight_DispatchOnSubstateAA_AB_WaitAnimOrTimer (AB 0x09)
- Passo: `Anim_RunSlotScript` (índice do byte +0x84*4); se flag de bot ou bit 0 de 0x03001798 não ativos, espera timer 0x86 zerar; senão +0xAA.
- Confiança: confirmado.

## 0x08023900 Fight_DispatchOnSubstateAA_AB_MoveSlotTowardTarget (AB 0x0A)
- Passo: `Anim_RunSlotScript`; desloca o X do slot ±0x40 conforme bit 0 de +0xF8; `Fight_StepSlotTowardTarget` (0x21); ao chegar zera o campo e +0xAA.
- Confiança: confirmado.

## 0x08023988 Fight_DispatchOnSubstateAA_AB_SetupSlot43Sfx4E (AB 0x0B)
- Passo: +0x84=0x43; `Slot_SetupFromRecordList` (registro +0x10C); sfx 0x4E; +0xAA.
- Confiança: confirmado.

## 0x080239C0 Fight_DispatchOnSubstateAA_AB_WaitScriptFlag (AB 0x0C)
- Passo: `Anim_RunSlotScript`; se o byte de fim == -1 então +0xAA.
- Confiança: confirmado.

## 0x08023A04 Fight_DispatchOnSubstateAA_AB_StepFadeBytes8 (AB 0x0D)
- Passo: byte de fade decrementa 8; se já era 8: `Tile_ClearSlots(0x21,1)` e +0xAA; senão soma 8 ao byte vizinho.
- Confiança: confirmado.

## 0x08023A48 Fight_DispatchOnSubstateAA_AB_CopyDisplayValues (AB 0x0E)
- Passo: grava 0x26 e 0 no registro do bot (0x03000590 + bot*0x100 + 0xC4/0xC5) e os valores de `Fight_ClampDisplayValue1To99` (0x08036E54) de +0xE8 e +0xEC em +0xC6 e +0xC7; +0xAA.
- Confiança: confirmado.

## 0x08023AB0 Fight_DispatchOnSubstateAA_AB_0F_SetStepCode2C (AB 0x0F)
- Passo (decompile): grava 0x2C em 0x03001800+0xA6; `0x03001800+0xAA++`.
- Lê/escreve: +0xA6, +0xAA. Para o Lua: porta (uma linha).
- Confiança: confirmado (decompile); o sentido de 0x2C é chute (ver nota de +0xA6).

## Despachante AC (índices 0x00, 0x08, 0x0B, 0x0E, 0x0F)

## 0x08023AF0 Fight_DispatchOnSubstateAA_AC_00_RosCalcPart (AC 0x00)
- Passo (decompile): igual AA 0x00 sem a chamada `Fight_ApplyStatusCostIfFlag` e sem o teste de técnica: `Fight_AccumRosPart(bot, part)`; se `FUN_0803725C()` == 0: `Fight_IncrementActionUsageCounter_08036270()`, `FUN_08037280(bot, part)` e +0xAA = 4; senão +0xA6 = 0x1A e +0xAA++.
- Lê/escreve: 0x03001800+0x87, +0xA7, +0xA6, +0xAA.
- Para o Lua: porta a lógica de controle.
- Confiança: confirmado (decompile) para o fluxo. Nomes dos alvos: ver AA 0x00 (conflito 0x0803579C aberto).

## 0x08023B44 Fight_DispatchOnSubstateAA_AC_08_LoadGfxSetupSlot21 (AC 0x08)
- Passo (decompile): `decompress_and_store(0x084D777C, 0x06014800)`; `bios_cpufastset(0x0850FD98, 0x050002E0, 8)`; slot 0x21 (gObjectSlots+0x840): halfword +0x00 |= 0x220; bytes +0x08 = 0x40, +0x09 = 0x40; halfword +0x18 = 0; halfword +0x30 (0x870) = bot2+0x1742 e +0x32 (0x872) = bot2+0x1744 − 4, com bot2 = byte 0x03001800+0x88; byte +0x24 (0x864) = 6; `0x03000440+0x84 = 0x45`; `Slot_SetupFromRecordList(*(0x083B6100+0x114), 0x21)`; `0x03000440+0x86 = 0x3C`; `0x03001800+0xAA++`.
- Lê/escreve: gObjectSlots+0x840..0x872, gObjectSlots+bot2*0x100+0x1742/0x1744, 0x03000440+0x84/0x86, 0x03001800+0xAA.
- Para o Lua: porta a parte do slot.
- Confiança: confirmado (decompile). Difere do AB 0x08 e AD 0x08: o contador usa 0x45 (não 0x42) e o registro vem de +0x114. Antes a seção dizia "como AA 0x04"; o bot vem do byte +0x88 (não +0x87).

## 0x08023C18 Fight_DispatchOnSubstateAA_AC_SetupSlot46Sfx4E (AC 0x0B)
- Passo: +0x84=0x46; `Slot_SetupFromRecordList` (registro +0x118); sfx 0x4E; +0xAA.
- Confiança: confirmado.

## 0x08023C50 Fight_DispatchOnSubstateAA_AC_0E_SetBotFlagBF (AC 0x0E)
- Passo (decompile): escreve 1 no byte `0x03000590 + byte(0x03001800+0x88)*0x100 + 0xBF` (flag do bot 2, offset Ghidra 0xBF); `0x03001800+0xAA++`.
- Lê/escreve: 0x03001800+0x88, 0x03000590+bot*0x100+0xBF; +0xAA.
- Para o Lua: porta (uma linha).
- Confiança: confirmado (decompile) para a escrita. O significado da flag +0xBF é chute. Antes a seção dizia "lê e escreve +0xAA": o correto é escrever a flag do bot, não ler.

## 0x08023C78 Fight_DispatchOnSubstateAA_AC_0F_SetStepCode2B (AC 0x0F)
- Passo (decompile): grava 0x2B em 0x03001800+0xA6; `0x03001800+0xAA++`.
- Lê/escreve: +0xA6, +0xAA. Para o Lua: porta (uma linha).
- Confiança: confirmado (decompile); o sentido de 0x2B é chute (ver nota de +0xA6).

## Despachante AD (índices 0x00, 0x08, 0x0B, 0x0D, 0x0E, 0x0F, 0x12, 0x15, 0x16)

## 0x08023CB8 Fight_DispatchOnSubstateAA_AD_00_RosCalcPartNoTech16 (AD 0x00)
- Passo (decompile): `Fight_AccumRosPart(bot, part)` (bot = byte +0x87, part = byte +0xA7); `Fight_ApplyStatusCostIfFlag(bot)`; se `FUN_0803725C()` == 0: se `Fight_HasNoTechnique16Part(byte +0x88)` == 0 OU `Fight_HasNoTechnique16Part(bot)` == 0: grava 0x39 em +0xA6 e +0xAA++; senão `Fight_IncrementActionUsageCounter_08036270()`, `FUN_08037280(bot, part)` e +0xAA = 4. Se `FUN_0803725C()` != 0: +0xA6 = 0x1A e +0xAA++.
- Lê/escreve: 0x03001800+0x87, +0x88, +0xA7, +0xA6, +0xAA.
- Para o Lua: porta a lógica de controle.
- Confiança: confirmado (decompile) para o fluxo. Difere do AB 0x00: o teste de técnica 16 é feito para os dois bots (+0x88 e +0x87), não só um. O sentido de 0x39 é chute.

## 0x08023D44 Fight_DispatchOnSubstateAA_AD_08_LoadGfxSetupSlot21 (AD 0x08)
- Passo (decompile): `decompress_and_store(0x084D78C0, 0x06014800)`; `bios_cpufastset(0x0850FDB8, 0x050002E0, 8)`; slot 0x21: halfword +0x00 |= 0x220; bytes +0x08 = 0x40, +0x09 = 0x40; halfword +0x18 = 0; halfword +0x30 = bot2+0x1742 e +0x32 = bot2+0x1744 − 4 (bot2 = byte 0x03001800+0x88); byte +0x24 = 6; `0x03000440+0x84 = 0x42`; `Slot_SetupFromRecordList(*(0x083B6100+0x108), 0x21)`; `0x03000440+0x86 = 0x3C`; `0x03001800+0xAA++`.
- Lê/escreve: gObjectSlots+0x840..0x872, gObjectSlots+bot2*0x100+0x1742/0x1744, 0x03000440+0x84/0x86, +0xAA.
- Para o Lua: porta a parte do slot.
- Confiança: confirmado (decompile). Igual ao AB 0x08 no fluxo (contador 0x42, registro +0x108 em vez de +0x104).

## 0x08023E18 Fight_DispatchOnSubstateAA_AD_SetupSlot44Sfx4E (AD 0x0B)
- Passo: +0x84=0x44; `Slot_SetupFromRecordList` (registro +0x110); sfx 0x4E; +0xAA.
- Confiança: confirmado.

## 0x08023E50 Fight_DispatchOnSubstateAA_AD_StepFadeBytes8Clear (AD 0x0D)
- Passo: como AB 0x0D, mas `Slot_ClearAttrMask(0x21,1)` no fim e o segundo byte desce 8.
- Confiança: confirmado.

## 0x08023E94 Fight_DispatchOnSubstateAA_AD_LoadGfxSetupSlot (AD 0x0E)
- Passo: como AB 0x08 (gráficos, flags |0x8000, valores de display, +0x84=0x42, slot registro +0x108, timer 0x3C).
- Confiança: confirmado.

## 0x08023F48 Fight_DispatchOnSubstateAA_AD_0F_FadeSlotBytesStep8 (AD 0x0F)
- Passo (decompile): lê o byte gObjectSlots+0x848 (= slot 0x21 +0x08). Se é 0x40 ('@'): `0x03001800+0xAA++`. Senão: byte +0x848 += 8 e byte +0x849 += 8 (gObjectSlots, bytes, não halfwords).
- Lê/escreve: gObjectSlots+0x848/0x849; 0x03001800+0xAA.
- Para o Lua: porta.
- Confiança: confirmado (decompile). Correção: não são halfwords; é o mesmo passo de fade de 8 de AB 0x0D/AD 0x0D, aqui com teste de 0x40 em vez de 8.

## 0x08023F80 Fight_DispatchOnSubstateAA_AD_SetupSlot43Sfx4E (AD 0x12)
- Passo: igual AB 0x0B (+0x84=0x43; slot registro +0x10C; sfx 0x4E).
- Confiança: confirmado.

## 0x08023FB8 Fight_DispatchOnSubstateAA_AD_WriteBotDisplayRows (AD 0x15)
- Passo: escreve 0x29 e 0 nos bytes +0xC9..0xCC e +0xC4..0xC7 do registro do bot (dois bots, base 0x03000590) com `Fight_ClampDisplayValue1To99`; +0xAA.
- Confiança: confirmado.

## 0x08024078 Fight_DispatchOnSubstateAA_AD_16_SetStepCode2D (AD 0x16)
- Passo (decompile): grava 0x2D em 0x03001800+0xA6; `0x03001800+0xAA++`.
- Lê/escreve: +0xA6, +0xAA. Para o Lua: porta (uma linha).
- Confiança: confirmado (decompile); o sentido de 0x2D é chute (ver nota de +0xA6).

## Despachante AE (índices 0x00, 0x08)

## 0x080240B8 Fight_DispatchOnSubstateAA_AE_00_RosCalcPart (AE 0x00)
- Passo (decompile): idêntico ao AA 0x00 (`Fight_AccumRosPart`, `Fight_ApplyStatusCostIfFlag`, `FUN_0803725C`; se 0: `Fight_IncrementActionUsageCounter_08036270`, `FUN_08037280` e +0xAA = 4; senão +0xA6 = 0x1A e +0xAA++).
- Confiança: confirmado (decompile). Nomes dos alvos: ver AA 0x00.

## 0x0802410C Fight_DispatchOnSubstateAA_AE_08_LoadGfxSetupSlot21 (AE 0x08)
- Passo (decompile): `decompress_and_store(0x084D7A04, 0x06014800)`; `bios_cpufastset(0x0850FDD8, 0x050002E0, 8)`; slot 0x21: halfword +0x00 |= 0x220; bytes +0x08 = 0x40, +0x09 = 0x40; halfword +0x18 = 0; halfword +0x06 = 0x4D00; halfword +0x30 = bot2+0x1742, +0x32 = bot2+0x1744 − 4 (bot2 = byte 0x03001800+0x88); byte +0x24 = 6; se o byte 0x03001800+0xF8 tem bit 0: halfword +0x00 |= 0x1000 (fica 0x1220); `0x03000440+0x86 = 0x3C`; `0x03001800+0xAA++`.
- Lê/escreve: gObjectSlots+0x840..0x872, bot2 em gObjectSlots+bot2*0x100+0x1742/0x1744, 0x03001800+0x88/0xF8/0xAA, 0x03000440+0x86.
- Para o Lua: porta a parte do slot.
- Confiança: confirmado (decompile). Diferenças para AD 0x08: sem contador +0x84, halfword +0x06 = 0x4D00 e bit 0x1000 condicional em +0xF8.

## Nota sobre o byte +0xA6
Os handlers 0x1C (Z 0x0F), 0x1D (AA 0x0B), 0x2B (AC 0x0F), 0x2C (AB 0x0F) e 0x2D (AD 0x16) escrevem
o byte +0xA6 do bloco 0x03001800 e avançam +0xAA. Também 0x1A (ROS falhou, AA/AB/AC/AD/AE 0x00) e 0x39 (AB/AD 0x00,
sem técnica 16). Provável: são o código de passo/efeito que o resto do combate lê. Ainda não conferido quem lê +0xA6.

Pendente redo: nenhum endereço de "NÃO DEFINIDO" ficou sem decompile. Restam só "chute" nos significados
(valores 0x39/0x1C/0x1D/0x2B/0x2C/0x2D, flag +0xBF, 0x4800/0x4D00, 0x58/0x40 de display). Os handlers
"provável" de AA/AB (0x03..0x0F que não são NÃO DEFINIDO) não foram reconferidos nesta rodada.
