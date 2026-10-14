# Relatorio 2 da fusao de *_simbolos.csv em symbols/mtbe.csv

Gerado por `merge2.py` (scratchpad, rodada HA/HC/FH/JT/DT/FL). Nao foi feito commit.

## Regras aplicadas

- Endereco 0x08XXXXXX normalizado: maiusculo, bit thumb zerado. Linhas novas inseridas na ordem de endereco.
- Mesmo endereco e mesmo nome: nada. Nome FUN_/sub_/DAT_/vazio/`?` ignorado.
- Nome diferente: troca se o comentario diz "confirmado" (fonte desta rodada: HA, HC, FH, JT, DT, FL); o nome antigo vai para o comentario como "(antes: X)". Senao mantem o mtbe e lista como conflito.
- Kind: func/function/handler/event_handler/state_handler/helper -> func_thumb. "provavel"/"chute" na coluna kind (linhas mal formadas da FL3) -> func_thumb, com a confianca no inicio do comentario.
- Kind em troca: passa a ser o kind do CSV. Nao apaga linha nenhuma. Coluna count vazia nas linhas novas.

## Resumo

| Item | Quantidade |
|---|---|
| Linhas no mtbe antes | 2592 |
| Linhas no mtbe depois | 3090 |
| Enderecos novos (adicionados) | 498 |
| Trocados (nome do mtbe -> nome confirmado desta rodada) | 81 |
| Ja iguais (mesmo nome) | 118 |
| Conflitos (nome diferente, mantido o mtbe) | 62 |
| Conflitos internos entre CSVs da rodada (escolhido o confirmado) | 3 |
| Ignorados: nome vazio | 0 |
| Ignorados: nome generico (FUN_/sub_/DAT_/?) | 29 |
| Linhas invalidas | 0 |

Linhas validas lidas por arquivo: DT=16, FH=12, FL=17, FL1=36, FL10=40, FL2=32, FL3=30, FL4=24, FL5=35, FL6=34, FL7=38, FL8=36, FL9=40, HA1=47, HA2=42, HA3=42, HA4=42, HA5=9, HA6=42, HA7=42, HA8=42, HC1=11, HC2=11, HC3=11, HC4=11, HC5=11, JT=11

### Adicionados por fonte

| Fonte | Adicionados |
|---|---|
| FH | 1 |
| FL | 17 |
| FL1 | 36 |
| FL10 | 40 |
| FL2 | 32 |
| FL3 | 30 |
| FL4 | 24 |
| FL5 | 35 |
| FL6 | 34 |
| FL7 | 38 |
| FL8 | 36 |
| FL9 | 40 |
| HA1 | 16 |
| HA2 | 17 |
| HA4 | 15 |
| HA5 | 2 |
| HA6 | 19 |
| HA7 | 23 |
| HA8 | 24 |
| HC4 | 1 |
| HC5 | 7 |
| JT | 11 |

## Trocados (renomeados por confirmado nesta rodada)

