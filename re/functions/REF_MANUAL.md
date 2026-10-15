# REF_MANUAL: manual oficial (EN, Nintendo EU) × o que a engenharia reversa achou (2026-10-15)

Fonte: manual do jogo (PDF da Nintendo EU, cópia local em `D:/claude-brain/Medabots/ref/`, fora do git). Só
paráfrase aqui; o texto do manual não é copiado. Status: **bate** = já confirmado no código/emulador;
**bate (código)** = de acordo com a leitura estática; **diverge** = conflito a resolver; **aberto** = não visto ainda.

| # | O manual diz (paráfrase) | O que temos | Status |
|---|---|---|---|
| 1 | 4 tipos de peça (cabeça, braço esq., braço dir., pernas); Tinpet + Medalha + peças | bloco do bot: peças k=0..3 em B+0x48+0x18k (DM.md) | bate (código) |
| 2 | Peça + medalha da mesma especialidade aumenta o ROS | ROS soma bônus de medalha (R1b +0xEE, DM.md) | bate (código) |
| 3 | Usar a especialidade sobe o nível de habilidade da medalha | habilidades por especialidade +0x20..+0x27 (XP.md) | bate (código) → validar no emulador |
| 4 | Cabeça tem "quantidade de usos", que recarrega no fim de cada batalha | campo +8 usos (DM.md) | uso bate; **recarga ao fim: aberto** |
| 5 | Braço tem Carga (tempo antes de usar) e Radiação (resfriamento depois) | +7 carga (DM.md); **radiação não documentada** | aberto |
| 6 | Propulsão da perna reduz o tempo de carga dos braços | nosso DM usa propulsão/2 no dano da especialidade 1 | **diverge/aberto** (pode ser os dois) |
| 7 | Evasão = chance de desviar; Defesa = reduz dano na armadura | B+0xA3 evasão, B+0xA4 defesa (DM.md) | bate (código) |
| 8 | Proximidade vale para Strike/Berserk/Defend/Heal; Distância para Shoot/Aim Shoot/Support/Interrupt | ROS += distância se a especialidade é de classe "remota", senão proximidade (DM.md, Speciality_IsRemotenessClass) | **bate** |
| 9 | Medaforce enche ao receber dano e com o comando Charge MF | medidor B+0xC1 (MF.md) | bate (código) → validar quanto enche |
| 10 | Medaforce exige o medidor **cheio**, qualquer que seja o custo | MF4: custo b2, gasto min(b2, C1) | **diverge/aberto** — validar no emulador |
| 11 | Se o bot perder a peça antes de agir, o ataque vira "miss" | não documentado | aberto |
| 12 | Rotação: programa de 1 a 8 passos; Auto executa; dá para jogar só o líder | Auto/IA (AI.md); 8 passos não visto | aberto |
| 13 | Rubberobo Medal (item) tira da batalha | itens da bolsa (IN3); fuga não ligada a id | aberto |
| 14 | Veículos (Mini S.Wheel, Silver Locket, Wings of Wind, Periscope) para terra/ar/água | modo de andar E+0xC5C trocado por 7 itens (VA3, IN3) | bate (código): ligar cada veículo ao id |
| 15 | **Troca de peças pelo cabo link** com a outra versão (Rokusho), na mulher no fundo de uma loja; **batalha por link: quem perde entrega uma peça** | README dizia "link sem troca"; LK4 achou passo 72 da tela LINK que soma item recebido | **diverge: corrigir** — há troca e há perda de peça |
| 16 | Mini-jogo "Protect Select Corps" (seguir sequência de botões) | não documentado | aberto |
| 17 | Mais de 20 medalhas; tela de medalhas mostra nível e atributos | roster 30 × 0x40 (IV.md) | bate (código) |
| 18 | Loja vende peças; tela de compra com quantidade | gShopQuantities 0x02023000, Shop_TickBuyQuantityMenu (PD4) | bate (código) |
| 19 | Pernas: tipo de terreno; mais rápido = mais chances de atacar | tipo de perna 100..106 (DM.md); velocidade por terreno não visto | aberto |

## Ações
- Corrigir no `pc/research/README.md` a nota "link sem troca" (item 15).
- Itens 4, 5, 6, 10, 11, 13, 16, 19 entram no `PLANO_EMULADOR.md`.
