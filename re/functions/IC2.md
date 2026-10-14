# IC2: continuação de IC (struct de tipo do sprite grande, ícone de medalha, ícone de perna)

Status: EM ANDAMENTO (2026-10-14, agente GH-IC2). Só leitura da ROM original e Ghidra. Nenhuma imagem extraída.
Base: `IC.md` (passos 1-5) e `IC_simbolos.csv`.

## Passo 0: início
- Nada fechado ainda. Alvos: (1) struct de tipo `param_1[kind]` e vetores de 8 B; (2) escrita dos tiles 960..963
  (medalha); (3) `PartDetail_GetCurrentSelectionLegIconIndex` 0x0800F6CC.

## Passo 1 (parcial): PartPreviewPiece 0x08045E6C e a cadeia (CONFIRMADO no decompile)
- Assinatura Ghidra: `PartPreviewPiece(int tipos, uint *lista, uint kind, uint lado, int oam, ushort *tiles, uint lado2)`.
  - `tipos` (param_1) = VETOR de ponteiros: `struct_tipo = *(int **)(tipos + kind*4)`. Esse é o "param_1[kind]".
  - `lista` (param_2) = lista de palavras u32 (descritor), terminada por palavra com bits 0-5 = 0.
  - `oam` (param_5) = base do buffer de OAM; a entrada do tipo = oam + kind*0x10.
  - `tiles` (param_6) = contador u16 de tiles (VRAM), cada tile 0x20 B.
- Dentro da struct de tipo `T = struct_tipo`:
  - `*T` (word 0) = ponteiro para o VETOR de registros de 8 B: registro i em `*T + i*8`:
    word0 (+0) = ponteiro do bloco de gráfico (descomprimido por `Asset_UnpackOrCopyBlock` para buffer DAT_08045fb0);
    word1 (+4) = ponteiro de paleta (16 cores, 0x10 halfwords; 0 = sem paleta).
  - `T+0x11` = banco de paleta base (byte). Paleta destino = (T+0x11 + lado) * 0x20 + DAT_08045f2c (e de novo + DAT_08045f30).
  - `T+0x0F` e `T+0x10` = sub-kinds filhos (renderizados recursivamente com a mesma `lista`, lado e tiles).
- Palavra da lista `w`: `kind = w & 0x3F`; `idx = ((w>>6) + (lado2 & (w & 0xFFFF)>>15)) & 0x7F`;
  flag = `(w & 0x7FFF) >> 14` (só se DAT_08045ee4+0x96 == 0); bit 0x4000 vai para o byte +1 do OAM.
- Quirk confirmado: a paleta só é enviada quando o casamento de `kind` é a PRIMEIRA palavra da lista (contador iVar11 == 0).
  Se o casamento não é a primeira, só o gráfico é copiado (sem paleta).
- Gráfico: `Asset_UnpackOrCopyBlock(word0, buffer)` devolve tamanho; `CpuSet(buffer, VRAM_OBJ + (*tiles)*0x20, (tam & 0x3FFFFF)>>1)`
  (VRAM_OBJ = DAT_08045fb4, a confirmar no literal). Depois `*tiles += BuildCompositePreviewOamTemplate(...)`.

## Passo 2: quem passa param_1 (CONFIRMADO no decompile, cadeia completa)
- `PartPreview_LoadLargeDisplaySprite` 0x08044934 (r0=bot/ctx, r1=parte, r2=tamanho..., param_6 = 100/200/300/400
  por caso 0..3) -> `LoadPartLargeDisplaySpriteFromAppearanceTable` 0x08046A60 (args: `addr`, ctx, parte, TAMANHO 0..3,
  x, y, param_7 (short), lado, flags). Flags: bit 0x2 = espelho (vira `lado2` = 0/1); bit 0x4 = desenha agora (senão só marca
  dirty em `ctx_estado+0x2C |= 1`); 0x8 = caminho recursivo `RenderCompositePreviewDescriptorRecursive`.
- Em 0x08046A60 o TAMANHO escolhe (ponteiro da lista de descritores e largura inicial de tiles):

