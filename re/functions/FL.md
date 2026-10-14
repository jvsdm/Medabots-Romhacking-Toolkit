# FL: funções FUN_ que sobraram (Medabots Metabee EU)

Fonte: `FL_lista.txt` (940 FUN_ no Ghidra). Candidatas = sem nome no mtbe.csv e sem seção em nenhum .md (434).
Ordem: mais referências primeiro (BL + ponteiros em literal pool, contados na ROM). Reservadas para outro agente:
HC (`HC_lista.txt`) e FH (0x0803725C, 0x08037280, 0x080376F4, 0x080370AC, 0x08036AC4, 0x08036E54, 0x08036B84,
0x08036D24). Ghidra: nomes renomeados só quando confirmados ou prováveis.

## 0x08001454 Init_CopyStartupGraphicsToVram
- Assinatura: `void Init_CopyStartupGraphicsToVram(void)`
- O que faz: duas chamadas de CpuFastSet (BIOS) copiam dados da ROM (fonte em DAT_08001470/DAT_08001478) para a
  paleta em 0x05000000 e, em seguida, para outro bloco de VRAM/paleta. Rotina de inicialização.
- Chama: `bios_cpufastset` x2. Chamada por: provavelmente o init (não verificado).
- Memória: lê ROM (ponteiros em DAT_08001470..78); escreve 0x05000000 (paleta) e destino.
- Para o Lua: não precisa (hardware/BIOS).
- Confiança: confirmado (conteúdo); destino exato provável.

## 0x08007D28 Tile_ClearSlotsAndDispatchState
- Assinatura: `undefined4 Tile_ClearSlotsAndDispatchState(void)`
- O que faz: chama `Tile_ClearSlots(0, 0x80)` e depois despacha por tabela de ponteiros indexada pelo byte
  `*(DAT_08007D48 + 0x3c)`, só se o índice for < 5 (senão retorna). Tabela em DAT_08007D4C; jump table não recuperada.
- Chama: `Tile_ClearSlots`; despacho indireto (ponteiro de função). Chamada por: não verificado.
- Memória: lê estrutura apontada por DAT_08007D48 (byte +0x3c). Tabela de ponteiros em DAT_08007D4C.
- Para o Lua: a porta depende do estado; precisa do mapeamento dos 5 handlers.
- Confiança: provável (despacho confirmado; nome do estado é chute).

## 0x08040994 BgMap_TickRectFillState
- Assinatura: `undefined4 BgMap_TickRectFillState(void)`
- O que faz: se o byte +1 do bloco (DAT_080409D0) não é 2 e o byte +0x12 é 1, preenche um retângulo do mapa de
  fundo (via `FUN_0803f51c` ou `BgMap_FillRect16`, conforme o byte +1) com tile 3 ou 0x13 (conforme +0xF) e zera
  +0xE e +0x12. Por fim incrementa o contador +0x1B. Tick de animação/atualização de retângulo.
- Chama: `FUN_0803f51c`, `BgMap_FillRect16` (0x0803EF8C). Chamada por: não verificado (provavelmente o tick de tela).
- Memória: lê/escreve o bloco em DAT_080409D0 (+0x01, +0x0E, +0x0F, +0x10, +0x11, +0x12, +0x1B).
- Para o Lua: precisa do tile do retângulo; porta se a tela for portada.
- Confiança: provável.

## 0x0801554C Util_IsSlotIndexValid
- Assinatura: `uint Util_IsSlotIndexValid(uint idx, int mode)`
- O que faz: se `mode == 0` retorna 0 quando idx < 3; senão, se `mode != 0` retorna 0 quando idx > 2. Caso
  contrário retorna 1. Validação de índice (3 slots no modo 0; até 2 no modo !=0).
- Chama: nada. Chamada por: não verificado.
- Para o Lua: trivial, portar se for usado.
- Confiança: confirmado (lógica); significado de "slot" é provável.

## 0x08040A00 BgMap_TickFlashStep
- Assinatura: `undefined4 BgMap_TickFlashStep(void)`
- O que faz: se o byte +2 do bloco (DAT_08040A28) é 1, chama `BgMap_FillRect16(30, 6, 0, 13, ...)` e soma 2 ao
  contador +0x1B; senão soma 1. Passo de um efeito de piscar/contagem.
