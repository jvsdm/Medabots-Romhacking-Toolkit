# HA8: handlers da tabela dos despachantes Fight_DispatchOnSubstateAA_AH..AP

Lista: `HA8_lista.txt` (42 handlers). Cada handler é entrada de tabela indexada por +0xAA (subestado) do bloco
0x03001800. Conferido na ROM: todas as 42 linhas batem com `tabela_do_stub[idx]` (ponteiro com bit Thumb).
Tabelas (literal stub+0x24): AH 0x083AE124, AI 0x083AE184, AJ 0x083AE1E4, AK 0x083AE264, AL 0x083AE280,
AM 0x083AE298, AN 0x083AE2EC, AO 0x083AE340, AP 0x083AE3C0. Bloco (literal stub+0x20): 0x03001800.

Resultado (redo, rodada 2): todas as 42 entradas têm função no Ghidra e código lido. As 24 seções que estavam
"sem função" foram decompiladas e renomeadas (`Fight_DispatchOnSubstateAA_<Desp>_<idx>_<Passo>`). Confiança das
42 seções: 39 "confirmado (decompile)" (nome provável) e 3 "provável" (0x08025D6C, 0x08025E88, 0x08026810).

Convenção usada: "+1" = avança para o próximo subestado; "=0x0D" e "+3" = salto explícito; "timer +0x86 = 0x3C"
é contador de frames, não subestado. O campo +0x87 é o bot ativo e +0x88 o bot alvo (troca no AJ 0x0B).

## 0x08025790 Fight_DispatchOnSubstateAA_AH_FormatCounterDigits (AH, idx 0x12)
- Assinatura: `void f(void)`
- O que faz: formata o valor de +0x51 em 6 glifos (`Num_FormatDecimal6GlyphsBase`), grava +0xA6=0x35, avança +1.
- Memória: lê +0x51; escreve +0xA6, +0xAA.
- Para o Lua: porta junto com a formatação de número.
- Confiança: confirmado (decompile); nome provável.

## 0x080257E8 Fight_DispatchOnSubstateAA_AI_00_AccumPartCheckCost (AI, idx 0x00)
- Assinatura: `undefined4 f(void)`
- O que faz: acumula a peça do bot ativo (`Fight_AccumRosPart`, bot +0x87, parte +0xA7) e aplica o custo de status
  (`Fight_ApplyStatusCostIfFlag`). Se `FUN_0803725c` == 0 e o alvo (+0x88) não tem peça de técnica 16
  (`Fight_HasNoTechnique16Part`), grava +0xA6=0x39 e avança +1. Se tem, incrementa o contador de uso
  (`Fight_IncrementActionUsageCounter_08036270`), chama `FUN_08037280` e vai para o subestado 4. Se `FUN_0803725c` != 0,
  grava +0xA6=0x1A e avança +1.
- Memória: lê +0x87, +0x88, +0xA7; escreve +0xA6, +0xAA.
- Para o Lua: porta junto com as funções de peça (`FUN_0803725c` e `FUN_08037280` ainda sem nome).
- Confiança: confirmado (decompile); nome provável.

## 0x08025858 Fight_DispatchOnSubstateAA_AI_LoadTechniqueGraphics (AI, idx 0x08)
- Assinatura: `void f(void)`
- O que faz: escolhe um de dois gráficos (LZ77 + cpufastset de 16 words) pelo byte de técnica da peça (`== '.'`),
  ajusta registro IO em base+0x840 (|= 0x220), avança +1 e faz salto por outra tabela (não recuperada).
- Memória: lê +0xA7, +0x87, +0x49 do bot; escreve IO, +0xAA.
- Para o Lua: porta a escolha de gráfico; o salto final precisa do próximo despachante.
- Confiança: confirmado (decompile); nome provável.

## 0x08025974 Fight_DispatchOnSubstateAA_AI_0B_SetupTargetSpriteSfx (AI, idx 0x0B)
- Assinatura: `undefined4 f(void)`
- O que faz: limpa o bit de atributo do slot 0x21 (`Slot_ClearAttrMask`), zera o byte de controle do slot e monta
  o registro de sprite com a posição do bot alvo (+0x88): X de base+0x1742 + bot*0x100, Y de base+0x1744 + bot*0x100 − 4.
  Preenche campos fixos (tipo 7, 0xE0, 0) e toca `snd_sfx_request(0x6A)`; avança +1.
