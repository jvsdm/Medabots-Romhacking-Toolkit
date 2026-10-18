# OW: sistema de mapa (overworld) — Medabots Metabee EU

Status: PARCIAL (2026-10-14). Método: decompile no Ghidra (GhidraMCP) + valores de literal pool lidos na ROM original com `python -I` (só leitura). Nada rodou no emulador nesta rodada.
Símbolos: `OW_simbolos.csv`. Nenhum rename aplicado no Ghidra: os nomes que já existem (`Overworld_*`, `Map_*`) batem com o que foi lido.

Correção de endereço: `Overworld_CheckStepAllowed` tem a ENTRADA em **0x08057D80**. O 0x08057E6A citado no pedido é um endereço no meio da mesma função (o Ghidra devolve a função inteira para os dois).

## 1. RAM do mapa (tabela)

Base IWRAM `0x030057E0` (literais de `Overworld_*`/`Map_*`). Endereço = base + offset.

| Endereço | Offset | Nome sugerido | Uso confirmado | Confiança |
|---|---|---|---|---|
| 0x030057E0 | — | gMapState (base) | base de quase todos os campos do mapa | confirmado (literais) |
| 0x03005FDE | +0x7FE | gCurrentMapId | índice de gMapArea/gMapDimensions/gMapEncounters; New Game grava 23 | confirmado |
| 0x03006409 | +0xC29 | gEncounterEnabledFlag | `Overworld_ShouldTriggerRandomEncounter` exige != 0 | confirmado |
| 0x0300640A | +0xC2A | gEncounterSlotIndex | índice 0..3 do template escolhido (`Fight_SelectTemplateForCurrentEncounter`) | confirmado |
| 0x0300640B | +0xC2B | gStepCounter | conta passos (byte); > 16 começa o sorteio | confirmado |
| 0x0300640C | +0xC2C | gEncounterBlockFlags | bit 0 = encontro bloqueado | confirmado |
| 0x0300640D | +0xC2D | gEncounterRollSuppressed | != 0 = não rola o template (e não sorteia) | confirmado |
| 0x0300640E | +0xC2E | (zerado no encontro) | reset | confirmado |
| 0x0300640F | +0xC2F | (zerado no encontro) | reset | confirmado |
| 0x03006415 | +0xC35 | gMapLoadStateA | zerado em `Map_LoadCurrentSceneResources` | confirmado (nome provável) |
| 0x03006416 | +0xC36 | gMapLoadStateB | idem | confirmado (nome provável) |
| 0x03006428 | +0xC48 | gMapSceneBlock | bloco de estado da cena; `+0xC48+0x0A` recebe pixel de x | provável |
| 0x03006432 | +0xC52 | gPlayerPixelX | calculado no load: tile_x*16 + desl. | provável (pesquisa) |
| 0x03006436 | +0xC56 | gPlayerTileX | x=18 no New Game (pesquisa) | confirmado (pesquisa) |
| 0x03006437 | +0xC57 | gPlayerTileY | y=13 no New Game (pesquisa) | confirmado (pesquisa) |
| 0x0300643C | +0xC5C | gStepCollisionIdx | byte de estado do passo | provável |
| 0x0300643D | +0xC5D | gStepKindFlags | bits de item/conjunto em `Overworld_CheckStepAllowed` | provável |
| 0x0300643F | +0xC5F | gEncounterSuppressB | != 0 = sem encontro | confirmado |
| 0x03006447 | +0xC67 | gLoadFacingSource | lido no load para `Overworld_SetPlayerFacingAndRefreshFollower` (facing inicial) | provável |
| 0x03003815 | slot 0x2F +0x25 | gPlayerLoadFlag | recebe 1 no load (`Map_LoadCurrentSceneResources`) | provável |
| 0x03006D3A | (DAT 0x03006D38+2) | índice na tabela de velocidade 0x083D2A40 (não multiplicador; OW2) | usado em `Map_GetScaledStepDelta` | conf (OW2) |
| 0x03002B64 | — | gKeysHeld | palavra de teclas lida pelo `Overworld_ProcessDirectionInput` | confirmado (literal) |
| 0x03002C30 | — | gObjectSlots | 128 slots × 0x40 B (todos os objetos do mapa) | confirmado |
| 0x030037F0 | slot 0x2F | gPlayerObject | objeto do jogador = slot 0x2F (`Map_SetObjectAnimationVariant(0x2F, facing)`) | confirmado no código |

