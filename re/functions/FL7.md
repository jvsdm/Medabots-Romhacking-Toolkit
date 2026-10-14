# FL7 — 41 funções da faixa FL7_lista.txt

Ponteiros de RAM/IO vêm do literal pool e não foram resolvidos nesta passada (aparecem como `DAT_...`).

## 0x080026E0 StateMachine_InitTableAndStep
- Assinatura: `void StateMachine_InitTableAndStep(void)`
- O que faz: grava uma tabela fixa de 10 bytes (0x17, 0x29, 0x2C, 0x25, 0x00, 0x2E, 0x1F, 0x2D, 0x2E, 0xFE) num buffer de RAM apontado por literal, chama `StateMachine_DispatchStep` sobre o estado em `*DAT_08002728`; se o retorno for 1, marca o flag `*DAT_0800272C = 1`.
- Chama: `StateMachine_DispatchStep` / Chamada por: não verificado.
- Memória: escreve no buffer de RAM (endereço no literal `DAT_08002724`+4..+0xD), lê/escreve estado (`DAT_08002728`), escreve flag (`DAT_0800272C`).
- Para o Lua: não precisa (passo de máquina de estados; significado dos bytes da tabela não identificado).
- Confiança: provável (padrão de passo + flag); conteúdo da tabela: chute.

## 0x08002F70 Map_EnterSceneSetup
- Assinatura: `void Map_EnterSceneSetup(void)`
- O que faz: marca `*DAT_08002FF8 = 1`; se o byte de estado em `ctx+0x8E` não for 2, reconfigura REG_DISPCNT, copia três words de config para RAM, chama `Map_LoadCurrentMapAndEntities`, `Overworld_ResetMapSceneBlendState` e `FUN_080497E0(0x40,0x2F00,5,...)` (provável transição/fade), e liga as camadas com `REG_DISPCNT |= 0xF60`. Depois zera/ajusta `ctx+0x84` e `ctx+0x88 = 0x12` e incrementa um contador global.
- Chama: `Map_LoadCurrentMapAndEntities`, `Overworld_ResetMapSceneBlendState`, `FUN_080497E0`.
- Memória: IO `REG_DISPCNT` (0x04000000); escreve contexto da cena (`ctx+0x84`, `ctx+0x88`); contador em `DAT_08003014`.
- Para o Lua: o carregamento do mapa é o que importa; registradores de IO não precisam ser portados.
- Confiança: provável (nome pelos chamados).

## 0x08007E38 PartsGrid_InitScreen
- Assinatura: `void PartsGrid_InitScreen(void)`
- O que faz: monta a tela de grade de peças: zera DISPCNT e configura registradores de fundo (`0x500`, três BG words), descomprime cinco blocos gráficos com `decompress_and_store` (0x080009BC), carrega tiles de quatro conjuntos com `LoadTilesWithMalias`, limpa VRAM/paleta com `bios_cpufastset` (vários blocos de 8 words), liga camadas (`REG_DISPCNT |= 0x1F60`), chama os desenhos `PartsGrid_*` (tile selecionado, gráfico, rótulo, ícones de célula, linhas de detalhe, modo de detalhe) e `Sprite_InitPairC`/`Sprite_SetPairSizeC`, e termina com `SetGpuBlendMode(5,0x3F,0x20)`.
- Chama: `decompress_and_store`, `LoadTilesWithMalias`, `bios_cpufastset`, `PartsGrid_UploadSelectedTile`, `PartsGrid_DrawSelectedGraphic`, `PartsGrid_DrawSelectedLabel`, `PartsGrid_DrawCellIconLayer`, `PartsGrid_DrawDetailRows`, `PartsGrid_DrawDetailByMode`, `Sprite_InitPairC`, `Sprite_SetPairSizeC`, `SetGpuBlendMode`.
- Memória: IO DISPCNT/BG; VRAM 0x05000000 (paleta/tiles); contexto da tela em `DAT_08008014` (`+0x44`, `+0x46`, `+0x84`); contador em `DAT_08008018`.
- Para o Lua: layout e tiles da tela de peças (ver PartsGrid_* e Sprite_*); DMA/BIOS não precisa.
- Confiança: provável (pelos helpers `PartsGrid_*` que ele chama).

## 0x0800C29C Dispatch_CallHandlerByByte33
- Assinatura: `void Dispatch_CallHandlerByByte33(void)`
- O que faz: despachante mínimo: lê o byte em `ctx+0x33` (ctx = ponteiro do literal `DAT_0800C2B8`), usa-o como índice (×4) numa tabela de ponteiros (base no literal `DAT_0800C2B4`) e chama o alvo via `CallViaR0`.
- Chama: `CallViaR0` (trampolim `bx r1`, ver README). Chamada por: callers não verificados (função de 32 bytes, refs=1).
- Memória: lê byte de contexto (`ctx+0x33`) e a tabela de handlers (ROM ou RAM, a confirmar pelo literal).
- Para o Lua: o handler alvo é que importa; o despachante em si é só troca de ponteiro.
- Confiança: provável (dispatch por tabela); subsistema: chute.

## 0x0800D25C Dispatch_CallHandlerByByte33_b
- Assinatura: `void Dispatch_CallHandlerByByte33_b(void)`
- O que faz: mesmo código do 0x0800C29C (duplicata com outro literal): lê `ctx+0x33`, indexa tabela de ponteiros de 4 bytes e chama o alvo via `CallViaR0`.
- Chama: `CallViaR0`. Chamada por: não verificado.
- Memória: lê byte de contexto e a tabela de handlers (literais `DAT_0800D274`/`DAT_0800D278`).
- Para o Lua: não precisa (despachante).
- Confiança: confirmado (código idêntico ao anterior); subsistema: chute.

