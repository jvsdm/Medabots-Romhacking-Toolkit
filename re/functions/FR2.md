# FR2: funções pendentes de FL4 (17)

Lista vinda de "Pendente" do FL4.md. Tamanho real = Body do get_function_by_address.
Renomeadas no Ghidra só as de confiança "provável"; o restante fica FUN_ com nome sugerido no CSV.

## 0x080126EC Battle_SumBotPartLossScore
- Assinatura: `undefined4 Battle_SumBotPartLossScore(void)` (FUN_080126ec, 0x080126EC-0x0801274B, 96 bytes)
- O que faz: percorre 3 blocos de bot (stride 0x100) e, para cada bot ativo (byte +0x19 != 0), soma 8 ou 16 por parte
  com flag zero; o total vezes 2 é somado ao campo 0x16C do estado de luta. Parece pontuação de perda de partes para a
  atualização de tela da luta.
- Chama: nenhuma função própria (só leituras). Chamada por: agregador 0x080125B4 (R0, lista de seis atualizadores).
- Memória: lê ponteiros DAT_08012750 + bloco do bot (0x03000590 + bot*0x100, offsets +0x19, +0x18, -0x30); escreve
  estado de luta em DAT_0801274C+0x16C (halfword).
- Para o Lua: sim, se a barra/pontuação de partes for portada; é só soma de flags.
- Confiança: provável (laço e somas claros; semântica exata do campo 0x16C não confirmada).

## 0x08013164 Fight_PartSelectStep
- Assinatura: `void Fight_PartSelectStep(void)` (FUN_08013164, 0x08013164-0x08013221, 0xBD bytes)
- O que faz: passo por quadro do menu de escolha de parte. Desenha overlay dos bots ativos, anima quadros de entrada,
  atualiza o sprite composto, passa o efeito do slot 9, avança um contador de quadros e atualiza seletor de tipo de
  parte e texto de status. Botão A (bit 0): toca SFX 4, encerra o efeito e avança o estado para 0x15. Botão B (bit 1):
  toca SFX 5, limpa slots 0..6 e 0x14..0x26, apaga o fundo, zera o contador e avança o estado.
- Chama: `Fight_DrawActiveBotsOverlay`, `Fight_AnimateEntryFrames`, `Sprite_UpdateComposite4Motion`,
  `Fight_StepPartEffectSlot9`, `Fight_InitKindObjectsForState`, `Fight_UpdatePartKindSelector`,
  `Fight_DrawSelectedActionStatusText`, `Fight_FinishPartEffectSlot9`, `Tile_ClearSlots`, `BgMap_FillRect16`,
  `snd_sfx_request`. Chamada por: ainda não rastreado.
- Memória: ponteiro PTR_DAT_080131E8+0x80 (quadro de entrada); PTR_DAT_080131EC+0x84 (contador); estado de entrada em
  PTR_DAT_080131F0 (bits 1 e 2); estado da luta em PTR_DAT_080131F8; REG_BG0CNT.
- Para o Lua: sim, é o menu de escolha de parte da luta (A confirma, B cancela).
- Confiança: provável (sequência de passos e SFX claros; nomes dos blocos de estado não rotulados).

## 0x080140B0 (sem nome; sugestão Battle_PanelEntryStep)
- Assinatura: `undefined4 FUN_080140b0(void)` (0x080140B0-0x0801424F, 0x1A0 bytes)
- O que faz: máquina de estados de 4 fases com temporizador em +0x86 (corta em 0x3C quadros). Fase 0: inicializa
  objeto (posição 0xC2/0xC4, paleta, flags) e grava registrador por CpuFastSet. Fase 1: espera bit de bloqueio, decrementa
  contador +0xE0 e, ao zerar, configura um segundo objeto e avança. Fase 2 e 3: passos de animação com incremento/decremento
  em tabela de bytes, avançando até o limite.
- Chama: `Tile_ClearSlots`, `bios_cpufastset`. Chamada por: ainda não rastreado.
- Memória: DAT_080140CC+0x84 (fase), DAT_080140CC+0x86 (temporizador), DAT_080141B8 (objeto), DAT_08014124 (objeto 2).
- Para o Lua: provável que seja uma entrada de painel com animação; precisa confirmar pelo chamador.
- Confiança: chute (fases claras, mas sem rótulo do painel nem do chamador).

## 0x080629DC Dispatch_CallHandlerByByteIndex4
- Assinatura: `undefined4 Dispatch_CallHandlerByByteIndex4(void)` (FUN_080629dc, 0x080629DC-0x08062A05, 0x2A bytes)
- O que faz: copia 7 palavras de uma tabela da ROM para a pilha, pega 1 byte de índice e chama, via `CallViaR0`, a
  entrada de 4 ponteiros desse índice. Despachante com índice de byte (padrão de `CallViaR0`).
