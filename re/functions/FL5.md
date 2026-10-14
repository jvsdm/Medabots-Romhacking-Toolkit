# FL5: documentação das funções da faixa FL5 (lista FL5_lista.txt)

Ghidra MCP usado sem salvar. Nomes já aplicados no Ghidra ficam listados em FL5_simbolos.csv.

## 0x080025B4 Menu_ReadPadMoveCursor
- Assinatura: `void Menu_ReadPadMoveCursor(void)`
- O que faz: lê um byte de estado de entrada (ponteiro PTR_DAT_0800268c). Bits 0x40/0x80 mexem no contador
  short +2 (vertical), 0x20/0x10 no short +0 (horizontal). Bit 0x02 inverte um flag em +0x88 da struct de tela;
  bit 0x01 zera +0x0E e incrementa um contador e inverte um flag em +0x85; bit 0x02 (do byte de eventos) grava 1
  e zera o contador; bits 0x08 e 0x04 chamam `Obj_InitSlotAndLoad(0,0,0)` e somam 2 ou 3 no contador.
- Chama: `Obj_InitSlotAndLoad`. Chamada por: não verificado.
- Memória: lê/escreve por ponteiros (literal pool 0x0800268C..0x080026A0); não conferi os destinos reais.
- Para o Lua: não precisa (lógica de cursor de menu; depende do estado de entrada do GBA).
- Confiança: provável (função de cursor/entrada de menu; destinos pelo literal pool não resolvidos).

## 0x08002DF4 Gfx_SlideWipeStep
- Assinatura: `void Gfx_SlideWipeStep(void)`
- O que faz: quando o estado da tela (struct em DAT_08002eac, byte +0x8E) não é 2, faz fade de paleta por linhas
  (`Palette_FadeRowsToColorA/B`) até 0x11 passos (contador +0x84). Depois copia dois retângulos de mapa de fundo
  (`BgMap_CopyRectWithOffset`) com deslocamento vindo do contador +0x88 (0..0x12). Ao chegar em 0x13 incrementa
  um contador global.
- Chama: `Palette_FadeRowsToColorA`, `Palette_FadeRowsToColorB`, `BgMap_CopyRectWithOffset`. Chamada por: não verificado.
- Memória: struct de tela em DAT_08002eac (+0x84, +0x88, +0x8E); destinos de paleta e de mapa vêm de literal pool.
- Para o Lua: porta como animação de transição (fade + deslize de faixas de tiles), não o hardware.
- Confiança: provável (transição de tela por frame).

## 0x080075AC PartsGrid_SceneInit
- Assinatura: `void PartsGrid_SceneInit(void)`
- O que faz: inicializa a tela da grade de peças: ajusta REG_DISPCNT (mascara e depois liga 0x1F60), zera
  registradores de BG, descomprime 8 blocos de gráficos (`decompress_and_store`), carrega tiles (`LoadTilesWithMalias`),
  limpa VRAM/paleta/OAM com `bios_cpufastset` (0x70, 8, 0x40 palavras), chama a sequência `PartsGrid_*` de desenho,
  `Sprite_InitPairA`/`Sprite_SetPairSizeB`, e desenha o mapa de fundo. Se o estado +0x3C == 4 e o flag +0x4B é 0,
  marca o flag e toca a música 7 (`snd_song_request`). Zera +0x84 e incrementa um contador global (DAT_08007830).
- Chama: `decompress_and_store`, `LoadTilesWithMalias`, `bios_cpufastset`, `PartsGrid_SplitRowCol`,
  `PartsGrid_UploadCellTiles`, `PartsGrid_DrawEntries`, `PartsGrid_DrawIconLayers`, `PartsGrid_DrawCursorCell`,
  `PartsGrid_UpdateSelectedPreview`, `PartsGrid_DrawCellLabel`, `PartsGrid_CountFlagged30`,
  `PartsGrid_DrawSelectedIcon/Panel/Stats`, `Sprite_InitPairA`, `Sprite_SetPairSizeB`, `SetGpuBlendMode`,
  `snd_song_request`. Chamada por: não verificado.
- Memória: REG_DISPCNT (0x04000000); VRAM 0x05000000; struct da tela em DAT_0800782c (+0x3C, +0x3F, +0x4B, +0x84).
- Para o Lua: porta como "montar tela de peças" (ler dados de peça e desenhar a grade); o resto é hardware.
- Confiança: provável (nome pelo conjunto de chamadas PartsGrid_*; ids de gráfico não verificados).

## 0x0800BAD8 Dispatch_StateHandlerTable
- Assinatura: `void Dispatch_StateHandlerTable(void)`
- O que faz: despacho genérico. Lê o byte de estado em +0x33 de uma struct (ponteiro em DAT_0800baf4), usa-o
  como índice numa tabela de ponteiros de função (DAT_0800baf0) e chama o alvo com `CallViaR0`.
- Chama: `CallViaR0` (trampolim `bx r0`). Chamada por: não verificado.
- Memória: lê struct em DAT_0800baf4 (+0x33) e tabela em DAT_0800baf0; não resolvidos.
- Para o Lua: não precisa, é só uma máquina de estados (porta como tabela de funções).
- Confiança: provável quanto ao despacho; tabela e struct não identificadas.


