# AS5d: efeito dos despachantes S, U, V, W, X, Y (opcodes 0x29, 0x2B..0x2F)

Leitura: Ghidra MCP (só leitura, sem salvar), decompile dos passos com corpo novo. Sem emulador, sem texto do jogo.
Nomes já existentes no `symbols/mtbe.csv` foram usados; nenhum rename feito. Nome do arquivo: **AS5d** (o `AS5.md`
existente é outro tema, Op54/Op56/Op53; não foi sobrescrito).
Tabela de passos: stub do despachante +0x24 = tabela de handlers indexada por `+0xAA` (4 bytes por entrada).
Gancho: `0x03001800+0xAA` = passo; `+0xB5` = handler concluído.

## Compartilhados (descritos uma vez)
| endereço de entrada | nome | efeito | conf. |
|---|---|---|---|
| 0x08030608 | ActionFamily_TrapFamily_Handler | chama `Fight_RunSubstateDispatchB()`; resultado != 0 => +0xAA++ | confirmado |
| 0x080305E0 | ActionFamily_StatusCheckFamily_Handler | chama `Fight_RunSubstateDispatch(fi.A6)`; resultado != 0 => +0xAA++ (runner genérico) | confirmado |
| 0x0803080C | Fight_DispatchOnSubstateAA_Shared_SetDone | `fi+0xB5 = 1`; não mexe em +0xAA | confirmado |
| 0x08030674 | Fight_DispatchOnSubstateAA_Shared_WaitTimerOrSkip | espera +0x86 ou pula; +0xAA++ | confirmado (SU.md) |
| 0x08020DF8 | ..._S_04_LoadGfxAndSfx29 | LZ77 dos gráficos + cpufastset (zera); posiciona sprite do alvo (pos. do bot +0x1742/+0x1744, -4); SFX 0x29; +1 | confirmado |
| 0x08020EDC | ..._S_05_StepTargetSpriteAnim | slot 0x21 anda 0x20 px por quadro (sinal = bit0 de `moveDirectionFlags`) e outra coord ±1; termina quando byte do slot == 0x20; +1 | provavel |
| 0x08020FB8 | ..._S_06_LoadGfxAndSlots | LZ + cpufastset; `Slot_SetupFromRecordList(rec+0x8C, 0x21)`; +1 | confirmado |
| 0x08021000 | ..._S_07_WaitSlotAnim | `Anim_RunSlotScript(rec+0x8C, 0x21)` até byte do slot == 0xFF; +1 | confirmado |

Usados em S, U, X e Y: 04, 05, 06, 07 (mesmos endereços). Usados em X e Y: 09, 0A, 0B, 0D (ver abaixo).

