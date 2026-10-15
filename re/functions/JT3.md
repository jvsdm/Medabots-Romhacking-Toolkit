# Despachantes por tabela (GH-JT3, 2026-10-13)

Itens da tarefa GH-JT3 (HANDOFF_DISCOVERY). Método: `disassemble_function` do despachante, literal da tabela
no pool (lido na ROM), índice (byte/halfword/word da RAM), depois a tabela lida da ROM original (Python, só leitura).
Alvo sem bit thumb quando a tabela é de ponteiros de código; "thumb" = bit 0 setado na ROM.

Dois padrões de chamada, não de salto:
- `bl 0x0807CCF0` (`CallViaR0`, trampolim `bx r0`) e `bl 0x08040BD8` (`Anim_RunSlotScript`): o despachante
  carrega o ponteiro e chama; retorna pelo `pop {r0}; bx r0` normal. Não é tail-call de tabela.

## 0x08065ED0 `Screen_DispatchByState3A` (duas tabelas, salto real em 0x08065EF4 e 0x080660A0)

- Flag 0x03000B90 (literal 0x08065EF8): se 0 vai para o ramo B (0x08066088); senão ramo A.
- Índice nos dois ramos: halfword em `0x03006F74 + 0x3A` (literal 0x08065EFC / 0x080660A4). Índice > 5 sai.
- Ramo A: tabela em **0x08065F04** (ponteiro no literal 0x08065F00), 6 entradas.
- Ramo B: tabela em **0x080660AC** (ponteiro no literal 0x080660A8), 6 entradas.
- Nenhum alvo é função no Ghidra: são casos internos do corpo de 0x08065ED0 (corpo vai até 0x0806622D).

| Ramo | Índice | Alvo | Resumo (miolo) |
|---|---|---|---|
| A | 0 | 0x08065F1C | chama 0x080440B4; grava byte +0x7B (0 ou 0xFF); halfword +6; avança passo se +0xA bate |
| A | 1 | 0x08065F74 | variação de A0 com sinal de +0xA/+0xE |
| A | 2 | 0x08065FB8 | grava +6; avança passo se +0xA bate (sem chamada) |
| A | 3 | 0x08065FD4 | chama 0x080440D4; grava byte +0x7C; avança passo |
| A | 4 | 0x0806602C | variação de A3 com sinal de +0xA |
| A | 5 | 0x08066070 | grava +6; se +0xA bate, vai para 0x08066220 (contador global 0x0806623C += 1) |
| B | 0 | 0x080660C4 | byte +0x7B pelo sinal de +0xE; avança passo se +0xE bate |
| B | 1 | 0x08066104 | chama 0x080440B4; grava +0x7B; avança se +0xE bate |
| B | 2 | 0x08066134 | grava +6 e +0xE; avança se +0xA e +0xE batem |
| B | 3 | 0x08066164 | byte +0x7C pelo sinal de +0xE; avança se +0xE bate |
| B | 4 | 0x080661A4 | chama 0x080440D4; grava +0x7C; avança se +0xE bate |
| B | 5 | 0x08066200 | fim: se +0xA e +0xE batem, grava +6; contador 0x0806623C += 1 |

Obs.: "avança passo" = halfword +0x3A += 1 (0x080661E8). Descrição de A/B é de leitura do disassembly, não decompilada.

## Tabelas do byte 0x03000473 (ctx 0x03000440 + 0x33) — `Dispatch_StateHandlerTable` e irmãs

Os três despachantes usam o MESMO byte (literais 0x0800BAF4, 0x0800BE88, 0x0800D184 = 0x03000440; +0x33 nos
três). Cada um começa em um ponto diferente da mesma tabela de 34 ponteiros em **0x083ACAC0**:
- `0x0800BAD8` Dispatch_StateHandlerTable: base 0x083ACAC0 (literal 0x0800BAF0), 34 entradas (idx 0..33).
- `0x0800BE6C` Dispatch_CallHandlerByByte33: base 0x083ACACC (= BAD8 + 3 entradas), 31 entradas (idx 0..30).
- `0x0800D168` Dispatch_StateHandlerTable_B: base 0x083ACB14 (= BAD8 + 21 entradas), 13 entradas (idx 0..12).
O fim de cada tabela é o primeiro valor que não é ponteiro (0x083ACB48.. aponta para dados; entradas 34+ de
BAD8 são texto/dados). Os índices de 0..33 cobrem as três; índice maior que o fim não tem bound check (lê dado).

