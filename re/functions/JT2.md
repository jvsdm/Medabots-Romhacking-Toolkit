# Despachantes por tabela/índice (GH-JT2, 2026-10-13)

Continuação de JT.md (que cobriu os UNRECOVERED_JUMPTABLE de fight/J_02). Aqui: os despachantes por byte/word com
tabela na ROM que ficaram sem alvo. Leitura da ROM só leitura (`scratchpad/jt2/rd.py`).

Resultado geral: **dois são saltos reais por tabela** (0x0801387C e 0x08071630), **um é trampolim de chamada por
tabela** (0x08040974), e **três não têm tabela de código** (0x0803C7DC, 0x08065A94, 0x080575DC): as "tabelas" são
ponteiros de dados ou o `bx r0` é só retorno. Os endereços que a tarefa citou como tabela (0x0803C8A2, 0x08065AFE)
são na verdade o `bx r0` de retorno da própria função; as tabelas reais estão nos literais (ver cada seção).

## Resumo

| Despachante | Índice (RAM) | Tabela (ROM) | Entradas | Tipo | Alvos |
|---|---|---|---|---|---|
| 0x0801387C Dispatch_ByteStateIndexTable | byte 0x03000579 (= 0x03000440+0x139), `cmp 5` | 0x080138A8 | 6 | salto real `mov pc,r0` | 6 cases internos (miolo) |
| 0x08040974 Dispatch_CallHandlerByByteAt1B | byte 0x0300045B (= 0x03000440+0x1B), sem limite | 0x083B2990 | 18+ (fim não achado) | chamada `CallViaR0` | funções (ver tabela) |
| 0x0803C7DC Fight_UpdateAnimScriptSlotFrame | byte slot+0x2B / +0x2C (slot = gObjectSlots + k*0x40, k = byte 0x0300056F) | 0x083CEDF4 / 0x083B4C60 | 8+ | ponteiros de DADOS passados como argumento | não são código |
| 0x08065A94 FUN_08065a94 | word [ptr 0x03006F74]+8: 0 → +0x10, senão +0x14 | 0x08485678 (pares de ponteiros) | 2 usados | ponteiros de DADOS | não são código |
| 0x08071630 Dispatch_CallHandlerOrStartAnims | word fight_information+0x08 (0x03001808), `cmp 7` | 0x08071650 | 8 | salto real `mov pc,r0` | 5 labels internos (miolo) |
| 0x080575DC Menu_ChoiceCountdownStep | sem índice por tabela; contador byte 0x0300650F | nenhuma | 0 | não é despachante | retorno normal |

## 1. 0x0801387C Dispatch_ByteStateIndexTable (`Dispatch_ByteStateIndexTable`, corpo 0x0801387C..0x08013A57)

- Índice: `ldr r1,[0x0801389C]` = **0x03000440**; `ldr r2,[0x080138A0]` = **0x139**; byte em **0x03000579**
  (mesmo seletor de J_02/G02). `cmp 5; bls` (0..5), >5 sai pelo retorno 0x08013A50.
- Tabela: literal 0x080138A4 aponta para **0x080138A8** (6 ponteiros thumb, sem bit):

| Índice | Alvo | Passo (lido no código) | Seletor após |
|---|---|---|---|
| 0 | 0x080138C0 | zera +0x96 e 0x03000430; DISPCNT &= 0xF0FF; ajusta BG0..3 CNT; espera `bl 0x0801A788` ≠ 0; zera +0x84 | 1 |
| 1 | 0x0801393C | CpuFastSet de fill (0x800 unidades) em 0x03002C30 (gObjectSlots) | 2 |
| 2 | 0x08013968 | DISPCNT \|= 0x1F40; `bl 0x080158F8`, `0x08043F9C`; gfx por fi+0x38 (tabela de 5 palavras 0x083B2628) via `0x080441A8` | 3 |
| 3 | 0x080139B4 | 2x CpuFastSet de 0x200 unidades (tiles de fundo -> 0x06002800) | 4 |
| 4 | 0x080139F0 | 2x CpuFastSet de 0x200 unidades (tiles de fundo) | 5 |
| 5 | 0x08013A2C | CpuFastSet 0x800 unidades: 0x02028000 -> gObjectSlots; grava callback 0x08044101 em 0x030017F0; +1 em 0x0300137C; zera seletor | 0 (fim) |

