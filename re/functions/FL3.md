# FL3 — funções documentadas (faixa 3 de FL_lista)

Faixa: 41 alvos de BL (lista `FL3_lista.txt`). Nomes do Ghidra eram todos `FUN_`; ao renomear, o nome novo
segue `Subsistema_Verbo`.

## 0x08000824 FUN_08000824
- Assinatura: `void FUN_08000824(void)`
- O que faz: chama um callback via `CallViaR0(*DAT_08000838)` e grava `2` em `*DAT_0800083c` (provável: avança um estado global ao fim do callback). Corpo de 20 bytes.
- Chama: `CallViaR0` (trampolim). Chamada por: não verificado.
- Memória: ponteiro em ROM (`DAT_08000838`) e byte/word de estado em `DAT_0800083c` (endereço não resolvido).
- Confiança: chute quanto ao significado do estado; leitura do código é confirmada.
- Nome: mantido `FUN_08000824` (sem nome seguro).

## 0x08000E40 Gfx_LoadTileAsset
- Assinatura: `void Gfx_LoadTileAsset(u32 a, u32 b, u32 src, u32 c)`
- O que faz: descomprime um asset via `decompress_and_store(param_3, DAT_08000E6C)` e copia as linhas de tiles para VRAM com `CopyTileRowsToVram(param_1, param_2, uVar1, param_4)`. Não retorna valor.
- Chama: `decompress_and_store` (0x080009BC), `CopyTileRowsToVram`. Chamada por: não verificado.
- Memória: lê ROM (fonte comprimida, ponteiro em `DAT_08000E6C`); escreve VRAM/buffer de tiles via as funções chamadas.
- Confiança: provável (nomes das funções chamadas já são do Ghidra).

## 0x08002C34 Menu_RedrawFrameByState
- Assinatura: `void Menu_RedrawFrameByState(void)`
- O que faz: lê o byte de estado `ctx+0x8E` (ctx = `DAT_08002C50`) e, conforme 0/1/2 (ou outro), redesenha duas ou três faixas do mapa de fundo com `BgMap_FillRect16(0xD, y, 0x11, h, tile, 0)` (colunas 0xD..0x11); grava o próximo passo em `ctx+0x88` (1, 3, 5 ou 7). Depois limpa os slots de tile `Tile_ClearSlots(0,1)` e `Tile_ClearSlots(10,7)` e incrementa um contador `*DAT_08002D44`.
- Chama: `BgMap_FillRect16`, `Tile_ClearSlots`. Chamada por: não verificado.
- Memória: lê `ctx+0x8E`; escreve `ctx+0x88` e o contador em `DAT_08002D44`; escreve mapa de fundo BG via `BgMap_FillRect16`.
- Confiança: provável (parece atualizar um quadro/caixa de janela de menu conforme o estado; a tabela de tiles não foi lida).

## 0x08003EE0 Gfx_ResetTileState
- Assinatura: `void Gfx_ResetTileState(void)`
- O que faz: copia um bloco de 8 words com `bios_cpufastset` (SWI 0x0C), limpa 0x80 slots de tile com `Tile_ClearSlots(0,0x80)`, zera o byte/word `PTR[0x96]` e os campos apontados por `PTR_DAT_08003F1C`, `_08003F20` (=2) e `_08003F24` (=0x10). Reinicialização de estado gráfico.
- Chama: `bios_cpufastset`, `Tile_ClearSlots`. Chamada por: não verificado.
- Memória: escreve RAM via ponteiros (estado de tiles) e a área de origem/destino do CpuFastSet (endereços em `DAT_08003F10/14`, não resolvidos).
- Confiança: provável.

## 0x0800B814 FUN_0800B814
- Assinatura: `void FUN_0800B814(void)`
- O que faz: passo de animação: decrementa em 8 um contador em `ctx+0x1C8` e outro em `ctx+DAT_0800B850` (ctx = `DAT_0800B84C`). Quando o contador chega a zero, limpa a máscara de atributos dos slots (`Slot_ClearAttrMask(7,1)`) e incrementa um estado em `ctx+DAT_0800B854`. Parece um deslize/fade de saída.
- Chama: `Slot_ClearAttrMask`. Chamada por: não verificado.
- Memória: lê/escreve `ctx+0x1C8`, `ctx+DAT_0800B850`, `ctx+DAT_0800B854`.
- Confiança: chute quanto ao efeito (subsistema de tela/menu não confirmado). Nome mantido.