| Endereco | mtbe (antigo) | novo | fonte |
|---|---|---|---|
| 0x0801D318 | ActionFamily_SwordHammer_Handler | Fight_DispatchOnSubstateAA_B_00_CheckSourcePartBroken | HA1 |
| 0x0801D36C | ActionFamily_ImmediateAdvance_Handler | Fight_DispatchOnSubstateAA_B_03_EndResult1 | HA1 |
| 0x0801D3F8 | ActionFamily_AttackClear_Handler | Fight_DispatchOnSubstateAA_C_03_EndResult1 | HA1 |
| 0x0801D524 | ActionFamily_FireHandler | Fight_DispatchOnSubstateAA_D_02_WaitSlot9ReachTarget | HA1 |
| 0x0801D6A4 | ActionFamily_CancelGravHandler | Fight_DispatchOnSubstateAA_E_01_LoadPhaseGfx | HA2 |
| 0x0801D878 | Fight_AnimateFollowupSourceIntro | Fight_DispatchOnSubstateAA_E_03_AnimateBotEntry | HA2 |
| 0x0801D95C | Fight_AdvanceFollowupSourceApproach | Fight_DispatchOnSubstateAA_E_04_StepBotSlot | HA2 |
| 0x0801DAEC | ActionFamily_AutoRecover_Handler | Fight_DispatchOnSubstateAA_E_06_ShowPartsHud | HA2 |
| 0x0801DC74 | ActionFamily_Repair_Handler | Fight_DispatchOnSubstateAA_E_07_SpawnSlot21 | HA2 |
| 0x0801DD74 | ActionFamily_AntiAir_Handler | Fight_DispatchOnSubstateAA_E_09_RunSlot21Anim | HA2 |
| 0x0801DDDC | ActionFamily_Scout_Handler | Fight_DispatchOnSubstateAA_E_0B_SetSelectedTarget | HA2 |
| 0x0801DE44 | ActionFamily_DrainCharge_Handler | Fight_DispatchOnSubstateAA_F_00_CopyPartDataPair | HA2 |
| 0x0801DED4 | ActionFamily_Impair_Handler | Fight_DispatchOnSubstateAA_F_02_HoldFlag | HA2 |
| 0x0801E09C | ActionFamily_TeamAttack_Handler | Fight_DispatchOnSubstateAA_G_03_BuildBattleBackground | HA2 |
| 0x0801E200 | ActionFamily_Strengthen_Handler | Fight_DispatchOnSubstateAA_G_04_ResetSideSprites | HA2 |
| 0x0801E2B8 | ActionFamily_Confusion_Handler | Fight_DispatchOnSubstateAA_G_06_StepSideSequencesHold | HA2 |
| 0x0801E300 | ActionFamily_Counterattack_Handler | Fight_DispatchOnSubstateAA_H_00_LoadSpritesTiles | HA2 |
| 0x0801E3C0 | Fight_AdvanceSequenceFromResultFlags | Fight_DispatchOnSubstateAA_H_01_ChooseActionPath | HA2 |
| 0x0801E43C | ActionFamily_InterruptChange_Handler | Fight_DispatchOnSubstateAA_H_02_CheckBotFlag40 | HA2 |
| 0x0801E54C | ActionFamily_DefenseChange_Handler | Fight_DispatchOnSubstateAA_H_04_WaitSlotOffset10 | HA2 |
| 0x0801E9E0 | ActionFamily_InterruptChange_Handler_V2 | Fight_DispatchOnSubstateAA_H_10_SfxStep2F | HA2 |
| 0x0801E9FC | ActionFamily_AttackChange_Handler_V2 | Fight_DispatchOnSubstateAA_H_11_WaitPartIconRows | HA2 |
| 0x0801EBF8 | Fight_QueueDefenderHeadBreakIfHeadZero | Fight_DispatchOnSubstateAA_H_1A_QueueBrokenBotOrSkip | HA2 |
| 0x0801EED4 | Fight_DispatchOnSubstateAA_H_Substate24 | Fight_DispatchOnSubstateAA_H_24_JumpToStep0A | HA3 |
| 0x0801F374 | Fight_DispatchOnSubstateAA_H_Substate44 | Fight_DispatchOnSubstateAA_H_44_PlaySfx2EAndAdvance | HA3 |
| 0x0801F474 | Fight_DispatchOnSubstateAA_H_Substate4D | Fight_DispatchOnSubstateAA_H_4D_JumpTo3E | HA3 |
| 0x0801F5AC | Fight_DispatchOnSubstateAA_I_Substate01 | Fight_DispatchOnSubstateAA_I_01_EndHandlerNoEffect | HA3 |
| 0x08020040 | Fight_DispatchOnSubstateAA_K_Substate08 | Fight_DispatchOnSubstateAA_K_08_ResetStepToZero | HA3 |
| 0x08020078 | Fight_DispatchOnSubstateAA_L_Substate00 | Fight_DispatchOnSubstateAA_L_00_CheckFlagF6JumpTo2 | HA3 |
| 0x080205A4 | Fight_ShowInlineEffectTextForCurrentPart | Fight_DispatchOnSubstateAA_M_00_LoadGfxSpawnTargetObj | HA4 |
| 0x08020728 | Fight_ForceBreakSelectedPart | Fight_DispatchOnSubstateAA_M_04_ApplyPartDamage | HA4 |
| 0x08020844 | Fight_ResolveSelectedPartAttackBaseStep | Fight_DispatchOnSubstateAA_O_00_ApplyPenaltyOrMsg | HA4 |
| 0x08020928 | Fight_SelectRepairTargetPart | Fight_DispatchOnSubstateAA_P_0B_RepairPartOrSkip | HA4 |
| 0x080209D8 | Fight_ResolveSelectedPartAttackBaseStepIfUsable | Fight_DispatchOnSubstateAA_Q_00_PenaltyIfFlagClearOrMsg | HA4 |
| 0x08020AD0 | Fight_SelectBrokenPartDamageTarget | Fight_DispatchOnSubstateAA_R_0C_SplitCountOrSkip | HA4 |
| 0x08020B4C | Fight_AdvanceRepairTickWithEffects | Fight_DispatchOnSubstateAA_R_08_LoadPartIconsAndJump | HA4 |
| 0x08021300 | Fight_EnsureMirroredSupportSummaryMatchesCurrentAction | Fight_DispatchOnSubstateAA_S_0C_CheckPartThenBranch | HA4 |
| 0x08021418 | Fight_AccumulateActionResultStatsByActionSlot | Fight_DispatchOnSubstateAA_S_0F_RecordDamageAndJump | HA4 |
| 0x08021774 | Fight_EnsureMirroredSupportSummaryMatchesTeamForm | Fight_DispatchOnSubstateAA_S_23_CheckFlagOrJump25 | HA4 |
| 0x080219C0 | Fight_DispatchOnSubstateAA_T_PlayThenSetup | Fight_DispatchOnSubstateAA_T_09_PlayThenSetup | HA5 |
| 0x08021AFC | Fight_DispatchOnSubstateAA_T_WaitSlotsReady | Fight_DispatchOnSubstateAA_T_0C_WaitSlotsReady | HA5 |
| 0x08021B78 | Fight_DispatchOnSubstateAA_T_WaitScriptEndClearMask | Fight_DispatchOnSubstateAA_T_0D_WaitScriptEndClearMask | HA5 |
| 0x08021C94 | Fight_DispatchOnSubstateAA_T_RunSlotsThenClear | Fight_DispatchOnSubstateAA_T_10_RunSlotsThenClear | HA5 |
| 0x08023A48 | Fight_CommitBoostChargeSecondaryEffect | Fight_DispatchOnSubstateAA_AB_CopyDisplayValues | HA6 |
| 0x08023FB8 | Fight_CommitDrainChargePrimaryAndSecondaryEffects | Fight_DispatchOnSubstateAA_AD_WriteBotDisplayRows | HA6 |
| 0x08036AC4 | FightPreview_AdvanceSlotX | Fight_AdvanceSlotXByWaveTable | FH |
| 0x08036B84 | FightPreview_SetPartSpriteId | Fight_CopyPartArmorToDisplayValues | FH |
| 0x080370AC | FightPreview_TickSpriteTimersCount | Fight_TickSlotScriptsUntilDone | FH |
| 0x0803725C | Fight_CheckRosMeetsMinimumThreshold | Fight_SpendRos20ReturnsExhausted | FH |
| 0x0803769C | Fight_GetActionProficiencyBase | Fight_RollPartValue | FH |
| 0x0804DC34 | NoOp | Event_Op00_Cont_NoOp | HC1 |
| 0x0804DD04 | EventOpcode03_CloseMessageBox | Event_Op03_Cont_CloseMessageBox | HC1 |
| 0x0804DD18 | EventOpcode04_WaitFrames | Event_Op04_Cont_WaitFrames | HC1 |
| 0x0804DD5C | EventOpcode07_FadeToBlack | Event_Op07_Cont_FadeToBlack | HC1 |
| 0x0804DD70 | EventOpcode08_FadeFromBlack | Event_Op08_Cont_FadeFromBlack | HC1 |
| 0x0804DDB4 | EventOpcode0B_WarpA_ReloadDestinationScene | Event_Op0B_Cont_WarpA_LoadScene | HC1 |
| 0x0804DDC0 | EventOpcode0C_WarpB_ReloadDestinationScene | Event_Op0C_Cont_WarpB_LoadScene | HC1 |
| 0x0804E43C | EventOpcode1A_NpcDoAnimation | Event_Op1A_Cont_NpcDoAnimation | HC1 |
| 0x0804E450 | EventOpcode1D_HopEventNpc | Event_Op1D_Cont_HopEventNpc | HC1 |
| 0x0804E464 | EventOpcode1E_FlickerEventNpc | Event_Op1E_Cont_FlickerNpc | HC2 |
| 0x0804E480 | EventOpcode1F_InitiateMovingActorA | Event_Op1F_Cont_InitiateMovingActorA | HC2 |
| 0x0804E48C | EventOpcode20_InitiateMovingActorB | Event_Op20_Cont_InitiateMovingActorB | HC2 |
| 0x0804E51C | EventOpcode23_MoveActorA | Event_Op23_Cont_MoveActorA | HC2 |
| 0x0804E528 | EventOpcode24_MoveActorB | Event_Op24_Cont_MoveActorB | HC2 |
| 0x0804E5A8 | EventOpcode27_ActorDoAnimation | Event_Op27_Cont_ActorDoAnimation | HC2 |
| 0x0804E5BC | EventOpcode28_HopActor | Event_Op28_Cont_HopActor | HC2 |
| 0x0804E5D0 | EventOpcode29_MoveAllActorsA | Event_Op29_Cont_MoveAllTrackedActorsA | HC2 |
| 0x0804E5E0 | EventOpcode2A_MoveAllActorsB | Event_Op2A_Cont_MoveAllTrackedActorsB | HC2 |
| 0x0804E7DC | EventOpcode2E_NoOp | Event_Op2E_Cont_NoOp | HC2 |
| 0x0804E7E0 | EventOpcode2F_NoOp | Event_Op2F_Cont_NoOp | HC2 |
| 0x0804E7E4 | EventOpcode30_NoOp | Event_Op30_Cont_NoOp | HC3 |
| 0x0804E860 | EventOpcode32_NoOrYesBox_Continuation | Event_Op32_Cont_NoOrYesBox | HC3 |
| 0x0804F5C8 | NoOp_0804F5C8 | Event_Op51_Cont_NoOp | HC3 |
| 0x0804F5CC | NoOp_0804F5CC | Event_Op52_Cont_NoOp | HC3 |
| 0x0804F5D0 | NoOp_0804F5D0 | Event_Op53_Cont_NoOp | HC3 |
| 0x0804F5D4 | NoOp_0804F5D4 | Event_Op54_Cont_NoOp | HC3 |
| 0x0804F5D8 | NoOp_0804F5D8 | Event_Op55_Cont_NoOp | HC3 |
| 0x0804F5DC | EventOpcode56_LoadEntities | Event_Op56_Cont_LoadEntities | HC3 |
| 0x0804F83C | EventOpcode66_AnimateTrackedObjectTransition | Event_Op66_Cont_Animate_Tracked_Object_Transition | HC5 |
| 0x0804FB1C | EventOpcode76_RunFlow4ThenSceneCommand5 | Event_Op76_Cont_Start_Flow4_Then_Scene_Command_05 | HC5 |
| 0x0804FC6C | EventOpcode7E_MoveAllTrackedActorsC | Event_Op7E_Cont_Move_All_Tracked_Actors_C | HC5 |

