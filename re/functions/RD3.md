# RD3: revisao de DADOS/TABELAS (RD3_lista.txt)

Metodo: docs (JT2-JT5, IC, GF4, IN2/IN3, CS2, SN, N2, FS10/SN3) + bytes da ROM (python -I). Sem decompile.

## Trocas de nome (rename_data, resultado "attempted", nao confirmado)
- 0x0805BC54: Unnamed_ShopTableBaseRef -> gFrameTableBasePtr (literal = 0x08483594). Dono nao localizado.
- 0x080631C0: gBattlePanel_Literal_Char_Src -> gBattlePanel_CharSrcPtr (literal = 0x08785EC0).
- 0x0805D36C: ItemListSwitch_Table -> KeyItemBag_Case_Id0 (era codigo, alvo do id 0; label).

## Mudancas de kind
- gMotionData (0x080814C4): label -> data.
- gObjTextGlyphFrames (0x08075FF8): ptr -> table (78 x 4).

## Achados
- KeyItemBag_DispatchTable: 41 entradas (ids 0..0x28), nao 40. Bytes confirmam (0x0805D368 = ultimo alvo).
- Descritores gGfx_PartLarge Sz0..Sz3: 122 entradas, stride 0x10/0x10/0x10/0x20, pelo espacamento entre tabelas.
- Literal 0x08066860 (Menu14_CallSubstateHandler_Pool): e pool de literais de funcao em 0x08066870, nao tabela.

## Em aberto
- Data_080A89F2: conteudo nao identificado, nome automatico mantido.
- N de gLZ77RecordTable e dos StateTable_CopySrc: nao fechado.
- gMotionData: fim nao fechado (ate 0x080ABE00).
