# VA3: tabelas de cena (0x083D2A88, 0x083D2A98, 0x083D2B18), escritores de E+0xC5C e retornos (GH-VA3, Medabots Metabee EU)

Status: PARCIAL (2026-10-14). Continua VA.md e VA2.md. Método: Ghidra MCP só leitura (decompile, disasm, xrefs), bytes lidos da ROM original com `python -I`, scan do opcode 0x11 com `pc/extract/extract_events.py` importado por caminho (só leitura). Sem emulador, sem commit, mtbe.csv não editado, pc/research/README.md não editado (escopo desta rodada).
Renames aplicados no Ghidra (§6). Símbolos: `VA3_simbolos.csv`.

## 1. Tabelas (objetivo 1)

| Tabela | Endereço | Tamanho | Confiança |
|---|---|---|---|
| A (velocidade por walk state) | 0x083D2A88 | 16 B (índice = E+0xC5C, 0..15) | confirmado: leitores 0x0805330C, 0x0804F824, 0x080511EC, 0x08051C0C, 0x08053354 |
| B (passabilidade por cena) | 0x083D2A98 | 16 blocos de 8 B = 128 B (índice = E+0xC28); fim em 0x083D2B18 | confirmado o leitor 0x0805334C (bloco de 8 B). 16 = intervalo até a tabela C e igual ao tamanho de A |
| C (valor por arg0 do 0x11) | 0x083D2B18 | 14 entradas (índice = arg0, 0..13); fim em 0x083D2B26 | confirmado o leitor 0x0804E3B8. Tamanho provável (ver §1.3) |

### 1.1 Tabela B: 16 blocos (bytes 0..7; 1 = bloqueia)

| idx | bytes 0..7 | passabilidade v0..v3 (v = valor de colisão) |
|---|---|---|
| 0 | 00 01 00 01 01 01 01 01 | livre, bloqueia, livre, bloqueia |
| 1 | 00 01 00 01 01 01 01 01 | igual ao 0 |
| 2 | 01 01 00 00 01 01 01 01 | bloqueia, bloqueia, livre, livre |
| 3..8 | 00 01 00 01 01 01 01 01 | igual ao 0 |
| 9 | 01 01 00 00 01 01 01 01 | igual ao 2 |
| 10 | 00 01 00 01 01 01 01 01 | igual ao 0 |
| 11, 12 | 00 00 00 00 01 01 01 01 | livre, livre, livre, livre |
| 13, 14 | 00 01 00 01 01 01 01 01 | igual ao 0 |
| 15 | 00 00 00 00 00 00 00 00 | tudo livre |

- VA.md §3 dizia que os bytes 4..7 valem 01 em todos os blocos lidos (0..9). Isso vale para 0..10 e 13..14; o 11 e o 12 também têm 4..7 = 01. Só o 15 é todo zero. Papel dos bytes 4..7 segue sem identificação.
- Blocos 0..9 batem com VA.md; 10..15 são novos.
- Tabela A (16 B): `00 01 00 00 00 00 00 00 00 00 00 02 00 00 00 01`.

### 1.2 Tabela C: 0x083D2B18

Início (lido): `00 03 04 05 06 07 08 02 09 01 0A 0E 0F 0B | 00 70 71 72 73 74 75 76 77 78 79 7A 7B 7C 00 7E 01 27 ...`
- Índices 0..13 são os valores que o 0x11 põe em E+0xC5C/C69/C28 (§3): 00,03,04,05,06,07,08,02,09,01,0A,0E,0F,0B.
- Índice 14 (0x2B26) = 00. A partir de 0x2B26 começa a área que as funções lêem como fonte DMA (§1.3).
- Valores 0x70..0x7E começam no índice 15 (0x2B27). Nenhum script passa arg0 ≥ 14 (§3), então o 0x11 nunca os alcança. Significado não resolvido; provavelmente dado da área DMA e não ids de cena.

### 1.3 Tamanho da tabela C (inferência)

