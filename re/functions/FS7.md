# FS7: 38 endereços da lista FS7 (criados pelo MedabotsSync)

Lote feito no Ghidra (renames aplicados: 28 funções). 29 são código Thumb decompilado; 9 não têm instruções no
Ghidra (halt_baddata) e ficaram como `kind=label` no CSV. Grupo principal: 19 passos por quadro numa tabela de
ponteiros Thumb em 0x083AFCB8..0x083AFDA4 (cada entrada é um passo de animação/tela). Chamador direto: nenhum
(são chamados por ponteiro). A tela que usa essa tabela não foi identificada.

Convenção de RAM nos passos da tabela: `+0x84` = byte de estado do slot, `+0x86` = contador u16 de quadros,
`+0x9C` = flag de modo, `+0x122`/`+0x1A2` = índice do script. Os endereços exatos de cada ponteiro (DAT_*) são
do objeto de tela, não confirmados.

## 0x0803E03C Anim_RunSlotScriptThenSfx54
- O que faz: a cada quadro conta, roda um script de slot (Anim_RunSlotScript); quando o byte +0x122 vale -1, toca SFX 0x54 e avança o passo.
- RAM: +0xC6 (u16) recebe valor fixo; contador de passo incrementado; sem escrita em +0x86.
- Ponteiro: 0x083AFD54. Chamador direto: nenhum.
- Confiança: provável (lógica lida; significado da tela chute).

## 0x0803E098 Anim_TweenUntilDoneThenReset
- O que faz: anda o tween até o alvo (Tween_StepUntilTarget); ao chegar, dispara o som de barra cheia e reseta a direção do tween.
- RAM: incrementa o contador de passo; +0x86 pelo Counter_IncIfFlagBit0.
- Ponteiro: 0x083AFD58.
- Confiança: provável.

## 0x0803E0C0 Anim_TweenClampThenSfx2A
- O que faz: passo de tween com fixação no alvo; ao terminar toca SFX 0x2A e zera +0x86.
- RAM: u16 +0x86 = 0 (no objeto); incrementa contador de passo.
- Ponteiro: 0x083AFD5C.
- Confiança: provável.

## 0x0803E0F0 Anim_DrawSegmentUntil120
- O que faz: desenha um segmento de 2 tiles (Gauge_DrawTwoTileSegment) e conta até 0x78 (120 quadros), depois avança o passo.
- RAM: +0x86 (u16) de 0 a 0x78; contador de passo.
- Ponteiro: 0x083AFD60.
- Confiança: provável (contagem confirmada; nome do desenho chute).

## 0x0803E120 Anim_DrawSegmentThenJumpPtr
- O que faz: desenha segmento, incrementa +0x86, escolhe um destino pelo byte de script (== 4 soma no contador, senão grava 0x48) e chama um ponteiro de salto não recuperado pelo Ghidra.
- RAM: +0x86 (u16); escreve um u32 no destino escolhido; o salto indireto não foi resolvido.
- Ponteiro: 0x083AFD64. Pendente: jump table em 0x0803E168 (Ghidra: "too many branches").
- Confiança: chute no que o salto faz; confirmado o resto.

## 0x0803E170 Anim_DrawSegmentThenLoadSlot
- O que faz: desenha segmento, incrementa +0x86, carrega um slot (Obj_InitSlotAndLoad(1,0,0)) e avança o passo.
- RAM: +0x86 (u16); contador de passo.
- Ponteiro: 0x083AFD68.
- Confiança: provável.

## 0x0803E1A4 Anim_DrawSegmentThenDispatchLoad
- O que faz: desenha segmento e despacha uma máquina de estados (StateMachine_DispatchStep) escolhida por +0x9C; ao concluir carrega o slot 1.
- RAM: +0x86 (u16); ponteiro de estado lido de +0x10 ou +0x14; contador de passo.
- Ponteiro: 0x083AFD6C.
- Confiança: provável.

## 0x0803E1FC Anim_DrawSegmentThenDispatchStep
- O que faz: desenha segmento e despacha a máquina de estados em +4; quando ela termina, avança o passo.
- RAM: +0x86 (u16); contador de passo.
- Ponteiro: 0x083AFD70.
- Confiança: provável.

