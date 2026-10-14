# FS9: 38 funções de FS9_lista.txt (Metabee EU)

Origem: `FS9_lista.txt` (endereço TAB origem: `ptr em <tabela>` ou `BL de <endereço>`). Todos os 38 já são função no Ghidra
(`get_function_by_address`, corpo inteiro, nenhum miolo). Decompile feito em todos.
Xrefs: `get_xrefs_to` e `get_function_xrefs` voltaram vazios para os 38 (o Ghidra não indexa chamadas por tabela).
Quem aponta é a própria origem da lista (tabela de ponteiros ou o BL de 0x08069212 em 0x0806AA10).
Renomeados no Ghidra: 25 (confirmado ou provável). Os 13 chutes ficaram como FUN_. Nada salvo.
Tudo é Thumb. Não há dado nem miolo aqui. Todas as seções abaixo são código.
Padrão: a maioria são callbacks de frame (tick + `StateMachine_DispatchSub`/`PointRing_StepFrame`, fade de paleta em 9 passos,
mudança de estado em `gMainState`/`MainState_RequestModeTransition`). Os `Battle_Bot*` operam sobre o registro do bot de 0x40 bytes
(contador em +0x1A, ponteiro de tabela de salto não recuperado pelo Ghidra). Blocos: 0x08060AF4..0x08061998 (telas de título/
inicialização), 0x08062754..0x080655FC (grade de peças), 0x08067870..0x08067B28 (texto/rolagem com PointRing), 0x0806AA10..0x0806BCC4
(batalha), 0x0806DBFC..0x0806E074 (efeitos de batalha).

Convenção de RAM nas seções: REG_DISPCNT 0x04000000, REG_BLDCNT/BLDALPHA, REG_BG2CNT/BG3CNT; VRAM/OAM via `bios_cpufastset`/
`bios_cpuset`; demais endereços são ponteiros `DAT_...` (não resolvidos aqui).

## 0x08060AF4 Screen_TickStateMachineFrame
- O que faz: tick de quadro: anima posições de sprite e carrega frame, roda `StateMachine_DispatchSub`; ao terminar, copia buffers de paleta e incrementa contador de estado.
- RAM: ponteiro de estado +0x84 (zerado), contador de estado (DAT_08060b24). Chamada pela tabela 0x0848384C.
- Confiança: provável (padrão de tick, nome do subsistema não confirmado).

## 0x08060B28 Screen_FadeOutAndTransition
- O que faz: fade de paleta em 9 passos (Palette_FadeRowsToColorA/B); no fim, mascara REG_DISPCNT, limpa com cpufastset e pede transição de modo 6 ou 5 (escolhida por flag +0x48 do objeto).
- RAM: REG_DISPCNT; +0x84 (passo do fade); objeto de estado em DAT_08060bd0.
- Confiança: provável. Tabela 0x08483850.

## 0x08060DD4 Screen_InitLoadLZ77TilesAndMaps
- O que faz: inicialização de tela: zera VRAM e OAM (bios_cpufastset, ClearObjectAttributeEntries), define REG_DISPCNT=0x60, REG_BLDCNT/BLDALPHA, carrega blocos LZ77 de tiles e descompacta mapas (Asset_UnpackOrCopyBlock, decompress_and_store).
- RAM: REG_DISPCNT, REG_BLDCNT, REG_BLDALPHA; VRAM 0x06004000 (cpuset); ponteiros de bloco em DAT_08060f..
- Confiança: provável. Tabela 0x080AFB98.

## 0x0806113C Screen_EnableBgLayersAndClearVram
- O que faz: liga as camadas REG_DISPCNT |= 0x200|0x400|0x800|0x1000 (BG1..BG4), limpa blocos de VRAM com cpufastset e avança o estado.
- RAM: REG_DISPCNT (0x04000000); objeto de estado +0x84; contador DAT_080611e4.
- Confiança: confirmado (bits de DISPCNT e as chamadas lidas no decompile). Tabela 0x084838AC.

## 0x08061440 Screen_TickStateMachineStep
- O que faz: `StateMachine_DispatchSub`; ao terminar, zera +0x84 do objeto e incrementa o contador de estado. Versão sem paleta da 0x08060AF4.
- RAM: objeto em DAT_08061460+0x84; contador DAT_08061464.
- Confiança: provável. Tabela 0x084838BC.

