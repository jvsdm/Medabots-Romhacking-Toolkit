# G: lacunas espalhadas (lista G_lista.txt, 23 endereços)

Nenhum endereço da lista já tinha seção em R0..R4 (conferido por grep antes).
Quatro não são início de função no Ghidra (0x0805BB38, 0x080736BC, 0x080745DA, 0x0807DD58): uma linha cada, sem seção.
Confiança segue o formato de R0.md. Nomes do mtbe.csv reaproveitados quando batem.

## 0x0800508C Menu_DrawPartIconGrid
- Assinatura: `void Menu_DrawPartIconGrid(void)`
- O que faz: percorre 9 registros de 8 bytes; para cada um, desenha o bloco de ícone da peça (DrawPartIconTileBlock). Se a entrada tem peça e o id < 0x78, carrega as estatísticas (GetPartStats) e usa o índice; senão usa o ícone vazio (0xFF).
- Chama: DrawPartIconTileBlock, GetPartStats. Chamada por: não mapeado.
- Para o Lua: sim (grade de ícones de peça no menu).
- Confiança: provável.

## 0x08017CE8 Ui_TickBlinkCounter
- Assinatura: `void Ui_TickBlinkCounter(void)`
- O que faz: incrementa o contador de 16 bits +0x86 do bloco de UI e chama FUN_08017C50 com 1 ou 0 conforme o bit 3 (pisca de 8 em 8 quadros).
- Chama: FUN_08017c50. Chamada por: não mapeado.
- Para o Lua: sim (piscar de cursor/indicador).
- Confiança: provável.

## 0x08042B80 Speciality_IsRemotenessClass (sem nome) (revisto CH1: provável)
- Assinatura: `int f(int id)`
- O que faz: mapeia o id 0..7 para 0 ou 1 (0,1,4,5 viram 0; 2,3,6,7 viram 1); fora disso devolve o próprio valor. Não se sabe o significado do agrupamento.
- Chama: nada. Chamada por: não mapeado.
- Para o Lua: sim se a tabela de agrupamento for usada.
- Confiança: revisto CH1: provável. Lido no decompile: switch de 0..7 com saidas 0 e 1. O nome 'Speciality_IsRemotenessClass' veio do Ghidra e e palpite; significado nao fechado.

## 0x0804305C Sprite_WriteComposite4Objects
- Assinatura: `void Sprite_WriteComposite4Objects(int ...)` (11 argumentos)
- O que faz: escreve 4 objetos de sprite (2x2) na área de OAM do slot 0x40-byte (base 0x03000590 + slot*0x40 segundo o offset da pesquisa), com tile base, x/y, flip (param 9), paleta (param 10) e prioridade. É a mesma assinatura de CreateCompositeBattleSpriteObjects usada em R0b (0x08015960 e 0x08015A00).
- Chama: nada. Chamada por: 0x08015A00 e demais chamadas de CreateCompositeBattleSpriteObjects.
- Memória: escreve RAM OAM (shadow) em 0x03000590+.
- Para o Lua: sim (montagem do sprite do bot).
- Confiança: provável. ATENÇÃO: mtbe.csv tem CreateCompositeBattleSpriteObjects em 0x08042E58 (outro endereço): conferir se é o mesmo corpo ou um duplicado.

## 0x080432A0 Sprite_UpdateComposite4Motion
- Assinatura: `void Sprite_UpdateComposite4Motion(int slot, short x)`
- O que faz: atualiza 4 sprites de um grupo com um contador de fase (+0x26, +0x24) e um índice de 0..3 (+0x21); calcula a posição com CompositeSprite_ComputeOffsetWord (movimento em arco/órbita) e grava x/y de cada sprite.
- Chama: CompositeSprite_ComputeOffsetWord. Chamada por: 0x0806AAA4, 0x0806ADB0.
- Para o Lua: sim (animação do grupo de 4 sprites).
- Confiança: provável.

## 0x0804361C Slot_FindNextActiveWrap
- Assinatura: `int Slot_FindNextActiveWrap(int start, int dir)`
- O que faz: percorre a tabela de 0x1F registros de 0x40 bytes a partir de start (dir 0 para frente, senão para trás) com wrap em 0 e 0x1E; devolve o primeiro com bit 0x80 ligado e bit 0x40 desligado, ou 0x1E se não achar.
- Chama: nada. Chamada por: não mapeado.
- Para o Lua: sim.
- Confiança: provável.

## 0x080437E0 Util_Are4ByteFieldsEqual
- Assinatura: `u32 Util_Are4ByteFieldsEqual(void)`
- O que faz: devolve 1 se os bytes +0x98, +0x99, +0x9A e +0x9B do bloco apontado por DAT_0804380C são todos iguais; 0 caso contrário.
- Chama: nada. Chamada por: não mapeado.
- Para o Lua: sim (predicado simples).
- Confiança: confirmado (código lido); o significado do bloco não.