Campos do slot de objeto (0x40 B), vistos nas funções de passo: +0x0C (X em 8.8, `>>8` = pixel), +0x10 (Y em 8.8) (CU: +0x0C = X, +0x10 = Y; ver CU.md §3c e CU2.md), +0x20 (sentido do passo: 0 N = Y-, 1 S = Y+, 2 W = X-, 3 E = X+, conforme `Map_TickObjectStep`) (CU), +0x27 (contador de quadros do passo), +0x28 (timer do passo, 16/velocidade), +0x2A (índice de velocidade na tabela ROM), +0x30 (halfword: código de pose; 0x71, 0x78, 0x82..0x84 tocam o som de passo 0x57 (CU: 0x81 não entra)), +0x04 X, +0x09 quadro, +0x18 ângulo (só na batalha, pesquisa G7). Ver §5.

## 2. Teclas e direção (confirmado no código)

`Overworld_ProcessDirectionInput` 0x08051CB8 lê `gKeysHeld` 0x03002B64, mascara D-pad e monta um nibble (RIGHT=1, LEFT=2, UP=4, DOWN=8). Facing escolhido:

- nibble 1, 5, 9 (direita, direita+cima, direita+baixo) → facing 3 (E)
- 2, 6, 10 → facing 2 (W)
- 4 → facing 0 (N)
- 8 → facing 1 (S)
- combinações com ambos os lados (3, 7, 11..15) caem no padrão 0 (N): comportamento não confirmado.

Se a direção muda, grava o facing no slot do jogador e chama `Map_SetObjectAnimationVariant(0x2F, facing)`. Depois: `Overworld_CheckMovementCollision` (limite de borda do mapa) e `Overworld_CheckMovementObjectBlocker` (outro objeto na frente). Se libera, calcula o passo e chama `Overworld_WalkStepByDirection`.

Velocidade: `Map_GetScaledStepDelta(slot)` 0x08051940 = `T[obj+0x2A] × m`, onde `T` é a tabela de 1 byte em ROM 0x083D2A40 (início: 1,2,4,0,1,2,0,4,5,6,0,8,9,10,0,0,...) e `m` = byte 0x03006D3A. O passo em frames é `0x10 / delta` (16 px por tile). Corrida = `m` maior (pesquisa: 2 px/quadro com Z/A). Tabela T: significado dos índices não confirmado.

## 3. Passo, tile e colisão (confirmado no código)

- `Overworld_WalkStepByDirection` 0x08051988 (param 0..3 = facing): move a posição de 8.8 do slot do jogador em `delta` por quadro. Ao fim do passo (contador de quadros do passo chega a 0), recalcula o tile (`ikki_x_to_tile`, `ikki_y_to_tile`) e, nesta ordem:
  1. `Map_CheckWalkOverTrigger` (gatilho de passar por cima; se houver, para);
  2. `Map_FindWarpTriggerAtTile(x, y)`: warp → som via tabela (+3 do bloco) e `Overworld_ResetAndStartScreenTransitionEffect` (transição de tela);
  3. senão, se o objeto não é o tipo 1 e `Overworld_ShouldTriggerRandomEncounter` devolve 1 → `Overworld_BuildEncounterNpcSnapshot` 0x080505FC e `GameState_StartFlowByCommand(0x21, 0)` (entra na batalha).