## Despachante S (0x29, 19 passos; entrada 0x08020CFC; tabela 0x083ADB40)
Fluxo de ponta a ponta: ROS e custo do ator; grava dano no registro do lado B do alvo (`trapValue`/`trapPower`),
com fase B quando a técnica bate; anima 3 slots até o alvo; mensagem 0x1E..0x21 (ou 0x71 na fase B); SFX 0x29, 0x3E,
0x40, 0x52.
| passo | entrada | nome | efeito | conf. |
|---|---|---|---|---|
| 00 | 0x08020D24 | Fight_DispatchOnSubstateAA_S_00_PenaltyComputeModeCopyPart | `Fight_AccumRosPart`, penalidade virus/bug; gasta 20 ROS (esgotado => msg 0x1A, +1 e sai); escala ataque pela parte; contador de uso; `partEffectMode` pela técnica (10 => 0, 11 => 1, 12 => 2, outras => 3); `partModeCopy` = técnica; => passo 04 | confirmado |
| 04 | 0x08020DF8 | ..._S_04_LoadGfxAndSfx29 | ver compartilhados | confirmado |
| 05 | 0x08020EDC | ..._S_05_StepTargetSpriteAnim | ver compartilhados | provavel |
| 06 | 0x08020FB8 | ..._S_06_LoadGfxAndSlots | ver compartilhados | confirmado |
| 07 | 0x08021000 | ..._S_07_WaitSlotAnim | ver compartilhados | confirmado |
| 08 | 0x08021038 | Fight_DispatchOnSubstateAA_S_08_LoadGfxByMode | por `partEffectMode`: 0 e 1 carregam gfx e timer 0x3C (slot rec +0x64/+0x68 conforme modo); 2 carrega gfx e grava +0xD00; 3 carrega gfx e slot de rec+0xE0; depois flags 0x220 no slot (+0x1000 e x-1 se bit0); +1 | confirmado |
| 09 | 0x080211C8 | ..._S_09_StepSpriteByteTimerByMode | byte do slot += 8 por quadro; em 0x40: timer = 1 (modo 3) ou 0x3C; +1 | confirmado |
| 0A | 0x0802121C | ..._S_0A_TimerThenBranchByMode | timer (+0x86) decresce; em 0: modo < 3 => zera byte e +1; modo 3 => SFX 0x52 e vai para 0x20 | confirmado |
| 0B | 0x080212A0 | ..._S_0B_StepSpriteUntil10 | coord do sprite ±1 por quadro até byte do slot == 0x10; +1 | confirmado |
| 0C | 0x08021300 | ..._S_0C_CheckPartThenBranch | testa `sideRecords[teamRecordBranch]`: `supportFlag` e `trapTechnique == partModeCopy`; ok => zera byte, +1; falha => modo 2 vai para 0x14, senão SFX 0x3E e 0x0E | confirmado |
| 0D | 0x08021370 | ..._S_0D_StepSpriteUntil10BranchMode | byte == 0x10 => (modo 2: 0x14; senão SFX 0x3E e 0x0E); senão byte ±8, +1 | confirmado |
| 0E | 0x080213E4 | ..._S_0E_CountdownClearSlots | byte == 8 => `ClearObjectAttributeEntries(0x21,1)`, +1; senão byte -= 8 | confirmado |
| 0F | 0x08021418 | ..._S_0F_RecordDamageAndJump | grava registro do lado B: se vazio ou técnica diferente (`sideBPhase=0`; `supportFlag=1`; `trapTechnique=partModeCopy`; zera valor/poder): modo 2 soma 1 ao valor, senão valor += ROS total e poder += `attackAccum`+`actionCounter` (clamp 0xFF); se técnica igual: `sideBPhase=1` e soma metade; +1 e salta pela tabela não recuperada em 0x0802155C | confirmado (escrita); salto provavel |
| 10 | 0x08021564 | ..._S_10_SetMsgByMode | msg 0x1E (modo 0), 0x1F (1), 0x20 (2), 0x21 (3); se `sideBPhase != 0` => msg 0x71; +1 | confirmado |
| 11 | 0x080305E0 | status genérico (compartilhado) | ver compartilhados | confirmado |
| 12 | 0x0803080C | SetDone (compartilhado) | ver compartilhados | confirmado |
| 14 | 0x080215C8 | ..._S_14_SetFlagsSfx40 | grava 4 e 0 em campos de ctx (+0x?) e zera +0x870; SFX 0x40; +1 | confirmado (campos sem nome: provavel) |
| 15 | 0x08021608 | ..._S_15_StepCoordDownClearSlot | `Fight_StepSlotCoordDownInRange(0x21, -0x40, -0x50)` até chegar; `ClearObjectAttributeEntries(0x21,1)`; +1 | confirmado |
| 16 | 0x08021418 | = 0F | igual 0F | confirmado |
| 17 | 0x08021564 | = 10 | igual 10 | confirmado |
| 18, 19 | 0x080305E0, 0x0803080C | genéricos | ver compartilhados | confirmado |
| 20 | 0x08021638 | ..._S_20_WaitSlotAnimTimer | `Anim_RunSlotScript(rec+0xE0, 0x21)` até byte == 0xFF; timer = 0x3C; +1 | confirmado |
| 21 | 0x0802167C | ..._S_21_SetupSpritesAfterTimer | countdown do timer; ao zerar copia 2 blocos de 0x40 (sprites), grava x = 0x58/0x40/0x28, attr 6, y = 0x88 (ou 0x68 se bit0 de direção); +1 | confirmado |
| 22 | 0x08021734 | ..._S_22_StepThreeSlotsTowardTarget | `Fight_StepSlotTowardTarget` nos slots 0x21, 0x22, 0x23; quando os 3 chegam, +1 | confirmado |
| 23 | 0x08021774 | ..._S_23_CheckFlagOrJump25 | teste igual ao 0C; falha => SFX 0x3E e vai para 0x25; ok => zera byte, +1 | confirmado |
| 24 | 0x080217D8 | ..._S_24_MoveSpriteStep | 3 sprites andam ±8 por quadro (sinal por <8); byte == 0x10 => SFX 0x3E e +1 | confirmado |
| 25 | 0x0802186C | ..._S_25_ClearSlotsAfterMove | byte == 8 => `ClearObjectAttributeEntries(0x21,3)`, +1; senão recua 8 | confirmado |
| 26, 27 | = 0F, = 10 | | | confirmado |
| 28, 29 | genéricos (0x080305E0, 0x0803080C) | | | confirmado |

