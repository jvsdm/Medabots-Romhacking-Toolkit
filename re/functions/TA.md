# TA: onde a técnica muda o comportamento (GH-TA, 2026-10-14, PARCIAL)

Leitura: ROM original (`python -I`, só leitura) + Ghidra (decompile e get_xrefs_to; Ghidra não salvo, não fechado).
Símbolos: `TA_simbolos.csv`. Base do que já está em `TQ.md`, `TQ2.md`, `DM.md` (não repetido aqui).

## Dois bytes diferentes com o mesmo "+0xA8" (importante)

- **F+0xA8** = fight `0x03001800` + 0xA8 (`0x030018A8`): id do roteiro (técnica 0..59, medaforça 0x50+id).
  Gravado só por dois handlers de seleção: `Fight_DispatchOnSubstateAA_BR_14_ActionSelectValidatePart` 0x08030C64
  (escrita em 0x08030C68) e `Fight_DispatchOnSubstateAA_BR_49_StartMedaforceAction` 0x080318DE (escrita em 0x08031940).
- **B+0xA8** = bot `0x03000590 + bot*0x100` + 0xA8 (`0x03000638` para bot 0; `bot*0x100` por bot; `bot = fight+0x87`
  nos leitores de efeito). É a **técnica ativa do bot** para os efeitos: gravada por `ActionScript_Op1B_LoadPaletteAndSetTechniqueId`
  (funcao que contem 0x080340F8) e lida pelos despachantes de efeito/objeto abaixo. Valor 0x00 a 0x3B e medaforça 0x50+.

## 1. Leitores de F+0xA8 (0x030018A8) — Ghidra get_xrefs_to, confirmado

| leitor | endereço | papel | decisão por técnica |
|---|---|---|---|
| Fight_LookupActionScriptByte | 0x08034E94 | `ROM[gTechniqueActionScriptPtrTable[F+0xA8] + F+0xBC]` | escolhe o roteiro (só troca o ponto de entrada) |
| Fight_AdvanceActionScriptCursor | 0x08034F08 | avança cursor | nenhuma |
| Fight_ReadActionScriptInlineArgs | 0x08034EB8 | copia argumentos do opcode | nenhuma |
| Fight_WaitForActionScriptByteMatch (opcode 0x02) | 0x08033924 | salto por byte | nenhuma (usa F+0xA8 só como cursor) |
| Fight_WaitForActionScriptChoiceByteMatch (opcode 0x03) | 0x0803399C | escolha de rótulo por fight+0xAB | ver §3 |

Nenhum destes faz decisão de efeito. A diferença de comportamento entre técnicas NÃO passa por F+0xA8 nos leitores
de roteiro; passa pelos leitores de B+0xA8 (§2).

## 2. Leitores de B+0xA8 (técnica ativa do bot) — onde o efeito muda

Confirmados por decompile (Ghidra, nomes do Ghidra):

| função | endereço | decisão (confirmada no decompile) |
|---|---|---|
| Fight_DispatchOnSubstateAA_AS_LoadIconAtActor | 0x08027A04 | `v = B+0xA8 - 4`; se `v < 5` chama jump table[v] (5 variantes de ícone/estado); senão: som 0x33, carga de gráficos, sprite com `B+0xA8`-derivado. |
| Fight_DispatchOnSubstateAA_AV_LoadEffectGfx | 0x08028512 | `v = B+0xA8 - 0x0C`; se `v > 0x1C` efeito padrão (CpuFastSet, slot 0x21); senão jump table[v] (29 variantes de efeito por técnica 12..40). |
| Fight_DispatchOnSubstateAA_BE_LoadEffectGfx | 0x0802B0FC | carga do efeito; `B+0xA8 == 0x1E` escolhe a sub-variante (paleta/sprite alt); som 0x4C. |
| Fight_DispatchOnSubstateAA_BE_SelectTargets | 0x0802B1DA | `B+0xA8 == 0x1E` (no ator): marca alvo com perna 'd' (flag `+0xD2 = 1`); senão marca perna 'j' (`+0xD2 = 2`). |
| ObjScript_Op1E_SpawnEffectSlot23PartPal | 0x0803A4BA | se `B+0xDD < 4` cor da peça `B+0xA7*0x18+0x49`; senão paleta `B+0xA8 + 0x50` (medaforça). Gráfico/paleta, não dano. |

