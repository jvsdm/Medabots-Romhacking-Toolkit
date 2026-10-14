# GF3: catálogo de gráficos, parte 3 (overworld, diálogo/retratos, menu de campo, status e painel de batalha, grade de peças, título/logo)

Status: PARCIAL (2026-10-14). Só endereços e metadados lidos no literal pool da ROM original
(`Medabots - Metabee (Europe).gba`, só leitura). Nenhuma imagem extraída. Continua `GF.md` e `GF2.md`.

Convenção igual à do GF2: "Malias +2" = u16 do cabeçalho (semântica não confirmada). "ptr" = o endereço é uma
TABELA DE PONTEIROS (o carregador escolhe a entrada por índice), não a imagem. "cru" = sem cabeçalho.

Correção de endereços: vários da lista pendente do GF2 são miolo de função. Entradas reais (Ghidra
`get_function_by_address`): Move_Character_Portraits 0x0803F51C; LoadPortraitToVram_Unk 0x0803F480;
Menu_InfoPanelStep 0x0800CE90; InitializeBattleStatusScreen 0x08003A38 (0x08003A9C é miolo);
BattlePanel_SceneInit 0x08062FB4; PartsGrid_EnterScreenInit 0x080087CC; Map_LoadOverworldSpriteGraphicsForCurrentMap
0x08048288; Map_LoadCurrentMapLayers 0x0804B2B0; Assets_LoadPairAndFastCopy 0x0803F3A8.

Gfx_LoadTileAsset 0x08000E40 (entrada) não tem xref (`get_xrefs_to` e `get_function_xrefs` vazios). Corpo:
`decompress_and_store` (src = r2) para o buffer 0x02030000, depois `FUN_08000F1C(dst, r1, buffer, r3)` (cópia para VRAM).
Wrapper genérico; chamadores provavelmente por ponteiro de tabela (sem BL).

## Tabela: assets por tela

### Overworld (mapa de campo)
Orquestrador: `Map_LoadCurrentSceneResources` 0x0804DDCC (chama `Map_LoadCurrentMapLayers`,
`Map_LoadOverworldSpriteGraphicsForCurrentMap` e `Overworld_InitPlayerSprites`). Chamado por
`Event_Op0B_Cont_WarpA_LoadScene` e `Event_Op0C_Cont_WarpB_LoadScene` (warp). Confiança: confirmado.

| Endereço do asset | Formato | Tamanho (cabeçalho) | Destino | Função que carrega | Tela |
|---|---|---|---|---|---|
| 0x083F7E80 | ptr (tabela de sprites, ≥14 entradas) | 4 B por entrada | lê 0x085D7018, 0x085FB35C, 0x085D7454... | Map_LoadOverworldSpriteGraphicsForCurrentMap 0x08048288 (bios_lz77_vram 0x0807CCD8) | Overworld: sprites de personagem |
| 0x085D7018 | LZ77 0x10 | 0x000900 | VRAM 0x06010000 + n×0x900 | idem | idem (entrada 0) |
| 0x085FB35C | LZ77 0x10 | 0x000900 | slot seguinte | idem | idem (entrada 1) |
| 0x08413160 | ptr/índice por mapa | — | lido pela rotina 0x08048684 | idem | idem (escolhe os slots) |
| 0x08404334 | ptr (camadas do mapa) | 4 B por entrada | lida em Map_LoadCurrentMapLayers | Map_LoadCurrentMapLayers 0x0804B2B0 | Overworld: camadas |
| 0x083F9080 | ptr (tiles BG) | 4 B por entrada | VRAM 0x06009000 (BG) | Map_LoadCurrentMapLayers | Overworld: tileset |
| 0x08600058 | LZ77 0x10 | 0x001970 | (entrada de 0x083F9080) | idem | idem |
| 0x083F8A80 | ptr (tiles BG) | 4 B por entrada | VRAM 0x06003000 (BG) | Map_LoadCurrentMapLayers | Overworld: tileset |
| 0x085FC6AC | LZ77 0x10 | 0x0065E0 | (entrada de 0x083F8A80) | idem | idem |
| 0x086019D8 | LZ77 0x10 | 0x0038A0 | (entrada de 0x083F8A80) | idem | idem |