## Despachante U (0x2B, 25 passos; entrada 0x08021E04; tabela 0x083ADC6C)
Fluxo: ROS; cura de status: zera status pendentes do alvo, liga flags visuais no alvo, SFX 0x37, msg 0x23. Sem dano e sem revive.
| passo | entrada | nome | efeito | conf. |
|---|---|---|---|---|
| 00 | 0x08021E2C | ..._U_00_CheckRosAndTargetStatus | checa ROS e status do alvo (corpo não relido) | provavel |
| 04..07 | = S_04..S_07 | compartilhados | ver compartilhados | confirmado |
| 08 | 0x08021EA0 | ..._U_08_LoadEffectGfxOnTarget | LZ + cpufastset; flags 0x200 (+0x1000 se bit0) no slot; posiciona sprite no alvo (+0x870); `Slot_SetupFromRecordList(rec+0xF0, 0x21)`; timer 0x3C; +1 | confirmado |
| 0A | 0x08021F90 | ..._U_0A_ScrollBgAndStepSlotToTarget | BG rola ±0x40 (bit0); `Fight_StepSlotTowardTarget(0x21)`; ao chegar limpa flags, zera rolagem, +1 | confirmado |
| 0B | 0x08022010 | ..._U_0B_ClearTargetStatus | zera pendentes de status do alvo (kind/part/E8/EC); seta flag no objeto do alvo, bytes +8/+9 = 0x40; SFX 0x37; +1 | confirmado |
| 0C | 0x080220A0 | ..._U_0C_RunAnimAndClearTargetSlot | corpo não relido (SU.md: anima e limpa slot do alvo) | provavel |
| 0D | 0x0802213C | ..._U_0D_SetMessage23 | msg 0x23; +1 | confirmado (SU.md) |
| 09 | 0x08030674 | compartilhado | ver compartilhados | confirmado |
| 01, 02, 03, 0E, 0F | compartilhados | | | confirmado |

## Despachante V (0x2C, 9 passos; entrada 0x08022154 via CANCEL.md; tabela 0x083ADCAC)
Fluxo: ROS; 4 orbes giram em volta do ator; alterna flag +0x58 do bloco e msg 0x72/0x73. Sem dano, status ou revive escritos.
| passo | entrada | nome | efeito | conf. |
|---|---|---|---|---|
| 00 | 0x0802217C | ..._V_00_CheckRos | checa ROS; +0xAA=4 em uso (SU.md) | provavel |
| 04 | 0x080221C8 | ..._V_04_SpawnFourOrbsOnActor | cria 4 orbes no ator; SFX 0x5C; +1 | confirmado (SU.md) |
| 05 | 0x080222A8 | ..._V_05_SpiralOrbsAroundActor | órbitas em espiral; +1 ao fim da escala | provavel (SU.md) |
| 06 | 0x08022380 | ..._V_06_ToggleFlag58Message | inverte flag +0x58; msg 0x72/0x73; +1 | confirmado (SU.md) |
| 01, 02, 03, 07, 08 | compartilhados | | | confirmado |

