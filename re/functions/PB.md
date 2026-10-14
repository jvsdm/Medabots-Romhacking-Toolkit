# GH-PB: montagem dos bots do jogador (inicio da luta) e retorno ao save (2026-10-14)

Status: EM ANDAMENTO (rascunho; secoes "Pendente" no fim). Enderecos: Ghidra. Offset da pesquisa do PC = Ghidra - 0x40.
Nada gravado no Ghidra (so leitura). Simbolos em `PB_simbolos.csv`.

## Passo 0: fontes lidas
- README.md (funcoes), DF.md, DF2.md (inimigos: Fight_InitializeEnemyBotStates 0x080150C0), SV.md, PS.md.
- DM.md §8.2 (Medabot_SpawnFromRecord 0x08041D70, formula de especialidade do jogador), EN.md §5, AI2.md §pesos.
- pc/research/README.md: grep (sem leitura integral).

## Passo 1: pontos ja conhecidos (de DF/DF2/DM/SV/PS, nao re-verificados ainda)
- Fight_RefreshBotPartRecords 0x08014EC8 (jogador): chama Fight_CopyBotSlotRecords, Fight_CopyBotIdentityRecords e Fight_InitializePartStates(0,n). SENTIDO a confirmar.
- Fight_CopyBotSlotRecords 0x08014EE8: DF diz bloco -> roster (0x03000BE0+slot*0x40); DF2 diz que volta ao save. Confirmar no decompile.
- Fight_InitializePartStates 0x08014F90: bloco+0x48+0x18k <- gPartStats(B+0x43+k) (+0x4C max, +0x59 atual, +0x5F id).
- Medabot_SpawnFromRecord 0x08041D70 (jogador): B+0x20.. = medalha+8 * (1+nivel/10), teto 100. Origem de IWRAM 0x03000537: aberta.
- Tela de escolha (PS.md): 0x08013E30 UpdateSelectedMedabotPreviewScreen; grade 3x3 ctx 0x03000440; time 0x030017A0.
- Time: 9 registros de 8 B em 0x030017A0 ([estado, genero, medalha, cabeca, braco D, braco E, pernas, indice]).

## Passo 2: Fight_RefreshBotPartRecords 0x08014EC8 e Fight_CopyBotSlotRecords 0x08014EE8 (CONFIRMADO no decompile)
- Literais (lidos na ROM, leitura): 0x08014EE4 = 0x03001800 (fi); 0x08014F34 = 0x03001800 (fi); 0x08014F38 = **0x030017A0** (time, 9x8 B);
  0x08014F3C = **0x03000BA0** (roster base - 0x40; o indice vem de Slot_FindByIdPlus1, que devolve indice+1); 0x08014F40 = 0x03000590 (blocos).
- `Fight_RefreshBotPartRecords` 0x08014EC8: chama, nesta ordem:
  1. `Fight_CopyBotSlotRecords()`;
  2. `Fight_CopyBotIdentityRecords()` (DF2: copia bot+0x00..0x07 para tabela de 8 B por id);
  3. `Fight_InitializePartStates(0, fi+0x5A)` (contagem = fi+0x5A = bots ativos do jogador).
- `Fight_CopyBotSlotRecords` 0x08014EE8 (CONFIRMADO): para i = 0 .. fi+0x5A - 1:
  - `t = (fi+0x41+i) & 0xF`; `medalha = time[t*8 + 2]` (0x030017A0 + t*8 + 2);
  - `slot = Slot_FindByIdPlus1(medalha)` (indice+1 do registro de medalha no roster);
  - `Mem_CopyBytes(0x03000BA0 + slot*0x40, 0x03000590 + i*0x100, 0x40)`.
  => **sentido BLOCO -> ROSTER (save)**: os 0x40 B do bloco do bot i voltam para o registro de medalha do roster (0x03000BE0 + idx*0x40).
  Efeito no save: grava no registro de medalha o que estiver no bloco (bot+0x00..0x3F): cabecalho (+0x01,+0x04..+0x07),
  nivel (+0x05), cabeca e especialidade (+0x0A..+0x27), pecas (+0x48..+0x5F), status (+0xBA), etc. Ou seja, o bloco
  e a copia de trabalho e o roster recebe o estado de volta (pendente: ver "Pendente" sobre quando o registro muda).
- Consequencia: fi+0x41+i = indice do time (0..8) do bot i do jogador (teampick) — provavel, o escritor ainda nao foi achado.
- Confianca: confirmado (leitura de Mem_CopyBytes(dst, src, n); literais lidos). Slot_FindByIdPlus1 = indice+1: provavel (pelo literal 0x03000BA0).

