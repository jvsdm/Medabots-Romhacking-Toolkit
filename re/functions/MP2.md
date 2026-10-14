# MP2: pendências do mapa (GH-MP2, Medabots Metabee EU)

Status: PARCIAL (2026-10-14). Base: MP.md. Método: decompile e xrefs no Ghidra (só leitura; nada salvo nem fechado); literais e blocos lidos na ROM com python -I (sem extrair imagem); scan de BL na ROM. Sem emulador, sem commit.
Símbolos: `MP2_simbolos.csv`. Renome feito: `FUN_0804830C` -> `Map_LoadMapPaletteToShadowBuffer`.

## 1. 0x030057DC = buffer da ANIMAÇÃO DE TILES (confirmado no decompile)

- `Map_LoadCurrentMapLayers` 0x0804B2B0 descomprime (LZ77) `ROM[0x083F8D80 + 4m]` no buffer cujo endereço está em 0x030057DC (quando o bloco ≠ 0).
- Leitor: `Map_UpdateBackgroundAnimations` 0x080481D8, chamado por `Map_LoadCurrentMapAndEntities` 0x08048108. Chamador por quadro: não achado (sem BL; pendente).
- Descritor: `ROM[0x08409480 + 4m]`, indexado por gCurrentMapId (0x03005FDE). Bytes: +0 = T (tiles de 32 B por quadro); +1 = nº de quadros; +2 = habilitado; +3.. = sequência de índices de quadro, termina em 0xFF e reinicia.
  - Mapa 23: T=11, 4 quadros, sequência 0x5,1x5,2x5,3x5 (repetida). Bloco LZ77 = 1408 B = 4 x 11 x 32.
  - 68 mapas com +2 ≠ 0. Mapas 1 e 10: bloco de 32 B, desligado.
- Contador 0x03006416 (base+0xC36) recarrega com 2: avança quadro a cada 3 chamadas. Índice em 0x03006415 (base+0xC35).
- DMA3 por passo: SAD = [0x030057DC] + idx*T*0x20; DAD = 0x06009020 (tile 1 do tileset em 0x06009000); 16 bits; count = T*16 halfwords; CNT = 0x80000000 | T<<4.
- Ligado por 0x03006414 (base+0xC34) ≠ 0 e descritor+2 ≠ 0. Escrito por new_game_start 0x0804A482 e pelo evento 0x84 (EventOpcode84_SetMapSceneVariantWhenPlayerFacesUp_Continuation 0x0804FE84).
- Conclusão: animação de tiles de fundo, não eventos de tile nem overlay. Efeito visual não testado no emulador.

## 2. Paleta do mapa (confirmado no código)

- `Map_LoadMapPaletteToShadowBuffer` 0x0804830C (antes FUN_0804830C). Chamada por Map_LoadCurrentMapAndEntities 0x08048108 e Map_LoadCurrentSceneResources 0x0804DDCE.
- Ponteiro = ROM[0x083F9980 + 4m] (mapa 0 -> 0x085FC09C; mapa 10 -> 0x08606284; mapa 23 -> 0x08613EF0). Formato BGR555 (confere nos primeiros valores).
- DMA3: SAD = ponteiro, DAD = 0x0202FA00 (buffer EWRAM de 0x200 B), CNT = 0x84000068 (32 bits, 0x68 words = 0x1A0 B = bancos 0..12). Bancos 13..15 vêm de outro lugar (H1: 14 = 0x084E43D8, 15 = 0x08488520).
- Se o ponteiro é 0: caminho alternativo com 8 words de 0x080AC72C (não decodificado).
- 0x0202FA00 vai ao hardware por CopyPaletteBuffersToHardware 0x08001390 (xref confirmado).
- Ordem: paleta -> Map_LoadCurrentEventObjectPalettes -> Map_LoadCurrentMapLayers -> Map_UpdateBackgroundAnimations -> Map_LoadEntitySpawnsForCurrentMap.

## 3. Origem do scroll (parcial)

