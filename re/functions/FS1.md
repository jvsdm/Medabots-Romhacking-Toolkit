# FS1: 38 candidatos da lista FS1_lista.txt (funções criadas pelo MedabotsSync)

Todos os 38 endereços já são função no Ghidra (FUN_xxxxxxxx antes do rename). Renomeados no Ghidra agora (37);
0x08001278 é dado (kind=label, não renomeado). Nenhum xref do Ghidra aponta para eles: o Ghidra não lista ponteiro
de tabela como xref. "Quem chama" = a tabela de ponteiros indicada (lida na ROM). Pesquisa de BL na ROM: só 0x08004798
teria um BL, e é um falso positivo (fora de código); nenhum dos outros 37 tem BL para si.

Subgrupos (pelas tabelas de ponteiros):
- Tabela 0x083AC454+4*n (despachante FUN_08003374, citado em FM4, provável): 0x080034A4..0x080038F0, 0x08003D4C..0x08004028, 0x080043A8..0x08004644 (tela de peças/Tinpet, PS.md).
- Tabela 0x083AC520/0x083AC540: 0x08004764..0x080048B0 (BattlePreview).
- Tabela 0x083AC560 (TP2.md, escolha de até 3 bots): 0x080049C8, 0x080049EC, 0x08004AF8, 0x08004C58, 0x08004CA4.
- Tabela 0x0807E874: 0x0800099C e 0x080009A8 (callbacks de trilha; tabela em si não identificada).

## 0x0800099C Sound_TickTrackAStub
- O que faz: trampolim; chama `snd_track_tick_a` (0x08074090) e devolve.
- RAM: nenhuma própria. Ponteiro em 0x0807E874 (tabela de callbacks, não identificada).
- Confiança: confirmado (código); quem chama a tabela: provável.

## 0x080009A8 Sound_TickTrackBStub
- O que faz: trampolim; chama `snd_track_tick_b` (0x080740D4) e devolve.
- RAM: nenhuma própria. Ponteiro em 0x0807E878.
- Confiança: confirmado (código); tabela: provável.

## 0x08001278 (kind=label, sem nome)
- O que faz: não é função. Ghidra decodifica como instrução inválida (halt_baddata); é dado/pool.
- RAM: nenhuma. Valor 0x08001279 em 0x08001274 (ponteiro de tabela não identificada).
- Confiança: confirmado que não é código (Bad instruction); tabela de origem: chute.

## 0x080034A4 PartScreen_FadeInPalettesStep
- O que faz: avança um passo de tela: Pair_TickDownCounterIfFlag; 9 passos de fade A/B das paletas (0x20-4*n); ao fim chama FUN_0800393C e incrementa o contador de sub-estado.
- RAM: contador de sub-estado via ponteiro em literal (não resolvido); +0x84 do bloco de tela.
- Confiança: confirmado (código); entrada da tabela 0x083AC458: provável.

## 0x08003504 PartScreen_HandleCursorInputStep
- O que faz: passo de entrada: anima slot 0; esquerda/direita muda o índice (+0x8F, 0..2) com SFX 2 e redesenha; botão de bit 0x02 e 0x01 tocam SFX e avançam o sub-estado (sub=8 no 0x01).
- RAM: bloco de tela (+0x8F, +0x84); posições de OBJ calculadas (+2, +4).
- Confiança: provável (estrutura confirmada; mapa de bits de botão e SFX não lidos).

## 0x08003620 PartScreen_ClearAndFadeOutStep
- O que faz: limpa 1 entrada de OBJ, copia buffers de paleta para hardware, zera +0x84, pede fade (snd_fade_request 8) e avança o sub-estado.
- RAM: OAM (ClearObjectAttributeEntries), gPaletteShadowBuffer.
- Confiança: confirmado (código).

## 0x08003654 PartScreen_FadeToColorStep
- O que faz: 9 passos de fade A/B com passo (n<<2); ao fim avança o sub-estado.
- RAM: bloco de tela +0x84.
- Confiança: confirmado (código).

## 0x080036A8 PartScreen_StartSongStep
- O que faz: pede música id 6 (snd_song_request), marca flag = 1 e carrega timer 0x10.
- RAM: flag e timer por ponteiro.
- Confiança: confirmado (código).

## 0x080036C8 PartScreen_WaitBlendAnimStep
- O que faz: roda Anim_RunSlotScript no slot indicado e espera GpuBlend_TickStep terminar; então avança o sub-estado.
- RAM: registro de slot por ponteiro.
- Confiança: confirmado (código).