## 0x0800D168 Dispatch_StateHandlerTable_B
- Assinatura: `void Dispatch_StateHandlerTable_B(void)`
- O que faz: mesmo padrão de 0x0800BAD8: lê o byte +0x33 de uma struct (DAT_0800d184), usa como índice numa tabela
  de ponteiros (DAT_0800d180) e chama com `CallViaR0`.
- Chama: `CallViaR0`. Chamada por: não verificado.
- Memória: struct em DAT_0800d184 (+0x33); tabela em DAT_0800d180.
- Para o Lua: não precisa (despacho de estado; porta como tabela de funções).
- Confiança: provável (despacho); tabela não identificada.

## 0x0800EA2C PartDetail_SceneStep
- Assinatura: `void PartDetail_SceneStep(void)`
- O que faz: passo por frame tela de detalhe de peça: `Sprite_IncrementXYIfFlag`, `PartsGrid_DrawLargeDisplayIfOwned`,
  limpa dois flags de sprite (halfwords +0x40 e +0x80 da struct em DAT_0800ea80) com máscara, recarrega 0x380 bytes
  de tiles no slot (0x12,5), zera o byte +0x4A e chama `PartDetail_SetupAttributeCursorSprite`; incrementa um contador.
- Chama: `Sprite_IncrementXYIfFlag`, `PartsGrid_DrawLargeDisplayIfOwned`, `LoadTilesWithMalias`,
  `PartDetail_SetupAttributeCursorSprite`. Chamada por: não verificado.
- Memória: structs em DAT_0800ea80 (+0x40, +0x80), DAT_0800ea90 (+0x4A), contador DAT_0800ea94.
- Para o Lua: porta como desenho da tela de detalhe da peça; resto é sprite/tile.
- Confiança: provável (nome pelos callees PartDetail_/PartsGrid_).

## 0x08011344 Scene_InitBgAndLoadRecord
- Assinatura: `void Scene_InitBgAndLoadRecord(void)`
- O que faz: zera slots de tile e pares de slot (`Tile_ClearSlots`, `Slots_ClearFourPairs`), preenche com 0 os quatro
  mapas de fundo BG0..BG3 (`BgMap_FillRect16`, 32x32), liga blend (`SetGpuBlendMode(5,0x3F,0x20)`), monta um bloco de
  cópia (fonte = registro, destino = 0x05000000) e copia 4 bytes de um registro de 8 bytes (índice = byte +0x186 da
  struct em DAT_08011484) para a struct em DAT_08011488 (+0x133..+0x136). Calcula o slot por id com
  `Slot_FindByIdPlus1`, chama `set_state_timed` e incrementa o índice +0x186.
- Chama: `Tile_ClearSlots`, `Slots_ClearFourPairs`, `BgMap_FillRect16`, `SetGpuBlendMode`, `Slot_FindByIdPlus1`,
  `set_state_timed`. Chamada por: não verificado.
- Memória: BGxCNT via literal pool (0x08011460..0x0801146C); VRAM/paleta 0x05000000; structs em DAT_08011484 (+0x16E,
  +0x170..+0x173, +0x186) e DAT_08011488 (+0x133..+0x136).
- Para o Lua: porta como entrada de cena com carga de um registro (qual registro é chute).
- Confiança: provável (reset de BG e cópia de registro confirmados no código; o que é o registro não).

## 0x08013288 Fight_EntryAnimStep
- Assinatura: `void Fight_EntryAnimStep(void)`
- O que faz: passo por frame da entrada da batalha: desenha overlay dos bots ativos (`Fight_DrawActiveBotsOverlay`),
  anima quadros de entrada (`Fight_AnimateEntryFrames`), move sprites compostos (`Sprite_UpdateComposite4Motion`),
  incrementa o byte +0x84 de um contexto, desenha texto de status da ação. Ao contador +0x86 chegar a 0x10 limpa tiles,
  preenche BG0/BG1 e copia paleta, depois grava 0x0D no estado (DAT_0801332c).
- Chama: `Fight_DrawActiveBotsOverlay`, `Fight_AnimateEntryFrames`, `Sprite_UpdateComposite4Motion`,
  `Fight_InitKindObjectsForState`, `Fight_DrawSelectedActionStatusText`, `Tile_ClearSlots`, `BgMap_FillRect16`,
  `bios_cpufastset`. Chamada por: não verificado.
- Memória: contexto em PTR_DAT_08013314 (+0x80) e PTR_DAT_08013318 (+0x84, +0x86); estado em DAT_0801332c.
- Para o Lua: porta como animação de entrada da batalha (bots entram, depois a tela de luta).
- Confiança: provável (nome pelos callees Fight_*; offsets de contexto não mapeados ao fight_information).

