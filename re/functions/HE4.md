# HE4: handlers de opcode de evento (0x69..0x88)

Tabela: `0x083D222C + 4*op` (ponteiros com bit Thumb; o endereço da seção é ponteiro-1, igual a HE4_lista.txt).
Helpers comuns (lidos no Ghidra): `Script_ReadBytes(n)` 0x0804D8CC (lê n bytes do script para o buffer de argumentos
0x03006474); `site_GetEventScript` 0x0804D83C (lê 1 byte, avança o cursor). A contagem de bytes de cada handler é o
primeiro `movs r0,#N; bl 0x0804D8CC` (ou 1 chamada a 0x0804D83C). Padrão da maioria: marca um pedido pendente em
0x03006469 (= 0x030057E0+0xC89 = 1) e o executor de cena roda depois (confirmado só pelo padrão; executor não lido).
Nomes Toolkit: `src/Medabots.Rom/Events/EventOpcodeDefinitions.cs`. PC: `pc/extract/extract_events.py` e `pc/game/src/script.lua`.
Não há função Ghidra nos endereços 0x0804D158..0x0804D7F8 exceto os marcados (FUN_ ou nome). O resto foi lido por
disassembly do ROM (capstone), não pelo decompilador: confiança "provável" nessas seções.

## 0x69 Set_Map_Scene_Variant (Toolkit: variant, skip_full_reload)
- Lê 2 bytes. Compara a variante nova com a atual (0x030057E0+0xC94) e, se iguais, sai; se diferentes, grava a
  variante (+0xC95?) e chama `FUN_08052BEC` / rotina de recarga de cena (0x0804D1F2 = saída).
- Memória: 0x03006474 (argumentos); endereço da variante não fechado.
- Confiança: provável (fluxo lido, nomes de campo não confirmados). PC: **falta**.

## 0x6A (sem definição no Toolkit)
- Só `bx lr`: 0 bytes, nada faz. Reservado. PC: **falta** (não é necessário; é no-op).

## 0x6B Set_Current_Chapter (Toolkit: chapter)
- Lê 1 byte (`bl 0x0804D83C`) e grava em 0x03005FDF (= 0x030057E0+0x7FF). Confirmado pelo código.
- PC: **implementado** (`extract_events.py` decodifica 0x6B; `script.lua` `chapter(value)`).

## 0x6C Initiate_Moving_Actor_C (Toolkit: packed_actor_id, sprite, x, y, move)
- Lê 5 bytes para 0x03006474. Usa o byte 0 como slot (0x0804DBD0 guarda valor+1 numa tabela por slot) e grava
  posição/sprite no bloco de atores (0x030064A0 + offset). Liga o pedido pendente.
- Memória: 0x03006474..+5; tabela de atores (DBD0/DBF0); 0x03006D20+0xA (escrita de ponteiro/flag de movimento).
- Confiança: provável. PC: **falta** (o extrator decodifica só 0x1F/0x21, Initiate_Actor).

## 0x6D Move_Actor_D (Toolkit: tracked_object_slot, move)
- Lê 2 bytes; slot = byte 0; grava 1 byte de Move em 0x03006D20+0xA e liga o pedido pendente.
- Confiança: provável. PC: **parcial** (`extract_events.py` trata 0x6D como `actor_move` com 1 byte de Move; sem efeito no `script.lua`).

## 0x6E Initiate_Moving_Actor_D (Toolkit: packed_actor_id, sprite, x, y, move)
- Mesmo corpo de 0x6C (5 bytes, tabela de atores, 0x0804DBD0). Confiança: provável. PC: **falta**.

## 0x6F Move_Actor_E (Toolkit: tracked_object_slot, move)
- Mesmo corpo de 0x6D (2 bytes). Confiança: provável. PC: **falta** (extrator só cobre 0x6D).

## 0x70, 0x71, 0x72, 0x73, 0x76 (sem argumentos; Toolkit: Run_Scene_Command_13/00/02/08, Start_Medabot_Link)
- Só marcam o pedido pendente (0x03006469 = 1) e saem. Nenhum argumento. Cada um chama um comando diferente do executor
  de cena (ainda não separado). Confiança: provável (o corpo é idêntico em 0x70/71/72/73/76).
- PC: **falta** (os cinco).

## 0x74 Reserved_NoOp, 0x75 (sem definição), 0x77 (sem definição), 0x7C Reserved_NoOp
- Só `bx lr`: 0 bytes, nada faz. PC: **falta**, não necessário (no-op).

## 0x78 Run_Scene_Command_10_If_At_Least_Three_Complete_Part_Sets (Toolkit: scene_arg, jump_if_fewer...)
- Lê 2 bytes. Copia 2 bytes de RAM (0x0C8A / 0x0C8C) para o registro de estado; lê o byte 0 para 0x03006D38.
  Chama `FUN_08047F9C` e compara com 2: se maior, grava 1 no pedido pendente; senão segue e pode saltar pelo 2º byte.
- Confiança: provável. PC: **parcial** (`extract_events.py` trata 0x78 como salto relativo no último byte, `BRANCH_OPS`; a
  condição "3 conjuntos completos" não está no `script.lua`).

