# HC4: continuacoes dos opcodes de evento 0x57..0x63 (tabela 0x083D2630)

Chamadas por `Event_DispatchContinuation` (0x0804DC0C) enquanto `E+0xC89` != 0. Estado 1 e gravado pela 1a fase (HE3).
Subpasso = `gCurrentEventOpcodeSubstate` (0x03006473). Args = `gEventScriptArguments` (0x03006474+).

## 0x0804F674 Event_Op57_Cont_NoOp
- Corpo vazio (`bx lr`). Opcode 0x57 (setup NoOp, HE3): nunca grava estado 1, entao esta continuacao nao roda.
- Usa: so o opcode 0x57. Confianca: confirmado (decompile + HE3).

## 0x0804F678 Event_Op58_Cont_NoOp
- Corpo vazio. Opcode 0x58 (sem definicao no C#): nunca grava estado 1, continuacao morta.
- Usa: so o opcode 0x58. Confianca: confirmado (decompile + HE3).

## 0x0804F67C Event_Op59_Cont_OpenShop
- Subpasso 0: `FUN_08050CC0` (Event_ResetSceneCommandStep) e sobe o subpasso para 1.
- Subpasso 1: `FUN_08050CCC(3, args[0])` (roda a sequencia de cena 3 com o id da loja) ate retornar != 0; entao `FUN_08050290` (Event_EndOpcode) zera o pendente.
- Argumento: args[0] = shop id (lido na 1a fase, `Script_ReadBytes(1)`). Opcode unico: 0x59.
- Confianca: confirmado (decompile; offset do args[0] pelo literal 0x0804F6C0, nao conferido na ROM).

## 0x0804F6C4 Event_Op5A_Cont_NoOp
- Corpo vazio. Opcode 0x5A (Set_Secondary_Marker_Facing, HE3) nunca grava estado 1: continuacao morta.
- Usa: so o opcode 0x5A. Confianca: confirmado (decompile + HE3).

## 0x0804F6C8 Event_Op5B_Cont_NoOp
- Corpo vazio. Opcode 0x5B (Jump_If_Fewer_Than_Three_Complete_Part_Sets) e so salto: nao grava estado 1; continuacao morta.
- Usa: so o opcode 0x5B. Confianca: confirmado (decompile + HE3).

## 0x0804F6CC Event_Op5C_Cont_NoOp
- Corpo vazio. Opcode 0x5C (Jump_If_Missing_Complete_Monochrome_Part_Set) e so salto; continuacao morta.
- Usa: so o opcode 0x5C. Confianca: confirmado (decompile + HE3).

## 0x0804F6D0 Event_Op5D_Cont_NoOp
- Corpo vazio. Opcode 0x5D (Update_Overworld_Event_Mode_Flags) grava flags na 1a fase e nao usa estado 1: continuacao morta.
- Usa: so o opcode 0x5D. Confianca: confirmado (decompile + HE3).

## 0x0804F6D4 Event_Op5E_Cont_NoOp
- Corpo vazio. Opcode 0x5E (Set_Overworld_Event_Mode_Bit0) e sincrono; continuacao morta.
- Usa: so o opcode 0x5E. Confianca: confirmado (decompile + HE3).

## 0x0804F6D8 Event_Op5F_Cont_NoOp
- Corpo vazio. Opcode 0x5F (Set_Random_Encounter_Mode_Flags) e sincrono; continuacao morta.
- Usa: so o opcode 0x5F. Confianca: confirmado (decompile + HE3).

## 0x0804F6DC Event_Op62_Cont_NoOp
- Corpo vazio. Opcode 0x62 (Equip_Starter_Parts, HE3) chama `site_StarterBot` e nao grava estado 1: continuacao morta.
- Usa: so o opcode 0x62. Confianca: confirmado (decompile + HE3).

## 0x0804F6E0 Event_Op63_Cont_ShowEffect (corrigido em SCN63: espera o despachante de cena 0x0805424C; ver SCN63.md)
- Subpasso 0: `Map_ModeDispatch` (0x08050BC8) ate retornar != 0. Se `gOverworldUiState & 0x3F` (0x03005350+0x1D4) nao for 0xD/0x16, termina (`FUN_08050290`, Event_EndOpcode). Se for, avanca.
- Subpasso 1: `FUN_08051940(0x2F)`, grava `16 / resultado` em 0x03006D20 (significado chute); `FUN_08050124(0x2F, (args[0]<<4)|args[1], 0)` cria o objeto de render 0x2F. Avanca.
- Subpasso 2: grava 2 (se modo 0xD) ou 3 em +0xC67 e espera `FUN_0804F188(+0xC67)` != 0; depois marca +0xC5B=1, converte x/y do objeto (+0xC52/+0xC54) para tile (+0xC56/+0xC57), chama `Map_ClampCameraToLoadedMap`, `Map_ApplySceneVariantScrollPreset`, `Overworld_SetPlayerFacingAndRefreshFollower`, `Map_UpdatePlayerMarkerScreenPosition`, `Overworld_ResetUiRenderState` (0x08050C94) e termina.
- Argumentos (lidos na 1a fase, `Script_ReadBytes(3)`): args[0] = id do efeito (0x03006474), args[1] = tx (0x03006475). ty fica em args[2] e nao aparece aqui. Memoria: 0x03005350, 0x03002C30, 0x03006D20 (chute).
- Confianca: confirmado (decompile + literais lidos na ROM, offsets 0xC94/0xC95 = args); efeito dos subpassos 1-2 provavel, FUN_0804F188 e FUN_08051940 sem nome.
