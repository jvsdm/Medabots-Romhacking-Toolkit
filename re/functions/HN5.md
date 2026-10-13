# HN5: handlers de Fight_DispatchOnSubstateAA_BG (0x0A-0x0C), _BH (32) e 5 handlers compartilhados

Lista: `HN5_lista.txt` (40 entradas, todas lidas). Bloco `fi` = 0x03001800 (+0xAA passo, +0x86 timer, +0xA6 código de
passo, +0x87 bot atuante, +0x60 base do lado (0 ou 3), +0xD2[3] marca por bot, +0xD6[3] valor por bot (short), +0xEC valor
do efeito). Bloco do bot = 0x03000590 + bot*0x100 (+0x40 ativo, +0x59 armadura da cabeça, +0xBD flags).
Slots de efeito 0x21..0x26 (registro de 0x40 bytes; base+0x840 = slot 0x21). Sprites do bot = slots 0x5A+bot*4..0x5D.
Ghidra: 40 renomeados (35 com nome novo `_BG_`, `_BH_` ou `_Shared_`; 5 com o nome que já estava no mtbe.csv, que vence).
Padrão: passo de "efeito" = descomprime + CpuFastSet + monta slots + sfx, +0xAA++; passo final de cada sub-roteiro
grava um código em +0xA6 (0x42, 0x59, 0x5B, 0x5C) e +0xAA++. Índices ausentes na lista (0x00, 0x06-0x0F, 0x12, 0x16,
0x1D-0x1F, 0x23, 0x2D-0x36, 0x38-0x3A, 0x3C-0x3D) são handlers compartilhados documentados em outras listas.

## 0x0802C1A4 Fight_DispatchOnSubstateAA_BG_SquashSlotsThenClear
- Passo BG 0x0A. Para cada aliado marcado (+0xD2) e cada oponente com +0xD6 != 0: byte de escala do slot -8, byte seguinte +8.
- Quando a escala do primeiro slot aliado chega a 8: `Tile_ClearSlots(0x21,6)` e +0xAA++ (-> 0x0B).
- Memória: lê +0x87, +0xD2, +0xD6; escreve +0x60, registros de slot, +0xAA.
- Confiança: confirmado (decompile).

## 0x0802C24C Fight_DispatchOnSubstateAA_BG_TransferGauge
- Passo BG 0x0B. `Fight_AddByteClampToMax(fi+0x28 + fi[0x89]*8, fi.EC, 0xA0)` e `Fight_SubClampZeroByte(fi+0x28 + fi[0x8A]*8, fi.EC)`:
  passa o valor +0xEC do gauge de um lado (+0x8A) para o outro (+0x89), teto 0xA0 (gauge: ver HA6).
- Memória: lê +0x89, +0x8A, +0xEC; escreve gauge em +0x28+i*8, +0xAA (-> 0x0C).
- Confiança: confirmado (código); "gauge = medaforce" é provável.

## 0x0802C294 Fight_DispatchOnSubstateAA_BG_SetStepCode5B
- Passo BG 0x0C. Grava +0xA6 = 0x5B e +0xAA++ (fim do sub-roteiro BG, segue para o passo genérico de +0xA6).
- Confiança: confirmado.

## 0x0802C2D4 Fight_DispatchOnSubstateAA_BH_SpawnSideSlots
- Passo BH 0x01. Descomprime gráfico, CpuFastSet de 8 palavras; slots 0x21..0x23 sobre os aliados marcados (+0xD2) e
  0x24..0x26 sobre os oponentes com +0xD6 != 0 (posição = sprite do bot +0x1742/+0x1744 - 4), `Slot_SetupFromRecordList`.
- sfx 0x4D; +0xAA++ (-> 0x02).
- Confiança: confirmado.

## 0x0802C440 Fight_DispatchOnSubstateAA_BH_RunSideSlotsUntilEnd
- Passo BH 0x02. `Anim_RunSlotScript` nos slots 0x21..0x26 ativos; quando o roteiro termina (byte 0xFF):
  `Tile_ClearSlots(0x21,6)`, zera contador +0x84 (outro bloco) e +0xAA++ (-> 0x03).
- Confiança: confirmado.

## 0x0802C4F8 Fight_DispatchOnSubstateAA_BH_BlinkAllyPreview
- Passo BH 0x03. Contador +0x84++; `FightPreview_SetGroupActive4(bot, contador, 2)` nos aliados marcados.
- Em 0x3C (60 quadros): `FightPreview_ShowGroup4` em cada aliado marcado e +0xAA++ (-> 0x04).
- Confiança: confirmado.

