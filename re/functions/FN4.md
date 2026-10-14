# FN4 - nomes para FUN_ da lista FN4_lista.txt

Conferido no Ghidra antes de cada um: 20 dos 55 ja tinham nome (pulados); 35 eram FUN_ e foram tratados. Chamadores por xref do Ghidra (quase vazio para chamadas por tabela) e por busca na ROM (ponteiro addr|1 e BL). Hints do FN2 (comentarios no Ghidra) foram conferidos e usados quando batiam com o decompile.

- 0x08000824 Irq_Slot3_CallHandlerAndAck: provavel; ponteiro na tabela IRQ 0x0807E860 idx 3; chama *DAT_08000838 e grava 2.
- 0x08002190 Link_MarkHandshakeReady: provavel; chamado por Link_HandshakeFrameStep 0x08065B34; marca byte +6.
- 0x0800393C PartScreen_InitSpritesAndLoadGfx: provavel; LZ77 + Slot_SetupFromRecordList; chamado pela tela de peca (0x080034EC, 0x080038DA).
- 0x08004A40 Overlay_TickThenAdvanceWhenReached: provavel; tabela 0x083AC568 idx 1 (passo de sobreposicao).
- 0x08004A78 Overlay_ResetFieldsAtState18AndAdvance: provavel; tabela 0x083AC568 idx 2; zera campos ao estado 0x18.
- 0x0800994C Obj_UpdateAttrBit4000ByFlag8: provavel; liga/desliga bit 0x4000; 4 chamadores em 0x0800783x-0x0800799x.
- 0x0800B79C Obj_StepTableEntry_Flag4000AndCount: provavel; ponteiro 083ACA4C; mesmo padrao de 0x4000 com contador.
- 0x0801275C Fight_AddByteTimes5ToXpPool: confirmado; fi+0x16C += byte(fi+0x16B)*5; chamado por 080125A2 (pool de XP).
- 0x080140B0 Fight_SceneStep65_TimedSequence: provavel; entrada 65 da tabela de cena de batalha 0x083AD080 (0x083AD184).
- 0x08017268 Fight_UpdateBotNameLine: provavel; copia nome/medalha e monta a linha de texto; chamado por 08013410 e 08017662.
- 0x080173B4 Fight_UpdateBotInfoPanel: provavel; param 0 so texto, param 1 redesenha painel; 4 chamadores.
- 0x080178CC Fight_AnimateSideLabelBandIfPart: provavel; se tipo de perna 'P' anima faixa lateral; chamado por 08012C88.
- 0x080191E4 Fight_AddBotMeterFromTable: provavel; le tabela por (bot+0x91-100)*11 + byte +0x38 e soma no medidor; chamado por 080190D6.
- 0x080353AC Ui_SetHalfwordModeFromBit1: confirmado; grava u16 0 ou 2 conforme bit 1; chamadores 0801CC62, 08021CA8.
- 0x08035C3C Obj_SetWordFromBit0OfF8: provavel; escreve 0x110 ou base conforme bit 0 de +0xF8; chamado por 0801E1C0.
- 0x08035DD4 Util_IsOffsetAbove0xA8: confirmado; retorna (arg + const) > 0xA8; 4 chamadores.
- 0x0803D15C Anim_StepSlotScriptAndPosition: provavel; Anim_RunSlotScript + posicao por CompositeSprite_ComputeOffsetWord; ponteiro 083AFBC4.
- 0x0803D5E0 Link_StepAdvanceWhenReached: provavel; ponteiros 083AFC7C/083AFDC0 (tabela da tela Link, AN2).
- 0x0803D608 Link_StepCycle8AndAdvance: provavel; conta +0x86 de 0 a 8; ponteiros 083AFC80/083AFDBC/083AFDC4/083AFDCC.
- 0x0803D6AC Link_ResetFieldsAndSetGs50: confirmado; zera campos do ator e grava 0x50 no GS; ponteiro 083AFC8C.
- 0x0803D71C Link_RollTableEntryAndAdvance: confirmado; randbyte() & 0x1F indexa tabela; ponteiro 083AFC90.
- 0x08041B1C Fight_UpdateBotFlag4000ByState: provavel (hint FN2); stride 0x40 por byte +0x2B; ponteiro 083B29B0.
- 0x08047850 Sel_RequestIfIndexBelow4: provavel (hint FN2); grava 1, 1, 5 em tres ponteiros; 4 chamadores 0806xxxx-0807xxxx.
- 0x08048038 Fx_StateMachine_Dispatch3: provavel (hint FN2); maquina de 3 estados no byte +0x64; chamadores 0804E816/0804E88A.
- 0x080482CC Gfx_LoadLz77Table12ByIndex: provavel (hint FN2); 12 chamadas LZ77 para VRAM com passo 0x900; chamador 08059970.
- 0x080483F8 Bot_ReadCurrentByteOrDefault: provavel (hint FN2); le 1 B conforme byte +0xC30 == 0xFF; 5 chamadores 0804848x-0804866x.
- 0x080486B0 Icon_BuildSlotRecordList: provavel (hint FN2); laco ate 0xFF com registros de 0x20 B; chamador 0804CD50.
- 0x0804877C Icon_Build12SlotRecords: provavel (hint FN2); 12 registros de 0x20 B; chamador 08059978.
- 0x08049344 Slot_ClearIndexedBuffers: confirmado; zera 4 B e 0x40 B indexados por arg; chamador 0804889E.
- 0x08049A6C Anim_UpdateSoftFloatStep: provavel (hint FN2); passo com soft-float e jump table 0..6; 5 chamadores.
- 0x0804ACC8 Map_AdvanceSubroutineStep_F: confirmado; so chama GameState_AdvanceSubroutine; ponteiro 083D21AC.
- 0x0804ACD4 Map_InitScreenAndStartTransition: provavel; limpa OAM/paleta, DISPCNT 0x1E40, carrega o mapa, SND_PolicyRequest(0xB); ponteiro 083D21B0.
- 0x0804AF98 Map_HandleMarkerStepThenAdvance: provavel; Map_HandleMarkerCommandFromState(0); ponteiro 083D21EC.
- 0x0804AFCC Screen_TransitionStepResetOnDone: provavel; ponteiro 083D21F4.
- 0x0804AFE0 Overworld_StartTransitionThenAdvance: provavel; ponteiro 083D21F8.

Em aberto: os callers por ponteiro de tabela (083AFxxx, 083ACxxx, 083D21xx) so foram confirmados por ponteiro, nao pelo passo na tabela; nomes "provavel" nao testados no emulador. Nenhum nome de 0x0804A6xx/0x0804ABxx (ja nomeados por outro agente) foi alterado.