- Papel: carga de tela em 6 etapas. É gêmeo do J:0x02 (JT.md): mesmos passos 2, 3/4 e 5 (o idx 5 grava o mesmo
  callback 0x08044101). Não é função separada: os 6 alvos são labels internos de 0x0801387C.
- Confiança: confirmado (lido no código e na ROM). Quem chama 0x0801387C não foi rastreado aqui.

## 4. 0x08071630 Dispatch_CallHandlerOrStartAnims (corpo 0x08071630..0x0807173F)

- Índice: `ldr r1,[0x08071648]` = **0x03001800** (fight_information); índice = **word em fi+0x08 (0x03001808)**,
  `cmp 7; bhi` (0..7; >7 cai no bloco 0x080716FC, que zera fi+0x08 e dispara 3 passos + 2 chamadas de 0x08071DB8).
- Tabela: literal 0x0807164C aponta para **0x08071650** (8 ponteiros thumb, sem bit):

| Índice | Alvo | Passo (lido no código) |
|---|---|---|
| 0 | 0x08071670 | `bl 0x0803FAA0(1,0,0)`; passo 0x08071898 com (0x19,0x4B,8); fi+0x08 += 1 |
| 1 | 0x080716D8 | ESPERA: `0x0803FB3C(tabela 0x080716F8[(fi+8)>>1 + 0xF])`; só avança (fi+8 += 1) se der verdadeiro |
| 2 | 0x08071682 | `bl 0x0803FB0C`; passo 0x08071898 (0x19,0x4B,8); fi+0x08 += 1 |
| 3 | 0x080716D8 | ESPERA (mesmo bloco do 1) |
| 4 | 0x08071696 | `bl 0x0803FB0C`; 0x080718D4 (0x17,0x36); fi+0x08 += 1 |
| 5 | 0x080716D8 | ESPERA (mesmo bloco do 1) |
| 6 | 0x080716AA | `bl 0x0803FB0C`; 0x080718D4 (0x17,0x33) e (0x19,0x4B); 0x08071898 (0x18,0x3F,6); fi+0x08 += 1 |
| 7 | 0x080716D8 | ESPERA (mesmo bloco do 1) |

- Papel: máquina de passos por fi+0x08 (passo interno da luta; nome do campo não confirmado). Pares par/ímpar:
  passo par = ação e avanço; passo ímpar = espera por condição (predicado 0x0803FB3C) antes de avançar.
- Os alvos 0x08071670/0x08071682/0x08071696/0x080716AA/0x080716D8 são labels internos de 0x08071630 (não são função).
- Confiança: confirmado no código e na ROM. O predicado 0x0803FB3C e as rotinas 0x08071898/0x080718D4 não foram
  decompiladas aqui (fora do escopo).

## 2. 0x08040974 Dispatch_CallHandlerByByteAt1B (corpo 0x08040974..0x08040989)

- Código: `ldr r1,[0x0804098C]` = **0x083B2990** (tabela); `ldr r0,[0x08040990]` = **0x03000440**; índice =
  **byte em 0x0300045B** (`ldrb [r0,#0x1B]`); entrada = `tabela[idx]` passada em r0 para `bl 0x0807CCF0`
  (CallViaR0: `bx r0`). Não há limite no código: o fim da tabela foi achado pelo dado não-ponteiro logo depois.
- Tabela: **23 entradas (índices 0..22)**, ponteiros thumb em 0x083B2990..0x083B29EB. O índice 23 em 0x083B29EC
  não é ponteiro (dado). Valores fora de 0..22 chamam lixo: ver nota de quem grava 0x0300045B.

