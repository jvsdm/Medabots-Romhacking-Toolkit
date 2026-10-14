# FN1: funções FUN_ restantes (< 0x08030000), lote 1

## 0x0800058C Scene_InitSoundAndStartSong0
- Assinatura: `void Scene_InitSoundAndStartSong0(void)`
- O que faz: chama `scene_start_block()`, carrega a tabela de músicas e quatro globais de som (`snd_song_table_set`, `snd_store_global_word`, `snd_store_global_728`, `snd_store_field_720`, `snd_store_field_var`) e pede a música 0 (`snd_song_request(0)`).
- Chama: scene_start_block, snd_song_table_set, snd_store_* , snd_song_request / Chamada por: não verificado.
- Memória: lê literais DAT_080005BC..C8 (dados de som); escreve nas globais de som via helpers.
- Para o Lua: não precisa (inicialização de som/cena do GBA).
- Confiança: provável (nome pelo conteúdo; a semântica dos snd_store_* vem de outras funções).

## 0x08000824 FUN_08000824 (sugestão: Callback_ThenSetState2)
- Assinatura: `undefined4 FUN_08000824(void)`
- O que faz: chama um ponteiro de função lido do literal `DAT_08000838` (via CallViaR0) e depois grava `2` no byte apontado por `DAT_0800083C`.
- Confiança: chute (o alvo do ponteiro e o significado do byte de estado não foram lidos).

## 0x08002190 FUN_08002190 (sugestão: Flag_SetPendingIfActive)
- Assinatura: `void FUN_08002190(void)`
- O que faz: se o ponteiro `DAT_080021A0` aponta para algo não-zero no primeiro byte, marca o byte +6 da estrutura com 1.
- Confiança: chute (nome e papel da estrutura não confirmados).

## 0x080021A4 FUN_080021A4 (sugestão: Init_ChannelStruct)
- Assinatura: `undefined4 FUN_080021A4(void)`
- O que faz: zera um halfword (`DAT_080021D8`), mascara um registro com `DAT_080021E0` e seta o halfword em 1; preenche um campo de estrutura (`DAT_080021E4`, +0xE, +0x6D = 0xC0) e zera o byte +6 de `DAT_080021F0`. Parece inicialização de canal (som ou timer); não confirmado.
- Confiança: chute.

## 0x08002328 FUN_08002328 (sugestão: Grid_CopyRowsIfSumIsMinusOne)
- Assinatura: `undefined8 FUN_08002328(int param_1)`
- O que faz: chama uma função via R0 (`DAT_080023C8`); se ela retorna não-zero, percorre 4 blocos de 0xC bytes, soma 4 halfwords de cada bloco e, quando a soma dá -1, copia o bloco com `bios_cpuset` e marca um bit em `ctx+3`. No fim junta o bit em `ctx+2`.
- Memória: ctx em `DAT_080023CC` (+2, +3), vetor de registros em `ctx+0x2C`.
- Confiança: chute.

## 0x08002438 FUN_08002438 (sugestão: Input_PollKeyHistory)
- Assinatura: `undefined4 FUN_08002438(void)`
- O que faz: lê o registro de teclas (`DAT_08002518`), grava um bit de estado a partir de `DAT_08002520`, compara com um valor fixo (`DAT_08002524`) e, se bater e o contador for >3, troca dois buffers de 4 entradas, zera 4 bytes e sinaliza com `DAT_08002528`/`DAT_0800252C`. Empilha o valor de entrada numa janela de 5 posições (contador em +0x14 e +0x18). Parece reconhecimento de sequência de botões; não confirmado.
- Para o Lua: provavelmente vira o detector de combo do PC (a confirmar).
- Confiança: chute.

