# MF5: ganho do medidor de Medaforce medido no emulador (2026-10-15)

Fontes: `rec_18` (usuário: escolhe Charge, leva um golpe antes de agir, executa o Charge, volta com 60) e `rec_19`
(usuário: só leva um golpe, +20). Medidor = bot+0xC1 (0x03000651 para o bot 0), teto 80 (0x50). Watch de 4 bytes
no replay. Confiança: **confirmado**. Fecha a divergência do REF_FAQ (o FAQ estava certo).

| Evento | Ganho | Quem escreve | Quando |
|---|---|---|---|
| Ser **alvo** de um ataque | +20 (+40 se B+0xC0 & 8) | `Fight_AddMedaforceOnTargeted` 0x0803628C (pc 0x080362BC), chamado por H_01 (lr 0x0801E42B) | na declaração do golpe, antes da animação de dano (rec_19: bot 3 ganha em f1704, apanha em f1840) |
| Executar **Charge MF** | +40, enchendo +1 por quadro | `Fight_StepMedaforceGaugeFill` 0x0801AF6C (pc 0x0801AF92), alvo em fi+0x51 | rec_18 f1300-1340: 20 → 60 |
| Teto | 80 | o enchimento para em 80 (rec_18 f2256-2276: 60 → 80) | — |

- Levar dano em si não dá MF: o ganho é do alvo declarado (o golpe pode ainda errar — a conferir).
- Correção: o nome/comentário antigo de 0x0803628C dizia "para o bot que agiu, após ação bem-sucedida"; é o alvo.
