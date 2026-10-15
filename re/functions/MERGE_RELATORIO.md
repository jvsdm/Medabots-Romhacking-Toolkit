# Relatorio da fusao de *_simbolos.csv em symbols/mtbe.csv

Gerado por `merge_mtbe.py` (scratchpad). Nao foi feito commit.

## Resumo

| Item | Quantidade |
|---|---|
| Linhas no mtbe antes | 1134 |
| Linhas no mtbe depois | 2592 |
| Enderecos novos (adicionados) | 1458 |
| Substituidos (nome generico no mtbe -> nome real) | 0 |
| Ja iguais | 216 |
| Conflitos mtbe x novo (nome real diferente) | 258 |
| Decisoes manuais do MERGE_NOTAS aplicadas (renomear/manter/pendente) | 13 |
| Correcoes de comentario | 5 |
| Conflitos internos entre *_simbolos.csv (mesmo endereco) | 7 |
| Ignorados: nome generico (FUN_/sub_/DAT_...) sem entrada no mtbe | 62 |
| Ignorados: sem nome (ex.: code_nofunc vazio) | 55 |
| Linhas invalidas nos simbolos | 0 |
| Nomes repetidos no mtbe depois da fusao (mesmo nome, enderecos diferentes) | 3 |

Linhas lidas por arquivo: G=19, HA1=23, HA2=42, HA3=42, HA4=42, HA5=7, HA6=42, HA7=19, HA8=18, HE1=37, HE2=37, HE3=40, HE4=38, HN1=40, HN10=29, HN2=40, HN3=40, HN4=40, HN5=40, HN6=40, HN7=40, HN8=40, HN9=40, HO1=61, HO2=10, R0=210, R0a=23, R0b=24, R1=227, R1a=33, R1b=34, R2=226, R2a=34, R2b=35, R3=178, R4=245

## Conflitos mtbe x novo (decisao humana)

