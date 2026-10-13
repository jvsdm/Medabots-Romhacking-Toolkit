# HA5: handlers de Fight_DispatchOnSubstateAA_T (parcial: 9 de 42 lidos)

Lista: `HA5_lista.txt` (42 entradas, todas na tabela do despachante T; as de U..Z ainda não lidas).
Bloco da batalha: `0x03001800`; subestado no byte +0xAA. Timer dos handlers T: halfword em `0x03000440+0x86`
(literal `DAT_08021A60`/`DAT_08021C8C`), não no bloco 0x03001800. Os slots de animação são 0x21, 0x22, 0x23
(chamados com `Anim_RunSlotScript` / `Slot_SetupFromRecordList`); o contexto de animação é `0x083B6100`
(+0xEC = tabela de slots, +0x8C = lista de registros).
Ghidra: renomeados nesta rodada (redo HA5) com o padrão `Fight_DispatchOnSubstateAA_T_<idx>_<Passo>`.

## 0x080219C0 Fight_DispatchOnSubstateAA_T_09_PlayThenSetup
- Assinatura: `void f(void)`
- O que faz: toca o roteiro do slot 0x21; enquanto o byte de dados (DAT_08021A04+DAT_08021A08) não é 0xFF, espera. Ao fim: prepara o slot 0x21 a partir da lista de registros, timer = 0x3C e +0xAA++.
- Chama: Anim_RunSlotScript, Slot_SetupFromRecordList. Chamada por: T (índice 0x09).
- Memória: lê +0xE4, +0xEC do contexto 0x083B6100; lê byte 0x03003492 (fim do roteiro); escreve timer (0x03000440+0x86) = 0x3C e +0xAA.
- Para o Lua: porta junto com a animação de slot.
- Confiança: confirmado (decompile). O byte de dados (DAT_08021A04+DAT_08021A08) é o marcador de fim do roteiro (0xFF).

## 0x08021A14 Fight_DispatchOnSubstateAA_T_0A_WaitTimer
- Assinatura: `void f(void)`
- O que faz (decompile): chama `Anim_RunSlotScript(*(0x083B6100+0xEC), 0x21)`; decrementa o timer `0x03000440+0x86`; se o byte `0x03000440+0x89` for 0 e o bit 0 do byte `0x03001798` estiver ligado, zera o timer (salto de espera); quando o timer chega a 0, `0x03001800+0xAA++`.
- Chama: Anim_RunSlotScript. Chamada por: T (índice 0x0A).
- Memória: lê/escreve timer 0x03000440+0x86 (halfword); lê 0x03000440+0x89; lê bit 0 de 0x03001798; escreve 0x03001800+0xAA.
- Para o Lua: porta.
- Confiança: confirmado (decompile) para o fluxo. O significado do bit 0 de 0x03001798 (fight_info-0x68) e do byte +0x89 é chute.

## 0x08021A6C Fight_DispatchOnSubstateAA_T_0B_SetupPositions
- Assinatura: `void f(void)`
- O que faz (decompile): chama `Anim_RunSlotScript(0x083B6100+0xEC, 0x21)`; copia 0x40 bytes duas vezes com `FUN_0803F63C` (em 0x03003470+0x00 e +0x40, com origens +0x40 e +0x80: ordem dos argumentos provável); grava em 0x03003470: +0x32 = 0x58, +0x72 = 0x40, +0xB2 = 0x28 (halfwords); +0x24/+0x64/+0xA4 = 6 (bytes); +0x30/+0x70/+0xB0 = 0x68 ou 0x88 (halfwords), escolhido pelo bit 0 de 0x03001800+0xF8; `0x03001800+0xAA++`.
- Chama: Anim_RunSlotScript, FUN_0803F63C. Chamada por: T (índice 0x0B).
- Memória: escreve 0x03003470 nos offsets +0x24, +0x30, +0x32, +0x64, +0x70, +0x72, +0xA4, +0xB0, +0xB2; escreve 0x03001800+0xAA; lê 0x03001800+0xF8.
- Para o Lua: porta (posições iniciais dos três blocos).
- Confiança: confirmado (decompile) para as escritas. O significado de 0x58/0x40/0x28 e de 0x68/0x88 é chute; o bloco 0x03003470 é um objeto de posição, não identificado.

## 0x08021AFC Fight_DispatchOnSubstateAA_T_0C_WaitSlotsReady
- Assinatura: `void f(void)`
- O que faz: toca os slots 0x21, 0x22, 0x23; move cada um para o alvo (Fight_StepSlotTowardTarget); quando os três chegam, prepara os três slots a partir da lista de registros; +0xAA++.
- Chama: Anim_RunSlotScript, Fight_StepSlotTowardTarget, Slot_SetupFromRecordList. Chamada por: T (índice 0x0C).
- O que faz (decompile): toca os slots 0x21, 0x22, 0x23; chama Fight_StepSlotTowardTarget em cada um e conta quantos retornaram "chegou"; quando a contagem é 3, prepara os três slots a partir de `*(0x083B6100+0xE8)` e `0x03001800+0xAA++`.
- Chama: Anim_RunSlotScript, Fight_StepSlotTowardTarget, Slot_SetupFromRecordList. Chamada por: T (índice 0x0C).
- Memória: lê +0xE8/+0xEC; escreve +0xAA.
- Para o Lua: porta.
- Confiança: confirmado (decompile) para o fluxo; o retorno de Fight_StepSlotTowardTarget ser "chegou" é provável.

