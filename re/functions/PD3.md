# PD3: chamador de Fight_InitPartStateSetGS10, struct de fight_information, modos 1x2 da tela de detalhe, conflito 0x03002D59 e buffer 0x02023000 (GH-PD3, 2026-10-14)

Escopo: (1) entrada real e chamador de Fight_InitPartStateSetGS10 + struct DAT_08011700; (2) diferenca modo 1 x 2 de ctx+0x3E;
(3) conflito gPartDetailMode 0x03002D59 x ctx+0x3E 0x0300047E; (4) quem preenche 0x02023000.
Metodo: Ghidra (leitura: decompile, disassemble, xrefs), ROM original so leitura via python -I. Nada salvo no Ghidra (sem save), mtbe.csv intocado, sem commit.

## 1. Entrada real, chamador e struct
- A entrada real e 0x080116C4 (`Fight_InitPartStateSetGS10`, search_functions_by_name). 0x08011720 e so um ramo interno (ja nao e funcao propria).
- Renomeada no Ghidra para `Fight_InitPartDetailFromBattleMode` (provavel; era Fight_InitPartStateSetGS10). Nome antigo no CSV.
- Chamador: NAO ha xref direto para 0x080116C4 nem para 0x080116C5 (so um dado). A unica referencia e uma palavra de dado em
  0x083ACFB0 = 0x080116C5. Essa palavra cai numa tabela de ponteiros de estados de batalha (vizinhos: Fight_CommitPaletteOscStep
  0x080115B4, Fight_FadeRowsUpStep 0x080115E8, Fx_InitBlendTableAndTick 0x08011644, Fight_InitBotPartPanel 0x08011764,
  Fight_FadeRowsDownStep 0x08011A58, Fight_DrawOscSetGS3D 0x08011ABC). Entao ela e chamada por indice de estado (gs), nao por
  chamada direta.
  - A base da tabela NAO foi achada. Nao existe ponteiro literal para 0x083ACF00-0x083AD000. Descartei 0x083ACCD0 (e dado de
    DebugMotion_Main 0x0800D7BC, nao tabela). Dispatcher de bot 0x08013650 usa outra base (0x083B722C) e nao e este.
  - Fato de encaixe: a funcao grava gs (0x0300137C) = 0x10, entao a tabela e indexada por gs; o indice de 0x083ACFB0 fica em aberto.
- Correcao ao PD2 secao 1a: a frase "Fight_PreviewFadeThenInit 0x080624DC tambem chama a mesma funcao" NAO se sustenta: nao ha xref
  para 0x080116C4/0x080116C5 vindo de 0x080624DC. Riscar essa afirmacao.
- Struct DAT_08011700 = ponteiro para fight_information = 0x03001800 (valor no pool 0x08011700, confirmado).
  - +0x168 = 0x03001968 (fight_information+0x168): MODO. Ramo: 0 = sai sem gravar; 1, 2, 3 = grava.
  - +0x169 (DAT_08011758 = 0x169): indice lido para ctx+0x3D (= celula row*6+col da grade).
- A funcao grava: ctx+0x3C = 2 (zera 0x14 bytes de ctx+0x3C antes), ctx+0x3D = fight+0x169, ctx+0x3E = modo, gs = 0x10 (0x0300137C),
  e chama Slots_ClearFourPairs e ClearObjectAttributeEntries(0,0x80); zera o byte ctx+0x96 e a word 0x03000430 (DAT_080116FC).
- Leitura: esta e a ponte batalha -> tela de detalhe de peca (Fight prepara o modo e entra no estado GS10 da grade).

## 2. Modo 1 x modo 2 (ctx+0x3E)
- Dentro de Fight_InitPartDetailFromBattleMode: modo 1 e modo 2 produzem o MESMO efeito (zera ctx+0x3C..+0x50, grava 2 em ctx+0x3C,
  copia +0x169 e grava o modo). Modo 0 nao grava nada (sai cedo). Modo 3 = mesmo efeito de 1/2.
- Em PartDetail_DispatchSubstate 0x0800E78C (confirmado no decompile, mais preciso que o PD2):
  - modo 0: tabela JT_Mode0 (sub < 7);
  - modo 1 e 2: MESMA tabela JT_Mode12 (sub < 8);
  - modo 3: tabela JT_Mode3 (sub < 8).
  Logo a diferenca 1x2 nao esta no despacho de sub-estado, e sim nos handlers que leem ctx+0x3E. Eles (xrefs de 0x0300047E,
  leitura): PartDetail_DrawCurrentSelectionSpecialityBadge 0800F7DA, PartDetail_DispatchLegIconStep 0800E362, PartDetailScreen_Init
  0800E23E, Menu_PartsDetailDispatch 0800E560, PartDetail_DrawCurrentSelectionDescription 0800F860, PartDetail_DrawCurrentSelectionStats
  0800FA86, PartDetail_HandleSelectionChangeInput (0800FE1E..0800FF0A), PartDetail_HandleSelectionConfirm 0800E400,
  PartDetail_HandleAttributeCursorInput 0800EAD2, PartDetail_CountAvailablePartsForCurrentAttributeFilter 0800FC8A/FCAC.
  - NAO decompilados um a um (faltou contexto). Diferenca visual 1x2 = ABERTA. Proximo passo: decompilar PartDetailScreen_Init
    0800E23E e PartDetail_DrawCurrentSelectionStats 0800FA86 e comparar os ramos de ctx+0x3E == 1 e == 2.
