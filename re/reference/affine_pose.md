# Pose afim do bot grande (tarefa 11) - estatica, SEM resposta ainda

Sessao nuvem, 2026-10-12. O repo nao tem ROM nem decompilacao dos renderizadores (so os comentarios do Simon no CSV e
`battle_anim.md`), entao **a formula nao pode ser derivada aqui**. Este arquivo separa o que ja esta nos dados do que
so o Ghidra responde, para a sessao local nao refazer leitura.

## 1. O que ja se sabe (de `battle_anim.md` e do CSV)

- Estado por molde (16 B, `MotionInterpolateKeyframe` 0x08047428): `+0x18` x 8.8, `+0x1A` y 8.8, `+0x1C` angulo s16
  (1/65536 de volta, ou seja 20 graus = ~0x0E39). O interpolador e inteiro: `atual += d / restante` (divisao com
  truncamento a cada quadro, nao fracao acumulada) - um tocador em ponto flutuante diverge disso por ate ~1 unidade/quadro.
- Quem transforma angulo -> matriz: `SetObjectAffineMatrixFromAngleAndScale` 0x08001CAC (Simon: "fixed default-scale
  and externally supplied scale sources") e `SetObjectScaleVectorFromMode` 0x08001EA8; `RotateObjectOffsetByAffineAngle`
  0x08001FF8 gira um vetor (x,y) pelo angulo do registro de objeto. Esses sao do motor de objetos GERAL (OAM sombra
  0x08001520), nao do preview composto.
- O ramo afim do bot grande esta em `RenderCompositePreviewDescriptorRecursive` 0x080465B8 (entradas com bit 14) e
  `QueueVisibleCompositePreviewTemplateEntryWithAffineBounds` 0x08045C30 / `QueueCompositePreviewTemplateEntryOam`
  0x08045680; `ExpandCompositePreviewTemplateEntryToOam` 0x08045120 gera os halfwords `+0x0C/+0x0E` lidos pelo ramo afim.
- `large_part_layout.md` secao 4 so cobre a pose de repouso (angulo 0, escala 1).

## 2. Hipoteses a testar (todas "provavel")

1. **Unidade/formula**: `idx = (angulo >> 8) & 0xFF` em tabela de 256 (padrao BIOS ArcTan/ObjAffineSet usa 16 bits
   `>> 8`); `pa = cos*sx`, `pb = -sin*sx`, `pc = sin*sy`, `pd = cos*sy`, em 8.8. Se o jogo usa o angulo cru (1/65536)
   como 0..255 << 8, a diferenca de ~20 graus do PC pode ser **sinal** (lado 0 gira ao contrario do lado 1) ou
   **pivo**, nao a unidade.
2. **Pivo**: rotacao em torno do CENTRO do sprite (OAM afim double-size ou nao), e nao em torno de rawX/rawY. Um molde
   girado em torno do centro tem a posicao do canto movida por `R*(c - p) - (c - p)` (c = centro, p = pivo
   rawX/rawY). Se o PC gira em torno do pivo, o angulo "parece" errado em ~dezenas de graus nos bracos.
3. **Deslocamento global do corpo (+30 px e volta)**: provavelmente e a **linha do molde 1** (raiz) em
   `gMotionOffsetTable[roteiro]` (offset s8,s8 por quadro-chave), acumulada na ancora de todos os filhos, mais
   a posicao do slot de objeto (`gObjectSlots`), e nao um passo separado. Checar: roteiro 44 (tipo 13), linha 1 da
   tabela de deslocamentos, 13 quadros-chave.
4. **Inicio do tipo 13**: `MotionPlayStep` toca desde k = 0 (contador zerado quando o tipo muda, a conferir se a
   troca de tipo zera k e o contador de `estado+lado*0x410`); a duracao da primeira etapa e `dur[0]`.
   Se o PC comecar do quadro-chave 1, o angulo inicial diverge.

## 3. O que a sessao local precisa responder (nesta ordem)

1. Dump de `gMotionOffsetTable[44]` e `gMotionAngleTable[44]` (roteiro do tipo 13 estilo 0): linha do molde 1 e dos
   moldes de braco (14-19), todos os 13 quadros-chave; comparar com a medicao do PC (corpo +30 e volta).
2. Em `0x08001CAC`: ler (decompilar) como `angulo` vira `pa/pb/pc/pd`: tabela de seno (onde? tamanho?), shift do
   angulo, `escala` (de onde vem; 0x100 = 1.0?). Anotar no CSV.
3. Em `0x080465B8`/`0x08045C30`: onde o angulo do estado do molde (`+0x1C`) entra (lido de `estado+molde*0x10`?), se
   o sprite afim usa flag double-size (bit 9 do attr0), e como a posicao do canto compensa o centro.
4. Watchpoint de escrita em `gMotionState + molde*0x10 + 0x1C` durante o corte do Missile (rec_2 f1446-1550): confirmar
   que o primeiro valor escrito e `angulo[roteiro][molde][0]` (hipotese 4) e que o interpolador e o descrito.
5. Ler a OAM afim (OAM 0x07000006/E/16/1E de cada grupo) de um quadro conhecido e comparar com `angulo` => descobre
   unidade, sinal e escala sem ler codigo.
6. Se 1-5 nao fecharem a diferenca de ~20 graus: comparar o PC contra o jogo no mesmo quadro-chave (k = 0, `restante`
   = 0) para separar erro de interpolacao (inteiro vs ponto flutuante) de erro de formula.

## 4. Conferido local (G6, 2026-10-12, Ghidra offline: desmonte com tools/rom_asm.py)

- **0x08001CAC** (chamado so de `BuildShadowOamFromObjectState` 0x08001520, em 0x080017B0): `idx = angulo & 0x3FF`
  (1024 passos por volta, nao 65536). Tabela de seno Q14 em ROM 0x08075250 (1024 halfwords; sin[0]=0, sin[256]=16384);
  `cos = tab[idx+256]`, `sin = tab[idx]`. Escalas sx/sy vem dos bytes +8/+9 do registro do objeto (0 vira 1). Bits
  0x1000 e 0x2000 do halfword de flags negam X/Y. Escreve no buffer afim 0x03001380: grupo de 0x20 B por indice,
  pa +0x06, pb +0x0E, pc +0x16, pd +0x1E.
- **Angulo de entrada**: halfword em `registro_objeto+0x18` (o chamador faz `ldrh [r6,#0x18]` e passa direto). Nao e o
  campo +0x1C 1/65536 do molde da secao 1; a unidade certa (1/1024 ou >>6) ainda nao foi confirmada.
- **OAM real (rec_2, f1470, f1446-1546 a cada 4)**: sprites afins com attr0 bit 9 (double-size) = 0 em todos, entao o
  pivo e o centro do sprite. Matriz medida: pa = 256 em angulo 0 e ~255 com angulo; pb = +sin*sy, pc = -sin*sx.
  Angulos das pernas quantizados em passos de 8 indices (2,81 graus).
- **Tocador x jogo NAO fecha**: pernas 20-24 (OBJ 94/96/95/93/92, tiles 544/548/552/568/572) comparadas com
  `pc/game/src/motion.lua` (tipo 13, bot 43, roteiro 44, 73 quadros) em angulo local e em angulo acumulado pela cadeia
  de pais (`cache/battle/043/battle.lua`), com sinal +/-1, escala de tempo 0,5-1,5 e deslocamento -60..+80 quadros.
  Melhor erro medio quadratico ~19 graus nas pernas que se mexem (perna 20 e 22 so batem porque ficam quase paradas).
  Logo falta algo alem de unidade/sinal: ou o tipo que o jogo toca nao e 13 durante todo o corte, ou o quadro de
  inicio nao e o mesmo.
- Arquivos de apoio (apagados): dumps OAM em scratchpad G6.

## 5. Proximo passo sugerido

1. Confirmar o angulo de entrada: watch de escrita em `registro_objeto+0x18` durante f1446-1546 (rec_2) e comparar o
   primeiro valor com `angulo[roteiro][molde][0]`.
2. Achar o quadro de inicio do roteiro no jogo (primeiro quadro em que a perna 21 sai de 0) e ver se os tipos
   seguintes (14/15/12) mudam o angulo do OAM. B16 ja diz que o slot 1 fica em 13 de f1445 a f1565.

## 6. G7 (rec_2 ck_1400, f1440-1560): lista de registros e +0x18 (PARCIAL)

- **Lista que 0x08001520 le**: base `0x03002C30` (= `gObjectSlots`, mtbe.csv), 128 registros x 0x40 B. Laco em
  0x08001520+0x44 (pc 0x08001564): so entra registro com bit 0x8000 em +0x00; ordena por byte +0x0B; r6 = base + k*0x40.
  Campos vistos nos registros ativos (halfwords): +0x00 flags (0xC404), +0x02 y, +0x04 x (strh pelo proprio 0x08001520),
  +0x06 attr2-like, +0x0A (0x800C), +0x20, +0x28, +0x2C (tipo). Escala/flip em +0x08/+0x09 (bytes).
- **+0x18 (angulo)**: chamada de 0x08001CAC so se `ldrh [r6,#0x18] != 0` (0x0800179E-0x080017A2) e indice <= 0x1F.
- **Medido**: watch de escrita sobre `0x03002C30:0x2000` em f1440-1560 (rec_2, ck_1400): ZERO escritas em +0x18 de
  qualquer registro (as escritas caem em +0x00..0x06, +0x0A, +0x20..+0x32). Dumps em f1470/1500/1530: registros ativos
  (1530: slots 10, 11, 14-19) com +0x18 = 0. Logo, em rec_2 nesta janela os sprites afins das pernas NAO saem deste
  laco com angulo; a pose do OAM (pa/pb/pc/pd em 0x03001380) vem de outro caminho ou de registro fora de 0x03002C30.
  Nao rastreado: quem escreve 0x03001380 (watch) e a origem do angulo.
- Ferramenta: `tools/replay_record.py` agora loga todas as escritas (`DIFF`) do --watch (antes cortava em 48 por quadro;
  agora 4096). Nao faz diferenca nos prints.
- Nao feito (G7 passos 2-3): tabela quadro->molde->{x,y,ang} e G7_compare.png nao gerados; pose do tocador segue sem
  fechar com o jogo.

## 7. G8 (rec_2 ck_1400, f1446-1530): de onde vem a pose das pernas (PARCIAL, 2026-10-12)

- **Correcao de G6/G7**: o buffer 0x03001380:0x400 e a OAM sombra inteira (128 x 8 B). Os campos afins sao os
  halfwords com endereco = 6 mod 8 (pa +6, pb +E, pc +16, pd +1E de cada grupo de 0x20). Escritas em outros
  enderecos (attr0-2) vem de 0x08001C78/0x08001B82-0x08001BF6 (laco de 0x08001520, normal) e 0x080006EC (copia).
  O 0x08001CAC NAO escreve nas pernas nesta janela: todas as escritas de pa/pb/pc/pd com watch filtrado por
  endereco (6 mod 8) vem de outro caminho (abaixo).
- **Caminho real (dinamico, medido)**:
  1. VBlank `FUN_0800065c` (ramo `*0x08000780 == 2`) chama `FUN_08045a14`: copia 32 grupos de 4 halfwords
     (src -> EWRAM stage, stride 0x20 no destino) = a matriz afim por molde, pronta para a OAM.
  2. A fonte (EWRAM 0x02024FA4, 0x100 B) e escrita por `FUN_080465B8` (RenderCompositePreviewDescriptorRecursive,
     recursiva) em 0x0804685C-0x08046910, chamada de 0x08046794/0x08046A1F/0x08046A51 etc.
  3. Dentro dela, o angulo acumulado (local_58 = pai + halfword do registro +0x0C) vai para `FUN_0807cce0`, que e
     so um wrapper de `SWI 0x0F` (BIOS ObjAffineSet): o seno/cosseno vem da BIOS (alpha >> 8 = 256 passos por
     volta; sx/sy dos bytes do molde). Matriz (hipotese, padrao da SWI, NAO conferida): pa = cos*sx, pb = -sin*sx, pc = sin*sy, pd = cos*sy.
  4. Registro do composto: ctx 0x02026000 (word0 0x02024000, word1 = 0x083D1D78 ROM, word2 = 0x020260D0 = registros
     de 0x10 B por molde). Halfword +0x0C de cada registro = angulo do molde (1/65536 de volta). Pernas do Metabee:
     k=7,9,11 e k=8,10,12 (mesmo valor nos pares, em 1480: 0x06E8/0x06B0; em 1520: 0x1A2B/0x0E9A); raiz k=14/15.
  5. Quem grava o +0x0C: `0x080474F4`/`0x0804752E` (lr 0x08044AD3), copia de `state+0x1C`; o `state` (gMotionState,
     molde*0x10) e atualizado por `FUN_08047428` = interpolador: a cada quadro anda `FUN_0807ccc8(alvo-atual, restante)`
     em `+0x1C` (angulo, 1/65536) e em `+0x18/+0x1A` (x,y <<8), com alvo = `gMotionAngleTable` (param_2) e
     posicao = `gMotionOffsetTable` (param_3), passo de tabela = param_9, contagem = param_8 (moldes).
  6. Entrada: `FUN_08046D1C` (BuildFourPartLargePreviewComposite) chamada por `FUN_080447ec` (lr 0x080448DD), com
     r2 = 0x2B (sel 43 = bot 43), a cada quadro do corte.
- **Aberto (nao fechado)**: (a) a formula exata de sx/sy e do pivo do molde (bytes +0x12/+0x13 e o `restante`);
  (b) mapear grupo OAM (pa em 0x03001380+0x20k+6) para o registro k; (c) confirmar em 2 quadros que
  alpha = (angulo>>8) bate com o pa/pb medido (o grau de quantizacao da BIOS e 1,41 grau); (d) a comparacao de
  pose com o tocador `pc/game/src/motion.lua` (B15/B16 seguem valendo). rig.lua/battle.lua NAO alterados.
- Ferramenta de apoio: `replay_record.py --watch` filtrado por PC (`--watch-skip-pc`) e `--break` com lr funcionam
  para achar escritores; `--break` grava muitos .bin (apagar depois).

## 8. G9 (rec_2 ck_1400, f1460-1530): interpolador conferido valor a valor (PARCIAL: pivo/OAM ainda nao)

- **Enderecos (medidos e lidos no literal pool de site_MoveStyleRead 0x08044A3C)**: contador do lado s = byte 0x020260CC+s;
  passo (keyframe) do lado s = byte 0x020260CE+s; estado do lado s = 0x020260D0 + s*0x410; molde m (1-based) em
  0x020260D0 + 0x10*(m-1): x +0x18 (8.8), y +0x1A (8.8), angulo +0x1C (1/65536). Tabelas ROM: angulo 0x083D1E40,
  deslocamento 0x083D1F2C, duracao 0x083D2018 (ponteiro por roteiro), roteiro->id 0x0807E8EC/0x083CAD78.
- **Mapeamento de moldes**: indice de estado g (0-based) = molde g+1 (linha g+1 da tabela). Ex.: pernas do bot 43 =
  moldes 7..12 = estado 6..11 (7/9/11 iguais, 8/10/12 iguais).
- **Roteiro do tipo 13, bot 43**: roteiro 44 (indice 0-based), 13 keyframes, duracoes 8,8,8,6,6,6,4,1,1,1,2,16,6 (73 quadros).
  O contador zera em f1460 (novo roteiro, passo 0); ha 2 chamadas paradas (f1461, f1462 com contador 0); o passo 0
  comeca a contar em f1463. **Quadro 0 do tocador = f1462** (estado antes do 1o passo); quadro n = f1462+n.
  Passo 1 em f1470, passo 2 em f1478, passo 12 em f1530 (conferido).
- **Estado de partida NAO e zero**: em f1462 os moldes ja carregam o estado do roteiro anterior (ex.: molde 7 = 1820,
  molde 6 = (768,-256) px*256). O interpolador parte desse estado; o jogo nao zera os moldes ao trocar de tipo.
- **Interpolador (decompilado, confere)**: a cada quadro contador++; se passo < N: para cada molde (63 moldes, 0..62 no
  laco, estado g), alvo = tabela[molde g+1][passo] (zero se o molde nao tem linha); se contador < dur: restante = dur -
  contador, angulo += Div(menor_caminho(alvo-angulo), restante) (menor caminho so entre d e d-65536), x/y += Div(alvo*256 -
  x/y, restante); senao snap ao alvo. Se algum molde fez snap, passo++ e contador = 0. Divisao truncada (C).
- **Medicao**: `motion.lua` (corrigido, ver abaixo) com o estado de f1462 como partida, comparado com os dumps de
  f1463..f1530 (todos os moldes 1..49): erro 0 no angulo e 0 px na posicao. Os moldes 50..62 nao tem linha na tabela e o
  jogo le dados fora dela (erro grande la; sem uso no bot 43 aparente).
- **Correcoes em motion.lua (G9)**: (a) `start(tipo, bot, prev)` aceita o estado anterior e herda os moldes (sem `prev`,
  zero como antes); (b) o laco passa a cobrir os 63 moldes, molde sem linha tem alvo 0 (antes ficava parado, o que
  deixava 2-3 px de erro com estado herdado). rig.lua e battle.lua NAO alterados (battle.lua ainda nao passa `prev`).
- **Unidade do angulo**: o tocador usa 1/65536 de volta, igual ao estado. O BIOS ObjAffineSet usa so alpha>>8 (256
  passos por volta, 1,41 graus); o pivo e o sinal da matriz ainda NAO conferidos com a OAM afim (proximo passo: dump de
  0x03001380 por quadro e comparar pa/pb/pc/pd com cos/sin de (ang>>8)*1,41 graus e sx/sy dos bytes do molde).
- Teste: `replay_record.py rec_2 --frames 1530 --dump 0x020260C0:0x440@F...` (F = 1462..1530, usa ck_1400) e comparar com
  `motion.lua` (start com o estado do dump de f1462). Script de conferencia nao foi guardado (apagado com o scratchpad).
