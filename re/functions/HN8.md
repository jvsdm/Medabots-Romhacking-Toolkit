# HN8: handlers dos despachantes Fight_DispatchOnSubstateAA_BL..BP (40 de 40 lidos)

Lista: `HN8_lista.txt` (40 entradas). Cada handler é uma entrada da tabela indexada por +0xAA (passo) do bloco de luta
`0x03001800`. Todos já eram funções no Ghidra (após CreateHandlerFunctions); os 40 foram decompilados e renomeados
como `Fight_DispatchOnSubstateAA_<Desp>_<idx>_<Passo>`. Todos são `void f(void)` (sem argumentos).

Padrão do grupo (animações de técnicas do Robattle, mesmo roteiro do HN3):
- BL (só passos 0x13/0x14 nesta lista): final de técnica de suporte com número por alvo (igual a AW..BA).
- BM: técnica OFENSIVA que lê o medaforce (+0xC1 do bot) de cada inimigo, puxa orbes até o usuário, dispara de volta
  e converte o valor em dano numa peça sorteada (zera +0xC1). Mensagem 0x67.
- BN: suporte em todos os aliados vivos: efeito em duas camadas e liga o bit 3 (0x08) de +0xC0 de cada aliado
  (o mesmo bit que dobra o limite em HA8: provável "medaforce/carga em dobro"). Mensagem 0x68.
- BO: auto-buff do usuário: anéis em loop (4x, passos 2-3), efeito final e soma +0xEC ao byte +0x05 do usuário
  (clamp 0xFF). Mensagem 0x51.
- BP: ataque às pernas dos inimigos: lê a armadura da perna (+0xA1 = +0x59 + 3*0x18), efeito sobe de baixo da tela
  até cada alvo, sfx 0x22, espera `FightPreview_DispatchEffectSlotsC`, ramifica +2/+1.
- Campos comuns: +0x87 bot ativo; +0x60 base do lado alvo (`Fight_GetAllySideBaseIndex` / `Fight_GetOpponentSideBaseIndex`);
  +0xD2..+0xD4 flag "alvo i"; +0xD6/+0xD8/+0xDA valor por alvo; +0x84 contador; +0x86 timer; +0xF8 bit 0 = lado
  (espelha X e o sentido de rotação); +0xA6 código de mensagem. Slots de efeito: 10..12 (registro +0x280) e
  0x21..0x23 (+0x840), 0x24..0x26 (+0x900). Posição do bot: +0x1742/+0x1744 do registro de objetos (y-4).
- Passos ausentes da lista (BM 03/06/07/0E, BN 06, BP 03, BO >0x0A) devem ser handlers compartilhados com outros
  despachantes (provável; não verificado nesta rodada).

## 0x0802E294 Fight_DispatchOnSubstateAA_BL_13_DrawTargetValueNumbers
- O que faz: para cada alvo marcado (+0xD2+i), escreve o número de +0xD6/+0xD8/+0xDA (`Fight_WriteNumberTiles`) e monta
  o OAM sobre o bot +0x60+i (`Fight_BuildBotOamEntriesAt`, slot 0x21). Depois sfx 0x5B, +0x84 = 0, +0xAA += 1.
- Memória: lê +0x60, +0xD2.., +0xD6..; escreve +0x84, +0xAA. Chamada por: BL (0x13).
- Confiança: confirmado.

## 0x0802E358 Fight_DispatchOnSubstateAA_BL_14_AnimateNumbersUntilDone
- O que faz: `Fight_StepRowAnimation` em cada número; +0x84++ (pula para 0x78 se +0x89 == 0, botão bit 0 e > 0x32).
  Em 0x78: `Tile_ClearSlots(0x21 + (+0x60)..., 0xC)` e +0xAA += 1.
- Memória: lê +0xD2.., +0x60, +0x89, entrada; escreve +0x84, +0xAA. Chamada por: BL (0x14).
- Confiança: confirmado.

## 0x0802E44C Fight_DispatchOnSubstateAA_BM_00_MarkOpponentsReadMedaforce
- O que faz: `FightPreview_ClearPartCounters3`; +0x60 = lado inimigo do bot +0x87; para cada inimigo presente (+0x40) com
  cabeça inteira (+0x59 != 0) marca +0xD2+i = 1 e guarda +0xC1 (medaforce) em +0xD6+2i. +0xAA += 1.
