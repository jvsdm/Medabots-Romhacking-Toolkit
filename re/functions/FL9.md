# FL9: funções 0x0800277C..0x08302DF8 (41 entradas de FL9_lista.txt)

Formato igual ao README (assinatura, o que faz, chama/chamada por, memória, Lua, confiança).
Nomes do Ghidra eram todos FUN_ (nenhum nomeado em mtbe.csv antes deste lote).

## 0x0800277C Sm_RunSelectedDispatch_FlagDone
- Assinatura: `void Sm_RunSelectedDispatch_FlagDone(void)`
- O que faz: escolhe um contexto (ctx de DAT_08002798+0xC ou DAT_080027b4+8) conforme o nibble alto do byte em DAT_08002794+0x28; chama `StateMachine_DispatchStep(ctx)`; se retornar != 0, grava 1 em `*DAT_080027b8` (flag de "passo concluído").
- Chama: StateMachine_DispatchStep / Chama por: não rastreado.
- Memória: lê ponteiros em literal pool; escreve byte apontado por DAT_080027b8.
- Para o Lua: não precisa (despacho interno de máquina de estados).
- Confiança: provável (estrutura lida; a identidade dos ponteiros não).

## 0x08003104 Menu_AnimateRevealRows
- Assinatura: `void Menu_AnimateRevealRows(void)`
- O que faz: a cada chamada incrementa o contador `ctx+0x84` até `ctx+0x88` (revelação em passos). Em cada passo copia retângulos de tiles do BG (`BgMap_CopyRectWithOffset`, `BgMap_FillRect16`). Quando termina, escolhe um de três layouts pelo byte `ctx+0x8E` (0/1/2) e copia as linhas, depois carrega tiles (`LoadTilesWithMalias`), formata um número (`Num_FormatDecimalToGlyphs`), monta sprites de texto (`Text_LayoutGlyphSprites`), chama `FUN_08003300` e marca `*DAT_080032e8 = 2` (fim).
- Chama: BgMap_CopyRectWithOffset, BgMap_FillRect16, LoadTilesWithMalias, Num_FormatDecimalToGlyphs, Text_LayoutGlyphSprites, FUN_08003300.
- Memória: lê ctx (0x84, 0x88, 0x8E) e ponteiros; escreve contador no ctx e flag em DAT_080032e8.
- Para o Lua: portar a lógica de revelação e o layout (são as linhas de tela do resumo/lista).
- Confiança: provável (o nome do subsistema é chute: tela de menu com linhas e número).

## 0x0800C6D8 Dispatch_CallHandlerByMode_A
- Assinatura: `void Dispatch_CallHandlerByMode_A(void)`
- O que faz: wrapper curto. Lê o byte `ctx+0x33` (modo) e chama `CallViaR0(tabela[modo])`, tabela de ponteiros em DAT_0800c6f0.
- Chama: CallViaR0 (trampolim bx r0). Chamada por: não rastreado.
- Memória: lê byte do ctx (+0x33) e a tabela na ROM.
- Para o Lua: não precisa (despacho), a tabela alvo é que importa.
- Confiança: confirmado (estrutura); o subsistema do ctx é desconhecido.

## 0x0800D3C4 Dispatch_CallHandlerByMode_B
- Assinatura: `void Dispatch_CallHandlerByMode_B(void)`
- O que faz: idêntico ao _A, com outro literal (DAT_0800d3dc/DAT_0800d3e0): `CallViaR0(tabela[ctx+0x33])`.
- Chama: CallViaR0. Chamada por: não rastreado.
- Memória: byte ctx+0x33 e tabela na ROM.
- Para o Lua: não precisa.
- Confiança: confirmado.

## 0x080087CC PartsGrid_EnterScreenInit
- Assinatura: `void PartsGrid_EnterScreenInit(void)`
- O que faz: entra na tela de grade de peças. Ajusta REG_DISPCNT (liga BGs/OBJ, mantém máscara), zera OAM/paleta (`bios_cpufastset` em 0x05000000), descompacta 4 assets (`decompress_and_store`), carrega 4 bancos de tiles (`LoadTilesWithMalias`), chama os desenhos `PartsGrid_*` (tile selecionado, gráfico, rótulo, ícones, barras), `Sprite_InitPairC`, `SetGpuBlendMode(5,0x3f,0x20)`. Zera ctx+0x84 e incrementa contador em DAT_0800897c.
- Chama: decompress_and_store, LoadTilesWithMalias, bios_cpufastset, PartsGrid_*, Sprite_InitPairC, SetGpuBlendMode.
- Memória: escreve REG_DISPCNT, VRAM/palette, ctx (0x3F, 0x48, 0x84), contador DAT_0800897c.
- Para o Lua: não precisa para o PC (é setup de hardware); o conteúdo visual é que interessa.
- Confiança: provável.

