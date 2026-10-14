# FL1 — 41 funções (faixa FL1 de FL1_lista.txt)

Lista: `FL1_lista.txt` (endereço, tamanho, refs). Ghidra: nomes `FUN_` renomeados nesta rodada; nomes já dados por
outra fonte apenas registrados no CSV. Confiança: confirmado (lido no código) | provável | chute.

## 0x08002A2C Menu_CursorStepAndConfirm
- Assinatura: `undefined4 Menu_CursorStepAndConfirm(void)`
- O que faz: a cada frame, roda o slot de animação `Anim_RunSlotScript(*DAT_08002AC0,0)`. Botões de direção
  (bit 0x40 / 0x80 de `*DAT_08002AC4`) mudam um índice de 0..3 (`& 3`, com `snd_sfx_request(2)`) e reposicionam um
  sprite (`x = idx*0x10 + 8`). Botão de confirmar (bit 2 / 1 de `*PTR_DAT_08002AD0`) limpa slots de tile
  (`Tile_ClearSlots`), toca fade/sfx e grava um código de estado em `*puVar2`.
- Chama: `Anim_RunSlotScript`, `snd_sfx_request`, `snd_fade_request`, `Tile_ClearSlots`. Chamada por: desconhecido
  (callback de estado de tela).
- Memória: lê/escreve ponteiros via `DAT_08002AC0..AD4` (IWRAM de estado do menu; endereços exatos nos literais).
  Escreve `ptr+0x8E` (índice do cursor) e `ptr+0x84`.
- Para o Lua: não precisa (áudio/tela); só a lógica de cursor, se for portado.
- Confiança: provável (nome de subsistema "Menu" por sfx e cursor; estrutura exata não confirmada).

## 0x080036F4 Menu_ResetSubstateByMode
- Assinatura: `undefined4 Menu_ResetSubstateByMode(void)`
- O que faz: copia 3 palavras de literais para o início de um bloco de estado (`DAT_08003728`). Depois lê o byte
  `obj+0x8F` do objeto em `DAT_08003738`: se 0, zera 0x14 bytes em `obj+0x50` e grava 3 no estado; se 1 ou 2, zera
  0x14 bytes em `obj+0x3C` e grava 4 ou 5 num código de substado, zerando `*DAT_08003780`.
- Chama: `Mem_FillBytes`. Chamada por: desconhecido (inicialização de modo de menu).
- Memória: RAM do objeto de menu (`obj+0x8F` = modo, `obj+0x50`/`+0x3C` = buffer de 0x14 bytes).
- Para o Lua: não precisa, a não ser que se porte a máquina de estados do menu.
- Confiança: provável (o modo 0/1/2 vem do byte +0x8F; significado dos códigos 3/4/5 não confirmado).

## 0x080093AC Shop_SnapshotQuantitiesTable
- Assinatura: `void Shop_SnapshotQuantitiesTable(void)`
- O que faz: copia 0x40 bytes de `gShopQuantities` para uma tabela de 0x40 bytes indexada por um byte de contexto
  (`PTR[0x3D] * 0x40` a partir de `PTR_DAT_080093D4`), e incrementa um contador (word) em `PTR_DAT_080093DC`.
- Chama: `Mem_CopyBytes`. Chamada por: desconhecido (salvamento de quantidades da loja).
- Memória: lê `gShopQuantities` (símbolo já no CSV); escreve tabela de 0x40 bytes por slot (IWRAM/SRAM, ver literal).
- Para o Lua: sim, se a loja for portada: é só cópia de 0x40 bytes por slot.
- Confiança: provável (nome do símbolo `gShopQuantities` vem do literal; slot e contador ainda por confirmar).

## 0x0800C804 Shop_HandleItemListAndCounts
- Assinatura: `undefined4 Shop_HandleItemListAndCounts(void)`
- O que faz: função de frame da tela de loja. Botões cima/baixo mudam um cursor de 0..2 (`obj+0x34`, `> ` wrap).
  Para cada um dos 3 cursores desenha uma linha de texto (`Mem_CopyBytes` de 8 bytes + `Text_LayoutGlyphSprites`),
  com 3 casas de quantidade formatadas (`Num_FormatDecimal6Glyphs`) em `(0x20, 0x23, 0x26)`. Botão A incrementa
  (`Map64_AddCountClamp99`) ou decrementa (`Map64_SubRemoveIfZero`) a contagem de um item (limite 0x40 por entrada,
  99 no clamp). Botão B limpa a área (`Tile_ClearSlots(8,0x21)`) e grava 1 em `*PTR_DAT_0800CB34`.
- Chama: `Mem_CopyBytes`, `Text_LayoutGlyphSprites`, `Tile_ClearSlots`, `Num_FormatDecimal6Glyphs`,
  `Map64_AddCountClamp99`, `Map64_SubRemoveIfZero`. Chamada por: desconhecido (loop de frame da loja).
- Memória: RAM de estado da loja (`obj+0x34` cursor, `obj+0x35..` texto), contagens do inventário Map64 (`PTR_0800CB28`).
- Para o Lua: sim, se a loja for portada (lógica de cursor e contagens de 0..99).
- Confiança: provável (nomes `Map64_*` vêm de outra fonte; papel "loja" inferido pelas quantidades e pelo snapshot).

## 0x0800D664 FUN_0800D664 (sem nome; não renomeado)
- Assinatura: `undefined4 FUN_0800D664(void)`
- O que faz: despachante. Lê o byte `*(ptr+0x33)` de um bloco de estado (`DAT_0800D680`), usa como índice de uma
  tabela de ponteiros de função (`DAT_0800D67C + idx*4`) e chama via `CallViaR0` (trampolim `bx r1`).