- Memória: lê bot +0x40, +0x59, +0xC1; escreve +0x60, +0xD2.., +0xD6.., +0xAA. Chamada por: BM (0x00).
- Confiança: confirmado (significado de +0xC1 = medaforce: R0a/R0b).

## 0x0802E4C8 Fight_DispatchOnSubstateAA_BM_01_SpawnOrbOnOpponents
- O que faz: LZ77 + CpuFastSet 0x10 (gráfico); para cada alvo monta o slot 10+i (escala 4, rotação 0, destino = posição
  do inimigo) e `Slot_SetupFromRecordList(rec+0x120)`. sfx 0x6A, +0xAA += 1.
- Memória: escreve slots 10..12 (+0x280..), +0xAA. Chamada por: BM (0x01).
- Confiança: confirmado.

## 0x0802E5D4 Fight_DispatchOnSubstateAA_BM_02_GrowOrbScale
- O que faz: escala (+8/+9) dos slots marcados += 4 até 0x40; então timer +0x86 = 0x3C e +0xAA += 1.
- Memória: lê +0xD2..; escreve slots 10..12 +8/+9, +0x86, +0xAA. Chamada por: BM (0x02).
- Confiança: confirmado.

## 0x0802E638 Fight_DispatchOnSubstateAA_BM_04_SpinOrbsUntilArrived
- O que faz: gira cada orbe (+0x18 ±0x40 conforme +0xF8) e chama `Fight_StepSlotTowardTarget(10+i)`; quando todos
  chegam, sfx 0x51 para cada alvo com valor (+0xD6) != 0 e +0xAA += 1.
- Memória: lê +0xD2.., +0xD6.., +0xF8; escreve slots 10..12 +0x18, +0xAA. Chamada por: BM (0x04).
- Confiança: confirmado.

## 0x0802E70C Fight_DispatchOnSubstateAA_BM_05_RunOrbAnimScripts
- O que faz: para alvos com valor != 0, `Anim_RunSlotScript(tabela[+0x84], 10+i)`; quando todos terminaram (+0x22 do
  slot == 0xFF, ou valor 0), +0xAA += 1.
- Memória: lê +0xD2.., +0xD6.., +0x84; escreve +0xAA. Chamada por: BM (0x05).
- Confiança: confirmado.

## 0x0802E7A0 Fight_DispatchOnSubstateAA_BM_08_RetargetOrbsToUser
- O que faz: destino dos slots 10..12 marcados (+0x30/+0x32) = posição do usuário (+0x87), zera um byte de estado do
  slot. +0xAA += 1.
- Memória: lê +0x87, +0xD2..; escreve slots 10..12, +0xAA. Chamada por: BM (0x08).
- Confiança: confirmado.

## 0x0802E820 Fight_DispatchOnSubstateAA_BM_09_SpinOrbsBackToUser
- O que faz: como BM_04, com rotação no sentido oposto, movendo os orbes até o usuário; ao chegarem todos, sfx 0x2B e
  +0xAA += 1.
- Memória: lê +0xD2.., +0xF8; escreve slots 10..12, +0xAA. Chamada por: BM (0x09).
- Confiança: confirmado.

## 0x0802E8D4 Fight_DispatchOnSubstateAA_BM_0A_ShrinkOrbsAndClear
- O que faz: escala dos orbes -= 2; ao chegar em 2, `Tile_ClearSlots(10+i, 1)` e +0xAA += 1.
- Memória: lê +0xD2..; escreve slots 10..12, +0xAA. Chamada por: BM (0x0A).
- Confiança: confirmado.

## 0x0802E948 Fight_DispatchOnSubstateAA_BM_0B_LaunchShotsAtOpponents
- O que faz: carrega 2 gráficos (LZ77 + CpuFastSet 8, duas vezes); para cada alvo monta o slot 0x21+i saindo do usuário
  (+0x87) com destino no inimigo (+0x60+i), paleta (+0x3C)+2, espelho se bit 0 de +0xF8; `Slot_SetupFromRecordList(rec+0x1A8)`.
  +0xAA += 1.
