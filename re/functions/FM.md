# FM: menu de campo (abre no mapa) e a bolsa (GH-FM, 2026-10-14, PARCIAL)

Leitura: ROM original `Medabots - Metabee (Europe).gba` (só leitura, python -I; bytes e literais lidos direto), Ghidra
(decompile/xrefs; nada salvo, nada renomeado). Símbolos: `FM_simbolos.csv`. Código sem função no Ghidra: `FM_lista.txt`.
Confiança: **confirmado** (lido no código ou medido), **provável**, **pendente**.

## 1. Como abre (o que já se sabe)

- **Tecla: B**, não START. Medido na ROM em `rec_7` (mapa livre): B abre o menu; START não abre nada ali
  (`pc/research/README.md`, MN1). **confirmado (medido)**.
- Estado: `gMainState` = 1 (mapa) e `gamestate_menu` = 2 (0x03002B8C). Tabela `MainState1` 0x083D2104:
  `[1]` = `EventScript_RunCurrent` 0x0804A220 (VM de eventos do mapa), `[2]` = **0x0805716D**
  (= `Overworld_InitChoiceMenuScreen` 0x0805718C no Ghidra, corpo 0x0805718C..0x080573F3). **confirmado (tabela lida)**.
- Quem vai do mapa (menu 1) para o menu 2 com a tecla B: **pendente**. Não achei o escritor de
  `gamestate_menu`=2 no caminho do mapa (ver §4).

## 2. Mapa: máquina de sub-estado (confirmado)

- `Overworld_UpdateMapSceneStateMachine` (ctx = literal 0x03005310): byte de estado **0x03005311** (`ctx+1`).
  Se estado > 4, só atualiza marcadores/sprites; senão chama a tabela de 5 entradas em **0x08050F80**
  (entradas 0x08050F94, 0x08051078, 0x080510C0, 0x0805134C, 0x08051368; despacho `mov pc,r0`, ponteiros sem bit Thumb).
- Escritores do byte 0x03005311 (Ghidra xrefs): 0x08051026, 0x0805106A, 0x080510A8 (região do estado 0..1),
  0x0805135A, 0x08051370 (estado 3..4), 0x08051118, e fora da função: `Overworld_WalkStepByDirection` 0x08051BD4,
  `Overworld_InitMapScene` 0x0804A7FC, `Overworld_FinishTransitionInitPlayer` 0x0804A910, `new_game_start` 0x0804A50A.
- Leitura de teclas **held** (0x03002B64) dentro do mapa: `Overworld_UpdateMapSceneStateMachine` em 0x0805114C
  (região do estado 2) e `Overworld_ProcessDirectionInput` em 0x08051CD2. **Provável** que o B seja lido em 0x0805114C.
  Não confirmado: a leitura exata de bit 1 (B) ali.

## 3. Sub-estados do menu de escolha (confirmado no despacho, provável no conteúdo)

- Despachante 0x0805716C (Thumb, bytes lidos; **função desde o sync de 2026-10-14 (nome: FieldMenu_DispatchSubstate)**): `push lr`; base = tabela 0x083D5FF4;
  índice = `gamestate_subroutine` (0x0300137C) × 4; chama o handler. Lacuna de Ghidra: 0x0805716C não tem função
  própria (nem o `disassemble`).
- Tabela 0x083D5FF4 (lida; o literal do despachante aponta para ela): **sub 0 = 0x0805718D
  (Overworld_InitChoiceMenuScreen)**; sub 1 = 0x08057441 (`Map_TickChoiceListOpen` 0x08057440, provável); sub 2 = 0x080574D5
  (pendente). A palavra 0x083D5FF0 (= 0x083D5F6C) não é usada por este despachante.
- `Overworld_InitChoiceMenuScreen` 0x0805718C: monta o menu de **4 linhas**. Para k = 0..3 chama
  `Overworld_CheckStepAllowed(k)` 0x08057D80; retorno 1 = linha ATIVA, senão INATIVA (OW2 passo 8, **confirmado**).
  Os 13 BLs com kind imediato a 0x08057D80 são deste menu (OW2). O mapeamento linha → opção pelo texto é **pendente**.
- Conteúdo do menu (medido em MN1/MN2, texto da ROM, não copiado aqui): 4 opções, na ordem
  MEDAMATCH, ITEM, VEÍCULO, SALVAR; dinheiro embaixo. Mapeamento provável: linha 0 = MEDAMATCH, linha 1 = ITEM,
  linha 2 = VEÍCULO, linha 3 = SALVAR (**provável**, pela ordem; não confirmado pelo código).
- Observação: `Overworld_CheckStepAllowed` tem um ramo kind 3 que exige a parte 0x6B (OW2/R3). Se a linha 3 fosse
  SALVAR, o teste não faz sentido para salvar; a linha que depende da parte 0x6B é a candidata a VEÍCULO. **Pendente.**

## 4. Bolsa de key items (ITEM?) e gamestate_menu=4

- `MainState2` tabela 0x08483594 (lida em KI.md): `[4]` = 0x0805CB50 `Dispatch_StateTableFrameCount` (despacha a
  bolsa por `gamestate_subroutine`: `[2]` = `KeyItemBag_InitListScreen` 0x0805CDAC, `[5]` = `KeyItemBag_ListInputStep` 0x0805CFB4).
  **confirmado (tabelas lidas)**.
- Quem grava `gamestate_menu`=4 com `gMainState`=2 (a entrada do campo para a bolsa): **pendente**. Xrefs de
  0x03002B8C (Ghidra, todas as escritas): AgbMain, SoftResetSequence, MainState2 (0x0805BC12, falha de link: menu 10),
  Screen_ResetGfxAndState 0x08044200, MainState_RequestModeTransition (0x08047xxx), FUN_0804765C (Menu_SetStateByArg01,
  0x08047686-0x080476B2, arg 0/1), Menu_ResetSubstateByMode, Parts_* e Battle_SetDefaults*. Nenhuma escrita com valor 4
  no caminho do mapa foi confirmada.
- Hipótese: a linha ITEM do menu de escolha (§3) chama o caminho que grava `gamestate_menu`=4 em MainState2. **Não confirmado.**

## 5. Como volta ao mapa

- Não fechado. Pelo que existe: `Map_ExitStateDispatchFlow` 0x08057B0C e `StartFlowByCommand` (MN.md §2.1) saem do
  mapa para outras cenas; a saída do menu de escolha (sub 3 / sub 4 do mapa) é **pendente**.

## 6. Pendente (próxima rodada)

1. Achar a escrita de `gamestate_menu`=2 (com `gMainState`=1) no caminho do mapa e o teste do B (bit 1 de 0x03002B64 ou
   0x0300058C) em 0x0805114C: decompile/disasm da região 0x08051080..0x08051180 (é o miolo de
   Overworld_UpdateMapSceneStateMachine no Ghidra; os handlers 0x08050F94..0x08051368 não viram função).
2. Decodificar os handlers de sub 1..4 e a linha → opção (texto da escolha: ver `Overworld_InitChoiceMenuScreen`).
3. Escritor de `gamestate_menu`=4 com `gMainState`=2 (bolsa) e o uso de `E+0xC5C` (KI.md passo 2).
4. Volta ao mapa a partir do menu de escolha (sub 3/4).

## 7. Renomeações

Nenhuma. Os nomes de `Overworld_InitChoiceMenuScreen` e `Map_TickChoiceListOpen` já existiam; o despachante 0x0805716C
ainda não tem função no Ghidra, então não foi renomeado.
