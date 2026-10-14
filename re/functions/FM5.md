# FM5: retorno da tela de B, arg=0 de Menu_SetStateByArg01, tela de SALVAR e MEDAWATCH (GH-FM5, 2026-10-14, PARCIAL)

Leitura: ROM original (só leitura; literais e tabelas via python -I), Ghidra (decompile, disassemble, xrefs).
Renomeados no Ghidra: `FUN_0804765C` -> `Menu_SetStateByArg01`; `FUN_08003374` -> `PartScreen_DispatchSubstate`.
Símbolos: `FM5_simbolos.csv`. Continua de FM, FM2, FM3, FM4. Confiança: **confirmado** (lido no código/bytes), **provável**, **pendente**.
Endereços de estado: gMainState 0x0300178C, gamestate_menu 0x03002B8C, gamestate_subroutine 0x0300137C, teclas novas 0x0300058C,
teclas seguradas 0x03001798, ctx de mensagens/menu 0x03000440, E = 0x030057E0 (E+0xCBA = 0x0300649A, E+0xC88 = 0x03005468).

## 0. Correção de FM4 §4

- FM4 disse que o sub 3 da tela de B não escreve gMainState. **Certo no sub 3 em si**, mas o fim do sub 3 chama
  `Anim_SetDefaultTimers` (0x08044098), chamado de `Gfx_BgStripeStep` 0x08002C10 quando o contador (ctx+0x84) passa de 0x12.
  Essa função grava: **gMainState = 1, gamestate_menu = 1, gamestate_subroutine = 7**. **Confirmado** (bytes + pools).
  Ou seja, o escritor de gMainState=1 existe e é este.

## 1. Como a tela de B volta ao mapa (objetivo 1)

Cadeia confirmada:
1. Mapa, sub 2 (0x080510C0), B novo (0x0300058C bit 1) com E+0xC33 != 0 (0x08051204): grava gMainState=0, menu=1, sub=0 e
   **E+0xCBA = 2** (0x08051288 é o str do byte; FM2 §1).
2. MainState0 menu 1 (tabela 0x083AC404): sub 0 init (0x080027DC) -> sub 1 revela (0x080028AC) -> sub 2 controle (0x08002A2C).
3. No sub 2, B -> sub 3 (0x08002AF8 `Gfx_BgStripeStep`): desenha 0x12 passos de faixas.
4. Fim das faixas -> `Anim_SetDefaultTimers` (0x08044098): **gMainState=1, menu=1, sub=7**. Volta ao mapa pelo
   `MainState1[1]` = `EventScript_RunCurrent` (0x0804A220), com sub 7 (**provável** que o sub 7 do VM de eventos seja a
   retomada; não confirmado).
5. E+0xCBA continua 2 até o sub 4 do mapa (0x08051368, grava 1 e gMainState=1 em 0x08051376), FM2 §2.

Outros escritores de gMainState=1 / gamestate_menu=1 achados (xref 0x0300178C e 0x03002B8C):
- `Menu_SetStateByArg01(0)` (ver §2), chamado de `Gfx_ResetBgLayersAndSong` 0x08011584 (**confirmado**, bytes: `mov r0,#0; bl`).
- `MenuB_Sub9_ExecRow` 0x08002F32 chama `Menu_SetStateByArg01(1)` (k=2 de FM4), confirmado.

**Pendente**: quem chama `Gfx_ResetBgLayersAndSong` (não fiz o xref) e o significado de sub 7 no VM de eventos.

## 2. Ramo arg=0 de Menu_SetStateByArg01 0x0804765C (objetivo 3, confirmado no código)

Decompile + disasm (0x0804765C..0x080476D8). Os dois ramos:
- **arg=0**: copia 1 byte de (0x03000440 + 0x9C) para (0x03005310 + 0x39) (= ctx do mapa +0x39; **confirmado**). Depois lê E+0xC88:
  - E+0xC88 == 0: **gMainState=1, gamestate_menu=1, gamestate_subroutine=0x0D**.
  - E+0xC88 != 0: **gMainState=1, gamestate_menu=1, gamestate_subroutine=5**.
  Ou seja, volta para o VM de eventos do mapa (menu 1) com sub 0x0D ou 5. Quem chama: `Gfx_ResetBgLayersAndSong` 0x08011584.
  O significado de E+0xC88 (nome da flag) e de sub 0x0D/5 no VM: **pendente** (MN.md cita E+0xC88 como guarda do mapa).