- Memória: lê +0x3C, +0x60, +0x87, +0xF8, +0xD2..; escreve slots 0x21..0x23, +0xAA. Chamada por: BM (0x0B).
- Confiança: confirmado.

## 0x0802EAA4 Fight_DispatchOnSubstateAA_BM_0C_WaitEffectSlotsB
- O que faz: +0xAA += 1 quando `FightPreview_DispatchEffectSlotsB()` devolve != 0.
- Chamada por: BM (0x0C). Confiança: confirmado.

## 0x0802EAC0 Fight_DispatchOnSubstateAA_BM_0D_SetMessage67
- O que faz: +0xA6 = 0x67 (código do texto) e +0xAA += 1.
- Chamada por: BM (0x0D). Confiança: confirmado.

## 0x0802EAD8 Fight_DispatchOnSubstateAA_BM_0F_DrainMedaforceAsDamage
- O que faz: para cada alvo: zera o medaforce (+0xC1) do inimigo; sorteia a peça (`Fight_RollStatusRowRandomEntry`) em
  +0xD2+i; `Fight_ApplyDamageToPart(bot, peça, valor +0xD6)`; +0xD2+i |= (+0xD1)<<7; +0xD6 = dano final (+0xC8);
  armadura +0x59+peça*0x18 -= dano; `Fight_BackupHalvedStatsIfFlagClear`; cabeça zerada -> `Fight_QueueHeadBrokenBotForResolution`.
  +0xAA += 1.
- Memória: lê +0x60, +0xC8, +0xD1, +0xD6..; escreve bot +0xC1, +0x59.., +0xD2.., +0xD6.., +0xAA. Chamada por: BM (0x0F).
- Confiança: confirmado (leitura); "medaforce vira dano" = provável.

## 0x0802EBE8 Fight_DispatchOnSubstateAA_BN_00_MarkLivingAllies
- O que faz: `FightPreview_ClearPartCounters3`; +0x60 = lado aliado de +0x87; marca +0xD2+i para aliados presentes com
  cabeça inteira. +0xAA += 1.
- Memória: lê bot +0x40, +0x59; escreve +0x60, +0xD2.., +0xAA. Chamada por: BN (0x00).
- Confiança: confirmado.

## 0x0802EC4C Fight_DispatchOnSubstateAA_BN_01_SpawnEffectOnAllies
- O que faz: LZ77 + CpuFastSet 0x10; slot 0x21+i sobre cada aliado marcado, escala 0x80; `Slot_SetupFromRecordList(rec+0x120)`.
  sfx 0x6A, +0xAA += 1.
- Memória: escreve slots 0x21..0x23, +0xAA. Chamada por: BN (0x01).
- Confiança: confirmado.

## 0x0802ED34 Fight_DispatchOnSubstateAA_BN_02_ShrinkEffectScale
- O que faz: escala dos slots 0x21+i -= 4 até 0x40; então sfx 0x51 e +0xAA += 1.
- Memória: escreve slots 0x21..0x23, +0xAA. Chamada por: BN (0x02).
- Confiança: confirmado.

## 0x0802ED90 Fight_DispatchOnSubstateAA_BN_03_RunEffectAnimScripts
- O que faz: `Anim_RunSlotScript(tabela[+0x84], 0x21+i)` para cada aliado; quando algum termina (+0x22 == 0xFF), timer
  +0x86 = 0x20 e +0xAA += 1.
- Memória: lê +0xD2.., +0x84; escreve +0x86, +0xAA. Chamada por: BN (0x03).
- Confiança: confirmado.

## 0x0802EE14 Fight_DispatchOnSubstateAA_BN_04_SpawnSecondLayerAfterTimer
- O que faz: decrementa +0x86; em 0: LZ77 + CpuFastSet 8, monta a 2a camada (slots 0x24..0x26, escala 0x40, +6 = 0x8100)
  sobre cada aliado marcado, sfx 0x2A, +0xAA += 1.
- Memória: lê +0x60, +0xD2..; escreve +0x86, slots 0x24..0x26, +0xAA. Chamada por: BN (0x04).
- Confiança: confirmado.

