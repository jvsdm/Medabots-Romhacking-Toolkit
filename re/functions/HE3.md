# HE3: handlers de opcode de evento (0x46-0x68), Metabee EU

Fonte: `EventOpcodeTable` 0x083D222C + 4*op (lista `HE3_lista.txt`, conferida contra a ROM: 35/35 batem).
Tamanho de argumento = `LegacyEventOpcodeTable.Lengths[op] - 1` (usado pelo leitor do Toolkit como fallback).
Ghidra: só 13 entradas têm função definida; as demais são "No function found" (código não analisado nesta
entrada). Nesses casos a leitura veio dos bytes Thumb da ROM + `mtbe.csv` + `EventOpcodeDefinitions.cs`.
Os rótulos `EventOpcodeNN` do `mtbe.csv` nesta faixa estão deslocados (ex.: 0x0804C9A0 rotulado 0x43 e
0x0804C9C4 rotulado 0x44; a tabela diz 0x48 e 0x49). Vale a numeração da tabela.
Leitura de argumento: `EventScript_ReadBytes` 0x0804D8CC (N bytes) e `EventScript_ReadNextByte` 0x0804D83C.
Legenda PC: implementado | parcial | falta (ver `pc/extract/extract_events.py` e `pc/game/src/script.lua`).

## 0x46 (0x0804C984) Stop_Music (provável)
- Args: 0 bytes. Ghidra: sem função nesta entrada.
- Faz: chama `Overworld_ApplyMapSceneModePolicy(2, 0)` (0x0804842C), que para a música.
- Memória: não lida além da chamada (efeito dentro da política não decodificado).
- PC: falta (extract não decodifica 0x46; script.lua não tem).
- Confiança: provável (bytes + nome do Toolkit).

## 0x47 (0x0804C994) Fade_Out_Music (provável)
- Args: 0 bytes. Ghidra: sem função nesta entrada.
- Faz: chama `FUN_08047BC0(0x78)` (0x78 = duração fixa, provável em frames). Nome do Toolkit bate.
- Memória: não decodificada dentro de 0x08047BC0.
- PC: falta.
- Confiança: provável.

## 0x48 (0x0804C9A0) Play_Persistent_Music (provável)
- Args: 1 byte (music id, tipo Music do Toolkit). Lê com `EventScript_ReadNextByte`.
- Faz: `Overworld_ApplyMapSceneModePolicy(3, byte)`: toca a música persistente id.
- Memória: via política de modo de mapa (não decodificada).
- PC: falta.
- Confiança: provável. (mtbe chama isso de 0x43; o nome dele está errado.)

## 0x49 (0x0804C9C4) Play_Map_Music (provável)
- Args: 0. Faz: `Overworld_ApplyMapSceneModePolicy(4, 0)`: toca a música do mapa.
- Memória: via política de modo de mapa. Confirma H.md (0x49 = C9C4).
- PC: falta.
- Confiança: provável.

## 0x4A (0x0804C9D4) Jump_If_Not_Player_Direction
- Args: 2 bytes (dir Direction 1 byte, jump 1 byte). Ghidra: sem função; usa ReadBytes.
- Faz: salto relativo se a direção do jogador não for `dir` (condição não decodificada).
- Memória: não decodificada (provável leitura da direção do objeto do jogador).
- PC: parcial. BRANCH_OPS marca "jump", mas o Lua não avalia a condição e segue o próximo passo.
- Confiança: chute no corpo; tamanho confirmado pelo legado.

## 0x4B (0x0804CA28) Jump_If_Item_Count_Below_Amount
- Args: 3 bytes (item, amount, jump). Lidos com ReadBytes(3).
- Faz: `c = FUN_08047D8C(item)`; salta se `c == 0` ou `c < amount`.
- Memória: contagem de item no save (via FUN_08047D8C, não decodificada).
- PC: implementado (`item_below` no Lua, linha ~109).
- Confiança: confirmado (lido no Ghidra FUN_0804ca28).

## 0x4C (0x0804CA84) Jump_If_Missing_Part
- Args: 3 bytes (bot, part, jump). ReadBytes(3).
- Faz: `PartSet_GetLegIconIndexForMatchingSlotPartId(bot, part) == -1` → salta.
- Memória: leitura de slots de peça via PartSet (não decodificada). Caches `*(iVar1+0xC60)` quando achou.
- PC: parcial. "jump" sem avaliar condição (Lua cai no próximo).
- Confiança: confirmado (Ghidra FUN_0804ca84).