| Endereco | mtbe | novo | decisao | fonte | nota |
|---|---|---|---|---|---|
| 0x08000840 | NoOpActionHandler | ObjScript_Op00_Stub | mantido mtbe (nome real) | HO1 |  |
| 0x08004FD0 | LoadBattlePreviewSpriteTilesForAllSlots | desenha_9_slots_a | mantido mtbe (nome real) | R0 |  |
| 0x08005000 | BuildBattlePreviewSpriteObjectsForAllSlots | desenha_9_slots_b | mantido mtbe (nome real) | R0 |  |
| 0x0800508C | DrawBattlePreviewPartIcons | Menu_DrawPartIconGrid | mantido mtbe (nome real) | G |  |
| 0x08005118 | DrawBattlePreviewMedalLabel | desenha_icone_a | mantido mtbe (nome real) | R0 |  |
| 0x0800519C | DrawBattlePreviewNameLabel | desenha_icone_b | mantido mtbe (nome real) | R0 |  |
| 0x0800522C | DrawBattlePreviewPortraitTileBlock | carrega_paleta_icone | mantido mtbe (nome real) | R0 |  |
| 0x0800529C | InitializeBattlePreviewPortraitRenderer | init_estado_parte_batalha | mantido mtbe (nome real) | R0 |  |
| 0x080053C8 | RenderBattlePreviewPortraitFrame | desenha_parte_batalha | mantido mtbe (nome real) | R0 |  |
| 0x080054C0 | RenderBattlePreviewLargePortraitFrame | desenha_slot_batalha_5 | mantido mtbe (nome real) | R0 |  |
| 0x08005588 | HandleBattleMenuDirectionalInput | troca_selecao_parte | mantido mtbe (nome real) | R0 |  |
| 0x08005628 | InitializeBattlePreviewComparisonSprites | init_4_slots_b | mantido mtbe (nome real) | R0 |  |
| 0x080056A4 | UpdateBattlePreviewComparisonSpriteSlot | atualiza_slot_b | mantido mtbe (nome real) | R0 |  |
| 0x08005798 | RefreshBattlePreviewComparisonSprites | aplica_mascara_slots_b | mantido mtbe (nome real) | R0 |  |
| 0x080058A4 | HandleBattlePartPreviewToggle | processa_troca_slots | mantido mtbe (nome real) | R0 |  |
| 0x08005948 | RefreshBattlePartPreviewSprite | checa_parte_sinal | mantido mtbe (nome real) | R0 |  |
| 0x08005D1C | PartDetail_UpdateHeadAttributeMatchMarker | desenha_stats_parte | mantido mtbe (nome real) | R0 |  |
| 0x08006154 | PartDetail_UpdateLegsAttributeMatchMarker | desenha_stats_parte_d | mantido mtbe (nome real) | R0 |  |
| 0x0800656C | InitializeAlternateCompositePreviewFrame | Battle_InitPartSprites | mantido mtbe (nome real) | R0 |  |
| 0x0800667C | RenderAlternateCompositePreviewFrame | Battle_InitPartSpriteObj | mantido mtbe (nome real) | R0 |  |
| 0x08006764 | RenderAlternateCompositePreviewLargeFrame | Parts_CreateIconSprite | mantido mtbe (nome real) | R0 |  |
| 0x0800EE34 | PartSetPreview_DrawAttributeMatchMarkers | PartsGrid_DrawFourPartIcons | mantido mtbe (nome real) | R0 |  |
| 0x08018A54 | Fight_PlayBattleResultMusicByPostBattleMode | Fight_PlayResultCueByState | mantido mtbe (nome real) | R1 |  |
| 0x080191A4 | Fight_SubtractActionChargeMeterClampZero | Fight_SubBotMeterClampZero | mantido mtbe (nome real) | R1 |  |
| 0x08019440 | Fight_AdvanceTargetResolutionIndicatorForBot | Fight_UpdateBotScreenXFromMeterInverted | mantido mtbe (nome real) | R1 |  |
| 0x08019544 | Fight_UpdateTargetResolutionIndicatorAnimation | Fight_UpdateBotMeterDisplayObject | mantido mtbe (nome real) | R1 |  |
| 0x08019600 | Fight_UpdateSecondaryEffectIndicatorObjects | Fight_DrawSecondaryEffectIcon | mantido mtbe (nome real) | R1 |  |
| 0x08019B40 | Fight_DrawReactionSourceIcon | Fight_DrawPartStatusIcon | mantido mtbe (nome real) | R1 |  |
| 0x0801A0D0 | Fight_RefreshReactionIndicatorsForActiveBots | Fight_PlaceActiveBotObjects | mantido mtbe (nome real) | R1 |  |
| 0x0801A640 | Fight_SetSupportBonusSlotIndicesForCurrentTeam | Fight_UpdatePhaseFromCounter | mantido mtbe (nome real) | R1 |  |
| 0x0801B274 | Fight_ResetBotActionSelectionState | Fight_SelectBotActivePart | mantido mtbe (nome real) | R1 |  |
| 0x0801B3AC | Fight_CommitStagedPartReplacement | Fight_CommitActiveBotPartFields | mantido mtbe (nome real) | R1 |  |
| 0x0801B5FC | Fight_ResetBotResolutionAndActionScratch | Fight_ResetBotPartFlags | mantido mtbe (nome real) | R1 |  |
| 0x0801B69C | Fight_SelectRecoveryTargetFromPendingEffect | Fight_AssignBotTargetSlot | mantido mtbe (nome real) | R1 |  |
| 0x0801B76C | Fight_UpdateActionSlotAvailabilityState | Fight_ChooseBotActionCode | mantido mtbe (nome real) | R1 |  |
| 0x0801BC04 | Fight_CreateTargetResolutionIndicatorObject | Fight_SetupBotSpriteEntry | mantido mtbe (nome real) | R1 |  |
| 0x0801BE48 | Fight_GainPartProficiencyExperience | Fight_StepBotPartTimer | mantido mtbe (nome real) | R1 |  |
| 0x0801BF14 | Fight_SyncBotProficienciesToMedalState | Fight_SyncBotPartTimerToSlot | mantido mtbe (nome real) | R1 |  |
| 0x0801D6A4 | ActionFamily_CancelGravHandler | Fight_DispatchOnSubstateAA_E_LoadGfx | mantido mtbe (nome real) | HA2 |  |
| 0x0801DF0C | ActionFamily_Sacrifice_Handler | Fight_DispatchOnSubstateAA_G_LoadPaletteTiles | mantido mtbe (nome real) | HA2 |  |
| 0x0801E09C | ActionFamily_TeamAttack_Handler | Fight_DispatchOnSubstateAA_G_BuildBattleBackground | mantido mtbe (nome real) | HA2 |  |
| 0x0801E300 | ActionFamily_Counterattack_Handler | Fight_DispatchOnSubstateAA_H_LoadSpritesTiles | mantido mtbe (nome real) | HA2 |  |
| 0x0801F390 | Fight_AdvanceRepairTickWithStatusRefresh | Fight_DispatchOnSubstateAA_H_Substate45 | mantido mtbe (nome real) | HA3 |  |
| 0x0801F4AC | Fight_TryStartRepairFollowup | Fight_DispatchOnSubstateAA_I_Substate00 | mantido mtbe (nome real) | HA3 |  |
| 0x0801F9EC | Fight_CommitPendingEffectFromCurrentAction | Fight_DispatchOnSubstateAA_I_Substate07 | mantido mtbe (nome real) | HA3 |  |
| 0x0801FEE8 | Fight_ShowActionSourceMarker | Fight_DispatchOnSubstateAA_K_Substate04 | mantido mtbe (nome real) | HA3 |  |
| 0x08020248 | Fight_AdvanceMirroredCounterTransition | Fight_DispatchOnSubstateAA_L_Substate04 | mantido mtbe (nome real) | HA3 |  |
| 0x080202E0 | Fight_AdvanceActionSequenceAnimationStep | Fight_DispatchOnSubstateAA_L_Substate07 | mantido mtbe (nome real) | HA3 |  |
| 0x08020728 | Fight_ForceBreakSelectedPart | Fight_DispatchOnSubstateAA_M_04_ApplyPartDamage | mantido mtbe (nome real) | HA4 |  |
| 0x08020928 | Fight_SelectRepairTargetPart | Fight_DispatchOnSubstateAA_P_0B_RepairPartOrSkip | mantido mtbe (nome real) | HA4 |  |
| 0x08020AD0 | Fight_SelectBrokenPartDamageTarget | Fight_DispatchOnSubstateAA_R_0C_SplitCountOrSkip | mantido mtbe (nome real) | HA4 |  |
| 0x08020B4C | Fight_AdvanceRepairTickWithEffects | Fight_DispatchOnSubstateAA_R_08_LoadPartIconsAndJump | mantido mtbe (nome real) | HA4 |  |
| 0x08021300 | Fight_EnsureMirroredSupportSummaryMatchesCurrentAction | Fight_DispatchOnSubstateAA_S_0C_CheckPartThenBranch | mantido mtbe (nome real) | HA4 |  |
| 0x08021418 | Fight_AccumulateActionResultStatsByActionSlot | Fight_DispatchOnSubstateAA_S_0F_RecordDamageAndJump | mantido mtbe (nome real) | HA4 |  |
| 0x08021774 | Fight_EnsureMirroredSupportSummaryMatchesTeamForm | Fight_DispatchOnSubstateAA_S_23_CheckFlagOrJump25 | mantido mtbe (nome real) | HA4 |  |
| 0x08022748 | Fight_EnableStoredReactionSourceFromCurrentPart | Fight_DispatchOnSubstateAA_W_07_ApplyActorChargeBuff | mantido mtbe (nome real) | HN1 |  |
| 0x08022C28 | Fight_CommitAutoRecoverSecondaryEffect | Fight_DispatchOnSubstateAA_X_0E_InflictStatus1F | mantido mtbe (nome real) | HN1 |  |
| 0x08022F50 | Fight_CommitDirectRepairToHead | Fight_DispatchOnSubstateAA_Y_0E_ReviveTarget | mantido mtbe (nome real) | HN1 |  |
| 0x08023A48 | Fight_CommitBoostChargeSecondaryEffect | Fight_DispatchOnSubstateAA_AB_CopyDisplayValues | mantido mtbe (nome real) | HA6 |  |
| 0x08023FB8 | Fight_CommitDrainChargePrimaryAndSecondaryEffects | Fight_DispatchOnSubstateAA_AD_WriteBotDisplayRows | mantido mtbe (nome real) | HA6 |  |
| 0x08027494 | Fight_CommitMarkedTargetDamageResults | Fight_DispatchOnSubstateAA_AR_ApplyTeamPartDamage | mantido mtbe (nome real) | HN2 |  |
| 0x08028130 | Fight_AccumulateTeamFormSupportSummary | Fight_DispatchOnSubstateAA_AT_ApplyTeamSupportBonus | mantido mtbe (nome real) | HN2 |  |
| 0x08029B84 | Fight_ApplyRepairAcrossMarkedTargets | Fight_DispatchOnSubstateAA_AY_06_ApplyArmorRepair | mantido mtbe (nome real) | HN3 |  |
| 0x08029F50 | Fight_MarkTargetsWithBrokenPartsForDamageOverflow | Fight_DispatchOnSubstateAA_AZ_02_FlagAlliesWithBrokenParts | mantido mtbe (nome real) | HN3 |  |
| 0x08029FE0 | Fight_ApplyDamageOverflowAcrossMarkedTargets | Fight_DispatchOnSubstateAA_AZ_06_RestoreBrokenParts | mantido mtbe (nome real) | HN3 |  |
| 0x0802E1C8 | Fight_ApplySelectedPartDamageToMarkedTargets | Fight_DispatchOnSubstateAA_BL_12_DamageRandomPartOfTargets | mantido mtbe (nome real) | HN7 |  |
| 0x08034F68 | do_damage_on_part | Fight_ApplyDamageToPart | pendente humano | R1b | MERGE_NOTAS: "revisar" (do_damage_on_part x Fight_ApplyDamageToPart); mantido mtbe |
| 0x08035164 | Fight_SubtractPartArmor | Fight_SubtractPartArmorClampZero | mantido mtbe (nome real) | R1b |  |
| 0x08035194 | Fight_ApplyBrokenLegStatPenalty | Fight_BackupHalvedStatsIfFlagClear | mantido mtbe (nome real) | R1b |  |
| 0x080351E8 | Fight_RestoreLegEffectiveStats | Fight_CopyStatsIfFlagSet | mantido mtbe (nome real) | R1b |  |
| 0x08035740 | safe_add | Fight_AddByteClampToMax | mantido mtbe | R1b | MERGE_NOTAS: manter mtbe |
| 0x08035754 | safe_subtract | Fight_SubClampZero | mantido mtbe | R1b | MERGE_NOTAS: manter mtbe |
| 0x0803579C | Fight_GetPartProficiencyBase | Fight_AccumRosPart | pendente humano | R1b | MERGE_NOTAS: "revisar" (Fight_GetPartProficiencyBase x Fight_AccumRosPart); mantido mtbe |
| 0x08035814 | Fight_ApplyVirusBugRosPenalty | Fight_ApplyStatusCostIfFlag | mantido mtbe | R1b | MERGE_NOTAS: manter mtbe |
| 0x08035844 | Fight_GetBaseDefenderRosReduction | Fight_AccumRosBase | mantido mtbe (nome real) | R1b |  |
| 0x08035880 | Fight_IsAntiAirAgainstAirTarget | Fight_CheckPairCondition | mantido mtbe | R1b | MERGE_NOTAS: manter mtbe |
| 0x08036CCC | Fight_ClampRepairToMissingArmor | FightPreview_SplitCountByCapacity | mantido mtbe (nome real) | R2 |  |
| 0x08036D24 | Fight_RepairPartTick | FightPreview_FillPartSlot | mantido mtbe (nome real) | R2 |  |
| 0x08036E54 | Fight_ClampDisplayValue1To99 | FightPreview_ClampCount1To99 | mantido mtbe (nome real) | R2 |  |
| 0x08036E64 | Fight_SetPendingActionEffect | FightPreview_SetPartStatsByType | mantido mtbe (nome real) | R2 |  |
| 0x08036FA0 | Fight_ResetBotActionRuntimeState | Fight_ResetBotPartState | mantido mtbe (nome real) | R2 |  |
| 0x08036FF8 | Fight_SpawnInlineEffectTextObjects | FightPreview_SetupSpriteRow | mantido mtbe (nome real) | R2 |  |
| 0x0803713C | Fight_StartMirroredCounterActionPhase | Fight_CommitActionState | mantido mtbe (nome real) | R2 |  |
| 0x08037230 | does_part_exist_or_is_broken | FightPreview_IsPartSlotEdge | mantido mtbe (nome real) | R2 |  |
| 0x08037280 | Fight_ScaleAttackValueByPartScalar | Fight_ComputePartValue | mantido mtbe (nome real) | R2 |  |
| 0x080372D4 | Fight_ClampDamageToCurrentArmor | FightPreview_SplitCountByMax | mantido mtbe (nome real) | R2 |  |
| 0x08037498 | Fight_ClampHeadUsesToRemaining | FightPreview_SplitCountByFree | mantido mtbe (nome real) | R2 |  |
| 0x0803769C | Fight_GetActionProficiencyBase | Fight_RollPartValue | mantido mtbe (nome real) | R2 |  |
| 0x08037798 | Fight_FindBotWithReactionSource | Fight_FindTargetBotWithPart | mantido mtbe (nome real) | R2 |  |
| 0x0803836C | Fight_ClearMultiTargetResolutionScratch | FightPreview_ClearPartCounters3 | mantido mtbe (nome real) | R2 |  |
| 0x08038394 | Fight_ClearPrimaryAndSecondaryEffectBlocks | FightPreview_ClearPartFields | mantido mtbe (nome real) | R2 |  |
| 0x08039E00 | FightPreview_BeginSideEntranceSequence | ObjScript_Op14_SpawnActorOffscreen | mantido mtbe (nome real) | HO1 |  |
| 0x0803FD28 | Parse_text | ScriptVM_RunOpcodes | mantido mtbe (nome real) | R2 |  |
| 0x080402B4 | TextMeasureWord | Text_RenderGlyphStepVariant | mantido mtbe (nome real) | R2 |  |
| 0x08040CD0 | ClearObjectAttributeEntries | Tile_ClearSlots | mantido mtbe (nome real) | R2 |  |
| 0x08040D0C | MaskObjectAttribute0ForEntries | Slot_ClearAttrMask | mantido mtbe (nome real) | R2 |  |
| 0x08040DB8 | BuildObjectTextFromGlyphStream | Text_LayoutGlyphSprites | mantido mtbe (nome real) | R2 |  |
| 0x08041D3C | BattleMedalState_FindRecordIndexByMedalId | Slot_FindByIdPlus1 | mantido mtbe (nome real) | R2 |  |
| 0x08041D70 | BattleMedalState_AllocateForMedal | Medabot_SpawnFromRecord | mantido mtbe (nome real) | R2 |  |
| 0x08041FE4 | BattleMedalState_ClearByMedalId | Slot_ReleaseById | mantido mtbe (nome real) | R2 |  |
| 0x08042028 | BattleMedalState_CopyIntoFreeRecord | Slot_CopyRecordIntoFree | mantido mtbe (nome real) | R2 |  |
| 0x0804206C | BattleMedalState_GetMedianLevel | Level_ComputeReferenceFromSlots | mantido mtbe (nome real) | R2 |  |
| 0x08042114 | PartInventory_GetOwnedAndEquippedCounts | Inv_ReadCounter16 | mantido mtbe (nome real) | R2 |  |
| 0x0804213C | PartInventory_AddOwnedCount | Inv_AddCountClamp99 | mantido mtbe (nome real) | R2 |  |
| 0x08042170 | PartInventory_RemoveOwnedCount | Inv_SubCountClampZero | mantido mtbe (nome real) | R2 |  |
| 0x08042CA0 | ComputeBattleSpriteStatOffsets | CompositeSprite_ComputeClampedOffsetWord | mantido mtbe (nome real) | R2 |  |
| 0x0804305C | BuildCompositeBattleSpriteObjects | Sprite_WriteComposite4Objects | pendente humano | G | MERGE_NOTAS: conferir se e o mesmo corpo de 0x08042E58; mantido mtbe |
| 0x080432A0 | AnimateCompositeBattleSpriteObjects | Sprite_UpdateComposite4Motion | mantido mtbe (nome real) | G |  |
| 0x080436AC | Part_FindNextOwnedPartWithMatchingGender | site_PartCycle | mantido mtbe (nome real) | R2 |  |
| 0x0804B6F4 | EventScript_FetchAndRunPrimaryOpcodeHandler | Script_DispatchHandler | mantido mtbe (nome real) | R3 |  |
| 0x0804B73C | EventInitOpcode00_NoOp | Event_Op00_Nop | mantido mtbe (nome real) | HE1 |  |
| 0x0804B740 | EventInitOpcode01_Skip3BytesAndMarkInitialized | Event_Op01_ShowMessageA | mantido mtbe (nome real) | HE1 |  |
| 0x0804B74C | EventInitOpcode02_Skip3BytesAndMarkInitialized | Event_Op02_ShowMessageB | mantido mtbe (nome real) | HE1 |  |
| 0x0804B758 | EventInit_Skip3BytesAndMarkInitialized | Event_ReadMessageArgsAndDefer | mantido mtbe (nome real) | HE1 |  |
| 0x0804B778 | EventInitOpcode03_MarkInitialized | Event_Op03_CloseMessageBox | mantido mtbe (nome real) | HE1 |  |
| 0x0804B78C | EventInitOpcode04_ReadByteAndMarkInitialized | Event_Op04_WaitXFrames | mantido mtbe (nome real) | HE1 |  |
| 0x0804B7B4 | EventInitOpcode05_MarkInitialized | Event_Op05_WaitForButtonPress | mantido mtbe (nome real) | HE1 |  |
| 0x0804B7C8 | EventInitOpcode06_ResetOverworldStateAndMarkInitialized | Event_Op06_End | mantido mtbe (nome real) | HE1 |  |
| 0x0804B858 | EventInitOpcode07_ResetAndStartScreenTransition | Event_Op07_FadeToBlack | mantido mtbe (nome real) | HE1 |  |
| 0x0804B8AC | EventInitOpcode08_StartScreenTransition | Event_Op08_FadeFromBlack | mantido mtbe (nome real) | HE1 |  |
| 0x0804B900 | EventInitOpcode09_ResetAndStartScreenTransitionWithArgs | Event_Op09_FadeToWhite | mantido mtbe (nome real) | HE1 |  |
| 0x0804B958 | EventInitOpcode0A_StartScreenTransitionWithArgs | Event_Op0A_FadeFromWhite | mantido mtbe (nome real) | HE1 |  |
| 0x0804B9B0 | EventInitOpcode0B_SetConfigFlagAndRead5Bytes | Event_Op0B_WarpA | mantido mtbe (nome real) | HE1 |  |
| 0x0804B9CC | EventInitOpcode0C_ClearConfigFlagAndRead5Bytes | Event_Op0C_WarpB | mantido mtbe (nome real) | HE1 |  |
| 0x0804B9E8 | EventInit_Read5ConfigBytesAndMarkInitialized | Script_ReadStateBlock | mantido mtbe (nome real) | R3 |  |
| 0x0804BA5C | EventInitOpcode0D_ReadFacingAndApplyToPlayer | Event_Op0D_PlayerFaceDirection | mantido mtbe (nome real) | HE1 |  |
| 0x0804BA9C | EventInitOpcode0E_StartMapTransitionFromStoredState | Event_Op0E_PlayerHopInPlace | mantido mtbe (nome real) | HE1 |  |
| 0x0804BAEC | EventInitOpcode0F_ReadByteAndMarkInitialized | Event_Op0F_PlayerHopInDirection | mantido mtbe (nome real) | HE1 |  |
| 0x0804BB14 | EventInitOpcode10_ResetMovementQueueFromArgs | Event_Op10_MovePlayer | mantido mtbe (nome real) | HE1 |  |
| 0x0804BBA0 | EventInitOpcode11_ReadByteAndMarkInitialized | Event_Op11_ChangePlayerCostume | mantido mtbe (nome real) | HE1 |  |
| 0x0804BBC8 | EventInitOpcode12_ClearEventFlagByPackedId | Event_Op12_AddEventPermanently | mantido mtbe (nome real) | HE1 |  |
| 0x0804BBF4 | EventInitOpcode13_SetEventFlagByPackedId | Event_Op13_RemoveEventPermanently | mantido mtbe (nome real) | HE1 |  |
| 0x0804BC20 | EventInitOpcode14_ClearCurrentEventActiveObjectIfMatched | Event_Op14_RemoveCurrentEventNow | mantido mtbe (nome real) | HE1 |  |
| 0x0804BC6C | EventInitOpcode15_ClearCurrentEventActiveFlag | Event_Op15_RemoveCurrentEventNowKeepCollision | mantido mtbe (nome real) | HE1 |  |
| 0x0804BCD4 | EventInitOpcode16_ClearActiveObjectByPackedId | Event_Op16_RemoveEventNow | mantido mtbe (nome real) | HE1 |  |
| 0x0804BD08 | EventInitOpcode17_SetEventFlagFieldByPackedId | Event_Op17_SetEventVariable | mantido mtbe (nome real) | HE1 |  |
| 0x0804BD44 | EventInitOpcode18_IncrementEventFlagFieldByPackedId | Event_Op18_IncrementEventVariable | mantido mtbe (nome real) | HE1 |  |
| 0x0804BD70 | EventInitOpcode19_CacheActiveObjectIdAndFacingVariant | Event_Op19_GotoEvent | mantido mtbe (nome real) | HE1 |  |
| 0x0804BDD8 | EventInitOpcode1A_ApplyCurrentEventFacingVariant | Event_Op1A_NpcDoAnimation | mantido mtbe (nome real) | HE1 |  |
| 0x0804BE50 | EventInitOpcode1B_UpdateCurrentEventFacingVariantFromInput | Event_Op1B_EventNpcFacePlayer | mantido mtbe (nome real) | HE1 |  |
| 0x0804BEC4 | EventInitOpcode1C_ReadFacingVariantAndApplyToCurrentEvent | Event_Op1C_RotateEventNpc | mantido mtbe (nome real) | HE1 |  |
| 0x0804BF34 | EventInitOpcode1D_StartTransitionForCurrentEventObject | Event_Op1D_HopEventNpc | mantido mtbe (nome real) | HE1 |  |
| 0x0804BFB4 | EventInitOpcode1E_SetTransitionArgsForCurrentEventObject | Event_Op1E_FlickerEventNpc | mantido mtbe (nome real) | HE1 |  |
| 0x0804C02C | EventOpcode1F_InitiateMovingActorA_Setup | Event_Op1F_InitiateMovingActorA | mantido mtbe (nome real) | HE1 |  |
| 0x0804C078 | EventOpcode20_InitiateMovingActorB_Setup | Event_Op20_InitiateMovingActorB | mantido mtbe (nome real) | HE1 |  |
| 0x0804C0C4 | EventOpcode21_InitiateActor_Setup | Event_Op21_InitiateActor | mantido mtbe (nome real) | HE1 |  |
| 0x0804C100 | EventOpcode22_RotateActor_Setup | Event_Op22_RotateActor | mantido mtbe (nome real) | HE1 |  |
| 0x0804C130 | EventOpcode23_MoveActorA_Setup | Event_Op23_MoveActorA | mantido mtbe (nome real) | HE2 |  |
| 0x0804C168 | EventOpcode24_MoveActorB_Setup | Event_Op24_MoveActorB | mantido mtbe (nome real) | HE2 |  |
| 0x0804C1A0 | EventOpcode25_UnloadActor_Setup | Event_Op25_UnloadActor | mantido mtbe (nome real) | HE2 |  |
| 0x0804C1C0 | EventOpcode26_FlickerActor_Setup | Event_Op26_FlickerActor | mantido mtbe (nome real) | HE2 |  |
| 0x0804C210 | EventOpcode27_ActorDoAnimation_Setup | Event_Op27_ActorDoAnimation | mantido mtbe (nome real) | HE2 |  |
| 0x0804C26C | EventOpcode28_HopActor_Setup | Event_Op28_HopActor | mantido mtbe (nome real) | HE2 |  |
| 0x0804C2C8 | EventOpcode29_SetNpcWalkAnimation | Event_Op29_MoveAllTrackedActorsA | mantido mtbe (nome real) | HE2 |  |
| 0x0804C2F0 | EventOpcode2A_SetNpcAnimationFrame | Event_Op2A_MoveAllTrackedActorsB | mantido mtbe (nome real) | HE2 |  |
| 0x0804C318 | EventOpcode2B_ConfigureActorAttachment | Event_Op2B_InitiateFreeTrackedActor | mantido mtbe (nome real) | HE2 |  |
| 0x0804C364 | EventOpcode2C_SetActorPriority | Event_Op2C_SetActorPriority | mantido mtbe (nome real) | HE2 |  |
| 0x0804C3AC | EventOpcode2D_SetActorTileOffset | Event_Op2D_MoveActorC | mantido mtbe (nome real) | HE2 |  |
| 0x0804C478 | EventOpcode30_RelativeLongJump | Event_Op30_RelativeLongJump | mantido mtbe (nome real) | HE2 |  |
| 0x0804C4B4 | EventOpcode31_YesOrNoBox_Setup | Event_Op31_YesOrNoBox | mantido mtbe (nome real) | HE2 |  |
| 0x0804C4F8 | EventOpcode32_NoOrYesBox_Setup | Event_Op32_NoOrYesBox | mantido mtbe (nome real) | HE2 |  |
| 0x0804C53C | EventOpcode33_StartBattle_Setup | Event_Op33_StartBattle | mantido mtbe (nome real) | HE2 |  |
| 0x0804C588 | EventOpcode34_RotateNpc | Event_Op34_RotateNpc | mantido mtbe (nome real) | HE2 |  |
| 0x0804C5E4 | EventOpcode35_SetTrackedActorsFacingVariant | Event_Op35_SetAllTrackedActorsFacingVariant | mantido mtbe (nome real) | HE2 |  |
| 0x0804C648 | EventOpcode36_GetItem_Setup | Event_Op36_GetItem | mantido mtbe (nome real) | HE2 |  |
| 0x0804C678 | EventOpcode37_LoseItem_Setup | Event_Op37_LoseItem | mantido mtbe (nome real) | HE2 |  |
| 0x0804C6A8 | EventOpcode38_GetPart_Setup | Event_Op38_GetPart | mantido mtbe (nome real) | HE2 |  |
| 0x0804C6C8 | EventOpcode39_LosePart_Setup | Event_Op39_LosePart | mantido mtbe (nome real) | HE2 |  |
| 0x0804C6E8 | EventOpcode3A_GetMoney_Setup | Event_Op3A_GetMoney | mantido mtbe (nome real) | HE2 |  |
| 0x0804C708 | EventOpcode3B_LoseMoney_Setup | Event_Op3B_LoseMoney | mantido mtbe (nome real) | HE2 |  |
| 0x0804C728 | EventOpcode3C_GetMedal_Setup | Event_Op3C_GetMedal | mantido mtbe (nome real) | HE2 |  |
| 0x0804C750 | EventOpcode3D_GetTinpet_Setup | Event_Op3D_GetTinpet | mantido mtbe (nome real) | HE2 |  |
| 0x0804C928 | EventOpcode42_NoOpWithArgs | Event_Op42_NoOp | mantido mtbe (nome real) | HE2 |  |
| 0x0804C9A0 | EventOpcode43_ApplyMapSceneModePolicy3 | Event_Op48_Play_Persistent_Music | mantido mtbe (nome real) | HE3 |  |
| 0x0804C9C4 | EventOpcode44_ApplyMapSceneModePolicy4 | Event_Op49_Play_Map_Music | mantido mtbe (nome real) | HE3 |  |
| 0x0804CB94 | EventOpcode4F_JumpIfPreviousSceneCommandFailed | Event_Op4F_Jump_If_Previous_Scene_Command_Failed | mantido mtbe (nome real) | HE3 |  |
| 0x0804CC5C | EventOpcode50_JumpIfCurrentActiveObjectIndexNotEqual | Event_Op51_Jump_If_Found_Part_Index_Not_Equal | mantido mtbe (nome real) | HE3 |  |
| 0x0804CCAC | EventOpcode51_RandomJump | Event_Op52_Random_Jump | mantido mtbe (nome real) | HE3 |  |
| 0x0804CCF4 | EventOpcode52_EnterDeferredState | Event_Op53_Set_Map_Sprite_Slot | mantido mtbe (nome real) | HE3 |  |
| 0x0804CD60 | EventOpcode53_SetObjectRenderMode | Event_Op54_NoOp | mantido mtbe (nome real) | HE3 |  |
| 0x0804CD64 | EventOpcode54_ReadByteAndEnterDeferredState | Event_Op55_NoOp | mantido mtbe (nome real) | HE3 |  |
| 0x0804CD68 | EventOpcode55_EnterDeferredState | Event_Op56_Load_Entities | mantido mtbe (nome real) | HE3 |  |
| 0x0804CD7C | EventOpcode56_Primary | Event_Op57_NoOp | mantido mtbe (nome real) | HE3 |  |
| 0x0804CD80 | EventOpcode57_Primary | Event_Op58_NoOp | mantido mtbe (nome real) | HE3 |  |
| 0x0804CD84 | EventOpcode58_Primary | Event_Op59_Open_Shop | mantido mtbe (nome real) | HE3 |  |
| 0x0804CDA4 | EventOpcode5A_SetSecondaryMarkerFacingVariant | Event_Op5A_Set_Secondary_Marker_Facing | mantido mtbe (nome real) | HE3 |  |
| 0x0804CE04 | EventOpcode5B_JumpIfFewerThanThreeCompletePartSets | Event_Op5B_Jump_If_Fewer_Than_Three_Complete_Part_Sets | mantido mtbe (nome real) | HE3 |  |
| 0x0804CE44 | EventOpcode5C_JumpIfMissingCompleteMonochromePartSet | Event_Op5C_Jump_If_Missing_Complete_Monochrome_Part_Set | mantido mtbe (nome real) | HE3 |  |
| 0x0804CE90 | EventOpcode5D_UpdateOverworldEventModeFlags | Event_Op5D_Update_Overworld_Event_Mode_Flags | mantido mtbe (nome real) | HE3 |  |
| 0x0804CEF8 | EventOpcode5E_SetOverworldEventModeBit0 | Event_Op5E_Set_Overworld_Event_Mode_Bit0 | mantido mtbe (nome real) | HE3 |  |
| 0x0804CF40 | EventOpcode5F_SetRandomEncounterModeFlags | Event_Op5F_Set_Random_Encounter_Mode_Flags | mantido mtbe (nome real) | HE3 |  |
| 0x0804CF5C | EventOpcode60_FocusEventObject | Event_Op60_Focus_Event_Object | mantido mtbe (nome real) | HE3 |  |
| 0x0804D00C | EventOpcode61_RestoreFocusedEventModeFlags | Event_Op61_Restore_Focused_Event_Mode_Flags | mantido mtbe (nome real) | HE3 |  |
| 0x0804D018 | EventOpcode62_EquipStarterParts | Event_Op62_Equip_Starter_Parts | mantido mtbe (nome real) | HE3 |  |
| 0x0804D024 | EventOpcode63_ShowEffect | Event_Op63_Show_Effect | mantido mtbe (nome real) | HE3 |  |
| 0x0804D084 | EventOpcode64_ResetOverworldUiRenderState | Event_Op64_Reset_Overworld_UI_Render_State | mantido mtbe (nome real) | HE3 |  |
| 0x0804D090 | EventOpcode65_NoOp | Event_Op65_NoOp | mantido mtbe (nome real) | HE3 |  |
| 0x0804D094 | EventOpcode66_AnimateTrackedObjectTransition_Setup | Event_Op66_Animate_Tracked_Object_Transition | mantido mtbe (nome real) | HE3 |  |
| 0x0804D0E0 | EventOpcode67_Primary | Event_Op67_Skip_Two_Bytes_Wait | mantido mtbe (nome real) | HE3 |  |
| 0x0804D100 | EventOpcode68_ClearCollisionForPackedObject | Event_Op68_Clear_Collision_For_Packed_Object | mantido mtbe (nome real) | HE3 |  |
| 0x0804D158 | EventOpcode69_SetMapSceneVariant | Event_Op69_Set_Map_Scene_Variant | mantido mtbe (nome real) | HE4 |  |
| 0x0804D204 | EventOpcode6A_Primary | Event_Op6A_NoOp | mantido mtbe (nome real) | HE4 |  |
| 0x0804D208 | EventOpcode6B_SetCurrentChapter | Event_Op6B_Set_Current_Chapter | mantido mtbe (nome real) | HE4 |  |
| 0x0804D270 | EventOpcode6D_Primary | Event_Op6D_Move_Actor_D | mantido mtbe (nome real) | HE4 |  |
| 0x0804D2A8 | EventOpcode6E_Primary | Event_Op6E_Initiate_Moving_Actor_D | mantido mtbe (nome real) | HE4 |  |
| 0x0804D2F4 | EventOpcode6F_Primary | Event_Op6F_Move_Actor_E | mantido mtbe (nome real) | HE4 |  |
| 0x0804D32C | EventOpcode70_Primary | Event_Op70_Run_Scene_Command_13 | mantido mtbe (nome real) | HE4 |  |
| 0x0804D340 | EventOpcode71_Primary | Event_Op71_Run_Scene_Command_00 | mantido mtbe (nome real) | HE4 |  |
| 0x0804D354 | EventOpcode72_Primary | Event_Op72_Run_Scene_Command_02 | mantido mtbe (nome real) | HE4 |  |
| 0x0804D368 | EventOpcode73_Primary | Event_Op73_Run_Scene_Command_08 | mantido mtbe (nome real) | HE4 |  |
| 0x0804D39C | EventOpcode78_RunSceneCommand10IfAtLeastThreeCompletePartSets | Event_Op78_Run_Scene_Command_10_If_At_Least_Three_Complete_Part_Sets | mantido mtbe (nome real) | HE4 |  |
| 0x0804D400 | EventOpcode79_RunSceneCommand12_Setup | Event_Op79_Run_Scene_Command_12 | mantido mtbe (nome real) | HE4 |  |
| 0x0804D424 | EventOpcode7A_RunSceneCommand11_Setup | Event_Op7A_Run_Scene_Command_11 | mantido mtbe (nome real) | HE4 |  |
| 0x0804D438 | EventOpcode7B_JumpIfNoSparePart | Event_Op7B_Jump_If_No_Spare_Part | mantido mtbe (nome real) | HE4 |  |
| 0x0804D48C | EventOpcode7C_ReservedNoOp | Event_Op7C_Reserved_NoOp | mantido mtbe (nome real) | HE4 |  |
| 0x0804D4AC | EventOpcode7E_MoveAllTrackedActorsC_Setup | Event_Op7E_Move_All_Tracked_Actors_C | mantido mtbe (nome real) | HE4 |  |
| 0x0804D4FC | EventOpcode80_BeginBatchedObjectCommandBlock | Event_Op80_Begin_Batched_Object_Command_Block | mantido mtbe (nome real) | HE4 |  |
| 0x0804D580 | EventOpcode81_QueueBatchedObjectCommand | Event_Op81_Queue_Batched_Object_Command | mantido mtbe (nome real) | HE4 |  |
| 0x0804D5DC | EventOpcode82_ExecuteBatchedObjectCommandBlock_Setup | Event_Op82_Execute_Batched_Object_Command_Block | mantido mtbe (nome real) | HE4 |  |
| 0x0804D5F0 | EventOpcode53_Primary | Event_Op83_Set_Object_Render_Mode | mantido mtbe (nome real) | HE4 |  |
| 0x0804D758 | EventOpcode84_SetMapSceneVariantWhenPlayerFacesUp | Event_Op84_Set_Map_Scene_Variant_When_Player_Faces_Up | mantido mtbe (nome real) | HE4 |  |
| 0x0804D7AC | EventOpcode56_Primary_0804D7AC | Event_Op86_Queue_Event_Object_Facing_Cycle | mantido mtbe (nome real) | HE4 |  |
| 0x0804D7E4 | EventOpcode57_Primary_0804D7E4 | Event_Op87_Execute_Event_Object_Facing_Cycle_Block | mantido mtbe (nome real) | HE4 |  |
| 0x0804D8CC | EventScript_ReadBytes | Script_ReadBytes | mantido mtbe (nome real) | R3 |  |
| 0x0804D8F4 | Map_FindActiveObjectIndexByPackedId | Table64_FindIndexByKey | mantido mtbe (nome real) | R3 |  |
| 0x0804D938 | Map_FindActiveObjectByPackedId | Table64_FindOffsetByKey | mantido mtbe (nome real) | R3 |  |
| 0x0804DBD0 | Map_SetTrackedObjectSlotByIndex | Event_SetTrackedActorSlot | mantido mtbe (nome real) | HE4 |  |
| 0x0804DBF0 | Map_GetTrackedObjectSlotByIndex | Event_GetTrackedActorSlot | mantido mtbe (nome real) | HE4 |  |
| 0x0804DDCC | Overworld_ReloadWarpDestinationScene | Map_LoadCurrentSceneResources | mantido mtbe (nome real) | R3 |  |
| 0x0804FA1C | EventOpcode70_RunSceneCommand13 | Event_Op70_Cont_Run_Scene_Command_13 | mantido mtbe (nome real) | HE4 |  |
| 0x08050168 | Overworld_AdvanceObjectWalkAnimationBySpeed | Map_TickObjectAnimationTimes | mantido mtbe (nome real) | G |  |
| 0x0805076C | Map_SetTrackedObjectFacingVariant | Map_SetObjectParamAndFrame | mantido mtbe (nome real) | R3 |  |
| 0x080508B4 | Map_TickObjectAnimation | Map_ObjectAnimStep | mantido mtbe (nome real) | R3 |  |
| 0x08050918 | Map_TickObjectHop | Map_ObjectArcStep | mantido mtbe (nome real) | R3 |  |
| 0x080509EC | Map_TickTrackedObjectTransition | Map_ObjectMoveStateStep | mantido mtbe (nome real) | R3 |  |
| 0x08050BC8 | Overworld_RunMapLoadCommandState | Map_ModeDispatch | mantido mtbe (nome real) | R3 |  |
| 0x080517F0 | Map_InitPlayerMapMarker | Overworld_InitPlayerSprites | mantido mtbe (nome real) | R3 |  |
| 0x08051988 | Overworld_AdvancePlayerStep | Overworld_WalkStepByDirection | mantido mtbe (nome real) | R3 |  |
| 0x08051CB8 | Overworld_HandlePlayerWalkInput | Overworld_ProcessDirectionInput | mantido mtbe (nome real) | R3 |  |
| 0x08051EFC | Overworld_UpdateFollowerFacingAndAnimation | Overworld_UpdateFollowerFacing | mantido mtbe (nome real) | R3 |  |
| 0x0805203C | Overworld_AdvanceFollowerStep | Overworld_AdvanceWalkStep | mantido mtbe (nome real) | R3 |  |
| 0x080521A8 | Map_UpdateIdleFacingEventAnimation | Overworld_TickRandomObjectAnim | mantido mtbe (nome real) | R3 |  |
| 0x08052204 | Map_RefreshType6MarkerAnimations | Map_TickAllObjectAnimations | mantido mtbe (nome real) | R3 |  |
| 0x08053238 | EventFlag_SetIndexedFieldByPackedId | EventField3Bit_SetByPackedId | mantido mtbe (nome real) | R3 |  |
| 0x08053280 | EventFlag_IncrementIndexedFieldByPackedId | EventField3Bit_IncrementByPackedId | mantido mtbe (nome real) | R3 |  |
| 0x080533E4 | Overworld_BeginUiSceneState | Map_InitObjectState | mantido mtbe (nome real) | R3 |  |
| 0x0805424C | Overworld_DispatchUiSceneState | Scene_DispatchStateHandler | mantido mtbe (nome real) | R3 |  |
| 0x080594C8 | Overworld_RunPostTransitionMapSpriteReload | Screen_HandleStateTransition | mantido mtbe (nome real) | R3 |  |
| 0x0805B0D8 | Shop_BuildSellablePartList | Shop_BuildAvailablePartList | mantido mtbe (nome real) | G |  |
| 0x0807CCC0 | CPUFastSet | bios_cpufastset | mantido mtbe (nome real) | R4 |  |
| 0x0807CCC4 | CPUSet | bios_cpuset | mantido mtbe (nome real) | R4 |  |
| 0x0807CCC8 | Div | bios_div | mantido mtbe (nome real) | R4 |  |
| 0x0807CCD0 | SWI_Div | bios_div_ret2 | mantido mtbe (nome real) | R4 |  |
| 0x0807CCD8 | LZ77UnCompVRAM | bios_lz77_vram | mantido mtbe (nome real) | R4 |  |
| 0x0807CCDC | LZ77UnCompWRAM | bios_lz77_wram | mantido mtbe (nome real) | R4 |  |
| 0x0807CCE0 | ObjAffineSet | bios_objaffineset | mantido mtbe (nome real) | R4 |  |
| 0x083CE374 | FightPreview_EntryCountBlocks | ObjectScript_ArgCountTable | mantido mtbe (nome real) | HO1 |  |
| 0x083D2630 | gEventOpcodeTableB | EventOpcodeContinuationTable | mantido mtbe (nome real) | HE1 |  |

