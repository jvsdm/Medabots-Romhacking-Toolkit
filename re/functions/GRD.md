# GRD: guardas (Defence 27, Full Block 28, Half Block 29), armadilhas (10/11/12), CntrAttk (54), passivos (24/25/26), Strengthen (53)

Leitura estática (docs `re/functions/*.md` + mtbe.csv). **Ghidra MCP estava fechado nesta rodada** (porta 8080 recusou
conexão), então nenhuma função nova foi decompilada: 0x080358F0, 0x08030608 (corpo), 0x08035B9C (corpo) e o
despachante das guardas **não foram lidos aqui**. Confiança: "confirmado" = decompile/bytes citados em doc antiga;
"provável" = padrão de uso ou descrição de jogo; "aberto" = não fechado.

## 1. Mapa por técnica

| Id | Técnica | O que o código sabe | Onde fica o estado | Quando dispara | Confiança |
|---|---|---|---|---|---|
| 10 | Grap Trap | armadilha com slot; família `ActionFamily_TrapFamily_Handler` 0x08030608 (AF_10 oscila o slot 60 quadros, AD.md) | `fi+0x2B` = `team_1_trap` (chute), `fi+0x2C` = `team_1_trap_value` (chute) | ataque de Strike/Berserk (REF_MEDAPEDIA, "30") | armadilha: provável; gatilho: provável; efeito: aberto |
| 11 | Shot Trap | igual à 10 | igual | ataque de Shoot/Aim (REF "31") | provável / aberto |
| 12 | Attack Clr | igual à 10 | igual | qualquer ataque (REF "32") | provável / aberto |
| 27 | Defence | nada lido | aberto (sugestão: `fight.sides[s].guard`, só no porte) | aberto: redirige golpe do aliado para o guarda | **aberto** |
| 28 | Full Block | nada lido | aberto | aberto: anula todo o golpe | **aberto** |
| 29 | Half Block | nada lido | aberto | aberto: anula golpe fraco (limiar não lido) | **aberto** |
| 54 | CntrAttk | nada lido (REF "35" = protege e devolve o dano acima de um limiar) | aberto | aberto | **aberto** |
| 24 | CancelOptic | corpo do passivo não lido | aberto | descrição de jogo: anula Laser/Beam (4,5) | chute (sem doc) |
| 25 | CancelBomb | idem | aberto | descrição de jogo: Missile/Napalm (6,7) | chute (sem doc) |
| 26 | CancelGrav | `ActionFamily_CancelGravHandler` 0x0801D6A4 = **animação** da fase (gfx 0x1B/0x1C/0x1D, CF.md), não o dano | aberto | descrição de jogo: Break/Press (8,9) | chute (sem doc) |
| 53 | Strengthen | dano: `Fight_AddStrengthenDamageBonus` 0x08035B9C (DM §3): soma `poder` (P+6) das peças 0..2 com técnica 53 e armadura ≠ 0 | ROS: `Fight_AddStrengthenRosBonus` 0x080358F0 **não lido** | chamado no cálculo de ROS (DM §2) e de dano (DM §3) | dano: provável (DM §3); ROS: aberto |
| 51/52 | Team Form / TeamAttack | ramo em `fi+0x8A` e registro `fi+0x2A + 8*k` (DM §2/§3) | `fi+0x2A` registro, `+0x2B` = 51 | ataque 52 com registro do time = 51 | ver DM §2/§3 |

## 2. Estado do lado (o que existe no RAM)

- `F+0x2A` (0x0300182A) = `gFight_TargetDamageRecord`, registro de 8 B × N indexado por `F+0x89` (mtbe, provável).
  Conflito: DM §2 diz "registro de 8 B em F+0x28" (bônus Scout `F.rec[F.89].+0x28`), mas FI.md (Conflito 2) resolveu
  que `F+0x28` **não** é campo separado: é o mesmo array de `F+0x2A + 8*k`. Usar `F+0x2A`; o "+0x28" do DM é
  deslocamento relativo ao registro, **provável, não fechado**.
