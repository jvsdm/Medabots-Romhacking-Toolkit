# HN6: 3 handlers de batalha (AG / compartilhado) + 38 handlers de opcode de evento (0x24-0x41, 0x6A-0x71)

Lista: `HN6_lista.txt` (41 entradas). Todas lidas no Ghidra (decompile, depois de CreateHandlerFunctions) e
RENOMEADAS no Ghidra com os nomes abaixo. Literais do pool lidos na ROM EU.
Abreviação: `E` = 0x030057E0 (estado do script de evento). Campos: `E+0xC89` = pedido pendente (1 = a parte
"adiada" do opcode roda depois pelo executor), `E+0xC8A` = cursor do script (halfword), `E+0xC8C` = cursor salvo,
`E+0xC94..` = buffer de argumentos (0x03006474), `0x03006D20` = bloco de argumentos do executor de ator
(`[10]` = slot de render). Helpers: `Script_ReadBytes(n)` 0x0804D8CC, `site_GetEventScript` 0x0804D83C (1 byte),
`Map_GetTrackedObjectSlotByIndex` 0x0804DBF0 (slot lógico -> slot de objeto, -1 se vazio; o código grava
resultado+0x4F = slot de render). Os nomes de evento repetem os propostos em HE2/HE4 para não conflitar no merge,
exceto 0x29, 0x2A, 0x2E, 0x2F, 0x6A (corrigidos/novos). Esta rodada confirma pelo decompilador o que HE2/HE4 leram
por capstone.

## Handlers de batalha (bloco fight_information 0x03001800)

## 0x08030780 Fight_DispatchOnSubstateAA_AG_ScrollSlotWaitTimer
- Passo: AG +0xAA = 0x09 (tabela AG 0x083AE0C4). Desloca o halfword `gObjectSlots+0x858` (slot 0x21, campo +0x18,
  provável X) em -0x40 ou +0x40 por frame (conforme bit 0 de 0x03001800+0xF8, lado), com máscara 0x3FF.
- Depois decrementa o timer `0x03000440+0x86`; o timer é pulado se `0x03000440+0x89 == 0` e bit 0 (botão A) de
  `0x03001798` (pressed_buttons). Timer 0 ou pulado: +0xAA++ (vai para 0x0A, `0x08021F90` de U).
- Nota: 0x03000440 está no mtbe como gTextState, mas aqui é usado como bloco de timer da batalha (+0x84/+0x86/+0x89).
- Confiança: confirmado (decompile + literais); significado de +0x18 = X é provável.

## 0x0803080C Fight_DispatchOnSubstateAA_Shared_SetDone
- Passo terminal compartilhado: só faz `0x03001800+0xB5 = 1` (handler concluído). Não mexe em +0xAA.
- Aparece em 48 tabelas (J idx 0x04, AG idx 0x03, e AA..BH, K..Z conforme HN6_lista). Em J é a última entrada
  real (idx 5..7 = 0x08000840, padding).
- Confiança: confirmado (decompile + tabelas J 0x083AD914 e AG 0x083AE0C4 lidas na ROM).

## 0x0803081C Fight_DispatchOnSubstateAA_AG_WaitSlotScriptEnd
- Passo: AG +0xAA = 0x0E. Roda `Anim_RunSlotScript(0x083B6100[0x03000440+0x84], 0x21)` (roteiro do slot 0x21
  escolhido pelo índice +0x84).
- Quando o byte `gObjectSlots+0x862` (slot 0x21 +0x22, estado do roteiro) == 0xFF (fim), +0xAA++ (vai para 0x0F,
  `0x08025104`).
- Confiança: confirmado (decompile + literais).

## Handlers de opcode de evento (tabela 0x083D222C + 4*op)

## 0x0804C168 Event_Op24_MoveActorB
- Lê 2 bytes (slot, move) em E+0xC94; `0x03006D20[10]` = slot de render; E+0xC89 = 1 (movimento roda adiado).
- Corpo idêntico a 0x2D, 0x6D, 0x6F. Confiança: confirmado.

## 0x0804C1A0 Event_Op25_UnloadActor
- Lê 1 byte (packed id) em E+0xC94 e chama `Map_RemoveTrackedObjectByPackedId` (0x08050554) na hora; sem pendente.
- Confiança: confirmado.

## 0x0804C1C0 Event_Op26_FlickerActor
- Lê 2 bytes (slot, frames). `0x03006D20`: [10] = slot de render, [0] = frames, [1] = 0, [2] = 8, [3] = 8
  (ritmo do pisca); E+0xC89 = 1. Confiança: confirmado.

## 0x0804C210 Event_Op27_ActorDoAnimation
- Lê 3 bytes (slot, anim, frames). Aplica `Map_SetObjectRenderSlotFacingVariant(render, anim & 0xF)` na hora;
  `0x03006D20[0] = 0`, `[2] = frames`; E+0xC89 = 1. Confiança: confirmado (anim = variante de virada, 4 bits).

