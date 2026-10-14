# FL6 — funções da lista FL6_lista.txt (ROM Metabee EU)

Tamanhos da lista são aproximados (vão até a próxima função); o corpo real vem do `get_function_by_address`.

## 0x080026A4 Portrait_TickMoveAndFlagDone
- Corpo: 0x080026A4-0x080026D5 (lista diz 60 B).
- O que faz: chama `Move_Character_Portraits(0,1,8,4, x, y)` com x/y lidos de um contexto (DAT_080026D8 +0x88 e +0x85); quando o retorno é 1 (movimento terminou), grava 1 em `*DAT_080026DC` (flag de "pronto").
- Subsistema: retrato de diálogo (movimento do retrato). Confiança: provável.
- Renomeado: `Portrait_TickMoveAndFlagDone`.

## 0x08002EC4 Scene_TickSubstate_PlaySong7 (NÃO renomeado)
- Corpo: 0x08002EC4-0x08002F5D (lista diz 172 B).
- O que faz: máquina de 4 subestados pelo byte `ctx+0x8E`. Estado 0 pede a música 7 (`snd_song_request(7)`), zera `ctx+0x8F` e ajusta dois ponteiros; estado 1 só ajusta ponteiros; estado 2 chama `FUN_0804765c(1)` e sai; outros: zera `gPlayTimeRunning`, põe `gMainState=2` e `DAT_08002F68=5`. Nos estados 0 e 1 limpa tiles 0x28..0x57 (`Tile_ClearSlots`).
- Subsistema: cena (transição de tela com música). Confiança: chute no nome; leitura do fluxo é confirmada.
- Não renomeado: o nome proposto exige saber qual cena é (falta contexto de `gMainState`).

## 0x08007A04 PartsGrid_InitScreen
- Corpo: 0x08007A04-0x08007B4F (lista diz 804 B).
- O que faz: inicialização da tela de peças: ajusta `REG_DISPCNT` (máscara e depois OR 0x1F60 = BG + OBJ), escreve registradores de BG, descomprime 4 blobs (`decompress_and_store`), carrega 4 conjuntos de tiles (`LoadTilesWithMalias`, 0x200 cada), limpa VRAM/paleta com `bios_cpufastset`, chama os `PartsGrid_Draw*` / `PartsGrid_UploadSelectedTile`, `Sprite_InitPairC`, `SetGpuBlendMode(5,0x3F,0x20)`, zera `ctx+0x84` e incrementa um contador em `DAT_08007BD4`.
- Subsistema: menu de peças (grade). Confiança: provável.
- Renomeado: `PartsGrid_InitScreen`.

## 0x0800BE6C Dispatch_CallHandlerByByte33
- Corpo: 0x0800BE6C-0x0800BE83 (lista diz 1072 B: a lista junta o trecho até a próxima função; a função é só 0x17 B).
- O que faz: `CallViaR0(tabela[ctx+0x33 * 4])`, tabela em `DAT_0800BE84`. É um despachante de 1 linha: escolhe o handler pelo byte de estado em `ctx+0x33`.
- Subsistema: despacho genérico de estado (contexto não identificado). Confiança: confirmado no código; nome provável.
- Renomeado: `Dispatch_CallHandlerByByte33`.

## 0x0800D188 Hud_DrawLabelAndDecimal6
- Corpo: 0x0800D188-0x0800D23D (lista diz 212 B).
- O que faz: se o bit 4 de `*DAT_0800D1A4` está zerado, copia 8 bytes de texto (`Mem_CopyBytes`) e monta os glifos (`Text_LayoutGlyphSprites`, 8 caracteres); senão limpa os tiles 8..15. Alterna um byte (XOR 1) quando `*DAT_0800D248 & 0x30`. Limpa tiles 0x20..0x22, formata um número de 6 dígitos (`Num_FormatDecimal6Glyphs`) e monta os glifos na posição 0x20. Se o bit 2 de `*DAT_0800D254` está ligado, limpa tiles 0x21.. e marca `*DAT_0800D258=1`.
- Subsistema: HUD de texto + número (contador). Confiança: provável (nome do rótulo não identificado).
- Renomeado: `Hud_DrawLabelAndDecimal6`.

