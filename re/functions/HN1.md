# HN1: handlers dos despachantes Fight_DispatchOnSubstateAA_* (lista HN1_lista.txt, 40 entradas)

Despachantes U, V, W, X, Y, Z (0x08021E04..0x08022FF0) e AP, AQ, AR (0x08026CF4..0x08026FD0). Bloco = `fight_information`
0x03001800: +0xAA = passo (`action_sequence_state`), +0xB5 = handler concluído (zerado pelo stub), +0xA6 = código do próximo
passo de apresentação/mensagem, +0x87 = bot que age, +0x88 = alvo, +0xA7 = peça usada, +0xEC/+0xE8/+0xC8/+0xCA = valores da ação.
Bot = 0x03000590 + bot*0x100 (DAT_ não resolvidos; endereços pelo padrão de R1/HA). Todos os 40 decompilados e renomeados no Ghidra
(`Fight_DispatchOnSubstateAA_<Desp>_<idx>_<Passo>`). "Próximo" = valor gravado em +0xAA (+1 se nada for dito).

Padrões:
- Passo 0x00 de U/V/W/X/Y/Z = abertura comum: `Fight_AccumRosPart(ator, peça)`, `Fight_ApplyStatusCostIfFlag`,
  `Fight_CheckRosMeetsMinimumThreshold` (0x0803725C). Se falhar: +0xA6=0x1A e +1 (mensagem de falha). Se passar:
  `Fight_IncrementActionUsageCounter`, às vezes `Fight_ComputePartValue` (0x08037280), e +0xAA=4 (pula 1..3; entra na animação).
- Passos 0x04/0x08 = carregar gráfico (LZ77 `decompress_and_store` + `bios_cpufastset` da paleta) e montar slots de OBJ
  (gObjectSlots: +0x840.. = slot 0x21.., posição do bot em +0x1742/+0x1744 + bot*0x100). Depois passos de animação que só
  avançam quando o slot termina (`Anim_RunSlotScript`, `Fight_StepSlotTowardTarget`, `Tile_ClearSlots`).
- Último passo = efeito no bot (cura/status/buff) e grava +0xA6 com o código de mensagem (0x1B, 0x23, 0x28, 0x29, 0x72/0x73).
- X e Y têm a mesma sequência visual (Y reaproveita X_09..X_0B e X_0D); só mudam o teste no passo 0 e o efeito final.

## 0x08021E2C U 0x00 -> Fight_DispatchOnSubstateAA_U_00_CheckRosAndTargetStatus
- Passo: abertura da técnica de U (ROS). Falha: +0xA6=0x1A, +1. Passa: se alvo +0xC9 == 0 (sem status) grava +0xA6=0x70 e +1
  (mensagem "nada a fazer", provável); senão incrementa uso e vai para 4.
- Lê: +0x87, +0xA7, +0x88, bot alvo +0xC9. Escreve: +0xA6, +0xAA.
- Confiança: confirmado (decompile); significado de 0x70 é chute.

## 0x08021EA0 U 0x08 -> Fight_DispatchOnSubstateAA_U_08_LoadEffectGfxOnTarget
- Passo: carrega gráfico/paleta do efeito, liga bit 0x200 (e 0x1000 se +0xF8&1) no slot 0x21, posiciona sobre o alvo (+0x88),
  `Slot_SetupFromRecordList(.., 0x21)`, timer +0x86 = 0x3C. Próximo: +1.
- Confiança: confirmado (decompile).

## 0x08021F90 U 0x0A -> Fight_DispatchOnSubstateAA_U_0A_ScrollBgAndStepSlotToTarget (também AF, AG, AI, AJ)
- Passo: rola um registro de tela 0x40 por quadro (sinal por +0xF8&1) e anda o slot 0x21 até o alvo
  (`Fight_StepSlotTowardTarget`). Ao chegar: limpa bits do slot +0x840, zera o rolamento, +1.
- Confiança: confirmado (decompile). Handler compartilhado por 5 despachantes.

## 0x08022010 U 0x0B -> Fight_DispatchOnSubstateAA_U_0B_ClearTargetStatus
- Passo: efeito de U: zera +0xC9..+0xCC do bot alvo (status/condição; provável "cura de status"), liga bits no slot
  do alvo (0x47+alvo*3), escala 0x40, SFX 0x37. Próximo: +1.
