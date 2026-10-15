# MS4: interpretador de roteiro (ctx 0x03000440) — tabelas, handlers e quem usa (2026-10-14)

Continua MS3.md. Textos do jogo nao copiados. Confianca: "confirmado" = decompile ou bytes da ROM;
"provavel" = leitura parcial; "aberto" = nao fechado.

## 1. Tabelas (lidas da ROM com python -I)

- T1 `0x083B2940` (indexada por byte de roteiro, e pelo estado-9): entradas validas 0..42 (43 entradas).
  Entrada 34 = `0x08000841` (sentinela, sem handler). A partir da 43 = dados (nao codigo).
- T2 `0x083B2954` (= T1 + 5 words, indexada por `ctx+0x1B`): entradas validas 0..37 (38). Entrada 29 = sentinela
  `0x08000841`. A partir da 38 = dados.
- Tabela de estados do StateMachine `0x083B291C` (confirmado): estados 0..8. Estado 0..5 = Gfx_PaletteFlushStep,
  MsgState_FadeToneStep, MsgState_BG0OnThenDispatch, Gfx_LoadTileSetStep3, MsgState_ClearBoxAndRunScript,
  ScriptVM_RunOpcodes (estado 5); estado 6 = despachante por byte (0x080408D0); estados 7 e 8 = sentinela.
  Logo, T1 comeca na mesma memoria que os estados 9+ (estado 9 = T1[0]).

## 2. Quem usa o ctx 0x03000440 (confirmado)

`StateMachine_DispatchStep` 0x0803FB3C usa o ctx 0x03000440 como sua struct (estado em +4, ponteiro do roteiro em
+8, pc em +0x0C) e a tabela 0x083B291C. Ha ~80 chamadas (BL) a esse despachante na ROM (ex.: 0x08002736,
0x08004052, 0x08031100, 0x0805CA4C). Ou seja: e um motor generico de cena/roteiro, nao de uma tela so.
Os callers exatos (qual tela chama com qual roteiro) NAO foram separados nesta tarefa: aberto.

## 3. Interpretador de roteiro (confirmado no decompile)

- `ScriptVM_RunOpcodes` 0x0803FD28 (estado 5): le o byte em `roteiro[pc]` (pc = ctx+0x0C, roteiro = ctx+8):
  - 0xF7 espera frames (Script_OpWaitFrames); 0xF8 inverte flag ctx+0x27 e pc+1; 0xF9 quebra/limpa linha de
    texto; 0xFA espera frames ou tecla; 0xFB mostra retrato (Script_OpShowPortrait); 0xFC espera tecla e pc+1 se
    ok; 0xFD espera tecla com linha; 0xFE avanca passo (Script_OpAdvanceStep); 0xFF = escape (ver abaixo).
  - qualquer outro byte (default) = caractere: renderiza glifo (Text_RenderGlyphStep), sem consumir argumento.
  - o loop para ao setar ctx+0x13 (yield por frame).
- Escape `0xFF` = `Script_OpSubcommandOrNextState` 0x08040820 (confirmado): le `roteiro[pc+1]` (subcomando):
  - se subcomando < 5: chama a tabela `PTR` 0x08040884 (entradas 0..4; ver MS.md/MS2.md; nao decompiladas aqui).
  - se subcomando >= 5: `ctx+4 += 1` (estado 5 -> 6). Ai o estado 6 (`ScriptVM_State6_SubcmdDispatch`) chama
    `T1[roteiro[pc+1]]`, ou seja o mesmo byte de subcomando indexa T1.
  - Consumo: o subcomando usa 1 byte de argumento (pc+1). Os handlers de T1 nao leem pc+2 (verificado nos
    corpos lidos abaixo).
- Quem avanca o pc depois de um handler de T1: NAO achei no conjunto lido (nenhum handler de T1 escreve ctx+0x0C).
  Hipotese: o estado 6 volta a 5 quando T2/T1 terminam (SetStateDone grava ctx+4=8) e o pc e avancado em outro
  ponto. Aberto.

## 4. Handlers de T1/T2 (decompile)