## 0x0801427C Fight_StepBattleFrame
- Assinatura: `void Fight_StepBattleFrame(void)`
- O que faz: passo principal por frame da batalha. Lê o byte de estado +0x55 de um contexto (DAT_080142a4). Se é 0,
  atualiza timers de efeito secundário e posições de dígitos de contador. Depois testa bits 0x02/0x04/0x08/0x10 do
  byte: cada um escolhe um timer e grava 0x60/0x68/0x70/0x78. Sem bit ativo, anima os bots (exceto tipos de status
  0x0F/0x10), desenha overlay, atualiza sprites dos seis bots e eventos de posição; quando `Fight_UpdateBotPositionEvents`
  retorna != 0, faz o passo 544 e incrementa um contador.
- Chama: `Fight_TickSecondaryEffectTimers`, `Fight_UpdateCounterDigitPositions`, `Fight_TickActiveBotsExceptStatusTypes0F10`,
  `Fight_DrawActiveBotsOverlay`, `Fight_UpdateSixBotSprites`, `Fight_UpdateBotPositionEvents`,
  `Fight_TickActiveBotsExceptStatusTypes0F10_Step544`. Chamada por: não verificado.
- Memória: contexto em DAT_080142a4 (+0x55); timers em DAT_080142a8/bc/d0/e4/08014310.
- Para o Lua: porta como loop de frame da batalha (timers e animação dos bots).
- Confiança: provável (nome pelos callees Fight_*; a escolha dos timers 0x60..0x78 é lida no código).

## 0x0801C23C Fight_TimerStepAdvance
- Assinatura: `void Fight_TimerStepAdvance(void)`
- O que faz: se o bit 0 do byte apontado por DAT_0801c28c está ligado, incrementa um contador short em +0xC4 de um
  contexto (DAT_0801c290). Quando o contador chega a (tabela do bot +0x1744 indexada por byte +0x87 * 0x100) + 4,
  zera o byte +0x9D e incrementa o byte +0x9C de outra struct (DAT_0801c294).
- Chama: nenhuma. Chamada por: não verificado.
- Memória: DAT_0801c28c (flag), DAT_0801c290 (+0xC4), DAT_0801c294 (+0x87, +0x9C, +0x9D), bloco +0x1744 por bot.
- Para o Lua: porta como temporizador de passo (espera N frames, depois avança a máquina de ação).
- Confiança: provável no comportamento; nomes de struct e o significado de +0x1744 não confirmados.

## 0x080346C4 Fight_ChooseRandomOpponentTarget
- Assinatura: `void Fight_ChooseRandomOpponentTarget(void)`
- O que faz: passo de ação da batalha. Lê o lado do ator (byte +0x87 da fight_information em DAT_08034700), calcula
  o lado adversário (`Fight_GetOpponentSideBaseIndex`) e grava em +0x60 (side base, confirmado no HANDOFF). Escolhe um
  bot ativo aleatório nesse lado (`Fight_PickRandomActiveBotInSide`) e grava id+1 no bloco do bot ator
  (base DAT_08034704 + bot*0x100 + 0xE0). Depois sorteia a parte alvo, copia o alvo para a fight_information e avança
  o cursor do script de ação.
- Chama: `Fight_GetOpponentSideBaseIndex`, `Fight_PickRandomActiveBotInSide`, `Fight_SelectRandomTargetPartForSelectedBot`,
  `Fight_CopySelectedTargetToFightInfo`, `Fight_AdvanceActionScriptCursor`. Chamada por: não verificado.
- Memória: fight_information (DAT_08034700: +0x60, +0x87); bloco do bot (DAT_08034704 +0xE0).
- Para o Lua: porta como sorteio de alvo (adversário aleatório, depois parte aleatória).
- Confiança: provável (lado +0x60 e base do bot batem com o HANDOFF; o alvo é lido no código).

## 0x0801275C Util_AddScaledByteToShort (sem rename)
- Assinatura: `void FUN_0801275c(void)`
- O que faz: uma linha. Soma `byte(+0x16B) * 5` ao short +0x16C de um contexto (ponteiro em DAT_08012778).
- Chama: nenhuma. Chamada por: não verificado.
- Memória: contexto em DAT_08012778 (+0x16B lido, +0x16C escrito).
- Para o Lua: porta como `ctx.v16C = ctx.v16C + ctx.b16B * 5`.
- Confiança: confirmado (operação lida no código); o significado do campo não.

## 0x08035410 Slot_BlinkRepeatStep
- Assinatura: `void Slot_BlinkRepeatStep(int slot, byte *cnt, byte *rep)`
- O que faz: liga o bit 0x8000 de um halfword do slot (`slot*0x40 + DAT_08035448`). Se `*rep == 0`, incrementa `*cnt`;
  ao ficar ímpar limpa uma máscara do halfword. Se `*rep != 0`: quando `*cnt == *rep` zera `*cnt`, limpa outra máscara
  e decrementa `*rep`; senão incrementa `*cnt`. Parece um piscar com número de repetições.
- Chama: nenhuma. Chamada por: não verificado.
- Memória: slot OAM-like em DAT_08035448 (halfword de flags, +0x40 por slot); máscaras em DAT_0803544C e DAT_08035468.
- Para o Lua: porta como piscar de sprite com contador de repetições.
- Confiança: provável (lógica lida; semântica do halfword não confirmada).

