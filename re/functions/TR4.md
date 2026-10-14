# TR4: consumidor de fi+0xA6 e códigos de passo de Conceal (37) e Scout (36) (GH-TR4, 2026-10-14)

Continuação de TR2.md e TR3.md. Só leitura: Ghidra (decompile, xrefs) e ROM lida por python isolado (`python -I`).
Nada salvo no Ghidra, nada renomeado (nenhum nome novo confirmado), mtbe.csv não editado, sem emulador.
Bloco do bot: `B = 0x03000590 + bot*0x100`. Struct de luta: `fi = 0x03001800`.
Confiança: C = confirmado nesta rodada (decompile ou ROM) | P = provável | A = aberto.

## Resposta direta

| Pergunta | Resposta | Conf |
|---|---|---|
| Quem consome fi+0xA6? | `ActionFamily_StatusCheckFamily_Handler` 0x080305E0. Lê `fi+0xA6` e chama `Fight_RunSubstateDispatch(código)`; se ≠0, `fi+0xAA++`. | C |
| O que `Fight_RunSubstateDispatch` 0x0801C0BC faz? | Grava o código em `fi+0xA2` (`DAT_0801c0e4+0xA2`), depois chama `table[fi+0xA1]` (0x083AD580[fi+0xA1]) via `CallViaR0`. Se `fi+0xA1==3`, zera `fi+0xA1` e devolve 1. | C |
| Quais tabelas o consumidor indexa? | Dois níveis: (1) a tabela de família 0x083AD580[fi+0xA1] (o consumidor aparece nas entradas 20, 30, 33, 41, 58); (2) a tabela de mensagens `gTextGameMessages` 0x083C6744[código] via `Fight_TryUiActionByIndex` 0x0801AC24 → `StateMachine_DispatchStep` (C, decompile). | C |
| O que é a entrada 0x1D? | Ponteiro 0x083C528B → **texto de mensagem** (caracteres de glifo + FD/FA/FF; ex.: `FA 50` = espera 80 quadros, `FF 04` = fim). Não é efeito de jogo. | C |
| Quem grava fi+0xA6 = 0x1D? | `Fight_DispatchOnSubstateAA_AA_0B_SetStepCode1D` 0x08023718: `fi+0xA6=0x1D; fi+0xAA++`. Só isso. Nenhum escritor de +0xB9. | C |
| Como o 0x1D chega ao consumidor? | Tabela AA 0x083ADE40 (dispatcher `Fight_DispatchOnSubstateAA_AA` 0x0802342C, indexado por fi+0xAA): entrada **0x0C** = `ActionFamily_StatusCheckFamily_Handler`. | C |
| Código da técnica 36 (Scout) | **0x1C**. Roteiro 0x083C6AA2 começa com opcode 0x30 → `ActionScript_Op30_CallSubstateZ` 0x0803447C → `Fight_DispatchOnSubstateAA_Z` 0x08022FF0. A entrada Z[0x0F] = `Fight_DispatchOnSubstateAA_Z_0F_SetStepCode1C` 0x08023415 grava `fi+0xA6=0x1C; fi+0xAA++`. Z[0x10] = consumidor (0x080305E0). Texto 0x1C = 0x083C5243. | C |
| O Scout incrementa bot+0xB9 ou grava fi+0x28+8k? | Não no que foi lido. Os dois handlers de código (AA_0B, Z_0F) só mexem em +0xA6 e +0xAA. | C (negativo) |
| O Conceal incrementa bot+0xB9? | Não no que foi lido. O roteiro 37 (0x083C6AA4) = 0x31 → AA[0x0B] (grava 0x1D) → AA[0x0C] (consumidor, mostra a mensagem), depois 0x32..0x35, 0x36. O incremento deve estar em um dos passos seguintes (ver pendentes). | A |

## Cadeia confirmada

```
roteiro 37 (Conceal, 0x083C6AA4): 0x31 ActionScript_Op31_CallSubstateAA 0x08034498
  -> Fight_DispatchOnSubstateAA_AA 0x0802342C  (table 0x083ADE40[fi+0xAA])
     AA[0x0B] Fight_DispatchOnSubstateAA_AA_0B_SetStepCode1D 0x08023718: fi+0xA6=0x1D, fi+0xAA++
     AA[0x0C] ActionFamily_StatusCheckFamily_Handler 0x080305E0
        -> Fight_RunSubstateDispatch(fi+0xA6) 0x0801C0BC: fi+0xA2=código; table 0x083AD580[fi+0xA1]
           -> Fight_TryUiActionByIndex 0x0801AC24 -> StateMachine_DispatchStep(0x083C6744[código])
              (código 0x1D = texto 0x083C528B)

roteiro 36 (Scout, 0x083C6AA2): 0x30 ActionScript_Op30_CallSubstateZ 0x0803447C
  -> Fight_DispatchOnSubstateAA_Z 0x08022FF0  (table 0x083ADDEC[fi+0xAA])
     Z[0x0F] Fight_DispatchOnSubstateAA_Z_0F_SetStepCode1C 0x08023415: fi+0xA6=0x1C, fi+0xAA++
     Z[0x10] ActionFamily_StatusCheckFamily_Handler 0x080305E0 (mesma cadeia acima, código 0x1C = texto 0x083C5243)
```