## 0x0804C26C Event_Op28_HopActor
- Lê 1 byte (slot). `0x03006D20+8` (halfword) = Y do objeto (`gObjectSlots[render]+0x10 >> 8`); zera +0x28 do
  objeto; chama `0x08047BA8(0x70)` (mtbe: Overworld_StartMapTransitionEffect; aqui o 0x70 parece efeito sonoro do
  pulo, chute); E+0xC89 = 1. Confiança: confirmado (fluxo); papel do 0x08047BA8 chute.

## 0x0804C2C8 Event_Op29_MoveAllTrackedActorsA
- Lê 1 byte em E+0xC94 e liga E+0xC89. Todo o efeito é adiado (executor não lido): por isso o nome do C#
  (move_pattern) e o do mtbe (SetNpcWalkAnimation) não podem ser decididos aqui. Confiança: confirmado (setup).

## 0x0804C2F0 Event_Op2A_MoveAllTrackedActorsB
- Mesmo corpo de 0x29 (1 byte em E+0xC94, pendente). Confiança: confirmado (setup); efeito não lido.

## 0x0804C318 Event_Op2B_InitiateFreeTrackedActor
- Lê 5 bytes (packed id, sprite, x, y, move). Aloca slot (`Map_FindFreeTrackedObjectSlot` 0x08050710) e mapeia
  (`Map_SetTrackedObjectSlotByIndex` 0x0804DBD0, slot lógico = byte 0); `0x03006D20[10]` = render; E+0xC89 = 1.
- Corpo idêntico a 0x6C e 0x6E. Confiança: confirmado.

## 0x0804C364 Event_Op2C_SetActorPriority
- Lê 1 byte em E+0xC94; se == 1, liga o bit 4 de E+0xC5D; senão desliga. Imediato (sem pendente).
- Confiança: confirmado (o bit 4 de E+0xC5D como prioridade de render é provável).

## 0x0804C3AC Event_Op2D_MoveActorC
- Mesmo corpo de 0x24 (2 bytes, render em `0x03006D20[10]`, pendente). O nome do mtbe (SetActorTileOffset) não
  tem base no setup. Confiança: confirmado.

## 0x0804C3E4 Event_Op2E_JumpIfNoSparePart
- Salva o cursor (E+0xC8C = E+0xC8A); lê 3 bytes (bot, part, offset). `r = 0x08047D9C(bot, part)`; se byte alto
  == byte baixo de `r` (provável: possuídas == equipadas, sem reserva), cursor = salvo + offset.