- Câmera: move com o jogador (`Map_AddCameraScrollX/Y`) só quando o jogador passa do centro da tela (x ≥ 0x80 px; limite direito = largura do mapa − 0xF0) e com o scroll na borda inicial 0. Lógica por eixo em `Overworld_WalkStepByDirection`. Provável para mapas maiores que a tela.
- Colisão de TILE (CORREÇÃO OW2: `Overworld_CheckStepAllowed` 0x08057D80 é do menu de 4 linhas, não da colisão; texto abaixo é histórico): `Overworld_CheckStepAllowed` 0x08057D80, parâmetro = kind 0..3, devolve 0 = pode, 1..6 = negado:
  - kind 0: exige item 0x12, conjunto de peças 0x55 e um byte em `gStepKindFlags`;
  - kind 1: item 0x18, bit 0x10 == 0;
  - kind 2: item 0x19 e conjunto 0x0B; `gMapArea[gCurrentMapId] == 0x0A` nega (passo negado; ver §4);
  - kind 3: item 0x26 e conjunto 0x6B completo (`PartSet_FindCompleteEntryIndex(0x6B)`); o tile da direção (`Map_GetAdjacentCollisionTileByDirection`) tem que ser 3 para passar, senão devolve 6.
  Ou seja: a colisão 3 da tabela (W4) só passa com o conjunto 0x6B. Detalhe do par item/conjunto por kind confirmado no código; qual objeto de mapa usa cada kind (quem chama com 0..3) não foi rastreado.
- Tabela de colisão em RAM 0x03005FF0 (pitch 0x03005FE0>>1; W2): 0 livre, 1 bloqueia, 2 porta/warp, 3 passagem condicional.

## 4. Mapa carregado e troca de mapa (confirmado no código)

`Map_LoadCurrentSceneResources` 0x0804DDCC, em ordem: `FUN_0804830C`; `Map_LoadCurrentEventObjectPalettes` 0x08048708; `Map_LoadCurrentMapLayers` 0x0804B2B0; `reset_step_and_get_encounter_flag` 0x0804387C (zera o passo); zera +0xC35/+0xC36; `Map_UpdateBackgroundAnimations`; `Map_LoadSecondaryMarkerRecord` 0x0805330C; recalcula X/Y em pixel (tile×16 + desl.); `Overworld_InitPlayerSprites` 0x080517F0; `Map_UpdatePlayerMarkerScreenPosition`; marca 1 em 0x03003815; `Overworld_SetPlayerFacingAndRefreshFollower(facing)` (facing = byte 0x03006447); `Overworld_SyncFollowerPositionAfterWarp` 0x0805309C; `Map_LoadOverworldSpriteGraphicsForCurrentMap` 0x08048288; `Event_EndOpcode`.

Tabelas por mapa (ROM):
- `gMapDimensions` 0x083F8900: largura/altura em tiles (2 B + 2 B), lida por `Map_GetMapSizeHalfTileByAxis` 0x080529AC (DT).
- `gMapArea` 0x083D2B6B (192 B): 0x0A = passo negado; leitor `Overworld_CheckStepAllowed` (literal 0x08057EA0) e `FUN_08048960` (DT3). Cópia `gMapAreaB` 0x08483774 sem leitor confirmado.
- `gMapMusic` 0x0847E644 (192 B): música por mapa (leitor `Map_LoadOverworldSpriteGraphicsForCurrentMap`, DT).
- Warp (evento 0x0B/0x0C; `Map_FindWarpTriggerAtTile` 0x08052C68): mapa, x e y do destino (ST/E2 da pesquisa).

Ordem de troca: warp → transição (`Overworld_StartScreenTransitionEffect` 0x08049D18 → `Overworld_RunScreenTransitionStep` 0x08049E08 → `Overworld_FinishTransitionInitPlayer` 0x0804A900) → recarga com `Map_LoadCurrentSceneResources`. Quem grava o id do mapa novo antes do load: não rastreado (pendente).

## 5. Objetos e NPCs (parcial)