Outros consumidores de fi+0xA6 (C): `Fight_DispatchOnSubstateAA_BR_31_RunStepCodeMessage` 0x08030E54 chama
`Fight_RunSubstateDispatch(F+0xA6)` e faz `GS++` se ≠0 (decompile). Ou seja, a mesma mensagem pode ser
disparada por outro despachante de cena 8.

## Entradas vizinhas (C, decompile e leitura da tabela)

- `Fight_DispatchOnSubstateAA_AA` 0x0802342C: `fi+0xB5=0`; chama `table[fi+0xAA]` (tabela 0x083ADE40).
- AA[0x00] 0x08023455 Ros da peça (falha → fi+0xA6=0x1A). AA[0x04] 0x080234A9, [0x05] 0x08023565, [0x07] 0x080235AD,
  [0x08] 0x080235E9, [0x09] 0x08023659, [0x0A] 0x080236E1: animação/slot/contador (sem +0xB9).
- AA[0x0B] 0x08023719 (fi+0xA6=0x1D); AA[0x0C] consumidor; AA[0x0D] e [0x11] `Shared_SetDone`; AA[0x0E..0x17] = NoOp.
- Z[0x0A] `Fight_DispatchOnSubstateAA_Z_0A_SetupAllySlotsFromBots` 0x08023275 (decompile): para os 3 bots do
  lado aliado (`fi+0x87` → `Fight_GetAllySideBaseIndex`), se `bot+0x40 != 0` monta o slot de objeto em
  `0x03000840 + k*0x40` (sprite 7, y 0xF0, pal 0x40), toca SFX 0x2E e `fi+0xAA++`. Não grava +0xB9 nem fi+0x28.
- Z[0x0B] `Fight_DispatchOnSubstateAA_Z_WaitAndDecaySlots` 0x08023329 (decompile): espera o contador
  `0x03000840+0x86` chegar a 0x14, depois 0x3C e `fi+0xAA++`; senão desce o campo +2 dos slots ativos.
- Z[0x0F] 0x08023415 (decompile): `fi+0xA6=0x1C; fi+0xAA++`.
- Z[0x10] = consumidor; Z[0x0C] e [0x1B] = `Shared_WaitTimerOrSkip`; Z[0x0E] `Z_AddGaugeMax160` (nome do Toolkit).
- Op 0x36 = `ActionScript_Op36_CallAF` 0x08034524 (chama `Fight_DispatchOnSubstateAA_AF`; ver AS2.md).

## Pendente (A) e próximo passo

1. Achar o escritor de bot+0xB9 (Conceal). Os passos que ainda não foram decompilados do roteiro 37 são 0x32, 0x33, 0x34,
   0x35 e 0x36 (Op36 → despachante AF). Hipótese P: um deles incrementa `B+0xB9` do alvo. Para verificar: decompilar os
   wrappers de 0x32..0x35 (`ActionScript_Op32..35`, nomes ainda não achados no mtbe.csv) e o despachante AF.
2. Escritor de fi+0x28+8k (tabela de Scout). O código 0x1C não o grava. Próximo: xrefs de 0x03001828/0x0300182A com
   escrita (STRB) e ver se o `Fight_DispatchOnSubstateAA_Z_0A_SetupAllySlotsFromBots` tem um irmão que escreve a tabela.
3. Duração: se o Conceal persiste até a derrota (só sabemos `Fight_ResetBotPartState` 0x08036FA0 zerar).
4. Replay (não feito): no emulador, medir 0x03000649 antes/depois de um uso de Conceal e de Scout (0x03001828).

## Símbolos

Nenhum nome novo confirmado nesta rodada. Os nomes já existentes foram conferidos no mtbe.csv e no decompile:
`Fight_DispatchOnSubstateAA_AA_0B_SetStepCode1D`, `Fight_DispatchOnSubstateAA_Z_0F_SetStepCode1C`,
`ActionFamily_StatusCheckFamily_Handler`, `ActionScript_Op30_CallSubstateZ`, `Fight_RunSubstateDispatch`.
Ver TR4_simbolos.csv (só cabeçalho + entradas de conferência, sem alteração de nome).
