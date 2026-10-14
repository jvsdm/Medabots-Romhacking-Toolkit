# MD: medalhas (gMedals) e fórmula ROS -> dano (GH-MD, 2026-10-14)

Leitura estática (Ghidra + ROM, só leitura). ROM = 0x08000000 + offset. Bloco do bot = 0x03000590 + bot*0x100
(literais 0x08035798, 0x08035BCC, 0x08038F80, 0x080376F0 = 0x03000590). `fight_information` = 0x03001800
(literal 0x08038F4C). Confiança: "confirmado" = decompile + literal; "provável" = padrão de uso.

## 1. gMedals 0x083B62D8 — 30 registros de 0x10 B (índice = id)

Refs na ROM (literais): 0x08005D8C, 0x08005EF4, 0x0800605C, 0x080061C4 (desenha_stats_parte e variantes),
0x0800B590, 0x08012958 (Fight_AwardPartExperience), 0x080151D4 (Fight_InitializeBotFromTemplate),
0x08017F50, 0x08041E28 (Medabot_SpawnFromRecord). Os endereços 0x0800B590 e 0x08017F50 não caíram em função
no Ghidra (provável: função de inicialização ou UI; não lida).

| Campo | Uso confirmado | Confiança |
|---|---|---|
| +0x00 | id (0..26; 27..29 = 0x1B..0x1D) | confirmado |
| +0x01 | repete o id (27..29 = 0x1A). É o atributo comparado com o byte 0 de cada peça | confirmado (Fight_GetMedalPartCompatibilityBonus) |
| +0x02 | copiado para o bot (+6); uso no cálculo não lido | provável: classe |
| +0x03..+0x06 | 4 limiares de nível (rank). Medalha 0 = 10/30/60/100. 0 ou 0xFF = sem limite | confirmado (Stage_FromLevelThresholds) |
| +0x07 | bônus de compatibilidade, somado por peça que casa (bot+7) | confirmado |
| +0x08..+0x0F | 8 bases de especialidade (uso em Medabot_SpawnFromRecord e no template do inimigo) | provável |

Leitores:
- `desenha_stats_parte` 0x08005D1C (+ `_b` 0x08005E84, `_d` 0x08006154): copia o registro para `+0x10E` do estado da
  peça e compara `GetPartStats[0]` com o byte +1 (marcador de peça compatível).
- `Fight_GetMedalPartCompatibilityBonus` 0x0803576C: para as 4 peças (`B+0x48+0x18k`), se o byte bot+4 (= +1 da medalha)
  é igual ao byte 0 da peça, soma bot+7 (= +7 da medalha).
- `Fight_InitializeBotFromTemplate` 0x080150FC (inimigos 3..5): medalha id = template+8; bot+1 = id; bot+4 = medalha+1;
  bot+6 = medalha+2; bot+7 = medalha+7; nível do bot bot+5 = template+9. Especialidade `bot+0x20+k` (k=0..7) =
  `clamp(1..100, T[(bot+4)*8+k] * (bot+5 >> 1))` com `T` = tabela de 8 bytes em 0x080151E8 (não identificada).
- `Medabot_SpawnFromRecord` 0x08041D70 (jogador): copia gMedals[id] para scratch; no primeiro slot livre de
  0x40 B (flag +0x80) grava id (+1), +4 = medalha+1, +6 = medalha+2, +7 = medalha+7; nível (+5) =
  `Level_ComputeReferenceFromSlots()`; estágio (+8) = `Stage_FromLevelThresholds(nível, +3, +4, +5, +6)`;
  XP (+2, u16) = `(nível+1)*100`; especialidade `+0x20 = B + B*(nível/10)` com `B = medalha+8`, teto 100.

## 2. Limiares de medalha: gMedals +3..+6 versus 10/29/59 do R0a

Não há divergência de valores; são cortes de **nível** (byte `slot+5`), não de XP.

