# HE1 - handlers de opcode de evento 0x00..0x22 (tabela primaria)

Status: COMPLETO (redo GH-HE1). Os 35 handlers foram decompilados no Ghidra (funcoes criadas por CreateHandlerFunctions)
e renomeados `Event_Op<XX>_<Nome>`. Literais do pool lidos na ROM EU (Medabots - Metabee (Europe).gba).

## Como o despachante funciona (confirmado)
- Bloco de estado do evento: base **0x030057E0** (mesma base do `event_flag_list`). Offsets usados aqui:
  +0xC88 = 0x03006468 gCurrentEventPendingFlag, **+0xC89 = 0x03006469 gCurrentEventStateByte** (0 = fase primaria,
  1 = fase de continuacao), +0xC8A = 0x0300646A cursor do script (u16), +0xC8E = 0x0300646E gCurrentEventId,
  +0xC92 = 0x03006472 opcode atual, +0xC93 = 0x03006473 subestado do opcode, **+0xC94.. = 0x03006474 gEventScriptArguments**,
  +0xCB4 = 0x03006494 indice do ActiveMapObjectEntry do evento atual. Tabela de objetos ativos: 0x030057E0+0x5DC
  (0x03005DBC), 8 bytes por entrada; +0x02 u16 id (12 bits baixos = id do evento), +0x04 byte render_slot(&0x1F)|flags
  (0x80 ativo, 0x40 slot par).
- `EventScript_RunCurrent` 0x0804A220: se Pending != 0 e StateByte == 0 chama `Script_DispatchHandler` 0x0804B6F4 (fase primaria);
  se StateByte != 0 chama 0x0804DC0C (fase de continuacao).
- `Script_DispatchHandler` 0x0804B6F4: le o byte do opcode (`site_GetEventScript` = EventScript_ReadNextByte), grava em +0xC92,
  zera +0xC89 e +0xC93 e chama `CallViaR0(0x083D222C[op])` (literal 0x0804B734 = 0x083D222C, confirmado).
- **Tabela de continuacao: 0x083D2630** (literal 0x0804DC28), indexada por +0xC92. Para 0x00..0x22:
  00/06/0D/12..19/1B/1C/21/22 -> 0x0804DC34 (comum; provavelmente "sem continuacao"); 01 0x0804DC38, 02 0x0804DCF8,
  03 0x0804DD04, 04 0x0804DD18, 05 0x0804DD3C, 07 0x0804DD5C, 08 0x0804DD70, 09 0x0804DD84, 0A 0x0804DD9C, 0B 0x0804DDB4,
  0C 0x0804DDC0, 0E 0x0804DE74, 0F 0x0804DF40, 10 0x0804E134, 11 0x0804E370, 1A 0x0804E43C, 1D 0x0804E450, 1E 0x0804E464,
  1F 0x0804E480, 20 0x0804E48C. Varios NAO sao funcoes no Ghidra (ex.: 0x0804DC34, 0x0804E480, 0x0804E48C: "No function found").
- Padrao: handler "imediato" nao mexe em +0xC89, entao o proximo opcode roda no tick seguinte. Handler "diferido" le os args
  para gEventScriptArguments e grava +0xC89 = 1; o efeito visivel (texto, fade, pulo, movimento) fica na continuacao.
- `Script_ReadBytes(n)` 0x0804D8CC (mtbe EventScript_ReadBytes) le n bytes do script para 0x03006474[0..n-1].
- Ids de evento (Short) sao big-endian: `CONCAT11(args[0], args[1])`.
- ERRO no mtbe: `symbols/mtbe.csv` diz "tabela 0x083D224C" (desvio de 0x20). A base certa e 0x083D222C.
- ERRO no mtbe: 0x08047BA8 "Overworld_StartMapTransitionEffect" e so um wrapper de `snd_sfx_request` (efeito sonoro).
  0E e 1D chamam com 0x70 = som do pulo.
- Tamanhos: os 35 batem com EventOpcodeDefinitions.cs e com o load_opcodes do extract_events.py (ROM_SIZE_FIX nao toca
  0x00..0x22). 06 e 19 nao tem definicao no C#; medido no codigo: 06 = 0 bytes, 19 = 2 bytes.

## Secoes (uma por opcode)