- Chama: alvo indexado (ver tabela no literal `DAT_0800D67C`). Chamada por: desconhecido.
- Memória: lê byte de estado em `DAT_0800D680+0x33`; a tabela não foi lida.
- Para o Lua: depende do alvo; o padrão (índice por byte, tabela de handlers) é portável.
- Confiança: confirmado (despacho); subsistema desconhecido: chute. Não renomeado.

## 0x0801053C Fight_ResetTemplateAndPalette
- Assinatura: `undefined4 Fight_ResetTemplateAndPalette(void)`
- O que faz: chama `Fight_BeginEncounterTemplateSelection`, zera o byte `+0x132` de um bloco de estado
  (`DAT_0801057C`), zera uma área por `bios_cpufastset` (fill com 0, contagem em `DAT_08010584`), zera a paleta
  (`0x05000000`, mesma contagem) e incrementa o contador de estado em `*DAT_08010588`.
- Chama: `Fight_BeginEncounterTemplateSelection`, `bios_cpufastset`. Chamada por: desconhecido (início de encontro).
- Memória: escreve paleta (0x05000000), área zerada via literal `DAT_08010580`, estado +0x132.
- Para o Lua: não precisa (BIOS e paleta); a parte do estado (+0x132 = 0) pode ser portada.
- Confiança: provável.

## 0x080125B4 Fight_AverageOverActiveBots
- Assinatura: `void Fight_AverageOverActiveBots(void)`
- O que faz: percorre 3 blocos de 0x100 bytes (a partir de `DAT_08012600 + DAT_08012604`). Para cada bloco com
  byte de flag `!= 0`, soma o byte no deslocamento `+0x305` (relativo à base `DAT_08012600+0x300`). Divide a soma
  por 3 (`aux_div_u32`) e soma o resultado a um halfword em `DAT_080125FC+0x16C`.
- Chama: `aux_div_u32` (endereço não conferido; o README dá `0x0807CEC8` como divisão sem sinal). Chamada por: desconhecido (cálculo por frame).
- Memória: lê blocos de bot (`0x03000590 + i*0x100` conforme README; base exata pelo literal); escreve halfword em
  `DAT_080125FC+0x16C`.
- Para o Lua: sim, se a fórmula for usada: é média de um campo sobre bots ativos.
- Confiança: provável (divisor 3 fixo e stride 0x100 lidos; significado do campo +0x305 não confirmado).

## 0x08012C64 Fight_BotSelectMenuFrame
- Assinatura: `undefined4 Fight_BotSelectMenuFrame(void)` (sem retorno útil)
- O que faz: função de frame de um menu de seleção de bot na luta. Desenha overlay dos bots ativos e anima as
  entradas (`Fight_DrawActiveBotsOverlay`, `Fight_AnimateEntryFrames`, `Sprite_UpdateComposite4Motion`), lê entrada
  horizontal (`Fight_HorizontalCursorInput`). Se o byte `+0x83` está ocupado, sai. Botão de cancelar (bit 2 de
  `*DAT_08012CC8`): recua o cursor `+0x80` (`- 1`), toca sfx 5, reescreve sprites e limpa flags; estado vai a 4.
  Botão de confirmar (bit 1): toca sfx 4, chama `FUN_08017914`, zera `+0x84` e avança o estado.
- Chama: `Fight_DrawActiveBotsOverlay`, `Fight_AnimateEntryFrames`, `Sprite_UpdateComposite4Motion`,
  `Fight_HorizontalCursorInput`, `FUN_080178CC`, `FUN_0801630C`, `Fight_RefreshBotDisplayedPartSprites`,
  `Fight_FillSideLabelBandA`, `Fight_ClearSlotFlag44`, `Tile_ClearSlots`, `BgMap_FillRect16`.
  Chamada por: desconhecido (estado do menu de luta).
- Memória: bloco de estado da luta (`DAT_08012CC4`: cursor `+0x80`, `+0x81`, `+0x82`, `+0x83`, `+0x84`).
- Para o Lua: sim (lógica de cursor e transição de estado), o resto é desenho.
- Confiança: provável (cursor e sfx confirmados; significado de bits A/B pela ordem dos testes, não confirmado no
  mapa de botões).

## 0x08013AE4 Fight_MarkSelectedOptionFrame
- Assinatura: `undefined4 Fight_MarkSelectedOptionFrame(void)`
- O que faz: frame de um menu de 3 opções da luta. Desenha overlay de bots, anima entradas
  (`Fight_AnimateEntryFrames`, `Sprite_UpdateComposite4Motion`). Lê o cursor `+0x80` (0, 1 ou 2) e faz OR no byte
  `+0x44` do bloco de estado com o bit 1, 2 ou 4 (um bit por opção). Depois limpa tiles (`Tile_ClearSlots`) e
  preenche o mapa de fundo (`BgMap_FillRect16`), e grava o estado 0xD em `*DAT_08013B88`.
- Chama: `Fight_DrawActiveBotsOverlay`, `Fight_AnimateEntryFrames`, `Sprite_UpdateComposite4Motion`,
  `Tile_ClearSlots`, `BgMap_FillRect16`. Chamada por: desconhecido (estado de escolha da luta).
- Memória: bloco de estado da luta (`DAT_08013B14`: cursor `+0x80`, marcação `+0x44`).
- Para o Lua: sim (marcação por bit e troca de estado).
- Confiança: provável (OR dos bits e estado 0xD lidos; significado das 3 opções não confirmado).