## 0x0803E234 Anim_DrawSegmentUntilOverlayReached
- O que faz: desenha segmento e move o alvo do overlay (Overlay_SetTargetAndCheckReached); ao chegar, zera +0x84 e avança.
- RAM: +0x86 (u16); +0x84 = 0.
- Ponteiro: 0x083AFD74.
- Confiança: provável.

## 0x0803E270 Anim_DrawCycleStateZeroToFour
- O que faz: desenha segmento e faz +0x84 ciclar 0..4; ao passar de 4 zera e avança o passo.
- RAM: +0x84 (byte) 0..4; +0x86 (u16).
- Ponteiro: 0x083AFD78.
- Confiança: provável.

## 0x0803E2AC Anim_DrawFadeSfx8Step
- O que faz: desenha segmento e pede fade de som (snd_fade_request(8)); avança o passo.
- RAM: +0x86 (u16); contador de passo.
- Ponteiro: 0x083AFD7C.
- Confiança: provável.

## 0x0803E2DC Anim_DrawFlushPaletteThenReset
- O que faz: desenha segmento, copia os buffers de paleta para o hardware (CopyPaletteBuffersToHardware), zera +0x84 e avança.
- RAM: +0x86 (u16); +0x84 = 0; escrita de paleta (CGRAM via buffer).
- Ponteiro: 0x083AFD80.
- Confiança: provável.

## 0x0803E314 Anim_DrawFadeRowsStep
- O que faz: desenha segmento e funde linhas de paleta para as cores A e B (Palette_FadeRowsToColorA/B) em 9 passos (+0x84 < 9); depois avança.
- RAM: +0x84 (byte) 0..8; +0x86 (u16); paleta.
- Ponteiro: 0x083AFD84.
- Confiança: provável.

## 0x0803E374 Anim_PickSlotActivateEntryCopy
- O que faz: sorteia um slot e desenha o cursor (Overlay_PickRandomSlotAndDrawCursor); se +0x9C == 1 soma 1 ao contador de itens (Inv_AddCountClamp99), senão ativa a entrada casada (Tinpet_ActivateMatchingEntry); copia 15 bytes (Mem_CopyBytes_Dup).
- RAM: +0x86 (u16); inventário (contador de peça); cópia de 0xF bytes para destino de tabela.
- Ponteiro: 0x083AFD98.
- Confiança: provável (chamadas lidas; semântica da escolha chute).

## 0x0803E434 Anim_DrawSegmentThenDispatchLoadAlt
- O que faz: igual a Anim_DrawSegmentThenDispatchLoad, mas usa os campos +0x18 e +0x1C; ao concluir carrega o slot 1.
- RAM: +0x86 (u16); contador de passo.
- Ponteiro: 0x083AFDA0.
- Confiança: provável.

## 0x0803E48C Anim_DrawSetValue3E
- O que faz: desenha segmento, incrementa +0x86 e grava o valor 0x3E no byte apontado por um ponteiro.
- RAM: +0x86 (u16); byte 0x3E em destino fixo.
- Ponteiro: 0x083AFDA4.
- Confiança: provável.

## 0x0803E4D8 Anim_GpuBlendTickThenCount
- O que faz: avança o blend de GPU (GpuBlend_TickStep); quando termina, conta o passo.
- RAM: contador de passo.
- Ponteiro: 0x083AFCBC.
- Confiança: provável.

## 0x0803E618 Anim_InitSlotAndLoadCount
- O que faz: carrega o slot 1 (Obj_InitSlotAndLoad(1,0,0)) e conta o passo.
- RAM: contador de passo.
- Ponteiro: 0x083AFCC8.
- Confiança: provável.

## 0x0803E674 Boot_ResetSystemWrapper
- O que faz: só chama Boot_ResetSystemAndState (0x0803EF20), que faz o reset de hardware e estado.
- RAM: a da rotina chamada.
- Ponteiro: 0x083AFCD8.
- Confiança: confirmado (chamada direta).

