# MN: esqueleto do jogo (boot, laço principal, estados globais, cenas)

Feito em 2026-10-14 pelo agente GH-MN. Lido no Ghidra (decompile + literais da ROM). Confiança: **confirmado** (lido no
código), **provável**, **chute**. Símbolos novos em `MN_simbolos.csv`. Nada foi renomeado no Ghidra: os nomes que
conferi já estavam lá (AgbMain, MainState0..3, GameState_StartFlowByCommand, StateMachine_DispatchSub, etc.).

## 1. Boot e laço principal

- Vetor 0x08000000 (ARM, `entry`) -> `crt0` 0x080000C0 (ARM): pilhas, `INTR_VECTOR` (0x03007FFC) -> `IntrMain`
  0x08000104 (ARM, lê REG_IE/REG_IF), e chama `AgbMain` 0x0800025C (Thumb). **confirmado** (nomes do CSV + xrefs).
- `AgbMain` (confirmado, decompile): `WAITCNT = 0x4014`; `ResetHardwareAndMemory` 0x080004E0;
  `InitializeInterruptsAndDefaultHandlers` 0x08000458; `DISPCNT = 0x60`; `gMainState = 3`; `gamestate_menu = 0`;
  `gamestate_subroutine = 0`; `InitializeLinkTransferState` 0x080020C8; `Scene_InitSoundAndStartSong0` 0x0800058C.
- Laço (`do { ... } while(1)`), uma volta por quadro:
  1. `VBlankWait` 0x0807CCE8 (provável; espera o VBlank).
  2. zera byte 0x03002410.
  3. `get_player_inputs` 0x08000844 (ver §3).
  4. `gFrameCount` 0x03001780 ++.
  5. Relógio de jogo: se `gPlayTimeRunning` (0x03000588) != 0 e `gPlayTimeFrames` (0x03002B88) <= 0x14996FE, soma 1.
     Trava em 21599999 (99:59:59), como já estava no HANDOFF.
  6. Entrada de link: `UpdateLinkTransferState` 0x080021F4 recebe o bloco 0x03005012 e grava o bitfield de slots
     conectados em 0x03005010. Para cada um dos 4 slots: sem sinal, o contador de 0x03005029+i sobe até 11 e então
     zera o bit correspondente em 0x03005172 (desconectado). Com sinal, zera o contador e liga bit em 0x03005172 se o
     bit não estava (provável: entrada recebida do peer).
  7. `is_input_soft_reset` 0x08000964 (confirmado, ver §4). Se der 0 e a flag 0x03005030 != 0, lê dois halfwords
     do bloco de link (0x03005018 ou 0x0300501C, conforme o byte 0x03000B90): se ambos forem 0xFFFF, grava
     `gMainState=0`, `gamestate_menu=10`, `gamestate_subroutine=0x10` (provável: peer sumiu, volta ao menu 10).
     Se der 1 (reset), faz `VBlankWait` + `Scene_InitSoundAndStartSong0(0x03002430)` + `VBlankWait`.
  8. **Despacho da cena**: `CallViaR0(gMainStateTable[gMainState])`, tabela 0x083ABF98 (6 entradas, só 0..3 são
     reais; 4 e 5 = `MainStateNop` 0x08000258; as entradas 6 e 7 do literal são lixo: já são outra tabela).
  9. `BuildShadowOamFromObjectState` 0x08001520 e `QueueLinkTransferPacket` 0x080022DC (pacote 0x03005014).

## 2. Máquina de estados global (confirmado)

Três variáveis escolhem a cena: `gMainState` (0x0300178C) escolhe o `MainStateN`; `gamestate_menu` (0x03002B8C)
escolhe o item da tabela daquele estado; `gamestate_subroutine` (0x0300137C) é o passo dentro da cena.

