# Handlers de opcode de evento 0x23..0x45 (HE2)

Tabela primária `0x083D222C + 4*op` (confirmada na ROM EU; ponteiros com bit Thumb). Lista: `HE2_lista.txt`.
REFEITO com o decompilador do Ghidra (depois de CreateHandlerFunctions): todas as 35 seções abaixo foram lidas por
`decompile_function_by_address`, com os literais do pool conferidos na ROM EU. A versão anterior (capstone) foi
substituída. Nomes no Ghidra: `Event_Op<XX>_<Nome>` (0x24..0x41 já tinham sido renomeados pela rodada HN6; esta rodada
renomeou 0x23, 0x30, 0x42, 0x43, 0x44, 0x45).

Abreviação: `E` = 0x030057E0 (estado do script de evento). Campos usados aqui:
`E+0xC88` = evento rodando, `E+0xC89` = pendente (1 = rodar a continuação nos quadros seguintes),
`E+0xC8A` = cursor do script (halfword), `E+0xC8C` = cursor salvo, `E+0xC8E` = gCurrentEventId,
`E+0xC92` = opcode atual, `E+0xC93` = zerado a cada opcode, `E+0xC94..` = argumentos (gEventScriptArguments),
`0x03006D20` = bloco de argumentos do executor de ator (`[10]` = slot de render = slot de objeto + 0x4F).

Despacho (confirmado no Ghidra):
- `Script_DispatchHandler` (0x0804B6F0): lê o opcode (`site_GetEventScript` 0x0804D83C) para E+0xC92, zera E+0xC89 e
  E+0xC93 e chama `table[op]` em 0x083D222C ("primário": lê os argumentos e faz o efeito imediato).
- `FUN_0804A220` (laço do overworld): se E+0xC88 != 0 e E+0xC89 == 0 chama o despachante; se E+0xC89 != 0 chama
  `FUN_0804DC0C`, que despacha `table2[E+0xC92]` na tabela de CONTINUAÇÃO `0x083D2630 + 4*op` (achada nesta rodada).
- Consequência: um primário que não liga E+0xC89 nunca roda a continuação (caso do 0x42).
- Continuações de 0x23..0x45 (tabela 0x083D2630; nomes do mtbe, código NÃO lido: não são funções no Ghidra):
  0x23 0x0804E51C, 0x24 0x0804E528, 0x26 0x0804E58C, 0x27 0x0804E5A8, 0x28 0x0804E5BC, 0x29 0x0804E5D0,
  0x2A 0x0804E5E0, 0x2B 0x0804E700, 0x2D 0x0804E784 (mtbe: EventOpcode2D_RotateActor), 0x2E/0x2F/0x30 no-op,
  0x31 0x0804E7E8, 0x32 0x0804E860, 0x33 0x0804E8C4, 0x36 0x0804E928, 0x37 0x0804EA18, 0x38 0x0804EB0C,
  0x39 0x0804EC38, 0x3A 0x0804ED88, 0x3B 0x0804EE00, 0x3C 0x0804EE8C, 0x3D 0x0804EF5C, 0x40 0x0804EFFC,
  0x41 0x0804F0A4, 0x42 0x0804F110 (MovePlayerOneStep, inalcançável), 0x43 0x0804F3F8 (ShakeCamera);
  0x25, 0x2C, 0x34, 0x35, 0x3E, 0x3F, 0x44, 0x45 apontam para `NoOp` 0x0804DC34.

Tamanho = bytes de argumento lidos pelo primário (sem contar o opcode). Marca PC: grep em `pc/extract/extract_events.py`
(decode_fx/BRANCH_OPS) e `pc/game/src/script.lua`.

## 0x23 Move_Actor_A (Event_Op23_MoveActorA, 0x0804C130)
- O que faz: `Script_ReadBytes(2)` (slot, move) em E+0xC94; `0x03006D20[10]` = `Map_GetTrackedObjectSlotByIndex(slot)`
  + 0x4F; E+0xC89 = 1. O movimento roda na continuação 0x0804E51C (mtbe: usa Event_RunActorStepSequence).
- Args: 2 bytes. C# bate. PC: implementado (extract `actor_move` com 1 byte de Move; script.lua `actorMove`).
- Confiança: confirmado (setup); efeito do movimento provável (continuação não decompilada).

## 0x24 Move_Actor_B (Event_Op24_MoveActorB, 0x0804C168)
- O que faz: corpo idêntico ao 0x23 (2 bytes, slot de render, pendente); a diferença está só na continuação
  (0x0804E528 x 0x0804E51C).
