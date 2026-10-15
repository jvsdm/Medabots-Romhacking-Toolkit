# BA3: estágios 2/3/7/10 da tabela 0x084857D0, consumidor 0x0806DE0C e 0x0806B530 (2026-10-14)

Tarefa BA3 (vindo de BA2). Funções criadas pelo sync no Ghidra e decompiladas. Renames aplicados (ver CSV).

## (1) Estágios de BotAnim (tabela 0x084857D0, hospedeiro BotAnim_TickStageCallbacksAll 0x0806B0D4)

Convenção confirmada no hospedeiro: retorno != 0 = estágio continua (+0x1A++); retorno 0 = terminou (slot_spawn(i,0)).
Slot = `(param & 0xff)*0x40 + base`, base em literal de pool (gObjectSlots 0x03002C30 nos casos abaixo).

| Estágio | Alvo | Nome (novo) | Confiança e o que faz |
|---|---|---|---|
| 2 | 0x0806B1A8 | BotAnim_StageSlideDown2 | confirmado (decompile): short +2 += 2 por quadro (provável Y de tela; o +6 é a posição copiada por slot_spawn); retorno = `+0x1A <= byte +0x22` (continua enquanto a fase não passa do limite em +0x22, que vem de slot_spawn param3). Antes sem nome. |
| 3 | 0x0806B1D0 | BotAnim_StageSlideUp3 | confirmado (decompile): Y-=2, mesma regra de fim do estágio 2. Antes sem nome. |
| 4 | 0x0806B1F8 | BotAnim_StageHold4 | confirmado (bytes `mov r0,#1; bx lr`): espera, retorna 1. Renomeado (antes FUN_0806b1f8). |
| 7 | 0x0806B2B8 | BotAnim_StageWobbleTimed7 | confirmado (decompile): se fase ímpar, decrementa os bytes +8 e +9 (offsets de tela, provável X/Y fino); acumulador +0x18 += 0x40 por quadro; retorno 0 quando fase > 0x8F (zera +0x18). Antes sem nome. |
| 10 | 0x0806B484 | BotAnim_StageWaitTimer10 | confirmado (decompile): se byte +0x27 != 0, decrementa; retorna 1 SEMPRE. Não termina sozinho: só sai se algo externo mudar +0x20. Correção ao BA2: é decremento, não incremento. slot_spawn kind 10 zera +0x27 e grava +0x21 = param*4+0x14. |

Estágios 4 e 11 não têm xref BL (são só ponteiros da tabela), por isso `get_xrefs_to` vazio é esperado.

## (2) 0x0806DE0C = consumidor da tabela 0x08485988 (SIM, parcial)

Renomeado: `battle_effects_5_slots_update` → **BattleFx_TickTimedStages5** (provavel: nome novo; corpo confirmado).

Corpo (confirmado no decompile):
- Loop de 5 slots (i = 0..4) a partir de base DAT_0806DE60 = 0x03002C30 (gObjectSlots).
- Slot com byte +0x20 != 0: fase +0x1A++; `period = byte_table[estágio-1]`; se `fase % period == 0`, chama `call_r1(i, ptr_table[estágio-1])`.
  - Retorno 1: nada. Retorno 0: zera +0x20 e +0x22. Retorno 0xFF: `slot_ptr_stub(i)`.
- Pools: literal 0x0806DE68 = **0x08485994** (base da tabela de ponteiros, 8 entradas usadas, estágios 1..8) e
  literal 0x0806DE64 = **0x084859B4** (tabela de períodos em bytes: 02 02 01 01 1D 04 03 02, estágios 1..8).
- Entradas usadas: 0x08485994..0x084859B0 = 0x0806DE9D, DFC1, DFF1, E015, E039, E039, E075, E075 (Thumb).
  A tabela 0x08485988 tem 11 ptrs no total, mas os 3 primeiros (0x0806DC7D, 0x0806DCC1, 0x08000841) NÃO são usados
  por este consumidor. Ou seja: a base usada é 0x08485994, não 0x08485988 (o BA2 disse 0x08485988 como base; é a mesma
  tabela, o acesso começa 0xC depois).
- Chamador: **Battle_TickEffectsAndTransition** (0x0806D59E, BL incondicional).

## (3) 0x0806B530 = slot_spawn (chamado quando o estágio retorna 0)

Confirmado (decompile): `param_2 == 0` → reset: zera +0x20, +0x21, +0x22, +0x1A e copia +0x30 para +0x6 (posição base).
`param_2 != 0` e slot livre (+0x20 == 0) → liga estágio kind 1..10 e sprite/parâmetros (kind 2/3/4/5/6/7/8/10 com
tabelas diferentes). Nome já existia; sem rename.
Chamadores (xref BL): BotAnim_TickStageCallbacksAll (0x0806B11A, o caminho "retorno 0"), Fight_TickSlotTimersAndSpawn,
intro_scene_step, scene_sequence_a, scene_sequence_b, Fight_TickSlotMotionPhases, dispatch_scene_state.

## Renames feitos no Ghidra (todos "Function renamed successfully")

- 0x0806B1A8 → BotAnim_StageSlideDown2
- 0x0806B1D0 → BotAnim_StageSlideUp3
- 0x0806B1F8 → BotAnim_StageHold4
- 0x0806B2B8 → BotAnim_StageWobbleTimed7
- 0x0806B484 → BotAnim_StageWaitTimer10
- 0x0806B130 → BotAnim_StageDone11
- 0x0806DE0C → BattleFx_TickTimedStages5

Não renomeei 0x0806B530 (já tinha nome bom).

## Pendências

- Tabela 0x08485988: os 3 primeiros ptrs (0x0806DC7D, 0x0806DCC1) têm consumidor próprio? Não investigado.
- Handlers 0x0806DE9C, DFC0, DFF0, E014, E038, E074 (ptrs do estágio 1..8 de BattleFx_TickTimedStages5): sem nome ainda.
- Chamador Battle_TickEffectsAndTransition (0x0806D59E): só confirmado o BL; não decompilado.
- BA2.md item (3) e a pendência de 0x0806DE20 ficam superados por este relatório (o consumidor é 0x0806DE0C).