## CORRECAO DE SENTIDO (passo 3): Mem_CopyBytes(SRC, DST, n)
- `Mem_CopyBytes` 0x0803F63C (decompile): `*param_2 = *param_1` -> **param_1 = origem, param_2 = destino**.
  Isto INVERTE o que DF.md / DF2.md / a primeira versao deste arquivo afirmavam: ver abaixo.
- Literais lidos na ROM: 0x08014F84 = 0x03001800 (fi); 0x08014F88 = **0x030017A0** (time); 0x08014F8C = **0x030005D0** (bloco+0x40 do bot 0);
  0x080150B8 = 0x03000590 (blocos); 0x080150BC = 0x0300052E (buffer de GetPartStats); 0x08041D5C = **0x03000BE0** (roster);
  0x08041E70 = 0x03000537; 0x08041E74 = 0x03000440.

## Passo 3: copias do inicio da luta (CONFIRMADO no decompile, com a ordem de Mem_CopyBytes corrigida)
Ordem em `Fight_SetupBattleBotStates` 0x080129F2 (CONFIRMADO): `if (ctx+0x89 != 1) { Fight_ClearBattleBuffer; Fight_UpdateBotActiveFlagCount; Fight_RefreshBotPartRecords; Fight_InitializeEnemyBotStates; }`
seguido de `Fight_RefreshBotPartSlotDisplays` e `Fight_ApplyPostBattleMode20PlayerSideState` (se ctx+0x89 != 1).
(ctx = 0x03001800 - ver literal 0x08012A14 no decompile; ctx+0x89 = flag de "link/nao iniciar".)

Para o jogador, `Fight_RefreshBotPartRecords` 0x08014EC8 = (1) `Fight_CopyBotSlotRecords`, (2) `Fight_CopyBotIdentityRecords`, (3) `Fight_InitializePartStates(0, fi+0x5A)`.

1. **Fight_CopyBotSlotRecords 0x08014EE8** (CONFIRMADO): para i = 0 .. fi+0x5A-1:
   `t = (fi+0x41+i) & 0xF`; `med = time[t*8 + 2]` (0x030017A0 + t*8 + 2 = **medalha do Medabot escolhido**);
   `r = Slot_FindByIdPlus1(med)`; `Mem_CopyBytes(0x03000BA0 + r*0x40, 0x03000590 + i*0x100, 0x40)`.
   => **roster -> bloco**: os 0x40 B do registro de medalha (save) entram em bot+0x00..0x3F. (Sentido corrigido.)
   Por isso bot+0x01 (medalha), +0x04..+0x07 (bytes de medalha), +0x05 (nivel), +0x0A..+0x27 (cabeca/especialidade) vem da medalha.
2. **Slot_FindByIdPlus1 0x08041D3C** (CONFIRMADO): varre 30 registros de 0x40 a partir de 0x03000BE0; achou se byte0 & 0x80 != 0 e byte1 == id;
   devolve indice+1; 0 se nao achou (nesse caso CopyBotSlotRecords copia de 0x03000BA0 = lixo; sem tratamento; **risco**).
   Portanto medalha = byte +1 do registro (confirma SV) e flag de registro valido = bit 7 de byte0.
3. **Fight_CopyBotIdentityRecords 0x08014F44** (CONFIRMADO, corrigido): para i = 0 .. fi+0x5A-1:
   `Mem_CopyBytes(0x030017A0 + ((fi+0x41+i)&0xF)*8, 0x030005D0 + i*0x100, 8)`:
   **registro do TIME -> bot+0x40..+0x47**:
   - bot+0x40 <- time[0] (estado; nao zero = presente no campo, ver +0x40 em DF.md);
   - bot+0x41 <- time[1] (genero); bot+0x42 <- time[2] (medalha);
   - **bot+0x43 <- time[3] (cabeca), +0x44 <- time[4] (braco D), +0x45 <- time[5] (braco E), +0x46 <- time[6] (pernas)**;
   - bot+0x47 <- time[7] (indice do registro no time).
   => DF2 dizia "bot+0x00..0x07 para tabela indexada" — ERRADO. O correto: o time preenche +0x40..+0x47 do bot.
