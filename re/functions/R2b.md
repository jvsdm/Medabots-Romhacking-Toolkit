# R2b: documentação das funções 0x08045680..0x08047E08 (metade final da faixa R2)

Pulados por já estarem em R2.md: 0x08047428 MotionInterpolateKeyframe, 0x080465B8 RenderCompositePreviewDescriptorRecursive.
Endereços sem função no Ghidra (miolo ou pool de literais) ficam com uma linha de nota.

## 0x08045680 QueueCompositePreviewTemplateEntryOam
- Assinatura: `u32 QueueCompositePreviewTemplateEntryOam(int *entry, u32 _, u32 start, s8 palBase, u32 end, u32 flags)`
- O que faz: reserva um bloco de slots de OAM (via FUN_08045588) e copia até `count = byte[+0x0B] * byte[+0x0A]` registros de 8 bytes do template (`*entry`) para ele, em `start..end`. Em cada registro: soma `palBase` ao campo de paleta (bits 4..7 do halfword +0x05), grava a coordenada de `entry+0x04` em +0x06 e escolhe o tamanho/prioridade do atributo pela faixa do valor (`entry+0x04`: ≤100, 101..200, 201..300, >300) nos bits do byte +0x05; `flags` vão para os bits 4 e 5 do byte +0x03.
- Chama: FUN_08045588 (alocação de slot de OAM). Chamada por: não lido nesta passada (provável: montagem da prévia composta; ver mtbe: Simon/Rokusho, "ordem de z").
- Memória: lê o template (ponteiro em `entry+0x00`); escreve no buffer de OAM (slot retornado por FUN_08045588).
- Para o Lua: não precisa (montagem de OAM da prévia; o Lua desenha direto). Se for portar, a ordem de z vem de `entry+0x04`.
- Confiança: confirmado (código); o nome segue o mtbe.csv (provável).

## 0x08045794 (sem função no Ghidra)
- Nota: o endereço cai logo após o fim de FUN_08045680 (0x08045791). Tratado como dado/pool de literais; nada a documentar.

## 0x08045A14 CopyObjAffineGroupsToShadowOam
- Assinatura: `void CopyObjAffineGroupsToShadowOam(u8 *dst, u16 *src)`
- O que faz: copia 32 grupos de 4 halfwords (pa, pb, pc, pd) de `src` para `dst`, com passo de 0x20 bytes no destino, escrevendo nos halfwords +0x06, +0x0E, +0x16, +0x1E. São os halfwords de matriz afim da OAM sombra (grupos de 0x20 B).
- Chama: não. Chamada por: VBlank 0x0800065C (nota G8 em `pc/research/README.md`: "VBlank 0x0800065C -> FUN_08045A14 (copia 32 grupos)"; confirmado pela nota, não relido aqui).
- Memória: lê 0x80 B de `src` (fonte EWRAM 0x02024FA4 segundo G8); escreve na OAM sombra 0x03001380 (cada grupo 0x20 B, halfwords 6/0xE/0x16/0x1E).
- Para o Lua: não precisa (cópia de OAM; o Lua usa a matriz calculada direto em `rig.draw_angle`).
- Confiança: confirmado (código); o destino 0x03001380 é provável (G8).

## 0x08045A40 (sem função no Ghidra)
- Nota: o endereço fica após o fim de FUN_08045A14 (0x08045A3F). No mtbe.csv está como `IsCompositePreviewPointOnScreen`, mas o Ghidra não tem função nesse endereço: provavelmente pool de literais ou o nome do mtbe está deslocado. Conferir antes de usar esse nome.

## 0x08045D34 BuildCompositePreviewOamTemplate
- Assinatura: `u32 BuildCompositePreviewOamTemplate(u8 *out, const u8 *part, s8 side, u16 x)`
- O que faz: monta o registro de template de OAM (8 B) de uma peça a partir do descritor `part`: bits de forma/tamanho de `part+0x15` nos bytes 1 e 3 de `out`, prioridade/paleta e tile via `FUN_08044DA8`, coordenada `x` (bits 0..9 de +0x04) e deslocamento vertical `side + part[0x11]` (bits 4..7 do byte +0x05). Zera +0x06. Os tamanhos em tiles `part[0x12]` e `part[0x13]` são divididos por `part[0x14]` (nibbles) se > 1 (`bios_div`). Retorna o número de tiles `(w>>3)*(h>>3)`, dobrado se o bit 0 de `part[0x15]` estiver ligado.
- Chama: bios_div (0x0807CD2C), FUN_08044DA8. Chamada por: PartPreviewPiece (0x08045E6C) (confirmado: o retorno é somado em `*param_6`).
- Memória: escreve 8 B em `out`; lê o descritor `part` (0x16 B a partir de `param_2`).
- Para o Lua: não precisa (monta OAM); o equivalente lógico seria o tamanho do sprite por `part[0x12..0x14]`.
- Confiança: confirmado (código); o nome do mtbe é provável.