## 0x080617E0 FUN_080617E0 (chute, não renomeada)
- O que faz: avança contador de 16 bits em +2 do objeto (passo DAT_08061814); rampa de 4 em 4 do byte em +0x208 até 0x40 (escrita em DAT_08061818+0x208 e posição em +0x...); se o byte baixo do contador zera, reseta objeto (Obj_ResetStateFields) e pede estado 4; senão grava 0x10-contador em DAT_08061840.
- RAM: objeto em DAT_08061810 (+2); DAT_08061818+0x208; DAT_08061840, DAT_0806185C.
- Confiança: chute (efeito e tela não confirmados). Tabela 0x084838D0.

## 0x08061998 Screen_ClearPairsAndAdvance
- O que faz: `Clear4PairsHalfwords` e incrementa contador de estado. Não tem lógica própria.
- RAM: contador DAT_080619ac.
- Confiança: provável (trivial). Tabela 0x080AFBA4.

## 0x08062754 Screen_TickThenPickNextState
- O que faz: `StateMachine_DispatchSub`; ao terminar, escolhe o próximo estado: se flag +0x2A é zero, limpa máscaras de atributo de 3 slots (0x61/7, 0x0D/0x34, 0x45/0x1C) e vai para 5; senão vai para 4.
- RAM: objeto em DAT_08062788+0x2A; Slot_ClearAttrMask nos slots de OAM.
- Confiança: provável. Tabela 0x08483940.

## 0x0806280C Screen_SetupNumberDisplay
- O que faz: destaca opção 8 (Screen_HighlightOption4), limpa OAM, reescreve 4 entradas de atributo de 0x300 em diante com máscara, define REG_DISPCNT (0x100..0x1000), posiciona sprite (+0x1C0/+0x1C2/+0x1C4/+0x1C6), formata número decimal (Num_FormatDecimalToGlyphs) e monta o texto em sprites (Text_LayoutGlyphSprites 0x15,1,7,0xF).
- RAM: REG_DISPCNT; OAM/atributos em DAT_08062938+0x1C0..; buffer de glifos em PTR_DAT_08062964.
- Confiança: provável (texto numérico confirmado pelas chamadas). Tabela 0x080AFBA0.

## 0x080646C4 Screen_InitLoadBgAndObjTimer
- O que faz: limpa OAM e pares, grava REG_DISPCNT, REG_BG2CNT, REG_BG3CNT, descompacta tiles/mapas (Asset_UnpackOrCopyBlock, cpuset) e cria um temporizador de objeto (Obj_InitTimerRecord, 0x10 quadros); incrementa estado.
- RAM: REG_DISPCNT, REG_BG2CNT (0x0400000C), REG_BG3CNT (0x0400000E); VRAM 0x05000000 (cpufastset de 0x48 palavras); objeto de timer em DAT_08064878.
- Confiança: provável. Tabela 0x080AFC48.

## 0x08064A08 FUN_08064A08 (chute, não renomeada)
- O que faz: varre 0x1D entradas de 0x40 bytes; para cada uma com bit 7 ligado no byte 0, acumula bit (1 << byte+1) numa máscara; zera três bytes do objeto e grava a máscara em +0x1C se o byte +0x7A for diferente de zero; incrementa contador. Parece montar a máscara das peças presentes.
- RAM: registros de 0x40 bytes a partir de DAT_08064a7c; objeto em DAT_08064a74 (+0x1C, +0x48..+0x4B, +0x7A).
- Confiança: chute (uso da máscara em peças é palpite; a estrutura do loop é confirmada). Tabela 0x080AFC58.

## 0x08065074 PartGrid_InitSlotsAndDraw
- O que faz: liga camadas de DISPCNT (0x200..0x1000); preenche 16 bytes de slot com 0xFE (vazio); se `DAT_08065100` é zero procura o primeiro slot vazio, senão desenha nomes de peça (Battle_DrawPartNameSprites); limpa e copia buffers com cpufastset; incrementa contador.
- RAM: REG_DISPCNT; slots em *DAT_080650fc+0x58..0x67; byte +0x2F = peça atual.
- Confiança: provável (grade de 16 slots bate com a grade de peças descrita em PS.md e GF4). Tabela 0x08483954.

## 0x08065208 PartGrid_HandleCursorStep
- O que faz: dispatcher de passo da grade: se +0x4C do objeto é zero, usa `StateMachine_DispatchStep`, senão `StateMachine_DispatchSub` (e reinicia a tela); move o cursor (Slot_SetPosStepByMode), e nos botões toca efeitos de som (snd_sfx_request 2, 4, 5), ajusta o contador de linha (+0x2E/+0x2F) e chama Battle_DrawPartNameSprites. Ramo com flag 0x40 decrementa, 0x80 incrementa.
- RAM: REG_DISPCNT não; objeto de grade em DAT_080652a0/DAT_08065300/DAT_08065394; OAM em DAT_08065308 e +0x200/+0x280/+0x2C0 (atributos de sprite).
- Confiança: provável (controle de cursor de grade de peças). Tabela 0x0848395C.