## 0x0800CBC8 Menu_UpdateDetailPage
- Assinatura: `void Menu_UpdateDetailPage(void)`
- O que faz: atualização por quadro de uma página de detalhe. Alterna um flag em `ctx+0x34` conforme bits de entrada; desenha duas linhas de texto a partir de buffers (`Mem_CopyBytes` + `Text_LayoutGlyphSprites`); move um cursor em `ctx+0x34`/`ctx+0x35` (lista com wrap 0..8 ou 0x51); formata dois números de 6 dígitos (`Num_FormatDecimal6Glyphs`); carrega um retrato na VRAM (`LoadPortraitToVram_Unk`) ou apaga a área (`BgMap_FillRect16`) conforme o byte de flags em `PTR_DAT_0800CE28`.
- Chama: `Mem_CopyBytes`, `Text_LayoutGlyphSprites`, `Tile_ClearSlots`, `Num_FormatDecimal6Glyphs`, `LoadPortraitToVram_Unk`, `BgMap_FillRect16`. Chamada por: loop de menu (não verificado).
- Memória: lê entrada/flags (ponteiros em `DAT_0800CC08`, `DAT_0800CC28`, `DAT_0800CCA4`, `DAT_0800CD28`); escreve `ctx+0x34`, `ctx+0x35..`, flag em `PTR_DAT_0800CE30`, VRAM via as funções chamadas.
- Confiança: provável (texto/cursor/retrato confirmados pelos nomes chamados; o significado exato da página não).

## 0x0800DA34 PartsGrid_InitScreen
- Assinatura: `void PartsGrid_InitScreen(void)`
- O que faz: inicializa a tela da grade de peças: toca música 7 se o byte `ctx+0x3C` == 4; liga bits em `REG_DISPCNT` (`| 0x1F60`); carrega 7 blocos comprimidos (`decompress_and_store`) e 4 conjuntos de tiles (`LoadTilesWithMalias`, 0x200 cada); limpa VRAM/paleta com `bios_cpufastset`; monta o mapa de fundo (`BgMap_CopyRectWithOffset`) e desenha textos, ids, ícones e cursor (`PartsGrid_*`, `Menu_SetupCursorSprite`). Se o byte `ctx+0x42` != 1, chama `PartsGrid_InitScreenBuffers`. Incrementa o contador em `DAT_0800DCFC`.
- Chama: `snd_song_request`, `decompress_and_store`, `LoadTilesWithMalias`, `bios_cpufastset`, `SetGpuBlendMode`, `BgMap_CopyRectWithOffset`, `PartsGrid_*`, `Menu_SetupCursorSprite`, `Num_DrawDecimal6BigTiles`. Chamada por: transição de tela (não verificado).
- Memória: lê/escreve `REG_DISPCNT`; `ctx` = `DAT_0800DA50`/`DAT_0800DBBC`/`DAT_0800DCEC` (estado da grade, campos +0x3C, +0x3E, +0x3F, +0x42, +0x44); escreve VRAM/paleta (`0x05000000`).
- Confiança: provável (prefixo `PartsGrid_` já usado pelas funções chamadas; nome da tela inferido).

## 0x08010B24 FUN_08010B24
- Assinatura: `void FUN_08010B24(void)`
- O que faz: zera 0x14 bytes de um bloco de estado (`Mem_FillBytes(DAT_08010B48, 0x14, 0)`) e grava `2` no primeiro byte, `3` em `*DAT_08010B4C` e `0` em `*DAT_08010B50`. Reset de estado.
- Chama: `Mem_FillBytes`. Chamada por: não verificado.
- Memória: escreve 20 bytes no bloco apontado por `DAT_08010B48` e dois bytes via ponteiros `DAT_08010B4C`/`DAT_08010B50` (endereços não resolvidos).
- Confiança: confirmado no código; subsistema não identificado. Nome mantido.

## 0x0801264C Fight_AdjustAvgStatBonus
- Assinatura: `void Fight_AdjustAvgStatBonus(void)`
- O que faz: calcula a média de um byte (+5 de cada bloco) sobre os 3 blocos de bot (stride 0x100) cujo flag (+0x00 nao zero) está ligado, para os dois lados (flags em +0xBA no segundo lado; bloco base em `DAT_080126E0`, lado 2 em +0x300). Se a média do lado 2 for menor, soma `(média1 - média2) * n * 0x10` a um short em `DAT_080126E8 + 0x16C`.
- Chama: `aux_div_u32` (divisão sem sinal). Chamada por: não verificado.
- Memória: lê blocos de bot (0x03000590 + bot*0x100, offsets +0x00/+0x05/+0xBA/+0x300); escreve short em `DAT_080126E8+0x16C`.
- Confiança: provável (layout de bot confirmado; o significado do bônus não).

