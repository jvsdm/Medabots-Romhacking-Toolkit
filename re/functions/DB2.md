# DB2: telas de debug restantes (Tinpet, Item, Face, Mdlpic, Rnd-tk, Battle, Sound, Motion ok em DB.md)

GH-DB2, 2026-10-14. Continuação de DB.md. Leitura de código (decompile + disassemble) e literais lidos da ROM EU
(python -I, só leitura). Nada validado no emulador. "confirmado" = decompile + literal; "provável" = inferência.

## Como o despachante acha a tela (comum a todas as opções 3..9)

Todos os despachantes têm o mesmo corpo (confirmado no disassemble):
    push {lr}; r1 = [pool tabela]; r0 = [pool ctx]; r0 += 0x33; r0 = byte; r0 = r1 + byte*4; r0 = [r0];
    bl 0x0807CCF0 (CallViaR0); pop; bx r0
- ctx = 0x03000440 (literal de todos); byte = `gDebugSubstate` 0x03000473 (ctx+0x33).
- Tabela = entrada do grupo na tabela 0x083ACAC0 (34 ponteiros, uma palavra por passo). Cada opção tem 3 slots
  (0,1,2): só o slot 0 é handler; slots 1 e 2 são NoOp (0x08000840/0x08000841). Ou seja, a tela é um só passo
  que roda por quadro até B (ou A sair), e o byte 0x03000473 fica em 0.
- Bases (literal da tabela): Tinpet 0x083ACAE4 (idx 9), Item 0x083ACAF0 (idx 12), Face 0x083ACAFC (idx 15),
  Mdlpic 0x083ACB08 (idx 18), Rnd-tk 0x083ACB14 (idx 21), Battle 0x083ACB20 (idx 24), Sound 0x083ACB2C (idx 27).
- Handler (state 0) de cada opção, pelo pointer da tabela 0x083ACAC0 (Thumb -1):
  Tinpet 0x0800C6F8, Item 0x0800C804, Face 0x0800CBC8, Mdlpic 0x0800CE90, Rnd-tk 0x0800D188, Battle 0x0800D27C,
  Sound 0x0800D3E4.
- Endereços dos despachantes e handler: pelo Ghidra, o corpo do handler termina pouco antes do despachante seguinte
  (ex.: Tinpet C6F8..C7E1, despachante Item em C7E4). Os nomes do sync nesses endereços (PartsBuild_UpdateStatusPanel,
  Shop_HandleItemListAndCounts, Menu_UpdateDetailPage, Menu_InfoPanelStep, Hud_DrawLabelAndDecimal6,
  PartsBuild_UpdateEditPanel) são ERRADOS para estas telas de debug (ver as seções).

## Padrão de tela (todas as opções de DB2)

- Linha: cursor 0x03000474 (ctx+0x34); valores 0x03000475 + cursor (ctx+0x35..).
- Nome: 8 bytes apontados por literal da opção, copiados para 0x030004DE, piscando por gFrameCount (0x03001780) bit 2
  (`0x04`). Texto com Text_LayoutGlyphSprites.
- Número: `Num_FormatDecimal6Glyphs(0x030004DE, valor)` (0x08041370), 6 dígitos.
- Entrada: just_pressed 0x0300058C: A = 0x01, B = 0x02. Repeat de Dir/Esq: 0x03001798 (Dir 0x10, Esq 0x20).
- B: `ClearObjectAttributeEntries(...)` (limpa OAM) e grava gamestate_subroutine 0x0300137C = 1 (volta aos controles).

## Tinpet (0x0800C6D8 despachante -> 0x0800C6F8 handler) — confirmado

- Mostra: nome "Tinpet" (texto em 0x083ACC40) e UM número: a linha 0 (0x03000475), 6 dígitos.
- Teclas:
  - Esq/Dir (repeat 0x03001798 & 0x30): inverte o valor da linha do cursor (`valor ^= 1`). Ou seja o valor é 0 ou 1.
  - A: se `Tinpet_IsPoolFull()` (0x08042218) == 0, chama `Tinpet_CreateRecord(valor_linha0)` (0x08042238).
  - B: limpa OAM (intervalo 8..0x21) e grava 0x0300137C = 1.
