# TR5: escritor de bot+0xB9 (Conceal) e de fi+0x28+8k (Scout) (GH-TR5, 2026-10-14)

Parcial. Só leitura: Ghidra (decompile) e ROM via `python -I`. Nada salvo no Ghidra, nenhum nome novo confirmado,
mtbe.csv não editado, sem emulador. Continua TR2, TR3 e TR4.
Bloco do bot: `B = 0x03000590 + bot*0x100` (offset Ghidra = offset direto). Struct de luta `fi = 0x03001800`.
Confiança: C = confirmado nesta rodada | P = provável | A = aberto.

## Resposta direta

| Pergunta | Resposta | Conf |
|---|---|---|
| Quem grava bot+0xB9 (Conceal)? | **Não achado.** Os passos do roteiro 37 que foram lidos (AB..AE, AF_11) não escrevem +0xB9. | C (negativo nos passos lidos), A (escritor) |
| Quem grava fi+0x28+8k (Scout)? | Não achado. Não foi varrido nesta rodada (ver Pendente). | A |

## Roteiro 37 (Conceal, 0x083C6AA4) e roteiro 36 (Scout, 0x083C6AA2)

Bytes lidos na ROM (C):

    Conceal: 31 01 | 06 08 | 32 01 | 06 08 | 33 01 01 | 06 08 | 34 01 | 06 08 | 35 21 01 | 06 08 | 36 01
    Scout:   30 01 | 31 01 | 06 08 | 32 01 | 06 08 | 33 01 01 | 06 08 | 34 01 | 06 08 | 35 21 01 | 06 08 | ...

Cada opcode 0x31..0x36 chama um despachante de substate AA pela tabela da ação (`fight+0xAA` indexa a tabela):

| op | wrapper (Ghidra, confirmado) | despachante | tabela ROM (C) |
|---|---|---|---|
| 0x31 | `ActionScript_Op31_CallSubstateAA` 0x08034498 | `Fight_DispatchOnSubstateAA_AA` 0x0802342C | 0x083ADE40 |
| 0x32 | `ActionScript_Op32_CallSubstateAB` 0x080344B4 | `Fight_DispatchOnSubstateAA_AB` (endereço não anotado nesta rodada) | 0x083ADEA0 |
| 0x33 | `ActionScript_Op33_CallSubstateAC` 0x080344D0 | `Fight_DispatchOnSubstateAA_AC` | 0x083ADF00 |
| 0x34 | `ActionScript_Op34_CallSubstateAD` 0x080344EC | `Fight_DispatchOnSubstateAA_AD` | 0x083ADF60 |
| 0x35 | `ActionScript_Op35_CallSubstateAE` 0x08034508 | `Fight_DispatchOnSubstateAA_AE` | 0x083ADFE0 |
| 0x36 | `ActionScript_Op36_CallAF` 0x08034524 | `Fight_DispatchOnSubstateAA_AF` 0x08024998 | 0x083AE064 |

Padrão dos wrappers 0x32..0x35 (C, decompile): `Despachante(); se +0xB5 != 0 → Fight_AdvanceActionScriptCursor()`.
Os despachantes AB..AF (C, decompile): `+0xB5 = 0; CallViaR0(tabela[fight+0xAA])`. Ou seja, cada opcode executa
UM passo, escolhido pelo índice que o próprio passo incrementa (`+0xAA++`). Por isso o passo exato de cada opcode
depende do valor de `fight+0xAA` naquele momento (não foi simulado).

Nota: o endereço do despachante AB não foi anotado nesta rodada (o decompile devolveu só o nome). Os demais
(AC, AD, AE, AF) estão na tabela acima pelos nomes do Ghidra.

## Passos de AB..AF com ponteiro de bot (C)

Varredura de literal-pool: procurei `0x03000590` (base do bot) nas funções 0x08022000..0x08026000 (22 ocorrências).
Atribuí cada uma à função que a precede no pool. Resultado nos passos das tabelas AB..AF:

| passo | função | escreve (bot) | +0xB9? |
|---|---|---|---|
| AB_0E | `Fight_DispatchOnSubstateAA_AB_CopyDisplayValues` 0x08023A48 | +0xC4 = 0x26, +0xC5 = 0, +0xC6, +0xC7 (valores de display) | não |
| AC_0E | `Fight_DispatchOnSubstateAA_AC_0E_SetBotFlagBF` 0x08023C50 | +0xBF = 1; +1 no índice | não |
| AD_15 | `Fight_DispatchOnSubstateAA_AD_WriteBotDisplayRows` 0x08023FB8 | +0xC9/+0xCA/+0xCB/+0xCC (duas linhas: bot e alvo, +0x87) | não |
| AE_0F | `Fight_DispatchOnSubstateAA_AE_ApplyDamageShowNumbers` 0x0802463C | aplica dano na peça (`Fight_ApplyDamageToPart`), mostra números | não |
| AE_13 | `Fight_DispatchOnSubstateAA_AE_BranchOnBotFlag04` 0x08024790 | só lê +0xBD bit 0x04 e desvia | não |
| AE_15 | `Fight_DispatchOnSubstateAA_AE_SetBotFlag01` 0x080247C8 | +0xBD |= 1 | não |
| AF_11 | `Fight_DispatchOnSubstateAA_AF_11_SetSlotParams` 0x08024D18 | +0xC9 = 0x2B, +0xCA (tipo 1..3), +0xCB, +0xCC (valores de display) | não |

Conclusão C: nenhum desses passos grava +0xB9. Os passos da cadeia Conceal que mexem no bot são de display (C4..CC)
e de flags (BD, BF). O +0xB9 não está nos literais de bot desses despachantes.

## Por que a varredura de `adds #0xB9` não bastava

Em `Fight_AwardPartExperience` (0x080127DC, vizinho de 0x080128BA) o `mov #0xB9; lsl #1` escreve em
`fight + bot*8 + 0x172` (tabela de 8 B por bot dentro da struct de luta), não em bot+0xB9. Não é o Conceal.

## Pendente (A)

1. Escritor de bot+0xB9: falta olhar os passos de AB..AE sem literal de bot (índices 0x00 RosCalcPart, 0x08 e 0x0B
   LoadGfx/SetupSlot), que chamam helpers. O bot pode chegar como argumento de um helper (sem literal na função).
   Sugestão: xrefs de cada helper chamado pelos passos AB..AE, procurando o que faz `STRB` em `base+0xB9`.
2. Os argumentos do opcode 0x33 (`01 01`) e do 0x35 (`21 01`) podem ser o índice de passo ou o alvo; não lidos.
3. Escritor de fi+0x28+8k (Scout): não varrido nesta rodada. Caminho: procurar STRB em `fight+0x28+8k` (base 0x03001800
   com `add #0x28`) nas funções que chamam `Fight_UpdatePhaseFromCounter` 0x0801A640.
4. Replay (não feito, sem emulador): ler 0x03000649 antes e depois de uma ação 37 (`replay_record.py --watch`).