## 0x08010170 FUN_08010170 (NÃO renomeado)
- Corpo: 0x08010170-0x080101E1 (lista diz 120 B).
- O que faz: incrementa um contador de byte (base `DAT_08010198` + offset); se o byte de estado é 0x1E, liga o bit 0x4000 de `ctx+0x100`; senão, se o byte & 4 é 0, liga 0x4000, e se é 4 aplica uma máscara (`DAT_080101A0`) ao halfword. Uso do bit 0x4000 no halfword sugere atributo de sprite/OBJ, não confirmado.
- Subsistema: animação ou sprite (chute). Confiança: chute.
- Não renomeado.

## 0x08011490 FUN_08011490 (NÃO renomeado)
- Corpo: 0x08011490-0x080114DD (lista diz 112 B).
- O que faz: lê o byte de índice `ctx+0x186`: se é 3, zera e grava 0x5B em `*DAT_080114B0`; senão, se a entrada da tabela (índice*8 + base) é 0, avança o índice; senão grava 0x4E em `*DAT_080114D0`. 0x4E é o opcode de dinheiro do evento (ver HANDOFF: compara com arg*10), então este ramo pode ser "dinheiro insuficiente"; 0x5B não identificado.
- Subsistema: loja/seleção de item (provável por causa de 0x4E). Confiança: chute.
- Não renomeado: falta confirmar o que `ctx+0x186` e a tabela representam.

## 0x0801277C Battle_SumPartBytesToCtx16C
- Corpo: 0x0801277C-0x080127CB (lista diz 500 B).
- O que faz: percorre 3 entradas com passo 0x100 a partir de `DAT_080127D0+DAT_080127D4`; onde o byte `+0x38` é não zero, soma 4 bytes (`+0x00`, `+0x18` de dois ponteiros) e soma o total ao halfword `ctx+0x16C` (`DAT_080127CC`).
- Subsistema: batalha, blocos de bot com passo 0x100 (padrão de `0x03000590 + bot*0x100`, offsets do Ghidra). Confiança: provável no subsistema; significado do total não confirmado.
- Renomeado: `Battle_SumPartBytesToCtx16C`.

## 0x08013330 Fight_DrawEntryScene
- Corpo: 0x08013330-... (lista diz 264 B).
- O que faz: desenha os bots ativos (`Fight_DrawActiveBotsOverlay`), anima os quadros de entrada (`Fight_AnimateEntryFrames`, byte `ctx+0x80`), atualiza um movimento composto de 4 sprites (`Sprite_UpdateComposite4Motion`), limpa VRAM (`bios_cpufastset`) e tiles 0..5 e 0x1E..0x2A, carrega dois blocos gráficos (`Fight_LoadTwoGraphicBlocksB`), copia um retângulo do mapa de fundo (`TileMap_ReadRectToBuffer` 12,4..9,21), zera `ctx+0x84` e incrementa um contador.
- Subsistema: batalha, cena de entrada. Confiança: provável.
- Renomeado: `Fight_DrawEntryScene`.

## 0x08014B34 Fight_TickPartGaugePhases
- Corpo: 0x08014B34-... (lista diz 252 B).
- O que faz: duas fases por lado (bytes `ctx+0x51` e `ctx+0x53`, 0..3). Na fase 0x51 atualiza um halfword de estado a partir de um byte da struct do bot (passo 0x100, offset 0xDD) mais o bit 0 de um byte alternado, e avança a fase a cada 2 quadros (contador em +0x86). Na fase 0x53 copia um halfword marcado com bit 0x8000 para a struct do bot e avança. Quando as duas fases chegam a 3, incrementa um contador em `*DAT_08014C2C` e zera +0x86.
- Subsistema: batalha, animação das peças (gauge). Confiança: provável (offsets 0x100/0xDD dizem bloco de bot).
- Renomeado: `Fight_TickPartGaugePhases`.