- Chama: `BgMap_FillRect16` (0x0803EF8C). Chamada por: não verificado.
- Memória: bloco em DAT_08040A28 (+0x02, +0x1B); DAT_08040A2C ponteiro de dado do tile.
- Para o Lua: não precisa se o efeito não for portado.
- Confiança: provável.

## 0x0806E184 Display_ApplyStateIfReady (nome provisório, não renomeado)
- Assinatura: `undefined4 FUN_0806e184(void)`
- O que faz: chama `FUN_080688e0`; se der não-zero, grava um ponteiro (local) e três halfwords em
  gMainState/DAT_0806E1xx, e escreve REG_DISPCNT com máscara (`DAT_0806E1D8 & REG_DISPCNT`). Parece aplicar o
  estado de vídeo/IO quando pronto.
- Chama: `FUN_080688e0`. Chamada por: não verificado.
- Memória: REG_DISPCNT (0x04000000); escreve em gMainState (ponteiros em 0x0806E1DC/E4/E8).
- Para o Lua: não precisa (hardware).
- Confiança: chute (não renomeado).

## 0x08002438 Pad_ProcessSampleQueue (chute, não renomeado)
- Assinatura: `undefined4 FUN_08002438(void)`
- O que faz: lê dois words de DAT_08002518 e um byte de DAT_08002520 para uma pequena estrutura (DAT_0800251C).
  Se a chave lida casa com DAT_08002524 e o contador +0x18 > 3, troca ponteiros de fila (+0x24/+0x28) e marca
  flags em DAT_0800252C/DAT_08002528. Depois avança contadores (+0x14, +0x18, até 5) e copia entradas de 16 bits
  para uma fila. Parece um amostrador de entrada de 4 ou 5 quadros com fila dupla.
- Chama: nada direto. Chamada por: não verificado.
- Para o Lua: o jogo lê entrada; a porta usaria o `love.keyboard`, não isto.
- Confiança: chute.

## 0x0801058C Fight_InitBotPartSelectScreen
- Assinatura: `undefined4 Fight_InitBotPartSelectScreen(void)`
- O que faz: inicializa a tela de seleção de peças do bot na batalha: limpa 4 pares de slots, ajusta REG_DISPCNT
  com máscara, descompacta 5 blocos gráficos (`decompress_and_store`), carrega tiles, escreve posições em
  estrutura de sprites, chama `GfxBankA_LoadTilesAndPalette`, liga camadas em DISPCNT |= 0x1F60 e desenha
  cabeçalho, rótulos, ícones e sprite de bandeira (`Fight_Draw*`, `Fight_SetupFlagSprite`).
- Chama: `Slots_ClearFourPairs`, `decompress_and_store` x5, `LoadTilesWithMalias`, `GfxBankA_LoadTilesAndPalette`,
  `Fight_DrawHeaderNameText`, `Fight_DrawStatusLabelText`, `Fight_DrawBotPartEntrySprites`,
  `Fight_DrawBotPartEntryIcons`, `Fight_DrawBotPartIconRow`, `Fight_SetupFlagSprite`, `Fight_FormatBotLabelText`,
  `FxBlend_ApplyFromObjA`. Chamada por: não verificado.
- Memória: REG_DISPCNT (0x04000000); estrutura de sprites em DAT_080106F4 (+0x65/+0x67/+0x69).
- Para o Lua: precisa (é a tela de peças), mas a porta é grande; usar as rotinas Fight_Draw* já documentadas.
- Confiança: provável (chamadas confirmadas; ordem de desenho é leitura).

## 0x08010A20 Tile_ResetBlendAndClearSlots
- Assinatura: `undefined4 Tile_ResetBlendAndClearSlots(void)`
- O que faz: `SetGpuBlendMode(0,0,0)`, preenche dois retângulos de 32x32 com `BgMap_FillRect16` (tiles dos
  ponteiros DAT_08010A70/74), `Tile_ClearSlots(0, 0x80)` e incrementa um contador em DAT_08010A78.
- Chama: `SetGpuBlendMode`, `FUN_0803ef8c` (BgMap_FillRect16), `Tile_ClearSlots`. Chamada por: não verificado.
- Para o Lua: não precisa (limpeza de hardware).
- Confiança: provável.

