# GH-DF2: derrota de fato e pecas dos inimigos (2026-10-14)

Status: CONCLUIDO com pendencias (fim do arquivo). Enderecos: Ghidra. Offset da pesquisa do PC = Ghidra - 0x40.
Nada gravado no Ghidra (so leitura). Simbolos em `DF2_simbolos.csv`; sem funcao em `DF2_lista.txt`.

## Resposta curta
1. **Quem grava bot+0xBA = 2 (derrotado)**: `Fight_ResetBotPartState` 0x08036FA0 (CONFIRMADO). Grava tambem +0x40 = 0,
   +0x59 = 0, +0xB9 = 0, +0xBC = 0, +0xBD &= 0xE4, zera +0xC4 (0x0C) e +0xDC (8). Unico chamador: K_00 0x0801FE14 (xref 0x0801FE3C).
   **Condicao**: a CABECA do bot (+0x59 == 0) quebrou. Quem enfileira e `Fight_QueueHeadBrokenBotForResolution` 0x08035234:
   se bot+0x59 == 0, grava (indice do bot + 1) no 1o byte livre de fi+0xE0..E2 (3 slots; se cheio, perde o registro).
   Chamadores de enfileirar: AE_ApplyPartDamage 0x0802473C, A_0F 0x0801CBE4, H_1A 0x0801EBF8, Fight_QueueHeadBreakIfDefenderHeadZero 0x08026CBC.
   **Bracos e pernas quebrados NAO derrotam** (nao ha fila para eles). Todas as pecas: NAO.
2. **Quem grava bot+0x40 = 0**: so `Fight_ResetBotPartState` 0x08036FA0 (mesma funcao). +0x40 = 3 e gravado em
   `Fight_BuildBotPartDisplayStats` 0x0801520C (spawn do inimigo) e `Y_0E ReviveTarget` 0x08022F50 (=3, BA=1).
3. **Pecas dos inimigos (+0x48..+0x5F)**: cadeia confirmada (ver passo 1): template -> ids -> gPartStats.
4. **Derrota e recompensa**: a recompensa NAO le T+0x9C nem +0xBA. `Fight_HandleRewardGrantOrSkipByPostBattleFlags` 0x08010C50
   so olha o nibble baixo de gPostBattleModeFlags (0x0300640E). Vitoria e derrota em BR_50 fazem o mesmo GS++ (so muda
   T+0x9C e o som). Logo, a derrota nao bloqueia a recompensa por esse caminho.

## Metodo
- Varredura do ROM (python -I, so leitura): literais 0x03000590 (base do bloco do bot) e 0x030004DC; ~330 funcoes com
  `ldr` desse literal. Interpretador Thumb linear simples rastreia o registrador base (base + offset) e lista os
  STRB/STR com offset do bloco 0x40, 0xBA, 0x59, 0x5F, 0x4C. Achados confirmados por decompile no Ghidra.
- Limite: funcoes que acham o bloco por outro literal (ex.: 0x03000890 do inimigo, ou so via add) nao entram na varredura.

## Passo 1 - pecas dos inimigos (CONFIRMADO no decompile)
Cadeia em `Fight_InitializeEnemyBotStates` 0x080150C0 (count = fi+0x5B), para cada inimigo i (bloco B = 0x03000590 + i*0x100, i = 3..5):
1. `Fight_InitializeBotFromTemplate` 0x080150FC: cabecalho (+0x00 = 0x80, +0x01, +0x04, +0x05, +0x06, +0x07, +0x09),
   cabeca de 14 B (gPartRecordPtrTable[rec+4]) em +0x0A..+0x17, especialidade +0x18..+0x27. NAO escreve +0x40 nem +0x43..+0x5F.
2. `Fight_BuildBotPartDisplayStats` 0x0801520C: para k = 0..count-1, com B_k = 0x03000890 + k*0x100 e registro de template
   fi + 4 + 0x0C*k (template de 0x0C B por bot; fi = 0x03001800):
   - B+0x40 <- 3 (presenca); B+0x41 <- gPartStats[cabeca+3] (genero, via GetPartStats);
   - B+0x42 <- B+0x01 (medalha); **B+0x43, +0x44, +0x45, +0x46 <- template +4, +5, +6, +7** (ids: cabeca, braco D, braco E, perna).