## 0x0801C298 FUN_0801C298 (NÃO renomeado)
- Corpo: 0x0801C298-... (lista diz 200 B).
- O que faz: incrementa o byte `ctx+0x9D`; se `*(DAT_0801C2E8+0x89)==0` e o bit 0 de `*DAT_0801C2EC` está ligado, força o byte para '<' (0x3C = 60). Quando `ctx+0x9D == 60`, limpa o tile 3 e incrementa `ctx+0x9C`. Sugere temporizador de 60 quadros (1 s).
- Subsistema: temporizador de tela (chute). Confiança: provável no temporizador; subsistema chute.
- Não renomeado.

## 0x08034708 Fight_RollPartAndAdvanceScript
- Corpo: 0x08034708-... (lista diz 612 B).
- O que faz: chama em sequência `Fight_RollPartValue(ctx+0x87)`, `Fight_ComputeEffectValueFromRos(ctx+0x87)` e `Fight_AdvanceActionScriptCursor()`. Passo de um opcode de script de ação: sorteia o valor da peça e avança o cursor.
- Subsistema: batalha, script de ação. Confiança: confirmado no código (chamadas); rótulo provável.
- Renomeado: `Fight_RollPartAndAdvanceScript`.

## 0x0803858C Object_StepSlotPosAndTarget
- Corpo: 0x0803858C-... (lista diz 196 B).
- O que faz: pega o slot de objeto (registro de 0x40 B, índice = byte em `DAT_080385C8`, base `DAT_080385C0`), soma 0x10 à halfword +0x18 com máscara, chama `Fight_StepSlotTowardTarget(slot)` e, se retorna não zero, limpa o tile do slot.
- Subsistema: objetos de batalha (slots de 0x40 B, como gObjectSlots). Confiança: provável.
- Renomeado: `Object_StepSlotPosAndTarget`.

## 0x0803C738 Anim_TickSlotScriptWithWait
- Corpo: 0x0803C738-... (lista diz 164 B).
- O que faz: para o slot (0x40 B) apontado pelo byte de índice: se o contador +0x16 é 0, liga o bit 0x8000 da halfword +0 e roda `Anim_RunSlotScript(tabela[+0x2D])` e `Fight_StepAnimScriptSlotB(tabela[+0x2E], 0)`; senão decrementa +0x16 (espera). Depois, se o contador +0x2B é 0 chama `Anim_AdvanceSlotTileOffset`; senão decrementa.
- Subsistema: animação de objeto por slot (mesma família de `Anim_RunSlotScript` 0x08040BD8). Confiança: provável.
- Renomeado: `Anim_TickSlotScriptWithWait`.

## 0x0803D3C0 Anim_TickSlotOffsetAndReload
- Corpo: 0x0803D3C0-... (lista diz 300 B).
- O que faz: roda `Anim_RunSlotScript(tabela[slot+0x26])`; se `Fight_StepSlotAnimOffsetTable(slot)` retorna não zero, recarrega a halfword +6 com `DAT_0803D420 + byte +0x28` e zera o byte +0x29.
- Subsistema: animação de objeto por slot. Confiança: provável.
- Renomeado: `Anim_TickSlotOffsetAndReload`.

## 0x080408F4 Dispatch_CallHandlerByByte1B
- Corpo: 0x080408F4-0x080408FF (lista diz 32 B).
- O que faz: `CallViaR0(tabela[ctx+0x1B * 4])`, tabela em `DAT_0804090C`. Mesmo padrão de `Dispatch_CallHandlerByByte33` (0x0800BE6C), com outro byte de índice.
- Subsistema: despachante genérico. Confiança: confirmado no código; nome provável.
- Renomeado: `Dispatch_CallHandlerByByte1B`.

## 0x08041B1C FUN_08041B1C (NÃO renomeado)
- Corpo: 0x08041B1C-... (lista diz 136 B).
- O que faz: mesmo padrão de `FUN_08010170`: pelo byte `ctx+0x84` (0x1E ou bits 4) liga o bit 0x4000 da halfword do slot de 0x40 B indicado por `ctx+0x2B` (ou aplica máscara), e incrementa `ctx+0x29` ou `ctx+0x84`.
- Subsistema: objetos/sprites de batalha ou mapa (chute). Confiança: chute.
- Não renomeado: o bit 0x4000 só é lido por quem chama; falta esse contexto.

