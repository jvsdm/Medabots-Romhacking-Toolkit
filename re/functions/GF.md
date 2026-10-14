# GF: catálogo de gráficos (carregadores e assets)

Status: PARCIAL (2026-10-14). Só endereços e metadados; nenhuma imagem extraída.
Ghidra: `decompress_and_store` 0x080009BC tem ~100 chamadores; `LoadTilesWithMalias` 0x08000E04 tem ~80.
`Gfx_LoadTileAsset` 0x08000E40 não tem xref (get_xrefs_to vazio). Esta passada cobriu 4 chamadores; o resto está em "Pendente".

## Formatos (lidos no cabeçalho da ROM original)
- **Malias** = cabeçalho com magic `4C 65` ("Le"), u16 em +2 (campo de tamanho; semântica NÃO confirmada, ver nota), u16 em +4..5 e byte em +6/+7 variando.
  Para mapas de tela (2048 B de tilemap) o campo +2 vale 0x0810 = 16 B de cabeçalho + 2048 B (conferido).
- **LZ77 tipo 0x10** = byte 0x10 + 24 bits de tamanho descomprimido (ex.: `10 10 08 00` = 0x000810).
- **Cru** = sem cabeçalho (paletas: `gGfx_PartsGrid_Palette_Banks`, 0x70 words, para 0x05000000).

## Tabela: assets por tela

Destino "VRAM" = endereço de escrita. Tamanho = bytes do cabeçalho (ver nota do formato).

### Montagem / grade de peças (PartsGrid)
| Endereço do asset | Formato | Tamanho (cabeçalho) | Destino | Função que carrega | Tela |
|---|---|---|---|---|---|
| 0x084E9650 | Malias | +2 = 0x1CA0 | VRAM 0x06004000 (char BG) | PartsGrid_InitScreen 0x08007A04 | Grade de peças |
| 0x084C3A24 | Malias | +2 = 0x0340 | VRAM 0x06006280 (char BG) | PartsGrid_InitScreen 0x08007A04 | Grade de peças |
| 0x084C3BF8 | Malias | +2 = 0x0580 | VRAM 0x06006800 (char BG) | PartsGrid_InitScreen 0x08007A04 | Grade de peças |
| 0x084C88B8 | Malias | +2 = 0x01C0 | VRAM 0x06017E00 (dentro da faixa OBJ) | PartsGrid_InitScreen 0x08007A04 | Grade de peças |
| 0x084F0920 | Malias (mapa) | +2 = 0x0810 | tilemap BG0 (BGCNT 0x0500 = SBB 5) | LoadTilesWithMalias via PartsGrid_InitScreen | Grade de peças |
| 0x084F1180 | Malias (mapa) | +2 = 0x0810 | tilemap BG1 (BGCNT 0x1801) | LoadTilesWithMalias via PartsGrid_InitScreen | Grade de peças |
| 0x084F1C28 | Malias (mapa) | +2 = 0x0810 | tilemap BG2 (BGCNT 0x1902) | LoadTilesWithMalias via PartsGrid_InitScreen | Grade de peças |
| 0x084F2C88 | Malias (mapa) | +2 = 0x0810 | tilemap BG3 (BGCNT 0x1A03) | LoadTilesWithMalias via PartsGrid_InitScreen | Grade de peças |
| 0x084F4300 | cru | 0x70 words | paleta 0x05000000 | bios_cpufastset em PartsGrid_InitScreen | Grade de peças |

Nota: `PartsGrid_InitScreen_08008234` (0x08008234, provável) usa a mesma família de cargas (decompress x7, tiles x4) e não foi lido nesta passada.

### Loja (seleção de peças)
| Endereço do asset | Formato | Tamanho (cabeçalho) | Destino | Função que carrega | Tela |
|---|---|---|---|---|---|
| 0x08785FB4 | Malias | +2 = 0x3A80 | VRAM 0x06010000 (OBJ) | Shop_InitPartSelectionScreen 0x080619E0 | Loja (seleção de peças) |
| 0x084C8034 | Malias | +2 = 0x09C0 | VRAM 0x06017400 (OBJ) | Shop_InitPartSelectionScreen 0x080619E0 | Loja (seleção de peças) |

Nota: a lista de peças da loja vem da ROM em 0x084838E4 (tabela de dados, não é imagem); é lida por GetPartStats 0x0806xxxx (fora do escopo).

