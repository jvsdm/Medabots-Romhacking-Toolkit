# HC: tabela de continuacao dos opcodes de evento (0x083D2630 + 4*op)

Fonte: ponteiros u32 LE lidos da ROM EU (`Medabots - Metabee (Europe).gba`, offset de arquivo 0x3D2630), 137 opcodes 0x00..0x88.
Todos os ponteiros tem bit thumb = 1; a coluna de alvo mostra o endereco sem o bit.
Despachado por `Event_DispatchContinuation` (0x0804DC0C) via `table2[E+0xC92]`. Ver HE1..HE4.

| op | alvo | já é função? | nome atual no Ghidra / mtbe.csv |
|---|---|---|---|
| 0x00 | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x01 | 0x0804DC38 | sim | Event_Op01_Cont_ShowMessageA (HC1; mtbe antes: EventOpcode01_ShowMessageA) |
| 0x02 | 0x0804DCF8 | sim | Event_Op02_Cont_Finish (HC1; mtbe antes: EventOpcode02_AdvanceEmbeddedInitOpcode) |
| 0x03 | 0x0804DD04 | sim | Event_Op03_Cont_CloseMessageBox (HC1; mtbe antes: EventOpcode03_CloseMessageBox) |
| 0x04 | 0x0804DD18 | sim | Event_Op04_Cont_WaitFrames (HC1; mtbe antes: EventOpcode04_WaitFrames) |
| 0x05 | 0x0804DD3C | sim | FUN_0804dd3c (mtbe: EventOpcode05_WaitForButtonPress) |
| 0x06 | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x07 | 0x0804DD5C | sim | Event_Op07_Cont_FadeToBlack (HC1; mtbe antes: EventOpcode07_FadeToBlack) |
| 0x08 | 0x0804DD70 | sim | Event_Op08_Cont_FadeFromBlack (HC1; mtbe antes: EventOpcode08_FadeFromBlack) |
| 0x09 | 0x0804DD84 | sim | FUN_0804dd84 (mtbe: EventOpcode09_FadeToWhite) |
| 0x0A | 0x0804DD9C | sim | FUN_0804dd9c (mtbe: EventOpcode0A_FadeFromWhite) |
| 0x0B | 0x0804DDB4 | sim | Event_Op0B_Cont_WarpA_LoadScene (HC1; mtbe antes: EventOpcode0B_WarpA_ReloadDestinationScene) |
| 0x0C | 0x0804DDC0 | sim | Event_Op0C_Cont_WarpB_LoadScene (HC1; mtbe antes: EventOpcode0C_WarpB_ReloadDestinationScene) |
| 0x0D | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x0E | 0x0804DE74 | sim | FUN_0804de74 (mtbe: EventOpcode0E_PlayerHopInPlace) |
| 0x0F | 0x0804DF40 | sim | FUN_0804df40 (mtbe: EventOpcode0F_PlayerHopInDirection) |
| 0x10 | 0x0804E134 | sim | FUN_0804e134 (mtbe: EventOpcode10_MovePlayer) |
| 0x11 | 0x0804E370 | sim | FUN_0804e370 (mtbe: EventOpcode11_ChangePlayerCostume) |
| 0x12 | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x13 | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x14 | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x15 | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x16 | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x17 | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x18 | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x19 | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x1A | 0x0804E43C | sim | Event_Op1A_Cont_NpcDoAnimation (HC1; mtbe antes: EventOpcode1A_NpcDoAnimation) |
| 0x1B | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x1C | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x1D | 0x0804E450 | sim | Event_Op1D_Cont_HopEventNpc (HC1; mtbe antes: EventOpcode1D_HopEventNpc) |
| 0x1E | 0x0804E464 | sim | Event_Op1E_Cont_FlickerNpc (HC2; mtbe antes: EventOpcode1E_FlickerEventNpc) |
| 0x1F | 0x0804E480 | sim | Event_Op1F_Cont_InitiateMovingActorA (HC2; mtbe antes: EventOpcode1F_InitiateMovingActorA) |
| 0x20 | 0x0804E48C | sim | Event_Op20_Cont_InitiateMovingActorB (HC2; mtbe antes: EventOpcode20_InitiateMovingActorB) |
| 0x21 | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x22 | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x23 | 0x0804E51C | sim | Event_Op23_Cont_MoveActorA (HC2; mtbe antes: EventOpcode23_MoveActorA) |
| 0x24 | 0x0804E528 | sim | Event_Op24_Cont_MoveActorB (HC2; mtbe antes: EventOpcode24_MoveActorB) |
| 0x25 | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x26 | 0x0804E58C | sim | FUN_0804e58c (mtbe: EventOpcode26_FlickerActor) |
| 0x27 | 0x0804E5A8 | sim | Event_Op27_Cont_ActorDoAnimation (HC2; mtbe antes: EventOpcode27_ActorDoAnimation) |
| 0x28 | 0x0804E5BC | sim | Event_Op28_Cont_HopActor (HC2; mtbe antes: EventOpcode28_HopActor) |
| 0x29 | 0x0804E5D0 | sim | Event_Op29_Cont_MoveAllTrackedActorsA (HC2; mtbe antes: EventOpcode29_MoveAllActorsA) |
| 0x2A | 0x0804E5E0 | sim | Event_Op2A_Cont_MoveAllTrackedActorsB (HC2; mtbe antes: EventOpcode2A_MoveAllActorsB) |
| 0x2B | 0x0804E700 | sim | FUN_0804e700 (mtbe: EventOpcode2B_InitiateActor) |
| 0x2C | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x2D | 0x0804E784 | sim | FUN_0804e784 (mtbe: EventOpcode2D_RotateActor) |
| 0x2E | 0x0804E7DC | sim | Event_Op2E_Cont_NoOp (HC2; mtbe antes: EventOpcode2E_NoOp) |
| 0x2F | 0x0804E7E0 | sim | Event_Op2F_Cont_NoOp (HC2; mtbe antes: EventOpcode2F_NoOp) |
| 0x30 | 0x0804E7E4 | sim | Event_Op30_Cont_NoOp (HC3; mtbe antes: EventOpcode30_NoOp) |
| 0x31 | 0x0804E7E8 | sim | Event_Op31_Cont_YesOrNoBox (HC3; mtbe antes: EventOpcode31_YesOrNoBox_Continuation) |
| 0x32 | 0x0804E860 | sim | Event_Op32_Cont_NoOrYesBox (HC3; mtbe antes: EventOpcode32_NoOrYesBox_Continuation) |
| 0x33 | 0x0804E8C4 | sim | FUN_0804e8c4 (mtbe: EventOpcode33_StartBattle_Continuation) |
| 0x34 | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x35 | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x36 | 0x0804E928 | sim | Event_Op36_Cont_GetItem (HC3; mtbe antes: EventOpcode36_GetItem) |
| 0x37 | 0x0804EA18 | sim | FUN_0804ea18 (mtbe: EventOpcode37_LoseItem) |
| 0x38 | 0x0804EB0C | sim | FUN_0804eb0c (mtbe: EventOpcode38_GetPart) |
| 0x39 | 0x0804EC38 | sim | FUN_0804ec38 (mtbe: EventOpcode39_LosePart) |
| 0x3A | 0x0804ED88 | sim | FUN_0804ed88 (mtbe: EventOpcode3A_GetMoney) |
| 0x3B | 0x0804EE00 | sim | FUN_0804ee00 (mtbe: EventOpcode3B_LoseMoney) |
| 0x3C | 0x0804EE8C | sim | FUN_0804ee8c (mtbe: EventOpcode3C_GetMedal) |
| 0x3D | 0x0804EF5C | sim | FUN_0804ef5c (mtbe: EventOpcode3D_GetTinpet) |
| 0x3E | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x3F | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x40 | 0x0804EFFC | sim | FUN_0804effc (mtbe: EventOpcode40_RunPulsedScreenTransitionLoop) |
| 0x41 | 0x0804F0A4 | sim | Event_Op41_Cont_FlickerAllActors (HC3; mtbe antes: EventOpcode41_FlickerAllActors) |
| 0x42 | 0x0804F110 | sim | FUN_0804f110 (mtbe: EventOpcode42_MovePlayerOneStep) |
| 0x43 | 0x0804F3F8 | sim | FUN_0804f3f8 (mtbe: EventOpcode43_ShakeCamera) |
| 0x44 | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x45 | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x46 | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x47 | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x48 | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x49 | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x4A | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x4B | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x4C | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x4D | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x4E | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x4F | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x50 | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x51 | 0x0804F5C8 | sim | Event_Op51_Cont_NoOp (HC3; mtbe antes: NoOp_0804F5C8) |
| 0x52 | 0x0804F5CC | sim | Event_Op52_Cont_NoOp (HC3; mtbe antes: NoOp_0804F5CC) |
| 0x53 | 0x0804F5D0 | sim | Event_Op53_Cont_NoOp (HC3; mtbe antes: NoOp_0804F5D0) |
| 0x54 | 0x0804F5D4 | sim | Event_Op54_Cont_NoOp (HC3; mtbe antes: NoOp_0804F5D4) |
| 0x55 | 0x0804F5D8 | sim | Event_Op55_Cont_NoOp (HC3; mtbe antes: NoOp_0804F5D8) |
| 0x56 | 0x0804F5DC | sim | Event_Op56_Cont_LoadEntities (HC3; mtbe antes: EventOpcode56_LoadEntities) |
| 0x57 | 0x0804F674 | sim | Event_Op57_Cont_NoOp (HC4; mtbe antes: NoOp_0804F674) |
| 0x58 | 0x0804F678 | sim | Event_Op58_Cont_NoOp (HC4; mtbe antes: NoOp_0804F678) |
| 0x59 | 0x0804F67C | sim | Event_Op59_Cont_OpenShop (HC4; mtbe antes: EventOpcode59_OpenShop) |
| 0x5A | 0x0804F6C4 | sim | Event_Op5A_Cont_NoOp (HC4; mtbe antes: NoOp_0804F6C4) |
| 0x5B | 0x0804F6C8 | sim | Event_Op5B_Cont_NoOp (HC4; mtbe antes: NoOp_0804F6C8) |
| 0x5C | 0x0804F6CC | sim | Event_Op5C_Cont_NoOp (HC4; mtbe antes: NoOp_0804F6CC) |
| 0x5D | 0x0804F6D0 | sim | Event_Op5D_Cont_NoOp (HC4; mtbe antes: NoOp_0804F6D0) |
| 0x5E | 0x0804F6D4 | sim | Event_Op5E_Cont_NoOp (HC4; mtbe antes: NoOp_0804F6D4) |
| 0x5F | 0x0804F6D8 | sim | Event_Op5F_Cont_NoOp (HC4; mtbe antes: NoOp_0804F6D8) |
| 0x60 | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x61 | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x62 | 0x0804F6DC | sim | Event_Op62_Cont_NoOp (HC4; mtbe antes: NoOp_0804F6DC) |
| 0x63 | 0x0804F6E0 | sim | Event_Op63_Cont_ShowEffect (HC4; sem nome no mtbe antes) |
| 0x64 | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x65 | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x66 | 0x0804F83C | sim | Event_Op66_Cont_Animate_Tracked_Object_Transition (HC5; mtbe antes: EventOpcode66_AnimateTrackedObjectTransition) |
| 0x67 | 0x0804F850 | sim | Event_Op67_Cont_Skip_Two_Bytes_Wait (HC5; sem nome no mtbe antes) |
| 0x68 | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x69 | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x6A | 0x0804F860 | sim | Event_Op6A_Cont_NoOp (HC5; mtbe antes: NoOp_0804F860) |
| 0x6B | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x6C | 0x0804F864 | sim | Event_Op6C_Cont_Initiate_Moving_Actor_C (HC5; sem nome no mtbe antes) |
| 0x6D | 0x0804F8E8 | sim | FUN_0804f8e8 |
| 0x6E | 0x0804F940 | sim | FUN_0804f940 |
| 0x6F | 0x0804F9C4 | sim | FUN_0804f9c4 |
| 0x70 | 0x0804FA1C | sim | Event_Op70_Cont_Run_Scene_Command_13 (mtbe: EventOpcode70_RunSceneCommand13) |
| 0x71 | 0x0804FA5C | sim | Event_Op71_Cont_Run_Scene_Command_00 (HC5; sem nome no mtbe antes) |
| 0x72 | 0x0804FA9C | sim | Event_Op72_Cont_Run_Scene_Command_02 (HC5; sem nome no mtbe antes) |
| 0x73 | 0x0804FADC | sim | Event_Op73_Cont_Run_Scene_Command_08 (HC5; sem nome no mtbe antes) |
| 0x74 | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x75 | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x76 | 0x0804FB1C | sim | Event_Op76_Cont_Start_Flow4_Then_Scene_Command_05 (HC5; mtbe antes: EventOpcode76_RunFlow4ThenSceneCommand5) |
| 0x77 | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x78 | 0x0804FB8C | sim | FUN_0804fb8c (mtbe: EventOpcode78_RunSceneCommand10) |
| 0x79 | 0x0804FBDC | sim | FUN_0804fbdc (mtbe: EventOpcode79_RunSceneCommand12) |
| 0x7A | 0x0804FC1C | sim | Event_Op7A_Cont_Run_Scene_Command_11 (HC5; sem nome no mtbe antes) |
| 0x7B | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x7C | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x7D | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x7E | 0x0804FC6C | sim | Event_Op7E_Cont_Move_All_Tracked_Actors_C (HC5; mtbe antes: EventOpcode7E_MoveAllTrackedActorsC) |
| 0x7F | 0x0804FC7C | sim | Event_Op7F_Cont_Copy_Tracked_Actor_Records (HC5; sem nome no mtbe antes) |
| 0x80 | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x81 | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x82 | 0x0804FD10 | sim | FUN_0804fd10 (mtbe: EventOpcode82_ExecuteBatchedObjectCommandBlock) |
| 0x83 | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x84 | 0x0804FE6C | sim | FUN_0804fe6c (mtbe: EventOpcode84_SetMapSceneVariantWhenPlayerFacesUp_Continuation) |
| 0x85 | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x86 | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |
| 0x87 | 0x0804FE9C | sim | FUN_0804fe9c |
| 0x88 | 0x0804DC34 | sim | Event_Op00_Cont_NoOp (HC1; mesmo alvo de 53 opcodes) |

Alvos unicos: 85; ja eram funcao antes de HC1..HC5: 30; nomeados e confirmados como funcao no Ghidra nesta rodada (HC1..HC5_simbolos.csv, HC_lista.txt): 55. Sem alvo pendente de virar funcao.