- `Fight_TrySelectMedaforceAction` 0x080155A8 (lê `bot+5`): slot 0 se `nível >= 10`; slot 1 se `0x1D < nível`
  (= `>= 30`); slot 2 se `0x3B < nível` (= `>= 60`). Os cortes estão FIXOS no código; não lêem gMedals.
  Ou seja, o R0a (10/29/59) e os +3/+4/+5 da medalha 0 (10/30/60) são a mesma régua escrita com `>` e `>=`.
- `Fight_AwardPartExperience` 0x080127DC: ao subir de nível, dispara evento de medaforça em nível 10, 30 e 60
  (comparações com `'\n'`, `0x1E`, `'<'` no byte +5 da peça). Os 3 bytes por medalha vêm de uma tabela de 3 B
  em 0x08012968 + id*3 (provável: lista de medaforça da medalha, a mesma usada por Fight_SelectMedaforceActionSource).
- Nível x XP: `slot+5` = nível (0..99; `'d'` = 100 = máximo, não ganha XP). `slot+2` (u16) = XP restante até o próximo
  nível, reiniciado em `(nível+1)*100`. Ao zerar, o nível sobe. XP = contador por nível; o corte é o nível.
- Estágio: `Stage_FromLevelThresholds` 0x080420C4 devolve 4 se `nível >= p5`, 3 se `>= p4`, 2 se `>= p3`, 1 se
  `>= p2`, senão 0 (valor 0 não conta). Chamadores: 0x080127DC e 0x08041D70 (ambos com os limiares da medalha).

## 3. Fórmula ROS -> dano (ação do bot)

Bloco do bot `B = 0x03000590 + bot*0x100`. Peça k (k = 0..2 no bot; 0 = cabeça): base `B + 0x48 + 0x18*k`.
Campos (a partir da base da peça): +0 atributo (casa com medalha+1), +1 tipo de ação, +4 armadura máx (B+0x4C),
+6 `power_or_evasion`, +0xC `attack_damage_scalar`, +0x11 armadura atual (B+0x59). `fi` = 0x03001800.

Passo 1 — ROS: `Fight_RollPartValue` 0x0803769C (`void f(bot)`):
```
ros = B.ab (+0xAB)
ros += Fight_GetMedalPartCompatibilityBonus(bot)      # 0x0803576C
ros += randbyte() & 7                                  # randbyte 0x080424B8
ros += B[0x20 + B.a9]                                  # especialidade indexada por +0xA9
fi.ros (+0xE8, u16) = ros
```

Passo 2 — gasto: `Fight_SpendRos20ReturnsExhausted` 0x0803725C:
```
fi.ros = max(0, fi.ros - 0x20)
return fi.ros == 0       # 1 = ROS insuficiente (+0xA6 = 0x1A)
```

Passo 3 — valor de ataque: `Fight_ScaleAttackValueByPartScalar` 0x08037280 (`void f(bot, peca)`):
```
v = (s16)fi.ros >> 2
v += peca.power_or_evasion                 # +6
v = aux_div_s32(v, peca.attack_damage_scalar)   # +0xC; 0x0807CD2C; divisor 0 -> 0
if v == 0: v = 1
fi.ataque (+0xEC) = v
```
Para efeito (sem peça): `Fight_ComputeEffectValueFromRos` 0x080376F4: `(ros>>3) + B.ac (+0xAC)`, 0 -> 1 (a primeira escrita é morta).

Passo 4 — bônus e crítico:
- `Fight_AddStrengthenDamageBonus` 0x08035B9C: para cada peça do bot com a armadura atual (+0x11) != 0 e o byte +1
  igual a `'5'`, soma `peca+5` em `fi+0xEC` (o ponteiro é 0x030018EC). Obs.: o decompile mostra o caractere `'5'`
  (0x35, confirmado no disassembly: `cmp r0,#0x35`); o mtbe diz 0x05, errado (ver pendências).
- `Fight_TryApplyCriticalRosOverride` 0x08038F34 (`int f(limiar)`): se `limiar < fi.ros`, zera `fi+0xEE`, marca no
  alvo (`B_alvo+0xBC |= 0x4C`) e grava `fi+0xF2 = fi.ros`. Retorna a condição. (O comentário do mtbe inverte a
  comparação; o código é `limiar < ros`.)

