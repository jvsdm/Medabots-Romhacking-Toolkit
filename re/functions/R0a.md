# R0a: funções da luta (bots, peças, seleção de ação), 25 endereços

Faixa R0a (lista `R0a_lista.txt`). Todas são Thumb. Blocos: bot = `0x03000590 + bot*0x100` (offsets Ghidra);
peças no bot a partir de +0x40. Nomes já existentes no mtbe.csv foram renomeados no Ghidra; os novos só
foram renomeados se confirmados ou prováveis.

## 0x080127DC Fight_AwardPartExperience (provável)
- Assinatura: `void Fight_AwardPartExperience(void)`
- O que faz: zera um buffer de 0x18 bytes e, para cada um dos 3 bots ativos (+0xBA), acha o slot da peça pelo id (+1). Se a durabilidade/contador (+2) não zerou, só desconta; se zerou, sobe o nível da peça (+5), recalcula o limite (nível+1)*100, grava um evento de "subiu de nível" e, nos níveis 10, 30 e 60, marca um flag extra. Usa `Stage_FromLevelThresholds` para o estágio.
- Chama: FUN_0803F624 (memset), `Slot_FindByIdPlus1`, FUN_0803F63C (memcpy), `Stage_FromLevelThresholds`. Chamada por: não mapeado.
- Memória: lê/escreve o bot (0x03000590 + i*0x100, +0xBA, +0x16C) e o slot de peça (base DAT_08012844, stride 0x40). Escreve eventos em DAT_08012848 + i*8 + 0x16E..0x174.
- Para o Lua: sim (progressão de nível da peça após a luta).
- Confiança: provável.

## 0x08014E7C Fight_ClearBattleBuffer (chute)
- Assinatura: `void Fight_ClearBattleBuffer(void)`
- O que faz: zera 0x600 bytes em DAT_08014E90 com FUN_0803F624 (memset). Limpa um buffer de luta; o destino não foi identificado.
- Chama: FUN_0803F624. Chamada por: não mapeado.
- Para o Lua: "não precisa" se o buffer for só de cache; confirmar o destino.
- Confiança: chute.

## 0x08014E94 Fight_UpdateBotActiveFlagCount (provável)
- Assinatura: `void Fight_UpdateBotActiveFlagCount(void)`
- O que faz: conta quantos dos 3 bytes de flag (+0x41..+0x43) do bot estão ligados e grava a contagem em +0x5A; copia +0x02 para +0x5B.
- Chama: nada. Chamada por: não mapeado (o agregador Fight_RefreshBotPartRecords não a chama; ver a lista de chamadas dele).
- Para o Lua: sim (contador de peças ativas, usado pelo resto da atualização).
- Confiança: provável.

## 0x08014EC8 Fight_RefreshBotPartRecords (provável)
- Assinatura: `void Fight_RefreshBotPartRecords(void)`
- O que faz: agregador: chama Fight_CopyBotSlotRecords (0x08014EE8), FUN_08014F44 (copia identidade, chute) e Fight_InitializePartStates(0, contagem) para o lado do bot de índice 0.
- Chama: FUN_08014EE8, FUN_08014F44, Fight_InitializePartStates. Chamada por: não mapeado.
- Para o Lua: sim (ordem de atualização dos registros de peça).
- Confiança: provável.

## 0x08014EE8 Fight_CopyBotSlotRecords (provável)
- Assinatura: `void Fight_CopyBotSlotRecords(void)`
- O que faz: para cada bot da contagem (+0x5A), pega o slot pelo id (`Slot_FindByIdPlus1` do byte +0x41 low nibble) e copia 0x40 bytes desse slot da tabela para o bloco do bot.
- Chama: `Slot_FindByIdPlus1`, FUN_0803F63C. Chamada por: Fight_RefreshBotPartRecords.
- Memória: lê PTR_DAT_08014F3C (tabela de slots, 0x40 cada); escreve em PTR_DAT_08014F40 + i*0x100.
- Para o Lua: sim.
- Confiança: provável.

