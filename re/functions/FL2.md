# FL2 — funções de FL2_lista.txt (41 alvos de BL, faixa 2)

## 0x080005D0 Gfx_VBlankIrq
- Assinatura: `void Gfx_VBlankIrq(void)`
- O que faz: handler de VBlank. Protege reentrada por bit 0 de um byte de controle, chama `VBlankIrqHandler`, limpa o bit correspondente em IF (REG_IF), chama `SwapDisplayBuffersOnVBlank` se o flag 0x80 estiver setado, incrementa um contador de frames (char) e, após 5, marca um byte de estado em 0x1E com 0xFF.
- Chama: VBlankIrqHandler, SwapDisplayBuffersOnVBlank. Chamada por: IRQ de VBlank (ponteiro).
- Memória: IO REG_IF/IRQ ack (0x04000202 via ponteiro); RAM: byte de controle em DAT_08000640 (ponteiro), contador de frame em DAT_08000650.
- Para o Lua: não precisa (IRQ/hardware).
- Confiança: confirmado (lido no código; endereços via ponteiros de literal pool).

## 0x08002AF8 Gfx_BgStripeStep
- Assinatura: `void Gfx_BgStripeStep(void)`
- O que faz: um passo por frame de uma animação de revelação em faixas do BG. Lê um contador de estado (ctx+0x84, byte). Para contador < 13, 2, 4, 6 copia linhas de mapa de tiles (`BgMap_CopyRectWithOffset`, 12x2 tiles, em linhas 1/3/5/7) com offsets de tabela. No contador 10 descomprime e carrega tiles; no 12 limpa um retângulo do mapa. Incrementa o contador e, acima de 18, chama `Anim_SetDefaultTimers`.
- Chama: BgMap_CopyRectWithOffset, decompress_and_store, LoadTilesWithMalias, BgMap_FillRect16, Anim_SetDefaultTimers. Chamada por: loop de tela (ponteiro/tabela; não confirmado).
- Memória: RAM contexto em DAT_08002C1C+0x84 (contador); tabelas de ponteiros em DAT_08002C20; ROM de tiles via DAT_08002C28/2C.
- Para o Lua: precisa (é efeito visual; só se for portar a tela de transição).
- Confiança: provável (nome do contexto de tela não confirmado; o contador é o mesmo que `Gfx_SceneInit` zera).

## 0x08003784 Gfx_SceneInit
- Assinatura: `void Gfx_SceneInit(void)`
- O que faz: prepara a cena de BG: limpa 4 pares de halfwords, ajusta REG_DISPCNT (máscara), escreve registradores de BG (0x500 e dois valores) via ponteiros, descomprime dados (`decompress_and_store`), chama FUN_08003904, copia ponteiros para a área de contexto, carrega três conjuntos de tiles (`LoadTilesWithMalias`), copia um retângulo de mapa, liga camadas em REG_DISPCNT (|0x1F60), define blend (`SetGpuBlendMode(5,0x3F,0x20)`), zera o contador de estado (ctx+0x84) e incrementa um contador global.
- Chama: Clear4PairsHalfwords, decompress_and_store, FUN_08003904, LoadTilesWithMalias, BgMap_CopyRectWithOffset, SetGpuBlendMode. Chamada por: não confirmado (provável inicialização de tela).
- Memória: IO REG_DISPCNT (0x04000000), BG cnt; VRAM 0x05000000 (palette/tiles); RAM contexto DAT_080038C4+0x84 e contador DAT_080038C8.
- Para o Lua: não precisa para a lógica; útil para saber as camadas usadas.
- Confiança: provável (faz setup de cena; qual cena não confirmada).

## 0x0800B79C FUN_0800b79c (sem nome)
- Assinatura: `void FUN_0800b79c(void)`
- O que faz: lê o byte de estado em ctx+DAT_0800B7C8 (ctx = DAT_0800B7C4). Se for 0x1E, seta bit 0x4000 na halfword ctx+0x1C0 e usa o contador de DAT_0800B810. Senão, se o bit 4 do byte estiver zerado, seta 0x4000; se estiver setado, aplica máscara DAT_0800B7CC (limpa). Depois incrementa um byte em (ctx+offset).
- Chama: não confirmado. Chamada por: não confirmado (provável callback de objeto).
- Memória: RAM ctx em DAT_0800B7C4 (+0x1C0 halfword de flags, byte de estado).
- Para o Lua: precisa só se o estado for portado (não identificado).
- Confiança: chute (subsistema não identificado; não renomeado).