## 0x08002EC4 FUN_08002EC4 (sugestão: Scene_OnSubstate8E_Dispatch)
- Assinatura: `undefined4 FUN_08002EC4(void)`
- O que faz: despacha pelo byte de estado `ctx+0x8E`. Em 1: grava 2/4/0 em três ponteiros e limpa entradas de atributo de objeto (0x28..0x57). Em 0: `snd_song_request(7)`, zera `ctx+0x8F`, grava 2 e 0, e limpa atributos. Em 2: chama `FUN_0804765C(1)` e sai. Demais: zera `gPlayTimeRunning`, grava 2 em `gMainState`, 5 e 0 em duas outras variáveis, e limpa atributos.
- Chama: snd_song_request, FUN_0804765C, ClearObjectAttributeEntries.
- Confiança: provável quanto ao papel de máquina de estados (valores de `gMainState` batem com a tabela de estado do jogo); chute quanto ao nome.

## 0x0800393C FUN_0800393C (sugestão: Scene_LoadSpriteAndSlots)
- Assinatura: `undefined4 FUN_0800393C(void)`
- O que faz: `decompress_and_store(DAT_08003994, DAT_08003998)` (descompacta dados), copia três words para `DAT_0800399C`, e monta um registro de sprite em `DAT_080039AC`: posição X = 0x88 - 8*n e Y = 0x20 + 0x18*n, onde n = `ctx+0x8F` de `DAT_080039B4`. Seta atributos (0xE, 0xFF) e chama `Slot_SetupFromRecordList(*DAT_080039B8, 0)`.
- Chama: decompress_and_store (0x080009BC, já documentado), Slot_SetupFromRecordList.
- Confiança: chute quanto ao nome; provável quanto ao sprite de posição por índice.

## 0x0800994C FUN_0800994C (sugestão: Sprite_SetOrClearFlag4000_ByFlag8)
- Assinatura: `undefined4 FUN_0800994C(void)`
- O que faz: lê o byte de flags em `DAT_08009974`; se bit 3 = 0, liga o bit 0x4000 no halfword +0x40 de um objeto e no +0x80 do mesmo (ponteiros `DAT_080099A0`); senão, aplica máscara `DAT_0800997C` nos mesmos campos. Parece mostrar/ocultar sprite (bit 0x4000 de atributo OAM); não confirmado.
- Confiança: chute.

## 0x0800B79C FUN_0800B79C (sugestão: Bot_SetFlag4000_OnState1E)
- Assinatura: `undefined4 FUN_0800B79C(void)`
- O que faz: lê um byte de estado em `DAT_0800B7C4 + DAT_0800B7C8`. Se for 0x1E, liga 0x4000 no halfword +0x1C0 e incrementa um contador em `DAT_0800B810 + DAT_0800B7C4`. Senão, liga 0x4000 (bit 2 = 0) ou aplica máscara `DAT_0800B7CC` no mesmo campo, e incrementa o contador.
- Observação: é a MESMA lógica de `FUN_08010170` com offsets diferentes (+0x1C0 aqui, +0x100 lá). Provavelmente cópia para outro bloco (bot/parte). Confirmar antes de fundir.
- Confiança: chute.

## 0x0800D664 FUN_0800D664 (sugestão: Dispatch_ByByte33)
- Assinatura: `undefined4 FUN_0800D664(void)`
- O que faz: lê o byte +0x33 de `DAT_0800D680`, usa como índice (x4) numa tabela de ponteiros em `DAT_0800D67C` e chama o ponteiro via CallViaR0. É um despachante simples.
- Confiança: provável (dispatcher); o alvo depende da tabela (não lida).

## 0x08010170 FUN_08010170 (sugestão: Bot_SetFlag4000_OnState1E_B)
- Assinatura: `undefined4 FUN_08010170(void)`
- O que faz: igual a `FUN_0800B79C` mas no bloco `DAT_08010198` (+0x100); usa `DAT_080101E4`/`DAT_080101BC` para o contador.
- Confiança: chute. Duplicata de `FUN_0800B79C`.

## 0x08010B24 FUN_08010B24 (sugestão: Struct_Init20_State2)
- Assinatura: `void FUN_08010B24(void)`
- O que faz: zera 0x14 bytes de `DAT_08010B48` (`Mem_FillBytes`), depois grava 2 no byte 0, 3 em `DAT_08010B4C` e 0 em `DAT_08010B50`.
- Confiança: provável quanto ao "zerar e setar estado" (valores 2/3 não identificados).

