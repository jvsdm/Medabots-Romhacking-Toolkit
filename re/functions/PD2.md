# PD2: modo/sub da tela de detalhe de peca, tabela 0x083AC9EC/9F8 e nomes da gPartsGridStateTable (GH-PD2, 2026-10-14)

Escopo: (1) quem grava ctx+0x3E (modo) e ctx+0x44 (sub), ctx = 0x03000440; (2) quem le as linhas 0x083AC9EC/0x083AC9F8;
(3) conferir entradas 15/32/47/61/106 da gPartsGridStateTable 0x083AC86C.
Metodo: get_xrefs_to e decompile (Ghidra, leitura), palavras da ROM original (python -I, so leitura).
Nada salvo no Ghidra, mtbe.csv intocado, sem commit.

## 0. Enderecos (ctx = 0x03000440)
- 0x0300047E = ctx+0x3E: modo da tela de detalhe (0..3), lido por PartDetail_DispatchSubstate 0x0800E78C.
  O mesmo byte e reaproveitado pela grade (flag 0x80 de selecao e contador 0..3), ver 1a.
- 0x03000484 = ctx+0x44: sub = linha/cursor da lista de detalhe; ctx+0x45 = numero de linhas (limite).
- ctx+0x3C: sub-estado (0..4) do despacho de PartsGrid/Tile. ctx+0x3D: indice da celula (row*6+col).

## 1. Quem grava e de onde se entra

### 1a. ctx+0x3E (modo)
- Loja (provavel, forte): Gfx_FadeThenPreviewInit 0x080627A0 chama
  FightPreview_InitSideState(*(byte*)(*DAT_08062800+0x19), gShopItems[...]*8). O modo vem de um byte da struct
  de loja (+0x19); o param2 e um item da lista de loja (u16). A funcao grava ctx+0x3E = param para 0..3
  (0 forca 0; 1/2/3 passam direto; outros valores nao gravam) e ctx+0x3C = 3.
- Batalha (confirmado no codigo; entrada nao localizada): Fight_InitPartStateSetGS10 0x08011720 grava
  ctx+0x3E = byte [struct+0x168] (struct em DAT_08011700, nao resolvido), ctx+0x3C = 2, ctx+0x3D = tabela.
  Sem xref direto: entra por tabela de estados (o nome diz GS 0x10). Fight_PreviewFadeThenInit 0x080624DC
  tambem chama a mesma funcao (fluxo de pre-batalha).
- Saida da grade: PartsGrid_TickRouteSelection 0x080092D4 grava ctx+0x3E = 0 ao sair para outra tela (gs 0x68).
- Uso da grade (nao e modo): PartsGrid_ToggleSelection grava (row*6+col)|0x80 em ctx+0x3E;
  PartsGrid_MoveSelectionOption4 0x0800F150 faz contador 0..3 com wrap em ctx+0x3E; PartsGrid_InputStep 0x08007868
  le o bit 0x80.