| Índice | Alvo (sem bit thumb) | Nome atual no Ghidra | Papel |
|---|---|---|---|
| 0 | 0x08040B50 | função desde o sync de 2026-10-14 (nome: Slot_SetTimer8) | `JT2_lista.txt` |
| 1 | 0x0804169C | Overlay_LoadTileSetsAndAdvance | (não lido) |
| 2 | 0x0804177C | BgMap_ClearCopyRowsStep (era FUN_0804177c) | zera 2 linhas de 8 e copia 2 padrões de 8 halfwords para o mapa BG; +0x29 += 1 |
| 3 | 0x08041818 | BgMap_CopyTwoRowsStep (era FUN_08041818) | zera 1 linha, copia 2 linhas de 8x8 da ROM para o mapa; +0x29 += 1 |
| 4 | 0x080418C0 | BgMap_CopyRect8x6 | (não lido) |
| 5 | 0x08041920 | Anim_SpawnSlotObject | (não lido) |
| 6 | 0x080419CC | Slot_SlideStepClamp | (não lido) |
| 7 | 0x08041A28 | Menu_CursorStepHandler | (não lido) |
| 8 | 0x08041B1C | FUN_08041b1c | muda bit 0x4000 de um atributo de slot conforme +0x84; +0x84 ou +0x29 += 1. Papel não fechado |
| 9 | 0x08041BA4 | Slot_CountdownAndClearTile | (não lido) |
| 10 | 0x08041818 | = índice 3 | |
| 11 | 0x0804177C | = índice 2 | |
| 12 | 0x0804172C | Gfx_ClearTwoRectsStep | (não lido) |
| 13 | 0x08041BF4 | função desde o sync de 2026-10-14 (nome: Slot_SetFrameFromStepAndAdvance) | `JT2_lista.txt` |
| 14 | 0x08000840 | NoOpActionHandler | não faz nada (1 instrução) |
| 15 | 0x08040994 | BgMap_TickRectFillState | (não lido) |
| 16 | 0x08040A00 | BgMap_TickFlashStep | (não lido) |
| 17 | 0x08040A40 | função desde o sync de 2026-10-14 (nome: BgMap_LoadTileStageStep) | `JT2_lista.txt` |
| 18 | 0x08040AF1 | sem função no Ghidra | `JT2_lista.txt` |
| 19 | 0x08040B0D | sem função no Ghidra | `JT2_lista.txt` |
| 20 | 0x08040B50 | = índice 0 | |
| 21 | 0x08040994 | = índice 15 | |
| 22 | 0x08040B50 | = índice 0 | |

- Papel do despachante: chamador de handler de passo de tela/sprite por byte de estado (0x0300045B). Os alvos são
  "um passo por quadro" de BG/OBJ (mapa, slot, cursor).
- Obs.: 0x08040B50, 0x08041BF4, 0x08040A40, 0x08040AF1 e 0x08040B0D são ponteiros thumb válidos, mas o Ghidra não
  tem função nesses endereços (é preciso criar a função antes de documentar). Não foram lidos.
- Confiança: tabela confirmada na ROM (23 entradas, fim pelo dado seguinte). Quem grava 0x0300045B não foi rastreado.

## 3. 0x0803C7DC Fight_UpdateAnimScriptSlotFrame (corpo 0x0803C7DC..0x0803C8A3)

- O `bx r0` em 0x0803C8A2 é só o retorno (não é tabela). Índices reais:
  slot = gObjectSlots (`0x03002C30`, literal 0x0803C7FC) + k*0x40, com **k = byte 0x0300056F** (= 0x03000440+0x12F).
- Tabela A `0x083CEDF4` (literal 0x0803C858), índice = **slot+0x2B**, entrada passada a `0x0801BD58(entrada, 0)`.
  Entradas lidas (ponteiros de DADOS na ROM, não código): [0] 0x083CE810, [1] 0x083CE836, [2] 0x083CE850,
  [3] 0x083CE86A, [4] 0x083CE890, [5] 0x083CE8A6, [6] 0x083CE8CC, [7] 0x083CE8F2. Fim não achado no código.
- Tabela B `0x083B4C60` (literal 0x0803C8A4), índice = **slot+0x2C**, entrada passada a `0x0803F15C(entrada, ...)`.
  Entradas lidas (dados, passo de 0x80): [0] 0x083B2B00, [1] 0x083B2B80, [2] 0x083B2C00, [3] 0x083B2C80,
  [4] 0x083B2D00, [5] 0x083B2D80, [6] 0x083B2E00, [7] 0x083B2E80. Fim não achado no código.