## 0x08014E20 Fight_OverlayTimerAdvance
- Assinatura: `undefined4 Fight_OverlayTimerAdvance(void)`
- O que faz: incrementa o temporizador `short` em `DAT_08014E48+0x86` a cada frame (o mesmo `+0x86` do HANDOFF,
  bloco `0x03000440`), chama `Overlay_SetTargetAndCheckReached`. Quando chega ao alvo, zera o short em
  `DAT_08014E4C+6` e incrementa o estado em `*DAT_08014E50`.
- Chama: `Overlay_SetTargetAndCheckReached`. Chamada por: desconhecido (passo de espera de um handler).
- Memória: timer `+0x86` (RAM, bloco do handler), short `+6` do bloco de overlay, estado em `DAT_08014E50`.
- Para o Lua: sim (espera por frames e avanço de estado).
- Confiança: provável (timer +0x86 bate com HANDOFF_DISCOVERY; nome de subsistema "Fight" pela cadeia).

## 0x0801C444 Fight_SideEffectStepAdvance
- Assinatura: `void Fight_SideEffectStepAdvance(void)`
- O que faz: anima entradas de um lado (`Fight_AnimateEntryFrames` com o byte `+0x87`), depois chama
  `Fight_StepSideEffectSprites8`. Se esse passo devolve diferente de 0, incrementa o byte `+0x9C` do bloco de estado.
- Chama: `Fight_AnimateEntryFrames`, `Fight_StepSideEffectSprites8`. Chamada por: desconhecido.
- Memória: bloco de estado (`DAT_0801C46C`: `+0x87`, `+0x9C`).
- Para o Lua: sim (avanço de contador de passo).
- Confiança: provável.

## 0x08037E2C Fight_EffectSpriteAnimStep
- Assinatura: `void Fight_EffectSpriteAnimStep(void)`
- O que faz: passo de animação de um efeito de luta. Escolhe um registro de 0x40 bytes pelo índice `*pbVar5`
  (base `DAT_08037E70`), roda o script de slot (`Anim_RunSlotScript`). Se o byte `+0x2D` é 0xFF, só incrementa
  `+0x29` (espera). Senão, quando `+0x2D & 0x3F == 0` toca um sfx conforme o modo `fight_information+0x51`
  (0 → 0x5D, 1 → 0x2E, 2 → 0x4B). Depois atualiza `+0x2B` por tabela (`DAT_08037F48`, índice `(+0x2D & 0x1F)*2`),
  move `+0x18` em ±0x10 (máscara `DAT_08037FD4`/`DAT_08037F50`, só se `+0x51 == 0`), incrementa `+0x2C`, calcula a
  palavra de offset (`CompositeSprite_ComputeOffsetWord`), chama `FightPreview_SpawnEffectSpriteAlt` e incrementa `+0x2D`.
- Chama: `Anim_RunSlotScript`, `snd_sfx_request`, `CompositeSprite_ComputeOffsetWord`, `FightPreview_SpawnEffectSpriteAlt`.
  Chamada por: desconhecido (efeitos de ataque/status).
- Memória: registros de efeito de 0x40 bytes a partir de `DAT_08037E70+0x2E` (lista de índices); modo em
  `fight_information+0x51` (0..3, conforme HANDOFF).
- Para o Lua: sim (animação de efeito por passos e sfx por modo).
- Confiança: provável (estrutura de 0x40 bytes e modo +0x51 lidos; nomes exatos dos campos não confirmados).

## 0x0803B900 Fight_EffectEntryTriggerStep
- Assinatura: `void Fight_EffectEntryTriggerStep(void)`
- O que faz: passo de um registro de efeito de 0x40 bytes (índice `*pbVar5`, base `DAT_0803B940`). Se o byte `+0x2B`
  está marcado: consome a marca (zera `+0x2B`), lendo um flag do bloco de estado (`DAT_0803B94C+DAT_0803B950` se o
  índice é 1; senão `DAT_0803B990+0x160`); se o flag está zerado, sai sem fazer nada. Em seguida incrementa `+0x29`
  e, se `+0x2D != 0`, incrementa mais uma vez (contador de passos).
- Chama: nada externo. Chamada por: desconhecido (passo de efeito, par de `Fight_EffectSpriteAnimStep`).
- Memória: registros de efeito (`+0x29` contador, `+0x2B` marca, `+0x2D` fase); flags em `DAT_0803B990+0x160`.
- Para o Lua: sim (máquina de passos de efeito).
- Confiança: provável.

## 0x0803CC40 Fight_EffectSlotScriptStep
- Assinatura: `void Fight_EffectSlotScriptStep(void)`
- O que faz: roda o script de slot de animação do registro de efeito atual (índice `*pbVar3`, offset `+0x2D` via
  tabela `DAT_0803CC7C`). Se o byte `+0x22` do mesmo registro vale 0xFF, libera o slot de tile (`Tile_ClearSlots(idx,1)`).
- Chama: `Anim_RunSlotScript`, `Tile_ClearSlots`. Chamada por: desconhecido.
- Memória: registro de efeito de 0x40 bytes (`+0x22`, `+0x2D`), tabela de ponteiros de script.
- Para o Lua: sim (dispara script e libera slot).
- Confiança: provável.

## 0x0803DCDC Overlay_InitScreenGraphics
- Assinatura: `undefined4 Overlay_InitScreenGraphics(void)`
- O que faz: inicialização de tela do overlay. Limpa 4 pares de slots (`Slots_ClearFourPairs`), configura blend
  (`SetGpuBlendMode(5,0x3F,0x20)`), ajusta `REG_DISPCNT` (AND e depois OR com `0x1F60`). Descompacta 5 conjuntos de
  gráficos (`decompress_and_store`), carrega tiles (`LoadTilesWithMalias` x4, 0x200 cada), copia paleta
  (`bios_cpufastset` em `0x05000000`, 0x60 palavras), zera uma área e inicializa `Overlay_CopySixBytesFromRandomTable`,
  `Overlay_DrawSixDigitRow`, OAM slots 2..4. Zera `DAT_0803DE68+0x1A0`, pede a música `snd_song_request(0x14)` e
  incrementa o estado em `*DAT_0803DE6C`.
