# TPL: template de encontro (0x28 B) e montagem do time inimigo (2026-10-17)

> **Atualizado em TPL2 (Ghidra no ar):** os pontos marcados como "ABERTO" ou "provavel" nas secoes 2 a 5 abaixo foram fechados ou corrigidos na secao 8. Em especial: bracos e pernas NAO sao a cabeca (sao rec+5..+7); a linha da habilidade e o `attribute` da medalha; niveis 0..1 usam a base sem multiplicar.

Rodada curta. Ghidra MCP estava OFFLINE (porta 8080 recusada): nenhum decompile novo nesta rodada.
O layout abaixo sai dos bytes do cache (`pc/cache/battle_tables.lua`, raw hex de 246 templates) e do que já
estava em EN.md, AI.md, RM7 e RV3 (README). Confiança: "confirmado" = já visto no decompile em EN.md;
"medido" = contagem nos 246 templates da ROM; "provável" = padrão de dados; "ABERTO" = não fechado.

## 1. Fluxo (resumo; detalhe em EN.md)

Template id -> `gEncounterTemplatePtrs` 0x083C1B80[id] -> 0x28 B em `fight_information` 0x03001800 ->
`Fight_InitializeEnemyBotStates` 0x080150C0 -> `Fight_InitializeBotFromTemplate` 0x080150FC (k = 0..2).
Id do encontro aleatório = `gMapEncountersExtra` 0x083BF210[mapa] (t0..t3, pesos 40/30/20/10; EN §2).

## 2. Registro de bot (0xC B por k; base = 0xC*k)

| Offset (no template) | Campo | Destino no bot (0x03000590 + (3+k)*0x100) | Confiança |
|---|---|---|---|
| +0 | char_id (fala antes da luta) | — | confirmado (leitura) |
| +3 | flags (1 nos 246 templates; só o do k=0 é lido) | — | medido |
| +4 | **id da cabeça** = id de bot; indexa `gPartRecordPtrTable` 0x083BA658 | peça 0 (cabeça) | confirmado (EN §5) |
| +5, +6, +7 | braço D, braço E, pernas (provável) | — | ver §4 |
| +8 | medalha (0-based, índice de gMedals 0x083B62D8, 0x10 B) | bot+4 = medalha+1 (jogo); bot+1 = medal[0] | confirmado (EN §5) |
| +9 | nível | bot+5 (usado em skill = base * (nível>>1)) | confirmado |
| +0xA..+0xD | 4 bytes de especialidade (ciclo) | bot+0x18..+0x1F (8 nibbles, alto e baixo) | confirmado (cópia, EN §4.1); janela ABERTA, ver §5 |

Tail do template: +0x24..+0x27 = 4 bytes (zerados em 64 templates; ex.: `51 30 03 00`). ABERTO (sem leitor).

Contagem de bots (medida): 3 registros com nível > 0: 198 templates; 2: 34; 1: 4; 0: 10.

## 3. Medalha e nível

- Medalha = +8, nível = +9 (ver tabela §2). `bot.medal_id` = +8 (0-based); `bot.level` = +9.
- Habilidades da CPU: `T[(bot+4)*8 + k] * (nível >> 1)`, mínimo 1, teto 100 (EN §5; DM §8.1). A linha da tabela
  usada no código (`enemy_skill_base[medal_id]`) difere do texto do DM (attr = medal+1): ABERTO, ver `encounter.lua`.

## 4. Peças do inimigo (cabeça, braços, pernas)

- **Cabeça**: +4, confirmado (EN §5 lê `0x083BA658[rec+4]`). Com template flags == 0 (nenhum dos 246), a
  Fight_SelectTierPartId 0x08011F20 sortearia entre +4..+7 (40/30/20/10); com flags == 1 usa +4 direto.
- **Braços e pernas**: o caminho confirmado (EN §5) não lê +5..+7. Medido: nos bots com nível > 0, os 4 bytes
  +4..+7 são **iguais em 665 de 666** registros (o único caso diferente: `51 51 52 52`). Por isso o porte usa
  o id da cabeça também para braços e pernas (`provável`: o bot carrega as próprias peças). ABERTO: o leitor
  de +5..+7 e o caso `51 51 52 52` (talvez variantes por raridade) não foram achados (Ghidra offline).
