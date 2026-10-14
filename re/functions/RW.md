# RW: recompensa, XP e fim de batalha (GH-RW, 2026-10-14)

Leitura estática (Ghidra decompile/xrefs + literais lidos da ROM com `python -I`). Nada testado no emulador.
Confiança: "confirmado" = decompile + literal; "provável" = padrão de uso; "aberto" = não fechado.
Bloco de bot (Ghidra): 0x03000590 + slot*0x100 (slots 0..2 = jogador, 3..5 = inimigos). E = 0x030057E0.

## 1. Peça ganha na vitória (confirmado)

Entrada: Fight_HandleRewardGrantOrSkipByPostBattleFlags 0x08010C50 (chamada pela tabela de estados DATA 0x083ACF00).
Byte de controle = **E+0xC2E = gPostBattleModeFlags 0x0300640E** (nome do mtbe.csv confirmado no literal 0x08010C74).
- Nibble baixo == 0 (caminho normal): chama Fight_TryAssignBotPartByRoll 0x080121CC. Depois:
  - `Inv_ReadCounter16(0x03001969, 0x03001968) == 0` -> 0x0300196A = 1 (peça nova), senão 0x0300196A = nibble (0).
  - `Inv_AddCountClamp99(0x03001969, 0x03001968, 1)` (entra no inventário, ver IN.md).
  - contador de estado 0x0300137C += 1.
- Nibble baixo != 0: zera o byte e põe 0x0300137C = 0x3E (pula a recompensa). Em evento de história (E+0xC2E com nibble) isso é o caminho sem peça (aberto: quem põe o nibble; ver §4).

Sorteio (confirmado):
- `Fight_TryAssignBotPartByRoll` 0x080121CC: se `Fight_CountAvailableBotParts` 0x080121F0 < `Encounter_RollPercent0To99` (0..99), ou se `Fight_PickRandomAvailableBotPart` 0x080122A4 devolve 0, chama `Fight_AssignRandomBotPartSlot(3)` 0x08012344.
- Contagem (0x080121F0): para inimigos 3..5 ativos (bloco+0xBA != 0; para no 1o inativo), soma +10 por peça de kind 1..3 com "atual" == 0 (bloco+0x71, +0x89, +0xA1 = base 0x4C + 0x18*k + 0x0D; **cabeça kind 0 não conta**). Ou seja: a chance de sortear pela regra "peça que o jogador não tem" = 10 por braço/perna quebrado do inimigo (%). Com zero peças quebradas, quase sempre cai no sorteio cego.
- Pick 0x080122A4: slot inimigo = 3 + tabela ROM 0x083AD070[rand&0xF] (valores 0,1,2 em ciclo); kind = rand&3, até 4 tentativas; escolhe a 1a peça do bot cujo jogador não tem (`Inv_ReadCounter16(id, kind) == 0`). Grava 0x03001968 = kind, 0x03001969 = id, 0x03001987 = slot.
- Cego 0x08012344 (param 3): slot = 3 + tabela[rand&0xF], kind = rand&3, id = bloco+0x5F+0x18*kind, sem checar posse.
- id da peça = byte bloco+0x5F+0x18*kind (campo +0x13 do registro de peça que começa em +0x4C, usado como id; a semântica "id" é provável, o offset é confirmado).
- **Regra de Robattle ("submissão")**: NÃO é "a peça do inimigo derrotado" nem "bot inteiro": a peça sai de um bot inimigo ativo (slots 3..5) e o que pesa é a peça quebrada (atual == 0, braço/perna). Sorteio por rand para slot e kind. Confiança: confirmado no código; a leitura "submissão" é provável.
- Peça da cabeça (kind 0) pode ser sorteada mas não conta na chance. (aberto: se o jogo distingue a "peça quebrada" também na cabeça em outro lugar.)

Escritores de 0x03001968/0x03001969 (xref): só Fight_PickRandomAvailableBotPart (080122AA) e Fight_AssignRandomBotPartSlot (0801235C). O campo 0x03001968/9 é lido também pelo desenho (Fight_DrawBotPartSprite, Fight_InitBotPartPanel).

## 2. Dinheiro e XP

