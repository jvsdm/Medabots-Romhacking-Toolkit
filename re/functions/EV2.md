# EV2 - uso real dos opcodes de evento (varredura da ROM)

Status: PARCIAL (varredura feita; 0x2F e 0x02 em aberto, ver §1). GH-EV2, 2026-10-14. ROM: `Medabots - Metabee (Europe).gba` (só leitura).
Varredura: script descartável (importa a leitura de `pc/extract/extract_events.py` por caminho, sem alterá-la; `python -I`), apagado ao fim.
Nenhum texto do jogo aparece aqui: só ids, bytes e nomes de opcode do C#.

## 0. Escopo e leitura

- Tabela de eventos no header da base 0x765EC0: **2288 eventos** (ids 0..2287). Cada um foi percorrido do início com BFS (como `decode_event`),
  limite de 512 instruções por evento (o do extract é 64).
- Variantes:
  - **A** = tamanhos do extract (C# + `ROM_SIZE_FIX`), 0x2F como salto multi-jump (não segue a queda), igual ao extract.
  - **B** = tamanhos da tabela legada `LegacyEventOpcodeTable.Lengths`, 0x2F como salto (igual ao extract). Mesmos resultados de erro que A, exceto 0x7D.
  - **C** = tamanhos legados, 0x2F como instrução linear de 2 bytes (o que o handler primário sugere, ver §1.3).
- Resultado de fim de script (A): **2090** chegam a `End` (0x06); **95** só têm goto/saltos; **103** erram.
- Os **99** erros da variante A são o mesmo caso: eventos que apontam para o bloco de zeros em 0x0076B990 (0x00 = Nop sem fim).
  São slots vazios (ids 32, 72, 73, 93, 117..121, 313, 323, 346..348, ...), não script. Não contam como erro de tamanho.
- Os **4** erros restantes da variante A são o opcode **0x7D** (ver §1.2): ids 1954, 2061, 2062, 2063.

## 1. Achados

### 1.1 Opcodes que o extract não reconhece (nenhum deles é reachable fora do 0x7D)

O C# (EventOpcodeDefinitions.cs) não define 0x54, 0x58, 0x67, 0x6A, 0x75, 0x77, 0x7D, 0x7F, 0x88 (0x06 e 0x19 são tratados à parte).
Com o C# sozinho, o extract para no primeiro desses que aparecer. Os tamanhos da tabela legada batem com as notas dos HE1..HE4
(ex.: 0x6A/0x75/0x77 = 0 args; 0x7D e 0x7F = 1 arg; 0x88 = 2 args; 0x67 = 2 args; 0x54/0x58 = 0 args).

### 1.2 0x7D: tamanho certo, falta no C#

- Legado = 2 bytes (1 arg), HE4 "1 arg". Único desses opcodes que aparece no fluxo (4 eventos: 1954, 2061, 2062, 2063).
- Sugestão (não aplicada, `pc/` não editado): acrescentar 0x7D = 2 ao `ROM_SIZE_FIX` do extract, ou definir no C#.
- Confiança: provável (tamanho pelo legado + HE4; não confirmei no handler).

### 1.3 0x2F: suspeito principal (tamanho e tratamento)

- O extract trata 0x2F como **multi-jump** (`MULTIJUMP_OP`): o walk para no 0x2F e não segue a queda. Na variante A, **541** sítios de 0x2F
  (542 eventos) cortam o fluxo. Por isso A tem tantos eventos "só goto/saltos" e um fim de script subcontado.
- O handler primário 0x0804C444 (disassembly Thumb, só leitura) é de efeito linear: lê um u16 de estado, chama 0x080532D4
  (leitura de bit de flag, não toca script), soma o resultado à variável e chama 0x0804D83C (leitor de script) uma vez.
  Ou seja: não tem tabela de saltos no handler primário. A continuação (0x0804E7E0) é NoOp (HC.md).
- Tamanho do 0x2F, teste pelos eventos: 2 bytes (C#/ROM_SIZE_FIX) é o único que deixa o evento 1654 chegar a `End`
  (2F 04 | 0E | 0D 02 | 01 08 00 F2 | 10 31 FF FF FF | 06). Já com linear 2 bytes há 285 eventos com erro, e com 3 bytes há 68
  (ajuste greedy, ver `suspect.py` e `suspect2.py`). Ou seja: **o tamanho não é fixo** ou o 0x0804D83C lê mais de 1 byte; não fechado.
- Recomendação: não mexer no 0x2F no extract antes de ler o 0x0804D83C inteiro. No disassembly ele lê mais de um byte
  (dois u16 do cursor), o que pode explicar o tamanho variável. Confiança: **confirmado** que o extract para no 0x2F;
  **aberto** o tamanho.

### 1.4 Sobreposição e 0x02 (suspeito secundário)

- Sobreposição (instrução começa dentro da anterior): só 1 evento na variante A (evento 1654, 0x04 em 5 e 0x0E em 6). É o 0x2F
  do §1.3: o extract pula para os bytes da "tabela" como alvos de salto.
- Na variante C aparecem caminhos onde 0x18 (Increment_Event_Variable, 3 bytes, HE1) é seguido de 0x02 (Show_Message_B, 4 bytes, mesmo
  helper de 0x01 segundo HE1) com byte de banco 0x00 (banco = byte-2 < 0). O 0x02 nunca aparece como opcode na variante A/B.
  Hipótese: o walk está desalinhado antes do 0x18, ou o 0x02 não é mensagem (HC.md: a continuação do 0x02 é Finish). Não confirmado.

### 1.5 Opcodes nunca usados no fluxo varrido

Não é o mesmo que "inválido": é "sem evento alcançável que o use". Ver Tabela 2.
- Destaque: **0x05 (Wait_For_Button_Press)** não aparece na A nem na B (só na C, onde o walk anda mais). 0x02 também não aparece na A/B.

## 2. Símbolos

`EV2_simbolos.csv` sem linhas: nada novo para o `mtbe.csv` (só os nomes do C# e do HE, já conhecidos).

## 3. Tabelas

## Tabela 1 - opcodes 0x00..0x88 (uso na varredura)

Colunas: `CS` = tamanho total no C# (EventOpcodeDefinitions.cs, com ROM_SIZE_FIX do extract); `Leg` = LegacyEventOpcodeTable.Lengths (total); `usos A` = sitios distintos alcancados pelo walk do extract (2F = salto, corta o fluxo); `ev A` = eventos onde aparece; `usos C` = mesmo com 0x2F linear de 2 bytes (variante C, com lixo de desalinhamento). `-` = sem tamanho.

| op | nome (C#) | no C# | CS | Leg | usos A | ev A | usos C |
|---|---|---|---|---|---|---|---|
| 0x00 | Nop | sim | 1 | 1 | 512 | 50688 | 675 |
| 0x01 | Show_Message_A | sim | 4 | 4 | 4955 | 5126 | 3020 |
| 0x02 | Show_Message_B | sim | 4 | 4 | 0 | 0 | 2 |
| 0x03 | Close_Message_Box | sim | 1 | 1 | 115 | 116 | 80 |
| 0x04 | Wait_X_Frames | sim | 2 | 2 | 881 | 897 | 661 |
| 0x05 | Wait_For_Button_Press | sim | 1 | 1 | 0 | 0 | 40 |
| 0x06 | (sem definicao no C#) | nao | - | 1 | 3100 | 3180 | 1949 |
| 0x07 | Fade_To_Black | sim | 2 | 2 | 14 | 14 | 141 |
| 0x08 | Fade_From_Black | sim | 2 | 2 | 15 | 15 | 98 |
| 0x09 | Fade_To_White | sim | 2 | 2 | 132 | 138 | 143 |
| 0x0A | Fade_From_White | sim | 2 | 2 | 133 | 139 | 110 |
| 0x0B | Warp_A | sim | 6 | 6 | 95 | 99 | 93 |
| 0x0C | Warp_B | sim | 6 | 6 | 18 | 20 | 67 |
| 0x0D | Player_Face_Direction | sim | 2 | 2 | 1156 | 1199 | 819 |
| 0x0E | Player_Hop_In_Place | sim | 1 | 1 | 134 | 141 | 132 |
| 0x0F | Player_Hop_In_Direction | sim | 2 | 2 | 4 | 4 | 16 |
| 0x10 | Move_Player | sim | 5 | 5 | 963 | 988 | 608 |
| 0x11 | Change_Player_Costume | sim | 2 | 2 | 95 | 95 | 76 |
| 0x12 | Add_Event_Permanently | sim | 3 | 3 | 1114 | 1148 | 813 |
| 0x13 | Remove_Event_Permanently | sim | 3 | 3 | 2549 | 2669 | 1675 |
| 0x14 | Remove_Current_Event_Now | sim | 1 | 1 | 586 | 604 | 460 |
| 0x15 | Remove_Current_Event_Now_Keep_Collision | sim | 1 | 1 | 1 | 1 | 8 |
| 0x16 | Remove_Event_Now | sim | 3 | 3 | 758 | 837 | 566 |
| 0x17 | Set_Event_Variable | sim | 4 | 4 | 1260 | 1267 | 673 |
| 0x18 | Increment_Event_Variable | sim | 3 | 3 | 44 | 44 | 41 |
| 0x19 | (sem definicao no C#) | nao | - | 3 | 96 | 96 | 105 |
| 0x1A | NPC_Do_Animation | sim | 3 | 3 | 5 | 5 | 11 |
| 0x1B | NPC_Reset_Animation | sim | 1 | 1 | 98 | 99 | 91 |
| 0x1C | Rotate_Event_NPC | sim | 2 | 2 | 348 | 352 | 202 |
| 0x1D | Hop_Event_NPC | sim | 1 | 1 | 19 | 19 | 23 |
| 0x1E | Flicker_Event_NPC | sim | 2 | 2 | 27 | 27 | 20 |
| 0x1F | Initiate_Moving_Actor_A | sim | 6 | 6 | 352 | 372 | 284 |
| 0x20 | Initiate_Moving_Actor_B | sim | 6 | 6 | 0 | 0 | 8 |
| 0x21 | Initiate_Actor | sim | 5 | 5 | 887 | 926 | 642 |
| 0x22 | Rotate_Actor | sim | 3 | 3 | 920 | 969 | 691 |
| 0x23 | Move_Actor_A | sim | 3 | 3 | 2597 | 2730 | 1958 |
| 0x24 | Move_Actor_B | sim | 3 | 3 | 18 | 18 | 12 |
| 0x25 | Unload_Actor | sim | 2 | 2 | 1228 | 1289 | 910 |
| 0x26 | Flicker_Actor | sim | 3 | 3 | 184 | 191 | 155 |
| 0x27 | Actor_Do_Animation | sim | 4 | 4 | 43 | 48 | 32 |
| 0x28 | Hop_Actor | sim | 2 | 2 | 104 | 112 | 85 |
| 0x29 | Move_All_Tracked_Actors_A | sim | 2 | 2 | 154 | 170 | 127 |
| 0x2A | Move_All_Tracked_Actors_B | sim | 2 | 2 | 8 | 8 | 12 |
| 0x2B | Initiate_Free_Tracked_Actor | sim | 6 | 6 | 7 | 7 | 7 |
| 0x2C | Set_Actor_Render_Priority | sim | 2 | 2 | 20 | 21 | 20 |
| 0x2D | Move_Actor_C | sim | 3 | 3 | 21 | 21 | 17 |
| 0x2E | Adjust_Value_If_No_Spare_Part | sim | 4 | 4 | 18 | 18 | 18 |
| 0x2F | Set_Value_From_EventFlag_Nibble | sim | 2 | 5 | 541 | 542 | 541 |
| 0x30 | Relative_Long_Jump | sim | 3 | 3 | 305 | 308 | 155 |
| 0x31 | Yes_or_No_Box | sim | 2 | 2 | 112 | 113 | 44 |
| 0x32 | No_or_Yes_Box | sim | 2 | 2 | 17 | 17 | 7 |
| 0x33 | Start_Battle | sim | 4 | 4 | 321 | 335 | 223 |
| 0x34 | Rotate_NPC | sim | 4 | 4 | 655 | 686 | 485 |
| 0x35 | Set_All_Tracked_Actors_Facing_Variant | sim | 2 | 2 | 16 | 18 | 8 |
| 0x36 | Get_Item | sim | 3 | 3 | 66 | 66 | 42 |
| 0x37 | Lose_Item | sim | 3 | 3 | 40 | 42 | 20 |
| 0x38 | Get_Part | sim | 4 | 4 | 181 | 182 | 103 |
| 0x39 | Lose_Part | sim | 4 | 4 | 16 | 16 | 20 |
| 0x3A | Get_Money | sim | 3 | 3 | 30 | 30 | 20 |
| 0x3B | Lose_Money | sim | 3 | 3 | 24 | 24 | 5 |
| 0x3C | Get_Medal | sim | 2 | 2 | 23 | 23 | 12 |
| 0x3D | Get_Tinpet | sim | 2 | 2 | 12 | 12 | 8 |
| 0x3E | Increase_Capped_Event_Counter | sim | 3 | 3 | 12 | 12 | 8 |
| 0x3F | Decrease_Capped_Event_Counter | sim | 3 | 3 | 18 | 18 | 12 |
| 0x40 | Pulse_Screen_Transition | sim | 2 | 2 | 0 | 0 | 6 |
| 0x41 | Flicker_All_Tracked_Actors | sim | 2 | 2 | 0 | 0 | 3 |
| 0x42 | Move_Player_One_Step | sim | 1 | 1 | 0 | 0 | 3 |
| 0x43 | Shake_Camera | sim | 3 | 3 | 99 | 105 | 73 |
| 0x44 | Play_Sound | sim | 2 | 2 | 775 | 798 | 541 |
| 0x45 | Play_Music | sim | 2 | 2 | 175 | 189 | 151 |
| 0x46 | Stop_Music | sim | 1 | 1 | 2 | 2 | 6 |
| 0x47 | Fade_Out_Music | sim | 1 | 1 | 15 | 16 | 14 |
| 0x48 | Play_Persistent_Music | sim | 2 | 2 | 102 | 103 | 89 |
| 0x49 | Play_Map_Music | sim | 1 | 1 | 281 | 299 | 226 |
| 0x4A | Jump_If_Not_Player_Direction | sim | 3 | 3 | 149 | 150 | 94 |
| 0x4B | Jump_If_Item_Count_Below_Amount | sim | 4 | 4 | 36 | 36 | 21 |
| 0x4C | Jump_If_Missing_Part | sim | 4 | 4 | 13 | 13 | 9 |
| 0x4D | Jump_If_Not_Has_Medal | sim | 3 | 3 | 28 | 28 | 2 |
| 0x4E | Jump_If_Not_Has_Money | sim | 4 | 4 | 22 | 22 | 3 |
| 0x4F | Jump_If_Previous_Scene_Command_Failed | sim | 2 | 2 | 45 | 46 | 24 |
| 0x50 | Jump_If_Current_Active_Object_Index_Not_Equal | sim | 4 | 4 | 23 | 23 | 16 |
| 0x51 | Random_Jump | sim | 3 | 3 | 70 | 70 | 44 |
| 0x52 | (sem nome) | nao | 3 | 3 | 0 | 0 | 6 |
| 0x53 | Set_Object_Render_Mode | sim | 3 | 3 | 169 | 172 | 112 |
| 0x54 | (sem definicao no C#) | nao | - | 1 | 0 | 0 | 3 |
| 0x55 | Reserved_NoOp | sim | 1 | 1 | 0 | 0 | 1 |
| 0x56 | Load_Entities | sim | 1 | 1 | 510 | 529 | 375 |
| 0x57 | Reserved_NoOp | sim | 1 | 1 | 0 | 0 | 1 |
| 0x58 | (sem definicao no C#) | nao | - | 1 | 0 | 0 | 5 |
| 0x59 | Open_Shop | sim | 2 | 2 | 18 | 18 | 10 |
| 0x5A | Set_Secondary_Marker_Facing_Variant | sim | 2 | 2 | 8 | 8 | 11 |
| 0x5B | Jump_If_Fewer_Than_Three_Complete_Part_Sets | sim | 2 | 2 | 4 | 4 | 0 |
| 0x5C | Jump_If_Missing_Complete_Monochrome_Part_Set | sim | 3 | 3 | 1 | 1 | 2 |
| 0x5D | Update_Overworld_Event_Mode_Flags | sim | 2 | 2 | 10 | 10 | 9 |
| 0x5E | Set_Overworld_Event_Mode_Bit0 | sim | 2 | 2 | 2 | 2 | 3 |
| 0x5F | Set_Random_Encounter_Mode_Flags | sim | 2 | 2 | 19 | 19 | 19 |
| 0x60 | Focus_Event_Object | sim | 4 | 4 | 31 | 31 | 18 |
| 0x61 | Restore_Focused_Event_Mode_Flags | sim | 1 | 1 | 31 | 31 | 16 |
| 0x62 | Equip_Starter_Parts | sim | 1 | 1 | 1 | 1 | 4 |
| 0x63 | Show_Effect | sim | 4 | 4 | 313 | 328 | 238 |
| 0x64 | Reset_Overworld_UI_Render_State | sim | 1 | 1 | 284 | 299 | 224 |
| 0x65 | Reserved_NoOp | sim | 1 | 1 | 0 | 0 | 1 |
| 0x66 | Animate_Tracked_Object_Transition | sim | 3 | 3 | 20 | 27 | 24 |
| 0x67 | (sem definicao no C#) | nao | - | 3 | 0 | 0 | 1 |
| 0x68 | Clear_Collision_For_Packed_Object | sim | 3 | 3 | 0 | 0 | 0 |
| 0x69 | Set_Map_Scene_Variant | sim | 3 | 3 | 116 | 124 | 62 |
| 0x6A | (sem definicao no C#) | nao | - | 1 | 0 | 0 | 1 |
| 0x6B | Set_Current_Chapter | sim | 2 | 2 | 9 | 9 | 5 |
| 0x6C | Initiate_Moving_Actor_C | sim | 6 | 6 | 10 | 10 | 12 |
| 0x6D | Move_Actor_D | sim | 3 | 3 | 158 | 161 | 144 |
| 0x6E | Initiate_Moving_Actor_D | sim | 6 | 6 | 1 | 1 | 2 |
| 0x6F | Move_Actor_E | sim | 3 | 3 | 4 | 5 | 5 |
| 0x70 | Run_Scene_Command_13 | sim | 1 | 1 | 2 | 2 | 2 |
| 0x71 | Run_Scene_Command_00 | sim | 1 | 1 | 1 | 1 | 4 |
| 0x72 | Run_Scene_Command_02 | sim | 1 | 1 | 2 | 2 | 3 |
| 0x73 | Run_Scene_Command_08 | sim | 1 | 1 | 3 | 3 | 4 |
| 0x74 | Reserved_NoOp | sim | 1 | 1 | 0 | 0 | 1 |
| 0x75 | (sem definicao no C#) | nao | - | 1 | 0 | 0 | 0 |
| 0x76 | Start_Medabot_Link | sim | 1 | 1 | 6 | 6 | 16 |
| 0x77 | (sem definicao no C#) | nao | - | 1 | 0 | 0 | 3 |
| 0x78 | Run_Scene_Command_10_If_At_Least_Three_Complete_Part_Sets | sim | 3 | 3 | 10 | 10 | 8 |
| 0x79 | Run_Scene_Command_12 | sim | 2 | 2 | 16 | 16 | 17 |
| 0x7A | Run_Scene_Command_11 | sim | 1 | 1 | 1 | 1 | 3 |
| 0x7B | Jump_If_No_Spare_Part | sim | 4 | 4 | 23 | 23 | 21 |
| 0x7C | Reserved_NoOp | sim | 1 | 1 | 0 | 0 | 2 |
| 0x7D | (sem definicao no C#) | nao | - | 2 | 0 | 0 | 5 |
| 0x7E | Move_All_Tracked_Actors_C | sim | 2 | 2 | 7 | 9 | 4 |
| 0x7F | (sem definicao no C#) | nao | - | 2 | 0 | 0 | 1 |
| 0x80 | Begin_Batched_Object_Command_Block | sim | 1 | 1 | 41 | 41 | 38 |
| 0x81 | Queue_Batched_Object_Command | sim | 9 | 9 | 100 | 100 | 90 |
| 0x82 | Execute_Batched_Object_Command_Block | sim | 1 | 1 | 41 | 41 | 38 |
| 0x83 | Set_Object_Render_Mode | sim | 3 | 3 | 125 | 128 | 99 |
| 0x84 | Set_Map_Scene_Variant_When_Player_Faces_Up | sim | 2 | 2 | 3 | 3 | 3 |
| 0x85 | Begin_Event_Object_Facing_Cycle_Block | sim | 1 | 1 | 91 | 91 | 69 |
| 0x86 | Queue_Event_Object_Facing_Cycle | sim | 9 | 9 | 117 | 117 | 84 |
| 0x87 | Execute_Event_Object_Facing_Cycle_Block | sim | 1 | 1 | 91 | 91 | 71 |
| 0x88 | (sem definicao no C#) | nao | - | 3 | 0 | 0 | 0 |

## Tabela 2 - opcodes nunca usados

- Variante A (extract): 22 ops: 0x02, 0x05, 0x20, 0x40, 0x41, 0x42, 0x52, 0x54, 0x55, 0x57, 0x58, 0x65, 0x67, 0x68, 0x6A, 0x74, 0x75, 0x77, 0x7C, 0x7D, 0x7F, 0x88
- Variante C (2F linear): 4 ops: 0x5B, 0x68, 0x75, 0x88

## Tabela 3 - distribuicao dos argumentos (bytes), 12 opcodes mais usados (variante A)

Cada posicao de argumento: os 4 valores mais frequentes (valor hex:contagem). Posicoes numeradas a partir do byte depois do opcode. Nao contem texto do jogo.

| op | nome | pos | top valores |
|---|---|---|---|
| 0x01 | Show_Message_A | 1 | 06:384, 02:370, 0A:344, 04:339 |
| 0x01 | Show_Message_A | 2 | 00:4408, 01:547 |
| 0x01 | Show_Message_A | 3 | 14:55, 47:51, 07:46, 3F:45 |
| 0x23 | Move_Actor_A | 1 | 00:1365, 01:777, 02:257, 03:153 |
| 0x23 | Move_Actor_A | 2 | 11:308, 21:298, 31:265, 01:252 |
| 0x13 | Remove_Event_Permanently | 1 | 07:372, 03:371, 04:319, 00:293 |
| 0x13 | Remove_Event_Permanently | 2 | 93:22, E6:19, 2D:16, 80:16 |
| 0x17 | Set_Event_Variable | 1 | 00:316, 04:195, 07:146, 06:132 |
| 0x17 | Set_Event_Variable | 2 | EE:24, F1:23, ED:20, 94:19 |
| 0x17 | Set_Event_Variable | 3 | 01:685, 02:307, 03:111, 00:84 |
| 0x25 | Unload_Actor | 1 | 00:559, 01:302, 02:130, 80:74 |
| 0x0D | Player_Face_Direction | 1 | 01:301, 03:289, 02:284, 00:282 |
| 0x12 | Add_Event_Permanently | 1 | 03:181, 00:173, 04:169, 07:125 |
| 0x12 | Add_Event_Permanently | 2 | 88:12, E6:11, 87:11, A8:10 |
| 0x10 | Move_Player | 1 | 11:261, 01:214, 21:136, 31:127 |
| 0x10 | Move_Player | 2 | FF:759, 21:27, 31:24, 01:23 |
| 0x10 | Move_Player | 3 | FF:869, 01:18, 13:18, 21:12 |
| 0x10 | Move_Player | 4 | FF:909, 21:17, 34:6, 35:6 |
| 0x22 | Rotate_Actor | 1 | 00:466, 01:259, 02:93, 03:72 |
| 0x22 | Rotate_Actor | 2 | 02:252, 01:241, 00:215, 03:212 |
| 0x21 | Initiate_Actor | 1 | 00:456, 01:247, 02:101, 03:51 |
| 0x21 | Initiate_Actor | 2 | 10:52, 61:44, 21:43, 11:42 |
| 0x21 | Initiate_Actor | 3 | 07:84, 0B:64, 0D:63, 0A:61 |
| 0x21 | Initiate_Actor | 4 | 06:83, 03:82, 08:77, 04:77 |
| 0x04 | Wait_X_Frames | 1 | 08:251, 14:112, 05:100, 04:87 |
## Tabela 4 - desalinhamento: pai suspeito (variante A e C)

Pai = ultimo opcode do caminho antes do erro (o tamanho dele e o suspeito). Erro = opcode desconhecido, banco de texto invalido (msg fora de 0..15), ou fim de ROM.

| variante | eventos com erro | erro: tipo | pai (op) | n |
|---|---|---|---|---|
| A | 103 | unknown | 12 | 3 |
| A | 103 | unknown | - | 1 |
| C | 285 | unknown | 00 | 47 |
| C | 285 | unknown | 2F | 13 |
| C | 285 | badbank | 0C | 11 |
| C | 285 | unknown | 11 | 9 |
| C | 285 | badbank | 2F | 8 |
| C | 285 | unknown | 09 | 8 |
| C | 285 | badbank | 07 | 7 |
| C | 285 | badbank | 18 | 7 |
| C | 285 | badbank | 13 | 6 |
| C | 285 | badbank | 09 | 5 |
| C | 285 | unknown | 0A | 5 |
| C | 285 | badbank | 0B | 4 |

Sobreposicao (instrucao comeca dentro da anterior), variante A/C: 04->0E:1 | C: 01->00:1, 01->05:1, 01->07:2, 01->0B:1, 01->10:1, 01->11:2, 01->2E:1, 09->13:1, 0B->2E:1, 10->3C:1, 13->04:1, 20->39:1
