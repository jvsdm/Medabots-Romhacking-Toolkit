# FN2: funções FUN_ 0x08030000..0x08058000 (lote do agente GH-FN2)

Progresso: 50 de 50 (ver FN2_lista.txt). Confiança: confirmado | provável | chute.

## 0x080330e8 Fight_RollStatusRowRandomEntry_Wrapper
- Confiança: confirmado
- wrapper que chama Fight_RollStatusRowRandomEntry

## 0x08034e94 FUN_08034e94
- Confiança: chute
- Nome sugerido: Fight_LookupByteViaPtrTable_A8. Lookup de byte em 2 niveis (tabela de ponteiros + u16 +0xBC)

## 0x080353ac FUN_080353ac
- Confiança: provavel
- Nome sugerido: Ui_SetHalfwordModeFromBit1. Grava u16 0 ou 2 em +2 conforme bit1 do arg

## 0x08035c3c FUN_08035c3c
- Confiança: chute
- Nome sugerido: Obj_SetFlagFromBit0_F8. Escreve 0x110 ou base conforme bit0 de byte +0xF8

## 0x08036bf4 FUN_08036bf4
- Confiança: chute
- Nome sugerido: Ui_FillWindowRect12x13_ByFlag. BgMap_FillRect16 em (12,13), largura 2 ou 0x10 conforme arg

## 0x0803d74c Screen_InitOverlayAndLoadAssets
- Confiança: provavel
- Init de tela: limpa, CpuFastSet, DISPCNT, 4x decompress_and_store, layout overlay, snd_song_request(8)

## 0x08041b1c FUN_08041b1c
- Confiança: chute
- Nome sugerido: Fight_UpdateBotFlag4000ByState. Liga/mascara bit 0x4000 em tabela stride 0x40 conforme estado +0x84

## 0x08043fec FUN_08043fec
- Confiança: chute
- Nome sugerido: Anim_AdvanceRingFrameEvery3. Contador ciclico em +0x7D; a cada 3 chamadas copia bytes e CpuSet

## 0x0804765c FUN_0804765c
- Confiança: chute
- Nome sugerido: Menu_SetStateByArg01. Arg 0 ou 1 escolhe um de dois conjuntos de flags e grava valor (0xD/5 ou 0)

## 0x080476e8 MainState_RequestModeTransition
- Confiança: provavel
- Switch de modo 0..0xC: grava gMainState=1 e flags de init, limpa globais em 10 e 0xB

## 0x08047850 FUN_08047850
- Confiança: chute
- Nome sugerido: Sel_RequestIfIndexBelow4. Se 0 <= arg < 4 grava 1, 1 e 5 em tres ponteiros

## 0x08047bcc Table_GetWordByIndex_08047bd8
- Confiança: confirmado
- Retorna palavra da tabela 0x08047BD8 [arg]

## 0x08047c00 FUN_08047c00
- Confiança: chute
- Nome sugerido: Overworld_ClearTwoSlotPairs. Chama Overworld_NoOpStateHandler e zera dois pares u16

## 0x08047c24 FUN_08047c24
- Confiança: provavel
- Nome sugerido: Obj_TwoStageInitThenDispatch. Estado 0 = init/reset de slot; 1 = StateMachine_DispatchStep

## 0x08047d24 Num_FormatTable4EntryToGlyphs
- Confiança: provavel
- Copia 4 palavras de tabela 0x08047D48 e formata a entrada [arg] como decimal em glifos

## 0x08047d9c Inv_ReadCounter16_Wrapper
- Confiança: confirmado
- Wrapper que chama Inv_ReadCounter16(arg1, arg2)

## 0x08047dc0 Map64_AddCountDefault1_Wrapper
- Confiança: confirmado
- Wrapper: arg 0 vira 1 e chama Map64_AddCountClamp99

## 0x08047dd8 Map64_SubDefault1_Wrapper
- Confiança: confirmado
- Wrapper: arg 0 vira 1 e chama Map64_SubRemoveIfZero

## 0x08047df0 Inv_AddCountClamp99_Wrapper
- Confiança: confirmado
- Wrapper que chama Inv_AddCountClamp99(arg1, arg2, arg3)

## 0x08047e20 Medabot_SpawnFromRecord_Wrapper
- Confiança: confirmado
- Wrapper que chama Medabot_SpawnFromRecord(arg1)

## 0x08047e30 Tinpet_IsPoolFull_Wrapper
- Confiança: confirmado
- Wrapper que chama Tinpet_IsPoolFull

## 0x08047e3c Tinpet_CreateRecord_Wrapper
- Confiança: confirmado
- Wrapper que chama Tinpet_CreateRecord(arg1)

## 0x08048000 FUN_08048000
- Confiança: chute
- Nome sugerido: Fx_SetupLayersAndInitRecord8. Escreve 0x500 e 0x1F60 em registradores, chama Fx_InitRecord8(arg,0x16,7), zera +100

## 0x08048038 FUN_08048038
- Confiança: provavel
- Nome sugerido: Fx_StateMachine_Dispatch3. Maquina de 3 estados no byte +100 (1: DispatchSubB; 2: StateTable_DispatchByIndex)