## 0x08013798 Init_CopyTileSetsAndSetBlend
- Assinatura: `undefined4 Init_CopyTileSetsAndSetBlend(void)`
- O que faz: cinco `bios_cpufastset` copiam blocos de dados (512 e 2048 words) para VRAM/paleta, depois
  `SetGpuBlendMode(5, 0x3F, 0x20)`, grava um ponteiro em DAT_08013818 e incrementa DAT_08013820.
- Chama: `bios_cpufastset` x5, `SetGpuBlendMode`. Chamada por: não verificado.
- Para o Lua: não precisa (hardware/BIOS).
- Confiança: provável (ordem e destino dos dados não verificados).

## 0x08014A60 StateMachine_TickStepCounter
- Assinatura: `undefined4 StateMachine_TickStepCounter(void)`
- O que faz: chama `StateMachine_DispatchStep` com o ponteiro em DAT_08014A7C+4; se o retorno for não-zero,
  incrementa o contador em DAT_08014A80. Wrapper de tick de máquina de estados.
- Chama: `StateMachine_DispatchStep`. Chamada por: não verificado.
- Para o Lua: depende do estado chamado; porta junto com a máquina de estados.
- Confiança: provável.

## 0x080322C8 Fight_ChooseActionTargetSlot
- Assinatura: `undefined4 Fight_ChooseActionTargetSlot(void)`
- O que faz: escolhe o alvo de uma ação de peça com fallbacks em cadeia. Tenta três filtros nos 3 bots do lado
  aliado (parte sem técnica de 16, Medaforce inativa, habilidade de parte ativa); se nada der candidato, relaxa
  o filtro, até cair em bot ativo aleatório do lado. Grava o slot (+1) em +0x5E da estrutura de ação e chama
  `Fight_SetPartActionMetadata`. Usa dois blocos (DAT_08032350, DAT_080323BC) e um ponteiro DAT_08032474.
- Chama: `Fight_GetAllySideBaseIndex`, `Fight_HasNoTechnique16Part`, `Fight_IsBotMedaforceInactive`,
  `Fight_HasActivePartWithSpeciality`, `Fight_HasAnyCandidateTarget`, `Fight_PickRandomCandidateTargetSlot`,
  `Fight_PickRandomActiveBotInSide`, `Fight_SetPartActionMetadata`. Chamada por: não verificado.
- Para o Lua: precisa (escolha de alvo), mas o RNG é o 0x080424B8 já documentado.
- Confiança: provável (ordem dos filtros lida no código; nomes das auxiliares vêm do mtbe).

## 0x08037BF4 Anim_TickSlotAndAdvanceFrame
- Assinatura: `undefined4 Anim_TickSlotAndAdvanceFrame(void)`
- O que faz: pega o índice de slot em DAT_08037C38+DAT_08037C3C, roda o script de animação desse slot
  (`Anim_RunSlotScript`, ponteiro em DAT_08037C30 indexado pelo byte +0x2E do slot de 0x40 bytes), e avança o
  quadro: +4 (short) até 0x14; ao chegar em 0x14 incrementa o byte +0x29 em vez disso.
- Chama: `Anim_RunSlotScript`. Chamada por: não verificado.
- Memória: slots de 0x40 bytes a partir de DAT_08037C34.
- Para o Lua: precisa se a animação correspondente for portada (ver `pc/research` sobre animações).
- Confiança: provável.

## 0x0803D74C Init_LoadScreenAssetsAndPlaySong8 (chute, não renomeado)
- Assinatura: `undefined4 FUN_0803d74c(void)`
- O que faz: mesma forma de `Fight_InitBotPartSelectScreen`: limpa slots, zera e copia VRAM/paleta com CpuFastSet,
  descompacta 4 blocos, carrega tiles, copia tabelas pequenas, liga camadas (DISPCNT |= 0x1F60), chama quatro
  rotinas de desenho `FUN_0803e7c0/7f0/83c/870`, `FxBlend_ApplyFromObjA` e `snd_song_request(8)`. Incrementa um
  contador em DAT_0803D924.
- Chama: `Slots_ClearFourPairs`, `decompress_and_store` x4, `LoadTilesWithMalias`, `GfxBankA_LoadTilesAndPalette`,
  `FxBlend_ApplyFromObjA`, `snd_song_request`. Chamada por: não verificado.
- Confiança: chute quanto ao nome (é uma tela; qual, não confirmado).