- **Dinheiro**: nenhuma recompensa em dinheiro na batalha encontrada. Dinheiro = u32 0x03002B70 (SAVE_HEADER_RAM, mtbe.csv). Único chamador de `AddClampedCounter` visto: `EventOpcode3A_GetMoney` 0x0804EDC6 (evento). Dinheiro perdido: não achado (`SubClampedCounter` não verificado nesta rodada; loja em Shop_CommitBuySellTransaction 0x08062600 é o gasto normal). Confiança: confirmado para o ganho (só evento); perda na batalha: aberto.
- **XP (valor)**: Battle_SumPartBytesToCtx16C 0x0801277C soma (u16) em **fight_information+0x16C (0x0300196C)** os bytes de bloco+0x52, +0x6A, +0x82, +0x9A dos bots inimigos ativos (slots 3..5; ativos = bloco+0xBA != 0). Ou seja, XP = função da força/peças dos inimigos derrotados ativos. Confirmado (literais 0x080127D4=0x0382 com base 0x03000590 -> slot 3; 0x080127D8=0x0352). Cinco-dígitos não medidos.
- **XP (destino)**: Fight_AwardPartExperience 0x080127DC. Para os bots do jogador i = 0..2 (bloco 0x03000590 + i*0x100), se bloco+0xBA != 0:
  - `Slot_FindByIdPlus1(bloco+1)` acha o registro de **medalha** na tabela 0x03000BE0 (registros de 0x40 B). Confirmado: bloco+1 indexa gMedals 0x083B62D8 (+id*0x10), não é id de peça. Logo o XP vai para a MEDALHA do bot, não para a peça.
  - Se rec+5 (nível) != 'd' (100): se fi+0x16C < rec+2 (XP restante) -> subtrai; senão **nível++**, rec+2 = (nível+1)*100, grava o evento de nível (fi+0x16E+k*8) e, se o estágio muda (`Stage_FromLevelThresholds` 0x080420C4 com limiares de gMedals+3..+6), atualiza rec+8 e o evento.
  - Medaforça: nível 10, 30 e 60 (rec+5 == 0x0A/0x1E/0x3C) marca flag fi+0x174 e lê o byte de 0x083B7F8C[medalha*3]. Confirmado.
  - `Stage` e medaforça: ver MD.md.
- Resumo para o jogo: XP = soma de partes dos inimigos ativos, dado a cada bot do jogador ativo, e sobe o nível da medalha dele.

## 3. Derrota

- Fim de tempo/nocaute: Fight_SceneStep48_CheckTimeUpOrLevelUp 0x08031120 (confirmado): se T+0x57 == 0 -> `Fight_EvaluateTeamWinState` 0x0801B53C + `snd_song_request(0x11)`; senão, se o bot(ctx+DAT_08031144)==0 -> passo 0x4F; senão 0x60 (nível).
- Fight_EvaluateTeamWinState 0x0801B53C (confirmado): compara bots com status 2 (Fight_CountBotsWithStatus2, lado 0 x 3), slots de peça vazios (Fight_CountEmptyPartSlots), e dois campos de byte (+0x29/+0x31); com empate usa randbyte. Grava **0x030004DC (T+0x9C, ctx; não fi+0x9C; DF) = 1 ou 2** (resultado: 1 = um lado, 2 = outro; qual é derrota não fechado só com esta função; o texto de derrota de BT2 da PC usa GameMessages[110]).
- Saída: Fight_SceneStep50 (BT.md): T+0x89 == 1 (link) -> 0x58; Fight_SceneStep53 0x0803133C: gamestate_menu = 10 (link, sub 0x20) ou 6 (sub 0x30).
- **Game over / perda de peça / volta ao último ponto: NÃO encontrados na cadeia lida**. Não há chamada a load/save/game over em 0x08031120..0x0803133C. Derrota = resultado 2 em 0x030004DC (T+0x9C; DF) e saída pela mesma rota. Quem decide: Fight_EvaluateTeamWinState (com randomização no empate). Aberto: se o resultado 2 bloqueia a recompensa (HandleReward não lê fi+0x9C; a entrada é por tabela de estados 0x083ACF00, não lida).

## 4. Luta de história (evento 0x33)

