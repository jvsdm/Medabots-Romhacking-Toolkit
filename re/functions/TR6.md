# TR6: Scout e Conceal medidos no emulador (2026-10-14)

Fonte: `data/records/rec_8` (gravação do usuário: batalha com 3 bots que só usam Heal/Scout/Conceal, autobattle
via ←+A em cada bot). Valores tirados do dump de IWRAM a cada 4 quadros; escritor por watch de 4 bytes no replay.
Confiança: **confirmado**.

| Efeito | Onde fica | Escritor (pc) | Chamado por | Soma | Teto |
|---|---|---|---|---|---|
| Conceal | bot+0xB9 do usuário (bot 2 = 0x03000849) | Util_AddByteClampToMax 0x08035740 (pc 0x08035752) | Fight_DispatchOnSubstateAA_AA_0A (0x080236E0, lr 0x08023701) | 0x25 na 1ª vez | 0x78 = 120 |
| Scout | fight+0x28+8·lado (lado 0 = jogador: 0x03001828) | Util_AddByteClampToMax (pc 0x08035752) | Fight_DispatchOnSubstateAA_Z_0E (0x080233E0, lr 0x080233FF) | 0x25 na 1ª vez | 0xA0 = 160 |

- Ambos **acumulam** a cada uso e **não são zerados** durante a batalha. Conceal do bot 2: 0→37→64→102→120 (teto).
  Scout do lado 0: 0→37→63→89→114→151→160 (teto). Os incrementos variam (~18..38), então vêm de um valor da peça.
- Conceal é lido pelo defensor em Fight_ComputeTotalRos (subtrai do ROS do atacante); Scout soma ao ROS do lado (TR2).
- Por que a análise estática não achou (TR3..TR5): olhavam o passo AA_0B (só mensagem 0x1D) e o Z_0F; o escritor é
  o passo ANTERIOR de cada despachante (AA_0A e Z_0E), chamado com ponteiro já calculado para o helper de clamp.
- Medaforces 0x22/0x24 (AS6) usam os mesmos despachantes.

Aberto: de onde vem o valor somado (campo da peça) e se algo zera no fim da batalha (Fight_ResetBotPartState zera +0xB9 na derrota).