- Memória: lê +0x88 e registros de sprite do bot alvo; escreve registros de sprite do slot, +0xAA.
- Para o Lua: porta (depende da representação de sprite/OAM).
- Confiança: confirmado (decompile); nome provável. A base dos registros de sprite (literal) não foi confirmada
  como 0x03001800; os campos +0x880 e +0x8/+0x2 são offsets de registro sem nome.

## 0x08025A18 Fight_DispatchOnSubstateAA_AI_SetActionResultCodes (AI, idx 0x11)
- Assinatura: `undefined4 f(void)`
- O que faz: zera +0xCA do bot, grava em +0xCB/+0xCC o resultado de `FUN_08036E54` sobre os stats +0xE8/+0xEC,
  grava código 0x2E ou 0x2F em +0xC9 conforme a técnica, avança +1.
- Memória: escreve +0xC9, +0xCA, +0xCB, +0xCC do bot; lê stats +0xE8/+0xEC; lê técnica.
- Para o Lua: porta; falta nome de `FUN_08036E54`.
- Confiança: confirmado (decompile); nome provável.

## 0x08025AAC Fight_DispatchOnSubstateAA_AI_12_SelectMessageByTechnique (AI, idx 0x12)
- Assinatura: `void f(void)`
- O que faz: lê o byte de técnica da peça do bot ativo (+0x87, parte +0xA7; campo +0x49 de registro de 0x18 bytes).
  Se é `'.'` grava +0xA6=0x31; senão 0x32. Avança +1.
- Memória: lê +0x87, +0xA7 e a tabela de peças; escreve +0xA6, +0xAA.
- Para o Lua: porta (o código de mensagem depende da tabela de textos).
- Confiança: confirmado (decompile); nome provável.

## 0x08025B20 Fight_DispatchOnSubstateAA_AJ_00_AccumPartCheckTargetCost (AJ, idx 0x00)
- Assinatura: `undefined4 f(void)`
- O que faz: mesmo início do AI_00 (`Fight_AccumRosPart`, `Fight_ApplyStatusCostIfFlag`, `FUN_0803725c`). Se
  `FUN_0803725c` == 0 e o byte +0xC1 do bot alvo (+0x88) é 0, grava +0xA6=0x74 e avança +1. Se o byte não é 0,
  incrementa o contador de uso, chama `FUN_08037280` e vai para o subestado 4. Se `FUN_0803725c` != 0, grava
  +0xA6=0x1A e avança +1.
- Memória: lê +0x87, +0x88, +0xA7, +0xC1 do bot alvo; escreve +0xA6, +0xAA.
- Para o Lua: porta junto com o AI_00.
- Confiança: confirmado (decompile); nome provável.

## 0x08025B9C Fight_DispatchOnSubstateAA_AJ_08_SetupTargetGraphicsSprite (AJ, idx 0x08)
- Assinatura: `undefined4 f(void)`
- O que faz: carrega gráfico (`decompress_and_store` + `bios_cpufastset` de 16 words); liga bits 0x220 no IO
  base+0x840; zera campos de controle; monta o registro de sprite do bot alvo (+0x88) a partir de base+0x1742 e
  base+0x1744 (Y − 4); monta o slot 0x21 com `Slot_SetupFromRecordList` (registro do literal +0x120); timer +0x86 = 0x3C;
  avança +1.
- Memória: lê +0x88 do bot e registros de sprite; escreve IO, registros de sprite, slot 0x21, +0x86, +0xAA.
- Para o Lua: porta (depende de LZ77 e OAM).
- Confiança: confirmado (decompile); nome provável. Muito parecido com AN 0x04, sem o SFX.

## 0x08025C84 Fight_DispatchOnSubstateAA_AJ_SwapActorAndTarget (AJ, idx 0x0B)
- Assinatura: `undefined4 f(void)`
- O que faz: `FightPreview_SplitValueAgainstLimit` com +0xC1 e stat +0xEC; +0x51 recebe o byte de +0xC8; divide
  contra 0x50 (dobrado se bit 8 de +0xC0); +0x52 = +0xC8; troca bot ativo (+0x87) com alvo (+0x88); chama
  `Fight_ResetBotVisualCounters`; avança +1.
