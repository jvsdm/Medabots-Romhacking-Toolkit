# MU: catálogo estado -> menu -> função -> tela (GH-MU, 2026-10-14)

Leitura: tabelas na ROM original `Medabots - Metabee (Europe).gba` (python -I, só leitura), checagem de função no Ghidra
(get_function_by_address, sem decompile em massa, sem salvar, sem renomear). Ponteiros da tabela trazem bit Thumb;
o Ghidra só acha pelo endereço par. Símbolos: `MU_simbolos.csv`. Sem função no Ghidra: `MU_lista.txt`.

Confiança: **confirmado** (tabela lida + função existe no Ghidra ou leitura de código anterior), **provável**,
**pendente** (não identificado).

## 1. Mecanismo

`gMainState` 0x0300178C -> `gMainStateTable` 0x083ABF98 (0..3 reais; 4 e 5 = `MainStateNop` 0x08000258).
`gamestate_menu` 0x03002B8C -> tabela do estado:

| gMainState | função | tabela de menus | entradas úteis |
|---|---|---|---|
| 0 | MainState0 0x08002534 (confirmado) | menu_list 0x083AC2C8 | 0..10, 63, 64..70 |
| 1 | MainState1 0x0804763C (confirmado) | MS1 0x083D2104 | 1, 2, 10..39 |
| 2 | MainState2 0x0805BBE0 (confirmado) | MS2 0x08483594 | 3..12, 16..28, 30..39 |
| 3 | MainState3 0x08068440 (confirmado) | MS3 0x084856E8 | 0..30 (31+ são bytes, não ponteiros) |

Ponteiro 0x08000841 (ou 0x08000840 no Ghidra) = `NoOpActionHandler`: entrada vazia. Quase todas as entradas
11..62 de menu_list e 3, 7, 9, 13..15, 29 de MS2/MS3 são vazias.

## 2. Catálogo (cenas com função conhecida ou nome provável)

