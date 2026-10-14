# SS2: fechamento das pendências do SS.md (Robattle, Metabee EU)

Data: 2026-10-14 (GH-SS2). Ghidra (IWRAM) + decompile + desmontagem, sem emulador. Não salvo no Ghidra.
Base: fight = 0x03001800 (struct de luta); bloco do bot = 0x03000590 + bot*0x100 (offsets do Ghidra).
Confiança: C = confirmado no decompile/desmontagem lido aqui; P = provável; A = aberto.

## 0. Ids de status = ids de técnica (C)

O id gravado em bot+0xC9 é o id de técnica que causou o efeito, não um enum separado:
- `Fight_DispatchOnSubstateAA_I_07_ApplyPartStateEffectAtXY` 0x0801FA24 passa como tipo o byte de técnica da peça
  (`bot*0x100 + k*0x18 + 0x49`, k = fi+0xA7). O tipo 0x2E/0x2F entra por aqui.
- A tabela de handlers de AV (0x08028850) é indexada por `fi+0xA8 - 0x0C` (técnica do atacante).
- Os tipos fixos dos handlers de AV (0x0D, 0x10, 0x12, 0x13, 0x2C) são as técnicas 13, 16, 18, 19, 44 do gTechniqueNames.
- Nomes 46 = No Escape (0x2E) e 47 = No Defense (0x2F) batem com a tabela de nomes (DT3).
- Ressalva: 0x0D..0x2F também são faixas de técnica. Um status não tem enum próprio: quem decide é a técnica.

## 1. Quem lê a máscara de proteção bot+0xCF (pergunta 1)

Leitor único: `FightPreview_IsPartTypeFlagSet` 0x08036D64 (lado, tipo) (C). Lê `bot*0x100 + base + 0xCF`, com o bit:
0x0D/0x0E = 0x01; 0x0F/0x10 = 0x08; 0x11/0x12 = 0x02; 0x13/0x14 = 0x04 (outros tipos: retorna 0).
O R2.md dizia "chamada por: prévia". Errado: os chamadores de combate são (C):

| Chamador | Endereço | Papel |
|---|---|---|
| `Fight_DispatchOnSubstateAA_AV_ApplyStatusToOpponents` | 0x080287DC (handlers 0x080288D4, 0x08028914, 0x08028950, 0x0802898C) | bloqueia o status novo no alvo |
| `Fight_DispatchOnSubstateAA_BE_RollRandomStatus` | 0x0802B550 | sorteio por slot k (rand&7) |
| `Fight_DispatchOnSubstateAA_I_00_ClassifyPartStateJump2` | 0x0801F51A | não aberto nesta rodada (A) |

Regra de AV (C): para cada um dos 3 slots do lado alvo, com bot vivo (+0x40 != 0, +0x59 != 0) e sem técnica 16 (`Fight_HasNoTechnique16Part`):
- 0x0D (Bug/Virus) com bit 0x01 ligado no alvo: não aplica. Sem bit: `fi+0xD2+k = 1` e `SetPending(alvo, 0x0D, fi+0xE8, fi+0xEC)`.
- 0x10 (Freeze) bit 0x08; 0x12 (Hold) bit 0x02; 0x13 (Fire) bit 0x04: mesmo padrão.
- 0x2C (Confusion): **sem teste de máscara** (C).

Sorteio de BE (C): `fi+0xD6+2k` (halfword) recebe rand&7. 0 -> testa 0x0D, 1 -> 0x11, 2 -> 0x13, 3 -> 0x10. Se o alvo tem o bit, grava 5 (bloqueado) e
o `snd_sfx_request(0x5B)` e o ícone de UI saem depois. Os 4 `SetPending` de BE (xrefs) não tiveram os tipos conferidos (P).

Escrita da máscara (SS.md §3, já conhecida e reconfirmada): `..AW_09_ApplyStatusGuardAndCure` 0x08029354 liga em bot+0xCF
0x01/0x08/0x02/0x04 conforme a variante (AW_00: 0x0D/0x0F/0x11/0x13 -> 0..3) e zera o par de status.

