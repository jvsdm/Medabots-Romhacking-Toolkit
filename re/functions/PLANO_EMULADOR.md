# Plano: subir "quase certeza" para "certeza" no emulador (2026-10-15)

Estado: funções 52% certeza / 48% quase; dados ROM 45% / 26%; RAM 36% / 52%. A leitura estática chegou ao limite:
o que falta é ver o efeito no jogo. Cada item abaixo = 1 gravação (ou 1 savestate do usuário) + 1 replay com watch.

## Como medir (receita que funcionou: BT7, TR6, SS3, DS)
1. Usuário grava com o `battle_record.lua` (L+R começa/para) a partir de um savestate. Não recarregar estado no meio
   (rec_14 ficou irreprodutível).
2. Primeiro passe, sem emulador: ler o `iwram.bin` da gravação (1 cópia a cada 4 quadros) e achar o quadro em que o
   valor muda (armadura bot+0x59+0x18k, status +0xC9..CC, medidor +0xC1, etc.).
3. Segundo passe: `tools/replay_record.py data/records/rec_N --watch ADDR:4@A-B --frames B` (UM watch por run,
   janela obrigatória, LEN 4). O log dá pc/lr do escritor → nome da função no mtbe.
4. Gravar `XX.md` + `XX_simbolos.csv` com comentário "confirmado (emulador rec_N)" (vence qualquer outra camada no
   build_sync). Depois apagar os dumps da gravação (fica só start.ss0 + keys.txt).
5. Alternativa sem gravação: savestate do usuário + `mgba_run_lua` (teclas por quadro, watch, prints). Conferir o
   timing com prints antes: a tela do próximo bot demora ~70-110 quadros; esquerda→A ~20-40 quadros.

## Alvos, em ordem de rendimento
| # | Alvo | O que confirma | Gravação |
|---|---|---|---|
| 1 | Fórmula de dano (MD/DM) | dano previsto × real por golpe; crítico; Leg Defense | rec_6/rec_7 (já existem) |
| 2 | Heal / Repair / Reactivate | quanto repara, teto, peça escolhida | nova: time com Heal (ss3 tem) |
| 3 | XP de medalha | ganho = soma do +6 das peças inimigas; curva (L+1)*100; subir de nível | rec_7 (vitória) |
| 4 | Medaforce | carga do medidor +0xC1 por turno/dano; custo; efeito de 2-3 ids | nova: batalha usando MF |
| 5 | IA do inimigo | driver 0x08012B94 e chamador 0x080322C8 (breakpoint) | rec_7 |
| 6 | Status restantes | Freeze/Hold/Fire/Confusion/Thunder: tick, duração, cura | nova: inimigos com essas técnicas |
| 7 | Recompensa | peça ganha, dinheiro, flag pós-batalha (E+0xC2E) | rec_7 (ganha Sumo Press) |
| 8 | Save | save/load e o bug "flags 0..15 não salvas" (SV2) | nova: salvar, recarregar |
| 9 | Eventos | opcode 0x2F (N da tabela), 0x11 (roupa), 0x7D | nova: cena com escolha |
| 10 | Mapa | velocidade por modo (C5C), estados de cena SceneState_XX (SC4) | nova: andar de patins/skate, trocar de mapa |
| 11 | RAM sem prova (~50) | quem escreve cada variável | watch durante qualquer gravação longa |

## Regras
- ROM original só leitura; savestates do usuário copiados para o scratchpad antes de carregar.
- mGBA sempre mudo, sem janela de Scripting; nada salvo sobre os arquivos do usuário.
- Nada de texto do jogo nos documentos (só ids).

## Do manual oficial (REF_MANUAL.md), a validar
| Alvo | Pergunta |
|---|---|
| MF cheio | Medaforce só pode ser usada com o medidor cheio, mesmo custando menos? (MF4 diz gasto = min(custo, medidor)) |
| Usos da cabeça | recarregam no fim da batalha? onde? |
| Radiação | campo de resfriamento do braço: qual offset, quem decrementa |
| Propulsão | reduz a carga dos braços? (hoje só a vemos no dano) |
| Peça perdida | atacar com peça quebrada antes do turno = "miss" |
| Rubberobo Medal | id do item e a rotina de fuga |
| Troca por link | precisa de 2 emuladores ligados; por enquanto só estático (passo 72 da tela LINK) |
| Protect Select Corps | mini-jogo de botões: onde fica |

## Do fórum e dos FAQs (REF_FORUM.md, REF_FAQ.md), a validar
| Alvo | Pergunta |
|---|---|
| Medidor de MF | FAQ: +40 ao carregar, +20 ao levar dano; código só mostra +20 em ação (+40 com B+0xC0&8). Watch em B+0xC1 numa batalha |
| Destroy | sempre erra se o alvo não está no turno de recuo? (explica o erro no ss2) |
| Flow (Melt) | tick = metade do golpe inicial? comparar +0xCC com o dano do golpe na rec_9 |
| Laser/Beam e Break/Press | dobram Poder / ROS listado? comparar dano previsto × real |
| Terreno × perna | tabela de velocidade por campo e tipo de perna; perna quebrada = atributos/2 |
| Radiação | tempo de recuo do braço; cabeça fixa em 6 |
| Guardas | Defense/Half/Full Block só contra dano; falham com ROS baixo |
| Fórmula da MF | base + prof/8 + compat/8, ignora esquiva/defesa/guarda |
| Tempo de batalha | opções 20/30/40 s; desempate por bots vivos e peças |