Entradas de BAD8 (índice: alvo, nome atual no Ghidra):

| Idx | Alvo | Nome |
|---|---|---|
| 0 | 0x0800BAF8 | debug_process |
| 1,2,4,5,7,8,10,11,13,14,16,17,19,20,22,23,25,26,28,29 | 0x08000840 | NoOpActionHandler (vazio) |
| 3 | 0x0800BE8C | Debug_PartsSpawnScreen |
| 6 | 0x0800C2BC | Menu_TickCountEditorAndGrant |
| 9 | 0x0800C6F8 | PartsBuild_UpdateStatusPanel |
| 12 | 0x0800C804 | Shop_HandleItemListAndCounts |
| 15 | 0x0800CBC8 | Menu_UpdateDetailPage |
| 18 | 0x0800CE90 | Menu_InfoPanelStep |
| 21 | 0x0800D188 | Hud_DrawLabelAndDecimal6 |
| 24 | 0x0800D27C | Menu_TickSingleCountSelector |
| 27 | 0x0800D3E4 | PartsBuild_UpdateEditPanel |
| 30 | 0x0800D684 | função desde o sync de 2026-10-14 (nome: Menu_ClearObjectAttrsAndAdvance) (miolo; fora de 0x0800D3E4 e de 0x0800D6A8) |
| 31 | 0x0800D6A8 | FightPreview_BuildLayers |
| 32 | 0x0800D710 | DebugMotion_Main |
| 33 | 0x0800D90C | DebugMotion_PlayKind |

Os 0x08000840 são o handler vazio (NoOpActionHandler), usados como "sem ação" nos slots intermediários.
Função de chamada: `CallViaR0` 0x0807CCF0. As três tabelas são de menu de DEBUG (nomes já vindos do debug build).

## 0x0805CB50 `Dispatch_StateTableFrameCount` (tabela 0x08483634, salto indireto via CallViaR0)

- Índice: **word** em 0x0300137C (literal 0x0805CB84). Sem bound check.
- Tabela: 0x08483634 (literal 0x0805CB80), **30 entradas** (idx 0..29). Entrada 30 em diante é texto/dados (lido).
- Chamada: `bl 0x0807CCF0` (CallViaR0) com o alvo. Depois: se o bit 0 do word apontado pelo literal 0x0805CB88 estiver
  setado, incrementa os halfwords apontados por 0x0805CB8C (+0 e +2).
- Mesmo índice 0x0300137C usado por 0x0806463C e 0x08068400 (ver abaixo).

| Idx | Alvo | Nome / papel |
|---|---|---|
| 0 | 0x0805CBFC | Screen_ResetLayersAndCounter |
| 1 | 0x0805CC3C | Screen_InitSpritesAndLoadTiles |
| 2 | 0x0805CDAC | Shop_InitItemListScreen |
| 3 | 0x0805CED0 | Screen_FadeStepThenLoadObj |
| 4 | 0x0805CF58 | Shop_UpdateSlotAndDispatchState |
| 5 | 0x0805CFB4 | Shop_ItemListInputStep |
| 6 | 0x0805D4A0 | Slot_ActivateCursor |
| 7 | 0x0805D4E0 | função desde o sync de 2026-10-14 (nome: StateTable_WaitSubThenAdvance) (miolo; em JT3_lista) |
| 8 | 0x0805D4FC | Gfx_FadeOutThenClear |
| 9 | 0x0805D580 | FUN_0805d580 (renomear não feito; ver corpo, 0x19 bytes) |
| 10 | 0x0805D5A4 | Menu_ResetOnInput |
| 11 | 0x0805D5D0 | função desde o sync de 2026-10-14 (nome: Fx_SetBldcntAndAdvance) (miolo; em JT3_lista) |
| 12 | 0x0805D5F0 | Screen_StepToBlank |
| 13 | 0x0805D658 | função desde o sync de 2026-10-14 (nome: StateTable_WaitSubThenAdvance_B) (miolo; em JT3_lista) |
| 14 | 0x0805D674 | Screen_TickFadeOutAndClear |
| 15 | 0x0805D750 | Screen_ClearBufferAndResetDispcnt |
| 16 | 0x0805D790 | Screen_InitLZ77TileBlocks |
| 17 | 0x0805D860 | Stats_DrawSummaryScreen |
| 18 | 0x0805DA14 | Gfx_FadeInStepAndEnableBg |
| 19 | 0x0805DAB0 | Flow_StepDispatchCounter |
| 20 | 0x0805DAD4 | função desde o sync de 2026-10-14 (nome: StateTable_JumpOnDone_6Or9Or13) (miolo; em JT3_lista) |
| 21 | 0x0805DB84 | Save_StepThenFlashOp |
| 22 | 0x0805DBC4 | Site_SaveLoadPromptStep |
| 23 | 0x0805DC4C | StateMachine_TickWithFrameTimer |
| 24 | 0x0805DC88 | função desde o sync de 2026-10-14 (nome: Screen_ResetPalettesAndAdvance) (miolo; em JT3_lista) |
| 25 | 0x0805DCB0 | função desde o sync de 2026-10-14 (nome: StateTable_WaitSubThenAdvance_C) (miolo; em JT3_lista) |
| 26 | 0x0805DCCC | Palette_FadeStepThenClear |
| 27 | 0x0805DD50 | Menu_TickDispatchWithTimeout |
| 28 | 0x0805DB20 | FUN_0805db20: se 0x0803FB3C(ptr+0x48) != 0, chama 0x080415E8(1,0x16,7) e conta. Nome não trocado |
| 29 | 0x0805DB4C | função desde o sync de 2026-10-14 (nome: StateTable_JumpOnDone_6Or9) (miolo; em JT3_lista) |

