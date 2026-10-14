# OW2: overworld, continuação (GH-OW2)

Status: PARCIAL (2026-10-14, GH-OW2). Base: OW.md. Símbolos: OW2_simbolos.csv. Pendente no fim do arquivo.

## Passo 1: xrefs e câmera (Ghidra)

- xref de 0x03006D3A: só `Map_GetScaledStepDelta` (0x08051954, READ) e um dado em 0x083D62A4 (literal). Não há escritor no código: o byte é gravado por outra via (RAM zerada no New Game? não medido). Ver §3.
- xref de 0x03005FDE (gCurrentMapId): escritores diretos = `Map_FinishEnterTransition` 0x0804B036 (WRITE) e `new_game_start` 0x0804A440 (WRITE, grava 23), `Script_ReadStateBlock` 0x0804B9F4 (WRITE, bloco de estado de save).
- `Map_CenterCameraForSmallMaps` 0x080528F8 (decompile): para cada eixo, se o mapa tem menos que a tela (X: tiles/2 < 15; Y: tiles/2 < 10), centraliza: scroll = (15 − metade) × 8 px (ou (10 − metade) × 8). Senão scroll = 0. Ou seja, o scroll fica em unidades de 8 px. Também faz `bios_lz77_vram` do bloco de tiles.
- `Map_ClampCameraToLoadedMap` 0x08052AA4 (decompile): alvo X = (byte do jogador − 7) limitado a ≥ 0; o limite superior é metade-tile do mapa − 15 (só quando o mapa é mais largo que a tela, e só se `Map_GetCameraScrollX()` == 0). Y idem com (byte − 4) e limite −10. Grava `Map_SetCameraScrollX((x & 0xFFF) << 4)` e `Map_SetCameraScrollY(...)`. Unidade final: px (tile << 4).
  Leitura: a câmera acompanha o jogador centrado com 7 tiles de folga em X e 4 em Y; o clamp só age quando scroll = 0 (ou seja, o código tem um caminho que zera o scroll quando o mapa cabe). Confirmar com os literais DAT_08052B34/38/3C/40 antes de fechar.
  Confiança: provável (lógica lida no decompile; literais não lidos ainda).

## Passo 2: velocidade e D-pad (Ghidra decompile, lido)

- `Map_GetScaledStepDelta` 0x08051940: `delta = B[obj+0x2A] * B[RAM[DAT_08051968+2]]`, com a MESMA base de tabela B nos dois termos (base = literal DAT_08051960). Se o literal DAT_08051968 for 0x03006D38, o segundo termo é `B[ RAM 0x03006D3A ]`: o byte 0x03006D3A é ÍNDICE na tabela, não multiplicador direto. Correção ao OW.md §2 e §1 ("multiplicador"). Literais ainda a confirmar (ver passo 3).
- `Overworld_ProcessDirectionInput` 0x08051CB8: nibble = bit4→1 (RIGHT), bit5→2 (LEFT), bit6→4 (UP), bit7→8 (DOWN) de gKeysHeld. Facing por `switch`: 1,5,9→3 (E); 2,6,10→2 (W); 4→0 (N); 8→1 (S); QUALQUER OUTRO valor (3, 7, 11, 12, 13, 14, 15) mantém facing 0 = N. Ou seja, L+R e U+D juntos NÃO param: andam para o NORTE (facing 0), sujeito a `Overworld_CheckMovementCollision(0)` e `Overworld_CheckMovementObjectBlocker(0)`. Confiança: confirmado (lido no decompile); execução real não testada.
- Se o estado 0x03006D38+? (flag em DAT_08051d38) está ligado, o passo é forçado: `Overworld_WalkStepByDirection(dir de DAT_08051e30)` + `Overworld_AdvanceWalkStep` (caminho de script/evento; não é D-pad).
- Ao mudar facing: grava no slot 0x2F, `Overworld_SetPlayerFacingAndRefreshFollower`, `Map_SetObjectAnimationVariant(0x2F, facing)`, `Overworld_UpdateFollowerFacing(3)`.
- Ao andar: `div(0x10, delta)` = frames por passo (grava em RAM, base DAT_08051dfc+DAT_08051e04); som 0x57 quando o byte de passo é 2 ou 9; depois `Overworld_WalkStepByDirection(facing)` e `Overworld_AdvanceWalkStep`.

## Passo 3: Overworld_WalkStepByDirection 0x08051988 (lido)

