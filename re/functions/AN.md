# AN: sistema de animacao (motion) dos Medabots grandes em batalha (GH-AN)

Status: CONCLUIDO com pendencias marcadas (2026-10-14). Confirmado por decompile e leitura da ROM (so leitura;
nenhuma imagem extraida). Nomes aplicados no Ghidra: MotionPlayStep, MotionInterpolateKeyframe, gMoveScriptTable,
gMotionOffsetTable, gMotionDurTable (gMotionAngleTable e gMoveStyle ja existiam).

## 1. Cadeia completa (script -> kind -> tocador -> molde)
1. Roteiro do ataque: a linha de script da tecnica (ROM 0x083CE164[sel]) traz o opcode 0x08, `ObjectOpcode08_SetMotionFromTable` 0x080395E8.
   Args: a0 = parametro u16; a1..a3 = kind do motion por peca (slot 1); a4..a6 = kind por peca (slot 2). Peca k -> a[1+k].
   Peca = byte `fight+0xA7` (ou `+0x162` se `fight+0xF6 & 2`). Os args ficam em `fight+0x102+2*N` (a1 = +0x104).
2. O opcode grava no slot de objeto (slot 0 base 0x03002C30; slot n = base + n*0x40):
   slot 1 `+0x2C` = a[1+peca] (escrito como base+0x6C); slot 2 `+0x2C` = a[4+peca] (escrito como base+0xAC).
   Lado do atacante (`fight+0xF8 & 2 == 0`) -> slot 1; senao slot 2. Confirmado no decompile.
3. `FightPreview_ApplyObjectMotionKind` 0x0803BBE0 copia `slot+0x2C` para `fight+0x112` (lado 0) ou `fight+0x113` (lado 1).
   Bit 0x80 de `fight+0x112` = tipo concluido (ligado por `FightPreview_AdvanceLeftSideSequenceStep` 0x080361F6 ao acabar).
   Os 7 bits baixos sao o `kind` que vai ao tocador. Bot do lado 0 = `fight+0x67`.
   Outros escritores de `fight+0x112`: `ObjScript_Op14_SpawnActorOffscreen` 0x08039E00 (grava 0x29 = entrada em cena) e `FightPreview_ResetSequenceStepFlags` 0x080361A8.
4. `MotionPlayStep(side, kind, bot)` 0x08044A3C avanca um passo do tipo (secao 2).
5. Chamadores: `FightPreview_AdvanceLeftSideSequenceStep` / `...RightSide...` (cena de ataque); `Fight_DispatchOnSubstateAA_J_00_PreviewAdvanceSideSequences` 0x0801FA70;
   `DebugMotion_PlayKind` 0x0800D90C (teste: kind do menu de debug nos dois lados por quadro); `ObjScript_Op14` (kind 0x29); `menu_state2_setup` 0x0806E9E0 (kind 0x2C); `Preview_InitCompositeAndMotion` 0x08072234.

## 2. Tocador: MotionPlayStep 0x08044A3C (confirmado)
- `r = gMoveScriptTable[kind*7 + gMoveStyle[bot]]` (u32 na ROM em 0x0807E8EC). `gMoveStyle` = 1 byte por bot em 0x083CAD78 (0..6, estilo das pernas).
- Contadores EWRAM: quadro do lado s = 0x020260CC + s; keyframe (progresso) = 0x020260CE + s.
- Estado dos moldes do lado s = 0x020260D0 + s*0x410; 16 B por molde (+0x18 x 8.8, +0x1A y 8.8, +0x1C angulo 1/65536 de volta).
- Passo: se progresso < N (N = keyframes de `dur`), chama o interpolador (estado, angulos[r], posicoes[r], dur[r], quadro, progresso, 0x40 moldes, passo N).
  Retorno > 0 (algum molde chegou) -> quadro = 0 e progresso += 1. Ao passar do ultimo keyframe: progresso = 0 e retorna 1 (tipo acabou).
- Literais na ROM: 0x08044AF4 = 0x0807E8EC; 0x08044AF8 = 0x083CAD78; 0x08044AFC = 0x083D2018; 0x08044B0C = 0x083D1E40; 0x08044B10 = 0x083D1F2C.