| gMainState | Função | Tabela indexada por gamestate_menu | Observação |
|---|---|---|---|
| 0 | `MainState0` 0x08002534 | `menu_list` 0x083AC2C8 (182 entradas) | Cenas do jogo. `[0]` = `Dispatch_CallIndexedHandler` 0x08002554 (indexa por gamestate_subroutine). `[63]` = `DebugMenu_Dispatch` 0x0800B890 (menu de debug, ninguém escolhe na ROM original). `[11..62]` e a maioria = `NoOpActionHandler` 0x08000840. |
| 1 | `MainState1` 0x0804763C | 0x083D2104 (provável) | `[1]` = `EventScript_RunCurrent` 0x0804A220: VM de eventos do **mapa**. `[2]` = 0x0805716C (provável: escolha do overworld, `Overworld_InitChoiceMenuScreen`, não confirmado). |
| 2 | `MainState2` 0x0805BBE0 | 0x08483594 (provável) | Usa o mesmo teste de link (0x03005030 + halfwords 0xFFFF / máscara 0x00FFFF00 em 0x03005010+0x1C): se passa, chama `table[menu]`; se não, grava menu=10, sub=0x10. `[3..12]` são handlers (ver lacunas). |
| 3 | `MainState3` 0x08068440 | 0x084856E8 (provável) | Estado do boot. `[0]` = 0x0806E460 (nome atual `Battle_DispatchThenEndTransition`: **nome suspeito** para o boot; ver lacunas). `[3]..[6]` = 0x0806E130, 0x08068DD0, 0x0806B744, 0x080708D8. |

### 2.1 Escolha de fluxo: `GameState_StartFlowByCommand` 0x08047878 (confirmado)

Recebe `(cmd, param)` e grava os três estados de uma vez (valores lidos dos literais):

| cmd | gMainState | gamestate_menu | sub | outros bytes |
|---|---|---|---|---|
| 0x00 | 2 | 0x12 | (não muda) | `0x03000584` = 0 |
| 0x01 | 2 | 0x12 | (não muda) | `0x03000584` = 1 |
| 0x02 | 2 | 0x12 | (não muda) | `0x03000584` = 2 |
| 0x03 | 2 | 0x0B | (não muda) | `0x03002420` = param |
| 0x04 | 2 | 0x15 | (não muda) | (chamado pelo evento op 0x76) |
| 0x05 | 2 | 0x14 | (não muda) | `0x03000588` = 0 |
| 0x06 | 2 | 0x17 | (não muda) | |
| 0x08 | 2 | 0x16 | (não muda) | |
| 0x10 | 3 | 4 | (não muda) | |
| 0x11 | 3 | 5 | (não muda) | |
| 0x12 | 3 | 3 | 0 | |
| 0x13 | 3 | 6 | (não muda) | |
| 0x20 | 0 | 6 | 0 | `0x03005E0D` = 1 (encontro por evento) |
| 0x21 | 0 | 6 | 0 | `0x03005E0D` = 0 (encontro aleatório) |
| 0x40 | 0 | 1 | 0x10 | |
| 0x41 | 1 | 1 | 0x18 | saída de mapa, caso 0 |
| 0x42 | 1 | 1 | 0x1C | saída de mapa, caso 3 |
| 0x43 | 1 | 1 | 0x28 | saída de mapa, caso 1 |

"(não muda)" = a função não escreve `gamestate_subroutine`; quem chama depende do passo que já estava. Cmd não listado
cai no `default` e não faz nada.

Como a cena 1 é o mapa, `Map_ExitStateDispatchFlow` 0x08057B0C escolhe a saída pelo byte `ctx+0x65` (ctx = literal 0x08057B34 -> 0x030064A0,
ou seja, byte 0x03006505): 0 -> cmd 0x41; 1 -> 0x43; 3 -> 0x42; 2 -> só grava sub=5 (sem troca de cena).

### 2.2 Passo de subestado: `StateMachine_DispatchSub` 0x08041CA4 (confirmado)