- Dentro do registro (rótulos conflitantes, não resolvidos):
  - `+0x2B` (mtbe `team_1_trap`, chute): DM §2 lê como técnica do time (51 = Team Form). Pode ser técnica de armadilha/guarda
    do lado; **o mesmo byte não tem uso confirmado como armadilha**.
  - `+0x2C`: FI.md = contador de drenagem (HA1, provável); mtbe `team_1_trap_value`.
  - `+0x2D`: FI.md = dano do registro (HA1, provável).
- Flags do defensor `bot+0xBC` (SS2 §1, DM §6.1): 0x04 bypass de esquiva (efeito 0x2E), 0x08 bypass de defesa (0x2F),
  0x10 esquivou, 0x20 defesa ativada, 0x40 crítico. **Confirmado** (`Fight_ApplyPendingEffectStatusFlags` 0x08033C20).
  Guardas 27–29 não usam esses bits (sem evidência).
- Carga/Boost (38–40): `Fight_DispatchOnSubstateAA_W_07_ApplyActorChargeBuff` 0x08022748 faz `bot+0xBD |= 0x08` (confirmado
  no decompile antigo); ligação exata 38/39/40 = provável.

## 3. Fluxo de ataque (onde cada coisa entra; DM §1)

1. `Fight_ComputeTotalRosPhase` (ROS total) → aqui entra Strengthen-ROS (0x080358F0, aberto) e Scout (+0x28/F+0x2A).
2. `Fight_ApplyPendingEffectStatusFlags` 0x08033C20 (bits 0x04/0x08 pelos efeitos 2E/2F).
3. `Fight_ResolveDefenderRosPhase` 0x08033CD0 → `Fight_ResolveDefenderRosReductions` 0x08038E54 → **Leg Defense**
   `Fight_ApplyDefenseReductionToRos` 0x08039068 (DM §6.5). Esta é a **única redireção de alvo confirmada**
   (`Fight_SetSelectedTargetPart` 0x08035944, peça de maior armadura exceto cabeça, quando EE > 31). **Não é proteção de aliado.**
4. `Fight_SeedPreDamageFromTotalRos` 0x08033D10 → `Fight_AccumulateBaseDamage` 0x08033D30 (aqui entra o Strengthen de
   dano, 0x08035B9C) → `Fight_ApplyDamageToPart` 0x08034F68.
5. Gancho de armadilha: `ActionFamily_TrapFamily_Handler` 0x08030608 roda no passo AS 0x36/0x01 (AS3c, AB.md). Ponto onde a
   armadilha disparia e onde a Full/Half Block deveria cortar o dano: **aberto** (corpo não lido).

Proteção de aliado (Defence 27): DM §6.5 já registra "não encontrada"; continua **aberta**.

## 4. Fórmulas com número (o que o porte usa)

- Strengthen dano (confirmado no DM §3, Ghidra antigo):
  `d += sum(P'.poder for k' in 0..2 if P'.tecnica == 53 and P'.armadura_atual != 0)` (pernas não entram).
- Strengthen ROS: **aberto** (0x080358F0 não lido).
- Gatilho de armadilha por especialidade do atacante (REF_MEDAPEDIA, descrição do jogo, provável):
  Grap Trap (10): especialidade 0 ou 1; Shot Trap (11): 2 ou 3; Attack Clr (12): qualquer.
- Passivos 24–26 (descrição da tarefa, **sem doc de ROM**): chute. Ver guard.lua.

## 5. Próximos passos (para o Opus / próxima rodada com Ghidra aberto)

1. Decompile 0x08030608 (TrapFamily) e o despachante de 27/28/29 (procurar literal/tabela com id 27..29 em `gTechniqueNames`
   ou no despachante de guarda por família). Ver se `+0x2B..+0x2D` do registro são lidos aqui.
2. Decompile 0x080358F0 (ROS de Strengthen) e 0x08035B9C (confirmar a fórmula de dano).
3. Achar o corpo dos passivos 24–26 (ROM: procurar comparações com 4..9 em função que recebe o id do golpe).
4. Achar o limiar de Half Block e de CntrAttk.

Arquivos: `re/functions/GRD_simbolos.csv`. Porte: `pc/game/src/fight/guard.lua`, testes `pc/game/tests/test_guard.lua`.