- Confiança: confirmado (decompile); "status" é provável.

## 0x080220A0 U 0x0C -> Fight_DispatchOnSubstateAA_U_0C_RunAnimAndClearTargetSlot
- Passo: encolhe/ajusta escala do slot do alvo até 4, roda a anim do slot 0x21; quando o script termina (-1) limpa slot
  0x21 e o do alvo. Próximo: +1.
- Confiança: confirmado (decompile).

## 0x0802213C U 0x0D -> Fight_DispatchOnSubstateAA_U_0D_SetMessage23
- Passo: final de U: +0xA6 = 0x23, +1.
- Confiança: confirmado.

## 0x0802217C V 0x00 -> Fight_DispatchOnSubstateAA_V_00_CheckRos
- Passo: abertura comum (ROS). Falha: +0xA6=0x1A, +1. Passa: incrementa uso, +0xAA=4.
- Confiança: confirmado.

## 0x080221C8 V 0x04 -> Fight_DispatchOnSubstateAA_V_04_SpawnFourOrbsOnActor
- Passo: carrega gráfico, cria 4 sprites (slots 0x21..0x24) na posição do ator, ângulos 0/0x40/0x80/0xC0, SFX 0x5C. +1.
- Confiança: confirmado (decompile); "orbs" é nome visual provável.

## 0x080222A8 V 0x05 -> Fight_DispatchOnSubstateAA_V_05_SpiralOrbsAroundActor
- Passo: gira os 4 sprites em volta do ator (`CompositeSprite_ComputeOffsetWord`, ângulo +4, raio +1 a cada 4 quadros; após 0xB4
  encolhe). Quando a escala chega a 4: limpa slots 0x21..0x24, +1.
- Confiança: confirmado (decompile).

## 0x08022380 V 0x06 -> Fight_DispatchOnSubstateAA_V_06_ToggleFlag58Message
- Passo: final de V: alterna o flag +0x58 do bloco (0->1 com +0xA6=0x72; 1->0 com +0xA6=0x73). +1.
- Confiança: confirmado; o que +0x58 liga (modo/ocultar) é chute.

## 0x080223E4 W 0x00 -> Fight_DispatchOnSubstateAA_W_00_CheckRosPickVariant
- Passo: abertura comum. Se passa: variante +0x51 = 0 se técnica da peça (+0x49) == 0x1B, 1 se 0x1C, 2 senão;
  `Fight_ComputePartValue`; +0xAA=4. Falha: +0xA6=0x1A, +1.
- Confiança: confirmado.

## 0x08022480 W 0x04 -> Fight_DispatchOnSubstateAA_W_04_LoadVariantGfxTwoSprites
- Passo: carrega um de 3 gráficos pela variante +0x51, cria 2 sprites (slots 0x21/0x22) a ±0x20 do ator, SFX 0x33. +1.
- Confiança: confirmado.

## 0x08022648 W 0x05 -> Fight_DispatchOnSubstateAA_W_05_MoveSpritesInward (também BC, BD)
- Passo: move os 2 sprites em volta do ator (ângulo +8, raio -1 a cada 4 quadros, escala até 0x40). Raio 0: SFX 0x30, limpa
  slot 0x22, +1.
- Confiança: confirmado.

## 0x08022708 W 0x06 -> Fight_DispatchOnSubstateAA_W_06_RunAnimUntilDone (também BC)
- Passo: roda a anim do slot 0x21; quando termina (-1) limpa o slot, +1.
- Confiança: confirmado.

## 0x08022748 W 0x07 -> Fight_DispatchOnSubstateAA_W_07_ApplyActorChargeBuff
- Passo: efeito de W no ator: bot +0xBD |= 8, +0xDF = técnica da peça, +0xD8 = 0 e soma +0xEC com teto 0xFF
  (`Fight_AddByteClampToMax`). Buff/carga no próprio bot (provável). +1.
- Confiança: confirmado (código); significado de +0xBD bit 3 provável.

## 0x080227C0 W 0x08 -> Fight_DispatchOnSubstateAA_W_08_SetMessage1B
- Passo: final de W: +0xA6 = 0x1B, +1.
- Confiança: confirmado.

