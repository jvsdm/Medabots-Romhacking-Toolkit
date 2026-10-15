# FM3: menu de campo, fechamento (GH-FM3, 2026-10-14, PARCIAL, em andamento)

Leitura: ROM original (só leitura, python -I, helper de rascunho fora do repo). Ghidra: decompile/xrefs, nada salvo, nada renomeado.
Continua de FM.md, FM2.md. Símbolos: FM3_simbolos.csv. Código sem função: FM3_lista.txt.

## Passo 0: tabelas lidas da ROM (confirmado, bytes)

- `menu_list` 0x083AC2C8: [0] = 0x08002555 (Dispatch_CallIndexedHandler), [1] = 0x080027BD (cena 1 = handler 0x080027BC), [2] = 0x08003375.
- `Overworld_ChoiceSubstateTable` 0x083D5FF4 (6 entradas, sub 0..5):
  - sub 0 = 0x0805718C (Overworld_InitChoiceMenuScreen)
  - sub 1 = 0x08057440 (Map_TickChoiceListOpen)
  - sub 2 = 0x080574D4 (FieldMenu_ChoiceSub2_SelectRow)
  - sub 3 = 0x080575DC (função desde o sync de 2026-10-14 (nome: Menu_ChoiceCountdownStep); a checar)
  - sub 4 = 0x08057688 (idem)
  - sub 5 = 0x08057698 (idem)

## Passo 1: o que a tecla B faz (lido)

- `menu_list` [1] (gamestate_menu=1 com gMainState=0) = 0x080027BC. O Ghidra NÃO tem função nesse endereço
  (disassemble_function: "No function found"). Ainda a decompilar por bytes; ver passo 1b.
- `MainState1` 0x083D2104 (menu 1 = mapa): [0] = 0x08000840 (NoOp), [1] = 0x0804A220 (VM de eventos), [2] = 0x0805716C (menu de escolha).
- `Overworld_InitChoiceMenuScreen` 0x0805718C (decompile, confirmado): zera contadores do menu (+0x66, +0x67, +0x6F, +0x6E),
  +0x6C = 0x10, +0x6D = 3, depois chama `GameState_AdvanceSubroutine` (sub +1). Para k = 0..3 chama
  `Overworld_CheckStepAllowed(k)`; se 1, carrega a faixa de fundo ATIVA e chama `Map_DrawChoiceBackgroundRow(k)`;
  senão carrega a faixa INATIVA. Depois carrega a imagem comum (Assets_LoadPairAndFastCopy e decompress_and_store).
  Ou seja: a ordem das linhas na tela é k = 0,1,2,3 de cima para baixo (provável pela ordem de desenho; a
  posição exata é o que `Map_DrawChoiceBackgroundRow` faz, a conferir).

## Passo 2: tabela 0x083D5FF4 inteira (16 entradas, lidas dos bytes; confirmado)

Valor na ROM (bit Thumb) -> endereço real = valor - 1. Coluna Ghidra = o que o Ghidra tem.

| sub | valor na ROM | endereço real | Ghidra | nota |
|---|---|---|---|---|
| 0 | 0x0805718D | 0x0805718C | Overworld_InitChoiceMenuScreen | monta o menu (passo 1) |
| 1 | 0x08057441 | 0x08057440 | Map_TickChoiceListOpen | abertura |
| 2 | 0x080574D5 | 0x080574D4 | função desde o sync de 2026-10-14 (nome: FieldMenu_ChoiceSub2_SelectRow) | seleção de linha (FM2 §3) |
| 3 | 0x080575DD | 0x080575DC | função desde o sync de 2026-10-14 (nome: Menu_ChoiceCountdownStep) | a checar |
| 4 | 0x08057689 | 0x08057688 | função desde o sync de 2026-10-14 (nome: Screen_StartFlowCmd40) | a checar |
| 5 | 0x08057699 | 0x08057698 | função desde o sync de 2026-10-14 (nome: Screen_TransitionEnterAndAdvance) | a checar |
| 6 | 0x080576BD | 0x080576BC | ? | a checar |
| 7 | 0x080576D1 | 0x080576D0 | ? | a checar |
| 8 | 0x080576E1 | 0x080576E0 | ? | a checar |
| 9 | 0x08057AD1 | 0x08057AD0 | ? | a checar |
| 10 | 0x08057B0D | 0x08057B0C | ? | a checar |
| 11 (0x0B) | 0x08057B6D | 0x08057B6C | Função desde o sync de 2026-10-14 (nome: FieldMenu_Sub0B_RowMachine) | destino do switch idx1 |
| 12 | 0x08057CB1 | 0x08057CB0 | Flow_WaitObjectThenCount (nome existente) | não usado pelo switch |
| 13 (0x0D) | 0x08057D05 | 0x08057D04 | a checar | destino do switch idx2 |
| 14 | 0x08057D15 | 0x08057D14 | ? | a checar |
| 15 (0x0F) | 0x08057D41 | 0x08057D40 | Função desde o sync de 2026-10-14 (nome: FieldMenu_Sub0F_Close) | destino do switch idx3..5 |

