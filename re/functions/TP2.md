# GH-TP2: escolha de ate 3 bots, valor de fi+0x41, despachante 0x083AC560/568 (2026-10-14)

Status: EM ANDAMENTO (rascunho). Leitura estatica (ROM EU, so leitura) + Ghidra decompile. Continua `TP.md` e `PB.md`.
Simbolos em `TP2_simbolos.csv`; codigo sem funcao em `TP2_lista.txt`.

## 1. Valor gravado em fi+0x41 (candidatos 0x0806B8CE, 0x0806C7E4) — EM ANDAMENTO
(sem resultado ainda)

## 2. Tela de escolha de ate 3 bots — EM ANDAMENTO
(sem resultado ainda)

## 3. Despachante de 0x083AC560 / 0x083AC568 — EM ANDAMENTO
(sem resultado ainda)

## 4. Relê de especialidades +0x20..+0x27 depois de Fight_AwardPartExperience — EM ANDAMENTO
(sem resultado ainda)

## Passo 1 (Ghidra): 0x0806C694 = battle_phase_spawn_move (0x0806C694-0x0806C881)
- Ghidra: 0x0806C7E4 NAO e entrada; esta no miolo de `battle_phase_spawn_move` (decompile lido, ja nomeado no Ghidra).
- 0x0806B8CE/0x0806B826 sem funcao no Ghidra. Xref de 0x0806B826: nenhuma. (Em andamento: literais de fi e chamador.)
- Literais (ROM, python -I): 0x0806C84C = 0x03001800 (fi); 0x0806C714 = 0x03002C30 (gObjectSlots); 0x0806C884 = 0x0300137C (GS).
- **Correcao ao TP.md**: o "candidato" 0x0806C7E4 grava a CONSTANTE 0 em fi+0x41 (ramo uVar1 >= 0x50 do decompile: `*(fi+0x41) = 0`), nao o r2. Logo NAO e escritor de valor de escolha; e limpeza no fim da fase de spawn/move.
- 0x0806B8CE (disassemble_function, Ghidra; corpo de `Screen_InitObjectSlotsLz77` 0x0806B820..0x0806BA78): `mov r3,#0` em 0x0806B8A0, depois `ldr r0,=fi; add r0,#0x41; strb r3,[r0]` = **fi+0x41 <- 0** (zera). Tambem fi+0x43 <- r5 (3) no 0x0806B8D8 e fi+0x7A0 <- 3 (GS de fase). E tela de INICIO de objetos/tela, nao escolha.
- **Resultado item (1) parcial**: os 3 candidatos de TP.md (0x080441EE, 0x0806B8CE, 0x0806C7E4) sao TODOS escritas de ZERO em fi+0x41 (limpeza de tela/fase). O escritor de valor nao-zero (indice do time) continua NAO achado. Proximo: procurar escrita com valor != 0 via r-variavel em fi+0x41..43 (ver item 2).

## 3. Despachante de 0x083AC560/0x083AC568 — PARCIAL (sem literal)
- Literal 0x083AC560 e 0x083AC568 (LE) na ROM inteira: ZERO ocorrencias (python -I). Nenhum `ldr rX,[pc]` aponta para a base. O despachante nao carrega a base por literal direto.
- Tabela: u32 de ponteiros Thumb: 0x083AC560 = 0x080049C9, 0x564 = 0x080049ED, 0x568 = 0x08004A19 (GS 0 -> 0x08004A18 com bit thumb), 0x56C = 0x08004A41, 0x570 = 0x08004A79, 0x574 = 0x08004AF9, 0x578 = 0x08004C59, 0x57C = 0x08004A41.
- Referencias a valores dentro da mesma regiao (0x083AC5A0..0x083AC610) existem em pools de 0x08004F88..0x08007224 e 0x083AC604..610: sao outras sub-tabelas/ponteiros, nao a base.

## 4. Relê de especialidades +0x20..+0x27 depois de Fight_AwardPartExperience — RESPOSTA: NAO (decompile 0x080127DC, CONFIRMADO)
- Fight_AwardPartExperience (0x080127DC) escreve no registro do roster (0x03000BA0 + idx*0x40 ... ver PB.md) somente: +0x02 (XP restante/limiar), +0x05 (nivel), +0x08 (estagio), e copia 3 B de medaforca. Nao toca +0x20..+0x27.
- Ela NAO recalcula nem relê especialidades. A luta seguinte copia o roster inteiro para o bloco (Fight_CopyBotSlotRecords 0x08014EE8, 0x40 B) ao iniciar; ou seja, dentro da mesma luta, +0x20..+0x27 do bloco vem da copia inicial. Especialidade so muda no recrutamento (Medabot_SpawnFromRecord 0x08041D70, ja documentado).

## 2. Tela de escolha de ate 3 bots antes da Robattle — ABERTO
- Nao fechado. Xrefs por endereco/nome no Ghidra (`get_function_xrefs` de Fight_SetupBattleBotStates) vazios; sem a funcao chamadora a entrada/cursor/confirmacao/saida nao foram medidos.
- Pistas mantidas: PS.md (UpdateSelectedMedabotPreviewScreen 0x08013E30), grade ctx 0x03000440, time 0x030017A0; regra de PB.md: fi+0x41+i = indice do time (0..8) do bot i, 0 = vazio (Fight_UpdateBotActiveFlagCount conta != 0).

## Pendencias (para a proxima rodada)
1. Escritor de VALOR nao-zero em fi+0x41..43 (indice do time). Os 3 candidatos de TP.md sao zeros (ver item 1). Varrer escritas com base fi via fi+0x41 em outras formas (strb com registrador de valor, e loops de copia).
2. Funcao da tela de escolha (cursor, confirma, sai) e sua cena/GS.
3. Despachante da tabela 0x083AC560/568: sem literal da base na ROM; achar por GS (0x0300137C) ou por tabela-mae.
