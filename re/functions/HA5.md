# HA5: handlers de Fight_DispatchOnSubstateAA_T (parcial: 7 de 42 lidos)

Lista: `HA5_lista.txt` (42 entradas, todas na tabela do despachante T; as de U..Z ainda não lidas).
Bloco da batalha: `0x03001800`; subestado no byte +0xAA; timer em +0x86 (halfword). Os slots de animação
são 0x21, 0x22, 0x23 (chamados com `Anim_RunSlotScript` / `Slot_SetupFromRecordList`).
Ghidra: nenhuma renomeação aplicada nesta rodada (nomes abaixo são propostos).

## 0x080219C0 Fight_DispatchOnSubstateAA_T_PlayThenSetup
- Assinatura: `void f(void)`
- O que faz: toca o roteiro do slot 0x21; enquanto o byte de dados (DAT_08021A04+DAT_08021A08) não é 0xFF, espera. Ao fim: prepara o slot 0x21 a partir da lista de registros, timer +0x86 = 0x3C e +0xAA++.
- Chama: Anim_RunSlotScript, Slot_SetupFromRecordList. Chamada por: T (índice 0x09).
- Memória: lê +0xE4, +0xEC (ponteiros do bloco); escreve +0x86, +0xAA.
- Para o Lua: porta junto com a animação de slot.
- Confiança: confirmado (decompile).

## 0x08021A14 Fight_DispatchOnSubstateAA_T_WaitTimer
- Assinatura: `void f(void)`
- O que faz: toca o slot 0x21, decrementa o timer +0x86; zera o timer se +0x89 == 0 e o bit 0 de `*DAT_08021A64` estiver ligado (salto de espera); ao chegar a 0, +0xAA++.
- Chama: Anim_RunSlotScript. Chamada por: T (índice 0x0A).
- Memória: lê/escreve +0x86 (halfword), lê +0x89; escreve +0xAA. Lê byte de flag apontado por DAT_08021A64.
- Para o Lua: porta.
- Confiança: confirmado (decompile); o significado do bit em `*DAT_08021A64` é chute.

## 0x08021A6C Fight_DispatchOnSubstateAA_T_SetupPositions
- Assinatura: `void f(void)`
- O que faz: toca o slot 0x21; copia 0x40 bytes em dois blocos (FUN_0803F63C); grava +0x32=0x58, +0x72=0x40, +0xB2=0x28; grava +0x24/+0x64/+0xA4=6; grava +0x30/+0x70/+0xB0 = 0x68 ou 0x88 (conforme bit 0 de +0xF8 de outra struct); +0xAA++.
- Chama: Anim_RunSlotScript, FUN_0803F63C. Chamada por: T (índice 0x0B).
- Memória: escreve +0x24, +0x30, +0x32, +0x64, +0x70, +0x72, +0xA4, +0xB0, +0xB2, +0xAA; lê +0xF8 da struct vizinha.
- Para o Lua: porta (posições iniciais dos três blocos).
- Confiança: confirmado (decompile); o significado dos valores 0x58/0x40/0x28 é chute.

## 0x08021AFC Fight_DispatchOnSubstateAA_T_WaitSlotsReady
- Assinatura: `void f(void)`
- O que faz: toca os slots 0x21, 0x22, 0x23; move cada um para o alvo (Fight_StepSlotTowardTarget); quando os três chegam, prepara os três slots a partir da lista de registros; +0xAA++.
- Chama: Anim_RunSlotScript, Fight_StepSlotTowardTarget, Slot_SetupFromRecordList. Chamada por: T (índice 0x0C).
- Memória: lê +0xE8/+0xEC; escreve +0xAA.
- Para o Lua: porta.
- Confiança: confirmado (decompile).

## 0x08021B78 Fight_DispatchOnSubstateAA_T_WaitScriptEndClearMask
- Assinatura: `void f(void)`
- O que faz: toca os slots 0x21..0x23; quando o byte de dados em (DAT_08021BC8+DAT_08021BCC) é 0xFF: limpa a máscara de atributo do slot 0x21 (Slot_ClearAttrMask, 3), timer +0x86 = 0x3C, +0xAA++.
- Chama: Anim_RunSlotScript, Slot_ClearAttrMask. Chamada por: T (índice 0x0D).
- Memória: escreve +0x86, +0xAA.
- Para o Lua: porta.
- Confiança: confirmado (decompile).

## 0x08021BD8 Fight_DispatchOnSubstateAA_T_InitGraphicsAndSlots
- Assinatura: `void f(void)`
- O que faz: descomprime dados (decompress_and_store) e copia com bios_cpufastset (8); prepara os slots 0x21..0x23; liga bit 15 de flags no slot 0x21 (Slot_SetFlagBit15, 3); zera os quatro bytes de 6 blocos de 0x100 (apenas quando `*(pc+0x40)` não é zero); timer +0x86 = 0; +0xAA++.
- Chama: decompress_and_store, bios_cpufastset (SWI), Slot_SetupFromRecordList, Slot_SetFlagBit15. Chamada por: T (índice 0x0F).
- Memória: escreve +0x86, +0xAA e os blocos de 0x100 a partir de DAT_08021C84+DAT_08021C88 (dados não identificados).
- Para o Lua: porta a parte de slots; a descompressão é do loader da ROM (não precisa no Lua).
- Confiança: confirmado (decompile); destino dos blocos = chute.

## 0x08021C94 Fight_DispatchOnSubstateAA_T_RunSlotsThenClear
- Assinatura: `void f(void)`
- O que faz: incrementa o timer +0x86 e chama FUN_080353AC(timer); em 6 blocos de 0x100, se um byte de flag (+0x40) estiver ligado, zera quatro entradas via FUN_08036AC4; toca os slots 0x21..0x23; ao fim do roteiro (byte de dados == 0xFF): zera 6 blocos de 0x100 (mesmo laço), zera +2 do registro, Tile_ClearSlots(0x21, 3), +0xAA++.
- Chama: FUN_080353AC, FUN_08036AC4, Anim_RunSlotScript, Tile_ClearSlots. Chamada por: T (índice 0x10).
- Memória: escreve +0x86, +0xAA; escreve blocos de 0x100 (dados não identificados); lê +0x40 de blocos de 0x100.
- Para o Lua: porta a parte de slots; os blocos são a conferir.
- Confiança: confirmado (decompile).

## Não lidos / sem função no Ghidra
- 0x08021DAC (T 0x11) e 0x08021DEC (T 0x12): Ghidra responde "No function found at or containing address". Provável: não é início de função (miolo ou dado). Conferir disassemble antes de documentar.
- Restantes (33 entradas: U 6, V 4, W 6, X 9, Y 5, Z 3): não lidos nesta rodada.
