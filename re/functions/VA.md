# VA: variantes de cena do mapa, bloco de 8 B (E+0xC10) e passabilidade (GH-VA, Medabots Metabee EU)

Status: PARCIAL (2026-10-14). Base: MP2.md (item 6), HE4.md (0x69), HE1.md (0x0B/0x0C/0x10). Método: decompile no Ghidra (só leitura; nada renomeado, nada salvo), literais e tabelas lidos na ROM com `python -I`, BFS dos scripts de evento via `pc/extract/extract_events.py` importado por caminho (somente leitura). Sem emulador, sem commit.
Símbolos: `VA_simbolos.csv`.

**Correção ao MP2.md**: `Map_LoadSecondaryMarkerRecord` fica em **0x0805330C** (mtbe.csv e R3.md), não em 0x08053320.

## 1. Base de evento e endereços

Base E = 0x030057E0. Logo: E+0xC10 = **0x030063F0**, E+0xC18 = 0x03006400 (guarda), E+0xC28 = 0x03006408, E+0xC5B = 0x0300643B (variante de cena), E+0xC5C = 0x0300643C (gMapPlayerWalkState, KI.md), E+0xC94 = arg0 do 0x69, E+0xC95 = arg1 do 0x69.

## 2. De onde vêm os blocos de 8 B (confirmado no decompile + ROM)

`Map_LoadSecondaryMarkerRecord` 0x0805330C (Ghidra, literais lidos na ROM):
1. Índice = byte de **E+0xC28** (0x03006408). Não achei escritor desse byte (ver 6).
2. Copia 8 B de **ROM 0x083D2A98 + idx*8** para **E+0xC10..C17** (0x030063F0..F7).
3. Grava 1 byte em **0x0300381A** (= obj+0x2A do slot do jogador 0x2F, índice de velocidade; OW2.md) = `ROM 0x083D2A88[byte E+0xC5C]`.

Tabela B (0x083D2A98, blocos de 8 B; ROM lida para os índices 0..9):

| idx | bytes 0..7 | leitura |
|---|---|---|
| 0, 1, 3..8 | 00 01 00 01 01 01 01 01 | T = [livre, bloqueia, livre, bloqueia] |
| 2, 9 | 01 01 00 00 01 01 01 01 | T = [bloqueia, bloqueia, livre, livre] |

Os bytes 4..7 valem 01 em todos os blocos lidos (papel não identificado). Número total de blocos não determinado (só 0..9 lidos).

Tabela A (0x083D2A88, 16 B, indexada por E+0xC5C): `00 01 00 00 00 00 00 00 00 00 00 02 00 00 00 01`. Escrita em 0x0300381A (velocidade do jogador).

Chamadores de `Map_LoadSecondaryMarkerRecord` (xrefs, 7): Overworld_InitPlayerSprites 0x08051810, Overworld_SetActorAnimFrameByTable 0x08051EAC, Screen_HandleStateTransition 0x0805952C, Map_FinishEnterTransition 0x0804B0EC, Screen_UpdateByStep 0x08058914, **Map_LoadCurrentSceneResources 0x0804DDF2**, EventOpcode11_ChangePlayerCostume 0x0804E398.

Consequência (inferência, não testada): a cada carga de mapa (Map_LoadCurrentSceneResources) E+0xC10 é recarregado da tabela B pelo índice E+0xC28. Logo a variante trocada pelo 0x69 só vale até a próxima carga, a menos que E+0xC28 mude junto.

## 3. Os 8 bytes 0x030063F0..F7

- **0x030063F0..F3 = passabilidade por valor de colisão 0..3** (MP2 item 6). Leitor: `Overworld_CheckMovementObjectBlocker` 0x080525D0 (T = byte[0x030063F0 + v]; T ≠ 0 bloqueia e vira a direção). Por isso o valor 3 é liberado ou bloqueado pelo byte **0x030063F3** (E+0xC13): bloqueia se ≠ 0.
- **0x030063F4..F7 (E+0xC14..C17)**: valor 01 nos 10 blocos lidos. Leitores não procurados (pendente).
- Outros escritores de 0x030063F0 (xrefs de literal 0x030063F0, 2 pools): Move_Player 0x0804BB14 (HE1; copia 8 B de 0x03006400 para E+0xC10 e zera; é uso de scratch e não de passabilidade) e o handler 0x080525D0 (lido, não escreve). Mais Op10 e EventOpcode0F (MP2) usam a mesma região como scratch de movimento.

## 4. O que o 0x69 faz (confirmado, Event_Op69_Set_Map_Scene_Variant 0x0804D17C)