- Memória: lê +0xC0, +0xC1, +0xC8, +0xEC, +0x87, +0x88; escreve +0x51, +0x52, +0x87, +0x88, +0xAA.
- Para o Lua: porta (troca de ator e alvo).
- Confiança: confirmado (decompile); nome provável.

## 0x08025D2C Fight_DispatchOnSubstateAA_AJ_0C_RunDispatchCWaitDone (AJ, idx 0x0C)
- Assinatura: `undefined4 f(void)`
- O que faz: chama `Fight_RunSubstateDispatchC(0)` (outro despachante de subestado, ainda sem nome); se retorna != 0,
  avança +1. Senão fica no mesmo subestado.
- Memória: escreve +0xAA.
- Para o Lua: porta quando `Fight_RunSubstateDispatchC` estiver documentado.
- Confiança: confirmado (decompile); nome provável.

## 0x08025D4C Fight_DispatchOnSubstateAA_AJ_0D_RunDispatchCPlaySfx51 (AJ, idx 0x0D)
- Assinatura: `undefined4 f(void)`
- O que faz: chama `Fight_RunSubstateDispatchC(0)` (ignora o retorno), toca `snd_sfx_request(0x51)` e avança +1.
- Memória: escreve +0xAA.
- Para o Lua: porta (o SFX é do Toolkit de som; o `Fight_RunSubstateDispatchC` ainda sem nome).
- Confiança: confirmado (decompile); nome provável.

## 0x08025D6C Fight_DispatchOnSubstateAA_AJ_0E_StepBotVisualDownWaitSlot (AJ, idx 0x0E)
- Assinatura: `undefined4 f(void)`
- O que faz: chama `Fight_RunSubstateDispatchC(0)`; `Fight_StepBotVisualDown` no bot ativo (+0x87) devolve um
  contador; roda `Anim_RunSlotScript` no slot 0x21 (ponteiro da tabela indexada por +0x84 do bot ativo); se o byte
  de controle do slot == -1, soma 1 ao contador. Se o contador chega a 2, chama `Fight_InitBattleSprites` e
  `Fight_ResetBotVisualCounters` e avança +1.
- Memória: lê +0x84, +0x87 do bot e o byte de controle do slot; escreve +0xAA.
- Para o Lua: porta quando as funções auxiliares estiverem documentadas.
- Confiança: provável (a leitura do byte de controle e o índice da tabela não foram cruzados com a ROM); nome provável.

## 0x08025DD4 Fight_DispatchOnSubstateAA_AJ_RestoreActorAndSetupSprite (AJ, idx 0x0F)
- Assinatura: `undefined4 f(void)`
- O que faz: restaura +0x87 ← +0x64, +0x51 ← +0x52, liga bits 0x220 no IO base+0x840, monta sprite do bot
  (registro +0x1742/+0x1744 do bot) e avança +1.
- Memória: lê +0x64, +0x52; escreve +0x87, +0x51, IO, +0xAA.
- Para o Lua: porta (depende da representação de OAM).
- Confiança: confirmado (decompile); nome provável.

## 0x08025E88 Fight_DispatchOnSubstateAA_AJ_13_StepBotVisualUpWaitIO (AJ, idx 0x13)
- Assinatura: `undefined4 f(void)`
- O que faz: chama `Fight_RunSubstateDispatchC(0)`; `Fight_StepBotVisualUp` no bot ativo (+0x87) devolve um
  contador. Se o bit 0x8000 do IO base+0x840 está apagado, soma 1 ao contador; se está aceso, liga 0x220 e mexe
  num byte de controle (+DAT): se vale 1 chama `Tile_ClearSlots(0x21, 1)`, senão decrementa esse byte e outro campo.
  Se o contador chega a 2, chama `Fight_InitBattleSprites` e `Fight_ResetBotVisualCounters` e avança +1.
- Memória: lê +0x87 e IO base+0x840; escreve IO, campos de controle, +0xAA.
- Para o Lua: porta quando as funções auxiliares estiverem documentadas.
- Confiança: provável (o campo de controle e o significado do bit 0x8000 não foram verificados); nome provável.

## 0x08025F14 Fight_DispatchOnSubstateAA_AJ_14_SetMessage75Advance (AJ, idx 0x14)
- Assinatura: `void f(void)`
- O que faz: grava +0xA6 = 0x75 (código de mensagem) e avança +1.
- Memória: escreve +0xA6, +0xAA.
- Para o Lua: porta.
- Confiança: confirmado (decompile); nome provável.

