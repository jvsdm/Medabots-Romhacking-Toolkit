# LK: recursos de link (cabo Link, SIO multiplayer), Metabee (Europe)

Feito em 2026-10-14 pelo agente GH-LK. Lido no Ghidra (decompile, xrefs) e nos literais da ROM original
(offset = endereço - 0x08000000). Confiança: **confirmado** (lido no código ou no literal), **provável**, **chute**.
Símbolos em `LK_simbolos.csv`. Renomeados no Ghidra só os que ficaram confirmados (§7).

## 1. Quem chama o quê (confirmado por xrefs)

- `AgbMain` 0x0800025C chama `InitializeLinkTransferState` 0x080020C8 (em 0x0800028E, no boot) e
  `UpdateLinkTransferState` 0x080021F4 (em 0x080002EA, por quadro, dentro do laço principal, ver MN.md §1).
- `SoftResetSequence` (0x080008C0) chama `InitializeLinkTransferState` (0x08000918) e `FUN_080021A4` (0x080008F2).
  Não chama `UpdateLinkTransferState` (esse só aparece em `AgbMain`).
- `FUN_08065B0C` (handshake por quadro, §4.1) chama `InitializeLinkTransferState` (em 0x08065B30) quando
  `(0x03005010 & 0xF00) == 0` e `gFrameCount & 0xF == 0`. Ou seja, tenta reabrir o link a cada 16 quadros
  enquanto nenhum peer aparece.
- `Sm_StepSfxRepeat3` (0x080659FA) e `Boot_ResetSystemAndState` (0x0803EF28 / 0x0803EF42) também chamam
  `InitializeLinkTransferState`; `Screen_InitBlendFade` (0x0805DD90) chama `FUN_080021A4` (ver §7).
- **Evento 0x76** (`Event_Op76_Start_Medabot_Link` 0x0804D384): grava `1` no byte 0x03006469 (literais
  0x0804D390 = 0x030057E0 e 0x0804D394 = 0x0C89; E+0xC89 = "pendente" do evento, ver HC1.md). **Confirmado.**
  O evento 0x76 completo, `Event_Op76_Cont_Start_Flow4_Then_Scene_Command_05` (0x0804FB1C), chama
  `GameState_StartFlowByCommand(4)` (gamestate_menu 0x15, MN.md §2.1) e depois o comando de cena 5.
- **Batalha** (BT.md): passo 0x50 `Fight_SceneStep50_EndBattleBranchLink` (T+0x89 == 1 = link, vai ao 0x58);
  passo 0x58 `Fight_SceneStep58_InitLinkTextBox` 0x08031978 (**confirmado**: `Obj_InitSlotAndLoad(1,0,0)` e
  sub++ em 0x0300137C = gamestate_subroutine); passo 0x53 `Fight_SceneStep53_ExitBattleScene` (link vai ao menu
  10, sub 0x20).

## 2. Registradores SIO (confirmado pelos literais e pelos xrefs)

| Endereço | Registrador | Uso no código de link |
|---|---|---|
| 0x04000120..0x04000126 | SIOMULTI0..3 | lidos em `FUN_08002438` (4 slots) |
| 0x04000128 | SIOCNT | init escreve 0x2000 e depois faz `|= 0x4003`: bits 12-13 = 2 (**modo multiplayer**), bit 14 = IRQ, bits 0-1 = 3 (baud 115200). Leitura: bit 7 = busy/start, bits 4-5 = ID (0 = pai), bit 3 = SI, bit 2 = SD. |
| 0x0400012A | SIOMLT_SEND | escrita de 1 halfword por passo em `FUN_08002438` (`buf[2*cont]`) |
| 0x04000134 | RCNT | zerado em `InitializeLinkTransferState` (modo SIO, não GPIO) |
| 0x04000200 | IE | init: `&= 0xFF3F` (desliga Timer3 e Serial) e, ao fim, `|= 0x80` (liga Serial). No pai: `&= 0xFF7F`, `|= 0x40` (Serial off, Timer3 on) |
| 0x04000202 | IF | ack com 0xC0 (Timer3 + Serial) em `UpdateLinkTransferState` |
| 0x04000208 | IME | 0 durante a configuração, 1 no fim |
| 0x0400010C / 0x0400010E | TM3CNT_L / TM3CNT_H | Timer3 cronometra a rodada: reload 0x63F7; TM3CNT_H = 0xC0 (enable + IRQ) |
| 0x03007FF8 | IntrCheck (BIOS) | `|= 0x80` (bit do Serial) em `FUN_08002438` ao fechar a rodada |