## 0x080654C0 Screen_FadeOutToStateSix
- O que faz: `StateMachine_DispatchSub`; quando termina, faz fade de 9 passos (Palette_FadeRowsToColorA/B); no fim mascara REG_DISPCNT, zera o estado e pede estado 6 (*DAT_08065524 = 6).
- RAM: REG_DISPCNT; objeto +0x84; estado DAT_08065524.
- Confiança: provável. Tabela 0x08483960.

## 0x0806552C Screen_FadeOutToModeTwo
- O que faz: `StateMachine_DispatchSub`; depois fade de 9 passos e, no fim, mascara REG_DISPCNT, limpa 0x80 entradas de OAM (ClearObjectAttributeEntries) e chama `MainState_RequestModeTransition(2)`.
- RAM: REG_DISPCNT; objeto +0x84.
- Confiança: confirmado (chamada de transição 2 e fade lidos no decompile). Tabela 0x08483964.

## 0x08065590 PartGrid_WaitInputOrExit
- O que faz: dispatcher de dois modos (`StateMachine_DispatchStep` ou `DispatchSub`); se a tecla de confirmar/cancelar (máscara 3 em DAT_080655f4) estiver pressionada, reinicia o estado e vai para 5.
- RAM: objeto em DAT_080655bc (+0x4C); estado DAT_080655f8; entrada de teclas DAT_080655f4.
- Confiança: provável. Tabela 0x08483968.

## 0x080655FC PartGrid_HandleSelectionAction
- O que faz: dispatcher de passo; se a tabela de estados (StateTable_DispatchByIndex) devolve valor com nibble baixo zero, verifica custo (>= 100) e vai para 6 ou 8 com timer 0xB4; senão vai para 5 (Obj_InitSlotAndLoad).
- RAM: custo no byte de DAT_0806568c; estado DAT_08065690/08065668; objeto DAT_08065664.
- Confiança: provável (lógica de confirmar peça/custo). Tabela 0x0848396C.

## 0x08067870 Screen_PointRingFadeStep
- O que faz: `PointRing_StepFrame`; fade de paleta por passos até 0x41 (valor 0x20 - n/2); depois só incrementa o contador.
- RAM: objeto +0x84 e +5 em DAT_080678bc; contador DAT_080678d0.
- Confiança: provável (usa o PointRing já conhecido). Tabela 0x080B0024.

## 0x08067974 Screen_ScrollTextRowsStep
- O que faz: `PointRing_StepFrame`; a cada 32 quadros desenha uma linha de texto (draw_text_rtl_tiles, da direita para a esquerda) e preenche a linha com bg_fill_row_tiles; atualiza HUD (hud_draw_row_tiles) e o contador de rolagem.
- RAM: objeto em DAT_080679d4 (+0x3A = quadro, +0x7D = índice de linha, +0xAC = ponteiro de texto); contador DAT_08067a6c+2.
- Confiança: provável (rolagem de texto, sem confirmar o uso exato). Tabela 0x080B002C.

## 0x08067A70 Screen_ScrollRowsUpPhase
- O que faz: `PointRing_StepFrame`; incrementa +0x3A e chama bg_scroll_rows_up; quando termina, zera +0x3A, incrementa a fase (+5) e ajusta contador de rolagem (< 3 usa DAT_08067ac8 - 2, senão DAT_08067aac + 1).
- RAM: objeto DAT_08067aa8 (+5, +0x3A); fase em DAT_08067ac8/DAT_08067aac.
- Confiança: provável. Tabela 0x080B0030.

## 0x08067ACC Screen_ClearRowsAndFadeMusic
- O que faz: `PointRing_StepFrame`; enquanto +0x3A < 15 zera uma entrada de tile em cada linha (stride 0x40 em VRAM, base DAT_08067b24); ao chegar a 15, incrementa contador, zera +0x3E e pede `snd_fade_request(0xC0)`.
- RAM: VRAM de tilemap (DAT_08067b20 >> 8 & 0x1F) * 0x800 + linha * 0x40; objeto DAT_08067af8.
- Confiança: provável (limpeza de linhas e fade de música). Tabela 0x080B0034.

## 0x08067B28 Screen_FadeAndWaitMusicToMode12
- O que faz: `PointRing_StepFrame`; enquanto +0x3E < 0xC1 faz fade de paleta por passos de /6 (aux_div_s32); depois espera qualquer trilha de som parar (snd_any_track_active) e pede transição de modo 0xC.
- RAM: objeto DAT_08067b74 (+0x3E); contador DAT_08067b98.
- Confiança: provável. Tabela 0x080B0038.

