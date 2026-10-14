# HA4: handlers dos despachantes Fight_DispatchOnSubstateAA_* (lista HA4_lista.txt, 42 entradas)

Cada entrada é um handler dentro da tabela de um despachante, indexado pelo subestado +0xAA. Subestado = `fight_information`
(0x03001800) +0xAA, que é `action_sequence_state` (+2 de `action_script_state` em +0xA8, types TSV). +0xB5 = `action_handler_complete_flag`
(+0xA8+0xD), zerado pelo despachante antes de cada passo.
Renomeados no Ghidra: 42 (decompile lido em todas as seções). As 18 seções que eram "sem função" foram redocumentadas e renomeadas na rodada GH-HA4-redo (2026-10-13).
Coluna "Próximo" = valor escrito em +0xAA ao sair (+1 quando não há outro valor).

## 0x08020474 L 0x0F -> Fight_DispatchOnSubstateAA_L_0F_LoadGfxAndSlots
- Passo: entrada do bot: descompacta gráficos, zera e configura o slot 10 (record list), toca SFX 0x2B.
- Lê/escreve: lê +0xF8 do bloco de bot (flag &2 escolhe o offset 0x154/0x158); escreve sprites (+0x280, +0x282, +0x284, +0x29C) e +0xAA.
- Próximo: 0x10. Confiança: confirmado (decompile).

## 0x08020530 L 0x10 -> Fight_DispatchOnSubstateAA_L_10_WaitSlotAnim
- Passo: espera a animação do slot 10 terminar (Anim_RunSlotScript), depois zera tiles (Tile_ClearSlots 10,1).
- Lê/escreve: lê byte de estado do slot; escreve +0x86=0x3C (timer) e +0xAA.
- Próximo: 0x11. Confiança: confirmado (decompile).

## 0x080205A4 M 0x00 -> Fight_DispatchOnSubstateAA_M_00_LoadGfxSpawnTargetObj
- Passo: descompacta gráficos (decompress_and_store, literais 0x080205F0/F4), copia com bios_cpufastset (SWI 0x0C), lê a posição (x, y) de uma tabela de 0x40 bytes por entrada, no índice (fi+0x87*4 + fi+0xA7 + 0x5A)*0x40, e cria o objeto do slot 0x21 com FUN_08036FF8(0x21, x, y, 4).
- Lê/escreve: fi+0x87 (bot alvo), fi+0xA7 (parte alvo), tabela de posições na ROM, +0xAA.
- Próximo: 0x01 (sempre). Confiança: confirmado (decompile).
- Obs: o mtbe.csv chama 0x08036FF8 de Fight_SpawnInlineEffectTextObjects; o uso aqui (slot 0x21, x, y, 4) parece criação de sprite, não texto. Conferir antes de renomear.

## 0x08020608 M 0x01 -> Fight_DispatchOnSubstateAA_M_01_WaitSpawnThenAdvance
- Passo: chama FUN_080370AC(0x21, 4) (mtbe: FightPreview_TickSpriteTimersCount). Se o retorno for diferente de zero, avança; senão repete o passo.
- Lê/escreve: +0xAA.
- Próximo: 0x02 quando o retorno é não zero; fica em 0x01 até lá. Confiança: provável (o decompile confirma só o desvio; o significado do retorno de 0x080370AC não foi lido).

## 0x08020628 M 0x02 -> Fight_DispatchOnSubstateAA_M_02_LoadGfxAndSlotPos
- Passo: carrega gráficos, posiciona o alvo via tabela (índices de +0x87 e +0xA7 do bot), configura slot 0x21 (record list).
- Lê/escreve: lê fi+0x87/+0xA7 e tabela de coordenadas; escreve em gObjectSlots (+0x840, +0x842 region) e +0xAA.
- Próximo: 0x03. Confiança: confirmado (decompile).

## 0x080206E8 M 0x03 -> Fight_DispatchOnSubstateAA_M_03_WaitSlotAnim
- Passo: espera a animação do slot 0x21, depois Tile_ClearSlots(0x21,1).
- Próximo: 0x04. Confiança: confirmado (decompile).