- Facing 0 (N) e 1 (S): move o eixo Y do jogador por ±delta a cada quadro; facing 2 (W) e 3 (E): eixo X. Enquanto o contador de passo (byte em obj+0x28 por DAT_08051c04) não zera, anda; o contador é decrementado por quadro.
- Câmera: move a scroll só se o jogador passa do centro (E: centro_jogador−? > 0x80 px; W: a condição é < 0x51) E o limite do mapa não foi atingido (E: largura do mapa − 0xF0 > posição de scroll; N/S analogamente com 0xA0 e 0x60). Ao mesmo tempo a câmera só se move (chama `Map_AddCameraScrollX/Y(±delta)`) quando a leitura de `Map_GetCameraScrollX/Y()` é 0 — ver passo 4 (preciso confirmar o que essa função devolve).
- Fim do passo (contador zerado): grava tile X/Y com `ikki_x_to_tile` / `ikki_y_to_tile` (ver passo 4), chama `Map_CheckWalkOverTrigger`; se nada, `Map_FindWarpTriggerAtTile(x,y)`: se achar warp, grava 3 no byte `DAT_08051c28+1` (estado de transição) e som, depois `Overworld_ResetAndStartScreenTransitionEffect`; senão, se o kind de objeto != 1 e `Overworld_ShouldTriggerRandomEncounter` → `Overworld_BuildEncounterNpcSnapshot` + `GameState_StartFlowByCommand(0x21,0)`.
- Unidade: o passo termina em 16 px (`16/delta` quadros × `delta` px/quadro). Por isso a posição do jogador é inteira em PIXEL, não 8.8 (ver passo 4). A nota "X em 8.8" do OW.md deve ser revista.

## Passo 4: warps (Ghidra, lido)

- Tabela de warps por mapa: ponteiro = `TBL[gCurrentMapId]` (literal DAT_08052CB0 em `Map_FindWarpTriggerAtTile` 0x08052C68; array de ponteiros de 4 B). Cada entrada tem 8 B; a lista termina quando o byte 0 é 0xFF.
  - +0: tile X de origem; +1: tile Y de origem;
  - +2: id do mapa destino (escrito em gCurrentMapId, ver abaixo);
  - +3: alta nibble = kind/som do passo (Map_FindWarpTriggerAtTile guarda `(b3 & 0xF0) >> 4` no slot de estado +3; `Overworld_WalkStepByDirection` usa esse valor para escolher o som); baixa nibble (b3 & 7) = facing ao chegar (`Overworld_ApplyWarpAtCurrentTile`);
  - +6: tile X de destino (px = tile×16 + deslocamento base); +7: tile Y de destino.
  Confiança: confirmado no decompile (estrutura de 8 B, campos lidos). Os literais do array em ROM ainda não foram lidos.
- `Overworld_ApplyWarpAtCurrentTile` 0x08052E88 (WRITE em 0x03005FDE em 0x08052ED8): procura a entrada cuja origem == tile atual; grava `gCurrentMapId = +2`, chama `Map_CenterCameraForSmallMaps`, facing = `+3 & 7`, posição px = `+6×16` e `+7×16` + deslocamento, recalcula tiles, `Map_ClampCameraToLoadedMap`, `Overworld_SyncFollowerPositionAfterWarp`. Chamado de `Overworld_UpdateMapSceneStateMachine` 0x08051368 (uma das fases da máquina de cena).
- Caminho do passo: `Overworld_WalkStepByDirection` (fim do passo) → `Map_FindWarpTriggerAtTile(x,y)` (só marca o som/kind; não troca o mapa) → `Overworld_ResetAndStartScreenTransitionEffect` (transição) → ao terminar, `Map_FinishEnterTransition` 0x0804B036.
- `Map_FinishEnterTransition` 0x0804B036 (WRITE em 0x03005FDE): quando a transição acaba, se o byte de estado da transição (state+0x14) == 0, grava `gCurrentMapId = (u8) state+0x26` (id destino guardado pela transição). Se state+0x14 != 0 o id NÃO muda (warp sem troca de mapa). Depois: centraliza câmera, posição px = tile×16 + deslocamento, limites com `Map_GetMapSizeHalfTileByAxis` (se o tile passa do tamanho do mapa, prende em tamanho−1), `Map_ClampCameraToLoadedMap`, `Map_LoadSecondaryMarkerRecord`, DISPCNT = 0x1E40 e marca estado 1 em `*DAT_0804b158`.
- Quem grava state+0x26 (destino) antes da transição: não confirmado nesta rodada (candidato: `Overworld_ResetAndStartScreenTransitionEffect` 0x08049D18 region); fica PENDENTE.
- `Script_ReadStateBlock` 0x0804B9F4 (WRITE em 0x03005FDE por DAT_0804b9f4): NÃO é warp. Lê 5 bytes do script para um bloco de estado da cena (é o bloco de save/evento); o id ali é o byte de +0x..; não grava gCurrentMapId de fato (o xref é de leitura do literal).
- `new_game_start` 0x0804A440: grava 23 (New Game).