## 0x0800CBA8 Gfx_DispatchStateHandler
- Assinatura: `void Gfx_DispatchStateHandler(void)`
- O que faz: trampolim de despacho. Lê um byte de ctx+0x33, usa como índice (×4) numa tabela de ponteiros (DAT_0800CBC0) e chama o handler via R0 (`CallViaR0`).
- Chama: CallViaR0 (trampolim). Chamada por: loop de estado (não confirmado).
- Memória: RAM ctx DAT_0800CBC4+0x33 (índice); tabela de ponteiros em DAT_0800CBC0.
- Para o Lua: não precisa (é só mecanismo de tabela; portar a tabela de handlers).
- Confiança: provável (despacho genérico; nome aplica o padrão de `FightPreview_BuildLayers`, que usa o mesmo ctx+0x33).

## 0x0800D6A8 FightPreview_BuildLayers
- Assinatura: `void FightPreview_BuildLayers(void)`
- O que faz: passo de preview de luta. Chama `FightPreview_InitCompositeBuffers`, depois monta duas camadas de sprite compostas (`BuildCompositePreviewSpriteLayer`): camada 0 em (0x38,0x68) e camada 1 em (0xB8,0x30), usando um byte de ctx+0x39 como parâmetro (cor/paleta). Incrementa ctx+0x33 ao fim.
- Chama: FightPreview_InitCompositeBuffers, BuildCompositePreviewSpriteLayer. Chamada por: Gfx_DispatchStateHandler (provável).
- Memória: RAM ctx DAT_0800D70C (+0x33 estado, +0x39 parâmetro).
- Para o Lua: precisa se o preview for portado (posições fixas acima).
- Confiança: provável (nome pelos callees; ctx ainda não nomeado).

## 0x08010AC8 Gfx_PaletteFadeStep
- Assinatura: `void Gfx_PaletteFadeStep(void)`
- O que faz: passo por frame de fade de paleta. Contador ctx+0x84 (byte; mesmo offset que `Gfx_BgStripeStep`). Se < 0x11 chama FUN_08049A6C, aplica `Palette_FadeRowsToColorA` e `Palette_FadeRowsToColorB` com intensidade 0x20 - 2*contador e incrementa. Senão grava DAT_08010B1C = DAT_08010B20 e chama `Anim_SetDefaultTimers`.
- Chama: FUN_08049A6C, Palette_FadeRowsToColorA, Palette_FadeRowsToColorB, Anim_SetDefaultTimers. Chamada por: loop de tela (provável).
- Memória: RAM ctx DAT_08010B04+0x84 (contador); escrita em DAT_08010B1C.
- Para o Lua: precisa se o fade for portado (a fórmula 0x20-2n está acima).
- Confiança: provável.

## 0x08012608 Fight_CountBotsBonus16C
- Assinatura: `void Fight_CountBotsBonus16C(void)`
- O que faz: percorre 3 blocos de bot (stride 0x100) a partir de DAT_08012644+DAT_08012648 e conta os que têm o byte não zero. Soma 8 × contagem à halfword ctx+0x16C (ctx = DAT_08012640).
- Chama: não. Chamada por: não confirmado (provável cálculo de estado da luta por frame).
- Memória: RAM blocos de bot (0x03000590+bot*0x100, offset do byte não confirmado); escrita em ctx+0x16C.
- Para o Lua: precisa se a regra de bônus for portada (contagem de bots vivos ×8).
- Confiança: provável (o que o byte representa e o significado de ctx+0x16C não confirmados).

## 0x08012E40 Fight_EntryScreenStep
- Assinatura: `void Fight_EntryScreenStep(void)`
- O que faz: passo de quadro da tela de entrada da luta. Chama `Fight_DrawActiveBotsOverlay`, `Fight_AnimateEntryFrames` com o byte de ctx+0x80, `Sprite_UpdateComposite4Motion`, `Fight_LoadBattleBotTilesSetExtra`, `Fight_SetupAllIndicatorSlotsAlt` e `TileMap_ReadRectToBuffer`. Zera ctx+0x84 e incrementa um contador global DAT_08012EA4.
- Chama: Fight_DrawActiveBotsOverlay, Fight_AnimateEntryFrames, Sprite_UpdateComposite4Motion, Fight_LoadBattleBotTilesSetExtra, Fight_SetupAllIndicatorSlotsAlt, TileMap_ReadRectToBuffer. Chamada por: não confirmado.
- Memória: RAM ctx DAT_08012EA0 (+0x80 frame, +0x84 flag), contador DAT_08012EA4.
- Para o Lua: precisa se a tela de entrada for portada.
- Confiança: provável.