## 0x08022800 X 0x00 -> Fight_DispatchOnSubstateAA_X_00_CheckRosAndTargetTech16
- Passo: abertura comum. Se passa e o alvo tem peça íntegra com técnica 0x16 (`Fight_HasNoTechnique16Part` == 0): +0xA6=0x39,
  +1 (bloqueado). Senão: uso, `Fight_ComputePartValue`, +0xAA=4.
- Confiança: confirmado.

## 0x08022870 X 0x08 -> Fight_DispatchOnSubstateAA_X_08_LoadEffectGfxOnTarget
- Passo: igual a U_08 com outro gráfico (paleta 0x10 words, record +0xF8): sprite no alvo, timer +0x86=0x3C. +1.
- Confiança: confirmado.

## 0x08022938 X 0x09 -> Fight_DispatchOnSubstateAA_X_09_WaitAnimOrTimer (também Y)
- Passo: roda anim do slot 0x21; decrementa o timer +0x86 enquanto +0x89 != 0 ou tecla (bit 0 de um registro) não apertada;
  timer 0 ou pulo: +1.
- Confiança: confirmado; "tecla" provável.

## 0x08022990 X 0x0A -> Fight_DispatchOnSubstateAA_X_0A_StepSlotToTarget (também Y)
- Passo: roda anim e anda o slot 0x21 até o alvo; ao chegar, +1.
- Confiança: confirmado.

## 0x080229C0 X 0x0B -> Fight_DispatchOnSubstateAA_X_0B_SlideOutAndClearSlot (também Y)
- Passo: roda anim; diminui um campo do slot em 8 por quadro; quando vale 8, limpa o slot 0x21 e +1.
- Confiança: confirmado.

## 0x08022A04 X 0x0C -> Fight_DispatchOnSubstateAA_X_0C_SpawnRingSpritesOnTarget
- Passo: sprite central (slot 0x25, record +0xFC) e 4 sprites em anel (0x21..0x24, raio 0x10) no alvo, SFX 0x4A. +1.
- Confiança: confirmado.

## 0x08022B40 X 0x0D -> Fight_DispatchOnSubstateAA_X_0D_ExpandRingAndClear (também Y)
- Passo: gira o anel (+4/quadro), após 0x5A quadros encolhe os 5 sprites; quando a escala chega a 4 limpa 0x21..0x25, +1.
- Confiança: confirmado.

## 0x08022C28 X 0x0E -> Fight_DispatchOnSubstateAA_X_0E_InflictStatus1F
- Passo: efeito de X no alvo: +0xC4 = 0x1F (código de status), +0xC5 = 0, +0xC6/+0xC7 = `Fight_ClampDisplayValue1To99`(+0xE8/+0xEC)
  (duração/força, provável). +1.
- Confiança: confirmado (código); "status" provável.

## 0x08022C90 X 0x0F -> Fight_DispatchOnSubstateAA_X_0F_SetMessage29
- Passo: final de X: +0xA6 = 0x29, +1.
- Confiança: confirmado.

## 0x08022CD0 Y 0x00 -> Fight_DispatchOnSubstateAA_Y_00_CheckRosAndTargetDown
- Passo: abertura comum. Se passa e o alvo tem +0x59 (armadura da cabeça) != 0: +0xA6=9, +1 (alvo não está fora). Se +0x59 == 0:
  uso, `Fight_ComputePartValue`, +0xAA=4.
- Confiança: confirmado (código); leitura "revive" provável.

## 0x08022D4C Y 0x08 -> Fight_DispatchOnSubstateAA_Y_08_LoadEffectGfxOnTarget
- Passo: igual a X_08 (outro gráfico): sprite no alvo, timer 0x3C. +1.
- Confiança: confirmado.

## 0x08022E14 Y 0x0C -> Fight_DispatchOnSubstateAA_Y_0C_SpawnRingSpritesOnTarget
- Passo: igual a X_0C com SFX 0x4B. +1.
- Confiança: confirmado.

