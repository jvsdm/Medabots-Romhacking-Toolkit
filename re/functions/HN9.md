# HN9: handlers das tabelas BP (1), BQ (10) e BR (29) dos despachantes Fight_DispatchOnSubstateAA_*

Lista: `HN9_lista.txt` (40 entradas). Todas lidas no Ghidra (decompile, depois de CreateHandlerFunctions) e
RENOMEADAS no Ghidra. Literais do pool lidos na ROM EU. Índices conferidos nas tabelas da ROM: BP `0x083AEC04`,
BQ `0x083AEC58`, BR `0x083AEC98` (todas as 40 batem).
Abreviações: `F` = fight_information `0x03001800` (+0x87 = bot ativo, +0xA6 = código de passo/mensagem, +0xA7, +0xA8 =
action_script_state, +0xAA = passo, +0xF8 bit0 = lado); `B(n)` = bloco do bot `0x03000590 + n*0x100` (+0xC0 =
flags de reação, +0xC1 = medaforce atual, +0xDD = peça escolhida 1..3, >=4 = medaforce); `S` = gObjectSlots
`0x03002C30` (slot k em `S+k*0x40`, +2/+4 = posição X/Y provável, +8/+9 = escala do affine, posição do sprite do
bot em `S+0x1742/0x1744 + bot*0x100`); `T` = `0x03000440` (+0x86 = timer do passo); `GS` = gamestate_subroutine
`0x0300137C` (u32); `GM` = gamestate_menu `0x03002B8C`.

## Achados gerais

- Quem chama BP/BQ/BR: stubs em `0x08034E40`/`0x08034E5C`/`0x08034E78` (função desde o sync de 2026-10-14: ActionScript_Op68_CallBP, ActionScript_Op69_CallBQ, ActionScript_Op6A_CallBR), entradas 0xA4/0xA5/0xA6
  da tabela de 188 ponteiros `0x083AF090` (literal em `0x0803335C`, perto de Fight_BeginPartActionValidation
  0x08033318). Provável: tabela indexada pelo id da ação. Confiança: provável (ponteiros lidos na ROM, o uso não foi lido).
- **BR 0x10..0x5x NÃO é passo +0xAA.** Esses handlers avançam `GS` (u32), não `F+0xAA`. Existe um 2º despachante
  em `0x08030860` (função desde o sync de 2026-10-14 (nome: Fight_SceneMain); `menu_list[8]`, ponteiro em `0x083AC2E8`): chama Battle_CheckExitShortcut
  0x0803EED8, Overlay_ClearTimerIfFlag 0x0803E780 e Fight_DrawActiveBotsOverlay 0x080195C0, depois `table[GS]` via
  0x0807CCF0 com tabela `0x083AECD8` = BR+0x40. Então BR índice i (i >= 0x10) = cena 8, subestado `GS = i-0x10`.
  A cena 8 é a validação da ação escolhida (peça ou medaforce) antes da execução; ela termina com `GM = 9, GS = 0`
  (cena 9, provável execução do script de ação +0xA8). Os nomes seguem o pedido (`_BR_<índice>`), com essa ressalva.
  Confiança: confirmado (bytes do despachante e literais lidos).
- Handlers compartilhados: `0x08030D3C` (BR 0x21, 0x30, 0x41), `0x08030E54` (BR 0x31, 0x42, 0x48), `0x08030C94`
  (BR 0x23, 0x28, 0x32, 0x43). Comuns de fim: BQ 0x07 e BR 0x0B.. = `0x08030674`/`0x080305E0`/`0x0803080C` (fora desta lista).
- BQ e BR 0x00..0x08 são duas animações de efeito de status quase iguais (BQ com 2 sprites, BR com 4). O fim grava
  um bit em `B(F+0x87)+0xC0` (BQ bit0, BR bit2) e um código de passo em `F+0xA6` (BQ 0x4F, BR 0x6A). Qual
  status cada uma aplica não foi confirmado.