Passo 5 — acumular e aplicar: `Fight_AccumulateBaseDamage` 0x08033D30 (grava `fi+0xEC`) e `Fight_ApplyDamageToPart`
0x08034F68 (`bot, peça, dano`). Não lidos nesta rodada (pendência).

Resumo:
```
ros  = ab + compat(bot) + rand&7 + espec[a9]          # 0x0803769C
ros  = max(0, ros-0x20); se 0: falha                    # 0x0803725C
atq  = max(1, ((ros>>2) + peca.pow) / peca.escala)      # 0x08037280
atq += bonus_forca(bot)                                  # 0x08035B9C
crit se limiar < ros: alvo |= 0x4C                       # 0x08038F34
dano = Fight_AccumulateBaseDamage(...)                    # 0x08033D30 (passo 5)
aplicado = Fight_ApplyDamageToPart(alvo, peca, dano)     # 0x08034F68 (passo 6)
```

Passo 5 — `Fight_AccumulateBaseDamage` 0x08033D30 (lido nesta rodada; atacante = fi+0x87, alvo = fi+0x88, peça usada =
fi+0xA7). Tudo em `fi+0xEC` (u16), partindo de `fi+0xEA` (pré-dano, semeado por `Fight_SeedPreDamageFromTotalRos`
0x08033D10 segundo o mtbe; não lido):
```
d = fi.pre (+0xEA) + atk.peca[sel].power (+6)
d += atk.B[+0xD0]
if peca_sel.byte2 (+2 do registro) in {0,1}: d += atk.B[+0xD3]
if peca_sel.byte2 in {2,3}:                  d += atk.B[+0xD4]
if peca_sel.byte2 == 1:                      d += atk.B[+0xA2] >> 1
if peca_sel.byte1 (+1) in {4,5}:             d += peca_sel.power
if atk.B[+0xD6] & 1:                         d += peca_sel.power
if peca_sel.byte1 == '4' (0x34) and ramo fi[0x8A*8+0x2B] == '3': d += fi[...+0x2D]   # ramo especial
for each peca k do atacante: Fight_AddStrengthenDamageBonus -> d += peca_k.power se peca_k.byte1 == 0x35 e armadura != 0
d -= tgt.B[+0xD1]  (safe, nao vai abaixo de 0)
if fi+0xDE != 0 and fi+0xDF == 0x1B: d -= tgt.B[+0xD8] (u16)
Fight_AdvanceActionScriptCursor()
```
Observação: o byte +2 do registro de peça (`B+0x4A` no Ghidra) define o modo do dano; o campo aparece com os valores 0..3
(provável: tipo de alcance/defesa). Não identificado.

Passo 6 — `Fight_ApplyDamageToPart` 0x08034F68 `(bot, peça, dano)`: NÃO reduz armadura diretamente. Com
`a = armadura atual (B+0x59 + 0x18*peça)`: se `dano < a`: `fi+0xC8 = fi+0xCC = dano`, `fi+0xCA = 0`, `fi+0xD1 = 0`;
senão: `fi+0xC8 = fi+0xCC = a`, `fi+0xCA = dano - a` (excedente), `fi+0xD1 = 1` (peça quebrou). Confirmado (decompile);
os campos de exibição/excedente são usados pelos chamadores (provável).

## 4. Pendências

- Tabela de 8 bytes `0x080151E8` (inimigo) e base de especialidade `0x08041E70` (jogador): não identificadas.
- `Level_ComputeReferenceFromSlots` (nível de referência): não lido.
- Fight_AddStrengthenDamageBonus: confirmado por disassembly que o literal é `cmp r0,#0x35` (ASCII '5'), e não 0x05
  como diz o mtbe. Mesmo padrão de caractere em outros pontos de Fight_AccumulateBaseDamage ('4', '3'): o byte +1
  mistura códigos ASCII e valores pequenos (aberto).
- Endereços 0x0800B590 e 0x08017F50 (literais de gMedals): Ghidra não tem função que os contém (aberto).