- Event_Op33_StartBattle 0x0804C53C (HE2.md): E+0xC2A = id; E+0xC2F = modo; **E+0xC2E = pós-batalha** (3º byte, confirmado pela cadeia; com nibble baixo != 0, o HandleReward pula a peça).
- Continuação 0x0804E8C4 (confirmado): passo 0 = Event_ResetSceneCommandStep; passo 1 = `Event_RunSceneCommandSequence(0x20, 0)`; ao terminar, copia um byte de E (1 ou 2) para DAT_0804E91C+0x39 e Event_EndOpcode.
- **Quem põe E+0xC2D = 1**: `GameState_StartFlowByCommand` 0x080479A0 caso **0x20** (confirmado: grava 1 em E+0xC2D, 0x0300178C = 0, 0x03002B8C = 6, 0x0300137C = 0). Caso 0x21 grava 0. A continuação do evento 0x33 chama o 0x20 via Event_RunSceneCommandSequence; xrefs de GameState_StartFlowByCommand: Event_Op76_Cont_Start_Flow4_Then_Scene_Command_05 0x0804FB4A, Event_RunSceneCommandSequence 0x08050D4C, Overworld_WalkStepByDirection 0x08051C5A, Map_ExitStateDispatchFlow 0x08057B40/B60.
- Com E+0xC2D = 1, Fight_SelectTemplateForCurrentEncounter 0x08011E20 não sorteia (usa E+0xC2A = id direto). Confirmado pelo leitor 0x08011E2E.
- Como o jogo entra na batalha: evento grava id -> cena 0x20 (flow) -> gamestate 6 -> SetupBattle (ver EN.md §1d). Aberto: o caminho exato de 0x03002B8C = 6 até a tela de luta (nome de 6 não fechado no BT.md).

## 5. Layout do bloco do bot inimigo (conflito resolvido no código)

Fight_InitializeBotFromTemplate 0x080150FC (k = 0..2 = registro do template; bloco = slot 3+k). Confirmado por decompile + literais:
- **Cabeça**: copia 14 B de `gPartRecordPtrTable 0x083BA658[rec+4]` para **bloco+0x0A** (dest = 0x03000590 + k*0x100 + 0x30A; literal 0x080151E4 = 0x0000030A). **Não é +0x4C.** Literal 0x080151E4 confirmado; a EN.md estava certa.
- +0x00 = 0x80; +0x01 = medalha (gMedals[rec+8] copiado para 0x0300052E, byte 0 -> +1); +0x04 = medalha byte 1; +0x05 = rec+9 (nível); +0x06 = medalha byte 2; +0x07 = medalha byte 7; +0x09 = rec+0xE.
- +0x18..+0x1F = 4 pares de nibbles de rec+0xA..0xD (especialidade crua).
- +0x20..+0x27 = T[(bot+4)*8 + k] * (bot+5 >> 1), T = gSpeciality8ByteTable 0x083AD380 (literal 0x080151E8); 0 -> 1; > 100 -> 100.
- **Conflito +0x4C**: o +0x4C (registros de peça, kind*0x18, atual em +0x0D = +0x59 etc.) é lido por Fight_CountAvailableBotParts, Fight_PickRandomAvailableBotPart e Battle_SumPartBytesToCtx16C, mas **não é escrito por Fight_InitializeBotFromTemplate**. Quem preenche +0x4C não foi achado nesta rodada (aberto). Não há prova de que o bloco inimigo seja escrito em +0x4C aqui.

## 6. Pendências

- Derrota: se o resultado 2 (0x030004DC = T+0x9C, DF) impede a recompensa; onde o resultado vira mensagem de derrota (GameMessages[110] no PC); não há game over na cadeia lida.
- Quem preenche bloco+0x4C (peças) do bot inimigo.
- Quem põe o nibble baixo de E+0xC2E em batalhas de história (sem peça) e em aleatórias (zero).
- Dinheiro perdido (SubClampedCounter).
- Caminho gamestate 6 -> tela de batalha.
- Valor exato de 0x030004DC / T+0x9C (1 = vitória ou derrota?). Endereço resolvido (DF).
- Medidos no emulador: nenhum.