## 0x08020728 M 0x04 -> Fight_DispatchOnSubstateAA_M_04_ApplyPartDamage
- Passo: aplica o dano na peça alvo (Fight_SubtractPartArmorClampZero), atualiza sprites das peças e objetos; se a parte é a cabeça (+0xA7==0), enfileira bot de cabeça quebrada.
- Lê/escreve: fi+0x87 (bot alvo) e fi+0xA7 (parte alvo); +0xAA.
- Próximo: 0x05. Confiança: confirmado (decompile); significado de +0x87/+0xA7 provável.

## 0x08020768 M 0x05 -> Fight_DispatchOnSubstateAA_M_05_PlayEffect
- Passo: toca efeito da peça (FUN_0803F63C com 0x0E), define +0xA6=0x18 (duração).
- Próximo: 0x06. Confiança: confirmado (decompile).

## 0x080207CC N 0x00 -> Fight_DispatchOnSubstateAA_N_00_CheckBotFlagMsgOrAdvance
- Passo: lê o byte +0xBD do bloco do bot alvo (0x03000590 + fi+0x88*0x100 + 0xBD). Se o bit 4 (&4) está zerado: +0xA6 = 0x19 (mensagem), +0xAA = 2 e +0xAB = 1. Se o bit está ligado: +0xAA += 1 e +0xAB = 0.
- Lê/escreve: bloco do bot (+0xBD), fi+0x88, escreve fi+0xA6, fi+0xAA, fi+0xAB.
- Próximo: 0x02 (desvio com mensagem 0x19) ou +1 (segue). Confiança: confirmado (decompile). O papel de +0xAB (provável: flag de mensagem/desvio) não foi confirmado.

## 0x08020844 O 0x00 -> Fight_DispatchOnSubstateAA_O_00_ApplyPenaltyOrMsg
- Passo: se FightPreview_IsPartSlotEdge(fi+0x88, fi+0x8B) == 0: Fight_AccumRosPart(fi+0x87, fi+0xA7), Fight_ApplyStatusCostIfFlag(fi+0x87) e FUN_0803725C. Se FUN_0803725C == 0: FUN_08037280(fi+0x87, fi+0xA7) (mtbe: Fight_ScaleAttackValueByPartScalar), Fight_IncrementActionUsageCounter_08036270, avança e +0xAB = 0. Se FUN_0803725C != 0: +0xA6 = 0x1A, +0xAA = 2, +0xAB = 1. Se IsPartSlotEdge != 0: +0xA6 = 0x26, +0xAA = 2, +0xAB = 1.
- Lê/escreve: fi+0x87, fi+0x88, fi+0x8B, fi+0xA7; escreve fi+0xA6, fi+0xAA, fi+0xAB.
- Próximo: +1 (caminho normal) ou 0x02 (com mensagem 0x1A ou 0x26). Confiança: confirmado (decompile).
- Obs: a nota antiga falava de uma subtabela 0x083ADA58 chamada aqui; o decompile não tem essa chamada. A chamada 0x0803579C aparece como Fight_AccumRosPart no Ghidra (mtbe: Fight_GetPartProficiencyBase; conflito ainda aberto).

## 0x080208E8 P 0x01 -> Fight_DispatchOnSubstateAA_P_01_ClearTilesApplyRepair
- Passo: Tile_ClearSlots(0, 0x80); fi+0xD0 = fi+0x8B (peça escolhida); FUN_08036CCC(fi+0x88, fi+0xD0, fi+0xEC) (mtbe: Fight_ClampRepairToMissingArmor) aplica o reparo; avança.
- Lê/escreve: fi+0x88, fi+0x8B, fi+0xEC (short); escreve fi+0xD0, fi+0xAA.
- Próximo: 0x02 (+1). Confiança: confirmado (decompile).
- Obs: a nota antiga dizia que este passo escolhia a peça mais danificada; o decompile não chama Fight_PickMostDamagedRepairablePart1Based (0x08033184). Essa escolha fica em outro passo; fi+0x8B já vem preenchido.