## 0x08038428 Effect_SpawnTimedSprite
- Assinatura: `void Effect_SpawnTimedSprite(void)`
- O que faz: a cada 8 frames (contador halfword +0x86 de um contexto em DAT_08038504, `& 7 == 0`) pega um slot livre
  (`Slot_FindFree`, limite 10), limpa o tile do slot, monta um objeto de 0x40 bytes: posição inicial da tabela em
  DAT_0803851c sorteada com `randbyte() & 3`, espelhada se o bit 0 de DAT_08038518+0xF8 está ligado. Define timer 0x78,
  tamanho 0x40 e variação de sprite.
- Chama: `Slot_FindFree`, `Tile_ClearSlots`, `randbyte`. Chamada por: não verificado.
- Memória: contexto DAT_08038504 (+0x86); slots de objeto em DAT_0803850c (+0x40 por slot); tabelas de posição em
  DAT_0803851c e DAT_08038584.
- Para o Lua: porta como emissor de efeito de partícula com posição aleatória.
- Confiança: provável (lógica lida; subsistema (batalha ou outro) não confirmado).

## 0x0803C680 Anim_SlotScriptTick
- Assinatura: `void Anim_SlotScriptTick(void)`
- O que faz: tick por frame de dois objetos de animação. (1) Se o contador +0x16 do slot está zerado, liga o bit
  0x8000 e roda `Anim_RunSlotScript` com o script escolhido pela tabela DAT_0803c6ec (índice = byte +0x2D); senão
  decrementa. (2) Se o timer +0x2B do segundo objeto zerou, chama `Anim_AdvanceSlotTileOffset`, soma o delta da tabela
  DAT_0803c734 (índice = byte +0x23) a +4 e avança +0x23 módulo 0x20; senão decrementa. Termina por jump table
  indireta (não recuperada).
- Chama: `Anim_RunSlotScript`, `Anim_AdvanceSlotTileOffset`, e uma jump table não recuperada. Chamada por: não verificado.
- Memória: slots de 0x40 bytes em DAT_0803c6a0 e DAT_0803c6f0; tabelas DAT_0803c6ec e DAT_0803c734.
- Para o Lua: porta como dois contadores de animação por slot que disparam scripts e avançam tiles.
- Confiança: provável (lógica lida; a jump table no fim não recuperada).

## 0x0803D2A8 FightPreview_StepSideLoad
- Assinatura: `void FightPreview_StepSideLoad(void)`
- O que faz: passo de carga da tela de prévia da batalha por slot. Se o timer +0x2B zerou: quando +0x2E está marcado,
  carrega tiles do lado (`FightPreview_LoadSideTileSlots`, `FightPreview_LoadSideTiles`), linhas de ícones de peça
  (`FightPreview_LoadPartIconRows`) e pares de ícones (`FightPreview_DrawPartIconPairs`), e limpa +0x2E. Depois conta
  quatro peças com `FightPreview_ConsumePartCount` (índices 0..3); recarrega ícones e, se a soma der 4, limpa um slot
  de tile. Sem timer, decrementa +0x2B ou +0x2C.
- Chama: `FightPreview_LoadSideTileSlots`, `FightPreview_LoadSideTiles`, `FightPreview_LoadPartIconRows`,
  `FightPreview_DrawPartIconPairs`, `FightPreview_ConsumePartCount`, `Tile_ClearSlots`. Chamada por: não verificado.
- Memória: slots de 0x40 bytes em DAT_0803d2d4 (+0x2B, +0x2C, +0x2D, +0x2E); contexto DAT_0803d348 (+0x87).
- Para o Lua: porta como carga progressiva das peças na prévia de batalha.
- Confiança: provável (nome pelos callees FightPreview_*; lógica lida).

## 0x0803FC20 Gfx_LoadTileSetStep3
- Assinatura: `void Gfx_LoadTileSetStep3(void)`
- O que faz: passo de carga em três etapas, pelo estado +0x26 de um contexto (DAT_0803fc3c). Estado 0, 1 ou 2: descomprime
  um bloco diferente (`decompress_and_store`) e carrega tiles no bloco 0xD (`LoadTilesWithMalias`, tamanho 1). Depois
  incrementa o estado; ao fim incrementa +4.
- Chama: `decompress_and_store`, `LoadTilesWithMalias`. Chamada por: não verificado.
- Memória: contexto DAT_0803fc3c (+0x26, +0x04); fontes de dados nos literal pools 0x0803FC54..0x0803FCB4.
- Para o Lua: porta como carga sequencial de três gráficos de cena.
- Confiança: confirmado (fluxo lido); nomes dos gráficos não identificados.

## 0x080419CC Slot_SlideStepClamp
- Assinatura: `void Slot_SlideStepClamp(void)`
- O que faz: escolhe o slot pelo byte +0x2B do contexto (DAT_08041a24) e soma 8 aos bytes +8 e +9 do slot (`slot*0x40 +
  DAT_08041a20`). Se o byte +8 passou de 0x3F, trava os dois em 0x40 e incrementa o contador +0x29. Zera +0x84.
