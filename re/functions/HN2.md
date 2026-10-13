# HN2: handlers dos despachantes Fight_DispatchOnSubstateAA_AR..AV (lista HN2_lista.txt, 40 entradas)

Contexto (confirmado nesta rodada): os despachantes AR..AV são chamados por wrappers em 0x08034BA0 + 0x1C*k
(chama o despachante; se +0xB5 != 0, avança o script). Os wrappers estão em `gActionScriptOpcodeHandlerTable`
(0x083AF180): AR = opcode 0x50 (entrada 0x083AF2C0), AS = 0x51, AT = 0x52, AU = 0x53, AV = 0x54. Então cada
despachante é a implementação animada de UM opcode do script de ação da Robattle, e +0xAA é o passo dentro dele.
Base: fight = 0x03001800; bot = 0x03000590 + bot*0x100; slots = gObjectSlots 0x03002C30 (+0x40/slot: 0x21 = +0x840,
0x22 = +0x880); temporizador/contadores em 0x03000440 +0x84 (u8) e +0x86 (u16); +0x89 = 0 permite pular com
o bit 0 de 0x03001798 (botão). Bot +0xA8 = id da técnica da ação (deduzido: AS usa 4..8, AV usa 0x0C..0x28).
Renomeados no Ghidra: 40 de 40 (todos com função criada, decompile lido). "Próximo" = valor final de +0xAA.

## Opcode 0x50 / AR: dano em peça aleatória dos 3 adversários (ataque em área)
Passo 0x00 (0x08026FF8, já em R1) escolhe a peça (Fight_RollStatusRowRandomEntry), aplica dano +0xEC e grava
+0xD2+i = peça | (quebrou<<7), +0xD6+2i = dano exibido; +0x60 = base do lado adversário.

## 0x080270BC Fight_DispatchOnSubstateAA_AR_LoadGfxAndTwoSprites (AR 0x01)
- Descomprime gfx 0x084DCD28 -> VRAM 0x06014800, paleta 0x085100D8 -> 0x050002E0. Slot 0x21 (y 0x30) e 0x22 (y 0x50), x 0x78, escala +0x08/+0x09 = 0x80, tiles 0x6500/0x6501.
- Lê/escreve: escreve slots 0x21/0x22 e +0xAA. Próximo: 0x02.
- Confiança: confirmado.

## 0x080271C4 Fight_DispatchOnSubstateAA_AR_ShrinkTwoSprites (AR 0x02)
- Reduz a escala do slot 0x21 de 8 em 8 até 0x40, depois a do 0x22 (ao terminar o 0x21 seta bit 0x8000 do slot 0x22; significado do bit não confirmado). sfx 0x30 quando cada um chega a 0x40.
- Ao fim: timer 0x03000440+0x86 = 0x3C. Próximo: 0x03 (+1).
- Confiança: confirmado.

## 0x0802724C Fight_DispatchOnSubstateAA_AR_SetupEffectSlot (AR 0x04)
- Tile_ClearSlots(0x21,2); slot 0x21 = OBJ 0xC624 (bit 0x1000 = espelho se fight+0xF8 bit 1), x 0x78, y 0x40; Slot_SetupFromRecordList(0x083B6100+0x168, 0x21).
- Próximo: 0x05. Confiança: confirmado.

## 0x080272EC Fight_DispatchOnSubstateAA_AR_WaitEffectScript (AR 0x05)
- Anim_RunSlotScript do slot 0x21 até slot+0x22 = 0xFF; então timer +0x86 = 0, sfx 0x73. Próximo: 0x06.
- Confiança: confirmado.

## 0x0802733C Fight_DispatchOnSubstateAA_AR_WaitThenFlushPalette (AR 0x06)
- Timer +0x86 sobe; FUN_080378A8 a cada frame; em 0x10 chama CopyPaletteBuffersToHardware e zera +0x84. Próximo: 0x07.
- Confiança: confirmado.

## 0x08027378 Fight_DispatchOnSubstateAA_AR_FadeToWhite (AR 0x07)
- Palette_FadeRowsToColorA/B(0x7FFF, +0x84*8, 0xFFFF) para +0x84 = 0..4 (clarão branco); depois zera +0x84. Próximo: 0x08.
- Confiança: confirmado.

## 0x080273DC Fight_DispatchOnSubstateAA_AR_FadeFromWhite (AR 0x08)
- Mesmo fade com intensidade 0x20 - 4*(+0x84), +0x84 = 0..8 (volta do branco). Próximo: 0x09.
- Confiança: confirmado.