## 0x080102C0 Scr_HideSpritesPlaySong8
- Assinatura: `void Scr_HideSpritesPlaySong8(void)`
- O que faz: seta bit 0x40 em 4 halfwords apontados por DAT_08010300 (desliga/oculta 4 sprites), pede música 8 (`snd_song_request(8)`), zera ctx+0x85 e incrementa contador em DAT_08010308. Entrada de uma tela de transição.
- Chama: snd_song_request. Chamada por: não rastreado.
- Memória: escreve 4 halfwords de OAM/sprite (bit 0x40), byte ctx+0x85, contador DAT_08010308.
- Para o Lua: portar só o pedido de música (id 8) e o efeito visual (ocultar sprites).
- Confiança: provável (id 8 da música lido direto no código).

## 0x08011764 Fight_InitBotPartPanel
- Assinatura: `void Fight_InitBotPartPanel(void)`
- O que faz: inicializa a tela de peças de um bot na luta: limpa 4 pares de slots (`Slots_ClearFourPairs`), limpa e carrega BG (`BgMap_FillRect16`, `decompress_and_store` x5, `LoadTilesWithMalias`), escreve uma sequência de ~20 pares de words (puVar3+0x65/0x67/0x69, tabela de sprites da VRAM), carrega paleta/tiles (`GfxBankA_LoadTilesAndPalette`), chama desenhos `Fight_Draw*`/`Fight_Format*`/`Fight_Setup*`, depois copia 0xE e 0xF bytes (`Mem_CopyBytes`) de tabelas por índice de peça. Incrementa contador em DAT_08011a54.
- Chama: Slots_ClearFourPairs, BgMap_FillRect16, decompress_and_store, LoadTilesWithMalias, GfxBankA_LoadTilesAndPalette, Fight_DrawHeaderNameText, Fight_DrawStatusLabelText, Fight_DrawBotPartEntrySprites, Fight_DrawBotPartEntryIcons, Fight_DrawBotPartIconRow, Fight_SetupFlagSprite, Fight_FormatBotLabelText, FxBlend_ApplyFromObjA, Fight_DrawBotPartSpriteBig, Mem_CopyBytes.
- Memória: ctx da peça em DAT_08011a04 (+0x38 = banco, +0x168 = índice), VRAM/OAM, DISPCNT.
- Para o Lua: portar o layout (nome, rótulo de status, ícones de peça); o resto é hardware.
- Confiança: provável.

## 0x08012A20 Fight_PartPanelStep
- Assinatura: `void Fight_PartPanelStep(void)`
- O que faz: máquina de 3 estados do byte apontado por DAT_08012a3c+DAT_08012a40: estado 0 limpa slots, copia 8 bytes do objeto (`Obj_CopySlot8Bytes`), ajusta DISPCNT e chama `Fight_RunStateDispatch(0)`; se o despacho ainda não terminou, fica no estado; estado 1 limpa ctx+0x84, zera VRAM (`bios_cpufastset`) e liga/desliga bits de DISPCNT; estado 2 carrega HUD, preenche linhas do tilemap, atualiza sprites de peças de bots ativos, cria sprites compostos dos bots, `FxBlend_ApplyFromObjB`, descompacta asset fixo, e incrementa contador DAT_08012b3c.
- Chama: Slots_ClearFourPairs, Obj_CopySlot8Bytes, Fight_RunStateDispatch, Fight_LoadHudTileset, Fight_FillPartTilemapRows, Fight_RefreshDisplayedPartSpritesForActiveBots, Fight_CreateAllBotCompositeSprites, Fight_UpdateSixBotSprites, FxBlend_ApplyFromObjB, Gfx_DecompressFixedAsset.
- Memória: lê/escreve o byte de estado em DAT_08012a3c+DAT_08012a40 (ctx da tela), REG_DISPCNT, contadores DAT_08012b34/3c.
- Para o Lua: portar a ordem dos estados (é a animação de entrada da tela de peças); o resto é hardware.
- Confiança: provável (a ordem dos 3 estados está confirmada no código).

## 0x08013824 Fight_InitActionSlotSelect
- Assinatura: `void Fight_InitActionSlotSelect(void)`
- O que faz: zera 0x14 bytes de um bloco de estado (PTR_DAT_08013868), põe [0]=2, [1]=índice do slot (acha o bot pelo id via `Slot_FindByIdPlus1`, menos 1), [5]=1, [6]=2; limpa 4 pares de slots e grava 4 e 0x28 em dois ponteiros de tamanho/controle.
- Chama: Mem_FillBytes, Slot_FindByIdPlus1, Slots_ClearFourPairs. Chamada por: não rastreado (provável tela de escolha de ação).
- Memória: bloco de estado (0x14 bytes), dois ponteiros de controle.
- Para o Lua: portar a inicialização do cursor de seleção (valores iniciais).
- Confiança: provável.