## 0x080038CC PartScreen_WaitBlendThenResetStep
- O que faz: espera GpuBlend_TickStep; ao fim chama FUN_0800393C e avança o sub-estado.
- RAM: contador de sub-estado.
- Confiança: confirmado (código).

## 0x080038F0 PartScreen_SetNextStateStep
- O que faz: grava 2 no estado da tela (passo de transição).
- RAM: byte de estado por ponteiro.
- Confiança: confirmado (código).

## 0x08003D4C PartSlots_UpdateAndFadeStep
- O que faz: atualiza os 4 slots de parte (ObjectSlot_UpdatePosition 0..3), monta a camada grande de preview, aplica máscara, checa sprites; faz fade (flag +0x56==1) ou espera blend; ao fim avança.
- RAM: bloco de slots, paletas, OAM.
- Confiança: confirmado (código).
- Divergência: FS.md listava este endereço como "sem função no Ghidra"; hoje já é função (FUN_08003d4c).

## 0x08003DE0 PartSlots_HandleInputStep
- O que faz: entrada da tela de slots: troca de slot, direção, e pelos bits de botão: bit 0x02 confirma (Parts_HandleConfirmInput), 0x01 encaixa parte (Parts_AttachSlotPart), 0x08 remove slot (Parts_CheckSlotRemoval).
- RAM: entrada de botões por ponteiro (DAT_08003E30).
- Confiança: confirmado (chamadas); mapeamento de bit a botão: provável.

## 0x08003E54 PartSlots_EnterBlendStep
- O que faz: atualiza slots, SetGpuBlendMode(6,0x3F,0x20), zera +0x84 e avança.
- RAM: blend registers, bloco de tela.
- Confiança: confirmado (código).

## 0x08003EA0 PartSlots_WaitBlendStep
- O que faz: atualiza slots e espera GpuBlend_TickStep; ao fim avança.
- RAM: contador por ponteiro.
- Confiança: confirmado (código).

## 0x08003F28 PartSlots_CommitPalettesStep
- O que faz: atualiza slots, copia buffers de paleta para hardware, zera +0x84 e avança.
- RAM: gPaletteShadowBuffer.
- Confiança: confirmado (código).

## 0x08003F74 PartSlots_FadeInThenTimerStep
- O que faz: atualiza slots; 9 passos de fade A/B; ao fim grava timer 0x10.
- RAM: bloco de tela +0x84; timer por ponteiro.
- Confiança: confirmado (código).

## 0x08003FE8 PartSlots_LoadSlotObjectStep
- O que faz: atualiza slots e chama Obj_InitSlotAndLoad(1,2,0); avança.
- RAM: slot de objeto.
- Confiança: confirmado (código).

## 0x08004028 PartSlots_DispatchSubStateStep
- O que faz: atualiza slots e chama StateMachine_DispatchStep (+0x58); quando retorna != 0 grava estado 2.
- RAM: contexto +0x58 (ponteiro).
- Confiança: confirmado (código).

## 0x080043A8 PartEditor_FadeInStep
- O que faz: cria sprite de ícone, alterna flag 0x4000, despacha handler do slot, valida conjunto e atualiza painel; faz fade (+0x56==0) e marca +0x56 = 1 ao fim.
- RAM: contexto do editor (+0x56, +0x84); OAM.
- Confiança: confirmado (código).

## 0x0800442C PartEditor_HandleInputStep
- O que faz: tratador principal do editor de peças: entrada de slot, ciclo de valor e de cada slot (1..4), loop de seleção; SFX conforme ação; redesenha painel; despacha ação do slot (1 = equipar, 2 = som de erro).
- RAM: contexto do editor (+0x5E alterna); ponteiros para estado de seleção.
- Confiança: provável (fluxo confirmado; semântica de cada ciclo: provável pelos nomes Parts_*).

## 0x080044F8 PartEditor_FinishPaletteStep
- O que faz: atualiza painel; se +0x50 == 0 copia paletas e zera +0x84; senão aplica blend; avança.
- RAM: contexto +0x50, +0x84.
- Confiança: confirmado (código).

## 0x0800454C PartEditor_FadeOutStep
- O que faz: atualiza painel; fade de 9 passos (se +0x50 == 0) ou GpuBlend; avança ao fim.
- RAM: contexto +0x50, +0x84.
- Confiança: confirmado (código).

## 0x080045C4 PartEditor_BuildEquipTableStep
- O que faz: cria sprite de ícone, chama Parts_BuildEquipTable e zera o estado.
- RAM: tabela de equipadas.
- Confiança: confirmado (código).

## 0x080045DC PartEditor_BlendOutStep
- O que faz: atualiza painel; SetGpuBlendMode(6,0x3F,0x20); avança.
- RAM: registros de blend.
- Confiança: confirmado (código).

