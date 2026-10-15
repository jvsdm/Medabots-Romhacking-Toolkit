# REF_FAQ: FAQs de terceiros (GameFAQs) × o que a engenharia reversa achou (2026-10-15)

Fontes (cópias locais em `D:/claude-brain/Medabots/ref/gamefaqs/`, fora do git):
- **Guia** = "Guide and Walkthrough" (Autocon, v1.3). Seções citadas como `Guia §x.y`.
- **Secrets** = "Secrets and Sidequests FAQ" (Autocon). Seções como `Secrets §x.y`.

Só paráfrase aqui; nenhum texto do FAQ ou do jogo é copiado. Roteiro e diálogos foram ignorados.
Status: **bate** = confirmado no código (decompile + literal) ou no PC; **bate (código)** = leitura estática
de acordo; **bate parcial** = parte confere, parte aberta; **diverge** = conflito a resolver; **aberto** = temos
ponto de partida mas não fechado; **novo** = não temos nada; **não verificável** = não dá para checar só com
o que temos (ou é cosmético/UI).

Referências internas: `MF.md`, `MF4.md`, `MG3.md` (medidor de Medaforça); `MD.md`, `XP.md` (medalhas, nível);
`DM.md` (gPartStats das peças); `DF.md`, `DF2.md`, `BT.md` (fim de batalha); `TA2.md`, `TQ.md`, `TQ2.md` (técnicas);
`SS2.md` (cargas); `AI.md` (CPU); `EV4.md`, `IN3.md`, `VA3.md` (encontros, veículos); `DB2.md`, `DT.md` (Tinpets,
preços); `REF_MANUAL.md` (cruzamento com o manual); `pc/research/README.md` (linhas citadas por número).

## Batalha e Medaforça