- Tabela: gObjectSlots 0x03002C30, slot `k` em `0x03002C30 + k×0x40` (`Map_TickObjectStep` 0x0805018C usa `DAT_080501BC` = 0x03002C30).
- `Map_TickObjectStep(slot, anim)` 0x0805018C: se +0x28 != 0, move o slot com `Map_GetScaledStepDelta` no eixo indicado em +0x20 (0/1 = eixo +0x10 = Y, 2/3 = eixo +0x0C = X) (CU). Chama `Map_TickObjectAnimationTimes` 0x08050168 se `anim != 0`. Quando o contador +0x28 chega a 0 (fim do quadro do passo), toca o som 0x57 para as poses 0x71, 0x78, 0x82..0x84 (CU: 0x81 não entra) , recalcula +0x28 = 16/delta e decrementa +0x27; devolve 1 quando o passo termina.
- `Map_ObjectAnimStep` 0x080508B4: máquina de 2 estados no bloco `DAT_080508C8` (byte +0 = estado; byte +10 = índice do objeto; +2 = quadros restantes). Estado 0: `Overworld_GetObjectAnimationIndexForFacing` (0x08050300) escolhe a animação pelo facing (+0x20 do objeto) e passa ao estado 1. Estado 1: `Map_TickObjectAnimation` 0x08049440 até +2 zerar, então devolve 1.
- `Map_SpawnTrackedObjectAtTile` 0x08050358 e `Map_StartObjectStep` 0x080500BC: nascem/iniciam passos (eventos 0x1F/0x21/0x23/0x24).
- Ator de andar com o jogador: `Overworld_UpdateFollowerFacing` 0x08051EFC e `Overworld_AdvanceWalkStep` 0x0805203C (passo do companheiro; não lido em detalhe).

## 6. Encontro aleatório (confirmado no código)

1. `Overworld_ShouldTriggerRandomEncounter` 0x080438C4 (chamada a cada passo; ver §3): só roda se +0xC29 != 0, +0xC2D == 0, +0xC5F == 0 e bit 0 de +0xC2C == 0. Soma 1 em +0xC2B; se > 16 e (> 31 ou `randbyte() & 0x1F == 0x1F`), zera +0xC2B, +0xC2E e +0xC2F e devolve 1. Entre 17 e 31 passos: 1/32 por passo; a partir de 32: certo. RNG = `randbyte` 0x080424B8 (R2).
2. Sorteio de template: `Fight_SelectTemplateForCurrentEncounter` 0x08011E20. Se +0xC2D == 0, lê 4 bytes de `gMapEncountersExtra` 0x083BF210 + `gCurrentMapId`×4 (mapa 23 = zeros: sem encontro), e sorteia `Encounter_RollPercent0To99` 0x08043C94 (`randbyte()×100/255`): < 40 → entrada 0; < 70 → 1; < 90 → 2; senão → 3 (pesos 40/30/20/10). Grava o índice em +0xC2A. Depois copia 0x28 B do template de `0x083C1B80[índice]` para o buffer `0x03001800` (fight_information) e, para cada um dos 3 blocos de 0xC B, sorteia a peça com `Fight_SelectTierPartId`.
3. `Fight_CopyEncounterTemplateEntry` 0x08011DE4 copia 2 B de `gMapEncounters` 0x083BEC10 (`gCurrentMapId`×8) para `DAT_08011E18`, e grava o byte +1 em `fight+0x38` (nível/faixa do encontro, provável).
4. Observação: a flag de encontro de `gMapEncounters` (byte +0) e o "área" (+1) continuam só provável (DT).

## 7. Pendente