- Quem grava 1 ou 2: escritores de ctx+0x3E (xrefs 0x0300047E WRITE) = Fight_InitPartDetailFromBattleMode 08011742 (modo vindo de
  fight+0x168), PartDetail_InitWithMode 08044C04/08044C0C (modo vindo do argumento = loja), PartsGrid_TickRouteSelection 08009362
  (grava 0), PartsGrid_ToggleSelection, PartsGrid_MoveSelectionOption4, PartsGrid_InputStep, PartEditor_DispatchSubmodeStep
  (08004700/080046DE/08004742 - ainda nao lido).
  - Batalha grava 1/2/3 conforme fight+0x168; loja grava conforme o argumento de PartDetail_InitWithMode. Qual valor cada uma usa:
    aberto.

## 3. Conflito: gPartDetailMode 0x03002D59 (ST2) x ctx+0x3E 0x0300047E
- Resolvido: o modo da tela de detalhe e ctx+0x3E = 0x0300047E. Evidencia: PartDetail_DispatchSubstate le ctx+0x3E (0x0800E79C) e
  Fight_InitPartDetailFromBattleMode/PartDetail_InitWithMode gravam nele.
- 0x03002D59 = gObjectSlots (0x03002C30) + 0x129. NAO e o modo da tela. Quem o usa:
  - leitura: PartDetail_DispatchModeHandler 0x08010146 (renomeado para ObjSlot_DispatchState129Handler, provavel): CallViaR0(
    table 0x083ACE14[byte*4]) e retorna (byte == 3). A tabela 0x083ACE14 comeca em 0x08010171 (handlers 0x080101E9, 0x0801022D, ...).
    Esse handler vive no ramo de cursor de atributo (PartDetail_MoveAttributeCursor 0x08010130 usa outro ctx);
  - leitura tambem em BgMap_FillRectTickCounter 0801024C, PartsBuild_SetOrClearSlotShapeAndAdvance 080101D6, Slot_CountdownStepAndClear
    08010214;
  - escrita: PartDetail_SetupAttributeCursorSprite 08010096, BgMap_FillRectTickCounter 08010250, Slot_CountdownStepAndClear 08010218,
    PartsBuild_SetOrClearSlotShapeAndAdvance 080101DA, e ObjScript_Op1F/2C/39/0D/13 (SpawnEffectSlot*, 0803A662/0803AE92/0803B7EA/
    08039942/08039D50). Ou seja: byte de estado do slot de objeto, compartilhado com efeitos/sprites.
- Decisao: ST2 esta errado ao chamar 0x03002D59 de gPartDetailMode. Proposta: gObjectSlots_State129 (provavel), nao aplicado como
  label de dado (so comentario no CSV).

## 4. Buffer 0x02023000 (copiado por PartsGrid_SnapshotScratchToCellStep 0x080093AC)
- Xrefs de 0x02023000 (EWRAM): escritores diretos = Sort_Records8Descending 0804247E (WRITE), Level_ComputeReferenceFromSlots
  08042086 (WRITE, recebe por PARAM), Fight_PickMostDamagedRepairablePart1Based 080331C4 (WRITE, PARAM em 0803318E),
  e Shop_TickBuyQuantityMenu 080621E6 (PARAM) mais refs DATA em 0806xxxx (tabela de loja).
- Leitores: Shop_InitPartSelectionScreen 08061C68, Shop_RefreshPartListScreen 08061D72 (DATA), Shop_CommitBuySellTransaction
  080626E4/08062672, Screen_InitNumberRevealLayout 080639DE, Sort_Records8Descending 08042462/08042476, PartsGrid_SnapshotScratchToCellStep 080093BA (PARAM).
- Conclusao: 0x02023000 e um scratch EWRAM compartilhado (buffer de registros de 8 bytes, ordenado por Sort_Records8Descending,
  usado por Level_ComputeReferenceFromSlots, Fight_PickMostDamagedRepairablePart1Based e a loja). Quem o preenche com a
  quantidade de cada item antes da copia (entrada 106 da gPartsGridStateTable): NAO fechado. Proximo passo: ver o escritor que
  roda antes de 080093AC (Shop_TickBuyQuantityMenu 080621E6 com PARAM e Shop_InitPartSelectionScreen).

## 5. Renames no Ghidra (nao salvos)
- 0x080116C4: Fight_InitPartStateSetGS10 -> Fight_InitPartDetailFromBattleMode (provavel).
- 0x08010146: PartDetail_DispatchModeHandler -> ObjSlot_DispatchState129Handler (provavel).
- Ja pendentes do PD2 (nao refeitos aqui): 0x080093AC -> PartsGrid_SnapshotScratchToCellStep; 0x08044BF0 -> PartDetail_InitWithMode.

## 6. Pendencias
- Base da tabela de estados que contem 0x080116C4 (indice real da entrada e chamador): aberta.
- Diferenca visual 1x2: aberta (ver secao 2, proximo passo).
- Quem preenche 0x02023000 antes da copia: aberta.
- PD2 secao 1a: corrigir a afirmacao de Fight_PreviewFadeThenInit (sem xref).
- Confianca: enderecos, xrefs e renames = confirmado (decompile/xref/pool). Papel de cada modo na tela = provavel/aberto.
