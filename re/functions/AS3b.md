# AS3b: opcodes do action script 0x1B..0x35 (Medabots Metabee EU) — GH-AS3b (2026-10-14)

Tabela `gActionScriptOpcodeHandlerTable` 0x083AF180 (4 B/entrada, thumb). Ponteiros lidos na ROM original com `python -I`
(`ponteiro & ~1` = função). Todos os 27 handlers já são função `FUN_` no Ghidra (sync feito). Decompile + literais
(`DAT_` lidos na ROM) conferidos. Renomeados no Ghidra com `ActionScript_OpXX_<Nome>`.

## Regras comuns (confirmadas)

- **Argumentos**: LEN (0x083C6EB8) = 0 para 0x1B..0x35; nenhum handler deste lote chama leitor inline de argumentos.
  Cursor avança `+1` (`Fight_AdvanceActionScriptCursor` 0x08034F08 faz `F.BC += 1 + LEN[op]`).
- **Dois tipos de handler**:
  - *Instantâneo*: não lê `+0xB5`, avança e o laço do roteiro (`Fight_RunActionScriptLoop` 0x08033840) segue para o próximo opcode no mesmo quadro. Não encerra o passo.
  - *Espera*: avança só quando `+0xB5 != 0`. Enquanto isso o opcode fica no cursor e é relido; o flag encerra o passo.
- **Lado (preview)**: quase todos os wrappers com `FightPreview_UpdateSingleLayerSideSprites` + `AdvanceLeft/RightSideSequenceStep`
  gravam o resultado em `fight+0x15F` (lado esquerdo) e `fight+0x160` (direito). Confirmado pelo literal (0x15F).
- **Estado**: `fight` = 0x03001800; bot = 0x03000590 + bot*0x100; peça k = bot+0x48+0x18*k (ver DM.md §0).
- **Byte 0x03000570** (= 0x03000440+0x130, base de IWRAM não identificada): zerado ao concluir 0x1C e 0x20. Efeito: aberto.

## Tabela por opcode

| op | handler | Ghidra (novo nome) | o que faz | encerra passo (+0xB5)? | avança cursor? | confiança |
|----|---------|--------------------|-----------|------------------------|----------------|-----------|
| 0x1B | 0x08034074 | ActionScript_Op1B_LoadPaletteAndSetTechniqueId | Limpa 40 entradas de OAM; `CpuFastSet(ROM 0x084E4658 -> 0x05000380, 8 words)` = copia 16 cores para a paleta OBJ (banco 14). Zera `fight+0x100` (u16) e `+0xFB..+0xFE`. Grava `fight+0xF9`: técnica da peça `fight[0xA7]` (bot+0xDD < 4) ou `'P' + bot+0xA8` (senão). Põe `0x03002C30+0x69=1` e `+0xA9=1`. Efeito visual (ícone/paleta da técnica no combate). | não (instantâneo) | sim | confirmado (código); efeito provável |
| 0x1C | 0x08034120 | ActionScript_Op1C_RunObjectScriptUntilYield | Lados (0x15F/0x160). Zera `+0xB5`, chama `ObjectScript_RunUntilYield` (roteiro de objeto, HO1/HO2). Se `+0xB5 != 0`: zera byte 0x03000570 e avança; senão repete (espera). | sim (quando o roteiro de objeto termina) | só ao concluir | confirmado (código); efeito provável |
| 0x1D | 0x08034170 | ActionScript_Op1D_ApplyDamageToTargetPart | Lados. `fight+0xD0 = fight[0x8B]` (índice da peça). `Fight_ApplyDamageToPart(alvo=fight[0x88], peça, dano=(s16)fight+0xEC)` (0x08034F68, DM.md §4). Aplica o dano acumulado (EC) na peça alvo. | não (instantâneo) | sim | confirmado |
| 0x1E | 0x080341C0 | ActionScript_Op1E_CallSubstateH | Lados (0x15F/0x160). `Fight_DispatchOnSubstateAA_H` (0x0801E2D8, despachante por +0xAA; passos HA3: sprite/efeito no bot ativo, números, SFX). | sim (quando `+0xB5 != 0`) | só ao concluir | confirmado (wrapper); efeito do despachante provável |

## Notas do lote

- O nome do Toolkit `Fight_HandleActionScriptOpcode1D` (0x08033F04) e `..1C` (0x08033F64) em `mtbe.csv` **não são** os
  ponteiros da tabela (1C = 0x08034120, 1D = 0x08034170). Não renomeados; o `mtbe.csv` pode estar desatualizado.
