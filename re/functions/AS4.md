# AS4: despachantes AS..AZ e Op40/44/48 - efeito verificado (GH-AS4)

Leitura: ROM original (so leitura) com `python -I`. Tabelas de passos lidas do literal em stub+0x24 (confirmado
nos 11 despachantes: ex. AS 0x08027960 -> tabela 0x083AE580). Decompile Ghidra so nos passos que escrevem dado de
batalha. Passos nao decompilados seguem por nome (provavel). Entradas de tabela vao ate o primeiro
`NoOpActionHandler` (0x08000840), que e preenchimento.

## Passos por despachante (tabela real)
- AO 0x08026884 (tab 0x83AE340): 00 H_00, 01 AO_01 RollPartValueForBot, 02 AO_02 ClampPartCounts, 03 AO_03 ShowFourPartValues,
  04/07/09/0D/0E AO_*, 10 AO_10 QueueHeadBreakIfDefenderHeadZero, demais H_*.
- AP 0x08026CF4 (tab 0x83AE3C0): 01 AP_01, 15 AP_15 ApplyDamageToRandomTargetPart, 18 AP_18 SetupHitEffectSprite,
  1B AP_1B LoopToStep02, demais H_*.
- AQ 0x08026ED0 (tab 0x83AE480): 00 AQ_00 PickActorPartToRepair, 02 AQ_02 SplitRepairAmountByCapacity,
  0D AQ_0D RepairPartByAmountCA, 13 AQ_13 JumpToStep04, demais H_*.
- AS 0x0802793C (tab 0x83AE580): 00 AS_SelectVariantByTechnique, 01 LoadIconAtActor, 02 SpinIcon, 03 SetupSecondSprite,
  04 ShrinkSecondSprite, 05 WaitTimerOrSkip, 06 ShrinkAndClearSprites, 07 SetStatusFlagByTechnique, 08 SetStepCodeByTechnique,
  09 StatusCheckFamily, 0A Shared_SetDone.
- AT 0x08027F5C (tab 0x83AE5C0): 04 LoadSupportIcon, 05 GrowIcon, 07 CheckSupportAlreadyActive, 08 PulseIcon,
  0B ApplyTeamSupportBonus, 0C SetStepCode50, 0D StatusCheckFamily, 0E SetDone.
- AU 0x08028248 (tab 0x83AE600): 04 LoadIconSprite, 06 SlideIcon, 07 BlinkThenClear, 08 SetActorReactionFlag2,
  09 SetStepCode64, 0A StatusCheckFamily, 0B SetDone.
- AV 0x08028404 (tab 0x83AE640): 00 SelectVariantByTechnique, 01 LoadEffectGfx, 02 WaitEffectSlots,
  03 ApplyStatusToOpponents, 04 BranchOnFlaggedTargets, 06 SpawnTargetEffectSlots, 07 WaitFlaggedSlotsArrived,
  09 ClearSlotsDispatchByType, 0A StatusCheckFamily, 0B SetDone.
- AW 0x08028C40 (tab 0x83AE680): 00 SelectStatusTypeFromTechnique, 01 LoadBannerByStatusType, 02 SlideBannerIn,
  03 ScrollBannerWaitTimer, 04/05 five-slot effect, 07 FlagAlliesSpawnEffects, 08 WaitAllyEffects,
  09 ApplyStatusGuardAndCure, 0A SetGuardMessageByType, 0B StatusCheckFamily, 0C SetDone.
- AX 0x080294B4 (tab 0x83AE6C0): 00 SetupCureBanner, 01 WaitEffectSlots, 02 FlagAlliesWithStatus, 04 SpawnCureEffects,
  05 WaitCureEffects, 06 ClearAllyStatus, 07 SetCureMessage, 08 StatusCheckFamily, 09 SetDone.
- AY 0x08029840 (tab 0x83AE700): 00 SetupRepairBanner, 01 WaitEffectSlots, 02 FlagRepairableAllies, 04 SpawnAllyHealEffects,
  05 WaitAllyHealEffects, 06 ApplyArmorRepair, 07 ShowAllyAmounts, 08 AnimateAmountsWait, 09 SetRepairMessage,
  0A StatusCheckFamily, 0B SetDone.