## 0x0801303C Fight_RunEntryAnimStep
- Assinatura: `void Fight_RunEntryAnimStep(void)`
- O que faz: um passo do quadro de entrada da luta: desenha overlay dos bots ativos (`Fight_DrawActiveBotsOverlay`), anima os frames de entrada usando o byte `ctx+0x80` (`Fight_AnimateEntryFrames`), move um composite de sprite (`Sprite_UpdateComposite4Motion`), limpa tiles (CpuFastSet + `Tile_ClearSlots(0,6)`, `(0x1E,0xB)`), chama `FUN_080166AC` e `Fight_ChooseBotActionCode`, lê um retângulo do mapa para buffer (`TileMap_ReadRectToBuffer`). Zera `ctx+0x84` e incrementa um contador.
- Chama: `Fight_DrawActiveBotsOverlay`, `Fight_AnimateEntryFrames`, `Sprite_UpdateComposite4Motion`, `bios_cpufastset`, `Tile_ClearSlots`, `FUN_080166AC`, `Fight_ChooseBotActionCode`, `TileMap_ReadRectToBuffer`. Chamada por: loop de batalha (não verificado).
- Memória: ctx em `DAT_080130B4` (+0x80, +0x84); contador em `DAT_080130D4`; buffers via `DAT_080130C8`/`DAT_080130CC`.
- Confiança: provável (nomes das chamadas são do Ghidra e indicam tela de luta).

## 0x08013FA8 Fight_SelectScreenFrame
- Assinatura: `void Fight_SelectScreenFrame(void)`
- O que faz: quadro da tela de seleção/prévia de Medabot: avança o slot de animação da prévia, tica contador de piscar (`Ui_TickBlinkCounter`), renderiza a prévia grande (`RenderSelectedMedabotLargePreviewFrame`), desenha as linhas de bots exceto o selecionado, ajusta a posição (`Slot_SetPosStepByMode(8, ...)`) e tica o fade (`GpuBlend_TickStep`). Quando o fade termina, zera um byte em `PTR_DAT_08014030[0x96]` e uma word, limpa 0x200 bytes de VRAM/paleta com CpuFastSet e define `*DAT_0801404C = 0x3C` (espera de 60 quadros).
- Chama: `AdvanceSelectedMedabotPreviewAnimationSlot`, `Ui_TickBlinkCounter`, `RenderSelectedMedabotLargePreviewFrame`, `Fight_DrawRowBotSlotsExceptSelected`, `GetPreviewSlotXPosition`, `Slot_SetPosStepByMode`, `Preview_NudgeOffsetIfFlag`, `GpuBlend_TickStep`, `bios_cpufastset`. Chamada por: não verificado.
- Memória: lê `PTR_gFrameCount_0801402C`; escreve VRAM `0x05000000` e campos de estado via `PTR_DAT_08014030`/`PTR_DAT_08014034`/`DAT_0801404C`.
- Confiança: provável (tela de escolha de bot com fade de entrada/saída).

## 0x0801C188 Gfx_LoadAssetAndAdvance
- Assinatura: `void Gfx_LoadAssetAndAdvance(void)`
- O que faz: descomprime um asset (`decompress_and_store(DAT_0801C1AC, DAT_0801C1B0)`), limpa 8 words com CpuFastSet e incrementa o byte de estado `ctx+0x9C` (ctx = `DAT_0801C1BC`). Passo de carga de tela em sequência.
- Chama: `decompress_and_store`, `bios_cpufastset`. Chamada por: não verificado.
- Memória: lê ROM comprimida; escreve VRAM/buffer e `ctx+0x9C`.
- Confiança: confirmado no código; subsistema provável (padrão igual a Gfx_LoadTileAsset).

## 0x0801C4A4 Fight_RunEntryVisualStep
- Assinatura: `void Fight_RunEntryVisualStep(void)`
- O que faz: passo visual da entrada na luta: anima os frames de entrada (`Fight_AnimateEntryFrames`, byte `ctx+0x87`), anima sprites de efeito (`Fight_AnimateSideEffectSprites8`), refaz o texto do nome do bot (`Fight_LayoutBotNameText`) e a aparência (`Fight_UpdateBotVisual`) com o índice em `ctx+0x88`. Marca `ctx+0x9E=1` e incrementa `ctx+0x9C`.
- Chama: as quatro funções acima. Chamada por: não verificado (irmã de `Fight_RunEntryAnimStep`).
- Memória: ctx em `DAT_0801C4DC`; lê/escreve `+0x87`, `+0x88`, `+0x9C`, `+0x9E`.
- Confiança: provável.

## 0x08038258 Anim_SlideSlotX16
- Assinatura: `void Anim_SlideSlotX16(void)`
- O que faz: passo de deslize de um slot de 0x40 bytes escolhido pelo índice em `DAT_0803828C+DAT_08038288`. Se o byte +0x2D do slot for 0x46, incrementa +0x29 (fim de sequência); senão, incrementa o word +4 quando +0x2D é ímpar e move o X (+0x18) em ±0x10 conforme o bit 0 de `DAT_080382BC+0xF8`, aplicando máscara `DAT_080382EC`. Por fim incrementa um contador no slot indicado por `DAT_080382F0`.
- Chama: nenhuma função nomeada. Chamada por: não verificado.
- Memória: slots de 0x40 bytes a partir de `DAT_08038284`; lê `DAT_080382BC+0xF8` (flags de entrada/controle não resolvidos).
- Confiança: provável (padrão de slot de 0x40 bytes igual ao das funções de cena).