- Chama: `CallViaR0`. Chamada por: ainda não rastreado. Parecido com `Dispatch_CallArgsByIndex` (0x0805CB20).
- Memória: lê tabela em DAT_08062A08 (7 palavras); índice em byte apontado por DAT_08062A0C.
- Para o Lua: não precisa; é despacho de ponteiro, o PC chama a função direto.
- Confiança: provável (despacho claro; tabela e índice não rotulados).

## 0x0806379C BattlePanel_DrawSlotsStep
- Assinatura: `void BattlePanel_DrawSlotsStep(void)` (FUN_0806379c, 0x0806379C-0x0806381F, 0x84 bytes)
- O que faz: desenha texto em dois slots do painel de batalha (com campos 0x31/0x33 e 0x32/0x34 do estado), rola as
  colunas de tile e, quando o campo short +0x3A é zero, configura um objeto (atributo 0xC000, posição 0x28/0x40,
  tipo 8, 0x2A) com `Obj_InitSlotAndLoad(1,0,1)` e incrementa um contador.
- Chama: `BattlePanel_DrawSlotText`, `Anim_ScrollTileColumnsAlt`, `Obj_InitSlotAndLoad`. Chamada por: ainda não rastreado.
- Memória: ponteiro DAT_08063820 (estado do painel); DAT_08063824 (base de objetos, +0x840 = atributo de OAM);
  contador em DAT_0806383C.
- Para o Lua: sim, se o painel de batalha for portado (texto dos dois slots + objeto de cursor).
- Confiança: provável (fluxo claro; campos e offsets sem rótulo).

## 0x08064610 Anim_WaitCounterThenAdvance
- Assinatura: `void Anim_WaitCounterThenAdvance(void)` (FUN_08064610, 0x08064610-0x0806462F, 0x2C bytes)
- O que faz: chama `Anim_StepRecordCounter` no contador em +0x50 do estado; quando retorna diferente de zero, define o
  estado de fase como 0x14 e zera o campo seguinte.
- Chama: `Anim_StepRecordCounter`. Chamada por: ainda não rastreado.
- Memória: ponteiro DAT_08064630 (+0x50, contador); estado de fase em DAT_08064634 (valor 0x14); DAT_08064638 zerado.
- Para o Lua: sim, como espera-e-avança de uma máquina de estados de animação.
- Confiança: provável (lógica de 3 linhas clara; nome do estado 0x14 não confirmado).

## 0x08065198 Anim_PaletteFadeThenSpawn
- Assinatura: `void Anim_PaletteFadeThenSpawn(void)` (FUN_08065198, 0x08065198-0x080651FC, 0x66 bytes)
- O que faz: enquanto o contador +0x84 for menor que 9, escurece/clareia as linhas de paleta A e B com nível
  0x20 − 4*contador e incrementa o contador. Ao chegar em 9: zera o campo +0x4C, cria o objeto 1 com
  `Obj_InitSlotAndLoad(1,0,0)`, zera +6 e avança o estado.
- Chama: `Palette_FadeRowsToColorA`, `Palette_FadeRowsToColorB`, `Obj_InitSlotAndLoad`. Chamada por: ainda não rastreado.
- Memória: ponteiro DAT_08065200 (estado); contador em DAT_080651D0+0x84; estado em DAT_08065204.
- Para o Lua: sim, como fade de paleta em 9 passos seguido de aparição de objeto.
- Confiança: provável (fade e passos claros; nome do efeito não confirmado).

## 0x08066574 (sem nome; sugestão Menu_RunSaveLoadOptionsThenFade)
- Assinatura: `undefined4 FUN_08066574(void)` (0x08066574-0x080665AD, 0x38 bytes)
- O que faz: zera um byte de controle; se o byte +0x4A do estado for zero, chama `Site_RunSaveLoadOption(0)` até 4
  vezes enquanto retornar diferente de zero; depois inicia o fade de mistura da tela.
- Chama: `Site_RunSaveLoadOption`, `Screen_InitBlendFade`. Chamada por: ainda não rastreado.
- Memória: DAT_0806658C (zerado); ponteiro DAT_08066590+0x4A.
- Para o Lua: depende do que `Site_RunSaveLoadOption` faz (save/load); no PC o save é arquivo, não EEPROM.
- Confiança: chute (o nome do sub-chamador "Site_" parece errado; ver mtbe).

