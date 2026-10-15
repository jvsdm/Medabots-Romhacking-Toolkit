# RF2 - revisao de itens (RF2_lista.txt, 34 itens)

Feitos: 34 de 34. Renomeados: 0 (nenhuma troca de nome; nomes do mtbe mantidos).
Escrito em RF2_simbolos.csv (34 linhas).

## Funcoes (2)
- 0x08002574: sem nome no mtbe; Ghidra ja tem `Scene_InitBg1AndAdvanceCounter`. Corpo confere. confirmado.
- 0x0800275C: sem nome no mtbe; Ghidra ja tem `Obj_DispatchStateAndCountDone`. Corpo confere. confirmado.

## Dados de cabecalho (2)
- 0x080000A0 header_title = MEDABOTSMTBV; 0x080000AC header_gamecode = A8BP. Bytes conferem. confirmado.

## RAM / registradores de hardware (16)
Nomes conferem com o mapa de IO do GBA (offsets 0x10..0x1E, 0x40, 0x88, DMA0/1/2, 0x124, 0x12A).
Usos na ROM: ver contagem de literais no CSV. Nenhuma troca.

## Rotulos de salto (alvos de tabela / miolo de funcao) (14) - kind dado -> label
- 0x08001A00: e Thumb dentro de BuildShadowOamFromObjectState (0x08001520-0x08001C9F), nao dado. Kind virou label, provavel. O prefixo "Obj_Mid" e chute.
- PartEditor_SubmodeCase0..4 (0x08004690..0x08004728): alvos da tabela 0x0800467C (5 x 4, stride 4). Labels provaveis.
- Parts_DispatchSlotHandler_JumpA/B (0x08006844, 0x080068A2): mov pc dentro de Parts_DispatchSlotHandler. Labels provaveis.
- Parts_DispatchSlotAction_ReadActionIndex / _Jump / _CheckByte9B (0x08006F44, 0x08006F56, 0x08006FA0): miolo de Parts_DispatchSlotAction. Tabela 0x08006F64 = 5 x 4.
- Tile_ClearSlotsAndDispatchState_Jump (0x08007D46): mov pc dentro de Tile_ClearSlotsAndDispatchState.
- Parts_PrepareFourPartSlots_AltEntry_8005a60 (0x08005A60) e Parts_BuildEquipTable_AltEntry_8007034 (0x08007034): sem push, sem funcao no Ghidra neste endereco. Ficam como label provavel; o dono nao foi identificado.

## Divergencias para o mtbe (nao alterei o mtbe.csv)
1. Tabela 0x08006854 e 0x080068AC: no mtbe as entradas seriam 0x08006238/284/2D0/31C/368. Os bytes da ROM dizem 0x08006868..0x08006888 (tabela A) e 0x080068C0..0x080068E0 (tabela B), stride 8. Vale reconferir antes de fundir.
2. mtbe marca 0x08007D28 como "Tile_ClearSlotsAndDispatchState_JumpTable" (data). Os bytes sao codigo (30 B5 82 B0 = push/sub), e o inicio da funcao. Nao e tabela.

## Em aberto
- Nenhum rename feito no Ghidra (todos os nomes ja estavam certos ou ja existiam).
- Dono de 0x08005A60 e 0x08007034: nao lido (fora do escopo do lote).