- É um salto condicional, não "ajuste de valor" (corrige o nome do C# / HE2). Confiança: confirmado (fluxo);
  semântica de `r` provável.

## 0x0804C444 Event_Op2F_JumpTableByEventField
- `v = EventField3Bit_GetByPackedId(halfword E+0xC8E)`; avança o cursor em `v`, lê 1 byte e faz
  cursor = cursor_original + byte. Tabela de saltos de bytes indexada por um campo de 3 bits.
- Explica a divergência de HE2: o tamanho é variável (tabela inline), como o extract_events.py já trata (multijump).
- Confiança: confirmado (decompile); origem de E+0xC8E não lida.

## 0x0804C4B4 Event_Op31_YesOrNoBox
- Salva cursor (C8A -> C8C); lê 1 byte (jump) em E+0xC94; E+0xCB5 = 0 (ordem Sim/Não); E+0xC89 = 1.
- Confiança: confirmado.

## 0x0804C4F8 Event_Op32_NoOrYesBox
- Igual ao 0x31 com E+0xCB5 = 1 (ordem invertida). Confiança: confirmado.

## 0x0804C53C Event_Op33_StartBattle
- Lê 3 bytes: id da batalha -> E+0xC2A, modo -> E+0xC2F, pós-batalha -> E+0xC2E. Se `0x0804B738()` == 0
  (stub que sempre devolve 0), E+0xC89 = 1. Confiança: confirmado.

## 0x0804C588 Event_Op34_RotateNpc
- Lê 3 bytes; chave = (C94<<8)|C95 (id de evento big-endian); `Table64_FindOffsetByKey`; se a entrada existe e
  bit 0x40 de [4] está ligado, `Map_SetEventMarkerFacingVariant((entry[4]&0x1F)+0x31, dir=C96)`. Imediato.
- Confiança: confirmado.

## 0x0804C5E4 Event_Op35_SetAllTrackedActorsFacingVariant
- Lê 1 byte; para os slots lógicos 0..3 mapeados cujo halfword `0x030055A0[(render)*4]` != 0, aplica a variante
  (`Map_SetObjectRenderSlotFacingVariant`). Imediato. Confiança: confirmado.

## 0x0804C648 Event_Op36_GetItem
- Lê 2 bytes (item, qtd); qtd 0 vira 1; E+0xC89 = 1 (soma no inventário é adiada). Confiança: confirmado.

## 0x0804C678 Event_Op37_LoseItem
- Mesmo corpo de 0x36 (qtd 0 -> 1, pendente). Confiança: confirmado.

## 0x0804C6A8 Event_Op38_GetPart
- Lê 3 bytes (bot, part, qtd) e liga E+0xC89. Efeito adiado. Confiança: confirmado (setup).

## 0x0804C6C8 Event_Op39_LosePart
- Mesmo corpo de 0x38. Confiança: confirmado (setup).

## 0x0804C6E8 Event_Op3A_GetMoney
- Lê 2 bytes (Short) e liga E+0xC89. Efeito adiado. Confiança: confirmado (setup).

## 0x0804C708 Event_Op3B_LoseMoney
- Mesmo corpo de 0x3A. Confiança: confirmado (setup).

## 0x0804C728 Event_Op3C_GetMedal
- Lê 1 byte em E+0xC94 e liga E+0xC89. Confiança: confirmado (setup).

## 0x0804C750 Event_Op3D_GetTinpet
- Mesmo corpo de 0x3C. Confiança: confirmado (setup).

## 0x0804C778 Event_Op3E_IncreaseCappedEventCounter
- Lê 2 bytes (slot, qtd). slot != 0: E+0xCBD += qtd, teto 0x20. slot == 0: E+0xCBC += qtd, mas o teto é testado e
  aplicado em E+0xCBD (bug do jogo: o contador 0 fica sem teto). Imediato.
- Confiança: confirmado (disassembly conferido).

## 0x0804C7DC Event_Op3F_DecreaseCappedEventCounter
- Lê 2 bytes. slot == 0: E+0xCBC -= qtd; senão E+0xCBD -= qtd; piso 0 (sem o bug do 0x3E). Imediato.
- Confiança: confirmado.

## 0x0804C840 Event_Op40_PulseScreenTransition
- Lê 1 byte (ciclos) em E+0xC94 e liga E+0xC89. Efeito adiado. Confiança: confirmado (setup).

## 0x0804C868 Event_Op41_FlickerAllTrackedActors
- Lê 1 byte (frames); para os slots lógicos 0..3 ativos grava em `gObjectSlots[render]`: +0x26 = frames, +0x27 = 0,
  +0x28 = 8, +0x29 = 8 (mesmos parâmetros do 0x26); E+0xC89 = 1. Confiança: confirmado.

## 0x0804D204 Event_Op6A_NoOp
- Só `bx lr` (0 bytes). Sem definição no Toolkit. Confiança: confirmado.

## 0x0804D208 Event_Op6B_Set_Current_Chapter
- Lê 1 byte e grava em E+0x7FF (0x03005FDF). Imediato. Confiança: confirmado.

## 0x0804D224 Event_Op6C_Initiate_Moving_Actor_C
- Mesmo corpo de 0x2B (5 bytes, aloca e mapeia slot, render, pendente). Confiança: confirmado.

## 0x0804D270 Event_Op6D_Move_Actor_D
- Mesmo corpo de 0x24 (2 bytes, render, pendente). Confiança: confirmado.

## 0x0804D2A8 Event_Op6E_Initiate_Moving_Actor_D
- Mesmo corpo de 0x2B. Confiança: confirmado.

## 0x0804D2F4 Event_Op6F_Move_Actor_E
- Mesmo corpo de 0x24. Confiança: confirmado.

## 0x0804D32C Event_Op70_Run_Scene_Command_13
- Só E+0xC89 = 1 (0 bytes); o comando é decidido pelo opcode no executor adiado. Confiança: confirmado (setup).

## 0x0804D340 Event_Op71_Run_Scene_Command_00
- Mesmo corpo de 0x70. Confiança: confirmado (setup).

## Achados
- Padrão dos eventos: setup lê args para E+0xC94 e liga E+0xC89; o efeito real fica na tabela de continuação
  (ex.: mtbe 0x0804E7E8 para 0x31). Imediatos (sem pendente): 0x25, 0x2C, 0x2E, 0x2F, 0x34, 0x35, 0x3E, 0x3F, 0x6B.
- Grupos de corpo idêntico: {0x24, 0x2D, 0x6D, 0x6F}, {0x2B, 0x6C, 0x6E}, {0x29, 0x2A, 0x3C, 0x3D, 0x40},
  {0x36, 0x37}, {0x38, 0x39}, {0x3A, 0x3B}, {0x70, 0x71}.
- Bug do jogo em 0x3E (teto aplicado no contador errado).
- 0x08047BA8 chamado com 0x70 no Hop: o nome do mtbe (transição de mapa) pode estar errado; HE2 viu a mesma função
  no 0x44 (Play_Sound do Toolkit), o que reforça "toca efeito sonoro".
