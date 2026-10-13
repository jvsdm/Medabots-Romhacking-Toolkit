# HC: tabela de continuacao dos opcodes de evento (0x083D2630 + 4*op)

Fonte: ponteiros u32 LE lidos da ROM EU (`Medabots - Metabee (Europe).gba`, offset de arquivo 0x3D2630), 137 opcodes 0x00..0x88.
Todos os ponteiros tem bit thumb = 1; a coluna de alvo mostra o endereco sem o bit.
Despachado por `Event_DispatchContinuation` (0x0804DC0C) via `table2[E+0xC92]`. Ver HE1..HE4.

| op | alvo | já é função? | nome atual no Ghidra / mtbe.csv |
|---|---|---|---|
| 0x00 | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x01 | 0x0804DC38 | não | mtbe: EventOpcode01_ShowMessageA |
| 0x02 | 0x0804DCF8 | não | mtbe: EventOpcode02_AdvanceEmbeddedInitOpcode |
| 0x03 | 0x0804DD04 | não | mtbe: EventOpcode03_CloseMessageBox |
| 0x04 | 0x0804DD18 | não | mtbe: EventOpcode04_WaitFrames |
| 0x05 | 0x0804DD3C | sim | FUN_0804dd3c (mtbe: EventOpcode05_WaitForButtonPress) |
| 0x06 | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x07 | 0x0804DD5C | não | mtbe: EventOpcode07_FadeToBlack |
| 0x08 | 0x0804DD70 | não | mtbe: EventOpcode08_FadeFromBlack |
| 0x09 | 0x0804DD84 | sim | FUN_0804dd84 (mtbe: EventOpcode09_FadeToWhite) |
| 0x0A | 0x0804DD9C | sim | FUN_0804dd9c (mtbe: EventOpcode0A_FadeFromWhite) |
| 0x0B | 0x0804DDB4 | não | mtbe: EventOpcode0B_WarpA_ReloadDestinationScene |
| 0x0C | 0x0804DDC0 | não | mtbe: EventOpcode0C_WarpB_ReloadDestinationScene |
| 0x0D | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x0E | 0x0804DE74 | sim | FUN_0804de74 (mtbe: EventOpcode0E_PlayerHopInPlace) |
| 0x0F | 0x0804DF40 | sim | FUN_0804df40 (mtbe: EventOpcode0F_PlayerHopInDirection) |
| 0x10 | 0x0804E134 | sim | FUN_0804e134 (mtbe: EventOpcode10_MovePlayer) |
| 0x11 | 0x0804E370 | sim | FUN_0804e370 (mtbe: EventOpcode11_ChangePlayerCostume) |
| 0x12 | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x13 | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x14 | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x15 | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x16 | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x17 | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x18 | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x19 | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x1A | 0x0804E43C | não | mtbe: EventOpcode1A_NpcDoAnimation |
| 0x1B | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x1C | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x1D | 0x0804E450 | não | mtbe: EventOpcode1D_HopEventNpc |
| 0x1E | 0x0804E464 | não | mtbe: EventOpcode1E_FlickerEventNpc |
| 0x1F | 0x0804E480 | não | mtbe: EventOpcode1F_InitiateMovingActorA |
| 0x20 | 0x0804E48C | não | mtbe: EventOpcode20_InitiateMovingActorB |
| 0x21 | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x22 | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x23 | 0x0804E51C | não | mtbe: EventOpcode23_MoveActorA |
| 0x24 | 0x0804E528 | não | mtbe: EventOpcode24_MoveActorB |
| 0x25 | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x26 | 0x0804E58C | sim | FUN_0804e58c (mtbe: EventOpcode26_FlickerActor) |
| 0x27 | 0x0804E5A8 | não | mtbe: EventOpcode27_ActorDoAnimation |
| 0x28 | 0x0804E5BC | não | mtbe: EventOpcode28_HopActor |
| 0x29 | 0x0804E5D0 | não | mtbe: EventOpcode29_MoveAllActorsA |
| 0x2A | 0x0804E5E0 | não | mtbe: EventOpcode2A_MoveAllActorsB |
| 0x2B | 0x0804E700 | sim | FUN_0804e700 (mtbe: EventOpcode2B_InitiateActor) |
| 0x2C | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x2D | 0x0804E784 | sim | FUN_0804e784 (mtbe: EventOpcode2D_RotateActor) |
| 0x2E | 0x0804E7DC | não | mtbe: EventOpcode2E_NoOp |
| 0x2F | 0x0804E7E0 | não | mtbe: EventOpcode2F_NoOp |
| 0x30 | 0x0804E7E4 | não | mtbe: EventOpcode30_NoOp |
| 0x31 | 0x0804E7E8 | não | mtbe: EventOpcode31_YesOrNoBox_Continuation |
| 0x32 | 0x0804E860 | não | mtbe: EventOpcode32_NoOrYesBox_Continuation |
| 0x33 | 0x0804E8C4 | sim | FUN_0804e8c4 (mtbe: EventOpcode33_StartBattle_Continuation) |
| 0x34 | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x35 | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x36 | 0x0804E928 | não | mtbe: EventOpcode36_GetItem |
| 0x37 | 0x0804EA18 | sim | FUN_0804ea18 (mtbe: EventOpcode37_LoseItem) |
| 0x38 | 0x0804EB0C | sim | FUN_0804eb0c (mtbe: EventOpcode38_GetPart) |
| 0x39 | 0x0804EC38 | sim | FUN_0804ec38 (mtbe: EventOpcode39_LosePart) |
| 0x3A | 0x0804ED88 | sim | FUN_0804ed88 (mtbe: EventOpcode3A_GetMoney) |
| 0x3B | 0x0804EE00 | sim | FUN_0804ee00 (mtbe: EventOpcode3B_LoseMoney) |
| 0x3C | 0x0804EE8C | sim | FUN_0804ee8c (mtbe: EventOpcode3C_GetMedal) |
| 0x3D | 0x0804EF5C | sim | FUN_0804ef5c (mtbe: EventOpcode3D_GetTinpet) |
| 0x3E | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x3F | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x40 | 0x0804EFFC | sim | FUN_0804effc (mtbe: EventOpcode40_RunPulsedScreenTransitionLoop) |
| 0x41 | 0x0804F0A4 | não | mtbe: EventOpcode41_FlickerAllActors |
| 0x42 | 0x0804F110 | sim | FUN_0804f110 (mtbe: EventOpcode42_MovePlayerOneStep) |
| 0x43 | 0x0804F3F8 | sim | FUN_0804f3f8 (mtbe: EventOpcode43_ShakeCamera) |
| 0x44 | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x45 | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x46 | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x47 | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x48 | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x49 | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x4A | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x4B | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x4C | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x4D | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x4E | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x4F | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x50 | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x51 | 0x0804F5C8 | não | mtbe: NoOp_0804F5C8 |
| 0x52 | 0x0804F5CC | não | mtbe: NoOp_0804F5CC |
| 0x53 | 0x0804F5D0 | não | mtbe: NoOp_0804F5D0 |
| 0x54 | 0x0804F5D4 | não | mtbe: NoOp_0804F5D4 |
| 0x55 | 0x0804F5D8 | não | mtbe: NoOp_0804F5D8 |
| 0x56 | 0x0804F5DC | não | mtbe: EventOpcode56_LoadEntities |
| 0x57 | 0x0804F674 | não | mtbe: NoOp_0804F674 |
| 0x58 | 0x0804F678 | não | mtbe: NoOp_0804F678 |
| 0x59 | 0x0804F67C | não | mtbe: EventOpcode59_OpenShop |
| 0x5A | 0x0804F6C4 | não | mtbe: NoOp_0804F6C4 |
| 0x5B | 0x0804F6C8 | não | mtbe: NoOp_0804F6C8 |
| 0x5C | 0x0804F6CC | não | mtbe: NoOp_0804F6CC |
| 0x5D | 0x0804F6D0 | não | mtbe: NoOp_0804F6D0 |
| 0x5E | 0x0804F6D4 | não | mtbe: NoOp_0804F6D4 |
| 0x5F | 0x0804F6D8 | não | mtbe: NoOp_0804F6D8 |
| 0x60 | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x61 | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x62 | 0x0804F6DC | não | mtbe: NoOp_0804F6DC |
| 0x63 | 0x0804F6E0 | não | sem nome no mtbe.csv |
| 0x64 | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x65 | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x66 | 0x0804F83C | não | mtbe: EventOpcode66_AnimateTrackedObjectTransition |
| 0x67 | 0x0804F850 | não | sem nome no mtbe.csv |
| 0x68 | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x69 | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x6A | 0x0804F860 | não | mtbe: NoOp_0804F860 |
| 0x6B | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x6C | 0x0804F864 | não | sem nome no mtbe.csv |
| 0x6D | 0x0804F8E8 | sim | FUN_0804f8e8 |
| 0x6E | 0x0804F940 | sim | FUN_0804f940 |
| 0x6F | 0x0804F9C4 | sim | FUN_0804f9c4 |
| 0x70 | 0x0804FA1C | sim | Event_Op70_Cont_Run_Scene_Command_13 (mtbe: EventOpcode70_RunSceneCommand13) |
| 0x71 | 0x0804FA5C | não | sem nome no mtbe.csv |
| 0x72 | 0x0804FA9C | não | sem nome no mtbe.csv |
| 0x73 | 0x0804FADC | não | sem nome no mtbe.csv |
| 0x74 | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x75 | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x76 | 0x0804FB1C | não | mtbe: EventOpcode76_RunFlow4ThenSceneCommand5 |
| 0x77 | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x78 | 0x0804FB8C | sim | FUN_0804fb8c (mtbe: EventOpcode78_RunSceneCommand10) |
| 0x79 | 0x0804FBDC | sim | FUN_0804fbdc (mtbe: EventOpcode79_RunSceneCommand12) |
| 0x7A | 0x0804FC1C | não | sem nome no mtbe.csv |
| 0x7B | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x7C | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x7D | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x7E | 0x0804FC6C | não | mtbe: EventOpcode7E_MoveAllTrackedActorsC |
| 0x7F | 0x0804FC7C | não | sem nome no mtbe.csv |
| 0x80 | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x81 | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x82 | 0x0804FD10 | sim | FUN_0804fd10 (mtbe: EventOpcode82_ExecuteBatchedObjectCommandBlock) |
| 0x83 | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x84 | 0x0804FE6C | sim | FUN_0804fe6c (mtbe: EventOpcode84_SetMapSceneVariantWhenPlayerFacesUp_Continuation) |
| 0x85 | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x86 | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |
| 0x87 | 0x0804FE9C | sim | FUN_0804fe9c |
| 0x88 | 0x0804DC34 | não | stub comum (mesmo alvo de 53 opcodes); mtbe: NoOp |

Alvos unicos: 85; ja sao funcao: 30; faltam virar funcao (HC_lista): 55.
