# FH: helpers de batalha chamados por muitos handlers (documentação função por função)

Tarefa GH-FH (2026-10-13). Bloco de combate: `fight_information` 0x03001800 (`+0xA6` código de passo/mensagem,
`+0xAA` passo, `+0xB5` handler concluído, `+0x87` bot que age, `+0x88` alvo, `+0xA7` peça usada, `+0xE8` valor de
ROS (u16), `+0xEC` valor de ataque/ação (s16)). Bloco do bot = 0x03000590 + bot*0x100. Peças: base = bot+0x48 +
0x18*k (k = 0..3), layout de `types/medabots_simon_types.tsv` (`BattlePartState`): +0x1 action_or_leg_type, +0x6
power_or_evasion, +0xC attack_damage_scalar, +0x11 current_armor. Offsets do Ghidra são estes (sem o +0x40 da pesquisa).

Confiança: "confirmado" = lido no decompile + literal pool; "provável" = padrão de uso; "chute" = só nome.

---

## 0x0803725C Fight_SpendRos20ReturnsExhausted
- Assinatura: `int f(void)`
- O que faz: chama `Fight_SubClampZero(&fi+0xE8, 0x20)`: subtrai 0x20 de `fi+0xE8` sem ir abaixo de 0. Retorna 1
  quando o valor, depois da subtração, é 0 (ou seja, `fi+0xE8` era <= 0x20 antes). Retorna 0 quando sobra ROS.
  O nome antigo (`FightPreview_ClearQueueFlag` em R2, `Fight_CheckRosMeetsMinimumThreshold` no mtbe) não descreve o
  efeito colateral; a leitura do retorno nos chamadores (ver abaixo) mostra que 1 = falha.
- Chama: 0x08035754 (`Fight_SubClampZero`, confirmado). Chamada por: 22 despachantes: O_00, Q_00, S_00, T_00, U_00,
  V_00, W_00, X_00, Y_00, Z_00, AA_00, AB_00, AC_00, AD_00, AE_00, AF, AG, AH, AI, AJ, AM, AN (exemplos:
  `Fight_DispatchOnSubstateAA_U_00_CheckRosAndTargetStatus` 0x08021E46, `Fight_DispatchOnSubstateAA_V_00_CheckRos`
  0x08022196, `Fight_DispatchOnSubstateAA_AA_00_RosCalcPart` 0x08023494).
- Memória: lê/escreve `fi+0xE8` (literal 0x030018E8, confirmado em 0x08037274).
- Efeito no chamador (confirmado nos HA4/HA6/HN1, lido): retorno != 0 = falha: `+0xA6 = 0x1A` (ROS insuficiente) e
  avança; retorno 0 = segue (ex.: `Fight_IncrementActionUsageCounter`, depois `+0xAA = 4`).
- Para o Lua: portar junto com `Fight_ScaleAttackValueByPartScalar` e `Fight_RollPartValue` (o ROS é o valor de
  `fi+0xE8`). Não é "sorteio".
- Renomeado no Ghidra: sim (era `FUN_0803725c`; nome novo `Fight_SpendRos20ReturnsExhausted`).
- Confiança: confirmado (corpo e literal); o significado de "ROS" é provável.

## 0x08037280 Fight_ScaleAttackValueByPartScalar
- Assinatura: `void f(int bot, int part)`
- O que faz: `fi+0xEC = (s16)(fi+0xE8) >> 2` e depois `+= byte(part+0x4E)` (power_or_evasion da peça k do bot);
  divide o resultado por `byte(part+0x54)` (attack_damage_scalar) com `aux_div_s32` (0x0807CD2C, divisão com sinal;
  divisor 0 dá 0). Se o resultado for 0, grava 1. Esse é o valor de ataque da ação, lido depois pelo cálculo de dano.
  O primeiro `*psVar3 = sVar1` é escrita morta (sobrescrito).
- Chama: 0x0807CD2C (`aux_div_s32`, confirmado). Chamada por: 19 despachantes: O_00, Q_00, S_00, W_00, X_00, Y_00,
  Z_00, AA_00, AB_00, AC_00, AD_00, AE_00, AF, AG, AH, AI, AJ, AM, AN (ex.: `Fight_DispatchOnSubstateAA_AA_00_RosCalcPart`
  0x08023494).
- Memória: lê `fi+0xE8` (u16), `fi+0xEC` (escreve), bloco do bot `0x03000590 + bot*0x100 + 0x48 + 0x18*part + 0x6 e +0xC`.
  Ponteiros: literais 0x080372CC = 0x03001800, 0x080372D0 = 0x03000590.
