# VA2: quem escreve E+0xC28 (variante de cena, 0x03006408) e com quais valores (GH-VA2, Medabots Metabee EU)

Status: **escritores fechados** (6 funções com store para E+0xC28, todas confirmadas no disassembly e nos literais). Valores: confirmados por código. Ligação com scripts: confirmada para o opcode 0x11; o restante dos scripts que alteram E+0xC5C não foi varrido (pendente, ver §6).
Método: Ghidra MCP (só leitura, sem salvar; único rename feito, ver §7), literais lidos na ROM com `python -I`. Sem emulador, sem commit, mtbe.csv não editado.
Símbolos: `VA2_simbolos.csv`. Base do bloco E = 0x030057E0 (E+0xC28 = 0x03006408, E+0xC5C = 0x0300643C, E+0xC69 = 0x03006449, E+0xC94 = 0x03006474).

## 1. Correção ao VA.md (§6) e à nota do research README

- VA.md dizia que os 6 literais com 0xC28 eram **READ**. Errado: são **stores** (`strb`) para E+0xC28. Os literais são o valor 0xC28 (offset) somado a E, não o endereço absoluto. Por isso "não há literal 0x03006408" continua certo.
- A pendência "escritor de E+0xC28 aberto" do `pc/research/README.md` está fechada por esta rodada (ver §2).

## 2. Escritores de E+0xC28 (todos os 6 xrefs de literal 0x0C28, sem exceção)

Cada escritor faz `strb` no byte E+0xC28 e, logo depois, chama `Map_LoadSecondaryMarkerRecord` 0x0805330C (confirmado nos 6 casos no disassembly).

| # | Função (endereço) | Store (instr.) | Valor gravado em E+0xC28 | Condição |
|---|---|---|---|---|
| 1 | Overworld_InitPlayerSprites 0x080517F0 | 0x0805180E | **E+0xC5C** (walk state) | Antes zera E+0xC58 (0x080517FA..0x08051802). Init do sprite do jogador. |
| 2 | Map_FinishEnterTransition 0x0804B008 | 0x0804B0EA | **E+0xC5C** | Fim da entrada de mapa. |
| 3 | Overworld_SetActorAnimFrameByTable 0x08051E38 | 0x08051EAA | **E+0xC5C** | Só no ramo `arg != 0` (0x08051E40 `bne`). O ramo `arg == 0` é o DMA de gráficos (0x083F83C0 / 0x083D2B26), sem store em C28. |
| 4 | Screen_UpdateByStep 0x0805830C (miolo em 0x08058888) | 0x08058912 | **E+0xC5C** | Estado 3 da máquina de cena (JT em 0x08058330), só se `[r4+0x12]` = 0x0E ou 0x12. |
| 5 | Screen_HandleStateTransition 0x080594C8 | 0x0805952A | **E+0xC5C** (que acabou de ser zerado) | Ramo `0x0805424C() == 4`; antes `strb 0, E+0xC5C` (0x080594F6). Logo C28 = 0 nesse caminho. Só se `[r5+0x12]` = 0x0E ou 0x12. |
| 6 | EventOpcode11 continuação 0x0804E370 (renomeada, §7) | 0x0804E396 | **ROM 0x083D2B18[E+0xC94]** | Valor vem do argumento do script (opcode 0x11). Mesmo valor vai para E+0xC5C e E+0xC69. |

Literais (lidos na ROM, offsets relativos a E): item 1 usa 0xC58 (zera) e 0xC5C → 0xC28; item 2 usa 0xC5C → 0xC28; item 3 usa 0xC5C → 0xC28; item 4 usa 0xC5C → 0xC28; item 5 usa 0xC5C → 0xC28 (e DMA 0x040000D4 no ramo); item 6 usa 0xC94 (arg), 0xC5C, 0xC69, 0xC28 e a tabela 0x083D2B18.

### 2.1 Quem chama os escritores (o que é a "variante de cena")

Os caminhos 1-5 copiam o walk state E+0xC5C para E+0xC28. Eles rodam em transições de mapa (entrada, init do sprite, máquina de cena). Não é script que escreve C28 diretamente: script só entra pelo item 6.

## 3. Opcode 0x11 (ChangePlayerCostume) e a ligação com o script