| tamanho | lista (descritor) = pool + parte*0x10 (3 usa *0x20) | tile inicial (u16, = `local_28[0]`) |
|---|---|---|
| 0 cabeça | 0x0807EE70 (DAT_08046aac) | 0x140 = 320 |
| 1 braço D | 0x0807F610 (DAT_08046ac0) | 0x1A0 = 416 |
| 2 braço E | 0x0807FDB0 (DAT_08046ad4) | 0x1E0 = 480 |
| 3 perna | 0x08080550 (DAT_08046b10) | 0x220 = 544 |

  (Valores do pool conferidos na ROM: 0x0807EE70, 0x0807F610, 0x0807FDB0, 0x08080550. Correção ao IC.md: a coluna
  "largura (local_28)" ali é o TILE INICIAL, não largura.)
- Se espelho (flag 2): `tile inicial += 0x160`.
- Chamada final: `PartPreviewPiece(ctx+4, lista, kind_da_1a_palavra, lado_byte, ctx+8, &tile_inicial, lado2)`.
  Então `ctx+4` = VETOR de struct de tipo (= `param_1` de PartPreviewPiece) e `ctx+8` = base do OAM.
  Para tamanho 1 e 2 (e 3 em ordem), depois `CopyCompositePreviewDescriptorGraphics` 0x08046034 copia a palette/gráfico por palavra.

## Passo 3: struct de tipo `T` e vetor de registros (CONFIRMADO no decompile de PartPreviewPiece + BuildCompositePreviewOamTemplate)
- `T+0x00` (u32): ponteiro do VETOR de registros de 8 B: `reg_i = *(T+0) + i*8`.
  - `reg_i+0` (u32): ponteiro do bloco de gráfico (passado a `Asset_UnpackOrCopyBlock`). 0 = sem gráfico.
  - `reg_i+4` (u32): ponteiro da paleta (16 cores = 0x10 halfwords via CpuSet). 0 = sem paleta.
- `T+0x0F`, `T+0x10`: sub-kinds filhos (recursão). 0 = nenhum.
- `T+0x11` (u8): banco de paleta base. Banco efetivo = `T+0x11 + lado` (lado = byte do parâmetro).
- `T+0x12` (u8): largura em px. `T+0x13` (u8): altura em px.
- `T+0x14` (u8): nibble baixo = divisor da largura (se >1 divide), nibble alto = divisor da altura (se >1 divide).
- `T+0x15` (u8) = flags do OAM: bit0 = 8bpp (conta tiles dobrado); bits1-2 -> attr0 bits 8-9; bits3-4 -> attr0 bits
  10-11 (modo do OBJ); bit5 -> attr1 bit12 (flip H); bit6 -> attr1 bit13 (flip V). Ver tabela no fim.

## Passo 4: entrada OAM (BuildCompositePreviewOamTemplate 0x08045D34, decompile)
- Entrada = `oam + kind*0x10`, 8 bytes usados:
  - attr0 byte1 = `(f&7)>>1 | ((f&0x1F)>>3)<<2 | (f&1)<<5`; byte0 = 0 (y = 0 é ajustado pelo chamador).
  - attr1 byte3 = `(byte3 & 1) | ((f>>5)&1)<<4 | ((f>>6)&1)<<5` (flip H/V); x é preenchido pelo chamador.
  - attr2 (u16) = `(attr2 & DAT_08045e68) | (tile & 0x3FF)`; byte5 = `(byte5&3) | 8 | ((T+0x11 + lado) & 0xF)<<4` (banco de paleta).
  - attr3 (bytes 6-7) = 0.
- Tamanho: w = `T+0x12` (/ divisor), h = `T+0x13` (/ divisor), depois `SetCompositePreviewOamShapeAndSize(entrada, w, h)`.
- Retorno = tiles = `(w>>3)*(h>>3)`, dobrado se `f&1` (8bpp). Esse valor é somado a `*tile_inicial`.
- Quem põe x/y: `RenderCompositePreviewDescriptorRecursive` usa `(x_base - T+0xC)*0x100` e `(y_base - T+0xD)*0x100`
  (ou seja, +0x0C/+0x0D são deslocamentos X/Y do tipo, em px, subtraídos da posição-base).