## 0x08025F54 Fight_DispatchOnSubstateAA_AK_00_CheckPartFlagMsg22 (AK, idx 0x00)
- Assinatura: `void f(void)`
- O que faz: pega o registro de 8 bytes da peça indicada por +0x8A do bloco (`+0x8A*8`). Se o byte +0x2A == 0 ou
  o byte +0x2B != '3', grava +0xA6 = 0x22, liga +0xAB = 1 e vai para o subestado 2. Senão zera +0xAB e avança +1.
- Memória: lê +0x8A e +0x2A/+0x2B do registro de peça; escreve +0xA6, +0xAB, +0xAA.
- Para o Lua: porta (depende do formato do registro de peça).
- Confiança: confirmado (decompile); nome provável. O significado de +0xAB não foi confirmado.

## 0x08025FC8 Fight_DispatchOnSubstateAA_AL_00_SetupThreeSlotSprites (AL, idx 0x00)
- Assinatura: `undefined4 f(void)`
- O que faz: `decompress_and_store` + `bios_cpufastset` de 8 words. Para os 3 slots (0x21..0x23) monta o registro de
  sprite de 0x40 bytes: tile (literal), X = 0x58 − 0x18*i, Y = 0x88, ou 0x68 com bit 0x1000 se o bit 0 de +0xF8 está
  aceso; chama `Slot_SetupFromRecordList` (registro do literal +0xE0). Toca `snd_sfx_request(0x52)` e avança +1.
- Memória: lê +0xF8; escreve registros de sprite (base+0x40*slot), +0xAA.
- Para o Lua: porta (depende de LZ77 e OAM).
- Confiança: confirmado (decompile); nome provável.

## 0x08026080 Fight_DispatchOnSubstateAA_AL_WaitSlotAnimations (AL, idx 0x01)
- Assinatura: `undefined4 f(void)`
- O que faz: roda `Anim_RunSlotScript` nos 3 slots (0x21..0x23 no bloco de 0x40); se algum slot tem +0x22 == 0xFF,
  define timer +0x86 = 0x3C e avança +1.
- Memória: lê +0x22 de cada slot; escreve +0x86, +0xAA.
- Para o Lua: porta (espera animação terminar).
- Confiança: confirmado (decompile); nome provável.

## 0x080260EC Fight_DispatchOnSubstateAA_AL_03_ClearThreeSlots (AL, idx 0x03)
- Assinatura: `undefined4 f(void)`
- O que faz: `Tile_ClearSlots(0x21, 3)` (limpa os 3 slots de animação) e avança +1.
- Memória: escreve os slots 0x21..0x23, +0xAA.
- Para o Lua: porta.
- Confiança: confirmado (decompile); nome provável.

## 0x08026130 Fight_DispatchOnSubstateAA_AM_00_AccumPartCheckCostNoTech (AM, idx 0x00)
- Assinatura: `undefined4 f(void)`
- O que faz: como o AI_00, sem o ramo de técnica 16: `Fight_AccumRosPart` e `Fight_ApplyStatusCostIfFlag` no bot
  ativo (+0x87, parte +0xA7); se `FUN_0803725c` == 0, incrementa o contador de uso, chama `FUN_08037280` e vai para o
  subestado 4. Se != 0, grava +0xA6 = 0x1A e avança +1.
- Memória: lê +0x87, +0xA7; escreve +0xA6, +0xAA.
- Para o Lua: porta junto com o AI_00.
- Confiança: confirmado (decompile); nome provável.

## 0x08026184 Fight_DispatchOnSubstateAA_AM_04_SetupBotSpriteSlot (AM, idx 0x04)
- Assinatura: `undefined4 f(void)`
- O que faz: `decompress_and_store` + `bios_cpufastset` de 8 words; grava IO base+0x840 com constante (+0x1000 se o bit 0
  de +0xF8 está aceso); zera um byte de controle; monta o sprite do bot ativo (+0x87) em X = base+0x1742 + bot*0x100 e
  Y = base+0x1744 + bot*0x100 − 6, com campos fixos; chama `Slot_SetupFromRecordList` (registro do literal +0x128) no
  slot 0x21; avança +1.