## 0x0802F8E4 Fight_DispatchOnSubstateAA_BP_08_ApplyTeamPendingDamage
- Passo: BP 0x08. Para os 3 bots do time (`F+0x60 + i`): se `F+0xD2[i] != 0` e o dano `F+0xD6[i]` (u16) != 0, chama
  Fight_ApplyDamageToPart(bot, 3, dano). Depois marca o resultado com `F+0xD1`<<7, copia `F+0xC8` e subtrai da armadura (+0x59) da peça.
- Chama Fight_BackupHalvedStatsIfFlagClear(bot). Próximo: +0xAA++ (0x09).
- Para o Lua: dano em área (3 bots). Confiança: confirmado (decompile); o significado de +0xD1/+0xC8 é provável.

## 0x0802F9B8 Fight_DispatchOnSubstateAA_BQ_00_LoadEffectTwoSlots
- Passo: BQ 0x00. Descompacta gfx `0x084E3138` -> VRAM `0x06014800`, paletas `0x085104B8`->`0x050002C0` e `0x084E4658`->`0x050002E0`.
- Monta os slots 0x21 e 0x22 (attr `0xC404`) acima do bot `F+0x87` (Y-0x1C, X±0x18 conforme o lado; slot 0x22 espelhado).
- `T+0x86 = 0`, SFX 0x2B. Próximo: 0x01. Confiança: confirmado.

## 0x0802FB74 Fight_DispatchOnSubstateAA_BQ_01_SlideSlotsX
- Passo: BQ 0x01. Por 0x18 frames move X dos slots 0x21/0x22 1 px por frame em sentidos opostos (sentido pelo lado).
- Timer `T+0x86`; no fim zera o timer. Próximo: 0x02. Confiança: confirmado.

## 0x0802FBF4 Fight_DispatchOnSubstateAA_BQ_02_LowerSlotsY
- Passo: BQ 0x02. Por 12 frames soma 2 ao Y dos slots 0x21/0x22 (`S+0x844`, `S+0x884`). Próximo: 0x03. Confiança: confirmado.

## 0x0802FC3C Fight_DispatchOnSubstateAA_BQ_03_SetupScaledIcon
- Passo: BQ 0x03. Tile_ClearSlots(0x21,2); recarrega a paleta; slot 0x21 com attr `0xC624` (affine) na posição do bot (Y-4),
  escala `S+0x848/0x849 = 0x80`. Próximo: 0x04. Confiança: confirmado.

## 0x0802FD08 Fight_DispatchOnSubstateAA_BQ_04_ShrinkIcon
- Passo: BQ 0x04. Escala do slot 0x21 cai 8 por frame até 0x40; ao chegar, SFX 0x54. Próximo: 0x05. Confiança: confirmado.

## 0x0802FD4C Fight_DispatchOnSubstateAA_BQ_05_GrowIcon
- Passo: BQ 0x05. Escala sobe 4 por frame até 0x60. Próximo: 0x06. Confiança: confirmado.

## 0x0802FD84 Fight_DispatchOnSubstateAA_BQ_06_ShrinkIconSetTimer
- Passo: BQ 0x06. Escala cai 4 por frame até 0x40; então `T+0x86 = 0x3C`. Próximo: 0x07 (`0x08030674`, espera comum). Confiança: confirmado.

## 0x0802FDC8 Fight_DispatchOnSubstateAA_BQ_08_ShrinkIconAndClear
- Passo: BQ 0x08. Escala cai 4 por frame até 4; então Tile_ClearSlots(0x21,1). Próximo: 0x09. Confiança: confirmado.

## 0x0802FE0C Fight_DispatchOnSubstateAA_BQ_09_SetReactionFlagBit0
- Passo: BQ 0x09. `B(F+0x87)+0xC0 |= 1` (efeito real do passo). Próximo: 0x0A. Confiança: confirmado.

## 0x0802FE38 Fight_DispatchOnSubstateAA_BQ_0A_SetStepCode4F
- Passo: BQ 0x0A. `F+0xA6 = 0x4F` (código de mensagem/passo usado depois). Próximo: 0x0B (`0x080305E0`, comum). Confiança: confirmado.