## 0x08045E6C PartPreviewPiece
- Assinatura: `int PartPreviewPiece(int ctx, u32 *list, u32 part, u32 pal, int base, u16 *cursor, u32 flip)`
- O que faz: desenha uma peça da prévia composta. Procura em `list` a entrada cujo campo de 6 bits (`& 0x3F`) iguala `part`; se achar, pega o índice de tile (bits 6..) somado a `flip & bit 15` e, se o descritor de tiles existe, copia os tiles com `bios_cpuset` (16 halfwords por lado, base 0x020 * (`piece+0x11` + `pal`)). Depois copia os tiles do quadro para o buffer (`FUN_08044D0C`, DMA-like), chama `FUN_08045D34` para o template de OAM e avança `*cursor` pelo retorno. Se o byte `+0x10` ou `+0x0F` do descritor de peça for ≠ 0, chama a si mesma para essas sub-peças (dois filhos).
- Chama: bios_cpuset (SWI 0x0B), FUN_08044D0C, FUN_08045D34, a si mesma. Chamada por: não lido nesta passada (provável: montagem da prévia composta, `BuildFourPartLargePreviewComposite` 0x08046D1C ou o renderer).
- Memória: lê a lista `list` e os descritores de peça (`ctx[part]`); escreve em VRAM de tiles via cpuset e no template de OAM; `*cursor` (halfword) é avançado.
- Para o Lua: não precisa (composição de prévia em tiles/OAM).
- Confiança: confirmado (código); a semântica de `flip` e do cursor é provável.

## 0x08046034 CopyCompositePreviewDescriptorGraphics
- Assinatura: `u32 CopyCompositePreviewDescriptorGraphics(int ctx, int desc, u32 *list, u32 part, u32 pal)`
- O que faz: procura em `list` (lista de entradas de 4 B terminada por campo de 6 bits = 0) a entrada cujo `& 0x3F` iguala `part`. Ao achar, pega o índice de tile (`(entrada & 0x3FFF) >> 6`) e chama `FUN_08044D0C` para obter o tamanho do bloco (0 = nada a copiar). Se houver tamanho, copia com `bios_cpuset` para o destino de tiles (base + `(desc[pal] & 0x3FF) * 0x20`), metade do tamanho em halfwords.
- Chama: FUN_08044D0C, bios_cpuset (SWI 0x0B). Chamada por: LoadPartLargeDisplaySpriteFromAppearanceTable (0x08046A60).
- Memória: lê a lista e o descritor (`ctx+4`, `ctx+8`); escreve em VRAM de tiles (destino vindo do literal `DAT_08046094`/`DAT_08046098`).
- Para o Lua: não precisa (cópia de tiles para VRAM).
- Confiança: confirmado (código); o nome segue o mtbe.csv (provável).

## 0x08046A60 LoadPartLargeDisplaySpriteFromAppearanceTable
- Assinatura: `u32 LoadPartLargeDisplaySpriteFromAppearanceTable(u32 _, int ctx, int part, int size, int x, int y, s16 z, u8 pal, u32 flags)`
- O que faz: escolhe a tabela de aparência pelo `size` (0..3; base de tabela e largura de 0x140, 0x1A0, 0x1E0 ou 0x220; passo 0x10 ou 0x20 por `part`). Com `flags & 4` zero, só configura o registro de objeto (`DAT_08046B14`, passo 0x30) com o flag de ativo e a posição. Com `flags & 4`, lê o estado do registro: se o bit 2 (0x2) não estiver ligado e o bit 3 (0x8) estiver, chama `RenderCompositePreviewDescriptorRecursive` com a posição deslocada pelas bordas da peça. Se o bit 2 estiver ligado, desenha a peça via `PartPreviewPiece`, e para `size` 1 e 2 chama `CopyCompositePreviewDescriptorGraphics` para as sub-peças da tabela; no fim marca o registro com `(bits & ~2) | 0xC`.
- Chama: PartPreviewPiece (0x08045E6C), RenderCompositePreviewDescriptorRecursive (0x080465B8), CopyCompositePreviewDescriptorGraphics (0x08046034). Chamada por: não lido nesta passada (provável: carga de peça grande da prévia).
- Memória: lê e escreve o registro de objeto (passo 0x30, flags em +0x2C, posição em +0x28); lê as tabelas de aparência pelos literais `DAT_08046AAC..08046B14`.
- Para o Lua: não precisa (prévia em OAM); a lógica de tamanho está em `part` e `size`.
- Confiança: confirmado (código); o nome e a semântica de `size` (0..3) são prováveis.