## 0x0804B73C 00 Nop (Event_Op00_Nop)
- O que faz: `return;` puro. Imediato. Args: 0 (C#: 0). PC: implementado (pulado por tamanho, sem efeito). Confianca: confirmado.

## 0x0804B740 01 Show_Message_A (Event_Op01_ShowMessageA)
- O que faz: chama `Event_ReadMessageArgsAndDefer` 0x0804B758 = `Script_ReadBytes(3)` + StateByte=1. Os 3 bytes (bank, id BE)
  vao para 0x03006474; a caixa de texto abre na continuacao 0x0804DC38. Diferido.
- Args: 3 (C#: EventBank + Short = 3). PC: implementado (`msg`, bank = byte-2). Confianca: confirmado (tamanho e fluxo; continuacao nao lida).

## 0x0804B74C 02 Show_Message_B (Event_Op02_ShowMessageB)
- O que faz: identico a 0x01 (mesmo helper 0x0804B758); a diferenca A/B esta so na continuacao (0x0804DCF8 vs 0x0804DC38). Diferido. A continuacao 0x0804DCF8 so chama Event_EndOpcode (0x08050290), sem fase de caixa (HC1.md); a caixa do 0x02 fica a confirmar no emulador.
- Args: 3 (C#: 3). PC: implementado (`msg`). Confianca: confirmado (primaria); diferenca A/B nao lida.

## 0x0804B778 03 Close_Message_Box (Event_Op03_CloseMessageBox)
- O que faz: so grava StateByte=1; o fechamento acontece na continuacao 0x0804DD04. Diferido.
- Args: 0 (C#: 0). PC: parcial (extrator grava `close`; nao ha branch `close` no script.lua). Confianca: confirmado (primaria).

## 0x0804B78C 04 Wait_X_Frames (Event_Op04_WaitXFrames)
- O que faz: le 1 byte para 0x03006474 (frames) e grava StateByte=1; a contagem fica na continuacao 0x0804DD18. Diferido.
- Args: 1 (C#: 1). PC: implementado (`wait_frames`). Confianca: confirmado (primaria).

## 0x0804B7B4 05 Wait_For_Button_Press (Event_Op05_WaitForButtonPress)
- O que faz: so grava StateByte=1; a espera pelo botao fica na continuacao 0x0804DD3C. Diferido.
- Args: 0 (C#: 0). PC: parcial (extrator grava `wait`; script.lua sem branch para `wait`). Confianca: confirmado (primaria).

## 0x0804B7C8 06 End (Event_Op06_End; sem definicao no C#)
- O que faz: encerra o evento: zera Pending (+0xC88), StateByte (+0xC89), +0xC93, o cursor (+0xC8A u16), o u16 em +0xC8E
  (event id), +0xC5F e +0xC60; zera 0x03006D3E e 0x03005348; `set_zeros(0x03006484, 0x10)` e `set_zeros(0x03006D20, 0x10)`;
  grava 1 em 0x03005815 (0x03002C30+0xBE5) e chama 0x0805196C. Imediato.
- Args: 0. PC: implementado (`end`). Confianca: confirmado.

## 0x0804B858 07 Fade_To_Black (Event_Op07_FadeToBlack)
- O que faz: le 1 byte (frames) para 0x03006474, prende em 1..16 (`<2 -> 1`, `>15 -> 16`), converte para float e chama
  `Overworld_ResetAndStartScreenTransitionEffect(cor=0, 0.0, 32.0, duracao)` 0x08049D90; StateByte=1 (espera na continuacao 0x0804DD5C).
- Args: 1 (C#: 1). PC: implementado (`fade`, to_black; o PC deve prender frames em 1..16). Confianca: confirmado.

## 0x0804B8AC 08 Fade_From_Black (Event_Op08_FadeFromBlack)
- O que faz: igual a 0x07, mas `Overworld_StartScreenTransitionEffect(0, 32.0, 0.0, duracao)` 0x08049D18 (sem reset). StateByte=1.
- Args: 1 (C#: 1). PC: implementado (`fade`). Confianca: confirmado.

## 0x0804B900 09 Fade_To_White (Event_Op09_FadeToWhite)
- O que faz: igual a 0x07 com cor 0x7FFF (branco): `0x08049D90(0x7FFF, 0.0, 32.0, duracao)`. StateByte=1.
- Args: 1 (C#: 1). PC: implementado (`fade`, to_white). Confianca: confirmado.

## 0x0804B958 0A Fade_From_White (Event_Op0A_FadeFromWhite)
- O que faz: `0x08049D18(0x7FFF, 32.0, 0.0, duracao)`, frames presos em 1..16. StateByte=1.
- Args: 1 (C#: 1). PC: implementado (`fade`, from_white). Confianca: confirmado.

## 0x0804B9B0 0B Warp_A (Event_Op0B_WarpA)
- O que faz: grava 1 em 0x0300643B (gCurrentMapSceneVariant) e chama `Script_ReadStateBlock` 0x0804B9E8, que le 5 bytes:
  b0 -> 0x03005FDE gCurrentMapId, b1 -> 0x03005FDC, b2 -> 0x03005FDD, b3 -> 0x03006436 (tile x), b4 -> 0x03006437 (tile y);
  copia 0x03003810 (direcao do jogador) para 0x03006447 e grava StateByte=1. A recarga do mapa fica na continuacao 0x0804DDB4.
- Args: 5 = map_id, unk1, unk2, x, y (C#: 5). A unica diferenca A/B na primaria e a scene variant (A=1, B=0).
- PC: implementado (`warp`; unk1/unk2 nao decodificados; scene variant nao aplicada). Confianca: confirmado.

## 0x0804B9CC 0C Warp_B (Event_Op0C_WarpB)
- O que faz: igual a 0x0B, mas grava 0 em 0x0300643B (scene variant). Continuacao 0x0804DDC0.
- Args: 5 (C#: 5). PC: implementado (`warp`, variant B). Confianca: confirmado. (O "ConfigFlag" do mtbe e esta scene variant.)

## 0x0804BA5C 0D Player_Face_Direction (Event_Op0D_PlayerFaceDirection)
- O que faz: le 1 byte (dir) para 0x03006474 e 0x03003811; `Map_SetObjectAnimationVariant(0x2F, dir)` (slot 0x2F = jogador),
  grava dir em 0x03003810 (direcao do jogador) e chama `Overworld_SetPlayerFacingAndRefreshFollower(dir)`. Imediato.
- Args: 1 (C#: 1). PC: implementado (`face_player`; ordem N/S/W/E ainda GUESS no extrator; a tabela 0x083D262C = 01 00 03 02
  mostra que 0<->1 e 2<->3 sao opostos). Confianca: confirmado.

## 0x0804BA9C 0E Player_Hop_In_Place (Event_Op0E_PlayerHopInPlace)
- O que faz: nao le args. Guarda (u8) do u16 em 0x030037F4 (Y do jogador, provavel) em 0x03006474, zera 0x03003816 e 0x03003818,
  toca o som 0x70 (0x08047BA8 -> snd_sfx_request) e grava StateByte=1. A continuacao 0x0804DE74 faz 16 quadros de arco
  (Y = base - tabela[i*0x10]/16) no slot 0x2F e encerra. Diferido.
- Args: 0 (C#: 0). PC: falta. Confianca: confirmado (pulo no lugar; o nome do mtbe "StartMapTransition" esta errado).

## 0x0804BAEC 0F Player_Hop_In_Direction (Event_Op0F_PlayerHopInDirection)
- O que faz: le 1 byte (dir) para 0x03006474 e grava StateByte=1; o pulo com deslocamento fica na continuacao 0x0804DF40
  (mtbe: EventOpcode0F_PlayerHopInDirection). Diferido.
- Args: 1 (C#: 1). PC: falta. Confianca: confirmado (primaria); provavel (efeito, pela continuacao no mtbe).

## 0x0804BB14 10 Move_Player (Event_Op10_MovePlayer)
- O que faz: `Script_ReadBytes(4)`; zera 0x03002B64; copia 8 bytes de 0x03006400 para 0x030063F0 e zera 0x030063F0..+8;
  zera 0x03003818 e 0x03003816/17; grava 1 em 0x0300643F; `0x080500BC(0x2F, args[passo], 0x083D2A30[0x0300643C])`;
  `Overworld_SetPlayerFacingAndRefreshFollower(0x03003810)`; StateByte=1 (via 0x030063F0+0x79). Passos na continuacao 0x0804E134.
- Args: 4 Move (nibble alto = dir, baixo = distancia) (C#: 4). PC: implementado (`move_player`). Confianca: confirmado.

## 0x0804BBA0 11 Change_Player_Costume (Event_Op11_ChangePlayerCostume)
- O que faz: le 1 byte (costume_id) para 0x03006474 e grava StateByte=1; a troca de visual/paletas fica na continuacao 0x0804E370.
- Args: 1 (C#: 1). PC: falta. Confianca: confirmado (primaria).

## 0x0804BBC8 12 Add_Event_Permanently (Event_Op12_AddEventPermanently)
- O que faz: `Script_ReadBytes(2)` e `EventFlag_ClearIndexedFlagByPackedId(id BE)` (limpa o bit "removido" do evento). Imediato.
- Args: 2 (C#: Short). PC: implementado (`visible` True, permanente). Confianca: confirmado (codigo); provavel (semantica "visivel").

## 0x0804BBF4 13 Remove_Event_Permanently (Event_Op13_RemoveEventPermanently)
- O que faz: `Script_ReadBytes(2)` e `EventFlag_SetIndexedFlagByPackedId(id BE)`. Imediato.
- Args: 2 (C#: Short). PC: implementado (`visible` False, permanente). Confianca: confirmado (codigo); provavel (semantica).

## 0x0804BC20 14 Remove_Current_Event_Now (Event_Op14_RemoveCurrentEventNow)
- O que faz: pega a entrada ativa `idx = 0x03006494`; se `(entrada.id & 0xFFF) == gCurrentEventId` chama `Map_ClearActiveObjectEntry`
  0x0804D980 (restaura a colisao do tile, reseta os render slots e zera a entrada de 8 bytes). Imediato.
- Args: 0 (C#: 0). PC: implementado (`remove_current`). Confianca: confirmado.

## 0x0804BC6C 15 Remove_Current_Event_Now_Keep_Collision (Event_Op15_RemoveCurrentEventNowKeepCollision)
- O que faz: na entrada `idx = 0x03006494`, limpa o bit 0x80 do byte +4 (ativo/visivel) e, se o bit 0x40 estiver ligado,
  `Map_ResetObjectRenderSlot(0x31+slot)` e `(0x5B+slot)`. NAO zera a entrada, entao a colisao fica. Nao confere o event id. Imediato.
- Args: 0 (C#: 0). PC: implementado (`remove_current`, keep_collision). Confianca: confirmado.

## 0x0804BCD4 16 Remove_Event_Now (Event_Op16_RemoveEventNow)
- O que faz: `Script_ReadBytes(2)`; procura a entrada ativa pelo id BE (`Table64_FindOffsetByKey`) e, se achar,
  `Map_ClearActiveObjectEntry`. Nao mexe em flag salva, entao a pessoa volta ao recarregar o mapa. Imediato.
- Args: 2 (C#: Short). PC: implementado (`remove_event`). Confianca: confirmado.

## 0x0804BD08 17 Set_Event_Variable (Event_Op17_SetEventVariable)
- O que faz: `Script_ReadBytes(3)`; `EventField3Bit_SetByPackedId(id BE, args[2] & 7)`. Valor de **3 bits** (0..7), nao nibble. Imediato.
- Args: 3 (C#: Short + Byte). PC: implementado (`setflag`; conferir se o PC mascara com &7). Confianca: confirmado.

## 0x0804BD44 18 Increment_Event_Variable (Event_Op18_IncrementEventVariable)
- O que faz: `Script_ReadBytes(2)`; `EventField3Bit_IncrementByPackedId(id BE)` (campo de 3 bits). Imediato.
- Args: 2 (C#: Short). PC: implementado (`incflag`). Confianca: confirmado.

## 0x0804BD70 19 Goto_Event (Event_Op19_GotoEvent; sem definicao no C#)
- O que faz: `Script_ReadBytes(2)`; grava o id BE em gCurrentEventId (0x0300646E) e zera o cursor (0x0300646A): o script continua
  do inicio de OUTRO evento. Se o id tiver objeto ativo, grava o indice dele em 0x03006494. Imediato. NAO e salto relativo.
- Args: 2 (event id BE). PC: implementado (`goto`; o extrator ja le como event id). Confianca: confirmado.

## 0x0804BDD8 1A NPC_Do_Animation (Event_Op1A_NpcDoAnimation)
- O que faz: `Script_ReadBytes(2)`; slot = (byte +4 da entrada ativa atual & 0x1F) + 0x31 -> 0x03006D2A;
  `Map_SetEventMarkerFacingVariant(slot, args[0] & 0xF, entrada)`; 0x03006D20 = 0, 0x03006D22 = args[1] (frames); StateByte=1.
  A espera/volta fica na continuacao 0x0804E43C. Atua so na pessoa do evento atual.
- Args: 2 = animation_id (4 bits uteis), frames (C#: 2). PC: falta. Confianca: confirmado.

## 0x0804BE50 1B Event_NPC_Face_Player (Event_Op1B_EventNpcFacePlayer; C#: NPC_Reset_Animation)
- O que faz: se a entrada ativa atual for o evento atual, `Map_SetEventMarkerFacingVariant(0x31+slot, 0x083D262C[0x03003811], entrada)`;
  a tabela 0x083D262C = 01 00 03 02 da a direcao OPOSTA a do jogador: a pessoa vira para o jogador. Imediato.
- Args: 0 (C#: 0). PC: falta. Confianca: confirmado (codigo); o nome do C# ("reset animation") nao bate com o efeito.

## 0x0804BEC4 1C Rotate_Event_NPC (Event_Op1C_RotateEventNpc)
- O que faz: le 1 byte (dir) para 0x03006474; se a entrada ativa atual for o evento atual,
  `Map_SetEventMarkerFacingVariant(0x31+slot, dir, entrada)`. Imediato.
- Args: 1 (C#: 1). PC: implementado (`face_self`). Confianca: confirmado.

## 0x0804BF34 1D Hop_Event_NPC (Event_Op1D_HopEventNpc)
- O que faz: sem args. Se a entrada ativa atual for o evento atual: 0x03006D2A = 0x31+slot, 0x03006D28 = Y do render slot
  (u32 em 0x03002C30 + slot*0x40 + 0x10, >>8), zera +0x28 do slot, toca o som 0x70 (pulo) e grava StateByte=1.
  Arco na continuacao 0x0804E450.
- Args: 0 (C#: 0). PC: falta. Confianca: confirmado (mesmo som e padrao do 0x0E).

## 0x0804BFB4 1E Flicker_Event_NPC (Event_Op1E_FlickerEventNpc)
- O que faz: le 1 byte (frames) para 0x03006474; se a entrada ativa atual for o evento atual: 0x03006D2A = 0x31+slot,
  0x03006D20 = frames, [1]=0, [2]=8, [3]=8; StateByte=1. O pisca fica na continuacao 0x0804E464.
- Args: 1 (C#: 1). PC: falta. Confianca: confirmado.

## 0x0804C02C 1F Initiate_Moving_Actor_A (Event_Op1F_InitiateMovingActorA)
- O que faz: `Script_ReadBytes(5)`; `slot = Map_FindFreeTrackedObjectSlot()`, `Map_SetTrackedObjectSlotByIndex(args[0], slot)`;
  0x03006D2A = `Map_GetTrackedObjectSlotByIndex(args[0]) + 0x4F`; StateByte=1. Spawn/movimento na continuacao 0x0804E480.
- Args: 5 = packed id, sprite, x, y, move (C#: 5). PC: implementado (`actor_spawn` com passos). Confianca: confirmado (primaria).

## 0x0804C078 20 Initiate_Moving_Actor_B (Event_Op20_InitiateMovingActorB)
- O que faz: primaria IDENTICA a 0x1F (5 bytes, mesmo registro de slot); a diferenca A/B esta so na continuacao 0x0804E48C
  (nao e funcao no Ghidra; nao lida).
- Args: 5 (C#: 5). PC: falta (extrator nao trata 0x20; so pula 6 bytes). Confianca: confirmado (primaria).

## 0x0804C0C4 21 Initiate_Actor (Event_Op21_InitiateActor)
- O que faz: `Script_ReadBytes(4)`; `Map_SpawnTrackedObjectAtTile(packed_id, sprite, x, y)` 0x08050358. Imediato.
- Args: 4 (C#: 4). PC: implementado (`actor_spawn`). Confianca: confirmado.

## 0x0804C100 22 Rotate_Actor (Event_Op22_RotateActor)
- O que faz: `Script_ReadBytes(2)`; `Map_SetObjectParamAndFrame(args[0] & 0xF, args[1])`. O slot usa so 4 bits. Imediato.
- Args: 2 = slot, dir (C#: 2). PC: implementado (`actor_rotate`; o extrator passa o byte inteiro, a ROM mascara &0xF). Confianca: confirmado.

## Resumo
- Tamanhos: 35/35 batem com o C# e com o extrator (06 = 0, 19 = 2, medidos no codigo). Nenhuma divergencia de tamanho.
- Divergencias de semantica: 19 e Goto_Event (troca de evento); 1B vira a pessoa para o jogador (nao "reset animation");
  17 grava 3 bits (&7); 1A usa so 4 bits do animation_id; 22 mascara o slot &0xF; 0B/0C diferem so na scene variant.
- Implementado no PC: 00, 01, 02, 04, 06, 07, 08, 09, 0A, 0B, 0C, 0D, 10, 12, 13, 14, 15, 16, 17, 18, 19, 1C, 1F, 21, 22.
- Parcial: 03, 05 (extrator grava, script.lua ignora).
- Falta: 0E, 0F, 11, 1A, 1B, 1D, 1E, 20.
