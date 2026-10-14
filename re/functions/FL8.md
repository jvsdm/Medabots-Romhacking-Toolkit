# FL8 — funções 0x08002730..0x0807E450 (lista FL8_lista.txt, 40 alvos)

Todas tinham nome `FUN_` no Ghidra no início. Os tamanhos da lista são "aprox. até próx. função" e
frequentemente erram (ex.: 0x08045A60 diz 7164 bytes, o corpo real é 0x29; 0x0807E450 diz 2.6 MB, o corpo
real é 0x45). Use o corpo real (`Body:` do get_function_by_address).

## 0x08002730 Fx_TickDispatchAndInitRecord
- Assinatura: `void Fx_TickDispatchAndInitRecord(void)`
- O que faz: chama `StateMachine_DispatchStep` sobre o ponteiro guardado em `DAT_08002754+4`. Quando devolve
  não-zero, inicia um registro de efeito (`Fx_InitRecord8(0,0x16,7)`) e incrementa um contador global (`DAT_08002758`).
- Chama: StateMachine_DispatchStep, Fx_InitRecord8 / Chamada por: não verificado.
- Memória: lê/escreve contador global (endereço no literal pool 0x08002758, não resolvido).
- Para o Lua: não precisa (ordem de efeitos da engine).
- Confiança: provável (lido no código; subsistema Fx pelo nome das chamadas).

## 0x08003018 Menu_TickSlideInPanelFade
- Assinatura: `void Menu_TickSlideInPanelFade(void)`
- O que faz: quadro de animação de um painel. Enquanto o estado em `+0x8E` não é 2, faz até 0x11 passos de
  fade de paleta (`Palette_FadeRowsToColorA/B`, linhas `0x20 - 2*passo`) e incrementa o passo em `+0x84`.
  Decrementa o timer `+0x88` e a cada quadro copia um retângulo do mapa de fundo deslocado (`BgMap_CopyRectWithOffset`)
  e preenche a linha de sombra (`BgMap_FillRect16`). Ao zerar o timer, reinicia o timer (`estado*2+1`), zera o passo
  e incrementa um contador global.
- Chama: FUN_08049a6c, Palette_FadeRowsToColorA, Palette_FadeRowsToColorB, BgMap_CopyRectWithOffset, BgMap_FillRect16.
- Memória: lê/escreve struct em `DAT_080030e4` (+0x84 passo, +0x88 timer, +0x8E estado); tabela de tiles por estado em `DAT_080030f0`.
- Para o Lua: não precisa se o painel for só visual; o efeito de fade e o scroll de mapa precisam ser reproduzidos.
- Confiança: provável (nome do subsistema pela forma; literais não resolvidos).

## 0x08008234 PartsGrid_InitScreen
- Assinatura: `void PartsGrid_InitScreen(void)`
- O que faz: setup da tela de grade de peças. Ajusta REG_DISPCNT (máscara, depois OR 0x1F60 = BGs/sprites),
  zera REG_BG e posições, descomprime 7 gráficos (`decompress_and_store`), carrega tiles (`LoadTilesWithMalias`
  x4) e paletas por `bios_cpufastset` (0x70 e 0x40 halfwords; 3 blocos de 8 words). Depois chama a família
  `PartsGrid_Draw*` (tile selecionado, gráfico, rótulo, ícone de célula, barra de status, região da peça,
  preview, valor de estatística) e `Sprite_InitPairC`. Fixa blend `SetGpuBlendMode(5,0x3F,0x20)`, zera
  `+0x84` e `+0x46`, e incrementa um contador global.
- Chama: decompress_and_store, LoadTilesWithMalias, bios_cpufastset, PartsGrid_UploadSelectedTile,
  PartsGrid_DrawSelectedGraphic, PartsGrid_DrawSelectedLabel, PartsGrid_DrawCellIconLayer, Sprite_InitPairC,
  PartsGrid_DrawSlotIconRow, PartsGrid_DrawCellStatusBar, PartsGrid_DrawPartRegion, PartsGrid_DrawPartPreview,
  PartsGrid_DrawCellPartStatValue, SetGpuBlendMode.
- Memória: IO REG_DISPCNT, REG_BG*; VRAM/paleta 0x05000000 (cpufastset); struct de tela em `DAT_08008428`.
- Para o Lua: sim, é a tela da grade de peças (o que a versão PC reconstrói); só a parte de tiles/paleta é hardware.
- Confiança: provável (lido no código; nomes `PartsGrid_*` vêm do mtbe.csv).

## 0x0800C2BC Menu_TickCountEditorAndGrant
- Assinatura: `void Menu_TickCountEditorAndGrant(void)`
- O que faz: quadro de um menu de seleção com contador. Lê o estado de botões (`DAT_0800C2E4`: bit 0x40 e 0x80
  andam o índice `+0x34` em 0..2 com volta). Redesenha os 3 textos de linha (`Mem_CopyBytes` + `Text_LayoutGlyphSprites`),
  ou limpa os slots se a flag 4 estiver setada. Atualiza duas linhas com setas (bit 0x10 e 0x20, letras 'w' = wrap)
  e formata 3 números de 6 dígitos (`Num_FormatDecimal6Glyphs`). Se o botão 1 está apertado, em modo 0 soma ou
  subtrai 1 de 4 itens do inventário (`Inv_AddCountClamp99` / `Inv_SubCountClampZero`, teto 0x78); em modo 2 zera
  tudo e soma 1 a todos os itens até 0x78 (efeito de cheat/debug: confirmar). Se o botão 2 está apertado, limpa
  a região e seta um flag.