## 0x08046CC8 FlipSyntheticCompositePreviewDescriptorOffsets
- Assinatura: `u32 FlipSyntheticCompositePreviewDescriptorOffsets(int a, int b, int c, int d)`
- O que faz: para cada um dos 4 argumentos com valor ≥ 0 (válido), troca o byte de deslocamento +0x0D do descritor sintético correspondente por `-2 - valor`, ou seja, espelha o deslocamento em X ao redor do eixo. Argumento negativo = não mexe.
- Chama: não. Chamada por: não lido nesta passada (provável: espelho da prévia com `flip`).
- Memória: lê/escreve 4 bytes (+0x0D) de descritores sintéticos apontados por literais `DAT_08046D0C..08046D18`.
- Para o Lua: não precisa (espelho de prévia). A fórmula `-2 - x` é a regra a copiar se a prévia for portada.
- Confiança: confirmado (código); o significado de "espelho" é provável.

## 0x08047204 UpdateTwoCompositePreviewSlotFlags (nome novo)
- Assinatura: `u32 UpdateTwoCompositePreviewSlotFlags(void)`
- O que faz: percorre duas entradas (registros de 0x30 B a partir de `*DAT_08047228`) e ajusta o byte de flags em +0x2C: se o bit 0 estiver ligado, troca por bit 1 (desliga 0, liga 1); senão, se bits 1 estiver desligado e bits 3 e 2 estiverem ligados, desliga o bit 2.
- Chama: não. Chamada por: não lido nesta passada.
- Memória: lê e escreve flags (+0x2C) de 2 registros de objeto (passo 0x30).
- Para o Lua: não precisa (estado de controle de OAM).
- Confiança: confirmado (código); o nome é novo e descreve só o efeito.

## 0x08047414 CompositePreviewContext_Init
- Assinatura: `void CompositePreviewContext_Init(int *ctx, int a, int b, int c)`
- O que faz: grava três ponteiros/valores no contexto (`ctx[0..2]` = `a`, `b`, `c`) e zera o byte de `a + DAT_08047424` (flag de limpeza).
- Chama: não. Chamada por: não lido nesta passada (provável: início da prévia composta).
- Memória: escreve em `ctx` (12 B) e um byte no endereço `a + literal DAT_08047424`.
- Para o Lua: não precisa.
- Confiança: confirmado (código).

## 0x08046D1C BuildFourPartLargePreviewComposite
- Assinatura: `u32 BuildFourPartLargePreviewComposite(u32 _, int ctx, u32 a, u32 b, u32 c, u32 d, s16 x, u8 pal, u32 flags, u32 slot)`
- O que faz: dois modos pelo bit 2 de `flags`. Sem o bit: registra um objeto novo no registro de 0x30 B (`*DAT_08046D88 + slot*0x30`): flag de ativo (bit 0), argumentos a..d em +0x04..+0x10 e `slot` em +0x28. Com o bit: monta a composição de 4 peças (tamanhos 0x140, 0x1A0, 0x1E0, 0x220, na ordem), copiando para um buffer de 0x80 B as três listas de cada peça (`DAT_080471F4..080471FC`, `DAT_08047200`) e desenhando a peça grande com `RenderCompositePreviewDescriptorRecursive`. Na variante das 4 peças (`else`), chama `PartPreviewPiece` para cada uma das quatro peças e, se `slot` (argumento 0x3C) ≠ 0, copia os gráficos das sub-peças (`CopyCompositePreviewDescriptorGraphics`) e espelha os deslocamentos (`FlipSyntheticCompositePreviewDescriptorOffsets`) quando `flags & 2`. Por fim copia 0x60 B de estado para o contexto e marca o registro com `(bits & ~2) | 0xC`.
- Chama: RenderCompositePreviewDescriptorRecursive (0x080465B8), PartPreviewPiece (0x08045E6C), CopyCompositePreviewDescriptorGraphics (0x08046034), FlipSyntheticCompositePreviewDescriptorOffsets (0x08046CC8), lib_memset. Chamada por: não lido nesta passada (provável: montagem da prévia de 4 partes, `FUN_080447EC` segundo a nota de R2).
- Memória: registros de objeto (passo 0x30, `DAT_08046D88`); buffer local de 0x80 B; tabelas de peças (literais `DAT_08046E2C..08046E48`, `DAT_08047050..0804705C`); contexto de 0x60 B em `ctx + slot*0x60`.
- Para o Lua: não precisa (prévia de 4 partes em OAM/tiles).
- Confiança: confirmado (código); a ordem de tamanhos e o papel de `slot` são prováveis.