- Variáveis: 0x03000475 (valor, linha 0), 0x03000474 (cursor, lido a cada quadro), 0x03000440.
- Efeito: `Tinpet_CreateRecord(v)` acha o primeiro dos 9 slots de 8 bytes em 0x030017A0 com byte 0 == 0 e grava:
  byte0 = 1 (em uso), byte1 = v&1, byte2 = 0x1E (30), bytes3..6 = 0x78 + (v&1). Ou seja: cria um Tinpet de id 0x78 ou
  0x79, com 30 de valor no byte 2 (provável: nível/contagem; ver a pesquisa de Tinpet em PS.md).
- Chama: Tinpet_IsPoolFull (0x08042218), Tinpet_CreateRecord (0x08042238), Num_FormatDecimal6Glyphs (0x08041370),
  ClearObjectAttributeEntries, Text_LayoutGlyphSprites. Chamada por: 0x0800C6D8 (CallViaR0 slot 9 de 0x083ACAC0).
- Confiança: confirmado (decompile + literais 0x030017A0, 0x03001798, 0x0300058C, 0x0300137C, 0x03001780).
- Achado de risco (provável): a tela só mexe na linha `cursor`; se o cursor vier de outra tela com valor 1 ou 2,
  Esq/Dir muda uma linha que não aparece, e A cria Tinpet com o valor da linha 0 (que pode não ser o que mudou).
  Confirmar no emulador: entrar em Tinpet com cursor 0 e com cursor 2.
- Nome Ghidra: 0x0800C6F8 renomeado para Debug_TinpetScreen (era PartsBuild_UpdateStatusPanel, errado).

## Item (0x0800C7E4 despachante -> 0x0800C804 handler) — confirmado

- Despachante: tabela 0x083ACAF0 (idx 12), byte 0x03000473; slot 0 = 0x0800C804; slots 1..2 NoOp.
- Mostra: nome da opção ("Item"), 3 linhas, cada uma com um rótulo (ponteiros 0x083ACC44, 0x083ACC4C, 0x083ACC54,
  via tabela 0x083ACC5C) e um número de 6 dígitos (linha 0 = 0x03000475, linha 1 = 0x03000476, linha 2 = 0x03000477).
  O rótulo da linha do cursor pisca; o cursor 0x03000474 é lido a cada quadro e vai de 0 a 2.
- Teclas:
  - Cima (0x40) / Baixo (0x80) em 0x0300058C: cursor -1 (0 -> 2) / +1 (2 -> 0).
  - Dir (0x10) / Esq (0x20) com repeat 0x03001798: valor da linha do cursor +1 (62 -> 0) / -1 (0 -> 62). Faixa 0..0x3E.
  - A (0x01): linha 0 -> `Map64_AddCountClamp99(v0, 1)` se v0 < 0x40 (0x080422B8). Linha 1 -> `Map64_SubRemoveIfZero(v1, 1)`
    se v1 < 0x40 (0x08042318). Linha 2 -> zera 0x03000477 e faz +1 em TODOS os ids 0..0x3E (63 itens, laço por
    `Map64_AddCountClamp99`), deixando o valor em 0x3F.
  - B (0x02): limpa OAM (8..0x21) e grava 0x0300137C = 1.
- Variáveis: 0x03000474 (cursor), 0x03000475..0x03000477 (valores), 0x03002BA0 (gItemInventory, 64 pares
  [0x80|id, quantidade] lidos e escritos pelas funções Map64_*), 0x03000440 (ctx), 0x0300058C (just pressed).
- Efeito: concede/tira itens do inventário de itens (Map64). Os pares (id, qtd) ficam em 0x03002BA0; cada soma
  é limitada a 99 (`Map64_AddCountClamp99`). Subtração que zera remove o par e desloca os seguintes.
- Chama: Map64_AddCountClamp99 (0x080422B8), Map64_SubRemoveIfZero (0x08042318), Num_FormatDecimal6Glyphs (0x08041370),
  Mem_CopyBytes, Text_LayoutGlyphSprites, ClearObjectAttributeEntries. Chamada por: 0x0800C7E4 (CallViaR0).
- Atenção: o sync dava nomes errados a este handler ("Shop_HandleItemListAndCounts"); o despachante é o "Item".
  Para o Lua: tela de debug; a lógica de inventário (Map64) já está em InventoryGetItemCount/Map64_*.
- Achado (provável): a linha 2 (A) zera a própria linha antes do laço, então o mesmo A usado de novo começa de 0 outra
  vez; é "conceder 1 de cada item 0..62".
- Confirmar no emulador: A na linha 0 com id 5 soma 1 no par [0x85, n] em 0x03002BA0.
- Nomes Ghidra: 0x0800C804 renomeado para Debug_ItemInventoryScreen; 0x0800C7E4 para Debug_ItemDispatch.