## 0x08013B8C Fight_BattleSceneInit
- Assinatura: `void Fight_BattleSceneInit(void)`
- O que faz: inicializa a cena de luta. Limpa 4 pares via `Slots_ClearFourPairs`, ajusta REG_DISPCNT e registradores de BG, descomprime 7 conjuntos (`decompress_and_store`), carrega tiles (`LoadTilesWithMalias`) e paletas por `bios_cpufastset` (VRAM 0x05000000 e paletas de mini-sprite). Desenha a UI: slots de bots laterais, caixa de nome do bot ativo, ícones e linhas/colunas de stats das 4 peças, IDs de peça mostrados (`Fight_GetDisplayedPartIdsOrBrokenGlyphs`), preview do Medabot selecionado, slots da linha de bots. Define a velocidade de animação (8), preenche o mapa de BG e liga blend (`SetGpuBlendMode(5,0x3F,0x20)`). Incrementa DAT_08013DE0.
- Chama: Slots_ClearFourPairs, decompress_and_store, LoadTilesWithMalias, bios_cpufastset, Fight_DrawSideBotSlots, Fight_DrawActiveBotNameBox, Fight_DrawFourPartIcons, Fight_DrawFourPartRowsWithStats, Fight_DrawFourPartStatColumns, Fight_GetDisplayedPartIdsOrBrokenGlyphs, SetSelectedMedabotPreviewPartIds, InitializeSelectedMedabotPreviewRenderer, Fight_DrawRowBotSlotsExceptSelected, SetCompositeBattleSpriteAnimationSpeed, FillBgTilemapRectSequential, SetGpuBlendMode. Chamada por: não confirmado (provável entrada da cena de luta).
- Memória: IO REG_DISPCNT/BG; VRAM 0x05000000; RAM bloco de estado DAT_08013DD4 (+10 = 1); contador DAT_08013DE0.
- Para o Lua: precisa (é a cena inteira da batalha; o layout é o que a tela de luta usa).
- Confiança: provável (callees com prefixo Fight_ confirmam o subsistema; a ordem exata é lida no código).

## 0x0801C110 Fight_UiActionStep
- Assinatura: `void Fight_UiActionStep(void)`
- O que faz: chama `Fight_TryUiActionByIndex` com o byte de ctx+0xA2 como índice. Se devolver não zero (ação aceita), incrementa o byte ctx+0xA1 (avança o passo).
- Chama: Fight_TryUiActionByIndex. Chamada por: não confirmado (provável dispatcher de menu de luta).
- Memória: RAM ctx DAT_0801C134 (+0xA1 passo, +0xA2 índice da ação).
- Para o Lua: precisa se a seleção de ação do menu de luta for portada.
- Confiança: provável.

## 0x0801C470 Fight_BotSideAnimStep
- Assinatura: `void Fight_BotSideAnimStep(void)`
- O que faz: passo por frame dos sprites laterais do bot. Chama `Fight_AnimateEntryFrames` e `Fight_AnimateSideEffectSprites8` com o byte ctx+0x87, `Fight_LoadBotSideTiles` com ctx+0x88, e incrementa o contador ctx+0x9C.
- Chama: Fight_AnimateEntryFrames, Fight_AnimateSideEffectSprites8, Fight_LoadBotSideTiles. Chamada por: não confirmado.
- Memória: RAM ctx DAT_0801C4A0 (+0x87, +0x88, +0x9C).
- Para o Lua: precisa se a animação lateral for portada.
- Confiança: provável.

