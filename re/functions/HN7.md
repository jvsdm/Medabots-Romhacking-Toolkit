# HN7: 18 handlers de batalha (BK 0x09-0x0F, BL 0x00-0x12) + 22 handlers de opcode de evento (0x72-0x87)

Lista: `HN7_lista.txt` (40 entradas, todas lidas no Ghidra por decompile, depois de CreateHandlerFunctions; literais
lidos na ROM EU). Todos os 40 renomeados no Ghidra. Os de evento recebem o mesmo nome de HE4 (para não conflitar no
merge); esta rodada confirma pelo decompilador o que HE4 leu por capstone, com as correções marcadas.
Abreviações: `fi` = 0x03001800 (+0xAA passo, +0xB5 concluído, +0xA6 código de passo/mensagem, +0x87 bot atuante,
+0x60 base do lado, +0xD2[3] marca por bot, +0xD6[3] valor por bot); `tm` = 0x03000440 (+0x84 contador, +0x86 timer);
`os` = 0x03002C30 (registros de slot de 0x40 bytes, os+0x840 = slot 0x21: +2 X, +4 Y, +6 tile/attr2, +8/+9 escala,
+0xA paleta, +0x18, +0x23..+0x28 estado de animação); bot = 0x03000590 + bot*0x100. `E` = 0x030057E0 (estado do script
de evento: +0xC89 pedido pendente, +0xC8A cursor, +0xC8C cursor salvo, +0xC94.. argumentos = 0x03006474).

## Tabelas lidas na ROM
- BK (tabela 0x083AEA30, 18 entradas reais + padding 0x08000840): 00 BH_InitCenterSlotB, 01 BH_GrowCenterSlot,
  02 Shared_WaitTimerOrSkip, 03 0x0802C758, 04 BH_MarkOpponentsPickPart (+3 -> 07; +1 -> 05), 05 0x0802C820,
  06 Shared_SetDone, 07 BH_SpawnSlotsTowardOpponents, 08 BH_MoveSlotsShowPartIcon, 09..0F (abaixo),
  10 ActionFamily_StatusCheckFamily_Handler, 11 Shared_SetDone. BK reusa o sub-roteiro BH 0x3B..0x3F (ícone da
  peça sorteada sobre cada oponente) e termina aplicando o status pendente 0x2B. Técnica de status em uma peça do
  time inimigo (provável).
- BL (tabela 0x083AEA88, 0x20 entradas; idx 0x20+ já são dados): 01 Z_05_FlushPalettesClear84, 02 Z_TickCounterUp,
  0A Z_TickCounterDown, 0C/0x16/0x1B Shared_SetDone, 0F Shared_WaitTimerOrSkip, 11 ActionFamily_StatusCheckFamily,
  13 BL_13_DrawTargetValueNumbers, 15 AR_SpawnPartBreakEffects, 18/19 0x08027798/0x080277DC,
  1A AR_WaitPartBreakSlotsRefreshBots. BL = ataque que só atinge oponentes com bot+0xBD bit 2 (o flag que BB liga,
  ver HN4) e causa dano numa peça aleatória.

## 0x0802DA0C Fight_DispatchOnSubstateAA_BK_09_WaitIconBounceDone
- Passo BK 0x09. Para cada oponente marcado (fi+0xD2+i), anda um quadro da animação de pulo do ícone no slot 0x24+i
  (`FUN_08036AC4(slot,0)`: soma tabela a Y, contador +0x23 até 0x10; 1 = terminou ou slot oculto).
- Todos terminaram: tm+0x86 = 0x3C, +0xAA++ (-> 0x0A, Shared_WaitTimerOrSkip).
- Confiança: confirmado (decompile + literais). Sugestão de nome para 0x08036AC4: `Slot_StepBounceAnim`.

## 0x0802DA60 Fight_DispatchOnSubstateAA_BK_0B_ShowMarkedSlotsAffine
- Passo BK 0x0B. Para cada marcado: `Slot_SetFlagBit15(0x21+i,1)`, os slot |= 0x220 (affine), escala +8/+9 = 0x80,
  +0x18 = 0. +0xAA++ (-> 0x0C).
- Confiança: confirmado.

