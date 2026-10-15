# VR: veículo (Landmotor), roupas do Ikki e recompensa — gravações do usuário (2026-10-15)

Fontes: `rec_20`..`rec_25` (usuário), dumps de IWRAM a cada 4 quadros. E = 0x030057E0. Confiança: **confirmado**
(valores medidos), salvo onde indicado.

## Modo de andar E+0xC5C (com C69 = anterior e C28 = gráfico; VA2/VA3)
| Valor | O que é | Como entra |
|---|---|---|
| 0 | Ikki normal | sair da roupa (item de novo) |
| 1 | **veículo (Landmotor)** | menu de campo → VEÍCULO (menu 1/2) — rec_22/23 |
| 3, 4, 5, 8, 6, 7, 13 | **roupas** do Ikki | itens da bolsa (main 2 / menu 4), na ordem da rec_25; bate com o IN3 (id 1→3, 5→4, 12→5, 13→8, 19→6, 39→7, 40→0x0D) |
| 0x0B / 0x0C | cenas de UI | (VA3) |
- Sequência ao trocar: C5C muda no menu da bolsa; C69 e C28 copiam 1-2 quadros depois, ao voltar para o mapa.
- Segundo o usuário, as roupas servem em pontos da história e ficam livres depois de zerar.

## Landmotor sem encontros (rec_22/23)
- Com C5C = 1 o contador de passos E+0xC2B **não sobe**, mesmo em mapa com encontro ligado (rec_23: C29 = 1, contador
  parado em 1 durante todo o passeio). É o gate "C5C != 1" de Overworld_WalkStepByDirection (EV4). Com roupa (C5C = 4,
  rec_25) o contador sobe normal (1..8).

## Recompensa (rec_20/21 + usuário)
- Vencer dá **peças**, não dinheiro; dinheiro só vendendo peças na loja (usuário). Medido: dinheiro 0x03002B70 fixo
  (30680) nas gravações.
- **Peça ganha (rec_26, emulador):** na tela de resultado (menu 6), f4193: `Inv_AddCountClamp99` 0x0804213C
  (pc 0x0804216C) soma 1 na quantidade da peça (tipo 1, bot 61: 4→5; endereço 0x03004C40 + tipo*0xF0 + bot*2),
  chamado por `Fight_HandleRewardGrantOrSkipByPostBattleFlags` 0x08010C50 (lr 0x08010CCF). Teto 99.
- **Venda na loja (rec_26, emulador):** f6354, `Shop_CommitBuySellTransaction` 0x08062614 grava o dinheiro
  (pc 0x080626AE: 30680 → 32130 = +1450, ou seja $14,50 guardado em centavos) e o inventário (pc 0x080626EC; a peça
  vendida tipo 1 bot 44: 3→2). Teto do dinheiro comparado com 0x1869E/0x1869F (99999).
- rec_20/21 não pegaram o momento (a peça já tinha sido creditada antes do início).