## 0x0802FE78 Fight_DispatchOnSubstateAA_BR_00_LoadEffectFourSlots
- Passo: BR 0x00. Igual ao BQ 0x00, com gfx `0x084E3444` e 4 slots (0x21..0x24, dois de cada lado do bot; 0x23/0x24 espelhados no lado 1).
- `T+0x86 = 0`, SFX 0x2B. Próximo: 0x01. Confiança: confirmado.

## 0x08030180 Fight_DispatchOnSubstateAA_BR_01_SlideSlotsX
- Passo: BR 0x01. Por 0x18 frames, X dos 4 slots ±1 (pares em sentidos opostos). Próximo: 0x02. Confiança: confirmado.

## 0x08030238 Fight_DispatchOnSubstateAA_BR_02_LowerSlotsY
- Passo: BR 0x02. Por 12 frames, Y dos 4 slots +2. Próximo: 0x03. Confiança: confirmado.

## 0x0803029C Fight_DispatchOnSubstateAA_BR_03_SetupSlot25Anim
- Passo: BR 0x03. Slot 0x25 (`S+0x940`, attr 0xC404, espelhado no lado 1) na posição do bot, Slot_SetupFromRecordList(`[0x083B6100+0x1BC]`, 0x25), SFX 0x30.
- Próximo: 0x04. Confiança: confirmado.

## 0x08030354 Fight_DispatchOnSubstateAA_BR_04_WaitSlot25Anim
- Passo: BR 0x04. Anim_RunSlotScript no slot 0x25 até `S+0x962 == 0xFF`; então `T+0x86 = 0`, Tile_ClearSlots(0x25,1).
- Próximo: 0x05. Confiança: confirmado.

## 0x080303A4 Fight_DispatchOnSubstateAA_BR_05_JitterSlotsX
- Passo: BR 0x05. Por 0x20 frames, a cada 2 frames, X dos 4 slots ±1 (máscara 0x1FF), sentidos alternados. Próximo: 0x06. Confiança: confirmado.

## 0x08030494 Fight_DispatchOnSubstateAA_BR_06_BlinkSpreadSlotsAndClear
- Passo: BR 0x06. Por 0x20 frames: a cada 4 frames Y dos 4 slots ±1 (&0xFF); a cada frame alterna o bit 0x8000 do halfword +0 dos slots (pisca).
- No fim Tile_ClearSlots(0x21,4). Próximo: 0x07. Confiança: confirmado.

## 0x0803059C Fight_DispatchOnSubstateAA_BR_07_SetReactionFlagBit2
- Passo: BR 0x07. `B(F+0x87)+0xC0 |= 4`. Próximo: 0x08. Confiança: confirmado.

## 0x080305C8 Fight_DispatchOnSubstateAA_BR_08_SetStepCode6A
- Passo: BR 0x08. `F+0xA6 = 0x6A`. Próximo: 0x09 (`0x080305E0`, comum). Confiança: confirmado.

## 0x08030890 Fight_DispatchOnSubstateAA_BR_10_ActionSelectLoadGfx
- Cena 8, GS 0x00 (BR 0x10). 3 subpassos no contador `T+0x139`: 0 = Fight_ResetBotPartFlags(F+0x87) + Fight_CopySixSpritePositions;
  1 = descompacta `0x084CA064` -> `0x06014E00`; 2 = paleta `0x084E4598` -> `0x050002C0`, GS++. Confiança: confirmado.

## 0x08030910 Fight_DispatchOnSubstateAA_BR_11_ActionSelectSetupSlot3
- Cena 8, GS 0x01. Slot 3 (`S+0xC0`, attr 0xC004) na posição do bot (Y-4), Slot_SetupFromRecordList(`[0x083B6100+0x54]`, 3), SFX 7.
- GS++. Confiança: confirmado.

