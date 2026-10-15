# LK4 - tela LINK (cabo), tabela 0x083AFC78 (96 ponteiros)

Leitor: `Scene_GsDispatch_Menu10_Link` 0x0803D538 -> CallViaR0(tabela[GS]), GS = 0x0300137C.
Quase todo passo "avanca" fazendo `*GS = *GS + 1` (literal 0x0300137C confirmado em cada Anim_* e Obj_* lido).
Passos de pulo gravam um valor fixo em GS.

## Passo 1: tabela (indice -> endereco -> nome). NoOp = 0x08000841 omitido (26 entradas: 15,25-31,46,47,69-71,76-79,87-95)

| idx | endereco | nome atual (CSV) | status |
|---|---|---|---|
| 0 | 0x0803D5A1 | Fight_ResetStateFlagsAndCounter | (ver passo 2) |
| 1 | 0x0803D5E1 | Link_StepAdvanceWhenReached | |
| 2 | 0x0803D609 | Link_StepCycle8AndAdvance | |
| 3 | 0x0803D635 | LinkTick_MenuStep3 | generico |
| 4 | 0x0803D675 | LinkTick_MenuStep4 | generico |
| 5 | 0x0803D6AD | Link_ResetFieldsAndSetGs50 | |
| 6 | 0x0803D71D | Link_RollTableEntryAndAdvance | |
| 7 | 0x0803D74D | Screen_InitOverlayAndLoadAssets | |
| 8 | 0x0803D929 | Link_StepPaletteFadeDown | |
| 9 | 0x0803D981 | Link_StepLoadObjAndAdvance | |
| 10 | 0x0803D9A1 | Fight_ShowPreBattleTextIndex1 | |
| 11 | 0x0803D9C9 | Link_StepCopyPaletteAndAdvance | |
| 12 | 0x0803D9F1 | Link_StepPaletteFadeUp | |
| 13 | 0x0803DA91 | Fight_ClearBattleBgAndBlend | |
| 14 | 0x0803DAED | Fight_InitSelectBlockA | |
| 16 | 0x0803E4B5 | PeerLost_MenuStep16 | generico |
| 17 | 0x0803E4D9 | Anim_GpuBlendTickThenCount | confirmado (AN2) |
| 18 | 0x0803E4F5 | Gfx_BlankSceneInit | |
| 19 | 0x0803E4D9 | Anim_GpuBlendTickThenCount | confirmado (AN2) |
| 20 | 0x0803E619 | Anim_InitSlotAndLoadCount | decompilado: Obj_InitSlotAndLoad(1,0,0); GS++ |
| 21 | 0x0803E635 | Flow_StepCounterTickA | |
| 22 | 0x0803E4B5 | PeerLost_MenuStep16 | generico |
| 23 | 0x0803E4D9 | Anim_GpuBlendTickThenCount | confirmado (AN2) |
| 24 | 0x0803E675 | Boot_ResetSystemWrapper | |
| 32 | 0x0803D74D | Screen_InitOverlayAndLoadAssets | |
| 33 | 0x0803DB1D | Snd_RequestSongByFlag | |
| 34 | 0x0803D929 | Link_StepPaletteFadeDown | |
| 35 | 0x0803D981 | Link_StepLoadObjAndAdvance | |
| 36 | 0x0803DB4D | Fight_StepMessageAdvanceSfx | |
| 37 | 0x0803DB95 | Link_StepDispatchSubB_ThenAdvance | |
| 38 | 0x0803DBB9 | Link_StepDispatchStateMachine | |
| 39 | 0x0803DBE1 | Link_StepWaitOverlayTarget | |
| 40 | 0x0803DC0D | Link_StepCycleTimer9 | |
| 41 | 0x0803DC3D | Link_StepSoundFadeOut | |
| 42 | 0x0803D9C9 | Link_StepCopyPaletteAndAdvance | |
| 43 | 0x0803DA41 | Link_StepPaletteFadeUp_B | |
| 44 | 0x0803DC59 | Link_StepSetStateWordByFlag | |
| 45 | 0x0803DC8D | Screen_ResetAndSetSub4Mode | |
| 48 | 0x0803DCDD | Overlay_InitScreenGraphics | |
| 49 | 0x0803DE71 | Link_StepGpuBlendTick | |
| 50 | 0x0803DE91 | Link_StepLoadSlotAndAdvance | |
| 51 | 0x0803DEB1 | Link_StepDispatchByFlag | |
| 52 | 0x0803DEF1 | LinkSync_MenuStep52 | generico |
| 53 | 0x0803DF99 | LinkSync_MenuStep53 | generico |
| 54 | 0x0803E019 | Link_StepGaugeAndTween | |
| 55 | 0x0803E03D | Anim_RunSlotScriptThenSfx54 | confirmado (AN2) |
| 56 | 0x0803E099 | Anim_TweenUntilDoneThenReset | confirmado (AN2) |
| 57 | 0x0803E0C1 | Anim_TweenClampThenSfx2A | confirmado (AN2) |
| 58 | 0x0803E0F1 | Anim_DrawSegmentUntil120 | confirmado (AN2) |
| 59 | 0x0803E121 | Anim_DrawSegmentThenJumpPtr | confirmado (AN2) |
| 60 | 0x0803E171 | Anim_DrawSegmentThenLoadSlot | decompilado: Gauge_DrawTwoTileSegment; Obj_InitSlotAndLoad(1,0,0); GS++ |
| 61 | 0x0803E1A5 | Anim_DrawSegmentThenDispatchLoad | decompilado: se obj+0x9c==1 usa tabela 083CF530+0x10 senao +0x14; StateMachine_DispatchStep; se !=0 Obj_InitSlotAndLoad + GS++ |
| 62 | 0x0803E1FD | Anim_DrawSegmentThenDispatchStep | decompilado: desenha; StateMachine_DispatchStep(083CF530+4); se !=0 GS++ |
| 63 | 0x0803E235 | Anim_DrawSegmentUntilOverlayReached | decompilado: Overlay_SetTargetAndCheckReached; se ok, estado +0x84=0 e GS++ |
| 64 | 0x0803E271 | Anim_DrawCycleStateZeroToFour | confirmado (AN2) |
| 65 | 0x0803E2AD | Anim_DrawFadeSfx8Step | decompilado: snd_fade_request(8); GS++ |
| 66 | 0x0803E2DD | Anim_DrawFlushPaletteThenReset | decompilado: CopyPaletteBuffersToHardware; estado +0x84=0; GS++ |
| 67 | 0x0803E315 | Anim_DrawFadeRowsStep | decompilado: se estado<9 faz Palette_FadeRowsToColorA/B(0,estado<<2,0000FFFF) e estado++; senao GS++ |
| 72 | 0x0803E375 | Anim_PickSlotActivateEntryCopy | decompilado: Overlay_PickRandomSlotAndDrawCursor; se obj+0x9c==1 Inv_AddCountClamp99 (inventario) senao Tinpet_ActivateMatchingEntry; Mem_CopyBytes_Dup(0x030004DE..., 0F); GS++ |
| 73 | 0x0803E171 | Anim_DrawSegmentThenLoadSlot | igual a 60 |
| 74 | 0x0803E435 | Anim_DrawSegmentThenDispatchLoadAlt | decompilado: como 61, mas tabela 083CF530 +0x18 / +0x1C; GS++ se !=0 |
| 75 | 0x0803E48D | Anim_DrawSetValue3E | confirmado (AN2) |
| 80 | 0x0803E681 | LinkSync_MenuStep80 | generico (a decompilar) |
| 81 | 0x0803D609 | Link_StepCycle8AndAdvance | |
| 82 | 0x0803D5E1 | Link_StepAdvanceWhenReached | |
| 83 | 0x0803D609 | Link_StepCycle8AndAdvance | |
| 84 | 0x0803E768 | (sem nome no CSV) | leaf: GS = 6 (str direto) |
| 85 | 0x0803D609 | Link_StepCycle8AndAdvance | |
| 86 | 0x0803E774 | (sem nome no CSV) | leaf: GS = 5 (str direto) |

