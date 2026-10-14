# R2a: funções 0x08043B7C..0x080455E0 (lista R2a_lista.txt, 34 entradas)

Todas renomeadas no Ghidra nesta rodada (nomes em inglês). Confiança indicada por seção. Nomes "provável" = papel
inferido pelo código de chamada e pelas escritas, sem confirmar o uso no fluxo do jogo.

## 0x08043B7C GfxBankA_LoadTilesAndPalette
- Assinatura: `void f(int bank, int palSel)`
- O que faz: descomprime o bloco `bank` (tabela de 0x10 bytes, em DAT_08043BD8) e carrega dois conjuntos de tiles (LoadTilesWithMalias). Copia 0x40 words de paleta (cpufastset) para BG palette 0x05000000 se `palSel == 0`, senão para o buffer DAT_08043BE8.
- Chama: decompress_and_store, LoadTilesWithMalias (x2), bios_cpufastset. Chamada por: não lido.
- Memória: ROM (tabelas em 0x08043BD8..0x08043BE8); escrita em VRAM de paleta 0x05000000 ou RAM.
- Para o Lua: "carregar gráficos de um banco" — não portar (VRAM).
- Confiança: confirmado (chamadas); papel de banco A: provável.

## 0x08043C08 GfxBankB_LoadTilesAndPalette
- Assinatura: `void f(int bank, int palSel)`
- O que faz: igual à FUN_08043B7C, mas com registros de 0x14 bytes (tabela em DAT_08043C64, 5 slots por banco) e 2 conjuntos de tiles nos slots 3 e 4.
- Chama: decompress_and_store, LoadTilesWithMalias (x2), bios_cpufastset. Chamada por: não lido.
- Memória: ROM (tabela em 0x08043C64..0x08043C74); VRAM de paleta.
- Para o Lua: não precisa (VRAM).
- Confiança: confirmado (código); papel: provável.

## 0x08043C94 Encounter_RollPercent0To99
- Assinatura: `void f(void)` (resultado vai para o retorno de `aux_div_s32`, nada devolvido aqui)
- O que faz: sorteia um byte (`randbyte()`), multiplica por 100 e divide por 0xFF (divisão com sinal 0x0807CD2C). Resultado: um valor 0..100, usado no limiar de encontro.
- Chama: randbyte, aux_div_s32. Chamada por: rotina de escolha de encontro/template (csv).
- Memória: nenhuma direta.
- Para o Lua: `math.random(0, 99)`-equivalente; o port usa o mesmo sorteio com o RNG do jogo.
- Confiança: confirmado. Nota: mtbe.csv diz `generate_random__`; o código chama `randbyte`. Ajustar a nota do csv.

## 0x08043CA8 Slots_ClearFourPairs
- Assinatura: `void f(void)`
- O que faz: zera (halfwords 0 e 2) quatro pares de ponteiros globais (PTR_DAT_08043CC4..08043CD0).
- Chama: nenhuma. Chamada por: FUN_08044BA8, FUN_08044C38.
- Memória: escrita em 8 halfwords de RAM apontados pelos literais 0x08043CC4..0x08043CD0.
- Para o Lua: "zerar 4 slots de posição" — porte trivial se precisar.
- Confiança: confirmado.

## 0x08043DF8 FxBlend_ApplyFromObjA
- Assinatura: `void f(void)`
- O que faz: lê o objeto global (DAT_08043E28). Se o byte +0x3C é 0, desliga o blend (SetGpuBlendMode(0,0,0)). Senão escreve um halfword com os bytes +0x3D/+0x3E no registro apontado por DAT_08043E2C, zera um halfword e grava um halfword (DAT_08043E34) em DAT_08043E30 - 4.
- Chama: SetGpuBlendMode. Chamada por: não lido.
- Memória: objeto (+0x3C..+0x3E) em RAM; registros de blend de GPU (IO) via literais.
- Para o Lua: efeito visual de transparência/blend — não portar.
- Confiança: confirmado (código); papel de registrador exato: provável.