## 0x080101E8 Slot_CountdownStepAndClear
- Assinatura: `void Slot_CountdownStepAndClear(void)`
- O que faz: subtrai 8 de um byte de contador em `ctx+0x108` e de outro byte (via literal `DAT_08010224`); quando o primeiro chega a zero, chama `Slot_ClearAttrMask(4,1)` e incrementa um byte de estado (literal `DAT_08010228`).
- Chama: `Slot_ClearAttrMask`. Chamada por: não verificado.
- Memória: lê/escreve contexto (`ctx+0x108` e dois campos por literal).
- Para o Lua: a lógica de contagem regressiva é simples de portar; limpeza de slot depende de `Slot_ClearAttrMask`.
- Confiança: provável (contagem regressiva + limpeza); subsistema: chute.

## 0x08011500 Gfx_ResetBgLayersAndSong
- Assinatura: `void Gfx_ResetBgLayersAndSong(void)`
- O que faz: pede a música 1 (`snd_song_request(1)`), limpa tiles (`Tile_ClearSlots(0,0x80)`) e quatro pares de slots (`Slots_ClearFourPairs`), preenche com zero os mapas dos BG0..BG3 (`BgMap_FillRect16` 32x32, tamanhos lidos de REG_BGxCNT via literais), zera três words de contexto e chama `FUN_0804765C(0)`.
- Chama: `snd_song_request`, `Tile_ClearSlots`, `Slots_ClearFourPairs`, `BgMap_FillRect16`, `FUN_0804765C`. Chamada por: não verificado.
- Memória: IO REG_BG0CNT..REG_BG3CNT (lidos); VRAM de mapas de fundo (escrita); contexto em `DAT_080115A0`/`DAT_080115A4`/`DAT_080115A8`.
- Para o Lua: o reset de fundos é o que importa; a música é a request de som.
- Confiança: provável (pelos chamados e pelo reset de BGs).

## 0x08012970 Battle_FrameDispatchOrExit
- Assinatura: `void Battle_FrameDispatchOrExit(void)`
- O que faz: chama `Battle_CheckExitShortcut`; se não sair, quando o estado em `*DAT_080129B0` é 2 limpa 0x200 halfwords via `bios_cpufastset`; depois despacha por tabela de ponteiros de 4 bytes indexada por `*DAT_080129C4` (`CallViaR0`). Função de quadro da batalha.
- Chama: `Battle_CheckExitShortcut`, `bios_cpufastset`, `CallViaR0`. Chamada por: não verificado.
- Memória: lê estado (`DAT_080129B0`, `DAT_080129C4`); limpa bloco de RAM/VRAM (endereço por literal `DAT_080129B4+DAT_080129B8`).
- Para o Lua: a ideia é "um passo de batalha por quadro"; a limpeza por BIOS não precisa ser portada.
- Confiança: provável (nome pelo `Battle_CheckExitShortcut`).

## 0x08013438 Fight_UpdateKindSelectorFrame
- Assinatura: `void Fight_UpdateKindSelectorFrame(void)`
- O que faz: quadro do seletor de tipo de peça na batalha: desenha overlay dos bots ativos, anima quadros de entrada, atualiza movimento de sprite, incrementa o substate `ctx+0x84`, reinicia objetos de tipo e o seletor B e chama `FUN_080173B4(1)`. Lê um byte de entrada (`*PTR_DAT_080134D0`): bit 1 toca SFX 5, limpa BG0 e tiles, mostra leitura numérica e avança `ctx+0x84`/contador; bit 0 toca SFX 4 e grava 0x1D no contador de passo.
- Chama: `Fight_DrawActiveBotsOverlay`, `Fight_AnimateEntryFrames`, `Sprite_UpdateComposite4Motion`, `Fight_InitKindObjectsForStateB`, `Fight_UpdatePartKindSelectorB`, `FUN_080173B4`, `snd_sfx_request`, `BgMap_FillRect16`, `Fight_DrawBotNumericReadout`, `Tile_ClearSlots`. Chamada por: não verificado.
- Memória: contexto `PTR_DAT_080134C8+0x80` (bot ativo) e `PTR_DAT_080134CC` (`+0x84`, `+0x86`); entrada em `PTR_DAT_080134D0`; passo em `PTR_DAT_080134D8`.
- Para o Lua: a lógica de seleção (entrada -> próximo passo) é o que vale portar; áudio/VRAM não.
- Confiança: provável (nomes dos chamados são de batalha).

## 0x08014C30 Fight_SelectThreeActionSources
- Assinatura: `void Fight_SelectThreeActionSources(void)`
- O que faz: zera um u16 de contexto e chama `Fight_SelectActionSource` três vezes (índices 3, 4 e 5), cada uma com um byte lido de uma tabela do contexto; ao fim incrementa um contador global.
- Chama: `Fight_SelectActionSource`. Chamada por: não verificado.
- Memória: escreve `DAT_08014C6C+6` (u16); lê bytes via literais `DAT_08014C70`+`DAT_08014C74/78/7C`; contador em `DAT_08014C80`.
- Para o Lua: portar o significado de `Fight_SelectActionSource` (não lido aqui).
- Confiança: provável (padrão de três chamadas com índices fixos).

