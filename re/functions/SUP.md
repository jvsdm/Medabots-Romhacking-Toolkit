# SUP: ações de Support e Heal da Robattle (Metabee EU)

Escopo: técnicas 36 Scout, 37 Conceal, 30 Recovery, 31 AutoRecover, 32 Repair, 33 Reactivate, 21 Break Form, 22 Stability.
Ghidra MCP estava FORA DO AR (connection refused na porta 8080) nesta rodada: nada novo foi decompilado. Tudo abaixo
vem de notas existentes (TR2, TR6, SS, HN1, HA4, R1, R2, FH, DM, REF_MEDAPEDIA). Confiança: C confirmado, P provável, A aberto.
Porte: `pc/game/src/fight/support.lua` (testes `pc/game/tests/test_support.lua`, OK 40).

## 1. Tabela por técnica

| Id | Técnica | Função da ROM | Alvo | Fórmula | Grava em | Confiança |
|---|---|---|---|---|---|---|
| 36 | Scout | `Fight_DispatchOnSubstateAA_Z_0E_AddScoutBonus` 0x080233E0 | lado do atacante | soma `incremento` ao byte do lado; teto 0xA0 (160) | `fight+0x28+8*lado` (0x03001828 + 8·lado; lado 0 = 0x03001828) | C (TR6, emulador) |
| 37 | Conceal | `Fight_DispatchOnSubstateAA_AA_0A_AddConceal` 0x080236E0 | o próprio usuário | soma `incremento` ao `bot+0xB9`; teto 0x78 (120) | `bot+0xB9` (= `ros_penalty` no contrato) | C (TR6); escritor e teto medidos |
| 30 | Recovery | passos AQ_00/AQ_02 (0x08026EF8/0x08026F34), P_01 (0x080208E8) | peça viva mais danificada do alvo | `valor = (E8>>2 + poder) / escala`, mín. 1; `cura = min(valor, maxhp-hp)`; excedente → peça com chain (+7) quebrada | `part.hp` (+0x59 do bot) | P (ver §3) |
| 31 | AutoRecover | `Fight_DispatchOnSubstateAA_X_0E_InflictStatus1F` 0x08022C28 (commit) + `Fight_TickSecondaryRepairEffectAndEmitResult` 0x08018D0C (tick) | alvo | commit: contador = clamp99(E8) em `+0xC6`, valor = clamp99(EC) em `+0xC7`, `+0xC4 = 0x1F`. Tick a cada 4 do contador: cura `+0xC7` na 1ª peça com hp<maxhp; zera ao fim | `bot+0xC4..0xC8` (`bot.repair`) | C (SS, HN1, R1) |
| 32 | Repair | passos P/R/H (0x08020928 / 0x08020AD0 / 0x0801F1C8) | peça quebrada (hp = 0) do alvo | `cura = min(valor, maxhp)` na peça; excedente → próxima quebrada se chain | `part.hp` | P (ordem de escolha A) |
| 33 | Reactivate | `Fight_DispatchOnSubstateAA_Y_00` 0x08022CD0 + `Y_0E_ReviveTarget` 0x08022F50 | bot com cabeça zerada | só age se `parts[0].hp == 0` (senão `+0xA6 = 9`); cabeça recebe `clamp(EC)`; `bot+0x40 = 3` | `part[0].hp`, `bot+0x40` (`bot.state`) | C no código; "reativa" = P |
| 21 | Break Form | (sem passo lido) | lado oposto | limpa armadilhas (`trap_tech/trap_value/trap_power`) | `fight.sides[1-s]` | P (REF_MEDAPEDIA §45, não lido no decompile) |
| 22 | Stability | (sem passo lido) | bots do próprio lado | limpa status pendente (`bot.pending`) | `bot+0xC9..0xCC` | P (REF_MEDAPEDIA §45) |

Valor `EC` de Recovery/Repair/Reactivate: `Fight_ScaleAttackValueByPartScalar` 0x08037280 (DM §3, FH): `EC = (E8>>2 + poder)/escala`, mín. 1 (C na fórmula; "escala" = peça +0x54 / registro +0xC).
Limitação `Fight_ClampDisplayValue1To99` 0x08036E54 (C): `>=100 → 99`, `0 → 1`.
Limitação de reparo por peça `Fight_AddPartArmorClampOverflow` 0x08018F0C (R1, C): soma até o máximo (+0x4C) e devolve o excedente.

## 2. Incremento de Scout e Conceal (aberto)

TR6 mediu no emulador: Conceal 0→37→64→102→120 (teto), Scout 0→37→63→89→114→151→160 (teto). Incrementos 37, 26–27, 25–38, 18.
Os primeiros 37 (0x25) batem nos dois. O porte usa `valor = (E8>>2 + poder)/escala` como chute (P), com `opts.amount` para forçar.
**ABERTO**: origem exata (campo da peça de quem usa; TR6 deixou em aberto). Scout e Conceal ficam zerados só na derrota (Conceal:
`Fight_ResetBotPartState` 0x08036FA0, C); Scout não zerado no que foi lido.

## 3. Pontos em aberto e divergências

- **Scout lido onde?** TR2 diz: `Fight_ComputeTotalRos` 0x08038BC0 soma `fi.rec[fi+0x89].+0x28` (tabela indexada pela fase). TR6 diz
  `fi+0x28+8·lado`. DM §2 diz o mesmo que TR2. O porte grava em `fight.sides[s].scout`, mas `calc.lua` soma `side.support` (DM §2) no ROS:
  **Opus: decidir se `calc.lua` deve somar `sides[s].scout`** (provável: é o mesmo valor).
- **Recovery: alvo.** AQ_00 (HN1, confirmado) usa `Fight_PickMostDamagedRepairablePart1Based(ator)` = o ator, não o alvo. O porte segue
  o contrato (peça viva do aliado). Se o jogo cura a peça do próprio ator, a seleção muda. ABERTO.
- **Desempate** de peças iguais no PickMostDamaged (0x08033184): ordenação decrescente via `Sort_Records8Descending`; empate e slot 0 não testados.
- **Divergência de nome:** `0x08036CCC` = `Fight_ClampRepairToMissingArmor` (mtbe, HA4, HN1) e `FightPreview_SplitCountByCapacity` (R2, prévia
  com +0xC8/+0xCC). Os usos de reparo (AQ_02, P_01, Y_0E) chamam com (bot, peça, EC). Usei a semântica de reparo, P.
- **Reactivate vs Repair:** o passo Y_0E (bot de cabeça zerada, +0x40=3) foi atribuído a 33 Reactivate (P). Repair (32) fica nos passos P/R/H.
- **Ícones e textos** de Support/Heal (`Fight_DrawSelectedActionStatusText` 0x08016C0C, `Fight_IsPartTechniqueSupportUtilityClass` 0x08016E5C): não tratados.
- **Cargas 38 Boost / 39 Rapid / 40 Accel:** não documentados aqui (ver SS §1: Boost_Chrg é efeito secundário 0x26 com indicador,
  `Fight_CommitBoostChargeSecondaryEffectToMarkedTargets`). Tempo de carga é de outro agente.
- **Medaforce 0x22/0x24** usam os mesmos despachantes (TR6): não cobertos.
