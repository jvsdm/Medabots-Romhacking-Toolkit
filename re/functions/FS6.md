# FS6: 38 funções da lista (FUN_ do sync, Ghidra)

Endereços da ROM EU. Dois grupos: (a) 11 chamadas por BL de código já nomeado; (b) 27 entradas de tabelas de ponteiro
(sem BL, sem xref no Ghidra). Tabela de passos do menu 10 (Link) = 0x083AFC78 (96 ponteiros, `gMenu10StepTable_AFC78`,
lida por `Scene_GsDispatch_Menu10_Link` 0x0803D538 com índice 0x0300137C; ver ST2.md). Índice = (ponteiro - 0x083AFC78)/4.
Renomes aplicados no Ghidra: 36 (todos exceto 0x0803D15C, que ficou FUN_ por falta de prova; 0x08016614 já tinha nome).

## 0x0801630C Fight_MarkEntrySelected
- O que faz: se o byte de guarda `+0x83` (em 0x03000...) é 0, marca a entrada `ctx+0x81` com `attr |= 0x20`, incrementa o contador `+0x22` e põe o byte de quadro `+8/+9` em 0x50 (bit 3 de `+0x22` setado) ou 0x40.
- RAM: bloco de slots de 0x40 bytes (base em DAT_08016358), ctx em DAT_08016354.
- Chamada por: Fight_BotSelectMenuFrame (0x08012C96).
- Confiança: provável (lido no código; nome conforme FN1 e R1).

## 0x08016370 Obj_SpawnGroup_FromTable4
- O que faz: lê registro de 4 bytes da tabela DAT_08016424[idx*4] (contagem, atributo, X, Y), limpa 10 entradas de atributo a partir do slot 0x3C e grava `count` sprites OBJ em DAT_08016428+0xF00 (tile, X, Y; bit 0x1000 se idx>2).
- RAM: buffer OAM em DAT_08016428 (+0xF00), atributos de slot.
- Chamada por: FUN_08012BC0 (0x08012BCC) e Fight_SubstateC_0_CpuCopyThenBotStep (0x0801C340).
- Confiança: provável (estrutura confirmada; significado do bit 0x1000 é chute).

## 0x080165D0 Fight_StepSixEntryHeights
- O que faz: percorre 6 entradas (stride 0x40 a partir de DAT_080165EC). Se o byte `+9` é 1, limpa o bit 1 de atributo (Slot_ClearAttrMask); senão subtrai 8 de `+9` (mín. 1) e de `+8`.
- RAM: slots 0x40 bytes, bytes +8/+9.
- Chamada por: FUN_08012DF4 (0x08012E10).
- Confiança: confirmado no código; nome provável.

## 0x08016614 Fight_DrawTwoFieldsIfCounterLow10
- O que faz: se o contador `DAT_08016654+0x84` < 10, lê o retângulo do tilemap (11,4, 19x16) e regrava deslocado (mesma família de 0x08016660).
- RAM: buffer de tilemap, contador +0x84 do bloco de UI.
- Chamada por: FUN_08012DF4 (0x08012E14).
- Confiança: confirmado (nome já existia no Ghidra; conferido).

## 0x08016DCC Fight_DrawBotFractionPanel
- O que faz: se o estado `bot+0x82 == 1`, limpa o retângulo 2,1 14x12 e desenha a fração de dois dígitos com `bot+0x58` e `bot+0x50`; senão apaga o retângulo 7,1.
- RAM: bloco do bot (0x03000590 + bot*0x100, offset Ghidra), tilemap.
- Chamada por: FUN_080130D8 (0x0801313C) e Fight_UpdatePartKindSelector (0x08016A22).
- Confiança: confirmado (corpo lido). DIVERGE de R1.md, que chama de Fight_DrawActionPanelForKind.

