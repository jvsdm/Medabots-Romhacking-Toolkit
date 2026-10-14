# AI2: fechamento do GH-AI (IA da CPU na Robattle)

Data: 2026-10-14. Só leitura: ROM original (`python -I`, sem escrita), Ghidra (decompile, xref), sem emulador.
Base: `AI.md` (algoritmo e "Pendente"). Símbolos: `AI2_simbolos.csv`. Funções a criar: `AI2_lista.txt`.

## 1. Tabela 0x083AD470 (gFightFallbackPriorityOrders): FECHADA

- 32 linhas x 4 B. Linhas 0..29: permutação de `01 02 03` seguida de `FF` (sentinela). Linhas 30 e 31: `FF FF FF FF`
  (vazias: o sorteio cai nelas, não há peça, `Fight_SelectActionSource(bot,0)`).
- Exemplo: linha 0 = `01 02 03 FF`; linha 1 = `02 03 01 FF`; ...
- **Escritor** de fi+0x64..+0x67 (0x03001864): único chamador é o BL em 0x08015490 dentro de
  `Fight_SelectFallbackActionFromPriorityList` 0x08015464. `Fight_RandomizePriorityListSlot` 0x080156D0 faz
  `randbyte() & 0x1F` e copia 4 B de `0x083AD470 + idx*4` (origem ROM) para `0x03001864` (destino fi+0x64). A direção
  NÃO está invertida (a hipótese antiga do AI.md estava errada).
- **Leitor**: o mesmo `Fight_SelectFallbackActionFromPriorityList` (varre até 4 entradas: `-1` = acaba; peça
  bloqueada = próxima; usável = `Fight_SelectActionSource(bot, e)`).
- Valores 1..3 são códigos de peça (mesma convenção da fonte +0xDD, 1..3 = peça). **Código 1 = cabeça confirmado**
  (`Fight_IsHeadActionUnavailable` 0x08015508 checa `param_2 == 1` e bot+0x58). 2 e 3 = braços (provável: 2 = direito,
  3 = esquerdo, pela ordem do ciclo de especialidade; não confirmado no nome).
- Correção ao AI.md §6: "valores até 0xF0, sem -1" estava errado (leitura antiga).
- **Colisão**: 0x03001864 também é a lista de 4 ids da prévia do Medabot (`team_preview_part_ids`, PS.md; HA2/HO1 leem
  `Fight_GetDisplayedPartIdsOrBrokenGlyphs(bot, fi+0x64)`). Ou seja, o fallback da CPU pode sobrescrever o buffer da
  prévia. Não verificado no jogo: pendente.
- Literais: 0x080156F0 = 0x083AD470 (tabela), 0x080156F4 = 0x03001864 (destino).

## 2. Driver do turno da CPU: FECHADO

- Despachante de cena: `Battle_FrameDispatchOrExit` 0x08012970 indexa a tabela de cena 0x083AD080 por
  `GS = [0x0300137C]`. Entradas lidas na ROM (ponteiros thumb): 0 = 0x080129C8, 1 = 0x08012A20, 2 = 0x08012B40,
  **3 = 0x08012B94**.
- **Função que contém o BL 0x08012BA6**: início 0x08012B94 (`push {lr}`), fim 0x08012BB4 (`pop` / `bx r0`). Sem função no
  Ghidra (`get_function_by_address` e `disassemble_function` falham em 0x08012B94 e 0x08012BA6).
- Corpo (disassembly lido na ROM):
  1. `bl 0x080196C0` (sem nome, sem função no Ghidra: ver `AI2_lista.txt`).
  2. `r0 = [0x03000440] + 0x89` = `gDeterministicRandom` (0x030004C9); se `== 1`, pula a escolha.
  3. Senão: `r0 = 3` e `bl Fight_SelectActionsForBotGroup 0x0801531C` (3 = primeiro bot do lado inimigo).
  4. `[0x0300137C] += 1` (avança o passo da cena).
  5. `pop` / `bx` (retorno).
- Chamador do 0x08012B94: só a tabela 0x083AD080 (ponteiro, sem BL).
- Resultado: a cada passo do turno, a cena chama o driver, que escolhe a ação de até 3 bots inimigos (ciclo, depois
  fallback) e avança o passo. O sentido de `gDeterministicRandom == 1` como condição de pular não fechou: não
  confirmado (ver Pendente).