## 0x08022F50 Y 0x0E -> Fight_DispatchOnSubstateAA_Y_0E_ReviveTarget
- Passo: efeito de Y: alvo +0x40 = 3, `Fight_ClampRepairToMissingArmor(alvo, cabeça, +0xEC)`, +0x59 = byte de +0xC8, +0xBA = 1;
  atualiza sprites das peças e reposiciona os bots. +1.
- Confiança: confirmado (código); "revive" provável.

## 0x08022FB4 Y 0x0F -> Fight_DispatchOnSubstateAA_Y_0F_PlayEffectSetMessage28
- Passo: final de Y: `FUN_0803F63C(bot alvo, dado, 0x0E)` (mesmo efeito de M_05), +0xA6 = 0x28, +1.
- Confiança: confirmado.

## 0x08023018 Z 0x00 -> Fight_DispatchOnSubstateAA_Z_00_CheckRos
- Passo: abertura comum: falha +0xA6=0x1A/+1; passa: uso, `Fight_ComputePartValue`, +0xAA=4.
- Confiança: confirmado.

## 0x0802306C Z 0x04 -> Fight_DispatchOnSubstateAA_Z_04_LoadEffectGfx (também BG)
- Passo: só carrega gráfico (LZ77) e paleta (0x10 words). +1.
- Confiança: confirmado.

## 0x080230A4 Z 0x05 -> Fight_DispatchOnSubstateAA_Z_05_FlushPalettesClear84 (também BE, BG)
- Passo: `CopyPaletteBuffersToHardware`, zera +0x84 do bloco (contador). +1.
- Confiança: confirmado.

## 0x08026D98 AP 0x18 -> Fight_DispatchOnSubstateAA_AP_18_LoadGfxPlaceOnPart
- Passo: carrega gráfico, SFX 0x2A, OBJ em gObjectSlots+0x280 posicionado sobre o bot (lado +0xF8&2; ajuste ±0x10 pela peça +0x8B,
  posições +0x154/+0x158). +1 e salto indireto no fim (tabela não recuperada pelo decompile).
- Confiança: confirmado (decompile, salto final não lido).

## 0x08026EC0 AP 0x1B -> Fight_DispatchOnSubstateAA_AP_1B_LoopToStep02
- Passo: grava +0xAA = 2 (volta ao passo 2: laço da sequência AP).
- Confiança: confirmado.

## 0x08026EF8 AQ 0x00 -> Fight_DispatchOnSubstateAA_AQ_00_PickActorPartToRepair
- Passo: `Fight_PickMostDamagedRepairablePart1Based(ator)`. Achou: +0xD0 = +0x8B = peça, +0xAA=2. Nada: +1.
- Confiança: confirmado.

## 0x08026F34 AQ 0x02 -> Fight_DispatchOnSubstateAA_AQ_02_RepairActorPart
- Passo: limpa todos os slots de tile (0..0x80), `Fight_ClampRepairToMissingArmor(ator, +0xD0, +0xEC)`. +1.
- Confiança: confirmado.

## 0x08026F6C AQ 0x0D -> Fight_DispatchOnSubstateAA_AQ_0D_RepairPartByAmountCA
- Passo: se +0xCA != 0, escolhe peça mais danificada pelo bot +0x88, grava +0xD0/+0x8B e repara (ator, +0xD0, +0xCA);
  +0xAA=0x10. Senão +1. (Segunda cura com valor +0xCA, provável drenagem/efeito extra.)
- Confiança: confirmado (código); intenção provável.

## 0x08026FC0 AQ 0x13 -> Fight_DispatchOnSubstateAA_AQ_13_JumpToStep04
- Passo: grava +0xAA = 4.
- Confiança: confirmado.

## 0x08026FF8 AR 0x00 -> Fight_DispatchOnSubstateAA_AR_00_DamageRandomPartsOfOpponentTeam
- Passo: zera +0xD2..+0xD4 e +0xD6..+0xDB; +0x60 = `Fight_GetOpponentSideBaseIndex(ator)`; para os 3 bots do outro lado vivos
  (+0x40 e +0x59 != 0): peça aleatória (`Fight_RollStatusRowRandomEntry`), `Fight_ApplyDamageToPart(bot, peça, +0xEC)`,
  +0xD2+i = peça | (+0xD1<<7), +0xD6+2i = +0xC8. +1.
- Confiança: confirmado (decompile). Dano em área no time adversário.
