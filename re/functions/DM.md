# DM: fórmula de dano/ROS/acerto completa (GH-DM, 2026-10-14)

Revisão de `MD.md` seção 3. Leitura estática (Ghidra decompile + xrefs + ROM só leitura, `python -I`).
ROM = 0x08000000 + offset. Confiança: "confirmado" = decompile + literal/bytes; "provável" = padrão de uso;
"aberto" = não fechado nesta rodada.

## 0. Notação

- `F` = fight_information 0x03001800 (literal confirmado em 0x08033D2C, 0x080357F8, 0x08038F4C, 0x08038FF0, 0x08039130).
  `F+0x87` = atacante (bot idx); `F+0x88` = alvo; `F+0x89` = slot de suporte; `F+0x8A` = ramo/índice de registro;
  `F+0xA7` = peça usada k (0 cabeça, 1 braço D, 2 braço E, 3 pernas); `F+0xA8` = índice de roteiro; `F+0xBC` = cursor do roteiro (u16).
- ROS: `F+0xE8` (u16) = total de ROS; `F+0xEA` (s16) = pré-dano; `F+0xEC` (u16) = acumulador de dano/ataque;
  `F+0xEE` (u16) = redução do defensor (scratch "EE"); `F+0xF2` = ROS resolvido (cópia).
- Bloco do bot `B(b)` = 0x03000590 + b*0x100 (literal confirmado em 0x0803587C, 0x080357FC, 0x08038F80, 0x08038F04, 0x08038FF4, 0x08039110, 0x080358AC...).
- Peça `P(b,k)` = B(b) + 0x48 + 0x18*k. Campos de P (offset a partir de P): +0 medalha, +1 **técnica** (id 0..59),
  +2 **especialidade** (0..7), +3 gênero, +4 armadura máx, +5 sucesso, +6 poder, +7 carga, +8 usos, +0xC escala de ataque, +0x11 armadura atual.
  Pernas (k=3): P+1 = **tipo de perna** (100..106), P+6 = propulsão (B+0xA2), P+7 = evasão (B+0xA3), P+8 = defesa (B+0xA4),
  P+9 = proximidade (B+0xA5), P+0xA = distância (B+0xA6). Confirmado: gPartStats kind 3 (GRAVE: leg 100, prop 67, eva 48, def 37, prox 14, dist 23) bate com esses offsets.
- Campos do bot (B): +4 medalha+1 (atributo), +5 **nível**, +7 bônus de compatibilidade, +0x20+k especialidade k=0..7,
  +0x91 = P3+1 = tipo de perna (no check anti-air), +0xB9 penalidade do alvo na ROS, +0xBC flags (ver §6),
  +0xC9 efeito pendente, +0xD0 bônus de ataque, +0xD1 defesa (subtraída), +0xD3/+0xD4 bônus por especialidade,
  +0xD6 flags do atacante (bit0 +poder, bit1 sem "zero-miss", bit2 +sucesso), +0xD8 (s16) penalidade extra se F+0xDF==0x1B,
  +0xA9 índice de especialidade (RollPartValue), +0xA2..+0xA6 pernas (ver acima).

## 1. Fluxo de uma ação de ataque (ordem dos handlers/roteiro)

Cada fase é um passo do roteiro de ação (avança o cursor com `Fight_AdvanceActionScriptCursor`, §5). A ordem abaixo vem dos
endereços e dos `Fight_AdvanceActionScriptCursor` de cada fase; a ligação exata ao opcode do roteiro (AS1/AS_criar) é
**provável**.

1. `Fight_ComputeTotalRosPhase` 0x08033C00 → `Fight_ComputeTotalRos` 0x08038BC0 (§2): `F.E8` = ROS total.
2. `Fight_ApplyPendingEffectStatusFlags` 0x08033C20 (§6): liga bits 0x04/0x08 em `alvo.flags` a partir de efeitos pendentes.
3. Fase sem nome 0x08033CD0 (confirmado pelos bytes: `bl 0x08038E54; bl 0x08034F08`) = `Fight_ResolveDefenderRosPhase` (chama §6 e avança):
   `Fight_ResolveDefenderRosReductions` 0x08038E54.