- Chama: `Slots_ClearFourPairs`, `SetGpuBlendMode`, `decompress_and_store`, `LoadTilesWithMalias`,
  `bios_cpufastset`, `Overlay_*`, `snd_song_request`. Chamada por: desconhecido (entrada do overlay).
- Memória: REG_DISPCNT (0x04000000), paleta 0x05000000, VRAM via `LoadTilesWithMalias`, estado em `DAT_0803DE6C`.
- Para o Lua: não precisa (hardware); só a ordem de passos e a troca de estado.
- Confiança: provável (o conjunto de chamadas é claro; "overlay" é o nome que o Ghidra já dá aos auxiliares).

## 0x0804169C Overlay_LoadTileSetsAndAdvance
- Assinatura: `undefined4 Overlay_LoadTileSetsAndAdvance(void)`
- O que faz: descompacta 3 conjuntos (`decompress_and_store`), carrega tiles em dois blocos de fundo (`LoadTilesWithMalias`
  com bloco 0x15 e 0x16), copia 8 bytes de paleta (`bios_cpufastset`, 8 palavras) e incrementa o byte `+0x29` do bloco
  de estado `DAT_08041728` (passo seguinte).
- Chama: `decompress_and_store`, `LoadTilesWithMalias`, `bios_cpufastset`. Chamada por: desconhecido (passo de carga do overlay,
  parte de `Overlay_InitScreenGraphics`).
- Memória: VRAM (blocos de tile 0x15/0x16), paleta, estado `+0x29`.
- Para o Lua: não precisa (carga de gráficos).
- Confiança: provável.

## 0x08057B0C Map_ExitStateDispatchFlow
- Assinatura: `void Map_ExitStateDispatchFlow(void)`
- O que faz: saída de uma tela de mapa. Se o byte de estado `ctx+0x65` não é 2, limpa a sombra de OAM por DMA
  (`Map_ClearOamShadowViaDma`) e libera slots de marcador (`Map_ReleaseMarkerRenderSlots`). Sempre reseta o slot de
  objeto 0x7F. Depois escolhe o próximo fluxo pelo estado: 0 → `GameState_StartFlowByCommand(0x41)`, 1 → 0x43,
  3 → 0x42; estado 2 grava 5 em `*DAT_08057B54`.
- Chama: `Map_ClearOamShadowViaDma`, `Map_ReleaseMarkerRenderSlots`, `Map_ResetObjectRenderSlot`,
  `GameState_StartFlowByCommand`. Chamada por: desconhecido (fim de estado do mapa).
- Memória: estado em `DAT_08057B34+0x65`; grava `*DAT_08057B54`.
- Para o Lua: sim (escolha do próximo fluxo pelo estado); o resto é hardware/OAM.
- Confiança: provável (códigos de fluxo 0x41/0x42/0x43 lidos; significado não confirmado).

## 0x0805CA3C Fx_RunStateStepAndInit
- Assinatura: `undefined4 Fx_RunStateStepAndInit(void)`
- O que faz: copia um registro de 0x53 (83) bytes de estado (`lib_memcpy` de `DAT_0805CA6C`) para a pilha, chama
  `StateMachine_DispatchStep` com ele. Quando o passo devolve diferente de 0, chama `Fx_InitRecord8(1,0x16,7)` e
  incrementa o estado em `*DAT_0805CA70`.
- Chama: `lib_memcpy`, `StateMachine_DispatchStep`, `Fx_InitRecord8`. Chamada por: desconhecido (efeito de tela).
- Memória: registro de 0x53 bytes em `DAT_0805CA6C` (cópia de trabalho na pilha); estado em `DAT_0805CA70`.
- Para o Lua: sim (máquina de estados do efeito).
- Confiança: provável.

## 0x0805CFB4 Shop_ItemListInputStep
- Assinatura: `void Shop_ItemListInputStep(void)`
- O que faz: passo de entrada da lista de itens da loja. Usa `(ctx+0x10 - 1) / 5` (páginas de 5 linhas, por
  `aux_div_s32`) e reposiciona o cursor. Para cima/baixo, muda índice (`+0x0C`, `+0x11`) com salto de página
  (5 em 5), toca sfx 2, redesenha a página (`Shop_DrawItemListPage`) e a marca de prioridade. Botões de lateral
  alteram `+0x12`, chamam `Shop_DrawSelectedNameText` e `Shop_BuildItemList64`. Para o item selecionado lê a tabela
  `gShopItems` (entradas de 4 bytes; id em halfword) e chama `FUN_08048960`: se devolve 1, toca sfx 4 e despacha
  pela tabela de ações (`PTR_switchdata...`). Se devolve 0 toca sfx 6; se 2..4 zera o estado e vai a 9. B (bit
  `*DAT_0805D460 & 2`) toca sfx 5, copia buffers de paleta e vai ao estado 7.
- Chama: `aux_div_s32`, `aux_div_u32`, `aux_mod_u32`, `Shop_DrawItemListPage`, `Shop_DrawSelectedNameText`,
  `Shop_BuildItemList64`, `Menu_UpdateSpritePriorityByRange`, `Screen_SetSlotHighlight`, `Slot_ActivateCursor`,
  `FUN_08048960`, `FUN_080476E8`, `CopyPaletteBuffersToHardware`, `snd_sfx_request`. Chamada por: desconhecido
  (loop da loja).
