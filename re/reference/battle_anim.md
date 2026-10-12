# Animação de batalha por molde (tarefa 7)

Metabee (E). Endereços de memória GBA. Fonte: decompilação no Ghidra (estática, sem emulador). O que está marcado
"provável" precisa ser conferido no visualizador Motion do menu de debug (seção 6).

## 1. Cadeia

```
tipo de movimento (0-46)  --gMoveScriptTable[tipo*7 + gMoveStyle[bot das pernas]]-->  roteiro (0-58)
roteiro  -> gMotionDurationTable[r]  duração de cada quadro-chave (u8, fim 0)   N = quantidade
         -> gMotionAngleTable[r]     s16 ângulo [linha][N]   (1/65536 de volta)
         -> gMotionOffsetTable[r]    (s8 x, s8 y) [linha][N]
```

| Símbolo | Endereço | Conteúdo |
|---|---|---|
| `gMoveScriptTable` | 0x0807E8EC | 47 x 7 u32 (tipo x estilo) -> nº do roteiro |
| `gMoveStyle` | 0x083CAD78 | u8 por bot (0-6), lido pelo bot das **pernas** |
| `gMotionAngleTable` | 0x083D1E40 | 59 ponteiros |
| `gMotionOffsetTable` | 0x083D1F2C | 59 ponteiros |
| `gMotionDurationTable` | 0x083D2018 | 59 ponteiros |
| dados | 0x080814C4-0x080ABE00 | roteiro 0 aponta os três para 0x080814C4 (vazio, N = 0) |

Linhas de dados: 49 (moldes 1-49), uma por molde, cada uma com N valores. Ângulos: linha `r` começa em
`ang + r*N*2`; deslocamentos: `off + r*N*2` (pares x,y).

## 2. Tocador

`MotionPlayStep(lado, tipo, bot_pernas)` 0x08044A3C, chamado uma vez por quadro:
1. roteiro = `gMoveScriptTable[tipo*7 + gMoveStyle[bot_pernas]]`; N = bytes até o 0 em `gMotionDurationTable[roteiro]` (máx. 255).
2. contador de quadros do lado += 1; se o quadro-chave atual k < N, chama
   `MotionInterpolateKeyframe(estado+lado*0x410+2, ang[r], off[r], dur[r], r, contador, k, 0x40, N)`.
3. se algum molde chegou ao alvo (retorno != 0): k += 1, contador = 0; quando k chega em N, zera k e devolve 1
   (roteiro terminou). Senão devolve 0.

`MotionInterpolateKeyframe` 0x08047428, para cada um dos 63 moldes (estado de 16 B por molde, `gBattlePartState`):
- `restante = dur[k] - contador`; se > 0:
  - ângulo (+0x1C, s16): `alvo - atual`, escolhendo o menor caminho entre `d`, `d - 0x10000` e `d + 0x10000`;
    `atual += d / restante`;
  - posição 8.8 (+0x18 = x, +0x1A = y): `atual += (alvo*256 - atual) / restante`.
- se `restante <= 0`: grava o alvo direto (ângulo e `x<<8`, `y<<8`) e conta o molde como "chegou".
- linha seguinte: ângulo `+N` (s16), deslocamento `+2N` bytes, estado `+0x10`.

Ou seja: a duração é **quantos quadros leva para chegar ao quadro-chave**, com interpolação linear, ângulo pelo
caminho mais curto. O deslocamento é relativo à âncora do molde (ver `large_part_layout.md`).

## 3. Quem escolhe o tipo

- Entrada na batalha: `FightPreview_BeginSideEntranceSequence` 0x08039E00 grava 0x29 (tipo 41 -> roteiro 35,
  2 quadros-chave, 28 quadros; mexe só os moldes 7-36, sem a raiz) em `fight_information+0x112` (lado 0) e toca.
- A cada passo, `FightPreview_AdvanceLeftSideSequenceStep` 0x080361C4 / `...Right...` 0x08036218 tocam o tipo
  guardado; bit 7 do byte = terminou.