- Para o Lua: portar (é só aritmética inteira). Depende de `fi+0xE8` vindo de `Fight_RollPartValue`/`AccumRosPart`.
- Retorno: nenhum; o valor vai em `fi+0xEC`.
- Renomeado no Ghidra: sim (era `FUN_08037280`; mantido o nome do mtbe).
- Confiança: confirmado (corpo e literal). O significado de "ataque" é provável.
- Divergência com R2.md: R2 e MERGE_RELATORIO chamam esta função de `Fight_ComputePartValue` (chute). O corpo confirma
  a divisão por `attack_damage_scalar`, então o nome do mtbe (`Fight_ScaleAttackValueByPartScalar`) é o que vale.

---

## 0x0803769C Fight_RollPartValue
- Assinatura: `void f(int bot)`
- O que faz: é a primeira metade do cálculo de valor da ação do bot ativo. Grava em `fi+0xE8` (u16) a soma de:
  `byte(bot+0xAB)` + `Fight_GetMedalPartCompatibilityBonus()` (0x0803576C, bônus de medalha/peça) + `(randbyte() & 7)`
  (0x080424B8) + `byte(0x03000590 + bot*0x100 + 0x20 + byte(bot+0xA9))` (nível de especialidade indexado por
  `bot+0xA9`, entre os bytes 0x20..0x27 do bloco do bot; ver `BattleBotStateRecord` no TSV).
- Chama: 0x0803576C, 0x080424B8. Chamada por: `FUN_08034708` (0x08034708, passo que chama esta, depois
  `FUN_080376F4`, depois `Fight_AdvanceActionScriptCursor`) e o despachante `Fight_DispatchOnSubstateAA_AO_01_RollPartValueForBot`
  (0x080268AC). Não tem outros chamadores.
- Memória: lê bloco do bot (`+0xA9`, `+0xAB`, tabela `+0x20`); escreve `fi+0xE8`. Literais: 0x080376EC = 0x03001800,
  0x080376F0 = 0x03000590.
- Para o Lua: portar a soma. O sorteio (`randbyte() & 7`) precisa do estado RNG (0x03000440), que já é documentado no
  README de pesquisa. O significado de `bot+0xA9` como "índice de especialidade" é provável.
- Retorno: nenhum.
- Renomeado no Ghidra: já tinha nome (`Fight_RollPartValue`), mantido.
- Confiança: confirmado (corpo e literais); "especialidade" provável.

## 0x080376F4 Fight_ComputeEffectValueFromRos (era FUN_080376F4)
- Assinatura: `void f(int bot)`
- O que faz: `fi+0xEC = (s16)(fi+0xE8) >> 3` seguido de `fi+0xEC = that + byte(0x03000590 + bot*0x100 + 0xAC)`;
  se o resultado for 0, grava 1. A primeira escrita (`>>3` sem o byte) é sobrescrita: só vale a soma final.
- Chama: nenhuma. Chamada por: `FUN_08034708` (0x08034716) e `Fight_DispatchOnSubstateAA_AO_01_RollPartValueForBot`
  (0x080268BC). Ou seja: 2 chamadores, ambos logo depois de `Fight_RollPartValue`.
- Memória: lê `fi+0xE8`, bloco do bot +0xAC (byte reservado no TSV: provável bônus do bot); escreve `fi+0xEC`.
  Literais: 0x08037720 = 0x03001800, 0x08037724 = 0x03000590.
- Para o Lua: portar junto com `Fight_RollPartValue`.
- Retorno: nenhum.
- Renomeado no Ghidra: sim (era `FUN_080376F4`; nome novo `Fight_ComputeEffectValueFromRos`).
- Confiança: confirmado (corpo e literais). Descrição de `bot+0xAC` é provável.
- Divergência: R2.md (seção FUN_080376f4) escreve a fórmula com dois termos `(ctx+0xe8 >> 3) + (ctx+0xe8 << 16 >> 19)`;
  o primeiro termo é escrita morta, então o resultado real é só o segundo termo + byte(+0xAC).

---

