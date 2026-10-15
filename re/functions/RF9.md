# RF9 - trocas e achados

Lista: RF9_lista.txt (34 itens: 7 funcao, 27 dado). Detalhes por endereco em RF9_simbolos.csv.

## Trocas de nome no Ghidra (4)
- 0x08068924: FUN_08068924 -> Tick_ModStepSetCountdown (provavel)
- 0x08068978: FUN_08068978 -> Gfx_ModStepFillTable4_DispCnt (provavel)
- 0x080692EC: FUN_080692ec -> Menu_EnterClearGfxSndRequest (provavel)
- 0x0806E9C0: FUN_0806e9c0 -> Ctx_ResetModeFlags (chute)

## Funcoes ja nomeadas no Ghidra, sem troca (3)
- 0x08060688 Dispatch_CallTableEntryByByte (confirmado)
- 0x08061958 Task_CallHandlerTableThenCount_B (confirmado)
- 0x08063948 Task_CopyCallTableEntryThenCount (confirmado)

## Dados
- 0x08066860 Menu14_CallSubstateHandler_Pool: pool de literais, nao funcao (provavel).
- 0x08067030 PointRing_StepFrame: bytes sao codigo, mas fora de funcao no Ghidra. Mantido como label chute.
- Tabelas de salto (mov pc), todas stride 4 bytes:
  - KeyItemBag_DispatchTable 0x0805D2C8: 41 entradas
  - dispatch_scene_state 0x0806A134: 23 entradas
  - dispatch_state_8 0x0806BD98: 8 entradas
  - anim_dispatch_state 0x0806CB6C: 7 entradas
- Demais 22 itens sao alvos de switch ou desvios, labels provavel sem nova analise.
