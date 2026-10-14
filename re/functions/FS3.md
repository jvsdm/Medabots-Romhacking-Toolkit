# FS3: 38 candidatos de ponteiro (0x08009274..0x080109CC), todos funções no Ghidra

Origem: `FS3_lista.txt` (candidatos "ptr em X", achados por ponteiro de tabela). Conferido agora no Ghidra com
`get_function_by_address`: os 38 são início de função (corpo FUN_xxxxxxxx), então não são miolo nem dado.
Renomeados no Ghidra (38/38, "Function renamed successfully"). Ninguém foi salvo; o programa segue aberto.
Os 38 aparecem como ponteiros em quatro tabelas de estado (ROM, 0x083AC9F8, 0x083ACCD8, 0x083ACDD8, 0x083ACE14..0x083ACE70);
o Ghidra não tem xref de código para essas entradas (ponteiros em dado), então "chamada por" = tabela indicada.
Todos têm corpo pequeno e o mesmo esqueleto: Sprite_IncrementXYIfFlag, PartsGrid_DrawLargeDisplayIfOwned, avanço do
contador de estado em [objeto+0x84] (ou via StateMachine_DispatchStep) e volta.

Tabelas (pelos ponteiros na ROM, valor ímpar = Thumb):
- 0x083AC9F8: 0x08009274, 0x080092D4, 0x08009374, 0x080093E0 (sem nome no mtbe; fica entre table_partsgrid_states 0x083AC880 e table_partsbuild_states 0x083ACAE0).
- 0x083ACCD8..0x083ACD64 (gMenu5StepTable_ACCD4 em 0x083ACCD4): 0x0800DD00..0x0800E9F0 (25 entradas da tela de detalhe).
- 0x083ACDD8..0x083ACDF0: 0x0800EA98, 0x0800EB60, 0x0800EBE4 (+ reusa 0x0800DEC8 e 0x0800DF14).
- 0x083ACE14..0x083ACE70 (gSceneStepTable_ACE14): 0x08010170..0x080109CC (parte de peças e seleção de bot na batalha).

## 0x08009274 PartsGrid_TickFadeOutStep
- Faz: quadro de transição da grade de peças. Byte de estado em [obj+0x4A]: 0 limpa flag, seta blend (SetGpuBlendMode 6,0x3F,0x20) e estado=7; 1 incrementa contador; 2 seta [0x080092D0]=2.
- RAM: objeto da grade (ponteiro via literal pool), byte [+0x42] e [+0x4A]; contador em IWRAM via literal.
- Confiança: confirmado (corpo lido); nome provável.

## 0x080092D4 PartsGrid_TickRouteSelection
- Faz: escolhe a próxima tela depois da seleção. Lê bit 0x40 de byte da entrada da grade; se marcado, carrega slot e estado 9; se não, testa máscara em DAT_08009348: senão limpa entradas de OAM, snd_fade_request(8) e estado 0x68.
- RAM: entrada da grade [+0x3D]*0x40 + base; máscara de bits em ROM/IWRAM via literal.
- Confiança: confirmado (fluxo lido); nome provável.

## 0x08009374 PartsGrid_TickDispatchSubstate
- Faz: despacha o sub-estado da grade: StateMachine_DispatchStep sobre tabela[obj+0x84*4]; se retorna não-zero, marca [DAT_080093A8]=2.
- RAM: [obj+0x84] (índice do sub-estado).
- Confiança: confirmado; nome provável.

## 0x080093E0 PartsGrid_ResetAndSetMode1
- Faz: limpa atributos de OAM (ClearObjectAttributeEntries 0,0x80), Slots_ClearFourPairs e Dispatch_Sub4Mode(1).
- RAM: OAM (via função), slots.
- Confiança: confirmado; nome provável (par com PartsGrid_ResetAndSetMode2).

## 0x0800DD00 PartDetail_SlideInFadeStep
- Faz: quadro de entrada da tela de detalhe: move slot (modo 3, alvo DAT_0800DD58, 0x50), desenha display grande; se [obj+0x42]==1, funde linhas de paleta (A e B) por passo [obj+0x84]<9; senão GpuBlend_TickStep. Ao fim, zera [+0x42] e incrementa contador.
- RAM: [obj+0x42], [obj+0x84]; paleta (via funções).
- Confiança: confirmado; nome provável.

## 0x0800DD88 PartDetail_SlideInLoadStep
- Faz: mesmo quadro de entrada e depois Obj_InitSlotAndLoad(1,2,0); incrementa contador.
- Confiança: confirmado; nome provável.

## 0x0800DDC0 PartDetail_SlideInDispatchStep
- Faz: quadro de entrada e StateMachine_DispatchStep sobre [DAT_0800DDF8+0x40]; ao concluir, incrementa contador.
- Confiança: confirmado; nome provável.

