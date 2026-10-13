# HE1 - handlers de opcode de evento 0x00..0x22 (tabela primaria)

Status: PARCIAL. Tabela conferida na ROM; handlers NAO lidos no Ghidra (ver bloqueio abaixo).

## Como o despachante funciona (confirmado)
- `Script_DispatchHandler` 0x0804B6F4 (nome do Ghidra; mtbe: EventScript_FetchAndRunPrimaryOpcodeHandler). Le o byte do opcode,
  grava em RAM e chama `CallViaR0(tabela[opcode])`. Literal do pool 0x0804B734 = 0x083D222C (confirmado na ROM).
- Tabela: base **0x083D222C**, 4 bytes por opcode, indice = opcode em hex (ponteiros Thumb). Os 35 ponteiros de HE1_lista.txt
  batem com a ROM (ex.: 0x00 -> 0x0804B73D, 0x21 -> 0x0804C0C5).
- ERRO no mtbe: `symbols/mtbe.csv` diz "tabela 0x083D224C" (desvio de 0x20). A base certa e 0x083D222C.
- Opcodes 0x06 (END) e 0x19 (GOTO) estao na tabela mas NAO tem definicao em EventOpcodeDefinitions.cs; o extrator trata
  0x06/0x19/0x2F a parte (`extract_events.py` linha 39).
- Tamanhos de argumento abaixo: do EventOpcodeDefinitions.cs (bytes apos o opcode). Confianca nos tamanhos: provavel.

## Bloqueio no Ghidra (nao resolvido nesta rodada)
- No projeto aberto, so existe funcao no despachante 0x0804B6F4. Nenhum handler 0x0804B73C..0x0804C100 esta definido
  ("No function found"), com ou sem bit Thumb. Por isso NAO foram renomeados nem decompilados. Para documentar de verdade
  e preciso definir as funcoes (criar a partir do endereco) e so depois rodar decompile/rename.
- Nomes Event_Op<XX>_<Nome> ficam PROPOSTOS no HE1_simbolos.csv, nao aplicados no Ghidra.

## Secoes (uma por opcode)