## 0x08021B78 Fight_DispatchOnSubstateAA_T_0D_WaitScriptEndClearMask
- Assinatura: `void f(void)`
- O que faz (decompile): toca os slots 0x21..0x23 (ponteiro em 0x083B6100+0xE8); quando o byte `0x03002C30+0x862` (= 0x03003492, marcador de fim de roteiro) é 0xFF: Slot_ClearAttrMask(0x21, 3), timer (0x03000440+0x86) = 0x3C, `0x03001800+0xAA++`.
- Chama: Anim_RunSlotScript, Slot_ClearAttrMask. Chamada por: T (índice 0x0D).
- Memória: escreve timer, +0xAA.
- Para o Lua: porta.
- Confiança: confirmado (decompile).

## 0x08021BD8 Fight_DispatchOnSubstateAA_T_0F_InitGraphicsAndSlots
- Assinatura: `void f(void)`
- O que faz (decompile): `decompress_and_store(DAT_08021C6C, DAT_08021C70)`; `bios_cpufastset(DAT_08021C74, DAT_08021C78, 8)`; prepara os slots 0x21, 0x22, 0x23 a partir de `*(0x083B6100+0x8C)`; `Slot_SetFlagBit15(0x21, 3)`; se o byte `bot0+0x40` (0x03000590+0x40) não é zero, zera 4 bytes em cada um dos 6 blocos de 0x100 (ponteiro base `0x03002C30+0x1763 = 0x03004393`, com passo de 0x40 entre os quatro bytes e 0x100 entre blocos); timer = 0; `0x03001800+0xAA++`.
- Chama: decompress_and_store, bios_cpufastset (SWI), Slot_SetupFromRecordList, Slot_SetFlagBit15. Chamada por: T (índice 0x0F).
- Memória: escreve timer, 0x03001800+0xAA e 24 bytes em 0x03004313..0x030043D3 (zeros, passo 0x100 entre bots). Lê flags 0x03000590+0x40+k*0x100 (k = 0..5, um por bot).
- Para o Lua: porta a parte de slots; a descompressão é do loader da ROM (não precisa no Lua).
- Confiança: confirmado (decompile) para o fluxo e os endereços; o que são os bytes 0x03004313.. (provavelmente uma tabela por bot) é chute.

## 0x08021C94 Fight_DispatchOnSubstateAA_T_10_RunSlotsThenClear
- Assinatura: `void f(void)`
- O que faz: incrementa o timer e chama FUN_080353AC(timer); em 6 blocos de 0x100 (flags em 0x03000590+0x40+k*0x100), se o byte de flag estiver ligado, zera quatro entradas via FUN_08036AC4; toca os slots 0x21..0x23; ao fim do roteiro (byte de dados == 0xFF): zera 6 blocos de 0x100 (mesmo laço), zera +2 do registro, Tile_ClearSlots(0x21, 3), `0x03001800+0xAA++`.
- Chama: FUN_080353AC, FUN_08036AC4, Anim_RunSlotScript, Tile_ClearSlots. Chamada por: T (índice 0x10).
- Memória: escreve timer, 0x03001800+0xAA; escreve blocos de 0x100 (dados não identificados); lê +0x40 de blocos de 0x100.
- Para o Lua: porta a parte de slots; os blocos são a conferir.
- Confiança: provável (lido no código antes; não reconferido nesta rodada).

## 0x08021DAC Fight_DispatchOnSubstateAA_T_11_ClearRecordSlot
- Assinatura: `void f(void)`
- O que faz (decompile): usa o byte `0x03001800+0x89` como índice `i`; zera 4 bytes em `0x03001800+0x2A+8*i` (+0x2A..+0x2D); `0x03001800+0xAA++`. Não toca em slots de animação.
- Chama: nada. Chamada por: T (índice 0x11).
- Memória: lê +0x89; escreve +0x2A..+0x2D (4 bytes de um registro de 8 bytes a partir de +0x2A) e +0xAA.
- Para o Lua: porta (é só uma limpeza de registro).
- Confiança: confirmado (decompile) para o efeito; o significado do vetor de registros de 8 bytes em +0x2A é chute.
- Obs.: antes o Ghidra não tinha função aqui ("miolo"/"sem função"); agora existe e é função própria.

## 0x08021DEC Fight_DispatchOnSubstateAA_T_12_SetStepCode25
- Assinatura: `void f(void)`
- O que faz (decompile): grava `0x03001800+0xA6 = 0x25` (código de passo/mensagem) e `0x03001800+0xAA++`.
- Chama: nada. Chamada por: T (índice 0x12).
- Memória: escreve +0xA6 e +0xAA.
- Para o Lua: porta (uma linha; o efeito de 0x25 depende do despachante de mensagem, ver +0xA6 em HANDOFF_DISCOVERY).
- Confiança: confirmado (decompile) para a escrita; o significado de 0x25 é chute.
- Obs.: antes era "sem função"; agora existe e é função própria.

## Não lidos nesta rodada
- Restantes (33 entradas: U 6, V 4, W 6, X 9, Y 5, Z 3): não lidos. Não eram seções a refazer (ainda não havia seção).

Pendente redo: U..Z (33 entradas em HA5_lista.txt) ainda sem seção. T 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0F, 0x10 foram reconferidos por decompile; "chute" restante só nos significados (0x58/0x40/0x28, bit 0x03001798, bytes 0x03004313..).
