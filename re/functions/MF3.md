# MF3: Medaforce por id (0x00..0x33), tabela final (GH-MF3, 2026-10-14)

Leitura estática: ROM original (só leitura, `python -I`, script em scratchpad, apagado) + os achados de MF, MF2, MD2, MD3,
MG3, QA4, TA2 §6 e AS3c. Sem emulador, sem commit. Nenhum texto do jogo copiado (só ids, opcodes e endereços).

## Como ler

- **id** = byte 0 do descritor `desc6` 0x083B7E54 (id*6), igual a B+0xA8 e ao índice de `gTechniqueActionScriptPtrTable`
  0x083C6CA8[0x50+id] (QA4).
- **roteiro**: ops do segmento `[ptr[0x50+id], ptr[0x51+id])`, lidos da ROM; termina em `01`. Ids 0x00..0x33 (52).
  O id 0x33 usa o ponteiro 0x083C6C99 (o ponteiro 0x84 não é válido), fim em 0x01 (conferido).
- **despachante / efeito**: nome do opcode em AS3c. C = confirmado no código/decompile, P = provável pelo nome do passo.
- **descritor b0..b5**: b0 = id. Ver §Descritor abaixo para o significado.
- **medalhas**: `medal_medaforce_list` 0x083B7F8C[medalha*3 + slot], 30 medalhas. Formato `medalha:nível`.
- **nível de liberação**: slot 0 = 10, slot 1 = 30, slot 2 = 60 (corte fixo em `Fight_TrySelectMedaforceAction` 0x080155A8,
  MF §2 e §7). O corte vale para a MEDALHA: o mesmo id pode liberar em 10 numa medalha e em 60 noutra.

Legenda dos opcodes do roteiro (AS3c):
- 41 alvo aleatório (inline) · 42 rolagem: F+0xEC = (F+0xE8>>3) + b4 (C, MD2) · 43 soma campo +4 do lado (P)
- 44 AP: aplicação de peça/HP (P) · 45 contador de uso (C) · 46 soma armadura máx−atual do bot alvo (P)
- 47 copia alvo + timer de lado (P) · 48 AQ sprites/troca de HP (P) · 49 metade da armadura do bot ativo → F+0xEC (C)
- 4A acumula 10 por peça vazia dos aliados em fi+0x8C (P, destino não identificado) · 4B acha bot com a peça (P)
- 4C F+0xEC = armadura da peça alvo, SOBRESCREVE a rolagem (C) · 4D F+0xEC = (d1+d2)*10, SOBRESCREVE a rolagem (C)
- 4E/4F busca de bot ativo com grupo (P) · **50 dano em peça aleatória dos adversários (C, HN2, AR_00 usa F+0xEC)**
- 51 estado no próprio bot (C, HN2) · 52 bônus de suporte do time (C) · 53 marca de reação no próprio bot (C)
- 54 estado em todos os adversários (C) · 55 proteção contra status do time (P) · 56 cura do time (P)
- 57 reparo de armadura do time (P) · 58 restaura peças do time (P) · 59 gauge (BA, P)
- 5A técnica sem dano: escala, marca, animação (BB, P) · 5B/5C soma de valor (BC/BD_AddV, P) · 5D sorteio de técnica (BE, P)
- 5E queda/flutua/copia/aplica (BF, P) · 5F flash/movimento/transição (BG, P) · 60 spawn/boost (BH, P)
- 61 carga de boost (BI, P) · 62 levanta/desprende (BJ, P) · 63 BK, P · 64 efeito de tela/borda (BL, P)
- 65 BM: inclui BM_0F, que zera o medidor do adversário e converte em dano (C) · 66 BN, P · 67 BO, P · 68 BP, P · 69 BQ, P
- 6A BR: família da máquina de ação da medaforça; passos BR_00..08 e BR_10..14 (C, wrapper)
- 40 AO: sprites laterais + passos H_* (troca/cura de barra) + quebra de cabeça (P)

## Tabela final

