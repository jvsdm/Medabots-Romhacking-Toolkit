# CR: Chain Reaction (2026-10-15)

Byte +7 do registro da peça (gPartStats 0x083B83FC, 16 B; cópia no bot em B+0x4F+0x18k) = **Chain Reaction**
(booleano). O editor WPF do Toolkit (`PartDefinition.ChainReactionOrDefense`, checkbox "Chain Reaction") já tratava
assim; nas pernas o mesmo byte é Defesa. Nossos docs chamavam +7 de "carga": errado. Pelo editor, +8 = usos
(cabeça) / carga (braço) / proximidade (perna) e +9 = radiação (braço) / distância (perna).

ROM (480 registros, sem pernas): Missile 13/13 e Napalm 8/8 com 1; Chain Gun 0/12 e Rifle 0/13; Hammer 12/19,
Sword 9/21; Destroy 6/6, Sacrifice 6/6, TeamAttack 4/4, Anti-Air/Sea todos. Metabee (bot 43): cabeça Missile
+7 = 1, +8 = 4 usos (o guia diz 4 usos). Bate com o guia (Hammer/Missile têm, Chain Gun não).

Código (decompile):
- `..._H_1D_ChainReactionOrAdvance` 0x0801EC3C: depois do dano, se a peça do atacante tem +7 (ou bot+0xD6&2) e
  sobrou dano (fi+0xCA, "leftover_damage") → volta ao passo 0x20 e aplica o resto em outra peça do alvo.
- `..._R_0C_ChainRepairToBrokenPart` 0x08020AD0: no reparo, +7 e sobra → escolhe uma peça quebrada e reparte o
  resto: é o "repara uma ou mais peças" da Medapedia (Recovery 1/5 e Repair 2/5 têm o flag).
- Técnica 48 (Destroy) no mesmo H_1D: conta fi+0xEC até zerar.

Pendente: quem calcula fi+0xCA (sobra = dano − armadura restante) e bot+0xD6 bit 2 (outra fonte de chain).