- **Correção para DM.md §5 e AS2.md**: a tabela LEN real tem `LEN[0x02]=1`, `LEN[0x03]=4`, `LEN[0x13]=0`
  (bytes `00 00 01 04 00 ...` a partir de 0x083C6EB8). O texto "0x13 e 0x03 = 1 byte" está errado.

| 0x1F | 0x08034200 | ActionScript_Op1F_CallSubstateI | Lados (0x15F/0x160). `Fight_DispatchOnSubstateAA_I` (0x0801F484; HA3: classifica estado da peça, sprite de estado no slot 11, efeito de estado em XY). | sim (`+0xB5`) | só ao concluir | confirmado (wrapper); efeito provável |
| 0x20 | 0x08034240 | ActionScript_Op20_CallSubstateJ_ClearObjectWait | `Fight_DispatchOnSubstateAA_J` (0x0801FA40; HA3: prévia dos lados, avanço das sequências, contador de passos). Ao concluir zera byte 0x03000570 e avança. | sim | só ao concluir | confirmado (wrapper); efeito provável |
| 0x21 | 0x0803426C | ActionScript_Op21_CallSubstateK | `Fight_DispatchOnSubstateAA_K` (0x0801FDEC; HA3: sprite 21 do bot, carregamento de gfx, mensagem 14). Sem lados. | sim | só ao concluir | confirmado (wrapper); efeito provável |
| 0x22 | 0x08034288 | ActionScript_Op22_CallSubstateL | Lados. `Fight_DispatchOnSubstateAA_L` (0x08020050; HA3: slot 11 (carrega, desliza, limpa), commit de ação se estado 3, ramo em flag F6). | sim | só ao concluir | confirmado (wrapper); efeito provável |

| 0x23 | 0x080342C8 | ActionScript_Op23_CallSubstateM | `Fight_DispatchOnSubstateAA_M` (0x0802057C; HA4: M_00 cria objeto no alvo, M_01 espera spawn, M_02 gfx e posição do slot, M_03 espera slot, **M_04 aplica dano em peça**, M_05 efeito). Sem lados. | sim | só ao concluir | confirmado (wrapper); efeito do despachante provável |
| 0x24 | 0x080342E4 | ActionScript_Op24_CallSubstateN | `Fight_DispatchOnSubstateAA_N` (0x080207A4; HA4: N_00 checa flag do bot → mensagem ou avança). Sem lados. | sim | só ao concluir | confirmado (wrapper); efeito provável |
| 0x25 | 0x08034300 | ActionScript_Op25_CallSubstateO | `Fight_DispatchOnSubstateAA_O` (0x0802081C; HA4: O_00 aplica penalidade ou mensagem). Sem lados. | sim | só ao concluir | confirmado (wrapper); efeito provável |
| 0x26 | 0x0803431C | ActionScript_Op26_CallSubstateP | Lados. `Fight_DispatchOnSubstateAA_P` (0x080208C0; HA4: P_01 limpa tiles e aplica reparo, P_0B repara peça ou pula, P_11 salta para 02). | sim | só ao concluir | confirmado (wrapper); efeito provável |

| 0x27 | 0x0803435C | ActionScript_Op27_CallSubstateQ | `Fight_DispatchOnSubstateAA_Q` (0x080209B0; HA4: Q_00 aplica penalidade se a flag está limpa, ou mensagem). Sem lados. | sim | só ao concluir | confirmado (wrapper); efeito provável |
| 0x28 | 0x08034378 | ActionScript_Op28_CallSubstateR | Lados. `Fight_DispatchOnSubstateAA_R` (0x08020A68; HA4: R_01 limpa tiles e conta divisões, R_0C divide ou pula, R_08 carrega ícones de peça e salta, R_12 salta para 02). | sim | só ao concluir | confirmado (wrapper); efeito provável |
| 0x29 | 0x080343B8 | ActionScript_Op29_CallSubstateS | `Fight_DispatchOnSubstateAA_S` (0x08020CFC). Sem passos documentados nos HA*; efeito **pendente**. Sem lados. | sim | só ao concluir | confirmado (wrapper); efeito pendente |
| 0x2A | 0x080343D4 | ActionScript_Op2A_CallSubstateT | `Fight_DispatchOnSubstateAA_T` (0x080218BC; HA5: T_00 ramo pelo registro de dano do alvo, T_09..T_0C prepara slots e posições e espera, T_0D espera fim do roteiro, T_0F..T_11 gráficos e slots, T_12 escreve passo 0x25). Sem lados. | sim | só ao concluir | confirmado (wrapper); efeito provável |

