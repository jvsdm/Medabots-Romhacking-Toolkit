# MP: formatos de dados do mapa, como o JOGO os lê (Medabots Metabee EU)

Status: PARCIAL (2026-10-14, GH-MP). Método: decompile no Ghidra (`Map_LoadCurrentMapLayers`, `Map_GetAdjacentCollisionTileByDirection`, `Map_GetMapSizeHalfTileByAxis`, `Map_FindWarpTriggerAtTile`), literais lidos na ROM original com `python -I` (só leitura) e checagem de 192 mapas por script (sem extrair imagem). Nada rodou no emulador. Nenhum commit. Ghidra não foi salvo nem fechado; nenhum rename aplicado no programa (ver "Renames").
Símbolos: `MP_simbolos.csv`. Base de RAM do mapa: gMapState `0x030057E0` (endereço = base + offset).

## 0. Resumo (o que muda em relação aos docs anteriores)

1. Unidade das dimensões é 8 px (meio tile). `gMapDimensions` 0x083F8900 guarda (largura, altura) em tiles de 8 px. Mapa 10 = 24x22 → 12x11 metatiles de 16 px = 192x176 px. Isto fecha a pendência de DT.md ("unidade do leitor fica em aberto").
2. Colisão = 1 byte por metatile 16x16, bruta (sem LZ77), tabela de ponteiros em 0x08404334 (4 B por mapa). Valores 0 livre, 1 bloqueio, 2 porta/warp, 3 passagem condicional (presente em 23 mapas). Confirmado no código e nos dados.
3. Camadas de tiles = 3 camadas BG (tilemaps GBA de 8x8), cada uma LZ77, ponteiros em 0x083F9080 (12 B por mapa = 3 ponteiros). Não é 4 B por mapa como diz GF3.md.
4. 0x08404334 não é "camadas" (GF3.md errou): é a tabela de colisão.
5. 0x083F8A80 é o tileset (4 B por mapa) que vai para VRAM 0x06009000. GF3.md dizia 0x06003000 (errado: 0x06003000/0x06005000/0x06007000 são os destinos das 3 camadas).
6. Há uma 4a tabela opcional 0x083F8D80 (4 B por mapa, LZ77) que o jogo descomprime para o ponteiro guardado em 0x030057DC. Nenhum doc e nenhum extrator cita.

## 1. Mapa carregado: `Map_LoadCurrentMapLayers` 0x0804B2B0 (confirmado)

Id do mapa = `gCurrentMapId` 0x03005FDE; `m` abaixo.

| Passo | Endereço RAM | Conteúdo | Fonte |
|---|---|---|---|
| dims | base+0x800 (0x03005FE0) largura, base+0x802 (0x03005FE2) altura | em 8 px | `Map_GetMapSizeHalfTileByAxis` 0x080529AC |
| origem do scroll | base+0x804 (X), base+0x806 (Y) | `Map_GetInitialScrollTileOrigin` (não decompilado) | confirmado (escrita) |
| colisão | base+0x810 (0x03005FF0) | cópia bruta de `ROM[gMapCollisionPtrTable[m]]`, tamanho `(w>>1)*(h>>1)` bytes | confirmado |
| camadas | VRAM por camada (ver §2) | LZ77 de `ROM[0x083F9080 + 12*m + 4*k]` | confirmado |
| tileset | VRAM 0x06009000 | LZ77 de `ROM[0x083F8A80 + 4*m]` | confirmado |
| 4a tabela | ponteiro RAM em 0x030057DC | LZ77 de `ROM[0x083F8D80 + 4*m]` (só se ≠ 0) | confirmado (código); conteúdo não lido |
| fallback | se layer0 == 0 ou tileset == 0 | não usa LZ77: preenche com zeros e chama `Map_ExpandMetaTilemapToBgScreenblocks` com dados fixos (literais DAT_0804b5xx) | confirmado (código) |

No fim: `Map_ClampCameraToLoadedMap` e `Overworld_UpdateSecondaryActorAnim`.

Literais usados (ROM): 0x0804B440 → 0x08404334 (colisão), 0x0804B444 → 0x083F9080 (camadas), 0x0804B448 → 0x083F8A80 (tileset), 0x0804B44C → 0x06009000, 0x0804B450 → 0x03006D14 (buffer de descompressão), destinos 0x06003000 / 0x06005000 / 0x06007000, 0x0804B460 → 0x083F8D80, 0x0804B464 → 0x030057DC.

## 2. Camadas de tiles (confirmado)

