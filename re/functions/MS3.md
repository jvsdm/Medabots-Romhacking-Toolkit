# MS3: despachantes de byte e de ctx+0x1B do motor de mensagens (2026-10-14)

Continua MS.md. Escopo: tabela de dispatch, handlers 9..22, o que cada um faz. Textos do jogo nao copiados.
Confianca: "confirmado" = decompile ou literal/bytes da ROM; "provavel" = leitura parcial; "aberto" = nao fechado.

## 1. Literais confirmados (ROM e decompile)

- `DAT_080408EC` = `0x083B2940`: base da tabela de handlers indexada por BYTE DO ROTEIRO (confirmado).
- `DAT_080408F0` = `0x03000440`: ctx da mensagem (confirmado).
- `DAT_0804090C` = `0x083B2954`: base da segunda tabela (JT2), indexada por `ctx+0x1B` (confirmado).
  ATENCAO: MS.md dizia JT2 = `0x083B2990`. Esse e o indice 20 da tabela de bytes, nao a base. A base correta e
  `0x083B2954` (= `0x083B2940 + 0x14`).
- `0x083B291C` (estados 0..11): a entrada 9 esta em `0x083B2940` (= tabela de bytes indice 0). Logo, estado 9..22 e
  a mesma memoria que a tabela de bytes. Estado 9 = `0x080408F4` = MsgState9_CtxB1BDispatch.

## 2. Despachante por byte: MsgState_DispatchByScriptByte 0x080408D0 (confirmado no decompile)

`CallViaR0( tab[ byte ] )` com `byte = roteiro[ ctx+0x0C (pc) + ctx+0x08 (ponteiro) + 1 ]`, indice = byte*4 sobre
`0x083B2940`. Ou seja: o byte de controle logo depois do byte de despacho e o indice. Nao ha checagem de limite no
decompile (provavel: a faixa valida e pequena; entradas 34 = sentinela `0x08000841`, 43..47 ja sao dados).

Entradas unicas da tabela de bytes (indices 0..42, sem contar dados):
`0x080408F5 / 0x08040915 / 0x08040935 / 0x08040955 / 0x08040975 / 0x08040995 / 0x08040A01 / 0x08040A41 /
0x08040AF1 / 0x08040B0D / 0x08040B51 / 0x08040B5D / 0x0804169D / 0x0804177D / 0x08041819 / 0x080418C1 /
0x08041921 / 0x080419CD / 0x08041A29 / 0x08041B1D / 0x08041BA5 / 0x08041BF5 / 0x0804172D`

## 3. Handlers 9..22 (estado e entrada)

| indice | endereco | nome Ghidra (atual) | o que faz (confirmado/provavel) | confianca |
|---|---|---|---|---|
| 9 | 0x080408F4 | MsgState9_CtxB1BDispatch | despacha `JT2[ctx+0x1B]` (confirmado) | confirmado |
| 10 | 0x08040914 | MsgState10_CtxB1BDispatch | igual ao 9 (nome existente; corpo nao lido) | provavel |
| 11 | 0x08040934 | MsgState11_CtxB1BDispatch | igual | provavel |
| 12 | 0x08040954 | MsgState12_CtxB1BDispatch | igual | provavel |
| 13 | 0x08040974 | MsgState13_CtxB1BDispatch | igual; MS.md ja confirmava ligacao a JT2 | confirmado |
| 14, 20 | 0x08040994 | MsgJT2_PortraitRectFillStep (antes: BgMap_TickRectFillState) | le ctx 0x03000440: se flag +1 != 2 e +0x12 == 1, move retrato (Move_Character_Portraits) ou preenche retangulo 16 px (BgMap_FillRect16, BG0CNT 0x04000008); zera +0x0E/+0x12; depois `ctx+0x1B++` | confirmado |
| 15, 21 | 0x08040A00 | BgMap_TickFlashStep | nome existente, corpo nao lido nesta tarefa | aberto |
| 16, 22 | 0x08040A40 | BgMap_LoadTileStageStep | nome existente, corpo nao lido | aberto |
| 17 | 0x08040AF0 | Slot_AdvanceStepByFlag | nome existente, corpo nao lido | aberto |
| 18 | 0x08040B0C | Palette_FadeStepAndDispatch | nome existente, corpo nao lido | aberto |
| 19 | 0x08040B50 | Slot_SetTimer8 | nome existente, corpo nao lido | aberto |
| (extra) | 0x08040B5C | Gfx_CpuSetCopy52Blocks | so na tabela de bytes; nome existente | aberto |
| (extra) | 0x0804169C, 0x0804177C, 0x08041818, 0x080418C0, 0x08041920, 0x080419CC, 0x08041A28, 0x08041B1C, 0x08041BA4, 0x08041BF4, 0x0804172C | nomes existentes (Overlay_LoadTileSetsAndAdvance, BgMap_ClearCopyRowsStep, BgMap_CopyTwoRowsStep, BgMap_CopyRect8x6, Anim_SpawnSlotObject, Slot_SlideStepClamp, Menu_CursorStepHandler, Fight_UpdateBotFlag4000ByState, Slot_CountdownAndClearTile, Slot_SetFrameFromStepAndAdvance, Gfx_ClearTwoRectsStep) | so existem nomes; NAO li corpos. Varios nomes parecem de outros subsistemas (menu, luta, slots) e a tabela de bytes pode ser usada por mais de um motor. Precisa checar cada corpo | aberto |

Observacao: a JT2 (`0x083B2954`) e a MESMA tabela para varias familias; so o primeiro item foi lido e liga o ctx de
mensagem. Antes de chamar os demais de "controle de texto", confirmar que eles tocam `0x03000440`.

## 4. Enderecos sem funcao

- `0x08040898` (entrada FF 0/2/3 da tabela 0x08040884): bytes `06 48 01 78 ...` (ldr), NAO comeca com push. Sem funcao
  no Ghidra, mas nao e alvo de MS3_criar.txt (regra: so `push xx B5`). Ja descrito em MS.md.
- `0x080408BC` (entrada FF 1/4): bytes `18 79 01 30 18 71 02 e0 ...` (ldrb/adds). Nao comeca com push. Mesmo caso.
Conclusao: nenhum alvo sem funcao com `push xx B5` nesta tabela; MS3_criar.txt nao foi criado.

## 5. Renomeacoes feitas no Ghidra

- 0x08040994: BgMap_TickRectFillState -> MsgJT2_PortraitRectFillStep (confirmado pelo decompile: usa ctx 0x03000440).

## 6. Em aberto

- Corpo dos handlers 15..22 e dos 11 extras: checar se usam o ctx de mensagem (0x03000440) antes de nomear como
  controle de texto.
- Estados 10..12 nao lidos (so o nome existente; provavel mesmo padrao do 9).
- Conteudo dos codigos de texto (quebra, espera, nome, cor, som, escolha): nao fechado nesta tarefa; a tabela
  de bytes e a JT2 mostram que portrait/rect/slot sao acionados por byte do roteiro, mas nao foi testado no emulador.
- Corrigir MS.md: JT2 = 0x083B2954 (nao 0x083B2990); 0x080408F4 chama JT2 com ctx+0x1B.