Resposta: quem bloqueia um status novo é AV (4 handlers) e BE (sorteio). Não achei leitura direta de +0xCF fora desse helper,
mas não varri todas as funções (A para a garantia total).

## 2. Aplicador e efeito de 0x2E (No Escape) e 0x2F (No Defense) (pergunta 2)

Aplicador (C): `I_07` 0x0801FA24 é a única rota que aceita técnica arbitrária. Passa tipo = técnica da peça, `p3 = fi+0xE8`, `p4 = fi+0xEC`.
A chamada efetiva dos handlers de AV não inclui 0x2E/0x2F. Portanto 0x2E/0x2F = técnicas de peça que caem em I_07 (P: a checagem final
do roteiro não foi lida aqui).

Registro (C, `Fight_SetPendingPartStatus` 0x08036E64, ramo "outros", que inclui 0x2B..0x2F): `ca = fi[0x8B]` (sem +1),
`cb = clamp(p3)` (duração 1..99), `cc = clamp(p4)`.

Efeito (C, `Fight_ApplyPendingEffectStatusFlags` 0x08033C20): alvo = bot fi+0x88:
- c9 == 0x2E: `bot+0xBC |= 0x04` (bypass de esquiva).
- c9 == 0x2F: `bot+0xBC |= 0x08` (bypass de defesa).
- c9 == 0x0F/0x10: `bot+0xBC |= 0x0C`.
- Ator (fi+0x87): `bot+0xBC & 1 -> |= 4`; `& 2 -> |= 8`.
Sem xref no Ghidra para 0x08033C20 (chamada indireta; quem chama = A).

Leitores (C):
- `Fight_ApplyEvasionReductionToRos` 0x08038F84: se o defensor (fi+0x88) tem `+0xBC & 0x04`, pula o cálculo de esquiva (ROS = 0 no caminho do bypass).
  Também liga `+0xBC |= 0x10` no defensor quando esquiva (C).
- `Fight_ApplyDefenseReductionToRos` 0x08039068: se o defensor tem `+0xBC & 0x08`, pula a redução de defesa.

Duração de 0x2E/0x2F = `cb = clamp(fi+0xE8)` (C). A contagem usa o tick comum (dec. 4/tick, SS.md §1). Quem limpa os bits 0x04/0x08 de +0xBC: A.

## 3. Scout, Conceal, Traps, Charges: onde guardam estado (pergunta 3)

Confirmado que não são status pendentes: nunca passam por `SetPending` (C).

- **Scout (36)**: bônus de acerto. `E8 += F.rec[F.89].+0x28` (DM.md:57, registro de 8 B em fi+0x28). `fi+0x89` = índice de fase (C) de
  `Fight_UpdatePhaseFromCounter` 0x0801A640: `fi+0x87 = fi+0x45 - 1`; `fi+0x89 = (fi+0x45-1)/3`; `fi+0x8A = fi+0x89 ^ 1`.
  Ou seja, o registro de suporte é trocado a cada 3 passos do contador. Quem escreve o registro fi+0x28: A (não achado).
  Duração: 1 fase (P). Além disso, `Z_0F` 0x08023414 grava só fi+0xA6 = 0x1C (mensagem de escoteiro), C.
- **Conceal (37)**: `AA_0B` 0x08023718 grava só fi+0xA6 = 0x1D (mensagem "Conceal sobe"), C. Estado de esquiva: nenhum achado (A).
- **Traps (10/11)**: `ActionFamily_TrapFamily_Handler` 0x08030608 (HN5). Estado da armadilha não lido aqui (A).
- **Charges (38-40, Boost/Rapid/Accel)**: `Fight_DispatchOnSubstateAA_W_07_ApplyActorChargeBuff` 0x08022748 faz `bot+0xBD |= 0x08`
  no ator e grava fi+0xDF = técnica da peça (HN1, C). A ligação exata de 38/39/40 a esse efeito = P. Duração: A.
  O bit 0x08 é lido pelo E00 (candidato a apoio aliado) e pelo ícone de peça (ver §5). O 0x2B da tabela de mensagens é "carga ultra rápida".

