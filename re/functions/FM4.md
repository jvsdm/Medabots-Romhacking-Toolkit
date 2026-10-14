# FM4: tela de B no mapa (menu de 4 linhas) e destino de cada linha (GH-FM4, 2026-10-14, PARCIAL)

Leitura: ROM original (só leitura, python -I, helper `rw.py` fora do repo), Ghidra (decompile, nomes do sync já presentes;
uma renomeação). Nada salvo. Não copiei texto do jogo: rótulos das opções vêm de `pc/research/README.md` (MN1, MN2).
Símbolos: `FM4_simbolos.csv`. Continua de FM, FM2, FM3. Confiança: **confirmado** (lido no código/bytes), **provável**, **pendente**.

## 0. Correção de FM3

- O sub 0 de MainState0 menu 1 (0x080027DC) é só o init da tela (REG_DISPCNT, BG0, 5 descompressões, som 6). **Confirmado.**
- A tela de B (gMainState=0, gamestate_menu=1) tem 4 linhas: é esta, não a faixa de 4 linhas do mapa (MainState1 menu 2).
  Ver §2 e §3.

## 1. Tela de B: sub-estados (tabela 0x083AC404, confirmado nos bytes)

| sub | endereço | o que faz (lido) | confiança |
|---|---|---|---|
| 0 | 0x080027DC `MainState0_Menu1_Sub0_InitScreen` | init: REG_DISPCNT=0x1F60, BG0CNT=0x500, descomprime 6 conjuntos de gráficos, snd_song 6, sub+1 | confirmado |
| 1 | 0x080028AC `Screen_RevealStepShowNumber` | revela 4 faixas (BG rows 1/3/5/7) em 0x12 passos; no fim escreve o número (dinheiro) e sub+1 | confirmado (nome do mtbe.csv, de FL10) |
| 2 | 0x08002A2C `Menu_CursorStepAndConfirm` | **controle da tela**: cima/baixo (0x40/0x80 de 0x03001798) muda cursor = ctx+0x8E & 3, som 2; **A** (bit 0 de 0x0300058C) -> sub 6, som 4; **B** (bit 1) -> fade, som 5, sub 3 | confirmado |
| 3 | 0x08002AF8 `Gfx_BgStripeStep` | fecha: desenha faixas do mapa em ordem inversa (0x1F-n), 0x12 passos; **não grava gMainState nem sub** | confirmado (bytes) |
| 4, 5 | 0x08000841 | NoOp | confirmado |
| 6 | 0x08002C34 `Menu_RedrawFrameByState` | desenha o quadro da opção do cursor (ctx+0x8E: 0,1,2,3); sub+1 | confirmado |
| 7 | 0x08002D48 `Gfx_BgRectWipeStep` | wipe por retângulo; ao fim, sub+1 | confirmado |
| 8 | 0x08002DF4 `Gfx_SlideWipeStep` | wipe deslizante; ao fim, sub+1 | confirmado |
| 9 | 0x08002EC4 `MenuB_Sub9_ExecRow` (renomeado, era FUN_08002ec4) | **destino por cursor**, ver §2 | confirmado |

Ponteiros usados: ctx 0x03002C30 (cursor +0x8E, contador +0x84); teclas 0x0300058C (A/B novos); 0x03001798 (cima/baixo).

## 2. Mapeamento linha k (cursor 0..3) -> opção aberta (sub 9, 0x08002EC4)

Sub 9 lê o cursor e faz o destino. Literais lidos da ROM (pool 0x08002F00 e 0x08002F24, 0x08002F60):