## 0x08017734 Fight_DrawBotInfoPanel
- O que faz: com arg 0, copia 3 bytes (cor) e 6 bytes (nome) do bot, calcula `bot+0x22 / 10` e desenha 8 linhas de tilemap, o decimal de 6 dígitos e a faixa lateral B. Com arg != 0, desenha a leitura numérica do bot e a faixa lateral A (se `bot+0xC1 != 'P'`).
- RAM: bloco do bot (+0x80, +0x82, +0x22, +0xC1), tilemap.
- Chamada por: FUN_080173B4 (0x0801752E e 0x08017578).
- Confiança: provável (função grande; efeito visual inferido do corpo).

## 0x08017A7C Fight_DrawBotSlotSpriteB
- O que faz: mapeia o índice (p>2 -> p-3) para o bot; se bate com `bot+0x3D` (lado 0) ou `bot+0x46` (lado 1) monta o composto de 4 sprites da peça e limpa bits de atributo de dois slots.
- RAM: bloco do bot, buffer de OBJ, atributos de slot.
- Chamada por: HandleSelectedMedabotPreviewCursorInput (0x08018416, 0x08018480, 0x080184AC, 0x080184DA, 0x08018504) e Fight_SlotConfirmStartSlide (0x080185CA).
- Confiança: provável (nome de R1 mantido).

## 0x08017C50 Fight_FillInfoBand_BlinkState
- O que faz: preenche a faixa 2,2 com um tile escolhido pelo lado do bot (`+0x3C`); arg 0 faz 0x1A de largura, arg != 0 faz 2 de largura com outro tile.
- RAM: tilemap BG.
- Chamada por: Fight_SlotConfirmStartSlide (0x080185D8) e Ui_TickBlinkCounter (0x08017CFE, 0x08017D0A), no pisca de 8 em 8 quadros.
- Confiança: provável (o sentido "pisca" vem do chamador; FN1 chamava de Fight_FillInfoRows_BySide).

## 0x08017D14 Fight_DrawFiveSlotRowsB
- O que faz: preenche 5 faixas de 9 tiles nas linhas 2, 4, 6, 8, 10 com o tile de DAT_08017D48.
- RAM: tilemap BG.
- Chamada por: Fight_BattleSceneInit (0x08013CC4).
- Confiança: confirmado no código (nome de R1 mantido).

## 0x08036BF4 Ui_FillRect12x13_ByFlag
- O que faz: preenche o retângulo (12,13) de altura 0 com largura 0x10 se arg==0, senão 2.
- RAM: tilemap BG.
- Chamada por: Fight_DispatchOnSubstateAA_H_14_StepByOrientation (0x0801EAAC).
- Confiança: confirmado (corpo); significado do flag é provável (HA2 já confirmou o chamador).

## 0x08037C50 Fight_SlotStepMoveOrSpawnEffect
- O que faz: roda o script de slot (`Anim_RunSlotScript`, tabela indexada por `+0x2E`). Se `+0x2D == 0xFF` incrementa `+0x29`; senão move X/Y (±0x10 pelo bit 0 de `bot+0xF8`), recalcula o offset de composto, gera o efeito (FightPreview_SpawnEffectSprite) e incrementa `+0x2D`. Caso `(+0x2D & 0x3F)==0` despacha por tabela indireta não recuperada.
- RAM: slot 0x40 bytes, bloco do bot, atributos.
- Chamada por: nenhuma por BL; entrada de tabela de passos 0x083AF6B8 (dona não identificada).
- Confiança: provável (corpo lido; a tabela não foi atribuída).

## 0x0803809C Fight_StepEndClearSlotAttr
- O que faz: limpa a máscara de atributo (ClearObjectAttributeEntries) do slot apontado pela tabela de índice.
- RAM: atributos de slot.
- Chamada por: nenhuma por BL; entradas de tabela 0x083AF6C0 e vizinhas (dona não identificada).
- Confiança: provável (corpo de 4 instruções; papel de "fim de passo" é chute).

