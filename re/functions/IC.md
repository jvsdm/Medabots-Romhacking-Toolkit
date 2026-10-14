# IC: ícones de peça/medalha e descritores do sprite grande de peça

Status: EM ANDAMENTO (2026-10-14, agente GH-IC). Só leitura da ROM original e Ghidra (decompile/xrefs). Nenhuma imagem extraída.
Ponto de partida: GF4.md (seção "Peça grande" e "Ícones de peça / medalha").

## Mapa inicial (já lido em outros arquivos)
- `Menu_DrawPartIconGrid` 0x0800508C, `PartSlot_DrawNameLabel*` 0x08005C10..0x080060D0, `Parts_DrawIconsRow` 0x08006414,
  `Parts_BuildSlotIcons` 0x0800712C, `PartsGrid_DrawSlotIconRow` 0x0800AB0C, `PartsGrid_DrawPartPreview` 0x0800AF0C
  (símbolos de R0, marcados "provável").
- Ícone de perna: `PartDetail_GetCurrentSelectionLegIconIndex` 0x0800F6CC; `FightPreview_CachePlayerLoadoutForScene` 0x080048D8
  guarda o índice de ícone de perna no cache de preview.
- Medalhas: `gMedals` 0x083B62D8 (16 B por registro; GF4/MB6: +0x05 = Lv do registro do save, não da ROM).
- Tiles de ícone de medalha (tela BUILD): medalha = RAW 0x084C2E58 (128 B) para o Kuwagata; outras espécies não achadas (MB7).
- Tipo de perna 100..106 -> tiles 768+4*(tipo-100), 2x2, banco 8/9 (MB4, bandeira do pod).

## Pendências desta passada (ver seções abaixo quando preenchidas)
1. Ícones de peça/medalha: função, origem dos tiles, índice.
2. Descritores do sprite grande (0x0807EE70/0x0807F610/0x0807FDB0/0x08080550).
3. Destino de 0x08785EC0.

## Passo 1: ícone de peça (tela MEDABOTS/BUILD) - CONFIRMADO no decompile
- `Menu_DrawPartIconGrid` 0x0800508C (corpo 0800508C-080050E?): laço de 9 células (grade 3x3 de bots). Por célula: byte +0 do
  registro de 8 B (vetor em RAM, `DAT_080050d8`) = 0 -> ícone vazio (id 0xFF); senão `bot = byte+6` (id do bot, 0..0x77;
  >= 0x78 -> vazio) e chama `GetPartStats(bot, 3, buf)` (kind 3 = PERNAS). O índice do ícone = `buf[+1] - 100`
  (campo +1 do registro de perna = tipo 100..106; 0xFF = placeholder). Depois `DrawPartIconTileBlock(idx, x, y, pal, 0x8300)`.
- `GetPartStats` 0x08042B58: copia 16 B de `gPartStats` 0x083B83FC + bot*0x40 + kind*0x10 (kind 0 cabeça, 1 braço D, 2 braço E, 3 perna).
- `DrawPartIconTileBlock` 0x080435B0: desenha bloco 2x2 de mapa BG (ver passo 2 abaixo).
- Tipo 100..106 -> tiles BG 768+4*(tipo-100) (MB4 do pc/research, confirmado no print).
- Chamadores de Menu_DrawPartIconGrid: a confirmar (xrefs).

## Passo 2: escrita do ícone no mapa BG (CONFIRMADO no decompile)
- `DrawPartIconTileBlock` 0x080435B0 (corpo 080435B0-...): `DrawPartIconTileBlock(idx, x, y, pal_word, base)`. Monta
  bloco 2x2 de entradas de tilemap com `FillBgTilemapRectSequential(2,2,x,y,...)`. Entrada = base + idx*4 (+0x1000 se idx>=4).
  Chamadores: `Menu_DrawPartIconGrid` 0x0800508C (base 0x8300) e `Overlay_DrawThreeTileBlocks` 0x0803E83C (chute) etc.