## 0x0801C360 Fight_StepSpriteGroupMoveAnim
- Assinatura: `void Fight_StepSpriteGroupMoveAnim(void)`
- O que faz: chama `Fight_UpdateSpriteGroupMoveAnim` com o byte `ctx+0x87`; se ela devolver não-zero, incrementa `ctx+0x9C` (passo seguinte).
- Chama: `Fight_UpdateSpriteGroupMoveAnim`. Chamada por: não verificado.
- Memória: lê/escreve `ctx+0x9C` (ctx pelo literal `DAT_0801C384`).
- Para o Lua: a lógica é um if; a animação está na função chamada.
- Confiança: confirmado (código de 5 linhas); nome da chamada é do mtbe.csv/Ghidra.

## 0x0803496C Fight_AccumulatePartBonusForSide
- Assinatura: `void Fight_AccumulatePartBonusForSide(void)`
- O que faz: grava em `ctx+0x60` o índice de base do lado aliado (`Fight_GetAllySideBaseIndex(ctx+0x87)`). Depois, para três bots do lado (índices base+0..2), se o byte de bot em `+0xBA` não é zero, percorre quatro peças (passo 0x18 a partir de `+0x59`) e, para cada peça com byte zero (peça inativa/quebrada), soma 10 a um u16 em `+0x8C` do contexto de batalha. Termina com `Fight_AdvanceActionScriptCursor`.
- Chama: `Fight_GetAllySideBaseIndex`, `Fight_AdvanceActionScriptCursor`. Chamada por: não verificado.
- Memória: bloco de bot `0x03000590 + bot*0x100` (offset `+0x59+0x18*i` = flag de peça, conforme HANDOFF); escrita em `ctx+0x60` e `+0x8C` do contexto de batalha (ponteiro `DAT_080349D4`).
- Para o Lua: a regra "+10 por peça zerada no lado" é portável; confirmar o significado do valor 10 antes.
- Confiança: provável (loop e offsets batem com o HANDOFF; semântica de +10 é chute).

## 0x08038650 Fight_CompleteSlotMoveAndSetup
- Assinatura: `void Fight_CompleteSlotMoveAndSetup(void)`
- O que faz: chama `Fight_StepSlotTowardTarget` com o byte lido de `DAT_080386EC+DAT_080386F0` (índice do slot). Quando ela termina (retorno não-zero), monta dois registros de slot com `Slot_SetupFromRecordList` (um com o índice e outro com índice+3); no primeiro grava estado 6 em `+0x0A`, zera `+0x28`, incrementa `+0x29` e copia 0x40 bytes de `+0xC0` para `+0x00`; no segundo grava em `+0x0B` um valor `((idx-0x21)+side)*4 + 'Y'` e estado 3 em `+0x29`.
- Chama: `Fight_StepSlotTowardTarget`, `Slot_SetupFromRecordList`, `Mem_CopyBytes`. Chamada por: não verificado.
- Memória: registros de 0x40 bytes em `DAT_080386F8` (+0x40*idx); contexto de lado em `DAT_080386FC+0x60`; literal `DAT_080386F4+0x1AC`/`+0x1B0` = listas de registros.
- Para o Lua: a sequência de estados (6 -> 3) e a cópia de 0x40 bytes são o que importa; o cálculo do `'Y'` (0x59) depende do lado, a confirmar.
- Confiança: provável (ordem das chamadas e campos). Tamanho na lista (176 bytes) confere.

## 0x0803C7DC Fight_UpdateAnimScriptSlotFrame
- Assinatura: `void Fight_UpdateAnimScriptSlotFrame(void)`
- O que faz: por quadro, para um slot de script de animação (`base + idx*0x40`): se o contador `+0x30` é zero, chama `Fight_StepAnimScriptSlot` com o handler indexado por `+0x2B` (byte do slot); se `ctx+0x66 == -1` preenche o retângulo do mapa (`BgMap_FillRect16`) e limpa tiles (`Tile_ClearSlots`), senão, se `+0x2D == 0`, copia um retângulo com offset (`BgMap_CopyRectWithOffset`). Se o contador não é zero, decrementa-o. No fim faz um salto indireto não recuperado pelo Ghidra.
- Chama: `Fight_StepAnimScriptSlot`, `BgMap_FillRect16`, `Tile_ClearSlots`, `BgMap_CopyRectWithOffset`, e um salto indireto (jump table em 0x0803C8A2, não recuperada). Chamada por: não verificado.
- Memória: slot `DAT_0803C7FC + idx*0x40` (campos +0x02, +0x04, +0x2B, +0x2C, +0x2D, +0x2E, +0x2F, +0x30); contexto `DAT_0803C800` (`+0x66`).
- Para o Lua: a máquina por quadro (timer, decisão e avanço) é portável; a cópia de mapa é de hardware de fundo.
- Confiança: provável (chamadas de animação e de BG). A jump table pendente: ver Pendente.

## 0x0803D5A0 Fight_ResetStateFlagsAndCounter
- Assinatura: `void Fight_ResetStateFlagsAndCounter(void)`
- O que faz: limpa uma word local com `bios_cpufastset` (fill de memória com zero, endereço/tamanho por literal `DAT_0803D5D0`/`DAT_0803D5D4`), zera u16 em `+0x86` e byte em `+0x84` do contexto, e incrementa um contador global.
- Chama: `bios_cpufastset`. Chamada por: não verificado.
- Memória: contexto `DAT_0803D5D8` (+0x84, +0x86); contador `DAT_0803D5DC`.
- Para o Lua: reset de estado, portável como atribuição de zero.
- Confiança: provável. Tamanho na lista (428 bytes) não bate com o corpo lido (0x0803D5A0-0x0803D5CF = 48 bytes): a lista é aproximada, a função de fato é curta.