## Conflitos mtbe x novo (decisao humana; mantido o mtbe)

| Endereco | mtbe | novo | fonte | motivo |
|---|---|---|---|---|
| 0x0801D3A8 | ActionFamily_Press_Handler | Fight_DispatchOnSubstateAA_C_00_CheckTargetPartBroken | HA1 | comentario sem "confirmado" |
| 0x0801D45C | ActionFamily_HoldHandler | Fight_DispatchOnSubstateAA_D_01_ClearSlot9AndPlaceSprites | HA1 | comentario sem "confirmado" |
| 0x0801D9DC | ActionFamily_Recovery_Handler | Fight_DispatchOnSubstateAA_E_05_SlideBotIntoPlace | HA2 | comentario sem "confirmado" |
| 0x0801DD30 | ActionFamily_Reactivate_Handler | Fight_DispatchOnSubstateAA_E_08_WaitCounterThenSfx30 | HA2 | comentario sem "confirmado" |
| 0x0801DF0C | ActionFamily_Sacrifice_Handler | Fight_DispatchOnSubstateAA_G_00_FillPaletteAndTiles | HA2 | comentario sem "confirmado" |
| 0x0801DF60 | Fight_DispatchOnSubstateAA_G_SetBgControl | Fight_DispatchOnSubstateAA_G_01_SlideBgSpritesTen | HA2 | comentario sem "confirmado" |
| 0x0801E474 | ActionFamily_AttackChange_Handler | Fight_DispatchOnSubstateAA_H_03_LoadGfxAndInitObj | HA2 | comentario sem "confirmado" |
| 0x0801ECC0 | Fight_DispatchOnSubstateAA_H_Substate20 | Fight_DispatchOnSubstateAA_H_20_ApplyPartDamage | HA3 | comentario sem "confirmado" |
| 0x0801ED50 | Fight_DispatchOnSubstateAA_H_Substate21 | Fight_DispatchOnSubstateAA_H_21_LoadBotSpriteAndPlace | HA3 | comentario sem "confirmado" |
| 0x0801EEE4 | Fight_DispatchOnSubstateAA_H_Substate28 | Fight_DispatchOnSubstateAA_H_28_LoadEffectSpriteAtBot | HA3 | comentario sem "confirmado" |
| 0x0801EFC8 | Fight_DispatchOnSubstateAA_H_Substate29 | Fight_DispatchOnSubstateAA_H_29_WaitSpriteMoveThenTimer60 | HA3 | comentario sem "confirmado" |
| 0x0801F018 | Fight_DispatchOnSubstateAA_H_Substate30 | Fight_DispatchOnSubstateAA_H_30_LoadEffectSpriteAtBot | HA3 | comentario sem "confirmado" |
| 0x0801F0F0 | Fight_DispatchOnSubstateAA_H_Substate38 | Fight_DispatchOnSubstateAA_H_38_LoadEffectSpriteAtBot | HA3 | comentario sem "confirmado" |
| 0x0801F1C8 | Fight_DispatchOnSubstateAA_H_Substate3B | Fight_DispatchOnSubstateAA_H_3B_PickRepairablePartOrSkip | HA3 | comentario sem "confirmado" |
| 0x0801F214 | Fight_DispatchOnSubstateAA_H_Substate3E | Fight_DispatchOnSubstateAA_H_3E_LoadGfxSpawnEffectAtBot | HA3 | comentario sem "confirmado" |
| 0x0801F290 | Fight_DispatchOnSubstateAA_H_Substate3F | Fight_DispatchOnSubstateAA_H_3F_WaitSlotAnimEndThenClear | HA3 | comentario sem "confirmado" |
| 0x0801F2DC | Fight_DispatchOnSubstateAA_H_Substate41 | Fight_DispatchOnSubstateAA_H_41_ShowNumberSpriteRowAtBot | HA3 | comentario sem "confirmado" |
| 0x0801F390 | Fight_AdvanceRepairTickWithStatusRefresh | Fight_DispatchOnSubstateAA_H_45_DrawPartIconsWaitTimer | HA3 | comentario sem "confirmado" |
| 0x0801F3F0 | Fight_DispatchOnSubstateAA_H_Substate47 | Fight_DispatchOnSubstateAA_H_47_RepairPartOrJump4A | HA3 | comentario sem "confirmado" |
| 0x0801F4AC | Fight_TryStartRepairFollowup | Fight_DispatchOnSubstateAA_I_00_ClassifyPartStateJump2 | HA3 | comentario sem "confirmado" |
| 0x0801F5BC | Fight_DispatchOnSubstateAA_I_Substate02 | Fight_DispatchOnSubstateAA_I_02_LoadPartStateSpriteSetupSlot11 | HA3 | comentario sem "confirmado" |
| 0x0801F780 | Fight_DispatchOnSubstateAA_I_Substate03 | Fight_DispatchOnSubstateAA_I_03_AnimateSlot11ThenClear | HA3 | comentario sem "confirmado" |
| 0x0801F800 | Fight_DispatchOnSubstateAA_I_Substate04 | Fight_DispatchOnSubstateAA_I_04_PlaceTwoPartStateSprites | HA3 | comentario sem "confirmado" |
| 0x0801F9B0 | Fight_DispatchOnSubstateAA_I_Substate05 | Fight_DispatchOnSubstateAA_I_05_WaitSlotsThenTimer90 | HA3 | comentario sem "confirmado" |
| 0x0801F9EC | Fight_CommitPendingEffectFromCurrentAction | Fight_DispatchOnSubstateAA_I_07_ApplyPartStateEffectAtXY | HA3 | comentario sem "confirmado" |
| 0x0801FA68 | Fight_DispatchOnSubstateAA_J_Substate00 | Fight_DispatchOnSubstateAA_J_00_PreviewAdvanceSideSequences | HA3 | comentario sem "confirmado" |
| 0x0801FADC | Fight_DispatchOnSubstateAA_J_Substate01 | Fight_DispatchOnSubstateAA_J_01_PreviewSideStepSprites | HA3 | comentario sem "confirmado" |
| 0x0801FBC4 | Fight_DispatchOnSubstateAA_J_Substate02 | Fight_DispatchOnSubstateAA_J_02_SubDispatchByStep | HA3 | comentario sem "confirmado" |
| 0x0801FD88 | Fight_DispatchOnSubstateAA_J_Substate03 | Fight_DispatchOnSubstateAA_J_03_PreviewSideStepCounter | HA3 | comentario sem "confirmado" |
| 0x0801FE14 | Fight_DispatchOnSubstateAA_K_Substate00 | Fight_DispatchOnSubstateAA_K_00_ConsumePendingBotRefresh | HA3 | comentario sem "confirmado" |
| 0x0801FE64 | Fight_DispatchOnSubstateAA_K_Substate02 | Fight_DispatchOnSubstateAA_K_02_LoadGfxPlaceSprite21 | HA3 | comentario sem "confirmado" |
| 0x0801FEC8 | Fight_DispatchOnSubstateAA_K_Substate03 | Fight_DispatchOnSubstateAA_K_03_WaitSprite21Ready | HA3 | comentario sem "confirmado" |
| 0x0801FEE8 | Fight_ShowActionSourceMarker | Fight_DispatchOnSubstateAA_K_04_LoadGfxSetupSlot21 | HA3 | comentario sem "confirmado" |
| 0x0801FF9C | Fight_DispatchOnSubstateAA_K_Substate05 | Fight_DispatchOnSubstateAA_K_05_WaitSlot21ClearAttrMasks | HA3 | comentario sem "confirmado" |
| 0x08020004 | Fight_DispatchOnSubstateAA_K_Substate06 | Fight_DispatchOnSubstateAA_K_06_QueueMessageCode14 | HA3 | comentario sem "confirmado" |
| 0x080200A8 | Fight_DispatchOnSubstateAA_L_Substate02 | Fight_DispatchOnSubstateAA_L_02_LoadTwoGfxSetupSlot11 | HA3 | comentario sem "confirmado" |
| 0x080201DC | Fight_DispatchOnSubstateAA_L_Substate03 | Fight_DispatchOnSubstateAA_L_03_SlideSlot11ThenClear | HA3 | comentario sem "confirmado" |
| 0x08020248 | Fight_AdvanceMirroredCounterTransition | Fight_DispatchOnSubstateAA_L_04_CommitActionIfState3 | HA3 | comentario sem "confirmado" |
| 0x08020288 | Fight_DispatchOnSubstateAA_L_Substate06 | Fight_DispatchOnSubstateAA_L_06_LoadGfxResetCounterSfx | HA3 | comentario sem "confirmado" |
| 0x080202E0 | Fight_AdvanceActionSequenceAnimationStep | Fight_DispatchOnSubstateAA_L_07_SlideUntilMarker0x28ThenTimer60 | HA3 | comentario sem "confirmado" |
| 0x08020340 | Fight_DispatchOnSubstateAA_L_Substate09 | Fight_DispatchOnSubstateAA_L_09_BranchOnB8Value | HA3 | comentario sem "confirmado" |
| 0x0802036C | Fight_DispatchOnSubstateAA_L_Substate0B | Fight_DispatchOnSubstateAA_L_0B_LoadGfxSetupSlot10ThenJump | HA3 | comentario sem "confirmado" |
| 0x08020428 | Fight_DispatchOnSubstateAA_L_Substate0C | Fight_DispatchOnSubstateAA_L_0C_WaitSlot10AnimThenTimer60 | HA3 | comentario sem "confirmado" |
| 0x08021A14 | Fight_DispatchOnSubstateAA_T_WaitTimer | Fight_DispatchOnSubstateAA_T_0A_WaitTimer | HA5 | comentario sem "confirmado" |
| 0x08021A6C | Fight_DispatchOnSubstateAA_T_SetupPositions | Fight_DispatchOnSubstateAA_T_0B_SetupPositions | HA5 | comentario sem "confirmado" |
| 0x08021BD8 | Fight_DispatchOnSubstateAA_T_InitGraphicsAndSlots | Fight_DispatchOnSubstateAA_T_0F_InitGraphicsAndSlots | HA5 | comentario sem "confirmado" |
| 0x0804DC38 | EventOpcode01_ShowMessageA | Event_Op01_Cont_ShowMessageA | HC1 | comentario sem "confirmado" |
| 0x0804DCF8 | EventOpcode02_AdvanceEmbeddedInitOpcode | Event_Op02_Cont_Finish | HC1 | comentario sem "confirmado" |
| 0x0804E7E8 | EventOpcode31_YesOrNoBox_Continuation | Event_Op31_Cont_YesOrNoBox | HC3 | comentario sem "confirmado" |
| 0x0804E928 | EventOpcode36_GetItem | Event_Op36_Cont_GetItem | HC3 | comentario sem "confirmado" |
| 0x0804F0A4 | EventOpcode41_FlickerAllActors | Event_Op41_Cont_FlickerAllActors | HC3 | comentario sem "confirmado" |
| 0x0804F674 | NoOp_0804F674 | Event_Op57_Cont_NoOp | HC4 | comentario sem "confirmado" |
| 0x0804F678 | NoOp_0804F678 | Event_Op58_Cont_NoOp | HC4 | comentario sem "confirmado" |
| 0x0804F67C | EventOpcode59_OpenShop | Event_Op59_Cont_OpenShop | HC4 | comentario sem "confirmado" |
| 0x0804F6C4 | NoOp_0804F6C4 | Event_Op5A_Cont_NoOp | HC4 | comentario sem "confirmado" |
| 0x0804F6C8 | NoOp_0804F6C8 | Event_Op5B_Cont_NoOp | HC4 | comentario sem "confirmado" |
| 0x0804F6CC | NoOp_0804F6CC | Event_Op5C_Cont_NoOp | HC4 | comentario sem "confirmado" |
| 0x0804F6D0 | NoOp_0804F6D0 | Event_Op5D_Cont_NoOp | HC4 | comentario sem "confirmado" |
| 0x0804F6D4 | NoOp_0804F6D4 | Event_Op5E_Cont_NoOp | HC4 | comentario sem "confirmado" |
| 0x0804F6D8 | NoOp_0804F6D8 | Event_Op5F_Cont_NoOp | HC4 | comentario sem "confirmado" |
| 0x0804F6DC | NoOp_0804F6DC | Event_Op62_Cont_NoOp | HC4 | comentario sem "confirmado" |
| 0x0804F860 | NoOp_0804F860 | Event_Op6A_Cont_NoOp | HC5 | comentario sem "confirmado" |

## Conflitos internos entre CSVs da rodada (escolhido o de comentario confirmado)

| Endereco | escolhido | fonte | descartados |
|---|---|---|---|
| 0x0801D738 | Fight_PrepareFollowupSourceDisplay | HA1 | Fight_DispatchOnSubstateAA_E_02_CopyBotBuffers (HA2) |
| 0x0801D878 | Fight_DispatchOnSubstateAA_E_03_AnimateBotEntry | HA2 | Fight_AnimateFollowupSourceIntro (HA1) |
| 0x0801D95C | Fight_DispatchOnSubstateAA_E_04_StepBotSlot | HA2 | Fight_AdvanceFollowupSourceApproach (HA1) |