- Base 0x8300 = tilemap com paleta 8 e tile 0x300 (= 768). idx 0..3 -> tiles 768..783 (paleta 8, banco 8);
  idx 4..6 -> +0x1000 = paleta 9 e tiles 784..795 (banco 9). Bate com MB4 (tipo 100..106 -> 768+4*(tipo-100)).
- idx 0xFF (vazio/placeholder): tilemap = 0x8300 + literal 0x080435D0 (= 0x101C) = 0x931C -> tile 796, paleta 9.
- Os tiles (bancos 8/9) são carregados de 0x084F3C20 + 32*b (MB4, GF4): não são extraídos aqui.

## Passo 3: ícone de medalha (PARCIAL)
- Os 128 B de 0x084C2E58 (ícone de medalha do Kuwagata, MB7) são IGUAIS ao bloco k=9 da tabela de 128 B que começa em
  0x084C29D8 (= 0x084C29D8 + 9*0x80 = 0x084C2E58; comparação byte a byte na ROM). A mesma tabela aparece em
  `Fight_LoadActiveBotIconTiles` 0x08017D50 (HD2): CpuFastSet de 0x20 words de 0x084C29D8+(bot+1)*0x80 para VRAM 0x06007800.
- Não há ponteiro literal para 0x084C2E58 no pool: o endereço é calculado. Hipótese (NÃO confirmada): medalha m -> 0x084C29D8 + (m+9)*0x80.
  Faltam: função da tela BUILD que escreve os tiles 960..963 e o índice real. Pendente.

## Passo 4: destino de 0x08785EC0 (CONFIRMADO no disasm de BattlePanel_SceneInit 0x08062FB4)
- Literais do pool: 0x08063188..0x08063204 lidos na ROM. Em 0x08063058: `r0 = [0x080631C0] = 0x08785EC0` (fonte) e
  `r1 = r5 = r4 + r5`, onde r4 = retorno de `Asset_UnpackOrCopyBlock` (0x08044D0C) para 0x0879DA90 e r5 = [0x080631BC] = 0x06004000.
  Então `decompress_and_store(0x08785EC0, 0x06004000 + tamanho(bloco 0x0879DA90))`. Antes disso, o bloco 0x0879DA90 é copiado
  (CpuSet 0x0807CCC4) para VRAM 0x06004000.
- Com tamanho 0x2FE0 (GF4, bloco 0x0879DA90): destino provável **VRAM 0x06006FE0** (0x06004000 + 0x2FE0 = 191 tiles de 32 B).
  Confiança: provável (o valor de r4 não foi medido; confirmar no emulador com watch em VRAM 0x06006FE0).
- Malias 0x08785EC0 = BG 4bpp, 0x03E0 B (31 tiles). Mesmo destino também chega a 0x08785FB4 (Malias 0x3A80 -> VRAM 0x06010000, OBJ).
- Note: 0x08785EC0 aparece em 0x08063058 (painel) e no número de revelação (GF4): mesma sequência de tiles após 0x0879DA90.

## Passo 5: descritores compostos do sprite grande (CONFIRMADO no decompile + leitura da ROM)
Chain: `PartPreview_LoadLargeDisplaySprite` 0x08044934 -> `LoadPartLargeDisplaySpriteFromAppearanceTable` 0x08046A60 ->
`PartPreviewPiece` 0x08045E6C (recursivo, para filhos +0x0F/+0x10) -> `BuildCompositePreviewOamTemplate` 0x08045D34 (OAM);
`CopyCompositePreviewDescriptorGraphics` 0x08046034 para tamanhos 1 e 2 (pós-carga do gráfico).

**Tabela (por id de peça, índice = param_3 * 0x10, ou * 0x20 no tamanho 3):**
| Tamanho (param_4) | Tabela | Entrada | Entradas | Largura (local_28) |
|---|---|---|---|---|
| 0 (cabeça) | 0x0807EE70 | 0x10 | 122 | 0x140 |
| 1 (braço D) | 0x0807F610 | 0x10 | 122 | 0x1A0 |
| 2 (braço E) | 0x0807FDB0 | 0x10 | 122 | 0x1E0 |
| 3 (pernas) | 0x08080550 | 0x20 | 122 | 0x220 |
- Histograma de palavras não nulas por entrada: Sz0 112x2 + 10x1; Sz1 110x2 + 12x1; Sz2 109x2 + 13x1; Sz3 54x5 + 62x1 + 6x4.
  Ou seja: cada peça usa 1-2 "grupos" (cabeça/braço) e 1-5 grupos (pernas). Entrada 0 (id 0) já tem dados.