| 0x2B | 0x080343F0 | ActionScript_Op2B_CallSubstateU | `Fight_DispatchOnSubstateAA_U` (0x08021E04, mtbe). Sem passos documentados; efeito **pendente**. Sem lados. | sim | só ao concluir | confirmado (wrapper); efeito pendente |
| 0x2C | 0x0803440C | ActionScript_Op2C_CallSubstateV | `Fight_DispatchOnSubstateAA_V` (0x08022154, mtbe). Sem passos documentados; efeito **pendente**. Sem lados. | sim | só ao concluir | confirmado (wrapper); efeito pendente |
| 0x2D | 0x08034428 | ActionScript_Op2D_CallSubstateW | `Fight_DispatchOnSubstateAA_W` (0x080223BC, mtbe). Sem passos documentados; efeito **pendente**. Sem lados. | sim | só ao concluir | confirmado (wrapper); efeito pendente |
| 0x2E | 0x08034444 | ActionScript_Op2E_CallSubstateX | `Fight_DispatchOnSubstateAA_X` (0x080227D8, mtbe). Sem passos documentados; efeito **pendente**. Sem lados. | sim | só ao concluir | confirmado (wrapper); efeito pendente |

| 0x2F | 0x08034460 | ActionScript_Op2F_CallSubstateY | `Fight_DispatchOnSubstateAA_Y` (0x08022CA8, mtbe). Sem passos documentados; efeito **pendente**. Sem lados. | sim | só ao concluir | confirmado (wrapper); efeito pendente |
| 0x30 | 0x0803447C | ActionScript_Op30_CallSubstateZ | `Fight_DispatchOnSubstateAA_Z` (0x08022FF0; HA6: contador de tick, slots de aliado a partir dos bots, decaimento de slots, gauge +160 (Z_0E), escreve passo 0x1C (Z_0F)). Sem lados. | sim | só ao concluir | confirmado (wrapper); efeito provável |
| 0x31 | 0x08034498 | ActionScript_Op31_CallSubstateAA | `Fight_DispatchOnSubstateAA_AA` (0x0802342C; HA6: AA_00 **cálculo de ROS da peça** (RosCalcPart), AA_04 gfx e SFX, AA_0A gauge +120, AA_0B escreve passo 0x1D). Sem lados. | sim | só ao concluir | confirmado (wrapper); efeito provável |
| 0x32 | 0x080344B4 | ActionScript_Op32_CallSubstateAB | `Fight_DispatchOnSubstateAA_AB` (0x08023730; HA6: AB_00 cálculo de ROS sem técnica (RosCalcPartNoTech16), AB_08 gfx e slot, AB_09 espera anim/timer, AB_0A move slot ao alvo, AB_0C espera flag do roteiro). Sem lados. | sim | só ao concluir | confirmado (wrapper); efeito provável |

| 0x33 | 0x080344D0 | ActionScript_Op33_CallSubstateAC | `Fight_DispatchOnSubstateAA_AC` (0x08023AC8; passo AC_0F escreve o código de efeito 0x2B, A6: "carga ultra rápida"). Demais passos sem doc. Sem lados. | sim | só ao concluir | confirmado (wrapper); efeito parcial (provável) |
| 0x34 | 0x080344EC | ActionScript_Op34_CallSubstateAD | `Fight_DispatchOnSubstateAA_AD` (0x08023C90; passo AD_16 escreve o código de efeito 0x2D, A6: "absorveu tempo do inimigo"). Demais passos sem doc. Sem lados. | sim | só ao concluir | confirmado (wrapper); efeito parcial (provável) |
| 0x35 | 0x08034508 | ActionScript_Op35_CallSubstateAE | `Fight_DispatchOnSubstateAA_AE` (0x08024090; HA7: AE_0F mostra números de dano, **AE_11 aplica dano na peça do bot alvo** (`Fight_DispatchOnSubstateAA_AE_ApplyPartDamage` 0x0802473C, DF.md confirmado), AE_10 espera linha de números, AE_13 ramo por flag 0x04 do bot, AE_1A gfx de peça quebrada, AE_1D roda script de slot). Sem lados. | sim | só ao concluir | confirmado (wrapper); efeito do despachante confirmado para AE_11 |

## Pendências deste lote

- Efeito pendente (despachante sem doc de passos): 0x29 (S), 0x2B (U), 0x2C (V), 0x2D (W), 0x2E (X), 0x2F (Y).
- Efeito parcial: 0x33 (AC) e 0x34 (AD) têm só o passo que escreve código de efeito (A6).
- Byte 0x03000570 (zerado em 0x1C/0x20): origem e uso não identificados.
- Lados (0x15F/0x160): quem consome esses bytes ainda não foi confirmado.
- 0x1B: a origem da paleta (ROM 0x084E4658) e o uso de `0x03002C30` não foram confirmados visualmente no emulador.
