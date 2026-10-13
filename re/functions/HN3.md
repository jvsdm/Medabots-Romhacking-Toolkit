# HN3: handlers dos despachantes Fight_DispatchOnSubstateAA_AV..BA (40 de 40 lidos)

Lista: `HN3_lista.txt` (40 entradas). Cada handler é uma entrada da tabela indexada por +0xAA (subestado) do bloco
de luta `0x03001800`. Todos já eram funções no Ghidra (depois do CreateHandlerFunctions). Todos os 40 foram
decompilados e renomeados no Ghidra como `Fight_DispatchOnSubstateAA_<Desp>_<idx>_<Passo>`.

Padrão do grupo: AW..BA são as animações de técnicas de SUPORTE do Robattle (efeito no próprio time).
- AW = proteção contra status, AX = cura de status, AY = reparo de armadura, AZ = recuperação de peça quebrada,
  BA = soma em um medidor (provavelmente o medaforce) dos aliados.
- Roteiro comum: banner da técnica no slot 0x21 (x = 0x38 ou 0xB8 conforme o lado, bit 0 de +0xF8) -> marcar aliados
  -> efeito por aliado nos slots 0x21..0x23 -> aplicar -> número / mensagem (+0xA6 = código do texto) -> fim.
- Campos do bloco 0x03001800: +0x87 = bot ativo; +0x60 = índice do primeiro bot do lado aliado
  (`Fight_GetAllySideBaseIndex`); +0xD2..+0xD4 = flag "aliado i é alvo" (zeradas por `FightPreview_ClearPartCounters3`);
  +0xD6/+0xD8/+0xDA = valor aplicado a cada aliado (halfword); +0xD0 = peça escolhida; +0x51 = variante da técnica;
  +0x84 = contador de frames; +0x86 = timer; +0x89 + bit 0 de um byte de entrada = pular espera.
- Bloco do bot (`0x03000590 + bot*0x100`): +0x40 presente, +0x59 armadura da cabeça (0 = cabeça quebrada),
  +0x59+peça*0x18 = armadura da peça, +0xA8 = código da técnica, +0xC9 = status pendente, +0xCF = máscara de proteção.
- Avanço: quase sempre +1. Os passos "marcar aliados" fazem +2 se houver alvo e +1 se não houver (o +1 cai num
  passo que não está nesta lista, provavelmente a mensagem de "sem efeito").
- Compartilhados: AV 0x07 = BE 0x07; AY 0x04/0x05/0x07 = AZ 0x04/0x05/0x07; AY 0x08 = AZ/BA/BF 0x08. O nome no
  Ghidra usa o primeiro despachante.

## 0x08028A00 Fight_DispatchOnSubstateAA_AV_04_BranchOnFlaggedTargets
- Assinatura: `void f(void)`
- O que faz: se alguma flag +0xD2..+0xD4 está ligada, +0xAA += 2; senão +0xAA += 1.
- Memória: lê +0xD2..+0xD4; escreve +0xAA. Chamada por: AV (0x04).
- Confiança: confirmado (decompile).

## 0x08028A38 Fight_DispatchOnSubstateAA_AV_06_SpawnTargetEffectSlots
- O que faz: LZ77 + cpufastset 8 (gráfico do efeito); para cada aliado marcado em +0xD2+i, monta o slot 0x21+i na
  posição do bot (+0x1742/+0x1744 do registro de objeto, y-4) e salta por tabela indexada por +0x51 (< 6, switch
  não recuperado pelo Ghidra). Sem alvo: sfx 0x5B e +0xAA += 1.
- Memória: lê +0x60, +0x51, +0xD2..; escreve slots 0x21..0x23, +0xAA. Chamada por: AV (0x06).
- Confiança: provável (o switch por +0x51 não foi lido).

## 0x08028B58 Fight_DispatchOnSubstateAA_AV_07_WaitFlaggedSlotsArrived
- O que faz: para cada aliado marcado, `FightPreview_AdvanceSlotX(0x21+i, 0)`; quando algum termina, timer +0x86 = 0x3C, +0xAA += 1.
- Memória: lê flags +0xD2..; escreve +0x86, +0xAA. Chamada por: AV (0x07), BE (0x07).
- Confiança: confirmado (decompile).