Correção: a entrada de sub 0x0D é 0x08057D04, não 0x08057CB0 (o decompile de 0x08057CB0 é do sub 12, que o switch não usa).
Ponteiros da tabela de switch 0x08057534 (32 bits): idx0 = 0x080575D4, idx1 = 0x0805754C, idx2 = 0x08057564, idx3..5 = 0x08057570.

## Passo 3: decodificação manual (sem função no Ghidra), helper fm3_thumb.py (fora do repo)

- Sub 0x0B (0x08057B6C, Thumb lido à mão, confirmado nos bytes): `push lr`; se `ctx+0x65` (linha) != 0 vai para 0x08057BC0;
  se linha == 0, faz switch sobre `ctx+0x70` (6 casos, tabela 0x08057B98 = 0x08057BB0, 0x08057C40, 0x08057C52, 0x08057C64,
  0x08057C76, 0x08057BB8). Linha 2 e linha 3 têm tabelas próprias (0x08057BD8 e 0x08057C1C). Cada caso chama
  `BL 0x08047BDC` (um passo de fila/texto, provável) e volta para 0x08057C48. **Confirmado** (bytes), **a conferir** o nome de 0x08047BDC.
  Resumo: o sub 0x0B é uma sub-máquina por linha; não é uma tela única.

## Passo 4: tecla B -> menu_list[1] (confirmado nos bytes)

- `menu_list` [1] = 0x080027BC é só um WRAPPER (sem corpo de tela): `push lr; ldr r1 = 0x083AC404; r0 = gamestate_subroutine*4 + r1;
  ldr r0 = tabela[sub]; BL 0x0807CCF0 (trampolim: chama r0); pop; bx`. Ou seja: MainState0 menu 1 = despacho por sub na tabela 0x083AC404.
- Logo: a tecla B (gMainState=0, menu=1, sub=0) cai em 0x083AC404[0]. Próximo passo: ler essa tabela e o handler de sub 0.

## Passo 5: sub 2 (0x080574D4, decodificado) — o switch usa r4-1 de Overworld_CheckStepAllowed

- Lê `keys_pressed` (0x0300058C) bit 0 (A). Se apertou A: linha = [ctx+0x65]; `r4 = Overworld_CheckStepAllowed(linha)`
  (0x08057D80). Se r4 == 0 sai (inativa). Se r4 != 0: zera [ctx+0x67], toca som 0x08047BA8(6), e faz `switch (r4 - 1)`
  (cmp 5 -> tabela 0x08057534). **Confirmado** (bytes). Consequência: o retorno de CheckStepAllowed é um código de ação
  (1..6), não só 0/1. A conferir no decompile de 0x08057D80 (abaixo).

## Passo 6: Overworld_CheckStepAllowed(k) 0x08057D80 (decompile, confirmado) — devolve CÓDIGO, não só 0/1

| k | regra (ordem das checagens) | códigos possíveis |
|---|---|---|
| 0 | item 0x12 == 0 -> 1; part 0x55 ausente -> 2; FUN_08048088(0)==0 -> 3; flag bit 2 setado -> 4; senão byte == 2 -> 5, senão 0 | 0..5 |
| 1 | item 0x18 == 0 -> 1; flag bit 0x10 -> 4; senão byte-1 < 2 -> 5, senão 0 | 0,1,4,5 |
| 2 | item 0x19 == 0 -> 1; part 0x0B ausente -> 2; FUN_08048088(1)==0 (ou byte == '\n') -> 3; flag bit 4 -> 4; senão byte-1 < 2 -> 5, senão 0 | 0..5 |
| 3 | item 0x26 == 0 -> 1; part 0x6B ausente -> 2; flag bit 8 -> 4; byte-1 < 2 senão 5; tile adjacente != 3 -> 6, senão 0 | 0,1,2,4,5,6 |
| outro | 0 | 0 |

- Desenho (InitChoiceMenuScreen): a faixa "ativa" é desenhada só quando o código é exatamente 1 (ver passo 1). Ou seja, pelo
  desenho, código 1 = linha com faixa ativa; pelo switch de sub 2, qualquer código != 0 segue e o índice é código-1.
  Isso é contraditório se 1 significar "indisponível" (a checagem de item 0 retorna 1). **Ambiguidade aberta**: ou a faixa
  de fundo ativa tem o sentido invertido, ou o código 1 é o "sem item -> sai". Não resolvi.
- Sub 2, switch por código-1 (tabela 0x08057534, confirmado): código 1 -> idx0 0x080575D4 (saída comum); código 2 -> idx1 (sub 0x0B);
  código 3 -> idx2 (sub 0x0D); códigos 4, 5, 6 -> idx3..5 (sub 0x0F).