Zera o byte `+4` do bloco, chama `table[+0x1B]` (passo de 4 bytes, tabela da cena) e devolve `estado > 7`. É o que
os handlers "esperam o texto/sub-passo terminar" usam (ex.: `Fight_SceneStep5C_WaitTextBoxDone`). `StateMachine_DispatchStep`
0x0803FB3C é a versão com o byte +4 e tabela `gMessageStateHandlerTable` 0x083B291C (confirmado).

## 3. Leitura de teclas (confirmado)

`get_player_inputs` 0x08000844 lê `REG_KEYINPUT` 0x04000130, inverte com `^0x3FF` (botões ativos em alto):

| endereço | conteúdo |
|---|---|
| 0x03001790 | teclas do quadro anterior |
| 0x03002B64 | teclas seguradas agora (u16) |
| 0x0300058C | teclas apertadas neste quadro (= agora & ~antes) |
| 0x03001C00 | teclas soltas neste quadro (= antes & ~agora) |
| 0x03001798 | "repeat": aperto novo, ou segurada com repetição a cada 4 quadros depois de 16 |
| 0x03002418 | contador da repetição (16 no aperto novo, 3..0 depois) |

Bits: 0 A, 1 B, 2 Select, 3 Start, 4 Dir, 5 Esq, 6 Cima, 7 Baixo, 8 R, 9 L.

## 4. Reset

- Reset por botões: `is_input_soft_reset` 0x08000964 (confirmado): se `held & 0xF == 0xF` (A+B+Select+Start seguros)
  **e** `pressed & 8` (Start apertado neste quadro), chama `SoftResetSequence` 0x080008C0 (provável: reinicia
  hardware e volta o `gMainState` para 0; o HANDOFF diz "soft reset = 0") e devolve 1. O laço então toca a música de novo.
- Interrupção: `Gfx_VBlankIrq` 0x080005D0 (confirmado em FL2.md): ack do IF, protege reentrada, chama
  `VBlankIrqHandler` 0x0800065C, troca buffers se o flag 0x80 estiver ligado, conta quadros.

## 5. Cenas e como se passa de uma para outra

Diagrama (condição entre parênteses):

```
boot: AgbMain -> gMainState=3, menu=0
  |
  v
MainState3 [menu 0] 0x0806E460  (boot / intro: função suspeita, ver lacunas)
  |-- cmd 0x10/0x11/0x13/0x12 (StartFlow) --> MainState3 [menus 4/5/6/3]  (cenas 3..6 não identificadas)
  |-- cmd 0x00/0x01/0x02/0x03/0x04/0x05/0x06/0x08 --> MainState2 [menus 0x12,0x0B,0x15,0x14,0x17,0x16]
  |-- cmd 0x40 --> MainState0 [menu 1] com sub=0x10
  '-- (título, menu inicial, save: NÃO ACHADO)

MainState0 [menu 1] (cena 1, 0x080027BC, sem função no Ghidra)
MainState0 [menu 6] (encontro, 0x08010264, sem função no Ghidra)
  <-- StartFlow 0x20 (evento op 0x33 "StartBattle", via Event_RunSceneCommandSequence(0x20))
  <-- StartFlow 0x21 (Overworld_WalkStepByDirection 0x08051988: passo sobre o mapa + Overworld_ShouldTriggerRandomEncounter)
  '--> sub=0 ... (passo seguinte da cena 6 não lido)

MainState0 [menu 7] Battle_FrameDispatchOrExit 0x08012970 (batalha: passo de turno)
  <-- Fight_SceneStep4F_ReturnToTurnScene: menu=7, sub=0x46
MainState0 [menu 8] Fight_SceneMain 0x08030860 (batalha: passa sub pela tabela 0x083AECD8)
MainState0 [menu 10] 0x0803D538 (cena 10, não identificada; destino do "peer sumiu" e do link)
MainState0 [menu 63] DebugMenu_Dispatch 0x0800B890 (debug; só pelo código de debug)

MainState1 [menu 1] EventScript_RunCurrent 0x0804A220 (MAPA: VM de eventos)
  |-- map exit: Map_ExitStateDispatchFlow 0x08057B0C, por ctx+0x65:
  |     0 -> cmd 0x41 (MainState1 menu 1 sub 0x18)
  |     1 -> cmd 0x43 (MainState1 menu 1 sub 0x28)
  |     3 -> cmd 0x42 (MainState1 menu 1 sub 0x1C)
  |     2 -> sub = 5 (sem cena nova)
  |-- evento op 0x76 (Event_Op76_Cont_Start_Flow4_Then_Scene_Command_05) --> cmd 4 (MainState2 menu 0x15)
  |-- passo aleatório (Overworld_WalkStepByDirection, Overworld_ShouldTriggerRandomEncounter) --> cmd 0x21 (MainState0 menu 6)
  |-- warp (Map_FindWarpTriggerAtTile) --> sub=3 no byte do mapa e efeito de transição (sem cena nova)
  '-- Overworld_BuildEncounterNpcSnapshot antes do cmd 0x21 (monta NPCs próximos)

MainState2 [menus 3..12] handlers (ver lacunas), com o teste de link -> menu 10 sub 0x10.
```

