# GH-AI — IA dos bots (escolha de ação e alvo na Robattle)

Escopo: como um bot controlado pela CPU (lado inimigo 3..5 e aliados não controlados pelo jogador) escolhe a
fonte da ação (peça ou medaforça) e o alvo. Base: decompile no Ghidra (leitura), literais do pool lidos da ROM
original (`python -I`, só leitura). Os nomes das funções já estavam aplicados no Ghidra; nada foi renomeado
nesta rodada (ver "Pendente").

## 1. Estado e blocos (confirmado)

- `fight_information` = `0x03001800` (FightTemplate copiado + estado da luta).
  - `+0x58` = flag global de medaforça/ação de cabeça (lido em `Fight_TrySelectMedaforceAction`).
  - `+0x5C` = índice do bot que age; `+0x5D` = peça; `+0x5E` = slot de alvo escolhido (base 1);
    `+0x60` = base do lado (0 aliado ou 3 inimigo) usada como contador de bot; `+0x61..+0x63` = candidatos (bot+1).
  - `+0x64..+0x67` = lista de prioridade de fallback (4 bytes, sentinela `-1`).
  - `+0xAA` (substado), `+0xB5` (handler feito), `+0xD2..D4` (afetados), `+0xE8` (valor da ação; ver §5).
- Bloco do bot = `0x03000590 + idx*0x100` (idx 0..5; inimigos 3..5). Offsets do bot usados na IA:

| off | significado (confirmado / provável) |
|---|---|
| +0x04 | atributo/medalha casada com a peça (vem do byte +1 do registro de medalha) |
| +0x05 | nível do bot (medalha); comparado com 10, 30 (>29), 60 (>59) |
| +0x09 | início do ciclo de especialidade (`ciclo_idx` começa aqui) |
| +0x18..+0x1F | **ciclo de especialidade**: 8 passos, cada um um código 0..15 (nibble) |
| +0x20..+0x27 | pesos por passo do ciclo (ver §5) |
| +0x3F | índice atual do ciclo (0..7; ao chegar em 8 volta a +0x09) |
| +0x40 | bot ativo (≠0) |
| +0x58 | cabeça usável (≠0) |
| +0x59 (por peça, stride 0x18) | peça íntegra (≠0); 0 = destruída/ausente |
| +0x56 / +0x57 (por peça) | alvo da peça (gravado por `Fight_SetPartActionMetadata`) |
| +0xC1 | token de medaforça: `'P'` = a medalha pode usar medaforça |
| +0xC4 | medaforça inativa (0 = inativa) |
| +0xC9 / +0xCA | efeito pendente (`0x2B` '+' trava a peça `+0xCA`; `0x2C` ',' = confusão) |
| +0xDD | fonte da ação escolhida (0 = nenhuma/fallback; 1..3 peça; 4..6 medaforça slot 0..2) |
| +0xE0 / +0xE1 | alvo copiado (bot, peça) |

## 2. Ciclo de especialidade: de onde vem a "personalidade" (confirmado)

`Fight_InitializeBotFromTemplate` 0x080150FC, chamada por `Fight_InitializeEnemyBotStates` 0x080150C0 para cada
inimigo do template.

- Registro do bot no template: 12 bytes em `fight_information + idx*0xC` (idx = posição no template):
  - `+4` = peça/atributo; `+8` = id da medalha (copia 16 bytes de `gMedals` 0x083B62D8 + id*0x10);
  - `+9` = nível (vai para bot+5); `+0xA..+0xD` = 4 bytes de **especialidade**.
- Os 4 bytes viram 8 nibbles (alto, baixo) em bot+0x18..+0x1F. Esse é o **ciclo de especialidade**.
- Assim o "estilo" de um bot = (a) o template de encontro (qual sequência de 8 códigos), (b) o atributo da
  medalha, (c) o nível. Não há tabela separada de "personalidade" por medalha: a sequência vem do template.
  Quais templates existem e onde ficam: `Fight_SelectTemplateForCurrentEncounter` 0x08011E20 (R0; não reli).

Códigos do ciclo (lidos no decompile de `Fight_TrySelectActionFromSpecialityCycle` 0x08015390):

| código | ação |
|---|---|
| 0 | nada (não escolhe; cai no fallback se for o caso) |
| 1, 2, 3 | peça da cabeça / direita / esquerda: `Fight_IsPartActionBlocked`; se livre, usa |
| 4 | **limpa a fonte** (`Fight_SelectActionSource(bot,0)`) e devolve 0 (não cai no fallback) |
| 5, 6, 7 | medaforça slot 0, 1, 2: `Fight_TrySelectMedaforceAction(bot, código-5, 0)` |
| outros | nada |

Correção ao `mtbe.csv` (já anotada em R0a): o código 4 não "cai na cabeça"; limpa a fonte.

## 3. Pipeline de decisão por turno (pseudocódigo)

