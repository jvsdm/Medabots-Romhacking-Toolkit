# PG: tela de grade de pecas (PartsGrid), tabela de estados e saltos de 0x08008CA0

Escopo: FUN_08007584 (despachante), tabela 0x083AC86C (112 ptrs), jump tables de 0x08008CA0, FUN_08007998/08007DDC, e a tabela 0x083AC2C8 vs gMainStateTable. Ghidra: 3 renomes aplicados (nao salvo). mtbe.csv e ROM nao editados.

## 1. Despachante 0x08007584 (PartsGrid_DispatchStateTable)
- Codigo (Ghidra, confirmado): `if (Battle_CheckExitShortcut()==0) CallViaR0(*(u32*)(*0x080075A8*4 + *0x080075A4))`.
- Literais: 0x080075A4 = 0x083AC86C (tabela); 0x080075A8 = 0x0300137C (gMainSubStateCounter no mtbe; o indice e o valor desse endereco).
- Tabela: `gPartsGridStateTable` 0x083AC86C, 112 entradas de 4 bytes (ponteiros Thumb, todos validos). Stride 0xC em 0x083AC880 (mtbe `table_partsgrid_states`) esta errado: 0x083AC880 cai no meio da tabela (entrada 5).
- Chamada por: xref vazio no Ghidra (chamada por ponteiro de outra tabela; nao rastreado).
- Confianca: confirmado (formato, literais) / provavel (nome). Ghidra: renomeado.