- Memória: lê +0x87, +0xF8; escreve IO, registros de sprite, slot 0x21, +0xAA.
- Para o Lua: porta (depende de LZ77 e OAM).
- Confiança: confirmado (decompile); nome provável. A constante do IO e o byte de controle não foram nomeados.

## 0x08026284 Fight_DispatchOnSubstateAA_AM_WaitAnimThenPlaceSprite (AM, idx 0x05)
- Assinatura: `undefined4 f(void)`
- O que faz: roda `Anim_RunSlotScript` (slot 0x21); se o byte de controle == -1, grava 0x80 em dois campos,
  ajusta posição (-4 do registro do bot) e avança +1.
- Memória: lê +0x128 (slot), byte de controle; escreve campos do registro de sprite, +0xAA.
- Para o Lua: porta.
- Confiança: confirmado (decompile); nome provável.

## 0x080262FC Fight_DispatchOnSubstateAA_AM_StepDownCounterWithSfx (AM, idx 0x06)
- Assinatura: `undefined4 f(void)`
- O que faz: se o byte de contador == '@' (0x40), toca `snd_sfx_request(0x53)` e avança +1; senão subtrai 8 do
  contador e do campo ligado.
- Memória: lê/escreve contador; escreve +0xAA.
- Para o Lua: porta.
- Confiança: confirmado (decompile); nome provável.

## 0x08026340 Fight_DispatchOnSubstateAA_AM_07_StepCounterUntilP (AM, idx 0x07)
- Assinatura: `void f(void)`
- O que faz: lê um byte do bloco (índice do literal `DAT_0802635c`). Se é `'P'` (0x50), avança +1. Senão soma 4 a esse
  byte e a um campo de contador (literal `DAT_08026374`), sem mudar de subestado.
- Memória: lê/escreve o byte de controle e o contador; escreve +0xAA quando avança.
- Para o Lua: porta.
- Confiança: confirmado (decompile); nome provável. O significado de 'P' como fim da espera é provável, não confirmado.

## 0x08026378 Fight_DispatchOnSubstateAA_AM_08_StepDownCounterUntilAt (AM, idx 0x08)
- Assinatura: `void f(void)`
- O que faz: lê o byte de contador do bloco (índice literal `DAT_0802639c`). Se é `'@'` (0x40), grava timer +0x86 = 0x3C
  e avança +1. Senão subtrai 4 desse byte e 4 de um campo (literal `DAT_080263b8`), sem mudar de subestado.
- Memória: lê/escreve contador; escreve +0x86 e +0xAA quando avança.
- Para o Lua: porta. É a variante sem SFX do AM_06.
- Confiança: confirmado (decompile); nome provável.

## 0x080263BC Fight_DispatchOnSubstateAA_AM_0A_ClearSlotSetPartDisplayValue (AM, idx 0x0A)
- Assinatura: `undefined4 f(void)`
- O que faz: `Tile_ClearSlots(0x21, 1)`; liga o bit 0x10 do byte +0xBD do bot ativo (+0x87); copia o byte de técnica
  da peça (registro de 0x18 bytes, campo +0x49) para +0xDF do bot; zera o campo de 16 bits +0xD8 e soma a ele o campo
  +0x4E da peça com teto 0xFF (`Fight_AddByteClampToMax`). Avança +1.
- Memória: lê +0x87, +0xA7 e campos da peça; escreve +0xBD, +0xD8, +0xDF, +0xAA.
- Para o Lua: porta.
- Confiança: confirmado (decompile); nome provável. Os significados de +0xBD, +0xD8 e +0xDF não foram confirmados.

## 0x08026444 Fight_DispatchOnSubstateAA_AM_0B_SetMessage2AAdvance (AM, idx 0x0B)
- Assinatura: `void f(void)`
- O que faz: grava +0xA6 = 0x2A (código de mensagem) e avança +1.
- Memória: escreve +0xA6, +0xAA.
- Para o Lua: porta.
- Confiança: confirmado (decompile); nome provável.

## 0x08026484 Fight_DispatchOnSubstateAA_AN_00_AccumPartCheckCostFlagAB (AN, idx 0x00)
- Assinatura: `undefined4 f(void)`
- O que faz: mesmo começo do AI_00 (`Fight_AccumRosPart`, `Fight_ApplyStatusCostIfFlag`, `FUN_0803725c`). Se
  `FUN_0803725c` == 0: incrementa o contador de uso, chama `FUN_08037280`, zera +0xAB e vai para o subestado 4. Senão
  liga +0xAB = 1, grava +0xA6 = 0x1A e avança +1.