## 0x08047574 EventScript_ClearGlobals
- Assinatura: `u32 EventScript_ClearGlobals(void)`
- O que faz: chama 15 vezes `FUN_08049714(ptr, n)` com ponteiros de global (literais `PTR_DAT_080475FC..08047634`) e tamanhos de 4 a 0x240 B (0x3C, 0x38, 0x200, 0x200, 0x34, 0x10, 0x240, ...). Na prática zera os blocos de variáveis globais de evento/mapa (o nome do mtbe, `EventScript_ClearGlobals`, bate com isso). Um dos ponteiros é `gMapLayerBufferPtr` e outro `gEventDbBase`, segundo os rótulos do Ghidra.
- Chama: FUN_08049714 (provável memset; não lido). Chamada por: não lido nesta passada.
- Memória: zera blocos de RAM de globais (endereços nos literais do pool).
- Para o Lua: não precisa como função; o estado de evento vive em tabelas Lua. Se for portar, o efeito é "limpar todas as globais de evento".
- Confiança: provável (chamadas e tamanhos lidos; FUN_08049714 não conferida).

## 0x0804765C (nome não encontrado)
- Assinatura: `void FUN_0804765C(int mode)`
- O que faz: para `mode == 0`: copia um byte de estado (+0x9C de um objeto) para outro (+0x39), e escolhe entre dois conjuntos de flags e um valor de fase (`13` ou `5`) conforme um byte em `DAT_0804769C`; grava 1 em duas flags e o valor no ponteiro escolhido. Para `mode == 1`: liga uma flag, põe 2 em outra e grava 0 no ponteiro de fase. Outros modos: nada.
- Chama: não. Chamada por: não lido nesta passada.
- Memória: flags e fase em RAM via literais `DAT_08047690..080476E4` (não identificados).
- Para o Lua: não precisa até o nome estar claro.
- Confiança: confirmado (código); o significado dos modos é chute. Sem nome novo.

## 0x08047850 (nome não encontrado)
- Assinatura: `void FUN_08047850(int idx)`
- O que faz: se `0 <= idx < 4`, grava 1 em duas flags e 5 em um byte de estado (literais `DAT_0804786C..08047874`). Fora da faixa, não faz nada.
- Chama: não. Chamada por: não lido nesta passada.
- Para o Lua: não precisa até o contexto estar claro.
- Confiança: confirmado (código); o papel de `idx` é chute. Sem nome novo.

## 0x08047BA8 (mtbe: Overworld_StartMapTransitionEffect)
- Assinatura: `u32 FUN_08047BA8(void)`
- O que faz: só chama `snd_sfx_request()`. O corpo não tem nenhuma transição de mapa.
- Chama: snd_sfx_request. Chamada por: não lido nesta passada.
- Para o Lua: não precisa (som).
- Confiança: confirmado (código). O nome do mtbe (`Overworld_StartMapTransitionEffect`) NÃO bate com o corpo: conferir antes de usar; não renomeado.