## 0x08014D94 Fight_SelectActionSourceStep
- Assinatura: `void Fight_SelectActionSourceStep(void)`
- O que faz: lê o índice do bot atual em ctx+0x87 (bloco do bot = 0x03000590 + bot*0x100) e, se for > 2 (bot de lado 1), chama `Fight_SelectActionSource(bot, byte em bot*0x100+base+0xDD)`. Incrementa contador em DAT_08014dc4.
- Chama: Fight_SelectActionSource. Chamada por: não rastreado.
- Memória: lê fight_information+0x87 (bot atual) e byte do bot +0xDD; escreve contador DAT_08014dc4.
- Para o Lua: portar a regra (só age para bots > 2) e o campo +0xDD.
- Confiança: provável (offset +0xDD lido do código; significado do campo não confirmado).

## 0x0801C3B0 Fight_BotEntryAnimStep
- Assinatura: `void Fight_BotEntryAnimStep(void)`
- O que faz: a cada quadro, para o bot atual (ctx+0x87): anima frames de entrada (`Fight_AnimateEntryFrames`), monta o texto do nome (`Fight_LayoutBotNameText`) e atualiza o visual (`Fight_UpdateBotVisual`). Depois avança um contador em ctx+0x9C: se ctx+0x9D == 0 incrementa e marca ctx+0x9E=1; senão força ctx+0x9C=5 (fim).
- Chama: Fight_AnimateEntryFrames, Fight_LayoutBotNameText, Fight_UpdateBotVisual.
- Memória: lê/escreve ctx+0x9C, 0x9D, 0x9E (ctx da animação de entrada, base = DAT_0801c3e4).
- Para o Lua: portar a sequência (frames, nome, visual) e o contador de fases 0..5.
- Confiança: provável.

## 0x08034AF4 Fight_PickOpponentTargetStep
- Assinatura: `void Fight_PickOpponentTargetStep(void)`
- O que faz: escolhe o alvo adversário do bot atual (ctx+0x87): pega a base do lado adversário (`Fight_GetOpponentSideBaseIndex`) e grava em ctx+0x60; chama `FUN_08037830(lado, bot)`; se der 0xFF (sem alvo válido), sorteia outro bot ativo do lado (`Fight_PickRandomActiveBotInSide`). Grava o alvo (índice+1) em bot*0x100+0xE0. Depois sorteia a parte alvo (`Fight_SelectRandomTargetPartForSelectedBot`), copia para fight_information (`Fight_CopySelectedTargetToFightInfo`) e avança o cursor do script de ação (`Fight_AdvanceActionScriptCursor`).
- Chama: Fight_GetOpponentSideBaseIndex, FUN_08037830, Fight_PickRandomActiveBotInSide, Fight_SelectRandomTargetPartForSelectedBot, Fight_CopySelectedTargetToFightInfo, Fight_AdvanceActionScriptCursor.
- Memória: lê/escreve fight_information+0x60 (lado base), bot+0xE0 (alvo), ctx+0x87.
- Para o Lua: portar a regra de escolha de alvo (lado oposto, fallback aleatório) e o avanço do script. Sorteio: usar o RNG do jogo (ver FUN_ de RNG, não nomeado aqui).
- Confiança: provável.

## 0x08038804 Anim_StepSlotScriptAndRelease
- Assinatura: `void Anim_StepSlotScriptAndRelease(void)`
- O que faz: roda um passo do script de animação do slot atual (`Anim_RunSlotScript(ctx+0x1B0, slot)`), e se o byte +0x22 do slot (slot*0x40 a partir de DAT_08038848, provavelmente gObjectSlots 0x03002C30) for -1 (fim), libera o slot (`Tile_ClearSlots(slot,1)`).
- Chama: Anim_RunSlotScript, Tile_ClearSlots. Chamada por: não rastreado.
- Memória: lê ctx+0x1B0 e byte do slot; escreve os tiles do slot na liberação.
- Para o Lua: portar a liberação do slot ao fim da animação.
- Confiança: provável.

## 0x0803C9E0 Anim_StepSlotDelayAndBlit
- Assinatura: `void Anim_StepSlotDelayAndBlit(void)`
- O que faz: para o slot atual (0x40 bytes a partir de DAT_0803ca00, provavelmente gObjectSlots): se o atraso em +0x30 (short) é 0, roda um passo do script do slot (`Fight_StepAnimScriptSlot`). Se ctx+0x66 == -1 (fim), preenche o retângulo com `BgMap_FillRectWrapped` e libera o slot; senão, se +0x2D == 0, copia o retângulo de tiles (`BgMap_CopyRectWrapped`, fonte em tabela de +0x2C). Se o atraso > 0, só decrementa.
- Chama: Fight_StepAnimScriptSlot, BgMap_FillRectWrapped, BgMap_CopyRectWrapped, Tile_ClearSlots.
- Memória: lê/escreve slot+0x30, +0x2B, +0x2C, +0x2D, +0x2E/0x2F, +0x2/+0x4; lê ctx+0x66.
- Para o Lua: portar o atraso, a cópia do retângulo e a liberação. Não precisa a parte de hardware.
- Confiança: provável.