Obs.: os alvos sem função ficam como casos sem nome; ler o ROM ou o Ghidra por faixa se for preciso.

## 0x0803C680 `Anim_SlotScriptTick` (tabela de ponteiros de DADOS em 0x083B6100; chama 0x08040BD8)

- Não é salto: entrada de tabela é argumento de `Anim_RunSlotScript` (0x08040BD8), que roda o bytecode de animação.
- Seleção em dois níveis: slot = byte em 0x0300056F (= 0x03000440 + 0x12F; literais 0x0803C6A4/A8) -> slot
  `0x03002C30 + slot*0x40`; entrada = byte em slot+0x2D (literal 0x0803C6EC aponta para 0x083B6100). Ponteiro lsl 2.
- Tabela 0x083B6100: ponteiros para registros de script (bytes tipo `06 01 00 06 01 01 ...`, lidos no ROM em
  0x083B55D4). Entradas válidas até pelo menos o índice 79 (todas na faixa 0x083B55xx..0x083B5Dxx); o fim não é
  determinado porque o índice é byte (até 255) sem bound check. Não há alvo de código a listar.
- Chamada 0x08040BD8 no `bl` em 0x0803C6CA (com r0 = ponteiro de script). Ramo "fim" em 0x0803C6FC chama 0x0804422C.

## 0x0806463C `Dispatch_StateTableWithCopy` (cópia de 12 palavras da ROM 0x080AFC18 para a pilha)

- Fonte da cópia: ponteiro em 0x080646A4 (= 0x080AFC18). Copia **12 palavras** (4 x ldmia/stmia de 3; 0x30 bytes).
  O frame tem 0x34 bytes: a palavra em sp+0x30 NÃO é copiada da ROM.
- Índice: word em 0x0300137C (literal 0x080646B8). Sem bound check: índice > 12 lê a pilha.
- `bl 0x0807CCF0` (CallViaR0) em 0x0806467C com `sp[idx*4]`. Índice 12 lê sp+0x30 (lixo de pilha, ou 0x200 no ramo abaixo).
- Ramo especial: se `[0x03000430] == 2` (literal 0x080646A8), grava 0x200 em sp+0x30 e chama CpuFastSet
  0x0807CCC0 (fonte = sp+0x30, modo fill). Com idx 12 e 0x03000430==2 o alvo lido vira 0x00000200.
  Não confirmado em jogo; registrado como armadilha.
- Depois: se bit 0 do word em 0x080646BC (literal) setado, incrementa halfwords 0x080646C0 +0 e +2.