- `Map_GetInitialScrollTileOrigin` 0x080529DC (lido): NÃO calcula centralização. Lê a struct apontada por [0x03006D14]: modo 0 -> (u16 P+0, u16 P+2); modo 1 -> (u16 P+4, u16 P+6).
- Map_LoadCurrentMapLayers grava em gMapState+0x804 (X) e +0x806 (Y) e usa como origem da cópia das camadas.
- Centralização (confirmada): `Map_CenterCameraForSmallMaps` 0x0805290C, chamada por Map_LoadCurrentMapLayers e por Overworld_ApplyWarpAtCurrentTile:
  - antes, descomprime a camada 0 (ROM[0x083F9080 + 12m]) no buffer [0x03006D14];
  - X: se (w8>>1) < 15 -> gMapState+0xC48 (0x03006428) = (15 - (w8>>1))*8, senão 0;
  - Y: se (h8>>1) < 10 -> gMapState+0xC4A (0x0300642A) = (10 - (h8>>1))*8, senão 0.
  - Equivale a ((30-w)//2) em tiles de 8 px (W3). Vai para 0xC48/0xC4A, não para 0x804/0x806.
- Aberto: quem escreve [0x03006D14] e o que P contém. Pelo decompile, P é o buffer da camada 0, cujo u16 @0 é lw; a "origem" lida daria (lw, 0), o que não bate com W3. Precisa de runtime ou do escritor de P.

## 4. Warps e cabeçalho de camada

- gWarpPtrTable 0x08412E60. Leitores achados: Map_FindWarpTriggerAtTile 0x08052C68 e Overworld_ApplyWarpAtCurrentTile 0x08052EA0.
  - +0 x, +1 y: casam com gPlayerTileX/Y.
  - +2: mapa destino. +3: nibble alto = tipo/som (guardado em estado+3); (+3 & 7) = facing ao chegar.
  - +6, +7: destino em tiles de 16 px (x16 + offset de scroll).
  - +4 e +5: NÃO lidos pelos dois leitores. Sem outro leitor achado (não é prova de que não são usados).
  - ApplyWarp para no primeiro match; FindWarpTrigger percorre tudo (último vence).
- Cabeçalho de camada, u32 @8: não lido pelos blocos decompilados. Pendente.

## 5. Pessoas: Map_LoadEntitySpawnsForCurrentMap 0x08051416 (confirmado no decompile)

- Ponteiro por mapa: ROM[0x08483234 + 4m] (índice = gCurrentMapId). Registros de 8 B, termina em 0xFF.
- Laço externo: 32 grupos (0..0x1F). Só entra o registro cujo +5 = grupo atual.
- +0 x, +1 y (metatile 16 px).
- +2 u16: bits 0..11 = id de evento (packed); bits 12..15 = kind. Se EventFlag_IsSetByPackedId(id) está ligada, a pessoa não entra.
- +4: nibble alto = sprite (Map_SetObjectAnimationVariant(slot, sprite*4 + facing)); nibble baixo = facing 0..3. 0xFF = sem sprite.
- +5 = grupo 0..31.
- +6 u16 = máscara de capítulo: aparece se (mask >> gCurrentChapter) & 1 (gCurrentChapter = 0x03005FDF, confirmado).
- Slots de objeto: até 30 pessoas, slots 0x31+k (e 0x5B+k no sprite secundário). Ocupação em gMapState+0x5E0. Kinds 0/2/4/6 têm tratamento visto; kinds 1/3/5/7/8 não separados.

## 6. Passo do jogador e colisão 0..3 (confirmado no decompile)

- Passo do jogador = Overworld_ProcessDirectionInput 0x08051DDC (não é CheckStepAllowed). Lê a tecla em 0x08051D3C (= 0x03002B64, de OW.md); converte para facing 0..3 (N=0, S=1, W=2, E=3).
- Por passo, nesta ordem:
  1. Overworld_CheckMovementCollision 0x08052CC0: NÃO é colisão de tile; é limite do mapa/câmera. Se bloqueia, só vira.
  2. Overworld_CheckMovementObjectBlocker 0x080525D0 (sem BL; chamada por ponteiro): se gMapState+0xC5C (0x0300643C) == 2 e o tile adjacente é 0 ou 2, atualiza o estado; depois v = Map_GetAdjacentCollisionTileByDirection(dir) (0 livre, 1 bloqueio, 2 porta/warp, 3 condicional) e T = byte[0x030063F0 + v]. T ≠ 0 bloqueia (e vira para a direção).
  3. Se livre: Map_GetScaledStepDelta(0x2F) -> Overworld_WalkStepByDirection 0x08051BC8 (move jogador/câmera; ao fechar o passo chama Map_FindWarpTriggerAtTile e Overworld_ShouldTriggerRandomEncounter); SFX 0x57 em terreno especial; Overworld_AdvanceWalkStep.
- Valor 3: não bloqueia por si. Bloqueia só se a entrada 0x030063F3 da tabela ≠ 0.
- Tabela 0x030063F0 (4 B = passabilidade por valor 0..3): escrita por Event_Op69_Set_Map_Scene_Variant 0x0804D17C (confirmado): alterna a variante de cena (0/1) e copia 8 B para gMapState+0xC10 (variante 0: de DAT_0804D1F8, depois zera; variante 1: do bloco anterior). Também escrita por Event_Op10_MovePlayer, EventOpcode0F_PlayerHopInDirection e Map_LoadSecondaryMarkerRecord 0x08053320.
- Conclusão: o valor 3 (passagem condicional) é liberado/bloqueado pela variante de cena (evento 0x69), não por OW2 (CheckStepAllowed = menu). Falta ler o bloco de 8 B da variante (ROM ou RAM).

## 7. Pendente
- Item 1: chamador por quadro de Map_UpdateBackgroundAnimations.
- Item 3: escritor de [0x03006D14] e conteúdo de P.
- Item 4: +4/+5 do warp (sem leitor entre os dois achados); u32 @8 do cabeçalho de camada.
- Item 6: origem exata de 0x03002B64 (tecla) e bloco de 8 B da variante do evento 0x69.
- Nada testado no emulador (regra da tarefa).
