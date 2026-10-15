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

# Página "Actions in Medarot 2 Core" (2026-10-15)

Fonte: `ref/Actions in Medarot 2 Core - Medapedia.html` (fora do git). A página numera as 60 ações 1..60 numa
ordem própria (por grupo); **não** é o id interno. Id interno = byte +1 da peça = índice de `gTechniqueNames`
0x083BE868 (DM §7.1), e o tipo de status pendente (B+0xC9) usa o mesmo id da técnica que o causou.

| Id | Nome (EN) | Página # | Efeito segundo a página (paráfrase) | Nosso |
|---|---|---|---|---|
| 0 / 1 | Sword / Hammer | 1 / 2 | Strike sem efeito extra | — |
| 2 / 3 | Rifle / Chain Gun | 12 / 13 | Shoot sem efeito extra | — |
| 4 / 5 | Laser / Beam | 14 / 15 | Optic: Poder ×2 | **bate (emulador rec_27, OP.md)** |
| 6 / 7 | Missile / Napalm | 16 / 17 | explosivo, sempre acerta | bate (código, DM §6.3) |
| 8 / 9 | Break / Press | 18 / 19 | gravidade: **Acerto** ×2 (não "ROS listado") | bate (código: ComputeTotalRos soma o sucesso de novo) → emulador pendente |
| 10 / 11 / 12 | Grap Trap / Shot Trap / Attack Clr | 30 / 31 / 32 | armadilhas contra Strike·Berserk / Shoot·Aim / qualquer ataque | AD.md (armadilha com slot) |
| 13 / 14 | Bug / Virus | 3 / 4 | status "Flux" | SS3: Bug = penalidade de ROS, sem dano (fórum: "Movement") |
| 15 / 16 | Thunder / Freeze | 5 / 6 | status "Stop" | **resolve REF_FORUM**: Stop = 15/16 |
| 17 / 18 | Wave / Hold | 7 / 8 | status "Bind" | **resolve REF_FORUM**: Bind = 17/18; rec_28 mostra 18 e 17 no slot pendente |
| 19 / 20 | Fire / Melt | 9 / 10 | status "Burn" (fórum: "Flow") | SS3: Melt tira +0xCC por tick |
| 21 / 22 | Break Form / Stability | 41 / 40 | limpa armadilhas / status do time; **passivo**: imune a armadilhas / status | passivo: aberto |
| 23 | Force Bind | 49 | ninguém usa Medaforce | aberto |
| 24 / 25 / 26 | CancelOptic / CancelBomb / CancelGrav | 57 / 58 / 59 | passivo: anula dano de 4·5 / 6·7 / 8·9 | **novo** — achar o teste |
| 27 / 28 / 29 | Defence / Full Block / Half Block | 27 / 29 / 28 | guarda: Defence leva o golpe no lugar do aliado; Full anula tudo; Half anula golpes fracos | **novo** (guardas) |
| 30 / 31 / 32 / 33 | Recovery / AutoRecover / Repair / Reactivate | 36 / 39 / 37 / 38 | repara peça viva / regenera com o tempo / revive peça quebrada / reativa bot | AutoRecover +0xC4 (SS.md); resto aberto |
| 34 / 35 | Anti-Air / Anti-Sea | 20 / 21 | forte contra voador / aquático | bate (código, pernas 100/106) |
| 36 / 37 | Scout / Conceal | 23 / 24 | acerto do time / evasão do usuário | **bate (emulador, TR6)** |
| 38 / 39 / 40 | Boost Chrg / Rapid Chrg / Accel Chrg | 25 / 26 / 56 | reduz carga/resfriamento do aliado / zera a próxima carga / passivo: carga menor | aberto |
| 41 | Drain Chrg | 46 | aumenta a carga do inimigo e reduz a própria | SS.md: soma +0xCC ao medidor (P) |
| 42 | Pushover | 50 | inimigo esquece a próxima ação | aberto |
| 43 / 44 / 46 / 47 | Impair / Confusion / No Escape / No Defense | 42 / 43 / 44 / 45 | trava uma peça / ação e alvo aleatórios / não esquiva / não defende | SS.md (parcial) |
| 45 | Use Drain | 47 | tira usos da cabeça do inimigo e soma aos seus | aberto |
| 48 | Destroy | 11 | destrói a peça **só se o alvo estiver no resfriamento** | DS: hipótese do erro no ss2 → confirmar |
| 49 | Sacrifice | 22 | golpe forte que destrói a própria peça | **bate (emulador, DS)** |
| 50 | Forcedrain | 48 | tira MF do inimigo e soma ao seu | aberto |
| 51 / 52 | Team Form / TeamAttack | 33 / 34 | prepara / dispara o ataque cruzado | bate (código: 52 + ramo 51 soma F.rec+0x2C/+0x2D) |
| 53 | Strengthen | 60 | passivo: **Poder e Acerto** das outras peças do usuário | código só mostra +Poder (0x08035B9C) → conferir Acerto |
| 54 | CntrAttk | 35 | protege e devolve o dano, só acima de um limiar | **novo** |
| 55..59 | Change / ItrpChange / AttkChange / DEF Change / HealChange | 51 / 55 / 52 / 53 / 54 | vira outra peça aleatória (do grupo) e age | aberto |

## O que muda no plano
- REF_FORUM "Bind/Stop aberto" → resolvido por nome (Stop = 15/16, Bind = 17/18). Falta o efeito no jogo: rec_28.
- Gravity dobra o **Acerto** da peça (não o ROS total): conferir com uma gravação de Break/Press.
- Novos alvos: CancelOptic/Bomb/Grav, guardas (27/28/29), Counter (54), Strengthen somando Acerto.