## 0x0804384C Lookup_FindPartIndexById
- Assinatura: `u8 Lookup_FindPartIndexById(u8 id)`
- O que faz: se id < 0x1E, procura nos 9 registros de 8 bytes o primeiro com byte [0] > 1 e byte [2] == id; devolve o índice (0..8) ou 0xFF.
- Chama: nada. Chamada por: não mapeado.
- Para o Lua: sim.
- Confiança: provável.

## 0x08050168 Map_TickObjectAnimationTimes
- Assinatura: `void Map_TickObjectAnimationTimes(u8 obj)`
- O que faz: chama FUN_08051940(obj) para saber quantas vezes, e chama Map_TickObjectAnimation(obj) esse número de vezes.
- Chama: FUN_08051940, Map_TickObjectAnimation. Chamada por: não mapeado.
- Para o Lua: sim (avanço de animação de objeto no mapa).
- Confiança: provável.

## 0x0805B0D8 Shop_BuildAvailablePartList
- Assinatura: `void Shop_BuildAvailablePartList(void)`
- O que faz: zera o byte +2 de 0x78 registros de 8 bytes e reconstrói gShopItems: para cada peça de 0 a 0x77 da categoria atual (byte +0x19 do bloco de jogo) com quantidade disponível (byte [1] < byte [0]), grava o id, a quantidade restante, as estatísticas (GetPartStats) e um valor (byte +0xB x 100, provavelmente preço). Guarda o total no byte +0x20 do bloco.
- Chama: GetPartStats. Chamada por: não mapeado.
- Memória: lê o bloco de jogo (0x0203....) e a tabela de peças; escreve gShopItems e o contador.
- Para o Lua: sim (lista de peças da loja/troca).
- Confiança: provável.

## 0x0805B188 Shop_LoadPartIconFrame
- Assinatura: `void Shop_LoadPartIconFrame(u8 part, int sub, int slot)`
- O que faz: conforme sub (0 a 3), descomprime (decompress_and_store) um frame de gráfico da peça para a VRAM no endereço slot*0x80, usando as tabelas de ponteiros da ROM; sub 3 usa outra tabela. Depois grava em +0x6E..: o byte part+4 no slot.
- Chama: decompress_and_store. Chamada por: 0x0805B22C.
- Para o Lua: sim (carga do ícone da peça; o desenho é do port).
- Confiança: provável.

## 0x0805B22C Shop_DrawPartListPage
- Assinatura: `int Shop_DrawPartListPage(int mode)`
- O que faz: copia até 4 entradas de gShopItems para uma página, limpa 4 linhas de tilemap (Slot_ClearAttrMask e Screen_ClearTileRowHalfwords), desenha nome (Text_LayoutGlyphSprites) e quantidade/preço (Screen_DrawDecimalNumberShadowed e Num_FormatDecimalToGlyphs) de cada uma e chama Shop_LoadPartIconFrame. O modo 0 e o modo != 0 usam textos diferentes (compra ou venda). Ao fim desenha um valor de total (0x61, 0x13).
- Chama: Slot_ClearAttrMask, Screen_ClearTileRowHalfwords, Text_LayoutGlyphSprites, Screen_DrawDecimalNumberShadowed, Num_FormatDecimalToGlyphs, Shop_LoadPartIconFrame. Chamada por: não mapeado.
- Memória: lê gShopItems (RAM); escreve OAM e tilemap.
- Para o Lua: sim (página da loja/troca).
- Confiança: provável.

## 0x0805B5BC Menu_UpdateSpritePriorityByRange
- Assinatura: `void Menu_UpdateSpritePriorityByRange(u16 *attr)`
- O que faz: se o flag +0x12 do estado está ligado, seta o bit 0x4000 do atributo do sprite quando o valor +0x13 está fora da faixa [+0x11, +0x11+5); senão limpa o bit (máscara). Efeito: prioridade/ocultar do cursor.
- Chama: nada. Chamada por: não mapeado.
- Para o Lua: sim se o cursor existir no port.
- Confiança: provável.

## 0x0806A748 FUN_0806a748 (sem nome) (revisto CH1: provável)
- Assinatura: `void f(void)`
- O que faz: para 3 entradas, compara a ordem entre elas (byte de ordem e halfword +0x1A) e emite partes de sprite (sprite_part_emit) com prioridade 5 a 7 conforme o rank; a outra metade do laço emite com ids 5, 6, 7 para os slots de tilemap.
- Chama: sprite_part_emit. Chamada por: não mapeado.
- Para o Lua: sim se houver ordenação de profundidade.
- Confiança: revisto CH1: provável. Lido no decompile: comparacoes de rank e sprite_part_emit com 5, 6, 7 ou rank 5..8. Papel de ordenacao confirmado pela estrutura; nome segue FUN_.

