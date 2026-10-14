# HC5: continuações dos opcodes de evento 0x66..0x7F (tabela 0x083D2630 + 4*op)

Cada seção é chamada por `Event_DispatchContinuation` (0x0804DC0C) enquanto E+0xC89 != 0. "Zerar o pendente" = chamar
`Event_EndOpcode` (0x08050290), que zera E+0xC89 e E+0xC93. Base de evento E = 0x030057E0. Ver HE3/HE4 e HC.md.

## 0x0804F83C Event_Op66_Cont_Animate_Tracked_Object_Transition
- Chama `Map_TickTrackedObjectTransition` (mtbe; Ghidra: Map_ObjectMoveStateStep) 0x080509EC a cada quadro. Quando devolve != 0, chama `Event_EndOpcode`.
- Usado por: opcode 0x66 (setup em 0x0804D094 grava o slot em 0x03006D2A e liga o pendente).
- Lê da 1ª fase: o slot do objeto rastreado (0x03006D2A). O 2º byte do argumento não aparece aqui.
- Confiança: confirmado (disassembly/decompile). Efeito do tick (arco vertical e modo de render): provável (já anotado no mtbe).

## 0x0804F850 Event_Op67_Cont_Skip_Two_Bytes_Wait
- Só chama `Event_EndOpcode`. Na prática, o opcode espera 1 quadro e não usa os 2 bytes consumidos no setup.
- Usado por: opcode 0x67 (setup em 0x0804D0E0 lê 2 bytes e liga o pendente).
- Lê da 1ª fase: nada.
- Confiança: confirmado (decompile de 0x0804F850: 1 chamada e retorno).

## 0x0804F860 Event_Op6A_Cont_NoOp
- Só `bx lr` (função de 2 bytes no Ghidra). Não faz nada.
- Usado por: opcode 0x6A, que é no-op (0 bytes de argumento; o handler também só retorna). Este pendente nunca é ligado, então a continuação não roda na prática.
- Lê da 1ª fase: nada.
- Confiança: confirmado (decompile e tamanho da função). Nome do mtbe (NoOp_0804F860) mantido no CSV.

## 0x0804F864 Event_Op6C_Cont_Initiate_Moving_Actor_C
- Passo 0 (E+0xC93 == 0): `FUN_08050358(arg0..arg3)` cria o objeto (id/sprite/x/y, conforme HE4), `FUN_080500BC(obj, arg4, 1)` inicia o Move; passo vai a 1.
- Passo 1: espera `Map_TickObjectStep` 0x0805018C(obj, 1) devolver != 0 (fim do movimento), então `Event_EndOpcode`.
- Usado por: opcode 0x6C (setup em HE4: grava o índice do objeto em 0x03006D20+0xA e liga o pendente). O 0x6E tem continuação própria (0x0804F940), não esta.
- Lê da 1ª fase: 5 bytes de argumento em E+0xC94.. (arg0 id, arg1 sprite, arg2 x, arg3 y, arg4 move) e o índice do objeto em 0x03006D20+0xA.
- Confiança: confirmado (fluxo de passos); provável (papel de 08050358 e 080500BC). Literais não resolvidos nesta passagem.

## 0x0804FA5C Event_Op71_Cont_Run_Scene_Command_00
- Passo 0: `FUN_08050CC0()` zera o passo da cena (0x03006D20+0xB) e avança. Passo 1: `FUN_08050CCC(0x00, 0)` roda a máquina de passos da cena até devolver != 0, então `Event_EndOpcode`.
- Usado por: opcode 0x71 (sem argumentos; setup só liga o pendente).
- Lê da 1ª fase: nada (o comando é fixo 0x00).
- Confiança: confirmado (fluxo e comando). Efeito do comando 0x00: provável (não lido aqui).

## 0x0804FA9C Event_Op72_Cont_Run_Scene_Command_02
- Mesmo corpo que 0x71, com `FUN_08050CCC(0x02, 0)`.
- Usado por: opcode 0x72 (sem argumentos).
- Lê da 1ª fase: nada.
- Confiança: confirmado (fluxo e comando). Efeito do comando 0x02: provável (não lido aqui).