Enderecos das entradas 84 e 86 nao viram funcao no Ghidra (sem prologo push); bytes `01 49 06 20 08 60 70 47` e `01 49 05 20 08 60 70 47`
= ldr r1,[pc]; movs r0,#N; str r0,[r1]; bx lr, com literal 0x0300137C. Confianca: confirmado.

Correcao da linha 72: `Mem_CopyBytes_Dup(src, dst=0x030004DE, 0x0F)`; src = tabela de entradas (0x03001800 + slot).
Divergencia de nome: idx 36 (0x0803DB4D) no Ghidra atual = `Fight_ShowPostBattleTextIndex1`; o CSV antigo dizia `Fight_StepMessageAdvanceSfx`.

## Passo 2: renames feitos no Ghidra (6), confirmados/provaveis
- 0x0803D635 LinkTick_MenuStep3 -> LinkTick_SetScreenByteFromFrameThenAdvance (provavel)
- 0x0803D675 LinkTick_MenuStep4 -> LinkTick_WaitTopBitThenCopyScreenByte (provavel)
- 0x0803E4B5 PeerLost_MenuStep16 -> PeerLost_FadeSfx8BlendThenAdvance (provavel)
- 0x0803DEF1 LinkSync_MenuStep52 -> LinkSync_ChoiceWaitAPressThenAdvance (provavel)
- 0x0803DF99 LinkSync_MenuStep53 -> LinkSync_GaugeTo60ThenSfx39 (provavel)
- 0x0803E681 LinkSync_MenuStep80 -> LinkSync_ExchangeRecordsThenAdvance (provavel)
Entradas 84 e 86 nao viraram funcao no Ghidra (sem prologo); ficam so no CSV.

