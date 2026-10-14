# OB: quem dispara o stub A, acumuladores de movimento e escritores de 0x030017F0

Lido no Ghidra (decompile, disassembly, xrefs) e na ROM original via `python -I` (pools e tabelas). Sem emulador.
Não editei mtbe.csv nem BI/BI2/HA2. Renames feitos no Ghidra (em memória, não salvei).

## (1) Quem dispara o stub A (0x0801C4FC = Fight_DispatchOnSubstateAA)

Cadeia confirmada, de cima para baixo:

1. `MainState0` (0x08002536, "estado 0"): `table(0x083AC2C8)[byte 0x03002B8C]`. Pool: 0x08002550 -> 0x03002B8C,
   0x0800254C -> 0x083AC2C8. Confirmado.
2. Entrada 9 da tabela 0x083AC2C8 (0x083AC2EC) = `Fight_RunActionScriptLoop` (0x08033840). Provável que seja o
   laço de batalha (a entrada 7, 0x083AC2E4, é `Battle_FrameDispatchOrExit` 0x08012970, que indexa por GS 0x0300137C).
   Mecânica confirmada; o significado "batalha" é provável.
3. `Fight_RunActionScriptLoop`: lê um byte do script (`Fight_LookupActionScriptByte`). Se < 0x80, é índice de op na
   tabela 0x083AF180 e chama a op. Se >= 0x80, avança o cursor e lê de novo. Repete enquanto o flag da iteração
   estiver ligado (as ops é que o ligam). Confirmado.
4. Tabela 0x083AF180, índice **4** = `ActionScript_Op04_DispatchOnSubstateAA` (0x080339F4). Confirmado (ROM).
5. Op04: chama `Fight_DispatchOnSubstateAA` (stub A, 0x0801C4FC) e, se `fight_information+0xB5 != 0`, chama
   `Fight_AdvanceActionScriptCursor`. Confirmado. Único chamador do stub A: a xref de 0x0801C4FC é só 0x080339F6.
6. Stub A: zera `+0xB5` e chama `table_A(0x083AD5CC)[+0xAA]`, com bloco 0x03001800 (pool 0x0801C51C). Confirmado.
   A/22 fica em 0x083AD654 (xref DATA da tabela, ver BI2).

Momento na Robattle: a op 04 roda uma vez por quadro enquanto o script estiver no byte 04. Cada quadro dá um passo
da família `+0xAA`. O avanço do script só acontece quando o passo marca `+0xB5`.

Em aberto: qual script (ou qual op anterior) põe `+0xAA` em 0x22 para chegar em A/22. Não rastreei os dados de
script que contêm o byte 04.

## (2) Acumuladores de movimento (NÃO são gObjectSlots)

Os dois usam o bloco **fight_information 0x03001800** (pool confirmado), não gObjectSlots 0x03002C30.

`Fight_StepAccumMoveA` (0x08043CD4, antes Obj_StepAccumMoveA), campos com base 0x03001800:
- `+0x39` = lado (1 -> ponteiro 0x0300057C, 2 -> ponteiro 0x03001C04; outro valor, nada).
- `+0x3B` = incremento (signed) somado a `+0x3F` (acumulador).
- Se `acc >> 2 != 0`: `+0x40 = acc >> 2` (passo em px), `acc &= 3`.
- `+0x3A` (máscara): bit 1 -> `ptr+2 += passo`; bit 2 -> `ptr+2 -= passo`; bit 4 -> `ptr+0 += passo`; bit 8 -> `ptr+0 -= passo`.
Ou seja: movimento de posição (X/Y, halfwords) de duas estruturas laterais, por acumulador de 2 bits. Provável.
Os ponteiros 0x0300057C e 0x03001C04 não têm nome no CSV; são "estrutura de posição do lado 1/2" (chute).

`Fight_StepAccumMoveB` (0x08043E78, antes Obj_StepAccumMoveB): mesma lógica com campos deslocados:
`+0x74` lado, `+0x76` incremento, `+0x7A` acumulador, `+0x7B` passo, `+0x75` máscara; mesmos ponteiros. Confirmado.