## 0x0800DE00 PartsGrid_HandleSelectionInput
- Faz: entrada da grade: chama PartsGrid_MoveSelectionGrid4/List30/Option4; com A (bit 1) ou B (bit 2) toca sfx 4/5/6 conforme contador de inventário (Inv_ReadCounter16) e tipo da peça (0x3C==4 → 0x40, senão 10).
- RAM: entrada da peça [+0x3D],[+0x3E]; teclas em [DAT_0800DE54].
- Confiança: confirmado (fluxo lido); nome provável.

## 0x0800DEC8 PartDetail_SlideInClearStep
- Faz: quadro de entrada com limpeza por CpuFastSet (bios_cpufastset, 8 palavras) e GpuBlend ajustado (SetGpuBlendMode 6,0x3F,0x20); incrementa contador.
- Confiança: confirmado; nome provável.

## 0x0800DF14 PartDetail_SlideInBlendStep
- Faz: quadro de entrada até GpuBlend_TickStep terminar; então incrementa contador.
- Confiança: confirmado; nome provável.

## 0x0800DF4C PartDetail_InitStateRegisters
- Faz: limpa OAM e grava o estado inicial: [DAT_0800DF6C]=0, [DAT_0800DF70]=2, [DAT_0800DF74]=0x10.
- Confiança: confirmado (3 escritas); significado dos três campos é chute.

## 0x0800DF78 PartDetail_SlideInPaletteCommitStep
- Faz: quadro de entrada que copia buffers de paleta para o hardware (CopyPaletteBuffersToHardware), zera [obj+0x84] e incrementa contador.
- Confiança: confirmado; nome provável.

## 0x0800DFB8 PartDetail_SlideInFadeOutStep
- Faz: quadro de entrada com fade de paleta por passo [obj+0x84]<9; ao completar, grava 0x10 no estado.
- Confiança: confirmado; nome provável.

## 0x0800E2B0 PartDetail_FadeCursorStep
- Faz: quadro de fade do cursor da tela de detalhe; se [obj+0x3C] é 0 ou 4 faz fade de paleta por passo [obj+0x84]; senão GpuBlend_TickStep; ao fim incrementa contador.
- Confiança: confirmado; nome provável.

## 0x0800E328 PartDetail_CursorLoadStep
- Faz: alterna forma do sprite do cursor (PartDetail_ToggleCursorSpriteShape), Obj_InitSlotAndLoad(1,2,0) e incrementa contador.
- Confiança: confirmado; nome provável.

## 0x0800E350 PartDetail_DispatchLegIconStep
- Faz: escolhe tabela de sub-estado por [obj+0x3E]<3 e índice do ícone de perna (PartDetail_GetCurrentSelectionLegIconIndex); StateMachine_DispatchStep; ao concluir, incrementa contador.
- Confiança: confirmado; nome provável.

## 0x0800E3A0 PartDetail_HandleSelectionConfirm
- Faz: entrada da tela de detalhe: se PartDetail_HandleSelectionChangeInput muda seleção, reseta campos e vai para o estado; B (bit 2) toca sfx 5 e avança; A (bit 8) toca sfx 4 e grava 3/4/5 no byte [+0x43] conforme [+0x3E]; senão nada.
- Confiança: confirmado; nome provável.

## 0x0800E43C PartDetail_PaletteCommitStep
- Faz: quadro com CopyPaletteBuffersToHardware (se modo 0 ou 4) ou SetGpuBlendMode; zera [obj+0x84] e incrementa contador.
- Confiança: confirmado; nome provável.

## 0x0800E488 PartDetail_FadeRowsStep
- Faz: fade de linhas de paleta por passo [obj+0x84]<9 (modo 0 ou 4) ou GpuBlend_TickStep; ao concluir, incrementa contador.
- Confiança: confirmado; nome provável.

## 0x0800E654 PartDetail_ResetCursorSpriteStep
- Faz: limpa atributo do cursor (Slot_ClearAttrMask 1,5), CpuFastSet (8 palavras), PartDetail_SetupSelectionCursorSprite, Obj_ResetStateFields; incrementa contador.
- Confiança: confirmado; nome provável.

## 0x0800E690 PartDetail_AnimSlotDispatchStep
- Faz: quadro com Anim_RunSlotScript(slot,6) e StateMachine_DispatchStep sobre [DAT+0x14]; ao concluir, incrementa contador.
- Confiança: confirmado; nome provável.

## 0x0800E6C8 PartDetail_HandleCursorMoveInput
- Faz: movimento do cursor por cima/baixo (toca sfx 2 e atualiza posição); A (bit 1) com [DAT_0800E768] toca sfx 4 e segue o estado 0x24; B toca sfx 5; sem tecla, nada.
- Confiança: confirmado; nome provável.