## 0x0802C588 Fight_DispatchOnSubstateAA_BH_BoostAlliesDrainOpponents
- Passo BH 0x04. Aliados marcados: `Fight_AddByteClampToMax(bot+0x01, fi.EC, 0x78)`; oponentes com +0xD6: `Fight_SubClampZeroByte(bot+0x01, fi.EC)`.
- Byte +0x01 do bloco do bot (teto 0x78 = 120) parece ser a carga/barra de ação (provável). +0xAA++ (-> 0x05).
- Confiança: confirmado (código); significado do byte provável.

## 0x0802C62C Fight_DispatchOnSubstateAA_BH_SetStepCode5C
- Passo BH 0x05. +0xA6 = 0x5C e +0xAA++. Fim do sub-roteiro 0x01-0x05.
- Confiança: confirmado.

## 0x0802C66C Fight_DispatchOnSubstateAA_BH_InitCenterSlot
- Passo BH 0x10. Descomprime + CpuFastSet; slot 0x21 no centro (x 0x78, y 0x40), escala 4x4, atributo 32000 (0x7D00); sfx 0x6E.
- Também é o alvo do salto de 0x2B quando uma cabeça quebra. +0xAA++ (-> 0x11).
- Confiança: confirmado.

## 0x0802C714 Fight_DispatchOnSubstateAA_BH_GrowCenterSlot
- Passo BH 0x11. Escala do slot 0x21 +4 (dois bytes) por quadro até 0x40; então timer +0x86 = 0x3C e +0xAA++ (-> 0x12, espera compartilhada).
- Confiança: confirmado.

## 0x0802C758 Fight_DispatchOnSubstateAA_BH_ShrinkCenterSlot
- Passo BH 0x13. Escala -4 por quadro até 0x20; então +0xAA++ (-> 0x14).
- Confiança: confirmado.

## 0x0802C790 Fight_DispatchOnSubstateAA_BH_MarkEligibleAllies
- Passo BH 0x14. `FightPreview_ClearPartCounters3`; para os 3 aliados: se ativo (+0x40), cabeça viva (+0x59) e
  `Fight_HasNoTechnique16Part`, marca +0xD2 = 1.
- Algum marcado: +0xAA += 3 (-> 0x17, efeito); nenhum: +0xAA++ (-> 0x15, encerra).
- Confiança: confirmado.

## 0x0802C820 Fight_DispatchOnSubstateAA_BH_ShrinkCenterSlotThenClear
- Passo BH 0x15 (ramo "ninguém elegível"). Escala -4 até 4; então `Tile_ClearSlots(0x21,1)` e +0xAA++ (-> 0x16).
- Confiança: confirmado.

## 0x0802C864 Fight_DispatchOnSubstateAA_BH_SpawnSlotsTowardAllies
- Passo BH 0x17. Limpa slot 0x21, descomprime; para cada aliado marcado: slot 0x21+i no centro (0x78,0x40) com
  alvo = sprite do bot (+0x1742/+0x1744-4), `Slot_SetupFromRecordList`. +0xAA++ (-> 0x18).
- Confiança: confirmado.

## 0x0802C960 Fight_DispatchOnSubstateAA_BH_MoveSlotsToAllies
- Passo BH 0x18. Roda roteiro e gira (+/-0x40 conforme +0xF8 bit 0) os slots marcados; `Fight_StepSlotTowardTarget`.
- Todos chegaram: zera 3 halfwords, `Slot_SetupFromRecordList` nos slots 0x21-0x23 (lista +0x10C), sfx 0x4E, +0xAA++ (-> 0x19).
- Confiança: confirmado.

## 0x0802CA5C Fight_DispatchOnSubstateAA_BH_RunMarkedSlotsUntilEnd
- Passo BH 0x19. `Anim_RunSlotScript` nos slots marcados (+0xD2); quando algum roteiro chega a 0xFF, +0xAA++ (-> 0x1A).
- Confiança: confirmado (DAT_0802CAC0 = fi+0xD2, provável).

## 0x0802CAD4 Fight_DispatchOnSubstateAA_BH_ShrinkMarkedSlotsThenClear
- Passo BH 0x1A. Escala dos slots marcados -4 até 4; então `Tile_ClearSlots(0x21,3)` e +0xAA++ (-> 0x1B).
- Confiança: confirmado.

## 0x0802CB34 Fight_CommitBoostChargeSecondaryEffectToMarkedTargets (nome do mtbe)
- Passo BH 0x1B. Para cada aliado marcado: bot+0xC4 = 0x26 (efeito secundário), +0xC5 = 0,
  +0xC6 = `FUN_08036E54(fi.E8)`, +0xC7 = `FUN_08036E54(fi.EC)`. +0xAA++ (-> 0x1C).
