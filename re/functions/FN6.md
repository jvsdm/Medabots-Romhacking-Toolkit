# FN6: nomes para 34 FUN_ (Medabots Metabee EU)

Metodo: decompile via Ghidra MCP; chamadores por scan de BL e de ponteiros na ROM (get_xrefs do MCP nao retornou nada).
Nomes com "provavel" foram renomeados no Ghidra; "chute" ficou FUN_ (nome sugerido aqui).

Padrao observado: a maioria das funcoes e passo de maquina de estados de cena. Retorno != 0 do "step" grava proximo estado (0,7,2 / 0,8 / 9 / 10 / 0xB / 0xC etc.). Tabela de ponteiros 0x08485700 (25 entradas, handlers 0x08068924..0x080692EC) nao tem literal direto na ROM; provavelmente e carregada por ponteiro de RAM.

- 0x08068924 FUN_ (chute Scene_StepCountdown16): se X%B==0 grava 0x10-X/B; retorna X==16*B. Ponteiros 0x0848570C/10.
- 0x08068978 FUN_ (chute): liga BG0, 4 escritas de tabela por X/B; retorna X==16*B. Ponteiro 0x08485714.
- 0x08068B74 Scene_ProgressIndex16 (provavel): grava X/B, retorna X==16*B. Ponteiros 0x08485720/24.
- 0x08068E00 Scene_EnterSetupA (provavel): setup de cena (gfx_clear, snd 0 e 0x13, fx_screen_setup_a, BG 0x1E00). Ptr 0x08485730.
- 0x08068E88 Scene_FxStepAdvance (provavel): fx_step_table_a() != 0 copia estado. Ptr 0x08485734.
- 0x08068F08 Scene_IntroStepAdvance (provavel): intro_scene_step() != 0 grava 0,4,2. Ptr 0x0848573C.
- 0x08069168 Scene_DispatchStepThenAdvance7 (provavel): dispatch_scene_state() != 0 vai p/ 7. Ptr 0x08485748.
- 0x08069194 Scene_EnterSetupB (provavel): setup, snd 0x17, vai p/ 8. Ptr 0x0848574C.
- 0x080691E0 Scene_AnimSeqStepAdvance9 (provavel): BotAnim_TickStageCallbacksAll + scene_sequence_b, vai p/ 9. Ptr 0x08485750.
- 0x0806920C Fight_FrameTickWaitGate10 (provavel): quadro de batalha; BL para 0x0806A860 e 0x0806AAA4; vai p/ 10. Ptr 0x08485754.
- 0x08069258 Scene_EnterSetupC (provavel): setup com menu_draw_stats, BG 0x1A00, vai p/ 0xB. Ptr 0x08485758.
- 0x080692A8 Scene_SequenceAFadeAdvance (provavel): scene_sequence_a, snd_fade 0x20, vai p/ 0xC. Ptr 0x0848575C.
- 0x080692EC FUN_ (chute): gfx_clear, snd 0, escreve modo 1/2 e FUN_08047850(0). Ptr 0x08485760.
- 0x0806A860 Fight_TickSlotTimersAndSpawn (provavel): loop de 3 slots, timers e spawns. Chamador: BL de 0x0806920C.
- 0x0806AAA4 Fight_TickSlotMotionPhases (provavel): loop de 3 slots, velocidade de sprite e Sprite_UpdateComposite4Motion. Chamador: BL de 0x0806920C.
- 0x0806B3D8 BotAnim_StageWalkStep (confirmado que e callback de estagio; nome provavel): anda X +-1 com limite 0x60. Tabela 0x084857D0 idx 9, via BotAnim_TickStageCallbacksAll 0x0806B0D4 (confirmado no decompile).
- 0x0806BB24 Scene_StepFxSongAdvance (provavel): fx_step != 0 toca musica 0x14 e incrementa. Ptr 0x08485904.
- 0x0806BB44 Scene_DispatchState8Anim (provavel): dispatch_state_8 != 0 incrementa; anim_dispatch_state. Ptr 0x08485908.
- 0x0806BB64 FUN_ (chute): ciclo de estados em buffer de 0xC bytes com aux_mod_u32 (3 e 7). Ptr 0x0848590C.
- 0x0806D588 Battle_TickEffectsAndTransition (provavel): battle_effect_affine_lines, CallViaR0, battle_effects_5_slots_update, battle_transition_main_state. Ptr 0x084856F0.
- 0x0806DBFC Battle_WaitTransitionA (provavel): bit 8 faz battle_slot_init; senao espera e vai p/ 3. Ptr 0x08485984.
- 0x0806DC7C Battle_TimedTransition41 (provavel): conta ate 0x29, slot_ptr_stub(2), vai p/ 2. Ptr 0x08485988.
- 0x0806E130 Battle_DispatchPhaseTable (provavel): CallViaR0 com tabela 0x084856F4 indexada por *DAT_0806e14c.
- 0x0806E150 Scene_StepLoadStateTriple (provavel): fx_step != 0 copia 3 halfwords p/ gMainState e vizinhos. 5 ptrs (0x08485968..0x08485B6C).
- 0x0806E4CC Scene_FadeStepLoadStateTriple (provavel): fade_step_bld != 0 zera e copia 3 halfwords. Ptr 0x084859E4.
- 0x0806E90C Scene_TimedWait61ThenAdvance2 (provavel): conta ate 0x3D, vai p/ 2. Ptr 0x084859EC.
- 0x0806E93C Scene_TimedWait60ThenAdvance8 (provavel): conta ate 0x3C, vai p/ 8. Ptr 0x084859F4.
- 0x0806E9C0 FUN_ (chute): grava 3, 1, 0 em tres enderecos. Ptr 0x08485A00.
- 0x0806EA98 Battle_MenuPhaseDispatch (provavel): menu_state2_setup, CallViaR0 com tabela 0x084856EC, battle_exit_button. Ptr 0x084856EC.
- 0x080732C8 Obj_FlushFlag8ToWord (provavel): se obj+0x50 bit3 grava obj+0x54 e limpa bit. Sem BL/ptr achado (hint do agente anterior de BL nao confirmado).
- 0x080733BC Obj_FlushFlag2ScaledValue (provavel): se bit1, (obj+0x40*obj+0x66)&mask. Sem BL/ptr achado.
- 0x080734F4 Obj_FlushFlag1Value69 (provavel): se bit0, grava obj+0x69|0x8000. Sem BL/ptr achado.
- 0x08073B2C FUN_ (chute): escreve halfwords com mascara 0x440 em ponteiro indexado. Sem BL/ptr achado.
- 0x0807DD08 Math_F64IsZero (provavel): sf_unpack/sf64_pack e retorna valor==0. Sem BL/ptr achado.

Em aberto: os 6 chute continuam FUN_; 0x080732C8..0x08073B2C e 0x0807DD08 sem chamador (talvez ponteiro montado em RAM ou CallViaR0); tabela 0x08485700 sem chamador achado.
