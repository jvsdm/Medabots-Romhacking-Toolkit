# JT8: tabelas de salto de PartDetail_DispatchSubstate (0x0800E78C) e tabela 0x083AC9F8 (GH-JT8, 2026-10-14)

Escopo: os 3 saltos indiretos de 0x0800E78C (FS3.md) e a tabela 0x083AC9F8 (sem nome no mtbe). Método: disassemble
no Ghidra (leitura), leitura de palavras da ROM original com python -I (só leitura), decisão por caso. Ghidra: 3
data labels renomeados (não salvo). Nada de commit, mtbe.csv intocado.

## 1. PartDetail_DispatchSubstate 0x0800E78C: estrutura

- Lê o modo em `[ctx+0x3E]` (ctx = IWRAM 0x03000440) e o sub-índice em `[ctx+0x44]`.
- Roteamento por modo (pela ordem de comparação no código):
  - modo 0 -> ramo 0x0800E7B8: sub <= 6, salto por **tabela A** em 0x0800E7D0 (`mov pc,r0` em 0x0800E7CA).
  - modo 1 ou 2 -> ramo 0x0800E86C: sub <= 7, salto por **tabela B** em 0x0800E884 (`mov pc,r0` em 0x0800E87E).
    O modo 2 cai no mesmo ramo do modo 1 (`cmp r1,#1 / bge` cobre 1 e 2).
  - modo 3 -> ramo 0x0800E930: sub <= 7, salto por **tabela C** em 0x0800E948 (`mov pc,r0` em 0x0800E940).
  - outro valor -> 0x0800E9D4 (retorno sem desenhar).
- Cada caso carrega um ponteiro (payload) e faz `bl 0x0803FB3C` (StateMachine_DispatchStep). O payload fica em
  `[0x03000448]` (+8 do bloco 0x03000440); o despachante chama o handler do estado (byte +4). O retorno é "estado > 7" (terminou).
- Se terminou (r4 != 0): `bl 0x0803FB0C` (Obj_ResetStateFields) e `[0x0300137C] = 0x21`. Esse 0x21 é o passo
  seguinte da cena: entrada 0x21 da tabela 0x083ACCD4 = **0x0800E690 `PartDetail_AnimSlotDispatchStep`** (já nomeado).
- Não há incremento do sub-índice `[ctx+0x44]` dentro desta função: quem avança o sub-estado está fora dela
  (pendente: provável no PartDetail_AnimSlotDispatchStep).

Fontes de dados (ponteiros de texto da ROM, lidos; nomes do mtbe são de rótulo e não batem exatamente):
- 0x083B82F0 (8 entradas): texto por especialidade. Índice = byte `[ctx+0xF0]` = 0x03000530 (scratch do registro
  da peça copiado em PartDetailScreen_Init). 8 entradas = 8 especialidades (ver comentário de 0x08042B80).
- 0x083B6FA4 (26 entradas): texto de medalha/compatibilidade. Índice = byte `[ctx+0xEE]` = 0x0300052E
  (MedaforceScratch; EN.md diz que a medalha é copiada para esse scratch).
- 0x08042B80 `Speciality_IsRemotenessClass(F0)`: devolve 1 para especialidade de distância, 0 para proximidade.
  O resultado +0x11 indexa 0x083B5578 (`B5578[0x11]` ou `[0x12]`): texto de classe (proximidade x distância).
- 0x083BDA04 (`gTextStatDescriptions` no mtbe, mas a tabela passa de 11 entradas: índice 0x13 é usado):
  descrições de estatística, indexadas por número fixo de linha.

### Tabela A (modo 0, sub 0..6), literal 0x0800E7CC -> 0x0800E7D0

| Sub | Alvo (label) | Payload | O que aparece na tela de detalhe de peça |
|---|---|---|---|
| 0 | 0x0800E7EC | B82F0[F0] | descrição da especialidade da peça |
| 1 | 0x0800E804 | B6FA4[EE] | descrição da medalha / compatibilidade |
| 2 | 0x0800E81C | B5578[class+0x11] | texto de classe (proximidade ou distância) |
| 3 | 0x0800E83C | DA04[0] | linha de estatística 0 |
| 4 | 0x0800E848 | DA04[1] | linha de estatística 1 |
| 5 | 0x0800E854 | DA04[2] | linha de estatística 2 |
| 6 | 0x0800E860 | DA04[3] | linha de estatística 3 |

### Tabela B (modos 1 e 2, sub 0..7), literal 0x0800E880 -> 0x0800E884

| Sub | Alvo (label) | Payload | Observação |
|---|---|---|---|
| 0 | 0x0800E8A4 | B82F0[F0] | especialidade |
| 1 | 0x0800E8BC | B6FA4[EE] | medalha |
| 2 | 0x0800E8D4 | B5578[class+0x11] | classe |
| 3 | 0x0800E8F4 | DA04[0] | estatística 0 |
| 4 | 0x0800E900 | DA04[1] | estatística 1 |
| 5 | 0x0800E90C | DA04[2] | estatística 2 |
| 6 | 0x0800E918 | DA04[4] | estatística 4 (pula [3]) |
| 7 | 0x0800E924 | DA04[5] | estatística 5 |