4. `Fight_SeedPreDamageFromTotalRos` 0x08033D10 (op 0x13 do roteiro, AS_criar): `F.EA = (s16)F.E8 >> 2`.
5. `Fight_AccumulateBaseDamage` 0x08033D30 (§3): `F.EC` = dano base.
6. `Fight_ApplyDamageToPart` 0x08034F68 (§4): aplica à peça alvo.

Família alternativa (handlers `AA_00_RosCalcPart`, HA4): `Fight_AccumRosPart` 0x0803579C → `Fight_SpendRos20ReturnsExhausted` 0x0803725C
(E8 -= 0x20; se zerar, falha com +0xA6=0x1A) → `Fight_ScaleAttackValueByPartScalar` 0x08037280 (`F.EC = (E8>>2 + P.poder) / P.escala`, mínimo 1).
Essa família escreve F.EC, mas `0x08033D30` zera F.EC no começo: **o valor do ScaleAttack não entra no dano do caminho 1**.
Qual golpe usa qual família: **aberto** (ver §8).

## 2. ROS total do ataque: `Fight_ComputeTotalRos` 0x08038BC0 (confirmado)

```
E8 = 0
Fight_AccumRosPart(atk, k)                       # 0x0803579C
    E8 += P.sucesso                              # P+5
    E8 += compat(atk)                            # 0x0803576C
    E8 += rand & 7                               # randbyte 0x080424B8
    E8 += atk.B[0x20 + P.especialidade]          # P+2
    E8 += atk.B[+0xA6] if Speciality_IsRemotenessClass(esp) else atk.B[+0xA5]   # distância ou proximidade da perna
if IsAntiAir(atk,k,alvo):  E8 += P.sucesso*16    # 0x08035880
if IsAntiSea(atk,k,alvo):  E8 += P.sucesso*16    # 0x080358B8
if atk.B[+0xD6] & 4:       E8 += P.sucesso
E8 += F.rec[F.89].+0x28                          # bônus de suporte (Scout; mtbe), registro de 8 B em F+0x28
Fight_AddStrengthenRosBonus(atk)                 # 0x080358F0 (não lido nesta rodada)
if P.tecnica in {8, 9} (Break, Press):           E8 += P.sucesso
if P.tecnica == 52 (TeamAttack) and F.rec[F.8A].+0x2B == 51 (Team Form): E8 += F.rec[F.8A].+0x2C
Fight_ApplyVirusBugRosPenalty(atk)               # 0x08035814 (Bug 0x0D / Virus 0x0E no efeito pendente)
E8 = safe_sub(E8, alvo.B[+0xB9])
```

IsAntiAir (0x08035880): `P_atk.tecnica == 34 ('"')` **e** `alvo.B[+0x91] == 100 ('d')` (tipo de perna "voadora").
IsAntiSea (0x080358B8): `P_atk.tecnica == 35 ('#')` **e** `alvo.B[+0x91] == 106 ('j')`.

## 3. Dano base: `Fight_AccumulateBaseDamage` 0x08033D30 (confirmado; reescrito do decompile)

```
atk = F.87; alvo = F.88; k = F.A7; P = P(atk,k)
F.EC = 0
d = F.EA + P.poder                                          # P+6
d += B(atk).D0
if P.especialidade in {0,1}:   d += B(atk).D3
if P.especialidade in {2,3}:   d += B(atk).D4
if P.especialidade == 1:       d += B(atk).A2 >> 1           # propulsão/2
if P.tecnica in {4,5} (Laser, Beam):             d += P.poder
if B(atk).D6 & 1:                                d += P.poder
if P.tecnica == 52 (TeamAttack) and F.rec[F.8A].+0x2B == 51 (Team Form):  d += F.rec[F.8A].+0x2D
Fight_AddStrengthenDamageBonus(atk)              # 0x08035B9C: para k' in 0..2 (cabeça e braços, NÃO pernas):
                                                 #   se P'.tecnica == 53 (Strengthen) e P'.armadura_atual != 0: d += P'.poder
d = safe_sub(d, alvo.B[+0xD1])                   # defesa do alvo
if F.DE != 0 and F.DF == 0x1B:  d = safe_sub(d, (s16) alvo.B[+0xD8])
F.EC = d
advance cursor
```