## 0x08040914 Dispatch_CallHandlerByByte1B
- Assinatura: `void Dispatch_CallHandlerByByte1B(void)`
- O que faz: despachante por tabela: lê o byte em `ctx+0x1B` (ctx pelo literal `DAT_08040930`), usa como índice ×4 na tabela de ponteiros (literal `DAT_0804092C`) e chama o alvo via `CallViaR0`.
- Chama: `CallViaR0`. Chamada por: não verificado.
- Memória: lê byte de contexto e a tabela de handlers.
- Para o Lua: não precisa (despachante).
- Confiança: provável (padrão de dispatch); subsistema: chute.

## 0x08041BA4 Slot_CountdownAndClearTile
- Assinatura: `void Slot_CountdownAndClearTile(void)`
- O que faz: no slot indicado por `ctx+0x2B` (registro de 0x40 bytes), subtrai 8 dos bytes `+0x08` e `+0x09`; quando `+0x08` chega a zero, limpa o tile do slot (`Tile_ClearSlots(idx,1)`) e incrementa um byte de estado no contexto (`+0x29`).
- Chama: `Tile_ClearSlots`. Chamada por: não verificado.
- Memória: registros de slot em `DAT_08041BEC` (+0x40*idx), contexto em `DAT_08041BF0`.
- Para o Lua: contagem regressiva por quadro, portável.
- Confiança: provável (mesmo padrão de Slot_CountdownStepAndClear). Nota: a lista dá 4060 bytes, mas o corpo lido tem 0x08041BA4-0x08041BE9 (70 bytes); a lista é aproximada.

## 0x0804545C Gfx_BlitSpriteFromOamAttr
- Assinatura: `void Gfx_BlitSpriteFromOamAttr(int dst, uint *attr, u16 x, int extra)`
- O que faz: decodifica uma palavra de atributo de sprite (bits de forma/tamanho via `OamSizeEncodingLookup`, índice de tile, paleta, prioridade), calcula a quantidade de tiles (metade se o campo baixo é zero) e chama `Tile_UnpackAndBlitBlock` com esses parâmetros.
- Chama: `OamSizeEncodingLookup`, `Tile_UnpackAndBlitBlock`. Chamada por: refs=0 no Ghidra (chamada só por ponteiro ou não detectada).
- Memória: lê a palavra de atributo (`*param_2`); escreve na VRAM via `Tile_UnpackAndBlitBlock`.
- Para o Lua: o formato do atributo é o que interessa (decodificação de OAM); o blit é de hardware.
- Confiança: provável (decodificação de OAM bem marcada).

## 0x0805718C Overworld_InitChoiceMenuScreen
- Assinatura: `void Overworld_InitChoiceMenuScreen(void)`
- O que faz: inicializa a tela de escolha do overworld: zera VRAM e descomprime fundos com `bios_lz77_vram`, monta quatro camadas de fundo (`FUN_08049210`, `FUN_080491B8`), e para cada uma das quatro linhas chama `Overworld_CheckStepAllowed(i)`: se permitido desenha a linha (`Map_DrawChoiceBackgroundRow(i)`) com outro bloco LZ77; senão usa o bloco padrão. Depois carrega gráficos com `Assets_LoadPairAndFastCopy` e `decompress_and_store`, zera campos do contexto (`+0x66`, `+0x67`, `+0x6E`, `+0x6F`) e define `+0x6C=0x10`, `+0x6D=3`. Termina com `GameState_AdvanceSubroutine`.
- Chama: `set_zeros`, `bios_lz77_vram`, `FUN_08049210`, `FUN_080491B8`, `Overworld_CheckStepAllowed`, `Map_DrawChoiceBackgroundRow`, `Assets_LoadPairAndFastCopy`, `decompress_and_store`, `GameState_AdvanceSubroutine`. Chamada por: não verificado.
- Memória: VRAM/tiles e mapas de fundo; contexto `DAT_0805743C` (+0x66..+0x6F).
- Para o Lua: a regra "linha permitida ou não" (`Overworld_CheckStepAllowed`) é a parte de jogo; o resto é carga de gráfico.
- Confiança: provável (pelos nomes de Overworld/Map). Ver também Pendente sobre `Overworld_CheckStepAllowed`.

## 0x0805C410 Screen_StepCounterAndDrawCell
- Assinatura: `void Screen_StepCounterAndDrawCell(void)`
- O que faz: chama `Anim_StepRecordCounter` no contador `*DAT_0805C44C + 0x50`; quando ela devolve não-zero, incrementa um contador global, zera u16 em `+0x3A` do contexto, chama `Screen_DrawCellAdvance`, grava o resultado em `+0x3A` e liga o bit 0x400 de REG_DISPCNT (BG2).
- Chama: `Anim_StepRecordCounter`, `Screen_DrawCellAdvance`. Chamada por: não verificado.
- Memória: IO REG_DISPCNT (0x04000000, bit 0x400); contexto `DAT_0805C44C+0x3A`; contador em `DAT_0805C450`.
- Para o Lua: o desenho da célula é o que importa.
- Confiança: provável (padrão de contador + redesenho).