```
para cada bot ativo do lado da CPU (grupo de 3 a partir de first_bot)      # Fight_SelectActionsForBotGroup 0x0801531C
    se bot+0x40 == 0: pula
    Fight_SelectActionFromCycleOrPendingOverride(bot)                         # 0x08015358
        se bot+0xC9 == ',' (0x2C, confusão):
            Fight_SelectFallbackActionFromPriorityList(bot)                   # 0x08015464
        senão:
            if Fight_TrySelectActionFromSpecialityCycle(bot) != 0:            # 0x08015390
                Fight_SelectFallbackActionFromPriorityList(bot)
    (o ciclo avança: bot+0x3F = (bot+0x3F+1) e volta a bot+0x09 em 8; Fight_AdvanceSpecialityCycleIndex 0x08015414)

Fight_SelectFallbackActionFromPriorityList(bot):                              # 0x08015464
    se bot+0xC1 == 'P' e (randbyte() & 1) != 0 e Fight_TrySelectMedaforceAction(bot,0,random=1) == 0: retorna
    FUN_080156D0: copia 4 bytes de ROM 0x083AD470 + (randbyte()&0x1F)*4 para fi+0x64   # Fight_RandomizePriorityListSlot
    para k in 0..3:                    # varre no máximo 4 entradas
        e = fi+0x64+k; se e == -1: Fight_SelectActionSource(bot,0); retorna
        se !Fight_IsPartActionBlocked(bot, e): Fight_SelectActionSource(bot, e); retorna
    Fight_SelectActionSource(bot,0)   # nada usável

Fight_IsPartActionBlocked(bot, peça)   # 0x080156F8
    bloqueado se: +0xC9 == '+' e +0xCA == peça
               ou (peça == cabeça e bot+0x58 == 0)              # Fight_IsHeadActionUnavailable
               ou bot+peça.+0x59 == 0                            # Fight_IsPartActionUnavailable
               ou Fight_BeginPartActionValidation(bot, peça) (não reli)

Fight_TrySelectMedaforceAction(bot, slot, random):                           # 0x080155A8
    se Fight_IsMedaforceSelectionBlockedByBattleState(...)  (0x08015648): sai com 1
    se fi+0x58 == 0 ou bot+0xC1 != 'P': sai com 1
    se random: slot = Fight_RandomSlotFromTable16() (0x08033074: ROM 0x083AF080 + (randbyte()&0xF))
    repete até 3 vezes:
        slot 0 exige bot+5 >= 10;  slot 1 exige bot+5 >= 30;  slot 2 exige bot+5 >= 60
        se ok: Fight_SelectMedaforceActionSource(bot, slot)  (0x08015668) -> fonte = slot+4; retorna 0
        senão (random): slot -= 1 e tenta de novo
    retorna 1 (nada escolhido)
```

Observação: em `Fight_IsMedaforceSelectionBlockedByBattleState` (= `Fight_IsMedaforceBlockedByChapter`, AI2) 0x08015648 o argumento é o `r0` que sobra
(chamador não passa nada); bloqueia quando bot >= 3 (`arg >= 3`) e gCurrentChapter (byte 0x03005FDF = `DAT_0801565C+DAT_08015660`) <= 2 (AI2; antes dizia > 2). O sentido real
fechado (AI2, confirmado na ROM e no decompile).

## 4. Escolha de alvo (confirmado, decompile de 0x080322C8)

`Fight_ChooseActionTargetSlot` (chamada pelo passo de execução da peça; chamador exato não mapeado):

```
lado_aliado = Fight_GetAllySideBaseIndex(fi+0x5C)        # +0x60 = base do lado (0 ou 3)
# filtro A: 3 bots do lado; candidato se bot ativo && sem técnica 0x16 nas peças && medaforça inativa
#           && bot tem peça com especialidade igual a bot_atual+6 (Fight_HasActivePartWithSpeciality)
# se Fight_HasAnyCandidateTarget() == 0: filtro B (sem a checagem de especialidade)
# se ainda 0: filtro C (ativo && especialidade, sem checar medaforça/técnica)
# se ainda 0: alvo = Fight_PickRandomActiveBotInSide(lado)        # D: aleatório no lado
senão: alvo = Fight_PickRandomCandidateTargetSlot()           # 0x0803301C, sorteia entre os candidatos fi+0x61..
fi+0x5E = alvo + 1       (slot 1-based)
Fight_SetPartActionMetadata(fi+0x5C, fi+0x5D)                 # 0x08033154: copia +0x5E/+0x5F para o registro da peça
```

Sorteios (todos via `randbyte` 0x080424B8, que usa LUT 0x083B29EC — ver README):
- `Fight_PickRandomCandidateTargetSlot` 0x0803301C: sorteia um índice (tabela de 16 via §6) e anda até 3 passos
  (direção por randbyte) buscando candidato != 0 na tabela em fi+0x61.