- Chama: Mem_CopyBytes, Text_LayoutGlyphSprites, Tile_ClearSlots, Num_FormatDecimal6Glyphs, Inv_AddCountClamp99,
  Inv_SubCountClampZero.
- Memória: lê estado de input em `DAT_0800C2E4`/`DAT_0800C4FC`/`DAT_0800C5E8`; inventário via Inv_*; flag em `DAT_0800C5EC`.
- Para o Lua: sim se for tela real; se for o modo de cheat/debug, não precisa na versão final.
- Confiança: provável para o fluxo; chute para o significado do modo 2 (possível debug).

## 0x0800D27C Menu_TickSingleCountSelector
- Assinatura: `void Menu_TickSingleCountSelector(void)`
- O que faz: quadro de um seletor de contador com uma linha (versão irmã de 0x0800C2BC, sem wrap). Redesenha o
  texto da linha (`Mem_CopyBytes` + `Text_LayoutGlyphSprites`) ou limpa slots se a flag 4 está setada. Bits 0x10 e
  0x20 de input somam/subtraem 1 no índice `+0x34` do struct apontado por `PTR_DAT_0800D364`. Formata um número de
  6 dígitos. Se o botão 1 está apertado, grava um item/valor e o contador em `DAT_0800D3B4/D3B8` e marca um flag.
  Se o botão 2 está apertado, limpa a região e seta um flag.
- Chama: Mem_CopyBytes, Text_LayoutGlyphSprites, Tile_ClearSlots, Num_FormatDecimal6Glyphs.
- Memória: input em `PTR_DAT_0800D360`/`D368`; estado de seleção em `PTR_DAT_0800D364` (+0x34, +0x35..); flag de saída em `PTR_DAT_0800D36C`.
- Para o Lua: sim, se for a mesma tela de menu; o modo de confirmação depende do que o botão 1 grava (não fechado).
- Confiança: provável (fluxo claro, semântica do valor gravado não confirmada).

## 0x0801022C BgMap_FillRectTickCounter
- Assinatura: `void BgMap_FillRectTickCounter(void)`
- O que faz: preenche o retângulo (12,8) tamanho 18x5 do mapa de fundo com o tile em `*DAT_08010258` (`BgMap_FillRect16`)
  e incrementa um contador de byte em `DAT_0801025C+DAT_08010260`. Função de uma linha de efeito.
- Chama: BgMap_FillRect16.
- Memória: escreve no mapa de fundo (BG); lê/escreve o contador (endereços via literal pool, não resolvidos).
- Para o Lua: não precisa se a versão PC desenhar a moldura direto; senão, precisa do preenchimento.
- Confiança: provável (lido no código).

## 0x08011644 Fx_InitBlendTableAndTick
- Assinatura: `void Fx_InitBlendTableAndTick(void)`
- O que faz: escreve 9 words (uma tabela 3x3) num buffer apontado por `DAT_08011690` a partir de literais,
  chama `SetGpuBlendMode(5,0x3F,0x20)` (blend da GBA) e grava um valor em `DAT_080116B8`. Incrementa um contador
  em `DAT_080116C0`. Parece setup de um efeito de blend/fade.
- Chama: SetGpuBlendMode.
- Memória: escreve buffer de 36 bytes (endereço em literal pool); IO de blend (BLDCNT/BLDALPHA via SetGpuBlendMode).
- Para o Lua: não precisa (blend de hardware); a tabela só precisa ser sabida se o efeito for reproduzido.
- Confiança: provável para "setup de blend"; o significado de cada word é chute.

## 0x080129C8 Fight_SetupBattleBotStates
- Assinatura: `void Fight_SetupBattleBotStates(void)`
- O que faz: passo de preparação da batalha. Zera dois bytes de contexto (`+0x13A` e o byte em `DAT_08012A14+DAT_08012A18`).
  Se o flag em `+0x89` não é 1, roda a sequência de setup: `Fight_ClearBattleBuffer`,
  `Fight_UpdateBotActiveFlagCount`, `Fight_RefreshBotPartRecords`, `Fight_InitializeEnemyBotStates`. Sempre chama
  `Fight_RefreshBotPartSlotDisplays`; e, se `+0x89` não é 1, `Fight_ApplyPostBattleMode20PlayerSideState`.
  Incrementa um contador de estado (`DAT_08012A1C`).
- Chama: Fight_ClearBattleBuffer, Fight_UpdateBotActiveFlagCount, Fight_RefreshBotPartRecords,
  Fight_InitializeEnemyBotStates, Fight_RefreshBotPartSlotDisplays, Fight_ApplyPostBattleMode20PlayerSideState.
- Memória: contexto de batalha em `DAT_08012A14` (+0x89 flag, +0x13A zerado); contador em `DAT_08012A1C`.
- Para o Lua: sim (é a lógica de batalha; o nome do modo "post battle" é do Ghidra, confirmar).
- Confiança: provável (sequência de chamadas bem nomeada; estrutura exata do contexto não fechada).