## Decisoes do MERGE_NOTAS aplicadas

| Endereco | antes (mtbe) | depois | motivo |
|---|---|---|---|
| 0x080156F8 | Fight_IsPartActionAvailable | Fight_IsPartActionBlocked | MERGE_NOTAS: renomear (retorno 1 = BLOQUEADA) |
| 0x080183A4 | HandleSelectedMedabotPreviewCursorInput | HandleSelectedMedabotPreviewCursorInput | MERGE_NOTAS: "renomear" o cursor 3x3 da luta, sem nome novo indicado; mantido mtbe |
| 0x08034F68 | do_damage_on_part | do_damage_on_part | MERGE_NOTAS: "revisar" (do_damage_on_part x Fight_ApplyDamageToPart); mantido mtbe |
| 0x08035740 | safe_add | safe_add | MERGE_NOTAS: manter mtbe |
| 0x08035754 | safe_subtract | safe_subtract | MERGE_NOTAS: manter mtbe |
| 0x0803579C | Fight_GetPartProficiencyBase | Fight_GetPartProficiencyBase | MERGE_NOTAS: "revisar" (Fight_GetPartProficiencyBase x Fight_AccumRosPart); mantido mtbe |
| 0x08035814 | Fight_ApplyVirusBugRosPenalty | Fight_ApplyVirusBugRosPenalty | MERGE_NOTAS: manter mtbe |
| 0x08035880 | Fight_IsAntiAirAgainstAirTarget | Fight_IsAntiAirAgainstAirTarget | MERGE_NOTAS: manter mtbe |
| 0x08042E58 | CreateCompositeBattleSpriteObjects | CreateCompositeBattleSpriteObjects | MERGE_NOTAS: conferir se e o mesmo corpo de 0x0804305C; mantido mtbe |
| 0x0804305C | BuildCompositeBattleSpriteObjects | BuildCompositeBattleSpriteObjects | MERGE_NOTAS: conferir se e o mesmo corpo de 0x08042E58; mantido mtbe |
| 0x0804538C | ShiftCompositePreviewTemplateEntryX | LayoutCompositePreviewEntryOamGrid | MERGE_NOTAS: Ghidra (posiciona grade de OAM) |
| 0x08045A40 | IsCompositePreviewPointOnScreen | IsCompositePreviewPointOnScreen | MERGE_NOTAS: "conferir endereco" (README: e pool, função desde o sync de 2026-10-14 (nome: IsCompositePreviewPointOnScreen)); mantido mtbe |
| 0x08047BA8 | Overworld_StartMapTransitionEffect | Overworld_StartMapTransitionEffect | MERGE_NOTAS: nome errado (so chama snd_sfx_request); sem nome correto indicado |

