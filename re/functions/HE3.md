# HE3: handlers de opcode de evento (0x46-0x68), Metabee EU

Fonte: `EventOpcodeTable` 0x083D222C + 4*op (lista `HE3_lista.txt`, conferida contra a ROM: 35/35 batem).
Revisão GH-HE3-redo: os 35 handlers agora são funções no Ghidra; todos foram decompilados e renomeados
`Event_Op<XX>_<Nome>`. Os literais de pool (DAT_...) foram lidos direto da ROM.

Convenções (confirmadas nos decompiles desta faixa):
- Base RAM do bloco de estado do evento = `0x030057E0`. O array de flags (`event_flag_list`) e `0x030057D8`, indice `(id&0xFFF)>>1` (EF). Offsets usados: +0xC89 = `gCurrentEventStateByte`
  (0x03006469; gravar 1 = opcode diferido, o handler de continuação roda nos quadros seguintes), +0xC8A =
  `gCurrentEventScriptCursor` (0x0300646A), +0xC8C = cópia do cursor antes de ler os args (0x0300646C),
  +0xC93 = `gCurrentEventOpcodeSubstate` (0x03006473), +0xC94.. = `gEventScriptArguments[]` (0x03006474..).
- `Script_ReadBytes(N)` (0x0804D8CC) copia N bytes para gEventScriptArguments e avança o cursor;
  `site_GetEventScript` (= EventScript_ReadNextByte, 0x0804D83C) lê 1 byte.
- Salto relativo dos Jump_If_*: `cursor = cursor_salvo + último_byte`, com cursor_salvo = posição logo após o opcode
  (bate com `alvo = at + 1 + byte` do extract_events.py).
- Tabela de continuação `gEventOpcodeTableB` 0x083D2630 + 4*op. Nesta faixa só 0x56, 0x59, 0x63, 0x66 e 0x67 gravam
  estado 1 e têm continuação real; as outras entradas são 0x0804DC35 ou stubs NoOp (0x0804F5C8..0x0804F6DC).
  As continuações NÃO são funções no Ghidra (conferido de novo depois do CreateHandlerFunctions: 0x0804F5DC, F67C,
  F6E0, F83C e F850 seguem sem função); foram lidas por disassembly Thumb dos bytes da ROM.