## 0x08038178 Anim_SlotTickStep
- Assinatura: `void Anim_SlotTickStep(void)`
- O que faz: avança um slot de animação. Lê o índice (byte) em DAT_080381B0+DAT_080381B4, roda `Anim_RunSlotScript` com o ponteiro em ctx+0x178. Para o registro do slot (stride 0x40 a partir de DAT_080381B8): se +0x2D == 0x5A incrementa +0x29; senão, a cada 8 quadros (+0x2D & 7 == 0) decrementa o halfword +4, e sempre incrementa +0x2D.
- Chama: Anim_RunSlotScript. Chamada por: loop de animação (não confirmado).
- Memória: RAM registros de slot de 0x40 bytes (DAT_080381B8); ponteiro de contexto em DAT_080381AC+0x178.
- Para o Lua: precisa se o motor de animação de slots for portado.
- Confiança: provável (o significado de +0x29 e +0x2D como contador/timer é inferido pelo padrão).

## 0x0803BE44 Fight_BotSlideScroll
- Assinatura: `void Fight_BotSlideScroll(void)`
- O que faz: passo de deslize horizontal de um bot. Lê a velocidade/passo (byte +0x2B) do registro do bot (stride 0x40, índice em DAT_0803BED4+DAT_0803BED8) e move a posição (halfword +0x30). Dois ramos (direção em +0x2C). Ao passar de limites fixos (0x78, 0x189, 0x160, 0xF0) limita a posição, cria o deslocamento e atualiza acumuladores em ctx+0x154/+0x158 e os registradores de scroll apontados por DAT_0803BF24/DAT_0803BF28 (ou DAT_0803C014/DAT_0803C018). Usa um shift (sVar1) da tabela de DAT_0803BEC8.
- Chama: não confirmado (é folha). Chamada por: não confirmado.
- Memória: RAM contexto DAT_0803BECC (+0x38, +0x154, +0x158); registros de bot (stride 0x40, +0x29, +0x2B, +0x2C, +0x30); escrita de scroll via ponteiros.
- Para o Lua: precisa se o deslize/scroll da câmera de luta for portado (limites 0x78/0x189 etc.).
- Confiança: provável (função grande; o papel de câmera/scroll é inferido das constantes; não validado no emulador).

## 0x0803CC8C Fight_SpawnEffectTimed
- Assinatura: `void Fight_SpawnEffectTimed(void)`
- O que faz: gerador de efeito temporizado. Se o byte +0x2B do registro (stride 0x40, índice em DAT_0803CCB4+DAT_0803CCB8) for 0, compara +0x28 com +0x2C: se igual, zera +0x28, pega um slot livre (`Slot_FindFree(0xE,0x10)`), preenche posição (X de +0x30 + ..., Y de +0x32), frame e flags (0x80, 0x23) e carrega o sprite pela lista (`Slot_SetupFromRecordList`). Senão incrementa +0x28. Se +0x2B não for 0, decrementa.
- Chama: Slot_FindFree, Slot_SetupFromRecordList. Chamada por: loop de luta (não confirmado).
- Memória: RAM contexto DAT_0803CCB0 (registros 0x40); slots de efeito a partir de DAT_0803CDBC; tabela de ponteiros DAT_0803CDC8.
- Para o Lua: precisa se os efeitos temporizados forem portados.
- Confiança: provável (o nome do efeito é inferido; a lógica de temporização é lida no código).

## 0x0803E4F4 Gfx_BlankSceneInit
- Assinatura: `void Gfx_BlankSceneInit(void)`
- O que faz: inicializa uma cena vazia: limpa 4 pares (`Slots_ClearFourPairs`), ajusta REG_DISPCNT e BG (0x500 e três registradores), zera blocos via `bios_cpufastset`, apaga 4 mapas de BG (`BgMap_FillRect16` 32x32), liga camadas (|0x1F60) e blend (`SetGpuBlendMode(5,0x3F,0x20)`), incrementa DAT_0803E614.
- Chama: Slots_ClearFourPairs, bios_cpufastset, BgMap_FillRect16, SetGpuBlendMode. Chamada por: não confirmado.
- Memória: IO REG_DISPCNT/BG; RAM/VRAM de mapas via ponteiros; contador DAT_0803E614.
- Para o Lua: não precisa (só limpeza de tela).
- Confiança: provável.

## 0x0804172C Gfx_ClearTwoRectsStep
- Assinatura: `void Gfx_ClearTwoRectsStep(void)`
- O que faz: apaga dois retângulos de mapa de 8x6 tiles com `BgMap_FillRect16`: um em (ctx+0x2C, ctx+0x2D) e outro em (0, 0x15), usando o tile de DAT_08041778. Incrementa ctx+0x29.
- Chama: BgMap_FillRect16. Chamada por: não confirmado.
- Memória: RAM ctx DAT_08041774 (+0x29, +0x2C, +0x2D).
- Para o Lua: não precisa.
- Confiança: provável.