- Memória: `gShopItems` (entradas de 4 bytes), estado `DAT_0805D088` (+0x0C cursor, +0x10 contagem, +0x11 índice,
  +0x12 flag de nome), `DAT_0805D254` (estado do item).
- Para o Lua: sim (lista paginada, cursor e ações).
- Confiança: provável (estrutura de loja: `gShopItems` e `Shop_*` vêm dos nomes dos auxiliares; ramo de ação não lido).

## 0x0805DA14 Gfx_FadeInStepAndEnableBg
- Assinatura: `undefined4 Gfx_FadeInStepAndEnableBg(void)`
- O que faz: fade de entrada em 9 passos (estado `+0x84` < 9): chama `Palette_FadeRowsToColorA` e
  `Palette_FadeRowsToColorB` com fator `0x20 - 4*passo`, e incrementa o passo. No primeiro passo liga os bits
  0x200, 0x400, 0x800 e 0x1000 em `REG_DISPCNT` (layers BG). Ao chegar em 9, chama `Obj_InitSlotAndLoad(1,2,0)` e
  incrementa o estado em `*DAT_0805DAAC`.
- Chama: `Palette_FadeRowsToColorA`, `Palette_FadeRowsToColorB`, `Obj_InitSlotAndLoad`. Chamada por: desconhecido.
- Memória: `REG_DISPCNT` (0x04000000), estado `DAT_0805DA88+0x84`, estado em `DAT_0805DAAC`.
- Para o Lua: não precisa (hardware); a curva de fade pode ser descrita.
- Confiança: provável.

## 0x0805E9E0 Gfx_FadeStepAndResetFields
- Assinatura: `undefined4 Gfx_FadeStepAndResetFields(void)`
- O que faz: fade em 9 passos (estado `+0x84` < 9) com `Palette_FadeRowsToColorA`/`B` e fator `0x20 - 4*passo`,
  incrementando o passo. No passo final zera o short `+2` e o byte `+6` do bloco apontado por `DAT_0805EA3C`, e
  incrementa o estado em `*DAT_0805EA40`.
- Chama: `Palette_FadeRowsToColorA`, `Palette_FadeRowsToColorB`. Chamada por: desconhecido.
- Memória: estado `DAT_0805EA18+0x84`; bloco `DAT_0805EA3C` (+2, +6); estado em `DAT_0805EA40`.
- Para o Lua: não precisa (paleta); a ordem de passos pode ser descrita.
- Confiança: provável (mesma curva que `Gfx_FadeInStepAndEnableBg`; sentido do fade não confirmado).

## 0x0805F960 Fx_StepStateMachineAdvance
- Assinatura: `undefined4 Fx_StepStateMachineAdvance(void)`
- O que faz: chama `StateMachine_DispatchStep` com o ponteiro em `DAT_0805F97C+0xC`. Se devolve diferente de 0,
  incrementa o estado em `*DAT_0805F980`. É a versão sem cópia de `Fx_RunStateStepAndInit`.
- Chama: `StateMachine_DispatchStep`. Chamada por: desconhecido (efeito).
- Memória: `DAT_0805F97C+0xC` (ponteiro de máquina de estados), estado em `DAT_0805F980`.
- Para o Lua: sim (passo de máquina de estados).
- Confiança: provável.

## 0x080601D4 Fx_ScrollTileBandStep
- Assinatura: `void Fx_ScrollTileBandStep(void)`
- O que faz: efeito de rolagem. Incrementa o short `+0x3A` do bloco `*DAT_08060238` e chama `Screen_ScrollTileBand(+0x3A + 8)`.
  Incrementa um contador `ushort` em `DAT_0806023C+0x5A` com volta em 0x48, e define o short `+0x46` com a entrada
  `contador/6` de uma tabela (`DAT_08060240`, `aux_div_u32`). Quando `+0x3A > 0xD` zera o byte do bloco e decrementa
  o contador em `DAT_08060244`.
- Chama: `Screen_ScrollTileBand`, `aux_div_u32`. Chamada por: desconhecido (efeito de faixa de tela).
- Memória: blocos de efeito (`+0x3A`, `+0x46`, `+0x5A`), tabela de 8 halfwords em `DAT_08060240` (não lida).
- Para o Lua: sim (oscilação por tabela); o resto é hardware de faixa.
- Confiança: provável.

## 0x08061468 Gfx_FadeOutStepAndDisableBg
- Assinatura: `undefined4 Gfx_FadeOutStepAndDisableBg(void)`
- O que faz: fade em 9 passos com `Palette_FadeRowsToColorA`/`B` e fator `passo << 2` (sentido contrário ao de
  `Gfx_FadeInStepAndEnableBg`). No passo final limpa bits de `REG_DISPCNT` (três AND com `DAT_080614D0/D4/D8`),
  zera slots de tile (`Tile_ClearSlots(0,0x80)`) e chama `FUN_080476E8(3)`.
- Chama: `Palette_FadeRowsToColorA`, `Palette_FadeRowsToColorB`, `Tile_ClearSlots`, `FUN_080476E8`. Chamada por: desconhecido.
- Memória: `REG_DISPCNT` (0x04000000), estado `DAT_08061498+0x84`.
- Para o Lua: não precisa (hardware); a sequência é portável.
- Confiança: provável.