## Passo 5: param_1[kind] = VETOR NA ROM 0x083D1D78 (CONFIRMADO)
- `CompositePreviewContext_Init` 0x08047414 grava o contexto 0x02026000: `+0` = estado (0x03005308 via 0x03002414),
  `+4` = **0x083D1D78** (literal de `FightPreview_InitCompositeBuffers` 0x080445F4 -> pool 0x080446B0), `+8` = OAM 0x020260D0 (+ slot*0x410).
  Ou seja: `param_1` de PartPreviewPiece = 0x083D1D78 (fixo, ROM). `struct_tipo[kind] = word(0x083D1D78 + kind*4)`.
- Só são lidos pelo código destes passos: T+0x00, T+0x0F, T+0x10, T+0x11, T+0x0C/0x0D (no caminho recursivo), T+0x12..0x15.
  Campos T+0x04 e T+0x08 não são lidos aqui (valores 0x0D/0x1A e 0x08/0x0F nos exemplos: não identificados).

## Passo 6: exemplos numéricos (LIDOS DA ROM com script `-I`, sem extrair imagem)
Ex. A: **cabeça, tamanho 0, parte 0** (lista 0x0807EE70). Palavras: `0x00004047` (kind 7, idx 1, flag 1) e `0x00004048` (kind 8, idx 1, flag 1).
- kind 7: T = 0x083D1970, vetor de registros 0x083CF5B0, reg[1] = 0x083CF5B8 -> gfx 0x08599EB8, pal 0x08599F3C.
  T: +0x0C/0D = (0,0); +0x10 = filho 8; +0x11 = banco 0; +0x12/13 = 16x16; +0x14 = 0x11 (sem divisor); +0x15 = 0x00.
- kind 8 (filho, por +0x10): T = 0x083D1988, vetor 0x083CF8F0, reg[1] = 0x083CF8F8 -> gfx 0x0859DE44; pal = 0x0859DE44 (IGUAL ao gfx, e não usada: a palavra 1 da lista não é a primeira). T: +0x0C/0D = (-13, -1); +0x12/13 = 32x32; +0x11 = 0; sem filho.
Ex. B: **perna, tamanho 3, parte 30** (lista 0x08080910 = 0x08080550 + 30*0x20). Palavras (4): `0x69` (kind 41, idx 1, flag 0), `0x6A` (42, idx 1, flag 1), `0x6B` (43, idx 1, flag 1), `0x6C` (44, idx 1, flag 1).
- kind 41: T = 0x083D1CA0, vetor 0x083D1810, reg[1] -> gfx 0x085D5564, pal 0x085D5634; T: (0,0), filho 42 (+0x10 = 0x2A), banco +0x11 = 3, 32x32.
- kind 42..44: T = 0x083D1CB8 / 0x083D1CD0 / 0x083D1CE8; cada uma 32x32, banco 3, filhos 43 / 44 / nenhum. kind 42 usa (+12, +1); kind 43 (+11, -7); kind 44 (-12, -13).
- Leitura: a perna grande é uma cadeia 41 -> 42 -> 43 -> 44 (filho por +0x10), todos no mesmo banco de paleta 3 (+ lado).

## Passo 7: destinos (CONFIRMADO no decompile e nos literais)
- Gráfico: `Asset_UnpackOrCopyBlock(reg+0, 0x02030800)` (buffer RAM); depois `CpuSet(0x02030800, 0x06010000 + tile*0x20, tam>>1 halfwords)`.
  O valor 0x06010000 é DAT_08045fb4 (OBJ VRAM). Tiles iniciais: cabeça 320 (0x06012800), braço D 416, braço E 480, perna 544 (0x06014400).
- Paleta (só se o casamento é a 1ª palavra da lista): `CpuSet(reg+4, 0x05000200 + (T+0x11+lado)*0x20, 0x10 halfwords)` (DAT_08045f2c = 0x05000200)
  e de novo para `0x02030400 + (T+0x11+lado)*0x20` (DAT_08045f30 = 0x02030400, cópia em RAM).
- Tamanho do tile: 4bpp = 32 B por tile; flag 8bpp (T+0x15 & 1) conta o dobro de tiles.