- **arg=1**: gMainState=1, gamestate_menu=2, gamestate_subroutine=0 (**confirmado**; é o caminho de VEÍCULO, FM4 §2 k=2).
- Outros valores: retorno sem fazer nada.

## 3. Tela de SALVAR: MainState2 menu 5 (objetivo 2, PARCIAL)

- Entrada (FM4 §2, k=3): gPlayTime = 0; gMainState=2; gamestate_menu=5; sub=0. Dispatcher 0x0805D704 (`Obj_UpdateFrameAndSetAttr`)
  com a tabela **0x08483670** (8 entradas, lidas nos bytes; valor com bit Thumb -> endereço = valor - 1):

| sub | endereço | nome / função | confiança |
|---|---|---|---|
| 0 | 0x0805D750 | `Screen_ClearBufferAndResetDispcnt` (já nomeado) | confirmado (FM4 §2) |
| 1 | 0x0805D790 | não lido | pendente |
| 2 | 0x0805D860 | não lido | pendente |
| 3 | 0x0805DA14 | não lido | pendente |
| 4 | 0x0805DAB0 | não lido | pendente |
| 5 | 0x0805DAD4 | não lido | pendente |
| 6 | 0x0805DB84 | não lido | pendente |
| 7 | 0x0805DBC4 | não lido | pendente |

- Ligação com o save (SV.md / SV2.md): `Save_WriteEepromAndFinish` 0x0806050C grava a EEPROM (e chama `lib_eeprom_write_verify`,
  SV2); os handlers do fluxo de salvar estão em 0x08483614 (Save_InitScreen), 0x08483688 (Save_StepThenFlashOp),
  0x08483724 (Save_FinishFlashStep), 0x08483770 (Save_WriteEepromAndFinish), já citados em SV.md. **Não confirmei** se essas
  quatro entradas pertencem à tabela 0x08483670 (os endereços estão fora de 0x08483670..0x0848368F) nem qual sub chama
  `SaveLoad_SaveGame` 0x0805E010 (xref de `SaveLoad_SaveGame`: próxima rodada). Não li SaveLoad_SaveGame aqui.
- Retorno ao mapa/menu depois de salvar: **pendente**.

## 4. MEDAWATCH: despachante 0x08003374 e tabela 0x083AC454 (objetivo 4, confirmado nos bytes e decompile)

- `PartScreen_DispatchSubstate` (ex-FUN_08003374): `CallViaR0(tabela[gamestate_subroutine])`, tabela 0x083AC454. É a tela de
  gamestate_menu=2 com gMainState=0 (k=0 de FM4), **confirmado**. A tabela tem 14 entradas; as NoOp são 6, 7, 10..13.
- Ponteiros usados pelos handlers: keys_pressed 0x0300058C (A = bit 0, B = bit 1), held 0x03001798 (cima/baixo = 0x80/0x40),
  cursor = ctx+0x8F em 0x03000440 (valores 0..2), contador = ctx+0x84 (0x03000440+0x84).