- AZ 0x08029E0C (tab 0x83AE740): 00 SetupReviveBanner, 01 WaitEffectSlots, 02 FlagAlliesWithBrokenParts,
  04/05 efeitos, 06 RestoreBrokenParts, 07/08 valores, 09 SetReviveMessage, 0A StatusCheckFamily, 0B SetDone.

## Efeito (decompile)
- **Op51 -> AS** (0x08027E1C etc.): le B+0xA8 do bot ativo (id de Medaforce; QA4). Se id-4 < 5 chama subpasso; senao
  AS_00 grava fight+0x51=5 e AS_07 seta bit 0x20 em B+0xD6. **Estado no proprio bot pela medaforca.** Confirmado.
- **Op40 -> AO**: AO_01 `Fight_RollPartValue` + `Fight_ComputeEffectValueFromRos` do bot ativo. Resto e exibicao e
  quebra de cabeca. **Rolagem de valor de peca, sem dano direto.** Confirmado.
- **Op44 -> AP**: AP_15 escolhe peca aleatoria do alvo, copia alvo e chama `Fight_ApplyDamageToPart` com fight+0xCA;
  AP_1B volta ao passo 02 (laco). **Dano em pecas aleatorias do alvo, repetido.** Confirmado.
- **Op48 -> AQ**: AQ_0D escolhe a peca mais danificada reparavel do ator e divide o reparo por capacidade.
  **Reparo de peca do proprio ator.** Confirmado.
- **Op52 -> AT**: AT_0B grava registro de apoio (+0x2A..+0x2D) do bot ativo com flag 'Z' e soma stats de apoio.
  **Bonus de apoio do time.** Confirmado (tipo de bonus provavel).
- **Op54 -> AV**: AV_03 percorre ate 3 adversarios com peca sem tecnica 16 e aplica o efeito pelo id (faixa
  0x0C..0x1C). **Estado em todos os adversarios.** Confirmado; status de cada id nao lido.
- **Op55 -> AW**: AW_09 seta bits de protecao em +0xCF dos aliados (bits 1/8, 2 ou 4 conforme fight+0x51). **Protecao
  contra status do time.** Confirmado.
- **Op56 -> AX**: AX_06 limpa status dos aliados; AX_02 marca quem tem status. **Cura de status do time.** Provavel:
  nao vi escrita em HP.
- **Op57 -> AY**: AY_06 para cada aliado marcado: peca mais danificada reparavel, divide por capacidade, soma em +0x59.
  **Reparo de armadura do time.** Confirmado.
- **Op58 -> AZ**: AZ_06 para cada aliado marcado: peca quebrada/ausente aleatoria, divide pelo maximo, soma em +0x59,
  chama `Fight_CopyStatsIfFlagSet`. **Restaura pecas do time.** Confirmado.
- **Op53 -> AU**: AU_08 seta flag de reacao do ator, AU_09 seta step code 0x64. Provavel.

## Ghidra (renomeados, 6; "Function renamed successfully")
0x08034684 Op40_RollPartValues; 0x0803476C Op44_DamageRandomTargetPart; 0x080348D0 Op48_RepairActorPart;
0x08034BBC Op51_SelfStatusByMedaforceId; 0x08034C10 Op54_StatusAllOpponents; 0x08034C48 Op56_TeamCureStatus.
Mantidos (confirmados): Op52_TeamSupportBonus, Op55_TeamProtectStatus, Op57_TeamRepairArmor, Op58_TeamRestoreParts.

## Pendente
- Op56: confirmar se ha cura de HP (nao vi escrita em HP no AX).
- Op54: mapa id -> status (AV_03 chama subpasso pelo id, nao lido).
- Op53 (AU_06/07), Op40 (AO_02/03/10), Op44 (AP_01/18) nao lidos.
- Nenhuma linha nova em pc/research/README.md ainda.
