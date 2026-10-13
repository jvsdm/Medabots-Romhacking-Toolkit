# Handlers de opcode de evento 0x23..0x45 (HE2)

Tabela `0x083D222C + 4*op` (confirmada na ROM EU; ponteiros com bit Thumb). Lista: `HE2_lista.txt`.
Ghidra: NENHUMA função definida nos endereços 0x0804C130..0x0804C960 (get_function_by_address e disassemble devolvem
"no function"); por isso nada foi renomeado no Ghidra. Leitura feita com capstone sobre a ROM (`re/tools`-like, scratch).
Padrão comum: o handler "setup" lê os argumentos com `EventScript_ReadBytes` (0x0804D8CC, n bytes) ou
`EventScript_ReadNextByte` (0x0804D83C), grava `gCurrentEventStateByte` (0x030057E0+0xC89 = 1) e volta; a parte
"deferred" roda depois pelo despachante de estado. Contagem de bytes = bytes lidos no caminho linear até o primeiro `bx`.
Divergências C# x ROM marcadas com "CONFERIR". Confiança geral: bytes lidos = confirmado no código; nomes = provável.

## 0x23 Move_Actor_A (Event_Op23_MoveActorA)
- O que faz: resolve slot de ator (0x0804DBF0) e move o ator; entra em execução de passos adiada.
- Args: 2 bytes (slot, move). C# bate. PC: parcial (extract_events.py decodifica; script.lua `actor_move` existe, sem checar 1 vs 2 bytes).
- Memória: 0x030057E0+0xC94 (slot ativo), 0x030057E0+0xC89 (estado), tabela de slots de objeto.
- Confiança: bytes confirmado; nome provável.

## 0x24 Move_Actor_B (Event_Op24_MoveActorB)
- O que faz: igual ao 0x23 (variante B; mesmo corpo, mesmo tamanho de leitura).
- Args: 2 bytes (slot, move). PC: parcial (mesmo caso do 0x23).
- Memória: idem 0x23. Confiança: bytes confirmado; nome provável.

## 0x25 Unload_Actor (Event_Op25_UnloadActor)
- O que faz: lê packed_actor_id e remove o objeto rastreado pelo slot (0x08050554 Map_RemoveTrackedObjectByPackedId).
- Args: 1 byte (packed id). PC: implementado (script.lua `actor_unload`).
- Memória: estado 0xC94 (id lido); lista de objetos. Confiança: confirmado (código lido).

## 0x26 Flicker_Actor (Event_Op26_FlickerActor)
- O que faz: resolve slot, grava slot em gEventScriptArguments[10] e arma o pisca por N frames.
- Args: 2 bytes (slot, frames). PC: implementado (script.lua `actor_flicker`). Confiança: bytes confirmado; nome provável.

## 0x27 Actor_Do_Animation (Event_Op27_ActorDoAnimation)
- O que faz: resolve slot, aplica variante de virada (0x08052D90), lê animação e tempo, entra em execução adiada.
- Args: 3 bytes (slot, animation_id, frames). PC: falta (extract_events.py não cita 0x27).
- Memória: objeto (0x03002C30 ao lado), estado 0xC89. Confiança: bytes confirmado; nome provável.

## 0x28 Hop_Actor (Event_Op28_HopActor)
- O que faz: resolve slot, semeia o salto a partir da posição Y atual e entra em execução adiada.
- Args: 1 byte (slot). PC: falta. Confiança: bytes confirmado; nome provável.

## 0x29 SetNpcWalkAnimation (Move_All_Tracked_Actors_A no C#)
- O que faz: lê 1 byte e grava em estado do objeto de todos os atores (0x08047D9C nao identificado no mtbe.csv).
- Args: 1 byte. PC: falta. Nome C# (move_pattern) diverge do mtbe.csv (SetNpcWalkAnimation): CONFERIR. Confiança: chute no efeito.

## 0x2A SetNpcAnimationFrame (Move_All_Tracked_Actors_B no C#)
- O que faz: como 0x29, outro byte de quadro (mesmo grupo de chamadas).
- Args: 1 byte. PC: falta. CONFERIR nome. Confiança: chute.