## 0x0804AFB8 Screen_UpdateStepWrap
- Assinatura: `void Screen_UpdateStepWrap(void)`
- O que faz: wrapper de uma linha: chama `Screen_UpdateByStep` com o byte de DAT_0804AFC8+0x14.
- Chama: Screen_UpdateByStep. Chamada por: não confirmado (provável tabela de telas).
- Memória: RAM DAT_0804AFC8+0x14 (passo da tela).
- Para o Lua: não precisa (só repassa o passo).
- Confiança: confirmado (corpo de 1 chamada).

## 0x08057CB0 FUN_08057cb0 (sem nome)
- Assinatura: `void FUN_08057cb0(void)`
- O que faz: passo de contagem com uma chamada externa (`FUN_08047C24(1,1,0,tabela[ctx+0x70+0x82])`). Se a chamada devolver não zero, incrementa o byte ctx+0x70; se o novo valor for < 6, escreve (*DAT_08057CF0 - 1) em *DAT_08057CF0, senão escreve 2 em DAT_08057D00.
- Chama: FUN_08047C24 (não identificada). Chamada por: não confirmado.
- Memória: RAM ctx DAT_08057CEC (+0x70 contador); tabela de ponteiros DAT_08057CE8; escritas em DAT_08057CF0 e DAT_08057D00.
- Para o Lua: precisa se a sequência for portada (depende de FUN_08047C24).
- Confiança: chute (subsistema não identificado; não renomeado).

## 0x0805CABC Flow_SubStepInitTimer
- Assinatura: `void Flow_SubStepInitTimer(void)`
- O que faz: chama `StateMachine_DispatchSub`; se devolver não zero, incrementa o contador DAT_0805CAEC e inicia um registro de timer (`Obj_InitTimerRecord`) em ctx+0x50 com parâmetros (0,0,1,0xC0).
- Chama: StateMachine_DispatchSub, Obj_InitTimerRecord. Chamada por: não confirmado (provável loop de máquina de estados).
- Memória: contador DAT_0805CAEC; ctx DAT_0805CAF0+0x50.
- Para o Lua: precisa se a máquina de estados for portada.
- Confiança: provável.

## 0x0805D4FC Gfx_FadeOutThenClear
- Assinatura: `void Gfx_FadeOutThenClear(void)`
- O que faz: fade de saída. Enquanto o contador ctx+0x84 < 9, chama `Palette_FadeRowsToColorA/B` com intensidade contador<<2 e incrementa. Ao terminar, limpa bits de REG_DISPCNT (duas máscaras), chama `Tile_ClearSlots(0,0x80)`, zera DAT_0805D574, põe 1 em DAT_0805D578 e 0x10 em DAT_0805D57C.
- Chama: Palette_FadeRowsToColorA, Palette_FadeRowsToColorB, Tile_ClearSlots. Chamada por: não confirmado.
- Memória: IO REG_DISPCNT; RAM ctx DAT_0805D52C+0x84; escritas em DAT_0805D574/78/7C.
- Para o Lua: precisa se o fade de saída for portado.
- Confiança: provável.

## 0x0805DAB0 Flow_StepDispatchCounter
- Assinatura: `void Flow_StepDispatchCounter(void)`
- O que faz: chama `StateMachine_DispatchStep` com o ponteiro em ctx+0x24; se devolver não zero, incrementa DAT_0805DAD0.
- Chama: StateMachine_DispatchStep. Chamada por: não confirmado.
- Memória: ctx DAT_0805DACC+0x24; contador DAT_0805DAD0.
- Para o Lua: precisa se a máquina de estados for portada.
- Confiança: provável (par de `Flow_SubStepInitTimer`).

