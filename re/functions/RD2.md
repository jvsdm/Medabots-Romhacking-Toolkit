# RD2: revisao de rotulos de dados/tabelas (RD2_lista.txt)

Revisados: 16 entradas kind=data/table. Nomes mantidos em todas; nenhum rename no Ghidra.

## Trocas de kind
- data -> table: SceneCommandStepTable (0x08050CF4), SceneCommandCodeTable (0x08050E6C), Scene_DispatchStateHandler_CodeTable (0x080542B0). Sao tabelas de ponteiros.

## Achados (tamanho confirmado)
- SceneCommandStepTable: 5 ptrs (20 B). Antes: "5 ptrs" ok.
- SceneCommandCodeTable: 33 ptrs (132 B), termina em 0x08050EF0.
- Overworld_MapSubstateTable: 5 ptrs (20 B), termina no 1o handler 0x08050F94.
- Scene_DispatchStateHandler_CodeTable: 140 ptrs (560 B), termina em 0x080544E0.
- FieldMenu_RowActionTable: 6 ptrs (24 B), termina em 0x0805754C. QA5 dizia "4+": agora 6.
- MapSpawn_ChapterFilterLiteral: pool de 2 words (0x03005FDF, 0x00000FFF), nao tabela.
- lit_EventFlagArrayBase_* (10 entradas): cada uma 1 word = 0x030057D8 (gEventFlagArrayBase). Nomes batem com o consumidor.

## Em aberto
- lit_EventDbBase0..3 (kind=label em RD2_lista) nao foram revisados: fora do filtro data/table.
- Entradas 0x08050D08..0x08050F40 e demais alvos sao codigo; nao reclassificados aqui.