- Confiança: confirmado.

## 0x0802CBD0 Fight_DispatchOnSubstateAA_BH_SetStepCode59
- Passo BH 0x1C. +0xA6 = 0x59 e +0xAA++. Fim do sub-roteiro 0x10-0x1C (bônus nos aliados).
- Confiança: confirmado.

## 0x0802CC10 Fight_DispatchOnSubstateAA_BH_MarkActiveOpponents
- Passo BH 0x20. `FightPreview_ClearPartCounters3`; oponentes ativos (+0x40) com cabeça viva (+0x59): +0xD2 = 1. +0xAA++ (-> 0x21).
- Confiança: confirmado.

## 0x0802CC74 Fight_DispatchOnSubstateAA_BH_SpawnSlotsBelowOpponents
- Passo BH 0x21. Descomprime; slot 0x21+i por oponente marcado em (0x78, 0xF0) (abaixo da tela), tile 0x4D00, espelho se +0xF8 bit 0. sfx 0x61, +0xAA++ (-> 0x22).
- Confiança: confirmado.

## 0x0802CD48 Fight_DispatchOnSubstateAA_BH_RaiseSlotsOntoOpponents
- Passo BH 0x22. `Fight_StepSlotCoordUpInRange` até a faixa do sprite do oponente; ao chegar, prende o slot na posição do bot.
- Todos chegaram: timer +0x86 = 0x3C e +0xAA++ (-> 0x23, espera compartilhada).
- Confiança: confirmado.

## 0x0802CE08 Fight_DispatchOnSubstateAA_BH_WaitMarkedSlotsAtTarget
- Passo BH 0x24. `Fight_StepSlotTowardTarget` nos slots marcados; todos chegaram: +0xAA++ (-> 0x25).
- Confiança: confirmado.

## 0x0802CE4C Fight_DispatchOnSubstateAA_BH_DetachOpponentSprites
- Passo BH 0x25. Copia os 4 registros de sprite do oponente (+0x1680..+0x1740) para os slots de trabalho (+0x900..),
  `Slot_ClearAttrMask(0x24+i*4,4)`, zera 4 bytes do bot, `Fight_ShowObjectSlot(0x72+bot)`. sfx 0x34, +0xAA++ (-> 0x26).
- Confiança: confirmado.

## 0x0802CFAC Fight_DispatchOnSubstateAA_BH_DropOpponentSprites
- Passo BH 0x26. Move/apaga o slot de efeito; `Fight_StepSlotAnimOffsetTable` nos 4 sprites do bot (0x5A..0x5D+bot*4);
  ao fim da tabela marca bit 0x2000 e desloca y. Todos terminaram: `Fight_HideObjectSlot(0x72+bot)`, sfx 0x41, +0xAA++ (-> 0x27).
- Confiança: confirmado (leitura visual "derrubar" é provável).

## 0x0802D198 Fight_DispatchOnSubstateAA_BH_SpawnDebrisSlots
- Passo BH 0x27. Para cada marcado, 4 slots (+0xC00+i*0x100) na posição do sprite do bot (+0x1682/+0x1684+4), tipo 6,
  parâmetro de uma tabela de 4 bytes. Zera contador +0x84 e +0xAA++ (-> 0x28).
- Confiança: confirmado.

## 0x0802D270 Fight_DispatchOnSubstateAA_BH_AnimDebrisRestoreSprites
- Passo BH 0x28. `Anim_AdvanceSlotTileOffset` nos 4 slots de cada marcado; quando o contador +0x84 = 0x10: copia de volta
  os registros (+0xD80.. -> sprites do bot), liga bit 15 (visível), `Tile_ClearSlots(0x21,0x1B)` e +0xAA++ (-> 0x29).
- Confiança: confirmado.

## 0x0802D3D4 Fight_DispatchOnSubstateAA_BH_ApplyHeadDamageShowNumbers
- Passo BH 0x29. Para cada oponente marcado: `Fight_ApplyDamageToPart(bot, 0 (cabeça), fi.EC)` (ou 1 se bot+0xBD bit 2);
  copia o dano (fi+0xC8) para +0xD6[i]; `Fight_WriteNumberTiles` + `Fight_BuildBotOamEntriesAt`. sfx 0x5B, +0x84 = 0, +0xAA++ (-> 0x2A).
- Confiança: confirmado.

## 0x0802D4D8 Fight_DispatchOnSubstateAA_BH_AnimDamageRows
- Passo BH 0x2A. `Fight_StepRowAnimation` dos números; +0x84++; pula para 0x78 após 0x32 se +0x89 == 0 e flag bit 0 (avanço rápido).
- Em 0x78: `Tile_ClearSlots` (4 por bot marcado) e +0xAA++ (-> 0x2B).
- Confiança: confirmado.

