# Saltos indiretos não recuperados pelo Ghidra (GH-JT, 2026-10-13)

Itens da tarefa 3 de `HANDOFF_DISCOVERY.md`. Ghidra marcou "UNRECOVERED_JUMPTABLE" em 6 pontos. Resultado:
**só um é salto por tabela de verdade (J:0x02). Os outros 4 + o fallback de J são RETORNO normal da função**
(`pop {rN}; pop {r0}; bx r0`, o LR empilhado no início). O Ghidra leu o `bx r0` como salto indireto.
Sem tabela, sem alvo a descobrir: o "próximo passo" de cada um é o +0xAA que ele grava antes de retornar.

Base comum: fight_information `0x03001800` (+0xAA = passo do despachante). Byte de seletor compartilhado por
J_02 e G02: `0x03000440 + 0x139` = **0x03000579** (ROM: literal `0x03000440`, offset `0x139`).

## Resumo

| Handler | Endereço do salto (Ghidra) | Tipo | Índice / seletor | Próximo +0xAA |
|---|---|---|---|---|
| E/05 (E_05_SlideBotIntoPlace) | 0x0801DADE | retorno (`bx r0`) | n/a | 0x05 -> **0x06** (E06), só ao chegar |
| G/02 (G_02_LoadBgTilesStaged) | 0x0801E08C | retorno (`bx r0`) | seletor 0x03000579 = etapa 0..2 | 0x02 -> **0x03** (G03), só na etapa 2 |
| H:0x3E (LoadGfxSpawnEffectAtBot) | 0x0801F28A | retorno (`bx r0`) | n/a | 0x3E -> **0x3F** |
| J:0x02 (SubDispatchByStep) | 0x0801FBDE (`mov pc,r0`) | **tabela real**, 5 entradas | seletor 0x03000579 = 0..4 | 0x02 -> **0x03** (ao fim da etapa 4) |
| J:0x02 fallback | 0x0801FD70 | retorno (`bx r0`) | seletor > 4 (sai sem fazer nada) | n/a |
| L:0x0B (LoadGfxSetupSlot10ThenJump) | 0x0802040C | retorno (`bx r0`) | n/a | 0x0B -> **0x0C** |

## E/05 (0x0801D9DC) — retorno, não salto

- Sem tabela. Último trecho: `pop {r4-r7}; pop {r0}; bx r0` em 0x0801DADA..DE.
- Base: fi = 0x03001800 (literal 0x0801DA20). Bot de follow-up = fi+0xDE. Sentido = bit0 de fi+0xF8.
- Desliza Y do bot (fi + bot*0x100 + 0x1640 + 2) em -2 (ou +2) até cruzar 0x78. Chegou: grava 0x78 e tiles, e
  `+0xAA += 1` (0x0801DAD2). Não chegou: segura o passo.
- Próximo: E06. Nada a descobrir.

## G/02 (0x0801DFE0) — retorno, não salto

- Sem tabela. Retorno em 0x0801E08A..8C (`pop {r4,r5}; pop {r0}; bx r0`).
- Seletor: byte 0x03000579 (literal 0x0801DFF8=0x03000440, 0x0801DFFC=0x139).
- Etapa 0: três CpuFastSet (0x0801E006..E028). Etapa 1: uma cópia de 0x800 (0x0801E050). Etapa 2 (0x0801E06C):
  grava ponteiro em 0x0801E090 (=0x030017F0), zera o seletor, chama 0x08040CD0, e `fi+0xAA += 1` (0x03001800 do
  literal 0x0801E098). Etapas 0 e 1 avançam só o seletor (0x0801E05C), sem +0xAA.
- Próximo: G03 (só na etapa 2). Obs.: o byte 0x03000579 é o mesmo de J_02; não concorrem no mesmo passo,
  mas vale conferir no replay se os dois handlers ficarem ativos juntos.

## H:0x3E (0x0801F214) — retorno, não salto

- Sem tabela. Retorno em 0x0801F284..28A (`add sp; pop {r4}; pop {r0}; bx r0`).
- Descomprime gfx (0x080009BC) e paleta (CpuFastSet 8), depois `FUN_08036C20(10, bot+0x8B, x, y, 0x20)`
  (x/y do bot em fi+0x158/+0x15A, ou +0x154/+0x156 se fi+0xF8 bit1). Depois `fi+0xAA += 1` (0x0801F27C).