- Chama: nenhuma. Chamada por: não verificado.
- Memória: slots de 0x40 bytes em DAT_08041a20; contexto DAT_08041a24 (+0x29, +0x2B, +0x84).
- Para o Lua: porta como deslize de 8 px por frame até 64 px, com fim de passo.
- Confiança: provável (lógica lida; o significado de +8/+9 como posição não confirmado).

## 0x0804F9C4 Event_OpWaitMapObjectStep
- Assinatura: `void Event_OpWaitMapObjectStep(void)`
- O que faz: opcode de evento de dois estados (byte em DAT_0804f9d8 + DAT_0804f9dc). Estado 0: chama
  `Map_StartObjectStep` com o parâmetro do objeto (byte +10 de DAT_0804f9f8) e vai para 1. Estado 1: chama
  `Map_TickObjectStep` a cada frame e, quando retorna != 0, chama `Event_EndOpcode` (fim do opcode).
- Chama: `Map_StartObjectStep`, `Map_TickObjectStep`, `Event_EndOpcode`. Chamada por: tabela de opcodes de evento
  (não verificado qual opcode; ver tabela 0x083D222C no HANDOFF).
- Memória: estado em DAT_0804f9d8+DAT_0804f9dc; parâmetros em DAT_0804f9f8 (+10) e DAT_0804fa18 (+10).
- Para o Lua: porta como opcode de evento que espera um passo de objeto de mapa terminar.
- Confiança: provável (fluxo lido; número do opcode não confirmado).

## 0x0805C17C Screen_SceneInit
- Assinatura: `void Screen_SceneInit(void)`
- O que faz: init de tela: ajusta quatro halfwords de posição (DAT_0805c270/78/7c/84) com base em constantes, carrega
  quatro blocos LZ77 de tiles (`Screen_LoadLZ77TileBlock`), descomprime um bloco com `Asset_UnpackOrCopyBlock` e copia
  com `bios_cpuset`, grava a contagem em +0x36 de um contexto, descomprime uma tabela e limpa 0x80 words de paleta e VRAM
  (`bios_cpufastset`). Por fim configura valores de um contexto (+0x380, +0x39C, +0x3A0..) e incrementa um contador.
- Chama: `Screen_LoadLZ77TileBlock`, `Asset_UnpackOrCopyBlock`, `bios_cpuset`, `decompress_and_store`, `bios_cpufastset`.
  Chamada por: não verificado.
- Memória: VRAM 0x05000000; contexto DAT_0805c2c4; contador DAT_0805c2d8.
- Para o Lua: porta como carga inicial de uma cena (fundo, tiles e paleta).
- Confiança: provável (lógica lida; qual cena não identificada).

## 0x0805CB50 Dispatch_StateTableFrameCount
- Assinatura: `void Dispatch_StateTableFrameCount(void)`
- O que faz: chama o handler de tabela `DAT_0805cb80[*DAT_0805cb84]` via `CallViaR0`. Se o bit 0 do byte apontado por
  DAT_0805cb88 está ligado, incrementa dois contadores short (DAT_0805cb8c e +2).
- Chama: `CallViaR0` (handler de tabela). Chamada por: não verificado.
- Memória: índice de estado DAT_0805cb84; flag DAT_0805cb88; contadores DAT_0805cb8c (+0, +2).
- Para o Lua: porta como despacho de estado mais contador de frames.
- Confiança: provável (lógica lida; tabela não identificada).

## 0x0805D5F0 Screen_StepToBlank
- Assinatura: `void Screen_StepToBlank(void)`
- O que faz: passo de transição para tela em branco. Estado 0..7: incrementa o byte de estado (DAT_0805d60c) e grava
  estado*2 num halfword de posição (DAT_0805d610). Ao chegar em 8: limpa bits de REG_DISPCNT (duas máscaras, DAT_0805d648
  e DAT_0805d64c), limpa 0x80 slots de tile (`Tile_ClearSlots`), marca +6 do contexto, grava timer 10 e zera outro byte.
- Chama: `Tile_ClearSlots`. Chamada por: não verificado.
- Memória: REG_DISPCNT (0x04000000); contexto *DAT_0805d60c (+6); timers DAT_0805d650/d654.
- Para o Lua: porta como fade/encolhimento em etapas até tela apagada.
- Confiança: provável (lógica lida; efeito visual exato não verificado).

## 0x0805DBC4 Site_SaveLoadPromptStep
- Assinatura: `void Site_SaveLoadPromptStep(void)`
- O que faz: incrementa um contador short +2 do contexto (DAT_0805dbe8). Quando o contador (byte baixo) passa de 0x1D e
  `snd_sfx_query_playing(4)` diz que não há SFX tocando, chama `Site_RunSaveLoadOption(0)` até 4 vezes (para na primeira
  que devolve 0), toca a música 6 (`snd_song_request`), reseta campos (`Obj_ResetStateFields`) e avança o estado
  (+1 se devolveu 0; senão vai para 0xC).