## 0x08014F44 FUN_08014F44 (chute, sem rename)
- Assinatura: `void FUN_08014F44(void)` (nome sugerido: `Fight_CopyBotIdentityRecords`)
- O que faz: para cada bot da contagem, copia 8 bytes de uma tabela indexada por (+0x41 & 0xF)*8 para o início do bloco do bot (stride 0x100).
- Chama: FUN_0803F63C. Chamada por: Fight_RefreshBotPartRecords.
- Para o Lua: sim se a tabela de 8 bytes for usada; confirmar o que é a tabela.
- Confiança: chute.

## 0x080150C0 Fight_InitializeEnemyBotStates (provável)
- Assinatura: `void Fight_InitializeEnemyBotStates(void)`
- O que faz: para cada bot inimigo (contagem em +0x5B do bloco de template), chama `Fight_InitializeBotFromTemplate`; depois monta a tabela de exibição (FUN_0801520C) e chama `Fight_InitializePartStates(3, contagem)`. O índice 3 indica o lado inimigo (bots 3..5).
- Chama: Fight_InitializeBotFromTemplate, FUN_0801520C, Fight_InitializePartStates. Chamada por: não mapeado.
- Para o Lua: sim (setup do lado inimigo).
- Confiança: provável.

## 0x080150FC Fight_InitializeBotFromTemplate (confirmado)
- Assinatura: `void Fight_InitializeBotFromTemplate(uint bot_index)`
- O que faz: copia o registro de medalha para o scratch, preenche o cabeçalho do bot e o registro da peça cabeça; expande 4 bytes de especialidade em 8 níveis e escala pelo nível da medalha.
- Chama: memcpy (cópias de 0x10 e 0xE bytes). Chamada por: Fight_InitializeEnemyBotStates.
- Para o Lua: sim.
- Confiança: confirmado (lido no código; nome vindo do mtbe).

## 0x0801520C Fight_BuildBotPartDisplayStats (provável)
- Assinatura: `void Fight_BuildBotPartDisplayStats(void)`
- O que faz: para cada bot da contagem, grava uma tabela de exibição no bloco +0x340 (stride 0x100): marca 3, chama `GetPartStats` com o id da peça e copia 4 bytes de identidade da peça.
- Chama: GetPartStats. Chamada por: Fight_InitializeEnemyBotStates.
- Para o Lua: sim (dados de estatística de peça na tela de status).
- Confiança: provável.

## 0x0801528C FUN_0801528C (chute, sem rename)
- Assinatura: `void FUN_0801528C(void)` (nome sugerido: `Fight_RefreshBotPartSlotDisplays`)
- O que faz: se o byte em DAT_080152C4+0x89 == 1 e o byte em DAT_080152C8 != 0, percorre os slots 3..5 (bloco +0x340) e 0..2 (bloco +0x40) e, onde há dado, chama FUN_08031A3C(slot). Senão, percorre os slots 0..5 (bloco +0x40) e faz o mesmo.
- Chama: FUN_08031A3C (não documentada aqui). Chamada por: não mapeado.
- Para o Lua: sim se a atualização de tela de peça for portada; FUN_08031A3C é o que de fato desenha.
- Confiança: chute.

## 0x0801531C Fight_SelectActionsForBotGroup (provável)
- Assinatura: `void Fight_SelectActionsForBotGroup(int first_bot)`
- O que faz: para 3 bots a partir de `first_bot`, se o bot tem dado em +0x40, tenta `Fight_TrySelectActionFromSpecialityCycle` e, se devolver "não resolvido", chama `Fight_SelectFallbackActionFromPriorityList`.
- Chama: Fight_TrySelectActionFromSpecialityCycle, Fight_SelectFallbackActionFromPriorityList. Chamada por: não mapeado.
- Para o Lua: sim (escolha de ação do turno, por lado).
- Confiança: provável.

