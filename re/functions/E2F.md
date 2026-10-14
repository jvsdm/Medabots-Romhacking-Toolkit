# E2F - tamanho real do opcode de evento 0x2F e 0x7D

Status: 0x2F = **confirmado** no handler (leitura completa); regra de N (tamanho da tabela) = **heurística confirmada por exemplos**, não por código.
0x7D = **confirmado**. 0x02 após 0x18 = **desalinhamento** (dentro de tabela do 0x2F), não mensagem.
GH-E2F, 2026-10-14. ROM: `D:/claude-brain/Medabots/Medabots - Metabee (Europe).gba` (só leitura, sem emulador).
Leitura do extrator: `pc/extract/extract_events.py` (importada por caminho, sem edição). Rascunhos apagados.
Nenhum texto do jogo aqui: só endereços, bytes, ids e nomes de opcode.

## 1. Handler 0x2F (leitura completa, Thumb)

Primário `0x0804C444` (entrada da tabela 0x083D222C[0x2F]); continuação `0x0804E7E0` (NoOp, HC.md).

```
0804C444 push {r4,r5,lr}
0804C446 ldr  r0,[pc]        ; r0 = 0x030057E0 (base E)
0804C448 ldr  r1,[pc]        ; r1 = 0x0C8A
0804C44A adds r4,r0,r1       ; r4 = 0x0300646A = cursor do script (E+0xC8A)
0804C44C ldrh r5,[r4]        ; r5 = base = cursor logo APOS o byte 0x2F
0804C44E adds r1,#4          ; r1 = 0x0C8E
0804C450 adds r0,r0,r1       ; r0 = 0x0300646E = id do evento atual
0804C452 ldrh r0,[r0]
0804C454 bl   0x080532D4     ; v = campo de 3 bits do PROPRIO evento (ver 2)
0804C458 ldrh r1,[r4]        ; r1 = cursor = base
0804C45A adds r1,r1,r0       ; r1 = base + v
0804C45C strh r1,[r4]        ; cursor = base + v
0804C45E bl   0x0804D83C     ; lê 1 byte em cursor (= T[v]); cursor += 1
0804C466 adds r5,r5,r0       ; r5 = base + T[v]
0804C468 strh r5,[r4]        ; cursor = base + T[v]  (SALTO; nao ha queda)
0804C46A pop  {r4,r5,pc}
```

Conclusão do handler:
- O 0x2F **não** é linear: ele salta para `base + T[v]`, onde `base` = endereço do byte seguinte ao 0x2F e `v` = campo do próprio evento.
- Só **um** byte da tabela é lido por execução (`T[v]`). O resto da tabela nunca é executado como código; é dado.
- Não há contagem N no código. Não existe "byte de contagem seguido de N alvos".
- Correção ao EV2 §1.3: lá está escrito que o handler é "de efeito linear" e tem "tabela sem fall-through"; a parte do salto está certa, a palavra "linear" não.

### 1.1 Leitor 0x0804D83C (`EventScript_ReadNextByte`, lido inteiro)

- Lê **1 byte** por chamada: `ROM[ base_ev + ptr_off + cursor + banco*0x4000 ]`, com `cursor` = u16 em 0x0300646A, incrementado de 1 a cada chamada (`0x0804D8A6..0x0804D8A8`).
- `ptr_off` vem da tabela de ponteiros de evento (indexada por id em 0x0300646E); `banco` = byte por id (lido em `base+bank_rel+id`).
- Não há leitura de 2 bytes nem de contagem. Logo o tamanho do 0x2F não sai daqui: sai da forma da tabela.

### 1.2 Campo `v` 0x080532D4 (lido)

- `v = (flag do id) & 7` se id é ímpar (byte em `0x030057D8 + ((id&0xFFF)>>1)`, bits 0-2; base corrigida) (EF); `v = (byte & 0x70) >> 4` se id é par (bits 4-6). Faixa 0..7.
- Logo a tabela tem no máximo **8 entradas** (T[0]..T[7]); a tabela pode ser menor.

### 1.3 Regra de tamanho (N) - heurística