## 0x08028BA8 Fight_DispatchOnSubstateAA_AV_09_ClearSlotsDispatchByType
- O que faz: `Tile_ClearSlots(0x21, 3)`; se +0x51 < 6, salta pela tabela de +0x51 (alvo não lido); senão +0xAA += 1.
- Memória: lê +0x51; escreve +0xAA. Chamada por: AV (0x09).
- Confiança: provável.

## 0x08028C68 Fight_DispatchOnSubstateAA_AW_00_SelectStatusTypeFromTechnique
- O que faz: lê +0xA8 do bot ativo e grava a variante em +0x51: 0x0D -> 0, 0x0F -> 1, 0x11 -> 2, 0x13 -> 3 (outro: não muda). +0xAA += 1.
- Memória: lê +0x87, bot+0xA8; escreve +0x51, +0xAA. Chamada por: AW (0x00).
- Confiança: confirmado (decompile); significado "tipo de status" provável.

## 0x08028CC8 Fight_DispatchOnSubstateAA_AW_01_LoadBannerByStatusType
- O que faz: carrega um de 4 gráficos (LZ77 + cpufastset 8) conforme +0x51 (0..3), prepara o slot 0x21 (banner,
  y 0xE0, x 0x38 ou 0xB8 pelo lado +0xF8 bit 0, espelha com |0x1000), sfx 0x61, +0xAA += 1.
- Memória: lê +0x51, +0xF8; escreve slot 0x21, +0xAA. Chamada por: AW (0x01).
- Confiança: confirmado (decompile).

## 0x08028E84 Fight_DispatchOnSubstateAA_AW_02_SlideBannerIn
- O que faz: toca o slot 0x21; sobe um campo do slot de 2 em 2 até 0x40 e desloca o fundo ±0x10 por frame (exceto
  variante 3). Ao chegar: timer +0x86 = 0x3C, +0xAA += 1.
- Memória: lê +0x51, +0xF8; escreve slot 0x21, +0x86, +0xAA. Chamada por: AW (0x02).
- Confiança: confirmado (decompile).

## 0x08028F4C Fight_DispatchOnSubstateAA_AW_03_ScrollBannerWaitTimer
- O que faz: toca o slot 0x21, continua o deslocamento ±0x10; decrementa o timer +0x86 (pulável se +0x89 == 0 e
  bit 0 do byte de entrada). Em 0: +0xAA += 1.
- Memória: lê/escreve +0x86; lê +0x89, +0x51, +0xF8; escreve +0xAA. Chamada por: AW (0x03).
- Confiança: confirmado (decompile).

## 0x08029004 Fight_DispatchOnSubstateAA_AW_04_SetupFiveSlotEffect
- O que faz: LZ77 + cpufastset 8; prepara o slot 0x21 e copia (CpuSet 0x20) para mais 4 slots, ajustando posições
  ±0x10 e frames diferentes (efeito de 5 peças em cruz). +0xAA += 1.
- Memória: escreve slots 0x21..0x25, +0xAA. Chamada por: AW (0x04).
- Confiança: confirmado (decompile); forma "em cruz" é chute.

## 0x08029178 Fight_DispatchOnSubstateAA_AW_05_PlayFiveSlotEffect
- O que faz: toca os 5 slots 0x21..0x25 (`Anim_RunSlotScript` + `Anim_AdvanceSlotTileOffset`); no fim do roteiro
  (byte 0xFF): `Tile_ClearSlots(0x21, 5)`, timer +0x86 = 0x3C, +0xAA += 1.
- Memória: escreve slots, +0x86, +0xAA. Chamada por: AW (0x05).
- Confiança: confirmado (decompile).

## 0x080291E0 Fight_DispatchOnSubstateAA_AW_07_FlagAlliesSpawnEffects
- O que faz: LZ77 + cpufastset 8; zera +0xD2..; +0x60 = lado aliado do bot ativo; para cada aliado presente
  (+0x40) com cabeça inteira (+0x59 != 0): flag +0xD2+i = 1 e efeito no slot 0x21+i na posição do bot. +0xAA += 1.