- Chama: `snd_sfx_query_playing`, `Site_RunSaveLoadOption`, `snd_song_request`, `Obj_ResetStateFields`.
  Chamada por: não verificado.
- Memória: contexto DAT_0805dbe8 (+2); estado em DAT_0805dc28 / DAT_0805dc48.
- Para o Lua: porta como passo de tela de opção de salvar/carregar (o valor do resultado da opção não foi lido).
- Confiança: provável (fluxo lido; `Site_RunSaveLoadOption` não decompilado aqui).

## 0x0805F144 Screen_ResetClearVramFade
- Assinatura: `void Screen_ResetClearVramFade(void)`
- O que faz: entrada de cena de reset: toca a música 0 (`snd_song_request(0)`), zera 0x6000000 com `bios_cpufastset`
  (tamanho DAT_0805f190), limpa pares de halfwords (`Clear4PairsHalfwords`) e tiles (`Tile_ClearSlots`), define
  REG_DISPCNT=0x60, BLDCNT (DAT_0805f194) e BLDY=0x10 (fade), e incrementa o estado DAT_0805f198.
- Chama: `snd_song_request`, `bios_cpufastset`, `Clear4PairsHalfwords`, `Tile_ClearSlots`. Chamada por: não verificado.
- Memória: REG_DISPCNT, REG_BLDCNT, REG_BLDY (0x04000000 region); VRAM 0x06000000.
- Para o Lua: porta como reset de cena com fade.
- Confiança: provável (lógica lida; a cena não identificada).

## 0x0805FADC Sm_StepWithTimeout
- Assinatura: `void Sm_StepWithTimeout(void)`
- O que faz: chama `StateMachine_DispatchStep` com o ponteiro em DAT_0805fb08+0x14. Se devolveu != 0, incrementa um
  contador halfword (+2 de DAT_0805fb0c) e, quando passa de 0x3C, grava estado 8 em DAT_0805fb10 (timeout).
- Chama: `StateMachine_DispatchStep`. Chamada por: não verificado.
- Memória: contexto DAT_0805fb08 (+0x14); contador DAT_0805fb0c (+2); estado DAT_0805fb10.
- Para o Lua: porta como passo de máquina de estados com limite de 60 frames.
- Confiança: provável (lógica lida; `StateMachine_DispatchStep` não decompilado aqui).

## 0x080606A8 Screen_ResetClearTablesFade
- Assinatura: `void Screen_ResetClearTablesFade(void)`
- O que faz: entrada de cena de reset (padrão igual ao de 0x0805F144): zera 0x28 words (`bios_cpufastset`), zera outra
  área de 0x... (DAT_080606fc), limpa pares de halfwords e a tabela de halfwords (`Clear4PairsHalfwords`,
  `Screen_ClearHalfwordTable`), define REG_DISPCNT=0x60, BLDCNT e BLDY=0x10 (fade) e incrementa o estado DAT_08060704.
- Chama: `bios_cpufastset`, `Clear4PairsHalfwords`, `Screen_ClearHalfwordTable`. Chamada por: não verificado.
- Memória: REG_DISPCNT/BLDCNT/BLDY; DAT_080606f4 (área limpa); DAT_08060704 (estado).
- Para o Lua: porta como reset de cena com fade.
- Confiança: provável (lógica lida; destino da área limpa não identificado).

## 0x08061860 Menu_HighlightOptionStep
- Assinatura: `void Menu_HighlightOptionStep(void)`
- O que faz: a cada frame chama `Screen_HighlightOption4` com o byte +0x19 de um contexto e o valor de DAT_0806188c*2.
  Se o contador +0x48 do contexto (DAT_08061888) é 0, chama `StateMachine_DispatchSub`; quando devolve != 0 zera +0x84
  e incrementa o estado (DAT_080618b0). Se o contador é != 0, decrementa-o (espera).
- Chama: `Screen_HighlightOption4`, `StateMachine_DispatchSub`. Chamada por: não verificado.
- Memória: contexto DAT_08061888 (+0x19, +0x48); DAT_080618ac (+0x84); estado DAT_080618b0.
- Para o Lua: porta como destaque de opção de menu com espera e avanço de estado.
- Confiança: provável (lógica lida; o nome `Screen_HighlightOption4` vem do Ghidra).

## 0x08062FB4 BattlePanel_SceneInit
- Assinatura: `void BattlePanel_SceneInit(void)`
- O que faz: init de tela do painel de batalha: toca a música 0x18 se não está tocando (`snd_song_current`,
  `snd_song_request`), limpa VRAM 0x6000000 e tabelas (`Clear4PairsHalfwords`, `Tile_ClearSlots`), define DISPCNT=0x60,
  BLDCNT, BLDY, BG1CNT. Carrega tiles LZ77 (`Screen_LoadLZ77TileBlock`), descomprime blocos (`Asset_UnpackOrCopyBlock`,
  `decompress_and_store`), escreve texto (`Text_CopyFEStringTo20`, `Text_LayoutGlyphSpritesTileBase`), desenha painel
  (`BattlePanel_InitOrRedraw`, `BattlePanel_DrawSlotText`), rola colunas (`Anim_ScrollTileColumns`), cria timer
  (`Obj_InitTimerRecord`) e liga BG1..BG3 e OBJ no DISPCNT (bits 0x200..0x1000). Incrementa um contador de estado.
