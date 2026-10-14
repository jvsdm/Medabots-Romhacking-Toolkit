# CH7 - revisão dos chutes (FS6-FS10, FN4-FN6)

Status: concluído. 29 endereços revisados (55 linhas de chute, 26 excluídas pelo filtro de faixa). Renomes no Ghidra feitos.

## Trocas de nome (Ghidra e CSV)
- 0x0806B134 FUN_0806b134 -> BotAnim_StepMoveByPhase (provavel)
- 0x0806B1FC FUN_0806b1fc -> BotAnim_StepOffsetByPhaseB (provavel)
- 0x0806B248 FUN_0806b248 -> BotAnim_StepColorByPhase (provavel)
- 0x0806BB64 FUN_0806bb64 -> Fx_StepRowCells3State (provavel)
- 0x0806E038 FUN_0806e038 -> BotAnim_SetTimerByFlagAndAdvance (provavel)
- 0x0806E074 FUN_0806e074 -> BotAnim_StepWaitBlinkCycle (provavel)

## Achados
- Endereços com nome já no Ghidra (Anim_StepSlotScriptAndPosition, Obj_BuildActiveSlotMaskAndReset, Fx_StepCounterResetAtZero, BotAnim_StageWalkStep, etc.): o chute antigo estava desatualizado; CSV corrigido, sem rename.
- 0x08064A08: FS9 (FUN_08064A08) e FN5 (Obj_BuildActiveSlotMaskAndReset) são o mesmo endereço; nome Obj_* mantido (confirmado).
- 0x0806BB24, 0x0806BB44, 0x0806DC7C, 0x0806DBFC: nomes FN6 mantidos (provavel/confirmado pelo corpo).
- 0x0806E3F8 Scene_SetSubstate2Wrapper: só wrapper de Sel_RequestIfIndexBelow4(2); nome mantido, significado segue chute.
- 0x0807DD58 e 0x080EC9EE: Ghidra para em dado (halt_baddata). Bytes confirmam prólogo push; corpo não recuperado. Mantidos como FUN_, chute.
- Ficaram FUN_ (chute): 0x08068924, 0x08068978, 0x080692EC, 0x0806E9C0, 0x08073B2C, 0x0807DD58, 0x080EC9EE.