## 0x0803DAEC Fight_InitSelectBlockA
- Assinatura: `void Fight_InitSelectBlockA(void)`
- O que faz: zera 0x14 bytes de um bloco de estado (DAT_0803db10), põe [0]=1, e grava 3 e 0 em dois ponteiros de controle (DAT_0803db14, DAT_0803db18). Irmão de Fight_InitActionSlotSelect (que põe 2 em vez de 1).
- Chama: Mem_FillBytes. Chamada por: não rastreado.
- Memória: bloco de 0x14 bytes e dois ponteiros de controle.
- Para o Lua: não precisa se o bloco for só zerado; confirmar o significado dos valores (chute: modo de seleção).
- Confiança: chute (significado dos campos).

## 0x08040954 Dispatch_CallHandlerByIdx1B
- Assinatura: `void Dispatch_CallHandlerByIdx1B(void)`
- O que faz: wrapper. Lê o byte ctx+0x1B (índice) e chama `CallViaR0(tabela[índice])`, tabela em DAT_0804096c.
- Chama: CallViaR0. Chamada por: não rastreado.
- Memória: byte ctx+0x1B; tabela na ROM.
- Para o Lua: não precisa; portar a tabela alvo quando for preciso.
- Confiança: confirmado (estrutura).

## 0x08049740 Mem_CountDiffBytes
- Assinatura: `int Mem_CountDiffBytes(const char *a, const char *b, int n)`
- O que faz: compara n bytes de dois buffers e devolve quantos são diferentes (sem early exit). Não lê/escreve nada além dos buffers.
- Chama: nada. Chamada por: não rastreado (provável checagem de mudança de dados).
- Memória: lê os dois buffers.
- Para o Lua: portar como função pura (ou usar a comparação nativa do Lua).
- Confiança: confirmado (lido no código).

## 0x0804A60C Map_InitScreenAndTransition
- Assinatura: `void Map_InitScreenAndTransition(void)`
- O que faz: zera 0x2000 bytes (`set_zeros`, VRAM/tilemap) e 0x200 bytes (paleta/OAM), monta o ponteiro de estado em DAT_0804a678 (+4 = 0x07000000 = OAM, +8 = ctx de render), reseta o estado de render afim (`Map_ResetRenderAffineState`), chama FUN_08048bb8, zera dois bytes do estado, inicia o efeito de transição (`Overworld_StartScreenTransitionEffect`) e avança a sub-rotina do jogo (`GameState_AdvanceSubroutine`).
- Chama: set_zeros, Map_ResetRenderAffineState, FUN_08048bb8, Overworld_StartScreenTransitionEffect, GameState_AdvanceSubroutine.
- Memória: escreve 0x2000+0x200 bytes (VRAM/OAM/paleta), estado em DAT_0804a678/0804a680.
- Para o Lua: portar a transição (efeito) e o estado inicial; o zerar de VRAM é hardware.
- Confiança: provável.

## 0x080575DC Menu_ChoiceCountdownStep
- Assinatura: `void Menu_ChoiceCountdownStep(void)`
- O que faz: se o contador em ctx+0x6F é 0, descompacta um asset (`decompress_and_store`) e avança a sub-rotina (`GameState_AdvanceSubroutine`). Se não é 0, desenha duas janelas de texto (`FUN_080492a8` x2, em linhas 0x1E-n e 0x1F-n), cria sprites de escolha (`Map_CreateChoiceSprites(1)`) e decrementa o contador. No fim faz uma chamada indireta por ponteiro (jump table não recuperada em 0x0805767C).
- Chama: decompress_and_store, GameState_AdvanceSubroutine, FUN_080492a8, Map_CreateChoiceSprites, e um ponteiro indireto.
- Memória: lê/escreve contador ctx+0x6F; escreve nas janelas de texto (VRAM).
- Para o Lua: portar a contagem regressiva que mostra as opções (2 janelas) antes de liberar o estado.
- Confiança: provável (a jump table final não foi recuperada).

## 0x0805C574 Menu_ThreeRowCursorInput
- Assinatura: `void Menu_ThreeRowCursorInput(void)`
- O que faz: lê o estado de botões (`*DAT_0805c5e8`; bits GBA: 0x01 A, 0x02 B, 0x10 direita, 0x20 esquerda, 0x40 cima, 0x80 baixo) e move o cursor entre 3 linhas (linha = ctx+5, valores 0..2), tocando SFX 2 e música 1. Na linha 0: esquerda/direita mudam uma opção (DAT_0805c674, DAT_0805c6a0). Na linha 1: direita alterna um flag (DAT_0805c6cc). Na linha 2: esquerda/direita trocam uma seleção e tocam música (DAT_0805c720). A ou B confirma: põe a linha em 2 e chama `Screen_SetRowPalette3`, `Screen_HighlightGridRow`, `Screen_HighlightBitmaskGrid`. Ao fim, anima a posição (`Slot_SetPosStepByMode`) e a paleta (`Palette_FadeTo16`) e atualiza o destaque da grade.
- Chama: snd_song_request, snd_sfx_request, Screen_DrawCellAdvance, Slot_SetPosStepByMode, Palette_FadeTo16, Screen_HighlightGridRow, Screen_HighlightBitmaskGrid, Screen_SetRowPalette3.
- Memória: lê o input em DAT_0805c5e8 (ponteiro para o estado de teclas); lê/escreve ctx+5 (linha), +2 (contador), +0x3A (posição).
- Para o Lua: portar a navegação (3 linhas, cada uma com seu comportamento) e os SFX/músicas. Mapa de teclas confirmado.
- Confiança: provável (a semântica exata de cada linha é inferida pelos handlers).

