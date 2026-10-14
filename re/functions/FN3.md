# FN3: funções FUN_ >= 0x08058000 (lista FN3_lista.txt, 28 entradas)

Pular (outro agente): 0805D4E0..0805DCB0, 080639C4, 080640DC, 0806445C, 080644D8, 08068238, 08068298, 08068328.
Confiança: confirmado (lido no código) | provável | chute. Nomes: FUN_ mantido quando é chute (sugestão no texto).

## 0x0805C0D4 FUN_0805c0d4 (chute: nome sugerido Task_CallHandlerAdvanceCounters)
- Assinatura: `void f(void)`
- O que faz: chama um handler via tabela de ponteiros indexada (CallViaR0), incrementa um par de shorts (contador e
  espelho em +2) e, se o bit 0 de um byte de flag estiver setado, incrementa outro par de shorts.
- Chama: CallViaR0 (trampolim). Chamada por: não verificado.
- Memória: ponteiros DAT_0805c114/c118/c11c/c120/c124 (literais, não resolvidos).
- Para o Lua: não precisa (controle de tarefa interna).
- Confiança: chute no nome.

## 0x08064B90 FUN_08064b90 (chute: nome sugerido Dispatch_IndexedTrampoline)
- Assinatura: `void f(void)`
- O que faz: salva lr, copia 2 words de um literal para uma pilha local e chama `local[idx]` via CallViaR0; idx vem de
  um byte em DAT_08064bb8. Trampolim de despacho genérico.
- Chama: CallViaR0. Chamada por: não verificado.
- Para o Lua: não precisa.
- Confiança: confirmado (o que faz); nome é chute.

## 0x08065A94 FUN_08065a94 (chute: nome sugerido Obj_StepThenTailJump)
- Assinatura: `void f(void)`
- O que faz: chama StateMachine_DispatchStep sobre um objeto (escolhido por obj+8: offset 0x14 ou 0x10). Se retornar
  != 0: se obj+8 == 0 incrementa um contador e zera short +0x3A; senão, se bit 0 de um flag, toca o SFX 0x0D e zera
  bytes +0x84/+0x1B e decrementa um contador de 2. Termina com salto indireto não recuperado (tail call).
- Chama: StateMachine_DispatchStep, snd_sfx_request, jump indireto (tabela não recuperada no decompile).
- Memória: contador em DAT_0805... (literais), objeto via ponteiro DAT_08065acc.
- Para o Lua: o SFX 0x0D precisa de mapeamento de som se for portado.
- Confiança: provável no fluxo; nome é chute.

## 0x08065B0C FUN_08065b0c (chute: nome sugerido Link_ExchangeStepCheck)
- Assinatura: `int f(void)`
- O que faz: rotina grande por frame sobre uma grade de shorts (cada célula em +10 + idx*4, estado em +6). Se a
  máscara 0xF00 do registrador de status for 0 e gFrameCount & 0xF for 0, chama InitializeLinkTransferState. Chama
  FUN_08002190 e Screen_ReadTwoBitField, depois compara a célula com o valor esperado (uVar1|1), zera contadores +0x3A
  e, após 0x12D frames sem resposta, faz Obj_ResetStateFields e marca estado 1. Parece handshake de troca (link cable).
- Chama: InitializeLinkTransferState, FUN_08002190, Screen_ReadTwoBitField, Obj_ResetStateFields.
- Memória: 0x03... (ponteiros literais), IO de status não resolvido; contador +0x3A do objeto.
- Para o Lua: não precisa (link/hardware).
- Confiança: provável (handshake de link); nome é chute. Nome Screen_ReadTwoBitField parece errado para o que faz
  (leitura de 2 bits de link); revisar.

