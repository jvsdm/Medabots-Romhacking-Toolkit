# SU: despachantes Fight_DispatchOnSubstateAA_S, _U, _V, _W, _X, _Y, _AC, _AD

Passos por despachante (literal stub+0x24 = tabela de handlers indexada por +0xAA; entrada de 4 bytes).
Fonte: seções já existentes em HN1.md, HA4.md, HA5.md, HA6.md (confirmado no decompile em cada uma).
Passo "compartilhado" = mesmo handler usado por outros despachantes (ex.: 0x08020DF8 = passo 04 de S, U, X, Y, AC, AD).
"Próximo" = valor gravado em +0xAA quando o passo termina, quando o texto da seção diz; senão **Pendente**
(não reconfirmado no decompile nesta rodada). Renomeação no Ghidra: **Pendente** (ver fim do arquivo).

Gancho do despachante: 0x03001800 +0xAA = passo; +0xB5 = handler concluído (zerado pelo stub).

## Despachante S
- Resumo (3 linhas; opcode 0x29 ainda pendente de confirmação):
  1. Abre com ROS e custo de status; carrega gráficos e anima sprites do alvo conforme o modo (fi+0x51).
  2. O passo 0x0F grava o registro de dano do alvo (fi+0x8A) com bônus; mensagens 0x1E..0x21 pelo modo.
  3. Os passos 0x14..0x25 são animação de sprite e slots; o efeito final não foi reconfirmado no decompile.


