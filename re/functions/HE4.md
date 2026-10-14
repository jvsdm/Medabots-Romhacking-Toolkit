# HE4: handlers de opcode de evento (0x69..0x88)

Tabela: `0x083D222C + 4*op` (ponteiros com bit Thumb; o endereço da seção é ponteiro-1, igual a HE4_lista.txt).
Revisão GH-HE4-redo (2026-10-13): todos os 32 handlers agora são funções no Ghidra e foram decompilados; literais
lidos na ROM EU. Base de evento E = 0x030057E0. Campos usados (todos confirmados nos literais):
- E+0xC8A (u16) cursor do script; E+0xC8C (u16) cursor salvo no início do handler (= byte depois do opcode).
  Saltos relativos: alvo = E+0xC8C + desloc. (bate com `alvo = at + 1 + byte` do extract_events.py).
- E+0xC89 = pedido pendente (1 = rodar a continuação); E+0xC92 = opcode atual; E+0xC93 = passo da continuação.
- E+0xC94.. (= 0x03006474) buffer de argumentos (`Script_ReadBytes(n)` 0x0804D8CC); `site_GetEventScript`
  0x0804D83C lê 1 byte.
- E+0xC5B (= 0x0300643B) gCurrentMapSceneVariant (HE1). E+0xCA4 (= 0x03005E84) tabela ator rastreado -> slot+1.
- Continuação: quem grava E+0xC89 = 1 é continuado por `gEventOpcodeTableB` 0x083D2630 + 4*op, indexado por
  E+0xC92 (por isso handlers com corpo idêntico, ex. 0x70..0x73, fazem coisas diferentes). Ao terminar, a continuação
  chama `0x08050290` (zera E+0xC89 e E+0xC93). Continuações lidas por disassembly Thumb dos bytes (só 0x0804FA1C é
  função no Ghidra).
- Comando de cena: `0x08050CC0` zera 0x03006D20+0xB (passo) e `0x08050CCC(cmd, 0)` roda a máquina de passos em
  0x03006D20 até devolver != 0.
Nomes Toolkit: `src/Medabots.Rom/Events/EventOpcodeDefinitions.cs`. PC: `pc/extract/extract_events.py` e `pc/game/src/script.lua`.
Tamanhos: o legado (`LegacyEventOpcodeTable.Lengths - 1`) bate com a ROM nos 32. O C# bate em todos os que define;
faltam no C# 0x6A, 0x75, 0x77 (no-op, 0 bytes), 0x7D (1), 0x7F (1), 0x88 (2). ROM_SIZE_FIX não toca esta faixa.

## 0x69 Set_Map_Scene_Variant (Toolkit: variant, skip_full_reload) — nome Ghidra: Event_Op69_Set_Map_Scene_Variant
- Lê 2 bytes. Só age se arg0 == variante atual (E+0xC5B): então variante = arg0 ^ 1 e chama
  `Map_ApplySceneVariantScrollPreset(nova)`. Se arg1 == 0: nova = 0 -> copia E+0xC10..C17 para E+0xC18 (guarda) e zera
  E+0xC10..C17; nova = 1 -> restaura E+0xC10..C17 de E+0xC18. arg1 != 0 pula a cópia.
- Ou seja, arg0 é a variante "de origem" (alterna 0<->1), não a de destino. Sem continuação (tabela B = 0x0804DC35).
- Confiança: confirmado. PC: **falta** (o warp do PC tem `variant`, mas o opcode 0x69 não é decodificado).

## 0x6A (sem definição no Toolkit)
- Só `bx lr` (função de 2 bytes no Ghidra, Event_Op6A_NoOp): 0 bytes. Confirmado. PC: **falta** (não necessário; no-op).

## 0x6B Set_Current_Chapter (Toolkit: chapter)
- Lê 1 byte (`bl 0x0804D83C`) e grava em 0x03005FDF (= 0x030057E0+0x7FF). Confirmado pelo código.
- PC: **implementado** (`extract_events.py` decodifica 0x6B; `script.lua` `chapter(value)`).

