# Despachantes Fight_DispatchOnSubstateAA_AF, AG, AM, AN, AO, AP, AQ (GH-AD)

Tabela de passos lida na ROM original (`Medabots - Metabee (Europe).gba`, só leitura, `python -I`) a partir do literal
stub+0x24. Os efeitos vêm das seções já existentes em HA2, HA3, HA4, HA7, HA8, HN1, HN5 e HN6: os 81 passos únicos
já têm seção, então nenhum precisou de decompile nesta rodada. Foram renomeados 35 passos no Ghidra (nome com o
índice). Ghidra não foi salvo nem fechado.

Tabelas (literal stub+0x24; tamanho = até o início da tabela seguinte, confirmado porque as entradas casam):
AF 0x083AE064 (0x00..0x17), AG 0x083AE0C4 (0x00..0x17), AM 0x083AE298 (0x00..0x14), AN 0x083AE2EC (0x00..0x14),
AO 0x083AE340 (0x00..0x1F), AP 0x083AE3C0 (0x00..0x2F), AQ 0x083AE480 (0x00..0x2F). Entradas depois do último passo
útil apontam para `ObjScript_Op00_Stub` (0x08000840, `bx lr`, não avança).

Legenda: "compartilhado" = usado por mais de um despachante, mantém o nome original, sem rename. "fim" = grava
+0xB5 = 1 e não mexe em +0xAA. "+1" = próximo +0xAA = índice+1. Entre parênteses, o próximo índice quando não é +1.

## Fight_DispatchOnSubstateAA_AF (opcode 0x36, ActionScript_Op36_CallAF)
| idx | passo (endereço) | efeito | próximo +0xAA |
|---|---|---|---|
| 00 | AccumPartAndCheckTech (0x080249C0) | acumula peça (Fight_AccumRosPart) e custo de status; ramo técnica: +0xA6=0x1A; sem técnica 16: +0xA6=0x39; senão contador de uso e 0x04 | +1, ou 0x04 |
| 01 | TrapFamily_Handler (compartilhado) | roda Fight_RunSubstateDispatchB; espera o retorno | +1 |
| 02 | StatusCheckFamily_Handler (compartilhado) | roda o código gravado em +0xA6 | +1 |
| 03 | Shared_SetDone | conclusão | fim |
| 04 | S_04_LoadGfxAndSfx29 (compartilhado AF/AG) | gráfico e sprite do alvo; SFX 0x29 | +1 (provável) |
| 05 | S_05_StepTargetSpriteAnim (compartilhado) | anda a tabela de passos do slot 0x21; ao contador 0x20 limpa e avança | +1 (provável) |
| 06 | S_06_LoadGfxAndSlots (compartilhado) | gráfico e configura o slot 0x21 | +1 (provável) |
| 07 | S_07_WaitSlotAnim (compartilhado) | espera a animação do slot 0x21 | 0x08 ao terminar |
| 08 | AF_08_SetupOverlay (0x08024A30) | gráfico, overlay do slot (|=0x220), timer 0x3C | +1 (0x0B) |
| 09 | Shared_WaitTimerOrSkip (0x08030674) | espera +0x86 ou pula | +1 |
| 0A | U_0A_ScrollBgAndStepSlotToTarget (compartilhado AF/AG/AI/AJ) | rola BG e anda o slot 0x21 até o alvo | +1 |
| 0B | AF_0B_InitSlotAttrs (0x08024AF4) | atributos do slot por bot; SFX 0x6A | +1 (0x0C, inferido) |
| 0C | AF_0C_WaitSlot22 (0x08024BCC) | espera FUN_08036AC4(0x22); timer 0x3C | +1 (0x0E, inferido) |
| 0D | Shared_WaitTimerOrSkip | igual ao 09 | +1 |
| 0E | AF_0E_SetSlotFlagsPos (0x08024BF8) | flag bit 15 do slot 0x21; posição 0x80 | +1 (0x0F) |
| 0F | AF_0F_CountDownToMark (0x08024C44) | decrementa a posição até a marca; SFX 0x3C | +1 (0x10) |
| 10 | AF_10_OscillateSlot (0x08024C94) | oscila x do slot ±2 por 0x3C quadros; limpa tiles | +1 (0x11) |
| 11 | AF_11_SetSlotParams (0x08024D18) | bot+0xC9=0x2B; modo do bot por +0x8B | +1 (0x12) |
| 12 | AF_12_CopyTileData (0x08024DA8) | copia 0xE bytes da fonte escolhida; +0xA6=0x30 | +1 (0x13) |
| 13 | StatusCheckFamily (compartilhado) | igual ao 02 | +1 |
| 14 | Shared_SetDone | conclusão | fim |
| 15..17 | ObjScript_Op00_Stub | não usados | - |