## 0x08040B5C Gfx_CpuSetCopy52Blocks
- O que faz: copia 52 blocos de 0x20 palavras, passo 0x40, com bios_cpuset; depois incrementa um byte de estado.
- RAM: destino de tiles/VRAM (endereço vem de literal); byte +0x1B do objeto.
- Ponteiro: 0x083B2988 (é a entrada da tabela gMessageDispatch12Table, handler do estado 12 do motor de mensagem).
- Confiança: provável (mecânica confirmada; nome de destino chute).

## 0x08042238 Tinpet_CreateRecord (já nomeada; kind=func_thumb)
- O que faz: acha o primeiro registro livre entre 9 (stride 8) e grava: estado 1, bit do parâmetro, 0x1E e quatro bytes 0x78+bit.
- RAM: tabela de 9 registros de 8 bytes (endereço no literal 0x08042264).
- Chamador: Tinpet_CreateRecord_Wrapper (0x08047E42) e PartsBuild_UpdateStatusPanel (0x0800C7D6). Divergência: mtbe.csv marca como label.
- Confiança: confirmado (lido no código).

## 0x08043AE0 Unk_08043AE0 (kind=label)
- O que faz: NÃO decodificado no Ghidra (halt_baddata, sem instruções). Os bytes em 0x08043AE0 são um prólogo Thumb (push {r4,lr}); provavelmente é código.
- RAM: desconhecida.
- Ponteiro: struct em 0x08043AD8 (fn 0x08043AE0, depois ponteiro RAM 0x03000B98).
- Confiança: chute no que faz; precisa re-disassemble no Ghidra.

## 0x08043B58 Unk_08043B58 (kind=label)
- O que faz: NÃO decodificado no Ghidra (halt_baddata). Bytes: push {lr}; BL. Provavelmente código pequeno.
- RAM: desconhecida.
- Chamador: BL em 0x0804D01A (o sync cita essa origem; o BL em 0x0804D01A não foi conferido no Ghidra).
- Confiança: chute; precisa re-disassemble.

## 0x08043E48 Obj_StepAccumMoveAWrapper
- O que faz: só chama Obj_StepAccumMoveA (0x08043CD4).
- RAM: campos do objeto A (+0x3B..+0x40), a da rotina chamada.
- Ponteiro: 0x0801E1F0 (entrada no lugar de 0x08043E48).
- Confiança: confirmado (chamada direta).

## 0x08044100 Obj_StepAccumMoveBThenSync
- O que faz: chama Obj_StepAccumMoveB (0x08043E78) e depois FUN_08043FEC com o byte +0x38 do objeto.
- RAM: campos do objeto B (+0x74..+0x7B); +0x38.
- Ponteiro: 0x08012B38.
- Confiança: provável (FUN_08043FEC não lida).

## 0x08045E6C PartPreviewPiece (já nomeada; kind=func_thumb)
- O que faz: monta a prévia de uma peça: acha o seletor de imagem ((entrada>>6)+variante)&0x7F, copia tiles com bios_cpuset e monta o OAM (BuildCompositePreviewOamTemplate); chama a si mesma para as partes +0x0F e +0x10.
- RAM: OAM (buffer), paleta/tiles (destino por literal), descritor de peça.
- Chamador: LoadPartLargeDisplaySpriteFromAppearanceTable (0x08046B6C), BuildFourPartLargePreviewComposite. Divergência: mtbe.csv marca como label; o código é função.
- Confiança: confirmado.

## 0x08047C00 FieldMenu_ClearTwoSlotPairs (renomeada)
- O que faz: chama Overworld_NoOpStateHandler (0x08047E4C) e zera dois pares de u16.
- RAM: dois pares de u16 (ponteiros em 0x08047C1C e 0x08047C20).
- Chamador: FieldMenu_Sub0D_Close (0x08057D04) antes de 0x0804A210.
- Confiança: provável (escrita confirmada; nome do menu da FM3).

## 0x08047C24 Obj_TwoStageInitThenDispatch (renomeada)
- O que faz: máquina de 2 estados: estado 0 inicializa o slot (Obj_InitSlotAndLoad ou Obj_ResetStateFields); estado 1 despacha StateMachine_DispatchStep e, ao concluir, volta ao 0 e incrementa o ciclo.
- RAM: bytes de estado em 0x08047C44+0/+1; contador em 0x08047C64.
- Chamador: FUN_0804A734 (0x0804A748), Flow_WaitObjectThenCount (0x08057CCA), eventos 0x36..0x3D (Event_Op36_Cont_GetItem, EventOpcode37..3D).
- Confiança: provável (lógica confirmada).

