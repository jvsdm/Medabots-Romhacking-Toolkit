# RV5 - revisão de nomes (60 handlers Fight_DispatchOnSubstateAA_V..AN)

Feito: 60 de 60 decompilados e registrados em `RV5_simbolos.csv` (confirmado/provavel; nenhum chute).
Renomeados no Ghidra (6):
- W_07: `Fight_DispatchOnSubstateAA_W_07_SetFlag08AddValueToCounterD8` (antes ApplyActorChargeBuff; sem "buff" no corpo).
- X_0D: `..._X_0D_OrbitRingShrinkAndClear` (antes ExpandRingAndClear; encolhe, nao expande).
- Y_0F: `..._Y_0F_CopyBlob14SetMessage28` (antes PlayEffectSetMessage28; so Mem_CopyBytes, sem efeito).
- AB: `..._AB_SetStatus26CopyDisplayValues` (antes CopyDisplayValues; grava tipo 0x26).
- AJ_00: `..._AJ_00_CheckRosAndTargetMedaforceZero` (antes AccumPartCheckTargetCost; le bot alvo +0xC1).
- AM_0A: `..._AM_0A_ClearSlotSetFlag10AddPartValueToCounterD8` (antes ClearSlotSetPartDisplayValue).
Achado: X_0E/AB/AD/AI_SetActionResultCodes gravam o bloco de status pendente (+0xC4/+0xC9, tipo + valores 1..99 em +0xC6/+0xC7 ou +0xCB/+0xCC).
Em aberto: nomes marcados "provavel" (AJ_13, AK_00, AI_LoadTechniqueGraphics tem tail call por jump table nao recuperada).