## 0x08013568 Fight_TickEntryAnimationHandler
- Assinatura: `void Fight_TickEntryAnimationHandler(void)`
- O que faz: handler de animação de entrada da batalha (um quadro por chamada). Desenha overlay dos bots ativos
  (`Fight_DrawActiveBotsOverlay`), anima frames de entrada (`Fight_AnimateEntryFrames`), movimento de sprite composto
  (`Sprite_UpdateComposite4Motion`), soma 2 no estado `+0x84`, prepara objetos do estado B e chama `FUN_080173B4(1)`.
  Usa o timer halfword `+0x86` (contado até 0x10). Em 0x10 limpa slots, preenche BG0/BG1 com o tile de cada
  registrador de controle, copia um bloco com CpuFastSet e grava 0x0D no próximo estado.
- Chama: Fight_DrawActiveBotsOverlay, Fight_AnimateEntryFrames, Sprite_UpdateComposite4Motion,
  Fight_InitKindObjectsForStateB, FUN_080173B4, Tile_ClearSlots, BgMap_FillRect16, bios_cpufastset.
- Memória: contexto de animação via `PTR_DAT_080135F8/PTR_DAT_080135FC` (+0x80, +0x84, +0x86); estado seguinte em `PTR_DAT_08013610`.
- Para o Lua: sim (animação de entrada da batalha); a parte de BG/CpuFastSet é só hardware.
- Confiança: provável (timer +0x86 bate com a nota do HANDOFF_DISCOVERY).

## 0x08014CF0 Fight_SyncBotStatusFromPart
- Assinatura: `void Fight_SyncBotStatusFromPart(void)`
- O que faz: sincroniza um byte de status por bot (offset `0xDD` do bloco do bot `idx*0x100`). Se o índice do
  bot em `+0x87` é menor que 3, grava um halfword no registrador de controle de display (`+6` de `DAT_08014D30`)
  com o valor do byte + 0x8000 e marca um flag na primeira passagem. Caso contrário (índice >= 3), espera o bit 0x8000
  do slot de outra estrutura e copia seu byte baixo para o byte de status do bot. Incrementa um contador global.
- Chama: nenhuma função (só leitura/escrita direta).
- Memória: bloco do bot `0x03000590 + bot*0x100` (byte 0xDD = o que o Ghidra mostra; a pesquisa usa 0x9D); estrutura de
  OAM/tela em `DAT_08014D30`; contador em `PTR_DAT_08014D80`.
- Para o Lua: não precisa se a tela só mostra o status; a leitura do byte 0xDD precisa (vai entrar no modelo).
- Confiança: provável para o fluxo; o significado do bit 0x8000 e do registro de display é chute.

## 0x0801C388 Fight_TickEntryAnimAndLoadTiles
- Assinatura: `void Fight_TickEntryAnimAndLoadTiles(void)`
- O que faz: um quadro da animação de entrada de um lado da batalha. Usa o byte `+0x87` (índice do bot) para
  `Fight_AnimateEntryFrames` e `Fight_LoadBotSideTiles`, depois soma 1 ao contador `+0x9C` do contexto.
- Chama: Fight_AnimateEntryFrames, Fight_LoadBotSideTiles.
- Memória: contexto de luta em `DAT_0801C3AC` (+0x87 índice, +0x9C contador).
- Para o Lua: sim (sequência de animação da batalha).
- Confiança: provável (três chamadas diretas, sem lógica extra).

## 0x08034A68 Fight_CopyBotPartWordAndAdvance
- Assinatura: `void Fight_CopyBotPartWordAndAdvance(void)`
- O que faz: carrega em `ctx+0xEC` (halfword) a palavra de status da parte selecionada: endereço =
  `base + bot*0x100 + parte*0x18 + 0x59`, com `bot` em `ctx+0x88` e `parte` em `ctx+0x8B`. Depois chama
  `Fight_AdvanceActionScriptCursor`, que avança o cursor do script de ação.
- Chama: Fight_AdvanceActionScriptCursor.
- Memória: lê o bloco do bot (`0x03000590 + bot*0x100`, offset 0x59 + 0x18*i, conforme HANDOFF_DISCOVERY);
  escreve `ctx+0xEC` (slot table do contexto de animação).
- Para o Lua: sim (lê estado de parte do bot que o script de ação usa).
- Confiança: confirmado para o endereço da parte (bate com "Bot part flag at bot+0x59+0x18*i"); o papel do cursor é provável.

## 0x08038700 Fight_PlaceBotMarkerAndRunSlotAnim
- Assinatura: `void Fight_PlaceBotMarkerAndRunSlotAnim(void)`
- O que faz: posiciona o marcador de um bot da batalha. Incrementa o contador do slot (`+0x28` de cada entrada de
  0x40 bytes, indexado pelo byte em `ctx+0x88`). Lê a posição X do bot (`+0x13E` de cada entrada da tabela de bots
  + 4, ou + 2 quando o flag 2 do slot está setado), grava em `+4` do slot, chama `Fight_PlaceBotMarkerSlotsAroundBase`
  e `Anim_RunSlotScript(ctx+0x1AC, idx)`. Se o campo `+0x22` do slot é -1, repete a posição, limpa slots e sai.