## 0x08048088 FUN_08048088
- Confiança: chute
- Nome sugerido: Table_GetByteStride8_Pair. Retorna byte de tabela com passo 8 (arg 0 ou 1; outro retorna 0)

## 0x080480d4 Table_GetWordByIndex_080480e0
- Confiança: confirmado
- Retorna palavra da tabela 0x080480E0 [arg]

## 0x08048210 FUN_08048210
- Confiança: chute
- Nome sugerido: Icon_MapIdToSlotIndex. Mapeia ID (0 = lookup; 0xB0..0xCF tabela aninhada; 0xFF); >=0xC0 vira (id-0xC0)*2+base+0x28

## 0x080482cc FUN_080482cc
- Confiança: provavel
- Nome sugerido: Gfx_LoadLz77Table12ByIndex. Chama FUN_08048210 e faz 12 bios_lz77_vram com passo 0x900

## 0x0804830c FUN_0804830c
- Confiança: chute
- Nome sugerido: Gfx_BuildDefaultOrEntryTileRecord. Se entrada da tabela == 0 grava 14 pares de constantes; senao 3 palavras

## 0x080483f8 FUN_080483f8
- Confiança: chute
- Nome sugerido: Bot_ReadCurrentByteOrDefault. Le byte de um de dois enderecos conforme byte +0xC30 == -1

## 0x08048684 FUN_08048684
- Confiança: chute
- Nome sugerido: Icon_LookupAndMapByIndex. Wrapper: byte de tabela indexada por arg, depois FUN_08048210

## 0x080486b0 FUN_080486b0
- Confiança: chute
- Nome sugerido: Icon_BuildSlotRecordList. Laco ate FUN_08048684(i) == 0xFF, preenche registros de 0x20 bytes

## 0x0804877c FUN_0804877c
- Confiança: chute
- Nome sugerido: Icon_Build12SlotRecords. Preenche 12 registros de 0x20 a partir de FUN_08048210

## 0x08048960 FUN_08048960
- Confiança: chute
- Nome sugerido: Fight_ClassifyActionState. Switch no arg devolve 0..4 conforme flags

## 0x080491b8 BgMap_FillRectTilePalette
- Confiança: provavel
- Preenche retangulo w x h do tilemap (stride 0x20 ou 0x40) com tile + (paleta << 12)

## 0x08049210 BgMap_CopyRectWithAttrs
- Confiança: provavel
- Copia retangulo de array fonte para tilemap aplicando mascara e atributos (paleta << 12)

## 0x080492a8 BgMap_CopyRectAddOffset
- Confiança: provavel
- Copia retangulo de array fonte para tilemap somando param9 a cada entrada (passo 0x20 ou 0x40)

## 0x08049344 FUN_08049344
- Confiança: chute
- Nome sugerido: Slot_ClearIndexedBuffers. Zera 4 bytes e 0x40 bytes em blocos indexados por (arg & 0xFF)

## 0x080497e0 FUN_080497e0
- Confiança: chute
- Nome sugerido: Obj_SetStateAndDispatchJT. Grava 3 args em struct, estado +2=1, despacha jump table em +1 (0..6)

## 0x08049a6c FUN_08049a6c
- Confiança: chute
- Nome sugerido: Anim_UpdateSoftFloatStep. Passo de estado com soft-float (sf_add/sf_cmp) e jump table em +1

## 0x08049cc8 FUN_08049cc8
- Confiança: chute
- Nome sugerido: Obj_LoadPairFromRecord. Copia 6 bits de dois bytes do registro para dois destinos

## 0x08049ec0 Palette_FadeRowsToColorA_Wrapper
- Confiança: confirmado
- Wrapper que chama Palette_FadeRowsToColorA

## 0x08049ed4 Palette_FadeRowsToColorB_Wrapper
- Confiança: confirmado
- Wrapper que chama Palette_FadeRowsToColorB

## 0x0804aa44 FUN_0804aa44
- Confiança: chute
- Nome sugerido: GameState_ToggleAndAdvance. Zera byte de estado se igual ao vizinho e chama GameState_AdvanceSubroutine

## 0x0804b738 Stub_ReturnZero_0804B738
- Confiança: confirmado
- Funcao vazia: retorna 0

## 0x0804f188 Map_CameraFollowStepByDir
- Confiança: provavel
- Move alvo e camera (Map_AddCameraScrollX/Y) em 4 direcoes por arg 0..3; usa ikki_x/y_to_tile no fim

## 0x08050124 FUN_08050124
- Confiança: chute
- Nome sugerido: Obj_SetNibblesAndStepTimer. Grava nibbles do arg em +0x20/+0x27, +0x2A = arg3, +0x28 = 16/passo

## 0x080502c8 FUN_080502c8
- Confiança: confirmado
- Nome sugerido: Util_NibbleHi12PlusLo2x3. Retorna (nibble alto*12) + (2 bits baixos*3)

## 0x08035dd4 FUN_08035dd4
- Confiança: chute
- Nome sugerido: Range_IsOutsideLimitA8. Retorna (arg + const) > 0xA8

## 0x08050320 FUN_08050320
- Confiança: provavel
- Nome sugerido: Obj_ComputeActorTableIndex. Indice = (byte do ator rastreado*0x40 + base + 0x2B) * 12 + arg2 * 3