Nota: o loop de 0x08048288 chama 0x08048684 esperando retorno (índice), mas o Ghidra decompila 0x08048684 como
`void` (wrapper que lê byte de 0x08413160 e chama 0x08048210): decompilação duvidosa. Destino 0x06010000 + n×0x900: confirmado no código.

### Caixa de diálogo e escolha (moldura, paletas)
Carregador: `Assets_LoadPairAndFastCopy` 0x0803F3A8. Chamadores: `Obj_InitSlotAndLoad` (0x0803FADC) e
`Overworld_InitChoiceMenuScreen` (0x0805738A). Confiança: confirmado.

| Endereço do asset | Formato | Tamanho (cabeçalho) | Destino | Função que carrega | Tela |
|---|---|---|---|---|---|
| 0x08486E6C | Malias | +2 = 0x0220 | VRAM 0x06000020 (char BG0) | Assets_LoadPairAndFastCopy | Caixa de diálogo (moldura, = H1 do README) |
| 0x084C39D8 | Malias | +2 = 0x0040 | VRAM 0x06001780 (char BG0) | Assets_LoadPairAndFastCopy | Caixa de diálogo / escolha (tiles auxiliares) |
| 0x08488520 | cru (paleta) | 0x0010 words | paleta 0x050001E0 (banco 15) | Assets_LoadPairAndFastCopy (CpuFastSet 0x0807CCC0) | Caixa de diálogo (ornamento, = H1 do README) |

### Retratos (diálogo)
Carregadores: `Move_Character_Portraits` 0x0803F51C e `LoadPortraitToVram_Unk` 0x0803F480 (mesmo trio de literais).
Confiança: confirmado no código.

| Endereço do asset | Formato | Tamanho | Destino | Função que carrega | Tela |
|---|---|---|---|---|---|
| 0x083AFE88 | ptr (1 entrada por personagem) | 4 B por entrada (ex.: 0x084889D4, 0x08488C94) | decompress → VRAM 0x06000F80 | Move_Character_Portraits / LoadPortraitToVram_Unk (decompress_and_store 0x080009BC) | Diálogo: retrato |
| 0x083B1748 | cru (paleta) | 0x0010 words (16 cores) | paleta 0x050001A0 | idem (CpuFastSet 0x0807CCC0) | Diálogo: retrato (paleta) |
| 0x083B4C60 | ptr (2 entradas: 0x083B2B00, 0x083B2B80) | 4 B por entrada | argumento de 0x0803F15C (layout, provável) | idem | Diálogo: retrato (layout) |

Conflito: `pc/research/README.md` diz `gPortraitImages` 0x083AFE88 = "Malias2, 2048 B". Pelo código, 0x083AFE88 é
tabela de ponteiros. Conferir antes de usar aquela nota. A moldura Malias2 0x08486E6C (cabeçalho +2 = 0x0220) bate com o README.

### Menu de campo (Menu_InfoPanelStep 0x0800CE90)
`decompress_and_store` 0x0800D114 → VRAM 0x06010000 (OBJ); src = entrada da tabela 0x083AC614 (5 colunas × 4 B)
escolhida por bytes de estado. Paleta por CpuFastSet 0x084E4438 → 0x050003E0. Textos por `Text_LayoutGlyphSprites`.
Confiança: provável (o decompilador não fechou o src).

| Endereço do asset | Formato | Tamanho | Destino | Função que carrega | Tela |
|---|---|---|---|---|---|
| 0x083AC614 | ptr (tabela 5 colunas) | — | src do decompress em 0x0800D114 | Menu_InfoPanelStep | Menu de campo (painel de info) |
| 0x084E4438 | cru (paleta) | 0x0010 words | 0x050003E0 (OBJ) | Menu_InfoPanelStep | Menu de campo |
| 0x084E4458 | cru | 8 words | buffer 0x0202F980 | InitializeBattleStatusScreen | compartilhado |
| 0x084E4478 | cru | 8 words | buffer 0x0202F9E0 | InitializeBattleStatusScreen | compartilhado |

