# GH-IN2: loja e key items (continuação de IN.md)

Status: EM ANDAMENTO (2026-10-14). Salvo a cada passo.

## Passo 1: quem chama as funções "Shop_*" (em andamento)
- Xrefs (Ghidra, 2026-10-14): Shop_InitItemListScreen 0x0805CDAC so aparece em 0x0848363C (tabela 0x08483634);
  Shop_ItemListInputStep 0x0805CFB4 so em 0x08483648 (mesma tabela). Dispatch_StateTableFrameCount 0x0805CB50 e
  referenciado so por DADO em 0x084835A4 (primeira entrada da tabela de quadros; sem literal de codigo achado).
- Shop_BuildItemList64 0x0805AE4C: chamado por Shop_InitItemListScreen (0x0805CDC0) e Shop_ItemListInputStep (0x0805D1D0).
- Shop_HandleItemListAndCounts 0x0800C804: sem xref direto; aparece so na tabela de despacho de DEBUG 0x083ACAC0
  (idx 12 de 0x0800BAD8, debug_process/Debug_PartsSpawnScreen etc.). Nao e chamado por nenhum caminho de jogo achado.
- Texto usado no evento 0x36 (GetItem): literal 0x080480E0 -> 0x08483ED8 (gTextKeyItems, 64 ponteiros validos); o
  mesmo ponteiro aparece no literal 0x0805B028 (perto de Shop_BuildItemList64). Logo o inventario Map64 0x03002BA0 e a
  BOLSA DE KEY ITEMS (indice = id). CONFIRMADO por referencia de texto.
- Shop_BuildItemList64 nao monta lista de venda: copia os itens POSSUIDOS (bit 0x80) do Map64. Logo a tela
  Shop_ItemListInputStep e a bolsa de key items (uso/visualizacao), nao compra.
- DECISAO (1): Shop_BuildItemList64, Shop_InitItemListScreen, Shop_ItemListInputStep = BOLSA DE KEY ITEMS (renomeados
  no Ghidra: KeyItemBag_BuildOwnedList 0x0805AE4C, KeyItemBag_InitListScreen 0x0805CDAC, KeyItemBag_ListInputStep
  0x0805CFB4). Shop_HandleItemListAndCounts = DEBUG (renomeado Debug_KeyItemCountEditorStep 0x0800C804): edita
  Map64 e concede itens; so alcancavel pela tabela de debug 0x083ACAC0. Nao e loja de venda.
  Obs.: a fila gShopItems (0x02000000) e reaproveitada pela bolsa; por isso o nome antigo "Shop" confundia.

## Passo 2: tabela de despacho 0x0805D2C8 (0x28 entradas) [confirmado, lida na ROM]
- Usada em Shop_ItemListInputStep (0x0805CFB4): so se FUN_08048960(id) devolve 1, e id (halfword de gShopItems) <= 0x28.
  Senao MainState_RequestModeTransition(4). Chamada indireta com o alvo (Ghidra: switchdata).
- Entradas (id: alvo): 0 -> 0x0805D36C; 1 -> 0x0805D384; 5 -> 0x0805D398; 12 -> 0x0805D3AC; 13 -> 0x0805D3C0;
  19 -> 0x0805D3D4; 39 -> 0x0805D3E8; todos os demais (2,3,4,6..11,14..18,20..38) -> 0x0805D410 (caso padrao).
- Esses alvos ficam DENTRO do corpo de Shop_ItemListInputStep (sem funcao propria no Ghidra). Efeito por id NAO
  decodificado nesta rodada. Logo: so 7 ids tem caso proprio; o resto cai no mesmo caminho.
- Palavra logo apos a tabela (0x0805D3FC) nao e alvo. [confirmado]