## 0x0806AA10 Battle_AdvanceBotPartTimerByTable
- O que faz: percorre 3 entradas; para cada uma, consulta uma tabela de bytes (índice = byte de DAT_0806aaa0 * 7 + valor), e quando o contador passa do limite incrementa o campo +0x1A do registro de 0x40 bytes (DAT_0806aa90) do bot correspondente; zera o contador.
- RAM: registros de bot de 0x40 bytes em DAT_0806aa90 (+0x1A); tabela em DAT_0806aa88/DAT_0806aa8c; contadores em DAT_0806aa98.
- Confiança: provável (função chamada pelo BL 0x08069212; o efeito por tabela é lido). Tabela/BL de origem 0x08069212 (ver FS9_lista).
- Nome do FN3 (Battle_AdvanceBotPartTimerByTable) confirmado pela leitura; renomeada.

## 0x0806B134 FUN_0806B134 (chute, não renomeada)
- O que faz: recebe índice de bot; em registro de 0x40 bytes atualiza +2 e +4 conforme +0x1A (< 0x20, < 0x30, senão +2) e chama um salto indireto (tabela não recuperada pelo Ghidra) com o resultado da comparação de +2 e +0x22.
- RAM: registro de bot em DAT_0806b16c (stride 0x40); tabela em DAT_0806b170.
- Confiança: chute (é um passo de animação de bot; o destino do salto não está resolvido). Tabela 0x084857D0. Nota: FS.md dizia "sem função no Ghidra"; agora é FUN_0806B134 com corpo.

## 0x0806B1FC FUN_0806B1FC (chute, não renomeada)
- O que faz: ajusta +4 do registro de bot (+2 se +0x1A < 0x10, -2 se 0xD2..0xE1); se +0x1A > 0xE1 chama `slot_ptr_stub`; depois salto indireto (tabela não recuperada).
- RAM: registro de bot em DAT_0806b238 (stride 0x40).
- Confiança: chute. Tabela 0x084857E0.

## 0x0806B248 FUN_0806B248 (chute, não renomeada)
- O que faz: escolhe valor de registrador de cor (0x1000 ou (0x20 - n) * 0x100 | n) por +0x1A do bot e grava no ponteiro DAT_0806b26c/DAT_0806b2a0; desce +4 se +0x1A é ímpar; zera contador em DAT_0806b2b4 se +0x1A > 0x3F; salto indireto.
- RAM: registro de bot em DAT_0806b268 (stride 0x40); registrador em DAT_0806b26c.
- Confiança: chute (efeito visual de flash/cor do bot). Tabela 0x084857E4.

## 0x0806B3D8 FUN_0806B3D8 (chute, não renomeada)
- O que faz: oscila +2 do registro de bot por ±1 por quadro (sinal pelo bit 1 do parâmetro); ao passar de 0x60 em +4, fixa +4 em 200 e retorna 0; senão flaga sprite (0x1000 em OAM) se o bit 8 de +0x1A estiver ligado, e retorna 1.
- RAM: registro de bot em DAT_0806b404 (stride 0x40); atributo de sprite em DAT_0806b480 (máscara) e puVar6[0xD].
- Confiança: chute (movimento de bob/balanço do bot). Tabela 0x084857F0.

## 0x0806BB24 FUN_0806BB24 (chute, não renomeada)
- O que faz: chama `fx_step_table_a`; quando termina pede a música 0x14 (snd_song_request) e incrementa o contador.
- RAM: contador DAT_0806bb40.
- Confiança: chute (música 0x14 não identificada no mtbe). Tabela 0x08485904.

## 0x0806BB44 FUN_0806BB44 (chute, não renomeada)
- O que faz: `dispatch_state_8`; quando retorna não zero incrementa o contador; depois sempre chama `anim_dispatch_state`.
- RAM: contador DAT_0806bb60.
- Confiança: chute. Tabela 0x08485908.

## 0x0806BB64 FUN_0806BB64 (chute, não renomeada)
- O que faz: autômato de 3 estados sobre uma linha de células: célula 0 = byte do vetor (índice do contador) mod 3; células seguintes são atualizadas por regra mod 7 a partir do vizinho anterior (incrementa, decrementa ou repete, com limite 1 e 2).
- RAM: vetor de células em DAT_0806bbb8+0xF..; contador em DAT_0806bbc4 (+1).
- Confiança: chute (é um padrão de células; o efeito visual não foi visto). Tabela 0x0848590C. FN3 sugeria `Effect_StepRowPattern3State`; esta leitura bate com o nome, mas não foi renomeada.

