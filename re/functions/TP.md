# GH-TP: escolha do time antes da Robattle e especialidade do jogador (2026-10-14)

Status: PARCIAL. Leitura estatica (ROM EU, so leitura, python -I) + decompile do Ghidra onde havia funcao.
Nada gravado no Ghidra, nenhum rename novo (sem nome confirmado). Simbolos em `TP_simbolos.csv`; codigo sem funcao em `TP_lista.txt`.

## 1. Quem grava fi+0x41..0x43 (0x03001841..43) — PARCIAL
Varredura: literal 0x03001800 aparece em ~150 pools; acesso por base+offset nao tem literal proprio. Procurei o padrao
`adds rX,#0x41..0x43` seguido de STRB [rX,#0] na ROM inteira: 11 ocorrencias, 9 com base T (0x03000440, nao fi) e 4 sobre fi (desses, 3 distintas e 1 e so a 2a escrita de 0x080441EE).
- **0x080441EE (CONFIRMADO, decode a mao; sem funcao no Ghidra; prologo 0x080441B8 `push {r4,r5,lr}`)**: `r2 = fi; r2 += 0x41; strb r4,[r2]; r2++ ; strb r4,[r2]; r0 = fi+0x43; strb r4,[r0]`. r4 = 0 (movs r4,#0 em 0x080441BC, sem outra escrita). => **ZERA fi+0x41..0x43** (slots do time vazios). Escritor de limpeza, nao de escolha.
- **0x0806B8CE (PROVAVEL, sem funcao no Ghidra; prologo 0x0806B826 `push {r6,r7}`?)**: grava r3 em fi+0x41 (`ldr r0,=fi; adds r0,#0x41; strb r3,[r0]`), depois outras escritas por ponteiro computado. Valor de r3 e origem: aberto.
- **0x0806C7E4 (PROVAVEL, sem funcao no Ghidra; prologo 0x0806C694 `push {r4-r6,lr}`)**: grava r2 em fi+0x41 (`adds r0,#0x41; strb r2,[r0]`), depois laco com ponteiro. Aberto se e o escritor da escolha (3 slots).
- Os 3 slots de escolha (1o lider) NAO foram fechados: falta decodificar o valor de r3/r2 nos dois candidatos e achar a funcao chamadora (tela de escolha).

## 2. Tela de escolha de ate 3 bots antes da Robattle — ABERTO
Nao fechado nesta rodada. Pistas: UpdateSelectedMedabotPreviewScreen 0x08013E30 (PS.md), grade ctx 0x03000440, time 0x030017A0.
Os escritores de fi+0x41 acima (0x0806B8CE, 0x0806C7E4) sao os candidatos para a funcao de escolha; falta confirmar entrada/saida.

## 3. Nivel e especialidades do jogador entram no bloco? — RESPOSTA PARCIAL (provavel forte)
- `Fight_CopyBotSlotRecords` 0x08014EE8 (PB.md, CONFIRMADO) copia **0x40 B do registro de medalha do roster** (0x03000BA0 + slot*0x40, sentido roster->bloco) para bot+0x00..0x3F.
- O roster ja tem +0x20..+0x27 (especialidades, gravadas no recrutamento por Medabot_SpawnFromRecord 0x08041D70, XP.md §3/§6). Logo o bloco recebe +0x20..+0x27 **por essa copia**, sem recalculo na luta.
- Nivel (+0x05) vem de roster+5 pela mesma copia (XP.md: roster+5 = nivel).
- Confirmado: Fight_RefreshBotPartRecords 0x08014EC8 NAO chama Medabot_SpawnFromRecord. Especialidade nao e recalculada na luta.
- Pendente: se a luta relê a especialidade do roster depois de Fight_AwardPartExperience (XP nao atualiza +0x20..+0x27).

## 4. Chamador de Fight_RefreshBotPartRecords (0x08004A2A) — CONFIRMADO
- BL 0x08004A2A -> 0x08014EC8 confirmado (varredura de BL na ROM; xref do Ghidra tambem: `From 08004a2a`).
- Funcao que contem o BL: **0x08004A18** (prologo `push {lr}`, pool de 0x03000440/0x0300137C). Nao existe no Ghidra (so e alcançada por tabela de ponteiros).
- Sequencia (BL decodificados): 0x08004E74 -> 0x080054C0 -> 0x08014E7C (Fight_ClearBattleBuffer) -> 0x08014E94 (Fight_UpdateBotActiveFlagCount) -> 0x08014EC8 (Fight_RefreshBotPartRecords); depois `*0x0300137C += 1` (GS++) e `pop {pc}`.
- E um passo (GS) de uma tabela de ponteiros: 0x08004A19 esta em 0x083AC568 (indice 0 dessa linha; entradas 0x080049C9 em 0x083AC560, 0x080049ED em 0x083AC564; 0x08004A41, 0x08004A79, 0x08004AF9... nas seguintes). O despachante que le 0x083AC560 nao foi achado (nao ha literal para essa base).
- Ou seja: a preparacao de luta de jogador (limpar buffer, contar bots ativos, montar bots) acontece nesse passo. Fight_SetupBattleBotStates 0x080129F2 tambem chama RefreshBotPartRecords (xref 0x080129EE): dois caminhos.
- Confianca: CONFIRMADO no caminho (bytes + xref); "cena/menu" de 0x083AC560: ABERTO.

## Pendencias
1. Valor de r3 (0x0806B8CE) e r2 (0x0806C7E4) e a funcao de escolha dos 3 bots (secao 1-2).
2. Despachante da tabela 0x083AC560 (qual gamestate_menu/cena) e o passo 0x08004A41 etc.
3. Se a luta relê +0x20..+0x27 do roster apos a luta.