| id | roteiro (ops) | despachante principal → efeito | b0..b5 | medalha:nível (liberação) |
|----|---------------|--------------------------------|--------|---------------------------|
| 00 | 06 08 1A 1B 1C 40 20 45 21 | 40 AO, sem rolagem: troca/cura de barra (P) | 00 01 50 00 14 01 | m0:10, m28:10 |
| 01 | 42 50 45 21 01 | 42 + **50 AR**: dano em peça aleatória do adversário (C) | 01 00 3C 00 1E 00 | m0:30, m5:60 |
| 02 | 41 42 43 08 1A 1B 1C 1D 44 20 45 21 | 41 alvo + 42 + 43 + 44 AP: dano em peça-alvo (P) | 02 03 50 00 0A 01 | m1:10, m29:10 |
| 03 | 41 42 46 08 1A 1B 1C 1D 44 20 45 21 | 41 + 42 + 46 (armadura perdida do alvo) + 44: dano em peça-alvo (P) | 03 02 3C 00 0A 01 | m1:30, m18:60 |
| 04 | 51 45 01 | 51 AS: estado no próprio bot, sem rolagem (C) | 04 02 28 00 00 03 | m2:10 |
| 05 | 51 45 01 | 51 AS: estado no próprio bot, sem rolagem (C) | 05 03 46 00 00 03 | m2:30, m23:60 |
| 06 | 51 45 01 | 51 AS (C) | 06 02 28 00 00 03 | m3:10 |
| 07 | 51 45 01 | 51 AS (C) | 07 03 46 00 00 03 | m3:30, m14:60 |
| 08 | 51 45 01 | 51 AS (C) | 08 02 28 00 00 03 | m4:10 |
| 09 | 51 45 01 | 51 AS (C) | 09 03 46 00 00 03 | m4:30, m13:60 |
| 0A | 42 52 45 01 | 42 + 52 AT: bônus de suporte do time (C) | 0A 04 3C 1E 1E 00 | m5:10, m26:30 |
| 0B | 53 45 01 | 53 AU: marca de reação no próprio bot, sem rolagem (C) | 0B 04 1E 00 00 03 | m1:60, m5:30 |
| 0C | 42 54 45 01 | 42 + 54 AV: estado em todos os adversários (C) | 0C 00 32 10 28 00 | m6:10 |
| 0D | 55 45 01 | 55 AW: proteção contra status do time, sem rolagem (P) | 0D 01 3C 00 00 02 | m6:30, m25:60 |
| 0E | 42 54 45 01 | 42 + 54 AV (C) | 0E 00 32 08 08 00 | m7:10, m28:60 |
| 0F | 55 45 01 | 55 AW (P) | 0F 01 3C 00 00 02 | m7:30, m12:60 |
| 10 | 42 54 45 01 | 42 + 54 AV (C) | 10 00 32 10 0A 00 | m8:10 |
| 11 | 55 45 01 | 55 AW (P) | 11 01 3C 00 00 02 | m8:30, m19:60 |
| 12 | 42 54 45 01 | 42 + 54 AV (C) | 12 00 32 10 08 00 | m9:10 |
| 13 | 55 45 01 | 55 AW (P) | 13 01 3C 00 00 02 | m9:30, m20:60 |
| 14 | 41 42 08 1A 1B 1C 1D 44 47 1B 1C 48 20 45 21 | 41 + 42 + 44 AP + 47 + 48: dano em peça-alvo com segunda passada (P) | 14 05 46 00 3C 01 | m10:10, m29:60 |
| 15 | 56 45 01 | 56 AX: cura do time, sem rolagem (P) | 15 05 28 00 00 02 | m2:60, m10:30 |
| 16 | 42 59 45 01 | 42 + 59 BA: gauge do time (P) | 16 07 28 00 02 02 | m11:10 |
| 17 | 5A 45 01 | 5A BB: técnica sem dano (escala/marca/anim), sem rolagem (P) | 17 07 32 00 00 00 | m11:30, m21:60 |
| 18 | 42 5B 45 01 | 42 + 5B BC (soma de valor, P): alvo não identificado | 18 04 28 00 0A 03 | m12:10 |
| 19 | 42 5C 45 01 | 42 + 5C BD (soma de valor, P) | 19 04 28 00 0A 03 | m12:30, m22:60 |
| 1A | 42 57 45 01 | 42 + 57 AY: reparo de armadura do time (P) | 1A 05 3C 00 3C 02 | m13:10, m26:10 |
| 1B | 41 42 08 49 1A 1B 1C 1D 44 20 45 21 | 41 + 42 + **49** (metade da armadura do próprio bot → F+0xEC, C) + 44: dano em peça-alvo (P) | 1B 05 50 00 00 01 | m11:60, m13:30 |
| 1C | 42 58 45 01 | 42 + 58 AZ: restaura peças do time (P) | 1C 05 46 00 28 02 | m14:10, m27:10 |
| 1D | 41 42 08 4A 1A 1B 1C 1D 44 20 45 21 | 41 + 42 + 4A (acúmulo fi+0x8C, P) + 44: dano em peça-alvo (P) | 1D 05 32 00 14 01 | m6:60, m14:30 |
| 1E | 42 5D 45 01 | 42 + 5D BE: sorteio de técnica (P) | 1E 02 32 10 0A 00 | m15:10 |
| 1F | 42 5E 45 01 | 42 + 5E BF (P) | 1F 03 28 00 1E 02 | m7:60, m15:30 |
| 20 | 42 5D 45 01 | 42 + 5D BE (P) | 20 02 32 10 0A 00 | m16:10 |
| 21 | 42 5E 45 01 | 42 + 5E BF (P) | 21 03 28 00 1E 02 | m16:30, m24:60 |
| 22 | 42 5F 45 01 | 42 + 5F BG: flash/movimento/transição (P) | 22 06 1E 00 1E 02 | m17:10 |
| 23 | 42 5E 45 01 | 42 + 5E BF (P) | 23 06 28 00 1E 02 | m0:60, m17:30 |
| 24 | 42 60 45 01 | 42 + 60 BH: spawn/impulso (P) | 24 06 1E 00 1E 02 | m18:10 |
| 25 | 42 5E 45 01 | 42 + 5E BF (P) | 25 06 28 00 1E 02 | m3:60, m18:30 |
| 26 | 42 61 45 01 | 42 + 61 BI: carga de boost (P) | 26 06 28 10 0A 02 | m19:10 |
| 27 | 42 62 21 45 01 | 42 + 62 BJ (P) | 27 06 3C 00 02 00 | m8:60, m19:30 |
| 28 | 42 54 45 01 | 42 + 54 AV: estado em adversários (C) | 28 07 32 10 0A 00 | m20:10, m29:30 |
| 29 | 42 63 45 01 | 42 + 63 BK (P) | 29 07 32 10 0A 00 | m10:60, m20:30 |
| 2A | 42 64 21 45 01 | 42 + 64 BL: efeito de tela/borda (P) | 2A 01 46 00 01 00 | m21:10, m27:30 |
| 2B | 4B 42 08 4C 1A 1B 1C 1D 44 20 45 21 | 4B + 42 + **4C** (F+0xEC = armadura da peça-alvo, sobrescreve a rolagem, C) + 44: dano em peça-alvo (C no 4C) | 2B 03 46 00 00 01 | m17:60, m21:30 |
| 2C | 42 65 21 45 01 | 42 + 65 BM (inclui BM_0F_D, que drena medaforça do adversário, C); passo deste id não lido (P) | 2C 07 3C 00 00 00 | m22:10, m28:30 |
| 2D | 66 45 01 | 66 BN, **sem rolagem** (P) | 2D 07 32 00 00 02 | m16:60, m22:30 |
| 2E | 42 67 45 01 | 42 + 67 BO (P) | 2E 04 28 00 14 03 | m23:10 |
| 2F | 68 45 01 | 68 BP, **sem rolagem**: AR_Show/Anim/Spaw + BP_04/08 (P) | 2F 04 46 00 00 00 | m4:60, m23:30 |
| 30 | 69 45 01 | 69 BQ, **sem rolagem** (P) | 30 04 32 00 00 03 | m24:10, m27:60 |
| 31 | 6A 45 01 | 6A BR, **sem rolagem**: família de controle da ação (C wrapper, efeito P) | 31 04 46 00 00 03 | m9:60, m24:30 |
| 32 | 41 42 08 4D 1A 1B 1C 1D 44 20 45 21 | 41 + 42 + **4D** (F+0xEC = (d1+d2)*10, sobrescreve a rolagem, C) + 44: dano em peça-alvo (P) | 32 07 3C 00 0A 01 | m25:10, m26:60 |
| 33 | 4E 42 08 4F 1A 1B 1C 1D 44 20 45 21 | 4E + 42 + 4F (acha bot ativo com grupo, P) + 44: dano em peça do bot escolhido (P) | 33 07 46 00 50 01 | m15:60, m25:30 |