- Memória: lê +0x87, bot+0x40/+0x59; escreve +0x60, +0xD2.., slots, +0xAA. Chamada por: AW (0x07).
- Confiança: confirmado (decompile).

## 0x080292D4 Fight_DispatchOnSubstateAA_AW_08_WaitAllyEffects
- O que faz: toca o slot de cada aliado marcado; quando um roteiro chega a 0xFF: `Tile_ClearSlots(0x21, 3)`, +0xAA += 1.
- Memória: lê +0xD2..; escreve +0xAA. Chamada por: AW (0x08).
- Confiança: confirmado (decompile).

## 0x08029354 Fight_DispatchOnSubstateAA_AW_09_ApplyStatusGuardAndCure
- O que faz: para cada aliado marcado, liga em bot+0xCF o bit da variante (0 -> 1, 1 -> 8, 2 -> 2, 3 -> 4); se o
  status pendente bot+0xC9 é o par dessa variante (0x0D/0x0E, 0x0F/0x10, 0x11/0x12, 0x13/0x14), limpa com
  `FightPreview_ClearPartFields(bot, 1)`. +0xAA += 1.
- Memória: lê +0x51, +0x60, +0xD2.., bot+0xC9; escreve bot+0xCF, +0xAA. Chamada por: AW (0x09).
- Para o Lua: portar (é a regra da técnica de proteção).
- Confiança: confirmado (decompile); "máscara de proteção" provável.

## 0x08029464 Fight_DispatchOnSubstateAA_AW_0A_SetGuardMessageByType
- O que faz: +0xA6 = 0x55/0x58/0x56/0x57 para a variante 0/1/2/3; +0xAA += 1.
- Memória: lê +0x51; escreve +0xA6, +0xAA. Chamada por: AW (0x0A).
- Confiança: confirmado (decompile).

## 0x080294DC Fight_DispatchOnSubstateAA_AX_00_SetupCureBanner
- O que faz: LZ77 + 2x cpufastset 8; prepara o banner no slot 0x21 (x 0x38/0xB8 pelo lado) e um segundo slot
  (valor 0x5B); +0x51 = 0; +0xAA += 1.
- Memória: lê +0xF8; escreve slots, +0x51, +0xAA. Chamada por: AX (0x00).
- Confiança: confirmado (decompile).

## 0x08029600 Fight_DispatchOnSubstateAA_AX_02_FlagAlliesWithStatus
- O que faz: zera +0xD2..; marca aliados presentes, com cabeça inteira e com status pendente (bot+0xC9 != 0).
  Algum marcado: +0xAA += 2; nenhum: += 1.
- Memória: lê bot+0x40/+0x59/+0xC9; escreve +0x60, +0xD2.., +0xAA. Chamada por: AX (0x02).
- Confiança: confirmado (decompile).

## 0x08029680 Fight_DispatchOnSubstateAA_AX_04_SpawnCureEffects
- O que faz: LZ77 + cpufastset 8; efeito no slot 0x21+i de cada aliado marcado (espelhado pelo lado); sfx 0x37; +0xAA += 1.
- Memória: lê +0x60, +0xD2.., +0xF8; escreve slots, +0xAA. Chamada por: AX (0x04).
- Confiança: confirmado (decompile).

## 0x0802976C Fight_DispatchOnSubstateAA_AX_05_WaitCureEffects
- O que faz: mesmo laço de AW_08 (toca os slots marcados; fim do roteiro -> `Tile_ClearSlots(0x21, 3)`, +0xAA += 1).
- Chamada por: AX (0x05). Confiança: confirmado (decompile).

## 0x080297EC Fight_DispatchOnSubstateAA_AX_06_ClearAllyStatus
- O que faz: para cada aliado marcado, `FightPreview_ClearPartFields(bot, 1)` (limpa o status); +0xAA += 1.
- Memória: lê +0x60, +0xD2..; escreve +0xAA. Chamada por: AX (0x06).
- Confiança: confirmado (decompile).

## 0x08029828 Fight_DispatchOnSubstateAA_AX_07_SetCureMessage
- O que faz: +0xA6 = 0x53; +0xAA += 1. Chamada por: AX (0x07).
- Confiança: confirmado (decompile).