- Chama: Fight_PlaceBotMarkerSlotsAroundBase, Anim_RunSlotScript, Tile_ClearSlots.
- Memória: tabela de slots de 0x40 bytes (`DAT_08038750`); tabela de bots (`DAT_080387F4`/`DAT_0803875C`, `+0x60` base lado, `+0x13E` posição).
- Para o Lua: sim (posicionamento de marcador/sprite da batalha).
- Confiança: provável (estrutura de slot ainda parcial).

## 0x0803C8AC Fight_TickSlotAnimAndCopyRect
- Assinatura: `void Fight_TickSlotAnimAndCopyRect(void)`
- O que faz: um quadro de animação de slot. Se o timer `+0x30` (halfword) do slot está em 0, chama
  `Fight_StepAnimScriptSlot` com a tabela de scripts `+0x2B` e incrementa o contador `+0x28`. Se o bit 1 de `+0x28`
  e `+0x27 == 0`, copia um retângulo com `BgMap_CopyRectWrapped` (sinal em X conforme o flag 2 de `DAT_0803C968+0xF8`,
  lado escolhido por tabelas `DAT_0803C96C`/`DAT_0803C9CC`). Se o timer é > 0, só decrementa.
- Chama: Fight_StepAnimScriptSlot, BgMap_CopyRectWrapped.
- Memória: slots de 0x40 bytes em `DAT_0803C8CC`; tabela de scripts em `DAT_0803C964`; flags em `DAT_0803C968`.
- Para o Lua: sim (animação de slot de batalha); a cópia de mapa é hardware.
- Confiança: provável (fluxo claro; significado do flag 2 chute).

## 0x0803DA90 Fight_ClearBattleBgAndBlend
- Assinatura: `void Fight_ClearBattleBgAndBlend(void)`
- O que faz: limpa o estado visual da batalha. Zera o blend (`SetGpuBlendMode(0,0,0)`), preenche os dois mapas de fundo
  de 32x32 com o tile apontado por `DAT_0803DAE0`/`DAE4` (`BgMap_FillRect16`), limpa 0x80 slots de tile e incrementa
  um contador global.
- Chama: SetGpuBlendMode, BgMap_FillRect16, Tile_ClearSlots.
- Memória: IO de blend e BG; contador em `DAT_0803DAE8`.
- Para o Lua: não precisa (limpeza de hardware), a não ser que a tela PC precise do fundo limpo.
- Confiança: provável (lido no código).

## 0x08040934 Fight_CallHandlerByIndex
- Assinatura: `void Fight_CallHandlerByIndex(void)`
- O que faz: trampolim. Lê um byte em `DAT_08040950+0x1B`, usa como índice numa tabela de ponteiros em
  `DAT_0804094C` e chama o endereço com `CallViaR0`. Não tem lógica própria; o efeito é do handler chamado.
- Chama: CallViaR0 (handler indexado, não resolvido aqui).
- Memória: tabela de ponteiros em `DAT_0804094C` (literal pool), índice em `DAT_08040950+0x1B`.
- Para o Lua: depende do handler; o trampolim em si não precisa.
- Confiança: confirmado para o mecanismo (lido no código); o alvo depende da tabela (não lida).

## 0x08045A60 Oam_QueuePreviewEntryWrapper
- Assinatura: `ulonglong Oam_QueuePreviewEntryWrapper(u32 a, u32 b, u16 c, int d, u8 e)`
- O que faz: wrapper de 42 bytes. Chama `QueueCompositePreviewTemplateEntryOam` com o tamanho fixo 0x80 e devolve
  só o r0 (os bits altos são lixo do retorno). Não tem lógica.
- Chama: QueueCompositePreviewTemplateEntryOam.
- Memória: nenhuma direta (a fila de OAM fica dentro da função chamada).
- Para o Lua: não precisa (é só plumbing); o que importa é a função de fila.
- Confiança: confirmado (lido no código). Corpo real 0x29 bytes; a lista dizia 7164 (tamanho errado).

## 0x08047E08 Inv_SubCountClampZeroTail
- Assinatura: `void Inv_SubCountClampZeroTail(u8 a, u8 b, u8 c)`
- O que faz: wrapper de 0x15 bytes que repassa três argumentos a `Inv_SubCountClampZero` (subtrai com piso zero no
  inventário). Não tem lógica própria.
- Chama: Inv_SubCountClampZero.
- Memória: via Inv_SubCountClampZero (inventário em IWRAM 0x03002BA0, par [0x80|id, qty], conforme HANDOFF_DISCOVERY).
- Para o Lua: sim (o efeito é do inventário), mas a função em si não precisa ser portada.
- Confiança: confirmado (lido no código); o nome "Tail" é só para diferenciar do wrapper.