## Descritor (b0..b5 → B+0xA8..B+0xAD)

- **b0 = id** (B+0xA8): confirmado, igual ao id do roteiro.
- **b1 = índice de especialidade 0..7** (B+0xA9): confirmado (MD2). Escolhe o nível em `B[0x20 + b1]` na rolagem.
  Nomes das 8 especialidades: 0 strike, 1 berserk, 2 shoot, 3 aim_shot, 4 defend, 5 heal, 6 support, 7 interrupt (MD2).
- **b2 = B+0xAA**: único leitor = BR_45 (`fi+0x51 = b2`, MD3, C). Hipótese forte (P): custo/quantidade do medidor
  consumido, porque fi+0x51 é o contador que BR_46 desce do medidor (MG3). Atenção: BR_26 também grava fi+0x51 (0x28 ou
  0x50), ainda não resolvido qual vale para a medaforça. Não é o contador de passo da luta (QA4/MD2 corrigiram isso).
- **b3 = base da rolagem** (B+0xAB): confirmado (MD2). Só ids COM 0x42 usam b3 e b4.
- **b4 = bônus fixo de efeito** (B+0xAC): confirmado (MD2, `F+0xEC = (F+0xE8>>3) + b4`, mínimo 1).
- **b5 = B+0xAD**: sem leitor achado (MD3). Valores 0..3. Não usar como classe/alcance até achar o leitor.