## 0x08062614 Shop_CommitBuySellTransaction
- Assinatura: `void Shop_CommitBuySellTransaction(void)`
- O que faz: confirma uma transação de loja quando a máquina de estados (`StateTable_DispatchByIndex`) termina. Zera
  `+0x84` e `+0x1B` do bloco. Se o byte `+0x2A` do pedido é 0 e o modo `(+0x05)` é 0 (compra): subtrai o preço
  (`+0x1C`) do dinheiro (`DAT_08062690`) e soma `gShopQuantities[i]` ao inventário (`gInventory`) para 0x78 entradas,
  indexadas por `gShopItems` (id a cada 8 bytes) e pela classe `+0x19` (passo 0xF0). Se o modo é diferente de 0
  (venda): soma o preço ao dinheiro com teto (`DAT_080626B4` → `DAT_0806273C`) e subtrai as quantidades do
  inventário. Ao fim limpa a máscara de atributo (`Slot_ClearAttrMask`), desenha o número (`Num_FormatDecimalToGlyphs`)
  e incrementa o estado em `*PTR_DAT_08062750`.
- Chama: `StateTable_DispatchByIndex`, `Slot_ClearAttrMask`, `Num_FormatDecimalToGlyphs`, `Text_LayoutGlyphSprites`.
  Chamada por: desconhecido (passo de transação da loja).
- Memória: dinheiro em `DAT_08062690`, `gInventory` (`PTR_gInventory`), `gShopItems`, `gShopQuantities`, pedido com
  preço `+0x1C` e classe `+0x19`.
- Para o Lua: sim (regra de compra e venda: dinheiro, inventário, teto).
- Confiança: provável (aritmética de compra/venda e uso de `gInventory` lidos; o significado de +0x05 é chute de compra/venda).

## 0x0806355C Menu_SwapTileBlocksAndLabels
- Assinatura: `void Menu_SwapTileBlocksAndLabels(void)`
- O que faz: troca 3 pares de blocos de tile de 0x20 bytes (três `bios_cpufastset` de 8 palavras com buffer de
  32 bytes, troca a/b). Marca o campo `+0x3C = 1` no objeto de `PTR_DAT_0806363C`, limpa slots (`Tile_ClearSlots(0,0x80)`),
  copia duas strings de fonte (`Text_CopyFEStringTo20`, índices a partir de `+0x31..+0x34`), desenha os textos
  (`Text_LayoutGlyphSpritesTileBase`), toca sfx 0x2A e rola colunas (`Anim_ScrollTileColumns`).
- Chama: `bios_cpufastset`, `Tile_ClearSlots`, `Text_CopyFEStringTo20`, `Text_LayoutGlyphSpritesTileBase`,
  `snd_sfx_request`, `Anim_ScrollTileColumns`. Chamada por: desconhecido.
- Memória: VRAM de tiles (blocos de 0x20), objeto com `+0x3C`, contador em `PTR_DAT_08063638`.
- Para o Lua: não precisa (VRAM); a ordem de textos pode ser descrita.
- Confiança: provável na mecânica; o subsistema (menu) é chute.

## 0x0806412C Fx_NumberRevealStep
- Assinatura: `void Fx_NumberRevealStep(void)`
- O que faz: efeito de revelação de número em 16 passos. Chama `Anim_ScrollTileRows` e incrementa o short `+0x3C` do
  bloco `*DAT_08064174`. Nos passos 1..7 ajusta bytes de offset de sprite (`-8*passo + 0x40`, `4*passo + 0x38`). Ao
  passar de 0x10, incrementa `DAT_08064178`. Se o byte `+0x7A` é 0, limpa uma máscara de atributo; senão, desenha
  um texto de 10 caracteres (copiado de uma tabela de ponteiros) e escreve dígitos decimais do valor (`aux_div_u32`
  e `aux_mod_u32` por 10) em tiles de 2 linhas com paleta 0x7000. Trata o caso especial de caractere `'d'`.
- Chama: `Anim_ScrollTileRows`, `Text_LayoutGlyphSprites`, `aux_div_u32`, `aux_mod_u32`, `bios_cpufastset`. Chamada por: desconhecido.
- Memória: bloco de efeito (`+0x3C`, `+0x7A`), tilemap (`0x0600xxxx` por endereços de literal), `DAT_080642B4`, `DAT_080643E4`.
- Para o Lua: sim (passos de revelação e dígitos decimais); o resto é tilemap.
- Confiança: provável (dígitos por /10 e %10 e passos 0x10 lidos; intenção do efeito não confirmada).

## 0x08064B90 FUN_08064B90 (sem nome; não renomeado)
- Assinatura: `undefined4 FUN_08064B90(void)`
- O que faz: despachante. Copia dois words de um bloco de estado (`DAT_08064BB4`) para uma pilha local, indexa a
  tabela com o byte em `DAT_08064BB8` e chama via `CallViaR0`.
- Chama: alvo indexado por byte. Chamada por: desconhecido.
- Memória: lê `DAT_08064BB4` e `DAT_08064BB8` (índice).
- Para o Lua: depende do alvo; padrão de tabela por índice.
- Confiança: confirmado (despacho); subsistema desconhecido: chute. Não renomeado.

## 0x08066280 Menu_SelectionFiveOptionsStep
- Assinatura: `void Menu_SelectionFiveOptionsStep(void)`
- O que faz: passo de um menu de até 5 opções (cursor em `obj+5`). Primeiro verifica uma entrada marcada com
  `0xEF00` na tabela (`PTR_DAT_080662DC`): se achou, grava o código no campo `+5` do bloco, marca `+0x7A = 1`,
  incrementa um contador, toca sfx 4 e reposiciona o cursor (`+5*0x10 + 0x1C`). Senão, despacha a sub-máquina
  (`StateMachine_DispatchStep`, índice por `+5`; valor 6 usa um ponteiro fixo). Com o cursor entre 0 e 4: botão
  (bit 0x40) sobe com volta em 4, (bit 0x80) desce com volta em 0, ambos com `Obj_ResetStateFields` e sfx 2. Botão de
  confirmar: se a opção é 1 e `+0x7B == 0` vai para 5; se é 2 sem `+0x7C`, marca `0xEF00 | código` e toca sfx 4.