## Passo 3: fluxo da tela LINK (GS = 0x0300137C; "+1" = avanco; valor fixo = pulo)
Confianca geral: provavel. Nao rodei no emulador; o fluxo vem so de tabela + decompile.

1. Abertura (idx 0-4): 0 zera flags, GS 1; 1 espera overlay, GS 2; 2 cicla 8 quadros, GS 3; 3 grava byte de tela, GS 4; 4 espera bit 0x8000 do link, GS 5.
2. Inicio da troca (5, 80-86): 5 zera campos e grava GS=0x50 (80). 80 le/grava 10 registros pelo link no bloco de estado;
   ao fim GS 81. 81-83 esperas (+1). 84 grava GS=6 (segue para o sorteio). 86 grava GS=5 (volta ao inicio da troca).
   Confianca do ramo 84/86: provavel; o caminho exato de chegada a 85/86 nao fechei.
3. Sorteio e entrada de cena (6-14, 32-34): 6 sorteia byte (rand&0x1F) na tabela de 32 bytes em 0x083AFDF8 -> obj+0x38, GS 7;
   7 monta cena e musica 8, GS 8; 8/12 fade de paleta, 9 carrega objeto, 10 texto pre-batalha (SM), 11 copia paleta,
   13 limpa BG, 14 init de bloco. 33 musica 0xE/0xF pelo flag obj+0x9c.
4. Texto e batalha (36-44): 36 texto pos-batalha com sfx 0xD; 37-41 SM e espera de overlay; 44 se obj+0x138==0 GS+1,
   senao GS=0x30 (48).
5. Escolha e troca (48-54): 48 Overlay_InitScreenGraphics (musica 0x14); 51 SM por flag; 52 escolha com A (sfx 4, grava ctx+0x1A0);
   53 barra ate 60 quadros (sfx 0x39); 54 barra + tween. Os passos de barra (55-67, 75) sao os Anim_* do AN2 (barra de link).
6. Recebimento de peca (72): sorteia slot; se obj+0x9c==1 chama Inv_AddCountClamp99 (soma 1 ao inventario de pecas);
   senao Tinpet_ActivateMatchingEntry; copia 0x0F bytes para 0x030004DE; GS+1. CANDIDATO a "troca de peca por link"
   (README dizia nao achada). Confianca: provavel no nome, chute no papel (obj+0x9c==1 pode ser "lado local").
7. Saida: 45 e 68 Screen_ResetAndSetSub4Mode (limpa e Dispatch_Sub4Mode(3), sem GS+1); 24 Boot_ResetSystemWrapper (reset, sem GS+1).
   16 e 22 (PeerLost): snd 8 + blend, GS+1; provavel ramo de "parceiro perdido" que segue para o reinicio.
8. Conectar: nao vejo passo proprio na tabela. O handshake SIO (InitializeLinkTransferState / UpdateLinkTransferState,
   ver README LK) e feito fora desta tabela; idx 1-4 e 80 sao os passos que esperam o link. Em aberto.

Entradas NoOp (0x08000841): 15, 25-31, 46, 47, 69-71, 76-79, 87-95.