## 0x0806A860 FUN_0806a860 (sem nome) (revisto CH1: provável)
- Assinatura: `void f(void)`
- O que faz: avança temporizadores de 3 bots (byte +0x1A da entrada) e, no limite (0x20), chama slot_spawn e zera; também decrementa o contador em +0x1A e controla a animação de um texto (bytes +0x27/+0x2A com wrap). Usa a área de tilemap dos marcadores.
- Chama: slot_spawn. Chamada por: não mapeado.
- Para o Lua: sim (ações temporizadas da luta).
- Confiança: revisto CH1: provável. Lido no decompile (grande, resumido): contadores, limite e slot_spawn. Papel de 'temporizador' pela estrutura; nome segue FUN_.

## 0x0806AA10 FUN_0806aa10 (sem nome) (revisto CH1: provável)
- Assinatura: `void f(void)`
- O que faz: para 3 entradas, incrementa um contador a cada quadro; quando passa o limite vindo de uma tabela (indexada por byte do modo), incrementa +0x1A do bloco do bot.
- Chama: nada. Chamada por: não mapeado.
- Para o Lua: sim se o contador existir.
- Confiança: revisto CH1: provável. Lido no decompile: laco de 3 entradas com comparacao contra byte da tabela e escrita em +0x1a. Papel de contador de bots pela estrutura.

## 0x0806AAA4 FUN_0806aaa4 (sem nome) (revisto CH1: provável)
- Assinatura: `void f(void)`
- O que faz: sequência de ações de 3 bots: quando o contador passa do limite, chama FUN_08043240, marca +0x2A, avança a fase (+byte) e, ao 3º passo, dispara slot_spawn e efeitos (snd_sfx_request 0x43 ou 0x44; 0x43 quando o slot já está ativo, 0x44 no caso de falha). Também move sprites (FUN_080432a0 e CompositeSprite_ComputeOffsetWord) e atualiza +0x1A de um bloco de posição.
- Chama: FUN_08043240, slot_spawn, snd_sfx_request, FUN_08048864, FUN_08069b38, FUN_080432a0. Chamada por: não mapeado.
- Para o Lua: sim (sequência de ataque/efeito dos bots), mas o detalhe pede leitura com a fase.
- Confiança: revisto CH1: provável. Lido no decompile (grande): sfx 0x43 e 0x44 confirmados no codigo; o restante do fluxo e resumo, nao verificado linha a linha.

## 0x0806ADB0 FUN_0806adb0 (sem nome) (revisto CH1: provável)
- Assinatura: `void f(void)`
- O que faz: para 3 bots, quando a fase +0x2A é 1 ou 2, chama FUN_080432a0; ao chegar em 0x18 (fase 1) ou 0x2C (fase 2), monta objetos compostos com CreateCompositeBattleSpriteObjects (tiles de 4 peças) e chama FUN_08043240 com 8 ou 0xC.
- Chama: FUN_080432a0, CreateCompositeBattleSpriteObjects, FUN_08043240. Chamada por: não mapeado.
- Para o Lua: sim (efeito de aparecer/sumir o sprite composto).
- Confiança: revisto CH1: provável. Lido no decompile: ramos por flag +0x2a, CreateCompositeBattleSpriteObjects e SetCompositeBattleSpriteAnimationSpeed com 8 ou 0xC. Papel de fases de sprite pela estrutura.

## 0x0807334C Snd_UpdateChannelFreqEnv
- Assinatura: `void Snd_UpdateChannelFreqEnv(int ch, short *out)`
- O que faz: se o flag 0x20 do canal estiver ligado, reseta o canal de hardware (snd_hw_channel_reset) e limpa o bit. Se o flag 2 estiver ligado, calcula a frequência (param +0x40 * +0x66 & máscara), grava em +0x4A, escreve em *out o byte de envelope de uma tabela (índice = freq >> 12) e limpa o flag 2.
- Chama: snd_hw_channel_reset. Chamada por: não mapeado.
- Para o Lua: não precisa (áudio do hardware; o port usa o próprio mixer).
- Confiança: provável.

## Sem função no Ghidra (uma linha cada)
- 0x0805BB38: sem função (literal pool ou dado; não documentado).
- 0x080736BC: sem função (literal pool ou dado; não documentado).
- 0x080745DA: sem função (literal pool ou dado; não documentado).
- 0x0807DD58: função desde o sync de 2026-10-14 (nome: FUN_0807dd58) (literal pool ou dado; não documentado).