## 0x08044F54 Gfx_QueueTileUploadOrCpuSet
- Corpo: 0x08044F54-... (lista diz 980 B).
- O que faz: se o primeiro parâmetro é não zero, desempacota ou copia o bloco (`Asset_UnpackOrCopyBlock`). Se o 4º parâmetro é 0, preenche o registro de DMA3 (fonte, destino = `(param_2 & 0xFFFF)*0x20 + base`, controle 0x94000000 | tamanho); senão faz `bios_cpuset` com flag de fill (0x04000000).
- Subsistema: gráficos (upload de tiles, 0x20 B por tile). Confiança: provável.
- Renomeado: `Gfx_QueueTileUploadOrCpuSet`. Obs.: aparece sem refs na lista (refs=0): pode ser chamado só por ponteiro.

## 0x0804FE9C Map_TickObjectMoveRecords
- Corpo: 0x0804FE9C-... (lista diz 544 B).
- O que faz: percorre `ctx+0x52` registros de 8 B (estado em +0x12 + 8*i). Estado 1: avança contador; ao passar de 5 procura o objeto pela chave (`Table64_FindOffsetByKey`), marca o marcador de evento (`Map_SetEventMarkerFacingVariant`) e, se há gatilho de warp (`Map_FindWarpTriggerAtTile`), chama `Overworld_StartMapTransitionEffect`. Estado 2: avança o movimento e, ao passar de 5, troca a animação do objeto (`Map_SetObjectAnimationVariant`). Quando um contador passa de 3, põe 0xFF no slot do objeto. Ao fim, zera `ctx+0x52`, limpa 0x40 B e chama `Event_EndOpcode`.
- Subsistema: mapa, objetos (movimento de NPC/evento). Confiança: provável.
- Renomeado: `Map_TickObjectMoveRecords`.
- Atenção: o `Overworld_StartMapTransitionEffect` chamado aqui é o nome de 0x08047BA8 do mtbe.csv, que o R2b já desmentiu (só toca som). Não confiar no nome.

## 0x0805C2DC Save_InitScreen
- Corpo: 0x0805C2DC-... (lista diz 308 B).
- O que faz: liga bits de DISPCNT (0x100, 0x200, 0x800, 0x1000) e limpa o bit 0 de `*DAT_0805C340`. Chama `Save_ClearAndRetryLoad`: se retorna 0, zera dois bytes de estado e marca o bit 0; se negativo, grava 9 em `*DAT_0805C344` e sai; se positivo, limpa duas linhas de tilemap e inicia o estado. Depois faz `Palette_FadeBlockUpload` e `Screen_HighlightBitmaskGrid(3)`, e incrementa `*DAT_0805C40C`.
- Subsistema: salvar (tela de save/carregamento). Confiança: provável.
- Renomeado: `Save_InitScreen`.

## 0x0805CBFC Screen_ResetLayersAndCounter
- Corpo: 0x0805CBFC-... (lista diz 64 B).
- O que faz: zera 4 B locais e limpa com `bios_cpufastset`, chama `Clear4PairsHalfwords`, põe `REG_DISPCNT = 0x160` e incrementa um contador em `*DAT_0805CC38`.
- Subsistema: reset de tela (chute quanto à cena). Confiança: confirmado no código; nome provável.
- Renomeado: `Screen_ResetLayersAndCounter`.

## 0x0805D674 Screen_TickFadeOutAndClear
- Corpo: 0x0805D674-... (lista diz 112 B).
- O que faz: pelo byte `ctx+0x84` (0..8), a cada quadro escurece as linhas da paleta (`Palette_FadeRowsToColorA` e `B`, passo `<<2`) e incrementa. Ao chegar a 9: ajusta `REG_DISPCNT` com máscara, limpa a VRAM em 0x06000000 e chama `FUN_080476E8(4)` (fim de transição).
- Subsistema: transição de tela (fade). Confiança: provável.
- Renomeado: `Screen_TickFadeOutAndClear`.

