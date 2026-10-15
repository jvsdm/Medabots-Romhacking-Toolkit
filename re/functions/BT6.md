# BT6: entrada e saida da Robattle (fechamento parcial de BT5)

Ferramenta: Ghidra MCP (sem salvar). Varredura: python -I na ROM original (scratchpad bt6, apagado). Relacionado: BT5.md, BT4.md, BT3.md.

## (1) Entrada: menu 6 -> ... -> menu 7 / GS 0x23

Tabela de menu 6 (0x083ACE24), lida com python -I (indice = GS, ponteiro thumb):
- 0 0x080102C0, 1 0x0801030C, 2 0x08010328, 3 0x08010358, 4 0x08010374, 5 0x080103A4, 6 0x080103F4 (PartsBuild_InitScreenLoadGfx, confirmado),
  7 0x08010478, 8 0x0801053C, 9 0x0801058C, 10 0x08010794, 11 0x080107F0, 12 0x08010814, 13 0x08010860, 14 0x080108D0,
  15 0x080108FC, 16 0x08010930, 17 0x08010968, 18 0x080109A0, 19 0x080109CC, 20 0x08010A20, 21 0x08010A7C (Overworld_Reload...),
  22 0x08010AC8, 23-31 e 36-39 = 0x08000840 (stub comum, nao usado), 32 = 18, 33 = 19, 34 = 20 (mesmos handlers),
  35 = 0x08010B24 (Fight_ResetPartStateBlock, confirmado: zera 0x14 bytes do bloco e grava 2, 3, 0).
- O GS 0x23 cai em Fight_ResetPartStateBlock, que nao grava gGameStateMenu nem GS. Ele e um passo de reset de estado de pecas, nao o setter.
- Quem grava GS = 0x23: NAO FECHADO. Varredura por escritores de GS (xrefs de 0x0300137C) so retornou AgbMain, SoftResetSequence,
  Screen_ResetGfxAndState 0x08044204 e MainState_Dispatch2 0x0805BC18. O ramo MainState_Dispatch2 grava 0 / menu 10 / GS 0x10 (atalho
  de BT4), nao 0x23. Os demais hits de 0x0300137C sao so ponteiros em literal pool (DATA), nao funcoes. Proximo passo: achar as
  funcoes que carregam o literal 0x0300137C junto com a constante 0x23 (script no ROM, ou ponto de escrita via mGBA watch em GS).

## (2) Saida e o ramo 0x0806D41C

- `MainState_KeyComboStateTransition` 0x0806D41C (nome ja no Ghidra): dois ramos, descritos em BT5. O ramo 1 (gMainState=2, menu 7, GS 0)
  tem atalho de tecla combinada. O ramo 2 (gMainState=3, menu 2, GS 10) e o candidato a saida.
- QUEM CHAMA (fechado): a funcao `Battle_TickEffectsAndTransition` tem entrada 0x0806D588 (push {lr} em 0x0806D588). O corpo
  chama battle_effect_affine_lines, um ponteiro de tabela (CallViaR0 com DAT_0806D5AC), BattleFx_TickTimedStages5 e depois
  MainState_KeyComboStateTransition. Ela NAO tem BL direto nem ponteiro para 0x0806D588 na ROM (varredura de BL e de ponteiro).
  Ela aparece como ponteiro-de-funcao (0x0806D589) na tabela da ROM em 0x084856F0 (entrada 0x0806D589 entre 0x0806EA99 e 0x0806E131),
  sem tabela base confirmada. Provavelmente e o tick por frame da batalha, chamado por dispatch indireto. Confirmar a base da tabela.
- Nota: Ghidra mostra o decompile em 0x0806D5A0 como Battle_TickEffectsAndTransition; a entrada real e 0x0806D588 (get_function_by_address
  em 0x0806D5A0 falha).
- Ramo 2 (menu 2 / GS 10): menu 2 nao foi decompilado; o que ele leva de volta ao mapa NAO FECHADO.
- Saida normal para o mapa (vitoria): ainda NAO FECHADA. Menu 6 idx 21 (0x08010A7C Overworld_ReloadMapSceneAndAdvanceGameState)
  chama Map_LoadCurrentMapAndEntities e Overworld_SetTransitionStepAndDispatch(0x40). Continua candidato a retorno ao mapa.

## Lacunas abertas
- Escritor de GS = 0x23 (qual funcao, qual ramo).
- Base da tabela que contem 0x0806D589 (0x084856F0) e o indice do tick de batalha.
- Menu 2 GS 10 e sua cadeia ate o mapa.
- Quem chama cmd 0x20/0x21 (BT5): 0x08050E6C e 0x080438C4, nao lidos nesta rodada.
