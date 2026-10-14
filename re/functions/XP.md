# XP: experiência e nível das medalhas (GH-XP, 2026-10-14)

Leitura estática: Ghidra decompile + literais lidos da ROM original com `python -I` (só leitura). Nada testado no emulador.
Convenções: `B` = bloco do bot = 0x03000590 + slot*0x100 (slots 0..2 jogador, 3..5 inimigos); `fi` = 0x03001800;
registro de medalha `R` = 0x03000BE0 + n*0x40 (30 registros), n = `Slot_FindByIdPlus1(B+1)-1`.
Confiança: "confirmado" = decompile + literal; "provável" = padrão de uso; "aberto" = não fechado.

## 1. Quais bytes dos inimigos entram no XP (confirmado)

`Battle_SumPartBytesToCtx16C` 0x0801277C (decompile + literais: 0x080127D0 = 0x03000590, 0x080127D4 = 0x0382, 0x080127D8 = 0x0352):
- Para os bots inimigos 3..5 com `B+0xBA != 0` (ativo): soma `u16` de **B+0x52, B+0x6A, B+0x82, B+0x9A** em `fi+0x16C` (+=, não zera).
- Esses 4 bytes são o **campo +6 de cada registro de peça** (base `B + 0x4C + 0x18*k`, k = 0..3, k = 0 cabeça).
  Pelo MD.md o +6 é `power_or_evasion` (usado no ROS). Ou seja: **valor da peça, não nível**. O nível do inimigo (B+5) não entra.
- Confiança: confirmado (offsets e flag); o nome "power_or_evasion" é provável (MD.md).
- Quem zera `fi+0x16C` e quem chama `Battle_SumPartBytesToCtx16C`: aberto.

## 2. Como o XP vira nível (confirmado em `Fight_AwardPartExperience` 0x080127DC)

Para i = 0..2, se `B_i+0xBA != 0`:
- `R` = registro de medalha do bot (`B_i+1` indexa `gMedals`; o registro fica em 0x03000BE0).
- Se `R+5 != 'd'` (100), guarda do teto:
  - se `fi+0x16C < R+2`: `R+2 -= fi+0x16C` (R+2 = XP restante até o próximo nível).
  - senão: `R+5 += 1` (sobe **uma vez por chamada**) e `R+2 = (novo_nível + 1) * 100`.
- O pool `fi+0x16C` **não é reduzido** em nenhum ramo: o excedente de uma batalha se perde ao subir de nível.
- **Fórmula**: XP do nível L para L+1 = `(L+1)*100` (confirmado: `(R+5+1)*100` na subida e no recrutamento). Ex.: nível 5 precisa de 600.
- **Teto**: nível **100** (0x64 = `'d'`). No nível 99 ainda sobe para 100; no 100 não ganha mais. (Pergunta "teto 99?": é 100.)
- Cada bot ativo da equipe recebe o mesmo pool da batalha.
- Confiança: confirmado. Se derrota ou outro caminho chama `Fight_AwardPartExperience`: aberto (entrada 0x080127E8 não lida).

## 3. O que mais sobe com o nível

Confirmado:
- **Medaforça**: cortes fixos no código (MD.md §2): `Fight_TrySelectMedaforceAction` 0x080155A8 lê `B+5` (>= 10, >= 30, >= 60). A subida também dispara evento em 10/30/60 (§4).
- **Estágio** (`R+8`): `Stage_FromLevelThresholds` 0x080420C4 usa `gMedals[id]+3..+6` (0 = sem limite) e devolve 0..4. Se o estágio muda na subida, grava: `fi+0x16F+8i` = 1 (flag), `fi+0x170+8i` = id, `fi+0x171+8i` = estágio antigo, `fi+0x172+8i` = id, `fi+0x173+8i` = estágio novo; `R+8` = estágio novo.
- **Especialidades NÃO sobem na subida.** `Medabot_SpawnFromRecord` 0x08041D70 (recrutamento) calcula `R+0x20..+0x27` = `B_k + B_k*(nível/10)` (teto 100). `Fight_AwardPartExperience` não recalcula. Se a luta relê a especialidade do registro: aberto.
  - Correção: as especialidades estão em +0x20..+0x27 (8 valores), não em +8..+15. Os +8..+15 do registro são as 8 **bases** que vêm de `gMedals`.