## 0x0805DC4C StateMachine_TickWithFrameTimer
- Corpo: 0x0805DC4C-... (lista diz 128 B).
- O que faz: incrementa um contador de 16 bits (`*DAT_0805DC7C+2`), chama `StateMachine_DispatchStep(ctx+0x2C)`, e se o retorno é não zero e o contador antigo (byte) passou de 0x3C (60 quadros), incrementa `*DAT_0805DC84`.
- Subsistema: máquina de estados com temporizador de 60 quadros. Confiança: confirmado no código; nome provável.
- Renomeado: `StateMachine_TickWithFrameTimer`.

## 0x0805F19C Screen_InitLayoutAndOam
- Corpo: 0x0805F19C-... (lista diz 444 B).
- O que faz: define os offsets de 3 camadas de fundo, carrega 3 blocos de tiles LZ77 (`Screen_LoadLZ77TileBlock`), desempacota um bloco de asset e o copia com `bios_cpuset`, descomprime outro, zera 0x200 B de paleta (`bios_cpufastset` em 0x05000000), limpa 8 palavras, e monta a entrada de OAM (atributos em 0x8000, 0x1000, posições 0x78/0x90, tamanhos) em duas listas de 0x20 halfwords. Marca `*DAT_0805F354+1`.
- Subsistema: tela (layout de fundo e sprites). Qual tela não identificada. Confiança: provável.
- Renomeado: `Screen_InitLayoutAndOam`.

## 0x0805FB14 StateMachine_TickTimeoutToState8
- Corpo: 0x0805FB14-... (lista diz 56 B).
- O que faz: chama `StateMachine_DispatchStep(ctx+0x18)`; se retorna não zero, incrementa um contador de 16 bits e, passando de 0x3C (60 quadros), grava 8 em `*DAT_0805FB48` (troca de estado).
- Subsistema: máquina de estados com timeout. Confiança: confirmado no código; nome provável.
- Renomeado: `StateMachine_TickTimeoutToState8`.

## 0x080609D4 Anim_CountdownAndSpawnObject
- Corpo: 0x080609D4-... (lista diz 72 B).
- O que faz: se o byte de contagem em `*DAT_080609F0` é 0, zera dois bytes e chama `Obj_InitSlotAndLoad(1,2,0)` (cria objeto) e incrementa um contador; senão decrementa o byte e grava `byte<<1` na halfword de `DAT_080609F4`. Depois atualiza a tabela de posição de sprite (`Anim_UpdateSpritePosTable`) e carrega o quadro a cada 10 (`Anim_LoadFrameEvery10`).
- Subsistema: animação de objeto (contagem regressiva, depois spawn). Confiança: provável.
- Renomeado: `Anim_CountdownAndSpawnObject`.

## 0x080618B4 Screen_TickFadeOutAndReset
- Corpo: 0x080618B4-... (lista diz 252 B).
- O que faz: mesmo fade de 9 passos de `Screen_TickFadeOutAndClear` (0x0805D674) com `Palette_FadeRowsToColorA/B`. Ao fim: aplica máscara em 4 halfwords de tilemap (passo 0x20), zera bytes de estado, limpa 0x78 B de uma área e grava 0xC e 0 em duas variáveis.
- Subsistema: transição de tela (fade). Confiança: provável.
- Renomeado: `Screen_TickFadeOutAndReset`.

## 0x08063208 Battle_TickPanelTimerAndDraw
- Corpo: 0x08063208-... (lista diz 76 B).
- O que faz: chama `Anim_StepRecordCounter(*DAT_0806324C + 0x50)`; quando retorna não zero, incrementa um contador, grava 600 (10 s a 60 quadros) em `ctx+0x3C` e chama `Obj_InitSlotAndLoad(1,0,1)`. Depois desenha o texto do painel (`BattlePanel_DrawSlotText(ctx+0x31, ctx+0x33, 0)`).
- Subsistema: batalha (painel com temporizador). Confiança: provável.
- Renomeado: `Battle_TickPanelTimerAndDraw`.

