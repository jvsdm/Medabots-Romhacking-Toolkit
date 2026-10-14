# GF2: catálogo de gráficos, continuação (chamadores de decompress_and_store / LoadTilesWithMalias)

Status: PARCIAL (2026-10-14). Só endereços e metadados lidos no literal pool da ROM original
(`Medabots - Metabee (Europe).gba`, só leitura). Nenhuma imagem extraída. Continua `GF.md`.

Convenção: "Malias +2" = u16 do cabeçalho (mesma semântica não confirmada, ver GF.md). Quando o
literal do pool é cru (sem "Le" nem 0x10), está marcado "cru". Endereço de VRAM = destino do literal.
`decompress_and_store(src, dst)` e `LoadTilesWithMalias(0, 0, src, bank, 0x200)`: o 3º/4º argumento
vêm dos pares do pool (ponteiro, destino). Bank de tilemap = valor de BGCNT (0x0500, 0x1801...).

## Tela: cena de batalha (InitializeBattleSceneScreen 0x080040BC)
| Endereço do asset | Formato | Tamanho (cabeçalho) | Destino | Função que carrega | Tela |
|---|---|---|---|---|---|
| 0x084E776C | Malias | +2 = 0x11A0 | VRAM 0x06004000 (char BG0) | InitializeBattleSceneScreen 0x080040BC | Batalha: cena de peças (BattleScene) |
| 0x084C50D0 | Malias | +2 = 0x0200 | VRAM 0x06006000 (char BG) | InitializeBattleSceneScreen | idem |
| 0x084C5250 | Malias | +2 = 0x0200 | VRAM 0x06006200 (char BG) | InitializeBattleSceneScreen | idem |
| 0x084C3A24 | Malias | +2 = 0x0340 | VRAM 0x06007480 (char BG) | InitializeBattleSceneScreen | idem (asset de PartsGrid) |
| 0x084C3BF8 | Malias | +2 = 0x0580 | VRAM 0x06006800 (char BG) | InitializeBattleSceneScreen | idem (asset de PartsGrid) |
| 0x084C55CC | Malias | +2 = 0x0380 | VRAM 0x06006E00 (char BG) | InitializeBattleSceneScreen | idem (também em Fight_BattleSceneInit) |
| 0x084C88B8 | Malias | +2 = 0x01C0 | VRAM 0x06017E00 (OBJ) | InitializeBattleSceneScreen | idem (asset de PartsGrid) |
| 0x084C8034 | Malias | +2 = 0x09C0 | VRAM 0x06017400 (OBJ) | InitializeBattleSceneScreen | idem (asset de loja/cena) |
| 0x084F08B0 | Malias (mapa) | +2 = 0x0810 | tilemap BG0 (BGCNT 0x0500) | LoadTilesWithMalias bank 0x0500 | idem |
| 0x084F0F68 | Malias (mapa) | +2 = 0x0810 | tilemap BG1 (BGCNT 0x1801) | LoadTilesWithMalias | idem |
| 0x084F198C | Malias (mapa) | +2 = 0x0810 | tilemap BG2 (BGCNT 0x1902) | LoadTilesWithMalias | idem |
| 0x084F2960 | Malias (mapa) | +2 = 0x0810 | tilemap BG3 (BGCNT 0x1A03) | LoadTilesWithMalias | idem |
| 0x084F3DC0 | cru | 0x70 words | buffer 0x0202FA00 (CpuFastSet) | InitializeBattleSceneScreen | idem (paleta/fundo) |
| 0x0850F498 | cru | 0x40 words | buffer 0x0202F800 | InitializeBattleSceneScreen | idem (compartilhado com Fight_BattleSceneInit e DetalhePeca) |
| 0x0850F2F8 | cru | 8 words | buffer 0x0202F980 | InitializeBattleSceneScreen | idem (compartilhado) |
| 0x084E43F8 | cru | 8 words | buffer 0x0202F9C0 | InitializeBattleSceneScreen | idem (compartilhado) |

Ramo `else` (quando byte +0x56 de DAT_08004194 é != 0): CpuFastSet direto para 0x05000000 (paleta BG,
0x70 words, src 0x084F3DC0 = mesmo literal) e para 0x05000200/0x05000380/0x050003A0/0x050003E0/0x050003C0.
Dado da cena de batalha (outra grafia): paleta OBJ em 0x05000200 vem do mesmo buffer.