## 0x2B ConfigureActorAttachment (Initiate_Free_Tracked_Actor no C#)
- O que faz: cria ator livre: packed id, sprite, x, y e movimento (aloca slot com 0x08050710 e grava com 0x0804DBD0).
- Args: 5 bytes (packed id, sprite, x, y, move). PC: falta (extract cita spawn 0x1F/0x21, não 0x2B). Confiança: bytes confirmado.

## 0x2C SetActorPriority (Set_Actor_Render_Priority no C#)
- O que faz: lê 1 byte (modo de prioridade) e aplica ao render dos atores.
- Args: 1 byte. PC: falta. Confiança: bytes confirmado; efeito chute.

## 0x2D Move_Actor_C (SetActorTileOffset no mtbe.csv)
- O que faz: resolve slot e aplica movimento (2 bytes). Nome no mtbe.csv (SetActorTileOffset) diverge do C# (Move_Actor_C): CONFERIR.
- Args: 2 bytes (slot, move). PC: falta (extract trata só 0x23/0x24/0x6D). Confiança: bytes confirmado.

## 0x2E Adjust_Value_If_No_Spare_Part
- O que faz: lê bot, parte e quantidade (3 bytes); chama 0x08047D9C (desconhecida) e ajusta valor se não há peça reserva.
- Args: 3 bytes (bot, part, amount). PC: falta. Confiança: bytes confirmado; nome C# provável.

## 0x2F Set_Value_From_EventFlag_Nibble (salto multi no extract)
- O que faz: soma um byte lido ao halfword de estado (0x080532D4 com halfword de [base+0xC8E]) e grava de volta.
- Args: ROM lê 1 byte da stream (0x0804D83C); C# diz 2 (arg2, base_value). CONFERIR: divergência de tamanho.
- PC: parcial (extract_events.py trata 0x2F como multijump de 4 ou mais bytes, outro sentido). Confiança: divergente.

## 0x30 Relative_Long_Jump
- O que faz: salto incondicional; soma Short big-endian com sinal ao cursor.
- Args: 2 bytes (Short). PC: implementado (extract/script.lua tratam como salto `always`). Confiança: confirmado (código lido).

## 0x31 Yes_or_No_Box
- O que faz: abre caixa Sim/Não; o salto vai no byte lido.
- Args: 1 byte (jump). PC: implementado (`yesno`; ordem Sim/Não é palpite do extract). Confiança: bytes confirmado.

## 0x32 No_or_Yes_Box
- O que faz: igual ao 0x31 com ordem invertida.
- Args: 1 byte. PC: implementado (ordem inversa é palpite). Confiança: bytes confirmado.

## 0x33 Start_Battle
- O que faz: lê id da batalha e os dois bytes de modo (pré e pós-batalha); entra no modo batalha.
- Args: 3 bytes (battle id, battle_mode_flags, post_battle_mode_flags). PC: implementado (battle id; os bytes de modo são ignorados). Confiança: confirmado.

## 0x34 Rotate_NPC
- O que faz: resolve o evento da pessoa (Short) e grava a direção.
- Args: 3 bytes (Short event id + dir). PC: parcial (extract trata id; direção vai para face_event, sem checar). Confiança: bytes confirmado.

## 0x35 Set_All_Tracked_Actors_Facing_Variant
- O que faz: lê 1 byte e aplica a variante de virada a todos os atores (0x030055A0 no caminho).
- Args: 1 byte. PC: falta. Confiança: bytes confirmado.

## 0x36 Get_Item
- O que faz: soma item (id, qtd) ao inventário do save.
- Args: 2 bytes (item_id, amount). PC: implementado (`item`, give=true). Confiança: confirmado.

## 0x37 Lose_Item
- O que faz: subtrai item.
- Args: 2 bytes. PC: implementado (`item`, give=false). Confiança: confirmado.