## 0x4D (0x0804CAE8) Jump_If_Not_Has_Medal
- Args: 2 bytes (medal, jump). ReadBytes(2).
- Faz: `FUN_08047DB0(medal) == 0` → salta.
- Memória: posse de medalha no save (não decodificada).
- PC: parcial (kind "jump", condição não avaliada).
- Confiança: confirmado (Ghidra FUN_0804cae8).

## 0x4E (0x0804CB34) Jump_If_Not_Has_Money
- Args: 3 bytes (money Short big-endian, jump). ReadBytes(3).
- Faz: `FUN_08047D4C() < money * 10` → salta. Atenção: o valor é multiplicado por 10.
- Memória: dinheiro do save (via FUN_08047D4C).
- PC: implementado (`money_below` no Lua, linha ~111). Checar o fator 10 no Lua.
- Confiança: confirmado (Ghidra FUN_0804cb34).

## 0x4F (0x0804CB94) Jump_If_Previous_Scene_Command_Failed
- Args: 1 byte (jump). ReadBytes(1).
- Faz: salta se o byte de estado `*(DAT_cbd4 + 0x39) == 2` (comando de cena anterior falhou).
- Memória: byte de estado (+0x39 do bloco apontado por DAT_0804CBD4), não nomeado.
- PC: implementado (`prev_failed`, Lua linha ~107; `won == false`, aproximação).
- Confiança: confirmado (Ghidra FUN_0804cb94).

## 0x50 (0x0804CBDC) indefinido no Toolkit (C# diz "Jump_If_Current_Active_Object_Index_Not_Equal", 2 args)
- Args: 3 bytes pela ROM (legado = 4). ReadBytes(3). O C# só tem 2 args.
- Faz: compara um índice de objeto ativo (byte de estado) com argumento e aplica salto relativo.
  Semântica exata não fechada (o byte a mais não foi identificado).
- Memória: estado do objeto ativo (offset do byte de estado não nomeado).
- PC: parcial, com problema de tamanho. Extract assume 3 bytes; a ROM consome 4. Risco de dessincronia.
- Confiança: chute no corpo; tamanho confirmado pelo legado.

## 0x51 (0x0804CC5C) compara índice ativo + salto (ver discrepância)
- Args: 2 bytes (ReadBytes(2)). Ghidra FUN_0804cc5c.
- Faz: compara `byte(estado) - 0x46` com outro byte; se diferente, aplica salto (`cursor = base + byte`).
  Bate com o nome C# 0x50 ("active object index" com bias 0x46 mencionado no mtbe).
- Memória: byte de estado do objeto ativo (não nomeado).
- PC: parcial/erro. Lua trata 0x51 como "random" (BRANCH_OPS), que é o corpo de 0x52 na ROM.
- Confiança: provável (corpo lido no Ghidra); nome: provável "Jump_If_Current_Active_Object_Index_Not_Equal".

## 0x52 (0x0804CCAC) Random_Jump (provável, C# chama 0x51)
- Args: 2 bytes (reserved, jump). ReadBytes(2). Ghidra FUN_0804ccac.
- Faz: `Random_NextScaled(0xFF)` (0x08049780); se `(r & 0xF) != 0` salta. O primeiro byte não entra no fluxo visível.
- Memória: RNG (`Random_NextScaled`).
- PC: falta no código do 0x52 (Lua tem "random" ligado ao 0x51 errado).
- Confiança: confirmado no corpo; numeração 0x52 pela tabela.