## Correcoes de comentario (MERGE_NOTAS)

| Endereco | nome | trecho antigo | trecho novo | motivo |
|---|---|---|---|---|
| 0x08015390 | Fight_TrySelectActionFromSpecialityCycle | value 4 falls back to head selection | value 4 clears the source and returns 0 | MERGE_NOTAS: comentario errado ("4 cai no head") |
| 0x0804B6F4 | EventScript_FetchAndRunPrimaryOpcodeHandler | 0x083D224C | 0x083D222C | MERGE_NOTAS: base da tabela de opcodes e 0x083D222C (literal 0x0804B734) |
| 0x0804E7E8 | EventOpcode31_YesOrNoBox_Continuation | 0x083D224C | 0x083D222C | MERGE_NOTAS: base da tabela de opcodes e 0x083D222C (literal 0x0804B734) |
| 0x0804E860 | EventOpcode32_NoOrYesBox_Continuation | 0x083D224C | 0x083D222C | MERGE_NOTAS: base da tabela de opcodes e 0x083D222C (literal 0x0804B734) |
| 0x08072D5C | snd_cmd_end |  (Ghidra FUN_0807495c) |  | MERGE_NOTAS: comentario cita FUN_0807495c, que e outra funcao |

## Conflitos internos entre *_simbolos.csv