### Batalha (bots, lado)
| Endereço do asset | Formato | Tamanho (cabeçalho) | Destino | Função que carrega | Tela |
|---|---|---|---|---|---|
| 0x084882CC | Malias | +2 = 0x0070 | LoadTilesWithMalias bloco 0x0D (count 600) | Fight_LoadBattleBotTilesSet 0x08015A90 | Batalha |
| 0x084881E0 | Malias | +2 = 0x0038 | LoadTilesWithMalias bloco 0x11 (count 0x210) | Fight_LoadBattleBotTilesSet 0x08015A90 | Batalha |
| 0x08488240 | Malias | +2 = 0x004C | LoadTilesWithMalias bloco 0x0F (count 0x230) | Fight_LoadBattleBotTilesSet 0x08015A90 | Batalha |
| 0x08488290 | Malias | +2 = 0x0060 | LoadTilesWithMalias bloco 0x10 (count 0x240) | Fight_LoadBattleBotTilesSet 0x08015A90 | Batalha |

Nota: o primeiro argumento de LoadTilesWithMalias é 0 em todos; o segundo é o bloco/banco de destino (0x0D, 0x11, 0x0F, 0x10 na ordem de chamada; o xref de Fight_LoadBotSideTiles 0x0801AC98 usa 0x0D/0x11, batendo com a nota da R1). Ordem e nomes dos bancos ainda não conferidos no emulador.

### Cena genérica com música 0x19 (tela não identificada)
Função: `Screen_LoadSceneWithSong` 0x08067DC6 (chamada também por outras telas). Assets:
| Endereço do asset | Formato | Tamanho (cabeçalho) | Destino | Função que carrega | Tela |
|---|---|---|---|---|---|
| 0x087A918C | LZ77 0x10 | 0x000810 | Screen_LoadLZ77TileBlock (BG1CNT 0x1E05) | Screen_LoadSceneWithSong 0x08067DC6 | Cena c/ música 0x19 (não identificada) |
| 0x087A92DC | LZ77 0x10 | 0x000810 | Screen_LoadLZ77TileBlock (BG2CNT 0x0C, valor 0x3D06 no ponteiro) | Screen_LoadSceneWithSong | idem |
| 0x087A9518 | LZ77 0x10 | 0x000810 | Screen_LoadLZ77TileBlock (BG3CNT 0x0E, 0x1C07) | Screen_LoadSceneWithSong | idem |
| 0x087A58EC | LZ77 0x10 | 0x004B20 | Asset_UnpackOrCopyBlock -> buffer 0x02030000 -> bios_cpufastset 0x06004000 | Screen_LoadSceneWithSong | idem |
| 0x08787144 | Malias | +2 = 0x3780 | VRAM 0x06010000 (OBJ) | Screen_LoadSceneWithSong | idem |
| 0x084C3A24 | Malias | +2 = 0x0340 | VRAM 0x06000080 (BG) | Screen_LoadSceneWithSong | idem (mesmo asset da grade de peças) |
| 0x084C8034 | Malias | +2 = 0x09C0 | VRAM 0x06017400 (OBJ) | Screen_LoadSceneWithSong | idem (mesmo asset da loja) |
| 0x084E43D8 | cru (paleta) | bios_cpufastset | 0x0202F9E0 (buffer de paleta) | Screen_LoadSceneWithSong | idem |

Nota: `Screen_LoadSceneWithSong` tem bloco de OAM (16 entradas em 0x03002C30) e `snd_song_request(0x19)`. Não é título nem mapa pelo nome; tela real a confirmar no emulador.

## Nomes aplicados no Ghidra (rename_data, status "attempted", NÃO conferido)
Ver `GF_simbolos.csv`. O retorno da ferramenta foi "Rename data attempted"; conferir no Ghidra antes de dar como aplicado.

## Pendente
- ~96 chamadores de `decompress_and_store` e ~76 de `LoadTilesWithMalias` ainda sem tabela (ex.: `Screen_LoadSpriteRowA` 0x08003300, `Screen_DecompressRowImageByIndex` 0x08003904, `InitializeBattleSceneScreen` 0x080040BC, `InitializeBattleStatusScreen`, `Fight_BattleSceneInit` 0x08013C14, `Overlay_InitScreenGraphics` 0x0803DD60, `PartDetailScreen_Init` 0x0800E0BC, `Menu_InfoPanelStep` 0x0800D114, `PartsGrid_EnterScreenInit` 0x08008810, `DebugMenu_Init` 0x0800B8E8).
- `Gfx_LoadTileAsset` 0x08000E40 (sem xref direto; chamadores de wrapper?) e `Screen_LoadLZ77TileBlock` (endereço não conferido nesta passada).
- Semântica do campo +2 do cabeçalho Malias (0x0070 nos blocos de batalha não bate com tamanho em bytes; provável contagem de tiles). Não confirmado.
- Título, mapa e menu de campo: nenhum chamador desses lidos.
- Confirmar no emulador (ROM de debug, só leitura de RAM/VRAM): qual cena é `Screen_LoadSceneWithSong` e destino real dos blocos 0x0D/0x11/0x0F/0x10 de batalha.