## 0x0805CED0 Screen_FadeStepThenLoadObj
- Assinatura: `void Screen_FadeStepThenLoadObj(void)`
- O que faz: enquanto o contador em ctx+0x84 é < 9, faz um passo de fade de paleta por linhas (`Palette_FadeRowsToColorA`, `Palette_FadeRowsToColorB`, valor 0x20 - 4*n) e incrementa o contador. Ao chegar em 9, chama `Obj_InitSlotAndLoad(1,2,0)` e incrementa o contador de fase em DAT_0805cf54.
- Chama: Palette_FadeRowsToColorA/B, Obj_InitSlotAndLoad.
- Memória: lê/escreve ctx+0x84; lê ctx+6 (cor do fade, escolha A/B).
- Para o Lua: portar o fade de 9 passos (efeito) e a carga do objeto no fim.
- Confiança: provável.

## 0x0805D790 Screen_InitLZ77TileBlocks
- Assinatura: `void Screen_InitLZ77TileBlocks(void)`
- O que faz: inicializa uma tela: grava três valores de offset/scroll (ctx+0, +2, +4), carrega três blocos de tiles LZ77 (`Screen_LoadLZ77TileBlock`), desempacota um bloco (`Asset_UnpackOrCopyBlock`) e copia por `bios_cpuset` (CpuSet), descompacta outro asset (`decompress_and_store`), ajusta um campo +0x36 e grava parâmetros de sprite (0xC000, 0x23, 0xD0, 0x18). Incrementa contador em DAT_0805d85c.
- Chama: Screen_LoadLZ77TileBlock x3, Asset_UnpackOrCopyBlock, bios_cpuset, decompress_and_store.
- Memória: escreve ctx de tela (+0..+0xE), VRAM (CpuSet), contador DAT_0805d85c.
- Para o Lua: não precisa para hardware; o asset descompactado é o que importa.
- Confiança: provável.

## 0x0805E478 Screen_InitFullLayoutLZ77
- Assinatura: `void Screen_InitFullLayoutLZ77(void)`
- O que faz: versão maior da inicialização de tela: carrega 3 blocos LZ77, desempacota e copia com CpuSet (0x800 palavras), limpa VRAM/paleta (`bios_cpufastset`), descompacta 2 assets, monta 2 sprites/entradas de tilemap (0xC000, 0x8000), escolhe entre dois ramos pelo byte `*DAT_0805e5e0` (grava tiles em BG1 ou usa 0xC000), posiciona 2 objetos (campos +2, +0x40, +0x21, +0x22) a partir de ctx de tela e incrementa o contador DAT_0805e710.
- Chama: Screen_LoadLZ77TileBlock x3, Asset_UnpackOrCopyBlock, bios_cpuset, bios_cpufastset, decompress_and_store.
- Memória: escreve VRAM/OAM/tilemap (BG1CNT via REG_BG1CNT), ctx de tela, contador DAT_0805e710. Lê byte de escolha em DAT_0805e5e0.
- Para o Lua: portar o layout (posições e o ramo escolhido); o resto é hardware.
- Confiança: provável.

## 0x0805F488 Menu_OptionsScreenStep
- Assinatura: `void Menu_OptionsScreenStep(void)`
- O que faz: passo por quadro de uma tela de opções com 3 seções (seção = ctx+5, valores 0..2 com volta circular). Move a posição (`Slot_SetPosStepByMode`), decrementa um timer em +0x48 (SFX 2 ao zerar), e nos botões: cima/baixo (0x40/0x80) trocam a seção e tocam música 0. Na seção 0 e 1, esquerda/direita mudam um contador decimal (+0x2B, faixa 0..0x33, e +0x2C, faixa 0..0x75) e redesenham com `Screen_DrawDecimalNumber`. Confirmar (A=0x01) dispara música/SFX conforme a seção e chama `Fx_InitRecord8` e `Obj_InitSlotAndLoad` (seção 2). Esconde camadas de sprite (máscara de bits em DAT_0805f628/57c) por seção.
- Chama: Slot_SetPosStepByMode, snd_sfx_request, snd_song_request, Screen_DrawDecimalNumber, Fx_InitRecord8, Obj_InitSlotAndLoad.
- Memória: lê teclas em DAT_0805f5a8/f670/f6c4/f714/f7e8; escreve ctx de opções (+5, +0x2B, +0x2C, +0x48, +0x4A, +0x4B, +0x49) e flags de sprite (OAM).
- Para o Lua: portar as seções de opções, faixas de valores (0..0x33 e 0..0x75) e a ação de confirmar. Mapa de teclas igual ao de Menu_ThreeRowCursorInput.
- Confiança: provável (faixas e seções inferidas do código; nomes das opções não conhecidos).

