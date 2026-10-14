# FM6: tela de SALVAR (MainState2 menu 5), MEDAWATCH (gMainState 0) e VM de eventos do mapa

Rodada 2026-10-14, Ghidra (decompile + ponteiros lidos da ROM com python -I). Nada de emulador.
Ponteiros de GS: `0x0300137C` = gamestate_subroutine (GS). Ponteiro do contexto da tela de salvar: `0x03006F74` (+2 = contador).

## 1. Tela de SALVAR: tabela de subs 0x08483670 (MainState2, menu 5)

Despachante: `MainState2_Menu5_Dispatch` 0x0805D704 (FM4), `CallViaR0(tabela[GS])`. A tabela tem 14 entradas (Thumb).
Entrada 13 = 0x0805DB20. Entradas 10 e 11 (0x0805DCB0, 0x0805DCCC) e 0x0D: **não lidas nesta rodada** (pendente).

| sub | endereço | nome | o que faz (lido) | confiança |
|---|---|---|---|---|
| 0 | 0x0805D750 | `Screen_ClearBufferAndResetDispcnt` | zera buffer por CpuFastSet, limpa 4 pares, DISPCNT=0x160; GS++ | confirmado |
| 1 | 0x0805D790 | `Screen_InitLZ77TileBlocks` | carrega blocos de tile LZ77 (3 blocos), descomprime mapa e paleta, configura BG; GS++ | confirmado (GS++); detalhe dos blocos: provável |
| 2 | 0x0805D860 | `Stats_DrawSummaryScreen` | desenha o RESUMO: (a) medalhas = conta bit 0x80 em 30 registros de 0x40 B a partir de 0x03000BE0; (b) peças = conta os 480 u16 não-zero de 0x03004C40; (c) tempo = `(u32 0x03002B70+0x18)` → `/3600` e depois `/60` e `%60` (unidade do campo: provável, README já dizia "m:ss GUESS"); (d) dois números em +0x14 e +0x16 de 0x03002B70 (provável WINS/TOTAL); (e) dinheiro = u32 em 0x03002B70 formatado; depois `Fx_InitRecord8`; GS++ | confirmado (o que conta); rótulos: provável |
| 3 | 0x0805DA14 | `Gfx_FadeInStepAndEnableBg` | fade de paleta em 9 passos (ctx 0x03000440+0x84); no 1º passo liga BG (DISPCNT bits 9..12); ao chegar a 9 chama `Obj_InitSlotAndLoad(1,2,0)`; GS++ | confirmado |
| 4 | 0x0805DAB0 | `Flow_StepDispatchCounter` | `StateMachine_DispatchStep` sobre a estrutura ROM 0x0848521C (+0x24); quando termina, GS++ | confirmado (estrutura: ponteiro lido) |
| 5 | 0x0805DAD4 | `StateTable_JumpOnDone_6Or9Or13` | `StateTable_DispatchByIndex`; ao terminar: se ctx+0x2A == 0 → GS = 6, ou 13 se bit0 de 0x03001784 estiver ligado; senão GS = 9 | confirmado |
| 6 | 0x0805DB84 | `Save_StepThenFlashOp` | `StateMachine_DispatchStep` sobre ROM 0x0848521C (+0x28); ao terminar: `snd_song_request(0)`, `snd_sfx_request(0)`, `Save_InvokeFlashOp`, ctx+2 = 0, GS++ (→ 7) | confirmado |
| 7 | 0x0805DBC4 | `SaveLoad_PromptStep` (antes: `Site_SaveLoadPromptStep`, NM2) | ver §1.1 | confirmado |

Sub 8..9 e 0xC (para não perder o fio do salvar): sub 8 = 0x0805DC4C `StateMachine_TickWithFrameTimer` (espera > 60 quadros, GS++ → 9); sub 9 = 0x0805DC88 `Screen_ResetPalettesAndAdvance` (limpa estado, copia paletas, GS++ → 10); sub 0xC = 0x0805DD50 `Menu_TickDispatchWithTimeout` (espera > 60 quadros, depois GS = 9).

### 1.1 Onde entra `SaveLoad_RunOption` (= Ghidra `Site_RunSaveLoadOption`, 0x0805DDED)

- Chamada no **passo 7** (`SaveLoad_PromptStep`, 0x0805DBFC): depois que o contador passa de 0x1D quadros e o SFX 4 está livre, chama `SaveLoad_RunOption(0)` até 4 vezes (para na primeira vez que devolve 0). Toca música 6.
  - Retorno 0 → ctx+2 = 0 e **GS = 8** (sucesso).
  - Retorno ≠ 0 nas 4 tentativas → ctx+2 = 0 e **GS = 0xC** (falha; sub 0xC mostra e volta a GS=9).