Modo: **multiplayer** (SIOCNT modo 2). Não é o modo normal (8/32 bits) nem UART.

## 3. Pacote, sincronização por quadro e detecção de peer

Bloco de estado: **0x03005170** (`gLinkSlotInput` no mtbe). Offsets do Ghidra (base 0x03005170):

- `+0x00` byte: estado da rodada (8 = pai iniciou o envio; 1 = envio ativo; 0 = ocioso).
- `+0x02` byte, `+0x03` byte: entram no cabeçalho do pacote (`byte1 = +2 ^ +3`).
- `+0x04` byte: "pacote pendente" (1 depois de `QueueLinkTransferPacket`).
- `+0x07` byte: `(SIOCNT & 0x7F) >> 6` (bit de erro), escrito em `FUN_08002438`.
- `+0x0B` byte: comando (cmd) a enviar.
- `+0x14` int: contador de passos de envio (0..5). `+0x18` int: contador de quadros recebidos (0..5; `>3` = rodada completa).
- `+0x1C` ptr: buffer de envio (base+0x30); `+0x20` ptr: buffer alternativo (base+0x3C), trocado com +0x1C.
- `+0x24` ptr: tabela de recepção (base+0x48), trocada com `+0x28` (base+0x78).
- `+0x2C` ptr: tabela de pacotes recebidos, 4 slots x 12 B (base+0xA8).

**Pacote de envio** (`QueueLinkTransferPacket` 0x080022DC, confirmado):
- hw0 = [cmd (byte0), cmd XOR (byte1)]; hw1 = checksum; hw2 = payload (1 halfword copiado do parâmetro); hw3 = 0.
- checksum = `~(hw0 + hw1 + hw2 + hw3)` em 16 bits, com hw1 = 0 durante a soma.
- Validação na recepção (`FUN_08002328`): o pacote é válido se `hw0+hw1+hw2+hw3 == 0xFFFF`. Se válido, copia o
  payload (hw2) para `param + 4*slot`, liga o bit `1<<slot` em `struct+3` e zera o payload.

**Rodada** (`FUN_08002438`, confirmado no código; quem a dispara não foi achado por xref):
- Sentinela de fim: `SIOMULTI0 == 0xFEFE` (literal 0x08002524) com `+0x18 > 3`. Ao fechar, troca as tabelas de
  recepção, zera `+0x18..+0x1B` para 0xFF, troca os buffers de envio se `+4`, zera `+4` e liga o bit 7 no IntrCheck.
  Significado exato do 0xFEFE: provável (sinal de "todos os slots prontos").
- Se `+0x14 < 4`: envia `buf[2*cont]` em SIOMLT_SEND. Se `+0x18 >= 0`: grava os 4 SIOMULTI na tabela, stride 12 B por slot.
- Ao fim, se `struct[0] != 0`: `TM3CNT_H = 0xC0` e, com `+0x14 < 5`, `SIOCNT |= 0x80` (start). O Timer3 (reload 0x63F7) marca o tempo entre quadros.

**Detecção de peer** (`UpdateLinkTransferState`, confirmado no código): o bitfield de slots ligados é
0x03005010 (`gLinkStateBlock`, bits 0..3). Para cada slot: sem sinal, o contador em 0x03005029+i sobe até 11 e o bit
é zerado (peer sumiu); com sinal, zera o contador e liga o bit (peer entrou).

**Pai e filho**: o pai é o aparelho com ID 0 (SIOCNT bits 4-5 == 0) e SI = 1, sem busy. Só o pai dispara a rodada
(bloco `UpdateLinkTransferState`, quando `struct[0] == 0`, `(SIOCNT & 0x88) == 8`, SD = 0 e `+0x14 == 6`).
Como o filho responde: provável; não lido por completo.

## 4. Handshake e camada de comando (provável)

