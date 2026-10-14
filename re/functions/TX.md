# Textos pré/pós-batalha e tabela de dados após gActionScriptOpcodeHandlerTable (GH-TX)

Continuação de `DT2.md` (pendências). Leitura na ROM original `Medabots - Metabee (Europe).gba` (só leitura, python -I).
Convenção: `ROM = 0x08000000 + offset`. Ghidra: funções sem corpo foram lidas por decodificação thumb manual
(sem capstone); os sites ainda não viraram função no Ghidra (listados em `TX_lista.txt`).

## 1. Como o índice da mensagem pré/pós-batalha é escolhido

Os quatro sites têm o mesmo padrão: `r0 = StateMachine_DispatchStep(tabela[índice])`.

| Site (corpo) | Tabela | Índice | Chama |
|---|---|---|---|
| 0x08010814 (PRE, principal) | gTextPreBattleMessage 0x083C2D3C | `byte [0x03001800]` | 0x0803FB3C |
| 0x08010BD0 (POST, principal) | gTextPostBattleMessage 0x083C3B04 | `byte [0x03001800]` | 0x0803FB3C |
| 0x0803D9A0 (PRE, fixo) | gTextPreBattleMessage | constante 1 (`ldr [r0,#4]`) | 0x0803FB3C |
| 0x0803DB4C (POST, fixo) | gTextPostBattleMessage | constante 1 (`ldr [r0,#4]`) | 0x0803FB3C |

- O índice de 0x03001800 é `fight_information +0x00`, que é `FightTemplate.character_id` (campo `+0x0`, tipo byte,
  em `re/types/medabots_simon_types.tsv`). Ou seja: **o índice é o personagem (oponente/líder), não o bot, a medalha
  ou o id do robattle**. Reforço: `Fight_SceneStep44` (HN10, 0x080310A0) usa o mesmo byte como índice de
  `gTextCharacterNames`. Confiança: provável (o nome "character_id" vem do types de Simon; o uso como índice da
  tabela de 96 entradas está confirmado no código).
- Após a chamada, o sítio principal faz: se `r0 != 0`, chama `Fight_UpdateBotPartEntryMotion` (0x08012098) e
  incrementa o contador em 0x0300137C; depois chama de novo `0x08012098`. Ordem exata do contador: provável.
  Os sites fixos (índice 1) não têm esse ramo.
- Os sites ficam em funções que o Ghidra não tem (`get_function_by_address` vazio). A nota do DT2 de que o corpo de
  `Fight_InitBotPartSelectScreen` (0x0801058C–0x080106E9) cobre 0x0801081A está **errada**: o corpo termina antes.
  A função correta começa em 0x08010814 (push {lr}), termina em 0x0801084F.
- Quem chama os sites principais: não é por `BL` direto. Os endereços 0x08010815, 0x08010BD1, 0x0803D9A1 e 0x0803DB4D
  aparecem como ponteiros em tabelas de passos: 0x083ACE54 (PRE principal), 0x083ACEF8 (POST principal),
  0x083AFCA0 (PRE fixo), 0x083AFD08 (POST fixo). Essas tabelas não têm `LDR` de literal nem xref no Ghidra; o
  despachante delas fica em aberto.

## 2. Tabela de bytes em 0x083AF380 (após gActionScriptOpcodeHandlerTable)

- Leitor único: `Fight_RollDamageFromTwoDice` 0x08034AA0 (sem função no Ghidra; literal 0x08034AEC em 0x08034AA8).
  Endereço 0x08034AA0: `push {r4-r7,lr}`, termina em `pop {r0}; bx r0` em 0x08034AE4.
- Algoritmo lido no corpo:
  1. `r = randbyte()` (Ghidra: `randbyte` 0x080424B8); `v1 = tabela[r & 15]`; grava `ctx[0x188] = v1`.
  2. `r = randbyte()`; `v2 = tabela[r & 15]`; grava `ctx[0x189] = v2`.
  3. `ctx+0xEC` (halfword) = `(v1 + v2) * 10`. (ctx = 0x03001800. Pela pesquisa (G1_damage), ctx+0xEC é o dano:
     **provável**.) Ou seja: é a soma de duas "rolagens" de dado, vezes 10.
- Formato: bytes sem sinal. Só os **16 primeiros** (0x083AF380..0x083AF38F) são lidos, com índice `& 15`.
  Valores 1..6 em ciclo (`i % 6 + 1`): um dado de seis faces com distribuição desigual de 16 entradas.
- Os bytes seguintes (0x083AF390..0x083AF48F, valores 1..0x20 em blocos de 8) e depois (0x083AF490..) com valores
  com sinal (-4..+4, em pares e em halfwords) **não são lidos por esse leitor**. Não identificado: provavelmente
  outras tabelas (sem leitor achado: só 1 literal aponta para 0x083AF380).
- Renomeado no Ghidra: 0x083AF380 → `gRollDieFaceTable` (rename_data; resultado "attempted", não reconferido).
- Confiança: confirmado (leitor, índice `& 15`, soma ×10 no corpo); "dano" em ctx+0xEC: provável.

## 3. Pendências

- Criar no Ghidra as funções de `TX_lista.txt` (5 endereços; sem função: 0x08010814, 0x08010BD0, 0x0803D9A0,
  0x0803DB4C, 0x08034AA0). Depois repetir o `decompile` e confirmar os nomes.
- Quem chama `Fight_RollDamageFromTwoDice` (despachante das tabelas 0x083ACE54 e vizinhas): não localizado.
- Confirmar em runtime (mGBA, sem alterar a ROM) o valor de `0x03001800` no início de uma batalha: não feito
  (fora do escopo; sem emulador nesta tarefa).

Símbolos: `TX_simbolos.csv`.