## 0x08015358 Fight_SelectActionFromCycleOrPendingOverride (confirmado)
- Assinatura: `void Fight_SelectActionFromCycleOrPendingOverride(int bot)`
- O que faz: se o efeito pendente em +0xC9 é 0x2C (','), chama direto o fallback da lista de prioridade; senão chama `Fight_TrySelectActionFromSpecialityCycle` e chama o fallback quando ele devolve nonzero.
- Chama: Fight_SelectFallbackActionFromPriorityList, Fight_TrySelectActionFromSpecialityCycle. Chamada por: não mapeado.
- Para o Lua: sim.
- Confiança: confirmado (lido no código).

## 0x08015390 Fight_TrySelectActionFromSpecialityCycle (confirmado)
- Assinatura: `int Fight_TrySelectActionFromSpecialityCycle(int bot)`
- O que faz: lê o valor da tabela de ação no índice de ciclo (+0x3F, tabela +0x18). 1..3: testa a peça com FUN_080156F8 (se falhar, limpa a fonte); 4: limpa a fonte com `Fight_SelectActionSource(bot,0)` e devolve 0; 5..7: tenta medaforça (`Fight_TrySelectMedaforceAction`). Retorno: 0 quando limpou a fonte, 1 nos outros caminhos. O chamador chama o fallback quando recebe nonzero, o que parece invertido em relação ao nome: não fixar sem reler o chamador.
- Chama: FUN_080156F8, Fight_SelectActionSource, Fight_TrySelectMedaforceAction. Chamada por: Fight_SelectActionFromCycleOrPendingOverride, Fight_SelectActionsForBotGroup.
- Para o Lua: sim.
- Confiança: confirmado. Atenção: o comentário do mtbe diz que o valor 4 "cai no head"; o código limpa a fonte e devolve 0. O mtbe deve ser corrigido.

## 0x08015414 Fight_AdvanceSpecialityCycleIndex (confirmado)
- Assinatura: `void Fight_AdvanceSpecialityCycleIndex(int bot)`
- O que faz: incrementa o índice de ciclo de especialidade do bot (+0x3F); ao chegar a 8 volta ao valor de +0x09.
- Chama: nada. Chamada por: não mapeado.
- Para o Lua: sim.
- Confiança: confirmado (lido no código).

## 0x08015464 Fight_SelectFallbackActionFromPriorityList (confirmado)
- Assinatura: `void Fight_SelectFallbackActionFromPriorityList(int bot)`
- O que faz: se o byte +0xC1 do bot é 'P', um sorteio dá ímpar e `Fight_TrySelectMedaforceAction(bot,0,1)` devolve 0, sai sem escolher. Senão, sorteia a lista de prioridade (`FUN_080156D0`) e varre até 4 entradas de `DAT_080154B0+100` (-1 = fim) testando com FUN_080156F8; a primeira usável vira a ação; sem nenhuma, usa `Fight_SelectActionSource(bot,0)`.
- Chama: randbyte, Fight_TrySelectMedaforceAction, FUN_080156D0, FUN_080156F8, Fight_SelectActionSource. Chamada por: Fight_SelectActionFromCycleOrPendingOverride, Fight_SelectActionsForBotGroup.
- Para o Lua: sim (usar `math.random` no lugar do randbyte; sorteio não bate com o original).
- Confiança: confirmado (lido no código).

## 0x080154E0 Fight_IsPartActionLockedByPendingEffect (confirmado)
- Assinatura: `int Fight_IsPartActionLockedByPendingEffect(int bot, uint part)`
- O que faz: devolve 1 se o efeito pendente em +0xC9 é 0x2B ('+') e +0xCA == `part`.
- Chama: nada. Chamada por: não mapeado (usado pela UI de disponibilidade).
- Para o Lua: sim.
- Confiança: confirmado.

## 0x08015508 Fight_IsHeadActionUnavailable (confirmado)
- Assinatura: `int Fight_IsHeadActionUnavailable(int bot, int part)`
- O que faz: devolve 1 se `part == 1` (cabeça) e o byte +0x58 do bot é zero.
- Chama: nada. Chamada por: não mapeado.
- Para o Lua: sim.
- Confiança: confirmado.