- O literal 0x083D2B26 é lido por 5 funções (0x08048224, 0x08051E6C, 0x08051EC0, 0x08058934, 0x08059548). VA2 diz que um desses é o ramo DMA de Overworld_SetActorAnimFrameByTable. Logo, de 0x2B26 em diante é outra tabela, e C tem 14 entradas. Provável; o código do 0x11 não checa limite.

## 2. Escritores de E+0xC5C (objetivo 2)

Ghidra marca [WRITE] pelo literal de offset (0xC5C / 0x0300643C). Cada função foi decompilada e os literais lidos na ROM. Todas gravam de fato, exceto onde indicado.

| Função | Store em E+0xC5C | Valor | Condição / observação |
|---|---|---|---|
| Overworld_CheckMovementObjectBlocker 0x080525E6 (nome já existia) | sim | cópia de E+0xC69 | só se C5C == 2 e o tile adjacente é 0 ou 2; depois SetActorAnimFrameByTable(0) |
| GameState_ToggleAndAdvance 0x0804AA44 (era FUN_0804aa44) | sim | C69, ou 0 se C69 == C5C (e zera C69) | confirma a nota da FN2 |
| Overworld_TransitionDone_SetWalk0B 0x0804AD68 (era FUN_0804ad68) | sim | 0x0B (constante) | quando Overworld_RunScreenTransitionStep retorna ≠ 0 |
| Overworld_WalkStateToggleTo1 0x0804AB60 (era FUN_0804ab60) | sim | se C5C == 1: C69; senão C69 = C5C antigo e C5C = 1 | troca com C69 (lado 1) |
| Overworld_SetWalk0C_UiScene 0x0804AE80 (era FUN_0804ae80) | sim | 0x0C (constante) | antes de BeginUiSceneState(0x0E) |
| Overworld_UiDone_RestoreWalkFromC69_A 0x0804ADB4 (era FUN_0804adb4) | sim | cópia de E+0xC69 | quando Overworld_IsUiSceneStateComplete ≠ 0 |
| Overworld_UiDone_RestoreWalkFromC69_B 0x0804AEC8 (era FUN_0804aec8) | sim | cópia de E+0xC69 | igual ao _A, outro estado da máquina |
| Overworld_WalkStateToggleTo2 0x0804ABCC (era FUN_0804abcc) | sim | se C5C == 2: C69; senão C69 = C5C antigo e C5C = 2 | troca com C69 (lado 2) |
| new_game_start 0x0804A4A2 | sim | 0; e E+0xC5B = 1 | os set_zeros (E+0x7FC..0xC4F) não tocam C5C. Confirma "Warp A grava 1" em C5B |
| Shop_ItemListInputStep 0x0805CFB4 | sim, por ramo | 3, 4, 5, 8, 6, 7, 0x0D conforme o item (§2.1) | não aparece por literal de offset; é store em ramo da jump table |

### 2.1 Shop_ItemListInputStep: item usado → E+0xC5C

O despacho lê o id do item de gShopItems (EWRAM 0x02040000, 4 B por entrada, u16). Jump table de 41 ptrs em 0x0805D2C8 (literal 0x0805D2C4), conferida na ROM:

| id do item | destino | E+0xC5C recebe |
|---|---|---|
| 0 | 0x0805D36C | não grava C5C (grava 0 em 0x03006F74 e state 0x0B) |
| 1 | 0x0805D384 | 0x03 |
| 5 | 0x0805D398 | 0x04 |
| 12 | 0x0805D3AC | 0x05 |
| 13 | 0x0805D3C0 | 0x08 |
| 19 | 0x0805D3D4 | 0x06 |
| 39 | 0x0805D3E8 | 0x07 |
| 40 | 0x0805D3FC | 0x0D |
| demais (2..38) | 0x0805D410 | sem C5C (erro / 0x04) |

Confiança: confirmado (disasm e tabela). Nomes dos itens não verificados.

## 3. Opcode 0x11 e arg0 (objetivo 3)

0x11 = Change_Player_Costume, tamanho 2 B (extrator). A continuação 0x0804E370 grava `E+0xC5C = E+0xC69 = E+0xC28 = tabela C[arg0]` (VA2 §3).

