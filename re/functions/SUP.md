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
| 30 | Recovery | passos AQ_00/AQ_02 (0x08026EF8/0x08026F34), P_01 (0x080208E8) | peça viva mais danificada do ATOR (§4, AQ_00) | `valor = (E8>>2 + poder) / escala`, mín. 1; `cura = min(valor, maxhp-hp)`; excedente → peça com chain (+7) quebrada | `part.hp` (+0x59 do bot) | P (ver §3) |
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

## 4. Rodada SUP2 (Ghidra no ar, 2026-10-17)

Confiança: C = visto no decompile; P = provável (deduzido do código/roteiro); A = aberto. Endereços de entrada em `SUP_simbolos.csv`.
Porte atualizado: `pc/game/src/fight/support.lua` (testes OK 49). `status.lua` e `battle.lua` não foram tocados.

**(1) Scout 36 e Conceal 37 — incremento = EC (C).** Z_0E `0x080233E0` = `Util_AddByteClampToMax(lado[targetSlotIndex], fi+0xEC, 0xA0)`.
AA_0A `0x080236E0` = `Util_AddByteClampToMax(bot[activeBot]+0xB9, fi+0xEC, 0x78)`. Util `0x08035740`: se `max < atual+v` grava `max`, senão `atual+v`.
Logo o incremento é o valor de **fi+0xEC** no momento do passo, não uma fórmula nova. A: quem grava EC antes do passo 0x30/0x31
(os roteiros Scout `0x083C6AA2` e Conceal `0x083C6AA4` começam nesses passos). O porte usa `fight.dmg` (EC) se > 0, senão o chute `partValue` (P).
A TR6 (37, 26–27, 25–38, 18) é compatível com EC variando por ação.

**(2) Recovery 30 — alvo é o ATOR (C).** AQ_00 `0x08026EF8`: `Fight_PickMostDamagedRepairablePart1Based(activeBot)` e `activeBot` é o ator
(P_0B usa `activeBot` como dono da peça usada). AQ_02 `0x08026F34`: `SplitCountByCapacity(activeBot, peça, EC)`: cura `min(EC, máx-atual)`,
excedente em fi+0xCA. Quantidade `EC = (fi+0xE8>>2 + poder) / escala` da peça usada (C, `0x08037280`), mínimo 1.
Quirk do excedente (C, AQ_0D `0x08026F6C`): a peça é escolhida pela mais danificada do **alvo**, e o excedente entra na peça **desse índice no ator**.
Só sob AQ (a família AQ está em HN1; não confirmei qual roteiro chama AQ_00 — o roteiro 30 `0x083C6A76` chama P(26), X(2E), Q(27) e O(25)). P.
O porte antigo curava o aliado; corrigido.

**(3) Repair 32 — ordem de escolha e quantidade (C na escolha, P no passo).** Setup `0x08032478` (RequireBrokenPart) sorteia com
`Fight_PickRandomBrokenOrMissingPart1Based` `0x0803325C` (C): 2 `randbyte`; início = b1&3; sentido = b2&1 (0 decrementa, 1 incrementa);
até 4 tentativas; primeira peça com **hp == 0** (cabeça, braços e pernas, índices 0..3). Sem quebrada → nada.
Quantidade: `EC` da peça usada; cura `min(EC, máx-atual)` (C, SplitCount). Excedente só com flag chain (+7) da peça **do ator** (R_0C `0x08020AD0`, C):
vai para outra quebrada do alvo (`SplitCountByMax`). A: o passo que aplica a cura no roteiro 32 `0x083C6A8B` (ops Q 27, R 28, Y 2F, Z, AA, AB).
Atenção: o roteiro 32 contém `2F` (Y); Y_00 só age com cabeça alvo zerada (C), então Repair com cabeça quebrada pode disparar o Reactivate
(A, a confirmar no emulador). H_3B `0x0801F1C8` (C) escolhe a mais danificada **viva** do alvo (outra família, não é o sorteio de Repair).

**(4) Reactivate 33 (C).** Y_00 `0x08022CD0`: chama `Fight_ScaleAttackValueByPartScalar(ator, peça)` (EC) e só segue se `parts[0].currentArmor == 0`
do alvo; senão `messageCode = 9`. Y_0E `0x08022F50`: `SplitCount(alvo, 0, EC)`, cabeça = `combatValue` = `min(EC, máx)`, `presence = 3`, `botStatus = 1`.
O roteiro 33 `0x083C6A9C` começa com `2F` (Y). "Reativa" é nome deduzido (P).

**(5) Break Form 21 e Stability 22.**
- Roteiros (C, bytes da ROM): 21 = `2A(T) 06 08 2B(U) 2C(V) 01 01 01 2D(W)x3 06 08 25(O) 80 81 81 81 80`;
  22 = mesmo rabo **sem** o `2A`. O rabo (U/V/W/O) é compartilhado com outras técnicas (é efeito de carga/status ±, ver A6).
- Stability como ação (C no setup): `0x08031F2C` escolhe **aliados do próprio lado** com `pendingStatusKind != 0`, sem peça de técnica 16,
  com especialidade da peça usada (1ª passada; 2ª sem especialidade). O porte limpa o status pendente desse alvo (P: o efeito do rabo não foi lido).
- Break Form como ação (A): a limpeza de armadilha do lado oposto (REF_MEDAPEDIA §45) **não foi achada** no código. O passo T_00 ramifica pelo
  dano do alvo (HA5), não pelas armadilhas. O porte mantém a limpeza como P.
- **Passivo / imunidade (A, não confirmado).** Não achei leitor do id 21 ou 22 nas peças do defensor (busca por valor não é possível no Ghidra
  MCP; a checagem de status em `0x08031F2C` é só do alvo ativo). REF_FAQ (guia) diz "Stability equipada protege de status": sem prova no código.
  Não implementar imunidade; regra exata a confirmar com emulador (equipar peça 22 e aplicar Stun/Bug).

**(6) Cargas 38 / 39 / 40 (roteiros, C).** Boost 38 `0x083C6AA6` chama AB (0x32), que grava `bot+0xC4=0x26` com `TR5` (confirmado no SUP §1).
Rapid 39 `0x083C6AAA` chama AC (0x33) = `messageCode 0x2B`, sem mudança de medidor visível (C). Accel 40 `0x083C6AAE` chama AD..AI (0x34..0x39):
o efeito de Speed Up 0x2C não bate com esses ops (A). Medidor +0xBB/+0xC7: ver CHG.md (de outro agente), não reconferido aqui.

**Correções ao §2/§3 acima:** a "origem do incremento" (§2) fica resolvida como EC (C); "Recovery: alvo" (§3) fica resolvido como ator (C);
o Repair usa sorteio de quebrada, não a primeira (C).