## 0x0805EA44 Menu_TextEntryInput
- Assinatura: `void Menu_TextEntryInput(void)`
- O que faz: tela de entrada de texto (teclado em grade). Anima um cursor com `aux_div_u32` (período 6), trata botões (máscaras 0x10, 0x20, 0x40, 0x80, 0x08, 0x100, 0x200) com efeitos sonoros (`snd_sfx_request` 2/4/5/6/7). Mantém coluna (+0xD), linha (+0xE, 0..3) e quantidade de caracteres (+0xF, máx. 10). Cada caractere é gravado como `linha*0x1A + coluna + 1` no buffer em +0x58, terminado por 0xFE. Desenha o texto com `Text_LayoutGlyphSprites` e atualiza sprites de cursor (stride 0x40 a partir de +0x140, `Table_SumIdsUntilEnd` para posição).
- Chama: aux_div_u32, snd_sfx_request, Slot_ClearAttrMask, Text_LayoutGlyphSprites, Table_SumIdsUntilEnd, bios_cpuset. Chamada por: não confirmado (provável tela de nome).
- Memória: RAM contexto DAT_0805EA84 (+0x106) e estado do teclado (DAT_0805EA8C, DAT_0805EB24, DAT_0805EB5C, DAT_0805EBB8, DAT_0805ECEC, DAT_0805ED58, DAT_0805EE94, DAT_0805EF08); sprites em OAM por ponteiros.
- Para o Lua: precisa se a entrada de texto for portada (tabela de caracteres e a regra 0x1A por linha).
- Confiança: provável (função grande; o caso "tela de nome" ainda não confirmado no jogo).

## 0x0805F9C8 Flow_StepWithTimeout
- Assinatura: `void Flow_StepWithTimeout(void)`
- O que faz: chama `StateMachine_DispatchStep` com ctx+0x40; se devolver não zero, incrementa a halfword ctx+2 e, quando passa de 0x3C, grava 0xD em DAT_0805F9FC (muda o estado).
- Chama: StateMachine_DispatchStep. Chamada por: não confirmado.
- Memória: ctx DAT_0805F9F4+0x40; contador DAT_0805F9F8+2; estado em DAT_0805F9FC.
- Para o Lua: precisa se a máquina de estados for portada.
- Confiança: provável.

## 0x080602BC Gfx_ClearVramAndBlend
- Assinatura: `void Gfx_ClearVramAndBlend(void)`
- O que faz: reset de tela: zera VRAM em 0x06000000 (`bios_cpufastset`, tamanho DAT_08060300), `Clear4PairsHalfwords`, `Tile_ClearSlots(0,0x80)`, define REG_DISPCNT=0x60, REG_BLDCNT (valor de DAT_08060304) e REG_BLDY=0x10, incrementa DAT_08060308.
- Chama: bios_cpufastset, Clear4PairsHalfwords, Tile_ClearSlots. Chamada por: não confirmado.
- Memória: IO REG_DISPCNT/BLDCNT/BLDY (0x04000000 área de blend); VRAM 0x06000000; contador DAT_08060308.
- Para o Lua: não precisa.
- Confiança: provável.

## 0x080614DC Gfx_WipeTransitionStep
- Assinatura: `void Gfx_WipeTransitionStep(void)`
- O que faz: passo por frame de transição (wipe/blend). Incrementa o contador (halfword ctx+2, de DAT_0806150C). Enquanto o byte ctx+0x208 < 5 mascara a halfword ctx+0x200; senão subtrai 4 de ctx+0x208 e de ctx+0x14. Se o byte baixo do contador < 0x11 escreve um valor (contador<<8 | (0x10 - contador)) em DAT_0806153C; senão grava 0x1000 em DAT_08061580, limpa bits de REG_DISPCNT, mascara ctx+0x200, chama `Obj_ResetStateFields` e incrementa DAT_0806158C. Zera o byte ctx+0x19 de DAT_08061590.
- Chama: Obj_ResetStateFields. Chamada por: não confirmado.
- Memória: RAM contexto DAT_08061510 (+0x200, +0x208, +0x14); escritas DAT_0806153C, DAT_08061580, DAT_0806158C, DAT_08061590.
- Para o Lua: precisa se a transição for portada (limite 0x11 de frames, igual ao de `Gfx_PaletteFadeStep`).
- Confiança: provável.