## 4. +0xDD x +0xDE (pergunta 4): não é conflito, são duas structs (C)

- **Bloco do bot (0x03000590 + bot*0x100)**: `+0xDD` = peça escolhida 1..3 (DF.md, escrito por `Fight_SelectActionSource` 0x08015564).
  `+0xDE` = índice 1-based da peça usado pelo ícone `Fight_DrawPartStatusIcon` 0x08019B40 (armadura lida em
  `(bot+0xDE - 1)*0x18 + bot + 0x59`), `+0xDF` = tipo da ação (ícone).
- **Struct de luta (0x03001800)**: `E00` `Fight_FindAllyFollowupActionSource` 0x0801D5A8 grava `fi+0xDE` (1-based do aliado) e `fi+0xDF`
  (= `ally+0xDF`), e `fi+0xAA += 1` ou `fi+0xB5 = 1`. Lido no decompile.
- Quem grava `bot+0xDE`: A (não é E00). O SS.md dizia "+0xDE índice da peça do ícone" e isso está certo para o bloco do bot.
  Falta só o escritor de bot+0xDE.

## 5. +0xBF e os usos de +0xBD bit 0x10 (pergunta 5)

Bit 0x10 de +0xBD (C):
- Escritor: `Fight_DispatchOnSubstateAA_AM_0A_ClearSlotSetPartDisplayValue` 0x080263BC (HA8.md): liga `bot_ativo+0xBD |= 0x10`,
  copia técnica da peça para `+0xDF`, zera `+0xD8` e faz `+0xAA += 1`.
- Leitor: `Fight_DrawPartStatusIcon` 0x08019B40. Com 0x10 e sem 0x08: ícone ESTÁTICO (`Slot_SetupFromRecordList`). Com 0x08: ícone por tipo `+0xDF`
  (com `Anim_RunSlotScript` se o sprite já é o mesmo). Nos dois casos exige armadura da peça (`bot+0xDE`) != 0. Slot 0x48+3*bot.
- Não é estado de combate: é o modo de exibição (C).

Bit 0x08 de +0xBD (para comparar): escrito por W_07 (HN1), lido pelo E00 (ally candidato) e pelo ícone modo A.

+0xBF (P, a partir do R2.md e HA6.md; decompile desta rodada: não):
- `0x080392D8` (R2): afinidade óptica grava +0xBF = 2, bomba = 3, gravidade = 4.
- HA6 escreve +0xBF (`0x03000590 + bot*0x100 + 0xBF`), e R1 (0x0801A6..) lê "bot +0x40 != 0 e +0xBF != 0" (scan de 6 bots).
- Resposta provável: +0xBF = código de afinidade do ataque (2 óptico, 3 bomba, 4 gravidade) do bot, sobrescrito por cada ação.

## Saída: o que mudou no SS.md

- §3 "Leitor da máscara +0xCF: A" passa a ser: `FightPreview_IsPartTypeFlagSet` 0x08036D64 (chamadores AV/BE/I_00).
- §5 "Quem usa 0x10 ainda não separado" passa a ser: 0x10 = modo estático do ícone (escritor AM_0A 0x080263BC).
- §7 pendências 1, 2 e 4 foram resolvidas (2 com o efeito por bit; 4 com a diferença de struct). 3 e 5 seguem abertas.

## 6. Em aberto
1. Escritor de bot+0xDE (a peça do ícone, §4).
2. Escritor do registro fi+0x28 (Scout) e a ligação 38/39/40 com +0xBD bit 0x08 (§3).
3. Estado da armadilha (TrapFamily_Handler 0x08030608) e duração de Conceal.
4. Quem limpa +0xBC bits 0x04/0x08 (§2) e o caller de 0x08033C20.
5. Leitura de I_00 (0x0801F51A) e dos tipos de BE para SetPending.

Renomeações no Ghidra (não salvas): 0x080288C8 (AV_0D), 0x08028908 (AV_10), 0x08028944 (AV_12), 0x08028980 (AV_13), 0x080289BC (AV_2C).
Símbolos: `SS2_simbolos.csv`.