| k | ação no código | destino (estado) | handler | confiança |
|---|---|---|---|---|
| 0 | snd_song 7; gamestate_menu = 2 (gMainState fica 0); sub = 0 | MainState0 menu 2 -> `FUN_08003374` (despacha por sub, tabela 0x083AC454) -> sub 0 = **0x08003394** `Gfx_InitBgLayersAndEnableDisplay` | confirmado (código); opção = MEDAWATCH (opção 1 de MN1, subtelas MEDABOTS/MEDALS/MEDAPARTS em MN2) | confirmado |
| 1 | gMainState = 2; gamestate_menu = 4; sub = 0 | MainState2 menu 4 = `Dispatch_StateTableFrameCount` 0x0805CB50 -> `KeyItemBag_InitListScreen` 0x0805CDAC | confirmado (código); opção = ITEM (opção 2, MN1) | confirmado |
| 2 | chama `Menu_SetStateByArg01(1)` 0x0804765C: gMainState = 1; gamestate_menu = 2; sub = 0 | MainState1 menu 2 = `FieldMenu_DispatchSubstate` 0x0805716C -> sub 0 = `Overworld_InitChoiceMenuScreen` 0x0805718C (a faixa de 4 linhas com CheckStepAllowed, FM/FM2/FM3) | confirmado (código); opção = VEÍCULO (opção 3, MN1) | confirmado código; rótulo provável |
| 3 | gPlayTime (0x03000588) = 0; gMainState = 2; gamestate_menu = 5; sub = 0 | MainState2 menu 5 -> tabela 0x08483670 (`Obj_UpdateFrameAndSetAttr` 0x0805D704) -> sub 0 = 0x0805D750 `Screen_ClearBufferAndResetDispcnt` | confirmado código; opção = SALVAR (opção 4, MN1) | provável (ordem + zera tempo de jogo, como um save) |

Cursor inicial: **pendente** (o init de sub 0 não zera ctx+0x8E; ver se outro passo zera antes da entrada).

## 3. Entrada pelo mapa (B)

- Mapa: sub 2 de 0x080510C0, B novo (bit 1 de 0x0300058C) com E+0xC33 != 0 -> grava gMainState = 0, gamestate_menu = 1,
  gamestate_subroutine = 0, E+0xCBA = 2 (FM2 §1, confirmado).
- Assim, B abre a tela de 4 linhas (§1-2), com gamestate_menu = 1 de gMainState 0 -> menu_list[1] 0x080027BC (wrapper, FM3).
- Os handlers de MainState0 menu 1 ficam em 0x083AC404 (12 entradas; 4, 5, 10, 11 = NoOp; úteis: 0,1,2,3,6,7,8,9).

## 4. Saídas

- Para a overlay de VEÍCULO (k=2): MainState1 menu 2 (§2). Sua saída já era conhecida (FM2 §3: sub 1 do menu de escolha
  e as saídas 0x0D/0x0F com 0x0804A210).
- Para ITEM (k=1) e SALVAR (k=3) e MEDAWATCH (k=0): não fechadas aqui (ver pendências).
- **B na tela (sub 2 -> sub 3)**: o sub 3 (0x08002AF8) fecha com faixas mas **não escreve gMainState nem gamestate_menu**
  (disasm completo, linhas 0x08002AF8..0x08002C1A). Nenhum escritor de gMainState=1/menu=1 foi achado no caminho.
  **Pendente e importante**: ou o retorno ao mapa passa por algum lugar que não li (provável: MainState0 dispatch com
  E+0xCBA=2), ou a função está incompleta no Ghidra. Próxima rodada: ler 0x0804A220 (EventScript_RunCurrent) e o uso de
  E+0xCBA (=2 no B, =1 ao voltar, FM2 sub 4).

## 5. Pendências

1. Saída da tela de B (sub 3 não avança; quem grava gMainState=1 ao fechar). Ver §4.
2. Rótulos das linhas da tela de B: as faixas vêm de imagens (tabela 0x08002A08 de ponteiros de tiles), não de texto; a
   correspondência k -> rótulo é por ordem (MN1) e por destino (§2), não por leitura do texto.
3. Overlay de VEÍCULO (MainState1 menu 2): as 4 linhas (k=0..3, código de CheckStepAllowed) têm o mesmo problema de ambiguidade
   de FM3 (código 1 = "item ausente" e também "faixa ativa"). Não resolvi aqui.
4. Tela de SALVAR (MainState2 menu 5): a tabela 0x08483670 tem 6 entradas; só sub 0 foi visto.
5. 0x0804765C (Menu_SetStateByArg01): o nome do sync é chute; o ramo arg=1 é confirmado (k=2), o ramo arg=0 não lido.

## 6. Renomeações no Ghidra

- 0x08002EC4: FUN_08002ec4 -> `MenuB_Sub9_ExecRow` (confirmado: destinos por cursor).
- Os demais nomes desta tela já vinham do sync (`MainState0_Menu1_Sub0_InitScreen`, `Menu_CursorStepAndConfirm`,
  `Menu_RedrawFrameByState`, `Gfx_BgStripeStep`, `Gfx_BgRectWipeStep`, `Gfx_SlideWipeStep`, `FieldMenu_DispatchSubstate`).
  Não renomeei o restante: sem confirmação suficiente.