- `gMapEncounters` +0/+1 (quem consome a flag de área e o que é +1 além de +0x38 da luta).
- Índices da tabela de velocidade 0x083D2A40 (+0x2A do objeto) e o byte 0x03006D3A (índice da mesma tabela, não multiplicador; OW2).
- Tradução do nibble de D-pad para combinações diagonais/opostas (3, 7, 11..15).
- Quem chama `Overworld_CheckStepAllowed`: resolvido (OW2, FL7): é a tela do menu de 4 linhas, não objetos de mapa.
- Quem grava o id de mapa antes da troca de mapa (warp).
- Câmera: `Map_CenterCameraForSmallMaps` 0x080528F8 e `Map_ClampCameraToLoadedMap` 0x08052AA4 não decompilados nesta rodada.
- Ghidra: nenhum rename feito; a função de `Overworld_CheckMovementCollision` (0x08052CC0) não testa tile e sim a borda do mapa; o nome atual serve, mas o comentário do `mtbe.csv` deve ser corrigido (W2 dizia o contrário).

## 8. CHAPENC (2026-10-18): quem grava cada flag de encontro, e se depende de capítulo (Ghidra xref + ROM + events.lua)

- **E+0xC29 (0x03006409)**: único escritor = `Overworld_ResetEncounterStepState` 0x0804387C (xref 0x0804389E). Grava
  `gMapEncounters[gCurrentMapId].+0`. Só o mapa decide: sem capítulo, sem flag.
- **E+0xC2C (0x0300640C)**: escritores = `Event_Op5F` 0x0804CF40 (WRITE 0x0804CF4C, byte inteiro) e `Debug_RndTkScreen`
  (debug, inverte bit 0). Único leitor de encontro = `Overworld_ShouldTriggerRandomEncounter` (0x080438EA), e só o bit 0.
  Não é zerado na carga de mapa: vale até o próximo 0x5F. Opcode 0x5F: 19 usos em 16 eventos de `pc/cache/events.lua` (ids 40, 271,
  774, 780, 1182, 1207, 1813, 1823, 1828, 1829, 1830, 1902, 2061, 2062, 2063, 2072),
  valores só **0 ou 1**: bit 0 = bloqueado. Bits 1..7 nunca são lidos por encontro.
- **E+0xC2D (0x0300640D)**: escritores = `GameState_StartFlowByCommand` (0x080479AA, 0x080479D8: 1 no cmd 0x20 = história,
  0 no 0x21 = aleatório) e resets de tela `Gfx_ResetBgLayersAndSong` 0x0801156E, `Gfx_ResetScreenBgAndBlend` 0x08011C20,
  `Menu_TickSingleCountSelector` 0x0800D392 (valor gravado pelos resets não conferido). Com != 0 o encontro inteiro
  fica desligado (ShouldTrigger), não só o sorteio.
- **E+0xC5F**: sem novo escritor de encontro; já documentado em EV.md (passo de cena).
- **gMapEncounters +1 (área)**: gravado em `fight+0x38` (0x03001838) por `Fight_CopyEncounterTemplateEntry` (0x08011E04).
  Leitores de `fight+0x38` (Ghidra xref): `Fight_DispatchOnSubstateAA_G_03_BuildBattleBackground` 0x0801E10C,
  `Fight_AddBotMeterFromTable` 0x080191FC, `Fight_InitBotPartPanel` 0x0801187C, `Fight_StoreBotLabelText` 0x080121B0 e
  outros. Provável: índice de cenário/área da batalha. NÃO confirmado.
- **gMapEncounters e gMapEncountersExtra** (0x083BEC10 / 0x083BF210): índice = só `gCurrentMapId` (E+0x7FE). Nenhum
  leitor depende de capítulo ou de flag. Os leitores são por literal (sem xref estática): `Fight_CopyEncounterTemplateEntry`
  0x08011E14, `Overworld_ResetEncounterStepState` 0x080438B4, `Fight_SelectTemplateForCurrentEncounter` 0x08011E64.
- **Porte**: `encounter.enterMap` (C29 = map_encounter[n].flag); `encounter.step(state, rng, C2C)` lê o bit 0 de
  `script.save.encounterFlags`; C2D é ligado em `host.battle` e zerado em `world.resume` (provável, reset de tela).