## 0x08020928 P 0x0B -> Fight_DispatchOnSubstateAA_P_0B_RepairPartOrSkip
- Passo: reparo. Se a peça alvo tem flag (bloco 0x03000590 + bot*0x100 + part*0x18 + 0x4F) e +0xCA != 0, escolhe peça a reparar (Fight_PickMostDamagedRepairablePart1Based) e chama FUN_08036CCC; senão pula.
- Lê/escreve: lê 0x03000590 (flag da peça), fi+0xCA, fi+0x88; escreve fi+0xD0 e fi+0x8B (slot-1).
- Próximo: 0x0E se reparou; senão 0x0C. Confiança: confirmado (decompile).

## 0x080209A0 P 0x11 -> Fight_DispatchOnSubstateAA_P_11_JumpTo02
- Passo: escreve 2 em +0xAA (volta ao passo 2) e retorna.
- Próximo: 0x02. Confiança: confirmado (decompile, 2 instruções).

## 0x080209D8 Q 0x00 -> Fight_DispatchOnSubstateAA_Q_00_PenaltyIfFlagClearOrMsg
- Passo: lê o byte de flag da peça (bloco do bot + fi+0x88*0x100 + fi+0x8B*0x18 + 0x59). Se for 0: Fight_AccumRosPart(fi+0x87, fi+0xA7), Fight_ApplyStatusCostIfFlag(fi+0x87), FUN_0803725C; se FUN_0803725C == 0: FUN_08037280, Fight_IncrementActionUsageCounter_08036270 e avança; senão +0xA6 = 0x1A, +0xAA = 2, +0xAB = 1. Se a flag não for 0: +0xA6 = 0x27, +0xAA = 2, +0xAB = 1.
- Lê/escreve: fi+0x87, fi+0x88, fi+0x8B, fi+0xA7, byte de flag do bot (+0x59 + parte*0x18); escreve fi+0xA6, fi+0xAA, fi+0xAB.
- Próximo: +1 (caminho normal) ou 0x02 (mensagens 0x1A ou 0x27). Confiança: confirmado (decompile).
- Obs: a nota antiga falava de subtabela 0x083ADA58, que não é chamada aqui. Difere de O 0x00 pelo teste da flag +0x59.

## 0x08020A90 R 0x01 -> Fight_DispatchOnSubstateAA_R_01_ClearTilesSplitCount
- Passo: Tile_ClearSlots(0, 0x80); fi+0xD0 = fi+0x8B; FightPreview_SplitCountByMax(fi+0x88, fi+0xD0, fi+0xEC) divide a contagem pela peça escolhida; avança.
- Lê/escreve: fi+0x88, fi+0x8B, fi+0xEC (short); escreve fi+0xD0, fi+0xAA.
- Próximo: 0x02 (+1). Confiança: confirmado (decompile).
- Obs: a nota antiga dizia que este passo escolhia peça quebrada (0x0803325C) e chamava 0x080351E8 e 0x080372D4; o decompile não tem essas chamadas. Origem de fi+0x8B a confirmar.

## 0x08020AD0 R 0x0C -> Fight_DispatchOnSubstateAA_R_0C_SplitCountOrSkip
- Passo: copia stats se flag (Fight_CopyStatsIfFlagSet); se não há peça quebrada válida, pula; senão divide a contagem (FightPreview_SplitCountByMax) pela peça escolhida.
- Lê/escreve: lê fi+0x88, fi+0xA7, fi+0xCA; escreve fi+0xD0, fi+0x8B.
- Próximo: 0x0D se pulou; 0x0F se dividiu. Confiança: confirmado (decompile).

## 0x08020B4C R 0x08 -> Fight_DispatchOnSubstateAA_R_08_LoadPartIconsAndJump
- Passo: carrega ícones das peças do lado (FightPreview_LoadDisplayedPartIdsForSide 0 ou 1) e chama FUN_080447EC duas vezes; no fim pula por tabela (jump table não recuperada, ptr 0x08020CE8).
- Lê/escreve: lê flag &2 em PTR_DAT_08020C14+0x30; escreve +0xAA.
- Próximo: 0x09, mais o salto pela tabela. Confiança: confirmado (decompile); destino do salto não resolvido.

## 0x08020CEC R 0x12 -> Fight_DispatchOnSubstateAA_R_12_JumpTo02
- Passo: escreve 2 em +0xAA e retorna.
- Próximo: 0x02. Confiança: confirmado (decompile). A nota antiga ("só carrega 0x03001800") estava incompleta.