## 0x0800E78C PartDetail_DispatchSubstate
- Faz: despachante de sub-estado da tela de detalhe por tabela indexada por [obj+0x44] (modo [obj+0x3E] 0..3). Jump table NÃO recuperada pelo Ghidra ("Could not recover jumptable at 0x0800E7CA/E87E/E940"); é chamada indireta.
- RAM: [obj+0x3E], [obj+0x44].
- Confiança: confirmado (estrutura); destino dos casos não lido (abre JT).

## 0x0800E9F0 PartDetail_SetupCursorAndState
- Faz: limpa atributo (Slot_ClearAttrMask 6,1), CpuFastSet, Obj_ResetStateFields, PartDetail_SetupCursorSpritePair e grava estado 0x13.
- Confiança: confirmado; nome provável.

## 0x0800EA98 PartDetail_HandleAttributeCursorInput
- Faz: entrada do cursor de atributo: move cursor (PartDetail_MoveAttributeCursor, slot modo 4, 0xA8); A toca sfx 4 (ou sfx 6 se contador de inventário zero); B toca sfx 5 e grava estado 2.
- Confiança: confirmado; nome provável.

## 0x0800EB60 PartDetail_TickFadeAndRoute
- Faz: quadro de roteamento por byte de estado [obj+0x4A]: 0 grava 10; 1 snd_fade_request(8) e avança; 2 grava 4.
- Confiança: confirmado; nome provável.

## 0x0800EBE4 PartsGrid_ResetAndSetMode2
- Faz: igual a PartsGrid_ResetAndSetMode1 mas Dispatch_Sub4Mode(2).
- Confiança: confirmado; nome provável.

## 0x08010170 PartsBuild_MarkSlotFlagStep
- Faz: marca bit 0x4000 em [base+0x100] (ou máscara) conforme byte de [base+índice]; incrementa contador. Base = DAT_08010198.
- RAM: IWRAM/bloco via literal (sem endereço fixo confirmado).
- Confiança: confirmado (lógica); significado de negócio é chute.

## 0x080103F4 PartsBuild_InitScreenLoadGfx
- Faz: liga BG0 em REG_DISPCNT (|= 0x100), zera buffers, decompress_and_store, grava três campos e zera [obj+0x3F],[+0x48],[+0x84]; incrementa contador.
- Confiança: confirmado; nome provável.

## 0x08010794 Fight_BotPartSelect_FadeInStep
- Faz: quadro da seleção de peça de bot na batalha: Fight_UpdateBotPartEntryMotion, Obj_StepAccumMoveA, fade de linhas de paleta por passo [obj+0x84]<9; ao concluir, incrementa contador.
- Confiança: confirmado; nome provável.

## 0x080107F0 Fight_BotPartSelect_InitSlotStep
- Faz: quadro com Fight_UpdateBotPartEntryMotion e Obj_InitSlotAndLoad(1,0,0); incrementa contador.
- Confiança: confirmado; nome provável.

## 0x08010860 Fight_BotPartSelect_WaitInputStep
- Faz: espera entrada: se bit 8 de [DAT_0801089C] e Map64_GetValue(0x32)!=0 e campo zero, toca sfx 4 e avança; senão, com A (bit 1) toca sfx 4 e grava 0x20.
- RAM: flags de mapa 0x32 (Map64_*).
- Confiança: confirmado; nome provável.

## 0x080108D0 Fight_BotPartSelect_DispatchStep
- Faz: quadro com StateMachine_DispatchSub; ao retornar não-zero, Obj_InitSlotAndLoad(1,2,0) e incrementa contador.
- Confiança: confirmado; nome provável.

## 0x080108FC Fight_BotPartSelect_FxStep
- Faz: StateMachine_DispatchStep sobre [DAT_08010928+0x50]; ao concluir, Fx_InitRecord8(1,0x16,7) e incrementa contador.
- Confiança: confirmado; nome provável.

## 0x08010930 Fight_BotPartSelect_TableDispatchStep
- Faz: StateTable_DispatchByIndex; se o resultado tem bits altos zero, Obj_ResetStateFields e avança; senão grava 0x20.
- Confiança: confirmado; nome provável.

## 0x08010968 Fight_BotPartSelect_FadeOutStep
- Faz: StateMachine_DispatchStep sobre [DAT_08010998+0x54]; ao concluir, snd_fade_request(8), Map64_SubRemoveIfZero(0x32,1) e incrementa contador.
- RAM: flag de mapa 0x32.
- Confiança: confirmado; nome provável.

## 0x080109A0 Fight_BotPartSelect_ClearPaletteStep
- Faz: quadro com Fight_UpdateBotPartEntryMotion, CopyPaletteBuffersToHardware, zera [obj+0x84] e incrementa contador.
- Confiança: confirmado; nome provável.

## 0x080109CC Fight_BotPartSelect_FadeRowsStep
- Faz: fade de linhas de paleta por passo [obj+0x84]<9 (cor A e B); ao concluir, incrementa contador.
- Confiança: confirmado; nome provável.