Ids SEM 0x42 (não usam b3/b4 nem a rolagem): 00, 04..09, 0B, 0D, 0F, 11, 13, 15, 17, 2D, 2F, 30, 31.

## Liberação e medidor (resumo)

- Gate de escolha: B+0xC1 == 0x50 (medidor cheio), depois o nível do bot (B+5) contra o slot (10/30/60). MF §2.
- Liberação do slot: quando a PEÇA de nível 10/30/60 sobe (MF §7). Mesma tabela 10/30/60 por medalha.
- Consumo do medidor: BR_46 desce B+0xC1 com fi+0x51 (MG3). Ver b2.

## Correções a outros arquivos (feitas ou pendentes)

- MF2 §6: corrigido para `F+0xA8 = B+0xA8 + 0x50` (QA4). Feito.
- MF2 §2: algumas linhas estavam erradas contra a ROM. Valem as desta tabela:
  - id 01 = `42 50 45 21 01` (MF2 não tinha 0x21).
  - ids 2D, 2F, 30, 31 = SEM 0x42 (MF2 dizia `42 + 5d..6a`).
  - id 2D = `66 45 01`; 2F = `68 45 01`; 30 = `69 45 01`; 31 = `6A 45 01`.
  - id 33 = `4E 42 08 4F ...` (ponteiro 0x083C6C99 conferido na ROM, fim em 0x01).
- TA2 §6: as linhas 4..40 são de técnica de peça; o que se lê de B+0xA8 são ids de Medaforce (QA4). Esta tabela é a correta.
- Sobrescrita de F+0xEC: ids 2B (4C) e 32 (4D) apagam a rolagem do 42 do próprio roteiro. Ao dar valor de dano a esses
  ids, usar o valor do 4C/4D, não o do 42.

## Pendências

1. Efeito exato de cada op P (43, 46, 47, 48, 4A, 4B, 4E/4F, 55..6A) e do passo 2C (BM).
2. Significado de b5 (B+0xAD): sem leitor.
3. Confirmar b2 como custo: ler BR_26 vs BR_45 na ordem da medaforça (fi+0x51).
4. Confirmar em emulador (proibido nesta tarefa): consumo do medidor, efeito por id.