## 0x0803DC8C Screen_ResetAndSetSub4Mode
- Assinatura: `undefined4 Screen_ResetAndSetSub4Mode(void)`
- O que faz: `Tile_ClearSlots(0, 0x80)`, `Slots_ClearFourPairs`, zera duas áreas com CpuFastSet (uma delas é
  paleta 0x05000000), `SetGpuBlendMode(5, 0x3F, 0x20)` e `Dispatch_Sub4Mode(3)`. Reset de tela antes de trocar
  de sub-modo.
- Chama: `Tile_ClearSlots`, `Slots_ClearFourPairs`, `bios_cpufastset` x2, `SetGpuBlendMode`, `Dispatch_Sub4Mode`.
  Chamada por: não verificado.
- Para o Lua: não precisa (limpeza de hardware), exceto a troca de sub-modo.
- Confiança: provável.

## 0x08041A28 Menu_CursorStepHandler
- Assinatura: `undefined4 Menu_CursorStepHandler(void)`
- O que faz: lê o byte de entrada em DAT_08041B10. Com bits 0xC0: toca sfx 2, inverte o flag +0x2A e ajusta a
  posição Y do sprite. Em qualquer quadro atualiza a posição (`Slot_SetPosStepByMode`) e avança +0x84. Bit 0x02:
  sfx 5, marca +0x2A = 1, define posição e zera +0x84, incrementa +0x29. Bit 0x01: sfx 4, posição e incremento de
  +0x29. Um cursor de menu com dois botões de confirmação/cancelamento (mapeamento exato não verificado).
- Chama: `snd_sfx_request` (2, 4, 5), `Slot_SetPosStepByMode`. Chamada por: não verificado.
- Memória: byte de entrada em DAT_08041B10; estrutura em DAT_08041B14 (+0x29..+0x2C, +0x84); sprites em DAT_08041B18.
- Para o Lua: precisa se o menu for portado; os sfx mapeiam para `pc/game` de som.
- Confiança: provável.

## 0x0804177C Obj_ClearAndStampRowsA (chute, não renomeado)
- Assinatura: `undefined4 FUN_0804177c(void)`
- O que faz: escreve zeros em 2 linhas de 8 halfwords, copia 8 halfwords de DAT_08041810 e 8 de DAT_08041814 para
  o buffer de tiles/sprites (endereço = byte +0x2D*0x40 + byte +0x2C*2 + DAT_0804180C), zera mais 2 linhas e
  incrementa +0x29 da estrutura em DAT_08041808.
- Chama: nada. Chamada por: não verificado.
- Confiança: chute quanto ao papel (preenchimento de buffer de sprite).

## 0x08041818 Obj_ClearAndStampRowsB (chute, não renomeado)
- Assinatura: `undefined4 FUN_08041818(void)`
- O que faz: mesma família de `FUN_0804177c`, com outra base de buffer (DAT_080418B4) e fontes de dados
  (DAT_080418B8 para 2 linhas de 8 halfwords, DAT_080418BC para 2 linhas), limpa 8 halfwords no fim e incrementa
  +0x29 em DAT_080418B0.
- Chama: nada. Chamada por: não verificado.
- Confiança: chute.

## 0x08049CC8 Slot_PackTileAndPalFields (chute, não renomeado)
- Assinatura: `undefined4 FUN_08049cc8(void)`
- O que faz: lê campos de 6 bits de uma estrutura em DAT_08049CF4 (bytes +3, +4, +5) e grava um halfword
  (+3 | +4 << 8, ambos com máscara 0x3F) em DAT_08049CF0 e o campo +5 em DAT_08049CF8. Empacota atributos de
  tile/paleta de um slot.
- Chama: nada. Chamada por: não verificado.
- Confiança: chute quanto ao nome; leitura do código é confirmada.

## 0x0804F8E8 Event_StepWaitThenEndOpcode
- Assinatura: `undefined4 Event_StepWaitThenEndOpcode(void)`
- O que faz: máquina de dois passos para um opcode de evento. Estado 0: chama `FUN_080500bc(arg, ..., 1)` e avança
  para 1. Estado 1: espera `FUN_0805018c(arg, 1)` retornar não-zero e então chama `Event_EndOpcode` (0x08050290).
- Chama: `FUN_080500bc`, `FUN_0805018c`, `Event_EndOpcode`. Chamada por: não verificado (handler de opcode).
- Para o Lua: precisa se o opcode for portado; ver `Event_EndOpcode` no mtbe.
- Confiança: provável.

