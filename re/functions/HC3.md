# HC3: continuações dos opcodes de evento (fase 2), tabela 0x083D2630

Fonte: decompile no Ghidra (`decompile_function_by_address`) das rotinas de `HC3_lista.txt`. Literais do pool lidos
na ROM EU: E = 0x030057E0; `E+0xC93` = passo da continuação; `E+0xC8C` = cursor salvo; `E+0xC8A` = cursor do script;
`E+0xC94` = argumento (salto). Ao terminar, a continuação chama `Event_EndOpcode` 0x08050290 (zera E+0xC89 e E+0xC93).

## 0x0804E7E4 Event_Op30_Cont_NoOp
- O que faz: `return` vazio (bx lr). Nada é desenhado, movido ou esperado; o opcode 0x30 (salto relativo longo)
  nunca liga o pendente, então esta rotina não roda para ele.
- Usada por: 0x30, 0x2E, 0x2F no mesmo padrão de no-op (HE2).
- Lê da 1ª fase: nada.
- Confiança: confirmado (decompile de uma linha).

## 0x0804E7E8 Event_Op31_Cont_YesOrNoBox
- O que faz: máquina de 3 passos sobre E+0xC93. Passo 0: `FUN_08048000(0)` abre a caixa sim/não com cursor inicial 0
  e avança. Passo 1: consulta `FUN_08048038()` até devolver != 0; se o nibble alto do resultado != 0, grava
  cursor = E+0xC8C + E+0xC94 (salto). Avança. Passo 2: espera `StateMachine_DispatchSub` (0x08041CA4) != 0 (caixa
  fechando) e chama `Event_EndOpcode`.
- Usada por: 0x31 (Yes_or_No_Box). Lê da 1ª fase: E+0xC94 (1 byte de salto). E+0xCB5 NÃO é lido aqui (HE2 dizia que
  era a ordem; a ordem vem do literal 0 da chamada).
- Confiança: confirmado (fluxo); provável (nibble alto = resposta que pula; a escolha de qual lado pula é palpite).

## 0x0804E860 Event_Op32_Cont_NoOrYesBox
- O que faz: 2 passos sobre E+0xC93. Passo 0: `FUN_08048000(1)` abre a caixa com cursor inicial 1 (ordem invertida)
  e avança. Passo 1: espera `FUN_08048038()` != 0; se nibble alto != 0 grava cursor = E+0xC8C + E+0xC94; depois
  `Event_EndOpcode` no mesmo quadro (não há passo de espera extra, diferente de 0x31).
- Usada por: 0x32 (No_or_Yes_Box). Lê da 1ª fase: E+0xC94 (salto). E+0xCB5 (=1 no setup) não é lido aqui.
- Confiança: confirmado (fluxo); provável (papel de FUN_08048000/08048038 = caixa de escolha; ver R3 0x08048000/0x08048038,
  ainda sem nome).

## 0x0804E928 Event_Op36_Cont_GetItem
- O que faz: 2 passos sobre E+0xC93. Passo 0: `FUN_08047c00()`, lê item (E+0xC94) e conta no inventário
  (`Inventory_GetItemCount`). Se já são 99: toca sfx 6, mostra mensagem "cheio" (nome do item em `Text_CopyStringToSlot`) e
  passa a 1. Senão: sfx 0x0E, quantidade = E+0xC95 limitada a 99 - contagem, soma no inventário (`FUN_08047dc0`), mostra
  nome e quantidade (`Text_WriteDecimalToSlot`) e passa a 2. Passo 1/2: espera a caixa (`FUN_08047c24`) e termina.
- Usada por: 0x36 (Get_Item). Lê da 1ª fase: E+0xC94 (item), E+0xC95 (qtd, já com 0 -> 1).
- Confiança: confirmado (fluxo, literais E+0xC93/C94/C95 conferidos na ROM); provável (nome `Inventory_*` e textos).