## 0x0802DAD0 Fight_DispatchOnSubstateAA_BK_0C_ShrinkMarkedSlots
- Passo BK 0x0C. Escala dos slots 0x21+i marcados -= 8 até 0x40. Todos em 0x40: sfx 0x3C, tm+0x86 = 0, +0xAA++ (-> 0x0D).
- Confiança: confirmado.

## 0x0802DB3C Fight_DispatchOnSubstateAA_BK_0D_ShakeSlotsOnTargets
- Passo BK 0x0D. X do slot 0x21+i = X do bot alvo (os+0x1742 + bot*0x100) ±2 conforme bit 2 de tm+0x86 (tremida).
- tm+0x86 conta até 0x3C; então `Tile_ClearSlots(0x21,6)` e +0xAA++ (-> 0x0E).
- Confiança: confirmado.

## 0x0802DBD8 Fight_DispatchOnSubstateAA_BK_0E_SetPendingStatus2B
- Passo BK 0x0E. Para cada oponente marcado: bot+0xC9 (status pendente) = 0x2B; bot+0xCB = `Fight_ClampDisplayValue1To99(fi+0xE8)`;
  bot+0xCC = clamp(fi+0xEC); bot+0xCA = peça (fi+0xD2 & 0x7F: 1 -> 2, 2 -> 3, outra -> 1).
- +0xAA++ (-> 0x0F). Confiança: confirmado (código); significado de 0x2B e +0xCB/+0xCC (força/duração?) é chute.

## 0x0802DC9C Fight_DispatchOnSubstateAA_BK_0F_SetMessage5E
- Passo BK 0x0F. fi+0xA6 = 0x5E (código do texto), +0xAA++ (-> 0x10, StatusCheckFamily). Confiança: confirmado.

## 0x0802DCDC Fight_DispatchOnSubstateAA_BL_00_LoadGfxSpawnSwingSlot
- Passo BL 0x00. LZ77 0x084E1A0C -> VRAM 0x06014800; CpuFastSet paleta 0x08510458 -> 0x050002E0 (paleta OBJ 7, 8 words).
- Slot 0x21: attr 0xC404, Y = 0xE0 (fora da tela), paleta 7, +0xB = 0xF0, +0x1C = 0; X = 0xB8 (bit 0 de fi+0xF8 = 0) ou 0x40.
- `Slot_SetupFromRecordList([0x083B6100+0x1A4], 0x21)`; +0xAA++ (-> 0x01). Confiança: confirmado.

## 0x0802DD98 Fight_DispatchOnSubstateAA_BL_03_SlideSlotIn
- Passo BL 0x03. `Anim_RunSlotScript`; Y do slot 0x21 += 1 (& 0xFF, de 0xE0 dá a volta) até 0x40.
- Chegou: tm+0x86 = 0, os+0x865 = 0x14, zera +0x863/+0x864/+0x868 (índice/contador da tabela de balanço); +0xAA++ (-> 0x04).
- Confiança: confirmado.

## 0x0802DE1C Fight_DispatchOnSubstateAA_BL_04_SwingSlotByTable
- Passo BL 0x04 (laço). A cada 32 quadros (tm+0x86 & 0x1F == 0) com índice os+0x863 < 8: +0xAA++ (-> 0x05, flash).
- Senão: tm+0x86++, anima o slot, os+0x864 = 4, conta os+0x868; ao zerar, lê o par (ângulo, duração) da tabela
  0x083AEB08 no índice +0x863 e grava o ângulo em +0x865 (espelhado 0x20-a se lado = 1). Fim (0xFF): limpa o slot 0x21,
  +0xAA = 0x0A (Z_TickCounterDown).
- Tabela: (14,10) (1C,10) (04,10) (0C,20) (04,10) (1C,10) (14,10) (00,3C) FF. Confiança: confirmado.

## 0x0802DF18 Fight_DispatchOnSubstateAA_BL_05_SetFlashTileSfx
- Passo BL 0x05. os+0x846 (slot 0x21 +6, tile/attr2) = 0x7F02, sfx 0x38, +0xAA++ (-> 0x06). Confiança: confirmado.