## 0x08011490 FUN_08011490 (sugestão: Counter_Step3_Else_Msg4E)
- Assinatura: `void FUN_08011490(void)`
- O que faz: contador em `DAT_080114AC + 0x186`. Se for 3, zera e grava 0x5B em `DAT_080114B0`. Senão, se a entrada `cnt*8` de uma tabela (`DAT_080114CC`) for 0, incrementa o contador; se não for 0, grava 0x4E em `DAT_080114D0`.
- Observação: 0x4E também é o opcode de dinheiro de evento (README/Key facts). Só coincidência até confirmar o endereço de `DAT_080114D0`.
- Confiança: chute.

## 0x0801275C FUN_0801275C (sugestão: Accum_Add5xByte)
- Assinatura: `void FUN_0801275C(void)`
- O que faz: `short [ctx+0x16C] += byte[ctx+0x16B] * 5` (ctx em `DAT_08012778`).
- Confiança: chute quanto ao papel.

## 0x080140B0 FUN_080140B0 (sugestão: Anim_StateMachine_Ctx84)
- Assinatura: `undefined4 FUN_080140B0(void)`
- O que faz: máquina de estados no byte `ctx+0x84`. Estado 0: prepara sprites/posições (`DAT_08014124+0xC0..0xE0`) e avança. Estado 1: espera o bit 0 de `DAT_080141B4`, decrementa `ctx+0xE0` (timer) escrevendo em `DAT_080141BC` (x0x1100), e quando zera configura posições (0x78,0x48) e avança. Estado 2: soma 8 num byte até 0x60. Estado 3: subtrai 8 até 0x40 e volta ao estado 4 (conta de frames em +0x86). Ao chegar em +0x86 == 0x3C: `ClearObjectAttributeEntries(3,2)`, `bios_cpufastset` (DAT_08014250, 8 words) e incrementa `DAT_08014258`.
- Chama: ClearObjectAttributeEntries, bios_cpufastset (wrapper 0x0807CCC0, CpuFastSet).
- Confiança: provável (máquina de estados de animação/transição); nome não confirmado.

## 0x0801630C FUN_0801630C (sugestão: Slot_MarkActive_SetPosByFlag)
- Assinatura: `undefined4 FUN_0801630C(void)`
- O que faz: se `ctx+0x83 == 0`, usa o índice em `ctx+0x81` (stride 0x40 a partir de `DAT_08016358`): seta bit 0x20 no halfword do slot, incrementa o byte +0x22, e grava 0x40 ou 0x50 em +8 e +9 conforme o bit 3 de +0x22.
- Confiança: chute.

## 0x08016370 FUN_08016370 (sugestão: Obj_SpawnGroup_FromTable4)
- Assinatura: `undefined4 FUN_08016370(uint param_1)`
- O que faz: lê 4 bytes da tabela `DAT_08016424` no índice `param_1*4` (contagem, byte 1, byte 2, byte 3). Limpa 10 entradas de atributo (`ClearObjectAttributeEntries(0x3C,10)`). Para cada um dos N = contagem: inicializa um slot de 0x20 (em `DAT_08016428+0xF00`) e um registro de 0x40 (em `+DAT_08016430`), com posição, contador e índice (`& 3`). Se `param_1 > 2`, liga 0x1000 no atributo.
- Confiança: chute.

## 0x080165D0 FUN_080165D0 (sugestão: Slots6_TickFadeAndClear)
- Assinatura: `undefined4 FUN_080165D0(void)`
- O que faz: loop de 6 registros (stride 0x40, base `DAT_080165EC`). Se o byte +9 == 1, chama `Slot_ClearAttrMask(i,1)`. Senão, decrementa +9 em 8 (mínimo 1, ao chegar ele fixa 1) e +8 em 8.
- Chama: Slot_ClearAttrMask.
- Confiança: chute (provável temporizador de saída/fade).