## 0x08030990 Fight_DispatchOnSubstateAA_BR_12_ActionSelectWaitSlot3
- Cena 8, GS 0x02. Anim_RunSlotScript(slot 3) até `S+0xE2 == 0xFF`; Tile_ClearSlots(3,1); GS++. Confiança: confirmado.

## 0x080309C8 Fight_DispatchOnSubstateAA_BR_13_ActionSelectCheckPart
- Cena 8, GS 0x03. O decompile do Ghidra quebra ("bad instruction"); li os bytes Thumb à mão.
- Se `B(F+0x87)+0xBD` bit0: `F+0xA6 = 4`, GS = 0x20 (caminho de erro). Senão, se `B+0xDD == 0` (nenhuma peça): GS = 0x10. Senão GS++ (0x04).
- Confiança: confirmado (bytes).

## 0x08030A24 Fight_DispatchOnSubstateAA_BR_14_ActionSelectValidatePart
- Cena 8, GS 0x04. Fight_SetTeamSideState3; valida a ação escolhida em `B+0xDD`. Erros gravam `F+0xA6` = 4 (+0xBD bit0), 5 (peça com
  armadura 0), 6 (+0xC9 == 0x2B e +0xCA == +0xDD), 7 (Fight_IsPartTechniqueSupportUtilityClass), 8 (cabeça sem usos em +0x58),
  9/0x0A/0x0B/0x0C (checagens de alvo +0xE0/+0xE1) e põem GS = 0x20.
- +0xDD >= 4 (medaforce): `F+0xA7 = +0xDD-4`, GS = 0x30. OK: `F+0x164 = 1`, Fight_AssignBotTargetSlot, `F+0xA8` = técnica da peça (+0x49), `F+0xA7` = peça-1, GM = 9, GS = 0.
- Confiança: confirmado (decompile); o sentido de cada código de erro é provável.

## 0x08030CFC Fight_DispatchOnSubstateAA_BR_20_NoPartCheckMedaforceFull
- Cena 8, GS 0x10 (nenhuma peça). Se `B+0xC1 == 0x50` (medaforce cheio): GS++ (0x11). Senão Fight_ResetBotVisualCounters e GS = 0x14.
- Confiança: confirmado; o significado de 0x50 é provável.

## 0x08030D3C Fight_DispatchOnSubstateAA_BR_21_RunDispatchBThenNext
- Cena 8, GS 0x11, 0x20 e 0x31 (BR 0x21/0x30/0x41). Fight_RunSubstateDispatchB() até devolver != 0; então GS++. Confiança: confirmado.

## 0x08030D58 Fight_DispatchOnSubstateAA_BR_22_RunDispatch0ThenNext
- Cena 8, GS 0x12. Fight_RunSubstateDispatch(0) até devolver != 0; então GS++. Confiança: confirmado.

## 0x08030C94 Fight_DispatchOnSubstateAA_BR_23_GotoEndCheck
- Cena 8, GS 0x13, 0x18, 0x22 e 0x33 (BR 0x23/0x28/0x32/0x43). Só grava GS = 0x40 (vai para BR 0x50). Confiança: confirmado.

## 0x08030D74 Fight_DispatchOnSubstateAA_BR_24_RunDispatchCThenNext
- Cena 8, GS 0x14 e 0x34. Fight_RunSubstateDispatchC(0) até devolver != 0; então GS++. Confiança: confirmado.

## 0x08030D90 Fight_DispatchOnSubstateAA_BR_25_RunDispatchCAnd1
- Cena 8, GS 0x15. Fight_RunSubstateDispatchC(0) e Fight_RunSubstateDispatch(1); quando a segunda devolve != 0, GS++. Confiança: confirmado.

## 0x08030DB4 Fight_DispatchOnSubstateAA_BR_26_BuildPartObjects
- Cena 8, GS 0x16. DispatchC(0), SFX 0x51, `F+0x51 = 0x50` se `B+0xC0` bit3, senão 0x28; Fight_BuildBotPartObjects8(F+0x87); GS++.
- Confiança: confirmado.