## 0x08004610 PartEditor_WaitBlendStep
- O que faz: atualiza painel; espera GpuBlend_TickStep; avança ao fim.
- RAM: contador por ponteiro.
- Confiança: confirmado (código).

## 0x08004644 PartEditor_DispatchSubmodeStep
- O que faz: limpa OAM (0x80 entradas), zera +0x96 e despacha por jump table pelo byte +0x5A (se < 5).
- RAM: contexto +0x5A, +0x96.
- Confiança: confirmado (código); a jump table (ponteiro em DAT_08004678) não foi recuperada pelo Ghidra (aberto).

## 0x08004764 BattlePreview_InitSlotSpritesStep
- O que faz: cria sprites de slot de preview (9), monta camada 5, limpa máscara 4 e avança.
- RAM: OAM; contexto +0x5C.
- Confiança: confirmado (código; nomes dos auxiliares vêm do NM2/R0).

## 0x08004798 BattlePreview_WaitTimerStep
- O que faz: atualiza camada e sprites; quando o timer (+0x5C) passa de 0x3C, vai para sub 0x30 (se +0x50 == 1) ou aplica paleta e avança.
- RAM: contexto +0x50, +0x5C.
- Confiança: confirmado (código).
- Divergência: FS1_lista diz "BL de 0x08003D6E"; a varredura da ROM não acha esse BL. Só existe ponteiro de tabela (0x083AC524).

## 0x080047EC BattlePreview_FadeOutStep
- O que faz: atualiza camada e slots; fade de 0x21 passos (Palette_FadeRowsToColorA/B); avança ao fim.
- RAM: paleta e contexto +0x84.
- Confiança: confirmado (código).

## 0x08004840 BattlePreview_ClearStateStep
- O que faz: limpa OAM (0x80), zera campos e grava o estado 7.
- RAM: campos de estado por ponteiro.
- Confiança: confirmado (código).

## 0x08004874 BattlePreview_FadeSoundStep
- O que faz: atualiza camada e slots; ao timer > 0x3C pede fade de som (snd_fade_request 8), SetGpuBlendMode(6,0x3F,8) e avança.
- RAM: timer +0x5C.
- Confiança: confirmado (código).

## 0x080048B0 BattlePreview_WaitBlendStep
- O que faz: atualiza camada e slots; espera GpuBlend_TickStep; avança.
- RAM: contador por ponteiro.
- Confiança: confirmado (código).

## 0x080049C8 TeamPick_LoadSlotObjectStep
- O que faz: atualiza camada; Obj_InitSlotAndLoad(1,0,0); avança.
- RAM: slot de objeto.
- Confiança: provável (tabela 0x083AC560 = escolha de bots, TP2.md; a associação ainda não foi testada no emulador).

## 0x080049EC TeamPick_DispatchSubStateStep
- O que faz: atualiza camada; StateMachine_DispatchStep (+0x2C); quando != 0 avança.
- RAM: contexto +0x2C.
- Confiança: provável (tabela 0x083AC560, TP2.md).

## 0x08004AF8 TeamPick_FillBotSlotsStep
- O que faz: a cada 4 quadros grava um byte de bot (até 0x48 entradas) no buffer de bots e alterna a flag; ao completar as duas linhas grava estado 0x3A.
- RAM: buffer de bots por ponteiro (offsets +0x51..+0x54, +0x86, +0x1A8); contador +0x86.
- Confiança: provável (lógica lida; RAM por literal não resolvido).

## 0x08004C58 TeamPick_ResetCounterStep
- O que faz: atualiza camada; ao contador 0x10 zera os bytes +0x51..+0x54 e avança o estado; senão incrementa o contador.
- RAM: contexto +0x51..+0x54, contador +0x86.
- Confiança: confirmado (código); tabela 0x083AC560: provável.

## 0x08004CA4 TeamPick_CompareSelectionStep
- O que faz: avança 2 entradas de seleção; ao cabo de 2 quadros compara os valores e decide por jump table (ponteiro DAT_08004DAC não recuperado) ou pelo contador de sucesso (+1), ou grava 0x32.
- RAM: contexto +0x51..+0x52, +0x1A6..+0x1AC.
- Confiança: provável (lógica lida); a jump table não foi recuperada (aberto).

Pendências desta rodada: jump tables de 0x08004644 (DAT_08004678) e 0x08004CA4 (DAT_08004DAC); a tela que usa 0x083AC454 (FM4 aponta menu 2, não confirmado com emulador); 0x0807E874 (tabela de callbacks de trilha).
