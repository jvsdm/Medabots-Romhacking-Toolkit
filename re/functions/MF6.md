# MF6: efeito de cada id de Medaforce (0x00..0x33), consumo e versões de time (2026-10-17)

Fontes: Ghidra MCP no ar (decompile sem salvar). Lidos nesta rodada: `Fight_DispatchOnSubstateAA_AS` 0x0802793C,
`AV_0D` 0x080288C8, `AV_ApplyStatusToOpponents` 0x080287DC, `BR_26` 0x08030DB4, `BR_45` 0x08031704,
`Fight_StepBotVisualDown` 0x0801AFB8, e a tabela de variantes 0x08028850 (lida direto da ROM). O resto vem de
MF2 (segmentos), MF3 (tabela por id) e MF4 (efeito de cada opcode 0x43..0x6A, já decompilado antes).
Porte: `pc/game/src/fight/medaforce.lua` (tabela EFFECT). Símbolos: `MF6_simbolos.csv`.

## 1. Consumo do medidor: CONFIRMADO (estático)

- `BR_45_SetupMedaforceSlots` 0x08031704: `partEffectMode (fi+0x51) = medaforceCost (bot+0xAA)` = **b2** do descritor.
- `BR_46` roda `Fight_StepBotVisualDown` 0x0801AFB8 por quadro: se `fi+0x51 != 0` e `bot+0xC1 != 0`, faz
  `bot+0xC1 -= 1` e `fi+0x51 -= 1`. Logo: **consumo = min(b2, C1)**, um ponto por quadro.
- `BR_26_BuildPartObjects` 0x08030DB4 também grava fi+0x51 = 0x28, ou 0x50 se `bot+0xC0 & 8`. Como BR_45 vem depois
  (GS 0x35 > 0x16), na medaforça o valor de BR_26 é sobrescrito. O flag 0x8 **não** muda o consumo da medaforça.
  Ainda não confirmado se a medaforça passa por BR_26 antes de BR_45 (irrelevante para o consumo, que vem de BR_45).
- b2 por id (MF2 §3, = offset +2 do descritor): 00:50 01:3C 02:50 03:3C 04:28 05:46 06:28 07:46 08:28 09:46
  0A:3C 0B:1E 0C:32 0D:3C 0E:32 0F:3C 10:32 11:3C 12:32 13:3C 14:46 15:28 16:28 17:32 18:28 19:28 1A:3C 1B:50
  1C:46 1D:32 1E:32 1F:28 20:32 21:28 22:1E 23:28 24:1E 25:28 26:28 27:3C 28:32 29:32 2A:46 2B:46 2C:3C 2D:32
  2E:28 2F:46 30:32 31:46 32:3C 33:46. Ex.: id 0x23 = 0x28 (confere com `battle_tables.lua`).
- ABERTO: confirmar no emulador (medidor 0x50, usar uma medaforça com b2 conhecido, ler C1 antes/depois).

## 2. Efeito por id (tipo, alvo, fórmula, porte)

Legenda: roll = roteiro tem 0x42 (rolagem `E8 = b3+compat+(r&7)+nível(b1)`, `EC = max(1, E8>>3 + b4)`).
"alvo" = quem recebe. Ops entre parênteses = opcodes de MF2 §2 / MF4.