## 0x080370AC Fight_TickSlotScriptsUntilDone (era FUN_080370AC; mtbe: FightPreview_TickSpriteTimersCount)
- Assinatura: `int f(int slot_base, uint count)` (retorna 1 se todos os slots terminaram).
- O que faz: percorre `count` slots de `gObjectSlots` (0x40 B cada) a partir de `slot_base`. Para cada slot ativo
  (u16 +0x00 != 0): se o contador +0x2C é 0, liga o bit 0x8000 em +0x00 (marca "rodando"), chama
  `Anim_RunSlotScript(*0x083B6190, slot)` (0x08040BD8). Se +0x22 == 0xFF, limpa o slot com `Tile_ClearSlots(slot, 1)`
  (0x08040CD0); senão marca "não terminou". Se o contador +0x2C não é 0, decrementa e marca "não terminou".
- Chama: 0x08040BD8 (`Anim_RunSlotScript`), 0x08040CD0 (`Tile_ClearSlots`). Chamada por 5 handlers: A_1B_WaitSlotCheckAdvance
  (0x0801CDBA), K_03_WaitSprite21Ready (0x0801FECE), M_01_WaitSpawnThenAdvance (0x0802060E), AE_WaitSlotAnim
  (0x08024876), AR_WaitPartBreakEffects (0x080277B2).
- Memória: lê/escreve `gObjectSlots` 0x03002C30 (+0x00, +0x22, +0x2C); literais 0x080370EC = 0x083B6190 (ponteiro de
  roteiro de animação), 0x080370F0 = 0x03002C30.
- Para o Lua: o padrão "espera todos os slots acabarem" (os handlers de animação o usam como porta). Portar junto com
  `Anim_RunSlotScript`.
- Retorno: 1 = todos terminaram (o handler avança +0xAA); 0 = ainda rodando.
- Renomeado no Ghidra: sim (era `FUN_080370ac`).
- Confiança: confirmado (corpo e literais). A ligação do ponteiro 0x083B6190 com o contexto de animação é provável.
- Divergência com R2.md: a seção antiga diz que o desligamento usa `FUN_08040cd0`, mas esse endereço é `Tile_ClearSlots`
  (ver mtbe, que o chama de `ClearObjectAttributeEntries`; o nome do mtbe para 0x08040CD0 está errado).

## 0x08036AC4 Fight_AdvanceSlotXByWaveTable (era FUN_08036ac4; mtbe: FightPreview_AdvanceSlotX)
- Assinatura: `int f(int slot, int flag)` (retorna 1 se o slot não está ativo ou já acabou, 0 se ainda andando).
- O que faz: se o slot de `gObjectSlots` está ativo (bit 0x8000 em +0x00) e o passo +0x23 não é 0x10: soma
  `tabela[passo]` ao halfword +0x04 (com máscara 0xFF, então é aritmética de 8 bits), incrementa o passo +0x23 e, se
  `flag != 0`, grava em +0x09 o valor `0x40 + 4*(passo+1)` (quadro/tile). A tabela tem 16 bytes em ROM 0x083AF620:
  `00 FF FE FD 00 FF 00 FF 01 00 01 00 03 02 01 00` (deltas de 8 bits: 0, -1, -2, -3, 0, -1, 0, -1, +1, 0, +1, 0, +3, +2, +1, 0).
  Ponteiro da tabela: literal 0x08036B18 = 0x083AF620.
- Chama: nenhuma. Chamada por 13 sites em ~10 funções: `Fight_DispatchOnSubstateAA_T_10_RunSlotsThenClear` (4 sites,
  0x08021CCC..0x08021CE4), `_AE_StepSlotAnim` (0x08024312), `_AF_WaitSlot22` (0x08024BD2), `_AV_07_WaitFlaggedSlotsArrived`
  (0x08028B6E), `_BH_DropOpponentSprites` (0x0802CFF6), `_BK_09_WaitIconBounceDone` (0x0802DA26), `Fight_SceneStep61_WaitBannerSlide`
  (0x0803143E), e mais 2 funções sem nome (`FUN_08036b1c`, `FUN_08036b50`) e 1 site sem função (0x08010E62).
- Memória: lê/escreve `gObjectSlots` 0x03002C30 (+0x00 bit 0x8000, +0x04 X, +0x09 quadro, +0x23 passo).
- Para o Lua: o movimento de "onda" de X e a troca de quadro por passo são simples; portar com a tabela acima.
- Retorno: 0 = ainda andando, 1 = parado/acabou.
- Renomeado no Ghidra: sim (era `FUN_08036ac4`).
- Confiança: confirmado (corpo e tabela da ROM). `+0x04` como X e `+0x09` como quadro: provável (tamanho de 8 bits).

