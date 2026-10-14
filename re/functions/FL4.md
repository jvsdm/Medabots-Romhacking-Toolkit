# FL4 — documentação de funções (faixa 4, 41 alvos)

Gerado a partir de `FL4_lista.txt`. Nomes renomeados no Ghidra (sem commit, Ghidra não salvo).
Tamanhos da lista são "aprox. ate prox. funcao": o tamanho real vem do `get_function_by_address`.

## 0x080010B4 Gfx_AffineMatrixFromAngle
- Assinatura: `void Gfx_AffineMatrixFromAngle(int angle, uint idx, int dx, int dy, int scaleA, int scaleB)` (Ghidra: FUN_080010b4, 0x080010B4-0x08001173)
- O que faz: lê dois valores de uma tabela seno/cosseno (índice `idx & mask`, par em +0x100) e divide por `param_1` via `bios_div` (0x0807CD2C) para montar uma matriz 2x2 (pa, pb, pc, pd) num buffer de afim; depois calcula a translação (dx/dy) e grava nos halfwords de um segundo buffer.
- Chama: `bios_div` (0x0807CD2C). Chamada por: ainda não rastreado.
- Memória: lê tabela de seno (ponteiro em 0x08001174 via literal). Escreve buffers de matriz de afim apontados por 0x0800117C e 0x08001180.
- Para o Lua: não precisa (cálculo de sprite de rotação/escala; no PC basta rotação própria).
- Confiança: provável (estrutura de matriz afim pela forma das gravações).

## 0x08002554 Dispatch_CallIndexedHandler
- Assinatura: `void Dispatch_CallIndexedHandler(void)` (Ghidra: FUN_08002554, 0x08002554-0x08002569)
- O que faz: lê um índice de RAM (literal 0x08002570) e chama o ponteiro `table[idx]` (tabela no literal 0x0800256C) via `CallViaR0`. Wrapper de despacho de callback.
- Chama: a função da tabela (indireto). Chamada por: ainda não rastreado.
- Memória: lê índice (RAM via literal 0x08002570) e tabela de ponteiros (ROM/RAM via 0x0800256C).
- Para o Lua: não precisa se o handler correspondente for portado; o despacho vira um `handlers[idx]()`.
- Confiança: confirmado (estrutura de chamada); o conteúdo da tabela ainda não identificado.

## 0x08002D48 Gfx_BgRectWipeStep
- Assinatura: `void Gfx_BgRectWipeStep(void)` (Ghidra: FUN_08002d48, 0x08002D48-0x08002DDB)
- O que faz: passo de animação por frame de uma transição de fundo. Copia um retângulo do mapa BG com offset e preenche outra faixa (`BgMap_CopyRectWithOffset`, `BgMap_FillRect16`), decrementando o contador em +0x88. Quando zera: descompacta dados (`decompress_and_store`), copia as paletas para o hardware, zera a flag +0x84, dispara `snd_fade_request(0x10)` se +0x8E == 0 e incrementa um contador global.
- Chama: `BgMap_CopyRectWithOffset`, `BgMap_FillRect16`, `decompress_and_store`, `CopyPaletteBuffersToHardware`, `snd_fade_request`. Chamada por: ainda não rastreado.
- Memória: struct de estado (base em literal 0x08002DDC; campos +0x84, +0x88, +0x8E); contador global (literal 0x08002DF0).
- Para o Lua: porta como animação de transição (wipe de fundo); o efeito é visual.
- Confiança: provável (wipe/fade de tela pelo padrão de contador + fade de som).

## 0x08004DB4 Battle_SlotDrawStep
- Assinatura: `void Battle_SlotDrawStep(void)` (Ghidra: FUN_08004db4, 0x08004DB4-0x08004DE1; a lista dizia 10232 bytes, que é o tamanho aproximado até a próxima função, não o desta)
- O que faz: zera o campo +6 de um struct (literal 0x08004DE4), chama `tick_par_contador_b`, `desenha_slot_batalha_5` e `Parts_CountFilledSlots`, copia as paletas para o hardware, zera a flag +0x84 e incrementa um contador global. Passo de quadro do desenho dos slots de batalha.
- Chama: `tick_par_contador_b`, `desenha_slot_batalha_5`, `Parts_CountFilledSlots`, `CopyPaletteBuffersToHardware` (nomes já vindos de outra fonte). Chamada por: ainda não rastreado.
- Memória: struct em 0x08004DE4 (+6), struct em 0x08004DE8 (+0x84), contador global 0x08004DEC.
- Para o Lua: porta como o desenho dos slots de batalha por frame (ver `Battle_*` de fight_information).
- Confiança: provável.