| T1 idx | endereco | nome (atual) | o que faz (confirmado) | ctx+0x1B / +0x29 | args |
|---|---|---|---|---|---|
| 0,1,2,3,4 (e 9,10,11,12,13 = estados) | 0x080408F4 .. 0x08040974 | MsgState9..13_CtxB1BDispatch | chama T2[ctx+0x1B] | le +0x1B | 0 |
| 5 e T2[0] | 0x08040994 | MsgJT2_PortraitRectFillStep | move retrato ou preenche retangulo BG0 16px; zera +0x0E/+0x12; +0x1B++ | +0x1B++ | 0 |
| 6 e T2[1] | 0x08040A00 | MsgJT2_FlashRectStep | se +2==1: preenche retangulo 16px e +0x1B+=2; senao +0x1B+=1 | +0x1B | 0 |
| 7 e T2[2] | 0x08040A40 | MsgJT2_LoadTileStageStep | por estagio (+0x26): LZ + tiles 0x06000020, ou retangulo; +0x1B++ ao fim | +0x1B, +0x26 | 0 |
| 8 e T2[3] | 0x08040AF0 | MsgJT2_AdvanceStepByFlag | +0x1B += 1 ou 2 conforme byte +0 | +0x1B | 0 |
| 9 e T2[4] | 0x08040B0C | MsgJT2_FadeStepAndDispatch | fade de paleta (contador +0x1C); ao zerar +0x1B++; jump indireto nao recuperado | +0x1B, +0x1C | 0 |
| 10,16,17,19,20,40,42 | 0x08040B50 | MsgJT2_SetStateDone | ctx+4 = 8 (sentinela: fim do passo) | +4 | 0 |
| 18 | 0x08040B5C | MsgJT2_CopyTiles52Step | 52 CpuSet (0x20 hw) para VRAM 0x06000280; +0x1B++ | +0x1B | 0 |
| 21 | 0x0804169C | MsgT1_LoadTileSetsStep | 3 LZ + LoadTilesWithMalias (0x15/0x16) + CpuFastSet; +0x29++ | +0x29 | 0 |
| 22, 31 | 0x0804177C | BgMap_ClearCopyRowsStep | limpa/copia linhas de tiles; +0x29++ | +0x29 | 0 |
| 23, 30 | 0x08041818 | BgMap_CopyTwoRowsStep | copia 2 linhas de tiles; +0x29++ | +0x29 | 0 |
| 24 | 0x080418C0 | BgMap_CopyRect8x6 | copia retangulo 8x6; +0x29++ | +0x29 | 0 |
| 25 | 0x08041920 | Anim_SpawnSlotObject | cria objeto no slot 0x03002C30 (0x40 B) com posicao de ctx; +0x29++ | +0x29 | 0 |
| 26 | 0x080419CC | Slot_SlideStepClamp | desloca slot +8/+9 e limita a 0x40; +0x29++ ao passar | +0x29 | 0 |
| 27 | 0x08041A28 | Menu_CursorStepHandler | le entrada (0x0300058C), SFX 2/4/5, move cursor | +0x29 | 0 |
| 28 | 0x08041B1C | Fight_UpdateBotFlag4000ByState | liga bit 0x4000 em flag do slot conforme +0x84 | +0x84 | 0 |
| 29 | 0x08041BA4 | Slot_CountdownAndClearTile | decrementa +8/+9 do slot; limpa atributos ao zerar; +0x29++ | +0x29 | 0 |
| 33 | 0x08041BF4 | Slot_SetFrameFromStepAndAdvance | frame = passo*0x10+1; +0x29++ | +0x29 | 0 |
| 32 | 0x0804172C | Gfx_ClearTwoRectsStep | limpa dois retangulos 8x6 via BgMap_FillRect16; +0x29++ | +0x29 | 0 |

Observacoes:
- Todos os handlers de T1/T2 lidos sao "passos" de um contador (ctx+0x1B ou ctx+0x29) e NAO consomem bytes de
  roteiro. Eles executam uma acao por chamada e avancam o contador.
- Os handlers 21..33 usam ctx 0x03000440 como contador +0x29 mas mexem em slots (0x03002C30) e VRAM: sao passos
  de cena/animacao, nao de texto. Os nomes "BgMap_" podem ser mantidos como estao.
- Nao confirmei se o byte 0xFF/T1 (estado 6) so e usado com subcomando >= 5. Os subcomandos 0..4 caem em
  0x08040884 (tabela de FF), outra via.

## 5. Renomeacoes feitas no Ghidra (confirmado pelo corpo)

- 0x080408D0 MsgState_DispatchByScriptByte -> ScriptVM_State6_SubcmdDispatch
- 0x08040A00 BgMap_TickFlashStep -> MsgJT2_FlashRectStep
- 0x08040A40 BgMap_LoadTileStageStep -> MsgJT2_LoadTileStageStep
- 0x08040AF0 Slot_AdvanceStepByFlag -> MsgJT2_AdvanceStepByFlag
- 0x08040B0C Palette_FadeStepAndDispatch -> MsgJT2_FadeStepAndDispatch
- 0x08040B50 Slot_SetTimer8 -> MsgJT2_SetStateDone
- 0x08040B5C Gfx_CpuSetCopy52Blocks -> MsgJT2_CopyTiles52Step
- 0x0804169C Overlay_LoadTileSetsAndAdvance -> MsgT1_LoadTileSetsStep
- 0x08040994 (ja em MS3): BgMap_TickRectFillState -> MsgJT2_PortraitRectFillStep

## 6. Em aberto

- Quem avanca ctx+0x0C (pc) apos um handler de T1 via estado 6 (e como o estado volta a 5).
- Callers do StateMachine (0x0803FB3C, ~80 chamadas): separar quais telas (abertura, creditos, dialogos com
  retrato) usam roteiro com ctx 0x03000440 e em que estado entram.
- Localizacao dos roteiros na ROM: o ponteiro vem de ctx+8, gravado pelo caller; nao achei a tabela de roteiros.
- Tabela PTR 0x08040884 (subcomandos 0..4 do escape 0xFF) nao decompilada nesta tarefa.
- Handlers 10..12 (estados) nao lidos individualmente (provavel igual ao 9).
- Nao testado no emulador.
