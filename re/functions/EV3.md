# EV3: continuações dos opcodes de evento (Get/Lose, StartBattle, Jump C76, FacePlayer)

Tabelas: primária 0x083D222C, continuação 0x083D2630 (4 B por opcode, índice = opcode). Símbolos: EV3_simbolos.csv.
Base do bloco de evento E = 0x030057E0.

## 1. Pares Get/Lose: a continuação confirma a subtração

| Opcode | Continuação | Efeito | Destino |
|---|---|---|---|
| 0x36 GetItem | Event_Op36_Cont_GetItem 0x0804E928 | soma, teto 99 | mapa 0x03002BA0 |
| 0x37 LoseItem | Event_Op37_Cont_LoseItem 0x0804EA18 | **subtrai** (min com a contagem) | mapa 0x03002BA0 |
| 0x38 GetPart | Event_Op38_Cont_GetPart 0x0804EB0C | soma, teto 99 | peças 0x03004C40 |
| 0x39 LosePart | Event_Op39_Cont_LosePart 0x0804EC38 | **subtrai** (piso 0) | peças 0x03004C40 |
| 0x3A GetMoney | Event_Op3A_Cont_GetMoney 0x0804ED88 | soma (teto 99999) | dinheiro 0x03002B70 |
| 0x3B LoseMoney | Event_Op3B_Cont_LoseMoney 0x0804EE00 | **subtrai** (limitado ao saldo) | dinheiro 0x03002B70 |

Confirmado no decompile da continuação:
- Item: `Map64_SubDefault1_Wrapper` -> `Map64_SubRemoveIfZero`. Se a contagem for 0, mostra a mensagem de falta e não subtrai.
- Peça: `Inv_SubCountClampZeroTail` -> `Inv_SubCountClampZero` com endereço `0x03004C40 + slot*0xF0 + bot*2` (pool 0x0804219C = 0x03004C40). Contagem 0 = mensagem de falta.
- Dinheiro: `SubClampedCounter` em `*0x08047D84 = 0x03002B70`, piso 0. Valor = u16 BE de 2 bytes do script × 10, limitado ao saldo.
- Ganho: `AddClampedCounter` em 0x03002B70 com teto `0x0001869F` = 99999 (pool 0x08047D70).

Os primários (Op36..Op3B) só leem argumentos e marcam o estado. Eles não são renomeados.

## 2. Op33 StartBattle (continuação = Event_Op33_Cont_StartBattle 0x0804E8C4)

- Passo 0: `Event_ResetSceneCommandStep`.
- Passo 1: `Event_RunSceneCommandSequence(0x20, 0)` roda até retornar não-zero. Depois copia `E+0xC4F` para `0x03005310+0x39` se o valor for 1 ou 2, e chama `Event_EndOpcode`.
- A batalha não é iniciada direto pelo opcode. A sequência de cena do comando 0x20 faz isso:
  - Passo 0 da sequência: só avança para o passo 1 (cmd 0x20 não faz transição).
  - Passo 1 (0x08050D40): `Overworld_BuildEncounterNpcSnapshot` e depois **`GameState_StartFlowByCommand(0x20, arg)` (0x08047878)**. O CSV diz que o 0x20 inicia o fluxo de batalha forçada e marca `gCurrentBattleSourceFlag = 1`.
  - Passo 2: configuração de VRAM/OAM/DMA, e o handler da tabela de 33 entradas do cmd 0x20 (0x08050EF0: `mov r0,#0xD`) chama `SND_PolicyRequest(0xD, 0)` (0x0804842C).
  - Passo 4: espera a transição de tela (`Overworld_RunScreenTransitionStep`, 0x08049E08) terminar.
- O que grava `E+0xC4F` (1 = ?, 2 = ? , que o Op4F lê como `estado+0x39 == 2`) não foi confirmado. Ficou aberto.
- Renomeado: `EventOpcode33_StartBattle_Continuation` -> `Event_Op33_Cont_StartBattle`.

## 3. Op88 Jump_If_Flag_C76_Clear (primário 0x0804D7F8)

- O teste da flag C76 está no PRIMÁRIO. Confirmado: `if (E[0xC76] == 0)` então `E[0xC8A] = ptr_antes_operandos + offset`, onde o offset é BE16 em `E+0xC94` (lido com `Script_ReadBytes(2)`).
- A continuação de 0x88 é `Event_Op00_Cont_NoOp` (0x0804DC34, só `return`). Não há teste na continuação.
- Nome mantido: `Event_Op88_Jump_If_Flag_C76_Clear` (já era correto).

## 4. Op1B EventNpcFacePlayer (primário 0x0804BE50)

- A direção vem do JOGADOR, passando por uma tabela da ROM, e não de argumento do script nem de tabela fixa.
- `dir = *(u8*)0x03003811` (gPlayerFaceDirection, CSV). O facing enviado a `Map_SetEventMarkerFacingVariant` é `*(u8*)(0x083D262C + dir)`.
- O objeto é escolhido por `E+0xCB4` (índice × 8) e só recebe o facing se `(u16 @ E+idx*8+0x5DE) & 0xFFF == u16 @ E+0xC8E`.
- A continuação é `Event_Op00_Cont_NoOp`. Não há efeito adicional.
- Nome mantido. Comentário: o nome `FacePlayer` descreve bem o caso, porque copia a direção do jogador.

## Renomeações (só continuações, feitas no Ghidra)

- 0x0804E8C4 `EventOpcode33_StartBattle_Continuation` -> `Event_Op33_Cont_StartBattle`
- 0x0804EA18 `EventOpcode37_LoseItem` -> `Event_Op37_Cont_LoseItem`
- 0x0804EB0C `EventOpcode38_GetPart` -> `Event_Op38_Cont_GetPart`
- 0x0804EC38 `EventOpcode39_LosePart` -> `Event_Op39_Cont_LosePart`
- 0x0804ED88 `EventOpcode3A_GetMoney` -> `Event_Op3A_Cont_GetMoney`
- 0x0804EE00 `EventOpcode3B_LoseMoney` -> `Event_Op3B_Cont_LoseMoney`

Não renomeados: Event_Op36_Cont_GetItem (já estava certo) e os primários.

## Em aberto
- Quem grava `E+0xC4F` depois do cmd 0x20 e o significado de 1 e 2.
- Op36/Op37 usam o mapa 0x03002BA0 (Map64). O endereço vem do CSV (R2, confirmado), não foi lido de pool nesta rodada.
- O mapa de cena do passo 4 (transição) depende de `Overworld_RunScreenTransitionStep`; não foi reavaliado.