## Passo 7: sub 0x0D e 0x0F (confirmado nos bytes) e a tabela de MainState0 menu 1

- Sub 0x0D = 0x08057D04 e sub 0x0F = 0x08057D40: **mesmo corpo**: `push lr; BL 0x08047C00; BL 0x0804A210; pop; bx`.
  Ou seja, ambos são de SAÍDA do menu (fecham a escolha e chamam o "fim de lista" 0x0804A210, já visto em FM2 e MN).
  A diferença entre os dois é só o que vem antes (o switch). 0x08047C00 não identificado (a conferir).
- Sub 0x0B = 0x08057B6C (passo 3): sub-máquina por linha; código 2 -> aqui.
- MainState0 menu 1 (passo 4): tabela **0x083AC404** = [0] 0x080027DC, [1] 0x080028AC, [2] 0x08002A2C.
  Sub 0 (0x080027DC, lido em bytes): carrega **6 conjuntos de gráficos** para a VRAM (0x06001800, 0x06001B00, 0x06001E00,
  0x06002100, 0x06002400, 0x06017400) via BL 0x080009BC, configura registradores de BG/OAM, usa o contexto de mensagens
  0x03000440 e chama 0x08074B88(6). **Confirmado** nos bytes. É uma tela cheia (não a faixa de 4 linhas do mapa).
  **Nome da tela: pendente** (precisa identificar os gráficos ou o texto que a tela mostra).

## Resumo dos 4 pontos

1. **Tecla B no mapa (0x08051204, confirmado)**: grava gMainState=0, gamestate_menu=1, sub=0, E+0xCBA=2. `menu_list[1]` = 0x080027BC é só
   um despachante por sub, para a tabela 0x083AC404. O sub 0 (0x080027DC) monta uma **tela cheia** (6 conjuntos de gráficos e
   chamada 0x08074B88). **Não é** a faixa de 4 linhas, que vem de MainState1 menu 2 (0x0805716C). Nome da tela: **pendente**.
   Hipótese que precisa de teste: a tela de B é diferente do menu de 4 linhas (FM/FM2 assumiam que era o mesmo).
2. **Tabela 0x083D5FF4 (16 entradas, passo 2)** e **switch 0x08057534 (passo 5)**: idx1 (código 2) -> sub 0x0B (máquina por linha, 0x08057B6C);
   idx2 (código 3) -> sub 0x0D (saída, 0x08057D04); idx3..5 (códigos 4, 5, 6) -> sub 0x0F (saída, 0x08057D40); idx0 (código 1) -> saída comum
   0x080575D4. As duas saídas chamam 0x08047C00 e 0x0804A210. **Confirmado** nos bytes. O que 0x08047C00 faz: **pendente**.
3. **Ordem das 4 linhas**: InitChoiceMenuScreen (passo 1) trata k = 0..3 em ordem, chama CheckStepAllowed(k) e desenha a faixa de cada linha
   com Map_DrawChoiceBackgroundRow(k). O **mapeamento k -> opção (MEDAMATCH/ITEM/VEÍCULO/SALVAR) continua pendente**. Pistas do código (não confirmadas):
   k=0 pede item 0x12 e parte 0x55; k=1 pede item 0x18; k=2 pede item 0x19 e parte 0x0B; k=3 pede item 0x26 e parte 0x6B e tile adjacente.
   **Ambiguidade**: o desenho só trata código 1 como faixa "ativa", mas o código 1 é o caso "item ausente". Não resolvi.
4. **Gravador de gamestate_menu=4 com gMainState=2 (bolsa)**: **pendente**. Não achei nas notas (KI.md, FM2.md, AB.md, LK2.md). O achado
   desta rodada: AB op 0x01 grava menu 8 (não 4). Para fechar: achar os literais 0x03002B8C no ROM (ldr de pool) e checar, em cada
   função, `mov rX,#4` seguido de str para 0x03002B8C com gMainState=2 no mesmo bloco. Não rodei (custo alto).

## Renomeações no Ghidra

Nenhuma. Os alvos confirmados (0x080027BC, 0x08057B6C, 0x08057D04, 0x08057D40, 0x08047C00) não têm função no Ghidra, e criar função
não foi feito. Nomes propostos estão em FM3_simbolos.csv. Overworld_CheckStepAllowed, Overworld_InitChoiceMenuScreen e
Flow_WaitObjectThenCount já tinham nome.

## Notas de método

- Thumb decodificado com um helper próprio (fm3_thumb.py, em scratchpad, fora do repo). Bug corrigido (extensão de sinal do BL).
- Pendente: nome da tela de B (0x083AC404 sub 0), função 0x08047C00, linha -> opção, gravador menu=4 (bolsa).