## 0x0803C33C Fight_ScrollEntryPhase
- Assinatura: `void Fight_ScrollEntryPhase(void)`
- O que faz: fase de rolagem da entrada. Usa o slot de 0x40 bytes indicado por um índice byte (`DAT_0803C36C+DAT_0803C370`). Se o byte +0x2B for zero, escolhe um de dois ramos pelo tipo em +0x25 (faixas 1..0xF e 0x11..0x1F): avança o tile (`Anim_AdvanceSlotTileOffset`), calcula o deslocamento de X (+2) e, quando a posição (+0x30) passa dos limiares (0x78, 0x160, 0x188), desloca as coordenadas de scroll (`Fight_ScrollEntryCoords`) e as posições de vários sprites. Se +0x2B > 0, decrementa-o (espera).
- Chama: `Anim_AdvanceSlotTileOffset`, `Fight_ScrollEntryCoords`. Chamada por: não verificado.
- Memória: slots de 0x40 bytes a partir de `DAT_0803C368` (não confirmado se é gObjectSlots); escreve coordenadas em `DAT_0803C3E4+0x154/0x158`; ponteiros `PTR_DAT_0803C45C..0803C53C` com posições de sprites.
- Confiança: provável (rolagem de entrada; valores de limiar lidos do código, significado do tipo +0x25 não confirmado).

## 0x0803CDCC Anim_StepSlotScript
- Assinatura: `void Anim_StepSlotScript(void)`
- O que faz: executa um passo do script de animação do slot: lê o índice de script em `+0x2D` do slot atual (slot = `DAT_0803CE10+DAT_0803CE14`, stride 0x40), chama `Anim_RunSlotScript` com o ponteiro da tabela `DAT_0803CE08`; se `+0x22 == -1`, limpa 1 slot de tile (`Tile_ClearSlots`).
- Chama: `Anim_RunSlotScript`, `Tile_ClearSlots`. Chamada por: não verificado.
- Memória: lê slot de 0x40 bytes em `DAT_0803CE0C`; tabela de ponteiros em `DAT_0803CE08`.
- Confiança: provável (a execução de script é confirmada pelo nome da chamada).

## 0x0803E634 FUN_0803E634
- Assinatura: `void FUN_0803E634(void)`
- O que faz: chama `StateMachine_DispatchStep` com o ponteiro em `DAT_0803E650+0x30`; se o passo devolver não zero, incrementa o contador em `DAT_0803E654`. Tick genérico de máquina de estados.
- Chama: `StateMachine_DispatchStep`. Chamada por: não verificado.
- Memória: lê ponteiro em `DAT_0803E650+0x30`; escreve contador em `DAT_0803E654`.
- Confiança: confirmado no código; subsistema não identificado. Nome mantido.

## 0x080418C0 BgMap_CopyRect8x6
- Assinatura: `void BgMap_CopyRect8x6(void)`
- O que faz: copia um retângulo de 8 × 6 entradas de 16 bits (linhas de 0x20 halfwords) de `DAT_0804191C` para o mapa de fundo, a partir do offset `ctx+0x2D*0x40 + ctx+0x2C*2` (base `DAT_08041918`). Depois incrementa `ctx+0x29` (ctx = `DAT_08041914`).
- Chama: nenhuma. Chamada por: não verificado.
- Memória: lê ROM/RAM em `DAT_0804191C`; escreve mapa de fundo (BG, bloco de 0x40 bytes por linha); escreve `ctx+0x29`.
- Confiança: confirmado no código (cópia de tilemap); uso de tela provável.

## 0x0804B008 Map_FinishEnterTransition
- Assinatura: `void Map_FinishEnterTransition(void)`
- O que faz: ao fim da transição de entrada no mapa (`Overworld_RunScreenTransitionStep` devolve não zero): grava o event DB base (`lit_EventDbBase3`), ajusta a posição do jogador (`ikki_x_to_tile`/`ikki_y_to_tile`, convertendo para tile e de volta a pixel ×0x10 com offsets por ponteiros), limita a posição ao tamanho do mapa (`Map_GetMapSizeHalfTileByAxis`), centraliza a câmera (`Map_CenterCameraForSmallMaps`), limpa o clamp da câmera (`Map_ClampCameraToLoadedMap`), carrega registro de marcador secundário (`Map_LoadSecondaryMarkerRecord`), define `REG_DISPCNT = 0x1E40`, limpa a sombra de OAM por DMA (`Map_ClearOamShadowViaDma`) e marca um flag em `DAT_0804B158`.
- Chama: `Overworld_RunScreenTransitionStep`, `ikki_x_to_tile`, `ikki_y_to_tile`, `Map_GetMapSizeHalfTileByAxis`, `Map_CenterCameraForSmallMaps`, `Map_ClampCameraToLoadedMap`, `Map_LoadSecondaryMarkerRecord`, `Map_ClearOamShadowViaDma`. Chamada por: não verificado.
- Memória: lê/escreve estado de mapa/jogador via ponteiros `DAT_0804B128..0804B158` (sem endereço fixo resolvido); `REG_DISPCNT` (0x04000000).
- Confiança: provável (chamadas nomeadas indicam entrada no mapa; `ikki` é o nome do jogador no Ghidra).