- Próximo: 0x3F. Quem entra: tabela H (0x3E), e H:0x3B/H:0x4D gravam +0xAA = 0x3E para cair aqui.

## J:0x02 (0x0801FBC4) — tabela real (5 entradas)

- Salto de fato: `mov pc,r0` em 0x0801FBDE. Índice = byte em **0x03000579**. Se índice > 4 (0x0801FBD4) vai para
  o retorno 0x0801FD6C/0x0801FD70 e sai sem fazer nada.
- Tabela: ponteiro no literal 0x0801FBE8 aponta para **0x0801FBEC** (5 palavras, sem bit thumb, valores lidos
  da ROM e batendo com os labels do Ghidra):

| Índice | Alvo | O que faz (fluxo lido) | Seletor após |
|---|---|---|---|
| 0 | 0x0801FC00 | zera ponteiro 0x03000430 e byte 0x03000440+0x96; DISPCNT &= 0xF0FF; BG; `bl 0x0801A788(1)`. Se der 0, espera. Se der 1: zera 0x03000440+0x84, avança | 1 |
| 1 | 0x0801FC78 | DISPCNT |= 0x1F40; `bl 0x080158F8`, `0x0801A0D0`, `0x08043F9C`; lê byte fi+0x38 e chama `0x080441A8` com a palavra da tabela 0x083B2628 (registros de 5 palavras, por fi+0x38; NÃO decodificada) | 2 |
| 2 | 0x0801FCC4 | três CpuFastSet (0x0807CCC0), 0x200 unidades cada: origem 0x02021000, 0x02021800, 0x02022000 -> VRAM 0x0600C800, 0x0600D000, 0x0600D800 (tiles de fundo) | 3 |
| 3 | 0x0801FD10 | CpuFastSet de 0x800 unidades: origem 0x02028000 -> 0x03002C30 (gObjectSlots) e avança | 4 |
| 4 | 0x0801FD3C | `bl 0x080158F8`, `0x0801A0D0`; grava 0x08044101 em **0x030017F0** (ponteiro de rotina, não decodificado); zera 0x03000440+0x84; zera fi+0x1AE; **fi+0xAA += 1** (0x0801FD60); zera seletor 0x03000579 | 0 |

- Próximo +0xAA: J:0x02 -> **J:0x03**. Descreve o que indexa: o seletor 0x03000579 (etapas de carga de fundo do
  passo J:0x02). O fi+0x38 indexa a tabela 0x083B2628 (5 palavras por valor), que não foi decodificada.
- Labels de J: são cases internos do J_02 (não são funções no Ghidra; ver `JT_lista.txt`).

## L:0x0B (0x0802036C) — retorno, não salto

- Sem tabela. Retorno em 0x0802040A..0C (`pop {r4,r5}; pop {r0}; bx r0`).
- Descomprime gfx (0x080009BC) e paleta (CpuFastSet 8). Pega x/y do bot (fi+0x15A..; via fi+0xF8 bit1).
  Cria OBJ em gObjectSlots (0x03002C30 + 0x280): atributo 0xC404 (de 0x08020414), 0x0E e 0x80.
  `Slot_SetupFromRecordList(0x083B6100+0x94, 10)` (slot 10), SFX 0x37 (`0x08074DE0`), e `fi+0xAA += 1`.
- Próximo: 0x0C.

## Fallback de J (0x0801FD70) e retornos

- 0x0801FD70 é o `bx r0` do retorno comum de J_02 (usado por J e pelos cases 0..4). Não é alvo de salto.

## Pendências

- Verificar no replay (ou no decompile) quem chama J:0x02 e G:0x02 com fi+0xAA = 0x02 (não confirmado aqui).
- Tabela 0x083B2628 (fi+0x38, 5 palavras por valor) e o destino 0x080441A8: não decodificados.
- Ponteiro 0x08044101 gravado em 0x030017F0 em J:0x02 idx 4 (rotina pós-passo?): não decodificado.
- Correção para os docs antigos: HA1.md (E/05), HA2.md (E05 e G02, linhas ~55, ~138, ~390), HA3.md (H:0x3E,
  J:0x02, L:0x0B) dizem "tail-call por ponteiro não recuperado". Na verdade é retorno normal. Quem mantém
  esses arquivos deve trocar a frase.