## 0x080627A0 Gfx_FadeThenPreviewInit
- Assinatura: `void Gfx_FadeThenPreviewInit(void)`
- O que faz: fade de paleta (A/B) com intensidade contador<<2 enquanto ctx+0x84 < 9; ao terminar chama `FightPreview_InitSideState` com o byte ctx+0x19 e um registro de `gShopItems` (índice ctx+0x1A + DAT_08062804, passo 8 bytes), e zera DAT_08062808.
- Chama: Palette_FadeRowsToColorA, Palette_FadeRowsToColorB, FightPreview_InitSideState. Chamada por: não confirmado.
- Memória: ctx DAT_080627D0+0x84; ctx DAT_08062800 (+0x19, +0x1A); tabela gShopItems (8 bytes por registro); escrita em DAT_08062808.
- Para o Lua: precisa se o preview for portado; o registro vem de gShopItems (ver mtbe.csv).
- Confiança: provável.

## 0x08063644 Gfx_BlendStepBattlePanels
- Assinatura: `void Gfx_BlendStepBattlePanels(void)`
- O que faz: passo de transição com blend. Incrementa o contador ctx+0x3C (de DAT_08063680). Enquanto < 0x11 faz `Palette_FadeRowsToColorB` e escreve BLDCNT (0xE50), BLDY (valor de contador) e BLDALPHA-like (8); ao terminar escreve 0xE10, zera ctx+0x3C e o byte ctx+0x84 de DAT_080636F0 e incrementa DAT_080636F4. Em todo quadro chama `BattlePanel_DrawSlotText` duas vezes (campos +0x31/+0x33 e +0x32/+0x34 de DAT_080636F8) e `Anim_ScrollTileColumns`.
- Chama: Palette_FadeRowsToColorB, BattlePanel_DrawSlotText, Anim_ScrollTileColumns. Chamada por: não confirmado.
- Memória: IO BLDCNT/BLDY (ponteiros PTR_REG_BLDCNT_080636EC); RAM contexto DAT_08063680/DAT_080636F8.
- Para o Lua: precisa se o painel de batalha com transição for portado.
- Confiança: provável.

## 0x08064524 Flow_StepSfxCommit
- Assinatura: `void Flow_StepSfxCommit(void)`
- O que faz: chama `StateMachine_DispatchStep` num dos dois sub-estados (offset 0x6C, ou 0x68 se o byte +0x7A do contexto estiver setado). Se devolver não zero: se o bit 0 de DAT_08064564 estiver setado toca o efeito 0xD (`snd_sfx_request`); senão só avança se um halfword (tabela DAT_08064590) bater com DAT_08064598. Por fim incrementa DAT_0806459C.
- Chama: StateMachine_DispatchStep, snd_sfx_request. Chamada por: não confirmado.
- Memória: ctx DAT_08064560 (+0x7A); flags DAT_08064564; contador DAT_0806459C.
- Para o Lua: precisa se a máquina de estados de menu for portada.
- Confiança: provável (a condição de avanço é lida no código; o significado do halfword não confirmado).

## 0x08064E18 Fight_BuildActiveSlotList
- Assinatura: `void Fight_BuildActiveSlotList(void)`
- O que faz: zera VRAM em 0x06000000 (`bios_cpufastset`), `Tile_ClearSlots(0,0x80)`, zera DAT_05000000 (cor 0 da paleta) e percorre 30 (0x1E) registros de 0x40 bytes a partir de PTR_DAT_08064E80: para cada um com bit 0x80 setado grava seu índice na lista em ctx+0xB0 e conta em ctx+0x30. Se o byte de estado for 2, limpa 4 pares, põe REG_DISPCNT=0x60 e avança o estado; senão grava 6 no estado e zera o próximo.
- Chama: bios_cpufastset, Tile_ClearSlots, Clear4PairsHalfwords. Chamada por: não confirmado.
- Memória: VRAM 0x06000000, 0x05000000; registros de 0x40 em PTR_DAT_08064E80 (30 entradas, bit 0x80 = em uso); lista em ctx+0xB0.
- Para o Lua: precisa se a lista de slots ativos for portada.
- Confiança: provável (30 entradas e bit 0x80 lidos no código; o que cada registro representa não confirmado).