## 0x08060708 Screen_InitLz77TilesAndSprites
- Assinatura: `undefined4 Screen_InitLz77TilesAndSprites(void)`
- O que faz: escreve três posições de sprite, carrega três blocos LZ77 de tiles (`Screen_LoadLZ77TileBlock`),
  limpa uma linha de tilemap com `bios_cpuset`, descompacta blocos grandes (`Asset_UnpackOrCopyBlock`,
  `decompress_and_store`), copia 0x70 e 0x18 words para paleta e preenche ~15 campos de sprite (y/x a partir de
  tabelas). Incrementa contador em DAT_08060968. Tela de inicialização de um menu com sprites.
- Chama: `Screen_LoadLZ77TileBlock` x3, `bios_cpuset`, `bios_cpufastset` x2, `Asset_UnpackOrCopyBlock`,
  `decompress_and_store` x2. Chamada por: não verificado.
- Confiança: provável (o papel exato da tela não confirmado).

## 0x08060F78 Screen_InitNumberDisplay
- Assinatura: `undefined4 Screen_InitNumberDisplay(void)`
- O que faz: limpa 0x80 slots, zera a estrutura de sprites (DAT_08061108), configura 4 sprites de 0x8000 com
  posições fixas (x 0x1A/0x98/0x48..0x58, y 0x22..0x50), formata um número para glifos (`Num_FormatDecimalToGlyphs`)
  e posiciona os glifos (`Text_LayoutGlyphSprites`). Incrementa contador em DAT_08061138.
- Chama: `bios_cpufastset`, `Tile_ClearSlots`, `Num_FormatDecimalToGlyphs`, `Text_LayoutGlyphSprites`.
  Chamada por: não verificado.
- Para o Lua: precisa se o número for mostrado (ver `Num_FormatDecimalToGlyphs` no mtbe).
- Confiança: provável.

## 0x08061CE0 Shop_RefreshPartListScreen
- Assinatura: `undefined4 Shop_RefreshPartListScreen(void)`
- O que faz: liga a camada DISPCNT |= 0x1000, limpa 8 blocos com `bios_cpufastset`, desenha a página da lista de
  peças (`Shop_DrawPartListPage`) e, conforme o contador do item (+0x20 da estrutura), desenha até 4 quantidades
  com `Screen_DrawDecimalNumberShadowed`. Incrementa um contador no fim. O nome "gShopQuantities" na mesma rotina
  sugere loja (provável).
- Chama: `bios_cpufastset` x8, `Shop_DrawPartListPage`, `Screen_DrawDecimalNumberShadowed` x4.
  Chamada por: não verificado.
- Confiança: provável.

## 0x08065B0C Menu_TickSelectTimeoutState (chute, não renomeado)
- Assinatura: `undefined4 FUN_08065b0c(void)`
- O que faz: a cada quadro chama rotinas de entrada (`FUN_080020c8` quando um contador está em 0, `FUN_08002190`,
  `Screen_ReadTwoBitField`); compara o item selecionado com a tabela em +0x0A e, se bater, avança um estado ou
  chama `FUN_0803fb0c` (`Obj_ResetStateFields`). Um contador em +0x3A conta até 0x12D e zera a seleção em 0x10C.
  Parece um timeout/seleção de menu.
- Chama: `FUN_080020c8`, `FUN_08002190`, `Screen_ReadTwoBitField`, `FUN_0803fb0c`. Chamada por: não verificado.
- Confiança: chute (lógica lida; papel não confirmado).

## 0x08068924 Calc_DivisibleStepAndCompare (chute, não renomeado)
- Assinatura: `undefined8 FUN_08068924(void)`
- O que faz: se `*DAT_08068960` é divisível por `*DAT_08068964`, grava `0x10 - quociente` em `*DAT_08068968`
  (halfword). Retorna um flag `(*DAT_08068960 == *DAT_08068964 * 0x10)`. Usa os auxiliares de divisão/resto com
  sinal (`aux_div_s32`/`aux_mod_s32`, ver HANDOFF: 0x0807CD2C/0x0807CDF8).
- Chama: `aux_mod_s32`, `aux_div_s32`. Chamada por: não verificado.
- Confiança: chute quanto ao propósito; aritmética confirmada.

Pendente: documentadas até 0x08068924 (ordem de refs; 25 de 434 candidatas), faltam 409 candidatas.