## 0x79 Run_Scene_Command_12 (Toolkit: scene_arg)
- Lê 1 byte para 0x03006D38+1 e marca o pedido pendente. Confiança: provável. PC: **falta**.

## 0x7A Run_Scene_Command_11
- Só marca o pedido pendente, 0 bytes. Confiança: provável. PC: **falta**.

## 0x7B Jump_If_No_Spare_Part (Toolkit: bot, part, jump) — nome Ghidra: Event_Op7B_Jump_If_No_Spare_Part
- Lê 3 bytes. Chama `FUN_08047D9C(bot, part)` (teste de peça sobressalente); se 0 (sem peça), salta com o 3º byte
  (rel. 16 bits em 0x0C96 + 1) ao cursor. Confirmado pelo código (cmp r0,#0 / bne).
- PC: **parcial** (`extract_events.py` trata 0x7B como salto em `BRANCH_OPS`; o teste "sem peça sobressalente" não é
  feito em `script.lua`; ver o item 0x38/0x39 Get_Part no `extract_events.py`).

## 0x7D (sem definição no Toolkit)
- Lê 1 byte e grava em 0x03006412 (= 0x030057E0+0xC32). Confiança: chute. PC: **falta**.

## 0x7E Move_All_Tracked_Actors_C (Toolkit: move_pattern)
- Lê 1 byte e grava no slot 0x0C94 e marca o pedido pendente. Confiança: provável. PC: **falta**.

## 0x7F (sem definição no Toolkit)
- Lê 1 byte para 0x0C94 e marca o pedido pendente. Confiança: provável (mesma forma de 0x7E). PC: **falta**.

## 0x80 Begin_Batched_Object_Command_Block (Toolkit: sem argumentos) — nome Ghidra: Event_Op80_Begin_Batched_Object_Command_Block
- Zera o byte 0x030064F2 (0x030064A0+0x52, contador do bloco) e limpa áreas de 0x030064A0 (8, 8, 0x40, 1, 1, 8 bytes) com
  `FUN_08049714` (memset). Depois chama o despachante de registros pelo byte 0x0C92 (tabela 0x083D222C) via 0x0807CCF0.
- Confiança: provável (nome do Toolkit + limpeza do bloco). PC: **falta**.

## 0x81 Queue_Batched_Object_Command (Toolkit: record_type + object id + arg4..arg8)
- Lê 8 bytes para 0x03006474. Incrementa o contador do bloco (0x030064A0+0x52), copia os bytes para a fila em
  0x030064A0+offset via `FUN_08049714`/0x0804D83C. Confiança: provável. PC: **falta**.

## 0x82 Execute_Batched_Object_Command_Block (sem argumentos)
- Só grava 1 byte de estado (`strb r1,[r0]`). Confiança: chute no detalhe (executa o bloco do 0x80/0x81). PC: **falta**.

## 0x83 Set_Object_Render_Mode (Toolkit: target_flags, render_mode)
- Lê 2 bytes; chama `FUN_080487C8` e `FUN_080507BC` (aplicam flags e modo ao objeto). Confiança: provável. PC: **falta**.

## 0x84 Set_Map_Scene_Variant_When_Player_Faces_Up (Toolkit: variant)
- Lê 1 byte e grava a variante em RAM (endereço não fechado; a condição 'jogador olha para cima' não foi lida). Confiança: provável. PC: **falta**.

## 0x85 Begin_Event_Object_Facing_Cycle_Block (sem argumentos) — nome Ghidra: Event_Op85_Begin_Event_Object_Facing_Cycle_Block
- Zera o byte 0x030064F2 (contador) e limpa blocos com `FUN_08049714` (4 chamadas). Confiança: provável.
  PC: **falta**.

## 0x86 Queue_Event_Object_Facing_Cycle (Toolkit: cycle_type, packed_object_id, arg4..arg8)
- Lê 8 bytes para 0x03006474; copia para a fila do ciclo de direção (0x030064A0 +...), usa `FUN_080497C0` (memcpy).
  Confiança: provável. PC: **falta**.

## 0x87 Execute_Event_Object_Facing_Cycle_Block (sem argumentos)
- Grava 1 byte de estado; executa o ciclo de direção enfileirado (provável). Confiança: chute no detalhe. PC: **falta**.

## 0x88 (sem definição no Toolkit)
- Lê 2 bytes (`movs r0,#2; bl 0x0804D8CC`) e grava 16 bits em [r6] (`strh`). Sem nome. Confiança: chute. PC: **falta**.

## Resumo para o port
- Com argumento: 0x69 (2), 0x6B (1), 0x6C (5), 0x6D (2), 0x6E (5), 0x6F (2), 0x78 (2), 0x79 (1), 0x7B (3), 0x7D (1),
  0x7E (1), 0x7F (1), 0x81 (8), 0x83 (2), 0x84 (1), 0x86 (8), 0x88 (2).
- Sem argumento: 0x6A, 0x70, 0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x7A, 0x7C, 0x80, 0x82, 0x85, 0x87.