- Como o jogo acha a peça a partir do id de bot: `battle.lua` `makeEnemy` usa `parts.lua[eq*4+k]`
  (id de bot por tipo de peça). Por isso o porte devolve ids de bot, não registros de peça.

## 5. Ciclo de especialidade da CPU

- Bytes: rec+0xA..+0xD (4 bytes) -> 8 nibbles -> `bot.cycle_codes[0..7]` (ordem: alto, baixo, do byte 0 ao 3).
- Leitor: `Fight_TrySelectActionFromSpecialityCycle` 0x08015390 (ação = `byte[bot+0x18 + byte(bot+0x3F)]`,
  RV3 no README, decompile confirmado); avanço do índice `Fight_AdvanceSpecialityCycleIndex` 0x08015414.
- `bot.cycle_base = 0` (início em bot+0x18); `bot.cycle_index = 0` (bot+0x3F, começa em 0).
- **ABERTO (importante)**: a janela rec+0xA..+0xD sobrepõe o registro seguinte (rec+0xC = char do k+1), e
  o RM7 do README diz "+0x18..+0x27 = especialidade". As duas leituras são compatíveis com os nibbles
  (todos os 246 templates têm nibbles <= 7 nas janelas testadas: +0x0A, +0x18, +0x22, +0x24), então os
  dados NÃO decidem. Implementado: rec+0xA..+0xD (EN §4.1, "confirmado (cópia)"). Confirmar no decompile do
  0x080150FC antes de confiar no ciclo.
- B+0x09 (pedido na tarefa): NÃO resolvido. EN §5 diz bot+9 = rec+0xE (= char do registro seguinte), aberto.

## 6. Pendências

1. Reabrir Ghidra (porta 8080) e decompilar 0x080150FC e 0x08011F20: fechar a janela do ciclo e o leitor de +5..+7.
2. Explicar o caso `51 51 52 52` (tier por raridade ou peça de outro bot).
3. Bot+0x09, bot+0x3F e o offset da cabeça (+0x0A x +0x4C, conflito já citado no EN §5).
4. Medalha: linha de `enemy_skill_base` (medal_id x medal+1).

## 7. Mapa e cache (correção desta rodada)

- `battle_tables.lua` grava `map_encounter_extra[m] = {t0=..,t1=..,t2=..,t3=..}`; `encounter.pickTemplate` lia
  `e[1]..e[4]` e nunca sorteava no jogo. Corrigido (aceita os dois formatos) e testado contra o cache
  (mapa 1 = 155, 245, 155, 167).

## 8. Rodada TPL2 (Ghidra no ar; decompile confirmado)

Funcoes lidas: 0x080150FC e 0x08011F20 (pedidas), mais 0x08011E20, 0x080150C0, 0x0801520C, 0x08014F90, 0x08015414
(e 0x080151E8 e 0x083AD380 pelos bytes da ROM). Confianca entre parenteses.

### 8.1 De onde vem as 4 pecas (corrige secao 4)

- Sorteio: `Fight_SelectTemplateForCurrentEncounter` 0x08011E20 copia o template e, so se fight+3 (flags) == 0,
  roda 4 vezes por registro k=0..2 `Fight_SelectTierPartId` 0x08011F20 (40/30/20/10) sobre rec+4..+7, lidos antes da
  escrita, e grava o resultado de volta em rec+4..+7. Cada peca sorteia de novo (confirmado). Com flags == 1 nao ha
  sorteio. O sorteio de template desse mesmo caminho e inline, nao usa 0x08011F20 (confirmado).
- Ordem: rec+4 = cabeca, rec+5 = braco D, rec+6 = braco E, rec+7 = pernas (confirmado: `Fight_BuildBotPartDisplayStats`
  copia rec+4..+7 para partIds do bot; `Fight_InitializePartStates` faz parts[0..3] <- partIds[0..3]).