## Face (0x0800CBA8 despachante -> 0x0800CBC8 handler, Ghidra "Menu_UpdateDetailPage") — confirmado

- Despachante: tabela 0x083ACAFC (idx 15), byte 0x03000473; slot 0 = 0x0800CBC8; slots 1..2 NoOp.
- Mostra: nome da opção ("Face") e 2 linhas: rótulos (ponteiros 0x083ACC68 e 0x083ACC70, via tabela 0x083ACC78) e
  2 números de 6 dígitos: linha 0 = 0x03000475 (id do retrato), linha 1 = 0x03000476 (variante). O rótulo da linha
  do cursor pisca (0x03001780 bit 2).
- Teclas:
  - Cima (0x40) / Baixo (0x80) em 0x0300058C: alterna o cursor 0x03000474 entre 0 e 1 (só 2 linhas).
  - Dir (0x10) / Esq (0x20) com repeat 0x03001798: na linha 0 o valor vai de 0 a 0xAF (175) e faz wrap;
    na linha 1 vai de 0 a 8 e faz wrap.
  - A (0x01): `LoadPortraitToVram_Unk(v0, v1, 0, 7, 1)` e `LoadPortraitToVram_Unk(v0, v1, 9, 7, 0)`: desenha o
    retrato v0 com a variante v1 em duas posições de fundo (colunas 0 e 9, linha 7).
  - B (0x02): `BgMap_FillRect16(0x11, 8, 0, 7, BG0CNT, 0)` (apaga o retângulo do mapa de fundo) e limpa OAM 8..0x21;
    grava 0x0300137C = 1.
- Variáveis: 0x03000474 (cursor), 0x03000475 / 0x03000476 (ids), 0x03000440 (ctx), 0x0300058C (just pressed),
  0x03001798 (repeat); REG_BG0CNT 0x04000008 (lido para o apagar); VRAM de retrato (pelo loader).
- Chama: LoadPortraitToVram_Unk (0x0803F480, provável retrato VRAM 0x06000F80), BgMap_FillRect16 (0x0803EF8C),
  Num_FormatDecimal6Glyphs, Text_LayoutGlyphSprites, ClearObjectAttributeEntries. Chamada por: 0x0800CBA8 (CallViaR0).
- Achado: o sync dava nome errado ("Menu_UpdateDetailPage"). Confirmado: é a tela Face.
- Para o Lua: tela de debug; o retrato em si é o que importa (ver a pesquisa de retratos em portraits, README).
- Confirmar no emulador: A com v0 = 0 e v1 = 0 e depois com v1 = 8 (a variante muda a paleta/quadro?).
- Nomes Ghidra: 0x0800CBC8 renomeado para Debug_FaceScreen; 0x0800CBA8 para Debug_FaceDispatch.

## Mdlpic (0x0800CE70 despachante -> 0x0800CE90 handler, Ghidra "Menu_InfoPanelStep") — confirmado

- Despachante: tabela 0x083ACB08 (idx 18), byte 0x03000473; slot 0 = 0x0800CE90; slots 1..2 NoOp.
- Mostra: nome da opção ("Mdlpic") e 2 linhas com rótulos (ponteiros 0x083ACC68 e 0x083ACC70, tabela 0x083ACC78,
  os mesmos rótulos da Face) e 2 números de 6 dígitos: linha 0 = 0x03000475 (modelo, 0..0x1D = 29 modelos), linha 1 =
  0x03000476 (variante, 0..4).
- Teclas:
  - Cima / Baixo (0x40 / 0x80, 0x0300058C): alterna o cursor 0x03000474 entre 0 e 1.
  - Dir / Esq (repeat 0x03001798): linha 0 faz wrap 0..0x1D; linha 1 faz wrap 0..4.
  - A (0x01): (1) DMA3 de paleta: registros 0x040000D4 (SAD = 0x084E4438), 0x040000D8 (DAD = 0x050003E0, OBJ pal 15),
    0x040000DC (CNT = 0x84000008, 8 palavras). (2) descompacta o gráfico do modelo: fonte = ponteiro
    `tabela[v0*5 + v1]` (tabela em 0x083AC614, 150 ponteiros para dados em ROM) para VRAM 0x06010000.
    (3) grava uma entrada de OAM na cópia de sprites 0x03002C30: attr0 = 0xC004, x = 0x40, y = 0x50, tile = 0x200,
    e bytes de paleta/tamanho (0xF e 0xC0). Ou seja: mostra a imagem do modelo como sprite grande.
  - B (0x02): limpa OAM (0..1 e 8..0x21) e grava 0x0300137C = 1.