## 0x080380F0 Fight_SlotStepAdvanceXFromTable
- O que faz: se `+0x2D == 0x46` incrementa `+0x29`; senão soma uma palavra de tabela (índice `(+0x2D & 0xF)*2`) ao X (`+2`), e se `+0x2D` tem bit 0, incrementa `+4`; por fim `+0x2D += 1`.
- RAM: slot 0x40 bytes (+2, +4, +0x29, +0x2D).
- Chamada por: nenhuma por BL; entradas da tabela 0x083AF6xx (dona não identificada).
- Confiança: provável.

## 0x080381E0 Fight_SlotStepSlideX16
- O que faz: se `+0x2D == 0x23` incrementa `+0x29`; senão move o X de `+0x18` em ±0x10 (bit 0 de `bot+0xF8`) e incrementa `+0x2D`.
- RAM: slot, bloco do bot (+0xF8).
- Chamada por: nenhuma por BL; entrada da tabela 0x083AF730 (dona não identificada).
- Confiança: provável.

## 0x08038AC0 Fight_SlotStepPlaceBotMarkers
- O que faz: incrementa `+0x28`, escolhe X por tabela (`0x13C`, ±2 conforme `+0xF8 & 2`), chama Fight_PlaceBotMarkerSlotsAroundBase, roda o script do slot (`bot+0x1B8`); se `+0x22 == 0xFF` reposiciona e limpa a entrada.
- RAM: slots, bloco do bot.
- Chamada por: nenhuma por BL; entrada da tabela 0x083AF8A8 (dona não identificada).
- Confiança: provável.

## 0x0803B9A4 Fight_StepPreviewCompositeLayers
- O que faz: se o slot `+0x30 == 0`, para o lado 0 ou 1 (`*DAT_0803B9C8`) carrega os ids de peça exibidos (FightPreview_LoadDisplayedPartIdsForSide) e monta 2 camadas de composto de prévia (BuildCompositePreviewSpriteLayerWithOffset); limpa o bit de pendência e marca `+0x29 = 1`. Senão decrementa `+0x30`.
- RAM: bloco do bot, flags de pendência em PTR_DAT_0803BAA4 / 0803BBA8.
- Chamada por: nenhuma por BL; entrada da tabela 0x083AFAF4 (dona não identificada).
- Confiança: provável.

## 0x0803C044 Fight_SlotStepSaveXYAndAdvanceTile
- O que faz: se `+0x2D == 0`, limpa a entrada; senão decrementa `+0x2D`, avança o offset de tile do slot (Anim_AdvanceSlotTileOffset) e copia X/Y do slot para a área `+0x154/+0x158` (base depende de `+0xEC`).
- RAM: slot, área 0x154..0x15C de um bloco de estado.
- Chamada por: nenhuma por BL; entrada da tabela 0x083AFB10 (dona não identificada).
- Confiança: provável.

## 0x0803CBD0 Fight_SlotStepSpawnNextRecord
- O que faz: se `+0x2B == 0`, copia campos do slot para o slot `+0x2E`, põe `+0x29 = 0x21`, carrega o registro de `Slot_SetupFromRecordList` (tabela por `+0x2D`) e limpa a entrada; senão decrementa `+0x2B`.
- RAM: slots 0x40 bytes.
- Chamada por: nenhuma por BL; entrada da tabela 0x083AFB6C (dona não identificada).
- Confiança: provável.

## 0x0803CECC Fight_StepPaletteFadeToColor
- O que faz: com `+0x2B == 0`, faz fade de linhas de paleta para duas cores (Palette_FadeRowsToColorA e B) com cor montada de `+0x2C/+0x2D/+0x2E`, decrementa `+0x2F` e, ao zerar, põe `+0x29 = 0`; senão decrementa `+0x2B`.
- RAM: slots, buffer de paleta.
- Chamada por: nenhuma por BL; entrada da tabela 0x083AFB90 (dona não identificada).
- Confiança: provável.

## 0x0803CF7C Fight_SlotStepAdvanceTileOffset
- O que faz: decrementa `+0x2C` (se não zero); se `+0x2B == 0` avança o offset de tile (Anim_AdvanceSlotTileOffset), senão decrementa `+0x2B`.
- RAM: slot 0x40 bytes.
- Chamada por: nenhuma por BL; entrada da tabela 0x083AFBAC (dona não identificada).
- Confiança: provável.