| estado | menu | função (Ghidra) | tela | confiança |
|---|---|---|---|---|
| 0 | 0 | Dispatch_CallIndexedHandler 0x08002554 (MN) | despachante por passo (não rechecado) | confirmado (MN) |
| 0 | 1 | 0x080027BC, SEM função | cena do fluxo 0x40 (sub 0x10) | pendente |
| 0 | 2 | 0x08003374, SEM função | pendente | pendente |
| 0 | 3 | 0x080039E0, SEM função | pendente | pendente |
| 0 | 4 | 0x08007584, SEM função | pendente | pendente |
| 0 | 5 | Menu5_Dispatch_PartsScreens 0x0800D9DC | MEDABOTS/peças: grade (passo 0 = MedabotGrid_InitScreen 0x0800DA34) e detalhe de peça (passo 66 = 0x0800EB34) | provável (ST2) |
| 0 | 6 | Scene_GsDispatch_Menu6_Encounter 0x08010264 | encontro (batalha de história 0x20 e aleatória 0x21) | confirmado (ST2/MN) |
| 0 | 7 | Battle_FrameDispatchOrExit 0x08012970 (BT) | batalha: quadro | confirmado (BT) |
| 0 | 8 | Fight_SceneMain 0x08030860 (BT) | batalha: execução da robattle | confirmado (BT) |
| 0 | 9 | Fight_RunActionScriptLoop 0x08033840 (BT) | batalha: scripts de ação | confirmado (BT) |
| 0 | 10 | Scene_GsDispatch_Menu10_Link 0x0803D538 | link (saída da batalha e "peer sumiu") | confirmado (ST2/BT) |
| 0 | 11..62 | NoOpActionHandler | vazio | confirmado |
| 0 | 63 | DebugMenu_Dispatch 0x0800B890 | menu de debug | confirmado |
| 0 | 64..70 | 0x08002574..0x0800277C (sem checagem nesta rodada) | pendente | pendente |
| 1 | 1 | EventScript_RunCurrent 0x0804A220 | MAPA (VM de eventos) | confirmado (MN) |
| 1 | 2 | 0x0805716C (sem checagem) | escolha do overworld | provável (MN) |
| 1 | 10 | site_NewGameClear 0x0804A2F0 | NEW GAME (limpa RAM, mapa 23) | provável (README, nome) |
| 1 | 11..39 | 0x0804A60C..0x0804ABCC (não checados; 0x0804B15C repete em 22, 25, 31, 32, 33, 37) | pendente | pendente |
| 2 | 3 | FUN_0805c0d4 | pendente | provável só pelo nome |
| 2 | 4 | Dispatch_StateTableFrameCount 0x0805CB50 | despachante | confirmado (nome) |
| 2 | 6..9, 12, 24..28, 30..39 | 0x0805D704..0x0805C988 (não checados) | pendente | pendente |
| 2 | 10, 23 | 0x08060688, SEM função | pendente | pendente |
| 2 | 11 | 0x08060D6C, SEM função | LOJA (cmd 0x03, arg = id da loja, SC.md) | provável (SC) |
| 2 | 16 | 0x08063948, SEM função | pendente | pendente |
| 2 | 17 | Dispatch_StateTableWithCopy 0x0806463C | despachante | confirmado (nome) |
| 2 | 18 (0x12) | 0x08064DB4, SEM função | flow 0x00/0x02 (cmd 0x71/0x72) | pendente |
| 2 | 19 | Dispatch_CallHandlerFromTable4 0x080649BC | despachante | confirmado (nome) |
| 2 | 20 (0x14) | Fight_CallHandlerCopyArgs52 0x08066824 | flow cmd 0x05 (0x76); nome "Fight_" é suspeito | pendente |
| 2 | 21 (0x15) | Fight_CallHandlerWithArgCopy 0x08065D10 | flow 0x04 = link Medabot (provável) | pendente (nome "Fight_" suspeito) |
| 2 | 22 (0x16) | FUN_08067b9c | flow cmd 0x08 (0x73) | pendente |
| 3 | 0 | Battle_DispatchThenEndTransition 0x0806E460 | boot/intro (nome suspeito, MN) | confirmado existe; tela pendente |
| 3 | 1, 2 | 0x0806EA98, 0x0806D588 (não checados) | pendente | pendente |
| 3 | 3 (0x12) | 0x0806E130, SEM função | flow cmd 0x12 (0x79) | pendente |
| 3 | 4 (0x10) | Dispatch_ScrollTickThenHandler 0x08068DD0 | flow cmd 0x10 (0x78) | pendente (nome "Scroll" é só de nome) |
| 3 | 5 (0x11) | Dispatch_CallHandlerByWordIndex 0x0806B744 | flow cmd 0x11 (0x7A) | pendente |
| 3 | 6 (0x13) | Fx_FrameTickAndDispatch 0x080708D8 | flow cmd 0x13 (0x70), espera Map_ModeDispatch | pendente |
| 3 | 8..30 | 0x08068920..0x080692EC (não checados) | pendente | pendente |

## 3. Fluxos (StartFlowByCommand 0x08047878, ligando estado, menu e tela)

| cmd | estado.menu | opcode | tela |
|---|---|---|---|
| 0x00 / 0x02 | 2.0x12 | 0x71 / 0x72 | flow 0x12 (pendente) |
| 0x03 | 2.0x0B | 0x59 | LOJA (arg = id) |
| 0x04 | 2.0x15 | 0x76 | link Medabot (provável) |
| 0x05 | 2.0x14 | 0x76 | pendente |
| 0x08 | 2.0x16 | 0x73 | pendente |
| 0x10 / 0x11 / 0x12 / 0x13 | 3.4 / 3.5 / 3.3 / 3.6 | 0x78 / 0x7A / 0x79 / 0x70 | pendente |
| 0x20 | 0.6 | 0x33 | batalha de história (encontro por evento) |
| 0x21 | 0.6 | (passo do mapa) | encontro aleatório |
| 0x40 | 0.1 (sub 0x10) | | mapa |
| 0x41 / 0x42 / 0x43 | 1.1 (sub 0x18 / 0x1C / 0x28) | | saída do mapa |

## 4. Pendente (próxima rodada)

1. Checar função e identificar tela das entradas marcadas "pendente" acima (prioridade: MS3 4/5/6, MS2 18/20/21/22, MS3 3, menu_list 1..4).
2. Quebrar a pilha de despachantes (Dispatch_*, Fight_CallHandler*) para achar as tabelas de passos reais.
3. Título, menu inicial, save: continuam sem achado (ver MN.md lacuna 2).
4. Renomeações no Ghidra (Menu_<Tela>_Main): nenhuma feita; só depois de confirmar a tela.
5. Os nomes "Fight_*" em MS2 20/21 vêm do Ghidra e não parecem batalha: suspeitos, não renomear sem leitura.
