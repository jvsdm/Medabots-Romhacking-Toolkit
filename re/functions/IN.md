# GH-IN: itens, loja, inventario, dinheiro (Medabots Metabee EU)

Status: EM ANDAMENTO (2026-10-14). Salvo a cada passo.

## Confirmado

## Pendente
- Inventario de itens (Map64): 0x03002BA0, 64 pares [b0=0x80|id (bit7 = usado), b1=qtd]. Compactado: ao zerar, desloca
  (Mem_SwapBytes). Funcoes: Map64_GetValue 0x08042278 (qtd ou 0), Map64_AddCountClamp99 0x080422B8 (soma, teto 99;
  usa `byte`, o teste `< 100` pode estourar em qtd alta), Map64_SubRemoveIfZero 0x08042318 (subtrai; remove se <=0).
  Wrappers: Inventory_GetItemCount 0x08047D8C, Map64_AddCountDefault1_Wrapper 0x08047DC0 (qtd 0 vira 1),
  Map64_SubDefault1_Wrapper 0x08047DD8. [confirmado Ghidra]
- Dinheiro: u32 em 0x03002B70 (Save_GetMoney 0x08047D4C). Soma com teto AddClampedCounter 0x08047D58 (teto = 0x1869F
  = 99999); subtracao com piso 0 SubClampedCounter 0x08047D74. [confirmado]
- Evento 0x36 GetItem (args: id, qtd): setup 0x0804C648; continuacao 0x0804E928: se qtd atual == 99 -> msg "cheio"
  (SFX 6, texto da tabela 0x080480E0[id]); senao soma min(qtd, 99-atual), SFX 0xE, texto + numero. [confirmado]
- Evento 0x37 LoseItem: continuacao 0x0804EA18: se 0 -> msg (SFX 6) e estado 1; senao subtrai min(qtd, atual), SFX 5;
  estado 3 se zerou, 2 se sobrou. [confirmado]
- Evento 0x3A GetMoney: arg 2 bytes (hi, lo) * 10 -> AddClampedCounter; SFX 7. 0x3B LoseMoney: arg*10, piso 0 (se
  dinheiro-arg*10 < 1 zera tudo), SFX 5. [confirmado]
- Evento 0x4B (0x0804CA28) = ITEM: args (id, qtd, salto). Salta se contagem == 0 OU contagem < qtd. [confirmado]
- Evento 0x4C (0x0804CAD8) = PECA (leg) e nao item; ver 0x4E abaixo.
- Evento 0x4E (0x0804CB34): args (dinheiro/10 em 2 bytes, salto). Salta se dinheiro < arg*10. [confirmado]
- Evento 0x59 Open_Shop: 0x0804CD84 le 1 byte (id da loja) e marca flag; continuacao 0x0804F67C roda cena 3 (abre
  a loja) e termina quando a cena acaba. [confirmado]
Confirmado ate aqui: Map64, dinheiro, eventos 36/37/3A/3B/4B/4E/59. Proximo: xrefs de 0x03002BA0 e 0x03002B70, tela da loja, tabela de itens/nomes.
- Transacao de compra/venda de PECAS: Shop_CommitBuySellTransaction 0x08062614. Ao confirmar, soma (ou subtrai, se
  venda) gShopQuantities[i] ao inventario de pecas gInventory[bot*2 + tipo*0xF0] para cada uma das 0x78 entradas de
  gShopItems (bot = u16 em gShopItems+i*8; tipo = byte de estado +0x19). Dinheiro: gMoney -= total (estado +0x1C)
  quando compra; teto/cap 99999 (0x0806273C/0x080626B4 = 0x1869F?, confirmar). [provavel; decompile sem tipos]
- Lista de ITENS (Map64 -> gShopItems): Shop_BuildItemList64 0x0805AE4C percorre 64 pares do 0x03002BA0, copia os com
  bit 0x80 (id = &0x7F, qtd) para gShopItems e conta em estado+0x10. Usada por Shop_InitItemListScreen 0x0805CDAC
  (5 por pagina). [confirmado leitura do codigo]
- Shop_ItemListInputStep 0x0805CFB4: no A, chama FUN_08048960(id do item) e despacha por tabela indexada pelo id
  (ponteiro em 0x0805D2C8, so se id <= 0x28; senao MainState_RequestModeTransition(4)). Resultado 1 = usa o item
  (efeito por id), 2..4 = SFX de erro/feedback. [provavel: e a TELA DE USO DE ITEM (mochila), nao a compra]
- Shop_HandleItemListAndCounts 0x0800C804 (cursor de 3 contadores de 0..0x3F): nos botoes faz Map64_AddCountClamp99 /
  Map64_SubRemoveIfZero e, em modo "tudo", da 1 de cada item de 0 a 0x3E. Parece MENU DE DEBUG (dar itens), nao
  loja. [provavel; checar xrefs/UI antes de nomear]
- CORRECAO: o 0x03002BA0 (64 pares) parece ser a bolsa de KEY ITEMS: o texto gTextKeyItems (0x08483ED8, 64 ponteiros
  validos, lido na ROM) tem 64 entradas. Nome de item = indice no texto. Os eventos 0x36/0x37/0x4B usam esse id.
  [provavel: liga por contagem, nao por uso]. O codigo "Shop_BuildItemList64" tambem monta essa lista; pode ser
  a tela de itens-chave e nao loja (o nome Shop_* pode estar errado).
- FUN_08048960 (usado em Shop_ItemListInputStep) e um classificador de ESTADO DE ACAO DE BATALHA (0..4), nao uso
  de item. Logo o despacho da lista nao e efeito de item: efeito por id NAO encontrado nesta rodada.
- Compra/venda de pecas (UI): Shop_TickBuyQuantityMenu 0x08061F00 (0x08061F00 a 0x08062240).
  * total = soma(preco * qtd) sobre as 0x78 entradas; modo compra usa preco*qtd; modo venda (estado +5 != 0) usa
    metade (>>1). Teto do total 0x0806247C (= 99999 provavel).
  * compra so se total <= gMoney e (dono + qtd) < 99 (estoque 99 por peca).
  * Commit (0x08062614): compra -> gMoney -= total e inventario de pecas += qtd; venda -> gMoney += total e -= qtd.
  * Nao ha desconto no codigo lido: preco unico por peca (gShopItems), so o modo venda e 50%.
- Peca ganha em batalha: Fight_HandleRewardGrantOrSkipByPostBattleFlags 0x08010C50 chama Inv_AddCountClamp99
  (0x0804213C) em 0x08010CCA. Peca comprada/ganha vai para o inventario de pecas (0x03004C40, slot*0xF0 + bot*2).
- Peca em evento: 0x2E JumpIfNoSparePart (0x0804C3E4) e 0x7B (0x0804D438) testam Inv_ReadCounter16 (0x08042114).
- Item de uso em batalha/mapa: NAO ACHADO. Fight_TryUiActionByIndex 0x0801AC24 (chama 0x0803FB3C com item de tabela)
  e a unica pista; nao decodificado. PENDENTE.
Pendente: efeito de uso de item; tabela de precos de key items (provavel nao vende); confirmar Shop_* vs bolsa de key items; renomear Shop_BuildItemList64 so se confirmar.