## 0x0805C0D4 FUN_0805C0D4
- Assinatura: `void FUN_0805C0D4(void)`
- O que faz: despacha uma função pela tabela `DAT_0805C114` indexada pelo byte em `DAT_0805C118` (`CallViaR0`), depois incrementa dois shorts (x/y) em `DAT_0805C11C`; se o bit 0 do byte em `DAT_0805C120` estiver ligado, incrementa mais dois shorts em `DAT_0805C124`. Padrão de avanço de posição por quadro.
- Chama: `CallViaR0` (trampolim) e a função da tabela. Chamada por: não verificado.
- Memória: lê byte `DAT_0805C118`, `DAT_0805C120`; escreve shorts em `DAT_0805C11C` e `DAT_0805C124`.
- Confiança: confirmado no código; subsistema não identificado (chute). Nome mantido.

## 0x0805CAF4 FUN_0805CAF4
- Assinatura: `void FUN_0805CAF4(void)`
- O que faz: chama `Anim_StepRecordCounter` no registro em `DAT_0805CB14+0x50`; quando ela devolve não zero, define o estado `DAT_0805CB18 = 3` e zera `DAT_0805CB1C`. Espera de contador e avanço de estado.
- Chama: `Anim_StepRecordCounter`. Chamada por: não verificado.
- Memória: lê/escreve estado em ponteiros `DAT_0805CB14..0805CB1C`.
- Confiança: confirmado no código; subsistema não identificado. Nome mantido.

## 0x0805D580 FUN_0805D580
- Assinatura: `void FUN_0805D580(void)`
- O que faz: tick de máquina de estados: `StateMachine_DispatchStep` no ponteiro em `DAT_0805D59C+0x20`; se não zero, incrementa o contador em `DAT_0805D5A0`.
- Chama: `StateMachine_DispatchStep`. Chamada por: não verificado.
- Confiança: confirmado no código (mesmo padrão de 0x0803E634). Nome mantido.

## 0x0805DB20 FUN_0805DB20
- Assinatura: `void FUN_0805DB20(void)`
- O que faz: tick de máquina de estados em `DAT_0805DB44+0x48`; ao terminar, chama `Fx_InitRecord8(1, 0x16, 7)` e incrementa o contador em `DAT_0805DB48`. Inicia um efeito ao fim de uma fase.
- Chama: `StateMachine_DispatchStep`, `Fx_InitRecord8`. Chamada por: não verificado.
- Confiança: confirmado no código; subsistema de efeitos provável (`Fx_`). Nome mantido.

## 0x0805EFFC Gfx_PaletteFadeStep
- Assinatura: `void Gfx_PaletteFadeStep(void)`
- O que faz: fade de paleta por quadros: enquanto o byte `ctx+0x84` < 9, chama `Palette_FadeRowsToColorA` e `Palette_FadeRowsToColorB` com passo `ctx+0x84 << 2` e incrementa o byte. Ao fim: ajusta `REG_DISPCNT` com máscara, limpa VRAM 0x06000000 (CpuFastSet), e conforme o byte em `DAT_0805F070` (0, 1 ou 2) copia registros por `bios_cpuset` e chama `FUN_080476E8(0|1)`, ou ajusta dois contadores (0x12, 10).
- Chama: `Palette_FadeRowsToColorA`, `Palette_FadeRowsToColorB`, `bios_cpufastset`, `bios_cpuset`, `FUN_080476E8`. Chamada por: transição de tela (não verificado).
- Memória: `REG_DISPCNT`; VRAM 0x06000000; `ctx+0x84` em `DAT_0805F030`.
- Confiança: provável (fade de paleta confirmado pelos nomes chamados).

## 0x0805FA18 Save_FinishFlashStep
- Assinatura: `void Save_FinishFlashStep(void)`
- O que faz: tick de máquina de estados em `DAT_0805FA4C+0x10`; ao terminar, para música (`snd_song_request(0)`) e SFX (`snd_sfx_request(0)`), chama `Save_InvokeFlashOp` (escrita na flash/save), zera um short em `*DAT_0805FA50+2` e incrementa `DAT_0805FA54`.
- Chama: `StateMachine_DispatchStep`, `snd_song_request`, `snd_sfx_request`, `Save_InvokeFlashOp`. Chamada por: não verificado.
- Memória: escreve o flag de save (short em `*DAT_0805FA50+2`).
- Confiança: provável (`Save_InvokeFlashOp` já nomeado no Ghidra). Ver também `Save_WaitSfxThenFlash`.

