# SC3: alvos das tabelas de cena (0x08050CF4, 0x08050E6C, 0x080542B0)

Confianca: **confirmado** = lido na ROM (python -I, bytes da tabela) + decompile/SC.md. **provavel** = nome deduzido.

## 1. Tabela de passos 0x08050CF4 (5 ptrs)

Confere com SC.md §1.1. Sem mudanca: 0x08050D08 (passo 0), 0x08050D40 (1), 0x08050D72 (2), 0x08050F38 (3), 0x08050F40 (4).
Event_RunSceneCommandSequence (decompile) confirma: se passo > 4 devolve 0; senao `jump[passo]`.

## 2. Tabela de codigos por comando 0x08050E6C (33 ptrs, cmd 0x00..0x20)

Lido na ROM: 13 alvos distintos. Cada alvo faz `mov r0,#codigo` e cai no avanco comum. Os 21 comandos
restantes (0x04,0x06,0x07,0x09..0x0F,0x15..0x1F) apontam para 0x08050F2A (avanco comum).

| alvo | cmds (indice) | codigo | rotulo (sym CSV) |
|---|---|---|---|
| 0x08050F00 | 0x00 | 0x10 | SceneCmdCode_Cmd00_To10 |
| 0x08050EF4 | 0x01 | 0x11 | SceneCmdCode_Cmd01_To11 |
| 0x08050F04 | 0x02 | 0x12 | SceneCmdCode_Cmd02_To12 |
| 0x08050EF8 | 0x03 | 0x14 | SceneCmdCode_Cmd03_To14 |
| 0x08050F0C | 0x05 | 0x1A | SceneCmdCode_Cmd05_To1A |
| 0x08050F08 | 0x08 | 0x1B | SceneCmdCode_Cmd08_To1B |
| 0x08050F10 | 0x10 | 0x15 | SceneCmdCode_Cmd10_To15 |
| 0x08050F18 | 0x11 | 0x16 | SceneCmdCode_Cmd11_To16 |
| 0x08050F14 | 0x12 | 0x19 | SceneCmdCode_Cmd12_To19 |
| 0x08050EFC | 0x13 | 0x13 | SceneCmdCode_Cmd13_To13 |
| 0x08050F22 | 0x14 | 0x0F | SceneCmdCode_Cmd14_To0F |
| 0x08050EF0 | 0x20 | 0x0D | SceneCmdCode_Cmd20_To0D |
| 0x08050F2A | demais (21) | — | SceneCmd_CommonAdvance (provavel) |

Confere com SC.md §2 (mesmos codigos). Nenhuma divergencia.

## 3. Tabela de estado 0x080542B0 (140 ptrs, nao e tabela de comando)

Confirmado: o despachante Scene_DispatchStateHandlerTail (0x080542A0, decompile) faz
`codigo = *(r9 + r8.+0xFC*4)`, grava em +0xF4; se codigo > 0x8B chama Overworld_EnterSceneState (0x080567F0) e
depois `jump[codigo]` na tabela 0x080542B0. Ou seja, o indice e o **estado** da cena de mapa, nao um comando de cena.
Esta tabela e a mesma que JT5/R3/VA3 descreveram.

Alvos distintos: 47 (valores de ponteiro). Agrupados por alvo (indice do estado):

- 0x080567F0 (Overworld_EnterSceneState): 90 entradas, idx 0x26..0x7F (funcao de verdade no Ghidra).
- 0x080567FE: idx 0x00, 0x04, 0x25 (miolo da mesma funcao, +0xE).
- 0x08055508: idx 0x10..0x12 (3 entradas, mesmo alvo).
- Demais 44 alvos: 1 entrada cada (ver lista abaixo).

Lista (idx -> alvo): 01->080544E0, 02->080545B4, 03->080550E4, 05->0805513C, 06->08055150, 07->080551D4,
08->08055258, 09->080552F0, 0A->0805531C, 0B->0805534C, 0C->080553D8, 0D->08055464, 0E->08055494, 0F->080554C4,
13->08055724, 14->0805579C, 15->08055824, 16->080558D4, 17->08055590, 18->08055618, 19->080556A0, 1A->08055994,
1B->08055A28, 1C->08055C90, 1D->08055CF8, 1E->08055D68, 1F->08055DD0, 20->08055E24, 21->08055E7C, 22->08055ED4,
23->08055F40, 24->08055F8C, 80->080560BC, 81->0805612C, 82->0805619C, 83->08056280, 84->080563CC, 85->080565B8,
86->08055BB4, 87->08056604, 88->080567A4, 89->08056660, 8A->08056724, 8B->080567D4.

**Estado: NAO nomeado.** Os 44 alvos (exceto 0x080567F0/0x080567FE) nao tem funcao no Ghidra: a consulta
get_function_by_address e decompile_function_by_address devolvem "No function found". Nao ha corpo lido nem
rotulo no mtbe.csv. Nao dou nomes por chute. Nao ha rename no Ghidra (nao existe funcao para renomear).
Para destravar: criar a funcao em cada alvo no Ghidra (nao tenho essa ferramenta aqui) ou disassemblar os bytes
(nao ha disassembler Thumb instalado: capstone/unicorn ausentes).

## 4. Opcodes de evento que usam os comandos (mtbe.csv, confere com SC.md §6)

- Event_Op33_StartBattle 0x0804C53C -> cmd 0x20 (continuacao 0x0804E8C4).
- Event_Op59_Open_Shop 0x0804CD84 -> cmd 0x03 (cont. Event_Op59_Cont_Run_Scene_Command_03 0x0804F67C).
- Event_Op70_Run_Scene_Command_13 0x0804D32C (cont. 0x0804FA1C) -> cmd 0x13.
- Event_Op71_Run_Scene_Command_00 0x0804D340 (cont. 0x0804FA5C) -> cmd 0x00.
- Event_Op72_Run_Scene_Command_02 0x0804D354 (cont. 0x0804FA9C) -> cmd 0x02.
- Event_Op73_Run_Scene_Command_08 0x0804D368 (cont. 0x0804FADC) -> cmd 0x08.
- Event_Op76 (cont. Event_Op76_Cont_Start_Flow4_Then_Scene_Command_05 0x0804FB1C) -> cmd 0x05 depois do flow 4.
- Event_Op78_Run_Scene_Command_10 0x0804D39C (cont. EventOpcode78_RunSceneCommand10 0x0804FB8C) -> cmd 0x10.
- Event_Op79_Run_Scene_Command_12 0x0804D400 (cont. EventOpcode79_RunSceneCommand12 0x0804FBDC) -> cmd 0x12.
- Event_Op7A_Run_Scene_Command_11 0x0804D424 (cont. Event_Op7A_Cont_Run_Scene_Command_11 0x0804FC1C) -> cmd 0x11.
- Event_Op4F_Jump_If_Previous_Scene_Command_Failed 0x0804CB94: le o resultado (SC.md §5).

## 5. Pendente

- Nomes e corpos dos 44 alvos de estado (tabela 0x080542B0) fora de 0x080567F0/0x080567FE. Bloqueado: sem
  funcao no Ghidra e sem disassembler local.
- O "Event_Op59_Cont..." etc. ja tem nome no mtbe; nada a renomear.
- Ghidra: nenhum rename feito (os 13 rotulos de codigo sao labels sem funcao; so no CSV).