| id | tipo | alvo | efeito (fórmula) | porte em Lua |
|----|------|------|------------------|--------------|
| 00 | ? | ? | ops 06..21 (AS1), sem efeito conhecido | unported |
| 01 | dano | 1 adversário vivo, peça aleatória | `EC` dano em peça (0x50, AR_00) | damage (ok) |
| 02 | dano | alvo + soma do lado | 0x43: `EC += peça_k+2` para peças com byte-2 ∈ {2,3} | damage, soma aberta |
| 03 | dano | alvo | 0x46: `EC += Σ(max−atual)` do alvo | damage, soma aberta |
| 04..09 | status próprio | próprio bot | 0x51 → AS (0x0802793C), estado 7 `AS_07 SetStatusFlagByTechnique` escolhe pelo id da técnica | unported (tipo do status aberto) |
| 0A | suporte | time | 0x52 AT: bônus de suporte (`ApplyTeamSupportBonus`) | team_support, unported |
| 0B | reação | próprio | 0x53 AU: marca de reação (bit do bot) | unported |
| 0C, 0E, 10, 12, 28 | status | adversários | 0x54 → AV (0x080287DC): variante por `bot+0xA8 − 0xC` (§3) | opp_state (aberto §3) |
| 0D, 0F, 11, 13 | status | adversários | AV_0D Bug, AV_10 Freeze, AV_12 Hold, AV_13 Fire (§3). MF2 lê 0x55 nesses ids: CONFLITO | opp_status (aberto) |
| 14 | dano | alvo | alvo + rolagem + 0x47 (cauda) + 0x49 | damage, aberto |
| 15 | cura de status | time | 0x56 AX_06: limpa campos de status dos aliados marcados | team_cure_status |
| 16 | medidor | time | 0x59 BA_08: `EC` dividido (SplitCountByFree) soma em `bot+0x10`, teto `+0x08` | team_gauge |
| 17 | medidor de técnica | adversários | 0x5A BB: `bot+0xBB = 20` nos adversários com cabeça (teto medidor de técnica 0x1F) | tech_gauge_opp |
| 18 | soma | próprio | 0x5B BC: `bot_ativo+0xD1 += EC` (teto 255) | self_add |
| 19 | soma | próprio | 0x5C BD: `bot_ativo+0xD0 += EC` (teto 255) | self_add |
| 1A | reparo | time | 0x57 AY_06: reparo de armadura (valor dividido por capacidade) | team_repair |
| 1B | dano | alvo | alvo + rolagem + 0x49 (metade da armadura do próprio bot) | damage, aberto |
| 1C | restaura | time | 0x58 AZ_06: revive peça quebrada/faltando (SplitCountByMax) | team_restore |
| 1D | dano | alvo | 0x4A: `fi+0x8C += 10` por peça quebrada aliada; + dano 0x44 | unported |
| 1E | status sorteado | marcados | 0x5D BE: 0 Bug, 1 Wave, 2 Fire, 3 Freeze, 4 Impair, 5 Confusion, 6/7 buffs | unported |
| 1F, 21, 23, 25 | boost | marcados | 0x5E BF: `EC` (teto 255) soma no byte bot+1/+2/+4/+5 (escolhido por fi+0x51) | unported |
| 20 | status sorteado | marcados | 0x5D BE (igual 1E) | unported |
| 22 | transfere medidor | 2 bots | 0x5F BG: `EC` sai do registro do bot fi+0x8A (piso 0), entra no do fi+0x89 (teto 0xA0) | unported |
| 24 | boost/drena | aliados/adversários | 0x60 BH: aliados `+EC` no byte bot+1 (teto 0x78); adversários `−EC` | unported |
| 26 | boost secundário | marcados | 0x61 BI: `bot+0xC4=0x26, +C5=0, +C6=fi+0xE8, +C7=EC` | unported |
| 27 | dano cabeça | marcados | 0x62 BJ: subtrai `EC` da peça 0 (cabeça); se zera, quebra; + op 0x21 | unported |
| 29 | status Impair | marcados | 0x63 BK: `bot+0xC9=0x2B`, `+0xCB=fi+0xE8`, `+0xCC=EC` | unported |
| 2A | dano | marcados | 0x64 BL: dano em peça aleatória dos marcados (`Fight_ApplyDamageToPart(…,200)`) | unported |
| 2B | dano | alvo | 0x4B busca bot com peça, 0x4C: `fi+0xEC = armadura da peça` (sobrescreve a rolagem); + dano 0x44 | damage, aberto |
| 2C | drena medidor → dano | adversários | 0x65 BM: `BM_0F` zera `C1` dos adversários marcados e converte em dano. Passo do 2C aberto | unported |
| 2D | medidor/reação | aliados vivos | 0x66 BN: `bot+0xC0 |= 8` (carga dobrada; só afeta BR_26) | unported |
| 2E | soma | marcados | 0x67 BO: `bot+5 += EC` (teto 255). ABERTO: o byte +5 é nível? (MF3) | unported |
| 2F | dano | marcados | 0x68 BP: dano na peça 3 (fixo), depois grava `fi+0xD6+2k` | unported |
| 30 | reação | próprio | 0x69 BQ: `bot+0xC0 |= 1`, sem rolagem | unported |
| 31 | reação | próprio | 0x6A BR: `bot+0xC0 |= 4`, sem rolagem | unported |
| 32 | dano | alvo | 0x4D: `EC = (d1+d2)*10` sobrescreve a rolagem; + dano 0x44 | damage, aberto |
| 33 | dano | bot com grupo | 0x4E acha bot com grupo do ativo (senão aleatório); 0x4F: se nenhum, `EC = 10`; + dano 0x44 | damage, aberto |