## 0x0805CC3C Screen_InitSpritesAndLoadTiles
- Assinatura: `void Screen_InitSpritesAndLoadTiles(void)`
- O que faz: inicialização de tela com sprites: escreve três valores de configuração, carrega três blocos de tiles LZ77 (`Screen_LoadLZ77TileBlock`), desempacota um asset (`Asset_UnpackOrCopyBlock`) e copia com `bios_cpuset`, descomprime outro bloco (`decompress_and_store`), e preenche duas tabelas de entradas de sprite (grupos de 2, 2 e 5 entradas de 0x40 bytes, com posição e atributo fixos) no contexto; liga um bit em `+0x1440`. Incrementa um contador no fim.
- Chama: `Screen_LoadLZ77TileBlock`, `Asset_UnpackOrCopyBlock`, `bios_cpuset`, `decompress_and_store`. Chamada por: não verificado.
- Memória: tabela de sprites em `DAT_0805CD9C` (+0x40*idx, +0x1440); contador em `DAT_0805CDA8`.
- Para o Lua: a lista de entradas de sprite (posições e atributos) é dado de tela; portar só os valores.
- Confiança: provável (chamadas de carga de tela).

## 0x0805D704 Obj_UpdateFrameAndSetAttr
- Assinatura: `void Obj_UpdateFrameAndSetAttr(void)`
- O que faz: chama a função de estado indexada por `*DAT_0805D744` (tabela de ponteiros de 4 bytes via `CallViaR0`); depois, se o bit 0 da palavra em `*DAT_0805D748` está ligado, incrementa dois u16 de posição (`DAT_0805D74C`, +0 e +2); por fim chama `Obj_SetAttrFromTable` com o campo de bits 3..18 da palavra.
- Chama: `CallViaR0` (handler por estado), `Obj_SetAttrFromTable`. Chamada por: não verificado.
- Memória: índice de estado em `DAT_0805D744`; palavra de controle em `DAT_0805D748`; posição em `DAT_0805D74C`.
- Para o Lua: o avanço de posição e a escolha do atributo são portáveis.
- Confiança: provável.

## 0x0805DCCC Palette_FadeStepThenClear
- Assinatura: `void Palette_FadeStepThenClear(void)`
- O que faz: fade em nove passos: se o contador `ctx+0x84` é menor que 9, chama `Palette_FadeRowsToColorA` e `Palette_FadeRowsToColorB` com (contador*4) e incrementa o contador. Ao chegar a 9: mascara REG_DISPCNT duas vezes (via literais), limpa os slots de tile (`Tile_ClearSlots(0,0x80)`), zera dois bytes de estado e define um byte em 0x10.
- Chama: `Palette_FadeRowsToColorA`, `Palette_FadeRowsToColorB`, `Tile_ClearSlots`. Chamada por: não verificado.
- Memória: IO REG_DISPCNT (escrita por máscara); contexto `DAT_0805DCFC+0x84`; estados em `DAT_0805DD44/48/4C`.
- Para o Lua: o fade é um contador de 9 passos; portável como interpolação.
- Confiança: provável.

## 0x0805F358 Screen_EnableLayersAndDrawNumbers
- Assinatura: `void Screen_EnableLayersAndDrawNumbers(void)`
- O que faz: liga bits 0x200, 0x400, 0x800 e 0x1000 de REG_DISPCNT (camadas 1 a 3 e OBJ), zera dois campos do contexto, desenha dois números decimais (`Screen_DrawDecimalNumber`) com a prioridade lida de REG_BG1CNT, define REG_BLDY a partir de metade de um byte e incrementa um contador.
- Chama: `Screen_DrawDecimalNumber`. Chamada por: não verificado.
- Memória: IO REG_DISPCNT (0x04000000), REG_BG1CNT (leitura), REG_BLDY (escrita); contexto em `PTR_DAT_0805F404`; contador em `PTR_DAT_0805F410`.
- Para o Lua: mostra números na tela; portável como texto/contador.
- Confiança: provável (nomes de registradores e de desenho de número).

## 0x0805FC94 Screen_InitClearAndStartSong
- Assinatura: `void Screen_InitClearAndStartSong(void)`
- O que faz: limpa a VRAM de tiles em 0x06000000 com `bios_cpufastset` (zero), limpa a tabela de halfwords e quatro pares, grava cinco valores de posição/atributo no contexto, pede a música 0x12 (`snd_song_request`), define estado 0x10 e incrementa um contador.
- Chama: `bios_cpufastset`, `Screen_ClearHalfwordTable`, `Clear4PairsHalfwords`, `snd_song_request`. Chamada por: não verificado.
- Memória: VRAM 0x06000000 (escrita); contexto `DAT_0805FCF8`; estado em `DAT_0805FD08`; contador `DAT_0805FD0C`.
- Para o Lua: a música e a inicialização são o que importa.
- Confiança: provável.

## 0x08060A1C Anim_UpdateStepAndSprites
- Assinatura: `void Anim_UpdateStepAndSprites(void)`
- O que faz: passo de máquina de estados com handler indexado pelo byte `ctx+0x17` (`StateMachine_DispatchStep`); se devolve não-zero incrementa um contador; depois chama `Anim_UpdateSpritePosTable` e `Anim_LoadFrameEvery10`.
- Chama: `StateMachine_DispatchStep`, `Anim_UpdateSpritePosTable`, `Anim_LoadFrameEvery10`. Chamada por: não verificado.
- Memória: contexto `DAT_08060A48` (+0x17); tabela de handlers `DAT_08060A4C`; contador `DAT_08060A50`.
- Para o Lua: o laço por quadro é portável.
- Confiança: provável.

