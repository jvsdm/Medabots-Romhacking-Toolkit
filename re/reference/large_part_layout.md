# Montagem do bot grande (preview) para qualquer bot

Tarefa 6 do HANDOFF. Escrito na nuvem por leitura estatica do Toolkit
(`ImageAssetRepository.LargePartDisplay.cs`, `ReadMedabotLargeDisplayFrame` e auxiliares) cruzado com
`EXPANSAO_MEDABOTS.md` 5.2 e 15 e com o CSV. **Sem ROM: tudo abaixo e "provavel" ate a sessao local rodar
`re/tools/large_part_layout.py` e conferir (secao 7).** Enderecos de arquivo da Metabee (E); somar `0x08000000` para memoria.

## 1. Tabelas

| O que | Offset | Tamanho | Nota |
|---|---|---|---|
| Tabela de moldes | `0x3D1D78` | 50 ponteiros (64 na ROM expandida) | indice = numero do molde; ponteiro -> registro de 0x18 B |
| Juntas sinteticas (moldes 3-6) | `0x7EE10` | 4 x 0x18 B | = `head_table - 0x60`; ordem: cabeca `0x7EE10`, `0x7EE28`, `0x7EE40`, pernas `0x7EE58` |
| Linhas de cabeca | `0x7EE70` | 122 x 16 B (4 entradas) | `gPreviewHead` |
| Linhas de braco direito | `0x7F610` | 122 x 16 B (4 entradas) | `gPreviewRightArm` |
| Linhas de braco esquerdo | `0x7FDB0` | 122 x 16 B (4 entradas) | `gPreviewLeftArm` |
| Linhas de pernas | `0x80550` | 122 x 32 B (8 entradas) | `gPreviewLegs`; termina em `0x81490` = mapa de espelho |
| Mapa de espelho | `0x81490` | 50 B | so serve ao movimento em batalha; o preview usa a troca fixa 14-16 <-> 17-19 (secao 4) |
| Paletas OBJ iniciais do preview | `PartDetailObjPaletteBlockA/B/C` (layout do Toolkit) | 3 x 32 B | bancos 9, 8, 10 |

Indice da linha = ID do bot (0-121; 120/121 = Tinpet M/F). Nao ha desvio de +0x20 aqui na Metabee (E);
o +0x20 do Toolkit e so entre Rokusho e Metabee (dados da Rokusho = Metabee + 0x20 ate ~0x08488000; por isso o
`descriptor_table = 0x083D1D98` do comentario do Simon e o nosso `0x083D1D78`).

## 2. Entrada da linha (u32)

```
bits 0-5   molde (0 = fim da linha)
bits 6-13  seletor de arte (8 bits; o original usa 7: patch 0x45ECC, ver 5.3)
bit 14     "centralizado" (0x4000): no lado 1 passa pelo ramo afim da ROM; o Toolkit so espelha o bitmap
bit 15     tem arte propria para o lado 1 / pose 2
```

- Cada linha lista os moldes **usados** daquela peca (so eles tem entrada); moldes de ligacao nao listados nao
  desenham, mas a arvore continua sendo percorrida por eles (so pegam offset).
- Moldes por peca: ate 4 entradas (cabeca, bracos) e ate 8 (pernas). A distribuicao real sai de
  `large_part_layout.py --histogram` (pedido a sessao local). Pelo HANDOFF o Metabee tem cabeca m09+m10,
  braco esq m14+m15, braco dir m17+m18, pernas m20-m24.
- Arte do pedaco: `blobs = u32 @ registro+0`; indice `i = (seletor + (variante & bit15)) * 2`;
  imagem = `u32 @ blobs + 4*i` (LZ77, 4bpp, tiles 1D), paleta = `u32 @ blobs + 4*i + 4` (32 B); se o ponteiro
  da paleta == o da imagem, nao ha paleta. `variante` = lado (0/1) no preview.
- Lado 1: antes de desenhar, o jogo copia a **arte** do braco oposto (mesmo ordinal de peca) para as entradas
  com bit 15 **limpo** (`targetByte1 >= 0`); entradas com bit 15 ligado usam o 2o par (variante 1). O Toolkit
  reproduz isso em `BuildCompositePreviewImageSourceOverrides`. Atributos (paleta, geometria) ficam do alvo.

## 3. Registro de molde (0x18 B)

| Off | Tam | Campo | Fonte/uso |
|---|---|---|---|
| 0x00 | u32 | ponteiro da lista de artes (pares imagem/paleta) | |
| 0x04 | s32 | `rawY`: ponto de encaixe (pivo) dentro da imagem, em y | x/y = ancora - raw |
| 0x08 | s32 | `rawX`: pivo em x | |
| 0x0C | s8 | deslocamento Y do filho em relacao a ancora do pai | acumula |
| 0x0D | s8 | deslocamento X | acumula; negado no lado 1 |
| 0x0E | s8 | chave de ordem (soma na chave herdada) | profundidade |
| 0x0F | u8 | molde **irmao** | mesma ancora do pai |
| 0x10 | u8 | molde **filho** | ancora = a deste molde ja deslocada |
| 0x11 | u8 | banco de paleta OBJ | |
| 0x12 | u8 | **altura** em px (bruta) | o codigo do Toolkit le 0x12 = altura, 0x13 = largura |
| 0x13 | u8 | **largura** em px (bruta) | |
| 0x14 | u8 | divisores: nibble alto = largura, baixo = altura | tamanho efetivo = bruto / divisor (min 1); `NaviImporter` e `EXPANSAO` 15.1 chamam 0x12/0x13 de "largura/altura" nessa ordem: **conferir** |
| 0x15 | u8 | forma OBJ (doc 15.1) | |
| 0x16-0x17 | | desconhecidos | Toolkit guarda como `RawByte16/17` |