## 0x080166AC Scene_LoadTwoTileSets
- Assinatura: `undefined4 FUN_080166AC(void)`
- O que faz: chama duas vezes `LoadTilesWithMalias`: (9, 0x15, ponteiro `DAT_080166E0`, `*DAT_080166E4`, 0x288) e (0x12, 0x15, `DAT_080166E8`, `*DAT_080166EC`, 0x2D0).
- Confiança: provável (carregamento de tiles; números de bloco não confirmados).

## 0x08016DCC FUN_08016DCC (sugestão: Hud_DrawTwoDigitFractionPanel)
- Assinatura: `undefined4 FUN_08016DCC(void)`
- O que faz: se `ctx+0x82 == 1`, preenche o retângulo de tilemap (2,1,14x12) e desenha uma fração de 2 dígitos (`Num_DrawFractionTwoDigits`) com dois bytes da entrada `ctx+0x80*0x100` (+0x50 e +0x58). Senão, limpa o retângulo (`BgMap_FillRect16`).
- Confiança: chute (HUD de texto; significado dos valores não confirmado).

## 0x08017268 FUN_08017268 (sugestão: Msgbox_UpdateByState)
- Assinatura: `undefined4 FUN_08017268(void)`
- O que faz: `Slot_ClearAttrMask(0x14,0x10)`, depois escolhe a fonte da mensagem por `ctx+0x82` (0..3): copia 3 bytes de tabela (`Mem_CopyBytes_Dup`) ou uma string de 14 bytes, ou monta um glifo repetido (`BuildRepeatedGlyphString`, 13 vezes) se o contador (+5) estiver abaixo do limiar (10, 30, 60). Termina com `Text_LayoutGlyphSprites`.
- Confiança: chute.

## 0x080173B4 FUN_080173B4 (sugestão: Msgbox_UpdateWithParam)
- Assinatura: `undefined4 FUN_080173B4(int param_1)`
- O que faz: como `FUN_08017268`, mas maior: limpa 0x24..0x2D de atributos, escolhe mensagem pelo estado `ctx+0x82` e pelo flag `ctx+0x58`/byte 0xC1 == 'P'. Chama `FUN_08017734(0 ou 1)`. Com `param_1 == 0` faz layout simples; com != 0 usa a flag 0x08 de `DAT_080175B4+0x84` como deslocamento.
- Chama: Slot_ClearAttrMask, FUN_08017734, Text_LayoutGlyphSprites.
- Confiança: chute.

## 0x080178CC FUN_080178CC (sugestão: Fight_UpdateSideLabelBlink)
- Assinatura: `undefined4 FUN_080178CC(void)`
- O que faz: se o byte 0xC1 do bot atual (`ctx+0x80`) == 'P', incrementa o contador `ctx+0x85` e preenche a faixa de rótulo A (bit 3 = 0) ou B (bit 3 = 1) com `Fight_FillSideLabelBandA/B`, pelo mesmo índice.
- Confiança: provável (pisca de rótulo de lado); nome não confirmado.

## 0x08017914 -> renomeado Fight_FillSideLabelBandForBot
- Assinatura: `undefined4 Fight_FillSideLabelBandForBot(void)`
- O que faz: wrapper. Se o byte 0xC1 do bot (`ctx+0x80`) == 'P', chama `Fight_FillSideLabelBandB`; senão `Fight_FillSideLabelBandA`.
- Confiança: provável.

## 0x08017A7C FUN_08017A7C (sugestão: Fight_WritePartPanelComposite)
- Assinatura: `undefined4 FUN_08017A7C(uint param_1, undefined4 param_2, undefined4 param_3)`
- O que faz: mapeia o índice (>2 vira -3) e compara com os bytes `+0x3D` e `+0x46` de `DAT_08017AB0` para escolher o lado (0 ou 1). Chama `Fight_GetDisplayedPartIdsOrBrokenGlyphs` e `Sprite_WriteComposite4Objects`, e depois limpa o bit 0x20 de duas entradas de OAM.
- Confiança: chute.

