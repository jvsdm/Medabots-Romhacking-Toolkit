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