## 0x08066460 FUN_08066460 (sem nome)
- Assinatura: `void FUN_08066460(void)`
- O que faz: lógica de entrada/comando: se o byte ctx+0x7A está zerado, lê um byte de DAT_080664B4; se zero, zera ctx+0x7A e grava 0xEF00 | byte+5 em DAT_08066500+6; senão lê um halfword de DAT_080664B8+10 e, se o byte alto for 0xEF, copia o byte baixo para ctx+5 e marca ctx+0x7A=1. Se o halfword de um registro bater com DAT_08066504, ou sobe para 8 e reseta (byte+5 == 4) ou inicializa timer (+0x50) e soma 2 em DAT_08066530.
- Chama: Obj_ResetStateFields, Obj_InitTimerRecord. Chamada por: não confirmado.
- Memória: ctx DAT_08066480 (+5, +0x7A); buffers DAT_080664B8 / DAT_08066500; contador DAT_08066530.
- Para o Lua: precisa se o protocolo de comandos 0xEF00 for portado.
- Confiança: chute (subsistema não identificado; não renomeado).

## 0x08067D00 Screen_LoadSceneWithSong
- Assinatura: `void Screen_LoadSceneWithSong(void)`
- O que faz: carrega uma tela inteira: toca a música 0x19 (`snd_song_request`), limpa VRAM 0x06000000 e 4 pares, define REG_DISPCNT=0x60 e BG1CNT, carrega 3 blocos LZ77 de tiles (`Screen_LoadLZ77TileBlock`), descomprime e copia paleta (`Asset_UnpackOrCopyBlock`, `bios_cpufastset`), monta dados de mapa e descompressão (`decompress_and_store`), e preenche 16 registros de sprite (stride 0x40, X 0xC4..0x78, Y 0x28, frame 3, prioridade).
- Chama: snd_song_request, bios_cpufastset, Tile_ClearSlots, Clear4PairsHalfwords, Screen_LoadLZ77TileBlock, Asset_UnpackOrCopyBlock, decompress_and_store, lib_memset. Chamada por: não confirmado.
- Memória: IO REG_DISPCNT/BG1CNT; VRAM 0x06000000 / 0x06001800; RAM de sprites em DAT_08067F50 (16 × 0x40).
- Para o Lua: precisa (é a montagem de tela com música; dados vêm das ROMs de asset).
- Confiança: provável.

## 0x08068F34 Screen_ResetWrap
- Assinatura: `void Screen_ResetWrap(void)`
- O que faz: grava 0xFF em DAT_08068F4C, 0xF em DAT_08068F4C+4 e chama `Screen_ResetGfxAndState`.
- Chama: Screen_ResetGfxAndState. Chamada por: não confirmado.
- Memória: RAM DAT_08068F4C (estado da tela).
- Para o Lua: não precisa.
- Confiança: confirmado (corpo de 3 linhas).

## 0x0806D7A0 Gfx_LoadScreenLZ77Assets
- Assinatura: `void Gfx_LoadScreenLZ77Assets(void)`
- O que faz: carrega assets de tela descomprimidos LZ77 para VRAM (`bios_lz77_vram` 4 vezes, destino 0x06000000 e outros), define registros de DMA/cópia em DAT_0806D848, preenche o registrador em DAT_0806D87C e chama `fx_screen_setup_a(2,1,0)`. Marca DAT_0806D8A8 = 1.
- Chama: bios_lz77_vram, fx_screen_setup_a. Chamada por: não confirmado.
- Memória: VRAM 0x06000000 e 0x05000000; RAM em DAT_0806D848, DAT_0806D87C, DAT_0806D8A8.
- Para o Lua: não precisa (carga de asset; ver a ROM).
- Confiança: provável.

## 0x0806E6E4 Gfx_LoadLz77RowsTable
- Assinatura: `void Gfx_LoadLz77RowsTable(void)`
- O que faz: descomprime dois blocos LZ77 (um para VRAM 0x06000000, outro para um buffer local da pilha), copia 20 linhas de 0x3C bytes do buffer para uma tabela de 0x40 bytes por linha, zerando os dois últimos halfwords de cada linha. Ajusta registros (ctx+0x10 = 0x34, ctx+4 = 7), liga BG0 (REG_DISPCNT |= 0x100) e marca DAT_0806E7D0 = 1. Retorno lido da pilha é artefato do decompile.
- Chama: bios_lz77_vram. Chamada por: não confirmado.
- Memória: VRAM 0x06000000; RAM de tabela de linhas (DAT_0806E7D4 aponta para o buffer); IO REG_DISPCNT.
- Para o Lua: precisa se o formato de tabela de linhas for portado.
- Confiança: provável (o retorno pelo decompile é suspeito; ver disassemble se precisar).

