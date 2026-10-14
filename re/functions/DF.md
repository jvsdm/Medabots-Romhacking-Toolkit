# GH-DF: derrota do jogador e bloco do bot (2026-10-14)

Status: PARCIAL (pendencias no fim do arquivo). Nada gravado no Ghidra. Endereços: Ghidra. Offset da pesquisa do PC = Ghidra - 0x40 (README).

## Passo 0: fontes lidas
- BT.md, BT2.md, EN.md, RW.md (ver seções abaixo)
- Lidos: BT.md, BT2.md, EN.md (§5 e §6), RW.md (§1 a §6), README (funcoes).

## Conflitos a fechar no Ghidra (passo 1)
- C1: o vencedor de Fight_EvaluateTeamWinState e T+0x9C (0x03000440+0x9C = 0x030004DC, BT.md/HN9/HN10)
  ou fi+0x9C (0x03001800+0x9C = 0x0300189C, RW.md/FH)? A tarefa pede fi+0x9C; BT.md afirma que sao bytes diferentes.
- C2: cabeca do bot inimigo em bloco+0x0A (EN, RW confirmam literal 0x080151E4 = 0x030A) x +0x4C (README antigo).
  Resolvido em favor de +0x0A para a CABECA; +0x4C = registros de peca, escritor ainda aberto.
- C3: escritor de bloco+0x4C (pecas) para bots do jogador e inimigos: RW diz "nao achado".

## Passo 2: Fight_EvaluateTeamWinState 0x0801B53C (Ghidra decompile + literais lidos na ROM)
- C1 RESOLVIDO: o vencedor e T+0x9C = 0x03000440+0x9C = **0x030004DC** (literal 0x0801B588 = 0x03000440). Nao e fi+0x9C.
  fi (0x03001800) so entra no desempate: fi+0x29 x fi+0x31 (literal 0x0801B584 = 0x03001800).
- Logica (confirmada no decompile):
  1. n2(lado 0) < n2(lado 3) -> T+0x9C = 1; n2 > -> 2. n2 = Fight_CountBotsWithStatus2(lado).
  2. empate: Fight_CountEmptyPartSlots(0) < (3) -> 1; > -> 2.
  3. empate: fi+0x31 < fi+0x29 -> 1; > -> 2.
  4. empate total: sorteio com randbyte. Se T+0x89 == 1 (link): usa 0x03000B90 (byte, literal 0x0801B5D8): ==0 -> 1 se (rand&1)!=0; !=0 -> 1 se (rand&1)==0. Se T+0x89 != 1: 1 se (rand&1)!=0.
  Fallback final = 2.
- Valor: 1 = lado 0 (jogador) venceu; 2 = jogador perdeu (coerente com BT/HN9: BR_50 grava 2 quando bot0 +0x40 == 0).

## Passo 3: auxiliares de vencedor e cabeça quebrada (Ghidra decompile)
- Fight_CountBotsWithStatus2 0x0801B4C8: conta bots (3 slots do lado: base = lado*0x100) com bot+0xBA == 2.
  => bot+0xBA = status do bot (0 = vazio, !=0 = ativo, 2 = derrotado/cabeca quebrada). CORRIGE BT.md ("+0x40 = bot sem peças").
- Fight_CountEmptyPartSlots 0x0801B4F8: para bots com bot+0xBA != 0, conta partes k=0..3 com bot+0x59+0x18*k == 0.
  Inclui a CABECA (k=0) — diferente do sorteio de peca (RW.md, que exclui k=0).
- Layout da peca (record 0x18 B em bot+0x4C+0x18*k, confirmado pelos decompiles):
  +0x4C = armadura MAXIMA (Fight_AddPartArmorClampOverflow le +0x4C como teto); +0x59 = armadura ATUAL (k=0 -> cabeca).
  Dano (Fight_ApplyDamageToPart 0x08034F68 e Fight_ApplyPendingEffectDamageToPart 0x08018FF0) nunca leva atual a 0 pelo
  caminho de efeito: 0x08018FF0 satura em 1. Zerar exige o caminho de dano (ver AE_ApplyPartDamage).
- Fight_DispatchOnSubstateAA_AE_ApplyPartDamage 0x0802473C (CONFIRMADO): aplica dano ao bot alvo F+0x88
  (Fight_SubtractPartArmorClampZero); se bot+0x59 == 0 (CABECA quebrada) -> Fight_QueueHeadBrokenBotForResolution(bot),
  F+0xD0 = 0, F+0xAA = 0x1A; senao F+0xAA = 0x13.  => TESTE DE DERROTA = bot+0x59 == 0 neste ponto.
