# Retratos dos dialogos (tarefa 10) - analise estatica, nao testada

Fonte: leitura do Toolkit (`PortraitAsset`, `ImageAssetRepository.BasicAssets`, `PortraitExpander`,
`RomLayout.Portraits`, `MedabotsTextCodec`, `MainWindow.MessagePreview`) e do CSV. Sem ROM: tudo abaixo e
"provavel" ate a sessao local conferir (secao 5).

## 1. Tag no texto: `<PORTRAIT:a,b,c>` = byte 0xFB + 3 bytes

| Byte | Nome | Significado (provavel) |
|---|---|---|
| a | lado | 1 = esquerda, 0 = direita (previa do WPF: `left = side==1 ? 8 : 240-8-64`) |
| b | personagem | indice na tabela de retratos (0-175 no jogo original; 1 byte, ate 255 com a expansao) |
| c | expressao | 0-8 (9 por personagem) |

- O retrato fica valendo nas caixas seguintes ate outro `<PORTRAIT>` (a previa propaga; confirmar no jogo).
- Em `Parse_text` (0x0803FD28) o 0xFB le 3 bytes; o jogo nao checa limite de personagem nem de expressao.

## 2. Tabelas (enderecos EU reais; o Toolkit usa +0x20 em varias tabelas: 0x3AFEA8 / 0x3B1768)

| Endereco | Conteudo |
|---|---|
| 0x083AFE88 `gPortraitImages` | 176 x 9 ponteiros de 4 B, indice = `personagem*9 + expressao`, cada um para uma imagem Malias2 |
| 0x083B1748 `gPortraitPalettes` | 176 ponteiros de 4 B, 1 paleta BGR555 de 16 cores (32 B) por personagem (as 9 expressoes dividem a paleta) |

- Imagem: 64x64 px, 4bpp, 8x8 tiles = 64 tiles x 32 B = 0x800 B descomprimidos (`PortraitExpander.PortraitTiles = 64`;
  o Toolkit le `tiles / 8` de largura, entao `PortraitTileWidth = 8`). Ordem dos tiles: linha a linha (tile 0..7 = primeira
  faixa de 8 px), cada tile 8x8 4bpp linear (`TilePixel`: `((y/8)*8 + x/8)*64 + (y%8)*8 + x%8`).
- Compressao: Malias2 (decoder do jogo 0x080009BC, r0 = origem, r1 = destino). Ha fallback "Le" com comandos 0xAA no Toolkit
  para dados sem Malias2; nao se sabe se o jogo usa.
- Indice 0 da paleta = transparente.

## 3. Carga (rotinas, enderecos do CSV)

- `get_portrait` 0x0803F41C: carrega imagem `[personagem*9+expressao]` + paleta do personagem para buffer (3 literais para as
  tabelas nas rotinas 0x0803F41C/0x0803F480/0x0803F51C: 3 para a de imagens e 4 para a de paletas, segundo o `PortraitExpander`).
- `LoadPortraitToVram_Unk` 0x0803F480: tiles em VRAM 0x06000F80 (tile 124 do char base 0 = 0x06000000 + 124*32), paleta em
  0x050001A0 (banco 13 da paleta de BG: 0x05000000 + 13*0x20). Logo o retrato e um **BG** (nao OBJ), tilemap proprio.
- `Move_Character_Portraits` 0x0803F51C: so carrega se `gTextState+0xE == 0` (provavel: "retrato ja trocado/ocupado").
- A caixa de dialogo e o texto usam a fonte 4bpp 8x16 (`gFont`, paleta `gFontPalette` 0x084E43D8); o retrato nao divide paleta com ela.

## 4. Regra para desenhar o retrato na caixa (para o PC / previa)

1. Ler `ptrImg = gPortraitImages[b*9+c]`, `ptrPal = gPortraitPalettes[b]`; descomprimir Malias2 -> 0x800 B; 64 tiles 8x8.
2. Montar a imagem 64x64 e a paleta (indice 0 transparente).
3. Posicao: **por cima da caixa**, `y = topo_da_caixa - 64` (a previa usa `BoxTop - 64`), `x = 8` (lado 1) ou `240-8-64 = 168` (lado 0).
4. Orientacao: as imagens sao desenhadas viradas para a **esquerda**; no lado 1 (retrato a esquerda, falando para a direita)
   espelhar horizontalmente (hflip); lado 0 desenha como esta. (Previa do WPF; `Move_Character_Portraits`/Parse deve usar
   flag H do tilemap - conferir.)
5. Sem animacao de boca: as 9 expressoes sao quadros estaticos escolhidos pelo `c`.

## 5. O que a sessao local precisa conferir

1. No Ghidra, decompilar 0x0803F41C, 0x0803F480, 0x0803F51C e a chamada em `Parse_text` 0x0803FD28 (caso 0xFB): confirmar a
   ordem dos 3 bytes (lado, personagem, expressao) e se o lado vira flag de hflip do tilemap (`0x0400`) ou escolha de outro
   tilemap/posicao.
2. Em emulador: `<PORTRAIT:1,..>` aparece a esquerda? Posicao (x, y) exata em pixels e se o retrato some quando acabam as caixas.
3. Confirmar VRAM 0x06000F80 / paleta 0x050001A0 e o tamanho 64x64 (64 tiles) e o endereco do tilemap do retrato.
4. Se `gTextState+0xE` e "retrato ja carregado" (evita recarregar a mesma imagem) ou outra coisa.
5. Contar `a`/`c` usados no dump do texto (`dump_text_v3.py`): valores de lado diferentes de 0/1? expressoes > 8?