## 0x08029868 Fight_DispatchOnSubstateAA_AY_00_SetupRepairBanner
- O que faz: igual a AX_00 com gráfico próprio (valor 0x5C, registro +0x170); +0x51 = 1; +0xAA += 1.
- Chamada por: AY (0x00). Confiança: confirmado (decompile).

## 0x08029984 Fight_DispatchOnSubstateAA_AY_02_FlagRepairableAllies
- O que faz: marca aliados presentes, com cabeça inteira e com peça danificada (`Fight_PickMostDamagedRepairablePart1Based`
  != 0). Algum: +0xAA += 2; nenhum: += 1.
- Chamada por: AY (0x02). Confiança: confirmado (decompile).

## 0x08029A14 Fight_DispatchOnSubstateAA_AY_04_SpawnAllyHealEffects
- O que faz: LZ77 + cpufastset 8; efeito no slot 0x21+i de cada aliado marcado; sfx 0x2A; +0xAA += 1.
- Chamada por: AY (0x04), AZ (0x04). Confiança: confirmado (decompile).

## 0x08029B04 Fight_DispatchOnSubstateAA_AY_05_WaitAllyHealEffects
- O que faz: mesmo laço de espera de AW_08 (fim -> `Tile_ClearSlots(0x21, 3)`, +0xAA += 1).
- Chamada por: AY (0x05), AZ (0x05). Confiança: confirmado (decompile).

## 0x08029B84 Fight_DispatchOnSubstateAA_AY_06_ApplyArmorRepair
- O que faz: para cada aliado marcado, até 4 vezes: escolhe a peça mais danificada (+0xD0 = peça), calcula a parte
  que cabe (`FightPreview_SplitCountByCapacity`), soma na armadura bot+0x59+peça*0x18 e acumula em +0xD6+2i; o resto
  passa para a próxima peça; para quando o resto é 0. +0xAA += 1.
- Memória: lê/escreve armaduras do bot, +0xD0, +0xD6..+0xDA; escreve +0xAA. Chamada por: AY (0x06).
- Para o Lua: portar (regra do reparo com transbordo entre peças).
- Confiança: confirmado (decompile).

## 0x08029C64 Fight_DispatchOnSubstateAA_AY_07_ShowAllyAmounts
- O que faz: para cada aliado marcado, escreve o número +0xD6/+0xD8/+0xDA em tiles (`Fight_WriteNumberTiles`) e
  monta o OAM sobre o bot (`Fight_BuildBotOamEntriesAt`); sfx 0x5B; +0x84 = 0; +0xAA += 1.
- Chamada por: AY (0x07), AZ (0x07). Confiança: confirmado (decompile).

## 0x08029D28 Fight_DispatchOnSubstateAA_AY_08_AnimateAmountsWait
- O que faz: anima a linha do número de cada aliado (`Fight_StepRowAnimation`); +0x84++ e pula para 0x78 se
  +0x89 == 0, botão pressionado e já passou de 0x32. Em 0x78: `Tile_ClearSlots(0x21 + 4*lado, 0x0C)`, +0xAA += 1.
- Memória: lê/escreve +0x84; lê +0x89, +0x60; escreve +0xAA. Chamada por: AY, AZ, BA, BF (0x08).
- Confiança: confirmado (decompile).

## 0x08029DF4 Fight_DispatchOnSubstateAA_AY_09_SetRepairMessage
- O que faz: +0xA6 = 0x3B; +0xAA += 1. Chamada por: AY (0x09). Confiança: confirmado (decompile).

## 0x08029E34 Fight_DispatchOnSubstateAA_AZ_00_SetupReviveBanner
- O que faz: igual a AY_00 (mesmo registro +0x170); +0x51 = 2; +0xAA += 1.
- Chamada por: AZ (0x00). Confiança: confirmado (decompile).

## 0x08029F50 Fight_DispatchOnSubstateAA_AZ_02_FlagAlliesWithBrokenParts
- O que faz: marca aliados presentes, com cabeça inteira e com peça quebrada (`Fight_PickRandomBrokenOrMissingPart1Based`
  != 0). Algum: +0xAA += 2; nenhum: += 1.
- Chamada por: AZ (0x02). Confiança: confirmado (decompile).

