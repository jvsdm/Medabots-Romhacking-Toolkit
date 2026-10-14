# HC2: continuacoes de opcodes de evento (segunda fase, tabela 0x083D2630)

Chamadas por `Event_DispatchContinuation` (0x0804DC0C) enquanto E+0xC89 != 0. E = 0x030057E0.
Estado da 1a fase: E+0xC89 pendente, E+0xC93 subestado do opcode, E+0xC94.. argumentos.
Fim da continuacao = chamar `Event_EndOpcode` (0x08050290), que zera E+0xC89 e E+0xC93.
Render do objeto de evento = 0x03006D20[10] (0x03006D2A), preenchido pela 1a fase.

## 0x0804E464 Event_Op1E_Cont_FlickerNpc
- O que faz: chama `Map_TickObjectFlicker(render)` (0x0805082C, nome do mtbe, lido no codigo). Enquanto 0x03006D20[0] (frames) > 0, alterna a visibilidade do objeto a cada 8 quadros (contador em [2], recarga em [3]) e decrementa frames. Quando frames chega a 0, restaura o objeto (+0x2D = 0, copia o valor salvo em +0x32) e devolve 1; so entao chama `Event_EndOpcode`.
- Usado por: 0x1E (Flicker_Event_NPC). Args da 1a fase: 1 byte (frames) para 0x03006474; grava 0x03006D20 = [frames, 0, 8, 8], render em [10].
- Confianca: confirmado (corpo e fim lidos; base do objeto (+0x40 por slot, provavel gObjectSlots 0x03002C30) nao conferida).

## 0x0804E480 Event_Op1F_Cont_InitiateMovingActorA
- O que faz: chama `Event_RunInitiateMovingActorSequence` (0x0804E498), que faz: fase E+0xC93 == 0 -> `Map_SpawnTrackedObjectAtTile(E+0xC94 id, E+0xC95 sprite, E+0xC96 x, E+0xC97 y)` (0x08050358), depois `FUN_080500bc(render, E+0xC98 move, 0)` (inicia o passo direcional do objeto) e vai para fase 1. Fase 1 -> espera `FUN_0805018c(render, 1)` != 0 e chama `Event_EndOpcode`.
- Usado por: 0x1F (e 0x20, mesma continuacao). Args da 1a fase: 5 bytes (id, sprite, x, y, move) em E+0xC94..C98; slot do tracked object via Map_SetTrackedObjectSlotByIndex.
- Confianca: confirmado (corpo e literais lidos). FUN_080500bc e FUN_0805018c nao lidos: "iniciar passo" e "esperar passo" sao provaveis.

## 0x0804E48C Event_Op20_Cont_InitiateMovingActorB
- O que faz: chama o mesmo `Event_RunInitiateMovingActorSequence` (0x0804E498). O corpo e identico ao do 0x1F: nao ha diferenca de comportamento entre A e B nesta fase.
- Usado por: 0x20. A 1a fase tambem e identica a do 0x1F (5 bytes, mesmo registro de slot).
- Confianca: confirmado (corpo lido; mesma chamada, sem ramo).

## 0x0804E51C Event_Op23_Cont_MoveActorA
- O que faz: chama `Event_RunActorStepSequence` (0x0804E534), compartilhado com 0x24. Fase E+0xC93 == 0 -> `Map_StartObjectStep(render, E+0xC95 move, 0)` (0x080500BC, nome do mtbe) e vai a fase 1. Fase 1 -> espera `Map_TickObjectStep(render, 1)` (0x0805018C) != 0 e chama `Event_EndOpcode`.
- Usado por: 0x23. Args da 1a fase: 2 bytes (slot em E+0xC94, move em E+0xC95); render gravado em 0x03006D20[10].
- Confianca: confirmado (corpo e literais lidos; nomes dos helpers vem do mtbe/R3, provavel).