- Memória: lê +0x87, +0xA7; escreve +0xAB, +0xA6, +0xAA.
- Para o Lua: porta junto com o AI_00.
- Confiança: confirmado (decompile); nome provável.

## 0x080264E4 Fight_DispatchOnSubstateAA_AN_SetupEffectSpritesAndSfx (AN, idx 0x04)
- Assinatura: `undefined4 f(void)`
- O que faz: LZ77 para gráfico + cpufastset 16 words; IO base+0x840 = const; monta registro de sprite do bot
  (+0x1742/+0x1744) e dois slots (`Slot_SetupFromRecordList` 0x21 e 0x22); toca `snd_sfx_request(0x5E)`; zera
  timer +0x86; avança +1.
- Memória: lê +0x87 do bot e registros; escreve IO, registros de sprite, +0x86, +0xAA.
- Para o Lua: porta (depende de LZ77 e OAM).
- Confiança: confirmado (decompile); nome provável.

## 0x08026628 Fight_DispatchOnSubstateAA_AN_WaitFramesWithAnimations (AN, idx 0x05)
- Assinatura: `undefined4 f(void)`
- O que faz: a cada frame roda `Anim_RunSlotScript` nos slots 0x21 e 0x22 e incrementa timer +0x86 até 0x5A (90);
  ao chegar, avança +1.
- Memória: lê/escreve +0x86; escreve +0xAA.
- Para o Lua: porta (espera de 90 frames).
- Confiança: confirmado (decompile); nome provável.

## 0x08026678 Fight_ApplyRandomPartReplacementEffect (AN, idx 0x06)
- Nome já existia no mtbe.csv (sem renomear). Ver mtbe.csv para a descrição completa.
- O que faz (leitura desta rodada): roda animações 0x21/0x22, marca +0x118=1, troca a peça por uma sorteada
  (`FUN_08037578`, `GetPartStats`, cópia de 0x18 bytes), e chama `Fight_BuildPartActionMetadataForAllParts`,
  `Fight_SelectActionSource`, `Fight_RefreshDisplayedPartSpritesForActiveBots` e `Fight_PlaceActiveBotObjects`; avança +1.
- Confiança: confirmado (decompile).

## 0x08026810 Fight_DispatchOnSubstateAA_AN_07_ClearSlotsCopyPartBlobs (AN, idx 0x07)
- Assinatura: `undefined4 f(void)`
- O que faz: `Tile_ClearSlots(0x21, 2)`; duas cópias de 15 bytes (`memcpy`) para dois ponteiros de uma tabela indexada
  por campos do bot (+0xA7 e bytes de +0x11A/+0x... do bloco); grava +0xA6 = 0x2F e avança +1.
- Memória: lê +0xA7 e índices; escreve a tabela apontada (buffers de 15 bytes), +0xA6, +0xAA.
- Para o Lua: porta quando a tabela de destino estiver nomeada.
- Confiança: provável (a fonte dos 15 bytes e o destino não foram nomeados); nome provável.

## 0x080268AC Fight_DispatchOnSubstateAA_AO_01_RollPartValueForBot (AO, idx 0x01)
- Assinatura: `undefined4 f(void)`
- O que faz: chama `Fight_RollPartValue` (bot ativo +0x87) e `FUN_080376f4` (mesmo bot); avança +1.
- Memória: lê +0x87; escreve +0xAA (e o que as duas funções escrevem).
- Para o Lua: porta quando `Fight_RollPartValue` e `FUN_080376f4` estiverem documentadas.
- Confiança: confirmado (decompile); nome provável.

## 0x080268D4 Fight_DispatchOnSubstateAA_AO_ClampPartCountsToLimit (AO, idx 0x02)
- Assinatura: `undefined4 f(void)`
- O que faz: chama `FightPreview_ClampPartCountToLimit` para as 4 posições (0..3) do bot com limite do stat +0xEC;
  avança +1.
- Memória: lê +0x88, +0xEC; escreve (via clamp) os contadores de peça; +0xAA.
- Para o Lua: porta.
- Confiança: confirmado (decompile); nome provável.

