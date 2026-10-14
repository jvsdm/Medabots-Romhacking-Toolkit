# FR3: pendentes de FL5, FL6, FL8 e chamadas sem nome

Ghidra MCP usado sem salvar (nomes aplicados na sessão; ver FR3_simbolos.csv). Ordem de processamento: lote FL5, FL6, FL8, depois chamadas sem nome.

## 0x08070914 Scene_ClearVramAndStartSong0
- Assinatura: `void Scene_ClearVramAndStartSong0(void)`
- O que faz: monta três transferências por DMA (padrão de registradores de DMA via ponteiro): preenche VRAM em 0x06000000 e uma faixa de paleta em 0x05000000 a partir de um zero local. Depois pede a música 0 (`snd_song_request(0)`), grava REG_DISPCNT=0x60, ajusta dois pares de 16 bits, zera 4 registradores de BG e marca o estado da cena em 3.
- Chama: `snd_song_request`. Chamada por: não verificado.
- Memória: REG_DISPCNT; ponteiros de literal pool para DMA3 e BG (não resolvidos); estado da cena (escrita 3).
- Para o Lua: porta como "limpar tela e começar música 0"; o DMA não precisa.
- Confiança: provável (padrão de DMA e de cena; destinos pelo literal pool não resolvidos).

## 0x08071460 Scene_InitObjectRenderSlots
- Assinatura: `void Scene_InitObjectRenderSlots(void)`
- O que faz: inicializa slots de render de objeto com `Map_InitializeObjectRenderSlot` (2 slots iniciais, 4 em laço com passo de 0x40 bytes, mais 4 slots de 0x1A em outro laço), grava posições e tamanhos em um array de objetos, e liga uma animação por `enable_anim4(0x18, 0x3F, 0xC)`.
- Chama: `Map_InitializeObjectRenderSlot`, `enable_anim4`. Chamada por: não verificado.
- Memória: array de objetos a partir de DAT_080715D8 (campos +0x2, +0x4, +0xB, +0x2C4, +0x2CB etc.; escritos por ponteiro).
- Para o Lua: porta como criação de sprites de cena (posições e animação fixas).
- Confiança: provável (laços e campos claros; nomes de campo ainda sem tabela).

## 0x08072080 Palette_FadeStepWithSfx
- Assinatura: `void Palette_FadeStepWithSfx(void)`
- O que faz: lê o contador de fade em +0x11 da struct de estado. Se for menor que 0x41, calcula o peso (|x-0x20|) e reescala as 256 cores BGR555 de uma paleta de origem; no passo 0x20 chama `slot_ptr_stub(0)` e no passo 0x14 toca o SFX 0x51; depois copia a paleta para o buffer de destino (dois DMAs de paleta). Se o contador passou de 0x40, toca o SFX 10 e incrementa +0x10. Botão A (bit 1 de entrada) avança o contador de fade.
- Chama: `slot_ptr_stub`, `snd_sfx_request`. Chamada por: não verificado.
- Memória: struct de estado em DAT_080720A4 (+0x10, +0x11); paleta de origem por literal pool; buffer de destino em DAT_08072174.
- Para o Lua: porta como fade de paleta com sons nos passos 0x14 e 0x20; o DMA não precisa.
- Confiança: provável (fórmula de escala confirmada; significado dos passos 0x14/0x20 pelo código, não pelo jogo).

## 0x0807D48C SoftFloat64_AddWrapper
- Assinatura: `void SoftFloat64_AddWrapper(u32 a_lo, u32 a_hi, u32 b_lo, u32 b_hi)`
- O que faz: desempacota os dois doubles (`sf_unpack` x2), soma com `sf64_add_core` e empacota com `sf64_pack`. Sequência igual à de `SoftFloat_Add64` (0x0807D4BC, já nomeada); pode ser entrada duplicada ou variante de chamada. Não comparei os chamadores.
- Chama: `sf_unpack`, `sf64_add_core`, `sf64_pack`. Chamada por: não verificado.
- Memória: só pilha.
- Para o Lua: não precisa (soft-float do compilador; Lua já tem float).
- Confiança: confirmado (a sequência está no código); nome de wrapper é provável.

