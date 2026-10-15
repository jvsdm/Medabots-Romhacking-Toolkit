# PD4: modos 0-3 da tela de detalhe, tabela de estados que contem 0x083ACFB0 e escritor de 0x02023000 (GH-PD4, 2026-10-14)

Escopo: (1) o que cada modo de ctx+0x3E mostra/permite e de onde vem; (2) base da tabela com 0x083ACFB0 e quem a indexa;
(3) quem preenche 0x02023000 antes de PartsGrid_SnapshotScratchToCellStep. Metodo: Ghidra (decompile, xrefs, pool lido do ROM via python -I).
Nada salvo no Ghidra. mtbe.csv intocado. Sem commit.

## 1. Modos 0-3 (ctx+0x3E = 0x0300047E)
Leitura principal: o modo e a PARTE DO BOT (corpo), nao um modo de permissao. Modo 0 = cabeca, 1 e 2 = bracos, 3 = pernas (provavel;
o que sustenta: sorteio de 4 partes em Fight_AssignRandomBotPartSlot, badge/icone de perna no modo 3, e o numero de linhas de stats).
- Origem na BATALHA: Fight_AssignRandomBotPartSlot 0x08012344 grava fight+0x168 = randbyte()&3 (confirmado) e fight+0x169 = byte do
  registro do sorteado. Fight_InitPartDetailFromBattleMode 0x080116C4 le esses dois e grava ctx+0x3D/ctx+0x3E.
- Origem na LOJA/PREVIEW: FightPreview_InitSideState 0x08044BF0 (ja renomeada; o nome PD2 PartDetail_InitWithMode saiu). Param1 = modo ->
  ctx+0x3E; param2 = celula -> ctx+0x3D; grava ctx+0x3C = 3 (a batalha grava 2). Chamadores: 0x080624DC (Fight_PreviewFadeThenInit,
  nome herdado, endereco na regiao da loja) e 0x080627F0 (Gfx_FadeThenPreviewInit). Quais valores a loja passa: nao lido ainda.
- Na tela (confirmado no decompile):
  - PartDetailScreen_Init 0x0800E23E: GetPartStats(ctx+0x3D, ctx+0x3E) -> todos os dados do item usam (celula, modo).
  - Stats (PartDetail_DrawCurrentSelectionStats 0x0800FA86): modo 0 = 4 linhas; modos 1 e 2 = 5 linhas (mesmo ramo); modo 3 = 6 linhas
    com outro grupo de ponteiros de numero.
  - Badge (0x0800F7DA): modos 0-2 usam Speciality_IsRemotenessClass(part+0xF0); modo 3 usa byte part+0xEF e outro tile base.
  - Descricao (0x0800F860): texto = ponteiro[celula*4 + modo] (tabela em 0x0800F898). Cada modo tem texto proprio.
  - Atributo (0x0800EAD2): A usa Inv_ReadCounter16(ctx+0x3D, ctx+0x3E) para tocar som de "cheio" (6) ou o normal (4).
- Botao A na tela (PartDetail_HandleSelectionConfirm 0x0800E400, confirmado): grava ctx+0x43 = 3 (modo 0), 4 (modo 1 e 2), 5 (modo 3).
  O despacho PartDetail_DispatchSubstate 0x0800E78C le ctx+0x44 (nao +0x43): o elo +0x43 -> +0x44 nao foi achado (aberto).
  Despacho: modo 0 -> JT_Mode0 (7 entradas, em 0x0800E7D0), modo 1/2 -> JT_Mode12 (8 entradas, 0x0800E884), modo 3 -> JT_Mode3 (8, 0x0800E948).
- Modo 1 x modo 2: NAO ha ramo de codigo que os separe nas funcoes lidas (stats, badge, confirm, despacho). Diferem nos DADOS
  (GetPartStats, texto da descricao, contagem do atributo). Provavel: braco direito x esquerdo (ordem nao confirmada).
- O que cada sub-estado faz (JT_Mode0/12/3 handlers): NAO lido. Logo "equipar / so ver / comparar" continua ABERTO. Nao foi achado
  nenhum ramo de "equipar" nas funcoes lidas; o que se ve e so o detalhe com a parte do bot.

## 2. Tabela de estados com 0x083ACFB0
- Inicio da tabela: 0x083ACCD4 (primeiro ponteiro Thumb contiguo; 0x083ACCD0 nao e ponteiro). Fim: 0x083AD060. 228 entradas de 4 bytes.
  Entradas com 0x08000840 = stub vazio (sem acao). A PD3 nao achava a base porque buscava ponteiro literal exato; a base e so o inicio
  da sequencia.
