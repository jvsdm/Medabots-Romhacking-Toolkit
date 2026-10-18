# GH-LOSS: derrota na Robattle (2026-10-18)

Status: CONFIRMADO no decompile para (1) resultado e destino; PROVAVEL para (2) armadura e (3) byte de historia.
Leitura estatica do Ghidra (sem renomear, sem salvar). Enderecos Ghidra; pesquisa do PC = Ghidra - 0x40 (README).
Replay: NAO feito (nenhuma gravacao de derrota em `data/records/INDEX.md`).

## 1. O que a derrota tira do jogador (peca, medalha, dinheiro): NADA achado
- Resultado: `Fight_EvaluateTeamWinState` 0x0801B53C grava T+0x9C = 0x030004DC (1 = jogador venceu, 2 = perdeu).
  Escritores de 0x030004DC (xref): so EvaluateTeamWinState e `Fight_DispatchOnSubstateAA_BR_50_CheckLeadersDefeated`
  0x08030E78 (grava 2 se bot0+0x40 == 0; 1 se bot3+0x40 == 0).
- Peca: a unica rotina que da peca e `Fight_HandleRewardGrantOrSkipByPostBattleFlags` 0x08010C50 (+ sorteio 0x080121CC),
  so na vitoria (nibble de E+0xC2E == 0). Nenhuma rotina de perda de peca aparece em 0x08031120..0x0803133C.
- Medalha/XP: `Fight_AwardPartExperience` 0x080127DC so sobe XP/nivel; nao tem ramo de perda.
- Dinheiro 0x03002B70: referencias na ROM = AgbMain, Screen_*, SaveLoad_*, Overworld_StartNewGame, Save_GetMoney,
  AddClampedCounter (evento 0x3A), SubClampedCounter (evento e loja Shop_CommitBuySellTransaction 0x08062600), Stats.
  Nenhuma funcao de luta. Caveat: a busca foi pelo literal; o xref de SubClampedCounter (0x08047D71) voltou vazio,
  entao a ausencia de perda de dinheiro na luta e confirmada so pela ausencia de literal.
- Conclusao: encontro aleatorio e Robattle de historia NAO tiram peca, medalha, nivel nem dinheiro. Sem diferenca entre eles.

## 2. Para onde volta e o que acontece com a armadura
- Fim: `Fight_SceneStep48_CheckTimeUpOrLevelUp` 0x08031120: timeUp == 0 -> EvaluateTeamWinState + musica 0x11;
  senao, bot do lado sem presenca -> 0x4F; senao 0x60 (nivel). Confirmado.
- Mensagem: `Fight_SceneStep4C_ShowTimeUpWinnerText` 0x08031220 e `Fight_SceneStep44_ShowLeaderDownWinnerText` 0x080310A8
  copiam o texto (14 B se T+0x9C == 1; 18 B se T+0x89 == 1 (link); senao tabela pelo indice). Confirmado.
  Texto de derrota (GameMessages[110] no PC, BT2) nao conferido no ROM nesta rodada.
- Saida: `Fight_SceneStep53_ExitBattleScene` 0x0803133C: ctx+0x132 = 1; se T+0x89 == 1 (link) -> gMainState 10, GS 0x20;
  senao gMainState 6, GS 0x30. Confirmado. NAO ha load de save, troca de mapa, game over nem checkpoint nessa faixa.
  Destino (mesma posicao / ultima casa): fica no fluxo do mundo, nao lido. PROVAVEL: mesma posicao.
- Armadura dos bots do jogador: a derrota NAO escreve no save (nenhum escritor de save por derrota achado). O bloco de
  luta do jogador e recopiado do time no inicio da luta (DF2 passo 1: Fight_RefreshBotPartRecords 0x08014EC8 ->
  Fight_InitializePartStates 0x08014F90). Dentro da luta, a cabeca quebrada leva a Fight_ResetBotPartState 0x08036FA0
  (+0xBA=2, +0x40=0, +0x59=0): so estado de luta. PROVAVEL: armadura do save fica como estava antes da luta.

## 3. Resultado gravado e quem le
- T+0x9C (0x030004DC) = 1/2. Leitores: SceneStep4C 0x08031220 (texto), SceneStep44 0x080310A8 (texto),
  Battle_GaugeTickStep 0x08010B66, Fight_BranchFlagAdvanceGS 0x08010BAA, Menu_SetStateByArg01 0x0804766E.
- Historia: `Event_Op33_StartBattle` 0x0804C53C grava E+0xC2E (pos-batalha, 0x0300640E). A continuacao 0x0804E8C4
  copia E+0xC2D (0x0300640D, 1 ou 2) para o byte de resultado do script (DAT_0804E91C+0x39). So isso alimenta
  `prev_failed` (0x4F) em `pc/game/src/script.lua`.
- E+0xC2D escritores (xref): GameState_StartFlowByCommand 0x080479A0 (1 no caso 0x20, 0 no 0x21) e resets de tela
  (Gfx_ResetBgLayersAndSong 0x0801156E, Gfx_ResetScreenBgAndBlend 0x08011C20, Menu_TickSingleCountSelector 0x0800D392).
  NENHUM grava 2 depois da luta. Leitores: Fight_SelectTemplateForCurrentEncounter 0x08011E2E, Fight_SetupFlagSprite,
  Fight_BotPartSelect_WaitInputStep, Overworld_ShouldTriggerRandomEncounter 0x080438CC.
  => PROVAVEL: `prev_failed` nunca dispara pela luta de historia (byte sempre 1). Se algum evento testa derrota de
  verdade, usa outro byte que nao foi achado.
- Encontro aleatorio: Overworld_ShouldTriggerRandomEncounter 0x0804392C zera contador e sorteia; nao le T+0x9C.

## 4. Ligacao para o porte (NAO editado: battle.lua e world.lua sao de outro agente)
- `pc/game/src/fight/reward.lua`: `M.lossPenalty(fight)` (fight.result == 2) devolve o que a derrota faz: nada tirado,
  message 110, destination "field", gameOver false, saveChanged false. `M.storyWonByte(eC2D, result)` devolve o byte
  para prev_failed.
- Em `battle.lua`, `loseAfter` (~linha 533) deve chamar `reward.lossPenalty(fight)`, mostrar `message` e voltar ao campo
  na mesma posicao. Hoje ja faz isso; o que muda e tirar o "ABERTO" do print.
- Em `world.lua`/`script.lua`: nada a aplicar na derrota. Para o byte de historia, passar
  `storyWonByte(E.C2D, fight.result)` ao montar o contexto do evento (com o caveat do §3).

## 5. Pendencias
1. Replay de uma derrota (precisa gravar luta perdida; nao existe em data/records).
2. Texto de derrota no ROM (GameMessages[110]) e destino exato (mesma posicao vs casa).
3. Em jogo: se alguma luta de historia termina em derrota e o que o script faz depois.
4. SubClampedCounter: varredura por BL direto (nao so literal) para fechar a ausencia de perda de dinheiro.