## 0x08036E54 Fight_ClampDisplayValue1To99 (era FUN_08036e54; mtbe: mesmo nome)
- Assinatura: `uint f(uint v)`
- O que faz: limita `v` ao intervalo 1..99: se `v >= 100` devolve 99; se `v == 0` devolve 1; senão devolve `v`.
- Chama: nenhuma. Chamada por 28 sites em ~11 funções: `FUN_08036e64` (6 sites), `_X_0E_InflictStatus1F` (2),
  `_AB_CopyDisplayValues` (2), `_AD_WriteBotDisplayRows` (4), `_AF_SetSlotParams` (2), `_AG_SetBotParamsFromE8EC` (2),
  `_AI_SetActionResultCodes` (2), `_BE_ApplyRandomStatus` (4), `Fight_CommitBoostChargeSecondaryEffectToMarkedTargets` (2),
  `_BK_0E_SetPendingStatus2B` (2). Ex.: `Fight_DispatchOnSubstateAA_AD_WriteBotDisplayRows` 0x08023FF4.
- Memória: nenhuma (só valor). Para o Lua: `math.max(1, math.min(99, v))` com a regra do zero (0 -> 1).
- Retorno: valor limitado.
- Renomeado no Ghidra: sim (era `FUN_08036e54`).
- Confiança: confirmado (corpo).

## 0x08036B84 Fight_CopyPartArmorToDisplayValues (era FUN_08036b84; mtbe: FightPreview_SetPartSpriteId)
- Assinatura: `void f(int bot, int part)`
- O que faz: copia a armadura atual da peça `part` do bot (byte `bot*0x100 + 0x03000590 + 0x18*part + 0x59`, ou seja
  `part+0x11` no tipo BattlePartState) para `fi+0xC8` e `fi+0xCC` (u16), e grava `fi+0xD1 = 1`. Ou seja, prepara os
  valores de exibição do dano/armadura da peça para o desenho da barra. O nome do mtbe ("SetPartSpriteId") não bate com o corpo.
- Chama: nenhuma. Chamada por 2 handlers: `Fight_DispatchOnSubstateAA_H_07_LoadStageGfx` (0x0801E60E) e
  `Fight_DispatchOnSubstateAA_H_20_ApplyPartDamage` (0x0801ED04).
- Memória: lê o bloco do bot (+0x59 + 0x18*part); escreve `fi+0xC8`, `fi+0xCC`, `fi+0xD1`. Literais: 0x08036BB0 = 0x03001800,
  0x08036BB4 = 0x03000590.
- Para o Lua: portar (três atribuições).
- Retorno: nenhum.
- Renomeado no Ghidra: sim (era `FUN_08036b84`).
- Confiança: confirmado (corpo e literais). Significado de `fi+0xD1` como "flag de exibição": provável.

## 0x08036D24 Fight_RepairPartTick (era FUN_08036d24; nome do mtbe mantido)
- Assinatura: `int f(char *counter, int bot, int part)` (retorna 1 quando acabou, 0 enquanto continua).
- O que faz: se a armadura atual da peça (`part+0x11`, byte `0x59` do bloco) é diferente da máxima (`part+0x04`,
  byte `0x4C` do bloco), soma 1 à atual e decrementa o contador `*counter`; retorna 0 se o contador ainda não chegou
  a zero. Retorna 1 se já está na máxima ou o contador zerou. É um "conserto" de uma peça por passo (animado).
- Chama: nenhuma. Chamada por 2 handlers: `Fight_DispatchOnSubstateAA_H_45_DrawPartIconsWaitTimer` (0x0801F3A2) e
  `Fight_DispatchOnSubstateAA_R_08_LoadPartIconsAndJump` (0x08020B66).
- Memória: lê/escreve o bloco do bot (+0x59 e +0x4C + 0x18*part) e o contador passado por ponteiro.
- Para o Lua: portar (um passo por quadro até o máximo ou o contador zerar).
- Renomeado no Ghidra: sim (era `FUN_08036d24`, nome igual ao do mtbe).
- Confiança: confirmado (corpo). "Conserto" é provável (ver restaurar peças nas ações de apoio).

## 0x0801C2F0 Fight_RunSubstateDispatchC (mtbe: mesmo nome)
- Assinatura: `int f(u8 arg)` (retorna 1 se o flag de saída fi+0x9E é diferente de 0).
- O que faz: grava `arg` em `fi+0x9D`, chama a rotina `table[fi+0x9C]` via trampolim `CallViaR0` (tabela de ponteiros
  Thumb em ROM 0x083AD5A4, literal em 0x0801C320) e retorna `fi+0x9E != 0`. É o par de `Fight_RunSubstateDispatch`
  (0x0801C0BC): usa outros bytes de estado (`+0x9C/+0x9D/+0x9E`) e não mexe em `+0xA6`.
