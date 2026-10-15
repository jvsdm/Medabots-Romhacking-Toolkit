# BB: Bind (Wave/Hold) e Bug no emulador (rec_28, rec_29; 2026-10-15)

`Fight_SetPendingPartStatus` 0x08036E64(bot, tipo, duração = fi+0xE8, valor = fi+0xEC), chamada pelo golpe normal
(`..._I_07_ApplyPartStateEffectAtXY` 0x0801FA24: tipo = técnica da peça) e pelas versões de Medaforce para o time
(`AV_0D/10/12/13/2C`, laço nos 3 bots do lado oposto). fi+0xE8 = ROS final, fi+0xEC = dano do golpe.

| Tipo | Duração +0xCB | Valor +0xCC |
|---|---|---|
| 13/14 Bug/Virus | ROS (1..99) | dano × 2 |
| 15/16 Thunder/Freeze | dano / 8 | 0 |
| 17/18 Wave/Hold (Bind) | ROS | dano / 4 |
| 19/20 Fire/Melt (Burn) | ROS | dano / 2 (= "Flow é metade do golpe" do fórum) |
| outros | ROS | valor do golpe |

Emulador:
- rec_28: Hold no bot 3, dano 43 (65→22), +0xCC = 10 = 43>>2; Wave, dano 20 (35→15), +0xCC = 5. Dump no quadro
  1174: fi+0xEC = 43, fi+0xEE (redução do defensor) = 63, **fi+0xE8 = 0** → duração 1 → o status some no tick
  seguinte (84 quadros depois).
- rec_29: Bug 3x no bot 3, dano 12 cada (65→53→41→29), +0xCC = 24 = 12×2; duração 1 nas três.
- Conclusão: a duração é o ROS que sobrou depois da evasão/defesa. Quando a defesa de perna zera o ROS o status
  dura 1 tick: é a regra do fórum "debuff de golpe com dano acaba muito rápido se a defesa de perna ativar".
- Bind: o tick soma +0xCC a um medidor do bot (`Fight_TickPendingEffectMeterGain`, tipos 0x11/0x12/0x29, SS.md):
  é o atraso de CRG/RAD. bot+0xBB mudou durante o status (25→31, 25→30, 23→16): candidato ao medidor; a confirmar.