O tamanho `N` da tabela não está no código. A regra abaixo é a mesma do port C# (`EventScriptReader.ReadMultiJumpOffsets`, `src/Medabots.Rom/Events/EventScriptReader.cs:163`) e do extrator (`multi_jump_offsets`, linha 203). Ela assume que o primeiro caso começa **logo após a tabela** (`base+N`):

```
T = []
para k em 1..8:
    T.append(ROM[base + k - 1])
    se k >= 2 e min(T) == k:  -> N = k ; alvos = {base + t : t em T[:k]} ; fim
N = 8 (sem parada)
```

- Consequência: **o tamanho do 0x2F é 1 + N bytes, com N variável de 2 a 8** (medido nos sítios alcançáveis: N=2: 306; N=3: 131; N=4: 55; N=5: 30; N=6: 9; N=7: 5; N=8: 5; total 541).
- Como a instrução nunca cai no byte seguinte, a parte que importa para o walk é só o conjunto de alvos. O comprimento só importa para quem quer "pular" o 0x2F linearmente.
- Limite de confiança: a regra é consistente com os exemplos abaixo, mas não foi verificada em execução. Qualquer N maior que o real só adiciona nós extras no walk (ver §3: não muda o fim de script).

### 1.4 Prova (exemplo do evento 0, sítio 0x00784FCA)

Bytes (ROM, do 2F em diante): `2F 04 0E 18 | 22 01 02 00 2C 10 31 ...`
- 2F em `0x00784FCA`; base = `0x00784FCB`.
- T = [04, 0E, 18, 22] -> k=4, min = 4 -> **N = 4**. Tabela = `0x00784FCB..0x00784FCE`.
- Alvos: `base+04 = 0x00784FCF` (= `01 02 00 2C`: mensagem, banco 0, id 0x2C), `base+0E = 0x00784FD9`, `base+18 = 0x00784FE3`, `base+22 = 0x00784FED`.
- O caso 0 começa em `base+N` exatamente (`0x00784FCF`): a regra de "logo após a tabela" fecha aqui.

## 2. Handler 0x7D (lido)

- Primário `0x0804D490`: `bl 0x0804D83C` **uma vez**, grava o byte em `0x03006412` (E+0xC32). Sem continuação (cont. = `0x0804DC34`, NoOp).
- Tamanho total = **2 bytes** (1 arg). Não há ramo nem tabela.
- Eventos afetados (todos com 0x7D como erro no extrator): 1954, 2061, 2062, 2063.

## 3. Contagem antes/depois

Varredura dos 2288 eventos (`event_count` = 2288; BFS igual a `decode_event`, só em memória, sem editar o extrator); "End" = alcança 0x06. Scripts descartáveis, apagados.

Limite de nós: o extrator usa `MAX_NODES = 256` (o docstring diz 64). EV2 usou 512. Os dois estão abaixo.