## 3. Chamadores de Fight_ChooseActionTargetSlot 0x080322C8: NÃO ACHADO estaticamente

- Varredura de todos os BL (pares `F000`/`F800`) de 0x08000000 a 0x080B0000 e de ponteiros (palavra `0x080322C9`):
  nenhum. Hipóteses: chamada indireta (tabela de offsets, ponteiro montado em RAM) ou via thunk.
- Próximo passo (precisa emulador, fora desta tarefa): breakpoint em 0x080322C8 com `replay_record.py --break`
  numa Robattle em que a CPU escolhe alvo.

## 4. Argumento de Fight_IsMedaforceBlockedByChapter 0x08015648 (antes "...BattleState")

- Único chamador: BL em 0x080155B0 dentro de `Fight_TrySelectMedaforceAction` 0x080155A8. O argumento é o **r0 de
  entrada** da própria `Fight_TrySelectMedaforceAction` (`bot`): entre o entry e o BL só há `mov r7,r0`, `mov r4,r1`,
  `mov r5,r2`, então r0 chega intacto.
- Lógica (decompile): retorna **1 (bloqueado)** se `arg >= 3` **e** `byte[0x03005FDF] <= 2`; senão 0. (AI.md dizia o
  inverso.) Literais: 0x0801565C = 0x030057E0 e 0x08015660 = 0x000007FF, então o byte é 0x03005FDF
  = `gCurrentChapter` (mtbe.csv).
- Interpretação (provável): bots 3..5 (inimigos) não usam medaforça enquanto o capítulo for <= 2.
- Renomeada no Ghidra (ver Renames).

## 5. Fight_BeginPartActionValidation 0x08033318 e pesos 0x083AD380

- Validação (decompile): `fi+0x5C = bot`, `fi+0x5D = part - 1`; zera e retorna o byte `fi+0x51` (o handler o escreve).
  Depois `CallViaR0(ptr[0x083AF090 + byte(0x03000590 + bot*0x100 + (part-1)*0x18 + 0x49)*4])`.
  Literais: 0x0803335C = 0x083AF090 (tabela de handlers), 0x08033360 = 0x03000590 (blocos de bot), 0x08033358 = 0x03001800.
  O campo +0x49 por parte é "código de ação" (provável; nome não fechado).
- **Único chamador**: BL em 0x08015722 dentro de `Fight_IsPartActionBlocked` 0x080156F8. Esta retorna 1 quando bloqueia.
- **Pesos** (`Fight_InitializeBotFromTemplate` 0x080150FC, decompile; literal 0x080151E8 = 0x083AD380):
  para o bot `idx+3` (inimigo), `w[k] = T[bot+4 * 8 + k]`; se `(bot+5) >> 1 != 0`, `w = w * ((bot+5)>>1)`;
  `0 -> 1`; `> 100 -> 100`; grava em `bot+0x20+k`. Valores lidos (linhas 0..15) são de 1 a 4, sem zero
  (linha 0 = `04 04 01 01 02 01 02 01`). Faixa de atributo acima de 15 não lida.

## Renames no Ghidra (confirmados)

- 0x08015648: `Fight_IsMedaforceSelectionBlockedByBattleState` -> `Fight_IsMedaforceBlockedByChapter`.
- 0x083AD470 (data): `gFightFallbackPriorityRows` -> `gFightFallbackPriorityOrders`. A ferramenta respondeu
  "attempted": conferir na interface.
- Nenhuma função nova (0x08012B94 e 0x080196C0 não existem no Ghidra). Lista em `AI2_lista.txt`.

## Pendente

- Criar função em 0x08012B94 (driver) e função em 0x080196C0 (chamada antes da escolha; não lida).
- Chamador de 0x080322C8: indireto; achar com breakpoint no emulador.
- Sentido de `gDeterministicRandom == 1` como condição de pular a escolha (ver o que grava 0x030004C9).
- Colisão 0x03001864: o fallback da CPU sobrescreve a lista de ids da prévia? Testar no emulador.
- Nome do campo +0x49 por parte e dos handlers da tabela 0x083AF090 (AI.md ainda não cobre).
- Nomes 2 e 3 (braço direito / esquerdo) para os códigos de peça: provável, não confirmado.