## 0x080476E8 (nome não encontrado)
- Assinatura: `u32 FUN_080476E8(u32 cmd)`
- O que faz: `switch` por comando (0..12). Comandos 0,1,2,3 e 12 não fazem nada. Comandos 4, 5, 6 gravam flags e um valor de fase (16, 32, 8) no ponteiro escolhido. Comandos 7 e 8 gravam 7 e 8 num byte de estado. Comando 9 liga `gPlayTimeRunning` e cai no caso comum (grava 1 em `gMainState`, 1 em outra flag e 5 no ponteiro de fase). Comandos 10 e 11 chamam `EventScript_ClearGlobals` (e, no 10, `FUN_08049714` com 0xCC0 B) e gravam o estado inicial (valor de fase 0 e 10). Comandos fora de 0..12 não fazem nada.
- Chama: EventScript_ClearGlobals (0x08047574), FUN_08049714. Chamada por: não lido nesta passada (provável: troca de estado de jogo por comando; ver o mtbe `GameState_StartFlowByCommand` em 0x08047878, que pode ser o mesmo tipo de rotina).
- Memória: flags de estado de jogo (gMainState, gPlayTimeRunning e ponteiros de literal não identificados); fase em um ponteiro escolhido por caso.
- Para o Lua: não precisa até o contexto estar claro.
- Confiança: confirmado (código); o significado dos valores de fase é chute. Sem nome novo.

## 0x08047BDC GetBotName
- Assinatura: `char *GetBotName(int bot)`
- O que faz: devolve `gBotNames[bot]` (ponteiro de 4 B por bot; tabela no literal `DAT_08047BE8`).
- Chama: não. Chamada por: não lido nesta passada.
- Memória: lê a tabela de ponteiros de nomes na ROM.
- Para o Lua: não precisa (o Lua já tem os nomes em `text.lua`).
- Confiança: confirmado (código). Nome já era GetBotName no Ghidra.

## 0x08047BEC GetPartName
- Assinatura: `char *GetPartName(int kind, int bot)`
- O que faz: devolve `gPartNames[bot*4 + kind]` (ponteiro de 4 B; tabela no literal `DAT_08047BFC`). `kind` = 0..3 (cabeça, braço, perna, corpo).
- Chama: não. Chamada por: não lido nesta passada.
- Memória: lê a tabela de ponteiros de nomes na ROM.
- Para o Lua: não precisa (o Lua já tem os nomes em `text.lua`). A ordem `kind` = posição da peça é provável.
- Confiança: confirmado (código); o significado de `kind` é provável. Nome já era GetPartName no Ghidra.

## 0x08047878 GameState_StartFlowByCommand
- Assinatura: `void GameState_StartFlowByCommand(u32 cmd, u8 arg)`
- O que faz: `switch` por comando: 0..6, 8, 0x10..0x13, 0x40..0x43 (mais 0x20 e 0x21, que tratam um byte de flag à parte e saem cedo). Cada caso grava flags de estado (1, 2 ou 3 num byte) e um valor de fase (ex.: 0x12, 0x0B, 0x15, 0x14, 0x17, 0x16, 4, 5, 6, 0x10, 0x18, 0x1C, 0x28). O caso 3 grava `arg` num byte. O caso 0x12 grava 0 no ponteiro de fase `DAT_08047BA4`. Comandos fora da lista não fazem nada.
- Chama: não. Chamada por: não lido nesta passada (provável: tela/fluxo de jogo a partir de um comando; é o nome do mtbe, que bate com o corpo).
- Memória: flags e fase em ponteiros de literal (`DAT_08047A88..08047BA4`, `DAT_080479BC..080479FC`), não identificados.
- Para o Lua: não precisa até mapear os valores de fase (provável: tabela de fluxos da máquina de estados).
- Confiança: confirmado (código); o nome do mtbe é provável.

## 0x08047BB4 Sound_RequestSong
- Assinatura: `u32 Sound_RequestSong(void)`
- O que faz: só chama `snd_song_request()`. Wrapper.
- Chama: snd_song_request. Chamada por: não lido nesta passada.
- Para o Lua: não precisa (áudio).
- Confiança: confirmado (código). Nome novo.

## 0x08047BC0 Sound_RequestFade
- Assinatura: `u32 Sound_RequestFade(void)`
- O que faz: só chama `snd_fade_request()`. Wrapper.
- Chama: snd_fade_request. Chamada por: não lido nesta passada.
- Para o Lua: não precisa (áudio).
- Confiança: confirmado (código). Nome novo.