## 0x08020D24 S 0x00 -> Fight_DispatchOnSubstateAA_S_00_PenaltyComputeModeCopyPart
- Passo: Fight_AccumRosPart(fi+0x87, fi+0xA7), Fight_ApplyStatusCostIfFlag(fi+0x87), FUN_0803725C. Se FUN_0803725C != 0: +0xA6 = 0x1A e avança. Senão: FUN_08037280(fi+0x87, fi+0xA7), Fight_IncrementActionUsageCounter_08036270; lê o byte +0x49 da peça alvo (bloco do bot + fi+0x87*0x100 + fi+0xA7*0x18 + 0x49) e define o modo fi+0x51: valor 10 -> 0; 11 -> 1; 12 -> 2; qualquer outro -> 3. Copia o byte +0x49 para fi+0x52 e põe +0xAA = 4.
- Lê/escreve: fi+0x87, fi+0xA7, byte +0x49 da peça (bloco do bot); escreve fi+0x51 (modo 0..3), fi+0x52, fi+0xA6, fi+0xAA.
- Próximo: 0x04 (ou +1 com mensagem 0x1A). Confiança: confirmado para o fluxo e os valores 10/11/12 (decompile). Significado do byte +0x49 (provável: categoria da peça, que define o modo do efeito).

## 0x08020DF8 S 0x04 -> Fight_DispatchOnSubstateAA_S_04_LoadGfxAndSfx29
- Passo: carrega gráficos e posiciona o sprite do alvo (tabela por +0x87), toca SFX 0x29.
- Lê/escreve: lê fi+0x87 (bot alvo); escreve sprites em gObjectSlots e +0xAA.
- Próximo: 0x05. Confiança: confirmado (decompile). Esta entrada aparece também nos despachantes AB..AU, T, U, X, Y (coluna extra da lista).

## 0x08020EDC S 0x05 -> Fight_DispatchOnSubstateAA_S_05_StepTargetSpriteAnim
- Passo: Fight_StepSlotAnimOffsetTable(0x21) (mtbe: avança a tabela de passos de animação do slot). Se o contador do objeto do slot 0x21 (gObjectSlots + DAT_08020F10) é 0x20: limpa bits de +0x840 pela máscara, zera a palavra de posição e avança. Senão: move o sprite do alvo em +/-0x20 no eixo x (sentido pelo bit 0 de fi+0xF8), anda 1 px no outro eixo se o bloqueio estiver livre e incrementa o contador do objeto.
- Lê/escreve: fi+0xF8 (sentido), objeto do slot 0x21 (+0x840, contador, posição), fi+0xAA.
- Próximo: 0x06 quando o contador chega a 0x20; senão fica em 0x05. Confiança: confirmado (fluxo do decompile); nomes exatos dos campos do objeto: provável.

## 0x08020FB8 S 0x06 -> Fight_DispatchOnSubstateAA_S_06_LoadGfxAndSlots
- Passo: carrega gráficos e configura slot 0x21 (record list +0x8C).
- Próximo: 0x07. Confiança: confirmado (decompile).

## 0x08021000 S 0x07 -> Fight_DispatchOnSubstateAA_S_07_WaitSlotAnim
- Passo: espera a animação do slot 0x21 (record +0x8C).
- Próximo: 0x08 quando termina. Confiança: confirmado (decompile).

## 0x08021038 S 0x08 -> Fight_DispatchOnSubstateAA_S_08_LoadGfxByMode
- Passo: carrega gráficos conforme o modo (fi+0x51, valores 0..3); modo 0/1 e 3 configuram slot, modo 2 ajusta BG; timer 0x3C.
- Lê/escreve: lê fi+0x51; escreve BG (0x0840 region), +0x86, +0xAA.
- Próximo: 0x09. Confiança: confirmado (decompile).

## 0x080211C8 S 0x09 -> Fight_DispatchOnSubstateAA_S_09_StepSpriteByteTimerByMode
- Passo: soma 8 a um byte de atributo do sprite por frame (DAT_080211F4 + DAT_080211F8; provável gObjectSlots +0x848). Quando o byte chega a 0x40: se fi+0x51 == 3, timer 0x03000440+0x86 = 1; senão = 0x3C (ctx, não fi; FI2). Avança.
- Lê/escreve: fi+0x51; escreve o byte do sprite, timer 0x03000440+0x86 (ctx, não fi+0x86; FI/FI2), fi+0xAA.
- Próximo: 0x0A (+1). Confiança: confirmado (decompile). Endereço exato do byte: provável.