## 0x6C Initiate_Moving_Actor_C (Toolkit: packed_actor_id, sprite, x, y, move)
- Lê 5 bytes. `Map_FindFreeTrackedObjectSlot` 0x08050710 (primeiro objeto livre 0..5 em 0x030057E0+(0x50+i)*0x40,
  bit 0x4000) e `Event_SetTrackedActorSlot(arg0, slot)` (E+0xCA4[arg0] = slot+1). Grava 0x03006D20+0xA = slot+0x4F
  (índice do objeto) e liga o pendente.
- Continuação 0x0804F864: `FUN_08050358(arg0..arg3)` cria o objeto (sprite, x, y), depois `FUN_080500BC(obj, arg4, 1)`
  inicia o Move e espera `FUN_0805018C(obj, 1)` (fim do movimento).
- Confiança: confirmado (handler); provável (papel de 08050358/080500BC/0805018C). PC: **falta** (o extrator só tem 0x1F/0x21).

## 0x6D Move_Actor_D (Toolkit: tracked_object_slot, move)
- Lê 2 bytes. `Event_GetTrackedActorSlot(arg0)` (E+0xCA4[arg0]-1) -> 0x03006D20+0xA = slot+0x4F; liga o pendente.
- Continuação 0x0804F8E8: `FUN_080500BC(obj, arg1, 1)` e espera `FUN_0805018C(obj, 1)` (anda e bloqueia até acabar).
- Confiança: confirmado. PC: **parcial** (`extract_events.py` trata 0x6D como `actor_move` com 1 byte de Move; sem efeito no `script.lua`).

## 0x6E Initiate_Moving_Actor_D (Toolkit: packed_actor_id, sprite, x, y, move)
- Corpo idêntico ao 0x6C (5 bytes). A diferença está só na continuação 0x0804F940 (não lida em detalhe).
- Confiança: confirmado (handler). PC: **falta**.

## 0x6F Move_Actor_E (Toolkit: tracked_object_slot, move)
- Corpo idêntico ao 0x6D (2 bytes). Continuação própria 0x0804F9C4 (não lida). Confiança: confirmado (handler). PC: **falta**.

## 0x70, 0x71, 0x72, 0x73, 0x76 (sem argumentos; Toolkit: Run_Scene_Command_13/00/02/08, Start_Medabot_Link)
- Handler: só E+0xC89 = 1. 0 bytes. Confirmado no decompile (corpo idêntico nos cinco).
- Continuações (bytes iguais exceto o imediato): 0x70 -> `0x08050CCC(0x13,0)` (0x0804FA1C, nome Ghidra
  Event_Op70_Cont_Run_Scene_Command_13), 0x71 -> cmd 0x00 (0x0804FA5C), 0x72 -> cmd 0x02 (0x0804FA9C), 0x73 -> cmd 0x08
  (0x0804FADC). Os nomes do Toolkit batem com o número do comando.
- 0x76 (0x0804FB1C): `GameState_StartFlowByCommand(4, 0)`; checa UMA vez 0x03006D20+0xC == 7 (não espera): se não for 7, o opcode termina sem o comando 5; se for, roda o comando de cena 0x05 (HC5.md, 0x0804FB1C).
- Confiança: confirmado (comandos); provável (o que cada comando de cena faz). PC: **falta** (os cinco).

## 0x74 Reserved_NoOp, 0x75 (sem definição), 0x77 (sem definição), 0x7C Reserved_NoOp
- Só `bx lr` (funções de 2 bytes no Ghidra): 0 bytes, nada faz. Confirmado. PC: **falta**, não necessário (no-op).

## 0x78 Run_Scene_Command_10_If_At_Least_Three_Complete_Part_Sets (Toolkit: scene_arg, jump_if_fewer...)
- Salva o cursor (E+0xC8C = E+0xC8A), lê 2 bytes, grava arg0 em 0x03006D38. `Team_CountSlotsWithStatus3` 0x08047F9C
  conta registros do time (9 x 8 bytes em 0x030017A0) com byte0 == 3. Se < 3: cursor = salvo + arg1 (salto); senão liga
  o pendente. Continuação 0x0804FB8C: comando de cena 0x10, depois espera `0x0804B738` e zera 0x03005310.