## 0x0800B858 Gfx_BgFillRowAndCount
- Assinatura: `void Gfx_BgFillRowAndCount(void)` (FUN_0800b858, 0x0800B858-0x0800B883)
- O que faz: preenche uma faixa do mapa BG (`BgMap_FillRect16`, 16 de largura, linha 0xC, altura 8) com um valor vindo de literal e incrementa um contador de byte (base 0x0800B888 + offset 0x0800B88C).
- Chama: `BgMap_FillRect16`. Chamada por: ainda não rastreado.
- Memória: literal 0x0800B884 (valor de preenchimento); contador de byte (0x0800B888 + 0x0800B88C).
- Para o Lua: porta como um passo de preenchimento de faixa de fundo.
- Confiança: provável (passo de animação de uma linha; destino do contador não identificado).

## 0x0800CE90 Menu_InfoPanelStep
- Assinatura: `void Menu_InfoPanelStep(void)` (FUN_0800ce90, 0x0800CE90-0x0800D147)
- O que faz: passo por frame de uma tela de seleção com painel de informações. Lê entrada (bits de `*0x0800CED0`) para alternar um estado (+0x34 0/1), redesenha duas strings (`Mem_CopyBytes` + `Text_LayoutGlyphSprites`) conforme o estado, move um índice de cursor (+0x35+estado, ciclo de 0x1D ou 4) com o D-pad, formata dois números de 6 dígitos (`Num_FormatDecimal6Glyphs`) a partir de bytes do struct e, se a flag &2 estiver ativa, limpa os tiles; senão, carrega gráfico (`decompress_and_store`) e preenche entradas de sprite.
- Chama: `Mem_CopyBytes`, `Text_LayoutGlyphSprites`, `Tile_ClearSlots`, `Num_FormatDecimal6Glyphs`, `decompress_and_store`. Chamada por: ainda não rastreado.
- Memória: struct de estado em 0x0800CED4 (+0x34, +0x35, +0x9E); entrada em 0x0800CED0, 0x0800CEF0, 0x0800CF6C, 0x0800D0DC; OAM/sprite em 0x0800D160.
- Para o Lua: porta como tela de menu com cursor e dois valores numéricos.
- Confiança: provável (tela de menu; o significado exato dos dois números não identificado).

## 0x0800E4F8 Menu_PartsDetailDispatch
- Assinatura: `void Menu_PartsDetailDispatch(void)` (FUN_0800e4f8, 0x0800E4F8-0x0800E639)
- O que faz: chama `PartsGrid_DrawLargeDisplayIfOwned`, limpa 0x80 tiles e despacha, por tabela de ponteiros (0x0800E524), um handler conforme o estado em `struct+0x3C` (valores 0..4; acima de 4 não faz nada).
- Chama: `PartsGrid_DrawLargeDisplayIfOwned`, `Tile_ClearSlots`, handler indireto da tabela. Chamada por: ainda não rastreado.
- Memória: struct de estado em 0x0800E520 (+0x3C); tabela em 0x0800E524.
- Para o Lua: porta como dispatcher de estados da tela de peças.
- Confiança: provável (dispatcher de 5 estados; handlers não identificados).

## 0x08010B54 Battle_GaugeTickStep
- Assinatura: `void Battle_GaugeTickStep(void)` (FUN_08010b54, 0x08010B54-0x08010B9B)
- O que faz: incrementa um valor de 16 bits em direção a alvo (`Fight_IncrementToTarget16`, par em 0x08010B7C/0x08010B80). Se o byte +0x9C de um struct (0x08010B84) for 1, incrementa um segundo valor (+2) e toca a música 0xE; senão toca 0xF. Incrementa um contador (0x08010B9C).
- Chama: `Fight_IncrementToTarget16`, `snd_song_request`. Chamada por: ainda não rastreado.
- Memória: struct em 0x08010B84 (+0x9C); valores de 16 bits 0x08010B7C (+2); contador 0x08010B9C.
- Para o Lua: porta como animação de barra (ex.: gauge/HP) com trilha sonora.
- Confiança: provável (passo de barra com contagem em direção a alvo; ver `Fight_IncrementToTarget16`).