## 0x08066460 FUN_08066460 (chute: nome sugerido Link_HandshakeStep)
- Assinatura: `void f(void)`
- O que faz: se o byte +0x7A do objeto está 0, lê um halfword da porta (cabeçalho 0xEF00 | byte) e, se bateu, guarda o
  byte em +5 e marca +0x7A=1; senão, se igual ao valor esperado, copia para o registro. Se o timer +0xE da célula
  bate com o valor, ou reseta o estado (quando +5 == 4, com DAT_08066508=8) ou reinicia o timer (Obj_InitTimerRecord
  com 0xC0) e soma 2 num contador.
- Chama: Obj_ResetStateFields, Obj_InitTimerRecord.
- Para o Lua: não precisa (troca de link).
- Confiança: provável; nome é chute.

## 0x08066574 FUN_08066574 -> renomeado Save_RunOptionsThenBlendFade (provável)
- Assinatura: `void f(void)`
- O que faz: zera um byte de estado; se o byte +0x4A do objeto for 0, chama Site_RunSaveLoadOption(0) até 4 vezes
  enquanto retornar != 0; por fim Screen_InitBlendFade.
- Chama: Site_RunSaveLoadOption, Screen_InitBlendFade.
- Para o Lua: a lógica de opções de salvar/carregar precisa ser portada se o save for reimplementado.
- Confiança: provável (o nome Site_RunSaveLoadOption vem de outra rodada; conferir).

## 0x08067B9C FUN_08067b9c (chute: nome sugerido Dispatch_CallFromLocalTable)
- Assinatura: `void f(void)`
- O que faz: copia 7 words de uma tabela (literal) para a pilha, chama `local[idx]` via CallViaR0 (idx de um byte em
  DAT_08067bd4) e decrementa um short de contador em +2 de DAT_08067bd8.
- Chama: CallViaR0. Para o Lua: não precisa.
- Confiança: confirmado (o que faz); nome é chute.

## 0x08068924 FUN_08068924 (chute: nome sugerido Calc_CheckMultipleOf16AndStore)
- Assinatura: `u64 f(void)` (retorno de 64 bits: lr e bool)
- O que faz: lê dois valores (um byte, um s32); se valor % divisor == 0, guarda 0x10 - (valor / divisor) num short;
  retorna se valor == divisor * 0x10.
- Chama: aux_mod_s32, aux_div_s32 (0x0807CDF8 e 0x0807CD2C, conferir se o trampolim bate).
- Para o Lua: não precisa.
- Confiança: confirmado (o que faz); nome é chute.

## 0x08068978 FUN_08068978 (chute: nome sugerido Anim_ShowBg0AndStepTiles)
- Assinatura: `u64 f(void)`
- O que faz: liga o bit 0x100 de REG_DISPCNT (BG0). Se valor % divisor == 0, quatro vezes: lê um halfword de uma tabela
  (indexada pelo quociente), soma um offset e grava o resultado em 3 words de um registro de 0x0C bytes (+4 e +8). Por
  fim, se valor == divisor*0x10, apaga bits de REG_DISPCNT com uma máscara.
- Chama: aux_mod_s32, aux_div_s32. Memória: REG_DISPCNT (0x04000000), tabela de halfwords em ROM (não resolvida).
- Para o Lua: o efeito de BG0 precisa de equivalente no LÖVE se a animação for portada.
- Confiança: confirmado (o que faz); nome é chute.

## 0x08068B74 FUN_08068b74 (chute: nome sugerido Calc_CheckMultipleOf16AndStoreQuotient)
- Assinatura: `u64 f(void)`
- O que faz: mesma forma do 0x08068924: se valor % divisor == 0 grava o quociente num halfword; retorna valor ==
  divisor*0x10. Helper de contagem em passos de 16.
- Chama: aux_mod_s32, aux_div_s32. Para o Lua: não precisa.
- Confiança: confirmado (o que faz); nome é chute.

