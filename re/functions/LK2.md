# LK2: continuação do link (menu 10, menu 0x15, troca, comando 0xEF00), Metabee (Europe)

Feito pelo agente GH-LK2 (2026-10-14). Lido no Ghidra (decompile, xrefs, nomes) e nos literais da ROM original
(offset = endereço - 0x08000000, só leitura). Confiança: **confirmado**, **provável**, **chute**.
Símbolos em `LK2_simbolos.csv`. Sem função no Ghidra: `LK2_lista.txt`. Renomeados no Ghidra: §5.

## 1. Menu 10 (0x0803D538), tabela de passos 0x083AFC78 (96 entradas)

Despachante `Scene_GsDispatch_Menu10_Link` (confirmado): chama `Battle_CheckExitShortcut`; se não sair, limpa com
`bios_cpufastset` quando `[0x0300047C]==2`, chama `Overlay_ClearTimerIfFlag` e `CallViaR0(tab[gamestate_subroutine])`.
A condição `(sub-0x10) < 0x10` (sub 0x10..0x1F) pula a saída.

Os handlers têm prefixo `Fight_` no Ghidra (ex.: `Fight_ResetStateFlagsAndCounter` 0x0803D5A0 no passo 0,
`Fight_InitSelectBlockA` 0x0803DAED no passo 14, `Screen_InitOverlayAndLoadAssets` 0x0803D74C no passo 32). Ou seja, o
menu 10 é um **laço de batalha reaproveitado** (estado de sprites, barras, paleta, som), não uma tela de troca.

Blocos (sub = índice):
- **0..14**: início da cena; passo 0 zera flags; passo 3 (0x0803D634) e passo 4 (0x0803D674) são o **tick de link**:
  por quadro, se `0x03000B90 == 0` (pai), escrevem `0x8000 | byte` no halfword de `0x03005010+6` e contam quadros
  (`0x03001780`). **Confirmado** (disasm + leitura de 0x03005010/0x03000B90).
- **15..31**: vazios (NoOp `0x08000841`), exceto 16..23.
- **16** (0x0803E4B5): entrada do "peer sumiu" (MainState2 manda sub 0x10 ao perder o link, MN.md §1). Toca som e
  gauge. Provável: tela de desconexão. Os passos 17..23 seguem com gauge/contador.
- **32** (0x0803D74C): `Screen_InitOverlayAndLoadAssets`: carrega overlay e assets. Confirmado no corpo.
- **33..44, 45..68**: passos de gauge, contador (`Counter_IncIfFlagBit0` 0x0803E894), sprites e sons. Provável:
  apresentação de dano/valores da batalha; não lidos um a um.
- **52, 53** (0x0803DEF1, 0x0803DF99) e **80** (0x0803E681): leem 0x03005010 (sincronia de link). Provável: pontos
  de troca de estado com o peer. Passo 53 = saída (LK.md chama de "passo 0x53" da batalha; é outro passo, mesmo número).
- **69..71, 76..79, 87..95**: vazios.
- **72..86**: sprites, sons e o passo 80 (sincronia).

Conclusão do item 1: **não há troca de peça/medalha no menu 10**. É a batalha com pontos de sincronização de link.
Confiança: provável no mapa completo; confirmado nos passos 3, 4 e 32.

## 2. Menu 0x15 (iniciado por StartFlow(4) no evento 0x76)

- `GameState_StartFlowByCommand(4)` grava `gMainState=2`, `gamestate_menu=0x15` (MN.md §2.1, confirmado).
- Tabela de MainState2: entrada 0x15 = `0x08065D11` (confirmado no pool 0x08483594 + 0x54).
- Handler: **`Fight_CallHandlerWithArgCopy` 0x08065D10** (confirmado no disasm): copia 8 palavras da ROM 0x080AFC88
  para a pilha e chama `[sub 0x0300137C]` (índice 0..7, via `0x0807CCF0`).

Tabela 0x080AFC88 (confirmada):

| sub | endereço | nome / função |
|---|---|---|
| 0 | 0x08065790 | função desde o sync de 2026-10-14 (nome: LinkMenu15Step0); chama 0x0803FAA0 e 0x08074CE0; toca `obj` 0x03006F74. Provável: preparo/início. |
| 1 | 0x080657C4 | `Sm_StepSfxRepeat3` (provável: espera de som/mensagem). |
| 2 | 0x08065A80 | função desde o sync de 2026-10-14 (nome: LinkMenu15Step2); chama `StateMachine_DispatchSub` (0x08041CA4) e 0x080476E8. Provável: espera de texto. |
| 3 | 0x08065A0C | `Battle_TickTwoStageSequence` (nome do Ghidra; provável: dois estágios de sequência). |
| 4 | 0x08065A94 | `FUN_08065a94` (sem nome). |
| 5 | 0x08065B0C | **`Link_HandshakeFrameStep`** (handshake por quadro, LK.md §4.1). Confirmado. |
| 6 | 0x08065C98 | função desde o sync de 2026-10-14 (nome: LinkMenu15Step6); toca `0x03005010` e `0x03000B90`. Provável: troca de comandos (liga ao §3). |
| 7 | 0x08065CFC | função desde o sync de 2026-10-14 (nome: LinkMenu15Step7); chama `StateMachine_DispatchSub` e 0x080476E8. Provável: fim da etapa. |

