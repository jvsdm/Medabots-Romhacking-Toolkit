# EV4: encontro aleatorio (cmd 0x21), tabelas de encontro e gravacoes do Op33 (GH-EV4, 2026-10-14)

Leitura estatica (Ghidra + literal pool lido na ROM original com `python -I`). Base E = 0x030057E0. Nada rodou no emulador.
Simbolos: `EV4_simbolos.csv`. Renomeado no Ghidra: 0x0300640A gEncounterSlotIndex -> `gEncounterTemplateId` (a chamada
respondeu so "Rename data attempted"; conferir na UI). Os demais nomes ja existiam e batem com o codigo.

## 1. Encontro aleatorio (cmd 0x21)

Chamador: `Overworld_WalkStepByDirection` 0x08051988 (confirmado: xref de 0x080438C4 em 0x08051C4A). Ao terminar cada tile:
1. contador de passo do tile (E+0xC58) chega a 0;
2. `Map_CheckWalkOverTrigger` == 0, `Map_FindWarpTriggerAtTile` == 0 e `E+0xC88` (gCurrentEventPendingFlag) == 0;
3. tipo do tile em `E+0xC5C` (gMapPlayerWalkState, 0x0300643C) != 1 (o tipo 1 nunca gera encontro);
4. `Overworld_ShouldTriggerRandomEncounter` 0x080438C4 != 0 -> `Overworld_BuildEncounterNpcSnapshot` e `GameState_StartFlowByCommand(0x21, 0)` 0x08047878 (xref 0x08051C5A).

`Overworld_ShouldTriggerRandomEncounter` (confirmado no decompile + literais 0x08043938..0x08043954):
- Retorna 0 se qualquer um falhar: `E+0xC2D == 0`, `E+0xC5F == 0`, `E+0xC29 != 0` (encontro habilitado do mapa), `(E+0xC2C & 1) == 0` (bloqueio).
  - `E+0xC2D != 0` desliga o encontro aleatorio inteiro, nao so o sorteio. Por isso o cmd 0x20 (historia, grava C2D = 1) nunca rola encontro.
  - `E+0xC5F` (0x0300643F) precisa ser 0. O nome do mtbe (`gEncounterSuppressB`) e provavel; o uso como gate e confirmado.
- Se passa: `E+0xC2B` (0x0300640B, gStepCounter) += 1. Se o valor for > 16 e (> 31 OU `randbyte() & 0x1F == 0x1F`), zera C2B, C2E e C2F e retorna 1.
  - Na pratica: entre 17 e 31 passos, 1/32 por passo; a partir de 32 passos dispara sempre.

Zeragem em 0x0804387C `Overworld_ResetEncounterStepState` (confirmado; chamada na carga de mapa por 5 funcoes):
- `E+0xC29 = gMapEncounters[gCurrentMapId*8 + 0]` e `E+0xC2B = 0`.

## 2. Tabela gMapEncounters 0x083BEC10 (192 x 8 B, indice = E+0x7FE gCurrentMapId)

Lido na ROM: mapa 0 = `00 04 01 01 00 00 00 00`; mapa 1 = `01 04 01 00 ...`; mapas 2..6 = `01 05 00 00 ...`; mapa 7 = `00 05 ...`.

| Byte | Uso | Confianca |
|---|---|---|
| +0 | encontro habilitado do mapa (0/1). Vai para E+0xC29 em cada carga de mapa; e o gate do sorteio | confirmado |
| +1 | valor 0..10 copiado para `fight_information+0x38` (0x03001838) em `Fight_CopyEncounterTemplateEntry` 0x08011DE4 (copia 2 B para o scratch 0x0300052E; so o byte +1 e usado). Quem le +0x38 nao foi achado | escrita confirmada; significado provavel (area/modelo) |
| +2, +3 | 0/1 nos dados. Nenhum leitor achado neste passo | aberto |
| +4..+7 | sempre 0 | medido |

Leitor de +0/+1: `Fight_BeginEncounterTemplateSelection` 0x08011DB0 (chamado por `Fight_ResetTemplateAndPalette` 0x08010540).
A tabela de sorteio dos bots e OUTRA: `gMapEncountersExtra` 0x083BF210 (secao 3).

## 3. Como escolhe o inimigo (Fight_SelectTemplateForCurrentEncounter 0x08011E20, confirmado)

- Se `E+0xC2D == 0` (encontro aleatorio): copia 4 B de `gMapEncountersExtra[gCurrentMapId]` (0x083BF210 + mapa*4) para o scratch 0x0300052E.
  Sorteio `Encounter_RollPercent0To99` (0x08043C94): r < 40 -> byte 0 (40%); < 70 -> byte 1 (30%); < 90 -> byte 2 (20%); senao byte 3 (10%).
  O byte escolhido e um **id de template** (0..245) e vai para `E+0xC2A` (gEncounterTemplateId, renomeado).
- Se `E+0xC2D != 0` (historia, cmd 0x20 / Op33): usa `E+0xC2A` direto, sem sorteio. O id veio do Op33.
- Copia 0x28 B de `gEncounterTemplatePtrs[E+0xC2A]` (0x083C1B80 + id*4) para `fight_information` 0x03001800.
- Se `template+3 == 0`, re-sorteia as pecas (Fight_SelectTierPartId 0x08011F20). Todos os 246 templates tem +3 = 1 (medido em EN.md), entao esse ramo nao roda com os dados da ROM.
- Chamador: `Fight_BeginEncounterTemplateSelection` 0x08011DB0 (xref confirmado, 0x08011DC8 e 0x08011DCC).

Mapas com byte0 = 0 em gMapEncounters (ex.: 23 = New Game) nao tem encontro pelo gate de C29, mesmo que tenham quarteto em gMapEncountersExtra.