### Status de batalha (InitializeBattleStatusScreen 0x08003A38)
Confiança: confirmado nos literais.

| Endereço do asset | Formato | Tamanho (cabeçalho) | Destino | Função que carrega | Tela |
|---|---|---|---|---|---|
| 0x084E6BF0 | Malias | +2 = 0x1320 | VRAM 0x06004000 (char BG0) | InitializeBattleStatusScreen | Batalha: status (char principal) |
| 0x084C50D0 | Malias | +2 = 0x0200 | VRAM 0x06006000 (char BG) | idem | idem |
| 0x084C5250 | Malias | +2 = 0x0200 | VRAM 0x06006200 (char BG) | idem | idem |
| 0x084C89C8 | Malias | +2 = 0x0500 | VRAM 0x06016C00 (OBJ) | idem | idem |
| 0x084C8034 | Malias | +2 = 0x09C0 | VRAM 0x06017400 (OBJ) | idem | idem (asset de loja/cena) |
| 0x084F0878, 0x084F0EFC, 0x084F1858, 0x084F27CC | Malias (mapa) | +2 = 0x0810 | tilemap BG0..BG3 (BGCNT 0x0400/0x0A/0x0C/0x0E como ponteiro) | LoadTilesWithMalias 0x08000E04 | idem |
| 0x084F3C20 | cru | 0x70 words | buffer 0x0202FA00 | idem (CpuFastSet) | idem (paleta BG) |
| 0x0850F498 | cru | 0x40 words | buffer 0x0202F800 e paleta OBJ 0x05000200 | idem | idem (compartilhado) |
| 0x0850F2B8 | cru | 8 words | buffer 0x0202F9A0; paletas 0x050003A0 e 0x050003C0 | idem | idem (compartilhado) |

### Painel de batalha (BattlePanel_SceneInit 0x08062FB4)
Confiança: confirmado nos literais; os carregadores 0x0805BA34 e 0x08044D0C são prováveis (não lidos).

| Endereço do asset | Formato | Tamanho (cabeçalho) | Destino | Função que carrega | Tela |
|---|---|---|---|---|---|
| 0x08785FB4 | Malias | +2 = 0x3A80 | VRAM 0x06010000 (OBJ) | BattlePanel_SceneInit | Batalha: painel (mesmo asset da loja) |
| 0x08785EC0 | Malias | +2 = 0x03E0 | decompress (destino não lido) | BattlePanel_SceneInit | Batalha: painel |
| 0x084C8034 | Malias | +2 = 0x09C0 | VRAM 0x06011E00 (OBJ) | BattlePanel_SceneInit | idem |
| 0x0879EFC0 | LZ77 0x10 | 0x000810 | via 0x0805BA34 (provável BG) | BattlePanel_SceneInit | idem (mapa) |
| 0x0879F138 | LZ77 0x10 | não lido | via 0x0805BA34 | BattlePanel_SceneInit | idem |
| 0x0879F3D0 | LZ77 0x10 | não lido | via 0x0805BA34 | BattlePanel_SceneInit | idem |
| 0x0879DA90 | LZ77 0x10 | 0x002FE0 | buffer 0x02030000 → VRAM 0x06004000 (0x0807CCC4) | BattlePanel_SceneInit (0x08044D0C) | idem |
| 0x0850F318 | cru | — | paleta 0x050003A0 | BattlePanel_SceneInit | idem |
| 0x0879D890 | cru | — | buffer 0x0202F9A0 | BattlePanel_SceneInit | idem |

### Grade de peças (PartsGrid_EnterScreenInit 0x080087CC)
Confiança: confirmado nos literais. Complementa GF.md (PartsGrid_InitScreen 0x08007A04).