## 0x0805FEC8 Anim_CountdownThenHideLayer
- Assinatura: `void Anim_CountdownThenHideLayer(void)`
- O que faz: contador de byte apontado por *DAT_0805fee4: se > 0, decrementa e grava metade do valor em *DAT_0805fee8 (short, valor de escala/posição). Quando chega a 0, zera dois bytes de estado (+2, +3), liga o bit 0x4000 em OAM (ocultar camada) e zera o short +0x5A; incrementa contador DAT_0805ff18.
- Chama: nada. Chamada por: não rastreado.
- Memória: lê/escreve o byte de contador, short de escala, OAM (bit 0x4000), contador DAT_0805ff18.
- Para o Lua: portar o temporizador (metade do valor a cada passo) e o ocultar.
- Confiança: provável.

## 0x080612E8 Text_CopyAndDispatchStep
- Assinatura: `void Text_CopyAndDispatchStep(void)`
- O que faz: copia texto do buffer de origem (DAT_0806132c) para ctx+0x270 até 0xFF (`Text_CopyUntilFF`), depois roda um passo da máquina de estados sobre esse buffer (`StateMachine_DispatchStep`). Quando o passo termina (!= 0), liga o bit 0x4000 em OAM (DAT_08061330+0x200) e incrementa o contador DAT_08061334.
- Chama: Text_CopyUntilFF, StateMachine_DispatchStep.
- Memória: lê buffer de texto (terminador 0xFF); escreve ctx+0x270.., OAM, contador.
- Para o Lua: portar a exibição de texto (copiar até 0xFF e avançar). O terminador 0xFF é confirmado.
- Confiança: provável.

## 0x08062484 Fight_PreviewFadeThenInit
- Assinatura: `void Fight_PreviewFadeThenInit(void)`
- O que faz: fase 1: 9 passos de fade (`Palette_FadeRowsToColorA/B`, 4*n) em ctx+0x84. Fase 2, se ctx+0x48 == 0: limpa bits de prioridade/visibilidade de três grupos de sprites (máscara em DAT_080625e4), grava 0x1000 e posições em OAM, e põe o estado em 0xB. Se ctx+0x48 != 0: chama `FightPreview_InitSideState(ctx+0x19, gShopItems[ctx+0x1A] ...)` e põe o estado em 0. Usa a tabela de itens `gShopItems` (0x02000000) para o item de preview.
- Chama: Palette_FadeRowsToColorA, Palette_FadeRowsToColorB, FightPreview_InitSideState.
- Memória: lê/escreve ctx+0x84, +0x48, +0x19, +0x1A; lê gShopItems (8 bytes por item).
- Para o Lua: portar o fade e a escolha de lado/item do preview.
- Confiança: provável.

## 0x080633FC BattlePanel_FadeAndScroll
- Assinatura: `void BattlePanel_FadeAndScroll(void)`
- O que faz: fase de fade (contador ctx+0x84 de 0 a 0x40, `Palette_FadeRowsToColorB` com metade do contador); ao terminar zera ctx+0x3C (short), zera o contador e incrementa DAT_08063470. Todo quadro desenha dois textos de slot (`BattlePanel_DrawSlotText`, com parâmetros dos bytes +0x31..+0x34) e rola colunas de tiles (`Anim_ScrollTileColumns`).
- Chama: Palette_FadeRowsToColorB, BattlePanel_DrawSlotText x2, Anim_ScrollTileColumns.
- Memória: lê/escreve ctx+0x84, +0x3C; contador DAT_08063470.
- Para o Lua: portar o fade (0..0x40) e o texto dos dois slots.
- Confiança: provável.

## 0x08063FB0 Sprite_ScriptRecordStep
- Assinatura: `void Sprite_ScriptRecordStep(void)`
- O que faz: roda um passo da máquina de estados (`StateMachine_DispatchStep` em ctx+100) e depois lê uma palavra de comando (ctx+10 ou +14, conforme o byte em DAT_08064040). Se o bloco está em modo 0: prefixo 0xFF00 zera um campo de posição; 0xFE00 grava um valor em ctx+6 (campo de deslocamento). Se o modo é 1: comandos 0xC000 gravam um byte em um vetor de 0x230 (lista de sprites) e incrementam um contador. Quando as duas coordenadas valem -0x100, libera o registro do slot (`Slot_ReleaseById`) ou copia o registro para livre e carrega o objeto (`Slot_CopyRecordIntoFree` + `Sprite_LoadAndInitObj`). Incrementa DAT_080640d8.
- Chama: StateMachine_DispatchStep, Slot_ReleaseById, Slot_CopyRecordIntoFree, Sprite_LoadAndInitObj.
- Memória: lê palavras de comando e ctx+0x230.. (lista de sprites), ctx+0x7A (modo), +0x231/+0x238 (id/parâmetro de sprite).
- Para o Lua: portar a interpretação dos comandos (0xFF00/0xFE00/0xC000 e o fim em -0x100). Confirmado na leitura, pouco contexto de uso.
- Confiança: provável.