## 0x08057440 Map_TickChoiceListOpen
- Assinatura: `void Map_TickChoiceListOpen(void)`
- O que faz: abertura animada de uma lista de escolha no mapa. Zera `+0x66`. Enquanto o contador `+0x6F` < 0x10,
  incrementa e desenha uma linha (`FUN_080492A8` com posição `0x1E - contador`) e chama `Map_CreateChoiceSprites(0)`.
  Ao chegar a 0x10: inicializa o slot de render 0x7F (`Map_InitializeObjectRenderSlot`), atualiza o sprite de cursor
  (`Map_UpdateCursorSpritePosition`), zera `+0x65`, chama `FUN_08047C00` e avança o subrotina com
  `GameState_AdvanceSubroutine`.
- Chama: FUN_080492A8, Map_CreateChoiceSprites, Map_InitializeObjectRenderSlot, Map_UpdateCursorSpritePosition,
  FUN_08047C00, GameState_AdvanceSubroutine.
- Memória: estado da lista em `DAT_0805748C` (+0x65, +0x66, +0x6F); dados da linha em `DAT_08057490`/`DAT_08057494`.
- Para o Lua: sim (lista de escolha do mapa; o efeito de abrir linha por linha é visível).
- Confiança: provável (fluxo claro; FUN_080492A8 e FUN_08047C00 ainda sem nome).

## 0x0805C454 Menu_TickCursorInputAndConfirm
- Assinatura: `void Menu_TickCursorInputAndConfirm(void)`
- O que faz: um quadro de menu com cursor no byte `+5`, entre o piso `+4` e o valor 2 (com volta).
  Chama `Screen_DrawCellAdvance` e `Palette_FadeBlockUpload`. Botão 0x40 sobe o cursor com volta e toca SFX 2
  (`snd_sfx_request(2)`). Botão 0x80 desce o cursor com volta e toca SFX 2. Botão 1 toca SFX 4 e, se o cursor está
  na posição 2, confirma: zera o estado, grava 0x14 em `+2`, faz `Palette_FadeTo16`, `Screen_SetRowPalette3`,
  `Screen_HighlightBitmaskGrid` e avança o estado. Senão, grava 0x2D em `+0x48` e vai para o estado 6.
- Chama: Screen_DrawCellAdvance, Palette_FadeBlockUpload, snd_sfx_request, Palette_FadeTo16,
  Screen_SetRowPalette3, Screen_HighlightBitmaskGrid.
- Memória: struct de menu em `DAT_0805C494` (+3A) e `DAT_0805C4CC`; entrada de input em `DAT_0805C49C`/`DAT_0805C4C8`;
  contador de frames via `gFrameCount`; estado em `PTR_DAT_0805C54C` / `DAT_0805C570`.
- Para o Lua: sim (menu com confirmação; sons SFX 2 e 4 precisam ser mapeados).
- Confiança: provável (estrutura de input clara; significado do estado 6 e do 0x2D chute).

## 0x0805CDAC Shop_InitItemListScreen
- Assinatura: `void Shop_InitItemListScreen(void)`
- O que faz: inicialização da tela de lista de itens (loja). Zera os campos de página/cursor (`+6`, `+0xC`, `+0x11`, `+0x12`),
  monta a lista (`Shop_BuildItemList64`) e desenha a página (`Shop_DrawItemListPage`). Se `+0x10` (número de itens)
  não é zero, posiciona o slot da tela (`Map_SetSlotScreenPosition`). Esconde as setas de paginação quando há uma
  página só (cálculo `aux_div_s32(n-1, 5)`, 5 itens por página). Limpa tiles e paleta (`bios_cpufastset` x5) e liga
  as camadas de DISPCNT 0x200/0x400/0x800/0x1000. Incrementa um contador global.
- Chama: Shop_BuildItemList64, Shop_DrawItemListPage, Map_SetSlotScreenPosition, aux_div_s32, bios_cpufastset.
- Memória: IO REG_DISPCNT; estado da loja em `*DAT_0805CE9C`; OAM/slots em `DAT_0805CEA0` (+0x1440, +0x14C0, +0x1480).
- Para o Lua: sim (tela da loja).
- Confiança: provável (lido no código; "5 por página" inferido da divisão).

## 0x0805D750 Screen_ClearBufferAndResetDispcnt
- Assinatura: `u32 Screen_ClearBufferAndResetDispcnt(void)`
- O que faz: zera um buffer de 4 bytes via `bios_cpufastset` (tamanho em `DAT_0805D788`), limpa 4 pares de halfwords
  (`Clear4PairsHalfwords`), grava `REG_DISPCNT = 0x160` (modo e camadas padrão) e incrementa um contador global.
- Chama: bios_cpufastset, Clear4PairsHalfwords.
- Memória: IO REG_DISPCNT; contador em `DAT_0805D78C`.
- Para o Lua: não precisa (reset de hardware); só o estado de limpeza conta.
- Confiança: provável (lido no código).

## 0x0805DD50 Menu_TickDispatchWithTimeout
- Assinatura: `void Menu_TickDispatchWithTimeout(void)`
- O que faz: um quadro de máquina de estados com timeout. Incrementa o halfword `+2` de uma struct apontada por
  `*DAT_0805DD7C` (contador de frames). Chama `StateMachine_DispatchStep` com o ponteiro em `DAT_0805DD80+0x30`. Se
  o passo devolveu não-zero e o contador (byte baixo) passou de 0x3C, grava o estado 9 em `*DAT_0805DD84`.