## 0x0801C1C0 Fight_SpawnHitEffect
- Assinatura: `void Fight_SpawnHitEffect(void)` (FUN_0801c1c0, 0x0801C1C0-0x0801C229)
- O que faz: monta um efeito de sprite de ataque no contexto de animação (0x0801C22C): zera +0xDC, grava posição X (+0xC0) e Y (+0xC4 = tabela + -0xC) a partir de uma tabela indexada pelo índice de bot (byte em fight+0x87, passo 0x100, base +0x1742), define +0xCA=6, +0xC6 = constante, +0xCB=0x81 (flags), toca SFX 0x2F e incrementa o contador de fight+0x9C.
- Chama: `snd_sfx_request(0x2F)`. Chamada por: ainda não rastreado (provável ataque/golpe).
- Memória: contexto de animação em 0x0801C22C (+0xC0..+0xCB); tabela de posição em +0x1742/+0x1744 do mesmo bloco; fight em 0x0801C234 (+0x87, +0x9C).
- Para o Lua: porta como disparo de efeito de golpe (posição + som).
- Confiança: provável (campos de sprite e SFX 0x2F; ligação com fight_information pelo índice do bot).

## 0x0801C4E0 Fight_AnimateBotEffects
- Assinatura: `void Fight_AnimateBotEffects(void)` (FUN_0801c4e0, 0x0801C4E0-0x0801C4F7)
- O que faz: lê o índice de bot em fight+0x87 (literal 0x0801C4F8) e chama `Fight_AnimateEntryFrames` e `Fight_AnimateSideEffectSprites8` com ele. Passo por frame dos efeitos de um bot.
- Chama: `Fight_AnimateEntryFrames`, `Fight_AnimateSideEffectSprites8`. Chamada por: ainda não rastreado.
- Memória: lê fight+0x87 (base no literal 0x0801C4F8).
- Para o Lua: wrapper trivial; porta junto com as duas funções chamadas.
- Confiança: confirmado (chamadas diretas com o mesmo índice).

## 0x080382F4 Anim_SlotSlideStep
- Assinatura: `void Anim_SlotSlideStep(void)` (FUN_080382f4, 0x080382F4-0x08038363)
- O que faz: passo de deslize de um slot de objeto (stride 0x40, base 0x0803831C, índice em byte de 0x08038320+0x08038324). Se +0x2D == 'Z' (0x5A, fim), incrementa +0x29; senão soma ±0x10 ao X (+0x18) conforme o bit 0 de fight+0xF8 (direção), aplica máscara (0x08038364) e incrementa o contador de outro slot (índice em 0x08038368).
- Chama: nenhuma (só memória). Chamada por: ainda não rastreado.
- Memória: slots em 0x0803831C (stride 0x40: +0x18 X, +0x29, +0x2D); fight+0xF8; contador do slot de destino (+0x2D).
- Para o Lua: porta como movimento de sprite por frame (deslize com direção).
- Confiança: provável (interpretação do marcador 'Z' como fim de animação).

## 0x0803C5F8 Anim_SlotFrameStep
- Assinatura: `void Anim_SlotFrameStep(void)` (FUN_0803c5f8, 0x0803C5F8-0x0803C67F)
- O que faz: passo por frame de um slot de animação (stride 0x40, base 0x0803C618). Se o contador de 16 bits +0x16 do slot for 0, marca o bit 0x8000 do slot e roda o script do slot (`Anim_RunSlotScript`, script escolhido por byte +0x2D via tabela em 0x0803C664); senão decrementa +0x16. Depois, o contador +0x2B: se zero, chama `Anim_AdvanceSlotTileOffset`; senão decrementa.
- Chama: `Anim_RunSlotScript`, `Anim_AdvanceSlotTileOffset`. Chamada por: ainda não rastreado.
- Memória: slots 0x0803C618 (stride 0x40: +0x00 flags, +0x16, +0x2B, +0x2D); tabela de scripts 0x0803C664; índices em 0x0803C61C/0x0803C620 e 0x0803C66C/0x0803C670.
- Para o Lua: porta como loop de animação de slot (delay + script + avanço de tile).
- Confiança: provável.