- Outros chamadores (xref de 0x0805DDED): `Save_ClearAndRetryLoad` (0x0805DF04, arg não verificado), `Save_RunOptionsThenBlendFade` 0x08066574 (arg 0, LK3), `Menu_SaveLoadWaitStep` 0x0805FA58 (arg 2, FL4), `Sm_StepSfxRepeat3` (0x080659D2, arg não verificado), FN3 e FR2 (arg 0, notas antigas).

### 1.2 Corpo de `SaveLoad_RunOption(arg)` (decompile, lido)

- Antes de tudo: zera um u32 (bios_cpuset), zera um byte em 0x0805DE2C/puVar3 e grava 1, troca um halfword (guardado em `uVar1`) e no fim faz `|= uVar1` em DAT_0805DEAC.
- **arg 0 (salvar)**: seta bit 0 do flag em DAT_0805DE50; chama `SaveLoad_SaveGame(buf)` (0x0805E010); depois `Save_WriteBlocksEeprom(DAT_0805DEA8, 0)`; esse valor é o retorno.
- **arg 1 (carregar)**: `Save_ReadVerify(buf)` e `SaveLoad_LoadGame(buf)`; retorna o `Save_ReadVerify`. Não grava EEPROM.
- **arg 2**: limpa o bit 0 de DAT_0805DEA0, **copia 16 bytes** de DAT_0805DEA4 para o buffer (não "limpa" como dizia NM2; ver divergência) e depois `Save_WriteBlocksEeprom(DAT_0805DEA8, 0)`.
- Ou seja: 0 = salvar grava as EEPROM blocks; o retorno do `Save_WriteBlocksEeprom` é o que decide sub 8 (ok) ou sub 0xC (falha).

## 2. MEDAWATCH: o que é cada menu de gMainState 0

Tabela `gMenuList0Table` = 0x083AC2C8 (gMainState 0, indexada por `gamestate_menu` em 0x03002B8C). Entradas lidas (Thumb, bit 0 retirado):

| índice (menu) | função despachante | tabela de subs | nota |
|---|---|---|---|
| 2 | 0x08003374 `PartScreen_DispatchSubstate` | 0x083AC454 | MEDAWATCH (FM5, confirmado) |
| 3 | 0x080039E0 `Scene_FrameTickSubstateDispatch` | 0x083AC4A0 | tela de MEDABOTS (pods) — provável |
| 4 | 0x08007584 `PartsGrid_DispatchStateTable` | 0x083AC86C | grade com cursor de células, `PartsGrid_SceneInit` (0x080075AC) = sub 0 — provável MEDALS |
| 5 | 0x0800D9DC `Menu5_Dispatch_PartsScreens` | 0x083ACCD4 | grade de 4 peças + páginas `k/N`, `MedabotGrid_InitScreen` (0x0800DA34) = sub 0; passo 66 = detalhe de peça (0x0800EB34, MU) — provável MEDAPARTS |

Despachantes de menu 3, 4 e 5 usam `Battle_CheckExitShortcut()` antes do passo (confirmado no decompile); o 3 também copia 0x200 halfwords por DMA a cada quadro quando `*0x03000430 == 2`.

Mapeamento do MEDAWATCH (Menu_ResetSubstateByMode, FM5, confirmado: cursor em ctx+0x8F = 0x03000440+0x8F):
- cursor 0 → menu **3** = MEDABOTS (pods + bot grande + nomes/medalhas): **provável**.
- cursor 1 → menu **4** = MEDALS (grade de medalhas, Lv/estatísticas no painel): **provável**.
- cursor 2 → menu **5** = MEDAPARTS (páginas k/N, quatro ícones de peça): **provável**.

Evidência usada: (a) a ordem MN2 (MEDABOTS/MEDALS/MEDAPARTS) bate com a ordem do cursor; (b) menu 3 monta 9 sprites de slot (`BattlePreview_CreateSlotSprites9`) e um bot composto grande (`BattlePreview_BuildLargeCompositeLayer5`), que é o que MB1 descreve para os pods; (c) menu 5 mostra número de página (`Num_DrawDecimal6BigTiles(página+1, …)`) e quatro ícones de peça; (d) menu 4 conta 30 entradas (`PartsGrid_CountFlagged30`, nome herdado de notas antigas) — 30 é o número de medalhas.