- Tabela primária 0x083D222C[0x11] = **0x0804BBA0** `Event_Op11_ChangePlayerCostume` (confirmado). Continuação 0x083D2630[0x11] = **0x0804E370** (confirmado).
- Primário (0x0804BBA0): lê 1 byte do script (`0x0804D83C`) e grava em **E+0xC94** (arg0 do evento); depois E+0xC89 = 1 (liga continuação, diferido). Confirmado pelos literais 0xC94 e 0xC89.
- Continuação (0x0804E370): `E+0xC5C = ROM[0x083D2B18 + arg0]`, `E+0xC69 = mesmo valor`, `E+0xC28 = mesmo valor` (item 6), depois chama 0x08048684 (recarga de paleta/sprites, função não lida) e termina com 0x08050290 (Event_EndOpcode). Confirmado no disassembly.
- Tabela 0x083D2B18 (início, lido): `00 03 04 05 06 07 08 02 09 01 0A 0E 0F 0B 00 70 71 72 73 74 75 76 77 78 ...`. Tamanho total não medido. Índice = arg0 do script. Isto é a chave: **o script escolhe a variante e o valor é o byte da tabela**, não o arg cru.
- Obs.: o nome antigo `EventOpcode11` no mtbe.csv é do sync e não é decimal; o índice é 0x11 em hexa (tabela de continuação). Não confundir com 0x0B (warp A, 0x0804B9B0) nem 0x0C (warp B).

## 4. Cadeia completa (o que vale na carga de cena)

1. Script opcode 0x11 (arg0) → continuação 0x0804E370 → E+0xC5C = E+0xC69 = E+0xC28 = `ROM 0x083D2B18[arg0]`.
2. Em cada entrada/transição de mapa, os escritores 1-5 recopiam E+0xC5C → E+0xC28. Em `Overworld_UpdateSecondaryActorAnim` 0x08052C30: se `E+0xC5C == 1` e `0x08048088() == 0`, faz `E+0xC5C = E+0xC69` e chama `Overworld_SetActorAnimFrameByTable(1)` (item 3), que regrava E+0xC28 = E+0xC5C.
3. Carregador de cena: `Map_LoadCurrentSceneResources` 0x0804DDF2 → `Map_LoadSecondaryMarkerRecord` 0x0805330C. Lê **E+0xC28** como índice do bloco de 8 B: copia `ROM 0x083D2A98 + C28*8` para E+0xC10..C17 (passabilidade, VA.md §2). Também grava 0x0300381A = `ROM 0x083D2A88[E+0xC5C]` (velocidade). Confirmado (leitura do 0x0805330C em VA.md; literal 0xC28 no 0x08053314 com [READ]).
4. Efeito: a passabilidade da cena depende de C28 (índice de bloco) e a velocidade depende de C5C.

Consequência: o byte C28 vale `ROM 0x083D2B18[arg0]` no primeiro frame após o 0x11 e depois é recopiado de C5C em cada transição. O opcode 0x69 (Event_Op69_Set_Map_Scene_Variant) NÃO escreve C28; mexe em E+0xC5B (variante, outro byte) e em E+0xC10..C1F. Logo, C5B e C28 são variáveis diferentes, que não se confundem.

## 5. Quem põe E+0xC5C (walk state) — o que sobra

Confirmados neste passo:
- Opcode 0x11 cont. (0x0804E370): `= ROM 0x083D2B18[arg0]`.
- Screen_HandleStateTransition 0x080594C8: zera (`strb 0`).
- Overworld_UpdateSecondaryActorAnim 0x08052C30: copia de E+0xC69 (sob condição).

Não verificados (pendente): a lista de xrefs de literal 0x0300643C tem vários [WRITE] (Overworld_CheckMovementObjectBlocker 0x080525E6, FUN_0804AA44, FUN_0804AD68, FUN_0804AB60, FUN_0804AE80, FUN_0804ADB4, FUN_0804AEC8, FUN_0804ABCC, new_game_start 0x0804A4A2, Shop_ItemListInputStep). O Ghidra marca WRITE pelo literal, não pelo store, então esses precisam de disassembly do store (`strb` com base E). Não feito por falta de contexto.

## 6. Pendências

- Varredura dos [WRITE] de E+0xC5C listados em §5 (um `strb` com base E e offset 0xC5C em cada).
- Tamanho da tabela 0x083D2B18 (quantas entradas, quantos arg0 válidos) e significado dos valores 0x00..0x0E (ids de cena?) e 0x70..0x7E (vistos na tabela 0x083D2B26 também).
- Ligar os scripts que usam 0x11 (contagem e mapa dono) com o extrator (`extract_events.py` não decodifica 0x11 com arg; falta o mapa evento → arg0).
- O que 0x0805424C e 0x08048088 retornam (condições dos caminhos 3 e 5) não foi lido.
- 0x0804A4A2 new_game_start grava algum byte de E+0xC5C/C28 (na lista de xrefs) — não conferido.

## 7. Renames

- Feito no Ghidra (confirmado): `0x0804E370` → `Event_Cont11_ChangePlayerCostume` (é a continuação do opcode 0x11; antes `EventOpcode11_ChangePlayerCostume`). Nada mais renomeado, nada salvo.
- Sugestões (não aplicadas, para mtbe.csv no futuro): ver `VA2_simbolos.csv`.