- Entrada por mapa = 3 ponteiros u32 em `0x083F9080 + 12*m` (camada 0, 1, 2). Ponteiro 0 = camada ausente.
- Cada camada descomprimida (LZ77, `bios_lz77_vram`) tem cabeçalho de 16 B:
  - u16 @0 = largura lw (em tiles de 8 px); u16 @2 = 0 (não usado); u16 @4 = altura lh; u32 @8 = valor não entendido (ex.: 287 no mapa 10, 815 no mapa 0; não é o tamanho).
  - Entradas de tilemap GBA, u16 cada, row-major a partir do byte 16: `tile (10 bits) | flipH (bit 10) | flipV (bit 11) | paleta (bits 12..15)`. Tamanho total confere com 16 + 2·lw·lh em todos os mapas testados (0, 10, 23, 30).
- Destino de cada camada: camada 0 → VRAM 0x06003000 (SBB), camada 1 → 0x06005000, camada 2 → 0x06007000. Tiles (gráficos) em 0x06009000.
- Largura da camada ≠ largura do mapa em mapas pequenos: mapa 10 tem dims 24x22 mas a camada é 30x22 (lw=30). Mapa 0 (58x56) e mapa 23 (58x32) têm lw = largura do mapa. Isto bate com a regra de centralização de W3 (origem ((30−w)//2, ...) em `extract_map.py`).
- Não confirmado: a relação entre o `u32 @8` e o conteúdo.

## 3. Colisão (confirmado)

- Tabela de ponteiros: `ROM[0x08404334 + 4*m]` (u32). Bloco de `(w>>1)*(h>>1)` bytes sem compressão. Index = `y*pitch + x`, pitch = `w>>1` (metatiles por linha), x e y em metatiles 16 px.
- Valores lidos na ROM (192 mapas, soma): 0 → 22190, 1 → 24655, 2 → 464, 3 → 1493. Mapas com 3: 31, 33, 34, 36, 37, 40, 42, 43, 44, 57, 71, 72, 83, 122, 142, 143, 151, 162, 163, 164, 176, 188, 189.
- `Map_GetAdjacentCollisionTileByDirection` 0x080524C8 (param: direção 0..3) lê o metatile adjacente ao jogador. Posição = `gPlayerTileX` 0x03006436 e `gPlayerTileY` 0x03006437 (metatiles 16 px):
  - 0 = N (y−1), 1 = S (y+1), 2 = W (x−1), 3 = E (x+1). Mesmos códigos de facing de `Overworld_ProcessDirectionInput` (OW2).
  - Não faz checagem de limite: na borda lê o byte anterior/seguinte do buffer (observação, não testada).
- Quem consome a direção e o valor 3 (passagem condicional, item/conjunto 0x6B): `Overworld_CheckStepAllowed` 0x08057D80 é o menu de 4 linhas (OW2 passo 8), então a ligação com o valor 3 não está fechada. OW.md §3 continua pendente.

## 4. Warps: `Map_FindWarpTriggerAtTile` 0x08052C68 (confirmado)

- `gWarpPtrTable` = 0x08412E60: u32 por mapa (índice = `gCurrentMapId`). Lista de entradas de 8 B, termina quando o byte 0 = 0xFF.
- Campos (conferidos nos mapas 0, 10 e 23):
  - +0 x e +1 y (metatile 16 px) de origem.
  - +2 id do mapa destino.
  - +3: nibble alto = som/tipo do passo (guardado em estado+3 pela função); nibble baixo (&7) = facing ao chegar.
  - +4, +5: não decodificados (valores sem padrão óbvio ligado ao destino). `extract_objects.py` também marca como "U4, U5".
  - +6 x e +7 y de destino (metatile 16 px).
- A função percorre a lista inteira e o último match vence (não para no primeiro).
- Exemplo confirmado: mapa 23, entrada [18,14] → mapa 0, destino (18,5) (ver OW2 passo 7).

## 5. Dimensões e origem (confirmado)

- `gMapDimensions` 0x083F8900: 2 B por mapa (largura, altura) em 8 px. Lido por `Map_GetMapSizeHalfTileByAxis(m, eixo)` 0x080529AC: ROM[0x083F8900 + 2m + eixo]. Não faz conversão: o nome "HalfTile" é a unidade (8 px), e o valor cru já é a contagem.
- Mapas: 0 = 58x56; 10 = 24x22; 23 = 58x32; 30 = 20x18.

## 6. Pessoas / spawns de evento (8 B, parcial: só pelos dados e pelo extrator)

- Ponteiro por mapa: `0x08483234 + 4m` (u32). Lista de registros de 8 B, termina em 0xFF (extrator `PEOPLE_PTR_TABLE`; leitor no Ghidra não feito nesta rodada).
- Campos (conferidos em mapas 0, 10, 23):
  - +0 x, +1 y (metatile 16 px).
  - +2..+3 u16 LE: `kind << 12 | evento` (ex.: mapa 23, pessoa em (18,13) = kind 8, evento 185, o evento de abertura citado em pc/research).
  - +4: `sprite << 4 | facing (2 bits)` (0xFF = sem sprite).
  - +5: grupo (0xFF = nenhum).
  - +6..+7 u16: máscara de capítulo (0x0003, 0x01FF, 0x03FF...).
- Os 0x1F/0x21 de script criam atores por slot (`extract_events.py`); são outra coisa.

## 7. Paleta, sprites, música, área

- Paleta por mapa: `0x083F9980 + 4m` = ponteiro para 0x200 B (16 bancos x 16 cores, BGR555). Extrator usa (`extract_map.py`). Não confirmado no Ghidra nesta rodada (não está em Map_LoadCurrentMapLayers).
- Sprites do mapa (overworld): `0x083F7E80` = tabela de ponteiros (≥14 entradas, 4 B cada; primeiros: 0x085D7018, 0x085FB35C, 0x085D7454, 0x085D7894). Índice por mapa: `0x08413160 + m` (byte; 0x60 no mapa 0). Leitor `Map_LoadOverworldSpriteGraphicsForCurrentMap` 0x08048288 (GF3.md).
- Música: `gMapMusic` 0x0847E644 (1 B por mapa). Mapa 23 = 26 (confere com DT.md).
- Área: `gMapArea` 0x083D2B6B (192 B). Mapas 0..15 e 10 = 0 nesta amostra. Valor 0x0A = passo negado (OW.md). Não reconferido nesta rodada.

## 8. Divergências (apontadas, nenhum arquivo do PC editado)

1. GF3.md (tabela Overworld): 0x083F9080 descrita como "ptr tiles BG, 4 B por entrada, VRAM 0x06009000" → errado: é tabela de 12 B (3 camadas), e as camadas vão para 0x06003000/5000/7000. 0x083F8A80 descrita como "VRAM 0x06003000" → errado: é o tileset, vai para 0x06009000. 0x08404334 descrita como "camadas" → errado: é colisão.
2. OW.md §1 e DT.md (gMapDimensions): unidade "em aberto" → resolvida: 8 px.
3. pc/extract/extract_map.py: LAYERS (12 B), GFX (4 B), DIMS e origem da camada batem com o código. Divergências: (a) não usa a 4a tabela 0x083F8D80 (LZ77 para 0x030057DC): pode faltar uma camada/tile extra; (b) a paleta 0x083F9980 não foi conferida no Ghidra.
4. pc/extract/extract_objects.py: colisão (`(w//2) x (h//2)`, bruta, 1 B por metatile), warps (8 B, `kind<<4 | facing&7`) e dims batem com o código. Pessoas (0x08483234, 8 B) conferidas só pelos dados, não no Ghidra.
5. Origem do mapa: o extrator põe a origem da camada (regra 30x20) só na imagem; a colisão fica em coordenadas do mapa (copiada direto de `m`), sem offset. Bate com o código.

## 9. Pendente

- `Map_GetInitialScrollTileOrigin` (origem do scroll) não decompilado: confirmar a regra ((30−w)//2) no código.
- Conteúdo da 4a tabela 0x083F8D80 (o que vai para 0x030057DC).
- Campos +4/+5 do warp e u32 @8 do cabeçalho de camada.
- Paleta 0x083F9980: quem a carrega (não é Map_LoadCurrentMapLayers).
- Pessoas 0x08483234: leitor no Ghidra (não feito).
- Valor 3 da colisão: ligação com a checagem de passo (OW2 diz que CheckStepAllowed é menu; a passagem condicional precisa de outra checagem).
- Checagem de limite em `Map_GetAdjacentCollisionTileByDirection` (leitura fora do buffer na borda).
- Testes no emulador: não feitos (regra desta tarefa).

## Renames

Nenhum rename foi aplicado no programa Ghidra (agentes em paralelo; regra: não salvar). Nomes propostos estão em `MP_simbolos.csv`; aplicar depois, quando o Ghidra estiver livre.