## Passo 8: forma e tamanho do OBJ (SetCompositePreviewOamShapeAndSize 0x08044DA8, decompile + simulação)
- Chamada por BuildCompositePreviewOamTemplate com (w = T+0x12 / divisor, h = T+0x13 / divisor).
- Tamanho: byte3 bits 6-7 (= attr1 bits 14-15) = índice de tamanho pelo lado MAIOR: 8->0, 16->1, 32->2, 64->3 (quadrado: bits 0/0x40/0x80/0xC0).
- Forma: byte1 bits 6-7 (= attr0 bits 14-15). Pelo decompile: w>h -> 0x80; w<h -> 0x40; w==h -> 0.
- Divergência (A CONFIRMAR): a tabela padrão do GBA (GBATEK) é shape 1 = horizontal (w>h), shape 2 = vertical (w<h).
  Pelo decompile, 64x32 sai shape=2 size=3 (padrão seria shape=1 size=3). Os quadrados batem.
  Único retângulo usado na tabela de tipos: **64x32** (tipos 13, 16, 19). Ver passo 9 (pendência).
- Varredura dos 63 tipos (script `-I` no scratchpad, só ROM): tamanhos 8x8 (2 tipos), 16x16 (13), 32x32 (26), 64x32 (3), 64x64 (1).
  Tipos 3..6 têm ponteiro de IWRAM (0x030052B0..0x030052F0): preenchidos em tempo de execução, não lidos aqui.
  Tipos 50..63 têm ponteiros que não são de struct (0x080814C4..): não usados pelas listas; não lidos.

## Passo 9: pendência de OAM
- Não verifiquei em que ponto o buffer 0x020260D0 vira hardware OAM (DMA). Se o DMA copia sem trocar bits, 64x32 sai
  transposto. Confirmar com `mgba_dump_oam` ou com o print de `oam_render.py`; sem emulador nesta passada.

## Passo 10: ícone de medalha / bot: quem escreve os tiles (CONFIRMADO no decompile + literais)
- Fórmula comum: `src = (byte k) * 0x80 + 0x084C29D8`, depois CpuFastSet de 0x20 words (128 B = 4 tiles 4bpp) para o destino.
- `PartsGrid_UploadSelectedTileB` 0x08009EA0 (decompile): `k = byte(0x03000BE0 + ctx[0x3D]*0x40 + 1)`; destino **0x06007800** = BG char tiles **960..963**;
  depois `FillBgTilemapRectSequential(2,2,2,3, ...)` (tilemap 0x63C0). Literais: 0x08009EDC = 0x03000BE0 (roster, stride 0x40),
  0x08009EE0 = 0x03000440 (ctx de menu), 0x08009EE4 = 0x084C29D8, 0x08009EE8 = 0x06007800.
- `BattlePreview_LoadEntryPalette` 0x0800522C (batalha, NÃO BUILD): registro de 8 B em 0x030017A0 + ctx[0x51]*8;
  se byte0 < 2 -> src fixo 0x084C38D8; senão `k = byte2` e destino **0x06006600**.
- Hipótese (m+9): ainda NÃO confirmada. Pelo código, o índice é um BYTE do roster 0x03000BE0 (+1, stride 0x40), não "m+9" direto.
  Falta: de onde vem esse byte para a MEDALHA (ver passo 11).

## Passo 11: escritor dos tiles 960..963 da tela BUILD (CONFIRMADO no decompile + literais; hipótese m+9 NÃO confirmada)
- `PartSlot_LoadIconPalette` 0x08005B44 (chamado por `Parts_CycleValueSlot` 0x08006980 e `InitializeBattleSceneScreen` 0x080042D4):
  `s = ctx[0x97]` (ctx = 0x03000440). Se `s < 0x1E (30)`: `k = byte(0x03000BE0 + s*0x40 + 1)`; senão `k = 0x1E` (fixo 0x084C38D8).
  `CpuFastSet(0x084C29D8 + k*0x80, 0x06007800, 0x20)` -> **tiles 960..963** (4 tiles = 128 B). Depois `FillBgTilemapRectSequential(2,2,0x10,9,...)`.
- `PartsGrid_UploadSelectedTileB` 0x08009EA0 (chamado por `PartsGrid_RedrawCellPanelB` 0x0800B476 e `PartsGrid_EnterScreenInit` 0x080088C2):
  mesma fórmula com `k = byte(0x03000BE0 + ctx[0x3D]*0x40 + 1)` -> 0x06007800.