- Confiança: confirmado (fluxo); provável ("byte0 == 3" = Medabot completo). PC: **parcial** (`BRANCH_OPS` trata 0x78 como
  salto no último byte; o teste e o comando 0x10 não estão no `script.lua`).

## 0x79 Run_Scene_Command_12 (Toolkit: scene_arg)
- Lê 1 byte para 0x03006D39 (0x03006D38+1) e liga o pendente. Continuação 0x0804FBDC: comando de cena 0x12.
- Confiança: confirmado. PC: **falta**.

## 0x7A Run_Scene_Command_11
- Só liga o pendente, 0 bytes. Continuação 0x0804FC1C: comando de cena 0x11, depois, se `0x0804B738` != 0, grava 1 em 0x03005349 (= 0x03005310+0x39; não zera 0x03005310) (HC5.md, 0x0804FC1C).
- Confiança: confirmado. PC: **falta**.

## 0x7B Jump_If_No_Spare_Part (Toolkit: bot, part, jump) — nome Ghidra: Event_Op7B_Jump_If_No_Spare_Part
- Salva o cursor, lê 3 bytes. `FUN_08047D9C(bot, part)` = `Inv_ReadCounter16`; se o byte alto do resultado == 0
  (nenhuma peça no inventário), cursor = salvo + arg2. Sem continuação. Confirmado pelo decompile.
- PC: **parcial** (`extract_events.py` trata 0x7B como salto em `BRANCH_OPS`; o teste não é feito em `script.lua`).

## 0x7D (sem definição no Toolkit) — nome Ghidra: Event_Op7D_Set_Field_C32
- Lê 1 byte e grava em 0x03006412 (= E+0xC32). Sem pendente, sem continuação. Uso do campo: desconhecido
  (mesma escrita que HN7.md 0x03006412). Confiança: confirmado (efeito), significado desconhecido. PC: **falta**.

## 0x7E Move_All_Tracked_Actors_C (Toolkit: move_pattern)
- Lê 1 byte para E+0xC94 e liga o pendente. Continuação 0x0804FC6C: `FUN_0804E5F0(0, 0)` (lê o padrão no buffer).
- Confiança: confirmado (handler); provável (continuação). PC: **falta**.

## 0x7F (sem definição no Toolkit)
- Lê 1 byte para E+0xC94 e liga o pendente (corpo igual ao 0x7E). Continuação 0x0804FC7C: copia blocos de 0x40 bytes:
  0x03003FF0+arg0*0x40 -> 0x03003830 e 0x03004A70+arg0*0x40 -> 0x030042B0 (arg0 = E+0xC94; HC5.md, 0x0804FC7C); grava u16 de E+0xC6E/E+0xC70 em 0x0300383C/0x03003840 e 0xFFFF em 0x030055A0+(arg0+0x4F)*4; termina na mesma rodada.
- Confiança: confirmado (handler); chute (o que a cópia representa). PC: **falta**.

## 0x80 Begin_Batched_Object_Command_Block (Toolkit: sem argumentos) — nome Ghidra: Event_Op80_Begin_Batched_Object_Command_Block
- Zera 0x030064F2 (0x030064A0+0x52, contador) e limpa 0x030064A0 +2 (8), +0xA (8), +0x12 (0x40 = 8 registros de 8),
  +0x52, +0x53, +0x54 (8) com `FUN_08049714` (memset 0). Depois lê o PRÓXIMO opcode (E+0xC92), zera E+0xC89 e E+0xC93 e
  o despacha na hora pela tabela 0x083D222C (`CallViaR0`). 0 bytes próprios; encadeia o opcode seguinte no mesmo quadro.
- Confiança: confirmado. PC: **falta**.

## 0x81 Queue_Batched_Object_Command (Toolkit: record_type + object id + arg4..arg8)
- Lê 8 bytes; copia para 0x030064A0+0x12 + 8*contador e incrementa o contador (+0x52). Depois encadeia o próximo
  opcode igual ao 0x80 (lê, zera pendente/passo, despacha). Confiança: confirmado. PC: **falta**.