## 0x0807DBCC SoftFloat64_CompareSafe
- Assinatura: `u32 SoftFloat64_CompareSafe(u32 a_lo, u32 a_hi, u32 b_lo, u32 b_hi)`
- O que faz: desempacota os dois doubles; se algum tem classe < 2 (zero/especial) devolve 1; senão devolve `sf64_cmp_core` (comparação -1/0/1). Mesma lógica de `SoftFloat64_Compare` (0x0807DA50, FL10) e de `Math_Sf64CompareSafe` (0x0807DB34, FL3): há pelo menos três entradas para o mesmo wrapper.
- Chama: `sf_unpack` x2, `sf64_cmp_core`. Chamada por: não verificado.
- Memória: só pilha.
- Para o Lua: não precisa.
- Confiança: confirmado (lógica lida); nome provável.

## 0x0807DD08 SoftFloat64_TestZeroPacked
- Assinatura: `void SoftFloat64_TestZeroPacked(u32 lo, u32 hi)`
- O que faz: desempacota um double e grava (valor == 0) numa variável local, mas essa variável não é devolvida; depois chama `sf64_pack`. O decompile não mostra saída útil: o efeito real está incompleto ou é o teste de zero usado só como flag.
- Chama: `sf_unpack`, `sf64_pack`. Chamada por: não verificado.
- Memória: só pilha.
- Para o Lua: não precisa.
- Confiança: chute (nome e efeito; conferir o disassembly antes de usar). Não renomeado no Ghidra.

## 0x08068A84 Palette_CrossfadeStep
- Assinatura: `int Palette_CrossfadeStep(void)` (devolve 1 quando o progresso chega ao fim, 0 caso contrário)
- O que faz: se o progresso (`*DAT_08068aa0`) é igual a divisor*0x20, devolve 1. Senão, só atualiza se o resto da divisão por `divisor` é 0: calcula peso = quociente e 0x20 - peso, mistura cada uma das cores de uma paleta de origem com a cor-alvo (BGR555) e envia o resultado à paleta (DMA para 0x05000000).
- Chama: `aux_mod_s32`, `aux_div_s32`. Chamada por: não verificado.
- Memória: progresso em DAT_08068AA0; divisor em DAT_08068AA4; buffer em DAT_08068B5C; paleta de origem e cor-alvo por literal pool.
- Para o Lua: porta como crossfade de paleta em passos discretos; o caso "resto != 0" não atualiza (comportamento a confirmar com o jogo).
- Confiança: provável (fórmula e retorno lidos; semântica de progresso/divisor inferida).

## 0x0806ED08 Scene_InitLz77GraphicsAndTilemaps
- Assinatura: `void Scene_InitLz77GraphicsAndTilemaps(void)`
- O que faz: descomprime seis blocos LZ77 direto para VRAM (`bios_lz77_vram`), configura três transferências por DMA (paleta/OAM), zera e preenche variáveis locais de tilemap e chama `blit_rect_tilemap` duas vezes (retângulos 0x1F x 10 x 0xC e 0x1E x 0x14 x 0x14). Depois cria o slot de render 0 (`Map_InitializeObjectRenderSlot`), liga o efeito de tela `fx_screen_setup_a(2,1,0)` e marca o estado da cena em 3.
- Chama: `bios_lz77_vram`, `blit_rect_tilemap`, `Map_InitializeObjectRenderSlot`, `fx_screen_setup_a`. Chamada por: não verificado.
- Memória: VRAM 0x06000000; estado da cena (escrita 3); campos do slot de render (0x90, 0x40).
- Para o Lua: porta como carregar gráficos e mapas de tela (ordem e retângulos); o LZ77 e o DMA não precisam.
- Confiança: provável (chamadas e constantes lidas; nomes de dados pelo literal pool não resolvidos).

## 0x08070D74 Scene_InitNineRenderSlotsAndFx
- Assinatura: `void Scene_InitNineRenderSlotsAndFx(void)`
- O que faz: transferências por DMA para paleta/OAM e grava posições de 7 objetos em um array de objetos; cria nove slots de render (`Map_InitializeObjectRenderSlot`, ids 6, 7, 8, 9, 2, 1, 3, 4, 5, base 0xC800), ajusta campos de posição e de frame de cada um; chama `fx_screen_setup_a(4,2,0)`, liga as camadas de BG em REG_DISPCNT (|= 0x1E00) e marca o estado em 1.
- Chama: `Map_InitializeObjectRenderSlot`, `fx_screen_setup_a`. Chamada por: não verificado.
- Memória: array de objetos (DAT_08070FE0, campos +0x42..+0x244); REG_DISPCNT; estado em DAT_0807102C.
- Para o Lua: porta como montar a cena com nove sprites e liga as camadas.
- Confiança: provável (chamadas e campos lidos; ordem e significado dos ids de slot pelo código).

