# TPL: template de encontro (0x28 B) e montagem do time inimigo (2026-10-17)

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