## 0x08043E54 Obj_CopySlot8Bytes
- Assinatura: `void f(void)`
- O que faz: copia 8 bytes da tabela em `DAT_08043E74 + byte(+0x38) * 8` para o campo +0x74 do objeto (DAT_08043E70) (DC3: sentido SRC->DST, decompile 0x08043E54), via FUN_0803F63C (cópia de bytes).
- Chama: FUN_0803F63C. Chamada por: não lido.
- Memória: RAM do objeto (+0x38, +0x74..+0x7B) e tabela de 8 bytes.
- Para o Lua: não precisa.
- Confiança: confirmado (código); papel: chute.

## 0x08043F9C FxBlend_ApplyFromObjB
- Assinatura: `void f(void)`
- O que faz: igual à FUN_08043DF8, para o objeto DAT_08043FCC (campos +0x77..+0x79).
- Chama: SetGpuBlendMode. Chamada por: não lido.
- Memória: objeto (+0x77..+0x79); registros de blend (IO).
- Para o Lua: não portar.
- Confiança: confirmado (código); papel: provável.

## 0x08044094 Anim_SetDefaultTimers
- Assinatura: `void f(void)`
- O que faz: grava 1, 1 e 7 em três bytes globais (DAT_080440A8, DAT_080440AC, DAT_080440B0). Valores iniciais de contadores de animação.
- Chama: nenhuma. Chamada por: não lido.
- Memória: RAM (três bytes via literais).
- Para o Lua: valores iniciais de tempo, se a animação for portada.
- Confiança: confirmado (código); significado: chute.

## 0x080440B4 Count_Entries3InTable9
- Assinatura: `int f(void)`
- O que faz: conta quantas das 9 entradas (passo de 8 bytes a partir de DAT_080440D0) têm o byte `0x03`. Devolve a contagem.
- Chama: nenhuma. Chamada por: não lido.
- Memória: leitura de 9 bytes (tabela em RAM via literal).
- Para o Lua: contagem simples; sem efeito colateral.
- Confiança: confirmado.

## 0x080440D4 Check_PairsAllEqual
- Assinatura: `int f(void)`
- O que faz: percorre pares de bytes consecutivos a partir de DAT_080440EC; se todo par é igual até o limite DAT_080440E8, devolve 1; senão 0.
- Chama: nenhuma. Chamada por: não lido.
- Memória: leitura de bytes via literais.
- Para o Lua: teste de igualdade; porte trivial.
- Confiança: confirmado.

## 0x08044118 TileMap_ReadRectToBuffer
- Assinatura: `void f(uint w, uint h, int x, int y, uint screenBase, u16 *dst)`
- O que faz: copia um retângulo de `w` halfwords por `h` linhas do mapa de fundo (VRAM 0x06000000 + bloco `(screenBase>>8 & 0x1F)*0x800` + y*0x40 + x*2) para o buffer `dst`.
- Chama: nenhuma. Chamada por: funções de desenho de campos de contador (ex.: FUN_080440D4 e vizinhos da R1, "FUN_08044118(...)").
- Memória: leitura de VRAM de tilemap (0x06000000+); escrita em RAM (`dst`).
- Para o Lua: não portar; o port desenha texto com a fonte, não lê o mapa.
- Confiança: confirmado (código).

## 0x08044160 TileMap_WriteBufferToRect
- Assinatura: `void f(uint w, uint h, int x, int y, uint screenBase, u16 *src)`
- O que faz: operação inversa da FUN_08044118: escreve o buffer `src` no retângulo do mapa de fundo (VRAM 0x06000000). É o que desenha os campos de números/rótulos na R1.
- Chama: nenhuma. Chamada por: FUN_08044118 vizinhos da R1 (campos de contador, grade de bots).
- Memória: escrita em VRAM de tilemap.
- Para o Lua: o efeito "desenhar campo na posição (x,y)" é o que importa; portar como desenho de texto.
- Confiança: confirmado (código).

## 0x080441A8 Gfx_DecompressFixedAsset
- Assinatura: `void f(u32 asset)`
- O que faz: chama decompress_and_store com o id recebido e um endereço fixo (DAT_080441B4).
- Chama: decompress_and_store. Chamada por: não lido.
- Memória: ROM (fixo); escrita via decompressão.
- Para o Lua: não precisa.
- Confiança: confirmado.

