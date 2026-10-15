# REF_FORUM: tópico "Explanation of game stats/mechanics" (GameFAQs) × achados (2026-10-15)

Fonte: tópico de fórum (testes de jogadores, ~2021), cópia local em `D:/claude-brain/Medabots/ref/gamefaqs/` (fora do
git). Paráfrase apenas. Status: **bate** = já temos no código/emulador; **diverge**; **novo** = não tínhamos; **aberto**.

## Dano, acerto e crítico (DM.md §2-6)
| Fórum (paráfrase) | Nosso | Status |
|---|---|---|
| Dano = Poder + (ROS do atacante − Evasão − Defesa se a defesa de perna ativou) / 4, com um fator aleatório pequeno só na parte bônus; nunca abaixo do Poder | `EA = E8 >> 2` (ROS/4) e `d = EA + P.poder`; E8 já leva `rand & 7` e as reduções de esquiva/defesa (DM §2, §6) | **bate** |
| Evasão/Defesa do defensor = perna + nível da medalha + bônus de compatibilidade | `EE = nível(+5) + compat + rand&7 + perna(+0xA3/+0xA4)` (DM §6.3/6.5) | **bate** |
| Crítico não é aleatório: ROS muito acima de Evasão+Defesa → crítico; pula esquiva e defesa de perna | `Fight_TryApplyCriticalRosOverride` por limiar; crítico retorna antes da esquiva (DM §6.4) | **bate** |
| Tipo de ataque muda a facilidade de crítico (Aim Shot alto, Shoot baixo) | multiplicador do limiar ×4 / ×2 / ×8 por especialidade (DM §6.4) | **bate** (mapear qual esp. é qual) |
| Defesa de perna ("Part Defense") manda o dano para a peça de maior armadura, exceto a cabeça | `Fight_PickHighestArmorNonHeadPart1Based` (DM §6.5) | **bate** |
| Mísseis/Napalm ("Bomb") não podem ser esquivados e encadeiam | técnicas 6/7 fora da regra de erro por ROS zero (DM §6.3) | **bate** (encadeamento: aberto) |
| Berserk soma metade da Propulsão ao Poder | especialidade 1: `d += propulsão >> 1` (DM §3) | **bate** |
| Optic (Laser/Beam) dobra o Poder; Gravity (Break/Press) dobra o ROS listado | técnicas 4/5 e 8/9 citadas na tabela (DM) | **aberto** — conferir no código |
| Depois de Strike/Berserk o bot não ativa defesa de perna até agir de outro jeito; Aim Shot reduz a evasão no turno de recuo | flags do atacante +0xD6 (DM) | **aberto** |

## Velocidade e pernas
| Fórum | Nosso | Status |
|---|---|---|
| Carga/Radiação = tempo de preparo/recuo do braço; cabeça vale sempre 6 | +7 carga (DM); radiação não achada | **novo/aberto** |
| Velocidade depende de carga/radiação, tipo de perna × terreno, nível da medalha e (pouco) propulsão | não documentado | **novo** |
| Tabela terreno × tipo de perna (Plain/Woods favorece duas pernas; Rock/Valley multi-pernas; Street/Inside rodas; Desert/Wilderness voadores; Seashore/Underwater aquáticos; Tank neutro; Float sempre um pouco bom; versão "indoors" com efeito maior) | tipo de perna 100..106 (DM); campo de batalha por mapa não ligado | **novo** — achar tabela na ROM |
| Perna quebrada: atributos pela metade e mais lento | não documentado | **novo** |
| Armadilhas (formações) perdem HP pela propulsão do inimigo; só uma ativa por vez | AF/opcode 0x36 "armadilha com slot" (AD.md) | bate em parte |

## Medalha
| Fórum | Nosso | Status |
|---|---|---|
| Compatibilidade soma ao ROS e à Evasão/Defesa | compat em ROS e em EE (DM) | **bate** |
| Proficiência da ação soma ao ROS da ação | `ROS += B[0x20+esp]` (DM: Fight_AccumRosPart) | **bate** |
| Nível da medalha soma à Evasão/Defesa e à velocidade | nível (+5) em EE (DM §6.3) | **bate** (velocidade: aberto) |

## Efeitos
| Fórum | Nosso | Status |
|---|---|---|
| Scout sobe o ROS do time pela Poder do scout, **acumula**, teto ~200 | TR6 (emulador): acumula, teto **160** (0xA0) | **bate** (teto exato = 160) |
| Conceal sobe a evasão só de quem usou | TR6: bot+0xB9 do usuário, teto 120, subtrai do ROS do atacante | **bate** |
| Cada bot só tem **um** debuff; aplicar outro apaga o anterior | slot único +0xC9 (SS.md) | **bate** |
| "Flow" = dano periódico igual à metade do golpe inicial | Melt (id 20) tick = +0xCC, não quebra a peça (SS3, emulador) | **bate** (código: Fire/Melt +0xCC = dano/2; BB.md) |
| "Movement" baixa muito o ROS | Bug (13): penalidade de ROS (SS3, DM) | **bate** (Bug = Movement) |
| "Bind" (andar 2× mais lento) e "Stop" (para de andar) | Wave/Hold 17/18, Thunder/Freeze 15/16 (SS.md) | **bate** (Stop = 15/16, Bind = 17/18; BB.md, REF_MEDAPEDIA) |
| Debuff de golpe com dano acaba muito rápido se a defesa de perna ativar | duração = ROS final (fi+0xE8); rec_28/29: defesa zerou o ROS → duração 1 (BB.md) | **bate (emulador)** |
| Destroy (berserk) destrói 1 peça sem errar, mas **sempre erra se o alvo não estiver no turno de recuo**; Sacrifice ignora guarda | DS (emulador): peça do atacante sempre quebra; alvo zera no acerto; ss2 errou | **bate** + regra do recuo **confirmada** (mensagem no jogo, SK.md) |
| Guardas (Defense / Half Block / Full Block) só protegem de golpes com dano; podem falhar com ROS baixo | não documentado | **novo** |
| Medaforce: poder base + 1/8 da proficiência + 1/8 da compatibilidade; ignora esquiva, defesa e guarda | MF.md (medidor, custo); fórmula não documentada | **novo** — conferir no código |

## Para o plano do emulador
Optic/Gravity, tabela de terreno, radiação, perna quebrada, regra do recuo do Destroy, Flow = metade do golpe,
nomes Bind/Stop, guardas, fórmula da Medaforce.
