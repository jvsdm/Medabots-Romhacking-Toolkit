# RF6 - revisao de 34 itens (RF6_lista.txt)

## Trocas
- 0x08043820 Table_BattleSpritePartPairs_Entry2 (antes: AreBattleSpritePartIdsValid): nao ha funcao nesse endereco; e a entrada 2 de uma tabela de 5 x 4 B que comeca em 0x08043818 (provavel). Nao renomeado no Ghidra (nao existe funcao ali).
- 0x08043818 Table_BattleSpritePartPairs (novo, provavel): 5 entradas de dois u16, stride 4.
- Tipos corrigidos no CSV (lista marcava "dado", bytes e Ghidra mostram codigo): Fight_ResetBotPartState, ObjScript_Op0D/1E/1F/2C/32/39, Overlay_InitScreenGraphics_AltEntry_803ddc6, Map_UpdateBackgroundAnimations_AltEntry_80481d8, EmitCompositePreviewTemplateEntriesSortedByY (label), Bot_InitStarterRecord (func_thumb, confirmado pelo decompile).
- Nenhum rename feito no Ghidra: todos os nomes de funcao conferidos no decompile estao corretos.

## Achados
- 0x0804A6BC, 0x0804A6EC, 0x0804AB18, 0x0804AB3C, 0x0804ABC0 sao wrappers de corpo identico (so GameState_AdvanceSubroutine). Os sufixos A-E sao so ordem de endereco; cabe a um agente de dispatch decidir se merecem nome unico.
- 0x0804ABA8 e 0x0804AB24 tambem sao identicos (zera byte+1, seta state 5).
- Pool de literais 0x0804A584 e 0x0804A8C8: 0x08765EC0 (base do banco de eventos) seguido de 0x030057D8 (flags).
- lit_EventDbBase0 (0x0804A3B6) e codigo, nao literal; o nome lit_ e impreciso e ficou mantido.
- TextItalicFont_Use (0x08040144): bytes parecem literal 0x084C59A4; mtbe diz codigo de texto. Conflito aberto, chute.
- EmitCompositePreviewTemplateEntriesSortedByY (0x08045858): shell de 12 B, corpo nao recuperado. Segue chute.
- Labels de ObjScript (Op0D/1E/1F/2C/32/39) nao tem funcao no Ghidra: confirmados so como alvos de switch (provavel).
