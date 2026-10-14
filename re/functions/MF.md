# MF: Medaforce de ponta a ponta (GH-MF, 2026-10-14, em andamento)

Leitura estática (Ghidra + ROM, só leitura). ROM = 0x08000000 + offset. Bloco do bot = 0x03000590 + bot*0x100
(no Ghidra: `fight_information` 0x03001800). Dados em MD.md (medalhas) e DT.md (nomes/descrições).

## Status
- Em andamento. Pendências no fim.

## 1. Seleção (BR_14 / BR_47 / BR_49)
- `Fight_DispatchOnSubstateAA_BR_14_ActionSelectValidatePart` 0x08030A24 (HN9): +0xDD >= 4 = medaforce, slot = +0xDD-4.
- `Fight_DispatchOnSubstateAA_BR_40_CheckMedaforceRequirements` 0x08031614: nível F+0xA7 vs bot+5 (10/30/60) e bot+0xC1 == 0x50.
- `Fight_DispatchOnSubstateAA_BR_47_LoadMedaforceName` 0x080318CC: copia nome de gTextMedaforceNames[bot+0xA8].
- `Fight_DispatchOnSubstateAA_BR_49_StartMedaforceAction` 0x08031918: F+0xA8 = bot+0xA8+0x50 (script de ação).

## 2. Seleção: gate e slot (confirmado, decompile + literais)
`Fight_TrySelectMedaforceAction` 0x080155A8 `(bot, slot, aleatorio)`:
- Bloqueia (retorna 1) se `Fight_IsMedaforceSelectionBlockedByBattleState` 0x08015648 != 0, ou `F+0x58 != 0`, ou
  **`B[+0xC1] != 0x50`** ('P' = medaforça cheia; 0x50 = 80 = teto do medidor). Literal 0x080155D8 = fight_information 0x03001800; 0x080155DC = bloco do bot 0x03000590.
- Slot 0 exige `nível(B+5) >= 10`; slot 1 `> 0x1D` (>=30); slot 2 `> 0x3B` (>=60). Com `aleatorio != 0`, sorteia um slot via `Fight_RandomSlotFromTable16` e desce para os anteriores se o nível não alcançar (é a escolha da CPU/aleatória).
- Confirma o slot como índice de medaforça pelo nível, cortes fixos (ver MD.md §2).

`Fight_SelectMedaforceActionSource` 0x08015668 `(bot, slot)` (confirmado no decompile + literais):
1. `scratch(0x0300052E)[0..2] = medal_medaforce_list[B+1]` = 3 bytes: `0x083B7F8C + medalha*3` (a lista de 3 medaforças da medalha, uma por slot). Medalha = B[+1].
2. `B+0xA8..+0xAD = desc6[ scratch[slot] ]` com `desc6 = 0x083B7E54 + id*6` (6 bytes por medaforça).
3. `Fight_SelectActionSource(bot, slot + 4)`: o valor 4..6 é a fonte de ação de medaforça (ligado a Fight_SelectMedaforceSourceStep 0x08014DC8, +0xDD > 3).
- B+0xA8 = id da medaforça (índice de gTextMedaforceNames, usado por BR_47 e pelo script BR_49 em `B+0xA8+0x50`).

## 3. Tabelas (ROM, lidas por python -I, só leitura)
- `desc6` 0x083B7E54, 6 B por id (52 entradas). Byte 0 = id (= índice do nome). Exemplos (hex, 6 bytes):
  id0 `00 01 50 00 14 01`, id1 `01 00 3c 00 1e 00`, id 0x23 `23 06 28 00 1e 02`, id 0x32 `32 07 3c 00 0a 01`.
  Bytes 1..5: campos não identificados (provável: tipo/classe, custo, alcance, flags). Pendente.
- `medal_medaforce_list` 0x083B7F8C, 3 B por medalha (30 medalhas): medalha 0 = `00 01 23` (slots 0/1/2 -> ids 0, 1, 0x23); medalha 1 = `02 03 0b`; medalha 2 = `04 05 15`. Padrão: slots 0/1/2 costumam ser pares de ids vizinhos e o 3º é de outro grupo.
- Nomes: gTextMedaforceNames 0x083B74B8 (50 ptrs), descrições gTextMedaforceDesc 0x083B7580 (DT.md).

## 4. Seleção no fluxo de ação (confirmado)
- Chamadores de `Fight_TrySelectMedaforceAction` 0x080155A8: `Fight_TrySelectActionFromSpecialityCycle` 0x08015390 (call 0x08015400,
  valores 5..7 do ciclo de especialidade = slots de medaforça) e `Fight_SelectFallbackActionFromPriorityList` (call 0x08015488).
  Ou seja, medaforça entra pela mesma escolha de ação da CPU e do jogador (ciclo de especialidade + fallback).