## 0x0803CE18 Anim_PaletteFadeStep
- Assinatura: `void Anim_PaletteFadeStep(void)` (FUN_0803ce18, 0x0803CE18-0x0803CEC9)
- O que faz: passo por frame de um fade de paleta sobre um slot (stride 0x40, índice em byte). Se +0x2B é 0, interpola duas cores (A e B, RGB15 montado de +0x2C/+0x2D/+0x2E) com `Palette_FadeRowsToColorA/B` usando a intensidade +0x2F; ao chegar a 0x10 zera +0x29 (fim). Senão decrementa +0x2B (atraso).
- Chama: `Palette_FadeRowsToColorA`, `Palette_FadeRowsToColorB`. Chamada por: ainda não rastreado.
- Memória: slots 0x0803CE3C (stride 0x40: +0x29, +0x2B..+0x2F); índice em 0x0803CE40/0x0803CE44; cores de destino nos literais 0x0803CEB8/0x0803CEBC.
- Para o Lua: porta como fade de paleta (flash de cor) por frame.
- Confiança: provável.

## 0x0803FB6C Gfx_PaletteFlushStep
- Assinatura: `void Gfx_PaletteFlushStep(void)` (FUN_0803fb6c, 0x0803FB6C-0x0803FB91)
- O que faz: se o byte de bloqueio em 0x0803FB84 for 0, copia os buffers de paleta para o hardware (`CopyPaletteBuffersToHardware`), zera +0x1C e avança o estado em +4; senão fixa o estado em 2.
- Chama: `CopyPaletteBuffersToHardware`. Chamada por: ainda não rastreado.
- Memória: bloco de estado em 0x0803FB84 (+0, +4, +0x1C).
- Para o Lua: não precisa (envio de paleta ao hardware); o PC aplica a paleta direto.
- Confiança: confirmado (chamada direta) no papel; o significado do bloqueio é provável.

## 0x08041920 Anim_SpawnSlotObject
- Assinatura: `void Anim_SpawnSlotObject(void)` (FUN_08041920, 0x08041920-0x080419BB)
- O que faz: cria um objeto num slot livre (stride 0x40, índice em +0x2B). Preenche X = `+0x2C*8+0x14`, Y = `+0x2D*8 + +0x2A*0x10 + 0x10`, valor fixo em +6, flags em +8/+9/+10 (=0xE)/+0xB (=0xFF) e +0x1C=0, marca `+0x84=1` no bloco de controle e incrementa +0x29.
- Chama: nenhuma. Chamada por: ainda não rastreado.
- Memória: slots em 0x080419BC (stride 0x40); controle em 0x080419C0 (+0x29, +0x2A..+0x2D, +0x84); constantes em 0x080419C4/0x080419C8.
- Para o Lua: porta como criação de sprite/objeto numa posição de grade.
- Confiança: provável.

## 0x0804F940 Event_MapObjectStepOpcode
- Assinatura: `void Event_MapObjectStepOpcode(void)` (FUN_0804f940, 0x0804F940-0x0804F9BF)
- O que faz: opcode de evento em duas fases. Fase 0: cria um objeto rastreado no tile (`Map_SpawnTrackedObjectAtTile` com 4 bytes de argumento vindos de 0x0804F954+offsets) e inicia um passo de objeto (`Map_StartObjectStep`); avança a fase. Fase 1: chama `Map_TickObjectStep`; quando retorna não zero, encerra o opcode com `Event_EndOpcode`.
- Chama: `Map_SpawnTrackedObjectAtTile`, `Map_StartObjectStep`, `Map_TickObjectStep`, `Event_EndOpcode`. Chamada por: tabela de opcodes de evento (provável).
- Memória: fase em 0x0804F954+0x0804F958; argumentos nos literais 0x0804F990..0x0804F99C e 0x0804F9A0/0x0804F9A4/0x0804F9C0.
- Para o Lua: porta como opcode de evento que espera um objeto de mapa terminar (ver `Event_EndOpcode`).
- Confiança: provável (chama `Event_EndOpcode`; a tabela de opcodes ainda não confirmada).