- Chama: StateMachine_DispatchStep.
- Memória: contador em `*DAT_0805DD7C+2`; estado em `*DAT_0805DD84`; máquina em `DAT_0805DD80+0x30`.
- Para o Lua: depende do que a máquina faz (não identificada aqui); o timeout de 60 frames precisa ser respeitado.
- Confiança: provável (contador e timeout lidos; o estado 9 e o dono da máquina não identificados).

## 0x0805F414 Screen_TickCountdownHalfStep
- Assinatura: `void Screen_TickCountdownHalfStep(void)`
- O que faz: contagem regressiva de um byte. Se o byte em `*DAT_0805F430` é 0, zera a struct associada (campos
  `+2`, `+3`, `+0`, `+6`, `+0x48..0x4B`), seta o bit 0x4000 em duas entradas de OAM/tela (`DAT_0805F480` e `+0x40`
  em bytes) e incrementa um contador (`DAT_0805F484`). Senão, decrementa o byte e grava metade dele (`>>1`) no
  halfword de posição apontado por `DAT_0805F434`.
- Chama: nenhuma.
- Memória: struct em `*DAT_0805F430`; entradas de OAM em `DAT_0805F480`; contador em `DAT_0805F484`.
- Para o Lua: provável sim (animação de fechar/abrir de uma caixa); confirmar a entrada.
- Confiança: provável (fluxo claro; significado do bit 0x4000 chute).

## 0x0805FD10 Screen_InitMenuLayoutAndAssets
- Assinatura: `void Screen_InitMenuLayoutAndAssets(void)`
- O que faz: setup de uma tela de menu. Define três posições/scrolls a partir de constantes, carrega três blocos de
  tiles LZ77 (`Screen_LoadLZ77TileBlock` x3), descompacta um bloco de asset (`Asset_UnpackOrCopyBlock`, `bios_cpuset`,
  `decompress_and_store`), limpa a VRAM/paleta (`bios_cpufastset` 0x80 e 0x10 words), e preenche três registros de
  objeto de 0x40 bytes (posições X/Y 0x78, 0x108 etc.). Zera o timer `+2` e põe 0x3C em `+0x15`.
- Chama: Screen_LoadLZ77TileBlock, Asset_UnpackOrCopyBlock, bios_cpuset, decompress_and_store, bios_cpufastset.
- Memória: struct da tela em `*DAT_0805FE50` (+0x36, +2, +0x15, +0x16); registros de objeto em `DAT_0805FE68`; contador em `DAT_0805FE80`.
- Para o Lua: sim (layout do menu); é o que a versão PC desenha.
- Confiança: provável (lido no código; ordem e significado dos valores das constantes não resolvidos).

## 0x080611E8 Menu_TickOptionFadeTransition
- Assinatura: `void Menu_TickOptionFadeTransition(void)`
- O que faz: transição de fade entre opções de menu. Se o byte `+6` do estado é 1, destaca a opção (`Screen_HighlightOption4`),
  faz até 9 passos de fade de paleta (`Palette_FadeRowsToColorA/B`, passo `0x20 - 4*k`) e depois inicializa o slot
  de objeto (`Obj_InitSlotAndLoad(1,0,0)`) e grava o estado 9. Se é outro valor, faz o mesmo fade e seta o bit 0x4000
  no registro de objeto, depois avança o estado.
- Chama: Screen_HighlightOption4, Palette_FadeRowsToColorA, Palette_FadeRowsToColorB, Obj_InitSlotAndLoad.
- Memória: estado em `*DAT_08061230` (+2, +6, +0x19); contador de passo em `DAT_08061234+0x84`.
- Para o Lua: sim (transição de menu visível).
- Confiança: provável (fluxo claro; valor 9 e 6 como estados não nomeados).

## 0x08061F00 Shop_TickBuyQuantityMenu
- Assinatura: `void Shop_TickBuyQuantityMenu(void)`
- O que faz: um quadro da tela de quantidade de compra/venda da loja (função grande, 0x57C bytes). Atualiza o cursor
  piscante (`Shop_BlinkCursorAttr`) e a posição do slot (`Slot_SetPosStepByMode`). Botão 8 cancela; se há item
  selecionado, toca SFX 4 e vai ao estado 9. Os botões 0x40/0x80 trocam o item da lista (SFX 2) e redesenham a página
  (`Shop_DrawPartListPage`). Botões 0x10/0x20 sobem/descem a quantidade, respeitando o limite de 99 e o estoque em
  `gShopQuantities`/inventário (`DAT_02040004` etc.). Desenha quantidades e o total do preço (`Screen_DrawDecimalNumberShadowed`
  e `Num_FormatDecimalToGlyphs`). Botão 1 confirma (SFX 4, estado 6); botão 2 cancela (SFX 5). Sem item, SFX 6.
- Chama: Slot_SetPosStepByMode, Shop_BlinkCursorAttr, snd_sfx_request, Shop_DrawPartListPage,
  Screen_DrawDecimalNumberShadowed, Num_FormatDecimalToGlyphs, Text_LayoutGlyphSprites, Slot_ClearAttrMask, Fx_InitRecord8, Obj_InitSlotAndLoad.