Resumo (AF, opcode 0x36): técnica tipo "armadilha" com slot: acumula dano e custo, carrega o gráfico no slot 0x21,
faz a peça oscilar por 60 quadros e termina com a marca (provável, pelos nomes e passos lidos no HA7).

## Fight_DispatchOnSubstateAA_AG (opcode 0x37, ActionScript_Op37_CallAG)
| idx | passo (endereço) | efeito | próximo +0xAA |
|---|---|---|---|
| 00 | AG_00_AccumPartAndCheckTech (0x08024E2C) | igual ao AF_00 (só muda o literal da struct) | +1, ou 0x04 |
| 01 | TrapFamily_Handler (compartilhado) | igual | +1 |
| 02 | StatusCheckFamily_Handler (compartilhado) | igual | +1 |
| 03 | Shared_SetDone | conclusão | fim |
| 04..07 | S_04..S_07 (compartilhados com AF) | ver AF 04..07 | 04→05→06→07→08 |
| 08 | AG_08_SetupSlots (0x08024E9C) | gráfico (0x10 words), flags do slot, timer 0x3C | +1 (0x0B) |
| 09 | AG_09_ScrollSlotWaitTimer (0x08030780) | desloca X do slot 0x21 ±0x40 por quadro até o timer | +1 (0x0A) |
| 0A | U_0A_ScrollBgAndStepSlotToTarget (compartilhado) | igual ao AF_0A | +1 (0x0B) |
| 0B | AG_0B_PlaySfx33SetFlags (0x08024F90) | SFX 0x33; flag do slot | +1 (0x0C) |
| 0C | AG_0C_SlideSlotsIn (0x08024FC4) | desliza o slot 0x21 ±0x80 até o destino | +1 (0x0D) |
| 0D | AG_0D_SpawnEffectSlot (0x0802503C) | limpa flags; setup do slot 0x21; SFX 0x4F | +1 (0x0F) |
| 0E | AG_0E_WaitSlotScriptEnd (0x0803081C) | roda o roteiro do slot 0x21 (0x083B6100[+0x84]) até o byte de fim | +1 (0x0F) |
| 0F | AG_0F_SetBotParamsFromE8EC (0x08025104) | bot+0xC9=0x2C; +0xCB/+0xCC de E8/EC | +1 (0x10) |
| 10 | AG_10_SetStep33 (0x08025174) | fight+0xA6=0x33 | +1 (0x11) |
| 11 | StatusCheckFamily | igual ao 02 | +1 |
| 12 | Shared_SetDone | conclusão | fim |
| 13..17 | ObjScript_Op00_Stub | não usados | - |

Resumo (AG, opcode 0x37): deslize de slot com SFX: o slot 0x21 entra, desloca e sai; grava os parâmetros do bot
(0x2C e E8/EC) e termina no passo 0x33 (provável).

## Fight_DispatchOnSubstateAA_AM (opcode 0x3E, ActionScript_Op3E_CallAM)
| idx | passo (endereço) | efeito | próximo +0xAA |
|---|---|---|---|
| 00 | AM_00_AccumPartCheckCostNoTech (0x08026130) | como AF_00 sem o ramo técnica 16; +0xA6=0x1A | +1 |
| 01 | TrapFamily_Handler (compartilhado) | igual | +1 |
| 02 | StatusCheckFamily_Handler (compartilhado) | igual | +1 |
| 03 | Shared_SetDone | conclusão | fim |
| 04 | AM_04_SetupBotSpriteSlot (0x08026184) | gráfico; IO 0x840 com constante; sprite do bot | +1 |
| 05 | AM_05_WaitAnimThenPlaceSprite (0x08026284) | roda a anim do slot 0x21; ao terminar grava 0x80 nos campos | +1 |
| 06 | AM_06_StepDownCounterWithSfx (0x080262FC) | contador do byte −8 até a marca '@'; SFX 0x53 | +1 |
| 07 | AM_07_StepCounterUntilP (0x08026340) | soma 4 ao byte até 'P' | +1 |
| 08 | AM_08_StepDownCounterUntilAt (0x08026378) | contador −4 até '@'; timer 0x3C | +1 |
| 09 | Shared_WaitTimerOrSkip | igual ao 09 de AF | +1 |
| 0A | AM_0A_ClearSlotSetPartDisplayValue (0x080263BC) | limpa slot 0x21; bit 0x10 em +0xBD; copia valor de técnica | +1 |
| 0B | AM_0B_SetMessage2AAdvance (0x08026444) | +0xA6=0x2A (mensagem) | +1 |
| 0C | StatusCheckFamily | igual | +1 |
| 0D | Shared_SetDone | conclusão | fim |
| 0E..14 | ObjScript_Op00_Stub | não usados | - |