Escolhido: maior prioridade (HE/HO/HN > H* > R*a/b > R*/G), depois ordem alfabetica do arquivo.

| Endereco | escolhido | outros (arquivo: nome) | decisao |
|---|---|---|---|
| 0x0801D738 | HA1: Fight_PrepareFollowupSourceDisplay | HA2: Fight_DispatchOnSubstateAA_E_CopyBotBuffers | escolhido pela prioridade |
| 0x0801D878 | HA1: Fight_AnimateFollowupSourceIntro | HA2: Fight_DispatchOnSubstateAA_E_SetupObjects | escolhido pela prioridade |
| 0x080393BC | HO1: ObjectScript_RunUntilYield | R2: ObjectScript_RunUntilFlagThenFlush | escolhido pela prioridade |
| 0x0803AB9C | HO1: ObjScript_Op28_SaveObjPalsToBuffer | HO2: ObjScript_Op28_CopyFourBlocks | escolhido pela prioridade |
| 0x08041D04 | R1: memcpy | R2: Mem_CopyBytes_Dup | escolhido pela prioridade |
| 0x08047F9C | HE3: PartSet_CountCompleteEntries | HE4: Team_CountSlotsWithStatus3 | escolhido pela prioridade |
| 0x0804D490 | HE4: Event_Op7D_Set_Field_C32 | HN7: Event_Op7D | escolhido pela prioridade |