Modo 1 e modo 2 têm a mesma lista; o que muda entre eles não está nesta função (não decidido).

### Tabela C (modo 3, sub 0..7), literal 0x0800E944 -> 0x0800E948

| Sub | Alvo (label) | Payload | Observação |
|---|---|---|---|
| 0 | 0x0800E968 | B6FA4[EE] | medalha (tela de compatibilidade medalha x peça: provável) |
| 1 | 0x0800E980 | DA04[0x13] | texto fora do tamanho do rótulo; tabela talvez outra |
| 2 | 0x0800E98C | DA04[0] | estatística 0 |
| 3 | 0x0800E998 | DA04[6] | |
| 4 | 0x0800E9A4 | DA04[7] | |
| 5 | 0x0800E9B0 | DA04[8] | |
| 6 | 0x0800E9BC | DA04[9] | |
| 7 | 0x0800E9C8 | DA04[10] | |

Confiança: alvos e payloads = confirmado (código e ROM lidos). Significado de cada linha na tela = provável
(nomes pelo conteúdo dos textos, não por screenshot). Modos 0..3 como "tipo de visão" = provável.

## 2. Tabela 0x083AC9F8: não é cabeçalho

- Os primeiros valores (0x08009275, 0x080092D5, 0x08009375, 0x08000841) não são contagem nem ponteiro de tabela:
  são ponteiros de função (Thumb). Então não é cabeçalho.
- Lendo ao redor, a região parece linhas de 3 ponteiros (stride 0xC) começando em **0x083AC9EC**:
  - 0x083AC9EC: 0x08009175 (`PartsGrid_ReloadRowTilesAndSetup`, PS.md), 0x080091E1, 0x08009249
  - **0x083AC9F8: 0x08009275 (`PartsGrid_TickFadeOutStep`), 0x080092D5 (`PartsGrid_TickRouteSelection`), 0x08009375 (`PartsGrid_TickDispatchSubstate`)**
  - 0x083ACA04: 0x08000841 (stub), 0x08000841, 0x08007965
  - 0x083ACA10: 0x08007999, 0x080093AD, 0x080093E1 (`0x080093E0`), depois 0x08000841 em sequência
  - depois, área de pares (x,y) 0x00480008...
- Conclusão: 0x083AC9F8 é o registro 1 de uma tabela de estados da grade de peças, do mesmo tipo da
  `table_partsgrid_states` (0x083AC880), mas NÃO alinhada com ela (0x083AC9EC - 0x083AC880 não é múltiplo de 0xC).
  Provável: tabela de estados da grade (3 ponteiros por estado). Não confirmei quem lê esta região
  (pendente: xref de dados para 0x083AC9EC/0x083AC9F8).
- Sugestão de nome (provável): `table_partsgrid_rows_9EC`.

Confiança: parcial. A leitura por stride 0xC é provável; os 0x08000841 são stub.

## 3. Flag 0x4000: PartsBuild_MarkSlotFlagStep 0x08010170

Decompile lido. Literais: base = DAT_08010198 = **0x03002C30 (gObjectSlots)**; offset do byte de estado =
DAT_0801019C = 0x127; offset do contador = DAT_080101BC = 0x127; contador de 0x1E = DAT_080101E4 = 0x129;
máscara DAT_080101A0 = 0xBFFF.

Lógica (halfword em base+0x100 = 0x03002D30):
- Se `byte[base+0x127] == 0x1E`: liga **0x4000** em base+0x100, e incrementa `byte[base+0x129]`.
- Senão, se `byte & 4 == 0`: liga 0x4000 (OR).
- Senão (bit 2 ligado): zera 0x4000 (AND 0xBFFF).
- Nos casos não-0x1E, incrementa o próprio byte `[base+0x127]` (contador de passo).

Significado: o mesmo padrão de FL6.md / FN1.md / FR1.md (0x1E e bit 2 do byte), com a base gObjectSlots.
Nas notas de FL9.md, o bit 0x4000 de OAM é usado para ocultar camada. Então 0x4000 ligado = oculto (provável; não
confirmado no emulador). Confiança: confirmado (lógica e endereços); significado do bit = provável.

## 4. Pendências

- Quem avança `[0x03000484]` (sub-índice ctx+0x44) entre os casos: provável PartDetail_AnimSlotDispatchStep 0x0800E690.
- Modo 1 x modo 2: o que muda na tela (não decidido nesta função).
- Tabelas 0x0800E7D0 / 0x0800E884 / 0x0800E948: renomeadas no Ghidra (data labels), não salvas.
- Destino de 0x08042B80: o nome do mtbe "Speciality_IsRemotenessClass" vem do comentário da própria função; não revisado.