- `Fight_PickRandomActiveBotInSide` 0x08033090: idem, com bot ativo (+0x40) no lado.
- `Fight_PickRandomIntactPart` 0x080330F4: peça íntegra (+0x59 != 0) do bot, 0..2.
- `Fight_PickRandomBrokenOrMissingPart1Based` 0x0803325C: peça com armadura zero (1-based).
- `Fight_PickMostDamagedRepairablePart1Based` 0x08033184: escolhe a peça de maior dano (+0x4C − +0x59) para reparo
  (determinístico; ordena decrescente).

Efeito de confusão (`+0xC9 == ','`): `Fight_AssignBotTargetSlot` 0x0801B69C troca o alvo por peça de recuperação
(mtbe; não reli nesta rodada).

## 5. Valor da ação (confirmado, decompile de 0x0803769C `Fight_RollPartValue`)

`fi+0xE8 (u16) = bot+0xAB + Fight_GetMedalPartCompatibilityBonus() (0x0803576C) + (randbyte()&7)`
`                + byte(bot + 0x20 + bot[0xA9])`

- Os pesos de `bot+0x20..+0x27` vêm de `Fight_InitializeBotFromTemplate`: para cada passo k:
  `peso[k] = ROM[0x083AD380 + bot[+4]*8 + k] * (bot[+5] >> 1)`, com `0 -> 1` e teto 100.
  Ou seja, peso = linha da tabela pelo atributo da medalha, escalado pelo nível.
- Essa rolagem é o valor usado nos efeitos (dano/potência) — não decide peça nem alvo.

## 6. Tabelas (ROM original; endereços dos literais confirmados)

| endereço | o que é | tamanho | estado |
|---|---|---|---|
| 0x083AD470 | linhas de fallback (`Fight_RandomizePriorityListSlot` copia 1 linha para fi+0x64) | 32 × 4 B | lidas; **conteúdo não bate com ids de peça (valores até 0xF0, sem -1)**: não fechado |
| 0x083AF080 | tabela de 16 bytes sorteada por `Fight_RandomSlotFromTable16` (rand&0xF) | 16 B | lida: `20 21 8A 46 57 46 60 46 3A 1A 98 01 28 18 00 21` (hex) |
| 0x083AD380 | pesos por atributo (linha de 8 B) para `bot+0x20` | ≥ 8 × nº de atributos | lido só o endereço; linhas não lidas |
| 0x083B62D8 | `gMedals` (16 B por medalha) | — | já no mtbe |
| 0x083BA658 | registro de peças (14 B copiados por `Fight_InitializeBotFromTemplate`) | — | endereço só |
| 0x03000590 / 0x03001800 | blocos de bot / fight_information | — | RAM |

Para o Lua, a tabela de 32 linhas (0x083AD470) é o ponto que mais precisa de confirmação antes de portar a
prioridade de fallback. Hipótese (não confirmada): as linhas não são de ids de peça e a leitura da origem
(`Mem_CopyBytes_Dup` com src/dst) pode estar invertida; confirmar lendo a escrita em fi+0x64 no emulador.

## 7. Medaforça pela CPU (resumo)

- Só entra por dois caminhos: código 5..7 do ciclo (sem sorteio), e o fallback com `'P'` + bit aleatório (com sorteio
  de slot por tabela 16).
- Requisitos: `bot+0xC1 == 'P'`, `fi+0x58 == 0`, estado de batalha liberado, nível do bot ≥ 10 / 30 / 60 para slot 0/1/2.
- Correção: R0a dizia "10/29/59"; decompile dá 10 (`<10` falha), 30 (`0x1D < n`), 60 (`0x3B < n`). Confere com MD.md/B3.
- A carga da medaforça (`Fight_AddMedaforceChargeFromAction` 0x0803628C, teto 0x50) não é decisão de IA; ver B3.

## Pendente

- Chamador de `Fight_SelectActionsForBotGroup` (BL em 0x08012BA6): Ghidra não tem função nesse endereço; achar a
  função que a contém (provável driver do turno do lado CPU) com `disassemble_function` ou pelo R0.
- Chamador de `Fight_ChooseActionTargetSlot` (não mapeado).
- Tabela 0x083AD470: confirmar semântica (ver §6).
- `Fight_BeginPartActionValidation` (chamada por `Fight_IsPartActionBlocked`): não lida.
- Linhas de 0x083AD380 e significado de `bot+0xAB`/`bot+0xA9`.
- Não há tabela de "personalidade" separada por medalha: a sequência está nos 4 bytes do template (§2). Quais
  templates usam quais sequências: ler `FightTemplate` (R0 / `Fight_SelectTemplateForCurrentEncounter`).
- Nenhum renome novo: as funções desta cadeia já estavam nomeadas no Ghidra (confirmado pelos decompiles).
