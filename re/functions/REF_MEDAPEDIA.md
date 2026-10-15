# REF_MEDAPEDIA: página "Stats" da Medapedia × achados (2026-10-15)

Fonte: cópia local `D:/claude-brain/Medabots/ref/Stats - Medapedia.html` (fora do git). Paráfrase apenas.
Nosso jogo (Medabots Metabee/Rokusho GBA) é a versão inglesa de **Medarot 2 Core** (a própria página cita a
"versão inglesa" do 2 Core). Medarot Navi é outro jogo (batalha em grade, AP); a página "Stats in Medarot Navi"
só lista terrenos dele e **não** vale para nós.

| Página (paráfrase) | Nosso | Status |
|---|---|---|
| 2 Core: a medalha mira peças de uma especialidade ("Target"/"Aim" na tela) | gMedalRecord_Aim (MedalRecord +0x06, mtbe) | bate (nome); ligar ao alvo da IA (AI.md) |
| Atributo da medalha + "compatibilidade": peça do mesmo atributo soma ao acerto de todas as ações | compat em ROS e EE (DM) | **bate** |
| Nível da medalha: acerto e dano base; decide quando aprende Medaforce (1 a 3 por medalha) | nível em EE (DM §6.3); MF por nível não visto | aberto (tabela de MF por nível) |
| Habilidades sobem usando peças daquela especialidade | +0x20..0x27 (XP.md) | bate (código) |
| 8 especialidades do 2 Core na ordem Strike, Berserk, Shoot, Aim Shot, Defend, Heal, Support, Interrupt | especialidade 0..7: 1 soma Propulsão/2 (Berserk), 0/1 usam proximidade (+0xD3), 2/3 distância (+0xD4); rec_27: Laser = especialidade 2 com a tela mostrando "Shoot" | **bate** — ordem 0..7 confirmada em 0, 1 e 2 |
| Cabeça/braço: ação, especialidade, atributo, armadura, poder, acerto (ROS, também chance de crítico), carga, resfriamento (Radiation), usos (só cabeça) | BattlePart +0 atributo, +1 técnica, +2 especialidade, +4 armadura, +5 acerto, +6 poder, +7 carga, +8 usos | bate; **resfriamento ainda sem offset** (candidato: +9, "proximityOrCooldown") |
| "Pierce": o efeito passa para outras peças quando sobra dano (símbolo ao lado do Poder) | fi+0xCA "leftover_damage" (mtbe) | **novo** — achar o flag de pierce na peça e quem usa fi+0xCA |
| Pernas: atributo, tipo de perna (terreno), armadura, propulsão (velocidade e golpes pesados), evasão, defesa, proximidade, distância | perna k=3: +0x12 propulsão, +0x13 evasão, +0x14 defesa, +0x15 proximidade, +0x16 distância (B+0xA2..A6) | **bate** |
| Scan/Conceal como stat de perna | só em jogos posteriores; aqui Scout/Conceal são ações (TR6) | n/a |

## Pendências que saem daqui
- Offset do resfriamento (Radiation) na peça: conferir +9 no PartStats/BattlePart.
- Pierce: flag na peça + uso de fi+0xCA.
- Medaforce aprendida por nível: tabela.
- Página mais útil para salvar: "Stats in Medarot 2 Core / Shingata" (mesma mecânica do nosso jogo); já temos
  "Actions in Medarot 2 Core" e "Medals in Medarot 2 Core" em `ref/` (ainda não lidas).