## 0x08026924 Fight_DispatchOnSubstateAA_AO_ShowFourPartValues (AO, idx 0x03)
- Assinatura: `undefined4 f(void)`
- O que faz: LZ77 + cpufastset 8; escolhe linha de stat pelo valor 2 em +0xF8 (bit 1); escreve 4 números de peça
  (`Fight_WriteNumberTiles`, campos +0xD6/+0xD8/+0xDA/+0xDC) e monta as linhas de sprite; toca `snd_sfx_request(0x5B)`; avança +1.
- Memória: lê +0xD6..+0xDC, +0xF8; escreve tiles/sprites; +0xAA.
- Para o Lua: porta (depende de LZ77 e tiles).
- Confiança: confirmado (decompile); nome provável.

## 0x08026A24 Fight_DispatchOnSubstateAA_AO_WaitPartRowsDrawn (AO, idx 0x04)
- Assinatura: `undefined4 f(void)`
- O que faz: chama `FUN_08036B50` para as 4 linhas de peça (posições 10, 14, 18, 22); se a última retorna != 0,
  grava timer 0x3C e avança +1.
- Memória: escreve timer; +0xAA.
- Para o Lua: porta.
- Confiança: confirmado (decompile); nome provável.

## 0x08026A74 Fight_DispatchOnSubstateAA_AO_DrawPartIconsWaitReady (AO, idx 0x07)
- Assinatura: `undefined4 f(void)`
- O que faz: `FUN_08036BB8` nas 4 peças; desenha ícones (`FightPreview_LoadPartIconRows`, `FightPreview_DrawPartIconPairs`);
  se as 4 retornam 1, grava timer 0x3C e avança +1.
- Memória: lê estado de cada peça; escreve timer, +0xAA.
- Para o Lua: porta.
- Confiança: confirmado (decompile); nome provável.

## 0x08026AF4 Fight_DispatchOnSubstateAA_AO_ShowPartFlagsOrBackup (AO, idx 0x09)
- Assinatura: `undefined4 f(void)`
- O que faz: LZ77 + cpufastset 8; para cada peça com flag +0xD2..+0xD5 != 0 chama `FUN_08036C20` (efeito por peça).
  Se todas as flags são zero, avança +1; senão `Fight_BackupHalvedStatsIfFlagClear` e grava subestado 0x0D.
- Memória: lê +0xD2..+0xD5, +0xF8; escreve +0xAA.
- Para o Lua: porta.
- Confiança: confirmado (decompile); nome provável.

## 0x08026BF8 Fight_DispatchOnSubstateAA_AO_0D_WaitPartEffectAnimsClear (AO, idx 0x0D)
- Assinatura: `undefined4 f(void)`
- O que faz: é o destino de 0x09 quando há backup. Para cada uma das 4 peças com flag +0xD2..+0xD5 do bot aceso, roda
  `Anim_RunSlotScript` nos slots 10, 11, 12, 13 (no literal do bloco de animação) e marca se o byte de controle do
  slot é -1. Se alguma marcou, `Tile_ClearSlots(10, 4)` e avança +1.
- Memória: lê +0xD2..+0xD5; escreve +0xAA e os slots 10..13.
- Para o Lua: porta.
- Confiança: confirmado (decompile); nome provável.

## 0x08026CBC Fight_QueueHeadBreakIfDefenderHeadZero (AO, idx 0x10)
- Nome já existia no mtbe.csv (sem renomear). Leitura desta rodada confere.
- O que faz: se o byte +0x59 do bot alvo é 0, chama `Fight_QueueHeadBrokenBotForResolution`; avança +1.
- Confiança: confirmado (decompile).

## 0x08026D1C Fight_DispatchOnSubstateAA_AP_01_WaitUnlessFlagB8Bit1 (AP, idx 0x01)
- Assinatura: `void f(void)`
- O que faz: se o bit 0x02 do byte +0xB8 do bloco de luta está apagado, avança +1; se está aceso, grava o subestado
  0x20 (' ') em vez de avançar.
- Memória: lê +0xB8; escreve +0xAA.
- Para o Lua: porta.
- Confiança: confirmado (decompile); nome provável. O significado de +0xB8 bit 1 não foi confirmado.