| Afirmação (paráfrase) | O que temos (arquivo) | Status |
|---|---|---|
| Medaforça só usa com medidor cheio, 80 pontos (Guia §2.7) | Teto 0x50 = 80 e gate `B+0xC1 == 0x50` em `Fight_TrySelectMedaforceAction` (MF.md §5, AI.md §7, pc/research/README.md:106); custo consumido de `B+0xC2`/`b2` (MF4.md) | bate |
| Cada Medaforça liberada ao nível 10, 30 e 60 (Guia §2.4, §2.7) | Cortes fixos 10/30/60 em `Fight_TrySelectMedaforceAction` 0x080155A8, nível = `bot+5` (MD.md; README:109); CPU idem (AI.md §7) | bate |
| Medidor sobe +40 ao carregar Medaforça (Guia §2.7) | `Fight_AddMedaforceChargeFromAction` 0x0803628C soma +0x14 (20), e +0x28 (40) se `B+0xC0 & 8` (MF.md §6, README:106). Só chamado no resolvedor de ação normal bem-sucedida (`H_01_ChooseActionPath`). Não achamos caminho de "carregar Medaforça" (DOWN) que some medidor | diverge (validar): código só soma ao agir com sucesso; +40 é flag, não ação de carga |
| Medidor sobe +20 ao levar dano (Guia §2.7) | Nenhum caminho de dano que some `B+0xC1` foi achado; o manual também diz que enche ao receber dano (REF_MANUAL item 9) | aberto / não achado (validar) |
| Medaforça não pode ser bloqueada, exceto por status (Guia §2.7) | Os checks são de estado e capítulo (`Fight_IsMedaforceBlockedByChapter`, AI.md:89/99), não de defesa | não verificável (checagem de natureza diferente) |
| Robattle com até 3 Medabots; o primeiro é o líder (Guia §2.6) | Três slots por lado (DF.md, passo 3). Conceito de "líder" não localizado no código | bate parcial (3 slots); líder aberto |
| Cabeça com armadura 0 derrota o Medabot; braço e perna não derrotam (Guia §2.4) | Fila de cabeça quebrada `bot+0x59 == 0` → `Fight_QueueHeadBrokenBotForResolution` (DF.md, passo 3); derrota = `Fight_ResetBotPartState` só pela cabeça (DF2.md; README:322) | bate |
| Cabeça quebrada do líder encerra o Robattle (Guia §2.4, §2.6) | Derrota de bot confirmada; o encerramento por líder não foi lido (lacuna 9 em BT.md; DF.md) | aberto |
| Opções de título: tempo de batalha 40, 30 ou 20 s, mensagens e BGM de batalha (Guia §2.2) | Não achado nas notas | novo |
| Tempo só conta durante CRG/RAD, não durante decisão nem ações (Guia §2.6) | Não achado (timer de batalha em `Fight_SceneStep48_CheckTimeUpOrLevelUp` 0x08031120 existe, mas a duração não foi lida; BT.md:124) | aberto |
| Tempo esgotado: vence quem tem mais Medabots; empate: mais peças (Guia §2.6) | `Fight_EvaluateTeamWinState` 0x0801B53C: conta bots não derrotados (`bot+0xBA == 2` conta derrota), depois peças vazias, depois `fi+0x29`/`fi+0x31`, por último sorteio (DF.md passo 2; BT.md:117) | bate parcial (desempate extra por `fi+0x29/0x31` e sorteio final não no FAQ) |
| Vencer dá uma peça do adversário; perder dá uma sua (Guia §2.6) | Vitória mostra "peça obtida" no PC (README:100); no link, quem perde entrega peça (REF_MANUAL item 15); perda contra CPU não verificada | bate parcial |
| Medaforça/ataque: rotação Auto com 8 ações (H, R, L, MF, MF1 a MF3, aleatória), com repetição e fallback aleatório se a ação não existe (Guia §2.4) | IA da CPU em AI.md; rotação do jogador de 1 a 8 passos não vista (REF_MANUAL item 12) | aberto |
| Medalha: habilidade sobe +1 por uso de peça; 8 pontos sobem de nível; máximo 100 (Guia §2.4) | Existe subida de habilidade com tabela 0x083AEE98 (FI.md:81; pc/research/README.md:110 e HN10). Teto 100 vale para o nível da medalha (XP.md), não para habilidade. Limiar de 8 pontos não lido | bate parcial (existe); limiar e teto aberto |
| Medalha: estatística por nível em faixas H/M/L (10/8/6 por dez níveis, X só na Medalha de Gato) (Secrets §6) | Não temos a tabela. Especialidade final = `T × (nível>>1)`, 0→1, máx 100 (DF.md, linha 89; XP.md) | não verificável (forma diferente, validar no emulador) |
| Kuwagata e Kabuto começam no nível 0 (Secrets §6) | Não verificado | não verificável |
| Peças têm armadura, ROS, poder, usos, carga, radiação, propulsão, evasão, defesa, proximidade e distância (Guia §2.4, Secrets §5) | gPartStats: +4 armadura, +5 ROS, +6 poder, +7 carga, +8 usos, B+0xA2 propulsão, +0xA3 evasão, +0xA4 defesa, +0xA5 proximidade, +0xA6 distância (DM.md, linhas 16-18). Radiação não documentada (REF_MANUAL item 5) | bate parcial (radiação aberta) |
| Proximidade para Strike, Berserk, Defend e Heal; distância para Shoot, Aim Shot, Support e Interrupt (Guia §2.4) | `ROS += distância` se a especialidade é da classe remota, senão proximidade (DM.md:53 e 197, `Speciality_IsRemotenessClass`) | bate |
| Propulsão acelera CRG/RAD e melhora Furious e Trap Clear (Guia §2.4) | Propulsão entra no dano da especialidade 1 como propulsão/2 (DM.md:77). Efeito em CRG/RAD não achado (REF_MANUAL item 6 = diverge/aberto) | diverge / aberto |
| Pernas com alta propulsão e proximidade aumentam o dano de Grapple, até 10 a 40 pontos (Secrets §1.1) | Parte da fórmula (propulsão/2) em DM.md:77; proximidade no Grapple não confirmada | aberto (validar) |
| Chain Reaction: a quebra continua em outras peças (Guia §2.4, Secrets §1.2) | Nenhuma menção achada ("Chain Reaction", "encadeia") | novo |
| Afinidade: peça e medalha de mesma afinidade ganham bônus (Secrets §1.1, Guia §2.4) | gMedals: +1 = atributo casado com a peça, +7 = bônus de compatibilidade (README:109; AI.md:133; `Fight_GetMedalPartCompatibilityBonus` 0x0803576C) | bate (código) |
| Berserk e Aim Shot: o Medabot não esquiva no turno seguinte (Guia §2.7, Secrets §1.2) | Não achado | novo |
| Lista de status: AutoRecover, Bind, Chaos, Counter, Defend, Flow, Full/Half Block, Movement, Stop, Can't Defend, Can't Evade, Useless (Guia §2.5) | Técnicas de status em TA2.md / TQ.md: Bug (13, tipo 0), Virus (14, status 0x10), Thunder (15, tipo 1), Freeze (16, status 0x12), Wave (17, tipo 2). Bug e Virus confirmam o grupo Movement | bate parcial (5 técnicas mapeadas; resto aberto) |
| Bind deixa CRG/RAD 2x mais lento; Time deixa 2x mais rápido (Guia §2.5) | Não lido nesta tarefa | aberto |
| Force Bind sela ou desela todas as Medaforças (Guia §2.7, Secrets §1.2) | Técnica 23 com efeito "nada" na tabela (TQ.md:100; TA2.md:218; ROM 0x083C6A6B). Efeito de selo não localizado | aberto |
| Stability: equipada protege de status; em batalha purifica aliado (Guia §2.7, Secrets §1.1) | Técnica 22, `RequirePendingEffect` (TQ.md:99) | bate parcial (purificação compatível; proteção ao equipar não lida) |
| Cargas Boost, Rapid e Accel Chrg (38 a 40) (Guia §2.7) | Grupo de cargas: `bot+0xBD |= 0x08` (SS2.md:76) | bate (grupo) |
| Team Form (3 torres), TeamAttack, Strengthen (Guia §2.7) | IDs 51, 52 e 53 (pc/research/README.md:80) | bate (ids); efeitos aberto |
| Confusion, Counter, Full Block, Half Block, Shot Trap, Grap Trap (Guia §2.7) | Técnicas de defesa e status listadas em TA2.md; efeitos não lidos | aberto |