## 0x0803CFC8 Fight_SlotStepStartScriptAfterDelay
- O que faz: com o atraso de `+0x2C` zerado, seta o bit 0x8000 de atributo e roda o script do slot (tabela por `+0x2D`); senão decrementa o atraso. Depois recalcula o offset de composto e ajusta `+0x26` em ±0x10 (bit 1 de `+0xF8`).
- RAM: slot, bloco do bot.
- Chamada por: nenhuma por BL; entrada da tabela 0x083AFBB0 (dona não identificada).
- Confiança: provável.

## 0x0803D0BC Fight_SlotStepSlideYByDelta
- O que faz: roda o script do slot e desliza Y (`+8`, `+9`) pelo passo `+0x26`, para cima ou para baixo conforme `+0x23`; ao chegar no limite limpa a entrada (ou zera `+0x29`).
- RAM: slot 0x40 bytes.
- Chamada por: nenhuma por BL; entrada da tabela 0x083AFBBC (dona não identificada).
- Confiança: provável (condição de parada lida; sentido do limite é chute).

## 0x0803D15C (FUN_ mantido)
- O que faz: roda o script do slot; com `+0x2B < 7` soma 8 a `+8/+9`; com `+0x2B > 0x3C` recua 4 ou limpa a entrada; depois move um segundo slot e recalcula o offset de composto.
- RAM: slots 0x40 bytes.
- Chamada por: nenhuma por BL; entrada da tabela 0x083AFBC4 (dona não identificada).
- Confiança: chute (muita lógica acoplada; nome não atribuído, sem prova de papel).

## 0x0803D928 Link_StepPaletteFadeDown
- O que faz: Obj_StepAccumMoveA; se o passo `+0x84 < 9`, faz fade das duas linhas de paleta com valor `0x20 - 4*n` e incrementa `n`; senão incrementa o contador do passo.
- RAM: bloco de estado do Link (+0x84), contador de passo (0x0300137C).
- Chamada por: tabela gMenu10StepTable_AFC78, índice 8 (0x083AFC98).
- Confiança: provável (tabela confirmada; sentido do fade é chute, por isso "Down").

## 0x0803D980 Link_StepLoadObjAndAdvance
- O que faz: Obj_StepAccumMoveA, Obj_InitSlotAndLoad(1,0,0) e incrementa o contador do passo.
- RAM: contador de passo.
- Chamada por: tabela gMenu10StepTable_AFC78, índice 9 (0x083AFC9C).
- Confiança: provável.

## 0x0803D9C8 Link_StepCopyPaletteAndAdvance
- O que faz: Obj_StepAccumMoveA, CopyPaletteBuffersToHardware, zera `+0x84` e incrementa o contador do passo.
- RAM: paleta, contador.
- Chamada por: tabela gMenu10StepTable_AFC78, índices 11 (0x083AFCA4) e 42 (0x083AFD20).
- Confiança: provável.

## 0x0803D9F0 Link_StepPaletteFadeUp
- O que faz: como D928, mas com fade de valor `4*n` (sobe); incrementa o passo `+0x84` até 9, depois o contador.
- RAM: paleta, +0x84, contador.
- Chamada por: tabela gMenu10StepTable_AFC78, índice 12 (0x083AFCA8).
- Confiança: provável.

## 0x0803DA40 Link_StepPaletteFadeUp_B
- O que faz: idêntico a 0x0803D9F0 (cópia com outro literal de dados).
- RAM: idem.
- Chamada por: tabela gMenu10StepTable_AFC78, índice 43 (0x083AFD24).
- Confiança: confirmado (corpo igual).

## 0x0803DB94 Link_StepDispatchSubB_ThenAdvance
- O que faz: Obj_StepAccumMoveA; se StateMachine_DispatchSubB devolve != 0, Obj_ResetStateFields e incrementa o contador.
- RAM: contador, estado.
- Chamada por: tabela gMenu10StepTable_AFC78, índice 37 (0x083AFD0C).
- Confiança: provável.