- Indice de 0x083ACFB0 = (0x083ACFB0 - 0x083ACCD4)/4 = 0xB7. Ou seja, gs = 0xB7 chama Fight_InitPartDetailFromBattleMode.
  Vizinhos (0xB4..0xBB): 0x080115B4, 0x080115E8, 0x08011644, 0x080116C4, stub, 0x08011764, 0x08011A58, 0x08011ABC.
- Quem indexa: Menu5_Dispatch_PartsScreens (inicio em 0x0800DA0x; nome ja existente, endereco exato nao confirmado). Pool 0x0800DA2C
  = 0x083ACCD4; codigo faz CallViaR0(table[*0x0300137C]) = gs. Confirmado.
- Fato a verificar: a funcao grava gs = 0x10 (indice do stub 0x08000840 nesta tabela). Nao e erro: gs 0x10 neste despacho e no-op e a
  tela de detalhe roda por outro despachante. Quem escreve gs = 0xB7 (o que dispara a entrada) nao foi achado: aberto.
- Rename feito no Ghidra (nao salvo): 0x083ACCD4 -> Menu5_StateHandlerTable (provavel).

## 3. Quem preenche 0x02023000
- 0x02023000 = gShopQuantities (confirmado pelo pool 0x08061FF0). Buffer de 0x40 bytes: um byte de quantidade por slot de compra.
  Usa +0x78 + ctx[0x1a] como indice (cursor) e os limites no registro 8-byte em 0x02040000.
- Escritor real: Shop_TickBuyQuantityMenu 0x080621E6 faz +1 e -1 nos bytes (confirmado no decompile) conforme o botao do cursor
  (ctx+0x1A = slot, 0x78 = offset). Ele e quem enche a quantidade, pelo jogador.
- Shop_InitPartSelectionScreen 0x08061C68 NAO escreve no buffer: le so (quantidade maxima / preco vindos de GetPartStats) e monta
  gShopItems em 0x02040000 (8 bytes por registro). Logo nao zera o buffer aqui.
- PartsGrid_SnapshotScratchToCellStep 0x080093AC copia 0x40 bytes do buffer para a celula da grade (ctx+0x3D * 0x40), e incrementa o contador.
- Zeragem do buffer: NAO achada. Xrefs de 0x02023000 (DATA/READ): 0x08061E78 (Shop_RefreshPartListScreen, READ), 0x08062698 e 0x08062744
  (Shop_CommitBuySellTransaction, READ), 0x080639DE (Screen_InitNumberRevealLayout, READ). Candidato: a funcao que abre o menu de
  quantidade. Aberto.
- Descartado: Fight_PickMostDamagedRepairablePart1Based, Level_ComputeReferenceFromSlots e Sort_Records8Descending usam o mesmo endereco
  mas com outro registro (8 bytes, PARAM), nao sao o escritor da copia da loja.

## 4. Renames no Ghidra (nao salvos)
- 0x083ACCD4: Menu5_StateHandlerTable (provavel, rename_data).
- Ja existentes e confirmados por esta rodada: FightPreview_InitSideState (0x08044BF0), Fight_AssignRandomBotPartSlot (0x08012344),
  PartDetail_HandleSelectionConfirm, PartDetail_DispatchSubstate, PartDetail_DrawCurrentSelection* (ja nomeados).

## 5. Correcoes ao PD3
- PD3 secao 2 dizia que modos 1 e 2 "produzem o mesmo efeito" na tabela: correto, mas o modo e a parte do bot (cabeca/braco/perna),
  nao um modo de funcao. PD3 secao 4 ainda aberto: nao ha escritor de 0x02023000 que o zere (ver secao 3).
- PD3 secao 1a (chamador de 0x080116C4 por 0x080624DC): a frase foi riscada no PD3; aqui 0x080624DC chama SO FightPreview_InitSideState
  (0x08044BF0), nao Fight_InitPartDetailFromBattleMode.

## 6. Pendencias
- Equipar / so ver / comparar: abrir JT_Mode12 (8 handlers em 0x0800E884), JT_Mode0 e JT_Mode3. Nao lido.
- Valores de modo passados pela loja em 0x080624DC e 0x080627F0.
- Elo ctx+0x43 -> ctx+0x44 (quem copia).
- Escritor que coloca gs = 0xB7 (entrada Fight_InitPartDetail) e quem zera 0x02023000.
