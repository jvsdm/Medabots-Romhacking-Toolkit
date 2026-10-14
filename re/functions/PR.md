# PR - sistema de progresso da história (GH-PR, 2026-10-14)

Status: PARCIAL. Confirmado: quem grava o capítulo, os 9 eventos que o escrevem, o formato das flags e
campos de evento (3 bits), os opcodes que os usam, e os dois leitores do capítulo já conhecidos.
Pendente: quem lê o capítulo além de spawn e Medaforça, e o layout exato no save para o endereço de flags.
Fontes: EV.md, EX.md, HE1..HE4.md, AI2.md, SV.md, pc/research/README.md (lido inteiro),
Ghidra (leitura, sem renomear nada novo), ROM original (leitura), extract_events.py importado por caminho.
Endereços da EU. Dados do jogo só do ROM original.

## 1. Capítulo (byte 0x03005FDF = gCurrentChapter)

- Endereço: `E+0x7FF` com E = 0x030057E0, ou seja 0x03005FDF. Confirmado (HE4, decompile).
- Save: RAM 0x030057E0 = save +0xFF8 (SV.md), então o capítulo fica no save em +0x17F7. Derivado do mapa linear de SV.md, não medido.
- Escritor único encontrado: opcode **0x6B** `Set_Current_Chapter` (handler `Event_Op6B_Set_Current_Chapter` 0x0804D208).
  Lê 1 byte do script e grava. Não tem continuação: é imediato. Confirmado no decompile.
  A busca por literal (0x03005FDF) só acha 0x08051484 (leitor) e 0x083D6298 (dado sem função, não identificado).
- Valor inicial: 0 (New Game limpa a RAM em 0x0804A2F0, README S1).
- Eventos que executam 0x6B (varredura de todos os 2288 eventos com o decodificador do extrator; só alcançáveis
  por BFS dentro do próprio evento, sem seguir Goto). Só ids e valores:

| evento id | valor gravado |
|---|---|
| 103 | 1 |
| 104 | 2 |
| 131 | 7 |
| 132 | 8 |
| 157 | 9 |
| 171 | 5 |
| 207 | 3 |
| 208 | 4 |
| 1627 | 6 |

  Nenhum evento grava 0. Ordem de capítulo (1 a 9) bate com a ordem de id só em parte (ex.: 1627 = 6), então
  a história não é linear por id. Mapa e texto não listados de propósito.

### Leitores do capítulo (quem consulta 0x03005FDF)

- **Spawns de mapa** (`Map_LoadEntitySpawnsForCurrentMap`, literal 0x08051484): cada registro de 8 B tem máscara
  u16 em +6; entra só se `(mascara >> capitulo) & 1` (EX.md, confirmado). Capítulo >= 16 nunca carrega; o C#
  `IsVisibleInChapter` devolve true nesse caso: é divergência do extrator a corrigir (EX.md).
- **Medaforça da CPU** (`Fight_IsMedaforceBlockedByChapter` 0x08015648, AI2.md, confirmado no decompile):
  bloqueia medaforça para bot >= 3 (inimigos) se capítulo <= 2. Interpretação provável: inimigos não usam
  medaforça no início.
- Pendente: outros leitores. A busca literal não acha acesso por base+offset (ex.: literal 0x030057E0 + 0x7FF).
  O 0x030057E0 tem ~140 literais na ROM; não varri um a um.

## 2. Flags de evento (ids empacotados)

Uma flag e um campo de 3 bits dividem o mesmo byte, por par de ids (confirmado nos decompiles):

| id | byte | flag (1 bit) | campo de 3 bits (variável) |
|---|---|---|---|
| par (id & 1 = 0) | `base + id>>1` | bit 7 (0x80) | bits 4..6 (0x70) |
| ímpar (id & 1 = 1) | `base + id>>1` | bit 3 (0x08) | bits 0..2 (0x07) |

- `base` = ponteiro lido de literal, que aponta para **0x030057D8** (todos os DAT_ usados nessas rotinas: 0x08053184,
  0x0805319C, 0x08053200, 0x08053218, 0x08053260, 0x0805327C, 0x080532F0, 0x08053308; literal 0x030057D8 confirmado).
  Ou seja, a área de flags começa 8 bytes antes do bloco E (0x030057E0). Índice máximo = 0x7FF (ids 0..0xFFF).
  Conflito a conferir: se o save for linear, 0x030057D8 cai em save +0xFF0, em cima do checksum +0xFF4 (SV.md).
  Não resolvido sem savestate.