## 0x0802121C S 0x0A -> Fight_DispatchOnSubstateAA_S_0A_TimerThenBranchByMode
- Passo: conta o timer (ctx 0x03000440+0x86, não fi+0x86; FI2); quando zera, se modo (fi+0x51) < 3 zera um byte e vai para 0x0B; se modo == 3 toca SFX 0x52 e vai para 0x20.
- Lê/escreve: lê fi+0x51, fi+0x89 e o timer 0x03000440+0x86 (ctx, não fi+0x86; FI2); escreve o timer 0x03000440+0x86 e +0xAA.
- Próximo: 0x0B ou 0x20. Confiança: confirmado (decompile).

## 0x080212A0 S 0x0B -> Fight_DispatchOnSubstateAA_S_0B_StepSpriteUntil10
- Passo: se o byte de estado do sprite (DAT_080212B4 + DAT_080212B8; provável gObjectSlots +0x867) == 0x10: avança. Senão: move a posição do sprite em 1 px (sentido pelo bit 0 de fi+0xF8) e incrementa o contador do objeto.
- Lê/escreve: fi+0xF8, byte de estado do sprite, contador do objeto, fi+0xAA.
- Próximo: 0x0C quando o byte chega a 0x10; senão fica em 0x0B. Confiança: confirmado (decompile). Endereço do byte: provável.

## 0x08021300 S 0x0C -> Fight_DispatchOnSubstateAA_S_0C_CheckPartThenBranch
- Passo: testa a flag da peça (fi+0x8A slot, +0x2A e +0x2B contra fi+0x52); se ok zera byte e vai para 0x0D; senão, modo 2 vai para 0x14, outros tocam SFX 0x3E e vão para 0x0E.
- Próximo: 0x0D, 0x0E ou 0x14. Confiança: confirmado (decompile).

## 0x08021370 S 0x0D -> Fight_DispatchOnSubstateAA_S_0D_StepSpriteUntil10BranchMode
- Passo: se o byte de estado do sprite == 0x10: se fi+0x51 == 2, +0xAA = 0x14; senão toca SFX 0x3E e +0xAA = 0x0E. Se não: se o byte de teste é menor que 8, soma 8 a um campo do sprite; senão subtrai 8 de outro campo; incrementa o contador do objeto.
- Lê/escreve: fi+0x51, byte de estado do sprite, dois campos de posição do sprite, contador do objeto, fi+0xAA.
- Próximo: 0x14 (modo 2) ou 0x0E; fica em 0x0D até o byte chegar a 0x10. Confiança: confirmado (fluxo e ramificação por modo); papel dos dois campos de posição: provável.
- Obs: a nota antiga falava de BL 0x08074DE0 e 0x08040CD0; essas chamadas não aparecem no decompile.

## 0x080213E4 S 0x0E -> Fight_DispatchOnSubstateAA_S_0E_CountdownClearSlots
- Passo: contagem regressiva de um byte em -8 até 8; ao chegar, Tile_ClearSlots(0x21,1) e vai para 0x0F.
- Próximo: 0x0F (ou fica em 0x0E). Confiança: confirmado (decompile).

## 0x08021418 S 0x0F -> Fight_DispatchOnSubstateAA_S_0F_RecordDamageAndJump
- Passo: grava o registro de dano do alvo (fi+0x8A seleciona o slot de 8 bytes em +0x2A..+0x2D); usa Fight_AddByteClampToMax com dano e bônus (+0xE8, +0xEC); modo 2 tem variante; no fim pula por tabela (não recuperada).
- Próximo: 0x10 e salto por tabela. Confiança: confirmado (decompile).