- **Nível do bot na batalha** (`B+5`): alimenta a medaforça e o ROS do alvo (`Fight_AccumRosBase` 0x08035844, GH-DM).
- Nível de referência para recrutar (`Level_ComputeReferenceFromSlots` 0x0804206C, decompile): copia o nível de cada um dos 30 registros para um array u16 (0x02023000, passo 8 B), ordena decrescente (`Sort_Records8Descending`) e devolve `(1º maior + 4º maior)/2`, ou seja, o ponto médio entre o 1º e o 4º maior nível. Literais: 0x080420B4 = 0x02023000, 0x080420BC = 0x03000BE0, 0x080420C0 = 0x02023018 (= entrada 3).

## 4. Eventos de Medaforça em 10/30/60

- No decompile de `Fight_AwardPartExperience`: ao chegar a 10 (`'\n'`), 30 (`0x1E`) ou 60 (`'<'`): `fi+0x174+8i = 1` e `fi+0x175+8i = lista[slot]`, onde lista = 3 bytes em `0x083B7F8C + id*3` (slot 0/1/2 = 10/30/60).
- **Quem lê `fi+0x174` e mostra a mensagem: aberto.** Não achei leitor (acesso por base+offset, sem xref direto).
- Parcial: `FUN_08011490` 0x08011490, renomeado **`Fight_MedalStageUpSeqStep`** (nome provisório): lê a flag de **estágio** `fi+0x16F+8*cnt` com `cnt = fi+0x186` (0..2); se setada, grava 0x4E em 0x0300137C; quando `cnt == 3`, zera e grava 0x5B. Literais: 0x080114AC = fi, 0x080114B0 = 0x0300137C, 0x080114CC = 0x016F, 0x080114D0 = 0x0300137C. Confiança: confirmado no decompile. O que a tela do passo 0x4E mostra: aberto.
- FN1.md chutava "Msg4E"; pelo que foi lido, é valor de estado (0x0300137C), não código de mensagem.

## 5. O "+N" da tela

- `R+7` é copiado de `gMedals[id]+7` (bônus de compatibilidade) no recrutamento.
- No combate, `Fight_GetMedalPartCompatibilityBonus` 0x0803576C soma `B+7` (= `R+7`) ao ROS quando o atributo da peça casa com a medalha (MD.md).
- O "+N" da lista = `R+7` (medido no MB6 da pesquisa: mesmo N nas 4 peças). A linha do desenho que lê `R+7` não foi localizada (provável).

## 6. Registro de medalha 0x03000BE0 (0x40 B) — confirmado pelo decompile

| off | campo | nota |
|---|---|---|
| +0x00 | flag (bit 0x80 = ocupado) | recrutamento |
| +0x01 | id da medalha (índice de `gMedals`) | confirmado |
| +0x02 | XP restante (u16) | confirmado |
| +0x04 | = medalha+1 (atributo casado com a peça) | confirmado (cópia) |
| +0x05 | nível (0..100) | confirmado |
| +0x06 | = medalha+2 | confirmado (cópia) |
| +0x07 | = medalha+7 (bônus de compat., o "+N") | confirmado (cópia) |
| +0x08 | estágio (0..4) | confirmado |
| +0x20..+0x27 | especialidades (8) | confirmado (recrutamento) |

## Pendências
- Chamador de `Fight_AwardPartExperience` (vitória ou derrota) e quem zera `fi+0x16C`.
- Leitor de `fi+0x174` (medaforça 10/30/60) e a tela que mostra a mensagem.
- Significado do passo 0x4E/0x5B (0x0300137C).
- Se as especialidades são relidas no início da batalha do jogador.