- `FightPreview_ApplyObjectMotionKind` (0x0803BBE0, nome novo): copia o tipo do **slot de objeto** atual
  (`gObjectSlots` 0x03002C30, 0x40 B por slot, índice em 0x0300056F) campo `+0x2C` para o byte do lado, depois de
  esperar `+0x30` (s16) quadros. Quem preenche `+0x2C` nos slots de batalha ainda não foi achado (provável: os
  handlers de família de ação, que usam `gObjectSlots`, e o opcode 0x24 `RunPreviewSequenceUntilComplete` do
  roteiro de ação).
- `ObjectSlot_TickRepeat` (0x0803CB54, nome novo): conta `+0x30`, depois `+0x2C`; com `+0x2E` > 0 recarrega `+0x2C`
  de `+0x2D` e chama 0x08074DE0 (`+0x2B`) — repetição genérica de animação de slot.
- Telas fora da batalha: tipo 44 (roteiro 57, 189 quadros, moldes 1/2/6/10/14-24) em 0x0806E9E0, 0x08072234 e
  0x080722B0 (cenas com o bot 0x2B); provável demo/título.

## 4. Tipos (gMoveScriptTable)

Os tipos 0-12 têm duas variantes por estilo (estilos 0,1,3,4 usam a primeira e 2,5,6 a segunda; no tipo 12 o estilo 1 também usa a segunda); a diferença
está nas pernas que mexem (variante 1: moldes 20-29; variante 2: 30 e 41-49). Os demais tipos ignoram o estilo.

| tipo | roteiro por estilo 0-6 | quadros-chave | quadros |
|---|---|---|---|
| 0 (0x00) | 38 38 39 38 38 39 39 | 24 / 17 | 86 / 86 |
| 1 (0x01) | 42 42 43 42 42 43 43 | 24 / 17 | 86 / 86 |
| 2 (0x02) | 40 40 41 40 40 41 41 | 24 / 17 | 87 / 86 |
| 3 (0x03) | 7 7 8 7 7 8 8 | 3 / 3 | 14 / 14 |
| 4 (0x04) | 11 11 12 11 11 12 12 | 3 / 3 | 14 / 14 |
| 5 (0x05) | 9 9 10 9 9 10 10 | 3 / 3 | 14 / 14 |
| 6 (0x06) | 13 13 14 13 13 14 14 | 4 / 4 | 28 / 28 |
| 7 (0x07) | 17 17 18 17 17 18 18 | 4 / 4 | 28 / 28 |
| 8 (0x08) | 15 15 16 15 15 16 16 | 4 / 4 | 28 / 28 |
| 9 (0x09) | 22 22 23 22 22 23 23 | 23 / 22 | 120 / 127 |
| 10 (0x0A) | 26 26 27 26 26 27 27 | 23 / 22 | 120 / 127 |
| 11 (0x0B) | 24 24 25 24 24 25 25 | 23 / 22 | 120 / 127 |
| 12 (0x0C) | 20 21 21 20 20 21 21 | 14 / 14 | 156 / 156 |
| 13 (0x0D) | 44 | 13 | 73 |
| 14 (0x0E) | 46 | 13 | 73 |
| 15 (0x0F) | 45 | 13 | 73 |
| 16 (0x10) | 31 | 19 | 88 |
| 17 (0x11) | 33 | 19 | 88 |
| 18 (0x12) | 32 | 19 | 88 |
| 19 (0x13) | 1 | 39 | 136 |
| 20 (0x14) | 3 | 39 | 136 |
| 21 (0x15) | 2 | 39 | 136 |
| 22 (0x16) | 28 | 13 | 73 |
| 23 (0x17) | 30 | 13 | 73 |
| 24 (0x18) | 29 | 13 | 73 |
| 25 (0x19) | 4 | 28 | 122 |
| 26 (0x1A) | 6 | 28 | 122 |
| 27 (0x1B) | 5 | 28 | 122 |
| 28 (0x1C) | 36 | 30 | 166 |
| 29 (0x1D) | 36 | 30 | 166 |
| 30 (0x1E) | 36 | 30 | 166 |
| 31 (0x1F) | 19 | 24 | 140 |
| 32 (0x20) | 52 | 11 | 78 |
| 33 (0x21) | 47 | 9 | 89 |
| 34 (0x22) | 53 | 13 | 111 |
| 35 (0x23) | 48 | 7 | 75 |
| 36 (0x24) | 54 | 17 | 94 |
| 37 (0x25) | 49 | 14 | 96 |
| 38 (0x26) | 55 | 2 | 4 |
| 39 (0x27) | 50 | 2 | 4 |
| 40 (0x28) | 51 | 2 | 48 |
| 41 (0x29) | 35 | 2 | 28 |
| 42 (0x2A) | 34 | 9 | 98 |
| 43 (0x2B) | 56 | 14 | 105 |
| 44 (0x2C) | 57 | 33 | 189 |
| 45 (0x2D) | 58 | 23 | 189 |
| 46 (0x2E) | 37 | 8 | 78 |