## 0x080638E8 Battle_TickPanelTextTwoSlots
- Corpo: 0x080638E8-... (lista diz 1492 B; o corpo real é bem menor, a lista junta o trecho até a próxima função).
- O que faz: `Anim_StepRecordCounter(ctx+0x50)`; se zero, desenha os textos dos dois slots do painel (`BattlePanel_DrawSlotText` com ctx+0x31/+0x33 e +0x32/+0x34). Se não zero, limpa tiles 0..0x80, grava 0x14 em `*DAT_08063914` e zera `*DAT_08063918`.
- Subsistema: batalha (painel). Confiança: provável. Irmã de `Battle_TickPanelTimerAndDraw`.
- Renomeado: `Battle_TickPanelTextTwoSlots`.

## 0x08064880 Anim_StepOscillatingSprite
- Corpo: 0x08064880-... (lista diz 96 B).
- O que faz: avança `Anim_StepRecordCounter(ctx+0x50)` (conta quando retorna não zero); calcula a halfword de posição `DAT_080648D8+1` = `(ctx+0x3A)*0x10 + 0x1B8` com máscara, liga o bit 0x100 em `DAT_080648D8` conforme o bit 0x20 de `ctx+0x3A`, e incrementa `ctx+0x3A`. Efeito de oscilação (vai e volta) do sprite.
- Subsistema: animação de sprite. Confiança: provável.
- Renomeado: `Anim_StepOscillatingSprite`.

## 0x08065A0C Battle_TickTwoStageSequence
- Corpo: 0x08065A0C-... (lista diz 136 B).
- O que faz: escolhe o estado (offset 0x18 ou 0x1C conforme `*(ctx+8) != 0`) e chama `StateMachine_DispatchStep`. Ao concluir (e com o bit 0 de `*DAT_08065A68`), toca o SFX 0xD (`snd_sfx_request(0xD)`) e incrementa `ctx+8`. Se ainda < 2, chama `Obj_ResetStateFields`; senão zera `ctx+0x84` e `ctx+0x1B` e decrementa `*DAT_08065A70`.
- Subsistema: batalha, sequência de 2 etapas com som. Confiança: provável.
- Renomeado: `Battle_TickTwoStageSequence`.

## 0x080666C8 Fx_TickStateThenInitRecord8
- Corpo: 0x080666C8-... (lista diz 76 B).
- O que faz: `StateMachine_DispatchStep(ctx+0x48)`; quando retorna não zero, chama `Fx_InitRecord8(1, 0x16, 7)` e incrementa um contador.
- Subsistema: efeitos (Fx). Confiança: confirmado no código; nome provável.
- Renomeado: `Fx_TickStateThenInitRecord8`.

## 0x08068400 Dispatch_CallHandlerLocalCopy
- Corpo: 0x08068400-0x0806843x (lista diz 96 B).
- O que faz: copia 7 palavras de uma tabela (`DAT_08068434`) para a pilha, chama `CallViaR0(local[*DAT_08068438])` e incrementa uma halfword em `DAT_0806843C+2`.
- Subsistema: despachante genérico. Confiança: confirmado no código; nome provável.
- Renomeado: `Dispatch_CallHandlerLocalCopy`.

## 0x0806B820 Screen_InitObjectSlotsLz77
- Corpo: 0x0806B820-... (lista diz 836 B).
- O que faz: descomprime 4 blocos LZ77 para VRAM (`bios_lz77_vram`), configura registradores de DMA/cópia, e posiciona ~13 slots de objeto com `Map_InitializeObjectRenderSlot` (OBJs com tamanhos e posições fixas em x/y como 0x78, 0x80, 0x98, 0xB8). Liga bits 0x1E00 de `REG_DISPCNT` (OBJ e BGs) e incrementa um contador.
- Subsistema: tela com sprites fixos (qual tela não identificada). Confiança: provável no tipo de init.
- Renomeado: `Screen_InitObjectSlotsLz77`.

