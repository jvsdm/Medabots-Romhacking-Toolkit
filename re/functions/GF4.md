# GF4: catálogo de gráficos, parte 4 (título, grade de peças, batalha, número, DebugMenu, peça grande)

Status: PARCIAL (2026-10-14). Só endereços e metadados lidos na ROM original (`Medabots - Metabee (Europe).gba`,
só leitura, python -I, literais do pool e cabeçalhos). Nenhuma imagem extraída. Continua `GF.md`, `GF2.md`, `GF3.md`.
Convenção: "Malias +2" = u16 do cabeçalho (semântica ainda não confirmada); "LZ77 size" = 24 bits após o byte 0x10.

## Correções aos endereços listados
- `DebugMenu_Init` = **0x0800B8B0** (corpo 0800B8B0-0800B909; Ghidra e DB.md). 0x0800B8E8 é só o `bl` (decompress_and_store) dentro dele.
- 0x08063A8C não é entrada de função; é o `bl` decompress_and_store dentro de `Screen_InitNumberRevealLayout` 0x080639C4 (corpo 080639C4-08063EA3).
- 0x0805BA34 = `Screen_LoadLZ77TileBlock` (corpo 0805BA34-0805BA95); 0x08044D0C = `Asset_UnpackOrCopyBlock` (08044D0C-08044D5B). Ambos já estavam no catálogo do Toolkit.
- 0x0801058C `Fight_InitBotPartSelectScreen`: corpo 0801058C-080106E9 (confirmado no Ghidra).
- `LoadPartLargeDisplaySpriteFromAppearanceTable` = 0x08046A60 (corpo 08046A60-08046CC5).

## Cabeçalhos da tela de título (cena 2, IN1/IN2)
| Endereço | Formato | Tamanho | Destino | Carregador | Tela | Confiança |
|---|---|---|---|---|---|---|
| 0x087BB2A0 | LZ77 0x10 | 0x000500 (1280 B) | VRAM 0x06007000 (QA3: pool 0x0806D86C; par de `bios_lz77`) | Title loader 0x0806D7A0 | Título: mapa BG | confirmado na ROM (QA3). Trace `IN1_swi_calls.txt` linha 70 (0x06006D00) não bate com o código: não usar como prova |
| 0x087BB7E0 | **sem cabeçalho LZ77 em +0** | — | trace (IN1 linha 71) diz VRAM 0x06007500 | Title loader | Título: mapa BG | **ANOMALIA**: +0..+0x1F são 0x00, depois 0x01 (1883 de 2048 bytes não nulos). Não é início de bloco; provável ponteiro errado ou meio de bloco. Não rotular até conferir no emulador (RAM/SWI) |

## Grade de peças: paleta 0x084F4840
- Carregada por `PartsGrid_EnterScreenInit` 0x080087CC (corpo 080087CC-08008907): `CpuFastSet(0x084F4840, r1 = 0x0A0<<19 = 0x05000000, r2 = 0x70)`.
- Destino: **paleta BG 0x05000000** (banco 0), 0x70 words (224 bytes = 7 bancos de 16 cores). Mesmo formato e destino da `gGfx_PartsGrid_Palette_Banks` 0x084F4300 de `PartsGrid_InitScreen` (GF.md): provavelmente outro lote da mesma grade (conferir se não sobrescreve o 0x084F4300).
- Confiança: confirmado (literal + r1 no disasm em 0x0800886C).

Outras cargas de `PartsGrid_EnterScreenInit` (sem tabela antes): Malias 0x084EC1F8 (+2 0x2B00) -> VRAM 0x06004000; 0x084C3A24 (0x0340) -> 0x06007480; 0x084C3E64 (0x0140) -> 0x06007880; 0x084C88B8 (0x01C0) -> 0x06017E00; tilemaps 0x084F0A80, 0x084F13C8, 0x084F1FA0, 0x084F3140 (0x0810) via LoadTilesWithMalias; paletas cruas 0x0850F2B8 -> 0x05000380 e 0x050003A0; 0x084E43F8 -> 0x050003C0.

## Batalha: painel (BattlePanel_SceneInit 0x08062FB4, corpo 08062FB4-08063185)
Confiança: confirmado nos literais e chamadas. A ordem de r2 (BGCNT) antes do 0x0805BA34 não foi fechada (os valores 0x1E05/0x1B06/0x3C07/0x2FF/0x1D06 parecem BGCNT/DISPCNT, não confirmado).