## 0x08061E98 Shop_FadeInStepThenBlink
- Assinatura: `void Shop_FadeInStepThenBlink(void)`
- O que faz: fade de entrada em nove passos (contador `ctx+0x84`): enquanto < 9, chama `Palette_FadeRowsToColorA/B` com (0x20 - contador*4) e incrementa; ao terminar, chama `Shop_BlinkCursorAttr`, zera u16 em `+2` de um bloco de estado e incrementa um contador.
- Chama: `Palette_FadeRowsToColorA`, `Palette_FadeRowsToColorB`, `Shop_BlinkCursorAttr`. Chamada por: não verificado.
- Memória: contexto `DAT_08061ED0+0x84`; estado `DAT_08061EF8`; contador `DAT_08061EFC`.
- Para o Lua: fade e piscar do cursor, portáveis.
- Confiança: provável (fade igual ao de Palette_FadeStepThenClear, com sentido inverso; nome "Shop" pelo cursor, a confirmar).

## 0x08063254 BattlePanel_UpdateSelectionFrame
- Assinatura: `void BattlePanel_UpdateSelectionFrame(void)`
- O que faz: chama `StateMachine_DispatchStep` no handler de `ctx+0x3C`; monta uma palavra de estado a partir de dois bytes (`+0x31`, `+0x33`) com bit 0x4000; se duas halfwords de entrada têm o padrão 0xC000 == 0x4000, escolhe o byte de slot (`+0x32`) e o campo `+0x34`, redesenha o painel (`BattlePanel_InitOrRedraw(1)`), copia string de texto (`Text_CopyFEStringTo20`), faz layout de glifos (`Text_LayoutGlyphSpritesTileBase`), inicializa slot (`Obj_InitSlotAndLoad`) e incrementa um contador. Sempre chama `BattlePanel_DrawSlotText` ao fim.
- Chama: `StateMachine_DispatchStep`, `BattlePanel_InitOrRedraw`, `Text_CopyFEStringTo20`, `Text_LayoutGlyphSpritesTileBase`, `Obj_InitSlotAndLoad`, `BattlePanel_DrawSlotText`. Chamada por: não verificado.
- Memória: contexto de painel (ponteiros `DAT_080632B8`, `DAT_080632BC`, `DAT_080632C0`); seleção em `DAT_08063348`; contador `DAT_08063350`.
- Para o Lua: a regra de seleção (qual slot ficou escolhido) é o que importa.
- Confiança: provável (nomes dos chamados de painel/texto).

## 0x08063EBC Anim_FinishClearAndLoadSlot
- Assinatura: `void Anim_FinishClearAndLoadSlot(void)`
- O que faz: quando `Anim_StepRecordCounter` (contador em `ctx+0x50`) devolve não-zero: limpa 0x40 words com `bios_cpufastset`, incrementa um contador, define `ctx+0x3C = 600` (timer) e chama `Obj_InitSlotAndLoad(1,0,1)`.
- Chama: `Anim_StepRecordCounter`, `bios_cpufastset`, `Obj_InitSlotAndLoad`. Chamada por: não verificado.
- Memória: contexto `DAT_08063EF8`; contador `DAT_08063F04`.
- Para o Lua: o timer de 600 é o dado a portar.
- Confiança: provável.

## 0x080648E0 Anim_UpdateBlinkFrameAndTimer
- Assinatura: `void Anim_UpdateBlinkFrameAndTimer(void)`
- O que faz: avança o contador de quadros `ctx+0x3A`; a partir de bit 0x20 desse contador define duas words de atributo (um campo de posição e um de flag 0x100), o que dá um piscar. Se o bit 0 da entrada em `*DAT_08064954` está ligado, inicia um registro de timer (`Obj_InitTimerRecord(ctx+0x50,0,0,1,0xC0)`) e incrementa um contador.
- Chama: `Obj_InitTimerRecord`. Chamada por: não verificado.
- Memória: atributos em `DAT_08064948`; contexto `DAT_0806494C` (+0x3A, +0x50); entrada `DAT_08064954`; contador `DAT_08064958`.
- Para o Lua: o piscar é função do quadro (bit 0x20); portável.
- Confiança: provável.

## 0x08065A94 (sem nome; não renomeado)
- Assinatura: `void FUN_08065a94(void)`
- O que faz: despacha um passo de máquina de estados escolhendo a entrada de tabela 0x10 ou 0x14 conforme `ctx+8` (se não zero, usa 0x14). Se o passo termina: quando `ctx+8 == 0` incrementa um contador e zera `ctx+0x3A`; senão, se o bit 0 da entrada está ligado, toca SFX 0xD, zera dois campos e subtrai 2 de um contador. Termina com salto indireto não recuperado pelo Ghidra.
- Chama: `StateMachine_DispatchStep`, `snd_sfx_request`, salto indireto (jump table em 0x08065AFE, não recuperada). Chamada por: não verificado.
- Memória: contexto `DAT_08065ACC`; tabela de handlers `DAT_08065AC8`; flags `DAT_08065B00`; contexto 2 `DAT_08065B04`.
- Para o Lua: a regra de seleção (ctx+8) é o que importa; subsistema não identificado.
- Confiança: chute (subsistema e significado); não renomeado.