## 0x0802DF48 Fight_DispatchOnSubstateAA_BL_06_BlendPaletteToWhite
- Passo BL 0x06. `FUN_0803F684(0x7FFF, tm+0x84, 0xFFFF)` + `FUN_0803F998(0x7FFF, tm+0x84, 0xFF00)` (mistura de
  paleta para branco, provável); tm+0x84 += 2 até 0x20, então +0xAA++ (-> 0x07).
- Confiança: confirmado no fluxo; efeito (flash branco) provável.

## 0x0802DF98 Fight_DispatchOnSubstateAA_BL_07_BlendPaletteBack
- Passo BL 0x07. Mesmas chamadas com cor 0x1084 (cinza); tm+0x84 -= 2 até 0x10, então +0xAA++ (-> 0x08).
- Confiança: confirmado no fluxo; efeito provável.

## 0x0802DFE8 Fight_DispatchOnSubstateAA_BL_08_ResetSlotLoopToSwing
- Passo BL 0x08. `Slot_SetupFromRecordList([0x083B6100+0x1A4], 0x21)` (restaura o tile) e +0xAA = 0x04 (volta ao balanço).
- Confiança: confirmado.

## 0x0802E010 Fight_DispatchOnSubstateAA_BL_0B_MarkOpponentsWithFlagBD4
- Passo BL 0x0B. `FightPreview_ClearPartCounters3`; fi+0x60 = `Fight_GetOpponentSideBaseIndex(fi+0x87)`.
- Marca fi+0xD2+i = 1 para oponente ativo (+0x40), cabeça viva (+0x59) e bot+0xBD bit 2. Algum: +0xAA += 2 (-> 0x0D);
  nenhum: +0xAA++ (-> 0x0C, Shared_SetDone). Confiança: confirmado.

## 0x0802E098 Fight_DispatchOnSubstateAA_BL_0D_SpawnEffectOnTargets
- Passo BL 0x0D. Para cada marcado, monta o slot 0x21+i sobre o bot (X = os+0x1742, Y = os+0x1744 - 4, escala 0x80,
  paleta 7, attr fixos do pool), sfx 0x54, +0xAA++ (-> 0x0E). Confiança: confirmado.

## 0x0802E140 Fight_DispatchOnSubstateAA_BL_0E_ShrinkEffectSlots
- Passo BL 0x0E. Escala dos slots marcados -= 8; quando algum chega a 0x40: tm+0x86 = 0x3C, +0xAA++ (-> 0x0F espera).
- Difere de BK_0C: basta UM slot chegar (bool), não todos. Confiança: confirmado.

## 0x0802E1A4 Fight_DispatchOnSubstateAA_BL_10_ClearSlotsSetMessage43
- Passo BL 0x10. `Tile_ClearSlots(0x21,3)`, fi+0xA6 = 0x43, +0xAA++ (-> 0x11 StatusCheckFamily). Confiança: confirmado.

## 0x0802E1C8 Fight_DispatchOnSubstateAA_BL_12_DamageRandomPartOfTargets
- Passo BL 0x12. Para cada marcado: peça = `Fight_RollStatusRowRandomEntry(bot)`; `Fight_ApplyDamageToPart(bot, peça, 200)`
  (dano resultante em fi+0xC8); fi+0xD2 = peça | (fi+0xD1 << 7); fi+0xD6[i] = dano; armadura bot+0x59+peça*0x18 -= dano;
  `Fight_BackupHalvedStatsIfFlagClear`; cabeça zerada -> `Fight_QueueHeadBrokenBotForResolution`.
- +0xAA++ (-> 0x13, DrawTargetValueNumbers). Confiança: confirmado.

## Opcodes de evento 0x72, 0x73, 0x76, 0x7A, 0x82, 0x87 (pedido pendente puro)
- Corpo idêntico: só `E+0xC89 = 1` (0 bytes de argumento). O efeito depende do executor de cena, que lê o opcode depois.
- Nomes (HE4): Run_Scene_Command_02 / _08, Start_Medabot_Link, Run_Scene_Command_11, Execute_Batched_Object_Command_Block,
  Execute_Event_Object_Facing_Cycle_Block. Confiança: confirmado (decompile). Corrige HE4: 0x82/0x87 gravam o pedido
  pendente (não "estado" genérico).