Nota de "marcados": os alvos marcados (`+0xD2`) vêm de BE_SelectTargets (0x0802B184), não lido em detalhe.

## 3. Versões de time (AV_0D/10/12/13/2C) e a tabela de variantes

- `Fight_DispatchOnSubstateAA_AV_ApplyStatusToOpponents` 0x080287DC: zera contadores, `opponentSideBase` = lado
  oposto do ativo, e procura o 1º adversário com `presence != 0`, `parts[0].currentArmor != 0`, sem peça de técnica
  16 (`Fight_HasNoTechnique16Part`) e com `bot+0xA8 − 0x0C < 0x1D`. Depois chama a variante pela tabela de salto
  0x08028850 indexada por **`bot+0xA8 − 0x0C`** (bot+0xA8 = byte0 do descritor = id da medaforça, README GH-TA3).
- Tabela 0x08028850 (lida da ROM; índice i = `bot+0xA8 − 0xC`) **[CORRIGIDO no MF7 §5.1: o ponteiro 0x08028850 aponta para 0x08028854; a entrada 0 é o id 0x0C. A leitura abaixo deslocou 1 posição.]**:
  - i=0 (id 0x0C): cabeçalho da própria tabela (lixo do decompile; ABERTO);
  - i=1 (0x0D) → `AV_0D_ApplyBugVirusUnlessMasked` 0x080288C8 (status 13 Bug);
  - i=3 (0x0F) → `AV_10_ApplyFreezeUnlessMasked` 0x08028908 (16 Freeze);
  - i=5 (0x11) → `AV_12_ApplyHoldUnlessMasked` 0x08028944 (18 Hold);
  - i=7 (0x13) → `AV_13_ApplyFireUnlessMasked` 0x08028980 (19 Fire);
  - demais (0x0E, 0x10, 0x12, 0x14..0x2C) → `AV_SkipToNextOpponent` 0x080289E4 (nenhum status).
  - `AV_2C_ApplyConfusion` 0x080289BC (Confusion 44) **não** está nessa tabela: alcance ABERTO.
- `AV_0D` (e variantes): para cada adversário (3 do lado oposto, pulando os inválidos), se
  `Fight_IsPartStatusProtected(bot, 0x0D) == 0`: marca `targetPartFlags` e chama
  `Fight_SetPendingPartStatus(bot, tech, fi+0xE8 (ROS), fi+0xEC (dano))`. Ao acabar, avança o substate.
- **CONFLITO (ABERTO, prioridade):** pelo MF2, os ids 0x0D/0x0F/0x11/0x13 leem `55` (proteção do time) e os
  0x0C/0x0E/0x10/0x12 leem `54` (estado nos adversários). A tabela de AV ativa status justamente para 0x0D/0x0F/0x11/0x13.
  Ou o campo `bot+0xA8` não é o id da medaforça no momento do despacho (ex.: é a técnica do bot), ou a leitura de
  MF2 dos segmentos está deslocada de 1 id. Resolver antes de tratar o `opp_status` como efeito real.
  Em Lua (`medaforce.lua`): tipo `opp_status` com o status da tabela, e a marca "CONFLITO" no efeito.

## 4. Pendências (com o que falta)

1. Confirmar no emulador o consumo = b2 (medidor 0x50, id com b2 conhecido, C1 antes/depois).
2. Resolver o conflito de §3 (id 0x0D: 55 ou AV-status). Ler a origem de `bot+0xA8` no despacho.
3. Passo do 2C (BM) e do 2D (efeito de `bot+0xC0 |= 8` só em BR_26 ou também na medaforça).
4. Campos sem nome: bot+1, +5, +8, +0x10 (0x59/0x60/0x67), +0xD0/+0xD1, bit +0xC0 bits 1/4 (0x69/0x6A).
5. Status por 0x51 (ids 04..09): AS_07 SetStatusFlagByTechnique 0x08027E1C, não decompilado nesta rodada.
6. BE_SelectTargets (0x0802B184): quem são os "marcados" de 1E/20/29/2A/2E.