`Fight_StepAccumMoveAWrapper` (0x08043E48, antes Obj_StepAccumMoveAWrapper): só chama A. Confirmado.

`Fight_StepAccumMoveBThenRingFrame` (0x08044100, antes Obj_StepAccumMoveBThenSync): chama B e depois
`Anim_AdvanceRingFrameEvery3(byte bloco+0x38)`. Confirmado.

**Divergência com o BI2:** `0x08043FEC` NÃO é movimento por acumulador. É `Anim_AdvanceRingFrameEvery3`
(nome já presente no Ghidra): contador cíclico 0..2 em `0x03001800+0x7D`; a cada 3 chamadas avança o índice
em `+0x7C` (0..127), copia bytes de tabelas da ROM (0x083C4238 e 0x083C4243) para `+0x7E` e `+0x7F` e faz
`CpuSet` para VRAM (0x02027000 / 0x06008020 como pools). Ou seja, animação de anel/tiles, confirmado pelo
disassembly e pelo decompile. Não é gObjectSlots.

## (3) Escritores de 0x030017F0 (valor gravado)

Todos os três têm store no próprio corpo. O store fica dentro de um caso de um despachante por tabela.

| Função (entrada) | Caso que grava | Valor gravado em 0x030017F0 | Confiança |
|---|---|---|---|
| Overworld_SetTransitionStepAndDispatch (0x080497E0) | caso 5 (`table[byte 0x03006F41] == 5`, entrada 0x08049930), store 0x080499A2 | **0x08049CC9** = Obj_LoadPairFromRecord (0x08049CC8) | confirmado (disasm + pool) |
| Dispatch_ByteStateIndexTable (0x0801387C) | caso 5 (`table[byte 0x03000579] == 5`, entrada 0x08013A2C), store 0x08013A3C | **0x08044101** = Fight_StepAccumMoveBThenRingFrame | confirmado (disasm + pool) |
| Fx_ClearIwramBlockMaskDispcntDisarm (0x08068EA8, antes Fx_StepTableBThenSetDispcnt) | só se `bl 0x080688E0` retorna != 0 | **0x08000841** = NoOpActionHandler (desarma) | confirmado (disasm + pool) |

Detalhes extras de Fx (mesma função): antes do desarme faz DMA3 (0x040000D4) com origem fixa de um zero da pilha,
destino **0x03002C30** e controle 0x85000800 = zerar 0x800 unidades de 32 bits (0x2000 bytes, 0x03002C30..0x03004C2F).
Depois faz `DISPCNT &= 0x80FF` (0x04000000) e grava 0x0300137C = `*0x03006F80`. A condição 0x080688E0 não foi lida.

Confirmado: o valor de 0x030017F0 é trocado no caso específico; o nome do callback continua sendo "passo por quadro".
Nota: o BI2 dizia "store não visível" nas três; agora o store e o valor estão lidos.

## Renames no Ghidra (nesta rodada)

- 0x08043CD4: Obj_StepAccumMoveA -> `Fight_StepAccumMoveA` (provável; base 0x03001800, não é Obj).
- 0x08043E78: Obj_StepAccumMoveB -> `Fight_StepAccumMoveB` (provável).
- 0x08043E48: Obj_StepAccumMoveAWrapper -> `Fight_StepAccumMoveAWrapper`.
- 0x08044100: Obj_StepAccumMoveBThenSync -> `Fight_StepAccumMoveBThenRingFrame` (confirmado: chama o anel).
- 0x08068EA8: Fx_StepTableBThenSetDispcnt -> `Fx_ClearIwramBlockMaskDispcntDisarm` (confirmado pelos efeitos).

## Em aberto

- Qual script/op põe `+0xAA` em 0x22 (entrada de A/22) e onde estão os bytes 04 no script.
- Condição 0x080688E0 (Fx_ClearIwramBlockMaskDispcntDisarm) e o significado exato de 0x03002B8C como "modo".
- Nomes dos ponteiros 0x0300057C e 0x03001C04 (estruturas laterais que os acumuladores movem).
- Correção ao BI2: o 0x08043FEC não é movimento, e Overworld/Dispatch/Fx agora têm valor confirmado.