- Memória: estado da loja em `*DAT_08061FEC`; input em `DAT_080620B8`; quantidades em `gShopQuantities` (`PTR_08061FF0`);
  preço/total em `*DAT_080623E8+0x1C`; leitura de IO/IWRAM em `0x02040004`... (endereços de preço, não resolvidos).
- Para o Lua: sim (compra/venda de itens é fluxo de jogo); a leitura de "_DAT_0204..." é do EWRAM e precisa ser entendida antes do port.
- Confiança: provável para o fluxo; os endereços `0x02040xxx` ainda não nomeados (chute sobre a tabela de preços).

## 0x08063354 Shop_TickItemTransferAnim
- Assinatura: `void Shop_TickItemTransferAnim(void)`
- O que faz: animação de transferência de um item entre dois slots. Chama o estado do contexto `DAT_080633E4+0x4C`
  (`StateMachine_DispatchStep`), faz scroll das colunas de tile (`Anim_ScrollTileColumns`) e redesenha os dois textos
  de slot (`BattlePanel_DrawSlotText` x2). Decrementa o contador `+0x3C` (halfword). Quando passa de zero, zera o estado,
  incrementa um contador, faz `Inv_SubCountClampZero` no item de origem e `Inv_AddCountClamp99` no destino (1 unidade),
  limpa 0x100 halfwords (`bios_cpufastset`) e toca SFX 0x4B.
- Chama: StateMachine_DispatchStep, Anim_ScrollTileColumns, BattlePanel_DrawSlotText, Inv_SubCountClampZero,
  Inv_AddCountClamp99, bios_cpufastset, snd_sfx_request.
- Memória: contexto em `*DAT_080633E8` (+0x31, +0x32, +0x33, +0x34 = ids e slots; +0x3C timer); estado em `DAT_080633EC+0x84`.
- Para o Lua: sim (efeito visível de troca de item); confirmar se é compra ou troca de peça.
- Confiança: provável (Inv_* lido no código; semântica exata da troca não confirmada).

## 0x08063F08 Menu_CompareSelectionAndAdvance
- Assinatura: `void Menu_CompareSelectionAndAdvance(void)`
- O que faz: quadro de um menu de seleção. Se o byte `+0x7A` do contexto está setado, roda `StateMachine_DispatchStep`
  sobre `DAT_08063F4C+0x60`. Escolhe um índice (0 ou 4) pelo byte em `DAT_08063F50` e compara o halfword resultante
  com uma constante: se igual, grava o estado 0x1A; se igual a outra constante e o campo `+0xE` == `+0xA`, incrementa
  um contador, zera `+0x48` e chama `Obj_InitSlotAndLoad(1,0,1)` quando `+0x7A` está setado. Senão, regrava `+6`.
- Chama: StateMachine_DispatchStep, Obj_InitSlotAndLoad.
- Memória: contexto em `*DAT_08063F48` (+0x48, +0x7A); seleção em `DAT_08063F54` (+6, +0xA, +0xE).
- Para o Lua: provável sim (lógica de seleção de menu), mas as constantes ainda não estão resolvidas.
- Confiança: chute no significado de cada constante; estrutura geral provável.

## 0x0806495C Fx_TickBlinkFrameAndSetOam
- Assinatura: `void Fx_TickBlinkFrameAndSetOam(void)`
- O que faz: avança um frame de efeito de piscar. Chama `Anim_StepRecordCounter` sobre `+0x50` do contexto; se ele
  devolve não-zero, zera um byte global. Depois calcula dois atributos de objeto a partir do frame `+0x3A`:
  o atributo 1 vem de `frame*0x10 + 0x1B8` com máscara, e o atributo 0 recebe o bit 0x100 quando o bit 0x20 do frame
  está setado. Por fim, incrementa o frame (halfword `+0x3A`).
- Chama: Anim_StepRecordCounter.
- Memória: contexto em `*DAT_080649AC` (+0x3A frame, +0x50 contador); registro de OAM em `DAT_080649B4` (halfwords 0 e 1).
- Para o Lua: sim se o efeito for visível (piscar); o cálculo de OAM é direto.
- Confiança: provável (lido no código; semântica do bit 0x20 como "lado/flip" chute).

## 0x08065D10 Fight_CallHandlerWithArgCopy
- Assinatura: `void Fight_CallHandlerWithArgCopy(void)`
- O que faz: copia um bloco de 32 bytes de `DAT_08065D3C` para a pilha e chama, via `CallViaR0`, a entrada de uma
  tabela de ponteiros indexada pelo byte em `DAT_08065D40`. O handler recebe o ponteiro para essa cópia.
- Chama: CallViaR0 (handler indexado, não resolvido aqui).
- Memória: origem em `DAT_08065D3C` (32 bytes), índice em `DAT_08065D40`.
- Para o Lua: depende do handler (mesmo padrão de Fight_CallHandlerByIndex).
- Confiança: confirmado para o mecanismo; alvo chute até ler a tabela.

