# RD4: revisao de rotulos de dados/tabelas (70 enderecos)

Metodo: mtbe.csv + docs da pasta (busca por endereco), depois bytes da ROM (python -I) para contar ponteiros e
tamanho. Ghidra so para rename_data. Detalhes por endereco em RD4_simbolos.csv.

## Trocas (rename_data feito no Ghidra)
- 0x083AC560 BattleStepTable_Base_Unresolved -> gTeamPickBattleStepTable
- 0x083AC568 BattleStepTable_Unknown -> gTeamPickBattleStepTable_Idx2 (interior da mesma tabela)
- 0x083AC880 gPartsGridStateTable_Entry5 -> gPartsGridStateTable_Idx5 (interior)
- 0x083ACA4C debug_menu_more_idk -> gObjStepTable_ACA4C
- 0x083ACAC0 StateHandlerTable_BAD8 -> gDebugStateHandlerTable
- 0x083ACAE0 table_partsbuild_states -> gDebugStateHandlerTable_Idx8
- 0x083ACBCC gTextProcessStrings3 -> gTextDebugStrings_ACBCC
- 0x083ACC40 debug_tinpet_name -> debug_tinpet_name_ptr (e um ponteiro, nao o texto)
- 0x083B5578 gTextDialogue_083B5578 -> gTextDialogueIntro

Kinds: labels de tabela passam a table/data conforme os bytes; gTextProcessStrings vira data (texto codificado).

## Achados
- Tabela 0x083AC560 (16 x 4) e a MESMA que 0x083AC568 (idx 2); 0x083AD10C, 0x083AD190 e 0x083AD114/118 caem
  dentro da cadeia 0x083AD080/0x083AD0F0 (BT3 i=35, i=68).
- 0x083ACAE0..0x083ACB38 sao idx de gDebugStateHandlerTable (stride 4), nao tabela de montagem (PS.md diz stride 0xC).
- debug_parts_spawn_names, debug_item_grant_names: 3 x 4 ponteiros cada; debug_menu_text: 11 x 4.
- gTextMedaforceDesc (0x083B7580): DT diz 50 ponteiros, mtbe 53, bytes mostram so 2 em sequencia. Pendente.
- gFontWidths/gItalicFontWidths: 512 B cada (confirmado EXPANSAO).

## Em aberto
- gGfx_Portrait_PonteirosImagens: mtbe 1584 entradas vs GF3 1 por personagem.
- gObjStepTable_ACA4C: 0x083ACA80 (tabela de opcoes do debug) cai dentro; rever.
- cursor_cell_xy_table: bytes batem so para 6 celulas (HD2 diz 9).
- Tamanhos nao medidos: gSpriteSetTable0, gBattleSprites, gRandomLUT, MedaforceMedalTable, SlotScriptPtrTable,
  gPartAttributeNamePointerTable, gGfx_Portrait_PonteirosLayout, gActionScriptOpcodeHandlerTable (>=128).
