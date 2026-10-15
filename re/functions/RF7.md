# RF7: revisão dos 34 itens de RF7_lista.txt

Só trocas e achados. Detalhes por endereço em `RF7_simbolos.csv`.

## Renomeados no Ghidra (2)

- `0x08053C00`: `Viewport_ClampAxisA` -> `Viewport_WorldToScreenWrapA` (antes: Viewport_ClampAxisA), provavel.
  Não é clamp: é wrapper. Único BL (0x08053C06) vai para 0x08053D78 (Viewport_WorldToScreenXY).
- `0x08053C14`: `Viewport_ClampAxisB` -> `Viewport_CheckBoundsWrapB` (antes: Viewport_ClampAxisB), provavel.
  BLs: 0x08053C22 -> 0x08053C7C (helper), 0x08053C2A -> 0x08053DE0 (Viewport_CheckBoundsSetState).

## Achados

- Os quatro FUNCAO (0x08053380, 0x08053BE0, 0x08053C00, 0x08053C14) são funções de verdade: começam com
  `push {lr}`. O Ghidra mostra corpo de 4 bytes (halt_baddata), então não dá para decompilar. Por isso os
  nomes que não foram trocados continuam `chute`.
- `0x08053C7C` (não está na lista): função com prólogo `f0 b5`, chamada por 0x08053BEE, 0x08053C22 e pelo
  ComputeOffsetA. Candidata a item próprio de RF8 ou de um próximo RF7; não foi catalogada aqui.
- `0x08054280` (Scene_DispatchStateHandlerTail): o item estava como `dado`, mas é chamado por BL de 0x08056818 e
  0x080568C0. Tratado como `func_thumb` (provavel), mesmo sem prólogo B5. Ghidra agora o lista como função.
- Tabelas confirmadas (N x stride, 4 B por ponteiro):
  - `0x08050CF4` SceneCommandStepTable: 5 entradas (passos 0..4).
  - `0x08050E6C`: 33 entradas (comando 0..32), sítio em 0x08050E4E.
  - `0x080542B0`: 0x8C = 140 entradas (estado de cena), sítio em 0x080542A2.
- Os itens `SceneCommandStepN`, `SceneState_xx` e `Event_Op*` que são internos a função ficaram como `label`
  `provavel`, sem gastar mais decompile. Os nomes do que cada alvo faz continuam `chute` onde a doc marca.
- Nomes de opcode (Event_Op4C, Op4D, Op81) foram mantidos conforme a regra do AGENTE.md. Há divergência de
  numeração entre o mtbe.csv (0x4B/0x4C) e IN/QA2; não resolvi.
- `lit_EventDbBase3` (0x0804B124): pool de literais, não tabela. O primeiro valor é 0x08765EC0 (base do banco
  de eventos, provável).

## Divergências com nomes antigos (para revisão)

- Os nomes `Viewport_ClampAxisA/B` tinham sido dados por suposição; a leitura dos BLs mostra wrappers.
- `Overworld_CheckMovementObjectBlocker` (0x080525D0) começa com prefixo de BL e não tem prólogo; a doc (MP2)
  diz que é entrada por ponteiro. O tipo (função x label) ficou em aberto.

## Em aberto

- Corpo real de Unk_08053380, Viewport_ComputeOffsetA e dos dois wrappers: precisa de disassembly manual ou de
  Ghidra com o corpo refeito (o corpo atual tem 4 bytes).
- Nomes do que fazem os estados SceneState_02..0F (todos `chute` pela doc SC4).
