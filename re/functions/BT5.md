# BT5: entrada na Robattle (quem poe menu 6/7/9) e saida

Ferramenta: Ghidra MCP (sem salvar). Varredura: python -I na ROM original. Relacionado: BT3.md (tabela GS), BT4.md (lacunas).

## (1) Como a batalha entra

Confirmado (decompile + literal pool):
- Evento 0x33 `Event_Op33_StartBattle` 0x0804C53C: le 3 bytes inline e grava E+0xC2A (id da batalha; = 0x0300640A),
  E+0xC2E e E+0xC2F (modo; = 0x0300640F). Se `Stub_ReturnZero_0804B738` der 0, grava E+0xC89 (0x03006469) = 1.
  NAO grava gGameStateMenu nem GS: so arma o pedido.
- `GameState_StartFlowByCommand` 0x08047878, comando 0x20 (batalha de historia) e 0x21 (encontro aleatorio):
  - E+0xC2D (0x0300640D) = 1 (cmd 0x20) ou 0 (cmd 0x21);
  - gMainState 0x0300178C = 0; gGameStateMenu 0x03002B8C = 6; GS 0x0300137C = 0.
  Ou seja, o encontro entra em MainState0 com menu 6 = `Scene_GsDispatch_Menu6_Encounter` 0x08010264 (tabela GS 0x083ACE24).
- Menu 6, passos por GS (nomes do mtbe): 6 PartsBuild_InitScreenLoadGfx, 8 Fight_ResetTemplateAndPalette,
  9 Fight_InitBotPartSelectScreen, 10..19 Fight_BotPartSelect_* (fade, input, dispatch, fade out), 12 Fight_ShowPreBattleTextForCharacter,
  21 Overworld_ReloadMapSceneAndAdvanceGameState (volta ao mapa, nao e a entrada), 22 Gfx_PaletteFadeStep.
- Quem chama cmd 0x20/0x21: nao fechado. Overworld_ShouldTriggerRandomEncounter 0x080438C4 nao foi decompilado nesta rodada.
  Sobre o cmd 0x20 (historia), o chamador (switch 0x08050E6C de Event_RunSceneCommandSequence) nao foi lido.

Varredura de escritores de gGameStateMenu=7 ou 9 (ldr-literal 0x03002B8C + mov #7/#9 em +-10 meias-palavras, 12 hits):
| endereco (store) | funcao | escreve | leitura |
|---|---|---|---|
| 0x08030CCC | Fight_SceneStep4F_ReturnToTurnScene | menu 7 | volta ao turno, interno da batalha |
| 0x08030C7C | Fight_DispatchOnSubstateAA_BR_14_ActionSelectValidatePart | menu 9 | interno (acao escolhida) |
| 0x08031956 | Fight_DispatchOnSubstateAA_BR_49_StartMedaforceAction | menu 9 | interno (roteiro de medaforca) |
| 0x08004856 | BattlePreview_ClearStateStep 0x08004840 | menu 7 + GS 0 + 0x03000430 0 | preview; ponto de tabela 0x083AC520 idx 3 |
| 0x08004E56 | BattlePreview_ResetOamAndState 0x08004E40 | menu 7 (confirmar) | preview; tabela 0x083AC590 |
| 0x08007DAA | PartsGrid_BlendStartStep 0x08007D04 | menu 7 | tela de pecas; nao ligada a menu 6 |
| 0x0806D41C | MainState_KeyComboStateTransition (renomeado) | menu 7 + gMainState 2 + GS 0 | ver abaixo |
| 0x08002EF2, 0x0805C334, 0x0805C9AC, 0x0805F0DA, 0x08044 09A | MenuB/Save/Text/Gfx/Anim | outros | falso positivo provavel |

Candidato forte mas NAO fechado: o preview (0x08004840 e 0x08004E40, tabela 0x083AC520/0x083AC590) grava menu 7 com GS 0.
Falta provar a cadeia menu 6 -> preview -> menu 7 (qual GS do menu 6 chama o preview; nao lido).

## (2) battle_transition_main_state 0x0806D41C (renomeado MainState_KeyComboStateTransition, provavel)

Pools (confirmados): 0x03001800 (fight_information), 0x03002B64 (gKeysHeld), 0x03007008, 0x0300058C (keys_pressed_edge), 0x0300178C, 0x03002B8C, 0x0300137C.
- Se fight_information+4 (u16) == 5 e (gKeysHeld & 8 OU *0x03007008 != 0): gMainState=3, menu=2, GS=10 (candidato a saida; nao confirmado).
- Senao, se (gKeysHeld & 4) e (keys_pressed_edge & 8): gMainState=2, menu=7, GS=0. Tecla combinada: NAO parece caminho normal.
  Chamador NAO achado: zero BL para 0x0806D41C e nenhum ponteiro 0x0806D41D/0x0806D41C na ROM (talvez tabela de halfwords ou dispatch indireto).
  Pode ser atalho de debug; a confirmar antes de usar.

## (3) Saida normal da batalha para o mapa: NAO FECHADA
- Atalho confirmado em BT4: Battle_CheckExitShortcut (gScreenCtx+0x89 == 1 e gLinkStateBlock+0x1C) -> gMainState=0, menu=10, GS=0x10.
  Menu 10 e a tela Link (0x0803D538); GS=0x10 cai no passo 16 (Fight_RunEntryAnimStep). Nao e a saida normal de vitoria.
- Passos de resultado 64..66 e 68 (BT3) continuam sem o escritor de GS final / menu 6 de volta.
- Menu 6 idx 21 Overworld_ReloadMapSceneAndAdvanceGameState (0x08010A7C): chama Map_LoadCurrentMapAndEntities e
  Overworld_SetTransitionStepAndDispatch(0x40,...). Candidato a retorno ao mapa a partir do menu 6, nao confirmado como caminho da luta.

## (4) Discrepancia de endereco (corrigir no mtbe.csv, que nao foi editado)
- mtbe.csv: `0x03005E0D gBattleEncounterFlag`. O codigo grava E+0xC2D = 0x0300640D (cmd 0x20/0x21). 0x03005E0D = base+0x62D,
  nao e o flag. O nome correto e 0x0300640D (hoje `gEncounterRollSuppressed`, com indicacao "[RW]"). Ver BT5_simbolos.csv.

## Lacunas abertas
- Quem chama cmd 0x20 (historia) e cmd 0x21 (Overworld_ShouldTriggerRandomEncounter): decompilar 0x08050E6C e 0x080438C4.
- Quem poe GS=0x23 (35) ao entrar na Robattle (BT4 lacuna 2): continua aberto. Esta rodada achou o caminho de menu 6, nao o GS 35.
- Cadeia menu 6 -> preview 0x083AC520 -> menu 7: provavel, nao confirmada.
- Saida normal (passos 64..68 e menu 6 de volta): aberta.
- Chamador de MainState_KeyComboStateTransition: nao achado.