## 0x08015528 Fight_IsPartActionUnavailable (confirmado)
- Assinatura: `bool Fight_IsPartActionUnavailable(int bot, int part)`
- O que faz: devolve 1 se o byte +0x59 do registro da peça (`(part-1)*0x18`) é zero.
- Chama: nada. Chamada por: não mapeado.
- Para o Lua: sim.
- Confiança: confirmado.

## 0x08015564 Fight_SelectActionSource (confirmado)
- Assinatura: `void Fight_SelectActionSource(int bot, uint source)`
- O que faz: grava a fonte da ação em +0xDD. Para 1..3 copia o alvo (+0xE0, +0xE1) do registro da peça (`(source-1)*0x18`, campos +0x56 e +0x57).
- Chama: nada. Chamada por: Fight_TrySelectActionFromSpecialityCycle, Fight_SelectFallbackActionFromPriorityList, Fight_SelectMedaforceActionSource.
- Para o Lua: sim.
- Confiança: confirmado.

## 0x080155A8 Fight_TrySelectMedaforceAction (confirmado)
- Assinatura: `int Fight_TrySelectMedaforceAction(int bot, int slot, int random)`
- O que faz: sai se a medaforça está bloqueada pelo estado de batalha, se o flag +0x58 está ligado ou se a medalha não é 'P'. Com `random`, sorteia o slot (FUN_08033074). Confere o nível da medalha (+5) contra os limites 10, 29 e 59 e chama `Fight_SelectMedaforceActionSource` com o slot 0..2 que passou. Devolve 1 se não selecionou.
- Chama: Fight_IsMedaforceSelectionBlockedByBattleState, FUN_08033074, Fight_SelectMedaforceActionSource. Chamada por: Fight_TrySelectActionFromSpecialityCycle, Fight_SelectFallbackActionFromPriorityList.
- Para o Lua: sim (limiares de nível da medaforça: 10, 29, 59).
- Confiança: confirmado.

## 0x08015648 Fight_IsMedaforceSelectionBlockedByBattleState (provável)
- Assinatura: `int Fight_IsMedaforceSelectionBlockedByBattleState(uint battle_state)`
- O que faz: devolve 1 quando o argumento é >= 3 e o byte de estado em DAT_0801565C+DAT_08015660 é <= 2. O sentido exato não está claro, porque o chamador não passa argumento novo (usa o r0 que sobrou).
- Chama: nada. Chamada por: Fight_TrySelectMedaforceAction.
- Para o Lua: sim se a regra de bloqueio da medaforça for portada; ler o chamador antes de fixar o sentido.
- Confiança: provável (a leitura do argumento é duvidosa).

## 0x08015668 Fight_SelectMedaforceActionSource (confirmado)
- Assinatura: `void Fight_SelectMedaforceActionSource(int bot, int slot)`
- O que faz: copia 3 bytes para a lista de medaforça da medalha; copia 6 bytes do descritor de medaforça (+0xA8) para o scratch; chama `Fight_SelectActionSource(bot, slot+4)`.
- Chama: memcpy, Fight_SelectActionSource. Chamada por: Fight_TrySelectMedaforceAction.
- Para o Lua: sim.
- Confiança: confirmado.

## 0x080156D0 FUN_080156D0 (chute, sem rename)
- Assinatura: `void FUN_080156D0(void)` (nome sugerido: `Fight_RandomizePriorityListSlot`)
- O que faz: sorteia um byte (`randbyte`), usa `& 0x1F` como índice e copia 4 bytes de uma origem fixa para a tabela de 32 entradas de 4 bytes.
- Chama: randbyte, memcpy. Chamada por: Fight_SelectFallbackActionFromPriorityList.
- Para o Lua: sim se a lista de prioridade embaralhada for portada (`math.random`).
- Confiança: chute.