## Opcodes 0x74, 0x75, 0x77, 0x7C (no-op)
- `bx lr` (bytes 70 47). 0 argumentos. Confiança: confirmado (ROM).

## 0x0804D39C Event_Op78_Run_Scene_Command_10_If_At_Least_Three_Complete_Part_Sets
- E+0xC8C = cursor; lê 2 bytes; 0x03006D38 = arg0. `FUN_08047F9C()` conta, em 9 registros de 8 bytes a partir de
  0x030017A0, os que têm byte 0 == 3 (conjuntos completos, provável). < 3: salto (cursor = salvo + arg1); >= 3: pedido pendente.
- Confiança: confirmado (código); significado de 0x030017A0 provável. Sugestão: `Parts_CountCompleteSets`.

## 0x0804D400 Event_Op79_Run_Scene_Command_12
- Lê 1 byte para 0x03006D39 (0x03006D38+1) e liga o pedido pendente. Confiança: confirmado.

## 0x0804D438 Event_Op7B_Jump_If_No_Spare_Part
- Salva o cursor, lê 3 bytes; `FUN_08047D9C(arg0, arg1)`; se o byte alto do resultado (quantidade) = 0, cursor = salvo + arg2.
- Confiança: confirmado (já tinha nome no Ghidra).

## 0x0804D490 Event_Op7D
- Lê 1 byte e grava em E+0xC32 (0x03006412). Sem pedido pendente. Confiança: confirmado (uso do campo: desconhecido).

## 0x0804D4AC Event_Op7E_Move_All_Tracked_Actors_C / 0x0804D4D4 Event_Op7F
- Os dois: lê 1 byte para E+0xC94 e liga o pedido pendente. Corpo idêntico; diferença só no executor. Confiança: confirmado.

## 0x0804D4FC Event_Op80_Begin_Batched_Object_Command_Block
- Zera 0x030064A0+0x52 (contador) e limpa +2(8), +0xA(8), +0x12(0x40), +0x52(1), +0x53(1), +0x54(8) com `FUN_08049714`.
- Depois lê o PRÓXIMO byte do script como opcode em E+0xC92, zera E+0xC89 e E+0xC93 e chama `0x083D222C[op]` (encadeia o
  opcode seguinte no mesmo quadro). Confiança: confirmado.

## 0x0804D580 Event_Op81_Queue_Batched_Object_Command
- Lê 8 bytes; copia para 0x030064A0+0x12 + n*8 (n = contador +0x52, depois ++) via `FUN_080497C0`; encadeia o próximo
  opcode como 0x80 (E+0xC92 = op, zera +0xC89/+0xC93). Confiança: confirmado. Corrige HE4 (o encadeamento não estava anotado).

## 0x0804D5F0 Event_Op83_Set_Object_Render_Mode
- Lê 2 bytes: arg0 = flags (bit 4 = modo "campo 0B", bit 6 = jogador/obj 0x2F, bit 5 = objeto do registro E+0x5DE+idx*8
  com idx = E+0xCB4 e id (& 0xFFF) = E+0xC8E, senão nibble baixo = slot rastreado); arg1 = valor.
- Bit 4: valor 0 -> `FUN_080487C8(slot)`, senão `Map_SetObjectSlotField0B`/`Map_SetTrackedObjectField0B`. Sem bit 4:
  `Map_SetObjectAttrBits10` ou `Map_SetTrackedObjectTileAttr`. Confiança: confirmado (fluxo); semântica do bit 5 provável.

## 0x0804D758 Event_Op84_Set_Map_Scene_Variant_When_Player_Faces_Up
- Só lê 1 byte (para E+0xC94) e liga o pedido pendente; a condição/variante é do executor. Confiança: confirmado.

## 0x0804D778 Event_Op85 / 0x0804D7AC Event_Op86
- 0x85: zera 0x030064A0+0x52 e limpa +2(8), +0xA(8), +0x12(0x40). Não encadeia (diferente de 0x80).
- 0x86: lê 8 bytes e enfileira em +0x12 + n*8 (contador +0x52 ++). Não encadeia. Confiança: confirmado.