| Endereço | Formato | Tamanho | Destino | Carregador | Tela |
|---|---|---|---|---|---|
| 0x0879EFC0 | LZ77 0x10 | 0x000810 (2048 B mapa) | Screen_LoadLZ77TileBlock 0x0805BA34 | BattlePanel_SceneInit | Batalha: painel, mapa BG |
| 0x0879F138 | LZ77 0x10 | 0x000810 (2048 B mapa) | idem | BattlePanel_SceneInit | Batalha: painel, mapa BG |
| 0x0879F3D0 | LZ77 0x10 | 0x000810 (2048 B mapa) | idem | BattlePanel_SceneInit | Batalha: painel, mapa BG |
| 0x0879DA90 | LZ77 0x10 | 0x002FE0 | Asset_UnpackOrCopyBlock 0x08044D0C: buffer 0x02030000 -> VRAM 0x06004000 (0x0807CCC4) | BattlePanel_SceneInit | Batalha: painel, tiles |
| 0x08785EC0 | Malias +2 0x03E0 | — | decompress_and_store; destino não fechado (r1 sobrescrito pelo bl anterior) | BattlePanel_SceneInit | Batalha: painel |
| 0x08785FB4 | Malias +2 0x3A80 | — | VRAM 0x06010000 (OBJ) | BattlePanel_SceneInit | Batalha: painel (mesmo da loja, GF.md) |
| 0x084C8034 | Malias +2 0x09C0 | — | VRAM 0x06011E00 (OBJ) | BattlePanel_SceneInit | idem |
| 0x0879D890 | cru | — | buffer 0x0202F9A0 | BattlePanel_SceneInit | idem |
| 0x0850F318 | cru | — | paleta 0x050003A0 | BattlePanel_SceneInit | idem |

Nota: 0x0879F138 e 0x0879F3D0 têm o mesmo cabeçalho (`10 10 08 00`) de 0x0879EFC0; são o trio de mapas do painel, com destinos de BGCNT ainda não fechados.

## Batalha: tiles da tela de peças do bot (Fight_InitBotPartSelectScreen 0x0801058C)
Confiança: confirmado nos literais (corpo 0801058C-080106E9).

| Endereço | Formato | Tamanho (+2) | Destino | Tela |
|---|---|---|---|---|
| 0x084E823C | Malias | 0x0AE0 | VRAM 0x06004000 (char BG) | Batalha: tela de peças do bot |
| 0x084C50D0 | Malias | 0x0200 | VRAM 0x06006000 | idem (asset de batalha, GF2) |
| 0x084C5250 | Malias | 0x0200 | VRAM 0x06006200 | idem |
| 0x084C92B0 | Malias | 0x0800 | VRAM 0x06012800 (OBJ) | idem |
| 0x084C8034 | Malias | 0x09C0 | VRAM 0x06017400 (OBJ) | idem (asset de loja) |
| 0x084F0FD4 | Malias (mapa) | 0x0810 | LoadTilesWithMalias 0x08000E04 (tilemap BG) | idem |
| 0x0850F498 | cru | 0x40 words | paleta OBJ 0x05000200 (via buffer) | idem (compartilhado, GF2) |
| 0x084F3F80, 0x084C7F74, 0x084C7F94 | cru | — | buffers 0x0202FA00, 0x0202FB00, 0x0202FB20 (CpuFastSet) | idem (dados de limpeza/paleta, não lidos) |

Fight_LoadBattleBotTilesSet 0x08015A90 (corpo 08015A90-08015AE5) confirma as 4 entradas já em GF.md: 0x084882CC (0x0070, bloco 0x0D), 0x084881E0 (0x0038, 0x11), 0x08488240 (0x004C, 0x0F), 0x08488290 (0x0060, 0x10), todas via LoadTilesWithMalias 0x08000E04.