Leitura do byte +1 (tecnica) e +2 (especialidade) das peças — ver §7.

## 4. Aplicação: `Fight_ApplyDamageToPart` 0x08034F68 (confirmado em MD.md passo 6)

Com `a = armadura atual da peça alvo` (B(alvo)+0x59+0x18*k'):
- `d < a`: resultado = d; excedente = 0; peça não quebra.
- senão: resultado = a; excedente = d - a; peça quebra (F+0xD1 = 1).
Não zera a peça: o pré-dano é o que decide o corte. Chamadores: `Fight_DispatchOnSubstateAA_A_02_ApplyDamageToHighestAlivePart` 0x0801C660 (provável ataque comum; não confirmado nesta rodada).

## 5. `Fight_AdvanceActionScriptCursor` 0x08034F08 (confirmado, era pendência 5)

```
fi+0xAA = 0                                       # estado de sequência do roteiro
script = ptr_table[0x083C6CA8][F.A8]             # ponteiro do roteiro (u32 por índice)
op  = script[F.BC]                                # byte no cursor (u16 em F+0xBC)
F.BC = F.BC + 1 + LEN[op]                         # LEN = tabela de bytes em 0x083C6EB8 indexada pelo opcode
```
A tabela `0x083C6EB8` tem os comprimentos (opcodes sem argumento = 0; LEN real (corrigido por AS3a/AS3b/TQ2): LEN[0x02]=1, LEN[0x03]=4, LEN[0x13]=0, demais 0; antes dizia 0x13 e 0x03 = 1 byte. Confirmado só para os
bytes lidos). O cursor avança o tamanho do opcode lido, não um passo fixo.

## 6. Acerto, esquiva, crítico e defesa de perna (confirmado; era pendência 6 e 7)

### 6.1 Flags do defensor (`alvo.B[+0xBC]`)
| bit | significado | quem liga |
|---|---|---|
| 0x01 | (pendente) | efeito de status; lido em 0x08033C20 |
| 0x02 | (pendente) | efeito de status; lido em 0x08033C20 |
| 0x04 | bypass de **esquiva** | efeito 0x2E ('.') em alvo.B[+0xC9]; bit 0x01; crítico 0x4C |
| 0x08 | bypass de **defesa** | efeito 0x2F ('/') em alvo.B[+0xC9]; bit 0x02; crítico 0x4C |
| 0x10 | **esquivou** (ROS zerou na esquiva) | `Fight_ApplyEvasionReductionToRos` |
| 0x20 | defesa ativada (redirecionou alvo) | `Fight_ApplyDefenseReductionToRos` |
| 0x40 | crítico | `Fight_TryApplyCriticalRosOverride` |

`Fight_ApplyPendingEffectStatusFlags` 0x08033C20: se `alvo.C9 == 0x2E` → 0x04; `== 0x2F` → 0x08; `in {0x0F,0x10}` → 0x0C;
depois `flags&1 → |=4`, `flags&2 → |=8`. (Efeitos 0x0D/0x0E = Bug/Virus, mtbe.)

### 6.2 Resolução (`Fight_ResolveDefenderRosReductions` 0x08038E54)
```
if not (flags & 4 and flags & 8):
    if evasao() == 0:  defesa()          # defesa só roda se a esquiva não terminou o golpe
else:
    crit_por_acao()                      # só o limiar de crítico
```

### 6.3 Esquiva (`Fight_ApplyEvasionReductionToRos` 0x08038F84)
```
if flags & 4 == 0:
    EE = Fight_AccumDefenderLevelRosBase(alvo)   # 0x08035844: EE = alvo.B[+5] (NÍVEL) + compat(alvo) + rand&7
    EE += alvo.B[+0xA3]                          # evasão da perna
    if flags & 8 == 0: EE = safe_sub(EE, 8)
    if crit_por_acao(): return 1                 # crítico: não esquiva
    E8 = safe_sub(E8, EE); F2 = E8
    if E8 == 0 and P_atk.tecnica not in {6, 7} and atk.B[+0xD6] & 2 == 0:
        alvo.flags |= 0x10                        # ERRO / esquivou
        return 1
return 0
```
Tecnica 6 (Missile) e 7 (Napalm) não erram por ROS zero (exceção confirmada).

### 6.4 Crítico (`Fight_TryApplyCriticalRosOverrideByActionKind` 0x08038E9C + `Fight_TryApplyCriticalRosOverride` 0x08038F34)
```
if IsAntiAir(...) or IsAntiSea(...):  lim = EE                        # bypass por aéreo/marítimo
else: lim = EE * (2 if P.especialidade==3 else 4 if P.especialidade==0 else 8)
crit = lim < E8                                                      # comparação na 0x08038F34 (limiar < ROS)
if crit: EE = 0; alvo.flags |= 0x4C; F2 = E8
return crit
```
Ou seja: especialidade 0 tem o limiar ×4, 3 ×2, demais ×8. (O comentário do mtbe inverte a comparação; o código é `limiar < ROS`.)

### 6.5 Defesa de perna / "Leg Defense" (`Fight_ApplyDefenseReductionToRos` 0x08039068)
```
if flags & 8 == 0:
    EE = Fight_AccumDefenderLevelRosBase(alvo) + alvo.B[+0xA4]     # + defesa da perna
    if flags & 4 == 0: EE = safe_sub(EE, 8)
    if not crit_por_acao() and EE > 31:
        E8 = safe_sub(E8, EE); F2 = E8
        alvo.flags |= 0x20                        # defesa ativada
        if peças_com_HP(alvo) == 1: parte = única viva
        else:                       parte = Fight_PickHighestArmorNonHeadPart1Based(alvo)   # 0x08039158
        Fight_SetSelectedTargetPart(atk, parte)  # 0x08035944: redireciona o golpe para longe da cabeça
```
Leg Defense = o golpe é redirecionado para a peça de maior armadura, exceto cabeça, quando EE > 31. **Não é proteção de aliado**: age
sobre a perna do próprio alvo. Proteção de aliado: não encontrada nesta rodada (**aberto**).

## 7. Byte +1 e +2 das peças: decodificados

### 7.1 Byte +1 (tecnica, 0..59) — gPartStats +1 (kinds 0..2) e P+1
ID → nome (de `pc/cache/parts.lua`, `technique_name`):
0 Sword, 1 Hammer, 2 Rifle, 3 Chain Gun, **4 Laser, 5 Beam**, **6 Missile, 7 Napalm**, 8 Break, 9 Press, … 30 Recovery,
32 Repair, **34 Anti-Air, 35 Anti-Sea**, 36 Scout, … 50 Forcedrain, **51 Team Form, 52 TeamAttack, 53 Strengthen**, 54 CntrAttk,
55 Change, … 59 HealChange.

**Mistura ASCII x valores pequenos: resolvida, não é mistura.** Os "caracteres" '3','4','5' do decompile são os
ids **51, 52, 53** (Team Form, TeamAttack, Strengthen), porque a variável é `char` e o Ghidra mostra o caractere. Os
valores pequenos **4 e 5** são Laser e Beam. O mesmo byte tem os dois espaços de id; não há codificação separada.

Leitores do byte +1 da peça (todos confirmados no decompile):
- `Fight_AccumulateBaseDamage` 0x08033D30: 4/5 → +poder; 52 + ramo 51 → +F.rec+0x2D; 52/53 nas outras partes via `0x08035B9C` (53 → +poder).
- `Fight_ComputeTotalRos` 0x08038BC0: 34 e 35 (anti-air/anti-sea, com legs 100/106); 8/9 → +sucesso; 52 + ramo 51 → +F.rec+0x2C.
- `Fight_ApplyEvasionReductionToRos` 0x08038F84: técnica do atacante ∉ {6,7} (Missile/Napalm) para a regra de erro por ROS zero.
- `Fight_IsAntiAirAgainstAirTarget` 0x08035880 e `Fight_IsAntiSeaAgainstSeaTarget` 0x080358B8.
- `Fight_DrawSelectedActionIcon` 0x08016D6C e `gTechniqueNames` 0x083BE868 (nomes).

Kind 3 (pernas): o byte +1 é o **tipo de perna** 0x64..0x6A = 100..106 (`d`..`j`). 100 = voadora (alvo de anti-air), 106 = marítima (alvo de anti-sea).

### 7.2 Byte +2 (especialidade, 0..7) — gPartStats +2
Histograma em gPartStats: 00:171, 01:39, 02:58, 03:41, 04:49, 05:23, 06:42, 07:57 (480 registros). Não é "modo 0..3": são 8 especialidades
(nomes em parts.lua: 2 = Shoot, 5 = Heal, 7 = Interrupt; demais a conferir).
Usos:
- `Fight_AccumRosPart` 0x0803579C: `ROS += atk.B[0x20 + esp]`; escolhe distância/proximidade pela classe de especialidade.
- Crítico (§6.4): multiplicador do limiar ×4 (esp 0), ×2 (esp 3), ×8 (demais).
- Dano (§3): esp 0/1 → +B[+0xD3]; esp 2/3 → +B[+0xD4]; esp 1 → +B[+0xA2]>>1.

## 8. Especialidades e bases de nível (era pendência 3)

### 8.1 Tabela da inimiga `0x083AD380` (ponteiro literal em 0x080151E8)
- 0x080151E8 contém o ponteiro 0x083AD380. A tabela tem 30 linhas × 8 B (240 B), índice `attr*8 + k`, `attr = medalha+1` (bot+4).
- Primeiras linhas: `04 04 01 01 02 01 02 01` / `01 01 04 04 01 02 01 02` / `01 01 03 03 02 02 02 02` …
- `Fight_InitializeBotFromTemplate` 0x080150FC (inimigos, bots 3..5; medalha = template+8, nível = template+9):
  `B(b).0x20+k = clamp(1..100, T[attr][k] * (nível >> 1))`; se `nível >> 1 == 0`, usa `T[attr][k]` sem multiplicar.

### 8.2 Jogador `Medabot_SpawnFromRecord` 0x08041D70
- Literais: 0x08041E70 contém o **ponteiro 0x03000537** (IWRAM), não é tabela. A "base" são 7 bytes em IWRAM 0x03000537..0x0300053D
  (= 0x03000440+0xF7..0xFD; o ponteiro 0x08041E74 = 0x03000440).
- `B(b).0x20 = gMedals[id].+8 * (1 + nível/10)`, teto 100 (`ref=B0; se B0 + B0*(n/10) < 0x65`), e para k=1..7:
  `B(b).0x20+k = IWRAM[0x03000537 + k-1] * (1 + nível/10)`, teto 100. Sem mínimo 1.
- Diferença com o inimigo: escala do jogador = nível/10; do inimigo = nível/2 com mínimo 1. Origem dos 7 bytes em 0x03000537: **aberto**.

### 8.3 Nível de referência `Level_ComputeReferenceFromSlots` 0x0804206C (confirmado)
```
A[0..29] = roster[i].+5 (nível), roster em IWRAM 0x03000BE0 + 0x40*i   # lê todos os 30, sem checar flag
sort descending por A (0x0804242C, Sort_Records8Descending, registros de 8 B em EWRAM 0x02023000)
return (A[0] - A[3]) / 2 + A[3]     # divisão com sinal (trunca p/ zero); A[3] = *0x02023018 = 4º maior
```
Ou seja: nível de referência = ponto médio entre o maior nível e o 4º maior do roster. (Chave de sort = u16 no offset 0: provável.)

## 9. Pseudocódigo de ponta a ponta (ataque comum, família 1)

```
# fase 1
E8 = ComputeTotalRos(atk,k)                          # §2
# fase 2
flags_alvo |= pendentes(alvo)                        # §6.1
# fase 3: resolução
if not (flags&4 and flags&8):
    if not evasao(): defesa()                         # §6.3, §6.5
else:
    crit_por_acao()                                   # §6.4
if errou (flags & 0x10): (sem dano; opcode que checa 0x10 = aberto)
# fase 4
EA = s16(E8) >> 2                                    # 0x08033D10
# fase 5
EC = EA + P.poder + B(atk).D0 + bônus_especialidade + bônus_tecnica(4/5, 52, 53)   # §3
EC = safe_sub(EC, B(alvo).D1); if F.DF==0x1B: EC = safe_sub(EC, B(alvo).D8)
# fase 6
aplica_a_peca(alvo, peca_escolhida, EC)              # §4
```

## 10. Divergências com a documentação anterior (corrigidas)

1. Byte +2 da peça é **especialidade 0..7**, não "modo 0..3".
2. Byte +1: a mistura ASCII '3','4','5' não existe: são ids de técnica 51/52/53. 4/5 = Laser/Beam (não ASCII).
3. `Fight_AddStrengthenDamageBonus` soma **P+6 (poder)** das peças 0..2 (cabeça e braços) com tecnica 53 e armadura ≠ 0. MD.md dizia "peça+5" e "para cada peça".
4. `Fight_AccumRosBase` (0x08035844) usa o **nível** do alvo (bot+5), não a perna. O nome do mtbe ("defender.legs...") está errado; renomeado no Ghidra para `Fight_AccumDefenderLevelRosBase`.
5. Especialidade do inimigo: `T * (nível>>1)`, mínimo 1; jogador: `B*(1+nível/10)`, sem mínimo.
6. A regra "ROS zero = erro" não vale para tecnica 6 (Missile) e 7 (Napalm).
7. `Fight_RollPartValue` (0x0803769C, "ab + compat + rand + espec[a9]") é a família alternativa; o caminho do dano usa `Fight_ComputeTotalRos` (§2). Ver §1.
8. `F.EC` do `ScaleAttack` é sobrescrito por `0x08033D30` no caminho 1 (MD.md dizia que o cálculo partia dele).
9. `Fight_SeedPreDamageFromTotalRos` = `E8 >> 2`, confirmado (MD.md: "semeado por ... não lido").

## 11. Pendências (abertas)

- Qual opcode do roteiro chama cada fase (§1) e qual checa `F.flags 0x10` para pular o dano (ERRO).
- Família de cada golpe: `ComputeTotalRos` (caminho 1) x `AccumRosPart`+`SpendRos20`+`ScaleAttack` (caminho 2, HA4 `AA_00`).
- Proteção de aliado (Leg Defense de aliado): não encontrada. Chamadores de `Fight_AddStrengthenRosBonus` (0x080358F0) não lidos.
- Origem dos 7 bytes 0x03000537..0x0300053D (base de especialidade do jogador).
- `F.rec[F.8A]` (registro de 8 B em F+0x28 e F+0x2B..0x2D): o "ramo" de equipe (Team Form 51) e o bônus de suporte (F+0x28).
- Nomes das especialidades 0,1,3,4,6 (só 2 Shoot, 5 Heal, 7 Interrupt confirmados em parts.lua).
- Literais 0x0800B590 e 0x08017F50 de gMedals (ver MD.md §4).
- Função sem nome 0x08033CD0 (ver §1): Ghidra não tem função nesse endereço; bytes confirmados.
- Sort do Level_ComputeReferenceFromSlots (chave, ordem): não lido em detalhe.