- Fila de cabeca quebrada: Fight_QueueHeadBrokenBotForResolution 0x08035234 (CONFIRMADO): se bot+0x59 == 0, grava
  id+1 do bot (0x03000590 + i*0x100 -> valor i+1) no primeiro byte livre de uma fila de 3 bytes em **0x030018E0 (fi+0xE0)**
  (literal 0x0803525C). Unico literal 0x030018E0 na ROM: leitores acessam fi com offset (nao ha literal direto).
- Chamadores (CONFIRMADOS no decompile): AE_ApplyPartDamage 0x0802473C, A_0F_QueueBrokenBotOrSkip 0x0801CBE4 (le fi+0x87
  e bloco+0x59), H_1A_QueueBrokenBotOrSkip 0x0801EBF8 (nome), Fight_QueueHeadBreakIfDefenderHeadZero 0x08026CBC (AO 0x10).

## Passo 4: derrota (quem grava o status) e presenca do bot
- Y_0E ReviveTarget (HN1, 0x08022F50, confirmado): grava alvo +0x40 = 3 e +0xBA = 1 (volta do bot ao campo).
  => +0x40 = presenca/bot no campo (!= 0), +0xBA = status (0 vazio, 1 ativo, 2 derrotado).
- BR_50 0x08030E78 (CONFIRMADO): T+0x9C = 2 se bot0 +0x40 == 0 (lider do jogador sumiu); senao 1 se bot3 +0x340 ... (bot 3 +0x40).
  Grava snd_fade(4)+SFX 0x6F e GS++; senao GS = 0x48. Usa +0x40, NAO +0xBA. (BT.md dizia "+0x40 = bot sem pecas": errado.)
- H_1A 0x0801EBF8 (CONFIRMADO): se bot+0x59 == 0 -> fila (id+1), AA += 1; senao AA += 3.
- Fight_StepBotVisualDown 0x0801AFB8 (CONFIRMADO): so anima gauge/numero (bot+0xC1 -1), nao grava +0xBA.
- AJ_0E 0x08025D6C (CONFIRMADO): chama StepBotVisualDown; nao grava +0xBA.
- Fight_SyncBotStatusFromPart 0x08014CF0 e Fight_SetTeamSideState3 0x0801B67C: nao gravam +0xBA.
- PENDENTE: quem grava +0xBA = 2 (e quem consome fi+0xE0). Nao achado nos .md; ver proximo passo.
- Fila fi+0xE0..E2: CONSUMIDOR = K_00 Fight_DispatchOnSubstateAA_K_00_ConsumePendingBotRefresh 0x0801FE14 (HA3.md,
  confirmado no corpo): zera o byte, grava fi+0x51 = id-1, atualiza sprites das peças (Fight_RefreshDisplayedPartSpritesForActiveBots,
  Fight_PlaceActiveBotObjects). NAO grava +0xBA: a fila e de REDESENHO, nao de derrota. (Corrige a hipotese do passo 3.)
- Fight_UpdateBotActiveFlagCount 0x08014E94 (CONFIRMADO): conta flags fi+0x41..0x43 != 0 (um por slot do lado 0; NAO e bot+0x41) -> fi+0x5A; fi+0x5B = fi+0x02.
- Y_00 0x08022CD0 (CONFIRMADO): ataque com bot alvo bot+0x59 == 0 -> AA+=1, A6=9.
- PENDENTE REAL: escritor de bot+0xBA = 2 (derrota do bot). Nao esta em HA/HN/AS/FL/RW/BT nem nas funcoes Down/Kill/Defeat.

## Passo 5: escritores do bloco de peca (bot+0x48+0x18*k; +0x4C = armadura max; +0x59 = armadura atual)
- Fight_CommitActiveBotPartFields 0x0801B3AC (CONFIRMADO): se fi+0x118 != 0, grava no registro da peca
  bot+0x48+0x18*k (k = fi+0xA7 = peca escolhida, bot = fi+0x87): +0x48 <- fi+0x11C; +0x49 (tecnica) <- fi+0x... (DAT_0801B4B8);
  +0x4D, +0x4E, +0x4F, +0x54 e +0x5F (id da peca) <- campos do fi. Depois Fight_RefreshDisplayedPartSpritesForActiveBots e
  Fight_PlaceActiveBotObjects. NAO grava +0x4C (max) nem +0x59 (atual).