| Variante (só muda 0x2F / 0x7D) | End @256 | End @512 | Saltos só | Erros @512 |
|---|---|---|---|---|
| **Atual** (2F multijump, C# heurística; 0x7D fora) | 2088 | **2090** | 95 | 103 (99 vazios + 4 de 0x7D) |
| 2F tabela N fixo 1..8 (qualquer N) | 2088 | 2090 | 95 | 103 (igual) |
| 2F = regra min (§1.3) | - | **2090** (diferença de ids = 0 para o atual) | 95 | 103 |
| **0x7D = 2 bytes** (sem mudar 2F) | 2092 | **2094** (+4) | 95 | **99** (todos vazios) |
| 2F linear, tamanho 1 | - | 1827 | 100 | 361 |
| 2F linear, tamanho 2 (= ROM_SIZE_FIX atual, se fosse linear) | 1895 | 1898 | 103 | 287 |
| 2F linear, tamanho 3 (melhor linear) | - | 2014 | 103 | 171 |
| 2F linear, tamanho 4..9 | - | 1894 / 1836 / 1825 / 1963 / 1953 / 1887 | 98-100 | 227-364 |

Leitura:
- Trocar o N da tabela **não muda** quais eventos chegam ao End: 0 eventos a mais. O 2F já é tratado como salto, e o walk não depende de N para achar o End.
- Tratar o 2F como linear de qualquer tamanho **perde** de 76 a 263 eventos. Isso confirma que o 2F não é linear.
- 0x7D com 2 bytes: **+4 eventos** (1954, 2061, 2062, 2063). Os 99 erros que restam são todos slots vazios: os 64 primeiros bytes do início são zero (teste feito para os 99).

## 4. O 0x02 depois de 0x18 com banco 0x00: desalinhamento

- O 0x02 é "Show_Message_B" (primário `0x0804B74C` = mesmo helper de 0x01, `0x0804B758`, que lê 3 bytes). Sua continuação (`0x0804DCF8`) só chama `Event_EndOpcode` (`0x08050290`). Então, pelo código, ele é mensagem, mas com banco = byte - 2: **banco 0x00 é inválido** (banco -2).
- Os sítios de "0x18 seguido de 0x02" aparecem **só** na varredura linear (tamanho 2 do 0x2F). No walk atual (multijump) **nenhum** 0x02 é alcançado (contagem = 0 em todos os eventos).
- Os 7 sítios (linear) estão **dentro da tabela de um 0x2F**, e o 0x18 não é alcançado pelo walk atual:
  - ev 0 (0x00784FCD), ev 1 (0x00784FFA), ev 4 (0x00785048), ev 6 (0x00785097), ev 7 (0x007850C4), ev 8 (0x007850F1): mesmo padrão `2F 04 0E 18 22 01 02 00 2C`. Tabela `04 0E 18 22` (N=4); o "18" é T[2] (byte `0x00784FCD`); o "02" em `+3` é o byte de banco da mensagem `01 02 00 2C` que começa em `0x00784FCF` (alvo T[0]).
  - ev 1390 (0x00774AAC): `2F 03 18 1D 63` com 2F em `0x00774AAA`; base `0x00774AAB`; T = [03, 18, 1D] (N=3, `min` = 3). O "18" é T[1]; o caso 0 começa em `base+3` = `0x00774AAE` (byte 63), e o "02" seguinte é argumento desse caso, não opcode.
- Conclusão: **desalinhamento**. Os bytes 0x18/0x02 são entradas de tabela lidas como instruções pelo walk linear. Não há mensagem com banco 0.

## 5. Proposta para o extrator (NÃO aplicada)

O extrator atual já implementa a regra de §1.3 (`multi_jump_offsets`, linha 203). Mudanças recomendadas, em ordem:

```python
# (a) 0x7D: 1 byte de argumento, nao esta no C#
ROM_SIZE_FIX = {0x2F: 2, 0x50: 4, 0x52: 3, 0x66: 3, 0x7D: 2}   # 0x7D +4 eventos (1954, 2061, 2062, 2063)

# (b) 0x2F: o tamanho nao e 2. Ele e 1 + N (N = 2..8, regra de multi_jump_offsets).
#     O valor 2 em ROM_SIZE_FIX nunca e usado pelo walk (0x2F sai antes, em MULTIJUMP_OP).
#     Trocar por marcacao explicita, para nao confundir quem le ops[0x2F][2]:
ROM_SIZE_FIX = {..., 0x2F: None}   # variavel; o walk trata por MULTIJUMP_OP
# ou: manter 2 com comentario "variavel: ver E2F.md".

# (c) Documentar o limite real: MAX_NODES = 256 (docstring diz 64 instrucoes).

```

Pseudocódigo do walk (já equivalente ao extrator, só para registro):

```
caso 0x2F:
    base = pos + 1
    (T, N) = regra_min(ROM, base)            # §1.3
    para t em T: visitar(base + t)           # alvo = base + T[j]
    parar                                     # sem queda; ocupa 1 + N bytes
caso 0x7D:
    proximo = at + 2                          # 1 byte de argumento
```

Obs. sobre a regra "contagem N seguida de N alvos" (hipótese do pedido): **descartada pelo código**. O handler não lê contagem, e a tabela não tem cabeçalho. A regra certa é a de §1.3.

## 6. Abertos

- N é heurística (primeiro caso logo após a tabela). Confirmar por execução (mGBA) em um 2F de N=4 (ev 0): escolher o estado do evento e ver se o salto cai em `base+4`.
- O campo `v` usa o nibble/bit do próprio evento: o walk atual visita todos os alvos da tabela (superconjunto), o que é correto para o alcance, não para "qual caso roda".