- Consequencia: o codigo antigo (`rarm = larm = legs = head`) estava ERRADO. O "665 de 666 iguais" e so um fato dos
  dados (quase sempre as 4 pecas sao iguais), nao uma regra.
- `Fight_InitializeBotFromTemplate` 0x080150FC NAO le rec+5..+7; quem le e a dupla 0x0801520C/0x08014F90, chamada por
  `Fight_InitializeEnemyBotStates` 0x080150C0 (confirmado).
- Bots: o loop de 0x080150C0 vai ate `enemyBotCount` (byte de fight_information, nao lido aqui). O filtro
  "nivel > 0" do porte e a contagem medida em EN §4, nao a regra do codigo (ABERTO: origem de enemyBotCount).

### 8.2 Ciclo de especialidade e janela rec+0xA..+0xD (corrige secao 5)

- Nibbles: le 4 bytes em rec+0xA..+0xD (rec = 0x0C*k); cada byte vira (alto, baixo) em bot+0x18+2j e +2j+1 (confirmado).
- A janela SOBREPOE o registro seguinte na leitura: o codigo le rec+0xC/+0xD mesmo sendo char/+1 do k+1 (confirmado).
  Os dados nao decidem; o codigo le esses bytes.
- **B+0x09 = rec+0xE** (`specialityCycleReset`, confirmado pela escrita em 0x080150FC). `Fight_AdvanceSpecialityCycleIndex`
  0x08015414: bot+0x3F += 1; ao chegar em 8, bot+0x3F = bot+9 (confirmado). `ai.lua` ja faz isso (`cycle_base`).
  O porte agora poe `cycle_base = rec+0xE` (antes era 0).
- Tail +0x24..+0x27: ABERTO. Nenhuma das funcoes lidas le esses bytes (nem o loop de 0x080150C0). So dados.

### 8.3 Tabela de habilidade da CPU (corrige DM §8.1 e a linha da medalha)

- Literal 0x080151E8 = 0x083AD380 (confirmado pelos bytes da ROM).
- Indice = `attributeId*8 + k`, com `attributeId` = byte+1 do registro de medalha copiado de gMedals 0x083B62D8 (16 B),
  e medalId = byte+0 (confirmado). Ou seja, a linha e o **attribute** da medalha: nao "medalha+1" e nao o indice do template.
- Byte+0 do registro = indice da medalha nas 30 (confirmado): bot+4 = indice, SEM +1 (o texto "bot+4 = medalha+1"
  de PORT_BATALHA.md esta errado). Byte+1 = indice para 0..26; medalhas 27, 28, 29 tem attribute 26 (confirmado nos bytes e no cache).
- Resposta: **a linha 0 e a medalha 0 (Kuwagata)**, porque o attribute dela e 0. DM §8.1 ("attr = medalha+1") esta errado.
  O porte passa a usar `medal.attribute` (cache) com fallback no indice.

### 8.4 Nivel 0..1 (resolve DM §8.1 vs EN §5)

- Codigo (confirmado): `mult = nivel >> 1`; se mult != 0, `base*mult`; depois 0 -> 1, > 100 -> 100.
- Logo nivel 0..1 usa a base (clamp 1..100). DM §8.1 esta certo; EN §5 ("minimo 1") esta errado.
  Porte corrigido em `encounter.lua` (`enemySkills`) e teste "nivel 1 -> base sem multiplicar (4)".

### 8.5 Mudanca no porte (encounter.lua) e pendencias

- `buildEnemyTeam` agora: (1) sorteio das pecas por registro k=0..2 se flags == 0 (4 rolagens cada, mesmo nivel 0);
  (2) bots com nivel > 0 com `head/rarm/larm/legs = rec+4..+7`; (3) `cycle_base = rec+0xE`; (4) skill por `attribute`.
- Pendencias: origem de enemyBotCount; tail +0x24..+0x27; `setup.lua` ainda poe `cycle_base = 0` e monta bots
  proprios (arquivo do Opus; nao editado); PORT_BATALHA.md diz "bot+4 = medalha+1" (errado, ver 8.3).