## Número de revelação (Screen_InitNumberRevealLayout 0x080639C4, corpo 080639C4-08063EA3)
Confiança: confirmado nos literais; destinos de BGCNT pendentes.
- LZ77 0x10 tilemaps (0x000810): 0x087A0FB4, 0x087A0D04, 0x087A1244, 0x087A10F0 via Screen_LoadLZ77TileBlock 0x0805BA34.
- LZ77 0x10 tiles 0x0879F80C (0x002EC0) via Asset_UnpackOrCopyBlock -> VRAM 0x06004000.
- Malias 0x084C3A24 (0x0340) -> VRAM 0x06004080; Malias 0x084C8034 (0x09C0) -> VRAM 0x06017400 (OBJ).
- Malias 0x08785EC0 (0x03E0) via decompress_and_store (destino não fechado, igual ao do painel).
- Paletas cruas: 0x0879F6CC e 0x0850F318 -> 0x050003A0 e buffer 0x0202F9A0; 0x084E4438 -> 0x05000200 / 0x05000220 / 0x05000240 (CpuFastSet, 0x10 words cada).
- Dígitos: tiles em VRAM 0x0600021A/0x0600025A/0x0600021C/0x0600025C (OBJ) e 0x06000206/0x06000246/0x06000248/0x06000218/0x06000258 (BG) (alvos de cópia de dígitos, não confirmado o conteúdo).
- Música 0x18 (N2).

## DebugMenu_Init (0x0800B8B0, corpo 0800B8B0-0800B909)
Confiança: confirmado (literais). Ordem: DMA3 limpa OAM (fonte 0x85000800); copia OAM 0x03002C30 -> 0x02020800 (DB.md); Malias 0x084C8034 (+2 0x09C0) -> VRAM 0x06017400 (decompress_and_store); paleta 0x084E43D8 -> OBJ 0x050003E0, 8 words (32 bytes = 16 cores; DMA3 CNT 0x84000008). Confirmar no emulador (ROM de debug).

## Peça grande (LoadPartLargeDisplaySpriteFromAppearanceTable 0x08046A60)
Confiança: confirmado no Ghidra (decompile). NÃO é um bitmap: é um renderizador de descritores compostos.
- `param_4` escolhe o tamanho e a tabela (entrada de 0x10 bytes, exceto tamanho 3 com 0x20; índice = `param_3 * 0x10` ou `* 0x20`):
  - 0 -> tabela 0x0807EE70, `local_28 = 0x140`
  - 1 -> tabela 0x0807F610, `0x1A0`
  - 2 -> tabela 0x0807FDB0, `0x1E0`
  - 3 -> tabela 0x08080550 (entrada de 0x20), `0x220`
  (ponteiros dos literais DAT_08046AAC/AC0/AD4/B10.)
- Se `param_9 & 4 == 0`: só marca estado no struct (ponteiro IWRAM 0x03005308). Senão desenha: `PartPreviewPiece` (BL 0x08045E6C) ou `RenderCompositePreviewDescriptorRecursive`; para tamanhos 1 e 2, `CopyCompositePreviewDescriptorGraphics` por descritor (BL 0x08046034).
- Chamada por: PartsGrid_RedrawLargeDisplay (R0.md), PartsGrid_DrawLargeDisplayIfOwned (R0.md), PartPreview_InitScreenBuffers (R0.md) e PartPreview_LoadLargeDisplaySprite 0x08044934 (R2a.md).
- Os desenhos grandes da peça ficam nessas tabelas de descritor (0x0807EE70..0x08080550; tamanho das tabelas não contado). Não é asset Malias: não entra na tabela de gráficos.

## Ícones de peça / medalha
Pendente: não lidos nesta passada. Ícone de perna: `PartDetail_GetCurrentSelectionLegIconIndex` (R0.md, índice +0xEF do struct; >= 100 usa outra faixa). A faixa 100+ e a tabela de ícones de medalha (gMedals 0x083B62D8, ver pc/research/README.md) não foram abertas.

## Nomes aplicados no Ghidra (rename_data)
Ver `GF4_simbolos.csv`. Status conferido apenas pelo retorno da ferramenta.

## Pendente
- 0x087BB7E0: confirmar no emulador (RAM/SWI) o endereço de origem real (sem cabeçalho na ROM).
- BGCNT de 0x0879EFC0/0x0879F138/0x0879F3D0 e de 0x087A0FB4..0x087A10F0 (Screen_LoadLZ77TileBlock): confirmar no emulador.
- Destino do decompress de 0x08785EC0 (painel e número): r1 sobrescrito.
- Tabelas de descritor 0x0807EE70/0x0807F610/0x0807FDB0/0x08080550: tamanho e formato (PartPreviewPiece).
- Ícones de peça/medalha: não iniciado.
- Gfx_LoadTileAsset 0x08000E40 e demais pendências de GF/GF2/GF3 seguem nos respectivos arquivos.