Lê 2 bytes: arg0 → E+0xC94, arg1 → E+0xC95.
- Só age se **arg0 == E+0xC5B** (variante atual). Então E+0xC5B = arg0 ^ 1 e chama `Map_ApplySceneVariantScrollPreset` 0x08052BEC (preset de scroll/câmera da variante).
- Se **arg1 == 0**:
  - nova variante = 0: salva **E+0xC10..C17 → E+0xC18..C1F** (guarda) e zera E+0xC10..C17.
  - nova variante = 1: restaura **E+0xC18 → E+0xC10**.
- arg1 ≠ 0 pula a cópia (a tabela de passabilidade fica como está).

Obs.: a descrição de MP2 ("variante 0: de DAT_0804D1F8, depois zera") está imprecisa. O literal 0x0804D1F8 é o offset 0xC18 (guarda), não uma tabela ROM. HE4.md está certo.

## 5. Quem põe E+0xC5B (variante)

- Warp A 0x0804B9B0 (0x0B): grava **1**; Warp B 0x0804B9CC (0x0C): grava **0** (HE1).
- Op69 (troca, acima).
- Evento 0x84 (continuação 0x0804FE6C/0x0804FE84): copia arg0 para o campo de variante ao olhar para cima (HE4). Não confirmado em detalhe.
- Leitores de E+0xC5B além do 0x69: não varridos (pendente). Sei que Op69 lê.

## 6. Pendências (não fechadas)

- **Escritor de E+0xC28 (0x03006408)**: 6 literal pools com 0xC28 (0x0804B150, 0x0804E3C4, 0x080518D8, 0x08051ED4, 0x08058944, 0x08059554) são todos **READ** (FinishEnterTransition, ChangePlayerCostume, InitPlayerSprites, SetActorAnimFrameByTable, Screen_UpdateByStep, Screen_HandleStateTransition). Não há literal 0x03006408 na ROM. Escrita provavelmente por cópia de bloco ou por base+offset calculado. Pendente.
- Tamanho da tabela B (quantos blocos) e significado dos bytes 4..7.
- Mapa script → variante: 124 ocorrências do 0x69 (ver 7) ainda não ligadas ao mapa dono de cada evento.

## 7. Usos do 0x69 nos scripts (extract_events.py, só leitura)

Varredura por caminho (`importlib` sobre `pc/extract/extract_events.py`, `decode_event` em todos os 2288 eventos; o 0x69 é pulado pelo `decode_event` e o gancho foi em `decode_fx`, que é chamado antes do pulo). Script: `scratchpad/VA/scan69.py`.
- 124 ocorrências alcançáveis em **45 eventos**: 55, 128, 133, 156, 157, 217, 235, 266, 277, 282, 394, 398, 427, 428, 429, 475, 583, 585..587, 617, 674, 812, 835, 1026, 1103, 1104, 1393, 1394, 1396, 1420, 1627, 1642, 1671, 1678, 1909..1911, 1959, 2067, 2068, 2070, 2072, 2073, 2080.
- arg0: 0 em 59, 1 em 65. arg1: 0 em 122, 1 em 2.
- O extrator ainda não decodifica 0x69 (sem efeito no PC). Tamanho 3 bytes (ops[0x69] = 3) = opcode + 2 args.

## 8. Ligação com a colisão 3 e o conjunto 0x6B (confirmado no decompile, com correção)

- O 0x6B **não** é o chapter (esse é o opcode 0x6B = Set_Current_Chapter, HE4). O conjunto 0x6B é um id de conjunto de peças: `PartSet_FindCompleteEntryIndex(0x6B)`.
- `Overworld_CheckStepAllowed` 0x08057D80 (param 3) é um **teste de opção de menu** (chamado por Overworld_InitChoiceMenuScreen e Map_CreateChoiceSprites), não de passo. Libera a linha 3 se: tem item 0x26 no inventário, conjunto 0x6B completo, flag byte &8 == 0 e o tile adjacente (Map_GetAdjacentCollisionTileByDirection) == **3**.
- Portanto: na caminhada, o valor 3 segue a tabela E+0xC10..C13 (acima). O conjunto 0x6B só libera uma opção de menu quando o jogador está diante de um tile 3. Isso corrige a leitura de W4.md ("passagem liberada pela peça 0x6B") e confirma OW2.md (CheckStepAllowed = menu).

## 9. Renames (proposta; nada aplicado no Ghidra)

Ver `VA_simbolos.csv`. Só os já confirmados entram como "confirmado"; o resto é provável.