## 0x080649BC Dispatch_CallHandlerFromTable4
- Assinatura: `void Dispatch_CallHandlerFromTable4(void)`
- O que faz: copia 4 palavras de uma tabela (DAT_080649f8) para a pilha, escolhe uma pelo índice em DAT_080649fc e chama `CallViaR0`. Se o bit 0 de DAT_08064a00 está ligado, incrementa o contador DAT_08064a04.
- Chama: CallViaR0 (ponteiro escolhido). Chamada por: não rastreado.
- Memória: lê 4 ponteiros e o índice; escreve contador DAT_08064a04.
- Para o Lua: não precisa; portar a tabela de 4 handlers quando for necessário.
- Confiança: confirmado (estrutura).

## 0x08065ED0 Screen_DispatchByState3A
- Assinatura: `void Screen_DispatchByState3A(void)`
- O que faz: despacha pelo estado short em ctx+0x3A (se < 6), escolhendo entre duas tabelas de ponteiros de função conforme o byte em DAT_08065ef8 (0 = tabela em DAT_080660a8, outro = tabela em DAT_08065f00). A jump table não foi recuperada pelo Ghidra.
- Chama: ponteiro escolhido (indireto). Chamada por: não rastreado.
- Memória: lê ctx+0x3A (estado) e as tabelas na ROM.
- Para o Lua: portar as duas tabelas de estado (0..5) se for preciso; o estado 0x3A é o que importa.
- Confiança: provável (jump tables não recuperadas).

## 0x080675D4 Screen_InitRingSpritesSong5
- Assinatura: `void Screen_InitRingSpritesSong5(void)`
- O que faz: inicialização de tela com anel de sprites: limpa buffer local de 0x80 bytes, pede música 5 (`snd_song_request(5)`), limpa VRAM 0x06000000 (`bios_cpufastset`), `Tile_ClearSlots(0,0x80)`, `Clear4PairsHalfwords`, carrega tiles LZ77 (`Screen_LoadLZ77TileBlock` x3) e paleta, descompacta um asset, põe 5 entradas de posição (0x78, 0x50), zera 4 pares de halfwords, monta 36 sprites em anel (0xC800, cálculo `aux_div_s32(i,0x12)`, 0x116), constrói identidade afim (`Affine_LoadIdentityBuffer`) e os pontos do anel com raio 0x32 (`Ring_BuildPointsFromRadius(0x32)`). Liga os bits de DISPCNT 0x100..0x1000 (BGs/OBJ). Incrementa DAT_0806786c.
- Chama: snd_song_request, bios_cpufastset, Tile_ClearSlots, Clear4PairsHalfwords, Screen_LoadLZ77TileBlock, decompress_and_store, Affine_LoadIdentityBuffer, Ring_BuildPointsFromRadius, aux_div_s32.
- Memória: VRAM 0x06000000, OAM/paleta, REG_DISPCNT, ctx de tela (+0x3A, +0x48..+0x4B, +0x84), contador DAT_0806786c.
- Para o Lua: portar o anel (36 sprites, raio 0x32, centro 0x78/0x50) e a música 5. O resto é hardware.
- Confiança: provável (36 sprites = 0x48/2 é leitura direta; o raio 0x32 é o argumento da chamada).

## 0x08068BC4 Fx_PeriodicSineEmitStep
- Assinatura: `void Fx_PeriodicSineEmitStep(void)`
- O que faz: efeito periódico. Liga REG_DISPCNT 0x100 quando o contador DAT_08068c94 é 0. A cada `contador % período` (período = byte em DAT_08068c98) == 0, escreve 4 entradas de efeito em DAT_08068c9c, com valor = tabela (ushort, índice = contador / período) + constante. Depois chama um ponteiro indireto (jump table não recuperada) com `contador == fase*0x10`.
- Chama: aux_mod_s32, aux_div_s32, ponteiro indireto. Chamada por: não rastreado.
- Memória: lê contador DAT_08068c94, período DAT_08068c98, tabela de ushort em DAT_08068ca0; escreve 4 entradas (ponteiro+valor+flag) e REG_DISPCNT.
- Para o Lua: portar a emissão periódica (período, tabela, 4 entradas). Tabela de valores não lida.
- Confiança: provável (tabela e handler indireto não recuperados).

## 0x0806BCFC Fx_StepTableB_Check
- Assinatura: `void Fx_StepTableB_Check(void)`
- O que faz: chama `fx_step_table_b()`; se devolver != 0, grava um registro de efeito (ponteiro para pilha, valores em DAT_0806bd30/bd34) e incrementa DAT_0806bd38.
- Chama: fx_step_table_b. Chamada por: não rastreado.
- Memória: escreve registro de efeito e contador DAT_0806bd38.
- Para o Lua: não precisa se o efeito for só temporização; ver fx_step_table_b (não documentada aqui).
- Confiança: provável.