- Chama: `snd_song_current`, `snd_song_request`, `bios_cpufastset`, `Clear4PairsHalfwords`, `Tile_ClearSlots`,
  `Screen_LoadLZ77TileBlock`, `Asset_UnpackOrCopyBlock`, `bios_cpuset`, `decompress_and_store`, `Text_CopyFEStringTo20`,
  `Text_LayoutGlyphSpritesTileBase`, `BattlePanel_InitOrRedraw`, `BattlePanel_DrawSlotText`, `Anim_ScrollTileColumns`,
  `Obj_InitTimerRecord`. Chamada por: não verificado.
- Memória: REG_DISPCNT, REG_BLDCNT, REG_BLDY, REG_BG1CNT; VRAM 0x05000000 e 0x06000000; contexto *DAT_080631c4
  (+0x31, +0x33, +0x36, +0x3A, +0x50).
- Para o Lua: porta como montagem do painel de batalha (texto e barra) ao entrar na tela.
- Confiança: provável (callees BattlePanel_/Text_ dão o contexto; a cena exata não confirmada).

## 0x08063840 BattlePanel_StepUpdate
- Assinatura: `void BattlePanel_StepUpdate(void)`
- O que faz: a cada frame redesenha os textos de dois slots do painel (`BattlePanel_DrawSlotText` com o lado 0 e 1),
  roda `StateMachine_DispatchStep` numa tabela de handlers indexada pelo byte +0x33 (DAT_080638d4). Quando o passo
  termina e o bit 0 de DAT_080638d8 está ligado, grava um valor em +6 e toca SFX 0x0D. Quando um dos halfwords +0xA ou
  +0xE iguala DAT_080638e0, reinicia o timer (`Obj_InitTimerRecord`) e incrementa um contador.
- Chama: `BattlePanel_DrawSlotText`, `StateMachine_DispatchStep`, `snd_sfx_request`, `Obj_InitTimerRecord`.
  Chamada por: não verificado.
- Memória: contexto *DAT_080638d0 (+0x31..+0x34); tabela de handlers DAT_080638d4; flag DAT_080638d8; estado DAT_080638dc.
- Para o Lua: porta como atualização do painel de batalha com SFX ao concluir um passo.
- Confiança: provável (lógica lida; a tabela de handlers não identificada).

## 0x0806463C Dispatch_StateTableWithCopy
- Assinatura: `void Dispatch_StateTableWithCopy(void)`
- O que faz: copia 12 words de DAT_080646a4 para a pilha. Se o byte DAT_080646a8 == 2, limpa 0x200 bytes de uma área
  com `bios_cpufastset`. Depois chama `CallViaR0` com o item da cópia indexado por DAT_080646b8. Se o bit 0 de
  DAT_080646bc está ligado, incrementa dois contadores short em DAT_080646c0.
- Chama: `bios_cpufastset`, `CallViaR0`. Chamada por: não verificado.
- Memória: tabela DAT_080646a4 (12 words); DAT_080646a8, DAT_080646ac/b0, DAT_080646b8, DAT_080646bc, DAT_080646c0.
- Para o Lua: porta como despacho de estado com tabela local e limpeza condicional.
- Confiança: provável (lógica lida; os alvos da tabela não identificados).

## 0x080657C4 Sm_StepSfxRepeat3
- Assinatura: `void Sm_StepSfxRepeat3(void)`
- O que faz: incrementa um contador short +2 de um contexto (DAT_0806583c). Se o flag bit 1 de DAT_08065840 está
  desligado, chama `StateMachine_DispatchStep` com a entrada de uma tabela indexada por (byte +0x8 do contexto +
  DAT_08065848). Quando o passo termina e o bit 0 de DAT_0806584c está ligado: toca SFX 0x0D, soma 1 ao contador +8;
  se for < 3, `Obj_ResetStateFields` (repete); senão zera +0x84 e +0x1B, incrementa o estado DAT_08065854 e liga o bit 1.
  Se o bit 1 já está ligado e +8 < 6, despacha por jump table (não recuperada).
- Chama: `StateMachine_DispatchStep`, `snd_sfx_request`, `Obj_ResetStateFields`, e jump table não recuperada.
  Chamada por: não verificado.
- Memória: contexto DAT_0806583c; flags DAT_08065840 (bit 1) e DAT_0806584c (bit 0); estado DAT_08065854.
- Para o Lua: porta como sequência que repete um passo com SFX três vezes e depois avança.
- Confiança: provável (lógica lida; jump table não recuperada).