- Args: 2 bytes. C# bate. PC: implementado (mesmo `actor_move` do 0x23). Confiança: confirmado (setup).

## 0x25 Unload_Actor (Event_Op25_UnloadActor, 0x0804C1A0)
- O que faz: lê 1 byte (packed id) para E+0xC94 e chama `Map_RemoveTrackedObjectByPackedId` (0x08050554) na hora.
  Sem pendente (continuação = NoOp).
- Args: 1 byte. C# bate. PC: implementado (`actor_unload`, só o slot = byte & 0xF). Confiança: confirmado.

## 0x26 Flicker_Actor (Event_Op26_FlickerActor, 0x0804C1C0)
- O que faz: lê 2 bytes (slot, frames). `0x03006D20`: [10] = render, [0] = frames, [1] = 0, [2] = 8, [3] = 8
  (ritmo do pisca); E+0xC89 = 1 (continuação 0x0804E58C conta os quadros).
- Args: 2 bytes. C# bate. PC: implementado (`actor_flicker`). Confiança: confirmado.

## 0x27 Actor_Do_Animation (Event_Op27_ActorDoAnimation, 0x0804C210)
- O que faz: lê 3 bytes (slot, anim, frames). Na hora: `Map_SetObjectRenderSlotFacingVariant(render, anim & 0xF)`;
  `0x03006D20[0] = 0`, `[2] = frames`; E+0xC89 = 1 (espera os quadros na continuação).
- Args: 3 bytes. C# bate. "animation_id" é na verdade a variante de virada (4 bits). PC: falta.
- Confiança: confirmado.

## 0x28 Hop_Actor (Event_Op28_HopActor, 0x0804C26C)
- O que faz: lê 1 byte (slot). `0x03006D20+8` (halfword) = Y do objeto (`gObjectSlots[render]+0x10 >> 8`,
  gObjectSlots = 0x03002C30, 0x40 por slot); zera +0x28 do objeto; `FUN_08047BA8(0x70)` = som do pulo
  (FUN_08047BA8 só chama `snd_sfx_request`, ver 0x44); E+0xC89 = 1 (arco do pulo na continuação).
- Args: 1 byte. C# bate. PC: falta. Confiança: confirmado (setup e som).

## 0x29 Move_All_Tracked_Actors_A (Event_Op29_MoveAllTrackedActorsA, 0x0804C2C8)
- O que faz: lê 1 byte para E+0xC94 e liga E+0xC89. Todo o efeito está na continuação 0x0804E5D0 (mtbe:
  EventOpcode29_MoveAllActorsA, usa Event_RunAllTrackedActorsStepSequence) = mover todos os atores com um Move.
  O nome do C# (move_pattern) é coerente com a continuação; o "SetNpcWalkAnimation" antigo do mtbe não tem base.
- Args: 1 byte. C# bate. PC: falta. Confiança: confirmado (setup); efeito provável (nome da continuação).

## 0x2A Move_All_Tracked_Actors_B (Event_Op2A_MoveAllTrackedActorsB, 0x0804C2F0)
- O que faz: corpo idêntico ao 0x29; continuação 0x0804E5E0 (MoveAllActorsB).
- Args: 1 byte. C# bate. PC: falta. Confiança: confirmado (setup); efeito provável.

## 0x2B Initiate_Free_Tracked_Actor (Event_Op2B_InitiateFreeTrackedActor, 0x0804C318)
- O que faz: lê 5 bytes (packed id, sprite, x, y, move). `Map_SetTrackedObjectSlotByIndex(byte0,
  Map_FindFreeTrackedObjectSlot())` (0x0804DBD0 / 0x08050710); `0x03006D20[10]` = render; E+0xC89 = 1
  (spawn + passo na continuação 0x0804E700). Mesmo corpo de 0x6C e 0x6E.
- Args: 5 bytes. C# bate. PC: falta (extract trata só 0x1F/0x21 como `actor_spawn`). Confiança: confirmado (setup).

## 0x2C Set_Actor_Render_Priority (Event_Op2C_SetActorPriority, 0x0804C364)
- O que faz: lê 1 byte para E+0xC94; se == 1, liga o bit 4 de E+0xC5D (0x0300643D, gOverworldEventModeFlags);
  senão desliga. Imediato (continuação = NoOp).
- Args: 1 byte. C# bate. PC: falta. Confiança: confirmado (bit 4 = prioridade de render: provável).

