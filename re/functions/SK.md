# SK: subida de habilidade da medalha e Destroy (2026-10-15)

## Habilidade por uso (`Fight_AwardSpecialitySkillUse` 0x0801BE48, decompile; antes "Fight_StepBotPartTimer")
- Só bots do jogador (0..2). Não roda se fi+0x89 == 1, se fi+0x164 == 0, ou se o **nível da medalha (bot+5) é 100**.
- Especialidade da ação: byte +2 da peça usada (bot+0xDD = 1..3 → peça k-1) ou bot+0xA9 (Medaforce/outros).
- bot+0x28+esp = contador de usos: +1 por ação; ao chegar a **8** zera e a habilidade bot+0x20+esp sobe 1;
  fi+0x166 = especialidade que subiu (FI.md tinha como "não identificado") e uma flag de aviso é ligada.
- Habilidade em 100: o contador para em 7 (não sobe mais).
- Bate com o guia Autocon (§2.4: 8 usos por ponto, teto 100) e explica a queixa do FAQ (Q5): com a medalha no
  nível 100 nenhuma habilidade sobe mais, mesmo baixa. É o `bot+5 == 100` no começo da função.

## Destroy (48): só funciona em alvo de costas (resfriamento)
- Observado pelo usuário numa das gravações de Destroy: o jogo mostra a mensagem de que o golpe só funciona em
  Medabots virados de costas (fase de resfriamento/RAD). Bate com a Medapedia (Actions in Medarot 2 Core) e com o
  fórum. Explica o erro no ss2 (DS.md). Falta ligar a mensagem ao código do teste (fi+0xA6).

## Outras coisas do guia (§4, §5) que não estavam no REF_FAQ
- Link: 5 opções (sem aposta, com aposta de peça, troca de peça, dar medalha, salvar); exige >= 5 peças, um
  Medabot completo e peças não equipadas; não dá para dar medalha de um tipo que o outro já tem; só salva se os
  dois confirmam. Liga com LK4 (passo 72 da tela LINK).
- Tinpets: 10 no jogo (6 masc., 4 fem.), só 9 cabem (bate com os 9 registros em 0x030017A0, DB2).
- "Chain Reaction" (o dano que sobra passa para outra peça; Medapedia chama de Pierce): Hammer e Missile têm,
  Chain Gun não. Pista para achar o flag: comparar essas técnicas/peças no gPartStats ou numa tabela por técnica;
  fi+0xCA (leftover_damage) é o candidato do lado da batalha.