## 0x08047BCC (nome não encontrado)
- Assinatura: `u32 FUN_08047BCC(int idx)`
- O que faz: devolve `DAT_08047BD8[idx]` (tabela de ponteiros de 4 B). Getter indexado.
- Chama: não. Chamada por: não lido nesta passada.
- Confiança: confirmado (código); o conteúdo da tabela não foi lido. Sem nome.

## 0x08047C00 (nome não encontrado)
- Assinatura: `u32 FUN_08047C00(void)`
- O que faz: chama `FUN_08047E4C` e zera dois pares de halfwords (`DAT_08047C1C` e `DAT_08047C20`, 4 B cada).
- Chama: FUN_08047E4C (não lido). Chamada por: não lido nesta passada.
- Confiança: confirmado (código). Sem nome.

## 0x08047C24 (nome não encontrado)
- Assinatura: `u32 FUN_08047C24(u8 a, u8 b, u8 c, u32 d)`
- O que faz: máquina de passos de dois estados (byte em `DAT_08047C44`). Estado 0: se o segundo byte for 0 chama `Obj_InitSlotAndLoad(a)`, senão `FUN_0803FB0C(b_byte, b, c)`; incrementa um contador (`DAT_08047C64`) e segue para o estado 1. Estado 1: chama `StateMachine_DispatchStep(d)`; quando ele devolve ≠ 0, volta a 0, incrementa o byte seguinte e devolve 1.
- Chama: Obj_InitSlotAndLoad, FUN_0803FB0C (não lido), StateMachine_DispatchStep. Chamada por: não lido nesta passada.
- Memória: estado em 2 bytes de `DAT_08047C44`; contador em `DAT_08047C64`.
- Para o Lua: não precisa até o uso estar claro.
- Confiança: confirmado (código); o papel do passo é provável (carga de objeto, depois despacho).

## 0x08047C84 Text_WriteDecimalToSlot (nome novo, provável)
- Assinatura: `u32 Text_WriteDecimalToSlot(int slot, u32 value)`
- O que faz: escreve `value` em decimal no buffer de texto `slot` (0..3; endereço em `DAT_08047CE0[slot]`): conta os dígitos, converte cada um para caractere (`'0' + dígito`) e grava com `FUN_080497C0`, seguido do terminador `0xFE`.
- Chama: Map_CountDecimalDigits, Map_FormatDecimalDigits, FUN_080497C0. Chamada por: não lido nesta passada.
- Memória: buffers de texto em RAM (4 ponteiros em `DAT_08047CE0`).
- Para o Lua: não precisa (o Lua formata número com `tostring`); o formato é o do texto do GBA (terminador 0xFE).
- Confiança: confirmado (código); o nome é provável.

## 0x08047CE4 Text_CopyStringToSlot (nome novo, provável)
- Assinatura: `u32 Text_CopyStringToSlot(int slot, const u8 *src)`
- O que faz: copia bytes de `src` para o buffer `slot` (ponteiros em `DAT_08047D00`), até achar `0xFE` ou até 16 bytes, e grava `0xFE` no fim.
- Chama: não. Chamada por: não lido nesta passada.
- Memória: lê `src`; escreve o buffer de texto do slot.
- Para o Lua: não precisa.
- Confiança: confirmado (código); o nome é provável.

## 0x08047D24 (nome não encontrado)
- Assinatura: `u32 FUN_08047D24(int slot)`
- O que faz: chama `Num_FormatDecimalToGlyphsNoPrefix(DAT_08047D48[slot])`. Formata um número para glifos no buffer do slot.
- Chama: Num_FormatDecimalToGlyphsNoPrefix. Chamada por: não lido nesta passada.
- Confiança: confirmado (código). Sem nome.

## 0x08047D4C (nome não encontrado)
- Assinatura: `u32 FUN_08047D4C(void)`
- O que faz: devolve o valor apontado por `DAT_08047D54` (getter de 1 variável global).
- Chama: não. Chamada por: não lido nesta passada.
- Confiança: confirmado (código). Sem nome (a variável não foi identificada).

## 0x08047D58 AddClampedCounter (nome novo, provável)
- Assinatura: `void AddClampedCounter(int delta)`
- O que faz: soma `delta` ao contador apontado por `DAT_08047D6C`; se passar do teto `DAT_08047D70`, grava o teto.
- Chama: não. Chamada por: não lido nesta passada.
- Para o Lua: `math.min(cnt + delta, max)`.
- Confiança: confirmado (código); o nome e o papel do contador são prováveis.