- Variáveis: 0x03000474 (cursor), 0x03000475 / 0x03000476 (modelo / variante), 0x03000440 (ctx), 0x0300058C (just),
  0x03001798 (repeat), 0x03002C30 (cópia de OAM), VRAM OBJ 0x06010000, OBJ pal 15 (0x050003E0).
- Chama: decompress_and_store (ROM -> VRAM; nome do sync), Num_FormatDecimal6Glyphs, Text_LayoutGlyphSprites,
  ClearObjectAttributeEntries. Chamada por: 0x0800CE70 (CallViaR0).
- Achado: o sync dava nome errado ("Menu_InfoPanelStep"). Confirmado: é a tela Mdlpic.
- Para o Lua: não precisa (debug de imagem); o mapa de ids 0x083AC614 é útil se o gráfico de modelo for portado.
- Confirmar no emulador: A com modelo 0 variante 0 e com modelo 0x1D variante 4 (o último da tabela).
- Nomes Ghidra: 0x0800CE90 renomeado para Debug_MdlpicScreen; 0x0800CE70 para Debug_MdlpicDispatch.

## Rnd-tk (0x0800D168 despachante -> 0x0800D188 handler, Ghidra "Hud_DrawLabelAndDecimal6") — confirmado

- Despachante: tabela 0x083ACB14 (idx 21), byte 0x03000473; slot 0 = 0x0800D188; slots 1..2 NoOp.
- Mostra: nome da opção ("Rnd-tk", ponteiro 0x083ACCA0 -> 0x083ACC98) piscando (0x03001780 bit 2) e UM número de
  6 dígitos: o byte em 0x0300640C (base 0x030057E0 + 0xC2C).
- Teclas:
  - Dir / Esq (repeat 0x03001798 & 0x30): inverte o bit 0 do byte 0x0300640C (`^= 1`). Só 0 e 1 são alcançados
    a partir de 0 ou 1; se o valor inicial for outro, o XOR só mexe no bit 0.
  - A: nada (não há ramo para A).
  - B (0x02): limpa OAM (8..0x21) e grava 0x0300137C = 1.
- Variáveis: 0x0300640C (byte; é `gEncounterBlockFlags` em mtbe.csv, ver nota abaixo), 0x03001798, 0x0300058C,
  0x0300137C, 0x03001780. Não usa cursor 0x03000474 nem as linhas 0x03000475..77.
- Efeito (provável): é a chave de encontro aleatório (Rnd = random): o bit 0 de 0x0300640C liga/desliga os
  encontros aleatórios (ou o modo de encontro). Confirmar no emulador: mudar o bit e andar no mapa, ver se aparece
  batalha; e comparar com o significado de gEncounterBlockFlags (mtbe.csv: "antes gRandomEncounterModeFlags").
- Chama: Num_FormatDecimal6Glyphs, Text_LayoutGlyphSprites, ClearObjectAttributeEntries, Mem_CopyBytes.
  Chamada por: 0x0800D168 (CallViaR0 slot 21).
- Achado: o sync dava nome errado ("Hud_DrawLabelAndDecimal6").
- Para o Lua: precisa ser portado só se a lógica de encontro aleatório for portada (é um byte de estado, sem hardware).
- Nomes Ghidra: 0x0800D188 renomeado para Debug_RndTkScreen; 0x0800D168 para Debug_RndTkDispatch.

## Battle (0x0800D25C despachante -> 0x0800D27C handler, Ghidra "Menu_TickSingleCountSelector") — confirmado

- Despachante: tabela 0x083ACB20 (idx 24), byte 0x03000473; slot 0 = 0x0800D27C; slots 1..2 NoOp.
- Mostra: nome "Battle" (ponteiro 0x083ACCAC -> 0x083ACCA4, piscando por 0x03001780 bit 2) e UM número de 6 dígitos:
  o valor da linha do cursor, `0x03000475 + cursor`.