Resumo (AM, opcode 0x3E): técnica de contagem: acumula custo, monta o sprite do bot, decrementa contadores até a
marca, mostra o valor da peça e a mensagem 0x2A.

## Fight_DispatchOnSubstateAA_AN (opcode 0x3F, ActionScript_Op3F_CallAN_SceneExit)
| idx | passo (endereço) | efeito | próximo +0xAA |
|---|---|---|---|
| 00 | AN_00_AccumPartCheckCostFlagAB (0x08026484) | como AM_00; liga +0xAB=1; +0xA6=0x1A | +1 |
| 01 | TrapFamily_Handler (compartilhado) | igual | +1 |
| 02 | StatusCheckFamily_Handler (compartilhado) | igual | +1 |
| 03 | Shared_SetDone | conclusão | fim |
| 04 | AN_04_SetupEffectSpritesAndSfx (0x080264E4) | LZ77 do gráfico; sprite do registro do bot; timer | +1 |
| 05 | AN_05_WaitFramesWithAnimations (0x08026628) | anim dos slots 0x21 e 0x22; timer até 0x5A | +1 |
| 06 | AN_06_ApplyRandomPartReplacementEffect (0x08026678) | roda anim; +0x118=1; troca peça sorteada; refresh dos sprites | +1 |
| 07 | AN_07_ClearSlotsCopyPartBlobs (0x08026810) | limpa slots 0x21/0x22; copia 2 blobs de 15 bytes; +0xA6=0x2F | +1 |
| 08 | StatusCheckFamily | igual | +1 |
| 09 | Shared_SetDone | conclusão | fim |
| 0A..14 | ObjScript_Op00_Stub | não usados | - |

Resumo (AN, opcode 0x3F): AN_06 é a troca aleatória de peça (Fight_ApplyRandomPartReplacementEffect). O opcode 0x3F
também sai da cena de batalha (grava cena 8, ver AS3c; não validado). Os demais passos montam animação e copiam blobs.

## Fight_DispatchOnSubstateAA_AO (opcode 0x40, ActionScript_Op40_SideSpritesAO)
| idx | passo (endereço) | efeito | próximo +0xAA |
|---|---|---|---|
| 00 | H_00_LoadSpritesTiles (0x0801E300, compartilhado AO/AP/AQ) | gráficos OBJ e paleta da luta | +1 |
| 01 | AO_01_RollPartValueForBot (0x080268AC) | sorteia valor de peça do bot ativo e calcula efeito | +1 |
| 02 | AO_02_ClampPartCountsToLimit (0x080268D4) | limita as 4 peças ao stat +0xEC | +1 |
| 03 | AO_03_ShowFourPartValues (0x08026924) | escreve 4 valores de peça e monta as linhas; SFX 0x5B | +1 |
| 04 | AO_04_WaitPartRowsDrawn (0x08026A24) | espera as 4 linhas; timer 0x3C | +1 |
| 05 | ActionFamily_Change_Handler (compartilhado) | avanço rápido ou espera do timer | +1 |
| 06 | H_10_SfxStep2F (0x0801E9E0, compartilhado AO/AP) | SFX 0x2F | +1 |
| 07 | AO_07_DrawPartIconsWaitReady (0x08026A74) | desenha ícones de peça; timer 0x3C | +1 |
| 08 | ActionFamily_HealChange_Handler (compartilhado) | espera timer; limpa slot 10 | +1 |
| 09 | AO_09_ShowPartFlagsOrBackup (0x08026AF4) | efeito por peça com flag; sem flag +1; com flag, backup e subestado 0x0D | +1, ou 0x0D |
| 0A | H_14_StepByOrientation (0x0801EA94, compartilhado) | chama FUN_08036BF4 com a orientação do bot | +1 |
| 0B | H_15 (0x0801EAC4, compartilhado) | só marca +0xB5=1 (hold) | hold |
| 0C | ObjScript_Op00_Stub | não usado | - |
| 0D | AO_0D_WaitPartEffectAnimsClear (0x08026BF8) | espera anims de efeito das 4 peças; limpa slots 10..13 | +1 |
| 0E | AO_0E (0x0801EBD4, compartilhado AO/AP) | atualiza sprites de duas camadas; timer 0x3C | +1 |
| 0F | ActionFamily_HealChange_Handler | igual ao 08 | +1 |
| 10 | AO_10_QueueHeadBreakIfDefenderHeadZero (0x08026CBC) | se +0x59 do alvo == 0, enfileira quebra de cabeça | +1 |
| 11 | H_14 | igual ao 0A | +1 |
| 12 | H_15 | igual ao 0B | hold |
| 13..1F | ObjScript_Op00_Stub | não usados | - |