## 0x0802D5BC Fight_DispatchOnSubstateAA_BH_CommitArmorCheckHeads
- Passo BH 0x2B. `Fight_SubtractPartArmorClampZero(bot, 0, +0xD6[i])`; cabeça a zero (+0x59 == 0): `Fight_QueueHeadBrokenBotForResolution`,
  +0xD2 = 0x80; senão liga bot+0xBD bit 0 (se bit 2 desligado).
- Alguma cabeça quebrou: +0xAA = 0x10 (salto absoluto; literal provável = fi); senão +0xAA++ (-> 0x2C).
- Confiança: confirmado (código); destino 0x10 depende do literal DAT_0802D650 (provável fi).

## 0x0802D670 Fight_DispatchOnSubstateAA_BH_SetStepCode42
- Passo BH 0x2C. +0xA6 = 0x42 e +0xAA++. Fim do sub-roteiro 0x20-0x2C (dano na cabeça dos oponentes).
- Confiança: confirmado.

## 0x0802D6B0 Fight_DispatchOnSubstateAA_BH_InitCenterSlotB
- Passo BH 0x37. Igual a 0x10 (slot central 0x78,0x40, escala 4, sfx 0x6E) com atributo 0x7E00. +0xAA++ (-> 0x38).
- Confiança: confirmado.

## 0x0802D758 Fight_DispatchOnSubstateAA_BH_MarkOpponentsPickPart
- Passo BH 0x3B. Oponentes ativos, cabeça viva e `Fight_HasNoTechnique16Part`: +0xD2 = 0x80 | `Fight_PickRandomIntactPart(bot)`.
- Algum marcado: +0xAA += 3 (-> 0x3E); nenhum: +0xAA++ (-> 0x3C).
- Confiança: confirmado.

## 0x0802D7EC Fight_DispatchOnSubstateAA_BH_SpawnSlotsTowardOpponents
- Passo BH 0x3E. Como 0x17, mas sobre os oponentes marcados (tile 0x4E00, sem `Slot_SetupFromRecordList`). +0xAA++ (-> 0x3F).
- Confiança: confirmado.

## 0x0802D8D0 Fight_DispatchOnSubstateAA_BH_MoveSlotsShowPartIcon
- Passo BH 0x3F. Gira e move os slots até o alvo; todos chegaram: `Slot_ClearAttrMask`, monta um ícone por bot cujo tile
  depende da parte sorteada (+0xD2 & 0x7F: 1, 2 ou outra). sfx 0x6A, +0xAA++ (-> 0x40).
- Confiança: confirmado (parte 1/2 = braços, provável).

## 0x080305E0 ActionFamily_StatusCheckFamily_Handler (nome do mtbe)
- Índice 0x01 de ~48 despachantes (A, AA..BH, C..Z). Chama `Fight_RunSubstateDispatch(fi.A6)`; retorno != 0: +0xAA++.
- É o passo genérico que roda o código gravado em +0xA6 pelos passos "SetStepCode".
- Confiança: confirmado.

## 0x08030608 ActionFamily_TrapFamily_Handler (nome do mtbe)
- Índice 0x01 de 25 despachantes (B, AA..AN, C, N..Z). Chama `Fight_RunSubstateDispatchB()`; retorno != 0: +0xAA++.
- Confiança: confirmado.

## 0x08030624 ActionFamily_HealChange_Handler (nome do mtbe)
- Índice 0x05 de H, L, P, R, AO, AP, AQ. Espera o timer +0x86 (decrementa); pula se +0x89 == 0 e flag bit 0 (avanço rápido).
- Ao fim: `Tile_ClearSlots(10,1)` e +0xAA++.
- Confiança: confirmado.

## 0x08030674 Fight_DispatchOnSubstateAA_Shared_WaitTimerOrSkip
- Índice 0x0E de 19 despachantes (T, U, Z, AA..BH). Igual a 0x08030624 sem limpar slot: espera +0x86 ou pula; +0xAA++.
- Confiança: confirmado.

## 0x080306B8 ActionFamily_Change_Handler (nome do mtbe)
- Índice 0x0F de H, I, P, R, AO, AP, AQ. Avanço rápido (+0x89 == 0 e flag bit 0): limpa slots 10..0x1D visíveis e +0xAA++.
- Depois testa o timer +0x86: zero -> limpa de novo e +0xAA++; senão decrementa.
- Observação: no avanço rápido com timer já em 0, +0xAA sobe 2 no mesmo quadro (possível pulo de passo).
- Confiança: confirmado.