## 0x53 (0x0804CCF4) Set_Object_Render_Mode (chute)
- Args: 2 bytes (legado = 3). C# diz target_flags + render_mode.
- Faz: mtbe diz "marca estado de continuação" e chama `0x08...` com r0=2; corpo não fechado.
- Memória: ver mtbe (sprite/palette do objeto, não confirmado nesta entrada).
- PC: falta.
- Confiança: chute (contraditório entre o corpo e o C#).

## 0x54 (0x0804CD60) sem argumento (chute)
- Args: 0 (legado = 1). Ghidra: sem função. Corpo curto, sem chamadas (só retorno).
- Faz: nada visível (provável NoOp ou ponto de espera). O mtbe diz "lê 1 byte", o legado diz 0 — conflito.
- Memória: nenhuma nos bytes lidos.
- PC: falta.
- Confiança: chute.

## 0x55 (0x0804CD64) lê 0 bytes, entra em estado diferido (chute)
- Args: 0 (legado 1). Bytes com ReadBytes de N=0 ou sem leitura; nome Toolkit: não existe (0x55 não tem definição).
- Faz: provável "espera" (estado diferido, continuação externa).
- Memória: byte de estado `gCurrentEventStateByte`.
- PC: falta.
- Confiança: chute.

## 0x56 (0x0804CD68) Load_Entities (chute)
- Args: 0 (legado 1). Toolkit: "Load_Entities" sem args. mtbe: "lê 8 bytes" (conflito).
- Faz: provável recarga de entidades de mapa a partir do script (continuação recarrega spawns).
- Memória: lista de entidades do mapa (ponteiro não confirmado).
- PC: falta.
- Confiança: chute.

## 0x57 (0x0804CD7C) sem argumento, estado diferido (chute)
- Args: 0. Toolkit: "Reserved_NoOp" (C#). mtbe: entra em estado diferido.
- Faz: provável espera externa; nome do Toolkit diz NoOp.
- Memória: estado de evento.
- PC: falta.
- Confiança: chute.

## 0x58 (0x0804CD80) sem argumento (chute)
- Args: 0 (legado 1). Toolkit: sem definição. mtbe: sem descrição.
- Faz: não fechado (tem chamada de leitura de 1 byte em uma das ramificações, não confirmado).
- PC: falta.
- Confiança: chute.

## 0x59 (0x0804CD84) Open_Shop
- Args: 1 byte (shop id 0..14). Toolkit: `Open_Shop`, shop_id Byte (C# confere).
- Faz: abre a loja `shop_id`; o handler de continuação espera a loja fechar (mtbe liga a shops).
- Memória: estado da loja (tabela de lojas 0x4838E4, por `shops.lua`).
- PC: implementado (extract `{op="shop"}`; Lua `host.shop`).
- Confiança: confirmado pelo Toolkit + PC (parte da tabela de lojas via SH1 em pc/research).

## 0x5A (0x0804CDA4) Set_Secondary_Marker_Facing_Variant (chute)
- Args: 1 byte (legado 2). Toolkit: `variant` Byte (C# confere o tamanho).
- Faz: provável "muda variante de direção do marcador" (mtbe descreve o 0x59 e não confirma).
- Memória: objeto 0x30 (facing/animação, mtbe; não confirmado).
- PC: falta.
- Confiança: chute.

## 0x5B (0x0804CE04) Jump_If_Fewer_Than_Three_Complete_Part_Sets
- Args: 1 byte (jump). ReadBytes(1). Ghidra FUN_0804ce04.
- Faz: `FUN_08047F9C() < 3` → salta.
- Memória: contagem de conjuntos de peça completos (PartSet_CountCompleteEntries).
- PC: parcial (kind "jump" da extract, sem avaliar condição).
- Confiança: confirmado (Ghidra).

## 0x5C (0x0804CE44) Jump_If_Missing_Complete_Monochrome_Part_Set
- Args: 2 bytes (part_id, jump). ReadBytes(2). Ghidra FUN_0804ce44.
- Faz: `FUN_08047FBC(part_id) == 0` → salta.
- Memória: conjuntos de peça monocromáticos (PartSet_HasCompleteMonochromeEntry).
- PC: parcial (condição não avaliada).
- Confiança: confirmado (Ghidra).

## 0x5D (0x0804CE90) Update_Overworld_Event_Mode_Flags (provável)
- Args: 1 byte (mode_control). ReadBytes(1).
- Faz: se o bit alto estiver ativo, alterna bit 0x20 de `gOverworldEventModeFlags`; senão atualiza o nibble do modo.
- Memória: `gOverworldEventModeFlags` e um byte companheiro (endereços não nomeados aqui).
- PC: falta.
- Confiança: provável (mtbe + C#), corpo não fechado.

## 0x5E (0x0804CEF8) Set_Overworld_Event_Mode_Bit0
- Args: 1 byte (enabled). ReadNextByte; grava em gEventScriptArguments[0].
- Faz: liga/desliga o bit 0 de `gOverworldEventModeFlags`.
- Memória: `gOverworldEventModeFlags` (RAM, endereço do Toolkit).
- PC: falta.
- Confiança: provável (mtbe, corpo lido parcialmente).

## 0x5F (0x0804CF40) Set_Random_Encounter_Mode_Flags
- Args: 1 byte (flags). ReadNextByte; copia em `gRandomEncounterModeFlags`.
- Faz: controla encontros aleatórios (Overworld_ShouldTriggerRandomEncounter).
- Memória: `gRandomEncounterModeFlags` (RAM).
- PC: falta.
- Confiança: provável (mtbe + Toolkit).

## 0x60 (0x0804CF5C) Focus_Event_Object
- Args: 3 bytes (target_mode 1 byte + packed object id Short). ReadBytes(3).
- Faz: foca a câmera/render no objeto rastreado ou no objeto ativo, conforme target_mode.
- Memória: câmera/staging de render do evento.
- PC: falta.
- Confiança: provável (mtbe menciona só 1 byte; C# diz Byte + Short; o legado diz 3 bytes → use 3).

## 0x61 (0x0804D00C) Restore_Focused_Event_Mode_Flags
- Args: 0 (legado 1). Ghidra: chama `Map_RestoreFocusedEventModeFlags` (0x0804DB80).
- Faz: restaura flags de modo de evento salvas pelo 0x60.
- PC: falta.
- Confiança: provável.

## 0x62 (0x0804D018) Equip_Starter_Parts
- Args: 0. Chama `site_StarterBot` (0x08044B58); nome do Toolkit diz equipar peças iniciais.
- Faz: equipa as peças iniciais do bot do jogador.
- Memória: inventário/slots de peça (não decodificado).
- PC: falta.
- Confiança: provável.

## 0x63 (0x0804D024) Show_Effect
- Args: 3 bytes (effect_id, x, y). ReadBytes(3).
- Faz: mostra o efeito `effect_id` na posição (x, y).
- Memória: não decodificada (provável OAM/tarefa de efeito).
- PC: falta.
- Confiança: provável (C# confere).

## 0x64 (0x0804D084) Reset_Overworld_UI_Render_State
- Args: 0. Chama `Overworld_ResetUiRenderState` (0x08050C94) e sai.
- Faz: reseta o estado affine da UI do overworld.
- Memória: registradores de affine/OAM da UI (via a função).
- PC: falta (não precisa no port; é reset de hardware de render).
- Confiança: confirmado pelo mtbe e pela chamada.

## 0x65 (0x0804D090) NoOp
- Args: 0. Corpo inerte (mtbe: "no-op stub"). C# "Reserved_NoOp".
- PC: falta (trivial: basta não fazer nada e avançar 1 byte).
- Confiança: confirmado (corpo vazio, mtbe).

## 0x66 (0x0804D094) Animate_Tracked_Object_Transition
- Args: 2 bytes pela ROM (legado = 3). C# só diz 1 (tracked_object_index). O mtbe diz 1 byte.
  Pelo tamanho do legado, o segundo byte é transition_mode (C# lista esse, sem tamanho certo).
- Faz: resolve o slot de render do objeto rastreado em gEventScriptArguments[10], zera o byte de transição e entra em modo diferido.
- Memória: byte de transição do slot (RAM, não nomeado); gEventScriptArguments[10].
- PC: falta.
- Confiança: provável no corpo; tamanho contraditório (1 vs 2): a ROM vence.

## 0x67 (0x0804D0E0) sem definição no Toolkit
- Args: 2 bytes (legado = 3). Sem definição no C#.
- Faz: não fechado (corpo com ReadBytes; sem leitura de corpo nesta sessão).
- PC: falta.
- Confiança: chute.

## 0x68 (0x0804D100) Clear_Collision_For_Packed_Object
- Args: 2 bytes (packed_object_id Short BE). Toolkit confere.
- Faz: acha o registro do objeto de mapa ativo com esse id e limpa a colisão na sua célula (`set_collision(x, y, 0)`).
- Memória: tabela de colisão do mapa (RAM); registros de objeto de mapa.
- PC: falta.
- Confiança: provável (mtbe + Toolkit).

## Discrepâncias para o port
- Tabela de dispatch (ROM) vs Toolkit C#: o C# dá 0x50 = active-object (2 args) e 0x51 = Random_Jump. A ROM mostra 0x50 com 3 bytes, 0x51 com a comparação de índice ativo e 0x52 com o Random_Jump. Confirmar com o emulador (fora desta tarefa). Se estiver certo, o PC decodifica 0x51 errado e 0x50 com tamanho errado.
- mtbe.csv: rótulos `EventOpcode43..55` nesta faixa estão deslocados em relação à tabela.
- 0x66: tamanho 2 pela ROM; C# e mtbe dizem 1.
- 0x4E: valor multiplicado por 10 antes da comparação (checar o Lua).