## 0x08026D44 Fight_DispatchOnSubstateAA_AP_ApplyDamageToRandomTargetPart (AP, idx 0x15)
- Assinatura: `void f(void)`
- O que faz: se +0xCA == 0 avança +1; senão sorteia peça alvo (`Fight_SelectRandomTargetPartForSelectedBot`),
  copia alvo para fight_information (`Fight_CopySelectedTargetToFightInfo`), +0xD0 = +0x8B, aplica dano
  (`Fight_ApplyDamageToPart`) e avança +3. Depois, salto por outra tabela (não recuperada).
- Memória: lê +0x87, +0x88, +0x8B, +0xCA; escreve +0xD0, +0xAA.
- Para o Lua: porta.
- Confiança: confirmado (decompile); nome provável.

## Máquina de estados (esboço, por despachante; "→ n" = próximo)

- AH: 0x12 FormatCounterDigits → 0x13.
- AI: 0x00 AccumPartCheckCost → 0x01 (ou 0x1A/0x39; ou 4 com uso); 0x08 LoadTechniqueGraphics → 0x09 + salto;
  0x0B SetupTargetSpriteSfx → 0x0C; 0x11 SetActionResultCodes → 0x12; 0x12 SelectMessageByTechnique → 0x13.
- AJ: 0x00 AccumPartCheckTargetCost → 0x01 (ou 0x74; ou 4 com uso); 0x08 SetupTargetGraphicsSprite → 0x09;
  0x0B SwapActorAndTarget → 0x0C; 0x0C → 0x0D; 0x0D → 0x0E (SFX 0x51); 0x0E StepBotVisualDownWaitSlot → 0x0F;
  0x0F RestoreActorAndSetupSprite → 0x10; 0x13 StepBotVisualUpWaitIO → 0x14; 0x14 SetMessage75Advance → 0x15.
- AK: 0x00 CheckPartFlagMsg22 → 0x01 (ou 0x02 com msg 0x22).
- AL: 0x00 SetupThreeSlotSprites → 0x01; 0x01 WaitSlotAnimations → 0x02 (ou timer); 0x03 ClearThreeSlots → 0x04.
- AM: 0x00 AccumPartCheckCostNoTech → 0x01 (ou 0x04 com uso); 0x04 SetupBotSpriteSlot → 0x05; 0x05 WaitAnimThenPlaceSprite
  → 0x06; 0x06 StepDownCounterWithSfx → 0x07 quando '@'; 0x07 StepCounterUntilP → 0x08 quando 'P';
  0x08 StepDownCounterUntilAt → 0x09 quando '@'; 0x0A ClearSlotSetPartDisplayValue → 0x0B; 0x0B SetMessage2AAdvance → 0x0C.
- AN: 0x00 AccumPartCheckCostFlagAB → 0x01 (ou 0x04 com uso); 0x04 SetupEffectSpritesAndSfx → 0x05; 0x05 WaitFramesWithAnimations
  (90 f) → 0x06; 0x06 ApplyRandomPartReplacementEffect → 0x07; 0x07 ClearSlotsCopyPartBlobs → 0x08 (msg 0x2F).
- AO: 0x01 RollPartValueForBot → 0x02; 0x02 ClampPartCountsToLimit → 0x03; 0x03 ShowFourPartValues → 0x04; 0x04 WaitPartRowsDrawn
  → 0x05; 0x07 DrawPartIconsWaitReady → 0x08; 0x09 ShowPartFlagsOrBackup → 0x0A ou 0x0D; 0x0D WaitPartEffectAnimsClear → 0x0E;
  0x10 QueueHeadBreakIfDefenderHeadZero → 0x11.
- AP: 0x01 WaitUnlessFlagB8Bit1 → 0x02 (ou vai para 0x20); 0x15 ApplyDamageToRandomTargetPart → 0x16 ou +3.

Pontos em aberto: os dois saltos por tabela não recuperados (AI 0x08, AP 0x15); os nomes `FUN_0803725c`, `FUN_08037280`,
`Fight_RunSubstateDispatchC`, `FUN_08036E54`, `FUN_08036B50`, `FUN_08036BB8`, `FUN_08036C20`, `FUN_08037578`,
`FUN_080376f4` e `Fight_RollPartValue` ainda sem documentação própria (são chamados por estes handlers).
O significado dos campos +0xAB, +0xBD, +0xB8 bit 1, +0xD8 e +0xDF não foi confirmado.