| sub | endereço | nome | o que faz (lido) | confiança |
|---|---|---|---|---|
| 0 | 0x08003394 | `Gfx_InitBgLayersAndEnableDisplay` | init da tela | confirmado (FM4) |
| 1 | 0x080034A4 | `PartScreen_FadeInPalettesStep` | fade de paleta em 9 passos; depois sub+1 | confirmado |
| 2 | 0x08003504 | `PartScreen_HandleCursorInputStep` | **controle**: cima/baixo muda cursor (0..2, som 2) e redesenha. **A** (bit 0): som 4, `Slot_SetupFromRecordList` com o registro escolhido, blend, contador=0, **sub=8** (abre a sub-tela escolhida). **B** (bit 1): som 5, sub+1 (-> 3, fecha) | confirmado |
| 3 | 0x08003620 | `PartScreen_ClearAndFadeOutStep` | limpa OAM e paleta, som de fade 8, sub+1 | confirmado |
| 4 | 0x08003654 | `PartScreen_FadeToColorStep` | fade para cor em 9 passos, sub+1 | confirmado |
| 5 | 0x080036A8 | `PartScreen_StartSongStep` | toca música 6; **grava gamestate_menu=1 e gamestate_subroutine=0x10** (gMainState não muda: fica 0) | confirmado (bytes); **pendente**: sub 0x10 do menu 1 cai fora da tabela 0x083AC404 (12 entradas) |
| 8 | 0x080036C8 | `PartScreen_WaitBlendAnimStep` | roda script do slot e tick de blend; quando termina, sub+1 (-> 9) | confirmado |
| 9 | 0x080036F4 | `Menu_ResetSubstateByMode` | copia buffers por DMA3 e escolhe o próximo menu pelo cursor (ver abaixo) | confirmado |

Sub 9, por cursor (ctx+0x8F):
- cursor 0 -> **gamestate_menu = 3**, sub=0 (MainState0 menu 3; opção da linha 0 do MEDAWATCH).
- cursor 1 -> **gamestate_menu = 4**, sub=0.
- cursor 2 -> **gamestate_menu = 5**, sub=0.
Todos com gMainState = 0. Mapeamento cursor -> conteúdo (MEDABOTS / MEDALS / MEDAPARTS, pela ordem de MN2): **provável**, não
confirmado por texto. Os destinos menu 3/4/5 de gMainState 0 **não foram lidos** (pendente).

Fluxo MEDAWATCH resumido: menu 2 -> sub 0 init -> sub 1 fade in -> sub 2 cursor (A: sub 8 -> 9 -> menu 3/4/5; B: sub 3 -> 4 -> 5 -> menu 1 com sub 0x10).

## 5. Pendências (próxima rodada)

1. Quem chama `Gfx_ResetBgLayersAndSong` 0x08011584 (xref) e o que é o sub 7 de EventScript_RunCurrent (retomada do mapa).
2. E+0xC88 (nome e significado) e sub 0x0D / 0x05 do VM de eventos (saída de `Menu_SetStateByArg01(0)`).
3. Sub 0x10 do menu 1 (B-screen / MEDAWATCH sub 5) — tabela 0x083AC404 tem só 12 entradas; ver se o dispatcher aceita 0x10.
4. Tela de SALVAR: subs 1..7 de 0x08483670 (decompile de 0x0805D790..0x0805DBC4) e a chamada a `SaveLoad_SaveGame`.
5. Destinos de MEDAWATCH cursor 0/1/2 (menu 3/4/5 de gMainState 0).
6. E+0xCBA: outros 11 pontos com o literal 0x0CBA (pool), a maioria sem função no Ghidra: 0x08048120 (leitura, chama
   0x080513C8), 0x0804A856, 0x0804A9A2, 0x0804AAB4, 0x0804AD14, 0x0804B100, 0x08050DD2, 0x08050DFA, 0x080517D0, 0x0805653E,
   0x0805772C. Confirmado: 0x0804A438 grava 0 no new game (init de 0x0804A2F0). Gravador de "2" no B = 0x08051288; de "1" =
   0x08051382 (sub 4). Os demais: **pendente**.

## 6. Renomeações no Ghidra

- 0x0804765C: `FUN_0804765c` -> `Menu_SetStateByArg01` (confirmado: arg 0 e 1).
- 0x08003374: `FUN_08003374` -> `PartScreen_DispatchSubstate` (confirmado: despacha tabela 0x083AC454).
- Nomes já existentes e confirmados nesta rodada: `Anim_SetDefaultTimers` (0x08044098, faz o retorno de gMainState=1),
  `Gfx_BgStripeStep`, `PartScreen_*`, `Menu_ResetSubstateByMode`, `Gfx_InitBgLayersAndEnableDisplay`. Ver CSV.