## 0x08068374 Fx_BlendAndStateStep
- Assinatura: `void Fx_BlendAndStateStep(void)` (FUN_08068374, 0x08068374-0x080683B5, 0x42 bytes)
- O que faz: avança o blend de partículas (`fx_particles_blend_step`) e despacha a máquina de estados do objeto
  (`StateMachine_DispatchStep` com ponteiro em +0x48). Se o despacho devolver não zero e o bit 0 do flag estiver
  ligado, inicia um timer no campo +0x50 (`Obj_InitTimerRecord` com 0,0,1,0xC0) e incrementa um contador.
- Chama: `fx_particles_blend_step`, `StateMachine_DispatchStep`, `Obj_InitTimerRecord`. Chamada por: ainda não rastreado.
- Memória: DAT_080683B8+0x48 (ponteiro de estado); flag em DAT_080683BC; DAT_080683C0+0x50 (timer); contador em DAT_080683C4.
- Para o Lua: sim, é o passo por quadro de um efeito com blend e timer.
- Confiança: provável (sequência e condição claras; nome do efeito genérico).

## 0x0806B744 Dispatch_CallHandlerByWordIndex
- Assinatura: `undefined4 Dispatch_CallHandlerByWordIndex(void)` (FUN_0806b744, 0x0806B744-0x0806B759, 0x16 bytes)
- O que faz: trampolim. Lê um byte de índice, multiplica por 4 e chama via `CallViaR0` a palavra da tabela naquele
  índice.
- Chama: `CallViaR0`. Chamada por: ainda não rastreado.
- Memória: índice em byte apontado por DAT_0806B760; tabela de ponteiros em DAT_0806B75C.
- Para o Lua: não precisa (despacho de ponteiro).
- Confiança: provável (código de 3 instruções; tabela não recuperada).

## 0x0806D9A8 Battle_IntroSetupStep
- Assinatura: `void Battle_IntroSetupStep(void)` (FUN_0806d9a8, 0x0806D9A8-0x0806DAA3, 0xFC bytes)
- O que faz: na primeira chamada (contador +8 == 0) toca a música 0, cria o objeto 1 (`Map_InitializeObjectRenderSlot`)
  e inicializa o slot 3 de batalha. Depois, se o byte +0x60 é zero, cria os objetos 3 e 4 com suas posições e
  inicializa os slots 7 e 8, zera o contador e marca o estado como 7. Caso contrário apenas incrementa o contador.
- Chama: `snd_song_request`, `Map_InitializeObjectRenderSlot`, `battle_slot_init`. Chamada por: ainda não rastreado.
- Memória: DAT_0806DA80 (estado, +8 contador); DAT_0806DA84 (objetos, +0x42/+0x44/+0x4B, +0x60, +0xC2/+0xC4, +0x102/+0x104,
  +0x132); estado em DAT_0806DA8C.
- Para o Lua: sim, é a montagem da entrada da batalha (música e objetos de cena).
- Confiança: provável (fluxo claro; significado dos objetos 3/4 não confirmado).

## 0x0806E96C Screen_LayerHideTimedStep
- Assinatura: `void Screen_LayerHideTimedStep(void)` (FUN_0806e96c, 0x0806E96C-0x0806E9BF, 0x54 bytes)
- O que faz: na primeira chamada (contador +8 == 0) aplica máscara em REG_DISPCNT (apaga camadas). Enquanto o
  contador não passar de 0x3C, incrementa. Ao passar: zera o contador, chama `fx_screen_setup_b(2,4,0)`, põe +4 = 10
  e muda o estado para 2. No fim faz uma chamada indireta por tabela não recuperada (o Ghidra não resolveu o salto).
- Chama: `fx_screen_setup_b`, e um destino indireto não recuperado. Chamada por: ainda não rastreado.
- Memória: REG_DISPCNT; DAT_0806E988 (estado, +4, +8); DAT_0806E9B0 (estado seguinte).
- Para o Lua: sim, como esmaecimento por tempo de camadas da tela (60 quadros).
- Confiança: provável (lógica clara; o destino do salto final ainda não foi recuperado).

## 0x080708D8 Fx_FrameTickAndDispatch
- Assinatura: `void Fx_FrameTickAndDispatch(void)` (FUN_080708d8, 0x080708D8-0x08070905, 0x2E bytes)
- O que faz: por quadro, se o byte +0xC está ligado chama `timer_if_flag`; se o byte +0xD está ligado chama
  `anim_6_frames_2_counters`; depois despacha por índice de byte (×4) para um handler de tabela via `CallViaR0`.