- Leitura de flag: `EventFlag_IsSetByPackedId` 0x080531E0 (par: bit 7; ímpar: bit 3).
- Escrita de flag: set `EventFlag_SetIndexedFlagByPackedId` 0x080531A0; clear `EventFlag_ClearIndexedFlagByPackedId`
  0x08053168; set-or-clear `EventFlag_SetOrClearByPackedId` 0x0805321C.
- Campo 3 bits: set `EventField3Bit_SetByPackedId` 0x08053238 (par: `b = b&0x8F | v<<4`; ímpar: `b = b&0xF8 | v`,
  v = arg & 7); get `EventField3Bit_GetByPackedId` 0x080532D4; increment `EventField3Bit_IncrementByPackedId` 0x08053280.
  Flag e campo não se sobrescrevem (máscaras 0x8F/0xF8). Confirmado.

## 3. Opcodes que mexem em flags e campos (decimal do Ghidra = número após "Op")

Números abaixo são o opcode em hex (os nomes do Ghidra e os títulos do HE1..HE4 usam o mesmo número sem "0x").

| opcode | nome Ghidra / handler | efeito | args |
|---|---|---|---|
| 0x12 | `Event_Op12_AddEventPermanently` (0x0804BBC8) | limpa a flag "removido" (evento volta a aparecer) | 2 (id BE) |
| 0x13 | `Event_Op13_RemoveEventPermanently` (0x0804BBF4) | liga a flag "removido" | 2 |
| 0x16 | `Event_Op16_RemoveEventNow` (0x0804BCD4) | some agora (sem salvar flag) | 2 |
| 0x17 | `Event_Op17_SetEventVariable` (0x0804BD08) | campo 3 bits = arg & 7 | 3 (id, valor) |
| 0x18 | `Event_Op18_IncrementEventVariable` (0x0804BD44) | campo 3 bits += 1 | 2 |
| 0x2F | `Event_Op2F_JumpTableByEventField` (0x0804C444) | lê o campo 3 bits do PRÓPRIO evento (gCurrentEventId), usa como índice numa tabela inline de saltos | variável |
| 0x3E | `Event_Op3E_IncreaseCappedEventCounter` (0x0804C778) | contador += qtd, teto 0x20 | 2 (sel, qtd) |
| 0x3F | `Event_Op3F_DecreaseCappedEventCounter` (0x0804C7DC) | contador -= qtd, piso 0 | 2 (sel, qtd) |
| 0x4C | `Jump_If_Missing_Part` (0x0804CA84) | salta se peça não existe | 3 |
| 0x50 | `Event_Op50_Jump_If_Event_Counter_Below` (0x0804CBDC) | salta se contador(sel) < valor | 3 (sel, valor, salto) |
| 0x6B | `Event_Op6B_Set_Current_Chapter` (0x0804D208) | grava capítulo | 1 |

- Contadores: sel 0 = E+0xCBC (0x0300649C, save +0x1CB4); sel != 0 = E+0xCBD (0x0300649D, save +0x1CB5).
  Teto: 0x20 (confirmado pelo decompile de 0x3E: compara com ' '). Bug do jogo (HE2): no sel 0 o teto é aplicado
  em CBD, não em CBC. Nota: "0x3E/0x3F" é o número do opcode, não teto.
- 0x50 compara com sinal no contador e sem sinal no valor (HE3).
- Ponto de ambiguidade: HE3 diz que o 0x50 confere contador < valor; o nome do C# ("Jump_If_Current_Active_Object_Index_Not_Equal") está errado, como o de 0x51.

## 4. O que fica em aberto

- Quem liga/desliga o "removido" nos eventos: opcodes 0x12 e 0x13 (ver HE1); quem chama esses opcodes nos scripts ainda não foi listado.
- Quem usa a tabela 0x083D6298 (dado perto de 0x083D6274, o endereço do contador CBC).
- Base de flags no save: conflito entre 0x030057D8 e checksum +0xFF4 (ver §2).
- Leitores do capítulo além de spawns e Medaforça (ver §1).
- Medir no emulador (não feito nesta tarefa): capítulo após evento 103/104 (savestate + replay).

## 5. Símbolos

Ver `PR_simbolos.csv`. Nenhum nome novo foi aplicado no Ghidra: as funções relevantes já tinham nomes.