## 3. Interpolador: MotionInterpolateKeyframe 0x08047428 (confirmado)
- `int f(MotionState *st, s16 *ang, s8 *pos, u8 *dur, u32 _, u32 quadro, u32 k, int moldes, int passo)`.
- Tabelas: `ang` = s16[molde][N] (passo de linha = N shorts); `pos` = pares (s8 x, s8 y)[molde][N]; `dur` = u8[N], uma duracao por keyframe, compartilhada pelos moldes.
- Por molde, com `quadro < dur[k]`: falta = dur[k] - quadro; angulo += bios_div(delta, falta), onde delta e a diferenca de angulo de menor modulo entre d, d-0x10000 e d+0x10000 (caminho curto; 0x10000 = volta). x e y (8.8) andam bios_div(alvo*256 - atual, falta).
  Com `quadro >= dur[k]`: cola no alvo (angulo = alvo; x = s8*256; y = s8*256) e conta como chegado.
- O laco roda 63 vezes (0x40 - 1). Estado g = molde g+1 (estado 0 = molde 1, a raiz). Ver pendencia 6.

## 4. Registro de motion: exemplo numerico (roteiro 44, tipo 13, bot 43, estilo 0)
- `gMoveScriptTable[13*7+0] = 44`. Ponteiros do roteiro 44: angulos 0x080A2E26, posicoes 0x080A3320, duracoes 0x080A381A.
- N = 13 keyframes; dur = [8,8,8,6,6,6,4,1,1,1,2,16,6] (soma 67 quadros).
- Molde 1 (raiz): angulo 0 em todos os keyframes; (x,y) = (0,-2),(-1,6),(-1,18),(0,21),(1,32),(1,32),(1,32),(1,28),(1,22),(1,19),(1,19),(2,2),(0,-2).
  O corpo sobe/desce ~30 px e volta (o "+30 px e volta" das notas G10/B11; ver affine_pose.md).
- Os moldes das pernas (20-24 neste bot) tem angulo nao nulo; ver affine_pose.md secao 9 (G10).

## 5. Pose afim do molde (resumo; detalhe em re/reference/affine_pose.md)
- O composto e montado em `RenderCompositePreviewDescriptorRecursive` 0x080465B8. O angulo do molde vem de `estado+molde*0x10+0x1C` e vai para o registro de 0x10 B do molde em 0x020260D0 (campo +0x0C), pelos 0x080474F4 / 0x0804752E.
- A matriz e ObjAffineSet (SWI 0x0F) com alpha = -acc. O sprite gira pelo proprio centro. O registro do molde tem x/y trocados (+0x08 = deslocamento vertical) (G10).
- Cada molde e uma peca: raiz (molde 1) = corpo; pernas = moldes 20-24 no bot 43; cabeca e bracos tem moldes proprios (ver large_part_layout.md).

## 6. Anim_RunSlotScript 0x08040BD8 (herdado do R2.md; nao re-decompilado nesta rodada)
- E o interpretador de animacao de SPRITE por slot (registros de 3 bytes: duracao, quadro, salto, fim; 0xFB..0xFF = controle). Usado pelos slots de efeito e pecas de OBJ (slots 3, 4, 0x1E, 0x23 etc.).
- NAO e o molde: o bot grande usa MotionPlayStep; Anim_RunSlotScript cuida dos efeitos com animacao de quadros.

## 7. Pendencias (nao fechadas)
1. Roteiro do ataque: quem monta a sequencia de 0x08 por tecnica (linha `0x083CE164[sel]` e `gTechniqueActionScriptPtrTable` 0x083C6CA8) so foi lido por HO1/TQ; o kind exato por tecnica nao foi extraido.
2. Bot do lado 1: `fight+0x68` (suposicao, nao lido).
3. Byte `fight+0x67` (bot do lado 0): confirmado so como argumento de MotionPlayStep; a origem dele nao foi rastreada.
4. O interpolador percorre 63 estados, mas o gMotionState (0x020260D0) tem 50 vagas no mtbe; moldes sem linha tem alvo 0 (G9). Conferir o limite antes de usar outro roteiro.
5. `dur` e compartilhada por todos os moldes; o passo avanca quando QUALQUER molde chega (retorno > 0), nao quando todos chegam. Confirmado no codigo; nao testado em jogo nesta rodada.
6. A linha i da tabela = estado i = molde i+1 (raiz = estado 0), mas a numeracao 1-based do mtbe nao foi revisada.