## Mundo, encontros, veículos e menus

| Afirmação (paráfrase) | O que temos (arquivo) | Status |
|---|---|---|
| Quatro veículos: Mini S. Wheel (Landmotor), Silver Locket, Wings of Wind (Femjet) e Periscope (Aquamar) (Guia §2.8) | Nomes em REF_MANUAL item 14. Modo de andar `E+0xC5C` trocado por 7 itens (VA3, IN3). Diferença: 7 ids no código contra 4 veículos no FAQ | bate (nomes); diverge na contagem (validar) |
| Landmotor não tem Robattle aleatório (Guia §2.8, Secrets §1.4) | Encontro só com tipo de tile `E+0xC5C != 1` (EV4.md:12). Veículo muda `E+0xC5C` (IN3.md). Ligação veículo → gate não confirmada | não verificável (há gancho) |
| Encontro aleatório: passos acima de 16 e sorteio `randbyte & 0x1F == 0x1F`, ou acima de 31 (não citado no FAQ) | `0x080438C4` (pc/research/README.md:103) | bate (código); referência do FAQ não cobre |
| Silver Locket só depois de zerar o jogo (Guia §2.8) | Não achado | novo |
| Veículos só utilizáveis quando o Medabot está montado e a área permite (Guia §2.8) | Não achado | novo |
| Menu de campo abre com B; START não abre nada no mapa livre (Guia §2.1) | pc/research/README.md:167 | bate |
| Menu tem Medawatch, Item, Veículo e Salvar (Guia §2.8) | pc/research/README.md:167 (MEDAMATCH, ITEM, VEÍCULO, SALVAR) | bate |
| SELECT reordena Medabots, Medalhas e itens (Guia §2.3) | SELECT duas vezes troca Tinpets no time (pc/research/README.md:306). Reordenar medalhas no PC não implementado (README:191) | bate parcial |
| Salvar pede confirmação dupla, com "não" como padrão (Guia §2.9) | Não achado (UI) | não verificável |
| Tinpets: 10 existem e 9 podem ser coletados (Secrets §8) | Time com 9 registros de 8 bytes em `0x030017A0` (DB2.md:42; pc/research/README.md:223) | bate (9 slots); 10 existentes não verificável |
| Peças de loja: preço é por peça, não por loja (Secrets §7) | DT.md:144 (preço em gPartStats +0x0B) | bate (princípio); valores não verificados (`parts.lua` não tem nome direto) |
| Lojas: Hopmart, Strawberry Heart (M.I. e M.), Vendor, Medamall, Medashop, Convenience Store, Bracha (Secrets §7) | Lojas por id: `G.md:88` (`gShopItems`), DT.md | não verificável (nomes não mapeados) |
| Peças de veículo vendidas no Lab, após o primeiro veículo (Guia §2.8) | Não achado | novo |
| Rubberobo Medal: item que tira da batalha quando se recusa (Guia §2.6) | REF_MANUAL item 13 (aberto, sem id). PLANO_EMULADOR.md:46 | aberto |
| Combo de reset: A+B+SELECT e START (Secrets §1.4) | Reset por START em `0x08000964` (`SoftReset_CheckComboAndRun`, pc/research/README.md:63 e 88) | bate parcial (combo exato a validar) |
| Test Mode: SELECT+START na tela-título, com 51 músicas e 117 efeitos (Secrets §1.3) | Não achado | novo (cosmético) |
| Tempo para em 99:59 (Secrets §1.3) | Não achado | novo |
| Medalhas secretas: Gato (Erika) e ? (Karin), e as seis de Nae (Secrets §4.1 a §4.3) | Não achado (são quests de história; ids não localizados) | novo |
| Medalink: quatro rodadas de lutas em Medashop; prêmio final são peças Zorin ou Bayonet (Secrets §4.4) | Não achado | novo |
| Batalhas de história (Medalink e Robattles de roteiro) são fixas, não sorteadas (Secrets §4.4) | Batalha de roteiro chama `Start_Battle(id)` direto; sorteio só em encontro de mapa (pc/research/README.md:39 e 104) | bate (código); Medalink não verificada |

