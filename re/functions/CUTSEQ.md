# CUTSEQ: qual tipo de motion o atacante toca no corte (2026-10-18)

Status: REGRA FECHADA PARA O TIPO (confiança alta no dado, média na ligação lado→slot); TEMPO POR FASE ABERTO.

## 1. Correção: a "sequência de 7 tipos" era leitura errada
`pc/extract/extract_motion.py` lia `cnt = argc[op]` = 7 halfwords a partir de cursor+2, o que pegava a1..a6 e o
próximo opcode (0x0C, valor 12). Por isso `tech[sel][1] = {13,14,15,13,15,14,12}` (Missile). Não é linha do tempo.

## 2. Formato real da linha da técnica
- Linha = `0x083CE164[sel]` (sel = técnica da peça, `fight+0xF9`). Stream de halfwords: opcode, depois `argc[op]`
  argumentos (`argc` em 0x083CE374). Medido na ROM para sel 0, 1, 2, 6.
- Sel 6 (Missile): `08 | a0=0 | a1..a3 = 13,14,15 | a4..a6 = 13,15,14 | 0C 00 | 0D ...`.
  Sel 0: a1..a3 = 0,1,2; a4..a6 = 0,2,1. Sel 1: 9,10,11 / 9,11,10. Sel 2 = sel 6.
- Opcode 0x08 = `ObjectOpcode08_SetMotionFromTable` 0x080395E8 (argc 7; AN.md, HO1.md).

## 3. Regra (tipo do golpe)
- k = peça que age (0..2; `kind` no battle.lua, `fight+0xA7`). Perna (3) não ataca por técnica.
- Lado 1 do atacante (`fight+0xF8 & 2 == 0`, usado no jogo como humano/jogador) -> slot 1 -> tipo = a[1+k].
- Lado 2 -> slot 2 -> tipo = a[4+k].
- Um tipo por golpe, sem sequência no tempo. Gravado em slot+0x2C; `FightPreview_ApplyObjectMotionKind`
  0x0803BBE0 copia para `fight+0x112`; `MotionPlayStep(side, kind, bot)` 0x08044A3C toca o roteiro
  `gMoveScriptTable[kind*7 + gMoveStyle[bot]]` (estilo por bot, `motion.lua` `style`).
- Estado do molde não zera entre tipos (AN §2). Ao acabar, a última pose fica.

## 4. Medições que sustentam
- B16 (rec_2, Metabee bot 43, Missile, peça 0): slot 1 = 13 de f1445 a f1565, sem trocar. A regra dá 13.
- Porte LÖVE (após a correção), `--player` com Missile da peça 0: Metabee bot 43 (estilo 0, tec 6) -> tipo 13;
  Noctobat bot 0 (estilo 2, tec 34) -> tipo 13; Magdosnake bot 30 (estilo 5, tec 14) -> tipo 9.
  Prints em `build/scratch/cutseq/` (bot43.png mostra os mísseis saindo do corpo). Log `[cutseq]`.

## 5. Abertos
1. Lado 2 (atacante inimigo, slot 2) não foi testado no porte; regra vem só do ROM/decompile.
2. Peça 3 (perna) e 0x08 com a0 != 0: nenhum caso visto nas linhas medidas.
3. Fases (preparo, golpe, volta) e duração: a linha tem 0x0C (ROS), 0x0D e 0x0F (args 0,80,70,0,1), não
   confirmado se é tempo. A duração da fase attack continua em CUT_ATTACK_FRAMES (aprox., B14/B15).
4. Velocidade: gObjectSlots de quem espera o fim do tipo (bit 0x80 de `fight+0x112`) não medido no corte.
5. Ghidra não foi reaberto nesta rodada: a regra usa o decompile já anotado em AN.md e HO1.md.

## 6. Arquivos
- Extrator: `pc/extract/extract_motion.py` (novo bloco `tech_parts`; `pc/cache/motion.lua` regenerado da ROM).
- Porte: `pc/game/src/battle.lua` função `startAttackMotion` (print `[cutseq]`).
- Símbolos: `CUTSEQ_simbolos.csv`.