## 0x0804F0A4 Event_Op41_Cont_FlickerAllActors
- O que faz: a cada quadro percorre os slots lógicos 0..3 com ator rastreado. Conta os que já têm o halfword
  `0x030055A0[render*4]` == 0 (pisca acabou) e soma `FUN_0805082c(render)` (provável: pisca ainda ativo) para os demais.
  Termina (`Event_EndOpcode`) quando todos os 4 slots estão zerados, ou quando a soma é 0 (ver risco abaixo).
- Usada por: 0x41 (Flicker_All_Tracked_Actors). Lê da 1ª fase: nada direto; o estado é o `gObjectSlots[render]` gravado
  pelo primário (+0x26..+0x29).
- Risco (provável, não testado): se nenhum slot lógico estiver rastreado, `iVar7` e `iVar6` ficam 0 e a rotina não termina.
  Conferir se o primário garante slot ativo.
- Confiança: confirmado (fluxo); provável (papel de `FUN_0805082c`).

## 0x0804F5C8 Event_Op51_Cont_NoOp
- O que faz: `return` vazio (bx lr). Nada é feito; o opcode 0x51 não liga o pendente. A entrada 0x0804F5C8 é um
  dos stubs NoOp de 0x51..0x55.
- Usada por: 0x51 (mtbe: NoOp_0804F5C8). Lê da 1ª fase: nada.
- Confiança: confirmado (decompile de uma linha; HE3 diz que 0x51..0x55 não gravam pendente).

## 0x0804F5CC Event_Op52_Cont_NoOp
- O que faz: `return` vazio (bx lr). Nada é feito; o opcode 0x52 não liga o pendente.
- Usada por: 0x52 (mtbe: NoOp_0804F5CC). Lê da 1ª fase: nada.
- Confiança: confirmado (decompile de uma linha).

## 0x0804F5D0 Event_Op53_Cont_NoOp
- O que faz: `return` vazio (bx lr). Nada é feito; o opcode 0x53 não liga o pendente. (C# chama 0x53 de "troca sprite
  de slot", mas o primário e a continuação não fazem isso; conferir no primário.)
- Usada por: 0x53 (mtbe: NoOp_0804F5D0). Lê da 1ª fase: nada.
- Confiança: confirmado (decompile de uma linha).

## 0x0804F5D4 Event_Op54_Cont_NoOp
- O que faz: `return` vazio (bx lr). Nada é feito; o opcode 0x54 não liga o pendente.
- Usada por: 0x54 (mtbe: NoOp_0804F5D4). Lê da 1ª fase: nada.
- Confiança: confirmado (decompile de uma linha).

## 0x0804F5D8 Event_Op55_Cont_NoOp
- O que faz: `return` vazio (bx lr). Nada é feito; o opcode 0x55 não liga o pendente.
- Usada por: 0x55 (mtbe: NoOp_0804F5D8). Lê da 1ª fase: nada.
- Confiança: confirmado (decompile de uma linha).

## 0x0804F5DC Event_Op56_Cont_LoadEntities
- O que faz: sem passo; roda tudo num quadro e termina. Chama `FUN_080505FC` (monta o snapshot de NPCs do encontro, nome
  do HE3), zera 0x900 bytes em 0x03003870 e 0x03004370 e 0x90 em 0x03005664 e 0x0300570C (`FUN_08049714` = zerar), chama
  `FUN_080506BC` (reconstrói os slots de render ativos), zera o buffer de colisão 0x03005FF0 (0x400) e copia nele a colisão
  do mapa atual (tabela 0x08404334 indexada por mapa; (w/2)*(h/2) bytes, `FUN_080497C0`), grava 1 em 0x0300649A,
  `FUN_080513C8` (carrega spawns do mapa), `Map_UpdateVisibleEventObjectSprites` e `Event_EndOpcode`.
- Usada por: 0x56 (Load_Entities). Lê da 1ª fase: nada (o primário só grava o pendente).
- Confiança: confirmado (decompile, tamanhos e ordem); provável (nomes de FUN_080505FC, FUN_080513C8, FUN_080506BC, vindos do HE3).
