# VAL: validação do calc.lua contra o jogo (rec_27, 2026-10-17)

Gravação: `data/records/rec_27` (Tortoise, Plasma Laser; Medabots sem flags). Replay com `--break` (ROS 0x0803579C,
esquiva 0x08038F84, defesa 0x08039068, dano 0x08033D30) e `--dump` de 0x03001800:0x100 e 0x03000590:0x600 em f+1, f+4, f+9.
Teste: `pc/game/tests/test_val.lua` (OK 13). Casos com o jogo: A (f3372), B/B2 (f1507), C (f2573).

## Golpe × termo × jogo × calc

| Golpe | Termo | Jogo | calc | Resultado |
|---|---|---|---|---|
| A f3373 | ROS total (E8) | 100 | 100 | OK |
| A | compat do atacante (4 peças, attr=2 = bot+4) | 4×7=28 (fonte: decompile 0x0803576C) | 28 | OK |
| A | esquiva EE (−8, evasão 12, nível 20, rand 3) | 27 (E8 100→73) | 27 | OK |
| A f3376 | defesa de perna EE (nível 20 + compat 0 + rand 3 + defesa 48 − 8) | 63 (+0xEE), flag 0x20 | 63 | OK |
| A | E8 final / redirecionamento | 10 | 10 | OK |
| B f1508 | ROS total (rand 4) | 102 | 102 | OK |
| B f1510→1511 | esquiva com flags 0x02 (bit 0x08 surge em f1511 sem pending) | E8 63, EE 0, flags 0x0A | E8 8 | DIVERGE (flags) |
| B2 (flags 0x08) | esquiva EE 39 (sem −8), defesa bypass | E8 63, EE 0 | E8 63 | OK |
| C f2574 | ROS (Press: +suc 2×, 20 hab, dist 12, compat 0, rand 7) | 87 | 87 | OK |
| C f2577 | esquiva EE (49+28+rand 1+42−8) | 112, E8 0, flag 0x10 | 112, evadido | OK |

## Compat (0x0803576C): soma por peça, confirmado
Decompile (Ghidra, `Fight_GetMedalPartCompatibilityBonus`): loop de 4 peças; para cada peça com `attributeId` igual ao
`bot+4`, soma `bot+7`. Ou seja, **soma por peça** (nº de peças com o atributo × bônus). `calc.M.compat` faz isso.
A dúvida do G2_hit.md §89 ("provável artefato, lê bot+7 constante") está **refutada**: o bônus é constante, mas a
contagem é por peça (bot 0 com 4 peças de attr 2 → 28; bot 3 com attr 26 → 0).

## Defesa de perna (EE > 31 em quase todo golpe)
Decompile 0x08039068 + 0x08035844: EE = nível + compat + rand&7 + `parts[3]+0xA4` (defesa) − 8 (se flag 0x04 == 0);
dispara se EE > 31 e não é crítico. O jogo faz exatamente isso: f3376 EE = 63 com defesa 48. Logo o "disparo quase
sempre" é do jogo para bots com perna de defesa ~40–50, não erro de porte. O calc só diverge por flags (abaixo).

## Divergência aberta (B)
- O alvo ganha bit 0x08 em B+0xBC entre f1508 (0x02) e f1511 (0x0A) sem efeito pendente (B+0xC9..0xCC = 0).
  Sem ele, a esquiva aplica −8 (EE 31 → E8 71) e a defesa dispara (diverge). Com 0x08 o calc bate.
- Quem seta 0x08 nessa janela (esquiva ou passo do roteiro) não foi achado. Próximo passo: watch de escrita em
  0x0300094C (B+0xBC do bot 3, LEN 4 alinhado) entre f1508 e f1511.
- Bit 0x02 cru (B+0xBC) não tem efeito no calc; no jogo também não parece ter (sem 0x04 nem 0x08 aparecer nele).

## Nota de endereço
Bot i: base 0x03000590 + i*0x100 (B+0xBC do bot 3 = 0x0300094C).

## Em aberto
- rec_28 e rec_29 não foram medidos (só rec_27, 3 golpes).
- Ordem exata de esquiva/defesa dentro do quadro f1510 (ambas com entrada no mesmo quadro).