4. **Fight_InitializePartStates(side=0, n=fi+0x5A) 0x08014F90** (CONFIRMADO, por bot i de 0..n-1, base B = 0x03000590+i*0x100):
   - B+0xB8 <- i (side/indice do slot); B+0xBA <- 1 (ativo; ver Y_0E);
   - para k = 0..3 (cabeca, braco D, braco E, perna), id = B+0x43+k: `GetPartStats(id, k, buf)`; `Mem_CopyBytes(buf, B+0x48+0x18k, 13)`
     (gPartStats -> registro de peca; DF2 estava certo nesta parte);
   - B+0x4C+0x18k = armadura MAXIMA (gPartStats +4); B+0x59+0x18k <- B+0x4C+0x18k (atual = max no inicio);
   - B+0x5F+0x18k <- B+0x43+k (id da peca). (B+0xA7 = B+0x5F+0x18*3, perna: id confirmado.)
   - Extra na perna (k=3): copia 6 B de B+0x94..0x99 para B+0xA1..0xA6 (atual da perna = max; os 5 bytes seguintes: campos da perna). Provavel: estado de "perna" para o bloco de atributos (+0xA1..+0xA7); nao verificado para que servem.
   Para os inimigos, o mesmo InitializePartStates e side=3 (ver DF2 passo 1; agora nao ha contradicao: inimigos preenchem +0x43..+0x46 em Fight_BuildBotPartDisplayStats).

## Medalha/nivel/especialidade do jogador (ponte com DM §8.2)
- Como +0x05 (nivel) e +0x20..+0x27 (especialidade) do jogador chegam ao bloco: por Medabot_SpawnFromRecord 0x08041D70 (DM §8.2, literal 0x08041E70 = 0x03000537). Quem chama essa funcao: PENDENTE (passo 4).
- Pendente: confirmar se Medabot_SpawnFromRecord e chamada dentro de Fight_RefreshBotPartRecords ou depois.

## Passo 4: fim da luta -> save (Fight_AwardPartExperience 0x080127DC, CONFIRMADO no decompile)
- Chamada no fim (xref de Slot_FindByIdPlus1 e do modulo de recompensa; FL3/DF2 ja citavam XP). Para os 3 slots do jogador
  (`bot = 0x03000590 + k*0x100`, k = 0..2) com **bot+0xBA != 0** (inclui derrotado = 2; so o vazio (0) e pulado):
  1. `r = Slot_FindByIdPlus1(bot+1)` (medalha) -> registro do roster `0x03000BE0 + (r-1)*0x40`;
  2. se roster+5 (nivel) != 'd' (100): `ganho = fi+0x16C` (u16, ganho de XP da luta, ver FL2/FL3);
     - se roster+2 (u16, XP restante ate o proximo nivel) >= ganho: roster+2 -= ganho;
     - senao: nivel += 1 (roster+5); roster+2 = (nivel+1)*100; grava registro de evento (ctx+0x16E.. / 0x170..) e:
       - se Stage_FromLevelThresholds(nivel) != roster+8: marca evento de estagio, roster+8 = novo estagio;
       - copia 3 B da lista de medaforce da medalha (`medaforce_list + medalha*3`) para o buffer;
       - nivel == 10 (0x0A), 30 (0x1E) ou 60 (0x3C): marca evento e grava o medaforce desbloqueado.
- Ou seja, **o que volta para o save ao fim da luta**: XP (roster+2, u16), nivel (roster+5), estagio (roster+8) e
  medaforces aprendidos (lista da medalha). Nao volta: armadura, pecas (+0x48..+0x5F do bloco), status (+0xBA), posicoes.
  O bloco do bot e descartavel; a proxima luta reconstroi o bloco a partir do roster + time (passo 2-4).
- Confianca: confirmado (decompile); nomes de campos do roster (+2 XP, +8 estagio) = provavel (DM.md / SV.md).
- Obs.: `Fight_AwardPartExperience` le o registro do roster pelo `Slot_FindByIdPlus1` (indice+1); se a medalha nao existir
  (retorno 0), o registro de 0x03000BA0 e usado - mesma ressalva do passo 2.
- Pendente: quem chama Fight_AwardPartExperience (em qual cena/derrota). Caminho de derrota: nao checado nesta rodada.