## 0x0802EF0C Fight_DispatchOnSubstateAA_BN_05_WaitSecondLayerSlots
- O que faz: `FUN_08036DE8(0x24+i, 0)` para cada aliado; quando algum devolve != 0, timer +0x86 = 0x3C e +0xAA += 1.
- Memória: lê +0xD2..; escreve +0x86, +0xAA. Chamada por: BN (0x05).
- Confiança: confirmado (FUN_08036DE8 sem nome; provável passo de animação do slot).

## 0x0802EF5C Fight_DispatchOnSubstateAA_BN_07_ShrinkBothLayersAndClear
- O que faz: escala das duas camadas (0x21+i e 0x24+i) -= 4 até 4; então `Tile_ClearSlots(0x21, 6)` e +0xAA += 1.
- Memória: escreve slots 0x21..0x26, +0xAA. Chamada por: BN (0x07).
- Confiança: confirmado.

## 0x0802EFD0 Fight_DispatchOnSubstateAA_BN_08_SetAllyFlagC0Bit3
- O que faz: para cada aliado marcado, bot(+0x60+i) +0xC0 |= 0x08. +0xAA += 1.
- Memória: lê +0x60, +0xD2..; escreve bot +0xC0, +0xAA. Chamada por: BN (0x08).
- Confiança: confirmado (efeito de jogo do bit = provável: limite dobrado em HA8).

## 0x0802F018 Fight_DispatchOnSubstateAA_BN_09_SetMessage68
- O que faz: +0xA6 = 0x68 e +0xAA += 1.
- Chamada por: BN (0x09). Confiança: confirmado.

## 0x0802F058 Fight_DispatchOnSubstateAA_BO_00_SpawnRingOnUser
- O que faz: LZ77 + CpuFastSet 8; monta o slot 0x21 sobre o usuário (+0x87), escala 0x40, +6 = 0x4100, contador +0x27 = 0.
  sfx 0x29, +0xAA += 1.
- Memória: lê +0x87; escreve slot 0x21, +0xAA. Chamada por: BO (0x00).
- Confiança: confirmado.

## 0x0802F13C Fight_DispatchOnSubstateAA_BO_01_AnimateFirstRing
- O que faz: `Fight_StepSlotAnimOffsetTable(0x21)`; gira ±0x20 e move X ±1 (em frames pares) conforme +0xF8, contando
  em +0x27 do slot; ao chegar em 0x20: zera rotação, +0x84 = 1, +0xAA += 1.
- Memória: lê +0xF8; escreve slot 0x21, +0x84, +0xAA. Chamada por: BO (0x01).
- Confiança: confirmado.

## 0x0802F214 Fight_DispatchOnSubstateAA_BO_02_SpawnNextRing
- O que faz: monta o slot 0x21 + (+0x84) igual ao BO_00 (sobre o usuário). sfx 0x29, +0xAA += 1.
- Memória: lê +0x84, +0x87; escreve slot 0x21+n, +0xAA. Chamada por: BO (0x02).
- Confiança: confirmado.

## 0x0802F2F4 Fight_DispatchOnSubstateAA_BO_03_AnimateRingLoop4
- O que faz: anima o anel 0x21+n como BO_01; ao fim limpa o slot, ajusta duas posições em n*0x10, n++; se n == 4,
  +0xAA += 1; senão +0xAA = 2 (volta a BO_02).
- Memória: lê +0xF8; escreve slot 0x21+n, +0x84, +0xAA. Chamada por: BO (0x03).
- Confiança: confirmado.

## 0x0802F3FC Fight_DispatchOnSubstateAA_BO_04_TargetSlotToUser
- O que faz: destino do slot 0x21 (+0x870/+0x872) = posição do usuário, velocidade 4. +0xAA += 1.
- Memória: lê +0x87; escreve slot 0x21, +0xAA. Chamada por: BO (0x04).
- Confiança: confirmado.

## 0x0802F468 Fight_DispatchOnSubstateAA_BO_05_MoveSlotToUser
- O que faz: `Fight_StepSlotTowardTarget(0x21)`; ao chegar, sfx 0x2B e +0xAA += 1.
- Chamada por: BO (0x05). Confiança: confirmado.

## 0x0802F48C Fight_DispatchOnSubstateAA_BO_06_ShrinkSlotAndClear
- O que faz: escala do slot 0x21 -= 4 até 4; então `Tile_ClearSlots(0x21, 1)` e +0xAA += 1.
- Chamada por: BO (0x06). Confiança: confirmado.