## 5. MF7 (2026-10-18): fechamento dos abertos de §3 e §4

Fontes: ROM original lida com `python -I` (words de 0x08028850..0x080288C4), Ghidra decompile de `BM_0F` 0x0802EAD8
(única função decompilada nesta rodada), e as entradas já confirmadas em `AS6`, `AW_00/09` (csv), `HN4` (BE) e `DM.md`.

### 5.1 Conflito 0x55 × AV: RESOLVIDO

- **Correção de §3:** a tabela de handlers começa em **0x08028854** (0x08028850 é só o ponteiro literal). Entrada i = id
  `0x0C + i`; são 29 entradas. Lidas da ROM: id 0x0C → AV_0D 0x080288C8; 0x0D → 0x080289E4 (nenhum); 0x0E → AV_10
  0x08028908; 0x0F, 0x11, 0x13 → nenhum; 0x10 → AV_12 0x08028944; 0x12 → AV_13 0x08028980; 0x14..0x27 → nenhum;
  0x28 → AV_2C 0x080289BC. O "lixo" da entrada 0 em §3 era o deslocamento de 1 posição.
- **bot+0xA8 é o id da Medaforce no despacho?** Sim. Único gravador: `Fight_SelectMedaforceActionSource` 0x08015668
  (TA3/MD3). Leitores que usam o id: AV_03 0x080287DC (índice = id − 0x0C), AW_00 0x08028C68 ("variante = bot_ativo+0xA8"),
  AS_07 0x08027E1C. A ação normal não grava bot+0xA8 (MD3 §3).
- **Quem aplica status no time inimigo (op 0x54 → AV):** 0x0C = Bug (0x0D), 0x0E = Freeze (0x10), 0x10 = Hold (0x12),
  0x12 = Fire (0x13), 0x28 = Confusion (0x2C). Cada um rola (0x42) e aplica nos adversários vivos com cabeça.
- **Quem dá proteção ao próprio time (op 0x55 → AW 0x08028C40):** 0x0D = Bug, 0x0F = Freeze, 0x11 = Hold, 0x13 = Fire.
  AW_00 grava a variante 0..3 (0x0D→0, 0x0F→1, 0x11→2, 0x13→3). AW_07 marca os aliados com cabeça inteira. AW_09
  (0x08029354) liga bit em B+0xCF desses aliados: variante 0 → 0x01, 1 → 0x08, 2 → 0x02, 3 → 0x04. Esses bits são
  os mesmos do `guardBit` de `status.lua` (Bug 0x01, Freeze 0x08, Hold 0x02, Fire 0x04). AW_0A põe a mensagem
  0x55/0x58/0x56/0x57 (variante 0..3).
- Os nomes dos handlers AV_0D/AV_10/AV_12/AV_13 batem com os status 0x0D/0x10/0x12/0x13 de BB.md. A proteção de
  0x0D..0x13 cobre o par do status do ataque (0x0D/0x0E etc.). **Status:** confirmado (ROM + csv de decompile).

### 5.2 AS_07 (ids 0x04..0x09, op 0x51): confirmado

- AS (0x0802793C) passo 07 = `SetStatusFlagByTechnique` 0x08027E1C: id = bot ativo+0xA8. Seta bit em **B+0xD6**
  (bot, não fight): 04 → 0x01, 05 → 0x08, 06 → 0x02, 07 → 0x10, 08 → 0x04, 09 (ou outro) → 0x20.
- Passo 08 (0x08027EDC): mensagem em fight+0xA6: 04 → 0x4A, 05 → 0x61, 06 → 0x4B, 07 → 0x62, 08 → 0x4C, outro → 0x63.
- Sem HP, sem alvo. Só marca no próprio bot. (AS4 dizia que o 09 seta 0x20 e que AS_00 grava fight+0x51=5; a versão
  correta é AS6 §5.) **Ainda não lido:** quem lê B+0xD6 e o que cada bit faz no jogo.

### 5.3 BE_SelectTargets 0x0802B184 (quem é "marcado"): confirmado (HN4)