## 0x0806DBFC FUN_0806DBFC (NÃO renomeado)
- Corpo: 0x0806DBFC-... (lista diz 1416 B).
- O que faz: lê `*DAT_0806DC2C & 8`. Se zero: espera contador (ctx+4) passar de `DAT_0806DC68`, então chama `slot_ptr_stub(2)` e `fx_screen_setup_b(4,1,0)`, põe estado 3 e grava 2 em `*DAT_0806DC6C`. Se 8: `battle_slot_init(6,2)`, toca SFX 3 e incrementa um contador.
- Subsistema: transição/tela de batalha ou menu (chute). Confiança: chute.
- Não renomeado: nome não confirmado.

## 0x0806EBA4 Hud_TickDecimal6AndStep
- Corpo: 0x0806EBA4-... (lista diz 140 B).
- O que faz: incrementa `ctx[0x78]` e soma 0x6F a `ctx[0x79]`; chama `format_6_digits`, `slots11_update` e `fx_step_table_b`. Quando o passo retorna não zero, aponta dois ponteiros (`DAT_0806EC10`) para o buffer local, zera `*DAT_0806EC1C`, faz `REG_DISPCNT &= máscara` e grava três halfwords com os valores de `ctx[0x78..0x7A]`.
- Subsistema: HUD numérico (6 dígitos, como `Hud_DrawLabelAndDecimal6`). Confiança: provável.
- Renomeado: `Hud_TickDecimal6AndStep`.

## 0x080709D4 Screen_InitLetterGridObjects
- Corpo: 0x080709D4-... (lista diz 568 B).
- O que faz: 9 chamadas `bios_lz77_vram` (tiles para VRAM), depois dois laços de `Map_InitializeObjectRenderSlot` que posicionam 22 objetos (x = 0x78 ± 8·n) com tile de letra (`'@'+n`, `n+1`), e mais 4 slots e 4 OBJs de fileira; no fim grava 0x38, 6 e 4 em variáveis de estado.
- Subsistema: tela com grade de letras/textos em objetos (não identificada). Confiança: provável.
- Renomeado: `Screen_InitLetterGridObjects`.

## 0x08071630 Dispatch_CallHandlerOrStartAnims
- Corpo: 0x08071630-... (lista diz 276 B).
- O que faz: se `*(DAT_08071648+8) < 8`, chama o handler de `tabela[índice]` (salto indireto, tabela em `DAT_0807164C`; "jumptable not recovered" no Ghidra). Senão zera o índice, chama `enable_anim4` (0x16, 0x19 e 0x17) e `slot_start_v2(3,0,0)` e `slot_start_v2(4,0xB,0)`, e incrementa um contador.
- Subsistema: sequência de animação com despachante. Confiança: provável.
- Renomeado: `Dispatch_CallHandlerOrStartAnims`. Pendente: recuperar a tabela de 8 handlers em `DAT_0807164C`.

## 0x08072188 FightPreview_TickIntroAndBuildLayer
- Corpo: 0x08072188-... (lista diz 172 B).
- O que faz: define `ctx+0x44` como 0xC8 ou 0x42 conforme o bit 4 de `ctx+0x5A`, e incrementa `ctx+0x5A`. Quando passa de 0x1F, chama `slot_ptr_stub(1)` e `FightPreview_InitCompositeBuffers`, prepara um ponteiro de 0x200 e chama `BuildCompositePreviewSpriteLayer(0, 0x2B, ..., 0x40, 0x42, 2, 8, ...)`, e incrementa um contador.
- Subsistema: prévia de luta (pré-visualização de composição de sprites). Confiança: provável.
- Renomeado: `FightPreview_TickIntroAndBuildLayer`.

## 0x0807D4BC SoftFloat_Add64
- Corpo: 0x0807D4BC-0x0807D4CF (lista diz 56 B).
- O que faz: soma de ponto flutuante de 64 bits em software: desempacota dois operandos (`sf_unpack`), chama `sf64_add_core` e reempacota (`sf64_pack`). Não usa ponteiros de jogo.
- Subsistema: biblioteca de ponto flutuante (libgcc-like). Confiança: confirmado no código.
- Renomeado: `SoftFloat_Add64`. Obs.: `local_3c ^= 1` no corpo é ruído do decompiler (sinal do resultado).