| Endereço do asset | Formato | Tamanho (cabeçalho) | Destino | Função que carrega | Tela |
|---|---|---|---|---|---|
| 0x084EC1F8 | Malias | +2 = 0x2B00 | VRAM 0x06004000 (char BG0) | PartsGrid_EnterScreenInit | Grade de peças (entrada) |
| 0x084C3A24 | Malias | +2 = 0x0340 | VRAM 0x06007480 (char BG) | idem | idem (mesmo asset do PartsGrid) |
| 0x084C3E64 | Malias | +2 = 0x0140 | VRAM 0x06007880 (char BG) | idem | idem |
| 0x084C88B8 | Malias | +2 = 0x01C0 | VRAM 0x06017E00 (OBJ) | idem | idem |
| 0x084F0A80, 0x084F13C8, 0x084F1FA0, 0x084F3140 | Malias (mapa) | +2 = 0x0810 | tilemap BG0..BG3 | LoadTilesWithMalias 0x08000E04 | idem |
| 0x084F4840 | cru (paleta) | — | paleta (destino não lido) | idem (CpuFastSet 0x0807CCC0) | idem |
| 0x0850F2B8 | cru | 8 words | 0x05000380 e 0x050003A0 | idem | idem |
| 0x084E43F8 | cru | 8 words | 0x050003C0 | idem | idem (paleta de texto) |

### Título e logo (cenas 0 e 2 da abertura, ver IN1_intro.md)
Carregador do título: 0x0806D7A0 (entrada). Logo: 0x0806E6E4. Menu do título: 0x0805C17C.
Endereços de título já documentados em `pc/research/IN1_intro.md` e `README.md` (IN2, IN4); aqui só o chamador.

| Endereço do asset | Formato | Tamanho (cabeçalho) | Destino | Função que carrega | Tela |
|---|---|---|---|---|---|
| 0x087B67B0 | LZ77 0x10 | 0x0061C0 | VRAM 0x06008000 (BG0) | Title loader 0x0806D7A0 (bios_lz77_vram 0x0807CCD8) | Título: BG0 |
| 0x087BB060 | LZ77 0x10 | 0x000500 | VRAM 0x06006800 (BG tilemap) | idem | Título: mapa BG |
| 0x087BB2A0 | LZ77 0x10 | não lido | VRAM 0x06007000 | idem | Título: mapa BG |
| 0x087BB7E0 | LZ77 0x10 | não lido | VRAM 0x06007800 | idem | Título: mapa BG |
| 0x087AEFD0 | LZ77 0x10 | 0x005060 | VRAM 0x06010000 (OBJ) | idem | Título: sprites (PRESS START, logo) |
| 0x0807C490 | cru (paleta) | — | paleta OBJ 0x05000200 | idem | Título: paleta OBJ |
| 0x087BAFA0 | cru (paleta) | — | paleta BG (bancos 0..5) | idem | Título: paleta BG |
| 0x0878F7E8 | LZ77 0x10 | 0x003880 | VRAM 0x06004000 (char BG) | menu do título, 0x0805C17C | Título: menu NEW GAME / OPTIONS (IN4) |
| 0x087F314C | LZ77 0x10 | 0x000DE0 | VRAM 0x06000000 (BG) | logo, 0x0806E6E4 | Logo NATSUME (cena 0) |

## Nomes aplicados no Ghidra (rename_data)
Ver `GF3_simbolos.csv`. Conferir o resultado de cada rename antes de dar como aplicado.

## Pendente
- Gfx_LoadTileAsset 0x08000E40: sem chamador achado; procurar uso por tabela/ponteiro.
- Cabeçalhos não lidos: 0x087BB2A0, 0x087BB7E0, 0x0879F138, 0x0879F3D0.
- Menu_InfoPanelStep: src exato do decompress (tabela 0x083AC614 e bytes de índice) não fechado.
- Painel de batalha: destinos de 0x0805BA34 e 0x08044D0C não lidos.
- Overworld: confirmar no emulador (ROM de debug, só leitura de VRAM) os slots 0x06010000 e os tiles 0x06009000/0x06003000.
- Conflito de gPortraitImages (README x código): ver "Retratos".
- DebugMenu_Init 0x0800B8E8 e Screen_InitNumberRevealLayout 0x08063A8C: não tratados nesta passada.