## 0x0806030C Screen_InitLoadAssets
- Assinatura: `void Screen_InitLoadAssets(void)`
- O que faz: inicialização de tela: define três bases de tilemap (`DAT_080603B4/BC/C0`, com −0xFF e valor fixo), carrega três blocos LZ77 de tiles (`Screen_LoadLZ77TileBlock`), descompacta um bloco (`Asset_UnpackOrCopyBlock` + `bios_cpuset`), limpa a paleta (CpuFastSet 0x38 words em 0x05000000), descompacta mais um asset (`decompress_and_store`) e preenche campos de sprite/controle no contexto `DAT_080603F8` (flags 0xC000, +0x0E=2, posições 0x112, 0x70, 0x18). Incrementa `DAT_080603FC`.
- Chama: `Screen_LoadLZ77TileBlock` (x3), `Asset_UnpackOrCopyBlock`, `bios_cpuset`, `bios_cpufastset`, `decompress_and_store`. Chamada por: transição de tela (não verificado).
- Memória: escreve paleta 0x05000000 e estado em `DAT_080603F8`.
- Confiança: provável (padrão de init de tela; ver também `Screen_InitLoadAssetsB`).

## 0x08061594 Text_RunMessageStep
- Assinatura: `void Text_RunMessageStep(void)`
- O que faz: copia uma string de texto até o terminador 0xFF (`Text_CopyUntilFF`) para um buffer de pilha (endereço escolhido pelo byte +5 de `*DAT_080615C4`, entre dois textos) e roda `StateMachine_DispatchStep` sobre esse buffer. Ao terminar, incrementa `DAT_080615D0`.
- Chama: `Text_CopyUntilFF`, `StateMachine_DispatchStep`. Chamada por: não verificado.
- Memória: lê texto (ponteiros `DAT_080615C8`/`DAT_080615CC`, não resolvidos); buffer de pilha de 48 bytes.
- Confiança: provável (mensagem com texto terminado em 0xFF; texto do jogo não copiado).

## 0x08062970 Shop_FadeInStep
- Assinatura: `void Shop_FadeInStep(void)`
- O que faz: fade de entrada por quadros: enquanto `ctx+0x84` < 9, chama `Palette_FadeRowsToColorA/B` com passo decrescente `0x20 - ctx+0x84*4`; ao fim, chama `Shop_BlinkCursorAttr`, zera um short em `*DAT_080629D0+2` e define `DAT_080629D4 = 0xC`, `DAT_080629D8 = 4`.
- Chama: `Palette_FadeRowsToColorA`, `Palette_FadeRowsToColorB`, `Shop_BlinkCursorAttr`. Chamada por: não verificado.
- Memória: `ctx+0x84` em `DAT_080629A8`; escreve estados em `DAT_080629D0..D8`.
- Confiança: provável (tela de loja pela função `Shop_` já nomeada).

## 0x080636FC Fight_PanelFadeStep
- Assinatura: `void Fight_PanelFadeStep(void)`
- O que faz: fade do painel de batalha: enquanto `ctx+0x84` < 0x41, chama `Palette_FadeRowsToColorB` com passo `0x20 - (ctx+0x84 >> 1)`; ao fim, duas cópias CpuFastSet de 8 words, zera `ctx+0x84`, incrementa `DAT_08063794` e grava 0x280 em `*ctx+0x3A`. Em todo quadro desenha o texto de dois slots (`BattlePanel_DrawSlotText`, bytes +0x31/+0x33 e +0x32/+0x34) e rola colunas de tiles (`Anim_ScrollTileColumns`).
- Chama: `Palette_FadeRowsToColorB`, `bios_cpufastset`, `BattlePanel_DrawSlotText` (x2), `Anim_ScrollTileColumns`. Chamada por: loop de batalha (não verificado).
- Memória: ctx em `DAT_08063798` (campos +0x31..+0x3A); `ctx+0x84` em `DAT_08063724`.
- Confiança: provável.

## 0x080645A0 FUN_080645A0
- Assinatura: `void FUN_080645A0(void)`
- O que faz: tick de máquina de estados cujo ponteiro sai de `DAT_080645FC + (0x68 ou 0x6C)`, escolhido pelo byte +0x7A do contexto em `DAT_08064600`. Quando termina, grava `DAT_08064608` (word) em `*(DAT_08064604+6)`; se os campos +0x0A e +0x0E do registro forem iguais a esse valor, incrementa `DAT_0806460C` e chama `Obj_InitTimerRecord(ctx+0x50, 0, 0, 1, 0xC0)`.
- Chama: `StateMachine_DispatchStep`, `Obj_InitTimerRecord`. Chamada por: não verificado.
- Confiança: confirmado no código; subsistema não identificado (chute). Nome mantido.