3. `Fight_InitializePartStates(side, n)` 0x08014F90 (para inimigos: side 3): para k = 0..3, GetPartStats(id = B+0x43+k, kind = k, buf)
   e Mem_CopyBytes(buf, B+0x48+0x18k, 0x0D). Assim:
   - +0x48+0x18k .. +0x54+0x18k <- bytes 0..12 de gPartStats (sub-registro de 0x10 B, bot*0x40 + kind*0x10);
   - **+0x4C+0x18k (armadura MAXIMA) <- gPartStats +4**;
   - **+0x59+0x18k (armadura ATUAL) <- +0x4C+0x18k** (copia o maximo no inicio);
   - **+0x5F+0x18k (id da peca) <- B+0x43+k**.
   Para o jogador, `Fight_RefreshBotPartRecords` 0x08014EC8 chama `Fight_InitializePartStates(0, n)` depois de
   `Fight_CopyBotSlotRecords` e `Fight_CopyBotIdentityRecords`. A origem de B+0x43..+0x46 do jogador (DC3: time -> bloco, em Fight_CopyBotIdentityRecords, SRC->DST)
   foi achada depois (ver PB.md, passo 3.3).
Correcao ao DF.md: +0x4C NAO e "sem escritor": e escrito por `Fight_InitializePartStates` (copia de gPartStats+4).

## Passo 2 - derrota de fato (CONFIRMADO no decompile)
- `Fight_ResetBotPartState` 0x08036FA0 (bot = 0x03000590 + idx*0x100): +0x40 <- 0; +0x59 <- 0; +0xB9 <- 0; **+0xBA <- 2**;
  +0xBC <- 0; +0xBD &= 0xE4; Mem_FillBytes(+0xC4, 0x0C); Mem_FillBytes(+0xDC, 8).
- `Fight_DispatchOnSubstateAA_K_00_ConsumePendingBotRefresh` 0x0801FE14: procura o 1o byte != 0 em fi+0xE0..E2; zera;
  fi+0x51 <- byte - 1 (indice do bot); `Fight_RefreshDisplayedPartSpritesForActiveBots`; `Fight_PlaceActiveBotObjects`;
  **`Fight_ResetBotPartState(fi+0x51)`**; AA <- 2. Sem byte: AA += 1. (A fila nao e so de redesenho, como dizia o DF.md.)
- `Fight_DispatchOnSubstateAA_BR_50_CheckLeadersDefeated` 0x08030E78 (CONFIRMADO): T+0x9C <- 0; se bot0+0x40 == 0 ->
  T+0x9C = 2 (jogador perdeu), fade(4), SFX 0x6F, GS++; senao se bot3+0x40 == 0 -> T+0x9C = 1, fade, SFX, GS++;
  senao GS <- 0x48. Usa +0x40 (lider), nao +0xBA.
- `Fight_EvaluateTeamWinState` 0x0801B53C (DF.md, ja confirmado): usa a contagem de +0xBA == 2 e as pecas com +0x59 == 0
  como desempate. Dois criterios: BR_50 (lider sumiu) e EvaluateTeamWinState (contagem). Ambos saem do mesmo ResetBotPartState.
- Efeitos colaterais de +0xBA != 0 (CONFIRMADO no codigo lido): `Fight_AwardPartExperience` 0x080127DC e
  `Battle_SumPartBytesToCtx16C` 0x0801277C tratam "+0xBA != 0" como ativo; um bot derrotado (BA = 2) ainda entra no calculo de XP.

## Passo 3 - ids e tabela de funcoes (confirmados)
- `Fight_QueueHeadBrokenBotForResolution` 0x08035234: enfileira so se bot+0x59 == 0 (CONFIRMADO).
- `Fight_HandleRewardGrantOrSkipByPostBattleFlags` 0x08010C50: nibble de gPostBattleModeFlags == 0 -> `Fight_TryAssignBotPartByRoll`
  (0x080121CC) + Inv_AddCountClamp99 + GS++; senao limpa o byte e GS = 0x3E (CONFIRMADO).

## Pendencias
1. Origem de B+0x43..+0x46 (ids de peca) e de B+0x41..+0x42 do JOGADOR (DC3: time -> bloco no inicio da luta, via Fight_CopyBotIdentityRecords, SRC->DST; ver PB.md passo 3.3).
2. Varredura limitada ao literal 0x03000590: outros acessos (base 0x03000890 do inimigo por literal proprio) nao cobertos.
   Para confirmar que so K_00 pode gravar +0xBA = 2 / +0x40 = 0, varrer com os outros literais de base.
3. Se a derrota do lider bloqueia a sequencia depois de BR_50 (fade, GS e tela de resultado): so se conhece o GS++ comum;
   o destino de GS 0x3E/0x48 e o texto (HN10) nao foram seguidos ate o fim.
4. Ponto de chamada de `Fight_SceneStep48` / BR_50 (cena 8, GS 0x40) e quem zera gPostBattleModeFlags: nao achado nesta rodada.
5. Fila fi+0xE0 com 3 slots: se a fila encher, o registro e descartado em silencio (sem fallback achado).