- Fórmula do endereço: **src = 0x084C29D8 + k*0x80**, com **k = byte +1 do registro de 0x40 B do roster 0x03000BE0**. A hipótese "(m+9)*0x80" não é o que o código faz: o índice vem de um byte da RAM, não de um m fixo.
- Checagem de aritmética: 0x084C38D8 = 0x084C29D8 + 30*0x80, então k = 30 é o ícone de vazio/padrão. 0x084C2E58 = k 9 (já no IC.md).
- Medalha: a ligação "ícone de medalha = ícone de bot k = bot+1" é PROVÁVEL (o bloco 9 = bloco de bot+1 de `Fight_LoadActiveBotIconTiles`), NÃO confirmada: falta confirmar que roster+1 = bot+1 (escrita em 0x03000BE0 não lida aqui).
- Outra escrita 0x06007800 e 0x06006600 (batalha, BattlePreview_LoadEntryPalette 0x0800522C, registro 0x030017A0 + ctx[0x51]*8, destino 0x06006600) e PartsGrid_UploadCellTiles 0x08009484 (0x03000BE0 com stride 0x40, bit 0x80 de +0 = tem ícone, destino 0x06006600) não são a BUILD.

## Passo 12: PartDetail_GetCurrentSelectionLegIconIndex 0x0800F6CC (CONFIRMADO no decompile)
- Código: `v = byte(ctx[0x03000440] + 0xEF); return v < 100 ? v : v - 100;` (ctx = 0x03000440, literal 0x0800F6DC).
- NÃO lê gPartStats: lê o byte 0xEF do contexto de menu 0x03000440. Hipótese anterior do IC.md (gPartStats[bot][3].+1-100) está ERRADA.
- Índice do ícone de perna = tipo da perna (100..106) menos 100. Valores < 100 (ex. 0xFF) passam sem mudar.
- Chamador: `0x0800E376` (xref). Não decompilado nesta passada.

## Resumo dos três itens
1. **Struct de tipo / sprite grande**: feito. Vetor ROM 0x083D1D78 (kind -> struct). Struct T: +0 = ponteiro do vetor de registros de 8 B (word0 gráfico, word1 paleta); +0x0C/0D = offset X/Y; +0x0F/+0x10 = filhos; +0x11 = banco de paleta; +0x12/13 = largura/altura; +0x14 = divisores; +0x15 = flags do OAM (8bpp, flip, modo). Forma OBJ via SetCompositePreviewOamShapeAndSize. Exemplos A e B no passo 6.
2. **Ícone de medalha / tiles 960..963**: escritor = PartSlot_LoadIconPalette 0x08005B44 e PartsGrid_UploadSelectedTileB 0x08009EA0. Fórmula real: 0x084C29D8 + k*0x80, com k = byte +1 do roster 0x03000BE0 (stride 0x40). A hipótese (m+9) NÃO é confirmada.
3. **PartDetail_GetCurrentSelectionLegIconIndex 0x0800F6CC**: decompile feito. Lê ctx 0x03000440 +0xEF, subtrai 100 se >= 100. Hipótese antiga do IC.md corrigida.

## Pendente (para o Opus revisar)
- Forma OBJ 64x32 (tipos 13, 16, 19): decompile dá shape 2 (vertical no padrão GBA). Confirmar se o DMA do OAM troca, com `mgba_dump_oam` ou `oam_render.py` (sem emulador nesta passada).
- Campos T+0x04 e T+0x08 não identificados (0x0D/0x1A e 0x08/0x0F nos exemplos).
- Quem escreve o roster 0x03000BE0 (+1 = bot+1?) e onde a MEDALHA entra na tela BUILD (qual registro de gMedals gera k). Sem isso, "medalha = bot k" é provável, não confirmado.
- Chamador 0x0800E376 de PartDetail_GetCurrentSelectionLegIconIndex não decompilado.
- Tipos 3..6 têm ponteiros de IWRAM (0x030052B0..0x030052F0): montados em tempo de execução, quem os escreve não lido.
- Destino VRAM de 0x08785EC0 (IC.md passo 4, 0x06006FE0): continua "provável", não confirmado no emulador.
- Nenhum nome novo foi aplicado no Ghidra: todas as funções confirmadas já tinham nome.