## 0x38 Get_Part
- O que faz: dá peça (bot, parte, quantidade) para o save.
- Args: 3 bytes. PC: implementado (`part` com "extra" não confirmado). Confiança: bytes confirmado.

## 0x39 Lose_Part
- O que faz: retira peça.
- Args: 3 bytes. PC: implementado (`part`, give=false). Confiança: bytes confirmado.

## 0x3A Get_Money
- O que faz: soma dinheiro (Short).
- Args: 2 bytes (Short). PC: implementado (`money`). Confiança: confirmado.

## 0x3B Lose_Money
- O que faz: subtrai dinheiro (Short).
- Args: 2 bytes. PC: implementado (`money`). Confiança: confirmado.

## 0x3C Get_Medal
- O que faz: lê 1 byte (medalha) e concede.
- Args: 1 byte. PC: falta. Confiança: bytes confirmado; efeito provável.

## 0x3D Get_Tinpet
- O que faz: lê 1 byte (tinpet) e concede.
- Args: 1 byte. PC: falta. Confiança: bytes confirmado.

## 0x3E Increase_Capped_Event_Counter
- O que faz: soma quantia (com teto) ao contador de evento indicado.
- Args: 2 bytes (counter_slot, amount). PC: falta. Confiança: bytes confirmado; teto não lido.

## 0x3F Decrease_Capped_Event_Counter
- O que faz: subtrai quantia do contador (com piso).
- Args: 2 bytes. PC: falta. Confiança: bytes confirmado.

## 0x40 Pulse_Screen_Transition
- O que faz: lê 1 byte (ciclos) e dispara a transição de tela.
- Args: 1 byte. PC: falta. Confiança: bytes confirmado.

## 0x41 Flicker_All_Tracked_Actors
- O que faz: lê 1 byte (frames) e arma pisca de todos os atores.
- Args: 1 byte. PC: falta. Confiança: bytes confirmado.

## 0x42 NoOp (Move_Player_One_Step no C#, NoOpWithArgs no mtbe.csv)
- O que faz: nada. A função é só `bx lr` (0x0804C928).
- Args: 0 bytes. PC: falta, mas é trivial (ignorar e seguir). Nome C# "Move_Player_One_Step" NÃO bate com o código: CONFERIR.
- Confiança: confirmado (código lido).

## 0x43 Shake_Camera (ApplyMapSceneModePolicy3 no mtbe.csv)
- O que faz: lê 2 bytes no caminho (axis, duration). Chamadas no caminho linear: nenhuma.
- Args: 2 bytes. PC: falta. CONFERIR: nome C# (Shake_Camera) x mtbe.csv (ApplyMapSceneModePolicy3) divergem; tabela da ROM aponta 0x0804C92C.
- Confiança: bytes confirmado; nome chute.

## 0x44 Play_Sound (ApplyMapSceneModePolicy4 no mtbe.csv)
- O que faz: lê 1 byte e chama `Overworld_StartMapTransitionEffect` (0x08047BA8) no caminho. Isto NÃO parece som.
- Args: 1 byte. PC: falta. CONFERIR: tabela da ROM aponta 0x0804C94C (mtbe.csv nomeia 0x0804C9C4 como 0x44, endereço fora da tabela).
- Confiança: bytes confirmado; nome chute (provavelmente errado).

## 0x45 Play_Music (ApplyMapSceneModePolicy no ROM)
- O que faz: lê 1 byte e chama `Overworld_ApplyMapSceneModePolicy`. Não parece música.
- Args: 1 byte. PC: falta. CONFERIR: nome C# (Play_Music) x código; tabela aponta 0x0804C960.
- Confiança: bytes confirmado; nome chute.

## Achados para o mtbe.csv (não editado)
- Nomes EventOpcode42/43/44 de `re/symbols/mtbe.csv` (0x0804C928 ok; 0x0804C9A0 e 0x0804C9C4 não estão na tabela).
- C# (`EventOpcodeDefinitions.cs`) e ROM discordam em 0x29..0x2D, 0x2F (tamanho), 0x42..0x45 (nomes).