## Pendente (estado atual)
- Caller 0x08004A2A (BL que chama Fight_RefreshBotPartRecords 0x08014EC8): Ghidra nao acha funcao la (decompile falhou).
- Teampick: quem escreve fi+0x41..0x43 (indices do time 0..8 dos 3 bots) e a tela de escolha dos 3 bots (Team/Parts screen de PS.md: nao ligada ainda).
- Medabot_SpawnFromRecord 0x08041D70: chamado por Medabot_SpawnFromRecord_Wrapper 0x08047E26 e Debug_PartsSpawnScreen (NAO pela luta). Logo, a especialidade do jogador (+0x20..+0x27) NAO e montada em Fight_RefreshBotPartRecords; onde ela e montada na luta: pendente.
- Origem de bot+0x05 (nivel) e +0x20..+0x27 do jogador no inicio da luta: pendente.

## Resumo: montagem do bot do JOGADOR no inicio da luta (ordem real)
1. `Fight_SetupBattleBotStates` 0x080129F2 (se fi+0x89 != 1): `Fight_UpdateBotActiveFlagCount` (fi+0x5A = nº de slots com fi+0x41..0x43 != 0).
2. `Fight_RefreshBotPartRecords` 0x08014EC8:
   a. `Fight_CopyBotSlotRecords` 0x08014EE8: roster (registro de medalha = `Slot_FindByIdPlus1(time[t*8+2])`) -> bot+0x00..0x3F (0x40 B);
   b. `Fight_CopyBotIdentityRecords` 0x08014F44: time[t*8 .. +7] -> bot+0x40..+0x47 (estado, genero, medalha, cabeca, braco D, braco E, perna, indice);
      com t = (fi+0x41+i)&0xF (i = 0..fi+0x5A-1), bloco = 0x03000590 + i*0x100;
   c. `Fight_InitializePartStates(0, fi+0x5A)` 0x08014F90: bot+0xB8 = i; bot+0xBA = 1; pecas k=0..3 (cabeca/D/E/perna) a partir de bot+0x43+k:
      gPartStats -> bot+0x48+0x18k (13 B); max +0x4C+0x18k; atual +0x59+0x18k = max; id +0x5F+0x18k = bot+0x43+k.
3. Ainda NAO montado nesta rotina: nivel (+0x05 vem de roster+5 via passo 1a), especialidade (+0x20..+0x27): so o template
   do inimigo (Fight_InitializeBotFromTemplate) e' conhecido; a formula do jogador (DM §8.2, Medabot_SpawnFromRecord) nao e chamada na luta. Pendente.

## Tabela (bloco do JOGADOR, bot i = 0..2) - adicional a DF.md
| Offset | Campo | Escrito por | Conf. |
|---|---|---|---|
| +0x00..+0x3F | copia do registro de medalha (roster) | Fight_CopyBotSlotRecords (src = roster) | C |
| +0x40..+0x47 | registro do time (estado, genero, medalha, cabeca, D, E, perna, indice) | Fight_CopyBotIdentityRecords (src = time) | C |
| +0x43..+0x46 | ids de cabeca/braco D/braco E/perna | idem (= time[3..6]) | C |
| +0xB8 | indice do slot (i) | Fight_InitializePartStates | C |
| +0xBA | status: 1 = ativo | Fight_InitializePartStates (2 = derrotado: Fight_ResetBotPartState) | C |
| +0x48+0x18k..+0x54 | registro de peca (gPartStats) | Fight_InitializePartStates | C |
| +0x4C+0x18k / +0x59+0x18k / +0x5F+0x18k | armadura max / atual / id | Fight_InitializePartStates | C |
| +0x41, +0x42 | genero e medalha (copiados do time) | Fight_CopyBotIdentityRecords | C |
| +0xA1..+0xA7 | copia de 6 B da perna (max -> atual) + id da perna em +0xA7 | Fight_InitializePartStates | C (bytes); uso P |

## Teampick (pendente)
- Os 3 slots fi+0x41..0x43 (indice do time, 0..8; 0 = vazio) e a escolha dos ate 3 bots (1o = lider, PC: teampick.lua) NAO tem
  escritor achado nesta rodada. Ghidra: sem literal direto para 0x03001841 (acesso via base fi). Proximo passo: achar a funcao
  do estado de "escolha de equipe" (cena antes de Fight_SetupBattleBotStates) e varrer escritas em fi+0x41..0x43.
- 0x08004A2A (BL que chama Fight_RefreshBotPartRecords): Ghidra nao tem funcao la; provavel miolo de funcao nao detectada.

## Nomes (propostos, NAO renomeados no Ghidra: outros .md usam os nomes atuais)
- Fight_CopyBotSlotRecords -> Fight_LoadBotFromRosterRecord (roster -> bloco); Fight_CopyBotIdentityRecords -> Fight_LoadBotFromTeamRecord.