- Chama: `Slot_SetPosStepByMode`, `StateMachine_DispatchStep`, `Obj_ResetStateFields`, `snd_sfx_request`.
  Chamada por: desconhecido.
- Memória: bloco de menu (`+5` cursor, `+0x7A`, `+0x7B`, `+0x7C`), tabela de marcas em `DAT_080662DC`.
- Para o Lua: sim (cursor de 5 opções e marcação).
- Confiança: provável (cursor 0..4 e marca 0xEF00 lidos; significado das opções não confirmado).

## 0x08067B9C FUN_08067B9C (sem nome; não renomeado)
- Assinatura: `undefined4 FUN_08067B9C(void)`
- O que faz: copia 7 words de um bloco (`DAT_08067BD0`) para a pilha, chama a entrada indexada por byte
  (`DAT_08067BD4`) via `CallViaR0`, e decrementa o short `+2` do bloco `DAT_08067BD8` (temporizador).
- Chama: alvo indexado. Chamada por: desconhecido.
- Memória: `DAT_08067BD0` (7 words), `DAT_08067BD4` (índice), `DAT_08067BD8+2` (timer).
- Para o Lua: depende do alvo; padrão de tabela com temporizador.
- Confiança: provável (despacho com decremento); subsistema desconhecido. Não renomeado.

## 0x08068B74 FUN_08068B74 (sem nome; não renomeado)
- Assinatura: `undefined8 FUN_08068B74(void)`
- O que faz: auxiliar. Lê um inteiro com sinal (`*DAT_08068BAC`) e um divisor de byte (`*DAT_08068BB0`). Se o
  resto (`aux_mod_s32`) é 0, grava o quociente (`aux_div_s32`, halfword) em `DAT_08068BB4`. Devolve se o valor é igual
  a `divisor*0x10`.
- Chama: `aux_mod_s32`, `aux_div_s32`. Chamada por: desconhecido.
- Memória: lê `DAT_08068BAC`, `DAT_08068BB0`; escreve `DAT_08068BB4`.
- Para o Lua: sim, se a regra for usada (divisão exata e comparação).
- Confiança: confirmado (aritmética lida); significado no jogo: chute. Não renomeado.

## 0x08068EA8 Fx_StepTableBThenSetDispcnt
- Assinatura: `undefined4 Fx_StepTableBThenSetDispcnt(void)`
- O que faz: chama `fx_step_table_b()`. Se devolve diferente de 0, aponta um word de `DAT_08068EE8` para uma variável
  local, grava duas constantes em `DAT_08068EE8+4` e `+8`, ajusta a máscara de `REG_DISPCNT` (AND com `DAT_08068EFC`)
  e copia um word de `DAT_08068F04` para `DAT_08068F00`.
- Chama: `fx_step_table_b`. Chamada por: desconhecido (efeito).
- Memória: `REG_DISPCNT` (0x04000000), blocos de `DAT_08068EE8` e `DAT_08068F00`.
- Para o Lua: não precisa (hardware), exceto a ordem de passos.
- Confiança: provável.

## 0x0806D600 Fx_StepTableBThenEnterScene
- Assinatura: `undefined4 Fx_StepTableBThenEnterScene(void)`
- O que faz: chama `fx_step_table_b()`. Se devolve diferente de 0: zera um word local e aponta `DAT_0806D670` para
  ele, grava duas constantes, ajusta `REG_DISPCNT` (AND), copia três halfwords de `PTR_DAT_0806D684` para
  `gMainState` e para dois outros ponteiros, limpa duas máscaras de bits e pede a música 0 (`snd_song_request(0)`).
- Chama: `fx_step_table_b`, `snd_song_request`. Chamada por: desconhecido (entrada de cena).
- Memória: `REG_DISPCNT`, `gMainState` (via `PTR_gMainState_0806D680`), blocos de `DAT_0806D670`.
- Para o Lua: sim (troca de cena e estado principal); a parte de hardware não.
- Confiança: provável (mesmo padrão de `Fx_StepTableBThenSetDispcnt`).

## 0x0806E5E0 Gfx_LoadLz77ScreenAndEnableBg0
- Assinatura: `undefined4 Gfx_LoadLz77ScreenAndEnableBg0(void)`
- O que faz: descomprime duas imagens LZ77 (`bios_lz77_vram`): uma para VRAM em `0x06000000` e outra para um buffer
  da pilha. Copia 20 linhas de 0x3C bytes do buffer (com 2 halfwords zerados por linha) para outro buffer. Configura
  dois ponteiros de DMA/cópia, define `REG_DISPCNT |= 0x100` (liga BG0), escreve 5 em `iVar4+4` e 1 em `*puVar5`.
  Devolve um word do buffer da pilha.
- Chama: `bios_lz77_vram`. Chamada por: desconhecido (carga de tela).
- Memória: VRAM 0x06000000, `REG_DISPCNT` (0x04000000), buffers da pilha, `DAT_0806E6C8/CC/D0..D8`.
- Para o Lua: não precisa (descompressão para VRAM); a tela pode ser reproduzida por imagem.
- Confiança: provável (LZ77 e BG0 lidos; conteúdo das imagens não identificado).