## Substituidos (nome generico -> nome real)

| Endereco | antes | depois | fonte |
|---|---|---|---|

## Kind diferente em enderecos substituidos (kind do mtbe mantido)

Nenhum.

## Nomes repetidos depois da fusao

| Nome | enderecos |
|---|---|
| FightPreview_AdvanceSequenceCursor | 0x0803D4AC, 0x0803D4AC |
| Map_TickObjectAnimation | 0x08049440, 0x080508B4 |
| snd_track_step | 0x08072D68, 0x08074118 |

## Ignorados e linhas invalidas

Genericos sem entrada no mtbe (nao adicionados): 62. Exemplos: 0x08042B80 FUN_08042b80 (G); 0x0806A748 FUN_0806a748 (G); 0x0806A860 FUN_0806a860 (G); 0x0806AA10 FUN_0806aa10 (G); 0x0806AAA4 FUN_0806aaa4 (G); 0x0806ADB0 FUN_0806adb0 (G); 0x08023118 ? (HA6); 0x08023274 ? (HA6)

Sem nome: 55. Exemplos: 0x0801D95C (HA2); 0x0801D9DC (HA2); 0x0801DAEC (HA2); 0x0801DC74 (HA2); 0x0801DD30 (HA2); 0x0801DD74 (HA2); 0x0801DDDC (HA2); 0x0801DE44 (HA2)

Invalidas: nenhuma

## Fora do escopo (nao esta no mtbe)

- Opcodes 0x2F (C# le 2 bytes; ROM le 1) e 0x42-0x45 (C# Play_Sound/Play_Music/Shake): sao C#, nao foram tocados.
- SND2/SND11 com "0xD8..0xFF" e tabela 0x083D222C indexada por 0x00..0x88: nao achado no mtbe.csv; corrigir na fonte.
- Comentario de 0x080156F8 (Fight_IsPartActionBlocked) ainda diz "Return nonzero when ... available": inverter na revisao humana.