## 0x08021564 S 0x10 -> Fight_DispatchOnSubstateAA_S_10_SetMsgByMode
- Passo: define a mensagem +0xA6 pelo modo fi+0x51: 0 -> 0x1E; 1 -> 0x1F; 2 -> 0x20; 3 -> 0x21 (outros valores não mudam). Se fi+0x53 != 0, +0xA6 = 0x71. Avança.
- Lê/escreve: fi+0x51, fi+0x53; escreve fi+0xA6, fi+0xAA.
- Próximo: 0x11 (+1). Confiança: confirmado (decompile). O passo 0x11 do despachante S ainda não tem seção.
- Obs: a nota antiga dizia "próximo desconhecido"; o decompile mostra +1.

## 0x080215C8 S 0x14 -> Fight_DispatchOnSubstateAA_S_14_SetFlagsSfx40
- Passo: grava estado de 4 em um byte do bloco de estado, zera 0x870, toca SFX 0x40.
- Próximo: 0x15. Confiança: confirmado (decompile).

## 0x08021608 S 0x15 -> Fight_DispatchOnSubstateAA_S_15_StepCoordDownClearSlot
- Passo: Fight_StepSlotCoordDownInRange(0x21, -0x40, -0x30) (mtbe: passo decrescente de coordenada do slot, dentro do limite). Quando retorna não zero: Tile_ClearSlots(0x21, 1) e avança.
- Lê/escreve: objeto do slot 0x21 (via a função chamada); fi+0xAA.
- Próximo: 0x16 quando termina; senão fica em 0x15. Confiança: confirmado (decompile).
- Obs: a nota antiga falava de carga de gráfico (0x08040BD8, dado 0x083B6100); não aparece no decompile.

## 0x08021638 S 0x20 -> Fight_DispatchOnSubstateAA_S_20_WaitSlotAnimTimer
- Passo: espera a animação do slot 0x21 (record +0xE0); depois timer 0x3C.
- Próximo: 0x21. Confiança: confirmado (decompile).

## 0x0802167C S 0x21 -> Fight_DispatchOnSubstateAA_S_21_SetupSpritesAfterTimer
- Passo: conta o timer (ctx 0x03000440+0x86, não fi+0x86; FI2); ao zerar, copia tiles (FUN_0803F63C) e posiciona sprites (0x58, 0x40, 0x28; x 0x88/0x68 pela flag de tela).
- Próximo: 0x22. Confiança: confirmado (decompile).

## 0x08021734 S 0x22 -> Fight_DispatchOnSubstateAA_S_22_StepThreeSlotsTowardTarget
- Passo: chama Fight_StepSlotTowardTarget nos slots 0x21, 0x22 e 0x23 (um passo cada) e conta quantos retornaram não zero. Com a contagem igual a 3, avança.
- Lê/escreve: objetos dos slots 0x21..0x23 (via a função chamada); fi+0xAA.
- Próximo: 0x23 quando os três chegam; senão fica em 0x22. Confiança: confirmado (decompile).
- Obs: a nota antiga falava de 0x08074DE0; não aparece no decompile.

## 0x08021774 S 0x23 -> Fight_DispatchOnSubstateAA_S_23_CheckFlagOrJump25
- Passo: testa flag do slot (fi+0x8A, +0x2A, +0x2B); se falhou, SFX 0x3E e pula direto para 0x25; se ok, zera byte e vai para 0x24.
- Próximo: 0x24 ou 0x25 (salto). Confiança: confirmado (decompile).

## 0x080217D8 S 0x24 -> Fight_DispatchOnSubstateAA_S_24_MoveSpriteStep
- Passo: um passo de movimento de sprite (+8 ou -8 em dois bytes); se o byte chegou a 0x10, toca SFX 0x3E.
- Próximo: sempre 0x25. Confiança: confirmado (decompile).

## 0x0802186C S 0x25 -> Fight_DispatchOnSubstateAA_S_25_ClearSlotsAfterMove
- Passo: move até o byte chegar a 8; então Tile_ClearSlots(0x21,3).
- Próximo: 0x26 quando termina; senão fica em 0x25. Confiança: confirmado (decompile).