Divergencia a conferir: o Simon descreve a chave de ordem do template como "descriptor byte +0x06"; o Toolkit usa
`+0x0E` (s8) e produz montagens corretas. Provavelmente e a mesma coisa vista por uma struct diferente.

## 4. Algoritmo (preview de 4 pecas, lado 0 ou 1)

Entrada: bot, lado. Origem: `ancora = (0x3C, 0x32)` (x, y), chave inicial `0x1C2`. Posicao do desenho = **relativa aos pes**
subtraindo a ancora (resultado esperado: pes na origem; a conferir).

1. Reunir as 4 linhas (cabeca, braco dir, braco esq, pernas) numa lista de entradas ate o molde 0 de cada uma.
2. Juntas sinteticas 3-6: copiar os 4 registros de `0x7EE10`; ids 3 (cabeca), 4 (filho = 1o molde do braco **esquerdo**,
   copiado de `0x7EE28`), 5 (filho = 1o molde do braco **direito**, de `0x7EE40`), 6 (pernas, `0x7EE58`); em cada uma
   `byte 0x10 = 1o molde da linha`. Lado 1: `byte 0x0D = -2 - byte 0x0D`.
3. Percurso a partir do molde **1** (registro real): para cada molde `m`:
   - registro logico `L` = sintetico (se 3-6) ou `tabela[m]`; registro de geometria `G` = `L`, exceto no lado 1 em molde
     real: `tabela[troca(m)]` com `troca`: 14<->17, 15<->18, 16<->19 (hex 0E<->11, 0F<->12, 10<->13).
   - `ay' = ay + G.0C`; `ax' = ax + (lado0 ? G.0D : -G.0D)`
   - `x = ax' - G.rawX` (lado 0) | `ax' - (L.largura_bruta - G.rawX)` (lado 1, imagem espelhada); `y = ay' - G.rawY`
   - `chave' = max(0, chave + G.0E)`
   - se `m` tem entrada, emitir pedaco (largura/altura/banco/arte vem de `L`)
   - recursao: filho `L.10` com `(ax', ay', chave')`; irmao `L.0F` com `(ax, ay, chave)` (ancora e chave do pai).
   Guardar pilha para nao ciclar.
4. Ordem de desenho: por chave **decrescente** (menor chave por cima, como a OAM).
5. Bitmap: lado 1 espelha cada imagem horizontalmente (nas entradas "normais" o jogo usa hflip da OAM; nas
   com bit 14 usa matriz afim; o Toolkit espelha o bitmap nos dois casos).

`large_part_layout.py` implementa 1-4 literalmente. Regra para montar **qualquer** bot: so trocar o ID do bot por peca
(`row(parte, bot_da_peca)`); os moldes sao compartilhados, entao um bot misto (ex.: cabeca do bot A, pernas do B)
sai do mesmo algoritmo.

## 5. Por que a regra gera as posicoes do Metabee (verificacao a fazer)

Sem a ROM nao da para somar os bytes; a conferencia fica com o local (secao 7). Estrutura esperada:
- Cabeca m09/m10: `x = (0x3C + off_x(3) + off_x(m09) [+ off_x(m10)]) - rawX`; alvo (-12,-49) e (-15,-75).
- Braco esq m14/m15 sob a junta 4; alvo (-3,-63) e (0,-42). Braco dir m17/m18 sob a junta 5; alvo (-36,-62) e (-36,-39).
- Pernas m20-m24 sob a junta 6 (e/ou molde 1, que o HANDOFF/`NaviImporter` dizem ser o pai das pernas).
Se algum valor nao bater, os suspeitos, em ordem: (a) qual registro (1 ou 2) liga as juntas 3-6 (molde 1/2 tambem
tem offset: o Navi tem (-4,0) no molde 2, ver 15.5); (b) a regra do irmao (ancora do pai, nao a deslocada); (c) a
negacao de 0x0D no lado 1.

## 6. Notas para o Toolkit/editor

- `anchorX = 0x3C`, `anchorY = 0x32` sao os valores padrao de `ReadMedabotLargeDisplayFrame`.
- O caminho "legado" (`BuildLegacyMedabotLargeDisplayFrame`, largura 68, ancoras por raiz) e uma aproximacao; o
  combinado acima e o fiel a ROM (`Composite*`).
- Desvio conhecido do Toolkit: nao implementa a matriz afim do ramo bit 14, nem a chave de ordem de 16 bits do
  `InsertCompositePreviewOamSpanBySortKey` (0x08045588): se sobrepor errado nas pernas, esse e o proximo ponto.

## 7. O que a sessao local precisa conferir

1. `python re/tools/large_part_layout.py ROM.gba --bot <Metabee> --side 0` e `--side 1`: comparar com as posicoes
   do HANDOFF (cabeca m09 (-12,-49), m10 (-15,-75); braco esq m14 (-3,-63), m15 (0,-42); braco dir m17 (-36,-62), m18 (-36,-39)).
   O id do Metabee e o do jogo (bot 0 no IDs do Toolkit; confirmar).
2. `--dump-molds` para os moldes 1-6: quais sao filho/irmao de 1 e 2 (como as juntas 3-6 entram na arvore).
3. `--histogram`: moldes por peca (4/8 e o maximo; qual e o real).
4. No Ghidra, `RenderCompositePreviewDescriptorRecursive` 0x080465B8: confirmar `y += byte 0x0C`, `x += byte 0x0D` (negado no
   lado 1), subtracao de `rawX/rawY` (u32 em +4/+8) e a ordem `0x12 = altura / 0x13 = largura`.
5. Confirmar `0x7EE10` como a tabela das 4 juntas (4 x 0x18 B antes de `gPreviewHead`) e o par 4/5 esq/dir.
