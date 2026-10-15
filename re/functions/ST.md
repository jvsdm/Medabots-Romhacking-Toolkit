# GH-ST: tabelas de passos de cena (0x083ACE54, 0x083ACEF8, 0x083AFCA0, 0x083AFD08, 0x083D212C)

Leitura na ROM original (só leitura). Parcial: a estrutura foi achada; o despachante principal do bloco 0x083ACE14
foi lido em assembly (sem função no Ghidra); quem chama a cadeia até a tela (cena) ficou em aberto.

## 1. Estrutura (confirmado na ROM)

Cada tabela é um vetor de ponteiros Thumb (0x08xxxxxx | 1). Vazio = 0x08000841 (stub). O fim é a primeira palavra
que não é ponteiro.

- **Bloco ACE14..AD060** (uma tabela só, lida de 0x083ACE14): 148 palavras. Não são 4 tabelas independentes:
  - 0x083ACE54 = base + 0x40 (índice 16 do bloco). Fim do bloco 0x083AD064 = 0x30702058 (dado).
  - 0x083ACEF8 = base + 0xE4 (índice 57). Mesmo fim.
  - Os sites PRE/POST principais (0x08010815, 0x08010BD1) são entradas desse bloco (ACE54[0], ACEF8[0]).
- **Bloco AFC78..AFDF8** (96 palavras, fim 0x03020100 em 0x083AFDF8):
  - 0x083AFCA0 = índice 10 do bloco. 0x083AFD08 = índice 36. Mesmo fim.
  - Entradas fixas 0x0803D9A1 (PRE fixo), 0x0803DB4D (POST fixo) = AFCA0[0] e AFD08[0]. Confirmado por contiguidade.
  - Literal 0x083AFDF8 aparece em 0x0803D744 (aponta para o fim do bloco). Quem o usa: não lido.
- **0x083AECD8** (gFightSceneSteps, cena 8, índice 0x0300137C): bloco 0x083AEB1C..0x083AEE98 (ver DT.md; não relido).
- **0x083D212C** (gSceneSubroutineTable, mapa; 53 entradas 0804A2F1..0804B009, fim 0): leitor já conhecido em
  EV.md (0804A220 `EventScript_RunCurrent` e 0804A2E0 chamam 0x083D212C[0x0300137C]). Sem novidade.

## 2. Leitor do bloco ACE14 (confirmado no código, Thumb manual)

Função em **0x0801013C** (push {r4,lr}; função desde o sync de 2026-10-14 (nome: PartDetail_DispatchModeHandler); pool em 0x0801015C..0x08010164):

    r1 = 0x083ACE14
    r4 = 0x03002C30 + 0x129 = 0x03002D59
    r0 = table[ *(u8*)0x03002D59 ]        ; LSL 2
    BL 0x0807CCF1 (chamada via r0 = passo)
    depois: se byte [0x03002D59]... (ramo cmp #3 em 0x08010154)

- Índice = **byte 0x03002D59** (não 0x0300137C, não 0x03001800). Significado do endereço 0x03002D59: não identificado
  (fica dentro da área de gObjectSlots 0x03002C30, offset 0x129; provável estado do combate/cena).
- Chamador: função em **0x0800EB34** (push {lr}), que chama 0x0800EBFD, 0x0800F441 e depois 0x0801013D.
  O corpo de 0x0800EB34 e o contexto de tela não foram lidos.

## 3. Sites de texto (TX.md) — o que mudou

Os sites de texto pré/pós (0x08010814, 0x08010BD0, 0x0803D9A0, 0x0803DB4C) passaram a ser funções desde o sync de 2026-10-14 (Fight_ShowPreBattleTextForCharacter, Fight_ShowPostBattleTextForCharacter, Fight_ShowPreBattleTextIndex1, Fight_ShowPostBattleTextIndex1). Seu
endereço "+1" é entrada desses blocos, então são *passos* de cena (não despachados por BL). O que é tocado pelo
passo (texto da tela) continua o mesmo de TX.md.

## 4. Pendências

- Significado de 0x03002D59 e a cadeia 0x0800EB34 -> 0x0801013D -> tela (qual cena: intro/fim de batalha).
- Função 0x08010264 (usa a subtabela em 0x083ACE24 = bloco + 0x10, via literal em 0x080102B8): não decompilada.
- Quem lê o bloco AFC78..AFDF8 (literal 0x083AFC78 <- 0x0803D59C; 0x083AFC38 <- 0x0803C734) e 0x0803D744.
- Renomear no Ghidra: ver ST_simbolos.csv (só o bloco ACE14, nome genérico).

Símbolos: `ST_simbolos.csv`.