Pelos nomes do Ghidra, também leem B+0xA8 (não decompilados nesta rodada, PENDENTE): AS_SelectVariantByTechnique
0x08027974, AS_SetStepCodeByTechnique 0x08027EEC, AS_SetStatusFlagByTechnique 0x08027E30, AV_SelectVariantByTechnique
0x0802843C, AV_ApplyStatusToOpponents 0x08028834, AW_00_SelectStatusTypeFromTechnique 0x08028C78, BF_SelectAlliesAndStat
0x0802B868, BE_SetResultMessage 0x0802B7BC, G_03_BuildBattleBackground 0x0801E1B4, BR_47_LoadMedaforceName 0x080318E0,
BR_49_StartMedaforceAction 0x08031938, ObjScript_Op0D/Op1F/Op2C/Op32/Op39 (0x080398EC, 0x0803A60C, 0x0803AE3C, 0x0803B3B4,
0x0803B794). Na lista: "pelo nome" e xref [DATA]; a decisão de cada um não foi lida.

Valor de referência: 0x1E (30) em BE_* não é o id de Anti-Air (34) nem de Anti-Sea (35) (TQ.md §4); tratar como
código de variante (chute) até o decompile de BE_SelectTargets ser cruzado com os ids de `TQ.md`.

## 3. Gravadores de fight+0xAB (0x030018AB): resultado da ação — provável

Ghidra não tem literal `0x030018AB` na ROM (acesso é base+0xAB), então a lista vem do get_xrefs_to.
Valores lidos da instrução (mov antes do strb; `provável` onde o nome dá o valor):

| gravador | endereço (escrita) | valor observado | leitura |
|---|---|---|---|
| Fight_DispatchOnSubstateAA_A_2D_EndResult0 | 0x0801D2BA | 0 | normal |
| Fight_DispatchOnSubstateAA_A_2E_EndResult1 | 0x0801D2D0 | 1 | resultado 1 |
| Fight_DispatchOnSubstateAA_A_2F_EndResult2 | 0x0801D2E6 | 2 | resultado 2 |
| Fight_DispatchOnSubstateAA_B_03_EndResult1 | 0x0801D378 | 1 | resultado 1 |
| Fight_DispatchOnSubstateAA_C_03_EndResult1 | 0x0801D404 | 1 | resultado 1 |
| Fight_DispatchOnSubstateAA_L_07_SlideUntilMarker0x28ThenTimer60 | 0x080202FE | (valor de registro) | — |
| Fight_DispatchOnSubstateAA_L_00/L_04/N_00/Q_00/O_00/AN_00/AK_00 | 0x08020098..0x08025F94 | 1 ou 2 (mov) | condicional a flags de peça/bot |
| Fight_HandleDefenderFollowupFlags | 0x08038D92/0x08038E3C/0x08038E46 | 1 / 0 | pós-defesa |
| Fight_UpdateActionArgSelectorFromDefenderState | 0x080391AC/0x080391C0 | 1 | estado do defensor |

Leitores: `ActionScript_Op3F_CallAN_SceneExit` 0x0803465A e `Fight_WaitForActionScriptChoiceByteMatch` 0x080339AC
(opcode 0x03: escolhe o rótulo `args[fight+0xAB]`, `TQ2.md` §3b).

Valores: 0 = normal (fim sem ocorrência), 1 e 2 = resultados alternativos (ex.: 1 = dano/parte quebrada, 2 = penalidade/
troca); a semântica exata de 1 e 2 é **provável** (nomes EndResult1/EndResult2), não lida por decompile de cada um.

## 4. Nomes dos opcodes 0x15 e 0x16 (correção)

Tabela 0x083AF180 (entradas 0x15 em 0x083AF1D4, 0x16 em 0x083AF1D8) aponta para 0x08033F04 e 0x08033F64. Ghidra agora
já tem os nomes certos (confirmado hoje):
- 0x08033F04 = `ActionScript_Op15_MarkStateFromDamageBelowThreshold` (era "Opcode1D" em `TQ2.md` §3c)
- 0x08033F64 = `ActionScript_Op16_SetStateFromPartFlag1C` (era "Opcode1C")

## 5. Pendente

- Decompile dos 15 leitores de B+0xA8 listados em §2 (cada um dá a decisão por técnica: setup/efeito/som/dano).
- Confirmar em runtime (mGBA, rec_2) o valor de B+0xA8 para Sword (0) e Missile (6): o roteiro é igual (TQ2), então
  a diferença de efeito deve sair daqui.
- Quem grava B+0xA8 além de 0x1B (`ActionScript_Op1B_LoadPaletteAndSetTechniqueId`): não varrido.
- Semântica de fight+0xAB = 1/2 (§3) só por nome.