## 0x0806F114 FightPreview_SetupCompositeScreen
- Assinatura: `undefined4 FightPreview_SetupCompositeScreen(void)`
- O que faz: inicializa buffers de composição de preview (`FightPreview_InitCompositeBuffers`), monta uma camada de sprite
  composta (`BuildCompositePreviewSpriteLayer` com 0x2B, 0x78, 0x50 e outros), chama `fx_screen_setup_a(2,4,0)`,
  liga OBJ em `REG_DISPCNT |= 0x1000`, grava 0xF em `DAT_0806F1B8+4` e marca `*DAT_0806F1BC = 1`.
- Chama: `FightPreview_InitCompositeBuffers`, `BuildCompositePreviewSpriteLayer`, `fx_screen_setup_a`. Chamada por: desconhecido.
- Memória: `REG_DISPCNT` (0x04000000), buffers de sprite em `DAT_0806F19C`, flag em `DAT_0806F1BC`.
- Para o Lua: sim se for reproduzido o preview; a montagem é de hardware.
- Confiança: provável (chamadas com nomes de preview lidas; o que o preview mostra não confirmado).

## 0x080710F4 Anim_StartSlotsIfNotDone
- Assinatura: `undefined4 Anim_StartSlotsIfNotDone(void)`
- O que faz: se o byte `DAT_08071130+0x620` é 0, inicia duas animações (`enable_anim4(0x18,0x3F,0xC)` e
  `enable_anim4(0x19,0x4E,8)`), depois `slot_start_v2(6,0x19,0x60)` e incrementa o contador em `*DAT_08071134`.
  Uma vez só, por flag.
- Chama: `enable_anim4`, `slot_start_v2`. Chamada por: desconhecido.
- Memória: flag `DAT_08071130+0x620`, contador em `DAT_08071134`.
- Para o Lua: sim (disparo de animações uma vez).
- Confiança: provável.

## 0x08071E0C Fx_RampCounterStepDone
- Assinatura: `undefined8 Fx_RampCounterStepDone(void)` (devolve 1 quando termina, 0 enquanto roda)
- O que faz: rampa em três fases sobre o short `+0x84` do bloco `DAT_08071E28`: até 0x3E soma 2; até 0x42 soma 1
  (espelhando em `+0x44`). Depois espera o contador `ushort +0x9A` passar de 0x1F. Então, se `+0x82 < 0x41`, chama
  `slot_ptr_stub(6)`, toca sfx 0x54, zera `+0x9A`, incrementa o byte `+0x10` de `DAT_08071E74` e devolve 1. Senão
  subtrai 4 de `+0x82` (espelhado em `+0x42`) e continua. Fora da espera, incrementa `+0x9A`.
- Chama: `slot_ptr_stub`, `snd_sfx_request`. Chamada por: desconhecido (passo de animação/transição).
- Memória: bloco `DAT_08071E28` (+0x42, +0x44, +0x82, +0x84, +0x9A), estado em `DAT_08071E74+0x10`.
- Para o Lua: sim (rampa e espera por contador).
- Confiança: provável.

## 0x08074728 Snd_UpdateChannelIfIdle
- Assinatura: `void Snd_UpdateChannelIfIdle(void)`
- O que faz: se o word `+0x340` do bloco `*DAT_08074748` é 0, chama `snd_channel_update_4` no canal em `+0x15C`.
- Chama: `snd_channel_update_4`. Chamada por: desconhecido (loop de som).
- Memória: bloco de som (`+0x340` estado, `+0x15C` canal).
- Para o Lua: não precisa (áudio), exceto se o som for reproduzido.
- Confiança: provável.

## 0x08074BCC Snd_StartSongWithCounter
- Assinatura: `undefined4 Snd_StartSongWithCounter(undefined4 songId)`
- O que faz: incrementa o `ushort` `+0x730` do bloco `*DAT_08074C04` (até o limite em `DAT_08074C08`), chama
  `snd_song_start(songId)` e decrementa o mesmo contador se for diferente de 0.
- Chama: `snd_song_start`. Chamada por: desconhecido (troca de música).
- Memória: contador em `*DAT_08074C04+0x730`.
- Para o Lua: não precisa (áudio).
- Confiança: provável (contador lido; efeito do contador não confirmado).

## 0x0807DA9C Math_SoftF64Compare
- Assinatura: `undefined4 Math_SoftF64Compare(u32 a_lo, u32 a_hi, u32 b_lo, u32 b_hi)`
- O que faz: compara dois valores de ponto flutuante de 64 bits em software. Desempacota cada um
  (`sf_unpack`, em 5 words) e, se algum tem classe menor que 2 (zero/especial), devolve 1. Senão devolve o resultado
  de `sf64_cmp_core`.
- Chama: `sf_unpack`, `sf64_cmp_core`. Chamada por: desconhecido (cálculo de tempo/valor em float).
- Memória: pilha apenas.
- Para o Lua: não precisa (Lua tem float nativo); só a regra de comparação se for necessária.
- Confiança: provável (biblioteca soft-float; semântica exata da classe 0/1 não confirmada).

## 0x0804AA44 FUN_0804AA44 (sem nome; não renomeado)
- Assinatura: `undefined4 FUN_0804AA44(void)`
- O que faz: compara dois bytes de um bloco (`DAT_0804AA68 + DAT_0804AA6C` contra `+DAT_0804AA70`). Se são iguais,
  zera o primeiro; em seguida copia o valor para o segundo byte e chama `GameState_AdvanceSubroutine`.
- Chama: `GameState_AdvanceSubroutine`. Chamada por: desconhecido.
- Memória: dois bytes de um bloco de estado (endereços pelos literais).
- Para o Lua: sim, se o fluxo de estado for portado (sincronização de flag e avanço).
- Confiança: confirmado (comparação e cópia lidas); subsistema desconhecido: chute. Não renomeado.