## 0x0805C128 Gfx_ScreenResetVram
- Assinatura: `void Gfx_ScreenResetVram(void)` (FUN_0805c128, 0x0805C128-0x0805C16F)
- O que faz: zera a VRAM (0x06000000) com `bios_cpufastset` (SWI 0x0C, valor 0 e tamanho do literal 0x0805C170), limpa 4 pares de halfwords (`Clear4PairsHalfwords`) e a tabela de halfwords da tela (`Screen_ClearHalfwordTable`), grava `REG_DISPCNT = 0x60` (OBJ ligado, mapeamento 1D), inicializa um registro de timer (`Obj_InitTimerRecord`) e incrementa um contador.
- Chama: `bios_cpufastset`, `Clear4PairsHalfwords`, `Screen_ClearHalfwordTable`, `Obj_InitTimerRecord`. Chamada por: ainda não rastreado.
- Memória: escreve VRAM; REG_DISPCNT (0x04000000); registro em literal 0x0805C174+0x50; contador em 0x0805C178.
- Para o Lua: não precisa (reset de hardware); o PC só redesenha a tela vazia.
- Confiança: confirmado (VRAM via CpuFastSet e DISPCNT).

## 0x0805CB20 Dispatch_CallArgsByIndex
- Assinatura: `void Dispatch_CallArgsByIndex(void)` (FUN_0805cb20, 0x0805CB20-0x0805CB44)
- O que faz: copia 6 palavras de um bloco (literal 0x0805CB48) para a pilha e chama `table[*0x0805CB4C]` (ponteiro escolhido por byte) via `CallViaR0`, passando o endereço dos argumentos. Despacho com argumentos empacotados.
- Chama: handler indireto (tabela não identificada). Chamada por: ainda não rastreado.
- Memória: bloco de 6 palavras em 0x0805CB48; índice em 0x0805CB4C.
- Para o Lua: porta como `handlers[idx](args)`, depois que a tabela for identificada.
- Confiança: confirmado (estrutura de despacho); conteúdo da tabela não identificado.

## 0x0805D5A4 Menu_ResetOnInput
- Assinatura: `void Menu_ResetOnInput(void)` (FUN_0805d5a4, 0x0805D5A4-0x0805D5C5)
- O que faz: se os 2 bits baixos do byte de entrada (0x0805D5C8) estão ativos, toca SFX 2, chama `Obj_ResetStateFields` e fixa o estado em 4 (0x0805D5CC).
- Chama: `snd_sfx_request`, `Obj_ResetStateFields`. Chamada por: ainda não rastreado.
- Memória: entrada em 0x0805D5C8; estado em 0x0805D5CC.
- Para o Lua: porta como transição de estado por tecla.
- Confiança: provável (ação de confirmação/cancelamento; quais teclas não confirmado).

## 0x0805DB84 Save_StepThenFlashOp
- Assinatura: `void Save_StepThenFlashOp(void)` (FUN_0805db84, 0x0805DB84-0x0805DBB5)
- O que faz: chama `StateMachine_DispatchStep` com o ponteiro em 0x0805DBB8+0x28. Quando ela retorna não zero: para a música (`snd_song_request(0)`) e o SFX (`snd_sfx_request(0)`), chama `Save_InvokeFlashOp` e zera um halfword no bloco apontado por 0x0805DBBC+2, incrementando o contador em 0x0805DBC0.
- Chama: `StateMachine_DispatchStep`, `snd_song_request`, `snd_sfx_request`, `Save_InvokeFlashOp`. Chamada por: ainda não rastreado.
- Memória: ponteiro de máquina de estados em 0x0805DBB8+0x28; bloco em 0x0805DBBC; contador em 0x0805DBC0.
- Para o Lua: porta como fim de uma etapa de salvamento (não grava de fato no PC; o save real fica em `Save_InvokeFlashOp`).
- Confiança: provável (a escrita em flash é a chamada `Save_InvokeFlashOp`, cujo nome já vem de outra fonte).