## 0x0806A748 FUN_0806a748 -> renomeado Battle_EmitBotSpritesInDepthOrder (provável)
- Assinatura: `void f(void)`
- O que faz: para 3 bots (índice i), se o byte de estado em DAT_0806a780[i] < 3, compara com os outros dois: conta
  quantos estão "à frente" (prioridade menor, ou empate e índice menor, usando um halfword em +0x1A de registros de
  0x40 bytes). Usa o resultado (5, 6 ou 7 + número de bots à frente) como ordem e chama sprite_part_emit(ordem, i, 0xF).
  Se o estado for 3 a 5, emite direto com ordem 5, 6 ou 7 conforme a comparação com a tabela DAT_0806a784.
- Chama: sprite_part_emit. Memória: registros de bot 0x40 bytes com +0x1A (ponteiro DAT_0806a81c).
- Para o Lua: ordenação de desenho dos sprites dos bots precisa ser replicada se a cena de batalha for portada.
- Confiança: provável (ordenação por profundidade); nome sugerido confirmar com a tabela DAT_0806a784.

## 0x0806A860 FUN_0806a860 (chute: nome sugerido Battle_UpdateThreeBotAnimTimers)
- Assinatura: `void f(void)`
- O que faz: atualiza três bots: se o estado < 3 e o bit 0 de um flag está setado, decrementa um timer em 4 (com
  piso 4) e soma 3 num halfword +0x1A do registro do bot. Para cada bot: se o estado < 3, incrementa um contador
  de passo; ao atingir o limite derivado da tabela (bits 0x15..0x17 do halfword), reduz outro timer e soma 3 no
  mesmo +0x1A. Depois: para o bot com timer 0x20 chama slot_spawn(i+1, 10, i, 0); senão avança um contador até o
  limite. Por último, a cada 8 frames (DAT_0806aa08 & 7 == 0), soma 2 em 3 bytes de Y (saturando em 0x20).
- Chama: slot_spawn. Memória: tabelas de bots e slots não resolvidas.
- Para o Lua: o timing de animação dos bots precisa ser portado se a animação de batalha for reimplementada.
- Confiança: provável (animação de bots); nome é chute.

## 0x0806AA10 FUN_0806aa10 (chute: nome sugerido Battle_AdvanceBotPartTimerByTable)
- Assinatura: `void f(void)`
- O que faz: para 3 bots: se o estado < 3, lê um limite numa tabela (indexada por estado e por um byte do modo) e
  incrementa um contador de 32 bits; ao atingir o limite, soma 1 no halfword +0x1A do registro da peça e zera o
  contador.
- Chama: nada direto. Memória: contadores de 3 words (DAT_0806aa98), registros de 0x40 bytes com +0x1A.
- Para o Lua: parte da temporização de animação de batalha.
- Confiança: provável (lógica de contador); nome é chute.

## 0x0806AAA4 FUN_0806aaa4 (chute: nome sugerido Battle_RunBotEntranceAndSpawnStep)
- Assinatura: `void f(void)`
- O que faz: máquina de estados de 3 bots (entrada/saída em cena). Por bot com estado < 3: se o halfword +0x1A da peça
  passou do limiar DAT_0806ab98, chama SetCompositeBattleSpriteAnimationSpeed e avança o estado; ao chegar a 3 cria
  o slot (slot_spawn) e, na primeira vez, inicializa 16 slots de render (Map_InitializeObjectRenderSlot, 0xC400) e
  toca SFX 0x43; caso contrário toca SFX 0x44 e zera a peça. Depois, por bot: desenha célula 3x3
  (draw_part_cell_3x3) e ajusta posições Y (+2, +0x2C e 0xD8 - metade do timer) e, para o lado 0/1, anima um
  composite (Sprite_UpdateComposite4Motion).
- Chama: SetCompositeBattleSpriteAnimationSpeed, slot_spawn, Map_InitializeObjectRenderSlot, snd_sfx_request,
  draw_part_cell_3x3, Sprite_UpdateComposite4Motion.