## 0x080441B8 Screen_ResetGfxAndState
- Assinatura: `void f(void)`
- O que faz: zera um bloco de RAM (bios_cpufastset de fill, tamanho em DAT_08044214) e a paleta de BG (0x05000000, mesmo tamanho); chama FUN_0803F624 (limpa 0x14 bytes em DAT_08044218), marca 3 nesse bloco, zera três bytes de estado em DAT_0804421C+0x41..0x43 e grava 0 e 3 em DAT_08044220/DAT_08044224.
- Chama: bios_cpufastset (x2), FUN_0803F624. Chamada por: não lido.
- Memória: RAM de estado; paleta de BG (0x05000000) via fill.
- Para o Lua: reset de estado de tela; porte só o estado, não o hardware.
- Confiança: provável (passos de limpeza confirmados; significado dos bytes: chute).

## 0x0804422C Anim_AdvanceSlotTileOffset
- Assinatura: `void f(int slot)`
- O que faz: para o slot `slot` (registros de 0x40 bytes a partir de DAT_080442A8), se o byte +0x24 ≠ 0 soma em dois halfwords (+4 e +2) valores de uma tabela (DAT_080442AC) indexada por (+0x24-1)*0x100 + (+0x25)*8 + (+0x27 & 3)*2, com máscara 0xFF/DAT_080442B0; depois incrementa o byte +0x27.
- Chama: nenhuma. Chamada por: não lido.
- Memória: RAM do slot (+0x02..+0x27) e tabela de deslocamentos em ROM/RAM via literal.
- Para o Lua: passo de deslocamento de animação por quadro. Porte se a animação for portada; valor exato da tabela ainda não lido.
- Confiança: confirmado (código); papel: provável.

## 0x080442B4 Angle32FromDelta
- Assinatura: `int f(int x0, int y0, int x1, int y1)`
- Enorme (0x337 bytes). O que faz: calcula |dx| e |dy| com ponto flutuante por software (sf32_*), e quantiza o ângulo do vetor em setores de 0 a 0x1C (valores de retorno vistos no código: 0, 4, 8, 0xC, 0x10, 0x14, 0x18, 0x1C e intermediários, com ajuste por limiares). Retorno 0 quando o vetor é nulo.
- Chama: sf32_from_i32_b, sf32_cmp_nan1_a, sf32_cmp_nanm1, sf32_mul, sf_add, sf32_div, sf_trunc_to_i32 (biblioteca de soft-float). Chamada por: não lido.
- Memória: leitura de constantes (literais DAT_0804432x..0804454x).
- Para o Lua: `math.atan2` quantizado em 32 direções (cuidado: a tabela de setores precisa ser verificada contra o uso antes de portar).
- Confiança: provável (estrutura confirmada; mapeamento setor→ângulo não verificado).

## 0x080445F4 FightPreview_InitCompositeBuffers
- Assinatura: `void f(void)`
- O que faz: limpa três buffers de RAM (0xD0, 0x820 e 0x60 bytes via FUN_0803F624), grava 2 em DAT_0804468C, aponta um ponteiro para DAT_08044694, zera-o (cpufastset de 0x200 words) e chama `CompositePreviewContext_Init`.
- Chama: FUN_0803F624 (x3), bios_cpufastset (x2), CompositePreviewContext_Init. Chamada por: preparação da cena de prévia (R2.md, linha ~125, "prepara os dois lados da prévia").
- Memória: RAM de buffers da prévia (EWRAM/IWRAM via literais).
- Para o Lua: inicialização de estado; não portar a limpeza.
- Confiança: provável.

## 0x080446B4 BuildCompositePreviewSpriteLayer
- Assinatura: `void f(int slot, u32 a, u32 b, u32 c, u32 d, u16 e, s16 f, int speed, u32 g, char h, char mode, u8 i)`
- O que faz: escolhe o multiplicador de velocidade (0..3 → 100/200/300/400), monta a estrutura de 4 partes (BuildFourPartLargePreviewComposite) no buffer do slot (`slot*0x410` + base), e se `mode == 2` zera um bloco de 0x200 words e dois bytes de estado.
- Chama: BuildFourPartLargePreviewComposite, bios_cpufastset. Chamada por: prévia de luta (csv: Simon/Rokusho).
- Memória: RAM do slot de prévia (base DAT_080447D4); pacote de OAM via literais.
- Para o Lua: `rig` de quatro partes; o port usa a lógica de composição, não a memória.
- Confiança: confirmado (chamadas e tabela 100..400); papel da composição: provável.