## 0x0805F104 Dispatch_StateStepCounter
- Assinatura: `void Dispatch_StateStepCounter(void)` (FUN_0805f104, 0x0805F104-0x0805F133)
- O que faz: chama `table[*0x0805F138]` via `CallViaR0` (passo de estado escolhido por byte); se o bit 0 de 0x0805F13C estiver ativo, incrementa dois halfwords de contador em 0x0805F140.
- Chama: handler indireto (tabela não identificada). Chamada por: ainda não rastreado.
- Memória: índice em 0x0805F138; flag em 0x0805F13C; contadores em 0x0805F140 (+0 e +2).
- Para o Lua: porta como despacho de estado com contador de quadros.
- Confiança: confirmado (estrutura); o conteúdo da tabela não identificado.

## 0x0805FA58 Menu_SaveLoadWaitStep
- Assinatura: `void Menu_SaveLoadWaitStep(void)` (FUN_0805fa58, 0x0805FA58-0x0805FAD3)
- O que faz: incrementa um contador de 16 bits; após 0x1D quadros e com o SFX 4 parado (`snd_sfx_query_playing`), chama `Site_RunSaveLoadOption(2)` até 4 vezes. Se todas dão zero, zera campos de estado e avança o estado; senão vai para o estado 0xC.
- Chama: `snd_sfx_query_playing`, `Site_RunSaveLoadOption`, `Obj_ResetStateFields`. Chamada por: ainda não rastreado.
- Memória: contador em 0x0805FA7C+2; estado em 0x0805FAB8 e 0x0805FAD8.
- Para o Lua: porta como espera de menu de salvar/carregar (a parte de I/O fica em `Site_RunSaveLoadOption`).
- Confiança: provável.

## 0x0806050C Save_WriteEepromAndFinish
- Assinatura: `void Save_WriteEepromAndFinish(void)` (FUN_0806050c, 0x0806050C-0x08060589)
- O que faz: se o SFX 4 não está tocando, zera um byte, troca um halfword, marca 1, copia dados com `bios_cpuset` para um buffer da pilha e grava na EEPROM com `lib_eeprom_write_verify(0, buf)` até 4 tentativas. Depois habilita bits em REG_IE, fixa o estado principal (gMainState) em 3 e outros estados em 2, 0 e 1.
- Chama: `snd_sfx_query_playing`, `bios_cpuset`, `lib_eeprom_write_verify`. Chamada por: ainda não rastreado.
- Memória: escreve EEPROM (via lib); REG_IE (ponteiro em 0x0806058C); gMainState (0x08060590).
- Para o Lua: porta como fim de gravação do save (no PC, o save é o arquivo da ferramenta, não a EEPROM).
- Confiança: confirmado (EEPROM write com verificação pelo nome da lib e loop de 4 tentativas); detalhes de estado provável.

## 0x080615D4 Menu_OptionCursorStep
- Assinatura: `void Menu_OptionCursorStep(void)` (FUN_080615d4, 0x080615D4-0x080617DD)
- O que faz: menu de opções com 4 itens. Atualiza o destaque de cada item conforme os bits de entrada (0x10/0x20/0x80 movem o cursor para 2/1/3 com SFX 2; 0x40 para 0; `Screen_HighlightOption4`), pisca o destaque a cada 8 quadros, A (bit 0) toca SFX 4 e marca as linhas (0x4000 de atributo), mudando o estado para 0xB; B (bit 1) toca SFX 5, limpa as linhas, liga REG_DISPCNT bit 0x200 e fixa o estado em 0x10. Se o cursor está em 4, o estado vai para 0xC.
- Chama: `snd_sfx_request`, `Screen_HighlightOption4`. Chamada por: ainda não rastreado.
- Memória: entrada em 0x08061610; cursor em byte +0x19 do estado (0x08061738 aponta); REG_DISPCNT; tabela de atributos de OAM/tela em 0x08061740 (stride 0x40).
- Para o Lua: porta como menu de 4 opções com cursor e confirmar/cancelar.
- Confiança: provável (lógica de cursor clara; ids de estado não rotulados).