### 4.1 `FUN_08065B0C` (renomeado para `Link_HandshakeFrameStep`, §7)
Laço por quadro sobre a grade de células do bloco 0x03005010: célula i em `+0x0A + 4*i`, valor do aparelho em `+6`.
- Valor "eu" = 0xFF40; "eu, confirmado" = 0xFF41 (`uVar1` e `uVar1|1`).
- Escreve no próprio slot a cada quadro: `+6 = byte(obj+0x49) | 0xFF40`. `obj+0x49` (ponteiro em 0x03006F74) = "pronto".
- Com os 2 slots ligados (`gLinkStateBlock & 3 == 3`) e a célula do peer igual a 0xFF40 ou 0xFF41: `obj+0x49 = 1`.
- Se a célula do peer é 0xFF41 (**o peer confirmou**): com slot local < 2, incrementa `gamestate_subroutine`
  (0x0300137C), zera `obj+0x3A` e grava 0xFF em `gLinkActiveFlag` (0x03005030). Confirmado no código.
- Timeout: `obj+0x3A` sobe a cada quadro; em 0x12D (301) sem resposta: `Obj_ResetStateFields`, `obj+8 = 1` e
  decrementa `gamestate_subroutine`. Provável (o valor 301 e o decremento não foram testados com emulador).
- Chama `InitializeLinkTransferState` (ver §1), `FUN_08002190`, `Screen_ReadTwoBitField` (§5), `Obj_ResetStateFields`.

### 4.2 `FUN_08066460` (sem nome no Ghidra; provável "comando 0xEF00")
- Halfword de comando: **0xEF00 | cmd** (byte alto 0xEF = assinatura, byte baixo = comando). Literal 0xFE0F
  (0x080664CC, 0x08066504) = valor de comparação do timer (provável).
- Com `obj+0x7A == 0`: sem entrada grava `0xEF00 | obj+5` na saída (bloco 0x03005010); com entrada, se o byte
  alto for 0xEF, guarda o byte baixo em `obj+5` e liga `obj+0x7A = 1`; se igual ao esperado, ecoa.
- Com `obj+0x7A` ligado copia o halfword de parâmetro.
- Fim: se o timer `+0xE` bate com 0xFE0F: se `obj+5 == 4`, vai ao estado 8 e chama `Obj_ResetStateFields`; senão
  `Obj_InitTimerRecord(..., 0xC0)` e avança +2.
- Quem chama: não encontrado nas xrefs de função (provável: chamada por ponteiro de tabela de estado).
- **Chute**: é a camada de comandos (candidata a troca de peça/medalha), não o transporte. Não confirmado.

## 5. Leitura de status (nome errado no Ghidra)
- Quem lê SIOCNT (0x04000128) é a função que contém 0x0805BB1E, chamada como `Screen_ReadTwoBitField`. Pelo
  nome, o nome está errado: não é leitura de tela. Sugestão `Link_ReadSioStatusBits`, provável (campo: SD/SI).
  Não renomeado, pendente de confirmação.

## 6. Pendente (não achado nesta rodada)
- **Troca de peças e medalhas por link**: busca por "Trade" e "Serial" no Ghidra vazia; não há função de troca
  com esse nome. Candidato provável: menu 0x15 (`GameState_StartFlowByCommand(4)`), handlers de MainState2. Sem achado.
- **Batalha por link, fluxo completo**: falta ler o handler do menu 10 (0x0803D538) e o menu 0x15. Sem achado.
- Significado de 0xFEFE e do bit SI no filho: confirmar com emulador (não usado nesta rodada, por regra).
- Conflito de documentação: EV.md cita o pendente em 0x03006468; HC1.md e o literal de Event_Op76 dizem 0x03006469
  (E+0xC89). Usado 0x03006469.

## 7. Renomeados no Ghidra (nesta rodada)
- `FUN_08065b0c` -> `Link_HandshakeFrameStep` (confirmado que é link: 0x03005010, 0x03005030, 0x0300137C).
- `FUN_08002438` -> `Link_SerialIrqTransferStep` (confirmado: SIOMULTI, SIOMLT_SEND, 0xFEFE).
- `FUN_08002328` -> `Link_CollectReceivedPackets` (confirmado: soma 0xFFFF e bit por slot).
- `FUN_080021a4` -> `Link_RearmSioMultiplayer` (confirmado: SIOCNT 0x2003, IE, TM3).
- Não renomeados (provável): `FUN_08066460` (sugestão `Link_CommandExchangeStep`) e a função de §5.