## 0x08064EB4 Screen_InitLoadAssetsB
- Assinatura: `void Screen_InitLoadAssetsB(void)`
- O que faz: inicialização de tela, variante de `Screen_InitLoadAssets`: três tilemaps (`DAT_0806501C/24/28`, valores `x`, `x-0xFF`, `x-0x1FE`), três blocos LZ77 (`Screen_LoadLZ77TileBlock`), um asset descompactado com `Asset_UnpackOrCopyBlock` + `bios_cpuset`, CpuFastSet de 0x400 bytes, dois `decompress_and_store`; configura 2 slots de sprite (de 0x40 bytes, a partir de `ctx+0x280`, ctx = `DAT_0806505C`) e escreve um número com `Num_FormatDecimalToGlyphs` + `Text_LayoutGlyphSprites` na posição (0x15,1). Incrementa `DAT_08065070`.
- Chama: `Screen_LoadLZ77TileBlock` (x3), `Asset_UnpackOrCopyBlock`, `bios_cpuset`, `bios_cpufastset`, `decompress_and_store` (x2), `Num_FormatDecimalToGlyphs`, `Text_LayoutGlyphSprites`. Chamada por: transição de tela (não verificado).
- Memória: contexto `DAT_0806505C`; lê o byte `*DAT_08065044+0x2E` para a posição do marcador.
- Confiança: provável.

## 0x08066534 Save_WaitSfxThenFlash
- Assinatura: `void Save_WaitSfxThenFlash(void)`
- O que faz: tick do registro em `DAT_0806656C+0x50` por `Anim_StepRecordCounter`; quando termina e o SFX 4 não está tocando (`snd_sfx_query_playing(4) == 0`), para música e SFX, chama `Save_InvokeFlashOp` e soma 2 ao contador `DAT_08066570`.
- Chama: `Anim_StepRecordCounter`, `snd_sfx_query_playing`, `snd_song_request`, `snd_sfx_request`, `Save_InvokeFlashOp`. Chamada por: não verificado.
- Confiança: provável (fluxo de salvamento após SFX).

## 0x08067F64 PartsInfo_InitScreen
- Assinatura: `void PartsInfo_InitScreen(void)`
- O que faz: inicialização da tela de informações de peça. Pega o registro de 0x40 bytes do item atual (`DAT_08068074 + índice*0x40`, índice em `DAT_08068078`), descompacta o asset escolhido por uma tabela de ponteiros (`decompress_and_store`, tabela `DAT_0806807C`, índice = byte +1 * 5 + (byte +8 − 1)), configura um sprite em `DAT_08068084` (posições 0x78, 0x28; flags 0x200; máscaras 0x40), copia uma string de até 16 bytes (terminada em 0xFE) de uma tabela de ponteiros (`DAT_08068090`) para `DAT_0806808C` e a desenha (`Text_LayoutGlyphSprites`). Se o byte +5 for 'd' (0x64) desenha quatro células fixas; senão desenha dois números de dois dígitos (`aux_div_u32` e `aux_mod_u32` por 10) nas entradas do mapa de fundo com tile base 0x7000. Liga as camadas BG (bits 0x200..0x1000 de `REG_DISPCNT`), zera campos de estado no contexto `DAT_08068214`, incrementa `DAT_08068230` e zera `DAT_08068234+0x84`.
- Chama: `decompress_and_store`, `Text_LayoutGlyphSprites`, `aux_div_u32`, `aux_mod_u32`. Chamada por: transição de tela (não verificado).
- Memória: `REG_DISPCNT`; mapa de fundo via ponteiros `DAT_08068098..08068224`; registros de item de 0x40 bytes a partir de `DAT_08068074`.
- Confiança: provável (tela de informações; nomes de texto/sprite confirmados, o conteúdo exato das duas formas de número não).

## 0x0806B2F4 Obj_UpdateSlotPosition
- Assinatura: `u8 Obj_UpdateSlotPosition(u32 slot)`
- O que faz: atualiza a posição de um objeto no bloco de 0x40 bytes indexado por `slot` (base `DAT_0806B374`). Soma a velocidade (tabela de 16 bits em `DAT_0806B378`, índice = byte +0x23) às posições de 8.8 em +0x0C e +0x10; converte para coordenada de tela em +2 (`>>7` para slots < 12, `>>8` para os demais, + 0x20) e +4 (0x10 − y). Se +0x1A > 8, recarrega +0x1A com a tabela `DAT_0806B3D0` >> 3 e marca +0x20 = 9. Retorna 1.
- Chama: nenhuma. Chamada por: não verificado.
- Memória: bloco de objeto de 0x40 bytes a partir de `DAT_0806B374`; tabelas de velocidade em `DAT_0806B378`/`DAT_0806B3D0`.
- Confiança: provável (física/posição de sprite; significado dos slots < 12 não confirmado).

