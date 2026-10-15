# OP: Optic (Laser/Beam) dobra o Poder (emulador rec_27, 2026-10-15)

Gravação rec_27: Tortoise (bot 0) ataca com o braço 1 (técnica 4 = Laser, especialidade 2, Poder 20 = byte +6 da
peça, B+0x4E+0x18k) o bot 3. Replay com watch em fi+0xEC (0x030018EC) e dump do bot e do fight_information no
quadro 2150, antes do dreno da armadura (2160..2188, peça 30 → 0).

| campo | valor |
|---|---|
| fi+0x87 atacante / +0x88 alvo / +0xA7 peça | 0 / 3 / 1 |
| fi+0xEA (ROS/4 com reduções, DM §2) | 15 |
| Poder da peça | 20 |
| B+0xD0 / +0xD3 / +0xD4 / +0xD6 | 0 / 0 / 0 / 0 |
| fi+0xEC dano final | **55** = 15 + 20 + 20 |

`Fight_AccumulateBaseDamage` 0x08033D30: soma `P.poder` uma segunda vez quando o byte +1 da peça (técnica) é 4 ou
5. Confirmado no emulador: Optic = Poder ×2 (o fórum dizia o mesmo, REF_FORUM "Optic dobra o Poder" → **bate**).
O mesmo trecho soma o Poder de novo quando `B+0xD6 & 1` (flag do atacante; ver DM §6) e soma +0xD3 (especialidade
0/1), +0xD4 (especialidade 2/3) e Propulsão/2 (especialidade 1). Escritores de fi+0xEC no quadro 1510 (outra ação):
0x08038E62, 0x08038F96, 0x0803585A..72, 0x08038FC8 (cadeia de redução de ROS, DM §6).

Ainda aberto: Gravity (Break/Press, técnicas 8/9) dobrar o ROS listado.