## 0x0806BC48 Fight_TickBattleFrame
- Assinatura: `void Fight_TickBattleFrame(void)`
- O que faz: despachante por quadro da batalha. Se o byte em `DAT_0806BC64+DAT_0806BC68` é zero, roda
  `counter_oscillate_x`, `battle_state_fire` e `select_entity_max`; senão, `battle_phase_spawn_move`. Depois, em
  sequência fixa: `fsm_obj_states_1_3`, `battle_update_objects`, `battle_timer_release_slot`,
  `battle_spawn_two_effects`, `battle_objects_frame_tile`, `battle_frame_tile_target`, `battle_frame_tile_state_c`,
  `battle_frame_tile_state_a`. Termina num salto indireto por tabela (não recuperada pelo Ghidra).
- Chama: counter_oscillate_x, battle_state_fire, select_entity_max, battle_phase_spawn_move, fsm_obj_states_1_3,
  battle_update_objects, battle_timer_release_slot, battle_spawn_two_effects, battle_objects_frame_tile,
  battle_frame_tile_target, battle_frame_tile_state_c, battle_frame_tile_state_a, e um jump table não recuperado.
- Memória: flag em `DAT_0806BC64+DAT_0806BC68` (não resolvido).
- Para o Lua: sim (é o laço principal de frame da batalha); a ordem das chamadas é o que importa.
- Confiança: confirmado para a sequência (lido no código); nomes das subfunções vêm do Ghidra (ainda sem doc).
  Pendente: o jump table final e os nomes `battle_*` (provavelmente de outro FL*).

## 0x0806E3C4 Fx_TickStepTableEntry
- Assinatura: `void Fx_TickStepTableEntry(void)`
- O que faz: roda `StateMachine_DispatchStep` sobre a entrada de uma tabela de ponteiros indexada pelo halfword `+0xC`
  do contexto. Quando o passo termina (retorno não-zero), grava 5 em `+4` e 2 num estado global (`DAT_0806E3F4`).
- Chama: StateMachine_DispatchStep.
- Memória: contexto em `DAT_0806E3F0`; tabela em `DAT_0806E3EC`; estado global em `DAT_0806E3F4`.
- Para o Lua: depende da máquina de estados da entrada (não identificada).
- Confiança: provável (fluxo claro; quem é a máquina não resolvido).

## 0x08066824 Fight_CallHandlerCopyArgs52
- Assinatura: `void Fight_CallHandlerCopyArgs52(void)`
- O que faz: mesmo padrão de Fight_CallHandlerWithArgCopy, mas copia 0x34 bytes (13 words) de `DAT_08066860`
  para a pilha e chama, com `CallViaR0`, a entrada da tabela indexada pelo byte em `DAT_08066864`. Depois, se o bit 1
  de `DAT_08066868` está setado, incrementa dois halfwords de contador (`DAT_0806686C` e +2).
- Chama: lib_memcpy, CallViaR0 (handler indexado, não resolvido).
- Memória: origem `DAT_08066860` (52 bytes); índice `DAT_08066864`; flag `DAT_08066868`; contadores `DAT_0806686C`.
- Para o Lua: depende do handler; o mecanismo não precisa ser portado como tal.
- Confiança: confirmado para o mecanismo (lido no código).

## 0x0807190C Fight_TickSlotScriptStep
- Assinatura: `u64 Fight_TickSlotScriptStep(u32 slot)`
- O que faz: um passo do script de um slot de batalha (slot = byte do parâmetro, bloco de 0x40 bytes a partir de
  `DAT_08071950`). Copia 8 bytes de uma tabela (`DAT_0807194C`) para a pilha. Se o índice `+0x21` é menor que 8 e o
  halfword `+0x1A` bate com a entrada da tabela, incrementa `+2` e o índice. Se não é, só incrementa `+2`. Se `+0x21 == 4`
  e `+0x22 != -1`, chama `slot_start_v2(1, +0x22, 0)`. Depois, se o halfword `+2` (no segundo bloco) é 0xC9, ajusta bits
  0xC00; se passou de 0xCF, devolve 0xFF; senão devolve 1.
- Chama: lib_memcpy, slot_start_v2.
- Memória: slots de 0x40 bytes em `DAT_08071950` e `DAT_08071994`; tabela de 8 bytes em `DAT_0807194C`.
- Para o Lua: sim (script de slot de batalha).
- Confiança: provável (passo claro; códigos 0xC9/0xCF/0xFF sem significado fechado).

## 0x0807E450 Math_SoftFloatCompare
- Assinatura: `u32 Math_SoftFloatCompare(u32 a, u32 b)`
- O que faz: compara dois floats de 32 bits em software. Desempacota os dois com `sf32_unpack`. Se qualquer um tem
  classe < 2 (zero ou especial), devolve 1; senão devolve `sf_cmp_core(a, b)`.
- Chama: sf32_unpack, sf_cmp_core.
- Memória: nenhuma (só pilha).
- Para o Lua: não precisa (Lua tem números nativos).
- Confiança: confirmado (lido no código). Corpo real 0x45 bytes; a lista dizia 2.6 MB (lixo, função pequena).

---
Pendente: 0x08068A84, 0x0806ED08, 0x08070D74, 0x080722B0, 0x0807D79C (ainda FUN_ no Ghidra, sem seção neste arquivo).
Parada por contexto. Retomar com get_function_by_address + decompile + rename, depois anexar aqui e em FL8_simbolos.csv.