## Passo 3: loja de PECAS (scene de loja), sequencia achada
Ordem (confirmado onde marcado):
1. Evento 0x59 Open_Shop (0x0804CD84) le 1 byte = id da loja (0..14). Continuacao 0x0804F67C roda a sequencia com
   o id (FUN_08050CCC; a funcao so tem tabela local de 5 estados em ctx+0xB). [confirmado]
2. Id da loja fica em byte 0x03002420 (lido por 0x08061CC8 em Shop_InitPartSelectionScreen). Referencia (literal) a 0x03002420 em codigo perto de
   0x08047AE4 (funcao nao localizada no Ghidra). [provavel]
3. Monta a lista (Shop_InitPartSelectionScreen 0x080619B0) [confirmado]:
   - se byte +5 da estrutura == 1 (modo VENDA): Shop_BuildAvailablePartList (pecas possuidas).
   - senao (COMPRA): le gShops (0x084838E4) + id*4 + i, i = 0..3, para bot != 0xFF; para cada entrada grava em
     gShopItems (0x02000000, 8 B por entrada): +0 u16 bot; +2 u8 quantidade possuida (gInventory 0x03004C40,
     indice bot*2 + tipo*0xF0); +3 u8 byte de stats; +4 u32 PRECO.
   - Corte: a leitura para se bot == 0xFF, ou se bot == 0x20 e tipo != 3 [confirmado no codigo].
4. PRECO por peca: GetPartStats(bot, tipo) copia 16 B de gPartStats + bot*0x40 + tipo*0x10 (0x083B83FC); preco =
   byte +0x0B desse registro * 100 (gravado em gShopItems+4). Nao ha tabela de preco separada. [confirmado]
   Obs.: a tabela de preco e este campo, nao um array a parte.
5. Quantidades a comprar: 1 byte por entrada em 0x02023000 (zerado/gravado na tela). Total = soma(preco*qtd)
   (Shop_TickBuyQuantityMenu 0x08061F00; limite 99 por peca; total cap 0x1869F = 99999 na linha 0x08061CDC).
   Venda (modo 1): total >> 1. [confirmado no Init; Tick ja lido em IN.md]
6. Compra/venda (Shop_CommitBuySellTransaction 0x08062614): compra -> gMoney -= total, gInventory += qtd; venda ->
   gMoney += total, gInventory -= qtd. [provavel, lido em IN.md]
7. Saida: NAO rastreada (qual estado de Shop_* chama fim de cena e MainState). PENDENTE.
Outras funcoes da tela (ja nomeadas por outros): Shop_RefreshPartListScreen 0x08061CE0, Shop_FadeInStep 0x08062970,
Shop_UpdateSlotAndDispatchState 0x0805CF58 (provavel: tela da bolsa, nao da loja de pecas).

## Passo 4: uso de key items no menu de campo
- NAO ACHADO como uso no campo. O unico caminho de uso e KeyItemBag_ListInputStep (passo 2): A -> FUN_08048960(id),
  e o despacho por id. Como FUN_08048960 foi lido em IN.md como classificador de estado de batalha (0..4), o uso
  real de key item continua sem confirmacao. PENDENTE.
- A tela da bolsa (KeyItemBag_InitListScreen) e alcancada so pela tabela 0x08483634 (indice 2), que e despachada por
  0x0805CB50 a partir da entrada 0 da tabela 0x084835A4 (ROM aponta so para 0x08483594 + 0x10, no codigo perto de
  0x0805BC54, funcao sem nome no Ghidra). Ponto de entrada do menu de campo NAO identificado. PENDENTE.

## Pendente (resumo)
- Saida da loja de pecas (passo 3.7) e a funcao 0x08047AE4 (escreve o id da loja).
- Efeito por id da tabela 0x0805D2C8 (miolo de Shop_ItemListInputStep, 0x0805D36C..0x0805D410).
- Chamador da base 0x08483594 (campo/menu) e uso real de key item.
- FUN_08048960 (conferir: foi lido como classificador de batalha em IN.md; nao reconfirmado aqui).