## 0x0806BC94 Battle_EffectFallPhase1Step
- O que faz: copia frames de animação (battle_anim_copy_frames), roda `effect_anim_fall`; quando termina marca flag 1 em +0x39, pede fade de som 0x30 e vai para estado 8.
- RAM: objeto DAT_0806bcbc (+0x39); estado DAT_0806bcc0.
- Confiança: provável (fase 1 de um efeito de queda). Tabela 0x08485914.

## 0x0806BCC4 Battle_EffectPhase2Step
- O que faz: atualiza objetos (battle_update_objects), trajetória (battle_anim_trajectory) e tile do frame (battle_objects_frame_tile), roda `effect_anim_phase2`; quando termina marca 2 em +0x39, fade de som 0x30, estado 8.
- RAM: objeto DAT_0806bcf4 (+0x39); estado DAT_0806bcf8.
- Confiança: provável (fase 2 do mesmo efeito). Tabela 0x08485918.

## 0x0806DBFC FUN_0806DBFC (chute, não renomeada)
- O que faz: lê o bit 8 de DAT_0806DC2C. Se ligado: `battle_slot_init(6,2)`, toca sfx 3 e incrementa contador. Se não: espera contador passar de DAT_0806dc68, depois `slot_ptr_stub(2)`, `fx_screen_setup_b(4,1,0)` e estado 3.
- RAM: DAT_0806dc2c (flag de bit 8); objeto DAT_0806dc64 (+4); estado DAT_0806dc6c.
- Confiança: chute (o ramo é lido; o efeito de tela não). Tabela 0x08485984. FN3 sugeria `Battle_TimedScreenFxStep`; FL6 deixou sem nome.

## 0x0806DC7C FUN_0806DC7C (chute, não renomeada)
- O que faz: conta até 0x29 quadros em +4; depois zera o contador, chama `slot_ptr_stub(2)` e `fx_screen_setup_b(4,1,0)`, define estado 2 e campos de tela.
- RAM: objeto DAT_0806dcac (+4); estado DAT_0806dcb0.
- Confiança: chute. Tabela 0x08485988.

## 0x0806DCC0 Battle_InitScreenAndSlots
- O que faz: inicializa a tela de batalha: `fx_screen_setup_a(4,2)`, música 0, `Map_InitializeObjectRenderSlot` em 5 slots (0,1,3,4 com posições e tamanhos fixos) e `battle_slot_init` nos slots 0,1,7,8; liga REG_DISPCNT |= 0x1300; estado 1.
- RAM: REG_DISPCNT; objeto de slots em DAT_0806ddfc (+2, +4, +0xB, +0x42.., +0xC2..+0x132); estado DAT_0806de08.
- Confiança: provável. Tabela 0x0848598C.

## 0x0806DE9C Battle_BuildBotSpriteOam
- O que faz: para um bot (registro de 0x40 bytes, contador +0x22 de 0 a 0x1E) escreve palavras de atributo de OAM (padrões 0x85000000 com posição e 0x80000000 com forma) e incrementa o contador; retorna 1; se o contador passou de 0x1E retorna 0.
- RAM: registro de bot em DAT_0806debc (stride 0x40, +0x22); atributos em DAT_0806dedc, DAT_0806df20, DAT_0806df48, DAT_0806dfb4.
- Confiança: provável (montagem de OAM de bot; campos de posição sem confirmar por um print). Tabela 0x08485994.

## 0x0806E038 FUN_0806E038 (chute, não renomeada)
- O que faz: ajusta timer +4 do registro de bot para 0xC8 ou 0x73 conforme o bit 0 de +0x22; incrementa +0x22 e chama salto indireto (tabela não recuperada).
- RAM: registro de bot em DAT_0806e058 (stride 0x40).
- Confiança: chute. Tabela 0x084859A4.

## 0x0806E074 FUN_0806E074 (chute, não renomeada)
- O que faz: passo de espera/piscar do bot: se +0x32 < +0x1A, avança estágio em +0x22 (até 8); no estágio 0 com `*DAT_0806e0d8 == 9` marca +0x32 com DAT_0806e0dc e zera +0x1A; senão mexe em +2/+6; no estágio >= 8 sorteia tempo com rand_scaled(0) (máscara DAT_0806e0bc + 0x20); depois salto indireto.
- RAM: registro de bot em DAT_0806e0b8 (stride 0x40); flag global DAT_0806e0d8.
- Confiança: chute (intervalo aleatório de animação de bot). Tabela 0x084859AC.