- Modo 1 x modo 2: PartDetail_DispatchSubstate usa a MESMA tabela B nos dois casos (cmp r1,#1 / bge). Nenhuma
  diferenca dentro desta funcao. Quem escolhe 1 ou 2 e o valor de entrada (loja: byte +0x19; batalha: byte +0x168).
  Diferenca visual: aberto.

### 1b. ctx+0x44 (sub)
- PartDetail_HandleCursorMoveInput 0x0800E6E0 e o escritor do avanco: cima/baixo mudam ctx+0x44 com wrap em
  ctx+0x45-1, depois PartDetail_UpdateSelectionCursorPosition. Logo sub = linha selecionada, nao sub-estado de
  maquina. PartDetail_DispatchSubstate usa esse sub como indice da tabela A/B.
- Zerado nas entradas de tela da grade: PartsGrid_InitScreen_CellGraphics 0x08007B0A, PartsGrid_InitScreen 0x08007F46,
  PartsGrid_InitScreen_StatusPreview 0x08008344, MedabotGrid_InitScreen 0x0800DCA2. Escrito tambem em
  PartsGrid_SlotScriptInputStep 0x08008C3E/C4A/C14.
- Correcao ao JT8 secao 4: o avanco de ctx+0x44 nao esta em PartDetail_AnimSlotDispatchStep, e sim em
  PartDetail_HandleCursorMoveInput.

## 2. Tabela 0x083AC9EC/0x083AC9F8: nao e tabela separada
- get_xrefs_to em 0x083AC9EC e 0x083AC9F8: vazio. Nenhum ponteiro de codigo aponta para eles.
- A unica referencia e a 0x083AC86C (gPartsGridStateTable, 112 entradas) em PartsGrid_DispatchStateTable 0x08007584:
  CallViaR0(gPartsGridStateTable[*0x0300137C]) quando Battle_CheckExitShortcut()==0.
  - despachante: PartsGrid_DispatchStateTable 0x08007584 (sem xref direto; chamado pelo loop);
  - indice: gs = byte em 0x0300137C;
  - as "linhas de 3 ponteiros" sao entradas 96..101 da mesma tabela (0x083AC86C + 4*i):
    96 (0x083AC9EC) 0x08009175 PartsGrid_ReloadRowTilesAndSetup
    97 0x080091E1 PartsGrid_OptionRowStep
    98 0x08009249 PartsGrid_OptionHandlerStep
    99 (0x083AC9F8) 0x08009275 PartsGrid_TickFadeOutStep
    100 0x080092D5 PartsGrid_TickRouteSelection
    101 0x08009375 PartsGrid_TickDispatchSubstate
  - o "stride 0xC" do JT8 era so o agrupamento de 3 entradas consecutivas. Nao existe tabela de 3 colunas.
- O label 0x083AC880 (table_partsgrid_states, JT8) cai no meio da tabela (offset 0x14): provavel mislabel. Nao renomeado.

## 3. Entradas da gPartsGridStateTable (lidas na ROM)
- 15, 32, 47, 61 = 0x08007D29 = funcao Tile_ClearSlotsAndDispatchState (generica): ClearObjectAttributeEntries(0,0x80)
  e despacho por ctx+0x3C (<5) pela tabela em 0x08007D50. Nao e de outra tela: e helper de tile usado como estado
  da grade. O sufixo _JumpTable esta errado (o label era de dado; a funcao e a entrada).
  Rename: rename_function_by_address falhou; rename_data nao alterou o nome da funcao. Pendente.
- 106 = 0x080093AD = funcao 0x080093AC (antes Shop_SnapshotQuantitiesTable): copia 0x40 bytes de 0x02023000
  (label mtbe EWRAM_LevelSortScratch; DM diz quantidade por item na loja) para 0x03000BE0 + ctx[0x3D]*0x40
  (gMedalRecords_RAM, 30 registros) e faz gs++ (0x0300137C). "Shop_" nao se sustenta: a funcao so copia e avanca
  estado da grade. Renomeada para PartsGrid_SnapshotScratchToCellStep (provavel). Quem preenche o scratch: nao verificado.

## 4. Renames no Ghidra (nao salvos)
- 0x080093AC: Shop_SnapshotQuantitiesTable -> PartsGrid_SnapshotScratchToCellStep (provavel).
- 0x08044BF0: FightPreview_InitSideState -> PartDetail_InitWithMode (confirmado: grava modo e ctx+0x3C).
- Falhou: 0x08007D28 Tile_ClearSlotsAndDispatchState_JumpTable (ver secao 3).

## 5. Conflitos e pendencias
- ST2 diz que gPartDetailMode = 0x03002D59 (gObjectSlots+0x129). Esta analise diz que o modo de detalhe e ctx+0x3E
  = 0x0300047E. Conflito aberto: falta xref de 0x03002D59 para decidir.
- Fight_InitPartStateSetGS10: quem chama pela tabela de estados e o que e a struct DAT_08011700 (+0x168). Pendente.
- Modo 1 x 2 na tela: pendente.
- Origem do buffer 0x02023000 (entrada 106): pendente.
- Confianca: enderecos e escritas = confirmado (decompile/xref). Papel de cada modo (loja x batalha) = provavel.
