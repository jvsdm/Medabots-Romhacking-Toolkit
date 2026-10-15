# RF8 - trocas e achados

Lista: RF8_lista.txt (34 itens, todos marcados `dado`). Nenhum renome feito no Ghidra. Nenhum item era FUNCAO.

## Trocas de tipo (dado -> label)
Todos os 34 são endereços de código (bytes Thumb válidos, sem prólogo push). Viraram `label`:
- 24 x SceneState_* (0x08055508..0x08056724): alvos de switch dentro de SceneState. Confiança `provavel`, nomes mantidos (comportamento ainda é chute do SC4).
- 3 x FieldMenu_RowAction (0x0805754C, 0x08057564, 0x08057570): alvos da tabela FieldMenu_RowActionTable 0x08057534, 6 x 4 bytes (ROM lida). `confirmado`.
- 6 x Screen_* (dispatchers `mov pc,r0` = 0x4687): `confirmado` pela tabela na ROM.

## Tabelas (N x stride 4), lidas da ROM
- 0x08057534 FieldMenu_RowActionTable: 6 x 4 (já estava no mtbe/FM2; QA5 dizia "4+").
- 0x08058334 JumpStep: 8 x 4.
- 0x08058384 JumpState6: 6 x 4.
- 0x0805857C JumpState5: 6 x 4.
- 0x08058720 JumpState4: 5 x 4.
- 0x0805886C JumpState7: 7 x 4.
- 0x08058C54 RedrawDispatch: 8 x 4.

## Achados (nome parece errado: não é início de função)
- 0x08058080 `Map_CreateChoiceSprites`: não começa com push; Ghidra não acha função nesse endereço. Está dentro de outra função. Nome mantido como `provavel`; falta achar o dono.
- 0x0805D2A0 `KeyItemBag_UseItem`: idem, começa com movs r0,#4 + BL (retorno SFX 4). Nome mantido como `provavel`; falta achar o dono.

## Em aberto
- Dono das duas funções acima (Map_CreateChoiceSprites e KeyItemBag_UseItem) e a função que contém os SceneState_*.
- Validar o comportamento dos SceneState_* (todos ainda chute do SC4).