## 0x080218E4 T 0x00 -> Fight_DispatchOnSubstateAA_T_00_PenaltyRecordFlagBranch
- Passo: Fight_AccumRosPart(fi+0x87, fi+0xA7), Fight_ApplyStatusCostIfFlag(fi+0x87), FUN_0803725C. Se FUN_0803725C != 0: +0xA6 = 0x1A e avança (+1). Se for 0: se o byte de registro de dano do alvo (fi+0x2A + fi+0x89*8) é 0: +0xA6 = 0x24 e avança (+1). Senão: Fight_IncrementActionUsageCounter_08036270 e +0xAA = 4.
- Lê/escreve: fi+0x87, fi+0x89, fi+0x2A (registro de dano de 8 bytes por slot, como em S 0x0F), fi+0xA6, fi+0xAA.
- Próximo: 0x01 (caminho de mensagem) ou 0x04 (caminho com efeito). Confiança: confirmado (decompile).
- Obs: a nota antiga (gráfico LZ 0x084D6A5C, paleta 0x0850FC38, salto para 0x08) não confere com o decompile. Ver a pendência sobre o pós-batalha no fim do arquivo.

## 0x08021950 T 0x08 -> Fight_DispatchOnSubstateAA_T_08_LoadGfxAndSlot
- Passo: descompacta gráficos, configura slot 0x21 (record +0xE4); se flag de fi bit0, marca +0x840 |= 0x1000.
- Próximo: 0x09. Confiança: confirmado (decompile).

---

## Esboço da máquina de estados (+0xAA), por despachante

- Entrada do bot (L): 0x0F (gráficos, SFX 0x2B) -> 0x10 (espera anim) -> 0x11.
- Efeito de dano (M): 0x00 (gráficos + objeto) -> 0x01 (espera o retorno de 0x080370AC) -> 0x02 (posiciona) -> 0x03 (espera anim) -> 0x04 (dano na peça) -> 0x05 (efeito, +0xA6=0x18) -> 0x06.
- Reparo (P/R): P 0x0B tenta reparar; se conseguiu vai para 0x0E, senão 0x0C -> R 0x0C divide contagem -> 0x0F, ou pula para 0x0D. P 0x01 aplica o reparo -> 0x02. R 0x01 divide a contagem -> 0x02. P 0x11 e R 0x12 apenas escrevem 0x02 em +0xAA. R 0x08 carrega ícones e pula por tabela para 0x09 (tabela não recuperada).
- Sequência de hit (S): 0x00 (penalidade; define o modo fi+0x51) -> 0x04 (SFX 0x29) -> 0x05 (move sprite do alvo) -> 0x06 -> 0x07 (espera) -> 0x08 (gráficos por modo) -> 0x09 (byte do sprite +8; timer) -> 0x0A (timer: modo 3 vai para 0x20, senão 0x0B) -> 0x0B (move até o byte 0x10) -> 0x0C (flag: ok -> 0x0D, senão modo 2 -> 0x14 ou 0x0E) -> 0x0D (move até 0x10: modo 2 -> 0x14, senão 0x0E) -> 0x0E (contagem) -> 0x0F (registra dano; salto por tabela) -> 0x10 (mensagem por modo) -> 0x11 -> 0x14 -> 0x15 (coordenada; limpa slot) -> 0x16 -> 0x20 (espera) -> 0x21 (timer, sprites) -> 0x22 (três slots chegam) -> 0x23 (flag: falhou -> 0x25) -> 0x24 -> 0x25 (move até 8) -> 0x26.
- Pós-batalha (T): 0x00 (penalidade; msg 0x1A ou 0x24; sem efeito -> +1; com efeito -> 0x04). T 0x08 (gráfico + slot) -> 0x09. Os passos T 0x01..0x07 não foram documentados.

Pendências (após GH-HA4-redo): a subtabela 0x083ADA58 não é chamada por O 0x00 nem Q 0x00 (confirmar onde o stub a usa); a jump table de R 0x08 e de S 0x0F (não recuperadas); T 0x01..0x07 (fluxo pós-batalha); origem de fi+0x8B (peça escolhida, que P 0x01 e R 0x01 apenas copiam); significado de +0xAB, do byte +0x49 da peça (modo do S 0x00) e do byte +0x59 (flag de Q 0x00); retorno de 0x080370AC (M 0x01). Conflitos com mtbe.csv vistos nesta rodada: 0x0803579C (Ghidra: Fight_AccumRosPart; mtbe: Fight_GetPartProficiencyBase) e 0x08036FF8 (mtbe: Fight_SpawnInlineEffectTextObjects; o uso aqui parece criação de sprite). Não editei mtbe.csv.