- Tamanho de argumento = bytes que o handler consome. Comparado com `EventOpcodeDefinitions.cs` (C#),
  `LegacyEventOpcodeTable.Lengths[op] - 1` (legado) e `ROM_SIZE_FIX` de `pc/extract/extract_events.py` (tamanho total).
- mtbe.csv: os rótulos `EventOpcode50..58` em 0x0804CC5C..0x0804CD84 estão deslocados de 1 (ex.: 0x0804CC5C é o 0x51,
  0x0804CD84 é o 0x59). Vale a numeração da tabela.
Legenda PC: implementado | parcial | falta (`pc/extract/extract_events.py` e `pc/game/src/script.lua`).

## 0x46 (0x0804C984) Stop_Music
- Assinatura: `void Event_Op46_Stop_Music(void)`. Args: 0 bytes (C# 0, legado 0).
- Faz: `Overworld_ApplyMapSceneModePolicy(2, 0)` (0x0804842C). O caso 2 zera a música atual
  (0x03006410 = 0x030057E0+0xC30) e chama `Sound_RequestSong(0)`.
- PC: falta (pulado pelo tamanho; Lua sem música de evento).
- Confiança: confirmado (Ghidra).

## 0x47 (0x0804C994) Fade_Out_Music
- Assinatura: `void Event_Op47_Fade_Out_Music(void)`. Args: 0 (C# 0, legado 0).
- Faz: `Sound_RequestFade(0x78)`: fade da música em 0x78 (120) unidades, provável quadros. Não espera o fade.
- PC: falta.
- Confiança: confirmado (Ghidra).

## 0x48 (0x0804C9A0) Play_Persistent_Music
- Assinatura: `void Event_Op48_Play_Persistent_Music(void)`. Args: 1 byte (music id; C# 1, legado 1).
- Faz: lê o byte para gEventScriptArguments[0] e chama `Overworld_ApplyMapSceneModePolicy(3, id)`. Caso 3: se o bit
  0x80 está limpo, grava `id & 0x7F` em 0x03006411 (+0xC31, música persistente), toca `Sound_RequestSong(id)` e marca
  0x03006410 = 0xFF; com bit 0x80 só grava o id em 0x03006411 e marca 0xFF (sem tocar). Se 0x03006410 já é 0xFF e o
  bit 0x80 está limpo, não faz nada.
- PC: falta.
- Confiança: confirmado (Ghidra; bit 0x80 = "só registrar" é leitura do código, semântica de jogo provável).

## 0x49 (0x0804C9C4) Play_Map_Music
- Assinatura: `void Event_Op49_Play_Map_Music(void)`. Args: 0 (C# 0, legado 0).
- Faz: `Overworld_ApplyMapSceneModePolicy(4, 0)`: pega a música do mapa (`FUN_080483F8`), grava em 0x03006410 e
  chama `Sound_RequestSong` só se for diferente da atual.
- PC: falta.
- Confiança: confirmado (Ghidra).

## 0x4A (0x0804C9D4) Jump_If_Not_Player_Direction
- Assinatura: `void Event_Op4A_Jump_If_Not_Player_Direction(void)`. Args: 2 bytes (dir, jump; C# 2, legado 2).
- Faz: `Script_ReadBytes(2)`; se `args[0] != *(u8*)0x03003811` salta por `args[1]`.
- Memória: 0x03003811 (= 0x03002C30+0xBE1; mtbe rotula `dpad_input_2`, o C# chama de direção do jogador).
- PC: parcial (extract: kind "jump"; o Lua não avalia a condição e segue o `next`).
- Confiança: confirmado no corpo; o significado do byte 0x03003811 é provável.

## 0x4B (0x0804CA28) Jump_If_Item_Count_Below_Amount
- Args: 3 bytes (item, amount, jump). Lidos com ReadBytes(3).
- Faz: `c = FUN_08047D8C(item)` (wrapper de `Map64_GetValue`); salta se `c == 0` ou `c < amount`.
- Memória: `Map64_GetValue` varre 64 pares de bytes em 0x03002BA0 (`[flag|id, count]`; bit 0x80 = entrada usada,
  id comparado com `item & 0x3F`) e devolve `count` (0 se não achou). Ou seja, inventário de itens = 0x03002BA0, 64 x 2 bytes.
- PC: implementado (`item_below` no Lua, linha ~109).
- Confiança: confirmado (lido no Ghidra).

## 0x4C (0x0804CA84) Jump_If_Missing_Part
- Args: 3 bytes (bot, part, jump). ReadBytes(3).
- Faz: `r = PartSet_GetLegIconIndexForMatchingSlotPartId(bot, part)`; se `r == -1` salta; senão guarda `r` em
  0x03006440 (0x030057E0+0xC60), que o 0x51 usa depois.
- PC: parcial. "jump" sem avaliar condição (Lua cai no próximo).
- Confiança: confirmado (Ghidra).

## 0x4D (0x0804CAE8) Jump_If_Not_Has_Medal
- Args: 2 bytes (medal, jump). ReadBytes(2).
- Faz: `FUN_08047DB0(medal) == 0` → salta. FUN_08047DB0 é wrapper de `Slot_FindByIdPlus1`: varre 30 registros de
  0x40 bytes em 0x03000BE0 (bit 0x80 do byte 0 = slot usado, byte +1 = id da medalha) e devolve índice+1 ou 0.
- Memória: lista de medalhas 0x03000BE0 (30 x 0x40).
- PC: parcial (kind "jump", condição não avaliada).
- Confiança: confirmado (Ghidra).

## 0x4E (0x0804CB34) Jump_If_Not_Has_Money
- Args: 3 bytes (money Short big-endian, jump). ReadBytes(3).
- Faz: `FUN_08047D4C() < money * 10` → salta. Atenção: o valor é multiplicado por 10.
- Memória: FUN_08047D4C devolve `*(u32*)0x03002B70` (dinheiro do save, literal em 0x08047D54).
- PC: implementado (`money_below`, Lua linha ~111), mas nem o extract (amount = Short BE cru) nem o Lua
  (`save.money < s.amount`) aplicam o fator 10. Só bate se `save.money` estiver em unidades de 10.
- Confiança: confirmado (Ghidra).

## 0x4F (0x0804CB94) Jump_If_Previous_Scene_Command_Failed
- Args: 1 byte (jump). ReadBytes(1).
- Faz: salta se `*(u8*)0x03005349 == 2` (comando de cena anterior falhou/perdeu).
- Memória: DAT_0804CBD4 = 0x03005310 (mtbe `gOverworldAffineState`, nome provavelmente estreito demais); byte +0x39
  = 0x03005349 = resultado do último comando de cena (2 = falhou).
- PC: implementado (`prev_failed`, Lua linha ~107; `won == false`, aproximação).
- Confiança: confirmado (Ghidra).

## 0x50 (0x0804CBDC) Jump_If_Event_Counter_Below (C#: "Jump_If_Current_Active_Object_Index_Not_Equal")
- Assinatura: `void Event_Op50_Jump_If_Event_Counter_Below(void)`. Args: 3 bytes (sel, value, jump).
  C# tem só 2 args (diverge); legado 3 e ROM_SIZE_FIX 4 total batem com a ROM.
- Faz: `Script_ReadBytes(3)`; `c = (s8)(sel == 0 ? *(u8*)0x0300649C : *(u8*)0x0300649D)`; se `c < value` salta por
  `jump`. Comparação com sinal no contador, sem sinal no valor.
- Memória: 0x0300649C / 0x0300649D (+0xCBC/+0xCBD), sem rótulo no mtbe; parecem contadores do evento.
- PC: parcial (kind "jump", tamanho certo pelo ROM_SIZE_FIX, condição não avaliada).
- Confiança: confirmado no corpo; nome provável (o nome do C# não bate com o código).

## 0x51 (0x0804CC5C) Jump_If_Found_Part_Index_Not_Equal (C#: "Random_Jump")
- Assinatura: `void Event_Op51_Jump_If_Found_Part_Index_Not_Equal(void)`. Args: 2 bytes (value, jump; C# 2, legado 2).
- Faz: `Script_ReadBytes(2)`; se `args[0] - 0x46 != *(u8*)0x03006440` salta por `args[1]`. 0x03006440 é o índice
  gravado pelo 0x4C quando a peça foi achada, então o par 0x4C/0x51 testa "qual" slot/ícone tem a peça.
- PC: parcial (extract: 0x51 = "jump", sem avaliar). O nome do C# (Random_Jump) está errado: o aleatório é o 0x52.
- Confiança: confirmado no corpo; ligação com o 0x4C provável (mesmo endereço).

## 0x52 (0x0804CCAC) Random_Jump (sem definição no C#; C# põe Random_Jump no 0x51)
- Args: 2 bytes (reserved, jump). ReadBytes(2).
- Faz: `Random_NextScaled(0xFF)` (0x08049780); se `(r & 0xF) != 0` salta (15/16 das vezes). O primeiro byte não é usado.
- PC: parcial (extract: kind "random", ROM_SIZE_FIX 3; o Lua não trata "random" e sempre segue o `next`).
- Confiança: confirmado (Ghidra).

## 0x53 (0x0804CCF4) Set_Map_Sprite_Slot (C#: "Set_Object_Render_Mode")
- Assinatura: `void Event_Op53_Set_Map_Sprite_Slot(void)`. Args: 2 bytes (slot, sprite; C# 2, legado 2).
- Faz: `Script_ReadBytes(2)`; `slot = args[0]` (se != 0, slot - 1, ou seja 1-based); grava `args[1]` em
  `0x03006418 + slot` (+0xC38: lista de sprites do mapa atual); recarrega gráficos
  (`Map_LoadOverworldSpriteGraphicsForCurrentMap`, 0x08048288) e paletas: `FUN_080486B0` se
  `gScreenTransitionState[1]` (0x03006F41) ∈ {1, 3, 5, 6}, senão `Map_LoadCurrentEventObjectPalettes` (0x08048708).
- PC: falta (pulado pelo tamanho).
- Confiança: confirmado no corpo; nome provável (args do C# "target_flags/render_mode" não batem).

## 0x54 (0x0804CD60) NoOp (sem definição no C#)
- Args: 0 (legado 0). Corpo: só `bx lr`. Continuação = stub NoOp.
- PC: falta: o extract não conhece 0x54 (fora do C#) e emite `unknown`, parando a leitura do script.
- Confiança: confirmado (Ghidra).

## 0x55 (0x0804CD64) NoOp (C# "Reserved_NoOp")
- Args: 0 (C# 0, legado 0). Corpo vazio; continuação NoOp. Não entra em estado diferido.
- PC: implementado (no-op; pulado pelo tamanho 1).
- Confiança: confirmado (Ghidra).

## 0x56 (0x0804CD68) Load_Entities
- Assinatura: `void Event_Op56_Load_Entities(void)`. Args: 0 (C# 0, legado 0).
- Faz: só grava `gCurrentEventStateByte = 1`. O trabalho é da continuação 0x0804F5DC (mtbe EventOpcode56_LoadEntities):
  `Overworld_BuildEncounterNpcSnapshot`; zera 0x03003870 e 0x03004370 (0x900 cada) e 0x03005664/0x0300570C (0x90);
  `Overworld_RebuildActiveObjectRenderSlots`; zera `gMapCollisionOccupancyBuffer` 0x03005FF0 (0x400) e copia nele a
  colisão do mapa atual (`gMapCollision[gCurrentMapId]` 0x08404334, `(w/2)*(h/2)` bytes, w/h em 0x03005FE0/0x03005FE2);
  marca 0x0300649A = 1; `Map_LoadEntitySpawnsForCurrentMap`, `Map_UpdateVisibleEventObjectSprites` e
  `Event_EndOpcode` (termina no mesmo quadro da continuação).
- PC: falta.
- Confiança: confirmado (setup no Ghidra; continuação por disassembly da ROM).

## 0x57 (0x0804CD7C) NoOp (C# "Reserved_NoOp")
- Args: 0 (C# 0, legado 0). Corpo vazio; continuação NoOp.
- PC: implementado (no-op; pulado pelo tamanho 1).
- Confiança: confirmado (Ghidra).

## 0x58 (0x0804CD80) NoOp (sem definição no C#)
- Args: 0 (legado 0). Corpo vazio; continuação NoOp.
- PC: falta: fora do C#, o extract emite `unknown` e para a leitura.
- Confiança: confirmado (Ghidra).

## 0x59 (0x0804CD84) Open_Shop
- Assinatura: `void Event_Op59_Open_Shop(void)`. Args: 1 byte (shop id; C# 1, legado 1).
- Faz: `Script_ReadBytes(1)` e estado = 1. Continuação 0x0804F67C: subpasso 0 chama `Event_ResetSceneCommandStep`
  (0x08050CC0) e avança; subpasso 1 roda `Event_RunSceneCommandSequence(3, shop_id)` (0x08050CCC) até retornar != 0
  e então `Event_EndOpcode`.
- Memória: gEventScriptArguments[0], gCurrentEventOpcodeSubstate (0x03006473).
- PC: implementado (extract `{op="shop"}`; Lua `host.shop`).
- Confiança: confirmado (setup no Ghidra; continuação por disassembly).

## 0x5A (0x0804CDA4) Set_Secondary_Marker_Facing
- Assinatura: `void Event_Op5A_Set_Secondary_Marker_Facing(void)`. Args: 1 byte (facing; C# 1, legado 1).
- Faz: lê o byte em args[0] e grava em 0x03003851 e 0x03003850; `anim = Overworld_GetObjectAnimationIndexForFacing(
  0x30, facing + 4 * *(u8*)0x0300385B)`; `Map_SetObjectAnimationVariant(0x30, anim)`;
  `Map_SetSecondaryMarkerFacing(facing)`. Slot 0x30 = marcador secundário (objeto de render 0x30).
- PC: falta.
- Confiança: confirmado (Ghidra).

## 0x5B (0x0804CE04) Jump_If_Fewer_Than_Three_Complete_Part_Sets
- Args: 1 byte (jump). ReadBytes(1).
- Faz: `FUN_08047F9C() < 3` → salta. FUN_08047F9C conta, nos 9 registros de 8 bytes em 0x030017A0, quantos têm
  byte 0 == 3.
- Memória: 0x030017A0 = 9 registros de Medabot montado (byte 0 = estado, 3 = completo; +3..+6 = 4 peças; ver 0x62).
- PC: parcial (kind "jump" da extract, sem avaliar condição).
- Confiança: confirmado (Ghidra).

## 0x5C (0x0804CE44) Jump_If_Missing_Complete_Monochrome_Part_Set
- Args: 2 bytes (part_id, jump). ReadBytes(2).
- Faz: `FUN_08047FBC(part_id) == 0` → salta. FUN_08047FBC procura nos 9 registros de 0x030017A0 um com byte 0 == 3
  e as 4 peças (+3..+6) iguais a `part_id` (set completo de um só modelo).
- Memória: 0x030017A0 (mesma tabela do 0x5B).
- PC: parcial (condição não avaliada).
- Confiança: confirmado (Ghidra).

## 0x5D (0x0804CE90) Update_Overworld_Event_Mode_Flags
- Assinatura: `void Event_Op5D_Update_Overworld_Event_Mode_Flags(void)`. Args: 1 byte (C# 1, legado 1).
- Faz: `b = args[0]`. Bit 0x80 limpo: `*(u8*)0x03006413 = b & 0xF`. Bit 0x80 ligado: se `(b & 0xF) == 1` liga o bit
  0x20 de `gOverworldEventModeFlags` (0x0300643D), senão desliga o bit 0x20.
- Memória: 0x03006413 (+0xC33, sem rótulo), 0x0300643D.
- PC: falta.
- Confiança: confirmado (Ghidra).

## 0x5E (0x0804CEF8) Set_Overworld_Event_Mode_Bit0
- Args: 1 byte (enabled; C# 1, legado 1). ReadNextByte; grava em args[0].
- Faz: `enabled == 1` liga o bit 0 de `gOverworldEventModeFlags` (0x0300643D); qualquer outro valor desliga.
- PC: falta.
- Confiança: confirmado (Ghidra).

## 0x5F (0x0804CF40) Set_Random_Encounter_Mode_Flags
- Args: 1 byte (flags; C# 1, legado 1). ReadNextByte.
- Faz: grava o byte direto em `gRandomEncounterModeFlags` (0x0300640C; não passa por args).
- PC: falta.
- Confiança: confirmado (Ghidra).

## 0x60 (0x0804CF5C) Focus_Event_Object
- Assinatura: `void Event_Op60_Focus_Event_Object(void)`. Args: 3 bytes (mode, hi, lo; C# Byte+Short = 3, legado 3).
- Faz: `Script_ReadBytes(3)`.
  - mode 0: `s = Map_GetTrackedObjectSlotByIndex(lo)` (0x0804DBF0; usa só o 3º byte); se `s != -1`,
    `Map_FocusEventObjectRenderSlot(s + 0x4F)` (0x0804D9F8).
  - mode 1: `id = hi<<8 | lo` (BE). `id == 0`: foca o objeto do evento atual,
    `(byte +4 do registro 0x03005DBC + 8*gCurrentEventActiveMapObjectIndex) & 0x1F) + 0x31`. Senão `e = Table64_FindOffsetByKey(id)`;
    se achou, foca `(e[4] & 0x1F) + 0x31`.
  - outro mode: nada.
- Memória: 0x03006494 (gCurrentEventActiveMapObjectIndex), registros de objeto de mapa em 0x03005DBC (8 bytes).
- PC: falta.
- Confiança: confirmado (Ghidra).

## 0x61 (0x0804D00C) Restore_Focused_Event_Mode_Flags
- Args: 0 (C# 0, legado 0). Chama `Map_RestoreFocusedEventModeFlags` (0x0804DB80), que copia
  `gOverworldEventModeFlagsBackup` (0x0300643E) de volta para 0x0300643D (desfaz o 0x60).
- PC: falta.
- Confiança: confirmado (Ghidra; efeito do callee pelo mtbe).

## 0x62 (0x0804D018) Equip_Starter_Parts
- Args: 0 (C# 0, legado 0). Chama `site_StarterBot` (0x08044B58).
- Faz (callee decompilado): `*(u8*)0x03000BE0 |= 0xC0`; bloco 0x030017A0 = `{3, ?, 1, 0x2B, 0x2B, 0x2B, 0x2B}`
  (4 peças 0x2B equipadas); inventário 0x03004C40: grava 1 em +0x57, +0x147, +0x237, +0x327 (peça 0x2B nas 4
  listas de peças, passo 0xF0, entrada de 2 bytes).
- PC: falta.
- Confiança: confirmado (Ghidra); significado dos campos do bloco 0x030017A0 provável.

## 0x63 (0x0804D024) Show_Effect
- Assinatura: `void Event_Op63_Show_Effect(void)`. Args: 3 bytes (id, tx, ty; C# 3, legado 3).
- Faz: `Script_ReadBytes(3)`; `Overworld_BeginUiSceneState(id, (tx & 0x7F)*16 + *(u16*)0x03006428,
  (ty & 0x7F)*16 + *(u16*)0x0300642A, 0)` (0x080533E4); estado = 1. Posição = tile relativo à origem em 0x03006428/2A.
- Continuação 0x0804F6E0 (disassembly): subpasso 0 roda 0x08050BC8 até != 0; se `gOverworldUiState` (0x03005350+0x1D4)
  & 0x3F for 0xD ou 0x16 segue para os subpassos 1-2, senão termina (`Event_EndOpcode`). Subpasso 1 cria o objeto
  0x2F com `(id<<4)|tx`; subpasso 2 espera `FUN_0804F188`, refaz âncoras de câmera/colisão do mapa,
  `Overworld_ResetUiRenderState` e termina.
- PC: falta.
- Confiança: confirmado no setup; continuação provável (lida em disassembly, ramos 1-2 não nomeados).

## 0x64 (0x0804D084) Reset_Overworld_UI_Render_State
- Args: 0 (C# 0, legado 0). Chama `Overworld_ResetUiRenderState` (0x08050C94) e sai.
- PC: falta (não precisa no port; é reset de render).
- Confiança: confirmado (Ghidra).

## 0x65 (0x0804D090) NoOp (C# "Reserved_NoOp")
- Args: 0 (C# 0, legado 0). Corpo vazio.
- PC: implementado (no-op; pulado pelo tamanho 1).
- Confiança: confirmado (Ghidra).

## 0x66 (0x0804D094) Animate_Tracked_Object_Transition
- Assinatura: `void Event_Op66_Animate_Tracked_Object_Transition(void)`. Args: 2 bytes (tracked index, mode;
  C# 2, legado 2, ROM_SIZE_FIX 3 total).
- Faz: `Script_ReadBytes(2)`; `slot = Map_GetTrackedObjectSlotByIndex(args[0]) + 0x4F`, grava em 0x03006D2A; zera o
  byte +0x26 do registro de render `0x03002C30 + slot*0x40`; estado = 1. O 2º byte não é lido no setup (provável uso
  dentro de `Map_TickTrackedObjectTransition`).
- Continuação 0x0804F83C: chama `Map_TickTrackedObjectTransition` (0x080509EC) a cada quadro e `Event_EndOpcode` quando
  ele retorna != 0.
- PC: falta.
- Confiança: confirmado (setup Ghidra; continuação por disassembly).

## 0x67 (0x0804D0E0) Skip_Two_Bytes_Wait (sem definição no C#)
- Args: 2 bytes (legado 2). `Script_ReadBytes(2)`; estado = 1.
- Continuação 0x0804F850: só `Event_EndOpcode`. Efeito: consome 2 bytes sem usá-los e espera 1 quadro.
- PC: falta: fora do C#, o extract emite `unknown` e para a leitura.
- Confiança: confirmado (Ghidra + disassembly).

## 0x68 (0x0804D100) Clear_Collision_For_Packed_Object
- Assinatura: `void Event_Op68_Clear_Collision_For_Packed_Object(void)`. Args: 2 bytes (id Short BE; C# 2, legado 2).
- Faz: `Script_ReadBytes(2)`; pega o registro de objeto do evento ATUAL `r = 0x03005DBC + 8*gCurrentEventActiveMapObjectIndex`;
  se `(r.u16[1] & 0xFFF) == id` chama `set_collision(r[0], r[1], 0)` (0x08052E58). Não procura o id na lista: só
  limpa se o objeto atual tem esse id.
- PC: falta.
- Confiança: confirmado (Ghidra).

## Discrepâncias para o port
- Tamanhos: a ROM bate com o legado em todos os 35. Diverge do C#: 0x50 (ROM 3, C# 2; ROM_SIZE_FIX corrige).
  Sem definição no C#: 0x52 (2, coberto pelo ROM_SIZE_FIX), 0x54 (0), 0x58 (0), 0x67 (2): para 0x54/0x58/0x67 o
  extract emite `unknown` e para.
- ROM_SIZE_FIX 0x66 = 3 agora é redundante (o C# já tem 2 args).
- Nomes do C# errados pelo código: 0x50 (é contador < valor), 0x51 (não é Random_Jump), 0x53 (troca sprite de slot do mapa).
- 0x4E: o Lua compara sem o fator 10.
- 0x52: o Lua não avalia "random" (ROM salta 15/16 das vezes).
- mtbe.csv: rótulos EventOpcode50..58 deslocados de 1 em relação à tabela.