## 0x08048000 Fx_SetupLayersAndInitRecord8 (renomeada)
- O que faz: grava 0x500 e 0x1F60 em registradores de exibição, chama Fx_InitRecord8(arg,0x16,7) e zera o byte +100 de um bloco.
- RAM: registradores de display (IO); bloco +100.
- Chamador: BL em 0x0804E810.
- Confiança: provável (escrita de IO lida; nome chute).

## 0x08049CC8 Obj_LoadPairFromRecord (renomeada)
- O que faz: copia 6 bits de dois bytes do registro para dois destinos; um deles com shift de 8.
- RAM: dois u16/u8 de destino (endereços em literal).
- Ponteiro: 0x080499B4 (tabela de chamadas).
- Confiança: provável.

## 0x080497E0 Overworld_SetTransitionStepAndDispatch (renomeada)
- O que faz: grava 3 argumentos num bloco de transição, marca estado +2=1, mascara um u16 e despacha por jump table em +1 (0..6); acima de 6 finaliza.
- RAM: bloco de transição (DAT_08049868), palavra de estado em 0x08049A68.
- Chamador: Map_EnterSceneSetup (0x08002FC6), Overworld_ReloadMapSceneAndAdvanceGameState (0x08010AA4), Overworld_StartScreenTransitionEffect (0x08049D7A), Overworld_ResetAndStartScreenTransitionEffect (0x08049DF4). Divergência: a nota do sync dizia "Obj_SetStateAndDispatchJT"; o nome Overworld_ segue os chamadores.
- Confiança: provável (despacho confirmado; papel chute).

## 0x08053360 Unk_08053360 (kind=label)
- O que faz: NÃO decodificado (halt_baddata). Bytes: push {r4,lr}; ldr; bl; pop {r4}; pop {r1}; bx r1 (trampolim).
- Chamador: BL em 0x08056BE0.
- Confiança: chute; precisa re-disassemble.

## 0x08053380 Unk_08053380 (kind=label)
- O que faz: NÃO decodificado (halt_baddata). Bytes: push {lr}; ldr; bl, com literal 0x3F800000 (float 1.0) no fim.
- Chamador: BL em 0x08056E90.
- Confiança: chute; precisa re-disassemble.

## 0x080533A4 Unk_080533A4 (kind=label)
- O que faz: NÃO decodificado (halt_baddata). Bytes: push {r4,lr}; mov; ldr; bl.
- Chamador: BL em 0x08056BB2.
- Confiança: chute.

## 0x080534C4 Unk_080534C4 (kind=label)
- O que faz: NÃO decodificado (halt_baddata). Bytes: push {r4-r7,lr} com frame grande (sub sp).
- Chamador: BL em 0x080541FC.
- Confiança: chute.

## 0x080535C8 Unk_080535C8 (kind=label)
- O que faz: NÃO decodificado (halt_baddata). Bytes: push {r4-r7,lr} com frame.
- Chamador: BL em 0x080534E6.
- Confiança: chute.

## 0x080537BC Unk_080537BC (kind=label)
- O que faz: NÃO decodificado (halt_baddata). Bytes: push {r4,lr}; ldr; grava 4 campos de 16 bits; pop; bx lr.
- Chamador: BL em 0x080534FC.
- Confiança: chute.

## 0x080537D8 Unk_080537D8 (kind=label)
- O que faz: NÃO decodificado (halt_baddata). Bytes: push {lr}; bl; pop; bx lr (trampolim curto).
- Chamador: BL em 0x08053466.
- Confiança: chute.

## Pendências
- Os 9 `Unk_` (kind=label) precisam de re-disassemble no Ghidra antes de qualquer nome.
- Anim_DrawSegmentThenJumpPtr: jump table em 0x0803E168 não recuperada.
- Divergência de chamador: o sync cita BL de 0x080563C6 para 0x080497E0, mas o Ghidra só lista os 4 chamadores acima.
- Tabela 0x083AFCB0..0x083AFDB8: tela não identificada; entradas 0x08000840 repetidas (vazias).