## 0x08030E14 Fight_DispatchOnSubstateAA_BR_27_StepPartObjects
- Cena 8, GS 0x17. DispatchC(0); Fight_StepBotVisualUp + Fight_StepBotPartObjects8; quando as duas devolvem 1: Fight_InitBattleSprites,
  Fight_ResetBotVisualCounters, GS++ (0x18 -> GotoEndCheck). Confiança: confirmado.

## 0x08030E54 Fight_DispatchOnSubstateAA_BR_31_RunStepCodeMessage
- Cena 8, GS 0x21, 0x32 e 0x38 (BR 0x31/0x42/0x48). Fight_RunSubstateDispatch(`F+0xA6`) até != 0; então GS++.
- Roda o passo/mensagem do código gravado antes em +0xA6. Confiança: confirmado (código); "mensagem" é provável.

## 0x08031614 Fight_DispatchOnSubstateAA_BR_40_CheckMedaforceRequirements
- Cena 8, GS 0x30 (caminho medaforce). Fight_ResetBotVisualCounters. Se `F+0x58 != 0`: `F+0xA6 = 0x79`, GS++.
- Se nível `F+0xA7` = 0/1/2 e `B+0x05` < 10/0x1E/0x3C: `F+0xA6 = 0x36`, GS++. Se `B+0xC1 == 0x50`: GS = 0x34 (segue). Senão `F+0xA6 = 0x37`, GS++.
- GS 0x31..0x33 = erro (DispatchB, mensagem, fim). Confiança: confirmado (código); "medaforce/nível" é provável.

## 0x08031704 Fight_DispatchOnSubstateAA_BR_45_SetupMedaforceSlots
- Cena 8, GS 0x35. DispatchC(0); paleta `0x084E45B8` -> `0x050002E0`; slots 0x21/0x22 na posição do bot (paleta `(F+0x3C)+2`),
  record list `[0x083B6100+0x134]`/`[+0x138]`; `F+0x51 = B+0xAA`; GS++. Confiança: confirmado.

## 0x0803185C Fight_DispatchOnSubstateAA_BR_46_WaitMedaforceAnim
- Cena 8, GS 0x36. DispatchC(0), Fight_StepBotVisualDown, Anim_RunSlotScript nos slots 0x21/0x22 até `S+0x862 == 0xFF`;
  então Fight_InitBattleSprites, Fight_ResetBotVisualCounters, Tile_ClearSlots(0x21,2), GS++. Confiança: confirmado.

## 0x080318CC Fight_DispatchOnSubstateAA_BR_47_LoadMedaforceName
- Cena 8, GS 0x37. FUN_0803F63C(`[0x083B74B8 + B+0xA8*4]`, `0x030004DE`, 0x11) (provável cópia do nome do medaforce para o buffer);
  `F+0xA6 = 0x38`; GS++ (0x38 roda a mensagem). Confiança: confirmado (código); nome provável.

## 0x08031918 Fight_DispatchOnSubstateAA_BR_49_StartMedaforceAction
- Cena 8, GS 0x39. `F+0x164 = 1`, `F+0xA8 = B+0xA8 + 0x50` (script de ação do medaforce), zera `B+0xE0/+0xE1`, GM = 9, GS = 0.
- Confiança: confirmado.

## 0x08030E78 Fight_DispatchOnSubstateAA_BR_50_CheckLeadersDefeated
- Cena 8, GS 0x40 (fim de qualquer caminho). Descompacta `0x084C8034` -> `0x06017400`; Fight_StepBotPartTimer + Fight_SelectBotActivePart(F+0x87).
- `T+0x9C = 0`; se `B(0)+0x40 == 0`: `T+0x9C = 2`; senão se `B(3)+0x40 == 0`: `T+0x9C = 1`; nesses casos snd_fade_request(4), SFX 0x6F, GS++. Senão GS = 0x48.
- Provável: +0x40 = líder vivo, `T+0x9C` = resultado (2 derrota, 1 vitória). Confiança: confirmado (código); sentido provável.