- Memória: registros de 0x40 bytes (+0x1A timer, +0x2A flag de animação, +0x3A limite); tabela de slots.
- Para o Lua: se a entrada dos bots em batalha for portada, este é o núcleo; SFX 0x43/0x44 precisam de mapa.
- Confiança: provável (fluxo de entrada/spawn); nome é chute. Função grande: decompile lido inteiro.

## 0x0806ADB0 FUN_0806adb0 -> renomeado Battle_UpdateCompositeBotMotion (provável)
- Assinatura: `void f(void)`
- O que faz: para 3 bots, conforme o flag +0x2A do registro (1 ou 2): anima o composite (Sprite_UpdateComposite4Motion)
  e atualiza Y do slot; no frame 0x18 (flag 1) ou 0x2C (flag 2), zerado pelo contador +2 e checado com o bit 0 de
  DAT_0806aef4/DAT_0806b014, cria objetos de sprite composite (CreateCompositeBattleSpriteObjects, 12 parâmetros) e
  muda a velocidade de animação (8 ou 0xC).
- Chama: Sprite_UpdateComposite4Motion, CreateCompositeBattleSpriteObjects, SetCompositeBattleSpriteAnimationSpeed.
- Para o Lua: parte de animação de sprites compostos de batalha.
- Confiança: provável (composites de bot); nome renomeado.

## 0x0806BB64 FUN_0806bb64 (chute: nome sugerido Effect_StepRowPattern3State)
- Assinatura: `void f(void)` (usa r6 não recuperado: unaff_r6)
- O que faz: atualiza uma linha de 0x0C células de estado (0..2, em +0xF) a partir de uma tabela de bytes e de
  aux_mod_u32 por 7: caso 0 copia a célula anterior; casos 1-3 decrementam (piso 1); casos 4-6 incrementam (teto 2).
  Incrementa um contador de frames em DAT_0806bbc4. Padrão de efeito visual.
- Chama: aux_mod_u32. Para o Lua: se o efeito for portado, replicar a regra de transição.
- Confiança: provável (regra de transição); nome é chute. Parâmetro r6 não recuperado pelo Ghidra.

## 0x0806DBFC FUN_0806dbfc (chute: nome sugerido Battle_TimedScreenFxStep)
- Assinatura: `void f(void)`
- O que faz: se o bit 0x08 de DAT_0806dc2c está setado: battle_slot_init(6,2), toca SFX 3, zera um campo e incrementa
  um contador. Senão, se o timer de DAT_0806dc64+4 passou de DAT_0806dc68: zera o timer, chama slot_ptr_stub(2) e
  fx_screen_setup_b(4,1,0), grava estado 3 e 2 halfwords, e marca DAT_0806dc6c=2. Caso contrário incrementa o timer.
- Chama: battle_slot_init, snd_sfx_request, slot_ptr_stub, fx_screen_setup_b.
- Confiança: provável (efeito de tela com timer); nome é chute.

## 0x0806E184 FUN_0806e184 (chute: nome sugerido Fx_StepTableBAndLatchState)
- Assinatura: `u32 f(void)` (retorno = lr salvo)
- O que faz: chama fx_step_table_b(); se retornar != 0: grava ponteiro e 2 words em DAT_0806e1cc, limpa bits de
  REG_DISPCNT com máscara DAT_0806e1d8, e copia 3 halfwords de PTR_DAT_0806e1e0 para os campos de gMainState e dois
  outros globais.
- Chama: fx_step_table_b. Memória: REG_DISPCNT (0x04000000), gMainState (nome vem do Toolkit).
- Confiança: provável; nome é chute.

## 0x08072B18 FUN_08072b18 (chute: nome sugerido Obj_SetByte2C)
- Assinatura: `void f(int obj, uint v)`
- O que faz: grava (v & 0xFF) no campo obj+0x2C. Setter trivial.
- Chama: nada. Para o Lua: não precisa.
- Confiança: confirmado (o que faz); nome é chute.