| Idx | Alvo | Nome / papel |
|---|---|---|
| 0 | 0x080639C4 | função desde o sync de 2026-10-14 (nome: Screen_InitNumberRevealLayout) (miolo; em JT3_lista) |
| 1 | 0x08063EBC | Anim_FinishClearAndLoadSlot |
| 2 | 0x08063F08 | Menu_CompareSelectionAndAdvance |
| 3 | 0x08063FB0 | Sprite_ScriptRecordStep |
| 4 | 0x080640DC | função desde o sync de 2026-10-14 (nome: StateTable_AdvanceIndex) (miolo; em JT3_lista) |
| 5 | 0x080640EC | Anim_ScrollTileRowsThenCountdown |
| 6 | 0x0806412C | Fx_NumberRevealStep |
| 7 | 0x0806445C | função desde o sync de 2026-10-14 (nome: Anim_ScrollCountdownSlide) (miolo; em JT3_lista) |
| 8 | 0x080644D8 | função desde o sync de 2026-10-14 (nome: Anim_ScrollThenLoadSlotAndSfx) (miolo; em JT3_lista) |
| 9 | 0x08064524 | Flow_StepSfxCommit |
| 10 | 0x080645A0 | Obj_StepThenTimerOnMatch |
| 11 | 0x08064610 | Anim_WaitCounterThenAdvance |
| 12 | (não copiado) | sp+0x30 (lixo ou 0x200). A entrada 12 da ROM (0x080646C4) NÃO é usada por este despachante |

## 0x08068400 `Dispatch_CallHandlerLocalCopy` (cópia de 7 palavras da ROM 0x080B003C para a pilha)

- Fonte: ponteiro em 0x08068434 (= 0x080B003C), 7 palavras em sp. Índice: word em 0x0300137C (literal 0x08068438).
- `bl 0x0807CCF0` (CallViaR0) em 0x0806841E; depois incrementa halfword em 0x0806843C +2 (contador).
- Sem bound check (índice > 6 lê pilha); a tabela tem exatamente 7 entradas (entrada 7 = dados).

| Idx | Alvo | Nome / papel |
|---|---|---|
| 0 | 0x08067D00 | Screen_LoadSceneWithSong |
| 1 | 0x08067F64 | PartsInfo_InitScreen |
| 2 | 0x08068238 | função desde o sync de 2026-10-14 (nome: Fx_WhiteFadeStepThenLoadSlot) (miolo; em JT3_lista) |
| 3 | 0x08068298 | função desde o sync de 2026-10-14 (nome: Fx_BlendStepThenLoadMedalArt) (miolo; em JT3_lista) |
| 4 | 0x08068328 | função desde o sync de 2026-10-14 (nome: Fx_FadeInStepThenAdvance) (miolo; em JT3_lista) |
| 5 | 0x08068374 | Fx_BlendAndStateStep |
| 6 | 0x080683C8 | Anim_WaitRecordThenSetState |

## 0x08065ED0 — casos sem função (ramos A e B)

Os doze alvos A0..A5 e B0..B5 não são funções no Ghidra: são casos dentro do corpo de 0x08065ED0 (0x08065ED0..0x0806622D).
Resumo por caso em "0x08065ED0" acima. Lista completa com descrição: `JT3_lista.txt`.

## Não-despachantes

- `0x08068BC4` (Fx_PeriodicSineEmitStep): o `pop {r1}; bx r1` final é retorno. Não há tabela de salto; a tabela
  halfword apontada por 0x08068CA0 (tabela não lida; pelo nome, provavelmente seno) indexada por divisão com sinal (0x0807CD2C).
- `0x0800BAD8` e irmãs: o `bx` é retorno, o salto é o `bl CallViaR0` (ver seção de tabelas do byte 0x03000473).

## Pendências

- 0x0805DB20 (FUN_0805db20) segue sem nome: não ficou claro o papel de 0x0803FB3C e 0x080415E8.
- Entradas de `JT3_lista.txt` (eram sem função; desde o sync de 2026-10-14 são funções): 0x0800D684, 0x0805D4E0, 0x0805D5D0, 0x0805D658, 0x0805DAD4,
  0x0805DC88, 0x0805DCB0, 0x0805DB4C, 0x080639C4, 0x080640DC, 0x0806445C, 0x080644D8, 0x08068238, 0x08068298,
  0x08068328. Descrição não feita (sem disassembly por função; a leitura em bloco no ROM fica para depois).
- Tamanho da tabela 0x083ACAC0 (34 é o fim lido, sem bound check em nenhum dos três despachantes). Os índices
  que passam do fim leem dado (não confirmado em jogo se acontece).
- Tabela 0x083B6100 (scripts): fim não determinado; índice é byte de slot+0x2D.
- Tabelas 0x08483634, 0x080AFC18 e 0x080B003C dependem de 0x0300137C (word), sem bound check.
- Os nomes do CSV de casos A/B e do alvo 0x0800D684 são propostos (kind=label), não aplicados no Ghidra.