- Fight_CopyBotSlotRecords 0x08014EE8 (CONFIRMADO): copia o bloco do bot (0x40 B em bot+0x00..0x3F) de volta para o registro
  da medalha no roster (0x03000BE0 + slot*0x40) — sentido bloco -> save (escrita NO ROSTER, nao no bloco).
- Fight_InitializeBotFromTemplate 0x080150FC (inimigos, EN.md §5): escreve bot+0x00..0x09, cabeca em +0x0A (14 B), especialidade
  em +0x18..+0x27. Nao escreve +0x48..+0x5F. Quem preenche +0x48..+0x5F dos inimigos: AINDA PENDENTE.
- Formato do registro de peca (0x18 B, base bot+0x48+0x18*k), somando os decompiles:
  +0x00 (0x48) byte de fi+0x11C; +0x01 (0x49) id de tecnica; +0x04 (0x4C) armadura MAXIMA; +0x05..+0x07 (0x4D..0x4F) campos;
  +0x0C (0x54) campo; +0x11 (0x59) armadura ATUAL (0 = quebrada); +0x17 (0x5F) id da peca.
  Nota: RW.md dizia "+0x13 do registro que comeca em +0x4C" = 0x5F; e o mesmo (0x5F - 0x4C = 0x13).
- CORRECAO: +0x41 e campo de fi (fi+0x41+i), nao do bot (Fight_UpdateBotActiveFlagCount e Fight_CopyBotIdentityRecords usam fi como base).
- Fight_CopyBotIdentityRecords 0x08014F44 (CONFIRMADO): copia bot+0x00..0x07 (cabecalho de identidade, 8 B) para a tabela de 8 B
  indexada pelo id do slot de fi+0x41+i.

## TABELA DO BLOCO DO BOT 0x03000590 + bot*0x100 (Ghidra; slots 0..2 jogador, 3..5 inimigo)
Legenda: C = confirmado no decompile/literal; P = provavel; A = aberto (quem escreve nao achado).
| Offset | Campo | Quem escreve | Quem le | Conf. |
|---|---|---|---|---|
| +0x00 | cabecalho de identidade (0x80 nos inimigos) | Fight_InitializeBotFromTemplate 0x080150FC | Fight_CopyBotIdentityRecords 0x08014F44 | C |
| +0x01, +0x04, +0x06, +0x07 | bytes de medalha (gMedals[rec+8]) | InitializeBotFromTemplate | - | C |
| +0x05 | nivel do bot (rec+9) | InitializeBotFromTemplate | Fight_AwardPartExperience 0x080127DC, FL3 (media) | C |
| +0x09 | rec+0x0E (uso aberto) | InitializeBotFromTemplate | - | C (escrita) / A (leitura) |
| +0x0A..+0x17 | cabeca (14 B), copiada de gPartRecordPtrTable[rec+4] | InitializeBotFromTemplate (literal 0x080151E4 = 0x030A) | - | C |
| +0x18..+0x1F | especialidade crua (nibbles de rec+0xA..0xD) | InitializeBotFromTemplate | - | C |
| +0x20..+0x27 | especialidade final = T[(bot+4)*8+k]*(bot+5>>1), 0->1, >100->100 | InitializeBotFromTemplate (T = 0x083AD380) | - | C |
| +0x40 | presenca no campo (!=0); Revive = 3 | Y_0E 0x08022F50 (=3); escrita de 0 na derrota: AUSENTE (A) | BR_50 0x08030E78, TickActiveBotStatusEffects 0x08018CBC | C (leitura) / A (escrita de 0) |
| +0x48 | campo de registro de peca k=0 (byte de fi+0x11C) | Fight_CommitActiveBotPartFields 0x0801B3AC | - | C |
| +0x48+0x18k +0x01 (0x49+0x18k) | id de tecnica da peca k (TQ) | Fight_CommitActiveBotPartFields | action script (TQ) | C |
| +0x4C+0x18k | armadura MAXIMA da peca k (+0x52 = power, +0x5F id via +0x13) | A (nao achado; nao escrito por inimigo nem commit) | Fight_AddPartArmorClampOverflow 0x08018F0C | A |
| +0x4D..+0x4F, +0x54 (+0x18k) | campos de peca | Fight_CommitActiveBotPartFields | - | C (escrita) |
| +0x59+0x18k | armadura ATUAL da peca k; k=0 = CABECA; 0 = quebrada | dano: Fight_SubtractPartArmorClampZero 0x08035164, Fight_AddPartArmorClampOverflow 0x08018F0C, Fight_ApplyDamageToPart 0x08034F68 (satura em 1) | Fight_CountEmptyPartSlots 0x0801B4F8, AE_ApplyPartDamage 0x0802473C, H_1A, A_0F, Y_00, BH_* | C |
| +0x5F+0x18k | id da peca k | Fight_CommitActiveBotPartFields; sorteio cego (RW) | Fight_PickRandomAvailableBotPart 0x080122A4 | C (escrita) |
| +0xBA | STATUS do bot: 0 vazio, 1 ativo (Y_0E, HN1), 2 derrotado (A: escritor nao achado) | Y_0E = 1; = 2 AUSENTE | Fight_CountBotsWithStatus2 0x0801B4C8, Fight_CountEmptyPartSlots, FL3, FL7, AS3c, R1 | C (leitura) / A (escrita 2) |
| +0xC1 | medaforce atual (0x50 = pronta); contador de gauge | BR_13/BR_20 (HN9), Fight_StepBotVisualDown 0x0801AFB8 decrementa | BR_20 0x08030CFC | C |
| +0xC4..+0xCE | efeitos de status (repair +0xC4/+0xC6, duracao +0xC9, flags +0xCE) | Fight_TickSecondaryRepairEffectAndEmitResult 0x08018D0C | Fight_TickActiveBotStatusEffects 0x08018CBC | C |
| +0xD2..+0xD4 | flags de alvo (HN3) | Fight_DispatchOnSubstateAA_... (HN3) | - | P |
| +0xDD | peca escolhida (1..3 = cabeca/direita/esquerda) | Fight_SelectActionSource 0x08015564 | BR_13, Fight_SyncBotStatusFromPart 0x08014CF0 | C |
| +0xE0, +0xE1 | zerados ao fim do medaforce (HN9) | - | - | P |

