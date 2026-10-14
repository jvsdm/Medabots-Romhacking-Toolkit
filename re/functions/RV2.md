# RV2 - revisao de nomes (60 enderecos de RV2_lista.txt)

Resultado: 54 funcoes revisadas (45 confirmado + 9 provavel no CSV), 11 renomeadas no Ghidra, 6 enderecos sao labels (nao renomeados). CSV: `RV2_simbolos.csv`.

## Trocas de nome (renomeadas no Ghidra)
- 0x0800B938 `DebugMenu_HandleInput` (antes: controls_in_debug_menu).
- 0x0800AD00 `Sprite_SetOrClearAttr0ShapeBit` (antes: Sprite_ToggleAttr0ShapeBit). Liga/desliga 0x4000 conforme o bit 8 do flag; nao inverte.
- 0x0800EF58 `Sprite_SetOrClearAttr0ShapeBitB` (antes: Sprite_ToggleAttr0ShapeBitB). Mesma logica do acima.
- 0x0800C7E4 `DebugMenu_Option4_Dispatch` (antes: debug_option4_item_entry).
- 0x0800CE70 `DebugMenu_Option6_Dispatch` (antes: debug_option6_mdlpic_entry). "mdlpic" nao aparece no corpo.
- 0x0800EE34 `PartsGrid_DrawFourPartRows` (antes: PartsGrid_DrawFourPartIcons). Preenche 4 linhas de tilemap, nao icones.
- 0x0800F6CC `PartDetail_GetCurrentSelectionNormalizedIndex` (antes: PartDetail_GetCurrentSelectionLegIconIndex). Devolve byte +0xEF mod 100; nada de perna no corpo.
- 0x0800FD74 `PartDetail_SetOrClearCursorSpriteShape` (antes: PartDetail_ToggleCursorSpriteShape). Mesma logica de set/clear.
- 0x08010170 `PartsBuild_SetOrClearSlotShapeAndAdvance` (antes: PartsBuild_MarkSlotFlagStep). Provavel.
- 0x080121A8 `Fight_StoreBotLabelText` (antes: Fight_FormatBotLabelText). Copia 8 bytes para a tabela; nao formata. Provavel.
- 0x0801528C `Fight_RunPartActionPhasesForActiveSlots` (antes: Fight_RefreshBotPartSlotDisplays). Nao atualiza display. Provavel.

## Achados
- 0x08008D90, 0x08008E08, 0x08008E80, 0x08008EF8, 0x08008F04, 0x08008F10 NAO sao inicio de funcao no Ghidra (get_function_by_address falha). Sao blocos de jump-table dentro de `PartsGrid_SubStateSwitchStep` (0x08008CA0-0x080090C7). Ficaram como `label` com `chute`; os nomes atuais nao foram verificados.
- `decompile_function_by_address` em endereco que nao e inicio de funcao devolve a funcao que o contem, sem aviso. Antes de decompilar, usar `get_function_by_address` para confirmar.
- Mapa de botoes confirmado nos corpos: 0x01 A, 0x02 B, 0x04 SELECT, 0x08 START, 0x10 DIREITA, 0x20 ESQUERDA, 0x40 CIMA, 0x80 BAIXO. Bate com o README.