## 0x080722B0 Preview_StepSfxSequenceAndMotion
- Assinatura: `u32 Preview_StepSfxSequenceAndMotion(void)` (devolve 1 quando a animação de motion termina)
- O que faz: copia 9 bytes de uma tabela de sequência para a pilha; ajusta quatro campos de 16 bits (=200) de uma struct de tela; faz DMA; monta a camada de sprites de preview com `BuildCompositePreviewSpriteLayer`; a cada passo lê o par (tipo, valor) da sequência: tipo 1 toca o SFX do valor; tipo 0 avança um contador até o valor e então avança o índice. Depois roda `MotionPlayStep(0, 0x2C, 0)`; quando termina, zera o byte +0x10 da struct e devolve 1.
- Chama: `lib_memcpy`, `BuildCompositePreviewSpriteLayer`, `snd_sfx_request`, `MotionPlayStep`. Chamada por: não verificado.
- Memória: tabela de sequência em DAT_08072354; struct de tela em DAT_0807236C (+0x10, +0x11, +0x12); campos 0x84/0xC4/0x104/0x144 em DAT_08072358.
- Para o Lua: porta como sequência de sons e de movimento de preview.
- Confiança: provável (fluxo e chamadas lidos; significado da struct pelo uso).

## 0x0807D79C SoftFloat64_Divide
- Assinatura: `void SoftFloat64_Divide(u32 a_lo, u32 a_hi, u32 b_lo, u32 b_hi)`
- O que faz: desempacota os dois operandos (`sf_unpack` x2); trata NaN/zero/infinito por classe (retorna a classe especial pelo ponteiro); no caso normal calcula sinal = XOR, depois divisão longa de mantissa de 64 bits (laço de deslocamento e subtração com arredondamento no bit 0x80) e empacota com `sf64_pack`.
- Chama: `sf_unpack` x2, `sf64_pack`. Chamada por: não verificado.
- Memória: só pilha.
- Para o Lua: não precisa.
- Confiança: provável (laço de divisão e XOR de sinal lidos; tratamento de classe não confirmado nos casos).

## 0x08037830 Fight_FindActiveBotInSideWithGroup
- Assinatura: `int Fight_FindActiveBotInSideWithGroup(int side_base, int attacker_bot)`
- O que faz: começa num slot aleatório entre 0 e 2 do lado (`Fight_RandomSlotFromTable16`), escolhe a direção do passo com `randbyte` e testa até 3 bots: retorna o primeiro com +0x40 != 0 (ativo) e +0x91 igual ao de `attacker_bot`. Sem achado, devolve 0xFF. Retorna o índice absoluto do bot (base + slot).
- Chama: `Fight_RandomSlotFromTable16`, `randbyte`. Chamada por: `Fight_SetOpponentBaseAndAdvanceScript` (0x08034B68, FL10) e o escolhedor de alvo (FL9).
- Memória: blocos de bot em 0x03000590 + i*0x100 (lidos: +0x40 ativo, +0x91 grupo; a base exata vem de DAT_08037878).
- Para o Lua: porta como escolher alvo vivo do mesmo grupo, com ordem aleatória; 0xFF = sem alvo.
- Confiança: provável (lógica e retorno lidos; significado de +0x91 como "grupo" ainda inferido de R2).

## 0x08003300 Screen_LoadSpriteRowA
- Assinatura: `void Screen_LoadSpriteRowA(void)`
- O que faz: descomprime uma imagem (`decompress_and_store`) para VRAM; grava dois registros de OBJ: um com os valores de `DAT_08003354`, e um de OBJ em x=0x88 e y = 8 + 0x10*byte[+0x8E] do estado de tela, com tamanho 0xE e prioridade/flag em +0xB. Termina com `Slot_SetupFromRecordList` sobre a lista de registros em DAT_08003370.
- Chama: `decompress_and_store`, `Slot_SetupFromRecordList`. Chamada por: FL9 e FL10 (`FUN_08003300` aparece no código de ambos; provável tela de resultado).
- Memória: estado de tela em DAT_0800336C (+0x8E); OBJ em DAT_08003364; imagem em DAT_0800334C.
- Para o Lua: porta como desenhar a faixa de sprite A na linha calculada pelo índice da tela.
- Confiança: provável (fluxo e campos lidos; ver R0: "OBJ em (136,y)").