## 0x08017B24 FUN_08017B24 (sugestão: Fight_SetPartPanelPosFromTable)
- Assinatura: `undefined4 FUN_08017B24(void)`
- O que faz: grava 0 no byte +0x21C e zera/ajusta halfwords de posição (+0x200, +0x204). Lê dois bytes de uma tabela (`DAT_08017B98`) indexada por `(ctx+0x3C*3 + ctx+0x3D)*2` e soma/subtrai 0x10 na posição, conforme `ctx+0x3C == 0`.
- Confiança: chute.

## 0x08017734 FUN_08017734 (sugestão: Fight_DrawBotInfoPanel)
- Assinatura: `undefined4 FUN_08017734(int param_1)`
- O que faz: com `param_1 == 0`: copia 3 bytes e 6 bytes de tabelas (índices do bot em `ctx+0x80` e `ctx+0x82`), calcula `ctx+0x22 / 10` (`aux_div_u32`), preenche as linhas do painel (`BgMap_FillRect16`/`FillBgTilemapRectSequential`), desenha um decimal de 6 dígitos (`Num_DrawDecimal6Tilemap`, valor em `+0x110`) e chama `Fight_FillSideLabelBandB`. Com `param_1 != 0`: limpa o retângulo, chama `Fight_DrawBotNumericReadout`, e `Fight_FillSideLabelBandA` se não for 'P'.
- Chama: aux_div_u32, Num_DrawDecimal6Tilemap, Fight_DrawBotNumericReadout, Fight_FillSideLabelBandA/B.
- Confiança: chute quanto ao nome (é o painel de info do bot).

## 0x08017C50 FUN_08017C50 (sugestão: Fight_FillInfoRows_BySide)
- Assinatura: `undefined4 FUN_08017C50(int param_1)`
- O que faz: com `param_1 == 0` preenche a faixa superior do painel (colunas 0x1A ou 2, conforme `+0x3C`); com `!= 0` limpa a faixa correspondente.
- Confiança: chute.

## 0x08017D14 FUN_08017D14 (sugestão: Tilemap_FillFiveBandRows)
- Assinatura: `undefined4 FUN_08017D14(void)`
- O que faz: 5 vezes, preenche um retângulo 9 colunas de largura nas linhas 2, 4, 6, 8 e 10 (`FillBgTilemapRectSequential`) com o tile de `DAT_08017D48` e palette `i*0x40000`.
- Confiança: chute.

## 0x08017D50 FUN_08017D50 (sugestão: Fight_CopyPartTileset_CpuFastSet)
- Assinatura: `undefined4 FUN_08017D50(void)`
- O que faz: `bios_cpufastset(src, DAT_08017D8C, 0x20)` com origem = tabela indexada por `(ctx+0x3C*3 + ctx+0x3D)*0x100` (+1) * 0x80 + `DAT_08017D88`. Cópia de 0x20 words (CpuFastSet, 0x80 bytes por entrada).
- Chama: bios_cpufastset (wrapper 0x0807CCC0).
- Confiança: chute.

## 0x08017D90 FUN_08017D90 (sugestão: Fight_ShowPartLabelText)
- Assinatura: `undefined4 FUN_08017D90(void)`
- O que faz: `Slot_ClearAttrMask(0x16,10)`, copia 0xB bytes de uma tabela de ponteiros (`DAT_08017DF4`, índice = `(ctx+0x3C*3 + ctx+0x3D)*0x100 + 1`) para o buffer `ctx+0x9E`, e chama `Text_LayoutGlyphSprites(…, 0x16, 0xD, 2, 7, 0xC, 1, 2)`.
- Confiança: chute.

## 0x080191E4 FUN_080191E4 (sugestão: Fight_AddBotMeterFromTable)
- Assinatura: `undefined4 FUN_080191E4(int param_1)`
- O que faz: lê o byte `bot+0x91` do bot `param_1` (base `DAT_08019220 + param_1*0x100`), subtrai 100, multiplica por 0xB, soma o byte `ctx+0x38` e usa como índice numa tabela de 1 byte (`DAT_08019224`). Passa o valor para `Fight_AddBotMeterCapped255(param_1, valor)`.
- Chama: Fight_AddBotMeterCapped255.
- Confiança: chute (a tabela não foi lida).