Resumo (AO, opcode 0x40): mostra os valores das 4 peças do bot, limita e atualiza os sprites laterais; no fim quebra a
cabeça do defensor se o alvo ficou sem cabeça.

## Fight_DispatchOnSubstateAA_AP (opcode 0x44, ActionScript_Op44_SideSpritesAP)
| idx | passo (endereço) | efeito | próximo +0xAA |
|---|---|---|---|
| 00 | H_00_LoadSpritesTiles (compartilhado) | igual ao AO 00 | +1 |
| 01 | AP_01_WaitUnlessFlagB8Bit1 (0x08026D1C) | se bit 0x02 de +0xB8 apagado, +1; senão subestado | +1 |
| 02 | H_0A_SetupFourSprites (0x0801E864) | gráfico e 4 sprites do bot na etapa | +1 |
| 03 | H_0B_TickSpriteTimers (0x0801E8DC) | timer dos sprites; se ativo, +0x86=10 | +1 |
| 04 | ActionFamily_HealChange_Handler | igual | +1 |
| 05 | H_0D_LoadNumberSpriteRow (0x0801E918) | gráfico, dígitos, linha de sprite; SFX 0x5B | +1 |
| 06 | H_0E_WaitSpriteStep10 (0x0801E9B0) | avança o passo 10 de sprite; timer 0x3C | +1 |
| 07 | ActionFamily_Change_Handler | igual | +1 |
| 08 | H_10_SfxStep2F (compartilhado AO/AP) | SFX 0x2F | +1 |
| 09 | H_11_WaitPartIconRows (0x0801E9FC) | espera linhas de ícones de peça | +1 |
| 0A | ActionFamily_HealChange_Handler | igual | +1 |
| 0B | H_13_BackupStatsOrSkip (0x0801EA5C) | se +0xD1 == 0, +1; senão backup e +3 | +1, ou +3 |
| 0C | H_14 | igual | +1 |
| 0D | H_15 | igual | hold |
| 0E | H_16_LoadPartGfx (0x0801EAD4) | gráfico de peça e sprites na posição do bot | +1 |
| 0F | H_17_RunSlotAnim (0x0801EB5C) | anim do slot 0x0A até acabar; limpa | +1 |
| 10 | AO_0E (compartilhado AO/AP) | duas camadas de sprites | +1 |
| 11 | ActionFamily_HealChange_Handler | igual | +1 |
| 12 | H_1A_QueueBrokenBot (0x0801EBF8) | se o bot não está quebrado, enfileira; senão +3 | +1, ou +3 |
| 13 | H_14 | igual | +1 |
| 14 | H_15 | igual | hold |
| 15 | AP_15_ApplyDamageToRandomTargetPart (0x08026D44) | sorteia peça alvo e aplica dano; depois salta por outra tabela (não recuperada) | +3 |
| 16 | H_14 | igual | +1 |
| 17 | H_15 | igual | hold |
| 18 | AP_18_LoadGfxPlaceOnPart (0x08026D98, compartilhado AP/AQ) | gráfico, OBJ sobre o bot, SFX 0x2A; salto indireto no fim | +1 |
| 19 | H_08_AdvanceSlotOffsetByStage (0x0801E830, compartilhado AP/AQ) | avança o deslocamento do slot pela etapa; timer 60 | +1 |
| 1A | ActionFamily_HealChange_Handler | igual | +1 |
| 1B | AP_1B_LoopToStep02 (0x08026EC0) | volta ao passo 02 (laço) | 2 |
| 1C..1F | ObjScript_Op00_Stub | não usados | - |
| 20 | H_30_LoadEffectSpriteAtBot (0x0801F018) | efeito de ataque no bot alvo; OBJ em gObjectSlots+0x280; SFX 0x2A | +1 |
| 21 | H_04_WaitSlotOffset10 (0x0801E54C) | espera o deslocamento do slot 10; timer 60 | +1 |
| 22 | ActionFamily_HealChange_Handler | igual | +1 |
| 23 | H_14 | igual | +1 |
| 24 | H_15 | igual | hold |
| 25..2F | ObjScript_Op00_Stub | não usados | - |