## 0x0802F4D0 Fight_DispatchOnSubstateAA_BO_07_SpawnBoostEffectOnUser
- O que faz: LZ77 + CpuFastSet 0x10; slot 0x21 sobre o usuário, `Slot_SetupFromRecordList(rec+0x1A0, 0x21)`. sfx 0x2C,
  +0xAA += 1.
- Memória: lê +0x87; escreve slot 0x21, +0xAA. Chamada por: BO (0x07).
- Confiança: confirmado.

## 0x0802F594 Fight_DispatchOnSubstateAA_BO_08_RunBoostAnimThenClear
- O que faz: `Anim_RunSlotScript(rec+0x1A0, 0x21)`; ao terminar (+0x22 == 0xFF), `Tile_ClearSlots(0x21, 1)` e +0xAA += 1.
- Chamada por: BO (0x08). Confiança: confirmado.

## 0x0802F5D8 Fight_DispatchOnSubstateAA_BO_09_AddValueToUserByte05
- O que faz: `Fight_AddByteClampToMax(bot(+0x87) + 0x05, (short)+0xEC, 0xFF)`; +0xAA += 1. (+0x05 é o byte que
  `Fight_SubBotMeterByByte5Div10` usa; significado ainda aberto.)
- Memória: lê +0x87, +0xEC; escreve bot +0x05, +0xAA. Chamada por: BO (0x09).
- Confiança: confirmado (leitura); significado do stat = chute.

## 0x0802F610 Fight_DispatchOnSubstateAA_BO_0A_SetMessage51
- O que faz: +0xA6 = 0x51 e +0xAA += 1.
- Chamada por: BO (0x0A). Confiança: confirmado.

## 0x0802F650 Fight_DispatchOnSubstateAA_BP_00_MarkOpponentsReadLegArmor
- O que faz: como BM_00, mas guarda em +0xD6+2i a armadura da perna (+0xA1) de cada inimigo vivo. +0xAA += 1.
- Memória: lê bot +0x40, +0x59, +0xA1; escreve +0x60, +0xD2.., +0xD6.., +0xAA. Chamada por: BP (0x00).
- Confiança: confirmado (+0xA1 = peça 3 = pernas, R0b).

## 0x0802F6CC Fight_DispatchOnSubstateAA_BP_01_SpawnRisingEffectOnTargets
- O que faz: LZ77 + 2x CpuFastSet 8; slot 0x21+i em (0x78, 0xF0) (abaixo da tela), velocidade 4,
  `Slot_SetupFromRecordList(rec+0x1B4)`, espelho por +0xF8. sfx 0x61, +0xAA += 1.
- Memória: lê +0xD2.., +0xF8; escreve slots 0x21..0x23, +0xAA. Chamada por: BP (0x01).
- Confiança: confirmado.

## 0x0802F7B8 Fight_DispatchOnSubstateAA_BP_02_RaiseEffectsToTargets
- O que faz: `Fight_StepSlotCoordUpInRange(0x21+i, y-4, y+0x10)` com y do alvo; ao chegar zera +0x27/+0x28 e +0x29 = 1;
  quando todos chegam, timer +0x86 = 0x3C e +0xAA += 1.
- Memória: lê +0x60, +0xD2..; escreve slots, +0x86, +0xAA. Chamada por: BP (0x02).
- Confiança: confirmado.

## 0x0802F864 Fight_DispatchOnSubstateAA_BP_04_PlaySfx22
- O que faz: sfx 0x22 e +0xAA += 1.
- Chamada por: BP (0x04). Confiança: confirmado.

## 0x0802F880 Fight_DispatchOnSubstateAA_BP_05_WaitEffectSlotsC
- O que faz: +0xAA += 1 quando `FightPreview_DispatchEffectSlotsC()` devolve != 0.
- Chamada por: BP (0x05). Confiança: confirmado.

## 0x0802F89C Fight_DispatchOnSubstateAA_BP_06_BranchOnNonzeroValues
- O que faz: se algum alvo marcado tem +0xD6+2i != 0 (perna com armadura), +0xAA += 2; senão +0xAA += 1.
- Memória: lê +0xD2.., +0xD6..; escreve +0xAA. Chamada por: BP (0x06).
- Confiança: confirmado.