## 0x08027444 Fight_DispatchOnSubstateAA_AR_WaitThenClearSlot (AR 0x09)
- Timer +0x86 até > 0x23; Tile_ClearSlots(0x21,1). Próximo: 0x0A. Confiança: confirmado.

## 0x0802747C Fight_DispatchOnSubstateAA_AR_SetStepCode3A (AR 0x0A)
- +0xA6 = 0x3A (código de passo/mensagem); +0xAA++ -> 0x0B (não está nesta lista). Confiança: confirmado.

## 0x08027544 Fight_DispatchOnSubstateAA_AR_ShowDamageNumbers (AR 0x0C)
- Para cada um dos 3 adversários ativos: Fight_WriteNumberTiles(dano +0xD6/+0xD8/+0xDA) e Fight_BuildBotOamEntriesAt(.., bot, 0x21). sfx 0x5B, +0x84 = 0. Próximo: 0x0D.
- Confiança: confirmado.

## 0x0802761C Fight_DispatchOnSubstateAA_AR_AnimateDamageNumbers (AR 0x0D)
- Fight_StepRowAnimation dos números de cada bot; contador 0x03000440+0x84 até 0x78 (ou pula para 0x78 após 0x32 com botão). Ao fim limpa 12 slots a partir do lado (+0x60). Próximo: 0x0E.
- Confiança: confirmado.

## 0x08027494 Fight_DispatchOnSubstateAA_AR_ApplyTeamPartDamage (AR 0x0E)
- Para os 3 bots do lado +0x60 (ativos, cabeça > 0): armadura da peça (bot+0x59 + (D2&0x7F)*0x18) -= dano +0xD6+2i; Fight_BackupHalvedStatsIfFlagClear; se a peça é a cabeça e zerou, Fight_QueueHeadBrokenBotForResolution.
- É aqui que o dano vira estado (antes só era exibido). Próximo: 0x0F. Confiança: confirmado.

## 0x080276E8 Fight_DispatchOnSubstateAA_AR_SpawnPartBreakEffects (AR 0x0F, também BH 0x0F)
- Carrega gfx 0x084CF178. Para cada i com +0xD2+i bit 7 (peça quebrou): Fight_SpawnInlineEffectTextObjects(slot 0x21+4i, x/y do slot da peça 0x5A+bot*4+peça, 4).
- Próximo: +3 (0x12) se alguma quebrou, senão +1 (0x10). Confiança: confirmado.

## 0x08027798 Fight_DispatchOnSubstateAA_AR_WaitPartBreakEffects (AR 0x12, também BH)
- Para cada peça quebrada (0x030018D2+i bit 7) FightPreview_TickSpriteTimersCount(0x21+4i, 4); quando devolve != 0, +0xAA++ -> 0x13.
- Confiança: confirmado.

## 0x080277DC Fight_DispatchOnSubstateAA_AR_SetupPartBreakSlots (AR 0x13, também BH)
- Carrega gfx 0x084CDF48. Para cada peça quebrada: slot 0x21+i = OBJ 0xC004 na posição do slot da peça; Slot_SetupFromRecordList(0x083B6100+0x88, 0x21+i) (explosão da peça). Próximo: 0x14.
- Confiança: confirmado.

## 0x080278B8 Fight_DispatchOnSubstateAA_AR_WaitPartBreakSlotsRefreshBots (AR 0x14, também BH)
- Roda o script dos slots 0x21+i; quando algum chega a +0x22 = 0xFF, limpa o slot. Então Fight_RefreshDisplayedPartSpritesForActiveBots + Fight_PlaceActiveBotObjects (sprite sem a peça). Próximo: 0x15.
- Confiança: confirmado.

## Opcode 0x51 / AS: efeito de estado no próprio bot (técnicas 4..8)

## 0x08027964 Fight_DispatchOnSubstateAA_AS_SelectVariantByTechnique (AS 0x00)
- fight+0x51 = técnica(bot +0x87,+0xA8) - 4 (0..4), ou 5 se fora de 4..8. Próximo: 0x01. Confiança: confirmado (jump table lida).