- Teclas:
  - Dir (0x10) / Esq (0x20), com repeat 0x03001798: soma +1 / -1 ao valor da linha do cursor, SEM wrap (é um byte,
    passa de 0xFF para 0 e de 0 para 0xFF).
  - A (0x01): limpa OAM (8..0x21) e dispara uma batalha/encontro: escreve na RAM do bloco E e grava gamestate_menu = 6:
    - 0x0300640D = 1; 0x0300640E = 0; 0x0300640F = 0;
    - 0x0300640A = valor da linha do cursor (o id que a batalha usa);
    - 0x03002B8C (gamestate_menu) = 6 (menu 6 = encontro, NX1.md / BT2.md);
    - 0x0300137C (gamestate_subroutine) = 0.
    Ou seja, A sai do debug e entra no menu 6 (encontro) com esse id. Não volta ao menu de debug sozinho.
  - B (0x02): limpa OAM (8..0x21) e grava 0x0300137C = 1 (volta aos controles).
- Variáveis: 0x03000474 (cursor, só lido: esta tela não o altera), 0x03000475..77 (valores), 0x03000440 (ctx),
  0x030057E0 + 0xC2C..0xC2F = 0x0300640A..0x0300640F (bloco E de encontro; 0x0300640C é o byte de Rnd-tk),
  0x03002B8C (gamestate_menu), 0x0300137C, 0x0300058C, 0x03001798.
- Chama: Num_FormatDecimal6Glyphs, Text_LayoutGlyphSprites, ClearObjectAttributeEntries, Mem_CopyBytes.
  Chamada por: 0x0800D25C (CallViaR0 slot 24).
- Achado (provável): a tela escreve no mesmo bloco E que Rnd-tk liga/desliga (0x0300640C). O valor 0x0300640A é o
  id de encontro, e 0x0300640D = 1 parece a flag de "encontro por debug". Confirmar no emulador: escolher o valor X,
  A, e ver a batalha e o byte 0x0300640A = X.
- Achado: o sync dava nome errado ("Menu_TickSingleCountSelector"). Confirmado: é a tela Battle. A função
  Menu_TickSingleCountSelector (nome do sync) era usada como se fosse um seletor de contagem; não é.
- Para o Lua: não precisa (é ferramenta de debug que lança o encontro).
- Nomes Ghidra: 0x0800D27C renomeado para Debug_BattleScreen; 0x0800D25C para Debug_BattleDispatch.

## Sound (0x0800D3C4 despachante -> 0x0800D3E4 handler, Ghidra "PartsBuild_UpdateEditPanel") — confirmado

- Despachante: tabela 0x083ACB2C (idx 27), byte 0x03000473; slot 0 = 0x0800D3E4; slots 1..2 NoOp.
- Mostra: nome da opção (ponteiros 0x083ACCB0 e 0x083ACCB8 para os rótulos das 2 linhas, tabela 0x083ACCC0), e 2
  números de 6 dígitos: linha 0 = 0x03000475 (efeito sonoro, 0..0x7F = 127), linha 1 = 0x03000476 (música, 0..0x35 = 53).
  O rótulo da linha do cursor pisca (0x03001780 bit 2).
- Teclas:
  - Cima (0x40) / Baixo (0x80) em 0x0300058C: alterna o cursor 0x03000474 entre 0 e 1.
  - Dir / Esq (repeat 0x03001798 & 0x10 / 0x20): na linha 0, valor 0..0x7F com wrap; na linha 1, 0..0x35 com wrap.
  - A (0x01):
    - cursor 0: `snd_sfx_request(v0)` (0x08074DE0): toca o efeito v0.
    - cursor 1: `snd_song_request(v1)` (0x08074B88): troca para a música v1.
  - B (0x02): `snd_song_request(0)` (para a música), limpa OAM (8..0x21) e grava 0x0300137C = 1.
- Variáveis: 0x03000474 (cursor), 0x03000475 (sfx), 0x03000476 (música), 0x03000440 (ctx), 0x0300058C, 0x03001798,
  0x03001780, 0x0300137C.
- Chama: snd_sfx_request (0x08074DE0), snd_song_request (0x08074B88), Num_FormatDecimal6Glyphs, Text_LayoutGlyphSprites,
  ClearObjectAttributeEntries, Mem_CopyBytes. Chamada por: 0x0800D3C4 (CallViaR0 slot 27).
- Achado: o sync dava nome errado ("PartsBuild_UpdateEditPanel"). Confirmado: é a tela Sound.
- Para o Lua: não precisa se o som não for portado (é o driver de áudio, MS.md/SN2.md). Se for, esta tela dá a
  tabela de efeito/música de teste (0..0x7F e 0..0x35).