Como os nomes de cena aparecem no código (confirmado pelos literais): o `gamestate_menu` é o único valor que
troca de "tela" de verdade; `gMainState` troca só o tipo de loop. Só o `StartFlowByCommand` e os `GameState_*`
escrevem esses valores pelo código de jogo lido até agora. Escritas com imediatos (`mov r0,#8; str` etc.) não são
achadas por xref: para achar quem escreve `gamestate_menu=8` ou `=0x1B` precisa varrer a ROM.

## 6. Variáveis globais de estado (RAM)

| endereço | nome | tipo | quem escreve (exemplos) | significado |
|---|---|---|---|---|
| 0x0300178C | gMainState | u32 | AgbMain (3), StartFlow, SoftReset (0) | índice em `gMainStateTable`. Confirmado. |
| 0x03002B8C | gamestate_menu | u32 | AgbMain (0), StartFlow, link (10) | índice em `menu_list` (MS0), em `MS1`, `MS2`, `MS3`. Confirmado. |
| 0x0300137C | gamestate_subroutine | u32 | AgbMain (0), StartFlow, `StateTable_*`, `Map_ExitStateDispatchFlow` (=5) | passo dentro da cena. Confirmado. |
| 0x03000584 | (sem nome) | u8 | StartFlow cmd 0..2 | variante do fluxo 0/1/2. Provável. |
| 0x03002420 | current_shop (nome do mtbe.csv) | u8 | StartFlow cmd 3 (param) | conflito: o CSV diz `current_shop`; aqui é o parâmetro do cmd 3. Não renomeei. |
| 0x03005E0D | (sem nome) | u8 | StartFlow 0x20 (=1) / 0x21 (=0) | flag "encontro/batalha vindo do mapa". Provável. |
| 0x03000588 | gPlayTimeRunning | u8 | StartFlow cmd 5 (=0), relógio | flag do relógio de jogo. Confirmado. |
| 0x03002B88 | gPlayTimeFrames | u32 | AgbMain (relógio) | tempo de jogo em quadros, trava 21599999. Confirmado. |
| 0x03001780 | gFrameCount | u32 | AgbMain | contador de quadros. Confirmado. |
| 0x03002410 | (sem nome) | u8 | AgbMain (zera todo quadro) | flag por quadro. Confirmado escrita. |
| 0x03005030 | (sem nome) | u8 | link | flag de link ativo (teste em AgbMain e MS2). Provável. |
| 0x03005010 | (bloco de link) | struct | UpdateLinkTransferState | +0 bitfield de slots conectados; +0x18..0x1B contadores de timeout; +0x1D/+0x1E flags. Provável. |
| 0x03005014 | (pacote de link) | u32 | QueueLinkTransferPacket | pacote de saída. Provável. |
| 0x03005170 | (entrada de link) | struct | link | entrada por slot; +2 = byte de teclas. Provável. |
| 0x03000B90 | (sem nome) | u8 | link | índice do slot local (0 ou 1, afeta qual halfword é lido). Provável. |
| 0x03002B64 | held_buttons | u16 | get_player_inputs | teclas seguradas. Confirmado. |
| 0x0300058C | keys_pressed | u16 | get_player_inputs | teclas novas neste quadro. Confirmado. |
| 0x03001C00 | keys_released | u16 | get_player_inputs | teclas soltas neste quadro. Confirmado. |
| 0x03001790 | keys_prev | u16 | get_player_inputs | teclas do quadro anterior. Confirmado. |
| 0x03001798 | keys_repeat | u16 | get_player_inputs | aperto + repetição. Confirmado. |
| 0x03002418 | keys_repeat_timer | u8 | get_player_inputs | contador da repetição. Confirmado. |
| 0x03000440 | gMessageCtx | ctx | (texto/ messages; não é estado de cena) | ver A6.md |
| 0x03000470 | gDebugReturnScene | u8 | debug | cena para voltar do menu de debug. Já no CSV. |