## 0x82 Execute_Batched_Object_Command_Block (sem argumentos)
- Handler: só E+0xC89 = 1, 0 bytes. Continuação 0x0804FD10: percorre os registros da fila (tipo 1..4), resolve o objeto
  (`FUN_0805076C`, `Event_GetTrackedActorSlot`), inicia o movimento (`FUN_080500BC`), espera (`FUN_0805018C`) e no fim limpa
  a fila (memset 0x40). Confiança: confirmado (handler); provável (continuação). PC: **falta**.

## 0x83 Set_Object_Render_Mode (Toolkit: target_flags, render_mode)
- Lê 2 bytes (f = arg0, m = arg1). Alvo: f&0x40 -> objeto do jogador (0x2F); f&0x20 -> NPC do mapa (registro E+0xCB4*8,
  se id&0xFFF em +0x5DE == E+0xC8E, objeto = (+0x5E0 & 0x1F) + 0x31); senão ator rastreado f&0xF.
- f&0x10 == 0: grava m nos bits de atributo (`Map_SetObjectAttrBits10`, ou `Map_SetTrackedObjectTileAttr` no ator).
  f&0x10 != 0: m == 0 -> `Map_ResetObjectField0B` (volta o +0xB ao padrão pela tabela do tipo: 0xFF/0/0x40);
  m != 0 -> grava m no +0xB (`Map_SetObjectSlotField0B` / `Map_SetTrackedObjectField0B`). Sem continuação.
- Confiança: confirmado. PC: **falta**.

## 0x84 Set_Map_Scene_Variant_When_Player_Faces_Up (Toolkit: variant)
- Handler: lê 1 byte e liga o pendente (nada mais). Continuação 0x0804FE6C: testa E+0xC36 e, conforme o valor, copia
  arg0 (E+0xC94) para o campo de variante; depois termina. A leitura "olha para cima" vem do Toolkit.
- Confiança: confirmado (1 byte); provável (condição). PC: **falta**.

## 0x85 Begin_Event_Object_Facing_Cycle_Block (sem argumentos) — nome Ghidra: Event_Op85_Begin_Event_Object_Facing_Cycle_Block
- Zera 0x030064F2 (contador) e limpa 0x030064A0 +2 (8), +0xA (8), +0x12 (0x40) com `FUN_08049714` (3 chamadas, não 4).
  Não encadeia o próximo opcode (diferente de 0x80). Confiança: confirmado. PC: **falta**.

## 0x86 Queue_Event_Object_Facing_Cycle (Toolkit: cycle_type, packed_object_id, arg4..arg8)
- Lê 8 bytes e copia para 0x030064A0+0x12 + 8*contador (contador++). Não encadeia. Confiança: confirmado. PC: **falta**.

## 0x87 Execute_Event_Object_Facing_Cycle_Block (sem argumentos)
- Handler: só E+0xC89 = 1, 0 bytes. Continuação 0x0804FE9C usa a fila de 0x030064A0 e 0x03002C30 (não lida em detalhe).
- Confiança: confirmado (handler). PC: **falta**.

## 0x88 (sem definição no Toolkit) — nome Ghidra: Event_Op88_Jump_If_Flag_C76_Clear
- Salva o cursor e lê 2 bytes; se o byte E+0xC76 (0x03006456) == 0, cursor = salvo + (arg0<<8 | arg1) (Short big-endian,
  mesmo formato do 0x30 Relative_Long_Jump). Sem continuação. Significado de E+0xC76: desconhecido.
- Confiança: confirmado. PC: **falta** (o extrator para em 0x88: opcode desconhecido; se for incluído, é salto de 16 bits).

## Resumo para o port
- Com argumento: 0x69 (2), 0x6B (1), 0x6C (5), 0x6D (2), 0x6E (5), 0x6F (2), 0x78 (2), 0x79 (1), 0x7B (3), 0x7D (1),
  0x7E (1), 0x7F (1), 0x81 (8), 0x83 (2), 0x84 (1), 0x86 (8), 0x88 (2).
- Sem argumento: 0x6A, 0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x7A, 0x7C, 0x80, 0x82, 0x85, 0x87.
- Saltos: 0x78 (1 byte, se < 3), 0x7B (1 byte, se sem peça), 0x88 (Short BE, se E+0xC76 == 0).
- 0x80 e 0x81 executam o opcode seguinte no mesmo quadro (para um leitor linear o tamanho continua 1 e 9).