## 0x08066714 Text_BuildMessageAndStep
- Assinatura: `void Text_BuildMessageAndStep(void)`
- O que faz: monta uma mensagem num buffer em `ctx+0x270` (escreve 0xFB, depois 0x00 e 0x0C, depois copia bytes de uma string apontada por `ctx+0x3C` até o terminador 0xFF, mais dois bytes finais). Chama `StateMachine_DispatchStep` sobre esse buffer; quando o passo termina, escolhe um de dois registros (conforme `ctx+0x2A`), grava uma halfword de estado em `+6` (0xFF00 ou outro valor, conforme `ctx+0x2A`) e, se as duas halfwords de estado valem 0xFF00, chama `Obj_ResetStateFields`; se são iguais inicia timer (`Obj_InitTimerRecord`) e define estado 5; senão define estado 3 e 6.
- Chama: `StateMachine_DispatchStep`, `Obj_ResetStateFields`, `Obj_InitTimerRecord`. Chamada por: não verificado.
- Memória: buffer de mensagem em `ctx+0x270` (ctx = `DAT_08066790`); string de origem em `ctx+0x3C` do contexto `DAT_0806678C`; registros em `DAT_080667F4`/`DAT_080667A0`.
- Para o Lua: o formato da mensagem (0xFB, 0x0C, terminador 0xFF) é o que precisa ser entendido para portar o texto; os valores são códigos de controle, não texto do jogo.
- Confiança: provável (formato de buffer e terminador 0xFF bem definidos; significado dos códigos: chute).

## 0x08068978 (sem nome; não renomeado)
- Assinatura: `int FUN_08068978(void)` (devolve bool em r0)
- O que faz: liga BG0 (`REG_DISPCNT |= 0x100`). Se `*DAT_08068A3C` é múltiplo de `*DAT_08068A40`, calcula com divisão com sinal uma posição numa tabela de halfwords e grava quatro registros de 8 bytes (ponteiro para `local_24`, valor de tabela + constante, flag). Devolve se o valor de `*DAT_08068A3C` é igual a `*DAT_08068A40 * 0x10`; nesse caso desliga uma parte de REG_DISPCNT.
- Chama: `aux_mod_s32`, `aux_div_s32` (auxiliares de aritmética, README). Chamada por: não verificado.
- Memória: IO REG_DISPCNT; contexto e tabela via literais.
- Para o Lua: a lógica (período, passo e fim) é o que importa; significado do contador não identificado.
- Confiança: chute (função de barra/contagem sem nome claro). Não renomeado.

## 0x0806BB64 (sem nome; não renomeado)
- Assinatura: `void FUN_0806bb64(void)`
- O que faz: atualiza um vetor de até ~12 células de bytes (`ctx+0xF..`): a célula inicial vem de `mod 3` de um byte de tabela; cada célula seguinte sobe ou desce por faixas de `mod 7` do byte correspondente de uma tabela (casos 0; 1-3 decrementa com piso em 1; 4-6 incrementa com teto em 2). Incrementa um contador no fim.
- Chama: `aux_mod_u32`. Chamada por: não verificado.
- Memória: vetor de estado em `DAT_0806BBB8+0xF`; tabela de bytes em `DAT_0806BBBC`; índice em `DAT_0806BBC0`; contador em `DAT_0806BBC4`.
- Para o Lua: a regra de transição é portável se o uso for entendido.
- Confiança: chute (subsistema e uso das células não identificados). Não renomeado.

## 0x0806E2A0 Screen_InitRowBufferAndFx
- Assinatura: `int Screen_InitRowBufferAndFx(void)`
- O que faz: descomprime dois blocos LZ77 para VRAM (`bios_lz77_vram`) com endereços escolhidos por um índice de contexto; copia 20 linhas (0x14) de 30 halfwords de uma origem para um buffer local, zerando as duas últimas halfwords de cada linha; aponta a estrutura de DMA para o buffer e para VRAM 0x05000000; chama `fx_screen_setup_a(2,1,0)` e `Obj_InitSlotAndLoad(1,2,0)`; liga REG_DISPCNT 0x200 (BG1) e grava 4 no contexto e 1 num contador.
- Chama: `bios_lz77_vram`, `fx_screen_setup_a`, `Obj_InitSlotAndLoad`. Chamada por: não verificado.
- Memória: IO REG_DISPCNT (0x200); VRAM 0x05000000 (paleta); contexto `DAT_0806E39C` (+0x0C, +0x04); contador `DAT_0806E3C0`.
- Para o Lua: o efeito é uma tela de fx com linhas; portar só se for usado.
- Confiança: provável (chamadas de carga e efeito de tela).

## 0x0806EC30 Screen_InitDmaAndDispcnt
- Assinatura: `void Screen_InitDmaAndDispcnt(void)`
- O que faz: configura quatro transferências de DMA (registros de origem, destino e controle em blocos de 3 words, padrão DMA3) para VRAM e paleta; força REG_DISPCNT = 0x60; escreve valores de inicialização em uma estrutura de 0x7C+ halfwords (3, 1, 0xB, 0x7C0, 0x100) e zera quatro pares de halfwords de estado.
- Chama: nenhuma função; só escrita direta. Chamada por: não verificado.
- Memória: IO DMA3 (0x040000D4..) e REG_DISPCNT (0x04000000); VRAM 0x06000000 e 0x05000000; estrutura em `PTR_DAT_0806ECE8`.
- Para o Lua: não precisa (inicialização de hardware).
- Confiança: provável (padrão DMA reconhecível; endereço exato dos registros não resolvido).