## 0x2D Move_Actor_C (Event_Op2D_MoveActorC, 0x0804C3AC)
- O que faz: corpo idêntico ao 0x24 (2 bytes, render, pendente). A continuação é 0x0804E784, que o mtbe chama
  EventOpcode2D_RotateActor: o segundo byte pode ser direção e não Move. CONFERIR na continuação.
  O nome antigo do mtbe para o primário (SetActorTileOffset) não tem base no código.
- Args: 2 bytes. C# bate no tamanho. PC: falta. Confiança: confirmado (setup); efeito incerto.

## 0x2E Jump_If_No_Spare_Part (Event_Op2E_JumpIfNoSparePart, 0x0804C3E4; C#: Adjust_Value_If_No_Spare_Part)
- O que faz: E+0xC8C = E+0xC8A (cursor logo após o opcode); lê 3 bytes (bot, part, offset);
  `r = FUN_08047D9C(bot, part)` (= `Inv_ReadCounter16`); se byte alto de r == byte baixo (provável: possuídas ==
  equipadas, sem reserva), cursor = salvo + offset (sem sinal). Senão segue. Imediato.
- Args: 3 bytes (tamanho do C# bate). O 3o byte é deslocamento de SALTO, não "amount".
- PC: falta. O extract_events.py não lista 0x2E em BRANCH_OPS: o alvo do salto não é visitado (pode perder código).
- Confiança: confirmado (fluxo); semântica de r provável.

## 0x2F Jump_Table_By_Event_Field (Event_Op2F_JumpTableByEventField, 0x0804C444; C#: Set_Value_From_EventFlag_Nibble)
- O que faz: base = cursor após o opcode; `v = EventField3Bit_GetByPackedId(gCurrentEventId)` (campo de 3 bits do
  PRÓPRIO evento na lista de flags: id ímpar = bits 0-2, par = bits 4-6); lê o byte em base+v;
  cursor = base + byte. Tabela inline de saltos (até 8 bytes), sem fall-through. Imediato.
- Args: VARIÁVEL (tabela inline de v+1 bytes no mínimo). C# diz 2 bytes; ROM_SIZE_FIX do extract diz 2 no total;
  nenhum dos dois é o tamanho real (o extract não usa esse tamanho: trata 0x2F como multijump antes).
- PC: implementado (extract `multijump` com contagem heurística de alvos; script.lua escolhe `to[sel+1]`).
- Confiança: confirmado.

## 0x30 Relative_Long_Jump (Event_Op30_RelativeLongJump, 0x0804C478)
- O que faz: base = cursor após o opcode; lê 2 bytes; cursor = base + (C94<<8 | C95) (Short big-endian, soma em 16
  bits = com sinal). Imediato (continuação no-op).
- Args: 2 bytes. C# bate. PC: implementado (extract kind `always`, alvo = at + 1 + s16; script.lua salta).
- Confiança: confirmado.

## 0x31 Yes_or_No_Box (Event_Op31_YesOrNoBox, 0x0804C4B4)
- O que faz: E+0xC8C = cursor após o opcode; lê 1 byte (salto) para E+0xC94; E+0xCB5 = 0; E+0xC89 = 1.
  A caixa e o salto rodam na continuação 0x0804E7E8.
- Args: 1 byte. C# bate. PC: implementado (`yesno`; alvo = at + 1 + byte; qual lado é Sim é palpite do extract).
- Confiança: confirmado (setup).

## 0x32 No_or_Yes_Box (Event_Op32_NoOrYesBox, 0x0804C4F8)
- O que faz: igual ao 0x31 com E+0xCB5 = 1 (cursor inicial / ordem invertida); continuação 0x0804E860.
- Args: 1 byte. C# bate. PC: implementado (ordem invertida é palpite). Confiança: confirmado (setup).

## 0x33 Start_Battle (Event_Op33_StartBattle, 0x0804C53C)
- O que faz: lê 3 bytes, um a um: id da batalha -> E+0xC2A, modo -> E+0xC2F, pós-batalha -> E+0xC2E. Se
  `FUN_0804B738()` == 0 (stub que devolve 0), E+0xC89 = 1; a batalha começa na continuação 0x0804E8C4.
- Args: 3 bytes. C# bate. PC: implementado (`battle` com o id; bytes de modo ignorados). Confiança: confirmado.

## 0x34 Rotate_NPC (Event_Op34_RotateNpc, 0x0804C588)
- O que faz: lê 3 bytes; chave = (C94<<8)|C95 (id de evento big-endian); `Table64_FindOffsetByKey(chave)`; se
  achou e o bit 0x40 de entry[4] está ligado, `FUN_0805218C((entry[4] & 0x1F) + 0x31, dir = C96)` (variante de
  virada do marcador do evento). Imediato.
- Args: 3 bytes. C# bate. PC: implementado (`face_event` -> host.faceEvent). Confiança: confirmado.

## 0x35 Set_All_Tracked_Actors_Facing_Variant (Event_Op35_SetAllTrackedActorsFacingVariant, 0x0804C5E4)
- O que faz: lê 1 byte; para os slots lógicos 0..3 mapeados cujo halfword `0x030055A0[(render)*4]` != 0, aplica
  `Map_SetObjectRenderSlotFacingVariant(render, variante)`. Imediato.
- Args: 1 byte. C# bate. PC: falta. Confiança: confirmado.

## 0x36 Get_Item (Event_Op36_GetItem, 0x0804C648)
- O que faz: lê 2 bytes (item, qtd); qtd 0 vira 1; E+0xC89 = 1. A soma (e a mensagem) ficam na continuação 0x0804E928.
- Args: 2 bytes. C# bate. PC: implementado (`item` give=true; o PC não trata qtd 0 -> 1).
- Confiança: confirmado (setup); soma no inventário provável (continuação não lida).

## 0x37 Lose_Item (Event_Op37_LoseItem, 0x0804C678)
- O que faz: mesmo corpo do 0x36 (qtd 0 -> 1, pendente); continuação 0x0804EA18.
- Args: 2 bytes. C# bate. PC: implementado (`item` give=false; sem qtd 0 -> 1). Confiança: confirmado (setup).

## 0x38 Get_Part (Event_Op38_GetPart, 0x0804C6A8)
- O que faz: lê 3 bytes (bot, part, qtd) e liga E+0xC89; efeito na continuação 0x0804EB0C.
- Args: 3 bytes. C# bate. PC: implementado (`part`; o 3o byte vira "extra", que pelo C# é a quantidade).
- Confiança: confirmado (setup).

## 0x39 Lose_Part (Event_Op39_LosePart, 0x0804C6C8)
- O que faz: mesmo corpo do 0x38; continuação 0x0804EC38.
- Args: 3 bytes. C# bate. PC: implementado (`part` give=false; remove 1 entrada, ignora a quantidade).
- Confiança: confirmado (setup).

## 0x3A Get_Money (Event_Op3A_GetMoney, 0x0804C6E8)
- O que faz: lê 2 bytes (Short) e liga E+0xC89; efeito na continuação 0x0804ED88.
- Args: 2 bytes. C# bate. PC: implementado (`money`). Confiança: confirmado (setup).

## 0x3B Lose_Money (Event_Op3B_LoseMoney, 0x0804C708)
- O que faz: mesmo corpo do 0x3A; continuação 0x0804EE00.
- Args: 2 bytes. C# bate. PC: implementado (`money`). Confiança: confirmado (setup).

## 0x3C Get_Medal (Event_Op3C_GetMedal, 0x0804C728)
- O que faz: lê 1 byte (medalha) para E+0xC94 e liga E+0xC89; efeito na continuação 0x0804EE8C.
- Args: 1 byte. C# bate. PC: falta. Confiança: confirmado (setup).

## 0x3D Get_Tinpet (Event_Op3D_GetTinpet, 0x0804C750)
- O que faz: mesmo corpo do 0x3C; continuação 0x0804EF5C.
- Args: 1 byte. C# bate. PC: falta. Confiança: confirmado (setup).

## 0x3E Increase_Capped_Event_Counter (Event_Op3E_IncreaseCappedEventCounter, 0x0804C778)
- O que faz: lê 2 bytes (slot, qtd). slot != 0: E+0xCBD += qtd, teto 0x20. slot == 0: E+0xCBC += qtd, mas o teto é
  testado e aplicado em E+0xCBD (bug do jogo: o contador 0 fica sem teto). Imediato.
- Args: 2 bytes. C# bate. PC: falta. Confiança: confirmado.

## 0x3F Decrease_Capped_Event_Counter (Event_Op3F_DecreaseCappedEventCounter, 0x0804C7DC)
- O que faz: lê 2 bytes. slot == 0: E+0xCBC -= qtd; senão E+0xCBD -= qtd; piso 0 (resultado negativo vira 0).
  Imediato.
- Args: 2 bytes. C# bate. PC: falta. Confiança: confirmado.

## 0x40 Pulse_Screen_Transition (Event_Op40_PulseScreenTransition, 0x0804C840)
- O que faz: lê 1 byte (ciclos) para E+0xC94 e liga E+0xC89; o laço de transição roda na continuação 0x0804EFFC
  (mtbe: RunPulsedScreenTransitionLoop).
- Args: 1 byte. C# bate. PC: falta. Confiança: confirmado (setup).

## 0x41 Flicker_All_Tracked_Actors (Event_Op41_FlickerAllTrackedActors, 0x0804C868)
- O que faz: lê 1 byte (frames); para os slots lógicos 0..3 ativos (mesmo teste do 0x35) grava em
  `gObjectSlots[render]` (0x03002C30): +0x26 = frames, +0x27 = 0, +0x28 = 8, +0x29 = 8; E+0xC89 = 1
  (continuação 0x0804F0A4 conta o pisca).
- Args: 1 byte. C# bate. PC: falta. Confiança: confirmado.

## 0x42 NoOp (Event_Op42_NoOp, 0x0804C928; C#: Move_Player_One_Step)
- O que faz: nada (`return`). Como não liga E+0xC89, a continuação 0x0804F110 (mtbe: MovePlayerOneStep) nunca roda a
  partir deste opcode: o nome do C# descreve a continuação inalcançável.
- Args: 0 bytes. C# bate (0). PC: falta (trivial: ignorar). Confiança: confirmado.

## 0x43 Shake_Camera (Event_Op43_ShakeCamera, 0x0804C92C)
- O que faz: `Script_ReadBytes(2)` (eixo, duração) e liga E+0xC89. O tremor roda na continuação 0x0804F3F8 (mtbe:
  EventOpcode43_ShakeCamera, "timed camera shake on X or Y"). O nome antigo do mtbe para o primário
  (ApplyMapSceneModePolicy3) não tem base.
- Args: 2 bytes. C# bate. PC: falta. Confiança: confirmado (setup); efeito provável (nome da continuação).

## 0x44 Play_Sound (Event_Op44_PlaySound, 0x0804C94C)
- O que faz: lê 1 byte e chama `FUN_08047BA8(byte)`, que só repassa para `snd_sfx_request`: toca efeito sonoro.
  Imediato. O nome do C# está CERTO; o nome do mtbe para 0x08047BA8 (Overworld_StartMapTransitionEffect) é que
  está errado (mesma função do som 0x70 do 0x28).
- Args: 1 byte. C# bate. PC: falta (sem som de evento). Confiança: confirmado.

## 0x45 Play_Music (Event_Op45_PlayMusic, 0x0804C960)
- O que faz: lê 1 byte para E+0xC94 e chama `FUN_0804842C(1, byte)`; o caso 1 dessa função grava a música atual em
  E+0xC30 e chama `Sound_RequestSong(byte)`. Imediato. O nome do C# está CERTO.
  (0x0804842C é um seletor de política de música por modo: 2 = parar, 3/4/5.. = música do mapa etc.)
- Args: 1 byte. C# bate. PC: falta. Confiança: confirmado.

## Achados
- Tabela de continuação dos opcodes de evento: `0x083D2630 + 4*op`, despachada por `FUN_0804DC0C` (índice E+0xC92)
  enquanto E+0xC89 != 0. Útil para os outros lotes (HE1/HE3/HE4).
- Divergências de tamanho C# x ROM neste lote: só 0x2F (variável; C# 2 bytes, ROM_SIZE_FIX 2 no total).
  Semântica errada no C# (tamanho certo): 0x2E (3o byte = salto, não quantidade), 0x27 (animation_id = variante de
  virada), 0x42 (nome da continuação inalcançável).
- Corrige HE2 antigo: 0x44 e 0x45 SÃO som e música (as funções chamadas é que têm nome errado no mtbe:
  0x08047BA8 -> wrapper de snd_sfx_request; 0x0804842C -> política de música).
- Para o mtbe.csv (não editado): 0x08047BA8 renomear (ex.: Sound_PlaySfx); 0x0804842C (ex.: Sound_ApplyMusicPolicy);
  primários 0x0804C3AC (SetActorTileOffset) e 0x0804C92C (ApplyMapSceneModePolicy3) com nomes sem base.
- Extract: 0x2E falta em BRANCH_OPS (alvo do salto não é visitado).