## 0x08003904 Screen_DecompressRowImageByIndex
- Assinatura: `void Screen_DecompressRowImageByIndex(void)`
- O que faz: escolhe a imagem pelo byte +0x8F do estado de tela: 0 usa a imagem A (DAT_08003924), 1 usa a imagem B (DAT_08003934), 2 ou mais usa a imagem C (DAT_0800391C). Depois chama `decompress_and_store` com a imagem escolhida e o destino DAT_08003938.
- Chama: `decompress_and_store`. Chamada por: não verificado (é o caso "escolhe imagem por byte 0x8F" de R0).
- Memória: estado de tela em DAT_08003918 (+0x8F); tabela de ponteiros de imagem por literal pool.
- Para o Lua: porta como "escolher a imagem da faixa pelo índice".
- Confiança: provável (decisão por byte lida; conteúdo das imagens não conferido).

## 0x08048BB8 Map_ResetAffineScaleBlock
- Assinatura: `void Map_ResetAffineScaleBlock(void)`
- O que faz: zera dois inteiros de 32 bits em +0x10/+0x14 e reinicia um bloco afim: escala 0x100 (1.0) em +0x8, +0xE, +0x32, +0x34; zera +0xA, +0xC, +0x30, +0x36, +0x2C, +0x2E. É um reset de matriz identidade.
- Chama: nada. Chamada por: `Map_ResetRenderAffineState` (0x08049318, faz o reset completo; FL9 mostra os dois em sequência).
- Memória: bloco de estado afim em DAT_08048BDC.
- Para o Lua: não precisa (matriz afim do hardware).
- Confiança: provável (escrita lida; ligação com `Map_ResetRenderAffineState` pelo FL9).

## 0x08047850 (sem nome; não renomeado)
- Assinatura: `void FUN_08047850(int idx)`
- O que faz: se 0 <= idx < 4, grava 1 em dois bytes de flag (DAT_0804786C e DAT_08047870) e 5 em DAT_08047874. Fora dessa faixa, não faz nada. O FL10 chama com idx=1 logo depois de pedir a música 0.
- Chama: nada. Chamada por: FL10 (`FUN_08047850(1)`, logo após `snd_song_request(0)`, FL10.md ~linha 370). Outros chamadores: não verificado.
- Memória: três bytes em DAT_0804786C/70/74 (escrita).
- Para o Lua: depende de o significado dos flags ser conhecido; por enquanto não portar.
- Confiança: chute (o efeito dos flags e da fase 5 não foi identificado). Não renomeado.

## 0x08066D8C PointRing_StepFrame
- Assinatura: `void PointRing_StepFrame(void)` (não lê argumentos)
- O que faz: avança um efeito de anel de 72 pontos a cada frame. Mantém um timer de rotação (+0x48 e +0x49): quando zera, sorteia novo raio (+0x48 via `randbyte`) e recalcula; senão reconstrói os pontos com `Ring_BuildPointsFromRadius`. Move os ângulos de três eixos em direção ao alvo com `Byte_EaseTowardTarget` (limite 0x7F) e recalcula os ângulos de 16 bits. Cada 256 frames sorteia um modo (+0x7E, 0..3) que muda as velocidades (+0x86..+0x88), limitadas a ±16. Para cada um dos 72 pontos compõe a matriz (`Affine_ComposeThreeTransforms`, a cada 18 pontos) e grava x/y projetados (>>8) em uma tabela de 0x40 bytes por ponto.
- Chama: `randbyte`, `Ring_BuildPointsFromRadius`, `Byte_EaseTowardTarget`, `Affine_LoadIdentityBuffer`, `Affine_ComposeThreeTransforms`, `aux_mod_s32`, `aux_div_s32`. Chamada por: não verificado.
- Memória: estado do efeito em DAT_08066DD4 (+0x48, +0x49), DAT_080670C4 (ângulos +0x80..+0x9E), tabela de pontos em DAT_080672F0 (stride 0x40, campos +2/+4).
- Para o Lua: porta como animação de anel girando (é o que o código faz); não depende de hardware.
- Confiança: provável (mecânica lida nas duas leituras de ângulo e pontos; uso no jogo chute). Renomeado como provável. Obs.: a suposição do FL10 de que este callee é "copia a string" do `Text_TypewriterRowStep` (0x080678D4) NÃO se sustenta: o corpo não lê argumentos nem string, e a rotina é de efeito de pontos.