- Papel: passo de animação por slot de OBJ (contador halfword em slot+0x30; ao zerar, chama o quadro da tabela A
  ou B). Nenhum alvo é função: não entra em JT2_lista.txt.
- Confiança: tabelas lidas na ROM; semântica dos dados (animação/script) não decodificada.

## 5. 0x08065A94 FUN_08065a94 (corpo 0x08065A94..0x08065AFF)

- Não é tabela de código. Seleção em 2 entradas: `ldr r4,[0x08065ACC]` = 0x03006F74 (ponteiro para struct); índice
  = `*(*(0x03006F74) + 8)`: se 0 usa `base+0x10`, senão `base+0x14`, com **base = 0x08485678** (literal 0x08065AC8).
- Entradas: [4] 0x08485688 -> 0x084852FE; [5] 0x0848568C -> 0x08485324 (ponteiros de dados; [0..3] = 0x08485268,
  0x0848528F, 0x084852B5, 0x084852E7 não usados aqui). Passa a entrada a `0x0803FB3C` (predicado): se falso sai.
- Se verdadeiro e `*(0x03006F74)+8 == 0`: contador 0x0300137C += 1 e zera halfword +0x3A.
  Senão, se bit0 de byte 0x0300058C: SFX 0x0D (`0x08074DE0`), zera **byte 0x0300045B** (índice do despachante
  0x08040974, seção 2) e byte 0x03000440+0x84, e 0x0300137C -= 2.
- Papel: passo de espera/cancelamento de menu-luta. Não há alvo de função; o `bx r0` em 0x08065AFE é retorno.
- Confiança: confirmado no código; o significado de 0x03006F74 não foi decodificado.

## 6. 0x080575DC Menu_ChoiceCountdownStep (corpo 0x080575DC..0x0805767C)

- NÃO é despachante: não há tabela nem índice por tabela. O `bx r0` em 0x0805767C é retorno normal
  (`pop {r0}; bx r0`), não salto.
- Estado: contador de byte em **0x0300650F** (= 0x030064A0+0x6F, literal 0x08057658). Se 0 (0x080575F0): LZ
  descomprime 0x084C22E8 em 0x06002100 (0x080009BC, literal 0x08057680/0x08057684) e chama 0x0804A210. Se > 0:
  desenha 2 retângulos (`0x080492A8`) com passos 0x1E - c e 0x1F - c, chama
  `0x08058070(1)` e decrementa o contador.
- Nome atual `Menu_ChoiceCountdownStep` é coerente com o uso; mantido.
- Confiança: confirmado no código.

## Pendências e itens para o usuário

- Ponteiros thumb válidos (0x08040B50 = Slot_SetTimer8, 0x08041BF4 = Slot_SetFrameFromStepAndAdvance e 0x08040A40 = BgMap_LoadTileStageStep são funções desde o sync de 2026-10-14; 0x08040AF1 e 0x08040B0D seguem sem função): ver `JT2_lista.txt` (0x08040B50, 0x08041BF4,
  0x08040A40, 0x08040AF1, 0x08040B0D). Nenhum criado aqui.
- Labels internos (miolo) de 0x0801387C e 0x08071630: estão em `JT2_lista.txt` só para referência; não são funções.
- Tabela 0x083B2990 (0x08040974): quem grava 0x0300045B não foi rastreado; ver o valor máximo do índice antes de
  dizer que 23 entradas são todas as usadas.
- Predicados e rotinas chamadas (0x0803FB3C, 0x0803FB0C, 0x0803FAA0, 0x08071898, 0x080718D4, 0x0801BD58, 0x0803F15C)
  não foram decompiladas aqui.
- Ghidra: renomeados FUN_0804177c -> BgMap_ClearCopyRowsStep e FUN_08041818 -> BgMap_CopyTwoRowsStep. Comentários de
  disassembly em 0x08013898 (salto de 0x0801387C), 0x08071644 (salto de 0x08071630) e 0x08040982 (chamada de 0x08040974).
