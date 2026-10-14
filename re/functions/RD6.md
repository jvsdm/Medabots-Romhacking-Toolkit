# RD6 - revisão de rótulos de dados/tabelas

Lista: `RD6_lista.txt` (63 endereços, todos kind data/table/ptr/label; nenhum era rótulo de código).
Saída: `RD6_simbolos.csv`.

## Trocas de nome
- `0x0847E388`: `get_item_text` -> `gEventItemTextTable` (antes: get_item_text). É tabela de ponteiros, não função. Xrefs: EventOpcode38/39/3D e Event_Op36. Renomeado no Ghidra via rename_data.

## Trocas de kind (label -> dado)
- `0x084BFC6C` gFont, `0x084C59AC` gItalicFont, `0x084C4BDC` gGfx_PartDetail_Tiles_Bg1: label -> data.
- `0x084C29D8` gGfx_BotIconTable_Base e `0x0847DF24` gTextPointerTable: label -> table (confirmado).
- `0x083F7E80` gMapSpritePtrTable: label -> table.

## Achados
- gMapDimensions: 2 B por mapa; mapa 10 = 24x22 conferido nos bytes.
- gBootSubStateTable: 11 ponteiros + NoOp = 12 entradas.
- gTextPointerTable: 16 ponteiros = os 16 gTextDialogue_* (xref Screen_UpdateByStep).
- gMapAnimDescTable e gMapBlockB: tabelas de 192 x 4 B.
- Cabeçalhos Malias (`4C 65` + u16 em +2) batem com as notas GF/GF2/GF3 para todos os gGfx_* da lista.

## Em aberto (conflitos nos docs)
- gTitleMenuSubStateTable: TL.md diz 12 entradas, mas 0x08483634 começa dentro desse range. Tamanho real não fechado.
- gMainState2_Menu5SubTable: FM4 diz 6 entradas, FM5 diz 8.
- KeyItemBag_SubStateTable e KeyItemBag_MenuTable4: nome pode ser enganoso; a tabela 0x08483634 também é usada por Shop_InitItemListScreen. Nomes mantidos.
- sRandomAdvanceTable: xrefs (Obj/Fight/fx) sugerem uso genérico de fase, não só aleatório. Nome mantido, provável.
- gPoseIdNestedTablePtrs, gEventItemTextTable, gMapSpritePtrTable: número de entradas não fechado.
- gItalicFont: CSV indica ROM EU danificada nessa área.