Resumo (AP, opcode 0x44): troca de HP e peça dos dois lados: sorteia a peça alvo e aplica dano (idx 15), carrega o
efeito sobre o bot (18, 20) e faz laços (1B). Mesma família de passos de AO e AQ.

## Fight_DispatchOnSubstateAA_AQ (opcode 0x48, ActionScript_Op48_SideSpritesAQ)
| idx | passo (endereço) | efeito | próximo +0xAA |
|---|---|---|---|
| 00 | AQ_00_PickActorPartToRepair (0x08026EF8) | escolhe a peça mais danificada do ator; achou: +0xD0=+0x8B=peça, +0xAA=2 | 2, ou +1 |
| 01 | Shared_SetDone | conclusão | fim |
| 02 | AQ_02_RepairActorPart (0x08026F34) | limpa slots de tile; ClampRepairToMissingArmor(ator,+0xD0,+0xEC) | +1 |
| 03 | H_00_LoadSpritesTiles | igual ao AO 00 | +1 |
| 04 | H_3E_LoadGfxSpawnEffectAtBot (0x0801F214, compartilhado AQ/P/R) | gráfico e efeito de spawn no bot | +1 |
| 05 | H_3F_WaitSlotAnimEndThenClear (0x0801F290, compartilhado) | anim do slot 10 até acabar; timer 60 | +1 |
| 06 | ActionFamily_HealChange_Handler | igual | +1 |
| 07 | H_41_ShowNumberSpriteRowAtBot (0x0801F2DC, compartilhado) | linha de número no bot | +1 |
| 08 | H_0E_WaitSpriteStep10 (0x0801E9B0) | igual ao AP 06 | +1 |
| 09 | ActionFamily_Change_Handler | igual | +1 |
| 0A | H_44_PlaySfx2EAndAdvance (0x0801F374, compartilhado) | SFX 0x2E | +1 |
| 0B | H_45_DrawPartIconsWaitTimer (0x0801F390, compartilhado) | ícones de peça; timer 60 | +1 |
| 0C | ActionFamily_HealChange_Handler | igual | +1 |
| 0D | AQ_0D_RepairPartByAmountCA (0x08026F6C) | se +0xCA != 0, repara a peça mais danificada do bot +0x88 e +0xAA=0x10; senão +1 | 0x10, ou +1 |
| 0E | H_14 | igual | +1 |
| 0F | Shared_SetDone | conclusão | fim |
| 10 | AP_18_LoadGfxPlaceOnPart (compartilhado AP/AQ) | igual ao AP 18 | +1 |
| 11 | H_08_AdvanceSlotOffsetByStage (compartilhado) | igual ao AP 19 | +1 |
| 12 | ActionFamily_HealChange_Handler | igual | +1 |
| 13 | AQ_13_JumpToStep04 (0x08026FC0) | +0xAA=4 (laço) | 4 |
| 14..1F | ObjScript_Op00_Stub | não usados | - |

Resumo (AQ, opcode 0x48): reparo: escolhe a peça mais danificada do ator, repara (02, 0D), mostra o efeito e o
número (04..0B) e tem laço de volta ao passo 04 (13).

## Renomeados no Ghidra (35, nome com o índice)
AF 00/08/0B/0C/0E/0F/10/11/12; AG 00/08/09/0B/0C/0D/0E/0F/10; AM 04/05/06/07/08; AN 00/04/05/06/07;
AO 01/02/03/04/07/09/10. Lista em `AD_simbolos.csv`. Os passos compartilhados (TrapFamily, StatusCheck, Shared_*,
H_xx, S_xx, U_0A, AP_18, H_08, AO_0E, H_14, H_15) mantêm o nome original.

## Pendente
- Próximos +0xAA marcados "inferido" (AF 0B, 0C; AG 10) não foram lidos no corpo; são da cadeia de nomes e da seção HA7.
- AF 04..07 e AG 04..07 (S_xx) têm efeito só pela seção HA4; não rechecados.
- AP 15 salta para outra tabela (não recuperada); AQ 00 e AP 18 idem.
- Cena 8 em 0x3F: não validada no emulador (AS3c).
- Nada foi testado no emulador (fora de escopo desta tarefa).