## 4. Op33 StartBattle: o que grava cada byte (corrige BT5 e EV3)

`Event_Op33_StartBattle` 0x0804C53C (decompile + literais 0x0804C574..0x0804C584, confirmado):

| Byte do script | Destino | Uso |
|---|---|---|
| 1o | E+0xC2A (0x0300640A) | id do template (gEncounterTemplateId) |
| 2o | **E+0xC2F** (0x0300640F) | "modo" da batalha. Leitor: `Fight_WaitSubBFlagOrJumpGS77` 0x08011B30: se C2F == 0 vai para GS 0x77; senao avanca o estado (decompile) |
| 3o | **E+0xC2E** (0x0300640E) | gPostBattleModeFlags, ver secao 5 |

Depois: `Stub_ReturnZero_0804B738` (sempre 0 neste jogo) -> grava E+0xC89 (0x03006469, pendente do evento) = 1.
Ou seja: BT5 dizia "E+0xC2E e E+0xC2F (modo)". Correto: o 2o byte e o modo (C2F) e o 3o byte sao as flags pos-batalha (C2E).

## 5. E+0xC2E (0x0300640E) = flags pos-batalha (leitores confirmados)

- Nibble baixo, lido em `Fight_HandleRewardGrantOrSkipByPostBattleFlags` 0x08010C60 e `Fight_AssignRandomPartSlotStep` 0x08011C98:
  0 = concede peca (sorteio de peca do bot: `Fight_TryAssignBotPartByRoll` / `Fight_AssignRandomBotPartSlot`). Diferente de 0 = zera o nibble e pula (GS 0x3E ou 0x40).
- Nibble alto, lido em `Fight_ApplyPostBattleMode20PlayerSideState` 0x08018A1E e `Fight_PlayResultCueByState` 0x08018A72:
  0x20 = escreve 0x50 em 3 bytes espacados de 0x100 (lado do jogador; destino nao identificado); 0x10 = trilha 0x0C; 0x00 = trilha da tabela; outro valor = trilha 0x0D. Se ctx+0x89 == 1: trilha 9.
- Escritores: `Event_Op33_StartBattle` (0x0804C55C) e o reset em `Overworld_ShouldTriggerRandomEncounter` (0x0804392C). O Ghidra tambem mostra escrita em `Menu_TickSingleCountSelector` 0x0800D386 (menu de debug do DB2), em `Gfx_ResetBgLayersAndSong` 0x08011574, em `Gfx_ResetScreenBgAndBlend` 0x08011C26 e em `Fight_AssignRandomPartSlotStep` 0x08011CA4. Nao conferido nessas funcoes.

## 6. E+0xC2A e E+0xC2F: leitores e escritores

- C2A (id): lido em `Fight_SelectTemplateForCurrentEncounter` (0x08011E8A). Escritores: Op33 (0x0804C548) e o menu de debug (0x0800D39E). O literal 0x0C2A aparece tambem nos pools 0x08051198, 0x080511F8 e 0x08052180, em regioes que o Ghidra nao reconhece como funcao; nao decodificado.
- C2F (modo): lido em `Fight_WaitSubBFlagOrJumpGS77` (0x08011B30). Escritores: Op33 (0x0804C552), menu de debug (0x0800D38C) e reset em 0x08043932 (zera C2F ao fim do encontro).
- Cuidado: o reset de C2D nao existe em 0x08043932. Em 0x08043932 o byte zerado e C2F; em 0x0804392C, C2E; em 0x08043926, C2B.

## 7. E+0xC4F (0x0300642F): quem grava (NAO FECHADO)

- Leitor confirmado: `Event_Op33_Cont_StartBattle` 0x0804E8C4 (literal 0x0804E918 = 0xC4F, usado em 0x0804E8F8). Se o valor for 1 ou 2, copia para `0x03005310 + 0x39` (gMapSceneSubstate+0x39) e chama `Event_EndOpcode`.
- Segundo leitor: literal 0x08059178 = 0xC4F, usado em 0x08059136. Fica numa regiao que o Ghidra nao reconhece como funcao (`No function found`). Pelo codigo, desenha texto e compara o byte com 1 e 2. Nao decodificado a fundo.
- Escritor: NAO achado. O endereco 0x0300642F aparece so numa tabela de ponteiros RAM na ROM (0x083D62A8..0x083D62C4: 0x0300642E, 0x03006498, 0x03006499, 0x0300642F, 0x030057C0, 0x030057CE, 0x030057D0, 0x030057C4). Nenhum LDR nem ADR aponta para essa tabela (varredura nas faixas 0x083D6200..0x083D6300 vazia). Logo o acesso e por indice ou por calculo.
- Hipotese (nao provada): escrita generica por script (variavel de evento indexada). Ver pendencias.
- Significado de 1 e 2: nao confirmado. O que se sabe: 1 ou 2 e copiado para o byte 0x39 do estado do mapa.

## Pendencias

- Escritor de E+0xC4F (secao 7). Sugestao: achar o acesso a tabela 0x083D6288/0x083D62A8 (indice vindo de script?) ou o escritor generico de variavel de evento.
- Leitores de gMapEncounters +2 e +3; leitor de fight+0x38 (byte +1 de gMapEncounters).
- Literais 0x0C2A e 0x0C2F em 0x08051198..0x08052180 (regiao sem funcao no Ghidra).
- Leitor de E+0xC5C por tipo (tabela 0x083D2A88; o byte de velocidade vai para 0x0300381A, gPlayerSpeedIndex). So o gate tipo != 1 foi lido.
- `Overworld_BuildEncounterNpcSnapshot` (chamado antes do cmd 0x21) nao foi lido.