## 0x08029FE0 Fight_DispatchOnSubstateAA_AZ_06_RestoreBrokenParts
- O que faz: como AY_06, mas escolhe peça quebrada ao acaso e divide com `FightPreview_SplitCountByMax`; chama
  `Fight_CopyStatsIfFlagSet` por peça; no fim redesenha as peças (`Fight_RefreshDisplayedPartSpritesForActiveBots`,
  `Fight_PlaceActiveBotObjects`). +0xAA += 1.
- Chamada por: AZ (0x06). Para o Lua: portar. Confiança: confirmado (decompile).

## 0x0802A0D0 Fight_DispatchOnSubstateAA_AZ_09_SetReviveMessage
- O que faz: +0xA6 = 0x3C; +0xAA += 1. Chamada por: AZ (0x09). Confiança: confirmado (decompile).

## 0x0802A110 Fight_DispatchOnSubstateAA_BA_00_SpawnCasterSprites
- O que faz: LZ77 + 2x cpufastset 8; monta dois sprites (slots 0x21 e 0x22) sobre o bot ativo (y do bot - 4, x 0x78);
  sfx 0x67; +0xAA += 1.
- Memória: lê +0x87 e a posição do bot; escreve slots, +0xAA. Chamada por: BA (0x00).
- Confiança: confirmado (decompile).

## 0x0802A274 Fight_DispatchOnSubstateAA_BA_01_RaiseCasterSprites
- O que faz: soma 4 em quatro bytes dos slots (tamanho/escala) até o primeiro chegar a 0x40; então timer +0x86 = 0x3C, +0xAA += 1.
- Chamada por: BA (0x01). Confiança: confirmado (decompile); "escala" é provável.

## 0x0802A2D4 Fight_DispatchOnSubstateAA_BA_03_SlideSpritesUntil20
- O que faz: decrementa duas coordenadas por frame até a primeira chegar a 0x20; então sfx 0x42, +0xAA += 1.
- Chamada por: BA (0x03). Confiança: confirmado (decompile).

## 0x0802A31C Fight_DispatchOnSubstateAA_BA_04_WaitCasterAnim
- O que faz: toca o slot 0x21 (registro +0x184); no fim do roteiro: `Tile_ClearSlots(0x21, 1)`, +0xAA += 1.
- Chamada por: BA (0x04). Confiança: confirmado (decompile).

## 0x0802A360 Fight_DispatchOnSubstateAA_BA_05_FlagAlliesCopyOrbSlots
- O que faz: zera +0xD2..; para cada aliado presente com cabeça inteira, copia o slot base para o slot 0x22+1+i
  (0x40 bytes), põe na posição do bot e marca +0xD2+i. `Tile_ClearSlots(0x22, 1)`; +0xAA += 1.
- Chamada por: BA (0x05). Confiança: confirmado (decompile).

## 0x0802A440 Fight_DispatchOnSubstateAA_BA_06_MoveOrbsToAllies
- O que faz: para cada marcado, gira um campo do slot (+0x20) e `Fight_StepSlotTowardTarget(0x23+i)`; quando todos
  chegam: sfx 0x2B, +0xAA += 1.
- Chamada por: BA (0x06). Confiança: confirmado (decompile).

## 0x0802A4C4 Fight_DispatchOnSubstateAA_BA_07_ShrinkOrbsAndClear
- O que faz: reduz 4 por frame os dois bytes de tamanho dos slots marcados; ao chegar a 4: `Tile_ClearSlots(0x24, 3)`, +0xAA += 1.
- Chamada por: BA (0x07). Confiança: confirmado (decompile).

## 0x0802A534 Fight_DispatchOnSubstateAA_BA_08_AddGaugeToAllies
- O que faz: para cada aliado marcado, `FightPreview_SplitCountByFree(bot, +0xEC)`, guarda o valor (+0xC8) em
  +0xD6+2i e soma no byte +0x10 do registro do bot com teto no byte +0x08 (`Fight_AddByteClampToMax`). +0xAA += 1.
- Memória: lê +0xEC, +0xC8; escreve +0xD6.., registro do bot +0x10. Chamada por: BA (0x08).
- Para o Lua: portar. Confiança: confirmado (decompile); "medidor = medaforce" é chute (a base do registro não foi conferida).