## 0x0804E528 Event_Op24_Cont_MoveActorB
- O que faz: chama o mesmo `Event_RunActorStepSequence` (0x0804E534) da 0x23. Corpo identico: nao ha diferenca A/B nesta fase.
- Usado por: 0x24. A 1a fase tambem e identica a da 0x23 (2 bytes).
- Confianca: confirmado (corpo lido; mesma chamada, sem ramo).

## 0x0804E5A8 Event_Op27_Cont_ActorDoAnimation
- O que faz: chama `Map_ObjectAnimStep()` (0x080508B4, lido em R3). Enquanto nao devolver 1, o laco segue: estado 0 escolhe a variante de animacao do objeto render (0x03006D20[10]) e estado 1 faz tick da animacao ate o contador zerar. Quando devolve 1, chama `Event_EndOpcode`.
- Usado por: 0x27. Args da 1a fase: 3 bytes (slot, variante, frames); 1a fase zera 0x03006D20[0] e grava frames em [2] (provavel contador).
- Confianca: confirmado (corpo e fim lidos); detalhe do contador em [2] provavel.

## 0x0804E5BC Event_Op28_Cont_HopActor
- O que faz: chama `Map_ObjectArcStep()` (0x08050918, lido em R3) a cada quadro: anda 16 passos de uma trajetoria de arco (tabela de 16 shorts, altura por passo). Quando devolve 1 (fim dos 16 passos), chama `Event_EndOpcode`.
- Usado por: 0x28 (Hop_Actor). A 1a fase le 1 byte (slot), grava Y do objeto em 0x03006D20+8, zera +0x28 do objeto e toca o som 0x70 (0x08047BA8).
- Confianca: confirmado (corpo e fim lidos); significado da tabela de altura: chute.

## 0x0804E5D0 Event_Op29_Cont_MoveAllTrackedActorsA
- O que faz: chama `Event_RunAllTrackedActorsStepSequence` (0x0804E5F0) com flag A = 0 e modo 1. Fase 0: para cada um dos 4 slots de ator ativos (`Map_GetTrackedObjectSlotByIndex`, 0x0804DBF0) cuja halfword em 0x030055A0[(render+0x4F)*4] != 0, chama `Map_StartObjectStep(render, E+0xC94 move, 0)` (0x080500BC). Vai a fase 1: para cada slot, soma o resultado de `Map_TickObjectStep(render, 1)` (0x0805018C); slot ativo sem participar conta como "pronto". Encerra com `Event_EndOpcode` se os 4 slots estiverem vazios ou se a soma for != 0.
- Usado por: 0x29 (Move_All_Tracked_Actors_A). Args da 1a fase: 1 byte (move) em E+0xC94; liga E+0xC89.
- Confianca: confirmado (disassembly 0x0804E5F0 lido). Achado: o fim e no PRIMEIRO ator que termina (ou slot nao participante), nao em "todos"; na pratica os atores partem no mesmo quadro e terminam juntos (provavel). Conferir com replay antes de portar.

## 0x0804E5E0 Event_Op2A_Cont_MoveAllTrackedActorsB
- O que faz: igual a 0x29, mas chama `Event_RunAllTrackedActorsStepSequence` com flag B = 1 (3o argumento de `Map_StartObjectStep`). Mesmo laco de 4 slots, mesmo criterio de fim.
- Usado por: 0x2A. Args da 1a fase: 1 byte (move) em E+0xC94.
- Confianca: confirmado (disassembly do 0x0804E5F0; flag vem do literal de chamada). Diferenca A/B = so o flag.

## 0x0804E7DC Event_Op2E_Cont_NoOp
- O que faz: nada. O corpo e so `bx lr` (disassembly lido).
- Usado por: 0x2E (no mtbe, EventOpcode2E_NoOp). O primario 0x2E tambem e no-op (HE2), entao esta continuacao nunca deveria ser chamada.
- Confianca: confirmado.

## 0x0804E7E0 Event_Op2F_Cont_NoOp
- O que faz: nada. Corpo = `bx lr` (disassembly lido).
- Usado por: 0x2F (no mtbe, EventOpcode2F_NoOp). Primario 0x2F tambem no-op (HE2).
- Confianca: confirmado.