- `Fight_DispatchOnSubstateAA_BR_49_StartMedaforceAction` 0x08031918 (decompile): `F+0xA8 = B[sel+0xA8] + 0x50`
  (confirmado: 0x50 = 'P'), `F+0x164 = 1`, zera `B+0xE0/+0xE1`, GM=9, GS=0. A ação executada é o **script de ação de
  id 0x50+medaforça**: o despachante de script usa o opcode 0x50..0x6A (AS2: wrappers 0x08034524+, um por opcode).
  Confirmar o caminho exato desse script (tabela de técnica 0x083B6100/0x083AF180) é pendência.

## 5. Medidor (B+0xC1, 0..0x50)
- Teto 0x50 (80) confirmado pelo gate acima e pelo BR_20. Somar: `Fight_AddMedaforceChargeFromAction` 0x0803628C (ver §6).
## 6. Carga do medidor (confirmado)
- `Fight_AddMedaforceChargeFromAction` 0x0803628C (decompile): `B[+0xC1] = min(0x50, B[+0xC1] + g)`, com `g = 0x14` (20);
  `g = 0x28` (40) se `B[+0xC0] & 8`. `B` = bloco do bot que age (`fight+0x87`).
- Chamadores: `Fight_DispatchOnSubstateAA_H_01_ChooseActionPath` 0x0801E3C0 (linhas 0x0801E3F0 e 0x0801E426), no
  resolver de ação normal (HA2: "resolução normal bem-sucedida"). Sem carga em falha/esquiva (provável).
- Consumo/zeragem: BM_0F 0x0802EAD8 zera o medidor do inimigo (técnica "drena medaforça"); BM_00 0x0802E44C lê +0xC1 do inimigo.
  Zerar o medidor ao executar a medaforça (BR_49) não aparece no trecho lido: pendente.

## 7. Níveis que liberam slots (confirmado; corrige a origem da tabela)
- `Fight_AwardPartExperience` 0x080127DC (decompile): ao subir o nível de uma peça do bot (`nível` no registro de 0x40 B
  do time), se o novo nível for 10 (`'\n'`), 30 (`0x1E`) ou 60 (`'<'`), grava no registro de medaforça do bot
  (fight+0x174 = 1, ids em fight+0x170/+0x172/+0x173). O id é `medal_medaforce_list[medalha][0|1|2]` (slot 0/1/2).
- **Correção ao MD.md §2 e ao texto antigo "tabela em 0x08012968"**: 0x08012968 é uma palavra de literal (pool) que
  aponta para `0x083B7F8C` (lido na ROM: `0x083B7F8C`). A tabela é a de 3 B por medalha, a mesma de §2.
- Nível da peça x medaforça: a liberação é por nível da PEÇA (registro +5 do time, `'\n'`, `0x1E`, `'<'`), e a seleção
  (§2) usa o nível do BOT (B+5). Ambos com os mesmos cortes 10/30/60.

## 8. Quem é a CPU
- `Fight_SelectFallbackActionFromPriorityList` (0x08015488, R0a): se `B[+0xC1] == 0x50` há sorteio; no ramo que sorteia
  chama `Fight_TrySelectMedaforceAction(bot, 0, 1)` (slot aleatório com fallback para slots menores). Se não sai nada,
  cai na lista de prioridade de peças. Ou seja: CPU com medidor cheio usa medaforça com probabilidade ~1/2 quando há
  slot liberado.
- Ciclo de especialidade (`Fight_TrySelectActionFromSpecialityCycle` 0x08015390): valores 5..7 tentam os slots de
  medaforça (mesma função).

## 9. Efeito de dano/cura/status (pendente)
- O efeito real de cada medaforça está no script de ação de id 0x50+id (BR_49). Ainda NÃO lido: tabela de scripts por
  técnica e o formato dos 6 bytes do registro (desc6, §3). Não há campo "custo" explícito: o custo parece ser o medidor
  cheio (0x50) mais o nível do bot. Hipótese (não confirmada).

## 10. Pendências
- Campos 1..5 de desc6 (0x083B7E54): tipo, alcance, custo?, flags. Ler o handler de script de id 0x50+.
- Efeito por medaforça (dano/cura/status) e a tabela de scripts (ligar opcode 0x50+id ao roteiro 0x083AF180/AS1-AS2).
- Zeragem do medidor ao executar a medaforça.
- Nenhum nome novo foi aplicado no Ghidra nesta rodada: as funções-chave já tinham nome; nenhuma renomeação
  confirmada ainda sem o efeito.