- Chama: as rotinas da tabela (não lidas). Chamada por 10 sites: `Fight_DispatchOnSubstateAA_AJ_0C_RunDispatchCWaitDone`
  (0x08025D30), `_AJ_0D_RunDispatchCPlaySfx51` (0x08025D50), `_AJ_0E_StepBotVisualDownWaitSlot` (0x08025D70),
  `_AJ_13_StepBotVisualUpWaitIO` (0x08025E8C), `_BR_24_RunDispatchCThenNext` (0x08030D78), `_BR_25_RunDispatchCAnd1`
  (0x08030D94), `_BR_26_BuildPartObjects` (0x08030DB8), `_BR_27_StepPartObjects` (0x08030E18), `_BR_45_SetupMedaforceSlots`
  (0x08031710), `_BR_46_WaitMedaforceAnim` (0x08031860).
- Memória: `fi+0x9C` (índice), `fi+0x9D` (arg, escrito), `fi+0x9E` (flag de saída, lido). Literais: 0x0801C31C = 0x03001800,
  0x0801C320 = 0x083AD5A4.
- Retorno: 1 quando `fi+0x9E != 0` (o handler que chamou avança).
- Renomeado no Ghidra: não (já tinha nome).
- Confiança: confirmado (corpo e literais). A tabela 0x083AD5A4 tem pelo menos 0x0C entradas lidas; o conteúdo não foi decodificado.

## Quem lê +0xA6 (cadeia confirmada, parcial)
- Leitor: `ActionFamily_StatusCheckFamily_Handler` (0x080305E0, usado como handler compartilhado). Lê `fi+0xA6` e chama
  `Fight_RunSubstateDispatch(code)` (0x0801C0BC). Se o retorno é != 0, `fi+0xAA += 1` (avança o passo).
- `Fight_RunSubstateDispatch(code)`: grava `code` em `fi+0xA2` e chama a rotina `table[fi+0xA1]` (9 entradas em ROM
  0x083AD580: 0801C0F9, 0801C111, 0801C139, 08000841 (sub-estado 3 = "pronto"), 0801C189, 0801C1C1, 0801C23D,
  0801C299, 08000841). Retorna 1 se `fi+0xA1 == 3` (e zera).
- Sub-estado 1 (0x0801C110): chama `FUN_0801AC24(fi+0xA2)`; se retorna != 0, `fi+0xA1 += 1`.
- `FUN_0801AC24(code)` (0x0801AC24): se a flag 0x03.. (DAT_0801AC58) é 0 ou `DAT_0801AC5C+0x89 == 1`, chama
  `StateMachine_DispatchStep(table[code])`, com a tabela de ponteiros em ROM 0x083C6744 (literal 0x0801AC60), e zera
  `+0x1B` se o passo terminou. Senão retorna 1.
- Codigo -> rotina: a tabela 0x083C6744 tem entradas com valores 0x083C4E24, 0x083C4E4C, ... (0x083C5848 e mais), que
  apontam para DADOS (roteiros de estado), não para código Thumb. Por isso o mapa código -> mensagem NÃO foi fechado.
  Os codigos (0x1A, 0x1C, 0x1D, 0x25, 0x2B, 0x2C, 0x2D, 0x39) são índices nessa tabela. O tamanho da tabela e o
  mapeamento de cada código ficam em aberto (ver Pendente).
- Confiança: confirmado (cadeia de chamadas e literais); mapa código -> roteiro: NÃO fechado.

---

## Pendente (não feito nesta rodada)
- Mapa código -> roteiro/mensagem de `+0xA6`: ler o tamanho da tabela 0x083C6744 e a rotina `StateMachine_DispatchStep`
  (decompilar e achar o byte do código de cada entrada). Os códigos 0x1A..0x39 vêm de HA*.md e são índices nela.
- Confirmar o significado de `fi+0xA1`/`fi+0xA2` com os handlers HA*, e o conteúdo da tabela 0x083AD5A4 (rotinas de
  `Fight_RunSubstateDispatchC`).
- Conferir nos HA*: os nomes `Fight_DispatchOnSubstateAA_*_SetStepCodeNN` (produtores de +0xA6) para listar cada código.