## 0x08044934 PartPreview_LoadLargeDisplaySprite
- Assinatura: `void f(int slot, u32 a, u8 b, s16 x, s16 y, int speed, u8 c, char d, uint mode)`
- O que faz: mapeia `speed` (0..3 → 100/200/300/400), define o buffer de saída (`slot*0x410` + base em DAT_08044A24) e chama LoadPartLargeDisplaySpriteFromAppearanceTable. Se `mode == 2`, zera um bloco de 0x200 words e dois bytes de estado.
- Chama: LoadPartLargeDisplaySpriteFromAppearanceTable, bios_cpufastset. Chamada por: não lido.
- Memória: RAM do slot de prévia; estado em `*piVar2 + 0x...`.
- Para o Lua: mesmo papel da FUN_080446B4, para a versão de sprite único "grande" (retrato/peça grande).
- Confiança: provável (espelho de FUN_080446B4).

## 0x08044B28 Gfx_LoadDefaultAsset
- Assinatura: `void f(void)`
- O que faz: chama `decompress_and_store()` sem argumentos (wrapper).
- Chama: decompress_and_store. Chamada por: não lido.
- Memória: escrita via decompressão.
- Para o Lua: não precisa.
- Confiança: confirmado.

## 0x08044B34 Roster_ResolveIdIfZero
- Assinatura: `char f(char id)`
- O que faz: se `id == 0`, substitui pelo byte da tabela em `DAT_08044B4C + (byte de DAT_08044B50 + DAT_08044B54)`. Caso contrário devolve `id`.
- Chama: nenhuma. Chamada por: opcode de script 0xFB de retrato (R2.md, seção do opcode que mostra personagem; ali o id vem do fluxo de script).
- Memória: leitura de tabela em ROM/RAM via literais.
- Para o Lua: "id 0 = personagem do estado atual"; porte como resolução de id.
- Confiança: confirmado (código); tabela de origem: provável.

## 0x08044B58 site_StarterBot
- Assinatura: `void f(void)`
- O que faz: configura o bot inicial: liga bits 6-7 de um byte de estado (|0xC0), grava 3 e 1 e quatro bytes de 0x2B em um bloco de 7 bytes, e marca quatro flags (+0x57, +0x..., +0xF0) com 1.
- Chama: nenhuma. Chamada por: não lido.
- Memória: RAM de estado do jogo (literais DAT_08044B94..08044BA4).
- Para o Lua: não portar (é só dado inicial do save; ver csv "rotina do bot inicial").
- Confiança: confirmado (código); destino exato dos bytes: provável.

## 0x08044BA8 FightPreview_InitSideState
- Assinatura: `void f(int mode, u8 side)`
- O que faz: chama Slots_ClearFourPairs e Tile_ClearSlots(0,0x80); zera um byte e um ponteiro; limpa 0x14 bytes do lado (FUN_0803F624) e marca 3; guarda `side` em +0x3D. Se `mode` é 0, 2 ou 3 grava `mode` em +0x3E (0 para mode 0); grava 5 e 0x10 em dois literais.
- Chama: Slots_ClearFourPairs, Tile_ClearSlots, FUN_0803F624. Chamada por: cena de prévia (R2 "FightPreview_LoadDisplayedPartIdsForSide").
- Memória: RAM de estado do lado (+0x3C..+0x3E) e literais.
- Para o Lua: inicializa o lado da prévia; o port usa só o modo.
- Confiança: provável.