## 0x0806D8AC Battle_SetupRenderSlots
- Assinatura: `void Battle_SetupRenderSlots(void)`
- O que faz: configura slots de renderização de objeto conforme o estado em `DAT_0806D8F4+8`: se 0, inicializa o slot 0 (`Map_InitializeObjectRenderSlot(0, 0xCC00, 0x224, 6, 3)`), posição (0x78, 0x3C), `battle_slot_init(1,0)` e liga `REG_DISPCNT |= 0x1000`; se 0x20, inicializa o slot 1 e `battle_slot_init(2,1)`. Depois, se `DAT_0806D980+0x20` for 0, roda `fx_screen_setup_a`, `slot_ptr_stub(1)`, mais um slot de render, liga `REG_DISPCNT |= 0x300` e marca `DAT_0806D990`; senão incrementa um contador.
- Chama: `Map_InitializeObjectRenderSlot`, `battle_slot_init`, `fx_screen_setup_a`, `slot_ptr_stub`. Chamada por: não verificado.
- Memória: `REG_DISPCNT`; contextos em `DAT_0806D8F4`, `DAT_0806D8F8`, `DAT_0806D980`, `DAT_0806D984`.
- Confiança: provável (setup de tela de batalha pelos nomes de slot).

## 0x0806E7EC Screen_InitLZ77ToVram
- Assinatura: `void* Screen_InitLZ77ToVram(void)`
- O que faz: descomprime dois blocos LZ77 para VRAM (`bios_lz77_vram`), depois monta em buffer de pilha 20 linhas de 0x1E halfwords somando 0x1000 (banco de paleta) às entradas copiadas; registra os ponteiros de tabela, liga `REG_DISPCNT |= 0x200` e define os campos `DAT_0806E900` (0x10) e `ctx+4 = 9` e `DAT_0806E908 = 3`.
- Chama: `bios_lz77_vram` (x2). Chamada por: não verificado.
- Memória: VRAM via LZ77; `REG_DISPCNT`; buffer de pilha; contexto `DAT_0806E8E0`.
- Confiança: provável.

## 0x08071164 Anim_StageDispatch
- Assinatura: `void Anim_StageDispatch(void)`
- O que faz: despachador de estágio de animação. Se o contador em `DAT_0807117C+8` for >= 0x10, habilita `enable_anim4(0x18, 0x3F, 0xC)`, zera o contador, define `+4 = 0xC` e marca `*DAT_080712C4 = 2` (fim). Senão chama o handler da tabela `DAT_08071180` indexada pelo contador (jump table não recuperada pelo Ghidra).
- Chama: `enable_anim4`, handler da tabela. Chamada por: não verificado.
- Confiança: provável (despachador; a tabela de handlers não foi lida).

## 0x08071F74 FUN_08071F74
- Assinatura: `void FUN_08071F74(void)`
- O que faz: passo de temporização: sobe `ctx+0x144` (+2 até 0x3E, depois +1 até 0x42); então conta `ctx+0x15A` até 0x20; depois desce `ctx+0x142` em −4 até 0x41. Quando desce abaixo de 0x41, chama `slot_ptr_stub(9)`, `snd_sfx_request(0x54)`, marca o byte `DAT_08071FE4` = 0x21 e incrementa `ctx+0x10` (de `DAT_08071FE8`), e termina chamando um ponteiro de jump table com 1 (ou 0 nos outros casos) — table não recuperada.
- Chama: `slot_ptr_stub`, `snd_sfx_request`, jump table não recuperada. Chamada por: não verificado.
- Memória: contexto `DAT_08071F90` (+0x142, +0x144, +0x15A); `DAT_08071FE8+0x10`.
- Confiança: chute quanto ao efeito (sequência de tempo com SFX 0x54). Nome mantido.

## 0x08074E24 Snd_StartSfxCounted
- Assinatura: `void Snd_StartSfxCounted(u32 sfx)`
- O que faz: inicia um efeito sonoro (`snd_sfx_start(param)`) e mantém um contador em `ctx+0x730` (ctx = `DAT_08074E5C`): incrementa até o limite `DAT_08074E60`, e depois do disparo decrementa se diferente de zero.
- Chama: `snd_sfx_start`. Chamada por: não verificado.
- Memória: contador word em `ctx+0x730`.
- Confiança: provável (controle de voz/limite de SFX pelo contador).

## 0x0807DB34 Math_Sf64CompareSafe
- Assinatura: `int Math_Sf64CompareSafe(u64 a, u64 b)` (valores de 64 bits como dois pares de words)
- O que faz: desempacota dois floats de 64 bits (`sf_unpack`) e, se algum tiver classe < 2 (zero, NaN ou infinito, conforme a biblioteca), devolve −1 (0xFFFFFFFF); senão devolve `sf64_cmp_core`.
- Chama: `sf_unpack` (x2), `sf64_cmp_core`. Chamada por: não verificado.
- Confiança: provável (biblioteca soft-float; classes não confirmadas nos nomes).

## Pendente
- 0x0806F5A0 (FUN_0806F5A0, 4920 bytes, refs=1): não documentado nesta rodada (função grande; decompile não feito por limite de contexto).