## 0x08072B20 FUN_08072b20 (chute: nome sugerido Obj_SetByte69_AndFlags51)
- Assinatura: `void f(int obj, u8 v)`
- O que faz: grava v em obj+0x69 e faz OR de 0x51 no byte de flags obj+0x50.
- Confiança: confirmado (o que faz); nome é chute.

## 0x08072B90 FUN_08072b90 (chute: nome sugerido Obj_LatchNextByteFromPtr14)
- Assinatura: `void f(int obj)`
- O que faz: copia o byte apontado por obj+0x14 para obj+0x6F, avança o ponteiro obj+0x14 em 1 e marca flag 0x40 em obj+0x50.
- Confiança: confirmado (o que faz); nome é chute.

## 0x080732C8 FUN_080732c8 (chute: nome sugerido Obj_EmitByte54IfFlag08)
- Assinatura: `void f(int obj, u16 *out)`
- O que faz: se obj+0x50 bit 0x08: escreve o byte obj+0x54 em *out e limpa o bit 0x08.
- Família com 0x080733BC, 0x080734F4: saídas gated por bit de flag em obj+0x50 (padrão de "emitir e limpar").
- Confiança: confirmado (o que faz); nome é chute.

## 0x080733BC FUN_080733bc (chute: nome sugerido Obj_EmitScaledWordIfFlag02)
- Assinatura: `void f(int obj, u16 *out)`
- O que faz: se bit 0x02 de obj+0x50: calcula (obj+0x40 halfword * byte obj+0x66) & DAT_08073400, grava em obj+0x4A e
  em *out, e troca bit 0x02 por bit 0x01.
- Confiança: confirmado (o que faz); nome é chute.

## 0x080734F4 FUN_080734f4 (chute: nome sugerido Obj_EmitByte69WithFlagBit15IfFlag01)
- Assinatura: `void f(int obj, u16 *out)`
- O que faz: se bit 0x01 de obj+0x50: *out = (byte obj+0x69) | 0x8000 e limpa o bit 0x01.
- Confiança: confirmado (o que faz); nome é chute.

## 0x08073AB8 FUN_08073ab8 (chute: nome sugerido Table_WriteRecord12Bytes)
- Assinatura: `void f(int idx, u32 v)`
- O que faz: grava um registro de 12 bytes na tabela DAT_08073ad8 no índice idx: [0]=v, [1]=word da tabela
  DAT_08073adc no índice idx-1, [2]=constante DAT_08073ae0.
- Confiança: confirmado (o que faz); nome é chute.

## 0x08073B2C FUN_08073b2c (chute: nome sugerido Slot_SetAttrMode440)
- Assinatura: `void f(int idx)`
- O que faz: obtém ponteiro (tabela DAT_08073b5c, índice idx-1) e grava halfword: (K1 & valor) | K2; depois
  (K1 & valor) | 0x440, onde K1=DAT_08073b60. Padrão de escrita de atributo de sprite (OAM attr). Duas escritas
  seguidas (a primeira é sobrescrita pela segunda) - conferir se é intencional.
- Confiança: provável (atributo de sprite); nome é chute.

## 0x0807DD08 FUN_0807dd08 (chute: nome sugerido SoftF64_ZeroTestRoundtrip)
- Assinatura: `void f(u32 lo, u32 hi)`
- O que faz: desempacota f64 com sf_unpack, calcula (resultado == 0) numa variável local e reempacota com sf64_pack.
  O resultado do teste não é usado (código morto aparente).
- Chama: sf_unpack, sf64_pack. Faixa soft-float: pode ser nomeada em bloco (SoftF64_) quando as outras forem lidas.
- Confiança: provável (soft-float); nome é chute.

## 0x08302DF8 FUN_08302df8 (sugestão: não é função; dado)
- Assinatura: n/a
- O que faz: Ghidra criou função em região de dados: uma instrução de coprocessador seguida de halt_baddata. Não é código
  executável útil; provavelmente dado ou literal fora do fluxo.
- Confiança: provável (dado). Não renomeado; sugiro ignorar ou marcar como dado no Ghidra.