## 5. Roteiros (o que os dados mostram)

| Roteiros | Quadros-chave / quadros | Moldes que mexem | Leitura (provável) |
|---|---|---|---|
| 1-3 | 39 / 136 (segura 30 no 7º) | raiz, cabeça, braços, pernas 20-36 | ataque longo com pausa |
| 4-6 | 28 / 122 | idem | ataque |
| 7-12 | 3 / 14 (2,4,8) | corpo + um dos grupos de pernas | reação curta (golpe recebido) |
| 13-18 | 4 / 28 (12,4,8,4) | idem | reação/esquiva |
| 19 | 24 / 140 | tudo | ataque especial |
| 20-21 | 14 / 156 | tudo, pernas por variante | ataque longo |
| 22-27 | 22-23 / 120-127 | tudo, por variante | andar/avançar |
| 28-30, 44-46 | 13 / 73 | tudo | ataque (golpe) |
| 31-33 | 19 / 88 | tudo | ataque com tremor (1-4 alternado) |
| 34 | 9 / 98 | tudo | — |
| 35 | 2 / 28 | 7-36, sem a raiz | entrada (tipo 41) |
| 36 | 30 / 166 | tudo | — |
| 37 | 8 / 78 | tudo | — |
| 38-43 | 17-24 / 86 | por variante | andar (tipos 0-2) |
| 47-49, 52-54, 56 | 7-17 / 75-111 | tudo | ações de Medaforce/especiais |
| 50, 55 | 2 / 4 | quase tudo | pose instantânea |
| 51 | 2 / 48 | sem a raiz, variante 2 | — |
| 57, 58 | 33 e 23 / 189 | poucos moldes | cenas fora da batalha (tipos 44, 45) |

## 6. Rotulos dos tipos (A1, Motion no emulador)

Capturas feitas para os 47 tipos (0-46), 5 quadros por tipo; detalhes e metodo em `pc/research/A1_motion_kinds.md`.
Confianca: alta = confirmada na tela; media = pela duracao/roteiro; baixa = palpite visual. "?" = sem rotulo.