Derrota (resposta direta):
- Fight_EvaluateTeamWinState 0x0801B53C grava T+0x9C = 0x030004DC (1 = jogador venceu, 2 = jogador perdeu). Quem le:
  fi+0x9C NAO e o vencedor (0x0300189C e contador de animacao de entrada, FL/FH). So T+0x9C e lido por HN10 (passo 0x4A/0x4C, texto do vencedor,
  T+0x9E copia o nome, 0x03002B74 para 1 = jogador), BR_50 (grava o mesmo byte).
- Bloqueio da recompensa por derrota: Fight_HandleRewardGrantOrSkipByPostBattleFlags 0x08010C50 NAO le T+0x9C (RW); fica aberto se a
  derrota impede a recompensa (sem leitura explicita).
- Cabeca quebrada = derrota: teste em AE_ApplyPartDamage 0x0802473C e H_1A/A_0F: bot+0x59 == 0 -> enfileira o bot em fi+0xE0 (fila
  de REDESENHO, consumida por K_00 0x0801FE14). A derrota de fato (bot+0xBA = 2 e bot+0x40 = 0) NAO foi achada.
- Game over / perda de peca / volta ao ultimo ponto: nao achados (ver RW §3). Dinheiro em batalha: nao ha perda achada.
- Saida: BR_50 -> GS 0x48 ou fade; Fight_SceneStep48 (T+0x57 == 0 = tempo) -> EvaluateTeamWinState; Fight_SceneStep53 grava gamestate 10 (link) ou 6.

## Pendencias (passo 6)
1. Escritor de bot+0xBA = 2 e de bot+0x40 = 0 (derrota). Nao esta em HA/HN/AS/FL/RW/BT; hipotese: fluxo de KO em outra funcao (AR_ApplyTeamPartDamage 0x08027494, BP_08 0x0802F8E4, BN_07 0x0802EF5C) — NAO verificado.
2. Escritor da armadura MAXIMA (+0x4C+0x18k) de inimigos e do jogador.
3. Escritor de +0x48..+0x5F para os bots inimigos (sem Fight_InitializeBotFromTemplate).
4. Quem le bloco+0x09 (rec+0x0E) e +0x24..+0x27 do template.