Nomes herdados a revisar: `InitializeBattleStatusScreen` (0x08003A38, sub 0 de menu 3) tem nome de "status de batalha" (GF2/GF3), mas o conteúdo é a tela de pods do MEDABOTS (**provável**). Não renomeei no Ghidra.

## 3. VM de eventos do mapa: `EventScript_RunCurrent` (0x0804A220, MainState1[1])

Decompile lido. Passos por quadro:
1. Contador de quadros (`0x03005310` + 4; é o 0x03005314 de EV.md) +1.
2. Se **E+0xC88 == 0** (sem evento pendente): `CallViaR0(gEventSubTable[GS])`, tabela **0x083D212C** (a mesma tabela aparece como ponteiro nas duas chamadas).
3. Se E+0xC88 ≠ 0: se E+0xC89 == 0 → `Script_DispatchHandler` (0x0804B6F4, fase primária); senão → `Event_DispatchContinuation`. Depois, se **E+0xC5F ≠ 0**, `CallViaR0(gEventSubTable[GS])` de novo.
4. Depois sempre: `Map_UpdateVisibleEventObjectSprites`, `Map_TickAllObjectAnimations`, `Map_UpdateBackgroundAnimations`, 32 × `Map_RecomputeRenderAffineEntry`, `Map_UpdateEventObjectDepthAndMarkerState`, cópia de 8 linhas por objeto.

**E+0xC88 = 0x03006468 = gCurrentEventPendingFlag** (EV.md, confirmado). **E+0xC89 = 0x03006469** = gCurrentEventStateByte (0 = fase primária). **E+0xC5F = 0x0300643F** = flag "rodar passo da cena também neste quadro" (EV.md, confirmado). E = 0x030057E0.

Tabela 0x083D212C (gEventSubTable, indexada por GS; entradas Thumb):

| sub | endereço | nome | o que faz (lido) | confiança |
|---|---|---|---|---|
| 0 | 0x0804A2F0 | (init de New Game, FM/Ghidra) | não relido | herdado |
| 5 | 0x0804A6E0 | `Map_TickSceneStateMachine` | só chama `Overworld_UpdateMapSceneStateMachine` (não muda GS aqui) | confirmado |
| 7 | 0x0804A6F8 | `Map_ResetOamAndRenderSlotsStep` | limpa OAM shadow por DMA, `Overworld_RebuildActiveObjectRenderSlots`, `SND_PolicyRequest(5,0)`, zera byte de ctx (0x03005311 +1), **GS = 5** (volta ao sub 5) | confirmado |
| 13 (0x0D) | 0x0804A944 | `Overworld_ReloadMapSceneAfterReset` | zera VRAM/OAM (set_zeros 0x2000 e 0x200), DISPCNT = 0x1E40, carrega mapa atual (`Map_LoadCurrentMapAndEntities`), zera flag de encontro, `SND_PolicyRequest(0xE,0)`, clamp da câmera, reset do blend, `Overworld_StartScreenTransitionEffect`, **GS++ (→ 0x0E)** | confirmado |

Divergência com FM5 §5 (pendência 2): o sub 0x0D **não** chama `Menu_SetStateByArg01(0)`; ele recarrega o mapa com transição e avança GS. Quem grava GS = 0x0D continua **pendente**.

Pendência de FM5 (sub 7 = "retomada do mapa"): **confirmado**: sub 7 (0x0804A6F8) limpa OAM, reconstrói slots e volta a GS = 5 (sub 5, tick da cena do mapa).

## 4. Pendências (próxima rodada)

1. Tela de SALVAR: subs 0x0A (0x0805DCB0), 0x0B (0x0805DCCC) e 0x0D (0x0805DB20) do 0x08483670. Confirmar `Save_InvokeFlashOp` (CH3 dá 0x0805DEB0, não lido aqui) e o destino de `Save_ClearAndRetryLoad` (0x0805DF04), que chama `SaveLoad_RunOption` com arg não verificado.
2. MEDAWATCH: confirmar por texto/tile que menu 3 = MEDABOTS, menu 4 = MEDALS e menu 5 = MEDAPARTS (hoje **provável**). Ler `PartsGrid_CountFlagged30` para ver o que conta.
3. Quem grava GS = 0x0D e GS = 5 do mapa (xref de 0x0300137C ligado ao 0x0804A6F8 e 0x0804A944).
4. Sub 0x0D de EventScript_RunCurrent: quem o seta (ver pendência 3).
5. `Stats_DrawSummaryScreen`: confirmar a unidade do campo de tempo (0x03002B70+0x18) em um save real.

Arquivos: `FM6_simbolos.csv` no mesmo diretório.