| tipo | roteiro (estilo 0) | rotulo provavel | confianca |
|---|---|---|---|
| 0-2 | 38-42 | andar / avancar (secao 4) | baixa |
| 3-5, 6-8 | 7-18 | reacao curta ao golpe (corpo + pernas) | baixa |
| 9-11 | 22-26 | andar / avancar (roteiros 22-27) | baixa |
| 12 | 20 | ataque longo | baixa |
| 13-18 | 31-33, 44-46 | golpe / tremor | baixa |
| 19 | 1 | ataque especial | baixa |
| 20-21 | 2-3 | ataque longo | baixa |
| 22-24 | 28-30 | golpe | baixa |
| 25-27 | 4-6 | ataque | baixa |
| 28-30 | 36 | ? (movimento amplo) | baixa |
| 31 | 19 | ? (movimento amplo) | baixa |
| 32-35 | 47-52 | ? (Medaforce / especiais) | sem rotulo |
| 35 | 48 | ? | sem rotulo |
| 36 | 54 | ? | sem rotulo |
| 37 | 49 | ? | sem rotulo |
| 38-39 | 55, 50 | pose instantanea (2 quadros-chave) | media |
| 40 | 51 | ? | sem rotulo |
| 41 | 35 | entrada (FightPreview, roteiro 35, 28 quadros) | media |
| 42 | 34 | ? | sem rotulo |
| 43 | 56 | ? | sem rotulo |
| 44 | 57 | cena fora da batalha (189 quadros; demo/titulo) | baixa |
| 45 | 58 | cena fora da batalha (189 quadros) | baixa |
| 46 | 37 | ? | sem rotulo |

Nota A2 (parcial, ver pc/research/A2_motion_events.md): nenhum rotulo confirmado pelo jogo; `FightPreview_ApplyObjectMotionKind` nao roda na batalha rec_2, entao a ligacao acao -> tipo ainda nao foi achada. Os rotulos desta secao seguem como palpite.

Nota A3 (rec_2, ver pc/research/A2_motion_events.md, secao A3): o roteiro de objeto e `ObjectScript_OpcodeDispatch` 0x080393EC (le byte de `ObjectScript_ReadWord` 0x0803D424, grava em gObjectSlots+0x29, chama `0x083AF8C0[opcode]`). Valores medidos: +0x2C parado em slot 1 = 13 (f1460-1840) e slot 2 = 32 (f1560 em diante); +0x2C que cai por quadro = contador. Quem grava o tipo em +0x2C ainda nao foi achado. Rotulos desta secao seguem como palpite.

Nota: o rotulo "retirada", "parte quebrada", "funcao cessada", "vitoria" e "hit/flinch" nao puderam ser
atribuidos com os dados atuais. Ver A1_motion_kinds.md para o que falta.

### Passos de conferencia (feitos parcialmente)

O visualizador **Motion** do menu de debug (`DebugMotion_Main` 0x0800D710) monta dois bots e toca o tipo
`[0x03000475 + byte 0x03000474]` (`DebugMotion_PlayKind` 0x0800D90C chama `MotionPlayStep` dos dois lados com
ele). Direita/esquerda mudam o tipo; START/SELECT, o bot (0-121); A avança.
1. Para cada tipo 0-46, anotar o que o bot faz e preencher a coluna "Leitura" das seções 4 e 5.
2. Testar o mesmo tipo com bots de estilos diferentes (`gMoveStyle` 0 e 2) para confirmar as variantes.
3. Em batalha, pôr um watchpoint de escrita em `fight_information+0x112` e em `0x03002C30 + slot*0x40 + 0x2C`
   para achar quem escolhe o tipo de cada ação (falta para ligar ação -> animação).

Nota A4 (rec_2, ver pc/research/A2_motion_events.md, secao A4): o +0x2C do slot 1 = 13 (f1446) e escrito pelo handler de opcode 8 do roteiro
de objeto, `0x080395E8` (tabela 0x083AF8C0[8]): valor = halfword de `fight_information+0x104+2*k`. Ainda sem a origem de k/da tabela e sem rotulo do tipo 13.

Nota A5 (rec_2, ver pc/research/A2_motion_events.md, secao A5 e pc/research/A5_preview_rows.tsv): a tabela +0x104 vem da linha
da ROM `0x083CE164[sel]` (FightPreview_UnpackSequenceEntry 0x0803D450), com sel = action_or_leg_type da peca k (ate f1445
sel=6 = peca 0). Logo, o tipo de movimento de um slot = tabela ROM indexada por tecnica da peca + cursor + k. Cadeia fechada
ate a tabela; falta o avanco do cursor (+0x100), o ramo +0xDD>3 e o nome de cada linha. Rotulos da tabela desta secao: ainda
sem confirmacao no jogo.