Ou seja, o menu 0x15 é a **tela de link iniciada pelo evento 0x76**: handshake (sub 5) e depois troca de comandos
(sub 6). Confiança: confirmado a tabela e o sub 5; provável o papel dos demais.

## 3. Troca de peças e medalhas por link: não achei função de troca

- Busca por nome: "Trade" e "Exchange" sem resultado no Ghidra (**confirmado**). "Link" retorna só as funções de
  transporte e de estado já documentadas (LK.md).
- Protocolo: cada passo de comando envia **1 halfword** (`QueueLinkTransferPacket`, LK.md §3). Trocar uma peça
  exigiria várias rodadas; não há sinal disso no código lido.
- Tabela de comandos 0x080AFCC0 (confirmada, 13 ponteiros; a seguir há texto que não é código): índice = comando (provável).
  Chamada por uma função perto de 0x08066760..0x0806685E (referenciada pelo pool em 0x08066860; sem função no Ghidra).
  0: 0x08065D44 | 1: 0x08065ED0 | 2: 0x08066240 | 3: 0x08066280 | 4: 0x08066460 (`Link_CommandExchangeStep`) |
  5: 0x08066534 | 6: 0x080665B0 | 7..12: 0x08066574, 0x080665FC, 0x0806668C, 0x080666C8, 0x080666F4, 0x08066714.
- **Candidato provável (não confirmado)**: handler 1 (0x08065ED0) chama `Count_Entries3InTable9` (0x080440B4) e
  `Check_PairsAllEqual` (0x080440D4), que varrem **0x03004C40** (inventário, confirmado pelo literal). Parece checagem
  de dados do inventário antes de uma troca. Handler 6 (0x080665B0) chama `FightPreview_SetSceneMode` (0x08044C38),
  que grava `gMainState` e `gamestate_menu`: transição de cena após o comando. Precisa do emulador para confirmar.
- Veredito: **troca por link não confirmada**. O caminho mais provável é a tabela de comandos 0x080AFCC0 com o
  inventário em 0x03004C40. Não há função com "troca" no nome.

## 4. Comando 0xEF00 e leitura de SIOCNT (item 4)

- `FUN_08066460` (confirmado no decompile): se `obj+0x7A == 0`: pai (`0x03000B90 == 0`) grava `0xEF00 | obj+5` no
  halfword de envio; filho lê o halfword recebido, e se o byte alto é 0xEF guarda o byte baixo em `obj+5` e liga
  `obj+0x7A`. Timer 0xFE0F: se `obj+5 == 4`, vai ao estado 8 e chama `Obj_ResetStateFields`; senão
  `Obj_InitTimerRecord(..., 0xC0)` e avança 2. Confirmado. Está na tabela 0x080AFCC0 no índice 4 (§3).
  Renomeado para **`Link_CommandExchangeStep`**.
- `Screen_ReadTwoBitField` 0x0805BB0C (confirmado): lê **SIOCNT** (0x04000128) e grava `(SIOCNT & 0x3F) >> 4` no byte
  **0x03000B90** (slot local). Ou seja, lê o **ID do aparelho** (bits 4-5), não "bits de status". Renomeado para
  **`Link_ReadSioPlayerId`** (o nome sugerido `Link_ReadSioStatusBits` não bate com o que a função faz).

## 5. Renomeados no Ghidra (nesta rodada)

- `FUN_08066460` -> `Link_CommandExchangeStep` (confirmado).
- `Screen_ReadTwoBitField` (0x0805BB0C) -> `Link_ReadSioPlayerId` (confirmado: SIOCNT bits 4-5 -> 0x03000B90).
- Não renomeado: `Scene_GsDispatch_Menu10_Link` (já tinha nome). Os demais estão só no CSV.

## 6. Pendente

- Função do comando 0x080AFCC0: quem indexa (`obj+5`?) e o que fazem os handlers 2, 3, 5, 7..12. Não lido por completo.
- Handlers (função desde o sync de 2026-10-14 nos casos 0x08065790 = LinkMenu15Step0, 0x08065A80 = LinkMenu15Step2, 0x08065C98 = LinkMenu15Step6, 0x08065CFC = LinkMenu15Step7; menu 10 passos 1..2,5..13 e outros em `LK2_lista.txt`, não conferidos):
  criar a função é trabalho do MedabotsSync (fora deste escopo).
- Troca de peça/medalha: confirmar com emulador (handler 1 do comando, varrer 0x03004C40). Não feito, por regra.
- Confirmar no emulador o significado de 0xFE0F e de cada passo do menu 10 (regra: sem emulador nesta rodada).