## 0x0804FADC Event_Op73_Cont_Run_Scene_Command_08
- Mesmo corpo de 0x71/0x72: passo 0 `FUN_08050CC0` (Event_ResetSceneCommandStep); passo 1 `FUN_08050CCC(0x08, 0)` (Event_RunSceneCommandSequence) até != 0, então `Event_EndOpcode`.
- Usado por: opcode 0x73 (sem argumentos).
- Lê da 1ª fase: nada.
- Confiança: confirmado (fluxo e comando). Efeito do comando 0x08: provável (não lido aqui).

## 0x0804FB1C Event_Op76_Cont_Start_Flow4_Then_Scene_Command_05
- Máquina de 4 passos no byte de passo E+0xC93 (0x030057E0 + 0xC93):
  - Passo 0: `GameState_StartFlowByCommand(4, 0)` (0x08047878); passo vai a 1.
  - Passo 1: NÃO espera. Checa uma vez `0x03006D20+0xC == 7`. Se for 7, passo vai a 2; se não, `Event_EndOpcode` (o opcode termina sem rodar o comando 5).
  - Passo 2: `Event_ResetSceneCommandStep` (0x08050CC0), passo vai a 3.
  - Passo 3: `Event_RunSceneCommandSequence(0x05, 0)` (0x08050CCC) até != 0, então `Event_EndOpcode`.
- Usado por: opcode 0x76 (sem argumentos).
- Lê da 1ª fase: nada. Lê o estado do flow em 0x03006D20+0xC.
- Confiança: confirmado (decompile e literais). Divergência: HE4 dizia "espera ... == 7"; a leitura mostra que o teste é de um quadro só (se não for 7, o opcode acaba). Efeito do fluxo 4 e do comando 5: provável.

## 0x0804FC1C Event_Op7A_Cont_Run_Scene_Command_11
- Passo 0: `Event_ResetSceneCommandStep` (0x08050CC0), passo vai a 1. Passo 1: `Event_RunSceneCommandSequence(0x11, 0)` (0x08050CCC) até != 0; então chama `FUN_0804B738()` e, se der != 0, grava 1 em 0x03005349 (literal 0x03005310 + 0x39). Depois `Event_EndOpcode`.
- Usado por: opcode 0x7A (sem argumentos).
- Lê da 1ª fase: nada.
- Confiança: confirmado (decompile e literal da ROM). Divergência: HE4 dizia "zera 0x03005310"; o código grava 1 no byte +0x39 desse bloco. Efeito de 0x0804B738 e do comando 0x11: provável.

## 0x0804FC6C Event_Op7E_Cont_Move_All_Tracked_Actors_C
- Só chama `Event_RunAllTrackedActorsStepSequence(0, 0)` (0x0804E5F0). Essa rotina percorre os 4 slots de ator rastreado, anda cada um pelo padrão de passos e termina o opcode quando todos acabam (mtbe).
- Usado por: opcode 0x7E (setup em HE4 lê 1 byte para E+0xC94 e liga o pendente). O padrão de movimento fica nesse buffer; a continuação não recebe o argumento por parâmetro.
- Lê da 1ª fase: o byte do padrão em E+0xC94.
- Confiança: confirmado (chamada); provável (semântica de 0x0804E5F0, vinda do mtbe).

## 0x0804FC7C Event_Op7F_Cont_Copy_Tracked_Actor_Records
- Roda numa rodada só (sem espera). Usa arg0 (E+0xC94, índice do ator rastreado) e as constantes da ROM: base 0x03002C30 (gObjectSlots).
  - Grava u16 de E+0xC6E em 0x0300383C (bytes 1..2; byte 0 preservado) e u16 de E+0xC70 em 0x03003840.
  - Copia 0x40 bytes de 0x03003FF0 + arg0*0x40 para 0x03003830 (`mem_copy` 0x080497C0).
  - Grava 0xFFFF (short) em 0x030055A0 + (arg0+0x4F)*4.
  - Copia 0x40 bytes de 0x03004A70 + arg0*0x40 para 0x030042B0.
  - Termina com `Event_EndOpcode`.
- Usado por: opcode 0x7F (setup em HE4 lê 1 byte para E+0xC94 e liga o pendente).
- Lê da 1ª fase: arg0 (E+0xC94).
- Confiança: confirmado (leitura dos literais na ROM). Significado dos blocos (slots de registro de objeto 0x4F+i e 0x79+i copiados para os slots de trabalho 0x30 e 0x5A): chute. Divergência com HE4: HE4 citava E+0xC6E e E+0xC0C como fontes; o código lê E+0xC6E/E+0xC70 e usa 0xC0C só como deslocamento do destino (0x0300383C).