## 0x080279E4 Fight_DispatchOnSubstateAA_AS_LoadIconAtActor (AS 0x01)
- Carrega gfx 0x084DD0C8 (+ caminhos por técnica via jump table) e 0x084DD300; sfx 0x33. Slot 0x21 = OBJ 0xC604 sobre o bot (x/y do slot 0x5D+bot*4, y-4), escala 0x40, tile 0x6600 ou 0x6601 (fight+0x51 bit 0). Timer +0x86 = 0.
- Próximo: 0x02. Confiança: provável (caudas da jump table não lidas).

## 0x08027BA0 Fight_DispatchOnSubstateAA_AS_SpinIcon (AS 0x02)
- Gira o slot 0x21 (+0x18 += 0x40 & 0x3FF) até o timer +0x86 > 0x3C. Próximo: 0x03. Confiança: confirmado.

## 0x08027BEC Fight_DispatchOnSubstateAA_AS_SetupSecondSprite (AS 0x03)
- Continua o giro; slot 0x22 = OBJ 0xC624 na mesma posição, escala 0x80, tile 0x6700. Próximo: 0x04. Confiança: confirmado.

## 0x08027CB0 Fight_DispatchOnSubstateAA_AS_ShrinkSecondSprite (AS 0x04)
- Gira 0x21; escala do 0x22 -= 8 até 0x40; então sfx 0x30, +0x84 = 0x3C. Próximo: 0x05. Confiança: confirmado.

## 0x08027D24 Fight_DispatchOnSubstateAA_AS_WaitTimerOrSkip (AS 0x05)
- Gira 0x21; +0x84 decresce até 0 (pula se +0x89 == 0 e botão); sfx 0x2B. Próximo: 0x06. Confiança: confirmado.

## 0x08027D98 Fight_DispatchOnSubstateAA_AS_ShrinkAndClearSprites (AS 0x06)
- Escala dos slots 0x21/0x22 -= 4 até 4; então Tile_ClearSlots(0x21,2). Próximo: 0x07. Confiança: confirmado.

## 0x08027E1C Fight_DispatchOnSubstateAA_AS_SetStatusFlagByTechnique (AS 0x07)
- bot(+0x87)+0xD6 |= flag pela técnica: 4->0x01, 5->0x08, 6->0x02, 7->0x10, 8->0x04, outro->0x20 (campo optic_bomb_grav_bonus_flags nos types).
- É o efeito real do opcode. Próximo: 0x08. Confiança: confirmado (disassembly).

## 0x08027EDC Fight_DispatchOnSubstateAA_AS_SetStepCodeByTechnique (AS 0x08)
- +0xA6 = 0x4A/0x61/0x4B/0x62/0x4C para técnica 4..8, 0x63 senão (mensagem do resultado). Próximo: 0x09. Confiança: confirmado.

## Opcode 0x52 / AT: bônus de suporte do time (support_bonus_state +0x28)

## 0x08027F84 Fight_DispatchOnSubstateAA_AT_LoadSupportIcon (AT 0x04)
- Gfx 0x084CAD2C; slot 0x21 |= 0x220, escala +0x08 = 8 / +0x09 = 0x40, y 0x3C, tile 0xE00; espelha se fight+0xF8 bit 0. Próximo: 0x05.
- Confiança: confirmado.

## 0x08028034 Fight_DispatchOnSubstateAA_AT_GrowIcon (AT 0x05)
- Escala do slot 0x21 += 8 até 0x40; timer +0x86 = 0x3C, zera contador slot+0x27. Próximo: 0x06. Confiança: confirmado.

## 0x08028078 Fight_DispatchOnSubstateAA_AT_CheckSupportAlreadyActive (AT 0x07)
- Registro do time = fight + 0x28 + 8*(+0x8A): se +2 != 0 e +3 == 0x5A ('Z', já ativo) zera o contador e vai a 0x08 (pulso); senão sfx 0x3E e +2 (0x09).
- Confiança: confirmado.

## 0x080280D4 Fight_DispatchOnSubstateAA_AT_PulseIcon (AT 0x08)
- Contador slot+0x27 0..0x10: escala +8 nos 8 primeiros frames, -8 nos seguintes; em 0x10 sfx 0x3E. Próximo: 0x09. Confiança: confirmado.

## 0x08028130 Fight_DispatchOnSubstateAA_AT_ApplyTeamSupportBonus (AT 0x0B)
- Se o registro do time não estava ativo: +0x53 = 0, grava +2 = 1, +3 = 0x5A, +4 = clamp(+0xE8), +5 = clamp(+0xEC + bot+0xD5). Se já ativo: +0x53 = 1 e soma metade de cada valor (clamp 0xFF, Fight_AddByteClampToMax).
- Próximo: 0x0C. Confiança: confirmado.