## 2. Jump tables de 0x08008CA0 (PartsGrid_SubStateSwitchStep)
- Entrada: ctx = 0x03000440 (literal 0x08008CBC). Modo = byte ctx+0x42; sub-estado = byte ctx+0x44.
- Tabela A (modo 0, `mov pc` em 0x08008CDA, literal 0x08008CDC -> 0x08008CE0), indice 0..4 (cmp #4):
  - 0 -> 0x08008CF4 PartsGrid_SubStateA0_DispatchCtx18: DispatchStep(*(ctx+0x18))
  - 1 -> 0x08008D00 PartsGrid_SubStateA1_DispatchCtx1C: DispatchStep(*(ctx+0x1C))
  - 2 -> 0x08008D0C PartsGrid_SubStateA2_CellLookup: lookup de celula (+0x3D*0x40 + base 0x03000BE0 + 4) -> ponteiro -> DispatchStep
  - 3 -> 0x08008D30 PartsGrid_SubStateA3_CellLookupB: lookup de celula (+6) -> ponteiro -> DispatchStep
  - 4 -> 0x08008D54 PartsGrid_SubStateA4_DispatchCtx20: DispatchStep(*(ctx+0x20))
- Tabela B (modo 1, `mov pc` em 0x08008D72, literal 0x08008D74 -> 0x08008D78), indice 0..5 (cmp #5):
  - 0 -> 0x08008D90 PartsGrid_SubStateB0_CellOrTable: se byte celula+5 <= 9: DispatchStep(*(ctx+0x10)); senao copia 3+6 bytes e DispatchStep da tabela por ctx+0xF0
  - 1 -> 0x08008E08 PartsGrid_SubStateB1_CellOrTable: igual B0, limiar 0x1D, ramo DispatchStep(*(ctx+0x10)); senao copia e tabela por ctx+0xF0
  - 2 -> 0x08008E80 PartsGrid_SubStateB2_CellOrTable: igual B0, limiar 0x3B, ramo DispatchStep(*(ctx+0x10)); senao copia e tabela por ctx+0xF0
  - 3 -> 0x08008EF8 PartsGrid_SubStateB3_DispatchCtx24: DispatchStep(*(ctx+0x24))
  - 4 -> 0x08008F04 PartsGrid_SubStateB4_DispatchCtx28: DispatchStep(*(ctx+0x28))
  - 5 -> 0x08008F10 PartsGrid_SubStateB5_SwitchOn46: le ctx+0x46: 0 -> 0x08008F32, 1 -> 0x08008FA8, 2 -> 0x0800901C, outro -> nada
- Modo 2 (sem jump table): sub-estado 0 -> DispatchStep(*(lit 0x080090A4 +0x34)); 1 -> DispatchStep(*(lit 0x080090C8 +0x38)); outro -> nada.
- Fim comum (0x080090B4): se o retorno do DispatchStep for != 0, chama 0x0803FB0C (nao lido) e grava 0x49 em 0x080090CC (proximo estado).
- Confianca: confirmado (aritmetica e alvos lidos no disasm e na ROM); significado dos sub-estados: chute. Alvos sem funcao no Ghidra: ver PG_criar.txt.

## 3. Renomes (0x08007998 e 0x08007DDC)
- 0x08007998 -> `PartsGrid_SlideOutStep`: mesmo corpo de PartsGrid_SlideInStep (0x08007834) com outro contador; sentido 'saida' provavel.
- 0x08007DDC -> `PartsGrid_SetSelectorPosByIndex`: escreve 8/0x18/0x28/0x38 em ponteiro de literal conforme ctx+0x42 (0..3); eixo chute.

## 4. 0x083AC2C8 x gMainStateTable (0x083ABF98)
- NAO sao a mesma tabela. 0x083ABF98: 6 entradas (0x08002535 MainState0, 0x0804763D, 0x0805BBE1, 0x08068441, 0x08000259 x2); indice = gMainState 0x0300178C. 0x083AC2C8: 182 entradas (0x08002555, 0x080027BD, 0x08003375, ...), e o menu_list do MainState0. Estao separadas por 0x330 bytes.
- MainState0 (0x08002534) indexa com o literal 0x08002550 = 0x03002B8C (gGameStateMenu). A FS2 dizia 'gMainState'; corrigido aqui. Cena 63 = menu de debug (0x0800B891).
- Rotulo `gMainStateTable` em 0x083AC2C8 no mtbe.csv esta errado; proposta: `gMenuList0Table` (ou manter menu_list). Nao editei mtbe.csv.
- Confianca: confirmado (literais e entradas lidas na ROM).

## 5. Tabela 0x083AC86C: indice -> funcao -> nome atual
Nome atual = nome no mtbe.csv (FUN_ = sem nome). Entradas 0x08000840 = NoOpActionHandler (nao faz nada).

| idx | endereco | nome |
|---|---|---|
| 0 | 0x080075AC | PartsGrid_SceneInit |
| 1 | 0x08007834 | PartsGrid_SlideInStep |
| 2 | 0x08007868 | PartsGrid_InputStep |
| 3 | 0x08007964 | PartsGrid_BlendInStep |
| 4 | 0x08007998 | PartsGrid_SlideOutStep |
| 5 | 0x080079CC | PartsGrid_ResetOamAndDispatch |
| 6 | 0x08000840 | NoOpActionHandler |
| 7 | 0x08007998 | PartsGrid_SlideOutStep |
| 8 | 0x08007A04 | PartsGrid_InitScreen_CellGraphics |
| 9 | 0x08007BD8 | PartsGrid_BlendTickStep |
| 10 | 0x08007BFC | PartsGrid_LoadSlotStep |
| 11 | 0x08007C20 | PartsGrid_DispatchSubStep |
| 12 | 0x08007C4C | PartsGrid_CursorInputStep |
| 13 | 0x08007D04 | PartsGrid_BlendStartStep |
| 14 | 0x08007BD8 | PartsGrid_BlendTickStep |
| 15 | 0x08007D28 | Tile_ClearSlotsAndDispatchState_JumpTable |
| 16 | 0x08000840 | NoOpActionHandler |
| 17 | 0x08000840 | NoOpActionHandler |
| 18 | 0x08000840 | NoOpActionHandler |
| 19 | 0x08000840 | NoOpActionHandler |
| 20 | 0x08007D04 | PartsGrid_BlendStartStep |
| 21 | 0x08007BD8 | PartsGrid_BlendTickStep |
| 22 | 0x08007DDC | PartsGrid_SetSelectorPosByIndex |
| 23 | 0x08000840 | NoOpActionHandler |
| 24 | 0x08007E38 | PartsGrid_InitScreen |
| 25 | 0x0800801C | PartsGrid_SlideDetailStep |
| 26 | 0x08008050 | PartsGrid_SlideLoadStep |
| 27 | 0x08008084 | PartsGrid_DetailDispatchStep |
| 28 | 0x080080C8 | PartsGrid_DetailInputStep |
| 29 | 0x080081BC | PartsGrid_DetailDispatchStepRev |
| 30 | 0x08008200 | PartsGrid_BlendSlideStep |
| 31 | 0x0800801C | PartsGrid_SlideDetailStep |
| 32 | 0x08007D28 | Tile_ClearSlotsAndDispatchState_JumpTable |
| 33 | 0x08000840 | NoOpActionHandler |
| 34 | 0x08000840 | NoOpActionHandler |
| 35 | 0x08000840 | NoOpActionHandler |
| 36 | 0x08008200 | PartsGrid_BlendSlideStep |
| 37 | 0x0800801C | PartsGrid_SlideDetailStep |
| 38 | 0x08007DDC | PartsGrid_SetSelectorPosByIndex |
| 39 | 0x08000840 | NoOpActionHandler |
| 40 | 0x08008234 | PartsGrid_InitScreen_StatusPreview |
| 41 | 0x08008430 | PartsGrid_BlendTickStepB |
| 42 | 0x08008454 | PartsGrid_LoadSlotStepB |
| 43 | 0x08008478 | PartsGrid_DispatchStepB |
| 44 | 0x080084A4 | PartsGrid_CellInputStep |
| 45 | 0x08008590 | PartsGrid_BlendStartStepB |
| 46 | 0x08008430 | PartsGrid_BlendTickStepB |
| 47 | 0x08007D28 | Tile_ClearSlotsAndDispatchState_JumpTable |
| 48 | 0x080085B4 | PartsGrid_SetupDetailSprites |
| 49 | 0x080085E8 | PartsGrid_DispatchByCellStep |
| 50 | 0x08008650 | PartsGrid_CellMainInputStep |
| 51 | 0x08000840 | NoOpActionHandler |
| 52 | 0x08008590 | PartsGrid_BlendStartStepB |
| 53 | 0x08008430 | PartsGrid_BlendTickStepB |
| 54 | 0x08007DDC | PartsGrid_SetSelectorPosByIndex |
| 55 | 0x08000840 | NoOpActionHandler |
| 56 | 0x080087CC | PartsGrid_EnterScreenInit |
| 57 | 0x08008980 | PartsGrid_IconStripStep |
| 58 | 0x080089A8 | PartsGrid_IconStripInputStep |
| 59 | 0x08008A34 | PartsGrid_IconStripBlendStep |
| 60 | 0x08008980 | PartsGrid_IconStripStep |
| 61 | 0x08007D28 | Tile_ClearSlotsAndDispatchState_JumpTable |
| 62 | 0x08000840 | NoOpActionHandler |
| 63 | 0x08000840 | NoOpActionHandler |
| 64 | 0x08008A5C | PartsGrid_IconAnimStep |
| 65 | 0x08000840 | NoOpActionHandler |
| 66 | 0x08000840 | NoOpActionHandler |
| 67 | 0x08000840 | NoOpActionHandler |
| 68 | 0x08008A34 | PartsGrid_IconStripBlendStep |
| 69 | 0x08008980 | PartsGrid_IconStripStep |
| 70 | 0x08007DDC | PartsGrid_SetSelectorPosByIndex |
| 71 | 0x08000840 | NoOpActionHandler |
| 72 | 0x08008B80 | PartsGrid_ClearAndInitStep |
| 73 | 0x08008BB8 | PartsGrid_DispatchStepC |
| 74 | 0x08008BE0 | PartsGrid_SlotScriptInputStep |
| 75 | 0x08008CA0 | PartsGrid_SubStateSwitchStep |
| 76 | 0x08000840 | NoOpActionHandler |
| 77 | 0x08000840 | NoOpActionHandler |
| 78 | 0x080090D0 | PartsGrid_SpritePairStep |
| 79 | 0x08000840 | NoOpActionHandler |
| 80 | 0x08000840 | NoOpActionHandler |
| 81 | 0x08000840 | NoOpActionHandler |
| 82 | 0x08000840 | NoOpActionHandler |
| 83 | 0x08000840 | NoOpActionHandler |
| 84 | 0x08000840 | NoOpActionHandler |
| 85 | 0x08000840 | NoOpActionHandler |
| 86 | 0x08000840 | NoOpActionHandler |
| 87 | 0x08000840 | NoOpActionHandler |
| 88 | 0x08000840 | NoOpActionHandler |
| 89 | 0x08000840 | NoOpActionHandler |
| 90 | 0x08000840 | NoOpActionHandler |
| 91 | 0x08000840 | NoOpActionHandler |
| 92 | 0x08000840 | NoOpActionHandler |
| 93 | 0x08000840 | NoOpActionHandler |
| 94 | 0x08000840 | NoOpActionHandler |
| 95 | 0x08000840 | NoOpActionHandler |
| 96 | 0x08009174 | PartsGrid_ReloadRowTilesAndSetup |
| 97 | 0x080091E0 | PartsGrid_OptionRowStep |
| 98 | 0x08009248 | PartsGrid_OptionHandlerStep |
| 99 | 0x08009274 | PartsGrid_TickFadeOutStep |
| 100 | 0x080092D4 | PartsGrid_TickRouteSelection |
| 101 | 0x08009374 | PartsGrid_TickDispatchSubstate |
| 102 | 0x08000840 | NoOpActionHandler |
| 103 | 0x08000840 | NoOpActionHandler |
| 104 | 0x08007964 | PartsGrid_BlendInStep |
| 105 | 0x08007998 | PartsGrid_SlideOutStep |
| 106 | 0x080093AC | Shop_SnapshotQuantitiesTable |
| 107 | 0x080093E0 | PartsGrid_ResetAndSetMode1 |
| 108 | 0x08000840 | NoOpActionHandler |
| 109 | 0x08000840 | NoOpActionHandler |
| 110 | 0x08000840 | NoOpActionHandler |
| 111 | 0x08000840 | NoOpActionHandler |

Achados na tabela: entradas 15/32/47/61 usam `Tile_ClearSlotsAndDispatchState_JumpTable` (0x08007D28) e 106 usa `Shop_SnapshotQuantitiesTable` (0x080093AC): nomes do mtbe que parecem de outra tela (colisao a checar). Ver FS2/FS3 para as funcoes PartsGrid_*.