- Confirmar no emulador: A no cursor 0 com v0 = 0 e com v0 = 0x0D (SFX 0x0D aparece em Battle_TickTwoStageSequence);
  A no cursor 1 com v1 = 8 (snd_song_request(8) aparece em Screen_InitOverlayAndLoadAssets 0x0803D74C; provável música de tela).
- Nomes Ghidra: 0x0800D3E4 renomeado para Debug_SoundScreen; 0x0800D3C4 para Debug_SoundDispatch.

## Resumo das 7 telas (e o que testar no emulador depois)

| Opção (ordem do menu) | Despachante | Handler (tela) | Linhas e faixa | A faz | Variáveis RAM | Chama |
|---|---|---|---|---|---|---|
| 3 Tinpet | 0x0800C6D8 | 0x0800C6F8 | 1 (0/1) | Tinpet_CreateRecord(v) | 0x03000475, 0x030017A0 (pool 9x8) | Tinpet_CreateRecord 0x08042238 |
| 4 Item | 0x0800C7E4 | 0x0800C804 | 3 (0..62; 0..62; 0..62) | Map64 +1 / -1 / +1 em 0..62 | 0x03002BA0 (gItemInventory) | Map64_AddCountClamp99 0x080422B8, Map64_SubRemoveIfZero 0x08042318 |
| 5 Face | 0x0800CBA8 | 0x0800CBC8 | 2 (0..0xAF; 0..8) | desenha retrato 2x | 0x03000475/76, VRAM | LoadPortraitToVram_Unk 0x0803F480 |
| 6 Mdlpic | 0x0800CE70 | 0x0800CE90 | 2 (0..0x1D; 0..4) | DMA paleta + gráfico do modelo + OAM | 0x03000475/76, 0x03002C30, VRAM 0x06010000 | decompress_and_store 0x080009BC |
| 7 Rnd-tk | 0x0800D168 | 0x0800D188 | (byte 0x0300640C) | nada | 0x0300640C | Num_FormatDecimal6Glyphs |
| 8 Battle | 0x0800D25C | 0x0800D27C | 1 (cursor) | grava 0x0300640A..0F, gamestate_menu=6 | 0x0300640A..0F, 0x03002B8C | (escrita direta) |
| 9 Sound | 0x0800D3C4 | 0x0800D3E4 | 2 (0..0x7F; 0..0x35) | sfx ou musica | 0x03000475/76 | snd_sfx_request 0x08074DE0, snd_song_request 0x08074B88 |

Todas as telas usam: gDebugSubstate 0x03000473 (byte do despachante), cursor 0x03000474, ctx 0x03000440, just pressed
0x0300058C, repeat 0x03001798, e saem por B com gamestate_subroutine 0x0300137C = 1.

## Como testar no emulador (próxima rodada; não feito aqui)

- Entrar no menu de debug por debug_poke (DB.md), abrir cada opção pelo índice 3..9 em 0x03000472, e comparar o
  que aparece com a tabela acima.
- Battle: escolher um valor X na linha, A, ver se a batalha começa e se 0x0300640A = X. Repetir com 0x0300640C
  invertido (Rnd-tk) antes, para ver se muda a chance de encontro.
- Item: A na linha 0 com id 5 e conferir o par [0x85, n] em 0x03002BA0; depois A na linha 1 e ver a remoção.
- Tinpet: A cria registro em 0x030017A0 (byte0 = 1, byte2 = 0x1E, bytes 3..6 = 0x78 ou 0x79).
- Mdlpic / Face: A com v0 = 0 e com o último id; ver se a imagem muda.
- Sound: A no cursor 0 com v0 = 0x0D; no cursor 1 com v1 = 8.
- Ponto em aberto (provável): a tela Tinpet e a Battle não mudam o cursor. Se o cursor vier de outra tela com 1 ou 2,
  a edição cai numa linha que não aparece. Confirmar com cursor 0 e cursor 2 antes de entrar na tela.

## Pendente

- Nada de Ghidra ficou sem função nas 7 telas. Os nomes do sync (PartsBuild_*, Shop_*, Menu_*, Hud_*, Gfx_*) estavam
  errados para estas telas e foram trocados pelos nomes Debug_*.
- Entradas RAM 0x0300640A, 0x0300640D e os nomes em DB2_simbolos.csv são "provável", não confirmados no emulador.
- O despachante do Item/Mdlpic/etc. guarda o byte 0x03000473 mesmo quando a opção não é a atual; ver se o estado volta
  a 0 ao sair (não lido nesta rodada).
