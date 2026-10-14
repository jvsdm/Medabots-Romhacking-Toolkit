# RM3 - variaveis de RAM (revisao parcial)

Trocas de nome (Ghidra renomeado com rename_data):
- 0x03006428 gMapCameraClampX -> gMapCameraScrollX; 0x0300642A gMapCameraClampY -> gMapCameraScrollY (leitura em Map_GetCameraScrollX/Y).
- 0x03006432/34 ikki_x/y -> gPlayerPixelX/Y (px do jogador, nao nome de personagem); 0x03006438 -> gPlayerWalkCooldown; 0x03006439 -> gPlayerQueuedDirection (chute).
- 0x03006447 gLoadFacingSource -> gPlayerFacingDir; 0x0300644E/50 str_ikki_x/y -> gSecondaryMarkerScreenX/Y.
- 0x03006463 dpad_input_3 -> gSecondaryMarkerFacing (escrito por Map_SetSecondaryMarkerFacing, nao e entrada).
- 0x030064A1 gEventScriptRuntimeState_vm_substate -> gObjTwoStageSubstate (escrito por Obj_TwoStageInitThenDispatch).
- 0x03006D3E gObjectPoseBaseByte -> gEquippedLegPartByte (byte de perna equipada, GH-CS2).

Achados:
- Map_UpdateBackgroundAnimations 0x08048178 confirma enabled/frame/delay (0x03006414-16).
- Overworld_ShouldTriggerRandomEncounter 0x080438EA: 0x0300640B e o contador de passos, 0x0300640C e a trava inicial.

Nao revisados nesta rodada (faltam xrefs): 0x030064F4/FC, 0x03006492, 0x03006506/07/0D/0E/0F, 0x03006D?? sp_usr/sp_irq/INTR_VECTOR (ponteiros fixos, nao mexer).