## Para validar no emulador

1. Medidor de Medaforça: qual ação enche +20 e qual enche +40; se levar dano enche; se "carregar Medaforça" (DOWN) soma algo (MF.md §6).
2. Tempo de Robattle: opções de 20, 30 e 40 s; se o relógio só anda durante CRG/RAD.
3. Fim de Robattle: cabeça quebrada do líder encerra a luta; cabeça de não líder não encerra.
4. Desempate por tempo: comparar com a regra do FAQ (mais Medabots, depois mais peças).
5. Bind e Time: CRG/RAD 2x mais lento e mais rápido; Flow causa dano em CRG/RAD.
6. Landmotor: zero encontros aleatórios; quais dos 7 ids em IN3/VA3 são veículos.
7. Propulsão: afeta CRG/RAD? (REF_MANUAL item 6).
8. Radiação (RAD): valor e efeito no ciclo da peça.
9. Force Bind: sela e desela Medaforças.
10. Chain Reaction: flag de quebra em cadeia.
11. Habilidade de medalha: 8 pontos por nível, teto 100.
12. Rotação Auto: 8 ações, repetição e fallback aleatório.
13. Rubberobo Medal: qual item foge da batalha e se START recusa a luta.
14. Combo de reset exato e Test Mode.
15. Tinpets: quantos são coletáveis (9?).

## Ids úteis citados (só nomes e ids; sem texto do FAQ)

- Técnicas (ids em TA2/TQ, pc/research/README.md:80): 13 Bug, 14 Virus, 15 Thunder, 16 Freeze, 17 Wave, 22 Stability, 23 Force Bind, 36 Scout, 37 Conceal, 38 a 40 Boost/Rapid/Accel Chrg, 51 Team Form, 52 TeamAttack, 53 Strengthen.
- Veículos: Mini S. Wheel (Landmotor), Silver Locket, Wings of Wind (Femjet), Periscope (Aquamar). Ids de item não confirmados; candidatos: 1, 5, 12, 13, 19, 39, 40 (IN3.md:26, ligados a `E+0xC5C`).
- Item que foge da batalha: Rubberobo Medal. Id não localizado.
- Medalhas secretas: Medalha de Gato, Medalha de ?, e as seis de Nae (nomes de espécie: Bat, Kappa, Mouse, Chameleon, Rabbit, Monkey). Ids não localizados.
- Prêmio da Medalink: peças Zorin (Rokusho) e Bayonet (Metabee). Ids não localizados.
- Medabot Metabee: bot 43 (pc/research/README.md:219).
- Medaforça: ids 0x00 a 0x33 (MF3.md).
- Shops (nomes): Hopmart, Strawberry Heart, Medamall, Medashop, Convenience Store, Bracha. Ids em `G.md` (`gShopItems`).

## Observações

- Nada foi testado no emulador; esta tabela é só cruzamento estático com as notas e o FAQ.
- Não houve alteração em outras notas nem em `pc/research/README.md`; nenhum achado novo confirmado foi adicionado.