## 0x08044C38 FightPreview_SetSceneMode
- Assinatura: `void f(int mode)`
- O que faz: chama Slots_ClearFourPairs; limpa dois buffers de 0x14 bytes; marca flags; conforme `mode` (0,1,2,3) escreve tipos (4 ou 10) e zera contadores diferentes.
- Chama: Slots_ClearFourPairs, FUN_0803F624 (x2). Chamada por: não lido.
- Memória: RAM de estado da prévia (buffer em DAT_08044C78).
- Para o Lua: modo da cena (equivale a um enum); porte como estado.
- Confiança: provável.

## 0x08044CDC Dispatch_Sub4Mode
- Assinatura: `void f(int mode)`
- O que faz: despacha por `mode` (0..3) para FUN_08064BEC, FUN_08064C0C, FUN_08064C2C ou FUN_08064C4C (quatro rotinas da região 0x08064xxx, ainda sem seção).
- Chama: FUN_08064BEC, FUN_08064C0C, FUN_08064C2C, FUN_08064C4C. Chamada por: não lido.
- Memória: nenhuma direta.
- Para o Lua: ver as quatro rotinas 0x08064Bxx..0x08064Cxx antes de portar.
- Confiança: confirmado (despacho).

## 0x08044D0C Asset_UnpackOrCopyBlock
- Assinatura: `u32 f(u32 *hdr, int dst)`
- O que faz: lê o cabeçalho `*hdr`. Se o nibble de tipo (bits 4-7 do byte baixo) é 1, descomprime LZ77 para WRAM (bios_lz77_wram). Senão faz bios_cpuset (cópia 16/32 bits) e registra em DAT_08044D60 o ponteiro de fim do bloco, o destino e a flag 0x80000000.
- Chama: bios_lz77_wram, bios_cpuset. Chamada por: Tile_UnpackAndBlitBlock (FUN_08045084) e outros.
- Memória: ROM (bloco de origem), RAM/WRAM (destino), registros em DAT_08044D60.
- Para o Lua: não precisa.
- Confiança: confirmado (código); formato do cabeçalho: provável.

## 0x08044DA8 SetCompositePreviewOamShapeAndSize
- Assinatura: `void f(u8 *attr, int w, int h)`
- O que faz: escreve shape (bits 14-15 do atributo 0, em byte+1) e size (bits 14-15 do atributo 1, em byte+3) de um registro de OAM a partir de dimensões `w` e `h` (8, 0x10, 0x20, 0x40). Trata `w < h`, `w > h` e casos iguais (quadrado) com um comparador.
- Chama: nenhuma. Chamada por: BuildCompositePreviewOamTemplate / renderer de prévia (csv).
- Memória: registro de OAM (buffer do renderer).
- Para o Lua: tamanho do sprite; porte como tamanho, não como bits.
- Confiança: confirmado (código).

## 0x08044E64 OamSizeEncodingLookup
- Assinatura: `u32 f(u32 shapeBits, u32 sizeBits)`
- O que faz: tabela de busca: para a combinação (shape, size) devolve o valor de codificação de OAM correspondente (valores constantes de DAT_08044EAC..08044F50), ou 0 se não bate.
- Chama: nenhuma. Chamada por: FUN_08044FB4, FUN_0804538C, FUN_08045120, FUN_08044DA8 vizinhos.
- Memória: leitura de constantes via literal.
- Para o Lua: tabela de tamanhos de sprite; porte como tabela.
- Confiança: confirmado (código); nome: provável.

## 0x08044FB4 Tile_BlitScaledRect
- Assinatura: `void f(u32 dst, u32 src, u32 shapeBits, u32 sizeBits, int rows, int cols, int scale)`
- O que faz: obtém o tamanho em pixels via OamSizeEncodingLookup; calcula um passo com `bios_div(0x40, 2 - scale)` e copia em blocos (bios_cpuset, modo cópia) um retângulo de `rows` x `cols` tiles, pulando linhas conforme o passo.
- Chama: OamSizeEncodingLookup, bios_div, bios_cpuset. Chamada por: Tile_UnpackAndBlitBlock (FUN_08045084).
- Memória: cópia de tiles entre RAM/VRAM (destino/origem em argumentos).
- Para o Lua: não portar (cópia de gráfico).
- Confiança: provável (passo/escala interpretado pelo código; nome: provável).

