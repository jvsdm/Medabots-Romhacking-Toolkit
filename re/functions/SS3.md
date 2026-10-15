# SS3: status e encontro medidos no emulador (2026-10-14)

Fontes: gravações do usuário `rec_9` (Melt), `rec_10` (Bug), `rec_11` (andando até cair em encontros), dumps de IWRAM
a cada 4 quadros + watch de escrita no replay. Confiança: **confirmado**. Bloco de efeito pendente: bot+0xC9 tipo,
+0xCA estágio, +0xCB duração, +0xCC valor (SS.md). Armadura atual da peça k = bot+0x59+0x18k.

## Melt (id 20) — dano por turno ("veneno")
- Aplicado por Fight_SetPendingPartStatus 0x08036E64 (pc 0x08036F3A), chamado de I_07 0x0801F9EC.
- Tick: Fight_TickPendingEffectDurationAndEmitResult 0x08018DF4 → Fight_ApplyPendingEffectDamageToPart 0x08018FF0
  (escrita da armadura em pc 0x08019016). Um tick por vez do bot; duração cai 4 por tick (44→40→36→32).
- Dano por tick = +0xCC (17 na rec_9). **Não quebra peça**: deixa a peça com 1 e passa o resto para a próxima peça
  com armadura (ex.: braço 13→1 e cabeça 35→30 no mesmo tick = 12+5 = 17). Portanto o Melt sozinho não derruba o bot.
- Reaplicar com duração 1 some no tick seguinte (f1432/f6068). Estágio +0xCA varia (2..4) por aplicação.

## Bug (id 13)
- Valor +0xCC grande (86/88), duração cai 4 por tick (37→33→29→25→21), **sem dano por tick** (as quedas de armadura
  na rec_10 são golpes). Efeito = penalidade de ROS (Fight_ApplyVirusBugRosPenalty 0x08035814, DM.md).

## Encontro aleatório (rec_11, 8 encontros)
- Contador E+0xC2B (0x0300640B) +1 por passo; encontros com o contador em 32, 32, 23, 20, 32, 20, 32, 32: nunca
  antes de 17, sempre no 32 → bate com "> 16 e (> 31 ou rand&31 == 31)" (EV4.md).
- Ao voltar da batalha (fugir) o contador recomeça em 4, não em 0.