## Despachante W (0x2D, 11 passos; entrada não lida nesta rodada; tabela 0x083ADCEC)
Fluxo: ROS (variante pela técnica da peça); buff de carga no ator: bot `reactionStatusFlags |= 8`, `+0xDF` = técnica
da peça, `extraPenalty` (+0xD8) = 0 e += `attackAccum` até 0xFF. Efeito no próximo golpe fora do SU.
| passo | entrada | nome | efeito | conf. |
|---|---|---|---|---|
| 00 | 0x080223E4 | ..._W_00_CheckRosPickVariant | escolhe variante pela técnica; falha => +0xA6 = 0; sucesso => +0xAA = 4 | confirmado (SU.md) |
| 04 | 0x08022480 | ..._W_04_LoadVariantGfxTwoSprites | gfx de variante, 2 sprites; SFX 0x33; +1 | confirmado (SU.md) |
| 05 | 0x08022648 | ..._W_05_MoveSpritesInward | sprites vão para dentro; +1 | confirmado (SU.md) |
| 06 | 0x08022708 | ..._W_06_RunAnimUntilDone | anima slot até fim; +1 | confirmado (SU.md) |
| 07 | 0x08022748 | ..._W_07_SetFlag08AddValueToCounterD8 | ver acima; nome do mtbe mantido (SU.md chamava de ApplyActorChargeBuff) | confirmado (escrita); campo D8 = extraPenalty provavel |
| 08 | 0x080227C0 | ..._W_08_SetMessage1B | msg 0x1B; +1 | confirmado (SU.md) |
| 01, 02, 03, 09, 0A | compartilhados | | | confirmado |

## Despachante X (0x2E, 18 passos; entrada não lida nesta rodada; tabela 0x083ADD2C)
Fluxo: ROS; bloqueio (msg 0x39) se o alvo tem técnica 16 (passo 00); senão grava status 0x1F no alvo com os valores
de ROS total e `attackAccum` (clamp 1..99); anim de anel no alvo; msg 0x29 no fim.
| passo | entrada | nome | efeito | conf. |
|---|---|---|---|---|
| 00 | 0x08022800 | ..._X_00_CheckRosAndTargetTech16 | bloqueio se técnica 16 no alvo (SU2) | confirmado (SU2) |
| 04..07 | compartilhados S | | | confirmado |
| 08 | 0x08022870 | ..._X_08_LoadEffectGfxOnTarget | gfx de efeito no alvo; timer 0x3C; +1 | confirmado (SU.md) |
| 09 | 0x08022938 | ..._X_09_WaitAnimOrTimer (X,Y) | espera anim ou timer; +1 | confirmado (SU.md) |
| 0A | 0x08022990 | ..._X_0A_StepSlotToTarget (X,Y) | slot anda até o alvo; +1 | confirmado (SU.md) |
| 0B | 0x080229C0 | ..._X_0B_SlideOutAndClearSlot (X,Y) | slot 0x21 sai deslizando e limpa; +1 | confirmado (SU.md) |
| 0C | 0x08022A04 | ..._X_0C_SpawnRingSpritesOnTarget | sprites de anel no alvo; SFX 0x4A; +1 | confirmado (SU.md) |
| 0D | 0x08022B40 | ..._X_0D_ExpandRingAndClear (X,Y) | ver conflito 1 (mtbe: OrbitRingShrinkAndClear) | confirmado (SU.md) |
| 0E | 0x08022C28 | ..._X_0E_InflictStatus1F | alvo: `repairEffectA` = 0x1F (status), `field_0xC5` = 0, `repairEffectB` = clamp1..99(ROS total), `field_0xC7` = clamp1..99(`attackAccum`); +1 | confirmado |
| 0F | 0x08022C90 | ..._X_0F_SetMessage29 | msg 0x29; +1 | confirmado (SU.md) |
| 01, 02, 03 | compartilhados | | | confirmado |