## 0x0804B73C 00 Nop (Event_Op00_Nop)
- O que faz: nao faz nada. Args: 0 bytes. PC: implementado (ignorado, sem efeito). Confianca: provavel (C# + mtbe "NoOp").

## 0x0804B740 01 Show_Message_A
- O que faz: abre caixa de texto A. Args: 3 bytes = bank (1 byte) + id (2 bytes, big-endian). PC: implementado (`msg`, bank = byte-2).
- Memoria (mtbe init): so pula 3 bytes na fase init. Confianca: provavel.

## 0x0804B74C 02 Show_Message_B
- O que faz: caixa de texto B, mesmo formato de 0x01. Args: 3 bytes. PC: implementado (`msg`). Confianca: provavel.

## 0x0804B778 03 Close_Message_Box
- O que faz: fecha a caixa de texto. Args: 0. PC: parcial (extrator grava `close`; nao ha branch `close` no script.lua). Confianca: provavel.

## 0x0804B78C 04 Wait_X_Frames
- O que faz: espera N quadros. Args: 1 byte (frames). PC: implementado (`wait_frames`). Confianca: provavel.

## 0x0804B7B4 05 Wait_For_Button_Press
- O que faz: espera apertar botao. Args: 0. PC: parcial (extrator grava `wait`; script.lua sem branch para `wait`). Confianca: provavel.

## 0x0804B7C8 06 (END, sem definicao no C#)
- O que faz: fim do script (tratado como END pelo extrator). Args: nenhum. PC: implementado (`end`). Confianca: provavel (extrator).

## 0x0804B858 07 Fade_To_Black
- O que faz: fade para preto em N quadros; mtbe: le 1 byte e prende em 1..16. Args: 1. PC: implementado (`fade`, to_black). Confianca: provavel.

## 0x0804B8AC 08 Fade_From_Black
- O que faz: fade de preto para tela; mtbe: 1 byte com mesma faixa 1..16. Args: 1. PC: implementado (`fade`). Confianca: provavel.

## 0x0804B900 09 Fade_To_White
- O que faz: fade para branco. Args: 1. PC: implementado (`fade`, to_white). Confianca: provavel.

## 0x0804B958 0A Fade_From_White
- O que faz: fade de branco. Args: 1. PC: implementado (`fade`, from_white). Confianca: provavel.

## 0x0804B9B0 0B Warp_A
- O que faz: teleporte (mapa, x, y). Args: 5 bytes = map_id, unk1, unk2, x, y. PC: implementado (`warp`; unk1/unk2 nao decodificados).
- Divergencia: mtbe chama o handler de "SetConfigFlagAndRead5Bytes" (5 bytes, mesmo tamanho). Confirmar no Ghidra. Confianca: provavel.

## 0x0804B9CC 0C Warp_B
- O que faz: teleporte variante B, mesmos 5 bytes. PC: implementado (`warp`, variant B). Divergencia de nome igual a 0x0B (mtbe: ClearConfigFlag). Confianca: provavel.

## 0x0804BA5C 0D Player_Face_Direction
- O que faz: vira o jogador para a direcao. Args: 1 (dir). PC: implementado (`face_player`; ordem N/S/W/E = GUESS no extrator). Confianca: provavel.

## 0x0804BA9C 0E Player_Hop_In_Place
- O que faz: pulo no lugar. Args: 0. PC: falta. Divergencia: mtbe chama de "StartMapTransitionFromStoredState". Confianca: chute.

## 0x0804BAEC 0F Player_Hop_In_Direction
- O que faz: pulo na direcao. Args: 1 (dir). PC: falta. Confianca: provavel (so C#).

## 0x0804BB14 10 Move_Player
- O que faz: move o jogador por ate 4 passos (nibble alto = dir, baixo = distancia, 0xF = fim). Args: 4 bytes. PC: implementado (`move_player`). Confianca: confirmado no extrator (ROM conferida: "10 11 FF FF FF").

## 0x0804BBA0 11 Change_Player_Costume
- O que faz: troca o visual do jogador (recarrega sprites). Args: 1 (costume_id). PC: falta. Ver T5_sprites.md (EventOpcode11). Confianca: provavel.

## 0x0804BBC8 12 Add_Event_Permanently
- O que faz: deixa o evento/pessoa visivel de forma permanente. Args: 2 (event_id Short). PC: implementado (`visible` True, permanente).
- Divergencia: mtbe chama de "ClearEventFlagByPackedId". Confirmar. Confianca: provavel.

## 0x0804BBF4 13 Remove_Event_Permanently
- O que faz: esconde o evento de forma permanente. Args: 2. PC: implementado (`visible` False, permanente).
- Divergencia: mtbe chama de "SetEventFlagByPackedId". Confirmar. Confianca: provavel.

## 0x0804BC20 14 Remove_Current_Event_Now
- O que faz: remove a pessoa que disparou o evento (ate recarregar o mapa). Args: 0. PC: implementado (`remove_current`). Confianca: provavel.

## 0x0804BC6C 15 Remove_Current_Event_Now_Keep_Collision
- O que faz: igual a 0x14, mantendo a colisao. Args: 0. PC: implementado (`remove_current`, keep_collision). Confianca: provavel.

## 0x0804BCD4 16 Remove_Event_Now
- O que faz: some com a pessoa do evento indicado ate recarregar o mapa. Args: 2 (event_id). PC: implementado (`remove_event`). Confianca: provavel.

## 0x0804BD08 17 Set_Event_Variable
- O que faz: grava valor (nibble) no estado do evento. Args: 3 = event_id (Short) + value (1). PC: implementado (`setflag`). Confianca: provavel.

## 0x0804BD44 18 Increment_Event_Variable
- O que faz: soma 1 ao estado do evento. Args: 2 (event_id). PC: implementado (`incflag`). Confianca: provavel.

## 0x0804BD70 19 (GOTO, sem definicao no C#)
- O que faz: salto relativo (goto). Args: 2 bytes (offset). PC: implementado (`goto`). Confianca: provavel (extrator; mtbe nao nomeia).

## 0x0804BDD8 1A NPC_Do_Animation
- O que faz: toca animacao N em uma pessoa por frames. Args: 2 (animation_id, frames). PC: falta. Confianca: provavel (C#).

## 0x0804BE50 1B NPC_Reset_Animation
- O que faz: volta a pessoa para a animacao padrao. Args: 0. PC: falta. Confianca: chute.

## 0x0804BEC4 1C Rotate_Event_NPC
- O que faz: vira a pessoa que disparou o evento. Args: 1 (dir). PC: implementado (`face_self`). Confianca: provavel.

## 0x0804BF34 1D Hop_Event_NPC
- O que faz: pulo da pessoa do evento. Args: 0. PC: falta. Confianca: chute.

## 0x0804BFB4 1E Flicker_Event_NPC
- O que faz: pisca a pessoa por frames. Args: 1. PC: falta. Confianca: provavel (C#).

## 0x0804C02C 1F Initiate_Moving_Actor_A
- O que faz: cria ator com movimento (id empacotado, sprite, x, y, move). Args: 5 bytes. PC: implementado (`actor_spawn` com passos).
- Confianca: provavel (extrator; ver E2 em research).

## 0x0804C078 20 Initiate_Moving_Actor_B
- O que faz: variante B de 0x1F, mesmos 5 bytes. PC: falta (extrator nao trata 0x20). Confianca: provavel (C#).

## 0x0804C0C4 21 Initiate_Actor
- O que faz: cria ator (id empacotado, sprite, x, y). Args: 4 bytes. PC: implementado (`actor_spawn`). Confianca: confirmado no extrator (T5/E2).

## 0x0804C100 22 Rotate_Actor
- O que faz: vira o ator do slot. Args: 2 (slot, dir). PC: implementado (`actor_rotate`). Confianca: provavel.

## Resumo PC (0x00..0x22, 35 entradas)
- Implementado: 01, 02, 04, 06, 07, 08, 09, 0A, 0B, 0C, 0D, 10, 12, 13, 14, 15, 16, 17, 18, 19, 1C, 1F, 21, 22 (+ 00 sem efeito).
- Parcial: 03, 05.
- Falta: 0E, 0F, 11, 1A, 1B, 1D, 1E, 20.