## Tela: batalha, cena de luta (Fight_BattleSceneInit 0x08013C14)
| Endereço do asset | Formato | Tamanho (cabeçalho) | Destino | Função que carrega | Tela |
|---|---|---|---|---|---|
| 0x084EEA64 | Malias | +2 = 0x10A0 | VRAM 0x06004000 (char BG0) | Fight_BattleSceneInit 0x08013C14 | Batalha: cena de luta (FightScene) |
| 0x084C58F4 | Malias | +2 = 0x0100 | VRAM 0x06006000 (char BG) | Fight_BattleSceneInit | idem |
| 0x084C3BF8 | Malias | +2 = 0x0580 | VRAM 0x06006800 (char BG) | Fight_BattleSceneInit | idem (asset de PartsGrid) |
| 0x084C55CC | Malias | +2 = 0x0380 | VRAM 0x06006E00 (char BG) | Fight_BattleSceneInit | idem |
| 0x084C3F0C | Malias | +2 = 0x0280 | VRAM 0x06007200 (char BG) | Fight_BattleSceneInit | idem |
| 0x084C58D8 | Malias | +2 = 0x0040 | VRAM 0x06007480 (char BG) | Fight_BattleSceneInit | idem |
| 0x084C88B8 | Malias | +2 = 0x01C0 | VRAM 0x06017E00 (OBJ) | Fight_BattleSceneInit | idem (asset de PartsGrid) |
| 0x084F0B7C | Malias (mapa) | +2 = 0x0810 | tilemap BG0 | Fight_BattleSceneInit (LoadTilesWithMalias) | idem |
| 0x084F16C8 | Malias (mapa) | +2 = 0x0810 | tilemap BG1 | Fight_BattleSceneInit | idem |
| 0x084F22C4 | Malias (mapa) | +2 = 0x0810 | tilemap BG2 | Fight_BattleSceneInit | idem |
| 0x084F35FC | Malias (mapa) | +2 = 0x0810 | tilemap BG3 | Fight_BattleSceneInit | idem |
| 0x084F4D80 | cru | 0x70 words | paleta BG 0x05000000 | Fight_BattleSceneInit | idem |
| 0x0850F498 | cru | 0x40 words | paleta OBJ 0x05000200 (via ponteiro gMiniSpritePalettes) | Fight_BattleSceneInit | idem (compartilhado) |

Nota: o literal 0x08013D40 (0x0000F0FF) é máscara de DISPCNT; 0x08013DCC aponta para 0x03002C30 (bloco de OAM).

## Tela: sobreposição / tela de dados (Overlay_InitScreenGraphics 0x0803DD60)
| Endereço do asset | Formato | Tamanho (cabeçalho) | Destino | Função que carrega | Tela |
|---|---|---|---|---|---|
| 0x084EF490 | Malias | +2 = 0x2F40 | VRAM 0x06004000 (char BG0) | Overlay_InitScreenGraphics 0x0803DD60 | Sobreposição (Overlay) |
| 0x084C7C54 | Malias | +2 = 0x08C0 | VRAM 0x06007000 (char BG) | Overlay_InitScreenGraphics | idem |
| 0x084C7E7C | Malias | +2 = 0x0120 | VRAM 0x06007A00 (char BG) | Overlay_InitScreenGraphics | idem |
| 0x084C8034 | Malias | +2 = 0x09C0 | VRAM 0x06017400 (OBJ) | Overlay_InitScreenGraphics | idem (asset de loja/cena) |
| 0x084E3698 | Malias | +2 = 0x2580 | VRAM 0x06010000 (OBJ) | Overlay_InitScreenGraphics | idem |
| 0x084F0C08 | Malias (mapa) | +2 = 0x0810 | tilemap BG0 | Overlay_InitScreenGraphics | idem |
| 0x084F1734 | Malias (mapa) | +2 = 0x0810 | tilemap BG1 | Overlay_InitScreenGraphics | idem |
| 0x084F233C | Malias (mapa) | +2 = 0x0810 | tilemap BG2 | Overlay_InitScreenGraphics | idem |
| 0x084F3788 | Malias (mapa) | +2 = 0x0810 | tilemap BG3 | Overlay_InitScreenGraphics | idem |
| 0x084F4F40 | cru | 0x60 words | paleta BG 0x05000000 | Overlay_InitScreenGraphics | idem |
| 0x085104F8 | cru | 0x10 words | paleta OBJ 0x05000200 | Overlay_InitScreenGraphics | idem |