## 0x08028230 Fight_DispatchOnSubstateAA_AT_SetStepCode50 (AT 0x0C)
- +0xA6 = 0x50; +0xAA++. Confiança: confirmado.

## Opcode 0x53 / AU: marca reação no próprio bot

## 0x08028270 Fight_DispatchOnSubstateAA_AU_LoadIconSprite (AU 0x04)
- Gfx 0x084DD368; slot 0x21 tile 0x6A00, y += 4, contador slot+0x27 = 0; timer +0x86 = 0x3C. Próximo: 0x05. Confiança: confirmado.

## 0x080282E0 Fight_DispatchOnSubstateAA_AU_SlideIcon (AU 0x06)
- 16 frames movendo o x do slot 0x21 em ±1 (sentido por fight+0xF8 bit 0); então sfx 0x28 e zera o contador. Próximo: 0x07. Confiança: confirmado.

## 0x08028350 Fight_DispatchOnSubstateAA_AU_BlinkThenClear (AU 0x07)
- Pisca o slot 0x21 (bit 0x8000 a cada 2 frames) até o contador = 0x3C; Tile_ClearSlots(0x21,1). Próximo: 0x08. Confiança: confirmado.

## 0x080283C0 Fight_DispatchOnSubstateAA_AU_SetActorReactionFlag2 (AU 0x08)
- bot(+0x87)+0xC0 |= 2 (reaction_state_flags, enum ACTION34_AFFINITY_GATE nos types). Efeito real do opcode. Próximo: 0x09. Confiança: confirmado.

## 0x080283EC Fight_DispatchOnSubstateAA_AU_SetStepCode64 (AU 0x09)
- +0xA6 = 0x64; +0xAA++. Confiança: confirmado.

## Opcode 0x54 / AV: estado em todos os adversários (técnicas 0x0C,0x0E,0x10,0x12,0x28)

## 0x0802842C Fight_DispatchOnSubstateAA_AV_SelectVariantByTechnique (AV 0x00)
- fight+0x51 = 0/1/2/3/4 para técnica 0x0C/0x0E/0x10/0x12/0x28 (outras: não grava). Próximo: 0x01. Confiança: confirmado.

## 0x08028504 Fight_DispatchOnSubstateAA_AV_LoadEffectGfx (AV 0x01)
- Por técnica (jump table de 29, 6 caminhos) carrega o gráfico; caminho padrão: paleta 0x084E4658 -> 0x050002C0, slot 0x21 OBJ 0xC604 em y 0xE0, x 0xB8 (ou 0x38 espelhado), Slot_SetupFromRecordList(0x083B6100[n], 0x21).
- Próximo: 0x02. Confiança: provável (caminhos por técnica não lidos).

## 0x080287C0 Fight_DispatchOnSubstateAA_AV_WaitEffectSlots (AV 0x02, também AX/AY/AZ)
- Espera FightPreview_DispatchEffectSlots devolver != 0; +0xAA++. Confiança: confirmado.

## 0x080287DC Fight_DispatchOnSubstateAA_AV_ApplyStatusToOpponents (AV 0x03)
- Fight_ClearMultiTargetResolutionScratch; +0x60 = lado adversário. Para cada adversário ativo sem peça de técnica 0x16 (imunidade): se ainda não tem o efeito (FightPreview_IsPartTypeFlagSet), +0xD2+i = 1 e Fight_SetPendingActionEffect(bot, efeito, +0xE8, +0xEC).
- Efeito por técnica: 0x0C->0x0D, 0x0E->0x10, 0x10->0x12, 0x12->0x13, 0x28->0x2C (este sem teste de já-tem). Próximo: 0x04. Confiança: confirmado (disassembly).

## Padrões
- Cada opcode = animação (load gfx -> escala/giro/pulso -> espera com pulo por botão -> limpa slots) + UM passo de efeito real (AR 0x0E, AS 0x07, AT 0x0B, AU 0x08, AV 0x03) + passo final que grava +0xA6 (código de mensagem: 0x3A, 0x4A..0x63, 0x50, 0x64).
- Ramificação por técnica: bot(+0x87)+0xA8 indexa jump tables locais (AS 4..8, AV 0x0C..0x28).
- Saltos não lineares: AR 0x0F (+3 se peça quebrou), AT 0x07 (+2 se suporte novo).