## Passo 5: tabela de velocidade, corrida e escritor de 0x03006D3A (lido)

- Tabela 0x083D2A40 (32 B, 1 B por índice, lida por `Map_GetScaledStepDelta` com base literal 0x083D2A40):
  `01 02 04 00 01 02 00 04 05 06 00 08 09 0A 00 00 | 00 00 00 00 02 01 00 08 0A 09 00 04 06 05 00 00`
  Índices 0..31. Entradas 0 = parado (delta 0; NÃO usar como índice de jogador: `div(0x10,0)` quebra).
- Fórmula (lida no decompile): `delta(px/quadro) = T[obj+0x2A] × T[RAM 0x03006D3A]`. Os DOIS termos usam a MESMA tabela. Logo 0x03006D3A é um ÍNDICE de multiplicador, não um multiplicador direto (corrige OW.md §1/§2 e a tabela "gWalkSpeedMul").
- Leitura: com índice 0 (valor 0x03006D3A = 0 → T[0] = 1), delta = 1 px/quadro × T[obj+0x2A]: 16 quadros por tile. Índice 1 em 0x03006D3A dá ×2 (2 px/quadro = a "corrida" medida na pesquisa com Z/A). Ou seja, caminhar = multiplicador 1 e correr = multiplicador 2, e o byte 0x03006D3A escolhe qual. Isto é provável (bate com a pesquisa), não medido nesta rodada.
- Índice de velocidade do jogador (obj+0x2A do slot 0x2F = 0x0300381A): valor do New Game não lido (sem emulador).
- Escritor de 0x03006D3A: NENHUM acesso direto a 0x03006D3A no código; só o literal 0x03006D38 (DAT 0x08051968) aparece. Possível escrita via base 0x03006D38 + 2 em outra função não encontrada por xref. PENDENTE.

## Passo 6: câmera (decompile)

- Unidade: a câmera é em PIXEL. `Map_SetCameraScrollX((x & 0xFFF) << 4)` = tile × 16.
- `Map_CenterCameraForSmallMaps` 0x080528F8: para mapa menor que a tela (tela = 15×10 tiles; 240×160 px), o scroll vira (15 − tiles_x) × 8 e (10 − tiles_y) × 8, ou seja, centralização (metade do que falta para a tela). Senão scroll = 0. Chamado no warp (`Overworld_ApplyWarpAtCurrentTile`) e na troca de mapa (`Map_FinishEnterTransition`). Confirmado (lido).
- `Map_ClampCameraToLoadedMap` 0x08052AA4: alvo X = (tile X do jogador − 7), mínimo 0; limite superior = tiles_x − 15 (só se o mapa é mais estreito que alvo+15). Y: tile Y − 4, limite tiles_y − 10. A condição `Map_GetCameraScrollX() != 0 → alvo = 0` NÃO está entendida: ver abaixo. Provável; literais DAT_08052B34..40 não lidos.
- `Overworld_WalkStepByDirection` (passo 3): a câmera anda com o jogador só depois do centro (E/S: posição > 0x80 px / limite de mapa; W/N: posição < 0x51 ou limite) e quando o `Map_GetCameraScrollX/Y()` devolve 0. `Map_GetCameraScrollX` 0x08052A50 e `Map_AddCameraScrollX/Y` NÃO decompilados nesta rodada: o que a função devolve (valor do scroll ou flag de trava) está PENDENTE. Não afirmar "scroll 0 = borda" sem isso.
- Sub-pixel: NÃO existe em 8.8 para o jogador. Posição = inteiro em px (16 bits). Ponto de tile: `Map_PixelXToTileX` 0x0805273C (ex-`ikki_x_to_tile`, renomeada): tile = (px − origem) >> 4 com correção para negativo; `Map_PixelYToTileY` 0x08052760 (ex-`ikki_y_to_tile`, renomeada). Origem = ushort em literal DAT_08052758 + DAT_0805275C (não lido). Confirmado no decompile.
- Correção a OW.md: "X em 8.8, >>8 = pixel" está errado para o jogador; a posição é px (16 bits), e o passo termina em 16 px.

## Passo 7: warps (dados lidos da ROM)