## 0x080665FC Fx_InitStringStateMachine
- Assinatura: `void Fx_InitStringStateMachine(void)`
- O que faz: prepara um contexto (DAT_08066630): grava 0xFB em +0x270, 1 em um byte de DAT_08066634, 0xC e 0 em outros
  dois bytes, copia uma sequência de bytes terminada em 0xFF (de DAT_08066640) para +0x274 e grava 0xFF e 4 no fim.
  Depois roda `StateMachine_DispatchStep` em +0x270. Quando termina: `Obj_ResetStateFields`, `Fx_InitRecord8(1,0x16,7)`
  e incrementa o estado DAT_08066688.
- Chama: `StateMachine_DispatchStep`, `Obj_ResetStateFields`, `Fx_InitRecord8`. Chamada por: não verificado.
- Memória: contexto *DAT_08066630 (+0x270, +0x274...); estado DAT_08066688.
- Para o Lua: porta como efeito que lê uma sequência de bytes e roda uma máquina de estados.
- Confiança: provável (lógica lida; significado da sequência não identificado).

## 0x080683C8 Anim_WaitRecordThenSetState
- Assinatura: `void Anim_WaitRecordThenSetState(void)`
- O que faz: chama `Anim_StepRecordCounter` no registro +0x50 de um contexto (ponteiro em PTR_DAT_080683f0). Quando
  devolve != 0, zera o estado principal (gMainState), grava estado 6 e o valor 0x50 em dois outros ponteiros.
- Chama: `Anim_StepRecordCounter`. Chamada por: não verificado.
- Memória: contexto em PTR_DAT_080683f0 (+0x50); gMainState; ponteiros PTR_DAT_080683f8/fc.
- Para o Lua: porta como espera de um contador de animação e troca de estado.
- Confiança: provável (lógica lida).

## 0x0806B764 Screen_InitFxSetup
- Assinatura: `void Screen_InitFxSetup(void)`
- O que faz: init de cena: monta três registros de cópia (fonte, destino 0x06000000, tamanho) no contexto DAT_0806b7f4,
  define REG_DISPCNT=0x60, toca a música 0 (`snd_song_request(0)`), chama `fx_screen_setup_a(3,2,0)`, zera quatro
  pares de halfwords e incrementa um contador de estado.
- Chama: `snd_song_request`, `fx_screen_setup_a`. Chamada por: não verificado.
- Memória: REG_DISPCNT; VRAM 0x06000000; contexto DAT_0806b7f4.
- Para o Lua: porta como preparação de efeitos de cena.
- Confiança: provável (lógica lida; cena não identificada).

## 0x0806DAA4 Screen_IntroTimeline
- Assinatura: `void Screen_IntroTimeline(void)`
- O que faz: linha do tempo de uma introdução de tela. No frame 0 toca a música 2, configura BLDCNT (0x244 / 0x1000),
  liga DISPCNT bit 0x400, habilita IRQ e DISPSTAT (bit 0x10), e preenche uma tabela de 0xA0 halfwords com uma curva
  (`aux_div_u32(0x10000, 0x100 - i*4)`, simétrica em torno de 0x7F). Nos frames seguintes: a partir de 0x41 limpa um
  bit de flags, chama `battle_slot_init(5,2)` e salta para um handler por jump table (não recuperada). Em 0x39 cria um
  objeto de renderização (`Map_InitializeObjectRenderSlot`) e chama `battle_slot_init(4,2)`. Incrementa o contador
  de frames +8.
- Chama: `snd_song_request`, `aux_div_u32`, `battle_slot_init`, `Map_InitializeObjectRenderSlot`, jump table não
  recuperada. Chamada por: não verificado.
- Memória: REG_BLDCNT, REG_IE, REG_DISPSTAT, REG_DISPCNT; tabela de curva em DAT_0806db20; contexto PTR_DAT_0806db08 (+8).
- Para o Lua: porta como timeline de intro (fade por curva em tempo fixo, depois objetos aparecem).
- Confiança: provável (lógica lida; jump table não recuperada).

## 0x0806EB28 Fx_StepFadeLatch
- Assinatura: `void Fx_StepFadeLatch(void)`
- O que faz: se o contador +4 de DAT_0806eb8c passou de 0x4C, lê o byte +0x77 e, se o bit 7 está desligado, grava um
  valor de brilho em DAT_0806eb90 e decrementa o byte. Chama `fx_step_table_a`. Quando devolve != 0, liga a máscara
  em REG_DISPCNT (DAT_0806eb94) e copia três halfwords do contexto para DAT_0806eb98/9c/a0. Incrementa o contador +4.
- Chama: `fx_step_table_a`. Chamada por: não verificado.
- Memória: contexto DAT_0806eb8c (+4, +0x77); REG_DISPCNT; DAT_0806eb90, DAT_0806eb98..a0.
- Para o Lua: porta como fade em rampa seguida de latch de parâmetros.
- Confiança: provável (lógica lida; os destinos não identificados).

Pendente: 5 funções não documentadas (0x08070914, 0x08071460, 0x08072080, 0x0807D48C, 0x0807DBCC), da 37ª à 41ª de FL5_lista.txt. Nomes, CSV e FL5.md já salvos até a 36ª. Obs.: a seção 0x0801275C ficou depois de 0x0803D2A8 (ordem de processamento, não de endereço).
