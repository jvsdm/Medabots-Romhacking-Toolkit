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

## Slots por medalha (ROM, 0x083B7F8C, 3 B por medalha; confirmado: lido por `extract_battle_tables.py`, chave `medaforce`)

Slot 0 libera com nivel >= 10, slot 1 com >= 30, slot 2 com >= 60 (MF.md §2, XP.md §4). Id em hexadecimal.

| medalha | slot0 | slot1 | slot2 | | medalha | slot0 | slot1 | slot2 |
|---|---|---|---|---|---|---|---|---|
| 0 | 00 | 01 | 23 | | 15 | 1e | 1f | 33 |
| 1 | 02 | 03 | 0b | | 16 | 20 | 21 | 2d |
| 2 | 04 | 05 | 15 | | 17 | 22 | 23 | 2b |
| 3 | 06 | 07 | 25 | | 18 | 24 | 25 | 03 |
| 4 | 08 | 09 | 2f | | 19 | 26 | 27 | 11 |
| 5 | 0a | 0b | 01 | | 20 | 28 | 29 | 13 |
| 6 | 0c | 0d | 1d | | 21 | 2a | 2b | 17 |
| 7 | 0e | 0f | 1f | | 22 | 2c | 2d | 19 |
| 8 | 10 | 11 | 27 | | 23 | 2e | 2f | 05 |
| 9 | 12 | 13 | 31 | | 24 | 30 | 31 | 21 |
| 10 | 14 | 15 | 29 | | 25 | 32 | 33 | 0d |
| 11 | 16 | 17 | 1b | | 26 | 1a | 0a | 32 |
| 12 | 18 | 19 | 0f | | 27 | 1c | 2a | 30 |
| 13 | 1a | 1b | 09 | | 28 | 00 | 2c | 0e |
| 14 | 1c | 1d | 07 | | 29 | 02 | 28 | 14 |

Obs.: medalhas 0..25 seguem o padrao (slot0 = 2m, slot1 = 2m+1); 26..29 nao. Os 52 ids (0x00..0x33) aparecem.

## Descritor (desc6 0x083B7E54, 6 B por id): campos confirmados no porte

- b0 = id; b1 = especialidade (B+0xA9, indice de B+0x20); b3 = base da rolagem (B+0xAB); b4 = bonus fixo de efeito (B+0xAC).
- b2 (B+0xAA) e b5 (B+0xAD): ABERTO (sem leitor achado; MD2.md §4). Nao usados em `medaforce.lua`.
- Rolagem (MD2.md §1): `E8 = b3 + compat + (rand&7) + nivel(esp b1)`; `EC = max(1, (E8>>3) + b4)`.

## Efeito por id no porte (`pc/game/src/fight/medaforce.lua`, tabela EFFECT)

Leitura so da cadeia de opcodes (MF2.md §2) e do nome dos handlers (AS2.md). Ghidra MCP estava fora do ar
(porta 8080 recusou conexao) nesta rodada: os handlers 0x5B..0x6A e os de efeito de 0x41..0x4F NAO foram decompilados.

| tipo no porte | ids | efeito | confianca |
|---|---|---|---|
| damage | 01 | rolagem + dano em peca aleatoria de um oponente vivo (AR_00, 0x50); EC vai a `calc.applyDamage` | confirmado (opcode); escolha de peca ABERTO |
| damage (ABERTO) | 02, 03, 14, 1b, 2b | alvo (0x41) + rolagem + soma/armadura (0x43, 0x46, 0x47, 0x49, 0x4C) | provavel; soma nao portada |
| self_state | 04..09 | estado por tecnica no proprio bot (0x51, AS) | confirmado (HN2); tipo nao lido |
| self_reaction | 0b | marca de reacao no proprio bot (0x53) | confirmado (HN2) |
| opp_state | 0c, 0e, 10, 12 | rolagem + estado em todos os adversarios (0x54) | confirmado (HN2); tipo nao lido |
| team_protect | 0d, 0f, 11, 13 | protecao contra status do time (0x55) | provavel (AW) |
| team_heal | 15 | cura do time (0x56) | provavel (AX) |
| team_gauge | 16 | rolagem + gauge do time (0x59) | provavel (BA) |
| technique_nodamage | 17 | tecnica sem dano (0x5A) | provavel (BB) |
| team_repair | 1a | rolagem + reparo de armadura do time (0x57) | provavel (AY) |
| team_restore | 1c | rolagem + restaura pecas do time (0x58) | provavel (AZ) |
| team_support | 0a | rolagem + bonus de suporte do time (0x52) | confirmado (AT) |
| unported | 00, 18, 19, 1d, 1e..31, 32, 33 | ver ops na tabela de MF2 §2 (0x06..0x21, 0x4A, 0x4D..0x4F, 0x5B..0x6A) | pendente |

Gasto do medidor no porte: `min(fight+0x51, B+0xC1)` com 0x28 (ou 0x50 com B+0xC0&8), MG3.md BR_26/BR_46. Provavel, nao
testado no emulador.

## Pendente para a proxima rodada (com Ghidra no ar)

1. Decompile de BC..BR (0x0802AAD0..0x0802FE50) para os ids 18, 19, 1e..31, 33 (ops 0x5B..0x6A).
2. Tipo de status de 04..09 (0x51) e de 0c..13 (0x54): ler a tabela de tipo em AS/HN2.
3. Os campos b2 e b5 do descritor (MD2 §4): o leitor deve estar nos handlers acima.
4. Confirmar em emulador: consumo do medidor (0x28 x 0x50) e se toda medaforca passa por BR_26.