- Alvos: adversários vivos, com cabeça (+0x59 != 0) e sem Stability (técnica 22, `Fight_HasNoTechnique16Part`).
  Se o id ativo é **0x1E**: marca (valor 1) quem tem **+0x91 == 0x64**. Senão: marca (valor 2) quem tem **+0x91 == 0x6A**.
  Nenhum marcado: fim sem efeito. Algum marcado: fight+0xAA += 2.
- **+0x91** = tipo de perna (DM.md §6: `IsAntiAir` usa 0x64 'd' = voadora; `IsAntiSea` usa 0x6A 'j' = aquática). AS6
  §4F chama o mesmo campo de "grupo". Nome aberto (perna vs grupo); o valor é o mesmo.
- Ids que chamam BE: 0x1E e 0x20 (op 0x5D). Os outros marcados vêm de BF (aliados) ou AW_07 (aliados com cabeça).
- Porte: `M.markBE(fight, bot, id)` em `medaforce.lua`.

### 5.4 Passo do 0x2C (op 0x65 → BM): BM_0F

- `Fight_DispatchOnSubstateAA_BM` 0x0802E424 é um stub de 32 bytes que segue a tabela de passos. O passo do 0x2C é
  **BM_0F** 0x0802EAD8 (decompile desta rodada). Para cada k = 0..2 com marca fi+0xD2+k != 0 (alvo oppSideBase+k):
  1. `bot_alvo.medaforceGauge` (C1) = 0. Drena o medidor do alvo.
  2. parte = `Fight_RollStatusRowRandomEntry` (sorteio); fi+0xD2+k = parte | (partBreakFlag << 7).
  3. fi+0xD6+2k = fi+0xEC (EC da rolagem); `Fight_SplitDamageByPartArmor(alvo, parte, EC)`.
  4. armadura da peça −= EC (char). `Fight_BackupHalvedStatsIfFlagClear`. Se a cabeça zerou: `Fight_QueueBrokenBotForResolution`.
  - A nota antiga do csv (BM_0F) falava em "slot D6 / fight+0xC8". O decompile mostra o valor **EC (fi+0xEC)**; o csv
    deve ser corrigido (ver `MF7_simbolos.csv`).
- **Origem das marcas do 0x2C: ABERTO.** O roteiro do 0x2C (MF2 §2: 0x42 + 0x65) não chama BE. O porte usa todos os
  adversários vivos e avisa no código (`aberto`).
- Porte: `M.drainMeter` + kind `drain`. Ganho de medidor/dano: sem emulador.

### 5.5 Ids ainda sem efeito portado (EFFECT em `medaforce.lua`)

Portados nesta rodada: 0x04..0x09 (`self_flag`, AS_07), 0x0C/0x0E/0x10/0x12/0x28 (`opp_status`, AV), 0x0D/0x0F/0x11/0x13
(`team_protect`, AW), 0x17 (BB, bot+0xBB = 20), 0x18/0x19 (BC/BD, soma em B+0xD1/+0xD0, teto 255), 0x2C (`drain`, BM_0F),
0x2D (BN, c0 |= 8 nos aliados com cabeça), 0x30/0x31 (BQ/BR, c0 |= 1/4).

Ainda não portados (kind `unported`): 0x00 (AS1 pendente), 0x0A (AT suporte), 0x0B (AU reação), 0x15 (AX cura de status,
B+0xC9), 0x16 (BA, SplitCountByFree), 0x1A (AY reparo), 0x1C (AZ restaura), 0x1D (4A + 0x44), 0x1E/0x20 (BE: marcação
portada, status sorteado não), 0x1F/0x21/0x23/0x25 (BF boost), 0x22 (BG transfere medidor), 0x24 (BH), 0x26 (BI),
0x27 (BJ), 0x29 (BK Impair), 0x2A (BL), 0x2E (BO, +5), 0x2F (BP).

### 5.6 Campos sem nome (bot+1, +5, +8, +0x10)

Não apareceram nos handlers lidos nesta rodada (AV, AW, AS_07, BE, BM). Continuam abertos. +0x10 aparece em BA_08 (0x16)
e +5 em BO (0x2E), ambos não lidos.

### 5.7 Testes

`pc/game/tests/test_medaforce.lua`: `OK 71` (inclui os casos MF7: protecao, AS_07, 0x28 Confusion, drain do 0x2C, BE,
0x17/0x2D/0x30). O teste antigo do id 0x0D passou a usar 0x0C (era erro de §3). Sem emulador: nada disso foi visto no jogo.