Varredura com `decode_event` (2288 eventos, BFS igual ao extrator; script só leitura em scratchpad, apagado no fim):
- 95 ocorrências alcançáveis em 68 eventos.
- arg0: 0 em 59; 1 em 3; 2 em 7; 3 em 3; 4 em 7; 5 em 1; 6 em 1; 7 em 7; 8 em 2; 10 em 1; 12 em 3; 13 em 1. Não aparecem 9, 11 nem ≥ 14.
- Eventos com arg0 ≠ 0 (id: lista de arg0): 166 [0,1,1,1], 235 [13,0], 330 [7,7,2], 335 [8], 357 [2], 392 [7,2], 398 [2], 410 [7,2], 418 [7,2], 472 [10], 617 [12,12,12,0,0,0], 742 [6,3], 770 [4], 978 [5,0], 1415 [0,4,4], 1505 [4,4,0], 1565 [4], 1634 [4,2,3], 1775 [3], 1784 [0,8,0]. Lista de 68 eventos com 0x11 vai no log do scan, não aqui.
- Mapa dono de cada evento: não ligado.

## 4. Retornos (objetivo 4)

**0x0805424C = Scene_DispatchStateHandler** (Ghidra decompila como `void`, mas é tail-call):
- Marca +0xF0 = 1; estado = `+0x1D4 & 0x3F` → +0xF8; código = tabela 0x083D5F70 (literal 0x080542A8)[estado] → +0xF4.
- Se código > 0x8B: `r0 = Overworld_EnterSceneState()` (0x080567F0), e esse r0 vira o código.
- Depois `mov pc, r0` na tabela 0x080542B0 (0x8C entradas). **Não há pop nem retorno próprio**: o handler escolhido devolve o seu r0 direto a quem chamou 0x0805424C (disasm 080542A2, sem pop).
- Logo, o valor que Screen_HandleStateTransition compara com 4 (VA2) é o r0 do handler de estado. O significado do código 4 não foi verificado.

**0x08048088 = Map_GetEncounterRecordByte** (renomeado, provável):
- arg 0: devolve `ROM[0x083BEC10 + E[0x7FE]*8 + 2]`.
- arg 1: devolve `ROM[0x083BEC10 + E[0x7FE]*8 + 3]`.
- Outro arg: devolve 0.
- E[0x7FE] = gCurrentMapId (0x03005FDE, OW/DT). 0x083BEC10 = gMapEncounters (DT, provável). Então 0x08048088(0) == 0 quer dizer "o byte +2 do registro do mapa atual é 0"; (1) é o byte +3.
- Chamadores: Overworld_UpdateSecondaryActorAnim usa (0) == 0 (VA2, R3); FM3 usa (0) e (1) nas opções de menu.

## 5. Pendências

- Tamanho da tabela C (14) é inferência. Confirmar com um 0x11 real no emulador, se precisar.
- Papel dos bytes 4..7 dos blocos da tabela B.
- Valores 0x70..0x7E (fora do alcance do 0x11): não resolvido.
- Mapa evento → mapa dono do 0x11 (§3).
- Significado do código 4 (0x0805424C) e dos handlers da tabela 0x080542B0.
- Nome de gMapEncounters (0x083BEC10): provável, vem de DT.md.

## 6. Renames no Ghidra (aplicados; mtbe.csv não editado)

- 0x0804AA44 → GameState_ToggleAndAdvance (confirmado)
- 0x0804AB60 → Overworld_WalkStateToggleTo1 (confirmado)
- 0x0804ABCC → Overworld_WalkStateToggleTo2 (confirmado)
- 0x0804AD68 → Overworld_TransitionDone_SetWalk0B (confirmado)
- 0x0804AE80 → Overworld_SetWalk0C_UiScene (confirmado)
- 0x0804ADB4 → Overworld_UiDone_RestoreWalkFromC69_A (provável)
- 0x0804AEC8 → Overworld_UiDone_RestoreWalkFromC69_B (provável)
- 0x08048088 → Map_GetEncounterRecordByte (provável)

Símbolos e comentários para o mtbe.csv no futuro: `VA3_simbolos.csv`.