Nota: a chamada final `snd_song_request(0x14)` indica música 0x14 nesta tela (confirmado no código).

## Tela: detalhe da peça (PartDetailScreen_Init 0x0800E0BC)
| Endereço do asset | Formato | Tamanho (cabeçalho) | Destino | Função que carrega | Tela |
|---|---|---|---|---|---|
| 0x084EE0DC | Malias | +2 = 0x0FE0 | VRAM 0x06004000 (char BG0) | PartDetailScreen_Init 0x0800E0BC | Detalhe da peça (PartDetail) |
| 0x084C4BDC | Malias | +2 = 0x1080 | VRAM 0x06006000 (char BG) | PartDetailScreen_Init | idem |
| 0x084C4FBC | Malias | +2 = 0x0040 | VRAM 0x06007080 (char BG) | PartDetailScreen_Init | idem |
| 0x084C4B4C | Malias | +2 = 0x0100 | VRAM 0x06007100 (char BG) | PartDetailScreen_Init | idem |
| 0x084C4FFC | Malias | +2 = 0x0100 | VRAM 0x06007200 (char BG) | PartDetailScreen_Init | idem |
| 0x084C53C8 | Malias | +2 = 0x0380 | VRAM 0x06007300 (char BG) | PartDetailScreen_Init | idem |
| 0x084C3E64 | Malias | +2 = 0x0140 | VRAM 0x06007880 (char BG) | PartDetailScreen_Init | idem |
| 0x084C88B8 | Malias | +2 = 0x01C0 | VRAM 0x06017E00 (OBJ) | PartDetailScreen_Init | idem (asset de PartsGrid) |
| 0x084F0B44 | Malias (mapa) | +2 = 0x0810 | tilemap BG0 | PartDetailScreen_Init | idem |
| 0x084F1608 | Malias (mapa) | +2 = 0x0810 | tilemap BG1 | PartDetailScreen_Init | idem |
| 0x084F2244 | Malias (mapa) | +2 = 0x0810 | tilemap BG2 | PartDetailScreen_Init | idem |
| 0x084F3468 | Malias (mapa) | +2 = 0x0810 | tilemap BG3 | PartDetailScreen_Init | idem |
| 0x084F4BC0 | cru | 0x70 words | paleta BG 0x05000000 | PartDetailScreen_Init | idem |

Ramo `else` (byte de modo != 0) usa também 0x084F4BC0 e 0x084F3DC0-like buffers; sem assets novos.

## Pendente
- Chamadores ainda não lidos: `InitializeBattleStatusScreen` (0x08003A9C-0x08003AC8, 5 chamadas),
  `Fight_InitBotPartSelectScreen` (0x080105C6, 5 chamadas), `BattlePanel_SceneInit` (0x08063058),
  `Screen_InitNumberRevealLayout` (0x08063A8C), `Move_Character_Portraits` (0x0803F56A) e
  `LoadPortraitToVram_Unk` (0x0803F4BC): retratos/caixa de diálogo, `Menu_InfoPanelStep` (0x0800D114),
  `PartsGrid_EnterScreenInit` (0x08008810), `DebugMenu_Init` (0x0800B8E8).
- Título/logo, overworld (tileset, tilemap, sprites de personagem) e menu de campo: nenhum chamador desses
  foi achado nas xrefs de decompress_and_store (lista de 120 retornou só telas de batalha/peças/loja/cena).
  Próximo passo: xrefs de `Gfx_LoadTileAsset` 0x08000E40 e de `Assets_LoadPairAndFastCopy` 0x0803F3xx.
- Malias "+2" continua sem semântica confirmada (blocos de mapa dão 0x0810 = 2048 B de tilemap; tiles dão
  0x11A0, 0x2F40 etc., não batem com bytes exatos).
- Confirmar no emulador (ROM de debug, só leitura de VRAM) o destino de cada bloco.
