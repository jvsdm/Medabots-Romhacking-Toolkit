# CH3 — revisão dos chutes (GH-CH3)

Status: EM ANDAMENTO. R3 parcial (16 de 38 revisadas). R4 não começou.

## Feito (R3)
- Revisados (CSV + R3.md atualizados, com "revisto CH3: ..."):
  - confirmado: 0x08058208 (Map_HandleMarkerCommandFromState)
  - provável: 0x080507ec, 0x0805082c, 0x08051940, 0x0805196c, 0x0805330c, 0x080533e4, 0x080581c8,
    0x080589a4, 0x08058b90, 0x08058c0c, 0x080594c8, 0x08059578, 0x08059734, 0x080598c0, 0x0805b0b0
- Renomeados no Ghidra:
  - FUN_08051940 -> Map_GetScaledStepDelta (CSV e R3.md atualizados)
  - 0x080589a4 Screen_StepCyclicIndexDown -> Screen_StepCyclicIndex (faz as duas direções; CSV e R3.md atualizados)
- Nomes já no Ghidra e trocados no CSV: 0x0805082c -> Map_TickObjectFlicker; 0x0805196c -> Overworld_ClearFollowerQueuedMove.
- Chamadores (get_function_xrefs) conferidos para 08051940 (R3.md atualizado). Não conferidos para os demais.

## Achados
- Ghidra e mtbe/CSV divergiam em 2 nomes (0x0805082c e 0x0805196c tinham FUN_ no CSV e nome no Ghidra).
- Nomes que o código não sustenta sozinho: "Screen_*" e "Map_*" de tela/menu ainda dependem de confirmação em uso.

## Pendente: R3 (22 entradas)
0x0805830c Screen_UpdateByStep (função grande, 0x0805830c-0x080589a3; decompile pendente),
0x08059960 Screen_BuildMainPanels (grande, 0x08059960-0x08059e2d),
0x0805b5fc Shop_IsEntryMarked, 0x0805ba34 Screen_LoadLZ77TileBlock, 0x0805bb0c Screen_ReadTwoBitField,
0x0805bc5c Screen_DrawCellAdvance, 0x0805bf14 Palette_FadeTo16, 0x0805cb90 Screen_SetSlotHighlight,
0x0805ddec Site_RunSaveLoadOption, 0x0805deb0 Save_InvokeFlashOp, 0x0805e2d8 Obj_SetFieldFromTable4,
0x0805e300 Table_SumIdsUntilEnd, 0x08062a10 BattlePanel_InitOrRedraw, 0x08062b48 BattlePanel_DrawSlotText,
0x08064bec/0x08064c0c/0x08064c2c/0x08064c4c Battle_SetDefaults[A-D] (provavelmente iguais; conferir se são 4 casos),
0x080656dc Obj_InitTimerRecord, 0x08065710 Anim_StepRecordCounter,
0x08066d8c FUN_08066d8c (sem nome; mecânica lida, uso chute), 0x080672f4 Str_LengthZeroIfOne.

## Pendente: R4 (~30 entradas, R4_simbolos.csv)
Não começou. Usar `rtk proxy grep -n -i chute R4_simbolos.csv`.

## Processo
- Helper: scratchpad ch3/apply.py (lê JSON de decisões e atualiza CSV + R3.md/R4.md; uso: python apply.py lote.json).
- Confiança no formato: confirmado | provável | chute, com "revisto CH3: ..." no comentário do CSV.