## Despachante Y (0x2F, 18 passos; entrada não lida nesta rodada; tabela 0x083ADD8C)
Fluxo: ROS; só age se a cabeça do alvo tem armadura 0 (passo 00); revive: alvo presence = 3, reparte peças pela
capacidade com `attackAccum`, armadura da cabeça = `combatValue`, `botStatus = 1`, reposiciona objetos dos bots.
| passo | entrada | nome | efeito | conf. |
|---|---|---|---|---|
| 00 | 0x08022CD0 | ..._Y_00_CheckRosAndTargetDown | só age se armadura da cabeça do alvo == 0 (SU2) | confirmado (SU2) |
| 04..07 | compartilhados S | | | confirmado |
| 08 | 0x08022D4C | ..._Y_08_LoadEffectGfxOnTarget | gfx de efeito no alvo; timer 0x3C; +1 | confirmado (SU.md) |
| 09, 0A, 0B | compartilhados X | | | confirmado (SU.md) |
| 0C | 0x08022E14 | ..._Y_0C_SpawnRingSpritesOnTarget | anel no alvo; SFX 0x4B; +1 | confirmado (SU.md) |
| 0D | 0x08022B40 | = X_0D | ver conflito 1 | confirmado (SU.md) |
| 0E | 0x08022F50 | ..._Y_0E_ReviveTarget | presence = 3; `FightPreview_SplitCountByCapacity(alvo, 0, attackAccum)`; armadura parte 0 = `combatValue`; `botStatus = 1`; `Fight_RefreshDisplayedPartSpritesForActiveBots`; `Fight_PlaceActiveBotObjects`; +1 | confirmado |
| 0F | 0x08022FB4 | ..._Y_0F_CopyBlob14SetMessage28 | copia bloco do bot alvo para buffer (SU2: 0xE bytes); msg 0x28; +1 | provavel (ver conflito 2) |

## Técnicas que usam cada despachante
**Não fechado nesta rodada.** TQ2.md não lista os opcodes 0x29..0x2F; o mapeamento técnica/Medaforce -> roteiro
precisa de varredura das tabelas de roteiro na ROM (ainda não feita). Em aberto.

## Conflitos com mtbe.csv (para o Opus decidir)
1. `0x08022B40` (X_0D e Y_0D): mtbe = `Fight_DispatchOnSubstateAA_X_0D_OrbitRingShrinkAndClear`; SU.md =
   `ExpandRingAndClear`. Confiança baixa para os dois nomes; manter o mtbe até ver o corpo.
2. `0x08022FB4` (Y_0F): mtbe = `CopyBlob14SetMessage28`; SU.md = `PlayEffectSetMessage28`; SU2 diz 0xE bytes (nome do
   mtbe diz 14). Não decompilado nesta rodada.
3. `0x08022748` (W_07): mtbe = `SetFlag08AddValueToCounterD8` (confere com o corpo); SU.md = `ApplyActorChargeBuff`.
   Sugiro manter o mtbe.

## Em aberto
- Corpos não relidos: U_00 (0x08021E2C), U_0C (0x080220A0), V_00/V_04/V_05/V_06, W_00/W_04/W_05/W_06, X_00, X_08..X_0C,
  Y_00, Y_08, Y_0C, Y_0F. Descrições acima vêm de SU.md/SU2.md; "confirmado (SU.md)" não foi revisto neste Ghidra.
- Salto do S_0F para tabela não recuperada em 0x0802155C (a continuação dos passos 0F/16/26 não foi lida).
- Mapeamento técnica -> roteiro (opcodes 0x29, 0x2B..0x2F).