- Chama: `timer_if_flag`, `anim_6_frames_2_counters`, `CallViaR0`. Chamada por: ainda não rastreado.
- Memória: DAT_08070908 (estado, +0xC/+0xD); índice em byte em DAT_08070910; tabela em DAT_0807090C.
- Para o Lua: sim, é o laço por quadro do efeito (ordem: timer, animação, handler).
- Confiança: provável (ordem clara; nomes dos auxiliares vêm do Ghidra, sem confirmação de semântica).

## 0x08071304 Screen_TableStepTimedSetup
- Assinatura: `undefined4 Screen_TableStepTimedSetup(void)` (FUN_08071304, 0x08071304-0x0807145B, 0x158 bytes)
- O que faz: se o contador +8 passou de 0x19, chama `fx_screen_setup_b(4,2,0)`, zera o contador, põe +4 = 0xE e
  muda o estado para 2. Senão, despacha por tabela indexada pelo contador (chamada indireta; o salto não foi
  recuperado pelo Ghidra) e devolve o resultado.
- Chama: `fx_screen_setup_b`, destino indireto não recuperado. Chamada por: ainda não rastreado.
- Memória: DAT_08071320 (estado, +4, +8); tabela em DAT_08071324; estado seguinte em DAT_0807145C.
- Para o Lua: provável (passo de sequência com tempo limite, equivalente ao par de `Screen_LayerHideTimedStep`).
- Confiança: provável (padrão do 0x0806E96C; tabela não recuperada).

## 0x08071FF8 Anim_FrameCycleStep
- Assinatura: `longlong Anim_FrameCycleStep(void)` (FUN_08071ff8, 0x08071FF8-0x0807205F, 0x68 bytes)
- O que faz: incrementa o contador +0x1A. Em quadros ímpares, incrementa o short +4; ao passar de −1, toca SFX 0x59.
  O campo +6 recebe um valor cíclico de 4 passos ((contador>>2)&3) + base. Quando +4 passa de 0x3F: toca SFX 7,
  programa a DMA3 (origem e destino com ponteiros da ROM/RAM) e incrementa o campo +0x10 do estado.
- Chama: `snd_sfx_request` (0x59 e 7). Chamada por: ainda não rastreado.
- Memória: DAT_08072060 (estado: +4, +6, +0x1A); DAT_08072064 (base); DMA3 SAD/DAD/CNT via PTR_REG_DMA3SAD_08072074;
  flag +0x11 zerada; contador +0x10.
- Para o Lua: sim, como ciclo de quadros de um efeito, com o som do fim. A DMA não é necessária no PC.
- Confiança: provável (ciclo e som claros; significado da DMA e do campo +0x10 não confirmado).

## 0x08074F0C Sound_TrackSfxTickCounter
- Assinatura: `undefined4 Sound_TrackSfxTickCounter(undefined4 canal)` (FUN_08074f0c, 0x08074F0C-0x08074F43, 0x38 bytes)
- O que faz: incrementa o short +0x730 do estado até o limite em DAT_08074F48; chama `snd_track_apply_sfx_all` com o
  parâmetro recebido; depois decrementa +0x730 se não for zero. Contador de tempo para o efeito sonoro.
- Chama: `snd_track_apply_sfx_all`. Chamada por: ainda não rastreado (sem xref na lista).
- Memória: DAT_08074F44 (estado de som); campo +0x730 (short).
- Para o Lua: sim, se o áudio do PC tiver efeito por tempo; senão, não precisa.
- Confiança: provável (contador claro; significado de +0x730 não confirmado).

## 0x0807DB80 Math_SoftFloat64Compare
- Assinatura: `undefined4 Math_SoftFloat64Compare(undefined4 a_lo, undefined4 a_hi, undefined4 b_lo, undefined4 b_hi)`
  (FUN_0807db80, 0x0807DB80-0x0807DBC9, 0x4A bytes)
- O que faz: desempacota dois valores de ponto flutuante de 64 bits com `sf_unpack`. Se um dos dois tem classe menor
  que 2 (zero, infinito ou NaN, pela convenção de unpack), devolve 1. Senão compara com `sf64_cmp_core`.
- Chama: `sf_unpack`, `sf64_cmp_core`. Chamada por: ainda não rastreado (sem xref na lista).
- Memória: só pilha.
- Para o Lua: não precisa (Lua tem aritmética de ponto flutuante nativa).
- Confiança: provável (desempacotamento e retorno especial claros; convenção de classe não confirmada).

---
Pendente (chutes, sem rename): 0x08066574 (nome FUN_; sugestão Menu_RunSaveLoadOptionsThenFade; o "Site_" do
chamador parece errado) e 0x080140B0 (sugestão Battle_PanelEntryStep; falta o chamador). Destinos indiretos não
recuperados: 0x0806E96C e 0x08071304 (ambos saltam por tabela).