- Ponteiro da tabela de warps: literal 0x08052CB0 → 0x08412E60 (array de ponteiros de 4 B por mapa, índice = gCurrentMapId).
- Mapa 0 → 0x08411E08; mapa 23 → 0x08411FE7 (New Game).
- Validação: a entrada do mapa 23 `[18,14, dest 0, b3=0x31, ..., dest tile (18,5)]` bate com a pesquisa ("saída do lab (18,14) → mapa 0 (18,5)"). Confirmado.
- Campos +4 e +5 de cada entrada: não decodificados (valores 0/7/16 etc.).
- Mapa 0, entrada 0: x 12, y 0, dest 33, b3 0x00 → exemplo de origem de borda.

## Passo 8: quem chama Overworld_CheckStepAllowed (corrige OW.md §3)

- O PASSO DO JOGADOR NÃO CHAMA `Overworld_CheckStepAllowed`. Nem `Overworld_ProcessDirectionInput`, nem `Overworld_WalkStepByDirection` (lidos). OW.md dizia que o kind 0..3 era a colisão do passo: isso está ERRADO ou é só outro uso.
- Os 13 BLs para 0x08057D80 com kind imediato (`MOVS r0,#k` antes do BL) são de MENU DE ESCOLHA DE 4 LINHAS:
  - kind 0: 0x08057244, 0x0805782E; kind 1: 0x080572B0, 0x080578F0; kind 2: 0x080572F0, 0x08057930; kind 3: 0x08057330, 0x08057970.
  - Também 0x08058080 (kind 0), 0x080580D4 (1), 0x08058112 (2), 0x08058150 (3) em `Map_CreateChoiceSprites`; e 0x08057500 com kind em registrador (não constante).
- `Overworld_InitChoiceMenuScreen` 0x08057244 (decompile): para cada k=0..3 chama `Overworld_CheckStepAllowed(k)`; se o retorno == 1, desenha a linha ATIVA (`Map_DrawChoiceBackgroundRow(k)`); senão desenha a linha INATIVA. Ou seja, o retorno 1 = disponível. Isso CONTRADIZ "0 = pode, 1..6 negado" do OW.md. Confiança no lido: confirmado; o que cada kind significa no jogo: PENDENTE (ver item de texto da escolha).
- Conclusão: `Overworld_CheckStepAllowed` é a checagem de disponibilidade das 4 opções do menu de tile (ação no mapa), não a colisão de passo. A colisão de tile (tabela 0x03005FF0, 0 livre / 1 bloqueia / 2 porta / 3 condicional) é lida em `Map_GetAdjacentCollisionTileByDirection` e em `Overworld_CheckMovementCollision` (borda) — ver OW.md §3, ainda não reconferido.

## Passo 9: D-pad com lados opostos (confirmado)

- Nibble 3 (L+R), 7, 11, 12 (U+D), 13, 14, 15: não estão no switch → facing 0 (N) → o jogador ANDA PARA NORTE (se o caminho estiver livre). Isto é o comportamento do código, não um bug de teste.
- Diagonais com um lado horizontal (5, 6, 9, 10) viram E/W (horizontal ganha).

## Resumo do que mudou em relação ao OW.md

1. Velocidade: 0x03006D3A é índice (não multiplicador); tabela 0x083D2A40 confirmada (32 B).
2. Posição: px, não 8.8.
3. Passo do jogador não chama CheckStepAllowed; essa função é a disponibilidade de 4 linhas de menu (retorno 1 = ativo).
4. Opostos: andam N.
5. Warps: estrutura 8 B lida e validada com o lab (mapa 23).

## Pendente (para a próxima rodada)

- Escritor de 0x03006D3A (ou quem põe 1 ali para correr).
- `Map_GetCameraScrollX` 0x08052A50, `Map_AddCameraScrollX/Y`: se o retorno é o scroll ou flag; a condição de câmera.
- Literais de `Map_ClampCameraToLoadedMap` (DAT_08052B34/38/3C/40) e origem de `Map_PixelXToTileX` (DAT_08052758/5C).
- Quem grava state+0x26 (id destino) e state+0x14 antes de `Map_FinishEnterTransition`.
- Significado de cada kind 0..3 na escolha (texto/ação) e do retorno 0/2..6 do `Overworld_CheckStepAllowed` (ver Map_CreateChoiceSprites).
- `Overworld_AdvanceWalkStep` (companheiro, slot 0x30) — lido só parcialmente: usa 0xC70 como eixo Y px do companheiro.
- Campos +4/+5 da entrada de warp.
- Testes no emulador: NÃO feitos nesta rodada (regra: sem emulador).

## Arquivos
- `OW2_simbolos.csv` (renames aplicados: `Map_PixelXToTileX` 0x0805273C, `Map_PixelYToTileY` 0x08052760). Nenhum outro rename: os nomes `Overworld_CheckStepAllowed` e o da câmera continuam, por ora, pendentes de decisão.
- Ghidra: sem commit, sem salvar, sem emulador.