**Palavra de descritor u32 (lida por PartPreviewPiece):**
- bits 0-5: `kind` (1..63). Seleciona a "struct de tipo" em `param_1[kind]` (ponteiro global; endereço não resolvido aqui).
  Kinds vistos: cabeça 7/8 (+9..13 raros); braço D 17/18 (+19); braço E 14/15 (+16); pernas 20..39 (30 = o mais comum).
  Cadeia de filhos: a struct tem os bytes +0x0F e +0x10 com sub-kinds, renderizados recursivamente.
- bits 6-12: `index` (7 bits, 0..127) no vetor de registros de 8 B da struct de tipo (+0): word0 = ponteiro do bloco de gráfico
  (descomprimido por `Asset_UnpackOrCopyBlock` 0x08044D0C, tiles para buffer 0x02030000 e depois VRAM); word1 = destino
  RAM de paleta (CpuSet 16 words); 0 = sem paleta.
- bit 13: não usado no código lido.
- bit 14 (0x4000): flag copiada para o byte +1 do OAM (`& 0xFC | flag`), igual ao caminho "affine/centered" do aviso do Toolkit.
- bit 15: soma 1 ao índice quando o parâmetro `param_7` (lado/espelho) tem bit 0 setado: `idx = ((w>>6) + (param_7 & (w>>15))) & 0x7f`.
- Terminador: palavra com bits 0-5 = 0 (interrompe o laço). Entradas 0x10 têm no máximo 4 palavras, 0x20 têm até 8.

**Campos do OAM (BuildCompositePreviewOamTemplate, lidos de cada struct de tipo):**
- +0x0C / +0x0D: offset X / Y do tipo (em px, multiplicado por 0x100 no cálculo da posição).
- +0x11: banco de paleta base; paleta OBJ = (banco + param_4) (param_4 = lado/offset de tela). No DMA de paleta: (+0x11 + param_4) * 0x20.
- +0x12 / +0x13: largura / altura em px. +0x14: nibble baixo = divisor da largura, nibble alto = divisor da altura (se > 1).
- +0x15: flags de forma/espelho: bit 0 = 4bpp/tamanho de tile dobrado; bits de attr1 derivados de (f&7, f&0x1F, f&1<<5).
- Tile do OBJ: `param_4 & 0x3FF` (contador acumulado em *param_6 por tiles = (w/8)*(h/8)).
- Forma/tamanho do OBJ: `SetCompositePreviewOamShapeAndSize` (não aberto nesta passada).

**Como o id escolhe a entrada:** `entrada = tabela + id_da_peça * 0x10` (ou * 0x20 para pernas). O id vem de `param_3`
(chamador `PartPreview_LoadLargeDisplaySprite` recebe o id da peça/bot). Tamanho de cada grupo: cabeça/braços/pernas = `param_4` 0..3.

## Passo 6: pendências do formato (não fechadas nesta passada)
- Endereço do vetor de struct de tipo (`param_1[kind]`, ponteiro de 64 entradas) e das structs: não resolvido.
- Vetores de registros de 8 B por tipo (word0 = gráfico): endereços não resolvidos; sem isso o endereço ROM exato de cada tile da peça grande fica aberto.
- `SetCompositePreviewOamShapeAndSize`: forma/tamanho exatos (não aberto).
- Cabeçalhos de paleta e bank de cada struct de tipo: não extraídos.

## Pendente (resumo)
1. Ícone de medalha: função que escreve tiles 960..963 da tela BUILD e índice exato (hipótese m+9 em 0x084C29D8 não confirmada).
2. Struct de tipo e vetores de gráfico do sprite grande (ver passo 6).
3. Confirmar no emulador: VRAM 0x06006FE0 (destino de 0x08785EC0, valor de r4).