| Passo | Endereço | Nome (seção) | Efeito (resumo da seção) | Próximo +0xAA |
|---|---|---|---|---|
| S 0x00 | 0x08020D24 | Fight_DispatchOnSubstateAA_S_00_PenaltyComputeModeCopyPart | Fight_AccumRosPart(fi+0x87, fi+0xA7), Fight_ApplyStatusCostIfFlag(fi+0x87), FUN_0803725C. Se FUN_0803725C != 0: +0xA6 = 0x1A e avança. Senão: FUN_0803 | **Pendente** |
| S 0x01 | 0x08030608 | ActionFamily_TrapFamily_Handler (compartilhado: AC,AD,S,U,V,W,X,Y) |  - Índice 0x01 de 25 despachantes (B, AA..AN, C, N..Z). Chama `Fight_RunSubstateDispatchB()`; retorno != 0: +0xAA++. - Confiança: confirmado. | +0xAA++ |
| S 0x02 | 0x080305E0 | ActionFamily_StatusCheckFamily_Handler (compartilhado: AC,AD,S,U,V,W,X,Y) |  - Índice 0x01 de ~48 despachantes (A, AA..BH, C..Z). Chama `Fight_RunSubstateDispatch(fi.A6)`; retorno != 0: +0xAA++. - É o passo genérico que roda o | +0xAA++ |
| S 0x03 | 0x0803080C | Fight_DispatchOnSubstateAA_Shared_SetDone (compartilhado: AC,AD,S,U,V,W,X,Y) | só faz `0x03001800+0xB5 = 1` (handler concluído). Não mexe em +0xAA. | +0xAA |
| S 0x04 | 0x08020DF8 | Fight_DispatchOnSubstateAA_S_04_LoadGfxAndSfx29 (compartilhado: AC,AD,S,U,X,Y) | carrega gráficos e posiciona o sprite do alvo (tabela por +0x87), toca SFX 0x29. | **Pendente** |
| S 0x05 | 0x08020EDC | Fight_DispatchOnSubstateAA_S_05_StepTargetSpriteAnim (compartilhado: AC,AD,S,U,X,Y) | Fight_StepSlotAnimOffsetTable(0x21) (mtbe: avança a tabela de passos de animação do slot). Se o contador do objeto do slot 0x21 (gObjectSlots + DAT_08 | **Pendente** |
| S 0x06 | 0x08020FB8 | Fight_DispatchOnSubstateAA_S_06_LoadGfxAndSlots (compartilhado: AC,AD,S,U,X,Y) | carrega gráficos e configura slot 0x21 (record list +0x8C). | **Pendente** |
| S 0x07 | 0x08021000 | Fight_DispatchOnSubstateAA_S_07_WaitSlotAnim (compartilhado: AC,AD,S,U,X,Y) | espera a animação do slot 0x21 (record +0x8C). | **Pendente** |
| S 0x08 | 0x08021038 | Fight_DispatchOnSubstateAA_S_08_LoadGfxByMode | carrega gráficos conforme o modo (fi+0x51, valores 0..3); modo 0/1 e 3 configuram slot, modo 2 ajusta BG; timer 0x3C. | **Pendente** |
| S 0x09 | 0x080211C8 | Fight_DispatchOnSubstateAA_S_09_StepSpriteByteTimerByMode | soma 8 a um byte de atributo do sprite por frame (DAT_080211F4 + DAT_080211F8; provável gObjectSlots +0x848). Quando o byte chega a 0x40: se fi+0x51 = | **Pendente** |
| S 0x0A | 0x0802121C | Fight_DispatchOnSubstateAA_S_0A_TimerThenBranchByMode | conta o timer (ctx 0x03000440+0x86, não fi+0x86; FI2); quando zera, se modo (fi+0x51) < 3 zera um byte e vai para 0x0B; se modo == 3 toca SFX 0x52 e v | **Pendente** |
| S 0x0B | 0x080212A0 | Fight_DispatchOnSubstateAA_S_0B_StepSpriteUntil10 | se o byte de estado do sprite (DAT_080212B4 + DAT_080212B8; provável gObjectSlots +0x867) == 0x10: avança. Senão: move a posição do sprite em 1 px (se | **Pendente** |
| S 0x0C | 0x08021300 | Fight_DispatchOnSubstateAA_S_0C_CheckPartThenBranch | testa a flag da peça (fi+0x8A slot, +0x2A e +0x2B contra fi+0x52); se ok zera byte e vai para 0x0D; senão, modo 2 vai para 0x14, outros tocam SFX 0x3E | **Pendente** |
| S 0x0D | 0x08021370 | Fight_DispatchOnSubstateAA_S_0D_StepSpriteUntil10BranchMode | se o byte de estado do sprite == 0x10: se fi+0x51 == 2, +0xAA = 0x14; senão toca SFX 0x3E e +0xAA = 0x0E. Se não: se o byte de teste é menor que 8, so | +0xAA = 0x14; senão toca SFX 0x3E e |
| S 0x0E | 0x080213E4 | Fight_DispatchOnSubstateAA_S_0E_CountdownClearSlots | contagem regressiva de um byte em -8 até 8; ao chegar, Tile_ClearSlots(0x21,1) e vai para 0x0F. | **Pendente** |
| S 0x0F | 0x08021418 | Fight_DispatchOnSubstateAA_S_0F_RecordDamageAndJump | grava o registro de dano do alvo (fi+0x8A seleciona o slot de 8 bytes em +0x2A..+0x2D); usa Fight_AddByteClampToMax com dano e bônus (+0xE8, +0xEC); m | **Pendente** |
| S 0x10 | 0x08021564 | Fight_DispatchOnSubstateAA_S_10_SetMsgByMode | define a mensagem +0xA6 pelo modo fi+0x51: 0 -> 0x1E; 1 -> 0x1F; 2 -> 0x20; 3 -> 0x21 (outros valores não mudam). Se fi+0x53 != 0, +0xA6 = 0x71. Avanç | **Pendente** |
| S 0x11 | 0x080305E0 | ActionFamily_StatusCheckFamily_Handler (compartilhado: AC,AD,S,U,V,W,X,Y) |  - Índice 0x01 de ~48 despachantes (A, AA..BH, C..Z). Chama `Fight_RunSubstateDispatch(fi.A6)`; retorno != 0: +0xAA++. - É o passo genérico que roda o | +0xAA++ |
| S 0x12 | 0x0803080C | Fight_DispatchOnSubstateAA_Shared_SetDone (compartilhado: AC,AD,S,U,V,W,X,Y) | só faz `0x03001800+0xB5 = 1` (handler concluído). Não mexe em +0xAA. | +0xAA |
| S 0x14 | 0x080215C8 | Fight_DispatchOnSubstateAA_S_14_SetFlagsSfx40 | grava estado de 4 em um byte do bloco de estado, zera 0x870, toca SFX 0x40. | **Pendente** |
| S 0x15 | 0x08021608 | Fight_DispatchOnSubstateAA_S_15_StepCoordDownClearSlot | Fight_StepSlotCoordDownInRange(0x21, -0x40, -0x30) (mtbe: passo decrescente de coordenada do slot, dentro do limite). Quando retorna não zero: Tile_Cl | **Pendente** |
| S 0x16 | 0x08021418 | Fight_DispatchOnSubstateAA_S_0F_RecordDamageAndJump | grava o registro de dano do alvo (fi+0x8A seleciona o slot de 8 bytes em +0x2A..+0x2D); usa Fight_AddByteClampToMax com dano e bônus (+0xE8, +0xEC); m | **Pendente** |
| S 0x17 | 0x08021564 | Fight_DispatchOnSubstateAA_S_10_SetMsgByMode | define a mensagem +0xA6 pelo modo fi+0x51: 0 -> 0x1E; 1 -> 0x1F; 2 -> 0x20; 3 -> 0x21 (outros valores não mudam). Se fi+0x53 != 0, +0xA6 = 0x71. Avanç | **Pendente** |
| S 0x18 | 0x080305E0 | ActionFamily_StatusCheckFamily_Handler (compartilhado: AC,AD,S,U,V,W,X,Y) |  - Índice 0x01 de ~48 despachantes (A, AA..BH, C..Z). Chama `Fight_RunSubstateDispatch(fi.A6)`; retorno != 0: +0xAA++. - É o passo genérico que roda o | +0xAA++ |
| S 0x19 | 0x0803080C | Fight_DispatchOnSubstateAA_Shared_SetDone (compartilhado: AC,AD,S,U,V,W,X,Y) | só faz `0x03001800+0xB5 = 1` (handler concluído). Não mexe em +0xAA. | +0xAA |
| S 0x20 | 0x08021638 | Fight_DispatchOnSubstateAA_S_20_WaitSlotAnimTimer | espera a animação do slot 0x21 (record +0xE0); depois timer 0x3C. | **Pendente** |
| S 0x21 | 0x0802167C | Fight_DispatchOnSubstateAA_S_21_SetupSpritesAfterTimer | conta o timer (ctx 0x03000440+0x86, não fi+0x86; FI2); ao zerar, copia tiles (FUN_0803F63C) e posiciona sprites (0x58, 0x40, 0x28; x 0x88/0x68 pela fl | **Pendente** |
| S 0x22 | 0x08021734 | Fight_DispatchOnSubstateAA_S_22_StepThreeSlotsTowardTarget | chama Fight_StepSlotTowardTarget nos slots 0x21, 0x22 e 0x23 (um passo cada) e conta quantos retornaram não zero. Com a contagem igual a 3, avança. | **Pendente** |
| S 0x23 | 0x08021774 | Fight_DispatchOnSubstateAA_S_23_CheckFlagOrJump25 | testa flag do slot (fi+0x8A, +0x2A, +0x2B); se falhou, SFX 0x3E e pula direto para 0x25; se ok, zera byte e vai para 0x24. | **Pendente** |
| S 0x24 | 0x080217D8 | Fight_DispatchOnSubstateAA_S_24_MoveSpriteStep | um passo de movimento de sprite (+8 ou -8 em dois bytes); se o byte chegou a 0x10, toca SFX 0x3E. | **Pendente** |
| S 0x25 | 0x0802186C | Fight_DispatchOnSubstateAA_S_25_ClearSlotsAfterMove | move até o byte chegar a 8; então Tile_ClearSlots(0x21,3). | **Pendente** |
| S 0x26 | 0x08021418 | Fight_DispatchOnSubstateAA_S_0F_RecordDamageAndJump | grava o registro de dano do alvo (fi+0x8A seleciona o slot de 8 bytes em +0x2A..+0x2D); usa Fight_AddByteClampToMax com dano e bônus (+0xE8, +0xEC); m | **Pendente** |
| S 0x27 | 0x08021564 | Fight_DispatchOnSubstateAA_S_10_SetMsgByMode | define a mensagem +0xA6 pelo modo fi+0x51: 0 -> 0x1E; 1 -> 0x1F; 2 -> 0x20; 3 -> 0x21 (outros valores não mudam). Se fi+0x53 != 0, +0xA6 = 0x71. Avanç | **Pendente** |
| S 0x28 | 0x080305E0 | ActionFamily_StatusCheckFamily_Handler (compartilhado: AC,AD,S,U,V,W,X,Y) |  - Índice 0x01 de ~48 despachantes (A, AA..BH, C..Z). Chama `Fight_RunSubstateDispatch(fi.A6)`; retorno != 0: +0xAA++. - É o passo genérico que roda o | +0xAA++ |
| S 0x29 | 0x0803080C | Fight_DispatchOnSubstateAA_Shared_SetDone (compartilhado: AC,AD,S,U,V,W,X,Y) | só faz `0x03001800+0xB5 = 1` (handler concluído). Não mexe em +0xAA. | +0xAA |

## Despachante U
- Resumo (3 linhas; opcode 0x2B, efeito provável):
  1. Abertura de ROS; se o alvo não tem status, cai na mensagem 0x70 (nada a fazer).
  2. Zera o status do alvo (+0xC9..+0xCC) e roda um efeito de slot 0x21 sobre o alvo.
  3. Termina com mensagem 0x23 (passo 0x0D).


| Passo | Endereço | Nome (seção) | Efeito (resumo da seção) | Próximo +0xAA |
|---|---|---|---|---|
| U 0x00 | 0x08021E2C | Fight_DispatchOnSubstateAA_U_00_CheckRosAndTargetStatus | abertura da técnica de U (ROS). Falha: +0xA6=0x1A, +1. Passa: se alvo +0xC9 == 0 (sem status) grava +0xA6=0x70 e +1 (mensagem "nada a fazer", provável | +1 |
| U 0x01 | 0x08030608 | ActionFamily_TrapFamily_Handler (compartilhado: AC,AD,S,U,V,W,X,Y) |  - Índice 0x01 de 25 despachantes (B, AA..AN, C, N..Z). Chama `Fight_RunSubstateDispatchB()`; retorno != 0: +0xAA++. - Confiança: confirmado. | +0xAA++ |
| U 0x02 | 0x080305E0 | ActionFamily_StatusCheckFamily_Handler (compartilhado: AC,AD,S,U,V,W,X,Y) |  - Índice 0x01 de ~48 despachantes (A, AA..BH, C..Z). Chama `Fight_RunSubstateDispatch(fi.A6)`; retorno != 0: +0xAA++. - É o passo genérico que roda o | +0xAA++ |
| U 0x03 | 0x0803080C | Fight_DispatchOnSubstateAA_Shared_SetDone (compartilhado: AC,AD,S,U,V,W,X,Y) | só faz `0x03001800+0xB5 = 1` (handler concluído). Não mexe em +0xAA. | +0xAA |
| U 0x04 | 0x08020DF8 | Fight_DispatchOnSubstateAA_S_04_LoadGfxAndSfx29 (compartilhado: AC,AD,S,U,X,Y) | carrega gráficos e posiciona o sprite do alvo (tabela por +0x87), toca SFX 0x29. | **Pendente** |
| U 0x05 | 0x08020EDC | Fight_DispatchOnSubstateAA_S_05_StepTargetSpriteAnim (compartilhado: AC,AD,S,U,X,Y) | Fight_StepSlotAnimOffsetTable(0x21) (mtbe: avança a tabela de passos de animação do slot). Se o contador do objeto do slot 0x21 (gObjectSlots + DAT_08 | **Pendente** |
| U 0x06 | 0x08020FB8 | Fight_DispatchOnSubstateAA_S_06_LoadGfxAndSlots (compartilhado: AC,AD,S,U,X,Y) | carrega gráficos e configura slot 0x21 (record list +0x8C). | **Pendente** |
| U 0x07 | 0x08021000 | Fight_DispatchOnSubstateAA_S_07_WaitSlotAnim (compartilhado: AC,AD,S,U,X,Y) | espera a animação do slot 0x21 (record +0x8C). | **Pendente** |
| U 0x08 | 0x08021EA0 | Fight_DispatchOnSubstateAA_U_08_LoadEffectGfxOnTarget | carrega gráfico/paleta do efeito, liga bit 0x200 (e 0x1000 se +0xF8&1) no slot 0x21, posiciona sobre o alvo (+0x88), `Slot_SetupFromRecordList(.., 0x2 | **Pendente** |
| U 0x09 | 0x08030674 | Fight_DispatchOnSubstateAA_Shared_WaitTimerOrSkip |  - Índice 0x0E de 19 despachantes (T, U, Z, AA..BH). Igual a 0x08030624 sem limpar slot: espera +0x86 ou pula; +0xAA++. - Confiança: confirmado. | +0xAA++ |
| U 0x0A | 0x08021F90 | Fight_DispatchOnSubstateAA_U_0A_ScrollBgAndStepSlotToTarget | rola um registro de tela 0x40 por quadro (sinal por +0xF8&1) e anda o slot 0x21 até o alvo (`Fight_StepSlotTowardTarget`). Ao chegar: limpa bits do sl | **Pendente** |
| U 0x0B | 0x08022010 | Fight_DispatchOnSubstateAA_U_0B_ClearTargetStatus | efeito de U: zera +0xC9..+0xCC do bot alvo (status/condição; provável "cura de status"), liga bits no slot do alvo (0x47+alvo*3), escala 0x40, SFX 0x3 | **Pendente** |
| U 0x0C | 0x080220A0 | Fight_DispatchOnSubstateAA_U_0C_RunAnimAndClearTargetSlot | encolhe/ajusta escala do slot do alvo até 4, roda a anim do slot 0x21; quando o script termina (-1) limpa slot 0x21 e o do alvo. Próximo: +1. | Próximo: +1 |
| U 0x0D | 0x0802213C | Fight_DispatchOnSubstateAA_U_0D_SetMessage23 | final de U: +0xA6 = 0x23, +1. | +1 |
| U 0x0E | 0x080305E0 | ActionFamily_StatusCheckFamily_Handler (compartilhado: AC,AD,S,U,V,W,X,Y) |  - Índice 0x01 de ~48 despachantes (A, AA..BH, C..Z). Chama `Fight_RunSubstateDispatch(fi.A6)`; retorno != 0: +0xAA++. - É o passo genérico que roda o | +0xAA++ |
| U 0x0F | 0x0803080C | Fight_DispatchOnSubstateAA_Shared_SetDone (compartilhado: AC,AD,S,U,V,W,X,Y) | só faz `0x03001800+0xB5 = 1` (handler concluído). Não mexe em +0xAA. | +0xAA |

## Despachante V
- Resumo (3 linhas; opcode 0x2C):
  1. Abertura de ROS; cria 4 sprites (slots 0x21..0x24) em volta do ator.
  2. Gira e encolhe os sprites; depois alterna o flag +0x58 com mensagens 0x72/0x73. O sentido do flag é chute.
  3. Sem dano nem status.


| Passo | Endereço | Nome (seção) | Efeito (resumo da seção) | Próximo +0xAA |
|---|---|---|---|---|
| V 0x00 | 0x0802217C | Fight_DispatchOnSubstateAA_V_00_CheckRos | abertura comum (ROS). Falha: +0xA6=0x1A, +1. Passa: incrementa uso, +0xAA=4. | +1 |
| V 0x01 | 0x08030608 | ActionFamily_TrapFamily_Handler (compartilhado: AC,AD,S,U,V,W,X,Y) |  - Índice 0x01 de 25 despachantes (B, AA..AN, C, N..Z). Chama `Fight_RunSubstateDispatchB()`; retorno != 0: +0xAA++. - Confiança: confirmado. | +0xAA++ |
| V 0x02 | 0x080305E0 | ActionFamily_StatusCheckFamily_Handler (compartilhado: AC,AD,S,U,V,W,X,Y) |  - Índice 0x01 de ~48 despachantes (A, AA..BH, C..Z). Chama `Fight_RunSubstateDispatch(fi.A6)`; retorno != 0: +0xAA++. - É o passo genérico que roda o | +0xAA++ |
| V 0x03 | 0x0803080C | Fight_DispatchOnSubstateAA_Shared_SetDone (compartilhado: AC,AD,S,U,V,W,X,Y) | só faz `0x03001800+0xB5 = 1` (handler concluído). Não mexe em +0xAA. | +0xAA |
| V 0x04 | 0x080221C8 | Fight_DispatchOnSubstateAA_V_04_SpawnFourOrbsOnActor | carrega gráfico, cria 4 sprites (slots 0x21..0x24) na posição do ator, ângulos 0/0x40/0x80/0xC0, SFX 0x5C. +1. | +1 |
| V 0x05 | 0x080222A8 | Fight_DispatchOnSubstateAA_V_05_SpiralOrbsAroundActor | gira os 4 sprites em volta do ator (`CompositeSprite_ComputeOffsetWord`, ângulo +4, raio +1 a cada 4 quadros; após 0xB4 encolhe). Quando a escala cheg | +1 a cada 4 quadros; a |
| V 0x06 | 0x08022380 | Fight_DispatchOnSubstateAA_V_06_ToggleFlag58Message | final de V: alterna o flag +0x58 do bloco (0->1 com +0xA6=0x72; 1->0 com +0xA6=0x73). +1. | +1 |
| V 0x07 | 0x080305E0 | ActionFamily_StatusCheckFamily_Handler (compartilhado: AC,AD,S,U,V,W,X,Y) |  - Índice 0x01 de ~48 despachantes (A, AA..BH, C..Z). Chama `Fight_RunSubstateDispatch(fi.A6)`; retorno != 0: +0xAA++. - É o passo genérico que roda o | +0xAA++ |
| V 0x08 | 0x0803080C | Fight_DispatchOnSubstateAA_Shared_SetDone (compartilhado: AC,AD,S,U,V,W,X,Y) | só faz `0x03001800+0xB5 = 1` (handler concluído). Não mexe em +0xAA. | +0xAA |

## Despachante W
- Resumo (3 linhas; opcode 0x2D, efeito provável):
  1. Abertura de ROS com variante pela técnica da peça (+0x49).
  2. Dois sprites giram para dentro do ator; no fim o ator ganha carga (bot +0xBD |= 8, +0xEC até 0xFF).
  3. Termina com mensagem 0x1B.


| Passo | Endereço | Nome (seção) | Efeito (resumo da seção) | Próximo +0xAA |
|---|---|---|---|---|
| W 0x00 | 0x080223E4 | Fight_DispatchOnSubstateAA_W_00_CheckRosPickVariant | abertura comum. Se passa: variante +0x51 = 0 se técnica da peça (+0x49) == 0x1B, 1 se 0x1C, 2 senão; `Fight_ComputePartValue`; +0xAA=4. Falha: +0xA6=0 | +0xAA=4 |
| W 0x01 | 0x08030608 | ActionFamily_TrapFamily_Handler (compartilhado: AC,AD,S,U,V,W,X,Y) |  - Índice 0x01 de 25 despachantes (B, AA..AN, C, N..Z). Chama `Fight_RunSubstateDispatchB()`; retorno != 0: +0xAA++. - Confiança: confirmado. | +0xAA++ |
| W 0x02 | 0x080305E0 | ActionFamily_StatusCheckFamily_Handler (compartilhado: AC,AD,S,U,V,W,X,Y) |  - Índice 0x01 de ~48 despachantes (A, AA..BH, C..Z). Chama `Fight_RunSubstateDispatch(fi.A6)`; retorno != 0: +0xAA++. - É o passo genérico que roda o | +0xAA++ |
| W 0x03 | 0x0803080C | Fight_DispatchOnSubstateAA_Shared_SetDone (compartilhado: AC,AD,S,U,V,W,X,Y) | só faz `0x03001800+0xB5 = 1` (handler concluído). Não mexe em +0xAA. | +0xAA |
| W 0x04 | 0x08022480 | Fight_DispatchOnSubstateAA_W_04_LoadVariantGfxTwoSprites | carrega um de 3 gráficos pela variante +0x51, cria 2 sprites (slots 0x21/0x22) a ±0x20 do ator, SFX 0x33. +1. | +1 |
| W 0x05 | 0x08022648 | Fight_DispatchOnSubstateAA_W_05_MoveSpritesInward | move os 2 sprites em volta do ator (ângulo +8, raio -1 a cada 4 quadros, escala até 0x40). Raio 0: SFX 0x30, limpa slot 0x22, +1. | +1 |
| W 0x06 | 0x08022708 | Fight_DispatchOnSubstateAA_W_06_RunAnimUntilDone | roda a anim do slot 0x21; quando termina (-1) limpa o slot, +1. | +1 |
| W 0x07 | 0x08022748 | Fight_DispatchOnSubstateAA_W_07_ApplyActorChargeBuff | efeito de W no ator: bot +0xBD /= 8, +0xDF = técnica da peça, +0xD8 = 0 e soma +0xEC com teto 0xFF (`Fight_AddByteClampToMax`). Buff/carga no próprio  | **Pendente** |
| W 0x08 | 0x080227C0 | Fight_DispatchOnSubstateAA_W_08_SetMessage1B | final de W: +0xA6 = 0x1B, +1. | +1 |
| W 0x09 | 0x080305E0 | ActionFamily_StatusCheckFamily_Handler (compartilhado: AC,AD,S,U,V,W,X,Y) |  - Índice 0x01 de ~48 despachantes (A, AA..BH, C..Z). Chama `Fight_RunSubstateDispatch(fi.A6)`; retorno != 0: +0xAA++. - É o passo genérico que roda o | +0xAA++ |
| W 0x0A | 0x0803080C | Fight_DispatchOnSubstateAA_Shared_SetDone (compartilhado: AC,AD,S,U,V,W,X,Y) | só faz `0x03001800+0xB5 = 1` (handler concluído). Não mexe em +0xAA. | +0xAA |

## Despachante X
- Resumo (3 linhas; opcode 0x2E, status provável):
  1. Abertura de ROS; bloqueia (0x39) se o alvo tem peça com técnica 0x16.
  2. Coloca o status 0x1F no alvo (+0xC4) e anel de 5 sprites girando, depois encolhe.
  3. Termina com mensagem 0x29.


| Passo | Endereço | Nome (seção) | Efeito (resumo da seção) | Próximo +0xAA |
|---|---|---|---|---|
| X 0x00 | 0x08022800 | Fight_DispatchOnSubstateAA_X_00_CheckRosAndTargetTech16 | abertura comum. Se passa e o alvo tem peça íntegra com técnica 0x16 (`Fight_HasNoTechnique16Part` == 0): +0xA6=0x39, +1 (bloqueado). Senão: uso, `Figh | +1 (bloqueado) |
| X 0x01 | 0x08030608 | ActionFamily_TrapFamily_Handler (compartilhado: AC,AD,S,U,V,W,X,Y) |  - Índice 0x01 de 25 despachantes (B, AA..AN, C, N..Z). Chama `Fight_RunSubstateDispatchB()`; retorno != 0: +0xAA++. - Confiança: confirmado. | +0xAA++ |
| X 0x02 | 0x080305E0 | ActionFamily_StatusCheckFamily_Handler (compartilhado: AC,AD,S,U,V,W,X,Y) |  - Índice 0x01 de ~48 despachantes (A, AA..BH, C..Z). Chama `Fight_RunSubstateDispatch(fi.A6)`; retorno != 0: +0xAA++. - É o passo genérico que roda o | +0xAA++ |
| X 0x03 | 0x0803080C | Fight_DispatchOnSubstateAA_Shared_SetDone (compartilhado: AC,AD,S,U,V,W,X,Y) | só faz `0x03001800+0xB5 = 1` (handler concluído). Não mexe em +0xAA. | +0xAA |
| X 0x04 | 0x08020DF8 | Fight_DispatchOnSubstateAA_S_04_LoadGfxAndSfx29 (compartilhado: AC,AD,S,U,X,Y) | carrega gráficos e posiciona o sprite do alvo (tabela por +0x87), toca SFX 0x29. | **Pendente** |
| X 0x05 | 0x08020EDC | Fight_DispatchOnSubstateAA_S_05_StepTargetSpriteAnim (compartilhado: AC,AD,S,U,X,Y) | Fight_StepSlotAnimOffsetTable(0x21) (mtbe: avança a tabela de passos de animação do slot). Se o contador do objeto do slot 0x21 (gObjectSlots + DAT_08 | **Pendente** |
| X 0x06 | 0x08020FB8 | Fight_DispatchOnSubstateAA_S_06_LoadGfxAndSlots (compartilhado: AC,AD,S,U,X,Y) | carrega gráficos e configura slot 0x21 (record list +0x8C). | **Pendente** |
| X 0x07 | 0x08021000 | Fight_DispatchOnSubstateAA_S_07_WaitSlotAnim (compartilhado: AC,AD,S,U,X,Y) | espera a animação do slot 0x21 (record +0x8C). | **Pendente** |
| X 0x08 | 0x08022870 | Fight_DispatchOnSubstateAA_X_08_LoadEffectGfxOnTarget | igual a U_08 com outro gráfico (paleta 0x10 words, record +0xF8): sprite no alvo, timer +0x86=0x3C. +1. | +1 |
| X 0x09 | 0x08022938 | Fight_DispatchOnSubstateAA_X_09_WaitAnimOrTimer (compartilhado: X,Y) | roda anim do slot 0x21; decrementa o timer +0x86 enquanto +0x89 != 0 ou tecla (bit 0 de um registro) não apertada; timer 0 ou pulo: +1. | +1 |
| X 0x0A | 0x08022990 | Fight_DispatchOnSubstateAA_X_0A_StepSlotToTarget (compartilhado: X,Y) | roda anim e anda o slot 0x21 até o alvo; ao chegar, +1. | +1 |
| X 0x0B | 0x080229C0 | Fight_DispatchOnSubstateAA_X_0B_SlideOutAndClearSlot (compartilhado: X,Y) | roda anim; diminui um campo do slot em 8 por quadro; quando vale 8, limpa o slot 0x21 e +1. | +1 |
| X 0x0C | 0x08022A04 | Fight_DispatchOnSubstateAA_X_0C_SpawnRingSpritesOnTarget | sprite central (slot 0x25, record +0xFC) e 4 sprites em anel (0x21..0x24, raio 0x10) no alvo, SFX 0x4A. +1. | +1 |
| X 0x0D | 0x08022B40 | Fight_DispatchOnSubstateAA_X_0D_ExpandRingAndClear (compartilhado: X,Y) | gira o anel (+4/quadro), após 0x5A quadros encolhe os 5 sprites; quando a escala chega a 4 limpa 0x21..0x25, +1. | +1 |
| X 0x0E | 0x08022C28 | Fight_DispatchOnSubstateAA_X_0E_InflictStatus1F | efeito de X no alvo: +0xC4 = 0x1F (código de status), +0xC5 = 0, +0xC6/+0xC7 = `Fight_ClampDisplayValue1To99`(+0xE8/+0xEC) (duração/força, provável).  | **Pendente** |
| X 0x0F | 0x08022C90 | Fight_DispatchOnSubstateAA_X_0F_SetMessage29 | final de X: +0xA6 = 0x29, +1. | +1 |

## Despachante Y
- Resumo (3 linhas; opcode 0x2F, revive provável):
  1. Abertura de ROS; só age se a cabeça do alvo está com armadura 0 (senão mensagem 9).
  2. Passo 0x0E: alvo +0x40=3, reparo da cabeça, reposiciona os bots; visual igual ao de X.
  3. Termina com efeito de M_05 e mensagem 0x28.


| Passo | Endereço | Nome (seção) | Efeito (resumo da seção) | Próximo +0xAA |
|---|---|---|---|---|
| Y 0x00 | 0x08022CD0 | Fight_DispatchOnSubstateAA_Y_00_CheckRosAndTargetDown | abertura comum. Se passa e o alvo tem +0x59 (armadura da cabeça) != 0: +0xA6=9, +1 (alvo não está fora). Se +0x59 == 0: uso, `Fight_ComputePartValue`, | +1 (alvo não está fora |
| Y 0x01 | 0x08030608 | ActionFamily_TrapFamily_Handler (compartilhado: AC,AD,S,U,V,W,X,Y) |  - Índice 0x01 de 25 despachantes (B, AA..AN, C, N..Z). Chama `Fight_RunSubstateDispatchB()`; retorno != 0: +0xAA++. - Confiança: confirmado. | +0xAA++ |
| Y 0x02 | 0x080305E0 | ActionFamily_StatusCheckFamily_Handler (compartilhado: AC,AD,S,U,V,W,X,Y) |  - Índice 0x01 de ~48 despachantes (A, AA..BH, C..Z). Chama `Fight_RunSubstateDispatch(fi.A6)`; retorno != 0: +0xAA++. - É o passo genérico que roda o | +0xAA++ |
| Y 0x03 | 0x0803080C | Fight_DispatchOnSubstateAA_Shared_SetDone (compartilhado: AC,AD,S,U,V,W,X,Y) | só faz `0x03001800+0xB5 = 1` (handler concluído). Não mexe em +0xAA. | +0xAA |
| Y 0x04 | 0x08020DF8 | Fight_DispatchOnSubstateAA_S_04_LoadGfxAndSfx29 (compartilhado: AC,AD,S,U,X,Y) | carrega gráficos e posiciona o sprite do alvo (tabela por +0x87), toca SFX 0x29. | **Pendente** |
| Y 0x05 | 0x08020EDC | Fight_DispatchOnSubstateAA_S_05_StepTargetSpriteAnim (compartilhado: AC,AD,S,U,X,Y) | Fight_StepSlotAnimOffsetTable(0x21) (mtbe: avança a tabela de passos de animação do slot). Se o contador do objeto do slot 0x21 (gObjectSlots + DAT_08 | **Pendente** |
| Y 0x06 | 0x08020FB8 | Fight_DispatchOnSubstateAA_S_06_LoadGfxAndSlots (compartilhado: AC,AD,S,U,X,Y) | carrega gráficos e configura slot 0x21 (record list +0x8C). | **Pendente** |
| Y 0x07 | 0x08021000 | Fight_DispatchOnSubstateAA_S_07_WaitSlotAnim (compartilhado: AC,AD,S,U,X,Y) | espera a animação do slot 0x21 (record +0x8C). | **Pendente** |
| Y 0x08 | 0x08022D4C | Fight_DispatchOnSubstateAA_Y_08_LoadEffectGfxOnTarget | igual a X_08 (outro gráfico): sprite no alvo, timer 0x3C. +1. | +1 |
| Y 0x09 | 0x08022938 | Fight_DispatchOnSubstateAA_X_09_WaitAnimOrTimer (compartilhado: X,Y) | roda anim do slot 0x21; decrementa o timer +0x86 enquanto +0x89 != 0 ou tecla (bit 0 de um registro) não apertada; timer 0 ou pulo: +1. | +1 |
| Y 0x0A | 0x08022990 | Fight_DispatchOnSubstateAA_X_0A_StepSlotToTarget (compartilhado: X,Y) | roda anim e anda o slot 0x21 até o alvo; ao chegar, +1. | +1 |
| Y 0x0B | 0x080229C0 | Fight_DispatchOnSubstateAA_X_0B_SlideOutAndClearSlot (compartilhado: X,Y) | roda anim; diminui um campo do slot em 8 por quadro; quando vale 8, limpa o slot 0x21 e +1. | +1 |
| Y 0x0C | 0x08022E14 | Fight_DispatchOnSubstateAA_Y_0C_SpawnRingSpritesOnTarget | igual a X_0C com SFX 0x4B. +1. | +1 |
| Y 0x0D | 0x08022B40 | Fight_DispatchOnSubstateAA_X_0D_ExpandRingAndClear (compartilhado: X,Y) | gira o anel (+4/quadro), após 0x5A quadros encolhe os 5 sprites; quando a escala chega a 4 limpa 0x21..0x25, +1. | +1 |
| Y 0x0E | 0x08022F50 | Fight_DispatchOnSubstateAA_Y_0E_ReviveTarget | efeito de Y: alvo +0x40 = 3, `Fight_ClampRepairToMissingArmor(alvo, cabeça, +0xEC)`, +0x59 = byte de +0xC8, +0xBA = 1; atualiza sprites das peças e re | **Pendente** |
| Y 0x0F | 0x08022FB4 | Fight_DispatchOnSubstateAA_Y_0F_PlayEffectSetMessage28 | final de Y: `FUN_0803F63C(bot alvo, dado, 0x0E)` (copia bot alvo -> dado; M_05 faz o inverso, tabela -> dado; DC4), +0xA6 = 0x28, +1. | +1 |

## Despachante AC
- Resumo (3 linhas; opcode 0x33, parcial):
  1. AC_00 calcula ROS; AC_08 carrega gráfico; passos 09..0F movem slots até o alvo (compartilham handlers de AB).
  2. AC_0E e AC_0F fecham o efeito; AC_0F grava o código de efeito 0x2B ('carga ultra rápida', A6).
  3. Os demais passos não foram documentados.


| Passo | Endereço | Nome (seção) | Efeito (resumo da seção) | Próximo +0xAA |
|---|---|---|---|---|
| AC 0x00 | 0x08023AF0 | Fight_DispatchOnSubstateAA_AC_00_RosCalcPart | igual AA 0x00 sem a chamada `Fight_ApplyStatusCostIfFlag` e sem o teste de técnica: `Fight_AccumRosPart(bot, part)`; se `FUN_0803725C()` == 0: `Fight_ | **Pendente** |
| AC 0x01 | 0x08030608 | ActionFamily_TrapFamily_Handler (compartilhado: AC,AD,S,U,V,W,X,Y) |  - Índice 0x01 de 25 despachantes (B, AA..AN, C, N..Z). Chama `Fight_RunSubstateDispatchB()`; retorno != 0: +0xAA++. - Confiança: confirmado. | +0xAA++ |
| AC 0x02 | 0x080305E0 | ActionFamily_StatusCheckFamily_Handler (compartilhado: AC,AD,S,U,V,W,X,Y) |  - Índice 0x01 de ~48 despachantes (A, AA..BH, C..Z). Chama `Fight_RunSubstateDispatch(fi.A6)`; retorno != 0: +0xAA++. - É o passo genérico que roda o | +0xAA++ |
| AC 0x03 | 0x0803080C | Fight_DispatchOnSubstateAA_Shared_SetDone (compartilhado: AC,AD,S,U,V,W,X,Y) | só faz `0x03001800+0xB5 = 1` (handler concluído). Não mexe em +0xAA. | +0xAA |
| AC 0x04 | 0x08020DF8 | Fight_DispatchOnSubstateAA_S_04_LoadGfxAndSfx29 (compartilhado: AC,AD,S,U,X,Y) | carrega gráficos e posiciona o sprite do alvo (tabela por +0x87), toca SFX 0x29. | **Pendente** |
| AC 0x05 | 0x08020EDC | Fight_DispatchOnSubstateAA_S_05_StepTargetSpriteAnim (compartilhado: AC,AD,S,U,X,Y) | Fight_StepSlotAnimOffsetTable(0x21) (mtbe: avança a tabela de passos de animação do slot). Se o contador do objeto do slot 0x21 (gObjectSlots + DAT_08 | **Pendente** |
| AC 0x06 | 0x08020FB8 | Fight_DispatchOnSubstateAA_S_06_LoadGfxAndSlots (compartilhado: AC,AD,S,U,X,Y) | carrega gráficos e configura slot 0x21 (record list +0x8C). | **Pendente** |
| AC 0x07 | 0x08021000 | Fight_DispatchOnSubstateAA_S_07_WaitSlotAnim (compartilhado: AC,AD,S,U,X,Y) | espera a animação do slot 0x21 (record +0x8C). | **Pendente** |
| AC 0x08 | 0x08023B44 | Fight_DispatchOnSubstateAA_AC_08_LoadGfxSetupSlot21 | `decompress_and_store(0x084D777C, 0x06014800)`; `bios_cpufastset(0x0850FD98, 0x050002E0, 8)`; slot 0x21 (gObjectSlots+0x840): halfword +0x00 /= 0x220; | **Pendente** |
| AC 0x09 | 0x080238A0 | Fight_DispatchOnSubstateAA_AB_WaitAnimOrTimer (compartilhado: AC,AD) | `Anim_RunSlotScript` (índice do byte +0x84*4); se flag de bot ou bit 0 de 0x03001798 não ativos, espera timer 0x86 zerar; senão +0xAA. | +0xAA |
| AC 0x0A | 0x08023900 | Fight_DispatchOnSubstateAA_AB_MoveSlotTowardTarget (compartilhado: AC,AD) | `Anim_RunSlotScript`; desloca o X do slot ±0x40 conforme bit 0 de +0xF8; `Fight_StepSlotTowardTarget` (0x21); ao chegar zera o campo e +0xAA. | +0xAA |
| AC 0x0B | 0x08023C18 | Fight_DispatchOnSubstateAA_AC_SetupSlot46Sfx4E | +0x84=0x46; `Slot_SetupFromRecordList` (registro +0x118); sfx 0x4E; +0xAA. | +0xAA |
| AC 0x0C | 0x080239C0 | Fight_DispatchOnSubstateAA_AB_WaitScriptFlag (compartilhado: AC,AD) | `Anim_RunSlotScript`; se o byte de fim == -1 então +0xAA. | +0xAA |
| AC 0x0D | 0x08023A04 | Fight_DispatchOnSubstateAA_AB_StepFadeBytes8 (compartilhado: AC,AD) | byte de fade decrementa 8; se já era 8: `Tile_ClearSlots(0x21,1)` e +0xAA; senão soma 8 ao byte vizinho. | +0xAA; senão soma 8 ao byte vizinho |
| AC 0x0E | 0x08023C50 | Fight_DispatchOnSubstateAA_AC_0E_SetBotFlagBF | escreve 1 no byte `0x03000590 + byte(0x03001800+0x88)*0x100 + 0xBF` (flag do bot 2, offset Ghidra 0xBF); `0x03001800+0xAA++`. | +0xAA++` |
| AC 0x0F | 0x08023C78 | Fight_DispatchOnSubstateAA_AC_0F_SetStepCode2B | grava 0x2B em 0x03001800+0xA6; `0x03001800+0xAA++`. | +0xAA++` |

## Despachante AD
- Resumo (3 linhas; opcode 0x34, parcial):
  1. AD_00 calcula ROS; AD_08 carrega gráfico; passos 09..16 movem slots e escrevem dados do bot.
  2. AD_16 grava o código de efeito 0x2D ('absorveu tempo do inimigo', A6).
  3. Os demais passos não foram documentados.


| Passo | Endereço | Nome (seção) | Efeito (resumo da seção) | Próximo +0xAA |
|---|---|---|---|---|
| AD 0x00 | 0x08023CB8 | Fight_DispatchOnSubstateAA_AD_00_RosCalcPartNoTech16 | `Fight_AccumRosPart(bot, part)` (bot = byte +0x87, part = byte +0xA7); `Fight_ApplyStatusCostIfFlag(bot)`; se `FUN_0803725C()` == 0: se `Fight_HasNoTe | **Pendente** |
| AD 0x01 | 0x08030608 | ActionFamily_TrapFamily_Handler (compartilhado: AC,AD,S,U,V,W,X,Y) |  - Índice 0x01 de 25 despachantes (B, AA..AN, C, N..Z). Chama `Fight_RunSubstateDispatchB()`; retorno != 0: +0xAA++. - Confiança: confirmado. | +0xAA++ |
| AD 0x02 | 0x080305E0 | ActionFamily_StatusCheckFamily_Handler (compartilhado: AC,AD,S,U,V,W,X,Y) |  - Índice 0x01 de ~48 despachantes (A, AA..BH, C..Z). Chama `Fight_RunSubstateDispatch(fi.A6)`; retorno != 0: +0xAA++. - É o passo genérico que roda o | +0xAA++ |
| AD 0x03 | 0x0803080C | Fight_DispatchOnSubstateAA_Shared_SetDone (compartilhado: AC,AD,S,U,V,W,X,Y) | só faz `0x03001800+0xB5 = 1` (handler concluído). Não mexe em +0xAA. | +0xAA |
| AD 0x04 | 0x08020DF8 | Fight_DispatchOnSubstateAA_S_04_LoadGfxAndSfx29 (compartilhado: AC,AD,S,U,X,Y) | carrega gráficos e posiciona o sprite do alvo (tabela por +0x87), toca SFX 0x29. | **Pendente** |
| AD 0x05 | 0x08020EDC | Fight_DispatchOnSubstateAA_S_05_StepTargetSpriteAnim (compartilhado: AC,AD,S,U,X,Y) | Fight_StepSlotAnimOffsetTable(0x21) (mtbe: avança a tabela de passos de animação do slot). Se o contador do objeto do slot 0x21 (gObjectSlots + DAT_08 | **Pendente** |
| AD 0x06 | 0x08020FB8 | Fight_DispatchOnSubstateAA_S_06_LoadGfxAndSlots (compartilhado: AC,AD,S,U,X,Y) | carrega gráficos e configura slot 0x21 (record list +0x8C). | **Pendente** |
| AD 0x07 | 0x08021000 | Fight_DispatchOnSubstateAA_S_07_WaitSlotAnim (compartilhado: AC,AD,S,U,X,Y) | espera a animação do slot 0x21 (record +0x8C). | **Pendente** |
| AD 0x08 | 0x08023D44 | Fight_DispatchOnSubstateAA_AD_08_LoadGfxSetupSlot21 | `decompress_and_store(0x084D78C0, 0x06014800)`; `bios_cpufastset(0x0850FDB8, 0x050002E0, 8)`; slot 0x21: halfword +0x00 /= 0x220; bytes +0x08 = 0x40,  | **Pendente** |
| AD 0x09 | 0x080238A0 | Fight_DispatchOnSubstateAA_AB_WaitAnimOrTimer (compartilhado: AC,AD) | `Anim_RunSlotScript` (índice do byte +0x84*4); se flag de bot ou bit 0 de 0x03001798 não ativos, espera timer 0x86 zerar; senão +0xAA. | +0xAA |
| AD 0x0A | 0x08023900 | Fight_DispatchOnSubstateAA_AB_MoveSlotTowardTarget (compartilhado: AC,AD) | `Anim_RunSlotScript`; desloca o X do slot ±0x40 conforme bit 0 de +0xF8; `Fight_StepSlotTowardTarget` (0x21); ao chegar zera o campo e +0xAA. | +0xAA |
| AD 0x0B | 0x08023E18 | Fight_DispatchOnSubstateAA_AD_SetupSlot44Sfx4E | +0x84=0x44; `Slot_SetupFromRecordList` (registro +0x110); sfx 0x4E; +0xAA. | +0xAA |
| AD 0x0C | 0x080239C0 | Fight_DispatchOnSubstateAA_AB_WaitScriptFlag (compartilhado: AC,AD) | `Anim_RunSlotScript`; se o byte de fim == -1 então +0xAA. | +0xAA |
| AD 0x0D | 0x08023E50 | Fight_DispatchOnSubstateAA_AD_StepFadeBytes8Clear | como AB 0x0D, mas `Slot_ClearAttrMask(0x21,1)` no fim e o segundo byte desce 8. | **Pendente** |
| AD 0x0E | 0x08023E94 | Fight_DispatchOnSubstateAA_AD_LoadGfxSetupSlot | como AB 0x08 (gráficos, flags /0x8000, valores de display, +0x84=0x42, slot registro +0x108, timer 0x3C). | **Pendente** |
| AD 0x0F | 0x08023F48 | Fight_DispatchOnSubstateAA_AD_0F_FadeSlotBytesStep8 | lê o byte gObjectSlots+0x848 (= slot 0x21 +0x08). Se é 0x40 ('@'): `0x03001800+0xAA++`. Senão: byte +0x848 += 8 e byte +0x849 += 8 (gObjectSlots, byte | +0xAA++` |
| AD 0x10 | 0x080238A0 | Fight_DispatchOnSubstateAA_AB_WaitAnimOrTimer (compartilhado: AC,AD) | `Anim_RunSlotScript` (índice do byte +0x84*4); se flag de bot ou bit 0 de 0x03001798 não ativos, espera timer 0x86 zerar; senão +0xAA. | +0xAA |
| AD 0x11 | 0x08023900 | Fight_DispatchOnSubstateAA_AB_MoveSlotTowardTarget (compartilhado: AC,AD) | `Anim_RunSlotScript`; desloca o X do slot ±0x40 conforme bit 0 de +0xF8; `Fight_StepSlotTowardTarget` (0x21); ao chegar zera o campo e +0xAA. | +0xAA |
| AD 0x12 | 0x08023F80 | Fight_DispatchOnSubstateAA_AD_SetupSlot43Sfx4E | igual AB 0x0B (+0x84=0x43; slot registro +0x10C; sfx 0x4E). | **Pendente** |
| AD 0x13 | 0x080239C0 | Fight_DispatchOnSubstateAA_AB_WaitScriptFlag (compartilhado: AC,AD) | `Anim_RunSlotScript`; se o byte de fim == -1 então +0xAA. | +0xAA |
| AD 0x14 | 0x08023A04 | Fight_DispatchOnSubstateAA_AB_StepFadeBytes8 (compartilhado: AC,AD) | byte de fade decrementa 8; se já era 8: `Tile_ClearSlots(0x21,1)` e +0xAA; senão soma 8 ao byte vizinho. | +0xAA; senão soma 8 ao byte vizinho |
| AD 0x15 | 0x08023FB8 | Fight_DispatchOnSubstateAA_AD_WriteBotDisplayRows | escreve 0x29 e 0 nos bytes +0xC9..0xCC e +0xC4..0xC7 do registro do bot (dois bots, base 0x03000590) com `Fight_ClampDisplayValue1To99`; +0xAA. | +0xAA |
| AD 0x16 | 0x08024078 | Fight_DispatchOnSubstateAA_AD_16_SetStepCode2D | grava 0x2D em 0x03001800+0xA6; `0x03001800+0xAA++`. | +0xAA++` |

## Pendente
- Renomear no Ghidra os passos ainda sem nome no padrão Fight_DispatchOnSubstateAA_<D>_<idx>_<Passo> (não feito nesta rodada).
- Confirmar +0xAA de cada passo marcado Pendente; efeito de S/U..Y/AC/AD já descrito nas seções HN1/HA4/HA5/HA6.
- Passos compartilhados: documentar uma vez (dono = primeiro despachante da lista).