## 0x0806E460 Battle_DispatchThenEndTransition
- Assinatura: `void Battle_DispatchThenEndTransition(void)`
- O que faz: chama `CallViaR0(tabela[*DAT_0806e480])` (tabela em DAT_0806e47c, índice de byte) e depois `battle_timer_end_transition()`.
- Chama: CallViaR0, battle_timer_end_transition. Chamada por: não rastreado.
- Memória: lê índice em DAT_0806e480 e tabela na ROM.
- Para o Lua: portar a ordem (handler escolhido, depois fim de transição do timer).
- Confiança: provável.

## 0x0806EEA8 Screen_InitObjectSlotsLZ77Vram
- Assinatura: `void Screen_InitObjectSlotsLZ77Vram(void)`
- O que faz: inicialização de tela de batalha: descompacta 7 blocos LZ77 direto na VRAM (`bios_lz77_vram`, primeiro em 0x06000000), preenche uma estrutura de DMA/cópia (DAT_0806f06c), configura `fx_screen_setup_a(4,1,0)`, registra objetos de render (`Map_InitializeObjectRenderSlot`: slots 9, 10, 11..13 com posição (0xC0,200), e 3..8 em fila com x = 0xA0 + 8*i e y = 200), limpa texto e fila (`clear_text_buffer`, `init_fila_4`, `init_hide_sprites`, `zero_counters_f0`), liga DISPCNT 0x1E00 (BG1-3 + OBJ), incrementa DAT_0806f0dc.
- Chama: bios_lz77_vram, fx_screen_setup_a, Map_InitializeObjectRenderSlot, clear_text_buffer, init_fila_4, init_hide_sprites, zero_counters_f0.
- Memória: VRAM 0x06000000.., OAM (slots 3..13, posições), REG_DISPCNT, contador DAT_0806f0dc.
- Para o Lua: portar a disposição dos objetos (posições dos slots 3..13) e a ordem; o resto é hardware.
- Confiança: provável.

## 0x0807105C Anim_StartPairOnce
- Assinatura: `void Anim_StartPairOnce(void)`
- O que faz: se o byte de flag em DAT_080710a0+DAT_080710a4 é 0, habilita duas animações (`enable_anim4` com ids 0x16 e 0x17) e inicia dois slots (`slot_start_v2` slots 5 e 6, parâmetro 0x48). Incrementa o contador DAT_080710a8. Roda uma vez.
- Chama: enable_anim4, slot_start_v2. Chamada por: não rastreado.
- Memória: lê flag, escreve contador DAT_080710a8.
- Para o Lua: portar as duas animações e o disparo único.
- Confiança: provável.

## 0x080719B8 Anim_StepSlotTimerReturnStatus
- Assinatura: `uint64 Anim_StepSlotTimerReturnStatus(uint slot)`
- O que faz: para o slot (slot*0x40 a partir de DAT_080719fc): copia 8 bytes de tabela (`lib_memcpy`, DAT_080719f8); se o estado +0x21 < 8 e o short +0x1A confere com a tabela, decrementa o timer +2 e avança o estado. Se o estado = 4 e o byte +0x22 != -1, inicia `slot_start_v2(2, anim)`. Se o short +0x2 do slot é 0x1F, ajusta o bit 0xC00 do campo +0. Devolve 0xFF se o timer < 0x19, senão 1.
- Chama: lib_memcpy, slot_start_v2.
- Memória: lê/escreve campos do slot (+0, +2, +0x1A, +0x21, +0x22); lê a tabela de 8 bytes.
- Para o Lua: portar o timer e o retorno de status (0xFF / 1).
- Confiança: provável.

## 0x080746E0 Snd_UpdateChannelIfIdle
- Assinatura: `void Snd_UpdateChannelIfIdle(void)`
- O que faz: se o short em ctx+600 (0x258) é 0, chama `snd_channel_update_2(ctx+0x74)` para atualizar o canal de som.
- Chama: snd_channel_update_2. Chamada por: não rastreado.
- Memória: lê ctx+0x258 (ponteiro em DAT_08074700).
- Para o Lua: não precisa (som).
- Confiança: provável.

## 0x0807DA24 Sf64_CmpWrapper
- Assinatura: `void Sf64_CmpWrapper(u32 a_lo, u32 a_hi, u32 b_lo, u32 b_hi)`
- O que faz: desempacota dois valores de 64 bits (`sf_unpack`) e chama `sf64_cmp_core` para comparar. Auxiliar de ponto flutuante por software (sem retorno aparente).
- Chama: sf_unpack, sf64_cmp_core. Chamada por: não rastreado.
- Memória: só pilha.
- Para o Lua: não precisa.
- Confiança: provável.

## 0x08302DF8 (não é função)
- Sem função no Ghidra (tamanho desconhecido, 3 bytes). A leitura do código mostra `cdp` (coprocessador): é dado ou literal na região de ROM fora do código. Não documentado; não renomeado.
- Confiança: confirmado (dado).