## 0x0803DBB8 Link_StepDispatchStateMachine
- O que faz: Obj_StepAccumMoveA; StateMachine_DispatchStep(*(DAT+4)); se conclui, incrementa o contador.
- RAM: contador.
- Chamada por: tabela gMenu10StepTable_AFC78, índice 38 (0x083AFD10).
- Confiança: provável.

## 0x0803DBE0 Link_StepWaitOverlayTarget
- O que faz: Obj_StepAccumMoveA; Overlay_SetTargetAndCheckReached; ao chegar, zera `+0x86` e incrementa o contador.
- RAM: +0x86, contador.
- Chamada por: tabela gMenu10StepTable_AFC78, índice 39 (0x083AFD14).
- Confiança: provável.

## 0x0803DC0C Link_StepCycleTimer9
- O que faz: Obj_StepAccumMoveA; conta `+0x86` de 0 a 8 e, ao chegar em 8, zera e incrementa o contador.
- RAM: +0x86, contador.
- Chamada por: tabela gMenu10StepTable_AFC78, índice 40 (0x083AFD18).
- Confiança: provável.

## 0x0803DC3C Link_StepSoundFadeOut
- O que faz: Obj_StepAccumMoveA, snd_fade_request(8) e incrementa o contador.
- RAM: contador; som (request 8).
- Chamada por: tabela gMenu10StepTable_AFC78, índice 41 (0x083AFD1C).
- Confiança: provável.

## 0x0803DC58 Link_StepSetStateWordByFlag
- O que faz: Obj_StepAccumMoveA; se o byte `+0x138` é 0 incrementa a palavra de estado, senão grava 0x30.
- RAM: palavra de estado (DAT_0803DC78 / DAT_0803DC88), +0x138.
- Chamada por: tabela gMenu10StepTable_AFC78, índice 44 (0x083AFD28).
- Confiança: provável.

## 0x0803DE70 Link_StepGpuBlendTick
- O que faz: Counter_IncIfFlagBit0; GpuBlend_TickStep; se conclui, incrementa o contador.
- RAM: contador, blend de GPU.
- Chamada por: tabela gMenu10StepTable_AFC78, índice 49 (0x083AFD3C).
- Confiança: provável.

## 0x0803DE90 Link_StepLoadSlotAndAdvance
- O que faz: Counter_IncIfFlagBit0; Obj_InitSlotAndLoad(1,0,0); incrementa o contador.
- RAM: contador, slot.
- Chamada por: tabela gMenu10StepTable_AFC78, índice 50 (0x083AFD40).
- Confiança: provável.

## 0x0803DEB0 Link_StepDispatchByFlag
- O que faz: Counter_IncIfFlagBit0; escolhe o estado por `+0x9C` (==1 usa DAT+8, senão DAT+0xC) e chama StateMachine_DispatchStep; se conclui, incrementa o contador.
- RAM: +0x9C, ponteiros de estado.
- Chamada por: tabela gMenu10StepTable_AFC78, índice 51 (0x083AFD44).
- Confiança: provável.

## 0x0803E018 Link_StepGaugeAndTween
- O que faz: Counter_IncIfFlagBit0; Gauge_SetCounterFrom0x68; Gauge_DispatchByStepIndex; Tween_SetSpeedAndPlayEntry; incrementa o contador.
- RAM: contador, medidor, tween.
- Chamada por: tabela gMenu10StepTable_AFC78, índice 54 (0x083AFD50).
- Confiança: provável.

## Em aberto
- Dona das tabelas 0x083AF6B8..0x083AF8xx, 0x083AFAF4..0x083AFBC4 (lidas por literais em 0x08037BEC, 0x0803B8F8 e outros; funções desses literais sem nome no Ghidra). Não confirmei se são tabelas de passo de slot ou outra coisa.
- Tabela indireta de 0x08037C50 (DAT_08037CC0) não recuperada.
- 0x0803D15C ficou sem nome.