## 0x08045084 Tile_UnpackAndBlitBlock
- Assinatura: `u32 f(int size, ...)`
- O que faz: se `size != 0`, descomprime um bloco com Asset_UnpackOrCopyBlock (FUN_08044D0C), chama Tile_BlitScaledRect com o endereço dado, e, se o último argumento é 0, registra o bloco no mesmo esquema de DAT_08045118 (ponteiros + tamanho + flag 0x94000000), senão copia por bios_cpuset.
- Chama: Asset_UnpackOrCopyBlock, Tile_BlitScaledRect, bios_cpuset. Chamada por: ExpandCompositePreviewTemplateEntryToOam (FUN_08045120).
- Memória: ROM de origem; VRAM/RAM de destino; registro em DAT_08045118.
- Para o Lua: não portar.
- Confiança: provável.

## 0x08045120 ExpandCompositePreviewTemplateEntryToOam
- Assinatura: `u64 f(char *out, u32 pal, int tileBase, u32 *tpl, int entry, u32 page, u32 x)`
- O que faz: lê o cabeçalho de 12 bytes do template (`*tpl`), calcula tamanho com OamSizeEncodingLookup, chama Tile_UnpackAndBlitBlock (FUN_08045084), e escreve os registros de OAM de 8 bytes em `out` (grade linhas x colunas), com tile = índice*largura + base e x avançando pelo passo.
- Chama: OamSizeEncodingLookup, Tile_UnpackAndBlitBlock, bios_cpuset. Chamada por: InitCompositePreviewTemplateEntry (csv) e cadeia de prévia.
- Memória: buffer de OAM (saída); ROM/RAM do template.
- Para o Lua: gera a lista de sprites de um template; relevante para o desenho de prévia.
- Confiança: confirmado (código, escrita dos registros); papel exato no fluxo: provável.

## 0x0804538C LayoutCompositePreviewEntryOamGrid
- Assinatura: `void f(u32 *entry, uint y0, uint x0, u16 pal)`
- O que faz: escreve a grade de registros de OAM de um template já expandido (linhas `entry+0x0A`, colunas `entry+0x0B`): byte 0 = y, halfword 1 com X (passo derivado de OamSizeEncodingLookup) e o bit de paleta. Não desloca registros existentes: posiciona-os.
- Chama: OamSizeEncodingLookup. Chamada por: QueueCompositePreviewTemplateEntryOam e vizinhos (csv).
- Memória: registros de OAM (escrita).
- Para o Lua: posicionamento de sprites em grade; porte como cálculo de posição.
- Confiança: confirmado (código). Nota: mtbe.csv chama de `ShiftCompositePreviewTemplateEntryX` com descrição "desloca em X"; o código **posiciona** a grade (escreve y e x), não desloca. Ajustar o csv.

## 0x08045588 InsertCompositePreviewOamSpanBySortKey
- Assinatura: `u64 f(u32 *list, s16 count, u32 pos, s16 key)`
- O que faz: insere um span de registros de OAM de 8 bytes na lista, deslocando para baixo os registros cuja chave (halfword +6) é maior que `key`, de trás para frente. Devolve o ponteiro de inserção.
- Chama: nenhuma. Chamada por: QueueCompositePreviewTemplateEntryOam (csv) e o renderer de prévia.
- Memória: lista de OAM (leitura e escrita).
- Para o Lua: ordenação de z-order da prévia; porte como ordenação estável.
- Confiança: confirmado (código).

## 0x080455E0 CopyCompositePreviewTemplateEntryOam
- Assinatura: `u64 f(int *entry, int dst, u32 start, u32 end)`
- O que faz: copia os registros de OAM já expandidos de um template (`entry->+0`, quantidade linhas x colunas) para o destino a partir de `start` (até `end`). Grava a chave de ordenação (halfword +6) de `entry+2` em cada registro e ajusta os bits 2-3 do byte +5 conforme a chave (faixas 0x65, 0xC9, 0x12D: 4, 8 ou 0xC).
- Chama: nenhuma. Chamada por: prévia de template (csv).
- Memória: registros de OAM (leitura da origem, escrita no destino).
- Para o Lua: não portar os bits; manter ordem por chave.
- Confiança: confirmado (código); significado das faixas de chave: provável.