## 7. Lacunas

1. **Cenas do `menu_list` 1..7, 9..10 e 64..70 sem função no Ghidra** (0x080027BC, 0x08003374, 0x080039E0,
   0x08007584, 0x0800D9DC, 0x08010264, 0x0803D538, 0x08002574...): decompile dá "No function found". Só dá para
   descrever por literal. Precisa criar as funções (script do MedabotsSync, não eu).
2. **Título, menu inicial, save, loja**: não achei qual `gamestate_menu`/`gMainState` cada uma usa. Suspeita: `MainState3`
   (cenas 0..6 da tabela 0x084856E8) ou `menu 0` com sub por `0x083AECD8`. Não confirmado.
3. **Boot**: `gMainStateTable[3][0]` = 0x0806E460 tem nome `Battle_DispatchThenEndTransition` no CSV, o que parece
   errado para o primeiro passo de jogo. Ler a função antes de aceitar o nome.
4. **Tabela 0x083D2104 (MS1)** e **0x08483594 (MS2)**: só três e poucas entradas lidas; resto não mapeado.
5. **`Dispatch_CallIndexedHandler`** (menu 0): tabela de sub-passos em 0x0800256C (literal) não identificada; o byte
   `DAT_08002570` também não.
6. **Quem seta `gamestate_menu` = 8, 9, 0x0B..0x17 com imediato**: xref não acha; precisa varrer a ROM por
   `mov/str` ou pelo `StartFlow`. A ordem real "mapa -> encontro (6) -> batalha (8/7) -> resultado -> mapa" está
   montada só pelos handlers, sem a transição de volta do resultado.
7. **Efeito do `SoftResetSequence`** (0x080008C0): não lido.
8. **Byte `ctx+0x65` do mapa** (`Map_ExitStateDispatchFlow`): ctx = 0x030064A0 pelo literal, byte 0x03006505. Nome não conferido.
9. **`Gfx_VBlankIrq`/`IntrMain`**: só o caminho principal; a parte de cada IRQ não foi relida (FL2.md cobre).
10. **`GameState_AdvanceSubroutine`** (usada por `Map_TickChoiceListOpen` e FUN_0804AA44): endereço não achado no
    CSV; provavelmente incrementa 0x0300137C. Não confirmado.
11. **Conflitos de nome com o mtbe.csv**: 0x03002420 (`current_shop` no CSV, aqui é o param do cmd 3); 0x03001798
    (`keys_held` no CSV, mas o código põe ali aperto novo + repetição, então é `keys_repeat`). Não mexi em mtbe.csv.
12. **Renomeações**: nenhuma feita. Os nomes que confirmei já estavam no Ghidra (AgbMain, MainState0..3,
    GameState_StartFlowByCommand, StateMachine_DispatchSub/DispatchStep, get_player_inputs, is_input_soft_reset).
    Os nomes `MainState1/2/3` e `Battle_DispatchThenEndTransition` precisam de revisão antes de virar confirmados.