## 0x08070C0C Screen_InitMenuTilesAndFx
- Assinatura: `void Screen_InitMenuTilesAndFx(void)`
- O que faz: descomprime cinco blocos LZ77 para VRAM (`bios_lz77_vram`); preenche uma estrutura de DMA (quatro transferências de três words) e a configuração de uma tabela de 0x24 halfwords com valor 0xC40; zera dois campos de estado; chama `fx_screen_setup_a(4,2,0)`; liga REG_DISPCNT 0x1E00 (camadas 1 a 3 e OBJ conforme bits); marca um contador.
- Chama: `bios_lz77_vram`, `fx_screen_setup_a`. Chamada por: não verificado.
- Memória: IO REG_DISPCNT; VRAM 0x05000000 e tiles; contexto `DAT_08070D68` (+0x0C, +0x0D); contador `DAT_08070D70`.
- Para o Lua: carga de tela (tiles e configuração de fx); portar só o que for usado.
- Confiança: provável.

## 0x08071744 Snd_FadeOutThenSetState
- Assinatura: `void Snd_FadeOutThenSetState(void)`
- O que faz: se um byte de flag do contexto (`DAT_08071768+DAT_0807176C`) é zero, pede fade de som (`snd_fade_request(0x20)`), grava 0x11 em um u16 de estado (`+4`) e define estado 2.
- Chama: `snd_fade_request`. Chamada por: não verificado.
- Memória: flag de contexto; estado em `DAT_08071770`; `DAT_08071774` = byte de estado.
- Para o Lua: a regra de transição (pedir fade só quando a flag está zerada) é portável.
- Confiança: provável.

## 0x08072234 Preview_InitCompositeAndMotion
- Assinatura: `longlong Preview_InitCompositeAndMotion(void)`
- O que faz: prepara um ponteiro de dados com o valor 0x200, aponta uma estrutura para `*DAT_080722A0 + DAT_080722A4`, chama `BuildCompositePreviewSpriteLayer` com parâmetros fixos (0x2B, 0x40, 0x42, 2, 8 ...) e `MotionPlayStep(0,0x2C,0)`; incrementa um byte de contador (+0x10) e zera +0x11.
- Chama: `BuildCompositePreviewSpriteLayer`, `MotionPlayStep`. Chamada por: não verificado.
- Memória: contexto `DAT_080722AC` (+0x10, +0x11); estrutura em `DAT_0807229C`.
- Para o Lua: preview de peça montada com animação; portar a composição quando o preview for portado.
- Confiança: provável (nome pelos chamados `BuildCompositePreview*`).

## 0x0807D4F4 SoftFloat_Binary64Op
- Assinatura: `void SoftFloat_Binary64Op(u32 a_lo, u32 a_hi, u32 b_lo, u32 b_hi)` (com saída empacotada por `sf64_pack`)
- O que faz: rotina de ponto flutuante por software de 64 bits: desempacota dois operandos (`sf_unpack`), trata casos de zero/infinito/NaN por ramos, faz multiplicação 64x64 (`sf_mul32x32_64`) e normalização, e empacota o resultado. O ramo principal é uma operação de 64 bits com laço de normalização; qual operação exata (mul ou div) não ficou confirmado só pela leitura.
- Chama: `sf_unpack`, `sf_mul32x32_64`, `sf64_pack`. Chamada por: não verificado.
- Memória: só pilha (variáveis locais); constantes em literais.
- Para o Lua: não precisa; o LÖVE usa números de ponto flutuante nativos.
- Confiança: provável (é soft-float, pela família sf_); operação exata: chute.

## 0x0807E3E0 SoftFloat_Compare32
- Assinatura: `void SoftFloat_Compare32(u32 a, u32 b)`
- O que faz: desempacota dois valores de 32 bits em formato soft-float (`sf32_unpack`) e compara com `sf_cmp_core`. O resultado vai para o chamador via flags/registradores (não é devolvido explicitamente).
- Chama: `sf32_unpack`, `sf_cmp_core`. Chamada por: não verificado.
- Memória: só pilha.
- Para o Lua: não precisa (comparação de float nativo).
- Confiança: confirmado (código de 4 chamadas, nome do auxiliar claro). Tamanho na lista (112) não bate com o corpo (38 bytes); lista aproximada.

## Pendente
- Pendente: saltos indiretos não recuperados pelo Ghidra em 0x0803C7DC (tabela em 0x0803C8A2) e 0x08065A94 (tabela em 0x08065AFE).
- Pendente: 0x08065A94, 0x08068978, 0x0806BB64 sem nome (subsistema não confirmado).
- Pendente: chamadores ("Chamada por") não verificados em todas as funções (exigiria get_function_xrefs).
- Pendente: `Fight_SelectActionSource`, `Overworld_CheckStepAllowed`, `Fight_AdvanceActionScriptCursor` e `GameState_AdvanceSubroutine` só citados, não lidos.
- Pendente: tamanhos da lista FL7_lista.txt são aproximados e não batem com o corpo em vários casos (0x08041BA4, 0x0803D5A0, 0x0807E3E0).
- Pendente: FL7_simbolos.csv ainda não está fundido no re/symbols/mtbe.csv.
